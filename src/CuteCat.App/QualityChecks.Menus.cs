using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using CuteCat.Core;

namespace CuteCat.App;

public static partial class QualityChecks
{
    public static async Task MenuOpening(CompanionHost host,MainWindow window,string dir)
    {
        Directory.CreateDirectory(dir);Directory.CreateDirectory(Path.Combine(dir,"ui"));
        host.Update(new Preferences{Quiet=true,IdleNaps=false});window.Hide();
        bool Visible()=>host.Menu.IsOpen&&host.Menu.PopupHandle!=IntPtr.Zero&&Native.IsWindowVisible(host.Menu.PopupHandle);
        async Task CatRequest()
        {CancelMenuMode(host.Surface.Handle,0x205,IntPtr.Zero,new IntPtr((150<<16)|160));await Task.Delay(300);}
        async Task TrayRequest()
        {host.TrayMouseUp(null,new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Right,1,0,0,0));await Task.Delay(300);}
        try
        {
            await CatRequest();Check("cat right-click callback opens a real visible menu",Visible()&&!window.IsVisible,new{foreground=Native.GetForegroundWindow().ToInt64(),menu=host.Menu.PopupHandle.ToInt64()});
            if(host.Menu.View is { } catMenu)RenderElement(catMenu,Path.Combine(dir,"ui","cat-menu.png"));host.Menu.Close();
            host.ShowCat(false);await TrayRequest();Check("tray right-click callback opens a real menu even with cat hidden",Visible()&&!host.Visible&&!window.IsVisible);
            if(host.Menu.View is { } trayMenu)RenderElement(trayMenu,Path.Combine(dir,"ui","tray-menu.png"));host.Menu.Close();
            var activate=host.Menu.ActivateWindow;host.Menu.ActivateWindow=_=>false;host.ShowCat(true);
            await CatRequest();Check("cat menu remains visible when focus request is refused",Visible());
            var hello=((PetMenuItems)host.Menu.View!).Items.OfType<MenuItem>().First(i=>i.Header?.ToString()=="Pet & say hello");
            ((IInvokeProvider)new MenuItemAutomationPeer(hello).GetPattern(PatternInterface.Invoke)).Invoke();await Task.Delay(150);
            Check("unfocused menu remains usable and closes after selection",host.Cat.Action==CatAction.Pet&&!host.Menu.IsOpen&&!host.Cat.AutonomyPaused);
            host.ShowCat(false);await TrayRequest();Check("hidden-tray path remains visible when focus request is refused",Visible());
            var root=(PetMenuItems)host.Menu.View!;
            root.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice,Environment.TickCount,MouseButton.Left){RoutedEvent=Mouse.PreviewMouseDownOutsideCapturedElementEvent});await Task.Delay(150);
            Check("unfocused menu still handles outside-capture dismissal",!host.Menu.IsOpen&&!host.Cat.AutonomyPaused);host.Menu.ActivateWindow=activate;
            for(int i=0;i<3;i++){await TrayRequest();Check("repeated tray open "+(i+1),Visible());host.Menu.Close();}
            Check("closing menu releases capture and autonomy",Native.GetCapture()==IntPtr.Zero&&!host.Cat.AutonomyPaused);
        }
        finally
        {
            host.Menu.Close();
            File.WriteAllText(Path.Combine(dir,"menu-opening-checks.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,uiAccess=RuntimeAccess.HasUiAccess,checks=Checks},new JsonSerializerOptions{WriteIndented=true}));
        }
    }
    public static async Task Menus(CompanionHost host,MainWindow window,string dir,string fixture,bool simulatedActivation=false)
    {
        Directory.CreateDirectory(dir);Directory.CreateDirectory(Path.Combine(dir,"ui"));
        string outside=Path.Combine(dir,"outside-fixture");Directory.CreateDirectory(outside);
        host.Update(new Preferences{Quiet=true,IdleNaps=false});window.Activate();
        using var process=Process.Start(new ProcessStartInfo(fixture){Arguments="--app-window \""+outside+"\" --menu-owner "+Environment.ProcessId,UseShellExecute=true})!;
        try
        {
            for(int i=0;i<40&&!File.Exists(Path.Combine(outside,"ready"));i++)await Task.Delay(100);
            IntPtr other=IntPtr.Zero;Native.EnumWindows((h,_)=>{Native.GetWindowThreadProcessId(h,out uint pid);if(pid==process.Id&&Native.IsWindowVisible(h))other=h;return true;},IntPtr.Zero);
            Check("menu dismissal fixture is a separate test-owned process",other!=IntPtr.Zero);
            if(simulatedActivation)
            {
                IntPtr active=other;
                host.Menu.ForegroundWindow=()=>active;
                host.Menu.ActivateWindow=handle=>
                {
                    IntPtr previous=active;active=handle;
                    if(previous==host.Menu.PopupHandle&&previous!=IntPtr.Zero&&previous!=handle)CancelMenuMode(previous,0x6,IntPtr.Zero,handle);
                    if(handle==host.Menu.PopupHandle&&handle!=IntPtr.Zero)CancelMenuMode(handle,0x6,new IntPtr(1),previous);
                    return true;
                };
            }
            window.Hide();host.Menu.ActivateWindow(other);await Task.Delay(100);
            if(!simulatedActivation&&Native.GetForegroundWindow()!=other)throw new InvalidOperationException("Windows did not provide the fixture with foreground activation. This native menu check requires an active input desktop.");
            Check("menu opens from a background cat with the control panel hidden",host.Menu.ForegroundWindow()==other&&!window.IsVisible);
            var point=new V2(host.Cat.WorkArea.Right-360,host.Cat.WorkArea.Bottom-560);
            async Task Open()
            {if(!host.Menu.IsOpen)host.Menu.ActivateWindow(other);host.Menu.Show(point);await Task.Delay(200);}
            await Open();
            Check("explicit menu establishes activation and native mouse capture",host.Menu.IsOpen&&host.Menu.PopupHandle!=IntPtr.Zero&&host.Menu.ForegroundWindow()==host.Menu.PopupHandle&&Native.GetCapture()==host.Menu.PopupHandle,
                new{host.Menu.IsOpen,foreground=Native.GetForegroundWindow().ToInt64(),popup=host.Menu.PopupHandle.ToInt64(),capture=Native.GetCapture().ToInt64()});
            Check("opening the menu keeps the main panel hidden and pauses roaming",!window.IsVisible&&host.Cat.AutonomyPaused);
            foreach(string theme in new[]{"Light","Dark"})
            {window.SetTheme(theme);await Task.Delay(100);if(host.Menu.View is { } view)RenderElement(view,Path.Combine(dir,"ui",theme+"-menu.png"));}
            host.Menu.ActivateWindow(other);await Task.Delay(250);
            Check("another-process activation notification dismisses the menu",!host.Menu.IsOpen&&host.Menu.ForegroundWindow()==other);
            Check("outside dismissal releases capture and resumes autonomy",Native.GetCapture()==IntPtr.Zero&&!host.Cat.AutonomyPaused);
            foreach(var button in new[]{MouseButton.Left,MouseButton.Right})
            {
                await Open();var menu=(PetMenuItems)host.Menu.View!;
                // The same WPF event delivered for a click outside the captured tree,
                // without moving the user's pointer or injecting system input.
                menu.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice,Environment.TickCount,button){RoutedEvent=Mouse.PreviewMouseDownOutsideCapturedElementEvent});
                await Task.Delay(150);Check(button+" outside-capture click dismisses the menu",!host.Menu.IsOpen&&!host.Cat.AutonomyPaused,new{host.Menu.IsOpen,host.Cat.AutonomyPaused,captured=Mouse.Captured?.GetType().Name,menu.IsKeyboardFocusWithin});
            }
            await Open();var root=(PetMenuItems)host.Menu.View!;var profiles=root.Items.OfType<MenuItem>().First(i=>i.HasItems);
            profiles.IsSubmenuOpen=true;await Task.Delay(150);
            Check("submenu stays open inside the menu family",host.Menu.IsOpen&&profiles.IsSubmenuOpen);
            root.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(root)!,Environment.TickCount,Key.Escape){RoutedEvent=Keyboard.PreviewKeyDownEvent});
            await Task.Delay(100);Check("first Escape closes the submenu only",host.Menu.IsOpen&&!profiles.IsSubmenuOpen);
            profiles.IsSubmenuOpen=true;await Task.Delay(100);
            var study=profiles.Items.OfType<MenuItem>().First(i=>i.Header?.ToString()=="Study");
            ((IInvokeProvider)new MenuItemAutomationPeer(study).GetPattern(PatternInterface.Invoke)).Invoke();await Task.Delay(200);
            Check("submenu command executes and closes the whole menu",host.CurrentProfile.Id=="study"&&!host.Menu.IsOpen&&!host.Cat.AutonomyPaused,new{host.CurrentProfile.Id,host.Menu.IsOpen,host.Cat.AutonomyPaused});
            await Open();root=(PetMenuItems)host.Menu.View!;
            root.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(root)!,Environment.TickCount,Key.Escape){RoutedEvent=Keyboard.PreviewKeyDownEvent});
            await Task.Delay(150);Check("Escape closes the menu and restores prior focus",!host.Menu.IsOpen&&host.Menu.ForegroundWindow()==other&&!host.Cat.AutonomyPaused,new{host.Menu.IsOpen,host.Cat.AutonomyPaused,focus=host.Menu.ForegroundWindow().ToInt64(),other=other.ToInt64()});
            await Open();window.Show();window.Activate();host.Menu.ActivateWindow(new WindowInteropHelper(window).Handle);await Task.Delay(250);
            Check("opening the control panel dismisses without stealing its focus",!host.Menu.IsOpen&&host.Menu.ForegroundWindow()==new WindowInteropHelper(window).Handle);
            window.Hide();host.Menu.ActivateWindow(other);await Open();
            var hello=((PetMenuItems)host.Menu.View!).Items.OfType<MenuItem>().First(i=>i.Header?.ToString()=="Pet & say hello");
            ((IInvokeProvider)new MenuItemAutomationPeer(hello).GetPattern(PatternInterface.Invoke)).Invoke();await Task.Delay(200);
            Check("ordinary menu commands still execute and dismiss",host.Cat.Action==CatAction.Pet&&!host.Menu.IsOpen&&!host.Cat.AutonomyPaused,new{host.Cat.Action,host.Menu.IsOpen,host.Cat.AutonomyPaused});
            await Open();CancelMenuMode(host.Menu.PopupHandle,0x1F,IntPtr.Zero,IntPtr.Zero);await Task.Delay(150);
            Check("native cancellation clears menu ownership",!host.Menu.IsOpen&&Native.GetCapture()==IntPtr.Zero&&!host.Cat.AutonomyPaused);
            bool repeats=true;for(int i=0;i<3;i++){await Open();repeats&=host.Menu.IsOpen;host.Menu.ActivateWindow(other);await Task.Delay(150);repeats&=!host.Menu.IsOpen&&!host.Cat.AutonomyPaused;}
            Check("repeated opening and outside dismissal leave no stale menu",repeats);
            await Open();var stale=(PetMenuItems)host.Menu.View!;host.Menu.Show(point);var replacement=host.Menu.View;
            stale.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice,Environment.TickCount,MouseButton.Left){RoutedEvent=Mouse.PreviewMouseDownOutsideCapturedElementEvent});await Task.Delay(150);
            Check("late events from an old menu do not close its replacement",host.Menu.IsOpen&&ReferenceEquals(host.Menu.View,replacement)&&host.Cat.AutonomyPaused);host.Menu.Close();
            Check("cat surface still declines mouse activation",CancelMenuMode(host.Surface.Handle,0x21,IntPtr.Zero,IntPtr.Zero)==new IntPtr(3));
            var activate=host.Menu.ActivateWindow;host.Menu.ActivateWindow=_=>false;
            await Open();Check("denied activation keeps the requested menu visible",host.Menu.IsOpen&&Native.IsWindowVisible(host.Menu.PopupHandle)&&host.Cat.AutonomyPaused);host.Menu.Close();host.Menu.ActivateWindow=activate;
        }
        finally
        {
            host.Menu.Close();File.WriteAllText(Path.Combine(outside,"quit"),"");
            File.WriteAllText(Path.Combine(dir,"menu-checks.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,uiAccess=RuntimeAccess.HasUiAccess,simulatedActivation,checks=Checks},new JsonSerializerOptions{WriteIndented=true}));
        }
    }
    [DllImport("user32.dll",EntryPoint="SendMessageW")]private static extern IntPtr CancelMenuMode(IntPtr window,uint message,IntPtr wp,IntPtr lp);
}
