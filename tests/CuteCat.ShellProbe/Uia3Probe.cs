using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using UIA=Interop.UIAutomationClient;

namespace CuteCat.ShellProbe;

internal static class Uia3Probe
{
    public static void Run(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var thread=new Thread(()=>
        {
            using var output=new StreamWriter(path){AutoFlush=true};
            var automation=(UIA.IUIAutomation)new UIA.CUIAutomation8();
            if(automation is UIA.IUIAutomation2 options){options.ConnectionTimeout=250;options.TransactionTimeout=500;}
            var clock=Stopwatch.StartNew();string previous="";
            while(clock.Elapsed.TotalSeconds<120)
            {
                var roots=new List<object>();IntPtr after=IntPtr.Zero;
                for(int i=0;i<64;i++)
                {
                    var h=FindWindowEx(IntPtr.Zero,after,"Windows.UI.Core.CoreWindow",null);if(h==IntPtr.Zero||h==after)break;after=h;
                    GetWindowThreadProcessId(h,out uint id);
                    using var process=Process.GetProcessById((int)id);
                    if(process.ProcessName is not ("ShellExperienceHost" or "ShellHost"))continue;
                    try
                    {
                        var root=automation.ElementFromHandle(h);var cache=automation.CreateCacheRequest();
                        foreach(int prop in new[]{30001,30002,30003,30010,30011,30012,30022,30031})cache.AddProperty(prop);
                        var found=root.FindAllBuildCache(UIA.TreeScope.TreeScope_Subtree,automation.CreateTrueCondition(),cache);
                        var nodes=new List<object>();
                        for(int n=0;n<Math.Min(found.Length,300);n++)
                        {
                            var e=found.GetElement(n);int type=(int)e.GetCachedPropertyValue(30003);
                            if(type is 50020 or 50006)continue;
                            nodes.Add(new{type,id=e.GetCachedPropertyValue(30011),klass=e.GetCachedPropertyValue(30012),enabled=e.GetCachedPropertyValue(30010),off=e.GetCachedPropertyValue(30022),invoke=e.GetCachedPropertyValue(30031),rect=e.GetCachedPropertyValue(30001)});
                        }
                        roots.Add(new{hwnd=h.ToInt64(),process=process.ProcessName,nodes});
                    }
                    catch(Exception e){roots.Add(new{hwnd=h.ToInt64(),error=e.GetType().Name,code=e.HResult});}
                }
                string current=JsonSerializer.Serialize(roots);
                if(current!=previous){output.WriteLine(JsonSerializer.Serialize(new{at=clock.Elapsed.TotalMilliseconds,roots}));previous=current;}
                Thread.Sleep(100);
            }
        });
        thread.SetApartmentState(ApartmentState.MTA);thread.Start();thread.Join();
    }
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string klass,string? title);
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
}
