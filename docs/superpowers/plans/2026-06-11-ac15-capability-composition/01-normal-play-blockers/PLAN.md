# AC15 Normal Play Blockers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the two Phase 16.2 review blockers before larger refactoring: Green compact timestamps must persist consistently, and unsupported Green normal stages must be logged and skipped with protocol success.

**Architecture:** Add small switch-free `Application/Ac15` modules for timestamp parsing and normal stage filtering. Green, Blue, and Yellow handlers may call these modules directly while later stages replace table-binding switches.

**Tech Stack:** C# 13, .NET 10, EF Core SQLite, Mediator handlers, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15PlayDatetime.cs` - shared AC15 timestamp parser.
- `Application/Ac15/Ac15NormalStageFilter.cs` - shared bounds and policy filtering.
- `Application/Ac15/Ac15NormalStagePolicies.cs` - standard and Green AI-aware normal-stage policies.

Modify:

- `Application/Handlers/UpdatePlayResultCommand.Ac15.cs` - delegate timestamp parsing to `Ac15PlayDatetime`.
- `Application/Handlers/UpdatePlayResultCommand.Green.cs` - replace reject-on-any-invalid stage validation with log-skip-success filtering.
- `Application/Ac15/Ac15NormalPlayService.cs` - delegate timestamp parsing and stage filtering to the new modules until stage 2 replaces this service shape.
- `Tests/Green/GreenPlayResultHandlerTests.cs` - add behavior coverage for the two blockers.

## Task 1: Green Timestamp Consistency

**Files:**
- Modify: `Tests/Green/GreenPlayResultHandlerTests.cs`
- Create: `Application/Ac15/Ac15PlayDatetime.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Ac15.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Ac15/Ac15NormalPlayService.cs`

- [ ] **Step 1: Write the failing Green compact timestamp test**

Add this test to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_CompactTimestampMatchesSavePlayAndRecentRows()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            PlayDatetime = "20260514032442",
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 1,
                    StageMode = 0,
                    PlayResult = 1,
                    PlayScore = 1000,
                    IsRecent = true
                }
            ]
        }),
        CancellationToken.None);

    var expected = new DateTime(2026, 5, 14, 3, 24, 42);
    var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
    var play = await fixture.Context.SongPlayDataGreen.SingleAsync(row => row.Baid == 1 && row.SongId == 101);
    var recent = await fixture.Context.GreenRecentSongs.SingleAsync(row => row.Baid == 1 && row.SongNo == 101);

    Assert.Equal(1u, result);
    Assert.Equal(expected, save.LastPlayDatetime);
    Assert.Equal(expected, play.PlayTime);
    Assert.Equal(expected, recent.LastPlayed);
}
```

- [ ] **Step 2: Run the timestamp test and verify it fails**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_CompactTimestampMatchesSavePlayAndRecentRows"
```

Expected: FAIL because `UserSaveDataGreen.LastPlayDatetime` is parsed through `DateTime.TryParse` while play/recent rows use the AC15 compact parser.

- [ ] **Step 3: Add the shared timestamp parser**

Create `Application/Ac15/Ac15PlayDatetime.cs`:

```csharp
using System.Globalization;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15PlayDatetime
{
    private static readonly string[] Formats =
    [
        Constants.DateTimeFormat,
        "yyyy-MM-dd HH:mm:ss"
    ];

    public static DateTime ParseOrNow(string playDatetime)
        => DateTime.TryParseExact(
            playDatetime,
            Formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : DateTime.Now;
}
```

- [ ] **Step 4: Route existing AC15 timestamp parsing through `Ac15PlayDatetime`**

In `Application/Handlers/UpdatePlayResultCommand.Ac15.cs`, replace `ParseAc15PlayDatetimeOrNow` with:

```csharp
private static DateTime ParseAc15PlayDatetimeOrNow(string playDatetime)
    => Ac15PlayDatetime.ParseOrNow(playDatetime);
```

Remove the now-unused `using System.Globalization;` from that file.

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, replace:

```csharp
var playTime = DateTime.TryParse(playResultData.PlayDatetime, out var parsed)
    ? parsed
    : DateTime.Now;
```

with:

```csharp
var playTime = ParseAc15PlayDatetimeOrNow(playResultData.PlayDatetime);
```

In `Application/Ac15/Ac15NormalPlayService.cs`, replace the private `ParsePlayDatetimeOrNow` method body with:

```csharp
private static DateTime ParsePlayDatetimeOrNow(string playDatetime)
    => Ac15PlayDatetime.ParseOrNow(playDatetime);
```

Remove `using System.Globalization;` from `Application/Ac15/Ac15NormalPlayService.cs`.

- [ ] **Step 5: Run the timestamp test and verify it passes**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_CompactTimestampMatchesSavePlayAndRecentRows"
```

Expected: PASS.

## Task 2: Green Log-Skip-Success Stage Filtering

**Files:**
- Modify: `Tests/Green/GreenPlayResultHandlerTests.cs`
- Create: `Application/Ac15/Ac15NormalStageFilter.cs`
- Create: `Application/Ac15/Ac15NormalStagePolicies.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Modify: `Application/Ac15/Ac15NormalPlayService.cs`

- [ ] **Step 1: Write the failing Green mixed-stage skip test**

Add this test to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_SkipsUnsupportedNormalStagesAndPersistsValidStages()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            PlayDatetime = "20260514032442",
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 1,
                    StageMode = 0,
                    PlayResult = 1,
                    PlayScore = 1000
                },
                new CommonPlayResultData.StageData
                {
                    SongNo = 102,
                    Level = 1,
                    StageMode = 2,
                    PlayResult = 1,
                    PlayScore = 9000
                }
            ]
        }),
        CancellationToken.None);

    Assert.Equal(1u, result);
    var play = Assert.Single(await fixture.Context.SongPlayDataGreen.ToListAsync());
    Assert.Equal(101u, play.SongId);
    Assert.Empty(await fixture.Context.SongBestDataGreen.Where(row => row.SongId == 102).ToListAsync());
}
```

Add this companion test:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_AllUnsupportedNormalStagesReturnSuccessWithoutMutation()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 1024,
                    Level = 1,
                    StageMode = 0,
                    PlayResult = 1,
                    PlayScore = 9000
                },
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 1,
                    StageMode = 2,
                    PlayResult = 1,
                    PlayScore = 9000
                }
            ]
        }),
        CancellationToken.None);

    Assert.Equal(1u, result);
    Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
    Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
    Assert.Empty(await fixture.Context.GreenRecentSongs.ToListAsync());
}
```

- [ ] **Step 2: Run the stage filtering tests and verify they fail**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_SkipsUnsupportedNormalStagesAndPersistsValidStages|FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_AllUnsupportedNormalStagesReturnSuccessWithoutMutation"
```

Expected: FAIL because Green currently rejects a payload when any stage is unsupported or outside bounds.

- [ ] **Step 3: Add switch-free stage filter and policies**

Create `Application/Ac15/Ac15NormalStageFilter.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15NormalStagePolicy(
    Func<CommonPlayResultData.StageData, Ac15StageSupportDecision> IsSupported,
    Func<CommonPlayResultData.StageData, CrownType, Ac15BestUpdatePolicy> GetBestUpdatePolicy);

public static class Ac15NormalStageFilter
{
    public static IReadOnlyList<CommonPlayResultData.StageData> Filter(
        uint baid,
        IEnumerable<CommonPlayResultData.StageData> stages,
        Ac15ProtocolLimits limits,
        Ac15NormalStagePolicy policy,
        ILogger logger)
    {
        var accepted = new List<CommonPlayResultData.StageData>();
        foreach (var stage in stages)
        {
            if (stage.SongNo >= limits.SongFlagBytes * 8
                || stage.Level < limits.MinCourseLevel
                || stage.Level > limits.MaxCourseLevel)
            {
                logger.LogWarning(
                    "Skipping invalid AC15 stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                    baid,
                    stage.SongNo,
                    stage.Level,
                    stage.StageMode);
                continue;
            }

            var decision = policy.IsSupported(stage);
            if (!decision.IsSupported)
            {
                logger.LogWarning(
                    "Skipping unsupported AC15 stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode} reason={Reason}",
                    baid,
                    stage.SongNo,
                    stage.Level,
                    stage.StageMode,
                    decision.Reason ?? "unsupported");
                continue;
            }

            accepted.Add(stage);
        }

        return accepted;
    }
}
```

Create `Application/Ac15/Ac15NormalStagePolicies.cs`:

```csharp
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15NormalStagePolicies
{
    public static Ac15NormalStagePolicy Standard { get; } = new(
        stage => stage.StageMode is 0 or 1
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not a standard normal stage"),
        (_, _) => new Ac15BestUpdatePolicy(AllowScoreUpdate: true, AllowCrownUpdate: true));

    public static Ac15NormalStagePolicy Green { get; } = new(
        stage => stage.StageMode is 0 or 1 or 3 or 4
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not supported by Green"),
        (stage, _) =>
        {
            var isAiBattle = GreenStageModeInterpreter.IsAiBattle(stage.StageMode);
            var allowCrownUpdate = !isAiBattle || GreenAiBattleLevels.AllowsCrown(stage.Level, stage.SupportLevel);
            return new Ac15BestUpdatePolicy(AllowScoreUpdate: true, AllowCrownUpdate: allowCrownUpdate);
        });
}
```

- [ ] **Step 4: Use the stage filter in Green normal play orchestration**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, replace the invalid-payload check:

```csharp
if (HasInvalidAc15MedalTotals(currentDonmedal, saveData.TotalGetKatsumedal, playResultData)
    || playResultData.AryStageInfoes.Any(stage => !IsValidGreenStage(stage)))
{
    logger.LogWarning("Rejecting invalid Green playresult payload for baid {Baid}", request.Baid);
    return 0;
}
```

with:

```csharp
if (HasInvalidAc15MedalTotals(currentDonmedal, saveData.TotalGetKatsumedal, playResultData))
{
    logger.LogWarning("Rejecting invalid Green medal totals for baid {Baid}", request.Baid);
    return 1;
}

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
```

Remove `IsValidGreenStage` from `Application/Handlers/UpdatePlayResultCommand.Green.cs` after no callers remain.

- [ ] **Step 5: Align Blue and Yellow pre-filtering with the shared policy**

In `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, update `IsSupportedBlueStage` to call the standard policy after the Blue-specific log prefix:

```csharp
private bool IsSupportedBlueStage(uint baid, CommonPlayResultData.StageData stage)
{
    var accepted = Ac15NormalStageFilter.Filter(
        baid,
        [stage],
        Ac15EraProfiles.Blue.Limits,
        Ac15NormalStagePolicies.Standard,
        logger);
    return accepted.Count == 1;
}
```

In `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`, replace the body of `IsSupportedYellowNormalStage` with:

```csharp
private bool IsSupportedYellowNormalStage(uint baid, CommonPlayResultData.StageData stage)
{
    var accepted = Ac15NormalStageFilter.Filter(
        baid,
        [stage],
        Ac15EraProfiles.Yellow.Limits,
        Ac15NormalStagePolicies.Standard,
        logger);
    return accepted.Count == 1;
}
```

- [ ] **Step 6: Route `Ac15NormalPlayService` through the shared policy**

In `Application/Ac15/Ac15NormalPlayService.cs`, replace the `IsValidStage` call plus hook support check inside the stage loop with:

```csharp
var policy = profile.Era == GameEra.Green
    ? Ac15NormalStagePolicies.Green
    : Ac15NormalStagePolicies.Standard;

foreach (var stage in Ac15NormalStageFilter.Filter(baid, playResultData.AryStageInfoes, profile.Limits, policy, NullLogger.Instance))
{
    var difficulty = MapDifficulty(stage.Level);
    var crown = MapCrown(stage.PlayResult);
    var isShin = stage.StageMode == 1 || stage.StageMode == 4;
    var bestPolicy = policy.GetBestUpdatePolicy(stage, crown);
    var row = ToPlayRow(baid, playResultData.PlayMode, stage, difficulty, crown, isShin, playTime);
    AddPlayRow(context, profile, row, cancellationToken);

    if (playResultData.PlayMode != (uint)PlayMode.DanMode || isShin)
    {
        await UpsertBestAsync(
            context,
            profile,
            baid,
            new Ac15BestRow(stage.SongNo, difficulty, isShin, stage.PlayScore, stage.ScoreRate, crown),
            bestPolicy,
            cancellationToken);
    }

    await SetFavoriteAsync(context, profile, baid, stage.SongNo, stage.IsFavorite, profile.Limits.MaxFavoriteSongs, cancellationToken);
    await UpsertRecentAsync(context, profile, baid, stage.SongNo, playTime, cancellationToken);
}
```

Add `using Microsoft.Extensions.Logging.Abstractions;` to `Application/Ac15/Ac15NormalPlayService.cs`.

- [ ] **Step 7: Run focused Green tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_CompactTimestampMatchesSavePlayAndRecentRows|FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_SkipsUnsupportedNormalStagesAndPersistsValidStages|FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_AllUnsupportedNormalStagesReturnSuccessWithoutMutation|FullyQualifiedName~GreenAiBattlePlayResultTests"
```

Expected: PASS.

- [ ] **Step 8: Run AC15 and affected era slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15|FullyQualifiedName~GreenPlayResultHandlerTests|FullyQualifiedName~YellowPlayResultHandlerTests|FullyQualifiedName~BluePlayResultHandlerTests"
```

Expected: PASS.

- [ ] **Step 9: Commit stage 1**

Run:

```powershell
git add Application/Ac15/Ac15PlayDatetime.cs Application/Ac15/Ac15NormalStageFilter.cs Application/Ac15/Ac15NormalStagePolicies.cs Application/Handlers/UpdatePlayResultCommand.Ac15.cs Application/Handlers/UpdatePlayResultCommand.Green.cs Application/Handlers/UpdatePlayResultCommand.Blue.cs Application/Handlers/UpdatePlayResultCommand.Yellow.cs Application/Ac15/Ac15NormalPlayService.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Fix AC15 normal play blocker behavior"
```

## Self-Review

- Spec coverage: timestamp parsing and Green log-skip-success filtering are directly covered.
- Non-goals honored: no route changes, no shared tables, no source-shape tests.
- Type consistency: `Ac15NormalStagePolicy`, `Ac15NormalStageFilter`, and `Ac15NormalStagePolicies` are introduced here and reused by later stages.
