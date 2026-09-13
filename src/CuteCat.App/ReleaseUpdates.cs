using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using CuteCat.Core;

namespace CuteCat.App;

/// <summary>User-initiated, same-publisher updates. Release metadata is untrusted until verified.</summary>
internal sealed class ReleaseUpdates
{
    private static readonly HttpClient Client=CreateClient();
    public string Status { get; private set; }="Check for updates when you're ready.";
    public UpdatePackage? Available { get; private set; }
    public string? Downloaded { get; private set; }
    public bool Busy { get; private set; }
    public event Action? Changed;
    private static HttpClient CreateClient()
    {var client=new HttpClient{Timeout=TimeSpan.FromMinutes(3)};client.DefaultRequestHeaders.UserAgent.ParseAdd("CuteCat/"+BuildInfo.Version);return client;}
    public async Task Check()
    {
        if(Busy)return;Busy=true;Available=null;Downloaded=null;Status="Checking GitHub for updates…";Changed?.Invoke();
        try
        {
            byte[] bytes=await ReadLimited(new Uri($"https://api.github.com/repos/{UpdatePolicy.Repository}/releases?per_page=20"),1024*1024);
            using var json=JsonDocument.Parse(bytes);
            foreach(var release in json.RootElement.EnumerateArray().Where(r=>!r.GetProperty("draft").GetBoolean()&&!r.GetProperty("prerelease").GetBoolean())
                .Select(r=>(release:r,tag:r.GetProperty("tag_name").GetString()??""))
                .Where(r=>r.tag.StartsWith('v')&&UpdatePolicy.TryVersion(r.tag[1..],out _))
                .OrderByDescending(r=>Version.Parse(r.tag[1..])))
            {
                string version=release.tag[1..];if(Version.Parse(version)<=Version.Parse(BuildInfo.Version))continue;
                string fileName=UpdatePolicy.FileName(version),metadata=$"CuteCat-{version}-update.json";
                var assets=release.release.GetProperty("assets").EnumerateArray().ToArray();
                var exe=assets.FirstOrDefault(a=>a.GetProperty("name").GetString()==fileName);
                var info=assets.FirstOrDefault(a=>a.GetProperty("name").GetString()==metadata);
                if(exe.ValueKind!=JsonValueKind.Object||info.ValueKind!=JsonValueKind.Object)continue;
                if(info.GetProperty("browser_download_url").GetString()!=UpdatePolicy.AssetUrl(version,metadata))continue;
                var package=JsonSerializer.Deserialize<UpdatePackage>(await ReadLimited(new Uri(UpdatePolicy.AssetUrl(version,metadata)),8192));
                if(package is null||package.Version!=version||!UpdatePolicy.Valid(package,BuildInfo.Version)||exe.GetProperty("browser_download_url").GetString()!=package.Url||exe.GetProperty("size").GetInt64()!=package.Size)continue;
                Available=package;break;
            }
            Status=Available is null?"You're on the newest compatible version.":$"Version {Available.Version} is available. Download it to verify the installer.";
        }
        catch(Exception e)when(Expected(e)){Status="Could not check for updates. Try again when GitHub is reachable.";}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task Download()
    {
        if(Busy||Available is not { } package)return;
        Busy=true;Downloaded=null;Status="Downloading and verifying the installer…";Changed?.Invoke();
        string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CuteCat","Updates");
        string part=Path.Combine(folder,Guid.NewGuid().ToString("N")+".partial");
        try
        {
            if(!UpdatePolicy.Valid(package,BuildInfo.Version))throw new InvalidDataException();
            Directory.CreateDirectory(folder);
            using var response=await Client.GetAsync(package.Url,HttpCompletionOption.ResponseHeadersRead);response.EnsureSuccessStatusCode();
            if(response.Content.Headers.ContentLength is long length&&length!=package.Size)throw new InvalidDataException();
            using var timeout=new CancellationTokenSource(TimeSpan.FromMinutes(5));
            await using(var input=await response.Content.ReadAsStreamAsync(timeout.Token))
            await using(var output=new FileStream(part,FileMode.CreateNew,FileAccess.Write,FileShare.None,65536,true))
            {
                byte[] buffer=new byte[65536];long total=0;int count;
                while((count=await input.ReadAsync(buffer,timeout.Token))>0)
                {total+=count;if(total>package.Size)throw new InvalidDataException();await output.WriteAsync(buffer.AsMemory(0,count),timeout.Token);}
                if(total!=package.Size)throw new InvalidDataException();
            }
            using(var locked=File.Open(part,FileMode.Open,FileAccess.Read,FileShare.Read))
            {
                string hash=Convert.ToHexString(await SHA256.HashDataAsync(locked));
                if(!string.Equals(hash,package.Sha256,StringComparison.OrdinalIgnoreCase)||!InstallerSignature.SamePublisher(part,Environment.ProcessPath!,package.Version))throw new CryptographicException();
            }
            string destination=Path.Combine(folder,UpdatePolicy.FileName(package.Version));File.Move(part,destination,true);Downloaded=destination;
            Status=$"Version {package.Version} is verified. Open setup to review and install it.";
        }
        catch(Exception e)when(Expected(e)){Status="The download could not be verified. Nothing was installed. A publisher change needs a separate manual review.";}
        finally{try{if(File.Exists(part))File.Delete(part);}catch(IOException){}Busy=false;Changed?.Invoke();}
    }
    public void Install()=>Launch(Downloaded,Available?.Version);
    public void Launch(string? file,string? version)
    {
        if(Busy||file is null||version is null)return;
        FileStream? locked=null;
        try
        {
            locked=File.Open(file,FileMode.Open,FileAccess.Read,FileShare.Read);
            if(!InstallerSignature.SamePublisher(file,Environment.ProcessPath!,version))throw new CryptographicException();
            if(file==Downloaded&&Available is { } package&&!string.Equals(Convert.ToHexString(SHA256.HashData(locked)),package.Sha256,StringComparison.OrdinalIgnoreCase))throw new CryptographicException();
            var process=Process.Start(new ProcessStartInfo(file){UseShellExecute=true})??throw new IOException();
            var held=locked;locked=null;
            _=Task.Run(async()=>{try{await process.WaitForExitAsync();}finally{held.Dispose();process.Dispose();}});
            Status="Setup is open. Your data stays in your Windows profile.";
        }
        catch(Exception e)when(Expected(e)||e is System.ComponentModel.Win32Exception){Status="Setup was cancelled or its signature could not be verified. Nothing was changed by Cute Cat.";}
        finally{locked?.Dispose();Changed?.Invoke();}
    }
    public static List<(string Version,string Path)> RecoveryInstallers()
    {
        var result=new List<(string,string)>();string folder=Path.Combine(AppContext.BaseDirectory,"Recovery");
        if(!Directory.Exists(folder))return result;
        foreach(string file in Directory.EnumerateFiles(folder,"CuteCat-*-Setup.exe").Take(8))
        {
            string name=Path.GetFileName(file),version=name[8..^10];
            if(UpdatePolicy.TryVersion(version,out var parsed)&&parsed<Version.Parse(BuildInfo.Version)&&InstallerSignature.SamePublisher(file,Environment.ProcessPath!,version))result.Add((version,file));
        }
        return result.OrderByDescending(p=>Version.Parse(p.Item1)).ToList();
    }
    private static async Task<byte[]> ReadLimited(Uri uri,int limit)
    {
        using var response=await Client.GetAsync(uri,HttpCompletionOption.ResponseHeadersRead);response.EnsureSuccessStatusCode();
        if(response.Content.Headers.ContentLength>limit)throw new InvalidDataException();
        using var cancellation=new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await using var input=await response.Content.ReadAsStreamAsync(cancellation.Token);using var output=new MemoryStream();
        byte[] buffer=new byte[8192];int count;
        while((count=await input.ReadAsync(buffer,cancellation.Token))>0){if(output.Length+count>limit)throw new InvalidDataException();output.Write(buffer,0,count);}
        return output.ToArray();
    }
    private static bool Expected(Exception e)=>e is HttpRequestException or IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or CryptographicException or OperationCanceledException or FormatException or KeyNotFoundException;
}

internal static class InstallerSignature
{
    public static bool SamePublisher(string installer,string app,string expectedVersion)
    {
        try
        {
            if(!Trusted(installer)||!Trusted(app))return false;
            // X509CertificateLoader accepts certificate encodings, not signed PE files.
            // Keep this suppression at the two explicit Authenticode extraction calls.
#pragma warning disable SYSLIB0057
            using var a=X509Certificate.CreateFromSignedFile(app);using var b=X509Certificate.CreateFromSignedFile(installer);
#pragma warning restore SYSLIB0057
            var info=FileVersionInfo.GetVersionInfo(installer);
            return a.GetKeyAlgorithm()==b.GetKeyAlgorithm()&&a.GetPublicKeyString()==b.GetPublicKeyString()&&info.ProductName?.Trim()=="Cute Cat"&&
                Version.TryParse(info.ProductVersion?.Trim(),out var version)&&version.ToString(3)==expectedVersion;
        }
        catch(Exception e)when(e is CryptographicException or IOException or UnauthorizedAccessException or ArgumentException){return false;}
    }
    public static bool Trusted(string path)
    {
        var file=new TrustFile{Size=(uint)Marshal.SizeOf<TrustFile>(),Path=path};IntPtr pointer=Marshal.AllocHGlobal(Marshal.SizeOf<TrustFile>());
        try
        {
            Marshal.StructureToPtr(file,pointer,false);
            var data=new TrustData{Size=(uint)Marshal.SizeOf<TrustData>(),Ui=2,UnionChoice=1,File=pointer,StateAction=0,ProviderFlags=0x1000};
            var action=new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
            return WinVerifyTrust(new IntPtr(-1),ref action,ref data)==0;
        }
        finally{Marshal.DestroyStructure<TrustFile>(pointer);Marshal.FreeHGlobal(pointer);}
    }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct TrustFile{public uint Size;[MarshalAs(UnmanagedType.LPWStr)]public string Path;public IntPtr File,Subject;}
    [StructLayout(LayoutKind.Sequential)]private struct TrustData{public uint Size;public IntPtr Policy,Sip;public uint Ui,Revocation,UnionChoice;public IntPtr File;public uint StateAction;public IntPtr State,Url;public uint ProviderFlags,Context;public IntPtr Signature;}
    [DllImport("wintrust.dll",ExactSpelling=true)]private static extern int WinVerifyTrust(IntPtr window,ref Guid action,ref TrustData data);
}
