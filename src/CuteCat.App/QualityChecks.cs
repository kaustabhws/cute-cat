using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CuteCat.Core;
using Color=System.Drawing.Color;
using System.Windows.Xps.Packaging;

namespace CuteCat.App;

public static class QualityChecks
{
    public static bool Failed {get;private set;}
    private static readonly List<object> Checks=[];
    private static void Check(string name,bool success,object? details=null)
    {Checks.Add(new{name,success,details});if(!success)Failed=true;}
    // Explicit developer observation of an opted-in live helper. Bounded; no message
    // text, accessibility names, window titles, process paths or screenshots.
    public static async Task ObserveShell(CompanionHost host,string dir)
    {
        Directory.CreateDirectory(dir);double start=FrameClock.Now;int initial=host.ShellDismissals;
        using var output=new StreamWriter(Path.Combine(dir,"notification-observation.jsonl")){AutoFlush=true};
        string previous="";
        while(FrameClock.Now-start<120)
        {
            string snapshot=JsonSerializer.Serialize(new{version=BuildInfo.Version,uiAccess=RuntimeAccess.HasUiAccess,
                status=host.ShellScanStatus,stage=host.Attempt.Stage.ToString(),suppressed=host.IsSuppressed,
                roots=host.ShellRootCount,button=host.Attempt.Target?.Button,dismissals=host.ShellDismissals-initial,
                journey=host.LastJourney});
            if(snapshot!=previous){await output.WriteLineAsync("{\"seconds\":"+(FrameClock.Now-start).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"state\":"+snapshot+"}");previous=snapshot;}
            if(host.ShellDismissals>initial)break;
            await Task.Delay(100);
        }
    }
    public static async Task ShellCheck(CompanionHost host,string dir)
    {
        Directory.CreateDirectory(dir);host.Update(host.Settings with{Notifications=true,Quiet=true});host.ShowCat(true);
        host.Cat.MoveTo(new(host.Cat.WorkArea.Left+220*host.Cat.Scale,host.Cat.WorkArea.Top+270*host.Cat.Scale),FrameClock.Now);
        await Task.Delay(800);host.WindowsTest();
        double begin=FrameClock.Now;var states=new HashSet<string>();bool targeted=false;
        while(host.TestNotification.Sent&&FrameClock.Now-begin<12)
        {states.Add(host.ShellScanStatus);targeted|=host.Attempt.Target?.Practice==false;if(host.ShellDismissals>0)break;await Task.Delay(50);}
        await Task.Delay(250);Native.SHQueryUserNotificationState(out int notificationState);
        File.WriteAllText(Path.Combine(dir,"shell-check.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,os=Environment.OSVersion.ToString(),
            elevated=WindowsTestNotification.IsElevated,emittedWindowsTest=host.TestNotification.Sent,testResult=host.TestNotification.Result,
            scanStates=states,supportedBannerTargeted=targeted,confirmedDismissal=host.ShellDismissals>0&&host.TestNotification.DismissedByClose,
            notificationState,journey=host.LastJourney,suppressed=host.IsSuppressed,
            note="Uses a real WinRT app toast. UserCanceled plus UIA disappearance confirms dismissal. No notification message text or screenshots are collected."},new JsonSerializerOptions{WriteIndented=true}));
        host.Update(host.Settings with{Notifications=false});
    }
    public static async Task Benchmark(CompanionHost host,string dir,string? mode=null)
    {
        Directory.CreateDirectory(dir);var rows=new List<object>();
        using var process=Process.GetCurrentProcess();
        foreach(string workload in mode=="idle"?new[]{"Idle"}:mode=="active"?new[]{"Walk and run"}:new[]{"Idle","Walk and run"})
        {
            host.Update(host.Settings with{Quiet=true});host.Park();await Task.Delay(1500);
            if(workload!="Idle")host.Perform(CatAction.Walk);
            process.Refresh();var cpu=process.TotalProcessorTime;double begin=FrameClock.Now;
            long paints=host.Surface.PaintCount;int lap=0;
            while(FrameClock.Now-begin<300)
            {
                if(workload!="Idle" && host.Cat.Action==CatAction.Idle)host.Perform(lap++%2==0?CatAction.Run:CatAction.Walk);
                await Task.Delay(50);
            }
            process.Refresh();double elapsed=FrameClock.Now-begin;
            rows.Add(new{workload,durationSeconds=elapsed,cpuTotalMachinePercent=(process.TotalProcessorTime-cpu).TotalSeconds/elapsed/Environment.ProcessorCount*100,
                privateMiB=process.PrivateMemorySize64/1048576d,workingMiB=process.WorkingSet64/1048576d,paints=host.Surface.PaintCount-paints,
                surfaceBytes=host.Surface.Bytes,gdiHandles=Native.GetGuiResources(process.Handle,0)});
            File.WriteAllText(Path.Combine(dir,"performance.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,os=Environment.OSVersion.ToString(),logicalProcessors=Environment.ProcessorCount,rows},new JsonSerializerOptions{WriteIndented=true}));
        }
    }
    public static async Task Run(CompanionHost host,MainWindow window,string dir,string? appFixture=null)
    {
        Directory.CreateDirectory(dir);Directory.CreateDirectory(Path.Combine(dir,"ui"));
        Directory.CreateDirectory(Path.Combine(dir,"frames"));
        host.Session.End();host.History.Clear();host.Update(new Preferences{Theme="Light"});host.ShowCat(true);host.Park();
        ExportArt(dir);
        await Task.Delay(500);
        Check("native layered surface paints",host.Surface.LastPaintSucceeded&&host.Surface.PaintCount>0);
        Check("native cat is visible after a hidden launcher",Native.IsWindowVisible(host.Surface.Handle));
        Check("padding alpha is zero",host.Surface.AlphaAt(0,0)==0);
        Check("single native surface under 1 MiB",host.Surface.Bytes<1024*1024,host.Surface.Bytes);
        foreach(string theme in new[]{"Light","Dark"})
        {
            window.SetTheme(theme);
            foreach(string page in new[]{"Focus","Profiles","Your cat","App guard","Quiet desktop","Settings","Updates"})
            {window.Navigate(page);window.UpdateLayout();await Task.Delay(140);RenderWindow(window,Path.Combine(dir,"ui",theme+"-"+page.Replace(' ','-')+".png"));}
            host.Menu.Show(new(host.Cat.WorkArea.Right-350,host.Cat.WorkArea.Bottom-500));await Task.Delay(180);
            if(host.Menu.View is { } menu)RenderElement(menu,Path.Combine(dir,"ui",theme+"-pet-menu.png"));
            Check(theme+" modern context menu opens",host.Menu.IsOpen);
            if((host.Menu.View as ContextMenu)?.Items.OfType<MenuItem>().FirstOrDefault(i=>i.HasItems) is { } profiles)
            {
                profiles.IsSubmenuOpen=true;await Task.Delay(120);profiles.ApplyTemplate();
                Check(theme+" profile submenu opens",profiles.Template.FindName("PART_Popup",profiles) is System.Windows.Controls.Primitives.Popup{IsOpen:true});
                profiles.IsSubmenuOpen=false;
            }
            host.Menu.Close();
        }
        window.SetTheme("Light");window.Navigate("Focus");
        window.Width=780;window.Height=620;window.Navigate("Your cat");window.UpdateLayout();
        RenderWindow(window,Path.Combine(dir,"ui","Light-minimum-size.png"));
        window.Width=900;window.Height=700;window.Navigate("Focus");
        var buttons=FindButtons(window).ToList();
        var start=buttons.First(b=>b.Content?.ToString()=="Start focusing");start.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Check("native focus start button works",host.Session.Status==SessionStatus.Running);
        await Task.Delay(320);start.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Check("native pause button works",host.Session.Status==SessionStatus.Paused);
        buttons.First(b=>b.Content?.ToString()=="End session").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Check("native end button works",host.Session.Status==SessionStatus.Ready);
        var origin=host.Cat.Position;host.Down(origin);host.Up(origin);await Task.Delay(1750);
        Check("tap resumes walking or a required turn",host.Cat.IsTravelling&&host.Cat.Action is CatAction.Walk or CatAction.Turn);
        host.Down(host.Cat.Position);host.Move(host.Cat.Position+new V2(-35,-35));
        Check("drag pose active",host.Cat.Action==CatAction.Drag);
        host.Up(host.Cat.Position);await Task.Delay(900);
        Check("drop resumes walking or a required turn",host.Cat.IsTravelling&&host.Cat.Action is CatAction.Walk or CatAction.Turn,new{host.Cat.Action,host.Cat.IsTravelling,host.Cat.Quiet,host.Cat.AutonomyPaused,host.Cat.ReducedMotion});
        host.Down(host.Cat.Position);host.LostCapture();await Task.Delay(900);
        Check("lost capture recovers",!host.Gesture.Pressed&&host.Cat.IsTravelling&&host.Cat.Action is CatAction.Walk or CatAction.Turn,new{host.Cat.Action,host.Cat.IsTravelling,host.Cat.Quiet,host.Cat.AutonomyPaused,host.Cat.ReducedMotion});
        host.Update(host.Settings with{Quiet=true});host.Park();await Task.Delay(350);
        host.StartPractice();double deadline=FrameClock.Now+9;double contactError=double.NaN;
        while(host.Practice?.IsVisible==true&&FrameClock.Now<deadline)
        {
            if(host.Attempt.Target is { } t && host.Surface.PaintedPose.Reach>.999)
                contactError=(host.Surface.PaintedPaw-t.Button.Center).Length;
            await Task.Delay(10);
        }
        Check("practice dismissed only through paw pipeline",host.Practice?.DismissedByPaw==true,new{contactError});
        await Task.Delay(700);
        host.Update(host.Settings with{Size=160,Quiet=true});
        host.Cat.MoveTo(new(host.Cat.WorkArea.Left+220*host.Cat.Scale,host.Cat.WorkArea.Top+260*host.Cat.Scale),FrameClock.Now);
        var farStart=host.Cat.Position;host.StartPractice(true);var afterBegin=host.Cat.Position;
        deadline=FrameClock.Now+5;
        while(host.Practice?.IsVisible==true&&FrameClock.Now<deadline)await Task.Delay(10);
        Check("bottom-edge card dismissed from a distant position",host.Practice?.DismissedByPaw==true);
        Check("native notification approach starts without teleporting",farStart==afterBegin);
        Check("native urgent run reaches contact promptly",host.LastJourney is {Contact:>0} j&&j.Contact-j.Started<3.2,host.LastJourney);
        Check("native adaptive contact stays accurate",host.LastJourney?.ContactError<=1.5);
        await Task.Delay(650);host.Update(host.Settings with{Size=128});
        host.StartPractice();host.ShowCat(false);await Task.Delay(500);
        Check("hide cancels pending dismissal",host.Practice?.DismissedByPaw==false&&host.Attempt.Target==null);
        long paints=host.Surface.PaintCount;await Task.Delay(350);
        Check("hidden stops native painting",host.Surface.PaintCount==paints);
        host.Practice?.Close();host.ShowCat(true);host.StartPractice();host.Down(host.Cat.Position);await Task.Delay(300);
        Check("touch cancels pending dismissal",host.Practice?.DismissedByPaw==false&&host.Attempt.Target==null);
        host.LostCapture();host.Practice?.Close();
        host.StartPractice();host.Practice!.Left-=24;await Task.Delay(350);
        Check("moved practice control cancels approach",host.Attempt.Target==null&&!host.Practice.DismissedByPaw);host.Practice.Close();
        host.Update(host.Settings with{Notifications=true});host.StartPractice();host.Update(host.Settings with{Notifications=false});await Task.Delay(350);
        Check("disable invalidates pending approach",host.Attempt.Target==null&&host.Practice?.DismissedByPaw==false);host.Practice?.Close();
        host.Update(host.Settings with{ReducedMotion=true});await Task.Delay(1200);paints=host.Surface.PaintCount;await Task.Delay(1200);
        Check("reduced motion lowers frame rate",host.Surface.PaintCount-paints<=2);
        host.Update(host.Settings with{ReducedMotion=false});
        foreach(var action in Enum.GetValues<CatAction>())
        {
            host.Perform(action);await Task.Delay(350);
            Check(action+" paints through native surface",host.Surface.LastPaintSucceeded);
        }
        host.Perform(CatAction.Idle);
        var fixture=new NotificationControlFixture();fixture.Show();fixture.UpdateLayout();await Task.Delay(100);
        bool invoked=await fixture.InvokeThroughResolver();await Task.Delay(80);
        Check("UIA close resolver invokes the exact test control",invoked&&fixture.Invocations==1);
        Check("UIA resolver accepts a toast that is itself the root",await fixture.ResolverFindsControl(true));
        System.Windows.Automation.AutomationProperties.SetAutomationId(fixture.Context,"ToastCenterScrollViewer");
        Check("live toast scroll container is not mistaken for notification history",await fixture.ResolverFindsControl());
        System.Windows.Automation.AutomationProperties.SetAutomationId(fixture.Cross,"");
        Check("UIA resolves the Windows close-icon label fallback",await fixture.ResolverFindsControl());
        System.Windows.Automation.AutomationProperties.SetName(fixture.Cross,"More options");
        Check("UIA does not mistake the options button for close",!await fixture.ResolverFindsControl());
        System.Windows.Automation.AutomationProperties.SetAutomationId(fixture.Cross,"DismissButton");
        System.Windows.Automation.AutomationProperties.SetAutomationId(fixture.Context,"NotificationCenter");
        Check("UIA resolver excludes Notification Center history",!await fixture.ResolverFindsControl());
        System.Windows.Automation.AutomationProperties.SetAutomationId(fixture.Context,"NotificationCenterGrid");
        Check("UIA resolver excludes the observed Windows 11 NotificationCenterGrid",!await fixture.ResolverFindsControl());
        System.Windows.Automation.AutomationProperties.SetAutomationId(fixture.Context,"ToastCenterScrollViewer");
        System.Windows.Automation.AutomationProperties.SetAutomationId(fixture.Toast,"NormalToastView");
        fixture.Height=390;fixture.Cross.Margin=new Thickness(0,220,0,0);fixture.UpdateLayout();await Task.Delay(100);
        Check("Windows 11 flexible toast dismiss control below image is found",await fixture.ResolverFindsControl());
        System.Windows.Automation.AutomationProperties.SetAutomationId(fixture.Cross,"SettingsButton");
        Check("flexible toast settings control below image is rejected",!await fixture.ResolverFindsControl());
        Check("observed Snipping Tool dismiss geometry is accepted",ShellNotifications.ValidCloseGeometry(new Area(1445,582,455,422),new Area(1841,811,50,50),1.25,true));
        Check("flexible toast wide action button is rejected",!ShellNotifications.ValidCloseGeometry(new Area(1445,582,455,422),new Area(1465,944,415,40),1.25,true));
        Check("flexible toast outside dismiss button is rejected",!ShellNotifications.ValidCloseGeometry(new Area(1445,582,455,422),new Area(1841,1005,50,50),1.25,true));
        var falseTarget=new NotificationTarget("not-a-shell",new System.Windows.Interop.WindowInteropHelper(fixture).Handle.ToInt64(),Environment.ProcessId,new Area(0,0,32,32),FrameClock.Now,0);
        Check("production invoker rejects an app posing as a shell notification",!await Task.Run(()=>new ShellNotifications().Invoke(falseTarget,0,()=>FrameClock.Now)));
        fixture.Close();
        var turnPosition=host.Cat.Position;
        var turnYaw=new List<double>();var turnTimes=new List<double>();
        void ObserveTurn(double now)
        {
            if(host.Cat.IsTurning&&host.Surface.LastPaintSucceeded){turnYaw.Add(host.Surface.PaintedPose.BodyYaw);turnTimes.Add(now);}
        }
        host.Frame+=ObserveTurn;host.Perform(CatAction.Turn);await Task.Delay(1050);host.Frame-=ObserveTurn;
        Check("native turn paints a front view",turnYaw.Any(y=>Math.Abs(y)<.1));
        Check("native turn has no instantaneous mirror",turnYaw.Count>35&&turnYaw.Zip(turnYaw.Skip(1),(a,b)=>Math.Abs(a-b)).Max()<.15);
        Check("native turn keeps feet pivot planted",host.Cat.Position==turnPosition);
        double[] turnIntervals=turnTimes.Zip(turnTimes.Skip(1),(a,b)=>(b-a)*1000).Order().ToArray();
        double turnMean=turnIntervals.Average(),turnP95=turnIntervals[(int)((turnIntervals.Length-1)*.95)];
        Check("native turning cadence",turnMean<=20&&turnP95<=25,new{turnMean,turnP95});
        host.Update(host.Settings with{Quiet=false});
        var cadence=new List<object>();
        foreach(var action in new[]{CatAction.Walk,CatAction.Run})
        {
            host.Cat.MoveTo(new(host.Cat.WorkArea.Center.X,host.Cat.WorkArea.Bottom-20*host.Cat.Scale),FrameClock.Now);
            host.Perform(action);await Task.Delay(300);host.Cadence.Clear();host.RecordCadence=true;
            await Task.Delay(action==CatAction.Run?400:1100);host.RecordCadence=false;
            var values=host.Cadence.Order().ToArray();double mean=values.Average(),p95=values[(int)((values.Length-1)*.95)];
            cadence.Add(new{action=action.ToString(),count=values.Length,mean,p95});
            Check(action+" measured active cadence",values.Length>=15&&mean<=20&&p95<=25,new{mean,p95});
        }
        host.Perform(CatAction.Idle);host.TestIdleSeconds=90;host.Update(host.Settings with{IdleMinutes=1,IdleNaps=true});await Task.Delay(650);
        Check("native idle transition falls asleep",host.Brain.AutoSleeping&&host.Cat.Action==CatAction.Sleep);
        host.Surface.SaveFrame(Path.Combine(dir,"sleep-bubble.png"));
        host.TestIdleSeconds=0;await Task.Delay(450);Check("native returning input starts a wake stretch",host.Cat.Action==CatAction.Wake);
        await Task.Delay(1600);host.Update(host.Settings with{Accessory=PetAccessory.Bandana,AccessoryColor="Rose"});await Task.Delay(200);
        Check("accessory reaches native painter",host.Surface.PaintedPose.Accessory==PetAccessory.Bandana);
        ExportCustomizations(dir);
        if(appFixture is not null){await CheckAppGuard(host,window,dir,Path.GetFullPath(appFixture));await CheckGuardPolicies(host,dir,Path.GetFullPath(appFixture));}
        window.Hide();host.Update(host.Settings with{Quiet=true,AppGuard=false,AppRules=[]});host.Park();await Task.Delay(1000);
        using var process=Process.GetCurrentProcess();process.Refresh();var cpu=process.TotalProcessorTime;var begin=FrameClock.Now;
        await Task.Delay(5000);process.Refresh();
        var metrics=new{durationSeconds=FrameClock.Now-begin,cpuTotalMachinePercent=(process.TotalProcessorTime-cpu).TotalSeconds/(FrameClock.Now-begin)/Environment.ProcessorCount*100,
            privateMiB=process.PrivateMemorySize64/1048576d,workingMiB=process.WorkingSet64/1048576d,surfaceBytes=host.Surface.Bytes,
            gdiHandles=Native.GetGuiResources(process.Handle,0),cadence};
        File.WriteAllText(Path.Combine(dir,"native-checks.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,elevated=WindowsTestNotification.IsElevated,os=Environment.OSVersion.ToString(),displayScale=CompanionHost.DpiFor(System.Windows.Forms.Screen.PrimaryScreen!),checks=Checks,metrics},new JsonSerializerOptions{WriteIndented=true}));
    }
    public static async Task Features(CompanionHost host,MainWindow window,string dir,string fixture)
    {
        Directory.CreateDirectory(dir);Directory.CreateDirectory(Path.Combine(dir,"ui"));
        host.Update(new Preferences{IdleNaps=false});host.ShowCat(true);host.Menu.Close();
        host.Cat.MoveTo(new(host.Cat.WorkArea.Center.X,host.Cat.WorkArea.Bottom-30),FrameClock.Now);
        var pointer=host.Cat.Position;host.Down(pointer);host.Move(pointer+new V2(-35,-35));host.Up(pointer+new V2(-35,-35));await Task.Delay(1000);
        Check("fresh native drop resumes movement",host.Cat.IsTravelling,new{host.Cat.Action,host.Cat.Quiet,host.Cat.AutonomyPaused,host.Cat.Position});
        host.Down(host.Cat.Position);host.LostCapture();await Task.Delay(1000);
        Check("fresh native capture loss resumes movement",host.Cat.IsTravelling,new{host.Cat.Action,host.Cat.Quiet,host.Cat.AutonomyPaused,host.Cat.Position});
        host.Update(new Preferences{Quiet=true,Theme="Dark",Accessory=PetAccessory.Bandana,AccessoryColor="Rose"});host.ShowCat(true);
        foreach(string theme in new[]{"Light","Dark"})
        {
            window.SetTheme(theme);
            host.Update(host.Settings with{AppGuard=false,AppRules=[new AppRule(fixture,"Example distraction")]});
            foreach(string page in new[]{"Profiles","Your cat","App guard","Settings","Updates"})
            {window.Navigate(page);window.UpdateLayout();await Task.Delay(150);RenderWindow(window,Path.Combine(dir,"ui",theme+"-"+page.Replace(' ','-')+".png"));}
            host.Menu.Show(new(host.Cat.WorkArea.Right-360,host.Cat.WorkArea.Bottom-550));await Task.Delay(250);
            if(host.Menu.View is { } menu)RenderElement(menu,Path.Combine(dir,"ui",theme+"-pet-menu.png"));host.Menu.Close();
        }
        await CheckAppGuard(host,window,dir,fixture);
        await CheckGuardPolicies(host,dir,fixture);
        File.WriteAllText(Path.Combine(dir,"feature-checks.json"),JsonSerializer.Serialize(new{checks=Checks},new JsonSerializerOptions{WriteIndented=true}));
    }
    private static async Task CheckAppGuard(CompanionHost host,MainWindow window,string dir,string fixture)
    {
        foreach(bool veto in new[]{false,true})
        {
            string folder=Path.Combine(dir,veto?"app-veto":"app-close");Directory.CreateDirectory(folder);
            host.Update(host.Settings with{AppGuard=true,Notifications=false,Quiet=true,AppRules=[new AppRule(fixture,"Test-owned distraction")]});
            host.Cat.MoveTo(new(host.Cat.WorkArea.Left+250,host.Cat.WorkArea.Bottom-30),FrameClock.Now);
            bool angry=false;void Observe(double _)=>angry|=host.Cat.Pose.Anger>.5;
            host.Frame+=Observe;
            using var process=Process.Start(new ProcessStartInfo(fixture){Arguments="--app-window \""+folder+"\""+(veto?" --veto":""),UseShellExecute=true})!;
            for(int i=0;i<30&&!File.Exists(Path.Combine(folder,"ready"));i++)await Task.Delay(100);
            IntPtr fixtureWindow=IntPtr.Zero;
            Native.EnumWindows((h,_)=>{Native.GetWindowThreadProcessId(h,out uint pid);if(pid==process.Id&&Native.IsWindowVisible(h))fixtureWindow=h;return true;},IntPtr.Zero);
            bool injected=Native.GetForegroundWindow()!=fixtureWindow;
            if(injected)host.TestForegroundWindow=fixtureWindow;
            var states=new HashSet<string>();
            double until=FrameClock.Now+9;
            while(FrameClock.Now<until&&!File.Exists(Path.Combine(folder,"close-requests.txt"))){states.Add(host.AppScanStatus+":"+host.AppGuardStatus+":"+host.Attempt.Stage);await Task.Delay(50);}
            await Task.Delay(900);host.Frame-=Observe;
            bool requested=File.Exists(Path.Combine(folder,"close-requests.txt"));
            File.WriteAllText(Path.Combine(folder,"observation.json"),JsonSerializer.Serialize(new{injectedForeground=injected,fixtureWindow=fixtureWindow.ToInt64(),states,journey=host.LastJourney}));
            Check(veto?"app save veto remains open":"selected app receives a normal close request",requested&&(veto?!process.HasExited:process.HasExited),new{injectedForeground=injected,states});
            Check("app guard shows angry expression",angry);
            Check("app guard paw contacts the caption button",host.LastJourney?.ContactError<=1.5,host.LastJourney);
            if(veto){await Task.Delay(1200);Check("save prompt is not clicked repeatedly",requested&&File.ReadAllText(Path.Combine(folder,"close-requests.txt"))=="1");}
            File.WriteAllText(Path.Combine(folder,"quit"),"");
            host.TestForegroundWindow=null;host.Update(host.Settings with{AppGuard=false,AppRules=[]});await Task.Delay(350);
        }
        window.Show();window.Activate();
    }
    private static void ExportCustomizations(string dir)
    {
        using var bitmap=new Bitmap(1000,650);using var g=Graphics.FromImage(bitmap);using var painter=new CatPainter();
        g.Clear(Color.FromArgb(248,247,240));g.FillRectangle(System.Drawing.Brushes.DarkSlateGray,0,325,1000,325);
        var accessories=Enum.GetValues<PetAccessory>();
        for(int row=0;row<2;row++)for(int i=0;i<accessories.Length;i++)
        {
            var pose=CatRig.Evaluate(row==0?CatAction.Idle:CatAction.Sleep,1.5,0,1) with{Accessory=accessories[i],AccessoryColor="Rose",HeadYaw=.7,BodyYaw=.7,TailYaw=.7};
            painter.Draw(g,pose,.65f,i*200,row*325+45);
            using var font=new Font("Segoe UI",12);g.DrawString(accessories[i].ToString(),font,row==0?System.Drawing.Brushes.DarkSlateGray:System.Drawing.Brushes.White,i*200+18,row*325+260);
        }
        bitmap.Save(Path.Combine(dir,"accessories-and-sleep.png"),ImageFormat.Png);
    }
    public static async Task Policies(CompanionHost host,string dir,string fixture)
    {
        Directory.CreateDirectory(dir);host.Update(new Preferences{IdleNaps=false});host.ShowCat(true);
        await CheckGuardPolicies(host,dir,fixture);
        File.WriteAllText(Path.Combine(dir,"policy-checks.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,checks=Checks},new JsonSerializerOptions{WriteIndented=true}));
    }
    private static async Task CheckGuardPolicies(CompanionHost host,string dir,string fixture)
    {
        string folder=Path.Combine(dir,"guard-policies");Directory.CreateDirectory(folder);
        host.Update(new Preferences{AppGuard=true,Quiet=true,IdleNaps=false,AppRules=[new(fixture,"Policy fixture",CloseDelaySeconds:3,DailyAllowanceMinutes:1)]});
        using var process=Process.Start(new ProcessStartInfo(fixture){Arguments="--app-window \""+folder+"\"",UseShellExecute=true})!;
        try
        {
            for(int i=0;i<40&&!File.Exists(Path.Combine(folder,"ready"));i++)await Task.Delay(100);
            IntPtr hwnd=IntPtr.Zero;Native.EnumWindows((h,_)=>{Native.GetWindowThreadProcessId(h,out uint pid);if(pid==process.Id&&Native.IsWindowVisible(h))hwnd=h;return true;},IntPtr.Zero);
            Check("policy fixture has a test-owned window",hwnd!=IntPtr.Zero);
            host.TestForegroundWindow=hwnd;await Task.Delay(800);
            Check("daily allowance postpones native action and counts selected foreground",host.Attempt.Target is null&&!process.HasExited&&host.UsedToday(fixture)>0);
            host.AllowApp(fixture,5);host.EditRules(rules=>rules.Select(r=>r with{DailyAllowanceMinutes=0}).ToList());await Task.Delay(500);
            Check("temporary exception postpones native action",host.Attempt.Target is null&&!process.HasExited);
            host.AllowApp(fixture,0);await Task.Delay(700);
            Check("revoked exception starts fresh grace",host.Attempt.Target is null&&!process.HasExited&&host.AppGuardStatus.Contains("paw in"));
            host.EditRules(rules=>rules.Select(r=>r with{CloseDelaySeconds=0}).ToList());
            for(int i=0;i<30&&host.Attempt.Target is null;i++)await Task.Delay(50);
            Check("eligible app begins the paw journey",host.Attempt.Target?.AppWindow==true);
            host.AllowApp(fixture,5);await Task.Delay(650);
            Check("granting exception cancels an in-flight close",host.Attempt.Target is null&&!process.HasExited&&!File.Exists(Path.Combine(folder,"close-requests.txt")));
            host.AllowApp(fixture,0);
            for(int i=0;i<30&&host.Attempt.Target is null;i++)await Task.Delay(50);
            host.SelectProfile("break");await Task.Delay(650);
            Check("switching to Break cancels an in-flight close",host.Attempt.Target is null&&!process.HasExited&&host.CurrentProfile.Id=="break");
            host.SelectProfile("work");double deadline=FrameClock.Now+9;
            while(FrameClock.Now<deadline&&!process.HasExited)await Task.Delay(50);
            Check("resuming eligible profile closes only the fixture",process.HasExited&&File.Exists(Path.Combine(folder,"close-requests.txt")));
            host.SelectProfile("study");host.EditRules(_=>[new(fixture,"Study fixture",Action:AppRuleAction.Remind)]);
            Check("profile rule editing keeps Work independent",host.Settings.Profiles.Single(p=>p.Id=="work").Rules.Single().Action==AppRuleAction.CloseWindow&&host.CurrentProfile.Rules.Single().Action==AppRuleAction.Remind);
            host.Cat.MoveTo(new(host.Cat.WorkArea.Center.X,host.Cat.WorkArea.Bottom-40),FrameClock.Now);host.RememberSpot();var saved=host.Settings.RestingSpots.Single();
            Check("explicit monitor spot is persisted in normalized coordinates",saved.X>=0&&saved.X<=1&&saved.Y>=0&&saved.Y<=1);
        }
        finally{host.TestForegroundWindow=null;host.Update(new Preferences{Quiet=true});File.WriteAllText(Path.Combine(folder,"quit"),"");}
    }
    private static void RenderElement(FrameworkElement element,string path)
    {
        Border? chrome=VisualTreeHelper.GetChildrenCount(element)>0?VisualTreeHelper.GetChild(element,0) as Border:null;
        var effect=chrome?.Effect;if(chrome is not null)chrome.Effect=null;
        element.UpdateLayout();var bitmap=new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth*1.25),(int)Math.Ceiling(element.ActualHeight*1.25),120,120,PixelFormats.Pbgra32);
        bitmap.Render(element);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var file=File.Create(path);encoder.Save(file);
        using var xps=new XpsDocument(Path.ChangeExtension(path,"xps"),FileAccess.Write);
        XpsDocument.CreateXpsDocumentWriter(xps).Write(element);
        if(chrome is not null)chrome.Effect=effect;
    }
    private static IEnumerable<Button> FindButtons(DependencyObject parent)
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++)
        {var child=VisualTreeHelper.GetChild(parent,i);if(child is Button b)yield return b;foreach(var nested in FindButtons(child))yield return nested;}
    }
    public static void RenderWindow(Window window,string path)
    {
        var content=(FrameworkElement)window.Content;content.UpdateLayout();
        var bitmap=new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth*1.25),(int)Math.Ceiling(content.ActualHeight*1.25),120,120,PixelFormats.Pbgra32);
        bitmap.Render(content);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var f=File.Create(path);encoder.Save(f);
        using var xps=new XpsDocument(Path.ChangeExtension(path,"xps"),FileAccess.Write);
        XpsDocument.CreateXpsDocumentWriter(xps).Write(content);
    }
    public static void ExportArt(string dir)
    {
        Directory.CreateDirectory(dir);
        using var painter=new CatPainter();
        var iconImages=new List<byte[]>();int[] iconSizes=[16,24,32,48,64,128,256];
        foreach(int size in iconSizes)
        {
            using var icon=new Bitmap(size,size);using(var g=Graphics.FromImage(icon)){g.Clear(Color.Transparent);painter.DrawIcon(g,size);}
            using var memory=new MemoryStream();icon.Save(memory,ImageFormat.Png);iconImages.Add(memory.ToArray());
            if(size==128)icon.Save(Path.Combine(dir,"icon.png"),ImageFormat.Png);
        }
        using(var iconFile=File.Create(Path.Combine(dir,"app.ico")))using(var writer=new BinaryWriter(iconFile))
        {
            writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)iconSizes.Length);int offset=6+16*iconSizes.Length;
            for(int i=0;i<iconSizes.Length;i++){writer.Write((byte)(iconSizes[i]==256?0:iconSizes[i]));writer.Write((byte)(iconSizes[i]==256?0:iconSizes[i]));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(iconImages[i].Length);writer.Write(offset);offset+=iconImages[i].Length;}
            foreach(var bytes in iconImages)writer.Write(bytes);
        }
        var actions=new[]{CatAction.Idle,CatAction.Walk,CatAction.Run,CatAction.Groom,CatAction.Sleep,CatAction.Meow,CatAction.Play,CatAction.Drag,CatAction.Paw};
        using var sheet=new Bitmap(960,768);using(var g=Graphics.FromImage(sheet))
        {
            for(int i=0;i<actions.Length;i++)
            {
                int x=i%3*320,y=i/3*256;
                using var bg=new SolidBrush(i%2==0?Color.FromArgb(247,246,239):Color.FromArgb(35,46,39));g.FillRectangle(bg,x,y,320,256);
                painter.Draw(g,CatRig.Evaluate(actions[i],.62,.18,1.1),.92f,x+6,y+3);
                using var font=new Font("Segoe UI",11);using var ink=new SolidBrush(i%2==0?Color.FromArgb(72,87,72):Color.FromArgb(220,230,214));g.DrawString(actions[i].ToString(),font,ink,x+17,y+12);
            }
        }
        sheet.Save(Path.Combine(dir,"character-sheet.png"),ImageFormat.Png);
        using var sizes=new Bitmap(960,460);using(var g=Graphics.FromImage(sizes))
        {
            g.Clear(Color.FromArgb(247,246,239));g.FillRectangle(new SolidBrush(Color.FromArgb(35,46,39)),0,230,960,230);
            for(int row=0;row<2;row++)for(int col=0;col<3;col++)
            {float scale=new[]{96,128,160}[col]/180f;painter.Draw(g,CatRig.Evaluate(CatAction.Idle,0,0,1),scale,col*320+30,row*230+10);}
        }
        sizes.Save(Path.Combine(dir,"desktop-sizes.png"),ImageFormat.Png);
        foreach(var action in actions)
        {
            string folder=Path.Combine(dir,"frames",action.ToString());Directory.CreateDirectory(folder);
            using var frame=new Bitmap(384,308);using var g=Graphics.FromImage(frame);
            for(int i=0;i<100;i++)
            {
                g.Clear(Color.FromArgb(247,246,239));double time=i/50d;
                painter.Draw(g,CatRig.Evaluate(action,time,time*(action==CatAction.Run?2.8:1.25),time),1.2f);
                frame.Save(Path.Combine(folder,$"{i:000}.png"),ImageFormat.Png);
            }
        }
        ExportTurns(dir,painter);
    }

    private static void ExportTurns(string dir,CatPainter painter)
    {
        string folder=Path.Combine(dir,"frames","Turn");Directory.CreateDirectory(folder);
        var cat=new Companion(new Area(0,0,1200,800));cat.Tick(0);
        using var frame=new Bitmap(384,308);using var g=Graphics.FromImage(frame);
        using var sheet=new Bitmap(1280,512);using var sg=Graphics.FromImage(sheet);
        sg.Clear(Color.FromArgb(247,246,239));int cell=0;
        int[] sampleFrames=[12,19,27,33,39,46,51,57];
        for(int i=0;i<180;i++)
        {
            double time=i/50d;
            if(i==12 || i==95)cat.Perform(CatAction.Turn,time);
            cat.Tick(time);
            g.Clear(Color.FromArgb(247,246,239));painter.Draw(g,cat.Pose,1.2f);
            frame.Save(Path.Combine(folder,$"{i:000}.png"),ImageFormat.Png);
            if(sampleFrames.Contains(i))
            {
                int x=cell%4*320,y=cell/4*256;
                painter.Draw(sg,cat.Pose,.92f,x+6,y+3);
                using var font=new Font("Segoe UI",10);using var ink=new SolidBrush(Color.FromArgb(74,70,58));
                sg.DrawString($"{Math.Max(0,time-.24):0.00}s",font,ink,x+15,y+12);cell++;
            }
        }
        sheet.Save(Path.Combine(dir,"turn-sequence.png"),ImageFormat.Png);
    }
}
