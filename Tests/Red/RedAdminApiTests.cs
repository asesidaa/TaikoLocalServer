using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedAdminApiTests
{
    [Fact]
    public async Task Ac15ProfileSettings_Red_ReadsAndSavesRedProfileOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        fixture.Context.UserSaveDataNijiiro.Add(UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(1));
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.Costume1 = 5;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], Ac15EraProfiles.Red.Limits.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], Ac15EraProfiles.Red.Limits.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], Ac15EraProfiles.Red.Limits.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = true;
        save.IsChallengeCompe = true;
        save.IsExplain = true;
        save.DispScoreType = 1;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        fixture.Context.UserSaveDataRed.Add(save);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateAc15ProfileSettingsController(fixture.Context);

        var getResult = await controller.Get("Red", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var setting = Assert.IsType<Ac15ProfileSettingsDto>(ok.Value);
        Assert.Equal(5u, Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi").CurrentId);
        Assert.Equal([0u, 5u], Assert.Single(setting.Customization.CostumeSlots, slot => slot.Slot == "kigurumi").UnlockedIds);
        Assert.Equal([10u], setting.Customization.Title!.UnlockedTitleIds);
        Assert.Equal([0u, 4u], setting.Customization.Tone!.UnlockedToneIds);
        Assert.Equal(4u, setting.Customization.Tone.ToneId);
        Assert.True(setting.Options.Folder!.ShowFolderCloseButton);
        Assert.True(setting.Options.CustomizationBehavior!.ApplyCostumeChangesFromPlayResults);
        Assert.True(setting.Options.Tutorials!.DisableHowToPlayTutorial);
        Assert.Equal(3u, setting.Options.SongSelect!.LocalRankingDifficulty);
        Assert.Equal(2u, setting.Options.SongSelect.DefaultSelectedAndSelfBestDifficulty);

        var saveResult = await controller.Put("Red", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("RED", 0),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("kigurumi", 7, [0, 7])
                ],
                Title: new Ac15TitleSelectionUpdateDto("Red Title", 10, [10]),
                Tone: new Ac15ToneSelectionUpdateDto(6, [0, 6]),
                Colors: null),
            new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: null,
                Folder: new Ac15FolderOptionsDto(false),
                SongSelect: new Ac15SongSelectOptionsDto(4, 3),
                Taikojuku: null,
                Tutorials: new Ac15TutorialOptionsDto(false),
                CustomizationBehavior: new Ac15CustomizationBehaviorOptionsDto(false))));

        Assert.IsType<NoContentResult>(saveResult);
        Assert.Equal("RED", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(7u, save.Costume1);
        Assert.Contains(7u, BitsetCodec.Decode(save.CostumeFlg1, Ac15EraProfiles.Red.Limits.CostumeFlagBytes));
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, Ac15EraProfiles.Red.Limits.TitleFlagBytes));
        Assert.Contains(6u, BitsetCodec.Decode(save.ToneFlg, Ac15EraProfiles.Red.Limits.ToneFlagBytes));
        Assert.False(save.IsTojiru);
        Assert.False(save.IsAutoCostumeOn);
        Assert.True(save.IsChallengeCompe);
        Assert.False(save.IsExplain);
        Assert.Equal(1u, save.DispScoreType);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataGreen.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataYellow.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataNijiiro.FindAsync(1u));
    }

    [Fact]
    public async Task Ac15ProfileSettings_Red_RejectsInvalidSharedAc15Settings()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.DispLevelChassis = 2;
        fixture.Context.UserSaveDataRed.Add(save);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateAc15ProfileSettingsController(fixture.Context);

        var result = await controller.Put("Red", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("RED", 0),
            Customization: null,
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: null,
                Folder: null,
                SongSelect: new Ac15SongSelectOptionsDto(5, null),
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null)));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(2u, save.DispLevelChassis);
        Assert.Equal("DON", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
    }

    [Fact]
    public async Task PlayData_Red_UsesRedBestPlayAndFavoriteRowsOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        fixture.Context.SongBestDataRed.AddRange(
            new SongBestDatumRed { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Clear },
            new SongBestDatumRed { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 930000, BestRate = 93, BestCrown = CrownType.Gold });
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen { Baid = 1, SongId = 102, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 888888, BestRate = 88, BestCrown = CrownType.Clear });
        fixture.Context.SongBestDataYellow.Add(new SongBestDatumYellow { Baid = 1, SongId = 103, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 777777, BestRate = 77, BestCrown = CrownType.Clear });
        fixture.Context.SongPlayDataRed.Add(new SongPlayDatumRed
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Oni,
            IsShin = false,
            Score = 900000,
            ScoreRate = 90,
            Crown = CrownType.Clear,
            PlayTime = new DateTime(2026, 6, 15, 1, 2, 3, DateTimeKind.Utc)
        });
        fixture.Context.RedFavoriteSongs.Add(new RedFavoriteSongs { Baid = 1, SongNo = 101 });
        await fixture.Context.SaveChangesAsync();

        var controller = CreatePlayDataController(fixture.Context);
        var result = await controller.GetSongBestRecords("Red", 1);

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
        var recent = Assert.Single(row.RecentPlayData);
        Assert.Equal(ScoreRank.None, recent.ScoreRank);
        Assert.Equal(101u, recent.SongNumber);
    }

    [Fact]
    public async Task PlayHistory_Red_UsesRedPlaysAndFavoritesOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.RedFavoriteSongs.Add(new RedFavoriteSongs { Baid = 1, SongNo = 101 });
        fixture.Context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = 1, SongNo = 102 });
        fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = 103 });
        fixture.Context.YellowFavoriteSongs.Add(new YellowFavoriteSongs { Baid = 1, SongNo = 104 });
        fixture.Context.SongPlayDataRed.Add(new SongPlayDatumRed
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Hard,
            Score = 123456,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 15, 2, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataBlue.Add(new SongPlayDatumBlue
        {
            Baid = 1,
            SongId = 102,
            Difficulty = Difficulty.Hard,
            Score = 654321,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 15, 3, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataGreen.Add(new SongPlayDatumGreen
        {
            Baid = 1,
            SongId = 103,
            Difficulty = Difficulty.Hard,
            Score = 777777,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 15, 4, 0, 0, DateTimeKind.Utc)
        });
        fixture.Context.SongPlayDataYellow.Add(new SongPlayDatumYellow
        {
            Baid = 1,
            SongId = 104,
            Difficulty = Difficulty.Hard,
            Score = 888888,
            Crown = CrownType.Gold,
            PlayTime = new DateTime(2026, 6, 15, 5, 0, 0, DateTimeKind.Utc)
        });
        await fixture.Context.SaveChangesAsync();

        var controller = CreatePlayHistoryController(fixture.Context);
        var result = await controller.GetSongHistory("Red", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongHistoryResponse>(ok.Value);
        var row = Assert.Single(response.SongHistoryData);
        Assert.Equal(101u, row.SongId);
        Assert.Equal(123456u, row.Score);
        Assert.True(row.IsFavorite);
        Assert.Equal(ScoreRank.None, row.ScoreRank);
    }

    [Fact]
    public async Task FavoriteSongs_Red_ReadsWritesAndRejectsEleventhFavoriteOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.RedFavoriteSongs.Add(new RedFavoriteSongs { Baid = 1, SongNo = 101 });
        fixture.Context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = 1, SongNo = 102 });
        fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = 103 });
        fixture.Context.YellowFavoriteSongs.Add(new YellowFavoriteSongs { Baid = 1, SongNo = 104 });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateFavoriteSongsController(fixture.Context, fixture.Catalog);

        var getResult = await controller.GetFavoriteSongs("Red", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult);
        Assert.Equal([101u], Assert.IsAssignableFrom<List<uint>>(ok.Value));

        var addResult = await controller.UpdateFavoriteSong("Red", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 105,
            IsFavorite = true
        });

        Assert.IsType<NoContentResult>(addResult);
        Assert.Equal([101u, 105u], await fixture.Context.RedFavoriteSongs
            .Where(row => row.Baid == 1)
            .OrderBy(row => row.SongNo)
            .Select(row => row.SongNo)
            .ToListAsync());
        Assert.Single(await fixture.Context.BlueFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Single(await fixture.Context.GreenFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Single(await fixture.Context.YellowFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());

        fixture.Context.RedFavoriteSongs.AddRange(
            new RedFavoriteSongs { Baid = 1, SongNo = 102 },
            new RedFavoriteSongs { Baid = 1, SongNo = 103 },
            new RedFavoriteSongs { Baid = 1, SongNo = 104 },
            new RedFavoriteSongs { Baid = 1, SongNo = 106 },
            new RedFavoriteSongs { Baid = 1, SongNo = 107 },
            new RedFavoriteSongs { Baid = 1, SongNo = 108 },
            new RedFavoriteSongs { Baid = 1, SongNo = 109 });
        await fixture.Context.SaveChangesAsync();

        var tenthResult = await controller.UpdateFavoriteSong("Red", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 110,
            IsFavorite = true
        });

        Assert.IsType<NoContentResult>(tenthResult);

        var rejectResult = await controller.UpdateFavoriteSong("Red", new SetFavoriteRequest
        {
            Baid = 1,
            SongId = 111,
            IsFavorite = true
        });

        Assert.IsType<BadRequestObjectResult>(rejectResult);
        Assert.Equal(Ac15EraProfiles.Red.Limits.MaxFavoriteSongs, await fixture.Context.RedFavoriteSongs.CountAsync(row => row.Baid == 1));
    }

    [Fact]
    public async Task SongLeaderboard_Red_UsesRedBestRowsOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.AddRange(
            new UserDatum { Baid = 1, MyDonName = "RED1" },
            new UserDatum { Baid = 2, MyDonName = "RED2" });
        fixture.Context.SongBestDataRed.AddRange(
            new SongBestDatumRed { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 800000, BestRate = 80, BestCrown = CrownType.Clear },
            new SongBestDatumRed { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 900000, BestRate = 90, BestCrown = CrownType.Gold },
            new SongBestDatumRed { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 950000, BestRate = 95, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 999999, BestRate = 99, BestCrown = CrownType.Dondaful });
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen { Baid = 2, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 777777, BestRate = 77, BestCrown = CrownType.Clear });
        fixture.Context.SongBestDataYellow.Add(new SongBestDatumYellow { Baid = 1, SongId = 101, Difficulty = Difficulty.Oni, IsShin = false, BestScore = 666666, BestRate = 66, BestCrown = CrownType.Clear });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateSongLeaderboardController(fixture.Context);

        var result = await controller.GetSongLeaderboard("Red", 101, 1, (uint)Difficulty.Oni);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SongLeaderboardResponse>(ok.Value);
        Assert.Equal([2u, 1u], response.LeaderboardData.Select(row => row.Baid).ToList());
        Assert.Equal([900000u, 800000u], response.LeaderboardData.Select(row => row.BestScore).ToList());
        Assert.All(response.LeaderboardData, row => Assert.Equal(ScoreRank.None, row.BestScoreRank));
        Assert.NotNull(response.UserScore);
        Assert.Equal(2, response.UserScore!.Rank);
    }

    [Fact]
    public async Task DanBestData_Red_UsesRedDanRowsOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "don" });
        fixture.Context.DanScoreDataRed.Add(new DanScoreDatumRed
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
        fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 3,
            IsExtra = false,
            MedleyUniqueId = 20003,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            SoulGaugeTotal = 300,
            ComboCountTotal = 900
        });
        fixture.Context.DanScoreDataYellow.Add(new DanScoreDatumYellow
        {
            Baid = 1,
            DanId = 4,
            IsExtra = false,
            MedleyUniqueId = 20004,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            SoulGaugeTotal = 400,
            ComboCountTotal = 1200
        });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateDanBestDataController(fixture.Context);

        var result = await controller.GetDanBestData("Red", 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DanBestDataResponse>(ok.Value);
        var row = Assert.Single(response.DanBestDataList);
        Assert.Equal(1u, row.DanId);
        Assert.Equal(DanClearState.GoldNormalClear, row.ClearState);
        Assert.Equal(100u, row.SoulGaugeTotal);
        var stage = Assert.Single(row.DanBestStageDataList);
        Assert.Equal(101u, stage.SongNumber);
        Assert.Equal(1000u, stage.PlayScore);
    }

    [Fact]
    public async Task GameData_Red_ReturnsRedMusicAndDanCatalogData()
    {
        var redCatalog = new RedHandlerFixture.TestRedCatalog(
            musicInfoFileOrder:
            [
                new Ac15MusicInfoEntry
                {
                    SongNo = 201,
                    MusicId = "red_song",
                    Title = "Ｍｙ Ｍｕｓｃｌｅ Ｈｅａｒｔ ～高天原～",
                    GenreName = "アニメ",
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
                    Name = "Red Dan",
                    VerupNo = 8100,
                    Songs = [new Ac15TaikojukuSong { SongNo = 201, Level = Difficulty.Easy }],
                    Conditions = new Ac15TaikojukuConditions { SoulGauge = 80 },
                    ExcellentConditions = new Ac15TaikojukuConditions { SoulGauge = 100 }
                }
            ]);
        await using var fixture = await RedHandlerFixture.CreateAsync(redCatalog);
        var controller = CreateGameDataController(fixture.Catalog);

        var musicResult = controller.GetMusicDetails("Red");
        var danResult = controller.GetDanData("Red");

        var musicOk = Assert.IsType<OkObjectResult>(musicResult);
        var musicRows = Assert.IsAssignableFrom<Dictionary<uint, MusicDetail>>(musicOk.Value);
        var music = Assert.Single(musicRows).Value;
        Assert.Equal(201u, music.SongId);
        Assert.Equal("Ｍｙ Ｍｕｓｃｌｅ Ｈｅａｒｔ ～高天原～", music.SongName);
        Assert.Equal("My Muscle Heart ~高天原~", music.SongNameEN);
        Assert.Equal("My Muscle Heart ~高天原~", music.SongNameCN);
        Assert.Equal("My Muscle Heart ~高天原~", music.SongNameKO);
        Assert.Equal(SongGenre.Anime, music.Genre);
        Assert.Equal(5, music.StarOni);

        var danOk = Assert.IsType<OkObjectResult>(danResult);
        var danRows = Assert.IsAssignableFrom<List<DanData>>(danOk.Value);
        var dan = Assert.Single(danRows);
        Assert.Equal(1u, dan.DanId);
        Assert.Equal("Red Dan", dan.Title);
        Assert.Equal(8100u, dan.VerupNo);
        Assert.Equal(201u, Assert.Single(dan.OdaiSongList).SongNo);
        var border = Assert.Single(dan.OdaiBorderList);
        Assert.Equal((uint)DanConditionType.SoulGauge, border.OdaiType);
        Assert.Equal(80u, border.RedBorderTotal);
        Assert.Equal(100u, border.GoldBorderTotal);
    }

    [Fact]
    public async Task CustomizationCatalog_Red_ReturnsRedCatalogSlices()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        var controller = CreateCustomizationCatalogController(fixture.Catalog);

        var costumes = Assert.IsType<OkObjectResult>(controller.GetCostumes("Red"));
        var titles = Assert.IsType<OkObjectResult>(controller.GetTitles("Red"));
        var neiros = Assert.IsType<OkObjectResult>(controller.GetNeiros("Red"));

        var costumeRows = Assert.IsAssignableFrom<IReadOnlyList<Costume>>(costumes.Value);
        var titleRows = Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Title>>(titles.Value);
        var neiroRows = Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Neiro>>(neiros.Value);
        Assert.Contains(costumeRows, row => row.CostumeType == "kigurumi");
        Assert.Equal("Red Title", titleRows[10].TitleName);
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
