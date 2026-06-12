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
