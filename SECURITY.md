# Security / file behaviour

The public release contains ordinary, non-obfuscated managed DLLs and documentation. No installer EXE, BAT/CMD/PowerShell script, nested/password-protected archive, private diagnostic capture or game/Unity binary is shipped.

The Expansion installs Harmony runtime hooks and patches a validated capacity helper in the current game's memory through the Core. It does not permanently write those changes to GameAssembly.dll. In-process native patching is part of its function, not proof of malware; nevertheless any detection deserves review rather than automatic dismissal.

The Core's separate bootstrap repairs a recognized malformed, locally generated CoreModule after build and structure checks, saves a backup, handles known obsolete compatibility copies and changes the BepInEx UnityLogListening setting. See the Core SECURITY.md for the exact file behaviour. This is distinct from native game modification.

These three author components have no telemetry, remote downloader or auto-updater and do not change Windows security settings. BepInEx can separately retrieve reference libraries on initial startup; the game uses its normal network services.

SHA-256 manifests identify the exact distributed bytes. Hashes, GitHub hosting and clean source are not guarantees of antivirus approval. Nexus performs its own security review. Do not delete/repack a quarantined file merely to conceal a detection or ask users to disable their antivirus. Provide the exact hash, source and behaviour description for review.

Report vulnerabilities privately to the author's Nexus account. Public bug reports should omit personal information and must not include game binaries or private support ZIPs.
