using CommunityToolkit.WinUI.Notifications;
using System.Security.Principal;

namespace CuteCat.App;

/// <summary>Sends only this app's test notification. Never requests access to other notification content.</summary>
public sealed class WindowsTestNotification
{
    private bool _registered;
    public string Result { get; private set; }="NotSent";
    public bool DismissedByClose { get; private set; }
    public bool Sent { get; private set; }
    public static bool IsElevated
    {
        get{using var identity=WindowsIdentity.GetCurrent();return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);}
    }
    public event Action? Activated;
    public bool Send()
    {
        DismissedByClose=false;Sent=false;
        if(IsElevated){Result="Elevated";return false;}
        if(string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath),"dotnet",StringComparison.OrdinalIgnoreCase))
        {Result="UseExecutable";return false;}
        try
        {
            if(!_registered)
            {
                ToastNotificationManagerCompat.OnActivated+=_=>Activated?.Invoke();_registered=true;
            }
            string setting=ToastNotificationManagerCompat.CreateToastNotifier().Setting.ToString();
            if(setting!="Enabled"){Result=setting;return false;}
            new ToastContentBuilder().AddText("A little tap for a quieter desk")
                .AddText("This is a real Windows notification from Cute Cat.")
                .AddAudio(new ToastAudio{Silent=true})
                .Show(toast=>
                {
                    toast.Tag="CuteCatTest";toast.Group="CuteCat";toast.ExpirationTime=DateTimeOffset.Now.AddSeconds(15);
                    toast.Dismissed+=(_,args)=>
                    { Result=args.Reason.ToString();DismissedByClose=args.Reason==Windows.UI.Notifications.ToastDismissalReason.UserCanceled; };
                    toast.Failed+=(_,_)=>Result="Failed";
                });
            Sent=true;Result="Sent";return true;
        }
        catch(Exception e)when(e is System.Runtime.InteropServices.COMException or InvalidOperationException or UnauthorizedAccessException or IOException)
        {Result="Unavailable";return false;}
    }
}
