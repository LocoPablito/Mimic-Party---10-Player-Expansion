# Pre-release checklist — 10 Player Expansion

Do not make v1.1.0 the Primary Nexus file until all runtime items pass.

## Build / packaging

- [x] Expansion source is separated from Mimic Party Modding Core.
- [x] Core is a separate hard dependency.
- [x] BepInEx compile dependency pinned to 6.0.0-be.788.
- [x] Public ZIP installs from archive root as `BepInEx/plugins/...`.
- [x] Public ZIP contains no BAT/CMD/PowerShell/EXE/nested archives/original game binaries.
- [x] Author attribution is `arribbaa`.
- [x] GitHub Actions builds the standalone Expansion repo against the Core repo.

## Runtime validation

Use BepInEx 6 Unity IL2CPP Windows x64 build #788.

- [ ] Restore original Mimic Party files before migrating from v1.0.x.
- [ ] BepInEx starts Mimic Party successfully.
- [ ] Mimic Party Modding Core v1.0.0 loads without errors.
- [ ] Expansion v1.1.0 loads without errors.
- [ ] Lobby shows `1/10` with default config.
- [ ] Player 6 can join and remain connected.
- [ ] 6-10 player full round passes.
- [ ] Normal lobby/reaction voice remains functional.
- [ ] Live voice is isolated during the active performance/playback window.
- [ ] Live voice returns immediately afterward.
- [ ] Workshop packs remain functional.
- [ ] Takes/playback work beyond slot 5.
- [ ] Scores/wheel work beyond slot 5.
- [ ] Rematch retains connected players.
- [ ] Second match starts and completes at least one round.
- [ ] Genuine disconnect still removes the player normally.
- [ ] Game exits cleanly and no permanent `GameAssembly.dll` modification remains.

Only after all runtime items pass should v1.1.0 replace v1.0.x as the Primary Nexus file.
