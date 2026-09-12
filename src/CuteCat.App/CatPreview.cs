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
    private readonly Bitmap _bitmap=new(640,512,PixelFormat.Format32bppPArgb);
    private readonly Graphics _graphics;
    private readonly WriteableBitmap _image=new(640,512,96,96,PixelFormats.Pbgra32,null);
    public CatPreview() { _graphics=Graphics.FromImage(_bitmap);Height=260;MinWidth=200;Focusable=false; }
    public void Update(CatPose pose)
    {
        _graphics.Clear(System.Drawing.Color.Transparent);_painter.Draw(_graphics,pose,2);_graphics.Flush();
        var data=_bitmap.LockBits(new Rectangle(0,0,640,512),ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
        try{_image.WritePixels(new Int32Rect(0,0,640,512),data.Scan0,data.Stride*512,data.Stride);}
        finally{_bitmap.UnlockBits(data);}
        InvalidateVisual();
    }
    protected override void OnRender(DrawingContext dc)
    {
        double w=Math.Min(ActualWidth,ActualHeight*1.25),h=w*.8;
        dc.DrawImage(_image,new Rect((ActualWidth-w)/2,(ActualHeight-h)/2,w,h));
    }
    public void Dispose(){_graphics.Dispose();_bitmap.Dispose();_painter.Dispose();}
}
