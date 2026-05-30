---
phase: 05-blue-battle-runtime-support
verified: 2026-05-30T21:27:22Z
reverified: 2026-05-30T21:38:45Z
initial_status: gaps_found
status: gaps_resolved
score: 6/6 must-haves verified
overrides_applied: 0
gaps: []
resolved_gaps:
  - truth: "BTL-01/BTL-02: selected battle specials are fully persisted and readable through battleuserdata."
    status: resolved
    reason: "BlueBattleNpcState now stores NpcCostumeId plus SelectedSpecialId1/2/3, playresult persistence fills all three selected specials, and GetBattleUserDataQuery.Blue.cs reads complete BlueBattleNpcStates into NpcDatas."
    artifacts:
      - path: "Domain/Entities/BlueBattleNpcState.cs"
        issue: "Resolved: SelectedSpecialId was replaced with SelectedSpecialId1/SelectedSpecialId2/SelectedSpecialId3 and NpcCostumeId was added for required NPC row emission."
      - path: "Application/Common/BlueBattleStateExtensions.cs"
        issue: "Resolved: persists npc.SpecialId1, npc.SpecialId2, and npc.SpecialId3 into explicit Blue-owned fields."
      - path: "Application/Handlers/GetBattleUserDataQuery.Blue.cs"
        issue: "Resolved: reads complete persisted BlueBattleNpcStates into CommonBattleUserDataResponse.NpcDatas."
      - path: "Tests/Blue/BlueBattlePersistenceShapeTests.cs"
        issue: "Resolved: source guard now requires plural selected-special fields and rejects the singular SelectedSpecialId shape."
    tests:
      - "BlueBattlePlayResultHandlerTests.UpdatePlayResult_Blue_BattlePayloadPersistsBlueBattleRowsOnly"
      - "BlueBattlePlayResultHandlerTests.UpdatePlayResult_Blue_BattlePayloadEchoesNpcSelectedSpecialsThroughBattleUserData"
      - "BlueBattleUserDataTests.Handle_PersistedCompleteNpcRows_EmitsNpcDatasWithSelectedSpecials"
      - "BlueBattlePersistenceTests.AddBlueBattleNpcSelectedSpecialsMigration_PreservesExistingSelectedSpecialAsSlot1"
deferred:
  - truth: "Cabinet/RPCS3 battle smoke evidence exists for the full battle flow."
    addressed_in: "Phase 6"
    evidence: "ROADMAP Phase 6 success criterion 1 and REQUIREMENTS FULL-01 require repeatable normal and battle Blue smoke evidence."
---

# Phase 05: Blue Battle Runtime Support Verification Report

**Phase Goal:** Blue battle runtime support, including Blue-owned battle persistence, battleuserdata readback, data-derived initialdata battle advertisement, battle playresult persistence separation, source guards, and server-side verification artifacts.
**Verified:** 2026-05-30T21:27:22Z
**Re-verified:** 2026-05-30T21:38:45Z
**Status:** gaps_resolved
**Re-verification:** Yes - the initial verifier gap for BTL-01/BTL-02 was gap-closed with tests, production persistence/readback changes, and an EF migration.

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | BTL-01: Blue battle persistence stores battle user, NPC, unlock flag, selected special, assignment, token, boss-life, and last-stage state in Blue-owned tables. | VERIFIED | `BlueBattleNpcState` stores `SelectedSpecialId1`, `SelectedSpecialId2`, `SelectedSpecialId3`, and `NpcCostumeId`; `BlueBattleStateExtensions` persists all three client-reported `SpecialId` values; migration `20260530213652_AddBlueBattleNpcSelectedSpecials` preserves the old slot-1 value. |
| 2 | BTL-02: `battleuserdata.php` returns safe persisted battle state without unsafe zero defaults. | VERIFIED | `GetBattleUserDataQuery.Blue.cs` reads complete persisted `BlueBattleNpcStates` into `NpcDatas` and omits incomplete NPC rows; tests cover persisted NPC readback and playresult-to-battleuserdata echo. |
| 3 | BTL-03: `initialdatacheck.php` advertises battle from the approved data-derived Blue battle catalog decision. | VERIFIED | `BlueBattleDataLoader` requires the five battle XML files, parses stage IDs, token reward IDs, and NPC progression cap; `GetInitialDataQuery.Blue.cs` emits `is_battleplay`, stage flags, special flags, and cap only from `blue.BattleCatalog`, with false/zero output when unavailable. |
| 4 | BTL-04: Blue battle playresults are mapped and persisted without corrupting normal Blue playresult, self-best, crown, Dani, or shop state. | VERIFIED | `PlayResultMappers` classifies battle payloads from battle sections; `UpdatePlayResultCommand.Blue.cs` branches to `HandleBlueBattle` before normal unlock/stage/Dani/shop writes; handler tests prove normal Blue state remains unchanged. |
| 5 | BTL-05: Battle rewards/unlocks update only approved Blue battle state and do not derive blocked effects. | VERIFIED | `BlueBattleStateExtensions` stores client-reported release/stage/token/assignment observations in BlueBattle tables/bitsets and tests cover no normal unlock mirror, token reward effect, boss completion, or stage graph calculation. |
| 6 | BTL-06: Battle controllers, handlers, mappers, entities, migrations, and tests are Blue-owned and do not depend on Green AI Battle semantics. | VERIFIED | `BlueBattleSourceGuardTests` passed and scans production Phase 05 battle files for Green AI Battle, normal Blue mutation, unapproved constants, and data-derived initialdata approval evidence. |

**Score:** 6/6 truths verified

### Deferred Items

Items not met by Phase 05 but explicitly addressed in later milestone phases.

| # | Item | Addressed In | Evidence |
|---|------|-------------|----------|
| 1 | Cabinet/RPCS3 battle smoke evidence | Phase 6 | REQUIREMENTS line for FULL-01 is pending and ROADMAP Phase 6 success criterion 1 owns repeatable normal and battle smoke evidence. |

### Required Artifacts

| Artifact | Expected | Status | Details |
|---|---|---|---|
| `05-RESOLUTION.md` | Row-level runtime gate and latest 05-08 data-derived initialdata decision | VERIFIED | Rows 1, 3, 4, 5, 6, and 25 approve parsed-data initialdata; rows 18-24 and 26 remain bounded store/echo or blocked. |
| `BlueBattleCatalog` / `BlueBattleDataLoader` | Blue catalog boundary for battle XML | VERIFIED | Loader parses `battlestageinfo.xml`, `battletokeninfo.xml`, `battlenpcinfo.xml`, and gates advertisement on the complete five-file parsed set. |
| `GetInitialDataQuery.Blue.cs` / `InitialDataMappers.cs` | Data-derived initialdata response | VERIFIED | Handler converts catalog values into protocol-width bitsets; mapper only forwards DTO fields. No hardcoded IDs, bitsets, stage 33, or cap 65 in handlers/mappers. |
| `BlueBattleUserState`, `BlueBattleNpcState`, `BlueBattleTokenState`, `BlueBattleStageResult`, `BlueBattleReleaseState` | Blue-owned persistence | VERIFIED | NPC persistence now carries `NpcCostumeId` plus `SelectedSpecialId1/2/3`; unresolved fields remain nullable until client values are stored. |
| `BattleUserDataController`, `GetBattleUserDataQuery.Blue.cs`, `BattleUserDataMappers.cs` | Mediator-backed battleuserdata readback | VERIFIED | Safe scalar/token readback remains; complete persisted NPC rows now populate `NpcDatas`, while incomplete rows remain omitted. |
| `UpdatePlayResultCommand.BlueBattle.cs`, `BlueBattleStateExtensions.cs` | Battle playresult branch and BlueBattle persistence | VERIFIED | Normal-state bypass and BlueBattle writes are verified; battle NPC selected special 1/2/3 values are stored and echoed. |
| `BlueBattleSourceGuardTests`, `BlueBattleRequirementTests` | Source guards and traceability | VERIFIED | Tests enforce plural selected-special persistence/readback coverage in addition to the data-derived initialdata decision. |
| `05-BATTLE-RUNTIME-VERIFICATION.md`, `05-VALIDATION.md` | Server-side verification artifacts | VERIFIED | Both explicitly state cabinet/RPCS3 smoke is not Phase 05 and is Phase 6/FULL-01. |

### Key Link Verification

| From | To | Via | Status | Details |
|---|---|---|---|---|
| `BlueBattleDataLoader` | `IBlueCatalog.BattleCatalog` | `BlueEraGameDataCatalog.InitializeAsync` | WIRED | Blue catalog initializes `battleCatalog` from the loader and exposes it through `IBlueCatalog`. |
| `IBlueCatalog.BattleCatalog` | `initialdatacheck.php` response | `GetInitialDataQuery.Blue.cs` and `InitialDataMappers.Map` | WIRED | Data-derived stage/special/cap fields flow from catalog to DTO to wire response. |
| `BattleUserDataController` | `GetBattleUserDataQueryHandler` | `Mediator.Send(new GetBattleUserDataQuery(...))` | WIRED | Controller no longer constructs a local success response. |
| `GetBattleUserDataQueryHandler` | `BlueBattleNpcStates` | DB query for complete NPC readback | WIRED | Query reads complete NPC rows and maps persisted selected-special values to `NpcDatas`. |
| `PlayResultController` | `UpdatePlayResultCommand.BlueBattle` | `PlayResultMappers.Map` then `HandleBlueBattle` branch | WIRED | Battle-classified payloads branch before normal Blue side effects. |

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|---|---|---|---|---|
| `GetInitialDataQuery.Blue.cs` | `ReleaseBattleStageFlg` | `blue.BattleCatalog.ReleaseBattleStageIds` from `BlueBattleDataLoader.ReadStageIds` | Yes | VERIFIED |
| `GetInitialDataQuery.Blue.cs` | `ReleaseBattleSpecialFlg` | `blue.BattleCatalog.ReleaseBattleSpecialIds` from `BlueBattleDataLoader.ReadSpecialIds` | Yes | VERIFIED |
| `GetInitialDataQuery.Blue.cs` | `BattleBondsLvCap` | `blue.BattleCatalog.BattleBondsLvCap` from `battlenpcinfo.xml` required/typoed required-exp elements | Yes | VERIFIED |
| `GetBattleUserDataQuery.Blue.cs` | `AryTokenDatas` | `BlueBattleTokenStates` with non-null `TokenValue` | Yes | VERIFIED |
| `GetBattleUserDataQuery.Blue.cs` | `NpcDatas` / selected specials | Complete `BlueBattleNpcStates` rows with required fields present | Yes | VERIFIED |
| `UpdatePlayResultCommand.BlueBattle.cs` | BlueBattle stage/release/token/NPC rows | Client-reported `CommonPlayResultData` battle sections | Yes | VERIFIED |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|---|---|---|---|
| Focused selected-special gap close | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattlePersistenceShapeTests\|FullyQualifiedName~BlueBattlePlayResultHandlerTests\|FullyQualifiedName~BlueBattleUserDataTests\|FullyQualifiedName~BlueBattlePersistenceTests" --no-restore` | 17 passed | PASS |
| Focused Blue battle suite | `dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore` | 37 passed | PASS |
| EF migration listing | `dotnet ef migrations list --project Infrastructure --startup-project Host` | `20260530213652_AddBlueBattleNpcSelectedSpecials` listed | PASS |
| Blue initialdata data-derived behavior | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialDataTests --no-restore` | 3 passed | PASS |
| Blue battle source guards | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleSourceGuardTests --no-restore` | 4 passed | PASS |
| Blue battle requirement tests | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleRequirementTests --no-restore` | 4 passed | PASS |
| Full test suite | `dotnet test Tests/Tests.csproj --no-restore` | Not rerun during gap close | NOT_RUN |
| Host temp-output build | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase05-verify" --no-restore` | Not rerun during gap close | NOT_RUN |

### Probe Execution

| Probe | Command | Result | Status |
|---|---|---|---|
| None discovered | `Get-ChildItem scripts -Recurse -Filter probe-*.sh` | No probe scripts found; phase docs do not declare probe scripts | SKIPPED |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|---|---|---|---|---|
| BTL-01 | 05-04, 05-05 | Blue battle persistence stores user, NPC, unlock flags, selected specials, assignment, tokens, boss life, last-stage state | SATISFIED | `BlueBattleNpcState` stores `SelectedSpecialId1/2/3` and `BlueBattleStateExtensions` persists all three selected specials from battle playresult NPC data. |
| BTL-02 | 05-07 | `battleuserdata.php` returns safe default and persisted state | SATISFIED | `GetBattleUserDataQuery.Blue.cs` emits complete persisted NPC rows through `NpcDatas` and omits incomplete NPC rows rather than emitting unsafe defaults. |
| BTL-03 | 05-08 | `initialdatacheck.php` advertises battle according to approved design | SATISFIED | Stage flags, special flags, and cap derive from parsed Blue battle XML through the catalog boundary. |
| BTL-04 | 05-09, 05-10 | Battle playresult maps/persists without normal Blue corruption | SATISFIED | Battle branch returns before normal Blue writes; focused tests pass. |
| BTL-05 | 05-03, 05-10 | Rewards/unlocks update only approved state | SATISFIED | Store/echo-only behavior is bounded to BlueBattle state; blocked effects remain unimplemented. |
| BTL-06 | 05-11 | Blue-owned implementation and no Green AI Battle dependency | SATISFIED | Source guards passed; production files are Blue-owned. |
| FULL-01 | Phase 6 | Cabinet/RPCS3 smoke evidence | DEFERRED | Phase 05 docs do not claim smoke; Phase 6/FULL-01 owns it. |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|---|---|---|---|---|
| `Domain/Entities/BlueBattleNpcState.cs` | n/a | `SelectedSpecialId` singular | RESOLVED | Replaced with `SelectedSpecialId1`, `SelectedSpecialId2`, and `SelectedSpecialId3`. |
| `Application/Common/BlueBattleStateExtensions.cs` | n/a | `npcState.SelectedSpecialId = npc.SpecialId1` | RESOLVED | Now persists all three selected specials from client-reported battle NPC state. |
| `Application/Handlers/GetBattleUserDataQuery.Blue.cs` | n/a | No `BlueBattleNpcStates` read | RESOLVED | Now reads complete persisted NPC rows into `NpcDatas`. |

Debt-marker scan did not find `TODO`, `FIXME`, or `XXX` in the Phase 05 production/test files checked. Broader matches were unrelated existing `return null` helpers, not Phase 05 stubs.

### Human Verification Required

None for Phase 05. Cabinet/RPCS3 battle smoke is intentionally deferred to Phase 6/FULL-01 and is not counted as a Phase 05 failure.

### `.tools` Evidence Check

`git ls-tree -r --name-only HEAD .tools` returns only `.tools/parse_featureboard.py`; no `.tools/blue` evidence files are tracked. Phase 05 durable evidence is tracked under `.planning/phases/05-blue-battle-runtime-support/`. Legacy plan text still mentions `.tools/blue/...`, but the final summaries and tracked artifacts document that evidence was moved out of `.tools`.

### Gaps Summary

The blocking selected-special gap is resolved. The wire and DTO surfaces carry three selected special values, the playresult mapper preserves all three, persistence now stores all three in Blue-owned NPC state, and `battleuserdata.php` reads complete NPC rows back through `NpcDatas`. Cabinet/RPCS3 smoke remains deferred to Phase 6/FULL-01 and is not claimed by this report.

---

_Verified: 2026-05-30T21:27:22Z_
_Verifier: the agent (gsd-verifier)_
