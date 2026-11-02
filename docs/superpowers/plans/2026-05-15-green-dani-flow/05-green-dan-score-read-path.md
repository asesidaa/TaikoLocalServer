# Task 5: Green Dan Score Read Path

**Files:**
- Modify: `Application/Handlers/GetDanScoreQuery.cs`
- Modify: `Application/Handlers/GetDanScoreQuery.Green.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

- [ ] **Step 1: Write failing read-path test**

Add this test to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
[Fact]
public async Task GetDanScore_Green_ReturnsSavedChallengeLevelRows()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
    {
        Baid = 1,
        DanId = 1,
        IsExtra = false,
        MedleyUniqueId = 20001,
        ClearGrade = GreenDanClearGrade.NormalClear,
        ArrivalSongCount = 2,
        SoulGaugeTotal = 150,
        ComboCountTotal = 300,
        DanStageScoreData =
        [
            new() { Baid = 1, DanId = 1, IsExtra = false, StageIndex = 0, SongNumber = 101, PlayScore = 1000, HighScore = 1000, GoodCount = 10, OkCount = 2, BadCount = 1, DrumrollCount = 4, TotalHitCount = 13, ComboCount = 12 },
            new() { Baid = 1, DanId = 1, IsExtra = false, StageIndex = 1, SongNumber = 102, PlayScore = 2000, HighScore = 2000, GoodCount = 20, OkCount = 3, BadCount = 0, DrumrollCount = 5, TotalHitCount = 23, ComboCount = 22 }
        ]
    });
    await fixture.Context.SaveChangesAsync();

    var handler = new GetDanScoreQueryHandler(
        NullLogger<GetDanScoreQueryHandler>.Instance,
        fixture.Context,
        fixture.Catalog);

    var response = await handler.Handle(new GetDanScoreQuery(1, GameEra.Green, 0, [1]), CancellationToken.None);

    var dan = Assert.Single(response.AryDanScoreDatas);
    Assert.Equal(1u, dan.DanId);
    Assert.Equal(2u, dan.ArrivalSongCnt);
    Assert.Equal(150u, dan.SoulGaugeTotal);
    Assert.Equal(300u, dan.ComboCntTotal);
    Assert.Equal(2, dan.AryDanScoreDataStages.Count);
    Assert.Equal(1000u, dan.AryDanScoreDataStages[0].HighScore);
    Assert.Equal(2000u, dan.AryDanScoreDataStages[1].HighScore);
}
```

- [ ] **Step 2: Run the test and verify it fails**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GetDanScore_Green_ReturnsSavedChallengeLevelRows"
```

Expected: FAIL because `GetDanScoreQuery.Green` returns an empty response.

- [ ] **Step 3: Inject catalog into GetDanScoreQueryHandler**

Modify the constructor section of `Application/Handlers/GetDanScoreQuery.cs` to add `IGameDataCatalog`:

```csharp
public partial class GetDanScoreQueryHandler : IRequestHandler<GetDanScoreQuery, CommonDanScoreDataResponse>
{
    private readonly ILogger<GetDanScoreQueryHandler> logger;
    private readonly ITaikoDbContext context;
    private readonly IGameDataCatalog gameDataService;

    public GetDanScoreQueryHandler(
        ILogger<GetDanScoreQueryHandler> logger,
        ITaikoDbContext context,
        IGameDataCatalog gameDataService)
    {
        this.logger = logger;
        this.context = context;
        this.gameDataService = gameDataService;
    }
```

- [ ] **Step 4: Implement Green Dan score query**

Replace `Application/Handlers/GetDanScoreQuery.Green.cs` with:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetDanScoreQueryHandler
{
    private partial async ValueTask<CommonDanScoreDataResponse> HandleGreen(
        GetDanScoreQuery request,
        CancellationToken cancellationToken)
    {
        var requestedIds = request.DanIds.ToHashSet();
        var knownChallengeLevels = gameDataService.Green().TaikojukuFileOrder
            .Select(pack => pack.ChallengeLevel)
            .ToHashSet();

        var validRequestedIds = requestedIds
            .Where(id => knownChallengeLevels.Contains(id))
            .ToHashSet();

        var rows = await context.DanScoreDataGreen
            .Where(row => row.Baid == request.Baid && validRequestedIds.Contains(row.DanId))
            .Include(row => row.DanStageScoreData)
            .ToListAsync(cancellationToken);

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

            foreach (var stage in row.DanStageScoreData.OrderBy(stage => stage.StageIndex).Take((int)row.ArrivalSongCount))
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

- [ ] **Step 5: Run read-path test**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GetDanScore_Green_ReturnsSavedChallengeLevelRows"
```

Expected: PASS.

- [ ] **Step 6: Commit Task 5**

```powershell
git add Application/Handlers/GetDanScoreQuery.cs Application/Handlers/GetDanScoreQuery.Green.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Read Green Dani score data"
```
