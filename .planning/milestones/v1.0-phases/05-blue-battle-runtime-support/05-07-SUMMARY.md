---
phase: 05-blue-battle-runtime-support
plan: "05-07"
subsystem: blue-battle-protocol
tags: [blue, battle, battleuserdata, protobuf, mediator, tests, gsd]

requires:
  - phase: 05-blue-battle-runtime-support
    provides: "05-02 battleuserdata row approvals and 05-05 BlueBattle persistence tables"
provides:
  - "Mediator-backed Blue battleuserdata.php response path"
  - "Nullable common battleuserdata DTO matching Blue wire schema names"
  - "Presence-aware BattleUserDataResponse mapper with fail-closed optional fields"
  - "Focused battleuserdata and route guard tests"
affects: [05-08-initialdata-battle-gating, 05-10-battle-playresult-persistence, 05-11-blue-battle-closeout]

tech-stack:
  added: []
  patterns: [mediator-backed-blue-protocol-endpoint, nullable-battleuserdata-dto, protobuf-presence-mapping]

key-files:
  created:
    - Application/Dtos/CommonBattleUserDataResponse.cs
    - Application/Handlers/GetBattleUserDataQuery.Blue.cs
    - Adapters.GameProtocol.Blue/Mappers/BattleUserDataMappers.cs
    - Tests/Blue/BlueBattleUserDataTests.cs
    - .planning/phases/05-blue-battle-runtime-support/05-07-SUMMARY.md
  modified:
    - Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs
    - Tests/Blue/BlueRouteSkeletonTests.cs

key-decisions:
  - "Blue battleuserdata.php now uses a Blue-owned Mediator query and mapper instead of local controller success construction."
  - "Optional BattleUserDataResponse fields are set only when the common DTO value is non-null, preserving generated protobuf presence semantics."
  - "The query emits persisted scalar values and complete token rows, while leaving NPC rows empty until a complete row is explicitly available."

patterns-established:
  - "Battle protocol readback uses nullable common DTO fields to keep unresolved rows absent on the wire."
  - "Route skeleton tests allow Mediator only for the newly implemented BattleUserDataController endpoint."

requirements-completed: [BTL-02, BTL-06]

duration: 11 min
completed: 2026-05-30
---

# Phase 05 Plan 05-07: Battleuserdata Protocol Behavior Summary

**Mediator-backed Blue battleuserdata readback with nullable DTO fields and protobuf presence-aware mapping**

## Performance

- **Duration:** 11 min
- **Started:** 2026-05-30T19:10:36Z
- **Completed:** 2026-05-30T19:21:12Z
- **Tasks:** 1
- **Files modified:** 7

## Accomplishments

- Added `CommonBattleUserDataResponse` with nullable scalar/byte fields and explicit repeated-row DTOs matching the Blue wire schema.
- Added `GetBattleUserDataQueryHandler`, which validates the BAID, reads BlueBattle state, returns `Result = 1`, and emits only persisted approved values.
- Replaced the `battleuserdata.php` controller-local success stub with a Mediator query and `BattleUserDataMappers.Map`.
- Added focused TDD coverage for new-user omission, persisted scalar/token readback, explicit DTO row mapping, and controller source guard behavior.

## Task Commits

1. **Task 1 RED: Add failing battleuserdata tests** - `43c58d33` (test)
2. **Task 1 GREEN: Implement Blue battleuserdata readback** - `eb809dc8` (feat)

## Files Created/Modified

- `Application/Dtos/CommonBattleUserDataResponse.cs` - Common battleuserdata response DTO with nullable optional fields and explicit repeated row DTOs.
- `Application/Handlers/GetBattleUserDataQuery.Blue.cs` - Blue-owned query over `UserData`, `BlueBattleUserStates`, and token rows.
- `Adapters.GameProtocol.Blue/Mappers/BattleUserDataMappers.cs` - Presence-aware mapper to generated Blue `BattleUserDataResponse`.
- `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs` - Mediator-backed Blue battleuserdata endpoint.
- `Tests/Blue/BlueBattleUserDataTests.cs` - Focused RED/GREEN tests for fail-closed behavior and mapping.
- `Tests/Blue/BlueRouteSkeletonTests.cs` - Allows Mediator usage for the implemented BattleUserData endpoint only.

## Decisions Made

- Keep unresolved optional battleuserdata fields un-emitted by preserving nulls through the common DTO and generated `ShouldSerialize*` wire helpers.
- Emit token rows only when a persisted `TokenValue` exists.
- Keep NPC rows empty from the query for now because the existing persisted NPC shape does not contain every required field in the generated response row.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- A post-commit parallel rerun of the two focused `dotnet test` filters hit a transient `Domain/obj` file lock during simultaneous builds. Rerunning `BlueRouteSkeletonTests` by itself passed. No code changes were needed.

## Verification Results

- RED: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleUserDataTests` failed with missing `GetBattleUserDataQueryHandler`, `GetBattleUserDataQuery`, `BattleUserDataMappers`, and `CommonBattleUserDataResponse`.
- RED route filter: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRouteSkeletonTests` failed at the same compile gate caused by the new failing tests.
- GREEN: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleUserDataTests` passed with 4 tests.
- GREEN: `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRouteSkeletonTests` passed with 7 tests.
- Acceptance source check confirmed `BattleUserDataController` calls `Mediator.Send(new GetBattleUserDataQuery(...))` and `BattleUserDataMappers.Map(...)` and no longer constructs `new BattleUserDataResponse { Result = 1 }` locally.

## Known Stubs

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

`05-08` can gate initialdata battle fields with battleuserdata readback implemented. `05-10` can later populate BlueBattle state from battle playresults; this plan intentionally does not compute battle progression, rewards, NPC defaults, or battle availability.

## Self-Check: PASSED

- Found `Application/Dtos/CommonBattleUserDataResponse.cs`.
- Found `Application/Handlers/GetBattleUserDataQuery.Blue.cs`.
- Found `Adapters.GameProtocol.Blue/Mappers/BattleUserDataMappers.cs`.
- Found `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs`.
- Found `Tests/Blue/BlueBattleUserDataTests.cs`.
- Found `Tests/Blue/BlueRouteSkeletonTests.cs`.
- Found task commit `43c58d33`.
- Found task commit `eb809dc8`.
- Confirmed both required focused test commands pass.

---
*Phase: 05-blue-battle-runtime-support*
*Completed: 2026-05-30*
