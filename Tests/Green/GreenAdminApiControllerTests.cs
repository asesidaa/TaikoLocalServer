using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
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
    public void GameData_Green_MusicDetailsRouteReturnsCatalogStarFields()
    {
        var catalog = new FileGameDataCatalog([new GreenHandlerFixture.TestGreenCatalog(
            musicInfoFileOrder:
            [
                new GreenMusicInfoEntry
                {
                    SongNo = 105,
                    MusicId = "tank",
                    FileOrder = 0,
                    Title = "Tank!",
                    CategoryId = (uint)SongGenre.Anime,
                    HasExtreme = true,
                    StarEasy = 3,
                    StarNormal = 5,
                    StarHard = 6,
                    StarOni = 6,
                    StarUra = 9
                }
            ])]);
        var controller = new GameDataController(catalog);

        var result = controller.GetMusicDetails("Green");

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsAssignableFrom<Dictionary<uint, MusicDetail>>(ok.Value);
        var row = rows[105];
        Assert.Equal(3, row.StarEasy);
        Assert.Equal(5, row.StarNormal);
        Assert.Equal(6, row.StarHard);
        Assert.Equal(6, row.StarOni);
        Assert.Equal(9, row.StarUra);
    }

    [Fact]
    public void GameData_Green_DanDataRouteMapsCourseLevelsToSharedDifficultyEnum()
    {
        var catalog = new FileGameDataCatalog([new GreenHandlerFixture.TestGreenCatalog()]);
        var controller = new GameDataController(catalog);

        var result = controller.GetDanData("Green");

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsAssignableFrom<List<DanData>>(ok.Value);
        var first = Assert.Single(rows, row => row.DanId == 1);
        Assert.All(first.OdaiSongList, song => Assert.Equal((uint)Difficulty.Easy, song.Level));
    }

    [Fact]
    public void GameData_Green_DanDataRouteReturnsParsedMedleyConditions()
    {
        var catalog = new FileGameDataCatalog([new GreenHandlerFixture.TestGreenCatalog()]);
        var controller = new GameDataController(catalog);

        var result = controller.GetDanData("Green");

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsAssignableFrom<List<DanData>>(ok.Value);
        var first = Assert.Single(rows, row => row.DanId == 1);
        Assert.Contains(first.OdaiBorderList, border =>
            border.OdaiType == (uint)DanConditionType.SoulGauge
            && border.BorderType == (uint)DanBorderType.All
            && border.RedBorderTotal == 90
            && border.GoldBorderTotal == 95);
        Assert.Contains(first.OdaiBorderList, border =>
            border.OdaiType == (uint)DanConditionType.TotalHitCount
            && border.BorderType == (uint)DanBorderType.All
            && border.RedBorderTotal == 420
            && border.GoldBorderTotal == 460);
    }

    [Fact]
    public async Task UserSettings_Green_GetDecodesCustomizationBitsets()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.Costume1 = 5;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], GreenProtocolBytes.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], GreenProtocolBytes.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], GreenProtocolBytes.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var controller = CreateUserSettingsController(fixture.Context);
        var result = await controller.GetUserSetting("Green", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var setting = Assert.IsType<UserSetting>(ok.Value);
        Assert.Equal(5u, setting.Kigurumi);
        Assert.Equal(new List<uint> { 0, 5 }, setting.UnlockedKigurumi);
        Assert.Equal(new List<uint> { 10 }, setting.UnlockedTitle);
        Assert.Equal(new List<uint> { 0, 4 }, setting.UnlockedTone);
        Assert.Equal(4u, setting.ToneId);
    }

    [Fact]
    public async Task UserSettings_Green_PostPersistsUnlockBitsetsWhenEditingIsFree()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var controller = CreateUserSettingsController(
            fixture.Context,
            new AuthSettings
            {
                AuthenticationRequired = false,
                AllowFreeProfileEditing = true
            });

        var result = await controller.SaveUserSetting("Green", 1, new UserSetting
        {
            MyDonName = "GREEN",
            Kigurumi = 7,
            Head = 8,
            Body = 9,
            Face = 10,
            Puchi = 11,
            UnlockedKigurumi = [0, 7],
            UnlockedHead = [0, 8],
            UnlockedBody = [0, 9],
            UnlockedFace = [0, 10],
            UnlockedPuchi = [0, 11],
            UnlockedTitle = [10],
            UnlockedTone = [0, 4],
            ToneId = 4,
            Title = "Green Title",
            TitlePlateId = 0,
            BodyColor = 2,
            FaceColor = 3,
            LimbColor = 4
        });

        Assert.IsType<NoContentResult>(result);
        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Contains(7u, BitsetCodec.Decode(save!.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes));
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, GreenProtocolBytes.TitleFlagBytes));
        Assert.Contains(4u, BitsetCodec.Decode(save.ToneFlg, GreenProtocolBytes.ToneFlagBytes));
        Assert.Equal(4u, save.DefaultToneSetting);
        Assert.Equal("GREEN", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
    }

    [Fact]
    public async Task UserSettings_Green_PostFreeEditingUsesTitleIdAndIgnoresFreeTextTitle()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.Title = "Persisted Title";
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var controller = CreateUserSettingsController(
            fixture.Context,
            new AuthSettings
            {
                AuthenticationRequired = false,
                AllowFreeProfileEditing = true
            });

        var result = await controller.SaveUserSetting("Green", 1, new UserSetting
        {
            MyDonName = "GREEN",
            Title = "Injected Free Text",
            TitlePlateId = 10,
            UnlockedTitle = []
        });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(10u, save.TitleplateId);
        Assert.Equal("Persisted Title", save.Title);
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, GreenProtocolBytes.TitleFlagBytes));
    }

    [Fact]
    public async Task UserSettings_Green_PostRestrictedEnforcesPersistedUnlockBitsets()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.Title = "Persisted Title";
        save.TitleplateId = 10;
        save.Costume1 = 5;
        save.Costume2 = 6;
        save.Costume3 = 7;
        save.Costume4 = 9;
        save.Costume5 = 10;
        save.DefaultToneSetting = 4;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], GreenProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg2 = BitsetCodec.Encode([0, 6, 8], GreenProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg3 = BitsetCodec.Encode([0, 7], GreenProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg4 = BitsetCodec.Encode([0, 9], GreenProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg5 = BitsetCodec.Encode([0, 10], GreenProtocolBytes.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], GreenProtocolBytes.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4, 6], GreenProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var controller = CreateUserSettingsController(
            fixture.Context,
            new AuthSettings
            {
                AuthenticationRequired = true,
                AllowFreeProfileEditing = false
            });
        controller.ControllerContext.HttpContext.User = CreateUserPrincipal(1);

        var result = await controller.SaveUserSetting("Green", 1, new UserSetting
        {
            MyDonName = "GREEN",
            Kigurumi = 25,
            Head = 8,
            Body = 26,
            Face = 27,
            Puchi = 28,
            ToneId = 12,
            Title = "Locked Title",
            TitlePlateId = 99,
            UnlockedKigurumi = [0, 5, 25],
            UnlockedHead = [0, 6, 8],
            UnlockedBody = [0, 7, 26],
            UnlockedFace = [0, 9, 27],
            UnlockedPuchi = [0, 10, 28],
            UnlockedTitle = [10, 99],
            UnlockedTone = [0, 4, 6, 12]
        });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Persisted Title", save.Title);
        Assert.Equal(10u, save.TitleplateId);
        Assert.Equal(5u, save.Costume1);
        Assert.Equal(8u, save.Costume2);
        Assert.Equal(7u, save.Costume3);
        Assert.Equal(9u, save.Costume4);
        Assert.Equal(10u, save.Costume5);
        Assert.Equal(4u, save.DefaultToneSetting);
        Assert.Equal(new List<uint> { 0, 5 }, BitsetCodec.Decode(save.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes));
        Assert.Equal(new List<uint> { 10 }, BitsetCodec.Decode(save.TitleFlg, GreenProtocolBytes.TitleFlagBytes));
        Assert.Equal(new List<uint> { 0, 4, 6 }, BitsetCodec.Decode(save.ToneFlg, GreenProtocolBytes.ToneFlagBytes));
    }

    [Fact]
    public async Task CustomizationCatalog_Green_ReturnsCatalogSlices()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var controller = new CustomizationCatalogController(fixture.Catalog);

        var costumes = Assert.IsType<OkObjectResult>(controller.GetCostumes("Green"));
        var titles = Assert.IsType<OkObjectResult>(controller.GetTitles("Green"));
        var neiros = Assert.IsType<OkObjectResult>(controller.GetNeiros("Green"));

        Assert.IsAssignableFrom<IReadOnlyList<Costume>>(costumes.Value);
        Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Title>>(titles.Value);
        Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Neiro>>(neiros.Value);
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

    private static UserSettingsController CreateUserSettingsController(
        ITaikoDbContext context,
        AuthSettings? authSettings = null)
    {
        var effectiveAuthSettings = authSettings ?? new AuthSettings { AuthenticationRequired = false };
        var httpContext = CreateHttpContext();
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(Options.Create(effectiveAuthSettings))
            .BuildServiceProvider();

        return new UserSettingsController(
            context,
            Options.Create(effectiveAuthSettings))
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

    private static ClaimsPrincipal CreateUserPrincipal(uint baid)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, baid.ToString()),
                new Claim(ClaimTypes.Role, "User")
            ],
            "Test",
            ClaimTypes.Name,
            ClaimTypes.Role));
}
