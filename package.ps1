$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Dist = Join-Path $Root "dist"
$Dll = Join-Path $Root "src\MimicParty.TenPlayerExpansion\bin\Release\net6.0\MimicParty10PlayerExpansion.dll"

if (-not (Test-Path -LiteralPath $Dll)) {
    throw "Expansion DLL was not built: $Dll"
}

Remove-Item $Dist -Recurse -Force -ErrorAction SilentlyContinue
New-Item $Dist -ItemType Directory | Out-Null

$Stage = Join-Path $Dist "stage"
New-Item (Join-Path $Stage "BepInEx\plugins") -ItemType Directory -Force | Out-Null

Copy-Item $Dll (Join-Path $Stage "BepInEx\plugins\MimicParty10PlayerExpansion.dll")
Copy-Item (Join-Path $Root "README.md") (Join-Path $Stage "README.txt")
Copy-Item (Join-Path $Root "CHANGELOG.md") (Join-Path $Stage "CHANGELOG.txt")
Copy-Item (Join-Path $Root "LICENSE.txt") (Join-Path $Stage "LICENSE.txt")

$Zip = Join-Path $Dist "MimicParty_10_Player_Expansion_v1.1.0_by_arribbaa_NEXUS.zip"
Compress-Archive -Path (Join-Path $Stage "*") -DestinationPath $Zip

$Hash = (Get-FileHash -LiteralPath $Zip -Algorithm SHA256).Hash.ToLowerInvariant()
"$Hash  $(Split-Path -Leaf $Zip)" | Set-Content -LiteralPath (Join-Path $Dist "SHA256SUMS.txt") -Encoding UTF8

Remove-Item $Stage -Recurse -Force

Write-Host "Release package created:" -ForegroundColor Green
Write-Host $Zip
