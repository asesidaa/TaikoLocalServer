# Phase 23 White Feature Inventory

**Created:** 2026-06-17
**Scope:** White AC15 0.13 evidence classification for Phase 23. This inventory prevents White support from being inferred from Red, Yellow, or Blue adjacency.

## White 0.13 Proven

| Surface | Evidence | Classification | Phase 23 use |
|---------|----------|----------------|--------------|
| White proto inputs | `proto/white/taiko.proto`, `proto/white/vsinterface.proto` | `PROVEN_INPUT` | May generate White wire DTOs in Plan 02 without changing proto inputs. |
| Shared startup/version message family | `StartupAuth*`, `VerupAuth*`, `VerupComplete*` in `proto/white/vsinterface.proto` | `PROVEN_PROTO_SHAPE` | Supports keeping shared `/v01r00/chassis/*` ownership unless route evidence contradicts it. |
| Direct protobuf request/response shapes | Direct request/response messages in `proto/white/taiko.proto` | `PROVEN_PROTO_SHAPE` | Supports direct-protobuf expectation only; not route approval. |
| Local White data root | `Host/wwwroot/data/white/data/config/ST7100-1` | `PROVEN_DATA_ROOT_FOR_FOUNDATION` | Accepted active planning root for catalog work; not route/root runtime proof. |
| Current White IDB file availability | `.tools/white/EBOOT.ELF.i64` length `129893515` bytes | `PROVEN_NONZERO_FILE` | Corrects stale zero-byte notes; not automatic route-prefix proof. |

## Route-Proven Phase 23 Scaffold Candidates

| Prefix | Suffix | Request | Response | Status |
|--------|--------|---------|----------|--------|
| None | None | None | None | No White route suffix is route-proven or `SCAFFOLD_APPROVED` in `23-WHITE-EVIDENCE.md`. |

Plan 03 may not create route scaffolds from this table until route evidence updates the evidence artifact.

## Proto-Only Leads

These are White protocol surfaces with request/response messages but no proven White `.php` route strings in the current evidence pass.

| Candidate suffix | Proto request | Proto response | Classification | Notes |
|------------------|---------------|----------------|----------------|-------|
| `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | No-state probe lead only after route proof. |
| `heartbeat.php` | `HeartBeatRequest` | `HeartBeatResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | No-state probe lead only after route proof. |
| `baidcheck.php` | `BAIDRequest` | `BAIDResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Identity/runtime binding deferred beyond Phase 23. |
| `mydonentry.php` | `MydonEntryRequest` | `MydonEntryResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Identity/runtime binding deferred beyond Phase 23. |
| `userdata.php` | `UserDataRequest` | `UserDataResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Userdata contains profile, reward, recommendation, and embedded challenge-like readback leads. |
| `playresult.php` | `PlayResultRequest` | `PlayResultResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Normal runtime, reward, Taikojuku/Dani, tournament, and embedded challenge arrays require later evidence. |
| `initialdatacheck.php` | `InitialdatacheckRequest` | `InitialdatacheckResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Metadata advertisement lead; catalog/profile binding is out of Plan 01. |
| `getfolder.php` | `GetfolderRequest` | `GetfolderResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Event folder/catalog lead only. |
| `gettelop.php` | `GettelopRequest` | `GettelopResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Telop/catalog lead only. |
| `recommend.php` | `RecommendRequest` | `RecommendResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Recommendation catalog/readback lead only. |
| `taikojuku.php` | `TaikojukuRequest` | `TaikojukuResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | White Dani/Taikojuku lead; runtime call order and persistence are unproven. |
| `selfbest.php` | `SelfBestRequest` | `SelfBestResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Self-best readback lead; White-owned persistence deferred. |
| `crownsdata.php` | `CrownsDataRequest` | `CrownsDataResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Crown byte width/encoding require later proof. |
| `tournamentcheck.php` | `TournamentcheckRequest` | `TournamentcheckResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Tournament probe lead only; no runtime tournament semantics proven. |
| `headclerk2.php` | `HeadClerk2Request` | `HeadClerk2Response` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Compatibility probe lead only. |
| `getreitai.php` | `GetreitaiRequest` | `GetreitaiResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Compatibility candidate; do not scaffold without route proof. |
| `rewardcardcheck.php` | `RewardcardcheckRequest` | `RewardcardcheckResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Reward/present lead; not Yellow item shop or Banacoin behavior. |
| `rewardexecution.php` | `RewardexecutionRequest` | `RewardexecutionResponse` | `PROTO_ONLY_NOT_ROUTE_APPROVED` | Reward/present lead; state-changing role unproven. |

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
| Standalone `challengecompe.php` route and dedicated `ChallengeCompeRequest/Response` | Red and Yellow comparisons | Other-era only for now. White has embedded challenge-like arrays but no standalone White challenge route proof. |
| Item shop routes and item purchase state | Yellow, Blue/Green item-shop flows | Other-era only. White proto has no `Getitemshopinfo`, `Itempurchase`, shop season, or medal spend surface. |
| Banacoin wallet/payment/balance/error route set | Blue/Yellow/Red compatibility surfaces | Other-era only. White proto has no Banacoin authority surface. |
| Blue battle runtime | Blue | Other-era only. White has no battle userdata, battle initial data, token, NPC, or battle playresult evidence. |
| Tokkun play mode and tutorial readback | Blue, Yellow, Red tutorial-only | Other-era only. White proto has no Tokkun tutorial or stage result fields. |
| WaiWai tutorial/playresult behavior | Yellow/Blue protocol leads | Other-era only. White proto has no WaiWai fields. |
| Yellow Don/Katsu medal item-shop economy | Yellow | Other-era only. White reward/present leads are Don Point/present-style, not medals/shop. |

## Absent In White 0.13 Evidence

| Surface | Classification | Reason |
|---------|----------------|--------|
| Item shop | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No White proto route or data evidence for item-shop catalog, purchase, shop seasons, or spend state. |
| Banacoin authority | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No White proto route or data evidence for wallet, balance, payment, coupon, receipt, BNID, settlement, or transaction state. |
| Battle | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No battle route, battle fields, battle data files, token/NPC state, or battle AdminApi/WebUI evidence. |
| Tokkun | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No Tokkun playresult, tutorial flag, stage-history, or Banacoin-for-Tokkun evidence. |
| WaiWai | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No WaiWai tutorial, stage, or readback fields. |
| Gacha runtime | `ABSENT_IN_WHITE_0_13_EVIDENCE` | No explicit White gacha response payloads or runtime state. |
| Standalone `challengecompe.php` | `ABSENT_IN_WHITE_0_13_EVIDENCE` | White proto has embedded challenge/user/bng arrays but no standalone request/response route. |
| AdminApi/WebUI readback | `OUTSIDE_PHASE_23` | No White runtime persistence exists yet; UI/API exposure waits for implemented White-owned surfaces. |
| Catalog/profile binding | `OUTSIDE_PHASE_23` | Phase 23 Plan 01 records evidence only. Phase 24 owns catalog/profile binding. |
| Persistence | `OUTSIDE_PHASE_23` | No White-owned EF tables or runtime writes are created by Plan 01. |
| Collectable runtime | `OUTSIDE_PHASE_23` | Collectable data, including Don Challenge if in range, is intentionally late after core behavior is stable. |

## Unknown/Unresolved

| Question | Classification | Current blocker |
|----------|----------------|-----------------|
| Exact game route prefix | `UNRESOLVED_BLOCKS_ROUTE_CODE` | Current IDB scan did not prove `/v07r00/chassis` or any replacement prefix. |
| Exact `.php` suffixes | `UNRESOLVED_BLOCKS_ROUTE_CODE` | Current IDB scan found no `.php` route suffix strings. |
| Missing-content-type fallback scope | `UNRESOLVED_BLOCKS_ROUTE_CODE` | Host fallback must wait for exact prefix proof. |
| Cabinet call order | `UNKNOWN_RUNTIME` | Needs logs, capture, or IDA call graph evidence. |
| Runtime active root selection | `UNKNOWN_RUNTIME` | `ST7100-1` is accepted local data evidence, not runtime selection proof. |
| Taikojuku/Dani runtime classification | `UNKNOWN_RUNTIME` | Proto/data leads exist, but playresult classification and readback semantics are unproven. |
| Reward/present state semantics | `UNKNOWN_RUNTIME` | `present.xml` and reward fields exist, but reward timing and mutation rules are unproven. |
| Embedded ChallengeCompe-like arrays | `UNKNOWN_RUNTIME` | White field placement differs from Red; readback route and state semantics are unproven. |
| Tournament behavior | `UNKNOWN_RUNTIME` | `Tournamentcheck*` and playresult `tournament_mode` exist, but no full runtime contract is proven. |

## Plan Boundaries

Plan 01 delivers evidence and inventory only. Later plans may use this artifact to generate White wire and add first-class adapter identity, but route scaffolds remain blocked while the evidence artifact has zero `SCAFFOLD_APPROVED` route suffixes.

The current nonzero IDB evidence supersedes prior zero-byte notes, but route strings are still not proven. That distinction is the main boundary for Phase 23.
