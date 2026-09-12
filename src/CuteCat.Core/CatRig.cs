namespace CuteCat.Core;

public enum CatAction { Idle, Walk, Run, Groom, Sleep, Meow, Play, Drag, Land, Paw, Turn, Wake, Notice, Celebrate }

// All values are continuous rig parameters; there are no frame indices or image assets.
public readonly record struct CatPose(
    double Sit, double Curl, double Bob, double Stretch, double HeadTilt, double HeadDrop,
    double Tail, double Eyes, double Mouth, double Blush, double Reach,
    V2 HindFar, V2 ForeFar, V2 HindNear, V2 ForeNear,
    double BodyYaw=1, double HeadYaw=1, double TailYaw=1, V2? PawEnd=null,
    double SleepPhase=0,double SleepBubble=0,double Anger=0,PetAccessory Accessory=PetAccessory.None,string AccessoryColor="Sage",
    double EarLeft=0,double EarRight=0,double Celebration=0)
{
    public static CatPose Blend(CatPose a, CatPose b, double t) => new(
        Ease.Mix(a.Sit,b.Sit,t), Ease.Mix(a.Curl,b.Curl,t), Ease.Mix(a.Bob,b.Bob,t),
        Ease.Mix(a.Stretch,b.Stretch,t), Ease.Mix(a.HeadTilt,b.HeadTilt,t), Ease.Mix(a.HeadDrop,b.HeadDrop,t),
        Ease.Mix(a.Tail,b.Tail,t), Ease.Mix(a.Eyes,b.Eyes,t), Ease.Mix(a.Mouth,b.Mouth,t),
        Ease.Mix(a.Blush,b.Blush,t), Ease.Mix(a.Reach,b.Reach,t), V2.Lerp(a.HindFar,b.HindFar,t),
        V2.Lerp(a.ForeFar,b.ForeFar,t), V2.Lerp(a.HindNear,b.HindNear,t), V2.Lerp(a.ForeNear,b.ForeNear,t),
        PawEnd:V2.Lerp(a.PawEnd??CatRig.Contact,b.PawEnd??CatRig.Contact,t),SleepPhase:b.SleepPhase,
        SleepBubble:Ease.Mix(a.SleepBubble,b.SleepBubble,t),EarLeft:Ease.Mix(a.EarLeft,b.EarLeft,t),EarRight:Ease.Mix(a.EarRight,b.EarRight,t),Celebration:Ease.Mix(a.Celebration,b.Celebration,t));
}

public static class CatRig
{
    public const double CanvasWidth = 320, CanvasHeight = 256, CharacterHeight = 180;
    public static readonly V2 Pivot = new(160, 228);
    public static readonly V2 Contact = new(287, 107);
    public static V2 CanonicalPaw(CatPose pose) => V2.Lerp(
        V2.Lerp(new V2(204,219)+pose.ForeNear,new V2(198,210),pose.Curl),pose.PawEnd??Contact,pose.Reach);
    public static V2 Paw(CatPose pose) => Project(CanonicalPaw(pose),pose.BodyYaw,-24);
    public static V2 Project(V2 point,double yaw,double depth=0)
    {
        yaw=Math.Clamp(yaw,-1,1);
        return new(Pivot.X+(point.X-Pivot.X)*yaw+depth*Math.Sqrt(Math.Max(0,1-yaw*yaw)),point.Y);
    }
    public static V2 ContactOffset(double scale, int facing) => new((Contact.X-Pivot.X)*scale*facing,(Contact.Y-Pivot.Y)*scale);

    public static CatPose Evaluate(CatAction action, double age, double phase, double time, bool reduced = false,double strideScale=1)
    {
        double b = reduced ? 0 : Math.Sin(time * 1.9) * 1.15;
        double blinkClock = time % 5.7;
        double eyes = 1 - Ease.Pulse(blinkClock, 4.8, .13);
        CatPose p = new(.12,0,b,0,0,0,Math.Sin(time*1.65)*.12,eyes,0,0,0,default,default,default,default);
        if(!reduced)p=p with{EarLeft=-10*Ease.Pulse(time%9.3,7.1,.24),EarRight=8*Ease.Pulse(time%9.3,7.37,.26)};
        double w = phase * 2 * Math.PI;
        switch (action)
        {
            case CatAction.Walk:
            case CatAction.Run:
                bool run = action == CatAction.Run;
                double stride = run ? 26*Math.Clamp(strideScale,1,2) : 17, lift = run ? 19 : 11;
                p = p with { Sit=0, Bob=run ? -4-5*Math.Cos(w*2) : -1.5-1.6*Math.Cos(w*2),
                    Stretch=run ? .07*Math.Sin(w*2) : .018*Math.Sin(w*2), HeadTilt=run ? -5 : -1,
                    Tail=.24+Math.Sin(w-.7)*.13,
                    HindFar=Foot(phase+.5,stride,lift), ForeFar=Foot(phase+(run?.62:0),stride,lift),
                    HindNear=Foot(phase,stride,lift), ForeNear=Foot(phase+(run?.12:.5),stride,lift) };
                break;
            case CatAction.Groom:
                double lick = .5 + .5 * Math.Sin(age * 5.3);
                p = p with { Sit=.75, HeadTilt=10+lick*7, HeadDrop=6+lick*3, Eyes=.18,
                    ForeNear=new(2-12*lick,-55-lick*20), Mouth=lick*.48, Blush=.6, Tail=-.25 };
                break;
            case CatAction.Sleep:
                p = p with { Curl=1, Sit=.45, Bob=b*.55, HeadDrop=36, HeadTilt=16, Eyes=0, Tail=-.8,
                    SleepPhase=reduced?.4:age,SleepBubble=reduced?.72:.66+.22*Math.Sin(age*1.85) };
                break;
            case CatAction.Wake:
                double stretch=Ease.Pulse(age,.65,.65);
                p=p with{Sit=.2,Curl=1-Ease.Smooth(age/.7),Stretch=.12*stretch,HeadTilt=-12*stretch,
                    HeadDrop=15*(1-Ease.Smooth(age/.7)),Eyes=.15+.85*Ease.Smooth(age/.9),
                    ForeNear=new(17*stretch,-12*stretch),ForeFar=new(12*stretch,-8*stretch),Tail=.4*stretch,Mouth=.95*stretch,Blush=.5};
                break;
            case CatAction.Notice:
                p=p with{HeadTilt=-7*Ease.Smooth(age/.18),Eyes=1.16,EarLeft=-6,EarRight=5,Tail=.28};
                break;
            case CatAction.Celebrate:
                double hop=Ease.Pulse(age,.68,.48),joy=Ease.Pulse(age,.8,.8);
                p=p with{Bob=-18*hop,Stretch=.07*hop,HeadTilt=-8*joy,Eyes=.08,Mouth=.55*joy,Blush=1,Tail=.45,
                    ForeNear=new(4*hop,-18*hop),ForeFar=new(0,-14*hop),HindNear=new(0,-8*hop),HindFar=new(0,-6*hop),Celebration=joy};
                break;
            case CatAction.Meow:
                double mew = Ease.Pulse(age%2.1,.78,.56);
                p = p with { Sit=.58, HeadTilt=-7*mew, HeadDrop=-3*mew, Eyes=.85-.55*mew, Mouth=mew, Blush=.7 };
                break;
            case CatAction.Play:
                double bounce = Math.Max(0,Math.Sin(age*4.4));
                p = p with { Bob=-25*bounce, Stretch=.12*Math.Sin(age*4.4), HeadTilt=-12*bounce,
                    ForeNear=new(12*bounce,-38*bounce), HindNear=new(-8*bounce,-29*bounce),
                    HindFar=new(0,-25*bounce),ForeFar=new(0,-28*bounce),Tail=.55, Blush=.5 };
                break;
            case CatAction.Drag:
                p = p with { Sit=.1, Stretch=-.07, Eyes=1.22, Mouth=.35, HeadTilt=-7,
                    ForeNear=new(-6,10), ForeFar=new(0,8), HindNear=new(5,9), HindFar=new(4,7), Tail=.5 };
                break;
            case CatAction.Land:
                p = p with { Stretch=.12*(1-Ease.Smooth(age/.55)), Bob=5*(1-Ease.Smooth(age/.55)), Blush=.6 };
                break;
            case CatAction.Paw:
                double reach = Ease.Smooth(age/.44) * (1-Ease.Smooth((age-.82)/.32));
                p = p with { Sit=.3, Reach=reach, HeadTilt=-12*reach, Eyes=1, Tail=.3 };
                break;
            case CatAction.Turn:
                double shift=Math.Sin(Math.PI*Math.Clamp(age/TurnTransition.Duration,0,1));
                p=p with {Sit=.24,Bob=2.4*shift,Stretch=.025*shift,HeadTilt=-4*shift,
                    HindNear=new(0,-5*Ease.Pulse(age,.31,.20)),ForeFar=new(0,-6*Ease.Pulse(age,.38,.21)),
                    ForeNear=new(0,-6*Ease.Pulse(age,.57,.21)),HindFar=new(0,-4*Ease.Pulse(age,.63,.18))};
                break;
        }
        if (reduced) p = p with { Bob=0, Tail=0, HindFar=default,ForeFar=default,HindNear=default,ForeNear=default,Stretch=0 };
        return p;
    }

    // The stance travels linearly opposite the body; the airborne paw returns on a smooth arc.
    public static V2 Foot(double phase, double stride, double lift)
    {
        double u = phase-Math.Floor(phase);
        if (u < .62) return new(stride*(1-2*u/.62),0);
        double swing=(u-.62)/.38;
        return new(stride*(2*Ease.Smooth(swing)-1),-lift*Math.Pow(Math.Sin(Math.PI*swing),2));
    }
}
