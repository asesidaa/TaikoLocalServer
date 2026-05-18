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

Observed id parsing:

| Client evidence | Extractor behavior |
|---|---|
| `sub_602708` compiles `^\w+_(\d+)000\.nut` before collecting costume ids. | `cos_001000.nut`, `head_001000.nut`, `body_001000.nut`, `paint_001000.nut`, and `acc_001000.nut` all contribute logical id `1`. |
| `sub_602708` parses the captured group as an integer and sets `bit[id]` in the target costume flag bitset. | Files such as `paint_012001.nut` and `cos_036001.nut` are variants, not separate protocol unlock ids. |
| `BAIDResponse.costume_flg_1..5` are five separate 32-byte bitsets. | The same logical id can exist in multiple slots, so extraction emits distinct `(costumeId, costumeType)` rows instead of one global `id -> type` row. |

IDA evidence:

- The probe found both `don3d::CDonFileDataMan::RequestLoadingImpl` and `don3d::CDonFileDataMan::RequestImmediatelyImpl` strings in the local Green EBOOT database.
- The five `don3d` path strings are present in the same database and are loaded through an indexed five-entry table in `sub_1EA90`.
- The decompiled table initializer pairs indices `0` through `4` with `/data/don3d/full/cos`, `/data/don3d/parts/head`, `/data/don3d/parts/body`, `/data/don3d/parts/paint`, and `/data/don3d/parts/acc`, respectively.
- `sub_602708` uses the Green costume regex above, extracts the captured decimal id, and sets the corresponding bit at `bitset[id >> 3] |= 1 << (id & 7)`.
- `sub_23468C` handles `BAIDResponse` and copies `ary_costumedata.costume_1..5` into player data offsets `0x84..0x94`, then mirrors them to current offsets `0x6c..0x7c`.
- `sub_22FF18` reads five separate costume flag bitsets from player data offsets `+240`, `+272`, `+304`, `+336`, and `+368`, then passes the discovered ids to `AssignCostume`, `AssignCosHead`, `AssignCosBody`, paint, and accessory flows.
- `sub_2A1D5C` loads render assets by slot: `costume_1` uses `cos` and `face`; split costume uses `head`, `body`, `paint`, and `acc`.
- `sub_2A0F14` / `sub_2A0558` select `full` for `cos` and `face`, `parts` for `head`, `body`, `paint`, and `acc`; `sub_29ECC0` formats paths as `%s/%s/%s_%06d%s`.

Server vocabulary reasoning:

- The server and WebUI already expose five costume fields: `kigurumi`, `head`, `body`, `face`, and `puchi`.
- `full/cos`, `parts/head`, and `parts/body` map directly by name and asset layer.
- `parts/paint` is the face/paint layer, so the extractor emits the existing `face` slot name.
- `parts/acc` is the accessory layer used by the Green client and maps to the existing `puchi` slot name in server vocabulary.

Local artifact: `.tools/green-customization-ida/slot_probe.json`
