using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CuteCat.App;

internal static class ControlMotion
{
    public static readonly DependencyProperty SmoothProperty=DependencyProperty.RegisterAttached("Smooth",typeof(bool),typeof(ControlMotion),new PropertyMetadata(false,Attach));
    private static readonly HashSet<CheckBox> LoadedControls=[];
    private sealed record Motion(DependencyObject Target,DependencyProperty Property,double From,double To,double Started,double Seconds);
    private static readonly List<Motion> Motions=[];
    private static readonly DispatcherTimer Timer=new(DispatcherPriority.Background){Interval=TimeSpan.FromSeconds(1d/60)};
    static ControlMotion(){Timer.Tick+=(_,_)=>Advance();}
    public static bool Allowed { get; private set; }=true;
    internal static int ActiveAnimations=>Motions.Count;
    internal static bool ClockRunning=>Timer.IsEnabled;
    internal static event Action? PreferenceChanged;
    public static bool GetSmooth(DependencyObject value)=>(bool)value.GetValue(SmoothProperty);
    public static void SetSmooth(DependencyObject value,bool enabled)=>value.SetValue(SmoothProperty,enabled);
    public static void SetAllowed(bool value)
    {if(Allowed==value)return;Allowed=value;if(!value)foreach(var check in LoadedControls)Update(check,false);PreferenceChanged?.Invoke();}
    private static void Attach(DependencyObject value,DependencyPropertyChangedEventArgs args)
    {
        if(value is not CheckBox check||args.NewValue is not true)return;
        check.Loaded+=(_,_)=>{LoadedControls.Add(check);Update(check,false);};
        check.Unloaded+=(_,_)=>{LoadedControls.Remove(check);Update(check,false);};
        check.Checked+=Changed;check.Unchecked+=Changed;
    }
    private static void Changed(object sender,RoutedEventArgs args)
    {if(ReferenceEquals(sender,args.OriginalSource))Update((CheckBox)sender,true);}
    private static void Update(CheckBox check,bool animate)
    {
        if(check.Template is null)return;
        check.ApplyTemplate();
        if(check.Template.FindName("Knob",check) is FrameworkElement knob&&knob.RenderTransform is TranslateTransform offset)
        {
            // Compiled templates may freeze their initial Freezable. Each loaded control owns its animation target.
            if(offset.IsFrozen){offset=offset.CloneCurrentValue();knob.RenderTransform=offset;}
            To(offset,TranslateTransform.XProperty,check.IsChecked==true?18:0,animate&&check.IsLoaded);
        }
        foreach(string name in new[]{"TrackOn","KnobOn"})
            if(check.Template.FindName(name,check) is UIElement overlay)To(overlay,UIElement.OpacityProperty,check.IsChecked==true?1:0,animate&&check.IsLoaded);
    }
    private static void To(Animatable target,DependencyProperty property,double value,bool animate)
        =>Move(target,property,value,animate,160);
    private static void To(UIElement target,DependencyProperty property,double value,bool animate)
        =>Animate(target,property,value,animate);
    internal static void Animate(UIElement target,DependencyProperty property,double value,bool animate,double milliseconds=160)
        =>Move(target,property,value,animate,milliseconds);
    private static void Move(DependencyObject target,DependencyProperty property,double value,bool animate,double milliseconds)
    {
        double from=(double)target.GetValue(property);Motions.RemoveAll(m=>ReferenceEquals(m.Target,target)&&m.Property==property);
        if(!animate||!Allowed||!double.IsFinite(from)||Math.Abs(from-value)<.001)target.SetValue(property,value);
        else Motions.Add(new(target,property,from,value,FrameClock.Now,milliseconds/1000));
        if(Motions.Count>0)Timer.Start();else Timer.Stop();
    }
    private static void Advance()
    {
        double now=FrameClock.Now;
        // Elapsed time starts at the interaction, not at WPF's last presentation tick (which may be stale over RDP).
        foreach(var motion in Motions.ToArray())
        {
            if(!Motions.Contains(motion))continue;
            double u=Math.Clamp((now-motion.Started)/motion.Seconds,0,1),eased=1-Math.Pow(1-u,3);
            motion.Target.SetValue(motion.Property,motion.From+(motion.To-motion.From)*eased);
            if(u>=1)Motions.Remove(motion);
        }
        if(Motions.Count==0)Timer.Stop();
    }
}
