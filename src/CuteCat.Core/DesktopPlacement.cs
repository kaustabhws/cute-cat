namespace CuteCat.Core;

public sealed record MonitorSpot(string Monitor,double X,double Y);
public static class DesktopPlacement
{
    public static Area Footprint(V2 position,double scale)=>new(position.X-145*scale,position.Y-170*scale,290*scale,184*scale);
    public static bool Intersects(Area a,Area b)=>a.IsValid&&b.IsValid&&a.Left<b.Right&&a.Right>b.Left&&a.Top<b.Bottom&&a.Bottom>b.Top;
    public static V2 Restore(MonitorSpot? spot,Area work,double scale)=>work.Clamp(spot is null?
        new V2(work.Right-174*scale,work.Bottom-20*scale):new(work.Left+work.Width*spot.X,work.Top+work.Height*spot.Y),158*scale,170*scale,20*scale);
    public static MonitorSpot Remember(string monitor,V2 position,Area work)=>new(monitor,
        Math.Clamp((position.X-work.Left)/work.Width,0,1),Math.Clamp((position.Y-work.Top)/work.Height,0,1));
    public static V2? Avoid(V2 current,Area work,double scale,Area protectedArea)
    {
        if(!work.IsValid||!protectedArea.IsValid||!Intersects(Footprint(current,scale),protectedArea))return null;
        var candidates=new[]{new V2(work.Left+174*scale,work.Bottom-20*scale),new V2(work.Right-174*scale,work.Bottom-20*scale),
            new V2(work.Left+174*scale,work.Top+185*scale),new V2(work.Right-174*scale,work.Top+185*scale)};
        return candidates.Select(p=>work.Clamp(p,158*scale,170*scale,20*scale)).Where(p=>!Intersects(Footprint(p,scale),protectedArea))
            .OrderBy(p=>(p-current).Length).Select(p=>(V2?)p).FirstOrDefault();
    }
}
