using System.Drawing;
using System.Drawing.Drawing2D;
using CuteCat.Core;

namespace CuteCat.App;

public sealed partial class CatPainter
{
    private string? _coat;
    private static PetAppearance Appearance(CatPose pose)=>pose.Appearance??PetAppearance.FromLegacy(pose.Accessory,pose.AccessoryColor);
    private static Color Shade(Color color,double factor)=>Color.FromArgb((int)(color.R*factor),(int)(color.G*factor),(int)(color.B*factor));
    private void ApplyCoat(string hex)
    {
        if(_coat==hex)return;_coat=hex;
        var body=ColorTranslator.FromHtml(PetAppearance.Color(hex,PetAppearance.Oat));
        bool dark=(body.R*.2126+body.G*.7152+body.B*.0722)<125;
        var outline=hex==PetAppearance.Oat?Color.FromArgb(74,70,58):Shade(body,dark?.5:.39);
        var ink=dark?Color.FromArgb(252,240,217):Color.FromArgb(64,61,52);
        _cream.Color=body;_tailFill.Color=body;_legFill.Color=body;
        _legFar.Color=hex==PetAppearance.Oat?Color.FromArgb(228,213,181):Shade(body,.85);
        _outline.Color=outline;_tailEdge.Color=outline;_legEdge.Color=outline;
        _ink.Color=ink;_face.Color=ink;_shine.Color=dark?Shade(body,.6):Color.FromArgb(255,250,235);
    }
    private static GraphicsState AtNeck(Graphics g,CatPose p)
    {
        var saved=g.Save();var center=CatRig.Project(new(205-p.Curl*24-p.Sit*4,140+p.HeadDrop+p.Bob-p.Sit*5),p.HeadYaw);
        float scale=(float)(1-.13*p.Curl);
        g.TranslateTransform((float)center.X,(float)center.Y);g.RotateTransform((float)(p.HeadTilt*p.HeadYaw));g.ScaleTransform(scale,scale);g.TranslateTransform(0,36);
        return saved;
    }
    private static void DrawCollar(Graphics g,CatPose p,PetAppearance look)
    {
        if(look.Collar==CatCollar.None)return;
        var saved=AtNeck(g,p);var color=ColorTranslator.FromHtml(look.CollarColor);
        using var edge=new Pen(Shade(color,.53),7){StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var band=new Pen(color,4){StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var strap=new GraphicsPath();strap.AddBezier(-30,-1,-19,14,17,14,29,-1);g.DrawPath(edge,strap);g.DrawPath(band,strap);
        using var line=new Pen(Shade(color,.5),1.6f){StartCap=LineCap.Round,EndCap=LineCap.Round};
        if(look.Collar!=CatCollar.Classic)
        {
            // The neckwear is painted afterwards and naturally covers a pendant beneath it.
            g.TranslateTransform((float)(8*p.HeadYaw),9);g.DrawLine(line,0,0,0,5);
            using var gold=new SolidBrush(Color.FromArgb(230,189,102));
            if(look.Collar==CatCollar.Bell){g.FillEllipse(gold,-5,3,10,11);g.DrawEllipse(line,-5,3,10,11);g.DrawLine(line,0,10,0,13);}
            else
            {
                using var heart=new GraphicsPath();heart.AddBezier(0,6,-7,-1,-10,8,0,15);heart.AddBezier(0,15,10,8,7,-1,0,6);
                g.FillPath(gold,heart);g.DrawPath(line,heart);
            }
        }
        g.Restore(saved);
    }
    private static void DrawScarf(Graphics g,CatPose p,PetAppearance look)
    {
        var saved=AtNeck(g,p);var color=ColorTranslator.FromHtml(look.NeckwearColor);
        using var fill=new SolidBrush(color);using var edge=new Pen(Shade(color,.55),2){LineJoin=LineJoin.Round};
        float side=(float)(-23*p.HeadYaw-13*Math.Sqrt(1-p.HeadYaw*p.HeadYaw));
        using var tail=new GraphicsPath();tail.AddBezier(side-5,2,side-10,11,side-10,21,side-6,25);tail.AddLine(side-6,25,side+6,21);tail.AddBezier(side+6,21,side+2,14,side+7,9,side+4,3);tail.CloseFigure();
        g.FillPath(fill,tail);g.DrawPath(edge,tail);
        using var bandEdge=new Pen(edge.Color,10){StartCap=LineCap.Round,EndCap=LineCap.Round};using var band=new Pen(color,6){StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var wrap=new GraphicsPath();wrap.AddBezier(-29,-2,-21,12,18,12,29,-2);g.DrawPath(bandEdge,wrap);g.DrawPath(band,wrap);
        using var stitch=new Pen(Color.FromArgb(230,239,231,213),1);g.DrawLine(stitch,side-5,18,side+1,16);g.Restore(saved);
    }
    private static void DrawHat(Graphics g,PetAppearance look)
    {
        var color=ColorTranslator.FromHtml(look.HatColor);using var fill=new SolidBrush(color);
        using var edge=new Pen(Shade(color,.5),2.3f){LineJoin=LineJoin.Round,StartCap=LineCap.Round,EndCap=LineCap.Round};
        using var light=new Pen(Color.FromArgb(185,255,250,232),1.5f){StartCap=LineCap.Round};
        using var shape=new GraphicsPath();
        if(look.Hat==CatHat.Beanie)
        {
            shape.AddBezier(-32,-36,-36,-80,31,-81,34,-36);shape.CloseFigure();g.FillPath(fill,shape);g.DrawPath(edge,shape);
            using var cuff=new GraphicsPath();cuff.AddBezier(-34,-42,-15,-46,18,-46,35,-42);cuff.AddLine(35,-42,34,-32);cuff.AddBezier(34,-32,14,-29,-16,-29,-34,-33);cuff.CloseFigure();
            g.FillPath(fill,cuff);g.DrawPath(edge,cuff);for(int i=-25;i<=25;i+=10)g.DrawLine(light,i,-39,i,-34);
            g.FillEllipse(fill,-8,-81,16,15);g.DrawEllipse(edge,-8,-81,16,15);
        }
        else if(look.Hat==CatHat.Beret)
        {
            shape.AddBezier(-36,-39,-52,-64,15,-82,38,-58);shape.AddBezier(38,-58,47,-45,22,-35,4,-34);shape.CloseFigure();
            g.FillPath(fill,shape);g.DrawPath(edge,shape);g.DrawLine(edge,9,-66,11,-74);
            using var band=new Pen(Shade(color,.82),7){StartCap=LineCap.Round,EndCap=LineCap.Round};g.DrawBezier(band,-27,-37,-9,-40,14,-40,29,-39);
        }
        else if(look.Hat==CatHat.Sunhat)
        {
            shape.AddBezier(-29,-39,-30,-56,-19,-65,0,-64);shape.AddBezier(0,-64,22,-64,29,-54,29,-39);shape.CloseFigure();g.FillPath(fill,shape);g.DrawPath(edge,shape);
            using var ribbon=new Pen(Shade(color,.68),6);g.DrawBezier(ribbon,-28,-41,-12,-35,17,-35,28,-41);
            g.FillEllipse(fill,-51,-41,102,14);g.DrawEllipse(edge,-51,-41,102,14);g.DrawBezier(light,-40,-35,-20,-29,19,-29,40,-35);
        }
        else if(look.Hat==CatHat.PartyHat)
        {
            shape.AddBezier(-23,-35,-17,-54,-8,-73,0,-90);shape.AddBezier(0,-90,8,-73,19,-49,24,-35);shape.AddBezier(24,-35,8,-29,-7,-29,-23,-35);shape.CloseFigure();
            g.FillPath(fill,shape);g.DrawPath(edge,shape);using var dot=new SolidBrush(Color.FromArgb(249,235,197));
            g.FillEllipse(dot,-4,-74,7,7);g.FillEllipse(dot,3,-56,8,8);g.FillEllipse(dot,-14,-43,7,7);g.FillEllipse(dot,-5,-95,10,10);g.DrawEllipse(edge,-5,-95,10,10);
        }
    }
}
