# AC15 Dani Capabilities Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert AC15 Dani save and readback into switch-free modules that operate on handler-supplied Dan score and Dan stage tables.

**Architecture:** Era handlers bind concrete Dan `DbSet`s, queries including stage rows, Mapperly delegates, catalog challenge rows, and typed save callbacks. `Application/Ac15` owns Dan validation, score aggregation, packed flag calculations, readback ordering, requested-id filtering, and response projection without selecting tables by `GameEra`.

**Tech Stack:** C# 13, .NET 10, EF Core SQLite, Mapperly, Mediator handlers, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15DaniWriter.cs` - switch-free Dani save workflow.
- `Application/Ac15/Ac15DaniReadback.cs` - switch-free Dan score readback workflow.
- `Tests/Ac15/Ac15DaniCapabilityTests.cs` - behavior tests for table binding and readback.

Modify:

- `Application/Ac15/Ac15DaniRecords.cs` - add `Ac15DaniTables<TScore,TStage>`.
- `Application/Ac15/Ac15DaniService.cs` - remove after callers move to writer/readback.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- `Application/Handlers/GetDanScoreQuery.Blue.cs`
- `Application/Handlers/GetDanScoreQuery.Green.cs`
- `Application/Handlers/GetDanScoreQuery.Yellow.cs`
- Existing Blue, Green, and Yellow Dani tests.

## Task 1: Table Bundle And Readback

**Files:**
- Modify: `Application/Ac15/Ac15DaniRecords.cs`
- Create: `Application/Ac15/Ac15DaniReadback.cs`
- Create: `Tests/Ac15/Ac15DaniCapabilityTests.cs`

- [ ] **Step 1: Write readback capability tests**

Create `Tests/Ac15/Ac15DaniCapabilityTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15DaniCapabilityTests
{
    [Fact]
    public async Task GetScoresAsync_ReadsOnlyTheBoundTableAndTruncatesByArrivalSongCount()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            MedleyUniqueId = 20005,
            ClearGrade = Ac15DanClearGrade.NormalClear,
            ArrivalSongCount = 1,
            SoulGaugeTotal = 150,
            ComboCountTotal = 300,
            DanStageScoreData =
            [
                StageBlue(0, 101, 1000),
                StageBlue(1, 102, 2000)
            ]
        });
        database.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            MedleyUniqueId = 30005,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            ArrivalSongCount = 1,
            SoulGaugeTotal = 999,
            ComboCountTotal = 999,
            DanStageScoreData = [StageGreen(0, 201, 9000)]
        });
        await database.Context.SaveChangesAsync();

        var scores = await Ac15DaniReadback.GetScoresAsync(
            BlueTables(database.Context),
            baid: 1,
            requestedDanIds: [5, 6],
            knownChallengeLevels: [5],
            CancellationToken.None);
        var response = Ac15DaniReadback.BuildResponse(scores);

        var dan = Assert.Single(response.AryDanScoreDatas);
        Assert.Equal(5u, dan.DanId);
        Assert.Equal(1u, dan.ArrivalSongCnt);
        var stage = Assert.Single(dan.AryDanScoreDataStages);
        Assert.Equal(1000u, stage.HighScore);
    }

    private static Ac15DaniTables<DanScoreDatumBlue, DanStageScoreDatumBlue> BlueTables(TaikoDbContext context)
        => new(
            context.DanScoreDataBlue,
            context.DanScoreDataBlue.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToBlueDanScoreDatum,
            Ac15DaniMapper.ApplyToBlueDanScoreDatum,
            Ac15DaniMapper.ToBlueDanStageScoreDatum,
            Ac15DaniMapper.ApplyToBlueDanStageScoreDatum);

    private static DanStageScoreDatumBlue StageBlue(uint index, uint songNo, uint score)
        => new()
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            StageIndex = index,
            SongNumber = songNo,
            PlayScore = score,
            HighScore = score,
            GoodCount = 10,
            OkCount = 2,
            BadCount = 1,
            DrumrollCount = 4,
            TotalHitCount = 13,
            ComboCount = 12
        };

    private static DanStageScoreDatumGreen StageGreen(uint index, uint songNo, uint score)
        => new()
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            StageIndex = index,
            SongNumber = songNo,
            PlayScore = score,
            HighScore = score,
            GoodCount = 10,
            OkCount = 2,
            BadCount = 1,
            DrumrollCount = 4,
            TotalHitCount = 13,
            ComboCount = 12
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

- [ ] **Step 2: Run readback tests and verify compile failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15DaniCapabilityTests"
```

Expected: compile failure because `Ac15DaniTables<TScore,TStage>` and `Ac15DaniReadback` do not exist.

- [ ] **Step 3: Add Dani table bundle**

Add this record to `Application/Ac15/Ac15DaniRecords.cs`:

```csharp
public sealed record Ac15DaniTables<TScore, TStage>(
    DbSet<TScore> Scores,
    IQueryable<TScore> ScoresWithStages,
    Func<TScore, ICollection<TStage>> GetStages,
    Func<TScore, Ac15DaniScore> ToScore,
    Func<TScore, Ac15DaniScoreSummary> ToSummary,
    Func<Ac15DaniScore, TScore> CreateScore,
    Action<Ac15DaniScore, TScore> ApplyScore,
    Func<Ac15DaniStageScore, Ac15DaniScore, TStage> CreateStage,
    Action<Ac15DaniStageScore, TStage> ApplyStage)
    where TScore : class, IAc15DanScoreDatum
    where TStage : class, IAc15DanStageScoreDatum;
```

- [ ] **Step 4: Add switch-free Dani readback**

Create `Application/Ac15/Ac15DaniReadback.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class Ac15DaniReadback
{
    public static async ValueTask<IReadOnlyList<Ac15DaniScore>> GetScoresAsync<TScore, TStage>(
        Ac15DaniTables<TScore, TStage> tables,
        uint baid,
        IReadOnlySet<uint> requestedDanIds,
        IReadOnlySet<uint> knownChallengeLevels,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        where TStage : class, IAc15DanStageScoreDatum
    {
        var validRequestedIds = requestedDanIds.Where(knownChallengeLevels.Contains).ToHashSet();
        var rows = await tables.ScoresWithStages
            .Where(row => row.Baid == baid && validRequestedIds.Contains(row.DanId))
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(row => row.DanId)
            .Select(tables.ToScore)
            .ToArray();
    }

    public static CommonDanScoreDataResponse BuildResponse(IReadOnlyList<Ac15DaniScore> rows)
    {
        var response = new CommonDanScoreDataResponse { Result = 1 };
        foreach (var row in rows.OrderBy(row => row.DanId))
        {
            var responseData = new CommonDanScoreDataResponse.DanScoreData
            {
                DanId = row.DanId,
                ArrivalSongCnt = row.ArrivalSongCount,
                SoulGaugeTotal = row.SoulGaugeTotal,
                ComboCntTotal = row.ComboCountTotal
            };

            foreach (var stage in row.Stages.OrderBy(stage => stage.StageIndex).Take((int)row.ArrivalSongCount))
            {
                responseData.AryDanScoreDataStages.Add(new CommonDanScoreDataResponse.DanScoreDataStage
                {
                    PlayScore = stage.PlayScore,
                    GoodCnt = stage.GoodCount,
                    OkCnt = stage.OkCount,
                    NgCnt = stage.BadCount,
                    PoundCnt = stage.DrumrollCount,
                    HitCnt = stage.TotalHitCount,
                    ComboCnt = stage.ComboCount,
                    HighScore = stage.HighScore
                });
            }

            response.AryDanScoreDatas.Add(responseData);
        }

        return response;
    }
}
```

- [ ] **Step 5: Run readback tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15DaniCapabilityTests"
```

Expected: PASS.

## Task 2: Dani Writer Without Table Switches

**Files:**
- Create: `Application/Ac15/Ac15DaniWriter.cs`
- Modify: `Application/Ac15/Ac15DaniService.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Modify: existing Blue, Green, and Yellow Dani save tests.

- [ ] **Step 1: Add a Dani save behavior test that uses a bound table bundle**

Add this test to `Tests/Ac15/Ac15DaniCapabilityTests.cs`:

```csharp
[Fact]
public async Task SaveAsync_WritesOnlyTheBoundDaniTables()
{
    await using var database = await SchemaDatabase.CreateAsync();
    var saveState = new Ac15DaniSaveState(1, DisplayDan: 1, IsAutoCostumeOn: false, DanCostumeId: 36);
    Ac15DaniSaveUpdate? saveUpdate = null;

    await Ac15DaniWriter.SaveAsync(
        BlueTables(database.Context),
        PlayResultDanClear(danId: 5),
        Ac15EraProfiles.Blue.Limits,
        challenges: [new Ac15DaniChallenge(5, 20005)],
        saveState,
        update => saveUpdate = update,
        NullLogger.Instance,
        CancellationToken.None);
    await database.Context.SaveChangesAsync();

    Assert.Single(await database.Context.DanScoreDataBlue.ToListAsync());
    Assert.Empty(await database.Context.DanScoreDataGreen.ToListAsync());
    Assert.NotNull(saveUpdate);
    Assert.Equal(5u, saveUpdate!.GotDanMax);
    Assert.Equal(6u, saveUpdate.DisplayDan);
}

private static CommonPlayResultData PlayResultDanClear(uint danId)
    => new()
    {
        PlayMode = (uint)PlayMode.DanMode,
        DanResult = (uint)Ac15DanClearGrade.NormalClear,
        ComboCntTotal = 300,
        AryStageInfoes =
        [
            new()
            {
                SongNo = 101,
                Level = 1,
                PlayScore = 1000,
                GoodCnt = 10,
                OkCnt = 2,
                NgCnt = 1,
                PoundCnt = 4,
                HitCnt = 13,
                ComboCnt = 12,
                SoulGauge = 150,
                PlayDan = danId
            }
        ]
    };
```

Add `using Microsoft.Extensions.Logging.Abstractions;` to `Tests/Ac15/Ac15DaniCapabilityTests.cs`.

- [ ] **Step 2: Run the Dani writer test and verify compile failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15DaniCapabilityTests.SaveAsync_WritesOnlyTheBoundDaniTables"
```

Expected: compile failure because `Ac15DaniWriter` does not exist.

- [ ] **Step 3: Create `Ac15DaniWriter` by moving existing switch-free algorithms**

Create `Application/Ac15/Ac15DaniWriter.cs` with this public entry point:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class Ac15DaniWriter
{
    public static async ValueTask SaveAsync<TScore, TStage>(
        Ac15DaniTables<TScore, TStage> tables,
        CommonPlayResultData playResultData,
        Ac15ProtocolLimits limits,
        IEnumerable<Ac15DaniChallenge> challenges,
        Ac15DaniSaveState saveState,
        Action<Ac15DaniSaveUpdate> applySaveUpdate,
        ILogger logger,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        where TStage : class, IAc15DanStageScoreDatum
    {
        if (playResultData.PlayMode != (uint)PlayMode.DanMode)
        {
            return;
        }

        var danIds = playResultData.AryStageInfoes
            .Select(stage => stage.PlayDan.GetValueOrDefault())
            .Where(dan => dan != 0)
            .Distinct()
            .ToArray();

        if (danIds.Length != 1)
        {
            logger.LogWarning("Skipping AC15 Dani save for baid {Baid}: expected one PlayDan value, got {Count}", saveState.Baid, danIds.Length);
            return;
        }

        if (playResultData.DanResult > (uint)Ac15DanClearGrade.GoldClear)
        {
            logger.LogWarning("Skipping AC15 Dani save for baid {Baid}: invalid DanResult {DanResult}", saveState.Baid, playResultData.DanResult);
            return;
        }

        var danId = danIds[0];
        var knownChallengeLevels = challenges.Select(row => row.DanId).ToHashSet();
        var challenge = challenges.FirstOrDefault(row => row.DanId == danId);
        if (challenge is null
            || !Ac15DanHelpers.IsKnownDanId(danId, limits)
            || !knownChallengeLevels.Contains(danId))
        {
            logger.LogWarning("Skipping AC15 Dani save for baid {Baid}: unknown Dan id {DanId}", saveState.Baid, danId);
            return;
        }

        var isExtra = Ac15DanHelpers.IsExtraDanId(danId, limits);
        var key = new Ac15DaniScoreKey(saveState.Baid, danId, isExtra);
        var existing = await GetScoreAsync(tables, key, cancellationToken);
        var updatedScore = BuildUpdatedScore(saveState.Baid, existing, danId, isExtra, challenge.MedleyUniqueId, playResultData);
        var summaries = (await GetScoreSummariesAsync(tables, saveState.Baid, cancellationToken)).ToList();

        await UpsertScoreAsync(tables, updatedScore, cancellationToken);

        summaries.RemoveAll(row => row.DanId == updatedScore.DanId && row.IsExtra == updatedScore.IsExtra);
        summaries.Add(new Ac15DaniScoreSummary(updatedScore.DanId, updatedScore.IsExtra, updatedScore.ClearGrade));

        applySaveUpdate(BuildSaveUpdate(updatedScore, summaries, saveState, limits, Ac15DanHelpers.ClampGrade(playResultData.DanResult)));
    }
}
```

Move these existing private methods from `Application/Ac15/Ac15DaniService.cs` into `Ac15DaniWriter.cs` unchanged except for replacing `context/profile` parameters with `Ac15DaniTables<TScore,TStage>`:

```csharp
GetScoreAsync<TScore,TStage>
GetScoreSummariesAsync<TScore,TStage>
UpsertScoreAsync<TScore,TStage>
UpsertStages<TStage>
BuildUpdatedScore
BuildUpdatedStage
BuildSaveUpdate
```

Use these method bodies when converting the table access:

```csharp
private static async ValueTask<Ac15DaniScore?> GetScoreAsync<TScore, TStage>(
    Ac15DaniTables<TScore, TStage> tables,
    Ac15DaniScoreKey key,
    CancellationToken cancellationToken)
    where TScore : class, IAc15DanScoreDatum
    where TStage : class, IAc15DanStageScoreDatum
    => await tables.ScoresWithStages.SingleOrDefaultAsync(
        score => score.Baid == key.Baid && score.DanId == key.DanId && score.IsExtra == key.IsExtra,
        cancellationToken) is { } row
        ? tables.ToScore(row)
        : null;

private static async ValueTask<IReadOnlyList<Ac15DaniScoreSummary>> GetScoreSummariesAsync<TScore, TStage>(
    Ac15DaniTables<TScore, TStage> tables,
    uint baid,
    CancellationToken cancellationToken)
    where TScore : class, IAc15DanScoreDatum
    where TStage : class, IAc15DanStageScoreDatum
    => (await tables.Scores.Where(row => row.Baid == baid).ToListAsync(cancellationToken))
        .Select(tables.ToSummary)
        .ToArray();

private static async ValueTask UpsertScoreAsync<TScore, TStage>(
    Ac15DaniTables<TScore, TStage> tables,
    Ac15DaniScore score,
    CancellationToken cancellationToken)
    where TScore : class, IAc15DanScoreDatum
    where TStage : class, IAc15DanStageScoreDatum
{
    var row = await tables.ScoresWithStages.SingleOrDefaultAsync(
        existing => existing.Baid == score.Baid && existing.DanId == score.DanId && existing.IsExtra == score.IsExtra,
        cancellationToken);
    if (row is null)
    {
        row = tables.CreateScore(score);
        UpsertStages(tables.GetStages(row), score, tables.CreateStage, tables.ApplyStage);
        tables.Scores.Add(row);
        return;
    }

    tables.ApplyScore(score, row);
    UpsertStages(tables.GetStages(row), score, tables.CreateStage, tables.ApplyStage);
}
```

- [ ] **Step 4: Run the Dani writer test**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15DaniCapabilityTests.SaveAsync_WritesOnlyTheBoundDaniTables"
```

Expected: PASS.

## Task 3: Bind Dani Tables In Era Handlers

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Modify: `Application/Handlers/GetDanScoreQuery.Blue.cs`
- Modify: `Application/Handlers/GetDanScoreQuery.Green.cs`
- Modify: `Application/Handlers/GetDanScoreQuery.Yellow.cs`
- Delete: `Application/Ac15/Ac15DaniService.cs`

- [ ] **Step 1: Add handler table binders**

Add this helper to the Blue partials that save or read Dani:

```csharp
private Ac15DaniTables<DanScoreDatumBlue, DanStageScoreDatumBlue> BlueDaniTables()
    => new(
        context.DanScoreDataBlue,
        context.DanScoreDataBlue.Include(score => score.DanStageScoreData),
        score => score.DanStageScoreData,
        Ac15DaniMapper.ToAc15DaniScore,
        Ac15DaniMapper.ToAc15DaniScoreSummary,
        Ac15DaniMapper.ToBlueDanScoreDatum,
        Ac15DaniMapper.ApplyToBlueDanScoreDatum,
        Ac15DaniMapper.ToBlueDanStageScoreDatum,
        Ac15DaniMapper.ApplyToBlueDanStageScoreDatum);
```

Add Green and Yellow equivalents by replacing the entity and Mapperly delegate names with `Green` and `Yellow`:

```csharp
private Ac15DaniTables<DanScoreDatumGreen, DanStageScoreDatumGreen> GreenDaniTables()
    => new(
        context.DanScoreDataGreen,
        context.DanScoreDataGreen.Include(score => score.DanStageScoreData),
        score => score.DanStageScoreData,
        Ac15DaniMapper.ToAc15DaniScore,
        Ac15DaniMapper.ToAc15DaniScoreSummary,
        Ac15DaniMapper.ToGreenDanScoreDatum,
        Ac15DaniMapper.ApplyToGreenDanScoreDatum,
        Ac15DaniMapper.ToGreenDanStageScoreDatum,
        Ac15DaniMapper.ApplyToGreenDanStageScoreDatum);

private Ac15DaniTables<DanScoreDatumYellow, DanStageScoreDatumYellow> YellowDaniTables()
    => new(
        context.DanScoreDataYellow,
        context.DanScoreDataYellow.Include(score => score.DanStageScoreData),
        score => score.DanStageScoreData,
        Ac15DaniMapper.ToAc15DaniScore,
        Ac15DaniMapper.ToAc15DaniScoreSummary,
        Ac15DaniMapper.ToYellowDanScoreDatum,
        Ac15DaniMapper.ApplyToYellowDanScoreDatum,
        Ac15DaniMapper.ToYellowDanStageScoreDatum,
        Ac15DaniMapper.ApplyToYellowDanStageScoreDatum);
```

- [ ] **Step 2: Replace playresult Dani save calls**

In Blue, Green, and Yellow `UpdatePlayResultCommand` partials, replace:

```csharp
await Ac15DaniService.SaveAsync(
    context,
    playResultData,
    Ac15EraProfiles.Blue,
    blue.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
    new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, BlueDanCostumeId),
    update =>
    {
        saveData.GotDanFlg = update.GotDanFlg;
        saveData.GotDanExtraFlg = update.GotDanExtraFlg;
        saveData.GotDanMax = update.GotDanMax;
        saveData.DispTaikojukuDan = update.DisplayDan;
        if (update.ApplyDanCostume)
        {
            saveData.Costume1 = update.DanCostumeId;
            saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [update.DanCostumeId], BlueProtocolBytes.CostumeFlagBytes);
        }
    },
    logger,
    cancellationToken);
```

with:

```csharp
await Ac15DaniWriter.SaveAsync(
    BlueDaniTables(),
    playResultData,
    Ac15EraProfiles.Blue.Limits,
    blue.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
    new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, BlueDanCostumeId),
    update =>
    {
        saveData.GotDanFlg = update.GotDanFlg;
        saveData.GotDanExtraFlg = update.GotDanExtraFlg;
        saveData.GotDanMax = update.GotDanMax;
        saveData.DispTaikojukuDan = update.DisplayDan;
        if (update.ApplyDanCostume)
        {
            saveData.Costume1 = update.DanCostumeId;
            saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [update.DanCostumeId], BlueProtocolBytes.CostumeFlagBytes);
        }
    },
    logger,
    cancellationToken);
```

Use `GreenDaniTables()` and `Ac15EraProfiles.Green.Limits` in Green. Use `YellowDaniTables()` and `Ac15EraProfiles.Yellow.Limits` in Yellow.

- [ ] **Step 3: Replace Dan score readback partial bodies**

In each `GetDanScoreQuery.*.cs`, reduce the era partial body to table binding plus shared readback. Blue:

```csharp
private partial async ValueTask<CommonDanScoreDataResponse> HandleBlue(
    GetDanScoreQuery request,
    CancellationToken cancellationToken)
{
    var limits = Ac15EraProfiles.Blue.Limits;
    var knownChallengeLevels = gameDataService.Blue().TaikojukuFileOrder
        .Select(pack => pack.ChallengeLevel)
        .Where(id => Ac15DanHelpers.IsKnownDanId(id, limits))
        .ToHashSet();

    var rows = await Ac15DaniReadback.GetScoresAsync(
        BlueDaniTables(),
        request.Baid,
        request.DanIds.ToHashSet(),
        knownChallengeLevels,
        cancellationToken);

    return Ac15DaniReadback.BuildResponse(rows);
}
```

Apply the same structure in Green and Yellow with the corresponding catalog, profile limits, and table binder.

- [ ] **Step 4: Delete the old switched service**

Delete `Application/Ac15/Ac15DaniService.cs` after no callers remain.

- [ ] **Step 5: Run Dani behavior tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15DaniCapabilityTests|FullyQualifiedName~BlueDanScoreTests|FullyQualifiedName~BlueAdminApiDaniTests|FullyQualifiedName~GreenPlayResultHandlerTests|FullyQualifiedName~YellowDaniTests"
```

Expected: PASS.

- [ ] **Step 6: Run AC15 and era slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Yellow"
```

Expected: PASS for all four commands.

- [ ] **Step 7: Commit stage 3**

Run:

```powershell
git add Application/Ac15/Ac15DaniRecords.cs Application/Ac15/Ac15DaniWriter.cs Application/Ac15/Ac15DaniReadback.cs Application/Ac15/Ac15DaniService.cs Application/Handlers/UpdatePlayResultCommand.Blue.cs Application/Handlers/UpdatePlayResultCommand.Green.cs Application/Handlers/UpdatePlayResultCommand.Yellow.cs Application/Handlers/GetDanScoreQuery.Blue.cs Application/Handlers/GetDanScoreQuery.Green.cs Application/Handlers/GetDanScoreQuery.Yellow.cs Tests/Ac15/Ac15DaniCapabilityTests.cs Tests/Blue Tests/Green Tests/Yellow
git commit -m "Compose AC15 Dani through bound tables"
```

## Self-Review

- Spec coverage: Dani save/readback use `Ac15DaniTables<TScore,TStage>`, handlers bind concrete era tables, stage ordering and `ArrivalSongCount` truncation stay behavior-tested.
- Non-goals honored: no shared Dan EF table, no repository-shaped Dan persistence, no `GameEra` table selection inside shared Dani code.
- Type consistency: `Ac15DaniTables<TScore,TStage>`, `Ac15DaniWriter`, and `Ac15DaniReadback` are the only new shared Dani entry points.
