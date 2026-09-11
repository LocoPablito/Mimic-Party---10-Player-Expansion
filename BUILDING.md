# Building

Use the .NET 8 SDK to build the net6.0 plugin. Clone the Core source next to this repository or set MIMIC_PARTY_CORE_PROJECT to its csproj.

```powershell
./build.ps1
```

Core source revision used for compilation: 46c03672bab67ed32a060198ec8f2d56d3fdfd6a.

Publication uses the existing runtime DLL from the pinned release asset in release.json, not the newly compiled output. Its SHA-256 must be 245aa2148807a56b48105fef49352c628830e08097cf80d22848adf9e5627a8c. Run `python scripts/package_release.py` to assemble the R3 archive and hashes.

The runtime source is preserved; R3 changes distribution/docs only. Private regression fixtures and game-derived captures are not current source-tree or release dependencies.
