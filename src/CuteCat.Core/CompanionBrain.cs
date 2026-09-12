namespace CuteCat.Core;

public enum PetAccessory { None, Bandana, BowTie, BellCollar, Flower }
public enum ActivityLevel { Calm, Balanced, Playful }
public enum AppRuleAction { CloseWindow, Remind }
public enum AppRuleScope { Always, DuringFocus }
public sealed record AppRule(string Path,string Name,AppRuleAction Action=AppRuleAction.CloseWindow,AppRuleScope Scope=AppRuleScope.Always,bool Enabled=true,int CloseDelaySeconds=0,int DailyAllowanceMinutes=0);
public enum IdleTransition { None, Sleep, Wake }

/// <summary>Only elapsed inactivity enters the brain. No input events or content.</summary>
public sealed class CompanionBrain
{
    public bool AutoSleeping { get; private set; }
    private bool _manualSleep;
    public bool WantsSleep=>_manualSleep||AutoSleeping;
    public void UserAction(CatAction action){AutoSleeping=false;_manualSleep=action==CatAction.Sleep;}
    public IdleTransition Observe(double idleSeconds,bool enabled,double thresholdSeconds,bool busy)
    {
        if(_manualSleep||busy)return IdleTransition.None;
        if(AutoSleeping&&(!enabled||idleSeconds<2)){AutoSleeping=false;return IdleTransition.Wake;}
        if(!AutoSleeping&&enabled&&double.IsFinite(idleSeconds)&&idleSeconds>=thresholdSeconds)
        {AutoSleeping=true;return IdleTransition.Sleep;}
        return IdleTransition.None;
    }
}

public static class AppRulePolicy
{
    public static AppRule? Match(IEnumerable<AppRule> rules,string executable,bool enabled,bool focusing,double now,double pausedUntil)
    {
        if(!enabled||now<pausedUntil||string.IsNullOrWhiteSpace(executable))return null;
        return rules.FirstOrDefault(r=>r.Enabled&&string.Equals(r.Path,executable,StringComparison.OrdinalIgnoreCase)&&
            (r.Scope==AppRuleScope.Always||focusing));
    }
}
