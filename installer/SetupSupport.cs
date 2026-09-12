using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;

// Small setup-only .NET Framework helper (part of Windows 11). No arbitrary
// command execution, input injection, process killing or user-data deletion.
internal static class SetupSupport
{
    private const int Failed=20, Running=21, Expired=22;
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if(args.Length==0)return Failed;
            if(args[0]=="status")
            {
                using(var certificate=Certificate(args[1]))
                {
                    if(certificate.NotAfter.ToUniversalTime()<=DateTime.UtcNow)return Expired;
                    return IsTrusted()?10:0;
                }
            }
            if(args[0]=="trust")
            {
                using(var certificate=Certificate(args[1]))
                {
                    if(certificate.NotAfter.ToUniversalTime()<=DateTime.UtcNow)return Expired;
                    if(IsTrusted())return 0;
                    using(var store=new X509Store(StoreName.Root,StoreLocation.LocalMachine))
                    {store.Open(OpenFlags.ReadWrite);store.Add(certificate);}
                    return 10; // This attempt created trust; caller owns rollback.
                }
            }
            if(args[0]=="undo-trust"){RemoveTrust();return 0;}
            string directory=SafeDirectory(args[1]);
            if(args[0]=="close")return CloseOwnApp(directory)?0:Running;
            if(args[0]=="cache")return CacheInstaller(directory,args[2])?10:0;
            if(args[0]=="undo-cache")
            {
                string recovery=RecoveryDirectory(directory),file=Path.Combine(recovery,"CuteCat-"+SetupBuild.Version+"-Setup.exe");
                if(File.Exists(file)&&SignedInstaller(file,SetupBuild.Version))File.Delete(file);
                if(Directory.Exists(recovery)&&Directory.GetFileSystemEntries(recovery).Length==0)Directory.Delete(recovery);
                return 0;
            }
            if(args[0]=="complete")
            {
                VerifyExecutable(directory);
                bool owned=args.Length>2&&args[2]=="1";
                string priorThumb=ReadIni(directory,"Thumbprint");
                bool priorOwned=ReadIni(directory,"OwnedCertificate")=="1"&&ReadIni(directory,"Product")=="Cute Cat"&&
                    string.Equals(ReadIni(directory,"Path"),directory,StringComparison.OrdinalIgnoreCase);
                owned|=priorOwned&&priorThumb==SetupBuild.CertificateThumbprint;
                string legacy=Path.Combine(directory,"cute-cat-install.json");
                if(File.Exists(legacy))
                {
                    using(var stream=File.OpenRead(legacy))
                    {
                        var receipt=(LegacyReceipt)new DataContractJsonSerializer(typeof(LegacyReceipt)).ReadObject(stream);
                        owned|=string.Equals(Path.GetFullPath(receipt.path),directory,StringComparison.OrdinalIgnoreCase)&&
                            string.Equals(receipt.thumbprint,SetupBuild.CertificateThumbprint,StringComparison.OrdinalIgnoreCase)&&receipt.addedTrust;
                    }
                }
                SecureDirectory(directory);
                WriteIni(directory,"OwnedCertificate",owned?"1":"0");
                WriteIni(directory,"Thumbprint",SetupBuild.CertificateThumbprint);
                WriteIni(directory,"Product","Cute Cat");
                WriteIni(directory,"Path",directory);
                PruneRecovery(directory);
                if(priorOwned&&priorThumb!=SetupBuild.CertificateThumbprint&&Array.IndexOf(SetupBuild.RetiredCertificates,priorThumb)>=0)RemoveTrust(priorThumb);
                RemoveMatchingUserShortcut(directory);
                // These two files belong exclusively to the preceding reviewed manual install.
                if(File.Exists(legacy))
                {
                    File.Delete(legacy);
                    string oldGuide=Path.Combine(directory,"GETTING-STARTED.md");
                    if(File.Exists(oldGuide))File.Delete(oldGuide);
                }
                string legacyNotes=Path.Combine(directory,"INSTALLATION-NOTES.md");
                if(File.Exists(legacyNotes))File.Delete(legacyNotes);
                return 0;
            }
            if(args[0]=="uninstall")
            {
                if(!CloseOwnApp(directory))return Running;
                RemoveRecovery(directory);
                RemoveStartupEntries(directory);
                RemoveMatchingUserShortcut(directory);
                // Missing/corrupt optional metadata must not prevent program removal.
                // Without ownership proof, preserve certificate trust instead.
                if(ReadIni(directory,"Product")=="Cute Cat"&&string.Equals(ReadIni(directory,"Path"),directory,StringComparison.OrdinalIgnoreCase)&&
                    ReadIni(directory,"OwnedCertificate")=="1"&&ReadIni(directory,"Thumbprint")==SetupBuild.CertificateThumbprint)RemoveTrust();
                return 0;
            }
            return Failed;
        }
        catch{return Failed;}
    }

    private static X509Certificate2 Certificate(string path)
    {
        var certificate=new X509Certificate2(path);
        if(!string.Equals(certificate.Thumbprint,SetupBuild.CertificateThumbprint,StringComparison.OrdinalIgnoreCase))
        {certificate.Dispose();throw new InvalidDataException();}
        return certificate;
    }
    private static bool IsTrusted()
    {
        using(var store=new X509Store(StoreName.Root,StoreLocation.LocalMachine))
        {
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates.Find(X509FindType.FindByThumbprint,SetupBuild.CertificateThumbprint,false).Count>0;
        }
    }
    private static void RemoveTrust(string thumbprint=null)
    {
        thumbprint=thumbprint??SetupBuild.CertificateThumbprint;
        if(thumbprint!=SetupBuild.CertificateThumbprint&&Array.IndexOf(SetupBuild.RetiredCertificates,thumbprint)<0)throw new InvalidDataException();
        using(var store=new X509Store(StoreName.Root,StoreLocation.LocalMachine))
        {
            store.Open(OpenFlags.ReadWrite);
            foreach(var certificate in store.Certificates.Find(X509FindType.FindByThumbprint,thumbprint,false))store.Remove(certificate);
        }
    }
    private static string SafeDirectory(string path)
    {
        string full=Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
        string parent=Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)).TrimEnd(Path.DirectorySeparatorChar);
        string leaf=Path.GetFileName(full);
        if(!string.Equals(Path.GetDirectoryName(full),parent,StringComparison.OrdinalIgnoreCase)||
            !(string.Equals(leaf,"Cute Cat",StringComparison.OrdinalIgnoreCase)||string.Equals(leaf,SetupBuild.LegacyDirectory,StringComparison.OrdinalIgnoreCase)))throw new InvalidDataException();
        if(Directory.Exists(full)&&(File.GetAttributes(full)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException();
        return full;
    }
    private static void VerifyExecutable(string directory)
    {
        using(var stream=File.OpenRead(Path.Combine(directory,"CuteCat.exe")))using(var sha=SHA256.Create())
        {
            string hash=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","");
            if(!string.Equals(hash,SetupBuild.ExecutableSha256,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException();
        }
    }
    private static void SecureDirectory(string directory)
    {
        var acl=new DirectorySecurity();acl.SetAccessRuleProtection(true,false);
        acl.SetOwner(new SecurityIdentifier("S-1-5-32-544"));
        var inheritance=InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit;
        foreach(string sid in new[]{"S-1-5-18","S-1-5-32-544"})
            acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(sid),FileSystemRights.FullControl,inheritance,PropagationFlags.None,AccessControlType.Allow));
        acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-5-32-545"),FileSystemRights.ReadAndExecute,inheritance,PropagationFlags.None,AccessControlType.Allow));
        Directory.SetAccessControl(directory,acl);
    }
    private static string ReadIni(string directory,string key)
    {
        var value=new StringBuilder(2048);
        GetPrivateProfileString("CuteCatSetup",key,"",value,value.Capacity,Path.Combine(directory,"setup-state.ini"));return value.ToString();
    }
    private static string RecoveryDirectory(string directory)
    {
        string recovery=Path.Combine(SafeDirectory(directory),"Recovery");
        if(Directory.Exists(recovery)&&(File.GetAttributes(recovery)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException();
        return recovery;
    }
    private static bool CacheInstaller(string directory,string source)
    {
        string recovery=RecoveryDirectory(directory);Directory.CreateDirectory(recovery);SecureDirectory(recovery);
        string destination=Path.Combine(recovery,"CuteCat-"+SetupBuild.Version+"-Setup.exe");
        bool added=!File.Exists(destination);
        using(var input=File.Open(source,FileMode.Open,FileAccess.Read,FileShare.Read))
        {
            if(!SignedInstaller(source,SetupBuild.Version))throw new InvalidDataException();
            if(!string.Equals(Path.GetFullPath(source),destination,StringComparison.OrdinalIgnoreCase))
            {
                string temp=Path.Combine(recovery,"pending-setup.exe");
                if(File.Exists(temp)&&(File.GetAttributes(temp)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException();
                using(var output=File.Open(temp,FileMode.Create,FileAccess.Write,FileShare.None))input.CopyTo(output);
                if(!SignedInstaller(temp,SetupBuild.Version))throw new InvalidDataException();
                if(File.Exists(destination))File.Delete(destination);
                File.Move(temp,destination);
            }
        }
        return added;
    }
    private static void PruneRecovery(string directory)
    {
        string recovery=RecoveryDirectory(directory),destination=Path.Combine(recovery,"CuteCat-"+SetupBuild.Version+"-Setup.exe");
        var candidates=new List<string>();
        foreach(string file in Directory.GetFiles(recovery,"CuteCat-*-Setup.exe"))
            if(SignedInstaller(file,null))candidates.Add(file);
        candidates.Sort(delegate(string a,string b){return Version.Parse(FileVersionInfo.GetVersionInfo(b).ProductVersion.Trim()).CompareTo(Version.Parse(FileVersionInfo.GetVersionInfo(a).ProductVersion.Trim()));});
        int other=0;
        foreach(string file in candidates)if(!string.Equals(file,destination,StringComparison.OrdinalIgnoreCase)&&++other>1)File.Delete(file);
    }
    private static void RemoveRecovery(string directory)
    {
        string recovery=RecoveryDirectory(directory);if(!Directory.Exists(recovery))return;
        foreach(string file in Directory.GetFiles(recovery,"CuteCat-*-Setup.exe"))
            if((File.GetAttributes(file)&FileAttributes.ReparsePoint)==0&&SignedInstaller(file,null))File.Delete(file);
        if(Directory.GetFileSystemEntries(recovery).Length==0)Directory.Delete(recovery);
    }
    private static bool SignedInstaller(string path,string expectedVersion)
    {
        try{return ValidateSignedInstaller(path,expectedVersion);}
        catch(CryptographicException){return false;}
        catch(IOException){return false;}
        catch(ArgumentException){return false;}
    }
    private static bool ValidateSignedInstaller(string path,string expectedVersion)
    {
        if((File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)return false;
        var info=FileVersionInfo.GetVersionInfo(path);Version version;
        if(info.ProductName==null||info.ProductName.Trim()!="Cute Cat"||!Version.TryParse((info.ProductVersion??"").Trim(),out version)||(expectedVersion!=null&&version.ToString(3)!=expectedVersion))return false;
        using(var certificate=new X509Certificate2(X509Certificate.CreateFromSignedFile(path)))
            if(certificate.Thumbprint!=SetupBuild.CertificateThumbprint)return false;
        var file=new TrustFile{Size=(uint)Marshal.SizeOf(typeof(TrustFile)),Path=path};IntPtr pointer=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TrustFile)));
        try
        {
            Marshal.StructureToPtr(file,pointer,false);
            var data=new TrustData{Size=(uint)Marshal.SizeOf(typeof(TrustData)),Ui=2,UnionChoice=1,File=pointer,ProviderFlags=0x1000};
            var action=new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");return WinVerifyTrust(new IntPtr(-1),ref action,ref data)==0;
        }
        finally{Marshal.DestroyStructure(pointer,typeof(TrustFile));Marshal.FreeHGlobal(pointer);}
    }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct TrustFile{public uint Size;[MarshalAs(UnmanagedType.LPWStr)]public string Path;public IntPtr File,Subject;}
    [StructLayout(LayoutKind.Sequential)]private struct TrustData{public uint Size;public IntPtr Policy,Sip;public uint Ui,Revocation,UnionChoice;public IntPtr File;public uint StateAction;public IntPtr State,Url;public uint ProviderFlags,Context;public IntPtr Signature;}
    [DllImport("wintrust.dll",ExactSpelling=true)]private static extern int WinVerifyTrust(IntPtr window,ref Guid action,ref TrustData data);
    private static void WriteIni(string directory,string key,string value)
    {if(!WritePrivateProfileString("CuteCatSetup",key,value,Path.Combine(directory,"setup-state.ini")))throw new IOException();}

    private static bool CloseOwnApp(string directory)
    {
        string image=Path.Combine(directory,"CuteCat.exe");
        if(!File.Exists(image))return true;
        uint session;var key=new StringBuilder(33);
        if(RmStartSession(out session,0,key)!=0)return false;
        try
        {
            if(RmRegisterResources(session,1,new[]{image},0,null,0,null)!=0)return false;
            uint needed=0,count=0,reasons=0;
            int result=RmGetList(session,out needed,ref count,null,ref reasons);
            if(result==0&&needed==0)return true;
            if(result!=234)return false;
            var info=new RmInfo[needed];count=needed;
            if(RmGetList(session,out needed,ref count,info,ref reasons)!=0)return false;
            bool found=false;
            for(int i=0;i<count;i++)
            {
                string actual=ImagePath(info[i].Process.Id);
                if(string.Equals(actual,image,StringComparison.OrdinalIgnoreCase))found=true;
                else if(RmAddFilter(session,null,ref info[i].Process,null,2)!=0)return false; // RmNoShutdown
            }
            if(!found)return true;
            // WM_QUERYENDSESSION/WM_ENDSESSION via Restart Manager allows WPF to
            // save state. Never use the force-shutdown flag or a process-name kill.
            if(RmShutdown(session,0,IntPtr.Zero)!=0)return false;
            for(int i=0;i<count;i++)if(string.Equals(ImagePath(info[i].Process.Id),image,StringComparison.OrdinalIgnoreCase))return false;
            return true;
        }
        finally{RmEndSession(session);}
    }
    private static string ImagePath(int pid)
    {
        IntPtr process=OpenProcess(0x1000,false,pid);if(process==IntPtr.Zero)return null;
        try{var text=new StringBuilder(32768);int size=text.Capacity;return QueryFullProcessImageName(process,0,text,ref size)?text.ToString():null;}
        finally{CloseHandle(process);}
    }
    private static void RemoveStartupEntries(string directory)
    {
        string expected="\""+Path.Combine(directory,"CuteCat.exe")+"\" --tray";
        foreach(string sid in Registry.Users.GetSubKeyNames())
        {
            if(!sid.StartsWith("S-1-5-21-",StringComparison.Ordinal)||sid.EndsWith("_Classes",StringComparison.Ordinal))continue;
            using(var run=Registry.Users.OpenSubKey(sid+@"\Software\Microsoft\Windows\CurrentVersion\Run",true))
                if(run!=null&&string.Equals(run.GetValue("CuteCat") as string,expected,StringComparison.OrdinalIgnoreCase))run.DeleteValue("CuteCat",false);
        }
    }
    private static void RemoveMatchingUserShortcut(string directory)
    {
        string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"Cute Cat.lnk");
        if(!File.Exists(path))return;
        object shell=null,shortcut=null;
        try
        {
            shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
            shortcut=shell.GetType().InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{path});
            string target=(string)shortcut.GetType().InvokeMember("TargetPath",BindingFlags.GetProperty,null,shortcut,null);
            if(string.Equals(target,Path.Combine(directory,"CuteCat.exe"),StringComparison.OrdinalIgnoreCase))File.Delete(path);
        }
        finally{if(shortcut!=null)Marshal.FinalReleaseComObject(shortcut);if(shell!=null)Marshal.FinalReleaseComObject(shell);}
    }

    [DataContract]private sealed class LegacyReceipt
    {
        [DataMember]public string path {get;set;}
        [DataMember]public string thumbprint {get;set;}
        [DataMember]public bool addedTrust {get;set;}
    }
    [StructLayout(LayoutKind.Sequential)]private struct RmProcess{public int Id;public System.Runtime.InteropServices.ComTypes.FILETIME Start;}
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct RmInfo
    {
        public RmProcess Process;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=256)]public string Application;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=64)]public string Service;
        public uint Type,Status,Session;
        [MarshalAs(UnmanagedType.Bool)]public bool Restartable;
    }
    [DllImport("rstrtmgr.dll",CharSet=CharSet.Unicode)]private static extern int RmStartSession(out uint session,uint flags,StringBuilder key);
    [DllImport("rstrtmgr.dll",CharSet=CharSet.Unicode)]private static extern int RmRegisterResources(uint session,uint fileCount,string[] files,uint processCount,RmProcess[] processes,uint serviceCount,string[] services);
    [DllImport("rstrtmgr.dll")]private static extern int RmGetList(uint session,out uint needed,ref uint count,[In,Out]RmInfo[] info,ref uint reasons);
    [DllImport("rstrtmgr.dll",CharSet=CharSet.Unicode)]private static extern int RmAddFilter(uint session,string module,ref RmProcess process,string service,int action);
    [DllImport("rstrtmgr.dll")]private static extern int RmShutdown(uint session,uint flags,IntPtr callback);
    [DllImport("rstrtmgr.dll")]private static extern int RmEndSession(uint session);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]private static extern uint GetPrivateProfileString(string section,string key,string fallback,StringBuilder value,int size,string file);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool WritePrivateProfileString(string section,string key,string value,string file);
    [DllImport("kernel32.dll")]private static extern IntPtr OpenProcess(uint access,bool inherit,int pid);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]private static extern bool QueryFullProcessImageName(IntPtr process,uint flags,StringBuilder text,ref int size);
    [DllImport("kernel32.dll")]private static extern bool CloseHandle(IntPtr handle);
}
