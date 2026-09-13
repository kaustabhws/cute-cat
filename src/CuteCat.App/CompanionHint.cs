using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using CuteCat.Core;

namespace CuteCat.App;

/// <summary>Small, nonactivating companion UI. Clicking its buttons never changes the foreground app.</summary>
internal sealed class CompanionHint : Window
{
    private readonly TextBlock _title=new(){FontSize=16,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap};
    private readonly TextBlock _detail=new(){FontSize=12,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,5,0,12)};
    internal Button Primary { get; }=new(){MinHeight=30,Padding=new Thickness(10,5,10,5),Focusable=false};
    private readonly Button _dismiss=new(){Content="Later",MinHeight=30,Padding=new Thickness(10,5,10,5),Focusable=false};
    private Action? _action,_later;
    private IntPtr _handle;
    private (int X,int Y)? _position;
    public CompanionHint()
    {
        Title="Cute Cat companion cue";Width=256;SizeToContent=SizeToContent.Height;WindowStyle=WindowStyle.None;
        ResizeMode=ResizeMode.NoResize;AllowsTransparency=true;Background=Brushes.Transparent;ShowInTaskbar=false;ShowActivated=false;Topmost=true;
        var content=new StackPanel{Margin=new Thickness(16)};content.Children.Add(_title);content.Children.Add(_detail);
        var actions=new WrapPanel();Primary.SetResourceReference(StyleProperty,"Primary");Primary.Margin=new Thickness(0,0,6,0);actions.Children.Add(Primary);actions.Children.Add(_dismiss);content.Children.Add(actions);
        _title.SetResourceReference(TextBlock.ForegroundProperty,"Ink");_detail.SetResourceReference(TextBlock.ForegroundProperty,"Muted");
        var card=new Border{CornerRadius=new CornerRadius(16),BorderThickness=new Thickness(1),Child=content};
        card.SetResourceReference(Border.BackgroundProperty,"Paper");card.SetResourceReference(Border.BorderBrushProperty,"Line");Content=card;
        Primary.Click+=(_,_)=>_action?.Invoke();_dismiss.Click+=(_,_)=>_later?.Invoke();
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);_handle=new WindowInteropHelper(this).Handle;
        SetWindowLongPtr(_handle,-20,new IntPtr(GetWindowLongPtr(_handle,-20).ToInt64()|0x08000080L));
        HwndSource.FromHwnd(_handle)?.AddHook(NoActivate);
    }
    private static IntPtr NoActivate(IntPtr h,int message,IntPtr w,IntPtr l,ref bool handled)
    {if(message==0x21){handled=true;return new IntPtr(3);}return IntPtr.Zero;}
    public void Present(string title,string detail,string button,Action action,Action? later=null)
    {
        bool changed=_title.Text!=title||_detail.Text!=detail||!Equals(Primary.Content,button)||(_later is null)!=(later is null);
        _title.Text=title;_detail.Text=detail;if(!Equals(Primary.Content,button))Primary.Content=button;_action=action;_later=later;
        _dismiss.Visibility=later is null?Visibility.Collapsed:Visibility.Visible;
        if(!IsVisible){Show();changed=true;}if(changed)UpdateLayout();
    }
    public void Clear(){_action=null;_later=null;if(IsVisible)Hide();_position=null;}
    public void Follow(Companion cat,double dpi)
    {
        if(!IsVisible)return;
        double width=ActualWidth*dpi,height=ActualHeight*dpi;var area=cat.WorkArea;
        double x=cat.Position.X-width/2,y=cat.Position.Y-167*cat.Scale-height-10*dpi;
        if(y<area.Top+8*dpi){x=cat.Position.X+110*cat.Scale;y=cat.Position.Y-120*cat.Scale;}
        var position=((int)Math.Clamp(x,area.Left+6*dpi,Math.Max(area.Left+6*dpi,area.Right-width-6*dpi)),
            (int)Math.Clamp(y,area.Top+6*dpi,Math.Max(area.Top+6*dpi,area.Bottom-height-6*dpi)));
        if(_position==position)return;_position=position;
        Native.SetWindowPos(_handle,new IntPtr(-1),position.Item1,position.Item2,0,0,0x1|0x10|0x40);
    }
    [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")]private static extern IntPtr GetWindowLongPtr(IntPtr h,int index);
    [DllImport("user32.dll",EntryPoint="SetWindowLongPtrW")]private static extern IntPtr SetWindowLongPtr(IntPtr h,int index,IntPtr value);
}
