using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BluePlayResultMapperTests
{
    private const uint TokkunPlayMode = (uint)PlayMode.Tokkun;

    [Fact]
    public void Map_BluePlayResult_PreservesDirectRequestFields()
    {
        var request = CreateRequest();
        request.ReleaseSongNoes = [104];
        request.GetToneNoes = [4];
        request.GetCostumeNo1s = [1];
        request.GetCostumeNo2s = [2];
        request.GetCostumeNo3s = [3];
        request.GetCostumeNo4s = [4];
        request.GetCostumeNo5s = [5];
        request.GetTitleNoes = [10];
        request.ItemshopTutorialFlg = 1;
        request.IsDevil = true;
        request.IsExplain = true;
        request.DifficultyPlayedCourse = 4;
        request.DifficultyPlayedStar = 8;
        request.AryCurrentCostume = new PlayResultRequest.CostumeData
        {
            Costume1 = 11,
            Costume2 = 12,
            Costume3 = 13,
            Costume4 = 14,
            Costume5 = 15
        };
        request.AryStageInfoes.Add(CreateStage(101, 1, 0));

        var envelope = PlayResultMappers.Map(request);

        Assert.Equal(1u, envelope.Metadata.Baid);
        Assert.Equal("20260528120000", envelope.Metadata.PlayDatetime);
        Assert.Equal([104u], envelope.Profile.ReleaseSongNoes);
        Assert.Equal([4u], envelope.Profile.GetToneNoes);
        Assert.Equal([1u], envelope.Profile.GetCostumeNo1s);
        Assert.Equal([2u], envelope.Profile.GetCostumeNo2s);
        Assert.Equal([3u], envelope.Profile.GetCostumeNo3s);
        Assert.Equal([4u], envelope.Profile.GetCostumeNo4s);
        Assert.Equal([5u], envelope.Profile.GetCostumeNo5s);
        Assert.Equal([10u], envelope.Profile.GetTitleNoes);
        Assert.Equal(1u, envelope.Profile.ItemshopTutorialFlg);
        Assert.True(envelope.Profile.IsDevil);
        Assert.True(envelope.Profile.IsExplain);
        Assert.True(envelope.Profile.HasDifficultyPlayedCourse);
        Assert.True(envelope.Profile.HasDifficultyPlayedStar);
        Assert.True(envelope.Profile.HasAryCurrentCostume);
        Assert.Equal(11u, envelope.Profile.AryCurrentCostume.Costume1);
        var stage = Assert.Single(envelope.Normal!.Stages);
        Assert.Equal(101u, stage.SongNo);
        Assert.Equal(0u, stage.StageMode);
        Assert.True(stage.IsPushed);
    }

    [Fact]
    public void Map_TokkunStageInfo_ClassifiesTokkunAndPreservesRawFacts()
    {
        var request = CreateRequest();
        request.PlayMode = TokkunPlayMode;
        request.TokkunTutorialFlg = 1;
        request.AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
        {
            BanacoinDatetime = "20260528120000",
            TokkunSongCnt = 3,
            TookunSongnoes = [101, 102, 103],
            TokkunSpeedchangeCnt = 4,
            TokkunAutoplayCnt = 5,
            TokkunJumpCnt = 6
        };

        var envelope = PlayResultMappers.Map(request);

        Assert.Equal(TokkunPlayMode, envelope.Metadata.PlayMode);
        Assert.NotNull(envelope.Tokkun);
        Assert.Equal(1u, envelope.Tokkun!.TutorialFlg);
        Assert.NotNull(envelope.Tokkun.StageData);
        Assert.Equal("20260528120000", envelope.Tokkun.StageData!.BanacoinDatetime);
        Assert.Equal(3u, envelope.Tokkun.StageData.TokkunSongCnt);
        Assert.Equal([101u, 102u, 103u], envelope.Tokkun.StageData.TookunSongnoes);
        Assert.Equal(4u, envelope.Tokkun.StageData.TokkunSpeedchangeCnt);
        Assert.Equal(5u, envelope.Tokkun.StageData.TokkunAutoplayCnt);
        Assert.Equal(6u, envelope.Tokkun.StageData.TokkunJumpCnt);
    }

    [Fact]
    public void Map_TokkunPlayModeWithoutStageInfo_ClassifiesTokkun()
    {
        var request = CreateRequest();
        request.PlayMode = TokkunPlayMode;

        var envelope = PlayResultMappers.Map(request);

        Assert.Equal(TokkunPlayMode, envelope.Metadata.PlayMode);
        Assert.Null(envelope.Tokkun);
    }

    [Fact]
    public void Map_TokkunStageInfoWithNonTokkunMode_PreservesRawFactsButDoesNotClassifyTokkun()
    {
        var request = CreateRequest();
        request.PlayMode = 0;
        request.AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
        {
            BanacoinDatetime = "20260528120000",
            TokkunSongCnt = 3,
            TookunSongnoes = [101, 102, 101],
            TokkunSpeedchangeCnt = 4,
            TokkunAutoplayCnt = 5,
            TokkunJumpCnt = 6
        };

        var envelope = PlayResultMappers.Map(request);

        Assert.Equal(0u, envelope.Metadata.PlayMode);
        Assert.NotNull(envelope.Tokkun);
        Assert.Equal("20260528120000", envelope.Tokkun!.StageData!.BanacoinDatetime);
        Assert.Equal(3u, envelope.Tokkun.StageData.TokkunSongCnt);
        Assert.Equal([101u, 102u, 101u], envelope.Tokkun.StageData.TookunSongnoes);
        Assert.Equal(4u, envelope.Tokkun.StageData.TokkunSpeedchangeCnt);
        Assert.Equal(5u, envelope.Tokkun.StageData.TokkunAutoplayCnt);
        Assert.Equal(6u, envelope.Tokkun.StageData.TokkunJumpCnt);
    }

    [Fact]
    public void Map_TutorialOnly_PreservesTutorialButDoesNotClassifyTokkun()
    {
        var request = CreateRequest();
        request.TokkunTutorialFlg = 1;

        var envelope = PlayResultMappers.Map(request);

        Assert.NotNull(envelope.Tokkun);
        Assert.Equal(1u, envelope.Tokkun!.TutorialFlg);
        Assert.Null(envelope.Tokkun.StageData);
    }

    [Fact]
    public void Map_MixedTokkunAndBattleSections_PreservesBothClassifierStates()
    {
        var request = CreateRequest();
        request.PlayMode = TokkunPlayMode;
        request.AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
        {
            BanacoinDatetime = "20260528120000",
            TokkunSongCnt = 1,
            TookunSongnoes = [101],
            TokkunSpeedchangeCnt = 0,
            TokkunAutoplayCnt = 0,
            TokkunJumpCnt = 0
        };
        request.AryReleaseBattledata = new PlayResultRequest.ReleaseBattleData
        {
            AssignNextStageId = 2
        };
        request.AryStageInfoes.Add(CreateStage(101, 1, 0, includeBattle: true));

        var envelope = PlayResultMappers.Map(request);

        Assert.NotNull(envelope.Tokkun);
        Assert.NotNull(envelope.BlueBattle);
        Assert.Equal([101u], envelope.Tokkun!.StageData!.TookunSongnoes);
        Assert.Single(envelope.BlueBattle!.Stages);
        Assert.Equal(101u, envelope.BlueBattle.Stages[0].SongNo);
    }

    [Fact]
    public void Map_BluePlayResultResponse_ReturnsResult()
    {
        var response = PlayResultMappers.Map(1);

        Assert.Equal(1u, response.Result);
    }

    private static PlayResultRequest CreateRequest() => new()
    {
        Baid = 1,
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
        PlayMode = 0,
        AreaCode = 1,
        Reserved = new byte[16]
    };

    private static PlayResultRequest.StageData CreateStage(
        uint songNo,
        uint level,
        uint stageMode,
        bool includeBattle = false)
    {
        var stage = new PlayResultRequest.StageData
        {
            SongNo = songNo,
            Level = level,
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
            StageMode = stageMode,
            SelectedFolderId = 9,
            SoulGauge = 100,
            WaiwaiResult = 0,
            WaiwaiGauge = 0
        };

        if (includeBattle)
        {
            stage.AryBattlestagedata = new PlayResultRequest.StageData.BattleStageData
            {
                SupportLv = 1,
                BattleStageId = 2,
                NpcData = new PlayResultRequest.StageData.BattleStageData.BattleNpcData
                {
                    NpcId = 1,
                    AcquiredExp = "0",
                    TotalExp = "0",
                    Dpn = 0,
                    NpcCostumeId = 0,
                    SpecialId1 = 0,
                    SpecialId2 = 0,
                    SpecialId3 = 0,
                    BondsLv = 0
                },
                KillCnt = 0,
                BossLife = 0,
                TotalDamage = 0,
                CriticalCnt = 0,
                SpecialMoveCnt = 0
            };
        }

        return stage;
    }
}
