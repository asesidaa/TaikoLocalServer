namespace TaikoLocalServer.Tests.Green;

public sealed class GreenProfileCountersTests
{
    [Theory]
    [InlineData(0u, nameof(UserSaveDataGreen.CategJpopCnt))]
    [InlineData(1u, nameof(UserSaveDataGreen.CategAnimeCnt))]
    [InlineData(2u, nameof(UserSaveDataGreen.CategDoyoCnt))]
    [InlineData(3u, nameof(UserSaveDataGreen.CategVocaloidCnt))]
    [InlineData(4u, nameof(UserSaveDataGreen.CategGameCnt))]
    [InlineData(5u, nameof(UserSaveDataGreen.CategNamcoCnt))]
    [InlineData(6u, nameof(UserSaveDataGreen.CategVarietyCnt))]
    [InlineData(7u, nameof(UserSaveDataGreen.CategClassicCnt))]
    public void ApplyStage_IncrementsMatchingGenreCounter(uint musicCateg, string expectedProperty)
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = musicCateg };

        GreenProfileCounters.ApplyStage(save, stage);

        var property = typeof(UserSaveDataGreen).GetProperty(expectedProperty)!;
        Assert.Equal(1u, (uint)property.GetValue(save)!);
    }

    [Theory]
    [InlineData(8u)]
    [InlineData(99u)]
    [InlineData(uint.MaxValue)]
    public void ApplyStage_GenreOutOfRangeDoesNotMoveAnyCounter(uint musicCateg)
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
            MusicCateg = 0,
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
}
