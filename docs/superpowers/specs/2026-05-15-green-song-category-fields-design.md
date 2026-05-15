# Green Song Category & Recommendation Fields — Design

**Date:** 2026-05-15
**Branch:** `feat/green-version-support`
**Scope:** Wire up the song-category counters, pushed-song counter, recommend-songs catalog, and the adjacent recent-list/favorites-cap fixes on the Green save and load paths.

## Background

The Green (AC15) client tracks per-profile counters that drive title (称号) unlocks at 10 / 50 / 100 plays plus the "達人" title for each:

- 8 genre counters: `categ_jpop_cnt`, `categ_anime_cnt`, `categ_doyo_cnt`, `categ_variety_cnt`, `categ_classic_cnt`, `categ_game_cnt`, `categ_namco_cnt`, `categ_vocaloid_cnt`
- `song_pushed_cnt` — plays of pushed (おすすめ) songs
- `song_favorite_cnt` — plays of favorited songs
- `song_recent_cnt` — plays of songs picked from the 最近あそんだ曲 folder

These columns already exist on `UserSaveDataGreen` and are returned on `userdata.php`, but no handler ever writes to them. They sit at 0 forever, so all the play-count titles are unreachable in our emulation today.

Two adjacent gaps surfaced while auditing the same code path:

1. **Pushed songs are server-driven initial data** (via `recommend.php` and the `userdata.php` `recommend_song`/`recommend_best_song` fields), but our server advertises none. The `is_pushed` stage flag therefore never fires in practice, so even with correct save plumbing `song_pushed_cnt` would never advance.
2. **The 最近あそんだ曲 (recently-played) folder is real UI** the client renders during song select, ordered by play recency. Our load path orders by `SongNo` descending and the save path only adds rows when the client opted-in via `stage.IsRecent` — both wrong.
3. **Favorites cap of 5** (Green-era; raised to 10 only in ムラサキVer) is not enforced on save.

This design wires all of the above as one cohesive change.

## Goals

- Genre / pushed / favorite / recent counters move on each play, driving title progression correctly.
- Operators can declare pushed songs via a JSON file under `wwwroot/data/green/` without code changes.
- The 最近あそんだ曲 folder reflects actual play recency, capped at 10 entries.
- Favorites list capped at 5 on the play-result write path.
- Per-stage `IsPushed` is captured end-to-end (wire → DTO → handler → DB) for both counter aggregation and future replay/admin uses.

## Non-goals

- Profile-aware recommendations (age/gender). Older Taiko cabinets had a face camera; that hardware doesn't exist in RPCS3 emulation. The `gender_type`/`player_age` request fields on `recommend.php` are accepted and ignored. Recommendations are global.
- Changes to `TotalCreditCnt`. Credit accounting is meaningless in our emulated context.
- Rewriting `IsFavorite` toggle semantics on the play-result path. The audit doc flags this as questionable, but it's out of scope here; the cap-of-5 guard is sufficient.
- TaikoWebUI changes — admin UI doesn't surface these counters.
- Nijiiro parity. None of these fields apply to the Nijiiro protocol.

## Genre → counter mapping

The protocol's `music_categ` (uint, per stage) maps to a counter via the `Domain.Enums.SongGenre` ordering:

| `music_categ` | `SongGenre` | Counter |
|---|---|---|
| 0 | Pop | `CategJpopCnt` |
| 1 | Anime | `CategAnimeCnt` |
| 2 | Kids | `CategDoyoCnt` (どうよう) |
| 3 | Vocaloid | `CategVocaloidCnt` |
| 4 | GameMusic | `CategGameCnt` |
| 5 | NamcoOriginal | `CategNamcoCnt` |
| 6 | Variety | `CategVarietyCnt` |
| 7 | Classical | `CategClassicCnt` |

Values outside 0..7 reject the entire play (consistent with the existing `Level` / `StageMode` / `PlayResult` validation in `IsValidGreenStage`).

## Architecture

```
Wire (proto)                  DTO                          Handler                       DB
─────────────────────────────────────────────────────────────────────────────────────────────────
PlayResultDataRequest         CommonPlayResultData         UpdatePlayResultCommand       UserSaveDataGreen
 .StageData                    .StageData                   .HandleGreen                  .Categ*Cnt
   .MusicCateg          ─►       .MusicCateg          ─►      ┐                ─►          .Song*Cnt
   .IsPushed (NEW edge)  ─►       .IsPushed (NEW)      ─►      │                            .SongPushedCnt
   .IsFavorite                    .IsFavorite                  ├─ GreenProfileCounters
   .IsRecent                      .IsRecent                    │   .ApplyStage()         SongPlayDatumGreen
                                                               │                          .IsPushed (NEW col)
                                                               └─ SaveStageAsync          .MusicCategory
                                                                                          ...

UserDataResponse              CommonUserDataResponse       UserDataQuery                 UserSaveDataGreen
 .Categ*Cnt           ◄─        .Categ*Cnt           ◄─     .HandleGreen                  .Categ*Cnt
 .SongPushedCnt (was 0)         .SongPushedCnt (NEW set)   reads save-data columns        .SongPushedCnt
 .SongFavoriteCnt       ◄─      .SongFavoriteCnt       ◄─  (was list-length, now col)    .SongFavoriteCnt
 .SongRecentCnt         ◄─      .SongRecentCnt         ◄─                                  .SongRecentCnt
 .RecommendSong         ◄─      .RecommendSong         ◄─   reads IGreenCatalog.Recommend GreenRecommendEntry
 .RecommendBestSong     ◄─      .RecommendBestSong     ◄─                                  (recommend_songs.json)

RecommendResponse             CommonRecommendResponse      GetRecommendQuery
 .RecommendSong         ◄─      .RecommendSong         ◄─    reads IGreenCatalog.Recommend
 .RecommendBestSong     ◄─      .RecommendBestSong     ◄─

GreenRecentSongs              UserDataQuery.HandleGreen    UpdatePlayResultCommand.HandleGreen
 .LastPlayed (NEW col)  ◄─     OrderByDescending(LastPlayed) UpsertFavoriteAndRecent: upsert
                                .Take(10)                     for every stage, trim to 10
```

## Components

### 1. DTO + wire mapper

**`Application/Dtos/CommonPlayResultData.Green.cs`** — extend the `StageData` partial with `public bool IsPushed { get; set; }`. Green-only, so it lives in the Green partial, not the base.

**`Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`** — `MapStage` copies `stage.IsPushed`.

### 2. Schema + migration

**`Domain/Entities/SongPlayDatumGreen.cs`** — add `public bool IsPushed { get; set; }` next to `IsFavorite`/`IsRecent`/`IsPapamama`.

**`Domain/Entities/GreenRecentSongs.cs`** — add `public DateTime LastPlayed { get; set; }`.

**EF migration: `Infrastructure/Persistence/Migrations/<timestamp>_AddGreenSongCategoryFields.cs`**
- Add `IsPushed` column to `SongPlayDataGreen` (INTEGER NOT NULL DEFAULT 0).
- Add `LastPlayed` column to `GreenRecentSongs` (TEXT NOT NULL DEFAULT '1970-01-01 00:00:00' — `DateTime.UnixEpoch` so old rows sort to the bottom).
- No data backfill needed beyond the defaults.

Generated via:
```bash
dotnet ef migrations add AddGreenSongCategoryFields --project Infrastructure --startup-project Host
```

### 3. `GreenProfileCounters` helper

**`Application/Common/GreenProfileCounters.cs`** — pure static class.

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenProfileCounters
{
    public static void ApplyStage(UserSaveDataGreen saveData, CommonPlayResultData.StageData stage)
    {
        IncrementGenreCounter(saveData, stage.MusicCateg);
        if (stage.IsPushed)   saveData.SongPushedCnt   = SafeIncrement(saveData.SongPushedCnt);
        if (stage.IsFavorite) saveData.SongFavoriteCnt = SafeIncrement(saveData.SongFavoriteCnt);
        if (stage.IsRecent)   saveData.SongRecentCnt   = SafeIncrement(saveData.SongRecentCnt);
    }

    private static void IncrementGenreCounter(UserSaveDataGreen saveData, uint musicCateg)
    {
        switch (musicCateg)
        {
            case 0: saveData.CategJpopCnt     = SafeIncrement(saveData.CategJpopCnt); break;
            case 1: saveData.CategAnimeCnt    = SafeIncrement(saveData.CategAnimeCnt); break;
            case 2: saveData.CategDoyoCnt     = SafeIncrement(saveData.CategDoyoCnt); break;
            case 3: saveData.CategVocaloidCnt = SafeIncrement(saveData.CategVocaloidCnt); break;
            case 4: saveData.CategGameCnt     = SafeIncrement(saveData.CategGameCnt); break;
            case 5: saveData.CategNamcoCnt    = SafeIncrement(saveData.CategNamcoCnt); break;
            case 6: saveData.CategVarietyCnt  = SafeIncrement(saveData.CategVarietyCnt); break;
            case 7: saveData.CategClassicCnt  = SafeIncrement(saveData.CategClassicCnt); break;
        }
    }

    private static uint SafeIncrement(uint current)
        => current == uint.MaxValue ? current : current + 1;
}
```

`MusicCateg >= 8` falls through silently here because `IsValidGreenStage` already rejects the play upstream; the switch's `default` no-op is defensive.

### 4. Save handler

**`Application/Handlers/UpdatePlayResultCommand.Green.cs`:**

- `IsValidGreenStage`: append `&& stage.MusicCateg <= 7`.
- `HandleGreen` per-stage loop: add `GreenProfileCounters.ApplyStage(saveData, stage);` before `SaveStageAsync(...)`.
- `SaveStageAsync`: include `IsPushed = stage.IsPushed` in the `new SongPlayDatumGreen { ... }` initializer.
- `UpsertFavoriteAndRecentAsync` — restructure:

```csharp
private async Task UpsertFavoriteAndRecentAsync(
    uint baid,
    CommonPlayResultData.StageData stage,
    DateTime playTime,
    CancellationToken cancellationToken)
{
    // Favorites: cap at 5 inserts; remove still allowed.
    var favorite = await context.GreenFavoriteSongs.FindAsync([baid, stage.SongNo], cancellationToken);
    if (stage.IsFavorite && favorite is null)
    {
        var count = await context.GreenFavoriteSongs.CountAsync(s => s.Baid == baid, cancellationToken);
        if (count < 5)
        {
            context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = baid, SongNo = stage.SongNo });
        }
    }
    else if (!stage.IsFavorite && favorite is not null)
    {
        context.GreenFavoriteSongs.Remove(favorite);
    }

    // Recents: server-tracked. Upsert on every played stage; ignore stage.IsRecent for population.
    var recent = await context.GreenRecentSongs.FindAsync([baid, stage.SongNo], cancellationToken);
    if (recent is null)
    {
        context.GreenRecentSongs.Add(new GreenRecentSongs
        {
            Baid = baid,
            SongNo = stage.SongNo,
            LastPlayed = playTime
        });
    }
    else
    {
        recent.LastPlayed = playTime;
    }

    // Trim to 10 oldest after upsert.
    var overage = await context.GreenRecentSongs
        .Where(s => s.Baid == baid)
        .OrderByDescending(s => s.LastPlayed)
        .Skip(10)
        .ToListAsync(cancellationToken);
    context.GreenRecentSongs.RemoveRange(overage);
}
```

Pass `playTime` from the caller (`SaveStageAsync` already has it).

### 5. Recommend-songs catalog

**Operator-owned data file: `Host/wwwroot/data/green/recommend_songs.json`**
```json
{
  "recommendSong": 0,
  "recommendBestSongs": []
}
```
Default-installed as empty (= no pushed songs advertised; current behavior preserved on fresh installs). Add a `<None Include="wwwroot/data/green/recommend_songs.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>` block to `Host/Host.csproj` (or rely on an existing glob if present).

**`Application/Catalog/Green/GreenRecommendEntry.cs`:**
```csharp
namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenRecommendEntry
{
    public uint RecommendSong { get; init; }
    public IReadOnlyList<uint> RecommendBestSongs { get; init; } = [];

    public static GreenRecommendEntry Empty { get; } = new();
}
```

**`Infrastructure/GameDataCatalog/Green/GreenRecommendLoader.cs`** — mirrors the structure of `GreenItemShopLoader`/`GreenTelopLoader`. Reads the JSON via `System.Text.Json`, filters song IDs by `green.GreenMusicInfos.ContainsKey(...)` membership and `SongNo < GreenProtocolBytes.SongFlagBytes * 8`. Missing file → `GreenRecommendEntry.Empty`.

**`Application/Abstractions/IGreenCatalog.cs`** — add `GreenRecommendEntry Recommend { get; }`.

**`Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`** — load via the new loader during `InitializeAsync`.

**`Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`** — add `recommend_songs.json` to the required list if other Green data files are required, or omit if the file is optional with defaults. Match the convention of other Green loaders (verify during implementation; default to optional since empty is a valid state).

### 6. Load handlers

**`Application/Handlers/UserDataQuery.Green.cs`** — three changes:
```csharp
SongFavoriteCnt = saveData.SongFavoriteCnt,   // was: (uint)favorites.Length
SongRecentCnt   = saveData.SongRecentCnt,     // was: (uint)recent.Length
SongPushedCnt   = saveData.SongPushedCnt,     // NEW
RecommendSong       = green.Recommend.RecommendSong,
RecommendBestSong   = green.Recommend.RecommendBestSongs.ToList(),
```
And change the `recent` query ordering:
```csharp
var recent = await context.GreenRecentSongs
    .Where(song => song.Baid == request.Baid)
    .OrderByDescending(song => song.LastPlayed)   // was: SongNo
    .Select(song => song.SongNo)
    .Take(10)
    .ToArrayAsync(cancellationToken);
```
The `AryFavoriteSongNoes` / `AryRecentSongNoes` arrays still come from the `favorites` and `recent` queries — only the count fields move to the save-data columns.

**`Application/Handlers/GetRecommendQuery.Green.cs`** — replace the stub:
```csharp
public partial ValueTask<CommonRecommendResponse> Handle(GetRecommendQuery request, CancellationToken cancellationToken)
{
    var green = gameDataService.Green();
    return ValueTask.FromResult(new CommonRecommendResponse
    {
        Result = 1,
        RecommendSong = green.Recommend.RecommendSong,
        RecommendBestSong = green.Recommend.RecommendBestSongs.ToList()
    });
}
```
`gender_type` and `player_age` are ignored by design.

## Behavior matrix

| Scenario | Before | After |
|---|---|---|
| Play J-POP song (music_categ=0) | No counter moves | `CategJpopCnt` += 1 |
| Play song with `is_pushed=true` and pushed list configured | `SongPushedCnt` stays 0 (was never read either) | `SongPushedCnt` += 1, returned on userdata |
| Play song with `is_pushed=true` but no pushed list configured | n/a (client can't set is_pushed without server advertisement) | n/a |
| Play song with `music_categ=99` | Persists with bogus category, counter stays 0 | Whole play rejected (Result=0) |
| `recommend.php` request | Returns `Result=1`, no songs | Returns configured pushed-song catalog |
| 最近あそんだ曲 folder | List ordered by SongNo desc, populated only when `stage.IsRecent=true` | Ordered by play time desc, every play upserts, trimmed to 10 |
| 6th favorite via play-result write | Inserted unconditionally | Rejected silently (cap=5) |
| `userdata.php` `song_pushed_cnt` | Field never set → 0 default | Returned from save-data column |

## Tests

Under `Tests/Green/`:

- **`GreenProfileCountersTests.cs`** (NEW) — pure unit tests:
  - Each `music_categ` 0..7 increments the matching counter.
  - `IsPushed=true` / `IsFavorite=true` / `IsRecent=true` flag tests for each respective counter.
  - `uint.MaxValue` saturation does not wrap.
- **`GreenRecommendCatalogLoaderTests.cs`** (NEW):
  - Loads valid JSON.
  - Filters out-of-catalog song IDs.
  - Missing file → `GreenRecommendEntry.Empty`.
- **`GreenPlayResultHandlerTests.cs`** (extend):
  - `MusicCategOutOfRangeRejectsPlay`.
  - `EachStageIncrementsCategoryAndSongCounters` end-to-end through the handler.
  - `RecentListUpsertsForEveryStageAndOrdersByPlayTime`.
  - `RecentListTrimsToTenOldest`.
  - `FavoritesCapAtFiveRejectsSixthInsert`.
  - `FavoritesRemovalStillWorksAtCap` (regression — verify cap doesn't block deletes).
- **`GreenUserDataHandlerTests.cs`** (extend):
  - `UserDataReturnsRecommendSongsFromCatalog`.
  - `SongPushedCntComesFromSaveData`.
  - `SongFavoriteCntAndRecentCntComeFromSaveData` (not list length).
- **`GreenPlayResultMapperTests.cs`** (extend):
  - `MapStageCopiesIsPushed`.

## Migration / rollout

- Server applies the EF migration automatically on startup (existing behavior).
- Default `recommend_songs.json` ships with empty fields — no behavior change for operators who don't edit it.
- Existing rows: `IsPushed=false` on all historical plays, `LastPlayed=UnixEpoch` on all historical recents (sorts to the bottom of the folder until they get replayed). No backfill required.
- Existing counters (`Categ*Cnt`, `Song*Cnt`) start at 0 today and start incrementing from the first play after upgrade; no historical reconstruction from `SongPlayDatumGreen.MusicCategory` rows. Replay-from-log is a separate `LocalSaveModScoreMigrator`-style concern outside this scope.

## Risks

- **Recent-list semantics change is the riskiest piece.** Today the table only contains songs the client said `IsRecent=true` for; after this change, every played song goes in. If any other handler reads `GreenRecentSongs` expecting client-marked recents only, it will see different data. Audit before merging.
- **Counter saturation choice** (saturate at `uint.MaxValue` vs. reject the play). Saturating means once a counter hits the cap it silently freezes. Given 4 billion plays is unreachable, the choice is mostly aesthetic.
- **Favorites cap silent reject.** The client may already enforce 5 locally, so rejection should be unobservable. If the client allows 6+ locally for some reason, that 6th favorite simply won't persist server-side — a small drift, no crash.
