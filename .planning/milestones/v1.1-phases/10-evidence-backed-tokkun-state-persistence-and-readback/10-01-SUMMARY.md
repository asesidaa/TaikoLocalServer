---
phase: 10-evidence-backed-tokkun-state-persistence-and-readback
plan: "01"
subsystem: database
tags: [blue, tokkun, ef-core, sqlite, protobuf]

requires:
  - phase: 09-tokkun-mapper-and-safe-playresult-acceptance
    provides: Blue Tokkun mapper DTO surface and safe no-write playresult branch
provides:
  - PlayMode.Tokkun named protocol value and mode-based mapper classifier
  - Nullable Blue Tokkun tutorial storage on UserSaveData_Blue
  - BlueTokkunStageResults append-only history schema for raw Tokkun stage facts
  - EF migration and SQLite/source-shape tests for Tokkun persistence boundaries
affects: [phase-10, phase-11, blue-playresult, blue-userdata]

tech-stack:
  added: []
  patterns: [Blue-owned EF persistence, nullable optional protocol state, JSON raw ordered list storage]

key-files:
  created:
    - Domain/Entities/BlueTokkunStageResult.cs
    - Infrastructure/Persistence/Migrations/20260606153034_AddBlueTokkunState.cs
    - Infrastructure/Persistence/Migrations/20260606153034_AddBlueTokkunState.Designer.cs
    - Tests/Blue/BlueTokkunPersistenceTests.cs
    - Tests/Blue/BlueTokkunPersistenceShapeTests.cs
  modified:
    - Domain/Enums/PlayMode.cs
    - Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs
    - Domain/Entities/UserSaveDataBlue.cs
    - Application/Abstractions/ITaikoDbContext.Blue.cs
    - Infrastructure/Persistence/TaikoDbContext.Blue.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Tests/Blue/BluePlayResultMapperTests.cs
    - Tests/Green/GreenAuthConfigTests.cs

key-decisions:
  - "Tokkun classification now follows the proven Blue play_mode value via PlayMode.Tokkun = 3."
  - "Tokkun summary history stores raw protocol facts only, including TookunSongnoesJson for ordered duplicate song numbers."

patterns-established:
  - "Blue Tokkun history rows use a generated Id and Baid foreign key, with no server upload timestamp."
  - "Tokkun tutorial state remains nullable on UserSaveDataBlue and is not initialized by default save creation."

requirements-completed:
  - TKST-01
  - TKST-02
  - TKST-03
  - TKST-04

duration: 18 min
completed: 2026-06-06
---

# Phase 10 Plan 01: Classifier And Storage Schema Summary

**Blue Tokkun mode classification and EF schema for nullable tutorial state plus append-only raw stage history**

## Performance

- **Duration:** 18 min
- **Started:** 2026-06-06T15:16:00Z
- **Completed:** 2026-06-06T15:33:16Z
- **Tasks:** 2
- **Files modified:** 13

## Accomplishments

- Added `PlayMode.Tokkun = 3` and changed Blue playresult classification to use `request.PlayMode == (uint)PlayMode.Tokkun`.
- Added `UserSaveDataBlue.TokkunTutorialFlg` as nullable raw `uint?` with no default initialization.
- Added `BlueTokkunStageResult`, `BlueTokkunStageResults` EF mapping, and generated `AddBlueTokkunState` migration.
- Added mapper, SQLite reload, migration shape, and source-shape tests proving raw ordered song-list fidelity and storage isolation.

## Task Commits

1. **Task 1: Name the proven Tokkun mode and update mapper classification** - `783a0792` (feat)
2. **Task 2: Add Blue Tokkun storage schema and schema proof** - `e4f89c06` (feat)

## Files Created/Modified

- `Domain/Enums/PlayMode.cs` - Names the proven Tokkun play mode value.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - Classifies Tokkun by play mode while preserving raw stage data separately.
- `Domain/Entities/UserSaveDataBlue.cs` - Adds nullable raw tutorial storage.
- `Domain/Entities/BlueTokkunStageResult.cs` - Adds append-only raw Tokkun summary/history row.
- `Application/Abstractions/ITaikoDbContext.Blue.cs` - Exposes the Blue Tokkun history DbSet.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs` - Maps the Blue Tokkun history table and Baid relationship.
- `Infrastructure/Persistence/Migrations/20260606153034_AddBlueTokkunState.cs` - Adds nullable tutorial column and Blue Tokkun history table.
- `Tests/Blue/BluePlayResultMapperTests.cs` - Covers mode-only and stage-only classifier behavior.
- `Tests/Blue/BlueTokkunPersistenceTests.cs` - Covers migration scope, SQLite reload, nullable tutorial storage, and no cross-state writes.
- `Tests/Blue/BlueTokkunPersistenceShapeTests.cs` - Covers source-shape boundaries for Blue-owned Tokkun storage.
- `Tests/Green/GreenAuthConfigTests.cs` - Keeps the throwing test context in sync with the expanded shared DbContext interface.

## Decisions Made

`TookunSongnoes` is stored as a JSON string column. This preserves raw order and duplicates without inventing a relational readback surface before protocol evidence requires one.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated ITaikoDbContext test double for new DbSet**
- **Found during:** Task 2 (schema test green run)
- **Issue:** `GreenAuthConfigTests.ThrowingTaikoDbContext` no longer implemented `ITaikoDbContext` after adding `BlueTokkunStageResults`.
- **Fix:** Added the throwing `DbSet<BlueTokkunStageResult>` property to the test double.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceShapeTests"`
- **Committed in:** `e4f89c06`

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** The fix was required to keep the shared context contract compiling. No runtime scope was widened.

## Issues Encountered

- The first migration-shape test expected a `name:` argument for EF's anonymous object column syntax and overmatched `Down()` drops. The test was corrected to match generated EF C# shape while preserving the migration-scope assertion.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Plan 10-02 can persist classified Tokkun uploads into the newly available nullable tutorial field and append-only history table.

---
*Phase: 10-evidence-backed-tokkun-state-persistence-and-readback*
*Completed: 2026-06-06*
