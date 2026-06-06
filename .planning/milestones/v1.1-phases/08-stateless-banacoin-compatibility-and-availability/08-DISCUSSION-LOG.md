# Phase 8: Stateless Banacoin Compatibility and Availability - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md. This log preserves the alternatives considered.

**Date:** 2026-06-04T04:29:27.2447349+08:00
**Phase:** 8-Stateless Banacoin Compatibility and Availability
**Areas discussed:** getbanacoininfo.php proof gate, Compatibility response shape, Banacoin request visibility, Phase 8 live-proof threshold

---

## getbanacoininfo.php Proof Gate

| Question | Options Presented | User's Choice / Notes |
|----------|-------------------|-----------------------|
| What proof should Phase 8 require before adding the Blue getbanacoininfo.php route? | Call plus block; IDA route proof; Keep absent | Initial selection: IDA route proof. |
| What level of IDA proof should count for getbanacoininfo.php route availability? | Tokkun-flow xref; Any route xref; Schema enough | User rejected the framing as too narrow. IDA evidence must check response parsing, value use, fail conditions, and valid response values. Route availability is not the main issue; add the route with at least `Result = 1` even if unused. |
| What should Phase 8 require before choosing values beyond `Result = 1`? | Field-use table; Parser notes; Stub only | Field-use table. |
| Until the field-use table proves optional values are needed, how should optional response fields be treated? | Omit optionals; Echo identity; Populate neutral | Omit optionals. |
| What should Phase 8 tests prove for getbanacoininfo.php once the route is added? | Route and stateless; Route only; Full field values | Route and stateless. Later verification discussion narrowed Phase 8 verification to build-only and no low-value tests. |

**Notes:** The final context treats the route as in scope to add. The hard gate moved from route availability to field-level response parser/use/fail-condition evidence.

---

## Compatibility Response Shape

| Question | Options Presented | User's Choice / Notes |
|----------|-------------------|-----------------------|
| How should Phase 8 decide values for existing Banacoin-adjacent responses? | Trace then set; Preserve current; Max permissive | Trace then set. |
| How should Phase 8 handle `BalancecheckResponse.CoinCoupon`? | Parser-driven; Positive allowance; Keep zero | Parser-driven. |
| How should Phase 8 handle `BnidResult` and `Chid` in `banacoinpayment.php`? | Trace fail use; Always empty; Synthetic values | Trace fail use. |
| How should Phase 8 document success/status codes? | Fail-map table; Current constants; Minimal note | Fail-map table. |

**Notes:** Response values are neither current-code defaults nor guessed permissive values. The planner must ground meaningful values in client parser and fail-condition evidence.

---

## Banacoin Request Visibility

| Question | Options Presented | User's Choice / Notes |
|----------|-------------------|-----------------------|
| What should Phase 8 expose so operators/developers can inspect the request sequence? | Sanitized sequence; Existing logs; Evidence note | Existing logs. |
| How should Phase 8 handle request/log evidence in artifacts? | Sanitized summaries; Raw excerpts ok; No excerpts | Raw excerpts ok. |
| Where should raw Banacoin request excerpts live if needed? | Local only; Redacted docs; No raw use | User rejected the security framing: this project treats game-accepted request identifiers as meaningless and moddable, not sensitive. |
| What artifact should Phase 8 produce so later phases can inspect the Banacoin sequence? | Sequence table; Verification note; No artifact | User clarified that Phase 8 will not run the game; any artifact is pure client evidence. Sequence does not matter, and endpoints must never rely on specific sequence. |
| Should Phase 8 tests explicitly protect that endpoints do not depend on call order? | Assert independent; Stateless only; Docs only | User rejected tests here. Sequence/order tests are meaningless; the client cares about valid values. |

**Notes:** Use existing controller logs. Do not add a new sequence logger. Do not create sequence/order tests. Focus on client evidence and valid response values.

---

## Phase 8 Live-Proof Threshold

| Question | Options Presented | User's Choice / Notes |
|----------|-------------------|-----------------------|
| Since Phase 8 will not run the game, what is the done condition? | Client evidence plus implementation; Evidence only; Implementation only | Client evidence plus implementation. |
| What automated verification should Phase 8 require after implementation? | Build only; Focused tests; Full test pass | Build only. |
| How should Phase 8 hand off cabinet/RPCS3 proof to Phase 11? | Phase 11 owns it; Optional now; Block now | Phase 11 owns it. |
| What client-evidence scope is required before Phase 8 can choose final response values? | Response-use trace; Fail branches only; Current route code | Response-use trace. |

**Notes:** Phase 8 should make no live-game claim. It closes with IDA/client parser-use evidence, implementation, and build verification. Phase 11 owns final cabinet/RPCS3 Tokkun proof.

---

## the agent's Discretion

- Keep the implementation in the existing Blue direct-protobuf controller style.
- Keep Banacoin compatibility stateless and Blue-owned.
- Do not add runtime payment abstractions, persistence, AdminApi/WebUI surfaces, or low-value sequence/constant tests.

## Deferred Ideas

None.
