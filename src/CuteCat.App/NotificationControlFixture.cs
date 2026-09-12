using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Interop;
using CuteCat.Core;

namespace CuteCat.App;

// Native integration fixture, never created during normal app use.
internal sealed class NotificationControlFixture : Window
{
    public Button Cross { get; }=new(){Content="×",Width=32,Height=32,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top};
    public GroupBox Toast { get; }=new(){Padding=new Thickness(0)};
    public GroupBox Context { get; }=new(){Padding=new Thickness(0)};
    public int Invocations {get;private set;}
    public NotificationControlFixture()
    {
        Width=360;Height=150;WindowStyle=WindowStyle.ToolWindow;ShowActivated=false;ShowInTaskbar=false;Title="Notification control test";
        var grid=new Grid();grid.Children.Add(new TextBlock{Text="App-owned notification control fixture",Margin=new Thickness(10,45,10,0)});grid.Children.Add(Cross);
        Toast.Content=grid;Context.Content=Toast;Content=Context;
        AutomationProperties.SetAutomationId(Toast,"ToastView");AutomationProperties.SetAutomationId(Cross,"DismissButton");
        AutomationProperties.SetName(Cross,"Dismiss notification");Cross.Click+=(_,_)=>Invocations++;
    }
    public async Task<bool> InvokeThroughResolver()
    {
        var hwnd=new WindowInteropHelper(this).Handle;
        return await Task.Run(()=>
        {
            var root=AutomationElement.FromHandle(hwnd);
            var resolved=ShellNotifications.ResolveCloseControl(root,Environment.ProcessId,1.25);
            if(resolved is null)return false;
            ((InvokePattern)resolved.Value.close.GetCurrentPattern(InvokePattern.Pattern)).Invoke();return true;
        });
    }
    public async Task<bool> ResolverFindsControl(bool startAtToast=false)
    {
        var hwnd=new WindowInteropHelper(this).Handle;
        return await Task.Run(()=>
        {
            var root=AutomationElement.FromHandle(hwnd);
            if(startAtToast)root=root.FindFirst(TreeScope.Descendants,new PropertyCondition(AutomationElement.AutomationIdProperty,"ToastView"));
            return root is not null&&ShellNotifications.ResolveCloseControl(root,Environment.ProcessId,1.25) is not null;
        });
    }
}
