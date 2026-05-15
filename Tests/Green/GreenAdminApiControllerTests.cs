using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Green;

public class GreenAdminApiControllerTests
{
    [Fact]
    public void ScoreFacet_CanRepresentAlternateGreenScore()
    {
        var row = new SongBestData
        {
            SongId = 101,
            Difficulty = Difficulty.Oni,
            BestScore = 900000,
            BestRate = 90,
            BestCrown = CrownType.Clear,
            BestScoreRank = ScoreRank.None,
            AlternateScore = new ScoreFacet
            {
                Label = "Shin",
                BestScore = 930000,
                BestRate = 93,
                BestCrown = CrownType.Gold
            }
        };

        Assert.Equal("Shin", row.AlternateScore.Label);
        Assert.Equal(ScoreRank.None, row.BestScoreRank);
    }

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

    private static PlayDataController CreatePlayDataController(ITaikoDbContext context)
    {
        return new PlayDataController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };
    }

    private static FavoriteSongsController CreateFavoriteSongsController(ITaikoDbContext context, IGameDataCatalog catalog)
    {
        return new FavoriteSongsController(context, catalog)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };
    }

    private static DanBestDataController CreateDanBestDataController(ITaikoDbContext context)
    {
        return new DanBestDataController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(new AuthSettings { AuthenticationRequired = false }))
            .BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = services };
    }
}
