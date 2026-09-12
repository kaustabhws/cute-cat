using System.Diagnostics;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using CuteCat.Core;

namespace CuteCat.App;

public sealed record DesktopApp(string Path,string Name);
public sealed record AppCloseCandidate(NotificationTarget Target,AppRule Rule);
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
            try
            {
                if(!Eligible(window))return null;
                Native.GetWindowThreadProcessId(window,out uint pid);
                string? path=Executable((int)pid);if(path is null||!Selectable(path))return null;
                var rule=AppRulePolicy.Match(settings.AppRules,path,settings.AppGuard,focusing,now,pausedUntil);
                if(rule is null)return null;
                using var process=Process.GetProcessById((int)pid);
                string identity="app:"+window.ToInt64()+":"+pid+":"+process.StartTime.ToFileTimeUtc();
                if(_handled.ContainsKey(identity))return null;
                if(rule.Action==AppRuleAction.Remind)
                    return new(new(identity,window.ToInt64(),(int)pid,new Area(0,0,1,1),now,epoch,AppWindow:true),rule);
                var close=CloseControl(window,(int)pid);if(close is null){LastScan="NoCaption";return null;}
                LastScan="SupportedApp";
                return new(new(identity,window.ToInt64(),(int)pid,close.Value.Area,now,epoch,AppWindow:true),rule);
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
                if(current is null||!NotificationAttempt.Matches(target,current.Target,now())||current.Rule.Action!=AppRuleAction.CloseWindow)return AppCloseResult.Cancelled;
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
        return cls.ToString() is not ("#32770" or "Progman" or "WorkerW" or "Shell_TrayWnd");
    }
    private static (Area Area,AutomationElement? Control)? CloseControl(IntPtr window,int pid)
    {
        if(!Native.GetWindowRect(window,out var bounds))return null;
        double dpi=Math.Max(96,Native.GetDpiForWindow(window))/96d;
        var title=new TitleBar{Size=(uint)Marshal.SizeOf<TitleBar>(),States=new uint[6],Rects=new Native.Rect[6]};
        if(SendMessageTimeout(window,0x33F,IntPtr.Zero,ref title,2,80,out _)!=IntPtr.Zero)
        {
            var r=title.Rects[5];var area=new Area(r.Left,r.Top,r.Right-r.Left,r.Bottom-r.Top);
            if((title.States[5]&(1|0x8000|0x10000))==0&&CaptionGeometry(bounds,area,dpi))return(area,null);
        }
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
            if(button.TryGetCurrentPattern(InvokePattern.Pattern,out _))return(area,button);
        }
        return null;
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
    [DllImport("user32.dll",EntryPoint="SendMessageTimeoutW")]private static extern IntPtr SendMessageTimeout(IntPtr window,uint message,IntPtr wparam,ref TitleBar data,uint flags,uint timeout,out IntPtr result);
    [DllImport("user32.dll")]private static extern bool PostMessage(IntPtr window,uint message,IntPtr wparam,IntPtr lparam);
}
