using System.Runtime.InteropServices;
using System.Text.Json;
using CuteCat.Core;
using Forms=System.Windows.Forms;

namespace CuteCat.App;

internal static class SupportDiagnostics
{
    // Deliberate allowlist. Never serialize Preferences, targets, exception messages or the host.
    public static string Report(CompanionHost host)=>JsonSerializer.Serialize(new
    {
        product="Cute Cat",version=BuildInfo.Version,reportSchema=1,
        windowsBuild=Environment.OSVersion.Version.ToString(),architecture=RuntimeInformation.ProcessArchitecture.ToString(),
        uiAccess=RuntimeAccess.HasUiAccess,elevated=WindowsTestNotification.IsElevated,
        monitors=Forms.Screen.AllScreens.Length,dpiScale=Math.Round(host.Cat.Scale*CatRig.CharacterHeight/host.Settings.Size,2),
        settingsReadOnly=host.Store.ReadOnly,saveFailure=host.SaveNotice is not null,
        catVisible=host.Visible,suppressed=host.IsSuppressed,reducedMotion=host.Cat.ReducedMotion,menuVisible=host.Menu.IsOpen,
        notificationHelper=host.Settings.Notifications,appGuard=host.Settings.AppGuard,profileGuard=host.CurrentProfile.GuardEnabled,
        notificationControls=Code(host.ShellScanStatus),appControls=Code(host.AppScanStatus),
        windowsTest=TestCode(host.TestNotification.Result),confirmedWindowsDismissals=host.ShellDismissals,
        practice=host.LastJourney is {Practice:true} journey?(journey.Dismissed?"Dismissed":journey.Cancelled?"Cancelled":"In progress"):"Not the latest action",
        note="Control availability and app-owned tests only. This report does not certify real mouse interaction or every notification layout."
    },new JsonSerializerOptions{WriteIndented=true});
    private static string Code(string code)=>code is "NoMatchingApp" or "NoForegroundWindow" or "NoCaption" or "SupportedApp" or "NoShellWindow" or "SupportedBanner" or "Unavailable"?code:"No supported control detected";
    private static string TestCode(string code)=>code is "NotSent" or "Sent" or "UserCanceled" or "TimedOut" or "Elevated"?code:"Windows test unavailable";
    public static string Explanation(CompanionHost host)
    {
        if(host.Store.ReadOnly)return "Settings are protected because this data belongs to a different or unreadable format. Your original file is preserved.";
        if(!host.Visible)return "Your cat is hidden. Show it to resume paw actions.";
        if(host.IsSuppressed)return "Paw actions are resting while the desktop is locked, disconnected, suspended or showing a full-screen app.";
        if(host.Cat.ReducedMotion)return "Reduced motion is active. Paw approaches are paused; your saved rules are unchanged.";
        if(host.Menu.IsOpen)return "The companion menu is open. Paw actions resume after it closes.";
        if(!host.Settings.Notifications&&!host.Settings.AppGuard)return "Your companion is ready. App guard and notification dismissal are both off until you choose to enable them.";
        return "Your enabled helpers are watching for supported controls. A save prompt, an unselected app or an unsupported close button is left alone.";
    }
}
