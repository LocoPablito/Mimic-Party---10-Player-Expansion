# 10 Player Expansion for Mimic Party

**Runtime 1.1.2 · by arribbaa**

Up to **10 total players** in private Classic and Versus lobbies. Versus keeps the game's RED/BLUE selection: the mod does not impose equal teams or a separate five-player quota per side.

[Download on Nexus](https://www.nexusmods.com/mimicparty/mods/1) · [GitHub downloads](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/releases/latest) · [Report an issue](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/issues)

## Requirements

- [BepInEx Pack for Mimic Party](https://www.nexusmods.com/mimicparty/mods/3) **1.0.0**, Windows x64 IL2CPP.
- [Mimic Party Modding Core](https://www.nexusmods.com/mimicparty/mods/2) **1.0.0**.
- The game/loader combination listed in [Compatibility](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/blob/main/COMPATIBILITY.md).

**The current main download is Expansion only.** Install both requirements separately. It does not include BepInEx, the Core, or the bootstrap. The older **Complete** download already contains Core/Bootstrap and needs the Pack, not another Core copy.

## Installation

1. Close Mimic Party. Migrating from the old v1.0.x on-disk patch? Restore the original game files with its uninstaller or Steam file verification before using this runtime release.
2. Extract the BepInEx Pack into the folder containing `Mimic Party.exe`.
3. Extract the Core runtime ZIP into that same folder.
4. Extract this Expansion ZIP there. It adds `BepInEx/plugins/MimicParty10PlayerExpansion.dll`.
5. Start normally through Steam. Initial interop generation can take longer. Create a private lobby.

Use matching versions across the group. Do not run old private repair installers, combine an on-disk patch with this release, or keep duplicate DLLs under other filenames/subfolders. A working 1.1.2 installation does not need its interop cache regenerated for this documentation/package revision.

## Features and settings

Classic and Versus share the configured **total** capacity, default 10. After a successful start, close the game and edit `BepInEx/config/com.arribbaa.mimicparty.10playerexpansion.cfg`:

```ini
[Multiplayer]
MaxPlayers = 10
```

Allowed range: **6–10**. Values outside it are clamped. Restart after changing the setting. In the documented build, the mode cards use the updated shared capacity; the Classic lobby shows 1/10 when alone. Versus still requires at least two players to start.

The Expansion also installs hooks intended to suppress live voice during playback and retain the lobby during rematch. It does not replace Workshop sound packs. Complete large-group audio/rematch behaviour remains outside the confirmed scope; see Compatibility.

## Removal and troubleshooting

Close the game and remove only `BepInEx/plugins/MimicParty10PlayerExpansion.dll`. Its config can be removed separately. Keep Core/Pack if other mods use them. Native capacity changes are in memory and disappear when the process exits.

Missing Core dependency: install the Core runtime, not the Developer Starter. Unrecognized game version or loader failure: do not force the patch; check `BepInEx/LogOutput.log`. Do not disable security software as an installation step.

**R3 changes packaging and documentation only; Expansion runtime bytes remain 1.1.2.** Documentation uses its own `MimicParty10PlayerExpansion/` directory.

[Compatibility](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/blob/main/COMPATIBILITY.md) · [Changelog](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/blob/main/CHANGELOG.md) · [Build instructions](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/blob/main/BUILDING.md) · [License](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/blob/main/LICENSE.txt) · [Security](https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion/blob/main/SECURITY.md)
