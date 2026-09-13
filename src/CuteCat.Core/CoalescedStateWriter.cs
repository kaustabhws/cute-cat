using System.Diagnostics;

namespace CuteCat.Core;

/// <summary>One background writer and one latest snapshot. Runtime policy changes never wait for disk.</summary>
public sealed class CoalescedStateWriter(Func<AppState,Task> write)
{
    private readonly object _gate=new();
    private AppState? _pending;
    private Task? _worker;
    private long _first,_last;
    private bool _flush;
    private int _failed;
    private long _writes;
    public bool HasError=>Volatile.Read(ref _failed)!=0;
    public long Writes=>Interlocked.Read(ref _writes);
    public void Queue(AppState snapshot)
    {
        lock(_gate)
        {
            long now=Stopwatch.GetTimestamp();if(_pending is null)_first=now;
            _pending=snapshot;_last=now;_worker??=Task.Run(Drain);
        }
    }
    public Task FlushAsync()
    {lock(_gate){if(_worker is null)return Task.CompletedTask;_flush=true;return _worker;}}
    private async Task Drain()
    {
        while(true)
        {
            AppState? next=null;
            lock(_gate)
            {
                if(_pending is null){_worker=null;_flush=false;return;}
                long now=Stopwatch.GetTimestamp();
                // Settle quick edits, but checkpoint at least once per second during a long burst.
                if(_flush||Stopwatch.GetElapsedTime(_last,now).TotalMilliseconds>=120||Stopwatch.GetElapsedTime(_first,now).TotalSeconds>=1)
                {next=_pending;_pending=null;}
            }
            if(next is null){await Task.Delay(20).ConfigureAwait(false);continue;}
            try{await write(next).ConfigureAwait(false);Interlocked.Increment(ref _writes);Volatile.Write(ref _failed,0);}
            catch(Exception e)when(e is IOException or UnauthorizedAccessException){Volatile.Write(ref _failed,1);}
        }
    }
}
