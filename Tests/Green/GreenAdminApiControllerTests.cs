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
    public async Task FavoriteSongs_Green_AllowsTenFavoritesAndRejectsEleventh()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        for (uint songNo = 101; songNo <= 109; songNo++)
        {
            fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = songNo });
        }
        await fixture.Context.SaveChangesAsync();

        var controller = CreateFavoriteSongsController(fixture.Context, fixture.Catalog);
        var tenthResult = await controller.UpdateFavoriteSong("Green", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 110,
            IsFavorite = true
        });

        Assert.IsType<NoContentResult>(tenthResult);

        var eleventhResult = await controller.UpdateFavoriteSong("Green", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 111,
            IsFavorite = true
        });

        Assert.IsType<BadRequestObjectResult>(eleventhResult);
        Assert.Equal(Ac15EraProfiles.Green.Limits.MaxFavoriteSongs, await fixture.Context.GreenFavoriteSongs.CountAsync(row => row.Baid == 1));
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
            ClearGrade = Ac15DanClearGrade.GoldClear,
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
    public async Task SongLeaderboard_Nijiiro_KeepsScoreTiesAtSameRank()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.AddRange(
            new UserDatum { Baid = 1, MyDonName = "DON1" },
            new UserDatum { Baid = 2, MyDonName = "DON2" },
            new UserDatum { Baid = 3, MyDonName = "DON3" });
        fixture.Context.SongBestDataNijiiro.AddRange(
            new SongBestDatumNijiiro { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, BestScore = 900000, BestRate = 95, BestCrown = CrownType.Gold },
            new SongBestDatumNijiiro { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Clear },
            new SongBestDatumNijiiro { Baid = 3, SongId = 101, Difficulty = Difficulty.Oni, BestScore = 800000, BestRate = 80, BestCrown = CrownType.Clear });
        await fixture.Context.SaveChangesAsync();

        var controller = CreateSongLeaderboardController(fixture.Context);
        var result = await controller.GetSongLeaderboard(101, 2, (uint)Difficulty.Oni);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongLeaderboardResponse>(ok.Value);
        Assert.Equal([1, 1, 3], response.LeaderboardData.Select(row => row.Rank).ToList());
        Assert.NotNull(response.UserScore);
        Assert.Equal(1, response.UserScore!.Rank);
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
    public async Task Ac15ProfileSettings_Green_GetDecodesProfileSettings()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.Costume1 = 5;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], GreenProtocolBytes.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], GreenProtocolBytes.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], GreenProtocolBytes.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        save.DispDanType = 2;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = true;
        save.IsExplain = true;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        save.DispTaikojukuDan = 2;
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.DanScoreDataGreen.AddRange(
            new DanScoreDatumGreen
            {
                Baid = 1,
                DanId = 1,
                IsExtra = false,
                ClearGrade = Ac15DanClearGrade.GoldClear
            },
            new DanScoreDatumGreen
            {
                Baid = 1,
                DanId = 101,
                IsExtra = true,
                ClearGrade = Ac15DanClearGrade.NotClear
            });
        await fixture.Context.SaveChangesAsync();

        var controller = CreateAc15ProfileSettingsController(fixture.Context);
        var result = await controller.Get("Green", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var setting = Assert.IsType<Ac15ProfileSettingsDto>(ok.Value);
        var kigurumi = Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi");
        Assert.Equal(5u, kigurumi.CurrentId);
        Assert.Equal([0u, 5u], kigurumi.UnlockedIds);
        Assert.Equal([10u], setting.Customization.Title!.UnlockedTitleIds);
        Assert.Equal([0u, 4u], setting.Customization.Tone!.UnlockedToneIds);
        Assert.Equal(4u, setting.Customization.Tone.ToneId);
        Assert.True(setting.Options.NamePlate!.DisplayDanOnNamePlate);
        Assert.True(setting.Options.Folder!.ShowFolderCloseButton);
        Assert.True(setting.Options.CustomizationBehavior!.ApplyCostumeChangesFromPlayResults);
        Assert.True(setting.Options.Tutorials!.DisableHowToPlayTutorial);
        Assert.Equal(3u, setting.Options.SongSelect!.LocalRankingDifficulty);
        Assert.Equal(2u, setting.Options.SongSelect.DefaultSelectedAndSelfBestDifficulty);
        Assert.Equal(2u, setting.Options.Taikojuku!.FolderDan);
        Assert.DoesNotContain(1u, setting.Options.Taikojuku.SelectableFolderDans);
        Assert.Contains(2u, setting.Options.Taikojuku.SelectableFolderDans);
        Assert.DoesNotContain(101u, setting.Options.Taikojuku.SelectableFolderDans);
    }

    [Fact]
    public async Task Ac15ProfileSettings_Green_PutPersistsProfileSettings()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.DispScoreType = 2;
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 3,
            IsExtra = false,
            ClearGrade = Ac15DanClearGrade.NotClear
        });
        await fixture.Context.SaveChangesAsync();

        var controller = CreateAc15ProfileSettingsController(fixture.Context);

        var result = await controller.Put("Green", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("GREEN", 1),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("kigurumi", 7, [0, 7]),
                    new Ac15CostumeSlotUpdateDto("head", 8, [0, 8]),
                    new Ac15CostumeSlotUpdateDto("body", 9, [0, 9]),
                    new Ac15CostumeSlotUpdateDto("face", 10, [0, 10]),
                    new Ac15CostumeSlotUpdateDto("puchi", 11, [0, 11])
                ],
                Title: new Ac15TitleSelectionUpdateDto("Green Title", 10, [10]),
                Tone: new Ac15ToneSelectionUpdateDto(4, [0, 4]),
                Colors: new Ac15CostumeColorsDto(2, 3, 4)),
            new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: new Ac15NamePlateOptionsDto(false),
                Folder: new Ac15FolderOptionsDto(false),
                SongSelect: new Ac15SongSelectOptionsDto(4, 3),
                Taikojuku: new Ac15TaikojukuFolderDanUpdateDto(3),
                Tutorials: new Ac15TutorialOptionsDto(true),
                CustomizationBehavior: new Ac15CustomizationBehaviorOptionsDto(false))));

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("GREEN", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(1u, (await fixture.Context.UserData.FindAsync(1u))!.MyDonNameLanguage);
        Assert.Equal(7u, save.Costume1);
        Assert.Equal(8u, save.Costume2);
        Assert.Equal(9u, save.Costume3);
        Assert.Equal(10u, save.Costume4);
        Assert.Equal(11u, save.Costume5);
        Assert.Contains(7u, BitsetCodec.Decode(save.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes));
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, GreenProtocolBytes.TitleFlagBytes));
        Assert.Contains(4u, BitsetCodec.Decode(save.ToneFlg, GreenProtocolBytes.ToneFlagBytes));
        Assert.Equal("Green Title", save.Title);
        Assert.Equal(10u, save.TitleplateId);
        Assert.Equal(4u, save.DefaultToneSetting);
        Assert.Equal(2u, save.ColorBody);
        Assert.Equal(3u, save.ColorFace);
        Assert.Equal(4u, save.ColorLimb);
        Assert.Equal(0u, save.DispDanType);
        Assert.False(save.IsTojiru);
        Assert.Equal(2u, save.DispScoreType);
        Assert.False(save.IsAutoCostumeOn);
        Assert.True(save.IsExplain);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.Equal(3u, save.DispTaikojukuDan);
    }

    [Fact]
    public async Task Ac15ProfileSettings_Green_PutRejectsInvalidDefaultSelectedAndSelfBestDifficulty()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.DispLevelChassis = 2;
        save.DispLevelSelf = 3;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var controller = CreateAc15ProfileSettingsController(fixture.Context);

        var result = await controller.Put("Green", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("GREEN", 0),
            Customization: null,
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: null,
                Folder: null,
                SongSelect: new Ac15SongSelectOptionsDto(2, 5),
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null)));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(2u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
    }

    [Fact]
    public async Task UserSettings_Nijiiro_PostPersistsCustomizationWithoutUnlockEnforcement()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(1);
        save.Title = "Persisted Title";
        save.TitlePlateId = 10;
        save.CurrentKigurumi = 5;
        save.CurrentHead = 6;
        save.CurrentBody = 7;
        save.CurrentFace = 9;
        save.CurrentPuchi = 10;
        save.SelectedToneId = 4;
        fixture.Context.UserSaveDataNijiiro.Add(save);
        await fixture.Context.SaveChangesAsync();

        var controller = CreateUserSettingsController(
            fixture.Context,
            new AuthSettings
            {
                AuthenticationRequired = true,
                AllowFreeProfileEditing = false
            });
        controller.ControllerContext.HttpContext.User = CreateUserPrincipal(1);

        var result = await controller.SaveUserSetting("Nijiiro", 1, new UserSetting
        {
            MyDonName = "NIJIIRO",
            Title = "Edited Title",
            TitlePlateId = 99,
            Kigurumi = 25,
            Head = 8,
            Body = 26,
            Face = 27,
            Puchi = 28,
            ToneId = 12
        });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Edited Title", save.Title);
        Assert.Equal(99u, save.TitlePlateId);
        Assert.Equal(25u, save.CurrentKigurumi);
        Assert.Equal(8u, save.CurrentHead);
        Assert.Equal(26u, save.CurrentBody);
        Assert.Equal(27u, save.CurrentFace);
        Assert.Equal(28u, save.CurrentPuchi);
        Assert.Equal(12u, save.SelectedToneId);
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

    private static SongLeaderboardController CreateSongLeaderboardController(ITaikoDbContext context)
    {
        return new SongLeaderboardController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };
    }

    private static Ac15ProfileSettingsController CreateAc15ProfileSettingsController(ITaikoDbContext context)
    {
        var authSettings = new AuthSettings { AuthenticationRequired = false, AllowFreeProfileEditing = true };
        var httpContext = CreateHttpContext();
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(Options.Create(authSettings))
            .BuildServiceProvider();

        return new Ac15ProfileSettingsController(context, Options.Create(authSettings))
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
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
