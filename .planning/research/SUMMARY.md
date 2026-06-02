# Project Research Summary

**Project:** TaikoLocalServer Blue Support v1.1 - Blue Tokkun Mode Support
**Domain:** ASP.NET Core Blue AC15 cabinet protocol support, direct protobuf playresult handling, and local SQLite persistence
**Researched:** 2026-06-03
**Confidence:** MEDIUM

## Executive Summary

Blue Tokkun support should be built as a narrow Blue-owned practice-mode integration inside the existing TaikoLocalServer architecture. The repo already has the right stack: ASP.NET Core 10, direct protobuf Blue controllers under `/v10r03/chassis/*`, Mapperly wire-to-common DTO mapping, Mediator handlers, EF Core SQLite persistence, and focused xUnit guard tests. Experts would not add a payment subsystem, scraper, parallel data pipeline, or cross-era abstraction for this. Tokkun belongs beside the existing Blue normal and battle branches, with its own DTO fields, classifier, handler partial, and tests.

The recommended approach is evidence-first and permissive at the protocol edge. Keep Banacoin endpoints stateless and success-shaped so Tokkun can always be played, but never store Banacoin balance, coupons, payments, deductions, `chid`, BNID result, or transaction history. Accept and classify Tokkun `playresult.php` uploads from proven Tokkun fields such as `ary_tokkunstage_info` and `tokkun_tutorial_flg`, not from a guessed numeric `play_mode`. Persist only Blue-owned Tokkun state that has a concrete readback or audit need; otherwise log and return success.

The main risk is accidentally treating Tokkun as normal Blue playresult and corrupting normal scores, crowns, Dani, battle, favorites, recent songs, profile counters, unlocks, shop state, or rewards. The roadmap must put classification and replacement guardrails before persistence. The old v1.0 source guards that banned Tokkun terms are now stale, but they must be replaced with bounded positive support tests that allow Tokkun only in named Blue Tokkun paths and still forbid invented score, reward, Banacoin, or battle semantics.

## Key Findings

### Recommended Stack

Use the existing TaikoLocalServer stack with no new runtime dependencies. Current Blue protocol code already has generated protobuf fields for Tokkun and Banacoin-adjacent messages, current controllers already use direct protobuf, and current handlers already separate Blue normal and battle behavior through partial files.

**Core technologies:**
- .NET SDK 10 / C# 13: project baseline; keep Tokkun in the existing `net10.0` solution.
- ASP.NET Core 10: hosts Blue `/v10r03/chassis/*` direct protobuf endpoints.
- protobuf-net 3.2.x: serializes existing Blue wire DTOs; no schema/tool change is required unless proto evidence changes.
- Mediator.SourceGenerator 3.0.2: preserve controller-to-application dispatch through `UpdatePlayResultCommand(GameEra.Blue)`.
- EF Core SQLite 10: use only for proven Blue-owned Tokkun tutorial or summary state; never for Banacoin state.
- xUnit 2.9.3: add mapper, handler isolation, Banacoin statelessness, source guard, and route-surface tests.

### Expected Features

**Must have:**
- Tokkun can be selected, entered, completed, and acknowledged against TaikoLocalServer.
- Banacoin-looking flow is stateless and permissive enough to avoid blocking Tokkun.
- Blue Tokkun playresults are accepted, logged, and classified without guessed `play_mode`.
- Tokkun uploads return success and do not write normal, battle, Dani, profile, favorite, recent, unlock, medal, costume, title, or shop state.
- Existing full direct-protobuf request logging is preserved for Tokkun and Banacoin evidence.
- Old Tokkun source guards are replaced by positive bounded support tests and no-contamination tests.
- Cabinet/RPCS3 smoke guidance covers selection, Banacoin request sequence, gameplay entry, final upload, and post-upload userdata behavior.

**Should have:**
- Evidence-tagged Tokkun protocol contract documenting proven, observed, and deliberately ignored fields.
- Sanitized raw fixture corpus for Tokkun playresult and Banacoin-adjacent requests.
- Permissive unknown-field handling that logs unexpected `play_mode`, `stage_mode`, payment, and summary values while returning success.
- Blue-owned tutorial flag readback or Tokkun summary persistence only after evidence proves the client consumes it.

**Defer:**
- `PlayMode.Tokkun` enum value until RPCS3/cabinet logs or deeper IDA prove the exact numeric value.
- `getbanacoininfo.php` route until traffic or route xrefs prove the Blue client calls it and current absence blocks Tokkun.
- Tokkun summary history UI, derived analytics, practice timer accounting, jump/autoplay/speed semantics, and final review behavior.
- Any real Banacoin wallet, payment, settlement, receipt, or balance model.

### Architecture Approach

Tokkun should be integrated as a Blue playresult kind with a dedicated branch before any normal save logic. Controllers stay thin; mappers translate generated Blue wire DTOs into common Blue Tokkun DTO fields; the application handler classifies exactly one Blue playresult kind and routes to Tokkun, battle, normal/Dani, or an ambiguous success/no-cross-write path.

**Major components:**
1. `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`: direct protobuf logging, mapping, Mediator dispatch, response mapping.
2. `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`: map `payment_method`, `tokkun_tutorial_flg`, and `ary_tokkunstage_info` into common DTO fields with optional-field presence preserved.
3. `Application/Dtos/CommonPlayResultData.BlueTokkun.cs`: raw Blue Tokkun fields and summary DTOs, separate from battle and normal state.
4. `Application/Common/BluePlayResultMapping.cs`: central classifier returning `Tokkun`, `Battle`, `Normal`, or `Ambiguous` without relying on a guessed enum value.
5. `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs`: accept Tokkun result, optionally store proven Tokkun facts, and never call normal/battle persistence helpers.
6. Blue Banacoin controllers: keep heartbeat, balancecheck, payment, and error-log as stateless compatibility stubs; add no EF or Mediator dependency.
7. Conditional persistence: `UserSaveDataBlue.TokkunTutorialFlg` or `BlueTokkunPlaySummary` only if evidence proves readback or audit value.

### Critical Pitfalls

1. **Tokkun falls through normal Blue playresult** - classify Tokkun before normal save logic and test that normal, Dani, battle, favorite, unlock, shop, and profile state remains unchanged.
2. **Old guards are deleted instead of replaced** - retire stale "Tokkun must not exist" guards only in the same phase that adds bounded Tokkun isolation guards.
3. **Banacoin state is persisted** - keep all Banacoin endpoints stateless, success-shaped, and log-only; no entities, migrations, balances, coupons, or transaction records.
4. **Practice semantics are invented from names or wiki text** - use the wiki only for gameplay scoping; local proto, logs, IDA, SQLite, and cabinet/RPCS3 evidence decide server behavior.
5. **`getbanacoininfo.php` is added from proto alone** - do not expand route surface unless a real client call or IDA route path proves it is required.
6. **Numeric `play_mode` is guessed** - current evidence suggests Tokkun labels exist, but the playresult value is not proven; classify from Tokkun optional fields until proven.
7. **Unknown Tokkun shapes hard-fail** - log and return success for unknown or mixed Tokkun data unless client evidence proves a failure response is required.

## Implications for Roadmap

Based on research, suggested phase structure:

### Phase 1: Tokkun Evidence Contract and Guardrail Reset

**Rationale:** The classifier and tests must be designed before runtime changes, because the largest regression risk is fall-through into normal Blue persistence.
**Delivers:** Evidence ledger, initial Tokkun fixture/log plan, common DTO contract, classifier policy, replacement source-guard policy, and explicit unknowns for `play_mode`, `stage_mode`, tutorial readback, and route surface.
**Addresses:** Tokkun playresult classification, full request logging, evidence-tagged protocol contract, old source guard replacement.
**Avoids:** Guessed `play_mode`, stale source guards, wiki-derived semantics, and loss of direct-protobuf evidence.

### Phase 2: Stateless Banacoin Compatibility and Availability

**Rationale:** Tokkun selection may be blocked before any useful playresult evidence is available, so the permissive Banacoin path must be validated early.
**Delivers:** Tests and any minimal changes for heartbeat, balancecheck, banacoinpayment, and banacoinerrorlog to return enough stateless success behavior for Tokkun entry. Keeps `getbanacoininfo.php` absent unless proven.
**Addresses:** Stateless Banacoin allow path, mode selection reachability, no Banacoin persistence.
**Avoids:** Banacoin tables, real payment modeling, public service restrictions, and proto-only route expansion.

### Phase 3: Tokkun Mapper and Safe Playresult Acceptance

**Rationale:** Once the protocol contract is bounded, the server can safely accept Tokkun uploads without writing gameplay progression.
**Delivers:** Blue Tokkun common DTO fields, mapper tests, classifier implementation, `HandleBlueTokkun` success path, ambiguous mixed-payload handling, and no-cross-write handler tests.
**Addresses:** Tokkun final upload acceptance, no normal/battle/Dani/profile/favorite/unlock contamination, permissive unknown-field behavior.
**Avoids:** Normal score/crown persistence, battle state contamination, hard-failing unknown Tokkun modes, and invented rewards.

### Phase 4: Evidence-Backed Tokkun State Readback

**Rationale:** Persistence should follow observed client need, not field names. This phase can be skipped or narrowed if logs show no readback requirement.
**Delivers:** If proven, `TokkunTutorialFlg` in Blue save data and `UserDataResponse`; if proven useful, Blue-only Tokkun summary rows for song/count/speed/autoplay/jump facts. Includes migrations and tests only for the selected state.
**Addresses:** Tutorial flag readback and optional Tokkun summary capture.
**Avoids:** Banacoin persistence, normal play table reuse, inferred practice timer, and reward/unlock side effects.

### Phase 5: Cabinet/RPCS3 Smoke and Contract Tightening

**Rationale:** Automated tests cannot prove mode selection, payment sequence, gameplay entry, retry behavior, or exact numeric mode constants.
**Delivers:** Repeatable smoke notes, captured request samples, decision on `coin_coupon` value, decision on `getbanacoininfo.php`, and optional addition of `PlayMode.Tokkun` only after numeric proof.
**Addresses:** End-to-end playability, exact route sequence, `play_mode`/`stage_mode` uncertainty, post-upload userdata behavior.
**Avoids:** Shipping tests-only support that still fails on cabinet/RPCS3.

### Phase Ordering Rationale

- Evidence and guardrails come first because a small mapper change can otherwise route Tokkun into normal state writes.
- Banacoin compatibility comes before runtime persistence because it may gate access to gameplay and request capture.
- Acceptance and isolation come before readback because Tokkun can be playable with log-and-success behavior while persistence remains uncertain.
- Readback is conditional because current proto proves field existence, not client consumption.
- Cabinet/RPCS3 validation finishes the loop because only real traffic can close numeric mode, route, and retry uncertainties.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 1:** Requires local proto/binary/log/IDA alignment and an unknowns ledger for classifier inputs.
- **Phase 2:** Needs RPCS3/cabinet traffic or targeted IDA if current Banacoin stubs block selection.
- **Phase 4:** Needs readback evidence before adding tutorial or summary persistence.
- **Phase 5:** Requires cabinet/RPCS3 execution and log capture; tests are not enough.

Phases with standard patterns:
- **Phase 3:** Implementation uses established mapper, DTO partial, Mediator handler partial, and xUnit isolation-test patterns once Phase 1 defines the contract.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Existing repo versions, Blue direct-protobuf controllers, generated wire DTOs, EF Core, and tests are well established. |
| Features | MEDIUM-HIGH | Table stakes are consistent across project scope, proto, code, and gameplay context; exact runtime constants and route sequence remain unproven. |
| Architecture | MEDIUM | Patterns are clear from Blue normal and battle code, but Tokkun-specific readback/persistence boundaries need traffic evidence. |
| Pitfalls | HIGH for repo risks, MEDIUM for gameplay behavior | Fall-through, guard removal, Banacoin persistence, and route-surface risks are concrete in current code/tests; exact Tokkun cabinet behavior needs capture. |

**Overall confidence:** MEDIUM

### Gaps to Address

- Numeric Tokkun `play_mode`: do not add a final enum value until RPCS3/cabinet logs or deeper IDA prove it.
- Tokkun `stage_mode` values: log and accept unknown Tokkun shapes; do not route them through normal stage validation.
- Banacoin allowance value: current `coin_coupon = 0` may or may not block selection; keep stateless and adjust only from observed cabinet behavior.
- `getbanacoininfo.php`: generated messages exist, but route use is unproven; keep absent until blocking evidence exists.
- `tokkun_tutorial_flg` readback: persist/serialize only if first-run or second-run traces prove client dependency.
- Tokkun summary persistence: store only observed non-payment facts if an audit/readback value is proven; otherwise keep log-only.
- Full cabinet/RPCS3 evidence: final done needs selection, Banacoin sequence, gameplay entry, final upload, and post-upload userdata notes.

## Sources

### Primary (HIGH confidence)

- `.planning/PROJECT.md` - v1.1 scope, evidence hierarchy, Blue state separation, and Banacoin non-storage constraint.
- `.planning/research/STACK.md` - current stack, protocol fields, integration points, and evidence gaps.
- `.planning/research/FEATURES.md` - table stakes, differentiators, MVP definition, anti-features, and feature dependencies.
- `.planning/research/ARCHITECTURE.md` - Blue Tokkun branch architecture, component responsibilities, data flow, and test architecture.
- `.planning/research/PITFALLS.md` - critical risks, phase mapping, and "looks done but isn't" checklist.
- `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` - Tokkun and Banacoin wire field shape.
- Current Blue controller, mapper, handler, persistence, and test files cited by the research documents.

### Secondary (MEDIUM confidence)

- Local IDA daemon Tokkun probes cited in the research files - confirms Blue binary Tokkun code/assets/protobuf strings and Banacoin route strings, but not final `play_mode`.
- Cabinet/RPCS3 logs to be captured during implementation - required to close route sequence and runtime constant gaps.

### Tertiary (LOW confidence for protocol, MEDIUM for gameplay context)

- Wiki gameplay context: https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/%E5%9F%BA%E6%9C%AC%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0#tokkunmode - useful only for gameplay scoping and anti-features; local evidence outranks it.

---
*Research completed: 2026-06-03*
*Ready for roadmap: yes*
