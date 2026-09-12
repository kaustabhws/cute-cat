using System.Diagnostics;
using System.Windows.Threading;

namespace CuteCat.App;

public sealed class FrameClock : IDisposable
{
    public static double Now=>Stopwatch.GetTimestamp()/(double)Stopwatch.Frequency;
    public int FramesPerSecond
    {
        get=>Volatile.Read(ref _fps);
        set {int next=Math.Clamp(value,0,120);if(Interlocked.Exchange(ref _fps,next)!=next)_changed.Set();}
    }
    private int _fps=60,_queued;
    private readonly CancellationTokenSource _stop=new();
    private readonly AutoResetEvent _changed=new(false);
    private readonly Thread _worker;
    public FrameClock(Dispatcher dispatcher,Action<double> frame)
    {
        _worker=new Thread(()=>
        {
            IntPtr timer=Native.CreateWaitableTimerEx(IntPtr.Zero,null,2,0x1f0003);
            if(timer==IntPtr.Zero)timer=Native.CreateWaitableTimerEx(IntPtr.Zero,null,0,0x1f0003);
            double deadline=Now;
            int previous=-1;
            var waits=new[]{timer,_changed.SafeWaitHandle.DangerousGetHandle(),_stop.Token.WaitHandle.SafeWaitHandle.DangerousGetHandle()};
            WaitHandle[] managedWaits=[_changed,_stop.Token.WaitHandle];
            try
            {
                while(!_stop.IsCancellationRequested)
                {
                    int fps=FramesPerSecond;
                    if(fps==0) {WaitHandle.WaitAny(managedWaits);previous=-1;continue;}
                    double step=1d/fps;
                    if(fps!=previous){deadline=Now;previous=fps;}
                    deadline+=step;double remaining=deadline-Now;
                    if(remaining<=0){deadline=Now;remaining=.0001;}
                    if(timer!=IntPtr.Zero)
                    {
                        long due=-(long)Math.Max(1,remaining*10_000_000);
                        Native.SetWaitableTimer(timer,ref due,0,IntPtr.Zero,IntPtr.Zero,false);
                        uint ready=Native.WaitForMultipleObjects(3,waits,false,0xffffffff);
                        if(ready!=0){previous=-1;continue;}
                    }
                    else
                    {
                        int ready=WaitHandle.WaitAny(managedWaits,(int)Math.Max(1,remaining*1000));
                        if(ready!=WaitHandle.WaitTimeout){previous=-1;continue;}
                    }
                    if(_stop.IsCancellationRequested)continue;
                    if(Interlocked.CompareExchange(ref _queued,1,0)!=0)continue;
                    dispatcher.BeginInvoke(DispatcherPriority.Render,new Action(()=>
                    {
                        Interlocked.Exchange(ref _queued,0);
                        if(!_stop.IsCancellationRequested)frame(Now);
                    }));
                }
            }
            finally { if(timer!=IntPtr.Zero)Native.CloseHandle(timer); }
        }){IsBackground=true,Name="Cute Cat frame clock"};
        _worker.Start();
    }
    public void Dispose() { _stop.Cancel();_worker.Join();_changed.Dispose();_stop.Dispose(); }
}
