using CuteCat.Core;

internal static class ProfileChecks
{
    public static void Run(Action<string,bool> check)
    {
        var monday=new DateTime(2026,9,14,10,0,0);var utc=new DateTimeOffset(2026,9,14,10,0,0,TimeSpan.Zero);
        var rule=new AppRule(@"C:\Games\Example.exe","Example",CloseDelaySeconds:5,DailyAllowanceMinutes:1);
        var profiles=ProfilePolicy.Defaults([rule],ActivityLevel.Playful);
        check("default profiles preserve rules only in Work",profiles[0].Rules.Count==1&&profiles[1].Rules.Count==0&&!profiles[2].GuardEnabled);
        profiles[1]=profiles[1] with{Schedule=new(true,31,540,1020,10)};
        check("manual profile choice overrides schedule",ProfilePolicy.Resolve(profiles,"work",false,monday).Id=="work");
        check("automatic profile follows matching weekday",ProfilePolicy.Resolve(profiles,"work",true,monday).Id=="study");
        check("outside schedule returns to selected profile",ProfilePolicy.Resolve(profiles,"work",true,monday.AddHours(8)).Id=="work");
        check("weekend not included in weekday schedule",ProfilePolicy.Resolve(profiles,"work",true,monday.AddDays(5)).Id=="work");
        var overnight=new ProfileSchedule(true,1,22*60,2*60);
        check("overnight includes late start day",overnight.Includes(monday.Date.AddHours(23)));
        check("overnight includes early following day",overnight.Includes(monday.Date.AddDays(1).AddHours(1)));
        check("overnight excludes early start day and exact end",!overnight.Includes(monday.Date.AddHours(1))&&!overnight.Includes(monday.Date.AddDays(1).AddHours(2)));
        check("equal time schedule is full selected day",new ProfileSchedule(true,1,0,0).Includes(monday));
        profiles[0]=profiles[0] with{Schedule=new(true,31,0,0,10)};
        check("equal priority has stable profile order",ProfilePolicy.Resolve(profiles,"break",true,monday).Id=="work");
        profiles[1]=profiles[1] with{Schedule=profiles[1].Schedule! with{Priority=11}};
        check("higher schedule priority wins",ProfilePolicy.Resolve(profiles,"break",true,monday).Id=="study");
        var gate=new GuardGate();var exceptions=new[]{new AppException(rule.Path.ToLowerInvariant(),utc.AddMinutes(5))};
        check("temporary exception wins over used allowance",gate.Evaluate(rule,"window1",exceptions,600,utc,0).Permission==GuardPermission.TemporaryException);
        check("allowance runs before grace period",gate.Evaluate(rule,"window1",[],59,utc,0).Permission==GuardPermission.DailyAllowance);
        check("allowance expiry starts a new grace period",gate.Evaluate(rule,"window1",[],60,utc,10).RemainingSeconds==5);
        check("grace cannot act early",!gate.Evaluate(rule,"window1",[],60,utc,14.9).CanAct);
        check("grace expires exactly once deadline reached",gate.Evaluate(rule,"window1",[],60,utc,15).CanAct);
        check("window identity replacement restarts grace",!gate.Evaluate(rule,"window2",[],60,utc,15).CanAct);
        check("return after foreground loss restarts grace",ResetAndEvaluate());
        bool ResetAndEvaluate(){gate.Reset();return gate.Evaluate(rule,"window2",[],60,utc,30).RemainingSeconds==5;}
        check("expired exception cannot authorize use",gate.Evaluate(rule,"window3",exceptions,60,utc.AddMinutes(6),31).Permission==GuardPermission.GracePeriod);
        check("exception is exact path rather than executable name",gate.Evaluate(rule,"window3",[new(@"C:\Other\Example.exe",utc.AddMinutes(5))],60,utc,36).CanAct);
        var ledger=new UsageLedger();var day=DateOnly.FromDateTime(monday);
        ledger.Observe(rule.Path,day,0);ledger.Observe(rule.Path.ToLowerInvariant(),day,1);ledger.Observe(null,day,2);
        check("foreground usage is aggregated case insensitively",ledger.Used(rule.Path,day)==2);
        ledger.Observe(null,day,3);check("unselected foreground is not counted",ledger.Used(rule.Path,day)==2);
        ledger.Observe(rule.Path,day,4);ledger.Observe(rule.Path,day,100);
        check("suspend gaps do not spend allowance",ledger.Used(rule.Path,day)==2);
        ledger.Observe(rule.Path,day.AddDays(1),101);check("new local day has a fresh allowance",ledger.Used(rule.Path,day.AddDays(1))==0);
        var restored=new UsageLedger(ledger.Snapshot());check("allowance usage survives restart",restored.Used(rule.Path,day)==2);
        restored.Observe(null,day.AddDays(14),0);check("old allowance totals are pruned",restored.Snapshot().Count==0);
        ledger.Clear();check("clear allowance totals does not leak prior interval",ledger.Snapshot().Count==0);
        var area=new Area(-1920,-200,1920,1040);double scale=.8;var position=new V2(-1550,650);
        var spot=DesktopPlacement.Remember("left",position,area);
        check("resting position round trips on negative monitor",(DesktopPlacement.Restore(spot,area,scale)-position).Length<.01);
        check("resting spot adapts to resized monitor",new Area(0,0,1000,700).Contains(DesktopPlacement.Restore(spot,new(0,0,1000,700),scale)));
        var protection=DesktopPlacement.Footprint(position,scale);
        var away=DesktopPlacement.Avoid(position,area,scale,protection);
        check("cat chooses an unobstructed nearby resting corner",away is V2 p&&!DesktopPlacement.Intersects(DesktopPlacement.Footprint(p,scale),protection));
        check("unobstructed position stays put",DesktopPlacement.Avoid(position,area,scale,new(0,0,20,20)) is null);
        check("no safe corner does not invent a destination",DesktopPlacement.Avoid(position,area,scale,area) is null);
        var cat=new Companion(new(0,0,1920,1040));cat.Tick(0);var origin=cat.Position;cat.Notice(-1,0);cat.Tick(.1);double head=cat.Pose.HeadYaw;
        check("notice is a planted intermediate look",cat.Position==origin&&head<1&&head>-1&&cat.Pose.BodyYaw==1);
        cat.Tick(.22);double look=cat.Pose.HeadYaw;cat.Approach(new(400,500),-1,.22);cat.Tick(.22);
        check("notice flows into run without head snap",Math.Abs(cat.Pose.HeadYaw-look)<.001);
        cat.Stop(.3);cat.Tick(1);check("cancelling notice prevents delayed travel",!cat.IsTravelling);
        check("wake includes procedural yawn",CatRig.Evaluate(CatAction.Wake,.65,0,0).Mouth>.8);
        check("ear twitch is continuous and subtle",Math.Abs(CatRig.Evaluate(CatAction.Idle,0,0,7.1).EarLeft)<=10&&CatRig.Evaluate(CatAction.Idle,0,0,7.1).EarLeft!=0);
        check("celebration has a reduced-motion pose",CatRig.Evaluate(CatAction.Celebrate,.7,0,0,true).Bob==0);
        string version="0.9.0";var package=new UpdatePackage(version,UpdatePolicy.AssetUrl(version,UpdatePolicy.FileName(version)),50_000_000,new string('A',64));
        check("valid release metadata accepted",UpdatePolicy.Valid(package,"0.8.0"));
        check("downgrade metadata is not an automatic update",!UpdatePolicy.Valid(package,"1.0.0"));
        check("rollback is an explicit separate decision",UpdatePolicy.Valid(package,"1.0.0",true));
        check("foreign download origin rejected",!UpdatePolicy.Valid(package with{Url="https://example.com/a.exe"},"0.8.0"));
        check("query and userinfo URL tricks rejected",!UpdatePolicy.Valid(package with{Url=package.Url+"?redirect=evil"},"0.8.0")&&!UpdatePolicy.Valid(package with{Url=package.Url.Replace("github.com","github.com@evil.example")},"0.8.0"));
        check("malformed hash and excessive installer size rejected",!UpdatePolicy.Valid(package with{Sha256="123"},"0.8.0")&&!UpdatePolicy.Valid(package with{Size=UpdatePolicy.MaximumBytes+1},"0.8.0"));
        check("version path traversal rejected",!UpdatePolicy.TryVersion("../0.9.0",out _));
        check("same version is not offered as update",!UpdatePolicy.Valid(package,version));
    }
}
