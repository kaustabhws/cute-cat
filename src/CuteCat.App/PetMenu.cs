using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using CuteCat.Core;
using Forms=System.Windows.Forms;

namespace CuteCat.App;

/// <summary>An activatable, borderless menu window shared by the cat and tray icon.</summary>
public sealed class PetMenu(CompanionHost host)
{
    private PetMenuItems? _menu;
    private Window? _window;
    private Action<bool>? _dismiss;
    private IntPtr _returnFocus;
    internal Func<IntPtr> ForegroundWindow { get; set; }=Native.GetForegroundWindow;
    internal Func<IntPtr,bool> ActivateWindow { get; set; }=Native.SetForegroundWindow;
    public bool IsOpen=>_window?.IsVisible==true;
    internal FrameworkElement? View=>_menu;
    internal IntPtr PopupHandle=>_window is null?IntPtr.Zero:new WindowInteropHelper(_window).Handle;
    internal int OpenCount { get; private set; }
    internal int CloseCount { get; private set; }
    public void Close()=>_dismiss?.Invoke(true);

    public void Show(V2 point)
    {
        OpenCount++;
        IntPtr previous=ForegroundWindow();
        if(_menu is not null&&ContainsWindow(_menu,previous))previous=_returnFocus;
        Close();host.Brain.UserAction(CatAction.Idle);host.CancelPaw();host.Cat.Stop(FrameClock.Now);host.Cat.AutonomyPaused=true;
        var menu=Build();
        var screen=Forms.Screen.FromPoint(new System.Drawing.Point((int)point.X,(int)point.Y));
        double dpi=CompanionHost.DpiFor(screen);var work=screen.WorkingArea;
        menu.MaxHeight=work.Height/dpi-24;menu.ApplyTemplate();menu.Measure(new Size(double.PositiveInfinity,menu.MaxHeight));
        double width=Math.Max(260,menu.DesiredSize.Width),height=Math.Max(60,menu.DesiredSize.Height);
        var view=new Window
        {
            Title="Cute Cat menu",WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,
            AllowsTransparency=true,Background=Brushes.Transparent,ShowInTaskbar=false,ShowActivated=false,Topmost=true,
            SizeToContent=SizeToContent.WidthAndHeight,MaxHeight=work.Height/dpi-16,Content=menu,
            Left=Math.Clamp(point.X/dpi,work.Left/dpi+8,Math.Max(work.Left/dpi+8,work.Right/dpi-width-8)),
            Top=Math.Clamp(point.Y/dpi,work.Top/dpi+8,Math.Max(work.Top/dpi+8,work.Bottom/dpi-height-8))
        };
        _menu=menu;_window=view;_returnFocus=previous;
        HwndSource? source=null;IntPtr handle=IntPtr.Zero;
        bool opening=true,armed=false,closing=false;
        void Dismiss(bool restore)
        {
            if(!ReferenceEquals(_window,view)||closing)return;
            closing=true;
            bool ownedFocus=view.IsActive||ForegroundWindow()==handle;
            CloseSubmenus(menu);view.Close();
            IntPtr current=ForegroundWindow();
            if(restore&&ownedFocus&&(current==handle||current==IntPtr.Zero)&&previous!=IntPtr.Zero&&Native.IsWindow(previous)&&Native.IsWindowVisible(previous))
                ActivateWindow(previous);
        }
        _dismiss=Dismiss;
        IntPtr OnMessage(IntPtr window,int message,IntPtr wp,IntPtr lp,ref bool handled)
        {
            // Native cancellation only; no global hooks or foreground polling.
            if(message==0x1F&&!opening)view.Dispatcher.BeginInvoke(DispatcherPriority.Input,new Action(()=>Dismiss(false)));
            return IntPtr.Zero;
        }
        view.SourceInitialized+=(_,_)=>
        {handle=new WindowInteropHelper(view).Handle;source=HwndSource.FromHwnd(handle);source?.AddHook(OnMessage);};
        view.Activated+=(_,_)=>{if(!opening)armed=true;};
        view.Deactivated+=(_,_)=>
        {
            if(opening||!armed)return;
            view.Dispatcher.BeginInvoke(DispatcherPriority.Input,new Action(()=>
            {if(ReferenceEquals(_window,view)&&!view.IsActive&&!ContainsWindow(menu,ForegroundWindow()))Dismiss(false);}));
        };
        menu.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent,new MouseButtonEventHandler((_,e)=>
        {if(ReferenceEquals(e.OriginalSource,menu))Dismiss(false);}),true);
        menu.AddHandler(MenuItem.ClickEvent,new RoutedEventHandler((_,e)=>
        {if(e.OriginalSource is MenuItem{HasItems:false,StaysOpenOnClick:false})Dismiss(true);}),true);
        view.PreviewKeyDown+=(_,e)=>
        {
            if(!ReferenceEquals(_window,view)||e.Key!=Key.Escape)return;
            var submenu=OpenSubmenu(menu);
            if(submenu is not null)submenu.IsSubmenuOpen=false;else Dismiss(true);
            e.Handled=true;
        };
        view.Closed+=(_,_)=>
        {
            CloseCount++;
            if(source is not null&&!source.IsDisposed)source.RemoveHook(OnMessage);
            CloseSubmenus(menu);
            if(Mouse.Captured is Visual captured&&PresentationSource.FromVisual(captured) is HwndSource capturedSource&&ContainsWindow(menu,capturedSource.Handle))Mouse.Capture(null);
            if(!ReferenceEquals(_window,view))return;
            _menu=null;_window=null;_dismiss=null;_returnFocus=IntPtr.Zero;host.Cat.AutonomyPaused=false;
        };
        try
        {
            view.Show();
            // Explicitly show without relying on a launcher's initial hidden-window flag.
            Native.SetWindowPos(handle,new IntPtr(-1),0,0,0,0,0x1|0x2|0x10|0x40);
            // Focus/capture are best effort. Failure must NEVER make the requested UI vanish.
            ActivateWindow(handle);menu.Focus();Mouse.Capture(menu,CaptureMode.SubTree);
            opening=false;armed=view.IsActive||ForegroundWindow()==handle;
        }
        catch{Dismiss(true);throw;}
    }
    private static bool ContainsWindow(ItemsControl items,IntPtr handle)
    {
        if(handle==IntPtr.Zero)return false;
        if(PresentationSource.FromVisual(items) is HwndSource source&&source.Handle==handle)return true;
        foreach(var item in items.Items.OfType<MenuItem>())if(ContainsWindow(item,handle))return true;
        return false;
    }
    private static void CloseSubmenus(ItemsControl items)
    {foreach(var item in items.Items.OfType<MenuItem>()){CloseSubmenus(item);item.IsSubmenuOpen=false;}}
    private static MenuItem? OpenSubmenu(ItemsControl items)
    {foreach(var item in items.Items.OfType<MenuItem>().Where(i=>i.IsSubmenuOpen))return OpenSubmenu(item)??item;return null;}
    internal PetMenuItems Build()
    {
        var menu=new PetMenuItems{MinWidth=260};
        menu.SetResourceReference(FrameworkElement.StyleProperty,"PetMenuStyle");
        menu.Items.Add(new MenuItem{Header=host.Settings.Nickname+" · your companion",IsEnabled=false});
        menu.Items.Add(new Separator());
        menu.Items.Add(Item("Open Cute Cat","\uE80F",host.OpenPanel));
        menu.Items.Add(Item("Pet & say hello","\uEB51",()=>host.Perform(CatAction.Pet)));
        menu.Items.Add(Item(host.Cat.Action==CatAction.Sleep?"Wake up":"Take a nap","\uE708",()=>host.Perform(host.Cat.Action==CatAction.Sleep?CatAction.Wake:CatAction.Sleep)));
        menu.Items.Add(Item("Park by the taskbar","\uE81D",host.Park));
        menu.Items.Add(Item("Return to my resting spot","\uE80F",host.ReturnToSpot));
        menu.Items.Add(new Separator());
        var profiles=new MenuItem{Header="Profile · "+host.CurrentProfile.Name};
        foreach(var profile in host.Settings.Profiles)
        {var choice=Item(profile.Name,"\uE8A5",()=>host.SelectProfile(profile.Id));choice.IsCheckable=true;choice.IsChecked=profile.Id==host.CurrentProfile.Id;profiles.Items.Add(choice);}
        menu.Items.Add(profiles);
        if(host.CurrentApp is { } selected)menu.Items.Add(Item("Allow "+selected.Rule.Name+" · 5 minutes","\uE916",()=>host.AllowApp(selected.Rule.Path,5)));
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
