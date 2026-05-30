---
phase: 05-blue-battle-runtime-support
verified: 2026-05-30T21:54:27Z
status: passed
score: 6/6 must-haves verified
overrides_applied: 0
re_verification:
  previous_verification_result: gaps_found
  previous_score: 4/6
  gaps_closed:
    - "BTL-01/BTL-02 selected battle specials now persist and read back through battleuserdata."
  gaps_remaining: []
  regressions: []
deferred:
  - truth: "Cabinet/RPCS3 battle smoke evidence exists for the full Blue battle flow."
    addressed_in: "Phase 6"
    evidence: "ROADMAP Phase 6 success criterion 1 and REQUIREMENTS FULL-01 require repeatable normal and battle Blue smoke evidence."
---

# Phase 05: Blue Battle Runtime Support Verification Report

**Phase Goal:** Blue battle mode has Blue-owned persistence and protocol behavior for battle userdata, initial data, battle playresult, progression, unlocks, rewards, and readback.
**Verified:** 2026-05-30T21:54:27Z
**Status:** passed
**Re-verification:** Yes - after BTL-01/BTL-02 selected-special gap closure at current HEAD.

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|---|---|---|
| 1 | BTL-01: Blue battle persistence stores battle user state, NPC state, unlock flags, selected specials, stage assignment, tokens, boss life, and last-stage state in Blue-owned tables. | VERIFIED | `Domain/Entities/BlueBattleNpcState.cs` has `NpcCostumeId` plus `SelectedSpecialId1/2/3`; `Application/Common/BlueBattleStateExtensions.cs` persists `npc.SpecialId1`, `npc.SpecialId2`, and `npc.SpecialId3`; migration `20260530213652_AddBlueBattleNpcSelectedSpecials` preserves the old singular value as slot 1 and adds slots 2/3. |
| 2 | BTL-02: Blue `battleuserdata.php` returns evidence-backed default and persisted battle state without unsafe zero-default fields. | VERIFIED | `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs` sends `GetBattleUserDataQuery`; `Application/Handlers/GetBattleUserDataQuery.Blue.cs` reads tokens and complete `BlueBattleNpcStates` rows; `Adapters.GameProtocol.Blue/Mappers/BattleUserDataMappers.cs` maps `LastSelectSpecial1/2/3` into `NpcDatas`; incomplete NPC rows stay omitted. |
| 3 | BTL-03: Blue `initialdatacheck.php` advertises battle availability and release flags according to the approved battle design. | VERIFIED | `Infrastructure/GameDataCatalog/Blue/BlueBattleDataLoader.cs` requires the five-file battle XML set, parses stage IDs, token reward IDs, and NPC cap; `Application/Handlers/GetInitialDataQuery.Blue.cs` derives stage/special bitsets and cap from `blue.BattleCatalog`, not hardcoded IDs, bitsets, or cap values. |
| 4 | BTL-04: Blue `playresult.php` safely maps and persists `BattleStageData` without corrupting normal Blue playresult, self-best, crown, Dani, or shop state. | VERIFIED | `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` classifies battle payloads from release or battle stage sections; `Application/Handlers/UpdatePlayResultCommand.Blue.cs` branches to `HandleBlueBattle` before normal Blue writes; handler tests prove normal Blue state remains unchanged. |
| 5 | BTL-05: Blue battle rewards and unlocks update Blue battle and normal save state only where the approved design says they should. | VERIFIED | `Application/Common/BlueBattleStateExtensions.cs` stores client-reported battle release/stage/token/NPC state in BlueBattle tables/bitsets only; `05-RESOLUTION.md` keeps token rewards, stage graph transitions, boss completion, stage 33 behavior, and normal unlock mirrors blocked. |
| 6 | BTL-06: Battle controllers, handlers, mappers, entities, migrations, and tests are Blue-owned and do not depend on Green AI Battle semantics. | VERIFIED | `Tests/Blue/BlueBattleSourceGuardTests.cs` covers production battle files and rejects Green AI Battle truth, normal Blue mutation in battle-state paths, unapproved XML/default inference, and hardcoded initialdata constants. |

**Score:** 6/6 truths verified

### Deferred Items

Items not met by Phase 05 but explicitly addressed in later milestone phases.

| # | Item | Addressed In | Evidence |
|---|---|---|---|
| 1 | Cabinet/RPCS3 battle smoke evidence | Phase 6 | `.planning/ROADMAP.md` Phase 6 success criterion 1 and `.planning/REQUIREMENTS.md` FULL-01 own repeatable normal and battle Blue smoke evidence. Phase 05 docs do not claim smoke. |

### Required Artifacts

| Artifact | Expected | Status | Details |
|---|---|---|---|
| `Domain/Entities/BlueBattleNpcState.cs` | Blue-owned NPC state with all selected specials | VERIFIED | Stores `NpcCostumeId`, `SelectedSpecialId1`, `SelectedSpecialId2`, `SelectedSpecialId3`, nullable unresolved fields, and BAID/NPC key ownership through EF. |
| `Infrastructure/Persistence/Migrations/20260530213652_AddBlueBattleNpcSelectedSpecials.cs` | Schema migration for selected-special gap | VERIFIED | Renames `SelectedSpecialId` to `SelectedSpecialId1` and adds `NpcCostumeId`, `SelectedSpecialId2`, and `SelectedSpecialId3`. `dotnet ef migrations list` includes this migration. |
| `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` | Wire-to-common battle NPC mapping | VERIFIED | Maps wire `NpcCostumeId`, `SpecialId1`, `SpecialId2`, and `SpecialId3` into `CommonPlayResultData.BattleNpcData`. |
| `Application/Common/BlueBattleStateExtensions.cs` | BlueBattle persistence helpers | VERIFIED | Upserts user, NPC, token, stage-result, and release rows without normal Blue state writes. Persists all three selected specials. |
| `Application/Handlers/GetBattleUserDataQuery.Blue.cs` | Persisted battleuserdata readback | VERIFIED | Reads persisted user/token/NPC state, filters NPC rows for complete required fields, and emits selected-special slots into common DTO rows. |
| `Adapters.GameProtocol.Blue/Mappers/BattleUserDataMappers.cs` | Common-to-wire battleuserdata mapper | VERIFIED | Emits `NpcDatas` and maps `LastSelectSpecial1/2/3`; optional scalar/byte fields are only set when present in the common DTO. |
| `Infrastructure/GameDataCatalog/Blue/BlueBattleDataLoader.cs` and `Application/Catalog/Blue/BlueBattleCatalog.cs` | Data-derived initialdata catalog boundary | VERIFIED | Five-file XML gate drives `EnablesBattleAdvertisement`; parsed stage IDs, token reward IDs, and cap flow to `BattleCatalog`. |
| `Application/Handlers/GetInitialDataQuery.Blue.cs` and `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs` | Initialdata battle response | VERIFIED | Uses `blue.BattleCatalog` for battle availability, stage bitset, special bitset, and cap; mapper only forwards nullable DTO fields. |
| `Tests/Blue/BlueBattle*` and `Tests/Blue/BlueInitialDataTests.cs` | Regression and source-guard coverage | VERIFIED | Focused selected-special, battleuserdata, initialdata, playresult, source-guard, and requirement tests pass at current HEAD. |
| `.planning/phases/05-blue-battle-runtime-support/05-BATTLE-RUNTIME-VERIFICATION.md` | Phase 05 server verification record | VERIFIED | Records automated server verification and explicitly states cabinet/RPCS3 battle smoke is Phase 6/FULL-01. |

### Key Link Verification

| From | To | Via | Status | Details |
|---|---|---|---|---|
| Blue playresult wire NPC selected specials | `BlueBattleNpcState.SelectedSpecialId1/2/3` | `PlayResultMappers.MapBattleNpcData` -> `CommonPlayResultData.BattleNpcData` -> `BlueBattleStateExtensions.UpsertBlueBattleNpcStateAsync` | WIRED | Wire `SpecialId1/2/3` are mapped, persisted, and tested by `UpdatePlayResult_Blue_BattlePayloadPersistsBlueBattleRowsOnly`. |
| `BlueBattleNpcStates` | `BattleUserDataResponse.NpcDatas` | `GetBattleUserDataQueryHandler` -> `BattleUserDataMappers.MapNpcData` | WIRED | Complete NPC rows are read from EF and emitted with `LastSelectSpecial1/2/3`; incomplete rows are omitted. |
| `BattleUserDataController` | application handler | `Mediator.Send(new GetBattleUserDataQuery(request.Baid))` | WIRED | Controller uses mediator and mapper, not local success construction. |
| Blue battle XML files | `initialdatacheck.php` battle fields | `BlueBattleDataLoader` -> `IBlueCatalog.BattleCatalog` -> `GetInitialDataQuery.Blue.cs` -> `InitialDataMappers` | WIRED | Data-derived stage/special/cap values flow through catalog boundary into wire response. |
| Blue battle playresult | normal-state protection | `UpdatePlayResultCommand.Blue.cs` battle branch before normal writes | WIRED | Battle payloads return through `HandleBlueBattle`; normal score/history/best/Dani/shop paths are bypassed. |

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|---|---|---|---|---|
| `BlueBattleStateExtensions.cs` | `SelectedSpecialId1/2/3` | Client-reported `CommonPlayResultData.BattleNpcData.SpecialId1/2/3` from wire mapper | Yes | VERIFIED |
| `GetBattleUserDataQuery.Blue.cs` | `NpcDatas.LastSelectSpecial1/2/3` | Complete persisted `BlueBattleNpcStates` rows | Yes | VERIFIED |
| `GetBattleUserDataQuery.Blue.cs` | `AryTokenDatas` | Persisted `BlueBattleTokenStates` rows with non-null `TokenValue` | Yes | VERIFIED |
| `GetInitialDataQuery.Blue.cs` | `ReleaseBattleStageFlg` | `blue.BattleCatalog.ReleaseBattleStageIds` from parsed `battlestageinfo.xml` | Yes | VERIFIED |
| `GetInitialDataQuery.Blue.cs` | `ReleaseBattleSpecialFlg` | `blue.BattleCatalog.ReleaseBattleSpecialIds` from parsed `battletokeninfo.xml` reward IDs | Yes | VERIFIED |
| `GetInitialDataQuery.Blue.cs` | `BattleBondsLvCap` | `blue.BattleCatalog.BattleBondsLvCap` from `battlenpcinfo.xml` required-exp count | Yes | VERIFIED |
| `UpdatePlayResultCommand.BlueBattle.cs` | BlueBattle stage/release/token rows | Client-reported battle sections from `CommonPlayResultData` | Yes | VERIFIED |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|---|---|---|---|
| Selected-special gap closure | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattlePersistenceShapeTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests|FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueBattlePersistenceTests" --no-restore` | 17 passed | PASS |
| Focused Blue battle suite | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | 37 passed | PASS |
| Initialdata, source guards, requirement, mapper coverage | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueInitialDataTests|FullyQualifiedName~BlueBattleSourceGuardTests|FullyQualifiedName~BlueBattleRequirementTests|FullyQualifiedName~BlueBattlePlayResultMapperTests" --no-restore` | 15 passed | PASS |
| Full server test suite | `dotnet test Tests/Tests.csproj --no-restore` | 611 passed | PASS |
| Host temp-output build | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase05-reverify" --no-restore` | Succeeded, 0 warnings, 0 errors | PASS |
| EF migration listing | `dotnet ef migrations list --project Infrastructure --startup-project Host --no-build` | `20260530213652_AddBlueBattleNpcSelectedSpecials` listed | PASS |
| Tracked `.tools` evidence check | `git ls-tree -r --name-only HEAD .tools` | No `.tools` paths are tracked; the legacy featureboard converter is preserved under `tools/parse_featureboard.py` | PASS |

### Probe Execution

| Probe | Command | Result | Status |
|---|---|---|---|
| None discovered | `Get-ChildItem scripts -Recurse -Filter probe-*.sh`; `rg "probe-.*\.sh" .planning/phases/05-blue-battle-runtime-support` | No runnable phase probe scripts declared or found | SKIPPED |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|---|---|---|---|---|
| BTL-01 | 05-04, 05-05, gap closure commit `58ba47f9` | Blue battle persistence stores battle user state, NPC state, unlock flags, selected specials, assignment, tokens, boss life, and last-stage state in Blue-owned tables. | SATISFIED | BlueBattle entities/DbSets/migrations exist; selected specials 1/2/3 are persisted; focused tests pass. |
| BTL-02 | 05-07, gap closure commit `58ba47f9` | Blue `battleuserdata.php` returns evidence-backed default and persisted battle state without unsafe zero-default fields. | SATISFIED | Mediator-backed handler reads complete persisted NPC rows into `NpcDatas`, including selected specials, and omits incomplete NPC rows. |
| BTL-03 | 05-08 | Blue `initialdatacheck.php` advertises battle availability and release flags according to approved design. | SATISFIED | Data-derived unlock-all decision is implemented through parsed Blue battle catalog data with explicit false/zero unavailable-data output. |
| BTL-04 | 05-09, 05-10 | Blue `playresult.php` maps/persists battle stage data without normal Blue corruption. | SATISFIED | Battle branch occurs before normal writes; handler tests assert normal Blue score/history/best/Dani/shop state remains unchanged. |
| BTL-05 | 05-03, 05-10 | Battle rewards/unlocks update only approved state. | SATISFIED | Store/echo-only BlueBattle persistence is implemented; unapproved reward/progression effects remain blocked in `05-RESOLUTION.md` and source guards. |
| BTL-06 | 05-11 | Battle code is Blue-owned and does not depend on Green AI Battle semantics. | SATISFIED | Source guards and requirement tests pass; production battle files use Blue-owned entities, handlers, mappers, and catalog loaders. |
| FULL-01 | Phase 6 | Repeatable normal and battle Blue cabinet/RPCS3 smoke evidence. | DEFERRED | Not claimed by Phase 05; Phase 6 owns it. |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|---|---|---|---|---|
| n/a | n/a | No unresolved `TODO`, `FIXME`, `XXX`, `PLACEHOLDER`, placeholder text, stub returns, or console-log-only implementations found in checked Phase 05 production/test files. | INFO | No blocking anti-patterns found. |
| `Infrastructure/Persistence/Migrations/20260530185853_AddBlueBattleState.cs` | 23 | Historical `SelectedSpecialId` column | INFO | Historical migration state only; superseded by `20260530213652_AddBlueBattleNpcSelectedSpecials`, which renames the column to `SelectedSpecialId1` and adds slots 2/3. |

### Human Verification Required

None for Phase 05. Cabinet/RPCS3 battle smoke is intentionally not claimed here and remains Phase 6/FULL-01.

### `.tools` Evidence Check

`git ls-tree -r --name-only HEAD .tools` returns no tracked paths. The legacy featureboard converter is preserved under `tools/parse_featureboard.py`; local `.tools` files remain ignored. Durable Phase 05 evidence is under `.planning/phases/05-blue-battle-runtime-support/`.

### Gaps Summary

No active gaps remain for Phase 05. The selected-special gap is closed at current HEAD: `58ba47f9` persists all three Blue battle NPC selected specials and `3c9c9133` records the closure. BTL-01 through BTL-06 are satisfied by code, wiring, data flow, and passing tests. Cabinet/RPCS3 smoke is not claimed and is deferred to Phase 6/FULL-01.

---

_Verified: 2026-05-30T21:54:27Z_
_Verifier: the agent (gsd-verifier)_
