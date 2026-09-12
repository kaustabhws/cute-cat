namespace CuteCat.App;

internal static class BuildInfo
{
    public static string Version => typeof(BuildInfo).Assembly.GetName().Version!.ToString(3);
}
