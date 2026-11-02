# AC15 Catalog Readback And Initial Data Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move shared catalog-backed readback for folders, telops, recommendations, item-shop info rows, and initial-data common fields into AC15 services.

**Architecture:** Build `Ac15CatalogSnapshot` adapters from existing Blue/Green catalogs, then have handlers call `Ac15CatalogReadbackService` and `Ac15InitialDataService`. Blue battle availability and Green ghost/AI flags stay in era-specific handler code or hooks.

**Tech Stack:** C# 13, .NET 10, Mediator handlers, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15CatalogSnapshot.cs` - canonical catalog data used by readback services.
- `Application/Ac15/Ac15CatalogSnapshotFactory.cs` - Blue/Green catalog-to-snapshot mapping.
- `Application/Ac15/Ac15CatalogReadbackService.cs` - folders, telops, recommendations, and item-shop info rows.
- `Application/Ac15/Ac15InitialDataService.cs` - common initial-data response builder.
- `Tests/Ac15/Ac15CatalogReadbackServiceTests.cs`
- `Tests/Ac15/Ac15InitialDataServiceTests.cs`

Modify:

- `Application/Handlers/GetFolderQuery.Blue.cs`
- `Application/Handlers/GetFolderQuery.Green.cs`
- `Application/Handlers/GetTelopQuery.Blue.cs`
- `Application/Handlers/GetTelopQuery.Green.cs`
- `Application/Handlers/GetItemShopInfoQuery.Blue.cs`
- `Application/Handlers/GetItemShopInfoQuery.Green.cs`
- `Application/Handlers/GetInitialDataQuery.Blue.cs`
- `Application/Handlers/GetInitialDataQuery.Green.cs`
- `Application/Handlers/GetRecommendQuery.cs` and Green/Blue controller routing only if the handler is wired during this stage.

## Task 1: Catalog Snapshot And Readback Service

**Files:**
- Create: `Tests/Ac15/Ac15CatalogReadbackServiceTests.cs`
- Create: `Application/Ac15/Ac15CatalogSnapshot.cs`
- Create: `Application/Ac15/Ac15CatalogSnapshotFactory.cs`
- Create: `Application/Ac15/Ac15CatalogReadbackService.cs`

- [ ] **Step 1: Write failing catalog readback tests**

Create `Tests/Ac15/Ac15CatalogReadbackServiceTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CatalogReadbackServiceTests
{
    [Fact]
    public void BuildFolderResponse_ReturnsOnlyRequestedKnownFolders()
    {
        var snapshot = Snapshot();

        var response = Ac15CatalogReadbackService.BuildFolderResponse(snapshot, [2, 99, 1]);

        Assert.Equal(1u, response.Result);
        Assert.Equal([2u, 1u], response.AryEventfolderDatas.Select(row => row.FolderId));
    }

    [Fact]
    public void BuildTelopResponse_ReturnsEmptySuccessWhenMissing()
    {
        var response = Ac15CatalogReadbackService.BuildTelopResponse(Snapshot(), 99);

        Assert.Equal(1u, response.Result);
        Assert.Null(response.VerupNo);
        Assert.Null(response.Telop);
    }

    [Fact]
    public void BuildItemShopInfo_ReturnsRowsOrderedByItemNoWhenEnabled()
    {
        var response = Ac15CatalogReadbackService.BuildItemShopInfo(Snapshot());

        Assert.Equal(1u, response.Result);
        Assert.Equal(7u, response.SeasonId);
        Assert.Equal([1u, 2u], response.AryItemshopData.Select(row => row.ItemNo));
    }

    private static Ac15CatalogSnapshot Snapshot() => new(
        SongHashVersion: 456,
        SongNoesInFileOrder: [101, 102, 103],
        EventFolders: new Dictionary<uint, EventFolderData>
        {
            [1] = new() { FolderId = 1, VerupNo = 8, SongNoes = [101] },
            [2] = new() { FolderId = 2, VerupNo = 9, SongNoes = [102] }
        },
        Telops: new Dictionary<uint, Ac15TelopEntry>
        {
            [5] = new() { TelopId = 5, VerupNo = 10, StartDatetime = "20260101000000", EndDatetime = "20261231235959", Message = "hello" }
        },
        RecommendSong: 101,
        RecommendBestSongs: [102, 103],
        ItemShopCatalog: new Ac15ItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 7,
            Seasons = new Dictionary<uint, Ac15ItemShopSeason>
            {
                [7] = new()
                {
                    SeasonId = 7,
                    VerupNo = 70,
                    Telop = "shop",
                    StartDatetime = "20260101000000",
                    EndDatetime = "20261231235959",
                    AfterstartDays = 1,
                    BeforecloseDays = 2,
                    Items =
                    [
                        new() { ItemNo = 2, ItemType = Ac15ShopItemType.Tone, ItemId = 44, Price = 100 },
                        new() { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 102, Price = 200 }
                    ]
                }
            }
        },
        TaikojukuPacks: []);
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CatalogReadbackServiceTests"
```

Expected: compile failure for missing `Ac15CatalogSnapshot` and `Ac15CatalogReadbackService`.

- [ ] **Step 3: Add snapshot and readback service**

Create `Application/Ac15/Ac15CatalogSnapshot.cs`:

```csharp
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15CatalogSnapshot(
    uint SongHashVersion,
    IReadOnlyList<uint> SongNoesInFileOrder,
    IReadOnlyDictionary<uint, EventFolderData> EventFolders,
    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops,
    uint RecommendSong,
    IReadOnlyList<uint> RecommendBestSongs,
    Ac15ItemShopCatalog ItemShopCatalog,
    IReadOnlyList<Ac15TaikojukuEntry> TaikojukuPacks);
```

Create `Application/Ac15/Ac15CatalogReadbackService.cs`:

```csharp
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CatalogReadbackService
{
    public static CommonGetFolderResponse BuildFolderResponse(
        Ac15CatalogSnapshot snapshot,
        IEnumerable<uint> requestedFolderIds)
    {
        var response = new CommonGetFolderResponse { Result = 1 };
        foreach (var folderId in requestedFolderIds)
        {
            if (snapshot.EventFolders.TryGetValue(folderId, out var folderData))
            {
                response.AryEventfolderDatas.Add(folderData);
            }
        }

        return response;
    }

    public static CommonGetTelopResponse BuildTelopResponse(Ac15CatalogSnapshot snapshot, uint telopId)
    {
        if (!snapshot.Telops.TryGetValue(telopId, out var entry))
        {
            return new CommonGetTelopResponse { Result = 1 };
        }

        return new CommonGetTelopResponse
        {
            Result = 1,
            VerupNo = entry.VerupNo,
            StartDatetime = entry.StartDatetime,
            EndDatetime = entry.EndDatetime,
            Telop = entry.Message
        };
    }

    public static CommonRecommendResponse BuildRecommendResponse(Ac15CatalogSnapshot snapshot)
        => new()
        {
            Result = 1,
            RecommendSong = snapshot.RecommendSong,
            RecommendBestSong = snapshot.RecommendBestSongs.ToList()
        };

    public static CommonItemShopInfoResponse BuildItemShopInfo(Ac15CatalogSnapshot snapshot)
    {
        var catalog = snapshot.ItemShopCatalog;
        var season = catalog.ActiveSeason;
        if (!catalog.IsEnabled || season is null || season.Items.Count == 0)
        {
            return new CommonItemShopInfoResponse { Result = 1 };
        }

        return new CommonItemShopInfoResponse
        {
            Result = 1,
            VerupNo = season.VerupNo,
            SeasonId = season.SeasonId,
            Telop = season.Telop,
            StartDatetime = season.StartDatetime,
            EndDatetime = season.EndDatetime,
            AfterstartDays = season.AfterstartDays,
            BeforecloseDays = season.BeforecloseDays,
            AryItemshopData = season.Items
                .OrderBy(item => item.ItemNo)
                .Select(item => new CommonItemShopInfoResponse.ItemShopData
                {
                    ItemNo = item.ItemNo,
                    ItemType = item.ItemType.ToProtocolValue(),
                    ItemId = item.ItemId,
                    ItemPrice = item.Price
                })
                .ToList()
        };
    }
}
```

- [ ] **Step 4: Add Blue/Green snapshot factory**

Create `Application/Ac15/Ac15CatalogSnapshotFactory.cs` with Blue and Green mapping helpers. The factory must not reference generated `Wire` classes.

```csharp
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CatalogSnapshotFactory
{
    public static Ac15CatalogSnapshot FromBlue(IBlueCatalog blue) => new(
        blue.SongHashVersion,
        blue.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
        blue.EventFolders,
        blue.Telops.ToDictionary(pair => pair.Key, pair => MapTelop(pair.Value)),
        blue.Recommend.RecommendSong,
        blue.Recommend.RecommendBestSongs.ToArray(),
        MapItemShop(blue.ItemShopCatalog),
        blue.TaikojukuFileOrder.Select(MapTaikojuku).ToArray());

    public static Ac15CatalogSnapshot FromGreen(IGreenCatalog green) => new(
        green.SongHashVersion,
        green.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
        green.EventFolders,
        green.Telops.ToDictionary(pair => pair.Key, pair => MapTelop(pair.Value)),
        green.Recommend.RecommendSong,
        green.Recommend.RecommendBestSongs.ToArray(),
        MapItemShop(green.ItemShopCatalog),
        green.TaikojukuFileOrder.Select(MapTaikojuku).ToArray());

    private static Ac15TelopEntry MapTelop(BlueTelopEntry entry) => new()
    {
        TelopId = entry.TelopId,
        VerupNo = entry.VerupNo,
        StartDatetime = entry.StartDatetime,
        EndDatetime = entry.EndDatetime,
        Message = entry.Message
    };

    private static Ac15TelopEntry MapTelop(GreenTelopEntry entry) => new()
    {
        TelopId = entry.TelopId,
        VerupNo = entry.VerupNo,
        StartDatetime = entry.StartDatetime,
        EndDatetime = entry.EndDatetime,
        Message = entry.Message
    };

    private static Ac15ItemShopCatalog MapItemShop(BlueItemShopCatalog catalog)
        => catalog.IsEnabled
            ? new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = catalog.ActiveSeasonId,
                Seasons = catalog.Seasons.ToDictionary(pair => pair.Key, pair => MapSeason(pair.Value))
            }
            : Ac15ItemShopCatalog.Disabled;

    private static Ac15ItemShopCatalog MapItemShop(GreenItemShopCatalog catalog)
        => catalog.IsEnabled
            ? new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = catalog.ActiveSeasonId,
                Seasons = catalog.Seasons.ToDictionary(pair => pair.Key, pair => MapSeason(pair.Value))
            }
            : Ac15ItemShopCatalog.Disabled;

    private static Ac15ItemShopSeason MapSeason(BlueItemShopSeason season) => new()
    {
        SeasonId = season.SeasonId,
        VerupNo = season.VerupNo,
        Telop = season.Telop,
        StartDatetime = season.StartDatetime,
        EndDatetime = season.EndDatetime,
        AfterstartDays = season.AfterstartDays,
        BeforecloseDays = season.BeforecloseDays,
        Items = season.Items.Select(item => new Ac15ItemShopEntry
        {
            ItemNo = item.ItemNo,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            Price = item.Price
        }).ToArray()
    };

    private static Ac15ItemShopSeason MapSeason(GreenItemShopSeason season) => new()
    {
        SeasonId = season.SeasonId,
        VerupNo = season.VerupNo,
        Telop = season.Telop,
        StartDatetime = season.StartDatetime,
        EndDatetime = season.EndDatetime,
        AfterstartDays = season.AfterstartDays,
        BeforecloseDays = season.BeforecloseDays,
        Items = season.Items.Select(item => new Ac15ItemShopEntry
        {
            ItemNo = item.ItemNo,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            Price = item.Price
        }).ToArray()
    };

    private static Ac15TaikojukuEntry MapTaikojuku(BlueTaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Songs = entry.Songs.Select(song => new Ac15TaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };

    private static Ac15TaikojukuEntry MapTaikojuku(GreenTaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Songs = entry.Songs.Select(song => new Ac15TaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };
}
```

- [ ] **Step 5: Run catalog readback tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CatalogReadbackServiceTests"
```

Expected: PASS.

## Task 2: Initial Data Service

**Files:**
- Create: `Tests/Ac15/Ac15InitialDataServiceTests.cs`
- Create: `Application/Ac15/Ac15InitialDataService.cs`
- Modify: `Application/Handlers/GetInitialDataQuery.Blue.cs`
- Modify: `Application/Handlers/GetInitialDataQuery.Green.cs`

- [ ] **Step 1: Write failing initial-data service tests**

Create `Tests/Ac15/Ac15InitialDataServiceTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15InitialDataServiceTests
{
    [Fact]
    public void BuildCommonInitialData_HidesActiveShopSongsFromDefaultSongFlags()
    {
        var snapshot = new Ac15CatalogSnapshot(
            SongHashVersion: 456,
            SongNoesInFileOrder: [101, 102],
            EventFolders: new Dictionary<uint, EventFolderData>(),
            Telops: new Dictionary<uint, Ac15TelopEntry>(),
            RecommendSong: 101,
            RecommendBestSongs: [],
            ItemShopCatalog: new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = 1,
                Seasons = new Dictionary<uint, Ac15ItemShopSeason>
                {
                    [1] = new()
                    {
                        SeasonId = 1,
                        VerupNo = 2,
                        Items = [new() { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 102, Price = 10 }]
                    }
                }
            },
            TaikojukuPacks: []);

        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Green);

        Assert.Equal(1u, response.Result);
        Assert.Equal(456u, response.SongHashVer);
        Assert.True(response.IsDanplay);
        Assert.True(response.IsItemshop);
        Assert.True((response.DefaultSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.DefaultSongFlg[102 >> 3] & (1 << (102 & 7))) == 0);
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15InitialDataServiceTests"
```

Expected: compile failure because `Ac15InitialDataService` does not exist.

- [ ] **Step 3: Add initial-data service**

Create `Application/Ac15/Ac15InitialDataService.cs`:

```csharp
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15InitialDataService
{
    public static CommonInitialDataCheckResponse BuildCommonInitialData(
        Ac15CatalogSnapshot snapshot,
        Ac15EraProfile profile)
    {
        var activeShop = snapshot.ItemShopCatalog.ActiveSeason;
        var activeShopWithRows = snapshot.ItemShopCatalog.IsEnabled && activeShop is { Items.Count: > 0 }
            ? activeShop
            : null;
        var shopSongIds = activeShopWithRows is not null
            ? activeShopWithRows.Items.Where(item => item.ItemType == Ac15ShopItemType.Song).Select(item => item.ItemId).ToHashSet()
            : [];
        var defaultSongNoes = snapshot.SongNoesInFileOrder.Where(songNo => !shopSongIds.Contains(songNo));

        var response = new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = Ac15ProtocolBytes.CreateFixedBitset(defaultSongNoes, profile.Limits.SongFlagBytes),
            AchievementSongBit = new byte[profile.Limits.SongFlagBytes],
            UraReleaseBit = new byte[profile.Limits.SongFlagBytes],
            SongHashVer = snapshot.SongHashVersion,
            IsDanplay = profile.Features.Dani,
            IsClose = false,
            IsItemshop = profile.Features.ItemShop && activeShopWithRows is not null,
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        return response;
    }

    public static List<CommonInitialDataCheckResponse.InformationData> BuildItemShopInfoRows(Ac15CatalogSnapshot snapshot)
    {
        var activeShop = snapshot.ItemShopCatalog.ActiveSeason;
        return activeShop is { Items.Count: > 0 }
            ?
            [
                new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = activeShop.SeasonId,
                    VerupNo = activeShop.VerupNo
                }
            ]
            : [];
    }

    public static List<CommonInitialDataCheckResponse.InformationData> BuildTelopInfoRows(Ac15CatalogSnapshot snapshot)
        => snapshot.Telops.Values
            .OrderBy(entry => entry.TelopId)
            .Select(entry => new CommonInitialDataCheckResponse.InformationData
            {
                InfoId = entry.TelopId,
                VerupNo = entry.VerupNo
            })
            .ToList();

    public static List<CommonInitialDataCheckResponse.InformationData> BuildEventFolderInfoRows(Ac15CatalogSnapshot snapshot)
        => snapshot.EventFolders.Values
            .Select(entry => new CommonInitialDataCheckResponse.InformationData
            {
                InfoId = entry.FolderId,
                VerupNo = entry.VerupNo
            })
            .ToList();

    public static List<CommonInitialDataCheckResponse.InformationData> BuildTaikojukuInfoRows(
        Ac15CatalogSnapshot snapshot,
        Ac15EraProfile profile,
        Func<uint, uint, uint> verupSelector)
        => snapshot.TaikojukuPacks
            .Where(entry => entry.ChallengeLevel >= profile.Limits.MinNormalDanId
                && entry.ChallengeLevel <= profile.Limits.MaxNormalDanId)
            .Select(entry => new CommonInitialDataCheckResponse.InformationData
            {
                InfoId = entry.ChallengeLevel,
                VerupNo = verupSelector(entry.ChallengeLevel, entry.VerupNo)
            })
            .ToList();
}
```

- [ ] **Step 4: Update Blue and Green initial-data handlers**

In Blue, build a snapshot from `gameDataService.Blue()`, call `BuildCommonInitialData`, then add Blue-specific fields:

```csharp
var blue = gameDataService.Blue();
var snapshot = Ac15CatalogSnapshotFactory.FromBlue(blue);
var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Blue);
var battle = blue.BattleCatalog;

response.IsBattleplay = battle.EnablesBattleAdvertisement;
response.ReleaseBattleStageFlg = battle.EnablesBattleAdvertisement
    ? BlueProtocolBytes.CreateFixedBitset(battle.ReleaseBattleStageIds, BlueProtocolBytes.BattleStageFlagBytes)
    : new byte[BlueProtocolBytes.BattleStageFlagBytes];
response.ReleaseBattleSpecialFlg = battle.EnablesBattleAdvertisement
    ? BlueProtocolBytes.CreateBattleSpecialBitset(battle.ReleaseBattleSpecialIds)
    : new byte[BlueProtocolBytes.BattleSpecialFlagBytes];
response.BattleBondsLvCap = battle.EnablesBattleAdvertisement ? battle.BattleBondsLvCap ?? 0 : 0;
response.AryBlueItemShopDatas = Ac15InitialDataService.BuildItemShopInfoRows(snapshot);
response.AryBlueTelopDatas = Ac15InitialDataService.BuildTelopInfoRows(snapshot);
response.AryBlueEventFolderDatas = Ac15InitialDataService.BuildEventFolderInfoRows(snapshot);
response.AryBlueTaikojukuDatas = Ac15InitialDataService.BuildTaikojukuInfoRows(snapshot, Ac15EraProfiles.Blue, (_, verupNo) => verupNo == 0 ? 3 : verupNo);
response.AryBlueLegaltermsDatas = [];
return ValueTask.FromResult(response);
```

In Green, build a snapshot from `gameDataService.Green()`, call `BuildCommonInitialData`, then add Green fields:

```csharp
var green = gameDataService.Green();
var snapshot = Ac15CatalogSnapshotFactory.FromGreen(green);
var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Green);

response.IsGhostbattleplay = true;
response.AryGreenItemShopDatas = Ac15InitialDataService.BuildItemShopInfoRows(snapshot);
response.AryGreenTelopDatas = Ac15InitialDataService.BuildTelopInfoRows(snapshot);
response.AryGreenEventFolderDatas = Ac15InitialDataService.BuildEventFolderInfoRows(snapshot);
response.AryGreenTaikojukuDatas = Ac15InitialDataService.BuildTaikojukuInfoRows(snapshot, Ac15EraProfiles.Green, (_, verupNo) => verupNo + 1);
return ValueTask.FromResult(response);
```

Add `using TaikoLocalServer.Application.Ac15;` to both handlers.

- [ ] **Step 5: Run initial-data tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15InitialDataServiceTests|FullyQualifiedName~BlueInitialDataTests|FullyQualifiedName~GreenTaikojukuTests|FullyQualifiedName~GreenItemShopProtocolTests"
```

Expected: PASS.

## Task 3: Handler Readback Migration

**Files:**
- Modify: `Application/Handlers/GetFolderQuery.Blue.cs`
- Modify: `Application/Handlers/GetFolderQuery.Green.cs`
- Modify: `Application/Handlers/GetTelopQuery.Blue.cs`
- Modify: `Application/Handlers/GetTelopQuery.Green.cs`
- Modify: `Application/Handlers/GetItemShopInfoQuery.Blue.cs`
- Modify: `Application/Handlers/GetItemShopInfoQuery.Green.cs`

- [ ] **Step 1: Update folder and telop handlers**

Replace Blue folder handler body with:

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromBlue(gameDataService.Blue());
return ValueTask.FromResult(Ac15CatalogReadbackService.BuildFolderResponse(snapshot, request.FolderIds));
```

Replace Green folder handler body with:

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromGreen(gameDataService.Green());
return ValueTask.FromResult(Ac15CatalogReadbackService.BuildFolderResponse(snapshot, request.FolderIds));
```

Replace Blue telop handler body with:

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromBlue(gameDataService.Blue());
return ValueTask.FromResult(Ac15CatalogReadbackService.BuildTelopResponse(snapshot, request.TelopId));
```

Replace Green telop handler body with:

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromGreen(gameDataService.Green());
return ValueTask.FromResult(Ac15CatalogReadbackService.BuildTelopResponse(snapshot, request.TelopId));
```

Add `using TaikoLocalServer.Application.Ac15;` to all four files.

- [ ] **Step 2: Update item-shop info handlers**

Replace Blue item-shop info body with:

```csharp
cancellationToken.ThrowIfCancellationRequested();
var snapshot = Ac15CatalogSnapshotFactory.FromBlue(gameDataService.Blue());
if (!snapshot.ItemShopCatalog.IsEnabled || snapshot.ItemShopCatalog.ActiveSeason is null)
{
    logger.LogInformation("Blue GetItemShopInfo returning empty because item shop is disabled or inactive");
}

return ValueTask.FromResult(Ac15CatalogReadbackService.BuildItemShopInfo(snapshot));
```

Replace Green item-shop info body with:

```csharp
cancellationToken.ThrowIfCancellationRequested();
var snapshot = Ac15CatalogSnapshotFactory.FromGreen(gameDataService.Green());
if (!snapshot.ItemShopCatalog.IsEnabled || snapshot.ItemShopCatalog.ActiveSeason is null)
{
    logger.LogInformation("Green GetItemShopInfo returning empty because item shop is disabled");
}

return ValueTask.FromResult(Ac15CatalogReadbackService.BuildItemShopInfo(snapshot));
```

- [ ] **Step 3: Run catalog-backed handler tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueInitialDataTests|FullyQualifiedName~BlueItemShopProtocolTests|FullyQualifiedName~BlueRouteSkeletonTests|FullyQualifiedName~GreenEventFolder|FullyQualifiedName~GreenTelop|FullyQualifiedName~GreenItemShopProtocolTests|FullyQualifiedName~GreenTaikojukuTests"
```

Expected: PASS.

- [ ] **Step 4: Commit stage 3**

Run:

```powershell
git add Application/Ac15 Application/Handlers/GetFolderQuery.Blue.cs Application/Handlers/GetFolderQuery.Green.cs Application/Handlers/GetTelopQuery.Blue.cs Application/Handlers/GetTelopQuery.Green.cs Application/Handlers/GetItemShopInfoQuery.Blue.cs Application/Handlers/GetItemShopInfoQuery.Green.cs Application/Handlers/GetInitialDataQuery.Blue.cs Application/Handlers/GetInitialDataQuery.Green.cs Tests/Ac15
git commit -m "Extract AC15 catalog readback services"
```

