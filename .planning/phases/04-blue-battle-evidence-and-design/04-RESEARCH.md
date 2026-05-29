# Phase 04: Blue Battle Evidence And Design - Research

**Researched:** 2026-05-30 [VERIFIED: gsd init.phase-op]
**Domain:** Blue AC15 battle protocol evidence, local battle XML inventory, and Phase 5 implementation gate [VERIFIED: .planning/ROADMAP.md]
**Confidence:** MEDIUM [VERIFIED: codebase grep] - local proto, generated wire, route stubs, tests, and XML inventory are strong, but required IDA/client/default-width proof remains Phase 4 work.

<user_constraints>
## User Constraints (from CONTEXT.md)

Source for this section: `.planning/phases/04-blue-battle-evidence-and-design/04-CONTEXT.md` [VERIFIED: codebase grep]

### Locked Decisions

### Evidence sources and gate
- **D-01:** Phase 4 should not depend on RPCS3/cabinet logs. Use static client/proto evidence for battle design; reserve RPCS3/cabinet logs for later verification.
- **D-02:** For wire/runtime mechanics, IDA/client behavior is authoritative when it disagrees with proto, generated wire, or local XML. Proto/wire and XML are candidate maps, not final semantics.
- **D-03:** Wiki pages are important gameplay-semantics sources. Use them to avoid heavy IDA checking for gameplay topics such as whether battle score/crown state is separate from normal records.
- **D-04:** Phase 4 should produce a traceable evidence pack: field-by-field source citations, local file inventory, IDA/client notes, Wiki gameplay notes, and explicit unknowns.
- **D-05:** If a battle field/default cannot be proven, do not silently stub or blanket-block it. Stop for case-by-case user approval before Phase 5 planning relies on it.

### Battle data-file inventory
- **D-06:** Treat `Host/wwwroot/data/blue/data/config/S10100-1/battle` XML files as candidate inputs until IDA/client evidence proves their exact runtime role.
- **D-07:** Inventory all five known battle files: `battleadjsetting.xml`, `battlenpcinfo.xml`, `battlestageinfo.xml`, `battlesupportinfo.xml`, and `battletokeninfo.xml`.
- **D-08:** Phase 4 should document row counts, key fields, and candidate relationships only. Do not create committed runtime JSON or loaders in this phase.
- **D-09:** Local Blue battle data is a hard prerequisite for Phase 4/5 work. Do not substitute committed fixtures for missing local battle data.

### Wire/default-state contract
- **D-10:** Every battle byte-array field needs explicit width/default proof before implementation, including release info flags, battle stage flags, battle special flags, NPC costume/special flags, and token/state arrays.
- **D-11:** Optional battle protobuf fields with unproven defaults should be omitted using protobuf presence semantics.
- **D-12:** Repeated battle rows such as NPC and token data require evidence for minimum safe row counts. Do not assume empty lists or one row per XML row.
- **D-13:** Phase 5 must include tests for `ShouldSerialize`/omission behavior, exact byte lengths, required repeated-row counts, and source guards preventing Green AI Battle implementation leakage.

### Battle playresult effects
- **D-14:** Battle score/crown state is separate from normal Blue score/crown records per the linked gameplay Wiki. Do not update normal Blue self-best or crowns from battle stages.
- **D-15:** Persist battle-owned progress from battle playresults: battle stage/progression, bonds level, best battle power, NPC/token/special state, rewards, and related battle state.
- **D-16:** Battle reward arrays should mirror into normal Blue unlock handling when they use existing playresult release fields.
- **D-17:** Battle stages should not write normal Blue play history, recent/favorites, profile play counters, normal self-best, or normal crowns. Use battle-owned history/progress instead.

### Phase 5 runtime boundary
- **D-18:** Phase 5 runtime implementation requires an approved Phase 4 evidence pack and battle design spec, with unresolved cases approved by the user.
- **D-19:** Phase 5 battle implementation must be Blue-owned only: Blue entities, handlers, mappers, controllers, migrations, catalog types, tests, and source guards. Green AI Battle is contrast material only.
- **D-20:** After the design gate passes, Phase 5 should implement the full battle loop: `battleuserdata.php`, `initialdatacheck.php` battle flags, battle playresult persistence, rewards/unlocks, progression, tests, and source guards.
- **D-21:** Phase 5 done means automated proof and build: focused unit/integration tests, mapper presence/width tests, source guards, full tests, and temp-output Host build. RPCS3/cabinet battle smoke stays in the verification stage.

### the agent's Discretion
None. The user made explicit decisions for all selected gray areas.

### Deferred Ideas (OUT OF SCOPE)
- RPCS3/cabinet battle logs are deferred to verification-stage proof rather than Phase 4 design evidence.
- Runtime Blue battle JSON/default data generation is deferred until Phase 5, after the Phase 4 design gate.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| BTEV-01 | Battle menu entry and at least one attempted battle flow have cabinet/RPCS3 logs or equivalent client evidence. | Plan static equivalent-client evidence capture from Blue IDA/client artifacts and existing route observations; do not require RPCS3/cabinet logs in Phase 4 per D-01. [VERIFIED: 04-CONTEXT.md] |
| BTEV-02 | Blue battle-related proto messages are mapped to generated Blue wire types and documented with request/response ownership. | Use the proto/wire map in this research as the checklist for `InitialdatacheckResponse`, `BattleUserData*`, `PlayResultRequest.StageData.BattleStageData`, and `ReleaseBattleData`. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] |
| BTEV-03 | Local Blue `config/S10100-1/battle` files are inventoried and classified as required, optional, or unknown for battle menu entry. | All five known files are present under the local symlinked Blue data tree, with row counts and hashes recorded below; menu-entry required/optional status remains unknown until IDA/client proof. [VERIFIED: local XML inventory] |
| BTEV-04 | Battle release flag byte widths, NPC state defaults, costume/special defaults, token defaults, stage assignment defaults, and boss/last-stage defaults are confirmed from proto, data, IDA/client evidence, or cabinet traces. | This research enumerates every known width/default risk and recommends a field-by-field evidence matrix before any Phase 5 runtime task is allowed. [VERIFIED: 04-CONTEXT.md; proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] |
| BTEV-05 | The battle design decides whether battle playresults affect normal Blue scores/crowns based on client evidence. | Gameplay semantics support separate battle score/crown state; Phase 4 must record this separately from wire/runtime proof and gate any normal-record side effects. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB] |
| BTEV-06 | Battle implementation is blocked until BTEV-01 through BTEV-05 are satisfied or explicitly revised with user approval. | Plan 04-03 as an implementation gate review that either approves Phase 5 inputs or records user-approved exceptions per field. [VERIFIED: 04-CONTEXT.md] |
</phase_requirements>

## Summary

Phase 4 should be planned as an evidence-pack and design-spec phase, not a runtime implementation phase. [VERIFIED: .planning/phases/04-blue-battle-evidence-and-design/04-CONTEXT.md] The planner should split work into route/proto/client-evidence capture, local XML inventory/default-width analysis, and a final gate document that explicitly blocks Phase 5 unless every battle field/default/row-count decision is proven or user-approved. [VERIFIED: .planning/ROADMAP.md]

Current code already has the key Blue battle touchpoints, but they are intentionally incomplete: `battleuserdata.php` returns only `Result = 1`, `initialdatacheck.php` can omit or include nullable battle fields, and `playresult.php` maps normal direct-protobuf fields while ignoring `BattleStageData` and `ReleaseBattleData`. [VERIFIED: codebase grep] The local battle XML exists and is symlinked from the local Blue RPCS3 data tree, but the XML can only provide candidate catalog/default inputs until client/IDA evidence proves runtime meaning. [VERIFIED: local XML inventory]

**Primary recommendation:** Plan Phase 4 around a traceable field matrix: route -> proto field -> generated wire property -> local XML candidate -> IDA/client proof -> default/width decision -> Phase 5 task/gate status. [VERIFIED: 04-CONTEXT.md]

## Project Constraints (from AGENTS.md)

- Blue remains a first-class era with Blue-owned persistence, handlers, mappers, catalogs, tests, routes, and state. [VERIFIED: AGENTS.md]
- Battle runtime must be specified from proto, logs, IDA/client evidence, or cabinet/RPCS3 traces before implementation. [VERIFIED: AGENTS.md]
- Blue, Green, and Nijiiro persistent state must remain separate unless the state is truly shared identity state. [VERIFIED: AGENTS.md]
- Known Blue direct-protobuf and shared startup/verup assumptions must be preserved unless newer client evidence contradicts them. [VERIFIED: AGENTS.md]
- Normal Track A A6-A8 must remain finished before battle runtime implementation; Phase 4 may prepare Track B evidence/design only. [VERIFIED: AGENTS.md]
- Full done still requires repeatable cabinet/RPCS3 smoke evidence for battle flows, but Phase 4 context defers those logs to verification-stage proof. [VERIFIED: AGENTS.md; 04-CONTEXT.md]
- Blue runtime data under `Host/wwwroot/data/blue/data` is local/operator-supplied and may be gitignored. [VERIFIED: AGENTS.md; Host/.gitignore]
- If the normal Host output is locked, use `dotnet build Host/Host.csproj -o <temp-output>` for build validation. [VERIFIED: AGENTS.md]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Battle route ownership | Game protocol adapter | Host route gating | Blue controllers own `/v10r03/chassis/*` routes, while Host enables/disables adapter assemblies by era. [VERIFIED: .planning/research/ARCHITECTURE.md; Tests/Blue/BlueRouteSkeletonTests.cs] |
| Battle initial availability flags | Application handler | Blue protocol mapper | `GetInitialDataQuery.Blue.cs` builds common initial data and `InitialDataMappers.cs` owns wire presence for optional battle fields. [VERIFIED: codebase grep] |
| Battle user state | Application/Domain/Infrastructure | Blue protocol adapter | Phase 5 must add Blue-owned entities, DbSets, handlers, and mapping only after Phase 4 proves defaults and widths. [VERIFIED: 04-CONTEXT.md] |
| Local battle catalog data | Infrastructure catalog layer | Local filesystem data | XML under `Host/wwwroot/data/blue/data/config/S10100-1/battle` is operator-local candidate data and should not become committed runtime JSON in Phase 4. [VERIFIED: local XML inventory; 04-CONTEXT.md] |
| Battle playresult interpretation | Application handler | Blue protocol mapper | `PlayResultController` sends mapped common data into `UpdatePlayResultCommand` today; battle semantics need Blue-owned DTO/handler design before persistence. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs; Application/Handlers/UpdatePlayResultCommand.Blue.cs] |
| Phase 5 implementation gate | Planning docs | Tests/source guards | Phase 4 must create approval evidence; Phase 5 tasks must not proceed on unproven defaults. [VERIFIED: .planning/REQUIREMENTS.md; 04-CONTEXT.md] |

## Sources Read

- Project instructions: `AGENTS.md` from prompt and repository. [VERIFIED: codebase grep]
- Phase constraints: `.planning/phases/04-blue-battle-evidence-and-design/04-CONTEXT.md`. [VERIFIED: codebase grep]
- Project planning state: `.planning/REQUIREMENTS.md`, `.planning/ROADMAP.md`, `.planning/STATE.md`. [VERIFIED: codebase grep]
- Prior research: `.planning/research/SUMMARY.md`, `.planning/research/ARCHITECTURE.md`, `.planning/research/PITFALLS.md`. [VERIFIED: codebase grep]
- Original roadmap: `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`. [VERIFIED: codebase grep]
- Protocol inputs: `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs`. [VERIFIED: codebase grep]
- Blue runtime touchpoints: `BattleUserDataController.cs`, `InitialDataCheckController.cs`, `PlayResultController.cs`, `InitialDataMappers.cs`, `PlayResultMappers.cs`, `CommonInitialDataCheckResponse.Blue.cs`, `GetInitialDataQuery.Blue.cs`, `UpdatePlayResultCommand.Blue.cs`. [VERIFIED: codebase grep]
- Guard tests: `Tests/Blue/BluePlayResultMapperTests.cs`, `Tests/Blue/BlueRouteSkeletonTests.cs`, `Tests/Blue/BlueDocsTests.cs`, plus `Tests/Blue/BlueInitialDataTests.cs` and `Tests/Blue/BlueWireGenerationTests.cs` found during research. [VERIFIED: codebase grep]
- Local battle data: `Host/wwwroot/data/blue/data/config/S10100-1/battle`. [VERIFIED: local XML inventory]
- Gameplay reference: Blue Enso Battle Wiki page, opened on 2026-05-30. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB]

## Battle Surface Map

### Current Route And Stub Map

| Surface | Current State | Planning Concern |
|---------|---------------|------------------|
| `/v10r03/chassis/battleuserdata.php` | Blue adapter owns the route and returns `new BattleUserDataResponse { Result = 1 }` without Mediator. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs] | Phase 5 must not replace this stub until Phase 4 proves safe default/persisted `BattleUserDataResponse` contents. [VERIFIED: 04-CONTEXT.md] |
| `/v10r03/chassis/initialdatacheck.php` | Blue controller is Mediator-backed and maps `GetInitialDataQuery(GameEra.Blue)` through `InitialDataMappers.Map`. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs] | Battle advertisement fields should be added through `CommonInitialDataCheckResponse.Blue.cs` only after width/default proof. [VERIFIED: Application/Dtos/CommonInitialDataCheckResponse.Blue.cs] |
| `/v10r03/chassis/playresult.php` | Blue controller uses direct protobuf `PlayResultRequest` and sends `UpdatePlayResultCommand(..., GameEra.Blue, common)`. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs] | Battle playresults need a Blue-owned DTO/mapper extension before handler persistence, because the current mapper ignores battle nested messages. [VERIFIED: Tests/Blue/BluePlayResultMapperTests.cs] |
| Blue route skeleton guard | `battleuserdata.php` is in the expected Blue route list, and non-implemented controllers are guarded from accidental `Mediator.Send`. [VERIFIED: Tests/Blue/BlueRouteSkeletonTests.cs] | Phase 5 should update the mediator-backed allowlist only when battleuserdata becomes implemented. [VERIFIED: Tests/Blue/BlueRouteSkeletonTests.cs] |
| Blue initialdata omission guard | Existing test asserts battle fields are omitted by default via `ShouldSerialize*` returning false. [VERIFIED: Tests/Blue/BlueInitialDataTests.cs] | Phase 5 needs new positive and negative omission tests for each battle field that becomes enabled. [VERIFIED: 04-CONTEXT.md] |

### Proto To Generated Wire Ownership

| Proto Surface | Generated Wire Surface | Request/Response Owner | Planning Risk |
|---------------|------------------------|------------------------|---------------|
| `InitialdatacheckResponse.is_battleplay` | `InitialdatacheckResponse.IsBattleplay` plus `ShouldSerializeIsBattleplay`. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | `GetInitialDataQuery.Blue.cs` -> `InitialDataMappers.cs`. [VERIFIED: codebase grep] | Prove when to advertise battle menu entry and whether omission or explicit false is safer. [VERIFIED: 04-CONTEXT.md] |
| `InitialdatacheckResponse.release_battle_stage_flg` | `InitialdatacheckResponse.ReleaseBattleStageFlg` plus `ShouldSerializeReleaseBattleStageFlg`. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | Initial data response. [VERIFIED: codebase grep] | Exact byte width and default bits are unknown. [VERIFIED: 04-CONTEXT.md] |
| `InitialdatacheckResponse.release_battle_special_flg` | `InitialdatacheckResponse.ReleaseBattleSpecialFlg` plus `ShouldSerializeReleaseBattleSpecialFlg`. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | Initial data response. [VERIFIED: codebase grep] | Exact byte width and relation to NPC `release_special_flg` are unknown. [VERIFIED: 04-CONTEXT.md] |
| `InitialdatacheckResponse.battle_bonds_lv_cap` | `InitialdatacheckResponse.BattleBondsLvCap` plus `ShouldSerializeBattleBondsLvCap`. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | Initial data response. [VERIFIED: codebase grep] | Wiki and local NPC data both point at a 65-level concept, but client proof should decide whether this field is `65` or omitted. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB; VERIFIED: local XML inventory] |
| `BattleUserDataRequest` | `BattleUserDataRequest` with `Baid`, `ChassisId`, and `ShopId`. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | `BattleUserDataController`. [VERIFIED: codebase grep] | Request is simple, but response defaults are the hard part. [VERIFIED: proto/blue/taiko.proto] |
| `BattleUserDataResponse.release_info_flg` | `BattleUserDataResponse.ReleaseInfoFlg` plus `ShouldSerializeReleaseInfoFlg`. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Battle userdata response. [VERIFIED: codebase grep] | Width/default is not covered by `BlueProtocolBytes`; must be proven separately. [VERIFIED: Application/Common/BlueProtocolBytes.cs] |
| `BattleUserDataResponse.release_battle_stage_flg` | `BattleUserDataResponse.ReleaseBattleStageFlg` plus `ShouldSerializeReleaseBattleStageFlg`. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Battle userdata response. [VERIFIED: codebase grep] | Must be reconciled with initialdata `release_battle_stage_flg`. [VERIFIED: proto/blue/taiko.proto] |
| `BattleUserDataResponse.npc_data` | `BattleUserDataResponse.NpcDatas` list with required `NpcCostumeFlg` and optional `ReleaseSpecialFlg`. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | Battle userdata response. [VERIFIED: codebase grep] | Minimum NPC row count, costume flag width, selected-special defaults, and release-special width are unknown. [VERIFIED: 04-CONTEXT.md] |
| `BattleUserDataResponse.ary_token_data` | `BattleUserDataResponse.AryTokenDatas` list. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | Battle userdata response. [VERIFIED: codebase grep] | Minimum token row count and token defaults are unknown; XML has two token rows but that is candidate data only. [VERIFIED: local XML inventory; 04-CONTEXT.md] |
| `BattleUserDataResponse.assign_stage_id` | `BattleUserDataResponse.AssignStageId` plus `ShouldSerializeAssignStageId`. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | Battle userdata response. [VERIFIED: codebase grep] | Default assignment must be client-proven rather than inferred from stage XML. [VERIFIED: 04-CONTEXT.md] |
| `PlayResultRequest.StageData.BattleStageData` | `StageData.AryBattlestagedata` reference containing battle progress/result fields. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | Playresult request. [VERIFIED: codebase grep] | Current mapper ignores this object, so Phase 5 needs explicit DTO and persistence design. [VERIFIED: Tests/Blue/BluePlayResultMapperTests.cs] |
| `PlayResultRequest.ReleaseBattleData` | `PlayResultRequest.AryReleaseBattledata` reference with release IDs, token data, and `AssignNextStageId`. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | Playresult request. [VERIFIED: codebase grep] | Current mapper ignores this object; Phase 5 must decide which releases mirror to normal Blue unlock state. [VERIFIED: Tests/Blue/BluePlayResultMapperTests.cs; 04-CONTEXT.md] |

### Current Normal-Mode Boundary

- `UpdatePlayResultCommand.Blue.cs` accepts only normal Blue stage modes `0` and `1`; unsupported stage modes are logged and skipped. [VERIFIED: Application/Common/BluePlayResultMapping.cs; Application/Handlers/UpdatePlayResultCommand.Blue.cs]
- `CommonPlayResultData.StageData` has a shared `SupportLevel` field, but the Blue mapper does not map `BattleStageData.SupportLv` into it today. [VERIFIED: Application/Dtos/CommonPlayResultData.cs; Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs]
- Existing Blue playresult mapping intentionally does not infer runtime semantics from `AryBattlestagedata` or `AryReleaseBattledata`. [VERIFIED: Tests/Blue/BluePlayResultMapperTests.cs]
- Planning must preserve normal Blue playresult behavior while adding separate battle state; battle stages should not flow into normal play history, recent/favorites, self-best, crowns, Dani, or normal profile counters. [VERIFIED: 04-CONTEXT.md]

## Data File Inventory

The local Blue data tree is a symbolic link from `Host/wwwroot/data/blue/data` to `H:\RPCS3\rpcs3-blue\dev_hdd0\game\SCEEXE001\USRDIR\data`. [VERIFIED: local filesystem probe] `Host/.gitignore` ignores `wwwroot/data/blue/data`, so the battle XML must remain operator-local and should not be committed. [VERIFIED: Host/.gitignore]

| File | Present | Size | SHA-256 | Verified Counts | Key Fields / Candidate Role | Menu Entry Classification |
|------|---------|------|---------|-----------------|-----------------------------|---------------------------|
| `battleadjsetting.xml` | yes [VERIFIED: local XML inventory] | 3,842 bytes [VERIFIED: local XML inventory] | `309ED4FCCBF6A637BABDA290057869EEDBA01A78FEBDE74849CAC6C05F18E8FE` [VERIFIED: Get-FileHash] | One `adjustedsetting` node with 33 immediate field groups. [VERIFIED: local XML inventory] | Damage rates, critical rates, treasurebox/token/special selection arrays look like battle tuning inputs. [VERIFIED: local XML field scan; ASSUMED candidate role] | Unknown until IDA/client proves whether menu entry needs it. [VERIFIED: 04-CONTEXT.md] |
| `battlenpcinfo.xml` | yes [VERIFIED: local XML inventory] | 3,819 bytes [VERIFIED: local XML inventory] | `05C1D806872D3A11B42638F44C276B6E61C560444E25B46AE78E3B1CC4D0A790` [VERIFIED: Get-FileHash] | Header `size=1`; actual `npcinfo` rows=1; row has 65 `requred_exp` values and 65 `atk` values. [VERIFIED: local XML inventory] | NPC id, starting exp, special attack fields, required exp, and attack values are candidate NPC/default progression inputs. [VERIFIED: local XML field scan; ASSUMED candidate role] | Unknown; repeated `BattleUserDataResponse.npc_data` may require one or more response rows, but XML count alone is not safe proof. [VERIFIED: 04-CONTEXT.md] |
| `battlestageinfo.xml` | yes [VERIFIED: local XML inventory] | 5,803 bytes [VERIFIED: local XML inventory] | `8192799768D12E6E28F3847333E1D10C71A26B065296AA130C9492C4110256CC` [VERIFIED: Get-FileHash] | Header `size=11`; actual `stageinfo` rows=11; stage ids include `1..10` plus `33`. [VERIFIED: local XML inventory] | Stage id, stage number, proper level, reward exp, prev/next links, enemy species/life arrays, and boss availability/life are candidate stage/progression inputs. [VERIFIED: local XML field scan; ASSUMED candidate role] | Unknown; likely important for stage assignment but exact menu-entry dependency needs client proof. [ASSUMED; VERIFIED: local XML inventory] |
| `battlesupportinfo.xml` | yes [VERIFIED: local XML inventory] | 586,190 bytes [VERIFIED: local XML inventory] | `A49D9EA0102AF1EF656AED761E423F37DD972D29180C98FB720426CA207BA354` [VERIFIED: Get-FileHash] | Header `size=756`; actual `musicid` rows=756; actual `supportinfo` rows=756; all sampled `supportinfo` entries have `coursepatterns` count=5. [VERIFIED: local XML inventory] | Music-id keyed support/course pattern data is candidate song-to-battle-course assignment input. [VERIFIED: local XML field scan; ASSUMED candidate role] | Unknown; large row count makes row-count proof critical before response generation. [VERIFIED: 04-CONTEXT.md] |
| `battletokeninfo.xml` | yes [VERIFIED: local XML inventory] | 1,793 bytes [VERIFIED: local XML inventory] | `5E92A5EDAA1AC4D3C56C0DB1661CA41C831D407B40C84AEDFF22952D6B772AD6` [VERIFIED: Get-FileHash] | Header `size=2`; actual `tokeninfo` rows=2; token id `1` has 8 rewards; token id `17` has 2 rewards. [VERIFIED: local XML inventory] | Token id, available level, available release-stage id, reward token value, reward type, reward id, and uid are candidate battle token/reward inputs. [VERIFIED: local XML field scan; ASSUMED candidate role] | Unknown; token rows map to `BattleUserDataResponse.ary_token_data` and `ReleaseBattleData.ary_battletokendata` only after evidence. [VERIFIED: proto/blue/taiko.proto; local XML inventory] |

**Important data-planning constraint:** Phase 4 can document row counts, hashes, key fields, and candidate relationships only; committed loaders, committed runtime JSON, and migrations belong to Phase 5 or later after the gate passes. [VERIFIED: 04-CONTEXT.md]

## Wire/Default-State Risks

| Risk Area | Why It Matters | Planning Action |
|-----------|----------------|-----------------|
| Optional field omission | Generated wire exposes `ShouldSerialize*` for battle initialdata and battleuserdata optional scalar/byte fields. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] | For each unproven field, document omission as the default plan and add Phase 5 tests before enabling it. [VERIFIED: 04-CONTEXT.md] |
| Byte-array widths | Existing `BlueProtocolBytes` constants cover normal song/tone/title/costume/Dan/content/crown arrays, not battle release or NPC/special arrays. [VERIFIED: Application/Common/BlueProtocolBytes.cs] | Prove widths for `release_info_flg`, both `release_battle_stage_flg` fields, `release_battle_special_flg`, `npc_costume_flg`, and `release_special_flg` via IDA/client evidence. [VERIFIED: proto/blue/taiko.proto; 04-CONTEXT.md] |
| Required nested NPC fields | `BattleUserNpcData` requires `npc_id`, `total_exp`, `max_dpn`, `npc_costume_id`, `npc_costume_flg`, and last selected specials. [VERIFIED: proto/blue/taiko.proto] | Do not emit NPC rows until minimum row count, string numeric format, DPN default, costume default, and special defaults are proven. [VERIFIED: 04-CONTEXT.md] |
| Required token rows | `BattleUserTokenData` and `ReleaseBattleData.BattleTokenData` carry `token_id` and `token_value`. [VERIFIED: proto/blue/taiko.proto] | Distinguish persisted token state from playresult release token deltas; XML row count alone is not enough. [VERIFIED: local XML inventory; 04-CONTEXT.md] |
| Stage assignment defaults | `BattleUserDataResponse.assign_stage_id` is optional, while `ReleaseBattleData.assign_next_stage_id` is required if `ReleaseBattleData` is present. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | Prove first-stage and next-stage behavior before persisting or returning defaults. [VERIFIED: 04-CONTEXT.md] |
| Last boss/stage state | `last_battle_stage_id`, `last_boss_life`, and `last_npc_id` are optional userdata fields. [VERIFIED: proto/blue/taiko.proto] | Prove whether new users should omit these fields or send explicit values. [VERIFIED: 04-CONTEXT.md] |
| Battle playresult normal-field overlap | `StageData` still carries normal song, score, play result, favorite/recent, and folder fields next to optional `BattleStageData`. [VERIFIED: proto/blue/taiko.proto] | The design must say which normal fields are read for battle and which are ignored for normal persistence. [VERIFIED: 04-CONTEXT.md] |
| Green AI Battle leakage | Green has separate AI Battle stage-mode logic and tests, but Phase 4 decisions forbid using it as Blue truth. [VERIFIED: .planning/research/PITFALLS.md; 04-CONTEXT.md] | Add a Phase 5 source guard requirement rejecting Green AI Battle dependencies in Blue battle files. [VERIFIED: 04-CONTEXT.md] |

## Gameplay Semantics

- The external gameplay source says Blue Enso Battle was added in the 2018-06-27 update and selected during one-player player entry. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB, lines 40-42]
- The source says songs played in battle can still grant titles. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB, line 44]
- The source says the mode records bond level and best battle power, while normal crowns and scores are not recorded because rules differ. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB, lines 45-47]
- The source says Don and Katsu medals can still be earned after battle play. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB, line 48]
- The source says the mode ended the day before Green Ver. started. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB, line 49]
- Gameplay-semantics evidence is acceptable for decisions like score/crown separation, but IDA/client/proto evidence remains authoritative for wire shape, byte widths, defaults, and safe runtime behavior. [VERIFIED: 04-CONTEXT.md]

## Phase 5 Gate Inputs

Phase 4 should produce these concrete inputs before Phase 5 planning starts:

| Gate Input | Must Contain | Blocks Phase 5 If Missing |
|------------|--------------|---------------------------|
| Battle route evidence pack | Endpoint sequence for battle menu entry and attempted battle flow from static client/IDA evidence or later logs; route ownership for `battleuserdata.php`, `initialdatacheck.php`, and `playresult.php`. [VERIFIED: .planning/REQUIREMENTS.md; codebase grep] | yes, unless user revises BTEV-01. [VERIFIED: 04-CONTEXT.md] |
| Proto/wire field matrix | Every battle proto field, generated wire property, presence helper status, current mapper/handler owner, and Phase 5 owner. [VERIFIED: proto/blue/taiko.proto; Adapters.GameProtocol.Blue/Wire/Game.cs] | yes for BTEV-02. [VERIFIED: .planning/REQUIREMENTS.md] |
| Local XML inventory appendix | All five file hashes, row counts, key fields, and candidate relationships, plus explicit required/optional/unknown classification for menu entry. [VERIFIED: local XML inventory] | yes for BTEV-03. [VERIFIED: .planning/REQUIREMENTS.md] |
| Default/width proof matrix | Width/default proof for every battle byte array, repeated row, stage assignment, boss/last-stage field, NPC field, and token field. [VERIFIED: 04-CONTEXT.md] | yes for BTEV-04 unless user approves an exception. [VERIFIED: 04-CONTEXT.md] |
| Battle playresult effects decision | Clear rule that battle progress is battle-owned, normal self-best/crowns/history are not updated, and release arrays mirror normal unlocks only where approved. [VERIFIED: 04-CONTEXT.md; CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB] | yes for BTEV-05. [VERIFIED: .planning/REQUIREMENTS.md] |
| Implementation gate review | A final "approved / blocked / approved with exceptions" status with user-approved unknowns listed one by one. [VERIFIED: 04-CONTEXT.md] | yes for BTEV-06. [VERIFIED: .planning/REQUIREMENTS.md] |

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1. [VERIFIED: AGENTS.md] |
| Config file | `Tests/Tests.csproj`; no separate xUnit config was identified in the required-source pass. [VERIFIED: codebase grep] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueInitialDataTests|FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BlueRouteSkeletonTests|FullyQualifiedName~BlueDocsTests"` [VERIFIED: existing test names] |
| Full suite command | `dotnet test Tests/Tests.csproj` [VERIFIED: AGENTS.md] |
| Host build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-evidence"` when default output is locked. [VERIFIED: AGENTS.md] |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated / Manual Validation | File Exists? |
|--------|----------|-----------|-------------------------------|--------------|
| BTEV-01 | Battle menu entry and attempted battle flow have equivalent client evidence. | evidence review | Manual review of `04-01` evidence pack; automated grep can require route names and evidence-source rows. [VERIFIED: .planning/REQUIREMENTS.md] | no, Wave 0 gap |
| BTEV-02 | Proto and generated wire fields are mapped with ownership. | doc/source guard | Add a doc/source test that checks the design matrix references `InitialdatacheckResponse`, `BattleUserDataResponse`, `BattleStageData`, and `ReleaseBattleData`. [VERIFIED: proto/blue/taiko.proto] | no, Wave 0 gap |
| BTEV-03 | All five battle XML files are inventoried with counts and classifications. | local data audit | PowerShell XML audit should parse local symlinked files and record counts/hashes; CI fallback should skip with explicit local-data unavailable status. [VERIFIED: local XML inventory] | no, Wave 0 gap |
| BTEV-04 | Byte widths and defaults are proven or explicitly unknown-approved. | evidence matrix | Automated checklist should fail if any required field remains `UNKNOWN` without a user-approved exception. [VERIFIED: 04-CONTEXT.md] | no, Wave 0 gap |
| BTEV-05 | Battle playresults do not update normal Blue score/crown/history except approved unlock mirrors. | design review + future tests | Phase 4 design must state the rule; Phase 5 tests should cover normal-state non-mutation. [VERIFIED: 04-CONTEXT.md] | partial, existing normal mapper guard only |
| BTEV-06 | Phase 5 implementation stays blocked until BTEV-01..BTEV-05 pass or are revised. | gate review | Require final `04-03` gate status and user approval for exceptions before Phase 5 planning. [VERIFIED: 04-CONTEXT.md] | no, Wave 0 gap |

### Sampling Rate

- Per plan/task commit: run the quick Blue-focused test command when code/tests change; for docs-only evidence commits, run a deterministic grep/checklist script or doc test if the plan adds one. [VERIFIED: existing tests; ASSUMED doc-test command until planner defines it]
- Per wave merge: run `dotnet test Tests/Tests.csproj` if any source or test file changes; docs-only waves can use checklist validation plus full test only at closeout. [VERIFIED: AGENTS.md; ASSUMED docs-only sampling]
- Phase gate: BTEV-01..BTEV-05 evidence matrix complete, BTEV-06 gate approved, and no runtime loaders/JSON/migrations committed in Phase 4. [VERIFIED: 04-CONTEXT.md]

### Wave 0 Gaps

- `Tests/Blue/BlueBattleEvidenceTests.cs` or equivalent doc guard should be added only if the planner wants automated evidence completeness checks. [ASSUMED]
- A local XML audit command/script should be captured in plan docs, but should not create committed runtime JSON or loaders. [VERIFIED: 04-CONTEXT.md]
- The final gate review artifact should define `APPROVED`, `BLOCKED`, and `APPROVED_WITH_USER_EXCEPTIONS` statuses. [ASSUMED]

## Planning Recommendations

1. Plan `04-01` as route/proto/client evidence capture. It should produce a route sequence, proto/wire ownership matrix, and static client/IDA evidence notes for battle menu entry and attempted battle flow. [VERIFIED: .planning/ROADMAP.md; 04-CONTEXT.md]
2. Plan `04-02` as local XML and default/width analysis. It should preserve hashes/counts from this research, then add IDA/client proof for byte widths, default values, and repeated row requirements. [VERIFIED: local XML inventory; 04-CONTEXT.md]
3. Plan `04-03` as the design spec plus implementation gate. It should make a final Phase 5 input table and explicitly block runtime implementation if any BTEV requirement is incomplete without user approval. [VERIFIED: .planning/ROADMAP.md; 04-CONTEXT.md]
4. Keep all Phase 4 outputs under `.planning/phases/04-blue-battle-evidence-and-design/` or docs. Do not add committed battle loaders, generated battle JSON, EF migrations, or runtime battle behavior in Phase 4. [VERIFIED: 04-CONTEXT.md]
5. Use Green AI Battle only for negative/source-guard contrast. Do not use `GreenStageModeInterpreter`, Green AI Battle crown logic, or Green ghost token handling as Blue protocol truth. [VERIFIED: .planning/research/PITFALLS.md; 04-CONTEXT.md]
6. Treat XML roles as candidates until the client proves them. The row counts are verified; their runtime semantics are not. [VERIFIED: local XML inventory; 04-CONTEXT.md]

## Open Questions/Unknowns

These items are `RESOLVED FOR PLANNING`: the underlying evidence is not claimed as known, but each unknown is routed to a concrete Phase 4 plan output and gate/approval path.

| # | Unknown | Planning Status | Routed Plan | Gate / Approval Path |
|---|---------|-----------------|-------------|----------------------|
| 1 | What exact static client/IDA evidence satisfies BTEV-01 without cabinet/RPCS3 logs in Phase 4? [VERIFIED: 04-CONTEXT.md] | RESOLVED FOR PLANNING | `04-01` | `04-01-BATTLE-EVIDENCE.md` must record the equivalent client evidence source or mark the route claim `UNKNOWN - requires case-by-case user approval before Phase 5 relies on it`; `04-03` blocks or records a named exception before Phase 5. |
| 2 | Which endpoint sequence and request/response fields are required for battle menu entry? [VERIFIED: .planning/REQUIREMENTS.md] | RESOLVED FOR PLANNING | `04-01` | `04-01` route sequence must cover `initialdatacheck.php`, `battleuserdata.php`, and `playresult.php` with required/unknown status; `04-03` gate cannot approve Phase 5 unless BTEV-01/BTEV-02 are complete or user-approved. |
| 3 | What are the exact byte widths for `release_info_flg`, `release_battle_stage_flg`, `release_battle_special_flg`, `npc_costume_flg`, and `release_special_flg`? [VERIFIED: proto/blue/taiko.proto; 04-CONTEXT.md] | RESOLVED FOR PLANNING | `04-02` | `04-02` default/width matrix must prove each width or mark the row `UNKNOWN - requires case-by-case user approval before Phase 5 relies on it`; `04-03` requires one exception row per unproven required width. |
| 4 | Should new-user battleuserdata omit optional fields, send explicit zeros, or send first-stage defaults? [VERIFIED: proto/blue/taiko.proto; 04-CONTEXT.md] | RESOLVED FOR PLANNING | `04-02` | `04-02` records omission/default proof using generated presence semantics and client/IDA evidence where available; unproven optional defaults stay omission candidates until `04-03` approval. |
| 5 | How many `npc_data` and `ary_token_data` rows must be returned for safe client behavior? [VERIFIED: proto/blue/taiko.proto; local XML inventory] | RESOLVED FOR PLANNING | `04-02` | `04-02` records minimum safe row-count proof or an `UNKNOWN` row; `04-03` blocks Phase 5 or records per-row-count user approval. |
| 6 | How does stage id `33` in `battlestageinfo.xml` relate to the normal `1..10` stage chain? [VERIFIED: local XML inventory] | RESOLVED FOR PLANNING | `04-02` | `04-02` classifies stage id `33` as proven/candidate/unknown with source; `04-03` keeps Phase 5 blocked for any design that depends on id `33` without evidence or named approval. |
| 7 | Does `battle_bonds_lv_cap` need to be sent as `65`, omitted, or calculated from local data/client state? [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB; VERIFIED: local XML inventory] | RESOLVED FOR PLANNING | `04-02` | `04-02` must not infer `65`; it records proof or `UNKNOWN`, and `04-03` approves only proven behavior or a named exception. |
| 8 | What battle stage mode or play mode discriminates battle playresult from normal playresult in the Blue client, if any? [VERIFIED: proto/blue/taiko.proto; Application/Common/BluePlayResultMapping.cs] | RESOLVED FOR PLANNING | `04-03` | `04-03` design records how battle playresults are identified from 04-01/04-02 evidence; if unresolved, the gate status is `BLOCKED` or `APPROVED_WITH_USER_EXCEPTIONS` with a specific Phase 5 constraint. |
| 9 | Which `ReleaseBattleData` release IDs mirror to normal Blue unlock arrays, and which remain battle-owned only? [VERIFIED: proto/blue/taiko.proto; 04-CONTEXT.md] | RESOLVED FOR PLANNING | `04-03` | `04-03` Battle Playresult Effects and Phase 5 Ownership Map must list approved mirror paths; unresolved release IDs require individual exception rows before Phase 5 relies on them. |
| 10 | What do `battletokeninfo.xml` reward `type` values `0` and `1` mean in Blue battle reward handling? [VERIFIED: local XML inventory] | RESOLVED FOR PLANNING | `04-02` | `04-02` records candidate meaning/proof status for token reward types; `04-03` blocks token reward behavior or records named user approval for each unproven interpretation. |

## Environment Availability

| Dependency | Required By | Available | Version / Evidence | Fallback |
|------------|-------------|-----------|--------------------|----------|
| .NET SDK | Existing tests/build validation | yes | `10.0.201` from `dotnet --version`. [VERIFIED: shell probe] | none needed |
| PowerShell | Local XML inventory commands | yes | `5.1.26100.8491`. [VERIFIED: shell probe] | .NET test helper if planner prefers |
| `gsd-sdk` | Phase init/commit workflow | yes | `gsd-sdk v1.1.0`; `init.phase-op 04` succeeded. [VERIFIED: shell probe] | direct git/doc edits if needed |
| Local Blue battle data | BTEV-03 and BTEV-04 evidence | yes | `Host/wwwroot/data/blue/data` symlink target exists and all five battle XML files are present. [VERIFIED: local filesystem probe] | none; context says do not substitute committed fixtures |
| IDA/client artifact | BTEV-01/BTEV-04 static evidence | partial | `.tools/blue/EBOOT.ELF.i64` exists; `idat64.exe` and `ida.exe` were not on PATH. [VERIFIED: local filesystem probe; shell probe] | use existing artifacts or user-provided IDA/client notes |
| SQLite CLI | Possible local DB inspection if planner adds checks | yes | `sqlite3.exe` found under Miniconda. [VERIFIED: shell probe] | EF Core test fixtures |

**Missing dependencies with no fallback:**
- None for writing Phase 4 research/design. [VERIFIED: environment audit]

**Missing dependencies with fallback:**
- IDA executable is not on PATH; existing `.tools/blue/EBOOT.ELF.i64` and any user-provided client notes can still support static evidence, but fresh IDA automation may need user setup. [VERIFIED: shell probe]

## Security Domain

Security enforcement is enabled in `.planning/config.json`. [VERIFIED: .planning/config.json]

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | no | Phase 4 does not add auth/session flows. [VERIFIED: phase scope] |
| V3 Session Management | no | Phase 4 does not add session state. [VERIFIED: phase scope] |
| V4 Access Control | no | Phase 4 does not expose new admin/game endpoints. [VERIFIED: phase scope] |
| V5 Input Validation | yes | Treat XML fields, client evidence, and future byte widths as validated inputs; do not parse XML with ad hoc string slicing in future runtime work. [VERIFIED: local XML inventory; ASSUMED future parser guidance] |
| V6 Cryptography | no | Phase 4 does not add cryptography. [VERIFIED: phase scope] |
| V7 Error Handling and Logging | yes | Evidence capture should keep request context bounded and avoid dumping huge protobuf payloads or sensitive local paths beyond necessary provenance. [VERIFIED: .planning/research/PITFALLS.md] |

### Known Threat Patterns For This Phase

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Unsafe local XML assumptions become runtime behavior | Tampering | Require IDA/client proof and later structured XML parsing before Phase 5 loaders. [VERIFIED: 04-CONTEXT.md] |
| Cross-era battle logic leakage from Green | Tampering | Add source guards rejecting Green AI Battle dependencies in Blue battle runtime files. [VERIFIED: 04-CONTEXT.md] |
| Evidence logs expose excessive payload or local environment detail | Information Disclosure | Record bounded request summaries, hashes, and file paths needed for provenance only. [VERIFIED: .planning/research/PITFALLS.md] |

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Protobuf serialization/presence | Custom byte serializer or manual wire omission | Existing generated `Adapters.GameProtocol.Blue/Wire/Game.cs` plus protobuf-net presence helpers | Current routes already use generated protobuf-net wire types. [VERIFIED: codebase grep] |
| XML inventory | Regex/string slicing for XML structure | PowerShell/.NET XML parser for research; later structured C# XML parser if Phase 5 needs runtime loading | The files are nested Boost-serialization XML and contain repeated nested `item` arrays. [VERIFIED: local XML field scan] |
| Battle implementation defaults | Zero-filled arrays or one-row guesses | Field-by-field IDA/client proof and user-approved exceptions | Context forbids unsafe default stubs. [VERIFIED: 04-CONTEXT.md] |
| Blue battle design | Green AI Battle clone | Blue-owned design and source guards | Blue battle is not Green AI Battle. [VERIFIED: .planning/research/PITFALLS.md] |

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | XML file candidate roles are inferred from filenames and field names, not proven runtime semantics. | Data File Inventory | A Phase 5 loader could load the wrong file for the wrong response field. |
| A2 | A docs/source test such as `BlueBattleEvidenceTests.cs` may be useful for evidence completeness. | Validation Architecture | Planner may prefer manual review only; automated doc tests could be overkill. |
| A3 | Docs-only waves can use checklist validation before full test closeout. | Validation Architecture | If source changes sneak in, tests could be under-sampled. |
| A4 | Gate statuses `APPROVED`, `BLOCKED`, and `APPROVED_WITH_USER_EXCEPTIONS` are suggested names. | Validation Architecture | Planner may choose a different status vocabulary. |

## Sources

### Primary (HIGH confidence)
- `.planning/phases/04-blue-battle-evidence-and-design/04-CONTEXT.md` - locked decisions, deferred scope, and canonical references. [VERIFIED: codebase grep]
- `.planning/REQUIREMENTS.md` - BTEV and BTL requirements. [VERIFIED: codebase grep]
- `.planning/ROADMAP.md` - Phase 4/5 goals, plans, dependencies, and success criteria. [VERIFIED: codebase grep]
- `proto/blue/taiko.proto` - battle proto messages and field numbers. [VERIFIED: codebase grep]
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - generated wire fields and presence helpers. [VERIFIED: codebase grep]
- `Adapters.GameProtocol.Blue/Controllers/*`, `Adapters.GameProtocol.Blue/Mappers/*`, `Application/Handlers/*.Blue.cs`, and `Application/Dtos/*.Blue.cs` - current Blue runtime touchpoints. [VERIFIED: codebase grep]
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/*.xml` - local file inventory, counts, fields, and hashes. [VERIFIED: local XML inventory]

### Secondary (MEDIUM confidence)
- `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md` - original Track B design guidance and non-goals. [VERIFIED: codebase grep]
- `.planning/research/SUMMARY.md`, `.planning/research/ARCHITECTURE.md`, `.planning/research/PITFALLS.md` - prior GSD research framing. [VERIFIED: codebase grep]
- WikiWiki Blue Enso Battle page - gameplay semantics only. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB]

### Tertiary (LOW confidence)
- Candidate XML role interpretations inferred from local filenames/fields. [ASSUMED]

## Metadata

**Confidence breakdown:**
- Battle route/proto/wire map: HIGH - verified from source and generated wire files. [VERIFIED: codebase grep]
- Local XML inventory: HIGH for presence/counts/hashes, LOW for runtime semantics. [VERIFIED: local XML inventory]
- Gameplay score/crown separation: MEDIUM - supported by wiki and locked context, but wire/runtime mechanics still need client evidence. [CITED: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB; VERIFIED: 04-CONTEXT.md]
- Default/byte-width decisions: LOW - they are explicitly unknown until Phase 4 evidence tasks complete. [VERIFIED: 04-CONTEXT.md]
- Phase 5 gate shape: HIGH - directly locked in context and requirements. [VERIFIED: 04-CONTEXT.md; .planning/REQUIREMENTS.md]

**Research date:** 2026-05-30 [VERIFIED: current environment]
**Valid until:** 2026-06-06 for planning assumptions; refresh earlier if Blue client/IDA evidence changes. [ASSUMED]
