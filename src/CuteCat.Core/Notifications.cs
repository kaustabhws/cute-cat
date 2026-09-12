namespace CuteCat.Core;

public sealed record NotificationTarget(string Identity, long Window, int Process, Area Button, double SeenAt, int Epoch, bool Practice=false,bool AppWindow=false);
public enum PawStage { None, Approaching, Reaching, Committing }

// Only identity and geometry enter the core. Text and screenshots are never collected.
public sealed class NotificationAttempt
{
    public NotificationTarget? Target { get; private set; }
    public PawStage Stage { get; private set; }
    public int Generation { get; private set; }
    public double ReachStarted { get; private set; }
    public bool Begin(NotificationTarget candidate,double now)
    {
        if (Target!=null || !candidate.Button.IsValid || now<candidate.SeenAt || now-candidate.SeenAt>8) return false;
        Target=candidate; Stage=PawStage.Approaching; Generation++; return true;
    }
    public void Reached(double now) { if (Stage!=PawStage.Approaching) return; Stage=PawStage.Reaching; ReachStarted=now; }
    public bool CanCommit(NotificationTarget current,double now,bool painted,bool enabled)
    {
        if (!enabled || !painted || Stage!=PawStage.Reaching || now-ReachStarted<.48 || now-ReachStarted>.82 || Target is not { } t) return false;
        return Matches(t,current,now);
    }
    public bool MarkCommitting() { if(Stage!=PawStage.Reaching)return false; Stage=PawStage.Committing;return true; }
    public bool Expired(double now) => Target is { } t && (now-t.SeenAt>8 || now<t.SeenAt);
    public void Cancel() { Target=null;Stage=PawStage.None;Generation++; }
    public static bool Matches(NotificationTarget a,NotificationTarget b,double now) =>
        now>=a.SeenAt && now-a.SeenAt<=8 && a.Identity==b.Identity && a.Window==b.Window && a.Process==b.Process &&
        a.Epoch==b.Epoch && a.Practice==b.Practice && a.AppWindow==b.AppWindow && a.Button.IsValid && b.Button.IsValid &&
        SameGeometry(a.Button,b.Button,2);
    public static bool SameGeometry(Area a,Area b,double tolerance)=>
        Math.Abs(a.Left-b.Left)<=tolerance&&Math.Abs(a.Top-b.Top)<=tolerance&&Math.Abs(a.Width-b.Width)<=tolerance&&Math.Abs(a.Height-b.Height)<=tolerance;
}

public sealed class PointerGesture
{
    public bool Pressed { get; private set; }
    public bool Dragging { get; private set; }
    private V2 _start,_origin;
    public void Down(V2 pointer,V2 cat) { Pressed=true;Dragging=false;_start=pointer;_origin=cat; }
    public V2? Move(V2 pointer,double threshold)
    {
        if(!Pressed) return null;
        if((pointer-_start).Length>=threshold) Dragging=true;
        return Dragging ? _origin+pointer-_start : null;
    }
    public CatAction Up() { CatAction a=Dragging?CatAction.Land:CatAction.Meow; Pressed=false;Dragging=false;return a; }
    public void Cancel() { Pressed=false;Dragging=false; }
}
