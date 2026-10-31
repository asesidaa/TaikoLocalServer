# extract

Copies (and, in later versions, converts) the files TaikoLocalServer needs from a Taiko no Tatsujin Nijiiro install.

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

Converting the don-chan models and animations for the 3D preview is next. See `docs/webui-refactor-plan.md` §2.

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
