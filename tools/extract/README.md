# extract

Copies and converts the files TaikoLocalServer needs from a Taiko no Tatsujin Nijiiro install.

```bash
extract --game "G:\Games\Taiko no Tatsujin Nijiiro Version (SBWY 39.06)"
```

| Option | |
| --- | --- |
| `--game` | The game install folder, the one containing `Data/x64`. Passing `Data/x64` itself also works. `Data_mods` is never read. |
| `--out` | The server's `wwwroot` folder. Defaults to `wwwroot` next to `extract.exe`, which is right when it sits beside `TaikoLocalServer.exe`. |

It is safe to re-run, for example after a game update. Outputs that are already up to date (not older than the game's file; datatables must also match in size) are skipped. It exits with code `2` when a required file is missing or a texture fails to convert, and `1` on any other error. One bad texture doesn't stop the rest.

## What it does today

| From `Data/x64/` | To `<out>/` | |
| --- | --- | --- |
| `datatable/{musicinfo,music_order,wordlist,don_cos_reward,shougou,neiro}.bin` | `data/datatable/` | Copied as-is. The server decrypts them on startup. |
| `model/paint/*.nutexb` (58) | `don-model/paint/*.png` | Face paint, the `face` costume slot |
| `model/acce/*.nutexb` (149) | `don-model/acce/*.png` | Puchi accessories |
| `model/face/*.nutexb` (22) | `don-model/face/*.png` | Expression atlases: 12 layers stacked vertically |
| `model/{body,head,cos}/<name>/<name>/<name>.numdlb` (483) | `don-model/{body,head,cos}/<name>.glb` | Skinned don-chan parts, textures embedded |

Converting the animations is next. See `docs/webui-refactor-plan.md` §2.

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
