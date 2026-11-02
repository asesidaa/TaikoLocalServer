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

## Older games (`--era`, `src/ac15.rs`, `src/source.rs`)

```bash
extract --era green --game "D:\RPCS3\Taiko no Tatsujin Green Version (SBWY 13.02)"
extract --era yellow --game "D:\Taiko no Tatsujin - Yellow Version.zip"
```

`--era` is one of `green`, `blue`, `yellow`, `red`, `white`, `murasaki`, `kimidori`, `momoiro`. `--game` is the RPCS3 install (or anything down to its `dev_hdd0/game/SCEEXE000/USRDIR/data`, also one folder deeper, like Blue's `[ARCADE SYSTEM 357] … ReadyToPlay.7z` folder), or a `.zip` of it. Zips are read in place, without unpacking: stored and deflate members, zip64, every member CRC-checked. `--inputs --era <era>` lists what it reads, relative to `USRDIR` (paths start with `data/`).

| From `USRDIR/data/` | To `<out>/data/<era>/data/` | |
| --- | --- | --- |
| `config/<id>/{musicinfo,musicmedleyinfo}.xml`, plus `defmusic.bin` (all but Green and Blue), `present.xml` and `spacialbaid.xml` (White, Murasaki, Kimidori) | same path | Kimidori and Momoiro keep them at the top of `data`. `<id>` is Green `S11100-1`, Blue `S10100-1`, Yellow `ST9100-1`, Red `ST8100-1`, White `ST7100-1`, Murasaki `ST6100-1` |
| `config/S10100-1/battle/*.xml` | same | Blue's battle mode, when the dump has it (11.01BF doesn't) |
| `config/S11100-1/rewardtitlefiltering.xml`, `nutdata/{cos,title,tone}_name/nutdatapack.ndp` | same | Green's name packs (`GreenCatalogExtractor`) |
| `fumen/tuning.bin` | same | |
| `movie/*`, `don3d/{cos,face,full/cos,full/face,parts/*}/*`, `nutdata/**/{title,tone}_name/*` | same, **empty** | Only the names are read: attract movie ids, and the costume, title and tone ids (`Ac15Don3dDirScanner`, `Ac15CustomizationSourceParser`). An existing file is never replaced |

That's 1.6 MB (Momoiro) to 8.3 MB (Green). When the game has only another variant of the config id, it's used and written under the id the server reads: Red 10.01 ships `ST8100-7` only (an early Red, 25 Dojo courses). The White zip we have is an incomplete dump (no `config`, `tuning.bin`, `don3d` or `lm_data`), so White reports them missing.

| From `USRDIR/data/nutdata/` | To `<out>/game_data/<era>/` | |
| --- | --- | --- |
| `{cos_icon,tone_icon}/`, `pack/<…>/<nn>/tone_icon/`, `<id>/appendable/<nn>/tone_icon/` | `cos_icon/cos_icon_NNN.png`, `tone_icon/icon_neiro_NNN.png` | Costume and drum-sound icons. `cos_icon` exists only in Green (91); every game has its tone icons |
| the same folders for `cos_name`, `title_name`, `tone_name` | `cos_name/`, `title_name/shougou_NNN.png`, `tone_name/` | The names, as the game draws them (text rendered to images). These are the only title names Momoiro has (284 images) |

Packs are merged by file name. A game's own `nutdata/<id>/appendable` packs are applied last, so they win over the packs it inherited. Green: 91 costume icons, 7 tone icons, 418 titles; Momoiro: 5 tone icons, 284 titles; Blue: 7 and 364; Red 10.01: 1 and 11.

### NTP3 textures (`.nut`, `src/nut.rs`)

The PS3 NUT, big-endian. Header: `NTP3`, u16 version (`0x0100` or `0x0200`), u16 texture count, 8 bytes of padding. Then per texture, at 0x10:

| Offset | |
| --- | --- |
| +0x00 | u32 total size (header + data) |
| +0x08 | u32 data size |
| +0x0C | u16 header size (0x50, more with a mip size table) |
| +0x11 | u8 mip count |
| +0x13 | u8 format: `0` DXT1, `1` DXT3, `2` DXT5, `14` A8R8G8B8, `17` R8G8B8A8 (bytes in that order). Data is linear, not swizzled |
| +0x14 | u16 width, u16 height |
| +0x20 | u32 data offset from this header (`0x0200` only) |
| +0x30 | `eXt\0` block (0x10 bytes), then `GIDX`, u32 0x10, **u32 texture id** (what NUD materials refer to) |

Version `0x0100` lays them out header, data, header, data (data right after its header, the next header after `total`). `0x0200` puts every header first (the next one `header size` later) and the data where +0x20 says. The base mip comes first.

### NUT packs (`.ndp`)

`NUT_PACK_TYPE1` is a boost `binary_oarchive` (big-endian, 32-bit PS3 sizes): the magic padded to 16 bytes, u32 22 + `serialization::archive`, the archive's header and class info, then at 0x3E the u32 entry count. From 0x4B each entry is a u32 name length, the name (`cos_icon_000.nut`), a u32 offset and a u32 size. The first entry has a 5-byte class header between its name and offset. The data starts 4 bytes after the last entry (a u32 blob size), and offsets count from there. Every entry is a whole NTP3. All 347 packs in Green and all 13 in Momoiro parse this way.

| From `USRDIR/data/lumendata/packed/` | To `<out>/game_data/<era>/ui/` | |
| --- | --- | --- |
| `song_select/packeddata.ddp` | `crown_{none,silver,gold}.png`, `crown_course_{gold,silver}_{easy,normal,hard,mania,extreme}.png` | Crowns, and a crown beside each difficulty's face (Momoiro has no `extreme`) |
| `song_select` (`courseicon`), else `enso_result` (`result.lm`, labels `easy,normal,hard,mania`) | `difficulty_{easy,normal,hard,mania}.png` | Green to Kimidori use the compact song-select icon; Momoiro the result screen's, with its caption |
| `dani_result/packeddata.ddp` (`dani_title`) | `dan_{syokyuu,10kyuu…1kyuu,syodan,2dan…,tatsujin}.png` | The dan names as the Dojo result draws them. Older games have fewer ranks (Momoiro: 4kyuu–10dan) |
| `enso_result/packeddata.ddp` (`titlepanel`) | `plate_{normal,gold,platinum,rainbow}.png` | Title plates by rarity. Momoiro has none |
| | `ui.json` | Each set's labels, in the game's order |

Every label of a set is drawn on one shared canvas, so they line up. Green, Blue, Yellow and Red: 47 images; Murasaki 39, Kimidori 36, Momoiro 32.

### PS3 Lumen packs (`LM_NUT_TYPE1`, `src/lmpack.rs`)

`lumendata/packed/<scene>/packeddata.ddp` holds a scene's `.lm` files and their atlases; `packlist.txt` beside it lists the same names (it isn't needed). It's a boost binary archive like the NUT packs: the `.lm` count at 0x3E, then from 0x4B per `.lm` its name (u32 length + bytes; the first one adds a 5-byte class header), u32 offset, u32 size and the `[from, to)` range of its textures in the texture list. Then 5 bytes, the u32 texture count, 9 bytes, and a u32 (offset, size) per texture. The data starts 16 bytes after the last pair: first the `.lm` files, then the textures, whose offsets count from the end of the last `.lm`. Each texture is a whole NTP3, and a `.lm`'s textures are its atlases in id order. All 230 packs in Green (765 `.lm`, 20,668 textures) and all 106 in Momoiro parse. `lm_data/output.ddp` and the loose `lm_data/don/` aren't used.

### PS3 Lumen (`.lm`, `src/lumen.rs`)

The same `LMB` tag stream as Nijiiro's `.nulm`, big-endian (the header's second word reads `0x10` either way, which tells them apart). The differences:

- Graphics are tag `0xF023`, always 18 words: one quad, `x y u v` × 4 (floats), then u32 atlas id and u16 fill (`0x41` or `0x40` textured, `0` solid colour with the colour index in the atlas field). Triangles `0 1 2`, `0 2 3`. No `0xF024`.
- `0xF007` lists one entry per atlas (id, name symbol, width, height), and each atlas is its own `.nut` at its own size (400×400, 64×64…), not layers of one texture. `Lumen::render_layers` samples them separately, and only the atlases a set uses are decoded (song select has 788).
- u16 fields (place mode, depth, clip depth, colours) sit at the same byte offsets, so they read as-is in big-endian.

To find a sprite in another scene, `LM_PACK=<packeddata.ddp> [LM_NAME=<lm>] [LM_SPRITE=<id> LM_OUT=<dir>] cargo test --release lm_ls -- --ignored --nocapture` lists every sprite's labels and the instance names it places, and renders a sprite's labels.

In `--inputs` rules a `*` segment matches any one folder name (`data/config/*/musicinfo.xml`, `data/nutdata/*/*/title_name/`); `ExtractRunner.Match` and the WebUI's `gamefolder.js` match the same way.

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
