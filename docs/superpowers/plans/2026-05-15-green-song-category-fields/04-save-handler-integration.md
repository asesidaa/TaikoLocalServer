# Task 4: Save-Handler Integration

**Goal:** Wire the new counters and `IsPushed` into the Green playresult save path: reject stages with `music_categ > 7`, invoke `GreenProfileCounters.ApplyStage` per stage, and persist `IsPushed` on the play log row.

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

**Acceptance Criteria:**
- [ ] `IsValidGreenStage` rejects stages where `stage.MusicCateg > 7`.
- [ ] A play with an out-of-range `music_categ` returns `Result=0` and writes nothing.
- [ ] Each valid stage increments the appropriate `Categ*Cnt` and any active `Song(Pushed|Favorite|Recent)Cnt` counters on `UserSaveDataGreen`.
- [ ] `SongPlayDatumGreen.IsPushed` is persisted from `stage.IsPushed`.
- [ ] Existing tests still pass; no regression in dan/favorite/recent paths handled by other tasks.

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests"` → all pass.

---

- [ ] **Step 1: Write the failing integration tests**

Append these tests to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_IncrementsGenreAndSongCounters()
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
            PlayDatetime = "2026-05-15 12:00:00",
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 1,
                    PlayResult = 1,
                    PlayScore = 100000,
                    GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                    OptionFlg = [0], ToneFlg = [0],
                    MusicCateg = 0,
                    IsPushed = true,
                    IsFavorite = true,
                    IsRecent = false
                },
                new CommonPlayResultData.StageData
                {
                    SongNo = 102,
                    Level = 1,
                    PlayResult = 1,
                    PlayScore = 100000,
                    GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                    OptionFlg = [0], ToneFlg = [0],
                    MusicCateg = 1,
                    IsRecent = true
                }
            ]
        }),
        CancellationToken.None);

    Assert.Equal(1u, result);
    var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
    Assert.Equal(1u, save.CategJpopCnt);
    Assert.Equal(1u, save.CategAnimeCnt);
    Assert.Equal(0u, save.CategGameCnt);
    Assert.Equal(1u, save.SongPushedCnt);
    Assert.Equal(1u, save.SongFavoriteCnt);
    Assert.Equal(1u, save.SongRecentCnt);
}

[Fact]
public async Task UpdatePlayResult_Green_RejectsMusicCategOutOfRange()
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
            PlayDatetime = "2026-05-15 12:00:00",
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 1,
                    PlayResult = 1,
                    PlayScore = 100000,
                    GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                    OptionFlg = [0], ToneFlg = [0],
                    MusicCateg = 8
                }
            ]
        }),
        CancellationToken.None);

    Assert.Equal(0u, result);
    Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
    var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
    Assert.Equal(0u, save.CategJpopCnt);
}

[Fact]
public async Task UpdatePlayResult_Green_PersistsIsPushedOnPlayLog()
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
            PlayDatetime = "2026-05-15 12:00:00",
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 1,
                    PlayResult = 1,
                    PlayScore = 100000,
                    GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                    OptionFlg = [0], ToneFlg = [0],
                    MusicCateg = 0,
                    IsPushed = true
                }
            ]
        }),
        CancellationToken.None);

    Assert.Equal(1u, result);
    var play = await fixture.Context.SongPlayDataGreen.SingleAsync(row => row.Baid == 1);
    Assert.True(play.IsPushed);
}
```

- [ ] **Step 2: Run the tests and verify they fail**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_IncrementsGenreAndSongCounters Or FullyQualifiedName~UpdatePlayResult_Green_RejectsMusicCategOutOfRange Or FullyQualifiedName~UpdatePlayResult_Green_PersistsIsPushedOnPlayLog"
```

Expected:
- `IncrementsGenreAndSongCounters` fails (counters all 0 — helper not wired).
- `RejectsMusicCategOutOfRange` fails (play accepted with Result=1).
- `PersistsIsPushedOnPlayLog` fails (play.IsPushed is false — not persisted).

- [ ] **Step 3: Add the music_categ range check to `IsValidGreenStage`**

Open `Application/Handlers/UpdatePlayResultCommand.Green.cs`. Modify `IsValidGreenStage` to append a `MusicCateg <= 7` clause:

```csharp
private static bool IsValidGreenStage(CommonPlayResultData.StageData stage, IGreenCatalog green)
{
    return stage.SongNo < GreenProtocolBytes.SongFlagBytes * 8
        && green.GreenMusicInfos.ContainsKey(stage.SongNo)
        && stage.Level is >= MinGreenCourseLevel and <= MaxGreenCourseLevel
        && stage.StageMode <= MaxGreenStageMode
        && stage.PlayResult <= MaxGreenPlayResult
        && stage.MusicCateg <= 7
        && (stage.PlayDan is null || GreenDanHelpers.IsKnownGreenDanId(stage.PlayDan.Value));
}
```

- [ ] **Step 4: Call `GreenProfileCounters.ApplyStage` per stage**

Still in `UpdatePlayResultCommand.Green.cs::HandleGreen`. Find the existing per-stage loop:

```csharp
foreach (var stage in playResultData.AryStageInfoes)
{
    await SaveStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
}
```

Insert `GreenProfileCounters.ApplyStage(saveData, stage);` before `SaveStageAsync`:

```csharp
foreach (var stage in playResultData.AryStageInfoes)
{
    GreenProfileCounters.ApplyStage(saveData, stage);
    await SaveStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
}
```

- [ ] **Step 5: Persist `IsPushed` on the play log**

Still in `UpdatePlayResultCommand.Green.cs`. Find `SaveStageAsync` and the `new SongPlayDatumGreen { ... }` initializer. Add `IsPushed = stage.IsPushed,` next to the existing `IsPapamama = stage.IsPapamama,` line. The relevant block becomes:

```csharp
IsFavorite = stage.IsFavorite,
IsRecent = stage.IsRecent,
IsPapamama = stage.IsPapamama,
IsPushed = stage.IsPushed,
SoulGauge = stage.SoulGauge.GetValueOrDefault(),
```

- [ ] **Step 6: Run the tests and verify they pass**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests"
```

Expected: all Green play-result handler tests pass, including the three new ones.

- [ ] **Step 7: Commit Task 4**

```bash
git add Application/Handlers/UpdatePlayResultCommand.Green.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Integrate Green profile counters and persist IsPushed"
```
