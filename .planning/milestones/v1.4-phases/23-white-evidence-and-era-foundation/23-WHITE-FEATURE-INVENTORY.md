# Phase 23 White Feature Inventory

**Created:** 2026-06-17
**Updated:** 2026-06-17 after route-proof checkpoint continuation
**Scope:** White AC15 0.13 evidence classification for Phase 23. This inventory prevents White support from being inferred from Red, Yellow, Blue, Green, or Nijiiro adjacency.

## White 0.13 Proven

| Surface | Evidence | Classification | Phase 23 use |
|---------|----------|----------------|--------------|
| White proto inputs | `proto/white/taiko.proto`, `proto/white/vsinterface.proto` | `PROVEN_INPUT` | May generate White wire DTOs in Plan 02 without changing proto inputs. |
| White IDA route evidence source | `.tools/white/EBOOT.ELF.i64`; backend probe `database_opened=true`, `ida_available=true`; live size `129893515` bytes | `PROVEN_IDA_SOURCE` | Supersedes stale zero-byte notes and supports route table evidence review. |
| White game service prefix | `v07r00` string at `0xc38468`; big-endian pointer at `0xcb0d98` in the service table | `PROVEN_ROUTE_PREFIX` | Supports `/v07r00/chassis/{approved suffix}` route scaffolds after Task 3 human approval. |
| Shared startup/version service prefix | `v01r00` string at `0xc38470`; big-endian pointer at `0xcb0d9c` in the same service table | `PROVEN_SHARED_SERVICE_PREFIX` | Keeps startup/version under shared `/v01r00/chassis/*`. |
| Shared startup/version suffixes | `chassis/startupauth.php`, `chassis/verupauth.php`, `chassis/verupcomplete.php` in the startup/version suffix table; matching `proto/white/vsinterface.proto` messages | `PROVEN_SHARED_ROUTE_FAMILY` | Supports shared ownership; White Plan 03 must not duplicate these routes. |
| Direct protobuf request/response shapes | Direct request/response messages in `proto/white/taiko.proto` and `proto/white/vsinterface.proto` | `PROVEN_PROTO_SHAPE` | Supports direct-protobuf expectation only; not state semantics. |
| Local White data root | `Host/wwwroot/data/white/data/config/ST7100-1` | `PROVEN_DATA_ROOT_FOR_FOUNDATION` | Accepted active planning root for catalog work; not runtime selection proof. |

## Route-Proven Phase 23 Scaffold Candidates

These are the only route-proven White game scaffold candidates for Phase 23. They remain no-state compatibility scaffolds and require explicit Task 3 approval before Plans 02 or 03 proceed.

| Prefix | Suffix | Request | Response | Status |
|--------|--------|---------|----------|--------|
| `/v07r00/chassis` | `playresult.php` | `PlayResultRequest` | `PlayResultResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `baidcheck.php` | `BAIDRequest` | `BAIDResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `mydonentry.php` | `MydonEntryRequest` | `MydonEntryResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `userdata.php` | `UserDataRequest` | `UserDataResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `crownsdata.php` | `CrownsDataRequest` | `CrownsDataResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `recommend.php` | `RecommendRequest` | `RecommendResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `selfbest.php` | `SelfBestRequest` | `SelfBestResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `heartbeat.php` | `HeartBeatRequest` | `HeartBeatResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `initialdatacheck.php` | `InitialdatacheckRequest` | `InitialdatacheckResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `tournamentcheck.php` | `TournamentcheckRequest` | `TournamentcheckResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `gettelop.php` | `GettelopRequest` | `GettelopResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `getfolder.php` | `GetfolderRequest` | `GetfolderResponse` | `SCAFFOLD_APPROVED` |
| `/v07r00/chassis` | `taikojuku.php` | `TaikojukuRequest` | `TaikojukuResponse` | `SCAFFOLD_APPROVED` |

## Proto-Only Or Blocked Leads

These are White protocol or candidate surfaces without exact approved route suffix evidence in the supplied IDA route table probe. They must not be scaffolded in Phase 23.

| Candidate surface | Proto/evidence | Classification | Notes |
|------------------|----------------|----------------|-------|
| `headclerk2.php` | `HeadClerk2Request` / `HeadClerk2Response` exist, but no exact route suffix string was found. | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Compatibility candidate only. |
| `getreitai.php` | `GetreitaiRequest` / `GetreitaiResponse` exist, but no exact route suffix string was found. | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Compatibility candidate only. |
| `rewardcardcheck.php` | `RewardcardcheckRequest` / `RewardcardcheckResponse` exist, but no exact route suffix string was found. | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Reward/present lead only; not Yellow item shop or Banacoin behavior. |
| `rewardexecution.php` | `RewardexecutionRequest` / `RewardexecutionResponse` exist, but no exact route suffix string was found. | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Reward/present lead only; state-changing role unproven. |
| Standalone `challengecompe.php` | No exact standalone White route suffix string in the supplied IDA route table evidence. | `NOT_ROUTE_APPROVED` | Embedded challenge-like arrays do not prove a standalone route. |

## Data-Only Leads

| Data source | Evidence | Classification | Later work |
|-------------|----------|----------------|------------|
| `musicinfo.xml` | Present under `ST7100-1`; research counted 568 rows. | `DATA_ONLY_CATALOG_LEAD` | Catalog/profile binding in Phase 24. |
| `musicmedleyinfo.xml` | Present under `ST7100-1`; research counted 25 medley rows. | `DATA_ONLY_DANI_LEAD` | White Taikojuku/Dani catalog lead; runtime support still evidence-gated. |
| `defmusic.bin` | Present under `ST7100-1`. | `DATA_ONLY_DEFAULT_SONG_LEAD` | Default song flags/initial data later. |
| `present.xml` | Present under `ST7100-1`; research counted 10 Don Point threshold entries. | `DATA_ONLY_REWARD_LEAD` | White reward/present behavior later; not Yellow shop or Banacoin state. |
| `spacialbaid.xml` | Present under `ST7100-1`. | `DATA_ONLY_SPECIAL_BAID_LEAD` | Special BAID behavior later, after runtime proof. |
| `fumen/tuning.bin` and `tuning_ext.bin` | Present under White data root. | `DATA_ONLY_FUMEN_LEAD` | Catalog validation in Phase 24. |
| `chassisinfo.xml` | Present under `ST7100-1`. | `DATA_ONLY_OPERATOR_LEAD` | Do not use as route proof. |

## Other-Era Only

| Surface | Seen in | White classification |
|---------|---------|----------------------|
| Standalone `challengecompe.php` route and dedicated ChallengeCompe runtime behavior | Red and Yellow comparisons | Other-era only for now. White has embedded challenge-like fields but no standalone approved White route. |
| Item shop routes and item purchase state | Yellow, Blue/Green item-shop flows | Other-era only. White proto has no approved item-shop route and no Phase 23 shop behavior. |
| Banacoin wallet/payment/balance/error route set | Blue/Yellow/Red compatibility surfaces | Other-era only. White has no approved Banacoin authority route. |
| Blue battle runtime | Blue | Other-era only. White has no approved battle route, battle data, token, NPC, or battle playresult evidence. |
| Tokkun play mode and tutorial readback | Blue, Yellow, Red tutorial-only | Other-era only. White has no approved Tokkun route, tutorial flag, or stage result evidence. |
| WaiWai tutorial/playresult behavior | Yellow/Blue protocol leads | Other-era only. White has no approved WaiWai route or fields. |
| Yellow Don/Katsu medal item-shop economy | Yellow | Other-era only. White reward/present leads are Don Point/present-style, not medals/shop. |

## Absent In White 0.13 Evidence

| Surface | Classification | Reason |
|---------|----------------|--------|
| Item shop | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No approved White route or data evidence for item-shop catalog, purchase, shop seasons, or spend state. |
| Banacoin authority | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No approved White route or data evidence for wallet, balance, payment, coupon, receipt, BNID, settlement, or transaction state. |
| Battle | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No approved battle route, battle fields, battle data files, token/NPC state, or battle AdminApi/WebUI evidence. |
| Tokkun | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No approved Tokkun route, tutorial flag, stage-history, or Banacoin-for-Tokkun evidence. |
| WaiWai | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No approved WaiWai route, tutorial, stage, or readback fields. |
| Gacha runtime | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No approved White gacha route or runtime state. |
| Standalone `challengecompe.php` | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No approved standalone route; embedded challenge-like arrays remain runtime evidence leads only. |
| AdminApi/WebUI readback | `OUTSIDE_PHASE_23` | No White runtime persistence exists yet; UI/API exposure waits for implemented White-owned surfaces. |
| Catalog/profile binding | `OUTSIDE_PHASE_23` | Phase 23 records evidence and no-state scaffolds only. Phase 24 owns catalog/profile binding. |
| Persistence | `OUTSIDE_PHASE_23` | No White-owned EF tables or runtime writes are created by Plan 01. |
| Collectable runtime | `OUTSIDE_PHASE_23` | Collectable data, including Don Challenge if in range, is intentionally late after core behavior is stable. |

## Unknown/Unresolved

| Question | Classification | Current blocker |
|----------|----------------|-----------------|
| Human approval for route scaffolding | `RESOLVED_PHASE_23` | Route-proof approval was recorded on 2026-06-17 before Plans 02 and 03 proceeded. |
| Missing-content-type fallback implementation | `RESOLVED_PHASE_23` | Host fallback now adds only exact `/v07r00/chassis` missing-content-type protobuf handling. |
| Cabinet call order | `UNKNOWN_RUNTIME` | Needs logs, capture, or IDA call graph evidence. |
| Runtime active root selection | `UNKNOWN_RUNTIME` | `ST7100-1` is accepted local data evidence, not runtime selection proof. |
| Taikojuku/Dani runtime classification | `UNKNOWN_RUNTIME` | Approved route/proto/data leads exist, but playresult classification and readback semantics are unproven. |
| Reward/present state semantics | `UNKNOWN_RUNTIME` | `present.xml` and reward fields exist, but reward timing and mutation rules are unproven. |
| Embedded ChallengeCompe-like arrays | `UNKNOWN_RUNTIME` | White field placement differs from Red; readback route and state semantics are unproven. |
| Tournament behavior | `UNKNOWN_RUNTIME` | `Tournamentcheck*` and playresult `tournament_mode` exist, but no full runtime contract is proven. |

## Plan Boundaries

Plan 01 delivered evidence and inventory only. Plans 02 and 03 used the approved route proof to generate White wire, add first-class adapter identity, and add no-state route scaffolds.

With Task 3 approval recorded on 2026-06-17, Phase 23 route scaffolding is limited to the fourteen `SCAFFOLD_APPROVED` `/v07r00/chassis/{suffix}` routes. The current route proof does not imply persistence, catalog binding, profile mutation, reward semantics, ChallengeCompe semantics, unlocks, shop, wallet/payment, battle, Tokkun, WaiWai, or gacha behavior.
