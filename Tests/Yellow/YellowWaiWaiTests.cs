using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowWaiWaiTests
{
    [Fact]
    public void YellowPlayResultMapper_MapsOnlyProtocolBackedWaiWaiStageFacts()
    {
        var request = CreateWireRequest(1);
        request.WaiwaiTutorialFlg = 11;
        var wireStage = new PlayResultRequest.StageData
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
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            IsPapamama = false,
            PlayDan = 0,
            SelectedFolderId = 9,
            WaiwaiResult = 4,
            WaiwaiGauge = 88
        };
        request.AryStageInfoes.Add(wireStage);

        var envelope = PlayResultMappers.Map(request);

        Assert.Equal(11u, envelope.Profile.WaiwaiTutorialFlg);
        var stage = Assert.Single(envelope.Normal!.Stages);
        Assert.Equal(4u, stage.WaiwaiResult);
        Assert.Equal(88u, stage.WaiwaiGauge);
    }

    [Fact]
    public async Task YellowPlayResult_PersistsProtocolBackedWaiWaiTutorialFlag()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        save.WaiwaiTutorialFlg = 3;
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 1,
                WaiwaiTutorialFlg = 11,
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.Equal(11u, reloaded.WaiwaiTutorialFlg);
    }

    [Fact]
    public async Task YellowPlayResult_PreservesWaiWaiFactsOnlyAsPlayHistory()
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
                    CreateStage(101, 1, 0, waiwaiResult: 4, waiwaiGauge: 88)
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(4u, play.WaiwaiResult);
        Assert.Equal(88u, play.WaiwaiGauge);

        var best = await fixture.Context.SongBestDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.Equal(765432u, best.BestScore);
        var save = await fixture.Context.UserSaveDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.Equal(0u, save.WaiwaiTutorialFlg);
        Assert.Empty(await fixture.Context.DanScoreDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowShopItemStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueBattleStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
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
        uint waiwaiResult = 0,
        uint waiwaiGauge = 0)
        => new()
        {
            SongNo = songNo,
            Level = level,
            StageMode = stageMode,
            PlayResult = 2,
            PlayScore = 765432,
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
            SoulGauge = 100,
            WaiwaiResult = waiwaiResult,
            WaiwaiGauge = waiwaiGauge
        };

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
