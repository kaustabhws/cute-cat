namespace CuteCat.Core;

public sealed record ProfileSchedule(bool Enabled=false,int Days=31,int StartMinute=540,int EndMinute=1020,int Priority=0)
{
    public bool Includes(DateTime local)
    {
        if(!Enabled)return false;
        int minute=local.Hour*60+local.Minute;
        int day=((int)local.DayOfWeek+6)%7;
        bool HasDay(int value)=>(Days&(1<<((value+7)%7)))!=0;
        if(StartMinute==EndMinute)return HasDay(day);
        if(StartMinute<EndMinute)return HasDay(day)&&minute>=StartMinute&&minute<EndMinute;
        return minute>=StartMinute?HasDay(day):minute<EndMinute&&HasDay(day-1);
    }
}
public sealed record FocusProfile(string Id,string Name,List<AppRule> Rules,ActivityLevel Activity=ActivityLevel.Balanced,
    bool GuardEnabled=true,bool QuietDuringFocus=true,ProfileSchedule? Schedule=null);

public static class ProfilePolicy
{
    public static List<FocusProfile> Defaults(IEnumerable<AppRule> rules,ActivityLevel activity)=>
    [new("work","Work",rules.ToList(),activity),new("study","Study",[],ActivityLevel.Calm),new("break","Break",[],ActivityLevel.Playful,false,false)];
    public static FocusProfile Resolve(IReadOnlyList<FocusProfile> profiles,string selected,bool automatic,DateTime local)
    {
        if(profiles.Count==0)throw new ArgumentException("At least one profile is required.",nameof(profiles));
        var chosen=profiles.FirstOrDefault(p=>p.Id==selected)??profiles[0];
        if(!automatic)return chosen;
        return profiles.Select((p,i)=>(profile:p,index:i)).Where(p=>p.profile.Schedule?.Includes(local)==true)
            .OrderByDescending(p=>p.profile.Schedule!.Priority).ThenBy(p=>p.index).Select(p=>p.profile).FirstOrDefault()??chosen;
    }
}

public sealed record AppException(string Path,DateTimeOffset Until);
public sealed record AppUsage(string Path,DateOnly Day,double Seconds);

/// <summary>Aggregated selected-app foreground time. Never records titles or a browsing timeline.</summary>
public sealed class UsageLedger
{
    private readonly Dictionary<(string,DateOnly),double> _totals=[];
    private string? _previous;
    private DateOnly _day;
    private double _last=double.NaN;
    public bool Dirty { get; private set; }
    private static string Key(string path)=>path.ToUpperInvariant();
    public UsageLedger(IEnumerable<AppUsage>? saved=null)
    {
        foreach(var row in saved??[])if(double.IsFinite(row.Seconds)&&row.Seconds>=0)_totals[(Key(row.Path),row.Day)]=Math.Min(row.Seconds,86400);
    }
    public void Observe(string? selectedApp,DateOnly day,double now)
    {
        if(!double.IsFinite(now))return;
        double dt=now-_last;
        if(_previous is not null&&_day==day&&dt>=0&&dt<=5)
        {var key=(Key(_previous),day);_totals[key]=Math.Min(86400,_totals.GetValueOrDefault(key)+dt);Dirty|=dt>0;}
        _previous=selectedApp;_day=day;_last=now;
        foreach(var key in _totals.Keys.Where(k=>k.Item2<day.AddDays(-13)||k.Item2>day).ToArray())_totals.Remove(key);
    }
    public double Used(string path,DateOnly day)=>_totals.GetValueOrDefault((Key(path),day));
    public List<AppUsage> Snapshot()=>_totals.Select(p=>new AppUsage(p.Key.Item1,p.Key.Item2,p.Value)).ToList();
    public void Saved()=>Dirty=false;
    public void Clear(){_totals.Clear();_previous=null;_last=double.NaN;Dirty=true;}
}

public enum GuardPermission { Ready, TemporaryException, DailyAllowance, GracePeriod }
public sealed record GuardDecision(GuardPermission Permission,double RemainingSeconds=0)
{public bool CanAct=>Permission==GuardPermission.Ready;}

public sealed class GuardGate
{
    private string? _identity;
    private double _since;
    public void Reset(){_identity=null;_since=0;}
    public GuardDecision Evaluate(AppRule rule,string identity,IEnumerable<AppException> exceptions,double usedSeconds,DateTimeOffset utc,double now)
    {
        var allowed=exceptions.FirstOrDefault(e=>string.Equals(e.Path,rule.Path,StringComparison.OrdinalIgnoreCase)&&e.Until>utc);
        if(allowed is not null){Reset();return new(GuardPermission.TemporaryException,(allowed.Until-utc).TotalSeconds);}
        double remaining=rule.DailyAllowanceMinutes*60-Math.Max(0,usedSeconds);
        if(rule.DailyAllowanceMinutes>0&&remaining>0){Reset();return new(GuardPermission.DailyAllowance,remaining);}
        if(_identity!=identity){_identity=identity;_since=now;}
        if(now<_since){_since=now;}
        double delay=Math.Max(0,rule.CloseDelaySeconds-(now-_since));
        return delay>0?new(GuardPermission.GracePeriod,delay):new(GuardPermission.Ready);
    }
}
