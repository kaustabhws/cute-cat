using CuteCat.Core;
using System.Text.Json;

int passed=0,failed=0;
void Check(string name,bool condition) {if(condition){passed++;Console.WriteLine("PASS "+name);}else{failed++;Console.WriteLine("FAIL "+name);}}
var s=new FocusSession();s.Start(5,0);for(int i=1;i<=40;i++)s.Tick(i);
Check("running counts elapsed seconds",s.Elapsed==40);
s.Pause(40);s.Tick(100);Check("paused time excluded",s.Elapsed==40);
s.Resume(100);s.Tick(101);Check("resume has no catch-up",s.Elapsed==41);
s.Tick(200);Check("unobserved gap pauses safely",s.Status==SessionStatus.Paused&&s.Elapsed==41);
s.Restore(new(25,63,SessionStatus.Running),500);Check("restart always pauses",s.Status==SessionStatus.Paused&&s.Elapsed==63);
s.End();s.Start(5,0);int completes=0;for(int i=1;i<=350;i++)if(s.Tick(i))completes++;
Check("completion is emitted once",completes==1&&s.Status==SessionStatus.Completed);
s.End();s.Start(5,0,true);for(int i=1;i<=301;i++)if(s.Tick(i))completes++;
Check("break earns no focus credit",completes==1);
bool invalid=false;try{s.End();s.Start(4,0);}catch(ArgumentOutOfRangeException){invalid=true;}Check("duration bounds enforced",invalid);
s.Restore(new(1000,double.NaN,SessionStatus.Running),0);Check("invalid recovery normalized",s.Minutes==180&&s.Elapsed==0);

var a=new Area(-1920,-200,1920,1080);var cat=new Companion(a,.8);cat.Tick(0);cat.MoveTo(new(a.Center.X,a.Bottom-16),0);cat.Perform(CatAction.Walk,0);
V2 start=cat.Position;for(int i=1;i<=60;i++)cat.Tick(i/60d);
Check("walk actually travels",(cat.Position-start).Length>20);
Check("gait advances with displacement",cat.Phase>0);
Check("negative monitor bounds respected",a.Contains(cat.Position));
cat.MoveTo(new(-9999,9999),1);Check("drag positions clamped",a.Contains(cat.Position));
cat.Perform(CatAction.Meow,2);for(int i=0;i<=96;i++)cat.Tick(2+i/60d);
Check("pet resumes walking or a necessary turn within two seconds",cat.IsTravelling&&cat.Action is CatAction.Walk or CatAction.Turn);
cat.Perform(CatAction.Land,4);for(int i=0;i<=54;i++)cat.Tick(4+i/60d);
Check("drop resumes walking or a necessary turn within one second",cat.IsTravelling&&cat.Action is CatAction.Walk or CatAction.Turn);
cat.Configure(a,.8,true,false,5);cat.Perform(CatAction.Meow,5);for(int i=0;i<600;i++)cat.Tick(5+i/60d);
Check("quiet suppresses autonomous movement",cat.Action==CatAction.Idle);
cat.Configure(a,.8,false,true,16);cat.Perform(CatAction.Walk,16);Check("reduced motion prevents walk",cat.Action==CatAction.Idle);
cat.SetVisible(false,17);start=cat.Position;cat.Tick(80);Check("hidden remains still and stops render clock",cat.Position==start&&cat.SuggestedFps==0);
cat.SetVisible(true,81);cat.Configure(a,.8,false,false,81);cat.Perform(CatAction.Groom,81);cat.Tick(81.5);var before=cat.Pose;
cat.Configure(a,.8,false,false,81.5);Check("idempotent settings preserve action",cat.Action==CatAction.Groom);
cat.Perform(CatAction.Sleep,81.5);cat.Tick(81.5);Check("state switch starts at previous pose",cat.Pose==before);
cat.Tick(81.66);Check("sleep transitions through intermediate pose",cat.Pose.Curl>0&&cat.Pose.Curl<1);
cat.Tick(82);Check("sleep closes eyes and curls",cat.Pose.Eyes==0&&cat.Pose.Curl==1);
for(double phase=0;phase<1;phase+=.05)
{if(!double.IsFinite(CatRig.Foot(phase,17,11).X))throw new Exception("Nonfinite foot");}
Check("gait cycle has no position seam",(CatRig.Foot(.999999,17,11)-CatRig.Foot(0,17,11)).Length<.001);
var mid=CatRig.Evaluate(CatAction.Paw,.6,0,0);Check("contact plateau reaches declared anchor",(CatRig.Paw(mid)-CatRig.Contact).Length<.001);
Check("left and right contact are symmetric",CatRig.ContactOffset(1,-1).X==-CatRig.ContactOffset(1,1).X);

var turner=new Companion(new Area(0,0,1200,800));turner.Tick(0);V2 planted=turner.Position;
turner.Perform(CatAction.Turn,0);
Check("turn starts without a direction snap",turner.Facing==1&&turner.Pose.BodyYaw==1&&turner.IsTurning);
double lastYaw=1,maxYawStep=0;bool frontSeen=false,monotonic=true;
for(int i=1;i<=52;i++)
{
    turner.Tick(i/60d);double yaw=turner.Pose.BodyYaw;
    maxYawStep=Math.Max(maxYawStep,Math.Abs(yaw-lastYaw));monotonic&=yaw<=lastYaw+.00001;
    frontSeen|=Math.Abs(yaw)<.08;lastYaw=yaw;
}
Check("turn passes through front view",frontSeen);
Check("turn is continuous and monotonic at 60 Hz",monotonic&&maxYawStep<.1);
Check("turn stays planted",turner.Position==planted&&turner.Velocity.Length==0);
Check("turn settles into opposite direction",turner.Facing==-1&&!turner.IsTurning&&turner.Pose.BodyYaw==-1&&turner.Pose.HeadYaw==-1&&turner.Pose.TailYaw==-1);
turner.Perform(CatAction.Paw,1);turner.Tick(1.6);
Check("left-facing paw uses actual projected endpoint",(CatRig.Paw(turner.Pose)-new V2(33,107)).Length<.01);

var transition=new TurnTransition();transition.Start(-1,0);transition.Advance(.23);
Check("head leads and tail follows body",transition.Current.Head<transition.Current.Body&&transition.Current.Tail>transition.Current.Body);
transition.Advance(.4);var orientation=transition.Current;transition.Cancel();transition.Advance(3);
Check("cancel preserves partial orientation",transition.Current==orientation&&!transition.Active);
transition.Start(1,3);Check("retarget starts at the displayed orientation",transition.Current==orientation);
transition.Advance(4);Check("retarget settles without old turn finishing later",transition.Current==new CatOrientation(1,1,1));

var edgeCat=new Companion(new Area(-900,0,900,600));edgeCat.Tick(0);edgeCat.MoveTo(new(-205,580),0);edgeCat.Perform(CatAction.Walk,0);
bool reachedEdge=false;
for(int i=1;i<=180;i++){edgeCat.Tick(i/60d);if(edgeCat.IsTurning){reachedEdge=Math.Abs(edgeCat.Position.X-(-165))<.01;break;}}
Check("right edge is approached before a planted turn",reachedEdge&&edgeCat.Facing==1);
edgeCat.Tick(4);edgeCat.MoveTo(new(-735,580),4);edgeCat.Perform(CatAction.Walk,4);
Check("left edge also turns through front view",edgeCat.IsTurning&&edgeCat.Pose.BodyYaw==-1);
edgeCat.Tick(4.4);double heldYaw=edgeCat.Pose.BodyYaw;edgeCat.Perform(CatAction.Drag,4.4);edgeCat.Tick(4.4);
Check("drag interrupts turn without mirroring",!edgeCat.IsTurning&&edgeCat.Pose.BodyYaw==heldYaw&&!edgeCat.IsTravelling);
edgeCat.SetVisible(false,4.5);edgeCat.Tick(8);
Check("hide clears queued turn and travel",!edgeCat.IsTurning&&!edgeCat.IsTravelling&&edgeCat.SuggestedFps==0);

var approach=new Companion(new Area(0,0,1400,800));approach.Tick(0);var closeAt=new V2(900,700);
approach.Approach(closeAt,1,0);
Check("notification approach waits for initial turn",approach.IsTurning&&!approach.Arrived);
for(int i=1;i<=400;i++)approach.Tick(i/60d);
Check("notification arrival includes final target-facing turn",approach.Arrived&&approach.Position==closeAt&&approach.Pose.BodyYaw==1);

var desktop=new Area(0,0,1920,1040);var bottomClose=new Area(1862,955,32,32);
double largeScale=160/180d*1.25;
var reachPlan=NotificationApproach.Plan(new(240,320),desktop,largeScale,bottomClose);
Check("bottom-edge banner gets an adaptive paw plan",reachPlan is not null&&reachPlan.PawEnd.Y>CatRig.Contact.Y);
Check("adaptive destination fits desktop",reachPlan is not null&&desktop.Contains(reachPlan.Destination));
if(reachPlan is not null)
{
    var sprint=new Companion(desktop,largeScale);sprint.Tick(0);sprint.MoveTo(new(240,320),0);V2 from=sprint.Position;
    sprint.Approach(reachPlan.Destination,reachPlan.Facing,0,reachPlan.TravelSeconds);
    Check("notification response never teleports at start",sprint.Position==from);
    double previousDistance=(sprint.Position-reachPlan.Destination).Length,maxStep=0;bool approached=true;V2 last=sprint.Position;
    for(int i=1;i<=180;i++)
    {
        sprint.Tick(i/60d);double distance=(sprint.Position-reachPlan.Destination).Length;
        approached&=distance<=previousDistance+.01;maxStep=Math.Max(maxStep,(sprint.Position-last).Length);
        previousDistance=distance;last=sprint.Position;
    }
    Check("far-away cat arrives within three seconds",sprint.Arrived&&sprint.Position==reachPlan.Destination);
    Check("urgent path is continuous and heads toward target",approached&&maxStep<60&&maxStep>0);
    sprint.ReachTo(reachPlan.PawEnd,3);sprint.Tick(3.6);
    V2 screenPaw=sprint.Position+(CatRig.Paw(sprint.Pose)-CatRig.Pivot)*largeScale;
    Check("adaptive paw contacts actual cross center",(screenPaw-bottomClose.Center).Length<.01);
}
bool allSizes=true;
foreach(double dpi in new[]{1,1.25,1.5,2,2.5})foreach(int size in new[]{96,128,160})
{
    double scale=size/180d*dpi;var area=new Area(-1920*dpi,0,1920*dpi,1040*dpi);
    var cross=new Area(area.Right-58*dpi,area.Bottom-85*dpi,32*dpi,32*dpi);
    var plan=NotificationApproach.Plan(new(area.Left+250*scale,area.Top+320*scale),area,scale,cross);
    allSizes&=plan is not null&&NotificationApproach.IsReachable(plan.PawEnd);
}
Check("notification plans cover every size and tested DPI tier",allSizes);
Check("off-monitor notification is not chased offscreen",NotificationApproach.Plan(default,desktop,1,new(2100,900,32,32)) is null);
var cancelSprint=new Companion(desktop);cancelSprint.Tick(0);cancelSprint.Approach(new(700,800),1,0);cancelSprint.Tick(.2);cancelSprint.Perform(CatAction.Drag,.2);
var held=cancelSprint.Position;cancelSprint.Tick(1);Check("drag cancels urgent travel",cancelSprint.Position==held&&!cancelSprint.IsTravelling);

var settling=new NotificationStabilizer();var entering=new NotificationTarget("entering",1,2,new(1910,950,32,32),0,1);
Check("new banner waits for entrance animation",settling.Observe(entering,0) is null);
Check("sliding banner resets stability timer",settling.Observe(entering with{Button=new(1860,950,32,32)},.1) is null);
Check("unstable banner is not committed",settling.Observe(entering with{Button=new(1860,950,32,32)},.18) is null);
var stable=settling.Observe(entering with{Button=new(1860,950,32,32)},.24);
Check("settled banner keeps original lifetime",stable is not null&&stable.SeenAt==0);
Check("disappearance clears stability",settling.Observe(null,.3) is null&&settling.Observe(entering,.4) is null);
Check("replacement cannot inherit a settled candidate",settling.Observe(entering with{Identity="replacement"},.6) is null);

var gesture=new PointerGesture();gesture.Down(new(0,0),new(100,100));Check("click threshold holds cat",gesture.Move(new(2,1),5)==null);
Check("click greets",gesture.Up()==CatAction.Meow);
gesture.Down(new(0,0),new(100,100));Check("drag preserves grab offset",gesture.Move(new(20,30),5)==new V2(120,130));
Check("drag releases through landing",gesture.Up()==CatAction.Land&&!gesture.Pressed);
gesture.Down(default,default);gesture.Cancel();Check("lost capture clears gesture",!gesture.Pressed&&!gesture.Dragging);

var target=new NotificationTarget("toast/close",12,34,new(100,100,24,24),0,1);
var attempt=new NotificationAttempt();Check("valid target accepted",attempt.Begin(target,0));
Check("second target cannot replace in-flight target",!attempt.Begin(target with{Identity="other"},.1));
attempt.Reached(1);Check("no dismissal before contact",!attempt.CanCommit(target,1.2,true,true));
Check("no dismissal until painted",!attempt.CanCommit(target,1.6,false,true));
Check("exact painted contact accepted",attempt.CanCommit(target,1.6,true,true));
Check("disabled helper blocks dismissal",!attempt.CanCommit(target,1.6,true,false));
Check("replacement identity rejected",!attempt.CanCommit(target with{Identity="new"},1.6,true,true));
Check("recycled window process rejected",!attempt.CanCommit(target with{Process=35},1.6,true,true));
Check("different HWND rejected",!attempt.CanCommit(target with{Window=20},1.6,true,true));
Check("moved close control rejected",!attempt.CanCommit(target with{Button=new(105,100,24,24)},1.6,true,true));
Check("cancelled epoch rejected",!attempt.CanCommit(target with{Epoch=2},1.6,true,true));
Check("practice cannot masquerade as shell target",!attempt.CanCommit(target with{Practice=true},1.6,true,true));
Check("expired target rejected",!NotificationAttempt.Matches(target,target,8.1));
Check("backwards clock rejected",!NotificationAttempt.Matches(target,target,-1));
attempt.Cancel();Check("cancellation removes pending action",attempt.Target==null&&attempt.Stage==PawStage.None&&!attempt.CanCommit(target,1.6,true,true));
Check("expired candidate not retried",!attempt.Begin(target,10));
Check("invalid geometry rejected",!attempt.Begin(target with{Button=new(0,0,0,0)},0));

string dir=Path.Combine(Path.GetTempPath(),"CuteCat-checks-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
var store=new StateStore(dir);await store.SaveAsync(new AppState{Settings=new Preferences{Nickname="Test",Size=160}});
Check("atomic state round trip",store.Load().Settings.Nickname=="Test"&&store.Load().Settings.Size==160);
await Task.WhenAll(Enumerable.Range(0,10).Select(i=>store.SaveAsync(new AppState{Settings=new Preferences{Nickname="Cat "+i}})));
Check("serialized saves leave valid JSON",store.Load().Settings.Nickname.StartsWith("Cat "));
Check("prior atomic version kept",File.Exists(store.StatePath+".bak"));
File.WriteAllText(store.StatePath,"broken{");var broken=new StateStore(dir);broken.Load();
Check("corrupt state preserved",broken.Notice!=null&&Directory.GetFiles(dir,"*.unreadable-*").Length==1);
File.WriteAllText(store.StatePath,"{\"Schema\":99}");var future=new StateStore(dir);future.Load();await future.SaveAsync(new());
Check("future schema never overwritten",future.ReadOnly&&File.ReadAllText(store.StatePath).Contains("99"));
File.WriteAllText(store.StatePath,"{\"Schema\":1,\"Settings\":null,\"History\":null,\"Session\":null}");
Check("null sections recover",new StateStore(dir).Load().Settings.Nickname=="Pip");
File.WriteAllText(store.StatePath,"{\"Version\":1,\"Nickname\":\"Legacy\",\"CatVisible\":false,\"Size\":180,\"NotificationGuard\":true,\"PositionRatio\":0.4,\"Session\":{\"Phase\":\"Focus\",\"Duration\":1500,\"Elapsed\":37},\"History\":[]}");
var legacy=new StateStore(dir);var migrated=legacy.Load();
Check("legacy nickname and size migrate",migrated.Settings.Nickname=="Legacy"&&migrated.Settings.Size==160);
Check("legacy permissions and visibility preserved",migrated.Settings.Notifications&&!migrated.Settings.CatVisible);
Check("legacy session migrates paused",migrated.Session.Elapsed==37&&migrated.Session.Status==SessionStatus.Paused);
Check("legacy original separately preserved",File.Exists(store.StatePath+".legacy-v1.json"));
File.WriteAllText(store.StatePath,"{\"Version\":99,\"Nickname\":\"Future\"}");var unknownLegacy=new StateStore(dir);unknownLegacy.Load();await unknownLegacy.SaveAsync(new());
Check("future legacy format preserved",unknownLegacy.ReadOnly&&File.ReadAllText(store.StatePath).Contains("99"));
File.WriteAllText(store.StatePath,"{\"schema\":99}");var lower=new StateStore(dir);lower.Load();
Check("case variant future schema preserved",lower.ReadOnly);
File.WriteAllText(store.StatePath,"[]");var wrongRoot=new StateStore(dir);wrongRoot.Load();Check("malformed root handled without crashing",wrongRoot.Notice!=null);
File.WriteAllText(store.StatePath,"{\"Version\":1,\"Nickname\":\"Legacy\",\"Session\":{\"Phase\":\"Paused\",\"Duration\":\"00:25:00\",\"Elapsed\":\"00:00:45\"}}");
var spanLegacy=new StateStore(dir).Load();Check("legacy timespans safely migrate",spanLegacy.Session.Minutes==25&&spanLegacy.Session.Elapsed==45);
var brain=new CompanionBrain();
Check("idle threshold does not sleep early",brain.Observe(59,true,60,false)==IdleTransition.None);
Check("idle threshold starts a single automatic nap",brain.Observe(60,true,60,false)==IdleTransition.Sleep&&brain.Observe(80,true,60,false)==IdleTransition.None);
Check("returning input wakes automatic sleep",brain.Observe(0,true,60,false)==IdleTransition.Wake&&!brain.AutoSleeping);
brain.UserAction(CatAction.Sleep);Check("manual sleep is not undone by background idle checks",brain.Observe(0,true,60,false)==IdleTransition.None);
brain.UserAction(CatAction.Wake);Check("busy interaction postpones automatic nap",brain.Observe(90,true,60,true)==IdleTransition.None);
Check("nap starts after interaction finishes",brain.Observe(90,true,60,false)==IdleTransition.Sleep);
Check("turning naps off wakes an automatic nap",brain.Observe(90,false,60,false)==IdleTransition.Wake);
var rule=new AppRule(@"C:\Games\Example.exe","Example");
Check("rules match exact paths case insensitively",AppRulePolicy.Match(new[]{rule},@"c:\games\EXAMPLE.exe",true,false,10,0)==rule);
Check("same name at another path is not targeted",AppRulePolicy.Match(new[]{rule},@"C:\Other\Example.exe",true,false,10,0) is null);
Check("disabled guard cannot close selected apps",AppRulePolicy.Match(new[]{rule},rule.Path,false,true,10,0) is null);
Check("paused guard cannot close selected apps",AppRulePolicy.Match(new[]{rule},rule.Path,true,true,10,20) is null);
Check("per-rule disable wins",AppRulePolicy.Match(new[]{rule with{Enabled=false}},rule.Path,true,true,10,0) is null);
Check("focus-only rule leaves breaks alone",AppRulePolicy.Match(new[]{rule with{Scope=AppRuleScope.DuringFocus}},rule.Path,true,false,10,0) is null);
Check("focus-only rule activates during focus",AppRulePolicy.Match(new[]{rule with{Scope=AppRuleScope.DuringFocus}},rule.Path,true,true,10,0) is not null);
Check("app and notification targets cannot be confused",!NotificationAttempt.Matches(target,target with{AppWindow=true},1));
var napper=new Companion(desktop);napper.Tick(0);napper.Perform(CatAction.Sleep,0);for(int i=1;i<=4000;i++)napper.Tick(i/60d);
Check("sleep stays asleep until woken",napper.Action==CatAction.Sleep&&napper.Pose.SleepBubble>0&&napper.Pose.Curl==1);
napper.Perform(CatAction.Wake,67);for(int i=1;i<=150;i++)napper.Tick(67+i/60d);
Check("wake stretches then resumes activity",napper.IsTravelling&&napper.Pose.Curl==0);
napper.Accessory=PetAccessory.Bandana;napper.AccessoryColor="Rose";napper.Angry=true;napper.Tick(70);
Check("appearance and mood reach rendered pose",napper.Pose.Accessory==PetAccessory.Bandana&&napper.Pose.AccessoryColor=="Rose"&&napper.Pose.Anger>0);
bool topReach=true;
foreach(double dpi in new[]{1,1.25,1.5,2})foreach(int size in new[]{96,128,160})
{double scale=size/180d*dpi;var screen=new Area(0,0,1920*dpi,1040*dpi);topReach&=NotificationApproach.Plan(new(400*dpi,900*dpi),screen,scale,new Area(screen.Right-50*dpi,0,45*dpi,30*dpi)) is not null;}
Check("top caption close buttons are reachable at all supported sizes",topReach);
File.WriteAllText(store.StatePath,"{\"Schema\":1,\"Settings\":{\"Nickname\":\"Kept\"}}");
var upgradeStore=new StateStore(dir);var upgraded=upgradeStore.Load();
Check("schema 1 upgrades without enabling app closing",upgraded.Schema==4&&upgraded.Settings.Nickname=="Kept"&&!upgraded.Settings.AppGuard&&upgraded.Settings.AppRules.Count==0&&File.Exists(store.StatePath+".schema1.json"));
await upgradeStore.SaveAsync(upgraded with{Settings=upgraded.Settings with{Accessory=PetAccessory.Flower,AccessoryColor="Sky",Profiles=upgraded.Settings.Profiles.Select(p=>p.Id=="work"?p with{Rules=[rule]}:p).ToList()}});
var saved=upgradeStore.Load();Check("customizations and app rules survive restart",saved.Settings.Accessory==PetAccessory.Flower&&saved.Settings.AccessoryColor=="Sky"&&saved.Settings.AppRules.Single().Path==rule.Path);
ProfileChecks.Run(Check);
File.WriteAllText(store.StatePath,System.Text.Json.JsonSerializer.Serialize(new AppState{Schema=2,Settings=new Preferences{Nickname="Kept",AppGuard=true,AppRules=[rule]}}));
var schema2=new StateStore(dir).Load();
Check("schema 2 rules migrate only into Work with a preserved original",schema2.Schema==4&&schema2.Settings.Profiles.Single(p=>p.Id=="work").Rules.Count==1&&schema2.Settings.Profiles.Single(p=>p.Id=="study").Rules.Count==0&&schema2.Settings.AppGuard&&File.Exists(store.StatePath+".schema2.json"));
File.WriteAllText(store.StatePath,"{\"Schema\":3,\"Settings\":{\"Nickname\":\"Wardrobe\",\"Accessory\":4,\"AccessoryColor\":\"Rose\",\"Theme\":\"Dark\"}}");
var wardrobeStore=new StateStore(dir);var wardrobe=wardrobeStore.Load();
Check("schema 3 migrates flower to head slot and preserves theme",wardrobe.Settings.Appearance is{Hat:CatHat.Flower,HatColor:"#D794A5"}&&wardrobe.Settings.Theme=="Dark"&&File.Exists(store.StatePath+".schema3.json"));
Check("legacy neckwear and collar migrate to separate slots",PetAppearance.FromLegacy(PetAccessory.Bandana,"Sky").Neckwear==CatNeckwear.Bandana&&PetAppearance.FromLegacy(PetAccessory.BellCollar,"Honey").Collar==CatCollar.Bell);
var outfit=new PetAppearance{CoatColor="#555c69",Hat=CatHat.Beanie,HatColor="#aab6e9",Neckwear=CatNeckwear.Bandana,NeckwearColor="#d794a5",Collar=CatCollar.Heart,CollarColor="#dbb76d"};
await wardrobeStore.SaveAsync(wardrobe with{Settings=wardrobe.Settings with{Appearance=outfit,TitleBarStyle="Accent"}});
Check("coat and all independent slots survive restart",wardrobeStore.Load().Settings.Appearance==outfit.Normalize()&&wardrobeStore.Load().Settings.TitleBarStyle=="Accent");
Check("coat changes preserve all worn accessories",(outfit with{CoatColor=PetAppearance.Oat}).Hat==outfit.Hat&&(outfit with{CoatColor=PetAppearance.Oat}).CollarColor==outfit.CollarColor);
Check("invalid colours and outfit values use safe defaults",(outfit with{CoatColor="transparent",Hat=(CatHat)999,HatColor="#123"}).Normalize() is{CoatColor:PetAppearance.Oat,Hat:CatHat.None,HatColor:PetAppearance.Sage});
Check("title bar choice is normalized independently of theme",StateStore.Normalize(new(){Settings=new(){Theme="Dark",TitleBarStyle="invalid"}}).Settings is{Theme:"Dark",TitleBarStyle:"Theme"});
Check("custom colours are opaque six-digit RGB only",PetAppearance.IsColor("#abcdef")&&!PetAppearance.IsColor("#abc")&&!PetAppearance.IsColor("#00ffffff")&&!PetAppearance.IsColor("#zzzzzz"));
Console.WriteLine($"\n{passed} passed; {failed} failed. Isolated fixtures: {dir}");
Environment.ExitCode=failed==0?0:1;
