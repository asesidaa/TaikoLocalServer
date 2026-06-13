using System.Text.Json;
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Tests.Ac15;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BluePlayResultHandlerTests
{
    [Fact]
    public async Task UpdatePlayResult_Blue_GuestBaidDoesNotSave()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            0,
            GameEra.Blue,
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_UnknownUserDoesNotSave()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            99,
            GameEra.Blue,
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_TokkunExistingUserPersistsAllowedStateOnly()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var saveData = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        saveData.TotalGetDonmedal = 5;
        saveData.TotalGetKatsumedal = 7;
        saveData.CategJpopCnt = 3;
        saveData.SongPushedCnt = 4;
        saveData.LastPlayDatetime = new DateTime(2026, 5, 1, 8, 0, 0);
        saveData.Title = "Stable Title";
        saveData.TitleplateId = 10;
        saveData.Costume1 = 6;
        saveData.CostumeFlg1 = BlueProtocolBytes.CreateFixedBitset([0, 6], BlueProtocolBytes.CostumeFlagBytes);
        saveData.ReleaseSongFlg = BlueProtocolBytes.CreateFixedBitset([99], BlueProtocolBytes.SongFlagBytes);
        saveData.ToneFlg = BlueProtocolBytes.CreateFixedBitset([4], BlueProtocolBytes.ToneFlagBytes);
        saveData.TitleFlg = BlueProtocolBytes.CreateFixedBitset([10], BlueProtocolBytes.TitleFlagBytes);
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var request = CreateTokkunRequest(1);
        request.TokkunTutorialFlg = 7;
        request.GetDonmedal = 50;
        request.GetKatsumedal = 60;
        request.ReleaseSongNoes = [104];
        request.GetToneNoes = [8];
        request.GetCostumeNo1s = [1];
        request.GetTitleNoes = [11];
        request.AryCurrentCostume = new PlayResultRequest.CostumeData { Costume1 = 1 };
        request.AryStageInfoes.Add(CreateWireStage(101, 1, 0));

        var result = await handler.Handle(CreateBlueCommand(request), CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataBlue.SingleAsync(row => row.Baid == 1);
        Assert.Equal(5u, reloaded.TotalGetDonmedal);
        Assert.Equal(7u, reloaded.TotalGetKatsumedal);
        Assert.Equal(3u, reloaded.CategJpopCnt);
        Assert.Equal(4u, reloaded.SongPushedCnt);
        Assert.Equal(new DateTime(2026, 5, 1, 8, 0, 0), reloaded.LastPlayDatetime);
        Assert.Equal("Stable Title", reloaded.Title);
        Assert.Equal(10u, reloaded.TitleplateId);
        Assert.Equal(6u, reloaded.Costume1);
        Assert.True(BitIsSet(reloaded.CostumeFlg1, 6));
        Assert.False(BitIsSet(reloaded.CostumeFlg1, 1));
        Assert.True(BitIsSet(reloaded.ReleaseSongFlg, 99));
        Assert.False(BitIsSet(reloaded.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(reloaded.ToneFlg, 4));
        Assert.False(BitIsSet(reloaded.ToneFlg, 8));
        Assert.True(BitIsSet(reloaded.TitleFlg, 10));
        Assert.False(BitIsSet(reloaded.TitleFlg, 11));
        Assert.Equal(7u, reloaded.TokkunTutorialFlg);
        var tokkunStage = await fixture.Context.BlueTokkunStageResults.SingleAsync(row => row.Baid == 1);
        AssertTokkunHistoryRow(tokkunStage, "20260528120000", "20260528120000", [101]);
        await AssertTokkunForbiddenBlueStateEmptyAsync(fixture.Context);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_TokkunUnknownUserReturnsSuccessWithoutCreatingRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = CreateHandler(fixture);
        var request = CreateTokkunRequest(99);
        request.AryStageInfoes.Add(CreateWireStage(101, 1, 0));

        var result = await handler.Handle(CreateBlueCommand(request), CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.ToListAsync());
        await AssertTokkunForbiddenBlueStateEmptyAsync(fixture.Context);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_MixedTokkunPayloadReturnsSuccessBeforeBattleOrNormalWrites()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        var saveData = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        saveData.TotalGetDonmedal = 5;
        saveData.TotalGetKatsumedal = 7;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var request = CreateTokkunRequest(1);
        request.TokkunTutorialFlg = 7;
        request.GetDonmedal = 50;
        request.GetKatsumedal = 60;
        request.ReleaseSongNoes = [104];
        request.GetToneNoes = [8];
        request.GetCostumeNo1s = [1];
        request.GetTitleNoes = [11];
        request.AryReleaseBattledata = CreateReleaseBattleData(assignNextStageId: 12);
        request.AryStageInfoes.Add(CreateWireStage(101, 1, 0, includeBattle: true));

        var result = await handler.Handle(CreateBlueCommand(request), CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataBlue.SingleAsync(row => row.Baid == 1);
        Assert.Equal(5u, reloaded.TotalGetDonmedal);
        Assert.Equal(7u, reloaded.TotalGetKatsumedal);
        Assert.False(BitIsSet(reloaded.ReleaseSongFlg, 104));
        Assert.False(BitIsSet(reloaded.ToneFlg, 8));
        Assert.False(BitIsSet(reloaded.CostumeFlg1, 1));
        Assert.False(BitIsSet(reloaded.TitleFlg, 11));
        Assert.Equal(7u, reloaded.TokkunTutorialFlg);
        var tokkunStage = await fixture.Context.BlueTokkunStageResults.SingleAsync(row => row.Baid == 1);
        AssertTokkunHistoryRow(tokkunStage, "20260528120000", "20260528120000", [101]);
        await AssertTokkunForbiddenBlueStateEmptyAsync(fixture.Context);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_TokkunRepeatedUploadsAppendHistoryRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);
        var first = CreateTokkunRequest(1);
        first.TokkunTutorialFlg = 7;
        first.AryTokkunstageInfo!.TookunSongnoes = [101, 102, 101];
        first.AryTokkunstageInfo.TokkunSongCnt = 3;
        var second = CreateTokkunRequest(1);
        second.TokkunTutorialFlg = 1;
        second.AryTokkunstageInfo!.TookunSongnoes = [101, 102, 101];
        second.AryTokkunstageInfo.TokkunSongCnt = 3;

        var firstResult = await handler.Handle(CreateBlueCommand(first), CancellationToken.None);
        var secondResult = await handler.Handle(CreateBlueCommand(second), CancellationToken.None);

        Assert.Equal(1u, firstResult);
        Assert.Equal(1u, secondResult);
        var reloaded = await fixture.Context.UserSaveDataBlue.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, reloaded.TokkunTutorialFlg);
        var rows = await fixture.Context.BlueTokkunStageResults
            .Where(row => row.Baid == 1)
            .OrderBy(row => row.Id)
            .ToListAsync();
        Assert.Equal(2, rows.Count);
        AssertTokkunHistoryRow(rows[0], "20260528120000", "20260528120000", [101, 102, 101], songCount: 3);
        AssertTokkunHistoryRow(rows[1], "20260528120000", "20260528120000", [101, 102, 101], songCount: 3);
        await AssertTokkunForbiddenBlueStateEmptyAsync(fixture.Context);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_TutorialOnlyTokkunFactUpdatesFlagWithoutHistory()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var saveData = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        saveData.TokkunTutorialFlg = 5;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);
        var request = CreateTokkunRequest(1);
        request.PlayMode = (uint)PlayMode.Normal;
        request.TokkunTutorialFlg = 7;
        request.AryTokkunstageInfo = null;

        var result = await handler.Handle(CreateBlueCommand(request), CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataBlue.SingleAsync(row => row.Baid == 1);
        Assert.Equal(7u, reloaded.TokkunTutorialFlg);
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_SavesPlayBestCountersAndUnlocks()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var profile = Ac15ProfileMutationFacts.Empty with
        {
            GetDonmedal = 10,
            GetKatsumedal = 2,
            GetToneNoes = [4],
            GetCostumeNo1s = [1],
            GetCostumeNo2s = [2],
            GetCostumeNo3s = [3],
            GetCostumeNo4s = [4],
            GetCostumeNo5s = [5],
            GetTitleNoes = [10],
            ReleaseSongNoes = [104],
            HasAryCurrentCostume = true,
            AryCurrentCostume = new Ac15CostumeFacts(1, 2, 3, 4, 5)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playDatetime: "2026-05-28 12:00:00",
            profile: profile,
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await fixture.Context.SongPlayDataBlue.Where(row => row.Baid == 1).ToListAsync());
        var best = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(765432u, best!.BestScore);
        Assert.Equal(CrownType.Gold, best.BestCrown);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(10u, save!.TotalGetDonmedal);
        Assert.Equal(2u, save.TotalGetKatsumedal);
        Assert.True(BitIsSet(save.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(save.ToneFlg, 4));
        Assert.True(BitIsSet(save.CostumeFlg1, 1));
        Assert.True(BitIsSet(save.CostumeFlg2, 2));
        Assert.True(BitIsSet(save.CostumeFlg3, 3));
        Assert.True(BitIsSet(save.CostumeFlg4, 4));
        Assert.True(BitIsSet(save.CostumeFlg5, 5));
        Assert.True(BitIsSet(save.TitleFlg, 10));
        Assert.Equal(1u, save.Costume1);
        Assert.Equal(1u, save.CategJpopCnt);
        Assert.Equal(1u, save.SongPushedCnt);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_StoresCompactProtocolPlayDatetimeWithoutWarning()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var logger = new RecordingLogger<UpdatePlayResultCommandHandler>();
        var handler = CreateHandler(fixture, logger);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playDatetime: "20260528120000",
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(new DateTime(2026, 5, 28, 12, 0, 0), save!.LastPlayDatetime);
        Assert.DoesNotContain(logger.Events, log => log.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_SavesNormalAndShinBestSeparately()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            stages:
            [
                CreateStage(101, 1, 0, score: 100000),
                CreateStage(101, 1, 1, score: 200000)
            ]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var normal = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, false);
        var shin = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, true);
        Assert.NotNull(normal);
        Assert.NotNull(shin);
        Assert.Equal(100000u, normal!.BestScore);
        Assert.Equal(200000u, shin!.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_UnsupportedStageModeSkipsStageAndReturnsSuccess()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            stages:
            [
                CreateStage(101, 1, 3),
                CreateStage(102, 1, 4),
                CreateStage(103, 1, 99)
            ]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_FavoriteAndRecentUseBlueTables()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var stages = Enumerable.Range(101, 12)
            .Select(song => CreateStage((uint)song, 1, 0))
            .ToList();

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playDatetime: "2026-05-28 12:00:00",
            stages: stages),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Equal(5, await fixture.Context.BlueFavoriteSongs.CountAsync(row => row.Baid == 1));
        Assert.Equal(10, await fixture.Context.BlueRecentSongs.CountAsync(row => row.Baid == 1));
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniSavesDanRowsAndKeepsNormalStageRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var danStages = new List<Ac15StageResult>
        {
            CreateDanStage(101, 1, 326090, 124, 14, 0, 139, 138, 277, 51),
            CreateDanStage(102, 1, 593280, 230, 33, 3, 287, 156, 550, 99),
            CreateDanStage(103, 1, 818490, 314, 49, 6, 342, 156, 705, 100)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playMode: (uint)PlayMode.DanMode,
            playDatetime: "20260528120000",
            stages: danStages,
            dani: new Ac15DaniPlayResult(DanResult: 2, ComboCntTotal: 300, Stages: danStages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Equal(3, await fixture.Context.SongPlayDataBlue.CountAsync(row => row.Baid == 1));
        var dan = await fixture.Context.DanScoreDataBlue
            .Include(row => row.DanStageScoreData)
            .SingleAsync(row => row.Baid == 1 && row.DanId == 1 && !row.IsExtra);
        Assert.Equal(20001u, dan.MedleyUniqueId);
        Assert.Equal(Ac15DanClearGrade.GoldClear, dan.ClearGrade);
        Assert.Equal(3u, dan.ArrivalSongCount);
        Assert.Equal(100u, dan.SoulGaugeTotal);
        Assert.Equal(300u, dan.ComboCountTotal);
        Assert.Equal(3, dan.DanStageScoreData.Count);
        Assert.Equal(326090u, dan.DanStageScoreData.Single(stage => stage.StageIndex == 0).HighScore);
        var normalBest = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.Null(normalBest);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(1u, save!.GotDanMax);
        Assert.Equal(2u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniRejectsInvalidDanResultButKeepsNormalPlaySave()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var invalidDanStages = new List<Ac15StageResult>
        {
            CreateDanStage(101, 1, 123, 1, 2, 3, 4, 5, 6, 7)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playMode: (uint)PlayMode.DanMode,
            stages: invalidDanStages,
            dani: new Ac15DaniPlayResult(DanResult: 3, ComboCntTotal: 0, Stages: invalidDanStages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniUnknownDanSkipsOnlyDanRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var unknownDanStages = new List<Ac15StageResult>
        {
            CreateDanStage(101, 999, 123, 1, 2, 3, 4, 5, 6, 7)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playMode: (uint)PlayMode.DanMode,
            stages: unknownDanStages,
            dani: new Ac15DaniPlayResult(DanResult: 1, ComboCntTotal: 0, Stages: unknownDanStages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniExtraClearUpdatesExtraFlagsOnly()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(new BlueHandlerFixture.TestBlueCatalog(
            taikojukuFileOrder:
            [
                new BlueTaikojukuEntry
                {
                    UniqueId = 20101,
                    ChallengeLevel = 101,
                    VerupNo = 0,
                    Songs = [new BlueTaikojukuSong { SongNo = 101, Level = 1 }]
                }
            ]));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var extraDanStages = new List<Ac15StageResult>
        {
            CreateDanStage(101, 101, 100, 10, 2, 1, 4, 12, 13, 100)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playMode: (uint)PlayMode.DanMode,
            stages: extraDanStages,
            dani: new Ac15DaniPlayResult(DanResult: 2, ComboCntTotal: 0, Stages: extraDanStages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(0u, save!.GotDanMax);
        Assert.Equal(1u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(save.GotDanExtraFlg, 0));
        Assert.Equal(Ac15DanClearGrade.NotClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));
    }

    [Theory]
    [InlineData(true, 36u, true)]
    [InlineData(false, 7u, false)]
    public async Task UpdatePlayResult_Blue_DaniClearAppliesSpecialDanCostumeOnlyWhenAutoCostumeOn(
        bool isAutoCostumeOn,
        uint expectedCostume,
        bool shouldUnlockDanCostume)
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.IsAutoCostumeOn = isAutoCostumeOn;
        save.Costume1 = 7;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var costumeDanStages = new List<Ac15StageResult>
        {
            CreateDanStage(101, 1, 100, 1, 2, 3, 4, 5, 6, 100)
        };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playMode: (uint)PlayMode.DanMode,
            profile: Ac15ProfileMutationFacts.Empty with
            {
                HasAryCurrentCostume = true,
                AryCurrentCostume = new Ac15CostumeFacts(7, 0, 0, 0, 0)
            },
            stages: costumeDanStages,
            dani: new Ac15DaniPlayResult(DanResult: 1, ComboCntTotal: 0, Stages: costumeDanStages)),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(reloaded);
        Assert.Equal(expectedCostume, reloaded!.Costume1);
        Assert.Equal(shouldUnlockDanCostume, BitIsSet(reloaded.CostumeFlg1, 36));
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniDuplicateSongsKeepStageIndexRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var duplicateDanStages = new List<Ac15StageResult>
        {
            CreateDanStage(101, 1, 100, 1, 2, 3, 4, 5, 6, 7),
            CreateDanStage(101, 1, 200, 2, 3, 4, 5, 6, 7, 8)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Blue,
            playMode: (uint)PlayMode.DanMode,
            stages: duplicateDanStages,
            dani: new Ac15DaniPlayResult(DanResult: 1, ComboCntTotal: 0, Stages: duplicateDanStages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var rows = await fixture.Context.DanStageScoreDataBlue
            .Where(row => row.Baid == 1 && row.DanId == 1)
            .OrderBy(row => row.StageIndex)
            .ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(0u, rows[0].StageIndex);
        Assert.Equal(1u, rows[1].StageIndex);
        Assert.Equal(100u, rows[0].HighScore);
        Assert.Equal(200u, rows[1].HighScore);
    }

    private static UpdatePlayResultCommandHandler CreateHandler(
        BlueHandlerFixture fixture,
        ILogger<UpdatePlayResultCommandHandler>? logger = null)
    {
        return new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            logger ?? NullLogger<UpdatePlayResultCommandHandler>.Instance);
    }

    private static BlueHandlerFixture.TestBlueCatalog CreateShopCatalog()
        => new(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = new()
                {
                    SeasonId = 2,
                    VerupNo = 7,
                    Items =
                    [
                        new BlueItemShopEntry
                        {
                            ItemNo = 1,
                            ItemType = Ac15ShopItemType.Song,
                            ItemId = 101,
                            Price = 1300
                        }
                    ]
                }
            }
        });

    private static UpdateAc15PlayResultCommand CreateBlueCommand(PlayResultRequest request)
        => new(request.Baid, GameEra.Blue, PlayResultMappers.Map(request));

    private static PlayResultRequest CreateTokkunRequest(uint baid) => new()
    {
        Baid = baid,
        ChassisId = "268410000000",
        ShopId = "JPN0JPN0123",
        PlayDatetime = "20260528120000",
        IsRight = false,
        CardType = 1,
        IsTwoPlayers = false,
        BonusDailyFlg = false,
        BonusWeeklyFlg = false,
        BonusMonthlyFlg = false,
        GetDonmedal = 0,
        GetKatsumedal = 0,
        GenderType = 0,
        PlayerAge = 0,
        PlayMode = (uint)PlayMode.Tokkun,
        AreaCode = 1,
        Reserved = new byte[16],
        TokkunTutorialFlg = 1,
        AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
        {
            BanacoinDatetime = "20260528120000",
            TokkunSongCnt = 1,
            TookunSongnoes = [101],
            TokkunSpeedchangeCnt = 2,
            TokkunAutoplayCnt = 3,
            TokkunJumpCnt = 4
        }
    };

    private static PlayResultRequest.StageData CreateWireStage(
        uint songNo,
        uint level,
        uint stageMode,
        bool includeBattle = false)
    {
        var stage = new PlayResultRequest.StageData
        {
            SongNo = songNo,
            Level = level,
            StageMode = stageMode,
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
            SelectedFolderId = 9,
            SoulGauge = 100
        };

        if (includeBattle)
        {
            stage.AryBattlestagedata = new PlayResultRequest.StageData.BattleStageData
            {
                SupportLv = 3,
                BattleStageId = 12,
                NpcData = new PlayResultRequest.StageData.BattleStageData.BattleNpcData
                {
                    NpcId = 9,
                    AcquiredExp = "77",
                    TotalExp = "888",
                    Dpn = 456,
                    NpcCostumeId = 30,
                    SpecialId1 = 21,
                    SpecialId2 = 22,
                    SpecialId3 = 23,
                    BondsLv = 6
                },
                KillCnt = 5,
                BossLife = 12345,
                TotalDamage = 54321,
                CriticalCnt = 7,
                SpecialMoveCnt = 2
            };
        }

        return stage;
    }

    private static PlayResultRequest.ReleaseBattleData CreateReleaseBattleData(uint assignNextStageId)
    {
        var release = new PlayResultRequest.ReleaseBattleData
        {
            ReleaseInfoIds = [101],
            ReleaseBattleStageIds = [2],
            ReleaseNpcIds = [4],
            ReleaseNpcCostumeIds = [5],
            ReleaseNpcSpecialIds = [6],
            AssignNextStageId = assignNextStageId
        };
        release.AryBattletokendatas.Add(new PlayResultRequest.ReleaseBattleData.BattleTokenData
        {
            TokenId = 17,
            TokenValue = 765
        });

        return release;
    }

    private static Ac15StageResult CreateStage(
        uint songNo,
        uint level,
        uint stageMode,
        uint score = 765432)
    {
        return new Ac15StageResult
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

    private static Ac15StageResult CreateDanStage(
        uint songNo,
        uint danId,
        uint score,
        uint good,
        uint ok,
        uint bad,
        uint drumroll,
        uint combo,
        uint hits,
        uint soulGauge)
    {
        return CreateStage(songNo, 1, 0, score) with
        {
            PlayDan = danId,
            PlayResult = 0,
            GoodCnt = good,
            OkCnt = ok,
            NgCnt = bad,
            PoundCnt = drumroll,
            ComboCnt = combo,
            HitCnt = hits,
            SoulGauge = soulGauge
        };
    }

    private static async Task AssertTokkunForbiddenBlueStateEmptyAsync(TaikoDbContext context)
    {
        Assert.Empty(await context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await context.SongBestDataBlue.ToListAsync());
        Assert.Empty(await context.BlueBattleStageResults.ToListAsync());
        Assert.Empty(await context.BlueBattleUserStates.ToListAsync());
        Assert.Empty(await context.BlueBattleNpcStates.ToListAsync());
        Assert.Empty(await context.BlueBattleTokenStates.ToListAsync());
        Assert.Empty(await context.BlueFavoriteSongs.ToListAsync());
        Assert.Empty(await context.BlueRecentSongs.ToListAsync());
        Assert.Empty(await context.DanScoreDataBlue.ToListAsync());
        Assert.Empty(await context.DanStageScoreDataBlue.ToListAsync());
        Assert.Empty(await context.BlueShopSeasonStates.ToListAsync());
        Assert.Empty(await context.BlueShopItemStates.ToListAsync());
    }

    private static void AssertTokkunHistoryRow(
        BlueTokkunStageResult row,
        string playDatetime,
        string banacoinDatetime,
        uint[] tookunSongnoes,
        uint songCount = 1)
    {
        Assert.True(row.Id > 0);
        Assert.Equal(playDatetime, row.PlayDatetime);
        Assert.Equal((uint)PlayMode.Tokkun, row.PlayMode);
        Assert.Equal(banacoinDatetime, row.BanacoinDatetime);
        Assert.Equal(songCount, row.TokkunSongCnt);
        var reloadedSongs = JsonSerializer.Deserialize<uint[]>(row.TookunSongnoesJson);
        Assert.NotNull(reloadedSongs);
        Assert.Equal(tookunSongnoes, reloadedSongs);
        Assert.Equal(2u, row.TokkunSpeedchangeCnt);
        Assert.Equal(3u, row.TokkunAutoplayCnt);
        Assert.Equal(4u, row.TokkunJumpCnt);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEvent> Events { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Events.Add(new LogEvent(logLevel, formatter(state, exception)));
        }
    }

    private sealed record LogEvent(LogLevel Level, string Message);
}
