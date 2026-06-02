# Feature Research

**Domain:** Blue AC15 Tokkun mode support for TaikoLocalServer
**Researched:** 2026-06-03
**Confidence:** MEDIUM-HIGH

## Feature Landscape

Blue Tokkun should be treated as a cabinet-visible practice mode that rides on the existing Blue direct-protobuf game routes and the existing Blue card/user identity flow. The user-visible model is: a one-player practice session can be selected, Banacoin-looking checks must not block play, the user practices selected charts with Tokkun controls, and the server accepts the final Tokkun result without turning it into normal score/reward progression.

The protocol-visible model is narrower: Blue local evidence shows Tokkun fields in `UserDataResponse` and `PlayResultRequest`, Banacoin/payment protobuf messages, confirmed Banacoin-related route strings, and Blue binary Tokkun runtime assets/classes. It does not yet prove the numeric Tokkun `play_mode`, `stage_mode`, timing rules, reward rules, or exact readback state. Those must be captured from cabinet/RPCS3 logs or additional IDA tracing before implementation hardens classifiers.

Public wiki context says Tokkun was an AC-only one-player practice mode with normal-play fee plus Banacoin, free chart practice, a practice timer, final performance/review flow, and no ranking/Donder Hiroba/reward reflection. Use that only as gameplay context. Do not use it to decide Blue availability or server storage semantics because the local Blue proto/binary evidence and user statement supersede the wiki's Blue-era availability claim.

### Table Stakes

Features users assume exist. Missing these means Tokkun still feels broken.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Tokkun can be selected and reaches gameplay | The milestone exists because the Blue cabinet exposes Tokkun despite earlier roadmap assumptions. | HIGH | Requires route/protocol behavior that lets the cabinet pass mode selection, Banacoin checks, gameplay, and final upload. Local IDA found `GameTokkunMode`, `TokkunModeTask`, Tokkun Lumen assets, and `enso_tokkun` packed data. |
| Stateless Banacoin allow path | The cabinet/user flow involves Banacoin, but this repo must not store Banacoin state. | MEDIUM | Keep `heartbeat.php` advertising Banacoin available, `balancecheck.php` successful with `coin_coupon = 0`, `banacoinpayment.php` successful with echoed `personid`, and `banacoinerrorlog.php` successful/log-only. Add more only if cabinet logs prove a missing route blocks Tokkun. |
| Tokkun playresult classification | Tokkun must bypass normal Blue scoring and battle handling. | HIGH | Detect from proven Blue Tokkun signals: `payment_method`, `tokkun_tutorial_flg`, and/or `ary_tokkunstage_info` containing `banacoin_datetime`, song count/list, speed-change count, autoplay count, and jump count. Do not guess a numeric `play_mode`; current IDA probes did not prove it. |
| Accept Tokkun final upload with `result = 1` | The user needs Tokkun completion from the cabinet perspective. | MEDIUM | `playresult.php` should deserialize direct protobuf, log the full request, map Tokkun fields explicitly, return success, and avoid hard-failing unknown Tokkun mode/stage values while evidence is still being gathered. |
| No normal/battle/Dani/profile contamination | Wiki context and project constraints both point away from normal result reflection; local code currently would save non-battle playresults through normal logic. | HIGH | Tokkun uploads must not write `SongPlayDatumBlue`, `SongBestDatumBlue`, crowns, Dani, battle rows, profile counters, favorites, recent songs, item unlocks, costumes, or medals unless new Blue evidence proves a specific side effect. |
| Blue-owned Tokkun tutorial state where proven | Blue wire has `tokkun_tutorial_flg` in userdata and playresult; current mapper omits it by design. | MEDIUM | If cabinet logs show the flag is required for first-use UX or readback, persist it in Blue-owned save data and serialize it in `UserDataResponse`. Otherwise keep it absent/defaulted and log. |
| Blue-owned Tokkun summary capture where proven | The final result payload includes a compact Tokkun summary. | MEDIUM | It is reasonable to store a Blue-owned raw/summary row after logs confirm payload shape: Banacoin datetime, song count/list, speed-change count, autoplay count, jump count, and upload time. Store raw facts, not inferred rewards or timer semantics. |
| Focused regression tests and source guards | Existing tests intentionally block Tokkun semantics; v1.1 must replace that guard with a bounded contract. | MEDIUM | Add tests that Tokkun maps/accepts, Banacoin stubs are stateless, Tokkun does not touch normal/battle/Dani/reward/profile tables, and future code cannot accidentally route Tokkun through normal save logic. |
| Cabinet/RPCS3 smoke checklist | Server tests cannot prove mode selection and practice flow. | MEDIUM | Done should include repeatable evidence for selection, Banacoin check/payment sequence, gameplay entry, final upload, and post-upload userdata behavior. |

### Differentiators

Features that improve the implementation without expanding milestone scope.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Evidence-tagged Tokkun protocol contract | Future changes can see which fields are proven, observed, or deliberately ignored. | LOW | Put the final classifier/source notes in phase docs/tests. Include local proto, request logs, IDA findings, and wiki-as-context separation. |
| Raw Tokkun fixture corpus | Makes regressions obvious without requiring cabinet access for every edit. | MEDIUM | Capture sanitized direct-protobuf request payloads for balance/payment/playresult/userdata before and after implementation. |
| Permissive but observable unknown-field behavior | Lets the cabinet proceed while preserving evidence for later tightening. | LOW | Log unrecognized Tokkun `play_mode`, `stage_mode`, payment method, or summary values, return success, and avoid persistence unless a known contract is met. |
| Tokkun-specific AdminApi debug surface | Useful for developers/operators after backend proof exists. | MEDIUM | Defer until raw Tokkun summary persistence is proven. Keep out of the initial user-visible cabinet MVP. |

### Deferred Future

Features that may be useful later, but should not be required for this milestone.

| Feature | Why Defer | Trigger to Revisit |
|---------|-----------|--------------------|
| Exact Tokkun `play_mode` enum value in `Domain/Enums/PlayMode.cs` | Current local code knows normal/Dan/Gaiden/AiBattle only; IDA string/table probes found Tokkun labels but did not prove the numeric playresult value. | Add once cabinet/RPCS3 playresult logs or deeper IDA evidence prove the value. |
| Practice timer/accounting | Wiki gameplay context mentions timed practice, but server-side timer behavior is not proven. | Add only if the cabinet sends or expects server-owned timer state. |
| Speed-change/autoplay/jump semantics | Proto has counters, not meaning. | Add analytics or UI only after multiple observed payloads prove how counters increment. |
| Final performance/review timing | Wiki context says a final performance/review flow exists; protocol obligations are unproven. | Add only if cabinet logs show response fields or server state affect the review screen. |
| Banacoin balance, debit, settlement, receipts, or history | User explicitly says the repo must not store Banacoin state. | Do not revisit unless the project scope changes beyond local-server permissive Tokkun play. |
| `getbanacoininfo.php` route | Proto has request/response messages, but prior Blue evidence and current IDA probe did not prove a cabinet route string. | Add a stateless route only if Tokkun cabinet traffic calls it and current absence blocks play. |
| WebUI Tokkun history | Not needed to make Tokkun playable from the cabinet. | Add after backend Tokkun summary persistence has real fixtures and user value. |

### Anti-Features

Features that seem attractive but create incorrect or unsupported behavior.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Real Banacoin wallet/payment state | Tokkun uses Banacoin in the user flow. | TaikoLocalServer is not a Banacoin authority, and the milestone explicitly forbids storing Banacoin state. | Return stateless success/available responses that let Tokkun proceed; log requests. |
| Save Tokkun as normal scores/crowns/self-best | Tokkun playresult stages look like normal song stages. | Wiki context says no ranking/reward reflection, and local project constraints forbid invented score/reward behavior. | Bypass normal save logic; persist only proven Tokkun tutorial/summary facts. |
| Mirror Tokkun songs into recent/favorite/profile counters | Existing normal handler already updates these for accepted stages. | Practice mode should not mutate normal user-visible history without Blue evidence. | Tokkun branch must skip normal counters/favorites/recents. |
| Award songs, titles, costumes, medals, or item-shop unlocks from Tokkun | PlayResultRequest has generic reward arrays and medals. | No local evidence proves Tokkun rewards, and this would corrupt normal/shop state. | Ignore/log reward arrays on Tokkun uploads unless a specific Blue trace proves one. |
| Implement server-side practice time or time-of-day gating from wiki text | Wiki describes gameplay behavior. | Wiki-only claims are not server protocol requirements, and the client likely owns most UX timing. | Let the cabinet drive timing; only handle fields observed in requests/responses. |
| Use wiki to deny Blue Tokkun support | Wiki says Tokkun ended before/with Yellow. | User statement and local Blue proto/binary evidence contradict that for this project. | Treat wiki as gameplay context, not Blue availability authority. |
| Add `Tokkun = ?` to `PlayMode` by guess | A typed enum is attractive for handler clarity. | The numeric value is not yet proven by local evidence. | Keep classification based on proven Tokkun fields until logs/IDA prove the enum value. |

## Feature Dependencies

```text
Stateless Banacoin allow path
    -> Tokkun selection reaches gameplay
        -> Tokkun playresult fixture capture
            -> Tokkun classifier
                -> Tokkun no-normal-save handler
                    -> Optional tutorial/summary persistence
                        -> Userdata readback if proven

Cabinet/RPCS3 smoke
    -> proves missing routes or exact play_mode values
    -> tightens tests and enum definitions

Wiki gameplay context
    -> informs anti-features
    -> does not define server semantics
```

### Dependency Notes

- **Banacoin allow path before gameplay proof:** If Banacoin status/payment blocks mode entry, no useful Tokkun playresult evidence will be captured.
- **Fixture capture before handler semantics:** The current proto proves fields exist, not how Blue uses them. Captured payloads should drive the first classifier.
- **Classifier before persistence:** Tokkun must be identified before normal Blue save logic runs, otherwise test data and real users can get polluted normal scores/crowns/favorites.
- **Tutorial/readback after proof:** `tokkun_tutorial_flg` exists on both userdata and playresult, but current implementation intentionally omits it. Only serialize/read back after logs show it matters.
- **Enum after numeric proof:** `PlayMode` should gain Tokkun only after a concrete numeric value is observed.

## MVP Definition

### Launch With

- [ ] Banacoin permissive path: heartbeat, balancecheck, banacoinpayment, and banacoinerrorlog are stateless and do not persist balance/payment data.
- [ ] Tokkun playresult is accepted and returns result `1`.
- [ ] Tokkun playresult is classified without relying on guessed `play_mode`.
- [ ] Tokkun upload does not write normal scores, crowns, self-best, Dani, battle, favorites, recent songs, profile counters, unlocks, costumes, or medals.
- [ ] Tokkun tutorial/summary fields are either logged-only or Blue-owned persisted/read back only where cabinet evidence proves the need.
- [ ] Tests cover the no-contamination contract and stateless Banacoin behavior.
- [ ] Cabinet/RPCS3 smoke notes cover mode selection, Banacoin sequence, gameplay entry, final upload, and post-upload userdata.

### Add After Validation

- [ ] Typed `PlayMode.Tokkun` once exact numeric value is proven.
- [ ] Blue-owned Tokkun summary history after raw payload fixtures confirm stable fields.
- [ ] `tokkun_tutorial_flg` userdata serialization after first-use/readback behavior is observed.
- [ ] Additional stateless Banacoin route(s) only if logs prove missing route calls.

### Future Consideration

- [ ] WebUI/AdminApi Tokkun debug/history view.
- [ ] Derived Tokkun analytics for practiced songs and control usage.
- [ ] More restrictive classifier validation once enough real payloads exist.

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Stateless Banacoin allow path | HIGH | MEDIUM | P1 |
| Tokkun playresult acceptance | HIGH | MEDIUM | P1 |
| Tokkun no-normal-save branch | HIGH | HIGH | P1 |
| Tokkun field mapping/logging | HIGH | MEDIUM | P1 |
| Cabinet/RPCS3 smoke checklist | HIGH | MEDIUM | P1 |
| Tutorial flag persistence/readback if proven | MEDIUM | MEDIUM | P2 |
| Raw Tokkun summary persistence if proven | MEDIUM | MEDIUM | P2 |
| Typed Tokkun play mode enum | MEDIUM | LOW | P2 |
| WebUI Tokkun history | LOW | MEDIUM | P3 |
| Real Banacoin wallet/payment state | NEGATIVE | HIGH | Do not build |

**Priority key:**
- P1: Must have for launch.
- P2: Should have after evidence closes the ambiguity.
- P3: Useful later, not necessary for playable Tokkun.

## Sources

- `H:/TaikoLocalServer/.planning/PROJECT.md` - HIGH confidence. Defines v1.1 Tokkun scope, evidence hierarchy, state-separation rules, and Banacoin non-storage constraint.
- `H:/TaikoLocalServer/.planning/STATE.md` - HIGH confidence. Confirms v1.1 is in requirements/planning and focused on Blue Tokkun.
- `H:/TaikoLocalServer/proto/blue/taiko.proto` - HIGH confidence. Defines `tokkun_tutorial_flg`, `payment_method`, `ary_tokkunstage_info`, Banacoin messages, and heartbeat Banacoin status.
- `H:/TaikoLocalServer/Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - HIGH confidence. Current mapper preserves `PlayMode` and battle classification but omits Tokkun mapping.
- `H:/TaikoLocalServer/Application/Handlers/UpdatePlayResultCommand.Blue.cs` - HIGH confidence. Current non-battle Blue playresults enter normal save logic, so Tokkun needs an explicit branch to avoid normal-state writes.
- `H:/TaikoLocalServer/Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs`, `BanacoinPaymentController.cs`, `BanacoinErrorLogController.cs`, `HeartbeatController.cs` - HIGH confidence. Current Banacoin-related responses are stateless success/available stubs.
- `H:/TaikoLocalServer/Tests/Blue/BlueA4SourceGuardTests.cs`, `BlueMapperTests.cs`, `BluePlayResultMapperTests.cs` - HIGH confidence. Current guard/test posture intentionally prevents Tokkun runtime semantics and omits userdata Tokkun serialization.
- Blue IDA daemon probes: `.tools/blue/scratch/req_tokkun_features_20260603_probe.py` and `.tools/blue/scratch/req_tokkun_features_20260603_mode_probe.py` - MEDIUM-HIGH confidence. Found Tokkun runtime/assets and confirmed route/type strings, but did not prove numeric `play_mode`.
- Wiki gameplay context: <https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/%E5%9F%BA%E6%9C%AC%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0#tokkunmode> - MEDIUM confidence for gameplay context only. Do not use for Blue availability or server semantics.

---
*Feature research for: Blue Tokkun mode support*
*Researched: 2026-06-03*
