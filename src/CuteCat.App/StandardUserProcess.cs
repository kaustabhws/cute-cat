using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace CuteCat.App;

/// <summary>Keep this companion unelevated even when started by an elevated development shell.</summary>
internal static class StandardUserProcess
{
    public static bool Relaunch(string[] arguments,out int exitCode)
    {
        exitCode=0;
        using var identity=WindowsIdentity.GetCurrent();
        if(!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)||arguments.Contains("--standard-user"))return false;
        IntPtr token=IntPtr.Zero,buffer=IntPtr.Zero,standard=IntPtr.Zero;
        try
        {
            if(!OpenProcessToken(GetCurrentProcess(),0xF01FF,out token))return false;
            buffer=Marshal.AllocHGlobal(IntPtr.Size);
            standard=GetTokenInformation(token,19,buffer,IntPtr.Size,out _)?Marshal.ReadIntPtr(buffer):RestrictedMediumToken(token);
            if(standard==IntPtr.Zero)return false;
            string executable=Environment.ProcessPath!;
            string command=string.Join(" ",new[]{executable}.Concat(arguments).Append("--standard-user").Select(Quote));
            var startup=new StartupInfo{Size=Marshal.SizeOf<StartupInfo>(),Desktop="winsta0\\default",Flags=1,Show=(short)(arguments.Contains("--tray")?0:1)};
            if(!CreateProcessAsUser(standard,executable,new StringBuilder(command),IntPtr.Zero,IntPtr.Zero,false,0,IntPtr.Zero,AppContext.BaseDirectory,ref startup,out var process)&&
                !CreateProcessWithTokenW(standard,0,executable,new StringBuilder(command),0,IntPtr.Zero,AppContext.BaseDirectory,ref startup,out process))return false;
            try
            {
                if(arguments.Any(a=>a is "--qa" or "--shell-check" or "--benchmark" or "--art-preview"))
                {WaitForSingleObject(process.Process,0xffffffff);GetExitCodeProcess(process.Process,out uint code);exitCode=(int)code;}
            }
            finally{CloseHandle(process.Process);CloseHandle(process.Thread);}
            return true;
        }
        finally{if(standard!=IntPtr.Zero)CloseHandle(standard);if(buffer!=IntPtr.Zero)Marshal.FreeHGlobal(buffer);if(token!=IntPtr.Zero)CloseHandle(token);}
    }

    private static IntPtr RestrictedMediumToken(IntPtr source)
    {
        IntPtr admin=IntPtr.Zero,medium=IntPtr.Zero,restricted=IntPtr.Zero,info=IntPtr.Zero;
        try
        {
            if(!ConvertStringSidToSid("S-1-5-32-544",out admin)||!CreateRestrictedToken(source,1,1,new[]{new SidAttributes{Sid=admin}},0,IntPtr.Zero,0,IntPtr.Zero,out restricted))return IntPtr.Zero;
            if(!ConvertStringSidToSid("S-1-16-8192",out medium)){CloseHandle(restricted);return IntPtr.Zero;}
            info=Marshal.AllocHGlobal(Marshal.SizeOf<SidAttributes>());
            Marshal.StructureToPtr(new SidAttributes{Sid=medium,Attributes=0x20},info,false);
            if(!SetTokenInformation(restricted,25,info,Marshal.SizeOf<SidAttributes>()+(int)GetLengthSid(medium)))
            {CloseHandle(restricted);return IntPtr.Zero;}
            if(!SetUserObjectSecurity(restricted)){CloseHandle(restricted);return IntPtr.Zero;}
            return restricted;
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

    private static string Quote(string value)
    {
        var result=new StringBuilder("\"");int slashes=0;
        foreach(char c in value)
        {
            if(c=='\\'){slashes++;continue;}
            if(c=='\"'){result.Append('\\',slashes*2+1).Append(c);slashes=0;continue;}
            result.Append('\\',slashes).Append(c);slashes=0;
        }
        return result.Append('\\',slashes*2).Append('"').ToString();
    }
    [StructLayout(LayoutKind.Sequential)]private struct SidAttributes{public IntPtr Sid;public uint Attributes;}
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct StartupInfo
    {public int Size;public string? Reserved,Desktop,Title;public int X,Y,XSize,YSize,XChars,YChars,Fill,Flags;public short Show,ReservedBytes;public IntPtr ReservedPointer,Input,Output,Error;}
    [StructLayout(LayoutKind.Sequential)]private struct ProcessInfo{public IntPtr Process,Thread;public uint ProcessId,ThreadId;}
    [DllImport("kernel32.dll")]private static extern IntPtr GetCurrentProcess();
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool OpenProcessToken(IntPtr process,uint access,out IntPtr token);
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool GetTokenInformation(IntPtr token,int type,IntPtr info,int length,out int needed);
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool CreateRestrictedToken(IntPtr token,uint flags,uint disableCount,SidAttributes[] disable,uint deleteCount,IntPtr delete,uint restrictCount,IntPtr restrict,out IntPtr result);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool ConvertStringSidToSid(string sid,out IntPtr result);
    [DllImport("advapi32.dll",SetLastError=true)]private static extern bool SetTokenInformation(IntPtr token,int type,IntPtr data,int length);
    [DllImport("advapi32.dll")]private static extern uint GetLengthSid(IntPtr sid);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode)]private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string text,uint revision,out IntPtr descriptor,out uint size);
    [DllImport("advapi32.dll")]private static extern bool GetSecurityDescriptorDacl(IntPtr descriptor,out bool present,out IntPtr dacl,out bool defaulted);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool CreateProcessAsUser(IntPtr token,string app,StringBuilder command,IntPtr processAttributes,IntPtr threadAttributes,bool inherit,uint flags,IntPtr environment,string directory,ref StartupInfo startup,out ProcessInfo process);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool CreateProcessWithTokenW(IntPtr token,uint logon,string app,StringBuilder command,uint flags,IntPtr environment,string directory,ref StartupInfo startup,out ProcessInfo process);
    [DllImport("kernel32.dll")]private static extern bool CloseHandle(IntPtr handle);
    [DllImport("kernel32.dll")]private static extern IntPtr LocalFree(IntPtr memory);
    [DllImport("kernel32.dll")]private static extern uint WaitForSingleObject(IntPtr handle,uint milliseconds);
    [DllImport("kernel32.dll")]private static extern bool GetExitCodeProcess(IntPtr handle,out uint code);
}
