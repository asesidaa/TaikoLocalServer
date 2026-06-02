---
phase: 05
slug: blue-battle-runtime-support
status: server-verified
created: 2026-05-31
updated: 2026-06-02
cabinet_rpcs3_smoke: not-performed
---

# Phase 05 - Blue Battle Runtime Verification

This record closes the server-side Phase 05 battle runtime work with requirement traceability, row-resolution status, automated command results, and the explicit handoff for real cabinet/RPCS3 smoke.

## Requirement Traceability

| Requirement | Phase 05 coverage | Evidence |
|-------------|-------------------|----------|
| BTL-01 | Blue-owned battle persistence stores user, NPC, selected special 1/2/3, token, stage, and release state without Green or normal Blue battle storage. | `Tests/Blue/BlueBattlePersistenceTests.cs`, `Tests/Blue/BlueBattlePersistenceShapeTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs` |
| BTL-02 | `battleuserdata.php` is Mediator-backed, emits an IDA-safe starter only when no persisted battle state exists, and reads back client-reported battle user/NPC/selected-special/token/assignment state after battle playresult. NPC XML ids are normalized to zero-based runtime ids. | `Tests/Blue/BlueBattleUserDataTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `Tests/Blue/BlueBattleCatalogLoaderTests.cs`, `.planning/phases/05-blue-battle-runtime-support/05-UAT.md` |
| BTL-03 | `initialdatacheck.php` advertises battle from parsed Blue battle catalog data and emits explicit false/zero defaults when unavailable. | `Tests/Blue/BlueInitialDataTests.cs`, `Tests/Blue/BlueBattleSourceGuardTests.cs` |
| BTL-04 | Blue battle playresults map battle sections and bypass normal Blue score, crown, history, favorite/recent, shop, and Dani state. | `Tests/Blue/BlueBattlePlayResultMapperTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs` |
| BTL-05 | Battle rewards/progression stay store/echo only unless a row has exact proof or named approval; unresolved stage 33 and effect semantics remain blocked. | `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `.planning/phases/05-blue-battle-runtime-support/05-RESOLUTION.md` rows 18-24 and 26 |
| BTL-06 | Controllers, handlers, mappers, entities, migrations, catalog loaders, and tests are Blue-owned and guarded against Green AI Battle truth. | `Tests/Blue/BlueBattleSourceGuardTests.cs`, `Tests/Blue/BlueBattleRequirementTests.cs` |

## Row-Resolution Summary

The 2026-05-31 data-derived decision is recorded in `05-RESOLUTION.md` rows 1, 3, 4, 5, 6, and 25. The 2026-06-02 stage flag role correction refines row 4: parse the required five-file Blue battle XML set, advertise battle only when that set exists and parses, emit parsed `battlestageinfo.xml` stage IDs into the 8-byte global stage availability bitset, unlock all parsed battle special move (`必殺技`) reward IDs into the 16-byte initial special bitset, derive `battle_bonds_lv_cap` from parsed NPC progression data, and emit explicit false/zero defaults when the set is unavailable.

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
| 4c | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattlePlayResultHandlerTests\|FullyQualifiedName~BlueBattlePersistenceShapeTests\|FullyQualifiedName~BlueBattleSourceGuardTests\|FullyQualifiedName~BlueBattlePersistenceTests\|FullyQualifiedName~BlueBattleUserDataTests" --no-restore` | PASS | 23 passed | 2026-05-31 DPN/special-name fix |
| 4d | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | PASS | 39 passed | 2026-05-31 DPN/special-name fix |
| 4e | `dotnet ef migrations list --project Infrastructure --startup-project Host` | PASS | `20260531090513_RenameBlueBattleNpcMaxDpn` listed | 2026-05-31 DPN/special-name fix |
| 4f | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueBattleCatalogLoaderTests" --no-restore` | PASS | 10 passed | 2026-05-31 first-time battle crash fix |
| 4g | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | PASS | 40 passed | 2026-05-31 first-time battle crash fix |
| 4h | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueBattleCatalogLoaderTests" --no-restore` | PASS | 11 passed | 2026-05-31 starter NPC row fix |
| 4i | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | PASS | 41 passed | 2026-05-31 starter NPC row fix |
| 4j | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueBattleCatalogLoaderTests" --no-restore` | PASS | 11 passed | 2026-05-31 release stage/special starter-state fix |
| 4k | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | PASS | 41 passed | 2026-05-31 release stage/special starter-state fix |
| 4l | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattleUserDataTests" --no-restore` | PASS | 7 passed | 2026-05-31 explicit first-stage assignment fix |
| 4m | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | PASS | 41 passed | 2026-05-31 explicit first-stage assignment fix |
| 4n | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattleUserDataTests\|FullyQualifiedName~BlueInitialDataTests" --no-restore` | PASS | 10 passed | 2026-05-31 battle special row-gate fix |
| 4o | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | PASS | 41 passed | 2026-05-31 battle special row-gate fix |
| 4p | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueBattleCatalogLoaderTests" --no-restore` | PASS | 11 passed | 2026-06-01 starter NPC costume flag fix |
| 4q | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattleUserDataTests" --no-restore` | PASS | 7 passed | 2026-06-01 starter NPC costume flag fix |
| 4r | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | PASS | 41 passed | 2026-06-01 starter NPC costume flag fix |
| 4s | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | PASS | 39 passed | 2026-06-01 persisted readback/NPC id 0 correction |
| 4t | `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~BlueInitialDataTests\|FullyQualifiedName~BlueBattleUserDataTests\|FullyQualifiedName~BlueBattleCatalogLoaderTests\|FullyQualifiedName~BlueBattleRequirementTests" --no-restore` | PASS | 16 passed | 2026-06-02 stage flag role correction |
| 4u | `dotnet test Tests\Tests.csproj --filter BlueBattle --no-restore` | PASS | 39 passed | 2026-06-02 stage flag role correction |
| 5 | `dotnet test Tests/Tests.csproj --no-restore` | PASS | 613 passed | 2026-05-31 |
| 5a | `dotnet test Tests/Tests.csproj --no-restore` | PASS | 614 passed | 2026-05-31 first-time battle crash fix |
| 5b | `dotnet test Tests/Tests.csproj --no-restore` | PASS | 615 passed | 2026-05-31 starter NPC row fix |
| 5c | `dotnet test Tests/Tests.csproj --no-restore` | PASS | 615 passed | 2026-05-31 release stage/special starter-state fix |
| 5d | `dotnet test Tests/Tests.csproj --no-restore` | PASS | 615 passed | 2026-05-31 explicit first-stage assignment fix |
| 5e | `dotnet test Tests/Tests.csproj --no-restore` | PASS | 615 passed | 2026-05-31 battle special row-gate fix |
| 5f | `dotnet test Tests/Tests.csproj --no-restore` | PASS | 615 passed | 2026-06-01 starter NPC costume flag fix |
| 5g | `dotnet test Tests/Tests.csproj --no-restore` | PASS | 613 passed | 2026-06-01 persisted readback/NPC id 0 correction |
| 5h | `dotnet test Tests\Tests.csproj --no-restore` | PASS | 613 passed | 2026-06-02 stage flag role correction |
| 6 | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-dpn-special-fix" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-05-31 |
| 6a | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-crash-fix" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-05-31 first-time battle crash fix |
| 6b | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-starter-npc-fix" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-05-31 starter NPC row fix |
| 6c | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-release-state-fix" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-05-31 release stage/special starter-state fix |
| 6d | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-assign-stage-fix" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-05-31 explicit first-stage assignment fix |
| 6e | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-special-gate-fix" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-05-31 battle special row-gate fix |
| 6f | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-costume-flag-fix" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-06-01 starter NPC costume flag fix |
| 6g | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-persisted-readback" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-06-01 persisted readback/NPC id 0 correction |
| 6h | `dotnet build Host\Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-stage-default" --no-restore` | PASS | build succeeded, 0 warnings, 0 errors | 2026-06-02 stage flag role correction |
| 7 | `git diff --check` | PASS | whitespace check passed; CRLF conversion warnings only | 2026-05-31 battle special row-gate fix |
| 7a | `git diff --check` | PASS | whitespace check passed; CRLF conversion warnings only | 2026-06-01 starter NPC costume flag fix |
| 7b | `git diff --check` | PASS | whitespace check passed; CRLF conversion warnings only | 2026-06-01 persisted readback/NPC id 0 correction |
| 7c | `git diff --check` | PASS | whitespace check passed; CRLF conversion warnings only | 2026-06-02 stage flag role correction |

Note: an initial parallel focused test attempt hit a build-output lock on `Domain/obj/Debug/net10.0/TaikoLocalServer.Domain.dll`; the affected source-guard filter was rerun sequentially and passed.

Rows 4a and 4b are the selected-special gap-close verification added after the verifier reported BTL-01/BTL-02 gaps. Rows 4c through 4e record the DPN naming/maximum and `release_battle_special_flg` wording fix before RPCS3 smoke. Rows 4f through 6a record the first-time battle crash server fix: parsed NPC ids are exposed from the battle catalog, and first-time `battleuserdata.php` returns a catalog-derived `last_npc_id` when battle XML advertises battle. Rows 4h through 6b record the follow-up starter NPC row fix after RPCS3 still crashed with `last_npc_id` alone. Rows 4j through 6c record the release stage/special starter-state fix after RPCS3 still crashed with the starter NPC row but no release-special/default-session state. Rows 4l through 7 record the explicit first-stage assignment fix after the updated RPCS3 log and IDA review showed the crash follows a client teardown path and `assign_stage_id` is consumed during selected-stage setup. Rows 4n and 4o record the battle special row-gate fix after a deeper `sub_7497C` trace showed nested NPC rows are discarded unless raw bit 120 is set in both initialdata `release_battle_special_flg` and battleuserdata `npc_data.release_special_flg`. Rows 4p through 6f record the starter NPC costume flag fix after the next client-only pass showed `BattleUserNpcData.npc_costume_flg` is a selected-costume availability mask consumed by downstream battle support/model setup.

The 2026-06-01 screenshot-driven reanalysis supersedes the catalog-derived starter assumptions above for first-use response shape, but the played session proves persistence/readback is required after battle. Fresh daemon/subagent evidence shows the byte-array lengths were not the direct crash gate; the client consumes fixed 16/8/4/16-byte slices. The current fix keeps the IDA-safe widths, uses NPC id 0 for the starter, normalizes `battlenpcinfo.xml` id 1 to runtime id 0, and reads back persisted client-reported battle state after playresult while keeping selected specials backed by available special bits plus row-gate bit 120. The 2026-06-02 stage flag role correction establishes that initialdata stage flags are global stage availability and battleuserdata stage flags are separate user/progression state with the same bit numbering.

## UAT Crash Fix Record

`05-UAT.md` recorded a blocker where RPCS3 crashed after a first-time battle user request. The latest user-provided working response screenshot changes the evidence baseline: it proves default NPC/stage state can enter the flow, while the note proves at least one selected special must be unlocked for special-attack use. The later played session proves the first runtime NPC id is 0 and that persisted progress must be echoed after playresult: total exp 175, max DPN 34, selected specials 1/1/1, release info bit 1, last/assign stage 1. Fresh IDA confirms `sub_7497C` copies fixed slices and validates selected specials against the effective special mask. RPCS3 smoke has not been rerun after the persisted-readback correction; the UAT remains pending until that retest confirms the flow and special attack work.

## Cabinet/RPCS3 Smoke Handoff

Cabinet/RPCS3 battle smoke was not performed in Phase 05. Do not treat the automated server gates above as FULL-01. Phase 6 owns repeatable cabinet/RPCS3 battle smoke evidence with date, enabled eras, data paths, observed endpoint calls, and pass/fail notes.
