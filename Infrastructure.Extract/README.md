# Infrastructure.Extract

Copies and converts the files TaikoLocalServer needs from a Taiko no Tatsujin Nijiiro install. Server Management › Games runs it inside the server (`ExtractRunner`); the same code runs from the command line as `TaikoLocalServer extract`, without starting the server:

```bash
TaikoLocalServer extract --game "G:\Games\Taiko no Tatsujin Nijiiro Version (SBWY 39.06)"
```

| Option | |
| --- | --- |
| `--game` | The game install folder, the one containing `Data/x64`. Passing `Data/x64` itself also works. `Data_mods` is read only with `--mods`. |
| `--out` | The server's `wwwroot` folder. Defaults to the `wwwroot` next to `TaikoLocalServer.exe`. |
| `--mods` | Layer the install's LayeredFS `Data_mods` folder over `Data`, as the game sees it with the plugin. A file in `Data_mods/x64/<path>` replaces `Data/x64/<path>` (fonts, models, UI), and folders gain the mods' extra entries. Datatables (e.g. new songs) come from `Data_mods/x64_enc/datatable`, the re-encrypted ones; the JSON in `Data_mods/x64/datatable` is ignored. Switching `--mods` on or off rebuilds every output once. |
| `--inputs` | Print the game files extract reads, as JSON, and exit (the WebUI's "Choose game folder" sends only these). |

It is safe to re-run, for example after a game update. Outputs that are already up to date (not older than the game's file; datatables must also match in size) are skipped. It exits with code `2` when a required file is missing or a texture fails to convert, and `1` on any other error. One bad texture doesn't stop the rest.

Each step prints `<label>: N written, M already up to date, X missing, Y failed`, then its `MISSING` and `FAILED` lines (the server reads these for progress). A run that gets to the end, failed steps or not, also writes what it did to `<out>/data/extract.json` (`era`, `finishedAt`, `source`, and each step's counts), replacing the last one. A run stopped by an error (exit `1`) writes none.

## What it does today

| From `Data/x64/` | To `<out>/` | |
| --- | --- | --- |
| `datatable/{musicinfo,music_order,wordlist,don_cos_reward,shougou,neiro}.bin` | `data/datatable/` | Copied as-is. The server decrypts them on startup. |
| `model/paint/*.nutexb` (58) | `game_data/paint/*.png` | Face paint, the `face` costume slot |
| `model/acce/*.nutexb` (149) | `game_data/acce/*.png` | Puchi accessories |
| `model/face/*.nutexb` (22) | `game_data/face/*.png` | Expression atlases: 12 layers stacked vertically |
| `model/{body,head,cos}/<name>/<name>/<name>.numdlb` (483) | `game_data/{body,head,cos}/<name>.glb` | Skinned don-chan parts, textures embedded |
| `animation/*.nuanmb` (54) | `game_data/animation/don.glb` + `facekeys.json` | One animation library (a clip per file) and the face-expression keys |
| `lumen/000_default/common/name_plate.{nulm,nutexb}` | `game_data/nameplate/plate_<label>.png`, `dan_<label>.png`, `plates.json` | Title plates and dan badges rendered from the game's UI. `plates.json` lists the labels in order, and a plate's index is its `TitlePlateId` |
| same | `game_data/nameplate/nameplate.scene.json` + `atlas.png` | The animated nameplate for the WebUI's Lumen player (`lumen-player.js`): plates, badges and the game's whole nameplate with its text boxes per layout |
| `lumen/000_default/{common/option, common_songselect/common_song_select_{song_board_icon,main_diff_board}, enso_dani/dani_select/dani_select, enso_ai/result/result_ai_section}.{nulm,nutexb}` | `game_data/ui/{option,difficulty,crown,rank,dani,ai}_<label>.png` + one index per source | The WebUI's icons: song options (doron, abekobe, kimagure, detarame, note_pos, speedx…), difficulties (easy … ura), crowns (`badge_crown_1…3`), score ranks (`badge_rank_1…7`), Dani Dojo pass stamps (`stamp_{red,gold}_01…03`), and AI Battle's win / lose emblems and section crowns (`ai_{win,lose}`, `ai_{none,crown_silver,crown_gold,crown_rainbow}`) |
| `lumen/000_default/enso_normal/enso/onpu/onp_all.{nulm,nutexb}` | `game_data/ui/don_note.png` | The don note, cropped without its caption, on a square: the WebUI's logo and favicon (served to everyone by `api/Brand/Icon`) |
| `font/{jp_30,en_30,tw_30,kr_30}.{xml,nutexb}` | `game_data/font/<sheet>/<page>.{png,json}` | The nameplate's glyph sheets (Lumen font slot 1, the 勘亭流 face) in 256-code-point pages, so name and title previews use the game's lettering |
| `sound/se_neiro_{00…19}_v12a.nus3bank` | `game_data/tone/NN_{don,ka}.wav` | The drum tones' don and ka hits, the bank's two embedded PCM WAVs cut out as-is, for the Profile's tone preview |
| `movie/<8-digit id>.wmv` | `game_data/movies.json` | The ids of the attract movies (not the videos), so Server Management › Cabinets offers them instead of asking for numbers |
| `lumen/000_default/common/player_board.{nulm,nutexb}` | `game_data/scoreboard/panel_<difficulty>.png`, `digit_ns_0N.png`, `scoreboard.json` | The Profile achievement panel ("scoreboard") per difficulty, plus the game's counter digit glyphs |

## Game UI art (`Lumen/Lumen.cs`, `Nijiiro/NijiiroUi.cs`)

The game's 2D UI, including the nameplates and the player board, uses Lumen files (`.nulm`, magic `LMB\0`), the Flash-like format the game's 2D UI uses, with its texture atlas in the matching `.nutexb`. `Lumen.cs` parses the shapes (textured or solid-colour triangles) and the sprite timelines. It renders a sprite at a frame label by playing its display list up to that frame: depth order, place/move/remove, matrix or position transforms, colour multiply/add, nested sprites at frame 0, and **clip masks** (a placement's clip depth hides deeper objects outside its shape). The tag layout is documented in `docs/webui-refactor-plan.md` §3d.

- Sprites are found **by frame labels or instance names**, never by id, because ids change between builds:
  - `wood` → title plates
  - `5kyuu_red` → dan badges
  - the sprite that places `num_kiwami_instance` → the scoreboard. Its `bg_instance` is the per-difficulty panel, and its counters place the digit glyphs as `n1_ins`.
- The scoreboard panels match the WebUI's existing `rank_panel_*.webp`. The game draws the counts on top, so the panels have empty slots.
- Each set renders on one shared canvas (the union of every label's mask-clipped bounds), so plates line up.
- Tested on SBWY 39.06 (21 plates, 38 badges) and the newer ASB61 dump (93 plates, 38 badges), both with no failures. `ai_3` matches the WebUI's existing `nameplate_AI_3.webp`.
- Animated plates (shimmer, sparkles) are rendered at their first frame in the PNGs. `nameplate.scene.json` keeps the full timelines (format `lumen-scene` v1: shapes, sprites with instance names, colour and matrix tables, and the sets with their canvas bounds), and the WebUI plays them with WebGL.
- A placement on an occupied depth keeps the transform and colours it doesn't set (Toho Y22 3–5 swap their character that way). Ops after a keyframe tag belong to the keyframe, not to the last frame.

## Animation export (`Nijiiro/NijiiroAnims.cs`, `Nijiiro/Curve.cs`)

Started as `don-chan-previewer/scripts/nuanmb_to_glb`. `Curve.cs` is the game's ANIM v1.2 curve decoder (`nu::DecompressCurve::Decompress`, 0x140619040): its header bits, time tables, and the linear, stepped, constant and DCT codecs are described there. The rig comes from the game's `body_000000` `.nusktb`, so nothing outside the game install is needed. Only ANIM v1.2 is read (every Nijiiro file). Output is deterministic (bones in SKEL order) and the output accessors drop the optional min/max.

Checked against the previewer's `Don_3D_Nij.glb`: all 54 clips have the same channels, translation and scale are bit-identical, and rotations are within 2e-7 (float noise). `facekeys.json` is byte-identical. The library is rebuilt when any `.nuanmb` or the rig is newer than the output.

## Model export (`Nijiiro/NijiiroModels.cs`)

The output reproduces `numd-exporter` plus the `shaderType` extras that `don-chan-previewer`'s `normalize.ts` was written against. It was checked against all 482 previewer GLBs that exist in 39.06, and 478 match field-for-field: nodes, TRS, skin, inverse binds, every vertex attribute, indices, material names, alpha mode, extras, image names and decoded pixels. The other 4 differ deliberately:

- `head_052000`, `cos_004000`: one bone's rest quaternion has the opposite sign (`q` vs `-q`). It's the same rotation.
- `head_003000`: hidden limbs have zero-scale bones. We write identity rotation and identity inverse bind instead of an arbitrary angle.
- `cos_026000`: one decal object has no skin weights. `numd-exporter` writes all-zero weights, which three.js collapses to the origin (invisible). We write it as a static mesh at the model origin.

Conventions:

- **Nodes:** one node per bone, in SKEL order (so skin joints are `0..n`), then one mesh node per MESH object named `<object>-<subindex>`.
- **Materials:** one per MATL label, with the name kept verbatim because `normalize.ts` parses its flags (`_AT_ONE`, `_AA_ADD`, `FACIAL`, …).
  - `extras.shaderType` comes from the `__nust__` MATL.
  - `alphaMode` is `BLEND` exactly when the diffuse texture has any non-opaque pixel. This was inferred from all 2,152 previewer materials, with no exceptions.
- **Images:** PNG, named after their material (`normalize.ts` reads the texture name). Materials that share a source texture share one buffer view, so the output is about 20% smaller.
- **Vertex data:** the first colour set (`Color1`, sometimes `Color0`) becomes `COLOR_0`, with values kept as-is (some exceed 1). Up to 4 bone influences per vertex, renormalized.

Remaining `gltf-validator` errors all come from the game data and are identical in the previewer's GLBs: colours above 1, a few zero-length normals, and degenerate triangles. All 482 load in three.js's `GLTFLoader`.

Re-running skips a model whose `.glb` is newer than its `.numdlb`. Delete the output to force a rebuild after changing textures by hand.

## SSBH files (`Nijiiro/Ssbh.cs`, `Nijiiro/SsbhFormats.cs`)

Nijiiro's models and animations are Smash Ultimate's SSBH formats, read by hand from the layouts in [ssbh_lib](https://github.com/ultimate-research/ssbh_lib) (MIT), only the versions and fields used here. Little-endian. A file is `HBSS`, u64 64, u32 0, then at 0x10 the format's magic and u16 major, u16 minor; the format's fields start at 0x18. Every pointer is a u64 offset from where the pointer itself is (0 is null); an array is that and a u64 count; a string is a pointer to NUL-terminated UTF-8.

| Format | Magic, version | Fields read (offsets from 0x18) |
| --- | --- | --- |
| `.numdlb` | `LDOM` 1.7 | +8 skeleton file, +16 material files (strings), +40 mesh file, +48 entries (24 bytes: object name, u64 subindex, material label) |
| `.nusktb` | `LEKS` 1.0 | +0 bones (16 bytes: name, u16 index, i16 parent, flags), +48 local transforms (column-major 4×4) |
| `.numatb` | `LTAM` 1.5 | +0 entries (32 bytes: label, attributes, shader label); an attribute is u64 param id, a pointer to its value and u64 value type. `DiffuseMap` is id 30, a string (type 11) |
| `.numshb` | `HSEM` 1.9 | +112 objects (208 bytes), +152 vertex buffers, +168 index buffer, +184 rigging groups (40 bytes: object name, subindex, flags, bone buffers of u32 vertex + f32 weight) |
| `.nuanmb` | `MINA` 1.2 | +16 the last frame (keys run 0 to it), +24 tracks (32 bytes: name, u64 type, 0 is a transform; properties of name + u64 buffer index), +40 buffers |

A mesh object is its name, u64 subindex, parent bone, u32 vertex count and index count, four vertex buffer offsets and four strides (+36, +52), the index buffer offset (+68), the index type (+76: 0 u16, 1 u32), bounds, and its attributes at +192 (48 bytes: u32 usage, u32 data type, u32 buffer, u32 offset). Usages: 0 position, 1 normal, 4 UV, 5 colour; data types: 820 float3, 1076 float4, 1077 half4, 1079 float2, 1024 byte4 (/255). The first attribute of each usage is used.

## Nijiiro `.nutexb` notes

The PC build uses the Smash Ultimate `.nutexb` layout with two differences, both handled in `Textures/Nutexb.cs`:

- The footer starts with `46XT` instead of ` XNT`.
- Image data is **linear**, not Tegra-swizzled (footer `unk3 = 0`), so it is the raw DDS payload. Array layers and mips are stored one after another.

Model textures are BC3 (`0x04A0`) or RGBA8 (`0x0400`), fonts and UI BC7 (`0x04E0`). R8, BGRA8 and BC1–3 are read too; BC1–3 and BC7 round exactly as bcdec does (`Textures/Bcn.cs`). Swizzled (Switch) textures aren't read. Output was checked pixel-for-pixel against `don-chan-previewer`'s PNGs: all 229 are identical.

## Checking it

The outputs were checked file for file against the Rust tool this replaced (tools/extract, removed), with and without `--mods`: the same files, PNGs pixel for pixel, GLBs field for field with floats within 1e-4, and JSON value for value.
