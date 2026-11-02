---
phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
plan: "04"
subsystem: adapter
tags: [momoiro, ac15, metadata-routes, mapperly, catalog-readback]

requires:
  - phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
    provides: Momoiro root catalog, profile limits, snapshot projection, and Application metadata handlers from 40-02 and 40-03
provides:
  - Momoiro Mapperly mappings for recommendation and telop wire responses
  - Catalog-backed Momoiro recommend.php, defaultsong.php, songhash.php, telopcheck.php, and gettelop.php controllers
  - Explicit static operational semantics for Momoiro heartbeat.php and bookkeeping.php
affects: [momoiro-metadata-routes, momoiro-route-surface, phase-40-verification]

tech-stack:
  added: []
  patterns:
    - Mapperly source-generated common DTO to Momoiro wire response mappings
    - Thin Momoiro controllers using Mediator and typed catalog access for metadata routes
    - Static operational stubs only where route behavior is intentionally no-state

key-files:
  created:
    - Adapters.GameProtocol.Momoiro/Mappers/RecommendMappers.cs
    - Adapters.GameProtocol.Momoiro/Mappers/GetTelopMappers.cs
  modified:
    - Adapters.GameProtocol.Momoiro/GlobalUsings.cs
    - Adapters.GameProtocol.Momoiro/Controllers/RecommendController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/DefaultSongController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/SongHashController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/TelopCheckController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/GetTelopController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/HeartbeatController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/BookkeepingController.cs

key-decisions:
  - "Momoiro metadata controllers now call the 40-03 Application handlers and typed Momoiro catalog instead of returning Phase 39 scaffold responses."
  - "heartbeat.php and bookkeeping.php remain no-state operational compatibility routes and only log request metadata."
  - "Direct controller tests bypass Host startup, so catalog-backed controllers guard against empty Momoiro song-hash state by initializing the existing IGameDataCatalog abstraction before metadata readback."

patterns-established:
  - "Momoiro adapter response projection should stay Mapperly-generated, with helper conversions only for wire presence/null differences."
  - "Momoiro metadata controllers may initialize the existing catalog abstraction when direct invocation has not run Host startup, but do not read files or mutate persistence in controllers."

requirements-completed: [MOCAT-02, MOCAT-04, MOCAT-05]

duration: 13 min
completed: 2026-06-26
status: complete
---

# Phase 40 Plan 04: Momoiro Catalog-Backed Metadata Controllers and Mapperly Mappers Summary

**Momoiro metadata routes now return catalog-backed recommendation, song-hash, default-song, and telop responses through Application handlers and Mapperly-generated wire mappings.**

## Performance

- **Duration:** 13 min
- **Started:** 2026-06-26T06:00:27Z
- **Completed:** 2026-06-26T06:13:32Z
- **Tasks:** 3/3
- **Files modified:** 10

## Accomplishments

- Added Momoiro `RecommendMappers` and `GetTelopMappers` as Mapperly partial mapper declarations.
- Replaced no-state `recommend.php`, `defaultsong.php`, and `songhash.php` scaffolds with catalog-backed behavior.
- Replaced no-state `telopcheck.php` and `gettelop.php` scaffolds with Momoiro Application handler calls.
- Preserved `heartbeat.php` and `bookkeeping.php` as explicit static success routes with operational compatibility log wording.
- Left `proto/momoiro` untouched and did not add identity, userdata, self-best, playresult mutation, AdminApi, WebUI, crown endpoint, or unsupported route families.

## Task Commits

1. **Task 1: Add Momoiro Mapperly mappings** - `1d6d90a9` (feat)
2. **Task 2: Wire catalog-backed recommendation/default/song-hash controllers** - `d551c439` (feat)
3. **Task 3: Wire telop routes and keep operational routes static** - `07111d5f` (feat)

## Files Created/Modified

- `Adapters.GameProtocol.Momoiro/GlobalUsings.cs` - Adds Momoiro mapper and wire global imports.
- `Adapters.GameProtocol.Momoiro/Mappers/RecommendMappers.cs` - Maps `CommonRecommendResponse` to Momoiro `RecommendResponse`.
- `Adapters.GameProtocol.Momoiro/Mappers/GetTelopMappers.cs` - Maps `CommonGetTelopResponse` to Momoiro `GetTelopResponse` while omitting missing optional strings.
- `Adapters.GameProtocol.Momoiro/Controllers/RecommendController.cs` - Sends `GetRecommendQuery(GameEra.Momoiro, request.GenderType, request.PlayerAge)` and maps the common response.
- `Adapters.GameProtocol.Momoiro/Controllers/DefaultSongController.cs` - Sends `GetInitialDataQuery(GameEra.Momoiro)` and compacts default-song flags through the Momoiro hash table.
- `Adapters.GameProtocol.Momoiro/Controllers/SongHashController.cs` - Reads `IGameDataCatalog.Momoiro()` and returns song hash version plus encoded hash table.
- `Adapters.GameProtocol.Momoiro/Controllers/TelopCheckController.cs` - Sends `GetInitialDataQuery(GameEra.Momoiro)` and returns catalog telop IDs.
- `Adapters.GameProtocol.Momoiro/Controllers/GetTelopController.cs` - Sends `GetTelopQuery(GameEra.Momoiro, request.TelopId)` and maps the common response.
- `Adapters.GameProtocol.Momoiro/Controllers/HeartbeatController.cs` - Keeps static success semantics.
- `Adapters.GameProtocol.Momoiro/Controllers/BookkeepingController.cs` - Keeps static success semantics without accounting state.

## Verification

- `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore`
  - **Result:** Passed, 0 warnings, 0 errors.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests|FullyQualifiedName~Ac15RecommendQueryHandlerTests" --no-restore`
  - **Result:** Passed, 22/22.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests|FullyQualifiedName~MomoiroRouteSurfaceTests" --no-restore`
  - **Result:** Passed, 11/11.
- `git status --short -- proto/momoiro`
  - **Result:** PASS, no output.

## Mapperly Generated Source Inspection

- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/RecommendMappers.g.cs`
  - Generated assignments set `Result`, `RecommendSong`, and `RecommendBestSongs` from the common recommendation DTO.
- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/GetTelopMappers.g.cs`
  - Generated assignments set `Result`, `StartDatetime`, `EndDatetime`, and `Telop`; no `VerupNo` target assignment is generated.

## Decisions Made

- Used Mapperly `[MapProperty]` and `[MapperIgnoreSource]` for Momoiro wire naming and source-only telop data instead of handwritten mapping bodies.
- Used `IGameDataCatalog.Momoiro()` and shared AC15 codecs for default-song and song-hash route bytes, avoiding handler-local filesystem access.
- Kept static heartbeat/bookkeeping behavior as the intentional route semantics rather than treating them as unfinished scaffolds.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added catalog initialization guard for direct controller invocation**
- **Found during:** Task 2 (`RecommendController_ReturnsCatalogBackedRecommendation`, `DefaultSongController_ReturnsSongHashVersionAndCompactedFlagBody`, `SongHashController_ReturnsSongHashVersionAndEncodedTable`)
- **Issue:** The route tests instantiate controllers directly and bypass Host startup, so catalog-backed controllers initially observed the uninitialized Momoiro catalog and returned zero song-hash/recommendation data.
- **Fix:** Added a narrow guard in catalog-backed Momoiro controllers that calls the existing `IGameDataCatalog.InitializeAsync` abstraction only when the Momoiro song-hash table is empty.
- **Files modified:** `RecommendController.cs`, `DefaultSongController.cs`, `SongHashController.cs`, `TelopCheckController.cs`, `GetTelopController.cs`
- **Verification:** Both focused Momoiro metadata route commands passed after the guard.
- **Committed in:** `d551c439`, `07111d5f`

**Total deviations:** 1 auto-fixed blocking issue.
**Impact on plan:** The fix stays inside the adapter/controller files, uses the existing catalog abstraction, and does not add filesystem access, persistence, AdminApi, WebUI, or unsupported route behavior.

### Workflow-Scope Adjustment

**1. [Scope Boundary] Skipped standard STATE/ROADMAP/REQUIREMENTS updates**
- **Found during:** Plan closeout
- **Issue:** Standard GSD closeout updates `.planning/STATE.md`, `.planning/ROADMAP.md`, and `.planning/REQUIREMENTS.md`.
- **Adjustment:** The active user instruction constrained work to the plan files plus the required summary, so broader planning state files were not modified.
- **Files modified:** None.
- **Verification:** `git status --short` showed only the new summary plus the pre-existing unrelated `Host/.gitignore` before summary commit.

## Issues Encountered

- The first full Task 2 verification command failed with one remaining `TelopCheckController` scaffold failure, which was expected Task 3 scope. After Task 3, the same command passed 22/22.

## Known Stubs

None in files created or modified by this plan.

## Threat Flags

None. Existing binary-proven routes were upgraded or clarified; no new network endpoints, auth paths, file access patterns, persistence, schema changes, crown endpoint, or unsupported route families were introduced.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for 40-05 final Phase 40 verification and source-audit gates. The route behavior now matches the planned split: catalog-backed metadata where data exists, and explicit static success for heartbeat/bookkeeping.

## Self-Check: PASSED

- Summary exists: `.planning/phases/40-momoiro-protocol-limits-root-catalog-and-route-behavior/40-04-SUMMARY.md`.
- Created mapper files exist: `RecommendMappers.cs`, `GetTelopMappers.cs`.
- Task commits exist: `1d6d90a9`, `d551c439`, `07111d5f`.
- `proto/momoiro` remained untouched.
- Working tree after summary creation contains this untracked summary and the pre-existing unrelated `Host/.gitignore`.

---
*Phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior*
*Completed: 2026-06-26*
