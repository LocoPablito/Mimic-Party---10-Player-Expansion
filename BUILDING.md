# Building Mimic Party - 10 Player Expansion

by arribbaa

## Requirements

- .NET 8 SDK or newer
- Mimic Party Modding Core source
- Internet access for NuGet restore

The expansion targets BepInEx 6 Unity IL2CPP build #788 (`6.0.0-be.788`).

## Recommended checkout layout

```text
parent/
├─ MimicParty-Modding/
└─ Mimic-Party---10-Player-Expansion/
```

`MimicParty-Modding` is the Core repository:
https://github.com/LocoPablito/MimicParty-Modding

## Build

From this repository:

```powershell
./build.ps1
```

If the Core is not cloned next to this repository, set:

```powershell
$env:MIMIC_PARTY_CORE_PROJECT = "C:\path\to\MimicParty-Modding\src\MimicParty.ModdingCore\MimicParty.ModdingCore.csproj"
./build.ps1
```

The script restores, builds and creates a Nexus-ready archive in `dist/`.

## Output

```text
dist/
├─ MimicParty_10_Player_Expansion_v1.1.0_by_arribbaa_NEXUS.zip
└─ SHA256SUMS.txt
```

The public archive contains only:

```text
BepInEx/plugins/MimicParty10PlayerExpansion.dll
README.txt
CHANGELOG.txt
LICENSE.txt
```

It does not bundle BepInEx, Mimic Party Modding Core, HarmonyX, or original Mimic Party binaries.
