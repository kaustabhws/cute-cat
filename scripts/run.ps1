param([switch]$Standard)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$catVersion = (Select-Xml -Path (Join-Path $project 'src/CuteCat.App/CuteCat.App.csproj') -XPath '/Project/PropertyGroup/Version').Node.InnerText
$binary = Join-Path $project ('dist/CuteCat-' + $catVersion + '/CuteCat.exe')
$review = Join-Path $project ('dist/CuteCat-' + $catVersion + '-uiaccess-review/manifest.json')
if (-not $Standard -and (Test-Path -LiteralPath $review)) {
    $manifest = Get-Content -LiteralPath $review -Raw | ConvertFrom-Json
    if ($manifest.version -eq $catVersion -and $manifest.thumbprint -match '^[A-F0-9]{40}$') {
        $installed = Join-Path ([Environment]::GetFolderPath('ProgramFiles')) ('CuteCat-Local-' + $catVersion + '-' + $manifest.thumbprint.Substring(0,8) + '/CuteCat.exe')
        $entry = Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{1597EF40-EF65-4A51-A78C-A82FBAA17344}_is1' -ErrorAction SilentlyContinue
        if ($entry.InstallLocation -and $entry.DisplayVersion -eq $catVersion) { $installed = Join-Path $entry.InstallLocation 'CuteCat.exe' }
        if (Test-Path -LiteralPath $installed) {
            $signature = Get-AuthenticodeSignature -LiteralPath $installed
            if ($signature.Status -eq 'Valid' -and $signature.SignerCertificate.Thumbprint -eq $manifest.thumbprint) { $binary = $installed }
        }
    }
}
if (-not (Test-Path -LiteralPath $binary)) {
    & (Join-Path $PSScriptRoot 'build.ps1') -Publish
}
Start-Process -FilePath $binary -WorkingDirectory (Split-Path $binary) -WindowStyle Hidden
