using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace CuteCat.ShellProbe;

internal static class DialogWindowFixture
{
    public static void Run(string directory,bool mainWindow)
    {
        Directory.CreateDirectory(directory);var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};
        using var stream=new MemoryStream();using(var writer=new BinaryWriter(stream,Encoding.Unicode,true))
        {
            writer.Write(0x80C80080u|(mainWindow?0x20000u:0));writer.Write(0u);writer.Write((ushort)0);
            writer.Write((short)0);writer.Write((short)0);writer.Write((short)230);writer.Write((short)110);
            writer.Write((ushort)0);writer.Write((ushort)0);writer.Write(Encoding.Unicode.GetBytes("Cute Cat dialog fixture\0"));
        }
        IntPtr template=Marshal.AllocHGlobal((int)stream.Length),window=IntPtr.Zero;Marshal.Copy(stream.ToArray(),0,template,(int)stream.Length);
        bool stopping=false;int closes=0;
        DialogProc procedure=(h,message,wp,lp)=>
        {
            if(message==0x10)
            {if(!stopping)File.WriteAllText(Path.Combine(directory,"close-requests.txt"),(++closes).ToString());DestroyWindow(h);app.Shutdown();return new IntPtr(1);}
            return IntPtr.Zero;
        };
        var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(100)};
        timer.Tick+=(_,_)=>{if(File.Exists(Path.Combine(directory,"quit"))){stopping=true;DestroyWindow(window);app.Shutdown();}};
        app.Startup+=(_,_)=>
        {
            window=CreateDialogIndirectParam(IntPtr.Zero,template,IntPtr.Zero,procedure,IntPtr.Zero);
            if(window==IntPtr.Zero)throw new System.ComponentModel.Win32Exception();
            var area=SystemParameters.WorkArea;
            SetWindowPos(window,IntPtr.Zero,(int)area.Right-540,(int)area.Top+130,480,270,0x40);SetForegroundWindow(window);
            File.WriteAllText(Path.Combine(directory,"ready"),Environment.ProcessId.ToString());timer.Start();
        };
        try{app.Run();}finally{Marshal.FreeHGlobal(template);GC.KeepAlive(procedure);}
    }
    private delegate IntPtr DialogProc(IntPtr h,int message,IntPtr wp,IntPtr lp);
    [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern IntPtr CreateDialogIndirectParam(IntPtr module,IntPtr template,IntPtr owner,DialogProc procedure,IntPtr init);
    [DllImport("user32.dll")]private static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int height,uint flags);
    [DllImport("user32.dll")]private static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")]private static extern bool DestroyWindow(IntPtr h);
}
