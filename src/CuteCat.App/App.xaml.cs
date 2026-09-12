using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace CuteCat.App;

public partial class App : Application
{
    private CompanionHost? _host;
    private Mutex? _instance;
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
        if(!first){MessageBox.Show("Cute Cat is already running. Open it from the cat icon in the notification area.","Cute Cat");Shutdown();return;}
        try
        {
            _host=new CompanionHost(data,qa is not null||benchmark is not null);
            if(e.Args.Contains("--enable-notification-helper"))_host.Update(_host.Settings with{Notifications=true});
            if(Arg("--observe-notifications") is string observation)_=QualityChecks.ObserveShell(_host,Path.GetFullPath(observation));
            var main=new MainWindow(_host);MainWindow=main;
            _host.OpenRequested+=()=>{main.Show();main.WindowState=WindowState.Normal;main.Activate();};
            if(!e.Args.Contains("--tray") && benchmark is null && shellCheck is null)main.Show();
            if(qa is not null)
            {
                if(e.Args.Contains("--policy-checks"))await QualityChecks.Policies(_host,Path.GetFullPath(qa),Arg("--app-fixture")!);
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
    protected override void OnExit(ExitEventArgs e) { _host?.Dispose();_instance?.Dispose();base.OnExit(e); }
}
