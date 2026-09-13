using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;

namespace CuteCat.App;

/// <summary>A payload-free request to show this instance's own window. No general IPC commands are accepted.</summary>
internal sealed class SingleInstanceActivation : IDisposable
{
    private readonly HwndSource _source;
    private readonly uint _message;
    private readonly Action _open;
    internal SingleInstanceActivation(string key,Action open)
    {
        _open=open;_message=RegisterWindowMessage("CuteCat.Open."+key);
        _source=new HwndSource(new HwndSourceParameters("CuteCat.Activation."+key){ParentWindow=new IntPtr(-3),WindowStyle=0,Width=0,Height=0});
        _source.AddHook(Receive);
        // A normal shortcut may ask an existing UIAccess instance to show itself. Only this zero-payload message is allowed.
        ChangeWindowMessageFilterEx(_source.Handle,_message,1,IntPtr.Zero);
    }
    private IntPtr Receive(IntPtr hwnd,int message,IntPtr wp,IntPtr lp,ref bool handled)
    {
        if((uint)message==_message&&wp==IntPtr.Zero&&lp==IntPtr.Zero)
        {handled=true;_source.Dispatcher.BeginInvoke(DispatcherPriority.Normal,_open);}
        return IntPtr.Zero;
    }
    internal static async Task<bool> NotifyAsync(string key)
    {
        uint message=RegisterWindowMessage("CuteCat.Open."+key);
        // Covers two shortcuts clicked while the first process is still starting.
        for(int i=0;i<100;i++)
        {
            IntPtr target=Native.FindWindowEx(new IntPtr(-3),IntPtr.Zero,null,"CuteCat.Activation."+key);
            if(target!=IntPtr.Zero)
            {
                Native.GetWindowThreadProcessId(target,out uint pid);AllowSetForegroundWindow(pid);
                if(PostMessage(target,message,IntPtr.Zero,IntPtr.Zero))return true;
            }
            await Task.Delay(50);
        }
        return false;
    }
    public void Dispose(){_source.RemoveHook(Receive);_source.Dispose();}
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern uint RegisterWindowMessage(string message);
    [DllImport("user32.dll")]private static extern bool PostMessage(IntPtr hwnd,uint message,IntPtr w,IntPtr l);
    [DllImport("user32.dll")]private static extern bool AllowSetForegroundWindow(uint pid);
    [DllImport("user32.dll")]private static extern bool ChangeWindowMessageFilterEx(IntPtr hwnd,uint message,uint action,IntPtr change);
}
