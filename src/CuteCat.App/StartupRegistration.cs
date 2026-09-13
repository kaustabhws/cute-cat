using Microsoft.Win32;

namespace CuteCat.App;

internal static class StartupRegistration
{
    private const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
    public static void Apply(bool enabled)
    {
        using var run=Registry.CurrentUser.CreateSubKey(RunKey);
        string command="\""+Environment.ProcessPath+"\" --tray";
        if(enabled){if(!string.Equals(run.GetValue("CuteCat") as string,command,StringComparison.OrdinalIgnoreCase))run.SetValue("CuteCat",command);}
        else run.DeleteValue("CuteCat",false);
    }
    public static bool IsRegistered()
    {
        using var run=Registry.CurrentUser.OpenSubKey(RunKey);
        return string.Equals(run?.GetValue("CuteCat") as string,"\""+Environment.ProcessPath+"\" --tray",StringComparison.OrdinalIgnoreCase);
    }
}
