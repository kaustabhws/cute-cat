using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace CuteCat.ShellProbe;

internal static class NormalUserLauncher
{
    public static string State {get;private set;}="NotStarted";
    public static bool Relaunch()
    {
        using var identity=WindowsIdentity.GetCurrent();
        if(!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)){State="AlreadyStandard";return false;}
        if(Environment.CommandLine.Contains("--medium-probe",StringComparison.Ordinal)){State="ChildStillAdmin";return false;}
        IntPtr token=IntPtr.Zero,buffer=IntPtr.Zero,linked=IntPtr.Zero;
        try
        {
            if(!OpenProcessToken(GetCurrentProcess(),0xF01FF,out token)){State="OpenToken:"+Marshal.GetLastWin32Error();return false;}
            buffer=Marshal.AllocHGlobal(IntPtr.Size);
            if(GetTokenInformation(token,18,buffer,4,out _))State="ElevationType:"+Marshal.ReadInt32(buffer);
            linked=ExplorerToken(identity.User);
            if(linked==IntPtr.Zero && GetTokenInformation(token,19,buffer,IntPtr.Size,out _))linked=Marshal.ReadIntPtr(buffer);
            if(linked==IntPtr.Zero)linked=Restrict(token);
            if(linked==IntPtr.Zero)return false;
            var startup=new StartupInfo{Size=Marshal.SizeOf<StartupInfo>(),Desktop="winsta0\\default",Flags=1,Show=0};
            var command=new StringBuilder("\""+Environment.ProcessPath+"\" --medium-probe");
            if(!CreateProcessAsUser(linked,Environment.ProcessPath!,command,IntPtr.Zero,IntPtr.Zero,false,0,IntPtr.Zero,AppContext.BaseDirectory,ref startup,out var process))
            {
                command=new StringBuilder("\""+Environment.ProcessPath+"\" --medium-probe");
                if(!CreateProcessWithTokenW(linked,0,Environment.ProcessPath!,command,0,IntPtr.Zero,AppContext.BaseDirectory,ref startup,out process)){State+=";Create:"+Marshal.GetLastWin32Error();return false;}
            }
            CloseHandle(process.Process);CloseHandle(process.Thread);return true;
        }
        finally{if(linked!=IntPtr.Zero)CloseHandle(linked);if(buffer!=IntPtr.Zero)Marshal.FreeHGlobal(buffer);if(token!=IntPtr.Zero)CloseHandle(token);}
    }
    private static IntPtr ExplorerToken(SecurityIdentifier? user)
    {
        foreach(var explorer in System.Diagnostics.Process.GetProcessesByName("explorer"))
        {
            using(explorer)
            {
                if(explorer.SessionId!=System.Diagnostics.Process.GetCurrentProcess().SessionId)continue;
                IntPtr process=OpenProcess(0x1000,false,explorer.Id),source=IntPtr.Zero;
                if(process==IntPtr.Zero)continue;
                try
                {
                    if(!OpenProcessToken(process,0xA,out source))continue;
                    using var owner=new WindowsIdentity(source);
                    if(owner.User!=user||!DuplicateTokenEx(source,0xF01FF,IntPtr.Zero,2,1,out IntPtr primary))continue;
                    if(new WindowsPrincipal(owner).IsInRole(WindowsBuiltInRole.Administrator))
                    {IntPtr reduced=Restrict(primary);CloseHandle(primary);return reduced;}
                    return primary;
                }
                finally{if(source!=IntPtr.Zero)CloseHandle(source);CloseHandle(process);}
            }
        }
        return IntPtr.Zero;
    }
    private static IntPtr Restrict(IntPtr source)
    {
        IntPtr admin=IntPtr.Zero,medium=IntPtr.Zero,restricted=IntPtr.Zero,info=IntPtr.Zero;
        try
        {
            ConvertStringSidToSid("S-1-5-32-544",out admin);
            if(!CreateRestrictedToken(source,1,1,new[]{new SidAttributes{Sid=admin}},0,IntPtr.Zero,0,IntPtr.Zero,out restricted))
            {State+=";Restrict:"+Marshal.GetLastWin32Error();return IntPtr.Zero;}
            ConvertStringSidToSid("S-1-16-8192",out medium);
            info=Marshal.AllocHGlobal(Marshal.SizeOf<SidAttributes>());
            Marshal.StructureToPtr(new SidAttributes{Sid=medium,Attributes=0x20},info,false);
            if(!SetTokenInformation(restricted,25,info,Marshal.SizeOf<SidAttributes>()+(int)GetLengthSid(medium)))
            {State+=";Integrity:"+Marshal.GetLastWin32Error();CloseHandle(restricted);return IntPtr.Zero;}
            if(!SetUserObjectSecurity(restricted)){State+=";Dacl:"+Marshal.GetLastWin32Error();CloseHandle(restricted);return IntPtr.Zero;}
            State+=";RestrictedMedium";return restricted;
        }
        finally{if(admin!=IntPtr.Zero)LocalFree(admin);if(medium!=IntPtr.Zero)LocalFree(medium);if(info!=IntPtr.Zero)Marshal.FreeHGlobal(info);}
    }
    private static bool SetUserObjectSecurity(IntPtr token)
    {
        using var identity=WindowsIdentity.GetCurrent();string sid=identity.User!.Value;
        IntPtr descriptor=IntPtr.Zero,userSid=IntPtr.Zero,buffer=IntPtr.Zero;
        try
        {
            if(!ConvertStringSecurityDescriptorToSecurityDescriptor("D:(A;;GA;;;"+sid+")(A;;GA;;;SY)(A;;GA;;;BA)",1,out descriptor,out _))return false;
            if(!GetSecurityDescriptorDacl(descriptor,out bool present,out IntPtr dacl,out _)||!present)return false;
            buffer=Marshal.AllocHGlobal(IntPtr.Size);Marshal.WriteIntPtr(buffer,dacl);
            if(!SetTokenInformation(token,6,buffer,IntPtr.Size)||!ConvertStringSidToSid(sid,out userSid))return false;
            Marshal.WriteIntPtr(buffer,userSid);return SetTokenInformation(token,4,buffer,IntPtr.Size);
        }
        finally{if(buffer!=IntPtr.Zero)Marshal.FreeHGlobal(buffer);if(userSid!=IntPtr.Zero)LocalFree(userSid);if(descriptor!=IntPtr.Zero)LocalFree(descriptor);}
    }
    [StructLayout(LayoutKind.Sequential)]private struct SidAttributes{public IntPtr Sid;public uint Attributes;}
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct StartupInfo
    {public int Size;public string? Reserved,Desktop,Title;public int X,Y,XSize,YSize,XChars,YChars,Fill,Flags;public short Show,ReservedBytes;public IntPtr ReservedPointer,Input,Output,Error;}
    [StructLayout(LayoutKind.Sequential)]private struct ProcessInfo{public IntPtr Process,Thread;public uint ProcessId,ThreadId;}
    [DllImport("kernel32.dll")]private static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll")]private static extern IntPtr OpenProcess(uint access,bool inherit,int pid);
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool DuplicateTokenEx(IntPtr token,uint access,IntPtr attributes,int level,int type,out IntPtr duplicate);
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool OpenProcessToken(IntPtr process,uint access,out IntPtr token);
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool GetTokenInformation(IntPtr token,int type,IntPtr info,int length,out int needed);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool CreateProcessWithTokenW(IntPtr token,uint logon,string app,StringBuilder command,uint flags,IntPtr environment,string directory,ref StartupInfo startup,out ProcessInfo process);
    [DllImport("kernel32.dll")]private static extern bool CloseHandle(IntPtr handle);
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool CreateRestrictedToken(IntPtr token,uint flags,uint disableCount,SidAttributes[] disable,uint deleteCount,IntPtr delete,uint restrictCount,IntPtr restrict,out IntPtr result);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool ConvertStringSidToSid(string sid,out IntPtr result);
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool SetTokenInformation(IntPtr token,int type,IntPtr data,int length);
    [DllImport("advapi32.dll")]private static extern uint GetLengthSid(IntPtr sid);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode)]private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string text,uint revision,out IntPtr descriptor,out uint size);
    [DllImport("advapi32.dll")]private static extern bool GetSecurityDescriptorDacl(IntPtr descriptor,out bool present,out IntPtr dacl,out bool defaulted);
    [DllImport("kernel32.dll")]private static extern IntPtr LocalFree(IntPtr memory);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool CreateProcessAsUser(IntPtr token,string app,StringBuilder command,IntPtr processAttributes,IntPtr threadAttributes,bool inherit,uint flags,IntPtr environment,string directory,ref StartupInfo startup,out ProcessInfo process);
}
