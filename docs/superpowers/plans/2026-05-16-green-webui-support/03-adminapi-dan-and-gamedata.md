# Task 3: AdminApi Dan And Game Data

**Goal:** Add era-aware AdminApi routes for Dan best data and catalog data so WebUI pages can load Green song and Dan definitions.

**Files:**
- Modify: `Adapters.AdminApi/Controllers/DanBestDataController.cs`
- Modify: `Adapters.AdminApi/Controllers/GameDataController.cs`
- Modify: `Tests/Green/GreenAdminApiControllerTests.cs`

- [ ] **Step 1: Add failing tests for Green Dan projection and game data route**

Append these tests to `Tests/Green/GreenAdminApiControllerTests.cs`:

```csharp
[Fact]
public async Task DanBestData_Green_MapsClearGradeSubset()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
    fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
    {
        Baid = 1,
        DanId = 1,
        IsExtra = false,
        MedleyUniqueId = 20001,
        ClearGrade = GreenDanClearGrade.GoldClear,
        SoulGaugeTotal = 100,
        ComboCountTotal = 300,
        DanStageScoreData =
        [
            new() { Baid = 1, DanId = 1, IsExtra = false, StageIndex = 0, SongNumber = 101, PlayScore = 1000, HighScore = 1000, GoodCount = 10, OkCount = 2, BadCount = 1, DrumrollCount = 4, TotalHitCount = 13, ComboCount = 12 }
        ]
    });
    await fixture.Context.SaveChangesAsync();

    var controller = CreateDanBestDataController(fixture.Context);
    var result = await controller.GetDanBestData("Green", 1);

    var ok = Assert.IsType<OkObjectResult>(result);
    var response = Assert.IsType<DanBestDataResponse>(ok.Value);
    var row = Assert.Single(response.DanBestDataList);
    Assert.Equal(1u, row.DanId);
    Assert.Equal(DanClearState.GoldNormalClear, row.ClearState);
    Assert.Single(row.DanBestStageDataList);
}

[Fact]
public void GameData_Green_MusicDetailsRouteReturnsCatalog()
{
    var catalog = new FileGameDataCatalog([new GreenHandlerFixture.TestGreenCatalog()]);
    var controller = new GameDataController(catalog);

    var result = controller.GetMusicDetails("Green");

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(ok.Value);
}

private static DanBestDataController CreateDanBestDataController(ITaikoDbContext context)
{
    return new DanBestDataController(context)
    {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~DanBestData_Green_MapsClearGradeSubset Or FullyQualifiedName~GameData_Green_MusicDetailsRouteReturnsCatalog"
```

Expected: compile fails because era-aware Dan and GameData methods do not exist yet.

- [ ] **Step 3: Update DanBestDataController**

Modify `Adapters.AdminApi/Controllers/DanBestDataController.cs`:

- Keep existing `GET api/DanBestData/{baid}` as a Nijiiro wrapper.
- Add `GET /api/{era}/DanBestData/{baid}`.
- Extract existing Nijiiro code to `BuildNijiiroDanBestData(uint baid)`.
- Add Green projection:

```csharp
private async Task<DanBestDataResponse> BuildGreenDanBestData(uint baid)
{
    var rows = await context.DanScoreDataGreen
        .Where(d => d.Baid == baid)
        .Include(d => d.DanStageScoreData)
        .ToListAsync();

    return new DanBestDataResponse
    {
        DanBestDataList = rows.Select(row => new DanBestData
        {
            DanId = row.DanId,
            ClearState = MapGreenClearGrade(row.ClearGrade),
            SoulGaugeTotal = row.SoulGaugeTotal,
            ComboCountTotal = row.ComboCountTotal,
            DanBestStageDataList = row.DanStageScoreData
                .OrderBy(stage => stage.StageIndex)
                .Select(stage => new DanBestStageData
                {
                    SongNumber = stage.SongNumber,
                    PlayScore = stage.PlayScore,
                    GoodCount = stage.GoodCount,
                    OkCount = stage.OkCount,
                    BadCount = stage.BadCount,
                    DrumrollCount = stage.DrumrollCount,
                    TotalHitCount = stage.TotalHitCount,
                    ComboCount = stage.ComboCount,
                    HighScore = stage.HighScore
                })
                .ToList()
        }).ToList()
    };
}

private static DanClearState MapGreenClearGrade(GreenDanClearGrade grade)
{
    return grade switch
    {
        GreenDanClearGrade.NotClear => DanClearState.NotClear,
        GreenDanClearGrade.NormalClear => DanClearState.RedNormalClear,
        GreenDanClearGrade.GoldClear => DanClearState.GoldNormalClear,
        _ => DanClearState.NotClear
    };
}
```

- [ ] **Step 4: Update GameDataController**

Modify `Adapters.AdminApi/Controllers/GameDataController.cs`:

- Keep existing routes as Nijiiro wrappers.
- Add era-aware routes:

```csharp
[HttpGet("/api/{era}/[controller]/MusicDetails")]
public IActionResult GetMusicDetails(string era)
{
    if (!EraRoute.TryParse(era, out var gameEra)) return EraRoute.BadEra(era);
    return Ok(catalog.For(gameEra).GetMusicDetailDictionary());
}
```

- Add `GET /api/{era}/GameData/DanData`. If `IEraGameDataCatalog` does not expose Dan data generically, use a private Green branch that returns Green Taikojuku entries transformed to `DanData` enough for WebUI display, and keep Nijiiro returning existing `dan_data.json` behavior if available. Do not block the whole Green WebUI plan on costume/title catalogs.
- Keep Costumes/Titles/LockedCostumes/LockedTitles as Nijiiro wrappers for now because Green controls are hidden.

- [ ] **Step 5: Run AdminApi Dan/game data tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests"
```

Expected: all tests in `GreenAdminApiControllerTests` pass.

- [ ] **Step 6: Commit**

```powershell
git add -- Adapters.AdminApi/Controllers/DanBestDataController.cs Adapters.AdminApi/Controllers/GameDataController.cs Tests/Green/GreenAdminApiControllerTests.cs
git commit -m "Add Green AdminApi Dan and catalog routes"
```