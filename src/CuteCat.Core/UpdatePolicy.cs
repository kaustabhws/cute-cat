using System.Text.RegularExpressions;

namespace CuteCat.Core;

public sealed record UpdatePackage(string Version,string Url,long Size,string Sha256);
public static partial class UpdatePolicy
{
    public const string Repository="kaustabhws/cute-cat";
    public const long MaximumBytes=250*1024*1024;
    [GeneratedRegex("^[0-9]+\\.[0-9]+\\.[0-9]+$",RegexOptions.CultureInvariant)]private static partial Regex VersionPattern();
    [GeneratedRegex("^[a-fA-F0-9]{64}$",RegexOptions.CultureInvariant)]private static partial Regex HashPattern();
    public static bool TryVersion(string? text,out Version version)
    {version=new(0,0,0);return text is not null&&VersionPattern().IsMatch(text)&&System.Version.TryParse(text,out version!);}
    public static string FileName(string version)=>$"CuteCat-{version}-Setup.exe";
    public static string AssetUrl(string version,string name)=>$"https://github.com/{Repository}/releases/download/v{version}/{name}";
    public static bool Valid(UpdatePackage package,string current,bool rollback=false)=>TryVersion(package.Version,out var next)&&TryVersion(current,out var installed)&&
        (rollback?next<installed:next>installed)&&package.Size>0&&package.Size<=MaximumBytes&&HashPattern().IsMatch(package.Sha256??"")&&
        string.Equals(package.Url,AssetUrl(package.Version,FileName(package.Version)),StringComparison.Ordinal);
}
