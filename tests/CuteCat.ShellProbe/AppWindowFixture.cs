using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CuteCat.ShellProbe;

internal static class AppWindowFixture
{
    public static void Run(string directory,bool veto)
    {
        Directory.CreateDirectory(directory);
        var app=new Application{ShutdownMode=ShutdownMode.OnMainWindowClose};
        var window=new Window{Title="Cute Cat app-rule fixture",Width=470,Height=290,Left=850,Top=160,
            Content=new TextBlock{Text="A test-owned window. No user documents are opened.",Margin=new Thickness(25),TextWrapping=TextWrapping.Wrap}};
        int requests=0;bool stopping=false;Window? prompt=null;
        window.Closing+=(_,e)=>
        {
            if(stopping)return;
            requests++;File.WriteAllText(Path.Combine(directory,"close-requests.txt"),requests.ToString());
            if(veto)
            {
                e.Cancel=true;
                prompt=new Window{Owner=window,Title="Fixture save confirmation",Width=320,Height=150,
                    WindowStartupLocation=WindowStartupLocation.CenterOwner,Content=new TextBlock{Text="Save prompt deliberately remains open.",Margin=new Thickness(20)}};
                prompt.Show();prompt.Activate();
            }
        };
        var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(100)};
        timer.Tick+=(_,_)=>{if(File.Exists(Path.Combine(directory,"quit"))){stopping=true;prompt?.Close();window.Close();}};timer.Start();
        window.Loaded+=(_,_)=>{File.WriteAllText(Path.Combine(directory,"ready"),Environment.ProcessId.ToString());window.Activate();};
        app.Run(window);
    }
}
