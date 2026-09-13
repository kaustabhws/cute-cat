using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CuteCat.Core;

public sealed record SettingsBundle(string Format,int Version,Preferences Settings);
public static class SettingsTransfer
{
    public const int MaximumBytes=512*1024;
    private static readonly JsonSerializerOptions Options=new(){WriteIndented=true,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow,MaxDepth=24};
    public static string Export(Preferences preferences)
    {
        var p=StateStore.Normalize(new(){Settings=preferences}).Settings;
        // A portable backup contains configuration, not session records, allowances or machine placement.
        p=p with{AppExceptions=[],RestingSpots=[],Monitor="",ParkX=.83,ParkY=.95};
        return JsonSerializer.Serialize(new SettingsBundle("CuteCat.Settings",1,p),Options);
    }
    public static Preferences Review(string json,Preferences current)
    {
        if(Encoding.UTF8.GetByteCount(json)>MaximumBytes)throw new InvalidDataException("The backup is larger than 512 KB.");
        SettingsBundle bundle;
        try{bundle=JsonSerializer.Deserialize<SettingsBundle>(json,Options)??throw new JsonException();}
        catch(JsonException){throw new InvalidDataException("This is not a supported Cute Cat settings backup.");}
        if(bundle.Format!="CuteCat.Settings"||bundle.Version!=1||bundle.Settings is null)
            throw new InvalidDataException("This backup format needs a different version of Cute Cat.");
        var source=bundle.Settings;
        if(source.Profiles is null||source.Profiles.Count is <1 or >8||source.Profiles.Any(p=>p is null||p.Rules is null||p.Rules.Count>50)||
            source.Outfits is null||source.Outfits.Count>24||source.Outfits.Any(o=>o is null||!OutfitPolicy.ValidId(o.Id)||o.Appearance is null))
            throw new InvalidDataException("The backup contains invalid profiles or outfits.");
        var normalized=StateStore.Normalize(new(){Settings=source}).Settings;
        if(normalized.Profiles.Count!=source.Profiles.Count||normalized.Outfits.Count!=source.Outfits.Count||
            normalized.Profiles.Where((p,i)=>p.Rules.Count!=source.Profiles[i].Rules.Count).Any())
            throw new InvalidDataException("Some rules or identifiers are invalid or duplicated. The backup was not imported.");
        return normalized with
        {
            AppGuard=false,Notifications=false,AutomaticProfiles=false,AvoidFocusedControls=false,AppExceptions=[],
            Startup=current.Startup,StartupInitialized=current.StartupInitialized,Monitor=current.Monitor,RestingSpots=current.RestingSpots.ToList(),ParkX=current.ParkX,ParkY=current.ParkY,
            Profiles=normalized.Profiles.Select(p=>p with{Rules=p.Rules.Select(r=>r with{Enabled=false}).ToList()}).ToList(),
            AppRules=normalized.AppRules.Select(r=>r with{Enabled=false}).ToList()
        };
    }
}
