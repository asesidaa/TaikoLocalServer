---
phase: 01-blue-a6-item-shop-and-unlocking
plan: "01-03"
subsystem: game-protocol
tags: [blue, item-shop, initialdata, protobuf, xunit]

requires:
  - phase: 01-blue-a6-item-shop-and-unlocking
    provides: Parser-proven Blue shop JSON and Blue-owned shop state helpers from 01-01 and 01-02
provides:
  - Era-aware item-shop info query dispatch for Green and Blue
  - Mediator-backed Blue getitemshopinfo.php active-season responses
  - Blue initialdatacheck.php item-shop advertisement gating
  - Blue item-shop wire mapper with optional purchase-field presence handling
affects: [blue-a6-item-shop, blue-initialdata, blue-purchase, green-item-shop-regression]

tech-stack:
  added: []
  patterns:
    - Shared query dispatch switches on explicit GameEra and delegates to era partial handlers
    - Blue protocol mappers translate Blue wire types into common application DTOs and commands

key-files:
  created:
    - Application/Handlers/GetItemShopInfoQuery.Blue.cs
    - Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs
    - Tests/Blue/BlueItemShopProtocolTests.cs
  modified:
    - Application/Handlers/GetItemShopInfoQuery.cs
    - Application/Handlers/GetItemShopInfoQuery.Green.cs
    - Application/Handlers/GetInitialDataQuery.Blue.cs
    - Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs
    - Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs
    - Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs
    - Tests/Green/GreenItemShopProtocolTests.cs

key-decisions:
  - "GetItemShopInfoQuery now requires an explicit GameEra and dispatches only Green and Blue item-shop info handlers."
  - "Blue initialdata shop metadata is advertised only when the Blue shop is enabled, the active season resolves, and that season has rows."
  - "Blue purchase preflight optional fields are mapped with protobuf ShouldSerialize* presence checks so omitted values remain null."

patterns-established:
  - "Blue item-shop protocol adapter files own Blue wire mappings instead of importing Green mapper or wire types."
  - "Initialdata advertisement and default-song clearing share one active-season-with-rows gate."

requirements-completed: [SHOP-01, SHOP-02, SHOP-03, SHOP-08]

duration: 10 min
completed: 2026-05-28
---

# Phase 01 Plan 01-03: Blue Shop Advertisement and Protocol Mappers Summary

**Blue item-shop advertisement now uses Blue-owned active-season data, with mediator-backed shop-info responses and optional-field-safe purchase mapping.**

## Performance

- **Duration:** 10 min
- **Started:** 2026-05-28T18:29:28Z
- **Completed:** 2026-05-28T18:39:15Z
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments

- Made `GetItemShopInfoQuery` era-aware and routed Green and Blue through separate partial handlers.
- Replaced Blue `getitemshopinfo.php` success-only behavior with a Mediator path that returns only the active Blue shop season and ordered item rows.
- Tightened Blue `initialdatacheck.php` shop advertisement so disabled, missing, or empty active seasons return no item-shop metadata.
- Added Blue item-shop protocol tests for shop-info responses, initialdata advertisement, default song flag clearing, and optional purchase fields.

## Task Commits

1. **Task 1: Make shop-info query era-aware and wire Blue getitemshopinfo** - `0dcfc0b7` (feat)
2. **Task 2: Lock initialdata advertisement and Blue optional-field mapping** - `cb5e8f35` (feat)

**Plan metadata:** recorded in the final docs commit for this plan

## Files Created/Modified

- `Application/Handlers/GetItemShopInfoQuery.cs` - Adds explicit era dispatch and unsupported-era rejection.
- `Application/Handlers/GetItemShopInfoQuery.Green.cs` - Moves Green behavior behind the Green partial handler.
- `Application/Handlers/GetItemShopInfoQuery.Blue.cs` - Returns active Blue shop season envelope and ordered rows.
- `Application/Handlers/GetInitialDataQuery.Blue.cs` - Gates item-shop advertisement and shop-song clearing on enabled active seasons with rows.
- `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs` - Maps Green shop-info requests to `GameEra.Green`.
- `Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs` - Sends Green shop-info requests through the mapper.
- `Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs` - Maps Blue shop-info and purchase wire types with Blue era and optional-field presence checks.
- `Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs` - Sends Blue shop-info requests through Mediator.
- `Tests/Green/GreenItemShopProtocolTests.cs` - Updates Green regression coverage for explicit era dispatch.
- `Tests/Blue/BlueItemShopProtocolTests.cs` - Adds Blue protocol coverage for SHOP-01, SHOP-02, and D-03 optional-field semantics.

## Decisions Made

- Followed the existing era-partial handler pattern instead of adding Blue behavior into the Green handler.
- Treated `ActiveSeason.Items.Count > 0` as part of the Blue initialdata advertisement gate, matching SHOP-01 and D-16.
- Preserved Green item-shop behavior by mapping Green requests to `GameEra.Green` before dispatch.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- Running the Blue and Green targeted tests in parallel caused a local compiler file lock (`CS2012` on `Application.dll`). Rerunning the same commands sequentially passed.

## Verification

- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopProtocolTests` - passed, 11 tests.
- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenItemShopProtocolTests` - passed, 3 tests.

## Known Stubs

None. Stub scan hits were limited to intentional protocol empty arrays in tests and `AryBlueLegaltermsDatas = []`.

## Threat Flags

None. The changed Blue catalog-to-protocol and Blue adapter-to-Mediator surfaces are covered by T-01-07, T-01-08, and T-01-09 in the plan threat model.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `01-04-PLAN.md` after Wave 2 companion plan `01-05-PLAN.md` completes. Purchase validation can now consume Blue mapper optional-field semantics and active-season shop-info behavior.

## Self-Check: PASSED

- Created files exist: Blue shop-info handler, Blue item-shop mapper, Blue protocol tests, and summary.
- Task commits exist: `0dcfc0b7`, `cb5e8f35`.
- No accidental tracked-file deletions were found in task commits.

---
*Phase: 01-blue-a6-item-shop-and-unlocking*
*Completed: 2026-05-28*
