using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CuteCat.Core;

namespace CuteCat.App;

public static partial class QualityChecks
{
    public static async Task AppGuardFlow(CompanionHost host,MainWindow window,string dir,string fixture,bool simulatedForeground=false)
    {
        Directory.CreateDirectory(dir);
        // First inspect the exact same adapter on the supplied Android Studio process without closing it.
        foreach(string processName in new[]{"studio64","ChatGPT"})
        {
          foreach(var studio in Process.GetProcessesByName(processName))
          {
            using(studio)
            {
                IntPtr handle=studio.MainWindowHandle;string? path=handle==IntPtr.Zero?null:studio.MainModule?.FileName;
                if(path is not null&&handle!=IntPtr.Zero)
                {
                    var adapter=new DesktopApps(()=>handle);
                    var result=await Task.Run(()=>adapter.Find(new(){AppGuard=true,AppRules=[new(path,"Selected application")]},false,FrameClock.Now,0,0));
                    File.WriteAllText(Path.Combine(dir,processName+"-detection.json"),JsonSerializer.Serialize(new{readOnly=true,found=result is not null,result?.CanClose,result?.CaptionSource,button=result?.Target.Button,status=adapter.LastScan}));
                }
            }
          }
        }
        foreach(string kind in new[]{"standard","custom","dialog"})
        {
            bool custom=kind=="custom";
            var stalledNotification=new TaskCompletionSource<NotificationTarget?>(TaskCreationOptions.RunContinuationsAsynchronously);int scans=0;
            host.TestNotificationScan=(_,_)=>{scans++;return stalledNotification.Task;};
            host.Update(new(){IdleNaps=false,Notifications=true});
            for(int i=0;i<20&&scans==0;i++)await Task.Delay(25);
            Check("notification scan is pending before app selection · "+kind,scans>0);
            string folder=Path.Combine(dir,kind+"-frame");Directory.CreateDirectory(folder);
            using var process=Process.Start(new ProcessStartInfo(fixture){Arguments=(kind=="dialog"?"--dialog-window":"--app-window")+" \""+folder+"\""+(custom?" --custom-caption":""),UseShellExecute=true,WindowStyle=ProcessWindowStyle.Hidden})!;
            try
            {
                for(int i=0;i<50&&!File.Exists(Path.Combine(folder,"ready"));i++)await Task.Delay(100);
                IntPtr hwnd=IntPtr.Zero;Native.EnumWindows((h,_)=>{Native.GetWindowThreadProcessId(h,out uint pid);if(pid==process.Id&&Native.IsWindowVisible(h))hwnd=h;return true;},IntPtr.Zero);
                Check("test window created · "+kind,hwnd!=IntPtr.Zero);
                host.Menu.Close();host.Update(host.Settings with{AppGuard=false});host.TestForegroundWindow=null;host.ExerciseDesktopEnvironment=true;host.TestIdleSeconds=0;window.Show();window.Navigate("App guard");window.UpdateLayout();
                var toggle=Descendants<CheckBox>(window).Single(c=>c.Content?.ToString()=="Enable app guard");toggle.IsChecked=true;
                Descendants<Button>(window).Single(b=>b.Content?.ToString()=="Add an app").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Window? picker=null;
                for(int i=0;i<60&&picker is null;i++){await Task.Delay(50);picker=Application.Current.Windows.OfType<Window>().FirstOrDefault(w=>w.Owner==window&&w.Title=="Choose a distracting app");}
                if(picker is null)throw new InvalidOperationException("App picker did not open.");
                var apps=Descendants<ComboBox>(picker).Single();apps.SelectedItem=apps.Items.OfType<DesktopApp>().FirstOrDefault(a=>string.Equals(a.Path,fixture,StringComparison.OrdinalIgnoreCase));
                Check("running app is available through the actual picker · "+kind,apps.SelectedItem is DesktopApp);
                Descendants<Button>(picker).Single(b=>b.Content?.ToString()=="Add app").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                await Task.Delay(250);
                Check("UI add and enable apply an active exact-path rule · "+kind,host.Settings.AppGuard&&host.CurrentProfile.Rules.Any(r=>r.Enabled&&r.Path==fixture));
                window.Hide();Native.SetForegroundWindow(hwnd);await Task.Delay(150);
                if(simulatedForeground)host.TestForegroundWindow=hwnd;
                bool real=Native.GetForegroundWindow()==hwnd;
                // No injected foreground identity in this end-to-end check.
                double deadline=FrameClock.Now+9;string source="None";
                while(FrameClock.Now<deadline&&!process.HasExited){real|=Native.GetForegroundWindow()==hwnd;source=host.CurrentApp?.CaptionSource??source;await Task.Delay(50);}
                Check((simulatedForeground?"explicit fixture foreground override · ":"actual foreground belongs to the controlled app · ")+kind,simulatedForeground?host.TestForegroundWindow==hwnd:real);
                Check("actual UI-configured app closes after paw contact · "+kind,process.HasExited&&host.LastJourney?.Dismissed==true,new{source,host.AppGuardStatus,host.AppScanStatus,host.LastJourney});
                if(custom)Check("custom frame uses native close hit-testing",source=="NativeHitTest");
                Check("only one pending notification scan is kept · "+kind,scans==1);
            }
            finally{host.ExerciseDesktopEnvironment=false;host.Update(new(){Quiet=true});stalledNotification.TrySetResult(null);host.TestNotificationScan=null;File.WriteAllText(Path.Combine(folder,"quit"),"");await Task.WhenAny(process.WaitForExitAsync(),Task.Delay(3000));}
        }
        foreach(string mode in new[]{"--veto","--no-close","--modal-look"})
        {
            string folder=Path.Combine(dir,mode[2..]);Directory.CreateDirectory(folder);
            using var process=Process.Start(new ProcessStartInfo(fixture){Arguments=(mode=="--modal-look"?"--dialog-window":"--app-window")+" \""+folder+"\" --custom-caption "+mode,UseShellExecute=true,WindowStyle=ProcessWindowStyle.Hidden})!;
            try
            {
                for(int i=0;i<40&&!File.Exists(Path.Combine(folder,"ready"));i++)await Task.Delay(100);
                IntPtr handle=IntPtr.Zero;Native.EnumWindows((h,_)=>{Native.GetWindowThreadProcessId(h,out uint pid);if(pid==process.Id&&Native.IsWindowVisible(h))handle=h;return true;},IntPtr.Zero);
                host.Update(new(){IdleNaps=false,Quiet=true,AppGuard=true,AppRules=[new(fixture,"Safety fixture")]});host.TestForegroundWindow=handle;
                if(mode=="--veto")
                {
                    for(int i=0;i<160&&!File.Exists(Path.Combine(folder,"close-requests.txt"));i++)await Task.Delay(50);
                    await Task.Delay(1800);
                    Check("custom-frame refusal or save prompt is left open after one normal close",!process.HasExited&&File.Exists(Path.Combine(folder,"close-requests.txt"))&&File.ReadAllText(Path.Combine(folder,"close-requests.txt"))=="1");
                }
                else
                {
                    await Task.Delay(1600);
                    Check(mode=="--modal-look"?"standalone message/save-style dialog is not targeted":"disabled custom-frame close command is not targeted",!process.HasExited&&!File.Exists(Path.Combine(folder,"close-requests.txt"))&&host.Attempt.Target is null);
                }
            }
            finally{host.TestForegroundWindow=null;host.Update(new(){Quiet=true});File.WriteAllText(Path.Combine(folder,"quit"),"");await Task.WhenAny(process.WaitForExitAsync(),Task.Delay(3000));}
        }
        var pending=new TaskCompletionSource<NotificationTarget?>(TaskCreationOptions.RunContinuationsAsynchronously);int pendingEpoch=-1;
        host.TestNotificationScan=(_,epoch)=>{pendingEpoch=epoch;return pending.Task;};host.Update(new(){Quiet=true,Notifications=true});
        for(int i=0;i<30&&pendingEpoch<0;i++)await Task.Delay(25);
        host.Update(host.Settings with{Notifications=false});
        pending.SetResult(new("cancelled-shell-result",0,0,new Area(host.Cat.WorkArea.Right-80,host.Cat.WorkArea.Top+100,40,30),FrameClock.Now,pendingEpoch));
        host.TestNotificationScan=(_,_)=>Task.FromResult<NotificationTarget?>(null);host.Update(host.Settings with{Notifications=true});await Task.Delay(450);
        Check("late notification observations from a cancelled epoch cannot start a paw",host.Attempt.Target is null);
        host.TestNotificationScan=null;host.Update(new(){Quiet=true});
        File.WriteAllText(Path.Combine(dir,"app-guard-checks.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,endToEndForegroundInjected=simulatedForeground,safetyFixtureForegroundInjected=true,checks=Checks},new JsonSerializerOptions{WriteIndented=true}));
    }
}
