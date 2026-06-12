# Phase 18 Red Evidence

**Created:** 2026-06-12
**Scope:** Red route, version, transport, data-root, route-suffix, and evidence-gap foundation for Phase 18 only.

## Evidence Authority

| Source | Authority | Notes |
|--------|-----------|-------|
| `.tools/red/EBOOT.ELF.i64` | Primary for route prefixes, route suffixes, startup/version strings, root strings, and addresses | Local IDB evidence accessed through the existing `ida-cli` daemon. Current SHA256 is `59E5E63C0B422080AE041173334B8528B84C87CD23C55F06BC4D5EF2055B5A68`. |
| `.planning/phases/18-red-evidence-and-capability-foundation/18-RESEARCH.md` | Primary Phase 18 capture of IDA-backed findings | Use when an exact row or xref was already captured. Re-query IDA instead of guessing if a route/root/address row is missing. |
| `proto/red/taiko.proto` and `proto/red/vsinterface.proto` | Primary protocol input evidence | Proto presence is not route/runtime proof by itself. |
| `Host/wwwroot/data/red/data` | Local operator data inventory | Active root selection is IDA-backed, not filename guessed. |
| Runtime logs | Acceptance evidence, not a foundation blocker | Manual RPCS3/cabinet smoke still needs to prove actual request order, content type, and probe behavior. |

## Route And Version Boundaries

| Boundary | Decision | Evidence |
|----------|----------|----------|
| Red game prefix | Use `/v08r01/chassis/*` for Red game-route probes. | IDA string `https://%s:%s@%s:%d/v08r01` at `0xDA7660`, xref from `0xEB9B50`. |
| Shared startup/version prefix | Keep startup/version under `/v01r00/chassis/*`. | IDA string `https://%s:%s@%s:%d/v01r00` at `0xDA76A0`, xref from `0xEB9B60`. |
| Startup auth suffix | Shared route remains `chassis/startupauth.php`. | IDA suffix string at `0xDA8330`. |
| Version auth suffix | Shared route remains `chassis/verupauth.php`. | IDA suffix string at `0xDA8348`. |
| Version complete suffix | Shared route remains `chassis/verupcomplete.php`. | IDA suffix string at `0xDA8360`. |

Route prefixes follow the Phase 18 rule `/vxxryy`, where `xx` is the startup-auth version and `yy` is the matching revision/proto revision. Red game routes are therefore `/v08r01`, while startup/version stays on the shared `/v01r00` surface.

## Shared Startup And Version Ownership

| Route | Owner | Phase 18 Decision |
|-------|-------|-------------------|
| `/v01r00/chassis/startupauth.php` | `Adapters.GameProtocol.Shared` | Preserve shared ownership. Later Red startup mapping should extend HDD-era resolution instead of adding a Red duplicate route. |
| `/v01r00/chassis/verupauth.php` | `Adapters.GameProtocol.Shared` | Preserve shared ownership. |
| `/v01r00/chassis/verupcomplete.php` | `Adapters.GameProtocol.Shared` | Preserve shared ownership. |

Red support should not add duplicate startup/version controllers under `/v08r01/chassis/*` unless later runtime or IDA evidence overturns the shared ownership proof.

## Direct-Protobuf Transport Expectation

| Surface | Current Status | Evidence | Open Gate |
|---------|----------------|----------|-----------|
| Red game request bodies | `direct-protobuf` expectation | Red has direct protobuf request/response messages in `proto/red/taiko.proto`, and current AC15 game routes use direct protobuf transport. | Manual runtime smoke must still confirm Red content type/framing and whether missing-content-type fallback is enough. |
| Red startup/version request bodies | Shared protobuf surface | `proto/red/vsinterface.proto` has startup/version messages compatible with the shared route family. | Later plans must prove Red HDD mapping and startup movie behavior before claiming complete shared startup support. |

This artifact does not treat transport as fully accepted runtime evidence. It records the implementation expectation and leaves manual smoke open.

## HDD And Version Mapping

| Mapping | Status | Evidence / Rationale |
|---------|--------|----------------------|
| `hdd_ver / 100 == 8` -> Red | Required later foundation work | Red route prefix is `/v08r01`; research records `GetStartupMovieDataQuery.ResolveEra` as still missing `8 => GameEra.Red`. |
| `hdd_ver / 100 == 9` -> Yellow | Existing behavior | Current shared startup mapping already handles Yellow. |
| `hdd_ver / 100 == 10` -> Blue | Existing behavior | Current shared startup mapping already handles Blue. |
| `hdd_ver / 100 == 11` -> Green | Existing behavior | Current shared startup mapping already handles Green. |
| `hdd_ver / 100 == 12` -> Nijiiro | Existing behavior | Current shared startup mapping already handles Nijiiro. |

Plan 18-01 records the needed Red mapping but does not implement it.

## Active Data-Root Decision

| Root | Classification | Evidence |
|------|----------------|----------|
| `ST8100-1` | Active Red runtime root | IDA strings include `/data/config/ST8100-1` at `0xD4DA40`, `/data/nutdata/ST8100-1` at `0xD4DAA8`, `/updates/ST8100-1` at `0xD4DAC0`, `/cache/ST8100-1` at `0xD6E408`, `ST8100-1` at `0xD72FC0`, and `ST8100-1-NA-MPR0-K01` at `0xDB13D0`. |
| `ST5100-1` | Inactive or historical local root only | Present under local `config/`, but not the active Red IDB root. |
| `ST5100-7` | Inactive or historical local root only | Present under local `config/`, but not the active Red IDB root. |
| `ST7100-1` | Inactive or historical local root only | Present under local `config/` and `nutdata/`, but not the active Red IDB root. |

Inactive or historical roots are therefore summarized as `ST5100-*` and `ST7100-1`; the active root is `ST8100-1`.

Phase 19 may bind catalog loaders to `ST8100-1` after parser/profile proof. Phase 18 only records the root evidence.

## Local Data-Root Inventory

`Host/wwwroot/data/red/data` exists and currently contains these top-level directories:

`config`, `content`, `develop`, `don3d`, `font`, `fumen`, `libsmart`, `lm_data`, `lumendata`, `lumenviewer`, `module`, `movie`, `nutdata`, `shader`, `sound`, `testmode`, `usio`, and `zanzou`.

It also contains root-level files including `chassisinfo.xml`, `config.xml`, `defmusic.bin`, `device.xml`, `dxt5.ddp`, `ensogame.lao`, `ensolayout.bin`, `forbidden.xml`, `musicinfo.xml`, and `musicmedleyinfo.xml`.

Local `config/` roots:

| Path | Classification |
|------|----------------|
| `Host/wwwroot/data/red/data/config/common` | Shared local config material, not the active runtime root. |
| `Host/wwwroot/data/red/data/config/ST5100-1` | Inactive or historical. |
| `Host/wwwroot/data/red/data/config/ST5100-7` | Inactive or historical. |
| `Host/wwwroot/data/red/data/config/ST7100-1` | Inactive or historical. |
| `Host/wwwroot/data/red/data/config/ST8100-1` | Active root by IDA evidence. |

Active `ST8100-1` candidates confirmed present: `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, `Host/wwwroot/data/red/data/fumen/tuning.bin`, and `Host/wwwroot/data/red/data/movie`.

No committed Red JSON sidecars were found under `Host/wwwroot/data/red`; later plans must add server-authored JSON only when a supported Red feature requires it.

## IDB-Known Route Suffix Inventory

These are IDB-known Red game suffixes under `/v08r01/chassis/*`. Phase 18 route probes may use these to collect runtime request-routing evidence, but this table is not a claim that gameplay behavior is implemented.

| Route Suffix | Address | Proto Surface | Phase 18 Treatment |
|--------------|---------|---------------|--------------------|
| `chassis/playresult.php` | `0xDA96F8` | `PlayResultRequest` / `PlayResultResponse` | Route probe only; no score, crown, Dani, Tokkun, reward, or challenge writes. |
| `chassis/banacoinerrorlog.php` | `0xDA9710` | `BanacoinerrorlogRequest` / `BanacoinerrorlogResponse` | Stateless compatibility probe only; no payment authority. |
| `chassis/baidcheck.php` | `0xDAA710` | `BAIDRequest` / `BAIDResponse` | Route probe only; identity implementation belongs to Phase 20. |
| `chassis/mydonentry.php` | `0xDAA728` | `MydonEntryRequest` / `MydonEntryResponse` | Route probe only; profile creation belongs to Phase 20. |
| `chassis/userdata.php` | `0xDAA740` | `UserDataRequest` / `UserDataResponse` | Route probe only; userdata readback belongs to Phase 20. |
| `chassis/challengecompe.php` | `0xDAA758` | `ChallengeCompeRequest` / `ChallengeCompeResponse` | Shared older-AC15 ChallengeCompe candidate; stateful semantics belong to Phase 21. |
| `chassis/balancecheck.php` | `0xDAA778` | `BalancecheckRequest` / `BalancecheckResponse` | Banacoin-adjacent stateless compatibility candidate only. |
| `chassis/banacoinpayment.php` | `0xDAA798` | `BanacoinpaymentRequest` / `BanacoinpaymentResponse` | Banacoin-adjacent stateless compatibility candidate only. |
| `chassis/crownsdata.php` | `0xDAA7B8` | `CrownsDataRequest` / `CrownsDataResponse` | Route probe only; crown byte format/width proof required before profile binding. |
| `chassis/recommend.php` | `0xDAA7D0` | `RecommendRequest` / `RecommendResponse` | Route probe only; catalog-backed behavior belongs to Phase 19/20. |
| `chassis/selfbest.php` | `0xDAA7E8` | `SelfBestRequest` / `SelfBestResponse` | Route probe only; score state belongs to Phase 20. |
| `chassis/heartbeat.php` | `0xDAA800` | `HeartBeatRequest` / `HeartBeatResponse` | Minimal safe probe can return status fields; no state. |
| `chassis/rewardcardcheck.php` | `0xDAA818` | `RewardcardcheckRequest` / `RewardcardcheckResponse` | Simple compatibility candidate only; no unlock, shop, medal, or payment semantics. |
| `chassis/rewardexecution.php` | `0xDAA838` | `RewardexecutionRequest` / `RewardexecutionResponse` | Simple compatibility candidate only; no unlock or gameplay state in Phase 18. |
| `chassis/initialdatacheck.php` | `0xDAADE8` | `InitialdatacheckRequest` / `InitialdatacheckResponse` | Route probe only; feature advertisement must not imply unsupported behavior. |
| `chassis/tournamentcheck.php` | `0xDAAE08` | `TournamentcheckRequest` / `TournamentcheckResponse` | Route probe only; no gacha or tournament state in Phase 18. |
| `chassis/bookkeeping.php` | `0xDAAE28` | `BookKeepingRequest` / `BookKeepingResponse` | Log-and-success probe only. |
| `chassis/coinsetting.php` | `0xDAAE40` | `CoinsettingRequest` / `CoinsettingResponse` | Log-and-success probe only. |
| `chassis/gettelop.php` | `0xDAAE58` | `GettelopRequest` / `GettelopResponse` | Route probe only; telop data proof belongs later. |
| `chassis/getfolder.php` | `0xDAAE70` | `GetfolderRequest` / `GetfolderResponse` | Route probe only; folder data proof belongs later. |
| `chassis/taikojuku.php` | `0xDAAE88` | `TaikojukuRequest` / `TaikojukuResponse` | Route probe only; Dani/Taikojuku binding belongs to Phase 19/20. |
| `chassis/headclerk2.php` | `0xDAAEA0` | `HeadClerk2Request` / `HeadClerk2Response` | Log-and-success probe only. |

## Proto-Only Candidate Surfaces

| Surface | Proto Evidence | Route Evidence | Phase 18 Decision |
|---------|----------------|----------------|-------------------|
| `getbanacoininfo.php` | `GetbanacoininfoRequest` / `GetbanacoininfoResponse` exist in `proto/red/taiko.proto`. | Not present in the captured IDB route suffix inventory. | Proto-only candidate. Do not add as a planned Phase 18 route unless runtime logs or deeper IDA proof shows the client reaches it. |
| `getreitai.php` | `GetreitaiRequest` / `GetreitaiResponse` exist in `proto/red/taiko.proto`. | Not present in the captured IDB route suffix inventory. | Proto-only candidate. Do not add as a planned Phase 18 route unless runtime logs or deeper IDA proof shows the client reaches it. |

## Explicit Absent Or Deferred Surfaces

| Surface | Evidence | Phase 18 Decision |
|---------|----------|-------------------|
| Red item shop | No `Getitemshopinfo*` or `Itempurchase*` messages found in Red proto; no IDB route suffix recorded. | Do not add Red item-shop routes, shop state, shop folders, medal handling, or Red shop settings. |
| Blue battle | No Red battle userdata or battle route messages; no `battleuserdata.php` route suffix recorded. | Keep battle absent. Do not mirror Blue battle state or token behavior. |
| WaiWai | Red proto inventory does not expose WaiWai fields, and the roadmap excludes Red WaiWai work. | Keep WaiWai absent. |
| AI/ghost, token-count, later-era medal/shop behavior | No current Red proto/IDA evidence for these surfaces. | Keep absent unless later local evidence proves otherwise. |
| Red `Ac15EraProfiles.Red` | Phase 18 context defers profile binding until IDB-backed limits and the capability matrix are complete. | Do not add a Red AC15 profile in Phase 18. |
| Red EF migrations and gameplay tables | Phase 18 forbids Red gameplay persistence. | Do not add save, score, Dan, Tokkun, ChallengeCompe, item-shop, medal, wallet, payment, coupon, or transaction tables. |
| Red AdminApi/WebUI | Phase 22 owns Red admin and WebUI readback after runtime surfaces exist. | Do not add AdminApi/WebUI Red surfaces in Phase 18. |

## Red Protocol Limits And Byte Formats

Red protocol limits, flag-array widths, packing rules, crown byte layout, and response byte formats remain unresolved before any `Ac15EraProfiles.Red` binding. Do not inherit Blue, Green, or Yellow protocol limits by proximity. Phase 19 must prove the Red limits from IDB/runtime evidence before shared AC15 profile binding.

## Unresolved Evidence Gaps

- Manual RPCS3/cabinet smoke has not yet proven the Red request sequence, missing-content-type behavior, direct-protobuf framing, or whether proto-only routes are reached.
- Red protocol limits, flag-array widths, packing, crown byte layout, and response byte formats are not yet IDB-proven enough for `Ac15EraProfiles.Red`.
- Route probes can prove routing and logging, but they do not prove profile, catalog, userdata, normal-play, Dani, Tokkun, ChallengeCompe, reward, payment, AdminApi, or WebUI semantics.
- Shared startup/version ownership is proven by IDB route strings, but Red HDD mapping and startup movie readback still need later implementation and verification.

## Capability Matrix

This matrix groups Red surfaces by phase owner. It is a planning and preservation artifact, not a runtime implementation contract. "Candidate" means the local proto, IDB, or data evidence justifies later investigation; it does not mean Phase 18 implements behavior.

### Phase 18 foundation

| Surface | Classification | Evidence | Phase 18 Bound |
|---------|----------------|----------|----------------|
| Red evidence artifact | Supported foundation work | Plan 18-01 plus IDA/proto/data evidence. | This document is canonical for route/version/root and capability inventory until later evidence updates it. |
| Red adapter and generated wire | Supported foundation work | `proto/red/taiko.proto` and `proto/red/vsinterface.proto`; research verified repo-local `protogen` generation. | Plan 18-02 may add adapter-local wire from immutable proto inputs. |
| Red route probes for IDB-known suffixes | Supported foundation work | IDB-known suffixes listed above from `18-RESEARCH.md` lines 171-192. | Probes are routing/logging only and must not call gameplay services or write state. |
| Host enabled-era gating | Supported foundation work | Existing Host application-part gating pattern. | Plan 18-03 owns Red enablement; disabled Red routes must stay absent. |
| Shared startup/version ownership | Supported foundation work | `/v01r00` at `0xDA76A0`; startup/version suffixes at `0xDA8330`, `0xDA8348`, and `0xDA8360`. | Keep shared `/v01r00/chassis/*`; Red must not duplicate these routes under `/v08r01`. |

### Phase 19 catalog/profile

| Surface | Classification | Evidence | Later-Phase Bound |
|---------|----------------|----------|-------------------|
| Active Red catalog root | Candidate | `ST8100-1` IDB root strings and local `config/ST8100-1` files. | Phase 19 binds catalog loaders only after parser/profile proof. |
| Music and medley catalog | Candidate | `musicinfo.xml` and `musicmedleyinfo.xml` exist under active `ST8100-1`. | Candidate catalog binding, not Phase 18 runtime support. |
| Tuning and default music data | Candidate | `fumen/tuning.bin` and `defmusic.bin` exist locally. | Candidate catalog binding, not a protocol-limit claim. |
| Telop, folder, recommendation, movie, and customization data | Candidate | Proto/route surfaces exist for telop/folder/recommend, and local data has `movie/` plus `nutdata/`. | Phase 19 must prove sidecar/data shape before responses are treated as supported. |
| Red `Ac15EraProfile` | Later-phase only | D-05 and D-09 require IDB-backed limits before profile binding. | Do not add `Ac15EraProfiles.Red` in Phase 18. |

### Phase 20 runtime/simple compatibility

| Surface | Classification | Evidence | Later-Phase Bound |
|---------|----------------|----------|-------------------|
| Identity/profile via `baidcheck.php` and `mydonentry.php` | Candidate | IDB routes `0xDAA710` and `0xDAA728`; `BAIDRequest`, `BAIDResponse`, `MydonEntryRequest`, and `MydonEntryResponse`. | Phase 20 owns Red-owned identity/profile behavior. |
| Userdata via `userdata.php` | Candidate | IDB route `0xDAA740`; `UserDataRequest` / `UserDataResponse`. | Phase 20 owns Red-owned userdata readback. |
| Normal play via `playresult.php` | Candidate | IDB route `0xDA96F8`; `PlayResultRequest` / `PlayResultResponse`. | Phase 20 owns normal score/crown/profile writes after no-cross-mode proof. |
| Self-best via `selfbest.php` | Candidate | IDB route `0xDAA7E8`; `SelfBestRequest` / `SelfBestResponse`. | Phase 20 owns Red self-best state. |
| Crowns via `crownsdata.php` | Candidate | IDB route `0xDAA7B8`; `CrownsDataRequest` / `CrownsDataResponse`. | Crown byte format and width remain unresolved before profile binding. |
| Dani/Taikojuku via `taikojuku.php` | Candidate | IDB route `0xDAAE88`; `TaikojukuRequest` / `TaikojukuResponse`. | Phase 20 owns Red Dani state after Phase 19 catalog/profile binding. |
| Tokkun tutorial | Candidate | `tokkun_tutorial_flg` exists in `UserDataResponse` and `PlayResultRequest`; `ary_tokkunstage_info` exists in `PlayResultRequest`. | Phase 20 may persist/read back tutorial state only; no raw Tokkun history, rewards, score, crown, challenge, or unlock writes are implied. |
| Reward card via `rewardcardcheck.php` | Compatibility/probe-only candidate | IDB route `0xDAA818`; `RewardcardcheckRequest` / `RewardcardcheckResponse`. | No item shop, medal, unlock, wallet, payment, coupon, settlement, receipt, or transaction authority. |
| Reward execution via `rewardexecution.php` | Compatibility/probe-only candidate | IDB route `0xDAA838`; `RewardexecutionRequest` / `RewardexecutionResponse`. | No reward unlock state or later-era shop behavior is implied. |
| Don point and reward fields | Compatibility/probe-only candidate | Red proto fields include `reward_ptn`, `reward_progress`, `get_donpoint`, `total_get_donpoint`, and `total_use_donpoint`. | Treat as simple profile/protocol compatibility only when runtime evidence requires it. |
| Banacoin error via `banacoinerrorlog.php` | Compatibility/probe-only candidate | IDB route `0xDA9710`; `BanacoinerrorlogRequest` / `BanacoinerrorlogResponse`. | Log/probe only; no wallet, balance, payment, coupon, settlement, receipt, or transaction authority. |
| Balance check via `balancecheck.php` | Compatibility/probe-only candidate | IDB route `0xDAA778`; `BalancecheckRequest` / `BalancecheckResponse`. | No balance authority or persisted wallet state. |
| Banacoin payment via `banacoinpayment.php` | Compatibility/probe-only candidate | IDB route `0xDAA798`; `BanacoinpaymentRequest` / `BanacoinpaymentResponse`. | No payment authority, deduction, receipt, settlement, coupon, or transaction persistence. |
| `getbanacoininfo.php` | Proto-only candidate | `GetbanacoininfoRequest` / `GetbanacoininfoResponse` exist, but no captured IDB route suffix. | Not a planned Phase 18 route; add only if runtime or deeper IDA proof requires it. |
| `getreitai.php` | Proto-only candidate | `GetreitaiRequest` / `GetreitaiResponse` exist, but no captured IDB route suffix. | Not a planned Phase 18 route; add only if runtime or deeper IDA proof requires it. |

### Phase 21 ChallengeCompe

| Surface | Classification | Evidence | Later-Phase Bound |
|---------|----------------|----------|-------------------|
| `challengecompe.php` | Shared older-AC15 candidate | IDB route `0xDAA758`; `ChallengeCompeRequest` / `ChallengeCompeResponse`. | Shared older-AC15 candidate, not a Red-only stateful feature. Phase 21 owns contract and stateful semantics. |
| Playresult challenge arrays | Shared older-AC15 candidate | `ary_challenge_id`, `ary_user_compe_id`, and `ary_bng_compe_id` exist on Red `PlayResultRequest.StageData`. | Preserve as evidence for Phase 21; do not write challenge state in Phase 18. |
| Userdata challenge flag | Shared older-AC15 candidate | `is_challengecompe` exists on Red `UserDataResponse`. | Readback semantics are unproven until Phase 21. |

### Explicit absent surfaces

| Surface | Classification | Evidence | Phase 18 Bound |
|---------|----------------|----------|----------------|
| Red item shop | Explicit absent | No Red item-shop request/response messages or IDB route suffixes found. | Do not add item-shop routes, state, settings requirements, or unlock mirrors. |
| Blue battle | Explicit absent | No Red battle userdata messages or `battleuserdata.php` route suffix. | Do not mirror Blue battle state, stage graph behavior, boss behavior, token behavior, or battle readback. |
| WaiWai | Explicit absent | No Red WaiWai proto fields; roadmap excludes Red WaiWai work. | Do not add a Red WaiWai mode or tutorial behavior. |
| AI/ghost | Explicit absent | No current Red proto/IDA evidence. | Do not add ghost state, ghost routes, or Green AI Battle behavior. |
| Token-count and shop-folder behavior | Explicit absent | No current Red proto/IDA evidence and later-era shop behavior is out of scope. | Do not add token-count, shop-folder, or medal/shop compatibility. |
| Later-era medal/shop behavior | Explicit absent | Red proto has Don point/reward fields, not Yellow/Blue/Green shop medal semantics. | Do not map Red reward fields to medal or item-shop systems. |
| Wallet/payment/coupon/transaction authority | Explicit absent | Banacoin-adjacent proto/routes are compatibility candidates only. | Do not add wallet, balance authority, payment deduction, coupon, settlement, receipt, or transaction persistence. |
| Red `Ac15EraProfile` | Explicitly deferred | D-05 and D-09 defer profile binding. | Do not add `Ac15EraProfiles.Red` in Phase 18. |
| Red EF migrations | Explicit absent | Phase 18 forbids gameplay persistence. | Do not add Red EF migrations. |
| Red gameplay tables | Explicit absent | Runtime state belongs to Phase 20/21 only after evidence. | Do not add Red save, score, crown, Dan, Tokkun, ChallengeCompe, shop, medal, wallet, payment, coupon, or transaction tables. |
| AdminApi | Explicit absent | Phase 22 owns admin readback after runtime surfaces exist. | Do not add Red AdminApi routes in Phase 18. |
| WebUI | Explicit absent | Phase 22 owns UI readback after runtime surfaces exist. | Do not add Red WebUI flows in Phase 18. |

## Preservation Guardrails

- Existing supported-era preservation is a Phase 18 requirement: Green, Blue, Yellow, Nijiiro, shared `/v01r00`, and shared `Application/Ac15` behavior must remain unchanged except for narrowly scoped Red foundation work in later Plan 18 tasks.
- Do not add Red gameplay persistence: Phase 18 must not write Red or existing-era save, score, crown, Dan, Tokkun, ChallengeCompe, shop, medal, wallet, payment, coupon, or transaction state.
- Do not add Red EF migrations: Red tables and migrations belong only to later runtime phases after evidence proves state shape.
- Do not add Ac15EraProfiles.Red: Red protocol limits, flag-array widths, packing, crown bytes, and response byte formats remain unresolved.
- Do not add AdminApi/WebUI Red: Red admin and WebUI surfaces belong to Phase 22 after implemented Red-owned runtime readback exists.
- Do not treat `challengecompe.php` as Red-only or stateful in Phase 18; it is a shared older-AC15 candidate whose contract belongs to Phase 21.
- Do not treat `rewardcardcheck.php`, `rewardexecution.php`, `balancecheck.php`, `banacoinpayment.php`, or `banacoinerrorlog.php` as wallet, payment, coupon, receipt, settlement, transaction, item-shop, medal, or unlock authority.
- Do not add proto-only `getbanacoininfo.php` or `getreitai.php` as Phase 18 planned routes unless runtime logs or deeper IDA proof show the Red client reaches them.
