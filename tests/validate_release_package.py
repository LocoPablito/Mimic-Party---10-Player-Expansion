from __future__ import annotations

from pathlib import Path
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parents[1]
DIST = ROOT / "dist"
PROJECT = ROOT / "src" / "MimicParty.TenPlayerExpansion" / "MimicParty.TenPlayerExpansion.csproj"

project_xml = ET.parse(PROJECT)
version = project_xml.findtext(".//Version")
if not version:
    raise SystemExit(f"Project version not found: {PROJECT}")

PACKAGE = DIST / f"MimicParty_10_Player_Expansion_v{version}_by_arribbaa_NEXUS.zip"

EXPECTED = {
    "BepInEx/plugins/MimicParty10PlayerExpansion.dll",
    "README.txt",
    "CHANGELOG.txt",
    "LICENSE.txt",
}

FORBIDDEN_SUFFIXES = {".exe", ".bat", ".cmd", ".ps1", ".msi", ".rar", ".7z"}
FORBIDDEN_GAME_FILES = {"gameassembly.dll", "global-metadata.dat", "unityplayer.dll"}

if not PACKAGE.exists():
    raise SystemExit(f"Package not found: {PACKAGE}")

with zipfile.ZipFile(PACKAGE, "r") as zf:
    files = {name.replace("\\", "/") for name in zf.namelist() if not name.endswith("/")}

    if files != EXPECTED:
        raise AssertionError(f"Unexpected package layout. Expected {sorted(EXPECTED)}, got {sorted(files)}")

    for name in files:
        p = Path(name)
        if p.suffix.lower() in FORBIDDEN_SUFFIXES:
            raise AssertionError(f"Forbidden executable/script/archive in public package: {name}")
        if p.name.lower() in FORBIDDEN_GAME_FILES:
            raise AssertionError(f"Original game binary must not be distributed: {name}")

    dll = zf.read("BepInEx/plugins/MimicParty10PlayerExpansion.dll")
    if b"arribbaa" not in dll.lower():
        raise AssertionError("Author attribution not found in expansion DLL metadata")

print(f"Expansion release package validation: PASS (v{version})")
