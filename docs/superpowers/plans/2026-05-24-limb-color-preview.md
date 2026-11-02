# Limb Color Preview Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the WebUI player preview visibly respond to `LimbColor` by adding one shared standard-Don limb mask layer.

**Architecture:** Runtime stays in the existing shared `PlayerPreview.razor` layered-image model: a single committed WebP mask is stacked in the standard Don branch and filtered with `TaikoCustomizationVisuals.GetCostumeColorFilter(Setting.LimbColor)`. A small Python helper under `tools/limb-mask/` documents and reproduces the mask asset; it is development tooling only and is not used by server startup, AdminApi, catalog loading, or WebUI runtime.

**Tech Stack:** .NET 10, Blazor/MudBlazor Razor components, xUnit markup tests, Python 3 with Pillow for one-off WebP asset generation.

**Reference:** [docs/superpowers/specs/2026-05-24-limb-color-preview-design.md](../specs/2026-05-24-limb-color-preview-design.md) (commit `7d615ec2`).

---

## Worktree And Safety

The working tree currently has an unrelated local settings edit in `Host/Configurations/ServerSettings.json`. Do not stage, edit, or revert that file. Run `git status --short` before each commit and stage only files listed in the task.

`.tools/` is ignored by git in this repo, so committed reproducibility tooling for this feature goes under `tools/limb-mask/`. Local ignored client data and any ad-hoc IDA outputs remain under `Host/wwwroot/data/green/data` and `.tools/` respectively.

## File Structure

- **Modify** `Tests/WebUi/GreenCustomizationWebUiTests.cs`:
  - Add a markup-shape test that proves `PlayerPreview.razor` renders a real limb mask layer in the standard branch.
  - Add an asset-existence test for the committed WebP mask path.
- **Create** `tools/limb-mask/extract_limb_mask.py`:
  - Deterministically derives the flat WebUI mask from the current standard preview art and existing body/face masks.
  - Accepts explicit input/output paths so it can be rerun from any checkout.
- **Create** `tools/limb-mask/README.md`:
  - Documents the client model/texture parser evidence checkpoint and the current fallback derivation.
- **Create** `TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp`:
  - Shared flat standard-Don limb mask used by the WebUI.
- **Modify** `TaikoWebUI/Shared/Customize/PlayerPreview.razor`:
  - Remove the unused `--taiko-limb-filter` CSS variable.
  - Add the limb mask `<MudImage>` only inside `Setting.Kigurumi == 0`.

## Task 1: Failing WebUI Tests

**Files:**
- Modify: `Tests/WebUi/GreenCustomizationWebUiTests.cs`

- [ ] **Step 1: Add tests for the missing limb mask layer**

Insert these two tests after `PlayerPreview_OwnsSharedNijiiroCostumeAndNameplateMarkup()`:

```csharp
[Fact]
public void PlayerPreview_RendersStandardLimbMaskWithLimbColorOnlyInNormalBranch()
{
    var markup = ReadWebUiFile("Shared", "Customize", "PlayerPreview.razor");
    var normalized = NormalizeLineEndings(markup);

    Assert.Contains("images/Costumes/masks/standard-limbmask-0000.webp", markup);
    Assert.Contains("TaikoCustomizationVisuals.GetCostumeColorFilter(Setting.LimbColor)", markup);
    Assert.DoesNotContain("--taiko-limb-filter", markup);

    var normalBranchStart = normalized.IndexOf("if (Setting.Kigurumi == 0)", StringComparison.Ordinal);
    Assert.True(normalBranchStart >= 0, "Could not find the standard Don branch.");

    var kigurumiBranchStart = normalized.IndexOf("\n                else\n", normalBranchStart, StringComparison.Ordinal);
    Assert.True(kigurumiBranchStart > normalBranchStart, "Could not find the kigurumi branch after the standard Don branch.");

    var standardBranch = normalized[normalBranchStart..kigurumiBranchStart];
    var kigurumiBranch = normalized[kigurumiBranchStart..];

    Assert.Contains("standard-limbmask-0000.webp", standardBranch);
    Assert.DoesNotContain("standard-limbmask-0000.webp", kigurumiBranch);
}

[Fact]
public void StandardLimbMaskAsset_IsCommittedWebP()
{
    var maskPath = Path.Combine(
        FindRepoRoot(),
        "TaikoWebUI",
        "wwwroot",
        "images",
        "Costumes",
        "masks",
        "standard-limbmask-0000.webp");

    Assert.True(File.Exists(maskPath), $"Missing limb mask asset at {maskPath}.");

    var bytes = File.ReadAllBytes(maskPath);
    Assert.True(bytes.Length > 12, "The limb mask asset is empty or truncated.");
    Assert.Equal((byte)'R', bytes[0]);
    Assert.Equal((byte)'I', bytes[1]);
    Assert.Equal((byte)'F', bytes[2]);
    Assert.Equal((byte)'F', bytes[3]);
    Assert.Equal((byte)'W', bytes[8]);
    Assert.Equal((byte)'E', bytes[9]);
    Assert.Equal((byte)'B', bytes[10]);
    Assert.Equal((byte)'P', bytes[11]);
}
```

- [ ] **Step 2: Run the focused tests and confirm the expected red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationWebUiTests"
```

Expected: FAIL. The new tests should report that `standard-limbmask-0000.webp` is not present in `PlayerPreview.razor` and the asset file does not exist.

Do not commit this red state.

## Task 2: Mask Derivation Tool And Asset

**Files:**
- Create: `tools/limb-mask/extract_limb_mask.py`
- Create: `tools/limb-mask/README.md`
- Create: `TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp`

- [ ] **Step 1: Record the parser evidence checkpoint**

Run:

```powershell
gh api repos/jam1garner/Smash-Forge/contents/'Smash Forge/Filetypes/Models/Nuds/NUD.cs' -H 'Accept: application/vnd.github.raw' | Select-String -Pattern 'NDP3|public override void Read'
gh api repos/jam1garner/Smash-Forge/contents/'Smash Forge/Filetypes/Textures/NUT.cs' -H 'Accept: application/vnd.github.raw' | Select-String -Pattern 'NTP3|public void ReadNTP3'
[System.Text.Encoding]::ASCII.GetString((Get-Content -Path 'Host/wwwroot/data/green/data/don3d/full/cos/cos_000000.nud' -Encoding Byte -TotalCount 4))
[System.Text.Encoding]::ASCII.GetString((Get-Content -Path 'Host/wwwroot/data/green/data/don3d/full/cos/cos_000000.nut' -Encoding Byte -TotalCount 4))
```

Expected:

- The first command includes `NDP3`.
- The second command includes `NTP3`.
- The local model header prints `NDP3`.
- The local texture header prints `NTP3`.

If local Green data is not present, skip only the two local header commands and note that in `tools/limb-mask/README.md`. Do not make committed tests depend on ignored local client data.

- [ ] **Step 2: Create the Python mask derivation script**

Create `tools/limb-mask/extract_limb_mask.py`:

```python
#!/usr/bin/env python3
from __future__ import annotations

import argparse
from pathlib import Path

try:
    from PIL import Image
except ImportError as exc:
    raise SystemExit("Pillow is required: python -m pip install Pillow") from exc


EXPECTED_SIZE = (463, 400)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Derive the shared standard Don limb mask for the WebUI preview."
    )
    parser.add_argument(
        "--standard-art",
        default="TaikoWebUI/wwwroot/images/Costumes/body/body-0000.webp",
        help="Standard Don body preview WebP.",
    )
    parser.add_argument(
        "--body-mask",
        default="TaikoWebUI/wwwroot/images/Costumes/masks/body-bodymask-0000.webp",
        help="Existing standard Don body color mask WebP.",
    )
    parser.add_argument(
        "--face-mask",
        default="TaikoWebUI/wwwroot/images/Costumes/masks/body-facemask-0000.webp",
        help="Existing standard Don face color mask WebP.",
    )
    parser.add_argument(
        "--out",
        default="TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp",
        help="Output WebP mask path.",
    )
    return parser.parse_args()


def load_rgba(path: Path) -> Image.Image:
    image = Image.open(path).convert("RGBA")
    if image.size != EXPECTED_SIZE:
        raise SystemExit(f"{path} has size {image.size}; expected {EXPECTED_SIZE}.")
    return image


def is_limb_surface(r: int, g: int, b: int, a: int) -> bool:
    if a < 80:
        return False

    # Standard art limb surfaces are the light cream regions left after the
    # existing body and face masks are excluded. This avoids recoloring outlines.
    return r >= 180 and g >= 160 and b >= 130 and abs(r - g) <= 45 and abs(g - b) <= 70


def derive_mask(standard_art: Image.Image, body_mask: Image.Image, face_mask: Image.Image) -> Image.Image:
    output = Image.new("RGBA", EXPECTED_SIZE, (0, 0, 0, 0))
    out_pixels = output.load()
    standard_pixels = standard_art.load()
    body_alpha = body_mask.getchannel("A").load()
    face_alpha = face_mask.getchannel("A").load()

    for y in range(EXPECTED_SIZE[1]):
        for x in range(EXPECTED_SIZE[0]):
            if body_alpha[x, y] > 0 or face_alpha[x, y] > 0:
                continue

            r, g, b, a = standard_pixels[x, y]
            if is_limb_surface(r, g, b, a):
                out_pixels[x, y] = (0, 0, 0, 255)

    return output


def main() -> None:
    args = parse_args()
    standard_art = load_rgba(Path(args.standard_art))
    body_mask = load_rgba(Path(args.body_mask))
    face_mask = load_rgba(Path(args.face_mask))

    mask = derive_mask(standard_art, body_mask, face_mask)
    alpha_bbox = mask.getchannel("A").getbbox()
    if alpha_bbox is None:
        raise SystemExit("Derived limb mask is empty.")

    out = Path(args.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    mask.save(out, format="WEBP", lossless=True, exact=True)
    print(f"wrote {out} size={mask.size} alpha_bbox={alpha_bbox}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 3: Create the tooling README**

Create `tools/limb-mask/README.md`:

```markdown
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
```

- [ ] **Step 4: Generate the mask asset**

Run:

```powershell
python tools/limb-mask/extract_limb_mask.py
```

Expected: output starts with:

```text
wrote TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp size=(463, 400)
```

- [ ] **Step 5: Verify mask dimensions and non-empty alpha**

Run:

```powershell
@'
from pathlib import Path
from PIL import Image

path = Path("TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp")
image = Image.open(path).convert("RGBA")
assert image.size == (463, 400), image.size
assert image.getchannel("A").getbbox() is not None
print(path, image.size, image.getchannel("A").getbbox())
'@ | python -
```

Expected: prints the mask path, `(463, 400)`, and a non-null alpha bounding box.

Do not commit yet; Task 3 makes the red markup test pass.

## Task 3: Render The Limb Mask In PlayerPreview

**Files:**
- Modify: `TaikoWebUI/Shared/Customize/PlayerPreview.razor`
- Modify: `Tests/WebUi/GreenCustomizationWebUiTests.cs`
- Create: `tools/limb-mask/extract_limb_mask.py`
- Create: `tools/limb-mask/README.md`
- Create: `TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp`

- [ ] **Step 1: Remove the unused limb CSS variable**

In `TaikoWebUI/Shared/Customize/PlayerPreview.razor`, replace:

```razor
<MudItem style=@($"height: auto; --taiko-limb-filter: {TaikoCustomizationVisuals.GetCostumeColorFilter(Setting.LimbColor)}")>
```

with:

```razor
<MudItem style="height: auto;">
```

- [ ] **Step 2: Add the limb mask layer to the standard branch**

In the `Setting.Kigurumi == 0` branch, add the limb mask after the face mask and before the opaque body art:

```razor
<MudImage Fluid="true" style=@($"position: relative; top: 0; left: 0; filter: {TaikoCustomizationVisuals.GetCostumeColorFilter(Setting.BodyColor)}") Src=@CostumeOrDefault("masks/body-bodymask", Setting.Body, "masks/body-bodymask-0000") />
<MudImage Fluid="true" Class="profile-image" style=@($"filter: {TaikoCustomizationVisuals.GetCostumeColorFilter(Setting.FaceColor)}") Src=@CostumeOrDefault("masks/body-facemask", Setting.Body, "masks/body-facemask-0000") />
<MudImage Fluid="true" Class="profile-image" style=@($"filter: {TaikoCustomizationVisuals.GetCostumeColorFilter(Setting.LimbColor)}") Src="images/Costumes/masks/standard-limbmask-0000.webp" />
<MudImage Fluid="true" Class="profile-image" onerror="this.src='images/Costumes/body/body-0000.webp'" Src=@($"images/Costumes/body/body-{Setting.Body.ToString().PadLeft(4, '0')}.webp") />
```

Do not add this image to the kigurumi branch.

- [ ] **Step 3: Run the focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationWebUiTests"
```

Expected: PASS. The test output should report no failed tests for `GreenCustomizationWebUiTests`.

- [ ] **Step 4: Commit the tested WebUI change**

Run:

```powershell
git status --short
git add -- Tests/WebUi/GreenCustomizationWebUiTests.cs TaikoWebUI/Shared/Customize/PlayerPreview.razor TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp tools/limb-mask/extract_limb_mask.py tools/limb-mask/README.md
git diff --cached --name-status
git commit -m "Add standard Don limb color preview"
```

Expected staged files:

```text
M	Tests/WebUi/GreenCustomizationWebUiTests.cs
M	TaikoWebUI/Shared/Customize/PlayerPreview.razor
A	TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp
A	tools/limb-mask/extract_limb_mask.py
A	tools/limb-mask/README.md
```

`Host/Configurations/ServerSettings.json` must not be staged.

## Task 4: Final Verification

**Files:**
- No planned file edits.

- [ ] **Step 1: Run the WebUI project build**

Run:

```powershell
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

Expected: exit code `0`.

- [ ] **Step 2: Rerun the focused WebUI tests after the commit**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationWebUiTests"
```

Expected: exit code `0`.

- [ ] **Step 3: Run the final status check**

Run:

```powershell
git status --short --branch
```

Expected: branch is ahead by the implementation commit plus prior documentation commits, and the only remaining unstaged file is the pre-existing `Host/Configurations/ServerSettings.json`.

Do not claim completion unless the build and focused tests both exit `0`.
