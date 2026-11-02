using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15DanHelpersTests
{
    private static readonly Ac15ProtocolLimits Limits = Ac15EraProfiles.Blue.Limits;

    [Theory]
    [InlineData(1u, true, false, 0u)]
    [InlineData(25u, true, false, 24u)]
    [InlineData(101u, false, true, 0u)]
    [InlineData(128u, false, true, 27u)]
    public void ClassifiesNormalAndExtraRanges(uint danId, bool isNormal, bool isExtra, uint packedIndex)
    {
        Assert.Equal(isNormal, Ac15DanHelpers.IsNormalDanId(danId, Limits));
        Assert.Equal(isExtra, Ac15DanHelpers.IsExtraDanId(danId, Limits));
        Assert.True(Ac15DanHelpers.IsKnownDanId(danId, Limits));
        Assert.Equal(packedIndex, Ac15DanHelpers.GetPackedIndex(danId, Limits));
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(26u)]
    [InlineData(100u)]
    [InlineData(129u)]
    public void RejectsInvalidRanges(uint danId)
    {
        Assert.False(Ac15DanHelpers.IsKnownDanId(danId, Limits));
        Assert.Throws<ArgumentOutOfRangeException>(() => Ac15DanHelpers.GetPackedIndex(danId, Limits));
    }

    [Fact]
    public void WritesAndReadsTwoBitPackedGrades()
    {
        var flags = new byte[Limits.DanFlagBytes];

        flags = Ac15DanHelpers.SetPackedGrade(flags, 0, Ac15DanClearGrade.NormalClear, Limits.DanFlagBytes);
        flags = Ac15DanHelpers.SetPackedGrade(flags, 1, Ac15DanClearGrade.GoldClear, Limits.DanFlagBytes);
        flags = Ac15DanHelpers.SetPackedGrade(flags, 2, Ac15DanHelpers.ClampGrade(9), Limits.DanFlagBytes);

        Assert.Equal(Ac15DanClearGrade.NormalClear, Ac15DanHelpers.GetPackedGrade(flags, 0));
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(flags, 1));
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(flags, 2));
        Assert.Equal(Ac15DanClearGrade.NotClear, Ac15DanHelpers.GetPackedGrade(flags, 3));
    }

    [Fact]
    public void DerivesMaxClearAndDisplayFallback()
    {
        var grades = new Dictionary<uint, Ac15DanClearGrade>
        {
            [1] = Ac15DanClearGrade.GoldClear,
            [2] = Ac15DanClearGrade.NotClear,
            [3] = Ac15DanClearGrade.NormalClear,
            [101] = Ac15DanClearGrade.GoldClear
        };

        Assert.Equal(3u, Ac15DanHelpers.GetGotDanMax(grades, Limits));
        Assert.Equal(2u, Ac15DanHelpers.GetNextUnclearedNormalDan(grades, Limits));
        Assert.Equal(2u, Ac15DanHelpers.GetDisplayDanAfterNormalClear(1, Limits));
        Assert.Equal(2u, Ac15DanHelpers.NormalizeDisplayDan(1, grades, Limits));
        Assert.Equal(2u, Ac15DanHelpers.NormalizeDisplayDan(0, grades, Limits));
        Assert.Equal(2u, Ac15DanHelpers.NormalizeDisplayDan(2, grades, Limits));
    }

    [Fact]
    public void DisplayDanCapsAtFinalNormalDan()
    {
        var allCleared = Enumerable.Range((int)Limits.MinNormalDanId, (int)Limits.MaxNormalDanId)
            .ToDictionary(dan => (uint)dan, _ => Ac15DanClearGrade.GoldClear);

        Assert.Equal(25u, Ac15DanHelpers.GetDisplayDanAfterNormalClear(25, Limits));
        Assert.Equal(25u, Ac15DanHelpers.GetNextUnclearedNormalDan(allCleared, Limits));
        Assert.Equal(25u, Ac15DanHelpers.NormalizeDisplayDan(25, allCleared, Limits));
    }
}
