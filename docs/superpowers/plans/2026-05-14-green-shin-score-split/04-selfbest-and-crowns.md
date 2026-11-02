# Task 4: Selfbest And Crowns

**Files:**
- Modify: `Tests/Green/GreenPlayResultHandlerTests.cs`
- Modify: `Application/Handlers/GetSelfBestQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs`

## Steps

- [ ] **Step 1: Update selfbest tests for saved Shin rows**

In `Tests/Green/GreenPlayResultHandlerTests.cs`, replace `GetSelfBest_Green_ReturnsParallelZeroShinRows` with:

```csharp
    [Fact]
    public async Task GetSelfBest_Green_ReturnsNormalAndShinSavedBests()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataGreen.AddRange(
            new SongBestDatumGreen
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Normal,
                IsShin = false,
                BestScore = 229170,
                BestCrown = CrownType.Clear
            },
            new SongBestDatumGreen
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Normal,
                IsShin = true,
                BestScore = 897650,
                BestCrown = CrownType.Clear
            });
        await fixture.Context.SaveChangesAsync();

        var handler = new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance);

        var response = await handler.Handle(new GetSelfBestQuery(1, GameEra.Green, 1, [101, 102]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal([101u, 102u], response.ArySelfbestScores.Select(row => row.SongNo).ToArray());
        Assert.Equal([101u, 102u], response.AryShinSelfbestScores.Select(row => row.SongNo).ToArray());
        Assert.Contains(response.ArySelfbestScores, row => row.SongNo == 101 && row.SelfBestScore == 229170);
        Assert.Contains(response.AryShinSelfbestScores, row => row.SongNo == 101 && row.SelfBestScore == 897650);
        Assert.Contains(response.ArySelfbestScores, row => row.SongNo == 102 && row.SelfBestScore == 0);
        Assert.Contains(response.AryShinSelfbestScores, row => row.SongNo == 102 && row.SelfBestScore == 0);
    }
```

Add a crowns regression test:

```csharp
    [Fact]
    public async Task CrownsData_Green_IgnoresShinBestRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = true,
            BestScore = 897650,
            BestCrown = CrownType.Gold
        });
        await fixture.Context.SaveChangesAsync();

        var controller = new CrownsDataController(fixture.Context, fixture.Catalog)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddLogging()
                        .BuildServiceProvider()
                }
            }
        };

        var result = await controller.CrownsData(new CrownsDataRequest
        {
            Baid = 1,
            ChassisId = "chassis",
            ShopId = "shop"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CrownsDataResponse>(ok.Value);
        var inflated = InflateZlib(response.HashCrownFlg);

        Assert.Equal(0, ReadTenBitValue(inflated, 101));
    }
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GetSelfBest_Green_ReturnsNormalAndShinSavedBests|FullyQualifiedName~CrownsData_Green_IgnoresShinBestRows"
```

Expected: selfbest fails because Shin rows are still zero; crowns may fail because the controller reads all best rows.

- [ ] **Step 3: Split Green selfbest lookup**

Replace `HandleGreen` in `Application/Handlers/GetSelfBestQuery.Green.cs` with:

```csharp
    private partial async ValueTask<CommonSelfBestResponse> HandleGreen(
        GetSelfBestQuery request,
        CancellationToken cancellationToken)
    {
        var difficulty = GreenPlayResultMapping.MapDifficulty(request.Difficulty);
        var requestedSongs = request.SongIdList ?? [];
        var requestedSet = requestedSongs.ToHashSet();
        var bestRows = await context.SongBestDataGreen
            .Where(row => row.Baid == request.Baid
                && row.Difficulty == difficulty
                && requestedSet.Contains(row.SongId))
            .ToListAsync(cancellationToken);

        var normalRowsBySong = bestRows
            .Where(row => !row.IsShin)
            .ToDictionary(row => row.SongId);
        var shinRowsBySong = bestRows
            .Where(row => row.IsShin)
            .ToDictionary(row => row.SongId);

        var normalRows = requestedSongs.Select(songNo =>
        {
            normalRowsBySong.TryGetValue(songNo, out var best);
            return new CommonSelfBestResponse.SelfBestData
            {
                SongNo = songNo,
                SelfBestScore = best?.BestScore ?? 0,
                SelfBestScoreRate = best?.BestRate ?? 0
            };
        }).ToList();

        var shinRows = requestedSongs.Select(songNo =>
        {
            shinRowsBySong.TryGetValue(songNo, out var best);
            return new CommonSelfBestResponse.SelfBestData
            {
                SongNo = songNo,
                SelfBestScore = best?.BestScore ?? 0,
                SelfBestScoreRate = best?.BestRate ?? 0
            };
        }).ToList();

        return new CommonSelfBestResponse
        {
            Result = 1,
            Level = request.Difficulty,
            ArySelfbestScores = normalRows,
            AryShinSelfbestScores = shinRows
        };
    }
```

- [ ] **Step 4: Keep crowns normal-only**

In `Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs`, update the query:

```csharp
        var bestRows = await context.SongBestDataGreen
            .Where(row => row.Baid == request.Baid && !row.IsShin)
            .ToListAsync(HttpContext.RequestAborted);
```

- [ ] **Step 5: Run selfbest and crowns tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GetSelfBest_Green_ReturnsNormalAndShinSavedBests|FullyQualifiedName~CrownsData_Green_IgnoresShinBestRows|FullyQualifiedName~GetSelfBest_Green_ReturnsSavedBest"
```

Expected: pass.

- [ ] **Step 6: Commit selfbest and crowns split**

Run:

```powershell
git add -- Tests/Green/GreenPlayResultHandlerTests.cs Application/Handlers/GetSelfBestQuery.Green.cs Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs
git commit -m "Return Green shin selfbest rows"
```
