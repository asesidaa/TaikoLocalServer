# Phase 04 Plan 04-03: Blue Battle Design Spec And Implementation Gate

## Scope And Guardrails

This document integrates the Phase 4 battle route/proto evidence from `04-01-BATTLE-EVIDENCE.md` and the local data/default inventory from `04-02-BATTLE-DATA-INVENTORY.md` into the Phase 5 design boundary. It is a design and gate artifact only. It does not implement battle runtime behavior, add loaders, add persistence, generate battle JSON, or replace current Blue battle stubs.

The evidence hierarchy remains locked by `04-CONTEXT.md`: D-01 allows static client/proto evidence for Phase 4; D-02 makes IDA/client behavior authoritative for wire mechanics; D-03 allows Wiki gameplay evidence for gameplay semantics; D-04 requires traceable evidence and explicit unknowns; D-05 requires case-by-case user approval before Phase 5 relies on an unproven field/default/row count.

Phase 5 runtime work is constrained by D-18 through D-21. D-18 requires an approved evidence pack and design spec before runtime implementation. D-19 requires Blue entities, Blue handlers, Blue mappers, Blue controllers, Blue migrations, Blue catalog types, tests, and source guards. D-20 defines the future full battle loop. D-21 requires automated proof, full tests, and a temp-output Host build during Phase 5, with RPCS3/cabinet battle smoke left for verification.

Green AI Battle is contrast/source-guard material only. It may identify risks to guard against, but it is not Blue protocol truth and must not be copied into Blue battle runtime plans.

## Evidence Inputs

| Input | Provides | Use In This Design |
|---|---|---|
| `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` | BTEV-01 and BTEV-02 route, proto, generated-wire, and current owner evidence. | Establishes candidate Phase 5 route surfaces: `initialdatacheck.php`, `battleuserdata.php`, and `playresult.php`; records `InitialdatacheckResponse`, `BattleUserDataResponse`, `BattleStageData`, and `ReleaseBattleData` ownership; preserves `UNKNOWN - requires case-by-case user approval before Phase 5 relies on it` rows for unproven runtime mechanics. |
| `.planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md` | BTEV-03 and BTEV-04 local XML inventory, hashes, row counts, candidate relationships, and default/width matrix. | Establishes the five local XML files as candidate data only, records unproven byte widths/defaults/repeated row counts, and routes every unsafe assumption to the gate instead of a runtime stub. |
| `04-CONTEXT.md` D-14 through D-21 | Locked playresult effects and Phase 5 boundary. | Defines battle-owned progress, normal Blue state protection, approved unlock mirrors, Blue-owned implementation surfaces, and verification obligations. |
| `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md` | Track B scope and non-goals. | Confirms Blue battle is a later Track B design family, separate from Green AI Battle and after normal Blue support. |
| `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` | Blue battle schema and protobuf presence helpers. | Define candidate fields and generated `ShouldSerialize*`/optional behavior, but do not prove safe runtime values by themselves per D-02 and D-11. |
| Current Blue mapper/handler/tests | Existing normal-mode behavior and no-inference guard. | Confirms `PlayResultMappers.Map` does not map battle sections today and `UpdatePlayResultCommand.Blue.cs` persists only normal Blue playresult state. |

## Battle Runtime Design Boundaries

Phase 5 may plan battle runtime implementation only inside a Blue-owned boundary and only after the final gate status allows the relevant evidence. The boundary is:

| Surface | Phase 5 Boundary | Design Constraint |
|---|---|---|
| `battleuserdata.php` | Replace the current read-only success-shaped stub with a Blue-owned query/mapper/controller flow only after response defaults, byte widths, and repeated row counts are proven or explicitly approved. | D-18 and D-19 apply. Do not infer safe `BattleUserDataResponse` values from local XML alone. |
| `initialdatacheck.php` battle flags | Add battle advertisement through Blue common DTO and Blue mapper presence handling only after `is_battleplay`, release battle flag widths, and `battle_bonds_lv_cap` behavior are proven or approved. | D-10, D-11, D-18, and D-19 apply. Omit unproven optional fields rather than zero-filling them. |
| `playresult.php` battle sections | Add a Blue battle playresult DTO/mapper/handler path that reads `BattleStageData` and `ReleaseBattleData` separately from normal Blue stage data. | D-14 through D-17 apply. Battle stages must not enter normal persistence paths by accident. |
| Local battle XML | Add Phase 5 catalog types/loaders only after the gate chooses proven or approved semantics for each file role. | D-06 through D-09 apply. XML row counts are local shape evidence, not runtime truth. |
| Green AI Battle references | Use only as negative/source-guard contrast. | D-19 applies. Green AI Battle source cannot define Blue stage modes, ghost data, crowns, tokens, or persistence semantics. |

## Battle Playresult Effects

D-14: Battle score/crown state is separate from normal Blue score/crown records. Battle stages must not update normal Blue self-best rows, normal Blue score rows, or normal Blue crowns. The current normal Blue path in `UpdatePlayResultCommand.Blue.cs` updates `SongPlayDataBlue`, `SongBestDataBlue`, Blue favorites/recent songs, Blue profile counters, and Dani; Phase 5 must route battle stages around those normal side effects unless a specific normal unlock mirror is approved.

D-15: Battle-owned progress includes stage/progression, bonds level, best battle power, NPC/token/special state, rewards, and related battle state. Phase 5 should model these as Blue battle state, not as Green ghost state and not as normal Blue song/crown state.

D-16: Existing normal release arrays may mirror to normal Blue unlock handling only when the design approves the specific release path. This means standard `release_song_no`, `get_tone_no`, `get_costume_no_1` through `get_costume_no_5`, and `get_title_no` can continue through normal Blue unlock handling when they are present as normal playresult release fields. `ReleaseBattleData.release_info_id`, `release_battle_stage_id`, `release_npc_id`, `release_npc_costume_id`, `release_npc_special_id`, battle token rows, and `assign_next_stage_id` are battle-owned unless the final gate or a later evidence-backed Phase 5 design approves a specific mirror.

D-17: Battle stages must not write normal Blue play history, recent/favorites, profile play counters, normal self-best, normal crowns, or Dani state. Battle stages need battle-owned history/progress instead. The future implementation must prove that battle payloads cannot pass through `SaveBlueStageAsync`, `UpsertBlueFavoriteAndRecentAsync`, `BlueProfileCounters.ApplyStage`, `UpsertBestAsync`, or `SaveBlueDanAsync` as normal stages.

## Phase 5 Ownership Map

| Ownership Area | Required Phase 5 Owner | Notes |
|---|---|---|
| Persistence | Blue entities, Blue DbSets, Blue EF configuration, and Blue migrations. | D-19 requires Blue-owned persistence. No Green ghost tables or normal Blue score tables may store battle-owned state. |
| Application handlers | Blue handlers and Blue partials for battle userdata, initial battle advertisement, and battle playresult persistence. | Do not place battle behavior in shared dispatch except for era routing. |
| Protocol mappers | Blue mappers for `BattleUserDataResponse`, `InitialdatacheckResponse` battle fields, `BattleStageData`, and `ReleaseBattleData`. | Generated wire fields remain adapter-local and map through application DTOs before persistence. |
| Controllers | Blue controllers for the existing `/v10r03/chassis/*` routes. | `BattleUserDataController` replacement is Phase 5 only after gate approval. |
| Catalog data | Blue catalog types and loaders for battle XML-derived data only after runtime role proof or named approval. | XML files stay candidate inputs until proof establishes use. |
| Tests | Blue tests for mapper presence/omission, byte widths, repeated row counts, persistence separation, unlock mirrors, and route ownership. | D-21 requires focused tests, full tests, source guards, and a temp-output Host build. |
| Source guards | Blue battle source guards preventing Green AI Battle dependencies. | Guard against `GreenAiBattle`, `GreenStageModeInterpreter`, Green ghost state, Green protocol byte constants, and Green battle tests as implementation truth. |

D-20 future loop, without implementing it in Phase 4: `battleuserdata.php`, `initialdatacheck.php` battle flags, battle playresult persistence, rewards/unlocks, progression, tests, and source guards.

## Phase 5 Test Obligations

Phase 5 tests must prove:

- Optional battle fields use generated presence semantics and unproven defaults remain omitted until approved, including `ShouldSerializeIsBattleplay`, `ShouldSerializeReleaseBattleStageFlg`, `ShouldSerializeReleaseBattleSpecialFlg`, `ShouldSerializeBattleBondsLvCap`, `ShouldSerializeReleaseInfoFlg`, nested `ShouldSerializeReleaseSpecialFlg`, and `ShouldSerializeAssignStageId`.
- Every emitted battle byte array has an exact approved width and default source, especially release info flags, battle stage flags, battle special flags, NPC costume flags, and special release flags.
- Required repeated rows have approved minimum safe counts, including `npc_data` and `ary_token_data`.
- Battle playresult persistence updates battle-owned progress and does not mutate normal Blue play history, recent/favorites, profile play counters, normal self-best, normal crowns, or Dani state.
- Unlock mirrors are covered one by one and cannot blanket-apply `ReleaseBattleData` to normal Blue unlocks.
- Source guards reject Green AI Battle implementation leakage.
- Full tests and a temp-output Host build run before Phase 5 closeout, per D-21.

## Multi-Source Coverage Audit

### GOAL Coverage

| GOAL | Phase 4 Output | Gate Result |
|---|---|---|
| Battle runtime implementation is unblocked by concrete evidence for battle routes, data files, byte widths, default state, playresult effects, and safe client behavior | `04-01-BATTLE-EVIDENCE.md` covers route/proto/generated-wire ownership; `04-02-BATTLE-DATA-INVENTORY.md` covers all five local XML files and the default/width proof matrix; this document covers battle playresult effects and the Phase 5 gate. | `BLOCKED_REQUIRED_EVIDENCE` for Phase 5 runtime work until the unresolved cases below are proven or approved one by one. |

### Requirement Coverage

| Requirement | Phase 4 Output | Coverage Status | Gate Consequence |
|---|---|---|---|
| BTEV-01 | `04-01-BATTLE-EVIDENCE.md` records route ownership for `initialdatacheck.php`, `battleuserdata.php`, and `playresult.php` plus equivalent client/static evidence. | Evidence artifact exists; exact menu-entry requirements and call timing remain `REQUIRED_UNKNOWN`. | Phase 5 may not rely on battle menu sequencing without proof or a named approval. |
| BTEV-02 | `04-01-BATTLE-EVIDENCE.md` maps `InitialdatacheckResponse`, `BattleUserDataResponse`, `BattleStageData`, and `ReleaseBattleData` to generated Blue wire types and owners. | Complete for proto/wire ownership. | Phase 5 still needs default/width approval before emitting values. |
| BTEV-03 | `04-02-BATTLE-DATA-INVENTORY.md` inventories `battleadjsetting.xml`, `battlenpcinfo.xml`, `battlestageinfo.xml`, `battlesupportinfo.xml`, and `battletokeninfo.xml`. | Complete for local file presence, hashes, row counts, and candidate roles. | XML roles remain candidate-only until IDA/client proof or approval. |
| BTEV-04 | `04-02-BATTLE-DATA-INVENTORY.md` lists default/width/row-count risks for battle flags, NPC state, token state, stage assignment, boss life, and last-stage behavior. | Incomplete for implementation because many rows remain `UNKNOWN - requires case-by-case user approval before Phase 5 relies on it`. | Phase 5 is blocked unless every listed unknown is proven or approved as a separate exception. |
| BTEV-05 | This document records D-14 through D-17 battle playresult effects and normal Blue state protection. | Complete for design decision. | Phase 5 tests must prove no battle write reaches normal scores, normal crowns, normal Blue play history, recent/favorites, profile play counters, normal self-best, or Dani. |
| BTEV-06 | This document defines fail-closed status rules and records the human gate decision. | Complete with `BLOCKED` decision. | Phase 5 runtime planning remains blocked until the missing evidence below is proven or separately approved. |

### Research Recommendation Coverage

| Research Recommendation | Phase 4 Output | Gate Status |
|---|---|---|
| 04-01 route/proto evidence capture | `04-01-BATTLE-EVIDENCE.md` records route sequence, current owners, proto/wire fields, and unknowns for route requirements. | Input present; route-default unknowns still need approval before use. |
| 04-02 XML/default-width analysis | `04-02-BATTLE-DATA-INVENTORY.md` records file hashes, row counts, candidate relationships, and the default/width proof matrix. | Input present; byte widths, defaults, and repeated row counts remain blocked where marked unknown. |
| 04-03 gate integration | This document maps evidence to design boundaries, test obligations, unresolved cases, and gate choices. | Task 04-03-T3 recorded a `BLOCKED` decision with one missing-evidence row per unresolved case. |

### CONTEXT Decision Coverage

| Decision | Output Mapping |
|---|---|
| D-01 | Phase 4 uses static client/proto evidence instead of requiring RPCS3/cabinet logs; later cabinet/RPCS3 smoke remains verification-stage work. |
| D-02 | Proto/wire/XML are candidate maps; IDA/client behavior remains authoritative for mechanics and defaults. |
| D-03 | Wiki gameplay semantics are used only for battle score/crown separation, not wire truth. |
| D-04 | 04-01, 04-02, and this document provide traceable route, wire, XML, playresult, and unknown matrices. |
| D-05 | Every unproven default/field/row count below is `REQUIRED_UNKNOWN` and requires case-by-case user approval. |
| D-06 | The local battle XML directory remains candidate input only. |
| D-07 | All five known battle XML files are inventoried by 04-02 and referenced by this gate. |
| D-08 | Phase 4 records row counts and key fields only; runtime JSON/loaders are forbidden targets. |
| D-09 | Local Blue battle data remains a prerequisite; no committed fixture substitution is allowed. |
| D-10 | Every battle byte-array field remains blocked until exact width/default proof or approval exists. |
| D-11 | Optional battle protobuf fields with unproven defaults must be omitted by generated presence semantics. |
| D-12 | Repeated NPC/token row counts cannot be guessed from XML row counts. |
| D-13 | Phase 5 test obligations include `ShouldSerialize`, exact widths, repeated-row counts, and source guards. |
| D-14 | Battle score/crown state is separate from normal Blue score/crown records; normal scores and normal crowns are protected. |
| D-15 | Battle stage/progression, bonds level, best battle power, NPC/token/special state, rewards, and related battle state are battle-owned progress. |
| D-16 | Normal release arrays may mirror to normal Blue unlock handling only for specifically approved release paths. |
| D-17 | Battle stages must not write normal Blue play history, recent/favorites, profile play counters, normal self-best, normal crowns, or Dani. |
| D-18 | Phase 5 runtime implementation requires this approved evidence/design gate. |
| D-19 | Phase 5 implementation must use Blue entities, Blue handlers, Blue mappers, Blue controllers, Blue migrations, catalog types, tests, and source guards. |
| D-20 | The future battle loop is listed without implementing it in Phase 4. |
| D-21 | Phase 5 closeout must include focused tests, mapper presence/width tests, source guards, full tests, and a temp-output Host build. |

## Phase 5 Gate Checklist

The gate fails closed. A missing prerequisite or unresolved `UNKNOWN` without a named user approval blocks Phase 5 runtime planning.

| Check | Required Evidence | Current Result | Consequence |
|---|---|---|---|
| 04-01 artifact exists | `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` | PASS | Route/proto evidence may be cited, but route-default unknowns remain blocked. |
| 04-02 artifact exists | `.planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md` | PASS | XML inventory may be cited, but candidate XML roles remain blocked. |
| BTEV-01 route/menu evidence | Equivalent client evidence and route sequence | PASS_WITH_REQUIRED_UNKNOWN | Phase 5 cannot assume menu-entry call timing or required fields. |
| BTEV-02 proto/wire ownership | Field-to-wire owner map | PASS | Phase 5 may use field ownership as input. |
| BTEV-03 local data inventory | All five XML files inventoried | PASS | Phase 5 may use file presence/counts as candidate evidence only. |
| BTEV-04 defaults/widths/row counts | Every battle default/width/row count proven or approved | FAIL_CLOSED | Phase 5 is blocked unless each `REQUIRED_UNKNOWN` row below receives proof or user approval. |
| BTEV-05 battle playresult effects | D-14 through D-17 design decision | PASS | Phase 5 must preserve normal Blue score/crown/history separation. |
| User approval coverage | One named approval per unresolved field/default/row count | FAIL_CLOSED | No blanket approval exists. |
| No runtime writes in Phase 4 | Forbidden target categories unchanged | PASS | This plan remains docs-only. |
| Final gate decision | Exactly one Task 04-03-T3 decision line | BLOCKED | Phase 5 remains blocked until every `MISSING_EVIDENCE` row below is proven or separately approved. |

## Unresolved Cases And User Approvals

Unresolved cases listed: 26. Named user approvals recorded: 0.

| # | Field / Default / Row Count | Missing Proof | Current Gate State | Required Before Phase 5 Relies On It |
|---:|---|---|---|---|
| 1 | Battle menu entry sequence and required `initialdatacheck.php` fields | Exact client requirement for battle menu entry and safe advertisement values. | REQUIRED_UNKNOWN | Client/IDA/log proof or one explicit user approval. |
| 2 | `battleuserdata.php` call timing and requirement | Whether the route is called before menu entry, after entry, or only during attempted flow. | REQUIRED_UNKNOWN | Client/IDA/log proof or one explicit user approval. |
| 3 | `InitialdatacheckResponse.is_battleplay` | Whether omission, explicit false, or explicit true is safe. | REQUIRED_UNKNOWN | Proof or approval for the exact emitted behavior. |
| 4 | `InitialdatacheckResponse.release_battle_stage_flg` | Exact byte width and default bits. | REQUIRED_UNKNOWN | Width/default proof or explicit exception. |
| 5 | `InitialdatacheckResponse.release_battle_special_flg` | Exact byte width and relation to NPC special flags. | REQUIRED_UNKNOWN | Width/default proof or explicit exception. |
| 6 | `InitialdatacheckResponse.battle_bonds_lv_cap` | Whether to omit, send `65`, or compute another value. | REQUIRED_UNKNOWN | Value-source proof or explicit exception. |
| 7 | `BattleUserDataResponse.release_info_flg` | Exact byte width and default. | REQUIRED_UNKNOWN | Width/default proof or explicit exception. |
| 8 | `BattleUserDataResponse.release_battle_stage_flg` | Exact byte width/default and relation to initial data. | REQUIRED_UNKNOWN | Width/default proof or explicit exception. |
| 9 | `BattleUserDataResponse.last_battle_stage_id` | New-user default, persisted value, and last-stage behavior. | REQUIRED_UNKNOWN | Default/progression proof or explicit exception. |
| 10 | `BattleUserDataResponse.last_boss_life` | New-user boss-life default and persistence source. | REQUIRED_UNKNOWN | Default/progression proof or explicit exception. |
| 11 | `BattleUserDataResponse.last_npc_id` | First/last NPC default semantics. | REQUIRED_UNKNOWN | Default proof or explicit exception. |
| 12 | `BattleUserDataResponse.npc_data` | Minimum safe repeated row count. | REQUIRED_UNKNOWN | Row-count proof or explicit exception. |
| 13 | `BattleUserNpcData.npc_costume_flg` | Exact byte width and default for each emitted NPC row. | REQUIRED_UNKNOWN | Width/default proof or explicit exception. |
| 14 | `BattleUserNpcData.release_special_flg` | Exact byte width and default. | REQUIRED_UNKNOWN | Width/default proof or explicit exception. |
| 15 | `BattleUserDataResponse.ary_token_data` | Minimum safe repeated token row count. | REQUIRED_UNKNOWN | Row-count proof or explicit exception. |
| 16 | `BattleUserDataResponse.assign_stage_id` | First-stage assignment default and source. | REQUIRED_UNKNOWN | Default/source proof or explicit exception. |
| 17 | `PlayResultRequest.StageData.BattleStageData` | How Blue identifies battle stages and prevents normal-stage persistence. | REQUIRED_UNKNOWN | Mapper/handler design proof or explicit exception. |
| 18 | `BattleStageData.npc_data` result fields | Persistence semantics for acquired exp, total exp, DPN, costume, specials, and bonds. | REQUIRED_UNKNOWN | Client/log proof or explicit exception. |
| 19 | `ReleaseBattleData.release_info_id` | Whether and how it mirrors normal Blue unlock handling. | REQUIRED_UNKNOWN | Specific mirror approval or battle-owned-only rule. |
| 20 | `ReleaseBattleData` stage/NPC/costume/special release arrays | Which releases are battle-owned and which may mirror normal unlocks. | REQUIRED_UNKNOWN | One decision per release path. |
| 21 | `ReleaseBattleData.ary_battletokendata` | Token row semantics and token value handling. | REQUIRED_UNKNOWN | Client/log proof or explicit exception. |
| 22 | `ReleaseBattleData.assign_next_stage_id` | How next-stage assignment updates persisted state. | REQUIRED_UNKNOWN | Transition proof or explicit exception. |
| 23 | `battlestageinfo.xml` stage id `33` | Runtime role, menu visibility, and last-stage behavior. | REQUIRED_UNKNOWN | Client/IDA proof or explicit exception. |
| 24 | `battletokeninfo.xml` reward `type` values `0` and `1` | Meaning of reward types and whether they apply unlock mirrors. | REQUIRED_UNKNOWN | Semantics proof or explicit exception. |
| 25 | Battle XML file menu-entry requirements | Which of the five XML files are required, optional, or unused for menu entry. | REQUIRED_UNKNOWN | Client/IDA proof or explicit exception per file role. |
| 26 | Boss-life and last-stage completion behavior | How boss-life zero/non-zero rows and last-stage state should be initialized and persisted. | REQUIRED_UNKNOWN | Client/log proof or explicit exception. |

## Gate Status

Final decision vocabulary is exactly `APPROVED`, `BLOCKED`, or `APPROVED_WITH_USER_EXCEPTIONS`.

Recommended Gate Status: BLOCKED

Rationale: BTEV-05 is now covered by the battle playresult effects design, but BTEV-06 cannot approve Phase 5 runtime planning while 26 required field/default/row-count/menu-entry cases remain `REQUIRED_UNKNOWN` and no named user approvals exist. `APPROVED` is invalid with the current document contents. `APPROVED_WITH_USER_EXCEPTIONS` is available only if the user approves each unresolved row separately and records the approved behavior, approval source, and Phase 5 constraint for every exception.

Final Gate Status: BLOCKED

## Missing Evidence Blocking Phase 5

| # | Missing Evidence Item | Missing Proof | Gate State | Phase 5 Constraint |
|---:|---|---|---|---|
| 1 | Battle menu entry sequence and required `initialdatacheck.php` fields | Exact client requirement for battle menu entry and safe advertisement values. | MISSING_EVIDENCE | Phase 5 cannot advertise battle menu entry until client/IDA/log proof or one explicit user approval exists. |
| 2 | `battleuserdata.php` call timing and requirement | Whether the route is called before menu entry, after entry, or only during attempted flow. | MISSING_EVIDENCE | Phase 5 cannot replace the stub based on assumed call timing. |
| 3 | `InitialdatacheckResponse.is_battleplay` | Whether omission, explicit false, or explicit true is safe. | MISSING_EVIDENCE | Phase 5 cannot emit the field without exact emitted-behavior proof or approval. |
| 4 | `InitialdatacheckResponse.release_battle_stage_flg` | Exact byte width and default bits. | MISSING_EVIDENCE | Phase 5 cannot set battle stage release flags without width/default proof or approval. |
| 5 | `InitialdatacheckResponse.release_battle_special_flg` | Exact byte width and relation to NPC special flags. | MISSING_EVIDENCE | Phase 5 cannot set battle special release flags without width/default proof or approval. |
| 6 | `InitialdatacheckResponse.battle_bonds_lv_cap` | Whether to omit, send `65`, or compute another value. | MISSING_EVIDENCE | Phase 5 cannot infer a bonds cap from XML counts alone. |
| 7 | `BattleUserDataResponse.release_info_flg` | Exact byte width and default. | MISSING_EVIDENCE | Phase 5 cannot emit release info flags without width/default proof or approval. |
| 8 | `BattleUserDataResponse.release_battle_stage_flg` | Exact byte width/default and relation to initial data. | MISSING_EVIDENCE | Phase 5 cannot emit battleuserdata stage flags without proof of width/default and relationship. |
| 9 | `BattleUserDataResponse.last_battle_stage_id` | New-user default, persisted value, and last-stage behavior. | MISSING_EVIDENCE | Phase 5 cannot set or persist last battle stage state from guessed defaults. |
| 10 | `BattleUserDataResponse.last_boss_life` | New-user boss-life default and persistence source. | MISSING_EVIDENCE | Phase 5 cannot initialize or persist boss life without default/progression proof. |
| 11 | `BattleUserDataResponse.last_npc_id` | First/last NPC default semantics. | MISSING_EVIDENCE | Phase 5 cannot set NPC identity defaults without proof or approval. |
| 12 | `BattleUserDataResponse.npc_data` | Minimum safe repeated row count. | MISSING_EVIDENCE | Phase 5 cannot emit NPC rows or rely on an empty list until row-count evidence exists. |
| 13 | `BattleUserNpcData.npc_costume_flg` | Exact byte width and default for each emitted NPC row. | MISSING_EVIDENCE | Phase 5 cannot serialize NPC rows until costume flag width/default is proven or approved. |
| 14 | `BattleUserNpcData.release_special_flg` | Exact byte width and default. | MISSING_EVIDENCE | Phase 5 cannot emit nested special release flags without width/default proof or approval. |
| 15 | `BattleUserDataResponse.ary_token_data` | Minimum safe repeated token row count. | MISSING_EVIDENCE | Phase 5 cannot emit token rows or rely on an empty list until row-count evidence exists. |
| 16 | `BattleUserDataResponse.assign_stage_id` | First-stage assignment default and source. | MISSING_EVIDENCE | Phase 5 cannot assign a starting stage from XML graph assumptions alone. |
| 17 | `PlayResultRequest.StageData.BattleStageData` | How Blue identifies battle stages and prevents normal-stage persistence. | MISSING_EVIDENCE | Phase 5 cannot route battle playresults until battle-stage identification and normal-state protection are proven. |
| 18 | `BattleStageData.npc_data` result fields | Persistence semantics for acquired exp, total exp, DPN, costume, specials, and bonds. | MISSING_EVIDENCE | Phase 5 cannot persist NPC progress from battle playresults without client/log proof or approval. |
| 19 | `ReleaseBattleData.release_info_id` | Whether and how it mirrors normal Blue unlock handling. | MISSING_EVIDENCE | Phase 5 must treat release info as battle-owned unless a specific mirror is proven or approved. |
| 20 | `ReleaseBattleData` stage/NPC/costume/special release arrays | Which releases are battle-owned and which may mirror normal unlocks. | MISSING_EVIDENCE | Phase 5 needs one evidence-backed decision per release path before applying unlock effects. |
| 21 | `ReleaseBattleData.ary_battletokendata` | Token row semantics and token value handling. | MISSING_EVIDENCE | Phase 5 cannot persist or grant token rewards without token semantics proof. |
| 22 | `ReleaseBattleData.assign_next_stage_id` | How next-stage assignment updates persisted state. | MISSING_EVIDENCE | Phase 5 cannot update assignment/progression state from this field without transition proof. |
| 23 | `battlestageinfo.xml` stage id `33` | Runtime role, menu visibility, and last-stage behavior. | MISSING_EVIDENCE | Phase 5 cannot route progression through stage `33` without client/IDA proof or approval. |
| 24 | `battletokeninfo.xml` reward `type` values `0` and `1` | Meaning of reward types and whether they apply unlock mirrors. | MISSING_EVIDENCE | Phase 5 cannot use reward type values for unlock or token behavior without semantic proof. |
| 25 | Battle XML file menu-entry requirements | Which of the five XML files are required, optional, or unused for menu entry. | MISSING_EVIDENCE | Phase 5 cannot treat any battle XML file as required or optional for menu entry without proof per file. |
| 26 | Boss-life and last-stage completion behavior | How boss-life zero/non-zero rows and last-stage state should be initialized and persisted. | MISSING_EVIDENCE | Phase 5 cannot implement completion or last-stage persistence without client/log proof or approval. |

## No Runtime Write Targets

Phase 4 must not write or modify these runtime targets:

| Forbidden Target | Reason |
|---|---|
| `Infrastructure/GameDataCatalog/Blue/*Battle*Loader.cs` | Loaders require approved XML roles and default semantics. |
| `Host/wwwroot/data/blue/*battle*.json` | Runtime battle JSON is deferred until Phase 5 evidence approval. |
| `Infrastructure/Persistence/Migrations/*Battle*.cs` | Battle persistence starts only in Phase 5. |
| `Domain/Entities/*Battle*.cs` | Battle entities are Phase 5 Blue-owned implementation work. |
| `Application/Handlers/*Battle*.cs` | Battle handlers are Phase 5 runtime work. |
| Battle edits to `UpdatePlayResultCommand.Blue.cs` | Phase 4 must not change normal Blue playresult behavior. |
| Runtime replacement of `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs` | The current stub is a Phase 4 read-only reference until the gate passes. |
