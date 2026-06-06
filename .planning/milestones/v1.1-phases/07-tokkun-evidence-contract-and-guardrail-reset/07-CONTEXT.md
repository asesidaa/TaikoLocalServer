# Phase 7: Tokkun Evidence Contract and Guardrail Reset - Context

**Gathered:** 2026-06-03T22:29:15.4478436+08:00
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase establishes the bounded Blue Tokkun protocol contract before runtime behavior changes. It must produce a battle-style evidence matrix for Tokkun fields, routes, and old guard decisions; document classifier inputs without assigning a guessed numeric `PlayMode.Tokkun` value; and reset the stale no-Tokkun source-guard posture so future phases implement Tokkun through real runtime control flow instead of word scans.

This phase does not implement Banacoin compatibility, playresult persistence, Tokkun readback persistence, or cabinet/RPCS3 verification. Those belong to Phases 8 through 11.

</domain>

<decisions>
## Implementation Decisions

### Evidence Ledger Shape
- **D-01:** Phase 7 should use a battle-style row-by-row evidence matrix, following the proven Phase 4/5 battle workflow pattern rather than a loose narrative-only document.
- **D-02:** The Tokkun matrix should classify rows with four statuses: proven, observed, deliberately ignored, and unknown/blocked.
- **D-03:** A Tokkun row can be marked proven from local proto/wire/current-code evidence plus Blue IDA/client evidence. Cabinet/RPCS3 proof is reserved for final verification unless a specific row cannot be resolved without it.
- **D-04:** The matrix should enumerate protocol fields, endpoint/route questions, and guard reset decisions together so follow-on planners get one contract.

### Tokkun Classifier Boundary
- **D-05:** `ary_tokkunstage_info` is the likely Tokkun upload signal and should be the primary classifier input until stronger client evidence proves a different rule.
- **D-06:** `tokkun_tutorial_flg` is tutorial-state/readback evidence, not a reliable standalone playresult classifier. It should not by itself classify a payload as Tokkun.
- **D-07:** Do not add `PlayMode.Tokkun` or assign a Tokkun numeric play mode until RPCS3/cabinet logs or deeper IDA evidence proves the value.
- **D-08:** If a payload is classified as Tokkun mode, be lenient and ignore other-mode material rather than failing or writing normal/battle state. One credit has one mode, and Tokkun should not save scores.
- **D-09:** Downstream classifier output may expose an `IsTokkun` branch plus protocol-backed Tokkun facts, including Tokkun-stage summary fields, tutorial-state presence, and ignored-other-mode indicators. It must not expose a guessed numeric Tokkun play-mode enum.

### Guard Reset
- **D-10:** Remove the old Tokkun-term source guard. It was not a reasonable guardrail and should not be replaced with a named word allowlist.
- **D-11:** Do not add new Tokkun-specific source-scanning guardrail tests. The correct Tokkun implementation should skip normal, battle, crown, score, and other unrelated routines through runtime control flow.
- **D-12:** Proof that Tokkun does not save normal or battle state should come from real behavior: implementation structure plus focused checks that execute actual Tokkun handling, not source-text scans.
- **D-13:** Interpret TKEV-03 as a reset: delete stale source guards and prevent side effects through runtime control flow. Do not plan replacement word-scan guardrails.

### Follow-on Handoff Surface
- **D-14:** Phase 8 should receive Banacoin-adjacent route unknowns and evidence gates, especially around `getbanacoininfo.php`. Phase 7 should not invent Banacoin response semantics.
- **D-15:** Phase 9 should receive the classifier contract: exact classifier inputs, the lenient ignored-other-mode rule, logging boundaries, and the no normal/battle write rule.
- **D-16:** Phase 10 may consider only protocol-backed Tokkun facts for persistence: tutorial state, Tokkun-stage summary fields, song count/list, speed-change count, autoplay count, jump count, timestamps, and upload time.
- **D-17:** Phase 11 must prove v1.1 with cabinet/RPCS3 Tokkun selection/upload/readback evidence plus real behavior checks that Tokkun does not write unrelated state.

### the agent's Discretion
None. The user made explicit decisions for all selected gray areas.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project and Phase Scope
- `.planning/PROJECT.md` - Current v1.1 Blue Tokkun scope, evidence hierarchy, Blue-owned state constraints, and superseding decision to reopen Tokkun despite earlier non-goal assumptions.
- `.planning/REQUIREMENTS.md` - v1.1 requirements TKEV-01 through TKEV-03 and follow-on Tokkun requirements for Phases 8 through 11.
- `.planning/ROADMAP.md` - Phase 7 goal, success criteria, and relationship to Phases 8 through 11.
- `.planning/STATE.md` - Current workflow position and recent milestone decisions.

### Prior Evidence-Gated Patterns
- `.planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-CONTEXT.md` - Battle evidence/design precedent: proof hierarchy, unknown handling, row-specific gates, and Blue-owned runtime boundary.
- `.planning/milestones/v1.0-phases/05-blue-battle-runtime-support/05-CONTEXT.md` - Battle runtime precedent: row-by-row resolution matrix before runtime use and no normal-state contamination.
- `.planning/milestones/v1.0-phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md` - Banacoin/payment boundary precedent: Blue `rewardexecution.php` and payment-looking behavior stay log/success or no-op unless evidence proves otherwise.
- `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md` - Historical Blue roadmap. Its Tokkun-out-of-scope assumption is stale for v1.1, but it records why old no-Tokkun guards existed.

### Blue Protocol and Runtime Touch Points
- `proto/blue/taiko.proto` - Blue protocol schema including `UserDataResponse.tokkun_tutorial_flg`, `PlayResultRequest.tokkun_tutorial_flg`, `PlayResultRequest.ary_tokkunstage_info`, `TokkunstageData`, Banacoin messages, and Blue battle fields for contrast.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - Generated Blue wire types and optional-field presence helpers for Tokkun, Banacoin, playresult, userdata, and battle sections.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - Current Blue playresult mapping path where Tokkun classifier facts will be mapped later.
- `Application/Dtos/CommonPlayResultData.cs` - Existing common playresult DTO shape. Tokkun additions should remain protocol-backed and avoid overloading normal score semantics.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - Current normal Blue playresult path that Tokkun-classified payloads must skip for score/crown/Dani/profile/favorite/recent/unlock writes.
- `Domain/Enums/PlayMode.cs` - Current known play modes. Do not add `Tokkun` until numeric evidence exists.

### Routes and Banacoin Surface
- `Tests/Blue/BlueRouteSkeletonTests.cs` - Current route ownership assertions, including the excluded `getbanacoininfo.php` route.
- `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs` - Current stateless Blue `banacoinpayment.php` log/success shape.
- `Adapters.GameProtocol.Blue/Controllers/BanacoinErrorLogController.cs` - Current Blue `banacoinerrorlog.php` log/success shape.

### Guard Reset and Verification References
- `Tests/Blue/BlueA4SourceGuardTests.cs` - Contains the stale Tokkun-term source guard that Phase 7 should remove or replace with non-source-scan behavior proof.
- `Tests/Blue/BlueBattleSourceGuardTests.cs` - Battle source-guard precedent. Use only as history; do not copy its word-scan pattern into Tokkun.
- `.planning/codebase/ARCHITECTURE.md` - Layering, Blue-owned era dispatch, adapter mapping, and persistence boundaries.
- `.planning/codebase/CONVENTIONS.md` - Partial-file and Mapperly conventions.
- `.planning/codebase/TESTING.md` - Real behavior/integration test patterns and source-guard caveats.

### Local Blue Evidence
- `.tools/blue/EBOOT.ELF.i64` - Local Blue IDA database for client evidence. Use through the local IDA workflow/tooling when resolving Tokkun runtime mechanics.
- `.tools/blue/idadrv.py` - Local IDA driver support material.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` already expose Tokkun fields and optional-field presence helpers.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` is the natural mapper integration point for an `IsTokkun` branch and protocol-backed Tokkun facts.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` and `UpdatePlayResultCommand.BlueBattle.cs` show the current branch points that Tokkun must bypass instead of writing normal or battle state.
- Existing Blue Banacoin controllers already provide stateless log/success behavior for two routes; Phase 8 decides additional route availability only with evidence.

### Established Patterns
- Blue behavior stays Blue-owned in Blue adapter mappers/controllers, Blue DTO partials, Blue handler partials, Blue entities, Blue EF mappings, and Blue tests.
- Generated protobuf DTOs map through `Application/Dtos/Common*` shapes before handler logic.
- Protocol fields with unproven semantics should be carried as raw/protocol-backed facts or ignored, not promoted into invented gameplay behavior.
- Source-text scans are not the desired Tokkun guard mechanism. Tokkun safety should be achieved by control-flow separation and behavior that exercises the actual runtime branch.

### Integration Points
- Add a Phase 7 Tokkun contract artifact with the evidence matrix and status taxonomy.
- Update or remove stale source-guard tests that prohibit Tokkun terms in Blue playresult code.
- Prepare mapper/DTO contract notes for Phase 9: `ary_tokkunstage_info` is the classifier signal; `tokkun_tutorial_flg` is state/readback; Tokkun mode ignores other-mode material.
- Prepare Phase 8 route unknowns around Banacoin-adjacent endpoints without adding wallet/payment state.

</code_context>

<specifics>
## Specific Ideas

- The user clarified that `ary_tokkunstage_info` is likely involved in Tokkun uploads, while `tokkun_tutorial_flg` likely marks whether the tutorial has been shown so later Tokkun selections skip the tutorial.
- The user clarified that mixed one-credit mode payloads should not normally happen. For leniency, if Tokkun mode is identified, ignore other-mode material instead of saving score/crown/battle state.
- The user explicitly rejected source-scan "guardrails" for Tokkun. Planners should not recreate them under a different name.

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope.

</deferred>

---

*Phase: 7-Tokkun Evidence Contract and Guardrail Reset*
*Context gathered: 2026-06-03T22:29:15.4478436+08:00*
