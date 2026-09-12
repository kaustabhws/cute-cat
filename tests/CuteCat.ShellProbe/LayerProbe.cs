using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CuteCat.ShellProbe;

// Read-only z-order/geometry observation. No UIA content, screenshots or input.
internal static class LayerProbe
{
    public static void Run(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        SetThreadDpiAwarenessContext(new IntPtr(-4));
        using var output=new StreamWriter(path){AutoFlush=true};var timer=Stopwatch.StartNew();
        var processes=new HashSet<int>();string previous="";double refresh=0;
        while(timer.Elapsed.TotalSeconds<600)
        {
            if(timer.Elapsed.TotalSeconds>=refresh)
            {
                processes.Clear();refresh=timer.Elapsed.TotalSeconds+2;
                foreach(string name in new[]{"CuteCat","ShellExperienceHost"})
                    foreach(var p in Process.GetProcessesByName(name)){processes.Add(p.Id);p.Dispose();}
            }
            var rows=new List<object>();var after=IntPtr.Zero;var seen=new HashSet<IntPtr>();
            for(int order=0;order<2048;order++)
            {
                var h=FindWindowEx(IntPtr.Zero,after,null,null);if(h==IntPtr.Zero||!seen.Add(h))break;after=h;
                GetWindowThreadProcessId(h,out uint id);if(!processes.Contains((int)id)||!IsWindowVisible(h))continue;
                var cls=new StringBuilder(128);GetClassName(h,cls,128);string name=cls.ToString();
                if(!name.StartsWith("CuteCat.Vector.")&&name!="Windows.UI.Core.CoreWindow")continue;
                GetWindowBand(h,out uint band);GetWindowRect(h,out var r);
                if(r.Right<=r.Left||r.Bottom<=r.Top)continue;
                rows.Add(new{hwnd=h.ToInt64(),order,pid=id,band,cat=name.StartsWith("CuteCat.Vector."),rect=new[]{r.Left,r.Top,r.Right-r.Left,r.Bottom-r.Top}});
            }
            string snapshot=JsonSerializer.Serialize(rows);
            if(snapshot!=previous){output.WriteLine(JsonSerializer.Serialize(new{seconds=timer.Elapsed.TotalSeconds,rows}));previous=snapshot;}
            Thread.Sleep(50);
        }
    }
    [StructLayout(LayoutKind.Sequential)]private struct Rect{public int Left,Top,Right,Bottom;}
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string? klass,string? title);
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
    [DllImport("user32.dll")]private static extern bool GetWindowBand(IntPtr h,out uint band);
    [DllImport("user32.dll")]private static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern int GetClassName(IntPtr h,StringBuilder text,int length);
    [DllImport("user32.dll")]private static extern bool GetWindowRect(IntPtr h,out Rect rect);
    [DllImport("user32.dll")]private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
}
