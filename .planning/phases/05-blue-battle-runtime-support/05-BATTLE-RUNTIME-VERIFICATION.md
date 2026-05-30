---
phase: 05
slug: blue-battle-runtime-support
status: server-verified
created: 2026-05-31
updated: 2026-05-31
cabinet_rpcs3_smoke: not-performed
---

# Phase 05 - Blue Battle Runtime Verification

This record closes the server-side Phase 05 battle runtime work with requirement traceability, row-resolution status, automated command results, and the explicit handoff for real cabinet/RPCS3 smoke.

## Requirement Traceability

| Requirement | Phase 05 coverage | Evidence |
|-------------|-------------------|----------|
| BTL-01 | Blue-owned battle persistence stores user, NPC, selected special 1/2/3, token, stage, and release state without Green or normal Blue battle storage. | `Tests/Blue/BlueBattlePersistenceTests.cs`, `Tests/Blue/BlueBattlePersistenceShapeTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs` |
| BTL-02 | `battleuserdata.php` is Mediator-backed and emits only persisted or row-approved fields, including complete persisted NPC rows through `NpcDatas`. | `Tests/Blue/BlueBattleUserDataTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs` |
| BTL-03 | `initialdatacheck.php` advertises battle from parsed Blue battle catalog data and emits explicit false/zero defaults when unavailable. | `Tests/Blue/BlueInitialDataTests.cs`, `Tests/Blue/BlueBattleSourceGuardTests.cs` |
| BTL-04 | Blue battle playresults map battle sections and bypass normal Blue score, crown, history, favorite/recent, shop, and Dani state. | `Tests/Blue/BlueBattlePlayResultMapperTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs` |
| BTL-05 | Battle rewards/progression stay store/echo only unless a row has exact proof or named approval; unresolved stage 33 and effect semantics remain blocked. | `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `.planning/phases/05-blue-battle-runtime-support/05-RESOLUTION.md` rows 18-24 and 26 |
| BTL-06 | Controllers, handlers, mappers, entities, migrations, catalog loaders, and tests are Blue-owned and guarded against Green AI Battle truth. | `Tests/Blue/BlueBattleSourceGuardTests.cs`, `Tests/Blue/BlueBattleRequirementTests.cs` |

## Row-Resolution Summary

The 2026-05-31 data-derived unlock-all decision is recorded in `05-RESOLUTION.md` rows 1, 3, 4, 5, 6, and 25. The approved behavior is exact: parse the required five-file Blue battle XML set, advertise battle only when that set exists and parses, unlock all parsed battle stage IDs into the 8-byte initial stage bitset, unlock all parsed battle token reward IDs into the 16-byte initial special bitset, derive `battle_bonds_lv_cap` from parsed NPC progression data, and emit explicit false/zero defaults when the set is unavailable.

This decision does not approve hardcoded current IDs, hardcoded byte arrays, hardcoded cap constants, token reward calculation, stage graph calculation, boss completion, stage `33` progression, or normal Blue unlock mirrors. Rows 18, 21, 22, 24, and 26 are approved only for client-state store/echo behavior; row 23 remains deferred except catalog/raw observation.

## Automated Gates

| Order | Command | Result | Count | Date |
|-------|---------|--------|-------|------|
| 1 | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleSourceGuardTests` | PASS | 4 passed | 2026-05-31 |
| 2 | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRouteSkeletonTests` | PASS | 8 passed | 2026-05-31 |
| 3 | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleRequirementTests` | PASS | 4 passed | 2026-05-31 |
| 4 | `dotnet test Tests/Tests.csproj --filter BlueBattle` | PASS | 37 passed | 2026-05-31 |
| 4a | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattlePersistenceShapeTests\|FullyQualifiedName~BlueBattlePlayResultHandlerTests\|FullyQualifiedName~BlueBattleUserDataTests\|FullyQualifiedName~BlueBattlePersistenceTests" --no-restore` | PASS | 17 passed | 2026-05-31 gap close |
| 4b | `dotnet ef migrations list --project Infrastructure --startup-project Host` | PASS | `20260530213652_AddBlueBattleNpcSelectedSpecials` listed | 2026-05-31 gap close |
| 5 | `dotnet test Tests/Tests.csproj` | PASS | 608 passed | 2026-05-31 |
| 6 | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase05"` | PASS | build succeeded, 0 warnings, 0 errors | 2026-05-31 |

Note: an initial parallel focused test attempt hit a build-output lock on `Domain/obj/Debug/net10.0/TaikoLocalServer.Domain.dll`; the affected source-guard filter was rerun sequentially and passed.

Rows 4a and 4b are the selected-special gap-close verification added after the verifier reported BTL-01/BTL-02 gaps. Rows 5 and 6 remain the original Phase 05 closeout gates and were not rerun during this gap-close pass.

## Cabinet/RPCS3 Smoke Handoff

Cabinet/RPCS3 battle smoke was not performed in Phase 05. Do not treat the automated server gates above as FULL-01. Phase 6 owns repeatable cabinet/RPCS3 battle smoke evidence with date, enabled eras, data paths, observed endpoint calls, and pass/fail notes.
