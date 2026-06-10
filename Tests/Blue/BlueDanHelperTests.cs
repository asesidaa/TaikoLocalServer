namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueDanHelperTests
{
    [Theory]
    [InlineData(1u, true, false)]
    [InlineData(25u, true, false)]
    [InlineData(101u, false, true)]
    [InlineData(128u, false, true)]
    [InlineData(0u, false, false)]
    [InlineData(129u, false, false)]
    public void BlueDanHelpers_ClassifiesKnownDanIds(uint danId, bool normal, bool extra)
    {
        Assert.Equal(normal, BlueDanHelpers.IsNormalDanId(danId));
        Assert.Equal(extra, BlueDanHelpers.IsExtraDanId(danId));
        Assert.Equal(normal || extra, BlueDanHelpers.IsKnownBlueDanId(danId));
    }

    [Fact]
    public void BlueDanHelpers_PacksNormalAndGoldClearGrades()
    {
        var flags = new byte[BlueProtocolBytes.DanFlagBytes];

        flags = BlueDanHelpers.SetPackedGrade(flags, 0, Ac15DanClearGrade.NormalClear);
        flags = BlueDanHelpers.SetPackedGrade(flags, 1, Ac15DanClearGrade.GoldClear);

        Assert.Equal(Ac15DanClearGrade.NormalClear, BlueDanHelpers.GetPackedGrade(flags, 0));
        Assert.Equal(Ac15DanClearGrade.GoldClear, BlueDanHelpers.GetPackedGrade(flags, 1));
        Assert.Equal(Ac15DanClearGrade.NotClear, BlueDanHelpers.GetPackedGrade(flags, 2));
    }

    [Fact]
    public void BlueDanHelpers_MapsNormalAndExtraPackedIndexes()
    {
        Assert.Equal(0u, BlueDanHelpers.GetPackedIndex(1));
        Assert.Equal(24u, BlueDanHelpers.GetPackedIndex(25));
        Assert.Equal(0u, BlueDanHelpers.GetPackedIndex(101));
        Assert.Equal(27u, BlueDanHelpers.GetPackedIndex(128));
    }

    [Fact]
    public void BlueDanHelpers_ComputesGotDanMaxFromClearedNormalDans()
    {
        var grades = new Dictionary<uint, Ac15DanClearGrade>
        {
            [1] = Ac15DanClearGrade.NormalClear,
            [2] = Ac15DanClearGrade.NotClear,
            [5] = Ac15DanClearGrade.GoldClear,
            [101] = Ac15DanClearGrade.GoldClear
        };

        Assert.Equal(5u, BlueDanHelpers.GetGotDanMax(grades));
    }

    [Fact]
    public void BlueDanHelpers_NormalizesInvalidOrClearedDisplayDan()
    {
        var grades = new Dictionary<uint, Ac15DanClearGrade>
        {
            [1] = Ac15DanClearGrade.NormalClear,
            [2] = Ac15DanClearGrade.GoldClear,
            [3] = Ac15DanClearGrade.NotClear
        };

        Assert.Equal(3u, BlueDanHelpers.NormalizeDisplayDan(0, grades));
        Assert.Equal(3u, BlueDanHelpers.NormalizeDisplayDan(1, grades));
        Assert.Equal(4u, BlueDanHelpers.NormalizeDisplayDan(4, grades));
        Assert.Equal(3u, BlueDanHelpers.GetNextUnclearedNormalDan(grades));
        Assert.Equal(2u, BlueDanHelpers.GetDisplayDanAfterNormalClear(1));
    }
}
