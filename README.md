# Mimic Party — 10 Player Expansion

**Version 1.1.2 · by arribbaa**

Bring a bigger group into Mimic Party. This mod raises the shared Classic/Versus capacity to **10 total players**, with free RED/BLUE team selection in Versus. It uses runtime patches rather than permanently replacing the game's native binary.

[Download release](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/releases/latest) · [Nexus Mods](https://www.nexusmods.com/mimicparty/mods/1) · [Modding Core](https://github.com/LocoPablito/MimicParty-Modding) · [Support](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/issues)

## Features

| Area | Behaviour |
|---|---|
| Classic | Up to 10 players; the tested lobby displays 1/10 when alone. |
| Versus | Up to 10 total; players choose RED or BLUE. No forced 5v5 split is imposed. |
| Configuration | MaxPlayers from 6 to 10, default 10. |
| Playback voice | Hooks temporarily suppress live voice during the playback/performance phase. |
| Rematch | Hooks are designed to keep the lobby together during rematch, without replacing normal disconnect handling. |
| Verification | In-process native-capacity self-check and clear hook/startup logging. |

The mode selection cards in the documented build update to "Up to 10 players" through the shared capacity logic. Workshop pack handling is not replaced by this mod.

## Choose your download

**Recommended: `MimicParty_10_Player_Expansion_v1.1.2_COMPLETE.zip` — Core included.**

This contains the verified Expansion 1.1.2, Core 1.0.0 and the Core package's Interop Bootstrap 1.0.0 compatibility component. **BepInEx is NOT included.** No separate Core download is needed for Complete.

**Advanced/update option: `MimicParty_10_Player_Expansion_v1.1.2_ONLY.zip`.** This contains only the Expansion. Install the official Core package, including its bootstrap, separately. Never install differently named duplicate copies of the same plugin.

## Requirements

Mimic Party for Windows x64 / Steam. Documented game build: **v0.1.73**, captured 11 September 2026, Unity **6000.4.2f1**. Use **BepInEx 6 Unity IL2CPP Windows x64, build 788**.

[Official pinned BepInEx download](https://builds.bepinex.dev/projects/bepinex_be/788/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.788%2B5b766a3.zip)

BepInEx 5 and Unity Mono builds are not substitutes. See [supported build](SUPPORTED_BUILD.md). Future Steam updates are not automatically guaranteed compatible.

## Install

1. Close the game. If using the old v1.0.x on-disk patch, restore the original files with its uninstaller or Steam's **Verify integrity** first. Do not install over a modified GameAssembly.dll.
2. Extract the official BepInEx archive into the folder containing `Mimic Party.exe`.
3. Extract the **Complete** archive into the same folder before starting the game. Preserve the folder paths below.
4. Start normally through Steam. Wait for first-start interop generation. Create a **private** lobby; keep participants on the same documented game/mod setup when testing multiplayer.

```text
BepInEx/plugins/MimicPartyModdingCore.dll
BepInEx/plugins/MimicParty10PlayerExpansion.dll
BepInEx/patchers/MimicParty.InteropBootstrap.dll
```

No installer EXE, command file, hotkey or repeated test sequence is needed. The compatibility bootstrap handles the recognized generated CoreModule duplicate-helper problem locally; already-valid files are left unchanged. It does not bundle or overwrite native game binaries.

Existing users with a working repaired setup can replace the Expansion with this same 1.1.2 DLL without regenerating interop files. Do not rerun the old private FIX installers.

## Configure

After the first successful launch, close the game and edit:
`BepInEx/config/com.arribbaa.mimicparty.10playerexpansion.cfg`

```ini
[Multiplayer]
MaxPlayers = 10
```

Supported range: 6–10. The limit is total occupancy, not a separate quota per team. The mod does not enforce equal teams; a 4/6 split is within the intended total. Versus still needs at least two players to start. A minimum-player message is not the maximum room size.

## Test status

The current Expansion binary passed an actual Windows startup/hook/capacity test on the documented build: all 13 automatic checks passed, the native helper returned 10 for the tested selectors, and the screenshots show Classic 1/10 and both mode cards at 10. The author additionally reports a successful seven-person test of an **older release**. That historical test is not described as a ten-client test of 1.1.2.

Full ten-client gameplay, audio/rematch behaviour through a complete large-group session and every team distribution are not independently established. The new automatic bootstrap has separate metadata/regression tests; the prior working setup does not itself validate that new fresh-install route. See [validation scope](VALIDATION.md).

## Uninstall / support

Close the game and remove `BepInEx/plugins/MimicParty10PlayerExpansion.dll`. Keep the Core/bootstrap if another mod needs them. The plugin's configuration may be removed separately. The native capacity patch is in memory and disappears when the game exits.

If a BepInEx console is visible, minimize it rather than closing it during play. For a hidden console on the next launch, set `[Logging.Console] Enabled = false` in `BepInEx/config/BepInEx.cfg` and restart.

Support: include installed versions, game build and a relevant excerpt from `BepInEx/LogOutput.log`. Remove personal paths and identifiers before posting. Do not upload game binaries or private diagnostic archives. No antivirus exception should be needed as a routine installation step; request review of unexpected detections instead.

[Changelog](CHANGELOG.md) · [Security / file behaviour](SECURITY.md) · [Build / provenance](BUILDING.md) · [License](LICENSE.txt)

Unofficial community mod. Mimic Party, Unity, BepInEx and related third-party software belong to their respective owners.
