using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace CuteCat.App;

internal static class ThemeChrome
{
    // Documented Windows 11 DWM attributes; keep native resize, snap and caption buttons.
    internal static uint ColorRef(Color color)=>(uint)(color.R|color.G<<8|color.B<<16);
    internal static int Apply(Window window,string style,bool dark)
    {
        IntPtr handle=new WindowInteropHelper(window).Handle;if(handle==IntPtr.Zero)return -1;
        if(style=="Windows")
        {using var key=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");dark=key?.GetValue("AppsUseLightTheme") is int v&&v==0;}
        uint enabled=dark?1u:0;DwmSetWindowAttribute(handle,20,ref enabled,4);
        bool native=style=="Windows"||SystemParameters.HighContrast;
        uint caption=native?uint.MaxValue:ColorRef(((SolidColorBrush)Application.Current.Resources[style=="Accent"?"Accent":"Paper"]).Color);
        uint text=native?uint.MaxValue:ColorRef(((SolidColorBrush)Application.Current.Resources[style=="Accent"?"ButtonInk":"Ink"]).Color);
        uint border=uint.MaxValue;
        int result=DwmSetWindowAttribute(handle,35,ref caption,4);
        DwmSetWindowAttribute(handle,36,ref text,4);DwmSetWindowAttribute(handle,34,ref border,4);
        return result;
    }
    [DllImport("dwmapi.dll")]private static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref uint value,int size);
}
