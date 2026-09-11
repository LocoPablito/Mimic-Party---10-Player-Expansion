$ErrorActionPreference = 'Stop'
$Root = $PSScriptRoot
$coreProject = $env:MIMIC_PARTY_CORE_PROJECT
if ([string]::IsNullOrWhiteSpace($coreProject)) {
    foreach ($candidate in @(
        (Join-Path $Root '../MimicParty-Modding/src/MimicParty.ModdingCore/MimicParty.ModdingCore.csproj'),
        (Join-Path $Root '_deps/MimicParty-Modding/src/MimicParty.ModdingCore/MimicParty.ModdingCore.csproj')
    )) { if (Test-Path -LiteralPath $candidate) { $coreProject = $candidate; break } }
}
if ([string]::IsNullOrWhiteSpace($coreProject) -or -not (Test-Path -LiteralPath $coreProject)) {
    throw 'Set MIMIC_PARTY_CORE_PROJECT to the Core project, or clone it next to this repository.'
}
$coreProject = (Resolve-Path -LiteralPath $coreProject).Path
$project = Join-Path $Root 'src/MimicParty.TenPlayerExpansion/MimicParty.TenPlayerExpansion.csproj'
dotnet restore $project -p:MimicPartyCoreProject="$coreProject"
if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }
dotnet build $project -c Release --no-restore -p:MimicPartyCoreProject="$coreProject"
if ($LASTEXITCODE -ne 0) { throw 'Expansion build failed' }
