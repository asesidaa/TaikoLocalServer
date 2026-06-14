---
phase: 21-older-ac15-challengecompe-capability-and-red-binding
plan: 02
subsystem: persistence
tags: [red, ac15, challengecompe, sqlite, playresult]

requires:
  - phase: 21-01
    provides: ChallengeCompe evidence gate, Red sidecar catalog, and shared task/rule records
  - phase: 20
    provides: Red runtime state, Red playresult handler, and preserved challenge fact mapping
provides:
  - Red-owned ChallengeCompe raw fact and derived progress tables
  - Shared transport-agnostic ChallengeCompe progress evaluator
  - Red playresult integration for enrolled non-Tokkun matched challenge facts
  - Focused SQLite no-write and progress regression coverage
affects: [phase-21, red, challengecompe, older-ac15-runtime]

tech-stack:
  added: []
  patterns:
    - Red-owned ChallengeCompe persistence with shared evaluator and direct ITaikoDbContext DbSets
    - task_id/slot sidecar fields exposed as compe_id/track_no aliases for evaluator matching

key-files:
  created:
    - Domain/Entities/RedChallengeCompeRawFact.cs
    - Domain/Entities/RedChallengeCompeProgress.cs
    - Infrastructure/Persistence/Migrations/20260614140300_RedChallengeCompeState.cs
    - Application/Ac15/ChallengeCompe/Ac15ChallengeCompeProgressEvaluator.cs
    - Tests/Red/RedChallengeCompeTests.cs
  modified:
    - Application/Abstractions/ITaikoDbContext.Red.cs
    - Infrastructure/Persistence/TaikoDbContext.Red.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Application/Ac15/ChallengeCompe/Ac15ChallengeCompeCatalog.cs
    - Application/Handlers/UpdatePlayResultCommand.Red.cs
    - Tests/Green/GreenAuthConfigTests.cs

key-decisions:
  - "Plan 21-02 proceeded because 21-CHALLENGECOMPE-EVIDENCE.md permits evidence-gated Red-owned stateful DonChare behavior."
  - "The existing sidecar task_id and slot fields are treated as evaluator aliases for compe_id and track_no, respectively."
  - "User-created and BNG challenge buckets remain mapped but unused; only ary_challenge_id personal task facts can write Red ChallengeCompe state."

patterns-established:
  - "ChallengeCompe evaluation stays transport-agnostic in Application/Ac15/ChallengeCompe while Red handler code owns concrete Red persistence."
  - "Song-set task progress stores a numeric ProgressValue separate from completion state."

requirements-completed: [RCOMP-02, RCHAL-02, D-01, D-02, D-03, D-04, D-05, D-06, D-12, D-13, D-14, D-15, D-16, D-17, D-23, D-26]

duration: 14 min
completed: 2026-06-14
---

# Phase 21 Plan 02: Red ChallengeCompe Persistence and Progress Evaluator Summary

**Red-owned ChallengeCompe state with opt-in, active-task matching, Tokkun exclusion, and shared progress evaluation.**

## Performance

- **Duration:** 14 min
- **Started:** 2026-06-14T13:57:32Z
- **Completed:** 2026-06-14T14:10:53Z
- **Tasks:** 3
- **Files modified:** 12

## Accomplishments

- Confirmed the Plan 01 evidence gate allows Plan 02 stateful work only within Red-owned DonChare `ary_challenge_*` scope.
- Added Red-owned raw fact and progress entities, DbSets, EF mappings, and migration output.
- Added a shared ChallengeCompe evaluator and integrated it into Red playresult handling after Tokkun exclusion.
- Added SQLite regression tests for enrolled progress, disabled/inactive/not-enrolled no-ops, Tokkun no-op, unmatched facts, user/BNG bucket no-op, song-set accumulation, and cross-era no-writes.

## Task Commits

1. **Task 1: Honor the stateful evidence gate** - gate-only, no source commit; verified against `21-CHALLENGECOMPE-EVIDENCE.md` and temp-output Host build.
2. **Task 2: Add Red-owned ChallengeCompe tables** - `3a827c27` (feat)
3. **Task 3: Evaluate and persist eligible progress from Red playresults** - `2c441954` (feat)

**Plan metadata:** committed with final STATE/ROADMAP updates.

## Files Created/Modified

- `Domain/Entities/RedChallengeCompeRawFact.cs` - Red-owned append-style matched upload facts.
- `Domain/Entities/RedChallengeCompeProgress.cs` - Red-owned per-task derived progress and completion state.
- `Application/Abstractions/ITaikoDbContext.Red.cs` - Red ChallengeCompe DbSet port members.
- `Infrastructure/Persistence/TaikoDbContext.Red.cs` - Red ChallengeCompe table mapping and indexes.
- `Infrastructure/Persistence/Migrations/20260614140300_RedChallengeCompeState.cs` - EF migration for Red ChallengeCompe tables.
- `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` - EF model snapshot updated for Red ChallengeCompe state.
- `Application/Ac15/ChallengeCompe/Ac15ChallengeCompeCatalog.cs` - Explicit `CompeId` and `TrackNo` aliases for task matching.
- `Application/Ac15/ChallengeCompe/Ac15ChallengeCompeProgressEvaluator.cs` - Shared transport-agnostic evaluator.
- `Application/Handlers/UpdatePlayResultCommand.Red.cs` - Enrolled non-Tokkun Red playresults persist matched ChallengeCompe progress.
- `Tests/Red/RedChallengeCompeTests.cs` - Focused SQLite behavior/no-write coverage.
- `Tests/Green/GreenAuthConfigTests.cs` - Test double updated for new Red DbSets.

## Decisions Made

- The evidence gate permits this plan because the implementation stays within Red-owned state, local sidecar tasks, DonChare `ary_challenge_id` facts, opt-in users, and Tokkun exclusion.
- `task_id` and `slot` remain the sidecar schema, with explicit aliases for `compe_id` and `track_no` to make the evaluator contract concrete without changing Plan 01 data shape.
- `ary_user_compe_id` and `ary_bng_compe_id` remain preserved input facts only; they do not create raw fact or progress rows.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added numeric progress value for count-style tasks**
- **Found during:** Task 3 (Evaluate and persist eligible progress from Red playresults)
- **Issue:** The initial Red progress table modeled completion state but not numeric progress, which would make `song_set_count` rules impossible to represent before completion.
- **Fix:** Added `ProgressValue` to raw fact and progress rows, regenerated the Red ChallengeCompe migration, and amended the Task 2 commit so the state schema stayed atomic.
- **Files modified:** `Domain/Entities/RedChallengeCompeRawFact.cs`, `Domain/Entities/RedChallengeCompeProgress.cs`, `Infrastructure/Persistence/Migrations/20260614140300_RedChallengeCompeState.cs`, `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`
- **Verification:** Focused Red ChallengeCompe tests passed, including song-set accumulation.
- **Committed in:** `3a827c27`

**2. [Rule 3 - Blocking] Updated stale ITaikoDbContext test double**
- **Found during:** Task 3 (focused test run)
- **Issue:** `GreenAuthConfigTests.ThrowingTaikoDbContext` implemented `ITaikoDbContext` and failed compilation after the new Red ChallengeCompe DbSets were added.
- **Fix:** Added throwing implementations for `RedChallengeCompeRawFacts` and `RedChallengeCompeProgress`.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~RedPlayResultHandlerTests"` passed with 13 tests.
- **Committed in:** `2c441954`

**Total deviations:** 2 auto-fixed (1 Rule 2 missing critical, 1 Rule 3 blocking)
**Impact on plan:** Both fixes were required for correctness and compilation; no scope expansion beyond Plan 02.

## Issues Encountered

- Task 1 had no file changes because the evidence artifact already contained the stateful execution verdict. The gate result is documented here and enforced by the source changes in Tasks 2-3.
- The first focused test run failed at compile time on the stale `ITaikoDbContext` test double; fixed before proceeding.

## Verification

| Command | Result |
|---------|--------|
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Task 1 gate build passed: 0 warnings, 0 errors |
| `dotnet ef migrations add RedChallengeCompeState --project Infrastructure --startup-project Host` | Passed after build; generated `20260614140300_RedChallengeCompeState` |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Task 2 build passed: 0 warnings, 0 errors |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~RedPlayResultHandlerTests"` | Passed: 13 passed, 0 failed |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Final build passed: 0 warnings, 0 errors |
| `rg -n "TODO|FIXME|placeholder|coming soon|not available" -- [touched files]` | Passed: no matches |

## Known Stubs

None.

## Threat Flags

None. The new Red SQLite tables and playresult evaluator are the planned trust-boundary changes covered by T-21-02-01 through T-21-02-03.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `21-03` to apply configured ChallengeCompe reward grants and userdata locks against the Red-owned progress state created here. `challengecompe.php` readback is still pending Plan 04.

## Self-Check: PASSED

- Created summary exists.
- Red raw fact entity exists.
- Red progress entity exists.
- Shared progress evaluator exists.
- Focused Red ChallengeCompe tests exist.
- Red ChallengeCompe migration exists.
- Task commits found: `3a827c27`, `2c441954`.

---
*Phase: 21-older-ac15-challengecompe-capability-and-red-binding*
*Completed: 2026-06-14*
