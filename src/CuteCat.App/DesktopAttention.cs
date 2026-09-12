using System.Runtime.InteropServices;
using CuteCat.Core;

namespace CuteCat.App;

/// <summary>Transient caret/focused-control geometry only. No text or input event capture.</summary>
internal static class DesktopAttention
{
    public static Area? ProtectedArea(double scale)
    {
        IntPtr window=Native.GetForegroundWindow();
        if(window==IntPtr.Zero)return null;
        uint thread=Native.GetWindowThreadProcessId(window,out uint pid);
        if(pid==Environment.ProcessId)return null;
        var gui=new GuiInfo{Size=(uint)Marshal.SizeOf<GuiInfo>()};
        if(!GetGUIThreadInfo(thread,ref gui))return null;
        if(gui.Caret!=IntPtr.Zero)
        {
            var start=new Native.Point(gui.CaretRect.Left,gui.CaretRect.Top);
            var end=new Native.Point(gui.CaretRect.Right,gui.CaretRect.Bottom);
            if(Native.ClientToScreen(gui.Caret,ref start)&&Native.ClientToScreen(gui.Caret,ref end))
                return new Area(start.X-140*scale,start.Y-65*scale,Math.Max(2,end.X-start.X)+280*scale,Math.Max(2,end.Y-start.Y)+130*scale);
        }
        if(gui.Focus!=IntPtr.Zero&&gui.Focus!=window&&Native.GetWindowRect(gui.Focus,out var r))
        {
            var area=new Area(r.Left-16*scale,r.Top-16*scale,r.Right-r.Left+32*scale,r.Bottom-r.Top+32*scale);
            if(area.IsValid&&area.Width<=640*scale&&area.Height<=220*scale)return area;
        }
        return null;
    }
    [StructLayout(LayoutKind.Sequential)]private struct GuiInfo
    {public uint Size,Flags;public IntPtr Active,Focus,Capture,MenuOwner,MoveSize,Caret;public Native.Rect CaretRect;}
    [DllImport("user32.dll")]private static extern bool GetGUIThreadInfo(uint thread,ref GuiInfo info);
}
