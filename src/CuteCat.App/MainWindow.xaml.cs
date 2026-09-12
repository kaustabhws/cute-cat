using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using CuteCat.Core;
using Forms=System.Windows.Forms;

namespace CuteCat.App;

public partial class MainWindow : Window
{
    private readonly CompanionHost _host;
    private readonly CatPreview _preview=new();
    private readonly Dictionary<string,Button> _tabs=[];
    private TextBlock? _timer,_sessionLabel,_summary,_notificationLabel,_catLabel,_testLabel,_guardLabel;
    private Button? _primary,_end;
    private int _minutes=25;
    private double _lastPreview;
    private string _current="Focus";
    public MainWindow(CompanionHost host)
    {
        InitializeComponent();_host=host;
        foreach(string name in new[]{"Focus","Your cat","App guard","Quiet desktop","Settings"})
        {var button=Button(name,()=>Navigate(name));button.MinHeight=34;button.Padding=new Thickness(13,7,13,7);button.Margin=new Thickness(4,0,0,0);button.BorderThickness=new Thickness(0);Navigation.Children.Add(button);_tabs[name]=button;}
        SetTheme(_host.Settings.Theme);Navigate("Focus");
        host.Frame+=UpdatePreview;host.Changed+=Refresh;
        Closing+=OnClosing;
        IsVisibleChanged+=(_,_)=>{if(IsVisible){_lastPreview=0;Refresh();}};
        Closed+=(_,_)=>{host.Frame-=UpdatePreview;host.Changed-=Refresh;_preview.Dispose();};
    }
    private void OnClosing(object? sender,CancelEventArgs e) { e.Cancel=true;Hide(); }
    public void Navigate(string name)
    {
        if(_preview.Parent is Panel old)old.Children.Remove(_preview);
        if(_preview.Parent is Border border)border.Child=null;
        _current=name;Page.Children.Clear();Page.ColumnDefinitions.Clear();Page.RowDefinitions.Clear();
        _timer=null;_sessionLabel=null;_summary=null;_notificationLabel=null;_primary=null;_end=null;_catLabel=null;_testLabel=null;_guardLabel=null;
        foreach(var (key,button) in _tabs)button.SetResourceReference(BackgroundProperty,key==name?"AccentSoft":"Paper");
        switch(name){case "Focus":FocusPage();break;case "Your cat":CatPage();break;case "App guard":AppsPage();break;case "Quiet desktop":NotificationsPage();break;default:SettingsPage();break;}
        Refresh();_lastPreview=0;UpdatePreview(FrameClock.Now);
    }
    private static TextBlock Text(string text,double size=14,string color="Ink",double bottom=0)
    {var t=new TextBlock{Text=text,FontSize=size,Margin=new Thickness(0,0,0,bottom)};t.SetResourceReference(TextBlock.ForegroundProperty,color);return t;}
    private static new TextBlock Title(string text) {var t=Text(text,31,"Ink",13);t.FontFamily=new FontFamily("Georgia");t.LineHeight=38;return t;}
    private static TextBlock Eyebrow(string text)=>Text(text.ToUpperInvariant(),10,"Muted",15);
    private static Button Button(string label,Action action,bool primary=false)
    {var b=new Button{Content=label};if(primary)b.SetResourceReference(StyleProperty,"Primary");b.Click+=(_,_)=>action();return b;}
    private static CheckBox Check(string label,bool value,Action<bool> action)
    {var c=new CheckBox{Content=label,IsChecked=value};c.Click+=(_,_)=>action(c.IsChecked==true);return c;}
    private static ComboBox Choice<T>(string name,(T value,string label)[] choices,T selected,Action<T> change)
    {
        var combo=new ComboBox{Margin=new Thickness(0,0,0,12)};
        foreach(var choice in choices){var item=new ComboBoxItem{Content=choice.label,Tag=choice.value};combo.Items.Add(item);if(EqualityComparer<T>.Default.Equals(choice.value,selected))combo.SelectedItem=item;}
        System.Windows.Automation.AutomationProperties.SetName(combo,name);
        combo.SelectionChanged+=(_,_)=>{if(combo.SelectedItem is ComboBoxItem{Tag:T value})change(value);};return combo;
    }
    private static WrapPanel Row(params UIElement[] items)
    {var row=new WrapPanel{Margin=new Thickness(0,0,0,14)};foreach(var i in items){if(i is FrameworkElement f)f.Margin=new Thickness(0,0,8,8);row.Children.Add(i);}return row;}
    private (StackPanel left,StackPanel right) Columns()
    {
        Page.ColumnDefinitions.Add(new(){Width=new GridLength(1.12,GridUnitType.Star)});Page.ColumnDefinitions.Add(new(){Width=new GridLength(30)});Page.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        var left=new StackPanel();var right=new StackPanel();Grid.SetColumn(right,2);Page.Children.Add(left);Page.Children.Add(right);return(left,right);
    }
    private void PreviewCard(StackPanel parent,string top,string caption)
    {
        var content=new StackPanel{Margin=new Thickness(19,18,19,18)};
        content.Children.Add(Eyebrow(top));_preview.Height=252;content.Children.Add(_preview);
        _catLabel=Text(caption,12,"Muted");_catLabel.TextAlignment=TextAlignment.Center;content.Children.Add(_catLabel);
        var card=new Border{CornerRadius=new CornerRadius(22),Child=content,Margin=new Thickness(0,6,0,17)};card.SetResourceReference(Border.BackgroundProperty,"Panel");parent.Children.Add(card);
    }
    private void FocusPage()
    {
        var(left,right)=Columns();left.Children.Add(Eyebrow("A little company for your day"));left.Children.Add(Title("One thing at a time."));
        left.Children.Add(Text("Settle into a small stretch of focus.\n"+_host.Settings.Nickname+" will keep you company.",14,"Muted",18));
        _sessionLabel=Text("READY WHEN YOU ARE",10,"Muted",2);left.Children.Add(_sessionLabel);
        _timer=Text("25:00",64,"Ink",11);_timer.FontWeight=FontWeights.Light;left.Children.Add(_timer);
        var custom=new TextBox{Text=_minutes.ToString(),Width=52,MaxLength=3,VerticalContentAlignment=VerticalAlignment.Center};
        System.Windows.Automation.AutomationProperties.SetName(custom,"Custom focus minutes, 5 to 180");
        left.Children.Add(Row(Button("25 min",()=>{_minutes=25;custom.Text="25";Refresh();}),Button("50 min",()=>{_minutes=50;custom.Text="50";Refresh();}),custom,Text("min",12,"Muted")));
        custom.LostFocus+=(_,_)=>{if(int.TryParse(custom.Text,out int m)&&m>=5&&m<=180)_minutes=m;else custom.Text=_minutes.ToString();Refresh();};
        _primary=Button("Start focusing",()=>
        {
            double now=FrameClock.Now;
            if(_host.Session.Status==SessionStatus.Running)_host.Session.Pause(now);
            else if(_host.Session.Status==SessionStatus.Paused)_host.Session.Resume(now);
            else {if(int.TryParse(custom.Text,out int m)&&m>=5&&m<=180)_minutes=m;_host.Session.Start(_minutes,now);}
            _host.Configure();_host.Save();Refresh();
        },true);
        _end=Button("End session",()=>{_host.Session.End();_host.Configure();_host.Save();Refresh();});
        left.Children.Add(Row(_primary,_end));
        left.Children.Add(Button("Take a 5-minute break",()=>{if(_host.Session.Status is SessionStatus.Running or SessionStatus.Paused)_host.Session.End();_host.Session.Start(5,FrameClock.Now,true);_host.Configure();_host.Save();Refresh();}));
        _summary=Text("",12,"Muted");_summary.Margin=new Thickness(0,23,0,0);left.Children.Add(_summary);
        PreviewCard(right,"Your desk companion","A calm little presence, just for you.");
        right.Children.Add(Text("A companion, at your pace.",16,"Ink",8));
        right.Children.Add(Text("Pet to say hello. Drag to move. Your cat rests during focus, and wanders when you're ready for a break.",13,"Muted",12));
        right.Children.Add(Text("Choose distracting desktop apps in App guard. Browser URL rules will come later.",11,"Muted"));
        right.Children.Add(Button("Choose distracting apps",()=>Navigate("App guard")));
    }
    private void CatPage()
    {
        var(left,right)=Columns();left.Children.Add(Eyebrow("Meet your little companion"));left.Children.Add(Title("Small paws.\nBig personality."));
        left.Children.Add(Text("A curious little oat-coloured cat, with nowhere urgent to be.",14,"Muted",20));
        left.Children.Add(Text("Call your cat",12,"Muted",7));var nickname=new TextBox{Text=_host.Settings.Nickname,MaxLength=24};
        System.Windows.Automation.AutomationProperties.SetName(nickname,"Cat nickname");nickname.LostFocus+=(_,_)=>_host.Update(_host.Settings with{Nickname=nickname.Text});nickname.Margin=new Thickness(0,0,0,17);left.Children.Add(nickname);
        left.Children.Add(Text("Say hello",12,"Muted",8));
        left.Children.Add(Row(Button("Walk",()=>_host.Perform(CatAction.Walk)),Button("Run",()=>_host.Perform(CatAction.Run)),Button("Meow",()=>_host.Perform(CatAction.Meow))));
        left.Children.Add(Row(Button("Groom",()=>_host.Perform(CatAction.Groom)),Button("Play",()=>_host.Perform(CatAction.Play)),Button("Sleep",()=>_host.Perform(CatAction.Sleep)),Button("Wake",()=>_host.Perform(CatAction.Wake)),Button("Turn around",()=>_host.Perform(CatAction.Turn))));
        left.Children.Add(Check("Quiet company · stay nearby",_host.Settings.Quiet,v=>_host.Update(_host.Settings with{Quiet=v})));
        left.Children.Add(Row(Button("Show cat",()=>_host.ShowCat(true)),Button("Hide cat",()=>_host.ShowCat(false)),Button("Park",()=>_host.Park())));
        PreviewCard(right,"Entirely drawn in code","Breathing, blinking, and being a cat.");
        right.Children.Add(Text("Dress up",16,"Ink",8));
        right.Children.Add(Choice("Pet accessory",new[]{(PetAccessory.None,"Just my cat"),(PetAccessory.Bandana,"Soft bandana"),(PetAccessory.BowTie,"Little bow tie"),(PetAccessory.BellCollar,"Bell collar"),(PetAccessory.Flower,"Daisy bloom")},_host.Settings.Accessory,v=>_host.Update(_host.Settings with{Accessory=v})));
        right.Children.Add(Choice("Accessory colour",new[]{("Sage","Sage green"),("Rose","Dusty rose"),("Sky","Cloud blue"),("Plum","Soft lavender"),("Honey","Warm honey")},_host.Settings.AccessoryColor,v=>_host.Update(_host.Settings with{AccessoryColor=v})));
        left.Children.Add(Text("Personality",12,"Muted",8));
        left.Children.Add(Choice("Cat activity",new[]{(ActivityLevel.Calm,"Calm · more grooming, gentle walks"),(ActivityLevel.Balanced,"Curious · a little of everything"),(ActivityLevel.Playful,"Playful · more runs and little hops")},_host.Settings.Activity,v=>_host.Update(_host.Settings with{Activity=v})));
        left.Children.Add(Check("Nap when I'm away",_host.Settings.IdleNaps,v=>_host.Update(_host.Settings with{IdleNaps=v})));
        left.Children.Add(Choice("Idle time before napping",new[]{(1,"After 1 minute idle"),(3,"After 3 minutes idle"),(5,"After 5 minutes idle"),(10,"After 10 minutes idle"),(15,"After 15 minutes idle")},_host.Settings.IdleMinutes,v=>_host.Update(_host.Settings with{IdleMinutes=v})));
        left.Children.Add(Text("Your cat curls up with a tiny nose bubble, then stretches awake when you return. Focus mode and quiet company keep activity gentle.",12,"Muted",12));
    }
    private void NotificationsPage()
    {
        var(left,right)=Columns();left.Children.Add(Eyebrow("A quieter desktop"));left.Children.Add(Title("Let a paw handle it."));
        left.Children.Add(Text("When a supported Windows banner arrives, your cat runs to its close button and gives it a gentle tap.",14,"Muted",16));
        left.Children.Add(Check("Dismiss Windows banners with a paw",_host.Settings.Notifications,v=>_host.Update(_host.Settings with{Notifications=v})));
        left.Children.Add(Text("Turning this on allows Cute Cat to dismiss supported visible Windows notifications. It looks only for their close controls; notification messages are not read or saved.",12,"Muted",16));
        _notificationLabel=Text("",12,"Ink",15);left.Children.Add(_notificationLabel);
        left.Children.Add(Row(Button("Try a practice card",()=>_host.StartPractice(),true),Button("Send Windows test",()=>_host.WindowsTest())));
        _testLabel=Text("",12,"Muted",12);left.Children.Add(_testLabel);
        left.Children.Add(Text("Practice uses a card owned by Cute Cat. The Windows test sends a real banner; turn the helper on to try its dismissal.",12,"Muted",16));
        left.Children.Add(Text("Some banners use a different layout. Those are left alone. The helper rests while the cat is hidden, on a full-screen app, or with reduced motion.",12,"Muted"));
        PreviewCard(right,"One small gesture","Ready for a little practice?");
        right.Children.Add(Text("Always yours to control",16,"Ink",8));right.Children.Add(Text("Pet, move or hide your cat to cancel an approach. Notification paws and App guard are separate choices. Browser tab rules are not enabled in this version.",13,"Muted"));
    }
    private void AppsPage()
    {
        var stack=new StackPanel();Page.Children.Add(stack);
        stack.Children.Add(Eyebrow("A focus buddy with small paws"));stack.Children.Add(Title("Less temptation. More focus."));
        stack.Children.Add(Text("Choose the desktop apps that pull you away. Your cat can give a stern little reminder, or run over and tap the window's close button.",14,"Muted",12));
        stack.Children.Add(Check("Enable app guard",_host.Settings.AppGuard,v=>_host.Update(_host.Settings with{AppGuard=v})));
        _guardLabel=Text(_host.AppGuardStatus,12,"Muted",14);stack.Children.Add(_guardLabel);
        stack.Children.Add(Row(Button("Add an app",AddAppRule,true),Button("Pause for 10 minutes",()=>_host.PauseGuard(10)),Button("Resume",()=>_host.PauseGuard(0))));
        if(_host.Settings.AppRules.Count==0)
            stack.Children.Add(Text("No apps selected yet. Add a desktop app, choose how your cat responds, then enable app guard.",14,"Muted",20));
        foreach(var rule in _host.Settings.AppRules)
        {
            var content=new StackPanel{Margin=new Thickness(16)};
            var header=new DockPanel();var remove=Button("Remove",()=>{_host.Update(_host.Settings with{AppRules=_host.Settings.AppRules.Where(r=>r.Path!=rule.Path).ToList()});Navigate("App guard");});
            remove.MinHeight=30;remove.Padding=new Thickness(10,4,10,4);DockPanel.SetDock(remove,Dock.Right);header.Children.Add(remove);
            var title=Text(rule.Name,16);title.FontWeight=FontWeights.SemiBold;header.Children.Add(title);content.Children.Add(header);
            content.Children.Add(Text(rule.Path,11,"Muted",8));
            void Change(Func<AppRule,AppRule> update)=>_host.Update(_host.Settings with{AppRules=_host.Settings.AppRules.Select(r=>r.Path==rule.Path?update(r):r).ToList()});
            content.Children.Add(Check("Protect against this app",rule.Enabled,v=>Change(r=>r with{Enabled=v})));
            var row=new Grid();row.ColumnDefinitions.Add(new());row.ColumnDefinitions.Add(new(){Width=new GridLength(14)});row.ColumnDefinitions.Add(new());
            var action=Choice("Response for "+rule.Name,new[]{(AppRuleAction.CloseWindow,"Angry paw · close the window"),(AppRuleAction.Remind,"Gentle reminder · leave it open")},rule.Action,v=>Change(r=>r with{Action=v}));
            var scope=Choice("When to protect against "+rule.Name,new[]{(AppRuleScope.Always,"Whenever app guard is on"),(AppRuleScope.DuringFocus,"Only during a focus session")},rule.Scope,v=>Change(r=>r with{Scope=v}));
            row.Children.Add(action);Grid.SetColumn(scope,2);row.Children.Add(scope);content.Children.Add(row);
            var card=new Border{CornerRadius=new CornerRadius(14),Child=content,Margin=new Thickness(0,0,0,12),BorderThickness=new Thickness(1)};
            card.SetResourceReference(Border.BackgroundProperty,"Panel");card.SetResourceReference(Border.BorderBrushProperty,"Line");stack.Children.Add(card);
        }
        stack.Children.Add(Text("Only selected executable files are matched. No window titles, typing or page content are read. A paw requests a normal close; save prompts and apps that refuse to close stay under your control. It never force-kills an app.",12,"Muted",8));
        stack.Children.Add(Text("Paw actions pause while the cat is hidden or reduced motion is on. A supported close button must be on the cat's selected monitor. Windows/system apps are excluded. Browser URL rules will come later.",12,"Muted"));
    }
    private async void AddAppRule()
    {
        var apps=await Task.Run(DesktopApps.OpenApps);
        var dialog=new Window{Owner=this,Title="Choose a distracting app",Width=510,Height=350,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner};
        var content=new StackPanel{Margin=new Thickness(24)};dialog.Content=content;
        content.Children.Add(Text("Which app gets in the way?",23,"Ink",14));content.Children.Add(Text("Pick an open desktop app, or browse to its executable.",13,"Muted",15));
        var select=new ComboBox{ItemsSource=apps,DisplayMemberPath="Name",Margin=new Thickness(0,0,0,12)};
        System.Windows.Automation.AutomationProperties.SetName(select,"Open desktop apps");content.Children.Add(select);
        var detail=Text("",11,"Muted",12);content.Children.Add(detail);DesktopApp? chosen=null;
        select.SelectionChanged+=(_,_)=>{chosen=select.SelectedItem as DesktopApp;detail.Text=chosen?.Path??"";};
        content.Children.Add(Row(Button("Browse .exe",()=>
        {
            var picker=new OpenFileDialog{Title="Choose a desktop app",Filter="Desktop applications (*.exe)|*.exe",CheckFileExists=true};
            if(picker.ShowDialog(dialog)!=true)return;
            if(!DesktopApps.Selectable(picker.FileName)){detail.Text="Choose a desktop app outside Windows' system folder. Cute Cat itself cannot be selected.";return;}
            chosen=new(picker.FileName,Path.GetFileNameWithoutExtension(picker.FileName));detail.Text=chosen.Path;
        }),Button("Add app",()=>
        {
            if(chosen is null){detail.Text="Choose an app first.";return;}
            _host.Update(_host.Settings with{AppRules=_host.Settings.AppRules.Where(r=>!string.Equals(r.Path,chosen.Path,StringComparison.OrdinalIgnoreCase)).Append(new AppRule(chosen.Path,chosen.Name)).ToList()});
            dialog.DialogResult=true;
        },true)));
        dialog.ShowDialog();Navigate("App guard");
    }
    private void SettingsPage()
    {
        var(left,right)=Columns();left.Children.Add(Eyebrow("Make yourself at home"));left.Children.Add(Title("The little details."));
        left.Children.Add(Text("Appearance",12,"Muted",8));
        left.Children.Add(Row(Button("Light",()=>Theme("Light")),Button("Dark",()=>Theme("Dark")),Button("System",()=>Theme("System"))));
        left.Children.Add(Text("Cat size",12,"Muted",8));left.Children.Add(Row(Button("Small",()=>_host.Update(_host.Settings with{Size=96})),Button("Medium",()=>_host.Update(_host.Settings with{Size=128})),Button("Large",()=>_host.Update(_host.Settings with{Size=160}))));
        left.Children.Add(Text("Your cat's monitor",12,"Muted",8));
        var monitor=new ComboBox();foreach(var screen in Forms.Screen.AllScreens)monitor.Items.Add(screen.DeviceName);
        monitor.SelectedItem=Forms.Screen.AllScreens.FirstOrDefault(s=>s.DeviceName==_host.Settings.Monitor)?.DeviceName??Forms.Screen.PrimaryScreen?.DeviceName;
        System.Windows.Automation.AutomationProperties.SetName(monitor,"Cat monitor");monitor.SelectionChanged+=(_,_)=>{if(monitor.SelectedItem is string device){_host.Update(_host.Settings with{Monitor=device});_host.Park();}};
        left.Children.Add(monitor);
        left.Children.Add(Check("Reduce motion",_host.Settings.ReducedMotion,v=>_host.Update(_host.Settings with{ReducedMotion=v})));
        left.Children.Add(Check("Follow Windows animation preference",_host.Settings.FollowWindowsMotion,v=>_host.Update(_host.Settings with{FollowWindowsMotion=v})));
        left.Children.Add(Check("Quiet meow sounds",_host.Settings.Sounds,v=>_host.Update(_host.Settings with{Sounds=v})));
        left.Children.Add(Check("Start with Windows",_host.Settings.Startup,v=>SetStartup(v)));
        right.Children.Add(Eyebrow("A small app, a local home"));
        right.Children.Add(Text("No account. No subscription.",20,"Ink",12));right.Children.Add(Text("Your settings and recent focus sessions stay in your Windows profile. There is no cloud service or image generator running behind your cat.",14,"Muted",22));
        right.Children.Add(Text("Cute Cat · "+BuildInfo.Version,13,"Ink",8));right.Children.Add(Text("Original vector artwork and continuous animation. Native Windows controls. Completely free.",13,"Muted",20));
        right.Children.Add(Button("Clear focus history",()=>
        {if(MessageBox.Show(this,"Clear the focus history stored by Cute Cat? Your cat settings will stay.","Clear focus history",MessageBoxButton.OKCancel,MessageBoxImage.Question)==MessageBoxResult.OK){_host.History.Clear();_host.Save();Refresh();}}));
        right.Children.Add(Text("Closing this window keeps your cat in the notification area. Use Quit to send it home.",12,"Muted",16));
        right.Children.Add(Button("Quit Cute Cat",()=>Application.Current.Shutdown()));
        if(_host.Store.Notice is string notice)right.Children.Add(Text(notice,12,"Muted"));
    }
    private void SetStartup(bool enable)
    {
        try
        {
            using var run=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if(enable)run.SetValue("CuteCat","\""+Environment.ProcessPath+"\" --tray");else run.DeleteValue("CuteCat",false);
            _host.Update(_host.Settings with{Startup=enable});
        }
        catch(Exception e) when(e is UnauthorizedAccessException or IOException){MessageBox.Show(this,"Windows did not allow the startup setting to change.","Cute Cat");Navigate("Settings");}
    }
    private void Theme(string name) { SetTheme(name);_host.Update(_host.Settings with{Theme=name}); }
    public void SetTheme(string name)
    {
        bool dark=name=="Dark";
        if(name=="System") {using var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");dark=key?.GetValue("AppsUseLightTheme") is int v&&v==0;}
        string[] keys=["Paper","Panel","Ink","Muted","Line","Accent","AccentSoft","ButtonInk"];
        string[] colors=dark?["#202B25","#2C382F","#F2F0E2","#B9C4B6","#455347","#B7CEA9","#394D3E","#202F24"]:["#F8F7F0","#EFEEE3","#293C32","#626D60","#DADDD0","#365841","#E0E8D9","#FAFAF4"];
        for(int i=0;i<keys.Length;i++)Application.Current.Resources[keys[i]]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i]));
        if(SystemParameters.HighContrast)
        {
            Application.Current.Resources["Paper"]=SystemColors.WindowBrush;Application.Current.Resources["Panel"]=SystemColors.ControlBrush;
            Application.Current.Resources["Ink"]=SystemColors.WindowTextBrush;Application.Current.Resources["Muted"]=SystemColors.WindowTextBrush;
            Application.Current.Resources["Accent"]=SystemColors.HighlightBrush;Application.Current.Resources["ButtonInk"]=SystemColors.HighlightTextBrush;
            Application.Current.Resources["Line"]=SystemColors.WindowTextBrush;Application.Current.Resources["AccentSoft"]=SystemColors.ControlBrush;
        }
    }
    public void Refresh()
    {
        if(!IsVisible)return;
        var session=_host.Session;
        if(_timer is not null)
        {
            double remaining=session.Status==SessionStatus.Ready?_minutes*60:session.Remaining;
            int seconds=(int)Math.Ceiling(remaining);_timer.Text=$"{seconds/60:00}:{seconds%60:00}";
            _sessionLabel!.Text=session.Status switch{SessionStatus.Running=>session.IsBreak?"TAKE A BREATH":"A LITTLE TIME FOR YOURSELF",SessionStatus.Paused=>"PAUSED · RESUME WHEN YOU'RE READY",SessionStatus.Completed=>"NICELY DONE. TAKE A BREATH.",_=>"READY WHEN YOU ARE"};
            _primary!.Content=session.Status switch{SessionStatus.Running=>"Pause",SessionStatus.Paused=>"Resume",_=>"Start focusing"};
            _end!.Visibility=session.Status is SessionStatus.Running or SessionStatus.Paused?Visibility.Visible:Visibility.Collapsed;
            int today=_host.History.Where(r=>r.Finished.LocalDateTime.Date==DateTime.Today).Sum(r=>r.Seconds)/60;
            int week=_host.History.Where(r=>r.Finished>=DateTimeOffset.Now.AddDays(-7)).Sum(r=>r.Seconds)/60;
            _summary!.Text=$"{today} minutes today  ·  {week} this week";
        }
        if(_notificationLabel is not null)_notificationLabel.Text=_host.NotificationStatus;
        if(_guardLabel is not null)_guardLabel.Text=_host.AppGuardStatus;
        if(_testLabel is not null)_testLabel.Text=_host.TestNotification.Result switch
        {
            "NotSent"=>"",
            "Sent"=>"Test sent to Windows. Do not disturb or a full-screen app can keep its banner out of sight.",
            "UserCanceled"=>"Windows confirmed that the test notification was dismissed.",
            "TimedOut"=>"The test banner expired before it was dismissed.",
            "Elevated"=>"Windows cannot show this test from an administrator process. Reopen Cute Cat normally.",
            _=>"Windows did not show the test. Check that notifications for Cute Cat are allowed."
        };
        if(_catLabel is not null)_catLabel.Text=_host.Cat.Hidden?"Your cat is resting out of sight.":_host.Cat.Action switch{CatAction.Walk=>"A little wander around the desk.",CatAction.Run=>"Somewhere very important to be.",CatAction.Groom=>"Keeping those little paws tidy.",CatAction.Sleep=>"Shh. A very small nap.",CatAction.Meow=>"A tiny hello, just for you.",CatAction.Play=>"A little spring in those paws.",_=>"Breathing, blinking, and being a cat."};
        Footer.Text=_host.SaveNotice??(_host.Cat.Hidden?"Cat hidden · timer stays with you":(_host.Brain.AutoSleeping?"Taking a little nap":"Companion here")+" · "+(_host.Settings.AppGuard?"app guard on":"app guard off")+" · "+(_host.Settings.Notifications?"notification paws on":"notification paws off"));
    }
    private void UpdatePreview(double now)
    {
        if(!IsVisible || _preview.Parent is null || now-_lastPreview<1d/30)return;
        _lastPreview=now;_preview.Update(_host.Cat.Pose);
    }
}
