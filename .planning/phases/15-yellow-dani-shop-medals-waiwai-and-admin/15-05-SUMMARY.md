---
phase: 15-yellow-dani-shop-medals-waiwai-and-admin
plan: "05"
subsystem: application
tags: [yellow, ac15, medals, item-shop, userdata]
requires:
  - phase: 15-yellow-dani-shop-medals-waiwai-and-admin
    provides: Yellow itempurchase route, shop state, and purchase rows
provides:
  - Active Yellow shop-season Don medal accumulation from playresults
  - Yellow BAID readback of active shop-season Don medal totals
  - Yellow userdata shop lock readback from Yellow purchased item rows
affects: [yellow, phase-15, medals, item-shop, userdata]
tech-stack:
  added: []
  patterns: [active AC15 shop balance routing, Yellow-owned shop lock readback]
key-files:
  created: []
  modified:
    - Application/Handlers/UpdatePlayResultCommand.Yellow.cs
    - Application/Handlers/BaidQuery.Yellow.cs
    - Application/Handlers/UserDataQuery.Yellow.cs
    - Tests/Yellow/YellowPlayResultHandlerTests.cs
    - Tests/Yellow/YellowItemShopPurchaseTests.cs
    - Tests/Yellow/YellowUserDataProtocolTests.cs
key-decisions:
  - "When an active Yellow item-shop season exists, Yellow playresult Don medals update `YellowShopSeasonState`; otherwise they continue to update `UserSaveDataYellow`."
  - "Yellow Katsu medals remain profile/readback-only and are never purchase currency."
  - "Yellow userdata shop locks are unlocked only by matching Yellow purchased item rows, not Blue or Green shop rows."
patterns-established:
  - "Yellow BAID and userdata readback use active Yellow shop state through Yellow-owned helpers instead of duplicating Blue/Green table queries."
requirements-completed: [YSHOP-02, YMED-01]
duration: 16 min
completed: 2026-06-08
---

# Phase 15 Plan 05: Yellow Medals And Shop-Lock Readback Summary

**Yellow playresults now feed active shop Don balances, while userdata shop locks read only Yellow purchased item rows.**

## Performance

- **Duration:** 16 min
- **Started:** 2026-06-08T04:00:40Z
- **Completed:** 2026-06-08T04:16:08Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments

- Routed Yellow playresult Don medals into `YellowShopSeasonState` when a non-empty active Yellow shop season exists.
- Preserved Phase 14 no-active-shop behavior by keeping Don medal accumulation on `UserSaveDataYellow` when no active shop exists.
- Kept Katsu medal accumulation on `UserSaveDataYellow` and added a guard proving Katsu cannot fund Yellow item purchases.
- Updated Yellow BAID readback to report active shop-season Don totals when present.
- Updated Yellow userdata readback to pass active-season Yellow purchased item rows into the AC15 shop-lock adapter.
- Added tests proving Blue/Green purchased rows do not unlock Yellow shop songs or tones.

## Task Commits

1. **Task 1: Add active-shop Don medal accumulation** - `3d5a2d63` (feat)
2. **Task 2: Feed Yellow purchased items into userdata shop locks** - `b73bab15` (feat)

**Plan metadata:** committed separately with this summary.

## Files Created/Modified

- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` - Uses the active Yellow shop season as the Don medal balance when present.
- `Application/Handlers/BaidQuery.Yellow.cs` - Reports active Yellow shop-season Don totals through BAID readback.
- `Application/Handlers/UserDataQuery.Yellow.cs` - Loads active-season Yellow unlocked shop item tuples for userdata masking.
- `Tests/Yellow/YellowPlayResultHandlerTests.cs` - Covers active-season Don accumulation, BAID totals, and overflow no-mutation behavior.
- `Tests/Yellow/YellowItemShopPurchaseTests.cs` - Proves Katsu medals are not item-shop purchase currency.
- `Tests/Yellow/YellowUserDataProtocolTests.cs` - Proves Yellow userdata shop locks ignore Blue/Green purchased rows and respond to Yellow purchased rows.

## Decisions Made

- Active Yellow shop seasons are the purchase balance authority for Don medals, but save-row Don totals remain the fallback when no active season exists.
- Katsu medals were left profile-only because no Yellow protocol evidence or shop contract supports spending them.
- Userdata lock readback uses `GetUnlockedYellowShopItemsAsync` so the table boundary stays auditable.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- The RED Task 1 test initially had a `BaidQueryHandler` constructor argument order mistake; fixing the test setup allowed the intended medal-routing failures to surface before implementation.

## Verification

- RED: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPlayResult"` - failed before implementation because active-shop Yellow playresults did not create/update `YellowShopSeasonState` and BAID readback did not report active-season totals.
- RED: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPersistenceBoundary"` - failed before implementation because Yellow userdata still passed an empty unlocked-shop set after Yellow purchase rows existed.
- Task 1 focused: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPlayResult"` - passed, 50 tests.
- Task 2 focused: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPersistenceBoundary"` - passed, 10 tests.
- Plan: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPersistenceBoundary"` - passed, 60 tests.
- Plan: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` - passed, 160 tests.
- Plan: `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase15-yellow"` - passed, 0 warnings, 0 errors.
- Plan: `git diff --check -- Application Tests .planning` - passed with no whitespace errors.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `15-06-PLAN.md`. Yellow medals now flow through the active shop state needed for purchases, and userdata lock readback uses Yellow-owned purchased item rows only. Phase-level verification, code review, and Phase 16 were not started.

## Self-Check: PASSED

- Summary exists on disk.
- Task commits `3d5a2d63` and `b73bab15` exist in git history.
- Plan-level verification commands passed before summary creation.

---
*Phase: 15-yellow-dani-shop-medals-waiwai-and-admin*
*Completed: 2026-06-08*
