using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using UIA=Interop.UIAutomationClient;

namespace CuteCat.ShellProbe;

// Explicit developer probe of one supplied process: structural metadata/geometry only, no titles or content.
internal static class AppControlProbe
{
    public static void Run(int processId,string path)
    {
        var worker=new Thread(()=>
        {
            var results=new List<object>();var automation=(UIA.IUIAutomation)new UIA.CUIAutomation8();
            if(automation is UIA.IUIAutomation2 options){options.ConnectionTimeout=200;options.TransactionTimeout=500;}
            EnumWindows((h,_)=>
            {
                GetWindowThreadProcessId(h,out uint pid);if(pid!=processId||!IsWindowVisible(h))return true;
                GetWindowRect(h,out var bounds);var klass=new StringBuilder(128);GetClassName(h,klass,128);
                var title=new TitleBar{Size=(uint)Marshal.SizeOf<TitleBar>(),States=new uint[6],Rects=new Rect[6]};
                bool native=SendTitle(h,0x33F,IntPtr.Zero,ref title,2,100,out _)!=IntPtr.Zero;
                int dwmResult=DwmGetWindowAttribute(h,5,out var caption,Marshal.SizeOf<Rect>());
                var hitPoints=new List<object>();double dpi=Math.Max(96,GetDpiForWindow(h))/96d;
                for(int y=8;y<=56;y+=12)for(int x=12;x<=84;x+=18)
                {
                    int sx=bounds.Right-(int)(x*dpi),sy=bounds.Top+(int)(y*dpi);long packed=((long)(ushort)sy<<16)|(ushort)sx;
                    bool hit=SendHit(h,0x84,IntPtr.Zero,new IntPtr(packed),2,50,out var result)!=IntPtr.Zero;
                    hitPoints.Add(new{x=sx,y=sy,ok=hit,result=result.ToInt64()});
                }
                var controls=new List<object>();string? error=null;
                try
                {
                    var root=automation.ElementFromHandle(h);var condition=automation.CreatePropertyCondition(30003,50000); // Button
                    var elements=root.FindAll(UIA.TreeScope.TreeScope_Descendants,condition);
                    for(int i=0;i<Math.Min(elements.Length,100);i++)
                    {
                        var item=elements.GetElement(i);var r=item.CurrentBoundingRectangle;
                        if(r.right<bounds.Right-160*dpi||r.top>bounds.Top+100*dpi)continue;
                        var parent=automation.RawViewWalker.GetParentElement(item);
                        controls.Add(new{id=item.CurrentAutomationId,type=item.CurrentControlType,klass=item.CurrentClassName,
                            enabled=item.CurrentIsEnabled,offscreen=item.CurrentIsOffscreen,invoke=item.GetCurrentPropertyValue(30031),
                            rect=new[]{r.left,r.top,r.right,r.bottom},parentType=parent?.CurrentControlType,parentId=parent?.CurrentAutomationId});
                    }
                }
                catch(Exception e){error=e.GetType().Name;}
                results.Add(new{window=h.ToInt64(),klass=klass.ToString(),owner=GetWindow(h,4).ToInt64(),enabled=IsWindowEnabled(h),
                    foreground=GetForegroundWindow()==h,style=GetWindowLongPtr(h,-16).ToInt64(),bounds,dpi,titleRead=native,title,dwmResult,caption,hitPoints,controls,error});
                return true;
            },IntPtr.Zero);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path,JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true,IncludeFields=true}));
        });
        worker.SetApartmentState(ApartmentState.MTA);worker.Start();worker.Join();
    }
    private delegate bool EnumProc(IntPtr h,IntPtr p);
    [StructLayout(LayoutKind.Sequential)]private struct Rect{public int Left,Top,Right,Bottom;}
    [StructLayout(LayoutKind.Sequential)]private struct TitleBar{public uint Size;public Rect Rect;[MarshalAs(UnmanagedType.ByValArray,SizeConst=6)]public uint[] States;[MarshalAs(UnmanagedType.ByValArray,SizeConst=6)]public Rect[] Rects;}
    [DllImport("user32.dll")]private static extern bool EnumWindows(EnumProc callback,IntPtr p);
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(IntPtr h,out uint process);
    [DllImport("user32.dll")]private static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")]private static extern bool IsWindowEnabled(IntPtr h);
    [DllImport("user32.dll")]private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]private static extern IntPtr GetWindow(IntPtr h,uint command);
    [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")]private static extern IntPtr GetWindowLongPtr(IntPtr h,int index);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern int GetClassName(IntPtr h,StringBuilder text,int max);
    [DllImport("user32.dll")]private static extern bool GetWindowRect(IntPtr h,out Rect rect);
    [DllImport("user32.dll")]private static extern uint GetDpiForWindow(IntPtr h);
    [DllImport("user32.dll",EntryPoint="SendMessageTimeoutW")]private static extern IntPtr SendTitle(IntPtr h,uint message,IntPtr wp,ref TitleBar title,uint flags,uint timeout,out IntPtr result);
    [DllImport("user32.dll",EntryPoint="SendMessageTimeoutW")]private static extern IntPtr SendHit(IntPtr h,uint message,IntPtr wp,IntPtr lp,uint flags,uint timeout,out IntPtr result);
    [DllImport("dwmapi.dll")]private static extern int DwmGetWindowAttribute(IntPtr h,int attribute,out Rect rect,int size);
}
