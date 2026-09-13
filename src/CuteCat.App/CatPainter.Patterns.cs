using System.Drawing;
using System.Drawing.Drawing2D;
using CuteCat.Core;

namespace CuteCat.App;

public sealed partial class CatPainter
{
    private string? _patternHex;
    private readonly SolidBrush _patternFill=new(Color.Sienna),_secondPatch=new(Color.FromArgb(74,70,67));
    private readonly Pen _stripes=new(Color.Sienna,7){StartCap=LineCap.Round,EndCap=LineCap.Round};
    private readonly Pen _tailStripes=new(Color.Sienna,13){DashPattern=[.65f,1.25f],StartCap=LineCap.Round,EndCap=LineCap.Round};
    private void ApplyPattern(PetAppearance look)
    {
        if(_patternHex==look.PatternColor)return;_patternHex=look.PatternColor;
        var color=ColorTranslator.FromHtml(PetAppearance.Color(look.PatternColor,"#956950"));
        _patternFill.Color=color;_stripes.Color=color;_tailStripes.Color=color;
        _secondPatch.Color=Shade(color,.48);
    }
    private void BodyPattern(Graphics g,CatPose p,GraphicsPath body,float cx,float half,float top,float belly)
    {
        var pattern=Appearance(p).Pattern;if(pattern==CoatPattern.Solid)return;
        var state=g.Save();g.SetClip(body,CombineMode.Intersect);
        // Coordinates are attached to the deforming body, including the planted front-view turn.
        float X(float u)=>cx+u*half*(float)p.BodyYaw;
        float width=half*(.36f+.64f*(float)Math.Abs(p.BodyYaw));
        if(pattern==CoatPattern.Tabby)
        {
            for(int i=0;i<3;i++)
            {
                float x=X(-.65f+i*.46f);using var stripe=new GraphicsPath();
                stripe.AddBezier(x,top-2,x-7,top+13,x+8,top+20,x+2,top+31);g.DrawPath(_stripes,stripe);
            }
            g.FillEllipse(_patternFill,X(-.78f)-8,belly-19,15,6);
        }
        else if(pattern==CoatPattern.Tuxedo)
        {
            // A rounded jacket leaves a cream belly and a clean bib beneath the face.
            g.FillEllipse(_patternFill,cx-half-8,top-24,half*2+16,belly-top+2);
            g.FillEllipse(_cream,X(.77f)-width*.43f,top+11,width*.9f,belly-top+5);
        }
        else
        {
            g.FillEllipse(_patternFill,X(-.5f)-width*.55f,top-11,width*1.12f,48);
            g.FillEllipse(_secondPatch,X(.3f)-width*.4f,top+22,width*.83f,40);
        }
        g.Restore(state);
    }
    private void HeadPattern(Graphics g,CatPose p,GraphicsPath head)
    {
        var pattern=Appearance(p).Pattern;if(pattern is CoatPattern.Solid or CoatPattern.Tabby)return;
        var state=g.Save();g.SetClip(head,CombineMode.Intersect);
        // The central face stays the coat colour so its existing contrast-adaptive eyes remain readable.
        if(pattern==CoatPattern.Tuxedo)
        {
            using var cap=new GraphicsPath();cap.AddBezier(-60,4,-29,-12,-22,-28,0,-35);
            cap.AddBezier(0,-35,22,-28,28,-12,60,4);cap.AddLine(60,4,60,-64);cap.AddLine(60,-64,-60,-64);cap.CloseFigure();g.FillPath(_patternFill,cap);
        }
        else
        {
            // Asymmetric markings follow head yaw, rather than swapping sides mid-turn.
            float side=(float)p.HeadYaw;
            g.FillEllipse(_patternFill,-23-25*side,-57,43,38);
            g.FillEllipse(_secondPatch,-18+35*side,-48,34,26);
        }
        g.Restore(state);
    }
    private static void DrawWater(Graphics g,CatPose p)
    {
        var at=CatRig.Project(new(210,222),p.HeadYaw);float x=(float)at.X,y=(float)at.Y;
        int alpha=(int)(235*p.Sip);using var cup=new SolidBrush(Color.FromArgb(alpha,162,191,223));
        using var edge=new Pen(Color.FromArgb(alpha,75,104,143),2){LineJoin=LineJoin.Round};
        using var shape=new GraphicsPath();shape.AddBezier(x-14,y-8,x-13,y+10,x+13,y+10,x+14,y-8);shape.CloseFigure();
        g.FillPath(cup,shape);g.DrawPath(edge,shape);
        using var water=new SolidBrush(Color.FromArgb(alpha,213,238,247));g.FillEllipse(water,x-12,y-10,24,5);
    }
}
