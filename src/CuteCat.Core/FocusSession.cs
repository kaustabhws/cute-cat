namespace CuteCat.Core;

public enum SessionStatus { Ready, Running, Paused, Completed }
public sealed record SessionSnapshot(int Minutes=25,double Elapsed=0,SessionStatus Status=SessionStatus.Ready,bool IsBreak=false);
public sealed record FocusRecord(DateTimeOffset Finished,int Seconds);

public sealed class FocusSession
{
    public int Minutes { get; private set; }=25;
    public double Elapsed { get; private set; }
    public SessionStatus Status { get; private set; }
    public bool IsBreak { get; private set; }
    public double Remaining => Math.Max(0,Minutes*60-Elapsed);
    private double _last;
    public void Start(int minutes,double now,bool isBreak=false)
    {
        if(minutes<5 || minutes>180) throw new ArgumentOutOfRangeException(nameof(minutes));
        if(Status==SessionStatus.Running || Status==SessionStatus.Paused) throw new InvalidOperationException("End the current session first.");
        Minutes=minutes;Elapsed=0;IsBreak=isBreak;Status=SessionStatus.Running;_last=now;
    }
    // Large unobserved gaps pause without awarding sleep/lock time.
    public bool Tick(double now)
    {
        if(Status!=SessionStatus.Running)return false;
        double delta=now-_last;
        if(delta<0)return false;
        _last=now;
        if(delta>5) { Status=SessionStatus.Paused;return false; }
        Elapsed=Math.Min(Minutes*60,Elapsed+delta);
        if(Elapsed>=Minutes*60) { Status=SessionStatus.Completed;return !IsBreak; }
        return false;
    }
    public void Pause(double now) { if(Status!=SessionStatus.Running)return; Tick(now); if(Status==SessionStatus.Running)Status=SessionStatus.Paused; }
    public void Resume(double now) { if(Status!=SessionStatus.Paused)return;Status=SessionStatus.Running;_last=now; }
    public void End() { Status=SessionStatus.Ready;Elapsed=0;IsBreak=false; }
    public SessionSnapshot Snapshot()=>new(Minutes,Elapsed,Status,IsBreak);
    public void Restore(SessionSnapshot value,double now)
    {
        Minutes=Math.Clamp(value.Minutes,5,180);Elapsed=double.IsFinite(value.Elapsed)?Math.Clamp(value.Elapsed,0,Minutes*60):0;
        IsBreak=value.IsBreak;
        Status=value.Status is SessionStatus.Running or SessionStatus.Paused ? SessionStatus.Paused : SessionStatus.Ready;
        _last=now;
    }
}
