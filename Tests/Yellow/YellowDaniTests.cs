using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowDaniTests
{
    [Theory]
    [InlineData(1, true, false, 0)]
    [InlineData(25, true, false, 24)]
    [InlineData(101, false, true, 0)]
    [InlineData(128, false, true, 27)]
    public void YellowDanHelpers_RecognizesNormalAndExtraRanges(
        uint danId,
        bool isNormal,
        bool isExtra,
        uint packedIndex)
    {
        Assert.Equal(isNormal, YellowDanHelpers.IsNormalDanId(danId));
        Assert.Equal(isExtra, YellowDanHelpers.IsExtraDanId(danId));
        Assert.True(YellowDanHelpers.IsKnownYellowDanId(danId));
        Assert.Equal(packedIndex, YellowDanHelpers.GetPackedIndex(danId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(26)]
    [InlineData(100)]
    [InlineData(129)]
    public void YellowDanHelpers_RejectsInvalidRanges(uint danId)
    {
        Assert.False(YellowDanHelpers.IsKnownYellowDanId(danId));
        Assert.Throws<ArgumentOutOfRangeException>(() => YellowDanHelpers.GetPackedIndex(danId));
    }

    [Fact]
    public void YellowDanHelpers_WritesAndReadsTwoBitPackedGrades()
    {
        var flags = new byte[Ac15EraProfiles.Yellow.Limits.DanFlagBytes];

        flags = YellowDanHelpers.SetPackedGrade(flags, 0, YellowDanClearGrade.NormalClear);
        flags = YellowDanHelpers.SetPackedGrade(flags, 1, YellowDanClearGrade.GoldClear);
        flags = YellowDanHelpers.SetPackedGrade(flags, 2, YellowDanHelpers.ClampGrade(9));

        Assert.Equal(YellowDanClearGrade.NormalClear, YellowDanHelpers.GetPackedGrade(flags, 0));
        Assert.Equal(YellowDanClearGrade.GoldClear, YellowDanHelpers.GetPackedGrade(flags, 1));
        Assert.Equal(YellowDanClearGrade.GoldClear, YellowDanHelpers.GetPackedGrade(flags, 2));
        Assert.Equal(YellowDanClearGrade.NotClear, YellowDanHelpers.GetPackedGrade(flags, 3));
    }

    [Fact]
    public void YellowDanHelpers_DerivesMaxClearAndDisplayFallback()
    {
        var grades = new Dictionary<uint, YellowDanClearGrade>
        {
            [1] = YellowDanClearGrade.GoldClear,
            [2] = YellowDanClearGrade.NotClear,
            [3] = YellowDanClearGrade.NormalClear,
            [101] = YellowDanClearGrade.GoldClear
        };

        Assert.Equal(3u, YellowDanHelpers.GetGotDanMax(grades));
        Assert.Equal(2u, YellowDanHelpers.GetNextUnclearedNormalDan(grades));
        Assert.Equal(2u, YellowDanHelpers.GetDisplayDanAfterNormalClear(1));
        Assert.Equal(2u, YellowDanHelpers.NormalizeDisplayDan(1, grades));
        Assert.Equal(2u, YellowDanHelpers.NormalizeDisplayDan(0, grades));
        Assert.Equal(2u, YellowDanHelpers.NormalizeDisplayDan(2, grades));
    }

    [Fact]
    public void YellowDanHelpers_DisplayDanCapsAtFinalNormalDan()
    {
        var allCleared = Enumerable.Range((int)YellowDanHelpers.MinNormalDanId, (int)YellowDanHelpers.MaxNormalDanId)
            .ToDictionary(dan => (uint)dan, _ => YellowDanClearGrade.GoldClear);

        Assert.Equal(25u, YellowDanHelpers.GetDisplayDanAfterNormalClear(25));
        Assert.Equal(25u, YellowDanHelpers.GetNextUnclearedNormalDan(allCleared));
        Assert.Equal(25u, YellowDanHelpers.NormalizeDisplayDan(25, allCleared));
    }
}
