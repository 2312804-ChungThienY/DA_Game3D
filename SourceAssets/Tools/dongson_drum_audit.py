"""Read-only delivery checks for the 2026-11-06 drum art replacement."""
import hashlib
import json
import pathlib
import re
import zipfile

ROOT = pathlib.Path(__file__).resolve().parents[2]
checks, failures = [], []


def check(ok, label):
    (checks if ok else failures).append(label)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest() if path.exists() else "absent"


baseline = json.loads((ROOT / "Logs/DongSonDrums-baseline.json").read_text())
changed_scenes = {
    "Assets/_Project/Scenes/02_HangLinhMach.unity",
    "Assets/_Project/Scenes/92_EnvironmentPreview.unity",
}
for name, digest in baseline.items():
    if name not in changed_scenes:
        check(sha(ROOT / name) == digest, "Unchanged bytes: " + name)

backup = ROOT / "SourceAssets/SceneBackups/2026-11-06_DongSonDrums"
prefab = ROOT / "Assets/_Project/Prefabs/Interactables/PF_DrumPuzzle_01.prefab"
guid = re.search(r"guid: (\w+)", prefab.with_suffix(".prefab.meta").read_text()).group(1)


def overrides(text):
    block = next(b for b in re.split(r"(?=--- !u!)", text)
                 if "m_SourcePrefab:" in b and "guid: " + guid in b)
    # Model children are intentionally replaced. Every serialized existing gameplay
    # override (collider, root pose, references, puzzle state etc.) must remain exact.
    return block.split("    m_Modifications:\n", 1)[1].split("    m_RemovedComponents:", 1)[0]


for name in sorted(changed_scenes):
    original = backup / pathlib.Path(name).name
    check(sha(original) == baseline[name], "Original scene backup matches baseline: " + name)
    check(original.with_suffix(".unity.meta").read_bytes() == (ROOT / (name + ".meta")).read_bytes(),
          "Scene GUID metadata preserved: " + name)
    check(overrides(original.read_text(encoding="utf-8")) == overrides((ROOT / name).read_text(encoding="utf-8")),
          "All existing drum-puzzle prefab overrides preserved: " + name)

source = json.loads((ROOT / "Logs/DongSonDrum-source.json").read_text())
archive_path = pathlib.Path(source["zip"])
check(sha(archive_path) == source["sha256"], "Supplied ZIP unchanged")
art = ROOT / "Assets/_Project/Art/DongSonDrum"
with zipfile.ZipFile(archive_path) as archive:
    for entry in archive.infolist():
        check((art / "Source" / entry.filename).read_bytes() == archive.read(entry),
              "Original source bytes retained: " + entry.filename)

assets = [art, *art.rglob("*")]
assets = [p for p in assets if p.suffix != ".meta"]
assets += [ROOT / "Assets/_Project/Editor/DongSonDrumSetup.cs",
           ROOT / "Assets/_Project/Tests/DongSonDrumProbe.cs"]
for path in assets:
    meta = pathlib.Path(str(path) + ".meta")
    check(meta.exists() and bool(re.search(r"guid: [0-9a-f]{32}", meta.read_text())),
          "Unity metadata valid: " + str(path.relative_to(ROOT)))

manifest = json.loads((ROOT / "Logs/DongSonDrumSaveRecovery/manifest.json").read_text(encoding="utf-8-sig"))
for path, expected in zip(manifest["paths"], manifest["hashes"]):
    actual = "absent" if not pathlib.Path(path).exists() else "-".join(
        f"{x:02X}" for x in hashlib.sha256(pathlib.Path(path).read_bytes()).digest())
    check(actual == expected, "Personal save restored: " + pathlib.Path(path).name)

for report_file in ["Logs/DongSonDrums-apply.json", "docs/reports/DongSonDrums_PlayMode_2026-11-06.json"]:
    report = json.loads((ROOT / report_file).read_text(encoding="utf-8-sig"))
    check(not report.get("failures") and not report.get("errors"), "Passed: " + report_file)

out = ROOT / "docs/reports/DongSonDrums_Delivery_2026-11-06.json"
out.write_text(json.dumps({"checks": checks, "failures": failures}, indent=2), encoding="utf-8")
print(json.dumps({"checks": len(checks), "failures": failures}, indent=2))
raise SystemExit(bool(failures))
