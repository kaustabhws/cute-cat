using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using CuteCat.Core;

namespace CuteCat.App;

public static partial class QualityChecks
{
    // App-owned controls and queued dispatcher callbacks, never system input or another app's content.
    public static async Task Responsiveness(CompanionHost host,MainWindow window,string dir)
    {
        Directory.CreateDirectory(dir);Directory.CreateDirectory(Path.Combine(dir,"ui"));
        var rules=Enumerable.Range(0,50).Select(i=>new AppRule(@"C:\CuteCat-QA\NeverRun\Fixture-"+i+".exe","Fixture "+i,AppRuleAction.Remind)).ToList();
        var profiles=Enumerable.Range(0,8).Select(i=>new FocusProfile("profile"+i,"Profile "+i,rules.ToList())).ToList();
        host.Update(new Preferences{Quiet=true,Startup=false,IdleNaps=false,Profiles=profiles,ActiveProfileId="profile0",
            Outfits=Enumerable.Range(0,24).Select(i=>new SavedOutfit("look"+i,"Look "+i,new(){Pattern=(CoatPattern)(i%4),Hat=(CatHat)(i%6)})).ToList()});
        window.Show();window.Width=900;window.Height=700;window.Navigate("Focus");await Task.Delay(700);
        List<object> rows=[];List<double> navigation=[];
        using var process=Process.GetCurrentProcess();
        foreach(string page in new[]{"Focus","Settings","Your cat","Hidden"})
        {
            if(page=="Hidden")window.Hide();else{window.Show();window.Navigate(page);window.UpdateLayout();}
            host.Perform(CatAction.Idle);await Task.Delay(400);
            List<double> delays=[],handlers=[];int changed=0;
            void Changed()=>changed++;host.Changed+=Changed;
            using var stop=new CancellationTokenSource();
            var probe=Task.Run(async()=>
            {
                while(!stop.IsCancellationRequested)
                {
                    double queued=FrameClock.Now;
                    await window.Dispatcher.InvokeAsync(()=>delays.Add((FrameClock.Now-queued)*1000),DispatcherPriority.Input);
                    await Task.Delay(20);
                }
            });
            process.Refresh();var cpu=process.TotalProcessorTime;long allocated=GC.GetTotalAllocatedBytes(false),paints=host.Surface.PaintCount,writes=host.StateWrites;double begin=FrameClock.Now;
            for(int i=0;i<40;i++)
            {
                if(page=="Settings")
                {
                    var toggle=Descendants<CheckBox>(window).First(c=>c.Content?.ToString()=="Settle while I'm working");
                    var watch=Stopwatch.StartNew();toggle.IsChecked=toggle.IsChecked!=true;toggle.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));handlers.Add(watch.Elapsed.TotalMilliseconds);
                }
                await Task.Delay(75);
            }
            stop.Cancel();await probe;host.Changed-=Changed;process.Refresh();double elapsed=FrameClock.Now-begin;
            rows.Add(new{page,seconds=elapsed,inputQueue=Distribution(delays),toggleHandler=Distribution(handlers),viewNotifications=changed,
                cpuCorePercent=(process.TotalProcessorTime-cpu).TotalSeconds/elapsed*100,allocatedMiBPerSecond=(GC.GetTotalAllocatedBytes(false)-allocated)/1048576d/elapsed,
                privateMiB=process.PrivateMemorySize64/1048576d,paints=host.Surface.PaintCount-paints,stateWrites=host.StateWrites-writes,previewBufferPixels=window.PreviewBufferPixels,controlAnimations=ControlMotion.ActiveAnimations});
        }
        window.Show();
        for(int i=0;i<8;i++)
        {
            window.Navigate("Settings");await Task.Delay(40);var watch=Stopwatch.StartNew();window.Navigate("Your cat");window.UpdateLayout();navigation.Add(watch.Elapsed.TotalMilliseconds);await Task.Delay(80);
        }
        window.Navigate("Settings");window.UpdateLayout();var check=Descendants<CheckBox>(window).First(c=>c.Content?.ToString()=="Settle while I'm working");
        check.IsChecked=false;check.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));await Task.Delay(200);check.ApplyTemplate();
        var knob=(FrameworkElement)check.Template.FindName("Knob",check);var transform=(TranslateTransform)knob.RenderTransform;
        List<object> animation=[];check.IsChecked=true;check.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));double start=FrameClock.Now;
        Check("toggle begins at its displayed position without a first-frame jump",Math.Abs(transform.X)<.01);
        for(int i=0;i<18;i++){animation.Add(new{ms=(FrameClock.Now-start)*1000,x=transform.X});await Task.Delay(12);}
        Check("toggle reaches its checked endpoint",Math.Abs(transform.X-18)<.01);
        Check("toggle visual and preference agree",host.Settings.SettleWhileWorking==check.IsChecked);
        var peer=new System.Windows.Automation.Peers.CheckBoxAutomationPeer(check);
        ((System.Windows.Automation.Provider.IToggleProvider)peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.Toggle)).Toggle();
        Check("accessibility toggle applies the actual preference",!host.Settings.SettleWhileWorking);
        await Task.Delay(45);double turning=transform.X;check.IsChecked=true;
        Check("rapid toggle reversal starts from current position",Math.Abs(turning-transform.X)<.01);
        await Task.Delay(230);host.Update(host.Settings with{ReducedMotion=true});check.IsChecked=false;
        Check("reduced motion settles toggles immediately",transform.X==0);host.Update(host.Settings with{ReducedMotion=false});
        RenderWindow(window,Path.Combine(dir,"ui","settings.png"));
        window.Navigate("Focus");window.UpdateLayout();await Task.Delay(250);
        var pageView=(FrameworkElement)window.FindName("Page");double narrow=pageView.Width;
        window.Width=1180;window.UpdateLayout();Check("resize starts from displayed layout width",Math.Abs(pageView.Width-narrow)<1);
        await Task.Delay(65);double between=pageView.Width;RenderWindow(window,Path.Combine(dir,"ui","resize-between.png"));
        await Task.Delay(220);double wide=pageView.Width;
        Check("resize passes through intermediate widths",between>narrow+1&&between<wide-1,new{narrow,between,wide});
        window.Width=900;window.UpdateLayout();await Task.Delay(65);double restoring=pageView.Width;await Task.Delay(230);
        Check("restoring smoothly returns to the smaller layout",restoring<wide-1&&restoring>pageView.Width+1);
        host.Update(host.Settings with{ReducedMotion=true});window.Width=1050;window.UpdateLayout();await Task.Delay(35);double direct=pageView.Width;await Task.Delay(200);
        Check("reduced motion disables resize interpolation",Math.Abs(pageView.Width-direct)<1);host.Update(host.Settings with{ReducedMotion=false});
        window.Width=900;await Task.Delay(250);RenderWindow(window,Path.Combine(dir,"ui","resize-settled.png"));
        async Task<bool> HasIntermediateResize(WindowState state)
        {
            double origin=pageView.Width;List<double> widths=[];window.WindowState=state;
            for(int i=0;i<18;i++){await Task.Delay(20);widths.Add(pageView.Width);}
            double end=pageView.Width;return Math.Abs(end-origin)>10&&widths.Any(w=>w>Math.Min(origin,end)+1&&w<Math.Max(origin,end)-1);
        }
        Check("maximizing the actual window animates content reflow",await HasIntermediateResize(WindowState.Maximized));
        Check("restoring the actual window animates content reflow",await HasIntermediateResize(WindowState.Normal));
        await Task.Delay(250);Check("UI animation clock sleeps after transitions finish",!ControlMotion.ClockRunning&&ControlMotion.ActiveAnimations==0);
        File.WriteAllText(Path.Combine(dir,"responsiveness.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,logicalProcessors=Environment.ProcessorCount,renderTier=RenderCapability.Tier>>16,
            rows,wardrobeNavigation=Distribution(navigation),animation,checks=Checks,
            note="Synthetic app-owned toggle events and Input-priority callbacks under the current desktop session; not end-to-end hardware input latency."},new JsonSerializerOptions{WriteIndented=true}));
    }
    private static object Distribution(List<double> values)
    {var sorted=values.Order().ToArray();return new{count=sorted.Length,mean=sorted.Length==0?0:sorted.Average(),p95=sorted.Length==0?0:sorted[(int)((sorted.Length-1)*.95)],max=sorted.Length==0?0:sorted[^1]};}
}
