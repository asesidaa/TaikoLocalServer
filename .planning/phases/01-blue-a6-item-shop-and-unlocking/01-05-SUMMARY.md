---
phase: 01-blue-a6-item-shop-and-unlocking
plan: "01-05"
subsystem: game-protocol
tags: [blue, item-shop, baid, userdata, locking, xunit]

requires:
  - phase: 01-blue-a6-item-shop-and-unlocking
    provides: Parser-proven Blue shop catalog data and Blue-owned shop persistence/state helpers from 01-01 and 01-02
provides:
  - Blue userdata active-season song and tone lock filtering
  - Blue BAID active-season costume lock filtering for item types 3 through 7
  - Blue BAID active-season Don medal total readback with disabled-shop zeroing
  - Focused Blue item-shop locking regression tests
affects: [blue-a6-item-shop, blue-userdata-readback, blue-baid-readback, blue-shop-purchase]

tech-stack:
  added: []
  patterns:
    - Blue readback derives active-season lock state from Blue catalog rows and BlueShopItemStates only
    - Blue item type to save-field mapping uses BlueProtocolBytes fixed widths for all readback filtering

key-files:
  created:
    - Tests/Blue/BlueItemShopLockingTests.cs
  modified:
    - Application/Handlers/BaidQuery.Blue.cs
    - Application/Handlers/UserDataQuery.Blue.cs

key-decisions:
  - "Blue userdata hides active-season shop songs and tones until matching unlocked BlueShopItemState rows exist."
  - "Blue BAID hides active-season costume item types 3..7 with the D-18 save-field mapping and BlueProtocolBytes.CostumeFlagBytes."
  - "Blue BAID reports active-season BlueShopSeasonState Don medal totals when the shop is enabled and 0/0 when disabled."

patterns-established:
  - "Readback locking composes catalog active-season rows with purchased BlueShopItemStates, rather than mutating save bits during read."
  - "Disabled Blue shop BAID responses do not fall back to UserSaveDataBlue global Don medal counters."

requirements-completed: [SHOP-05, SHOP-07, SHOP-08]

duration: 7 min
completed: 2026-05-28
---

# Phase 01 Plan 01-05: Blue BAID and Userdata Locking Summary

**Blue BAID and userdata readback now hide active-season shop content until Blue purchase state unlocks it.**

## Performance

- **Duration:** 7 min
- **Started:** 2026-05-28T18:46:05Z
- **Completed:** 2026-05-28T18:53:32Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments

- Added Blue userdata filtering for active-season shop songs and tones using `BlueShopItemStates` and `BlueProtocolBytes` widths.
- Added Blue BAID filtering for costume item types `3..7` using the D-18 mapping: kigurumi, body, head, face, and puchi fields.
- Changed Blue BAID Don medal readback to use active-season `BlueShopSeasonState` totals when the shop is enabled, and `0/0` when disabled.
- Added focused Blue locking tests for hidden and purchased-visible songs, tones, costumes, and BAID medal totals.

## Task Commits

Each task was committed atomically:

1. **Task 1: Hide locked active-season songs and tones in userdata** - `d59d9510` (feat)
2. **Task 2: Hide locked costumes and report BAID shop totals** - `b43f3eba` (feat)

**Plan metadata:** recorded in the final docs commit for this plan

## Files Created/Modified

- `Application/Handlers/UserDataQuery.Blue.cs` - Clears locked active-season shop song and tone bits from Blue userdata readback until matching unlocked rows exist.
- `Application/Handlers/BaidQuery.Blue.cs` - Clears locked active-season costume bits and reports enabled-season Don medal totals or disabled-shop zero totals.
- `Tests/Blue/BlueItemShopLockingTests.cs` - Covers locked and unlocked readback for songs, tones, item types `3..7`, and BAID medal totals.

## Decisions Made

- Followed the plan's narrow readback scope and did not implement purchase behavior in this plan.
- Used direct Blue catalog and `BlueShopItemStates` lookups for lock filtering, with no Green shop or Green protocol references.
- Treated disabled Blue shop BAID totals as `0/0`, even if global `UserSaveDataBlue` Don medal counters are nonzero.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Verification

- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopLockingTests` - passed, 6 tests.
- Acceptance checks confirmed Blue readback files query `BlueShopItemStates`, use `BlueProtocolBytes` widths, and contain no Green shop or Green protocol references.
- Stub scan found only literal test fixture values; no runtime stubs or placeholder UI/data paths were introduced.

## Known Stubs

None.

## Next Phase Readiness

Ready for `01-04-PLAN.md`. Purchase handling can now rely on readback locks hiding active-season Blue shop content until purchase state writes unlocked rows.

## Self-Check: PASSED

- Created summary and test file exist on disk.
- Task commits exist: `d59d9510`, `b43f3eba`.
- No accidental tracked-file deletions were found in task commits.

---
*Phase: 01-blue-a6-item-shop-and-unlocking*
*Completed: 2026-05-28*
