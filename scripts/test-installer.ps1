param([switch]$Install,[switch]$UninstallReinstall,[switch]$AcceptPreviewTrust,[string]$Output)
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$version=(Select-Xml -LiteralPath (Join-Path $project 'src/CuteCat.App/CuteCat.App.csproj') -XPath '/Project/PropertyGroup/Version').Node.InnerText
$setup=Join-Path $project "dist/installer/CuteCat-$version-Setup.exe"
$driver=Join-Path $project "dist/CuteCat-$version/CuteCat.exe"
$installed=Join-Path $env:ProgramFiles 'Cute Cat'
if(-not $Output){$Output=Join-Path $project ('artifacts/package-checks-'+(Get-Date -Format 'yyyyMMddHHmmss'))}
$Output=[IO.Path]::GetFullPath($Output);New-Item -ItemType Directory -Force -Path $Output|Out-Null
if($UninstallReinstall -and -not $Install){throw 'Uninstall/reinstall requires the explicit -Install switch.'}
$results=[Collections.Generic.List[object]]::new()
function Check([string]$Name,[bool]$Pass){$results.Add([ordered]@{name=$Name;pass=$Pass});if(-not $Pass){throw "Package check failed: $Name"}}
function RunSetup([string]$Name){
    $log=Join-Path $Output ($Name+'.log')
    $arguments=@('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/LOG="'+$log+'"'))
    if($AcceptPreviewTrust){$arguments+='/ACCEPTUIACCESS=1'}
    $process=Start-Process -FilePath $setup -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
    Check "$Name completed" ($process.ExitCode -eq 0 -and -not (Select-String -LiteralPath $log -Pattern 'raised an exception|Runtime error|could not finish installation' -Quiet))
}
try {
    $state=Join-Path $env:LOCALAPPDATA 'CuteCat/state.json'
    $before=if(Test-Path -LiteralPath $state){(Get-Content -LiteralPath $state -Raw|ConvertFrom-Json)}else{$null}
    if($Install){RunSetup 'install'}
    $publisher=Join-Path $installed 'CuteCat.exe'
    $recovery=Join-Path $installed "Recovery/CuteCat-$version-Setup.exe"
    $key='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{1597EF40-EF65-4A51-A78C-A82FBAA17344}_is1'
    Check 'single Installed apps registration has expected version' ((Get-ItemProperty -LiteralPath $key).DisplayVersion -eq $version)
    foreach($path in @($setup,$publisher,(Join-Path $installed 'CuteCat.SetupSupport.exe'),(Join-Path $installed 'unins000.exe'))){
        $signature=Get-AuthenticodeSignature -LiteralPath $path
        Check ('valid timestamped '+[IO.Path]::GetFileName($path)) ($signature.Status -eq 'Valid' -and $null -ne $signature.TimeStamperCertificate)
    }
    Check 'protected recovery copy is identical to release' ((Get-FileHash -LiteralPath $setup).Hash -eq (Get-FileHash -LiteralPath $recovery).Hash)
    $tampered=Join-Path $Output 'tampered-setup.exe';Copy-Item -LiteralPath $setup -Destination $tampered
    $stream=[IO.File]::Open($tampered,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite)
    try{$stream.Position=2048;$byte=$stream.ReadByte();$stream.Position=2048;$stream.WriteByte([byte]($byte -bxor 1))}finally{$stream.Dispose()}
    $cases=@(@{name='valid';path=$setup;version=$version;expected=$true},@{name='recovery';path=$recovery;version=$version;expected=$true},
        @{name='tampered';path=$tampered;version=$version;expected=$false},@{name='wrong-version';path=$setup;version='99.0.0';expected=$false},
        @{name='unsigned';path=$driver;version=$version;expected=$false},@{name='different-publisher';path=(Get-Command pwsh).Source;version=$version;expected=$false})
    foreach($case in $cases){
        $file=Join-Path $Output ($case.name+'.json')
        $arguments=@('--standard-user','--verify-installer',('"'+$case.path+'"'),'--publisher-file',('"'+$publisher+'"'),'--expected-version',$case.version,'--verification-output',('"'+$file+'"'))
        $process=Start-Process -FilePath $driver -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
        $verified=(Get-Content -LiteralPath $file -Raw|ConvertFrom-Json).verified
        Check ('updater '+$case.name) ($verified -eq $case.expected)
    }
    if($before){$after=Get-Content -LiteralPath $state -Raw|ConvertFrom-Json;Check 'upgrade preserves nickname and history' ($before.Settings.Nickname -eq $after.Settings.Nickname -and @($after.History).Count -ge @($before.History).Count)}
    if($UninstallReinstall){
        $stateHash=if(Test-Path -LiteralPath $state){(Get-FileHash -LiteralPath $state).Hash}else{''}
        $process=Start-Process -FilePath (Join-Path $installed 'unins000.exe') -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/LOG="'+(Join-Path $Output 'uninstall.log')+'"')) -WindowStyle Hidden -Wait -PassThru
        Check 'uninstaller completed' ($process.ExitCode -eq 0)
        Check 'uninstall removed registration and program directory' (-not (Test-Path -LiteralPath $key) -and -not (Test-Path -LiteralPath $installed))
        if($stateHash){Check 'uninstall preserves exact user state' ((Get-FileHash -LiteralPath $state).Hash -eq $stateHash)}
        RunSetup 'reinstall'
        Check 'reinstall restores verified recovery' ((Get-AuthenticodeSignature -LiteralPath $recovery).Status -eq 'Valid')
    }
}
finally{$results|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $Output 'package-checks.json');$results|Format-Table;Write-Output ('Evidence: '+$Output)}
