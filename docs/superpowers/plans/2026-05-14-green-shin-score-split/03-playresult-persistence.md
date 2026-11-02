# Task 3: Playresult Persistence

**Files:**
- Modify: `Tests/Green/GreenPlayResultHandlerTests.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`

## Steps

- [ ] **Step 1: Add failing tests for Shin split persistence and invalid stage mode**

Append these tests to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
    [Fact]
    public async Task UpdatePlayResult_Green_SavesNormalAndShinBestSeparately()
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
                        PlayScore = 229170,
                        GoodCnt = 49,
                        OkCnt = 12,
                        NgCnt = 1,
                        PoundCnt = 66,
                        ComboCnt = 54,
                        HitCnt = 127,
                        OptionFlg = [0, 0],
                        ToneFlg = new byte[16],
                        StarLevel = 2,
                        SoulGauge = 100
                    },
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 1,
                        PlayResult = 1,
                        PlayScore = 897650,
                        GoodCnt = 71,
                        OkCnt = 16,
                        NgCnt = 1,
                        PoundCnt = 73,
                        ComboCnt = 78,
                        HitCnt = 160,
                        OptionFlg = [0, 0],
                        ToneFlg = new byte[16],
                        StarLevel = 3,
                        SoulGauge = 100,
                        IsPapamama = true
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var normalBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        var shinBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, true);

        Assert.NotNull(normalBest);
        Assert.NotNull(shinBest);
        Assert.Equal(229170u, normalBest!.BestScore);
        Assert.Equal(897650u, shinBest!.BestScore);

        var plays = await fixture.Context.SongPlayDataGreen
            .Where(row => row.Baid == 1 && row.SongId == 101)
            .OrderBy(row => row.Id)
            .ToListAsync();

        Assert.Equal(2, plays.Count);
        Assert.False(plays[0].IsShin);
        Assert.Equal(0u, plays[0].StageMode);
        Assert.True(plays[1].IsShin);
        Assert.Equal(1u, plays[1].StageMode);
        Assert.True(plays[1].IsPapamama);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_RejectsUnknownStageMode()
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
                        SongNo = 101,
                        Level = 1,
                        StageMode = 2,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(0u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
    }
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_SavesNormalAndShinBestSeparately|FullyQualifiedName~UpdatePlayResult_Green_RejectsUnknownStageMode"
```

Expected: fails because `IsShin` is not set and `StageMode = 2` is not rejected.

- [ ] **Step 3: Add stage-mode validation and pass `IsShin` through save flow**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, add a constant:

```csharp
    private const uint MaxGreenStageMode = 1;
```

Update `IsValidGreenStage`:

```csharp
    private static bool IsValidGreenStage(CommonPlayResultData.StageData stage, IGreenCatalog green)
    {
        return stage.SongNo < GreenProtocolBytes.SongFlagBytes * 8
            && green.GreenMusicInfos.ContainsKey(stage.SongNo)
            && stage.Level <= MaxGreenCourseLevel
            && stage.StageMode <= MaxGreenStageMode
            && stage.PlayResult <= MaxGreenPlayResult
            && stage.PlayDan is null or (>= 1 and <= MaxGreenDanSlot);
    }
```

In `SaveStageAsync`, add this local before creating `SongPlayDatumGreen`:

```csharp
        var isShin = stage.StageMode == 1;
```

Add these assignments inside the `SongPlayDatumGreen` initializer:

```csharp
            StageMode = stage.StageMode,
            IsShin = isShin,
            IsPapamama = stage.IsPapamama,
```

Change the best upsert call:

```csharp
        await UpsertBestAsync(baid, stage, difficulty, crown, isShin, cancellationToken);
```

Change the method signature:

```csharp
    private async Task UpsertBestAsync(
        uint baid,
        CommonPlayResultData.StageData stage,
        Difficulty difficulty,
        CrownType crown,
        bool isShin,
        CancellationToken cancellationToken)
```

Change the lookup:

```csharp
        var existing = await context.SongBestDataGreen.FindAsync([baid, stage.SongNo, difficulty, isShin], cancellationToken);
```

Add `IsShin = isShin` when creating a new `SongBestDatumGreen`:

```csharp
                IsShin = isShin,
```

- [ ] **Step 4: Run persistence tests and confirm pass**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_SavesNormalAndShinBestSeparately|FullyQualifiedName~UpdatePlayResult_Green_RejectsUnknownStageMode"
```

Expected: pass.

- [ ] **Step 5: Commit persistence split**

Run:

```powershell
git add -- Tests/Green/GreenPlayResultHandlerTests.cs Application/Handlers/UpdatePlayResultCommand.Green.cs
git commit -m "Split Green playresult bests by shin mode"
```
