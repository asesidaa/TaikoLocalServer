---
phase: 05-blue-battle-runtime-support
plan: "05-08"
subsystem: blue-battle-runtime
tags: [blue, battle, initialdata, catalog, protobuf, tdd, tests, gsd]

requires:
  - phase: 05-blue-battle-runtime-support
    provides: "05-05 BlueBattle state, 05-07 battleuserdata readback, and the 2026-05-31 user approval for data-derived battle initialdata"
provides:
  - "Blue battle initialdata advertisement from the parsed five-file battle XML set"
  - "Data-derived initial stage and special release bitsets"
  - "NPC progression-derived battle bonds level cap"
  - "Concrete false/zero battle initialdata defaults when battle XML is unavailable"
affects: [05-11-blue-battle-closeout, phase-06-full-blue-verification]

tech-stack:
  added: []
  patterns: [blue-owned-battle-catalog-boundary, protobuf-presence-with-concrete-defaults, tdd-red-green]

key-files:
  created:
    - .planning/phases/05-blue-battle-runtime-support/05-08-SUMMARY.md
  modified:
    - Application/Catalog/Blue/BlueBattleCatalog.cs
    - Application/Common/BlueProtocolBytes.cs
    - Application/Handlers/GetInitialDataQuery.Blue.cs
    - Infrastructure/GameDataCatalog/Blue/BlueBattleDataLoader.cs
    - Tests/Blue/BlueInitialDataTests.cs
    - Tests/Blue/BlueBattleCatalogLoaderTests.cs
    - .planning/phases/05-blue-battle-runtime-support/05-RESOLUTION.md
    - .planning/phases/05-blue-battle-runtime-support/05-08-PLAN.md

key-decisions:
  - "The 2026-05-31 user decision supersedes prior fail-closed initialdata rows for this stage: emit data-derived Blue battle initialdata when required local battle XML exists and parses."
  - "Blue initialdata emits explicit false/zero battle defaults when the required battle XML set is unavailable instead of relying on omitted optional fields."
  - "Battle stage IDs, special move reward IDs, and bonds level cap are parsed through the Blue battle catalog boundary; handlers and mappers do not hardcode the current values."

patterns-established:
  - "BlueBattleCatalog carries parsed initialdata inputs from battle XML while the handler only converts catalog values into protocol-width bitsets."
  - "Initialdata optional protobuf fields are still mapper presence-aware, but the Blue handler now supplies concrete battle defaults."

requirements-completed: [BTL-03, BTL-06]

duration: 12 min
completed: 2026-05-30
---

# Phase 05 Plan 05-08: Blue Battle Initialdata Summary

**Blue battle initialdata now advertises parsed local battle content with concrete defaults when data is unavailable**

## Performance

- **Duration:** 12 min
- **Started:** 2026-05-30T20:36:36Z
- **Completed:** 2026-05-30T20:48:01Z
- **Tasks:** 1
- **Files modified:** 8

## Accomplishments

- Added RED tests proving Blue initialdata now emits concrete false/zero battle defaults and advertises parsed battle catalog values.
- Extended `BlueBattleDataLoader` and `BlueBattleCatalog` to parse stage IDs from `battlestageinfo.xml`, special move reward IDs from `battletokeninfo.xml`, and bonds cap from `battlenpcinfo.xml` `requred_exp` entries.
- Wired `GetInitialDataQuery.Blue.cs` to set `is_battleplay`, `release_battle_stage_flg`, `release_battle_special_flg`, and `battle_bonds_lv_cap` from the Blue battle catalog only.
- Updated `05-RESOLUTION.md` and `05-08-PLAN.md` to record the 2026-05-31 user approval that supersedes the older fail-closed initialdata rows.

## Task Commits

1. **Task 1 RED: Add failing tests for data-derived battle initialdata** - `82411ad5` (test)
2. **Task 1 GREEN: Derive Blue battle initialdata from parsed catalog** - `fe248d7d` (feat)

## Files Created/Modified

- `Tests/Blue/BlueInitialDataTests.cs` - RED/GREEN coverage for concrete defaults and parsed battle initialdata values.
- `Tests/Blue/BlueBattleCatalogLoaderTests.cs` - Loader expectations for parsed stage IDs, special move reward IDs, bonds cap, and source guard updates.
- `Application/Catalog/Blue/BlueBattleCatalog.cs` - Carries parsed initialdata battle inputs from the catalog layer.
- `Application/Common/BlueProtocolBytes.cs` - Adds Blue battle protocol byte-width constants for stage and special flags.
- `Infrastructure/GameDataCatalog/Blue/BlueBattleDataLoader.cs` - Parses the required battle XML set and exposes catalog-derived initialdata values.
- `Application/Handlers/GetInitialDataQuery.Blue.cs` - Emits parsed battle values or concrete false/zero defaults.
- `.planning/phases/05-blue-battle-runtime-support/05-RESOLUTION.md` - Records the latest user approval and data-derived initialdata runtime rule.
- `.planning/phases/05-blue-battle-runtime-support/05-08-PLAN.md` - Records the superseding user-decision override.

## Decisions Made

- Treat all five known Blue battle XML files as the required initialdata advertisement set for this approved stage.
- Set `is_battleplay=false`, 8-byte zero stage flags, 16-byte zero special flags, and cap `0` when the battle XML set is unavailable.
- Set `is_battleplay=true` only when all required battle XML files are present and parse; parsed IDs and cap remain catalog-owned inputs.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Applied the latest user-approved data-derived initialdata rule**
- **Found during:** Task 1 (Keep initialdata battle advertisement row-gated)
- **Issue:** The original 05-08 plan and older `05-RESOLUTION.md` rows still required omitted/fail-closed battle initialdata, but the user explicitly rejected that behavior on 2026-05-31 and approved parsed "unlock all" battle initialdata for this stage.
- **Fix:** Recorded the superseding approval in `05-RESOLUTION.md` and `05-08-PLAN.md`, then implemented parsed stage/special/cap catalog values plus concrete false/zero defaults.
- **Files modified:** `.planning/phases/05-blue-battle-runtime-support/05-RESOLUTION.md`, `.planning/phases/05-blue-battle-runtime-support/05-08-PLAN.md`, `Application/Catalog/Blue/BlueBattleCatalog.cs`, `Infrastructure/GameDataCatalog/Blue/BlueBattleDataLoader.cs`, `Application/Handlers/GetInitialDataQuery.Blue.cs`, tests.
- **Verification:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialDataTests`, `dotnet test Tests/Tests.csproj --filter BlueBattle`, and temp-output `dotnet build Host/Host.csproj -o ...` passed.
- **Committed in:** `fe248d7d`

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** Required by the latest user decision. Scope stayed within Blue-owned initialdata/catalog behavior and did not touch generated protobuf wire files, `.tools`, Green, or Nijiiro behavior.

## Issues Encountered

- A parallel verification attempt hit the known Blazor build file lock on `TaikoWebUI/obj/Debug/net10.0/blazor.build.boot-extension.json`. The same `BlueBattle` command passed when rerun sequentially.

## Verification Results

- RED: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialDataTests` failed before implementation with missing `BlueProtocolBytes` battle constants and `BlueBattleCatalog` parsed-value properties.
- GREEN: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialDataTests` passed with 3 tests.
- GREEN: `dotnet test Tests/Tests.csproj --filter BlueBattle` passed with 26 tests after rerunning sequentially.
- Final: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialDataTests` passed with 3 tests.
- Final: `dotnet test Tests/Tests.csproj --filter BlueBattle` passed with 26 tests.
- Final: `dotnet build Host/Host.csproj -o $env:TEMP\TaikoLocalServer-host-build-05-08-...` passed with 0 warnings and 0 errors.
- Local XML evidence check parsed stage IDs `1..10,33` into `FE 07 00 00 02 00 00 00`, special move reward IDs `1..10` into `FE 07 00 00 00 00 00 00 00 00 00 00 00 00 00 00`, and `battle_bonds_lv_cap=65`.

## Known Stubs

None. The zero battle fields are intentional concrete defaults for unavailable or malformed required battle XML, not placeholder data.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

`05-11` can close Phase 5 with `BTL-03` now implemented from parsed Blue battle XML. Cabinet/RPCS3 smoke evidence remains required in Phase 6 for full Blue verification.

## Self-Check: PASSED

- Found `.planning/phases/05-blue-battle-runtime-support/05-08-SUMMARY.md`.
- Found task commit `82411ad5`.
- Found task commit `fe248d7d`.
- Confirmed both required focused test commands pass when run sequentially.
- Confirmed temp-output Host build passes.

---
*Phase: 05-blue-battle-runtime-support*
*Completed: 2026-05-30*
