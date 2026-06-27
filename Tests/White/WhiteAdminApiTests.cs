using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteAdminApiTests
{
    [Fact]
    public async Task Ac15ProfileSettings_White_ReadsAndSavesWhiteProfileOnly()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        var save = UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(1);
        save.Costume1 = 0;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 36], Ac15EraProfiles.White.Limits.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], Ac15EraProfiles.White.Limits.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], Ac15EraProfiles.White.Limits.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = true;
        save.IsExplain = true;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        fixture.Context.UserSaveDataWhite.Add(save);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateAc15ProfileSettingsController(fixture.Context);

        var getResult = await controller.Get("White", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var setting = Assert.IsType<Ac15ProfileSettingsDto>(ok.Value);
        Assert.True(setting.Capabilities.SupportsTitle);
        Assert.False(setting.Capabilities.SupportsTitlePlate);
        var kigurumi = Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi");
        Assert.Equal(0u, kigurumi.CurrentId);
        Assert.Contains(36u, kigurumi.UnlockedIds);
        Assert.NotNull(setting.Customization.Title);
        Assert.Equal([10u], setting.Customization.Title!.UnlockedTitleIds);
        Assert.Equal(4u, setting.Customization.Tone!.ToneId);
        Assert.True(setting.Options.Folder!.ShowFolderCloseButton);
        Assert.True(setting.Options.CustomizationBehavior!.ApplyCostumeChangesFromPlayResults);
        Assert.True(setting.Options.Tutorials!.DisableHowToPlayTutorial);

        var saveResult = await controller.Put("White", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("WHITE", 0),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("kigurumi", 36, [0, 36])
                ],
                Title: new Ac15TitleSelectionUpdateDto("White Title", 10, [10]),
                Tone: new Ac15ToneSelectionUpdateDto(4, [0, 4]),
                Colors: null),
            new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: null,
                Folder: new Ac15FolderOptionsDto(false),
                SongSelect: new Ac15SongSelectOptionsDto(4, 3),
                Taikojuku: null,
                Tutorials: new Ac15TutorialOptionsDto(false),
                CustomizationBehavior: new Ac15CustomizationBehaviorOptionsDto(false))));

        Assert.IsType<NoContentResult>(saveResult);
        Assert.Equal("WHITE", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(36u, save.Costume1);
        Assert.Contains(36u, BitsetCodec.Decode(save.CostumeFlg1, Ac15EraProfiles.White.Limits.CostumeFlagBytes));
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, Ac15EraProfiles.White.Limits.TitleFlagBytes));
        Assert.Contains(4u, BitsetCodec.Decode(save.ToneFlg, Ac15EraProfiles.White.Limits.ToneFlagBytes));
        Assert.False(save.IsTojiru);
        Assert.False(save.IsAutoCostumeOn);
        Assert.False(save.IsExplain);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataRed.FindAsync(1u));
    }

    [Fact]
    public async Task PlayData_White_UsesWhiteBestPlayAndFavoriteRowsOnly()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.UserSaveDataWhite.Add(UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(1));
        fixture.Context.SongBestDataWhite.AddRange(
            new SongBestDatumWhite { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Clear },
            new SongBestDatumWhite { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 930000, BestRate = 93, BestCrown = CrownType.Gold });
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataRed.Add(new SongBestDatumRed { Baid = 1, SongId = 102, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 888888, BestRate = 88, BestCrown = CrownType.Clear });
        fixture.Context.SongPlayDataWhite.Add(new SongPlayDatumWhite
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Oni,
            IsShin = false,
            Score = 900000,
            ScoreRate = 90,
            Crown = CrownType.Clear,
            PlayTime = new DateTime(2026, 6, 18, 1, 2, 3, DateTimeKind.Utc)
        });
        fixture.Context.WhiteFavoriteSongs.Add(new WhiteFavoriteSongs { Baid = 1, SongNo = 101 });
        await fixture.Context.SaveChangesAsync();
        var controller = CreatePlayDataController(fixture.Context);

        var result = await controller.GetSongBestRecords("White", 1);

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
    public async Task PlayHistory_White_UsesWhitePlaysAndFavoritesOnly()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.WhiteFavoriteSongs.Add(new WhiteFavoriteSongs { Baid = 1, SongNo = 101 });
        fixture.Context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = 1, SongNo = 102 });
        fixture.Context.RedFavoriteSongs.Add(new RedFavoriteSongs { Baid = 1, SongNo = 103 });
        fixture.Context.SongPlayDataWhite.Add(new SongPlayDatumWhite
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Hard,
            Score = 123456,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 18, 2, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataBlue.Add(new SongPlayDatumBlue
        {
            Baid = 1,
            SongId = 102,
            Difficulty = Difficulty.Hard,
            Score = 654321,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 18, 3, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataRed.Add(new SongPlayDatumRed
        {
            Baid = 1,
            SongId = 103,
            Difficulty = Difficulty.Hard,
            Score = 777777,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 18, 4, 0, 0, DateTimeKind.Utc)
        });
        await fixture.Context.SaveChangesAsync();

        var controller = CreatePlayHistoryController(fixture.Context);
        var result = await controller.GetSongHistory("White", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongHistoryResponse>(ok.Value);
        var row = Assert.Single(response.SongHistoryData);
        Assert.Equal(101u, row.SongId);
        Assert.Equal(123456u, row.Score);
        Assert.True(row.IsFavorite);
        Assert.Equal(ScoreRank.None, row.ScoreRank);
    }

    [Fact]
    public async Task FavoriteSongs_White_ReadsWritesAndRejectsEleventhFavoriteOnly()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        for (uint songNo = 101; songNo <= 109; songNo++)
        {
            fixture.Context.WhiteFavoriteSongs.Add(new WhiteFavoriteSongs { Baid = 1, SongNo = songNo });
        }

        fixture.Context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = 1, SongNo = 201 });
        fixture.Context.RedFavoriteSongs.Add(new RedFavoriteSongs { Baid = 1, SongNo = 202 });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateFavoriteSongsController(fixture.Context, fixture.Catalog);

        var getResult = await controller.GetFavoriteSongs("White", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult);
        Assert.Equal(Enumerable.Range(101, 9).Select(value => (uint)value).ToList(), Assert.IsAssignableFrom<List<uint>>(ok.Value).OrderBy(song => song).ToList());

        var tenthResult = await controller.UpdateFavoriteSong("White", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 110,
            IsFavorite = true
        });
        var eleventhResult = await controller.UpdateFavoriteSong("White", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 111,
            IsFavorite = true
        });

        Assert.IsType<NoContentResult>(tenthResult);
        Assert.IsType<BadRequestObjectResult>(eleventhResult);
        Assert.Equal(Ac15EraProfiles.White.Limits.MaxFavoriteSongs, await fixture.Context.WhiteFavoriteSongs.CountAsync(row => row.Baid == 1));
        Assert.Single(await fixture.Context.BlueFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Single(await fixture.Context.RedFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task SongLeaderboard_White_UsesWhiteBestRowsOnly()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.UserData.AddRange(
            new UserDatum { Baid = 1, MyDonName = "WHITE1" },
            new UserDatum { Baid = 2, MyDonName = "WHITE2" });
        fixture.Context.SongBestDataWhite.AddRange(
            new SongBestDatumWhite { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 800000, BestRate = 80, BestCrown = CrownType.Clear },
            new SongBestDatumWhite { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Gold },
            new SongBestDatumWhite { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 950000, BestRate = 95, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataRed.Add(new SongBestDatumRed { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 777777, BestRate = 77, BestCrown = CrownType.Clear });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateSongLeaderboardController(fixture.Context);

        var result = await controller.GetSongLeaderboard("White", 101, 1, (uint)Difficulty.Oni);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongLeaderboardResponse>(ok.Value);
        Assert.Equal([2u, 1u], response.LeaderboardData.Select(row => row.Baid).ToList());
        Assert.Equal([900000u, 800000u], response.LeaderboardData.Select(row => row.BestScore).ToList());
        Assert.All(response.LeaderboardData, row => Assert.Equal(ScoreRank.None, row.BestScoreRank));
        Assert.NotNull(response.UserScore);
        Assert.Equal(2, response.UserScore!.Rank);
    }

    [Fact]
    public async Task DanBestData_White_UsesWhiteDanRowsOnly()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.DanScoreDataWhite.Add(new DanScoreDatumWhite
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
        fixture.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 2,
            IsExtra = false,
            MedleyUniqueId = 20002,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            SoulGaugeTotal = 200,
            ComboCountTotal = 600
        });
        fixture.Context.DanScoreDataRed.Add(new DanScoreDatumRed
        {
            Baid = 1,
            DanId = 3,
            IsExtra = false,
            MedleyUniqueId = 20003,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            SoulGaugeTotal = 300,
            ComboCountTotal = 900
        });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateDanBestDataController(fixture.Context);

        var result = await controller.GetDanBestData("White", 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DanBestDataResponse>(ok.Value);
        var row = Assert.Single(response.DanBestDataList);
        Assert.Equal(1u, row.DanId);
        Assert.Equal(DanClearState.GoldNormalClear, row.ClearState);
        Assert.Equal(100u, row.SoulGaugeTotal);
        Assert.Single(row.DanBestStageDataList);
        Assert.Equal(101u, row.DanBestStageDataList[0].SongNumber);
    }

    [Fact]
    public async Task GameDataAndCustomization_White_ReturnWhiteCatalogData()
    {
        var whiteCatalog = new WhiteHandlerFixture.TestWhiteCatalog(
            musicInfoFileOrder:
            [
                new Ac15MusicInfoEntry
                {
                    SongNo = 201,
                    MusicId = "white_song",
                    Title = "White Song",
                    CategoryId = (uint)SongGenre.Anime,
                    FileOrder = 9,
                    StarEasy = 2,
                    StarNormal = 3,
                    StarHard = 4,
                    StarOni = 5,
                    StarUra = 6
                }
            ],
            taikojukuFileOrder:
            [
                new Ac15TaikojukuEntry
                {
                    UniqueId = 20001,
                    ChallengeLevel = 1,
                    Name = "White Dan",
                    VerupNo = 7100,
                    Songs = [new Ac15TaikojukuSong { SongNo = 201, Level = Difficulty.Easy }],
                    Conditions = new Ac15TaikojukuConditions { SoulGauge = 80 },
                    ExcellentConditions = new Ac15TaikojukuConditions { SoulGauge = 100 }
                }
            ]);
        await using var fixture = await WhiteHandlerFixture.CreateAsync(whiteCatalog);
        var gameDataController = CreateGameDataController(fixture.Catalog);
        var customizationController = CreateCustomizationCatalogController(fixture.Catalog);

        var musicResult = gameDataController.GetMusicDetails("White");
        var danResult = gameDataController.GetDanData("White");
        var costumes = Assert.IsType<OkObjectResult>(customizationController.GetCostumes("White"));
        var titles = Assert.IsType<OkObjectResult>(customizationController.GetTitles("White"));
        var neiros = Assert.IsType<OkObjectResult>(customizationController.GetNeiros("White"));

        var musicOk = Assert.IsType<OkObjectResult>(musicResult);
        var musicRows = Assert.IsAssignableFrom<Dictionary<uint, MusicDetail>>(musicOk.Value);
        var music = Assert.Single(musicRows).Value;
        Assert.Equal(201u, music.SongId);
        Assert.Equal("White Song", music.SongName);
        Assert.Equal(SongGenre.Anime, music.Genre);
        Assert.Equal(5, music.StarOni);

        var danOk = Assert.IsType<OkObjectResult>(danResult);
        var danRows = Assert.IsAssignableFrom<List<DanData>>(danOk.Value);
        var dan = Assert.Single(danRows);
        Assert.Equal(1u, dan.DanId);
        Assert.Equal("White Dan", dan.Title);
        Assert.Equal(7100u, dan.VerupNo);
        Assert.Equal(201u, Assert.Single(dan.OdaiSongList).SongNo);
        var border = Assert.Single(dan.OdaiBorderList);
        Assert.Equal((uint)DanConditionType.SoulGauge, border.OdaiType);
        Assert.Equal(80u, border.RedBorderTotal);
        Assert.Equal(100u, border.GoldBorderTotal);

        var costumeRows = Assert.IsAssignableFrom<IReadOnlyList<Costume>>(costumes.Value);
        var titleRows = Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Title>>(titles.Value);
        var neiroRows = Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Neiro>>(neiros.Value);
        Assert.Contains(costumeRows, row => row.CostumeType == "kigurumi");
        Assert.Equal("White Title", titleRows[10].TitleName);
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
