---
phase: 05-blue-battle-runtime-support
plan: "05-04"
subsystem: blue-battle-persistence-shape
tags: [blue, battle, persistence, ef-core, source-guard, gsd]

requires:
  - phase: 05-blue-battle-runtime-support
    provides: "05-01 row-resolution matrix and 05-02 battleuserdata/initialdata row gate"
provides:
  - "Blue-owned battle persistence entity and DbSet surfaces"
  - "BAID-scoped EF mappings for battle user, NPC, token, stage-result, and release state"
  - "Source-shape tests that reject Green AI Battle and normal Blue storage leakage"
affects: [05-05-blue-battle-migration, 05-07-battleuserdata-runtime, 05-10-battle-playresult-persistence]

tech-stack:
  added: []
  patterns: [blue-owned-ef-shape, nullable-raw-battle-state, source-shape-guard-tests]

key-files:
  created:
    - Domain/Entities/BlueBattleUserState.cs
    - Domain/Entities/BlueBattleNpcState.cs
    - Domain/Entities/BlueBattleTokenState.cs
    - Domain/Entities/BlueBattleStageResult.cs
    - Domain/Entities/BlueBattleReleaseState.cs
    - Tests/Blue/BlueBattlePersistenceShapeTests.cs
    - .planning/phases/05-blue-battle-runtime-support/05-04-SUMMARY.md
  modified:
    - Application/Abstractions/ITaikoDbContext.Blue.cs
    - Infrastructure/Persistence/TaikoDbContext.Blue.cs
    - Tests/Green/GreenAuthConfigTests.cs

key-decisions:
  - "Blue battle persistence starts as nullable/raw BlueBattle* state, not zero-filled defaults or XML-derived rows."
  - "Stable battle state is keyed by BAID, while stage-result and release observations are append-style captures."
  - "Green AI Battle and normal Blue score/history/Dani/favorite/recent/shop tables are not battle-owned storage."

patterns-established:
  - "Battle persistence shape is protected by source tests before migration generation."
  - "Blue battle EF mappings use the existing UserDatum.Baid cascade relationship pattern."

requirements-completed: [BTL-01, BTL-06]

duration: 18 min
completed: 2026-05-30
---

# Phase 05 Plan 05-04: Blue Battle Entity And DbContext Shape Summary

**Blue-owned nullable/raw battle persistence shape with focused source guards and EF mapping checks**

## Performance

- **Duration:** 18 min
- **Started:** 2026-05-30T13:09:00Z
- **Completed:** 2026-05-30T13:27:34Z
- **Tasks:** 1
- **Files modified:** 10

## Accomplishments

- Added five Blue-owned battle entities for user state, NPC state, token state, stage-result capture, and release capture.
- Extended `ITaikoDbContext` and `TaikoDbContext` with BlueBattle DbSets and BAID-scoped cascade mappings to `UserDatum.Baid`.
- Added `BlueBattlePersistenceShapeTests` to verify file/entity shape, DbSet/mapping presence, nullable unresolved fields, and no Green AI Battle or normal Blue storage leakage.
- Used read-only subagents for EF/test convention review and evidence-linked nullable field design before production edits.

## Task Commits

1. **Task 1 RED: Add failing Blue battle persistence shape tests** - `6b30919a` (test)
2. **Task 1 GREEN: Add Blue battle persistence shape** - `b74044d0` (feat)

## Files Created/Modified

- `Domain/Entities/BlueBattleUserState.cs` - BAID root state for release flags, last/assigned stage fields, boss life, NPC, and cap values.
- `Domain/Entities/BlueBattleNpcState.cs` - BAID/NPC keyed nullable raw NPC battle state.
- `Domain/Entities/BlueBattleTokenState.cs` - BAID/token keyed raw token value state without reward semantics.
- `Domain/Entities/BlueBattleStageResult.cs` - Append-style raw battle stage/result observation capture.
- `Domain/Entities/BlueBattleReleaseState.cs` - Append-style raw battle release observation capture.
- `Application/Abstractions/ITaikoDbContext.Blue.cs` - BlueBattle DbSet surfaces.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs` - BlueBattle EF table, key, datetime, index, and user relationship mappings.
- `Tests/Blue/BlueBattlePersistenceShapeTests.cs` - Focused source-shape and leakage guards.
- `Tests/Green/GreenAuthConfigTests.cs` - Throwing test double updated for the widened context interface.

## Decisions Made

- Keep unresolved battle values nullable or represented by absent child rows, even where the client can tolerate zero.
- Do not derive initial battle rows, token effects, reward meanings, stage assignments, boss-life behavior, or unlock mirrors from XML or Green AI Battle code.
- Use append-style capture tables for stage results and release observations so later plans can add behavior only after row-specific approval.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Updated an existing ITaikoDbContext test double**
- **Found during:** Task 1 GREEN verification
- **Issue:** Adding BlueBattle DbSets to `ITaikoDbContext` broke `GreenAuthConfigTests.ThrowingTaikoDbContext`, which intentionally implements every context member.
- **Fix:** Added throwing BlueBattle DbSet stubs to that test double only.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePersistenceShapeTests`
- **Committed in:** `b74044d0`

---

**Total deviations:** 1 auto-fixed (missing critical).
**Impact on plan:** No scope expansion; the change was required to compile the widened context interface.

## Issues Encountered

- The first GREEN verification compile failed on the missing test-double interface members; fixed in the GREEN commit.

## Verification Results

- RED: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePersistenceShapeTests` failed with missing `BlueBattleUserState.cs` and missing DbSet/mapping strings, as expected.
- GREEN: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePersistenceShapeTests` passed with 3 tests after adding the entities, DbSets, mappings, and test-double stubs.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for remaining Wave 2 work:

- `05-06` can add raw battle catalog/data boundaries without deriving runtime defaults from XML.
- `05-09` can map battle playresult sections into raw DTO surfaces that later persist into BlueBattle tables.
- `05-05` migration work is unblocked by the 05-04 shape, but should wait for the Wave 2 sequencing gate.

## Self-Check: PASSED

- Found all five `Domain/Entities/BlueBattle*.cs` files.
- Found `Tests/Blue/BlueBattlePersistenceShapeTests.cs`.
- Found task commit `6b30919a`.
- Found task commit `b74044d0`.
- Confirmed the focused verification command passed.

---
*Phase: 05-blue-battle-runtime-support*
*Completed: 2026-05-30*
