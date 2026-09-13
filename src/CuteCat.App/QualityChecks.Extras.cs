using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using CuteCat.Core;

namespace CuteCat.App;

public static partial class QualityChecks
{
    public static async Task Extras(CompanionHost host,MainWindow window,string dir,string fixture)
    {
        Directory.CreateDirectory(dir);Directory.CreateDirectory(Path.Combine(dir,"ui"));
        host.Update(new Preferences{Quiet=true,IdleNaps=false});host.ShowCat(true);window.Show();
        try
        {
            var look=new PetAppearance{Pattern=CoatPattern.Calico,Hat=CatHat.Beret,Neckwear=CatNeckwear.Bandana,Collar=CatCollar.Heart};
            host.ChangeAppearance(look);host.SaveOutfit("Little artist");string id=host.Settings.Outfits.Single().Id;
            host.EditProfile("study",p=>p with{OutfitId=id});host.ChangeAppearance(look with{Pattern=CoatPattern.Tabby});host.SelectProfile("study");
            Check("profile-linked outfit reaches the native painted cat",host.CurrentAppearance==look&&host.Surface.PaintedPose.Appearance==look);
            host.ChangeAppearance(look with{CoatColor="#555C69"});
            Check("editing a linked look detaches without replacing the saved outfit",host.CurrentProfile.OutfitId is null&&host.Settings.Outfits.Single().Appearance==look);
            host.WearOutfit(id);Check("wearing a saved outfit restores the complete appearance",host.CurrentAppearance==look);
            host.Down(host.Cat.Position);host.Up(host.Cat.Position);await Task.Delay(700);
            Check("cat pet gesture uses the expressive procedural reaction",host.Cat.Action==CatAction.Pet&&host.Surface.PaintedPose.Affection>.8);
            host.Session.Start(25,FrameClock.Now);await Task.Delay(650);double before=host.Session.Elapsed;host.TakeBreak();
            Check("host break preserves current focus and shows a stretch",host.Session.CanReturnToFocus&&host.Session.InterruptedFocus!.Elapsed>=before&&host.Cat.Action==CatAction.Stretch);
            host.BackToFocus();Check("host return resumes rather than resets focus",!host.Session.IsBreak&&host.Session.Status==SessionStatus.Running&&host.Session.Elapsed>=before);host.Session.End();host.Configure();
            host.Perform(CatAction.Idle);host.PresentBreakCue(BreakCueKind.Water,FrameClock.Now);await Task.Delay(300);
            var hint=host.Hint!;IntPtr hintHandle=new WindowInteropHelper(hint).Handle;
            Check("break cue is a visible native window",hint.IsVisible&&Native.IsWindowVisible(hintHandle));
            Check("cue returns MA_NOACTIVATE without stealing foreground",CancelMenuMode(hintHandle,0x21,IntPtr.Zero,IntPtr.Zero).ToInt64()==3);
            RenderWindow(hint,Path.Combine(dir,"ui","water-cue.png"));
            host.ShowCat(false);Check("hiding cat dismisses the optional break cue",!hint.IsVisible);host.ShowCat(true);
            var safe=SupportDiagnostics.Report(host);using var support=JsonDocument.Parse(safe);
            Check("support report exposes only its allowlisted status fields",!safe.Contains(host.Store.DirectoryPath)&&!safe.Contains("Little artist")&&!safe.Contains("Outfits")&&!safe.Contains("AppRules")&&support.RootElement.GetProperty("reportSchema").GetInt32()==1);
            var imported=SettingsTransfer.Review(SettingsTransfer.Export(host.Settings),host.Settings);
            host.StartPractice();host.ImportSettings(imported);
            Check("import cancels pending paw and leaves monitoring off",host.Attempt.Target is null&&!host.Settings.AppGuard&&!host.Settings.Notifications&&!host.Settings.AutomaticProfiles);host.Practice?.Close();
            await CheckCountdown(host,dir,fixture);
            await CheckShortcut(host,window);
            host.ChangeAppearance(look);host.SaveOutfit("Little artist");host.ChangeAppearance(look with{Pattern=CoatPattern.Tabby,Hat=CatHat.Beanie,HatColor="#AAB6E9"});host.SaveOutfit("Cozy company");
            foreach(string theme in new[]{"Light","Dark"})
            {
                window.SetTheme(theme);window.Width=980;window.Height=800;
                foreach(string page in new[]{"Focus","Profiles","Your cat","Troubleshooting","Backup"})
                {window.Navigate(page);if(page=="Your cat")Descendants<TabControl>(window).Single().SelectedIndex=0;window.UpdateLayout();await Task.Delay(100);RenderWindow(window,Path.Combine(dir,"ui",theme+"-"+page+".png"));}
                window.Navigate("Your cat");var tabs=Descendants<TabControl>(window).Single();tabs.SelectedIndex=5;window.UpdateLayout();RenderWindow(window,Path.Combine(dir,"ui",theme+"-Outfits.png"));
            }
            window.Width=780;window.Height=620;window.Navigate("Your cat");window.UpdateLayout();
            await Task.Delay(250);
            Check("six wardrobe tabs fit at minimum window size",Descendants<TabControl>(window).Single().Items.Count==6);
            RenderWindow(window,Path.Combine(dir,"ui","minimum-wardrobe.png"));ExportExtrasArt(dir);
        }
        finally
        {
            host.Menu.Close();host.Update(new Preferences{Quiet=true});host.TestForegroundWindow=null;
            File.WriteAllText(Path.Combine(dir,"extras-checks.json"),JsonSerializer.Serialize(new{version=BuildInfo.Version,uiAccess=RuntimeAccess.HasUiAccess,foregroundInjectedForAppFixture=true,checks=Checks},new JsonSerializerOptions{WriteIndented=true}));
        }
    }
    private static async Task CheckCountdown(CompanionHost host,string dir,string fixture)
    {
        string folder=Path.Combine(dir,"countdown-fixture");Directory.CreateDirectory(folder);
        using var process=Process.Start(new ProcessStartInfo(fixture){Arguments="--app-window \""+folder+"\"",UseShellExecute=true,WindowStyle=ProcessWindowStyle.Hidden})!;
        try
        {
            for(int i=0;i<60&&!File.Exists(Path.Combine(folder,"ready"));i++)await Task.Delay(100);
            IntPtr hwnd=IntPtr.Zero;Native.EnumWindows((h,_)=>{Native.GetWindowThreadProcessId(h,out uint pid);if(pid==process.Id&&Native.IsWindowVisible(h))hwnd=h;return true;},IntPtr.Zero);
            host.TestForegroundWindow=hwnd;
            host.Update(new Preferences{Quiet=true,IdleNaps=false,AppGuard=true,AppRules=[new(fixture,"Countdown fixture",CloseDelaySeconds:12)]});
            for(int i=0;i<40&&host.Countdown is null;i++)await Task.Delay(100);
            Check("grace uses a visible countdown before any close request",hwnd!=IntPtr.Zero&&host.Countdown is{Seconds:>0}&&host.Hint?.IsVisible==true&&host.Attempt.Target is null&&!process.HasExited);
            var old=host.Countdown!;if(host.Hint is{IsVisible:true} hint)RenderWindow(hint,Path.Combine(dir,"ui","countdown.png"));
            host.Hint!.Primary.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));await Task.Delay(300);
            Check("countdown Allow button grants exact app exception and cancels countdown",host.Settings.AppExceptions.Any(e=>e.Path==fixture)&&host.Countdown is null&&host.Attempt.Target is null&&!process.HasExited);
            host.AllowApp(fixture,0);for(int i=0;i<30&&host.Countdown is null;i++)await Task.Delay(50);
            Check("stale countdown from previous policy epoch cannot grant another exception",!host.AllowCountdown(old));
            host.TestForegroundWindow=IntPtr.Zero;await Task.Delay(350);Check("foreground loss dismisses countdown",host.Countdown is null&&!host.Hint!.IsVisible);
            host.TestForegroundWindow=hwnd;host.EditRules(r=>r.Select(x=>x with{CloseDelaySeconds=0}).ToList());
            for(int i=0;i<40&&host.Attempt.Target is null;i++)await Task.Delay(50);
            Check("zero grace retains immediate paw and no countdown",host.Attempt.Target?.AppWindow==true&&host.Countdown is null);
            host.Update(host.Settings with{AppGuard=false});Check("guard disable cancels immediate intervention",host.Attempt.Target is null&&!process.HasExited);
        }
        finally{host.TestForegroundWindow=null;host.Update(new Preferences{Quiet=true,IdleNaps=false});File.WriteAllText(Path.Combine(folder,"quit"),"");await Task.WhenAny(process.WaitForExitAsync(),Task.Delay(3000));}
    }
    private static async Task CheckShortcut(CompanionHost host,MainWindow window)
    {
        async Task<bool> Launch(bool tray=false)
        {
            var start=new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden};
            start.ArgumentList.Add("--standard-user");start.ArgumentList.Add("--data-dir");start.ArgumentList.Add(host.Store.DirectoryPath);if(tray)start.ArgumentList.Add("--tray");
            using var child=Process.Start(start)!;await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(8));await Task.Delay(200);return child.ExitCode==0;
        }
        window.Hide();bool opened=await Launch();
        Check("second process opens the hidden main window without an already-running dialog",opened&&window.IsVisible&&Native.IsWindowVisible(new WindowInteropHelper(window).Handle));
        window.WindowState=WindowState.Minimized;opened=await Launch();Check("desktop shortcut restores a minimized existing window",opened&&window.WindowState==WindowState.Normal);
        window.WindowState=WindowState.Maximized;opened=await Launch();Check("desktop shortcut preserves a maximized window",opened&&window.WindowState==WindowState.Maximized);
        window.WindowState=WindowState.Minimized;opened=await Launch();Check("shortcut restores the previous maximized state",opened&&window.WindowState==WindowState.Maximized);
        window.Hide();opened=await Launch(true);Check("duplicate startup launch stays quiet",opened&&!window.IsVisible);window.WindowState=WindowState.Normal;host.OpenPanel();
    }
    private static void ExportExtrasArt(string dir)
    {
        using var painter=new CatPainter();using var sheet=new Bitmap(960,720);using var g=Graphics.FromImage(sheet);using var font=new Font("Segoe UI",11);
        var patterns=Enum.GetValues<CoatPattern>();
        for(int row=0;row<3;row++)for(int col=0;col<4;col++)
        {
            var bg=row%2==0?Color.FromArgb(248,247,252):Color.FromArgb(23,25,35);using var brush=new SolidBrush(bg);g.FillRectangle(brush,col*240,row*240,240,240);
            var look=new PetAppearance{Pattern=patterns[col],PatternColor=col==2?"#555C69":"#BD895B",Hat=row==2?CatHat.Beret:CatHat.None,Neckwear=row==2?CatNeckwear.Bandana:CatNeckwear.None};
            var p=CatRig.Evaluate(row==1?CatAction.Sleep:CatAction.Idle,.8,0,1) with{BodyYaw=row==2?-.7:1,HeadYaw=row==2?-.7:1,TailYaw=row==2?-.7:1,Appearance=look};
            painter.Draw(g,p,.82f,col*240-15,row*240+25);g.DrawString(patterns[col].ToString(),font,row%2==0?Brushes.Black:Brushes.White,col*240+14,row*240+14);
        }
        sheet.Save(Path.Combine(dir,"pattern-sheet.png"),ImageFormat.Png);
        bool unclipped=true;double largestJump=0;
        foreach(var action in new[]{CatAction.Pet,CatAction.Stretch,CatAction.Drink})
        {
            string folder=Path.Combine(dir,"frames",action.ToString());Directory.CreateDirectory(folder);using var frame=new Bitmap(384,308);using var fg=Graphics.FromImage(frame);
            var previous=CatRig.Evaluate(action,0,0,0);
            for(int i=0;i<150;i++)
            {
                double age=i/50d;var pose=CatRig.Evaluate(action,age,0,age) with{Appearance=new(){Pattern=CoatPattern.Tabby,Hat=CatHat.Beret,Neckwear=CatNeckwear.Bandana}};
                largestJump=Math.Max(largestJump,Math.Abs(pose.HeadTilt-previous.HeadTilt));previous=pose;
                fg.Clear(Color.FromArgb(248,247,252));painter.Draw(fg,pose,1.2f);frame.Save(Path.Combine(folder,$"{i:000}.png"),ImageFormat.Png);
            }
        }
        foreach(var pattern in patterns)foreach(var action in new[]{CatAction.Run,CatAction.Groom,CatAction.Sleep,CatAction.Pet,CatAction.Drink,CatAction.Stretch,CatAction.Paw})foreach(double yaw in new[]{-1d,0,1})
        {
            using var image=new Bitmap(320,256);using var canvas=Graphics.FromImage(image);painter.Draw(canvas,CatRig.Evaluate(action,.7,.3,1) with{BodyYaw=yaw,HeadYaw=yaw,TailYaw=yaw,Appearance=new(){Pattern=pattern,Hat=CatHat.PartyHat,Neckwear=CatNeckwear.Bandana}},1);
            for(int x=0;x<320;x++)unclipped&=image.GetPixel(x,0).A==0&&image.GetPixel(x,255).A==0;
            for(int y=0;y<256;y++)unclipped&=image.GetPixel(0,y).A==0&&image.GetPixel(319,y).A==0;
        }
        Check("procedural patterns and new poses remain inside canvas",unclipped);Check("pet and break head motion is continuous at 50 Hz",largestJump<2,new{largestJump});
    }
}
