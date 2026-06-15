---
phase: 22-red-adminapi-webui-and-runtime-closeout
plan: 1
subsystem: api
tags: [red, adminapi, ac15, runtime-state, catalog, tests]

requires:
  - phase: 20
    provides: Red-owned runtime save, best, play, favorite, recent, and Dani tables
  - phase: 21
    provides: Red ChallengeCompe boundary constraints that remain excluded from user settings
provides:
  - Red normal AdminApi user settings/profile read-write support
  - Red normal play data, play history, favorites, leaderboard, Dani, game data, and customization catalog readback
  - Focused Red AdminApi controller tests for Red-owned persistence and catalog isolation
affects: [22-02, 22-03, 22-04, red-adminapi, red-webui, ac15-admin-surfaces]

tech-stack:
  added: []
  patterns:
    - Red AdminApi branches use existing `/api/{era}/...` controller contracts
    - Red normal AdminApi behavior binds to Red-owned EF tables and `IRedCatalog`

key-files:
  created:
    - Adapters.AdminApi/Controllers/UserSettingsController.Red.cs
    - Adapters.AdminApi/Controllers/PlayDataController.Red.cs
    - Adapters.AdminApi/Controllers/PlayHistoryController.Red.cs
    - Adapters.AdminApi/Controllers/FavoriteSongsController.Red.cs
    - Adapters.AdminApi/Controllers/SongLeaderboardController.Red.cs
    - Tests/Red/RedAdminApiTests.cs
  modified:
    - Adapters.AdminApi/Controllers/UserSettingsController.cs
    - Adapters.AdminApi/Controllers/PlayDataController.cs
    - Adapters.AdminApi/Controllers/PlayHistoryController.cs
    - Adapters.AdminApi/Controllers/FavoriteSongsController.cs
    - Adapters.AdminApi/Controllers/SongLeaderboardController.cs
    - Adapters.AdminApi/Controllers/DanBestDataController.cs
    - Adapters.AdminApi/Controllers/GameDataController.cs
    - Adapters.AdminApi/Controllers/CustomizationCatalogController.cs
    - Application/Ac15/Ac15UserSettingsAccess.cs

key-decisions:
  - "Red AdminApi parity is limited to the normal surfaces listed in Plan 22-01."
  - "Red user settings reuse the AC15 settings service without exposing UserSaveDataRed.IsChallengeCompe."
  - "Red music details map the shared Ac15MusicInfoEntry.GenreName string into WebUI SongGenre values."

patterns-established:
  - "Red partial controller branches mirror the existing Yellow AC15 pattern while binding only Red-owned tables."
  - "Red catalog AdminApi projection uses IRedCatalog rather than filesystem paths or another era catalog."

requirements-completed: [RVER-01, RVER-02]

duration: 13 min
completed: 2026-06-15
---

# Phase 22 Plan 1: Red Normal AdminApi Parity Summary

**Red normal AdminApi readback and profile editing over Red-owned runtime tables and Red catalog slices**

## Performance

- **Duration:** 13 min from first task commit to final verification
- **Started:** 2026-06-15T05:22:31Z (first task commit timestamp; separate start timestamp was not captured)
- **Completed:** 2026-06-15T05:35:19Z
- **Tasks:** 3
- **Files modified:** 15

## Accomplishments

- Added Red user-settings AdminApi read/write support through `Ac15UserSettingsService`, `UserSaveDataRed`, and `DanScoreDataRed`.
- Added Red normal play data, play history, favorite song operations, and leaderboard readback using Red best/play/favorite tables only.
- Added Red Dani, music details, Taikojuku Dan data, and customization catalog readback through Red Dan rows and `IRedCatalog`.
- Added focused Red AdminApi tests proving Red-owned read/write boundaries, cross-era isolation, favorite limit enforcement, and Red catalog projection.

## Task Commits

1. **Task 1: Bind Red User Settings** - `6a6cd1f6` (`feat`)
2. **Task 2: Bind Red Play Data, History, Favorites, And Leaderboard** - `8fb40141` (`feat`)
3. **Task 3: Bind Red Dani, Music, And Customization Catalog Readback** - `6b8bd4a1` (`feat`)

## Files Created/Modified

- `Adapters.AdminApi/Controllers/UserSettingsController.Red.cs` - Red user settings get/save branch bound to Red save and Dan rows.
- `Adapters.AdminApi/Controllers/PlayDataController.Red.cs` - Red normal best/play/favorite projection with Shin alternate score support.
- `Adapters.AdminApi/Controllers/PlayHistoryController.Red.cs` - Red play history projection with Red favorite state.
- `Adapters.AdminApi/Controllers/FavoriteSongsController.Red.cs` - Red favorite song get/update behavior with AC15 favorite limit enforcement.
- `Adapters.AdminApi/Controllers/SongLeaderboardController.Red.cs` - Red leaderboard rows from non-Shin Red best rows.
- `Adapters.AdminApi/Controllers/DanBestDataController.cs` - Red Dani best-data branch over Red Dan rows.
- `Adapters.AdminApi/Controllers/GameDataController.cs` - Red music details and Taikojuku data projection through `IRedCatalog`.
- `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs` - Red costumes, titles, and tones catalog branches.
- `Application/Ac15/Ac15UserSettingsAccess.cs` - Red `UserSaveDataRed` accessor for shared AC15 settings service.
- `Tests/Red/RedAdminApiTests.cs` - Focused observable Red AdminApi coverage.

## Decisions Made

- Red AdminApi support is enabled only on implemented normal readback/edit surfaces: user settings, play data/history, favorites, leaderboard, Dani, game data, and customization catalog.
- Red ChallengeCompe opt-in remains out of `UserSetting`; `UserSaveDataRed.IsChallengeCompe` is not read or mutated by profile editing.
- Red music genre mapping uses the shared Red `Ac15MusicInfoEntry.GenreName` string because the Red catalog model does not expose the Green/Blue/Yellow numeric category field.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed Red music genre projection source**
- **Found during:** Task 3 (focused test build)
- **Issue:** The initial Red music projection referenced `Ac15MusicInfoEntry.CategoryId`, but Red uses the shared `Ac15MusicInfoEntry` model with `GenreName` instead.
- **Fix:** Added conservative Red `GenreName` to `SongGenre` mapping and kept Blue/Yellow projections on their existing numeric category field.
- **Files modified:** `Adapters.AdminApi/Controllers/GameDataController.cs`, `Tests/Red/RedAdminApiTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedAdminApi"` passed.
- **Committed in:** `6b8bd4a1`

---

**Total deviations:** 1 auto-fixed (1 bug)
**Impact on plan:** Correctness fix only; no scope expansion or unsupported AdminApi surface was added.

## Issues Encountered

- The combined Phase 22 focused filter was deferred because `RedDonChallenge` tests do not exist yet; later plans own that dependent surface.

## Known Stubs

- `Adapters.AdminApi/Controllers/DanBestDataController.cs:36` has a pre-existing `FIXME` for Nijiiro gaiden handling. This plan reused the same controller file but did not change that behavior; it does not block Red normal Dani readback.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedAdminApi"` - passed, 9 tests.
- Acceptance surface check: Red AdminApi branches were found only on user settings, play data, play history, favorites, leaderboard, Dani, game data, and customization catalog.
- Unsupported surface check: no Red shop, payment, coupon, wallet, Banacoin, or battle AdminApi controller branch was added.
- Combined Phase 22 filter deferred until later dependent plans add `RedDonChallenge` tests.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Plan 22-02 to add Red generic WebUI era routing against the normal AdminApi surfaces. Existing dirty Red ChallengeCompe WIP was preserved and not staged in this plan's commits.

## Self-Check: PASSED

- Summary file exists at `.planning/phases/22-red-adminapi-webui-and-runtime-closeout/22-01-SUMMARY.md`.
- Created files exist: `UserSettingsController.Red.cs`, `PlayDataController.Red.cs`, `PlayHistoryController.Red.cs`, `FavoriteSongsController.Red.cs`, `SongLeaderboardController.Red.cs`, and `Tests/Red/RedAdminApiTests.cs`.
- Task commits found: `6a6cd1f6`, `8fb40141`, `6b8bd4a1`.

---
*Phase: 22-red-adminapi-webui-and-runtime-closeout*
*Completed: 2026-06-15*
