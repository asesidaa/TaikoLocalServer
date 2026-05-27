# Blue A4 Enso Play Result, Score Readback, Crowns, And Rewards Spec

**Date:** 2026-05-28
**Status:** approved design; ready for implementation plan after written-spec review
**Scope:** Add Blue normal-play result persistence, self-best readback, crown
readback, favorites/recents, and playresult-carried unlock updates. This stage
does not implement battle, Dani progression, item-shop purchase state,
`rewardexecution.php`, Tokkun, Banacoin, AdminApi, or WebUI behavior.

## Purpose

Stage A4 replaces the Blue A1 success stubs for `playresult.php`,
`selfbest.php`, and `crownsdata.php` with real normal-play behavior.

The target is a Blue normal Enso flow:

1. A registered Blue card plays a normal song.
2. `playresult.php` directly receives a Blue `PlayResultRequest`.
3. The server records valid normal or Shin stage results.
4. Self-best and crown readback reflect the saved Blue results.
5. `userdata.php` readback reflects favorites, recents, counters, medals, and
   unlock arrays updated by normal play.

## Evidence Inputs

- Roadmap: `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`
- A0 evidence:
  `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md`
- A1 adapter skeleton:
  `docs/superpowers/specs/2026-05-27-blue-a1-era-foundation-adapter-skeleton-design.md`
- A2 catalog/data layout:
  `docs/superpowers/specs/2026-05-28-blue-a2-catalog-data-layout-design.md`
- A3 identity/profile/userdata:
  `docs/superpowers/specs/2026-05-28-blue-a3-identity-profile-userdata-design.md`
- Blue wire shape: `proto/blue/taiko.proto`
- Current Blue stubs: `Adapters.GameProtocol.Blue/Controllers/`
- Green implementation references:
  - `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`
  - `Adapters.GameProtocol.Green/Controllers/SelfBestController.cs`
  - `Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs`
  - `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
  - `Adapters.GameProtocol.Green/Mappers/SelfBestMappers.cs`
  - `Application/Handlers/UpdatePlayResultCommand.Green.cs`
  - `Application/Handlers/GetSelfBestQuery.Green.cs`
  - `Application/Common/GreenCrownResponseBuilder.cs`
  - `Application/Common/GreenPlayResultMapping.cs`
  - `Application/Common/GreenProfileCounters.cs`

## Decisions

| Topic | Decision |
|---|---|
| Playresult transport | Blue `playresult.php` maps the request body directly as `PlayResultRequest`; no Green-style gzip payload decoder. |
| Persistence | Add Blue-owned play, best, favorite, and recent tables. Do not reuse Green tables. |
| Handler shape | Add Blue partials to existing command/query handlers where the existing dispatch model already fits. |
| Normal stage modes | Save only recognized Blue normal/Shin stages, initially stage modes `0` and `1`. |
| Unsupported stage modes | Log unsupported or unknown stage modes, skip those stages, and still return result `1`. Do not reject the whole request. |
| Green AI stage modes | Treat Green stage modes `3` and `4` as Green ghost/AI meanings, not Blue normal meanings. |
| Self-best | Read from Blue best rows and preserve requested song order. |
| Crowns | Return a Blue-owned packed crown table built from Blue best rows and Blue catalog song numbers. |
| Unlocks | Apply `release_song_no`, `get_tone_no`, `get_costume_no_*`, and `get_title_no` from `PlayResultRequest` to `UserSaveDataBlue`. |
| Favorites/recents | Persist Blue favorites and recents in Blue-owned tables and expose them through `userdata.php`. |
| Reward execution | Keep `rewardexecution.php` deferred/log-only in A4. Green does not currently use it for this flow. |
| Battle/Tokkun | Log and ignore battle, release-battle, and Tokkun fields in A4. Do not add behavior or storage. |

## Architecture And Boundaries

A4 adds Blue-owned persistence beside `UserSaveDataBlue`:

```text
Domain/
  Entities/
    SongPlayDatumBlue.cs
    SongBestDatumBlue.cs
    BlueFavoriteSongs.cs
    BlueRecentSongs.cs

Application/
  Abstractions/
    ITaikoDbContext.Blue.cs
  Common/
    BluePlayResultMapping.cs
    BlueProfileCounters.cs
    BlueCrownResponseBuilder.cs
  Handlers/
    UpdatePlayResultCommand.Blue.cs
    GetSelfBestQuery.Blue.cs

Infrastructure/
  Persistence/
    TaikoDbContext.Blue.cs
    Migrations/<EF-generated AddBluePlayResultSupport migration>

Adapters.GameProtocol.Blue/
  Mappers/
    PlayResultMappers.cs
    SelfBestMappers.cs
```

`SongPlayDatumBlue` stores one row per saved Blue stage result.
`SongBestDatumBlue` stores the current best score/rate/crown keyed by `Baid`,
`SongId`, `Difficulty`, and `IsShin`. `BlueFavoriteSongs` and
`BlueRecentSongs` mirror the Green practical limits, but remain Blue-owned.
A4 also adds a Blue-owned `ReleaseSongFlg` byte array to `UserSaveDataBlue` so
`release_song_no` from `PlayResultRequest` has an explicit save-state target.

The implementation may extract neutral helpers only when the helper accepts
explicit byte counts or neutral domain values. Blue A4 must not depend on:

- Green generated wire types;
- `GreenProtocolBytes`;
- Green entities;
- Green ghost helpers;
- Green Dan helpers;
- Green item-shop state.

If crown packing or difficulty/crown mapping is structurally identical to
Green, copy the behavior into Blue-owned helpers or extract a neutral helper
behind Blue-owned constants. Do not make Blue import Green constants just
because the initial values match.

## Endpoint Behavior

### `playresult.php`

The Blue controller should:

1. Log high-signal request identifiers: BAID, chassis id, shop id, play
   datetime, stage count, and whether battle/Tokkun fields are present.
2. Map the direct `PlayResultRequest` body to `CommonPlayResultData`.
3. Send `UpdatePlayResultCommand(request.Baid, GameEra.Blue, common)`.
4. Return `PlayResultResponse { Result = result }`.

There is no Green-style outer/inner BAID comparison because Blue A0 evidence
showed that Blue `playresult.php` is a direct protobuf request rather than a
compressed inner playresult payload.

### `selfbest.php`

The Blue controller should:

1. Send `GetSelfBestQuery(request.Baid, GameEra.Blue, request.Level,
   request.ArySongNoes ?? [])`.
2. Map the common response to Blue `SelfBestResponse`.
3. Preserve the requested song order.
4. Return zero score rows for requested songs with no saved Blue best row.

### `crownsdata.php`

The Blue controller should:

1. Read `SongBestDataBlue` rows for the requested BAID.
2. Build a packed crown body using Blue catalog song numbers as valid indexes.
3. Return `Result = 1`, `SongHashVer` from `IBlueCatalog`, and gzip-compressed
   `HashCrownFlg`.

The crown table should include both normal and Shin best rows. For each song
and difficulty, the returned crown state should be the maximum saved crown
across normal and Shin rows.

### `userdata.php` Integration

A4 should update `UserDataQuery.Blue.cs` so Blue userdata readback returns:

- `AryFavoriteSongNoes` from `BlueFavoriteSongs`;
- `AryRecentSongNoes` from `BlueRecentSongs`, ordered newest first and capped
  to ten;
- `HashReleaseSongFlg` as the union of Blue catalog default song flags and
  `UserSaveDataBlue.ReleaseSongFlg`;
- counters and flags from `UserSaveDataBlue` after playresult updates.

A4 should not add AdminApi or WebUI surfaces for these rows. That belongs to
A7.

### `rewardcardcheck.php` And `rewardexecution.php`

`rewardcardcheck.php` should use the existing shared card lookup behavior:
return `Result = 1` and the known BAID for the access code, or BAID `0` for an
unknown card. It should not create Blue save data or reward state.

`rewardexecution.php` stays deferred/log-only in A4. It should not apply
unlocks, touch medals, write shop state, or create new reward state. Unlocks
that matter for A4 come from `PlayResultRequest` arrays.

## Playresult Update Rules

`UpdatePlayResultCommand.Blue.cs` should follow the Green cabinet-friendly
response posture while keeping Blue persistence separate.

Top-level request handling:

- BAID `0` returns `1` without writes.
- Unknown shared user returns `1` with a warning and no writes.
- Invalid medal arithmetic returns `1` with a warning and no writes for medal
  changes. Valid stages can still be saved if the request can otherwise be
  processed safely.
- `PlayDatetime` should parse when possible; otherwise use the current server
  time and log the fallback.

Save-data updates:

- `UserSaveDataBlueExtensions.CreateDefaultBlueSaveData()` should initialize
  `ReleaseSongFlg` as a fixed-width zero byte array using Blue constants.
- Add `GetDonmedal` to `UserSaveDataBlue.TotalGetDonmedal`.
- Add `GetKatsumedal` to `UserSaveDataBlue.TotalGetKatsumedal`.
- Update `ItemshopTutorialFlg` if present.
- Update `IsDevil`, `IsExplain`, and `WaiwaiTutorialFlg` if present.
- Update `DifficultyPlayedCourse` and `DifficultyPlayedStar` only when the
  request serialized those optional fields.
- Update `LastPlayDatetime` and `PrevAreaCode`.
- Apply current costume when `AryCurrentCostume` is present and
  `IsAutoCostumeOn` is true.
- Apply release and unlock arrays to Blue fixed-width bitsets:
  - `release_song_no` -> `UserSaveDataBlue.ReleaseSongFlg`.
  - `get_tone_no` -> `ToneFlg`.
  - `get_costume_no_1..5` -> `CostumeFlg1..5`.
  - `get_title_no` -> `TitleFlg`.

Stage handling:

- Save stage mode `0` and `1` as recognized Blue normal/Shin stage modes.
- Do not treat Green stage modes `3` and `4` as Blue normal stage modes.
- For unsupported or unknown stage modes, log BAID, song number, level, and
  stage mode, skip that stage, and continue.
- Reject an individual stage from persistence when song id is outside
  `BlueProtocolBytes.SongFlagBytes * 8` or level is outside `1..5`.
- A request can return `1` even when all stages are skipped for unsupported
  modes or invalid stage data.

Saved stage data should include the normal fields already present in
`SongPlayDatumGreen` when those fields exist in the Blue request:

- song id, difficulty, crown, score, score rate;
- good/ok/miss/combo/hit/pound counts;
- star level, option flags, tone flags;
- play mode, stage mode, `IsShin`;
- music category, selected folder, favorite/recent/papamama/pushed flags;
- soul gauge, play Dan value, Waiwai result/gauge, and play time.

Best-score updates:

- Map Blue level `1..5` to `Easy`, `Normal`, `Hard`, `Oni`, and `UraOni`.
- Map Blue play result `1`, `2`, and `3` to `Clear`, `Gold`, and `Dondaful`.
- Keep normal and Shin best rows separate through `IsShin`.
- Update best score/rate when the incoming score is higher.
- Update crown when the incoming crown ranks higher than the saved crown.

Favorites and recents:

- When a saved stage has `IsFavorite = true`, add the song to
  `BlueFavoriteSongs` if the user has fewer than five favorites.
- When a saved stage has `IsFavorite = false`, remove an existing favorite for
  that song.
- Upsert `BlueRecentSongs` with the stage play time for every saved stage.
- Trim recents to the ten newest rows after save.

Profile counters:

- Increment genre counters using the same category ids as the Green normal
  path unless Blue catalog evidence later proves a different category domain.
- Increment pushed/favorite/recent counters using saturating arithmetic.
- Do not update Dan score rows, Blue Dan summary fields, battle state, shop
  purchase state, or reward-execution state.

## Error Handling

A4 should keep the cabinet protocol response permissive:

- Guest BAID is not an error.
- Unknown shared user is a warning, not a failed protocol response.
- Unsupported stage modes are warnings and per-stage skips.
- Invalid stage level or song id skips that stage.
- Unsupported battle, release-battle, or Tokkun fields are logged and ignored.
- Mapper or deserialization failures that prevent understanding the request can
  return `Result = 0`.

The implementation should prefer targeted warnings with BAID, song number,
level, stage mode, and endpoint name over broad request dumps. Full request
string logging already exists in A1 stubs; A4 can reduce noisy logs where
needed.

## Testing And Verification

The implementation plan should use focused, test-first checks:

- Blue playresult mapper tests proving:
  - direct `PlayResultRequest` maps without a payload decoder;
  - stage fields, unlock arrays, optional current costume, difficulty-played
    optional presence, and stage mode are preserved;
  - battle/Tokkun field presence is detectable for logging without mapping to
    persisted behavior.
- Blue playresult handler tests proving:
  - guest BAID returns `1` and writes nothing;
  - unknown shared user returns `1` and writes nothing;
  - normal stage saves play and best rows;
  - normal and Shin best rows are separate;
  - higher score updates best score/rate;
  - higher crown updates crown;
  - unsupported stage mode skips only that stage and still returns `1`;
  - stage modes `3` and `4` are not saved as Blue normal stages;
  - out-of-range song id or level skips that stage;
  - favorite add/remove and recent trimming follow Blue limits;
  - medal overflow does not corrupt save data;
  - tone, costume, and title unlock arrays update Blue bitsets;
  - release-song arrays update `UserSaveDataBlue.ReleaseSongFlg`;
  - current costume updates only when auto-costume is enabled;
  - no Blue Dan, battle, shop, or reward-execution rows are written.
- Blue userdata tests proving:
  - favorites and recents are returned from Blue-owned tables;
  - recent songs are newest first and capped at ten.
- Blue selfbest tests proving:
  - requested song order is preserved;
  - missing best rows produce zero scores;
  - normal and Shin arrays read from `SongBestDataBlue`.
- Blue crowns tests proving:
  - empty best rows return an all-zero packed crown table;
  - Blue song numbers are used as crown indexes;
  - normal and Shin best rows are both considered;
  - out-of-catalog best rows are ignored.
- Source guard tests proving Blue A4 code does not reference:
  - `Adapters.GameProtocol.Green.Wire`;
  - `GreenProtocolBytes`;
  - Green entities;
  - Green ghost helpers;
  - Green Dan helpers;
  - Green item-shop state.

Verification commands:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenPlayResultHandlerTests
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenSelfBestMapperTests
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a4"
```

If normal Host output is locked, use the temp output build path above.

## Acceptance Criteria

- Blue `playresult.php` persists direct normal Blue play results without using
  Green's compressed payload decoder.
- `SongPlayDatumBlue`, `SongBestDatumBlue`, `BlueFavoriteSongs`, and
  `BlueRecentSongs` are mapped to Blue-owned tables.
- Blue normal and Shin best rows are stored separately.
- Blue self-best readback reflects saved Blue best rows.
- Blue crown readback reflects saved Blue crown rows and the Blue catalog song
  hash version.
- Blue `userdata.php` returns Blue favorite and recent songs after playresult
  writes.
- Blue playresult updates medals, counters, last-play data, current costume,
  release-song flags, and tone/costume/title unlock bitsets.
- Unsupported or unknown stage modes are logged and skipped while the endpoint
  still returns `Result = 1`.
- Stage modes `3` and `4` are not treated as Blue normal stage modes.
- `rewardexecution.php` remains deferred/log-only.
- Battle, Dani, item-shop purchase state, Tokkun, Banacoin, AdminApi, and WebUI
  behavior remain out of scope.
- Blue A4 code does not depend on Green protocol constants, Green generated
  wire types, Green entities, Green ghost helpers, Green Dan helpers, or Green
  item-shop state.

## Out Of Scope

- Real `rewardexecution.php` behavior.
- Blue reward state beyond unlock arrays carried in `playresult.php`.
- Blue Dani progression, Dan score rows, Dan summary flags, and WebUI Dan
  display.
- Blue item-shop purchase state, active-season medal state, and shop item
  locking.
- Blue battle mode, `BattleStageData`, `BattleUserData`, release-battle flags,
  battle tokens, NPC state, and battle progression.
- Tokkun behavior.
- Banacoin balance, payment, info, or error behavior.
- AdminApi and WebUI surfaces.
- Parsing `present.xml`, `spacialbaid.xml`, or `waiwaiconfig.xml`.
- Runtime scraping of external pages or wiki data.

## Handoff To Later Stages

A5 should own Blue Dani completion, Dan score rows, and Dan display-state
updates. A6 should own Blue item-shop purchase, reward execution if cabinet
evidence shows it is needed, active-season medal state, and shop item locking.
A7 should expose Blue score, favorite, recent, customization, and shop state
through AdminApi and WebUI. Track B should design every Blue battle field and
`battleuserdata.php` behavior separately.
