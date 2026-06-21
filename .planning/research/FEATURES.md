# Feature Research: v1.5 Murasaki AC15 Support

**Domain:** Brownfield TaikoLocalServer AC15 era support
**Researched:** 2026-06-21
**Confidence:** MEDIUM

## Evidence Strength

Local repo evidence is authoritative for feature planning. Evidence strength in this file uses this order:

1. **Local proto/data/code** - checked files in this repo, including `proto/murasaki/*` and `Host/wwwroot/data/murasaki/data`.
2. **Binary/client/cabinet evidence** - IDA, route strings, logs, captures, RPCS3/cabinet behavior. This is still needed for route inventory, active root, request ordering, and byte-heavy semantics.
3. **Cross-era implementation evidence** - existing Green/Blue/Yellow/Red/White AC15 capability patterns. Useful for reuse only after Murasaki wire/data shapes match.
4. **Wiki/product context** - useful for visible gameplay scope, dates, caps, and feature names, but never sufficient to define server behavior.

Overall confidence is **MEDIUM** because Murasaki proto/data evidence is strong for the message inventory and catalog inputs, while route extraction, active-root choice, global high-score semantics, song-hash bytes, default/mainichi hashes, shopping semantics, and reserved bytes still need binary/client/cabinet proof.

## Feature Landscape

### Table Stakes (Users Expect These)

Features users assume exist. Missing these = Murasaki support feels incomplete or unsafe to implement.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| First-class Murasaki era foundation | Existing AC15 support treats Blue, Yellow, Red, and White as first-class eras; Murasaki must not be a White variant. | MEDIUM | Evidence: `.planning/PROJECT.md`, `proto/murasaki/taiko.proto`, `proto/murasaki/vsinterface.proto`. Add `GameEra.Murasaki`, generated Murasaki wire DTOs, adapter/controller project, Host settings/gating, Murasaki-owned persistence, and era-routed AdminApi/WebUI hooks. |
| Route and transport proof | Cabinet compatibility depends on exact `.php` route names, prefix, and protobuf transport. | HIGH | Evidence: user context says `/v01r00` startup and `/v06r00` game requests, with `.php` paths and direct protobuf. Still require binary/client/log confirmation before locking route inventory. |
| Shared startup/version ownership | Murasaki `vsinterface.proto` exposes the same startup/verup message family as older AC15 startup routing. | MEDIUM | Evidence: `StartupAuth*`, `VerupAuth*`, `VerupComplete*` in `proto/murasaki/vsinterface.proto`. Reuse shared `/v01r00` only where route/client evidence matches. |
| Active catalog root and loader binding | Local Murasaki data has multiple config roots, so the server must bind the correct runtime root before building catalog-dependent features. | HIGH | Evidence: `Host/wwwroot/data/murasaki/data/config/common`, `ST5100-1`, `ST5100-7`, `ST6100-1`, plus `fumen/tuning.bin`. Required files include `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, and `spacialbaid.xml` for each version root. Active root must come from binary/client/cabinet evidence, not filename guessing. |
| Murasaki-owned wire mapping through application DTOs | Proto shape differs from White; direct handler persistence of wire DTOs would bake in the wrong boundary. | MEDIUM | Evidence: `proto/murasaki/taiko.proto` diverges from White around metadata and additional request families. Controllers should deserialize, map into Common/AC15 application shapes, call Mediator, then map back. |
| BAID, mydon entry, profile, and userdata | Normal cabinet login and readback are core AC15 support. | MEDIUM | Evidence: `BAID*`, `MydonEntry*`, and `UserData*` in Murasaki proto. Murasaki userdata exposes favorites, recent songs, release-song hash, challenge arrays, display settings, options/tone/title flags, reward progress, Don Point totals, and default option/shin settings. State must be Murasaki-owned except shared identity. |
| Favorite and recent song support with Murasaki limits | Favorites are user-visible and Murasaki raises the visible favorite cap compared with older context. | MEDIUM | Evidence: Murasaki proto exposes `ary_favorite_song_no`, `song_favorite_cnt`, and playresult `is_favorite`; wiki/product context and orchestrator verification say favorite limit is 10. Implement Murasaki limit 10 only for Murasaki, and do not regress other AC15 era limits. |
| Normal playresult persistence | A card-using Murasaki play session must upload results and read back scores, crowns, favorites, recents, rewards, and profile counters. | HIGH | Evidence: `PlayResultRequest`, `SelfBest*`, `CrownsData*`, and `UserData*` in local proto. Reuse AC15 normal-play services where field meanings match, but keep Murasaki tables, mappers, limits, byte packing, and no-cross-era boundaries separate. |
| Self-best and crown readback | Song select and score screens expect existing best scores and crown state. | MEDIUM | Evidence: Murasaki `SelfBestResponse` has normal and Shin self-best arrays; `CrownsDataResponse` has `song_hash_ver` and `hash_crown_flg`. Byte lengths/compression must be verified against Murasaki data/client behavior. |
| Split metadata request family | Murasaki replaces White's monolithic `initialdatacheck` shape with separate metadata requests; copying White `initialdatacheck.php` would be wrong. | HIGH | Evidence: Murasaki-only `defaultsong`, `mainichisong`, `foldercheck`, `getfolder`, `telopcheck`, `gettelop`, `songhash`, and `bestscore` messages. Build route compatibility for proven calls, but keep hash/table byte semantics evidence-gated. |
| `defaultsong.php` compatibility | The cabinet likely asks separately for default-song availability hashes. | HIGH | Evidence: `DefaultsongResponse.song_hash_ver` and `hash_default_song_flg`. Table-stakes route if binary/client evidence proves the path; byte contents must come from existing AC15 packing rules or a new Murasaki binary pass, not guessed arrays. |
| `mainichisong.php` compatibility | Mainichi Dojo/default daily song hashes are no longer bundled in `initialdatacheck`. | HIGH | Evidence: `MainichisongResponse.hash_mainichidojo_all` and `hash_mainichidojo_rare`. Treat as a separate Murasaki metadata capability; do not infer White field placement. |
| `foldercheck.php` / `getfolder.php` compatibility | Wiki/product context says feature folders were added, and Murasaki proto splits folder ids from folder contents. | MEDIUM | Evidence: `FoldercheckResponse.folder_id`, `GetfolderRequest.folder_id/hdd_ver`, `GetfolderResponse.song_no`, and local config roots. Sidecar or data-derived folder content must be explicit and Murasaki-owned. |
| `telopcheck.php` / `gettelop.php` compatibility | Murasaki splits telop id discovery from telop body fetch. | MEDIUM | Evidence: `TelopcheckResponse.telop_id`, `GettelopResponse.start_datetime/end_datetime/telop`. Use Murasaki-owned sidecar/defaults; do not wire White `initialdatacheck` telop arrays. |
| `songhash.php` compatibility | Murasaki has a dedicated song hash table request. | HIGH | Evidence: `SonghashResponse.song_hash_ver` and `song_hash_tbl`. This is table-stakes only as a proven route/response surface; exact table format requires binary/client proof. |
| `bestscore.php` compatibility | Murasaki appears to ask for global top scores by sequence, which is not the same as local self-best. | HIGH | Evidence: `BestScoreRequest.seq_id`, `BestScoreResponse.last_seq_id`, and nested best-3 rank score/name rows. Implement after binary/client pass defines sequencing and expected contents. Until then, avoid converting local self-best into fake global rankings. |
| Taikojuku/Dani | Murasaki proto and local medley data support the older AC15 Dani flow. | MEDIUM | Evidence: `Taikojuku*`, `UserDataResponse.disp_taikojuku_dan`, `PlayResultRequest.dan_result`, and `musicmedleyinfo.xml` in each root. Reuse AC15 Dani helpers with Murasaki-specific catalog root and wire placement. |
| Reward, present, and Don Point readback | Murasaki presents and Don Points are visible progression systems. | MEDIUM | Evidence: `present.xml` exists in each root; `ST5100-1` and `ST6100-1` present thresholds run through 30000, while `ST5100-7` extends beyond that. Proto exposes `reward_ptn`, `reward_progress`, `get_donpoint`, `total_get_donpoint`, and `total_use_donpoint`. Use active-root evidence before setting caps; orchestrator/wiki context says initial Murasaki cap is 30000. |
| `shoppingresult.php` route classification | Murasaki has a shopping result upload, but the semantics are not the same as Yellow item shop or Red reward execution. | HIGH | Evidence: `ShoppingResultRequest` uploads `use_donpoint`, tone/costume byte flags, and purchased song ids; response returns updated totals, flags, and release-song hash. Route compatibility and persistence need binary/client proof before mutation. |
| `communicationlog.php`, `bookkeeping.php`, `heartbeat.php`, and `headclerk2.php` compatibility | Older AC15 clients commonly call operational endpoints during normal operation. | LOW/MEDIUM | Evidence: local proto has these messages. Start with log-and-success/no-state behavior where route evidence proves calls. Do not infer economy, audit, or settlement semantics from field names. |
| Challenge array compatibility | Murasaki userdata/playresult has challenge arrays, but no standalone ChallengeCompe route. | HIGH | Evidence: `ary_challenge_stat`, `ary_user_compe_stat`, `ary_bng_compe_stat`, and stage challenge id arrays are present; standalone `ChallengeCompeRequest/Response` is absent. Preserve safe empty/readback compatibility until Murasaki-specific Don Challenge evidence is collected. |
| AdminApi/WebUI parity for implemented Murasaki state | Users need to inspect and edit supported Murasaki profiles without touching White/Red/Yellow state. | MEDIUM | Evidence: project architecture and prior AC15 milestones. Expose only implemented Murasaki-owned profile, scores, crowns, favorites/recents, Dani, rewards, and any proven challenge surfaces. |
| Automated and user-observed verification | Passing server tests alone is not enough for cabinet protocol compatibility. | MEDIUM | Evidence: project requirements and shipped AC15 milestone pattern. Include route/handler/catalog/persistence tests, generated-source inspection where mappers are involved, temp-output Host build if needed, and cabinet/RPCS3 smoke before closeout. |

### Differentiators (Competitive Advantage)

Features that make Murasaki support robust, but should follow the table-stakes compatibility path.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Murasaki-specific metadata capability layer | Prevents White-like code from leaking into Murasaki while still reusing AC15 catalog primitives. | HIGH | Build a dedicated application shape for split default/mainichi/folder/telop/songhash responses once route ordering and byte formats are known. |
| Real `bestscore.php` global ranking model | Could reproduce cabinet ranking panels better than local-only score readback. | HIGH | Requires binary/client/capture evidence for `seq_id`, pagination, `last_seq_id`, rank ordering, difficulty indexing, and whether empty/global/shared rows are acceptable. |
| Evidence-backed song hash/default/mainichi byte generation | Reduces brittle compatibility stubs and makes updates/root changes deterministic. | HIGH | Requires deriving byte table lengths and bit ordering from Murasaki client behavior or binary evidence. Existing AC15 packers are candidates only after comparison. |
| Proven `shoppingresult.php` persistence | Could support Murasaki-era Don Point spending and purchased song/voice/costume unlocks. | HIGH | Needs binary/client evidence for what the upload means, whether the server is authoritative, and how response flags should be merged. Until then, log-only or minimal compatibility is safer. |
| Murasaki Don Challenge data/progress | If Murasaki-specific data and client flow are proven, server-side challenge progress would align Murasaki with Red/White quality. | HIGH | Wiki/product context shows Murasaki update entries with Don Challenge songs, and proto has embedded challenge arrays. That is not enough by itself. Need Murasaki sidecar/proven bundles and route/readback/write semantics. |
| Active-root version strategy | Supporting `ST5100-1`, `ST5100-7`, and `ST6100-1` cleanly could handle multiple Murasaki dumps/updates. | MEDIUM | First milestone should lock one proven active root. Multi-root selection is useful later if real clients/logs require it. |
| Murasaki-specific WebUI capability profile | Keeps older-era UI compact and avoids exposing unsupported White-final/Red/Yellow controls. | MEDIUM | Should be driven by implemented capability groups: profile, costume, favorites, scores, Dani, rewards, and proven challenge/shopping surfaces. |

### Anti-Features (Commonly Requested, Often Problematic)

Features that seem good but create incorrect Murasaki behavior without evidence.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Treat Murasaki as a White variant | Murasaki looks White-like and shares many older AC15 fields. | Murasaki proto lacks White `initialdatacheck` and White reward route messages, and adds separate metadata/high-score/shopping/log requests. | Add first-class Murasaki wire/controllers/state; reuse AC15 services only behind Murasaki mappers and capability profiles. |
| Copy White `initialdatacheck.php` | White already bundles default songs, mainichi hashes, telops, folders, and Taikojuku metadata. | Murasaki has no `InitialdatacheckRequest/Response`; metadata is split across separate requests. | Implement proven `defaultsong`, `mainichisong`, `foldercheck/getfolder`, `telopcheck/gettelop`, `songhash`, and `bestscore` routes. |
| Fake global high scores from local self-best | `bestscore.php` looks like score readback, and local scores are available. | Murasaki `BestScoreResponse` is sequence-based and nested around best-3 rank rows with names; local self-best is a different contract. | Return only evidence-backed empty/global rows until binary/client proof defines behavior. Keep self-best on `selfbest.php`. |
| Invent song-hash/default/mainichi bytes | The proto names byte fields clearly. | Field names do not prove byte length, bit order, compression, versioning, or update semantics. Bad bytes can break song select. | Derive from binary/client/capture evidence or reuse existing packers only after comparing expected payloads. |
| Interpret `reserved`, `content_info`, or `default_option_setting` by name | These fields look useful for capability flags or defaults. | Byte-heavy fields are high-risk and may be opaque client contracts. | Preserve/pass through only if observed; otherwise use conservative defaults and document unresolved bytes. |
| Clone Red standalone `challengecompe.php` | Murasaki has challenge arrays and wiki context mentions Don Challenge. | Murasaki proto does not define `ChallengeCompeRequest/Response`; Red's route is a different protocol surface. | Keep embedded challenge arrays empty/safe until Murasaki-specific route/client evidence proves more. |
| Import White final 11.01 features | White final has newer compatibility work and may seem adjacent. | Murasaki is older and must not inherit final White Tokkun/Banacoin/difficulty-panel/heartbeat fields without local Murasaki evidence. | Use only `proto/murasaki`, local Murasaki data, and client evidence. |
| Add Banacoin wallet/payment routes | Some AC15 eras have Banacoin-adjacent compatibility routes. | Murasaki proto lacks `getbanacoininfo`, `balancecheck`, `banacoinpayment`, and `banacoinerrorlog`. | Keep absent unless binary/client evidence proves Murasaki calls a route outside the proto. |
| Add Blue battle, Yellow item shop, Tokkun, WaiWai, gacha, or tournaments | Existing eras support or expose some of these concepts. | Murasaki proto/data reviewed here does not prove these runtime systems; `shoppingresult` is not Yellow item shop, and `bestscore` is not tournament runtime. | Treat each as absent or future evidence-gated work. |
| Merge Murasaki state into White/Red tables | Saves implementation time. | Violates repo rule that AC15 era state stays separate except true shared identity; creates cross-era corruption. | Add Murasaki-owned save/history tables and reuse shared application algorithms. |
| Edit dumped `proto/` to fit existing code | Fast way to compile against existing mappers. | Proto is source evidence; manual edits destroy evidence and hide schema differences. | Generate Murasaki wire from the local dumped proto and adapt code around it. |
| Treat wiki as server-contract authority | Wiki is easy to browse and confirms visible product features. | Wiki cannot prove request paths, protobuf fields, byte packing, active root, or state mutation semantics. | Use wiki for scoping only; require local proto/data/binary/client evidence for implementation. |

## Feature Dependencies

```text
Route/transport proof
    -> Murasaki era foundation
        -> generated Murasaki wire DTOs
            -> Murasaki controllers and mappers
                -> runtime capability binding

Active data root proof
    -> catalog loading
        -> folder/telop/default/mainichi/songhash metadata
        -> Taikojuku/Dani
        -> reward/present/Don Point readback

BAID + mydon + userdata
    -> normal playresult persistence
        -> self-best/crowns/favorites/recents
        -> AdminApi/WebUI readback

Binary/client byte pass
    -> songhash/default/mainichi bytes
    -> bestscore global ranking
    -> shoppingresult mutation
    -> reserved/content_info/default_option_setting handling

Challenge array evidence + Murasaki challenge data
    -> server-side Don Challenge progress
    -> Don Challenge AdminApi/WebUI
```

### Dependency Notes

- **Route/transport proof before controllers:** The user context gives the intended route prefixes, but implementation should still verify route names and call order through binary/client/log evidence before claiming compatibility.
- **Active root before catalog features:** `ST5100-1`, `ST5100-7`, and `ST6100-1` all exist and differ. Root choice changes song counts, presents, hashes, and possible feature availability.
- **Split metadata before normal smoke:** If the cabinet expects `defaultsong`, `mainichisong`, `foldercheck`, `telopcheck`, or `songhash` before login/play, normal support can fail before userdata is reached.
- **Normal play before AdminApi/WebUI:** AdminApi/WebUI should expose only state the runtime actually persists and reads back for Murasaki.
- **Binary byte pass before semantic claims:** `bestscore`, `songhash`, `shoppingresult`, `reserved`, `content_info`, and `default_option_setting` are not safe to model from names alone.
- **Challenge compatibility before full Don Challenge:** Empty/proven embedded arrays are safer than importing Red `challengecompe.php` or White server-side challenge behavior prematurely.

## MVP Definition

### Launch With (v1.5)

Minimum viable Murasaki support for this milestone.

- [ ] First-class Murasaki era foundation - generated wire, adapter, Host gating, settings, Murasaki-owned persistence, and route scaffolding.
- [ ] Verified `/v01r00` startup and `/v06r00` game route ownership - direct-protobuf `.php` requests where local evidence proves them.
- [ ] Active Murasaki catalog root binding - one proven root among `ST5100-1`, `ST5100-7`, and `ST6100-1`, with `musicinfo`, `musicmedleyinfo`, `defmusic`, `present`, `spacialbaid`, and `fumen/tuning.bin`.
- [ ] BAID, mydon, userdata, profile, favorites, recents, options/tone/title/costume flags, reward progress, and Don Point totals - Murasaki-owned state, favorite cap 10 where verified.
- [ ] Split metadata routes - `defaultsong`, `mainichisong`, `foldercheck/getfolder`, `telopcheck/gettelop`, and `songhash` with evidence-backed or conservative payloads.
- [ ] Normal play runtime - `playresult`, self-best, crowns, release-song readback, score/crown/favorite/recent persistence, reward/Don Point updates, and no-cross-era writes.
- [ ] Taikojuku/Dani - use local `musicmedleyinfo.xml` and existing AC15 Dani patterns after field/call-order verification.
- [ ] Operational compatibility - `bookkeeping`, `heartbeat`, `headclerk2`, and `communicationlog` as proven no-state/log-success endpoints.
- [ ] AdminApi/WebUI parity for implemented Murasaki state - no unsupported controls.
- [ ] Verification - targeted tests, build, generated-source inspection where applicable, and user-observed cabinet/RPCS3 smoke before closeout.

### Add After Validation (v1.5.x)

Features to add once the core Murasaki loop is stable.

- [ ] Real `bestscore.php` global ranking - only after `seq_id`, row contents, and empty/default behavior are proven.
- [ ] Full song-hash/default/mainichi byte generation - after binary/client evidence defines table shape and bit ordering.
- [ ] `shoppingresult.php` mutation - after proving whether Murasaki expects authoritative Don Point spending, unlock merging, or a compatibility echo.
- [ ] Murasaki Don Challenge sidecar/progress - after local Murasaki data and client read/write behavior prove the server contract.
- [ ] Multi-root support - if real cabinet/client behavior needs multiple Murasaki version roots in one installation.

### Future Consideration (v2+)

Features to defer unless concrete Murasaki evidence appears.

- [ ] Banacoin wallet/payment/balance behavior - absent from local Murasaki proto.
- [ ] Tokkun/difficulty-panel/White-final behavior - not proven for Murasaki by the reviewed proto/data.
- [ ] Blue battle or AI battle behavior - no Murasaki evidence.
- [ ] Yellow-style item-shop seasons/Don-Katsu medal shop - `shoppingresult` needs its own Murasaki evidence and must not be mapped to Yellow shop by default.
- [ ] Standalone `challengecompe.php` cabinet route - absent from local Murasaki proto.

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| First-class Murasaki foundation | HIGH | MEDIUM | P1 |
| Route/transport proof | HIGH | HIGH | P1 |
| Active catalog root binding | HIGH | HIGH | P1 |
| BAID/mydon/userdata/profile | HIGH | MEDIUM | P1 |
| Normal playresult/self-best/crowns | HIGH | HIGH | P1 |
| Favorites/recents with Murasaki limits | HIGH | MEDIUM | P1 |
| Split metadata request family | HIGH | HIGH | P1 |
| Taikojuku/Dani | MEDIUM | MEDIUM | P1 |
| Reward/present/Don Point readback | MEDIUM | MEDIUM | P1 |
| Operational log/success routes | MEDIUM | LOW | P1 |
| AdminApi/WebUI parity | HIGH | MEDIUM | P1 |
| `bestscore.php` global ranking | MEDIUM | HIGH | P2 |
| `shoppingresult.php` state mutation | MEDIUM | HIGH | P2 |
| Murasaki Don Challenge progress | MEDIUM | HIGH | P2 |
| Multi-root version strategy | LOW/MEDIUM | MEDIUM | P2 |
| Banacoin/Tokkun/battle/standalone ChallengeCompe | LOW until proven | HIGH | P3 / anti-feature |

**Priority key:**
- P1: Must have for v1.5 launch or must be explicitly resolved as an evidence-gated compatibility surface.
- P2: Valuable after core runtime validation or if binary/client evidence pulls it into scope.
- P3: Future only; otherwise keep out.

## Existing-Era Feature Comparison

| Feature | White 0.13 / White Final | Red | Murasaki Approach |
|---------|---------------------------|-----|-------------------|
| Era foundation | White-owned adapter/wire/state with legacy/final split later | Red-owned adapter/wire/state | Murasaki-owned adapter/wire/state; no White variant naming. |
| Startup/version | Shared `/v01r00` where proven | Shared `/v01r00` where proven | Use `/v01r00` startup/version only after Murasaki route proof. |
| Initial metadata | White 0.13 has `initialdatacheck`; final White has additional fields on final routes | Red has `initialdatacheck` | Murasaki has no `initialdatacheck`; use split metadata routes. |
| Favorites | White-like older AC15 profile support | Red older AC15 support | Murasaki favorite cap is 10 by verified product context; keep era-specific limit. |
| Rewards/Don Points | Present/progress through White-owned state | Red rewards plus Don Challenge | Murasaki has present data and Don Point fields; shopping result is separate and evidence-gated. |
| Challenge/Don Challenge | Server-side stage-derived Don Challenge after White evidence; no standalone ChallengeCompe route for legacy White | Red has separate `challengecompe.php` compatibility plus server-side Don Challenge | Murasaki has embedded challenge arrays but no standalone ChallengeCompe proto; do not clone Red route. |
| Banacoin/Tokkun | White final support only where proven; legacy schema-separated | Red has Banacoin routes, not Murasaki proof | Keep absent for Murasaki unless local evidence appears. |
| Shopping | No Yellow-style shop by default | Reward routes, not Yellow shop | `shoppingresult.php` exists but semantics require binary/client proof. |

## Sources

- `.planning/PROJECT.md` - active v1.5 Murasaki scope, evidence hierarchy, route/root cautions, active and out-of-scope requirements.
- `.planning/MILESTONES.md` - shipped White/Red/Yellow/Blue milestone behavior and closeout expectations.
- `.codex/gsd-core/templates/research-project/FEATURES.md` - template structure adapted for this brownfield feature research.
- `proto/murasaki/taiko.proto` - Murasaki game protocol message inventory and field placement.
- `proto/murasaki/vsinterface.proto` - Murasaki startup/verup protocol message inventory.
- `proto/white/taiko.proto` - comparison source showing White `initialdatacheck` and White-only reward/getreitai surfaces.
- `proto/red/taiko.proto` - comparison source showing Red-only Banacoin and standalone `ChallengeCompe*` surfaces.
- `Host/wwwroot/data/murasaki/data/config` - local Murasaki config roots: `common`, `ST5100-1`, `ST5100-7`, `ST6100-1`.
- `Host/wwwroot/data/murasaki/data/fumen/tuning.bin` - local Murasaki tuning input.
- `Host/wwwroot/data/murasaki/data/config/*/present.xml` - local Don Point/present thresholds; `ST5100-1` and `ST6100-1` reach 30000, `ST5100-7` contains later higher thresholds.
- Public wiki update log, scoping only: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%82%A2%E3%83%83%E3%83%97%E3%83%87%E3%83%BC%E3%83%88%E5%B1%A5%E6%AD%B4/%E3%83%A0%E3%83%A9%E3%82%B5%E3%82%AD
- Public wiki AC15 history, scoping only: https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/AC%E3%81%AE%E6%AD%B4%E5%8F%B2/AC15#murasaki
- User-provided milestone context in this research request - `/v06r00` game prefix, Murasaki start date, favorite cap 10, Don Point cap 30000, feature-folder context, and known route-family concerns.

---
*Feature research for: v1.5 Murasaki AC15 Support*
*Researched: 2026-06-21*
