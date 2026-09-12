param([string]$Compiler)
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
if(-not $Compiler){$Compiler=Join-Path $project '.tools/inno-7.1.0/ISCC.exe'}
if(-not(Test-Path -LiteralPath $Compiler)){throw 'Inno Setup 7.1.0 is required. See docs/17-windows-installer.md for the pinned portable compiler.'}
$version=(Select-Xml -LiteralPath (Join-Path $project 'src/CuteCat.App/CuteCat.App.csproj') -XPath '/Project/PropertyGroup/Version').Node.InnerText
$package=Join-Path $project "dist/CuteCat-$version-uiaccess-review"
$payload=[IO.Path]::GetFullPath((Join-Path $package 'payload'))
$manifest=Get-Content -LiteralPath (Join-Path $package 'manifest.json') -Raw|ConvertFrom-Json
foreach($file in $manifest.files){
    $path=[IO.Path]::GetFullPath((Join-Path $payload $file.path))
    if(-not $path.StartsWith($payload+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Invalid payload path.'}
    if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.sha256){throw "Reviewed payload changed: $($file.path)"}
}
$signature=Get-AuthenticodeSignature -LiteralPath (Join-Path $payload 'CuteCat.exe')
if($signature.SignerCertificate.Thumbprint -ne $manifest.thumbprint){throw 'The app signer does not match the reviewed certificate.'}
$certificate=Join-Path $package 'local-preview.cer'
$build=Join-Path $project 'artifacts/installer-build'
$generated=Join-Path $project 'installer/generated'
New-Item -ItemType Directory -Force -Path $build,$generated|Out-Null
Get-ChildItem -LiteralPath $payload -Directory -Recurse|ForEach-Object{
    'Name: "{app}\'+[IO.Path]::GetRelativePath($payload,$_.FullName)+'"; Flags: uninsalwaysuninstall'
}|Set-Content -LiteralPath (Join-Path $generated 'Directories.iss') -Encoding utf8
$art=Join-Path $build 'art'
dotnet run --project (Join-Path $project 'tools/CuteCat.SetupArtwork') -c Release -- $art
if($LASTEXITCODE -ne 0){throw 'Installer artwork export failed.'}
$exeHash=(Get-FileHash -LiteralPath (Join-Path $payload 'CuteCat.exe') -Algorithm SHA256).Hash
$legacy='CuteCat-Local-'+$version+'-'+$manifest.thumbprint.Substring(0,8)
$history=Get-Content -LiteralPath (Join-Path $project 'installer/CertificateHistory.json') -Raw|ConvertFrom-Json
$retired=@($history.RetiredPreviewCertificates|Where-Object{$_ -ne $manifest.thumbprint})
if(@($retired|Where-Object{$_ -notmatch '^[A-F0-9]{40}$'}).Count){throw 'Invalid retired certificate identity.'}
$retiredCode=($retired|ForEach-Object{'"'+$_+'"'}) -join ','
$constants='internal static class SetupBuild { public const string Version="'+$version+'"; public const string CertificateThumbprint="'+$manifest.thumbprint+'"; public const string ExecutableSha256="'+$exeHash+'"; public const string LegacyDirectory="'+$legacy+'"; public static readonly string[] RetiredCertificates=new string[]{'+$retiredCode+'}; }'
$constants|Set-Content -LiteralPath (Join-Path $generated 'SetupBuild.cs') -Encoding utf8
$helper=Join-Path $build 'CuteCat.SetupSupport.exe'
$csc=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $csc /nologo /target:winexe /platform:x64 /optimize+ /warnaserror+ ('/out:'+$helper) /reference:System.Runtime.Serialization.dll (Join-Path $project 'installer/SetupSupport.cs') (Join-Path $generated 'SetupBuild.cs')
if($LASTEXITCODE -ne 0){throw 'Setup helper compilation failed.'}
& (Join-Path $PSScriptRoot 'sign-artifact.ps1') -Path $helper -Thumbprint $manifest.thumbprint
$expiry=([DateTimeOffset]::Parse($manifest.expires)).ToString('d MMMM yyyy',[Globalization.CultureInfo]::InvariantCulture)
foreach($file in @('Preview.txt','Getting-started.txt')){
    (Get-Content -LiteralPath (Join-Path $project ('installer/'+$file)) -Raw).Replace('@@EXPIRY@@',$expiry)|Set-Content -LiteralPath (Join-Path $generated $file) -Encoding utf8
}
$defines=[ordered]@{AppVersion=$version;SetupVersion=($version+'.1');PayloadDir=$payload;SupportExe=$helper;CertificateFile=$certificate;CertificateExpiry=$expiry;ExecutableSha256=$exeHash;LegacyDirectory=$legacy;ArtDir=$art}
$defines.GetEnumerator()|ForEach-Object{'#define '+$_.Key+' "'+$_.Value+'"'}|Set-Content -LiteralPath (Join-Path $generated 'Build.iss') -Encoding utf8
$powershell=(Get-Command pwsh).Source
$signCommand='$q'+$powershell+'$q -NoProfile -NonInteractive -File $q'+(Join-Path $PSScriptRoot 'sign-artifact.ps1')+'$q -Thumbprint '+$manifest.thumbprint+' -Path $f'
& $Compiler /Q ('/SCuteCatSign='+$signCommand) (Join-Path $project 'installer/CuteCat.iss')
if($LASTEXITCODE -ne 0){throw 'Setup compilation failed.'}
$setup=Join-Path $project "dist/installer/CuteCat-$version-Setup.exe"
Get-FileHash -LiteralPath $setup -Algorithm SHA256
$hash=(Get-FileHash -LiteralPath $setup -Algorithm SHA256).Hash
$name=[IO.Path]::GetFileName($setup)
[ordered]@{Version=$version;Url="https://github.com/kaustabhws/cute-cat/releases/download/v$version/$name";Size=(Get-Item -LiteralPath $setup).Length;Sha256=$hash} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $project "dist/installer/CuteCat-$version-update.json") -Encoding utf8
"$hash  $name" | Set-Content -LiteralPath ($setup+'.sha256') -Encoding ascii
