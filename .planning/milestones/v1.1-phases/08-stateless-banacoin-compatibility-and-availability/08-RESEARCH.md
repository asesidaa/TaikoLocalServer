# Phase 8 Research: Stateless Banacoin Compatibility and Availability

**Researched:** 2026-06-04
**Status:** Ready for planning
**Scope:** Blue `/v10r03/chassis/*` Banacoin-adjacent route compatibility with no wallet/payment persistence.

## Summary

Phase 8 should implement the smallest Blue-owned stateless compatibility surface:

- Add `/v10r03/chassis/getbanacoininfo.php` as a direct-protobuf Blue controller that logs the request and returns only `GetbanacoininfoResponse { Result = 1 }`.
- Keep existing `heartbeat.php`, `balancecheck.php`, `banacoinpayment.php`, and `banacoinerrorlog.php` stateless.
- Preserve existing success values unless IDA evidence proves a failing branch or a better stateless value.
- Do not add Mediator handlers, EF entities, migrations, AdminApi/WebUI work, configuration, BNID/CHID state, transaction logs, coupons, deductions, balances, or real Banacoin semantics.

The important Phase 8 correction from `08-CONTEXT.md` is that `getbanacoininfo.php` route existence is no longer the hard gate. The route may be added even if later unused. The hard gate is response-value evidence: optional `GetbanacoininfoResponse` fields and existing Banacoin success/status values must not be guessed from names.

## Evidence Sources

### Current Code

| Area | Evidence |
|------|----------|
| Heartbeat route | `Adapters.GameProtocol.Blue/Controllers/HeartbeatController.cs` returns `Result = 1`, `ComSvrStat = 1`, `GameSvrStat = 1`, `BnidSvrStat = 1`, `BanacoinStat = 1`. |
| Balance route | `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs` returns `Result = 1`, echoes `Personid`, sets `BnidResult = string.Empty`, `CoinCoupon = 0`. |
| Payment route | `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs` returns `Result = 1`, echoes `Personid`, sets `BnidResult = string.Empty`, `Chid = string.Empty`. |
| Error-log route | `Adapters.GameProtocol.Blue/Controllers/BanacoinErrorLogController.cs` returns `Result = 1`. |
| Missing info route | `Tests/Blue/BlueRouteSkeletonTests.cs` currently excludes `/v10r03/chassis/getbanacoininfo.php`. |
| Wire support | `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` already define `GetbanacoininfoRequest` and `GetbanacoininfoResponse`. No generated wire edits are needed. |

### IDA Probes

All IDA work used the shared Blue daemon through `.tools/blue/idadrv.py run`. The daemon was already checked and started before probing. Do not open `.tools/blue/EBOOT.ELF.i64` independently while this daemon is running.

Scratch probes used:

- `.tools/blue/scratch/req_phase8_banacoin_routes.py`
- `.tools/blue/scratch/req_phase8_banacoin_handlers.py`
- `.tools/blue/scratch/req_phase8_getbanacoininfo_deep.py`
- `.tools/blue/scratch/req_phase8_descriptor_refs.py`

Key findings:

| Subject | IDA Evidence | Interpretation |
|---------|--------------|----------------|
| `heartbeat.php` route | String `chassis/heartbeat.php` at `0xfc12c8`, route table reference `0x10ff484`. | Real Blue route string exists. |
| `balancecheck.php` route | String `chassis/balancecheck.php` at `0xfc1240`, route table reference `0x10ff470`. | Real Blue route string exists. |
| `banacoinpayment.php` route | String `chassis/banacoinpayment.php` at `0xfc1260`, route table reference `0x10ff474`. | Real Blue route string exists. |
| `banacoinerrorlog.php` route | String `chassis/banacoinerrorlog.php` at `0xfc0448`, route table reference `0x10fed1c`. | Real Blue route string exists. |
| `getbanacoininfo.php` route | No `chassis/getbanacoininfo.php` route string found in focused string/xref probes. | Route-use remains unproven by route string evidence, but Phase 8 context permits adding a stateless compatibility route. |
| `Getbanacoininfo*` messages | Descriptor strings `GetbanacoininfoRequest` at `0xfc608f`, `GetbanacoininfoResponse` at `0xfc611e`, field descriptor strings at nearby addresses. | Schema/descriptors exist; no response consumer or route callback was identified. |
| Balance response consumer | `billing::OnBalanceCheckResponse` decompiled at `0x21854`. | `Result == 1` and non-8-character `bnid_result` are the accepted path; `coin_coupon` is passed to entry flow. |
| Payment response consumer | `billing::OnBanaCoinPaymentResponse` decompiled at `0x20f0c`. | `Result == 1` is success. `bnid_result` length and `chid` are logged/used in billing flow, but empty `bnid_result` maps to the accepted non-error path. |
| Entry balance callback | `EntryNetwork::OnBanacoinBalanceCheckResponse` decompiled at `0x25e980`. | Receives success bool and coin count; stores coin count into active entry state when present. No client persistence requirement is implied. |
| Error-log request path | `BanaCoinErrorlogInfo` request/store/serialize functions identified. | Error log is request-side buffered/logged; response has only `Result`. |

## Response Field Use And Fail Maps

### `GetbanacoininfoResponse`

No focused IDA probe found a route string, requester callback, or response consumer for `getbanacoininfo.php`. Because the generated class marks only `result` as required and every other field as optional, the safest Phase 8 response is to serialize `Result = 1` only and omit all optional fields. This satisfies the Phase 8 context requirement to make the route available while avoiding invented identity/account semantics.

| Field | Parser Location | Downstream Use | Fail Condition | Chosen Stateless Value |
|-------|-----------------|----------------|----------------|------------------------|
| `result` | Generated descriptor exists; no route-specific consumer identified. | Required wire success field. | Unknown, but all related Blue route success values use `1`. | `1` |
| `player_type` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `com_svr_result` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `mb_id` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `baid` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `access_code` | Descriptor only. | Unknown. Echoing request identity is not proven. | Unknown. | Omit optional field. |
| `is_publish` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `card_own_num` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `reg_country_id` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `purpose_id` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `region_id` | Descriptor only. | Unknown. | Unknown. | Omit optional field. |
| `personid` | Descriptor only. | Unknown. Identity synthesis is not proven. | Unknown. | Omit optional field. |

### `HeartBeatResponse`

| Field | Parser Location | Downstream Use | Fail Condition | Chosen Stateless Value |
|-------|-----------------|----------------|----------------|------------------------|
| `result` | Heartbeat response receiver string/table found; route string exists. | General request success. | Non-success logs response error. | Keep `1`. |
| `com_svr_stat` | Heartbeat response schema/receiver. | Availability status. | Unknown exact fail values. | Keep `1`. |
| `game_svr_stat` | Heartbeat response schema/receiver. | Availability status. | Unknown exact fail values. | Keep `1`. |
| `bnid_svr_stat` | Heartbeat response schema/receiver. | Availability status. | Unknown exact fail values. | Keep `1`. |
| `banacoin_stat` | Heartbeat response schema/receiver. | Banacoin availability status. | Unknown exact fail values. | Keep `1`. |

### `BalancecheckResponse`

IDA decompile of `billing::OnBalanceCheckResponse` at `0x21854`:

- Logs `result`.
- If `result != 1`, calls entry callback with failure.
- Logs `personid`, `bnidresult`, and `banacoin`.
- If `bnid_result` length is `8`, also calls entry callback with failure.
- Otherwise calls entry callback with success and passes `coin_coupon`.

| Field | Parser Location | Downstream Use | Fail Condition | Chosen Stateless Value |
|-------|-----------------|----------------|----------------|------------------------|
| `result` | `billing::OnBalanceCheckResponse`, `0x21854`, reads `a1[3]`. | Gates success callback. | Any value other than `1`. | Keep `1`. |
| `personid` | `0x21854`, reads/logs `a1[2]`. | Logging/context. | No fail branch identified. | Echo request `Personid`. |
| `bnid_result` | `0x21854`, reads/logs `a1[5]`, checks length via `sub_639EC0`. | Failure marker if length is 8. | Length exactly `8`. | Keep empty string. |
| `coin_coupon` | `0x21854`, reads/logs `a1[4]`; `EntryNetwork::OnBanacoinBalanceCheckResponse` at `0x25e980` stores it into entry state when success. | Entry availability/coin count. | No zero-specific fail branch identified in this probe. | Keep `0` until cabinet/RPCS3 proves a nonzero needed. |

### `BanacoinpaymentResponse`

IDA decompile of `billing::OnBanaCoinPaymentResponse` at `0x20f0c`:

- `result == 1` enters success handling.
- `result == 103` has special handling.
- Other result values take failure/error handling.
- On success, `personid`, `bnidresult`, and `settlementid` (`chid`) are logged/used.
- Empty `bnid_result` gives length `0`; this is accepted by the same "no BNID error" style branch used by balance.

| Field | Parser Location | Downstream Use | Fail Condition | Chosen Stateless Value |
|-------|-----------------|----------------|----------------|------------------------|
| `result` | `billing::OnBanaCoinPaymentResponse`, `0x20f0c`, reads `a1[5]`. | Gates success/error payment response path. | Values other than `1` enter non-success paths; `103` is special non-normal handling. | Keep `1`. |
| `personid` | `0x20f0c`, reads/logs `a1[2]`. | Logging/context. | No fail branch identified. | Echo request `Personid`. |
| `bnid_result` | `0x20f0c`, reads/logs `a1[3]`, length influences payment state. | BNID result state, but real BNID state is out of scope. | Non-empty/error-looking values may alter payment state. | Keep empty string. |
| `chid` | `0x20f0c`, reads/logs `a1[4]` as settlement id. | Settlement id context. | No requirement for local settlement id identified. | Keep empty string. |

### `BanacoinerrorlogResponse`

| Field | Parser Location | Downstream Use | Fail Condition | Chosen Stateless Value |
|-------|-----------------|----------------|----------------|------------------------|
| `result` | Request buffering/serialization found for `BanaCoinErrorlogInfo`; response has only required `result`. | Acknowledges error-log upload. | Unknown, but related success responses use `1`. | Keep `1`. |

## Implementation Recommendations

1. Add `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs` only.
2. Return `new GetbanacoininfoResponse { Result = 1 }`.
3. Log the full request with `request.Stringify()`.
4. Update `Tests/Blue/BlueRouteSkeletonTests.cs` to make `getbanacoininfo.php` an owned Blue route and remove it from excluded/shared routes.
5. Do not add constant-value tests for every response field. The useful tests are route ownership and no-Mediator/no-persistence guards already present or easy to extend.
6. Run focused route tests and a temp-output Host build. Phase 8 context says automated verification is build-only, but route skeleton tests are appropriate because the phase changes route ownership.

## Verification Scope

Recommended commands:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRouteSkeletonTests
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase8"
```

Optional broader check if build outputs are not locked:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
```

Do not claim cabinet/RPCS3 Tokkun proof in Phase 8. Phase 11 owns live game proof.

## Planning Implications

This phase should be one execution plan in wave 1. The plan can safely be autonomous because the decisions are locked in `08-CONTEXT.md` and the implementation surface is narrow.

The plan must include:

- Research artifact preservation.
- Stateless controller addition.
- Route skeleton update.
- Source/persistence guard checks that no EF/Mediator/stateful Banacoin code was added.
- Focused route test and temp-output Host build verification.

## Research Complete

The route and response-value evidence is sufficient for planning Phase 8. Remaining uncertainties are deliberately assigned to later cabinet/RPCS3 smoke evidence and must not block this narrow stateless route implementation.
