# Phase 39 MOMOIRO Evidence Gate

**Created:** 2026-06-26
**Scope:** MOMOIRO 0.11 route inventory, shared startup/version ownership, direct-protobuf expectation, local evidence handles, and deferred runtime gaps before any MOMOIRO state behavior is claimed.

## Provenance

| Evidence Question | Status | Evidence Handle | Phase 39 Decision |
|-------------------|--------|-----------------|-------------------|
| Route inventory provenance | supplied and locked by Phase 39 context | `.planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md`, `.planning/phases/39-momoiro-evidence-and-era-foundation/39-RESEARCH.md`, `.tools/momoiro/EBOOT.ELF.i64` | Use the supplied route inventory as the active Phase 39 route gate. Fresh IDA route-string offsets were not recaptured in this phase. |
| Protocol schema input | available local evidence | `proto/momoiro/taiko.proto`, `proto/momoiro/vsinterface.proto` | Treat proto messages as necessary but not sufficient route evidence. Proto-only families stay absent without binary/client `.php` route evidence. |
| Shared startup/version ownership | shared AC15 behavior | `proto/momoiro/vsinterface.proto`, existing `Adapters.GameProtocol.Shared` route ownership | Keep `/v01r00/chassis/startupauth.php`, `/v01r00/chassis/verupauth.php`, and `/v01r00/chassis/verupcomplete.php` in Shared. Do not create MOMOIRO adapter startup/version controllers. |
| Game route ownership | MOMOIRO adapter game surface | supplied route inventory under `/v04r00/chassis` | Phase 39 route scaffolding plans may create only binary-proven MOMOIRO game routes under `/v04r00/chassis`. |
| Transport expectation | direct protobuf | existing older-AC15 transport pattern plus Phase 39 context | MOMOIRO game endpoints should use direct protobuf request/response transport unless current MOMOIRO client evidence proves otherwise. |

## Route Prefix Decisions

| Prefix | Ownership | Routes | Phase 39 Action |
|--------|-----------|--------|-----------------|
| `/v01r00/chassis` | `Adapters.GameProtocol.Shared` | `/v01r00/chassis/startupauth.php`, `/v01r00/chassis/verupauth.php`, `/v01r00/chassis/verupcomplete.php` | Record as shared startup/version behavior. No MOMOIRO adapter controllers. |
| `/v04r00/chassis` | future `Adapters.GameProtocol.Momoiro` game adapter | Binary-proven game route inventory listed below. | Evidence-gated route scaffold only. No catalog/profile/persistence/AdminApi/WebUI behavior in Plan 39-01. |

## Active Game Route Inventory

These rows are active because the route appears in the supplied and locked MOMOIRO binary route inventory and has a corresponding MOMOIRO proto family. Phase 39 action is limited to evidence-gated route scaffolding in later plans unless a row says otherwise.

| Route | Proto Family | Binary Route Evidence | Phase 39 Action | Runtime Behavior Claim |
|-------|--------------|-----------------------|-----------------|------------------------|
| `/v04r00/chassis/playresult.php` | `PlayResultRequest` / `PlayResultResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | No gameplay persistence, unlock, reward, Dan, challenge, or cross-era writes in Plan 39-01. |
| `/v04r00/chassis/baidcheck.php` | `BAIDRequest` / `BAIDResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | Identity/profile behavior deferred. |
| `/v04r00/chassis/mydonentry.php` | `MydonEntryRequest` / `MydonEntryResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | MyDon save behavior deferred. |
| `/v04r00/chassis/userdata.php` | `UserDataRequest` / `UserDataResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | Userdata, favorites, recent songs, release flags, challenge arrays, and crown readback deferred. |
| `/v04r00/chassis/recommend.php` | `RecommendRequest` / `RecommendResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | Recommendation catalog behavior deferred. |
| `/v04r00/chassis/selfbest.php` | `SelfBestRequest` / `SelfBestResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | Self-best readback deferred. |
| `/v04r00/chassis/heartbeat.php` | `HeartBeatRequest` / `HeartBeatResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold or explicit static-result operational route | No stateful runtime role claimed. |
| `/v04r00/chassis/defaultsong.php` | `DefaultsongRequest` / `DefaultsongResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | Default-song/catalog behavior deferred. |
| `/v04r00/chassis/bookkeeping.php` | `BookKeepingRequest` / `BookKeepingResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold or explicit static-result operational route | No accounting persistence claimed. |
| `/v04r00/chassis/songhash.php` | `SonghashRequest` / `SonghashResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | Song-hash table behavior deferred. |
| `/v04r00/chassis/telopcheck.php` | `TelopCheckRequest` / `TelopCheckResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | Telop catalog behavior deferred. |
| `/v04r00/chassis/gettelop.php` | `GetTelopRequest` / `GetTelopResponse` | supplied and locked by Phase 39 context | evidence-gated route scaffold | Telop content behavior deferred. |

## Proto-Only Absent Families

These rows are absent from MOMOIRO route scaffolding because proto presence is not enough. Each family needs both `proto/momoiro` message presence and binary/client route evidence before a route can be added.

| Proto-Only Family | Proto Evidence | Binary Route Evidence | Phase 39 Action |
|-------------------|----------------|-----------------------|-----------------|
| `shoppingresult.php` | `ShoppingResultRequest` / `ShoppingResultResponse` exist in `proto/momoiro/taiko.proto`. | Absent from the supplied `/v04r00/chassis` inventory. | Absent. Do not create a MOMOIRO shopping result controller or item-shop authority. |
| `bestscore.php` | `BestScoreRequest` / `BestScoreResponse` exist in `proto/momoiro/taiko.proto`. | Absent from the supplied `/v04r00/chassis` inventory. | Absent. Do not create a best-score route stub from proto alone. |
| `communicationlog.php` | `CommunicationLogRequest` / `CommunicationLogResponse` exist in `proto/momoiro/taiko.proto`. | Absent from the supplied `/v04r00/chassis` inventory. | Absent. Do not create a communication-log route stub from proto alone. |
| `mainichisong.php` | `MainichisongRequest` / `MainichisongResponse` exist in `proto/momoiro/taiko.proto`. | Absent from the supplied `/v04r00/chassis` inventory. | Absent. Do not create a mainichi-song route stub from proto alone. |

## Deferred Runtime Gaps

| Gap | Current Evidence | Required Before Runtime Support |
|-----|------------------|---------------------------------|
| Catalog layout and limits | MOMOIRO data is root-level under `Host/wwwroot/data/momoiro/data`; exact route behavior and limits are Phase 40 scope. | Binary/client-backed catalog root, song ordering, favorite/recent limits, default-song flags, song hash, release flags, and Don Point/reward limits. |
| Crown readback | `UserDataResponse.hash_crown_flg` exists in `proto/momoiro/taiko.proto`. | Binary/client proof of crown byte placement, packing, song count, and difficulty ordering before readback implementation. |
| Song unlocking | `hash_release_song_flg`, `release_song_no`, and song-hash fields exist in proto. | MOMOIRO-specific binary/client proof for unlock semantics and catalog order before persistence/readback mutation. |
| Challenge-shaped arrays | `PlayResultRequest` and `UserDataResponse` expose challenge arrays. | MOMOIRO-specific evidence before any Don Challenge, ChallengeCompe, reward-management, or standalone challenge behavior is claimed. |
| Shop and live-service behavior | `shoppingresult.php` proto family exists, but the route is absent from the supplied inventory. | Binary/client route evidence plus semantics before any item-shop, wallet, Banacoin, coupon, or transaction behavior is added. |

## Scope Guard

Plan 39-01 records evidence and RED validation contracts only. It does not add MOMOIRO runtime behavior, persistence, catalog loading, AdminApi/WebUI support, controllers, adapter project, or `GameEra.Momoiro`.
