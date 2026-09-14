namespace CuteCat.Core;

/// <summary>Finds a close region only where the window explicitly returns HTCLOSE (20).</summary>
public static class NativeCaptionLocator
{
    public static Area? Find(Area window,double dpi,Area? captionHint,Func<V2,int?> query)
    {
        if(!window.IsValid||!double.IsFinite(dpi)||dpi<.5||dpi>8||window.Width<100*dpi||window.Height<60*dpi)return null;
        int left=(int)Math.Floor(window.Right-110*dpi),right=(int)Math.Ceiling(window.Right+2);
        int top=(int)Math.Floor(window.Top-2),bottom=(int)Math.Ceiling(window.Top+70*dpi);
        var samples=new Dictionary<V2,int>();bool unavailable=false;int queries=0;
        int? Hit(double x,double y,bool fresh=false)
        {
            var point=new V2(Math.Round(x),Math.Round(y));
            if(unavailable||point.X<short.MinValue||point.X>short.MaxValue||point.Y<short.MinValue||point.Y>short.MaxValue)return null;
            if(!fresh&&samples.TryGetValue(point,out int known))return known;
            if(queries++>=64){unavailable=true;return null;}
            int? hit=query(point);if(hit is null){unavailable=true;return null;}samples[point]=hit.Value;return hit;
        }
        V2? seed=null;
        void TrySeed(V2 point)
        {if(seed is null&&point.X>=left&&point.X<=right&&point.Y>=top&&point.Y<=bottom&&Hit(point.X,point.Y)==20)seed=new(Math.Round(point.X),Math.Round(point.Y));}
        if(captionHint is {IsValid:true} hint)TrySeed(new(hint.Right-Math.Min(18*dpi,hint.Width/2),hint.Center.Y));
        foreach(double y in new[]{14d,30,48})foreach(double x in new[]{18d,38,64})TrySeed(new(window.Right-x*dpi,window.Top+y*dpi));
        if(seed is not { } at||unavailable)return null;
        int Edge(int inside,int outside,bool horizontal)
        {
            int? H(int value)=>horizontal?Hit(value,at.Y):Hit(at.X,value);
            if(H(outside)==20||unavailable){unavailable=true;return inside;}
            while(Math.Abs(inside-outside)>1&&!unavailable)
            {
                int middle=(int)Math.Floor((inside+outside)/2d);
                if(H(middle)==20)inside=middle;else outside=middle;
            }
            return outside<inside?inside:outside;
        }
        int x1=Edge((int)at.X,left,true),x2=Edge((int)at.X,right,true);
        int y1=Edge((int)at.Y,top,false),y2=Edge((int)at.Y,bottom,false);
        var region=new Area(x1,y1,x2-x1,y2-y1);
        if(unavailable||!region.IsValid||region.Width<10||region.Height<10||region.Width>80*dpi||region.Height>64*dpi)return null;
        // Validate the measured region again; a changing or nonrectangular hit target is left alone.
        foreach(var point in new[]{region.Center,new V2(x1+2,y1+2),new V2(x2-2,y1+2),new V2(x1+2,y2-2),new V2(x2-2,y2-2)})
            if(Hit(point.X,point.Y,true)!=20)return null;
        return region;
    }
}
