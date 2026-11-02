---
phase: 13-yellow-catalog-and-ac15-core-foundation
plan: 01
subsystem: catalog
tags: [yellow, ac15, catalog, infrastructure, tests]

requires:
  - phase: 12-yellow-evidence-and-era-foundation
    provides: Yellow era enum, adapter scaffold, `/v09r00/chassis` route boundary, and no-battle guardrails
provides:
  - Yellow-owned catalog contract and DTOs
  - PathHelper-backed Yellow `ST9100-1` required data paths
  - Yellow runtime catalog initialization and enabled-era DI registration
  - Focused Yellow catalog, loader, and registration tests
affects: [Phase 13, Phase 14, Phase 15, Yellow catalog, AC15 shared core]

tech-stack:
  added: []
  patterns: [era-owned catalog wrappers over shared AC15 loaders, enabled-era catalog DI, optional sidecar empty fallback]

key-files:
  created:
    - Application/Abstractions/IYellowCatalog.cs
    - Application/Catalog/Yellow/YellowMusicInfoEntry.cs
    - Application/Catalog/Yellow/YellowTaikojukuEntry.cs
    - Application/Catalog/Yellow/YellowItemShopCatalog.cs
    - Infrastructure/GameDataCatalog/Yellow/YellowEraGameDataCatalog.cs
    - Infrastructure/GameDataCatalog/Yellow/YellowGameDataPaths.cs
    - Infrastructure/GameDataCatalog/Yellow/YellowRequiredDataFiles.cs
    - Tests/Yellow/YellowCatalogContractTests.cs
    - Tests/Yellow/YellowCatalogLoaderTests.cs
    - Tests/Yellow/YellowInfrastructureRegistrationTests.cs
  modified:
    - Application/Common/CatalogExtensions.cs
    - Infrastructure/DependencyInjection.cs

key-decisions:
  - "Yellow catalog contracts remain era-owned while wrapping shared AC15 loader output."
  - "Yellow requires `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`; optional sidecars remain empty or disabled when absent."
  - "Yellow `musicmedleyinfo.xml` is normalized through a temporary loader copy when the final medley entry is missing its closing tag; source operator data is not edited."

patterns-established:
  - "Yellow catalog data paths resolve through `PathHelper.GetDataPath(GameEra.Yellow)` and `ST9100-1` config roots."
  - "Yellow runtime catalog registration is gated by `enabledEras.Contains(GameEra.Yellow)`."

requirements-completed: [YCAT-01, YCAT-02]

duration: 45 min
completed: 2026-06-08
---

# Phase 13 Plan 01: Yellow Catalog Foundation Summary

**Yellow-owned catalog contracts and runtime loaders for `ST9100-1` operator data with enabled-era DI registration**

## Performance

- **Duration:** 45 min
- **Started:** 2026-06-08T03:10:00+08:00
- **Completed:** 2026-06-08T03:55:08+08:00
- **Tasks:** 2
- **Files modified:** 28

## Accomplishments

- Added `IYellowCatalog`, Yellow catalog DTOs, `CatalogExtensions.Yellow()`, and Yellow runtime catalog initialization.
- Added `YellowGameDataPaths` and `YellowRequiredDataFiles` for `ST9100-1` `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- Added Yellow wrapper loaders over shared AC15 parsers plus empty/disabled optional sidecar behavior.
- Registered `YellowEraGameDataCatalog` only when Yellow is enabled.
- Added focused catalog contract, loader, optional-sidecar, runtime initialization, and DI registration tests.

## Task Commits

1. **Task 1/2: Yellow catalog contracts, paths, and loader wrappers** - `6177d077` (feat)
2. **Task 2/2: Yellow runtime catalog registration and tests** - `6177d077` (feat)

## Files Created/Modified

- `Application/Abstractions/IYellowCatalog.cs` - Yellow-owned era catalog interface.
- `Application/Catalog/Yellow/*.cs` - Yellow DTOs for music, Taikojuku, item shop, telop, recommend, gacha, and tournament rows.
- `Application/Common/CatalogExtensions.cs` - Added `Yellow()` accessor.
- `Infrastructure/GameDataCatalog/Yellow/*.cs` - Yellow paths, required-file checks, loader wrappers, and runtime catalog.
- `Infrastructure/DependencyInjection.cs` - Added enabled-era Yellow catalog registrations.
- `Tests/Yellow/YellowCatalogContractTests.cs` - Required path and contract tests.
- `Tests/Yellow/YellowCatalogLoaderTests.cs` - Required data loading, optional fallback, and runtime initialization tests.
- `Tests/Yellow/YellowInfrastructureRegistrationTests.cs` - Enabled/disabled DI registration tests.

## Decisions Made

- Yellow catalog files use Yellow DTOs and `IYellowCatalog`; no Blue/Green catalog types are exposed through Yellow production contracts.
- Optional Yellow sidecars such as telop, event folder, recommend, movie, and item-shop JSON are not required for startup.
- Local Yellow `musicmedleyinfo.xml` has a malformed final entry in the operator data; the loader repairs only that missing final close tag in a temporary copy.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Normalized Yellow medley XML final entry for parser compatibility**
- **Found during:** Task 1/2 focused loader verification
- **Issue:** `musicmedleyinfo.xml` failed `XDocument.LoadAsync` because the final `<MusicMedleyInfoData>` block is missing `</MusicMedleyInfoData>` before `</boost_serialization>`.
- **Fix:** `YellowTaikojukuLoader` creates a temporary repaired copy only when the last medley open tag is after the last medley close tag before the root close.
- **Files modified:** `Infrastructure/GameDataCatalog/Yellow/YellowTaikojukuLoader.cs`
- **Verification:** Focused Yellow catalog tests pass.
- **Committed in:** `6177d077`

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Required for local Yellow operator data to load; no source data, shared AC15 loader, persistence, or route behavior was changed.

## Issues Encountered

- First focused test run failed on Yellow medley XML parsing. Root cause was traced to the missing final medley close tag in local Yellow operator data; the second focused run passed after loader-side normalization.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowCatalogContractTests|FullyQualifiedName~YellowCatalogLoaderTests|FullyQualifiedName~YellowInfrastructureRegistrationTests"` - passed, 11/11.
- `git diff --check -- Application Infrastructure Tests` - passed.
- Source scope inspection found no Yellow EF entities, migrations, gameplay writes, AdminApi/WebUI code, Tokkun, Banacoin wallet/payment, or battle files in this plan.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `13-02`: Yellow profile, catalog snapshot bridge, initial-data, and Taikojuku application handlers can build on `IYellowCatalog` and `YellowEraGameDataCatalog`.

---
*Phase: 13-yellow-catalog-and-ac15-core-foundation*
*Completed: 2026-06-08*
