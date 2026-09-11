# Build and release provenance

The runtime target is net6.0. Build with the .NET 8 SDK and the Core source beside this repository, or pass the MimicPartyCoreProject property.

```powershell
./build.ps1
dotnet run --project tests/HookContractTests -c Release -f net8.0
```

Public release packaging deliberately preserves the Expansion DLL from the verified CI artifact: run `34623888111`, source revision `f52cf5c7d4b0b1da6e06cc1483eee15e92c34d95`. Later changes before publication update only tests/docs/packaging. A new compile can change assembly metadata and hashes despite unchanged runtime source; do not substitute it silently.

The Complete release embeds the author's Core 1.0.0 package components, with hashes verified and recorded in PROVENANCE.json. The source code is inspection-available under LICENSE.txt; it is not licensed as an unrestricted third-party reupload.

The release pipeline produces only distributable DLLs and documentation. Old private repair scripts and diagnostic tools are not part of the current public tree. Historical Git commits can still contain older material; removing current-tree files is not a history-erasure operation.
