---
phase: 15-yellow-dani-shop-medals-waiwai-and-admin
plan: "03"
subsystem: database
tags: [yellow, ac15, item-shop, medals, ef-core]
requires:
  - phase: 15-yellow-dani-shop-medals-waiwai-and-admin
    provides: Yellow-owned save state and Phase 14 medal totals
provides:
  - Yellow-owned shop season and purchased-item EF tables
  - Yellow active-season shop state helpers seeded from Yellow save medal totals
  - Yellow unlocked shop item lookup isolated from Blue and Green shop rows
affects: [yellow, phase-15, item-shop, medals, userdata]
tech-stack:
  added: []
  patterns: [Yellow-owned EF slice, active AC15 shop season helper, era-boundary tests]
key-files:
  created:
    - Domain/Enums/YellowShopItemStatus.cs
    - Domain/Entities/YellowShopSeasonState.cs
    - Domain/Entities/YellowShopItemState.cs
    - Application/Common/YellowShopStateExtensions.cs
    - Infrastructure/Persistence/Migrations/20260608030723_AddYellowShopState.cs
    - Tests/Yellow/YellowItemShopPurchaseTests.cs
  modified:
    - Application/Abstractions/ITaikoDbContext.Yellow.cs
    - Infrastructure/Persistence/TaikoDbContext.Yellow.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Tests/Yellow/YellowPersistenceBoundaryTests.cs
    - Tests/Green/GreenAuthConfigTests.cs
key-decisions:
  - "Yellow shop season and item purchase state uses Yellow-owned tables rather than Blue/Green shop rows or a shared discriminator."
  - "First active Yellow shop season creation seeds Don medal balances from `UserSaveDataYellow` so Phase 14 medal totals remain spendable."
  - "Disabled or empty Yellow item-shop catalogs do not create Yellow shop season rows."
patterns-established:
  - "Yellow shop schema mirrors the proven AC15 key shape while preserving era-specific table names, DbSets, and helper methods."
  - "Yellow unlocked-shop lookup returns only `YellowShopItemStatus.Unlocked` rows from Yellow-owned purchase state."
requirements-completed: [YSHOP-02, YMED-01]
duration: 31 min
completed: 2026-06-08
---

# Phase 15 Plan 03: Yellow Shop State Schema Summary

**Yellow-owned item-shop season and purchase state with active-season Don medal seeding**

## Performance

- **Duration:** 31 min
- **Started:** 2026-06-08T03:01:04Z
- **Completed:** 2026-06-08T03:32:20Z
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments

- Added Yellow-owned `YellowShopSeasonState` and `YellowShopItemState` entities, DbSets, EF mappings, and migration.
- Added `YellowShopItemStatus.Unlocked` for purchased Yellow item rows without reusing Blue/Green enum types.
- Added Yellow active-season helpers that seed the first active season from Yellow save Don medal totals and skip disabled or empty shop catalogs.
- Added unlocked item lookup tests proving only Yellow purchased item rows affect Yellow shop state.

## Task Commits

1. **Task 1: Add Yellow shop season and item tables** - `aee6c207` (feat)
2. **Task 2: Add Yellow active-season and unlock lookup helpers** - `9bd5e097` (feat)

**Plan metadata:** committed separately with this summary.

## Files Created/Modified

- `Domain/Enums/YellowShopItemStatus.cs` - Yellow-owned purchased item status enum.
- `Domain/Entities/YellowShopSeasonState.cs` - Yellow active shop season medal balance row keyed by BAID and season id.
- `Domain/Entities/YellowShopItemState.cs` - Yellow purchased shop item row keyed by BAID, season id, item type, and item id.
- `Application/Abstractions/ITaikoDbContext.Yellow.cs` - Adds Yellow shop DbSet contracts.
- `Infrastructure/Persistence/TaikoDbContext.Yellow.cs` and migration files - Map Yellow shop tables and relationships.
- `Application/Common/YellowShopStateExtensions.cs` - Adds active-season creation, direct lookup, and unlocked item tuple helpers.
- `Tests/Yellow/YellowPersistenceBoundaryTests.cs` - Adds Yellow shop table and no-shared-discriminator boundary checks.
- `Tests/Yellow/YellowItemShopPurchaseTests.cs` - Covers active-season seeding, disabled/empty catalogs, and Yellow-only unlocked item lookup.
- `Tests/Green/GreenAuthConfigTests.cs` - Extends the throw-only context test double for the expanded Yellow DbSet contract.

## Decisions Made

- Yellow shop state is stored in Yellow-owned EF tables even though the key shape matches the Blue/Green AC15 shop pattern.
- The first Yellow active shop season imports existing Yellow save Don medal totals; later seasons for the same BAID start at zero so old-season balances are not copied forward.
- Empty active shop catalogs are treated the same as disabled catalogs for state creation.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated handwritten ITaikoDbContext test double**
- **Found during:** Task 1 verification
- **Issue:** `Tests/Green/GreenAuthConfigTests.ThrowingTaikoDbContext` needed members for the new Yellow shop DbSets after `ITaikoDbContext.Yellow.cs` was expanded.
- **Fix:** Added throw-only `YellowShopSeasonStates` and `YellowShopItemStates` members to the test double.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPersistenceBoundary"` passed.
- **Committed in:** `aee6c207`

---

**Total deviations:** 1 auto-fixed (1 blocking). **Impact:** Required to keep existing test infrastructure compiling after the intended interface expansion; no behavior or scope change.

## Issues Encountered

- None beyond the expected handwritten test-double update documented above.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop"` - passed, 4 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPersistenceBoundary"` - passed, 10 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase15-yellow"` - passed, 0 warnings, 0 errors.
- `git diff --check -- Domain Application Infrastructure Tests .planning` - passed with no whitespace errors.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `15-04-PLAN.md`. Yellow now has the shop season and purchase-state foundation needed for stateful `itempurchase.php`, Don medal spending, and userdata shop-lock readback. Phase-level verification, code review, and Phase 16 were not started.

## Self-Check: PASSED

- Created files exist on disk.
- Task commits `aee6c207` and `9bd5e097` exist in git history.
- Plan-level verification commands passed before summary creation.

---
*Phase: 15-yellow-dani-shop-medals-waiwai-and-admin*
*Completed: 2026-06-08*
