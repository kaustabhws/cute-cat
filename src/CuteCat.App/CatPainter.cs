using System.Drawing;
using System.Drawing.Drawing2D;
using CuteCat.Core;

namespace CuteCat.App;

/// <summary>Original soft, toy-like cat. Orientation moves the rig in depth; it never flips a bitmap.</summary>
public sealed partial class CatPainter : IDisposable
{
    private readonly SolidBrush _cream=new(Color.FromArgb(252,240,213));
    private readonly SolidBrush _ink=new(Color.FromArgb(64,61,52));
    private readonly SolidBrush _ear=new(Color.FromArgb(226,171,151));
    private readonly SolidBrush _blush=new(Color.FromArgb(236,184,165));
    private readonly SolidBrush _tongue=new(Color.FromArgb(224,148,138));
    private readonly SolidBrush _shine=new(Color.FromArgb(255,250,235));
    private readonly Pen _outline=new(Color.FromArgb(74,70,58),4.3f){LineJoin=LineJoin.Round,StartCap=LineCap.Round,EndCap=LineCap.Round};
    private readonly Pen _face=new(Color.FromArgb(64,61,52),3.1f){LineJoin=LineJoin.Round,StartCap=LineCap.Round,EndCap=LineCap.Round};
    private readonly Pen _tailEdge=new(Color.FromArgb(74,70,58),25){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round};
    private readonly Pen _tailFill=new(Color.FromArgb(252,240,213),16.4f){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round};
    private readonly Pen _legEdge=new(Color.FromArgb(74,70,58),23){StartCap=LineCap.Round,EndCap=LineCap.Round};
    private readonly Pen _legFill=new(Color.FromArgb(252,240,213),14.4f){StartCap=LineCap.Round,EndCap=LineCap.Round};
    private readonly Pen _legFar=new(Color.FromArgb(228,213,181),14.4f){StartCap=LineCap.Round,EndCap=LineCap.Round};

    public void Draw(Graphics g,CatPose p,float scale,float offsetX=0,float offsetY=0)
    {
        ApplyCoat(Appearance(p).CoatColor);
        ApplyPattern(Appearance(p));
        var saved=g.Save();
        g.SmoothingMode=SmoothingMode.AntiAlias;g.PixelOffsetMode=PixelOffsetMode.HighQuality;
        g.TranslateTransform(offsetX,offsetY);g.ScaleTransform(scale,scale);
        float curl=(float)p.Curl,bob=(float)p.Bob;
        float rootY=bob+curl*8;
        double front=Math.Sqrt(Math.Max(0,1-p.BodyYaw*p.BodyYaw));

        // A shorter plush hook sweeps behind the shoulder while the body turns.
        using(var tail=new GraphicsPath())
        {
            float wave=(float)p.Tail*20;
            float L(float a,float b)=>a+(b-a)*curl;
            PointF T(float x,float y,double depth)=>ToPoint(CatRig.Project(new(x,y),p.TailYaw,depth));
            tail.AddBezier(T(L(115,102),L(187+rootY,196),5),T(L(77,100),L(187+rootY,232),8),
                T(L(62+wave,158),L(153,238),20),T(L(64+wave,192),L(135,221),27));
            tail.AddBezier(T(L(64+wave,192),L(135,221),27),T(L(65+wave,200),L(117,219),29),
                T(L(47+wave,204),L(119,212),38),T(L(50+wave,197),L(139,211),38));
            g.DrawPath(_tailEdge,tail);g.DrawPath(_tailFill,tail);
            if(Appearance(p).Pattern==CoatPattern.Tabby)g.DrawPath(_tailStripes,tail);
        }

        Leg(g,new(126,189+rootY),new V2(123,214)+p.HindFar,true,p.BodyYaw,24,curl);
        Leg(g,new(199,186+rootY),new V2(197,214)+p.ForeFar,true,p.BodyYaw,24,curl);

        // This cross-section stays round at the front view: no zero-width squash at the midpoint.
        float cx=(float)(160-10*p.BodyYaw);
        float half=(float)((62+p.Stretch*48)*(1-front)+45*front);
        float left=cx-half,right=cx+half;
        float top=140-(float)p.Sit*7+rootY+curl*5,belly=211+curl*6;
        using(var body=new GraphicsPath())
        {
            body.AddBezier(left,top+34,left-2,top+4,cx-35,top-4,cx,top);
            body.AddBezier(cx,top,right-11,top-2,right+5,top+13,right,top+39);
            body.AddBezier(right,top+39,right+1,belly+4,cx+23,belly+4,cx,belly);
            body.AddBezier(cx,belly,left+15,belly+6,left-6,belly-9,left,top+34);
            body.CloseFigure();g.FillPath(_cream,body);BodyPattern(g,p,body,cx,half,top,belly);g.DrawPath(_outline,body);
        }

        Leg(g,new(119,190+rootY),new V2(116,219)+p.HindNear,false,p.BodyYaw,-24,curl);
        CoverJoint(g,new(119,191+rootY),p.BodyYaw,-24,33,29);
        V2 paw=CatRig.CanonicalPaw(p);
        Leg(g,new(201,186+rootY),paw,false,p.BodyYaw,-24,0);
        CoverJoint(g,new(201,184+rootY),p.BodyYaw,-24,33,29);
        DrawNeckAccessory(g,p);
        DrawHead(g,p);
        if(p.Sip>.01)DrawWater(g,p);
        if(p.Celebration>.01)
        {
            using var sparkle=new Pen(Color.FromArgb((int)(210*p.Celebration),204,163,79),2.4f){StartCap=LineCap.Round,EndCap=LineCap.Round};
            foreach(var point in new[]{new V2(116,85),new V2(252,70),new V2(270,128)})
            {var at=CatRig.Project(point,p.HeadYaw);float x=(float)at.X,y=(float)at.Y,size=(float)(3+4*p.Celebration);g.DrawLine(sparkle,x-size,y,x+size,y);g.DrawLine(sparkle,x,y-size,x,y+size);}
        }
        if(p.Curl>.05)DrawSleep(g,p);
        if(p.ForeNear.Y < -25 && p.Reach<.01)
            Leg(g,new(201,186+rootY),paw,false,p.BodyYaw,-24,0);
        g.Restore(saved);
    }

    private static PointF ToPoint(V2 value)=>new((float)value.X,(float)value.Y);

    private void CoverJoint(Graphics g,V2 hip,double yaw,double depth,float width,float height)
    {
        V2 at=CatRig.Project(hip,yaw,depth);
        g.FillEllipse(_cream,(float)at.X-width/2,(float)at.Y-height/2,width,height);
    }

    private void Leg(Graphics g,V2 hip,V2 foot,bool far,double yaw,double depth,double curl)
    {
        foot=V2.Lerp(foot,new(hip.X+5,210),curl);
        double direction=foot.Y>=hip.Y?1:-1;
        using var path=new GraphicsPath();
        path.AddBezier(ToPoint(CatRig.Project(hip,yaw,depth)),
            ToPoint(CatRig.Project(new(hip.X-2,hip.Y+direction*12),yaw,depth)),
            ToPoint(CatRig.Project(new(foot.X-2,foot.Y-direction*7),yaw,depth)),ToPoint(CatRig.Project(foot,yaw,depth)));
        g.DrawPath(_legEdge,path);g.DrawPath(far?_legFar:_legFill,path);
    }

    private void DrawHead(Graphics g,CatPose p)
    {
        var saved=g.Save();
        V2 center=CatRig.Project(new(205-p.Curl*24-p.Sit*4,140+p.HeadDrop+p.Bob-p.Sit*5),p.HeadYaw);
        g.TranslateTransform((float)center.X,(float)center.Y);
        g.RotateTransform((float)(p.HeadTilt*p.HeadYaw));
        float headScale=(float)(1-.13*p.Curl);
        g.ScaleTransform(headScale*(1-.035f*(float)Math.Abs(p.HeadYaw)),headScale);
        PointF E(float x,float y,bool left)
        {
            float cx=left?-29:31,cy=-30;double a=(left?p.EarLeft:p.EarRight)*Math.PI/180;
            return new(cx+(float)((x-cx)*Math.Cos(a)-(y-cy)*Math.Sin(a)),cy+(float)((x-cx)*Math.Sin(a)+(y-cy)*Math.Cos(a)));
        }
        using(var head=new GraphicsPath())
        {
            head.AddBezier(-51,2,-52,-12,-44,-24,-40,-29);
            head.AddBezier(new PointF(-40,-29),E(-42,-39,true),E(-44,-51,true),E(-38,-53,true));
            head.AddBezier(E(-38,-53,true),E(-31,-55,true),E(-20,-39,true),new PointF(-16,-35));
            head.AddBezier(-16,-35,-5,-37,7,-37,18,-34);
            head.AddBezier(new PointF(18,-34),E(25,-40,false),E(35,-53,false),E(41,-50,false));
            head.AddBezier(E(41,-50,false),E(47,-48,false),E(43,-34,false),new PointF(44,-25));
            head.AddBezier(44,-25,53,-13,57,2,51,18);
            head.AddBezier(51,18,46,38,20,42,1,41);
            head.AddBezier(1,41,-23,43,-49,34,-51,17);
            head.AddBezier(-51,17,-53,12,-53,6,-51,2);
            head.CloseFigure();g.FillPath(_cream,head);HeadPattern(g,p,head);g.DrawPath(_outline,head);
        }
        using(var ear=new GraphicsPath())
        {
            ear.AddBezier(E(-36,-40,true),E(-36,-45,true),E(-28,-37,true),E(-26,-32,true));ear.AddBezier(E(-26,-32,true),E(-30,-30,true),E(-34,-29,true),E(-35,-30,true));ear.CloseFigure();g.FillPath(_ear,ear);
            ear.Reset();ear.AddBezier(E(34,-36,false),E(39,-43,false),E(39,-33,false),E(38,-29,false));ear.AddLine(E(38,-29,false),E(29,-31,false));ear.CloseFigure();g.FillPath(_ear,ear);
        }
        using(var mark=new Pen(_ear.Color,3.3f){StartCap=LineCap.Round,EndCap=LineCap.Round})
        {g.DrawLine(mark,-8,-24,-5,-19);g.DrawLine(mark,1,-25,1,-19);g.DrawLine(mark,10,-24,7,-19);}

        DrawHeadAccessory(g,p);

        // Low-set facial features and soft cheeks keep the expression readable at desktop size.
        float look=(float)p.HeadYaw*3;
        float e=(float)Math.Clamp(p.Eyes*(1-p.Anger*.4),0,1.3);
        g.TranslateTransform(look,2);
        if(e>.12)
        {
            float h=10.5f*e;
            g.FillEllipse(_ink,-22,-h/2,7.4f,h);g.FillEllipse(_ink,17,-h/2,7.4f,h);
            if(e>.7) {g.FillEllipse(_shine,-20.8f,-h/2+1.2f,1.7f,1.7f);g.FillEllipse(_shine,18.2f,-h/2+1.2f,1.7f,1.7f);}
        }
        else if(p.Affection>.3)
        {
            using var eye=new GraphicsPath();eye.AddBezier(-23,2,-21,-4,-16,-4,-13,2);g.DrawPath(_face,eye);
            eye.Reset();eye.AddBezier(15,2,18,-4,23,-4,25,2);g.DrawPath(_face,eye);
        }
        else
        {
            using var eye=new GraphicsPath();eye.AddBezier(-23,1,-20,4,-16,4,-13,1);g.DrawPath(_face,eye);
            eye.Reset();eye.AddBezier(15,1,18,4,22,4,25,1);g.DrawPath(_face,eye);
        }
        g.FillEllipse(_blush,-34,10,14,6.5f);g.FillEllipse(_blush,25,10,14,6.5f);
        g.FillEllipse(_ink,-1,9,4.2f,3);
        if(p.Anger>.05)
        {
            using var brow=new Pen(Color.FromArgb((int)(255*p.Anger),_ink.Color),3.2f){StartCap=LineCap.Round,EndCap=LineCap.Round};
            g.DrawLine(brow,-25,-12,-14,-7);g.DrawLine(brow,16,-7,27,-12);
        }
        if(p.SleepBubble>.01)
        {
            float radius=6+(float)p.SleepBubble*13;
            using var bubble=new SolidBrush(Color.FromArgb((int)(115*p.Curl),210,236,232));
            using var edge=new Pen(Color.FromArgb((int)(170*p.Curl),145,179,173),1.5f);
            g.FillEllipse(bubble,3,8-radius,radius*1.45f,radius*1.5f);g.DrawEllipse(edge,3,8-radius,radius*1.45f,radius*1.5f);
            using var glint=new SolidBrush(Color.FromArgb((int)(180*p.Curl),255,255,250));g.FillEllipse(glint,8,10-radius,4,5);
        }
        else if(p.Anger>.4)
        {
            using var pout=new GraphicsPath();pout.AddBezier(-7,19,-3,14,5,14,9,19);g.DrawPath(_face,pout);
        }
        else if(p.Mouth>.08)
        {
            float m=(float)p.Mouth;g.FillEllipse(_ink,-5,14,12,3+12*m);
            g.FillEllipse(_tongue,-2,18+4*m,6,2+4*m);
        }
        else
        {
            using var mouth=new GraphicsPath();mouth.AddBezier(-9,16,-6,21,-1,21,1,14);mouth.AddBezier(1,14,3,21,8,21,11,16);g.DrawPath(_face,mouth);
        }
        g.Restore(saved);
    }

    private static Color OutfitColor(CatPose p)=>ColorTranslator.FromHtml(Appearance(p).NeckwearColor);

    // Neckwear belongs between the body and the head. Drawing it here lets the
    // chin occlude the collar and prevents clothing from becoming facial hair.
    private void DrawNeckAccessory(Graphics g,CatPose p)
    {
        var look=Appearance(p);DrawCollar(g,p,look);
        var accessory=look.Neckwear switch{CatNeckwear.Bandana=>PetAccessory.Bandana,CatNeckwear.BowTie=>PetAccessory.BowTie,_=>PetAccessory.None};
        if(look.Neckwear==CatNeckwear.Scarf)DrawScarf(g,p,look);
        else DrawNeckwear(g,p with{Accessory=accessory});
    }
    private void DrawNeckwear(Graphics g,CatPose p)
    {
        if(p.Accessory is PetAccessory.None or PetAccessory.Flower)return;
        var saved=g.Save();
        V2 center=CatRig.Project(new(205-p.Curl*24-p.Sit*4,140+p.HeadDrop+p.Bob-p.Sit*5),p.HeadYaw);
        float scale=(float)(1-.13*p.Curl);
        g.TranslateTransform((float)center.X,(float)center.Y);g.RotateTransform((float)(p.HeadTilt*p.HeadYaw));g.ScaleTransform(scale,scale);g.TranslateTransform(0,36);
        Color color=OutfitColor(p);
        using var fill=new SolidBrush(color);using var edge=new Pen(Color.FromArgb(80,91,73),2){LineJoin=LineJoin.Round};
        using var strapEdge=new Pen(Color.FromArgb(80,91,73),7){StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var strap=new Pen(color,4){StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var collar=new GraphicsPath();collar.AddBezier(-28,-2,-18,10,16,10,28,-2);g.DrawPath(strapEdge,collar);g.DrawPath(strap,collar);
        if(p.Accessory==PetAccessory.Bandana)
        {
            // A small side fold rests on the shoulder, away from the central chin.
            float X(float x)=>(float)((x+21)*(.6+.4*Math.Abs(p.HeadYaw))-21*p.HeadYaw-12*Math.Sqrt(Math.Max(0,1-p.HeadYaw*p.HeadYaw)));
            using var fold=new GraphicsPath();fold.AddBezier(X(-26),0,X(-21),3,X(-12),6,X(-9),8);
            fold.AddBezier(X(-9),8,X(-15),13,X(-20),18,X(-27),19);fold.AddBezier(X(-27),19,X(-29),11,X(-30),5,X(-26),0);
            g.FillPath(fill,fold);g.DrawPath(edge,fold);
            using var seam=new Pen(Color.FromArgb(240,230,223),1.1f);g.DrawLine(seam,X(-25),7,X(-24),14);
            g.FillEllipse(fill,X(-24)-3,1,7,7);g.DrawEllipse(edge,X(-24)-3,1,7,7);
        }
        else if(p.Accessory==PetAccessory.BowTie)
        {
            g.TranslateTransform((float)(6*p.HeadYaw),13);
            using var shape=new GraphicsPath();shape.AddBezier(-2,0,-10,-5,-18,-10,-18,-4);shape.AddBezier(-18,-4,-18,2,-18,9,-13,7);
            shape.AddLine(-13,7,-2,4);shape.AddLine(-2,4,2,4);shape.AddBezier(2,4,10,9,18,10,18,4);
            shape.AddBezier(18,4,18,-3,18,-9,13,-7);shape.AddLine(13,-7,2,0);shape.CloseFigure();
            g.FillPath(fill,shape);g.DrawPath(edge,shape);g.FillEllipse(fill,-3,-1,6,7);g.DrawEllipse(edge,-3,-1,6,7);
        }
        else if(p.Accessory==PetAccessory.BellCollar)
        {
            g.TranslateTransform((float)(8*p.HeadYaw),7);g.DrawLine(edge,0,0,0,5);
            using var gold=new SolidBrush(Color.FromArgb(225,186,100));g.FillEllipse(gold,-5,3,10,11);g.DrawEllipse(edge,-5,3,10,11);g.DrawLine(edge,0,10,0,13);
        }
        g.Restore(saved);
    }
    private static void DrawHeadAccessory(Graphics g,CatPose p)
    {
        var look=Appearance(p);
        if(look.Hat==CatHat.None)return;
        if(look.Hat!=CatHat.Flower){DrawHat(g,look);return;}
        var saved=g.Save();g.TranslateTransform(-29,-30);g.RotateTransform((float)p.EarLeft);g.TranslateTransform(29,30);
        using var fill=new SolidBrush(ColorTranslator.FromHtml(look.HatColor));
        for(int i=0;i<5;i++){double a=i*Math.PI*2/5;g.FillEllipse(fill,-38+(float)Math.Cos(a)*6,-38+(float)Math.Sin(a)*6,9,9);}
        using var gold=new SolidBrush(Color.FromArgb(238,200,119));g.FillEllipse(gold,-35,-35,8,8);
        g.Restore(saved);
    }
    private void DrawSleep(Graphics g,CatPose p)
    {
        using var font=new Font("Segoe UI",13,FontStyle.Bold,GraphicsUnit.Pixel);
        for(int i=0;i<3;i++)
        {
            double u=(p.SleepPhase*.32+i/3d)%1;
            using var ink=new SolidBrush(Color.FromArgb((int)(150*p.Curl*Math.Sin(Math.PI*u)),131,161,150));
            V2 at=CatRig.Project(new(238+30*u,123-63*u),p.HeadYaw);
            g.DrawString(i==2?"Z":"z",font,ink,(float)at.X,(float)at.Y);
        }
    }

    public void DrawIcon(Graphics g,int size)
    {
        ApplyCoat(PetAppearance.Oat);
        var saved=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(size/124f,size/124f);
        g.TranslateTransform(-98,-73);DrawHead(g,CatRig.Evaluate(CatAction.Idle,0,0,0) with{HeadYaw=0});g.Restore(saved);
    }

    public void Dispose()
    {
        _cream.Dispose();_ink.Dispose();_ear.Dispose();_blush.Dispose();_tongue.Dispose();_shine.Dispose();
        _outline.Dispose();_face.Dispose();_tailEdge.Dispose();_tailFill.Dispose();_legEdge.Dispose();_legFill.Dispose();_legFar.Dispose();
        _patternFill.Dispose();_secondPatch.Dispose();_stripes.Dispose();_tailStripes.Dispose();
    }
}
