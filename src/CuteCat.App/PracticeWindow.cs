using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using CuteCat.Core;

namespace CuteCat.App;

public sealed class PracticeWindow : Window
{
    private readonly Button _close;
    private readonly string _id="practice:"+Guid.NewGuid().ToString("N");
    public bool DismissedByPaw { get; private set; }
    public PracticeWindow()
    {
        Title="Cute Cat • practice notification";Width=354;Height=138;WindowStyle=WindowStyle.None;
        ResizeMode=ResizeMode.NoResize;ShowActivated=false;ShowInTaskbar=false;Topmost=true;Background=Brushes.Transparent;AllowsTransparency=true;
        var border=new Border{Background=new SolidColorBrush(Color.FromRgb(249,246,235)),BorderBrush=new SolidColorBrush(Color.FromRgb(192,199,177)),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(14),Padding=new Thickness(20,16,16,16)};
        var grid=new Grid();grid.ColumnDefinitions.Add(new());grid.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
        var text=new StackPanel();text.Children.Add(new TextBlock{Text="PRACTICE NOTIFICATION",FontSize=10,FontWeight=FontWeights.SemiBold,Foreground=new SolidColorBrush(Color.FromRgb(98,112,89)),Margin=new Thickness(0,0,0,10)});
        text.Children.Add(new TextBlock{Text="A tiny tap. A quieter desk.",FontSize=17,FontFamily=new FontFamily("Georgia"),Foreground=new SolidColorBrush(Color.FromRgb(45,58,49))});
        text.Children.Add(new TextBlock{Text="This card belongs to Cute Cat.",FontSize=12,Margin=new Thickness(0,10,0,0),Foreground=new SolidColorBrush(Color.FromRgb(104,109,95))});
        grid.Children.Add(text);
        _close=new Button{Content="×",Width=30,Height=30,VerticalAlignment=VerticalAlignment.Top,FontSize=20,Padding=new Thickness(0),MinHeight=0,Background=Brushes.Transparent};
        System.Windows.Automation.AutomationProperties.SetName(_close,"Close practice notification");
        _close.Click+=(_,_)=>Close();Grid.SetColumn(_close,1);grid.Children.Add(_close);border.Child=grid;Content=border;
    }
    public NotificationTarget? Target(double now,int epoch)
    {
        if(!IsVisible || !_close.IsVisible)return null;
        var top=_close.PointToScreen(new Point(0,0));var bottom=_close.PointToScreen(new Point(_close.ActualWidth,_close.ActualHeight));
        return new(_id,new WindowInteropHelper(this).Handle.ToInt64(),Environment.ProcessId,new Area(top.X,top.Y,bottom.X-top.X,bottom.Y-top.Y),now,epoch,true);
    }
    public void PawDismiss() { DismissedByPaw=true;Close(); }
}
