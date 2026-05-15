# Task 2: AdminApi Score, History, And Favorites

**Goal:** Add era-aware AdminApi routes for score rows, play history, and favorite song management, including Green score pairing and the Green five-favorite cap.

**Files:**
- Create: `Adapters.AdminApi/Controllers/EraRoute.cs`
- Modify: `Adapters.AdminApi/Controllers/PlayDataController.cs`
- Modify: `Adapters.AdminApi/Controllers/PlayHistoryController.cs`
- Modify: `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`
- Modify: `Tests/Green/GreenAdminApiControllerTests.cs`

- [ ] **Step 1: Add failing tests for Green score projection and favorite cap**

Append these tests to `Tests/Green/GreenAdminApiControllerTests.cs`:

```csharp
[Fact]
public async Task PlayData_Green_PairsNormalAndShinBestRows()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    fixture.Context.SongBestDataGreen.AddRange(
        new SongBestDatumGreen { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Clear },
        new SongBestDatumGreen { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 930000, BestRate = 93, BestCrown = CrownType.Gold });
    fixture.Context.SongPlayDataGreen.Add(new SongPlayDatumGreen
    {
        Baid = 1,
        SongId = 101,
        Difficulty = Difficulty.Oni,
        IsShin = false,
        Score = 900000,
        ScoreRate = 90,
        Crown = CrownType.Clear,
        PlayTime = new DateTime(2026, 5, 16, 1, 2, 3, DateTimeKind.Utc)
    });
    await fixture.Context.SaveChangesAsync();

    var controller = CreatePlayDataController(fixture.Context);
    var result = await controller.GetSongBestRecords("Green", 1);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<SongBestResponse>(ok.Value);
    var row = Assert.Single(response.SongBestData);
    Assert.Equal(101u, row.SongId);
    Assert.Equal(900000u, row.BestScore);
    Assert.Equal(ScoreRank.None, row.BestScoreRank);
    Assert.NotNull(row.AlternateScore);
    Assert.Equal("Shin", row.AlternateScore!.Label);
    Assert.Equal(930000u, row.AlternateScore.BestScore);
}

[Fact]
public async Task FavoriteSongs_Green_RejectsSixthFavorite()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
    for (uint songNo = 101; songNo <= 105; songNo++)
    {
        fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = songNo });
    }
    await fixture.Context.SaveChangesAsync();

    var controller = CreateFavoriteSongsController(fixture.Context, fixture.Catalog);
    var result = await controller.UpdateFavoriteSong("Green", new SetFavoriteRequest
    {
        Baid = 1,
        SongId = 106,
        IsFavorite = true
    });

    Assert.IsType<BadRequestObjectResult>(result);
    Assert.Equal(5, await fixture.Context.GreenFavoriteSongs.CountAsync(row => row.Baid == 1));
}

private static PlayDataController CreatePlayDataController(ITaikoDbContext context)
{
    return new PlayDataController(context)
    {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };
}

private static FavoriteSongsController CreateFavoriteSongsController(ITaikoDbContext context, IGameDataCatalog catalog)
{
    return new FavoriteSongsController(context, catalog)
    {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~PlayData_Green_PairsNormalAndShinBestRows Or FullyQualifiedName~FavoriteSongs_Green_RejectsSixthFavorite"
```

Expected: compile fails because era-aware controller methods and constructor signatures do not exist yet.

- [ ] **Step 3: Add era route helper**

Create `Adapters.AdminApi/Controllers/EraRoute.cs`:

```csharp
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

internal static class EraRoute
{
    public static bool TryParse(string era, out GameEra gameEra)
    {
        return Enum.TryParse(era, ignoreCase: true, out gameEra)
               && Enum.IsDefined(gameEra);
    }

    public static BadRequestObjectResult BadEra(string era)
    {
        return new BadRequestObjectResult($"Unsupported game era '{era}'.");
    }
}
```

- [ ] **Step 4: Update PlayDataController route and dispatch**

Modify `Adapters.AdminApi/Controllers/PlayDataController.cs`:

- Change the class constructor to keep `ITaikoDbContext context` only.
- Keep existing `[HttpGet("{baid}")]` method as compatibility wrapper:

```csharp
[HttpGet("{baid}")]
public Task<ActionResult<SongBestResponse>> GetSongBestRecords(uint baid)
    => GetSongBestRecords(nameof(GameEra.Nijiiro), baid);
```

- Add era route:

```csharp
[HttpGet("/api/{era}/[controller]/{baid}")]
public async Task<ActionResult<SongBestResponse>> GetSongBestRecords(string era, uint baid)
{
    if (!EraRoute.TryParse(era, out var gameEra))
        return EraRoute.BadEra(era);

    if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
        return forbid;

    var user = await context.UserData.FindAsync(baid);
    if (user is null)
        return NotFound();

    return gameEra switch
    {
        GameEra.Nijiiro => Ok(await BuildNijiiroSongBestResponse(baid)),
        GameEra.Green => Ok(await BuildGreenSongBestResponse(baid)),
        _ => EraRoute.BadEra(era)
    };
}
```

- Move the existing Nijiiro body into `BuildNijiiroSongBestResponse(uint baid)` returning `Task<SongBestResponse>`.
- Add `BuildGreenSongBestResponse(uint baid)` that:
  - loads all `SongBestDataGreen` rows for `baid`
  - groups by `(SongId, Difficulty)`
  - uses the non-Shin row as primary when available, otherwise uses the Shin row as primary
  - sets `BestScoreRank = ScoreRank.None`
  - maps the paired Shin row to `AlternateScore = new ScoreFacet { Label = "Shin", ... }` when a distinct Shin row exists
  - loads `SongPlayDataGreen` to populate `LastPlayTime`, `PlayTime`, counts, hit fields, and `RecentPlayData`
  - marks favorites from `GreenFavoriteSongs` by matching `SongNo` to `SongId` for this first implementation, matching current Green test catalog ids

- [ ] **Step 5: Update FavoriteSongsController route and Green cap**

Modify `Adapters.AdminApi/Controllers/FavoriteSongsController.cs` constructor to accept catalog:

```csharp
public class FavoriteSongsController(ITaikoDbContext context, IGameDataCatalog catalog) : BaseAdminController<FavoriteSongsController>
```

Keep existing compatibility methods as Nijiiro wrappers and add:

```csharp
[HttpPost("/api/{era}/[controller]")]
public async Task<IActionResult> UpdateFavoriteSong(string era, SetFavoriteRequest request)
```

For Green:

```csharp
var existing = await context.GreenFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
if (request.IsFavorite)
{
    if (existing is not null) return NoContent();
    var count = await context.GreenFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
    if (count >= 5) return BadRequest("Green supports at most 5 favorite songs.");
    context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = request.Baid, SongNo = request.SongId });
}
else if (existing is not null)
{
    context.GreenFavoriteSongs.Remove(existing);
}
await context.SaveChangesAsync(HttpContext.RequestAborted);
return NoContent();
```

Add `GET /api/{era}/FavoriteSongs/{baid}` that returns Green `SongNo` values for Green and existing Nijiiro favorites for Nijiiro.

- [ ] **Step 6: Update PlayHistoryController route and Green projection**

Modify `Adapters.AdminApi/Controllers/PlayHistoryController.cs` similarly:

- Keep `GET api/PlayHistory/{baid}` as Nijiiro wrapper.
- Add `GET /api/{era}/PlayHistory/{baid}`.
- For Green, load `SongPlayDataGreen`, map to `SongHistoryData`, set `ScoreRank = ScoreRank.None`, copy supported counts/crown/play time/song number, and mark `IsFavorite` from `GreenFavoriteSongs`.

- [ ] **Step 7: Run AdminApi score/history/favorite tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests"
```

Expected: all tests in `GreenAdminApiControllerTests` pass.

- [ ] **Step 8: Commit**

```powershell
git add -- Adapters.AdminApi/Controllers/EraRoute.cs Adapters.AdminApi/Controllers/PlayDataController.cs Adapters.AdminApi/Controllers/PlayHistoryController.cs Adapters.AdminApi/Controllers/FavoriteSongsController.cs Tests/Green/GreenAdminApiControllerTests.cs
git commit -m "Add Green AdminApi score and favorite routes"
```