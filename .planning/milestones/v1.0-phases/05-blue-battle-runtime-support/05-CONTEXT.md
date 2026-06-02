# Phase 5: Blue Battle Runtime Support - Context

**Gathered:** 2026-05-30T18:21:05.3452238+08:00
**Status:** Ready for planning gate resolution; runtime implementation remains blocked until all Phase 4 missing-evidence rows are resolved.

<domain>
## Phase Boundary

This phase delivers Blue-owned battle runtime support for battle userdata, initial-data battle advertisement, battle playresult routing, battle-owned progression/rewards/unlocks, persistence, tests, and source guards.

Phase 5 begins by resolving the 26 `MISSING_EVIDENCE` rows from the Phase 4 design gate. Runtime behavior may not rely on any unresolved battle default, byte width, row count, route timing, stage progression rule, reward semantic, or unlock mirror. If any missing-evidence row remains unknown, planning and implementation for runtime behavior stays blocked until that row is resolved by concrete proof or a named user approval.

</domain>

<decisions>
## Implementation Decisions

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

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project and Phase Scope
- `.planning/PROJECT.md` - Full Blue support scope, strict evidence gate, Blue-owned state constraints, and current Phase 5 block.
- `.planning/REQUIREMENTS.md` - `BTL-*` runtime requirements and `BTEV-*` gate requirements carried from Phase 4.
- `.planning/ROADMAP.md` - Phase 5 goal, success criteria, and planned work breakdown.
- `.planning/research/ARCHITECTURE.md` - Blue-owned implementation shape and architecture constraints.
- `.planning/research/PITFALLS.md` - Battle pitfalls around Green AI Battle leakage, byte widths, unsafe defaults, and playresult ambiguity.

### Phase 4 Gate and Evidence
- `.planning/phases/04-blue-battle-evidence-and-design/04-CONTEXT.md` - Locked Phase 4 battle evidence and design decisions D-01 through D-21.
- `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` - Route, proto, generated-wire, and current-owner evidence for battle endpoints.
- `.planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md` - Local XML inventory, candidate relationships, and default/width proof matrix.
- `.planning/phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md` - Final `BLOCKED` gate status and the 26 `MISSING_EVIDENCE` rows Phase 5 must resolve.

### Gameplay Semantics
- `https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB` - User-approved gameplay/field-intent source for Blue Enso Battle. Use for gameplay semantics such as entry, titles/medals, bonds/best battle power, no normal score/crown recording, stage 10, bonds cap 65, specials, and reward concepts; do not use it alone to prove byte widths or required wire response shape.

### Blue Protocol and Runtime Touch Points
- `proto/blue/taiko.proto` - Blue `InitialdatacheckResponse`, `BattleUserDataResponse`, `PlayResultRequest.StageData.BattleStageData`, and `ReleaseBattleData` schema.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - Generated Blue wire types and optional-field presence helpers.
- `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs` - Current success-shaped battleuserdata stub to replace only after row resolution.
- `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs` - Current Blue initialdata path for future battle advertisement.
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` - Current Blue direct-protobuf playresult endpoint.
- `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs` - Existing optional Blue battle initialdata mapping pattern.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - Current mapper intentionally omits battle runtime semantics.
- `Application/Dtos/CommonPlayResultData.cs` - Existing common normal-stage DTO; Phase 5 should add Blue battle-owned DTO fields/partials rather than overloading normal stage data.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - Existing normal Blue playresult path that battle payloads must bypass for normal state writes.

### Local Battle Data
- `Host/wwwroot/data/blue/data/config/S10100-1/battle` - Local Blue battle data directory.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battleadjsetting.xml` - Candidate battle tuning/settings data.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battlenpcinfo.xml` - Candidate NPC data.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battlestageinfo.xml` - Candidate stage graph and boss data, including stage `33`.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battlesupportinfo.xml` - Candidate support/bonds/progression data.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battletokeninfo.xml` - Candidate token/reward data.

### Green Contrast Only
- `Application/Handlers/UpdatePlayResultCommand.Green.cs` - Green AI Battle contrast/source-guard material only.
- `Tests/Green/GreenAiBattlePlayResultTests.cs` - Green AI Battle contrast/source-guard material only.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.GameProtocol.Blue/Wire/Game.cs` exposes the generated battle schema and optional-field presence helpers needed for omission and exact-emission tests.
- `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs` already maps optional Blue battle initialdata fields only when application DTO values are present.
- `BattleUserDataController.cs` owns the Blue battleuserdata route but currently returns only `Result = 1`.
- `PlayResultController.cs`, `PlayResultMappers.cs`, and `UpdatePlayResultCommand.Blue.cs` show the current normal Blue playresult path that battle handling must branch away from.

### Established Patterns
- Blue behavior belongs in Blue-owned controllers, mappers, DTO partials, handler partials, entities, DbSets, EF mappings, migrations, tests, and source guards.
- Optional protobuf output should use generated presence semantics. Unresolved optional battle fields remain omitted rather than zero-filled.
- Local XML is operator/local game data and should become runtime catalog input only after role proof or named approval.
- Green AI Battle is useful for negative guards and risk discovery, not for Blue protocol truth.

### Integration Points
- Add a Phase 5 row-resolution artifact before runtime plans rely on any missing-evidence row.
- Add Blue battle application DTOs/partials for `BattleStageData` and `ReleaseBattleData` rather than overloading normal `StageData`.
- Add a battle branch in the Blue playresult mapper/handler that bypasses `SaveBlueStageAsync`, `UpsertBlueFavoriteAndRecentAsync`, `BlueProfileCounters.ApplyStage`, `UpsertBestAsync`, and `SaveBlueDanAsync`.
- Add Blue-owned battle persistence and catalog loaders only for resolved rows and resolved XML roles.

</code_context>

<specifics>
## Specific Ideas

- The first Phase 5 planning target should be a resolution matrix for the 26 Phase 4 `MISSING_EVIDENCE` rows.
- Wiki content is accepted as an evidence source for gameplay semantics and likely proto-field intent. In this discussion it was used to confirm battle is selected from 1P entry, titles/medals can be earned, bonds level and best battle power are recorded, normal crowns/scores are not recorded, and the mode has stage 10 / bonds level 65 concepts.
- Battle playresult mode values should be observed and recorded rather than normalized into existing normal Blue stage-mode filters.

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope.

</deferred>

---

*Phase: 5-Blue Battle Runtime Support*
*Context gathered: 2026-05-30T18:21:05.3452238+08:00*
