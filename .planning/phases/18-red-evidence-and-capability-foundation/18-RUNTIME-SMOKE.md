# Phase 18 Red Runtime Smoke

**Created:** 2026-06-13
**Status:** passed/user-confirmed-basic-connection
**Scope:** Red request-routing smoke for Phase 18 no-state probes only.

## Purpose

This record captures the user-run RPCS3/cabinet request-routing smoke needed before Phase 18 can be called runtime-verified.

The Phase 18 probes are not Red gameplay support. They only prove that the client can reach shared `/v01r00/chassis/*` startup/version routes and IDB-known `/v08r01/chassis/*` Red game-route probes, that generated Red protobuf request binding works, and that the minimal response shapes do not block evidence collection before enough routing logs are captured.

## Current Result

| Field | Value |
|-------|-------|
| Runtime smoke status | `passed/user-confirmed-basic-connection` |
| Runtime smoke date | 2026-06-13 |
| Verified by | User RPCS3/cabinet confirmation in Codex session |
| Server commit under test | Split Red route-probe controller working tree after `dcbad846` and `4a79d6cf` |
| Red enabled setting | `Host/Configurations/ServerSettings.json` has `ServerSettings:Eras:Red:Enabled = true` |
| Game route prefix under test | `/v08r01/chassis/*` |
| Shared startup/version prefix under test | `/v01r00/chassis/*` |

User confirmed that after splitting Red route probes into per-route controllers and rebuilding from this repository, the Red client connects successfully. Card scan was intentionally not performed because it requires later gameplay/profile support outside Phase 18.

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
| 1 | observed | `/v01r00/chassis/startupauth.php` | Shared startup/version | `Host/Logs/log-20260613.txt` shows HTTP 200 before the Red route-probe fix. |
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
| 18 | observed after fix | `/v08r01/chassis/initialdatacheck.php` | Red route probe | Previously returned 405 with the single-file controller shape; user confirmed the basic connection succeeds after split/rebuild. |
| 19 | observed after fix | `/v08r01/chassis/tournamentcheck.php` | Red route probe | Previously returned 405 with the single-file controller shape; user confirmed the basic connection succeeds after split/rebuild. |
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
| 2026-06-13 before fix | `/v08r01/chassis/initialdatacheck.php` | POST | 405 | `Content-Type: application/protobuf`; fixed by splitting Red route probes and rebuilding Host metadata. | resolved |
| 2026-06-13 before fix | `/v08r01/chassis/tournamentcheck.php` | POST | 405 | `Content-Type: application/protobuf`; fixed by splitting Red route probes and rebuilding Host metadata. | resolved |

## Missing Content-Type / Direct Protobuf Check

| Question | Result | Evidence |
|----------|--------|----------|
| Did Red game requests omit `Content-Type`? | not observed | The captured Red requests included `Content-Type: application/protobuf`. Missing-content-type fallback remains configured for `/v08r01/chassis/*`. |
| Did Host assign `application/protobuf` for `/v08r01/chassis/*` requests? | not exercised by captured run | Captured requests already supplied protobuf content type. |
| Did generated Red `[FromBody]` request DTO binding succeed? | passed for basic connection | User confirmed basic Red connection succeeds after split/rebuild. |
| Did any request fail before reaching the controller? | resolved | Pre-fix 405s on `initialdatacheck.php` and `tournamentcheck.php` were resolved by the split/rebuild. |

## Minimal Response Shape Check

| Question | Result | Evidence |
|----------|--------|----------|
| Did heartbeat status fields allow the client to continue? | not reached in recorded evidence | Basic connection passed; full gameplay/card flow remains out of Phase 18 scope. |
| Did Banacoin-adjacent required `personid` echoes serialize correctly where reached? | not reached in recorded evidence | Basic connection passed; payment semantics remain absent by design. |
| Did any empty list/minimal success response block further routing evidence? | no basic-connection blocker reported | User confirmed basic connection succeeds. |
| Did any route require additional fields before enough evidence was captured? | no Phase 18 blocker reported | Card scan/gameplay progression intentionally not tested because later support is required. |

## User Confirmation

Fill this section only from user-provided runtime evidence.

| Field | Value |
|-------|-------|
| Confirmed by user | yes |
| Confirmation date | 2026-06-13 |
| RPCS3/cabinet build | Red AC15 client under local RPCS3/cabinet smoke |
| Startup/version observations | Shared startup/auth route reached and returned 200 in `Host/Logs/log-20260613.txt`; user later confirmed basic connection succeeds after fix. |
| Red game-route observations | `initialdatacheck.php` and `tournamentcheck.php` were the initial Red route probes; pre-fix 405s resolved after split/rebuild. |
| Unknown routes | none reported for the successful basic-connection smoke |
| Binding/content-type result | generated Red route binding sufficient for basic connection; captured requests used `application/protobuf` |
| Response-shape blockers | none for Phase 18 basic connection; card scan intentionally not tested |
| Phase 18 runtime-smoke disposition | sufficient for Phase 18 close |

## Close Criteria

Phase 18 runtime smoke can be marked sufficient only when the user confirms one of these outcomes:

- Red route-probe logging is sufficient for Phase 18 close, with observed startup/version and game-route sequence recorded.
- Concrete routing or response-shape issues are identified, with enough detail to plan a narrow follow-up fix before close.

This criterion is now satisfied for Phase 18's basic connection scope. Deeper card scan, profile, and gameplay progression remain future-phase work.
