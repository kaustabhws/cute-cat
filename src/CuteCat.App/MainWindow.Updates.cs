using System.Windows;
using CuteCat.Core;

namespace CuteCat.App;

public partial class MainWindow
{
    private readonly ReleaseUpdates _updates=new();
    private Action? _updateRefresh;
    private void UpdatesPage()
    {
        var(left,right)=Columns();left.Children.Add(Eyebrow("A well-kept little companion"));left.Children.Add(Title("Updates, at your pace."));
        left.Children.Add(Text("Installed preview · "+BuildInfo.Version,15,"Ink",16));
        left.Children.Add(Text("Checks connect to GitHub. Downloads must match their published checksum and this app's signing identity before setup can open.",14,"Muted",18));
        var status=Text(_updates.Status,13,"Ink",16);left.Children.Add(status);
        var check=Button("Check for updates",async()=>await _updates.Check(),true);
        var download=Button("Download & verify",async()=>await _updates.Download());
        var install=Button("Open verified setup",_updates.Install);
        left.Children.Add(Row(check,download,install));
        void RefreshUpdates(){if(_current!="Updates")return;status.Text=_updates.Status;check.IsEnabled=!_updates.Busy;download.IsEnabled=!_updates.Busy&&_updates.Available is not null;install.IsEnabled=!_updates.Busy&&_updates.Downloaded is not null;}
        _updateRefresh=RefreshUpdates;RefreshUpdates();
        left.Children.Add(Check("Check once when Cute Cat starts",_host.Settings.CheckUpdatesAutomatically,v=>_host.Update(_host.Settings with{CheckUpdatesAutomatically=v})));
        left.Children.Add(Text("Automatic checks are optional. Downloads and installation always wait for you. This is preview signing, not a publicly verified publisher identity.",12,"Muted",12));
        right.Children.Add(Eyebrow("Recovery"));right.Children.Add(Text("Return to a previous preview",22,"Ink",14));
        right.Children.Add(Text("Setup keeps this and the previous verified installer in its protected Recovery folder. Reinstalling preserves your settings; newer data formats remain read-only in older builds.",13,"Muted",14));
        var recovery=ReleaseUpdates.RecoveryInstallers();
        if(recovery.Count==0)right.Children.Add(Text("No earlier compatible installer is saved yet. This release establishes the recovery baseline. The older unsigned setup cannot be used for verified rollback.",13,"Muted",12));
        foreach(var item in recovery)right.Children.Add(Button("Reinstall "+item.Version,()=>
        {
            if(MessageBox.Show(this,"Open the verified installer for "+item.Version+"? Your settings are preserved, but features from newer versions may be unavailable.","Return to an earlier preview",MessageBoxButton.OKCancel)==MessageBoxResult.OK)_updates.Launch(item.Path,item.Version);
        }));
        right.Children.Add(Text("If the app cannot start, open C:\\Program Files\\Cute Cat\\Recovery and run a saved setup. The wizard also supports a repair by reinstalling the same version.",12,"Muted",16));
        right.Children.Add(Button("Back to settings",()=>Navigate("Settings")));
    }
}
