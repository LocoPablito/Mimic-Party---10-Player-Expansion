"""Build plain public archives from the exact verified runtime binaries."""
from pathlib import Path
import hashlib
import json
import zipfile

ROOT = Path(__file__).resolve().parents[1]
sha = lambda data: hashlib.sha256(data).hexdigest()
expected_exp = '245aa2148807a56b48105fef49352c628830e08097cf80d22848adf9e5627a8c'
expected_core = '4ebd390eb5a587999a34919cea2309d7d81bffd992816971c54e07553727b87d'
expected_boot = '5e8408516988c28bd5e0f54d859877dc0939ce5e0a0d6bbae2e3b33c33e82f01'
found = []
for path in (ROOT / '_release-input/expansion').rglob('*.zip'):
    with zipfile.ZipFile(path) as z:
        for name in z.namelist():
            if name.replace('\\','/').split('/')[-1] == 'MimicParty10PlayerExpansion.dll': found.append(z.read(name))
if len(found) != 1: raise RuntimeError('Expected exactly one pinned Expansion DLL')
expansion = found[0]
assert sha(expansion) == expected_exp
with zipfile.ZipFile(ROOT / '_release-input/core/MimicParty_Modding_Core_v1.0.0.zip') as z:
    core = z.read('BepInEx/plugins/MimicPartyModdingCore.dll')
    boot = z.read('BepInEx/patchers/MimicParty.InteropBootstrap.dll')
    core_license = z.read('LICENSE.txt')
    core_provenance = json.loads(z.read('PROVENANCE.json'))
assert sha(core) == expected_core and sha(boot) == expected_boot
base = {'BepInEx/plugins/MimicParty10PlayerExpansion.dll': expansion}
for name in ['README.md','CHANGELOG.md','LICENSE.txt','SECURITY.md','SUPPORTED_BUILD.md','VALIDATION.md']:
    base[name] = (ROOT / name).read_bytes()
provenance = {
    'Package': 'Mimic Party - 10 Player Expansion', 'Version': '1.1.2', 'Author': 'arribbaa',
    'Expansion': {'SHA256': expected_exp, 'SourceCommit': 'f52cf5c7d4b0b1da6e06cc1483eee15e92c34d95', 'WorkflowRun': 34623888111},
    'CoreDistribution': core_provenance,
    'WindowsLoaderAndCapacityVerified': True, 'FullTenClientGameplayVerified': False,
    'BepInExIncluded': False, 'GameBinariesIncluded': False,
}
out = ROOT / 'dist'; out.mkdir(exist_ok=True)
outputs = []
for variant in ['COMPLETE','ONLY']:
    files = dict(base)
    if variant == 'COMPLETE':
        files['BepInEx/plugins/MimicPartyModdingCore.dll'] = core
        files['BepInEx/patchers/MimicParty.InteropBootstrap.dll'] = boot
        files['CORE_LICENSE.txt'] = core_license
    p = dict(provenance); p['Variant'] = variant; p['CoreIncluded'] = variant == 'COMPLETE'
    files['PROVENANCE.json'] = (json.dumps(p,indent=2)+'\n').encode()
    files['SHA256SUMS.txt'] = ''.join(f'{sha(b)}  {n}\n' for n,b in sorted(files.items())).encode()
    archive = out / f'MimicParty_10_Player_Expansion_v1.1.2_{variant}.zip'
    with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for name,data in sorted(files.items()):
            info = zipfile.ZipInfo(name,(2026,9,11,0,0,0)); info.compress_type=zipfile.ZIP_DEFLATED; info.external_attr=0o100644<<16
            z.writestr(info,data)
    with zipfile.ZipFile(archive) as z:
        assert z.testzip() is None and set(z.namelist()) == set(files)
        assert not any(Path(n).suffix.lower() in {'.exe','.bat','.cmd','.ps1','.zip','.7z','.rar'} for n in z.namelist())
        assert not any(Path(n).name.lower() in {'gameassembly.dll','unityplayer.dll','unityengine.coremodule.dll','global-metadata.dat'} for n in z.namelist())
    outputs.append(f'{sha(archive.read_bytes())}  {archive.name}\n')
    print(f'Public package validation PASS: {archive.name}')
(out/'SHA256SUMS.txt').write_text(''.join(outputs),encoding='ascii')
