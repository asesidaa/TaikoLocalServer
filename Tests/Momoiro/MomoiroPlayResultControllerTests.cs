using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroPlayResultControllerTests
{
    [Fact]
    public async Task PlayResultController_Momoiro_RedContractMapsDirectProtobufRequestToRuntimeState()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(4, "44444444444444444444", "CTRL");
        await fixture.SeedMomoiroSaveAsync(4, totalGetDonpoint: 5, rewardPtn: 1, rewardProgress: 2);
        await fixture.SeedMomoiroFavoriteAsync(4, 300, 0);
        await fixture.SeedMomoiroFavoriteAsync(4, 101, 1);
        var unsupportedBefore = await fixture.CountSelectedAdjacentUnsupportedRowsAsync(4);
        using var provider = fixture.BuildServiceProvider();

        var response = await InvokeActionAsync<PlayResultController, PlayResultResponse>(
            provider,
            nameof(PlayResultController.PlayResult),
            CreateRequest());

        Assert.Equal(1u, response.Result);
        Assert.Equal(2, await fixture.CountMomoiroPlayRowsAsync(4));
        Assert.Equal(1, await fixture.CountMomoiroDanRowsAsync(4));
        Assert.Equal(2, await fixture.CountMomoiroDanStageRowsAsync(4));

        var save = await fixture.Context.UserSaveDataMomoiro.SingleAsync(row => row.Baid == 4);
        Assert.Equal(38u, save.TotalGetDonpoint);
        Assert.Equal(0u, save.TotalUseDonpoint);
        Assert.Equal(7u, save.RewardPtn);
        Assert.Equal(11u, save.RewardProgress);
        Assert.True(BitIsSet(save.ReleaseSongFlg, MomoiroHandlerFixture.HighSongNo));
        Assert.NotEqual(0u, save.GotDanMax);

        var favorite = await fixture.Context.MomoiroFavoriteSongs.FindAsync(4u, 250u);
        Assert.NotNull(favorite);
        Assert.Equal(2, favorite!.DisplayOrder);

        var userdata = await new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()))
            .Handle(new Ac15UserDataQuery(4, GameEra.Momoiro), CancellationToken.None);
        Assert.Equal([300u, 101u, 250u, 102u], userdata.SongLists.AryFavoriteSongNoes);
        await AssertSelectedAdjacentUnsupportedRowsUnchangedAsync(fixture, 4, unsupportedBefore);
    }

    private static PlayResultRequest CreateRequest()
    {
        var request = new PlayResultRequest
        {
            Baid = 4,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            PlayDatetime = "20260608120000",
            CardType = 1,
            IsRight = false,
            IsTwoPlayers = false,
            ReleaseSongNoes = [MomoiroHandlerFixture.HighSongNo],
            GetDonpoint = 33,
            RewardPtn = 7,
            RewardProgress = 11,
            DanResult = (uint)Ac15DanClearGrade.GoldClear,
            PlayMode = (uint)PlayMode.DanMode,
            AreaCode = 12,
            Reserved = [],
            Accesstoken = string.Empty,
            ContentInfo = [],
            AryCurrentCostume = new PlayResultRequest.CustumeData { Costume1 = 1 }
        };

        request.AryStageInfoes.Add(CreateStage(250, score: 100_000, playDan: 1, soulGauge: 55, comboCnt: 120, goodCnt: 100, okCnt: 20, ngCnt: 4));
        request.AryStageInfoes.Add(CreateStage(102, score: 200_000, playDan: 1, soulGauge: 88, comboCnt: 220, goodCnt: 180, okCnt: 30, ngCnt: 2));
        request.AryStageInfoes[0].AryChallengeIds.Add(new PlayResultRequest.StageData.ResultcompeData { CompeId = 42, TrackNo = 1 });
        request.AryStageInfoes[0].AryUserCompeIds.Add(new PlayResultRequest.StageData.ResultcompeData { CompeId = 43, TrackNo = 2 });
        request.AryStageInfoes[0].AryBngCompeIds.Add(new PlayResultRequest.StageData.ResultcompeData { CompeId = 44, TrackNo = 3 });
        return request;
    }

    private static PlayResultRequest.StageData CreateStage(
        uint songNo,
        uint score,
        uint playDan,
        uint soulGauge,
        uint comboCnt,
        uint goodCnt,
        uint okCnt,
        uint ngCnt)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            StageMode = 0,
            PlayResult = 2,
            PlayScore = score,
            GoodCnt = goodCnt,
            OkCnt = okCnt,
            NgCnt = ngCnt,
            PoundCnt = 4,
            ComboCnt = comboCnt,
            HitCnt = 123,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            SongRemTime = 0,
            LevelRemTime = 0,
            IsPapamama = false,
            PlayDan = playDan,
            SoulGauge = soulGauge
        };

    private static async Task<TResponse> InvokeActionAsync<TController, TResponse>(
        IServiceProvider services,
        string actionName,
        object request)
        where TController : ControllerBase
    {
        var controller = ActivatorUtilities.CreateInstance<TController>(services);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services }
        };

        var method = typeof(TController).GetMethod(actionName, [request.GetType()]);
        Assert.NotNull(method);

        var result = method.Invoke(controller, [request]);
        if (result is Task<IActionResult> task)
        {
            result = await task;
        }
        else if (result is ValueTask<IActionResult> valueTask)
        {
            result = await valueTask;
        }

        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsType<TResponse>(ok.Value);
    }

    private static async Task AssertSelectedAdjacentUnsupportedRowsUnchangedAsync(
        MomoiroHandlerFixture fixture,
        uint baid,
        IReadOnlyDictionary<string, int> before)
    {
        var after = await fixture.CountSelectedAdjacentUnsupportedRowsAsync(baid);
        Assert.Equal(before.OrderBy(pair => pair.Key), after.OrderBy(pair => pair.Key));
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
