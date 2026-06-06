# Phase 10: Evidence-Backed Tokkun State Persistence and Readback - Context

**Gathered:** 2026-06-06T18:54:17.7207876+08:00
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase updates Blue Tokkun handling from safe acceptance to evidence-backed state persistence and readback. It should use the new runtime evidence that Tokkun playresults have `play_mode = 3`, persist the raw/protocol-backed Tokkun tutorial and summary facts, and read back only the proven userdata tutorial field.

This phase does not implement real Banacoin wallet/payment/receipt/coupon/history behavior, normal score/crown persistence, Dani, battle state, favorites/recent songs, shop state, rewards, unlocks, customization/title effects, or inferred Tokkun progression. It also does not claim final post-implementation cabinet/RPCS3 smoke proof; Phase 11 still owns that final proof.

</domain>

<decisions>
## Implementation Decisions

### Runtime Evidence And Classification
- **D-01:** Treat the user-reported live Tokkun session log showing `play_mode = 3` as proven runtime evidence. Phase 10 should add or plan `PlayMode.Tokkun = 3`; this is no longer a guessed value.
- **D-02:** `play_mode = 3` is the primary Tokkun classifier for Phase 10. `ary_tokkunstage_info` remains payload detail and summary evidence, but the old "numeric Tokkun mode unknown" guidance is superseded for this phase.
- **D-03:** The user also reported additional fields in the Tokkun session log. Planners must inspect the concrete log/proto field list before adding any extra persisted fields beyond named protocol-backed Tokkun facts. Do not store unnamed fields from memory.

### Tokkun Storage Shape
- **D-04:** Store Tokkun tutorial/readback state on `UserSaveData_Blue`, alongside existing Blue tutorial/profile flags. Store Tokkun summary/progress as separate Blue-owned history rows.
- **D-05:** Tokkun summary/history is append-only per classified upload. Do not dedupe by `banacoin_datetime` and do not keep latest-only state unless later evidence proves idempotency or replacement behavior.
- **D-06:** Persist `tookun_songno` with raw fidelity: preserve client order and duplicates if the client sends them. The exact representation is implementation discretion, but it must not normalize into unique songs.
- **D-07:** Store client-reported protocol timestamp data only, such as `play_datetime` and `banacoin_datetime` where available. Do not add a separate server `UploadedAtUtc` field in Phase 10.
- **D-08:** Interpret the roadmap "upload time" requirement as client-reported protocol time, not server-observed write time.

### Userdata Readback
- **D-09:** Before a user has a persisted Tokkun tutorial value, Blue userdata should continue to omit optional `tokkun_tutorial_flg`. Do not invent default `0` or `1` readback values.
- **D-10:** Update persisted `tokkun_tutorial_flg` only from Tokkun-classified uploads where the optional tutorial field is present. Tutorial-flag-only non-Tokkun uploads must not update this state.
- **D-11:** Store and return `tokkun_tutorial_flg` as a raw nullable `uint`. Do not normalize to bool and do not clamp to `0` or `1`.
- **D-12:** In Phase 10, protocol readback is limited to `UserDataResponse.tokkun_tutorial_flg`. Tokkun summary/history readback means durable server-side persistence and tests unless Phase 11 proves another client-facing response surface.

### Verification And Proof Boundary
- **D-13:** Phase 10 verification should include a focused automated suite: classifier/mapper tests, handler persistence tests, userdata readback tests, schema/migration reload tests, no-cross-write tests, and a Host build.
- **D-14:** The existing Tokkun session evidence is enough to unlock `PlayMode.Tokkun = 3` and result-shape planning. Phase 11 still owns final cabinet/RPCS3 smoke after Phase 10 implementation.
- **D-15:** Do not reintroduce Tokkun word-scan source guards. Safety proof should come from runtime behavior tests that execute Tokkun handling and assert only the allowed Tokkun state changes.

### the agent's Discretion
- Choose the exact history-row representation for `tookun_songno` as long as raw order and duplicates are preserved.
- Choose exact code shape, such as a `UpdatePlayResultCommand.BlueTokkun.cs` partial and/or `BlueTokkunStateExtensions`, as long as Blue-owned layering and existing partial-file patterns are preserved.
- Choose exact test file organization and migration name, provided the required proof in D-13 is covered.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project And Phase Scope
- `.planning/PROJECT.md` - Current v1.1 Blue Tokkun scope, evidence hierarchy, state separation constraints, and Phase 10 next goals.
- `.planning/REQUIREMENTS.md` - TKST-01 through TKST-04 for Phase 10, plus TKVF boundaries and out-of-scope Banacoin/progression behavior.
- `.planning/ROADMAP.md` - Phase 10 goal, success criteria, dependency on Phase 9, and Phase 11 final smoke boundary.
- `.planning/STATE.md` - Current workflow position and recent Phase 9 completion notes.

### Prior Tokkun Contracts And Summaries
- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md` - Earlier classifier/readback/persistence boundaries, now superseded only where this context locks `play_mode = 3`.
- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` - Row matrix for `tokkun_tutorial_flg`, `ary_tokkunstage_info`, `TokkunstageData`, and Phase 10 persistence candidates.
- `.planning/phases/08-stateless-banacoin-compatibility-and-availability/08-CONTEXT.md` - Stateless Banacoin boundary and no wallet/payment state decisions.
- `.planning/phases/09-tokkun-mapper-and-safe-playresult-acceptance/09-CONTEXT.md` - Phase 9 mapper/no-write decisions and raw Tokkun DTO surface.
- `.planning/phases/09-tokkun-mapper-and-safe-playresult-acceptance/09-01-SUMMARY.md` - Implemented Phase 9 files, verification, and handoff to Phase 10.
- `.planning/research/ARCHITECTURE.md` - Earlier Tokkun architecture research. Treat stale "play mode unknown" text as superseded by D-01/D-02 in this context.

### Codebase Maps
- `.planning/codebase/STACK.md` - Current .NET 10, ASP.NET Core, protobuf-net, EF Core, Mediator, Mapperly, and xUnit stack.
- `.planning/codebase/ARCHITECTURE.md` - Era-owned adapter/controller/mapper/Application layering, partial-file pattern, and cross-era state constraints.
- `.planning/codebase/INTEGRATIONS.md` - Blue `/v10r03/chassis/*` direct-protobuf route surface, local-only storage/logging model, and no outbound Banacoin integration.
- `.planning/codebase/CONVENTIONS.md` - Mapperly, partial-file, and local code style conventions.
- `.planning/codebase/TESTING.md` - xUnit and behavior-test patterns; use behavior tests rather than Tokkun source-word guards.

### Protocol And Runtime Touch Points
- `proto/blue/taiko.proto` - Blue schema source for `UserDataResponse.tokkun_tutorial_flg`, `PlayResultRequest.play_mode`, `PlayResultRequest.tokkun_tutorial_flg`, `PlayResultRequest.ary_tokkunstage_info`, and `TokkunstageData`.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - Generated Blue wire classes, optional presence helpers, `AryTokkunstageInfo`, and `TokkunstageData` field names. Do not manually edit generated wire output.
- `Domain/Enums/PlayMode.cs` - Add or plan `Tokkun = 3` from runtime evidence.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - Current Blue mapper preserving Tokkun fields and classifier state from Phase 9.
- `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` - Existing Blue Tokkun DTO fields for raw tutorial and summary facts.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - Current Tokkun log-and-success branch before battle/normal writes.
- `Application/Handlers/UserDataQuery.Blue.cs` - Blue userdata projection where persisted tutorial state should flow into common response data.
- `Application/Dtos/CommonUserDataResponse.Blue.cs` - Blue-specific common userdata DTO extension point.
- `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs` - Blue userdata wire mapper where optional `tokkun_tutorial_flg` should serialize only when persisted.

### Persistence And Tests
- `Domain/Entities/UserSaveDataBlue.cs` - Add raw nullable Tokkun tutorial state here per D-04 and D-11.
- `Application/Abstractions/ITaikoDbContext.Blue.cs` - Add any Blue Tokkun summary DbSet here.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs` - Configure Blue-owned Tokkun summary/history mappings and any `UserSaveData_Blue` column changes.
- `Tests/Blue/BlueMapperTests.cs` - Existing userdata mapper test currently asserts Tokkun tutorial omission; update to absence/presence behavior.
- `Tests/Blue/BluePlayResultMapperTests.cs` - Existing Tokkun mapper tests; add or update `play_mode = 3` classifier expectations.
- `Tests/Blue/BluePlayResultHandlerTests.cs` - Existing Tokkun no-write behavior tests; extend for allowed Tokkun persistence and forbidden cross-writes.
- `Tests/Blue/BlueUserDataTests.cs` - Blue userdata readback tests for persisted tutorial state.
- `Tests/Blue/BlueBattlePersistenceShapeTests.cs` - Persistence shape-test precedent for Blue-owned state and forbidden cross-storage references.
- `Tests/Blue/BlueBattlePersistenceTests.cs` - Migration/schema reload and no-cross-write precedent for Blue-owned mode state.

### Local Blue Evidence
- `.tools/blue/EBOOT.ELF.i64` - Local Blue IDA database if additional Tokkun fields need binary/client proof before storage.
- `.tools/blue/idadrv.py` - Shared local IDA driver workflow. Reuse the daemon before opening new Blue IDA work.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Application/Dtos/CommonPlayResultData.BlueTokkun.cs`: already exposes `IsTokkunPlayResult`, nullable `TokkunTutorialFlg`, and raw `TokkunStageDataDto`.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`: already maps `tokkun_tutorial_flg` optional presence/value and raw `TokkunstageData` facts.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`: already branches Tokkun before battle/normal writes, currently with log-and-success only.
- `UserSaveDataBlue`, `UserDataQuery.Blue.cs`, and `UserDataMappers.cs`: existing path for Blue tutorial/profile flags to flow into userdata.
- `TaikoDbContext.Blue.cs` and `ITaikoDbContext.Blue.cs`: established Blue-owned EF surface for new mode-specific state.
- Blue battle persistence tests: useful precedent for scoped migrations, schema reload, and forbidden cross-era/cross-mode storage assertions.

### Established Patterns
- Blue behavior stays Blue-owned in Blue adapter mappers/controllers, Blue DTO partials, Blue handler partials, Blue entities, Blue EF mappings, and Blue tests.
- Controllers deserialize, log, map, call Mediator, and map back. Business behavior stays in Application handlers.
- Generated `Wire/` files are not manually cleaned up unless regenerating protocol output.
- Protocol-backed raw facts can be persisted without inventing gameplay semantics.
- Behavior tests should execute real mapper/handler/userdata paths. Do not protect Tokkun with source-word scans.

### Integration Points
- Add `PlayMode.Tokkun = 3` and update Tokkun classifier tests/handler routing accordingly.
- Add `UserSaveDataBlue` nullable raw tutorial state and project it through `CommonUserDataResponse.Blue.cs` to `UserDataResponse.tokkun_tutorial_flg`.
- Add Blue-owned Tokkun summary/history persistence for raw Tokkun stage facts, preserving exact `tookun_songno` order and duplicates.
- Keep Tokkun summary/history separate from normal score/play, Dani, battle, favorite/recent, shop, unlock, reward, customization/title, and Banacoin storage.
- Update Phase 9 no-write tests so they allow only the new Tokkun tutorial/history state and still reject all unrelated writes.

</code_context>

<specifics>
## Specific Ideas

- During discussion, the user reported a played Tokkun session where the log shows `play_mode = 3` and additional fields. This supersedes the prior "Tokkun numeric play mode unknown" decision for Phase 10.
- A repo search during discussion did not find a committed log artifact for `play_mode = 3` in obvious paths. Treat the user-provided runtime observation as sufficient for `PlayMode.Tokkun = 3`, but require concrete log/proto inspection before storing any additional unnamed fields.
- The user selected `UserSaveData_Blue` for tutorial/readback state, not a separate Tokkun user-state table.
- The user selected client protocol time only. Do not add server-observed upload timestamps in Phase 10.

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope. Additional logged Tokkun fields are not a deferred feature; they are an evidence-inspection prerequisite before extending storage beyond named protocol-backed fields.

</deferred>

---

*Phase: 10-Evidence-Backed Tokkun State Persistence and Readback*
*Context gathered: 2026-06-06T18:54:17.7207876+08:00*
