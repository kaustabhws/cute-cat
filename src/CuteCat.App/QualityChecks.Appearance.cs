using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CuteCat.Core;
using Color=System.Drawing.Color;

namespace CuteCat.App;

public static partial class QualityChecks
{
    public static async Task Appearance(CompanionHost host,MainWindow window,string dir)
    {
        Directory.CreateDirectory(dir);Directory.CreateDirectory(Path.Combine(dir,"ui"));
        host.Update(new Preferences{Quiet=true,IdleNaps=false});host.ShowCat(true);host.Perform(CatAction.Idle);
        window.Navigate("Your cat");window.UpdateLayout();await Task.Delay(250);
        var tabs=Descendants<TabControl>(window).Single();
        var slate=Descendants<RadioButton>(window).First(r=>System.Windows.Automation.AutomationProperties.GetName(r)=="Coat colour · Slate");
        slate.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Check("coat swatch updates saved appearance and native painted pose immediately",host.Settings.Appearance?.CoatColor=="#555C69"&&host.Surface.PaintedPose.Appearance?.CoatColor=="#555C69");
        tabs.SelectedIndex=1;window.UpdateLayout();
        Descendants<RadioButton>(window).First(r=>System.Windows.Automation.AutomationProperties.GetName(r)=="Hat · Cozy beanie").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        tabs.SelectedIndex=2;window.UpdateLayout();
        Descendants<RadioButton>(window).First(r=>System.Windows.Automation.AutomationProperties.GetName(r)=="Neckwear · Bandana").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        tabs.SelectedIndex=3;window.UpdateLayout();
        Descendants<RadioButton>(window).First(r=>System.Windows.Automation.AutomationProperties.GetName(r)=="Collar · Tiny bell").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Check("hat neckwear and collar can be combined",host.Settings.Appearance is{Hat:CatHat.Beanie,Neckwear:CatNeckwear.Bandana,Collar:CatCollar.Bell});
        var hex=Descendants<TextBox>(window).First(t=>System.Windows.Automation.AutomationProperties.GetName(t).StartsWith("Custom Collar"));hex.Text="#ff9020";
        Descendants<Button>(window).First(b=>b.Content?.ToString()=="Use colour").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Check("custom slot colour normalizes without recolouring coat or hat",host.Settings.Appearance is{CollarColor:"#FF9020",CoatColor:"#555C69",HatColor:PetAppearance.Sage});
        hex.Text="invalid";Descendants<Button>(window).First(b=>b.Content?.ToString()=="Use colour").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Check("invalid custom colour leaves outfit unchanged",host.Settings.Appearance?.CollarColor=="#FF9020");
        await Task.Delay(200);var restored=new StateStore(host.Store.DirectoryPath).Load();
        Check("outfit and colours survive state reload",restored.Schema==5&&restored.Settings.Appearance==host.Settings.Appearance);
        foreach(string theme in new[]{"Light","Dark"})
        {
            window.SetTheme(theme);window.UpdateLayout();
            uint expected=ThemeChrome.ColorRef(((SolidColorBrush)Application.Current.Resources["Paper"]).Color);
            int result=ThemeChrome.Apply(window,"Theme",theme=="Dark");await Task.Delay(100);
            var caption=CaptureOwnWindow(window,Path.Combine(dir,theme+"-native-window.png"));
            Check(theme+" native title bar matches app surface",result==0&&caption.HasValue&&((uint)(caption.Value.R|caption.Value.G<<8|caption.Value.B<<16))==expected,new{requested=expected,result,actual=caption?.ToArgb()});
            for(int i=0;i<tabs.Items.Count;i++)
            {
                tabs.SelectedIndex=i;window.UpdateLayout();await Task.Delay(100);
                var name=((TabItem)tabs.Items[i]).Header.ToString();RenderWindow(window,Path.Combine(dir,"ui",theme+"-wardrobe-"+name+".png"));
            }
            window.Navigate("Settings");window.UpdateLayout();RenderWindow(window,Path.Combine(dir,"ui",theme+"-settings.png"));
            // Review the actual mouse-focus appearance without injecting global input.
            var navigation=(Panel)window.FindName("Navigation");
            var focusButton=navigation.Children.OfType<Button>().First(b=>b.Content?.ToString()=="Focus");
            focusButton.Focus();focusButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));window.UpdateLayout();
            Descendants<TextBox>(window).Single().Text="180";window.UpdateLayout();
            RenderWindow(window,Path.Combine(dir,"ui",theme+"-focused-navigation.png"));
            window.Navigate("Profiles");window.UpdateLayout();
            var profileName=Descendants<TextBox>(window).First(t=>System.Windows.Automation.AutomationProperties.GetName(t)=="Profile name");
            profileName.Focus();profileName.SelectAll();
            RenderWindow(window,Path.Combine(dir,"ui",theme+"-rounded-inputs.png"));
            window.Navigate("Your cat");tabs=Descendants<TabControl>(window).Single();
        }
        host.Update(host.Settings with{TitleBarStyle="Accent"});window.SetTheme("Dark");
        await Task.Delay(100);var accentCaption=CaptureOwnWindow(window,Path.Combine(dir,"accent-native-window.png"));
        var accent=((SolidColorBrush)Application.Current.Resources["Accent"]).Color;
        Check("accent title bar is independently selectable",accentCaption.HasValue&&accentCaption.Value.R==accent.R&&accentCaption.Value.G==accent.G&&accentCaption.Value.B==accent.B);
        host.Update(host.Settings with{TitleBarStyle="Windows"});window.SetTheme("Dark");
        int reset=ThemeChrome.Apply(window,"Windows",true);await Task.Delay(100);var systemCaption=CaptureOwnWindow(window,Path.Combine(dir,"windows-native-window.png"));
        Check("Windows title bar resets colour override",reset==0&&systemCaption.HasValue&&systemCaption!=accentCaption);
        host.Update(host.Settings with{TitleBarStyle="Theme"});window.SetTheme("Dark");
        window.Width=780;window.Height=620;window.UpdateLayout();await Task.Delay(250);RenderWindow(window,Path.Combine(dir,"ui","minimum-wardrobe.png"));
        Check("all wardrobe categories remain available at minimum size",tabs.Items.Count==6&&tabs.ActualWidth>300);
        ExportWardrobe(dir);
        File.WriteAllText(Path.Combine(dir,"appearance-checks.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,checks=Checks},new JsonSerializerOptions{WriteIndented=true}));
    }
    private static IEnumerable<T> Descendants<T>(DependencyObject parent)where T:DependencyObject
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++)
        {var child=VisualTreeHelper.GetChild(parent,i);if(child is T found)yield return found;foreach(var item in Descendants<T>(child))yield return item;}
    }
    public static void ExportWardrobe(string dir)
    {
        Directory.CreateDirectory(dir);using var painter=new CatPainter();
        using(var image=new Bitmap(320,256))using(var g=Graphics.FromImage(image))
        {
            var look=new PetAppearance{CoatColor="#555C69"};painter.Draw(g,CatRig.Evaluate(CatAction.Idle,0,0,0) with{Appearance=look},1);
            var body=ColorTranslator.FromHtml(look.CoatColor);
            var points=new[]{new System.Drawing.Point(145,180),new(205,125),new(204,214),new(116,214),new(64,133)};
            Check("whole coat recolours body head paws and tail",points.All(p=>image.GetPixel(p.X,p.Y).ToArgb()==body.ToArgb()),points.Select(p=>image.GetPixel(p.X,p.Y).ToArgb()).ToArray());
        }
        using(var sheet=new Bitmap(1200,960))using(var g=Graphics.FromImage(sheet))
        {
            var hats=Enum.GetValues<CatHat>();var actions=new[]{CatAction.Idle,CatAction.Turn,CatAction.Groom,CatAction.Sleep};
            using var font=new Font("Segoe UI",12,System.Drawing.FontStyle.Regular,GraphicsUnit.Pixel);
            for(int row=0;row<4;row++)for(int col=0;col<6;col++)
            {
                using var bg=new SolidBrush(ColorTranslator.FromHtml(row%2==0?"#F8F7FC":"#171923"));g.FillRectangle(bg,col*200,row*240,200,240);
                var look=new PetAppearance{CoatColor=col%2==0?"#FCF0D5":"#555C69",Hat=hats[col],HatColor="#AAB6E9",Neckwear=row==0?CatNeckwear.Bandana:row==1?CatNeckwear.Scarf:row==2?CatNeckwear.BowTie:CatNeckwear.None,NeckwearColor="#D794A5",Collar=CatCollar.Bell};
                double yaw=row==1?0:row==2?-1:1;
                var pose=CatRig.Evaluate(actions[row],1.5,.2,2) with{BodyYaw=yaw,HeadYaw=yaw,TailYaw=yaw,Appearance=look};
                painter.Draw(g,pose,.65f,col*200-3,row*240+50);
                g.DrawString(hats[col]+" · "+actions[row],font,row%2==0?System.Drawing.Brushes.Black:System.Drawing.Brushes.White,col*200+10,row*240+16);
            }
            sheet.Save(Path.Combine(dir,"wardrobe-poses.png"),ImageFormat.Png);
        }
        bool unclipped=true;
        foreach(var hat in Enum.GetValues<CatHat>())foreach(var action in new[]{CatAction.Walk,CatAction.Run,CatAction.Groom,CatAction.Sleep,CatAction.Turn})foreach(double yaw in new[]{-1d,0,1})
        {
            using var bitmap=new Bitmap(320,256);using var g=Graphics.FromImage(bitmap);painter.Draw(g,CatRig.Evaluate(action,.7,.2,1) with{HeadYaw=yaw,BodyYaw=yaw,TailYaw=yaw,Appearance=new(){Hat=hat,Neckwear=CatNeckwear.Scarf,Collar=CatCollar.Heart}},1);
            for(int x=0;x<320;x++)unclipped&=bitmap.GetPixel(x,0).A==0&&bitmap.GetPixel(x,255).A==0;
            for(int y=0;y<256;y++)unclipped&=bitmap.GetPixel(0,y).A==0&&bitmap.GetPixel(319,y).A==0;
        }
        Check("wardrobe remains inside transparent canvas in motion and both directions",unclipped);
    }
    private static Color? CaptureOwnWindow(Window window,string path)
    {
        // Explicit QA-only render of this application's own HWND, including its caption.
        IntPtr hwnd=new System.Windows.Interop.WindowInteropHelper(window).Handle;
        if(!Native.GetWindowRect(hwnd,out var r))return null;
        using var bitmap=new Bitmap(r.Right-r.Left,r.Bottom-r.Top);using var g=Graphics.FromImage(bitmap);IntPtr dc=g.GetHdc();
        bool captured;try{captured=PrintWindow(hwnd,dc,2);}finally{g.ReleaseHdc(dc);}
        if(!captured)return null;
        bitmap.Save(path,ImageFormat.Png);return bitmap.GetPixel(bitmap.Width/2,12);
    }
    [DllImport("user32.dll")]private static extern bool PrintWindow(IntPtr window,IntPtr dc,uint flags);
}
