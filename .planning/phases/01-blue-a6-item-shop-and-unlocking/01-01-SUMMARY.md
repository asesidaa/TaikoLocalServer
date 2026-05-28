---
phase: 01-blue-a6-item-shop-and-unlocking
plan: "01-01"
subsystem: catalog-data
tags: [blue, item-shop, catalog, parser, xunit]

requires:
  - phase: completed-blue-a0-a5
    provides: Blue era catalog, protocol, identity, playresult, and Dani foundations
provides:
  - Parser-proven Blue item-shop default data from local official cache
  - Blue reward shop parser validation for Boost marker, season envelope, and item rows
  - Blue item-shop loader fail-fast regression coverage
affects: [blue-a6-item-shop, shop-default-data, blue-catalog-loading]

tech-stack:
  added: []
  patterns:
    - Blue-owned binary evidence parser feeding committed AC15 JSON catalog data
    - Runtime reads committed JSON while tests prove local binary derivation

key-files:
  created:
    - Infrastructure/GameDataCatalog/Blue/BlueRewardShopDataParser.cs
    - Host/wwwroot/data/blue/blue_item_shop_data.json
    - Tests/Blue/BlueRewardShopDataParserTests.cs
    - Tests/Blue/BlueItemShopLoaderTests.cs
  modified:
    - Tests/Blue/BlueRewardShopDataParserTests.cs

key-decisions:
  - "Decoded the local Blue reward shop cache as one active season with verup_no 20170404, empty telop, 20181219070000 to 20190314020000 date bounds, 30 after-start days, 0 before-close days, and four kigurumi rows."
  - "Kept rewardshopdata.bin as local evidence only; runtime Blue shop loading uses committed blue_item_shop_data.json."

patterns-established:
  - "BlueRewardShopDataParser validates cache structure and supported item domains before default JSON can be trusted."
  - "BlueItemShopLoader tests exercise process-root runtime loading without adding parser runtime dependency."

requirements-completed: [SHOP-03, SHOP-08]

duration: 16 min
completed: 2026-05-28
---

# Phase 01 Plan 01-01: Parser-Proven Blue Shop Default Data Summary

**Blue item-shop default JSON derived from the local official rewardshopdata.bin cache with parser and loader proof.**

## Performance

- **Duration:** 16 min
- **Started:** 2026-05-28T17:48:38Z
- **Completed:** 2026-05-28T18:04:45Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments

- Added `BlueRewardShopDataParser` to validate the Boost serialization marker, season envelope, item count, row layout, supported item types, nonzero IDs, and nonzero prices.
- Committed `blue_item_shop_data.json` from the parsed local cache without copying or tracking `H:\taiko\blue\rewardshopdata.bin`.
- Added parser and loader tests covering cache values, committed JSON parity, missing enabled data, invalid active season, invalid dates, empty rows, duplicate items, unsupported item types, zero IDs, and zero prices.

## Task Commits

1. **Task 1: Decode the official Blue reward shop cache** - `7d53475d` (feat)
2. **Task 2: Commit parser-proven Blue shop JSON and loader tests** - `3292940b` (feat)

## Files Created/Modified

- `Infrastructure/GameDataCatalog/Blue/BlueRewardShopDataParser.cs` - Parses and validates the local official Blue reward shop cache.
- `Host/wwwroot/data/blue/blue_item_shop_data.json` - Runtime Blue item-shop default data in AC15 JSON shape.
- `Tests/Blue/BlueRewardShopDataParserTests.cs` - Proves local cache parsing and committed JSON parity.
- `Tests/Blue/BlueItemShopLoaderTests.cs` - Proves Blue loader default-data and fail-fast behavior.

## Decisions Made

- Used the cache's parsed date envelope directly as protocol date strings: `20181219070000` through `20190314020000`.
- Represented the absent cache telop text as an empty JSON string because no text payload exists in the binary evidence.
- Mapped all four cache rows to supported AC15 item type `3` / kigurumi domain; no title or unsupported item rows were present.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Corrected reward shop row item field layout**
- **Found during:** Task 1 (Decode the official Blue reward shop cache)
- **Issue:** The first parser pass treated item type and ID as two 16-bit fields, which made the official cache decode as unsupported `item_type 0`.
- **Fix:** Parsed the row as two reserved bytes, one byte `item_type`, one byte `item_id`, and a 32-bit price.
- **Files modified:** `Infrastructure/GameDataCatalog/Blue/BlueRewardShopDataParser.cs`, `Tests/Blue/BlueRewardShopDataParserTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRewardShopDataParserTests`
- **Committed in:** `7d53475d`

**2. [Rule 3 - Blocking] Fixed parser compile issue before Task 1 commit**
- **Found during:** Task 1 (Decode the official Blue reward shop cache)
- **Issue:** The item loop passed an `int` index to a helper expecting `uint`, blocking the focused test build.
- **Fix:** Made the parser loop index `uint` and used `ReadExactlyAsync` for complete file reads.
- **Files modified:** `Infrastructure/GameDataCatalog/Blue/BlueRewardShopDataParser.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRewardShopDataParserTests`
- **Committed in:** `7d53475d`

---

**Total deviations:** 2 auto-fixed (1 bug, 1 blocking issue)
**Impact on plan:** Both fixes were required to make the planned parser proof accurate. No scope was added beyond the plan.

## Issues Encountered

None remaining. The initial parser offset mistake was fixed before committing Task 1.

## User Setup Required

None - no external service configuration required.

## Verification

- `if (-not (Test-Path -LiteralPath 'H:\taiko\blue\rewardshopdata.bin')) { throw 'missing rewardshopdata.bin' }`
- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRewardShopDataParserTests` - passed, 6 tests
- `if (-not (Test-Path -LiteralPath 'Host/wwwroot/data/blue/blue_item_shop_data.json')) { throw 'missing blue_item_shop_data.json' }`
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueRewardShopDataParserTests|FullyQualifiedName~BlueItemShopLoaderTests"` - passed, 18 tests
- `git ls-files | rg -q "rewardshopdata\.bin"` - no tracked binary

## Known Stubs

None. The empty `telop` value is the parser-derived official cache value, not placeholder UI or runtime mock data.

## Next Phase Readiness

Ready for `01-02-PLAN.md`. Runtime Blue shop persistence can now rely on committed, parser-proven default shop rows.

## Self-Check: PASSED

- Created files exist: parser, default JSON, parser tests, loader tests, and summary.
- Task commits exist: `7d53475d`, `3292940b`.
- `rewardshopdata.bin` remains local-only and untracked.

---
*Phase: 01-blue-a6-item-shop-and-unlocking*
*Completed: 2026-05-28*
