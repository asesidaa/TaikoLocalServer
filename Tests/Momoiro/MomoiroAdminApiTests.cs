using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroAdminApiTests
{
    [Fact]
    public async Task Ac15ProfileSettings_Momoiro_ReadsAndSavesMomoiroProfileOnly()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataKimidori.Add(UserSaveDataKimidoriExtensions.CreateDefaultKimidoriSaveData(1));
        var save = UserSaveDataMomoiroExtensions.CreateDefaultMomoiroSaveData(1);
        save.Costume1 = 5;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], Ac15EraProfiles.Momoiro.Limits.CostumeFlagBytes);
        save.Title = "Momoiro Title";
        save.TitleFlg = BitsetCodec.Encode([10], Ac15EraProfiles.Momoiro.Limits.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], Ac15EraProfiles.Momoiro.Limits.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = true;
        save.IsExplain = true;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        fixture.Context.UserSaveDataMomoiro.Add(save);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateAc15ProfileSettingsController(fixture.Context);

        var getResult = await controller.Get("Momoiro", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var setting = Assert.IsType<Ac15ProfileSettingsDto>(ok.Value);
        Assert.Equal("Momoiro", setting.Era);
        Assert.True(setting.Capabilities.SupportsTitle);
        Assert.False(setting.Capabilities.SupportsTitlePlate);
        Assert.False(setting.Capabilities.SupportsTaikojukuFolderDan);
        Assert.Null(setting.Options.Taikojuku);
        Assert.Equal(5u, Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi").CurrentId);
        Assert.Equal([0u, 5u], Assert.Single(setting.Customization.CostumeSlots, slot => slot.Slot == "kigurumi").UnlockedIds);
        Assert.Equal("Momoiro Title", setting.Customization.Title!.TitleText);
        Assert.Equal([10u], setting.Customization.Title.UnlockedTitleIds);
        Assert.Equal(4u, setting.Customization.Tone!.ToneId);

        var saveResult = await controller.Put("Momoiro", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("MOMO", 0),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("kigurumi", 36, [0, 36])
                ],
                Title: new Ac15TitleSelectionUpdateDto("Momoiro Title 2", 11, [10, 11]),
                Tone: new Ac15ToneSelectionUpdateDto(6, [0, 6]),
                Colors: new Ac15CostumeColorsDto(2, 3, 4)),
            new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: null,
                Folder: new Ac15FolderOptionsDto(false),
                SongSelect: new Ac15SongSelectOptionsDto(4, 3),
                Taikojuku: null,
                Tutorials: new Ac15TutorialOptionsDto(false),
                CustomizationBehavior: new Ac15CustomizationBehaviorOptionsDto(false))));

        Assert.IsType<NoContentResult>(saveResult);
        Assert.Equal("MOMO", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(36u, save.Costume1);
        Assert.Contains(36u, BitsetCodec.Decode(save.CostumeFlg1, Ac15EraProfiles.Momoiro.Limits.CostumeFlagBytes));
        Assert.Equal("Momoiro Title 2", save.Title);
        Assert.Contains(11u, BitsetCodec.Decode(save.TitleFlg, Ac15EraProfiles.Momoiro.Limits.TitleFlagBytes));
        Assert.Contains(6u, BitsetCodec.Decode(save.ToneFlg, Ac15EraProfiles.Momoiro.Limits.ToneFlagBytes));
        Assert.False(save.IsTojiru);
        Assert.False(save.IsAutoCostumeOn);
        Assert.False(save.IsExplain);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataKimidori.FindAsync(1u));
    }

    [Fact]
    public async Task ScoreAndHistory_Momoiro_UseMomoiroRowsOnly()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(1, "11111111111111111111", "don");
        await fixture.SeedMomoiroBestAsync(1, 101, Difficulty.Oni, isShin: false, bestScore: 900_000, bestRate: 90, bestCrown: CrownType.Clear);
        await fixture.SeedMomoiroBestAsync(1, 101, Difficulty.Oni, isShin: true, bestScore: 930_000, bestRate: 93, bestCrown: CrownType.Gold);
        await fixture.SeedMomoiroPlayAsync(1, 101, Difficulty.Oni, CrownType.Clear, 900_000, new DateTime(2026, 6, 28, 1, 2, 3, DateTimeKind.Utc));
        await fixture.SeedMomoiroFavoriteAsync(1, 101, 0);
        fixture.Context.SongBestDataKimidori.Add(new SongBestDatumKimidori { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999_999, BestRate = 99, BestCrown = CrownType.Dondaful });
        fixture.Context.SongPlayDataKimidori.Add(new SongPlayDatumKimidori { Baid = 1, SongId = 102, Difficulty = Difficulty.Oni, Score = 999_999, Crown = CrownType.Dondaful, OptionFlg = [], ToneFlg = [], PlayTime = DateTime.UnixEpoch });
        fixture.Context.KimidoriFavoriteSongs.Add(new KimidoriFavoriteSongs { Baid = 1, SongNo = 102 });
        await fixture.Context.SaveChangesAsync();

        var playData = await CreatePlayDataController(fixture.Context).GetSongBestRecords("Momoiro", 1);
        var history = await CreatePlayHistoryController(fixture.Context).GetSongHistory("Momoiro", 1);

        var bestResponse = Assert.IsType<SongBestResponse>(Assert.IsType<OkObjectResult>(playData.Result).Value);
        var best = Assert.Single(bestResponse.SongBestData);
        Assert.Equal(101u, best.SongId);
        Assert.Equal(900_000u, best.BestScore);
        Assert.True(best.IsFavorite);
        Assert.Equal(ScoreRank.None, best.BestScoreRank);
        Assert.Equal(930_000u, best.AlternateScore!.BestScore);

        var historyResponse = Assert.IsType<SongHistoryResponse>(Assert.IsType<OkObjectResult>(history.Result).Value);
        var historyRow = Assert.Single(historyResponse.SongHistoryData);
        Assert.Equal(101u, historyRow.SongId);
        Assert.Equal(900_000u, historyRow.Score);
        Assert.True(historyRow.IsFavorite);
        Assert.Equal(ScoreRank.None, historyRow.ScoreRank);
    }

    [Fact]
    public async Task FavoriteSongs_Momoiro_PreservesDisplayOrderAndRejectsSixthFavorite()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(1, "11111111111111111111", "don");
        await fixture.SeedMomoiroFavoriteAsync(1, 300, 0);
        await fixture.SeedMomoiroFavoriteAsync(1, 101, 1);
        fixture.Context.KimidoriFavoriteSongs.Add(new KimidoriFavoriteSongs { Baid = 1, SongNo = 999 });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateFavoriteSongsController(fixture.Context, fixture.Catalog);

        var getResult = await controller.GetFavoriteSongs("Momoiro", 1);
        var thirdResult = await controller.UpdateFavoriteSong("Momoiro", new SetFavoriteRequest { Baid = 1, SongId = 250, IsFavorite = true });
        var fourthResult = await controller.UpdateFavoriteSong("Momoiro", new SetFavoriteRequest { Baid = 1, SongId = 400, IsFavorite = true });
        var fifthResult = await controller.UpdateFavoriteSong("Momoiro", new SetFavoriteRequest { Baid = 1, SongId = 401, IsFavorite = true });
        var sixthResult = await controller.UpdateFavoriteSong("Momoiro", new SetFavoriteRequest { Baid = 1, SongId = 402, IsFavorite = true });

        var ok = Assert.IsType<OkObjectResult>(getResult);
        Assert.Equal([300u, 101u], Assert.IsAssignableFrom<List<uint>>(ok.Value));
        Assert.IsType<NoContentResult>(thirdResult);
        Assert.IsType<NoContentResult>(fourthResult);
        Assert.IsType<NoContentResult>(fifthResult);
        Assert.IsType<BadRequestObjectResult>(sixthResult);
        Assert.Equal(2, (await fixture.Context.MomoiroFavoriteSongs.FindAsync(1u, 250u))!.DisplayOrder);
        var orderedResult = await controller.GetFavoriteSongs("Momoiro", 1);
        Assert.Equal(
            [300u, 101u, 250u, 400u, 401u],
            Assert.IsAssignableFrom<List<uint>>(Assert.IsType<OkObjectResult>(orderedResult).Value));
        Assert.Single(await fixture.Context.KimidoriFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task SongLeaderboard_Momoiro_UsesMomoiroBestRowsOnly()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        fixture.Context.UserData.AddRange(
            new UserDatum { Baid = 1, MyDonName = "MOMO1" },
            new UserDatum { Baid = 2, MyDonName = "MOMO2" });
        await fixture.Context.SaveChangesAsync();
        await fixture.SeedMomoiroBestAsync(1, 101, Difficulty.Oni, isShin: false, bestScore: 800_000, bestRate: 80, bestCrown: CrownType.Clear);
        await fixture.SeedMomoiroBestAsync(2, 101, Difficulty.Oni, isShin: false, bestScore: 900_000, bestRate: 90, bestCrown: CrownType.Gold);
        await fixture.SeedMomoiroBestAsync(2, 101, Difficulty.Oni, isShin: true, bestScore: 950_000, bestRate: 95, bestCrown: CrownType.Dondaful);
        fixture.Context.SongBestDataKimidori.Add(new SongBestDatumKimidori { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999_999, BestRate = 99, BestCrown = CrownType.Dondaful });
        await fixture.Context.SaveChangesAsync();

        var result = await CreateSongLeaderboardController(fixture.Context)
            .GetSongLeaderboard("Momoiro", 101, 1, (uint)Difficulty.Oni);

        var response = Assert.IsType<SongLeaderboardResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal([2u, 1u], response.LeaderboardData.Select(row => row.Baid).ToList());
        Assert.Equal([900_000u, 800_000u], response.LeaderboardData.Select(row => row.BestScore).ToList());
        Assert.All(response.LeaderboardData, row => Assert.Equal(ScoreRank.None, row.BestScoreRank));
        Assert.NotNull(response.UserScore);
        Assert.Equal(2, response.UserScore!.Rank);
    }

    [Fact]
    public async Task DanBestData_Momoiro_UsesMomoiroDanRowsOnly()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(1, "11111111111111111111", "don");
        await fixture.SeedMomoiroDanScoreAsync(1, 1, clearGrade: Ac15DanClearGrade.GoldClear, soulGaugeTotal: 100, comboCountTotal: 300);
        await fixture.SeedMomoiroDanStageScoreAsync(1, 1, stageIndex: 0, songNumber: 101, playScore: 1000);
        fixture.Context.DanScoreDataKimidori.Add(new DanScoreDatumKimidori
        {
            Baid = 1,
            DanId = 2,
            IsExtra = false,
            MedleyUniqueId = 20002,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            SoulGaugeTotal = 200,
            ComboCountTotal = 600
        });
        await fixture.Context.SaveChangesAsync();

        var result = await CreateDanBestDataController(fixture.Context).GetDanBestData("Momoiro", 1);

        var response = Assert.IsType<DanBestDataResponse>(Assert.IsType<OkObjectResult>(result).Value);
        var row = Assert.Single(response.DanBestDataList);
        Assert.Equal(1u, row.DanId);
        Assert.Equal(DanClearState.GoldNormalClear, row.ClearState);
        Assert.Equal(100u, row.SoulGaugeTotal);
        Assert.Equal(101u, Assert.Single(row.DanBestStageDataList).SongNumber);
    }

    [Fact]
    public async Task GameDataAndCustomization_Momoiro_ReturnMomoiroCatalogData()
    {
        var momoiroCatalog = new MomoiroHandlerFixture.TestMomoiroCatalog(
            musicInfoFileOrder:
            [
                new Ac15MusicInfoEntry
                {
                    SongNo = 201,
                    MusicId = "momoiro_song",
                    Title = "Momoiro Song",
                    CategoryId = (uint)SongGenre.Anime,
                    FileOrder = 9,
                    StarEasy = 2,
                    StarNormal = 3,
                    StarHard = 4,
                    StarOni = 5,
                    StarUra = 6
                }
            ],
            daniFileOrder:
            [
                new Ac15TaikojukuEntry
                {
                    UniqueId = 20001,
                    ChallengeLevel = 1,
                    Name = "Momoiro Dan",
                    VerupNo = 4100,
                    Songs = [new Ac15TaikojukuSong { SongNo = 201, Level = Difficulty.Easy }],
                    Conditions = new Ac15TaikojukuConditions { SoulGauge = 80 },
                    ExcellentConditions = new Ac15TaikojukuConditions { SoulGauge = 100 }
                }
            ]);
        await using var fixture = await MomoiroHandlerFixture.CreateAsync(momoiroCatalog);
        var gameDataController = CreateGameDataController(fixture.Catalog);
        var customizationController = CreateCustomizationCatalogController(fixture.Catalog);

        var musicRows = Assert.IsAssignableFrom<Dictionary<uint, MusicDetail>>(
            Assert.IsType<OkObjectResult>(gameDataController.GetMusicDetails("Momoiro")).Value);
        var danRows = Assert.IsAssignableFrom<List<DanData>>(
            Assert.IsType<OkObjectResult>(gameDataController.GetDanData("Momoiro")).Value);
        var costumeRows = Assert.IsAssignableFrom<IReadOnlyList<Costume>>(
            Assert.IsType<OkObjectResult>(customizationController.GetCostumes("Momoiro")).Value);
        var titleRows = Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Title>>(
            Assert.IsType<OkObjectResult>(customizationController.GetTitles("Momoiro")).Value);
        var neiroRows = Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Neiro>>(
            Assert.IsType<OkObjectResult>(customizationController.GetNeiros("Momoiro")).Value);

        var music = Assert.Single(musicRows).Value;
        Assert.Equal(201u, music.SongId);
        Assert.Equal("Momoiro Song", music.SongName);
        Assert.Equal(SongGenre.Anime, music.Genre);
        Assert.Equal(5, music.StarOni);

        var dan = Assert.Single(danRows);
        Assert.Equal(1u, dan.DanId);
        Assert.Equal("Momoiro Dan", dan.Title);
        Assert.Equal(4100u, dan.VerupNo);
        Assert.Equal(201u, Assert.Single(dan.OdaiSongList).SongNo);
        Assert.Equal((uint)DanConditionType.SoulGauge, Assert.Single(dan.OdaiBorderList).OdaiType);

        Assert.Contains(costumeRows, row => row.CostumeType == "kigurumi");
        Assert.Equal("Momoiro Title", titleRows[10].TitleName);
        Assert.Equal("Tone 4", neiroRows[4].NeiroName);
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

    private static SongLeaderboardController CreateSongLeaderboardController(ITaikoDbContext context)
        => new(context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

    private static DanBestDataController CreateDanBestDataController(ITaikoDbContext context)
        => new(context)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

    private static GameDataController CreateGameDataController(IGameDataCatalog catalog)
        => new(catalog)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

    private static CustomizationCatalogController CreateCustomizationCatalogController(IGameDataCatalog catalog)
        => new(catalog)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

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

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(new AuthSettings { AuthenticationRequired = false }))
            .BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = services };
    }
}
