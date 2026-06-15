using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueAdminApiParityTests
{
    [Fact]
    public async Task PlayData_Blue_PairsNormalAndShinBestRowsWithoutGreenState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataBlue.AddRange(
            new SongBestDatumBlue { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Clear },
            new SongBestDatumBlue { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 930000, BestRate = 93, BestCrown = CrownType.Gold });
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        fixture.Context.SongPlayDataBlue.Add(new SongPlayDatumBlue
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Oni,
            IsShin = false,
            Score = 900000,
            ScoreRate = 90,
            Crown = CrownType.Clear,
            PlayTime = new DateTime(2026, 5, 29, 1, 2, 3, DateTimeKind.Utc)
        });
        await fixture.Context.SaveChangesAsync();

        var controller = CreatePlayDataController(fixture.Context);
        var result = await controller.GetSongBestRecords("Blue", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongBestResponse>(ok.Value);
        var row = Assert.Single(response.SongBestData);
        Assert.Equal(900000u, row.BestScore);
        Assert.Equal(ScoreRank.None, row.BestScoreRank);
        Assert.NotNull(row.AlternateScore);
        Assert.Equal(930000u, row.AlternateScore!.BestScore);
    }

    [Fact]
    public async Task PlayHistory_Blue_UsesBluePlaysAndFavoritesOnly()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = 1, SongNo = 101 });
        fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = 102 });
        fixture.Context.SongPlayDataBlue.Add(new SongPlayDatumBlue
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Hard,
            Score = 123456,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 5, 29, 2, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataGreen.Add(new SongPlayDatumGreen
        {
            Baid = 1,
            SongId = 102,
            Difficulty = Difficulty.Hard,
            Score = 654321,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 5, 29, 3, 0, 0, DateTimeKind.Utc)
        });
        await fixture.Context.SaveChangesAsync();

        var controller = CreatePlayHistoryController(fixture.Context);
        var result = await controller.GetSongHistory("Blue", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongHistoryResponse>(ok.Value);
        var row = Assert.Single(response.SongHistoryData);
        Assert.Equal(101u, row.SongId);
        Assert.True(row.IsFavorite);
        Assert.Equal(ScoreRank.None, row.ScoreRank);
    }

    [Fact]
    public async Task FavoriteSongs_Blue_AllowsTenFavoritesAndRejectsEleventhWithoutMutatingGreen()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = 201 });
        for (uint songNo = 101; songNo <= 109; songNo++)
        {
            fixture.Context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = 1, SongNo = songNo });
        }
        await fixture.Context.SaveChangesAsync();

        var controller = CreateFavoriteSongsController(fixture.Context, fixture.Catalog);
        var tenthResult = await controller.UpdateFavoriteSong("Blue", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 110,
            IsFavorite = true
        });

        Assert.IsType<NoContentResult>(tenthResult);

        var eleventhResult = await controller.UpdateFavoriteSong("Blue", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 111,
            IsFavorite = true
        });

        Assert.IsType<BadRequestObjectResult>(eleventhResult);
        Assert.Equal(Ac15EraProfiles.Blue.Limits.MaxFavoriteSongs, await fixture.Context.BlueFavoriteSongs.CountAsync(row => row.Baid == 1));
        Assert.Equal([201u], await fixture.Context.GreenFavoriteSongs.Select(row => row.SongNo).ToListAsync());
    }

    [Fact]
    public async Task UserSettings_Blue_DecodesAndPersistsBlueCustomization()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.Costume1 = 5;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], BlueProtocolBytes.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], BlueProtocolBytes.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], BlueProtocolBytes.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        save.DispScoreType = 1;
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();

        var controller = CreateUserSettingsController(fixture.Context);
        var getResult = await controller.GetUserSetting("Blue", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var setting = Assert.IsType<UserSetting>(ok.Value);
        Assert.Equal(5u, setting.Kigurumi);
        Assert.Equal(new List<uint> { 0, 5 }, setting.UnlockedKigurumi);
        Assert.Equal(new List<uint> { 10 }, setting.UnlockedTitle);
        Assert.Equal(new List<uint> { 0, 4 }, setting.UnlockedTone);
        Assert.Equal(1u, setting.Ac15DispScoreType);

        var saveResult = await controller.SaveUserSetting("Blue", 1, new UserSetting
        {
            MyDonName = "BLUE",
            Kigurumi = 7,
            UnlockedKigurumi = [0, 7],
            UnlockedTitle = [10],
            Title = "Blue Title",
            TitlePlateId = 10,
            UnlockedTone = [0, 6],
            ToneId = 6,
            GreenIsTojiru = false,
            GreenIsAutoCostumeOn = false,
            Ac15DispScoreType = 1,
            GreenDispLevelChassis = 4,
            GreenDispLevelSelf = 3
        });

        Assert.IsType<NoContentResult>(saveResult);
        Assert.Equal("BLUE", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(7u, save.Costume1);
        Assert.Contains(7u, BitsetCodec.Decode(save.CostumeFlg1, BlueProtocolBytes.CostumeFlagBytes));
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, BlueProtocolBytes.TitleFlagBytes));
        Assert.Contains(6u, BitsetCodec.Decode(save.ToneFlg, BlueProtocolBytes.ToneFlagBytes));
        Assert.False(save.IsTojiru);
        Assert.False(save.IsAutoCostumeOn);
        Assert.Equal(1u, save.DispScoreType);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.NotNull(await fixture.Context.UserSaveDataGreen.FindAsync(1u));
    }

    [Fact]
    public async Task CustomizationCatalog_Blue_ReturnsBlueCatalogSlices()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var controller = new CustomizationCatalogController(fixture.Catalog);

        var costumes = Assert.IsType<OkObjectResult>(controller.GetCostumes("Blue"));
        var titles = Assert.IsType<OkObjectResult>(controller.GetTitles("Blue"));
        var neiros = Assert.IsType<OkObjectResult>(controller.GetNeiros("Blue"));

        Assert.IsAssignableFrom<IReadOnlyList<Costume>>(costumes.Value);
        Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Title>>(titles.Value);
        Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Neiro>>(neiros.Value);
    }

    [Fact]
    public async Task SongLeaderboard_Blue_UsesBlueScoreRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.AddRange(
            new UserDatum { Baid = 1, MyDonName = "BLUE1" },
            new UserDatum { Baid = 2, MyDonName = "BLUE2" });
        fixture.Context.SongBestDataBlue.AddRange(
            new SongBestDatumBlue { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 800000, BestRate = 80, BestCrown = CrownType.Clear },
            new SongBestDatumBlue { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Gold });
        fixture.Context.SongBestDataNijiiro.Add(new SongBestDatumNijiiro { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        await fixture.Context.SaveChangesAsync();

        var controller = CreateSongLeaderboardController(fixture.Context);
        var result = await controller.GetSongLeaderboard("Blue", 101, 1, (uint)Difficulty.Oni);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongLeaderboardResponse>(ok.Value);
        Assert.Equal([2u, 1u], response.LeaderboardData.Select(row => row.Baid).ToList());
        Assert.NotNull(response.UserScore);
        Assert.Equal(2, response.UserScore!.Rank);
    }

    private static PlayDataController CreatePlayDataController(ITaikoDbContext context)
    {
        return new PlayDataController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };
    }

    private static PlayHistoryController CreatePlayHistoryController(ITaikoDbContext context)
    {
        return new PlayHistoryController(context)
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

    private static UserSettingsController CreateUserSettingsController(ITaikoDbContext context)
    {
        var authSettings = new AuthSettings { AuthenticationRequired = false };
        var httpContext = CreateHttpContext();
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(Options.Create(authSettings))
            .BuildServiceProvider();

        return new UserSettingsController(context, Options.Create(authSettings))
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static SongLeaderboardController CreateSongLeaderboardController(ITaikoDbContext context)
    {
        return new SongLeaderboardController(context)
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
