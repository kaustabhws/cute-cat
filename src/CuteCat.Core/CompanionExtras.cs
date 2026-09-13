namespace CuteCat.Core;

public static class OutfitPolicy
{
    public static bool ValidId(string? id)=>id is {Length:>0 and <=64}&&id.All(c=>char.IsAsciiLetterOrDigit(c)||c is '-' or '_');
    public static string Name(string? name)
    {
        string clean=new((name??"").Where(c=>!char.IsControl(c)).ToArray());clean=clean.Trim();
        return clean.Length==0?"My outfit":clean[..Math.Min(32,clean.Length)];
    }
    public static PetAppearance Resolve(Preferences preferences,FocusProfile profile)=>
        preferences.Outfits.FirstOrDefault(o=>o.Id==profile.OutfitId)?.Appearance??preferences.Appearance??PetAppearance.Default;
    // Editing a linked look makes a personal copy. Saved outfits never change implicitly.
    public static Preferences Edit(Preferences preferences,string profileId,PetAppearance appearance)=>preferences with
    {Appearance=appearance.Normalize(),Profiles=preferences.Profiles.Select(p=>p.Id==profileId?p with{OutfitId=null}:p).ToList()};
    public static Preferences Remove(Preferences preferences,string id)=>preferences with
    {Outfits=preferences.Outfits.Where(o=>o.Id!=id).ToList(),Profiles=preferences.Profiles.Select(p=>p.OutfitId==id?p with{OutfitId=null}:p).ToList()};
}

public enum BreakCueKind { Stretch, Water }
/// <summary>Counts observed, attended focus time only. Busy moments defer a single cue; gaps never catch up.</summary>
public sealed class BreakCueScheduler
{
    private double _last=double.NaN,_elapsed;
    private bool _counting;
    private int _interval;
    private BreakCueKind _next;
    public void Reset(){_last=double.NaN;_elapsed=0;_counting=false;}
    public BreakCueKind? Tick(double now,bool enabled,int minutes,bool focusing,bool attended,bool canShow)
    {
        if(!double.IsFinite(now))return null;
        minutes=Math.Clamp(minutes,15,120);
        if(!enabled||!focusing||_interval!=minutes){Reset();_interval=minutes;}
        bool counting=enabled&&focusing&&attended;
        double delta=now-_last;_last=now;
        if(_counting&&counting&&delta is >=0 and <=5)_elapsed=Math.Min(minutes*60,_elapsed+delta);
        _counting=counting;
        if(!counting||!canShow||_elapsed<minutes*60)return null;
        _elapsed=0;var cue=_next;_next=_next==BreakCueKind.Stretch?BreakCueKind.Water:BreakCueKind.Stretch;return cue;
    }
}

// A countdown is presentation, never a second intervention timer.
public sealed record GraceCountdown(string Identity,string Path,int Epoch,int Seconds);
public static class CountdownPolicy
{
    public static GraceCountdown? Present(AppRule rule,string identity,int epoch,GuardDecision decision,bool eligible)=>
        eligible&&decision.Permission==GuardPermission.GracePeriod&&decision.RemainingSeconds>0?
            new(identity,rule.Path,epoch,(int)Math.Ceiling(decision.RemainingSeconds)):null;
    public static bool CanAllow(GraceCountdown shown,GraceCountdown? current)=>current is not null&&shown.Identity==current.Identity&&
        shown.Epoch==current.Epoch&&string.Equals(shown.Path,current.Path,StringComparison.OrdinalIgnoreCase);
}
