---
phase: 16-yellow-tokkun-and-banacoin-compatibility
plan: "01"
subsystem: persistence
tags: [yellow, ac15, tokkun, ef-core, playresult]
requires:
  - phase: 14-yellow-identity-userdata-crowns-self-best-and-normal-play
    provides: Yellow-owned save and normal playresult scaffolding with Tokkun placeholder branch
  - phase: 15-yellow-dani-shop-medals-waiwai-and-admin
    provides: Yellow-owned Dani/shop state and boundary guards that Phase 16 must avoid contaminating
provides:
  - Yellow Tokkun mapper contract tests for play mode, stage-data, and tutorial-only classification
  - Yellow-owned append-only Tokkun stage-history entity, DbSet, EF mapping, and migration
  - Schema and boundary tests proving raw protocol-field storage and forbidden state surfaces
affects: [yellow, phase-16, tokkun, playresult, persistence]
tech-stack:
  added: []
  patterns: [Yellow-owned Tokkun EF slice, behavior-based Tokkun mapper contract, raw ordered song JSON]
key-files:
  created:
    - Domain/Entities/YellowTokkunStageResult.cs
    - Infrastructure/Persistence/Migrations/20260608070833_AddYellowTokkunState.cs
    - Tests/Yellow/YellowTokkunPersistenceTests.cs
    - Tests/Yellow/YellowTokkunPersistenceShapeTests.cs
  modified:
    - Application/Abstractions/ITaikoDbContext.Yellow.cs
    - Infrastructure/Persistence/TaikoDbContext.Yellow.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Tests/Yellow/YellowPlayResultHandlerTests.cs
    - Tests/Yellow/YellowPersistenceBoundaryTests.cs
key-decisions:
  - "Existing Yellow mapper behavior already satisfied the Phase 16 classifier contract, so Plan 16-01 added focused contract coverage without production mapper changes."
  - "Yellow Tokkun history is physically Yellow-owned in `YellowTokkunStageResults`, not shared with Blue or any AC15 discriminator table."
  - "`BanacoinDatetime` is allowed as a raw Tokkun protocol timestamp while Banacoin wallet/payment/balance/coupon/receipt/transaction authority remains forbidden."
patterns-established:
  - "Yellow Tokkun schema mirrors Blue Tokkun raw history shape while preserving Yellow-owned table names and DbSets."
  - "Source-shape guards must distinguish protocol field names from stateful Banacoin authority."
requirements-completed: [YTOK-01, YTOK-02]
duration: 20 min
completed: 2026-06-08
---

# Phase 16 Plan 01: Yellow Tokkun Classifier And Schema Summary

**Yellow Tokkun classifier contract tests and Yellow-owned raw stage-history schema**

## Performance

- **Duration:** 20 min
- **Started:** 2026-06-08T14:53:00+08:00
- **Completed:** 2026-06-08T15:13:19+08:00
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments

- Added Yellow mapper tests proving `PlayMode.Tokkun = 3` classifies without stage data, stage data classifies with raw-field preservation, and tutorial-only payloads do not classify as Tokkun.
- Added `YellowTokkunStageResult`, `YellowTokkunStageResults`, EF mapping, and generated `AddYellowTokkunState` migration for append-only raw history rows.
- Added SQLite reload and source-shape tests proving raw song order/duplicates, nullable tutorial state, no server upload timestamps, and no normal/Dani/shop/battle/cross-era/payment storage.

## Task Commits

Each task was committed atomically:

1. **Task 1: Lock Yellow Tokkun mapper classification and raw field preservation** - `61470232` (test)
2. **Task 2: Add Yellow Tokkun raw history schema and schema proof** - `269c2718` (feat)

**Plan metadata:** committed separately with this summary.

## Files Created/Modified

- `Domain/Entities/YellowTokkunStageResult.cs` - Yellow-owned append-only raw Tokkun stage-history row.
- `Application/Abstractions/ITaikoDbContext.Yellow.cs` - Exposes the Yellow Tokkun history DbSet.
- `Infrastructure/Persistence/TaikoDbContext.Yellow.cs` - Maps `YellowTokkunStageResults` with BAID/play-time index and cascade from shared identity.
- `Infrastructure/Persistence/Migrations/20260608070833_AddYellowTokkunState.cs` - Creates only the Yellow Tokkun history table.
- `Tests/Yellow/YellowPlayResultHandlerTests.cs` - Locks Yellow Tokkun classifier and raw mapper behavior.
- `Tests/Yellow/YellowTokkunPersistenceTests.cs` - Proves SQLite reload, nullable raw tutorial values, ordered duplicate song JSON, and no unrelated rows.
- `Tests/Yellow/YellowTokkunPersistenceShapeTests.cs` - Proves Yellow-owned protocol-field-only schema shape.
- `Tests/Yellow/YellowPersistenceBoundaryTests.cs` - Allows only the new Yellow Tokkun history table while continuing to forbid battle and Banacoin authority.
- `Tests/Green/GreenAuthConfigTests.cs` - Updates a throw-only `ITaikoDbContext` test double for the new interface member.

## Decisions Made

- Existing Yellow mapper logic already had the correct classifier behavior, so no production mapper edit was needed for Task 1.
- Yellow Tokkun history stores only protocol-backed raw facts and keeps `UserSaveDataYellow.TokkunTutorialFlg` nullable and undefaulted.
- Boundary tests allow `BanacoinDatetime` only as a raw Tokkun timestamp, not as evidence of Banacoin authority.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated throw-only context test double for expanded Yellow DbSet contract**
- **Found during:** Task 2 focused verification
- **Issue:** `GreenAuthConfigTests.ThrowingTaikoDbContext` implemented `ITaikoDbContext`; adding `YellowTokkunStageResults` to the interface required the test double to expose the same throwing member.
- **Fix:** Added `DbSet<YellowTokkunStageResult> YellowTokkunStageResults => throw new NotSupportedException();`.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPersistenceBoundary"` passed.
- **Committed in:** `269c2718`

**2. [Rule 3 - Blocking] Narrowed overbroad Banacoin source guards to allow raw protocol timestamp**
- **Found during:** Task 2 focused verification
- **Issue:** New tests initially forbade the substring `Banacoin`, which contradicted the plan-required raw `BanacoinDatetime` Tokkun field.
- **Fix:** Changed guards to forbid stateful Banacoin/payment/balance/coupon/receipt/transaction authority while allowing `BanacoinDatetime`.
- **Files modified:** `Tests/Yellow/YellowTokkunPersistenceTests.cs`, `Tests/Yellow/YellowPersistenceBoundaryTests.cs`
- **Verification:** Focused schema and combined Plan 16-01 filters passed.
- **Committed in:** `269c2718`

---

**Total deviations:** 2 auto-fixed (2 blocking). **Impact on plan:** Both fixes were required to make the planned schema contract compile and match Phase 16 raw-field boundaries. No scope expansion.

## Issues Encountered

- `node .codex\get-shit-done\bin\gsd-tools.cjs query state.advance-plan` could not parse the current `STATE.md` position shape, so per-plan state was updated manually in this summary metadata commit.
- `roadmap.update-plan-progress 16` produced a malformed phase-summary table row before any summary existed; this commit restores the row schema and records 1/4 Phase 16 plans complete.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResult"` - passed, 28 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPersistenceBoundary"` - RED failed first on missing `YellowTokkunStageResult`/`YellowTokkunStageResults`, then passed, 13 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPersistenceBoundary"` - passed, 41 tests.
- Generated migration inspection confirmed `AddYellowTokkunState` creates only `YellowTokkunStageResults` with protocol-backed raw fields and no server timestamp or Banacoin authority columns.
- `git diff --name-only` scope check confirmed generated Yellow wire files, AdminApi, WebUI, Blue Tokkun source, Blue battle source, and Banacoin state files were not changed by Plan 16-01.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `16-02-PLAN.md`. Plan 16-01 provides the Yellow-owned schema and mapper contract that the next plan needs for allowed Tokkun tutorial/history writes. Phase-level verification, code review, and Phase 17 were not started.

---
*Phase: 16-yellow-tokkun-and-banacoin-compatibility*
*Completed: 2026-06-08*
