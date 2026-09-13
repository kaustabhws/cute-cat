using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CuteCat.App;

public partial class MainWindow
{
    private readonly DispatcherTimer _refreshTimer=new(DispatcherPriority.Background){Interval=TimeSpan.FromMilliseconds(80)};
    private bool _layoutReady,_previewOnScreen=true;
    private double _layoutWidth=-1;
    internal int PreviewBufferPixels=>_preview.BufferPixels;
    private void SetupMotion()
    {
        void ApplyMotionPreference(){if(!ControlMotion.Allowed&&_layoutWidth>0)ControlMotion.Animate(Page,WidthProperty,_layoutWidth,false);}
        ControlMotion.PreferenceChanged+=ApplyMotionPreference;
        _refreshTimer.Tick+=(_,_)=>{_refreshTimer.Stop();Refresh();};
        PageScroll.ScrollChanged+=(_,e)=>
        {
            if(e.ViewportWidthChange!=0||!_layoutReady)ResizeContent();
            UpdatePreviewVisibility();
        };
        Loaded+=(_,_)=>{ResizeContent();UpdatePreviewVisibility();};
        StateChanged+=(_,_)=>{if(WindowState==WindowState.Minimized)_refreshTimer.Stop();else{RequestRefresh();UpdatePreviewVisibility();}};
        Closed+=(_,_)=>{_refreshTimer.Stop();ControlMotion.PreferenceChanged-=ApplyMotionPreference;};
    }
    private void RequestRefresh()
    {if(IsVisible&&WindowState!=WindowState.Minimized&&!_refreshTimer.IsEnabled)_refreshTimer.Start();}
    private void ResizeContent()
    {
        double target=PageScroll.ViewportWidth-Page.Margin.Left-Page.Margin.Right;
        if(!double.IsFinite(target)||target<=0||Math.Abs(target-_layoutWidth)<.5)return;
        _layoutWidth=target;
        ControlMotion.Animate(Page,WidthProperty,target,_layoutReady&&IsVisible&&WindowState!=WindowState.Minimized,180);
        _layoutReady=true;
    }
    private void UpdatePreviewVisibility()
    {
        _previewOnScreen=false;
        if(_preview.Parent is null||!_preview.IsVisible||WindowState==WindowState.Minimized)return;
        var bounds=_preview.TransformToAncestor(PageScroll).TransformBounds(new Rect(_preview.RenderSize));
        _previewOnScreen=bounds.IntersectsWith(new Rect(0,0,PageScroll.ViewportWidth,PageScroll.ViewportHeight));
    }
}
