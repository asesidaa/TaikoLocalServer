---
phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
plan: "02"
subsystem: catalog
tags: [momoiro, ac15, root-catalog, protocol-limits, song-hash]

requires:
  - phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
    provides: Wave 0 RED catalog/profile tests from 40-01
provides:
  - Reduced Momoiro catalog interface and root-level data path helpers
  - Enabled-era Momoiro catalog registration and root-level catalog loader
  - Explicit Momoiro AC15 profile limits and catalog snapshot projection
affects: [momoiro-catalog, ac15-profile, phase-40-metadata-routes]

tech-stack:
  added: []
  patterns:
    - KIMIDORI-style root-level AC15 catalog loading with reduced Momoiro surface
    - Explicit unsupported-feature disabling in Ac15EraProfiles.Momoiro

key-files:
  created:
    - Application/Abstractions/IMomoiroCatalog.cs
    - Infrastructure/GameDataCatalog/Momoiro/MomoiroGameDataPaths.cs
    - Infrastructure/GameDataCatalog/Momoiro/MomoiroRequiredDataFiles.cs
    - Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs
    - Host/wwwroot/data/momoiro/momoiro_telop_data.json
  modified:
    - Application/Common/CatalogExtensions.cs
    - Infrastructure/DependencyInjection.cs
    - Application/Ac15/Ac15EraProfiles.cs
    - Application/Ac15/Ac15CatalogSnapshotFactory.cs
    - Host/Host.csproj

key-decisions:
  - "Momoiro catalog authority is limited to music order, song hash, root required files, and telops for Phase 40."
  - "Momoiro profile uses userdata-owned crown placement and disables unsupported feature families expressible in the current AC15 feature model."
  - "The committed Momoiro telop sidecar is intentionally empty and copied with PreserveNewest while raw momoiro/data remains excluded."

patterns-established:
  - "Root-level older-AC15 catalog loaders can validate operator data while exposing only the era surfaces proven for the current phase."

requirements-completed: [MOCAT-01, MOCAT-02, MOCAT-03, MOCAT-05]

duration: 11 min
completed: 2026-06-25
status: complete
---

# Phase 40 Plan 02: Momoiro Root Catalog, Profile Limits, and Snapshot Projection Summary

**Root-level Momoiro catalog loading with explicit AC15 profile limits and reduced catalog authority.**

## Performance

- **Duration:** 11 min
- **Started:** 2026-06-25T23:08:45Z
- **Completed:** 2026-06-25T23:19:26Z
- **Tasks:** 3/3
- **Files modified:** 10

## Accomplishments

- Added `IMomoiroCatalog`, Momoiro root data paths, and required-file validation for `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- Implemented and registered the enabled-era Momoiro catalog loader, including music/tuning enrichment, medley parsing, song-hash table construction, and empty telop sidecar loading.
- Added explicit Momoiro AC15 feature/profile limits and `Ac15CatalogSnapshotFactory.FromMomoiro` with unsupported surfaces disabled or empty.

## Task Commits

1. **Task 1: Add Momoiro catalog contract and root data paths** - `a27f85e9` (feat)
2. **Task 2: Implement and register the root-level Momoiro catalog** - `7ade34d2` (feat)
3. **Task 3: Add explicit Momoiro profile and snapshot projection** - `9361b7be` (feat)

## Files Created/Modified

- `Application/Abstractions/IMomoiroCatalog.cs` - Reduced Momoiro catalog contract for Phase 40 catalog metadata.
- `Application/Common/CatalogExtensions.cs` - Adds `IGameDataCatalog.Momoiro()` multiplexer accessor.
- `Infrastructure/GameDataCatalog/Momoiro/MomoiroGameDataPaths.cs` - Root-level Momoiro operator-data paths.
- `Infrastructure/GameDataCatalog/Momoiro/MomoiroRequiredDataFiles.cs` - Required-file guard with concrete missing path reporting.
- `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs` - Root-level catalog loader and telop sidecar reader.
- `Infrastructure/DependencyInjection.cs` - Registers Momoiro catalog services only when Momoiro is enabled.
- `Application/Ac15/Ac15EraProfiles.cs` - Adds explicit Momoiro feature flags, protocol limits, and userdata crown placement.
- `Application/Ac15/Ac15CatalogSnapshotFactory.cs` - Adds Momoiro snapshot projection with empty unsupported surfaces.
- `Host/Host.csproj` - Copies `momoiro_telop_data.json` while preserving raw `momoiro/data/**` exclusion.
- `Host/wwwroot/data/momoiro/momoiro_telop_data.json` - Empty server-authored telop sidecar.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroCatalogLoaderTests" --no-restore`
  - **Initial RED:** Failed as expected before implementation with `Era Momoiro is not enabled`.
  - **Final Task 2:** Passed, 2/2.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroProtocolLimitsTests|FullyQualifiedName~MomoiroCatalogLoaderTests" --no-restore`
  - **Result:** Passed, 5/5.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`
  - **Result:** Passed, 0 warnings, 0 errors.
  - **Output check:** `wwwroot\data\momoiro\momoiro_telop_data.json` exists in the temp output and contains `[]`.
- `dotnet build Infrastructure/Infrastructure.csproj --no-restore`
  - **Result:** Passed, 0 warnings, 0 errors.
- `git status --porcelain -- proto\momoiro`
  - **Result:** PASS, no output.

## Decisions Made

- Used the existing KIMIDORI root-layout loader pattern but kept the Momoiro interface reduced to Phase 40 needs.
- Parsed `musicmedleyinfo.xml` during initialization for validation/future catalog data without exposing Taikojuku or Dani catalog authority from `IMomoiroCatalog`.
- Modeled Momoiro crown support as userdata-owned profile metadata, not a standalone crown route or mutation path.

## Deviations from Plan

### Auto-fixed Issues

None.

### Workflow-Scope Adjustment

**1. [Scope Boundary] Skipped standard STATE/ROADMAP/REQUIREMENTS updates**
- **Found during:** Plan closeout
- **Issue:** Standard GSD closeout updates `.planning/STATE.md`, `.planning/ROADMAP.md`, and `.planning/REQUIREMENTS.md`.
- **Adjustment:** The current user instruction explicitly limited work to the plan files plus `40-02-SUMMARY.md`, so broader planning state files were not modified.
- **Files modified:** None.
- **Verification:** `git status --short --untracked-files=all` shows only the pre-existing unrelated `Host/.gitignore` after task commits before summary creation.

**Total deviations:** 0 auto-fixed; 1 workflow-scope adjustment.
**Impact on plan:** Production scope and summary are complete. Project-level planning counters remain for the orchestrator or an explicit closeout pass.

## Issues Encountered

None.

## Known Stubs

None. The empty Momoiro telop sidecar is intentional Phase 40 catalog data, not an unwired UI or runtime placeholder.

## Threat Flags

None. The new filesystem catalog surface matches the plan threat model and is covered by required-file validation and focused catalog tests.

## TDD Gate Compliance

This plan used the Wave 0 RED contracts committed by `40-01`; no new test files were added because the current allowed file set was production files plus this summary.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for 40-03 to wire Momoiro Application metadata handler dispatch against `Ac15CatalogSnapshotFactory.FromMomoiro` without adding persistence, AdminApi, WebUI, or unsupported route families.

## Self-Check: PASSED

- Files exist: all 10 plan files plus this summary were verified on disk.
- Task commits exist: `a27f85e9`, `7ade34d2`, `9361b7be`.
- `proto/momoiro` remained untouched.
- Final working tree only has the pre-existing unrelated `Host/.gitignore` outside this summary before the summary commit.

---
*Phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior*
*Completed: 2026-06-25*
