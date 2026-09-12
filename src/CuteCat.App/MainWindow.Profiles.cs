using System.Windows;
using System.Windows.Controls;
using CuteCat.Core;

namespace CuteCat.App;

public partial class MainWindow
{
    private static StackPanel NumberInput(string label,int value,int minimum,int maximum,Action<int> change)
    {
        var panel=new StackPanel{MinWidth=200};panel.Children.Add(Text(label,12,"Muted",6));
        var input=new TextBox{Text=value.ToString(),MaxLength=3,Width=88,HorizontalAlignment=HorizontalAlignment.Left};
        System.Windows.Automation.AutomationProperties.SetName(input,label);
        input.LostFocus+=(_,_)=>{if(int.TryParse(input.Text,out int n)&&n>=minimum&&n<=maximum){value=n;change(n);}else input.Text=value.ToString();};
        panel.Children.Add(input);return panel;
    }
    private void ProfilesPage()
    {
        var(left,right)=Columns();var profile=_host.CurrentProfile;
        left.Children.Add(Eyebrow("Make room for different days"));left.Children.Add(Title("A rhythm that fits."));
        left.Children.Add(Text("Each profile has its own app rules and activity level. Work, study, or take a real break.",14,"Muted",20));
        left.Children.Add(Choice("Focus profile",_host.Settings.Profiles.Select(p=>(p.Id,p.Name)).ToArray(),profile.Id,id=>{_host.SelectProfile(id);Navigate("Profiles");}));
        left.Children.Add(Text("Selecting a profile pauses schedules until you turn them on again.",12,"Muted",12));
        left.Children.Add(Check("Switch profiles on a schedule",_host.Settings.AutomaticProfiles,v=>{_host.Update(_host.Settings with{AutomaticProfiles=v});Navigate("Profiles");}));
        left.Children.Add(Text("Profile name",12,"Muted",6));
        var name=new TextBox{Text=profile.Name,MaxLength=32,Margin=new Thickness(0,0,0,12)};
        System.Windows.Automation.AutomationProperties.SetName(name,"Profile name");
        name.LostFocus+=(_,_)=>_host.EditProfile(profile.Id,p=>p with{Name=name.Text});left.Children.Add(name);
        left.Children.Add(Check("Use app guard in this profile",profile.GuardEnabled,v=>_host.EditProfile(profile.Id,p=>p with{GuardEnabled=v})));
        left.Children.Add(Check("Quiet company during focus",profile.QuietDuringFocus,v=>_host.EditProfile(profile.Id,p=>p with{QuietDuringFocus=v})));
        left.Children.Add(Choice("Profile activity",new[]{(ActivityLevel.Calm,"Calm · gentle company"),(ActivityLevel.Balanced,"Curious · a little variety"),(ActivityLevel.Playful,"Playful · more little adventures")},profile.Activity,v=>_host.EditProfile(profile.Id,p=>p with{Activity=v})));
        left.Children.Add(Button("Edit this profile's app rules",()=>Navigate("App guard"),true));
        var copy=Button("Duplicate profile",()=>
        {
            if(_host.Settings.Profiles.Count>=8)return;
            var next=profile with{Id=Guid.NewGuid().ToString("N"),Name=profile.Name+" copy",Rules=profile.Rules.ToList(),Schedule=new()};
            _host.Update(_host.Settings with{Profiles=_host.Settings.Profiles.Append(next).ToList(),ActiveProfileId=next.Id,AutomaticProfiles=false});Navigate("Profiles");
        });copy.IsEnabled=_host.Settings.Profiles.Count<8;
        var remove=Button("Delete profile",()=>
        {
            if(_host.Settings.Profiles.Count<2)return;
            var remaining=_host.Settings.Profiles.Where(p=>p.Id!=profile.Id).ToList();
            _host.Update(_host.Settings with{Profiles=remaining,ActiveProfileId=remaining[0].Id,AutomaticProfiles=false});Navigate("Profiles");
        });remove.IsEnabled=_host.Settings.Profiles.Count>1;left.Children.Add(Row(copy,remove));
        right.Children.Add(Eyebrow("Weekly schedule · local time"));right.Children.Add(Text("When should "+profile.Name+" take over?",21,"Ink",16));
        var schedule=profile.Schedule??new();bool enabled=schedule.Enabled;int days=schedule.Days,priority=schedule.Priority;
        right.Children.Add(Check("Enable this time window",enabled,v=>enabled=v));
        var dayRow=new WrapPanel{Margin=new Thickness(0,6,0,12)};string[] labels=["M","T","W","T","F","S","S"];
        for(int i=0;i<7;i++)
        {
            int day=i;Button? button=null;
            button=Button(labels[i],()=>{days^=1<<day;button!.SetResourceReference(BackgroundProperty,(days&(1<<day))!=0?"AccentSoft":"Paper");});
            button.Width=37;button.MinWidth=0;button.Padding=new Thickness(0);button.Margin=new Thickness(0,0,4,4);
            button.SetResourceReference(BackgroundProperty,(days&(1<<day))!=0?"AccentSoft":"Paper");
            System.Windows.Automation.AutomationProperties.SetName(button,new[]{"Monday","Tuesday","Wednesday","Thursday","Friday","Saturday","Sunday"}[i]);dayRow.Children.Add(button);
        }
        right.Children.Add(dayRow);
        string Time(int minute)=>$"{minute/60:00}:{minute%60:00}";
        var from=new TextBox{Text=Time(schedule.StartMinute),Width=86,MaxLength=5};var until=new TextBox{Text=Time(schedule.EndMinute),Width=86,MaxLength=5};
        System.Windows.Automation.AutomationProperties.SetName(from,"Schedule start, 24-hour HH:mm");System.Windows.Automation.AutomationProperties.SetName(until,"Schedule end, 24-hour HH:mm");
        right.Children.Add(Text("From / until · 24-hour clock",12,"Muted",6));right.Children.Add(Row(from,Text("to"),until));
        right.Children.Add(NumberInput("Priority when schedules overlap · 0–100",priority,0,100,v=>priority=v));
        var status=Text("",12,"Muted",12);status.Margin=new Thickness(0,14,0,12);right.Children.Add(status);
        right.Children.Add(Button("Save schedule",()=>
        {
            if(!TimeOnly.TryParseExact(from.Text,"HH:mm",out var start)||!TimeOnly.TryParseExact(until.Text,"HH:mm",out var end)||enabled&&days==0)
            {status.Text="Choose at least one day and use a time like 09:00.";return;}
            _host.EditProfile(profile.Id,p=>p with{Schedule=new(enabled,days,start.Hour*60+start.Minute,end.Hour*60+end.Minute,priority)});
            status.Text="Schedule saved.";
        },true));
        right.Children.Add(Text("An overnight window belongs to the day it starts. Equal times cover that entire day. Higher priority wins an overlap; ties use profile order. Outside a window, your selected profile applies. Times follow Windows' local clock, including daylight saving changes.",12,"Muted",12));
        right.Children.Add(Text("App guard's main switch is always yours. A schedule never turns that switch on or starts a focus timer.",12,"Muted"));
    }
}
