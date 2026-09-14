using System.Diagnostics;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using CuteCat.Core;

namespace CuteCat.App;

public sealed record DesktopApp(string Path,string Name);
public sealed record AppCloseCandidate(NotificationTarget Target,AppRule Rule,bool CanClose=true,bool Handled=false,string CaptionSource="None");
public enum AppCloseResult { Closed, Requested, Cancelled }

/// <summary>Only a user-selected executable's foreground, unowned top-level window.</summary>
public sealed class DesktopApps
{
    private readonly Func<IntPtr> _foreground;
    public DesktopApps(Func<IntPtr>? foreground=null)=>_foreground=foreground??Native.GetForegroundWindow;
    private readonly ConcurrentDictionary<string,NotificationTarget> _handled=new();
    public string LastScan { get; private set; }="NoMatchingApp";
    public void Reset()=>_handled.Clear();
    public static double IdleSeconds()
    {
        var info=new LastInput{Size=(uint)Marshal.SizeOf<LastInput>()};
        return GetLastInputInfo(ref info)?unchecked((uint)Environment.TickCount64-info.Time)/1000d:0;
    }
    public static bool Selectable(string path)
    {
        if(!System.IO.Path.IsPathFullyQualified(path)||!path.EndsWith(".exe",StringComparison.OrdinalIgnoreCase))return false;
        string windows=Environment.GetFolderPath(Environment.SpecialFolder.Windows).TrimEnd('\\')+"\\";
        string own=AppContext.BaseDirectory.TrimEnd('\\')+"\\";
        return !path.StartsWith(windows,StringComparison.OrdinalIgnoreCase)&&!path.StartsWith(own,StringComparison.OrdinalIgnoreCase)&&
            System.IO.Path.GetFileName(path) is not ("CuteCat.exe" or "CuteCat.SetupSupport.exe" or "ApplicationFrameHost.exe");
    }
    public static List<DesktopApp> OpenApps()
    {
        var apps=new Dictionary<string,DesktopApp>(StringComparer.OrdinalIgnoreCase);
        Native.EnumWindows((window,_)=>
        {
            try
            {
                if(!Eligible(window))return true;
                Native.GetWindowThreadProcessId(window,out uint pid);
                string? path=Executable((int)pid);
                if(path is not null&&Selectable(path))
                {
                    string name=FileVersionInfo.GetVersionInfo(path).FileDescription??System.IO.Path.GetFileNameWithoutExtension(path);
                    apps[path]=new(path,name.Length>80?name[..80]:name);
                }
            }
            catch(Exception e)when(PlatformFailure(e)){}
            return true;
        },IntPtr.Zero);
        return apps.Values.OrderBy(a=>a.Name).ToList();
    }
    public AppCloseCandidate? Find(Preferences settings,bool focusing,double now,double pausedUntil,int epoch)
    {
        {
            LastScan="NoMatchingApp";
            foreach(var pair in _handled.ToArray())
                if(!Native.IsWindow(new IntPtr(pair.Value.Window))||!Native.IsWindowVisible(new IntPtr(pair.Value.Window))||IsIconic(new IntPtr(pair.Value.Window)))_handled.TryRemove(pair.Key,out _);
            IntPtr window=_foreground();
            if(window==IntPtr.Zero){LastScan="NoForegroundWindow";return null;}
            try
            {
                if(!Eligible(window))return null;
                Native.GetWindowThreadProcessId(window,out uint pid);
                string? path=Executable((int)pid);if(path is null||!Selectable(path))return null;
                var rule=AppRulePolicy.Match(settings.AppRules,path,settings.AppGuard,focusing,now,pausedUntil);
                if(rule is null)return null;
                using var process=Process.GetProcessById((int)pid);
                string identity="app:"+window.ToInt64()+":"+pid+":"+process.StartTime.ToFileTimeUtc();
                bool handled=_handled.ContainsKey(identity);
                if(rule.Action==AppRuleAction.Remind)
                    return new(new(identity,window.ToInt64(),(int)pid,new Area(0,0,1,1),now,epoch,AppWindow:true),rule,Handled:handled);
                var close=CloseControl(window,(int)pid);
                if(close is null){LastScan="NoCaption";return new(new(identity,window.ToInt64(),(int)pid,new Area(0,0,1,1),now,epoch,AppWindow:true),rule,false,handled);}
                LastScan="SupportedApp";
                return new(new(identity,window.ToInt64(),(int)pid,close.Value.Area,now,epoch,AppWindow:true),rule,true,handled,close.Value.Source);
            }
            catch(Exception e)when(PlatformFailure(e)){return null;}
        }
    }
    public void Acknowledge(NotificationTarget target){if(_handled.Count>512)_handled.Clear();_handled[target.Identity]=target;}
    public AppCloseResult Close(NotificationTarget target,Preferences settings,bool focusing,Func<bool> valid,Func<double> now)
    {
        {
            try
            {
                if(!valid())return AppCloseResult.Cancelled;
                var current=Find(settings,focusing,now(),0,target.Epoch);
                if(current is null||current.Handled||!current.CanClose||!NotificationAttempt.Matches(target,current.Target,now())||current.Rule.Action!=AppRuleAction.CloseWindow)return AppCloseResult.Cancelled;
                IntPtr window=new(target.Window);
                var close=CloseControl(window,target.Process);
                if(close is null||!NotificationAttempt.SameGeometry(close.Value.Area,target.Button,2)||!valid()||_foreground()!=window)return AppCloseResult.Cancelled;
                Native.GetWindowThreadProcessId(window,out uint currentPid);if(currentPid!=target.Process)return AppCloseResult.Cancelled;
                // Mark before requesting close: never repeatedly press a save prompt or a
                // window that vetoed closing. Re-enable/edit the rule to explicitly retry.
                Acknowledge(target);
                if(close.Value.Control is { } control)
                {
                    if(!control.TryGetCurrentPattern(InvokePattern.Pattern,out var pattern)||!valid())return AppCloseResult.Cancelled;
                    ((InvokePattern)pattern).Invoke();
                }
                else if(!PostMessage(window,0x112,new IntPtr(0xF060),IntPtr.Zero))return AppCloseResult.Cancelled;
                for(int i=0;i<24;i++)
                {
                    if(!Native.IsWindow(window)||!Native.IsWindowVisible(window))return AppCloseResult.Closed;
                    Thread.Sleep(25);
                }
                return AppCloseResult.Requested; // Save prompts / app vetoes are left to the user.
            }
            catch(Exception e)when(PlatformFailure(e)){return AppCloseResult.Cancelled;}
        }
    }
    private static bool Eligible(IntPtr window)
    {
        if(window==IntPtr.Zero||!Native.IsWindowVisible(window)||!IsWindowEnabled(window)||GetWindow(window,4)!=IntPtr.Zero)return false;
        Native.GetWindowThreadProcessId(window,out uint pid);
        if(pid==Environment.ProcessId)return false;
        var cls=new StringBuilder(128);Native.GetClassName(window,cls,128);
        if(cls.ToString() is "Progman" or "WorkerW" or "Shell_TrayWnd")return false;
        if(cls.ToString()=="#32770")
        {
            // Some desktop utilities use a dialog as their main window. Ordinary message/save
            // dialogs have no minimize box; owned dialogs were already rejected above.
            if((GetWindowLongPtr(window,-16).ToInt64()&0x20000)==0)return false;
            bool otherMain=false;
            Native.EnumWindows((other,_)=>
            {
                Native.GetWindowThreadProcessId(other,out uint otherPid);
                if(other!=window&&otherPid==pid&&Native.IsWindowVisible(other)&&GetWindow(other,4)==IntPtr.Zero&&
                    (GetWindowLongPtr(other,-20).ToInt64()&0x80)==0)otherMain=true;
                return !otherMain;
            },IntPtr.Zero);
            if(otherMain)return false;
        }
        return true;
    }
    private static (Area Area,AutomationElement? Control,string Source)? CloseControl(IntPtr window,int pid)
    {
        if(!Native.GetWindowRect(window,out var bounds))return null;
        double dpi=Math.Max(96,Native.GetDpiForWindow(window))/96d;
        var title=new TitleBar{Size=(uint)Marshal.SizeOf<TitleBar>(),States=new uint[6],Rects=new Native.Rect[6]};
        if(SendMessageTimeout(window,0x33F,IntPtr.Zero,ref title,2,80,out _)!=IntPtr.Zero)
        {
            var r=title.Rects[5];var area=new Area(r.Left,r.Top,r.Right-r.Left,r.Bottom-r.Top);
            if((title.States[5]&(1|0x8000|0x10000))==0&&CaptionGeometry(bounds,area,dpi))return(area,null,"TitleBarInfo");
        }
        // JetBrains/JBR windows can expose empty TITLEBARINFOEX rectangles and no UIA caption,
        // while correctly identifying their native close region through WM_NCHITTEST.
        if(NativeCloseRegion(window,bounds,dpi) is { } native)return(native,null,"NativeHitTest");
        // Fallback is constrained to the window's caption corner, not content buttons.
        var root=AutomationElement.FromHandle(window);
        var buttons=root.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Button));
        foreach(AutomationElement button in buttons)
        {
            var b=button.Current;if(b.ProcessId!=pid||b.IsOffscreen||!b.IsEnabled)continue;
            var r=b.BoundingRectangle;var area=new Area(r.X,r.Y,r.Width,r.Height);
            if(!CaptionGeometry(bounds,area,dpi))continue;
            bool caption=false;AutomationElement? ancestor=button;
            for(int i=0;i<5&&ancestor is not null;i++)
            {
                var a=ancestor.Current;
                if(a.ControlType==ControlType.TitleBar||a.AutomationId is "TitleBar" or "Caption"){caption=true;break;}
                if(Automation.Compare(ancestor,root))break;
                ancestor=TreeWalker.RawViewWalker.GetParent(ancestor);
            }
            if(!caption)continue;
            if(b.AutomationId is not ("Close" or "CloseButton" or "PART_CloseButton")&&b.Name!="Close")continue;
            if(button.TryGetCurrentPattern(InvokePattern.Pattern,out _))return(area,button,"Accessibility");
        }
        return null;
    }
    private static Area? NativeCloseRegion(IntPtr window,Native.Rect bounds,double dpi)
    {
        IntPtr menu=GetSystemMenu(window,false);
        uint state=menu==IntPtr.Zero?uint.MaxValue:GetMenuState(menu,0xF060,0);
        if(state==uint.MaxValue||(state&3)!=0)return null;
        Area? hint=null;
        if(DwmGetWindowAttribute(window,5,out var caption,Marshal.SizeOf<Native.Rect>())==0)
            hint=new(bounds.Left+caption.Left,bounds.Top+caption.Top,caption.Right-caption.Left,caption.Bottom-caption.Top);
        var watch=Stopwatch.StartNew();
        int? Query(V2 point)
        {
            if(watch.ElapsedMilliseconds>120)return null;
            long packed=((long)(ushort)(short)point.Y<<16)|(ushort)(short)point.X;
            return SendHitTest(window,0x84,IntPtr.Zero,new IntPtr(packed),2|0x20,20,out var result)!=IntPtr.Zero?(int)result.ToInt64():null;
        }
        var region=NativeCaptionLocator.Find(new(bounds.Left,bounds.Top,bounds.Right-bounds.Left,bounds.Bottom-bounds.Top),dpi,hint,Query);
        if(region is null||!CaptionGeometry(bounds,region.Value,dpi)||!Native.GetWindowRect(window,out var current)||
            current.Left!=bounds.Left||current.Top!=bounds.Top||current.Right!=bounds.Right||current.Bottom!=bounds.Bottom)return null;
        return region;
    }
    private static bool CaptionGeometry(Native.Rect window,Area close,double dpi)=>close.IsValid&&close.Width>=10&&close.Height>=10&&close.Width<=80*dpi&&close.Height<=64*dpi&&
        close.Left>=window.Right-110*dpi&&close.Right<=window.Right+2&&close.Top>=window.Top-2&&close.Bottom<=window.Top+70*dpi;
    private static string? Executable(int pid)
    {
        IntPtr handle=Native.OpenProcess(0x1000,false,pid);if(handle==IntPtr.Zero)return null;
        try{var text=new StringBuilder(32768);int length=text.Capacity;return Native.QueryFullProcessImageName(handle,0,text,ref length)?System.IO.Path.GetFullPath(text.ToString()):null;}
        finally{Native.CloseHandle(handle);}
    }
    private static bool PlatformFailure(Exception e)=>e is InvalidOperationException or System.ComponentModel.Win32Exception or UnauthorizedAccessException or ArgumentException or COMException or ElementNotAvailableException or IOException;
    [StructLayout(LayoutKind.Sequential)]private struct LastInput{public uint Size,Time;}
    [StructLayout(LayoutKind.Sequential)]private struct TitleBar
    {public uint Size;public Native.Rect Rect;[MarshalAs(UnmanagedType.ByValArray,SizeConst=6)]public uint[] States;[MarshalAs(UnmanagedType.ByValArray,SizeConst=6)]public Native.Rect[] Rects;}
    [DllImport("user32.dll")]private static extern bool GetLastInputInfo(ref LastInput info);
    [DllImport("user32.dll")]private static extern IntPtr GetWindow(IntPtr window,uint command);
    [DllImport("user32.dll")]private static extern bool IsWindowEnabled(IntPtr window);
    [DllImport("user32.dll")]private static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")]private static extern IntPtr GetWindowLongPtr(IntPtr window,int index);
    [DllImport("user32.dll",EntryPoint="SendMessageTimeoutW")]private static extern IntPtr SendMessageTimeout(IntPtr window,uint message,IntPtr wparam,ref TitleBar data,uint flags,uint timeout,out IntPtr result);
    [DllImport("user32.dll")]private static extern bool PostMessage(IntPtr window,uint message,IntPtr wparam,IntPtr lparam);
    [DllImport("user32.dll")]private static extern IntPtr GetSystemMenu(IntPtr window,bool revert);
    [DllImport("user32.dll")]private static extern uint GetMenuState(IntPtr menu,uint item,uint flags);
    [DllImport("user32.dll",EntryPoint="SendMessageTimeoutW")]private static extern IntPtr SendHitTest(IntPtr window,uint message,IntPtr wp,IntPtr lp,uint flags,uint timeout,out IntPtr result);
    [DllImport("dwmapi.dll")]private static extern int DwmGetWindowAttribute(IntPtr window,int attribute,out Native.Rect rect,int size);
}
