# Phase 07 Plan 07-01: Blue Tokkun Evidence Contract

## Scope And Guardrails

This contract covers TKEV-01, TKEV-02, and TKEV-03 for Blue Tokkun support. It is a planning artifact only. It does not approve runtime Tokkun DTOs, mapper fields, handler branches, persistence, migrations, routes, wire regeneration, generated `Wire/` cleanup, `getbanacoininfo.php`, Banacoin response semantics, cabinet/RPCS3 proof claims, or raw request-log excerpts.

Phase 7 follows the locked decisions from `07-CONTEXT.md`: D-01 requires a battle-style row-by-row evidence matrix; D-02 fixes the status vocabulary to `proven`, `observed`, `deliberately ignored`, and `unknown/blocked`; D-03 makes local proto/wire/current-code plus Blue IDA/client evidence authoritative over stale wiki text while reserving cabinet/RPCS3 proof for Phase 11 unless a row is blocked on that proof; D-04 keeps protocol fields, endpoint questions, and guard reset decisions in one contract.

D-05 through D-09 define the Tokkun classifier boundary. `ary_tokkunstage_info` is the primary likely Tokkun upload classifier signal. `tokkun_tutorial_flg` is tutorial/readback evidence and is not a standalone classifier. `PlayMode.Tokkun` must not be added or assigned a numeric value. Later Tokkun-classified uploads are lenient: they return success and ignore other-mode material instead of failing or writing normal/battle state. Downstream classifier output may expose `IsTokkun` plus protocol-backed facts, but not a guessed enum.

D-10 through D-13 reset the guard strategy. The stale Tokkun-term source guard in `Tests/Blue/BlueA4SourceGuardTests.cs` is removed. It must not be replaced with another Tokkun word scan, named source guard, or allowlist. Future protection is behavior-based: Phase 9 and Phase 11 tests must exercise actual Tokkun runtime handling and prove no unrelated state writes.

D-14 through D-17 define the handoff. Phase 8 owns Banacoin-adjacent route availability, especially `getbanacoininfo.php`. Phase 9 owns mapper/classifier/logging/no-normal-battle-write runtime work. Phase 10 owns protocol-backed tutorial and summary persistence candidates. Phase 11 owns cabinet/RPCS3 Tokkun proof and final contract tightening.

## Evidence Priority

| Priority | Evidence Type | Use In This Artifact | Contract Rule |
|----------|---------------|----------------------|---------------|
| 1 | Blue IDA/client behavior | Confirms actual Blue route, field, parser, and runtime mechanics when available. | Authoritative over wiki text and over schema-only guesses. |
| 2 | Local proto and generated Blue wire | Maps request/response shape, field names, optional presence helpers, and route message types. | Proves protocol surface only; it does not prove gameplay, persistence, payment, score, reward, or route-call semantics. |
| 3 | Current server source and tests | Records current route ownership, mapper omissions, handler side effects, enum values, and stale guard posture. | Current behavior is a boundary to protect until a later phase proves changes. |
| 4 | Planning docs and archived battle gates | Provide evidence-gated workflow and no-cross-write precedent. | Use as process precedent, not Tokkun protocol truth. |
| Gate | Cabinet/RPCS3 logs | Final Blue Tokkun selection, upload, readback, and route proof. | Reserved for Phase 11 unless a row below explicitly blocks earlier work on that proof. |

## Status Taxonomy

| Status | Meaning | Runtime Permission |
|--------|---------|--------------------|
| `proven` | The row is backed by local proto/wire/current-code or existing Blue client evidence for the narrow claim stated. | May be cited by later plans only for that narrow claim. |
| `observed` | The row exists or is strongly indicated, but semantics, value, route timing, or persistence role are not fully proven. | May guide classifier or mapper design only with additional guardrails. |
| `deliberately ignored` | The row is recognized but intentionally produces no server-side effect in this milestone slice. | Must not write state or infer gameplay/payment/reward meaning. |
| `unknown/blocked` | Required proof is missing or belongs to a later phase. | Must not be implemented or relied on until the named owner closes the gate. |

## Unified Tokkun Row Matrix

| Row | Subject | Status | Evidence | Phase 7 Contract | Blocked Assumptions | Follow-on Owner |
|-----|---------|--------|----------|------------------|---------------------|-----------------|
| TK-01 | `UserDataResponse.tokkun_tutorial_flg` | observed | `proto/blue/taiko.proto` defines optional field 37; `Adapters.GameProtocol.Blue/Wire/Game.cs` has `TokkunTutorialFlg` plus presence helpers. | Tutorial/readback candidate only. It is not proof that first-run Tokkun tutorial state must already be persisted. | No default value, readback timing, first-run rule, or persistence rule is approved. | Phase 10, then Phase 11 proof. |
| TK-02 | `PlayResultRequest.tokkun_tutorial_flg` | observed | `proto/blue/taiko.proto` defines optional field 44; generated wire exposes presence helpers. | Tutorial-state upload evidence only. It is not a standalone Tokkun classifier. | Do not classify Tokkun solely from this flag, and do not treat it as score/progression state. | Phase 9 classifier, Phase 10 persistence if proven. |
| TK-03 | `PlayResultRequest.ary_tokkunstage_info` | observed | `proto/blue/taiko.proto` defines optional field 45; generated wire exposes `AryTokkunstageInfo`; current Blue mapper does not map it. | Primary likely Tokkun upload classifier signal until stronger client evidence proves a different rule. | Field presence does not prove numeric `play_mode`, normal score behavior, rewards, Banacoin state, or persistence side effects. | Phase 9 mapper/classifier. |
| TK-04 | `TokkunstageData.banacoin_datetime` | proven | Proto and generated wire define a required string inside `TokkunstageData`. | Protocol-backed summary fact candidate. Keep as raw/client-reported data if later persisted. | No payment, wallet, receipt, practice-time, or transaction-history meaning is approved. | Phase 10 persistence, Phase 11 validation. |
| TK-05 | `TokkunstageData.tokkun_song_cnt` | proven | Proto and generated wire define a required count field. | Protocol-backed summary fact candidate. | Do not infer ranking, score count, reward thresholds, or unlock progress. | Phase 10. |
| TK-06 | `TokkunstageData.tookun_songno` | proven | Proto and generated wire define the repeated song number field with the existing misspelling. | Protocol-backed summary fact candidate. Preserve the wire spelling when citing the field. | Do not infer song unlocks, favorites, recent songs, score rows, or crowns. | Phase 10. |
| TK-07 | `TokkunstageData.tokkun_speedchange_cnt` | proven | Proto and generated wire define a required count field. | Raw/protocol-backed summary fact candidate. | Do not infer achievement, reward, ranking, or practice rules. | Phase 10. |
| TK-08 | `TokkunstageData.tokkun_autoplay_cnt` | proven | Proto and generated wire define a required count field. | Raw/protocol-backed summary fact candidate. | Do not infer score validity or reward penalties. | Phase 10. |
| TK-09 | `TokkunstageData.tokkun_jump_cnt` | proven | Proto and generated wire define a required count field. | Raw/protocol-backed summary fact candidate. | Do not infer jump-point behavior, rewards, or progression. | Phase 10. |
| TK-10 | `PlayResultRequest.play_mode` | observed | Current wire has `PlayMode`; current `Domain/Enums/PlayMode.cs` defines `Normal = 0`, `DanMode = 1`, `GaidenMode = 4`, and `AiBattle = 6`. | Keep the raw numeric `play_mode` available for logging/context, but do not classify Tokkun from a guessed value. | No Tokkun numeric value is proven. | Phase 9 may log raw value; Phase 11 owns proof. |
| TK-11 | `PlayMode.Tokkun` | unknown/blocked | No current enum member exists; requirements explicitly forbid guessing. | Do not add `PlayMode.Tokkun` in Phase 7. Do not assign a numeric Tokkun play mode. | Any numeric constant, enum member, switch branch, or compatibility alias is blocked. | Phase 11 proof before any enum addition. |
| TK-12 | Mixed Tokkun plus other-mode material | deliberately ignored | `07-CONTEXT.md` D-08 says one credit has one mode and Tokkun should not save scores; leniency should ignore other-mode material. | Future Tokkun-classified uploads return success and ignore normal, Dani, battle, favorite, recent, unlock, and profile material instead of failing or writing those states. | Do not write `SongPlayDataBlue`, `SongBestDataBlue`, crowns, Dan rows, battle rows, favorites, recent songs, profile counters, unlocks, medals, customization, titles, shop, rewards, or Banacoin state. | Phase 9 behavior tests; Phase 11 smoke proof. |
| TK-13 | `banacoinpayment.php` | deliberately ignored | Current Blue controller exists as stateless log/success compatibility. Real Banacoin is out of scope. | Keep payment-like behavior stateless and permissive only where already proven necessary for compatibility. | No balance, coupon, deduction, receipt, BNID, transaction, wallet, or real payment state. | Phase 8 route/availability checks. |
| TK-14 | `banacoinerrorlog.php` | deliberately ignored | Current Blue controller exists as stateless log/success compatibility. | Error-log compatibility is not payment state and must remain side-effect free. | No transaction history, payment error state, or account state. | Phase 8. |
| TK-15 | `getbanacoininfo.php` | unknown/blocked | Proto/wire message types exist, but `BlueRouteSkeletonTests` currently excludes the route and no controller exists. | Do not add the route in Phase 7. Treat it as a route-surface unknown. | Schema presence alone does not prove the Blue Tokkun client calls it or that absence blocks play. | Phase 8 with cabinet/RPCS3 logs or IDA route proof. |
| TK-16 | Stale `BlueA4SourceGuardTests` Tokkun-term guard removal | proven | `Tests/Blue/BlueA4SourceGuardTests.cs` contained `BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics`, which scanned for `Tokkun`, `Tookun`, and `deferred fields`. | Remove that method and keep only the Green leakage guards. | Do not replace it with another Tokkun source scan, named source guard, or word allowlist. | Phase 7. |
| TK-17 | Behavior-based runtime guard policy | observed | `07-CONTEXT.md` D-11 through D-13 require runtime control-flow proof rather than source-text scans. | Later phases must test actual Tokkun handling and assert no normal/battle/Banacoin contamination. | Source text absence is not proof of safe behavior. | Phase 9 automated tests and Phase 11 final tests. |
| TK-18 | Tokkun persistence/readback candidates | unknown/blocked | Protocol-backed candidates are tutorial state and `TokkunstageData` summary fields. | Phase 7 names candidates only. It does not approve entities, DbSets, migrations, handlers, or readback. | No ranking, reward, score progression, payment history, practice-time rules, or unlock semantics. | Phase 10, then Phase 11. |
| TK-19 | Cabinet/RPCS3 final proof | unknown/blocked | Roadmap requires final Tokkun smoke evidence for selection, Banacoin request sequence, gameplay entry, upload, post-upload userdata behavior, and unexpected endpoints. | Phase 7 records proof needs only. It makes no smoke proof claim. | Done remains blocked until automated tests and cabinet/RPCS3 evidence agree. | Phase 11. |

## Classifier Contract

1. A later Blue Tokkun classifier should treat non-null `ary_tokkunstage_info` as the primary likely Tokkun upload signal.
2. `tokkun_tutorial_flg` may be mapped as tutorial/readback evidence, but it must not classify a payload as Tokkun by itself.
3. `PlayResultRequest.play_mode` may be logged as raw context, but Blue Tokkun classification must not depend on a guessed numeric `PlayMode.Tokkun` value.
4. A later common/application DTO may expose `IsTokkun` and protocol-backed Tokkun facts. It must not expose a guessed enum or convert Tokkun facts into normal score/progression semantics.
5. If a payload is classified as Tokkun, normal and battle state writes are bypassed. The accepted response remains success-shaped unless concrete client evidence proves a failure response is required.

## No Runtime Write Targets

Phase 7 must not create or modify these runtime targets:

| Forbidden Target | Reason |
|------------------|--------|
| `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` or equivalent DTO fields | Mapper/runtime data belongs to Phase 9 after this contract is consumed. |
| `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` Tokkun mapping | Classifier implementation belongs to Phase 9. |
| `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs` or Tokkun branches in existing handlers | No runtime side effects are approved in Phase 7. |
| Blue Tokkun entities, DbSets, EF configuration, migrations, or repositories | Persistence/readback belongs to Phase 10. |
| `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs` or `getbanacoininfo.php` route registration | Route availability belongs to Phase 8. |
| Generated `Adapters.GameProtocol.Blue/Wire/Game.cs` edits | Wire files are generated and are not manually cleaned up in this phase. |
| Cabinet/RPCS3 proof files or raw request/log excerpts | Final proof belongs to Phase 11 and must be sanitized. |

No Tokkun-classified upload may be used to write normal score, crown, Dani, battle, favorite, recent-song, profile, unlock, medal, customization, title, shop, reward, or real Banacoin/payment state unless a later owning phase proves a bounded behavior with concrete Blue evidence.

## Follow-On Gates

| Gate | Owner | Must Close Before Runtime Reliance |
|------|-------|------------------------------------|
| Banacoin route availability | Phase 8 | Prove every required Banacoin-adjacent endpoint from logs or IDA. Add `getbanacoininfo.php` only if evidence proves Blue Tokkun calls it and current absence blocks play. |
| Mapper and safe acceptance | Phase 9 | Implement Tokkun classification from `ary_tokkunstage_info` plus bounded facts, return success, log safely, and prove no normal/battle writes. |
| Persistence and readback | Phase 10 | Persist only protocol-backed tutorial and summary facts if required; keep state Blue-owned and separate from normal, battle, shop, and Banacoin storage. |
| Cabinet/RPCS3 final proof | Phase 11 | Prove selection, Banacoin sequence, gameplay entry, upload, userdata/readback behavior, unexpected endpoint behavior, and no unrelated state writes. |

## Decision Coverage

| Decision | Covered By |
|----------|------------|
| D-01 | Unified row matrix above follows the battle-style evidence pattern. |
| D-02 | Status taxonomy locks `proven`, `observed`, `deliberately ignored`, and `unknown/blocked`. |
| D-03 | Evidence priority ranks local proto/wire/current-code and Blue IDA/client evidence over stale wiki text. |
| D-04 | The matrix includes protocol fields, endpoint questions, persistence candidates, and guard reset decisions. |
| D-05 | Classifier contract names `ary_tokkunstage_info` as primary likely signal. |
| D-06 | Rows TK-01 and TK-02 keep `tokkun_tutorial_flg` as tutorial/readback evidence only. |
| D-07 | Rows TK-10 and TK-11 block guessed `PlayMode.Tokkun` numeric values. |
| D-08 | Row TK-12 defines lenient ignored-other-mode behavior. |
| D-09 | Classifier contract permits `IsTokkun` plus protocol-backed facts without a guessed enum. |
| D-10 | Row TK-16 records stale source guard removal. |
| D-11 | Row TK-16 blocks replacement Tokkun source scans. |
| D-12 | Row TK-17 requires behavior-based proof through runtime tests. |
| D-13 | Guard reset is defined as deleting stale scans and moving safety to runtime control flow. |
| D-14 | Row TK-15 and Follow-On Gates hand `getbanacoininfo.php` to Phase 8. |
| D-15 | Classifier and no-runtime-write sections hand mapper/logging/no-write work to Phase 9. |
| D-16 | Rows TK-01 through TK-09 and TK-18 hand protocol-backed persistence candidates to Phase 10. |
| D-17 | Row TK-19 hands cabinet/RPCS3 proof to Phase 11. |

## Verification Checklist

- `07-01-TOKKUN-EVIDENCE-CONTRACT.md` exists at the planned path.
- The contract uses all four statuses: `proven`, `observed`, `deliberately ignored`, and `unknown/blocked`.
- The contract includes `ary_tokkunstage_info`, `tokkun_tutorial_flg`, `PlayMode.Tokkun`, `getbanacoininfo.php`, `behavior-based`, `Phase 8`, `Phase 9`, `Phase 10`, and `Phase 11`.
- `Tests/Blue/BlueA4SourceGuardTests.cs` no longer contains `BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics`.
- No replacement Tokkun word-scan guard exists in `Tests/Blue/BlueA4SourceGuardTests.cs`.
- `Domain/Enums/PlayMode.cs` has no `Tokkun` enum member.
- `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs` does not exist.

## Phase 7 Result

Phase 7 approves a contract and a guard reset only. It does not approve runtime Tokkun behavior. Later phases must close their named gates before relying on any route, classifier, persistence, or proof row beyond the narrow contract above.
