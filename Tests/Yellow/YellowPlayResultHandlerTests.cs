using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowPlayResultHandlerTests
{
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
