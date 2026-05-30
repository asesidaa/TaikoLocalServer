# Phase 05: Blue Battle Runtime Support - Research

**Researched:** 2026-05-30
**Domain:** Blue game protocol runtime, EF Core SQLite persistence, protobuf-net wire mapping, IDA-backed client evidence
**Confidence:** MEDIUM - route/proto/current-code evidence is high confidence, but most battleuserdata defaults and progression semantics remain unresolved.

<user_constraints>
## User Constraints (from CONTEXT.md) [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

### Locked Decisions

### Gate resolution path
- **D-01:** Phase 5 must resolve every one of the 26 Phase 4 `MISSING_EVIDENCE` rows before runtime behavior relies on that row.
- **D-02:** Each row is resolved only by concrete proof or by a named user approval. No blanket approval and no broad assumption set is allowed.
- **D-03:** If any row remains unknown, Phase 5 planning blocks until that row is addressed. Do not work around unresolved rows with stub placeholders or partial runtime assumptions.
- **D-04:** Record row-by-row resolutions in a Phase 5 resolution matrix that cites the Phase 4 gate. Leave the completed Phase 4 gate artifact as history.
- **D-05:** Evidence standard: proto/generated wire can prove message shape; IDA/client/log/cabinet proof is authoritative for runtime mechanics; local XML remains candidate data unless proven consumed; the Wiki page is accepted for gameplay semantics and likely proto-field intent.

### Battleuserdata and initialdata defaults
- **D-06:** Wiki content may resolve gameplay semantics and likely field intent, but it does not by itself prove emitted byte widths, required response shape, or client-safe default serialization.
- **D-07:** `initialdatacheck.php` must not advertise battle availability with `is_battleplay` until the Phase 5 resolution matrix clears all required defaults, rows, and runtime behavior.
- **D-08:** Battle byte-array widths/defaults, including release battle stage/special/info flags and NPC special/costume flags, require client/IDA/log proof or explicit row approval with exact width/default. Do not compute widths from XML max IDs alone.
- **D-09:** New-user `battleuserdata.php` state should be minimal/empty where safe. If a field is required, fill it with resolved values. If a field is likely recorded after playresult, let playresult populate it instead of inventing startup defaults.

### Battle playresult handling
- **D-10:** Treat `StageData.ary_battlestagedata` or top-level `ary_release_battledata` presence as the primary trigger for a Blue battle playresult branch.
- **D-11:** Phase 5 should also inspect battle `play_mode`, `stage_mode`, and other mode flags because they likely differ from normal Blue. Prefer IDA tracing; otherwise allow observed values inside the battle branch and tighten after request logs.
- **D-12:** Once a request is classified as battle, bypass the normal Blue stage persistence path. Battle stages must not call normal score, crown, play history, recent/favorite, profile counter, self-best, or Dani updates.
- **D-13:** Persist client-reported `BattleStageData` values into Blue battle-owned tables once their rows are resolved, without deriving normal score/crown/history effects.
- **D-14:** Do not apply normal-stage support filters to battle-classified payloads. Record observed mode values and tighten behavior after IDA/log proof.

### Battle XML, rewards, and unlock semantics
- **D-15:** Add runtime loaders/catalogs for battle XML only after each file/field runtime role is proven or explicitly approved in the Phase 5 resolution matrix.
- **D-16:** Do not infer first-stage, next-stage, last-stage, boss-life, or stage `33` behavior from XML alone. Resolve each graph rule separately by proof or approval.
- **D-17:** Do not assign semantics to `battletokeninfo.xml` token IDs, token values, or reward `type` values `0` and `1` until each meaning is proven or explicitly approved.
- **D-18:** Existing normal playresult release arrays may continue to flow to normal Blue unlock/save-state handling by default. Treat `ReleaseBattleData` releases as battle-owned unless a specific release path is proven or approved as a normal unlock mirror.

### the agent's Discretion
None. The user made explicit decisions for all selected gray areas.

### Deferred Ideas (OUT OF SCOPE)
None - discussion stayed within phase scope.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| BTL-01 | Blue battle persistence stores battle user state, NPC state, unlock flags, selected specials, stage assignment, tokens, boss life, and last-stage state in Blue-owned tables. | Persistence is allowed only as Blue-owned schema/raw battle state until defaults and row counts are resolved; current Blue DbContext has normal Blue sets but no battle sets. [VERIFIED: .planning/REQUIREMENTS.md; VERIFIED: Infrastructure/Persistence/TaikoDbContext.Blue.cs; VERIFIED: Application/Abstractions/ITaikoDbContext.Blue.cs] |
| BTL-02 | Blue `battleuserdata.php` returns evidence-backed default and persisted battle state without unsafe zero-default fields. | The route and request checker are IDA-proven, but response default widths/row counts remain mostly missing, so full battleuserdata runtime planning stays blocked. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs] |
| BTL-03 | Blue `initialdatacheck.php` advertises battle availability and release flags according to the approved battle design. | IDA proves client consumption of `is_battleplay` and 8-byte/16-byte initial release flag copy behavior, but D-07 still blocks advertising battle until all required rows are clear. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| BTL-04 | Blue `playresult.php` safely maps and persists `BattleStageData` without corrupting normal Blue playresult, self-best, crown, Dani, or shop state. | User decision D-10 defines the battle branch trigger and D-12 requires bypassing `SaveBlueStageAsync`, recent/favorite, profile counters, self-best, and Dani. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md; VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs] |
| BTL-05 | Blue battle rewards and unlocks update Blue battle and normal save state only where the approved design says they should. | `ReleaseBattleData` remains battle-owned unless a specific mirror is proven or approved; token/reward semantics are still missing. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md; VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md] |
| BTL-06 | Battle controllers, handlers, mappers, entities, migrations, and tests are Blue-owned and do not depend on Green AI Battle semantics. | Existing architecture requires Blue-owned partial handlers, mappers, controllers, entities, migrations, tests, and source guards; Green AI Battle is contrast material only. [VERIFIED: AGENTS.md; VERIFIED: .planning/research/ARCHITECTURE.md; VERIFIED: .planning/research/PITFALLS.md] |
</phase_requirements>

## Summary

Phase 5 is not fully unblocked for runtime implementation. IDA evidence from `.tools/blue/EBOOT.ELF.i64` proves the client consumes some Blue battle initialdata fields, registers the `battleuserdata.php`, `initialdatacheck.php`, and `playresult.php` routes, and loads all five local battle XML files through battle-specific code paths. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]

The strict gate still holds. IDA did not resolve most `BattleUserDataResponse` default widths, repeated row counts, NPC/token/boss-life semantics, stage `33` progression role, reward type meanings, or release mirror behavior. [VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md; VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]

**Primary recommendation:** plan Phase 5 as gated implementation: 05-01 may add Blue-owned persistence/catalog foundations and raw capture surfaces, 05-04 may add guards/tests, but 05-02 full battleuserdata/advertisement and 05-03 progression/rewards/unlocks must include row-resolution checkpoints before emitting unproven values. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md; VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| `initialdatacheck.php` battle advertisement | Game Protocol Adapter | Application | The Blue adapter owns protobuf wire presence, while application DTOs decide whether resolved battle values exist. [VERIFIED: Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs; VERIFIED: Application/Dtos/CommonInitialDataCheckResponse.Blue.cs] |
| `battleuserdata.php` response | Game Protocol Adapter | Application, Database | The Blue controller currently returns a stub response, and Phase 5 must replace it with Blue query/mapper/persistence only after evidence rows are resolved. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| Battle playresult classification | Game Protocol Adapter | Application | Generated Blue wire types expose `AryBattlestagedata` and `AryReleaseBattledata`; user decision D-10 makes their presence the branch trigger. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| Battle state persistence | Database / Storage | Application | Blue battle state belongs in Blue-owned EF entities/DbSets/migrations and must not reuse Green AI Battle or normal Blue score tables. [VERIFIED: AGENTS.md; VERIFIED: .planning/research/ARCHITECTURE.md] |
| Battle XML catalog loading | Database / Storage | Application | Battle XML is local operator data; IDA proves file consumption, but field semantics still need row-level proof or approval before runtime behavior depends on them. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Host/wwwroot/data/blue/data/config/S10100-1/battle] |
| Regression/source guards | Tests | Application, Adapter | Phase 5 must prove battle payloads bypass normal Blue state writes and Green AI Battle implementation truth. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md; VERIFIED: .planning/research/PITFALLS.md] |

## Project Constraints (from AGENTS.md)

- Battle mode must be specified from proto, logs, IDA/client evidence, or cabinet/RPCS3 traces before runtime implementation. [VERIFIED: AGENTS.md]
- Blue must stay a first-class era with Blue-owned partial handlers, DTO fields, mappers, persistence, catalog data, tests, and routes. [VERIFIED: AGENTS.md]
- Blue, Green, and Nijiiro persistent state must remain separate unless the data is truly shared identity state. [VERIFIED: AGENTS.md]
- Preserve Blue direct-protobuf and startup/verup assumptions unless newer client evidence contradicts them. [VERIFIED: AGENTS.md]
- Finish normal Track A before battle runtime; Phase 5 is after Phase 4 and after normal Track A completion. [VERIFIED: AGENTS.md; VERIFIED: .planning/ROADMAP.md]
- Done requires repeatable cabinet/RPCS3 smoke evidence for normal and battle flows, not only passing server tests. [VERIFIED: AGENTS.md]
- Blue runtime data under `Host/wwwroot/data/blue/data` is local/operator-supplied and may be gitignored. [VERIFIED: AGENTS.md]
- If `Host/bin/Debug/net10.0` is locked by a running server, verify Host builds with a temp output path. [VERIFIED: AGENTS.md]
- C# project conventions use `net10.0`, C# 13, file-scoped namespaces, centralized package versions, Blue `.Blue.cs` partials, generated wire files under `Adapters.GameProtocol.Blue/Wire/`, and xUnit tests. [VERIFIED: AGENTS.md; VERIFIED: Directory.Build.props; VERIFIED: Directory.Packages.props; VERIFIED: Tests/Tests.csproj]

## Phase 5 Missing-Evidence Resolution Matrix

| # | Phase 4 Missing-Evidence Row | Phase 5 Status | Evidence Found In This Research | Plan Implication |
|---|------------------------------|----------------|---------------------------------|------------------|
| 1 | Battle menu entry sequence and required `initialdatacheck.php` fields | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | IDA proves `is_battleplay` is consumed by a battle availability helper, but the full menu entry sequence still has additional state checks. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Keep `is_battleplay` false/omitted until all required rows and a smoke/log sequence are approved. |
| 2 | `battleuserdata.php` call timing and requirement | STILL_MISSING_IDA_EVIDENCE | IDA proves route setup and request checking but not a clear caller sequence from menu entry. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Do not plan startup/menu timing assumptions without another IDA pass or cabinet/RPCS3 log. |
| 3 | `InitialdatacheckResponse.is_battleplay` safe emitted behavior | PROVEN | `OnInitialDataCheckResponse` reads an optional bool and defaults absent to false; helper `sub_250F04` consumes the stored battleplay flag with additional state checks. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Field handling can be implemented, but D-07 still blocks setting true until other rows clear. |
| 4 | `InitialdatacheckResponse.release_battle_stage_flg` width and default bits | STILL_MISSING_IDA_EVIDENCE | IDA proves an 8-byte copy/zero-fill path for the initial battle stage release flag, but default bit values are not proven. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Tests may assert exact 8-byte serialization only after a default source or approval exists. |
| 5 | `InitialdatacheckResponse.release_battle_special_flg` width and relation to NPC special flags | STILL_MISSING_IDA_EVIDENCE | IDA proves a 16-byte copy/zero-fill path for the initial battle special release flag, but the relation to NPC special flags is unresolved. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Do not derive special release defaults from XML max IDs or NPC row shape. |
| 6 | `InitialdatacheckResponse.battle_bonds_lv_cap` value | NEEDS_USER_APPROVAL | IDA proves the client stores the supplied cap, and the Phase 5 context records the Wiki as accepted for gameplay concepts such as bonds cap 65, but this does not prove wire default value. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] | Either keep omitted or ask for explicit approval to emit `65` as the cap. |
| 7 | `BattleUserDataResponse.release_info_flg` width and default | STILL_MISSING_IDA_EVIDENCE | This research did not find a response-field consumption path for battleuserdata. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Block emitted value until response parser/use site is found or approved. |
| 8 | `BattleUserDataResponse.release_battle_stage_flg` width/default and relation to initialdata | STILL_MISSING_IDA_EVIDENCE | Initialdata stage flag width is partially proven, but battleuserdata response field width/default is not proven. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Do not assume the battleuserdata field equals the initialdata field without proof. |
| 9 | `BattleUserDataResponse.last_battle_stage_id` default | STILL_MISSING_IDA_EVIDENCE | Proto/wire shape exists, but client default semantics were not resolved. [VERIFIED: proto/blue/taiko.proto; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Store persisted values only after playresult or approved initial default source. |
| 10 | `BattleUserDataResponse.last_boss_life` default | STILL_MISSING_IDA_EVIDENCE | Proto/wire shape exists, but client default semantics were not resolved. [VERIFIED: proto/blue/taiko.proto; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Do not invent boss-life defaults from XML alone. |
| 11 | `BattleUserDataResponse.last_npc_id` default and source | STILL_MISSING_IDA_EVIDENCE | IDA found selected NPC write helpers, but not battleuserdata default/readback semantics. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Persist selected NPC only when client reports it or an approved default exists. |
| 12 | `BattleUserDataResponse.npc_data` row count | STILL_MISSING_IDA_EVIDENCE | Phase 4 XML counts are local shape only; this research did not prove required response row count. [VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md] | Block required-row generation; raw persisted rows can be schema-ready but not emitted as complete defaults. |
| 13 | `BattleUserNpcData.npc_costume_flg` width/default | STILL_MISSING_IDA_EVIDENCE | Proto/wire field exists; exact width/default is not proven. [VERIFIED: proto/blue/taiko.proto; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Do not size from max costume IDs. |
| 14 | `BattleUserNpcData.release_special_flg` width/default | STILL_MISSING_IDA_EVIDENCE | Initialdata special flag width is partially proven, but nested NPC special flag width/default is not proven. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Treat as separate field family needing proof. |
| 15 | `BattleUserDataResponse.ary_token_data` row count | STILL_MISSING_IDA_EVIDENCE | Phase 4 found local token rows, but this research did not prove required response row count. [VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md] | Block token default rows until proof/approval. |
| 16 | `BattleUserDataResponse.assign_stage_id` first-stage default | STILL_MISSING_IDA_EVIDENCE | XML stage graph and IDA XML consumption are proven, but starting assignment semantics are not. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md] | Do not assign first stage from XML graph assumptions. |
| 17 | `PlayResultRequest.StageData.BattleStageData` identification and normal protection | PROVEN | User decision D-10 defines battle branch trigger by `AryBattlestagedata` or `AryReleaseBattledata` presence, and D-12 requires bypassing normal Blue persistence. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] | 05-03 can plan branch classification and normal-state protection tests. |
| 18 | `BattleStageData.npc_data` result fields persistence semantics | DEFER_RUNTIME_USE | Proto/wire shape exists; client/log proof for effect semantics was not found in this research. [VERIFIED: proto/blue/taiko.proto; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Allow raw battle-owned capture only after row modeling; defer derived progression effects. |
| 19 | `ReleaseBattleData.release_info_id` mirror semantics | DEFER_RUNTIME_USE | D-18 treats `ReleaseBattleData` releases as battle-owned unless a specific normal mirror is proven or approved. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] | Do not mirror to normal Blue unlock handling by default. |
| 20 | `ReleaseBattleData` stage/NPC/costume/special release arrays | DEFER_RUNTIME_USE | D-18 keeps these battle-owned unless a specific release path is proven or approved. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] | Persist battle-owned release records; block normal unlock side effects. |
| 21 | `ReleaseBattleData.ary_battletokendata` semantics | STILL_MISSING_IDA_EVIDENCE | Proto/wire shape exists; token semantics and value handling remain unresolved. [VERIFIED: proto/blue/taiko.proto; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Do not grant or spend battle tokens from this field until semantics are proven. |
| 22 | `ReleaseBattleData.assign_next_stage_id` progression update | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | Proto/wire shape exists; no IDA/log proof of transition behavior was found. [VERIFIED: proto/blue/taiko.proto; VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Record raw value if present; do not update assignment without proof/approval. |
| 23 | `battlestageinfo.xml` stage id `33` role | STILL_MISSING_IDA_EVIDENCE | IDA has a stage `33` packed asset string and local XML has stage `33`, but progression/menu/last-stage behavior is not proven. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md] | Do not route progression through stage `33` without proof/approval. |
| 24 | `battletokeninfo.xml` reward `type` values `0` and `1` | STILL_MISSING_IDA_EVIDENCE | Local token XML has reward types `0` and `1`, but semantic meaning is not proven. [VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md] | Do not implement reward type effects without proof/approval. |
| 25 | Battle XML file menu-entry requirements | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | IDA proves all five XML files are loaded/consumed by battle-specific code paths, but not which files are hard requirements for menu entry. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Plan loaders as consumed local inputs, but do not claim required/optional menu gating without smoke/log proof. |
| 26 | Boss-life and last-stage completion behavior | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | Proto/wire and XML shapes exist, but completion transition behavior remains unproven. [VERIFIED: proto/blue/taiko.proto; VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md] | Defer boss-life/last-stage state mutation until proof/approval. |

## IDA Evidence Anchors And Symbol Recommendations

**IDA database:** `.tools/blue/EBOOT.ELF.i64`. [VERIFIED: filesystem]
**Mutation status:** No IDB mutations were made. No symbols or comments were renamed in the database, and the database was not saved. Recommended names below are research-output recommendations only. [VERIFIED: ida-cli session behavior]

| Current Symbol | Address | Basis | Decompiled Behavior Summary | Confidence | Recommended Symbol / Comment |
|----------------|---------|-------|-----------------------------|------------|------------------------------|
| `sub_143720` | `0x143720` | String `void game::net::OnInitialDataCheckResponse(const InitialdatacheckResponse&)` and decompile. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Handles initialdata response result, reads optional battle-related booleans from the response presence mask, defaults absent bools to false, and calls `sub_13BFC8` with two byte arrays plus a cap value. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH for function role; MEDIUM for exact proto-field-to-offset mapping. | Rename to `Blue_OnInitialDataCheckResponse`; comment: "Consumes optional battle initialdata fields; absent bools default false; applies release flags/cap via sub_13BFC8." |
| `sub_13BFC8` | `0x13BFC8` | Called by `sub_143720`; xrefs to config flags. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Copies up to 8 bytes from first byte-array argument into a battle stage release flag global, up to 16 bytes from second byte-array argument into a battle special release flag global, zero-fills shorter arrays, stores a cap unless debug flags override. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH for copy widths; MEDIUM for proto field labels. | Rename to `BlueBattle_ApplyInitialReleaseFlagsAndCap`; comment: "Initialdata battle release stage=8 bytes, battle special=16 bytes; defaults still not proved." |
| `sub_250F04` | `0x250F04` | Calls `sub_5FBEEC`; additional checks on battle state globals. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Returns true only when stored battleplay flag is true and additional battle state checks pass. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueBattle_IsMenuAvailableCandidate`; comment: "Uses is_battleplay but also requires additional battle state." |
| `sub_5FBEEC` | `0x5FBEEC` | Calls `sub_187D64`; xref from `sub_250F04`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Reads the stored battleplay flag accessor chain. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueInitialData_GetIsBattleplay`. |
| `sub_187D64` | `0x187D64` | Reads `*(sub_140FA4()+4)`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Accesses the fourth byte in the initialdata boolean flag block. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueInitialData_ReadBattleplayFlag`. |
| `sub_140FA4` | `0x140FA4` | Returns address of the initialdata flag byte block. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Provides pointer to stored initialdata boolean flags. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueInitialData_GetFlagBlock`. |
| `sub_2DF364` | `0x2DF364` | Route strings including `chassis/battleuserdata.php`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Builds second route group with `userdata.php`, `selfbest.php`, `itempurchase.php`, `battleuserdata.php`, `rewardcardcheck.php`, and `rewardexecution.php`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH | Rename to `BlueNet_SetupUserStateRoutes`; comment: "`battleuserdata.php` is in user-state route group after itempurchase." |
| `sub_2E0CA8` | `0x2E0CA8` | String `CheckProtocolRequest...BattleUserDataRequest`; xrefs to battleuserdata globals. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Checks/sends direct-protobuf `BattleUserDataRequest` through the URL buffer for `battleuserdata.php`; requires route state `n2_12 == 3`; marks state 4 on success and 3 on failure. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH for route/request role; LOW for response-field semantics. | Rename to `BlueNet_CheckBattleUserDataRequest`; comment: "Request pipeline only; does not prove response defaults." |
| `sub_2DF8B0` | `0x2DF8B0` | Xrefs to battleuserdata route state globals. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Initializes route request buffers; battleuserdata buffer size is `4096` and uses route state offset `+5900`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueNet_InitializeUserStateRequestBuffers`. |
| `sub_2E05B0` | `0x2E05B0` | Xrefs to route state globals. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Resets route state globals and URL buffers for the user-state route group, including battleuserdata. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueNet_ResetUserStateRoutes`. |
| `sub_2DB31C` | `0x2DB31C` | Route string `chassis/playresult.php`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Builds route paths including `playresult.php` in a direct-protobuf route setup function. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH | Rename to `BlueNet_SetupPlayResultRoute`. |
| `sub_2DE860` | `0x2DE860` | String `CheckProtocolRequest...PlayResultRequest`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Checks/sends direct-protobuf `PlayResultRequest`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH | Rename to `BlueNet_CheckPlayResultRequest`. |
| `sub_2E3A60` | `0x2E3A60` | Route string `chassis/initialdatacheck.php`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Builds initialdata and adjacent startup/catalog routes; `battleuserdata.php` is not in this group. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH | Rename to `BlueNet_SetupInitialDataRoutes`. |
| `sub_12F44` | `0x12F44` | Xrefs from four battle XML path strings. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Loads/imports `battleadjsetting.xml`, `battlenpcinfo.xml`, `battlestageinfo.xml`, and `battletokeninfo.xml` through battle importer calls. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH | Rename to `BlueBattle_LoadCoreBattleXml`; comment: "Consumes four battle XML files; field semantics still unresolved." |
| `sub_796C24` | `0x796C24` | Xref from `battlesupportinfo.xml` path and GameBattleSetting code. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Loads/imports `battlesupportinfo.xml`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH | Rename to `BlueBattle_LoadSupportInfoXml`. |
| `sub_83B29C` | `0x83B29C` | Xrefs from config flag strings. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Registers debug/config flags `force_battlestage_allrelease`, `force_battlespecial_allrelease`, and `ignore_battlenpc_lvcap` behind version checks. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueConfig_RegisterBattleDebugFlags`. |
| `sub_83B72C` | `0x83B72C` | Xrefs from config flag strings. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Dumps/prints battle debug flag values. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueConfig_DumpBattleDebugFlags`. |
| `sub_38410` | `0x38410` | String `Battle Enso Decide Npc / playerId:`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Writes selected NPC ID into per-player battle state. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH | Rename to `BlueBattle_SetSelectedNpc`. |
| `sub_385AC` | `0x385AC` | String `Battle Enso Decide Support Lv / playerId:`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Writes selected support level into per-player battle state. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | HIGH | Rename to `BlueBattle_SetSelectedSupportLevel`. |
| `sub_796534` | `0x796534` | Returns string `GameBattleSetting`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Identifies a battle setting state/component. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueBattle_GameBattleSettingName`. |
| `sub_79921C` | `0x79921C` | Returns string `GameBattleSongSelect`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Identifies a battle song-select state/component. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueBattle_GameBattleSongSelectName`. |
| `sub_786C18` | `0x786C18` | Returns string `GameBattleEnsoResult`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Identifies a battle result state/component. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | MEDIUM | Rename to `BlueBattle_GameBattleEnsoResultName`. |
| stage `33` asset string | string xref only | `/data/lumendata/packed/battle/enso_stage/33/packeddata.ddp`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Proves the client has a packed battle stage 33 asset reference, not progression semantics. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | LOW for semantics | Comment only if a function xref is later identified: "Stage 33 asset exists; menu/progression role not proved." |

## Phase 5 Plan Implications

| Planned Plan | What Can Be Planned Now | What Must Stay Gated |
|--------------|-------------------------|----------------------|
| 05-01 Blue battle persistence and catalog/data foundations | Add Blue-owned EF entities/DbSets/migrations for battle user root, NPC rows, token rows, stage assignment, release records, and raw observed battle playresult payload shape; add catalog loaders only as local consumed-input readers with no inferred defaults. [VERIFIED: AGENTS.md; VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Do not seed default `battleuserdata.php` rows, first stage, boss life, token rewards, or stage `33` behavior from XML alone. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| 05-02 Battle userdata and initial-data protocol behavior | Implement optional-field plumbing and tests for omission/presence; initialdata stage release flag width 8 and special release flag width 16 may be used only after default source approval. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs] | Full `battleuserdata.php` persisted response and `is_battleplay=true` advertisement remain blocked until rows 1, 2, and 4-16 are resolved. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| 05-03 Battle playresult, progression, rewards, and unlocks | Add battle branch classification by `AryBattlestagedata` or `AryReleaseBattledata` presence; capture battle payload into Blue-owned state; prove normal Blue persistence bypass. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md; VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs] | Derived progression, reward type handling, token grants, next-stage assignment, stage `33`, boss-life completion, and normal unlock mirrors remain blocked. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix] |
| 05-04 Battle regression tests, source guards, and verification | Add tests/source guards before or alongside runtime plans: no Green AI Battle dependencies, no normal Blue state writes for battle payloads, optional battle fields omitted until set, exact-width tests only for resolved widths, and temp-output Host build gate. [VERIFIED: .planning/research/PITFALLS.md; VERIFIED: AGENTS.md] | Cabinet/RPCS3 battle smoke is required for final done and cannot be replaced by server tests. [VERIFIED: AGENTS.md; VERIFIED: .planning/REQUIREMENTS.md] |

## Standard Stack

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK | `global.json` pins `10.0.100` with `rollForward=latestFeature`; installed SDK is `10.0.201`. | Build, test, and run Host/tests. | Existing project runtime and build baseline. [VERIFIED: global.json; VERIFIED: dotnet --version] |
| C# / Target Framework | C# 13, `net10.0`. | Application, adapters, infrastructure, tests. | Central project configuration. [VERIFIED: Directory.Build.props] |
| ASP.NET Core | 10.0.7 packages. | Host process and protocol controllers. | Existing Host/controller stack. [VERIFIED: Directory.Packages.props; VERIFIED: Host/Program.cs] |
| EF Core SQLite | 10.0.7. | Blue-owned battle persistence and migrations. | Existing persistence stack and local SQLite storage. [VERIFIED: Directory.Packages.props; VERIFIED: Infrastructure/Persistence/TaikoDbContext.cs] |
| protobuf-net / protobuf-net.AspNetCore | 3.2.56 / 3.2.52. | Blue direct-protobuf route serialization. | Existing game protocol serialization stack. [VERIFIED: Directory.Packages.props; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] |
| Mediator.SourceGenerator / Mediator.Abstractions | 3.0.2. | Application command/query dispatch. | Existing handler pattern. [VERIFIED: Directory.Packages.props; VERIFIED: Application/Handlers] |
| IDA Pro via `ida-cli` | Local importable bridge at `H:/IDACLI/src/ida_cli`. | Client evidence and symbol/address research. | User-required Phase 5 evidence tool. [VERIFIED: ida-cli skill; VERIFIED: python import ida_cli] |

### Supporting

| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| xUnit | 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1. | Unit/integration regression tests. | Use for Blue protocol mapper, handler, persistence, source guard, and normal-state protection tests. [VERIFIED: Directory.Packages.props; VERIFIED: Tests/Tests.csproj] |
| Riok.Mapperly | 4.3.1. | Source-generated adapter mappings where local pattern already uses it. | Use only if new adapter mapping follows existing Mapperly pattern; custom battle semantics should stay explicit. [VERIFIED: Directory.Packages.props; VERIFIED: AGENTS.md] |
| ripgrep | 15.1.0 installed. | Source guard tests/manual code search. | Use for research and source guard development. [VERIFIED: rg --version] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Existing EF Core SQLite | Custom file persistence | Rejected; project persistence is EF Core SQLite and Blue battle state must integrate with existing DbContext/migrations. [VERIFIED: AGENTS.md; VERIFIED: Infrastructure/Persistence/TaikoDbContext.Blue.cs] |
| Existing protobuf-net generated wire types | Hand-rolled protobuf parsing | Rejected; generated Blue wire types already expose optional presence helpers and field ownership. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] |
| IDA/log/cabinet evidence | XML row-count inference | Rejected by D-08, D-15, D-16, and D-17. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| Blue-owned implementation | Green AI Battle reuse | Rejected; Green AI Battle is contrast/source-guard material only. [VERIFIED: .planning/research/PITFALLS.md; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |

**Installation:** No new external packages are recommended for Phase 5 research or planning. [VERIFIED: Standard Stack above]

**Version verification:** Existing versions were verified from `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `Tests/Tests.csproj`, and local CLI probes; no new NuGet/npm/PyPI/crates package install is planned. [VERIFIED: codebase grep; VERIFIED: dotnet --version; VERIFIED: python import ida_cli]

## Package Legitimacy Audit

No external package installation is recommended for this phase, so the Package Legitimacy Gate is not applicable. [VERIFIED: Standard Stack]

| Package | Registry | Age | Downloads | Source Repo | slopcheck | Disposition |
|---------|----------|-----|-----------|-------------|-----------|-------------|
| N/A | N/A | N/A | N/A | N/A | N/A | No packages to audit. [VERIFIED: Standard Stack] |

**Packages removed due to slopcheck [SLOP] verdict:** none. [VERIFIED: Standard Stack]
**Packages flagged as suspicious [SUS]:** none. [VERIFIED: Standard Stack]

## Architecture Patterns

### System Architecture Diagram

```text
Blue cabinet/RPCS3
  |
  | direct protobuf HTTP
  v
Adapters.GameProtocol.Blue controllers
  |
  +--> initialdatacheck.php
  |      -> Blue mapper emits optional battle fields only when resolved
  |      -> unresolved fields omitted by generated presence helpers
  |
  +--> battleuserdata.php
  |      -> gated Blue query/mapper
  |      -> persisted rows emitted only after width/default/row-count proof
  |
  +--> playresult.php
         -> mapper detects AryBattlestagedata or AryReleaseBattledata
         -> battle branch bypasses normal Blue score/crown/history/Dani writes
         -> Blue battle handler persists raw/resolved battle-owned state
         -> unresolved progression/reward/unlock effects stay gated
```

This data flow follows the current adapter/Application/Infrastructure split and Phase 5 D-10 through D-18 decisions. [VERIFIED: AGENTS.md; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

### Recommended Project Structure

```text
Application/
  Dtos/
    CommonPlayResultData.BlueBattle.cs       # Blue battle DTO extensions only after branch shape is defined
  Handlers/
    GetBattleUserDataQuery.Blue.cs           # Blue battleuserdata query
    UpdatePlayResultCommand.BlueBattle.cs    # Blue battle branch, separate from normal stage persistence
Domain/
  Entities/
    *Battle*Blue.cs                          # Blue-owned battle persistence entities
Infrastructure/
  GameDataCatalog/Blue/
    *Battle*Loader.cs                        # XML loaders for proven/approved file roles
  Persistence/
    TaikoDbContext.BlueBattle.cs             # Blue battle DbSets and EF mapping
Adapters.GameProtocol.Blue/
  Mappers/
    BattleUserDataMappers.cs                 # Wire response mapping with ShouldSerialize tests
    BattlePlayResultMappers.cs               # BattleStageData/ReleaseBattleData mapping
Tests/
  Blue/
    BlueBattle*Tests.cs                      # Mapper/handler/source-guard/regression coverage
```

The structure extends existing Blue-owned partial and adapter-local wire patterns. [VERIFIED: .planning/research/ARCHITECTURE.md; VERIFIED: AGENTS.md]

### Pattern 1: Optional Protobuf Presence Is The Safety Boundary

**What:** Use generated nullable backing fields and `ShouldSerialize*` helpers so unresolved optional fields remain omitted. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs]

**When to use:** Use for `InitialdatacheckResponse` and `BattleUserDataResponse` optional battle fields until exact defaults are proven or approved. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**Example:**

```csharp
// Source: Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs [VERIFIED: codebase grep]
if (common.ReleaseBattleStageFlg is not null)
{
    response.ReleaseBattleStageFlg = common.ReleaseBattleStageFlg;
}
```

### Pattern 2: Battle Branch Before Normal Blue Persistence

**What:** Classify battle playresults before iterating normal `StageData`, then route battle payloads to a Blue battle handler that does not call normal score/crown/history/Dani code. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**When to use:** Use in `PlayResultMappers` and `UpdatePlayResultCommand.Blue` before `SaveBlueStageAsync`, `UpsertBlueFavoriteAndRecentAsync`, `BlueProfileCounters.ApplyStage`, `UpsertBestAsync`, or `SaveBlueDanAsync` can run. [VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs]

**Example:**

```csharp
// Source: Phase 5 D-10/D-12 plus existing UpdatePlayResultCommand.Blue.cs normal write paths [VERIFIED: 05-CONTEXT.md; VERIFIED: codebase grep]
var isBattle = request.AryReleaseBattledata is not null
            || request.AryStagedata.Any(stage => stage.AryBattlestagedata is not null);

if (isBattle)
{
    // Route to Blue battle-owned persistence only.
    // Do not call SaveBlueStageAsync, normal favorite/recent, self-best, crown, counters, or Dani helpers.
}
```

### Pattern 3: Blue-Owned Persistence With Raw-Then-Resolved Semantics

**What:** Persist observed battle request/response values in Blue battle tables first, then add derived progression fields only as evidence rows resolve. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**When to use:** Use for `BattleStageData`, `ReleaseBattleData`, token rows, NPC rows, boss-life, and stage assignment where wire shape exists but behavior semantics remain missing. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]

### Anti-Patterns to Avoid

- **Zero-filled optional battle defaults:** Unsafe because D-08 requires exact width/default proof or approval before emitting byte arrays. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]
- **XML max-ID sizing:** Unsafe because local XML row counts do not prove wire widths/defaults. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]
- **Green AI Battle reuse:** Invalid because Green AI Battle does not define Blue battle protocol truth. [VERIFIED: .planning/research/PITFALLS.md]
- **Normal Blue write fallthrough:** Invalid because battle-classified payloads must not update normal Blue play history, recent/favorites, profile counters, self-best, crowns, or Dani. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Protobuf optional presence | Custom bool flags or manual protobuf serialization | Generated `Adapters.GameProtocol.Blue/Wire/Game.cs` `ShouldSerialize*` helpers | Existing generated wire types already encode omission semantics. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] |
| SQLite schema/migrations | Ad hoc SQL files or runtime table creation | EF Core DbSets, mappings, and migrations | Existing persistence stack uses EF Core SQLite. [VERIFIED: Infrastructure/Persistence/TaikoDbContext.Blue.cs; VERIFIED: Directory.Packages.props] |
| Battle field defaults | XML row-count inference or zero constants | IDA/client/log/cabinet proof or named user approval | D-08, D-15, D-16, and D-17 prohibit assumptions. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| Battle result side effects | Reuse Green AI Battle logic or normal Blue stage path | Blue battle-specific handler branch | Blue battle is separate from Green AI Battle and normal Blue score/crown/history state. [VERIFIED: .planning/research/PITFALLS.md; VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs] |
| Verification | Manual spot checks only | xUnit mapper/handler/source-guard tests plus temp Host build plus cabinet/RPCS3 smoke | Project constraints require server tests and repeatable cabinet/RPCS3 evidence. [VERIFIED: AGENTS.md] |

**Key insight:** Phase 5's hardest problem is not parsing battle fields; it is preventing unproven defaults and battle payloads from mutating normal Blue state. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md; VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs]

## Common Pitfalls

### Pitfall 1: Treating IDA Initialdata Widths As Full Battleuserdata Proof

**What goes wrong:** The planner assumes the 8-byte/16-byte initialdata copy widths also prove `BattleUserDataResponse` widths. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]

**Why it happens:** Initialdata and battleuserdata share similar field names, but this research found only initialdata copy behavior. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs]

**How to avoid:** Keep rows 7, 8, 13, and 14 unresolved until a battleuserdata response use site or approval exists. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]

**Warning signs:** Code emits battleuserdata byte arrays by reusing initialdata constants. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

### Pitfall 2: Advertising Battle Before Runtime Rows Are Resolved

**What goes wrong:** `is_battleplay=true` makes the client enter battle flow while `battleuserdata.php` and progression state still contain unsafe defaults. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**Why it happens:** IDA proves the field is consumed, but D-07 requires all required defaults/rows/runtime behavior to clear first. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**How to avoid:** Add field plumbing and tests first; keep advertising disabled until the resolution matrix is green or explicitly approved. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**Warning signs:** Tests assert `ShouldSerializeIsBattleplay()` true before rows 1, 2, and 4-16 are resolved. [VERIFIED: Tests/Blue/BlueInitialDataTests.cs; VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]

### Pitfall 3: Letting Battle Payloads Reach Normal Blue Persistence

**What goes wrong:** Battle stages update `SongPlayDataBlue`, `SongBestDataBlue`, recent/favorite rows, profile counters, self-best, crowns, or Dani. [VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs]

**Why it happens:** The current normal Blue handler iterates normal stages and writes multiple normal state surfaces. [VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs]

**How to avoid:** Branch before normal stage processing whenever `AryBattlestagedata` or `AryReleaseBattledata` is present. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**Warning signs:** Battle tests create rows in normal Blue tables, or source guards allow Green AI Battle references in Blue battle implementation. [VERIFIED: .planning/research/PITFALLS.md]

### Pitfall 4: Treating XML Consumption As XML Semantics

**What goes wrong:** The planner derives starting stage, token rewards, stage `33`, or boss-life transitions from XML structure. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**Why it happens:** IDA proves the client loads all five battle XML files, but it does not prove every field's server-side default or progression behavior. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]

**How to avoid:** Plan XML loaders as data readers and keep behavior rules behind row-level proof/approval. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

**Warning signs:** Code comments say "first stage is X because XML row order says so." [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

## Code Examples

Verified patterns from current codebase and Phase 5 decisions:

### Optional Initialdata Mapping

```csharp
// Source: Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs [VERIFIED: codebase grep]
if (common.IsBattleplay is { } isBattleplay)
{
    response.IsBattleplay = isBattleplay;
}
```

### Battle Classification Gate

```csharp
// Source: Phase 5 D-10 [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]
var isBattlePlayResult = request.AryReleaseBattledata is not null
                      || request.AryStagedata.Any(stage => stage.AryBattlestagedata is not null);
```

### Normal-State Protection Assertion Shape

```csharp
// Source: Phase 5 D-12 plus current normal Blue write surfaces [VERIFIED: 05-CONTEXT.md; VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs]
Assert.Empty(context.SongPlayDataBlue);
Assert.Empty(context.SongBestDataBlue);
Assert.Empty(context.BlueRecentSongs);
Assert.Empty(context.BlueFavoriteSongs);
Assert.Empty(context.DanScoreDataBlue);
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Phase 4 treated all battle XML as candidate data only. | Phase 5 research proves all five local battle XML files are loaded by client battle code, while field semantics remain gated. | 2026-05-30 IDA research pass. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | 05-01 can plan loaders as consumed inputs, but not behavior defaults. |
| Phase 4 left initialdata battle widths unknown. | IDA proves 8-byte stage release and 16-byte special release copy behavior for initialdata response handling. | 2026-05-30 IDA research pass. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli] | Width tests can be planned after default source/approval; defaults remain blocked. |
| Battle playresult branch was a design boundary. | D-10/D-12 give a concrete server-side branch trigger and normal-state bypass rule. | Phase 5 context gathered 2026-05-30. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] | 05-03 can plan branch tests even while progression effects remain gated. |
| `battleuserdata.php` route was a stub in server code. | IDA confirms client route/request pipeline but not response defaults. | 2026-05-30 IDA research pass. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs] | Full response behavior remains blocked. |

**Deprecated/outdated:**
- Treating Phase 4's final gate as fully unblocking Phase 5 is outdated; Phase 5 context explicitly says runtime implementation remains blocked until row-by-row resolution. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]
- Treating Green AI Battle as reusable implementation truth is invalid for Blue battle runtime. [VERIFIED: .planning/research/PITFALLS.md]

## Assumptions Log

All claims in this research are tagged as verified or cited from project/context/code/IDA artifacts. No `[ASSUMED]` claims are used. [VERIFIED: self-audit]

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| N/A | No `[ASSUMED]` claims. | All sections | N/A |

## Open Questions

1. **Which exact `battleuserdata.php` response fields are required for a new user?** [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]
   - What we know: route/request pipeline exists and current server route is a stub. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs]
   - What's unclear: response field widths/defaults, required NPC/token rows, assignment, boss life, and last-stage defaults. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]
   - Recommendation: run another IDA pass focused on `BattleUserDataResponse` parser/use sites or capture cabinet/RPCS3 logs before 05-02 runtime response planning. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]

2. **What should `battle_bonds_lv_cap` emit?** [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]
   - What we know: the client stores the supplied value and debug config can ignore the cap. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]
   - What's unclear: whether server should omit, send `65`, or compute from data. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]
   - Recommendation: ask for explicit approval to emit `65` or keep omitted until trace evidence. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]

3. **How do battle rewards, tokens, stage `33`, and boss-life transitions work?** [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]
   - What we know: proto/wire fields and XML inputs exist; IDA proves XML file loading. [VERIFIED: proto/blue/taiko.proto; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs; VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]
   - What's unclear: reward type meanings, token effects, assignment transitions, and stage `33` role. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]
   - Recommendation: defer runtime effects and only raw-capture observed values until proof or approval. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test/Host | yes | Installed `10.0.201`; `global.json` pins `10.0.100` with roll-forward. [VERIFIED: dotnet --version; VERIFIED: global.json] | None needed. |
| Python | ida-cli bridge | yes | 3.13.2. [VERIFIED: python --version] | None needed. |
| ida-cli | IDA evidence | yes | Import path `H:/IDACLI/src/ida_cli/__init__.py`. [VERIFIED: python import ida_cli] | Direct IDA GUI/manual notes if bridge fails. |
| Blue IDB | IDA evidence | yes | `.tools/blue/EBOOT.ELF.i64`, size 168805736, last write 2026-05-27 03:26:09. [VERIFIED: filesystem] | Block IDA-backed rows if missing. |
| ripgrep | Source discovery/source guards | yes | 15.1.0. [VERIFIED: rg --version] | PowerShell `Select-String`. |
| Local Blue battle XML | Catalog/data foundations | yes | Five XML files present in `Host/wwwroot/data/blue/data/config/S10100-1/battle`. [VERIFIED: filesystem] | Operator must provide local game data. |

**Missing dependencies with no fallback:** none found for research. [VERIFIED: Environment Availability]

**Missing dependencies with fallback:** none found for research. [VERIFIED: Environment Availability]

**Helper scripts/artifacts:** No helper scripts or extraction artifacts were created. IDA queries were run inline through `ida-cli` and this RESEARCH.md is the durable output. [VERIFIED: workspace inspection]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1. [VERIFIED: Directory.Packages.props; VERIFIED: Tests/Tests.csproj] |
| Config file | No separate xUnit config detected in the researched files; test package references live in `Tests/Tests.csproj`. [VERIFIED: Tests/Tests.csproj] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~TaikoLocalServer.Tests.Blue` [VERIFIED: Tests/Tests.csproj] |
| Full suite command | `dotnet test Tests/Tests.csproj` [VERIFIED: Tests/Tests.csproj] |
| Host build gate | `dotnet build Host/Host.csproj -o "$env:TEMP\\TaikoLocalServer-host-build-phase05"` when default Host output may be locked. [VERIFIED: AGENTS.md] |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| BTL-01 | Blue battle schema/entities/DbSets/migrations do not reuse Green or normal Blue score tables for battle state. | unit/integration/source guard | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattle` | No - Wave 0 gap. [VERIFIED: Tests/Blue] |
| BTL-02 | `battleuserdata.php` emits only resolved optional fields and persisted values. | mapper/controller/unit | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleUserData` | No - Wave 0 gap. [VERIFIED: Tests/Blue] |
| BTL-03 | Initialdata battle fields remain omitted until set and exact widths are tested only for resolved fields. | mapper/unit | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialData` | Partial - existing omission tests exist. [VERIFIED: Tests/Blue/BlueInitialDataTests.cs] |
| BTL-04 | Battle playresult branch bypasses normal Blue score/crown/history/recent/favorite/profile/Dani writes. | handler/integration | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePlayResult` | Partial - current mapper tests cover battle omission, not runtime branch. [VERIFIED: Tests/Blue/BluePlayResultMapperTests.cs] |
| BTL-05 | Battle rewards/unlocks update only approved battle-owned or approved mirror state. | handler/integration | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleReward` | No - blocked by unresolved rows. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix] |
| BTL-06 | Blue battle implementation has no Green AI Battle dependencies. | source guard | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleSourceGuard` | No - Wave 0 gap. [VERIFIED: .planning/research/PITFALLS.md] |

### Sampling Rate

- **Per task commit:** run the relevant focused `dotnet test Tests/Tests.csproj --filter ...` command plus source guard for touched areas. [VERIFIED: Validation Architecture]
- **Per wave merge:** run `dotnet test Tests/Tests.csproj`. [VERIFIED: Tests/Tests.csproj]
- **Phase gate:** run full suite and temp-output Host build before `$gsd-verify-work`; cabinet/RPCS3 smoke remains required for final done. [VERIFIED: AGENTS.md; VERIFIED: .planning/REQUIREMENTS.md]

### Wave 0 Gaps

- [ ] `Tests/Blue/BlueBattlePersistenceTests.cs` - covers BTL-01 Blue-owned tables and no normal table writes. [VERIFIED: Tests/Blue]
- [ ] `Tests/Blue/BlueBattleUserDataTests.cs` - covers BTL-02 optional presence and resolved defaults. [VERIFIED: Tests/Blue]
- [ ] `Tests/Blue/BlueBattlePlayResultHandlerTests.cs` - covers BTL-04 branch and normal-state protection. [VERIFIED: Tests/Blue]
- [ ] `Tests/Blue/BlueBattleSourceGuardTests.cs` - covers BTL-06 Green AI Battle leakage. [VERIFIED: .planning/research/PITFALLS.md]
- [ ] Row-resolution checkpoint tests or artifacts before 05-02 and 05-03 emit gated behavior. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | no new auth | Phase 5 cabinet protocol routes are existing local game protocol surfaces; Admin auth stack is unchanged. [VERIFIED: AGENTS.md; VERIFIED: Host/Program.cs] |
| V3 Session Management | no new sessions | Phase 5 does not introduce browser or API sessions. [VERIFIED: .planning/ROADMAP.md] |
| V4 Access Control | yes, data separation | Enforce Blue-owned state separation and prevent Green/Nijiiro/normal Blue battle leakage. [VERIFIED: AGENTS.md; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| V5 Input Validation | yes | Validate protobuf optional presence, byte widths, row counts, XML-derived IDs, and unsupported battle values before persistence/effects. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| V6 Cryptography | no new crypto | Phase 5 does not add cryptographic behavior. [VERIFIED: .planning/ROADMAP.md] |
| V8 Data Protection | yes, local integrity | Keep battle state in Blue-owned SQLite tables and avoid corrupting normal Blue/Green/Nijiiro state. [VERIFIED: AGENTS.md] |

### Known Threat Patterns for Blue Battle Runtime

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Cross-era or cross-mode state corruption | Tampering | Blue-owned battle tables plus tests proving no writes to normal Blue scores/crowns/history/Dani or Green AI Battle state. [VERIFIED: AGENTS.md; VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs] |
| Unsafe protobuf defaults causing client misbehavior | Denial of Service | Omit unresolved optional fields; emit byte arrays only after exact width/default proof or approval. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] |
| Local XML semantic overreach | Tampering | Parse XML as local input, but gate behavior semantics behind IDA/log/cabinet proof or user approval. [VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md] |
| Overbroad request logging | Information Disclosure | Use structured logs with bounded context and avoid dumping huge protobuf payloads. [VERIFIED: AGENTS.md; VERIFIED: .planning/research/PITFALLS.md] |

## Sources

### Primary (HIGH confidence)

- `AGENTS.md` - project constraints, stack, architecture, conventions, verification requirements. [VERIFIED: AGENTS.md]
- `.planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md` - Phase 5 locked decisions and canonical refs. [VERIFIED: codebase grep]
- `.planning/phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md` - 26 missing-evidence rows and gate status. [VERIFIED: codebase grep]
- `.tools/blue/EBOOT.ELF.i64` via `ida-cli` - Blue client route/field/XML evidence and address anchors. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]
- `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` - Blue battle schema and generated presence helpers. [VERIFIED: codebase grep]
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - current normal Blue persistence side effects that battle must bypass. [VERIFIED: codebase grep]

### Secondary (MEDIUM confidence)

- `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` - route/proto/current-owner Phase 4 evidence. [VERIFIED: codebase grep]
- `.planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md` - local battle XML inventory, row counts, hashes, and candidate roles. [VERIFIED: codebase grep]
- `.planning/research/ARCHITECTURE.md` - Blue-owned architecture guidance. [VERIFIED: codebase grep]
- `.planning/research/PITFALLS.md` - battle pitfalls and guardrails. [VERIFIED: codebase grep]
- `Directory.Build.props`, `Directory.Packages.props`, `Tests/Tests.csproj`, `global.json` - build/test/package versions. [VERIFIED: codebase grep]

### Tertiary (LOW confidence)

- Wiki gameplay semantics are referenced only through Phase 5 CONTEXT as a user-approved gameplay/field-intent source; this research did not fetch the page directly and does not use it for wire defaults. [CITED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - verified from project files and local CLI probes. [VERIFIED: Directory.Packages.props; VERIFIED: dotnet --version]
- Architecture: HIGH - constrained by AGENTS.md, existing Blue code, and Phase 5 context. [VERIFIED: AGENTS.md; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]
- IDA route/XML findings: HIGH - direct strings/xrefs/decompilation from `.tools/blue/EBOOT.ELF.i64`. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli]
- IDA initialdata field mapping: MEDIUM - copy widths are directly decompiled, but mapping from decompiled offsets to exact proto field names is inferred from generated wire layout/order. [VERIFIED: IDA .tools/blue/EBOOT.ELF.i64 via ida-cli; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs]
- Battleuserdata defaults/progression/rewards: LOW - unresolved by this research, intentionally left gated. [VERIFIED: Phase 5 Missing-Evidence Resolution Matrix]

**Research date:** 2026-05-30
**Valid until:** 2026-06-06, or sooner if new cabinet/RPCS3 logs, IDA symbol updates, or user approvals resolve missing rows. [VERIFIED: current_date; VERIFIED: .planning/phases/05-blue-battle-runtime-support/05-CONTEXT.md]
