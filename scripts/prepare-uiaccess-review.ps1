param([string]$CertificateThumbprint)
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$version=(Select-Xml -LiteralPath (Join-Path $project 'src/CuteCat.App/CuteCat.App.csproj') -XPath '/Project/PropertyGroup/Version').Node.InnerText
$package=Join-Path $project "dist/CuteCat-$version-uiaccess-review"
if(Test-Path -LiteralPath $package){throw 'The review package already exists; preserve it before preparing another build.'}
if($CertificateThumbprint){$certificate=Get-Item -LiteralPath "Cert:\CurrentUser\My\$CertificateThumbprint"}
else {
    $certificates=@(Get-ChildItem Cert:\CurrentUser\My | Where-Object {$_.Subject -eq 'CN=Cute Cat Protected Preview' -and $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date).AddDays(30)})
    if($certificates.Count -gt 1){throw 'Multiple preview identities exist. Select the intended certificate explicitly.'}
    if($certificates.Count -eq 1){$certificate=$certificates[0]}
    else {
        # Kept in this Windows user's CNG key store, never exported to a file or Git.
        $certificate=New-SelfSignedCertificate -Type CodeSigningCert -Subject 'CN=Cute Cat Protected Preview' -CertStoreLocation Cert:\CurrentUser\My -KeyAlgorithm RSA -KeyLength 3072 -Provider 'Microsoft Software Key Storage Provider' -KeyExportPolicy NonExportable -NotAfter (Get-Date).AddYears(3) -HashAlgorithm SHA256
    }
}
if(-not $certificate.HasPrivateKey){throw 'The signing identity has no accessible private key.'}
$payload=Join-Path $package 'payload'
dotnet publish (Join-Path $project 'src/CuteCat.App') -c Release -r win-x64 --self-contained true -o $payload -p:CuteCatUiAccess=true -p:DebugType=None -p:DebugSymbols=false
if($LASTEXITCODE -ne 0){throw 'UIAccess publish failed.'}
& (Join-Path $PSScriptRoot 'sign-artifact.ps1') -Path (Join-Path $payload 'CuteCat.exe') -Thumbprint $certificate.Thumbprint
Export-Certificate -Cert $certificate -FilePath (Join-Path $package 'local-preview.cer')|Out-Null
Copy-Item -LiteralPath (Join-Path $project 'docs/third-party-notices.md') -Destination (Join-Path $payload 'CUTECAT-THIRD-PARTY-NOTICES.md')
Copy-Item -LiteralPath (Join-Path $project 'docs/getting-started.md') -Destination (Join-Path $payload 'GETTING-STARTED.md')
$files=@(Get-ChildItem -LiteralPath $payload -File -Recurse | ForEach-Object {
    [ordered]@{path=[IO.Path]::GetRelativePath($payload,$_.FullName);sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}
})
[ordered]@{version=$version;thumbprint=$certificate.Thumbprint;expires=$certificate.NotAfter.ToUniversalTime().ToString('o');protectedPreview=($certificate.Subject -eq 'CN=Cute Cat Protected Preview');files=$files} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $package 'manifest.json') -Encoding utf8
@"
# Cute Cat $version — signed preview

Certificate: $($certificate.Thumbprint). Expires: $($certificate.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd')).
The nonexportable private key remains in this Windows user's CNG key store for future releases. It is not in this package or repository. Preparation does not add root trust or install the app.

Setup and its uninstaller are signed and timestamped with the same identity. Preview trust is a machine-wide opt-in, not a publicly verified publisher identity. UIAccess remains the previously authorized notification-layering experiment.
See docs/19-profiles-and-reliability.md for behavior, privacy, updates and recovery.
"@ | Set-Content -LiteralPath (Join-Path $package 'REVIEW.md') -Encoding utf8
[pscustomobject]@{Package=$package;Certificate=$certificate.Thumbprint;Expires=$certificate.NotAfter;TrustedByPreparation=$false}
