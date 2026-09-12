using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Automation;
using Condition=System.Windows.Automation.Condition;

internal static class Program
{
    private static int _result;
    private static void Main(string[] args)
    {
        var worker=new Thread(()=>{try{Run(args);}catch(Exception e){Console.Error.WriteLine(e);_result=1;}});
        worker.SetApartmentState(ApartmentState.MTA);worker.Start();worker.Join();Environment.ExitCode=_result;
    }
    private static void Run(string[] args)
    {
        string executable=Path.GetFullPath(args[0]),output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        SetThreadDpiAwarenessContext(new IntPtr(-4));
        var start=new ProcessStartInfo(executable){UseShellExecute=true,Arguments="/LANG=en /LOG=\""+Path.Combine(output,"wizard.log")+"\""};
        IntPtr window=FindWizard();
        using var process=window==IntPtr.Zero?Process.Start(start):null;
        for(int i=0;i<150&&window==IntPtr.Zero;i++){window=FindWizard();Thread.Sleep(100);}
        if(window==IntPtr.Zero)throw new Exception("Wizard window missing");
        var root=AutomationElement.FromHandle(window);var observations=new List<object>();
        string[] stages=args.Contains("--access-only")?new[]{"welcome","preview","access"}:new[]{"welcome","preview","access","shortcuts","ready"};
        foreach(string stage in stages)
        {
            Thread.Sleep(250);Capture(window,Path.Combine(output,stage+".png"));
            var buttons=root.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Button));
            observations.Add(new{stage,buttons=buttons.Cast<AutomationElement>().Where(x=>!x.Current.IsOffscreen).Select(x=>new{name=x.Current.Name,enabled=x.Current.IsEnabled}).ToArray()});
            Console.WriteLine("Captured "+stage);
            if(stage!=stages[^1])Click(root,"Next");
        }
        File.WriteAllText(Path.Combine(output,"wizard-controls.json"),JsonSerializer.Serialize(observations,new JsonSerializerOptions{WriteIndented=true}));
        Click(root,"Cancel");Thread.Sleep(250);
        GetWindowThreadProcessId(window,out uint pid);
        var dialog=AutomationElement.RootElement.FindFirst(TreeScope.Children,new AndCondition(
            new PropertyCondition(AutomationElement.ProcessIdProperty,(int)pid),new PropertyCondition(AutomationElement.ClassNameProperty,"#32770")));
        if(dialog is not null)Click(dialog,"Yes");
        else
        {
            // Styled Inno dialogs can be separate TMessageForm windows.
            foreach(AutomationElement owned in AutomationElement.RootElement.FindAll(TreeScope.Children,new PropertyCondition(AutomationElement.ProcessIdProperty,(int)pid)))
                if(TryClick(owned,"Yes"))break;
        }
        Console.WriteLine("Wizard cancelled before installation.");
    }
    private static void Click(AutomationElement root,string name)
    {if(!TryClick(root,name))throw new Exception("Enabled button not found: "+name);}
    private static bool TryClick(AutomationElement root,string name)
    {
        foreach(AutomationElement button in root.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Button)))
        {
            var b=button.Current;string text=b.Name.Replace("&","").Trim().Trim('<','>').Trim();
            if(text!=name||!b.IsEnabled||b.IsOffscreen)continue;
            if(b.NativeWindowHandle!=0)
            {
                // Inno's owner-drawn Win32 button provider advertises Invoke but
                // rejects it. Dispatch only this verified setup control's action;
                // no mouse/keyboard input or unrelated window messages are sent.
                GetWindowThreadProcessId(new IntPtr(b.NativeWindowHandle),out uint pid);
                if(pid!=root.Current.ProcessId)throw new Exception("Unexpected control owner");
                if(!PostMessage(new IntPtr(b.NativeWindowHandle),0xF5,IntPtr.Zero,IntPtr.Zero))throw new Exception("Control action failed");
                return true;
            }
            if(button.TryGetCurrentPattern(InvokePattern.Pattern,out var invoke)){((InvokePattern)invoke).Invoke();return true;}
        }
        return false;
    }
    private static IntPtr FindWizard()
    {
        IntPtr result=IntPtr.Zero;
        EnumWindows((h,p)=>
        {
            var cls=new StringBuilder(128);GetClassName(h,cls,cls.Capacity);
            if(cls.ToString()!="TWizardForm"||!IsWindowVisible(h))return true;
            var name=new StringBuilder(128);GetWindowText(h,name,name.Capacity);
            if(name.ToString()=="Cute Cat Setup"){result=h;return false;}return true;
        },IntPtr.Zero);return result;
    }
    private static void Capture(IntPtr window,string path)
    {
        if(!GetWindowRect(window,out var r))throw new Exception("Wizard bounds unavailable");
        using var bitmap=new Bitmap(r.Right-r.Left,r.Bottom-r.Top,PixelFormat.Format32bppArgb);
        using(var graphics=Graphics.FromImage(bitmap))
        {
            IntPtr dc=graphics.GetHdc();try{if(!PrintWindow(window,dc,2))throw new Exception("Wizard rendering failed");}finally{graphics.ReleaseHdc(dc);}
        }
        bitmap.Save(path,ImageFormat.Png);
    }
    [StructLayout(LayoutKind.Sequential)]private struct Rect{public int Left,Top,Right,Bottom;}
    private delegate bool Callback(IntPtr h,IntPtr p);
    [DllImport("user32.dll")]private static extern bool EnumWindows(Callback callback,IntPtr parameter);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern int GetClassName(IntPtr h,StringBuilder value,int length);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]private static extern int GetWindowText(IntPtr h,StringBuilder value,int length);
    [DllImport("user32.dll")]private static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")]private static extern bool GetWindowRect(IntPtr h,out Rect r);
    [DllImport("user32.dll")]private static extern bool PrintWindow(IntPtr h,IntPtr dc,uint flags);
    [DllImport("user32.dll")]private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
    [DllImport("user32.dll")]private static extern bool PostMessage(IntPtr h,uint message,IntPtr wparam,IntPtr lparam);
}
