using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CuteCat.Core;

namespace CuteCat.App;

public sealed class CatSurface : IDisposable
{
    public IntPtr Handle { get; }
    public long PaintCount { get; private set; }
    public int Bytes=>_width*_height*4;
    public V2 PaintedPaw { get; private set; }
    public CatPose PaintedPose { get; private set; }
    public bool LastPaintSucceeded { get; private set; }
    public event Action<V2>? PointerDown,PointerMove,PointerUp;
    public event Action? CaptureLost,DisplayChanged;
    public event Action<V2>? ContextRequested;
    private readonly Native.WndProc _proc;
    private readonly CatPainter _painter=new();
    private IntPtr _dc,_dib,_old,_bits;
    private Bitmap? _bitmap;
    private Graphics? _graphics;
    private int _width,_height;
    private bool _releasing;

    public CatSurface()
    {
        _proc=WindowProc;
        string cls="CuteCat.Vector."+Guid.NewGuid().ToString("N");
        var wc=new Native.WindowClass{Size=(uint)Marshal.SizeOf<Native.WindowClass>(),Proc=_proc,Instance=Native.GetModuleHandle(null),ClassName=cls,Cursor=Native.LoadCursor(IntPtr.Zero,new IntPtr(32649))};
        if(Native.RegisterClassEx(ref wc)==0)throw new Win32Exception();
        Handle=Native.CreateWindowEx(0x00080000|0x00000080|0x08000000|0x00000008,cls,"Cute Cat companion",0x80000000,0,0,1,1,IntPtr.Zero,IntPtr.Zero,wc.Instance,IntPtr.Zero);
        if(Handle==IntPtr.Zero)throw new Win32Exception();
        _dc=Native.CreateCompatibleDC(IntPtr.Zero);
    }
    public void Show(bool show)
    {
        Native.ShowWindow(Handle,show?4:0);
        // A hidden launcher STARTUPINFO must not override the explicit Show cat preference.
        if(show)Raise();
    }
    // Keeps desktop peers behind the pet. The Windows notification band additionally
    // requires a Windows-granted UIAccess token; TOPMOST alone cannot cross it.
    public void Raise()=>Native.SetWindowPos(Handle,new IntPtr(-1),0,0,0,0,0x1|0x2|0x10|0x40);
    public bool Paint(Companion cat)
    {
        int width=(int)Math.Ceiling(CatRig.CanvasWidth*cat.Scale),height=(int)Math.Ceiling(CatRig.CanvasHeight*cat.Scale);
        EnsureBuffer(width,height);
        _graphics!.Clear(Color.Transparent);
        _painter.Draw(_graphics,cat.Pose,(float)cat.Scale);
        _graphics.Flush();
        var pos=new Native.Point((int)Math.Round(cat.Position.X-CatRig.Pivot.X*cat.Scale),(int)Math.Round(cat.Position.Y-CatRig.Pivot.Y*cat.Scale));
        var size=new Native.Size(width,height);var origin=new Native.Point();var blend=new Native.Blend{Alpha=255,Format=1};
        LastPaintSucceeded=Native.UpdateLayeredWindow(Handle,IntPtr.Zero,ref pos,ref size,_dc,ref origin,0,ref blend,2);
        if(LastPaintSucceeded)
        {
            PaintCount++;PaintedPose=cat.Pose;
            V2 paw=CatRig.Paw(cat.Pose);
            PaintedPaw=new(pos.X+paw.X*cat.Scale,pos.Y+paw.Y*cat.Scale);
        }
        return LastPaintSucceeded;
    }
    public void SaveFrame(string path) { _bitmap?.Save(path,ImageFormat.Png); }
    public byte AlphaAt(int x,int y)=>x<0||y<0||x>=_width||y>=_height?(byte)0:Marshal.ReadByte(_bits,(y*_width+x)*4+3);
    private void EnsureBuffer(int w,int h)
    {
        if(w==_width && h==_height)return;
        ReleaseBuffer();_width=w;_height=h;
        var info=new Native.BitmapInfo{Size=40,Width=w,Height=-h,Planes=1,BitCount=32};
        _dib=Native.CreateDIBSection(_dc,ref info,0,out _bits,IntPtr.Zero,0);
        if(_dib==IntPtr.Zero)throw new Win32Exception();
        _old=Native.SelectObject(_dc,_dib);
        _bitmap=new Bitmap(w,h,w*4,PixelFormat.Format32bppPArgb,_bits);
        _graphics=Graphics.FromImage(_bitmap);
    }
    private IntPtr WindowProc(IntPtr h,uint msg,IntPtr wp,IntPtr lp)
    {
        if(msg==0x21)return new IntPtr(3); // MA_NOACTIVATE: petting never takes typing focus.
        if(msg is 0x201 or 0x200 or 0x202)
        {
            var p=new Native.Point((short)(lp.ToInt64()&0xffff),(short)((lp.ToInt64()>>16)&0xffff));Native.ClientToScreen(h,ref p);
            var v=new V2(p.X,p.Y);
            if(msg==0x201) { Native.SetCapture(h);PointerDown?.Invoke(v); }
            else if(msg==0x200) PointerMove?.Invoke(v);
            else { _releasing=true;Native.ReleaseCapture();_releasing=false;PointerUp?.Invoke(v); }
            return IntPtr.Zero;
        }
        if(msg==0x215 && !_releasing)CaptureLost?.Invoke();
        if(msg==0x205)
        {
            var point=new Native.Point((short)(lp.ToInt64()&0xffff),(short)((lp.ToInt64()>>16)&0xffff));Native.ClientToScreen(h,ref point);
            ContextRequested?.Invoke(new(point.X,point.Y));return IntPtr.Zero;
        }
        if(msg is 0x2e0 or 0x7e or 0x1a)DisplayChanged?.Invoke();
        return Native.DefWindowProc(h,msg,wp,lp);
    }
    private void ReleaseBuffer()
    {
        _graphics?.Dispose();_bitmap?.Dispose();_graphics=null;_bitmap=null;
        if(_old!=IntPtr.Zero) { Native.SelectObject(_dc,_old);_old=IntPtr.Zero; }
        if(_dib!=IntPtr.Zero) { Native.DeleteObject(_dib);_dib=IntPtr.Zero; }
    }
    public void Dispose() { Show(false);Native.DestroyWindow(Handle);ReleaseBuffer();Native.DeleteDC(_dc);_painter.Dispose(); }
}
