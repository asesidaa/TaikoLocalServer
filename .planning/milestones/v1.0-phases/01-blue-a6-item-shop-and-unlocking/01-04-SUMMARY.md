---
phase: 01-blue-a6-item-shop-and-unlocking
plan: "01-04"
subsystem: game-protocol
tags: [blue, item-shop, purchase, rewardexecution, playresult, xunit]

requires:
  - phase: 01-blue-a6-item-shop-and-unlocking
    provides: Blue shop catalog data, persistence helpers, protocol advertisement, and readback locking from plans 01-01 through 01-03 and 01-05
provides:
  - Blue itempurchase validation, season medal spend, purchase persistence, and immediate save-bit unlocks
  - Era-aware ItemPurchaseCommand dispatch for Green and Blue
  - Blue playresult Don medal accrual into active BlueShopSeasonState when shop is enabled
  - Rewardexecution success no-op regression coverage
affects: [blue-a6-item-shop, blue-purchase, blue-playresult-medals, blue-rewardexecution, green-item-shop-regression]

tech-stack:
  added: []
  patterns:
    - Era-dispatched item purchase command with era-specific partial handlers
    - Blue item-shop purchase mutates only Blue shop rows and Blue save fields
    - Rewardexecution remains controller-local success no-op for Phase 1

key-files:
  created:
    - Application/Handlers/ItemPurchaseCommand.Blue.cs
    - Tests/Blue/BlueItemShopPurchaseTests.cs
  modified:
    - Application/Handlers/ItemPurchaseCommand.cs
    - Application/Handlers/ItemPurchaseCommand.Green.cs
    - Application/Handlers/UpdatePlayResultCommand.Blue.cs
    - Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs
    - Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs
    - Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs
    - Tests/Blue/BlueItemShopStateTests.cs
    - Tests/Blue/BlueItemShopProtocolTests.cs
    - Tests/Green/GreenItemShopPurchaseTests.cs
    - Tests/Green/GreenGhostRewardTests.cs

key-decisions:
  - "ItemPurchaseCommand now carries an explicit GameEra and dispatches to Green or Blue partial handlers."
  - "Blue itempurchase owns spend, purchase persistence, and immediate D-18 save-bit unlocks for item types 1 through 7."
  - "Blue rewardexecution remains a Phase 1 log-and-success no-op with no Mediator call or state mutation."
  - "Enabled-shop Blue playresult Don medals accrue to BlueShopSeasonState; disabled or inactive shops retain existing non-shop Blue save behavior."

patterns-established:
  - "Blue purchase validation compares item_no, item_type, item_id, and item_price to the active Blue catalog before any mutation."
  - "Blue purchase duplicate detection uses the BlueShopItemStates composite key of BAID, season, item type, and item id."
  - "Blue shop tests snapshot save-bit arrays to prove exact unlock-field mutation or no-op behavior."

requirements-completed: [SHOP-04, SHOP-05, SHOP-06, SHOP-08]

duration: 14 min
completed: 2026-05-28
---

# Phase 01 Plan 01-04: Blue Purchase, Rewardexecution No-Op, and Playresult Medals Summary

**Blue itempurchase now validates active-season catalog tuples, spends season Don medals, persists purchases, and unlocks Blue save bits while rewardexecution stays non-mutating.**

## Performance

- **Duration:** 14 min
- **Started:** 2026-05-28T19:01:52Z
- **Completed:** 2026-05-28T19:15:16Z
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments

- Added era-aware `ItemPurchaseCommand` dispatch and a Blue purchase handler that validates catalog tuples before spending medals or applying unlocks.
- Wired Blue `itempurchase.php` through Mediator and Blue item-shop mappers while keeping Green mappers/tests on `GameEra.Green`.
- Added Blue purchase tests for preflight, disabled shop `0/0`, invalid tuples, zero-price rows, insufficient medals, duplicate purchases, successful persistence, and item types `1..7`.
- Moved enabled-shop Blue playresult Don medal accrual into `BlueShopSeasonState` and added disabled/no-active-season regression tests.
- Added rewardexecution coverage proving success no-op behavior leaves Blue shop rows and all Blue unlock bit fields unchanged.

## Task Commits

Each task was committed atomically:

1. **Task 1: Implement Blue itempurchase validation, spend, and unlock** - `47e2eca2` (feat)
2. **Task 2: Preserve rewardexecution no-op and move enabled-shop Don medals to Blue season state** - `d5267928` (feat)

**Plan metadata:** recorded in the final docs commit for this plan.

## Files Created/Modified

- `Application/Handlers/ItemPurchaseCommand.cs` - Adds explicit era dispatch and shared preflight/arithmetic helpers.
- `Application/Handlers/ItemPurchaseCommand.Green.cs` - Moves Green purchase behavior behind `HandleGreen`.
- `Application/Handlers/ItemPurchaseCommand.Blue.cs` - Adds Blue catalog validation, spend, purchase persistence, and D-18 unlock mapping.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - Routes enabled-shop Don medals into active Blue shop season state.
- `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs` - Maps Green purchase requests with `GameEra.Green`.
- `Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs` - Maps Blue purchase requests with `GameEra.Blue`.
- `Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs` - Sends Blue purchase requests through Mediator and maps the response.
- `Tests/Blue/BlueItemShopPurchaseTests.cs` - Covers purchase validation, spend, duplicate, disabled, and item type unlock behavior.
- `Tests/Blue/BlueItemShopStateTests.cs` - Covers enabled, disabled, and no-active-season Blue playresult medal behavior.
- `Tests/Blue/BlueItemShopProtocolTests.cs` - Covers Blue mapper era values and rewardexecution no-op behavior.
- `Tests/Green/GreenItemShopPurchaseTests.cs` - Updates direct command construction for explicit Green era.
- `Tests/Green/GreenGhostRewardTests.cs` - Updates direct Green purchase command construction after shared command dispatch changed.

## Decisions Made

- Followed the plan's D-02 interpretation of SHOP-06: purchase performs Phase 1 unlocks immediately, while rewardexecution remains success no-op.
- Returned success-shaped `0/0` Blue purchase totals when the shop is disabled or has no active season, matching D-13.
- Kept disabled/no-active-season Blue playresult Don medals on existing `UserSaveDataBlue.TotalGetDonmedal` behavior because no active shop accounting path exists in that mode.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated Green ghost reward purchase tests for explicit era dispatch**
- **Found during:** Task 1 (Implement Blue itempurchase validation, spend, and unlock)
- **Issue:** Adding `GameEra Era` to `ItemPurchaseCommand` required all direct command construction sites to pass an era. `Tests/Green/GreenGhostRewardTests.cs` was outside the plan file list but would fail compilation after the shared command shape changed.
- **Fix:** Updated those Green test calls to pass `GameEra.Green`, matching the Green mapper behavior.
- **Files modified:** `Tests/Green/GreenGhostRewardTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopPurchaseTests`; `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenItemShopPurchaseTests`; final Green item-shop verification command.
- **Committed in:** `47e2eca2`

---

**Total deviations:** 1 auto-fixed (1 blocking issue)
**Impact on plan:** Required by the planned shared command API change. No runtime scope was added.

## Issues Encountered

None remaining.

## Verification

- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopPurchaseTests` - passed, 14 tests.
- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenItemShopPurchaseTests` - passed, 5 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShopStateTests|FullyQualifiedName~BlueRewardExecution"` - passed, 9 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShopPurchaseTests|FullyQualifiedName~BlueItemShopStateTests|FullyQualifiedName~BlueRewardExecution"` - passed, 23 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopPurchaseTests|FullyQualifiedName~GreenItemShopStateTests"` - passed, 8 tests.
- Acceptance checks confirmed `ItemPurchaseCommand.Blue.cs` and Blue item-shop adapter files contain no `GreenShop`, `GreenProtocolBytes`, `Adapters.GameProtocol.Green`, or `UserSaveDataGreen` references.
- Acceptance checks confirmed `RewardExecutionController.cs` does not call `Mediator.Send`.

## Known Stubs

None. Stub scan hits were intentional empty active-season test fixtures in `BlueItemShopStateTests` and `BlueItemShopProtocolTests`.

## Threat Flags

None. The new purchase, save-state mutation, medal arithmetic, and rewardexecution surfaces are covered by T-01-10 through T-01-13 in the plan threat model.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `01-06-PLAN.md`. Blue item-shop purchase and medal mutation behavior is now implemented and covered by targeted regressions; the next plan can add final verification/source guard coverage.

## Self-Check: PASSED

- Created files exist: summary, Blue purchase handler, and Blue purchase tests.
- Task commits exist: `47e2eca2`, `d5267928`.
- No accidental tracked-file deletions were found in task commits.

---
*Phase: 01-blue-a6-item-shop-and-unlocking*
*Completed: 2026-05-28*
