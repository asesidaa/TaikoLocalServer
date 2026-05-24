# Limb Mask Tooling

This directory contains development-time tooling for the WebUI standard Don limb mask.

Runtime code does not parse `.nud` or `.nut` files. The WebUI consumes the committed flat mask at:

`TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp`

## Parser Evidence

Smash Forge has MIT-licensed C# readers for big-endian `NDP3` `.nud` models and `NTP3` `.nut` textures. The local Green standard model pair is expected at:

- `Host/wwwroot/data/green/data/don3d/full/cos/cos_000000.nud`
- `Host/wwwroot/data/green/data/don3d/full/cos/cos_000000.nut`

When local Green data is present, the file headers should be `NDP3` and `NTP3`.

The current committed mask is derived from the existing standard WebUI preview art and the existing standard body/face color masks. This is the fallback allowed by the design: it produces the same runtime artifact without adding model parsing to the server or WebUI. If a future parser-derived mask is produced, keep the same output path and rerun the WebUI tests.

## Regenerate

From the repository root:

```powershell
python tools/limb-mask/extract_limb_mask.py
```

Expected output includes:

```text
wrote TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp size=(463, 400)
```
