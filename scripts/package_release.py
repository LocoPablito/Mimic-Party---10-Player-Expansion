"""Repackage pinned public binaries with component-specific documentation."""
from pathlib import Path, PurePosixPath
from urllib.request import Request, urlopen
import hashlib
import json
import os
import zipfile

ROOT = Path(__file__).resolve().parents[1]
CFG = json.loads((ROOT / 'release.json').read_text(encoding='utf-8'))
CACHE = Path(os.environ.get('MIMIC_PACKAGE_INPUTS', ROOT / '.inputs'))
OUT = ROOT / 'dist'
SHA = lambda b: hashlib.sha256(b).hexdigest()
JSON = lambda x: (json.dumps(x, indent=2, ensure_ascii=False) + '\n').encode('utf-8')


def read_archive(path):
    result = {}
    seen = set()
    with zipfile.ZipFile(path) as archive:
        if archive.testzip() is not None:
            raise ValueError('Corrupt input archive')
        for info in archive.infolist():
            if info.is_dir():
                continue
            name = info.filename.replace('\\', '/')
            p = PurePosixPath(name)
            if p.is_absolute() or '..' in p.parts or ':' in name:
                raise ValueError('Unsafe archive path: ' + name)
            if name.casefold() in seen:
                raise ValueError('Duplicate archive path: ' + name)
            seen.add(name.casefold())
            result[name] = archive.read(info)
    return result


def get_input(key):
    item = CFG['Inputs'][key]
    CACHE.mkdir(parents=True, exist_ok=True)
    path = CACHE / item['File']
    if not path.exists():
        req = Request(item['URL'], headers={'User-Agent': 'arribbaa-release-packager'})
        with urlopen(req, timeout=120) as response:
            data = response.read()
        if SHA(data) != item['SHA256']:
            raise ValueError('Downloaded input identity mismatch: ' + key)
        path.write_bytes(data)
    if SHA(path.read_bytes()) != item['SHA256']:
        raise ValueError('Input identity mismatch: ' + key)
    return read_archive(path)


def require_dll(files, name, key):
    data = files[name]
    if SHA(data) != CFG['BinarySHA256'][key]:
        raise ValueError('Runtime DLL identity mismatch: ' + key)
    return data


def manifest(files):
    return ''.join(f'{SHA(data)}  {name}\n' for name, data in sorted(files.items())).encode('utf-8')


def write_archive(filename, files, manifest_path):
    files = dict(files)
    files[manifest_path] = manifest(files)
    if len({n.casefold() for n in files}) != len(files):
        raise ValueError('Case-insensitive path collision')
    forbidden = {'gameassembly.dll', 'unityplayer.dll', 'unityengine.coremodule.dll', 'global-metadata.dat'}
    for name in files:
        p = PurePosixPath(name)
        if p.is_absolute() or '..' in p.parts or ':' in name or p.name.casefold() in forbidden:
            raise ValueError('Forbidden path: ' + name)
        if p.suffix.lower() in {'.ttf', '.otf', '.woff', '.woff2', '.ttc', '.eot', '.zip', '.rar', '.7z'}:
            raise ValueError('Unexpected nested archive or font: ' + name)
    OUT.mkdir(exist_ok=True)
    path = OUT / filename
    with zipfile.ZipFile(path, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
        for name, data in sorted(files.items()):
            info = zipfile.ZipInfo(name, (2026, 9, 11, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            info.external_attr = 0o100644 << 16
            archive.writestr(info, data)
    if read_archive(path) != files:
        raise ValueError('Written archive mismatch')
    print(filename + ' ' + SHA(path.read_bytes()))
    return path


def documents(prefix):
    return {prefix + name: (ROOT / name).read_bytes() for name in
            ['README.md', 'CHANGELOG.md', 'COMPATIBILITY.md', 'LICENSE.txt', 'SECURITY.md']}


role = CFG['Component']
version = CFG['Version']
revision = 'R' + str(CFG['Revision'])
provenance = {k: CFG[k] for k in ['Author', 'Component', 'Version', 'Revision', 'BinaryVersions', 'BinarySHA256']}
provenance['SourceInputs'] = CFG['Inputs']
provenance['RuntimeRecompiled'] = False
provenance['ReleaseRepository'] = CFG['Repositories'][role]
provenance['NexusPage'] = CFG['Nexus'][role]
provenance['FreshAutomaticInstallConfirmed'] = False
outputs = []

if role == 'core':
    prefix = 'MimicPartyModdingCore/'
    source = get_input('Core')
    name = 'BepInEx/plugins/MimicPartyModdingCore.dll'
    files = documents(prefix)
    files[name] = require_dll(source, name, 'Core')
    provenance['IncludesBootstrap'] = False
    provenance['Requirements'] = {'Pack': CFG['Nexus']['pack']}
    files[prefix + 'PROVENANCE.json'] = JSON(provenance)
    outputs.append(write_archive(f'MimicParty_Modding_Core_v{version}_{revision}.zip', files, prefix + 'SHA256SUMS.txt'))
    starter = {p.name: p.read_bytes() for p in (ROOT / 'examples/StarterMod').iterdir() if p.is_file()}
    outputs.append(write_archive(f'MimicParty_Core_Developer_Starter_v{version}_{revision}.zip', starter, 'SHA256SUMS.txt'))
elif role == 'expansion':
    prefix = 'MimicParty10PlayerExpansion/'
    source = get_input('Expansion')
    name = 'BepInEx/plugins/MimicParty10PlayerExpansion.dll'
    files = documents(prefix)
    files[name] = require_dll(source, name, 'Expansion')
    provenance['IncludesCore'] = False
    provenance['IncludesBootstrap'] = False
    provenance['Requirements'] = {'Core': CFG['Nexus']['core'], 'Pack': CFG['Nexus']['pack']}
    files[prefix + 'PROVENANCE.json'] = JSON(provenance)
    outputs.append(write_archive(f'MimicParty_10_Player_Expansion_v{version}_{revision}.zip', files, prefix + 'SHA256SUMS.txt'))
elif role == 'pack':
    prefix = 'BepInExPack/'
    files = get_input('Pack')
    sources = get_input('Sources')
    require_dll(files, 'BepInEx/patchers/MimicParty.InteropBootstrap.dll', 'Bootstrap')
    lines = files[prefix + 'UPSTREAM_SHA256SUMS.txt'].decode('utf-8').splitlines()
    if len(lines) != 228:
        raise ValueError('Unexpected upstream file count')
    for line in lines:
        digest, name = line.split('  ', 1)
        if SHA(files[name]) != digest:
            raise ValueError('Upstream byte mismatch: ' + name)
    files.pop(prefix + 'SHA256SUMS.txt', None)
    sources.pop('SHA256SUMS.txt', None)
    old_url = CFG['Repositories']['core'] + '/releases/tag/bepinex-pack-v1.0.0'
    new_url = CFG['Repositories']['pack'] + '/releases/tag/' + CFG['Tag']
    for data in [files, sources]:
        for name in list(data):
            if name.endswith(('SOURCE_ACCESS.txt', 'README_SOURCE_ACCESS.txt')):
                data[name] = data[name].decode('utf-8').replace(old_url, new_url).encode('utf-8')
    files.pop('README_BEPINEX_PACK.txt', None)
    files[prefix + 'README.md'] = (ROOT / 'README.md').read_bytes()
    files[prefix + 'CHANGELOG.txt'] = (ROOT / 'CHANGELOG.md').read_bytes()
    files[prefix + 'COMPATIBILITY.md'] = (ROOT / 'COMPATIBILITY.md').read_bytes()
    files[prefix + 'SECURITY.md'] = (ROOT / 'SECURITY.md').read_bytes()
    files[prefix + 'PACK_LICENSE.txt'] = (ROOT / 'LICENSE.txt').read_bytes()
    provenance['IncludesCore'] = False
    provenance['IncludesExpansion'] = False
    provenance['UpstreamFilesPreserved'] = 228
    provenance['Requirements'] = {}
    files[prefix + 'PROVENANCE.json'] = JSON(provenance)
    sources['README_R2.md'] = ('# Corresponding source\n\nPack by arribbaa. Source/download location: ' + new_url + '\n\nDo not install this archive in the game folder. It preserves the upstream source and license material from the pinned original source archive; revision R2 changes distribution links only.\n').encode()
    outputs.append(write_archive(f'BepInEx_Pack_for_Mimic_Party_v{version}_{revision}.zip', files, prefix + 'SHA256SUMS.txt'))
    outputs.append(write_archive(f'BepInEx_Pack_for_Mimic_Party_v{version}_{revision}_SOURCES.zip', sources, 'SHA256SUMS.txt'))
else:
    raise ValueError('Unknown component')

(OUT / 'SHA256SUMS.txt').write_bytes(''.join(f'{SHA(p.read_bytes())}  {p.name}\n' for p in outputs).encode('ascii'))
