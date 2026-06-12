# Phase 18 Red Runtime Smoke

**Created:** 2026-06-13
**Status:** pending/human_needed
**Scope:** Red request-routing smoke for Phase 18 no-state probes only.

## Purpose

This record captures the user-run RPCS3/cabinet request-routing smoke needed before Phase 18 can be called runtime-verified.

The Phase 18 probes are not Red gameplay support. They only prove that the client can reach shared `/v01r00/chassis/*` startup/version routes and IDB-known `/v08r01/chassis/*` Red game-route probes, that generated Red protobuf request binding works, and that the minimal response shapes do not block evidence collection before enough routing logs are captured.

## Current Result

| Field | Value |
|-------|-------|
| Runtime smoke status | `pending/human_needed` |
| Runtime smoke date | Not run in current local artifacts |
| Verified by | Pending user RPCS3/cabinet confirmation |
| Server commit under test | Pending fill during smoke |
| Red enabled setting | `Host/Configurations/ServerSettings.json` has `ServerSettings:Eras:Red:Enabled = true` |
| Game route prefix under test | `/v08r01/chassis/*` |
| Shared startup/version prefix under test | `/v01r00/chassis/*` |

Do not change this status to passed until the user supplies concrete runtime observations.

## Server Setup

Run from the repository root after automated verification passes:

```powershell
dotnet run --project Host
```

If the normal debug output is locked by an already-running server, use an equivalent local run or build from the temp-output Host artifact that was just verified. The runtime smoke should target the local TaikoLocalServer instance with Red enabled.

## Expected Log Evidence

The Red route probes log full generated request DTOs with this message shape:

```text
Red route probe <suffix> request: <request dto>
```

Examples:

```text
Red route probe playresult.php request: ...
Red route probe userdata.php request: ...
Red route probe challengecompe.php request: ...
Red route probe heartbeat.php request: ...
```

Unknown routes should appear through the Host 404 diagnostic logging:

```text
Unknown request from: <remote-ip> <method> <path> 404
Request headers: ...
```

## Route Sequence Capture

Fill this table from the RPCS3/cabinet run. Use exact timestamps if available, otherwise preserve observed order.

| Order | Observed | Route | Expected owner | Notes |
|-------|----------|-------|----------------|-------|
| 1 | pending | `/v01r00/chassis/startupauth.php` | Shared startup/version | Record whether request appeared and response allowed progress. |
| 2 | pending | `/v01r00/chassis/verupauth.php` | Shared startup/version | Record whether request appeared and response allowed progress. |
| 3 | pending | `/v01r00/chassis/verupcomplete.php` | Shared startup/version | Record whether request appeared and response allowed progress. |
| 4 | pending | `/v08r01/chassis/playresult.php` | Red route probe | Record whether generated binding/logging worked if reached. |
| 5 | pending | `/v08r01/chassis/banacoinerrorlog.php` | Red route probe | Record whether generated binding/logging worked if reached. |
| 6 | pending | `/v08r01/chassis/baidcheck.php` | Red route probe | Record whether generated binding/logging worked if reached. |
| 7 | pending | `/v08r01/chassis/mydonentry.php` | Red route probe | Record whether generated binding/logging worked if reached. |
| 8 | pending | `/v08r01/chassis/userdata.php` | Red route probe | Record whether generated binding/logging worked if reached. |
| 9 | pending | `/v08r01/chassis/challengecompe.php` | Red route probe | Probe only; do not infer ChallengeCompe semantics. |
| 10 | pending | `/v08r01/chassis/balancecheck.php` | Red route probe | Probe only; no balance authority. |
| 11 | pending | `/v08r01/chassis/banacoinpayment.php` | Red route probe | Probe only; no payment authority. |
| 12 | pending | `/v08r01/chassis/crownsdata.php` | Red route probe | Probe only; no crown state. |
| 13 | pending | `/v08r01/chassis/recommend.php` | Red route probe | Probe only; no catalog-backed recommendation support. |
| 14 | pending | `/v08r01/chassis/selfbest.php` | Red route probe | Probe only; no score state. |
| 15 | pending | `/v08r01/chassis/heartbeat.php` | Red route probe | Expected minimal status fields set to `1`. |
| 16 | pending | `/v08r01/chassis/rewardcardcheck.php` | Red route probe | Probe only; no unlock, shop, medal, or payment semantics. |
| 17 | pending | `/v08r01/chassis/rewardexecution.php` | Red route probe | Probe only; no reward state. |
| 18 | pending | `/v08r01/chassis/initialdatacheck.php` | Red route probe | Probe only; no feature advertisement semantics. |
| 19 | pending | `/v08r01/chassis/tournamentcheck.php` | Red route probe | Probe only; no tournament/gacha state. |
| 20 | pending | `/v08r01/chassis/bookkeeping.php` | Red route probe | Log-and-success probe only. |
| 21 | pending | `/v08r01/chassis/coinsetting.php` | Red route probe | Log-and-success probe only. |
| 22 | pending | `/v08r01/chassis/gettelop.php` | Red route probe | Probe only; telop binding belongs later. |
| 23 | pending | `/v08r01/chassis/getfolder.php` | Red route probe | Probe only; folder binding belongs later. |
| 24 | pending | `/v08r01/chassis/taikojuku.php` | Red route probe | Probe only; Dani/Taikojuku state belongs later. |
| 25 | pending | `/v08r01/chassis/headclerk2.php` | Red route probe | Log-and-success probe only. |

Routes may appear in a different order or not appear at all during a single smoke. The important evidence is the actual request sequence, successful route binding where reached, and any blockers.

## Unknown Route Capture

Record any route that returned 404 or appeared in Host unknown-request logs.

| Timestamp/order | Route | Method | Status | Headers/content-type notes | Follow-up needed |
|-----------------|-------|--------|--------|----------------------------|------------------|
| pending | pending | pending | pending | pending | pending |

## Missing Content-Type / Direct Protobuf Check

| Question | Result | Evidence |
|----------|--------|----------|
| Did Red game requests omit `Content-Type`? | pending | pending |
| Did Host assign `application/protobuf` for `/v08r01/chassis/*` requests? | pending | pending |
| Did generated Red `[FromBody]` request DTO binding succeed? | pending | pending |
| Did any request fail before reaching the controller? | pending | pending |

## Minimal Response Shape Check

| Question | Result | Evidence |
|----------|--------|----------|
| Did heartbeat status fields allow the client to continue? | pending | pending |
| Did Banacoin-adjacent required `personid` echoes serialize correctly where reached? | pending | pending |
| Did any empty list/minimal success response block further routing evidence? | pending | pending |
| Did any route require additional fields before enough evidence was captured? | pending | pending |

## User Confirmation

Fill this section only from user-provided runtime evidence.

| Field | Value |
|-------|-------|
| Confirmed by user | pending |
| Confirmation date | pending |
| RPCS3/cabinet build | pending |
| Startup/version observations | pending |
| Red game-route observations | pending |
| Unknown routes | pending |
| Binding/content-type result | pending |
| Response-shape blockers | pending |
| Phase 18 runtime-smoke disposition | pending |

## Close Criteria

Phase 18 runtime smoke can be marked sufficient only when the user confirms one of these outcomes:

- Red route-probe logging is sufficient for Phase 18 close, with observed startup/version and game-route sequence recorded.
- Concrete routing or response-shape issues are identified, with enough detail to plan a narrow follow-up fix before close.

Until then, the runtime smoke remains `pending/human_needed`.
