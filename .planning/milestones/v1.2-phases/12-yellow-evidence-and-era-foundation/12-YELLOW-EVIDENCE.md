# Phase 12 Yellow Evidence

**Created:** 2026-06-07
**Scope:** Yellow route/version/transport foundation only.

## Route Prefix Decision

| Question | Status | Evidence Source | Decision |
|----------|--------|-----------------|----------|
| Exact Yellow game route prefix | RESOLVED | Explicit user approval on 2026-06-07 | Use `/v09r00` for Yellow game routes in Phase 12 scaffold work. |

The Yellow IDB was placed by the user at `.tools/yellow/EBOOT.ELF.i64` for later IDA research. Phase 12 records the location only; no IDA research was performed because the Phase 12 plans do not require it.

## Proto-Backed Endpoint Suffixes

| Endpoint suffix | Proto request | Proto response | Phase 12 treatment |
|-----------------|---------------|----------------|--------------------|
| `initialdatacheck.php` | `InitialdatacheckRequest` | `InitialdatacheckResponse` | Supported suffix; no Yellow runtime catalog or battle advertisement in Phase 12. |
| `tournamentcheck.php` | `TournamentcheckRequest` | `TournamentcheckResponse` | Supported suffix; runtime data deferred. |
| `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | Supported suffix; no-state scaffold candidate. |
| `coinsetting.php` | `CoinsettingRequest` | `CoinsettingResponse` | Supported suffix; no-state scaffold candidate. |
| `gettelop.php` | `GettelopRequest` | `GettelopResponse` | Supported suffix; catalog readback deferred. |
| `getfolder.php` | `GetfolderRequest` | `GetfolderResponse` | Supported suffix; catalog readback deferred. |
| `taikojuku.php` | `TaikojukuRequest` | `TaikojukuResponse` | Supported suffix; Dani runtime deferred. |
| `getitemshopinfo.php` | `GetitemshopinfoRequest` | `GetitemshopinfoResponse` | Supported suffix; shop runtime deferred. |
| `headclerk2.php` | `HeadClerk2Request` | `HeadClerk2Response` | Supported suffix; no-state scaffold candidate. |
| `playresult.php` | `PlayResultRequest` | `PlayResultResponse` | Supported direct protobuf request shape; persistence deferred. |
| `baidcheck.php` | `BAIDRequest` | `BAIDResponse` | Supported suffix; identity runtime deferred. |
| `mydonentry.php` | `MydonEntryRequest` | `MydonEntryResponse` | Supported suffix; identity runtime deferred. |
| `userdata.php` | `UserDataRequest` | `UserDataResponse` | Supported suffix; userdata runtime deferred. |
| `challengecompe.php` | `ChallengeCompeRequest` | `ChallengeCompeResponse` | Supported suffix; runtime data deferred. |
| `balancecheck.php` | `BalancecheckRequest` | `BalancecheckResponse` | Supported Banacoin-adjacent suffix; wallet/payment state deferred and out of Phase 12. |
| `banacoinpayment.php` | `BanacoinpaymentRequest` | `BanacoinpaymentResponse` | Supported Banacoin-adjacent suffix; no wallet/payment persistence in Phase 12. |
| `banacoinerrorlog.php` | `BanacoinerrorlogRequest` | `BanacoinerrorlogResponse` | Supported Banacoin-adjacent suffix; no wallet/payment persistence in Phase 12. |
| `getbanacoininfo.php` | `GetbanacoininfoRequest` | `GetbanacoininfoResponse` | Supported Banacoin-adjacent suffix; runtime compatibility deferred. |
| `crownsdata.php` | `CrownsDataRequest` | `CrownsDataResponse` | Supported suffix; crown encoding proof deferred. |
| `recommend.php` | `RecommendRequest` | `RecommendResponse` | Supported suffix; catalog readback deferred. |
| `selfbest.php` | `SelfBestRequest` | `SelfBestResponse` | Supported suffix; self-best runtime deferred. |
| `heartbeat.php` | `HeartBeatRequest` | `HeartBeatResponse` | Supported suffix; no-state scaffold candidate. |
| `itempurchase.php` | `ItempurchaseRequest` | `ItempurchaseResponse` | Supported suffix; item purchase and medal runtime deferred. |
| `rewardcardcheck.php` | `RewardcardcheckRequest` | `RewardcardcheckResponse` | Supported suffix; runtime behavior deferred. |
| `rewardexecution.php` | `RewardexecutionRequest` | `RewardexecutionResponse` | Supported suffix; runtime behavior deferred. |
| `getreitai.php` | `GetreitaiRequest` | `GetreitaiResponse` | Proto messages exist, but no Yellow route-call evidence is recorded; do not scaffold in Phase 12. |

## Shared Startup And Version Routes

| Route | Owner | Evidence | Phase 12 decision |
|-------|-------|----------|-------------------|
| `/v01r00/chassis/startupauth.php` | `Adapters.GameProtocol.Shared` | Yellow `vsinterface.proto` matches the shared startup request/response shape. | Keep shared ownership. |
| `/v01r00/chassis/verupauth.php` | `Adapters.GameProtocol.Shared` | Yellow `vsinterface.proto` matches the shared version-auth request/response shape. | Keep shared ownership. |
| `/v01r00/chassis/verupcomplete.php` | `Adapters.GameProtocol.Shared` | Yellow `vsinterface.proto` matches the shared version-complete request/response shape. | Keep shared ownership. |

Yellow must not add duplicate startup/version controllers under `/v09r00/chassis/*` unless later client evidence proves Yellow-specific ownership.

## Transport Evidence And Gaps

| Surface | Status | Evidence | Phase 12 treatment |
|---------|--------|----------|--------------------|
| Game route request body | SUPPORTED SCAFFOLD | Local `proto/yellow/yellow.proto` exposes direct request messages such as `PlayResultRequest` with `Baid` and no wrapper `PlayresultData`. | Use direct protobuf DTOs for scaffold controllers. |
| Missing or blank HTTP `Content-Type` behavior | UNVERIFIED RUNTIME GAP | No Yellow cabinet/RPCS3 HTTP capture is recorded in Phase 12. | Host fallback may be scoped to `/v09r00/chassis`, but runtime HTTP framing remains a Phase 17 smoke item. |
| `getreitai.php` route use | UNVERIFIED ROUTE GAP | Proto messages exist, but no local log, binary string citation, or user approval says Yellow calls this route. | Do not scaffold `getreitai.php` in Phase 12. |

## No Yellow Battle Evidence

| Surface | Yellow evidence | Decision |
|---------|-----------------|----------|
| Dedicated battle route | `proto/yellow/yellow.proto` contains no `BattleUserDataRequest` or `BattleUserDataResponse`. | No Yellow `battleuserdata.php` route. |
| Initial data | Yellow `InitialdatacheckResponse` contains no `is_battleplay`, `release_battle_stage_flg`, `release_battle_special_flg`, or `battle_bonds_lv_cap`. | No Yellow battle advertisement. |
| Play result | Yellow `PlayResultRequest` contains normal, medal, payment, and Tokkun fields, but no `BattleStageData`, `ReleaseBattleData`, `ary_battletokendata`, or `assign_next_stage_id`. | No Yellow battle classifier or Blue battle fallback. |
| Persistence | No Yellow persistence exists in Phase 12. | No Yellow battle tables, migrations, entities, or Blue battle fallback references. |

Summary: Yellow has no Yellow battle surface in Phase 12. This is an absence contract, not a deferred Blue battle mirror.

## Deferred Runtime Scope

Phase 12 does not implement Yellow persistence, catalog loading, normal play state, Tokkun state, shop or medal state, Dani state, Banacoin wallet/payment behavior, AdminApi routes, or WebUI routes. Those remain assigned to later Yellow phases.
