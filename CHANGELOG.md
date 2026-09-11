# Changelog

## 1.1.0

- Migrated from on-disk GameAssembly patching to BepInEx IL2CPP runtime patching.
- Added hard dependency on Mimic Party Modding Core 1.0.0+.
- Replaced absolute binary offsets with structural runtime signature resolution.
- Moved MaxPlayers, voice flow and rematch behavior to runtime method hooks.
- Added configurable player count from 6 to 10.
- Added transactional capacity patch validation and rollback.
