# Mimic Party - 10 Player Expansion

**Version 1.1.0 — by arribbaa**

Runtime 10-player expansion for Mimic Party.

This repository contains only the **10 Player Expansion**. The shared runtime foundation lives separately in **Mimic Party Modding Core**:

https://github.com/LocoPablito/MimicParty-Modding

## Architecture

```text
BepInEx 6 Unity IL2CPP x64
        ↓
Mimic Party Modding Core v1.0.0+
        ↓
Mimic Party - 10 Player Expansion v1.1.0
```

## Features

- Up to 10 players in private lobbies.
- Host capacity support for players 6-10.
- Configurable lobby size from 6 to 10 players.
- Runtime signature resolution instead of permanent GameAssembly.dll modification.
- Live-voice isolation during the active playback/performance window.
- Live voice returns after the performance window.
- Connected players remain together when Rematch starts.
- Genuine disconnect handling remains stock.
- Workshop sound packs remain handled by Mimic Party normally.

## Requirements

- Mimic Party for Windows / Steam.
- BepInEx 6 Unity IL2CPP x64 build #788 (`6.0.0-be.788`).
- Mimic Party Modding Core v1.0.0 or newer.

Official BepInEx builds:
https://builds.bepinex.dev/projects/bepinex_be

## Important migration from v1.0.x

The old v1.0.x release permanently patched `GameAssembly.dll` until restored.

Before using v1.1.0:

1. Close Mimic Party.
2. Restore the original game file using the old v1.0.x uninstaller if needed.
3. Verify Mimic Party through Steam.
4. Install BepInEx and Mimic Party Modding Core.
5. Install this expansion.

Do not run v1.1.0 on top of a v1.0.x-patched `GameAssembly.dll`.

## Installation

1. Install BepInEx 6 Unity IL2CPP x64 and start Mimic Party once.
2. Install Mimic Party Modding Core v1.0.0+.
3. Extract the release archive into the Mimic Party game folder.
4. Confirm `BepInEx/plugins/MimicParty10PlayerExpansion.dll` exists.
5. Start Mimic Party normally through Steam.

## Configuration

After the first successful run:

`BepInEx/config/com.arribbaa.mimicparty.10playerexpansion.cfg`

`MaxPlayers` supports **6-10**. Default: **10**.

## Source / author

Repository: https://github.com/LocoPablito/Mimic-Party---10-Player-Expansion

Core: https://github.com/LocoPablito/MimicParty-Modding

Author: **arribbaa**

## Release status

Build/static validation is complete. Runtime multiplayer validation remains required before v1.1.0 replaces the current v1.0.x Nexus release as Primary.
