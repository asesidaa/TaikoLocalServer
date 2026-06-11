---
phase: 14-yellow-identity-userdata-crowns-self-best-and-normal-play
plan: "01"
subsystem: database
tags: [yellow, ac15, identity, ef-core, protobuf]
requires:
  - phase: 13-yellow-catalog-and-ac15-core-foundation
    provides: Yellow catalog/profile/core contracts and `/v09r00` metadata route ownership
provides:
  - Yellow-owned Phase 14 EF save, best, play-history, favorite, and recent-song tables
  - Yellow default save helper with AC15-compatible flag lengths and defaults
  - Mediator-backed Yellow `baidcheck.php` and `mydonentry.php` identity routes
  - Focused Yellow identity, persistence-boundary, and route-guard tests
affects: [yellow, phase-14, identity, userdata, normal-play]
tech-stack:
  added: []
  patterns: [era-owned EF slice, Yellow handler partials, Yellow wire mapper]
key-files:
  created:
    - Domain/Entities/UserSaveDataYellow.cs
    - Domain/Entities/SongBestDatumYellow.cs
    - Domain/Entities/SongPlayDatumYellow.cs
    - Domain/Entities/YellowFavoriteSongs.cs
    - Domain/Entities/YellowRecentSongs.cs
    - Application/Abstractions/ITaikoDbContext.Yellow.cs
    - Application/Common/UserSaveDataYellowExtensions.cs
    - Infrastructure/Persistence/TaikoDbContext.Yellow.cs
    - Infrastructure/Persistence/Migrations/20260607222143_AddYellowPhase14State.cs
    - Application/Handlers/BaidQuery.Yellow.cs
    - Application/Handlers/AddMyDonEntryCommand.Yellow.cs
    - Adapters.GameProtocol.Yellow/Mappers/BaidResponseMapper.cs
    - Tests/Yellow/YellowHandlerFixture.cs
    - Tests/Yellow/YellowPersistenceBoundaryTests.cs
    - Tests/Yellow/YellowIdentityHandlerTests.cs
  modified:
    - Infrastructure/Persistence/TaikoDbContext.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Application/Handlers/BaidQuery.cs
    - Application/Handlers/AddMyDonEntryCommand.cs
    - Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs
    - Tests/Yellow/YellowRouteSkeletonTests.cs
    - Tests/Yellow/YellowCatalogBoundaryTests.cs
    - Tests/Green/GreenAuthConfigTests.cs
key-decisions:
  - "Yellow identity uses shared Cards/UserData/Credentials only for identity; gameplay/profile state is physically Yellow-owned."
  - "Plan 14-01 replaces only Yellow BAID and mydon scaffolds; userdata, playresult, self-best, crowns, shop, Tokkun, Banacoin, AdminApi/WebUI, and battle behavior remain deferred."
patterns-established:
  - "Yellow EF partial mirrors Blue/Green table shape with Yellow table names and no discriminator/shared gameplay table."
  - "Yellow BAID/mydon use handler partial dispatch with adapter-local Yellow wire mapping."
requirements-completed: [YUSR-01]
duration: 72 min
completed: 2026-06-07
---

# Phase 14 Plan 01: Yellow Identity Foundation Summary

**Yellow-owned identity/default-save persistence with Mediator-backed BAID and mydon routes**

## Performance

- **Duration:** 72 min
- **Started:** 2026-06-07T21:21:43Z
- **Completed:** 2026-06-07T22:33:50Z
- **Tasks:** 2
- **Files modified:** 24

## Accomplishments

- Added Yellow-owned EF tables for save data, best rows, play history, favorites, and recent songs, plus the generated `AddYellowPhase14State` migration.
- Added `CreateDefaultYellowSaveData` / `GetOrCreateYellowSaveDataAsync` using AC15 shared limits and safe Yellow defaults.
- Replaced Yellow `baidcheck.php` and `mydonentry.php` no-state scaffolds with Mediator-backed identity behavior and Yellow wire mapping.
- Added focused tests proving Yellow identity creates/reads Yellow save state without Blue/Green/Nijiiro gameplay writes.

## Task Commits

1. **Task 1: Add Yellow EF state and default save helper** - `b12a24aa` (feat)
2. **Task 2: Replace Yellow BAID and mydon scaffolds with Mediator-backed identity behavior** - `9929109c` (feat)
3. **Verification guard fix:** `1987a8e5` (test)

**Plan metadata:** committed separately with this summary.

## Files Created/Modified

- `Domain/Entities/*Yellow.cs` - Yellow save, best, play-history, favorite, and recent-song entities.
- `Application/Abstractions/ITaikoDbContext.Yellow.cs` - Yellow Phase 14 DbSet contract.
- `Infrastructure/Persistence/TaikoDbContext.Yellow.cs` and migration files - Yellow table mappings and schema migration.
- `Application/Common/UserSaveDataYellowExtensions.cs` - Yellow default save creation helper.
- `Application/Handlers/BaidQuery.Yellow.cs` and `AddMyDonEntryCommand.Yellow.cs` - Yellow identity handler partials.
- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` and `Mappers/BaidResponseMapper.cs` - Yellow BAID/mydon route replacements and wire mapping.
- `Tests/Yellow/*Identity*`, `*PersistenceBoundary*`, route/boundary tests - Focused Phase 14 identity and guard coverage.

## Decisions Made

- Yellow gameplay/profile state stays physically separate from Blue, Green, and Nijiiro; only card/user/credential identity rows are shared.
- `TokkunTutorialFlg` exists as a nullable schema placeholder on the Yellow save row, but Plan 14-01 does not read or write Tokkun behavior.
- The stale Phase 13 Yellow boundary guard was updated to allow Phase 14 BAID/mydon and Yellow save tables while preserving deferred Phase 15/16 and battle guardrails.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated handwritten ITaikoDbContext test double**
- **Found during:** Task 1 verification
- **Issue:** `Tests/Green/GreenAuthConfigTests.ThrowingTaikoDbContext` failed to compile after `ITaikoDbContext.Yellow.cs` added five DbSet properties.
- **Fix:** Added throw-only Yellow DbSet members to the test double.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPersistenceBoundary|FullyQualifiedName~YellowIdentity"` passed.
- **Committed in:** `b12a24aa`

**2. [Rule 3 - Blocking] Advanced stale Phase 13 Yellow boundary tests**
- **Found during:** Plan-level Yellow regression
- **Issue:** `YellowCatalogBoundaryTests` still asserted that only Phase 13 metadata routes could call Mediator and that Yellow persistence files must not exist.
- **Fix:** Updated the guard to allow Phase 14 BAID/mydon and Yellow-owned Phase 14 persistence while continuing to forbid deferred shop, Tokkun, Banacoin wallet, and battle surfaces.
- **Files modified:** `Tests/Yellow/YellowCatalogBoundaryTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` passed.
- **Committed in:** `1987a8e5`

---

**Total deviations:** 2 auto-fixed (2 blocking). **Impact:** Both were required to keep existing guard/test infrastructure aligned with the intended Phase 14 boundary; no scope creep.

## Issues Encountered

- `node .codex\get-shit-done\bin\gsd-tools.cjs query state.advance-plan` could not parse the current `STATE.md` shape. Metadata was updated directly for the per-plan closeout.
- `roadmap.update-plan-progress 14` rewrote the Phase 14 summary row into a progress row before a summary existed. The row was restored and explicit 14-01 plan progress was added under Phase 14.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPersistenceBoundary|FullyQualifiedName~YellowIdentity"` - passed, 4 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowIdentity|FullyQualifiedName~YellowRouteSkeleton|FullyQualifiedName~YellowPersistenceBoundary"` - passed, 17 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` - passed, 87 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase14-yellow"` - passed, 0 warnings, 0 errors.
- `git diff --name-only` after production commits showed only pre-existing `Host/.gitignore` before metadata closeout.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `14-02-PLAN.md`. Yellow now has the save/profile state and identity routes needed for userdata, self-best, and crown readback. Phase-level verification/review and Phase 15 were not started.

---
*Phase: 14-yellow-identity-userdata-crowns-self-best-and-normal-play*
*Completed: 2026-06-07*
