# Changelog

## 1.1.1

- Added verified support logic for the 11 Sep 2026 GameAssembly build.
- Updated the new shared mode-capacity helper so the configured MaxPlayers applies to both Classic and Versus.
- With MaxPlayers=10, Classic targets 10 total players and Versus targets 10 total players for 5v5 validation.
- Updated package generation so the archive filename follows the project version automatically.

## 1.1.0

- Migrated from on-disk GameAssembly patching to BepInEx IL2CPP runtime patching.
- Added hard dependency on Mimic Party Modding Core 1.0.0+.
- Replaced absolute binary offsets with structural runtime signature resolution.
- Moved MaxPlayers, voice flow and rematch behavior to runtime method hooks.
- Added configurable player count from 6 to 10.
- Added transactional capacity patch validation and rollback.
