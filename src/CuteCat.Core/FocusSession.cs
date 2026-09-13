namespace CuteCat.Core;

public enum SessionStatus { Ready, Running, Paused, Completed }
public sealed record SessionSnapshot(int Minutes=25,double Elapsed=0,SessionStatus Status=SessionStatus.Ready,bool IsBreak=false,SessionSnapshot? ReturnToFocus=null);
public sealed record FocusRecord(DateTimeOffset Finished,int Seconds);

public sealed class FocusSession
{
    public int Minutes { get; private set; }=25;
    public double Elapsed { get; private set; }
    public SessionStatus Status { get; private set; }
    public bool IsBreak { get; private set; }
    public SessionSnapshot? InterruptedFocus { get; private set; }
    public bool CanReturnToFocus=>IsBreak&&InterruptedFocus is not null;
    public double Remaining => Math.Max(0,Minutes*60-Elapsed);
    private double _last;
    public void Start(int minutes,double now,bool isBreak=false)
    {
        if(minutes<5 || minutes>180) throw new ArgumentOutOfRangeException(nameof(minutes));
        if(Status==SessionStatus.Running || Status==SessionStatus.Paused) throw new InvalidOperationException("End the current session first.");
        Minutes=minutes;Elapsed=0;IsBreak=isBreak;Status=SessionStatus.Running;_last=now;InterruptedFocus=null;
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
    public void End() { Status=SessionStatus.Ready;Elapsed=0;IsBreak=false;InterruptedFocus=null; }
    public void BeginBreak(double now)
    {
        if(IsBreak&&Status is SessionStatus.Running or SessionStatus.Paused)return;
        SessionSnapshot? paused=IsBreak?InterruptedFocus:null;
        if(!IsBreak&&Status is SessionStatus.Running or SessionStatus.Paused){Pause(now);if(Status==SessionStatus.Paused)paused=Snapshot() with{ReturnToFocus=null};}
        End();Start(5,now,true);InterruptedFocus=paused;
    }
    public bool ReturnToFocus(double now)
    {
        if(!CanReturnToFocus)return false;
        var saved=InterruptedFocus!;Restore(saved,now);Resume(now);return true;
    }
    public SessionSnapshot Snapshot()=>new(Minutes,Elapsed,Status,IsBreak,InterruptedFocus);
    public void Restore(SessionSnapshot value,double now)
    {
        Minutes=Math.Clamp(value.Minutes,5,180);Elapsed=double.IsFinite(value.Elapsed)?Math.Clamp(value.Elapsed,0,Minutes*60):0;
        IsBreak=value.IsBreak;
        Status=value.Status is SessionStatus.Running or SessionStatus.Paused ? SessionStatus.Paused : SessionStatus.Ready;
        InterruptedFocus=null;
        if(IsBreak&&value.ReturnToFocus is {IsBreak:false} previous&&previous.Status is SessionStatus.Running or SessionStatus.Paused&&double.IsFinite(previous.Elapsed))
        {
            int minutes=Math.Clamp(previous.Minutes,5,180);
            if(previous.Elapsed>=0&&previous.Elapsed<minutes*60)InterruptedFocus=new(minutes,previous.Elapsed,SessionStatus.Paused);
            if(InterruptedFocus is not null&&value.Status==SessionStatus.Completed)Status=SessionStatus.Completed;
        }
        _last=now;
    }
}
