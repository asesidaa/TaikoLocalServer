# 07 - IDA Slot Mapping Enrichment

**Goal:** Use local IDA evidence to map Green `don3d` costume directories to the five WebUI costume slots, then enrich extractor output without changing server or WebUI APIs.

**Files:**
- Create: `.tools/green-customization-ida/slot_probe.py`
- Create: `docs/green-client-evidence/08-green-costume-slot-mapping.md`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Merging/CostumeSlotMap.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/Extractor/Merging/CostumeMerger.cs`
- Modify: `Tests/Green/GreenCustomizationExtractorTests.cs`

## Task 1: Collect IDA Evidence

- [ ] **Step 1: Create IDA probe script**

Create `.tools/green-customization-ida/slot_probe.py`:

```python
import argparse
import json
from pathlib import Path

from ida_cli.agent_bridge import AgentSession


NEEDLES = [
    "/data/don3d/full/cos",
    "/data/don3d/parts/head",
    "/data/don3d/parts/body",
    "/data/don3d/parts/paint",
    "/data/don3d/parts/acc",
    "don3d::CDonFileDataMan::RequestLoadingImpl",
    "don3d::CDonFileDataMan::RequestImmediatelyImpl",
]


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--db", default=".tools/ida-snap/EBOOT.ELF.codex.i64")
    parser.add_argument("--out", default=".tools/green-customization-ida/slot_probe.json")
    args = parser.parse_args()

    with AgentSession.start(args.db, require_ida=True) as ida:
        backend = ida.probe_backend(require_ida=True)
        hits = ida.result(
            f"""
needles = {json.dumps([needle.lower() for needle in NEEDLES])}
items = ai.strings()
hits = []
for item in items:
    text = item.get("string") or item.get("value") or item.get("text") or "" if isinstance(item, dict) else str(item)
    lower = text.lower()
    if any(needle in lower for needle in needles):
        ea = item.get("ea") or item.get("address") or item.get("addr")
        xrefs = ai.xrefs_to(ea) if ea is not None else []
        contexts = []
        for xref in xrefs[:12]:
            frm = xref.get("from") or xref.get("frm") if isinstance(xref, dict) else None
            if frm is None:
                contexts.append({{"xref": xref}})
                continue
            try:
                contexts.append({{"xref": xref, "context": ai.context_pack(frm, disasm_limit=48, include_decompile=True)}})
            except Exception as exc:
                contexts.append({{"xref": xref, "error": str(exc)}})
        hits.append({{"string": item, "xrefs": contexts}})
__result__ = hits
""",
            request_id="green.customization.slot_probe",
            timeout_s=300,
        )

    output = {
        "backend": backend,
        "needles": NEEDLES,
        "hits": hits,
    }
    out = Path(args.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(output, indent=2, ensure_ascii=False), encoding="utf-8")
    print(out)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
```

- [ ] **Step 2: Run IDA probe**

Run:

```powershell
python .tools/green-customization-ida/slot_probe.py --db .tools/ida-snap/EBOOT.ELF.codex.i64 --out .tools/green-customization-ida/slot_probe.json
```

Expected: exits 0, prints `.tools/green-customization-ida/slot_probe.json`, and the JSON backend contains:

```json
{
  "database_opened": true,
  "ida_available": true
}
```

- [ ] **Step 3: Write evidence note**

Create `docs/green-client-evidence/08-green-costume-slot-mapping.md`:

````markdown
# Green Costume Slot Mapping Evidence

Target: `.tools/ida-snap/EBOOT.ELF.codex.i64`

Tool run:

```powershell
python .tools/green-customization-ida/slot_probe.py --db .tools/ida-snap/EBOOT.ELF.codex.i64 --out .tools/green-customization-ida/slot_probe.json
```

Observed strings:

| Game path | Extractor slot |
|---|---|
| `/data/don3d/full/cos` | `kigurumi` |
| `/data/don3d/parts/head` | `head` |
| `/data/don3d/parts/body` | `body` |
| `/data/don3d/parts/paint` | `face` |
| `/data/don3d/parts/acc` | `puchi` |

Reasoning:

- The paths are referenced from the `don3d::CDonFileDataMan` loading path.
- The five paths correspond one-to-one with the server's five existing customization fields.
- `parts/paint` maps to the face/paint visual layer and `parts/acc` maps to the accessory/puchi layer used by existing WebUI vocabulary.

Artifact: `.tools/green-customization-ida/slot_probe.json`
````

## Task 2: Apply Slot Mapping to Extractor

- [ ] **Step 1: Add failing slot-map test**

Append this test to `Tests/Green/GreenCustomizationExtractorTests.cs`:

```csharp
[Fact]
public void CostumeMerger_UsesDon3dDirectorySlotMap()
{
    var ndp = new[]
    {
        new NdpEntry(7, "cos_name_007.nut", 0, 1)
    };
    var scan = new Don3dScanResult(
        FullCosModelPairIds: [],
        DirectoryIds: new Dictionary<string, IReadOnlyList<uint>>
        {
            ["don3d/parts/head"] = [7]
        });

    var items = CostumeMerger.Merge(ndp, scan, new GreenCatalogOverrides());

    var costume = Assert.Single(items);
    Assert.Equal("head", costume.CostumeType);
}
```

Add these usings if absent:

```csharp
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;
```

- [ ] **Step 2: Run slot-map test and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~CostumeMerger_UsesDon3dDirectorySlotMap"
```

Expected: FAIL because all non-overridden costumes are currently `"unknown"`.

- [ ] **Step 3: Create slot map**

Create `Infrastructure/GameDataCatalog/Green/Extractor/Merging/CostumeSlotMap.cs`:

```csharp
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

internal static class CostumeSlotMap
{
    public static readonly IReadOnlyDictionary<string, string> DirectoryToCostumeType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["don3d/full/cos"] = "kigurumi",
            ["don3d/parts/head"] = "head",
            ["don3d/parts/body"] = "body",
            ["don3d/parts/paint"] = "face",
            ["don3d/parts/acc"] = "puchi"
        };

    public static IReadOnlyDictionary<uint, string> BuildIdMap(Don3dScanResult scan)
    {
        var result = new Dictionary<uint, string>();

        foreach (var pair in scan.DirectoryIds)
        {
            if (!DirectoryToCostumeType.TryGetValue(pair.Key, out var costumeType))
            {
                continue;
            }

            foreach (var id in pair.Value)
            {
                result.TryAdd(id, costumeType);
            }
        }

        return result;
    }
}
```

- [ ] **Step 4: Use slot map in `CostumeMerger`**

Modify `Infrastructure/GameDataCatalog/Green/Extractor/Merging/CostumeMerger.cs`:

Replace:

```csharp
var don3dIds = don3d.DirectoryIds.Values.SelectMany(ids => ids).ToHashSet();
```

With:

```csharp
var don3dIds = don3d.DirectoryIds.Values.SelectMany(ids => ids).ToHashSet();
var idToCostumeType = CostumeSlotMap.BuildIdMap(don3d);
```

Replace the `CostumeType = ...` initializer with:

```csharp
CostumeType = ResolveCostumeType(entry.Id, itemOverride, idToCostumeType),
```

Add this helper inside `CostumeMerger`:

```csharp
private static string ResolveCostumeType(
    uint id,
    GreenCostumeOverride? itemOverride,
    IReadOnlyDictionary<uint, string> idToCostumeType)
{
    if (!string.IsNullOrWhiteSpace(itemOverride?.CostumeType))
    {
        return itemOverride.CostumeType;
    }

    return idToCostumeType.TryGetValue(id, out var costumeType)
        ? costumeType
        : "unknown";
}
```

- [ ] **Step 5: Run extractor tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationExtractorTests"
```

Expected: PASS.

- [ ] **Step 6: Smoke extractor output with slot mapping**

Run:

```powershell
dotnet run --project GreenCatalogExtractor -- extract --game-data Host/wwwroot/data/green/data --out artifacts/green-customization-slot-smoke
```

Expected: `artifacts/green-customization-slot-smoke/green_costume_data.json` contains at least one of these values:

```json
"costumeType": "head"
```

- [ ] **Step 7: Build**

Run:

```powershell
dotnet build
```

Expected: PASS.

- [ ] **Step 8: Commit**

`.tools/green-customization-ida/slot_probe.py` and `.tools/green-customization-ida/slot_probe.json` remain local ignored artifacts. Commit the durable evidence note and extractor changes:

```powershell
git add -- docs/green-client-evidence/08-green-costume-slot-mapping.md Infrastructure/GameDataCatalog/Green/Extractor/Merging/CostumeSlotMap.cs Infrastructure/GameDataCatalog/Green/Extractor/Merging/CostumeMerger.cs Tests/Green/GreenCustomizationExtractorTests.cs
git commit -m "Enrich Green costume slot mapping from IDA evidence"
```
