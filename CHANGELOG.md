# Changelog

## Packaging revision R2 — 11 September 2026

- Makes the main archive Expansion-only, with explicit Core and BepInEx Pack requirements.
- Connects all three Nexus pages and dedicated repositories.
- Places documentation in MimicParty10PlayerExpansion/ to prevent overwriting other guides.
- Keeps the verified Expansion 1.1.2 DLL byte-for-byte unchanged.
- Replaces obsolete private repair material with concise installation and compatibility documentation.

## Runtime 1.1.2

Corrects LaunchRematch(INetworkService) resolution, adds per-hook logging and preserves the shared Classic/Versus capacity helper with its live self-check.

## Runtime 1.1.x

Migrates the old on-disk approach to BepInEx runtime patches and the Core dependency. Includes configurable capacity and playback/rematch hooks. Versus team selection is not forced.

## Legacy 1.0.x

Older on-disk distribution. Restore original game files before migrating.
