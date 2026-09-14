using CuteCat.Core;

internal static class CaptionChecks
{
    public static void Run(Action<string,bool> check)
    {
        bool geometry=true,budget=true;
        foreach(double dpi in new[]{1d,1.25,1.5,2,3})foreach(int offset in new[]{-1920,0,450})
        {
            var window=new Area(offset,94,1000*dpi,650*dpi);
            var close=new Area(Math.Round(window.Right-56*dpi),Math.Round(window.Top+3*dpi),Math.Round(48*dpi),Math.Round(36*dpi));int calls=0;
            int? Hit(V2 p){calls++;return p.X>=close.Left&&p.X<close.Right&&p.Y>=close.Top&&p.Y<close.Bottom?20:1;}
            var result=NativeCaptionLocator.Find(window,dpi,null,Hit);
            geometry&=result is Area a&&(a.Center-close.Center).Length<1;budget&=calls<=64;
        }
        check("native close hit regions resolve across DPI and negative monitor coordinates",geometry);
        check("native caption probing is bounded",budget);
        var box=new Area(0,0,1000,700);
        check("client-area buttons are never inferred to be close",NativeCaptionLocator.Find(box,1,null,_=>1) is null);
        check("maximize and minimize hit results are rejected",NativeCaptionLocator.Find(box,1,null,_=>9) is null&&NativeCaptionLocator.Find(box,1,null,_=>8) is null);
        check("an implausibly large close hit region is rejected",NativeCaptionLocator.Find(box,1,null,_=>20) is null);
        check("unavailable native hit-testing fails safely",NativeCaptionLocator.Find(box,1,null,_=>null) is null);
        int attempt=0;check("a query that becomes unavailable does not invent a close target",NativeCaptionLocator.Find(box,1,null,_=>++attempt<5?20:null) is null);
        check("invalid geometry is never queried",NativeCaptionLocator.Find(new(double.NaN,0,1000,700),1,null,_=>throw new Exception()) is null);
        var actual=new Area(940,5,50,40);var hint=new Area(800,0,190,29);
        var found=NativeCaptionLocator.Find(box,1,hint,p=>p.X>=actual.Left&&p.X<actual.Right&&p.Y>=actual.Top&&p.Y<actual.Bottom?20:1);
        check("DWM bounds seed a measured region rather than an assumed button size",found==actual);
    }
}
