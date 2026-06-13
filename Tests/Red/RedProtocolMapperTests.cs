using TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Red.Wire;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedProtocolMapperTests
{
    [Fact]
    public void PlayResultMapper_Red_MapsDonPointTokkunAndChallengeFacts()
    {
        var request = CreateWireRequest(1);
        request.GetDonpoint = 25;
        request.RewardPtn = 4;
        request.RewardProgress = 9;
        request.DifficultyTutorialFlg = 2;
        request.DifficultyPlayedCourse = 3;
        request.DifficultyPlayedStar = 8;
        request.TokkunTutorialFlg = 7;
        request.AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
        {
            BanacoinDatetime = "20260608120100",
            TokkunSongCnt = 3,
            TookunSongnoes = [101, 102, 101],
            TokkunSpeedchangeCnt = 3,
            TokkunAutoplayCnt = 4,
            TokkunJumpCnt = 5
        };
        var stage = new PlayResultRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 2,
            PlayScore = 765432,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            OptionFlg = [1],
            ToneFlg = [2],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            IsPapamama = false,
            PlayDan = 1,
            SoulGauge = 80,
            HitCnt = 123,
            StageMode = 0,
            SelectedFolderId = 9
        };
        stage.AryChallengeIds.Add(new PlayResultRequest.StageData.ResultcompeData { CompeId = 42, TrackNo = 2 });
        request.AryStageInfoes.Add(stage);

        var common = PlayResultMappers.Map(request);

        Assert.Equal(25u, common.GetDonpoint);
        Assert.Equal(4u, common.RewardPtn);
        Assert.Equal(9u, common.RewardProgress);
        Assert.Equal(2u, common.DifficultyTutorialFlg);
        Assert.Equal(3u, common.DifficultyPlayedCourse);
        Assert.Equal(8u, common.DifficultyPlayedStar);
        Assert.Equal(7u, common.TokkunTutorialFlg);
        Assert.NotNull(common.TokkunStageData);
        Assert.Equal([101u, 102u, 101u], common.TokkunStageData!.TookunSongnoes);
        var commonStage = Assert.Single(common.AryStageInfoes);
        Assert.Equal(42u, Assert.Single(commonStage.AryChallengeIds).CompeId);
        Assert.Equal(1u, commonStage.PlayDan);
        Assert.Equal(80u, commonStage.SoulGauge);
    }

    [Fact]
    public void UserDataMapper_Red_MapsDonPointFieldsToRedWire()
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            ReleaseSongFlg = [1, 2],
            ToneFlg = [3],
            TitleFlg = [4],
            AryFavoriteSongNoes = [101],
            AryRecentSongNoes = [102],
            TotalGetDonpoint = 120,
            TotalUseDonpoint = 30,
            RewardProgress = 8,
            DifficultyTutorialFlg = 2,
            TokkunTutorialFlg = 7,
            IsDevilRed = true,
            IsExplainRed = true,
            DispTaikojukuDan = 1,
            RecommendBestSong = [103]
        });

        Assert.Equal(120u, response.TotalGetDonpoint);
        Assert.Equal(30u, response.TotalUseDonpoint);
        Assert.Equal(8u, response.RewardProgress);
        Assert.Equal(2u, response.DifficultyTutorialFlg);
        Assert.Equal(7u, response.TokkunTutorialFlg);
        Assert.True(response.IsDevil);
        Assert.True(response.IsExplain);
        Assert.Equal([101u], response.AryFavoriteSongNoes);
        Assert.Equal([102u], response.AryRecentSongNoes);
        Assert.Equal([103u], response.RecommendBestSongs);
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
            GenderType = 0,
            PlayerAge = 0,
            PlayMode = (uint)PlayMode.Normal,
            AreaCode = 1,
            Reserved = new byte[16]
        };
}
