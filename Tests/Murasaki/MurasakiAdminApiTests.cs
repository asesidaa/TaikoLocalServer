using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Murasaki;

public sealed class MurasakiAdminApiTests
{
    [Fact]
    public async Task Ac15ProfileSettings_Murasaki_ReadsAndSavesMurasakiProfileOnly()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataWhite.Add(UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(1));
        var save = UserSaveDataMurasakiExtensions.CreateDefaultMurasakiSaveData(1);
        save.Costume1 = 5;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], Ac15EraProfiles.Murasaki.Limits.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], Ac15EraProfiles.Murasaki.Limits.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], Ac15EraProfiles.Murasaki.Limits.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = true;
        save.IsExplain = true;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        fixture.Context.UserSaveDataMurasaki.Add(save);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateAc15ProfileSettingsController(fixture.Context);

        var getResult = await controller.Get("Murasaki", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var setting = Assert.IsType<Ac15ProfileSettingsDto>(ok.Value);
        Assert.Equal("Murasaki", setting.Era);
        Assert.True(setting.Capabilities.SupportsTitle);
        Assert.False(setting.Capabilities.SupportsTitlePlate);
        Assert.Equal(5u, Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi").CurrentId);
        Assert.Equal([0u, 5u], Assert.Single(setting.Customization.CostumeSlots, slot => slot.Slot == "kigurumi").UnlockedIds);
        Assert.NotNull(setting.Customization.Title);
        Assert.Equal([10u], setting.Customization.Title!.UnlockedTitleIds);
        Assert.Equal(4u, setting.Customization.Tone!.ToneId);
        Assert.True(setting.Options.Folder!.ShowFolderCloseButton);
        Assert.True(setting.Options.CustomizationBehavior!.ApplyCostumeChangesFromPlayResults);
        Assert.True(setting.Options.Tutorials!.DisableHowToPlayTutorial);

        var saveResult = await controller.Put("Murasaki", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("MURA", 0),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("kigurumi", 36, [0, 36])
                ],
                Title: new Ac15TitleSelectionUpdateDto("Murasaki Title", 10, [10]),
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
        Assert.Equal("MURA", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(36u, save.Costume1);
        Assert.Contains(36u, BitsetCodec.Decode(save.CostumeFlg1, Ac15EraProfiles.Murasaki.Limits.CostumeFlagBytes));
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, Ac15EraProfiles.Murasaki.Limits.TitleFlagBytes));
        Assert.Contains(6u, BitsetCodec.Decode(save.ToneFlg, Ac15EraProfiles.Murasaki.Limits.ToneFlagBytes));
        Assert.False(save.IsTojiru);
        Assert.False(save.IsAutoCostumeOn);
        Assert.False(save.IsExplain);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataWhite.FindAsync(1u));
    }

    [Fact]
    public async Task ScoreAndHistory_Murasaki_UseMurasakiRowsOnly()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.UserSaveDataMurasaki.Add(UserSaveDataMurasakiExtensions.CreateDefaultMurasakiSaveData(1));
        fixture.Context.SongBestDataMurasaki.AddRange(
            new SongBestDatumMurasaki { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Clear },
            new SongBestDatumMurasaki { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 930000, BestRate = 93, BestCrown = CrownType.Gold });
        fixture.Context.SongBestDataWhite.Add(new SongBestDatumWhite { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataRed.Add(new SongBestDatumRed { Baid = 1, SongId = 102, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 888888, BestRate = 88, BestCrown = CrownType.Clear });
        fixture.Context.SongPlayDataMurasaki.Add(new SongPlayDatumMurasaki
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Oni,
            IsShin = false,
            Score = 900000,
            ScoreRate = 90,
            Crown = CrownType.Clear,
            PlayTime = new DateTime(2026, 6, 21, 1, 2, 3, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataWhite.Add(new SongPlayDatumWhite
        {
            Baid = 1,
            SongId = 102,
            Difficulty = Difficulty.Oni,
            Score = 999999,
            Crown = CrownType.Dondaful,
            PlayTime = new DateTime(2026, 6, 21, 2, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.MurasakiFavoriteSongs.Add(new MurasakiFavoriteSongs { Baid = 1, SongNo = 101 });
        fixture.Context.WhiteFavoriteSongs.Add(new WhiteFavoriteSongs { Baid = 1, SongNo = 102 });
        await fixture.Context.SaveChangesAsync();

        var playData = await CreatePlayDataController(fixture.Context).GetSongBestRecords("Murasaki", 1);
        var history = await CreatePlayHistoryController(fixture.Context).GetSongHistory("Murasaki", 1);

        var bestResponse = Assert.IsType<SongBestResponse>(Assert.IsType<OkObjectResult>(playData.Result).Value);
        var best = Assert.Single(bestResponse.SongBestData);
        Assert.Equal(101u, best.SongId);
        Assert.Equal(900000u, best.BestScore);
        Assert.True(best.IsFavorite);
        Assert.Equal(ScoreRank.None, best.BestScoreRank);
        Assert.Equal(930000u, best.AlternateScore!.BestScore);

        var historyResponse = Assert.IsType<SongHistoryResponse>(Assert.IsType<OkObjectResult>(history.Result).Value);
        var historyRow = Assert.Single(historyResponse.SongHistoryData);
        Assert.Equal(101u, historyRow.SongId);
        Assert.Equal(900000u, historyRow.Score);
        Assert.True(historyRow.IsFavorite);
        Assert.Equal(ScoreRank.None, historyRow.ScoreRank);
    }

    [Fact]
    public async Task FavoriteSongs_Murasaki_ReadsWritesAndRejectsEleventhFavoriteOnly()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        for (uint songNo = 101; songNo <= 109; songNo++)
        {
            fixture.Context.MurasakiFavoriteSongs.Add(new MurasakiFavoriteSongs { Baid = 1, SongNo = songNo });
        }

        fixture.Context.WhiteFavoriteSongs.Add(new WhiteFavoriteSongs { Baid = 1, SongNo = 201 });
        fixture.Context.RedFavoriteSongs.Add(new RedFavoriteSongs { Baid = 1, SongNo = 202 });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateFavoriteSongsController(fixture.Context, fixture.Catalog);

        var getResult = await controller.GetFavoriteSongs("Murasaki", 1);
        var tenthResult = await controller.UpdateFavoriteSong("Murasaki", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 110,
            IsFavorite = true
        });
        var eleventhResult = await controller.UpdateFavoriteSong("Murasaki", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 111,
            IsFavorite = true
        });

        var ok = Assert.IsType<OkObjectResult>(getResult);
        Assert.Equal(Enumerable.Range(101, 9).Select(value => (uint)value).ToList(), Assert.IsAssignableFrom<List<uint>>(ok.Value).OrderBy(song => song).ToList());
        Assert.IsType<NoContentResult>(tenthResult);
        Assert.IsType<BadRequestObjectResult>(eleventhResult);
        Assert.Equal(Ac15EraProfiles.Murasaki.Limits.MaxFavoriteSongs, await fixture.Context.MurasakiFavoriteSongs.CountAsync(row => row.Baid == 1));
        Assert.Single(await fixture.Context.WhiteFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Single(await fixture.Context.RedFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task SongLeaderboard_Murasaki_UsesMurasakiBestRowsOnly()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync();
        fixture.Context.UserData.AddRange(
            new UserDatum { Baid = 1, MyDonName = "MURA1" },
            new UserDatum { Baid = 2, MyDonName = "MURA2" });
        fixture.Context.SongBestDataMurasaki.AddRange(
            new SongBestDatumMurasaki { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 800000, BestRate = 80, BestCrown = CrownType.Clear },
            new SongBestDatumMurasaki { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Gold },
            new SongBestDatumMurasaki { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 950000, BestRate = 95, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataWhite.Add(new SongBestDatumWhite { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        await fixture.Context.SaveChangesAsync();

        var result = await CreateSongLeaderboardController(fixture.Context)
            .GetSongLeaderboard("Murasaki", 101, 1, (uint)Difficulty.Oni);

        var response = Assert.IsType<SongLeaderboardResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal([2u, 1u], response.LeaderboardData.Select(row => row.Baid).ToList());
        Assert.Equal([900000u, 800000u], response.LeaderboardData.Select(row => row.BestScore).ToList());
        Assert.All(response.LeaderboardData, row => Assert.Equal(ScoreRank.None, row.BestScoreRank));
        Assert.NotNull(response.UserScore);
        Assert.Equal(2, response.UserScore!.Rank);
    }

    [Fact]
    public async Task DanBestData_Murasaki_UsesMurasakiDanRowsOnly()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.DanScoreDataMurasaki.Add(new DanScoreDatumMurasaki
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
        fixture.Context.DanScoreDataWhite.Add(new DanScoreDatumWhite
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

        var result = await CreateDanBestDataController(fixture.Context).GetDanBestData("Murasaki", 1);

        var response = Assert.IsType<DanBestDataResponse>(Assert.IsType<OkObjectResult>(result).Value);
        var row = Assert.Single(response.DanBestDataList);
        Assert.Equal(1u, row.DanId);
        Assert.Equal(DanClearState.GoldNormalClear, row.ClearState);
        Assert.Equal(100u, row.SoulGaugeTotal);
        Assert.Equal(101u, Assert.Single(row.DanBestStageDataList).SongNumber);
    }

    [Fact]
    public async Task GameDataAndCustomization_Murasaki_ReturnMurasakiCatalogData()
    {
        var murasakiCatalog = new MurasakiHandlerFixture.TestMurasakiCatalog(
            musicInfoFileOrder:
            [
                new Ac15MusicInfoEntry
                {
                    SongNo = 201,
                    MusicId = "murasaki_song",
                    Title = "Murasaki Song",
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
                    Name = "Murasaki Dan",
                    VerupNo = 6100,
                    Songs = [new Ac15TaikojukuSong { SongNo = 201, Level = Difficulty.Easy }],
                    Conditions = new Ac15TaikojukuConditions { SoulGauge = 80 },
                    ExcellentConditions = new Ac15TaikojukuConditions { SoulGauge = 100 }
                }
            ]);
        await using var fixture = await MurasakiHandlerFixture.CreateAsync(murasakiCatalog);
        var gameDataController = CreateGameDataController(fixture.Catalog);
        var customizationController = CreateCustomizationCatalogController(fixture.Catalog);

        var musicRows = Assert.IsAssignableFrom<Dictionary<uint, MusicDetail>>(
            Assert.IsType<OkObjectResult>(gameDataController.GetMusicDetails("Murasaki")).Value);
        var danRows = Assert.IsAssignableFrom<List<DanData>>(
            Assert.IsType<OkObjectResult>(gameDataController.GetDanData("Murasaki")).Value);
        var costumeRows = Assert.IsAssignableFrom<IReadOnlyList<Costume>>(
            Assert.IsType<OkObjectResult>(customizationController.GetCostumes("Murasaki")).Value);
        var titleRows = Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Title>>(
            Assert.IsType<OkObjectResult>(customizationController.GetTitles("Murasaki")).Value);
        var neiroRows = Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Neiro>>(
            Assert.IsType<OkObjectResult>(customizationController.GetNeiros("Murasaki")).Value);

        var music = Assert.Single(musicRows).Value;
        Assert.Equal(201u, music.SongId);
        Assert.Equal("Murasaki Song", music.SongName);
        Assert.Equal(SongGenre.Anime, music.Genre);
        Assert.Equal(5, music.StarOni);

        var dan = Assert.Single(danRows);
        Assert.Equal(1u, dan.DanId);
        Assert.Equal("Murasaki Dan", dan.Title);
        Assert.Equal(6100u, dan.VerupNo);
        Assert.Equal(201u, Assert.Single(dan.OdaiSongList).SongNo);
        Assert.Equal((uint)DanConditionType.SoulGauge, Assert.Single(dan.OdaiBorderList).OdaiType);

        Assert.Contains(costumeRows, row => row.CostumeType == "kigurumi");
        Assert.Equal("Murasaki Title", titleRows[10].TitleName);
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
