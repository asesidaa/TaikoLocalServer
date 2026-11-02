---
phase: 05-blue-battle-runtime-support
plan: "05-09"
subsystem: blue-battle-playresult-mapping
tags: [blue, battle, playresult, mapper, dto, gsd]

requires:
  - phase: 05-blue-battle-runtime-support
    provides: "05-01 row-resolution matrix and D-10 battle branch trigger"
provides:
  - "Blue battle playresult classification from battle section presence"
  - "Raw DTO mapping for battle stage, NPC, release, token, play mode, and stage mode values"
  - "Mapper tests rejecting Green AI Battle and stage-mode-derived Blue battle truth"
affects: [05-10-battle-playresult-persistence, 05-11-battle-regression-tests]

tech-stack:
  added: []
  patterns: [blue-owned-dto-partial, raw-playresult-mapping, source-guarded-mapper-tests]

key-files:
  created:
    - Application/Dtos/CommonPlayResultData.BlueBattle.cs
    - Tests/Blue/BlueBattlePlayResultMapperTests.cs
    - .planning/phases/05-blue-battle-runtime-support/05-09-SUMMARY.md
  modified:
    - Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs

key-decisions:
  - "Blue battle playresult classification is driven only by `AryReleaseBattledata` or any stage `AryBattlestagedata` presence."
  - "Battle play mode and stage mode are preserved as observed raw values, not used as classifier truth."
  - "Release, token, assignment, boss-life, NPC progress, and reward effects remain unmapped as behavior and are only captured as raw DTO values."

patterns-established:
  - "Blue battle playresult data lives in a Blue-owned CommonPlayResultData partial."
  - "Mapper tests guard against Green AI Battle semantics and stage-mode-specific battle inference."

requirements-completed: [BTL-04, BTL-06]

duration: 10 min
completed: 2026-05-30
---

# Phase 05 Plan 05-09: Battle Playresult DTO Mapping Summary

**Blue battle playresult branch classification and raw DTO mapping without progression, reward, token, or normal-save effects**

## Performance

- **Duration:** 10 min
- **Started:** 2026-05-30T13:39:00Z
- **Completed:** 2026-05-30T13:49:11Z
- **Tasks:** 1
- **Files modified:** 4

## Accomplishments

- Added `CommonPlayResultData.BlueBattle.cs` with raw battle stage, NPC, release, and token DTO fields.
- Updated the Blue playresult mapper to classify battle payloads from top-level `AryReleaseBattledata` or any stage `AryBattlestagedata`.
- Preserved client-reported `PlayMode`, `StageMode`, battle stage fields, NPC raw strings/IDs, release arrays, token rows, and assign-next-stage value as DTO data only.
- Added focused mapper tests for branch triggers, raw field preservation, and no Green AI Battle/stage-mode truth leakage.

## Task Commits

1. **Task 1 RED: Add failing Blue battle playresult mapper tests** - `40dcb183` (test)
2. **Task 1 GREEN: Map Blue battle playresult DTOs** - `f583ef8c` (feat)

## Files Created/Modified

- `Application/Dtos/CommonPlayResultData.BlueBattle.cs` - Blue battle playresult DTO partial.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - Battle classification and raw mapping helpers.
- `Tests/Blue/BlueBattlePlayResultMapperTests.cs` - Trigger, raw preservation, and source-guard tests.

## Decisions Made

- The mapper does not infer battle from `StageMode`; it records mode values for later evidence review.
- `AcquiredExp` and `TotalExp` remain strings to preserve client-reported raw values.
- `AssignNextStageId`, token rows, release arrays, boss life, and NPC values are mapped only as raw observations for later handler plans.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Avoided DTO property/type name collision**
- **Found during:** Task 1 GREEN verification
- **Issue:** C# rejected a top-level property and nested type both named `BattleReleaseData`.
- **Fix:** Kept the public property as `BattleReleaseData` and renamed the backing DTO type to `BattleReleaseDataDto`.
- **Files modified:** `Application/Dtos/CommonPlayResultData.BlueBattle.cs`, `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePlayResultMapperTests`
- **Committed in:** `f583ef8c`

**2. [Rule 1 - Bug] Source guard initially matched its own forbidden literals**
- **Found during:** Task 1 GREEN verification
- **Issue:** The guard test scanned its own source and found contiguous forbidden token strings inside the forbidden-token list.
- **Fix:** Built those token strings from parts and renamed the test method so the guard checks source content without self-matching.
- **Files modified:** `Tests/Blue/BlueBattlePlayResultMapperTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePlayResultMapperTests`
- **Committed in:** `f583ef8c`

---

**Total deviations:** 2 auto-fixed (bugs).
**Impact on plan:** No scope expansion; both fixes were local to DTO/test correctness.

## Issues Encountered

- The first GREEN run failed on the DTO name collision.
- The second GREEN run failed because `Assert.NotNull` in this xUnit version returns void, so the test was adjusted to assert then dereference.
- The third GREEN run failed because the source guard matched its own forbidden literals; fixed by avoiding contiguous forbidden strings in the test source.

## Verification Results

- RED: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePlayResultMapperTests` failed with missing `IsBattlePlayResult`, `BattleReleaseData`, and `BattleStageData`, as expected.
- GREEN: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePlayResultMapperTests` passed with 4 tests.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Wave 2 is complete. Next is Wave 3:

- `05-03` is the reward/progression row checkpoint and must keep derived effects blocked unless exact proof or user approval is recorded.
- `05-05` can generate the BlueBattle migration and persistence tests after the 05-03 gate.

## Self-Check: PASSED

- Found `Application/Dtos/CommonPlayResultData.BlueBattle.cs`.
- Found `Tests/Blue/BlueBattlePlayResultMapperTests.cs`.
- Found task commit `40dcb183`.
- Found task commit `f583ef8c`.
- Confirmed the focused mapper test passed.

---
*Phase: 05-blue-battle-runtime-support*
*Completed: 2026-05-30*
