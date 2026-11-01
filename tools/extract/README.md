# extract

Copies and converts the files TaikoLocalServer needs from a Taiko no Tatsujin Nijiiro install.

```bash
extract --game "G:\Games\Taiko no Tatsujin Nijiiro Version (SBWY 39.06)"
```

| Option | |
| --- | --- |
| `--game` | The game install folder, the one containing `Data/x64`. Passing `Data/x64` itself also works. `Data_mods` is read only with `--mods`. |
| `--out` | The server's `wwwroot` folder. Defaults to `wwwroot` next to `extract.exe`, which is right when it sits beside `TaikoLocalServer.exe`. |
| `--mods` | Layer the install's LayeredFS `Data_mods` folder over `Data`, as the game sees it with the plugin. A file in `Data_mods/x64/<path>` replaces `Data/x64/<path>` (fonts, models, UI), and folders gain the mods' extra entries. Datatables (e.g. new songs) come from `Data_mods/x64_enc/datatable`, the re-encrypted ones; the JSON in `Data_mods/x64/datatable` is ignored. Switching `--mods` on or off rebuilds every output once. |

It is safe to re-run, for example after a game update. Outputs that are already up to date (not older than the game's file; datatables must also match in size) are skipped. It exits with code `2` when a required file is missing or a texture fails to convert, and `1` on any other error. One bad texture doesn't stop the rest.

## What it does today

| From `Data/x64/` | To `<out>/` | |
| --- | --- | --- |
| `datatable/{musicinfo,music_order,wordlist,don_cos_reward,shougou,neiro}.bin` | `data/datatable/` | Copied as-is. The server decrypts them on startup. |
| `model/paint/*.nutexb` (58) | `don-model/paint/*.png` | Face paint, the `face` costume slot |
| `model/acce/*.nutexb` (149) | `don-model/acce/*.png` | Puchi accessories |
| `model/face/*.nutexb` (22) | `don-model/face/*.png` | Expression atlases: 12 layers stacked vertically |
| `model/{body,head,cos}/<name>/<name>/<name>.numdlb` (483) | `don-model/{body,head,cos}/<name>.glb` | Skinned don-chan parts, textures embedded |
| `animation/*.nuanmb` (54) | `don-model/animation/don.glb` + `facekeys.json` | One animation library (a clip per file) and the face-expression keys |
| `lumen/000_default/common/name_plate.{nulm,nutexb}` | `don-model/nameplate/plate_<label>.png`, `dan_<label>.png`, `plates.json` | Title plates and dan badges rendered from the game's UI. `plates.json` lists the labels in order, and a plate's index is its `TitlePlateId` |
| same | `don-model/nameplate/nameplate.scene.json` + `atlas.png` | The animated nameplate for the WebUI's Lumen player (`lumen-player.js`): plates, badges and the game's whole nameplate with its text boxes per layout |
| `lumen/000_default/{common/option, common_songselect/common_song_select_{song_board_icon,main_diff_board}, enso_dani/dani_select/dani_select}.{nulm,nutexb}` | `don-model/ui/{option,difficulty,crown,rank,dani}_<label>.png` + one index per source | The WebUI's icons: song options (doron, abekobe, kimagure, detarame, note_pos, speedx…), difficulties (easy … ura), crowns (`badge_crown_1…3`), score ranks (`badge_rank_1…7`) and Dani Dojo pass stamps (`stamp_{red,gold}_01…03`) |
| `font/{jp_30,en_30,tw_30,kr_30}.{xml,nutexb}` | `don-model/font/<sheet>/<page>.{png,json}` | The nameplate's glyph sheets (Lumen font slot 1, the 勘亭流 face) in 256-code-point pages, so name and title previews use the game's lettering |
| `sound/se_neiro_{00…19}_v12a.nus3bank` | `don-model/tone/NN_{don,ka}.wav` | The drum tones' don and ka hits, the bank's two embedded PCM WAVs cut out as-is, for the Profile's tone preview |
| `lumen/000_default/common/player_board.{nulm,nutexb}` | `don-model/scoreboard/panel_<difficulty>.png`, `digit_ns_0N.png`, `scoreboard.json` | The Profile achievement panel ("scoreboard") per difficulty, plus the game's counter digit glyphs |

## Game UI art (`src/lumen.rs`, `src/ui.rs`)

The game's 2D UI, including the nameplates and the player board, uses Lumen files (`.nulm`, magic `LMB\0`), the Flash-like format the game's 2D UI uses, with its texture atlas in the matching `.nutexb`. `lumen.rs` parses the shapes (textured or solid-colour triangles) and the sprite timelines. It renders a sprite at a frame label by playing its display list up to that frame: depth order, place/move/remove, matrix or position transforms, colour multiply/add, nested sprites at frame 0, and **clip masks** (a placement's clip depth hides deeper objects outside its shape). The tag layout is documented in `docs/webui-refactor-plan.md` §3d.

- Sprites are found **by frame labels or instance names**, never by id, because ids change between builds:
  - `wood` → title plates
  - `5kyuu_red` → dan badges
  - the sprite that places `num_kiwami_instance` → the scoreboard. Its `bg_instance` is the per-difficulty panel, and its counters place the digit glyphs as `n1_ins`.
- The scoreboard panels match the WebUI's existing `rank_panel_*.webp`. The game draws the counts on top, so the panels have empty slots.
- Each set renders on one shared canvas (the union of every label's mask-clipped bounds), so plates line up.
- Tested on SBWY 39.06 (21 plates, 38 badges) and the newer ASB61 dump (93 plates, 38 badges), both with no failures. `ai_3` matches the WebUI's existing `nameplate_AI_3.webp`.
- Animated plates (shimmer, sparkles) are rendered at their first frame in the PNGs. `nameplate.scene.json` keeps the full timelines (format `lumen-scene` v1: shapes, sprites with instance names, colour and matrix tables, and the sets with their canvas bounds), and the WebUI plays them with WebGL.
- A placement on an occupied depth keeps the transform and colours it doesn't set (Toho Y22 3–5 swap their character that way). Ops after a keyframe tag belong to the keyframe, not to the last frame.

## Animation export (`src/anims.rs`, `src/v12/`)

Ported from `don-chan-previewer/scripts/nuanmb_to_glb`, which this replaces. The ANIM v1.2 curve decoder in `src/v12/` is copied from it unchanged; see that tool's README for the format notes. The changes:

- The rig comes from the game's `body_000000` `.nusktb` instead of a pre-made `--skeleton-glb`, so nothing outside the game install is needed.
- Only ANIM v1.2 is handled (every Nijiiro file). The `ssbh_data` v2 fallback is gone.
- Output is deterministic (bones in SKEL order), and output accessors drop the optional min/max, making the GLB about 11% smaller.

Checked against the previewer's `Don_3D_Nij.glb`: all 54 clips have the same channels, translation and scale are bit-identical, and rotations are within 2e-7 (float noise). `facekeys.json` is byte-identical. The library is rebuilt when any `.nuanmb` or the rig is newer than the output.

To check the decoder against a game install:

```bash
NUANMB_DIR="<game>/Data/x64/animation" cargo test --release --manifest-path tools/extract/Cargo.toml -- --nocapture
```

## Model export (`src/models.rs`)

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

## Nijiiro `.nutexb` notes

The PC build uses the Smash Ultimate `.nutexb` layout with two differences, both handled in `src/textures.rs`:

- The footer starts with `46XT` instead of ` XNT`.
- Image data is **linear**, not Tegra-swizzled (footer `unk3 = 0`), so it is the raw DDS payload. Array layers and mips are stored one after another.

Model textures are BC3 (`0x04A0`), R8 (`0x0100`) or RGBA8 (`0x0400`). UI textures elsewhere in the game use BC7. Every format the `image_dds` crate decodes works. Output was checked pixel-for-pixel against `don-chan-previewer`'s PNGs: all 229 are identical.

## Building

Needs a Rust toolchain ([rustup](https://rustup.rs), stable).

```bash
cargo build --release --manifest-path tools/extract/Cargo.toml
cargo test --release --manifest-path tools/extract/Cargo.toml
```

The binary ends up at `tools/extract/target/release/extract.exe`.
