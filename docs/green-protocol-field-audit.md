# Green Protocol Field Constraint Audit

Baseline commit: `74fde93 TEMP baseline before Green field audit`

Date: 2026-05-13

Scope: Green request/response fields whose invalid serialized values can crash the client or corrupt state that is later echoed back. This report is documentation only; no broad code fixes were implemented during the audit.

## Baseline Summary

Previous work now captured by baseline commit `74fde93`:

- `a835f93` fixed the first Taikojuku slot bug:
  - `GetTaikojukuResponse` `get_dan` now uses the 1-based `ChallengeLevel`/Dan slot, not `UniqueId`.
  - `InitialData` Taikojuku `info_id` now uses `ChallengeLevel` values in `1..25`.
  - Tests were added for those cases.
- An earlier crash mitigation (now superseded) tried to fix this by *omitting* `disp_taikojuku_dan` when out of range. The user reproduced the crash on a new card with that mitigation in place. A follow-up IDA pass (see evidence doc 07) showed the Green client reads `disp_taikojuku_dan_` at message offset `+0x31C` unconditionally, with no proto2 `has_*()` gate. Absent on wire decodes as `0`, which is observationally identical to `=0` at the consumer.
- Current mitigation (post-evidence-07):
  - `Application/Handlers/UserDataQuery.Green.cs` returns sentinel `1` for any out-of-range saved `DispTaikojukuDan` (including the new-card default of `0`).
  - `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs` forces wire field `34` to `1` for any out-of-range input.
  - `Tests/Green/GreenUserDataMapperTests.cs` and `GreenIdentityHandlerTests.cs` assert sentinel `1` on the wire for invalid input and the new-card path.

Confirmed root cause:

- New-card boot crashes in `sub_7FDFFC`'s song-list filter loop (RPCS3 CIA `0x7FEDFC`, `Access violation reading 0x1A90000`).
- `sub_7FDFFC` calls `sub_A1BA7C` (a trampoline to `Taikojuku_GetDanSlotSongRange @ 0x127F98`) with the parsed userdata's `disp_taikojuku_dan_` value.
- `Taikojuku_GetDanSlotSongRange` computes `begin = g_TaikojukuDanSlotTable + 84*dan - 84` then `end = begin + 8 * *(begin + 0x50)`. For `dan = 0`, `begin = table - 84` and `*(begin + 0x50)` reads four undefined bytes immediately before the table; whatever the linker placed there is multiplied by 8 and added to `begin`, producing an `end` pointer far outside any mapped region.
- The caller's loop iterates `[begin, end)` in 8-byte steps and crashes when it walks past the end of the segment.
- The client's `sub_7FDFFC` itself initialises its local slot to `1` (`v105 = 1;` at line 747) and only overwrites it with the wire value on a particular branch - direct evidence that `1` is the canonical safe sentinel for this slot.
- Proto2 `optional` semantics do not protect this field: the application code does not call `has_disp_taikojuku_dan()` before reading the value. The proto2 syntax only *exposes* presence machinery; using it is the developer's choice, and the Green client developer did not.

IDA metadata saved by Agent 1:

- Preferred DB `H:\taiko\EBOOT.ELF.i64` failed to open with IDA error code 4.
- Fallback DB `H:\TaikoLocalServer\.tools\ida-snap\EBOOT.ELF.codex.i64` was used and saved.
- Metadata changes:
  - `0x127F98` renamed to `Taikojuku_GetDanSlotSongRange`.
  - `0x1108CEC` renamed to `g_TaikojukuDanSlotTable`.
  - `0x128DD8` renamed to `OnTaikoJukuResponse`.
  - `0x7FDFFC` commented as the screen/filter builder using the current Taikojuku/Dan slot.
  - Comment added near `Taikojuku_GetDanSlotSongRange`: `a2 is 1-based Dan slot; valid 1..25. 0 underflows table, >25 overflows.`

## Priority Findings

### Must Fix Now

| Area | Finding | Why it matters | Later test |
|---|---|---|---|
| Taikojuku songs | `ary_jukusong_data` must have at most 10 songs per pack. | IDA evidence from `OnTaikoJukuResponse` shows the client stores songs into a 10-entry slot buffer and the 11th song enters an exception path. | `GreenTaikojukuTests.OmitsOrCapsJukupackSongsAboveTen` |
| Song IDs | Persisted Green `song_no` values must be valid Green catalog IDs and fit the 128-byte/1024-bit tables. | `crownsdata.php` uses `SongId` as a 10-bit table index. Invalid IDs can be echoed by `selfbest.php`, favorites, or recents and silently disappear from crowns. | `GreenPlayResultHandlerTests.RejectsOutOfCatalogSongNo` and `GreenCrownResponseBuilderTests.IgnoresOrRejectsSongNo1024` |
| Course level | Green stage/selfbest levels must be `0..4`. | Current invalid levels map to `Difficulty.None`, creating bad best rows and ambiguous readback. | `GreenPlayResultHandlerTests.RejectsStageLevelOutsideZeroThroughFour` |
| Optional save overwrites | Play-result optional fields currently lose presence and overwrite existing save values with `0`/`false`. | Missing current costume, difficulty state, tutorial flags, or booleans can clear state and later serialize unsafe defaults. | `GreenPlayResultHandlerTests.OmittedOptionalFieldsPreserveExistingGreenSaveData` |
| Equipped costume IDs | Current costume slots can persist any `uint` and are echoed in `baidcheck.php`. | Costume fields are catalog references. Source bitsets imply per-slot IDs fit within 32-byte/256-bit domains. | `GreenPlayResultHandlerTests.RejectsInvalidCurrentCostumeIds` |
| Reward arrays | In-range forged tone/costume/title IDs unlock directly without catalog validation. | Bitset range checks alone do not prove the item exists or is eligible. | `GreenRewardExecutionTests.RejectsUnknownInRangeRewardIds` |
| Medal totals and item purchase | Medal additions and shop spending use unchecked `uint` arithmetic and client-supplied item price. | Overflow or forged item purchase can corrupt medal totals. | `GreenItemPurchaseTests.RejectsUnknownItemAndOverflowingPrice` and `GreenPlayResultHandlerTests.DoesNotOverflowMedalTotals` |
| Item shop advertising | `initialdatacheck.php` advertises `is_itemshop=true`, while `getitemshopinfo.php` returns an empty catalog with optional empty strings. | If the client expects valid season/item rows once advertised, empty defaults may be unsafe or user-visible broken state. | `GreenInitialDataTests.DoesNotAdvertiseEmptyItemShopCatalog` |
| `disp_taikojuku_dan` sentinel | `userdata.php` must serialize `disp_taikojuku_dan` as a value in `1..25`; omission is unsafe. | The Green client reads `disp_taikojuku_dan_` at message `+0x31C` unconditionally (no proto2 `has_*()` gate, verified at `sub_19CFE0:151`, `sub_1016F8:506`, `sub_24377C:176`, `sub_7FDFFC:755`). Absent on wire decodes as `0`, which underflows `Taikojuku_GetDanSlotSongRange @ 0x127F98` and crashes new-card boot (RPCS3 `Access violation reading location 0x1a90000` at CIA `0x7FEDFC`). See `docs/green-client-evidence/07-disp-taikojuku-dan-no-presence-check.md`. | `GreenUserDataMapperTests.UserData_FallsBackToSentinelOneForInvalidDispTaikojukuDan` |

### Safe But Document

| Area | Constraint | Current status | Later test |
|---|---|---|---|
<!-- `disp_taikojuku_dan` row moved to Must Fix Now: omission is unsafe, the
client reads message+0x31C unconditionally. See evidence doc 07. -->

| Initial Taikojuku `info_id` | Omit invalid rows; valid `1..25`; response currently takes 3. | Fixed in baseline. | `GreenInitialDataTests.TaikojukuInfoIdsAreChallengeLevelsOneThroughTwentyFive` |
| Taikojuku `get_dan` | Response pack `get_dan` must be `1..25`. | Handler produces valid slots; mapper still trusts DTO. | `GreenTaikojukuMapperTests.DropsInvalidCommonPackDanSlot` |
| Hash bitsets | Song/release hash fields are 128 bytes; tone 16 bytes; costume 32 bytes; title 128 bytes; dan flags 18/36 bytes; content info 32 bytes. | Most handlers pad/truncate to fixed sizes. | `GreenProtocolBytesTests.AllGreenFixedBitsetsHaveExpectedLengths` |
| Crown table | `hash_crown_flg` must zlib-inflate to 1280 bytes. | Current builder emits fixed-size compressed table. | Existing crown-size tests plus `GreenCrownsDataTests.HashCrownFlgInflatesTo1280Bytes` |
| Empty telop/folder/recommend | Empty responses appear safe while initial data does not advertise rows. | Keep not advertised until implemented. | `GreenCatalogEndpointTests.EmptyCatalogEndpointsDoNotSerializeUnknownOptionals` |

### Needs More Client Evidence

Evidence collected in `docs/green-client-evidence/` proves the Green client has the searched endpoint and protobuf field-name strings, but the generated xref artifacts did not recover parser/decompiler contexts for most fields. Treat these as schema evidence, not value-range proof, unless a row below says otherwise.

| Area | Evidence status | Current code follow-up |
|---|---|---|
| `song_hash_ver` | IDA-proven field strings; `0` acceptance not found. See `docs/green-client-evidence/01-version-update-fields.md`. | Add fail-fast nonzero catalog-version validation in `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs` or `GreenEraGameDataCatalog.cs`. |
| `verup_no` fields | IDA-proven field strings; exact zero/nonzero catalog semantics not found. Taikojuku `0` remains source/prior-IDA-inferred no-update. | Omit telop/folder/tournament/shop version fields until real catalog rows are implemented; validate Taikojuku rows beside existing slot/song guards. |
| `disp_dan_type`, `got_dan_max` | IDA-proven field strings; enum/range not found. See `02-userdata-baid-fields.md`. | Normalize unknown nonzero values in `Application/Handlers/BaidQuery.Green.cs` before `BaidResponseMapper`. |
| Costume/tone/title catalog IDs | IDA-proven related field strings; exact catalog sets not found. Bit capacities remain source-inferred. | Guard selected/default IDs in `BaidQuery.Green.cs`; replace reward "already unlocked" placeholder with catalog/eligibility validation in `RewardExecutionCommand.Green.cs`. |
| `option_flg` | IDA-proven field strings; length/content not found. | Make userdata `option_flg` presence-aware and omit empty unknown bytes in `CommonUserDataResponse.Green.cs` / `UserDataMappers.cs`. |
| Score/counter maxima | IDA-proven score/count field strings; chart maxima not found. | Add chart-aware score/count checks to `UpdatePlayResultCommand.Green.cs:IsValidGreenStage` once chart note data is available. |
| Crown state `3` | Not found for Green crown response bitsets. | Keep `GreenPlayResultMapping.cs` downgrading Dondaful to full combo for `hash_crown_flg` until client bitset state `3` is proven. |
| Ghost ranks/tokens/sections | IDA-proven field/container strings; ID domains and section caps not found. | Validate/cap ghost release IDs, tokens, ranks, winnings, and sections in `UpdatePlayResultCommand.Green.cs`; filter outputs in ghost query handlers. |
| Shop dates and item rows | IDA-proven shop item/date field strings; date format not found. Current tree omits empty date strings and no longer advertises an empty shop. | Preserve optional presence in `ItemPurchaseController.cs`; implement `GreenItemShopLoader` before advertising item shop rows. |

Evidence reports:

- `docs/green-client-evidence/01-version-update-fields.md`
- `docs/green-client-evidence/02-userdata-baid-fields.md`
- `docs/green-client-evidence/03-score-crown-counter-fields.md`
- `docs/green-client-evidence/04-ghost-fields.md`
- `docs/green-client-evidence/05-shop-reward-catalog-fields.md`
- `docs/green-client-evidence/06-evidence-summary.md`

## Field Findings

### InitialData and Taikojuku

| Endpoint | Field | Server source | Client use | Constraint | Current behavior | Risk | Recommended guard/test |
|---|---|---|---|---|---|---|---|
| `initialdatacheck.php` | `ary_taikojuku_data.info_id` | `GetInitialDataQuery.Green.cs`; `InitialDataMappers.cs` | IDA: queued into Taikojuku request list without range check. | 1-based Taikojuku/Dan slot `1..25`; `0`, `101+`, `20001+` unsafe. Omit row when unknown. | Uses `ChallengeLevel`, filters `1..25`, takes 3. | Low now; critical if regressed. | Keep handler guard. Add `GreenInitialDataMapperTests.OmitsInvalidTaikojukuInfoIds`. |
| `initialdatacheck.php` | `ary_taikojuku_data.verup_no` | `GreenTaikojukuEntry.cs`; mapper | IDA: version comparison before queue callback. | Required in row; `0` appears safe but may suppress update/fetch. | Defaults to `0`. | Low functional. | `GreenInitialDataTests.TaikojukuVerupNoZeroDoesNotBlockValidSlots`. |
| `initialdatacheck.php` | `ary_taikojuku_data` count | `GetInitialDataQuery.Green.cs` | IDA: queue capacity 25; request builder sends max 11 per request. | Keep `<=25`; prefer `<=11` per request cycle. | Emits max 3. | Low. | `GreenInitialDataTests.TaikojukuInitialRowsAreCapped`. |
| `initialdatacheck.php` | `hash_default_song_flg` | `GreenProtocolBytes.cs` | IDA: client copies 128 bytes and zero-fills short input. | Exactly 128 bytes; bit index is Green `song_no`/unique ID, not file order. | Emits fixed 128 bytes; ignores IDs `>=1024`. | Low. | `GreenProtocolBytesTests.DefaultSongFlagIs128BytesAndSkipsOutOfRangeIds`. |
| `initialdatacheck.php` | `hash_mainichidojo_all`, `hash_mainichidojo_rare` | `GetInitialDataQuery.Green.cs`; mapper | IDA: client copies each as 128 bytes and zero-fills short input. | Exactly 128 bytes each. | Emits zeroed 128-byte arrays. | Low. | `GreenProtocolBytesTests.MainichiDojoFlagsAre128Bytes`. |
| `initialdatacheck.php` | `song_hash_ver` | `GreenMusicInfoLoader.cs` | Needs more client evidence. | `uint`; nonzero catalog version preferred. | Uses `musicinfo.xml` header version; can become `0` if missing/invalid. | Low/Medium. | `GreenCatalogLoaderTests.RejectsMissingOrZeroSongHashVersion`. |
| `initialdatacheck.php` | feature booleans | `GetInitialDataQuery.Green.cs`; mapper | Presence bits; omitted defaults false. | Bool only; `is_danplay=true` must pair with only valid Dan slots. | Serializes `is_danplay=true`, `is_itemshop=true`, `is_ghostbattleplay=true`. | Medium for advertised empty itemshop. | `GreenInitialDataTests.AdvertisedFeaturesHaveFetchablePayloads`. |
| `taikojuku.php` | request `get_dan` | `TaikojukuController.cs`; `GetTaikojukuQuery.Green.cs` | IDA: request builder sends queued slots, max 11. | Request slot IDs are `1..25`; invalid values must not echo. | Filters valid slots; all-invalid request falls back to `1..min(count,25)`. | Low for real client. | `GreenTaikojukuTests.AllInvalidRequestSlotsFallbackIsCappedToEleven`. |
| `taikojuku.php` | `JukupackData.get_dan` | `GetTaikojukuQuery.Green.cs`; `TaikojukuMappers.cs` | IDA: indexes `g_TaikojukuDanSlotTable` as `84 * (get_dan - 1)`. | Must be `1..25`; omit/drop pack if unknown. | Handler emits valid slots; mapper blindly serializes common DTO. | Low now; critical if mapper bypassed. | `GreenTaikojukuMapperTests.DropsInvalidCommonPackDanSlot`. |
| `taikojuku.php` | `JukupackData.verup_no` | `TaikojukuMappers.cs` | IDA: nonzero can trigger cache write path; `0` skips update write. | `0` safe for no-update behavior. | Defaults to `0`. | Low. | `GreenTaikojukuMapperTests.SerializesZeroVerupNoForNoUpdate`. |
| `taikojuku.php` | `ary_jukupack_data` count | `GetTaikojukuQuery.Green.cs` | IDA: client tracks queued count and drains returned packs. | Match requested count; real client max request count 11. | Usually one per slot; hostile all-invalid request can fallback to up to 25. | Medium. | `GreenTaikojukuTests.ResponsePackCountIsCappedToClientRequestLimit`. |
| `taikojuku.php` | `ary_jukusong_data` count | `GreenTaikojukuLoader.cs`; mapper | IDA: 10-entry slot buffer; 11th song enters exception path. | Max 10 songs per pack. | XML songs are not capped; fallback emits 3. | High. | `GreenTaikojukuTests.CapsOrRejectsPackWithMoreThanTenSongs`. |
| `taikojuku.php` | `ary_jukusong_data.song_no` | Loader reads XML `uniqueid`; fallback uses catalog `SongNo`. | Stored in slot table and passed to music-index binding. | Green `musicinfo.xml` unique ID, valid catalog ID, `>0`, likely `<1024`. | XML medley song IDs not validated; fallback safe. | Medium. | `GreenTaikojukuTests.OmitsPackSongsNotInGreenCatalog`. |
| `taikojuku.php` | `ary_jukusong_data.level` | Loader reads XML `difficulty`; fallback clamps to `0..4`. | Stored as second dword in slot table; downstream use needs more evidence. | Treat as course/difficulty `0..4`. | XML difficulty not validated. | Medium. | `GreenTaikojukuTests.OmitsOrNormalizesPackSongDifficultyOutsideZeroThroughFour`. |

### BAID and UserData

| Endpoint | Field | Server source | Client use | Constraint | Current behavior | Risk | Recommended guard/test |
|---|---|---|---|---|---|---|---|
| `baidcheck.php` | `player_type`, `baid`, `access_code`, `is_publish` | `BaidController.cs`; generated `Game.cs` | Source-inferred identity scalars. | `baid > 0`; `player_type` enum-like `0/1`. | Always serializes assigned values. | Low. | `GreenIdentityHandlerTests.SerializesExpectedPlayerTypeAndNonzeroBaid`. |
| `baidcheck.php` | `mydon_name`, `title`, `titleplate_id`, `personid` | `BaidQuery.Green.cs`; `BaidResponseMapper.cs` | Source-inferred profile display/catalog values. | Name/title length and charset need client evidence; titleplate must be valid/unlocked. | Name unvalidated; `titleplate_id=0`; `personid` empty string serializes. | Medium. | `GreenIdentityHandlerTests.OmitsEmptyPersonIdAndValidatesTitleplate`. |
| `baidcheck.php` | `color_face`, `color_body`, `color_limb` | `UserSaveDataGreenExtensions.cs`; `BaidQuery.Green.cs` | Source-inferred palette IDs. | Palette domain needs client evidence. | Defaults `0/1/3`; any persisted `uint` serializes. | Medium. | `GreenIdentityHandlerTests.NormalizesInvalidDonColorIds`. |
| `baidcheck.php` | `ary_costumedata.costume_1..5` | `UpdatePlayResultCommand.Green.cs`; `BaidQuery.Green.cs` | Source-inferred equipped costume catalog IDs. | Catalog/unlocked IDs; source bitsets imply IDs `<256`. | Play result can persist any `uint`; mapper serializes nested optionals even when `0`. | High. | `GreenPlayResultHandlerTests.RejectsInvalidCurrentCostumeIds`. |
| `baidcheck.php` | `costume_flg_1..5` | `GreenProtocolBytes.cs`; `BaidQuery.Green.cs`; reward/playresult handlers | Source-inferred unlock bitsets. | Exactly 32 bytes each, 256 bits. | Defaults set bit `0`; persisted bytes padded/truncated; invalid in-range IDs accepted. | Medium. | `GreenIdentityHandlerTests.CostumeFlagsAre32BytesAndCatalogAware`. |
| `baidcheck.php` | `disp_dan_type` | `UserSaveDataGreen.cs`; `BaidResponseMapper.cs` | Source-inferred Dan display enum. | Valid enum needs client evidence. | Default `0` always serializes. | Medium. | `GreenIdentityHandlerTests.NormalizesUnknownDispDanType`. |
| `baidcheck.php` | `got_dan_max`, `got_dan_flg`, `got_danextra_flg` | `GreenProtocolBytes.cs`; `BaidQuery.Green.cs` | Source-inferred Dan clear summary. | `got_dan_flg` 18 bytes; `got_danextra_flg` 36 bytes; max range needs evidence. | Defaults zero arrays and `got_dan_max=0`; no Green dan updater found. | Medium. | `GreenIdentityHandlerTests.DanFlagsHaveExpectedLengthsAndMaxIsInRange`. |
| `baidcheck.php` | `default_tone_setting` | `UserSaveDataGreen.cs`; `BaidResponseMapper.cs` | Source-inferred selected tone ID. | Valid tone ID, probably unlocked in `tone_flg`; bitset implies `<128`. | Default `0`; any persisted `uint` serializes. | Medium. | `GreenIdentityHandlerTests.DefaultToneMustBeUnlockedAndInRange`. |
| `baidcheck.php` | medal counters and tutorial flags | `UpdatePlayResultCommand.Green.cs`; `ItemPurchaseCommand.Green.cs`; `BaidResponseMapper.cs` | Source-inferred counters/toggles. | Counters must not wrap; tutorial flag ranges need evidence. | Counters use unchecked `uint` addition; false/zero optionals serialize. | Medium/High. | `GreenPlayResultHandlerTests.DoesNotOverflowMedalTotals`. |
| `baidcheck.php` | `content_info` | `BaidResponseMapper.cs`; `GreenProtocolBytes.cs` | Source-inferred opaque bytes. | Exactly 32 bytes. | Always 32 zero bytes. | Low. | `GreenIdentityHandlerTests.ContentInfoIs32Bytes`. |
| `userdata.php` | Optional field shape | `UserDataMappers.cs`; generated `Game.cs` | Source-inferred field presence behavior. | Unknown optional values should be omitted. | Mapper assigns many `0`, `false`, and empty arrays, making defaults serialize. | Medium. | `GreenUserDataMapperTests.DefaultCommonResponseOmitsUnknownOptionalFields`. |
| `userdata.php` | `hash_release_song_flg`, `song_hash_ver` | `UserDataQuery.Green.cs`; `GreenProtocolBytes.cs` | Source-inferred release bitset/version. | 128 bytes; song IDs `<1024`; version nonzero preferred. | Releases first 20 catalog songs; IDs `>=1024` skipped. | Low. | `GreenUserDataHandlerTests.ReleaseSongFlagIs128BytesAndUsesCatalogIds`. |
| `userdata.php` | `tone_flg`, `title_flg` | `UserDataQuery.Green.cs`; `GreenProtocolBytes.cs` | Source-inferred unlock bitsets. | Tone 16 bytes; title 128 bytes. | Padded/truncated; invalid in-range bits can persist. | Medium. | `GreenUserDataHandlerTests.ToneAndTitleFlagsAreFixedLengthAndCatalogAware`. |
| `userdata.php` | `option_flg` | `UserSaveDataGreen.cs`; `UserDataQuery.Green.cs`; mapper | Needs client evidence. | Length/content unknown; omit if unknown until proven. | Defaults to empty byte array and serializes. | High. | `GreenUserDataMapperTests.OmitsEmptyOptionFlgUntilLengthKnown`. |
| `userdata.php` | `default_option_setting`, `default_shin_setting` | `UserSaveDataGreenExtensions.cs`; `UserDataQuery.Green.cs` | Source-inferred play settings. | `default_option_setting` exactly 2 bytes; bool toggle. | Fixed two zero bytes; false bool serializes. | Low. | `GreenUserDataHandlerTests.DefaultOptionSettingIsTwoBytes`. |
| `userdata.php` | `disp_taikojuku_dan` | `UserDataQuery.Green.cs`; `UserDataMappers.cs` | IDA-backed Taikojuku slot; consumer reads `*(msg+0x31C)` directly with no proto2 `has_*()` gate. | Must serialize `1..25` on the wire; omission is unsafe. | Handler + mapper emit sentinel `1` for any out-of-range save value (including the new-card default of `0`). | Critical: omission crashes new-card boot at `Taikojuku_GetDanSlotSongRange @ 0x127F98`. | `GreenUserDataMapperTests.UserData_FallsBackToSentinelOneForInvalidDispTaikojukuDan`; `GreenIdentityHandlerTests.UserData_Green_NewSaveSendsSentinelOneForDispTaikojukuDan`. |
| `userdata.php` | `difficulty_played_course`, `difficulty_played_star` | `PlayResultMappers.cs`; `UpdatePlayResultCommand.Green.cs`; `UserDataQuery.Green.cs` | Source-inferred profile progress scalars. | Valid domain needs client evidence; likely small enum/range. | Any `uint` persists and serializes; absent request maps to `0`. | High. | `GreenPlayResultHandlerTests.OmittedDifficultyPlayedFieldsPreserveExistingValues`. |
| `userdata.php` | `is_devil`, `is_challengecompe`, `is_tojiru` | `UpdatePlayResultCommand.Green.cs`; mapper | Source-inferred booleans. | Boolean; preserve optional absence if request-side. | Defaults false and serializes. | Low/Medium. | `GreenUserDataMapperTests.BooleanDefaultsAreOmittedOrDocumented`. |
| `userdata.php` | `ary_favorite_song_no`, `song_favorite_cnt` | `GreenFavoriteSongs`; `UpdatePlayResultCommand.Green.cs`; `UserDataQuery.Green.cs` | Source-inferred favorite song IDs and count. | Valid Green catalog IDs; list cap needs evidence. | No cap/order/catalog validation; count equals all rows. | High. | `GreenUserDataHandlerTests.FavoritesRejectInvalidSongIdsAndCountMatchesSerializedList`. |
| `userdata.php` | `ary_recent_song_no`, `song_recent_cnt` | `GreenRecentSongs`; `UpdatePlayResultCommand.Green.cs`; `UserDataQuery.Green.cs` | Source-inferred recent song IDs and count. | Valid song IDs; observed cap 10. | Takes 10 but orders by `SongNo` descending, not play time; no catalog validation. | Medium. | `GreenUserDataHandlerTests.RecentsAreValidSongsCappedAtTenAndOrderedByPlayTime`. |
| `userdata.php` | category counts and recommendation scalars | `UserSaveDataGreen.cs`; mapper | Needs client evidence. | Counter/id domains unknown; recommendation IDs must be valid songs. | Mostly default `0`; empty recommendation list. | Medium. | `GreenUserDataMapperTests.OmitsUnknownRecommendationIdsAndDefaultCounters`. |

### Crowns and SelfBest

| Endpoint | Field | Server source | Client use | Constraint | Current behavior | Risk | Recommended guard/test |
|---|---|---|---|---|---|---|---|
| `selfbest.php` | `result` | `GetSelfBestQuery.Green.cs`; `SelfBestMappers.cs` | Source-inferred success flag. | Likely `1=OK`; error values need evidence. | Always `1`. | Low. | `GreenSelfBestHandlerTests.EmptyAndPopulatedResponsesReturnSuccess`. |
| `selfbest.php` | `level` | `SelfBestController.cs`; `SelfBestMappers.cs` | Echoes requested course level. | `0..4`; require presence or document omitted behavior. | Always assigned, so `0` serializes even when request omitted. | Medium. | `GreenSelfBestHandlerTests.RejectsSelfBestLevelOutsideZeroThroughFour`. |
| `selfbest.php` | `ary_selfbest_score` count | `GetSelfBestQuery.Green.cs` | One row per requested song. | Response count should match request count; cap needs evidence. | Preserves order and duplicates; unknown songs get zero rows. | Medium. | `GreenSelfBestHandlerTests.CapsAndEchoesRequestedSongRowsInOrder`. |
| `selfbest.php` | `ary_selfbest_score[].song_no` | `GetSelfBestQuery.Green.cs` | Song lookup/echo key. | Green catalog ID, likely `<1024`. | Echoes any requested `uint`; no catalog validation. | Medium. | `GreenSelfBestHandlerTests.RejectsOrOmitsOutOfCatalogSongNo`. |
| `selfbest.php` | score fields | `SongBestDatumGreen`; mapper | Best score readback. | Nonnegative `uint`; max score needs chart evidence. | Saved score or `0`; Ura best always `0`; Shin rows zero. | Medium. | `GreenSelfBestMapperTests.ZeroRowsAreIntentionalAndRequiredFieldsSerialize`. |
| `crownsdata.php` | `song_hash_ver` | `CrownsDataController.cs`; `GreenMusicInfoLoader.cs` | Source-inferred cache/version key. | Valid nonzero catalog version preferred. | Always assigned; malformed XML can produce `0`. | Medium. | `GreenCrownsDataTests.SongHashVersionIsNonzeroWhenCatalogLoads`. |
| `crownsdata.php` | `hash_crown_flg` | `GreenCrownResponseBuilder.cs`; `GreenProtocolBytes.cs` | Source-inferred compressed crown table. | Zlib body inflates to 1280 bytes. | Always present; empty rows produce compressed all-zero table. | Low. | `GreenCrownsDataTests.HashCrownFlgInflatesTo1280Bytes`. |
| `crownsdata.php` | crown song index | `GreenCrownResponseBuilder.cs` | 10-bit song table index. | `0..1023`; should also be a valid Green catalog song. | `SongId >=1024` ignored in crowns but may echo elsewhere. | High. | `GreenCrownResponseBuilderTests.RejectsOrIgnoresSongNo1024AndIncludes1023`. |
| `crownsdata.php` | per-course crown bits | `GreenCrownResponseBuilder.cs`; `GreenCrownState.cs` | Five 2-bit course states. | `0 none`, `1 clear`, `2 full combo`; value `3` needs client evidence. | Dondaful downgraded to full combo; invalid difficulties ignored. | Medium. | `GreenCrownResponseBuilderTests.EncodesAllFiveCoursesAndIgnoresInvalidDifficulty`. |

### Catalog, Reward, Shop, Event, Ghost, and Misc

| Endpoint | Field | Server source | Client use | Constraint | Current behavior | Risk | Recommended guard/test |
|---|---|---|---|---|---|---|---|
| `getitemshopinfo.php` | shop season/item fields | `GetItemShopInfoController.cs`; `ItemShopMappers.cs`; generated `Game.cs` | Source-inferred shop catalog payload. | If advertised, rows require valid `item_no`, `item_type`, `item_id`, `item_price`; date format needs evidence. | Returns `result=1`, empty optional strings, no items. | High if advertised. | `GreenItemShopTests.EmptyShopIsNotAdvertisedOrOmitsOptionalStrings`. |
| `itempurchase.php` | request item fields and medal totals | `ItemPurchaseController.cs`; `ItemPurchaseCommand.Green.cs` | Source-inferred shop spend. | Must match real catalog item and server price; arithmetic must not wrap. | Always returns success if affordable by client-supplied price; unknown/free/omitted price can succeed. | High. | `GreenItemPurchaseTests.ValidatesCatalogItemTypeIdPriceAndMedalBalance`. |
| `rewardcardcheck.php` | optional `baid` | `RewardCardCheckQuery.Green.cs` | Source-inferred card lookup. | `baid=0` may be unknown-card sentinel; needs client evidence. | Always `result=1`, `baid` found or `0`. | Low. | `GreenRewardCardCheckTests.UnknownCardBaidPresenceMatchesClientContract`. |
| `rewardexecution.php` | `release_song_no` | `RewardExecutionCommand.Green.cs` | Source-inferred song reward. | Valid Green catalog song IDs. | Ignored; returns success. | High for song rewards. | `GreenRewardExecutionTests.PersistsOrRejectsReleaseSongNo`. |
| `rewardexecution.php` | tone/costume/title arrays | `RewardExecutionCommand.Green.cs`; `GreenProtocolBytes.cs` | Source-inferred unlock bitsets. | Tone `<128`, costume `<256`, title `<1024`, plus catalog membership. | In-range IDs unlock; out-of-range ignored. | High. | `GreenRewardExecutionTests.RejectsUnknownInRangeRewardIds`. |
| `gettelop.php` | telop fields | `GetTelopController.cs`; generated `Game.cs` | Source-inferred telop fetch. | Valid requested telop ID; date/string semantics need evidence. | Returns only `result=1`; initial data advertises no telops. | Low. | `GreenTelopTests.EmptyTelopNotAdvertisedAndDoesNotSerializeUnknownFields`. |
| `getfolder.php` | folder/song rows | `GetFolderQuery.Green.cs`; mapper | Source-inferred event folder songs. | Folder IDs from catalog; songs valid Green catalog IDs. | Returns only `result=1`; initial data advertises no folders. | Low. | `GreenFolderTests.AdvertisedFoldersHaveValidCatalogSongs`. |
| `recommend.php` | recommended song IDs | `GetRecommendQuery.Green.cs`; mapper | Source-inferred recommendations. | Valid Green song IDs; list length needs evidence. | Controller returns only `result=1`; fields absent. | Low. | `GreenRecommendTests.EmptyRecommendResponseOmitsUnknownSongIds`. |
| `tournamentcheck.php` | gacha/tournament fields | `TournamentCheckQuery.Green.cs`; generated `Game.cs` | Source-inferred gacha/tournament catalog. | Gacha flag lengths and item IDs need evidence. | Returns only `result=1`; lists empty. | Medium if advertised. | `GreenTournamentTests.EmptyTournamentNotAdvertisedUntilCatalogFieldsKnown`. |
| `challengecompe.php` | challenge tracks/status | `GetChallengeCompeQuery.Green.cs`; generated `Game.cs` | Source-inferred competition state. | Track `level` likely `0..4`; `option_flg` size unknown. | Returns only `result=1`; save default `IsChallengeCompe=false`. | Low unless advertised. | `GreenChallengeCompeTests.EmptyChallengeResponsePairsWithFalseUserdataFlag`. |
| `getghostdata.php` | flags, token/winnings/rank lists | `GetGhostDataQuery.Green.cs`; `GhostMappers.cs`; `GreenProtocolBytes.cs` | Source-inferred ghost state. | Release flag 16 bytes; played-song flag 128 bytes; token/rank IDs need evidence. | New user gets zero flags and required nested zero rank/perf; any token IDs can persist. | Medium. | `GreenGhostDataTests.NewUserResponseHasExpectedFixedFlagLengthsAndNoUnknownTokens`. |
| `getghostscore.php` | `ary_best_section_data` | `GetGhostScoreQuery.Green.cs`; `GhostMappers.cs` | Source-inferred per-section ghost score. | Request level `0..4`; section count/cap needs evidence. | Empty list if no play; persisted sections ordered by `SectionNo`. | Low/Medium. | `GreenGhostScoreTests.RejectsInvalidLevelAndOrdersSections`. |
| Green token count | no Green route/wire type | `GetTokenCountQuery.Green.cs` | Source-inferred internal stub only. | Token ID domain absent. | HTTP route would 404; handler returns empty if invoked internally. | Medium if exposed. | `GreenRoutingTests.DoesNotExposeTokenCountWithoutGreenWireContract`. |
| Green song purchase | no Green route/wire type | `PurchaseSongCommand.Green.cs` | Source-inferred internal stub only. | Song/token/price constraints absent. | Would return success without debit/unlock if wired. | High if exposed. | `GreenRoutingTests.DoesNotExposeSongPurchaseStubWithoutCatalogValidation`. |
| misc liveness/auth | static result/status fields | startupauth, heartbeat, bookkeeping, verup, headclerk2 controllers | Source-inferred liveness protocol. | Required response scalars populated. | Static success responses. | Low. | `GreenSmokeTests.MiscEndpointsSerializeRequiredScalars`. |

### Play Result Request-Side Persistence

| Request field | Persisted/echo path | Server source | Constraint | Current behavior | Risk | Recommended guard/test |
|---|---|---|---|---|---|---|
| outer `baid_conf` vs decoded `baid` | All saves use outer `baid_conf`; decoded `baid` unused. | `PlayResultController.cs`; `PlayResultMappers.cs` | Must match; exact auth/card semantics need evidence. | No cross-check. | High. | `GreenPlayResultHandlerTests.RejectsOuterInnerBaidMismatch`. |
| `playresult_data` | Gzip inflated protobuf. | `GreenPlayResultPayloadDecoder.cs` | Gzip protobuf; bounded decoded size. | Decode failures return `Result=0`; no size cap. | Medium. | `GreenPlayResultPayloadDecoderTests.RejectsOversizedInflatedPayload`. |
| `ary_stage_info` | One play row and best update per stage. | `UpdatePlayResultCommand.Green.cs` | Stage count cap needs client evidence. | Unbounded persistence loop. | Medium. | `GreenPlayResultHandlerTests.RejectsOversizedStageArray`. |
| `stage.song_no` | Play row, best row, selfbest/crowns/favorite/recent echo. | `UpdatePlayResultCommand.Green.cs`; `GreenCrownResponseBuilder.cs` | Green catalog song ID and `<1024`. | Any `uint` persists; crowns ignore `>=1024`. | High. | `GreenPlayResultHandlerTests.RejectsOutOfCatalogStageSongNo`. |
| `stage.level` | Stored as `Difficulty`. | `GreenPlayResultMapping.cs` | `0..4`. | Other values map to `Difficulty.None` and persist. | High. | `GreenPlayResultHandlerTests.RejectsStageLevelOutsideZeroThroughFour`. |
| `stage.play_result` | Stored as `Crown`; echoed in crowns. | `GreenPlayResultMapping.cs` | `0..3`, with `3` crown encoding still needs client evidence. | `>3` becomes `None`; best can still persist. | Medium. | `GreenPlayResultHandlerTests.RejectsPlayResultOutsideZeroThroughThree`. |
| `stage.play_score` | Play/best row; selfbest echo. | `UpdatePlayResultCommand.Green.cs` | Score maximum needs chart evidence. | Any `uint` persists. | High. | `GreenPlayResultHandlerTests.RejectsImplausiblyLargeScoreAfterChartLimitKnown`. |
| count fields | Play history and ghost sections. | `UpdatePlayResultCommand.Green.cs` | Counts should not exceed chart notes. | Any `uint` persists. | Medium. | `GreenPlayResultHandlerTests.RejectsImpossibleHitCountsWhenChartNotesKnown`. |
| `stage.option_flg`, `stage.tone_flg` | Stored on play row. | `PlayResultMappers.cs` | Exact byte lengths need client evidence. | Null becomes `[]`; arbitrary length persists. | Medium. | `GreenPlayResultHandlerTests.RejectsOverlongStageOptionAndToneFlags`. |
| `stage.is_favorite`, `stage.is_recent` | Userdata favorite/recent arrays. | `UpdatePlayResultCommand.Green.cs`; `UserDataQuery.Green.cs` | Song must be valid catalog song. | Invalid song can enter favorites/recent; recent ordered by song ID. | High. | `GreenPlayResultHandlerTests.InvalidSongCannotEnterFavoritesOrRecents`. |
| `stage.play_dan`, top-level `dan_result` | `play_dan` stored on play row; `dan_result` ignored. | `PlayResultMappers.cs`; `UpdatePlayResultCommand.Green.cs` | `play_dan` should be `0` or `1..25`; result values need evidence. | Any `play_dan` persists. | Medium. | `GreenPlayResultHandlerTests.RejectsInvalidPlayDanSlot`. |
| reward arrays | Userdata/BAID bitsets. | `UpdatePlayResultCommand.Green.cs`; `GreenProtocolBytes.cs` | Tone `<128`, costume `<256`, title `<1024`, plus catalog eligibility. | In-range IDs unlock directly. | High. | `GreenPlayResultHandlerTests.RejectsForgedRewardIds`. |
| current/play costume | BAID equipped costume fields. | `PlayResultMappers.cs`; `UpdatePlayResultCommand.Green.cs` | Catalog/unlocked IDs; likely `<256`; preserve optional absence. | Missing current costume maps to all zero and overwrites; any `uint` persists. | High. | `GreenPlayResultHandlerTests.MissingCurrentCostumeDoesNotClearSavedCostume`. |
| difficulty played fields | Userdata echo. | `PlayResultMappers.cs`; `UpdatePlayResultCommand.Green.cs` | Valid domain needs client evidence; preserve optional absence. | Any `uint` persists; absence maps to `0` and overwrites. | High. | `GreenPlayResultHandlerTests.OmittedDifficultyPlayedFieldsPreserveExistingValues`. |
| medal rewards | BAID medal totals. | `UpdatePlayResultCommand.Green.cs` | Economy max and checked arithmetic. | Any `uint` added; overflow risk. | High. | `GreenPlayResultHandlerTests.DoesNotOverflowMedalTotals`. |
| tutorial/devil booleans and flags | BAID/userdata echo. | `PlayResultMappers.cs`; `UpdatePlayResultCommand.Green.cs` | Preserve optional presence; tutorial ranges need evidence. | Absent optionals become `0`/`false` and overwrite. | Medium. | `GreenPlayResultHandlerTests.OmittedTutorialAndBooleanFieldsPreserveExistingValues`. |
| `play_datetime` | Last play date and play row timestamp. | `UpdatePlayResultCommand.Green.cs`; `BaidQuery.Green.cs` | Exact request format needs evidence; server echo format is `yyyyMMddHHmmss`. | `DateTime.TryParse`; invalid becomes server current time. | Medium. | `GreenPlayResultHandlerTests.RejectsInvalidAndFuturePlayDatetime`. |
| `area_code`, `play_mode` | Userdata area and play row mode. | `UpdatePlayResultCommand.Green.cs`; generated `Game.cs` | `PlayMode` valid set likely `0,1,4,6`; area range needs evidence. | Any `uint` persists. | Medium. | `GreenPlayResultHandlerTests.RejectsUnknownPlayModeAndAreaCode`. |
| ghost request data | Ghost data and score endpoints. | `PlayResultMappers.cs`; `UpdatePlayResultCommand.Green.cs`; ghost handlers | Release info `<128`; token/rank/section domains need evidence. | Out-of-range release IDs ignored; any token/rank/section values persist. | Medium. | `GreenGhostRewardTests.RejectsUnknownTokenRankAndSectionValues`. |
| ignored DTO-only fields | No current echo path. | `PlayResultMappers.cs`; generated `Game.cs` | Document ignored behavior before future use. | Ignored or DTO-only. | Low. | `GreenPlayResultMapperTests.DocumentsIgnoredGreenPlayResultFields`. |

## Implementation Rule For Later

When implementing fixes after this report:

1. Treat optional protobuf fields as absent by default. Only set them when the value is known valid for the Green client.
2. Table indices and slots must be domain-checked before mapping to wire types.
3. Catalog references must be checked against Green catalog data, not only bitset capacity.
4. Request-side values that can be echoed later must be validated before persistence.
5. Required fields still need values, but they must be protocol-safe sentinel values proven by client evidence or tests.
6. Every guard should get a targeted regression test named in the tables above.
