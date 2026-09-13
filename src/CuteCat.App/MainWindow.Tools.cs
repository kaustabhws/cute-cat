using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using CuteCat.Core;

namespace CuteCat.App;

public partial class MainWindow
{
    private Action? _toolsRefresh;
    private void TroubleshootingPage()
    {
        var(left,right)=Columns();left.Children.Add(Eyebrow("A little help, close by"));left.Children.Add(Title("Let's check on things."));
        var status=Text("",14,"Muted",16);left.Children.Add(status);
        var controls=Text("",13,"Ink",20);left.Children.Add(controls);
        _toolsRefresh=()=>
        {
            status.Text=SupportDiagnostics.Explanation(_host);
            controls.Text="Menu · "+(_host.Menu.IsOpen?"open":"closed")+"\nCat · "+(_host.Visible?"shown":"hidden")+
                "\nApp guard · "+(_host.Settings.AppGuard&&_host.CurrentProfile.GuardEnabled?"on for this profile":"off for this profile")+
                "\nNotification helper · "+(_host.Settings.Notifications?"on":"off")+
                "\nNotification layer · "+(RuntimeAccess.HasUiAccess?"UIAccess available":"standard window; banners may cover the cat")+
                "\nWindows test · "+_host.TestNotification.Result;
        };
        left.Children.Add(Button("Open menu test",()=>_host.TestMenu()));
        left.Children.Add(Text("The same menu opens from the cat and tray. Click outside it to check dismissal on your desktop.",12,"Muted",14));
        left.Children.Add(Button("Try a practice notification",()=>_host.StartPractice()));
        left.Children.Add(Text("An app-owned card checks the run, paw contact and dismissal. It does not test Windows' notification controls.",12,"Muted",14));
        left.Children.Add(Button("Send a Windows test notification",_host.WindowsTest));
        left.Children.Add(Text("Uses a real Windows banner. Turn the notification helper on in Quiet desktop to test dismissal. Do not disturb can suppress banners; administrator processes cannot send this test.",12,"Muted",14));
        right.Children.Add(Eyebrow("Share only what helps"));right.Children.Add(Text("A private support report",22,"Ink",12));
        right.Children.Add(Text("Includes version, Windows build, display scale and helper status. Excludes executable paths, names, messages, window titles, focus history and your settings contents.",13,"Muted",16));
        var report=new TextBox{IsReadOnly=true,Text=SupportDiagnostics.Report(_host),AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,Height=285,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,FontSize=11};
        System.Windows.Automation.AutomationProperties.SetName(report,"Privacy-safe support report preview");right.Children.Add(report);
        var note=Text("",12,"Muted",10);right.Children.Add(note);
        right.Children.Add(Row(Button("Refresh report",()=>report.Text=SupportDiagnostics.Report(_host)),Button("Copy report",()=>
        {try{Clipboard.SetText(report.Text);note.Text="Support report copied.";}catch(ExternalException){note.Text="The clipboard is busy. Try again in a moment.";}})));
        right.Children.Add(Button("Back to settings",()=>Navigate("Settings")));
    }
    private void BackupPage()
    {
        var(left,right)=Columns();left.Children.Add(Eyebrow("Your cat, wherever you work"));left.Children.Add(Title("Keep a little backup."));
        left.Children.Add(Text("Export your profiles, exact app rules, saved outfits and preferences to a readable JSON file.",14,"Muted",18));
        left.Children.Add(Text("App paths and names are part of your backup. Store it somewhere you trust. Focus history, daily usage, temporary exceptions and monitor positions are excluded.",12,"Muted",20));
        var result=Text("",12,"Muted",14);left.Children.Add(result);
        left.Children.Add(Button("Export settings",()=>
        {
            var picker=new SaveFileDialog{Title="Export Cute Cat settings",Filter="Cute Cat settings (*.json)|*.json",FileName="CuteCat-settings.json",DefaultExt=".json",OverwritePrompt=true};
            if(picker.ShowDialog(this)!=true)return;
            string temp=picker.FileName+"."+Guid.NewGuid().ToString("N")+".tmp";
            try{File.WriteAllText(temp,SettingsTransfer.Export(_host.Settings),new UTF8Encoding(false));File.Move(temp,picker.FileName,true);result.Text="Settings exported.";}
            catch(Exception e)when(e is IOException or UnauthorizedAccessException){result.Text="The backup could not be written. Choose a writable folder.";}
            finally{try{if(File.Exists(temp))File.Delete(temp);}catch(IOException){}catch(UnauthorizedAccessException){}}
        },true));
        left.Children.Add(Button("Import & review settings",()=>
        {
            var picker=new OpenFileDialog{Title="Import Cute Cat settings",Filter="Cute Cat settings (*.json)|*.json",CheckFileExists=true};
            if(picker.ShowDialog(this)!=true)return;
            try
            {
                if(new FileInfo(picker.FileName).Length>SettingsTransfer.MaximumBytes)throw new InvalidDataException("The backup is larger than 512 KB.");
                var reviewed=SettingsTransfer.Review(File.ReadAllText(picker.FileName),_host.Settings);
                if(ReviewImport(reviewed)){SetTheme(_host.Settings.Theme);Navigate("Backup");}
            }
            catch(InvalidDataException e){result.Text=e.Message;}
            catch(Exception e)when(e is IOException or UnauthorizedAccessException){result.Text="The backup could not be read. Your current settings are unchanged.";}
        }));
        right.Children.Add(Text("A deliberate fresh start",22,"Ink",12));
        right.Children.Add(Text("Import replaces your profiles, outfits and preferences. Review each executable path before selecting its rule. App guard, notification dismissal, schedules and focused-control avoidance stay off after import; enable them separately when ready.",14,"Muted",16));
        right.Children.Add(Text("Your current session, history, usage totals, monitor spots and Windows startup setting stay on this PC. Settings from newer, unsupported formats are rejected.",12,"Muted",16));
        right.Children.Add(Button("Back to settings",()=>Navigate("Settings")));
    }
    private bool ReviewImport(Preferences imported)
    {
        var dialog=new Window{Owner=this,Title="Review settings import",Width=640,Height=650,MinHeight=400,WindowStartupLocation=WindowStartupLocation.CenterOwner};
        dialog.SourceInitialized+=(_,_)=>ApplyTitleBar(dialog);
        var root=new DockPanel{Margin=new Thickness(24)};dialog.Content=root;
        var heading=new StackPanel();heading.Children.Add(Text("Make these settings yours.",24,"Ink",12));
        heading.Children.Add(Text($"{imported.Profiles.Count} profiles · {imported.Outfits.Count} outfits. This replaces your current configuration. Every rule starts unchecked; select only apps you recognize on this PC.",13,"Muted",16));
        DockPanel.SetDock(heading,Dock.Top);root.Children.Add(heading);
        var footer=new StackPanel();footer.Children.Add(Text("Protection stays off after import. Your current session and Windows startup setting are preserved.",12,"Muted",10));
        footer.Children.Add(Row(Button("Cancel",()=>dialog.DialogResult=false),Button("Import reviewed settings",()=>{_host.ImportSettings(imported);dialog.DialogResult=true;},true)));
        DockPanel.SetDock(footer,Dock.Bottom);root.Children.Add(footer);
        var items=new StackPanel();root.Children.Add(new ScrollViewer{Content=items,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});
        foreach(var profile in imported.Profiles)
        {
            items.Children.Add(Text(profile.Name,17,"Ink",10));
            if(profile.Rules.Count==0)items.Children.Add(Text("No app rules in this profile.",12,"Muted",12));
            foreach(var rule in profile.Rules)
            {
                bool available=File.Exists(rule.Path)&&DesktopApps.Selectable(rule.Path);
                var toggle=Check(rule.Name,false,enable=>imported=imported with{Profiles=imported.Profiles.Select(p=>p.Id==profile.Id?p with{Rules=p.Rules.Select(r=>r.Path==rule.Path?r with{Enabled=enable}:r).ToList()}:p).ToList()});
                toggle.IsEnabled=available;items.Children.Add(toggle);
                items.Children.Add(Text(rule.Path+"\n"+(rule.Action==AppRuleAction.CloseWindow?"Normal window close":"Reminder only")+" · "+rule.CloseDelaySeconds+" s grace · "+rule.Scope+(available?"":"\nNot available here. Add this app again from App guard."),11,"Muted",16));
            }
        }
        return dialog.ShowDialog()==true;
    }
}
