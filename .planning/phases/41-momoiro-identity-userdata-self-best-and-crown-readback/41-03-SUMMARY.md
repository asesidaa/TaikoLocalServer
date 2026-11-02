---
phase: 41-momoiro-identity-userdata-self-best-and-crown-readback
plan: "03"
subsystem: application
tags: [momoiro, ac15, handlers, userdata, selfbest, crown-readback]

requires:
  - phase: 41-momoiro-identity-userdata-self-best-and-crown-readback
    provides: Momoiro readback persistence tables and default save helper from plan 41-02
provides:
  - Momoiro BAID and MyDon Application dispatch/readback
  - Momoiro userdata Application dispatch over Momoiro save, favorite, recent, best, and catalog state
  - Momoiro self-best Application dispatch over SongBestDataMomoiro only
  - Catalog-order compact crown payload support for Momoiro userdata
affects: [phase-41, 41-04, momoiro-readback, momoiro-controller-mappers]

tech-stack:
  added: []
  patterns:
    - Era-owned Application handler partials for Momoiro readback
    - Catalog-order crown packing for sparse/high raw Momoiro song IDs
    - Persisted DisplayOrder favorite readback with SongNo tie-breaker

key-files:
  created:
    - Application/Handlers/BaidQuery.Momoiro.cs
    - Application/Handlers/AddMyDonEntryCommand.Momoiro.cs
    - Application/Handlers/UserDataQuery.Momoiro.cs
    - Application/Handlers/GetSelfBestQuery.Momoiro.cs
    - Application/Ac15/MomoiroAc15UserDataAdapter.cs
  modified:
    - Application/Handlers/BaidQuery.cs
    - Application/Handlers/AddMyDonEntryCommand.cs
    - Application/Handlers/UserDataQuery.cs
    - Application/Handlers/GetSelfBestQuery.cs
    - Application/Ac15/Ac15CrownService.cs
    - Application/Dtos/Ac15/Ac15UserDataResponse.cs
    - Application/Common/UserSaveDataMomoiroExtensions.cs

key-decisions:
  - "Momoiro userdata favorites are ordered by persisted DisplayOrder, then SongNo, before applying the Momoiro favorite cap."
  - "Momoiro crown bytes are packed by Momoiro catalog/file order so high raw song IDs remain represented at their catalog ordinal."
  - "The shared Ac15SelfBestService remains unchanged; the Momoiro handler trims empty shin rows after shared response construction to satisfy the Momoiro readback contract without changing other eras."
  - "Controller/Mapperly failures from MomoiroCrownReadbackTests remain assigned to plan 41-04 and were not fixed in this Application-layer plan."

patterns-established:
  - "Momoiro Application handlers query only Momoiro DbSets after shared identity lookup."
  - "Use Ac15CrownService.BuildCatalogOrderBody for compact crown payloads where raw song IDs are not dense crown indices."

requirements-completed: [MORDB-01, MORDB-02, MORDB-03, MORDB-04, MORDB-05]
requirements-note: "Plan 41-03 completes the Application-layer handler slice for these requirements; REQUIREMENTS.md should remain pending until controller/mapping and final Phase 41 verification plans complete."

duration: 13min
completed: 2026-06-26
status: complete
---

# Phase 41 Plan 03: Momoiro Application Readback Summary

**Momoiro Application handlers for BAID, MyDon, userdata, self-best, ordered favorites, catalog-order release flags, and compact crown bytes**

## Performance

- **Duration:** 13 min
- **Started:** 2026-06-26T08:44:00Z
- **Completed:** 2026-06-26T08:56:40Z
- **Tasks:** 3/3
- **Files modified:** 12 implementation files plus this summary

## Accomplishments

- Added Momoiro dispatcher arms and partial handlers for BAID, MyDon, userdata, and self-best.
- Added `MomoiroAc15UserDataAdapter` using Momoiro save state, Momoiro catalog snapshots, persisted favorite order, recent ordering, counters, release flags, profile settings, mode flags, and reward readback.
- Extended `Ac15CrownService` with catalog-order crown packing so high raw Momoiro song IDs are preserved at their file-order ordinal.
- Extended the common AC15 userdata response with an optional `HashCrownFlg` payload for plan 41-04 adapter mapping.
- Kept playresult mutation, Dan tables, Tokkun, battle, ChallengeCompe, Don Challenge, AdminApi/WebUI, unsupported routes, proto files, and `Host/.gitignore` out of this plan.

## Task Commits

1. **Task 1: Wire Momoiro BAID and MyDon handlers** - `b05b28fc` (`feat`)
2. **Task 2: Wire Momoiro userdata readback and crown builder** - `b5b46b72` (`feat`)
3. **Task 3: Wire Momoiro self-best handler** - `c5853cb3` (`feat`)

## Files Created/Modified

- `Application/Handlers/BaidQuery.Momoiro.cs` - Momoiro BAID login/readback over shared cards plus `UserSaveDataMomoiro`.
- `Application/Handlers/AddMyDonEntryCommand.Momoiro.cs` - Momoiro MyDon registration through shared `Ac15MyDonEntryService`.
- `Application/Handlers/UserDataQuery.Momoiro.cs` - Momoiro userdata readback over Momoiro save, favorite, recent, best, and catalog state.
- `Application/Handlers/GetSelfBestQuery.Momoiro.cs` - Momoiro self-best readback over `SongBestDataMomoiro` only.
- `Application/Ac15/MomoiroAc15UserDataAdapter.cs` - Momoiro save/catalog/list snapshot adapter for shared AC15 userdata response building.
- `Application/Ac15/Ac15CrownService.cs` - Added catalog-order compact crown builder.
- `Application/Dtos/Ac15/Ac15UserDataResponse.cs` - Added optional `HashCrownFlg` for userdata-owned Momoiro crown payload.
- `Application/Common/UserSaveDataMomoiroExtensions.cs` - Added Momoiro get-or-create save helper needed by userdata readback.
- `Application/Handlers/BaidQuery.cs`, `AddMyDonEntryCommand.cs`, `UserDataQuery.cs`, `GetSelfBestQuery.cs` - Added Momoiro dispatcher arms.

## Verification

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests" --no-restore -- RunConfiguration.DisableParallelization=true` before implementation | RED as expected: exit 1, compiled, 4/4 failed on `Unsupported era: Momoiro`. |
| Same command after Task 1 | Expected partial GREEN: exit 1, 2 passed and 2 failed. Remaining failures were `GetSelfBestQuery_Momoiro_ReadsMomoiroNormalUraAndShinRowsOnlyInRequestOrder` and `UserDataQuery_Momoiro_ReadsMomoiroListsHashReleaseAndIgnoresAdjacentRows`, both still on `Unsupported era: Momoiro`. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests\|FullyQualifiedName~MomoiroCrownReadbackTests\|FullyQualifiedName~Ac15CrownServiceTests" --no-restore -- RunConfiguration.DisableParallelization=true` after Task 2 | Exit 1, compiled, 5 passed and 3 failed. Remaining failures were self-best dispatch plus the two controller-level `MomoiroCrownReadbackTests` against the still-scaffolded Momoiro `UserDataController`. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests.UserDataQuery_Momoiro\|FullyQualifiedName~Ac15CrownServiceTests" --no-restore -- RunConfiguration.DisableParallelization=true` after Task 2 | PASS: exit 0, 3 passed, 0 failed, 0 skipped. This verified the Application-layer userdata path and shared crown-service coverage. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests\|FullyQualifiedName~MomoiroSelfBest" --no-restore -- RunConfiguration.DisableParallelization=true` after Task 3 | PASS: exit 0, 4 passed, 0 failed, 0 skipped. |
| Final `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: exit 0, 4 passed, 0 failed, 0 skipped. |
| Final Task 2 exact command: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests\|FullyQualifiedName~MomoiroCrownReadbackTests\|FullyQualifiedName~Ac15CrownServiceTests" --no-restore -- RunConfiguration.DisableParallelization=true` | Expected later-plan RED: exit 1, 6 passed and 2 failed. The only failures were `MomoiroCrownReadbackTests.UserDataController_EmptyMomoiroSaveReturnsFixedZeroHashCrownFlg` and `MomoiroCrownReadbackTests.UserDataController_HighSongIdCrownPackingUsesMomoiroCatalogOrdinal`, both because `UserDataController` is still a 41-04 scaffold and `ShouldSerializeHashCrownFlg()` is false. |
| Final `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests\|FullyQualifiedName~MomoiroSelfBest" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: exit 0, 4 passed, 0 failed, 0 skipped. |
| `dotnet build Application/Application.csproj --no-restore` | PASS: build succeeded, 0 warnings, 0 errors, elapsed 00:00:01.48. |
| `git status --porcelain -- proto\momoiro` | PASS: no output; proto inputs unchanged. |
| `git diff --cached --name-only -- Host/.gitignore` | PASS: no output; `Host/.gitignore` was not staged. |
| `git diff --cached --name-only` | PASS: no output; no staged files remained after task commits. |
| `git status --short` | Shows only pre-existing unstaged `M Host/.gitignore`. |

## Decisions Made

- Keep Momoiro Application readback era-owned: handlers query `UserSaveDataMomoiro`, `SongBestDataMomoiro`, `MomoiroFavoriteSongs`, and `MomoiroRecentSongs` only.
- Preserve the persisted favorite order contract from plan 41-02 by ordering favorites on `DisplayOrder` and then `SongNo`, then applying `MaxFavoriteSongs`.
- Add a catalog-order crown builder rather than using raw-index `BuildInflatedBody`, because Momoiro high raw song IDs can exceed `CrownSongCount` while still belonging at a valid catalog ordinal.
- Leave Momoiro controller and Mapperly mapping failures for plan 41-04 to avoid widening this Application-layer plan.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added missing Momoiro get-or-create save helper**
- **Found during:** Task 2
- **Issue:** `UserDataQuery.Momoiro.cs` needed the adjacent-era get-or-create pattern, but `UserSaveDataMomoiroExtensions` only had `CreateDefaultMomoiroSaveData`.
- **Fix:** Added `GetOrCreateMomoiroSaveDataAsync` using `context.UserSaveDataMomoiro` only.
- **Files modified:** `Application/Common/UserSaveDataMomoiroExtensions.cs`
- **Verification:** Application-layer Task 2 check passed 3/3; final handler test command passed 4/4.
- **Committed in:** `b5b46b72`

**2. [Rule 1 - Bug] Kept Momoiro self-best shin rows aligned to the Phase 41 readback contract**
- **Found during:** Task 3 implementation review
- **Issue:** Shared `Ac15SelfBestService.BuildResponse` emits an empty shin row for every requested song, while the Momoiro RED contract expects only populated shin rows without changing other eras.
- **Fix:** The Momoiro handler still builds the response through `Ac15SelfBestService`, then trims empty shin rows in the Momoiro partial only.
- **Files modified:** `Application/Handlers/GetSelfBestQuery.Momoiro.cs`
- **Verification:** Task 3 command passed 4/4.
- **Committed in:** `c5853cb3`

### Workflow-Scope Adjustments

**1. Task 2 exact verify command includes plan 41-04 controller tests**
- **Found during:** Task 2 verification
- **Issue:** `MomoiroCrownReadbackTests` exercise `Adapters.GameProtocol.Momoiro.Controllers.UserDataController`, which remains a scaffold and is outside the Application-layer file list and user scope for 41-03.
- **Adjustment:** Ran and recorded the exact command result, then added a narrower Application-layer verification for the handler/crown-service slice. No controller or mapper files were changed.
- **Impact:** The Application-layer work is complete; the two remaining failures are expected input for plan 41-04.

**2. Corrected stale `STATE.md` body text after SDK state updates**
- **Found during:** Plan closeout
- **Issue:** `state.update-progress`, `state.record-metric`, `state.add-decision`, and `state.record-session` updated parts of `STATE.md`, but `state.advance-plan` could not parse the existing Current Position text and the body/frontmatter were left inconsistent.
- **Adjustment:** Re-ran the named-flag SDK commands where supported, then made a minimal correction to `STATE.md` for current position, progress text, last activity, duplicated decision prefixes, and next-step text.
- **Impact:** State now records 41-03 complete and 41-04 next without marking MORDB requirements complete.

**Total deviations:** 2 auto-fixed issues plus 2 workflow-scope adjustments.
**Impact on plan:** Application-layer behavior is complete without adding controller, AdminApi/WebUI, mutation, route, or proto scope.

## Known Stubs

None in files created or modified by this plan. The narrower placeholder scan over plan files returned no `TODO`, `FIXME`, `placeholder`, `coming soon`, or `not available` hits. Empty byte arrays and nullability in these files are intentional DTO/default-state patterns, not UI/rendering stubs.

## Threat Flags

None. The changes stay inside the planned Application handler/readback boundary and do not introduce new network endpoints, auth paths, filesystem access, schema changes, or trust boundaries.

## TDD Gate Compliance

Wave 0 RED tests already existed from Plan 41-01. This plan used those tests as the RED gate:

- Pre-implementation focused handler command failed 4/4 on `Unsupported era: Momoiro`.
- Task 1 moved BAID/MyDon to green while userdata/self-best remained red.
- Task 3 closed the remaining Application handler RED tests to 4/4 passing.

No new test-only RED commit was added because the repo policy forbids adding tests solely to satisfy a TDD checkbox when suitable evidence-backed tests already exist.

## Issues Encountered

- The Task 2 exact command is broader than the Application-layer task scope and still fails on controller-level crown serialization. This was recorded and left for plan 41-04.
- The installed GSD `state.advance-plan` command returned `Cannot parse Current Plan or Total Plans in Phase from STATE.md`; supported named-flag state commands were used where possible, then stale state text was corrected narrowly.
- `Host/.gitignore` was already dirty before this plan and remained unstaged and untouched.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Plan 41-04 can now wire Momoiro BAID/MyDon/UserData/SelfBest controllers and Mapperly mappers against the Application-layer responses created here. In particular, `Ac15UserDataResponse.HashCrownFlg` is ready to map to Momoiro wire `hash_crown_flg`.

## Self-Check: PASSED

- Created files found: `BaidQuery.Momoiro.cs`, `AddMyDonEntryCommand.Momoiro.cs`, `UserDataQuery.Momoiro.cs`, `GetSelfBestQuery.Momoiro.cs`, `MomoiroAc15UserDataAdapter.cs`, and this summary.
- Task commits found: `b05b28fc`, `b5b46b72`, and `c5853cb3`.
- No tracked file deletions were introduced by task commits.
- `Host/.gitignore` remains only as the pre-existing unstaged working-tree change and was not staged.

---
*Phase: 41-momoiro-identity-userdata-self-best-and-crown-readback*
*Completed: 2026-06-26*
