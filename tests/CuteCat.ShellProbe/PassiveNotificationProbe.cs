using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace CuteCat.ShellProbe;

// Opt-in development observation only. No notification/window names, message text or images.
internal static class PassiveNotificationProbe
{
    public static void Run(string file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file))!);
        Task.Run(async()=>
        {
            using var writer=new StreamWriter(file,false){AutoFlush=true};var clock=Stopwatch.StartNew();string previous="";
            while(clock.Elapsed.TotalSeconds<600)
            {
                var start=clock.Elapsed.TotalMilliseconds;var roots=new List<object>();
                try
                {
                    var hosts=new Dictionary<int,string>();
                    foreach(string name in new[]{"ShellExperienceHost","ShellHost","explorer","SnippingTool","ApplicationFrameHost","NotificationUxBroker","RuntimeBroker"})
                        foreach(var p in Process.GetProcessesByName(name)){hosts[p.Id]=p.ProcessName;p.Dispose();}
                    var seen=new HashSet<int>();
                    var candidates=new List<AutomationElement>();
                    foreach(string name in new[]{"Windows.UI.Core.CoreWindow","Xaml_WindowedPopupClass","Windows.UI.Composition.DesktopWindowContentBridge","Microsoft.UI.Content.DesktopChildSiteBridge"})
                    {
                        IntPtr after=IntPtr.Zero;
                        for(int i=0;i<128;i++)
                        {
                            IntPtr h=FindWindowEx(IntPtr.Zero,after,name,null);
                            if(h==IntPtr.Zero||h==after)break;after=h;
                            GetWindowThreadProcessId(h,out uint pid);
                            if(hosts.ContainsKey((int)pid)&&IsWindowVisible(h)&&seen.Add(h.ToInt32()))
                                try{candidates.Add(AutomationElement.FromHandle(h));}catch{}
                        }
                    }
                    EnumWindows((h,_)=>
                    {
                        GetWindowThreadProcessId(h,out uint pid);
                        if(!hosts.ContainsKey((int)pid)||!IsWindowVisible(h))return true;
                        var klass=new StringBuilder(256);GetClassName(h,klass,klass.Capacity);
                        if(hosts[(int)pid]=="explorer"&&klass.ToString() is not ("Windows.UI.Core.CoreWindow" or "Xaml_WindowedPopupClass" or "Windows.UI.Composition.DesktopWindowContentBridge" or "Microsoft.UI.Content.DesktopChildSiteBridge"))return true;
                        try{candidates.Add(AutomationElement.FromHandle(h));seen.Add(h.ToInt32());}catch{}
                        return true;
                    },IntPtr.Zero);
                    foreach(AutomationElement root in AutomationElement.RootElement.FindAll(TreeScope.Children,Condition.TrueCondition))
                    {
                        var state=root.Current;
                        if(hosts.ContainsKey(state.ProcessId)&&!state.IsOffscreen&&seen.Add(state.NativeWindowHandle)&&hosts[state.ProcessId]!="explorer")candidates.Add(root);
                    }
                    foreach(var root in candidates)
                    {
                        var r=root.Current;
                        if(r.ClassName is "CabinetWClass" or "Progman" or "Shell_TrayWnd")continue;
                        var nodes=new List<object>();var queue=new Queue<(AutomationElement,int)>();queue.Enqueue((root,0));var watch=Stopwatch.StartNew();int visited=0;
                        while(queue.Count>0&&visited++<250&&watch.ElapsedMilliseconds<450)
                        {
                            var (e,depth)=queue.Dequeue();var c=e.Current;var rect=c.BoundingRectangle;
                            if(c.ControlType!=ControlType.Text&&c.ControlType!=ControlType.Image)
                            {
                                bool? scroll=null;
                                if(e.TryGetCurrentPattern(ScrollPattern.Pattern,out var pattern))scroll=((ScrollPattern)pattern).Current.VerticallyScrollable;
                                nodes.Add(new{depth,id=c.AutomationId,klass=c.ClassName,type=c.ControlType.ProgrammaticName,off=c.IsOffscreen,enabled=c.IsEnabled,
                                    invoke=e.TryGetCurrentPattern(InvokePattern.Pattern,out _),scroll,rect=rect.IsEmpty?null:new[]{rect.X,rect.Y,rect.Width,rect.Height}});
                            }
                            if(depth>=12||c.IsOffscreen)continue;
                            var child=TreeWalker.RawViewWalker.GetFirstChild(e);
                            while(child is not null&&queue.Count<250){queue.Enqueue((child,depth+1));child=TreeWalker.RawViewWalker.GetNextSibling(child);}
                        }
                        roots.Add(new{process=hosts.GetValueOrDefault(r.ProcessId,"unknown"),hwnd=r.NativeWindowHandle,nodes});
                    }
                }
                catch(Exception e){roots.Add(new{error=e.GetType().Name});}
                string snapshot=JsonSerializer.Serialize(roots);
                if(snapshot!=previous){writer.WriteLine(JsonSerializer.Serialize(new{atMs=clock.Elapsed.TotalMilliseconds,scanMs=clock.Elapsed.TotalMilliseconds-start,roots}));previous=snapshot;}
                await Task.Delay(70);
            }
        }).GetAwaiter().GetResult();
    }
    private delegate bool EnumCallback(IntPtr hwnd,IntPtr parameter);
    [DllImport("user32.dll")]private static extern bool EnumWindows(EnumCallback callback,IntPtr parameter);
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
    [DllImport("user32.dll")]private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string className,string? title);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern int GetClassName(IntPtr hwnd,StringBuilder name,int length);
}
