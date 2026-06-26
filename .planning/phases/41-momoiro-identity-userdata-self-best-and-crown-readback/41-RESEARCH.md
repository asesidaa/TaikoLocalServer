# Phase 41: MOMOIRO Identity, Userdata, Self-Best, and Crown Readback - Research

**Researched:** 2026-06-26  
**Domain:** ASP.NET Core 10 / EF Core / AC15 MOMOIRO protocol readback  
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

MOMOIRO users can register, log in, enter MyDon/profile flow, and read back MOMOIRO-owned profile, score, release, hash, favorite/recent, and crown state without sharing era-owned state with any other era. [VERIFIED: 41-CONTEXT.md]

- MORDB-01: MOMOIRO card registration, login, mydon entry, and userdata readback use MOMOIRO-owned save state while sharing only true identity data across eras. [VERIFIED: 41-CONTEXT.md]
- MORDB-02: MOMOIRO self-best readback returns MOMOIRO-owned score state with era-correct normal, ura, and shin handling where proven. [VERIFIED: 41-CONTEXT.md]
- MORDB-03: MOMOIRO favorite and recent song readback uses binary/client-backed limits, ordering, truncation, and duplicate behavior. [VERIFIED: 41-CONTEXT.md]
- MORDB-04: MOMOIRO crown bytes in `userdata.php` use binary/client-backed packing, song count, difficulty placement, and default behavior. [VERIFIED: 41-CONTEXT.md]
- MORDB-05: MOMOIRO release-song and song-hash readback uses MOMOIRO catalog order and binary-backed `song_hash_ver`, `song_hash_tbl`, and `hash_release_song_flg` semantics. [VERIFIED: 41-CONTEXT.md]
- Phase 39 registered MOMOIRO as a first-class era under `/v04r00/chassis/*` with shared `/v01r00/chassis/*` startup/version routes. [VERIFIED: 41-CONTEXT.md]
- Phase 39 generated MOMOIRO wire from `proto/momoiro` and left proto inputs untouched. [VERIFIED: 41-CONTEXT.md]
- Phase 40 added the root MOMOIRO catalog from `Host/wwwroot/data/momoiro/data`, explicit AC15 profile limits, catalog-backed metadata routes, and static `heartbeat.php`/`bookkeeping.php`. [VERIFIED: 41-CONTEXT.md]
- Phase 40 verification passed focused serialized tests 27/27, full serialized suite 908/908, solution build, temp Host build, proto cleanliness, unsupported-route absence, and path-abstraction gates. [VERIFIED: 40-VERIFICATION.md]
- Native evidence names `baidcheck.php`, `mydonentry.php`, `userdata.php`, `recommend.php`, `selfbest.php`, and `heartbeat.php` in `.tools/momoiro/EBOOT.ELF.i64`. [VERIFIED: 41-CONTEXT.md]
- Generated MOMOIRO wire and IDA descriptor strings include `ary_selfbest_score`, `ary_shin_selfbest_score`, `self_best_score`, `ura_best_score`, `ary_favorite_song_no`, `ary_recent_song_no`, `song_favorite_cnt`, `song_recent_cnt`, `song_hash_ver`, `hash_release_song_flg`, and `hash_crown_flg`. [VERIFIED: 41-CONTEXT.md]
- `UserDataResponse.hash_crown_flg` is field 8 in current generated MOMOIRO wire; no standalone `crownsdata.php` route is in the binary route list. [VERIFIED: 41-CONTEXT.md + generated wire]
- Phase 40 established MOMOIRO song count `380`, song hash version `538116869`, 760-byte song hash table, 48-byte compact release/default flags, and an inferred 475-byte compact crown envelope for 380 songs times 10 bits. [VERIFIED: 41-CONTEXT.md + 40-VERIFICATION.md]

### the agent's Discretion

- Use existing profile values unless new binary/client evidence proves different favorite/recent values during Phase 41 research. [VERIFIED: 41-CONTEXT.md]
- Reuse shared AC15 helpers only after tests prove MOMOIRO crown byte length, song ordering, and difficulty placement. [VERIFIED: 41-CONTEXT.md]
- Follow current MOMOIRO generated wire for normal/ura/shin self-best handling. [VERIFIED: 41-CONTEXT.md]
- Add MOMOIRO-owned tables or columns without touching adjacent era state. [VERIFIED: 41-CONTEXT.md]

### Deferred Ideas (OUT OF SCOPE)

- `playresult.php` mutation, score writes, crown writes, favorite/recent writes, Don Point/reward mutation, Dan mutation, and challenge-shaped mutation belong to Phase 42. [VERIFIED: 41-CONTEXT.md]
- AdminApi/WebUI exposure belongs to Phase 43. [VERIFIED: 41-CONTEXT.md]
- Cabinet/RPCS3 acceptance belongs to Phase 44 unless the user provides evidence early. [VERIFIED: 41-CONTEXT.md]
- Proto edits, unsupported route families, Taikojuku/Tokkun/Banacoin/battle/gacha/tournament/Don Challenge/ChallengeCompe/event-folder/newer shop authority are out of scope. [VERIFIED: 41-CONTEXT.md]
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MORDB-01 | MOMOIRO card registration, login, mydon entry, and userdata readback use MOMOIRO-owned save state while sharing only true identity data across eras. | Use shared `Cards`, `UserData`, and `Credentials`; add only `UserSaveDataMomoiro` as the era save table for identity/profile readback. [VERIFIED: AddMyDonEntryCommand.Kimidori/Murasaki + 41-PATTERNS.md] |
| MORDB-02 | MOMOIRO self-best readback returns MOMOIRO-owned score state with normal, ura, and shin handling where proven. | Add `SongBestDatumMomoiro` and feed shared `Ac15SelfBestService`; map `ArySelfbestScores` and `AryShinSelfbestScores` to Momoiro wire. [VERIFIED: generated wire + GetSelfBestQuery.Kimidori/Murasaki] |
| MORDB-03 | MOMOIRO favorite/recent readback uses backed limits, ordering, truncation, and duplicate behavior. | Add `MomoiroFavoriteSongs` and `MomoiroRecentSongs`; use Phase 40 limits of 5/5, recent descending `LastPlayed`, and unique `(Baid, SongNo)` duplicate prevention. [VERIFIED: 40-VERIFICATION.md + Ac15NormalPlayWriter] |
| MORDB-04 | MOMOIRO crown bytes in `userdata.php` use backed packing, song count, difficulty placement, and defaults. | Build 10-bit crown values from `SongBestDatumMomoiro`, then compact by MOMOIRO song-hash/file order to 475 bytes for `UserDataResponse.HashCrownFlg`. [VERIFIED: generated wire + Ac15ProtocolBytes + Ac15SongHashCodec] |
| MORDB-05 | MOMOIRO release-song and song-hash readback uses catalog order and backed `song_hash_ver`, `song_hash_tbl`, and `hash_release_song_flg` semantics. | Use `IMomoiroCatalog.SongHashVersion`, `SongHashTable`, and `Ac15SongHashCodec.CompactBitset` for release/default flag wire envelopes. [VERIFIED: IMomoiroCatalog + MomoiroEraGameDataCatalog + 40-VERIFICATION.md] |
</phase_requirements>

## Project Constraints (from AGENTS.md)

- Keep era state separate; MOMOIRO persistence must not share Blue, Green, Yellow, Red, White, Murasaki, KIMIDORI, or Nijiiro era-owned state. [VERIFIED: AGENTS.md]
- Controllers deserialize, map, call Mediator, and map back; business behavior belongs in `Application/Handlers`. [VERIFIED: AGENTS.md]
- Use `IGameDataCatalog.For(GameEra)` and era catalog interfaces instead of hardcoded filesystem access from handlers. [VERIFIED: AGENTS.md]
- Resolve runtime data roots through `PathHelper` and era data path helpers. [VERIFIED: AGENTS.md]
- Mapperly mappers must remain source-generator driven; inspect emitted `.g.cs` files after `dotnet build /p:EmitCompilerGeneratedFiles=true`. [VERIFIED: AGENTS.md + CITED: mapperly.riok.app/docs/configuration/generated-source/]
- For Mapperly behavior, check current official Mapperly docs online, especially null-value behavior, constant/generated values, and generated-source inspection. [VERIFIED: AGENTS.md + CITED: mapperly.riok.app/docs/configuration/mapper/#null-values]
- Generated `Wire/` files stay out of manual cleanup unless regenerating protocol output. [VERIFIED: AGENTS.md]
- Tests should protect observable behavior, SQLite persistence, no-cross-era boundaries, parser/packing rules, and route responses. [VERIFIED: AGENTS.md]
- Do not add tests for generated protobuf property existence, controller attribute lists, project files, migrations, private methods, or implementation strings. [VERIFIED: AGENTS.md]
- This research turn must not modify source code beyond `41-RESEARCH.md` and must leave `Host/.gitignore` untouched. [VERIFIED: current user request]

## Summary

Phase 41 should add the smallest MOMOIRO-owned readback state needed by the four active runtime routes: `baidcheck.php`, `mydonentry.php`, `userdata.php`, and `selfbest.php`. [VERIFIED: 41-CONTEXT.md + generated wire] The required state is `UserSaveDataMomoiro`, `SongBestDatumMomoiro`, `MomoiroFavoriteSongs`, and `MomoiroRecentSongs`; shared identity remains limited to the existing `Cards`, `UserData`, and `Credentials` tables. [VERIFIED: AddMyDonEntryCommand.Kimidori/Murasaki + 41-PATTERNS.md]

The main implementation risk is crown packing. [VERIFIED: Ac15CrownService + Momoiro catalog] The existing shared `Ac15CrownService.BuildInflatedBody` indexes packed crown values by raw `SongId` and filters out song IDs greater than or equal to `limits.CrownSongCount`; MOMOIRO has 380 file-order songs but song IDs can exceed 380, so blindly using that helper with `Ac15EraProfiles.Momoiro.Limits` would drop high-ID songs. [VERIFIED: Ac15CrownService + MomoiroEraGameDataCatalog + 40-VERIFICATION.md] Phase 41 should reuse the lower-level AC15 crown-state and 10-bit packing rules, but build MOMOIRO crown values by song-hash/file-order ordinal before assigning `UserDataResponse.HashCrownFlg`. [VERIFIED: Ac15ProtocolBytes + Ac15SongHashCodec]

**Primary recommendation:** implement Phase 41 as KIMIDORI/Murasaki-shaped readback with MOMOIRO-owned EF tables, MOMOIRO dispatch arms, Momoiro-specific mappers, and an explicit Momoiro crown-by-hash-order builder; defer all playresult mutation, score writes, reward/Don Point writes, Dan/challenge mutation, and AdminApi/WebUI surfaces. [VERIFIED: 41-CONTEXT.md + repo grep]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Shared card/login identity | Database / Storage | Application | `Cards`, `UserData`, and `Credentials` are the existing shared identity boundary used by AC15 MyDon entry. [VERIFIED: Ac15MyDonEntryService] |
| MOMOIRO era save/profile state | Database / Storage | Application | Era profile/readback state must live in `UserSaveDataMomoiro`, not adjacent era save tables. [VERIFIED: AGENTS.md + 41-CONTEXT.md] |
| Self-best rows | Database / Storage | Application | `SongBestDatumMomoiro` owns score/crown readback rows; `Ac15SelfBestService` owns normal/ura/shin response composition. [VERIFIED: GetSelfBestQuery.Kimidori/Murasaki + Ac15SelfBestService] |
| Favorite and recent rows | Database / Storage | Application | MOMOIRO-owned favorite/recent tables provide `userdata.php` arrays and counts. [VERIFIED: UserDataQuery.Kimidori/Murasaki + 41-PATTERNS.md] |
| Release/song hash readback | Application | Infrastructure catalog | `IMomoiroCatalog` provides version/order/table; Application compacts flags before adapter mapping. [VERIFIED: IMomoiroCatalog + Ac15SongHashCodec] |
| Crown bytes in userdata | Application | Database / Storage | Application must transform best rows into 10-bit crown values by MOMOIRO catalog order; adapter only maps `HashCrownFlg`. [VERIFIED: generated wire + Ac15ProtocolBytes] |
| Wire response mapping | Adapter | Application | Mapperly mappers translate common DTOs to MOMOIRO wire; generated source must be inspected. [VERIFIED: AGENTS.md + Mapperly docs] |

## Direct Answers For Planning

### 1. MOMOIRO-Owned Persistence Needed Now vs Deferred

| Persistence Surface | Phase 41? | Table / Entity | Purpose | Notes |
|---------------------|-----------|----------------|---------|-------|
| Shared card row | Use existing | `Cards` | Access-code to BAID identity. | Shared identity only; not MOMOIRO-owned. [VERIFIED: Ac15MyDonEntryService] |
| Shared user identity | Use existing | `UserData` | MyDon name/language and shared BAID identity. | Shared identity only; do not store MOMOIRO profile fields here. [VERIFIED: Ac15MyDonEntryService] |
| Shared credential row | Use existing | `Credentials` | Existing AC15 MyDon service creates credential shell if missing. | Shared identity only. [VERIFIED: Ac15MyDonEntryService] |
| MOMOIRO save/profile state | Needed | `UserSaveDataMomoiro` -> table `UserSaveData_Momoiro` | BAID/profile colors/costumes/flags/release/options/counters/mode/reward readback defaults. | Copy Kimidori/Murasaki save shape unless intentionally trimming unsupported fields; use `Ac15EraProfiles.Momoiro` defaults. [VERIFIED: UserSaveDataKimidori/Murasaki + 41-PATTERNS.md] |
| MOMOIRO best/crown rows | Needed | `SongBestDatumMomoiro` -> table `SongBestDatum_Momoiro` | `selfbest.php` score readback and `userdata.php` crown bytes. | Writes are deferred, but readback needs rows that tests can seed. [VERIFIED: SongBestDatumKimidori/Murasaki + Ac15SelfBestService] |
| MOMOIRO favorites | Needed | `MomoiroFavoriteSongs` | `userdata.php` favorite array and count. | Key `(Baid, SongNo)` prevents duplicates like adjacent AC15 tables. [VERIFIED: KimidoriFavoriteSongs/MurasakiFavoriteSongs] |
| MOMOIRO recents | Needed | `MomoiroRecentSongs` | `userdata.php` recent array ordered by `LastPlayed`. | Key `(Baid, SongNo)` plus `LastPlayed` matches adjacent AC15 readback/writer shape. [VERIFIED: KimidoriRecentSongs/MurasakiRecentSongs + Ac15NormalPlayWriter] |
| MOMOIRO play history | Defer | `SongPlayDatumMomoiro` | Normal playresult history/mutation. | Not required for readback-only Phase 41; add in Phase 42 if mutation needs it. [VERIFIED: 41-CONTEXT.md] |
| MOMOIRO Dan tables | Defer / do not add now | `DanScoreDatumMomoiro`, `DanStageScoreDatumMomoiro` | Dan/challenge-compatible state if later proven. | Phase 41 has no Dani/Taikojuku scope; do not copy KIMIDORI Dan tables just because KIMIDORI has them. [VERIFIED: 41-CONTEXT.md + 41-PATTERNS.md] |
| Reward/Don Point mutation facts | Defer | none for Phase 41 | Future reward/Don Point mutation. | Readback fields can remain in save defaults; mutation authority is Phase 42. [VERIFIED: 41-CONTEXT.md + 40-VERIFICATION.md] |
| Tokkun/Banacoin/battle/gacha/tournament/event-folder/Don Challenge state | Defer / exclude | none | Unsupported route families. | No Phase 41 route/wire authority for these surfaces. [VERIFIED: 41-CONTEXT.md] |

EF planning checklist: add `ITaikoDbContext.Momoiro.cs`, `TaikoDbContext.Momoiro.cs`, an `OnModelCreatingMomoiro(modelBuilder)` call before `OnModelCreatingPartial`, and a migration that creates only the four Phase 41 MOMOIRO tables. [VERIFIED: TaikoDbContext.Kimidori/Murasaki + 41-PATTERNS.md]

### 2. Generated Wire Fields To Map

| Route | Request Fields | Response Fields To Map | Out-of-Scope / Do Not Invent |
|-------|----------------|------------------------|------------------------------|
| `baidcheck.php` | `access_code` from `BAIDRequest`. [VERIFIED: generated wire] | `Result`, `PlayerType`, `ComSvrResult`, `PersonalId`, `Baid`, `AccessCode`, `IsPublish`, `CardOwnNum`, `RegCountryId`, `PurposeId`, `RegionId`, `MydonName`, `Title`, colors, `AryCostumedata`, `AryFavoriteCostumedata`, `CostumeFlg1..5`, `RewardPtn`, `UpdateDatetime`, `DispDanType`, `GotDanMax`, `GotDanFlg`, `Accesstoken`, `ContentInfo`. [VERIFIED: generated wire] | No `MbId` mapping; current MOMOIRO `BAIDResponse` does not contain `mb_id`. [VERIFIED: generated wire grep] |
| `mydonentry.php` | `access_code`, `mydon_name`, required `reward_ptn`. [VERIFIED: generated wire] | `Result`, `ComSvrResult`, `PersonalId`, `Baid`, `AccessCode`, `IsPublish`, `CardOwnNum`, `RegCountryId`, `PurposeId`, `RegionId`, `MydonName`, `RewardPtn`, `Accesstoken`, `ContentInfo`. [VERIFIED: generated wire] | No `MbId` mapping; current MOMOIRO `MydonEntryResponse` does not contain `mb_id`. [VERIFIED: generated wire grep] |
| `userdata.php` | `baid`, `chassis_id`. [VERIFIED: generated wire] | `Result`, `IsExplain`, `AryFavoriteSongNoes`, `AryRecentSongNoes`, `SongHashVer`, `HashReleaseSongFlg`, `IsDevil`, `HashCrownFlg`, `DispScoreType`, `DispLevelTotal`, `DispLevelChassis`, `OptionFlg`, `ToneFlg`, `TitleFlg`, `RewardProgress`, category counters, `SongPushedCnt`, `SongFavoriteCnt`, `SongRecentCnt`, `TotalCreditCnt`, `PrevAreaCode`, `ConsecAreaCnt`, `RecommendSong`, `RecommendBestSongs`, `DispLevelSelf`, `IsAutoTitleOn`, `DefaultOptionSetting`, `DefaultShinSetting`, `TotalGetDonpoint`, `TotalUseDonpoint`. [VERIFIED: generated wire] | Challenge arrays and `FriendInfo` are generated fields but Phase 41 does not implement challenge/friend semantics. [VERIFIED: generated wire + 41-CONTEXT.md] Do not map absent KIMIDORI/Murasaki fields such as Taikojuku/Tokkun/tutorial fields into Momoiro. [VERIFIED: generated wire comparison] |
| `selfbest.php` | `baid`, `chassis_id`, `level`, `ary_song_no`. [VERIFIED: generated wire] | `Result`, `Level`, `ArySelfbestScores`, `AryShinSelfbestScores`; each `SelfBestData` maps `SongNo`, `SelfBestScore`, and `UraBestScore`. [VERIFIED: generated wire] | `BestRate` stays internal/common and is ignored because MOMOIRO wire `SelfBestData` does not expose rate fields. [VERIFIED: generated wire + Ac15SelfBestService] |

### 3. List, Release, Hash, and Crown Representation

| Concern | Representation | Required Tests |
|---------|----------------|----------------|
| Favorites | Store in `MomoiroFavoriteSongs` with unique key `(Baid, SongNo)` and cap response to `Ac15EraProfiles.Momoiro.Limits.MaxFavoriteSongs` (`5` from Phase 40). [VERIFIED: 40-VERIFICATION.md + KimidoriFavoriteSongs] Favorite native ordering is not proven; use deterministic `OrderBy(SongNo).Take(5)` for readback until a writer/order column is evidence-backed. [ASSUMED] | Assert max 5, duplicate prevention by key, MOMOIRO-only source, and deterministic returned values without asserting unsupported native ordering. [VERIFIED: AGENTS.md testing rules] |
| Recents | Store in `MomoiroRecentSongs` with unique key `(Baid, SongNo)`, order by `LastPlayed` descending, and cap to 5. [VERIFIED: UserDataQuery.Kimidori/Murasaki + Ac15NormalPlayWriter + 40-VERIFICATION.md] | Seed six recents and assert newest five in descending `LastPlayed`; seed adjacent-era recents and assert ignored. [VERIFIED: current adjacent AC15 tests pattern] |
| Duplicate behavior | Favorites and recents cannot duplicate a `(Baid, SongNo)` row because the EF key is `(Baid, SongNo)`. [VERIFIED: TaikoDbContext.Kimidori/Murasaki] Future playresult recents should upsert `LastPlayed` and trim old rows, matching `Ac15NormalPlayWriter`. [VERIFIED: Ac15NormalPlayWriter] | Phase 41 tests seed/update one row rather than relying on duplicate rows that SQLite keys disallow. [VERIFIED: EF model pattern] |
| Release flags | Persist internal `ReleaseSongFlg` in `UserSaveDataMomoiro` using the shared inflated `SongFlagBytes` envelope, then OR with catalog release/defaults in `Ac15UserDataService` and compact to 48 bytes through `Ac15SongHashCodec.CompactBitset(momoiro.SongHashTable)` for wire. [VERIFIED: UserSaveDataKimidori/Murasaki + Ac15UserDataService + Ac15SongHashCodec + 40-VERIFICATION.md] | Assert a seeded MOMOIRO release bit appears in `HashReleaseSongFlg` using MOMOIRO hash order; assert adjacent-era release flags are ignored. [VERIFIED: MORDB-05] |
| Song hash | `SongHashVer` is `IMomoiroCatalog.SongHashVersion` (`538116869`); `songhash.php` owns `song_hash_tbl` as 760 bytes and `userdata.php` returns only the version plus compact flags. [VERIFIED: IMomoiroCatalog + 40-VERIFICATION.md + generated wire] | Assert `userdata.php` returns version `538116869`; rely on Phase 40 metadata tests for `songhash.php` table bytes. [VERIFIED: 40-VERIFICATION.md] |
| Crown default | Empty `SongBestDatumMomoiro` rows produce 475 zero bytes in `UserDataResponse.HashCrownFlg`. [INFERRED: Ac15ProtocolBytes + 40-VERIFICATION.md] | Assert exact length 475 and all zero bytes for a fresh MOMOIRO save. [VERIFIED: generated wire field + Phase 40 limits] |
| Crown populated | For each song in MOMOIRO file/hash order, build a 10-bit value with 2 bits per difficulty in Easy, Normal, Hard, Oni, UraOni order, using `Ac15CrownService.MapCrownState` semantics (`Clear` -> 2, `Gold`/`Dondaful` -> 3, otherwise 0). [VERIFIED: Ac15CrownService + Ac15ProtocolBytes] Pack 380 values into 475 bytes. [INFERRED: 380 * 10 / 8 from Phase 40 limits] | Seed a high-ID MOMOIRO song from the catalog and assert its crown is not dropped; this catches accidental raw-`SongId` indexing by `Ac15CrownService.BuildInflatedBody`. [VERIFIED: Ac15CrownService + Momoiro catalog] |

### 4. Exact No-Cross-Era Boundaries And Tests

| Boundary | Required Test |
|----------|---------------|
| `mydonentry.php` creates/updates shared `Cards`, `UserData`, and `Credentials`, plus `UserSaveDataMomoiro` only. [VERIFIED: Ac15MyDonEntryService + 41-CONTEXT.md] | Create a new card through MOMOIRO MyDon; assert no Blue/Green/Yellow/Red/White/Murasaki/KIMIDORI/Nijiiro save row is created. [VERIFIED: 41-CONTEXT.md] |
| `baidcheck.php` must not treat an adjacent-era save as an existing MOMOIRO save. [VERIFIED: BaidQuery.Kimidori/Murasaki pattern] | Seed shared card/user plus KIMIDORI/Murasaki save only; assert MOMOIRO baidcheck reports new-user path until `UserSaveDataMomoiro` exists. [VERIFIED: MORDB-01] |
| `userdata.php` reads only `UserSaveDataMomoiro`, `MomoiroFavoriteSongs`, `MomoiroRecentSongs`, and `SongBestDatumMomoiro` for crown. [VERIFIED: 41-CONTEXT.md + UserDataQuery.Kimidori/Murasaki] | Seed adjacent favorites/recents/release/bests for same BAID; assert MOMOIRO response ignores them. [VERIFIED: MORDB-01] |
| `selfbest.php` reads only `SongBestDatumMomoiro`. [VERIFIED: GetSelfBestQuery.Kimidori/Murasaki] | Seed same song/difficulty in Momoiro and adjacent best tables; assert only MOMOIRO score appears. [VERIFIED: MORDB-02] |
| `userdata.php` crown reads only MOMOIRO best rows and stays on `hash_crown_flg`. [VERIFIED: generated wire + 41-CONTEXT.md] | Seed adjacent crown/best rows and assert MOMOIRO `HashCrownFlg` unchanged; assert route-surface guard still excludes `crownsdata.php`. [VERIFIED: MORDB-04] |
| Phase 41 routes do not write score/crown/favorite/recent/release mutation rows except save creation defaults. [VERIFIED: 41-CONTEXT.md] | After BAID/MyDon/UserData/SelfBest calls, assert no `SongPlayDatum*` or non-Momoiro best/favorite/recent rows were created. [VERIFIED: 41-CONTEXT.md] |
| `proto/momoiro` remains untouched. [VERIFIED: user request + AGENTS.md] | Run `git status --porcelain -- proto/momoiro` and require no output. [VERIFIED: 41-CONTEXT.md expected validation] |

### 5. Mapperly Generated-Source Inspections Required

Run:

```powershell
dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore
```

Mapperly emits generated source under the project `obj/.../generated/.../Riok.Mapperly/` path after build. [CITED: mapperly.riok.app/docs/configuration/generated-source/] Required inspections:

| Generated File | Required Inspection |
|----------------|---------------------|
| `BaidResponseMapper.g.cs` | Confirm `Result`, identity/profile fields, costume arrays, costume flags, reward pattern, update time, Dan display fields, access token, and content info are assigned only from MOMOIRO common DTO sections. [VERIFIED: generated wire + Mapperly docs] Confirm no `MbId` assignment exists because MOMOIRO wire lacks `mb_id`. [VERIFIED: generated wire grep] |
| `UserDataMappers.g.cs` | Confirm `SongHashVer`, `HashReleaseSongFlg`, `HashCrownFlg`, favorite/recent arrays, counters, display fields, recommendation fields, option/tone/title flags, default settings, and Don Point totals map intentionally. [VERIFIED: generated wire] Confirm absent KIMIDORI/Murasaki/Tokkun/Challenge fields are ignored rather than accidentally synthesized. [VERIFIED: generated wire comparison] |
| `SelfBestMappers.g.cs` | Confirm `Result`, `Level`, `ArySelfbestScores`, `AryShinSelfbestScores`, and row `SongNo`, `SelfBestScore`, `UraBestScore` map; confirm `BestRate` is ignored. [VERIFIED: generated wire + Ac15SelfBestService] |
| `MyDonEntryMappers.g.cs` if introduced | Confirm constants and common response fields match MOMOIRO wire and that Mapperly required-source strategy does not silently drop `reward_ptn` or content fields. [VERIFIED: generated wire + Mapperly docs] If controller maps manually like adjacent eras, no generated file is expected. [VERIFIED: repo grep] |

Mapperly `RequiredMappingStrategy` and ignore attributes should be used to make unsupported source/target fields explicit. [CITED: mapperly.riok.app/docs/configuration/mapper/] Null behavior should not be relied on silently for byte arrays; generated source must show whether null source arrays assign or leave target defaults. [CITED: mapperly.riok.app/docs/configuration/mapper/#null-values]

### 6. Low-Confidence Or Deferred Items

| Item | Status | Planner Handling |
|------|--------|------------------|
| Favorite native ordering | LOW confidence. [ASSUMED] | Use deterministic server ordering for Phase 41 tests, but do not claim native order until client/IDA evidence proves it. |
| Favorite native max | LOW confidence; Phase 40 uses conservative `5`. [ASSUMED] | Keep `5` unless new evidence appears; add a note in tests that it is a profile-bound cap. |
| Crown native byte constant | MEDIUM-LOW; 475 is inferred from 380 songs times 10 bits. [INFERRED: 40-VERIFICATION.md + Ac15ProtocolBytes] | Test length/difficulty placement now; cabinet acceptance remains Phase 44. |
| Exact `IsAutoTitleOn` semantics | LOW; field exists but no save/source semantics were proven. [VERIFIED: generated wire; ASSUMED semantics] | Leave default unless evidence backs a save field; do not map from `IsAutoCostumeOn`. |
| Challenge arrays and friend info in `UserDataResponse` | Deferred. [VERIFIED: generated wire + 41-CONTEXT.md] | Keep unset for Phase 41. |
| Reward/Don Point mutation and caps | Deferred to Phase 42. [VERIFIED: 41-CONTEXT.md + 40-VERIFICATION.md] | Read back existing/default save fields only. |
| Dan/challenge mutation | Deferred to Phase 42 only where proven. [VERIFIED: 41-CONTEXT.md] | Do not add Dan tables in Phase 41. |
| Playresult writes for score/crown/release/favorite/recent | Deferred to Phase 42. [VERIFIED: 41-CONTEXT.md] | Phase 41 tests seed rows directly and verify readback. |
| AdminApi/WebUI | Deferred to Phase 43. [VERIFIED: 41-CONTEXT.md] | No UI/API planning tasks in Phase 41. |
| Cabinet/RPCS3 acceptance | Deferred to Phase 44. [VERIFIED: 41-CONTEXT.md] | Automated tests are server-side compatibility guards only. |

## Standard Stack

No new external packages are required for Phase 41. [VERIFIED: repo grep + phase scope]

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK | 10.0.201 | Build and test the ASP.NET Core solution. | Current machine SDK. [VERIFIED: `dotnet --version`] |
| Entity Framework Core SQLite | 10.0.7 | Add MOMOIRO readback tables and migration. | Existing persistence layer uses EF Core and SQLite. [VERIFIED: Directory.Packages.props + TaikoDbContext] |
| Mediator | 3.0.2 | Route Application requests from controllers to handlers. | Existing handlers use Mediator. [VERIFIED: Directory.Packages.props + handlers] |
| protobuf-net | 3.2.56 / 3.2.52 | Direct protobuf game endpoint transport. | Generated wire and adapters use protobuf-net. [VERIFIED: Directory.Packages.props + generated wire] |
| Riok.Mapperly | 4.3.1 | Source-generated DTO-to-wire mapping. | Existing adapters use Mapperly and project rules require generated-source inspection. [VERIFIED: Directory.Packages.props + AGENTS.md] |
| xUnit / Microsoft.NET.Test.Sdk | 2.9.3 / 17.14.1 | Regression tests. | Existing `Tests/Tests.csproj` uses xUnit. [VERIFIED: Directory.Packages.props] |

### Supporting

| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| `dotnet ef` | 10.0.7 | Create EF migration. | Required for schema change. [VERIFIED: `dotnet ef --version`] |
| IDA idalib / ida-cli | available | Targeted native evidence checks. | Use only if Phase 41 implementation hits an unresolved native question. [VERIFIED: 41-CONTEXT.md] |
| Git | 2.52.0.windows.1 | Commit research/planning artifacts. | GSD commit flow. [VERIFIED: `git --version`] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Four-table Phase 41 MOMOIRO persistence | Copy all KIMIDORI runtime tables | Do not copy Dan/play rows; Phase 41 readback does not need them and no-cross-era scope is narrower. [VERIFIED: 41-CONTEXT.md + 41-PATTERNS.md] |
| Momoiro hash-order crown builder | `Ac15CrownService.BuildInflatedBody` directly | Direct helper drops high MOMOIRO song IDs because it indexes by raw song ID under `CrownSongCount=380`. [VERIFIED: Ac15CrownService + Momoiro catalog] |
| Handwritten mapping bodies | Mapperly-generated mappers | Project rules require Mapperly source-generator mapping for mapper classes, with handwritten helpers only where Mapperly discovers/uses them. [VERIFIED: AGENTS.md] |

**Installation:**

```bash
# No new package installation for Phase 41.
```

## Package Legitimacy Audit

No new external packages are installed in Phase 41, so the package legitimacy gate is not applicable. [VERIFIED: phase scope]

## Architecture Patterns

### System Architecture Diagram

```text
/v04r00/chassis request
  -> Momoiro controller deserializes direct protobuf
  -> Momoiro mapper maps wire request to common/Application DTO if needed
  -> Mediator dispatches GameEra.Momoiro request
  -> Application handler reads shared identity plus MOMOIRO-owned EF sets
  -> AC15 shared services compose userdata/selfbest/hash/list/crown shapes
  -> Momoiro mapper maps common response to generated Momoiro wire
  -> protobuf response
```

This flow keeps protocol mapping in the adapter, business/readback composition in Application, and persistence in Infrastructure/Domain. [VERIFIED: AGENTS.md + adjacent AC15 code]

### Recommended Project Structure

```text
Domain/Entities/
  UserSaveDataMomoiro.cs
  SongBestDatumMomoiro.cs
  MomoiroFavoriteSongs.cs
  MomoiroRecentSongs.cs
Application/Common/
  UserSaveDataMomoiroExtensions.cs
Application/Abstractions/
  ITaikoDbContext.Momoiro.cs
Application/Handlers/
  BaidQuery.Momoiro.cs
  AddMyDonEntryCommand.Momoiro.cs
  UserDataQuery.Momoiro.cs
  GetSelfBestQuery.Momoiro.cs
Application/Ac15/
  MomoiroAc15UserDataAdapter.cs
  MomoiroCrownResponseBuilder.cs
Infrastructure/Persistence/
  TaikoDbContext.Momoiro.cs
  Migrations/*_AddMomoiroReadbackState.cs
Adapters.GameProtocol.Momoiro/Mappers/
  BaidResponseMapper.cs
  UserDataMappers.cs
  SelfBestMappers.cs
Tests/Momoiro/
  MomoiroRuntimeReadbackTests.cs
  MomoiroControllerReadbackTests.cs
```

These files mirror adjacent AC15 pattern boundaries while omitting unsupported Dan/play tables. [VERIFIED: 41-PATTERNS.md + repo grep]

### Pattern 1: Shared Identity, Era-Owned Save

**What:** Reuse `Ac15MyDonEntryService` for shared `Cards`, `UserData`, and `Credentials`, but pass `context.UserSaveDataMomoiro` and `CreateDefaultMomoiroSaveData`. [VERIFIED: Ac15MyDonEntryService + 41-PATTERNS.md]

**When to use:** `mydonentry.php` and BAID/login flows. [VERIFIED: generated wire + adjacent controllers]

**Example:**

```csharp
// Source: AddMyDonEntryCommand.Kimidori.cs / Murasaki.cs analog.
return Ac15MyDonEntryService.HandleAsync(
    context,
    request.AccessCode,
    request.MydonName,
    request.MydonNameLanguage,
    context.UserSaveDataMomoiro,
    UserSaveDataMomoiroExtensions.CreateDefaultMomoiroSaveData,
    logger,
    "MOMOIRO",
    cancellationToken);
```

### Pattern 2: Self-Best From MOMOIRO Best Rows

**What:** Query `SongBestDataMomoiro` by BAID, requested song numbers, and `Ac15SelfBestService.GetRequestedDifficulties(request.Difficulty)`, then call `Ac15SelfBestService.BuildResponse`. [VERIFIED: GetSelfBestQuery.Kimidori/Murasaki + Ac15SelfBestService]

**When to use:** `selfbest.php`. [VERIFIED: generated wire]

**Example:**

```csharp
// Source: GetSelfBestQuery.Kimidori.cs / Murasaki.cs analog.
var difficulties = Ac15SelfBestService.GetRequestedDifficulties(request.Difficulty);
var bestRows = await context.SongBestDataMomoiro
    .Where(row => row.Baid == request.Baid)
    .Where(row => requestedSongs.Contains(row.SongId))
    .Where(row => difficulties.Contains(row.Difficulty))
    .Select(row => new Ac15BestRow(row.SongId, row.Difficulty, row.IsShin, row.BestScore, row.BestRate, row.BestCrown))
    .ToListAsync(cancellationToken);

return Ac15SelfBestService.BuildResponse(request.Difficulty, request.SongNoes, bestRows);
```

### Pattern 3: MOMOIRO Crown By Catalog Order

**What:** Build one 10-bit crown value per MOMOIRO file-order song, not per raw song ID. [VERIFIED: Ac15CrownService + Momoiro catalog]

**When to use:** `userdata.php` `HashCrownFlg`. [VERIFIED: generated wire]

**Example:**

```csharp
// Source: Ac15ProtocolBytes + Ac15SongHashCodec; raw BuildInflatedBody is not safe for Momoiro high IDs.
var fileOrderSongIds = momoiro.MusicInfoFileOrder;
var valuesBySongId = bestRows
    .GroupBy(row => row.SongId)
    .ToDictionary(group => group.Key, group => BuildTenBitCrownValue(group));

var orderedValues = fileOrderSongIds
    .Select(songId => valuesBySongId.GetValueOrDefault(songId))
    .ToArray();

var hashCrownFlg = Ac15ProtocolBytes.PackTenBitValues(
    orderedValues,
    Ac15EraProfiles.Momoiro.Limits.CrownPackedBytes,
    Ac15EraProfiles.Momoiro.Limits.CrownSongCount);
```

`BuildTenBitCrownValue` should reuse `Ac15CrownService.MapCrownState` and `Ac15ProtocolBytes.BuildCrownValue`; do not duplicate bit layout logic. [VERIFIED: Ac15CrownService + Ac15ProtocolBytes]

### Anti-Patterns to Avoid

- **Copying KIMIDORI Dan/play tables wholesale:** Phase 41 does not need `SongPlayDataMomoiro`, `DanScoreDataMomoiro`, or `DanStageScoreDataMomoiro`. [VERIFIED: 41-CONTEXT.md]
- **Using adjacent era save/best rows as fallback:** This violates no-cross-era boundaries. [VERIFIED: AGENTS.md + 41-CONTEXT.md]
- **Directly calling `Ac15CrownService.BuildInflatedBody` for Momoiro hash crown:** Raw song ID indexing drops high Momoiro song IDs under `CrownSongCount=380`. [VERIFIED: Ac15CrownService + Momoiro catalog]
- **Mapping generated challenge/friend fields because they exist:** Field presence is not behavior authority; Phase 41 explicitly defers challenge-shaped mutation. [VERIFIED: generated wire + 41-CONTEXT.md]
- **Editing `proto/momoiro`:** Phase 39 generated wire is current evidence and the user forbids source changes beyond research in this turn. [VERIFIED: 41-CONTEXT.md + current user request]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Shared identity creation | New MOMOIRO identity service | `Ac15MyDonEntryService` | Existing service already handles shared card/user/credential boundaries. [VERIFIED: Ac15MyDonEntryService] |
| Self-best normal/ura/shin composition | New per-route score algorithm | `Ac15SelfBestService` | Existing service preserves request order, zero rows, Oni/Ura handling, and shin arrays. [VERIFIED: Ac15SelfBestService] |
| Release/song hash compaction | Custom bit loops in controller | `Ac15SongHashCodec.CompactBitset` | Existing codec compacts by hash table order. [VERIFIED: Ac15SongHashCodec] |
| Crown bit layout | New 10-bit packing logic | `Ac15CrownService.MapCrownState` and `Ac15ProtocolBytes.BuildCrownValue`/`PackTenBitValues` | Existing code owns difficulty placement and two-bit crown states. [VERIFIED: Ac15CrownService + Ac15ProtocolBytes] |
| Wire mapping | Handwritten mapper bodies in Mapperly classes | Mapperly source-generated mapper plus helper conversions | Project rule requires Mapperly mapper bodies to remain generated. [VERIFIED: AGENTS.md] |
| EF schema state | Ad hoc SQLite SQL | EF Core entity mappings and migration | Existing persistence is EF Core with migrations. [VERIFIED: TaikoDbContext] |

**Key insight:** Phase 41 should reuse shared AC15 composition where it is ordinal-safe, but Momoiro crown packing must be adapted at the catalog-order boundary because Momoiro has 380 file-order songs and non-dense song IDs. [VERIFIED: 40-VERIFICATION.md + Ac15CrownService]

## Common Pitfalls

### Pitfall 1: Dropping High-ID Crown Rows

**What goes wrong:** High song IDs such as MOMOIRO medley/catalog IDs above 379 do not appear in `HashCrownFlg`. [VERIFIED: Momoiro catalog + Ac15CrownService]

**Why it happens:** `Ac15CrownService.BuildInflatedBody` filters `validSongNoes` to `songNo < limits.CrownSongCount` and writes `values[group.Key]`; this assumes raw song ID is a dense crown index. [VERIFIED: Ac15CrownService]

**How to avoid:** Build crown values by MOMOIRO file-order ordinal, then pack 380 ten-bit values. [VERIFIED: Ac15ProtocolBytes + IMomoiroCatalog]

**Warning signs:** A test seeded with a high song ID produces all-zero `HashCrownFlg`. [VERIFIED: testable behavior]

### Pitfall 2: Treating Adjacent Era Rows As MOMOIRO Fallback

**What goes wrong:** A card with KIMIDORI/Murasaki state looks registered in MOMOIRO or returns adjacent scores/lists. [VERIFIED: 41-CONTEXT.md risk]

**Why it happens:** Handler copies query code but uses the wrong DbSet or a shared AC15 table. [VERIFIED: adjacent code pattern]

**How to avoid:** Use only `UserSaveDataMomoiro`, `SongBestDataMomoiro`, `MomoiroFavoriteSongs`, and `MomoiroRecentSongs` after shared identity lookup. [VERIFIED: 41-PATTERNS.md]

**Warning signs:** No-cross-era tests fail when adjacent rows are seeded for the same BAID. [VERIFIED: expected validation]

### Pitfall 3: Over-Mapping Generated UserData Fields

**What goes wrong:** Challenge/friend/unsupported fields get nonzero defaults or are populated from unrelated common DTO fields. [VERIFIED: generated wire + Mapperly mapping risk]

**Why it happens:** Mapperly strict source mapping is relaxed or ignores are copied from a different era without checking Momoiro wire. [VERIFIED: Mapperly docs + generated wire comparison]

**How to avoid:** Use explicit `MapperIgnoreSource`/`MapperIgnoreTarget` entries and inspect `UserDataMappers.g.cs`. [CITED: mapperly.riok.app/docs/configuration/mapper/]

**Warning signs:** Generated source assigns `IsAutoTitleOn`, challenge arrays, or friend info from unrelated sources. [VERIFIED: generated wire inspection]

## Code Examples

### Entity Set Boundary

```csharp
// Source: 41-PATTERNS.md from Kimidori/Murasaki analogs.
public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataMomoiro> UserSaveDataMomoiro { get; }
    DbSet<SongBestDatumMomoiro> SongBestDataMomoiro { get; }
    DbSet<MomoiroFavoriteSongs> MomoiroFavoriteSongs { get; }
    DbSet<MomoiroRecentSongs> MomoiroRecentSongs { get; }
}
```

### Recent Readback Ordering

```csharp
// Source: UserDataQuery.Kimidori/Murasaki and Ac15NormalPlayWriter.
var recentSongs = await context.MomoiroRecentSongs
    .Where(row => row.Baid == baid)
    .OrderByDescending(row => row.LastPlayed)
    .Take(Ac15EraProfiles.Momoiro.Limits.MaxRecentSongs)
    .Select(row => row.SongNo)
    .ToArrayAsync(cancellationToken);
```

### Release Flag Compaction

```csharp
// Source: Ac15UserDataService + Ac15SongHashCodec.
var response = Ac15UserDataService.BuildResponse(snapshot, lists, display, modes, reward, profile);
var wireReleaseFlags = Ac15SongHashCodec.CompactBitset(
    response.SongFlags.HashReleaseSongFlg,
    momoiro.SongHashTable);
```

### Mapperly Build Gate

```powershell
# Source: Mapperly generated-source docs and AGENTS.md.
dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Treat Momoiro as scaffold/static success routes only. | Phase 41 wires identity/userdata/selfbest readback through Application handlers and MOMOIRO-owned EF state. | Phase 41 planning. [VERIFIED: 41-CONTEXT.md] | Runtime readback becomes stateful without playresult mutation. |
| Copy KIMIDORI/Murasaki runtime state wholesale. | Copy only readback-needed analogs and defer Dan/play/mutation tables. | Phase 41 research. [VERIFIED: 41-PATTERNS.md + 41-CONTEXT.md] | Smaller schema, stronger no-cross-era boundary. |
| Use raw song ID as crown array index. | Use MOMOIRO file/hash order for 380 ten-bit crown values. | Phase 41 research. [VERIFIED: Ac15CrownService + 40-VERIFICATION.md] | Prevents high-ID songs from being dropped. |
| Treat generated field presence as behavior proof. | Use generated fields as mapping surface only; require route/native/context evidence for behavior. | Existing Taiko project rule. [VERIFIED: AGENTS.md] | Prevents Challenge/Friend/unsupported route creep. |

**Deprecated/outdated:**

- Direct `Ac15CrownService.BuildInflatedBody` use for Momoiro crown readback is unsafe unless tests prove all relevant song IDs are dense under `CrownSongCount`. [VERIFIED: Ac15CrownService + Momoiro catalog]
- Any Phase 41 plan adding `SongPlayDatumMomoiro`, Dan tables, or challenge tables is broader than the readback scope. [VERIFIED: 41-CONTEXT.md]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Favorite response order can be deterministic `OrderBy(SongNo)` until native order evidence exists. | Direct Answers / Lists | Cabinet may expect insertion/order-of-selection behavior; Phase 42 writer may need an order column. |
| A2 | Favorite max remains `5` from Phase 40 conservative profile. | Direct Answers / Lists | Client may support a different cap; tests should frame this as current profile behavior. |
| A3 | 475-byte crown payload is correct because 380 songs times 10 bits equals 3800 bits. | Crown Representation | Native may include padding/versioning not represented by simple 10-bit packing. |
| A4 | `IsAutoTitleOn` should stay default/unset because semantics are unproven. | Wire Fields / Mapperly | Client may rely on a specific default if native behavior later proves it. |

## Open Questions

1. **What is the exact native favorite order?**  
   - What we know: fields and counts exist, and Phase 40 set the cap to 5. [VERIFIED: generated wire + 40-VERIFICATION.md]  
   - What's unclear: whether order is selection order, song-number order, or client-side sorted. [ASSUMED]  
   - Recommendation: use deterministic server ordering for Phase 41 and defer native order proof to a targeted IDA/client capture if tests or cabinet behavior demand it. [ASSUMED]

2. **Does `IsAutoTitleOn` have a backed save source?**  
   - What we know: Momoiro generated wire has nullable `is_auto_title_on` field 37. [VERIFIED: generated wire]  
   - What's unclear: whether any current common save field should populate it. [ASSUMED]  
   - Recommendation: leave unset/default in Phase 41 and do not map from `IsAutoCostumeOn`. [ASSUMED]

3. **Will Phase 42 need play-history storage separate from best rows?**  
   - What we know: Phase 41 readback can use `SongBestDatumMomoiro` only. [VERIFIED: 41-CONTEXT.md + Ac15SelfBestService]  
   - What's unclear: playresult mutation facts and history tables are Phase 42 scope. [VERIFIED: 41-CONTEXT.md]  
   - Recommendation: do not add `SongPlayDatumMomoiro` in Phase 41; add it in Phase 42 only if mutation implementation needs it. [VERIFIED: 41-CONTEXT.md]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test/migration | Yes | 10.0.201 | None needed. [VERIFIED: `dotnet --version`] |
| dotnet-ef | EF migration | Yes | 10.0.7 | Manual migration generation is not recommended. [VERIFIED: `dotnet ef --version`] |
| Git | Commit research/plans | Yes | 2.52.0.windows.1 | None needed. [VERIFIED: `git --version`] |
| IDA database | Optional targeted binary checks | Yes | `.tools/momoiro/EBOOT.ELF.i64` exists | Use only for targeted unresolved questions. [VERIFIED: filesystem probe + 41-CONTEXT.md] |
| SQLite tooling | Optional DB inspection | Yes | sqlite3 available | EF tests normally cover schema behavior. [VERIFIED: command availability probe] |

**Missing dependencies with no fallback:** none found. [VERIFIED: environment probes]

**Missing dependencies with fallback:** none found. [VERIFIED: environment probes]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1. [VERIFIED: Directory.Packages.props] |
| Config file | `Tests/Tests.csproj`. [VERIFIED: repo grep] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroRuntimeReadback|FullyQualifiedName~MomoiroControllerReadback|FullyQualifiedName~MomoiroRouteSurface|FullyQualifiedName~Ac15SongHashCodec" --no-restore -- RunConfiguration.DisableParallelization=true` |
| Full suite command | `dotnet test Tests/Tests.csproj --no-restore -- RunConfiguration.DisableParallelization=true` |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| MORDB-01 | MyDon/BAID shared identity plus MOMOIRO save only, no adjacent save reads/writes. | integration | focused Momoiro runtime readback filter | No - Wave 0 create `Tests/Momoiro/MomoiroRuntimeReadbackTests.cs`. [VERIFIED: test inventory] |
| MORDB-02 | `selfbest.php` returns Momoiro-owned normal/ura/shin rows and ignores adjacent rows. | integration/unit | focused Momoiro selfbest tests | No - Wave 0 add test methods. [VERIFIED: test inventory] |
| MORDB-03 | favorites/recent arrays cap/order/truncate and ignore adjacent era rows. | integration | focused Momoiro userdata tests | No - Wave 0 add test methods. [VERIFIED: test inventory] |
| MORDB-04 | `HashCrownFlg` length/default/high-ID placement/difficulty bits. | unit/integration | focused Momoiro crown tests plus shared codec test if helper added | No - Wave 0 add `MomoiroCrownReadbackTests`. [VERIFIED: test inventory] |
| MORDB-05 | `SongHashVer` and `HashReleaseSongFlg` use Momoiro catalog order/table. | integration | focused Momoiro userdata tests | No - Wave 0 add test methods. [VERIFIED: test inventory] |

### Sampling Rate

- **Per task commit:** run the focused Momoiro readback filter. [VERIFIED: 41-CONTEXT.md expected validation]
- **Per wave merge:** run `dotnet test Tests/Tests.csproj --no-restore -- RunConfiguration.DisableParallelization=true`. [VERIFIED: 41-CONTEXT.md expected validation]
- **Phase gate:** full serialized suite, `dotnet build TaikoLocalServer.slnx --no-restore`, Mapperly generated-source build/inspection, temp Host build if runtime artifacts change, and `git status --porcelain -- proto/momoiro`. [VERIFIED: 41-CONTEXT.md expected validation]

### Wave 0 Gaps

- [ ] `Tests/Momoiro/MomoiroRuntimeReadbackTests.cs` - covers MORDB-01 through MORDB-03. [VERIFIED: test inventory]
- [ ] `Tests/Momoiro/MomoiroControllerReadbackTests.cs` - covers protobuf route response mapping for BAID/MyDon/UserData/SelfBest. [VERIFIED: current controller scaffolds]
- [ ] `Tests/Momoiro/MomoiroCrownReadbackTests.cs` or equivalent methods - covers MORDB-04 high-ID and 475-byte behavior. [VERIFIED: Ac15CrownService risk]
- [ ] Mapperly generated-source inspection checklist - covers mapper behavior not expressible through source-text tests. [VERIFIED: AGENTS.md]

## Security Domain

Security enforcement is enabled because `.planning/config.json` does not disable it. [VERIFIED: .planning/config.json]

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | Limited | Existing cabinet access-code/card identity flow; Phase 41 does not add password/session auth. [VERIFIED: Ac15MyDonEntryService] |
| V3 Session Management | No | No session or cookie behavior is added. [VERIFIED: phase scope] |
| V4 Access Control | Yes | Era-owned DbSet boundaries and tests prevent cross-era state exposure. [VERIFIED: AGENTS.md + 41-CONTEXT.md] |
| V5 Input Validation | Yes | Protobuf request DTOs and bounded catalog/list queries; tests should cover oversized request song arrays only if existing handler behavior is changed. [VERIFIED: generated wire + handlers] |
| V6 Cryptography | No new crypto | Do not introduce custom crypto; existing credentials path remains unchanged. [VERIFIED: Ac15MyDonEntryService] |

### Known Threat Patterns for Phase 41

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Cross-era state confusion | Information Disclosure / Tampering | Query only MOMOIRO DbSets after shared identity lookup; add no-cross-era tests. [VERIFIED: AGENTS.md + 41-CONTEXT.md] |
| Unsupported field overposting | Tampering | Explicit Mapperly ignores and generated-source inspection. [CITED: mapperly.riok.app/docs/configuration/mapper/] |
| Unbounded response rows | Denial of Service | Cap favorite/recent arrays to profile limits and preserve requested selfbest song order. [VERIFIED: Ac15SelfBestService + Phase 40 profile] |
| Incorrect crown packing | Integrity | Unit/integration tests for byte length, high-ID song preservation, and difficulty placement. [VERIFIED: Ac15ProtocolBytes + Ac15CrownService] |

## Sources

### Primary (HIGH confidence)

- `.planning/phases/41-momoiro-identity-userdata-self-best-and-crown-readback/41-CONTEXT.md` - phase goal, requirements, native evidence summary, scope, validation shape. [VERIFIED: local file]
- `.planning/phases/40-momoiro-protocol-limits-root-catalog-and-route-behavior/40-VERIFICATION.md` - Phase 40 accepted catalog/profile/hash/crown-placement evidence. [VERIFIED: local file]
- `Adapters.GameProtocol.Momoiro/Wire/Game.cs` - current generated MOMOIRO request/response fields. [VERIFIED: repo grep]
- `Application/Handlers/*Kimidori.cs`, `Application/Handlers/*Murasaki.cs`, `Application/Ac15/*`, `Domain/Entities/*Kimidori.cs`, `Infrastructure/Persistence/TaikoDbContext.Kimidori.cs` - adjacent runtime readback patterns. [VERIFIED: repo grep]
- `Ac15SongHashCodec`, `Ac15ProtocolBytes`, `Ac15CrownService`, `Ac15SelfBestService` - shared packing and selfbest behavior. [VERIFIED: repo grep]
- `AGENTS.md` instructions provided by user - project-specific boundaries and testing rules. [VERIFIED: current prompt]

### Secondary (MEDIUM confidence)

- `https://mapperly.riok.app/docs/configuration/mapper/` - Mapperly mapper configuration, required mapping, ignore attributes. [CITED: mapperly.riok.app/docs/configuration/mapper/]
- `https://mapperly.riok.app/docs/configuration/mapper/#null-values` - Mapperly null-value behavior. [CITED: mapperly.riok.app/docs/configuration/mapper/#null-values]
- `https://mapperly.riok.app/docs/configuration/constant-generated-values/` - Mapperly `MapValue` behavior. [CITED: mapperly.riok.app/docs/configuration/constant-generated-values/]
- `https://mapperly.riok.app/docs/configuration/generated-source/` - Mapperly generated-source inspection command/path. [CITED: mapperly.riok.app/docs/configuration/generated-source/]

### Tertiary (LOW confidence)

- Favorite native order and exact native favorite cap remain inferred from current profile/adjacent behavior rather than newly proven by IDA instruction flow. [ASSUMED]
- Crown 475-byte envelope is arithmetic from verified song count and shared 10-bit crown layout; native instruction-flow proof remains deferred. [INFERRED: Phase 40 profile + Ac15ProtocolBytes]

## Metadata

**Confidence breakdown:**

- Standard stack: HIGH - versions are from `Directory.Packages.props` and local CLI probes. [VERIFIED: local files + commands]
- Architecture: HIGH - adjacent AC15 patterns and project rules are explicit. [VERIFIED: repo grep + AGENTS.md]
- Persistence split: HIGH for four required Phase 41 tables and deferred mutation tables. [VERIFIED: 41-CONTEXT.md + 41-PATTERNS.md]
- Wire mapping: HIGH for field presence and field names. [VERIFIED: generated wire]
- Crown implementation detail: MEDIUM - byte length and field placement are supported, but native constant/order proof remains incomplete. [INFERRED: 40-VERIFICATION.md + Ac15ProtocolBytes]
- Favorite ordering: LOW - deterministic server behavior is recommended, but native order is unproven. [ASSUMED]

**Research date:** 2026-06-26  
**Valid until:** 2026-07-03 for Mapperly/current package behavior; 2026-07-26 for stable local codebase findings unless generated wire or Phase 40 artifacts change.
