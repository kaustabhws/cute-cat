param([Parameter(Mandatory)][string]$Path,[Parameter(Mandatory)][string]$Thumbprint)
$ErrorActionPreference='Stop'
if($Thumbprint -notmatch '^[A-Fa-f0-9]{40}$'){throw 'Invalid signing certificate identity.'}
$signTool=Get-ChildItem -LiteralPath 'C:\Program Files (x86)\Windows Kits\10\bin' -Filter signtool.exe -Recurse |
    Where-Object {$_.Directory.Name -eq 'x64'} | Sort-Object FullName -Descending | Select-Object -First 1
if(-not $signTool){throw 'Install the Windows SDK signing tools before packaging.'}
& $signTool.FullName sign /q /fd SHA256 /sha1 $Thumbprint /tr http://timestamp.digicert.com /td SHA256 /d 'Cute Cat' $Path
if($LASTEXITCODE -ne 0){throw 'Authenticode signing or timestamping failed.'}
$signature=Get-AuthenticodeSignature -LiteralPath $Path
if($signature.SignerCertificate.Thumbprint -ne $Thumbprint -or -not $signature.TimeStamperCertificate -or $signature.Status -in @('HashMismatch','NotSigned')){throw 'The signed artifact failed validation.'}
