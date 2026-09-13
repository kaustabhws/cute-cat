using CuteCat.Core;

namespace CuteCat.App;

public sealed partial class CompanionHost
{
    public string? StartupNotice { get; private set; }
    public void InitializeStartup()
    {
        if(IsTest||Store.ReadOnly)return;
        // The user requested startup-on as the new default. Apply once; subsequent explicit opt-outs survive upgrades.
        bool enabled=!Settings.StartupInitialized||Settings.Startup;
        try
        {
            StartupRegistration.Apply(enabled);StartupNotice=null;
            if(!Settings.StartupInitialized)Update(Settings with{Startup=enabled,StartupInitialized=true});
        }
        catch(Exception e)when(e is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        {StartupNotice="Windows could not update startup. You can try again in Settings.";}
    }
    public PetAppearance CurrentAppearance=>OutfitPolicy.Resolve(Settings,CurrentProfile);
    public GraceCountdown? Countdown { get; private set; }
    private CompanionHint? _hint;
    internal CompanionHint? Hint=>_hint;
    private readonly BreakCueScheduler _breakCues=new();
    private double _breakCueUntil;
    private bool _returnCueShown;
    public void ChangeAppearance(PetAppearance appearance)=>Update(OutfitPolicy.Edit(Settings,CurrentProfile.Id,appearance));
    public void TestMenu()=>RequestMenu(Cat.Position);
    public void SaveOutfit(string name)
    {
        if(Settings.Outfits.Count>=24)return;
        Update(Settings with{Outfits=Settings.Outfits.Append(new SavedOutfit(Guid.NewGuid().ToString("N"),OutfitPolicy.Name(name),CurrentAppearance)).ToList()});
    }
    public void RemoveOutfit(string id)=>Update(OutfitPolicy.Remove(Settings,id));
    public void WearOutfit(string id)
    {if(Settings.Outfits.FirstOrDefault(o=>o.Id==id) is { } outfit)ChangeAppearance(outfit.Appearance);}
    private void ClearCountdown(){if(Countdown is null)return;Countdown=null;_hint?.Clear();}
    private void ShowCountdown(AppCloseCandidate app,GuardDecision decision,int epoch)
    {
        var shown=CountdownPolicy.Present(app.Rule,app.Target.Identity,epoch,decision,
            Settings.ShowGraceCountdown&&app.CanClose&&!app.Handled&&!IsSuppressed&&!Gesture.Pressed&&!Brain.WantsSleep&&Attempt.Target is null);
        if(shown is null){ClearCountdown();return;}
        DismissBreakCue();Countdown=shown;
        (_hint??=new()).Present((app.Rule.Action==AppRuleAction.CloseWindow?"A paw in ":"A reminder in ")+shown.Seconds+" s",
            app.Rule.Name+" is on your distraction list.","Allow 5 minutes",()=>AllowCountdown(shown));
        _hint.Follow(Cat,DpiFor(_screen));
    }
    internal bool AllowCountdown(GraceCountdown shown)
    {
        if(!CountdownPolicy.CanAllow(shown,Countdown)||shown.Epoch!=_shell.Epoch||CurrentApp is not { } app||
            app.Target.Identity!=shown.Identity||!Settings.AppGuard||!CurrentProfile.GuardEnabled||Menu.IsOpen||!Visible||IsSuppressed||
            (_qa&&TestForegroundWindow.HasValue?TestForegroundWindow.Value:Native.GetForegroundWindow()).ToInt64()!=app.Target.Window)return false;
        if(AppDecision(app,FrameClock.Now).Permission!=GuardPermission.GracePeriod){ClearCountdown();return false;}
        AllowApp(shown.Path,5);return true;
    }
    public void TakeBreak()
    {
        CancelPaw();Session.BeginBreak(FrameClock.Now);_returnCueShown=false;_breakCues.Reset();
        Configure();if(!Cat.Hidden&&!Cat.ReducedMotion)Cat.Perform(CatAction.Stretch,FrameClock.Now);
        Save();Changed?.Invoke();
    }
    public void BackToFocus()
    {
        CancelPaw();
        if(!Session.ReturnToFocus(FrameClock.Now)&&Session.IsBreak){Session.End();OpenPanel();}
        _returnCueShown=false;Configure();Save();Changed?.Invoke();
    }
    public void DismissBreakCue(){if(_breakCueUntil==0)return;_breakCueUntil=0;_hint?.Clear();}
    private void TickExtras(double now,double idle,bool busy)
    {
        if(busy||Brain.WantsSleep||Cat.ReducedMotion||Countdown is not null)DismissBreakCue();
        if(_breakCueUntil>0&&now>=_breakCueUntil)DismissBreakCue();
        bool free=!busy&&!Brain.WantsSleep&&!Cat.ReducedMotion&&Countdown is null&&_breakCueUntil==0&&Cat.Action is CatAction.Idle or CatAction.Walk;
        var cue=_breakCues.Tick(now,Settings.BreakCues,Settings.BreakCueMinutes,Session.Status==SessionStatus.Running&&!Session.IsBreak,idle<60&&!IsSuppressed,free);
        if(cue is { } kind)PresentBreakCue(kind,now);
        if(!Session.IsBreak)_returnCueShown=false;
        if(Settings.BreakCues&&Session.IsBreak&&Session.Status==SessionStatus.Completed&&!_returnCueShown&&free)
        {
            _returnCueShown=true;_breakCueUntil=now+20;
            (_hint??=new()).Present("Ready when you are.","Your break is over. Take your time.","Back to focus",BackToFocus,DismissBreakCue);
            _hint.Follow(Cat,DpiFor(_screen));
        }
    }
    internal void PresentBreakCue(BreakCueKind kind,double now)
    {
        if(Cat.Hidden||Gesture.Pressed||Attempt.Target is not null||Menu.IsOpen||Cat.ReducedMotion||Countdown is not null)return;
        _breakCueUntil=now+18;
        Cat.Perform(kind==BreakCueKind.Stretch?CatAction.Stretch:CatAction.Drink,now);
        (_hint??=new()).Present(kind==BreakCueKind.Stretch?"A little stretch?":"Time for a sip?",
            kind==BreakCueKind.Stretch?"Relax your shoulders. Your work can wait a moment.":"A little water for you. A little break for your eyes.","Take a break",TakeBreak,DismissBreakCue);
        _hint.Follow(Cat,DpiFor(_screen));
    }
    public void ImportSettings(Preferences reviewed)
    {
        // The review never changes registry startup or activates monitoring. Policy updates cancel every pending paw.
        var settings=reviewed with{AppGuard=false,Notifications=false,AutomaticProfiles=false,AvoidFocusedControls=false,Startup=Settings.Startup,StartupInitialized=Settings.StartupInitialized};
        Update(settings);ShowCat(settings.CatVisible);
    }
}
