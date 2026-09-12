using System.Text.Json;

namespace CuteCat.Core;

public sealed record Preferences
{
    public string Nickname { get; init; }="Pip";
    public int Size { get; init; }=128;
    public string Theme { get; init; }="Light";
    public bool Quiet { get; init; }
    public bool ReducedMotion { get; init; }
    public bool FollowWindowsMotion { get; init; }
    public bool Notifications { get; init; }
    public bool Sounds { get; init; }
    public bool Startup { get; init; }
    public bool CatVisible { get; init; }=true;
    public bool IdleNaps { get; init; }=true;
    public int IdleMinutes { get; init; }=3;
    public ActivityLevel Activity { get; init; }=ActivityLevel.Balanced;
    public PetAccessory Accessory { get; init; }
    public string AccessoryColor { get; init; }="Sage";
    public bool AppGuard { get; init; }
    public List<AppRule> AppRules { get; init; }=[];
    public List<FocusProfile> Profiles { get; init; }=[];
    public string ActiveProfileId { get; init; }="work";
    public bool AutomaticProfiles { get; init; }
    public List<AppException> AppExceptions { get; init; }=[];
    public List<MonitorSpot> RestingSpots { get; init; }=[];
    public bool RememberRestingSpots { get; init; }=true;
    public bool AvoidFocusedControls { get; init; }
    public bool SettleWhileWorking { get; init; }=true;
    public bool CheckUpdatesAutomatically { get; init; }
    public string Monitor { get; init; }="";
    public double ParkX { get; init; }=.83;
    public double ParkY { get; init; }=.95;
}
public sealed record AppState
{
    public int Schema { get; init; }=3;
    public Preferences Settings { get; init; }=new();
    public SessionSnapshot Session { get; init; }=new();
    public List<FocusRecord> History { get; init; }=[];
    public List<AppUsage> Usage { get; init; }=[];
}
public sealed class StateStore(string directory)
{
    private static readonly JsonSerializerOptions Options=new(){WriteIndented=true,PropertyNameCaseInsensitive=true};
    public string DirectoryPath { get; }=Path.GetFullPath(directory);
    public string StatePath=>Path.Combine(DirectoryPath,"state.json");
    public string? Notice { get; private set; }
    public bool ReadOnly { get; private set; }
    private readonly SemaphoreSlim _serial=new(1,1);
    public AppState Load()
    {
        if(!File.Exists(StatePath)) return new();
        try
        {
            if(new FileInfo(StatePath).Length>1024*1024) throw new JsonException();
            string json=File.ReadAllText(StatePath);
            using var document=JsonDocument.Parse(json);
            var root=document.RootElement;
            if(root.ValueKind!=JsonValueKind.Object)throw new JsonException();
            bool hasSchema=root.TryGetProperty("Schema",out _)||root.TryGetProperty("schema",out _);
            if(!hasSchema)
            {
                if(root.TryGetProperty("Version",out var version)&&version.TryGetInt32(out int v)&&v==1&&root.TryGetProperty("Nickname",out _))
                {
                    string backup=StatePath+".legacy-v1.json";
                    if(!File.Exists(backup))File.Copy(StatePath,backup,false);
                    Notice="Your previous cat settings were upgraded. A copy of the original settings is preserved.";
                    return ReadLegacy(root);
                }
                ReadOnly=true;Notice="This settings format is not recognized. The original file is preserved; changes will not be saved.";return new();
            }
            var state=JsonSerializer.Deserialize<AppState>(json,Options)??throw new JsonException();
            if(state.Schema is not (1 or 2 or 3)) { ReadOnly=true;Notice="These settings belong to a different app version. The original file is preserved; changes will not be saved.";return new(); }
            if(state.Schema==1&&!File.Exists(StatePath+".schema1.json"))File.Copy(StatePath,StatePath+".schema1.json");
            if(state.Schema==2&&!File.Exists(StatePath+".schema2.json"))File.Copy(StatePath,StatePath+".schema2.json");
            return Normalize(state);
        }
        catch(Exception e) when(e is JsonException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            try { File.Copy(StatePath,StatePath+".unreadable-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),false); }
            catch(IOException) { ReadOnly=true; }
            catch(UnauthorizedAccessException) { ReadOnly=true; }
            Notice="Settings could not be read. Safe defaults are in use; your original file has been preserved.";return new();
        }
    }
    private static AppState ReadLegacy(JsonElement root)
    {
        string S(JsonElement value,string key,string fallback="")=>value.TryGetProperty(key,out var p)&&p.ValueKind==JsonValueKind.String?p.GetString()??fallback:fallback;
        double N(JsonElement value,string key,double fallback=0)
        {
            if(!value.TryGetProperty(key,out var p))return fallback;
            if(p.ValueKind==JsonValueKind.Number&&p.TryGetDouble(out double n)&&double.IsFinite(n))return n;
            if(p.ValueKind==JsonValueKind.String&&TimeSpan.TryParse(p.GetString(),System.Globalization.CultureInfo.InvariantCulture,out var time))return time.TotalSeconds;
            return fallback;
        }
        bool B(JsonElement value,string key,bool fallback=false)=>value.TryGetProperty(key,out var p)&&p.ValueKind is JsonValueKind.True or JsonValueKind.False?p.GetBoolean():fallback;
        double size=N(root,"Size",128);int tier=new[]{96,128,160}.OrderBy(n=>Math.Abs(n-size)).First();
        var preferences=new Preferences{Nickname=S(root,"Nickname","Pip"),Theme=S(root,"Theme","Light"),Size=tier,
            Quiet=B(root,"Quiet"),ReducedMotion=B(root,"ReducedMotion"),FollowWindowsMotion=B(root,"FollowWindowsMotion"),
            Notifications=B(root,"NotificationGuard"),CatVisible=B(root,"CatVisible",true),Monitor=S(root,"Monitor"),ParkX=N(root,"PositionRatio",.83)};
        SessionSnapshot session=new();
        if(root.TryGetProperty("Session",out var oldSession)&&oldSession.ValueKind==JsonValueKind.Object)
        {
            double duration=N(oldSession,"Duration",1500),elapsed=N(oldSession,"Elapsed");
            string phase=oldSession.TryGetProperty("Phase",out var ph)?ph.ToString():"";
            bool active=phase is "Focus" or "Running" or "Paused" or "FocusPaused" or "Break" or "BreakPaused" || elapsed>0&&elapsed<duration;
            session=new(Math.Clamp((int)Math.Round(duration/60),5,180),elapsed,active?SessionStatus.Paused:SessionStatus.Ready,phase.Contains("Break",StringComparison.OrdinalIgnoreCase));
        }
        List<FocusRecord> records=[];
        if(root.TryGetProperty("History",out var oldHistory)&&oldHistory.ValueKind==JsonValueKind.Array)
            foreach(var record in oldHistory.EnumerateArray())
            {
                if(record.ValueKind!=JsonValueKind.Object||!B(record,"Completed"))continue;
                if(DateTimeOffset.TryParse(S(record,"Date"),System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.AssumeLocal,out var date))
                    records.Add(new(date,(int)N(record,"Seconds")));
            }
        return Normalize(new AppState{Settings=preferences,Session=session,History=records});
    }
    public async Task SaveAsync(AppState state)
    {
        if(ReadOnly)return;
        string json=JsonSerializer.Serialize(Normalize(state),Options);
        await _serial.WaitAsync().ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            string temp=StatePath+".tmp";
            await File.WriteAllTextAsync(temp,json).ConfigureAwait(false);
            if(File.Exists(StatePath))File.Replace(temp,StatePath,StatePath+".bak",true);
            else File.Move(temp,StatePath);
        }
        finally { _serial.Release(); }
    }
    public static AppState Normalize(AppState state)
    {
        var p=state.Settings??new();
        string name=p.Nickname?.Trim()??"Pip";
        if(name.Length==0)name="Pip";
        var rules=NormalizeRules(p.AppRules??[]);
        var profiles=(p.Profiles??[]).Where(x=>x is not null&&!string.IsNullOrWhiteSpace(x.Id)).Take(8).Select(x=>x with
        {
            Id=x.Id[..Math.Min(64,x.Id.Length)],Name=string.IsNullOrWhiteSpace(x.Name)?"Profile":x.Name.Trim()[..Math.Min(32,x.Name.Trim().Length)],
            Rules=NormalizeRules(x.Rules??[]),Activity=Enum.IsDefined(x.Activity)?x.Activity:ActivityLevel.Balanced,
            Schedule=(x.Schedule??new()) with{Days=(x.Schedule?.Days??31)&127,StartMinute=Math.Clamp(x.Schedule?.StartMinute??540,0,1439),EndMinute=Math.Clamp(x.Schedule?.EndMinute??1020,0,1439),Priority=Math.Clamp(x.Schedule?.Priority??0,0,100)}
        }).DistinctBy(x=>x.Id,StringComparer.Ordinal).ToList();
        if(profiles.Count==0)profiles=ProfilePolicy.Defaults(rules,Enum.IsDefined(p.Activity)?p.Activity:ActivityLevel.Balanced);
        var active=profiles.FirstOrDefault(x=>x.Id==p.ActiveProfileId)??profiles[0];
        var today=DateOnly.FromDateTime(DateTime.Today);
        return state with { Schema=3,Settings=p with { Nickname=name[..Math.Min(24,name.Length)],Size=p.Size is 96 or 128 or 160?p.Size:128,
            IdleMinutes=Math.Clamp(p.IdleMinutes,1,30),
            Accessory=Enum.IsDefined(p.Accessory)?p.Accessory:PetAccessory.None,
            AccessoryColor=p.AccessoryColor is "Sage" or "Rose" or "Sky" or "Plum" or "Honey"?p.AccessoryColor:"Sage",
            AppRules=active.Rules,Profiles=profiles,ActiveProfileId=active.Id,Activity=active.Activity,
            AppExceptions=(p.AppExceptions??[]).Where(e=>e is not null&&Path.IsPathFullyQualified(e.Path??"")&&e.Until>DateTimeOffset.UtcNow&&e.Until<=DateTimeOffset.UtcNow.AddDays(1)).DistinctBy(e=>e.Path,StringComparer.OrdinalIgnoreCase).Take(50).ToList(),
            RestingSpots=(p.RestingSpots??[]).Where(s=>s is not null&&!string.IsNullOrWhiteSpace(s.Monitor)&&double.IsFinite(s.X+s.Y)).Select(s=>s with{X=Math.Clamp(s.X,0,1),Y=Math.Clamp(s.Y,0,1)}).DistinctBy(s=>s.Monitor).Take(16).ToList(),
            Theme=p.Theme is "Light" or "Dark" or "System"?p.Theme:"Light",
            Monitor=p.Monitor??"",ParkX=double.IsFinite(p.ParkX)?Math.Clamp(p.ParkX,0,1):.83,ParkY=double.IsFinite(p.ParkY)?Math.Clamp(p.ParkY,0,1):.95 },
            Usage=(state.Usage??[]).Where(u=>u is not null&&!string.IsNullOrWhiteSpace(u.Path)&&double.IsFinite(u.Seconds)&&u.Seconds>=0&&u.Day>=today.AddDays(-13)&&u.Day<=today).Select(u=>u with{Seconds=Math.Min(86400,u.Seconds)}).TakeLast(1000).ToList(),
            Session=state.Session??new(), History=(state.History??[]).Where(r=>r is not null && r.Seconds>0 && r.Seconds<=10800 && r.Finished>=DateTimeOffset.UtcNow.AddDays(-30) && r.Finished<=DateTimeOffset.UtcNow.AddMinutes(1)).TakeLast(500).ToList() };
    }
    public static List<AppRule> NormalizeRules(IEnumerable<AppRule> rules)=>rules.Select(NormalizeRule).OfType<AppRule>().DistinctBy(r=>r.Path,StringComparer.OrdinalIgnoreCase).Take(50).ToList();
    private static AppRule? NormalizeRule(AppRule? rule)
    {
        try
        {
            if(rule is null||string.IsNullOrWhiteSpace(rule.Path)||!Path.IsPathFullyQualified(rule.Path)||!rule.Path.EndsWith(".exe",StringComparison.OrdinalIgnoreCase)||!Enum.IsDefined(rule.Action)||!Enum.IsDefined(rule.Scope))return null;
            return rule with{Path=Path.GetFullPath(rule.Path),Name=string.IsNullOrWhiteSpace(rule.Name)?Path.GetFileNameWithoutExtension(rule.Path):rule.Name[..Math.Min(rule.Name.Length,80)],CloseDelaySeconds=Math.Clamp(rule.CloseDelaySeconds,0,60),DailyAllowanceMinutes=Math.Clamp(rule.DailyAllowanceMinutes,0,720)};
        }
        catch(Exception e)when(e is ArgumentException or NotSupportedException or PathTooLongException){return null;}
    }
}
