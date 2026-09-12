param([switch]$Publish)
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    dotnet build CuteCat.slnx -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
    dotnet run --project tests/CuteCat.Checks -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Core checks failed.' }
    if ($Publish) {
        $catVersion = (Select-Xml -Path src/CuteCat.App/CuteCat.App.csproj -XPath '/Project/PropertyGroup/Version').Node.InnerText
        $catDistribution = Join-Path dist ('CuteCat-' + $catVersion)
        dotnet publish src/CuteCat.App -c Release -r win-x64 --self-contained true -o $catDistribution -p:DebugType=None -p:DebugSymbols=false
        if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
        Copy-Item -LiteralPath 'docs/getting-started.md' -Destination (Join-Path $catDistribution 'GETTING-STARTED.md')
        Copy-Item -LiteralPath 'docs/third-party-notices.md' -Destination (Join-Path $catDistribution 'CUTECAT-THIRD-PARTY-NOTICES.md')
        Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $catDistribution 'CuteCat.exe') | Format-List
    }
}
finally { Pop-Location }
