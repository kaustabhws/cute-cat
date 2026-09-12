$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $project '.tools/inno-7.1.0'
$compiler=Join-Path $directory 'ISCC.exe'
if(Test-Path -LiteralPath $compiler){Write-Output $compiler;return}
$downloads=Join-Path $project '.tools/downloads'
New-Item -ItemType Directory -Force -Path $downloads|Out-Null
$installer=Join-Path $downloads 'innosetup-7.1.0-x64.exe'
if(-not(Test-Path -LiteralPath $installer)){Invoke-WebRequest -Uri 'https://github.com/jrsoftware/issrc/releases/download/is-7_1_0/innosetup-7.1.0-x64.exe' -OutFile $installer}
if((Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash -ne '0362A383ED217D4C4239B5933866DD96D3EB2102737DA92F80F6057A4B40DF2F'){throw 'Unexpected compiler archive hash.'}
$process=Start-Process -FilePath $installer -ArgumentList @('/PORTABLE=1','/CURRENTUSER','/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/NOICONS',('/DIR="'+$directory+'"')) -WindowStyle Hidden -Wait -PassThru
if($process.ExitCode -ne 0 -or -not(Test-Path -LiteralPath $compiler)){throw 'Portable compiler setup failed.'}
Write-Output $compiler
