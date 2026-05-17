# Green Costume Slot Mapping Evidence

Target DB: `.tools/ida-snap/EBOOT.ELF.codex.i64`

Command run:

```powershell
python .tools/green-customization-ida/slot_probe.py --db .tools/ida-snap/EBOOT.ELF.codex.i64 --out .tools/green-customization-ida/slot_probe.json
```

Backend result: `database_opened=true`, `ida_available=true`, backend `idalib`.

Observed mapping:

| Game path | Extractor slot |
|---|---|
| `/data/don3d/full/cos` | `kigurumi` |
| `/data/don3d/parts/head` | `head` |
| `/data/don3d/parts/body` | `body` |
| `/data/don3d/parts/paint` | `face` |
| `/data/don3d/parts/acc` | `puchi` |

IDA evidence:

- The probe found both `don3d::CDonFileDataMan::RequestLoadingImpl` and `don3d::CDonFileDataMan::RequestImmediatelyImpl` strings in the local Green EBOOT database.
- The five `don3d` path strings are present in the same database and are loaded through an indexed five-entry table in `sub_1EA90`.
- The decompiled table initializer pairs indices `0` through `4` with `/data/don3d/full/cos`, `/data/don3d/parts/head`, `/data/don3d/parts/body`, `/data/don3d/parts/paint`, and `/data/don3d/parts/acc`, respectively.
- The request method decompilation shows `CDonFileDataMan` loading uses a `CollectCostumeID`, which ties the directory table to costume asset loading.

Server vocabulary reasoning:

- The server and WebUI already expose five costume fields: `kigurumi`, `head`, `body`, `face`, and `puchi`.
- `full/cos`, `parts/head`, and `parts/body` map directly by name and asset layer.
- `parts/paint` is the face/paint layer, so the extractor emits the existing `face` slot name.
- `parts/acc` is the accessory layer used by the Green client and maps to the existing `puchi` slot name in server vocabulary.

Local artifact: `.tools/green-customization-ida/slot_probe.json`
