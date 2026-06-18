using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueAdminApiDaniTests
{
    [Fact]
    public async Task DanBestData_Blue_MapsClearGradeSubset()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 1,
            IsExtra = false,
            MedleyUniqueId = 20001,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            SoulGaugeTotal = 100,
            ComboCountTotal = 300,
            DanStageScoreData =
            [
                new() { Baid = 1, DanId = 1, IsExtra = false, StageIndex = 0, SongNumber = 101, PlayScore = 1000, HighScore = 1000, GoodCount = 10, OkCount = 2, BadCount = 1, DrumrollCount = 4, TotalHitCount = 13, ComboCount = 12 }
            ]
        });
        await fixture.Context.SaveChangesAsync();
        var controller = new DanBestDataController(fixture.Context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

        var result = await controller.GetDanBestData("Blue", 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DanBestDataResponse>(ok.Value);
        var row = Assert.Single(response.DanBestDataList);
        Assert.Equal(1u, row.DanId);
        Assert.Equal(DanClearState.GoldNormalClear, row.ClearState);
        Assert.Single(row.DanBestStageDataList);
    }

    [Fact]
    public async Task DanBestData_Blue_DecodesCumulativeStageSnapshotsInCourseOrder()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 4,
            IsExtra = false,
            MedleyUniqueId = 20004,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            SoulGaugeTotal = 100,
            ComboCountTotal = 0,
            DanStageScoreData =
            [
                new() { Baid = 1, DanId = 4, IsExtra = false, StageIndex = 0, SongNumber = 300, PlayScore = 1000000, HighScore = 1000000, GoodCount = 404, OkCount = 61, BadCount = 0, DrumrollCount = 65, TotalHitCount = 530, ComboCount = 465 },
                new() { Baid = 1, DanId = 4, IsExtra = false, StageIndex = 1, SongNumber = 100, PlayScore = 1775000, HighScore = 1775000, GoodCount = 661, OkCount = 114, BadCount = 0, DrumrollCount = 198, TotalHitCount = 973, ComboCount = 775 },
                new() { Baid = 1, DanId = 4, IsExtra = false, StageIndex = 2, SongNumber = 200, PlayScore = 2931300, HighScore = 2931300, GoodCount = 1335, OkCount = 235, BadCount = 4, DrumrollCount = 295, TotalHitCount = 1865, ComboCount = 973 }
            ]
        });
        await fixture.Context.SaveChangesAsync();
        var controller = new DanBestDataController(fixture.Context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

        var result = await controller.GetDanBestData("Blue", 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DanBestDataResponse>(ok.Value);
        var row = Assert.Single(response.DanBestDataList);
        Assert.Equal(973u, row.ComboCountTotal);
        Assert.Equal([300u, 100u, 200u], row.DanBestStageDataList.Select(stage => stage.SongNumber).ToArray());
        Assert.Equal(2931300u, row.DanBestStageDataList.Aggregate(0u, (sum, stage) => sum + stage.HighScore));
        Assert.Equal(1335u, row.DanBestStageDataList.Aggregate(0u, (sum, stage) => sum + stage.GoodCount));
        Assert.Equal(235u, row.DanBestStageDataList.Aggregate(0u, (sum, stage) => sum + stage.OkCount));
        Assert.Equal(4u, row.DanBestStageDataList.Aggregate(0u, (sum, stage) => sum + stage.BadCount));
        Assert.Equal(295u, row.DanBestStageDataList.Aggregate(0u, (sum, stage) => sum + stage.DrumrollCount));
        Assert.Equal(1865u, row.DanBestStageDataList.Aggregate(0u, (sum, stage) => sum + stage.TotalHitCount));

        var stage2 = row.DanBestStageDataList[1];
        Assert.Equal(257u, stage2.GoodCount);
        Assert.Equal(53u, stage2.OkCount);
        Assert.Equal(133u, stage2.DrumrollCount);
        Assert.Equal(443u, stage2.TotalHitCount);
        Assert.Equal(775u, stage2.ComboCount);
    }

    [Fact]
    public void GameData_Blue_DanDataRouteReturnsBlueCatalogDanData()
    {
        var catalog = new FileGameDataCatalog([new BlueHandlerFixture.TestBlueCatalog()]);
        var controller = new GameDataController(catalog);

        var result = controller.GetDanData("Blue");

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsAssignableFrom<List<DanData>>(ok.Value);
        var first = Assert.Single(rows, row => row.DanId == 1);
        Assert.Equal(3, first.OdaiSongList.Count);
        Assert.All(first.OdaiSongList, song => Assert.Equal((uint)Difficulty.Easy, song.Level));
    }

    [Fact]
    public void GameData_Blue_MusicDetailsRouteReturnsBlueSongNames()
    {
        var catalog = new FileGameDataCatalog([new BlueHandlerFixture.TestBlueCatalog()]);
        var controller = new GameDataController(catalog);

        var result = controller.GetMusicDetails("Blue");

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsAssignableFrom<Dictionary<uint, MusicDetail>>(ok.Value);
        Assert.True(rows.ContainsKey(101));
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(new AuthSettings { AuthenticationRequired = false }))
            .BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = services };
    }
}
