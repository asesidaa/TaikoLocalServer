# Phase 23 White Evidence Gate

**Created:** 2026-06-17
**Updated:** 2026-06-17 after route-proof checkpoint continuation
**Approval:** Task 3 human approval recorded on 2026-06-17 from user response `approved`.
**Scope:** White AC15 0.13 route, startup/version, transport, data-root, and feature-surface evidence before route/controller implementation.

## IDA Backend Evidence

| Artifact | Evidence | Status |
|----------|----------|--------|
| IDA target | `.tools/white/EBOOT.ELF.i64` | `AVAILABLE` |
| Backend probe | `database_opened=true`, `ida_available=true` | `AVAILABLE` |
| Live filesystem size | `129893515` bytes | `AVAILABLE` |

The earlier raw string scan result is superseded for route proof by the running White IDA daemon evidence below. The nonzero IDB size proves the evidence source is available; the route decisions come from the IDA service and route pointer tables, not from file size alone.

## Route Prefix Decision

| Question | Status | Evidence Source | Decision |
|----------|--------|-----------------|----------|
| Exact White game route prefix | `PROVEN_ROUTE_PREFIX` | White IDA daemon target `.tools/white/EBOOT.ELF.i64`: `v07r00` string at `0xc38468`; big-endian pointer at `0xcb0d98` in the same service table as the `v01r00` entry. Nearby table entries include `_uninitialized`, `_initialized`, `_nic_activated`, `_wait_terminate`, `_terminated`, `ResidentServiceThread`, `DelayServiceThread`, `PriorityServiceThread`, and `VersionupServiceThread`. | `/v07r00/chassis/{suffix}` is the proven White game route prefix for the Phase 23 no-state scaffold allowlist. Route code still waits for the Task 3 human approval checkpoint. |
| Shared startup/version route prefix | `PROVEN_SHARED_SERVICE_PREFIX` | White IDA daemon target `.tools/white/EBOOT.ELF.i64`: `v01r00` string at `0xc38470`; big-endian pointer at `0xcb0d9c` in the same service table. | Keep startup/version ownership under shared `/v01r00/chassis/*`. White Plan 03 must not duplicate these routes in the White adapter. |

## Route Suffix Gate

The following suffixes are the only White game route suffixes currently `SCAFFOLD_APPROVED` for Phase 23. Approval is bounded to no-state compatibility scaffolds under `/v07r00/chassis/{suffix}` with the Task 3 human approval recorded on 2026-06-17. Proto request/response names are cross-checks; IDA route table strings are the route authority.

| Endpoint suffix | Proto request | Proto response | White route evidence | Plan 03 treatment |
|-----------------|---------------|----------------|----------------------|-------------------|
| `playresult.php` | `PlayResultRequest` | `PlayResultResponse` | IDA route table has format string `%s/%s` at pointer table address `0xd2c898` -> string `0xc39548`; route string `chassis/playresult.php` at `0xc39550`; pointer/table entry `0xd2c89c`. | `SCAFFOLD_APPROVED`; no-state success/default response only. No score, crown, Dani, profile, favorite, unlock, reward, ChallengeCompe, shop, wallet, battle, Tokkun, WaiWai, or gacha writes. |
| `baidcheck.php` | `BAIDRequest` | `BAIDResponse` | Route string `chassis/baidcheck.php` at `0xc3a140`; pointer/table entry `0xd2cd60`. | `SCAFFOLD_APPROVED`; no-state compatibility response only. No identity persistence or profile creation. |
| `mydonentry.php` | `MydonEntryRequest` | `MydonEntryResponse` | Route string `chassis/mydonentry.php` at `0xc3a158`; pointer/table entry `0xd2cd64`. | `SCAFFOLD_APPROVED`; no-state compatibility response only. No MyDon state persistence. |
| `userdata.php` | `UserDataRequest` | `UserDataResponse` | Route string `chassis/userdata.php` at `0xc3a170`; pointer/table entry `0xd2cd68`. | `SCAFFOLD_APPROVED`; no-state/default readback only. No profile, catalog, reward, challenge, or unlock semantics. |
| `crownsdata.php` | `CrownsDataRequest` | `CrownsDataResponse` | Route string `chassis/crownsdata.php` at `0xc3a188`; pointer/table entry `0xd2cd6c`. | `SCAFFOLD_APPROVED`; no-state default response only. No crown persistence or packing claims. |
| `recommend.php` | `RecommendRequest` | `RecommendResponse` | Route string `chassis/recommend.php` at `0xc3a1a0`; pointer/table entry `0xd2cd70`. | `SCAFFOLD_APPROVED`; no-state/default recommendation response only. No catalog binding. |
| `selfbest.php` | `SelfBestRequest` | `SelfBestResponse` | Route string `chassis/selfbest.php` at `0xc3a1b8`; pointer/table entry `0xd2cd74`. | `SCAFFOLD_APPROVED`; no-state default response only. No self-best persistence. |
| `heartbeat.php` | `HeartBeatRequest` | `HeartBeatResponse` | Route string `chassis/heartbeat.php` at `0xc3a1d0`; pointer/table entry `0xd2cd78`. | `SCAFFOLD_APPROVED`; no-state heartbeat response only. |
| `initialdatacheck.php` | `InitialdatacheckRequest` | `InitialdatacheckResponse` | Second game route table has format string `%s/%s` at `0xd2cd9c` -> string `0xc3a3e0`; route string `chassis/initialdatacheck.php` at `0xc3a3e8`; pointer/table entry `0xd2cda0`. | `SCAFFOLD_APPROVED`; no-state initial-data probe response only. No catalog binding or advertisement semantics beyond safe defaults. |
| `tournamentcheck.php` | `TournamentcheckRequest` | `TournamentcheckResponse` | Route string `chassis/tournamentcheck.php` at `0xc3a408`; pointer/table entry `0xd2cda4`. | `SCAFFOLD_APPROVED`; no-state/default tournament probe only. No tournament state or rewards. |
| `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | Route string `chassis/bookkeeping.php` at `0xc3a428`; pointer/table entry `0xd2cda8`. | `SCAFFOLD_APPROVED`; no-state bookkeeping response only. No cabinet/accounting state. |
| `gettelop.php` | `GettelopRequest` | `GettelopResponse` | Route string `chassis/gettelop.php` at `0xc3a440`; pointer/table entry `0xd2cdac`. | `SCAFFOLD_APPROVED`; no-state/default telop response only. No catalog binding. |
| `getfolder.php` | `GetfolderRequest` | `GetfolderResponse` | Route string `chassis/getfolder.php` at `0xc3a458`; pointer/table entry `0xd2cdb0`. | `SCAFFOLD_APPROVED`; no-state/default folder response only. No event folder semantics. |
| `taikojuku.php` | `TaikojukuRequest` | `TaikojukuResponse` | Route string `chassis/taikojuku.php` at `0xc3a470`; pointer/table entry `0xd2cdb4`. | `SCAFFOLD_APPROVED`; no-state/default Taikojuku response only. No Dani/Taikojuku persistence or playresult classification. |

### SCAFFOLD_APPROVED Allowlist

Approved for Phase 23 no-state route scaffolding after explicit human approval:

- `/v07r00/chassis/playresult.php`
- `/v07r00/chassis/baidcheck.php`
- `/v07r00/chassis/mydonentry.php`
- `/v07r00/chassis/userdata.php`
- `/v07r00/chassis/crownsdata.php`
- `/v07r00/chassis/recommend.php`
- `/v07r00/chassis/selfbest.php`
- `/v07r00/chassis/heartbeat.php`
- `/v07r00/chassis/initialdatacheck.php`
- `/v07r00/chassis/tournamentcheck.php`
- `/v07r00/chassis/bookkeeping.php`
- `/v07r00/chassis/gettelop.php`
- `/v07r00/chassis/getfolder.php`
- `/v07r00/chassis/taikojuku.php`

## Blocked Or Proto-Only Surfaces

These surfaces remain blocked because the IDA route table probe did not find exact White route suffix strings for them. Proto presence, other-era routes, field names, or data-file hints are not route approval.

| Surface | Current evidence | Phase 23 treatment |
|---------|------------------|--------------------|
| `headclerk2.php` | Proto request/response names exist, but no exact White route suffix string was found in the route table evidence supplied for this continuation. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; do not scaffold. |
| `getreitai.php` | Proto request/response names exist, but no exact White route suffix string was found in the route table evidence supplied for this continuation. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; do not scaffold. |
| `rewardcardcheck.php` | Proto request/response names exist, but no exact White route suffix string was found in the route table evidence supplied for this continuation. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; do not scaffold. |
| `rewardexecution.php` | Proto request/response names exist, but no exact White route suffix string was found in the route table evidence supplied for this continuation. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; do not scaffold. |
| Standalone `challengecompe.php` | No exact standalone White route suffix string in the supplied IDA route table evidence. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; do not scaffold. Embedded challenge-like proto fields do not prove a standalone route. |
| Item shop | No exact White item-shop route suffix strings in the supplied IDA route table evidence. | Blocked; no shop catalog, purchase, medal, or unlock behavior. |
| Banacoin authority | No exact White Banacoin route suffix strings in the supplied IDA route table evidence. | Blocked; no wallet, balance, payment, coupon, receipt, or transaction behavior. |
| Battle | No exact White battle route suffix strings in the supplied IDA route table evidence. | Blocked; no battle userdata, token, NPC, stage, or battle-classified playresult behavior. |
| Tokkun | No exact White Tokkun route suffix strings in the supplied IDA route table evidence. | Blocked; no Tokkun playresult, tutorial, history, or Banacoin-adjacent behavior. |
| WaiWai | No exact White WaiWai route suffix strings in the supplied IDA route table evidence. | Blocked; no WaiWai tutorial or playresult behavior. |
| Gacha runtime | No exact White gacha route suffix strings in the supplied IDA route table evidence. | Blocked; no gacha runtime state or readback behavior. |
| AdminApi/WebUI | No White runtime persistence exists yet. | Outside Phase 23 route scaffold scope. |

## Shared Startup/Version Ownership

| Route family | Owner | Evidence | Phase 23 decision |
|--------------|-------|----------|-------------------|
| `/v01r00/chassis/startupauth.php` | `Adapters.GameProtocol.Shared` | Startup/version suffix table has format string `%s/%s` at pointer table address `0xd2c7a0` -> string `0xc390c0`. Route string `chassis/startupauth.php` is at `0xc390c8`; pointer/table entry `0xd2c7a4`. Matching proto messages are `StartupAuthRequest` and `StartupAuthResponse` in `proto/white/vsinterface.proto`. | Keep shared ownership. White Plan 03 must not duplicate this controller. |
| `/v01r00/chassis/verupauth.php` | `Adapters.GameProtocol.Shared` | Route string `chassis/verupauth.php` is at `0xc390e0`; pointer/table entry `0xd2c7a8`. Matching proto messages are `VerupAuthRequest` and `VerupAuthResponse` in `proto/white/vsinterface.proto`. | Keep shared ownership. White Plan 03 must not duplicate this controller. |
| `/v01r00/chassis/verupcomplete.php` | `Adapters.GameProtocol.Shared` | Route string `chassis/verupcomplete.php` is at `0xc390f8`; pointer/table entry `0xd2c7ac`. Matching proto messages are `VerupCompleteRequest` and `VerupCompleteResponse` in `proto/white/vsinterface.proto`. | Keep shared ownership. White Plan 03 must not duplicate this controller. |

White game route proof for `/v07r00/chassis/*` does not change shared `/v01r00/chassis/*` startup/version ownership.

## Transport Expectation

| Surface | Status | Evidence | Phase 23 treatment |
|---------|--------|----------|--------------------|
| White game route body | `DIRECT_PROTOBUF_EXPECTED` | `proto/white/taiko.proto` exposes direct request/response message pairs for the approved route suffixes, not wrapper request envelopes. | Treat game endpoints as direct-protobuf for Phase 23 scaffolds unless capture evidence contradicts this. |
| Missing/blank HTTP `Content-Type` fallback | `PREFIX_PROVEN_PENDING_APPROVAL` | `/v07r00` service prefix is proven by the IDA service table. | After Task 3 human approval, Plan 03 may add a White-only exact-prefix fallback for `/v07r00/chassis`. It must not broaden fallback behavior to all AC15 routes. |
| Startup/version body | `DIRECT_PROTOBUF_EXPECTED` | `proto/white/vsinterface.proto` matches shared startup/version direct request/response messages. | Keep using the shared `/v01r00/chassis/*` protobuf route family. |

## Active Data Root

| Question | Status | Evidence Source | Decision |
|----------|--------|-----------------|----------|
| White active data root for Phase 23 planning | `ACCEPTED_FOR_FOUNDATION` | `Host/wwwroot/data/white/data/config/ST7100-1` exists and contains `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `chassisinfo.xml`. | Record `Host/wwwroot/data/white/data/config/ST7100-1` as the accepted Phase 23 data root. This is catalog/data evidence only, not route-prefix proof. |

The active root decision allows later catalog planning to target `ST7100-1`; it does not authorize runtime catalog binding, profile persistence, reward semantics, or route behavior beyond the explicitly approved no-state route scaffold list.

## IDB Evidence

| Artifact | Current evidence | What it proves | What it does not prove |
|----------|------------------|----------------|------------------------|
| `.tools/white/EBOOT.ELF.i64` | Live filesystem length is `129893515` bytes; White IDA backend reports `database_opened=true`, `ida_available=true`. | The current checkout has a usable White IDB evidence source for route table inspection. | File size alone does not prove routes, response semantics, call order, content type, data root selection, or state mutation behavior. |
| Route prefix/service table | `v07r00` string at `0xc38468` with pointer at `0xcb0d98`; `v01r00` string at `0xc38470` with pointer at `0xcb0d9c`. | White has a proven game service prefix and a proven shared startup/version service prefix. | Does not approve any suffix unless that suffix also has exact White route table evidence. |
| Game route suffix tables | Exact `chassis/*.php` strings and pointer/table entries listed in Route Suffix Gate. | Fourteen no-state route scaffold candidates are approved after human checkpoint approval. | Does not prove persistence, catalog binding, profile mutation, reward semantics, ChallengeCompe semantics, unlocks, shop, wallet/payment, battle, Tokkun, WaiWai, or gacha behavior. |

Initial raw scan commands from the first pass are retained as historical context only; their negative result was a tooling limitation, not current route-proof truth:

```powershell
Get-Item '.tools\white\EBOOT.ELF.i64' | Select-Object Length,FullName
rg -a -n "\.php" '.tools\white\EBOOT.ELF.i64'
rg -a -n "v07r00|v01r00|/chassis|chassis/|chassis" '.tools\white\EBOOT.ELF.i64'
rg -a -n "bookkeeping|heartbeat|baid|mydonentry|userdata|playresult|initialdatacheck|tournamentcheck|getfolder|gettelop|taikojuku|selfbest|crownsdata|recommend|rewardcardcheck|rewardexecution|getreitai|headclerk2" '.tools\white\EBOOT.ELF.i64'
```

## Unresolved Gaps

| Gap | Blocks | Needed evidence |
|-----|--------|-----------------|
| Blocked/proto-only route suffixes | Any controller for `headclerk2.php`, `getreitai.php`, `rewardcardcheck.php`, `rewardexecution.php`, standalone `challengecompe.php`, item shop, Banacoin authority, battle, Tokkun, WaiWai, gacha runtime, or AdminApi/WebUI | Exact White IDA route table strings, request-log/cabinet captures, RPCS3 traces, or equivalent local client evidence. |
| Runtime call order | Any claim that a proto surface is mandatory cabinet flow | Request logs, captures, or IDA call graph evidence. |
| Active runtime data selection beyond local file presence | Catalog/profile runtime implementation | Later catalog/profile plans should prove how `ST7100-1` is selected before hard runtime assumptions. |
| Reward, ChallengeCompe, Taikojuku/Dani, tournament, and collectable semantics | Stateful behavior | White-specific proto/data/log/capture/IDA proof in later phases. |

## Deferred Runtime Scope

Phase 23 Plan 01 does not implement White catalog loading, profile creation, userdata persistence, normal play, self-best persistence, crowns persistence, favorites, recent songs, Taikojuku/Dani state, reward/present state, Don Challenge, collectables, AdminApi, WebUI, item shop, Banacoin, battle, Tokkun, WaiWai, gacha, or tournament runtime behavior.

Unsupported or unproven White 0.13 surfaces remain absent. Red, Yellow, Blue, Green, and Nijiiro route sets are comparison material only, not scaffold authority.

## Plan 03 Gate

Plan 03 may add White game route scaffolds and Host missing-content-type fallback because Task 3 received explicit human approval of this artifact on 2026-06-17.

For the approved Phase 23 scope, Plan 03 route work is limited to:

- White adapter no-state controllers for the fourteen `SCAFFOLD_APPROVED` `/v07r00/chassis/{suffix}` routes listed above.
- A White-only exact-prefix missing/blank content-type fallback for `/v07r00/chassis`.
- Shared startup/version routes remaining in `Adapters.GameProtocol.Shared` under `/v01r00/chassis/*`.

Plan 03 must not scaffold blocked/proto-only suffixes and must not imply persistence, catalog binding, profile mutation, reward semantics, ChallengeCompe semantics, unlocks, shop, wallet/payment, battle, Tokkun, WaiWai, or gacha behavior.
