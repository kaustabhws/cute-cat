using System.Runtime.InteropServices;
using System.Text;

namespace CuteCat.App;

internal static class Native
{
    [StructLayout(LayoutKind.Sequential)] internal struct Point { public int X,Y; public Point(int x,int y){X=x;Y=y;} }
    [StructLayout(LayoutKind.Sequential)] internal struct Size { public int Width,Height; public Size(int w,int h){Width=w;Height=h;} }
    [StructLayout(LayoutKind.Sequential)] internal struct Rect { public int Left,Top,Right,Bottom; }
    [StructLayout(LayoutKind.Sequential,Pack=1)] internal struct Blend { public byte Op,Flags,Alpha,Format; }
    [StructLayout(LayoutKind.Sequential)] internal struct BitmapInfo { public uint Size; public int Width,Height; public ushort Planes,BitCount; public uint Compression,SizeImage; public int XPels,YPels; public uint Used,Important; }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] internal struct WindowClass
    { public uint Size,Style; public WndProc Proc; public int ClassExtra,WindowExtra; public IntPtr Instance,Icon,Cursor,Background; public string? MenuName,ClassName; public IntPtr SmallIcon; }
    internal delegate IntPtr WndProc(IntPtr hwnd,uint msg,IntPtr wp,IntPtr lp);
    internal delegate bool EnumProc(IntPtr hwnd,IntPtr param);
    [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] internal static extern ushort RegisterClassEx(ref WindowClass wc);
    [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] internal static extern IntPtr CreateWindowEx(uint ex,string cls,string title,uint style,int x,int y,int w,int h,IntPtr parent,IntPtr menu,IntPtr inst,IntPtr param);
    [DllImport("user32.dll")] internal static extern IntPtr DefWindowProc(IntPtr h,uint m,IntPtr w,IntPtr l);
    [DllImport("user32.dll")] internal static extern bool DestroyWindow(IntPtr h);
    [DllImport("user32.dll")] internal static extern bool ShowWindow(IntPtr h,int n);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool UpdateLayeredWindow(IntPtr hwnd,IntPtr dst,ref Point pos,ref Size size,IntPtr src,ref Point srcPos,uint key,ref Blend blend,uint flags);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr hwnd,IntPtr insert,int x,int y,int w,int h,uint flags);
    [DllImport("user32.dll")] internal static extern IntPtr SetCapture(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern bool ReleaseCapture();
    [DllImport("user32.dll")] internal static extern bool GetCursorPos(out Point p);
    [DllImport("user32.dll")] internal static extern bool ClientToScreen(IntPtr hwnd,ref Point p);
    [DllImport("user32.dll")] internal static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
    [DllImport("user32.dll")] internal static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern bool IsWindow(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern bool EnumWindows(EnumProc callback,IntPtr param);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]internal static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string? className,string? title);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern int GetClassName(IntPtr hwnd,StringBuilder text,int max);
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint process);
    [DllImport("user32.dll")] internal static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern bool SetForegroundWindow(IntPtr window);
    [DllImport("user32.dll")] internal static extern IntPtr GetCapture();
    [DllImport("user32.dll")] internal static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern IntPtr LoadCursor(IntPtr inst,IntPtr name);
    [DllImport("user32.dll")] internal static extern uint GetGuiResources(IntPtr process,uint flags);
    [DllImport("gdi32.dll")] internal static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern IntPtr CreateDIBSection(IntPtr dc,ref BitmapInfo info,uint usage,out IntPtr bits,IntPtr section,uint offset);
    [DllImport("gdi32.dll")] internal static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
    [DllImport("gdi32.dll")] internal static extern bool DeleteObject(IntPtr obj);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] internal static extern IntPtr GetModuleHandle(string? name);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] internal static extern IntPtr CreateWaitableTimerEx(IntPtr attr,string? name,uint flags,uint access);
    [DllImport("kernel32.dll")] internal static extern bool SetWaitableTimer(IntPtr timer,ref long due,int period,IntPtr completion,IntPtr arg,bool resume);
    [DllImport("kernel32.dll")] internal static extern uint WaitForSingleObject(IntPtr handle,uint ms);
    [DllImport("kernel32.dll")] internal static extern uint WaitForMultipleObjects(uint count,IntPtr[] handles,bool waitAll,uint milliseconds);
    [DllImport("kernel32.dll")] internal static extern bool CloseHandle(IntPtr handle);
    [DllImport("kernel32.dll",SetLastError=true)]internal static extern IntPtr OpenProcess(uint access,bool inherit,int id);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]internal static extern bool QueryFullProcessImageName(IntPtr process,uint flags,StringBuilder name,ref int length);
    [DllImport("shell32.dll")]internal static extern int SHQueryUserNotificationState(out int state);
}
