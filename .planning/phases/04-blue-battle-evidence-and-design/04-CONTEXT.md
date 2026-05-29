# Phase 4: Blue Battle Evidence And Design - Context

**Gathered:** 2026-05-30T03:20:08.4800704+08:00
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase gathers and records the evidence needed to design Blue battle mode before runtime implementation. It must map the Blue battle protocol, local battle data files, safe defaults, byte widths, repeated row requirements, battle playresult effects, and Phase 5 implementation gate.

This phase does not implement battle runtime behavior. It does not require RPCS3/cabinet battle logs; those are reserved for verification-stage proof. It may use IDA/client evidence, proto/wire contracts, local Blue battle XML, and gameplay Wiki evidence to write the design and identify unresolved cases.

</domain>

<decisions>
## Implementation Decisions

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

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project and Phase Scope
- `.planning/PROJECT.md` - Full Blue scope, strict evidence gate, normal support before battle runtime, and Blue-owned state constraints.
- `.planning/REQUIREMENTS.md` - `BTEV-*` requirements for Phase 4 and `BTL-*` requirements that Phase 4 gates for Phase 5.
- `.planning/ROADMAP.md` - Phase 4 and Phase 5 goals, success criteria, and planned work breakdown.
- `.planning/research/SUMMARY.md` - Research summary identifying Track B as under-specified until battle evidence/design.
- `.planning/research/ARCHITECTURE.md` - Track B architecture constraints and Blue-owned implementation shape.
- `.planning/research/PITFALLS.md` - Battle pitfalls around Green AI Battle leakage, byte widths, unsafe defaults, and playresult ambiguity.
- `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md` - Original Blue roadmap and Track B battle design guidance.

### Gameplay Semantics
- `https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB` - Blue Enso Battle gameplay reference; important source for battle score/crown separation, bonds/best battle power, rewards, stages, and specials.

### Blue Protocol and Runtime Touch Points
- `proto/blue/taiko.proto` - Blue `InitialdatacheckResponse` battle fields, `BattleUserData*`, `PlayResultRequest.StageData.BattleStageData`, and `ReleaseBattleData` wire messages.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - Generated Blue wire types and optional-field presence helpers for battle fields.
- `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs` - Current Blue battleuserdata stub; Phase 5 replaces this only after the design gate.
- `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs` - Current Blue initialdata controller path where battle flags may be added after proof.
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` - Current direct-protobuf Blue playresult endpoint.
- `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs` - Existing mapping for optional Blue battle initialdata fields.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - Current mapper intentionally does not infer battle runtime semantics.
- `Application/Dtos/CommonInitialDataCheckResponse.Blue.cs` - Existing Blue-only initialdata fields for battle flags.
- `Application/Handlers/GetInitialDataQuery.Blue.cs` - Existing Blue initialdata behavior to extend after battle design.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - Existing normal Blue playresult path; battle stages must not corrupt normal score/crown/history state.

### Local Battle Data
- `Host/wwwroot/data/blue/data/config/S10100-1/battle` - Local required Blue battle data directory for Phase 4/5 work.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battleadjsetting.xml` - Candidate battle tuning/settings data.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battlenpcinfo.xml` - Candidate NPC data.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battlestageinfo.xml` - Candidate stage data.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battlesupportinfo.xml` - Candidate support/bonds/progression data.
- `Host/wwwroot/data/blue/data/config/S10100-1/battle/battletokeninfo.xml` - Candidate token data.

### Tests and Guard References
- `Tests/Blue/BluePlayResultMapperTests.cs` - Existing guard that Blue playresult mapping does not infer unimplemented battle semantics.
- `Tests/Blue/BlueRouteSkeletonTests.cs` - Existing Blue route ownership and mediator-backed endpoint guard.
- `Tests/Blue/BlueDocsTests.cs` - Existing docs guard referencing Blue battle deferral and local data layout.
- `Application/Handlers/UpdatePlayResultCommand.Green.cs` - Green AI Battle contrast only; do not use as Blue protocol truth.
- `Tests/Green/GreenAiBattlePlayResultTests.cs` - Green AI Battle contrast only; useful for source guards, not Blue semantics.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.GameProtocol.Blue/Wire/Game.cs` already exposes generated battle fields and `ShouldSerialize*` methods for optional protobuf presence tests.
- `CommonInitialDataCheckResponse.Blue.cs` and `InitialDataMappers.cs` already carry optional Blue battle initialdata fields.
- `BattleUserDataController.cs` already owns the Blue route but is still a success-shaped stub.
- `PlayResultMappers.cs` already preserves normal direct-protobuf fields and avoids inferring battle semantics from optional battle sections.
- `BlueProtocolBytes` provides proven Blue fixed-width patterns for non-battle arrays; battle widths still require proof.

### Established Patterns
- Blue behavior belongs in `.Blue.cs` partial handlers and Blue adapter mappers, not shared cross-era files.
- Era-owned persistence should use Blue entities, Blue DbSets, Blue EF mappings, and Blue migrations.
- Optional protobuf fields should use generated presence helpers so omitted fields remain omitted.
- Source guards are already used to prevent unimplemented/stub routes from quietly calling Mediator and to prevent cross-era leakage.

### Integration Points
- Phase 4 research/design should inspect `proto/blue/taiko.proto`, generated wire types, local battle XML, Wiki gameplay evidence, and IDA/client behavior.
- Phase 5 implementation will likely connect through `BattleUserDataController`, `InitialDataCheckController`, `PlayResultController`, Blue mappers, `GetInitialDataQuery.Blue.cs`, and `UpdatePlayResultCommand.Blue.cs`.
- Phase 5 must add Blue-owned persistence and catalog/data foundations only after Phase 4 proves field widths, defaults, required rows, and reward/state effects.

</code_context>

<specifics>
## Specific Ideas

- Wiki gameplay evidence is accepted as important truth for gameplay semantics. Use IDA/client evidence for wire mechanics and runtime safety, but do not do heavy IDA work for every gameplay rule when the Wiki is clear enough.
- The linked Wiki says battle records bonds and best battle power separately from normal score/crown records, and that titles/medals can still be earned through battle. Phase 5 should reflect that split.
- Runtime JSON/loaders should wait for Phase 5; Phase 4 inventory is documentation and evidence only.

</specifics>

<deferred>
## Deferred Ideas

- RPCS3/cabinet battle logs are deferred to verification-stage proof rather than Phase 4 design evidence.
- Runtime Blue battle JSON/default data generation is deferred until Phase 5, after the Phase 4 design gate.

</deferred>

---

*Phase: 4-Blue Battle Evidence And Design*
*Context gathered: 2026-05-30T03:20:08.4800704+08:00*
