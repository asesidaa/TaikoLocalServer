---
phase: 01-blue-a6-item-shop-and-unlocking
plan: "01-02"
subsystem: persistence
tags: [blue, item-shop, ef-core, sqlite, xunit]

requires:
  - phase: 01-blue-a6-item-shop-and-unlocking
    provides: Parser-proven Blue shop default data from 01-01
provides:
  - Blue-owned shop season and item persistence tables
  - Zero-start Blue shop season state helpers
  - Blue fixed-width item-shop unlock bit helpers
  - Blue item-shop state regression tests
affects: [blue-a6-item-shop, blue-shop-purchase, blue-userdata-locking, blue-playresult-medals]

tech-stack:
  added: []
  patterns:
    - Blue-owned EF state mirrors Green table shape without sharing Green state
    - Active Blue shop season helper returns null for disabled or empty shops instead of creating state

key-files:
  created:
    - Domain/Enums/BlueShopItemStatus.cs
    - Domain/Entities/BlueShopSeasonState.cs
    - Domain/Entities/BlueShopItemState.cs
    - Infrastructure/Persistence/Migrations/20260528181315_AddBlueItemShopState.cs
    - Infrastructure/Persistence/Migrations/20260528181315_AddBlueItemShopState.Designer.cs
    - Application/Common/BlueShopStateExtensions.cs
    - Application/Common/BlueShopUnlocks.cs
    - Tests/Blue/BlueItemShopStateTests.cs
  modified:
    - Application/Abstractions/ITaikoDbContext.Blue.cs
    - Infrastructure/Persistence/TaikoDbContext.Blue.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Tests/Green/GreenAuthConfigTests.cs

key-decisions:
  - "Blue shop season state starts at zero and never seeds from UserSaveDataBlue medal totals."
  - "Disabled or empty active Blue shop catalogs return no active shop season state instead of creating persisted rows."
  - "Blue item-shop unlock helpers use BlueProtocolBytes fixed widths only."

patterns-established:
  - "BlueShopStateExtensions owns season state creation, active-season gating, and unlocked-item queries for later purchase/readback plans."
  - "BlueShopUnlocks mirrors fixed-width bit operations with BlueProtocolBytes, keeping Green constants out of Blue helpers."

requirements-completed: [SHOP-05, SHOP-08]

duration: 11 min
completed: 2026-05-28
---

# Phase 01 Plan 01-02: Blue Shop Persistence and State Helpers Summary

**Blue-owned item-shop tables and zero-start season helpers for later purchase, medal, and readback flows.**

## Performance

- **Duration:** 11 min
- **Started:** 2026-05-28T18:10:28Z
- **Completed:** 2026-05-28T18:21:32Z
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments

- Added Blue shop season/item entities, status enum, DbSets, EF mappings, model snapshot updates, and generated `AddBlueItemShopState` migration.
- Added zero-start Blue shop state helpers that do not seed from `UserSaveDataBlue.TotalGetDonmedal` or `TotalUseDonmedal`.
- Added Blue fixed-width unlock helpers and regression tests for zero-start state, disabled/empty shop state avoidance, unlocked-item queries, and bit operations.

## Task Commits

1. **Task 1: Add Blue shop entities, DbSets, mappings, and migration** - `97dfedf4` (feat)
2. **Task 2: Add zero-start Blue shop state helpers and tests** - `a2eab19d` (feat)

## Files Created/Modified

- `Domain/Enums/BlueShopItemStatus.cs` - Blue shop item status enum with `Unlocked = 2`.
- `Domain/Entities/BlueShopSeasonState.cs` - Blue-owned BAID/season medal totals.
- `Domain/Entities/BlueShopItemState.cs` - Blue-owned purchased item state.
- `Application/Abstractions/ITaikoDbContext.Blue.cs` - Blue shop DbSets on the application persistence port.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs` - EF mapping for Blue shop tables, composite keys, status conversion, datetime columns, and `UserDatum.Baid` cascade foreign keys.
- `Infrastructure/Persistence/Migrations/20260528181315_AddBlueItemShopState.cs` - Generated migration creating Blue shop tables.
- `Infrastructure/Persistence/Migrations/20260528181315_AddBlueItemShopState.Designer.cs` - Generated migration model metadata.
- `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` - EF model snapshot with Blue shop entities.
- `Application/Common/BlueShopStateExtensions.cs` - Zero-start season state and active-shop query helpers.
- `Application/Common/BlueShopUnlocks.cs` - Blue fixed-width set/clear/has-bit helpers.
- `Tests/Blue/BlueItemShopStateTests.cs` - Regression coverage for state and unlock helpers.
- `Tests/Green/GreenAuthConfigTests.cs` - Throw-only context updated for the expanded Blue persistence interface.

## Decisions Made

- Followed D-09/D-11 exactly: Blue shop state is season-scoped and starts at `0/0` regardless of global Blue medal counters.
- Encoded D-12 as an active-shop helper that returns `null` for disabled or empty shop catalogs without adding `BlueShopSeasonState` rows.
- Kept unlock helper logic Blue-local by calling `BlueProtocolBytes.FixedOrZero`; no Green protocol constants or Green shop state are referenced.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated throw-only ITaikoDbContext test stub**
- **Found during:** Task 1 (Add Blue shop entities, DbSets, mappings, and migration)
- **Issue:** Extending `ITaikoDbContext.Blue.cs` requires all hand-written interface implementations to expose the new Blue shop DbSets.
- **Fix:** Added throwing `BlueShopSeasonStates` and `BlueShopItemStates` properties to the `GreenAuthConfigTests` throw-only context.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopStateTests`
- **Committed in:** `97dfedf4`

---

**Total deviations:** 1 auto-fixed (1 blocking issue)
**Impact on plan:** Required compile support for the planned interface expansion. No functional scope was added.

## Issues Encountered

- The exact plan migration assertion using `$migrations -notmatch 'AddBlueItemShopState'` fails in PowerShell when `dotnet ef migrations list` returns an array containing other migration lines. The raw list contains `20260528181315_AddBlueItemShopState`, and the string-normalized assertion passed.

## Verification

- `dotnet ef migrations add AddBlueItemShopState --project Infrastructure --startup-project Host --output-dir Persistence/Migrations` - passed; generated `20260528181315_AddBlueItemShopState`.
- `$migrations = dotnet ef migrations list --project Infrastructure --startup-project Host; if ($migrations -notmatch 'AddBlueItemShopState') { throw 'AddBlueItemShopState migration missing' }` - executed; failed due PowerShell array matching semantics despite the migration being present.
- `$migrations = dotnet ef migrations list --project Infrastructure --startup-project Host | Out-String; if ($migrations -notmatch 'AddBlueItemShopState') { throw 'AddBlueItemShopState migration missing' }` - passed.
- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopStateTests` - passed, 5 tests.
- Acceptance checks confirmed new Blue helpers contain no `GreenShop`, `GreenProtocolBytes`, `Adapters.GameProtocol.Green`, or `UserSaveDataGreen` references.
- Acceptance checks confirmed `BlueShopUnlocks` calls `BlueProtocolBytes.FixedOrZero`.

## Known Stubs

None. Stub scan hits were limited to standard EF `null!` DbSet initialization and an intentional empty-season test fixture.

## Threat Flags

None. The new persistence surface is covered by the plan threat model: Blue-owned composite keys and `UserDatum.Baid` foreign keys isolate BAID/season/item rows.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `01-03-PLAN.md`. Later shop advertisement and mapper work can rely on Blue-owned state helpers and the generated Blue shop migration.

## Self-Check: PASSED

- Created files exist: Blue shop enum/entities, migration files, helper files, tests, and summary.
- Task commits exist: `97dfedf4`, `a2eab19d`.
- No accidental tracked-file deletions were found in task commits.

---
*Phase: 01-blue-a6-item-shop-and-unlocking*
*Completed: 2026-05-28*
