# Pitfalls Research: Blue Tokkun Mode Support

**Domain:** TaikoLocalServer Blue AC15 Tokkun mode support
**Researched:** 2026-06-03
**Confidence:** HIGH for repo/code risks, MEDIUM for Tokkun gameplay behavior until cabinet/RPCS3 logs prove exact flow

## Critical Pitfalls

### Pitfall 1: Treating Tokkun as normal Blue playresult

**What goes wrong:**
Tokkun `playresult.php` requests fall through the current normal Blue handler and write `SongPlayDataBlue`, `SongBestDataBlue`, normal crowns, profile counters, favorite/recent rows, unlock flags, medals, costumes, title bits, Dani summary, or normal `LastPlayDatetime`.

**Why it happens:**
`UpdatePlayResultCommand.Blue.cs` only diverts battle requests through `IsBattlePlayResult`. Everything else enters the normal path, and that path applies medals, tutorial flags, unlock bits, profile counters, favorites/recent songs, normal score rows, best rows, and Dani handling. The proto exposes Tokkun as optional playresult fields, so a minimal mapper change can accidentally make Tokkun look like an ordinary non-battle result.

**How to avoid:**
Classify Tokkun before the normal save path. Add a dedicated Blue Tokkun branch that returns success and writes only Tokkun state proven by Blue proto, logs, IDA, or cabinet/RPCS3 evidence. Until proven, do not write normal score, crown, Dani, profile, favorite, customization, shop, unlock, or battle state. Treat medals from Tokkun as untrusted unless logs prove a Blue Tokkun side effect.

**Warning signs:**
- A Tokkun test inserts rows into `SongPlayDataBlue` or `SongBestDataBlue`.
- `HandleBlue` still has only battle-vs-normal branching after Tokkun mapping is added.
- `AryTokkunstageInfo` or `tokkun_tutorial_flg` appears in `CommonPlayResultData` but there is no Tokkun-specific handler.
- Tests assert Tokkun updates `ReleaseSongFlg`, crowns, favorites, recent songs, or profile counters.

**Phase to address:**
Phase 1: Tokkun evidence and classification contract. Phase 4: Tokkun playresult isolation implementation.

---

### Pitfall 2: Retiring old Tokkun source guards without replacement guardrails

**What goes wrong:**
The current guards either block legitimate Tokkun work or get deleted wholesale. That removes the protection that originally prevented guessed Tokkun semantics from leaking into runtime behavior.

**Why it happens:**
`BlueA4SourceGuardTests.BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics` currently asserts that Blue playresult code and common DTOs do not contain "Tokkun" or "Tookun". `BlueMapperTests.UserDataMapper_Blue_OmitsTokkunTutorialFlag` asserts that userdata omits `tokkun_tutorial_flg`. Those tests were correct for v1.0, when Tokkun was a non-goal, but v1.1 explicitly reopens Tokkun.

**How to avoid:**
Replace the old guards in the same phase that introduces Tokkun DTOs. New guards should allow Tokkun names only in dedicated Blue Tokkun DTO/handler/mapper/test files, and should assert that Tokkun code does not call normal best/crown/Dani/unlock update helpers unless a source-backed decision says so. For userdata tutorial behavior, replace "always omitted" with an evidence-backed assertion: omitted, zero, or persisted Blue-owned value, whichever logs/IDA prove.

**Warning signs:**
- The old source guard is deleted in a commit that does not add a new Tokkun isolation guard.
- A source guard is loosened to allow Tokkun terms across all `Application/Handlers`.
- The userdata Tokkun tutorial test is removed before evidence decides readback behavior.

**Phase to address:**
Phase 1: define replacement guard policy. Phase 3: update userdata/tutorial tests. Phase 4: update playresult guards with Tokkun isolation assertions.

---

### Pitfall 3: Persisting Banacoin state

**What goes wrong:**
The server starts storing Banacoin balance, coupons, payment status, `chid`, or transaction history, or maps Banacoin price into Don medals/shop state. This creates unsupported local payment state and conflicts with the milestone rule that this repo must not store Banacoin state.

**Why it happens:**
Tokkun is tied to Banacoin in gameplay context, and Blue already has `balancecheck.php`, `banacoinpayment.php`, and `banacoinerrorlog.php` controllers. The proto also defines `Getbanacoininfo*` messages. It is tempting to model a payment subsystem instead of returning only the success-shaped behavior needed for Tokkun to be playable.

**How to avoid:**
Keep Banacoin endpoints stateless. Existing `BalanceCheckController`, `BanacoinPaymentController`, and `BanacoinErrorLogController` already log full requests and return success-shaped responses without persistence. Preserve that pattern. If cabinet logs prove another Banacoin endpoint is required, add the smallest Blue-owned stateless stub needed for progression, with no EF entity, migration, balance table, or medal/shop mutation.

**Warning signs:**
- A migration adds Banacoin, payment, coupon, balance, or transaction tables.
- `banacoin_price` affects `TotalGetDonmedal`, shop season state, or unlock state.
- A controller writes request fields from `BalancecheckRequest` or `BanacoinpaymentRequest` to the database.
- `getbanacoininfo.php` is implemented only because the proto contains a message, without a blocking client log.

**Phase to address:**
Phase 2: Banacoin route/stub behavior. Phase 5: cabinet/RPCS3 verification that the stateless responses are sufficient.

---

### Pitfall 4: Inventing Tokkun practice semantics from field names or the wiki

**What goes wrong:**
The server assigns meaning to `TokkunSongCnt`, `TookunSongnoes`, speed-change count, autoplay count, jump count, `banacoin_datetime`, or `tokkun_tutorial_flg` and builds rewards, scoring, play-time accounting, jump-point persistence, or unlock behavior that the Blue client did not prove.

**Why it happens:**
The public wiki describes Tokkun gameplay concepts: extra payment, practice time, song restarts, jump points, autoplay, speed changes, and a final review period. The Blue proto has similarly named counters. Those names are useful for scoping, but they do not prove what the server must persist or echo.

**How to avoid:**
Use the wiki only as a list of gameplay areas to investigate. Protocol behavior must come from Blue proto, full request logs, IDA/client evidence, SQLite observations, and cabinet/RPCS3 traces. Start with log-and-success. Persist a raw or normalized Tokkun summary only when a readback consumer is proven. Do not implement service-window restrictions, 8-minute timers, reward rules, score reflection, or jump-point state in the server unless the Blue client asks the server for them.

**Warning signs:**
- Comments say "Tokkun probably means..." without a source link or evidence file.
- Tests assert practice-time, autoplay, speed-change, or jump behavior based only on wiki text.
- `banacoin_datetime` is treated as a payment ledger timestamp.
- Tokkun implementation adds unlocks or rewards because normal playresult has those fields.

**Phase to address:**
Phase 1: evidence inventory and unknowns ledger. Phase 3: persistence only for proven readback. Phase 5: cabinet/RPCS3 validation.

---

### Pitfall 5: Copying public service restrictions into the local server

**What goes wrong:**
Tokkun remains unavailable because the server enforces public service constraints such as Banacoin availability, extra paid-coin checks, or time-of-day restrictions. The milestone requires enough Banacoin behavior so Tokkun can always be played.

**Why it happens:**
The wiki says Tokkun required Bandai Namco Coin, could be unavailable on some cabinets, and had time restrictions. Those are production service rules, not necessarily local server requirements.

**How to avoid:**
Default the local Blue server toward "playable" stubs. Return success-shaped Banacoin responses and do not enforce service windows. If the client has a config/attract-screen gate, satisfy the minimum fields it checks. Document any deliberately ignored service rule as a local-server compatibility decision.

**Warning signs:**
- Tests use wall-clock time to make Tokkun unavailable.
- Balance/payment stubs return failure when no local Banacoin balance exists.
- A config setting is added that defaults Tokkun/Banacoin unavailable.

**Phase to address:**
Phase 2: Banacoin and availability stubs. Phase 5: smoke verification that Tokkun is selectable without real Banacoin state.

---

### Pitfall 6: Implementing `getbanacoininfo.php` from proto alone

**What goes wrong:**
The Blue route surface grows a new endpoint that the cabinet never calls, or the endpoint returns guessed identity/payment data that later conflicts with actual client flow.

**Why it happens:**
`proto/blue/taiko.proto` defines `GetbanacoininfoRequest` and `GetbanacoininfoResponse`, but `BlueRouteSkeletonTests` explicitly excludes `/v10r03/chassis/getbanacoininfo.php`. The bounded IDA query for this research found route strings for `chassis/balancecheck.php`, `chassis/banacoinpayment.php`, and `chassis/banacoinerrorlog.php`, but not `getbanacoininfo`.

**How to avoid:**
Do not add `getbanacoininfo.php` until a cabinet/RPCS3 log, IDA route string, or route callback trace proves the Blue client requests it and that absence blocks Tokkun. If needed, add a stateless Blue controller, update `BlueRouteSkeletonTests`, and return only fields proven necessary for progression.

**Warning signs:**
- Route skeleton is updated before there is a failing log containing `/getbanacoininfo.php`.
- The endpoint is backed by Mediator or persistence on first implementation.
- Tests assert detailed `GetbanacoininfoResponse` identity fields without client evidence.

**Phase to address:**
Phase 2: route/stub decision. Phase 5: verify no hidden missing-route failures in cabinet/RPCS3 logs.

---

### Pitfall 7: Confusing `play_mode`, `stage_mode`, battle, Waiwai, and Tokkun

**What goes wrong:**
Tokkun is detected from the wrong field, or a Tokkun request is misclassified as battle/normal/Dani. This can either corrupt state or silently discard the only data needed for Tokkun readback.

**Why it happens:**
Current `PlayMode` enum has `Normal = 0`, `DanMode = 1`, `GaidenMode = 4`, and `AiBattle = 6`; it has no Tokkun entry. Existing battle classification uses battle-specific optional sections, not `PlayMode`. The Blue binary mode label table observed by the IDA daemon includes `tokkun` at index 8 and `waiwai` at index 9, but exact runtime classification still needs request-log confirmation.

**How to avoid:**
Define a Blue Tokkun classification contract before implementation. Candidate signals are `play_mode`, `ary_tokkunstage_info`, and `tokkun_tutorial_flg`; logs decide precedence. If signals conflict, log the full request and return success without writing normal/battle state until the conflict is understood. Add tests for normal, Dani, battle, Tokkun, Waiwai-like unknown, and malformed mixed requests.

**Warning signs:**
- Tokkun detection uses `stage_mode`.
- `PlayMode` enum is updated globally without Blue-specific evidence.
- Battle classification changes while adding Tokkun.
- Mixed battle plus Tokkun fields enter the battle persistence path without an explicit test.

**Phase to address:**
Phase 1: classification evidence. Phase 4: handler implementation and conflict tests.

---

### Pitfall 8: Hard-failing unknown Tokkun stage modes or optional sections

**What goes wrong:**
The cabinet upload fails, retries, or blocks because the server rejects a Tokkun request with unfamiliar `stage_mode`, missing ordinary stage rows, optional Tokkun sections, or unsupported mode combinations.

**Why it happens:**
Normal Blue stage validation only supports known normal/shin stage modes and skips unsupported stages while still returning success. Tokkun may carry different mode values or summary-only sections. A strict implementation could turn an unknown into a failure response or exception.

**How to avoid:**
Preserve the current Blue principle: unsupported or unknown mode data should be logged and acknowledged with success unless the client evidence proves a failure response is required. Tokkun classification should happen before normal stage validation; unknown Tokkun details should not be routed through normal `IsSupportedBlueStage`.

**Warning signs:**
- New tests expect result `0`, HTTP 400, or exception for unknown Tokkun mode data.
- Logs show repeated `playresult.php` retries after Tokkun.
- `IsSupportedBlueStage` grows Tokkun-specific guesses instead of a Tokkun branch.

**Phase to address:**
Phase 4: playresult handling. Phase 5: cabinet/RPCS3 retry-log check.

---

### Pitfall 9: Losing full direct-protobuf request evidence

**What goes wrong:**
Unknown Tokkun fields cannot be diagnosed because logging is reduced to a summary, raw optional sections are not captured, or Banacoin/Tokkun requests are not logged consistently.

**Why it happens:**
Blue game endpoints use direct protobuf bodies. `PlayResultController` currently logs `request.Stringify()` before mapping. Prior Blue work established that full request logging matters because optional direct-protobuf sections can carry the only evidence for new modes.

**How to avoid:**
Keep full request logging in Blue `playresult.php` and Banacoin controllers. If logs are too large, add targeted structured fields in addition to the full request, not instead of it. Phase evidence should include sample Tokkun `playresult`, balance, payment, and any missing-route logs.

**Warning signs:**
- `PlayResultController` changes from full `Stringify()` to selected fields.
- Tokkun mapper tests are written before any saved request sample exists.
- Banacoin controllers stop logging personid/mode/price request shape.

**Phase to address:**
Phase 1: logging/evidence capture. Phase 5: final smoke evidence.

---

### Pitfall 10: Using battle implementation patterns outside their proven boundary

**What goes wrong:**
Tokkun is implemented as another store-and-echo battle-like state machine, or battle persistence is touched while adding Tokkun. That risks breaking completed Blue battle behavior and reintroducing normal-state corruption.

**Why it happens:**
Battle mode recently added Blue-owned persistence and a proven store-and-echo boundary. Tokkun also has optional playresult sections, so the battle pattern can look reusable. But battle ID contracts, token rows, `release_info_flg`, and `release_battle_stage_flg` are battle-specific and were proven through separate IDA traces.

**How to avoid:**
Keep Tokkun state separate from `BlueBattle*` entities and helpers unless direct evidence says a field is shared. Do not modify battle token, NPC, stage, or release-info handling during Tokkun work. If Tokkun needs persistence, create Blue Tokkun-specific entities/DTOs with focused tests.

**Warning signs:**
- Tokkun code calls `ApplyBlueBattleReleaseDataAsync` or writes `BlueBattleStageResult`.
- A Tokkun migration changes existing battle tables.
- Battle tests are updated to accommodate Tokkun instead of remaining unchanged.

**Phase to address:**
Phase 3: Tokkun persistence design. Phase 4: playresult implementation with battle regression tests.

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Delete old Tokkun guards | Unblocks compile quickly | Guessed semantics can spread through normal handlers | Never without replacement guards in the same phase |
| Store raw Tokkun request as the only state | Fastest persistence | Hard to query, hard to migrate, easy to treat opaque data as truth | Acceptable only as temporary evidence capture, not runtime readback |
| Add Banacoin tables | Looks complete | Creates unsupported payment system and future cleanup burden | Never for this milestone |
| Add `getbanacoininfo.php` because proto exists | Avoids hypothetical 404 | Expands route surface without client proof | Only after blocking cabinet/RPCS3 evidence |
| Treat `play_mode = 8` as final from IDA table only | Easy classifier | Could misclassify if request logs differ | Use as hypothesis until verified with logs |

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Blue `playresult.php` | Map Tokkun fields into normal `CommonPlayResultData` and let normal handler run | Add explicit Tokkun classification and a dedicated handler branch |
| Blue Banacoin routes | Persist balance/payment/coupon state | Stateless success-shaped responses, full request logs, no DB writes |
| Blue userdata/BAID | Guess `tokkun_tutorial_flg` default | Decide from Blue logs/IDA; test the chosen omit/zero/persist behavior |
| Route skeleton | Add `getbanacoininfo.php` from proto alone | Add only if a real client call is observed and blocks Tokkun |
| Battle state | Reuse battle store/echo helpers | Keep Tokkun and battle state separate unless proven shared |

## "Looks Done But Isn't" Checklist

- [ ] **Mode selection:** Tokkun can be selected without real Banacoin balance, not merely compiled.
- [ ] **Playresult classification:** Tokkun result returns success and does not write normal score, best, crown, Dani, favorite, customization, unlock, shop, or battle state.
- [ ] **Banacoin boundary:** Balance/payment/error routes remain stateless; no Banacoin migration or entity exists.
- [ ] **Tutorial/readback:** `tokkun_tutorial_flg` behavior is evidence-backed and tested, not silently omitted by stale v1.0 guards.
- [ ] **Unknown modes:** Unsupported Tokkun or mixed optional sections log and return success instead of hard-failing.
- [ ] **Full evidence:** Sample full direct-protobuf Tokkun and Banacoin request logs are attached to the phase verification notes.
- [ ] **Route surface:** `getbanacoininfo.php` is still excluded unless a blocking client log proves it is needed.
- [ ] **Regression:** Existing Blue normal, Dani, shop, and battle tests still pass without broad expectation changes.

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Tokkun falls through normal playresult | Phase 1 and Phase 4 | Unit tests prove Tokkun leaves normal/battle/Dani/shop/favorite state unchanged |
| Old source guards block or vanish | Phase 1, Phase 3, Phase 4 | Guard tests allow only dedicated Tokkun paths and still ban guessed semantics |
| Banacoin persistence | Phase 2 | No Banacoin entities/migrations; controller tests assert stateless success responses |
| Guessed practice semantics | Phase 1 and Phase 3 | Unknowns ledger plus tests limited to proven fields |
| Public service restrictions block local play | Phase 2 | Cabinet/RPCS3 can enter Tokkun without real Banacoin balance or time gate |
| Proto-only `getbanacoininfo.php` | Phase 2 | Route skeleton remains unchanged unless failing log proves route need |
| Misclassification across modes | Phase 1 and Phase 4 | Tests cover normal, Dani, battle, Tokkun, unknown, and mixed requests |
| Unknown mode hard-fail | Phase 4 | Tests assert success plus warning logs for unsupported Tokkun shapes |
| Lost full request evidence | Phase 1 and Phase 5 | Verification artifacts include full request logs |
| Battle state contamination | Phase 3 and Phase 4 | Battle regression tests unchanged; no Tokkun writes to `BlueBattle*` tables |

## Sources

- `.planning/PROJECT.md` - v1.1 Tokkun scope, active requirements, evidence hierarchy, Banacoin non-storage rule.
- `.planning/MILESTONES.md` - v1.0 Blue support shipped baseline.
- `Tests/Blue/BlueA4SourceGuardTests.cs` - current Tokkun source guard that must be retired carefully.
- `Tests/Blue/BluePlayResultHandlerTests.cs` - current normal-state write behavior and unsupported-stage success behavior.
- `Tests/Blue/BluePlayResultMapperTests.cs` - current mapper ignores unimplemented Tokkun optional sections.
- `Tests/Blue/BlueRouteSkeletonTests.cs` - current Blue route surface and explicit `getbanacoininfo.php` exclusion.
- `Tests/Blue/BlueMapperTests.cs` - current userdata Tokkun tutorial omission guard.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - normal Blue playresult write path.
- `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs` and `Application/Dtos/CommonPlayResultData.BlueBattle.cs` - battle-specific diversion and persistence boundary.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - current battle classification and optional section mapping.
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` - full direct-protobuf Blue playresult logging.
- `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs`, `BanacoinPaymentController.cs`, `BanacoinErrorLogController.cs` - current stateless Banacoin responses.
- `proto/blue/taiko.proto` - `tokkun_tutorial_flg`, `ary_tokkunstage_info`, `TokkunstageData`, and Banacoin message shapes.
- `.tools/blue/battleuserdata-response-xrefs.md` - battle-specific IDA evidence boundaries that should not be generalized to Tokkun.
- IDA daemon request `req_tokkun_pitfalls_20260603_codex` using `.tools/blue/scratch/req_tokkun_pitfalls_20260603_codex.py` - found Tokkun mode/assets/task strings, mode label table entry `tokkun` at index 8, route strings for balance/payment/error, and no `getbanacoininfo` route string hit.
- Wiki context: https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/%E5%9F%BA%E6%9C%AC%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0#tokkunmode - gameplay scoping only; not protocol authority.

---
*Pitfalls research for: Blue Tokkun mode support*
*Researched: 2026-06-03*
