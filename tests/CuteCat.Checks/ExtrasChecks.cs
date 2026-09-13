using System.Text.Json;
using CuteCat.Core;

internal static class ExtrasChecks
{
    public static async Task Run(Action<string,bool> check,string directory)
    {
        var rule=new AppRule(@"C:\Games\Example.exe","Example",CloseDelaySeconds:5);
        var look=new PetAppearance{CoatColor="#555C69",Pattern=CoatPattern.Calico,PatternColor="#D09055",Hat=CatHat.Beret,Neckwear=CatNeckwear.Bandana,Collar=CatCollar.Heart};
        var p=StateStore.Normalize(new(){Settings=new(){Appearance=look,AppRules=[rule],Outfits=[new("study-look","Study look",look with{Pattern=CoatPattern.Tabby})]}}).Settings;
        p=p with{Profiles=p.Profiles.Select(x=>x.Id=="study"?x with{OutfitId="study-look"}:x).ToList()};
        check("unlinked profile keeps everyday appearance",OutfitPolicy.Resolve(p,p.Profiles[0])==look);
        check("linked profile resolves all saved appearance slots",OutfitPolicy.Resolve(p,p.Profiles[1]).Pattern==CoatPattern.Tabby&&OutfitPolicy.Resolve(p,p.Profiles[1]).Hat==CatHat.Beret);
        var edited=OutfitPolicy.Edit(p,"study",look with{CoatColor="#FFFFFF"});
        check("editing linked outfit unlinks without mutating saved look",edited.Profiles[1].OutfitId is null&&edited.Outfits[0].Appearance.CoatColor=="#555C69"&&edited.Appearance?.CoatColor=="#FFFFFF");
        check("deleting outfit clears profile references and preserves rules",OutfitPolicy.Remove(p,"study-look").Profiles[1].OutfitId is null&&OutfitPolicy.Remove(p,"study-look").AppRules.SequenceEqual(p.AppRules));
        check("outfit normalization rejects invalid ids and missing links",StateStore.Normalize(new(){Settings=p with{Outfits=[new("../bad","bad",look)]}}).Settings is var clean&&clean.Outfits.Count==0&&clean.Profiles[1].OutfitId is null);
        check("outfits are bounded and names normalized",StateStore.Normalize(new(){Settings=p with{Outfits=Enumerable.Range(0,30).Select(i=>new SavedOutfit("o"+i,new string('a',60),look)).ToList()}}).Settings is var bounded&&bounded.Outfits.Count==24&&bounded.Outfits.All(o=>o.Name.Length==32));
        var store=new StateStore(Path.Combine(directory,"extras"));Directory.CreateDirectory(store.DirectoryPath);
        string old=JsonSerializer.Serialize(new AppState{Schema=4,Settings=p with{AppGuard=true,Notifications=true,Outfits=[]}});
        File.WriteAllText(store.StatePath,old);var upgraded=store.Load();
        check("schema 4 migration preserves original and permissions",upgraded.Schema==5&&upgraded.Settings.AppGuard&&upgraded.Settings.Notifications&&File.ReadAllText(store.StatePath+".schema4.json")==old);
        await store.SaveAsync(new(){Settings=p});var restored=store.Load();
        check("patterns outfits and profile links survive restart",restored.Settings.Outfits.Single().Appearance.Pattern==CoatPattern.Tabby&&restored.Settings.Profiles[1].OutfitId=="study-look"&&restored.Settings.Appearance==look);
        check("invalid pattern enum and colour fall back safely",(look with{Pattern=(CoatPattern)99,PatternColor="url(bad)"}).Normalize() is{Pattern:CoatPattern.Solid,PatternColor:"#956950"});
        var source=p with{AppGuard=true,Notifications=true,AutomaticProfiles=true,Startup=true,AvoidFocusedControls=true,AppExceptions=[new(rule.Path,DateTimeOffset.UtcNow.AddMinutes(5))],Monitor="private-monitor",RestingSpots=[new("private-monitor",.2,.3)]};
        string json=SettingsTransfer.Export(source);var imported=SettingsTransfer.Review(json,new(){Startup=false,Monitor="local-display"});
        check("backup preserves theme and wardrobe without transient data",imported.Appearance==look&&imported.Outfits.Count==1&&!json.Contains("private-monitor")&&imported.AppExceptions.Count==0);
        check("import never silently enables guards schedules or startup",!imported.AppGuard&&!imported.Notifications&&!imported.AutomaticProfiles&&!imported.AvoidFocusedControls&&!imported.Startup);
        check("all imported rules start unselected",imported.Profiles.SelectMany(x=>x.Rules).All(r=>!r.Enabled));
        check("import preserves current machine placement and startup",SettingsTransfer.Review(json,new(){Startup=true,Monitor="kept"}) is{Startup:true,Monitor:"kept"});
        bool Reject(string value){try{SettingsTransfer.Review(value,new());return false;}catch(InvalidDataException){return true;}}
        check("unknown backup version rejected",Reject(json.Replace("\"Version\": 1","\"Version\": 99")));
        check("unknown backup fields rejected",Reject(json.Replace("\"Version\": 1","\"Unexpected\": true,\"Version\": 1")));
        check("malformed and oversized backups leave settings untouched",Reject("[]")&&Reject("null")&&Reject("{")&&Reject(new string(' ',SettingsTransfer.MaximumBytes+1)));
        check("invalid app path is rejected instead of silently dropped",Reject(json.Replace("C:\\\\Games\\\\Example.exe","relative.exe")));
        check("duplicate outfit identifiers rejected",Reject(JsonSerializer.Serialize(new SettingsBundle("CuteCat.Settings",1,source with{Outfits=[source.Outfits[0],source.Outfits[0]]}))));
        check("null profile list rejected",Reject(JsonSerializer.Serialize(new SettingsBundle("CuteCat.Settings",1,source with{Profiles=null!}))));
        var session=new FocusSession();session.Start(25,0);for(int i=1;i<=60;i++)session.Tick(i);session.BeginBreak(60);
        check("break preserves earned focus progress",session.IsBreak&&session.CanReturnToFocus&&session.InterruptedFocus?.Elapsed==60&&session.Remaining==300);
        session.Tick(61);session.BeginBreak(61);check("repeated take-break does not overwrite saved focus",session.Elapsed==1&&session.InterruptedFocus?.Elapsed==60);
        int completions=0;for(int i=62;i<=361;i++)if(session.Tick(i))completions++;
        check("completed break earns no focus record",session.Status==SessionStatus.Completed&&completions==0);
        var extended=new FocusSession();extended.Restore(session.Snapshot(),500);extended.BeginBreak(500);
        check("extending a completed break keeps the original focus progress",extended.IsBreak&&extended.InterruptedFocus?.Elapsed==60);
        var recovered=new FocusSession();recovered.Restore(session.Snapshot(),1000);
        check("restart keeps an explicit return from completed break",recovered.CanReturnToFocus&&recovered.Status==SessionStatus.Completed);
        recovered.ReturnToFocus(1000);recovered.Tick(1001);
        check("return resumes preserved progress without awarding break time",!recovered.IsBreak&&recovered.Elapsed==61&&recovered.Status==SessionStatus.Running&&!recovered.CanReturnToFocus);
        recovered.BeginBreak(1001);var restart=new FocusSession();restart.Restore(recovered.Snapshot(),2000);
        check("restart during break pauses and preserves return session",restart.Status==SessionStatus.Paused&&restart.InterruptedFocus?.Elapsed==61);
        restart.End();check("ending break clears the deferred return",!restart.CanReturnToFocus);
        restart.Restore(new(IsBreak:true,ReturnToFocus:new(Elapsed:double.NaN,Status:SessionStatus.Running)),0);
        check("invalid return snapshot cannot create focus credit",!restart.CanReturnToFocus);
        var cues=new BreakCueScheduler();BreakCueKind? cue=null;
        for(int i=0;i<=899;i++)cue=cues.Tick(i,true,15,true,true,true);
        check("break reminder never arrives early",cue is null);
        check("busy focus defers a due reminder",cues.Tick(900,true,15,true,true,false) is null);
        check("deferred reminder emits once when free",cues.Tick(901,true,15,true,true,true)==BreakCueKind.Stretch&&cues.Tick(902,true,15,true,true,true) is null);
        for(int i=903;i<1801;i++)cues.Tick(i,true,15,true,true,true);
        check("break cues alternate stretch and water",cues.Tick(1801,true,15,true,true,true)==BreakCueKind.Water);
        cues.Reset();cues.Tick(0,true,15,true,true,true);cues.Tick(4000,true,15,true,true,true);
        check("suspend gaps do not accrue reminder time",cues.Tick(4001,true,15,true,true,true) is null);
        cues.Reset();for(int i=0;i<2000;i++)cue=cues.Tick(i,true,15,true,false,true);
        check("idle time never creates a reminder",cue is null);
        cues.Reset();for(int i=0;i<899;i++)cues.Tick(i,true,15,true,true,false);cues.Tick(899,false,15,true,true,true);
        check("turning break cues off clears pending work",cues.Tick(900,true,15,true,true,true) is null);
        var shown=CountdownPolicy.Present(rule,"window-one",3,new(GuardPermission.GracePeriod,4.1),true)!;
        check("countdown rounds up existing grace without changing policy",shown.Seconds==5&&shown.Path==rule.Path);
        check("instant mode allowance and suppression show no countdown",CountdownPolicy.Present(rule,"one",1,new(GuardPermission.Ready),true) is null&&CountdownPolicy.Present(rule,"one",1,new(GuardPermission.DailyAllowance,12),true) is null&&CountdownPolicy.Present(rule,"one",1,new(GuardPermission.GracePeriod,4),false) is null);
        check("stale countdown cannot grant exception to another window",!CountdownPolicy.CanAllow(shown,shown with{Identity="replacement"})&&!CountdownPolicy.CanAllow(shown,shown with{Epoch=4})&&!CountdownPolicy.CanAllow(shown,null));
        check("countdown permits the same target while seconds advance",CountdownPolicy.CanAllow(shown,shown with{Seconds=2}));
        var cat=new Companion(new(0,0,1200,800));cat.Tick(0);var before=cat.Pose;cat.Perform(CatAction.Pet,0);cat.Tick(0);
        check("pet reaction blends from the displayed pose",cat.Pose==before);
        cat.Tick(.72);check("pet reaction has happy eyes and a gentle lean",cat.Pose.Affection>.95&&cat.Pose.Eyes<.1&&cat.Pose.HeadTilt<0);
        for(int i=44;i<=120;i++)cat.Tick(i/60d);
        check("pet returns to normal movement within two seconds",cat.Action is CatAction.Walk or CatAction.Turn);
        var reduced=CatRig.Evaluate(CatAction.Stretch,1.1,0,1,true);
        check("reduced motion removes break stretch movement",reduced.Stretch==0&&reduced.Bob==0&&reduced.ForeNear==default);
    }
}
