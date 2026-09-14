using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace CuteCat.ShellProbe;

internal static class AppWindowFixture
{
    public static void Run(string directory,bool veto,int menuOwner=0,bool customCaption=false,bool disableClose=false)
    {
        Directory.CreateDirectory(directory);
        var app=new Application{ShutdownMode=ShutdownMode.OnMainWindowClose};
        var area=SystemParameters.WorkArea;
        double width=Math.Min(470,Math.Max(280,area.Width-80)),height=Math.Min(290,Math.Max(180,area.Height-80));
        var window=new Window{Title="Cute Cat app-rule fixture",Width=width,Height=height,
            Left=area.Right-width-40,Top=area.Top+Math.Min(160,Math.Max(20,area.Height-height-40)),
            Content=new TextBlock{Text="A test-owned window. No user documents are opened.",Margin=new Thickness(25),TextWrapping=TextWrapping.Wrap}};
        int requests=0;bool stopping=false;Window? prompt=null;
        window.SourceInitialized+=(_,_)=>
        {
            var handle=new WindowInteropHelper(window).Handle;
            if(customCaption)HwndSource.FromHwnd(handle)?.AddHook((IntPtr h,int message,IntPtr wp,IntPtr lp,ref bool handled)=>
            {
                if(message==0x33F&&lp!=IntPtr.Zero)
                {
                    // Reproduce an AWT/custom frame: the legacy query succeeds but has no button rectangles.
                    for(int offset=44;offset<140;offset+=4)Marshal.WriteInt32(lp,offset,0);
                    handled=true;return new IntPtr(1);
                }
                return IntPtr.Zero;
            });
            if(disableClose)EnableMenuItem(GetSystemMenu(handle,false),0xF060,1);
        };
        window.Closing+=(_,e)=>
        {
            if(stopping)return;
            requests++;File.WriteAllText(Path.Combine(directory,"close-requests.txt"),requests.ToString());
            if(veto)
            {
                e.Cancel=true;
                prompt=new Window{Owner=window,Title="Fixture save confirmation",Width=320,Height=150,
                    WindowStartupLocation=WindowStartupLocation.CenterOwner,Content=new TextBlock{Text="Save prompt deliberately remains open.",Margin=new Thickness(20)}};
                prompt.Show();prompt.Activate();
            }
        };
        var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(100)};
        timer.Tick+=(_,_)=>{if(File.Exists(Path.Combine(directory,"quit"))){stopping=true;prompt?.Close();window.Close();}};timer.Start();
        window.Loaded+=(_,_)=>{window.Activate();if(menuOwner>0)AllowSetForegroundWindow((uint)menuOwner);File.WriteAllText(Path.Combine(directory,"ready"),Environment.ProcessId.ToString());};
        app.Run(window);
    }
    [DllImport("user32.dll")]private static extern bool AllowSetForegroundWindow(uint process);
    [DllImport("user32.dll")]private static extern IntPtr GetSystemMenu(IntPtr window,bool revert);
    [DllImport("user32.dll")]private static extern uint EnableMenuItem(IntPtr menu,uint item,uint flags);
}
