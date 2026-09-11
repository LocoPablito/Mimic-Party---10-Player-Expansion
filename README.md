# Mimic Party - 10 Player Expansion

**Version 1.1.1 — by arribbaa**

Runtime 10-player expansion for Mimic Party.

This repository contains only the **10 Player Expansion**. The shared runtime foundation lives separately in **Mimic Party Modding Core**:

https://github.com/LocoPablito/MimicParty-Modding

## Architecture

```text
BepInEx 6 Unity IL2CPP x64
        ↓
Mimic Party Modding Core v1.0.0+
        ↓
Mimic Party - 10 Player Expansion v1.1.1
```

## Features

- Up to 10 total players in private **Classic** lobbies.
- Up to 10 total players in private **Versus** lobbies.
- Versus team distribution is not forced by the mod: players can switch between RED and BLUE themselves, including uneven splits such as 4/6, as long as total occupancy does not exceed the configured room cap.
- Host capacity support for players 6-10.
- Configurable lobby size from 6 to 10 players.
- Runtime signature resolution instead of permanent GameAssembly.dll modification.
- Live-voice isolation during the active playback/performance window.
- Live voice returns after the performance window.
- Connected players remain together when Rematch starts.
- Genuine disconnect handling remains stock.
- Workshop sound packs remain handled by Mimic Party normally.
- The verified 11 Sep 2026 build includes an in-process native capacity self-check so the patched shared Classic/Versus helper can be validated without filling a 10-player lobby.

## Requirements

- Mimic Party for Windows / Steam.
- BepInEx 6 Unity IL2CPP x64 build #788 (`6.0.0-be.788`).
- Mimic Party Modding Core v1.0.0 or newer.

Official BepInEx builds:
https://builds.bepinex.dev/projects/bepinex_be

## Important migration from v1.0.x

The old v1.0.x release permanently patched `GameAssembly.dll` until restored.

Before using v1.1.1:

1. Close Mimic Party.
2. Restore the original game file using the old v1.0.x uninstaller if needed.
3. Verify Mimic Party through Steam.
4. Install BepInEx and Mimic Party Modding Core.
5. Install this expansion.

Do not run v1.1.1 on top of a v1.0.x-patched `GameAssembly.dll`.

## Installation

The 11 Sep 2026 Unity 6000.4.2f1 game build currently needs an additional BepInEx bootstrap compatibility step before plugins can load. The private validation package automates that step. Do not promote v1.1.1 to the public Nexus release until that path is runtime-verified.

After BepInEx is bootstrapped successfully:

1. Install Mimic Party Modding Core v1.0.0+.
2. Extract the release archive into the Mimic Party game folder.
3. Confirm `BepInEx/plugins/MimicParty10PlayerExpansion.dll` exists.
4. Start Mimic Party normally through Steam.

## Configuration

After the first successful run:

`BepInEx/config/com.arribbaa.mimicparty.10playerexpansion.cfg`

`MaxPlayers` supports **6-10**. Default: **10**.

The value is a **total room cap** for both Classic and Versus. It does not impose a RED/BLUE team ratio.

## Source / author

Repository: https://github.com/LocoPablito/Mimic-Party-10-Player-Expansion

Core: https://github.com/LocoPablito/MimicParty-Modding

Author: **arribbaa**

## Release status

Build/static validation is complete. Private runtime validation of the Unity 6000.4.2f1 BepInEx bootstrap and multiplayer capacity remains required before v1.1.1 replaces the current Nexus release as Primary.
