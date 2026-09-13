using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using Condition=System.Windows.Automation.Condition;
using System.Windows.Controls;
using System.Windows.Threading;
using Forms=System.Windows.Forms;
using CommunityToolkit.WinUI.Notifications;

namespace CuteCat.ShellProbe;

// Development-only probe: reports shell structure, never notification Name/Text/Value.
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var command=Environment.GetCommandLineArgs();
        if(command.Length>=3&&command[1]=="--app-window")
        {int option=Array.IndexOf(command,"--menu-owner");int owner=option>=0&&option+1<command.Length&&int.TryParse(command[option+1],out int value)?value:0;AppWindowFixture.Run(command[2],command.Contains("--veto"),owner);return;}
        if(command.Length>=3&&command[1]=="--layers"){LayerProbe.Run(command[2]);return;}
        if(command.Length>=3&&command[1]=="--uia3"){Uia3Probe.Run(command[2]);return;}
        if(command.Length>=3&&command[1]=="--watch"){PassiveNotificationProbe.Run(command[2]);return;}
        if(NormalUserLauncher.Relaunch())return;
        var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};
        var window=new Window{Title="Cute Cat notification check",Width=380,Height=150,WindowStartupLocation=WindowStartupLocation.CenterScreen,
            Content=new TextBlock{Text="Checking the Windows notification close control.\nThis test does not read notification messages.",Margin=new Thickness(20),TextWrapping=TextWrapping.Wrap}};
        var tray=new Forms.NotifyIcon{Icon=System.Drawing.SystemIcons.Information,Visible=true,Text="Cute Cat notification check"};
        bool shown=false,closed=false;string toastResult="sent";
        tray.BalloonTipShown+=(_,_)=>shown=true;tray.BalloonTipClosed+=(_,_)=>closed=true;
        app.Startup+=async(_,_)=>
        {
            window.Show();
            string desktop=DesktopName();
            var snapshots=new List<object>();
            snapshots.Add(new{at="before",shell=await Task.Run(Survey)});
            try
            {
                ToastNotificationManagerCompat.OnActivated+=_=>{};
                new ToastContentBuilder().AddText("Cute Cat notification check").AddText("Testing a paw tap on a real Windows 11 toast.")
                    .AddAudio(new ToastAudio{Silent=true}).SetToastScenario(ToastScenario.Reminder).SetToastDuration(ToastDuration.Long).Show(toast=>
                    {
                        toast.Tag="CuteCatProbe";toast.ExpirationTime=DateTimeOffset.Now.AddSeconds(20);toast.Dismissed+=(_,args)=>{closed=true;toastResult=args.Reason.ToString();};
                        toast.Failed+=(_,args)=>toastResult="failed:"+args.ErrorCode.HResult;
                    });
                shown=true;
            }
            catch(Exception error){toastResult=error.GetType().Name+":"+error.HResult;}
            for(int i=0;i<35;i++)
            {
                await Task.Delay(200);
                snapshots.Add(new{at=$"{(i+1)*200}ms",shell=await Task.Run(Survey)});
            }
            string output=Path.Combine(AppContext.BaseDirectory,"Results");Directory.CreateDirectory(output);
            var station=new StringBuilder(256);GetUserObjectInformation(GetProcessWindowStation(),2,station,station.Capacity*2,out _);
            SHQueryUserNotificationState(out int notificationState);
            string setting=ToastNotificationManagerCompat.CreateToastNotifier().Setting.ToString();
            int ownHistory=ToastNotificationManagerCompat.History.GetHistory().Count;
            var identity=System.Security.Principal.WindowsIdentity.GetCurrent();var principal=new System.Security.Principal.WindowsPrincipal(identity);
            bool elevated=principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            File.WriteAllText(Path.Combine(output,"probe.json"),JsonSerializer.Serialize(new{desktop,station=station.ToString(),session=Process.GetCurrentProcess().SessionId,elevated,launcher=NormalUserLauncher.State,os=Environment.OSVersion.ToString(),shown,closed,toastResult,notificationState,setting,ownHistory,snapshots},new JsonSerializerOptions{WriteIndented=true}));
            tray.Visible=false;tray.Dispose();app.Shutdown();
        };
        app.Run();
    }

    private static List<object> Survey()
    {
        var output=new List<object>();int enumerated=0;
        bool enumeration=EnumWindows((hwnd,param)=>
        {
            enumerated++;
            GetWindowThreadProcessId(hwnd,out uint pid);
            try
            {
                using var process=Process.GetProcessById((int)pid);
                string name=process.ProcessName;
                if(name is not ("ShellExperienceHost" or "ShellHost" or "explorer"))return true;
                string? executable=process.MainModule?.FileName;
                if(executable is null || !executable.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows)+"\\",StringComparison.OrdinalIgnoreCase))return true;
                var cls=new StringBuilder(256);GetClassName(hwnd,cls,cls.Capacity);
                string nativeClass=cls.ToString();bool visible=IsWindowVisible(hwnd);
                if(!visible){output.Add(new{process=name,nativeClass,visible});return true;}
                if(name=="explorer"&&nativeClass is not ("Windows.UI.Core.CoreWindow" or "Xaml_WindowedPopupClass" or "Windows.UI.Composition.DesktopWindowContentBridge" or "tooltips_class32"))return true;
                GetWindowRect(hwnd,out var r);
                var nodes=new List<object>();
                if(name!="explorer"||nativeClass=="Xaml_WindowedPopupClass")
                {
                    var root=AutomationElement.FromHandle(hwnd);
                    var queue=new Queue<(AutomationElement element,int depth)>();queue.Enqueue((root,0));
                    var watch=Stopwatch.StartNew();int visited=0;
                    while(queue.Count>0&&visited++<200&&watch.ElapsedMilliseconds<350)
                    {
                        var (element,depth)=queue.Dequeue();var current=element.Current;
                        if(current.ControlType!=ControlType.Text && current.ControlType!=ControlType.Image)
                        {
                            string id=current.AutomationId,klass=current.ClassName;
                            id=Regex.IsMatch(id,"^[A-Za-z_][A-Za-z0-9_.-]{0,100}$")?id:"";
                            klass=Regex.IsMatch(klass,"^[A-Za-z_][A-Za-z0-9_.-]{0,100}$")?klass:"";
                            var box=current.BoundingRectangle;
                            nodes.Add(new{depth,id,klass,type=current.ControlType.ProgrammaticName,offscreen=current.IsOffscreen,
                                enabled=current.IsEnabled,invoke=element.TryGetCurrentPattern(InvokePattern.Pattern,out _),
                                rect=box.IsEmpty?null:new double[]{box.X,box.Y,box.Width,box.Height}});
                        }
                        if(depth>=9)continue;
                        var child=TreeWalker.RawViewWalker.GetFirstChild(element);
                        while(child is not null&&queue.Count<200){queue.Enqueue((child,depth+1));child=TreeWalker.RawViewWalker.GetNextSibling(child);}
                    }
                }
                output.Add(new{process=name,nativeClass,visible,rect=new[]{r.Left,r.Top,r.Right-r.Left,r.Bottom-r.Top},nodes});
            }
            catch(Exception e)when(e is InvalidOperationException or System.ComponentModel.Win32Exception or COMException or ElementNotAvailableException or UnauthorizedAccessException)
            {output.Add(new{failure=e.GetType().Name});}
            return true;
        },IntPtr.Zero);
        output.Add(new{source="enumeration",enumeration,enumerated});
        try
        {
            var ids=new List<int>();
            foreach(string name in new[]{"ShellExperienceHost","ShellHost"})
                foreach(var process in Process.GetProcessesByName(name)){ids.Add(process.Id);process.Dispose();}
            if(ids.Count>0)
            {
                var conditions=ids.Select(id=>(Condition)new PropertyCondition(AutomationElement.ProcessIdProperty,id)).ToArray();
                Condition filter=conditions.Length==1?conditions[0]:new OrCondition(conditions);
                var roots=AutomationElement.RootElement.FindAll(TreeScope.Children,filter);
                var topLevel=AutomationElement.RootElement.FindAll(TreeScope.Children,Condition.TrueCondition);
                var systemRoots=new List<object>();
                foreach(AutomationElement top in topLevel)
                {
                    try
                    {
                        using var process=Process.GetProcessById(top.Current.ProcessId);
                        string? file=process.MainModule?.FileName;
                        if(file is not null&&file.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows)+"\\",StringComparison.OrdinalIgnoreCase))
                            systemRoots.Add(new{process=process.ProcessName,klass=top.Current.ClassName,hwnd=top.Current.NativeWindowHandle,offscreen=top.Current.IsOffscreen});
                    }
                    catch(ArgumentException){}catch(System.ComponentModel.Win32Exception){}
                }
                output.Add(new{source="rootSummary",total=topLevel.Count,systemRoots});
                foreach(AutomationElement root in roots)
                {
                    var nodes=new List<object>();var queue=new Queue<(AutomationElement,int)>();queue.Enqueue((root,0));int visited=0;
                    while(queue.Count>0&&visited++<240)
                    {
                        var (element,depth)=queue.Dequeue();var current=element.Current;
                        if(current.ControlType!=ControlType.Text&&current.ControlType!=ControlType.Image)
                        {
                            var box=current.BoundingRectangle;
                            string id=current.AutomationId,klass=current.ClassName;
                            nodes.Add(new{depth,id=Regex.IsMatch(id,"^[A-Za-z_][A-Za-z0-9_.-]{0,100}$")?id:"",klass=Regex.IsMatch(klass,"^[A-Za-z_][A-Za-z0-9_.-]{0,100}$")?klass:"",
                                type=current.ControlType.ProgrammaticName,offscreen=current.IsOffscreen,enabled=current.IsEnabled,
                                invoke=element.TryGetCurrentPattern(InvokePattern.Pattern,out _),rect=box.IsEmpty?null:new double[]{box.X,box.Y,box.Width,box.Height}});
                        }
                        if(depth>=9||current.IsOffscreen)continue;
                        var child=TreeWalker.RawViewWalker.GetFirstChild(element);
                        while(child is not null&&queue.Count<240){queue.Enqueue((child,depth+1));child=TreeWalker.RawViewWalker.GetNextSibling(child);}
                    }
                    output.Add(new{source="uia",process=root.Current.ProcessId,hwnd=root.Current.NativeWindowHandle,nodes});
                }
            }
        }
        catch(Exception e){output.Add(new{source="uia",error=e.GetType().Name});}
        return output;
    }

    private static string DesktopName()
    {
        IntPtr desktop=GetThreadDesktop(GetCurrentThreadId());var value=new StringBuilder(256);
        return GetUserObjectInformation(desktop,2,value,value.Capacity*2,out _)?value.ToString():"unavailable";
    }
    private delegate bool EnumCallback(IntPtr hwnd,IntPtr param);
    [StructLayout(LayoutKind.Sequential)] private struct Rect{public int Left,Top,Right,Bottom;}
    [DllImport("user32.dll")]private static extern bool EnumWindows(EnumCallback cb,IntPtr arg);
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
    [DllImport("user32.dll")]private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern int GetClassName(IntPtr hwnd,StringBuilder value,int size);
    [DllImport("user32.dll")]private static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
    [DllImport("user32.dll")]private static extern IntPtr GetThreadDesktop(uint thread);
    [DllImport("user32.dll")]private static extern IntPtr GetProcessWindowStation();
    [DllImport("shell32.dll")]private static extern int SHQueryUserNotificationState(out int state);
    [DllImport("kernel32.dll")]private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern bool GetUserObjectInformation(IntPtr handle,int index,StringBuilder value,int size,out int needed);
}
