# Green real support - design spec

**Date:** 2026-05-12
**Status:** design approved, written spec pending user review
**Scope:** phase 2 support for AC15 Green after the stub-first Green adapter/schema/catalog phase

## Context

Phase 1, documented in `docs/superpowers/specs/2026-05-12-green-version-support-design.md`, added the structural Green chassis:

- `GameEra.Green`
- `Adapters.GameProtocol.Green`
- generated Green protobuf wire types
- Green route stubs under `/v11r01/chassis/*.php`
- per-era EF tables such as `UserSaveData_Green`, `SongBestDatum_Green`, and `SongPlayDatum_Green`
- per-era catalog interfaces and startup era gating

This phase turns that scaffold into practical Green support for cabinet testing. The goal is to play songs, persist important uploaded state, read back scores/crowns/profile data, parse the copied Green datatables, and keep ambiguous Green-only fields observable without over-modeling them.

Public gameplay references, especially the AC15 Green page on wikiwiki (`https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/AC%E3%81%AE%E6%AD%B4%E5%8F%B2/AC15#green`), may be used to interpret Green feature names. Binary byte-field behavior comes from `proto/green/ida-byte-field-findings.md`.

## Goals

- Parse Green `musicinfo.xml` and use it as the source of truth for Green song IDs.
- Prove the Green song bitset namespace by unlocking 10 no-card songs and 20 logged-in songs from `musicinfo.xml` file order.
- Replace hard-coded Green controller responses with mapper and application handler flows.
- Create Green users through shared `Card` / `Credential` / `UserDatum` plus `UserSaveData_Green`.
- Persist normal play uploads, best scores, crowns, costumes, tones, titles, favorites, recent songs, options, medals, and ghost readback state.
- Return real data from core readback endpoints: `userdata`, `baidcheck`, `selfbest`, `crownsdata`, `getghostdata`, and `getghostscore`.
- Provide fake but consistent Taikojuku/Dan and score/crown seed data for test verification.
- Log unclear upload concepts during testing and defer schema changes until their meaning is clear.

## Non-goals

- Final Green Dan/Taikojuku grading support.
- Final challenge-competition, tournament/gacha, Waiwai collabo, and reward-title filtering semantics.
- Admin API and TaikoWebUI Green views.
- Lossless archival of every raw protobuf upload. Test-phase data loss is acceptable for unclear one-way fields.
- Reworking Nijiiro behavior except where shared helpers are needed.

## Approach

Use the phase-1 Green architecture and implement a pragmatic middle path:

- Typed support for the core song-play lifecycle.
- Catalog-backed responses when datatable meaning is clear.
- Deterministic fake data where that helps verify protocol assumptions.
- Logging for unclear fields instead of speculative database expansion.

## Green Song Catalog

`Host/wwwroot/data/green/datatable/musicinfo.xml` is the required song catalog for this phase. The loader parses Boost-serialization XML and keeps at least:

- `musicid`
- `uniqueid`
- `newrelease`
- `secret`
- `papamama`
- `hasextreme`
- `partsset`
- `wai2partsset`
- `musicname`
- `genrename`
- `demoplay`
- ordered `tag` values

`uniqueid` is treated as the Green protocol `song_no` and bit index. `musicinfo.xml` header `<version>` becomes `song_hash_ver` where Green responses expose it.

The first implementation only needs song-list behavior and basic metadata. Garbled local title text is tolerated because cabinet-facing IDs and categories are the immediate need.

### Unlock Test

The initial unlock policy is hard-coded for validation:

- `InitialDataCheck.hash_default_song_flg`: raw 128-byte bitset with the first 10 `uniqueid`s from `musicinfo.xml` file order enabled.
- `UserData.hash_release_song_flg`: raw 128-byte bitset with the first 20 `uniqueid`s from `musicinfo.xml` file order enabled.
- `InitialDataCheck.hash_mainichidojo_all`: raw 128-byte zero bitset.
- `InitialDataCheck.hash_mainichidojo_rare`: raw 128-byte zero bitset.

This proves XML parsing, `uniqueid` as the song namespace, and the 128-byte little-bit-endian bitset format.

## Card And Save-State Flow

Green uses the existing shared identity model:

- `Card`
- `Credential`
- slim `UserDatum`
- `UserSaveData_Green`

`baidcheck` sends `BaidQuery(GameEra.Green, accessCode)`. Unknown cards return a new-user response with the next proposed `baid`, following the Nijiiro pattern. `mydonentry` creates the shared identity rows plus default Green save data.

Known-card `baidcheck` returns stored Green profile state:

- `baid`
- access code
- player name
- title and title plate
- colors and current costume slots
- costume flags
- medals
- tutorial flags
- auto-costume flag
- Dan flags
- default tone
- content info
- timestamps

`userdata` returns:

- 20-song logged-in release bitset
- favorites and recent songs
- tone and title flags
- option state
- default option setting
- category counters
- recommendation fields
- difficulty/course fields
- challenge, close/tojiru, devil, and other Green toggles already present in the wire type

## Byte Field Contracts

Response bytes follow `proto/green/ida-byte-field-findings.md`.

Raw fixed-width fields:

- `InitialDataCheck.hash_default_song_flg`: 128 bytes
- `InitialDataCheck.hash_mainichidojo_all`: 128 bytes
- `InitialDataCheck.hash_mainichidojo_rare`: 128 bytes
- `UserData.hash_release_song_flg`: 128 bytes
- `UserData.tone_flg`: 16 bytes
- `UserData.title_flg`: 128 bytes
- `BAID.costume_flg_1..5`: 32 bytes each
- `BAID.got_dan_flg`: 18 bytes
- `BAID.got_danextra_flg`: 36 bytes
- `BAID.content_info`: 32 bytes
- `MydonEntry.content_info`: 32 bytes
- `Getghostdata.release_info_flag`: 16 bytes
- `Getghostdata.played_song_flag`: 128 bytes

Only `CrownsData.hash_crown_flg` is compressed. It is zlib-compressed, not gzip, and inflates to a 1280-byte body containing 1024 little-bit-endian 10-bit crown values.

## Options

Green `option_flg` is the same gameplay concept as Nijiiro options: player-selected modifiers and default play options. The exact Green byte width may differ.

Storage policy:

- `UserSaveDataGreen.OptionFlg` stores the raw Green user option block.
- `UserSaveDataGreen.DefaultOptionSetting` stores the raw default option block.
- The first 2 bytes of `DefaultOptionSetting` are treated as the known small option-setting value family used by Nijiiro.
- `SongPlayDatumGreen.OptionFlg` stores per-stage `PlayResultDataRequest.StageData.option_flg`.
- If Green sends larger blocks than Nijiiro, preserve the full bytes and decode only the known prefix.

During testing, logs include byte length and a short hex preview for option blocks.

## Play Result Flow

`PlayResultRequest.playresult_data` is deserialized directly as `PlayResultDataRequest` unless cabinet testing proves a wrapper. Green is not treated like Nijiiro's gzip-framed play result because the Green binary findings do not show endpoint decompression.

For each uploaded `StageData`:

- Insert `SongPlayDatumGreen`.
- Upsert `SongBestDatumGreen` when the score improves or the crown improves.
- Store song, difficulty, score, good/ok/miss/pound/combo/hit counts, crown, option bytes, tone bytes, folder/category, favorite/recent flags, star/support level, soul gauge, Waiwai fields, play dan, and play time.
- Map Green `level` to the existing `Difficulty` enum by course order: `0 = Easy`, `1 = Normal`, `2 = Hard`, `3 = Oni`, `4 = UraOni`.
- Map `play_result` to `CrownType` using the existing Nijiiro convention when the value is recognized. Unknown values log a warning and persist `CrownType.None`.
- Store `ScoreRate = 0` for Green in this phase because Green `StageData` does not expose an explicit score-rate field.
- Store `GhostStageSectionDatumGreen` when ghost section data is present.
- Update `GreenFavoriteSongs` and `GreenRecentSongs` from stage flags.

Top-level play result updates:

- Add released songs, tones, costume parts, and titles.
- Update current/play costumes, title, medals, area counters, difficulty fields, tutorial flags, `is_devil`, `is_explain`, and option bytes.
- Update ghost summary, token, and winnings fields when those upload objects are present.

Unclear one-way fields such as collabo and age-estimation details are logged and not schema-expanded in this phase.

## Score And Crown Semantics

Green score readback uses `SongBestDatumGreen`.

`selfbest` returns best scores for requested songs and the requested level. `ary_shin_selfbest_score` remains empty unless a clear separate mapping is identified.

`crownsdata` packs one 10-bit value per song:

```text
song_value =
  (state_easy      & 3) << 0 |
  (state_normal    & 3) << 2 |
  (state_hard      & 3) << 4 |
  (state_oni       & 3) << 6 |
  (state_ura_oni   & 3) << 8
```

State values:

- `0`: none
- `1`: clear
- `2`: full combo
- `3`: dondaful/all-good, or the best available Green all-good state

The 5-course, 2-bit-per-course layout is binary-supported; the label for value `3` remains partly inferred and must stay documented that way.

## Fake Seed Data

New Green users get deterministic fake best scores and crowns from the parsed `musicinfo.xml` list so cabinet behavior can verify score and crown assumptions before relying on real uploads.

Seed examples:

- first song: Easy clear with a score
- second song: Normal full combo
- third song: Hard dondaful/all-good state
- fourth song: Oni clear
- fifth song: Ura only if `hasextreme = 1`

Real uploads can overwrite these records when better.

## Ghost Support

Green ghost is treated as an older AI-battle-like subsystem. Persist fields that the Green cabinet can request back:

- release-info flags
- played-song flags
- input median and variance
- rank ID
- win points
- certified level ID
- total winnings
- per-level winnings
- ghost tokens
- per-stage section stats

`getghostdata` returns saved summary, flags, winnings, and tokens. `getghostscore` returns saved section stats for the requested song and level. Fields that do not round-trip through a Green readback endpoint are logged first and skipped unless later testing proves they matter.

## Taikojuku And Fake Dan

Taikojuku is enabled enough for cabinet flow testing, but not final graded Dan support.

Behavior:

- `InitialDataCheck.is_danplay = true`.
- `InitialDataCheck.ary_taikojuku_data` returns a small set of available pack IDs.
- `taikojuku.php` returns `JukupackData`.
- Prefer parsing `musicmedleyinfo.xml`; map each medley to pack data using `uniqueid` or `challengelv` as `get_dan`, and its `Content` entries as `(song_no, level)`.
- The implementation first tries `uniqueid` as `get_dan`. If cabinet requests do not match those IDs, it uses `challengelv`. If neither matches observed requests, it returns two or three deterministic fake packs from the first parsed songs in `musicinfo.xml`.

Dan flags:

- `got_dan_flg` uses fixed 18-byte 2-bit packed values.
- `got_danextra_flg` uses fixed 36-byte 2-bit packed values.
- On registration, the user starts with no Dan flags.
- On the next known-card `baidcheck` after registration, grant first normal Dan in `got_dan_flg`.
- Persist that in `UserSaveDataGreen.GotDanFlg`.

Green Dan score tables are not added in this phase. If `playresult` includes `play_dan` and `dan_result`, log them and only update packed flags when the mapping is obvious.

## Datatable Scope

Required Green datatables:

- `musicinfo.xml`
- `musicmedleyinfo.xml`

Useful if straightforward:

- `present.xml`
- `supportinfo.xml`

Deferred unless a response needs them:

- `rewardtitlefiltering.xml`
- `spacialbaid.xml`
- `waiwaiconfig.xml`
- `datatable/ghost/*.xml`
- `datatable/waiwaicollabo/*.xml`

When Green is enabled, missing required datatables fail startup with a clear path in the error. Deferred files log warnings at most.

## Other Endpoints

`itempurchase`:

- Read item data if an item-shop catalog is available.
- Subtract/use don medals when possible.
- Otherwise return a success-shaped no-op and log the request.

`rewardexecution`:

- Persist released songs, tones, costumes, and titles from request arrays.

`rewardcardcheck`:

- Resolve known card and return `baid` when possible.

`getitemshopinfo`, `getfolder`, `gettelop`, `tournamentcheck`, `challengecompe`, and `recommend`:

- Return catalog-backed data when available.
- Otherwise return deterministic empty success with request logging.

`bookkeeping`, `headclerk2`, `heartbeat`, startup auth, verup auth, and verup complete remain lightweight success responses.

## Error Handling And Logging

Invalid or missing required datatables fail startup when Green is enabled.

Malformed request payloads return a non-success result only when the wire type has a known failure result. Otherwise they log the parse problem and return the safest success-shaped response for cabinet testing.

Unknown upload fields are logged with:

- endpoint
- `baid`
- field name
- value if scalar
- byte length and short hex preview if bytes

Logs must not include full unbounded byte arrays.

## Testing

Unit tests:

- Green `musicinfo.xml` parser reads header version and file-order `uniqueid`s.
- Green `musicmedleyinfo.xml` parser creates deterministic pack data or fails into the fake-pack fallback.
- 128-byte little-bit-endian song bitsets.
- 2-bit Dan flag packing.
- 10-bit crown packing.
- zlib crown compression and inflate size.
- Green option bytes preserve full raw data.

Handler tests:

- New Green registration creates shared identity and `UserSaveDataGreen`.
- First known-card login after registration grants first fake Dan.
- `InitialDataCheck` unlocks 10 songs.
- `UserData` unlocks 20 songs.
- New user fake score/crown seed appears in `selfbest` and `crownsdata`.
- Play result saves `SongPlayDatumGreen` and upserts `SongBestDatumGreen`.
- Reward execution persists unlock arrays.
- Option changes round-trip through play result and user data.
- Ghost update data round-trips through `getghostdata` and `getghostscore`.

Cabinet smoke checklist:

- No-card song list shows 10 songs.
- Logged-in user song list shows 20 songs.
- Fake scores/crowns are visible in self-best/crown UI.
- A played score survives `selfbest`.
- Crown state survives `crownsdata`.
- Option changes survive next login.
- Fake Dan appears on second login.
- Ghost/AI-battle-like data does not crash the flow.

## Implementation Notes

Keep the existing Green project and DB schema unless implementation proves a missing persisted field is required for a round trip. Prefer additive changes.

Reusable helpers should live in Application or Infrastructure only when shared outside the Green adapter:

- fixed-width bitset creation
- 2-bit packing
- 10-bit crown packing
- zlib compression
- Green XML parsing

Green controllers should become thin:

1. log request summary
2. map wire request to common command/query
3. send Mediator request
4. map common response to Green wire response

Do not add admin/WebUI scope in this phase.
