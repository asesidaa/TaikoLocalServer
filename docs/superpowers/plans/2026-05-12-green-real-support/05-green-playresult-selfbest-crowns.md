# 05 - Green PlayResult, SelfBest, And Crowns

**Surface:** Parse Green play-result payloads, persist play history and best scores, store option/unlock updates, and return real self-best and zlib crown data.

**Why after 04:** Play results require real Green users and default save data.

**Files:**
- Modify: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/GetSelfBestQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/SelfBestController.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/SelfBestMappers.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs`
- Create: `Application/Common/GreenPlayResultMapping.cs`
- Create: `Tests/Green/GreenPlayResultHandlerTests.cs`

---

## Task 05.1: Add Green Play Result Mapping Helpers

**Acceptance Criteria:**
- [ ] Green levels map `0..4` to existing `Difficulty`.
- [ ] Known `play_result` values map to `CrownType`.
- [ ] Unknown values map to `CrownType.None`.

**Steps:**

- [ ] **Step 1: Add tests**

Create `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
using TaikoLocalServer.Application.Common;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenPlayResultHandlerTests
{
    [Theory]
    [InlineData(0, Difficulty.Easy)]
    [InlineData(1, Difficulty.Normal)]
    [InlineData(2, Difficulty.Hard)]
    [InlineData(3, Difficulty.Oni)]
    [InlineData(4, Difficulty.UraOni)]
    public void MapDifficulty_UsesGreenCourseOrder(uint level, Difficulty expected)
    {
        Assert.Equal(expected, GreenPlayResultMapping.MapDifficulty(level));
    }

    [Theory]
    [InlineData(0, CrownType.None)]
    [InlineData(1, CrownType.Clear)]
    [InlineData(2, CrownType.Gold)]
    [InlineData(3, CrownType.Dondaful)]
    [InlineData(99, CrownType.None)]
    public void MapCrown_MapsKnownValues(uint playResult, CrownType expected)
    {
        Assert.Equal(expected, GreenPlayResultMapping.MapCrown(playResult));
    }
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter GreenPlayResultHandlerTests`

Expected: FAIL because `GreenPlayResultMapping` does not exist.

- [ ] **Step 3: Create `Application/Common/GreenPlayResultMapping.cs`**

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenPlayResultMapping
{
    public static Difficulty MapDifficulty(uint level) => level switch
    {
        0 => Difficulty.Easy,
        1 => Difficulty.Normal,
        2 => Difficulty.Hard,
        3 => Difficulty.Oni,
        4 => Difficulty.UraOni,
        _ => Difficulty.None
    };

    public static CrownType MapCrown(uint playResult) => playResult switch
    {
        1 => CrownType.Clear,
        2 => CrownType.Gold,
        3 => CrownType.Dondaful,
        _ => CrownType.None
    };

    public static GreenCrownState MapGreenCrownState(CrownType crown) => crown switch
    {
        CrownType.Clear => GreenCrownState.Clear,
        CrownType.Gold => GreenCrownState.FullCombo,
        CrownType.Dondaful => GreenCrownState.Dondaful,
        _ => GreenCrownState.None
    };
}
```

- [ ] **Step 4: Run focused test**

Run: `dotnet test --filter GreenPlayResultHandlerTests`

Expected: PASS.

---

## Task 05.2: Map And Deserialize Green PlayResult

**Acceptance Criteria:**
- [ ] Controller deserializes `playresult_data` directly as `PlayResultDataRequest`.
- [ ] Mapper includes stages, unlock arrays, costumes, options, ghost objects, and Green-specific fields.

**Steps:**

- [ ] **Step 1: Update `PlayResultMappers.Map`**

Replace stub mapping with explicit mapping:

```csharp
public static CommonPlayResultData Map(PlayResultDataRequest request)
{
    return new CommonPlayResultData
    {
        Baid = request.Baid,
        ChassisId = request.ChassisId,
        ShopId = request.ShopId,
        PlayDatetime = request.PlayDatetime,
        IsRight = request.IsRight,
        CardType = request.CardType,
        IsTwoPlayers = request.IsTwoPlayers,
        AryStageInfoes = request.AryStageInfoes.Select(MapStage).ToList(),
        ReleaseSongNoes = request.ReleaseSongNoes.ToList(),
        GetToneNoes = request.GetToneNoes.ToList(),
        GetCostumeNo1s = request.GetCostumeNo1s.ToList(),
        GetCostumeNo2s = request.GetCostumeNo2s.ToList(),
        GetCostumeNo3s = request.GetCostumeNo3s.ToList(),
        GetCostumeNo4s = request.GetCostumeNo4s.ToList(),
        GetCostumeNo5s = request.GetCostumeNo5s.ToList(),
        GetTitleNoes = request.GetTitleNoes.ToList(),
        BonusDailyFlg = request.BonusDailyFlg,
        BonusWeeklyFlg = request.BonusWeeklyFlg,
        BonusMonthlyFlg = request.BonusMonthlyFlg,
        GetDonmedal = request.GetDonmedal,
        GetKatsumedal = request.GetKatsumedal,
        ItemshopTutorialFlg = request.ItemshopTutorialFlg,
        IsDevil = request.IsDevil,
        IsExplain = request.IsExplain,
        AryPlayCostume = MapCostume(request.AryPlayCostume),
        AryCurrentCostume = MapCostume(request.AryCurrentCostume),
        GenderType = request.GenderType,
        PlayerAge = request.PlayerAge,
        PlayMode = request.PlayMode,
        AreaCode = request.AreaCode,
        Reserved = request.Reserved ?? [],
        LowerlimitAge = request.LowerlimitAge,
        UpperlimitAge = request.UpperlimitAge,
        AgeScore = request.AgeScore,
        EstimationCount = request.EstimationCount,
        DanResult = request.DanResult,
        Accesstoken = request.Accesstoken,
        ContentInfo = request.ContentInfo ?? [],
        DifficultyPlayedCourse = request.DifficultyPlayedCourse,
        DifficultyPlayedStar = request.DifficultyPlayedStar,
        WaiwaiTutorialFlg = request.WaiwaiTutorialFlg,
        GhostReleaseData = MapGhostRelease(request.GhostReleaseData),
        GhostUpdatePerfData = request.GhostUpdatePerfdata is null ? null : new CommonPlayResultData.UpdateGhostPerfData
        {
            InputMedian = request.GhostUpdatePerfdata.InputMedian,
            InputVariance = request.GhostUpdatePerfdata.InputVariance
        },
        GhostUpdateRankData = MapGhostRank(request.GhostUpdateRank)
    };
}
```

Add private methods in the same mapper for `MapStage`, `MapCostume`, `MapGhostRelease`, and `MapGhostRank`. Use these generated names from `Wire/Game.cs`: `AryStageInfoes`, `AryTokendatas`, `AryWinningsDatas`, `GhostUpdatePerfdata`, and `GhostUpdateRank`.

- [ ] **Step 2: Update `PlayResultController.cs`**

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
{
    Logger.LogInformation(
        "Green PlayResult request: baid={Baid} chassis={ChassisId} payload_bytes={PayloadBytes}",
        request.BaidConf,
        request.ChassisIdConf,
        request.PlayresultData?.Length ?? 0);

    CommonPlayResultData commonRequest;
    try
    {
        commonRequest = PlayResultMappers.Map(
            Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(request.PlayresultData ?? [])));
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Failed to deserialize Green PlayResultDataRequest for baid {Baid}", request.BaidConf);
        return Ok(new PlayResultResponse { Result = 0 });
    }

    var result = await Mediator.Send(
        new UpdatePlayResultCommand(request.BaidConf, GameEra.Green, commonRequest),
        HttpContext.RequestAborted);

    return Ok(PlayResultMappers.Map(result));
}
```

- [ ] **Step 3: Build**

Run: `dotnet build`

Expected: PASS.

---

## Task 05.3: Persist Green PlayResult

**Acceptance Criteria:**
- [ ] Handler inserts `SongPlayDatumGreen`.
- [ ] Handler upserts `SongBestDatumGreen`.
- [ ] Handler updates save data, options, medals, unlock flags, favorites, recents, and ghost summaries.

**Steps:**

- [ ] **Step 1: Add persistence test**

Add a test using the fixture style from Task 04:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_SavesPlayAndBest()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            PlayDatetime = "2026-05-12 12:00:00",
            GetDonmedal = 10,
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 0,
                    PlayResult = 2,
                    PlayScore = 765432,
                    GoodCnt = 100,
                    OkCnt = 20,
                    NgCnt = 3,
                    PoundCnt = 4,
                    ComboCnt = 120,
                    HitCnt = 123,
                    OptionFlg = [1, 2, 3],
                    ToneFlg = [4],
                    IsFavorite = true,
                    IsRecent = true
                }
            ]
        }),
        CancellationToken.None);

    Assert.Equal((uint)1, result);
    Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
    var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy);
    Assert.NotNull(best);
    Assert.Equal((uint)765432, best!.BestScore);
    Assert.Equal(CrownType.Gold, best.BestCrown);
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter UpdatePlayResult_Green_SavesPlayAndBest`

Expected: FAIL because handler is still a stub.

- [ ] **Step 3: Implement `UpdatePlayResultCommand.Green.cs`**

Implement with these helper blocks:

```csharp
private partial async ValueTask<uint> HandleGreen(
    UpdatePlayResultCommand request,
    CancellationToken cancellationToken)
{
    var playResultData = request.PlayResultData;
    var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
    var playTime = DateTime.TryParse(playResultData.PlayDatetime, out var parsed)
        ? parsed
        : DateTime.Now;

    saveData.TotalGetDonmedal += playResultData.GetDonmedal;
    saveData.TotalGetKatsumedal += playResultData.GetKatsumedal;
    saveData.ItemshopTutorialFlg = playResultData.ItemshopTutorialFlg ?? saveData.ItemshopTutorialFlg;
    saveData.IsDevil = playResultData.IsDevil ?? saveData.IsDevil;
    saveData.IsExplain = playResultData.IsExplain ?? saveData.IsExplain;
    saveData.WaiwaiTutorialFlg = playResultData.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;
    saveData.DifficultyPlayedCourse = playResultData.DifficultyPlayedCourse;
    saveData.DifficultyPlayedStar = playResultData.DifficultyPlayedStar;
    saveData.LastPlayDatetime = playTime;
    saveData.PrevAreaCode = playResultData.AreaCode;

    ApplyCostume(saveData, playResultData.AryCurrentCostume);
    ApplyUnlockBits(saveData, playResultData);
    ApplyGhostUpdates(saveData, playResultData);

    foreach (var stage in playResultData.AryStageInfoes)
    {
        await SaveStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
    }

    await context.SaveChangesAsync(cancellationToken);
    return 1;
}
```

Add private helper methods in the same partial class with these signatures:

```csharp
private static void ApplyCostume(UserSaveDataGreen saveData, CommonPlayResultData.CostumeData costume);
private static void ApplyUnlockBits(UserSaveDataGreen saveData, CommonPlayResultData playResultData);
private async Task SaveStageAsync(
    uint baid,
    CommonPlayResultData.StageData stage,
    uint playMode,
    DateTime playTime,
    CancellationToken cancellationToken);
private async Task UpsertBestAsync(
    uint baid,
    CommonPlayResultData.StageData stage,
    Difficulty difficulty,
    CrownType crown,
    CancellationToken cancellationToken);
private async Task UpsertFavoriteAndRecentAsync(
    uint baid,
    CommonPlayResultData.StageData stage,
    CancellationToken cancellationToken);
private static void ApplyGhostUpdates(UserSaveDataGreen saveData, CommonPlayResultData playResultData);
```

`SaveStageAsync` creates `SongPlayDatumGreen` and calls `UpsertBestAsync`.

`UpsertBestAsync` updates when:

```csharp
var scoreImproves = stage.PlayScore > existing.BestScore;
var crownImproves = mappedCrown > existing.BestCrown;
```

Use explicit crown rank rather than relying on enum numeric order:

```csharp
private static int CrownRank(CrownType crown) => crown switch
{
    CrownType.Clear => 1,
    CrownType.Gold => 2,
    CrownType.Dondaful => 3,
    _ => 0
};
```

- [ ] **Step 4: Run focused test**

Run: `dotnet test --filter UpdatePlayResult_Green_SavesPlayAndBest`

Expected: PASS.

---

## Task 05.4: Implement SelfBest Readback

**Acceptance Criteria:**
- [ ] `GetSelfBestQuery.Green` returns saved best scores for requested songs and level.
- [ ] Controller sends Green query and maps response.

**Steps:**

- [ ] **Step 1: Add handler test**

```csharp
[Fact]
public async Task GetSelfBest_Green_ReturnsSavedBest()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
    {
        Baid = 1,
        SongId = 101,
        Difficulty = Difficulty.Easy,
        BestScore = 765432,
        BestCrown = CrownType.Gold
    });
    await fixture.Context.SaveChangesAsync();

    var handler = new GetSelfBestQueryHandler(
        fixture.Context,
        NullLogger<GetSelfBestQueryHandler>.Instance);

    var response = await handler.Handle(new GetSelfBestQuery(1, GameEra.Green, 0, [101]), CancellationToken.None);

    Assert.Equal((uint)1, response.Result);
    Assert.Contains(response.ArySelfBestScores, row => row.SongNo == 101 && row.SelfBestScore == 765432);
}
```

- [ ] **Step 2: Implement `GetSelfBestQuery.Green.cs`**

Read `SongBestDataGreen` filtered by `Baid`, `Difficulty`, and requested song IDs. Return `CommonSelfBestResponse` with `Result = 1`, `Level = request.Difficulty`, and one row per requested song with `SelfBestScore`. Use `0` when no best exists.

- [ ] **Step 3: Update `SelfBestController.cs`**

```csharp
var common = await Mediator.Send(
    new GetSelfBestQuery(request.Baid, GameEra.Green, request.Level, request.ArySongNoes),
    HttpContext.RequestAborted);
return Ok(SelfBestMappers.Map(common));
```

- [ ] **Step 4: Run tests and build**

Run:

```bash
dotnet test --filter GetSelfBest_Green_ReturnsSavedBest
dotnet build
```

Expected: both PASS.

---

## Task 05.5: Implement CrownsData Readback

**Acceptance Criteria:**
- [ ] `crownsdata` returns zlib-compressed 1280-byte inflated body.
- [ ] Saved `SongBestDatumGreen` crowns pack into 10-bit song values.

**Steps:**

- [ ] **Step 1: Add a controller/helper test**

Add a test that builds a best list and calls a public static helper you will create:

```csharp
[Fact]
public void BuildGreenCrownResponseBody_PacksSavedBestRows()
{
    var rows = new[]
    {
        new SongBestDatumGreen { SongId = 101, Difficulty = Difficulty.Easy, BestCrown = CrownType.Clear },
        new SongBestDatumGreen { SongId = 101, Difficulty = Difficulty.Normal, BestCrown = CrownType.Gold }
    };

    var packed = CrownsDataController.BuildInflatedCrownBody(rows);

    Assert.Equal(GreenProtocolBytes.CrownInflatedBytes, packed.Length);
    Assert.Equal(0b0000_1001, packed[(101 * 10) >> 3] & 0b0000_1111);
}
```

Create the helper in `Application/Common/GreenCrownResponseBuilder.cs`; tests should reference that helper directly instead of referencing the MVC controller.

- [ ] **Step 2: Implement crown response builder**

Preferred Application helper:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenCrownResponseBuilder
{
    public static byte[] BuildInflatedBody(IEnumerable<SongBestDatumGreen> bestRows)
    {
        var values = new ushort[1024];

        foreach (var group in bestRows.GroupBy(row => row.SongId).Where(group => group.Key < 1024))
        {
            GreenCrownState easy = GreenCrownState.None;
            GreenCrownState normal = GreenCrownState.None;
            GreenCrownState hard = GreenCrownState.None;
            GreenCrownState oni = GreenCrownState.None;
            GreenCrownState ura = GreenCrownState.None;

            foreach (var row in group)
            {
                var state = GreenPlayResultMapping.MapGreenCrownState(row.BestCrown);
                switch (row.Difficulty)
                {
                    case Difficulty.Easy: easy = Max(easy, state); break;
                    case Difficulty.Normal: normal = Max(normal, state); break;
                    case Difficulty.Hard: hard = Max(hard, state); break;
                    case Difficulty.Oni: oni = Max(oni, state); break;
                    case Difficulty.UraOni: ura = Max(ura, state); break;
                }
            }

            values[group.Key] = GreenProtocolBytes.BuildGreenCrownValue(easy, normal, hard, oni, ura);
        }

        return GreenProtocolBytes.PackGreenCrowns(values);
    }

    private static GreenCrownState Max(GreenCrownState left, GreenCrownState right)
        => left >= right ? left : right;
}
```

- [ ] **Step 3: Update `CrownsDataController.cs`**

```csharp
public class CrownsDataController(ITaikoDbContext context) : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Green CrownsData request: {Request}", request.Stringify());
        var bestRows = await context.SongBestDataGreen
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var inflated = GreenCrownResponseBuilder.BuildInflatedBody(bestRows);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = 0,
            HashCrownFlg = GreenProtocolBytes.CompressZlib(inflated)
        });
    }
}
```

Set `SongHashVer` from `IGameDataCatalog.Green().SongHashVersion` if you inject the catalog into the controller.

- [ ] **Step 4: Run tests and build**

Run:

```bash
dotnet test --filter GreenPlayResultHandlerTests
dotnet build
```

Expected: both PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Common Application/Handlers Adapters.GameProtocol.Green Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "feat(green): persist play results and return bests and crowns"
```
