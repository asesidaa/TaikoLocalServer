# Green Customization Catalog Sharing Design

Date: 2026-05-18

## Context

Green customization support currently loads Green-specific JSON files generated from the Green game-data tree. Those files enumerate IDs but often do not contain human-readable names. Online and local binary checks show that Green and Nijiiro use the same numeric ID spaces for titles, costumes, tones, and MyDon colors, with Green generally being a filtered subset of Nijiiro.

Evidence from local catalog comparison:

- Green title assets enumerate `0..367`; Nijiiro `shougou.uniqueId` has later IDs as well.
- Green costume availability is a per-slot subset of Nijiiro:
  - body `0..155`
  - face `0..57`
  - head `0..139`
  - kigurumi `0..150`
  - puchi `0..120`
- Green tone name packs enumerate `0..15`, but Green sound/select assets include tone IDs through `19`; Nijiiro neiro data has `0..19`.
- Existing WebUI costume preview images already use the same filename ID scheme and cover the Green ranges.

## Goal

Share Nijiiro customization data with Green as the canonical name source, while filtering the Green catalog to IDs that Green can actually use. Restore the WebUI player preview and color swatches for Green.

## Design

Add a small Green customization catalog composer in `Infrastructure/GameDataCatalog/Green`.

The composer takes:

- Green extracted costumes, titles, and neiros from `green_*_data.json`.
- Optional Nijiiro catalog data from `INijiiroCatalog`, when Nijiiro is enabled and initialized.

It returns Green-facing catalogs:

- **Titles:** keep only title IDs present in the Green extracted title catalog. For each ID, prefer the matching Nijiiro `Title` so Japanese/localized names are populated. Fall back to the Green extracted placeholder item when Nijiiro lacks that ID.
- **Costumes:** keep only `(CostumeType, CostumeId)` pairs present in Green extracted data. For each pair, prefer the matching Nijiiro `Costume` so names are populated. Fall back to the Green item when Nijiiro lacks the pair. Unknown Green slot entries remain available only if they cannot be matched to a known typed Nijiiro item.
- **Neiros:** prefer Green extracted IDs when they are complete; if Green extraction has fewer than the Nijiiro base tone range, use Nijiiro IDs `0..19` as the Green tone catalog. Prefer Nijiiro names for matching IDs and fall back to Green placeholders.

`GreenEraGameDataCatalog` will receive an optional `INijiiroCatalog`. After loading the Green JSON catalogs, it will pass both Green and Nijiiro data through the composer. If Nijiiro is disabled, missing, or empty, Green keeps its current extracted placeholder behavior.

## WebUI

In `TaikoWebUI/Pages/Profile.razor`:

- Remove the `@if (!IsGreen)` guard around the player visualizer so Green uses the existing layered costume preview.
- Pass `ShowSwatches="true"` to `ColorPicker` for Green as well as Nijiiro.

This is safe because Green and Nijiiro share color IDs and the existing costume image assets cover the Green ranges.

## Tests

Follow TDD when implementation resumes:

1. Add a failing unit test for the composer proving that Green titles/costumes receive Nijiiro names but Nijiiro-only IDs are filtered out.
2. Add a failing unit test proving Green neiro composition includes Nijiiro tone IDs through `19` when the Green extracted tone-name catalog only has `0..15`.
3. Update the WebUI markup test so it expects the player visualizer and color swatches to be available for Green.
4. Run targeted tests:
   - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationCatalogLoaderTests Or FullyQualifiedName~GreenCustomizationWebUiTests Or FullyQualifiedName~GreenAdminApiControllerTests"`

## Resume Checklist

- Create the composer class and tests first.
- Inject optional `INijiiroCatalog` into `GreenEraGameDataCatalog`.
- Update dependency injection carefully so Green still works when Nijiiro is not enabled.
- Restore the WebUI preview and swatches.
- Verify generated Green JSON files remain ignored and are not committed.
