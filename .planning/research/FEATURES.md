# Feature Research: v1.7 MOMOIRO AC15 0.11 Support

**Domain:** Brownfield TaikoLocalServer AC15 era support
**Researched:** 2026-06-25
**Confidence:** MEDIUM

## Evidence Strength

Local repo evidence is authoritative for feature planning. Evidence strength in this file uses this order:

1. **Project contract and local protocol/data** - `.planning/PROJECT.md`, `proto/momoiro/taiko.proto`, `proto/momoiro/vsinterface.proto`, and `Host/wwwroot/data/momoiro/data`.
2. **Binary/client/cabinet evidence** - `.tools/momoiro/EBOOT.ELF.i64`, route strings, IDA analysis, logs, captures, and RPCS3/cabinet behavior. This research inventoried the local IDB only; it does not claim route or byte-limit facts from binary analysis.
3. **Cross-era implementation evidence** - existing AC15 capability patterns from KIMIDORI, Murasaki, White, Red, Yellow, Blue, and Green. Useful only after MOMOIRO proto and binary route evidence match.
4. **Wiki/product context** - useful for version-era scoping, dates, Don Point context, and public feature timing. It is secondary evidence and never sufficient to define server behavior.

Overall confidence is **MEDIUM**. The local proto and root-level data inventory strongly define the candidate feature surface, but MOMOIRO 0.11 still needs binary/client research before locking `.php` route inventory, song unlock packing, crown packing, favorites/recent limits, challenge-array semantics, and Don Point/shopping limits.

## Feature Landscape

### Table Stakes (Users Expect These)

Features users assume exist. Missing these = MOMOIRO support feels incomplete or unsafe to implement.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| First-class MOMOIRO era foundation | Existing AC15 eras are first-class and MOMOIRO must not be a KIMIDORI/Murasaki alias. | MEDIUM | Add `GameEra.Momoiro`, MOMOIRO settings, DI/application-part gating, generated wire DTOs from `proto/momoiro`, MOMOIRO-owned adapter/controller routes, persistence, and AdminApi/WebUI route registration. |
| Shared startup/version routing | User contract says MOMOIRO 0.11 uses `/v01r00/chassis` for startup/version; `vsinterface.proto` exposes `StartupAuth`, `VerupAuth`, and `VerupComplete`. | MEDIUM | Reuse shared AC15 startup/version handling where HDD/version detection and direct-protobuf transport are proven for MOMOIRO. Do not add era-local startup controllers unless current evidence requires it. |
| `/v04r00/chassis/*.php` game route skeleton | User contract says non-startup game routes use `/v04r00` and all routes are `.php`. | HIGH | Proto names imply candidate route families, but implementation should still perform binary route-string proof before each route is considered supported. |
| Root-level catalog loading | MOMOIRO data is under `Host/wwwroot/data/momoiro/data` with root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`. | HIGH | Do not hardcode newer `config/STxxxx-*` layout. Catalog loading should follow the KIMIDORI/root-level path shape and validate required key files before route behavior depends on them. |
| BAID and mydon/profile support | `BAIDRequest/Response` and `MydonEntryRequest/Response` exist and include access code, BAID, mydon name, costume flags, reward pattern, and Dan display fields. | MEDIUM | Support card lookup/registration/profile defaults through MOMOIRO-owned save state. Preserve title id `0` semantics from repo policy where title state is used. |
| UserData readback | `UserDataResponse` is the main readback surface and contains favorites, recent songs, release-song hash, crowns, challenge arrays, options/tone/title flags, reward progress, category counts, recommendation fields, and Don Point totals. | HIGH | Crown data is inside userdata through `hash_crown_flg`; do not add `crownsdata.php`. Packing and array limits require binary/client proof before final implementation. |
| Normal playresult persistence | `PlayResultRequest` carries per-stage score facts, favorites/recent flags, `release_song_no`, options/tone/costume/title flags, Don Point/reward fields, Dan fields, and challenge id arrays. | HIGH | Persist normal score, crown, recent/favorite, unlock, Don Point, reward, and Dan state only after MOMOIRO-specific field placement and limit proof. Keep state MOMOIRO-owned. |
| Self-best readback | `SelfBestRequest/Response` exists and returns normal, ura, and shin self-best arrays by requested song/level. | MEDIUM | Use existing AC15 self-best capability only after MOMOIRO level/song ordering and response limits are verified. |
| Crowns-in-userdata readback | User specifically called out crowns inside userdata; proto confirms `hash_crown_flg` in `UserDataResponse`. | HIGH | Binary research must prove byte packing, song-order basis, level/ura/shin placement, compression expectations if any, and maximum song count. |
| Song unlocking/readback | Proto exposes `release_song_no` in playresult, `hash_release_song_flg` in userdata and shopping result, plus `song_hash_ver`. | HIGH | This is a required binary research gate. Do not copy KIMIDORI/Murasaki unlock byte semantics until MOMOIRO 0.11 packing and route use are proven. |
| Favorites and recent songs | `UserDataResponse` repeats `ary_favorite_song_no` and `ary_recent_song_no`; playresult stages include `is_favorite` and `is_recent`. | HIGH | Limits are not encoded in proto. Treat later-era favorite limits as unproven; binary/client proof decides MOMOIRO count, ordering, duplicate handling, and truncation. |
| Default, mainichi, songhash metadata | Proto exposes `defaultsong`, `mainichisong`, and `songhash` request/response families. | MEDIUM | Implement from MOMOIRO catalog data once route proof exists. `song_hash_tbl`, `hash_default_song_flg`, and mainichi bytes are byte-heavy and need client/binary validation. |
| Telop metadata | Proto exposes `telopcheck` and `gettelop` request/response families. | LOW | No committed MOMOIRO telop sidecar was observed in `Host/wwwroot/data/momoiro/`; add the route shape only if binary route proof exists, and return safe empty/success content until data exists. |
| Recommendations | Proto exposes `RecommendRequest/Response`, and userdata repeats recommendation fields. | LOW | Use deterministic catalog-backed recommendations or safe empty values. Do not infer personalized server recommendation semantics from newer eras. |
| Best score, heartbeat, bookkeeping, and communication logs | Proto exposes these compatibility surfaces. | LOW | Keep no-state/log-and-success or bounded readback behavior unless MOMOIRO client evidence proves a stateful role. |
| Don Point, reward, and shopping-result compatibility | Proto exposes `get_donpoint`, `reward_ptn`, `reward_progress`, `use_donpoint`, `total_get_donpoint`, `total_use_donpoint`, `ary_shopping_song_no`, and shopping release-song hash readback. | HIGH | Support MOMOIRO-owned totals and unlock flags after binary proof. Wiki context says early MOMOIRO had a 30000 point cap, but the cap must be confirmed against 0.11 binary/client behavior before hardcoding. |
| Challenge arrays as bounded protocol state | Proto includes challenge id arrays on playresult stages and challenge/user/bng stat arrays in userdata. | HIGH | Accept and preserve only evidence-backed normal payload fields. Arrays alone do not prove `challengecompe.php`, Don Challenge, reward side effects, or external challenge authority. |
| Dani Dojo state, separate from Taikojuku | Proto includes Dan readback/upload fields: `disp_dan_type`, `got_dan_max`, `got_dan_flg`, stage `play_dan`, and `dan_result`. | HIGH | Dan state is a candidate table-stakes feature if binary route/playresult behavior proves it. The proto does **not** expose a `TaikojukuRequest/Response` family, so do not add Taikojuku practice-folder routes. |
| AdminApi/WebUI readback for implemented state | Existing eras expose supported state through era-aware AdminApi/WebUI surfaces. | MEDIUM | Expose only implemented MOMOIRO-owned profile, scores, crowns, recent/favorite, unlock, Don Point/reward, Dan, and challenge-compatible state. No cross-era writes. |

### Deferred or Future Surfaces

Features to hold until stronger MOMOIRO-specific evidence exists.

| Feature | Why Deferred | Required Evidence |
|---------|--------------|-------------------|
| Exact `.php` route inventory | Proto message names and user route prefix are not enough to prove every handler the binary calls. | Binary route strings, IDA route table, client log/capture, or cabinet/RPCS3 request trace. |
| Song unlock byte semantics | `release_song_no` and `hash_release_song_flg` exist, but packing/order/limits are not self-describing. | Binary/client proof of song-hash version, release bitset length, song-number ordering, and deleted-song behavior. |
| Crown byte semantics | `hash_crown_flg` is in userdata, but byte layout and limits are unknown. | Binary/client proof for level placement, ura/shin handling, compression, song count, and SORAIRO removed-song crown caveats. |
| Favorites/recent limits | Repeated fields do not encode maximum count or ordering. | Binary/client proof for maximum favorite count, recent count, truncation order, and duplicate policy. |
| Don Point cap and shopping limits | Public wiki context suggests a 30000 launch cap and later shop-point changes, but that is not protocol proof for 0.11. | MOMOIRO 0.11 binary/client proof for cap, overflow behavior, total counters, and `shoppingresult.php` unlock effects. |
| Challenge-array semantics | The proto arrays may be passive readback, ranking metadata, or ignored compatibility. | Client/binary proof of what the cabinet expects in `ary_challenge_stat`, `ary_user_compe_stat`, `ary_bng_compe_stat`, and per-stage ids. |
| Dani result semantics | Dan fields exist, but exact clear-grade/rank packing and route expectations need proof. | Binary/client proof for Dan mode classification, `play_dan`, `dan_result`, `got_dan_flg`, and `got_dan_max`. |
| Telop/default/mainichi/songhash payload bytes | Proto confirms route families, but byte payload interpretation remains client-defined. | Binary/client proof or adjacent-era parser proof validated against MOMOIRO data. |
| AdminApi editing beyond readback | WebUI edits can corrupt era state if limits and packing are wrong. | Implement only after handler/persistence behavior and binary-backed limits are stable. |

### Anti-Features (Commonly Requested, Often Problematic)

Features that seem useful but create incorrect scope or unsafe behavior.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Treat MOMOIRO as KIMIDORI or Murasaki with renamed routes | Adjacent older AC15 eras look similar. | User explicitly requires MOMOIRO-owned support and evidence gates; later-era limits can be wrong. | Compose shared AC15 capabilities only after MOMOIRO proto and binary route evidence match. |
| Standalone `crownsdata.php` | Newer AC15 eras may have standalone crown surfaces. | MOMOIRO proto places crowns in `UserDataResponse.hash_crown_flg`; adding a route invents a contract. | Implement crowns inside userdata only. |
| Event folders or `getfolder.php` | Later eras expose folder routes and data. | MOMOIRO proto has no folder request/response family in the local input. | Keep folders absent unless a future MOMOIRO proto plus binary route evidence proves them. |
| Taikojuku practice-folder route | Adjacent eras and public wording can blur Dani and Taikojuku. | MOMOIRO proto has Dan fields but no `TaikojukuRequest/Response`; Dani Dojo and Taikojuku are separate surfaces. | Support Dan state only where proven; keep Taikojuku absent. |
| Stateful Don Challenge or `challengecompe.php` | Challenge arrays are present in userdata/playresult. | Arrays alone do not prove challenge route/readback semantics or reward behavior. | Accept/preserve bounded array state only after binary proof; no standalone ChallengeCompe route without proto plus binary route evidence. |
| Full Donder Hiroba/shop/gasha economy | Wiki mentions shop/customization and title-part gasha changes. | TaikoLocalServer is not the live web service and proto only proves cabinet compatibility fields. | Support cabinet-facing Don Point totals, shopping result compatibility, and unlock flags within MOMOIRO save state. |
| Banacoin, Tokkun, battle, tournament, gacha, WaiWai, or newer item-shop families | These exist in later AC15 eras or adjacent project history. | MOMOIRO 0.11 proto does not expose these feature families. | Treat them as absent unless future local MOMOIRO proto and binary route evidence prove otherwise. |
| Version behavior beyond MOMOIRO 0.11 | Public update history includes later MOMOIRO versions and changes. | The milestone target is 0.11. Later update behavior can contradict 0.11. | Keep 0.11 support narrow; start a separate milestone for later MOMOIRO versions. |
| Hardcoded `config/STxxxx-*` catalog root | Most newer AC15 eras use versioned config roots. | MOMOIRO data inventory is root-level. | Resolve through era data helpers and a MOMOIRO root-level catalog loader. |
| Persist generated wire DTOs directly | It is faster to wire controllers straight to EF. | It breaks the repo boundary and makes later wire corrections expensive. | Map wire DTOs through Application/Common AC15 DTOs and MOMOIRO-owned handlers. |

## Feature Dependencies

```text
MOMOIRO evidence gate
  -> generated wire and era foundation
  -> /v01r00 startup/version and /v04r00 game route skeleton
  -> root-level catalog loader
  -> BAID/mydon/profile defaults
  -> userdata and normal playresult
  -> self-best, crowns-in-userdata, release-song hash, favorites/recent
  -> Don Point/shopping, Dan state, challenge-compatible arrays
  -> AdminApi/WebUI readback

Binary route proof
  -> supported route list
  -> controller/handler implementation

Binary limit proof
  -> crown packing
  -> release-song packing
  -> favorites/recent truncation
  -> Don Point/shopping caps
  -> challenge/Dan semantics
```

### Dependency Notes

- **Catalog before userdata/playresult:** Unlock, crown, self-best, recommendation, default, mainichi, and songhash bytes all depend on song catalog ordering.
- **Binary route proof before route claims:** User-supplied `/v04r00` is the prefix contract, but each route still needs direct binary/client evidence before behavior is considered supported.
- **Binary limit proof before WebUI editing:** AdminApi/WebUI can read implemented state early, but editing byte-packed release/crown/favorite/recent/challenge fields should wait until limits are proven.
- **Dani before Taikojuku decisions:** Dan fields exist; Taikojuku messages do not. Implementing Dan state does not imply a practice-folder route.
- **Challenge arrays before challenge products:** Challenge arrays may be ordinary payload fields. Do not promote them into Don Challenge, ChallengeCompe, or reward behavior without route/proto proof.

## MVP Definition

### Launch With (v1)

Minimum viable MOMOIRO 0.11 support.

- [ ] First-class `GameEra.Momoiro`, settings, generated wire DTOs, adapter registration, and disabled-era gating.
- [ ] Shared `/v01r00/chassis/*.php` startup/version routing and `/v04r00/chassis/*.php` game route skeleton with direct-protobuf transport where verified.
- [ ] Root-level MOMOIRO catalog loader for `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- [ ] BAID/mydon/profile creation and readback through MOMOIRO-owned save state.
- [ ] Userdata and normal playresult for scores, self-best, crowns-in-userdata, recent/favorite, release-song flags, Don Point/reward, and Dan fields after binary limit proof.
- [ ] `songhash`, `defaultsong`, `mainichisong`, `telopcheck`, `gettelop`, and `recommend` compatibility where route proof exists.
- [ ] `shoppingresult` compatibility for Don Point totals and unlock flags after binary proof.
- [ ] Bounded challenge-array acceptance/readback only where route and payload behavior are proven.
- [ ] AdminApi/WebUI readback for implemented MOMOIRO-owned state only.

### Add After Validation (v1.x)

Features to add only after the core cabinet flow is verified.

- [ ] AdminApi/WebUI editing for byte-packed unlock/crown/favorite/recent/challenge data after limit proof.
- [ ] Richer catalog sidecars for telops, recommendations, and metadata if MOMOIRO-specific data is added.
- [ ] More complete Dan readback/editing after MOMOIRO `got_dan_flg` and `dan_result` semantics are proven.
- [ ] Challenge-array visualization if client evidence proves meaningful user-visible state.

### Future Consideration (v2+)

Out of current milestone unless new evidence starts a separate scope.

- [ ] Later MOMOIRO update behavior beyond 0.11.
- [ ] Standalone challenge route behavior.
- [ ] Taikojuku practice-folder behavior.
- [ ] Live-service shop/gasha/Donder Hiroba behavior.
- [ ] Any newer AC15 feature family missing from local MOMOIRO proto.

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Era foundation and generated wire | HIGH | MEDIUM | P1 |
| Shared startup/version routing | HIGH | MEDIUM | P1 |
| `/v04r00` game route skeleton and proof | HIGH | HIGH | P1 |
| Root-level catalog loader | HIGH | HIGH | P1 |
| BAID/mydon/profile | HIGH | MEDIUM | P1 |
| Userdata/readback core | HIGH | HIGH | P1 |
| Normal playresult persistence | HIGH | HIGH | P1 |
| Self-best | HIGH | MEDIUM | P1 |
| Crowns in userdata | HIGH | HIGH | P1 |
| Song unlocking/release hash | HIGH | HIGH | P1 |
| Favorites/recent | HIGH | HIGH | P1 |
| Don Point/reward/shopping result | MEDIUM | HIGH | P1 |
| Default/mainichi/songhash metadata | MEDIUM | MEDIUM | P1 |
| Telops/recommendations | MEDIUM | MEDIUM | P2 |
| Challenge arrays | MEDIUM | HIGH | P2 |
| Dani Dojo state | MEDIUM | HIGH | P2 |
| AdminApi/WebUI readback | HIGH | MEDIUM | P1 |
| AdminApi/WebUI editing for packed state | MEDIUM | HIGH | P2 |
| Taikojuku/folders/newer modes | LOW | HIGH | P3 or anti-feature unless proven |

**Priority key:**
- P1: Must have for launch or must be explicitly proven absent before launch.
- P2: Should have when binary/client evidence proves behavior and core flow is stable.
- P3: Future only; do not build in the v1.7 core path.

## Adjacent Era Feature Analysis

| Feature | KIMIDORI/Murasaki/White Pattern | MOMOIRO Approach |
|---------|----------------------------------|------------------|
| Era foundation | First-class era with owned wire/routes/persistence. | Same pattern, but MOMOIRO-owned from `proto/momoiro` and `/v04r00`. |
| Catalog layout | KIMIDORI uses root-level data; newer eras often use `config/STxxxx-*`. | Follow root-level layout. Do not import newer config-root assumptions. |
| Crowns | Later eras may have different crown routes/packing. | Use `UserDataResponse.hash_crown_flg` only; packing is a binary gate. |
| Favorites | Murasaki and later changed favorite capacity; KIMIDORI/MOMOIRO are older. | Do not assume the later limit. Prove MOMOIRO limit in binary/client behavior. |
| Folders | Murasaki/White route families may expose folder behavior. | MOMOIRO proto has no folder family; keep absent. |
| Dani/Taikojuku | Adjacent eras can support Dan state and sometimes Taikojuku route families. | Dan fields are present; Taikojuku request/response is absent. Keep them separate. |
| Don Point/shop | Public MOMOIRO context mentions point/shop changes. | Implement only cabinet-facing proto fields and MOMOIRO-owned totals/unlocks after cap proof. |
| Challenge | Older eras may expose challenge arrays or routes differently. | Arrays are protocol facts only; no standalone challenge product without route/proto proof. |

## Research Gaps for Roadmap

| Topic | Required Research | Phase Flag |
|-------|-------------------|------------|
| Route inventory | Extract `/v04r00/chassis/*.php` and shared `/v01r00/chassis/*.php` route proof from `.tools/momoiro/EBOOT.ELF.i64` or runtime traces. | Foundation phase must not skip. |
| Song unlocks | Prove `song_hash_ver`, `release_song_no`, and `hash_release_song_flg` behavior. | Runtime state phase needs deeper binary research. |
| Crown bytes | Prove `hash_crown_flg` layout, song count, level packing, ura/shin handling, and compression. | Runtime state phase needs deeper binary research. |
| Favorites/recent limits | Prove counts and ordering. | Runtime state phase needs deeper binary research. |
| Don Point/shopping | Prove 0.11 cap, totals, `shoppingresult.php` route use, and unlock side effects. | Reward/shop phase needs deeper binary research. |
| Challenge arrays | Prove whether arrays need persistence, echo, or safe omission. | Optional/compatibility phase should be evidence-gated. |
| Dan state | Prove play mode classification and Dan readback semantics. | P2 unless cabinet flow requires it earlier. |
| Telop/default/mainichi/songhash | Prove byte payload expectations against MOMOIRO data. | Metadata phase should include focused binary/client checks. |
| AdminApi/WebUI | Decide read-only vs editable packed fields after limits are known. | WebUI phase should trail runtime proof. |

## Sources

- `.planning/PROJECT.md` - current v1.7 MOMOIRO milestone contract and repo constraints. Confidence: HIGH.
- `proto/momoiro/taiko.proto` - local MOMOIRO game protocol message inventory. Confidence: HIGH.
- `proto/momoiro/vsinterface.proto` - local shared startup/version message inventory. Confidence: HIGH.
- `Host/wwwroot/data/momoiro/data` - root-level MOMOIRO game-data inventory. Confidence: HIGH.
- `.tools/momoiro/EBOOT.ELF.i64` - local binary research input exists; route/semantic analysis not performed in this feature research. Confidence for existence only: MEDIUM.
- `H:/TaikoLocalServer/.codex/gsd-core/templates/research-project/FEATURES.md` - output template. Confidence: HIGH.
- https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/AC%E3%81%AE%E6%AD%B4%E5%8F%B2/AC15#momoiro - secondary public product/version context. Confidence: LOW.
- https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%82%A2%E3%83%83%E3%83%97%E3%83%87%E3%83%BC%E3%83%88%E5%B1%A5%E6%AD%B4/%E3%83%A2%E3%83%A2%E3%82%A4%E3%83%AD - secondary public update context. Confidence: LOW.

---
*Feature research for: v1.7 MOMOIRO AC15 0.11 Support*
*Researched: 2026-06-25*
