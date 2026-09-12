param([Parameter(Mandatory)][string]$InstallPath)
$ErrorActionPreference = 'Stop'
$target = (Resolve-Path -LiteralPath $InstallPath).Path
$programFiles = [IO.Path]::GetFullPath([Environment]::GetFolderPath('ProgramFiles')) + [IO.Path]::DirectorySeparatorChar
if (-not $target.StartsWith($programFiles,[StringComparison]::OrdinalIgnoreCase) -or (Split-Path $target -Leaf) -notmatch '^CuteCat-Local-[0-9.]+-[A-F0-9]{8}$') { throw 'Not a Cute Cat review installation.' }
$receipt = Get-Content -LiteralPath (Join-Path $target 'cute-cat-install.json') -Raw | ConvertFrom-Json
if ($receipt.path -ne $target -or $receipt.thumbprint -notmatch '^[A-F0-9]{40}$') { throw 'Invalid installation receipt.' }
if (Get-Process CuteCat -ErrorAction SilentlyContinue | Where-Object {$_.Path -eq (Join-Path $target 'CuteCat.exe')}) { throw 'Quit this Cute Cat instance before removing it.' }
$shortcutPath = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Cute Cat.lnk'
if (Test-Path -LiteralPath $shortcutPath) {
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    if ($shortcut.TargetPath -eq (Join-Path $target 'CuteCat.exe')) { Remove-Item -LiteralPath $shortcutPath }
}
if ($receipt.addedTrust -and (Test-Path -LiteralPath "Cert:\LocalMachine\Root\$($receipt.thumbprint)")) { Remove-Item -LiteralPath "Cert:\LocalMachine\Root\$($receipt.thumbprint)" }
Remove-Item -LiteralPath $target -Recurse -Force
Write-Output 'Removed this review installation and its added certificate trust. User settings are preserved.'
