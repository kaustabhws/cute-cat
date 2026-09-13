using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace CuteCat.App;

public partial class App : Application
{
    private CompanionHost? _host;
    private Mutex? _instance;
    private SingleInstanceActivation? _activation;
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if(StandardUserProcess.Relaunch(e.Args,out int childExit)){Shutdown(childExit);return;}
        string? Arg(string flag) { int n=Array.IndexOf(e.Args,flag);return n>=0&&n+1<e.Args.Length?e.Args[n+1]:null; }
        if(Arg("--verify-installer") is string installer)
        {
            bool verified=InstallerSignature.SamePublisher(installer,Arg("--publisher-file")??Environment.ProcessPath!,Arg("--expected-version")??BuildInfo.Version);
            if(Arg("--verification-output") is string output)File.WriteAllText(output,System.Text.Json.JsonSerializer.Serialize(new{verified}));
            Shutdown(verified?0:1);return;
        }
        if(Arg("--art-preview") is string artPreview)
        {
            QualityChecks.ExportArt(Path.GetFullPath(artPreview));Shutdown();return;
        }
        string data=Arg("--data-dir")??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CuteCat");
        string? qa=Arg("--qa");
        string? benchmark=Arg("--benchmark");
        string? shellCheck=Arg("--shell-check");
        if(qa is not null)System.Windows.Media.RenderOptions.ProcessRenderMode=System.Windows.Interop.RenderMode.SoftwareOnly;
        if(qa is not null)data=Path.Combine(Path.GetFullPath(qa),"isolated-state");
        if(benchmark is not null)data=Path.Combine(Path.GetFullPath(benchmark),"isolated-state");
        if(shellCheck is not null)data=Path.Combine(Path.GetFullPath(shellCheck),"isolated-state");
        string hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(data).ToUpperInvariant())))[..20];
        _instance=new Mutex(true,"Local\\CuteCat.Vector."+hash,out bool first);
        if(!first)
        {
            try{first=_instance.WaitOne(0);}catch(AbandonedMutexException){first=true;}
            if(!first)
            {
                // A second automatic startup is quiet; a desktop/Start shortcut opens the existing panel.
                bool opened=e.Args.Contains("--tray")||await SingleInstanceActivation.NotifyAsync(hash);
                if(!opened)MessageBox.Show("Cute Cat could not bring its window forward. Please try opening it again in a moment.","Cute Cat");
                Shutdown(opened?0:1);return;
            }
        }
        try
        {
            MainWindow? panel=null;bool openPending=false;WindowState restoreState=WindowState.Normal;
            void OpenMain()
            {
                if(panel is null){openPending=true;return;}
                panel.Show();if(panel.WindowState==WindowState.Minimized)panel.WindowState=restoreState;
                var handle=new System.Windows.Interop.WindowInteropHelper(panel).Handle;
                Native.SetWindowPos(handle,IntPtr.Zero,0,0,0,0,0x1|0x2|0x4|0x40);panel.Activate();
            }
            _activation=new SingleInstanceActivation(hash,OpenMain);
            _host=new CompanionHost(data,qa is not null||benchmark is not null);
            if(qa is null&&benchmark is null&&shellCheck is null&&Arg("--data-dir") is null)_host.InitializeStartup();
            if(e.Args.Contains("--enable-notification-helper"))_host.Update(_host.Settings with{Notifications=true});
            if(Arg("--observe-notifications") is string observation)_=QualityChecks.ObserveShell(_host,Path.GetFullPath(observation));
            var main=new MainWindow(_host);panel=main;MainWindow=main;
            main.StateChanged+=(_,_)=>{if(main.WindowState!=WindowState.Minimized)restoreState=main.WindowState;};
            _host.OpenRequested+=OpenMain;
            if(openPending||!e.Args.Contains("--tray") && benchmark is null && shellCheck is null)OpenMain();
            if(qa is not null)
            {
                if(e.Args.Contains("--responsiveness-checks"))await QualityChecks.Responsiveness(_host,main,Path.GetFullPath(qa));
                else if(e.Args.Contains("--extras-checks"))await QualityChecks.Extras(_host,main,Path.GetFullPath(qa),Arg("--app-fixture")!);
                else if(e.Args.Contains("--menu-opening-checks"))await QualityChecks.MenuOpening(_host,main,Path.GetFullPath(qa));
                else if(e.Args.Contains("--menu-checks"))await QualityChecks.Menus(_host,main,Path.GetFullPath(qa),Arg("--app-fixture")!,e.Args.Contains("--menu-events-only"));
                else if(e.Args.Contains("--appearance-checks"))await QualityChecks.Appearance(host:_host,window:main,dir:Path.GetFullPath(qa));
                else if(e.Args.Contains("--policy-checks"))await QualityChecks.Policies(_host,Path.GetFullPath(qa),Arg("--app-fixture")!);
                else if(e.Args.Contains("--features-only"))await QualityChecks.Features(_host,main,Path.GetFullPath(qa),Arg("--app-fixture")!);
                else await QualityChecks.Run(_host,main,Path.GetFullPath(qa),Arg("--app-fixture"));
                Shutdown(QualityChecks.Failed?1:0);
            }
            if(benchmark is not null) { await QualityChecks.Benchmark(_host,Path.GetFullPath(benchmark),Arg("--benchmark-mode"));Shutdown(); }
            if(shellCheck is not null) { await QualityChecks.ShellCheck(_host,Path.GetFullPath(shellCheck));Shutdown(); }
        }
        catch(Exception ex)
        {
            if(qa is not null){Directory.CreateDirectory(qa);File.WriteAllText(Path.Combine(qa,"failure.txt"),ex.ToString());}
            else MessageBox.Show("Cute Cat could not start. Your local settings are preserved.\n\n"+ex.GetType().Name,"Cute Cat",MessageBoxButton.OK,MessageBoxImage.Information);
            Shutdown(1);
        }
    }
    protected override void OnExit(ExitEventArgs e) { _activation?.Dispose();_host?.Dispose();_instance?.Dispose();base.OnExit(e); }
}
