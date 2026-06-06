# AC15 Normal Play Core And Hooks Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move shared AC15 normal play save behavior behind typed persistence adapters while preserving Blue battle, Blue Tokkun, and Green AI battle as explicit era extensions.

**Architecture:** Introduce `Ac15NormalPlayService` with canonical stage rows, save updates, best-score policy, favorites/recent trimming, and Dan helper calls. Blue and Green handlers perform user existence checks, create the correct adapter/hooks, and call the core only after special-mode hooks decide the payload is normal play.

**Tech Stack:** C# 13, .NET 10, EF Core through existing DbContext, Mediator handlers, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15NormalPlayRecords.cs` - canonical play row, stage decision, save mutation, and best update policy records.
- `Application/Ac15/IAc15NormalPlayPersistence.cs` - port for era save, play rows, best rows, favorites, recent, and Dan writes.
- `Application/Ac15/Ac15NormalPlayService.cs` - shared normal play workflow.
- `Application/Ac15/BlueAc15NormalPlayAdapter.cs` - Blue persistence adapter and hooks for battle/Tokkun interception.
- `Application/Ac15/GreenAc15NormalPlayAdapter.cs` - Green persistence adapter and hooks for AI battle side effects.
- `Tests/Ac15/Ac15NormalPlayServiceTests.cs`

Modify:

- `Application/Ac15/IAc15EraHooks.cs` - add best-score update policy hook.
- `Application/Ac15/DefaultAc15EraHooks.cs` - default best-score policy allows score and crown updates.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs` only if Blue hooks delegate to existing private methods.
- `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs` only if Blue hooks delegate to existing private methods.
- Existing Blue/Green playresult tests.

## Task 1: Core Normal Play Service

**Files:**
- Create: `Tests/Ac15/Ac15NormalPlayServiceTests.cs`
- Create: `Application/Ac15/Ac15NormalPlayRecords.cs`
- Create: `Application/Ac15/IAc15NormalPlayPersistence.cs`
- Create: `Application/Ac15/Ac15NormalPlayService.cs`
- Modify: `Application/Ac15/IAc15EraHooks.cs`
- Modify: `Application/Ac15/DefaultAc15EraHooks.cs`

- [ ] **Step 1: Write failing normal-play service tests**

Create `Tests/Ac15/Ac15NormalPlayServiceTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15NormalPlayServiceTests
{
    [Fact]
    public async Task SaveAsync_ReturnsSuccessWhenBaidIsZero()
    {
        var result = await Ac15NormalPlayService.SaveAsync(
            baid: 0,
            new CommonPlayResultData(),
            Ac15EraProfiles.Blue,
            new FakePersistence(userExists: false),
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
    }

    [Fact]
    public async Task SaveAsync_SkipsMissingUserWithSuccess()
    {
        var persistence = new FakePersistence(userExists: false);

        var result = await Ac15NormalPlayService.SaveAsync(
            baid: 1,
            new CommonPlayResultData(),
            Ac15EraProfiles.Blue,
            persistence,
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.False(persistence.SaveWasCalled);
    }

    [Fact]
    public async Task SaveAsync_AddsStageRowsAndTrimsRecentSongs()
    {
        var persistence = new FakePersistence(userExists: true);
        var playResult = new CommonPlayResultData
        {
            PlayDatetime = "20260607010101",
            PlayMode = 0,
            AryStageInfoes =
            [
                new()
                {
                    SongNo = 101,
                    Level = 2,
                    StageMode = 0,
                    PlayResult = 2,
                    PlayScore = 123456,
                    ScoreRate = 87,
                    IsFavorite = true,
                    IsRecent = true
                }
            ]
        };

        var result = await Ac15NormalPlayService.SaveAsync(
            baid: 1,
            playResult,
            Ac15EraProfiles.Blue,
            persistence,
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(persistence.PlayRows);
        Assert.Single(persistence.BestRows);
        Assert.Equal([(1u, 101u)], persistence.Favorites);
        Assert.Equal([(1u, 101u)], persistence.Recent);
        Assert.True(persistence.TrimRecentWasCalled);
        Assert.True(persistence.SaveWasCalled);
    }

    private sealed class FakePersistence(bool userExists) : IAc15NormalPlayPersistence
    {
        public bool SaveWasCalled { get; private set; }
        public bool TrimRecentWasCalled { get; private set; }
        public List<Ac15PlayRow> PlayRows { get; } = [];
        public List<Ac15BestRow> BestRows { get; } = [];
        public List<(uint Baid, uint SongNo)> Favorites { get; } = [];
        public List<(uint Baid, uint SongNo)> Recent { get; } = [];

        public ValueTask<bool> UserExistsAsync(uint baid, CancellationToken cancellationToken)
            => ValueTask.FromResult(userExists);

        public ValueTask<Ac15SaveSnapshot> GetOrCreateSaveAsync(uint baid, CancellationToken cancellationToken)
            => ValueTask.FromResult(new Ac15SaveSnapshot(baid));

        public ValueTask AddPlayRowAsync(Ac15PlayRow row, CancellationToken cancellationToken)
        {
            PlayRows.Add(row);
            return ValueTask.CompletedTask;
        }

        public ValueTask UpsertBestAsync(uint baid, Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken cancellationToken)
        {
            BestRows.Add(row);
            return ValueTask.CompletedTask;
        }

        public ValueTask SetFavoriteAsync(uint baid, uint songNo, bool isFavorite, int maxFavorites, CancellationToken cancellationToken)
        {
            if (isFavorite)
            {
                Favorites.Add((baid, songNo));
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask UpsertRecentAsync(uint baid, uint songNo, DateTime playTime, CancellationToken cancellationToken)
        {
            Recent.Add((baid, songNo));
            return ValueTask.CompletedTask;
        }

        public ValueTask TrimRecentAsync(uint baid, int maxRecent, CancellationToken cancellationToken)
        {
            TrimRecentWasCalled = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveWasCalled = true;
            return ValueTask.CompletedTask;
        }
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15NormalPlayServiceTests"
```

Expected: compile failure because normal-play core types do not exist.

- [ ] **Step 3: Add normal-play records and persistence port**

Create `Application/Ac15/Ac15NormalPlayRecords.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15SaveSnapshot(uint Baid);

public sealed record Ac15PlayRow(
    uint Baid,
    uint SongId,
    Difficulty Difficulty,
    CrownType Crown,
    uint Score,
    uint ScoreRate,
    uint GoodCount,
    uint OkCount,
    uint MissCount,
    uint ComboCount,
    uint HitCount,
    uint PoundCount,
    uint StarLevel,
    uint SupportLevel,
    byte[] OptionFlg,
    byte[] ToneFlg,
    uint PlayMode,
    uint StageMode,
    bool IsShin,
    uint MusicCategory,
    uint SelectedFolderId,
    bool IsFavorite,
    bool IsRecent,
    bool IsPapamama,
    bool IsPushed,
    uint SoulGauge,
    uint PlayDan,
    uint WaiwaiResult,
    uint WaiwaiGauge,
    CommonPlayResultData.GhostStageData? GhostStageData,
    DateTime PlayTime);

public sealed record Ac15BestUpdatePolicy(bool AllowScoreUpdate, bool AllowCrownUpdate);
```

Create `Application/Ac15/IAc15NormalPlayPersistence.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public interface IAc15NormalPlayPersistence
{
    ValueTask<bool> UserExistsAsync(uint baid, CancellationToken cancellationToken);

    ValueTask<Ac15SaveSnapshot> GetOrCreateSaveAsync(uint baid, CancellationToken cancellationToken);

    ValueTask AddPlayRowAsync(Ac15PlayRow row, CancellationToken cancellationToken);

    ValueTask UpsertBestAsync(uint baid, Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken cancellationToken);

    ValueTask SetFavoriteAsync(uint baid, uint songNo, bool isFavorite, int maxFavorites, CancellationToken cancellationToken);

    ValueTask UpsertRecentAsync(uint baid, uint songNo, DateTime playTime, CancellationToken cancellationToken);

    ValueTask TrimRecentAsync(uint baid, int maxRecent, CancellationToken cancellationToken);

    ValueTask SaveChangesAsync(CancellationToken cancellationToken);
}
```

- [ ] **Step 4: Add normal-play service**

Before creating the service, add this method to `Application/Ac15/IAc15EraHooks.cs`:

```csharp
Ac15BestUpdatePolicy GetBestUpdatePolicy(CommonPlayResultData.StageData stage, CrownType crown);
```

Add this implementation to `Application/Ac15/DefaultAc15EraHooks.cs`:

```csharp
public Ac15BestUpdatePolicy GetBestUpdatePolicy(CommonPlayResultData.StageData stage, CrownType crown)
    => new(AllowScoreUpdate: true, AllowCrownUpdate: true);
```

Create `Application/Ac15/Ac15NormalPlayService.cs`:

```csharp
using System.Globalization;
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15NormalPlayService
{
    public static async ValueTask<uint> SaveAsync(
        uint baid,
        CommonPlayResultData playResultData,
        Ac15EraProfile profile,
        IAc15NormalPlayPersistence persistence,
        IAc15EraHooks hooks,
        CancellationToken cancellationToken)
    {
        if (baid == 0)
        {
            return 1;
        }

        if (!await persistence.UserExistsAsync(baid, cancellationToken))
        {
            return 1;
        }

        var special = await hooks.TryHandleSpecialPlayModeAsync(
            playResultData,
            new Ac15SpecialModeContext(baid, profile.Era),
            cancellationToken);
        if (special.Action == Ac15SpecialModeAction.Handled)
        {
            return special.Result;
        }

        _ = await persistence.GetOrCreateSaveAsync(baid, cancellationToken);
        await hooks.BeforeNormalSaveAsync(new Ac15NormalSaveContext(baid, profile.Era, playResultData), cancellationToken);

        var playTime = ParsePlayDatetimeOrNow(playResultData.PlayDatetime);
        foreach (var stage in playResultData.AryStageInfoes)
        {
            if (!IsValidStage(stage, profile))
            {
                continue;
            }

            var hookDecision = hooks.IsSupportedStage(stage);
            if (!hookDecision.IsSupported)
            {
                continue;
            }

            var difficulty = MapDifficulty(stage.Level);
            var crown = MapCrown(stage.PlayResult);
            var isShin = stage.StageMode == 1 || stage.StageMode == 4;
            var bestPolicy = hooks.GetBestUpdatePolicy(stage, crown);
            var row = ToPlayRow(baid, playResultData.PlayMode, stage, difficulty, crown, isShin, playTime);
            await persistence.AddPlayRowAsync(row, cancellationToken);

            if (playResultData.PlayMode != (uint)PlayMode.DanMode || isShin)
            {
                await persistence.UpsertBestAsync(
                    baid,
                    new Ac15BestRow(stage.SongNo, difficulty, isShin, stage.PlayScore, stage.ScoreRate, crown),
                    bestPolicy,
                    cancellationToken);
            }

            await persistence.SetFavoriteAsync(baid, stage.SongNo, stage.IsFavorite, profile.Limits.MaxFavoriteSongs, cancellationToken);
            await persistence.UpsertRecentAsync(baid, stage.SongNo, playTime, cancellationToken);
        }

        await hooks.AfterNormalSaveAsync(new Ac15NormalSaveContext(baid, profile.Era, playResultData), cancellationToken);
        await persistence.SaveChangesAsync(cancellationToken);
        await persistence.TrimRecentAsync(baid, profile.Limits.MaxRecentSongs, cancellationToken);
        return 1;
    }

    private static bool IsValidStage(CommonPlayResultData.StageData stage, Ac15EraProfile profile)
        => stage.SongNo < profile.Limits.SongFlagBytes * 8
           && stage.Level >= profile.Limits.MinCourseLevel
           && stage.Level <= profile.Limits.MaxCourseLevel;

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

    private static DateTime ParsePlayDatetimeOrNow(string playDatetime)
    {
        var formats = new[] { Constants.DateTimeFormat, "yyyy-MM-dd HH:mm:ss" };
        return DateTime.TryParseExact(
            playDatetime,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : DateTime.Now;
    }
}
```

- [ ] **Step 5: Run normal-play core tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15NormalPlayServiceTests"
```

Expected: PASS.

## Task 2: Blue Normal Play Adapter And Special Hooks

**Files:**
- Create: `Application/Ac15/BlueAc15NormalPlayAdapter.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`

- [ ] **Step 1: Add Blue adapter**

Create `Application/Ac15/BlueAc15NormalPlayAdapter.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed class BlueAc15NormalPlayAdapter(ITaikoDbContext context) : IAc15NormalPlayPersistence
{
    public async ValueTask<bool> UserExistsAsync(uint baid, CancellationToken cancellationToken)
        => await context.UserData.FindAsync([baid], cancellationToken) is not null;

    public async ValueTask<Ac15SaveSnapshot> GetOrCreateSaveAsync(uint baid, CancellationToken cancellationToken)
    {
        _ = await context.GetOrCreateBlueSaveDataAsync(baid, cancellationToken);
        return new Ac15SaveSnapshot(baid);
    }

    public ValueTask AddPlayRowAsync(Ac15PlayRow row, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.SongPlayDataBlue.Add(new SongPlayDatumBlue
        {
            Baid = row.Baid,
            SongId = row.SongId,
            Difficulty = row.Difficulty,
            Crown = row.Crown,
            Score = row.Score,
            ScoreRate = row.ScoreRate,
            GoodCount = row.GoodCount,
            OkCount = row.OkCount,
            MissCount = row.MissCount,
            ComboCount = row.ComboCount,
            HitCount = row.HitCount,
            PoundCount = row.PoundCount,
            StarLevel = row.StarLevel,
            OptionFlg = row.OptionFlg,
            ToneFlg = row.ToneFlg,
            PlayMode = row.PlayMode,
            StageMode = row.StageMode,
            IsShin = row.IsShin,
            MusicCategory = row.MusicCategory,
            SelectedFolderId = row.SelectedFolderId,
            IsFavorite = row.IsFavorite,
            IsRecent = row.IsRecent,
            IsPapamama = row.IsPapamama,
            IsPushed = row.IsPushed,
            SoulGauge = row.SoulGauge,
            PlayDan = row.PlayDan,
            WaiwaiResult = row.WaiwaiResult,
            WaiwaiGauge = row.WaiwaiGauge,
            PlayTime = row.PlayTime
        });
        return ValueTask.CompletedTask;
    }

    public async ValueTask UpsertBestAsync(uint baid, Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken cancellationToken)
    {
        var existing = await context.SongBestDataBlue.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
        if (existing is null)
        {
            context.SongBestDataBlue.Add(new SongBestDatumBlue
            {
                Baid = baid,
                SongId = row.SongId,
                Difficulty = row.Difficulty,
                IsShin = row.IsShin,
                BestScore = row.BestScore,
                BestRate = row.BestRate,
                BestCrown = policy.AllowCrownUpdate ? row.BestCrown : CrownType.None
            });
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

    public async ValueTask SetFavoriteAsync(uint baid, uint songNo, bool isFavorite, int maxFavorites, CancellationToken cancellationToken)
    {
        var favorite = await context.BlueFavoriteSongs.FindAsync([baid, songNo], cancellationToken);
        if (isFavorite && favorite is null)
        {
            var persisted = await context.BlueFavoriteSongs
                .Where(song => song.Baid == baid)
                .Select(song => song.SongNo)
                .ToArrayAsync(cancellationToken);
            var tracked = context.BlueFavoriteSongs.Local
                .Where(song => song.Baid == baid)
                .Select(song => song.SongNo);
            if (persisted.Concat(tracked).Distinct().Count() < maxFavorites)
            {
                context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = baid, SongNo = songNo });
            }
        }
        else if (!isFavorite && favorite is not null)
        {
            context.BlueFavoriteSongs.Remove(favorite);
        }
    }

    public async ValueTask UpsertRecentAsync(uint baid, uint songNo, DateTime playTime, CancellationToken cancellationToken)
    {
        var recent = await context.BlueRecentSongs.FindAsync([baid, songNo], cancellationToken);
        if (recent is null)
        {
            context.BlueRecentSongs.Add(new BlueRecentSongs { Baid = baid, SongNo = songNo, LastPlayed = playTime });
        }
        else
        {
            recent.LastPlayed = playTime;
        }
    }

    public async ValueTask TrimRecentAsync(uint baid, int maxRecent, CancellationToken cancellationToken)
    {
        var overage = await context.BlueRecentSongs
            .Where(song => song.Baid == baid)
            .OrderByDescending(song => song.LastPlayed)
            .Skip(maxRecent)
            .ToListAsync(cancellationToken);
        context.BlueRecentSongs.RemoveRange(overage);
        await context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => new(context.SaveChangesAsync(cancellationToken));

    private static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };
}
```

- [ ] **Step 2: Replace Blue stage persistence with the adapter call**

In `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, keep the existing checks for `Baid == 0`, missing user, `IsTokkunPlayResult`, and `IsBattlePlayResult` before normal handling. Keep the existing medal, tutorial, option, costume, and unlock-bit save mutations.

Replace the `foreach (var stage in playResultData.AryStageInfoes)` block, the final `context.SaveChangesAsync`, and `TrimBlueRecentSongsAsync` call with:

```csharp
foreach (var stage in playResultData.AryStageInfoes)
{
    if (!IsSupportedBlueStage(request.Baid, stage))
    {
        continue;
    }

    BlueProfileCounters.ApplyStage(saveData, stage);
}

await SaveBlueDanAsync(saveData, playResultData, blue, cancellationToken);

return await Ac15NormalPlayService.SaveAsync(
    request.Baid,
    playResultData,
    Ac15EraProfiles.Blue,
    new BlueAc15NormalPlayAdapter(context),
    DefaultAc15EraHooks.Instance,
    cancellationToken);
```

Add `using TaikoLocalServer.Application.Ac15;`.

- [ ] **Step 3: Run Blue playresult regressions**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~BlueSelfBestTests|FullyQualifiedName~BlueCrownsDataTests"
```

Expected: PASS. Blue battle and Blue Tokkun tests must still prove no normal Blue score writes.

## Task 3: Green Normal Play Adapter And AI Battle Hooks

**Files:**
- Create: `Application/Ac15/GreenAc15NormalPlayAdapter.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`

- [ ] **Step 1: Add Green adapter and hooks**

Create `Application/Ac15/GreenAc15NormalPlayAdapter.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed class GreenAc15NormalPlayAdapter(ITaikoDbContext context) : IAc15NormalPlayPersistence
{
    public async ValueTask<bool> UserExistsAsync(uint baid, CancellationToken cancellationToken)
        => await context.UserData.FindAsync([baid], cancellationToken) is not null;

    public async ValueTask<Ac15SaveSnapshot> GetOrCreateSaveAsync(uint baid, CancellationToken cancellationToken)
    {
        _ = await context.GetOrCreateGreenSaveDataAsync(baid, cancellationToken);
        return new Ac15SaveSnapshot(baid);
    }

    public ValueTask AddPlayRowAsync(Ac15PlayRow row, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var play = new SongPlayDatumGreen
        {
            Baid = row.Baid,
            SongId = row.SongId,
            Difficulty = row.Difficulty,
            Crown = row.Crown,
            Score = row.Score,
            ScoreRate = row.ScoreRate,
            GoodCount = row.GoodCount,
            OkCount = row.OkCount,
            MissCount = row.MissCount,
            ComboCount = row.ComboCount,
            HitCount = row.HitCount,
            PoundCount = row.PoundCount,
            StarLevel = row.StarLevel,
            SupportLevel = row.SupportLevel,
            OptionFlg = row.OptionFlg,
            ToneFlg = row.ToneFlg,
            PlayMode = row.PlayMode,
            StageMode = row.StageMode,
            IsShin = row.IsShin,
            MusicCategory = row.MusicCategory,
            SelectedFolderId = row.SelectedFolderId,
            IsFavorite = row.IsFavorite,
            IsRecent = row.IsRecent,
            IsPapamama = row.IsPapamama,
            IsPushed = row.IsPushed,
            SoulGauge = row.SoulGauge,
            PlayDan = row.PlayDan,
            WaiwaiResult = row.WaiwaiResult,
            WaiwaiGauge = row.WaiwaiGauge,
            PlayTime = row.PlayTime
        };

        context.SongPlayDataGreen.Add(play);

        if (row.GhostStageData is not null)
        {
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

        return ValueTask.CompletedTask;
    }

    public async ValueTask UpsertBestAsync(uint baid, Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken cancellationToken)
    {
        var existing = await context.SongBestDataGreen.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
        if (existing is null)
        {
            context.SongBestDataGreen.Add(new SongBestDatumGreen
            {
                Baid = baid,
                SongId = row.SongId,
                Difficulty = row.Difficulty,
                IsShin = row.IsShin,
                BestScore = row.BestScore,
                BestRate = row.BestRate,
                BestCrown = policy.AllowCrownUpdate ? row.BestCrown : CrownType.None
            });
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

    public async ValueTask SetFavoriteAsync(uint baid, uint songNo, bool isFavorite, int maxFavorites, CancellationToken cancellationToken)
    {
        var favorite = await context.GreenFavoriteSongs.FindAsync([baid, songNo], cancellationToken);
        if (isFavorite && favorite is null)
        {
            var count = await context.GreenFavoriteSongs.CountAsync(song => song.Baid == baid, cancellationToken);
            if (count < maxFavorites)
            {
                context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = baid, SongNo = songNo });
            }
        }
        else if (!isFavorite && favorite is not null)
        {
            context.GreenFavoriteSongs.Remove(favorite);
        }
    }

    public async ValueTask UpsertRecentAsync(uint baid, uint songNo, DateTime playTime, CancellationToken cancellationToken)
    {
        var recent = await context.GreenRecentSongs.FindAsync([baid, songNo], cancellationToken);
        if (recent is null)
        {
            context.GreenRecentSongs.Add(new GreenRecentSongs { Baid = baid, SongNo = songNo, LastPlayed = playTime });
        }
        else
        {
            recent.LastPlayed = playTime;
        }
    }

    public async ValueTask TrimRecentAsync(uint baid, int maxRecent, CancellationToken cancellationToken)
    {
        var overage = await context.GreenRecentSongs
            .Where(song => song.Baid == baid)
            .OrderByDescending(song => song.LastPlayed)
            .Skip(maxRecent)
            .ToListAsync(cancellationToken);
        context.GreenRecentSongs.RemoveRange(overage);
        await context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => new(context.SaveChangesAsync(cancellationToken));

    private static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };
}

public sealed class GreenAc15NormalPlayHooks : IAc15EraHooks
{
    public ValueTask<Ac15SpecialModeResult> TryHandleSpecialPlayModeAsync(
        CommonPlayResultData request,
        Ac15SpecialModeContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(Ac15SpecialModeResult.ContinueNormal());
    }

    public Ac15StageSupportDecision IsSupportedStage(CommonPlayResultData.StageData stage)
        => stage.StageMode is 0 or 1 or 3 or 4
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not supported by Green");

    public Ac15BestUpdatePolicy GetBestUpdatePolicy(CommonPlayResultData.StageData stage, CrownType crown)
    {
        var isAiBattle = GreenStageModeInterpreter.IsAiBattle(stage.StageMode);
        var allowCrownUpdate = !isAiBattle || GreenAiBattleLevels.AllowsCrown(stage.Level, stage.SupportLevel);
        return new Ac15BestUpdatePolicy(AllowScoreUpdate: true, AllowCrownUpdate: allowCrownUpdate);
    }

    public ValueTask BeforeNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public ValueTask AfterNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public void BuildUserDataExtras(CommonUserDataResponse response, Ac15UserDataContext context)
    {
    }

    public void BuildInitialDataExtras(CommonInitialDataCheckResponse response, Ac15InitialDataContext context)
    {
    }
}
```

- [ ] **Step 2: Replace Green stage persistence with the adapter call**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, preserve the current missing-user and invalid-payload result behavior: invalid Green payloads still return `0`. Keep the existing medal, tutorial, option, costume, unlock-bit, ghost-update, and ghost-played-song mutations.

Replace the `foreach (var stage in playResultData.AryStageInfoes)` block, the final `context.SaveChangesAsync`, and `TrimGreenRecentSongsAsync` call with:

```csharp
foreach (var stage in playResultData.AryStageInfoes)
{
    GreenProfileCounters.ApplyStage(saveData, stage);
}

ApplyGhostPlayedSongBits(saveData, playResultData);
await SaveGreenDanAsync(saveData, playResultData, green, cancellationToken);

return await Ac15NormalPlayService.SaveAsync(
    request.Baid,
    playResultData,
    Ac15EraProfiles.Green,
    new GreenAc15NormalPlayAdapter(context),
    new GreenAc15NormalPlayHooks(),
    cancellationToken);
```

Add `using TaikoLocalServer.Application.Ac15;`.

- [ ] **Step 3: Run Green playresult regressions**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests|FullyQualifiedName~GreenAiBattlePlayResultTests|FullyQualifiedName~GreenItemShopPurchaseTests|FullyQualifiedName~GreenSelfBest|FullyQualifiedName~GreenProtocolBytesTests"
```

Expected: PASS. Green AI battle tests must still prove AI battle stage modes are accepted and Green-specific ghost side effects remain.

## Task 4: Full Regression And Commit

**Files:**
- All files listed in this stage.

- [ ] **Step 1: Run AC15, Blue, and Green test slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
```

Expected: PASS for all three commands.

- [ ] **Step 2: Run Host temp-output build**

Run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected: build exits `0`.

- [ ] **Step 3: Commit stage 6**

Run:

```powershell
git add Application/Ac15 Application/Handlers/UpdatePlayResultCommand.Blue.cs Application/Handlers/UpdatePlayResultCommand.Green.cs Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs Tests/Ac15 Tests/Blue Tests/Green
git commit -m "Extract AC15 normal play core"
```
