using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CuteCat.Core;
using PixelFormat=System.Drawing.Imaging.PixelFormat;

namespace CuteCat.App;

public sealed class CatPreview : FrameworkElement,IDisposable
{
    private readonly CatPainter _painter=new();
    private Bitmap? _bitmap;
    private Graphics? _graphics;
    private WriteableBitmap? _image;
    private CatPose? _lastPose;
    private ImageSource? _drawnImage;
    internal int BufferPixels=>(_bitmap?.Width??0)*(_bitmap?.Height??0);
    public CatPreview() { Height=260;MinWidth=200;Focusable=false; }
    public void Update(CatPose pose)
    {
        if(ActualWidth<=0||ActualHeight<=0)return;
        // Match the displayed preview's physical pixels; the desktop cat renderer is unchanged.
        double pixels=Math.Min(ActualWidth,ActualHeight*1.25)*VisualTreeHelper.GetDpi(this).DpiScaleX;
        int width=Math.Clamp((int)Math.Ceiling(pixels/40)*40,160,960),height=width*4/5;
        if(_bitmap?.Width!=width)
        {
            _graphics?.Dispose();_bitmap?.Dispose();_bitmap=new(width,height,PixelFormat.Format32bppPArgb);_graphics=Graphics.FromImage(_bitmap);
            _image=new(width,height,96,96,PixelFormats.Pbgra32,null);_lastPose=null;
        }
        if(_lastPose==pose)return;_lastPose=pose;
        _graphics!.Clear(System.Drawing.Color.Transparent);_painter.Draw(_graphics,pose,width/(float)CatRig.CanvasWidth);_graphics.Flush();
        var data=_bitmap!.LockBits(new Rectangle(0,0,width,height),ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
        try{_image!.WritePixels(new Int32Rect(0,0,width,height),data.Scan0,data.Stride*height,data.Stride);}
        finally{_bitmap.UnlockBits(data);}
        // WritePixels invalidates its image resource. Rebuild the drawing only when the source changes.
        if(!ReferenceEquals(_drawnImage,_image))InvalidateVisual();
    }
    protected override void OnRender(DrawingContext dc)
    {
        double w=Math.Min(ActualWidth,ActualHeight*1.25),h=w*.8;
        if(_image is not null)dc.DrawImage(_image,new Rect((ActualWidth-w)/2,(ActualHeight-h)/2,w,h));_drawnImage=_image;
    }
    public void Dispose(){_graphics?.Dispose();_bitmap?.Dispose();_painter.Dispose();}
}
