---
phase: 15-yellow-dani-shop-medals-waiwai-and-admin
plan: "04"
subsystem: protocol
tags: [yellow, ac15, item-shop, medals, protobuf]
requires:
  - phase: 15-yellow-dani-shop-medals-waiwai-and-admin
    provides: Yellow-owned shop state schema and active-season helpers
provides:
  - Yellow Mediator-backed itempurchase route
  - Yellow itempurchase request and response wire mapping
  - Yellow AC15 purchase handler and unlock adapter
  - Reward route stateless-compatibility guards
affects: [yellow, phase-15, item-shop, medals, userdata]
tech-stack:
  added: []
  patterns: [Yellow wire-to-common mapper, AC15 purchase service adapter, reward compatibility guard]
key-files:
  created:
    - Application/Ac15/YellowAc15ItemShopAdapter.cs
    - Application/Handlers/ItemPurchaseCommand.Yellow.cs
  modified:
    - Application/Handlers/ItemPurchaseCommand.cs
    - Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs
    - Adapters.GameProtocol.Yellow/Mappers/ItemShopMappers.cs
    - Tests/Yellow/YellowItemShopPurchaseTests.cs
    - Tests/Yellow/YellowCatalogBoundaryTests.cs
    - Tests/Yellow/YellowRouteSkeletonTests.cs
key-decisions:
  - "Yellow itempurchase now uses the shared AC15 purchase validation flow behind a Yellow-owned persistence and unlock adapter."
  - "Yellow title-shop unlocks were not invented because the current shared AC15 shop item type contract exposes only Song, Tone, Kigurumi, Body, Head, Face, and Puchi."
  - "Yellow rewardcardcheck.php and rewardexecution.php remain stateless success compatibility routes and do not mutate shop, medal, unlock, or Banacoin state."
patterns-established:
  - "Yellow purchase controllers map Yellow wire DTOs to common application commands, send Mediator, and map common totals back to Yellow wire."
  - "Unsupported Yellow shop item types fail before spend/unlock rather than being persisted as unknown unlock state."
requirements-completed: [YSHOP-01, YSHOP-02]
duration: 25 min
completed: 2026-06-08
---

# Phase 15 Plan 04: Stateful Yellow Itempurchase Summary

**Yellow itempurchase now validates active catalog rows, spends Yellow Don medals, and returns Yellow wire totals while reward routes stay stateless.**

## Performance

- **Duration:** 25 min
- **Started:** 2026-06-08T03:34:00Z
- **Completed:** 2026-06-08T03:58:44Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments

- Added the Yellow branch to `ItemPurchaseCommandHandler` and implemented `ItemPurchaseCommand.Yellow.cs`.
- Added `YellowAc15ItemShopAdapter` for Yellow season state, duplicate checks, purchased rows, and supported Yellow save-flag unlocks.
- Added Yellow `ItempurchaseRequest` and `ItempurchaseResponse` mapper overloads, including optional tuple fields and Don medal totals.
- Replaced the Yellow `itempurchase.php` scaffold with a Mediator-backed route while keeping `getitemshopinfo.php` shape unchanged.
- Added reward compatibility tests proving `rewardcardcheck.php` and `rewardexecution.php` return success without mutating Yellow shop/save state.

## Task Commits

1. **Task 1: Add Yellow item-shop adapter and handler** - `0ccd8f16`, `286627f8`, `0aec0ae7` (test/test/feat)
2. **Task 2: Wire Yellow itempurchase route and preserve reward compatibility** - `01a4be06` (feat)

**Plan metadata:** committed separately with this summary.

## Files Created/Modified

- `Application/Ac15/YellowAc15ItemShopAdapter.cs` - Yellow AC15 purchase persistence and unlock adapter.
- `Application/Handlers/ItemPurchaseCommand.cs` - Adds Yellow dispatch.
- `Application/Handlers/ItemPurchaseCommand.Yellow.cs` - Loads Yellow save/catalog snapshot and invokes `Ac15ItemShopService`.
- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` - Makes `itempurchase.php` Mediator-backed and leaves reward routes stateless.
- `Adapters.GameProtocol.Yellow/Mappers/ItemShopMappers.cs` - Adds Yellow itempurchase command and response mapping.
- `Tests/Yellow/YellowItemShopPurchaseTests.cs` - Covers purchase validation, unsupported types, wire mapping, route source guard, and reward statelessness.
- `Tests/Yellow/YellowCatalogBoundaryTests.cs` - Updates Yellow implemented/deferred route boundaries for Plan 04.
- `Tests/Yellow/YellowRouteSkeletonTests.cs` - Updates the Yellow Mediator route count for the stateful purchase route.

## Decisions Made

- Did not invent a Title item type or protocol value. The current shared AC15 contract supports item type values for Song, Tone, Kigurumi, Body, Head, Face, and Puchi only.
- Kept Yellow `getitemshopinfo.php` on the Phase 13 catalog-backed shape, without adding Blue-only start/end timing fields.
- Kept reward routes as log-and-success compatibility only; purchase effects happen exclusively through `itempurchase.php`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated Yellow route source guards**
- **Found during:** Task 2 verification.
- **Issue:** Existing Yellow route guard tests still described `ItemPurchaseController` as a deferred no-state scaffold.
- **Fix:** Moved `ItemPurchaseController` into the implemented Mediator-backed controller set and updated the allowed Mediator count from 13 to 14.
- **Files modified:** `Tests/Yellow/YellowCatalogBoundaryTests.cs`, `Tests/Yellow/YellowRouteSkeletonTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPersistenceBoundary"` passed.
- **Committed in:** `01a4be06`

---

**Total deviations:** 1 auto-fixed (1 blocking). **Impact:** Required to keep boundary tests aligned with the intended Plan 04 route transition; no extra runtime scope was added.

## Issues Encountered

- The RED route/mapping tests initially failed at compile time because Yellow itempurchase mapper overloads were absent. The test also surfaced that Yellow `RewardcardcheckRequest` has no BAID field, so the reward compatibility request was corrected to use the actual generated Yellow wire shape.

## Verification

- RED: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPersistenceBoundary"` - failed before implementation because Yellow `ItempurchaseRequest`/`CommonItemPurchaseResponse` mapper overloads were missing.
- Task 2 focused: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPersistenceBoundary"` - passed, 33 tests.
- Plan: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPersistenceBoundary"` - passed, 33 tests.
- Plan: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~ItemShop|FullyQualifiedName~Yellow"` - passed, 243 tests.
- Plan: `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase15-yellow"` - passed, 0 warnings, 0 errors.
- Plan: `git diff --check -- Application Adapters.GameProtocol.Yellow Tests .planning` - passed with no whitespace errors.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `15-05-PLAN.md`. Yellow itempurchase now has the route/handler/adapter foundation needed for active-shop Don medal accumulation and userdata shop-lock readback. Phase-level verification, code review, and Phase 16 were not started.

## Self-Check: PASSED

- Summary exists on disk.
- Task commits `0ccd8f16`, `286627f8`, `0aec0ae7`, and `01a4be06` exist in git history.
- Plan-level verification commands passed before summary creation.

---
*Phase: 15-yellow-dani-shop-medals-waiwai-and-admin*
*Completed: 2026-06-08*
