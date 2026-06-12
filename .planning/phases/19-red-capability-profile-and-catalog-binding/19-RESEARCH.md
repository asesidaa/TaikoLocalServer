# Phase 19: Red Capability Profile and Catalog Binding - Research

**Researched:** 2026-06-13
**Domain:** Red AC15 catalog/profile binding
**Confidence:** High for route/root/catalog availability and shared parser compatibility; medium for any Red runtime sequence not yet manually smoked beyond Phase 18 basic connection.

## Summary

Phase 19 should be implemented as Red binding around existing AC15 catalog mechanisms. Phase 18 proved `/v08r01` game routes, shared `/v01r00` startup/version, and active `ST8100-1` Red runtime root. The active Red data root contains `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, `fumen/tuning.bin`, and `movie/`, which are enough to bind the same catalog capabilities already used by Green/Blue/Yellow where the file shape matches.

The current codebase already has shared AC15 loaders and services:
- `Ac15MusicInfoLoader`, `Ac15TuningLoader`, `Ac15TaikojukuLoader`
- `Ac15EventFolderLoader`, `Ac15TelopLoader`, `Ac15RecommendLoader`, `Ac15MovieLoader`
- `Ac15CustomizationSourceParser`, `Ac15CustomizationCatalogLoader`, `Ac15CustomizationCatalogComposer`
- `Ac15CatalogSnapshotFactory`, `Ac15InitialDataService`, `Ac15CatalogReadbackService`, `Ac15TaikojukuService`

## Red Evidence Used

| Evidence | Finding | Impact |
|----------|---------|--------|
| Phase 18 `18-RED-EVIDENCE.md` | Red game routes use `/v08r01`; startup/version remains `/v01r00`; active config root is `ST8100-1`. | Red path helper should bind `config/ST8100-1` and startup movie lookup should map HDD major version 8 to Red. |
| IDA `.tools/red/EBOOT.ELF.i64`, `sub_1C0004` | `OnCrownsDataResponse` uses `li r5, 0x400` and decodes 10-bit crown values. | Red can use `CrownSongCount = 1024` and `CrownPackedBytes = 1280` through the shared AC15 crown code. |
| IDA `.tools/red/EBOOT.ELF.i64`, `sub_1BDCFC` | `OnUserDataResponse` copies 128-byte and 16-byte protocol arrays into player state. | Red can use the shared song/title/tone flag byte limits where those same protocol fields are bound. |
| IDA `.tools/red/EBOOT.ELF.i64`, `sub_D2620` | `OnTaikoJukuResponse` branches when the song count is greater than 9. | Red can use `MaxSongsPerTaikojukuPack = 10`. |
| Local `ST8100-1/musicmedleyinfo.xml` | Challenge levels include normal levels starting at 1 and extra levels through 113. | Existing AC15 normal/extra Dan range model is suitable for Red catalog filtering. |

## Implementation Direction

1. Add Red catalog abstractions and loaders with shared AC15 record types where no Red-specific fields exist.
2. Promote generic customization extraction out of the Blue-only extractor so Red can reuse it without copying JSON write/extract code.
3. Add `Ac15EraProfiles.Red` with item shop disabled and the evidence-backed Red limits.
4. Add Red catalog-backed partial handlers and map only Phase 19 route probes to Mediator-backed catalog readback.
5. Add focused tests for Red loader output, Red profile flags/limits, Red startup movie resolution, and Red metadata protocol mapping.

## Non-Goals

- No Red EF tables, migrations, userdata state, score writes, Dani writes, Tokkun writes, or ChallengeCompe state.
- No Red item-shop, medals, wallet/payment/coupon/transaction behavior.
- No Red AdminApi or WebUI.

## Validation

Run focused Red/AC15 tests after implementation:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15|FullyQualifiedName~StartupMovie"
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Before Phase 19 close, run the full test suite if the shared AC15 profile/catalog code changed.
