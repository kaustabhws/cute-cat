using System.Runtime.InteropServices;

namespace CuteCat.App;

internal static class RuntimeAccess
{
    public static bool HasUiAccess
    {
        get
        {
            if(!OpenProcessToken(GetCurrentProcess(),8,out IntPtr token))return false;
            try{return GetTokenInformation(token,26,out int value,sizeof(int),out _)&&value!=0;}
            finally{Native.CloseHandle(token);}
        }
    }
    [DllImport("kernel32.dll")]private static extern IntPtr GetCurrentProcess();
    [DllImport("advapi32.dll")]private static extern bool OpenProcessToken(IntPtr process,uint desired,out IntPtr token);
    [DllImport("advapi32.dll")]private static extern bool GetTokenInformation(IntPtr token,int type,out int value,int size,out int needed);
}
