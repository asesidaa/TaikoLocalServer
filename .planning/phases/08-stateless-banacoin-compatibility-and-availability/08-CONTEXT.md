# Phase 8: Stateless Banacoin Compatibility and Availability - Context

**Gathered:** 2026-06-04T04:29:27.2447349+08:00
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase implements stateless Blue Banacoin-adjacent compatibility needed for Tokkun entry. It should add and maintain permissive Blue `/v10r03/chassis/*` Banacoin-compatible endpoints without creating wallet, balance, coupon, payment, deduction, receipt, BNID, CHID, transaction-history, or cross-era state.

This phase does not run cabinet/RPCS3, does not prove Tokkun end-to-end, does not implement Tokkun playresult classification or persistence, and does not add real Banacoin behavior. Phase 8 must use client/IDA evidence to determine valid response values. Phase 11 owns live cabinet/RPCS3 proof.

</domain>

<decisions>
## Implementation Decisions

### getbanacoininfo.php Route And Evidence Model
- **D-01:** Add Blue `getbanacoininfo.php` as a stateless route with at least `Result = 1`. Route existence is not the hard gate, and the route may be added even if it later proves unused.
- **D-02:** The real evidence gate is response parsing and value use, not route availability. IDA/client research must check how the response is parsed, trace how each value is used, identify fail conditions, and select valid stateless values from that analysis.
- **D-03:** Before choosing values beyond required `Result = 1`, produce a field-use table for `GetbanacoininfoResponse`: response field, parser location, downstream use, fail condition, and chosen stateless value.
- **D-04:** Until the field-use table proves optional `getbanacoininfo.php` fields are needed, omit optional response fields. Do not echo identity fields or populate neutral optional values by default.

### Compatibility Response Shape
- **D-05:** Existing Banacoin-adjacent response values for `heartbeat.php`, `balancecheck.php`, `banacoinpayment.php`, and `banacoinerrorlog.php` must be trace-driven. Do not preserve current values blindly and do not choose "max permissive" values from field names alone.
- **D-06:** `BalancecheckResponse.CoinCoupon` is parser-driven. Keep `0` only if tracing shows it is accepted; change it only when fail-condition evidence identifies the valid stateless value.
- **D-07:** Payment-looking identity fields such as `BnidResult` and `Chid` stay empty/default unless IDA shows a parser or fail branch needs a specific stateless value. Do not persist BNID, CHID, or transaction state.
- **D-08:** Success/status values need a fail-map table. For each `result` or status field, record parser use, failing values, and the chosen success value.

### Logs, Evidence, And Sequence
- **D-09:** Use existing per-controller request logs as the operational visibility surface. Do not add a separate Banacoin sequence logger in Phase 8.
- **D-10:** Raw client/request excerpts are acceptable in this project context. The user does not treat game-accepted cabinet request identifiers as meaningful secrets because they can be modded. Do not introduce unrelated secrets such as API keys, passwords, or tokens.
- **D-11:** Phase 8 will not run the game. Any artifact produced in this phase is client evidence, parser evidence, or implementation evidence, not cabinet/RPCS3 sequence evidence.
- **D-12:** Banacoin endpoints must never rely on a specific call sequence. Do not create sequence/order tests; they are not useful for this phase because the client cares about valid response values.

### Proof Threshold And Verification
- **D-13:** Phase 8 is done with client evidence plus implementation: IDA/client parser-use evidence, valid stateless response values, route/controller implementation, and build verification.
- **D-14:** Automated verification for Phase 8 is build-only. Run a build to catch compile and route-wiring issues. Skip tests that merely assert server-side constants, call order, or low-value route behavior.
- **D-15:** Phase 11 owns cabinet/RPCS3 proof. Phase 8 must make no live-game claim and should hand final Tokkun entry/smoke proof to Phase 11.
- **D-16:** Before Phase 8 chooses final response values, trace parsing and downstream use for every response field that might affect success or failure.

### the agent's Discretion
- Maintain the existing Blue adapter/controller style and direct-protobuf route pattern.
- Keep implementation narrowly scoped to Blue Banacoin-adjacent compatibility and value evidence. Avoid new dependencies, EF entities, migrations, Mediator handlers, AdminApi/WebUI work, or payment abstractions.
- Existing tests that encode route ownership may need maintenance when the route is added, but Phase 8 should not add new low-value sequence or response-constant tests.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project And Phase Scope
- `.planning/PROJECT.md` - Current v1.1 Blue Tokkun scope, evidence hierarchy, Blue-owned state constraints, and Phase 8 direction.
- `.planning/REQUIREMENTS.md` - TKBC-01 through TKBC-03 Banacoin compatibility requirements and out-of-scope real Banacoin behavior.
- `.planning/ROADMAP.md` - Phase 8 goal and success criteria, plus Phase 9 through Phase 11 boundaries.
- `.planning/STATE.md` - Current workflow position and recent Phase 7 completion state.

### Tokkun And Banacoin Evidence Contract
- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md` - Phase 7 decisions, especially D-14 handoff to Phase 8 and no invented Banacoin semantics.
- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` - Row matrix for Banacoin route unknowns, `getbanacoininfo.php`, and follow-on gates.
- `.planning/research/SUMMARY.md` - v1.1 research summary and Phase 8 risk framing. Treat the old "keep absent unless proven" recommendation as superseded by this Phase 8 discussion for route existence only; response values still require proof.

### Codebase Maps
- `.planning/codebase/STACK.md` - Existing .NET, ASP.NET Core, protobuf-net, EF Core, and xUnit stack.
- `.planning/codebase/ARCHITECTURE.md` - Blue adapter/controller layering, direct protobuf path, state separation, and no cross-era leakage constraints.
- `.planning/codebase/INTEGRATIONS.md` - Blue `/v10r03/chassis/*` route surface, local storage/logging model, and no outbound Banacoin integration.

### Blue Protocol And Runtime Surface
- `proto/blue/taiko.proto` - Blue Banacoin message shapes, including `GetbanacoininfoRequest` and `GetbanacoininfoResponse`.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - Generated Blue wire classes and optional-field presence helpers for Banacoin responses.
- `Adapters.GameProtocol.Blue/Controllers/HeartbeatController.cs` - Current Blue heartbeat success/status response.
- `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs` - Current Blue balancecheck response including `CoinCoupon`.
- `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs` - Current stateless payment-like log/success response.
- `Adapters.GameProtocol.Blue/Controllers/BanacoinErrorLogController.cs` - Current stateless error-log response.
- `Tests/Blue/BlueRouteSkeletonTests.cs` - Existing route ownership expectations, including current exclusion of `getbanacoininfo.php`.

### Local Blue Evidence
- `.tools/blue/EBOOT.ELF.i64` - Local Blue IDA database for response parser/value-use tracing.
- `.tools/blue/idadrv.py` - Local IDA driver support material. Use the shared IDA workflow when probing client behavior.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs` and `BanacoinErrorLogController.cs` already show the desired stateless controller shape: direct protobuf request, log, success response, no Mediator/EF dependency.
- `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs` and `HeartbeatController.cs` already expose the current response fields that need client parser-use validation.
- `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` already define `GetbanacoininfoRequest` and `GetbanacoininfoResponse`; no manual wire edit is needed.
- Existing `request.Stringify()` controller logging is the chosen visibility surface.

### Established Patterns
- Blue routes live under `/v10r03/chassis/*` and remain direct protobuf.
- Controllers should stay thin. For these Banacoin-adjacent compatibility endpoints, direct stateless controller responses are preferred over Mediator handlers.
- Blue, Green, Nijiiro, and shared identity state must remain separate. Phase 8 should add no Banacoin persistence.
- Generated `Wire/` files are not manually cleaned up unless regenerating protocol output.

### Integration Points
- Add `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs` or equivalent Blue controller for `/v10r03/chassis/getbanacoininfo.php`.
- Update route ownership expectations if existing tests encode the old absence of `getbanacoininfo.php`.
- Use IDA/client evidence to build the `GetbanacoininfoResponse` and existing Banacoin response field-use/fail-map tables before finalizing optional or status values.
- Verify with a build, preferably the temp-output Host build if the normal Host output is locked.

</code_context>

<specifics>
## Specific Ideas

- The user explicitly corrected the initial route-gating framing: route availability is the latest issue and should not be the main discussion topic. Add `getbanacoininfo.php` with at least `Result = 1`.
- "IDA evidence" means thorough exploration: inspect response parsing, value use, fail conditions, and valid values. A route string or schema presence alone is not enough to choose field values.
- Phase 8 does not run the game. Client evidence and implementation are enough for this phase; cabinet/RPCS3 proof belongs to Phase 11.
- Endpoints should never depend on a specific request sequence.
- Avoid tests that only assert constants or call order. The client cares about valid values.

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within Phase 8 scope.

</deferred>

---

*Phase: 8-Stateless Banacoin Compatibility and Availability*
*Context gathered: 2026-06-04T04:29:27.2447349+08:00*
