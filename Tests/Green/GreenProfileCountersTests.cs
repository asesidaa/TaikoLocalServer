namespace TaikoLocalServer.Tests.Green;

public sealed class GreenProfileCountersTests
{
    [Theory]
    [InlineData(1u, nameof(UserSaveDataGreen.CategJpopCnt))]
    [InlineData(2u, nameof(UserSaveDataGreen.CategAnimeCnt))]
    [InlineData(3u, nameof(UserSaveDataGreen.CategVocaloidCnt))]
    [InlineData(4u, nameof(UserSaveDataGreen.CategDoyoCnt))]
    [InlineData(5u, nameof(UserSaveDataGreen.CategVarietyCnt))]
    [InlineData(6u, nameof(UserSaveDataGreen.CategClassicCnt))]
    [InlineData(7u, nameof(UserSaveDataGreen.CategGameCnt))]
    [InlineData(8u, nameof(UserSaveDataGreen.CategNamcoCnt))]
    public void ApplyStage_IncrementsMatchingGenreCounter(uint musicCateg, string expectedProperty)
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = musicCateg };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(1u, CounterValue(save, expectedProperty));
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(9u)]
    [InlineData(99u)]
    [InlineData(uint.MaxValue)]
    public void ApplyStage_GenreWithoutProfileCounterDoesNotMoveAnyCounter(uint musicCateg)
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = musicCateg };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(0u, save.CategJpopCnt);
        Assert.Equal(0u, save.CategAnimeCnt);
        Assert.Equal(0u, save.CategDoyoCnt);
        Assert.Equal(0u, save.CategVocaloidCnt);
        Assert.Equal(0u, save.CategGameCnt);
        Assert.Equal(0u, save.CategNamcoCnt);
        Assert.Equal(0u, save.CategVarietyCnt);
        Assert.Equal(0u, save.CategClassicCnt);
    }

    [Fact]
    public void ApplyStage_IsPushedTrue_IncrementsSongPushedCnt()
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = 0, IsPushed = true };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(1u, save.SongPushedCnt);
    }

    [Fact]
    public void ApplyStage_IsFavoriteTrue_IncrementsSongFavoriteCnt()
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = 0, IsFavorite = true };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(1u, save.SongFavoriteCnt);
    }

    [Fact]
    public void ApplyStage_IsRecentTrue_IncrementsSongRecentCnt()
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = 0, IsRecent = true };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(1u, save.SongRecentCnt);
    }

    [Fact]
    public void ApplyStage_AllFlagsFalse_LeavesSongCountersAtZero()
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = 0 };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(0u, save.SongPushedCnt);
        Assert.Equal(0u, save.SongFavoriteCnt);
        Assert.Equal(0u, save.SongRecentCnt);
    }

    [Fact]
    public void ApplyStage_SaturatesAtUintMaxValue()
    {
        var save = new UserSaveDataGreen
        {
            Baid = 1,
            CategJpopCnt = uint.MaxValue,
            SongPushedCnt = uint.MaxValue,
            SongFavoriteCnt = uint.MaxValue,
            SongRecentCnt = uint.MaxValue
        };
        var stage = new CommonPlayResultData.StageData
        {
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true
        };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(uint.MaxValue, save.CategJpopCnt);
        Assert.Equal(uint.MaxValue, save.SongPushedCnt);
        Assert.Equal(uint.MaxValue, save.SongFavoriteCnt);
        Assert.Equal(uint.MaxValue, save.SongRecentCnt);
    }

    private static uint CounterValue(UserSaveDataGreen save, string propertyName)
        => propertyName switch
        {
            nameof(UserSaveDataGreen.CategJpopCnt) => save.CategJpopCnt,
            nameof(UserSaveDataGreen.CategAnimeCnt) => save.CategAnimeCnt,
            nameof(UserSaveDataGreen.CategVocaloidCnt) => save.CategVocaloidCnt,
            nameof(UserSaveDataGreen.CategDoyoCnt) => save.CategDoyoCnt,
            nameof(UserSaveDataGreen.CategVarietyCnt) => save.CategVarietyCnt,
            nameof(UserSaveDataGreen.CategClassicCnt) => save.CategClassicCnt,
            nameof(UserSaveDataGreen.CategGameCnt) => save.CategGameCnt,
            nameof(UserSaveDataGreen.CategNamcoCnt) => save.CategNamcoCnt,
            _ => throw new ArgumentOutOfRangeException(nameof(propertyName), propertyName, "Unsupported counter.")
        };
}
