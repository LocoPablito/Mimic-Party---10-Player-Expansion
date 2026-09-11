$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK not found. Install .NET 8 SDK (or newer), then run this script again."
}

$coreProject = $env:MIMIC_PARTY_CORE_PROJECT
if ([string]::IsNullOrWhiteSpace($coreProject)) {
    $candidates = @(
        (Join-Path $Root "..\MimicParty-Modding\src\MimicParty.ModdingCore\MimicParty.ModdingCore.csproj"),
        (Join-Path $Root "_deps\MimicParty-Modding\src\MimicParty.ModdingCore\MimicParty.ModdingCore.csproj")
    )

    $coreProject = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($coreProject) -or -not (Test-Path -LiteralPath $coreProject)) {
    throw "Mimic Party Modding Core source was not found. Clone https://github.com/LocoPablito/MimicParty-Modding next to this repository or set MIMIC_PARTY_CORE_PROJECT to the Core .csproj path."
}

$project = Join-Path $Root "src\MimicParty.TenPlayerExpansion\MimicParty.TenPlayerExpansion.csproj"
$coreProject = (Resolve-Path -LiteralPath $coreProject).Path

Push-Location $Root
try {
    dotnet restore $project -p:MimicPartyCoreProject="$coreProject"
    dotnet build $project -c Release --no-restore -p:MimicPartyCoreProject="$coreProject"
    & "$Root\package.ps1"
}
finally {
    Pop-Location
}
