# Phase 9: Tokkun Mapper and Safe Playresult Acceptance - Context

**Gathered:** 2026-06-05T01:47:37.9701760+08:00
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase accepts Blue Tokkun `playresult.php` uploads from the existing Blue direct-protobuf route, maps protocol-backed Tokkun facts into common application DTOs, classifies Tokkun without a guessed `PlayMode.Tokkun` numeric value, returns success, and proves the Tokkun path does not contaminate normal, battle, Dani, favorite/recent, profile, unlock, medal, customization, title, shop, reward, or Banacoin state.

This phase does not add Tokkun persistence/readback entities, migrations, userdata readback behavior, real Banacoin behavior, derived Tokkun rewards, normal score/crown side effects, or cabinet/RPCS3 proof. Phase 10 owns persistence/readback. Phase 11 owns live cabinet/RPCS3 Tokkun proof and final contract tightening.

</domain>

<decisions>
## Implementation Decisions

### Classifier And Deterministic Client Contract
- **D-01:** Treat non-null `ary_tokkunstage_info` as the Phase 9 Tokkun classifier signal, carrying forward the Phase 7 contract. `tokkun_tutorial_flg` must not classify a payload as Tokkun by itself.
- **D-02:** Do not add `PlayMode.Tokkun`, do not assign a Tokkun numeric `play_mode`, and do not depend on a guessed play-mode value for classification. Raw `play_mode` may be preserved or logged only as context.
- **D-03:** Do not design an "ambiguous mixed payload" branch. The target is an existing deterministic legacy game with well-defined behavior. If a Tokkun payload includes normal-looking or battle-looking material by design, the server treats it as Tokkun and continues.
- **D-04:** Tokkun-classified uploads return success unless concrete client evidence proves a failure response is required.

### Mapper DTO Surface
- **D-05:** Map `tokkun_tutorial_flg` optional presence and value into the common DTO for Phase 10 readback planning, while keeping it out of the standalone classifier rule.
- **D-06:** Preserve all raw/protocol-backed `TokkunstageData` facts in `CommonPlayResultData`: `banacoin_datetime`, `tokkun_song_cnt`, `tookun_songno`, `tokkun_speedchange_cnt`, `tokkun_autoplay_cnt`, and `tokkun_jump_cnt`.
- **D-07:** Preserve the existing wire spelling when referencing `tookun_songno`; do not "correct" generated wire names or manually clean generated `Wire/` output.
- **D-08:** Tokkun DTO fields must remain raw facts. They must not infer payment, practice-time, ranking, reward, unlock, score, crown, favorite, recent-song, battle, or normal progression semantics.

### Safe Acceptance And No-Write Behavior
- **D-09:** Add a Blue Tokkun acceptance path before normal Blue save logic and before battle persistence can consume the payload. The Tokkun branch must not call normal or battle write helpers.
- **D-10:** Phase 9 should be log-and-success plus mapper/classifier preservation. It should not create EF entities, DbSets, migrations, AdminApi/WebUI surfaces, or persistent Tokkun records.
- **D-11:** Existing full Blue playresult request dumps are the primary visibility source. Do not add special speculative logging for "mixed" payload scenarios. A bounded classifier log is acceptable if useful for tests or operator debugging, but not required as a new evidence source.
- **D-12:** Behavior-based tests must prove a Tokkun upload leaves unrelated state unchanged. Source-text scans, Tokkun word bans, and allowlist-style guard tests must not be reintroduced.

### Verification Expectations
- **D-13:** Mapper tests should prove Tokkun classifier state, `tokkun_tutorial_flg` presence/value, and every raw `TokkunstageData` field are preserved.
- **D-14:** Handler tests should prove Tokkun uploads return success for existing and unknown users according to current Blue playresult conventions, while leaving normal score/best, Dani, battle, favorite/recent, profile counters, unlock flags, medal totals, customization/title, shop, and Banacoin-like state untouched.
- **D-15:** Phase 9 verification is automated source/test/build evidence only. Do not claim cabinet/RPCS3 Tokkun proof here.

### the agent's Discretion
- Choose the exact code shape that best fits existing patterns, such as `CommonPlayResultData.BlueTokkun.cs`, a Blue playresult kind/classifier helper, or a focused `HandleBlueTokkun` partial.
- Choose the exact focused test files and fixture construction, provided the tests exercise real mapper and handler behavior rather than source scans.
- Add only minimal logging needed to make the Tokkun branch observable; the full request dump already exists at the controller boundary.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project And Phase Scope
- `.planning/PROJECT.md` - Current v1.1 Blue Tokkun scope, evidence hierarchy, state separation constraints, and Phase 9 next goals.
- `.planning/REQUIREMENTS.md` - TKPR-01 through TKPR-03 for Phase 9, plus Phase 10/11 boundaries and out-of-scope Banacoin/progression behavior.
- `.planning/ROADMAP.md` - Phase 9 goal, success criteria, dependencies, and the split between Phase 9 acceptance, Phase 10 persistence, and Phase 11 cabinet proof.
- `.planning/STATE.md` - Current workflow position and recent Phase 8 completion notes.

### Prior Tokkun And Banacoin Contracts
- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md` - Locked classifier boundary, no guessed Tokkun enum, behavior-based guard reset, and Phase 9 handoff.
- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` - Row matrix for `tokkun_tutorial_flg`, `ary_tokkunstage_info`, `TokkunstageData`, `PlayMode.Tokkun`, no-runtime-write targets, and Phase 9 gate.
- `.planning/phases/08-stateless-banacoin-compatibility-and-availability/08-CONTEXT.md` - Stateless Banacoin compatibility boundary and no wallet/payment state decisions.
- `.planning/phases/08-stateless-banacoin-compatibility-and-availability/08-01-SUMMARY.md` - Implemented `getbanacoininfo.php` route availability, optional-field omission, and Phase 11 live-proof handoff.
- `.planning/research/SUMMARY.md` - v1.1 research summary and Phase 9 architecture warning about Tokkun falling through normal Blue playresult handling.

### Codebase Maps
- `.planning/codebase/STACK.md` - Current .NET 10, ASP.NET Core, protobuf-net, EF Core, Mediator, Mapperly, and xUnit stack.
- `.planning/codebase/ARCHITECTURE.md` - Era-owned adapter/controller/mapper/Application layering, partial-file pattern, and cross-era state constraints.
- `.planning/codebase/INTEGRATIONS.md` - Blue `/v10r03/chassis/*` direct-protobuf route surface, local-only storage/logging model, and no outbound Banacoin integration.
- `.planning/codebase/CONVENTIONS.md` - Mapperly, partial-file, and local code style conventions for downstream planners.
- `.planning/codebase/TESTING.md` - xUnit and behavior-test patterns; use behavior tests rather than Tokkun source-word guards.

### Blue Protocol And Runtime Touch Points
- `proto/blue/taiko.proto` - Blue schema source for `PlayResultRequest.tokkun_tutorial_flg`, `PlayResultRequest.ary_tokkunstage_info`, and `TokkunstageData` fields.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - Generated Blue wire classes, optional presence helpers, `AryTokkunstageInfo`, and `TokkunstageData` field names.
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` - Existing direct-protobuf request logging, mapper dispatch, Mediator call, and success response mapping.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - Current Blue playresult mapper where Tokkun classifier facts and raw DTO fields should be preserved.
- `Application/Dtos/CommonPlayResultData.cs` - Shared common playresult DTO base; keep era-specific Tokkun fields in Blue-specific partials.
- `Application/Dtos/CommonPlayResultData.BlueBattle.cs` - Existing Blue battle DTO partial pattern for an era-specific playresult branch.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - Current normal Blue playresult save path that Tokkun must bypass.
- `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs` - Existing battle branch and allowed battle side effects, used as contrast for Tokkun no-write behavior.
- `Domain/Enums/PlayMode.cs` - Current known play modes. Do not add `Tokkun` until Phase 11 or later proves the numeric value.

### Focused Test Surfaces
- `Tests/Blue/BluePlayResultMapperTests.cs` - Existing Blue mapper tests to extend for Tokkun field preservation.
- `Tests/Blue/BluePlayResultHandlerTests.cs` - Existing normal Blue handler tests and fixture patterns.
- `Tests/Blue/BlueBattlePlayResultMapperTests.cs` - Existing battle classifier/mapper tests and raw client value preservation precedent.
- `Tests/Blue/BlueBattlePlayResultHandlerTests.cs` - Existing no-normal-state contamination tests for battle, useful as contrast but not as Tokkun semantics.
- `Tests/Blue/BlueA4SourceGuardTests.cs` - Current Green-leakage guard only. Do not add Tokkun word scans here.

### Local Blue Evidence
- `.tools/blue/EBOOT.ELF.i64` - Local Blue IDA database for future client evidence if Phase 9 planning uncovers a blocked protocol question.
- `.tools/blue/idadrv.py` - Shared local IDA driver workflow. Check/reuse the daemon before opening new Blue IDA work.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`: already logs full Blue playresult request dumps, maps wire DTOs, sends `UpdatePlayResultCommand`, and maps `Result = 1` responses.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`: natural place to map `TokkunTutorialFlg` optional presence/value, `AryTokkunstageInfo`, raw `TokkunstageData` facts, and classifier state.
- `Application/Dtos/CommonPlayResultData.BlueBattle.cs`: established pattern for adding Blue-specific playresult DTO fields without polluting other eras.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`: decisive branch point where Tokkun must be detected before normal save, medal, profile, unlock, favorite/recent, and Dani writes.
- `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`: contrast path showing how branch-specific handling bypasses normal score/crown writes; Tokkun should be stricter and avoid battle/shop/recent side effects too.
- `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`: reusable fixture style for proving branch-specific no-contamination behavior.

### Established Patterns
- Blue behavior stays Blue-owned in Blue adapter mappers/controllers, Blue DTO partials, Blue handler partials, Blue entities only when persistence is in scope, and Blue tests.
- Controllers deserialize, log, map, send Mediator, and map responses. Business behavior stays in Application handlers.
- Generated `Wire/` files are not manually edited.
- Protocol-backed raw facts can be preserved without inventing gameplay semantics.
- No source-scanning Tokkun guard should be added; safety is runtime control flow plus behavior tests.

### Integration Points
- Add Blue Tokkun DTO fields in an era-specific `CommonPlayResultData` partial rather than the shared base file unless a shared field already exists.
- Extend `PlayResultMappers.Map` to preserve Tokkun optional presence/value and raw `TokkunstageData` facts.
- Add Tokkun classification before the existing battle/normal save paths in `HandleBlue`.
- Add a Tokkun handler path that returns success and avoids EF writes outside whatever EF tracking is already necessary to validate current Blue user conventions.
- Add mapper and handler tests that exercise the real mapper and handler branch rather than inspecting source text.

</code_context>

<specifics>
## Specific Ideas

- The user explicitly corrected the mixed-payload framing: do not design around speculative ambiguity. The game is deterministic legacy software; if Tokkun carries other-looking fields by design, the server treats it as Tokkun and continues.
- The existing full Blue playresult dump is enough visibility for raw payload context. Do not invent a separate mixed-payload evidence model.
- Preserve all raw Tokkun facts now so Phase 10 can decide persistence/readback from protocol-backed data without revisiting the mapper boundary.

</specifics>

<deferred>
## Deferred Ideas

None. Discussion stayed within phase scope. Phase 10 persistence/readback and Phase 11 cabinet/RPCS3 proof remain roadmap-owned follow-on work, not new deferred ideas.

</deferred>

---

*Phase: 9-Tokkun Mapper and Safe Playresult Acceptance*
*Context gathered: 2026-06-05T01:47:37.9701760+08:00*
