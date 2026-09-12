param([Parameter(Mandatory)][string]$Package, [switch]$Install)
$ErrorActionPreference = 'Stop'
$packageRoot = (Resolve-Path -LiteralPath $Package).Path
$manifest = Get-Content -LiteralPath (Join-Path $packageRoot 'manifest.json') -Raw | ConvertFrom-Json
$certificatePath = Join-Path $packageRoot 'local-preview.cer'
$certificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new($certificatePath)
if ($certificate.Thumbprint -ne $manifest.thumbprint -or $certificate.NotAfter -le (Get-Date)) { throw 'Certificate does not match the reviewed package or has expired.' }
$payload = [IO.Path]::GetFullPath((Join-Path $packageRoot 'payload')) + [IO.Path]::DirectorySeparatorChar
foreach ($file in $manifest.files) {
    $source = [IO.Path]::GetFullPath((Join-Path $payload $file.path))
    if (-not $source.StartsWith($payload,[StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid package path.' }
    if ((Get-Item -LiteralPath $source).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Package links are not allowed.' }
    if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $file.sha256) { throw "Package file changed: $($file.path)" }
}
$signature = Get-AuthenticodeSignature -LiteralPath (Join-Path $payload 'CuteCat.exe')
if ($signature.SignerCertificate.Thumbprint -ne $certificate.Thumbprint) { throw 'Unexpected executable signature.' }
$target = Join-Path ([Environment]::GetFolderPath('ProgramFiles')) ('CuteCat-Local-' + $manifest.version + '-' + $certificate.Thumbprint.Substring(0,8))
if (-not $Install) {
    [pscustomobject]@{InstallPath=$target;TrustStore='LocalMachine/Root';Certificate=$certificate.Thumbprint;Expires=$certificate.NotAfter;Permissions='UIAccess: can interact with other apps controls and draw above Windows UI';Installed=$false}
    return
}
# Run -Install only after the user explicitly approves this reviewed trust/access change.
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Installation requires an administrator; the app runs asInvoker.' }
if (Test-Path -LiteralPath $target) { throw 'Install destination already exists. Do not overwrite it.' }
$addedTrust = $false
try {
    New-Item -ItemType Directory -Path $target | Out-Null
    $acl = [Security.AccessControl.DirectorySecurity]::new()
    $acl.SetAccessRuleProtection($true,$false)
    $acl.SetOwner([Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))
    foreach ($entry in @(@('S-1-5-18','FullControl'),@('S-1-5-32-544','FullControl'),@('S-1-5-32-545','ReadAndExecute'))) {
        $rule = [Security.AccessControl.FileSystemAccessRule]::new([Security.Principal.SecurityIdentifier]::new($entry[0]),$entry[1],'ContainerInherit,ObjectInherit','None','Allow')
        $acl.AddAccessRule($rule)
    }
    Set-Acl -LiteralPath $target -AclObject $acl
    foreach ($file in $manifest.files) {
        $destination = Join-Path $target $file.path
        New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
        Copy-Item -LiteralPath (Join-Path $payload $file.path) -Destination $destination
        if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $file.sha256) { throw 'Installed file verification failed.' }
    }
    if (-not (Test-Path -LiteralPath "Cert:\LocalMachine\Root\$($certificate.Thumbprint)")) {
        Import-Certificate -FilePath $certificatePath -CertStoreLocation Cert:\LocalMachine\Root | Out-Null
        $addedTrust = $true
    }
    $verified = Get-AuthenticodeSignature -LiteralPath (Join-Path $target 'CuteCat.exe')
    if ($verified.Status -ne 'Valid') { throw "Installed signature is not trusted: $($verified.Status)" }
    [ordered]@{path=$target;thumbprint=$certificate.Thumbprint;addedTrust=$addedTrust;version=$manifest.version} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $target 'cute-cat-install.json') -Encoding utf8
    [pscustomobject]@{Installed=$true;Executable=(Join-Path $target 'CuteCat.exe');Certificate=$certificate.Thumbprint}
}
catch {
    if ($addedTrust) { Remove-Item -LiteralPath "Cert:\LocalMachine\Root\$($certificate.Thumbprint)" }
    # Only this new, absolute, checked Program Files directory can be removed.
    $programFiles = [IO.Path]::GetFullPath([Environment]::GetFolderPath('ProgramFiles')) + [IO.Path]::DirectorySeparatorChar
    if ([IO.Path]::GetFullPath($target).StartsWith($programFiles,[StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $target)) { Remove-Item -LiteralPath $target -Recurse -Force }
    throw
}
