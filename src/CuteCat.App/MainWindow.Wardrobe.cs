using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CuteCat.Core;

namespace CuteCat.App;

public partial class MainWindow
{
    private readonly List<Action<PetAppearance>> _appearanceViews=[];
    private PetAppearance? _wardrobeLook;
    private int _wardrobeTab;
    private PetAppearance Look=>_host.Settings.Appearance??PetAppearance.Default;
    private static readonly (string Name,string Hex)[] Coats=[("Oat",PetAppearance.Oat),("Milk","#F4F2E9"),("Ginger","#E1AF73"),("Cocoa","#9F7B68"),("Slate","#555C69"),("Cloud","#C8D2DF"),("Lilac","#BDA9D2"),("Peach","#E9BDB5")];
    private static readonly (string Name,string Hex)[] OutfitColors=[("Periwinkle","#AAB6E9"),("Rose","#D794A5"),("Sage",PetAppearance.Sage),("Sky","#8FBBD1"),("Plum","#A795BE"),("Honey","#DBB76D")];
    private void ChangeLook(Func<PetAppearance,PetAppearance> change)
    {_host.Update(_host.Settings with{Appearance=change(Look).Normalize()});_preview.Update(_host.Cat.Pose);}
    private void RefreshWardrobe()
    {
        if(_current!="Your cat"||_wardrobeLook==Look)return;
        _wardrobeLook=Look;foreach(var update in _appearanceViews)update(Look);
    }
    private void CatPage()
    {
        var(left,right)=Columns(.84,1.26);
        left.Children.Add(Eyebrow("A little more you"));left.Children.Add(Title("Meet your\none of a kind."));
        PreviewCard(left,"Your cat, live","Every little detail is saved automatically.");_preview.Height=214;
        left.Children.Add(Row(Button("Turn",()=>_host.Perform(CatAction.Turn)),Button("Groom",()=>_host.Perform(CatAction.Groom)),Button("Meow",()=>_host.Perform(CatAction.Meow))));
        left.Children.Add(Text("Call your cat",12,"Muted",6));var nickname=new TextBox{Text=_host.Settings.Nickname,MaxLength=24,Margin=new Thickness(0,0,0,12)};
        System.Windows.Automation.AutomationProperties.SetName(nickname,"Cat nickname");nickname.LostFocus+=(_,_)=>_host.Update(_host.Settings with{Nickname=nickname.Text});left.Children.Add(nickname);
        left.Children.Add(Button("Reset outfit & coat",()=>ChangeLook(_=>PetAppearance.Default)));
        left.Children.Add(Text("One original cat. A wardrobe of possibilities. Choose a coat, then mix a hat, neckwear and a collar.",12,"Muted",8));
        right.Children.Add(Eyebrow("The wardrobe"));right.Children.Add(Text("Small details. All yours.",23,"Ink",18));
        var tabs=new TabControl{BorderThickness=new Thickness(0),SelectedIndex=_wardrobeTab};
        System.Windows.Automation.AutomationProperties.SetName(tabs,"Cat customization categories");right.Children.Add(tabs);
        StackPanel Tab(string name)
        {var content=new StackPanel{Margin=new Thickness(0,18,0,0)};tabs.Items.Add(new TabItem{Header=name,Content=content});return content;}
        var coat=Tab("Coat");coat.Children.Add(Text("A new shade of cute",17,"Ink",6));coat.Children.Add(Text("Changes the whole cat, from ears to tail. Facial details adapt to keep darker coats readable.",12,"Muted",16));
        coat.Children.Add(ColorChoices("Coat colour",Coats,a=>a.CoatColor,(a,c)=>a with{CoatColor=c},true));
        var hats=Tab("Hats");hats.Children.Add(Text("A little something on top",17,"Ink",12));
        var hatTiles=new WrapPanel();hats.Children.Add(hatTiles);
        foreach(var pair in new[]{(CatHat.None,"Bare ears"),(CatHat.Beanie,"Cozy beanie"),(CatHat.Beret,"Soft beret"),(CatHat.Sunhat,"Sun hat"),(CatHat.PartyHat,"Party hat"),(CatHat.Flower,"Daisy bloom")})
            hatTiles.Children.Add(OutfitTile("Hat",pair.Item2,a=>a.Hat==pair.Item1,a=>a with{Hat=pair.Item1}));
        hats.Children.Add(ColorChoices("Hat colour",OutfitColors,a=>a.HatColor,(a,c)=>a with{HatColor=c}));
        var neck=Tab("Neckwear");neck.Children.Add(Text("A finishing touch",17,"Ink",12));var neckTiles=new WrapPanel();neck.Children.Add(neckTiles);
        foreach(var pair in new[]{(CatNeckwear.None,"No neckwear"),(CatNeckwear.Bandana,"Bandana"),(CatNeckwear.BowTie,"Bow tie"),(CatNeckwear.Scarf,"Soft scarf")})
            neckTiles.Children.Add(OutfitTile("Neckwear",pair.Item2,a=>a.Neckwear==pair.Item1,a=>a with{Neckwear=pair.Item1}));
        neck.Children.Add(ColorChoices("Neckwear colour",OutfitColors,a=>a.NeckwearColor,(a,c)=>a with{NeckwearColor=c}));
        var collars=Tab("Collars");collars.Children.Add(Text("Close to the heart",17,"Ink",12));var collarTiles=new WrapPanel();collars.Children.Add(collarTiles);
        foreach(var pair in new[]{(CatCollar.None,"No collar"),(CatCollar.Classic,"Classic band"),(CatCollar.Bell,"Tiny bell"),(CatCollar.Heart,"Heart charm")})
            collarTiles.Children.Add(OutfitTile("Collar",pair.Item2,a=>a.Collar==pair.Item1,a=>a with{Collar=pair.Item1}));
        collars.Children.Add(ColorChoices("Collar colour",OutfitColors,a=>a.CollarColor,(a,c)=>a with{CollarColor=c}));
        collars.Children.Add(Text("Neckwear sits over the collar, just as it would on a little outfit.",12,"Muted",10));
        var habits=Tab("Personality");habits.Children.Add(Text("At your pace",17,"Ink",12));
        habits.Children.Add(Choice("Cat activity",new[]{(ActivityLevel.Calm,"Calm · gentle company"),(ActivityLevel.Balanced,"Curious · a little variety"),(ActivityLevel.Playful,"Playful · little adventures")},_host.CurrentProfile.Activity,v=>_host.EditProfile(_displayProfile,p=>p with{Activity=v})));
        habits.Children.Add(Text("Activity applies to your "+_host.CurrentProfile.Name+" profile. Appearance stays with your cat across profiles.",12,"Muted",12));
        habits.Children.Add(Check("Quiet company · stay nearby",_host.Settings.Quiet,v=>_host.Update(_host.Settings with{Quiet=v})));
        habits.Children.Add(Check("Nap when I'm away",_host.Settings.IdleNaps,v=>_host.Update(_host.Settings with{IdleNaps=v})));
        habits.Children.Add(Choice("Idle time before napping",new[]{(1,"After 1 minute idle"),(3,"After 3 minutes idle"),(5,"After 5 minutes idle"),(10,"After 10 minutes idle"),(15,"After 15 minutes idle")},_host.Settings.IdleMinutes,v=>_host.Update(_host.Settings with{IdleMinutes=v})));
        habits.Children.Add(Row(Button("Walk",()=>_host.Perform(CatAction.Walk)),Button("Run",()=>_host.Perform(CatAction.Run)),Button("Play",()=>_host.Perform(CatAction.Play))));
        habits.Children.Add(Row(Button("Sleep",()=>_host.Perform(CatAction.Sleep)),Button("Wake",()=>_host.Perform(CatAction.Wake))));
        habits.Children.Add(Row(Button("Show cat",()=>_host.ShowCat(true)),Button("Hide cat",()=>_host.ShowCat(false)),Button("Park",_host.Park)));
        tabs.SelectedIndex=Math.Clamp(_wardrobeTab,0,tabs.Items.Count-1);
        tabs.SelectionChanged+=(_,e)=>{if(ReferenceEquals(e.Source,tabs))_wardrobeTab=tabs.SelectedIndex;};
    }
    private RadioButton OutfitTile(string group,string label,Func<PetAppearance,bool> selected,Func<PetAppearance,PetAppearance> apply)
    {
        var content=new StackPanel();var preview=new Image{Width=98,Height=76,Stretch=Stretch.Uniform,IsHitTestVisible=false};content.Children.Add(preview);
        var title=Text(label,12);title.TextAlignment=TextAlignment.Center;content.Children.Add(title);
        var radio=new RadioButton{GroupName="Wardrobe"+group,Content=content,Width=112,Margin=new Thickness(0,0,8,10),Padding=new Thickness(5,5,5,9)};
        radio.SetResourceReference(StyleProperty,"WardrobeTile");System.Windows.Automation.AutomationProperties.SetName(radio,group+" · "+label);
        radio.Click+=(_,_)=>ChangeLook(apply);
        _appearanceViews.Add(look=>{radio.IsChecked=selected(look);preview.Source=WardrobeThumbnail(apply(look));});return radio;
    }
    private StackPanel ColorChoices(string name,(string Name,string Hex)[] colors,Func<PetAppearance,string> read,Func<PetAppearance,string,PetAppearance> apply,bool large=false)
    {
        var panel=new StackPanel{Margin=new Thickness(0,8,0,0)};panel.Children.Add(Text(name,12,"Muted",8));var row=new WrapPanel();panel.Children.Add(row);
        foreach(var swatch in colors)
        {
            var content=new StackPanel();var dot=new System.Windows.Shapes.Ellipse{Width=large?31:23,Height=large?31:23,Fill=(Brush)new BrushConverter().ConvertFromString(swatch.Hex)!,StrokeThickness=1,Margin=new Thickness(0,1,0,6)};dot.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty,"Line");content.Children.Add(dot);
            var label=Text(swatch.Name,10);label.TextAlignment=TextAlignment.Center;content.Children.Add(label);
            var radio=new RadioButton{Content=content,GroupName=name,Width=large?83:64,Margin=new Thickness(0,0,5,8),Padding=new Thickness(3,7,3,4)};radio.SetResourceReference(StyleProperty,"WardrobeTile");
            System.Windows.Automation.AutomationProperties.SetName(radio,name+" · "+swatch.Name);radio.Click+=(_,_)=>ChangeLook(a=>apply(a,swatch.Hex));row.Children.Add(radio);
            _appearanceViews.Add(a=>radio.IsChecked=read(a)==swatch.Hex);
        }
        var hex=new TextBox{Width=103,MaxLength=7,Text=read(Look),VerticalContentAlignment=VerticalAlignment.Center};System.Windows.Automation.AutomationProperties.SetName(hex,"Custom "+name+", hexadecimal");
        var note=Text("Use any hex colour, for example #AAB6E9.",11,"Muted",8);
        panel.Children.Add(Row(hex,Button("Use colour",()=>{string value=hex.Text.Trim();if(!PetAppearance.IsColor(value)){note.Text="Use # followed by six hexadecimal digits.";return;}ChangeLook(a=>apply(a,value.ToUpperInvariant()));note.Text="Colour saved.";})));
        panel.Children.Add(note);_appearanceViews.Add(a=>{if(!hex.IsKeyboardFocused)hex.Text=read(a);});return panel;
    }
    private static ImageSource WardrobeThumbnail(PetAppearance look)
    {
        using var painter=new CatPainter();using var bitmap=new System.Drawing.Bitmap(320,256,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);using var graphics=System.Drawing.Graphics.FromImage(bitmap);
        graphics.Clear(System.Drawing.Color.Transparent);painter.Draw(graphics,CatRig.Evaluate(CatAction.Idle,0,0,1) with{BodyYaw=.2,HeadYaw=.1,TailYaw=.5,Appearance=look},1);graphics.Flush();
        var bits=bitmap.LockBits(new System.Drawing.Rectangle(0,0,320,256),System.Drawing.Imaging.ImageLockMode.ReadOnly,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        try{var image=BitmapSource.Create(320,256,96,96,PixelFormats.Pbgra32,null,bits.Scan0,bits.Stride*256,bits.Stride);image.Freeze();return image;}
        finally{bitmap.UnlockBits(bits);}
    }
}
