# Changelog

## 1.1.2 — 11 September 2026

- Corrects rematch method resolution for `ResultsScreen.LaunchRematch(INetworkService)` in the documented game build.
- Adds explicit resolved-hook and installation logging.
- Preserves the shared Classic/Versus total-capacity patch and live native-helper verification.
- Documents free RED/BLUE selection with no forced 5v5 split.
- Records successful Windows loader/hook/capacity validation and the visible Classic 1/10 result.
- Public distribution now provides a Complete package with authorized Core/Bootstrap inclusion and an Expansion-only alternative. The shipped Expansion DLL is unchanged from the verified test.

## 1.1.1 — development revision

- Supports the shared mode-capacity helper introduced by the captured 11 September update.
- Makes the configured total apply to both Classic and Versus.
- Adds the in-process capacity self-check.

## 1.1.0 — runtime migration

- Moves from permanent GameAssembly patching to BepInEx IL2CPP runtime patches.
- Introduces the Modding Core dependency, structural signatures and transactional validation/rollback.
- Adds configurable capacity, voice-phase and rematch hooks.

## 1.0.x — legacy distribution

- Earlier on-disk patch implementation. The author reports a seven-person test of an older release.
- Restore original game files before migrating; do not run the old installer over the runtime-based release.

The separate Interop Bootstrap component is documented in the Core changelog. Build or helper checks do not claim a complete ten-client gameplay test.
