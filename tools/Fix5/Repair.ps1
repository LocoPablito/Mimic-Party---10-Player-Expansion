#requires -Version 5.1
[CmdletBinding()]
param([string]$GamePath = '', [switch]$Restore, [switch]$SelfTest)
Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

function File-Hash([string]$Path) {
    if (-not [IO.File]::Exists($Path)) { return 'MISSING' }
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}
function Assert-Hash([string]$Path, [string[]]$Allowed) {
    $actual = File-Hash $Path
    if ($Allowed -notcontains $actual) { throw "Hash-Pruefung fehlgeschlagen: $Path`nGefunden: $actual" }
    return $actual
}
function Safe-Target([string]$Root, [string]$Relative) {
    if ([IO.Path]::IsPathRooted($Relative) -or $Relative -match '(^|[\\/])\.\.([\\/]|$)') { throw 'Unsicherer relativer Pfad.' }
    $base = [IO.Path]::GetFullPath($Root).TrimEnd('\','/') + [IO.Path]::DirectorySeparatorChar
    $target = [IO.Path]::GetFullPath((Join-Path $Root $Relative))
    if (-not $target.StartsWith($base, [StringComparison]::OrdinalIgnoreCase)) { throw 'Pfad verlaesst Zielordner.' }
    return $target
}
function Write-Verified([string]$Source, [string]$Target, [string]$Expected) {
    [void](Assert-Hash $Source @($Expected))
    $parent = Split-Path -Parent $Target
    [void][IO.Directory]::CreateDirectory($parent)
    $temp = Join-Path $parent ('.arribbaa-' + [Guid]::NewGuid().ToString('N') + '.tmp')
    try {
        [IO.File]::Copy($Source, $temp, $false)
        [void](Assert-Hash $temp @($Expected))
        if ([IO.File]::Exists($Target)) { [IO.File]::Replace($temp, $Target, $null) }
        else { [IO.File]::Move($temp, $Target) }
        [void](Assert-Hash $Target @($Expected))
    } finally { if ([IO.File]::Exists($temp)) { [IO.File]::Delete($temp) } }
}
function Undo-State($State) {
    # Preflight ALL entries before restoring anything; preserve changes made later.
    foreach ($e in $State.Entries) {
        $target = Safe-Target $State.GamePath $e.Relative
        [void](Assert-Hash $target @($e.After, $e.Before))
        if ($e.Before -ne 'MISSING') { [void](Assert-Hash (Join-Path $State.BackupPath $e.BackupName) @($e.Before)) }
    }
    foreach ($e in $State.Entries) {
        $target = Safe-Target $State.GamePath $e.Relative
        if ($e.Before -eq 'MISSING') { if ([IO.File]::Exists($target)) { [IO.File]::Delete($target) } }
        else { Write-Verified (Join-Path $State.BackupPath $e.BackupName) $target $e.Before }
    }
}
function Install-Plan([string]$Root, [string]$BackupRoot, [object[]]$Plan) {
    $entries = @(); $number = 0
    # No game-folder writes before ALL input files and payloads passed preflight.
    foreach ($p in $Plan) {
        $target = Safe-Target $Root $p.Relative
        $before = Assert-Hash $target $p.AllowedBefore
        if ($p.After -ne 'MISSING') { [void](Assert-Hash $p.Source @($p.After)) }
        if ($before -ne $p.After) {
            $entries += [pscustomobject]@{ Relative=$p.Relative; Before=$before; After=$p.After; Source=$p.Source; BackupName=('{0:D2}.original' -f $number) }
            $number++
        }
    }
    if ($entries.Count -eq 0) { return [pscustomobject]@{ Changed=0; StatePath='' } }
    $backup = Join-Path $BackupRoot ('FIX5_' + (Get-Date -Format 'yyyyMMdd_HHmmss') + '_' + [Guid]::NewGuid().ToString('N').Substring(0,8))
    [void][IO.Directory]::CreateDirectory($backup)
    foreach ($e in $entries) {
        if ($e.Before -ne 'MISSING') {
            $target = Safe-Target $Root $e.Relative
            $dest = Join-Path $backup $e.BackupName
            [IO.File]::Copy($target, $dest, $false)
            [void](Assert-Hash $dest @($e.Before))
        }
    }
    $state = [pscustomobject]@{ Version=1; GamePath=[IO.Path]::GetFullPath($Root); BackupPath=$backup; Entries=$entries }
    $statePath = Join-Path $backup 'State.json'
    $state | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $statePath -Encoding UTF8
    try {
        foreach ($e in $entries) {
            $target = Safe-Target $Root $e.Relative
            [void](Assert-Hash $target @($e.Before))
            if ($e.After -eq 'MISSING') { [IO.File]::Delete($target) }
            else { Write-Verified $e.Source $target $e.After }
        }
        foreach ($e in $entries) { [void](Assert-Hash (Safe-Target $Root $e.Relative) @($e.After)) }
    } catch {
        $failure = $_
        try { Undo-State $state } catch { throw "Installation UND Ruecknahme fehlgeschlagen. Backup: $backup`n$failure`n$_" }
        throw "Installation abgebrochen; vorherige Dateien wiederhergestellt.`n$failure"
    }
    return [pscustomobject]@{ Changed=$entries.Count; StatePath=$statePath }
}
function Find-Game([string]$Requested) {
    if ($Requested) { return [IO.Path]::GetFullPath($Requested.Trim('"')) }
    $candidates = @('C:\Program Files (x86)\Steam\steamapps\common\Mimic Party', 'C:\Program Files\Steam\steamapps\common\Mimic Party', 'D:\SteamLibrary\steamapps\common\Mimic Party', 'E:\SteamLibrary\steamapps\common\Mimic Party')
    foreach ($p in $candidates) { if (Test-Path -LiteralPath (Join-Path $p 'Mimic Party.exe')) { return $p } }
    return [IO.Path]::GetFullPath((Read-Host 'Vollstaendiger Mimic-Party-Spielordner').Trim('"'))
}
function Assert-GameClosed {
    if (@(Get-Process -Name 'Mimic Party' -ErrorAction SilentlyContinue).Count -gt 0) { throw 'Mimic Party laeuft noch. Spiel zuerst normal schliessen.' }
}
function Read-RunLog([string]$Root, [datetime]$Since) {
    $candidates = @()
    foreach ($name in @('LogOutput.log','LogOutput.txt')) {
        $p = Join-Path $Root ('BepInEx\' + $name)
        if (Test-Path -LiteralPath $p) {
            $f = Get-Item -LiteralPath $p
            if ($f.LastWriteTimeUtc -ge $Since.AddSeconds(-2)) { $candidates += $f }
        }
    }
    if ($candidates.Count -eq 0) { throw 'Kein frisches BepInEx-Log von diesem Spielstart gefunden.' }
    return ($candidates | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1).FullName
}
function Evaluate-Log([string]$Text) {
    return [ordered]@{
        ChainloaderComplete = [bool]($Text -match 'Chainloader startup complete')
        CoreLoaded = [bool]($Text -match 'Mimic Party Modding Core v1\.0\.0 by arribbaa loaded\.')
        ExpansionActive = [bool]($Text -match 'is active: 10 total players in Classic/Versus')
        NativeHelperValues = [bool]($Text -match 'mode0=10, mode1=10, mode2=10, expected=10')
        NativeHelperPass = [bool]($Text -match 'CAPACITY SELF-CHECK PASS')
        NoLoggedErrors = [bool]($Text -notmatch '(?im)^\s*\[(Fatal|Error)\b')
    }
}
function Run-SelfTests {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('MP_FIX5_TEST_' + [Guid]::NewGuid().ToString('N'))
    [void][IO.Directory]::CreateDirectory($root)
    $passed = 0
    try {
        $game = Join-Path $root 'Spiel (Test) & Daten'; [void][IO.Directory]::CreateDirectory($game)
        $source = Join-Path $root 'new.bin'; [IO.File]::WriteAllText($source, 'NEW'); $new = File-Hash $source
        $oldFile = Join-Path $game 'target.dll'; [IO.File]::WriteAllText($oldFile, 'OLD'); $old = File-Hash $oldFile
        $plan = @([pscustomobject]@{Relative='target.dll'; Source=$source; After=$new; AllowedBefore=@($old,$new)})
        $r = Install-Plan $game (Join-Path $root 'backups') $plan
        if ((File-Hash $oldFile) -ne $new) { throw 'Copy test failed' }; $passed++
        $state = Get-Content -LiteralPath $r.StatePath -Raw | ConvertFrom-Json
        if ((File-Hash (Join-Path $state.BackupPath $state.Entries[0].BackupName)) -ne $old) { throw 'Backup test failed' }; $passed++
        $again = Install-Plan $game (Join-Path $root 'backups') $plan
        if ($again.Changed -ne 0) { throw 'Idempotency test failed' }; $passed++
        [IO.File]::WriteAllText($oldFile, 'USER_CHANGED'); $blocked=$false
        try { Undo-State $state } catch { $blocked=$true }
        if (-not $blocked -or [IO.File]::ReadAllText($oldFile) -ne 'USER_CHANGED') { throw 'Protected restore failed' }; $passed++
        [IO.File]::WriteAllText($oldFile, 'NEW'); Undo-State $state
        if ((File-Hash $oldFile) -ne $old) { throw 'Restore test failed' }; $passed++
        $bad = @([pscustomobject]@{Relative='target.dll'; Source=$source; After=('0'*64); AllowedBefore=@($old)})
        $blocked=$false; try { [void](Install-Plan $game (Join-Path $root 'badbackups') $bad) } catch { $blocked=$true }
        if (-not $blocked -or (File-Hash $oldFile) -ne $old -or (Test-Path -LiteralPath (Join-Path $root 'badbackups'))) { throw 'Fail-before-write test failed' }; $passed++
        $blocked=$false; try { [void](Safe-Target $game '..\outside') } catch { $blocked=$true }
        if (-not $blocked) { throw 'Path traversal test failed' }; $passed++
        $fatal = Evaluate-Log "[Message: BepInEx] Chainloader initialized`n[Fatal : BepInEx] Unable to execute IL2CPP chainloader"
        if ($fatal.NoLoggedErrors -or $fatal.ChainloaderComplete) { throw 'Fatal false-PASS test failed' }; $passed++
        $fake = Evaluate-Log '[Info] Loading [Mimic Party Modding Core] [Mimic Party - 10 Player Expansion]'
        if ($fake.CoreLoaded -or $fake.ExpansionActive -or $fake.NativeHelperPass) { throw 'Plugin-name false-PASS test failed' }; $passed++
        $success = Evaluate-Log "Chainloader startup complete`nMimic Party Modding Core v1.0.0 by arribbaa loaded.`nis active: 10 total players in Classic/Versus`nmode0=10, mode1=10, mode2=10, expected=10`nCAPACITY SELF-CHECK PASS"
        if (@($success.Values | Where-Object { -not $_ }).Count) { throw 'Positive log test failed' }; $passed++
        $remove = @([pscustomobject]@{Relative='target.dll'; Source=''; After='MISSING'; AllowedBefore=@($old)})
        $r = Install-Plan $game (Join-Path $root 'removebackups') $remove
        if (Test-Path -LiteralPath $oldFile) { throw 'Bridge removal test failed' }; $passed++
        Undo-State (Get-Content -LiteralPath $r.StatePath -Raw | ConvertFrom-Json)
        if ((File-Hash $oldFile) -ne $old) { throw 'Bridge restore test failed' }; $passed++
        $fresh = @([pscustomobject]@{Relative='newfolder\plugin.dll'; Source=$source; After=$new; AllowedBefore=@('MISSING')})
        $r = Install-Plan $game (Join-Path $root 'freshbackups') $fresh
        if ((File-Hash (Join-Path $game 'newfolder\plugin.dll')) -ne $new) { throw 'Fresh plugin install failed' }; $passed++
        Undo-State (Get-Content -LiteralPath $r.StatePath -Raw | ConvertFrom-Json)
        if (Test-Path -LiteralPath (Join-Path $game 'newfolder\plugin.dll')) { throw 'Fresh plugin rollback failed' }; $passed++
        Write-Output "SELFTEST PASS: $passed checks; PowerShell $($PSVersionTable.PSVersion)"
    } finally { Remove-Item -LiteralPath $root -Recurse -Force }
}
if ($SelfTest) { try { Run-SelfTests; exit 0 } catch { Write-Error $_; exit 1 } }

$allowedTargets = @('BepInEx\interop\UnityEngine.CoreModule.dll','BepInEx\core\UnityEngine.CoreModule.dll','BepInEx\plugins\MimicPartyModdingCore.dll','BepInEx\plugins\MimicParty10PlayerExpansion.dll')
$run = Join-Path $PSScriptRoot ('FIX5_Result_' + (Get-Date -Format 'yyyyMMdd_HHmmss') + '_' + [Guid]::NewGuid().ToString('N').Substring(0,6))
[void][IO.Directory]::CreateDirectory($run)
$transcript = $false; $exitCode=1; $report=[ordered]@{ Status='NOT_RUN'; Error=''; Stage='Preflight'; GameRuntimeVerified=$false; FullTenPlayerNetworkTest=$false }
try {
    Start-Transcript -LiteralPath (Join-Path $run 'Installer.log') | Out-Null; $transcript=$true
    Assert-GameClosed
    if ($Restore) {
        $statePath = (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'LAST_BACKUP.txt') -Raw).Trim()
        $state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
        if (@($state.Entries | Where-Object { $allowedTargets -notcontains $_.Relative }).Count) { throw 'Backup enthaelt unerwartete Zielpfade.' }
        Write-Host 'FIX5 auf den Zustand VOR dieser Reparatur zuruecksetzen (kein allgemeiner Mod-Uninstaller).'
        if ((Read-Host 'Mit JA bestaetigen') -cne 'JA') { throw 'Ruecknahme abgebrochen.' }
        Undo-State $state; $report.Status='RESTORED'; $exitCode=0
    } else {
        $manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Payload.json') -Raw | ConvertFrom-Json
        $GamePath = Find-Game $GamePath
        $report.GamePath=$GamePath
        if (-not (Test-Path -LiteralPath (Join-Path $GamePath 'Mimic Party.exe'))) { throw 'Mimic Party.exe fehlt im gewaehlten Ordner.' }
        [void](Assert-Hash (Join-Path $GamePath 'GameAssembly.dll') @($manifest.GameAssemblySHA256))
        [void](Assert-Hash (Join-Path $GamePath 'BepInEx\core\BepInEx.Unity.IL2CPP.dll') @($manifest.BepInExSHA256))
        [void](Assert-Hash (Join-Path $GamePath 'BepInEx\core\Il2CppInterop.Generator.dll') @($manifest.GeneratorSHA256))
        [void](Assert-Hash (Join-Path $GamePath 'BepInEx\core\Il2CppInterop.Runtime.dll') @($manifest.RuntimeSHA256))
        $fixed=$manifest.Payload.'UnityEngine.CoreModule.dll'; $original=$manifest.OriginalCoreModuleSHA256
        $plan = @([pscustomobject]@{ Relative=$allowedTargets[0]; Source=(Join-Path $PSScriptRoot '_payload\UnityEngine.CoreModule.dll'); After=$fixed; AllowedBefore=@($original,$fixed) },
                  [pscustomobject]@{ Relative=$allowedTargets[1]; Source=''; After='MISSING'; AllowedBefore=@('MISSING',$original,$fixed) })
        foreach ($name in @('MimicPartyModdingCore.dll','MimicParty10PlayerExpansion.dll')) {
            $sha=$manifest.Payload.$name
            $known=@('MISSING',$sha,'36383109400ab0d6f2a2482f2fbe6208c56a91cef6eea64ee16bdeb998c8107a','91dbf0c618dcbcf693e7b24e2398fbfdc35018624a8b67590b79335e8ddb3134','600032f360e64f5e3e623302a77e0862c8c8f70112151b517e2105607d542338')
            $plan += [pscustomobject]@{ Relative=('BepInEx\plugins\'+$name); Source=(Join-Path $PSScriptRoot ('_payload\'+$name)); After=$sha; AllowedBefore=$known }
        }
        # No reinstall, no interop deletion, no cache regeneration, no config rewrite.
        Write-Host 'FIX5: gezielte DLL-Reparatur + vorhandene Mod v1.1.1. Nur fuer den erfassten Build.'
        Write-Host 'Originale werden gesichert. Die alte CoreModule-Kopie unter core wird aus dem Ladepfad entfernt.'
        Write-Host 'Das Skript laedt nichts herunter. Es startet anschliessend das Spiel EINMAL.'
        [void](Read-Host 'Steam geoeffnet und Spiel geschlossen? Enter zum Reparieren und Testen')
        Assert-GameClosed
        $report.Stage='Install'
        $installed = Install-Plan $GamePath (Join-Path $GamePath 'arribbaa-backups') $plan
        if ($installed.Changed -gt 0) { Set-Content -LiteralPath (Join-Path $PSScriptRoot 'LAST_BACKUP.txt') -Value $installed.StatePath -Encoding UTF8 }
        $report.ChangedFiles=$installed.Changed; $report.BackupState=$installed.StatePath
        [void](Assert-Hash (Join-Path $GamePath 'GameAssembly.dll') @($manifest.GameAssemblySHA256))
        Write-Host 'DLL-Reparatur installiert und Hashes geprueft. Das ist noch kein bestandener Spieltest.'
        foreach ($name in @('LogOutput.log','LogOutput.txt')) {
            $oldlog=Join-Path $GamePath ('BepInEx\'+$name)
            if (Test-Path -LiteralPath $oldlog) { Move-Item -LiteralPath $oldlog -Destination (Join-Path $run ('Before-'+$name)) }
        }
        $report.Stage='Runtime'
        $started=[DateTime]::UtcNow
        Start-Process -FilePath (Join-Path $GamePath 'Mimic Party.exe') -WorkingDirectory $GamePath | Out-Null
        Write-Host 'Allein eine PRIVATE Classic-Lobby pruefen. Danach bei Bedarf Versus. Keine weiteren Spieler noetig.'
        Write-Host 'BepInEx-Konsole offen lassen. Spiel selbst normal beenden, DANN hier Enter druecken.'
        [void](Read-Host 'Nach Beenden des Spiels: Enter')
        Assert-GameClosed
        $log=Read-RunLog $GamePath $started
        Copy-Item -LiteralPath $log -Destination (Join-Path $run 'Runtime.log')
        $checks=Evaluate-Log (Get-Content -LiteralPath $log -Raw)
        $checks.GameAssemblyUnchanged = ((File-Hash (Join-Path $GamePath 'GameAssembly.dll')) -eq $manifest.GameAssemblySHA256)
        $checks.RepairedInteropStillInstalled = ((File-Hash (Join-Path $GamePath $allowedTargets[0])) -eq $fixed)
        $report.Checks=$checks
        $ok=(@($checks.Values | Where-Object { -not $_ }).Count -eq 0)
        $report.Status = if ($ok) { 'LOADER_AND_CAPACITY_HELPER_PASS' } else { 'NEEDS_REVIEW' }
        $report.Stage='Complete'
        foreach ($key in $checks.Keys) { Write-Host ($key + ': ' + $(if ($checks[$key]) {'PASS'} else {'FAIL'})) }
        Write-Host 'Ein Helper-PASS bestaetigt nicht automatisch zehn Netzwerkverbindungen oder die komplette Versus-Logik.'
        if ($ok) { $exitCode=0 }
    }
} catch {
    $report.Status='ERROR'; $report.Error=$_.ToString(); Write-Host ('FEHLER: ' + $_.ToString())
} finally {
    try {
        if ($GamePath -and (Test-Path -LiteralPath $GamePath)) {
            foreach ($rel in @('BepInEx\LogOutput.log','BepInEx\LogOutput.txt','BepInEx\config\BepInEx.cfg','BepInEx\config\com.arribbaa.mimicparty.10playerexpansion.cfg','BepInEx\interop\assembly-hash.txt')) {
                $src=Join-Path $GamePath $rel
                if (Test-Path -LiteralPath $src) { Copy-Item -LiteralPath $src -Destination (Join-Path $run ($rel.Replace('\','__'))) -Force }
            }
            $inventory=@()
            foreach ($rel in $allowedTargets) { $inventory += [pscustomobject]@{Path=$rel; SHA256=(File-Hash (Join-Path $GamePath $rel))} }
            $report.Inventory=$inventory
            # On failure collect only technical assemblies relevant to the next diagnosis.
            if ($report.Status -in @('ERROR','NEEDS_REVIEW')) {
                $names=@('Mimick.Runtime.dll','Mimick.Fusion.dll','Fusion.Runtime.dll','Il2Cppmscorlib.dll','Unity.Scripting.dll','UnityEngine.CoreModule.dll')
                $text = [string]$report.Error
                $currentLog=Join-Path $GamePath 'BepInEx\LogOutput.log'
                if (Test-Path -LiteralPath $currentLog) { $text += Get-Content -LiteralPath $currentLog -Raw }
                foreach ($m in [regex]::Matches($text,"(?:in assembly|file or assembly) '([A-Za-z0-9_.-]+)(?:,|')")) { $names += ($m.Groups[1].Value + '.dll') }
                $diag=Join-Path $run 'TechnicalAssemblies'; [void][IO.Directory]::CreateDirectory($diag)
                foreach ($name in ($names | Select-Object -Unique)) {
                    $src=Join-Path $GamePath ('BepInEx\interop\'+$name)
                    if (Test-Path -LiteralPath $src) { Copy-Item -LiteralPath $src -Destination (Join-Path $diag $name) }
                }
            }
        }
    } catch { $report.CollectionWarning=$_.ToString() }
    $report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $run 'Report.json') -Encoding UTF8
    if ($transcript) { Stop-Transcript | Out-Null }
    try {
        $zip=$run+'.zip'; Compress-Archive -LiteralPath $run -DestinationPath $zip -CompressionLevel Optimal
        Write-Host ''; Write-Host ('ERGEBNIS: '+$report.Status); Write-Host 'Diese Ergebnis-ZIP hier hochladen (keinen alten FIX-Installer erneut starten):'; Write-Host $zip
        Write-Host 'Enthaelt technische Logs/Pfade und bei Fehlern ausgewaehlte DLLs, keine Saves oder Sprachaufnahmen. Kein automatischer Upload.'
    } catch { Write-Host ('ZIP-Erstellung fehlgeschlagen. Ergebnisordner: '+$run) }
}
exit $exitCode
