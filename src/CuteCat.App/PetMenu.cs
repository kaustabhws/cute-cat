using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CuteCat.Core;
using Forms=System.Windows.Forms;

namespace CuteCat.App;

/// <summary>The same themed WPF popup serves both the cat and notification-area icon.</summary>
public sealed class PetMenu(CompanionHost host)
{
    private ContextMenu? _menu;
    public bool IsOpen=>_menu?.IsOpen==true;
    internal FrameworkElement? View=>_menu;
    public void Close(){if(_menu is not null)_menu.IsOpen=false;}
    public void Show(V2 point)
    {
        Close();host.Brain.UserAction(CatAction.Idle);host.CancelPaw();host.Cat.Stop(FrameClock.Now);host.Cat.AutonomyPaused=true;
        _menu=Build();
        var screen=Forms.Screen.FromPoint(new System.Drawing.Point((int)point.X,(int)point.Y));
        double dpi=CompanionHost.DpiFor(screen);var work=screen.WorkingArea;
        _menu.MaxHeight=work.Height/dpi-24;_menu.ApplyTemplate();_menu.Measure(new Size(double.PositiveInfinity,_menu.MaxHeight));
        double width=Math.Max(260,_menu.DesiredSize.Width),height=Math.Max(400,_menu.DesiredSize.Height);
        _menu.Placement=PlacementMode.AbsolutePoint;
        _menu.HorizontalOffset=Math.Clamp(point.X/dpi,work.Left/dpi+8,Math.Max(work.Left/dpi+8,work.Right/dpi-width-8));
        _menu.VerticalOffset=Math.Clamp(point.Y/dpi,work.Top/dpi+8,Math.Max(work.Top/dpi+8,work.Bottom/dpi-height-8));
        _menu.Closed+=(_,_)=>host.Cat.AutonomyPaused=false;
        _menu.IsOpen=true;
    }
    internal ContextMenu Build()
    {
        var menu=new ContextMenu{MinWidth=260};
        menu.Items.Add(new MenuItem{Header=host.Settings.Nickname+" · your companion",IsEnabled=false});
        menu.Items.Add(new Separator());
        menu.Items.Add(Item("Open Cute Cat","\uE80F",host.OpenPanel));
        menu.Items.Add(Item("Pet & say hello","\uEB51",()=>host.Perform(CatAction.Meow)));
        menu.Items.Add(Item(host.Cat.Action==CatAction.Sleep?"Wake up":"Take a nap","\uE708",()=>host.Perform(host.Cat.Action==CatAction.Sleep?CatAction.Wake:CatAction.Sleep)));
        menu.Items.Add(Item("Park by the taskbar","\uE81D",host.Park));
        menu.Items.Add(new Separator());
        var guard=Item("App guard","\uE72E",()=>host.Update(host.Settings with{AppGuard=!host.Settings.AppGuard}));
        guard.IsCheckable=true;guard.IsChecked=host.Settings.AppGuard;menu.Items.Add(guard);
        menu.Items.Add(Item(FrameClock.Now<host.GuardPausedUntil?"Resume app guard":"Pause app guard · 10 minutes","\uE769",()=>host.PauseGuard(FrameClock.Now<host.GuardPausedUntil?0:10)));
        var notifications=Item("Notification paws","\uE7F4",()=>host.Update(host.Settings with{Notifications=!host.Settings.Notifications}));
        notifications.IsCheckable=true;notifications.IsChecked=host.Settings.Notifications;menu.Items.Add(notifications);
        menu.Items.Add(new Separator());
        menu.Items.Add(Item(host.Visible?"Hide companion":"Show companion","\uE890",()=>host.ShowCat(!host.Visible)));
        menu.Items.Add(Item(host.Session.Status==SessionStatus.Running?"Pause focus":"Open focus timer","\uE916",()=>
        {if(host.Session.Status==SessionStatus.Running){host.Session.Pause(FrameClock.Now);host.Configure();host.Save();}else host.OpenPanel();}));
        menu.Items.Add(new Separator());menu.Items.Add(Item("Quit Cute Cat","\uE8BB",()=>Application.Current.Shutdown()));
        return menu;
    }
    private static MenuItem Item(string text,string glyph,Action action)
    {
        var icon=new TextBlock{Text=glyph,FontFamily=new FontFamily("Segoe Fluent Icons"),FontSize=16,VerticalAlignment=VerticalAlignment.Center};
        var item=new MenuItem{Header=text,Icon=icon};item.Click+=(_,_)=>action();return item;
    }
}
