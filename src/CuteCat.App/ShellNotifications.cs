using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using CuteCat.Core;
using Condition=System.Windows.Automation.Condition;

namespace CuteCat.App;

/// <summary>Only visible toast close controls owned by verified Windows shell binaries.</summary>
public sealed class ShellNotifications
{
    private int _epoch;
    private readonly object _automationLock=new();
    private readonly Dictionary<int,long> _hosts=[];
    private double _hostRefresh;
    public string LastScan { get; private set; }="NoBanner";
    public double LastScanMilliseconds { get; private set; }
    public int LastRootCount { get; private set; }
    public int Epoch=>Volatile.Read(ref _epoch);
    public void Cancel()=>Interlocked.Increment(ref _epoch);

    public NotificationTarget? Find(double now,int epoch)
    {
        lock(_automationLock)
        {
            var timing=Stopwatch.StartNew();LastScan="NoShellWindow";LastRootCount=0;
            try
            {
                RefreshHosts(now);
                foreach(var root in FindRoots())
                {
                    LastRootCount++;
                    var found=ReadToast(root,now,epoch);
                    if(found is not null){LastScan="SupportedBanner";return found.Value.target;}
                }
            }
            catch(Exception e)when(IsPlatformFailure(e)){LastScan="Unavailable";}
            finally{LastScanMilliseconds=timing.Elapsed.TotalMilliseconds;}
            return null;
        }
    }

    public bool Invoke(NotificationTarget target,int epoch,Func<double> now)
    {
        lock(_automationLock)
        {
            if(epoch!=Epoch||target.Epoch!=epoch)return false;
            try
            {
                IntPtr hwnd=new(target.Window);
                if(!TryTrustedWindow(hwnd,out _,out _))return false;
                var root=AutomationElement.FromHandle(hwnd);
                var found=ReadToast(root,now(),epoch);
                if(found is null || !NotificationAttempt.Matches(target,found.Value.target,now()) || epoch!=Epoch)return false;
                var close=found.Value.close;
                if(!close.TryGetCurrentPattern(InvokePattern.Pattern,out object pattern)||epoch!=Epoch)return false;
                ((InvokePattern)pattern).Invoke();
                // Invocation acceptance alone is not dismissal evidence. Confirm disappearance.
                for(int i=0;i<12;i++)
                {
                    Thread.Sleep(25);
                    try
                    {
                        if(!Native.IsWindowVisible(hwnd)||close.Current.IsOffscreen)return true;
                        var current=ReadToast(root,now(),epoch);
                        if(current is null || !NotificationStabilizer.SameIdentity(target,current.Value.target))return true;
                    }
                    catch(ElementNotAvailableException){return true;}
                }
            }
            catch(Exception e)when(IsPlatformFailure(e)){}
            return false;
        }
    }

    private (NotificationTarget target,AutomationElement close)? ReadToast(AutomationElement root,double now,int epoch)
    {
        var info=root.Current;
        IntPtr hwnd=new(info.NativeWindowHandle);
        if(info.IsOffscreen || !TryTrustedWindow(hwnd,out int pid,out long start))return null;
        double dpi=Math.Max(96,Native.GetDpiForWindow(hwnd))/96d;
        var resolved=ResolveCloseControl(root,pid,dpi,status=>LastScan=status);
        if(resolved is null)return null;
        var (toast,close,area)=resolved.Value;
        string identity=start+":"+string.Join('.',root.GetRuntimeId())+":"+string.Join('.',toast.GetRuntimeId())+":"+string.Join('.',close.GetRuntimeId());
        return (new(identity,hwnd.ToInt64(),pid,area,now,epoch),close);
    }

    // Structural resolver is shared with a test-owned UIA fixture. Invocation still requires a trusted shell process.
    internal static (AutomationElement toast,AutomationElement close,Area bounds)? ResolveCloseControl(AutomationElement root,int pid,double dpi,Action<string>? trace=null)
    {
        trace?.Invoke("NoToastView");
        var toastFilter=new OrCondition(new PropertyCondition(AutomationElement.AutomationIdProperty,"ToastView"),
            new PropertyCondition(AutomationElement.ClassNameProperty,"ToastView"),
            new PropertyCondition(AutomationElement.AutomationIdProperty,"NormalToastView"),
            new PropertyCondition(AutomationElement.ClassNameProperty,"FlexibleToastView"));
        var toasts=root.FindAll(TreeScope.Element|TreeScope.Descendants,toastFilter);
        foreach(AutomationElement toast in toasts)
        {
            if(toast.Current.IsOffscreen){trace?.Invoke("ToastOffscreen");continue;}
            if(InNotificationCenter(toast,root)){trace?.Invoke("NotificationCenterExcluded");continue;}
            var r=toast.Current.BoundingRectangle;
            var toastArea=new Area(r.X,r.Y,r.Width,r.Height);
            if(!toastArea.IsValid||toastArea.Width<100||toastArea.Width>700*dpi||toastArea.Height>550*dpi){trace?.Invoke("ToastGeometryRejected");continue;}
            trace?.Invoke("CloseControlUnavailable");
            var buttons=toast.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Button));
            foreach(AutomationElement close in buttons)
            {
                var b=close.Current;
                if(b.IsOffscreen||!b.IsEnabled||b.ProcessId!=pid||!close.TryGetCurrentPattern(InvokePattern.Pattern,out _))continue;
                var rect=b.BoundingRectangle;var area=new Area(rect.X,rect.Y,rect.Width,rect.Height);
                bool knownId=b.AutomationId is "DismissButton" or "CloseButton" or "ToastDismissButton" or "DismissNotificationButton";
                // Windows 11 FlexibleToastView may put the header BELOW a hero image.
                // A known dismiss control is still small, contained and right-aligned;
                // requiring it in the first 80 DIPs rejects Snipping Tool's real toast.
                bool flexible=toast.Current.AutomationId=="NormalToastView" || toast.Current.ClassName=="FlexibleToastView";
                if(!ValidCloseGeometry(toastArea,area,dpi,flexible&&knownId))continue;
                if(!knownId)
                {
                    // This is the Windows close icon's accessibility label, never a message label.
                    if(area.Center.X<toastArea.Right-48*dpi || b.Name is not ("Dismiss notification" or "Dismiss this notification" or "Close notification" or "Close" or "Dismiss"))continue;
                }
                return (toast,close,area);
            }
        }
        return null;
    }

    public static bool ValidCloseGeometry(Area toast,Area close,double dpi,bool flexibleHeader=false)=>toast.IsValid&&close.IsValid&&
        close.Width>=8&&close.Height>=8&&close.Width<=96*dpi&&close.Height<=96*dpi&&
        close.Center.X>=toast.Left+toast.Width*.6&&close.Center.X<=toast.Right+3&&
        (flexibleHeader ? close.Left>=toast.Left&&close.Right<=toast.Right+3&&close.Top>=toast.Top-3&&close.Bottom<=toast.Bottom+3&&close.Center.X>=toast.Right-64*dpi :
            close.Center.Y>=toast.Top-3&&close.Center.Y<=toast.Top+Math.Min(toast.Height*.5,80*dpi));

    private static bool InNotificationCenter(AutomationElement toast,AutomationElement root)
    {
        AutomationElement? element=toast;
        for(int i=0;i<12&&element is not null;i++)
        {
            var info=element.Current;
            // Shared toast classes can include ActionCenter in their namespace and a live
            // banner can have a ScrollViewer. Neither implies notification history.
            if(info.AutomationId is "NotificationCenter" or "NotificationCenterGrid" or "ActionCenter" or "NotificationCenterList" or "ActionCenterList" ||
                info.ClassName is "NotificationCenter" or "ActionCenter")return true;
            if(Automation.Compare(element,root))break;
            element=TreeWalker.RawViewWalker.GetParent(element);
        }
        return false;
    }

    private void RefreshHosts(double now)
    {
        if(now<_hostRefresh)return;
        _hostRefresh=now+.5;_hosts.Clear();
        foreach(string name in new[]{"ShellExperienceHost","ShellHost"})
            foreach(var process in Process.GetProcessesByName(name))
            {
                using(process)if(TryTrustedProcess(process.Id,out long started))_hosts[process.Id]=started;
            }
    }

    private List<AutomationElement> FindRoots()
    {
        var roots=new List<AutomationElement>();var seen=new HashSet<long>();
        foreach(string className in new[]{"Windows.UI.Core.CoreWindow","Xaml_WindowedPopupClass","Windows.UI.Composition.DesktopWindowContentBridge"})
        {
            IntPtr after=IntPtr.Zero;
            for(int i=0;i<128;i++)
            {
                IntPtr h=Native.FindWindowEx(IntPtr.Zero,after,className,null);
                if(h==IntPtr.Zero||h==after)break;after=h;
                try{if(seen.Add(h.ToInt64())&&TryTrustedWindow(h,out _,out _))roots.Add(AutomationElement.FromHandle(h));}
                catch(Exception e)when(IsPlatformFailure(e)){}
            }
        }
        Native.EnumWindows((h,p)=>
        {
            Native.GetWindowThreadProcessId(h,out uint pid);
            if(Native.IsWindowVisible(h))
            {
                try{if(seen.Add(h.ToInt64())&&TryTrustedWindow(h,out _,out _))roots.Add(AutomationElement.FromHandle(h));}
                catch(Exception e)when(IsPlatformFailure(e)){}
            }
            return true;
        },IntPtr.Zero);
        // EnumWindows alone is not sufficient for immersive shell roots on every Windows build.
        if(_hosts.Count>0)
        {
            var conditions=_hosts.Keys.Select(pid=>(Condition)new PropertyCondition(AutomationElement.ProcessIdProperty,pid)).ToArray();
            Condition filter=conditions.Length==1?conditions[0]:new OrCondition(conditions);
            foreach(AutomationElement root in AutomationElement.RootElement.FindAll(TreeScope.Children,filter))
            {
                long hwnd=root.Current.NativeWindowHandle;
                if(hwnd!=0&&seen.Add(hwnd)&&!root.Current.IsOffscreen&&TryTrustedWindow(new IntPtr(hwnd),out _,out _))roots.Add(root);
            }
        }
        return roots;
    }

    private static bool TryTrustedWindow(IntPtr hwnd,out int pid,out long start)
    {
        pid=0;start=0;
        if(hwnd==IntPtr.Zero||!Native.IsWindow(hwnd)||!Native.IsWindowVisible(hwnd))return false;
        if(!Native.GetWindowRect(hwnd,out var rect)||rect.Right-rect.Left<100||rect.Bottom-rect.Top<30)return false;
        var cls=new StringBuilder(128);Native.GetClassName(hwnd,cls,cls.Capacity);
        if(cls.ToString() is not ("Windows.UI.Core.CoreWindow" or "Xaml_WindowedPopupClass" or "Windows.UI.Composition.DesktopWindowContentBridge"))return false;
        Native.GetWindowThreadProcessId(hwnd,out uint process);pid=(int)process;
        return TryTrustedProcess(pid,out start);
    }

    private static bool TryTrustedProcess(int pid,out long start)
    {
        start=0;
        try
        {
            using var process=Process.GetProcessById(pid);
            IntPtr handle=Native.OpenProcess(0x1000,false,pid);
            if(handle==IntPtr.Zero)return false;
            string? path;
            try{var image=new StringBuilder(1024);int length=image.Capacity;path=Native.QueryFullProcessImageName(handle,0,image,ref length)?image.ToString():null;}
            finally{Native.CloseHandle(handle);}
            string windows=Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            bool trusted=string.Equals(path,Path.Combine(windows,"SystemApps","ShellExperienceHost_cw5n1h2txyewy","ShellExperienceHost.exe"),StringComparison.OrdinalIgnoreCase)||
                string.Equals(path,Path.Combine(windows,"System32","ShellHost.exe"),StringComparison.OrdinalIgnoreCase);
            if(trusted)start=process.StartTime.ToFileTimeUtc();
            return trusted;
        }
        catch(Exception e)when(IsPlatformFailure(e)){return false;}
    }
    private static bool IsPlatformFailure(Exception e)=>e is ElementNotAvailableException or InvalidOperationException or COMException or UnauthorizedAccessException or System.ComponentModel.Win32Exception or ArgumentException;
}
