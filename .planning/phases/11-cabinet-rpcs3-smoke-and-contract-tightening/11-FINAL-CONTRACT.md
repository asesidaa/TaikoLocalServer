---
phase: 11-cabinet-rpcs3-smoke-and-contract-tightening
status: verified
completed: 2026-06-07
requirements:
  - TKVF-03
---

# Phase 11 Final Blue Tokkun Contract

This contract records the final v1.1 Blue Tokkun behavior after automated server verification and user-confirmed cabinet/RPCS3 runtime verification.

## Confirmed Runtime Surface

- Blue Tokkun uses Blue game routes under `/v10r03/chassis/*`.
- Shared AC15 startup/version routes remain under `/v01r00/chassis/*`.
- Blue game endpoint request bodies remain direct protobuf.
- `PlayMode.Tokkun = 3` is the confirmed Blue Tokkun play mode value.
- Tokkun playresults are classified before battle and normal Blue playresult writes.
- Tokkun runtime support uses the existing Blue `playresult.php`, `userdata.php`, Banacoin-adjacent compatibility routes, and Mucha token flags.

## Banacoin-Adjacent Compatibility

TaikoLocalServer does not implement real Banacoin wallet, payment, settlement, coupon, deduction, receipt, BNID account, or transaction-history semantics.

The final v1.1 compatibility surface is permissive and stateless:

| Surface | Final behavior |
|---------|----------------|
| Mucha download state | `USE_TOKEN = 1`, `CONSUME_TOKEN = 1` |
| `baidcheck.php` | Returns publish/access identity shape with `ComSvrResult = 1`, `Personid = "1"`, `RegCountryId = "JPN"`, `MbId = 1`, `PurposeId = 1`, and `RegionId = 1` |
| `getbanacoininfo.php` | Returns `Result = 1` only |
| `heartbeat.php` | Returns success-shaped heartbeat response |
| `balancecheck.php` | Echoes `Personid`, returns `Result = 1`, `BnidResult = "Ok"`, and `CoinCoupon = 9999` |
| `banacoinpayment.php` | Echoes `Personid`, returns `Result = 1`, `BnidResult = "Ok"`, and `Chid = "1"` |
| `banacoinerrorlog.php` | Logs request and returns success |

These routes do not write EF state, shared wallet state, Green/Nijiiro state, AdminApi state, or WebUI state.

## Persisted Tokkun State

Protocol-facing readback is limited to the tutorial flag:

- `UserSaveDataBlue.TokkunTutorialFlg`
- Raw nullable `uint`
- Omitted from userdata when unset
- Updated only by Tokkun-classified uploads when the optional value is present

Server-side Tokkun history is append-only:

| Field | Source |
|-------|--------|
| `Baid` | Request BAID |
| `PlayDatetime` | Client protocol play datetime string |
| `PlayMode` | Blue playresult `play_mode` |
| `BanacoinDatetime` | Tokkun stage data protocol string |
| `TokkunSongCnt` | Tokkun stage data counter |
| `TookunSongnoesJson` | Raw ordered duplicate-preserving song list JSON |
| `TokkunSpeedchangeCnt` | Tokkun stage data counter |
| `TokkunAutoplayCnt` | Tokkun stage data counter |
| `TokkunJumpCnt` | Tokkun stage data counter |

## Explicit Non-Goals

Tokkun does not write or infer:

- Normal score, crown, play-count, or self-best state
- Dani state
- Battle state
- Favorites or recent songs
- Normal unlock state
- Medal totals from Tokkun practice semantics
- Customization/title state
- Item-shop purchase state
- Rewards, rankings, practice-time rules, jump-point rules, speed-change rules, autoplay rules, or song unlock side effects
- Real Banacoin balance, payment, receipt, coupon, deduction, BNID, CHID, or transaction-history state

## Runtime Verification Note

On 2026-06-07, the user confirmed Phase 11 runtime verification for cabinet/RPCS3 Tokkun behavior and approved marking the milestone complete. This confirmation covers Tokkun selection, Banacoin request sequence, gameplay entry, final upload, post-upload userdata behavior, and no unexpected blocking endpoint calls.

No local cabinet log file was added to the repository in Phase 11; the runtime evidence is recorded as user-confirmed external verification.
