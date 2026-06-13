using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenPlayResultMapperTests
{
    [Fact]
    public void Map_GreenPlayResult_PreservesStageModeAndIsPapamama()
    {
        var request = CreateRequest();
        request.AryStageInfoes.Add(new PlayResultDataRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 1,
            PlayScore = 123456,
            GoodCnt = 10,
            OkCnt = 2,
            NgCnt = 1,
            PoundCnt = 3,
            ComboCnt = 12,
            OptionFlg = [0, 0],
            ToneFlg = new byte[16],
            MusicCateg = 1,
            IsFavorite = false,
            IsRecent = false,
            IsPapamama = true,
            StageMode = 1,
            SelectedFolderId = 0,
            StarLevel = 3,
            SupportLevel = 0
        });

        var envelope = PlayResultMappers.Map(request);

        var stage = Assert.Single(envelope.Normal!.Stages);
        Assert.Equal(1u, stage.StageMode);
        Assert.True(stage.IsPapamama);
    }

    [Fact]
    public void Map_GreenPlayResult_TreatsZeroPlayDanAsNoDan()
    {
        var request = CreateRequest();
        request.AryStageInfoes.Add(new PlayResultDataRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 1,
            PlayScore = 123456,
            GoodCnt = 10,
            OkCnt = 2,
            NgCnt = 1,
            PoundCnt = 3,
            ComboCnt = 12,
            OptionFlg = [0, 0],
            ToneFlg = new byte[16],
            MusicCateg = 1,
            IsFavorite = false,
            IsRecent = false,
            IsPapamama = false,
            PlayDan = 0,
            StageMode = 0,
            SelectedFolderId = 0,
            StarLevel = 3,
            SupportLevel = 0
        });

        var envelope = PlayResultMappers.Map(request);

        Assert.Null(Assert.Single(envelope.Normal!.Stages).PlayDan);
    }

    [Fact]
    public void Map_GreenPlayResult_PreservesNonZeroPlayDan()
    {
        var request = CreateRequest();
        request.AryStageInfoes.Add(new PlayResultDataRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 1,
            PlayScore = 123456,
            GoodCnt = 10,
            OkCnt = 2,
            NgCnt = 1,
            PoundCnt = 3,
            ComboCnt = 12,
            OptionFlg = [0, 0],
            ToneFlg = new byte[16],
            MusicCateg = 1,
            IsFavorite = false,
            IsRecent = false,
            IsPapamama = false,
            PlayDan = 7,
            StageMode = 0,
            SelectedFolderId = 0,
            StarLevel = 3,
            SupportLevel = 0
        });

        var envelope = PlayResultMappers.Map(request);

        Assert.Equal(7u, Assert.Single(envelope.Normal!.Stages).PlayDan);
    }

    [Fact]
    public void Map_GreenPlayResult_PreservesIsPushed()
    {
        var request = CreateRequest();
        request.AryStageInfoes.Add(new PlayResultDataRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 1,
            PlayScore = 123456,
            GoodCnt = 10,
            OkCnt = 2,
            NgCnt = 1,
            PoundCnt = 3,
            ComboCnt = 12,
            OptionFlg = [0, 0],
            ToneFlg = new byte[16],
            MusicCateg = 0,
            IsFavorite = false,
            IsRecent = false,
            IsPapamama = false,
            IsPushed = true,
            StageMode = 0,
            SelectedFolderId = 0,
            StarLevel = 3,
            SupportLevel = 0
        });

        var envelope = PlayResultMappers.Map(request);

        Assert.True(Assert.Single(envelope.Normal!.Stages).IsPushed);
    }

    private static PlayResultDataRequest CreateRequest() => new()
    {
        Baid = 1,
        ChassisId = "268410000000",
        ShopId = "JPN0JPN0123",
        PlayDatetime = "20260514032442",
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
}
