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
