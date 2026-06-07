using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowPlayResultHandlerTests
{
    [Fact]
    public async Task UpdatePlayResult_Yellow_GuestBaidDoesNotSave()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            0,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 0,
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.UserSaveDataYellow.ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataYellow.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataYellow.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Yellow_UnknownUserDoesNotSave()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            99,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 99,
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.UserSaveDataYellow.ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataYellow.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataYellow.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Yellow_SavesNormalPlayBestCountersUnlocksFavoritesAndRecentOnlyInYellowTables()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.UserSaveDataNijiiro.Add(UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(1));
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Easy,
            BestScore = 111,
            BestCrown = CrownType.Clear
        });
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Easy,
            BestScore = 222,
            BestCrown = CrownType.Clear
        });
        fixture.Context.SongBestDataNijiiro.Add(new SongBestDatumNijiiro
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Easy,
            BestScore = 333,
            BestCrown = CrownType.Clear
        });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260608120000",
                GetDonmedal = 10,
                GetKatsumedal = 2,
                ItemshopTutorialFlg = 7,
                IsDevil = true,
                IsExplain = true,
                WaiwaiTutorialFlg = 3,
                DifficultyPlayedCourse = 4,
                DifficultyPlayedStar = 8,
                HasDifficultyPlayedCourse = true,
                HasDifficultyPlayedStar = true,
                ReleaseSongNoes = [104],
                GetToneNoes = [4],
                GetCostumeNo1s = [1],
                GetCostumeNo2s = [2],
                GetCostumeNo3s = [3],
                GetCostumeNo4s = [4],
                GetCostumeNo5s = [5],
                GetTitleNoes = [10],
                HasAryCurrentCostume = true,
                AryCurrentCostume = new CommonPlayResultData.CostumeData
                {
                    Costume1 = 1,
                    Costume2 = 2,
                    Costume3 = 3,
                    Costume4 = 4,
                    Costume5 = 5
                },
                AreaCode = 12,
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(101u, play.SongId);
        Assert.Equal(Difficulty.Easy, play.Difficulty);
        Assert.Equal(CrownType.Gold, play.Crown);
        Assert.Equal(765432u, play.Score);
        Assert.Equal(95u, play.ScoreRate);
        Assert.Equal(0u, play.StageMode);
        Assert.False(play.IsShin);
        Assert.True(play.IsFavorite);
        Assert.True(play.IsRecent);
        Assert.True(play.IsPushed);
        Assert.Equal(9u, play.SelectedFolderId);
        Assert.Equal(new DateTime(2026, 6, 8, 12, 0, 0), play.PlayTime);

        var best = await fixture.Context.SongBestDataYellow.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(765432u, best!.BestScore);
        Assert.Equal(95u, best.BestRate);
        Assert.Equal(CrownType.Gold, best.BestCrown);
        Assert.Single(await fixture.Context.YellowFavoriteSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());
        Assert.Single(await fixture.Context.YellowRecentSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());

        var save = await fixture.Context.UserSaveDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.Equal(10u, save.TotalGetDonmedal);
        Assert.Equal(2u, save.TotalGetKatsumedal);
        Assert.Equal(7u, save.ItemshopTutorialFlg);
        Assert.True(save.IsDevil);
        Assert.True(save.IsExplain);
        Assert.Equal(3u, save.WaiwaiTutorialFlg);
        Assert.Equal(4u, save.DifficultyPlayedCourse);
        Assert.Equal(8u, save.DifficultyPlayedStar);
        Assert.Equal(new DateTime(2026, 6, 8, 12, 0, 0), save.LastPlayDatetime);
        Assert.Equal(12u, save.PrevAreaCode);
        Assert.Equal(1u, save.Costume1);
        Assert.True(BitIsSet(save.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(save.ToneFlg, 4));
        Assert.True(BitIsSet(save.CostumeFlg1, 1));
        Assert.True(BitIsSet(save.CostumeFlg2, 2));
        Assert.True(BitIsSet(save.CostumeFlg3, 3));
        Assert.True(BitIsSet(save.CostumeFlg4, 4));
        Assert.True(BitIsSet(save.CostumeFlg5, 5));
        Assert.True(BitIsSet(save.TitleFlg, 10));
        Assert.Equal(1u, save.CategJpopCnt);
        Assert.Equal(1u, save.SongPushedCnt);
        Assert.Equal(1u, save.SongFavoriteCnt);
        Assert.Equal(1u, save.SongRecentCnt);

        var blueBest = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, false);
        var greenBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy, false);
        var nijiiroBest = await fixture.Context.SongBestDataNijiiro.FindAsync(1u, 101u, Difficulty.Easy);
        Assert.Equal(111u, blueBest!.BestScore);
        Assert.Equal(222u, greenBest!.BestScore);
        Assert.Equal(333u, nijiiroBest!.BestScore);
        Assert.Empty(await fixture.Context.SongPlayDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopItemStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopItemStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueBattleUserStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueBattleNpcStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueBattleTokenStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueBattleStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Yellow_SavesNormalAndShinBestSeparately()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    CreateStage(101, 1, 0, score: 100000),
                    CreateStage(101, 1, 1, score: 200000)
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var normal = await fixture.Context.SongBestDataYellow.FindAsync(1u, 101u, Difficulty.Easy, false);
        var shin = await fixture.Context.SongBestDataYellow.FindAsync(1u, 101u, Difficulty.Easy, true);
        Assert.NotNull(normal);
        Assert.NotNull(shin);
        Assert.Equal(100000u, normal!.BestScore);
        Assert.Equal(200000u, shin!.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Yellow_FavoriteAndRecentUseYellowLimits()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);
        var stages = Enumerable.Range(101, 12)
            .Select(song => CreateStage((uint)song, 1, 0))
            .ToList();

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260608120000",
                AryStageInfoes = stages
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.MaxFavoriteSongs, await fixture.Context.YellowFavoriteSongs.CountAsync(row => row.Baid == 1));
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.MaxRecentSongs, await fixture.Context.YellowRecentSongs.CountAsync(row => row.Baid == 1));
    }

    [Fact]
    public async Task UpdatePlayResult_Yellow_TokkunShapedPayloadReturnsSuccessWithoutNormalOrProfileWrites()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var saveData = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        saveData.TotalGetDonmedal = 5;
        saveData.TotalGetKatsumedal = 7;
        saveData.CategJpopCnt = 3;
        saveData.SongPushedCnt = 4;
        saveData.LastPlayDatetime = new DateTime(2026, 6, 1, 8, 0, 0);
        fixture.Context.UserSaveDataYellow.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var request = new CommonPlayResultData
        {
            Baid = 1,
            PlayMode = (uint)PlayMode.Tokkun,
            IsTokkunPlayResult = true,
            TokkunTutorialFlg = 7,
            TokkunStageData = new CommonPlayResultData.TokkunStageDataDto
            {
                BanacoinDatetime = "20260608120000",
                TokkunSongCnt = 1,
                TookunSongnoes = [101],
                TokkunSpeedchangeCnt = 2,
                TokkunAutoplayCnt = 3,
                TokkunJumpCnt = 4
            },
            GetDonmedal = 50,
            GetKatsumedal = 60,
            ReleaseSongNoes = [104],
            GetToneNoes = [8],
            GetCostumeNo1s = [1],
            GetTitleNoes = [11],
            AryCurrentCostume = new CommonPlayResultData.CostumeData { Costume1 = 1 },
            AryStageInfoes = [CreateStage(101, 1, 0)]
        };

        var result = await handler.Handle(new UpdatePlayResultCommand(1, GameEra.Yellow, request), CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.Equal(5u, reloaded.TotalGetDonmedal);
        Assert.Equal(7u, reloaded.TotalGetKatsumedal);
        Assert.Equal(3u, reloaded.CategJpopCnt);
        Assert.Equal(4u, reloaded.SongPushedCnt);
        Assert.Equal(new DateTime(2026, 6, 1, 8, 0, 0), reloaded.LastPlayDatetime);
        Assert.False(BitIsSet(reloaded.ReleaseSongFlg, 104));
        Assert.False(BitIsSet(reloaded.ToneFlg, 8));
        Assert.False(BitIsSet(reloaded.CostumeFlg1, 1));
        Assert.False(BitIsSet(reloaded.TitleFlg, 11));
        Assert.Null(reloaded.TokkunTutorialFlg);
        Assert.Empty(await fixture.Context.SongPlayDataYellow.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataYellow.ToListAsync());
        Assert.Empty(await fixture.Context.YellowFavoriteSongs.ToListAsync());
        Assert.Empty(await fixture.Context.YellowRecentSongs.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Yellow_NormalUploadFeedsUserdataSelfBestAndCrownsReadback()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260608120000",
                GetDonmedal = 10,
                GetKatsumedal = 2,
                ReleaseSongNoes = [104],
                GetToneNoes = [4],
                GetTitleNoes = [10],
                HasDifficultyPlayedCourse = true,
                DifficultyPlayedCourse = 4,
                HasDifficultyPlayedStar = true,
                DifficultyPlayedStar = 8,
                AreaCode = 12,
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var userdataHandler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));
        var userdata = await userdataHandler.Handle(new UserDataQuery(1, GameEra.Yellow), CancellationToken.None);

        Assert.Equal(1u, userdata.Result);
        Assert.Contains(101u, userdata.AryFavoriteSongNoes);
        Assert.Equal([101u], userdata.AryRecentSongNoes);
        Assert.True(BitIsSet(userdata.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(userdata.ToneFlg, 4));
        Assert.True(BitIsSet(userdata.TitleFlg, 10));
        Assert.Equal(1u, userdata.CategJpopCnt);
        Assert.Equal(1u, userdata.SongPushedCnt);
        Assert.Equal(1u, userdata.SongFavoriteCnt);
        Assert.Equal(1u, userdata.SongRecentCnt);
        Assert.Equal(12u, userdata.PrevAreaCode);
        Assert.Equal(4u, userdata.DifficultyPlayedCourse);
        Assert.Equal(8u, userdata.DifficultyPlayedStar);

        var selfBestHandler = new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance);
        var selfBest = await selfBestHandler.Handle(new GetSelfBestQuery(1, GameEra.Yellow, 1, [101]), CancellationToken.None);

        var selfBestRow = Assert.Single(selfBest.ArySelfbestScores);
        Assert.Equal(101u, selfBestRow.SongNo);
        Assert.Equal(765432u, selfBestRow.SelfBestScore);

        using var provider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var crownsController = new CrownsDataController(fixture.Context, fixture.Catalog)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = provider }
            }
        };
        var crownsResult = await crownsController.CrownsData(new CrownsDataRequest
        {
            Baid = 1,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123"
        });

        var ok = Assert.IsType<OkObjectResult>(crownsResult);
        var crowns = Assert.IsType<CrownsDataResponse>(ok.Value);
        Assert.Equal(1u, crowns.Result);
        Assert.Equal(789u, crowns.SongHashVer);
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.CrownPackedBytes, crowns.HashCrownFlg.Length);
        Assert.Equal((ushort)0b0000000011, ReadTenBitValue(crowns.HashCrownFlg, 101));

        Assert.Empty(await fixture.Context.DanScoreDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueBattleStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public void PlayResultMapper_Yellow_MapsNormalWirePayloadIntoCommonData()
    {
        var request = CreateWireRequest(1);
        request.GetDonmedal = 10;
        request.GetKatsumedal = 2;
        request.GetToneNoes = [4];
        request.GetCostumeNo1s = [1];
        request.GetCostumeNo2s = [2];
        request.GetCostumeNo3s = [3];
        request.GetCostumeNo4s = [4];
        request.GetCostumeNo5s = [5];
        request.GetTitleNoes = [10];
        request.ReleaseSongNoes = [104];
        request.ItemshopTutorialFlg = 7;
        request.IsDevil = true;
        request.IsExplain = true;
        request.AryCurrentCostume = new PlayResultRequest.CostumeData
        {
            Costume1 = 1,
            Costume2 = 2,
            Costume3 = 3,
            Costume4 = 4,
            Costume5 = 5
        };
        request.DifficultyPlayedCourse = 3;
        request.DifficultyPlayedStar = 8;
        request.AryStageInfoes.Add(new PlayResultRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            StageMode = 0,
            PlayResult = 2,
            PlayScore = 765432,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            HitCnt = 123,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            IsPapamama = false,
            PlayDan = 0,
            SoulGauge = 100,
            SelectedFolderId = 9
        });

        var common = PlayResultMappers.Map(request);

        Assert.Equal(1u, common.Baid);
        Assert.Equal("268410000000", common.ChassisId);
        Assert.Equal("JPN0JPN0123", common.ShopId);
        Assert.Equal("20260608120000", common.PlayDatetime);
        Assert.Equal([104u], common.ReleaseSongNoes);
        Assert.Equal([4u], common.GetToneNoes);
        Assert.Equal([1u], common.GetCostumeNo1s);
        Assert.Equal([2u], common.GetCostumeNo2s);
        Assert.Equal([3u], common.GetCostumeNo3s);
        Assert.Equal([4u], common.GetCostumeNo4s);
        Assert.Equal([5u], common.GetCostumeNo5s);
        Assert.Equal([10u], common.GetTitleNoes);
        Assert.Equal(10u, common.GetDonmedal);
        Assert.Equal(2u, common.GetKatsumedal);
        Assert.Equal(7u, common.ItemshopTutorialFlg);
        Assert.True(common.IsDevil);
        Assert.True(common.IsExplain);
        Assert.True(common.HasAryCurrentCostume);
        Assert.Equal(1u, common.AryCurrentCostume.Costume1);
        Assert.True(common.HasDifficultyPlayedCourse);
        Assert.True(common.HasDifficultyPlayedStar);
        Assert.Equal(3u, common.DifficultyPlayedCourse);
        Assert.Equal(8u, common.DifficultyPlayedStar);
        Assert.False(common.IsTokkunPlayResult);

        var stage = Assert.Single(common.AryStageInfoes);
        Assert.Equal(101u, stage.SongNo);
        Assert.Equal(1u, stage.Level);
        Assert.Equal(0u, stage.StageMode);
        Assert.Equal(765432u, stage.PlayScore);
        Assert.Equal(123u, stage.HitCnt);
        Assert.Equal([1, 2, 3], stage.OptionFlg);
        Assert.Equal([4], stage.ToneFlg);
        Assert.True(stage.IsPushed);
        Assert.True(stage.IsFavorite);
        Assert.True(stage.IsRecent);
        Assert.Equal(100u, stage.SoulGauge);
        Assert.Null(stage.PlayDan);
        Assert.Equal(9u, stage.SelectedFolderId);
    }

    [Fact]
    public void PlayResultMapper_Yellow_UsesTokkunFieldsOnlyForNoWriteDetection()
    {
        var request = CreateWireRequest(1);
        request.PlayMode = (uint)PlayMode.Normal;
        request.TokkunTutorialFlg = 9;
        request.AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
        {
            BanacoinDatetime = "20260608120100",
            TokkunSongCnt = 2,
            TookunSongnoes = [101, 102],
            TokkunSpeedchangeCnt = 3,
            TokkunAutoplayCnt = 4,
            TokkunJumpCnt = 5
        };

        var common = PlayResultMappers.Map(request);

        Assert.True(common.IsTokkunPlayResult);
        Assert.Equal(9u, common.TokkunTutorialFlg);
        Assert.NotNull(common.TokkunStageData);
        Assert.Equal("20260608120100", common.TokkunStageData!.BanacoinDatetime);
        Assert.Equal([101u, 102u], common.TokkunStageData.TookunSongnoes);
    }

    [Fact]
    public void PlayResultMapper_Yellow_MapsResultIntoYellowWireResponse()
    {
        var response = PlayResultMappers.Map(1);

        Assert.Equal(1u, response.Result);
    }

    private static UpdatePlayResultCommandHandler CreateHandler(YellowHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

    private static CommonPlayResultData.StageData CreateStage(
        uint songNo,
        uint level,
        uint stageMode,
        uint score = 765432)
    {
        return new CommonPlayResultData.StageData
        {
            SongNo = songNo,
            Level = level,
            StageMode = stageMode,
            PlayResult = 2,
            PlayScore = score,
            ScoreRate = 95,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            HitCnt = 123,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            SelectedFolderId = 9,
            SoulGauge = 100
        };
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private static ushort ReadTenBitValue(byte[] packed, int songIndex)
    {
        ushort value = 0;
        var bitOffset = songIndex * 10;
        for (var bit = 0; bit < 10; bit++)
        {
            var absoluteBit = bitOffset + bit;
            if ((packed[absoluteBit >> 3] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= (ushort)(1 << bit);
            }
        }

        return value;
    }

    private static PlayResultRequest CreateWireRequest(uint baid)
        => new()
        {
            Baid = baid,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            PlayDatetime = "20260608120000",
            IsRight = false,
            CardType = 1,
            IsTwoPlayers = false,
            BonusDailyFlg = false,
            BonusWeeklyFlg = false,
            BonusMonthlyFlg = false,
            GenderType = 0,
            PlayerAge = 0,
            PlayMode = (uint)PlayMode.Normal,
            AreaCode = 1,
            Reserved = new byte[16]
        };
}
