# Validation scope — 1.1.2

## Confirmed Windows game session

The recorded game session passed 13 automatic checks: chainloader complete; Core loaded; Expansion version; rematch signature; hooks installed; Expansion active; native-helper values; native-helper self-check; no logged errors; native game file unchanged; repaired interop preserved; Core preserved; Expansion hash matched.

The native helper returned 10 for selectors 0, 1 and 2. Screenshots show a Classic lobby at 1/10 and both mode selection cards at Up to 10 players. Versus was entered with one player; its minimum-player message does not display maximum occupancy.

Verified Expansion DLL SHA-256:
`245aa2148807a56b48105fef49352c628830e08097cf80d22848adf9e5627a8c`

The author reports an earlier seven-person test of a previous release. It is historical evidence, not a current ten-client certification.

## Not established by the above

No complete ten-client network/gameplay session, all possible team splits, or audio/rematch behaviour across a complete ten-player match is claimed. Hook installation is not end-to-end feature testing.

The Complete package adds the Core distribution's new Interop Bootstrap. Its 23 Windows regression checks and six private supplied-assembly metadata/load checks are separate from the prior game session. A fresh Windows game launch with that automatic bootstrap remains unrecorded. Existing valid generated assemblies are intended to be preserved.

Private diagnostic logs and game-derived DLLs are excluded from public downloads and the current source tree. Useful synthetic regression tests remain available for inspection.
