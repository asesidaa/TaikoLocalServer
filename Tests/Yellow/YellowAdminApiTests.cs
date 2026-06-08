using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowAdminApiTests
{
    [Fact]
    public async Task UserSettings_Yellow_ReadsAndSavesYellowProfileOnly()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        save.Costume1 = 5;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], Ac15EraProfiles.Yellow.Limits.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], Ac15EraProfiles.Yellow.Limits.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = true;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateUserSettingsController(fixture.Context);

        var getResult = await controller.GetUserSetting("Yellow", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var setting = Assert.IsType<UserSetting>(ok.Value);
        Assert.Equal(5u, setting.Kigurumi);
        Assert.Equal([0u, 5u], setting.UnlockedKigurumi);
        Assert.Equal([10u], setting.UnlockedTitle);
        Assert.Equal([0u, 4u], setting.UnlockedTone);
        Assert.Equal(4u, setting.ToneId);
        Assert.True(setting.GreenIsTojiru);
        Assert.True(setting.GreenIsAutoCostumeOn);
        Assert.Equal(3u, setting.GreenDispLevelChassis);
        Assert.Equal(2u, setting.GreenDispLevelSelf);

        var saveResult = await controller.SaveUserSetting("Yellow", 1, new UserSetting
        {
            MyDonName = "YELLOW",
            Kigurumi = 7,
            UnlockedKigurumi = [0, 7],
            UnlockedTitle = [10],
            Title = "Yellow Title",
            TitlePlateId = 10,
            UnlockedTone = [0, 6],
            ToneId = 6,
            GreenIsTojiru = false,
            GreenIsAutoCostumeOn = false,
            GreenDispLevelChassis = 4,
            GreenDispLevelSelf = 3
        });

        Assert.IsType<NoContentResult>(saveResult);
        Assert.Equal("YELLOW", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(7u, save.Costume1);
        Assert.Contains(7u, BitsetCodec.Decode(save.CostumeFlg1, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes));
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, Ac15EraProfiles.Yellow.Limits.TitleFlagBytes));
        Assert.Contains(6u, BitsetCodec.Decode(save.ToneFlg, Ac15EraProfiles.Yellow.Limits.ToneFlagBytes));
        Assert.False(save.IsTojiru);
        Assert.False(save.IsAutoCostumeOn);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataGreen.FindAsync(1u));
    }

    [Fact]
    public async Task PlayData_Yellow_UsesYellowBestRowsOnly()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        fixture.Context.SongBestDataYellow.AddRange(
            new SongBestDatumYellow { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Clear },
            new SongBestDatumYellow { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 930000, BestRate = 93, BestCrown = CrownType.Gold });
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen { Baid = 1, SongId = 102, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 888888, BestRate = 88, BestCrown = CrownType.Clear });
        fixture.Context.SongPlayDataYellow.Add(new SongPlayDatumYellow
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Oni,
            IsShin = false,
            Score = 900000,
            ScoreRate = 90,
            Crown = CrownType.Clear,
            PlayTime = new DateTime(2026, 6, 8, 1, 2, 3, DateTimeKind.Utc)
        });
        fixture.Context.YellowFavoriteSongs.Add(new YellowFavoriteSongs { Baid = 1, SongNo = 101 });
        await fixture.Context.SaveChangesAsync();

        var controller = CreatePlayDataController(fixture.Context);
        var result = await controller.GetSongBestRecords("Yellow", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongBestResponse>(ok.Value);
        var row = Assert.Single(response.SongBestData);
        Assert.Equal(101u, row.SongId);
        Assert.Equal(900000u, row.BestScore);
        Assert.Equal(ScoreRank.None, row.BestScoreRank);
        Assert.True(row.IsFavorite);
        Assert.NotNull(row.AlternateScore);
        Assert.Equal("Shin", row.AlternateScore!.Label);
        Assert.Equal(930000u, row.AlternateScore.BestScore);
    }

    [Fact]
    public async Task PlayHistory_Yellow_UsesYellowPlaysAndFavoritesOnly()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.YellowFavoriteSongs.Add(new YellowFavoriteSongs { Baid = 1, SongNo = 101 });
        fixture.Context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = 1, SongNo = 102 });
        fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = 103 });
        fixture.Context.SongPlayDataYellow.Add(new SongPlayDatumYellow
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Hard,
            Score = 123456,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 8, 2, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataBlue.Add(new SongPlayDatumBlue
        {
            Baid = 1,
            SongId = 102,
            Difficulty = Difficulty.Hard,
            Score = 654321,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 8, 3, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataGreen.Add(new SongPlayDatumGreen
        {
            Baid = 1,
            SongId = 103,
            Difficulty = Difficulty.Hard,
            Score = 777777,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 8, 4, 0, 0, DateTimeKind.Utc)
        });
        await fixture.Context.SaveChangesAsync();

        var controller = CreatePlayHistoryController(fixture.Context);
        var result = await controller.GetSongHistory("Yellow", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongHistoryResponse>(ok.Value);
        var row = Assert.Single(response.SongHistoryData);
        Assert.Equal(101u, row.SongId);
        Assert.Equal(123456u, row.Score);
        Assert.True(row.IsFavorite);
        Assert.Equal(ScoreRank.None, row.ScoreRank);
    }

    [Fact]
    public async Task FavoriteSongs_Yellow_ReadsAndWritesYellowRowsOnly()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.YellowFavoriteSongs.Add(new YellowFavoriteSongs { Baid = 1, SongNo = 101 });
        fixture.Context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = 1, SongNo = 102 });
        fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = 103 });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateFavoriteSongsController(fixture.Context, fixture.Catalog);

        var getResult = await controller.GetFavoriteSongs("Yellow", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult);
        Assert.Equal([101u], Assert.IsAssignableFrom<List<uint>>(ok.Value));

        var addResult = await controller.UpdateFavoriteSong("Yellow", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 104,
            IsFavorite = true
        });

        Assert.IsType<NoContentResult>(addResult);
        Assert.Equal([101u, 104u], await fixture.Context.YellowFavoriteSongs
            .Where(row => row.Baid == 1)
            .OrderBy(row => row.SongNo)
            .Select(row => row.SongNo)
            .ToListAsync());
        Assert.Single(await fixture.Context.BlueFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Single(await fixture.Context.GreenFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
    }

    private static PlayDataController CreatePlayDataController(ITaikoDbContext context)
        => new(context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

    private static PlayHistoryController CreatePlayHistoryController(ITaikoDbContext context)
        => new(context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

    private static FavoriteSongsController CreateFavoriteSongsController(ITaikoDbContext context, IGameDataCatalog catalog)
        => new(context, catalog)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

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

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(new AuthSettings { AuthenticationRequired = false }))
            .BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = services };
    }
}
