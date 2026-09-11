# Distribution preparation for Repair.ps1. Source plus this transformation are kept
# explicit until the private FIX5 work is consolidated. No game files are involved.
param([string]$Path = (Join-Path $PSScriptRoot 'Repair.ps1'))
$ErrorActionPreference = 'Stop'
$text = [IO.File]::ReadAllText($Path).Replace("`r`n", "`n")
# Windows PowerShell converts $null to an empty string for string parameters.
# NullString passes a genuine null backup path to System.IO.File.Replace.
$text = $text.Replace('[IO.File]::Replace($temp, $Target, $null)', '[IO.File]::Replace($temp, $Target, [NullString]::Value)')
[IO.File]::WriteAllBytes($Path, [Text.Encoding]::ASCII.GetBytes($text.Replace("`n", "`r`n")))
$hash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
if ($hash -ne 'f33a1408937587867dfedafb97cd0b57b57bff265a4115d6b622b79f7844a9d5') { throw "Unexpected prepared script: $hash" }
Write-Output "Prepared script SHA256: $hash"
