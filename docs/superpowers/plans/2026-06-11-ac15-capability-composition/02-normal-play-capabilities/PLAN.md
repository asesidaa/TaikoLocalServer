# AC15 Normal Play Capabilities Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace `Ac15NormalPlayService` table-only `GameEra` switches with handler-bound capabilities for save mutation, play rows, best rows, favorites, and recent songs.

**Architecture:** Domain save rows expose narrow capability interfaces for identical fields. Era handlers bind concrete save rows, concrete `DbSet`s, Mapperly delegates, unlock policies, and stage policies, then call switch-free `Application/Ac15` modules.

**Tech Stack:** C# 13, .NET 10, EF Core SQLite, Mapperly, Mediator handlers, xUnit.

---

## File Structure

Create:

- `Domain/Entities/IAc15SaveDataCapabilities.cs` - narrow save row shape interfaces.
- `Application/Ac15/Ac15UnlockFlagAccess.cs` - per-era unlock bit writers.
- `Application/Ac15/Ac15CommonProfileMutation.cs` - shared normal save-row mutation.
- `Application/Ac15/Ac15NormalPlayWriter.cs` - generic EF writer for play, best, favorite, and recent rows.

Modify:

- `Domain/Entities/UserSaveDataBlue.cs`
- `Domain/Entities/UserSaveDataGreen.cs`
- `Domain/Entities/UserSaveDataYellow.cs`
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- `Application/Ac15/Ac15NormalPlayMapper.cs`
- `Application/Ac15/Ac15NormalPlayService.cs`
- `Tests/Ac15/Ac15NormalPlayServiceTests.cs`
- `Tests/Ac15/Ac15CommonProfileMutationTests.cs`
- `Tests/Ac15/Ac15NormalPlayWriterTests.cs`
- Existing Blue, Green, and Yellow playresult tests touched by the refactor.

## Task 1: Save Capability Interfaces And Common Mutation

**Files:**
- Create: `Domain/Entities/IAc15SaveDataCapabilities.cs`
- Modify: `Domain/Entities/UserSaveDataBlue.cs`
- Modify: `Domain/Entities/UserSaveDataGreen.cs`
- Modify: `Domain/Entities/UserSaveDataYellow.cs`
- Create: `Application/Ac15/Ac15UnlockFlagAccess.cs`
- Create: `Application/Ac15/Ac15CommonProfileMutation.cs`
- Create: `Tests/Ac15/Ac15CommonProfileMutationTests.cs`

- [ ] **Step 1: Write common mutation behavior tests**

Create `Tests/Ac15/Ac15CommonProfileMutationTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CommonProfileMutationTests
{
    [Fact]
    public void TryApply_AddsDonMedalsToActiveShopSeasonAndKatsuToSave()
    {
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        var season = new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 4,
            TotalGetDonmedal = 100,
            TotalUseDonmedal = 20,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var applied = Ac15CommonProfileMutation.TryApply(
            save,
            season,
            PlayResult(getDonmedal: 25, getKatsumedal: 5),
            countedStages: [Stage(101, isFavorite: true)],
            Ac15ProfileCounterUpdater.Blue,
            Ac15UnlockFlagAccess.Blue,
            Ac15EraProfiles.Blue.Limits,
            new DateTime(2026, 5, 14, 3, 24, 42),
            ApplyBlueCostume);

        Assert.True(applied);
        Assert.Equal(125u, season.TotalGetDonmedal);
        Assert.Equal(0u, save.TotalGetDonmedal);
        Assert.Equal(5u, save.TotalGetKatsumedal);
        Assert.Equal(1u, save.SongFavoriteCnt);
        Assert.Equal(new DateTime(2026, 5, 14, 3, 24, 42), save.LastPlayDatetime);
    }

    [Fact]
    public void TryApply_DetectsDonMedalOverflowBeforeMutation()
    {
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.TotalGetDonmedal = uint.MaxValue;

        var applied = Ac15CommonProfileMutation.TryApply(
            save,
            shopSeasonState: null,
            PlayResult(getDonmedal: 1, getKatsumedal: 0),
            countedStages: [Stage(101)],
            Ac15ProfileCounterUpdater.Blue,
            Ac15UnlockFlagAccess.Blue,
            Ac15EraProfiles.Blue.Limits,
            DateTime.UnixEpoch,
            ApplyBlueCostume);

        Assert.False(applied);
        Assert.Equal(uint.MaxValue, save.TotalGetDonmedal);
        Assert.Equal(0u, save.SongRecentCnt);
    }

    [Fact]
    public void GreenUnlockPolicyLeavesSongReleaseFlagsAbsent()
    {
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);

        var applied = Ac15CommonProfileMutation.TryApply(
            save,
            shopSeasonState: null,
            new CommonPlayResultData
            {
                ReleaseSongNoes = [101],
                GetToneNoes = [5],
                AryStageInfoes = [Stage(101)]
            },
            countedStages: [Stage(101)],
            Ac15ProfileCounterUpdater.Green,
            Ac15UnlockFlagAccess.Green,
            Ac15EraProfiles.Green.Limits,
            DateTime.UnixEpoch,
            ApplyGreenCostume);

        Assert.True(applied);
        Assert.True(BitIsSet(save.ToneFlg, 5));
    }

    private static CommonPlayResultData PlayResult(uint getDonmedal, uint getKatsumedal)
        => new()
        {
            GetDonmedal = getDonmedal,
            GetKatsumedal = getKatsumedal,
            AreaCode = 10,
            IsDevil = true,
            IsExplain = true,
            WaiwaiTutorialFlg = 1,
            HasDifficultyPlayedCourse = true,
            DifficultyPlayedCourse = 3,
            HasDifficultyPlayedStar = true,
            DifficultyPlayedStar = 4,
            AryStageInfoes = [Stage(101)]
        };

    private static CommonPlayResultData.StageData Stage(uint songNo, bool isFavorite = false)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            StageMode = 0,
            MusicCateg = 2,
            IsFavorite = isFavorite,
            IsRecent = true
        };

    private static void ApplyBlueCostume(UserSaveDataBlue save, CommonPlayResultData.CostumeData costume)
    {
        save.Costume1 = costume.Costume1;
        save.Costume2 = costume.Costume2;
        save.Costume3 = costume.Costume3;
        save.Costume4 = costume.Costume4;
        save.Costume5 = costume.Costume5;
    }

    private static void ApplyGreenCostume(UserSaveDataGreen save, CommonPlayResultData.CostumeData costume)
    {
        save.Costume1 = costume.Costume1;
        save.Costume2 = costume.Costume2;
        save.Costume3 = costume.Costume3;
        save.Costume4 = costume.Costume4;
        save.Costume5 = costume.Costume5;
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
```

- [ ] **Step 2: Run mutation tests and verify compile failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CommonProfileMutationTests"
```

Expected: compile failure because the capability interfaces, unlock access record, and mutation helper do not exist.

- [ ] **Step 3: Add save capability interfaces**

Create `Domain/Entities/IAc15SaveDataCapabilities.cs`:

```csharp
namespace TaikoLocalServer.Domain.Entities;

public interface IAc15MedalSaveData
{
    uint TotalGetDonmedal { get; set; }
    uint TotalUseDonmedal { get; set; }
    uint TotalGetKatsumedal { get; set; }
    uint TotalUseKatsumedal { get; set; }
}

public interface IAc15TutorialSaveData
{
    uint ItemshopTutorialFlg { get; set; }
    bool IsDevil { get; set; }
    bool IsExplain { get; set; }
    uint WaiwaiTutorialFlg { get; set; }
    uint DifficultyPlayedCourse { get; set; }
    uint DifficultyPlayedStar { get; set; }
}

public interface IAc15PlayProfileSaveData
{
    DateTime LastPlayDatetime { get; set; }
    uint PrevAreaCode { get; set; }
}

public interface IAc15CustomizationSaveData
{
    bool IsAutoCostumeOn { get; set; }
    uint Costume1 { get; set; }
    uint Costume2 { get; set; }
    uint Costume3 { get; set; }
    uint Costume4 { get; set; }
    uint Costume5 { get; set; }
    byte[] CostumeFlg1 { get; set; }
    byte[] CostumeFlg2 { get; set; }
    byte[] CostumeFlg3 { get; set; }
    byte[] CostumeFlg4 { get; set; }
    byte[] CostumeFlg5 { get; set; }
    byte[] ToneFlg { get; set; }
    byte[] TitleFlg { get; set; }
}

public interface IAc15SongUnlockSaveData
{
    byte[] ReleaseSongFlg { get; set; }
}
```

Modify the class declaration in `Domain/Entities/UserSaveDataBlue.cs`:

```csharp
public partial class UserSaveDataBlue :
    IAc15MedalSaveData,
    IAc15TutorialSaveData,
    IAc15PlayProfileSaveData,
    IAc15CustomizationSaveData,
    IAc15SongUnlockSaveData
```

Modify the class declaration in `Domain/Entities/UserSaveDataGreen.cs`:

```csharp
public partial class UserSaveDataGreen :
    IAc15MedalSaveData,
    IAc15TutorialSaveData,
    IAc15PlayProfileSaveData,
    IAc15CustomizationSaveData
```

Modify the class declaration in `Domain/Entities/UserSaveDataYellow.cs`:

```csharp
public partial class UserSaveDataYellow :
    IAc15MedalSaveData,
    IAc15TutorialSaveData,
    IAc15PlayProfileSaveData,
    IAc15CustomizationSaveData,
    IAc15SongUnlockSaveData
```

- [ ] **Step 4: Add unlock access policies**

Create `Application/Ac15/Ac15UnlockFlagAccess.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15UnlockFlagAccess<TSave>(
    Action<TSave, IEnumerable<uint>>? ReleaseSongs,
    Action<TSave, IEnumerable<uint>> Tones,
    Action<TSave, IEnumerable<uint>> Titles,
    Action<TSave, IEnumerable<uint>> Costume1,
    Action<TSave, IEnumerable<uint>> Costume2,
    Action<TSave, IEnumerable<uint>> Costume3,
    Action<TSave, IEnumerable<uint>> Costume4,
    Action<TSave, IEnumerable<uint>> Costume5);

public static class Ac15UnlockFlagAccess
{
    public static Ac15UnlockFlagAccess<UserSaveDataBlue> Blue { get; } = new(
        (save, ids) => save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, ids, BlueProtocolBytes.SongFlagBytes),
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, BlueProtocolBytes.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, BlueProtocolBytes.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, BlueProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, BlueProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, BlueProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, BlueProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, BlueProtocolBytes.CostumeFlagBytes));

    public static Ac15UnlockFlagAccess<UserSaveDataGreen> Green { get; } = new(
        ReleaseSongs: null,
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, GreenProtocolBytes.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, GreenProtocolBytes.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, GreenProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, GreenProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, GreenProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, GreenProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, GreenProtocolBytes.CostumeFlagBytes));

    public static Ac15UnlockFlagAccess<UserSaveDataYellow> Yellow { get; } = new(
        (save, ids) => save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, ids, Ac15EraProfiles.Yellow.Limits.SongFlagBytes),
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, Ac15EraProfiles.Yellow.Limits.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, Ac15EraProfiles.Yellow.Limits.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes));
}
```

- [ ] **Step 5: Add common profile mutation helper**

Create `Application/Ac15/Ac15CommonProfileMutation.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CommonProfileMutation
{
    public static bool TryApply<TSave, TSeasonState>(
        TSave saveData,
        TSeasonState? shopSeasonState,
        CommonPlayResultData playResultData,
        IReadOnlyList<CommonPlayResultData.StageData> countedStages,
        Ac15ProfileCounterAccess<TSave> counterAccess,
        Ac15UnlockFlagAccess<TSave> unlockAccess,
        Ac15ProtocolLimits limits,
        DateTime playTime,
        Action<TSave, CommonPlayResultData.CostumeData> applyCurrentCostume)
        where TSave :
            IAc15MedalSaveData,
            IAc15TutorialSaveData,
            IAc15PlayProfileSaveData,
            IAc15CustomizationSaveData
        where TSeasonState : class, IAc15ShopSeasonState
    {
        var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;
        if (!CanAdd(currentDonmedal, playResultData.GetDonmedal)
            || !CanAdd(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal))
        {
            return false;
        }

        if (shopSeasonState is null)
        {
            saveData.TotalGetDonmedal += playResultData.GetDonmedal;
        }
        else
        {
            shopSeasonState.TotalGetDonmedal += playResultData.GetDonmedal;
            shopSeasonState.UpdatedAt = DateTime.UtcNow;
        }

        saveData.TotalGetKatsumedal += playResultData.GetKatsumedal;
        saveData.ItemshopTutorialFlg = playResultData.ItemshopTutorialFlg ?? saveData.ItemshopTutorialFlg;
        saveData.IsDevil = playResultData.IsDevil ?? saveData.IsDevil;
        saveData.IsExplain = playResultData.IsExplain ?? saveData.IsExplain;
        saveData.WaiwaiTutorialFlg = playResultData.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;
        if (playResultData.HasDifficultyPlayedCourse)
        {
            saveData.DifficultyPlayedCourse = playResultData.DifficultyPlayedCourse;
        }

        if (playResultData.HasDifficultyPlayedStar)
        {
            saveData.DifficultyPlayedStar = playResultData.DifficultyPlayedStar;
        }

        saveData.LastPlayDatetime = playTime;
        saveData.PrevAreaCode = playResultData.AreaCode;

        if (playResultData.HasAryCurrentCostume && saveData.IsAutoCostumeOn)
        {
            applyCurrentCostume(saveData, playResultData.AryCurrentCostume);
        }

        unlockAccess.ReleaseSongs?.Invoke(saveData, playResultData.ReleaseSongNoes);
        unlockAccess.Tones(saveData, playResultData.GetToneNoes);
        unlockAccess.Titles(saveData, playResultData.GetTitleNoes);
        unlockAccess.Costume1(saveData, playResultData.GetCostumeNo1s);
        unlockAccess.Costume2(saveData, playResultData.GetCostumeNo2s);
        unlockAccess.Costume3(saveData, playResultData.GetCostumeNo3s);
        unlockAccess.Costume4(saveData, playResultData.GetCostumeNo4s);
        unlockAccess.Costume5(saveData, playResultData.GetCostumeNo5s);

        foreach (var stage in countedStages)
        {
            Ac15ProfileCounterUpdater.ApplyStage(saveData, stage, counterAccess);
        }

        return true;
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;
}
```

- [ ] **Step 6: Run common mutation tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CommonProfileMutationTests"
```

Expected: PASS.

## Task 2: Generic Normal Play Writer

**Files:**
- Create: `Application/Ac15/Ac15NormalPlayWriter.cs`
- Modify: `Application/Ac15/Ac15NormalPlayMapper.cs`
- Modify: `Tests/Ac15/Ac15NormalPlayServiceTests.cs`
- Create: `Tests/Ac15/Ac15NormalPlayWriterTests.cs`

- [ ] **Step 1: Write normal writer behavior tests**

Create `Tests/Ac15/Ac15NormalPlayWriterTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15NormalPlayWriterTests
{
    [Fact]
    public async Task SaveAsync_WritesOnlyTheBoundEraTables()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var playTime = new DateTime(2026, 5, 14, 3, 24, 42);

        await Ac15NormalPlayWriter.SaveAsync(
            database.Context,
            BlueTables(database.Context),
            new Ac15NormalPlayWriteRequest(
                1,
                PlayMode: 0,
                Stages: [Stage(101, isFavorite: true, isRecent: true)],
                Ac15EraProfiles.Blue.Limits,
                playTime),
            Ac15NormalStagePolicies.Standard,
            CancellationToken.None);

        Assert.Single(await database.Context.SongPlayDataBlue.ToListAsync());
        Assert.Single(await database.Context.SongBestDataBlue.ToListAsync());
        Assert.Single(await database.Context.BlueFavoriteSongs.ToListAsync());
        Assert.Single(await database.Context.BlueRecentSongs.ToListAsync());
        Assert.Empty(await database.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await database.Context.SongPlayDataYellow.ToListAsync());
    }

    [Fact]
    public async Task SaveAsync_AllowsGreenGhostSectionsThroughAfterAddPlayRow()
    {
        await using var database = await SchemaDatabase.CreateAsync();

        await Ac15NormalPlayWriter.SaveAsync(
            database.Context,
            GreenTables(database.Context),
            new Ac15NormalPlayWriteRequest(
                1,
                PlayMode: 0,
                Stages:
                [
                    Stage(101, ghostStageData: new CommonPlayResultData.GhostStageData
                    {
                        ArySectionData =
                        [
                            new() { IsWin = true, GoodCnt = 10, OkCnt = 2, NgCnt = 1, PoundCnt = 4 }
                        ]
                    })
                ],
                Ac15EraProfiles.Green.Limits,
                DateTime.UnixEpoch),
            Ac15NormalStagePolicies.Green,
            CancellationToken.None);

        Assert.Single(await database.Context.SongPlayDataGreen.ToListAsync());
        var section = Assert.Single(await database.Context.GhostStageSectionDataGreen.ToListAsync());
        Assert.True(section.IsWin);
        Assert.Equal(10u, section.GoodCount);
    }

    private static Ac15NormalPlayTables<SongPlayDatumBlue, SongBestDatumBlue, BlueFavoriteSongs, BlueRecentSongs> BlueTables(TaikoDbContext context)
        => new(
            context.SongPlayDataBlue,
            context.SongBestDataBlue,
            context.BlueFavoriteSongs,
            context.BlueRecentSongs,
            Ac15NormalPlayMapper.ToBlueSongPlayDatum,
            Ac15NormalPlayMapper.ToBlueSongBestDatum);

    private static Ac15NormalPlayTables<SongPlayDatumGreen, SongBestDatumGreen, GreenFavoriteSongs, GreenRecentSongs> GreenTables(TaikoDbContext context)
        => new(
            context.SongPlayDataGreen,
            context.SongBestDataGreen,
            context.GreenFavoriteSongs,
            context.GreenRecentSongs,
            Ac15NormalPlayMapper.ToGreenSongPlayDatum,
            Ac15NormalPlayMapper.ToGreenSongBestDatum,
            AfterAddPlayRow: (play, row) => AddGreenGhostSections(context, play, row));

    private static void AddGreenGhostSections(TaikoDbContext context, SongPlayDatumGreen play, Ac15PlayRow row)
    {
        if (row.GhostStageData is null)
        {
            return;
        }

        uint sectionNo = 0;
        foreach (var section in row.GhostStageData.ArySectionData)
        {
            context.GhostStageSectionDataGreen.Add(new GhostStageSectionDatumGreen
            {
                Parent = play,
                SectionNo = sectionNo++,
                IsWin = section.IsWin,
                GoodCount = section.GoodCnt,
                OkCount = section.OkCnt,
                NgCount = section.NgCnt,
                PoundCount = section.PoundCnt
            });
        }
    }

    private static CommonPlayResultData.StageData Stage(
        uint songNo,
        bool isFavorite = false,
        bool isRecent = false,
        CommonPlayResultData.GhostStageData? ghostStageData = null)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            StageMode = 0,
            PlayResult = 2,
            PlayScore = 123456,
            ScoreRate = 87,
            IsFavorite = isFavorite,
            IsRecent = isRecent,
            GhostStageData = ghostStageData
        };

    private sealed class SchemaDatabase(SqliteConnection connection) : IAsyncDisposable
    {
        public TaikoDbContext Context { get; } = CreateContext(connection);

        public static async Task<SchemaDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var database = new SchemaDatabase(connection);
            await database.Context.Database.EnsureCreatedAsync();
            return database;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }

        private static TaikoDbContext CreateContext(SqliteConnection connection)
            => new(new DbContextOptionsBuilder<TaikoDbContext>()
                .UseSqlite(connection)
                .Options);
    }
}
```

- [ ] **Step 2: Run writer tests and verify compile failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15NormalPlayWriterTests"
```

Expected: compile failure because `Ac15NormalPlayWriter`, `Ac15NormalPlayTables`, and `Ac15NormalPlayWriteRequest` do not exist.

- [ ] **Step 3: Add normal play writer records and implementation**

Create `Application/Ac15/Ac15NormalPlayWriter.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15NormalPlayTables<TPlay, TBest, TFavorite, TRecent>(
    DbSet<TPlay> PlayRows,
    DbSet<TBest> BestRows,
    DbSet<TFavorite> FavoriteRows,
    DbSet<TRecent> RecentRows,
    Func<Ac15PlayRow, TPlay> CreatePlay,
    Func<uint, Ac15BestRow, bool, TBest> CreateBest,
    Action<TPlay, Ac15PlayRow>? AfterAddPlayRow = null)
    where TPlay : class, IAc15SongPlayDatum
    where TBest : class, IAc15SongBestDatum
    where TFavorite : class, IAc15FavoriteSong, new()
    where TRecent : class, IAc15RecentSong, new();

public sealed record Ac15NormalPlayWriteRequest(
    uint Baid,
    uint PlayMode,
    IReadOnlyList<CommonPlayResultData.StageData> Stages,
    Ac15ProtocolLimits Limits,
    DateTime PlayTime);

public static class Ac15NormalPlayWriter
{
    public static async ValueTask SaveAsync<TPlay, TBest, TFavorite, TRecent>(
        ITaikoDbContext context,
        Ac15NormalPlayTables<TPlay, TBest, TFavorite, TRecent> tables,
        Ac15NormalPlayWriteRequest request,
        Ac15NormalStagePolicy policy,
        CancellationToken cancellationToken)
        where TPlay : class, IAc15SongPlayDatum
        where TBest : class, IAc15SongBestDatum
        where TFavorite : class, IAc15FavoriteSong, new()
        where TRecent : class, IAc15RecentSong, new()
    {
        foreach (var stage in request.Stages)
        {
            var difficulty = MapDifficulty(stage.Level);
            var crown = MapCrown(stage.PlayResult);
            var isShin = stage.StageMode == 1 || stage.StageMode == 4;
            var bestPolicy = policy.GetBestUpdatePolicy(stage, crown);
            var playRow = ToPlayRow(request.Baid, request.PlayMode, stage, difficulty, crown, isShin, request.PlayTime);
            var play = tables.CreatePlay(playRow);
            tables.PlayRows.Add(play);
            tables.AfterAddPlayRow?.Invoke(play, playRow);

            if (request.PlayMode != (uint)PlayMode.DanMode || isShin)
            {
                await UpsertBestAsync(
                    tables.BestRows,
                    request.Baid,
                    new Ac15BestRow(stage.SongNo, difficulty, isShin, stage.PlayScore, stage.ScoreRate, crown),
                    bestPolicy,
                    tables.CreateBest,
                    cancellationToken);
            }

            await SetFavoriteAsync(tables.FavoriteRows, request.Baid, stage.SongNo, stage.IsFavorite, request.Limits.MaxFavoriteSongs, cancellationToken);
            await UpsertRecentAsync(tables.RecentRows, request.Baid, stage.SongNo, request.PlayTime, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        await TrimRecentAsync(tables.RecentRows, context.SaveChangesAsync, request.Baid, request.Limits.MaxRecentSongs, cancellationToken);
    }

    private static async ValueTask UpsertBestAsync<TBest>(
        DbSet<TBest> bestRows,
        uint baid,
        Ac15BestRow row,
        Ac15BestUpdatePolicy policy,
        Func<uint, Ac15BestRow, bool, TBest> create,
        CancellationToken cancellationToken)
        where TBest : class, IAc15SongBestDatum
    {
        var existing = await bestRows.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
        if (existing is null)
        {
            bestRows.Add(create(baid, row, policy.AllowCrownUpdate));
            return;
        }

        if (policy.AllowScoreUpdate && row.BestScore > existing.BestScore)
        {
            existing.BestScore = row.BestScore;
            existing.BestRate = row.BestRate;
        }

        if (policy.AllowCrownUpdate && CrownRank(row.BestCrown) > CrownRank(existing.BestCrown))
        {
            existing.BestCrown = row.BestCrown;
        }
    }

    private static async ValueTask SetFavoriteAsync<TFavorite>(
        DbSet<TFavorite> favorites,
        uint baid,
        uint songNo,
        bool isFavorite,
        int maxFavorites,
        CancellationToken cancellationToken)
        where TFavorite : class, IAc15FavoriteSong, new()
    {
        var favorite = await favorites.FindAsync([baid, songNo], cancellationToken);
        if (isFavorite && favorite is null)
        {
            var persisted = await favorites.Where(song => song.Baid == baid).Select(song => song.SongNo).ToArrayAsync(cancellationToken);
            var tracked = favorites.Local.Where(song => song.Baid == baid).Select(song => song.SongNo);
            if (persisted.Concat(tracked).Distinct().Count() < maxFavorites)
            {
                favorites.Add(new TFavorite { Baid = baid, SongNo = songNo });
            }
        }
        else if (!isFavorite && favorite is not null)
        {
            favorites.Remove(favorite);
        }
    }

    private static async ValueTask UpsertRecentAsync<TRecent>(
        DbSet<TRecent> recents,
        uint baid,
        uint songNo,
        DateTime playTime,
        CancellationToken cancellationToken)
        where TRecent : class, IAc15RecentSong, new()
    {
        var recent = await recents.FindAsync([baid, songNo], cancellationToken);
        if (recent is null)
        {
            recents.Add(new TRecent { Baid = baid, SongNo = songNo, LastPlayed = playTime });
            return;
        }

        recent.LastPlayed = playTime;
    }

    private static async ValueTask TrimRecentAsync<TRecent>(
        DbSet<TRecent> recents,
        Func<CancellationToken, Task<int>> saveChanges,
        uint baid,
        int maxRecent,
        CancellationToken cancellationToken)
        where TRecent : class, IAc15RecentSong
    {
        var overage = await recents
            .Where(song => song.Baid == baid)
            .OrderByDescending(song => song.LastPlayed)
            .Skip(maxRecent)
            .ToListAsync(cancellationToken);
        if (overage.Count == 0)
        {
            return;
        }

        recents.RemoveRange(overage);
        await saveChanges(cancellationToken);
    }

    private static Difficulty MapDifficulty(uint level) => level switch
    {
        1 => Difficulty.Easy,
        2 => Difficulty.Normal,
        3 => Difficulty.Hard,
        4 => Difficulty.Oni,
        5 => Difficulty.UraOni,
        _ => Difficulty.None
    };

    private static CrownType MapCrown(uint playResult) => playResult switch
    {
        1 => CrownType.Clear,
        2 => CrownType.Gold,
        3 => CrownType.Dondaful,
        _ => CrownType.None
    };

    private static Ac15PlayRow ToPlayRow(
        uint baid,
        uint playMode,
        CommonPlayResultData.StageData stage,
        Difficulty difficulty,
        CrownType crown,
        bool isShin,
        DateTime playTime)
        => new(
            baid,
            stage.SongNo,
            difficulty,
            crown,
            stage.PlayScore,
            stage.ScoreRate,
            stage.GoodCnt,
            stage.OkCnt,
            stage.NgCnt,
            stage.ComboCnt,
            stage.HitCnt,
            stage.PoundCnt,
            stage.StarLevel,
            stage.SupportLevel,
            stage.OptionFlg,
            stage.ToneFlg,
            playMode,
            stage.StageMode,
            isShin,
            stage.MusicCateg,
            stage.SelectedFolderId,
            stage.IsFavorite,
            stage.IsRecent,
            stage.IsPapamama,
            stage.IsPushed,
            stage.SoulGauge.GetValueOrDefault(),
            stage.PlayDan.GetValueOrDefault(),
            stage.WaiwaiResult.GetValueOrDefault(),
            stage.WaiwaiGauge.GetValueOrDefault(),
            stage.GhostStageData,
            playTime);

    private static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };
}
```

- [ ] **Step 4: Run writer tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15NormalPlayWriterTests"
```

Expected: PASS.

## Task 3: Bind Normal Play Capabilities In Era Handlers

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Modify: `Application/Ac15/Ac15NormalPlayService.cs`
- Modify: `Tests/Ac15/Ac15NormalPlayServiceTests.cs`
- Modify: existing Blue, Green, and Yellow playresult tests if method names or expected Green invalid-stage behavior changed in stage 1.

- [ ] **Step 1: Add handler-local normal play table binders**

In `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, add:

```csharp
private Ac15NormalPlayTables<SongPlayDatumBlue, SongBestDatumBlue, BlueFavoriteSongs, BlueRecentSongs> BlueNormalPlayTables()
    => new(
        context.SongPlayDataBlue,
        context.SongBestDataBlue,
        context.BlueFavoriteSongs,
        context.BlueRecentSongs,
        Ac15NormalPlayMapper.ToBlueSongPlayDatum,
        Ac15NormalPlayMapper.ToBlueSongBestDatum);
```

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, add:

```csharp
private Ac15NormalPlayTables<SongPlayDatumGreen, SongBestDatumGreen, GreenFavoriteSongs, GreenRecentSongs> GreenNormalPlayTables()
    => new(
        context.SongPlayDataGreen,
        context.SongBestDataGreen,
        context.GreenFavoriteSongs,
        context.GreenRecentSongs,
        Ac15NormalPlayMapper.ToGreenSongPlayDatum,
        Ac15NormalPlayMapper.ToGreenSongBestDatum,
        AfterAddPlayRow: AddGreenGhostStageSections);

private void AddGreenGhostStageSections(SongPlayDatumGreen play, Ac15PlayRow row)
{
    if (row.GhostStageData is null)
    {
        return;
    }

    uint sectionNo = 0;
    foreach (var section in row.GhostStageData.ArySectionData)
    {
        context.GhostStageSectionDataGreen.Add(new GhostStageSectionDatumGreen
        {
            Parent = play,
            SectionNo = sectionNo++,
            IsWin = section.IsWin,
            GoodCount = section.GoodCnt,
            OkCount = section.OkCnt,
            NgCount = section.NgCnt,
            PoundCount = section.PoundCnt
        });
    }
}
```

In `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`, add:

```csharp
private Ac15NormalPlayTables<SongPlayDatumYellow, SongBestDatumYellow, YellowFavoriteSongs, YellowRecentSongs> YellowNormalPlayTables()
    => new(
        context.SongPlayDataYellow,
        context.SongBestDataYellow,
        context.YellowFavoriteSongs,
        context.YellowRecentSongs,
        Ac15NormalPlayMapper.ToYellowSongPlayDatum,
        Ac15NormalPlayMapper.ToYellowSongBestDatum);
```

- [ ] **Step 2: Replace Blue duplicated normal save prelude with common mutation and writer**

In `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, after the battle/Tokkun gates and after loading `saveData`, `blue`, and `shopSeasonState`, replace the medal/tutorial/profile/unlock/stage-counter block with:

```csharp
var validStages = Ac15NormalStageFilter.Filter(
    request.Baid,
    playResultData.AryStageInfoes,
    Ac15EraProfiles.Blue.Limits,
    Ac15NormalStagePolicies.Standard,
    logger);
if (validStages.Count == 0)
{
    logger.LogWarning("Skipping Blue playresult with no valid normal stages for baid {Baid}", request.Baid);
    return 1;
}

playResultData.AryStageInfoes = validStages.ToList();
var playTime = Ac15PlayDatetime.ParseOrNow(playResultData.PlayDatetime);
if (!Ac15CommonProfileMutation.TryApply(
        saveData,
        shopSeasonState,
        playResultData,
        validStages,
        Ac15ProfileCounterUpdater.Blue,
        Ac15UnlockFlagAccess.Blue,
        Ac15EraProfiles.Blue.Limits,
        playTime,
        ApplyCostume))
{
    logger.LogWarning("Rejecting invalid Blue medal totals for baid {Baid}", request.Baid);
    return 1;
}
```

Keep the existing `Ac15DaniService.SaveAsync` call after this block.

Replace the final `Ac15NormalPlayService.SaveAsync(...)` call with:

```csharp
await Ac15NormalPlayWriter.SaveAsync(
    context,
    BlueNormalPlayTables(),
    new Ac15NormalPlayWriteRequest(request.Baid, playResultData.PlayMode, validStages, Ac15EraProfiles.Blue.Limits, playTime),
    Ac15NormalStagePolicies.Standard,
    cancellationToken);
return 1;
```

- [ ] **Step 3: Replace Green duplicated normal save prelude with common mutation and writer**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, keep Green ghost updates and Green ghost played-song mutation in the handler. Replace medal/tutorial/profile/unlock/stage-counter code with:

```csharp
var validStages = Ac15NormalStageFilter.Filter(
    request.Baid,
    playResultData.AryStageInfoes,
    Ac15EraProfiles.Green.Limits,
    Ac15NormalStagePolicies.Green,
    logger);
if (validStages.Count == 0)
{
    logger.LogWarning("Skipping Green playresult with no valid normal stages for baid {Baid}", request.Baid);
    return 1;
}

playResultData.AryStageInfoes = validStages.ToList();
var playTime = Ac15PlayDatetime.ParseOrNow(playResultData.PlayDatetime);
if (!Ac15CommonProfileMutation.TryApply(
        saveData,
        shopSeasonState,
        playResultData,
        validStages,
        Ac15ProfileCounterUpdater.Green,
        Ac15UnlockFlagAccess.Green,
        Ac15EraProfiles.Green.Limits,
        playTime,
        ApplyCostume))
{
    logger.LogWarning("Rejecting invalid Green medal totals for baid {Baid}", request.Baid);
    return 1;
}

await ApplyGhostUpdatesAsync(saveData, playResultData, cancellationToken);
ApplyGhostPlayedSongBits(saveData, playResultData);
```

Keep the existing `Ac15DaniService.SaveAsync` call after this block.

Replace the final `Ac15NormalPlayService.SaveAsync(...)` call with:

```csharp
await Ac15NormalPlayWriter.SaveAsync(
    context,
    GreenNormalPlayTables(),
    new Ac15NormalPlayWriteRequest(request.Baid, playResultData.PlayMode, validStages, Ac15EraProfiles.Green.Limits, playTime),
    Ac15NormalStagePolicies.Green,
    cancellationToken);
return 1;
```

- [ ] **Step 4: Replace Yellow duplicated normal save prelude with common mutation and writer**

In `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`, keep the Yellow Tokkun gate before normal writes and keep `LogYellowWaiWaiStageFacts` in the handler. Replace medal/tutorial/profile/unlock/stage-counter code with:

```csharp
var validStages = Ac15NormalStageFilter.Filter(
    request.Baid,
    playResultData.AryStageInfoes,
    Ac15EraProfiles.Yellow.Limits,
    Ac15NormalStagePolicies.Standard,
    logger);
if (validStages.Count == 0)
{
    logger.LogWarning("Skipping Yellow playresult with no valid normal stages for baid {Baid}", request.Baid);
    return 1;
}

playResultData.AryStageInfoes = validStages.ToList();
var playTime = Ac15PlayDatetime.ParseOrNow(playResultData.PlayDatetime);
if (!Ac15CommonProfileMutation.TryApply(
        saveData,
        shopSeasonState,
        playResultData,
        validStages,
        Ac15ProfileCounterUpdater.Yellow,
        Ac15UnlockFlagAccess.Yellow,
        Ac15EraProfiles.Yellow.Limits,
        playTime,
        ApplyYellowCostume))
{
    logger.LogWarning("Rejecting invalid Yellow medal totals for baid {Baid}", request.Baid);
    return 1;
}
```

Keep `Ac15DaniService.SaveAsync` and `LogYellowWaiWaiStageFacts` after this block.

Replace the final `Ac15NormalPlayService.SaveAsync(...)` call with:

```csharp
await Ac15NormalPlayWriter.SaveAsync(
    context,
    YellowNormalPlayTables(),
    new Ac15NormalPlayWriteRequest(request.Baid, playResultData.PlayMode, validStages, Ac15EraProfiles.Yellow.Limits, playTime),
    Ac15NormalStagePolicies.Standard,
    cancellationToken);
return 1;
```

- [ ] **Step 5: Remove the old switched normal play service path**

Delete `Application/Ac15/Ac15NormalPlayService.cs` after the handlers no longer call it.

Delete or rewrite `Tests/Ac15/Ac15NormalPlayServiceTests.cs` so the remaining shared normal-play coverage is in `Ac15NormalPlayWriterTests` and `Ac15CommonProfileMutationTests`.

- [ ] **Step 6: Run focused normal play tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CommonProfileMutationTests|FullyQualifiedName~Ac15NormalPlayWriterTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~GreenPlayResultHandlerTests|FullyQualifiedName~YellowPlayResultHandlerTests|FullyQualifiedName~GreenAiBattlePlayResultTests"
```

Expected: PASS. Blue battle, Blue Tokkun, Yellow Tokkun, and Green AI tests must still prove their special behavior remains outside normal writes.

- [ ] **Step 7: Run AC15 and era slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Yellow"
```

Expected: PASS for all four commands.

- [ ] **Step 8: Commit stage 2**

Run:

```powershell
git add Domain/Entities/IAc15SaveDataCapabilities.cs Domain/Entities/UserSaveDataBlue.cs Domain/Entities/UserSaveDataGreen.cs Domain/Entities/UserSaveDataYellow.cs Application/Ac15/Ac15UnlockFlagAccess.cs Application/Ac15/Ac15CommonProfileMutation.cs Application/Ac15/Ac15NormalPlayWriter.cs Application/Ac15/Ac15NormalPlayMapper.cs Application/Ac15/Ac15NormalPlayService.cs Application/Handlers/UpdatePlayResultCommand.Blue.cs Application/Handlers/UpdatePlayResultCommand.Green.cs Application/Handlers/UpdatePlayResultCommand.Yellow.cs Tests/Ac15/Ac15CommonProfileMutationTests.cs Tests/Ac15/Ac15NormalPlayWriterTests.cs Tests/Ac15/Ac15NormalPlayServiceTests.cs Tests/Blue Tests/Green Tests/Yellow
git commit -m "Compose AC15 normal play through capabilities"
```

## Self-Review

- Spec coverage: normal stage filtering, common profile mutation, normal play row writing, concrete `DbSet` binding, Green ghost row insertion, and no-cross-era table writes are covered.
- Non-goals honored: no shared EF tables, no repository-shaped persistence adapter, no broad `IAc15EraHooks` replacement, no special-mode generic machinery.
- Type consistency: `Ac15NormalStagePolicy`, `Ac15NormalPlayTables<TPlay,TBest,TFavorite,TRecent>`, `Ac15NormalPlayWriteRequest`, `Ac15UnlockFlagAccess<TSave>`, and `Ac15CommonProfileMutation` are the stable names used by later stages.
