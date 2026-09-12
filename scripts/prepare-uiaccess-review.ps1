param()
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$version = (Select-Xml -LiteralPath (Join-Path $project 'src/CuteCat.App/CuteCat.App.csproj') -XPath '/Project/PropertyGroup/Version').Node.InnerText
$package = Join-Path $project "dist/CuteCat-$version-uiaccess-review"
if (Test-Path -LiteralPath $package) { throw 'The review package already exists; preserve it and choose a new version.' }
$payload = Join-Path $package 'payload'
dotnet publish (Join-Path $project 'src/CuteCat.App') -c Release -r win-x64 --self-contained true -o $payload -p:CuteCatUiAccess=true -p:DebugType=None -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) { throw 'UIAccess publish failed.' }
# This creates a signing key ONLY, never a trusted root or a UIAccess grant.
# Its private key is deleted after signing; the public certificate is reviewable.
$certificate = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=Cute Cat Local Preview $version" -CertStoreLocation Cert:\CurrentUser\My -KeyExportPolicy NonExportable -NotAfter (Get-Date).AddDays(30) -HashAlgorithm SHA256
try {
    $executable = Join-Path $payload 'CuteCat.exe'
    $signature = Set-AuthenticodeSignature -LiteralPath $executable -Certificate $certificate -HashAlgorithm SHA256
    if ($signature.SignerCertificate.Thumbprint -ne $certificate.Thumbprint -or $signature.Status -notin @('Valid','NotTrusted','UnknownError')) { throw 'Signing failed.' }
    Export-Certificate -Cert $certificate -FilePath (Join-Path $package 'local-preview.cer') | Out-Null
    Copy-Item -LiteralPath (Join-Path $project 'docs/third-party-notices.md') -Destination (Join-Path $payload 'CUTECAT-THIRD-PARTY-NOTICES.md')
    Copy-Item -LiteralPath (Join-Path $project 'docs/getting-started.md') -Destination (Join-Path $payload 'GETTING-STARTED.md')
    $files = @(Get-ChildItem -LiteralPath $payload -File -Recurse | ForEach-Object {
        [ordered]@{path=[IO.Path]::GetRelativePath($payload,$_.FullName);sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}
    })
    [ordered]@{version=$version;thumbprint=$certificate.Thumbprint;expires=$certificate.NotAfter.ToUniversalTime().ToString('o');files=$files} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $package 'manifest.json') -Encoding utf8
    $review=@"
# Cute Cat $version — local UIAccess preview

This is the updated desktop companion, including user-selected app rules, idle naps, accessories and modern menus. The app guard starts off with no apps selected. It requests normal closes and never answers save prompts or kills processes.

The existing user authorization covers UIAccess for the companion. Installing this version trusts its public signing certificate so Windows can keep the cat above notifications. This is a machine-wide certificate trust change and broader access to other apps' UI. Microsoft intends UIAccess for assistive technology; this remains a local experiment, not a public-release eligibility claim.

Certificate: $($certificate.Thumbprint)
Expires: $($certificate.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss')) UTC

Preparation has not added trust. The private signing key is deleted after signing. The installer requests explicit acknowledgment for new trust, preserves existing user data, and removes only known owned certificate entries during upgrade/uninstall. The setup wrapper remains a development build.

See docs/18-focus-companion.md, docs/16-uiaccess-review.md and docs/17-windows-installer.md for behavior, access scope and removal.
"@
    $review|Set-Content -LiteralPath (Join-Path $package 'REVIEW.md') -Encoding utf8
    [pscustomobject]@{Package=$package;Certificate=$certificate.Thumbprint;SignatureStatus=$signature.Status;Trusted=$false;Installed=$false}
}
finally { Remove-Item -LiteralPath "Cert:\CurrentUser\My\$($certificate.Thumbprint)" -DeleteKey }
