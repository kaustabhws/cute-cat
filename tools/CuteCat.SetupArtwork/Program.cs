using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using CuteCat.App;
using CuteCat.Core;

string output=Path.GetFullPath(args[0]);Directory.CreateDirectory(output);
using var painter=new CatPainter();
foreach(bool dark in new[]{false,true})
{
    using var panel=new Bitmap(328,628);using var g=Graphics.FromImage(panel);
    g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
    g.Clear(ColorTranslator.FromHtml(dark?"#263C31":"#EAF0E4"));
    using var ink=new SolidBrush(ColorTranslator.FromHtml(dark?"#F7F2DF":"#294936"));
    using var muted=new SolidBrush(ColorTranslator.FromHtml(dark?"#BACBB6":"#687D61"));
    using var title=new Font("Georgia",35,FontStyle.Regular,GraphicsUnit.Pixel);
    using var label=new Font("Segoe UI",16,FontStyle.Regular,GraphicsUnit.Pixel);
    g.DrawString("A little\ncompany.",title,ink,new RectangleF(30,68,276,110));
    g.DrawString("CUTE CAT",label,muted,32,30);
    using var ground=new Pen(ColorTranslator.FromHtml(dark?"#4D6853":"#CBD8C3"),2);
    g.DrawLine(ground,44,514,284,514);
    var pose=CatRig.Evaluate(CatAction.Idle,0,0,1) with{BodyYaw=.4,HeadYaw=.2,TailYaw=.6};
    painter.Draw(g,pose,1.18f,-24,242);
    g.DrawString("Your desk. A softer pace.",label,muted,32,568);
    panel.Save(Path.Combine(output,dark?"wizard-dark.bmp":"wizard-light.bmp"),ImageFormat.Bmp);
    using var icon=new Bitmap(110,110);using(var ig=Graphics.FromImage(icon))
    {ig.Clear(ColorTranslator.FromHtml(dark?"#202020":"#FFFFFF"));painter.DrawIcon(ig,110);}
    icon.Save(Path.Combine(output,dark?"small-dark.bmp":"small-light.bmp"),ImageFormat.Bmp);
}
using(var sheet=new Bitmap(1120,1150))using(var g=Graphics.FromImage(sheet))
{
    var kinds=new[]{PetAccessory.Bandana,PetAccessory.BowTie,PetAccessory.BellCollar,PetAccessory.Flower};
    using var label=new Font("Segoe UI",13,FontStyle.Regular,GraphicsUnit.Pixel);
    for(int row=0;row<5;row++)for(int col=0;col<4;col++)
    {
        using var background=new SolidBrush(ColorTranslator.FromHtml(row%2==0?"#F8F7F0":"#263C31"));g.FillRectangle(background,col*280,row*230,280,230);
        var action=row==3?CatAction.Groom:row==4?CatAction.Sleep:CatAction.Idle;
        double yaw=row==1?0:row==2?-1:1;
        var pose=CatRig.Evaluate(action,1.5,.1,1) with{Accessory=kinds[col],AccessoryColor="Rose",HeadYaw=yaw,BodyYaw=yaw,TailYaw=yaw};
        painter.Draw(g,pose,.85f,col*280+5,row*230+9);
        g.DrawString(kinds[col]+" / "+(row==1?"front":row==2?"left":action.ToString()),label,row%2==0?Brushes.DarkSlateGray:Brushes.White,col*280+18,row*230+13);
    }
    sheet.Save(Path.Combine(output,"accessory-placement.png"),ImageFormat.Png);
}
using(var sheet=new Bitmap(1080,500))using(var g=Graphics.FromImage(sheet))
{
    using var label=new Font("Segoe UI",14,FontStyle.Regular,GraphicsUnit.Pixel);
    for(int row=0;row<2;row++)for(int col=0;col<4;col++)
    {
        using var background=new SolidBrush(ColorTranslator.FromHtml(row==0?"#F8F7F0":"#263C31"));g.FillRectangle(background,col*270,row*250,270,250);
        CatAction action=new[]{CatAction.Idle,CatAction.Notice,CatAction.Wake,CatAction.Celebrate}[col];
        var pose=CatRig.Evaluate(action,.7,.1,col==0?7.1:1) with{Accessory=row==0?PetAccessory.Flower:PetAccessory.Bandana,AccessoryColor="Rose",HeadYaw=col==1?.3:1};
        painter.Draw(g,pose,.8f,col*270,row*250+25);
        g.DrawString(new[]{"A tiny ear twitch","Something caught my eye","A sleepy yawn","A little celebration"}[col],label,row==0?Brushes.DarkSlateGray:Brushes.White,col*270+15,row*250+15);
    }
    sheet.Save(Path.Combine(output,"personality.png"),ImageFormat.Png);
}
