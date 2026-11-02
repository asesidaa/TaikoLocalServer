---
phase: 41-momoiro-identity-userdata-self-best-and-crown-readback
plan: "02"
subsystem: database
tags: [momoiro, ac15, ef-core, sqlite, persistence, userdata, selfbest, crown-readback]

requires:
  - phase: 41-momoiro-identity-userdata-self-best-and-crown-readback
    provides: Wave 0 RED contracts for future Momoiro readback tables
provides:
  - Momoiro-owned save, best, favorite, and recent domain entities
  - Momoiro default save helper using Ac15EraProfiles.Momoiro limits
  - Momoiro DbContext and ITaikoDbContext port surfaces
  - EF migration creating UserSaveData_Momoiro, SongBestDatum_Momoiro, MomoiroFavoriteSongs, and MomoiroRecentSongs
affects: [phase-41, phase-42, momoiro-readback, momoiro-persistence]

tech-stack:
  added: []
  patterns:
    - Era-owned AC15 persistence partials for Momoiro readback schema
    - Favorite ordering persisted with DisplayOrder while preserving BAID/SongNo uniqueness

key-files:
  created:
    - Domain/Entities/UserSaveDataMomoiro.cs
    - Domain/Entities/SongBestDatumMomoiro.cs
    - Domain/Entities/MomoiroFavoriteSongs.cs
    - Domain/Entities/MomoiroRecentSongs.cs
    - Application/Common/UserSaveDataMomoiroExtensions.cs
    - Application/Abstractions/ITaikoDbContext.Momoiro.cs
    - Infrastructure/Persistence/TaikoDbContext.Momoiro.cs
    - Infrastructure/Persistence/Migrations/20260626082924_AddMomoiroReadbackState.cs
    - Infrastructure/Persistence/Migrations/20260626082924_AddMomoiroReadbackState.Designer.cs
  modified:
    - Infrastructure/Persistence/TaikoDbContext.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Tests/Green/GreenAuthConfigTests.cs

key-decisions:
  - "Momoiro favorite order is persisted as DisplayOrder and indexed by (Baid, DisplayOrder), while the unique row identity remains (Baid, SongNo)."
  - "Momoiro readback schema adds only the four Phase 41 tables; no play-history, dedicated Dan, Tokkun, battle, ChallengeCompe, Don Challenge, shop-season, AdminApi/WebUI, or proto surfaces were added."
  - "The generated EF Designer full-model metadata was not treated as the migration delta; the active migration body and ModelSnapshot diff were inspected for Momoiro scope."

patterns-established:
  - "Momoiro persistence follows adjacent AC15 era partials but omits mutation-only dedicated tables in this plan."
  - "Generated migration-body table extraction is used when broad rg checks hit EF Designer full-model false positives."

requirements-completed: [MORDB-01, MORDB-02, MORDB-03, MORDB-04, MORDB-05]
requirements-note: "Plan 41-02 completes the persistence/schema slice for these runtime requirements; handler/controller readback behavior remains assigned to later Phase 41 plans."

duration: 12min
completed: 2026-06-26
status: complete
---

# Phase 41 Plan 02: Momoiro Readback Persistence Summary

**Momoiro-owned EF persistence for userdata defaults, self-best/crown source rows, ordered favorites, and recents**

## Performance

- **Duration:** 12 min
- **Started:** 2026-06-26T08:22:54Z
- **Completed:** 2026-06-26T08:34:35Z
- **Tasks:** 2/2
- **Files modified:** 12 implementation/schema/test-helper files plus this summary

## Accomplishments

- Added `UserSaveDataMomoiro`, `SongBestDatumMomoiro`, `MomoiroFavoriteSongs`, and `MomoiroRecentSongs` domain rows.
- Added `CreateDefaultMomoiroSaveData` using `Ac15EraProfiles.Momoiro.Limits`, including the Momoiro release-song byte envelope and `DateTime.UnixEpoch` last-play default.
- Added Momoiro ITaikoDbContext and TaikoDbContext partial surfaces with BAID cascade FKs to shared `UserData`.
- Generated `AddMomoiroReadbackState` migration creating exactly `UserSaveData_Momoiro`, `SongBestDatum_Momoiro`, `MomoiroFavoriteSongs`, and `MomoiroRecentSongs`.
- Configured `MomoiroFavoriteSongs.DisplayOrder` with an `(Baid, DisplayOrder)` index while preserving `(Baid, SongNo)` as the primary key.

## Task Commits

1. **Task 1: Add Momoiro readback entities and defaults** - `329a5409` (`feat`)
2. **Task 2: Add DbContext surfaces and EF migration** - `e893c74f` (`feat`)

## Files Created/Modified

- `Domain/Entities/UserSaveDataMomoiro.cs` - Momoiro-owned save/readback row.
- `Domain/Entities/SongBestDatumMomoiro.cs` - Momoiro-owned self-best and crown source row keyed by BAID, song, difficulty, and shin flag.
- `Domain/Entities/MomoiroFavoriteSongs.cs` - Momoiro favorite row with persisted display order.
- `Domain/Entities/MomoiroRecentSongs.cs` - Momoiro recent-song row with `LastPlayed`.
- `Application/Common/UserSaveDataMomoiroExtensions.cs` - Momoiro default save helper using Momoiro AC15 limits.
- `Application/Abstractions/ITaikoDbContext.Momoiro.cs` - Momoiro port DbSets.
- `Infrastructure/Persistence/TaikoDbContext.cs` - Calls `OnModelCreatingMomoiro` after Kimidori and before `OnModelCreatingPartial`.
- `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs` - Momoiro DbSets and EF table configuration.
- `Infrastructure/Persistence/Migrations/20260626082924_AddMomoiroReadbackState.cs` - Active migration body for the four readback tables.
- `Infrastructure/Persistence/Migrations/20260626082924_AddMomoiroReadbackState.Designer.cs` - EF generated migration metadata.
- `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` - EF snapshot with Momoiro readback entities.
- `Tests/Green/GreenAuthConfigTests.cs` - Existing throwing test DbContext updated for new Momoiro port properties.

## Verification

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests" --no-restore -- RunConfiguration.DisableParallelization=true` before implementation | RED as expected: compiled, 4/4 failed on `Unsupported era: Momoiro`. |
| `dotnet build Domain/Domain.csproj --no-restore` | PASS: build succeeded, 0 warnings, 0 errors, elapsed 00:00:00.79. |
| `dotnet build Application/Application.csproj --no-restore` | PASS: build succeeded, 0 warnings, 0 errors, elapsed 00:00:02.77. |
| `dotnet build Infrastructure/Infrastructure.csproj --no-restore` before migration | PASS: build succeeded, 0 warnings, 0 errors, elapsed 00:00:05.29. |
| `dotnet ef migrations add AddMomoiroReadbackState --project Infrastructure --startup-project Host` | PASS: build started, build succeeded, EF logged version/startup/shutdown, generated `20260626082924_AddMomoiroReadbackState`. |
| `dotnet build Infrastructure/Infrastructure.csproj --no-restore` after migration | PASS: build succeeded, 0 warnings, 0 errors, elapsed 00:00:03.58. |
| Literal plan grep: `$hits = rg -n "...pattern..." Infrastructure/Persistence/Migrations/*AddMomoiroReadbackState*.cs Infrastructure/Persistence/TaikoDbContext.Momoiro.cs; ...` | Exit 2 on PowerShell/Windows: `rg` received the slash-wildcard path as an invalid filename (`os error 123`). |
| PowerShell-expanded equivalent grep over `*AddMomoiroReadbackState*.cs` and `TaikoDbContext.Momoiro.cs` | Exit 1 because EF Designer full-model metadata contains pre-existing non-Momoiro `BlueBattle*`, `*Tokkun*`, `Red/WhiteDonChallenge*`, and `IsChallengeCompe` entries. No hits were in `TaikoDbContext.Momoiro.cs` or the active migration body. |
| Active migration-body check for unsupported Momoiro mutation tables | PASS: negative grep exit 1 for `SongPlayDatum_Momoiro`, `DanScoreDatum_Momoiro`, `DanStageScoreDatum_Momoiro`, `Tokkun`, `Battle`, `ChallengeCompe`, and `DonChallenge` across `20260626082924_AddMomoiroReadbackState.cs` and `TaikoDbContext.Momoiro.cs`. |
| Active migration table extraction | PASS: `CreateTable` and `DropTable` exactly matched `MomoiroFavoriteSongs`, `MomoiroRecentSongs`, `SongBestDatum_Momoiro`, and `UserSaveData_Momoiro`. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests" --no-restore -- RunConfiguration.DisableParallelization=true` after implementation | Expected later-plan RED: compiled, 4/4 failed only on `Unsupported era: Momoiro` in handler dispatch. The schema/port compile blocker is resolved. |
| `git status --porcelain -- proto\momoiro` | PASS: no proto changes. |
| `git diff --cached --name-only -- Host/.gitignore` | PASS: no staged `Host/.gitignore` changes. |

## Migration Inspection

- `20260626082924_AddMomoiroReadbackState.cs` creates only four tables: `MomoiroFavoriteSongs`, `MomoiroRecentSongs`, `SongBestDatum_Momoiro`, and `UserSaveData_Momoiro`.
- Every table has a BAID FK to shared `UserData` with cascade delete.
- `SongBestDatum_Momoiro` uses `(Baid, SongId, Difficulty, IsShin)` as the primary key and converts `Difficulty`/`BestCrown` as unsigned integers.
- `MomoiroFavoriteSongs` uses `(Baid, SongNo)` as the primary key and adds `IX_MomoiroFavoriteSongs_Baid_DisplayOrder`.
- `TaikoDbContextModelSnapshot.cs` diff adds only Momoiro entity blocks, including `DisplayOrder` and the `(Baid, DisplayOrder)` index.

## Decisions Made

- Persist favorite display order explicitly because Phase 41 readback must return `ary_favorite_song_no` by user-facing order, not native song-number sorting.
- Keep dedicated mutation-only state out of the persistence slice; no Momoiro play-history, Dan score/stage, Tokkun, battle, ChallengeCompe, Don Challenge, shop-season, AdminApi/WebUI, or proto files were added.
- Treat EF Designer unsupported-term hits as full-model metadata, not active migration delta, and verify the active migration body separately.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated ITaikoDbContext test fake for Momoiro port additions**
- **Found during:** Task 2 verification
- **Issue:** The focused test command failed to compile because `GreenAuthConfigTests.ThrowingTaikoDbContext` implements `ITaikoDbContext` and lacked the new Momoiro DbSet properties.
- **Fix:** Added four throwing Momoiro properties to the existing test fake.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** Re-ran the focused test command; it compiled and failed only on expected unimplemented Momoiro handler dispatch.
- **Committed in:** `e893c74f`

**Total deviations:** 1 auto-fixed blocking issue.
**Impact on plan:** No production scope expansion; the fix keeps existing tests compiling after the new port surface.

### Workflow-Scope Adjustments

**1. Skipped `requirements.mark-complete` for MORDB runtime requirements**
- **Found during:** Plan closeout
- **Issue:** Standard GSD closeout can mark plan frontmatter requirements complete, but this plan implements only the persistence/schema slice and leaves handler/controller readback behavior for later Phase 41 plans.
- **Adjustment:** Updated plan progress and session metadata, but left `.planning/REQUIREMENTS.md` MORDB checkboxes pending.
- **Verification:** Focused Wave 0 handler tests still fail only on unimplemented Momoiro dispatch.

## Known Stubs

None. Stub scan hits were limited to standard EF `DbSet = null!` declarations and entity byte-array property defaults; the default save helper initializes runtime save byte arrays with Momoiro profile limits.

## Threat Flags

None. The new persistent schema surface is the planned migration boundary covered by the plan threat model.

## TDD Gate Compliance

Wave 0 RED tests already existed from Plan 41-01, so no new test-only RED commit was added. The pre-implementation RED run still failed on `Unsupported era: Momoiro`, and this plan added the GREEN persistence commits `329a5409` and `e893c74f`.

## Issues Encountered

- The literal plan grep path is not PowerShell-compatible on Windows and exits with `os error 123`.
- The PowerShell-expanded equivalent broad grep is too broad for EF migrations because `*.Designer.cs` contains full-model metadata for pre-existing non-Momoiro tables. Active migration-body extraction verified the actual Momoiro delta.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

The Momoiro readback schema now exists for later Phase 41 handler/controller implementation. Current Wave 0 handler tests still fail on unimplemented Momoiro dispatch, which remains later-plan scope.

## Self-Check: PASSED

- Created summary and implementation files exist on disk.
- Task commits found: `329a5409`, `e893c74f`.
- `Host/.gitignore` remains outside this plan's staged/committed files.

---
*Phase: 41-momoiro-identity-userdata-self-best-and-crown-readback*
*Completed: 2026-06-26*
