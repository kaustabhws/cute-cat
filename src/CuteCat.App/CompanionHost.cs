using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using CuteCat.Core;
using Forms=System.Windows.Forms;

namespace CuteCat.App;

public sealed class CompanionHost : IDisposable
{
    public Companion Cat { get; }
    public CatSurface Surface { get; }
    public FocusSession Session { get; }=new();
    public Preferences Settings { get; private set; }
    public StateStore Store { get; }
    public List<FocusRecord> History { get; }
    public NotificationAttempt Attempt { get; }=new();
    public PointerGesture Gesture { get; }=new();
    public PracticeWindow? Practice { get; private set; }
    public string NotificationStatus { get; private set; }="Paw helper is off";
    public string? SaveNotice { get; private set; }
    public event Action? Changed,OpenRequested;
    public event Action<double>? Frame;
    public List<double> Cadence { get; }=[];
    public bool RecordCadence { get; set; }
    public bool IsSuppressed=>_locked||_suspended||(_fullscreen&&!_appFullscreenOverride);
    public CompanionBrain Brain { get; }=new();
    public string AppGuardStatus { get; private set; }="App guard is off";
    public double GuardPausedUntil { get; private set; }
    public int AppCloseRequests { get; private set; }
    internal double? TestIdleSeconds { get; set; }
    internal IntPtr? TestForegroundWindow { get; set; }
    public PetMenu Menu { get; private set; }=null!;
    public string ShellScanStatus=>_shell.LastScan;
    internal double ShellScanMilliseconds=>_shell.LastScanMilliseconds;
    internal int ShellRootCount=>_shell.LastRootCount;
    public int ShellDismissals { get; private set; }
    public WindowsTestNotification TestNotification { get; }=new();
    public PawJourneyMetrics? LastJourney { get; private set; }
    public bool Visible { get; private set; }=true;
    private readonly ShellNotifications _shell=new();
    private readonly DesktopApps _apps;
    internal string AppScanStatus=>_apps.LastScan;
    private readonly FrameClock _clock;
    private readonly DispatcherTimer _housekeeping;
    private readonly DispatcherTimer _notificationTimer;
    private readonly NotificationStabilizer _stabilizer=new();
    private readonly Forms.NotifyIcon _tray;
    private Forms.Screen _screen;
    private int _scanBusy;
    private bool _locked,_suspended,_fullscreen,_disposed,_appFullscreenOverride,_focusEligibility;
    private double _angryUntil;
    private double _guardMessageUntil;
    private double _lastSave,_lastCadence;
    private string? _ignored;
    private V2 _returnPosition;
    private NotificationApproach? _approach;
    private Task _saveTask=Task.CompletedTask;
    private readonly bool _qa;

    public CompanionHost(string dataDir,bool qa=false)
    {
        _qa=qa;Store=new StateStore(dataDir);var state=Store.Load();Settings=state.Settings;History=state.History;Visible=Settings.CatVisible;
        _apps=new DesktopApps(()=>_qa&&TestForegroundWindow.HasValue?TestForegroundWindow.Value:Native.GetForegroundWindow());
        _screen=SelectedScreen();
        var a=_screen.WorkingArea;double scale=Settings.Size/CatRig.CharacterHeight*DpiFor(_screen);
        Cat=new(new Area(a.X,a.Y,a.Width,a.Height),scale);Cat.Tick(FrameClock.Now);
        Surface=new();Configure();
        Cat.MoveTo(new(a.Left+a.Width*Settings.ParkX,a.Top+a.Height*Settings.ParkY),FrameClock.Now);
        Session.Restore(state.Session,FrameClock.Now);
        Surface.PointerDown+=Down;Surface.PointerMove+=Move;Surface.PointerUp+=Up;
        Surface.CaptureLost+=LostCapture;Surface.ContextRequested+=point=>Menu.Show(point);
        Surface.DisplayChanged+=()=>Configure();
        _tray=new Forms.NotifyIcon{Text="Cute Cat · a little company",Visible=!qa,Icon=MakeIcon()};
        _tray.DoubleClick+=(_,_)=>OpenRequested?.Invoke();
        Menu=new PetMenu(this);
        _tray.MouseUp+=(_,e)=>{if(e.Button==Forms.MouseButtons.Right){var point=Forms.Cursor.Position;Menu.Show(new(point.X,point.Y));}};
        _clock=new(Application.Current.Dispatcher,OnFrame);
        _housekeeping=new DispatcherTimer(DispatcherPriority.Background){Interval=TimeSpan.FromMilliseconds(250)};
        _housekeeping.Tick+=Housekeeping;_housekeeping.Start();
        _notificationTimer=new DispatcherTimer(DispatcherPriority.Background){Interval=TimeSpan.FromMilliseconds(100)};
        _notificationTimer.Tick+=NotificationTick;_notificationTimer.Start();
        TestNotification.Activated+=()=>Application.Current.Dispatcher.BeginInvoke(new Action(()=>OpenRequested?.Invoke()));
        if(!qa) { SystemEvents.SessionSwitch+=OnSessionSwitch;SystemEvents.PowerModeChanged+=OnPower;SystemEvents.DisplaySettingsChanged+=OnDisplay; }
        Cat.SetVisible(Visible,FrameClock.Now);Surface.Paint(Cat);Surface.Show(Visible);
    }
    public static double DpiFor(Forms.Screen screen)
    {
        // WPF's main visual and the per-monitor companion agree once HWND creation has completed.
        var pt=new Native.Point(screen.Bounds.Left+1,screen.Bounds.Top+1);
        IntPtr monitor=MonitorFromPoint(pt,2);
        return GetDpiForMonitor(monitor,0,out uint x,out _)==0 ? x/96d : 1;
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr MonitorFromPoint(Native.Point point,uint flags);
    [System.Runtime.InteropServices.DllImport("shcore.dll")] private static extern int GetDpiForMonitor(IntPtr monitor,int type,out uint x,out uint y);
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr icon);
    private static Icon MakeIcon()
    {
        using var bitmap=new Bitmap(64,64);using(var g=Graphics.FromImage(bitmap))using(var painter=new CatPainter())
        {g.Clear(Color.Transparent);painter.DrawIcon(g,64);}
        IntPtr h=bitmap.GetHicon();using var borrowed=Icon.FromHandle(h);var icon=(Icon)borrowed.Clone();DestroyIcon(h);return icon;
    }
    private Forms.Screen SelectedScreen()=>Forms.Screen.AllScreens.FirstOrDefault(s=>s.DeviceName==Settings.Monitor)??Forms.Screen.PrimaryScreen??Forms.Screen.AllScreens[0];
    public void Configure()
    {
        var selected=SelectedScreen();var a=selected.WorkingArea;
        double scale=Settings.Size/CatRig.CharacterHeight*DpiFor(selected);
        bool reduced=Settings.ReducedMotion || (Settings.FollowWindowsMotion && !SystemParameters.ClientAreaAnimation);
        bool quiet=Settings.Quiet || Session.Status==SessionStatus.Running;
        bool focusing=Session.Status==SessionStatus.Running&&!Session.IsBreak;
        var area=new Area(a.X,a.Y,a.Width,a.Height);
        if(area!=Cat.WorkArea || Cat.Scale!=scale || Cat.Quiet!=quiet || Cat.ReducedMotion!=reduced||focusing!=_focusEligibility)CancelPaw();
        _focusEligibility=focusing;
        _screen=selected;Cat.Configure(area,scale,quiet,reduced,FrameClock.Now);
        Cat.Accessory=Settings.Accessory;Cat.AccessoryColor=Settings.AccessoryColor;Cat.Activity=Settings.Activity;
    }
    public void Update(Preferences preferences)
    {
        bool rulesChanged=Settings.AppGuard!=preferences.AppGuard||!Settings.AppRules.SequenceEqual(preferences.AppRules);
        bool cancel=Settings.Notifications!=preferences.Notifications||rulesChanged;
        if(cancel)CancelPaw();
        if(rulesChanged)_apps.Reset();
        Settings=AppStateSettings(preferences);Configure();Save();Changed?.Invoke();
    }
    private static Preferences AppStateSettings(Preferences preferences)=>StateStore.Normalize(new AppState{Settings=preferences}).Settings;
    public void ShowCat(bool visible)
    {
        CancelPaw();Visible=visible;Settings=Settings with{CatVisible=visible};Cat.SetVisible(visible&&!IsSuppressed,FrameClock.Now);Surface.Show(visible&&!IsSuppressed);Save();Changed?.Invoke();
    }
    public void Park() { CancelPaw();Cat.Park(FrameClock.Now);Save();Changed?.Invoke(); }
    public void Perform(CatAction action)
    {
        Brain.UserAction(action);CancelPaw();if(!Visible)ShowCat(true);Cat.Perform(action,FrameClock.Now);
        if(action==CatAction.Meow && Settings.Sounds)MeowSound.Play();
        Changed?.Invoke();
    }
    public void Down(V2 p) { Brain.UserAction(CatAction.Drag);CancelPaw();Gesture.Down(p,Cat.Position);Cat.Stop(FrameClock.Now); }
    public void Move(V2 p) { if(Gesture.Move(p,5*DpiFor(_screen)) is V2 at)Cat.MoveTo(at,FrameClock.Now,true); }
    public void Up(V2 p)
    {
        if(!Gesture.Pressed)return;Move(p);var action=Gesture.Up();Brain.UserAction(action);Cat.Perform(action,FrameClock.Now);
        if(action==CatAction.Meow && Settings.Sounds)MeowSound.Play();Save();Changed?.Invoke();
    }
    public void LostCapture() { if(!Gesture.Pressed)return;Gesture.Cancel();Cat.Perform(CatAction.Land,FrameClock.Now);CancelPaw(); }
    public void CancelPaw()
    {
        if(Attempt.Target is { } target)_ignored=target.Identity;
        bool active=Attempt.Target!=null;
        Attempt.Cancel();_shell.Cancel();
        _stabilizer.Reset();_approach=null;
        Cat.Angry=false;_appFullscreenOverride=false;
        if(active&&LastJourney is not null)LastJourney.Cancelled=true;
        if(active)Cat.Stop(FrameClock.Now);
    }
    public void OpenPanel()=>OpenRequested?.Invoke();
    public void PauseGuard(int minutes)
    {CancelPaw();GuardPausedUntil=minutes>0?FrameClock.Now+minutes*60:0;_apps.Reset();AppGuardStatus=minutes>0?"App guard paused for "+minutes+" minutes":"App guard resumed";Changed?.Invoke();}
    public void StartPractice(bool nearBottom=false)
    {
        CancelPaw();Practice?.Close();ShowCat(true);
        var practice=new PracticeWindow();Practice=practice;
        double dpi=DpiFor(_screen);var area=_screen.WorkingArea;
        practice.Left=(area.Right-390*dpi)/dpi;practice.Top=(area.Bottom-(nearBottom?practice.Height+6:340)*dpi)/dpi;
        practice.Closed+=(_,_)=>{if(Attempt.Target?.Practice==true && !practice.DismissedByPaw)CancelPaw();};
        practice.Show();practice.UpdateLayout();
        // Only this explicit practice action is authorized while the automatic helper is off.
        var target=practice.Target(FrameClock.Now,_shell.Epoch);if(target is not null)BeginPaw(target,FrameClock.Now);
    }
    public void WindowsTest()
    {
        bool sent=TestNotification.Send();
        NotificationStatus=sent?"Windows test sent":TestNotification.Result=="Elevated"?
            "Windows tests need Cute Cat to run without administrator rights":"Windows did not allow the test notification";
        Changed?.Invoke();
    }
    private void BeginPaw(NotificationTarget target,double now)
    {
        if(!Visible || IsSuppressed || Gesture.Pressed || Cat.ReducedMotion)return;
        var plan=NotificationApproach.Plan(Cat.Position,Cat.WorkArea,Cat.Scale,target.Button);
        if(plan is null){if(target.AppWindow)AppGuardStatus="That window is outside this cat's selected monitor or reach";else NotificationStatus="That banner is outside this cat's reach";return;}
        if(!Attempt.Begin(target,now))return;
        if(Cat.Hidden){Cat.SetVisible(true,now);Surface.Show(true);}
        Cat.Angry=target.AppWindow;
        Surface.Raise();
        _returnPosition=Cat.Position;
        _approach=plan;LastJourney=new PawJourneyMetrics{Started=now,Start=Cat.Position,Destination=plan.Destination,Previous=Cat.Position,Practice=target.Practice};
        Cat.Approach(plan.Destination,plan.Facing,now,plan.TravelSeconds);NotificationStatus=target.Practice?"Running to the practice card":"A little paw is on the way";
        if(target.AppWindow)AppGuardStatus="Distraction spotted · a paw is on the way";
    }
    private void OnFrame(double now)
    {
        if(_disposed)return;
        if(RecordCadence) { if(_lastCadence>0)Cadence.Add((now-_lastCadence)*1000);_lastCadence=now; }
        else _lastCadence=0;
        Cat.Tick(now);
        if(Attempt.Target is not null&&LastJourney is not null)
        {
            double step=(Cat.Position-LastJourney.Previous).Length;
            LastJourney.MaxFrameStep=Math.Max(LastJourney.MaxFrameStep,step);LastJourney.Previous=Cat.Position;LastJourney.Frames++;
            if(step>.01&&LastJourney.FirstMovement==0)LastJourney.FirstMovement=now;
        }
        if(!Cat.Hidden)
        {
            Surface.Paint(Cat);
            TickPaw(now);
        }
        Frame?.Invoke(now);_clock.FramesPerSecond=Cat.SuggestedFps;
    }
    private void TickPaw(double now)
    {
        if(Attempt.Target is not { } target)return;
        if(Attempt.Expired(now) || Gesture.Pressed || !Visible || IsSuppressed) { CancelPaw();return; }
        if(target.Practice)
        {
            var practiceNow=Practice?.Target(now,target.Epoch);
            if(practiceNow is null || !NotificationAttempt.Matches(target,practiceNow,now)) { CancelPaw();return; }
        }
        if(Attempt.Stage==PawStage.Approaching && Cat.Arrived)
        {
            if(_approach is null){CancelPaw();return;}
            Cat.ReachTo(_approach.PawEnd,now);Attempt.Reached(now);if(LastJourney is not null)LastJourney.Arrival=now;return;
        }
        if(Attempt.Stage!=PawStage.Reaching)return;
        var current=target.Practice?Practice?.Target(now,target.Epoch):target;
        if(current is null) { CancelPaw();return; }
        bool rendered=Surface.LastPaintSucceeded && Surface.PaintedPose.Reach>=.999 && (Surface.PaintedPaw-target.Button.Center).Length<=1.5;
        if(!Attempt.CanCommit(current,now,rendered,target.Practice||(target.AppWindow?Settings.AppGuard:Settings.Notifications)))return;
        if(LastJourney is not null){LastJourney.Contact=now;LastJourney.ContactError=(Surface.PaintedPaw-target.Button.Center).Length;}
        if(!Attempt.MarkCommitting())return;
        Cat.HoldPawContact();
        int generation=Attempt.Generation,epoch=_shell.Epoch;
        if(target.Practice)
        {
            Practice!.PawDismiss();FinishPaw(true,now);return;
        }
        if(target.AppWindow)
        {
            var settings=Settings;bool focusing=Session.Status==SessionStatus.Running&&!Session.IsBreak;
            _=Task.Run(()=>_apps.Close(target,settings,focusing,()=>epoch==_shell.Epoch&&FrameClock.Now>=GuardPausedUntil,()=>FrameClock.Now)).ContinueWith(result=>
                Application.Current.Dispatcher.BeginInvoke(new Action(()=>
                {
                    if(_disposed||Attempt.Generation!=generation)return;
                    var outcome=result.Status==TaskStatus.RanToCompletion?result.Result:AppCloseResult.Cancelled;
                    if(outcome!=AppCloseResult.Cancelled)AppCloseRequests++;
                    FinishPaw(outcome==AppCloseResult.Closed,FrameClock.Now);
                    AppGuardStatus=outcome switch{AppCloseResult.Closed=>"Window closed · back to your focus",AppCloseResult.Requested=>"Close requested · any save prompt is yours to answer",_=>"That window changed; left it alone"};Changed?.Invoke();
                    _guardMessageUntil=FrameClock.Now+8;
                })),TaskScheduler.Default);
            return;
        }
        _=Task.Run(()=>_shell.Invoke(target,epoch,()=>FrameClock.Now)).ContinueWith(result=>
            Application.Current.Dispatcher.BeginInvoke(new Action(()=>
            {if(!_disposed && Attempt.Generation==generation)FinishPaw(result.Status==TaskStatus.RanToCompletion&&result.Result,FrameClock.Now);})),TaskScheduler.Default);
    }
    private void FinishPaw(bool success,double now)
    {
        if(LastJourney is not null){LastJourney.Finished=now;LastJourney.Dismissed=success;}
        if(success&&Attempt.Target is {Practice:false,AppWindow:false})ShellDismissals++;
        _ignored=Attempt.Target?.Identity;Attempt.Cancel();_shell.Cancel();
        Cat.Angry=false;_appFullscreenOverride=false;
        Cat.Perform(CatAction.Meow,now);
        // Return after a visible contact hold via a bounded scheduled callback.
        var returnTo=_returnPosition;int generation=Attempt.Generation;
        var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(550)};
        timer.Tick+=(_,_)=>{timer.Stop();if(!_disposed && Attempt.Generation==generation && !Gesture.Pressed)Cat.ReturnTo(returnTo,FrameClock.Now);};timer.Start();
        NotificationStatus=success?"Paw tap complete":"That banner changed; left it alone";Changed?.Invoke();
    }
    private void Housekeeping(object? sender,EventArgs e)
    {
        if(_disposed)return;double now=FrameClock.Now;
        var sessionBefore=Session.Status;
        if(Session.Tick(now)) { History.Add(new(DateTimeOffset.Now,Session.Minutes*60));Cat.Perform(CatAction.Play,now);Save(); }
        if(sessionBefore!=Session.Status)Configure();
        if(!_qa)CheckFullscreen();
        bool suppressed=IsSuppressed || !Visible;
        if(Cat.Hidden!=suppressed) { CancelPaw();Cat.SetVisible(!suppressed,now);Surface.Show(!suppressed); }
        bool busy=Attempt.Target is not null||Gesture.Pressed||Menu.IsOpen||suppressed;
        var idle=Brain.Observe(TestIdleSeconds??(_qa?0:DesktopApps.IdleSeconds()),Settings.IdleNaps,Settings.IdleMinutes*60,busy);
        if(idle==IdleTransition.Wake){CancelPaw();Cat.Perform(CatAction.Wake,now);}
        else if(!busy&&(idle==IdleTransition.Sleep||Brain.AutoSleeping&&Cat.Action!=CatAction.Sleep))
        {CancelPaw();Cat.Perform(CatAction.Sleep,now);}
        if(Attempt.Target?.AppWindow!=true&&now>_angryUntil)Cat.Angry=false;
        _clock.FramesPerSecond=Cat.SuggestedFps;
        if(now-_lastSave>15 && Session.Status==SessionStatus.Running)Save();
        Changed?.Invoke();
    }

    private async void NotificationTick(object? sender,EventArgs e)
    {
        if(_disposed)return;double now=FrameClock.Now;
        if(Attempt.Target?.Practice==true) { Changed?.Invoke();return; }
        if(_locked||_suspended||!Visible||Cat.ReducedMotion||Menu.IsOpen||(!Settings.Notifications&&!Settings.AppGuard))
        { if(Attempt.Target is not null)CancelPaw();NotificationStatus=!Settings.Notifications?"Paw helper is off":"Paw helper is resting";AppGuardStatus=!Settings.AppGuard?"App guard is off":"App guard is resting";Changed?.Invoke();return; }
        if(Attempt.Stage==PawStage.Committing)return;
        if(Interlocked.CompareExchange(ref _scanBusy,1,0)!=0)return;
        int epoch=_shell.Epoch;
        try
        {
            var preferences=Settings;bool focusing=Session.Status==SessionStatus.Running&&!Session.IsBreak;
            AppCloseCandidate? app=Settings.AppGuard?await Task.Run(()=>_apps.Find(preferences,focusing,now,GuardPausedUntil,epoch)):null;
            if(_disposed || epoch!=_shell.Epoch)return;
            if(app?.Rule.Action==AppRuleAction.Remind)
            {
                if(Attempt.Target is not null)CancelPaw();
                _apps.Acknowledge(app.Target);Cat.Perform(CatAction.Meow,FrameClock.Now);Cat.Angry=true;_angryUntil=FrameClock.Now+2.2;
                AppGuardStatus="A little reminder · this app is on your distraction list";return;
            }
            _appFullscreenOverride=app is not null;
            NotificationTarget? found=app?.Target;
            if(found is null&&Settings.Notifications&&!IsSuppressed)found=await Task.Run(()=>_shell.Find(now,epoch));
            if(_disposed||epoch!=_shell.Epoch)return;
            if(Attempt.Target is { } previous && (found is null || !NotificationAttempt.Matches(previous,found,FrameClock.Now)))
            {
                bool movedApp=previous.AppWindow&&found is {AppWindow:true}&&previous.Identity==found.Identity;
                CancelPaw();if(movedApp)_ignored=null;return;
            }
            if(found is null)_ignored=null;
            var ready=_stabilizer.Observe(found,FrameClock.Now);
            if(Attempt.Target is null && ready is not null && ready.Identity!=_ignored)BeginPaw(ready,FrameClock.Now);
            if(Attempt.Target is null)NotificationStatus=_shell.LastScan=="Unavailable"?"Windows notification controls are unavailable":"Watching for supported Windows banners";
            if(app is null&&Attempt.Target?.AppWindow!=true&&now>_angryUntil&&now>_guardMessageUntil)
                AppGuardStatus=!Settings.AppGuard?"App guard is off":now<GuardPausedUntil?"App guard paused · resume whenever you're ready":
                    _apps.LastScan=="NoCaption"?"This app does not expose a supported close control":"Watching your selected apps";
        }
        finally { Interlocked.Exchange(ref _scanBusy,0); }
        Changed?.Invoke();
    }
    private void CheckFullscreen()
    {
        IntPtr foreground=Native.GetForegroundWindow();
        if(foreground==IntPtr.Zero || foreground==Surface.Handle)return;
        Native.GetWindowThreadProcessId(foreground,out uint pid);
        if(pid==Environment.ProcessId) { _fullscreen=false;return; }
        var cls=new System.Text.StringBuilder(128);Native.GetClassName(foreground,cls,128);
        if(cls.ToString() is "Progman" or "WorkerW" or "Shell_TrayWnd") { _fullscreen=false;return; }
        if(Native.GetWindowRect(foreground,out var r))
        {
            var b=_screen.Bounds;
            _fullscreen=r.Left<=b.Left && r.Top<=b.Top && r.Right>=b.Right && r.Bottom>=b.Bottom;
        }
    }
    private void OnSessionSwitch(object sender,SessionSwitchEventArgs e)=>Application.Current.Dispatcher.BeginInvoke(new Action(()=>
    { _locked=e.Reason==SessionSwitchReason.SessionLock?true:e.Reason==SessionSwitchReason.SessionUnlock?false:_locked;
      if(_locked){CancelPaw();Session.Pause(FrameClock.Now);Gesture.Cancel();Save();} }));
    private void OnPower(object sender,PowerModeChangedEventArgs e)=>Application.Current.Dispatcher.BeginInvoke(new Action(()=>
    { if(e.Mode==PowerModes.Suspend){_suspended=true;CancelPaw();Session.Pause(FrameClock.Now);Save();} else if(e.Mode==PowerModes.Resume)_suspended=false; }));
    private void OnDisplay(object? sender,EventArgs e)=>Application.Current.Dispatcher.BeginInvoke(new Action(Configure));
    public void Save()
    {
        _lastSave=FrameClock.Now;
        if(Attempt.Target is null && !Gesture.Pressed)
            Settings=Settings with {ParkX=(Cat.Position.X-Cat.WorkArea.Left)/Cat.WorkArea.Width,ParkY=(Cat.Position.Y-Cat.WorkArea.Top)/Cat.WorkArea.Height};
        var snapshot=new AppState{Settings=Settings,Session=Session.Snapshot(),History=History.ToList()};
        _saveTask=SaveQuietly(snapshot);
    }
    private async Task SaveQuietly(AppState state)
    {
        try { await Store.SaveAsync(state).ConfigureAwait(false); }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException) { SaveNotice="Settings could not be saved. Check access to your local data folder."; }
    }
    public void Dispose()
    {
        if(_disposed)return;CancelPaw();Session.Pause(FrameClock.Now);Save();_disposed=true;
        _housekeeping.Stop();_notificationTimer.Stop();_clock.Dispose();Menu.Close();Practice?.Close();Surface.Dispose();
        _tray.Visible=false;_tray.Icon?.Dispose();_tray.Dispose();
        if(!_qa){SystemEvents.SessionSwitch-=OnSessionSwitch;SystemEvents.PowerModeChanged-=OnPower;SystemEvents.DisplaySettingsChanged-=OnDisplay;}
        _saveTask.GetAwaiter().GetResult();
    }
}
