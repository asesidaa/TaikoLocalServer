# Task 3: Playresult Dan Persistence

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

- [ ] **Step 1: Write failing first-Dan save test**

Add this test to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_DaniPlaySavesDanDataAndNormalBest()
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
            PlayDatetime = "20260515060858",
            PlayMode = 1,
            DanResult = 2,
            AryStageInfoes =
            [
                new() { SongNo = 101, Level = 1, PlayResult = 0, PlayScore = 326090, GoodCnt = 124, OkCnt = 14, NgCnt = 0, PoundCnt = 139, ComboCnt = 138, HitCnt = 277, PlayDan = 1, SoulGauge = 51 },
                new() { SongNo = 102, Level = 1, PlayResult = 0, PlayScore = 593280, GoodCnt = 230, OkCnt = 33, NgCnt = 3, PoundCnt = 287, ComboCnt = 156, HitCnt = 550, PlayDan = 1, SoulGauge = 99 },
                new() { SongNo = 103, Level = 1, PlayResult = 0, PlayScore = 818490, GoodCnt = 314, OkCnt = 49, NgCnt = 6, PoundCnt = 342, ComboCnt = 156, HitCnt = 705, PlayDan = 1, SoulGauge = 100 }
            ]
        }),
        CancellationToken.None);

    Assert.Equal(1u, result);

    var dan = await fixture.Context.DanScoreDataGreen
        .Include(row => row.DanStageScoreData)
        .SingleAsync(row => row.Baid == 1 && row.DanId == 1 && !row.IsExtra);

    Assert.Equal(20001u, dan.MedleyUniqueId);
    Assert.Equal(GreenDanClearGrade.GoldClear, dan.ClearGrade);
    Assert.Equal(3u, dan.ArrivalSongCount);
    Assert.Equal(3, dan.DanStageScoreData.Count);
    Assert.Contains(dan.DanStageScoreData, row => row.StageIndex == 0 && row.SongNumber == 101 && row.HighScore == 326090);

    var normalBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy, false);
    Assert.NotNull(normalBest);
    Assert.Equal(326090u, normalBest!.BestScore);
}
```

- [ ] **Step 2: Run the test and verify it fails**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_DaniPlaySavesDanDataAndNormalBest"
```

Expected: FAIL because no Green Dan row is saved.

- [ ] **Step 3: Add invalid Dan result test**

Add this test:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_DaniRejectsInvalidDanResultButKeepsNormalPlaySave()
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
            PlayMode = 1,
            DanResult = 3,
            AryStageInfoes =
            [
                new() { SongNo = 101, Level = 1, PlayScore = 123, PlayDan = 1 }
            ]
        }),
        CancellationToken.None);

    Assert.Equal(1u, result);
    Assert.Empty(await fixture.Context.DanScoreDataGreen.ToListAsync());
    Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
}
```

- [ ] **Step 4: Add duplicate-stage-index test**

Add this test:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_DaniDuplicateSongsUseStageIndexRows()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            PlayMode = 1,
            DanResult = 1,
            AryStageInfoes =
            [
                new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 1 },
                new() { SongNo = 101, Level = 1, PlayScore = 200, PlayDan = 1 }
            ]
        }),
        CancellationToken.None);

    var stages = await fixture.Context.DanStageScoreDataGreen
        .Where(row => row.Baid == 1 && row.DanId == 1)
        .OrderBy(row => row.StageIndex)
        .ToListAsync();

    Assert.Equal(2, stages.Count);
    Assert.Equal(0u, stages[0].StageIndex);
    Assert.Equal(1u, stages[1].StageIndex);
}
```

- [ ] **Step 5: Implement Dani detection and upsert helpers**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, call `SaveGreenDanAsync` after the stage save loop and before `SaveChangesAsync`:

```csharp
foreach (var stage in playResultData.AryStageInfoes)
{
    await SaveStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
}

await SaveGreenDanAsync(saveData, playResultData, green, cancellationToken);
```

Add these methods to the class:

```csharp
private async Task SaveGreenDanAsync(
    UserSaveDataGreen saveData,
    CommonPlayResultData playResultData,
    IGreenCatalog green,
    CancellationToken cancellationToken)
{
    if (playResultData.PlayMode != 1)
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
        logger.LogWarning("Skipping Green Dani save for baid {Baid}: expected one PlayDan value, got {Count}", saveData.Baid, danIds.Length);
        return;
    }

    if (playResultData.DanResult > (uint)GreenDanClearGrade.GoldClear)
    {
        logger.LogWarning("Skipping Green Dani save for baid {Baid}: invalid DanResult {DanResult}", saveData.Baid, playResultData.DanResult);
        return;
    }

    var danId = danIds[0];
    var pack = green.TaikojukuFileOrder.FirstOrDefault(row => row.ChallengeLevel == danId);
    if (pack is null || !GreenDanHelpers.IsKnownGreenDanId(danId))
    {
        logger.LogWarning("Skipping Green Dani save for baid {Baid}: unknown Dan id {DanId}", saveData.Baid, danId);
        return;
    }

    var isExtra = GreenDanHelpers.IsExtraDanId(danId);
    var danScore = await context.DanScoreDataGreen
        .Include(row => row.DanStageScoreData)
        .SingleOrDefaultAsync(row => row.Baid == saveData.Baid && row.DanId == danId && row.IsExtra == isExtra, cancellationToken);

    if (danScore is null)
    {
        danScore = new DanScoreDatumGreen
        {
            Baid = saveData.Baid,
            DanId = danId,
            IsExtra = isExtra,
            MedleyUniqueId = pack.UniqueId
        };
        context.DanScoreDataGreen.Add(danScore);
    }

    UpdateGreenDanScore(danScore, playResultData);
    await UpdateGreenDanSummaryAsync(saveData, cancellationToken);
}

private void UpdateGreenDanScore(DanScoreDatumGreen danScore, CommonPlayResultData playResultData)
{
    danScore.ClearGrade = GreenDanHelpers.ClampGrade(Math.Max((uint)danScore.ClearGrade, playResultData.DanResult));
    danScore.ArrivalSongCount = Math.Max(danScore.ArrivalSongCount, (uint)playResultData.AryStageInfoes.Count);
    danScore.ComboCountTotal = Math.Max(danScore.ComboCountTotal, playResultData.ComboCntTotal);
    danScore.SoulGaugeTotal = Math.Max(danScore.SoulGaugeTotal, playResultData.SoulGaugeTotal);

    for (var i = 0; i < playResultData.AryStageInfoes.Count; i++)
    {
        var stage = playResultData.AryStageInfoes[i];
        var stageIndex = (uint)i;
        var existing = danScore.DanStageScoreData.FirstOrDefault(row => row.StageIndex == stageIndex);
        if (existing is null)
        {
            existing = new DanStageScoreDatumGreen
            {
                Baid = danScore.Baid,
                DanId = danScore.DanId,
                IsExtra = danScore.IsExtra,
                StageIndex = stageIndex,
                SongNumber = stage.SongNo,
                BadCount = stage.NgCnt
            };
            danScore.DanStageScoreData.Add(existing);
        }

        existing.SongNumber = stage.SongNo;
        existing.PlayScore = Math.Max(existing.PlayScore, stage.PlayScore);
        existing.HighScore = Math.Max(existing.HighScore, stage.PlayScore);
        existing.ComboCount = Math.Max(existing.ComboCount, stage.ComboCnt);
        existing.DrumrollCount = Math.Max(existing.DrumrollCount, stage.PoundCnt);
        existing.GoodCount = Math.Max(existing.GoodCount, stage.GoodCnt);
        existing.OkCount = Math.Max(existing.OkCount, stage.OkCnt);
        existing.TotalHitCount = Math.Max(existing.TotalHitCount, stage.HitCnt);
        existing.BadCount = Math.Min(existing.BadCount, stage.NgCnt);
    }
}
```

Task 4 owns the real BAID flag projection. For this task, add a temporary private method that returns completed work:

```csharp
private ValueTask UpdateGreenDanSummaryAsync(UserSaveDataGreen saveData, CancellationToken cancellationToken)
    => ValueTask.CompletedTask;
```

- [ ] **Step 6: Run Task 3 tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_Dani"
```

Expected: PASS for the new Dani persistence tests.

- [ ] **Step 7: Commit Task 3**

```powershell
git add Application/Handlers/UpdatePlayResultCommand.Green.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Save Green Dani play results"
```
