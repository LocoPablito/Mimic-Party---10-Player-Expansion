# Mimic Party — 10 Player Expansion 1.1.2

Up to **10 total players** in Classic and Versus, with free RED/BLUE selection and no forced equal-team split. Includes runtime capacity checks, playback voice hooks and rematch keep-lobby hooks.

**Recommended download: COMPLETE — Core included. BepInEx is NOT included.** Install the official BepInEx 6 Unity IL2CPP Windows x64 build 788, then extract Complete into the game folder before launching. ONLY is for users who already installed the Core package and its bootstrap.

The shipped Core and Expansion DLLs are identical to the ones verified in the Windows runtime test: all 13 loader/hook/capacity checks passed, and Classic displays 1/10. An older release was reported tested with seven people; a full ten-client test of this release is not claimed.

The Complete package's separate bootstrap automates the known generated-assembly compatibility repair without distributing Unity/game binaries. Its regression and supplied-file load checks pass; the earlier working setup does not establish a fresh-game launch test of this new automatic component. See VALIDATION.md.

Restore original game files before migrating from v1.0.x on-disk patches. Do not rerun old private FIX installers. No EXE/script installer, private logs, game assemblies or nested archives are included.
