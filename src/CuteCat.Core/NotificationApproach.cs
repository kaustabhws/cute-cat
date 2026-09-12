namespace CuteCat.Core;

public sealed record NotificationApproach(V2 Destination, int Facing, V2 PawEnd, double TravelSeconds)
{
    public static NotificationApproach? Plan(V2 start, Area workArea, double scale, Area button)
    {
        if(!workArea.IsValid || !button.IsValid || !double.IsFinite(scale) || scale<=0 || !workArea.Contains(button.Center))return null;
        NotificationApproach? best=null;double bestDistance=double.PositiveInfinity;
        foreach(int facing in new[]{1,-1})
        {
            V2 ideal=button.Center-CatRig.ContactOffset(scale,facing);
            // The top of the canvas is transparent; 170 keeps the actual ears on
            // screen while making top-edge application caption buttons reachable.
            V2 goal=workArea.Clamp(ideal,158*scale,170*scale,20*scale);
            V2 paw=new(CatRig.Pivot.X+(button.Center.X-goal.X)/scale*facing,CatRig.Pivot.Y+(button.Center.Y-goal.Y)/scale);
            if(!IsReachable(paw))continue;
            double distance=(goal-start).Length;
            if(distance>=bestDistance)continue;
            bestDistance=distance;
            best=new(goal,facing,paw,Math.Clamp(distance/(650*scale),.35,1.55));
        }
        return best;
    }

    public static bool IsReachable(V2 paw)=>double.IsFinite(paw.X+paw.Y)&&paw.X>=224&&paw.X<=300&&paw.Y>=50&&paw.Y<=229;
}

/// <summary>Wait for the shell's entrance animation to settle before committing to a path.</summary>
public sealed class NotificationStabilizer
{
    private NotificationTarget? _candidate;
    private double _stableSince;
    public NotificationTarget? Observe(NotificationTarget? current,double now)
    {
        if(current is null){Reset();return null;}
        if(_candidate is null || !SameIdentity(_candidate,current))
        { _candidate=current with{SeenAt=now};_stableSince=now;return null; }
        var next=current with{SeenAt=_candidate.SeenAt};
        if(!NotificationAttempt.SameGeometry(_candidate.Button,next.Button,1))_stableSince=now;
        _candidate=next;
        if(now-next.SeenAt>8 || now-next.SeenAt<0){Reset();return null;}
        return now-_stableSince>=.12 ? next : null;
    }
    public void Reset(){_candidate=null;_stableSince=0;}
    public static bool SameIdentity(NotificationTarget a,NotificationTarget b)=>a.Identity==b.Identity&&a.Window==b.Window&&a.Process==b.Process&&a.Epoch==b.Epoch&&a.Practice==b.Practice&&a.AppWindow==b.AppWindow;
}
