# Limb Color Preview Design

Date: 2026-05-24

## Context

The WebUI customization flow already carries all three Don color fields:

- `ColorPicker.razor` edits body, face, and limb color IDs.
- `Profile.razor.cs` copies `BodyColor`, `FaceColor`, and `LimbColor` between the AdminApi response and the picker value.
- `UserSetting`, the Nijiiro and Green AdminApi controllers, persistence models, and BAID responses already expose `LimbColor`.

The preview gap is narrower. `PlayerPreview.razor` uses body and face color by stacking flat WebP masks over the existing costume preview images and applying `TaikoCustomizationVisuals.GetCostumeColorFilter(...)`. It currently references `Setting.LimbColor` only through an unused CSS variable, so changing limb color does not affect the rendered preview.

The committed WebUI costume assets include body, face, head, kigurumi, puchi, and a small set of body/face mask overlays. They do not include a limb mask overlay. The ignored Green client data contains `.nud` model and `.nut` texture files, and existing public tooling such as Smash Forge can parse those formats, but WebUI runtime should not parse client models.

## Goal

Add limb color preview to the WebUI player visualizer by rendering one shared standard-Don limb mask layer with the selected `LimbColor`.

The behavior should match the official model behavior: limb color is shared and applies only to the standard Don model, not to kigurumi-specific costume previews.

## Non-Goals

- Do not add `.nud` or `.nut` parsing to server startup, catalog loading, AdminApi, or WebUI runtime.
- Do not create per-costume limb masks.
- Do not change persistence, AdminApi contracts, color picker behavior, or BAID serialization unless implementation reveals a direct compile/test issue.
- Do not make kigurumi previews consume limb color.

## Runtime Design

Add one committed mask asset under the existing WebUI costume mask directory, for example:

`TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp`

The asset must:

- Match the existing preview image dimensions: `463x400`.
- Be transparent outside the standard Don limb recolor region.
- Use an opaque white mask shape inside the limb recolor region, matching the body/facemask overlay convention.
- Align with the existing standard body preview image stack.

Update `TaikoWebUI/Shared/Customize/PlayerPreview.razor` so the normal split-costume branch (`Setting.Kigurumi == 0`) includes the limb mask layer. The layer should:

- Use the same absolute stacking convention as existing preview overlays.
- Apply `TaikoCustomizationVisuals.GetCostumeColorFilter(Setting.LimbColor)`.
- Render near the other color mask layers, before the opaque costume art layers.

The kigurumi branch remains unchanged. If the limb mask asset fails to load in the browser, body/face/costume preview layers should still render; only limb recoloring should be absent.

Remove or stop relying on the unused `--taiko-limb-filter` CSS variable once the real image layer consumes `LimbColor`.

## Development-Time Mask Derivation

Prefer deriving the committed mask from the real client standard Don model/texture data:

- Standard model/texture source is expected to be the Green standard full costume pair, such as `Host/wwwroot/data/green/data/don3d/full/cos/cos_000000.{nud,nut}`, or the equivalent confirmed standard model path.
- Use Python for the derivation tooling because it is easy to script, test, and run against ignored local client data.
- First look for existing format parsers. Smash Forge is a known MIT-licensed C# parser for big-endian `NDP3` `.nud` models and `NTP3` `.nut` textures, and can be used as format reference.
- If exact limb-surface identification needs Taiko-specific evidence, use local `.tools`/IDA evidence to identify the relevant mesh or material, then encode the selector in the Python tool.

The Python tool should be a reproducibility aid, not a production dependency. A likely shape is:

```powershell
python .tools/limb-mask/extract_limb_mask.py `
  --model Host/wwwroot/data/green/data/don3d/full/cos/cos_000000.nud `
  --textures Host/wwwroot/data/green/data/don3d/full/cos/cos_000000.nut `
  --out TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp
```

If real client derivation is blocked or disproportionately expensive, create the same aligned flat mask from the current standard preview artwork and document why in the tool/readme or implementation notes. The committed mask remains the runtime deliverable either way.

## Tests

Add or update WebUI tests so they assert:

- `PlayerPreview.razor` contains an actual limb mask image layer.
- That layer applies `TaikoCustomizationVisuals.GetCostumeColorFilter(Setting.LimbColor)`.
- The mask path points at a committed asset that exists under `TaikoWebUI/wwwroot/images/Costumes/masks`.
- `Setting.LimbColor` is no longer represented only by an unused CSS variable.
- The limb mask layer is in the `Setting.Kigurumi == 0` branch and not the kigurumi branch.

Run targeted verification first:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationWebUiTests"
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

If the implementation touches broader shared preview behavior, also run the full test project after a successful build.
