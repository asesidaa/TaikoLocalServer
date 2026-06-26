---
phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
plan: "03"
subsystem: application
tags: [momoiro, ac15, metadata-handlers, catalog-readback]

requires:
  - phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
    provides: Momoiro root catalog, profile limits, and snapshot projection from 40-02
provides:
  - Momoiro Application dispatch for initial-data metadata
  - Momoiro Application dispatch for catalog-backed recommendations
  - Momoiro Application dispatch for missing-safe telop readback
affects: [momoiro-metadata-routes, phase-40-adapter-mappers]

tech-stack:
  added: []
  patterns:
    - Shared AC15 catalog snapshot readback through Momoiro typed catalog accessors
    - Application-layer behavior before adapter/controller route wiring

key-files:
  created:
    - Application/Handlers/GetInitialDataQuery.Momoiro.cs
    - Application/Handlers/GetRecommendQuery.Momoiro.cs
    - Application/Handlers/GetTelopQuery.Momoiro.cs
  modified:
    - Application/Handlers/GetInitialDataQuery.cs
    - Application/Handlers/GetRecommendQuery.cs
    - Application/Handlers/GetTelopQuery.cs
    - Tests/Ac15/Ac15RecommendQueryHandlerTests.cs

key-decisions:
  - "Momoiro metadata handlers use the typed Momoiro catalog and Ac15CatalogSnapshotFactory.FromMomoiro rather than controller-local catalog logic."
  - "Remaining Momoiro metadata route test failures are adapter/controller scaffold work assigned to 40-04."

patterns-established:
  - "Root-era Application metadata handlers should dispatch explicitly by GameEra and reuse shared AC15 readback services."

requirements-completed: [MOCAT-02, MOCAT-04]

duration: 6h 31m
completed: 2026-06-26
status: complete
---

# Phase 40 Plan 03: Momoiro Application Metadata Handler Dispatch Summary

**Momoiro Application handlers now read catalog-backed initial metadata, recommendations, and telops through shared AC15 services.**

## Performance

- **Duration:** 6h 31m, including checkpoint wait
- **Started:** 2026-06-25T23:23:30Z
- **Completed:** 2026-06-26T05:55:19Z
- **Tasks:** 3/3
- **Files modified:** 7

## Accomplishments

- Added `GameEra.Momoiro` dispatch for `GetInitialDataQuery`, backed by `Ac15InitialDataService.BuildCommonInitialData`.
- Added `GameEra.Momoiro` dispatch for `GetRecommendQuery`, backed by `Ac15CatalogReadbackService.BuildRecommendResponse`.
- Added `GameEra.Momoiro` dispatch for `GetTelopQuery`, backed by `Ac15CatalogReadbackService.BuildTelopResponse`.
- Kept behavior in Application handlers only; no identity, userdata, self-best, playresult, persistence, AdminApi, WebUI, route-family, or proto changes were added.

## Task Commits

1. **Task 1: Add Momoiro initial-data metadata handler** - `61911c10` (feat)
2. **Task 2: Add Momoiro recommendation handler** - `0531a290` (feat)
3. **Task 3: Add Momoiro telop handler** - `a01583e2` (feat)

## Files Created/Modified

- `Application/Handlers/GetInitialDataQuery.cs` - Adds Momoiro dispatch.
- `Application/Handlers/GetInitialDataQuery.Momoiro.cs` - Builds Momoiro common initial metadata from the Momoiro catalog snapshot and profile.
- `Application/Handlers/GetRecommendQuery.cs` - Adds Momoiro dispatch.
- `Application/Handlers/GetRecommendQuery.Momoiro.cs` - Builds Momoiro recommendations from the shared AC15 catalog readback service.
- `Application/Handlers/GetTelopQuery.cs` - Adds Momoiro dispatch.
- `Application/Handlers/GetTelopQuery.Momoiro.cs` - Builds Momoiro telop responses with missing-row success behavior.
- `Tests/Ac15/Ac15RecommendQueryHandlerTests.cs` - User-approved narrow fixture compatibility edit so the Momoiro recommendation test double implements `IMomoiroCatalog`.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests" --no-restore`
  - **Initial RED:** Failed as expected before handler work. Notable Application gaps: `GetTelopQuery is not implemented for era: Momoiro`; controller scaffold responses still omitted expected catalog-backed fields.
  - **After Task 3:** Failed with 4 remaining controller-scope failures in `RecommendController`, `DefaultSongController`, `SongHashController`, and `TelopCheckController`. The `GetTelopQuery_MomoiroMissingTelopReturnsSuccessWithOmittedFields` Application handler test passed. Remaining failures belong to Plan 40-04 adapter/controller and mapper work.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15RecommendQueryHandlerTests" --no-restore`
  - **Result:** Passed, 16/16.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests.GetTelopQuery_MomoiroMissingTelopReturnsSuccessWithOmittedFields" --no-restore`
  - **Result:** Passed, 1/1.
- `dotnet build Application/Application.csproj --no-restore`
  - **Result:** Passed, 0 warnings, 0 errors.
- `git status --short -- proto/momoiro`
  - **Result:** PASS, no output.

## Decisions Made

- Used `gameDataService.Momoiro()` and `Ac15CatalogSnapshotFactory.FromMomoiro(...)` in every Momoiro handler to preserve the typed catalog boundary created in 40-02.
- Kept route inventory boundaries in summary/plan documentation only; no `initialdatacheck.php` or unsupported route behavior was added.
- Treated the remaining metadata route failures as expected 40-04 adapter-controller work rather than weakening Application handler contracts.

## Deviations from Plan

### Auto-fixed Issues

None.

### User-Approved Scope Adjustment

**1. [Rule 3 - Blocking] Updated Momoiro recommendation test fixture for typed catalog verification**
- **Found during:** Task 2
- **Issue:** The planned handler uses `gameDataService.Momoiro()`, but `Tests/Ac15/Ac15RecommendQueryHandlerTests.cs` had a Momoiro fixture implementing only `IEraGameDataCatalog`, causing `InvalidCastException` before the recommendation behavior could be verified.
- **Fix:** After checkpoint approval, updated only `TestMomoiroCatalog` to implement `IMomoiroCatalog` and expose the minimal `MomoiroMusicInfos` and empty `Telops` members required by the contract.
- **Files modified:** `Tests/Ac15/Ac15RecommendQueryHandlerTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15RecommendQueryHandlerTests" --no-restore` passed, 16/16.
- **Committed in:** `0531a290`

### Workflow-Scope Adjustment

**1. [Scope Boundary] Skipped standard STATE/ROADMAP/REQUIREMENTS updates**
- **Found during:** Plan closeout
- **Issue:** Standard GSD closeout updates `.planning/STATE.md`, `.planning/ROADMAP.md`, and `.planning/REQUIREMENTS.md`.
- **Adjustment:** The active user instruction constrained work to plan files plus the required summary, with one approved fixture exception, so broader planning state files were not modified.
- **Files modified:** None.
- **Verification:** `git status --short` before summary creation showed only the pre-existing unrelated `Host/.gitignore`.

**Total deviations:** 1 user-approved blocking fix; 1 workflow-scope adjustment.
**Impact on plan:** Production handler scope is complete and verified at the Application layer. The remaining route-level red tests are expected handoff work for 40-04.

## Issues Encountered

- A parallel attempt to run two `dotnet test` commands caused transient `obj` file-lock build errors. Verification was rerun sequentially and the relevant focused tests/build passed.
- The full `MomoiroMetadataRouteTests` command remains red by design until 40-04 wires Momoiro adapter controllers and mappers to these Application handlers.

## Known Stubs

None in files modified by this plan. Existing Momoiro metadata controllers still return scaffold/static shapes and are intentionally deferred to 40-04.

## Threat Flags

None. This plan adds explicit Momoiro dispatch for catalog-backed Application behavior and does not introduce new network endpoints, auth paths, file access patterns, persistence, or schema changes.

## TDD Gate Compliance

This plan used Wave 0 RED contracts from 40-01. No new tests were added. The single test-file edit was a user-approved fixture compatibility patch needed to verify the already-planned typed Momoiro catalog handler contract.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for 40-04 to replace Momoiro metadata controller scaffolds with Mediator calls and Mapperly mappings over these Application handlers.

## Self-Check: PASSED

- Files exist: all 7 created/modified files plus this summary were verified on disk.
- Task commits exist: `61911c10`, `0531a290`, `a01583e2`.
- `proto/momoiro` remained untouched.
- Final working tree only has the pre-existing unrelated `Host/.gitignore` outside this summary before the summary commit.

---
*Phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior*
*Completed: 2026-06-26*
