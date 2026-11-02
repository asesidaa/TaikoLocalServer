# AC15 Userdata Core And Era Adapters Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build canonical AC15 userdata composition through typed Blue and Green adapters while preserving separate save tables and era-only fields.

**Architecture:** Add read-only userdata snapshot records and era adapter factories. `Ac15UserDataService` builds the shared `CommonUserDataResponse`, then Blue/Green handlers add proven extras such as Blue Tokkun tutorial readback and Green `IsDevilGreen`.

**Tech Stack:** C# 13, .NET 10, EF Core queries through existing handlers, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15UserDataRecords.cs` - canonical save snapshot, shop lock snapshot, and profile counters.
- `Application/Ac15/Ac15UserDataService.cs` - shared userdata response builder.
- `Application/Ac15/BlueAc15UserDataAdapter.cs` - maps `UserSaveDataBlue`, favorites, recent, Dan grades, and shop locks.
- `Application/Ac15/GreenAc15UserDataAdapter.cs` - maps `UserSaveDataGreen`, favorites, recent, Dan grades, and shop locks.
- `Tests/Ac15/Ac15UserDataServiceTests.cs`

Modify:

- `Application/Handlers/UserDataQuery.Blue.cs`
- `Application/Handlers/UserDataQuery.Green.cs`
- Existing Blue/Green userdata tests only when they need new expected helper names.

## Task 1: Userdata Service Contract

**Files:**
- Create: `Tests/Ac15/Ac15UserDataServiceTests.cs`
- Create: `Application/Ac15/Ac15UserDataRecords.cs`
- Create: `Application/Ac15/Ac15UserDataService.cs`

- [ ] **Step 1: Write failing userdata service tests**

Create `Tests/Ac15/Ac15UserDataServiceTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15UserDataServiceTests
{
    [Fact]
    public void BuildResponse_OrsCatalogAndSaveReleaseFlagsThenLocksShopSongs()
    {
        var response = Ac15UserDataService.BuildResponse(
            new Ac15UserDataSnapshot(
                SongHashVersion: 456,
                CatalogReleaseSongNoes: [101],
                SaveReleaseSongFlg: Ac15ProtocolBytes.CreateFixedBitset([102], 128),
                ToneFlg: Ac15ProtocolBytes.CreateFixedBitset([1], 16),
                TitleFlg: [],
                DefaultOptionSetting: [],
                OptionFlg: [],
                Favorites: [101],
                Recent: [102],
                RecommendSong: 101,
                RecommendBestSongs: [102],
                Counters: new Ac15ProfileCounters { SongFavoriteCnt = 1 },
                DisplayDan: 1,
                LockedSongIds: [102],
                LockedToneIds: [],
                TokkunTutorialFlg: null,
                IsDevil: false),
            Ac15EraProfiles.Blue);

        Assert.Equal(1u, response.Result);
        Assert.Equal(456u, response.SongHashVer);
        Assert.True((response.ReleaseSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.ReleaseSongFlg[102 >> 3] & (1 << (102 & 7))) == 0);
        Assert.Equal([101u], response.AryFavoriteSongNoes);
        Assert.Equal([102u], response.AryRecentSongNoes);
        Assert.Equal(1u, response.SongFavoriteCnt);
    }

    [Fact]
    public void BuildResponse_AddsTokkunTutorialOnlyWhenProfilePlacesItInUserdata()
    {
        var snapshot = MinimalSnapshot() with { TokkunTutorialFlg = 9 };

        var blue = Ac15UserDataService.BuildResponse(snapshot, Ac15EraProfiles.Blue);
        var green = Ac15UserDataService.BuildResponse(snapshot, Ac15EraProfiles.Green);

        Assert.Equal(9u, blue.TokkunTutorialFlg);
        Assert.Null(green.TokkunTutorialFlg);
    }

    private static Ac15UserDataSnapshot MinimalSnapshot() => new(
        SongHashVersion: 1,
        CatalogReleaseSongNoes: [],
        SaveReleaseSongFlg: [],
        ToneFlg: [],
        TitleFlg: [],
        DefaultOptionSetting: [],
        OptionFlg: [],
        Favorites: [],
        Recent: [],
        RecommendSong: 0,
        RecommendBestSongs: [],
        Counters: new Ac15ProfileCounters(),
        DisplayDan: 1,
        LockedSongIds: [],
        LockedToneIds: [],
        TokkunTutorialFlg: null,
        IsDevil: false);
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15UserDataServiceTests"
```

Expected: compile failure because userdata records and service do not exist.

- [ ] **Step 3: Add userdata records**

Create `Application/Ac15/Ac15UserDataRecords.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15UserDataSnapshot(
    uint SongHashVersion,
    IReadOnlyList<uint> CatalogReleaseSongNoes,
    byte[] SaveReleaseSongFlg,
    byte[] ToneFlg,
    byte[] TitleFlg,
    byte[] DefaultOptionSetting,
    byte[] OptionFlg,
    IReadOnlyList<uint> Favorites,
    IReadOnlyList<uint> Recent,
    uint RecommendSong,
    IReadOnlyList<uint> RecommendBestSongs,
    Ac15ProfileCounters Counters,
    uint DisplayDan,
    IReadOnlyList<uint> LockedSongIds,
    IReadOnlyList<uint> LockedToneIds,
    uint? TokkunTutorialFlg,
    bool IsDevil);

public sealed class Ac15ProfileCounters
{
    public uint CategJpopCnt { get; init; }
    public uint CategAnimeCnt { get; init; }
    public uint CategDoyoCnt { get; init; }
    public uint CategVarietyCnt { get; init; }
    public uint CategClassicCnt { get; init; }
    public uint CategGameCnt { get; init; }
    public uint CategNamcoCnt { get; init; }
    public uint CategVocaloidCnt { get; init; }
    public uint SongPushedCnt { get; init; }
    public uint SongFavoriteCnt { get; init; }
    public uint SongRecentCnt { get; init; }
    public uint TotalCreditCnt { get; init; }
    public uint PrevAreaCode { get; init; }
    public uint ConsecAreaCnt { get; init; }
    public bool DefaultShinSetting { get; init; }
    public uint DispLevelTotal { get; init; }
    public uint DispLevelChassis { get; init; }
    public uint DispLevelSelf { get; init; }
    public uint DifficultyPlayedCourse { get; init; }
    public uint DifficultyPlayedStar { get; init; }
    public bool IsChallengeCompe { get; init; }
    public bool IsTojiru { get; init; }
}
```

- [ ] **Step 4: Add userdata service**

Create `Application/Ac15/Ac15UserDataService.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class Ac15UserDataService
{
    public static CommonUserDataResponse BuildResponse(
        Ac15UserDataSnapshot snapshot,
        Ac15EraProfile profile)
    {
        var release = Ac15ProtocolBytes.OrBitsets(
            Ac15ProtocolBytes.CreateFixedBitset(snapshot.CatalogReleaseSongNoes, profile.Limits.SongFlagBytes),
            snapshot.SaveReleaseSongFlg,
            profile.Limits.SongFlagBytes);
        release = ClearBits(release, snapshot.LockedSongIds, profile.Limits.SongFlagBytes);

        var response = new CommonUserDataResponse
        {
            Result = 1,
            SongHashVer = snapshot.SongHashVersion,
            ReleaseSongFlg = release,
            ToneFlg = ClearBits(snapshot.ToneFlg, snapshot.LockedToneIds, profile.Limits.ToneFlagBytes),
            TitleFlg = Ac15ProtocolBytes.FixedOrZero(snapshot.TitleFlg, profile.Limits.TitleFlagBytes),
            DefaultOptionSetting = Ac15ProtocolBytes.FixedOrZero(snapshot.DefaultOptionSetting, 2),
            OptionFlg = snapshot.OptionFlg,
            AryFavoriteSongNoes = snapshot.Favorites.ToArray(),
            AryRecentSongNoes = snapshot.Recent.ToArray(),
            RecommendSong = snapshot.RecommendSong,
            RecommendBestSong = snapshot.RecommendBestSongs.ToList(),
            CategJpopCnt = snapshot.Counters.CategJpopCnt,
            CategAnimeCnt = snapshot.Counters.CategAnimeCnt,
            CategDoyoCnt = snapshot.Counters.CategDoyoCnt,
            CategVarietyCnt = snapshot.Counters.CategVarietyCnt,
            CategClassicCnt = snapshot.Counters.CategClassicCnt,
            CategGameCnt = snapshot.Counters.CategGameCnt,
            CategNamcoCnt = snapshot.Counters.CategNamcoCnt,
            CategVocaloidCnt = snapshot.Counters.CategVocaloidCnt,
            SongPushedCnt = snapshot.Counters.SongPushedCnt,
            SongFavoriteCnt = snapshot.Counters.SongFavoriteCnt,
            SongRecentCnt = snapshot.Counters.SongRecentCnt,
            TotalCreditCnt = snapshot.Counters.TotalCreditCnt,
            PrevAreaCode = snapshot.Counters.PrevAreaCode,
            ConsecAreaCnt = snapshot.Counters.ConsecAreaCnt,
            DefaultShinSetting = snapshot.Counters.DefaultShinSetting,
            DispLevelTotal = snapshot.Counters.DispLevelTotal,
            DispLevelChassis = snapshot.Counters.DispLevelChassis,
            DispLevelSelf = snapshot.Counters.DispLevelSelf,
            DispTaikojukuDan = GetSafeDisplayDan(snapshot.DisplayDan, profile),
            DifficultyPlayedCourse = snapshot.Counters.DifficultyPlayedCourse,
            DifficultyPlayedStar = snapshot.Counters.DifficultyPlayedStar,
            IsChallengeCompe = snapshot.Counters.IsChallengeCompe,
            IsTojiru = snapshot.Counters.IsTojiru
        };

        if (profile.WirePlacement.HasTokkunTutorialFlagInUserData)
        {
            response.TokkunTutorialFlg = snapshot.TokkunTutorialFlg;
        }

        return response;
    }

    private static byte[] ClearBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = Ac15ProtocolBytes.FixedOrZero(source, byteCount);
        var maxBits = byteCount * 8;
        foreach (var id in ids)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] &= (byte)~(1 << ((int)id & 7));
        }

        return result;
    }

    private static uint GetSafeDisplayDan(uint value, Ac15EraProfile profile)
        => value >= profile.Limits.MinNormalDanId && value <= profile.Limits.MaxNormalDanId
            ? value
            : profile.Limits.SafeDisplayDanFallback;
}
```

- [ ] **Step 5: Run userdata core tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15UserDataServiceTests"
```

Expected: PASS.

## Task 2: Blue And Green Userdata Handler Migration

**Files:**
- Create: `Application/Ac15/BlueAc15UserDataAdapter.cs`
- Create: `Application/Ac15/GreenAc15UserDataAdapter.cs`
- Modify: `Application/Handlers/UserDataQuery.Blue.cs`
- Modify: `Application/Handlers/UserDataQuery.Green.cs`

- [ ] **Step 1: Add adapter factories**

Create `Application/Ac15/BlueAc15UserDataAdapter.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class BlueAc15UserDataAdapter
{
    public static Ac15UserDataSnapshot CreateSnapshot(
        UserSaveDataBlue saveData,
        Ac15CatalogSnapshot catalog,
        IReadOnlyList<uint> favorites,
        IReadOnlyList<uint> recent,
        IReadOnlyCollection<(uint ItemType, uint ItemId)> unlockedShopItems)
    {
        var lockedSongIds = LockedIds(catalog, Ac15ShopItemType.Song, unlockedShopItems).ToArray();
        var lockedToneIds = LockedIds(catalog, Ac15ShopItemType.Tone, unlockedShopItems).ToArray();

        return new Ac15UserDataSnapshot(
            catalog.SongHashVersion,
            catalog.SongNoesInFileOrder,
            saveData.ReleaseSongFlg,
            saveData.ToneFlg,
            saveData.TitleFlg,
            saveData.DefaultOptionSetting,
            saveData.OptionFlg,
            favorites,
            recent,
            catalog.RecommendSong,
            catalog.RecommendBestSongs,
            Counters(saveData),
            saveData.DispTaikojukuDan,
            lockedSongIds,
            lockedToneIds,
            saveData.TokkunTutorialFlg,
            saveData.IsDevil);
    }

    private static IEnumerable<uint> LockedIds(
        Ac15CatalogSnapshot catalog,
        Ac15ShopItemType itemType,
        IReadOnlyCollection<(uint ItemType, uint ItemId)> unlockedShopItems)
        => catalog.ItemShopCatalog.ActiveSeason?.Items
            .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType.ToProtocolValue(), item.ItemId)))
            .Select(item => item.ItemId) ?? [];

    private static Ac15ProfileCounters Counters(UserSaveDataBlue saveData) => new()
    {
        CategJpopCnt = saveData.CategJpopCnt,
        CategAnimeCnt = saveData.CategAnimeCnt,
        CategDoyoCnt = saveData.CategDoyoCnt,
        CategVarietyCnt = saveData.CategVarietyCnt,
        CategClassicCnt = saveData.CategClassicCnt,
        CategGameCnt = saveData.CategGameCnt,
        CategNamcoCnt = saveData.CategNamcoCnt,
        CategVocaloidCnt = saveData.CategVocaloidCnt,
        SongPushedCnt = saveData.SongPushedCnt,
        SongFavoriteCnt = saveData.SongFavoriteCnt,
        SongRecentCnt = saveData.SongRecentCnt,
        TotalCreditCnt = saveData.TotalCreditCnt,
        PrevAreaCode = saveData.PrevAreaCode,
        ConsecAreaCnt = saveData.ConsecAreaCnt,
        DefaultShinSetting = saveData.DefaultShinSetting,
        DispLevelTotal = saveData.DispLevelTotal,
        DispLevelChassis = saveData.DispLevelChassis,
        DispLevelSelf = saveData.DispLevelSelf,
        DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
        DifficultyPlayedStar = saveData.DifficultyPlayedStar,
        IsChallengeCompe = saveData.IsChallengeCompe,
        IsTojiru = saveData.IsTojiru
    };
}
```

Create `Application/Ac15/GreenAc15UserDataAdapter.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class GreenAc15UserDataAdapter
{
    public static Ac15UserDataSnapshot CreateSnapshot(
        UserSaveDataGreen saveData,
        Ac15CatalogSnapshot catalog,
        IReadOnlyList<uint> favorites,
        IReadOnlyList<uint> recent,
        IReadOnlyCollection<(uint ItemType, uint ItemId)> unlockedShopItems)
    {
        var lockedSongIds = LockedIds(catalog, Ac15ShopItemType.Song, unlockedShopItems).ToArray();
        var lockedToneIds = LockedIds(catalog, Ac15ShopItemType.Tone, unlockedShopItems).ToArray();

        return new Ac15UserDataSnapshot(
            catalog.SongHashVersion,
            catalog.SongNoesInFileOrder,
            [],
            saveData.ToneFlg,
            saveData.TitleFlg,
            saveData.DefaultOptionSetting,
            saveData.OptionFlg,
            favorites,
            recent,
            catalog.RecommendSong,
            catalog.RecommendBestSongs,
            Counters(saveData),
            saveData.DispTaikojukuDan,
            lockedSongIds,
            lockedToneIds,
            TokkunTutorialFlg: null,
            saveData.IsDevil);
    }

    private static IEnumerable<uint> LockedIds(
        Ac15CatalogSnapshot catalog,
        Ac15ShopItemType itemType,
        IReadOnlyCollection<(uint ItemType, uint ItemId)> unlockedShopItems)
        => catalog.ItemShopCatalog.ActiveSeason?.Items
            .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType.ToProtocolValue(), item.ItemId)))
            .Select(item => item.ItemId) ?? [];

    private static Ac15ProfileCounters Counters(UserSaveDataGreen saveData) => new()
    {
        CategJpopCnt = saveData.CategJpopCnt,
        CategAnimeCnt = saveData.CategAnimeCnt,
        CategDoyoCnt = saveData.CategDoyoCnt,
        CategVarietyCnt = saveData.CategVarietyCnt,
        CategClassicCnt = saveData.CategClassicCnt,
        CategGameCnt = saveData.CategGameCnt,
        CategNamcoCnt = saveData.CategNamcoCnt,
        CategVocaloidCnt = saveData.CategVocaloidCnt,
        SongPushedCnt = saveData.SongPushedCnt,
        SongFavoriteCnt = saveData.SongFavoriteCnt,
        SongRecentCnt = saveData.SongRecentCnt,
        TotalCreditCnt = saveData.TotalCreditCnt,
        PrevAreaCode = saveData.PrevAreaCode,
        ConsecAreaCnt = saveData.ConsecAreaCnt,
        DefaultShinSetting = saveData.DefaultShinSetting,
        DispLevelTotal = saveData.DispLevelTotal,
        DispLevelChassis = saveData.DispLevelChassis,
        DispLevelSelf = saveData.DispLevelSelf,
        DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
        DifficultyPlayedStar = saveData.DifficultyPlayedStar,
        IsChallengeCompe = saveData.IsChallengeCompe,
        IsTojiru = saveData.IsTojiru
    };
}
```

- [ ] **Step 2: Update Blue userdata handler**

In `Application/Handlers/UserDataQuery.Blue.cs`, keep the existing user existence check, `GetOrCreateBlueSaveDataAsync`, Dan display normalization query, favorites query, recent query, and unlocked-shop query. Replace the return object initializer with:

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromBlue(blue);
var userdata = BlueAc15UserDataAdapter.CreateSnapshot(
    saveData,
    snapshot,
    favorites,
    recent,
    unlockedShopItems);
var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Blue);
response.DispTaikojukuDan = GetSafeBlueTaikojukuDanSlot(displayDan);
response.IsDevilBlue = saveData.IsDevil;
return response;
```

Keep `TokkunTutorialFlg` readback only through `Ac15UserDataService` and the Blue profile.

- [ ] **Step 3: Update Green userdata handler**

In `Application/Handlers/UserDataQuery.Green.cs`, keep the existing user existence check, `GetOrCreateGreenSaveDataAsync`, Dan display normalization query, favorites query, recent query, and unlocked-shop query. Replace the return object initializer with:

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromGreen(green);
var userdata = GreenAc15UserDataAdapter.CreateSnapshot(
    saveData,
    snapshot,
    favorites,
    recent,
    unlockedShopItems);
var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Green);
response.DispTaikojukuDan = GetSafeTaikojukuDanSlot(displayDan);
response.IsDevilGreen = saveData.IsDevil;
return response;
```

- [ ] **Step 4: Run userdata regressions**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15UserDataServiceTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~GreenUserDataMapperTests|FullyQualifiedName~GreenSaveDataTests"
```

Expected: PASS.

- [ ] **Step 5: Commit stage 5**

Run:

```powershell
git add Application/Ac15 Application/Handlers/UserDataQuery.Blue.cs Application/Handlers/UserDataQuery.Green.cs Tests/Ac15 Tests/Blue/BlueUserDataTests.cs Tests/Blue/BlueTokkunPersistenceTests.cs Tests/Green/GreenUserDataMapperTests.cs Tests/Green/GreenSaveDataTests.cs
git commit -m "Extract AC15 userdata composition core"
```
