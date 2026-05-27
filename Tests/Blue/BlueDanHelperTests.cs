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

        flags = BlueDanHelpers.SetPackedGrade(flags, 0, BlueDanClearGrade.NormalClear);
        flags = BlueDanHelpers.SetPackedGrade(flags, 1, BlueDanClearGrade.GoldClear);

        Assert.Equal(BlueDanClearGrade.NormalClear, BlueDanHelpers.GetPackedGrade(flags, 0));
        Assert.Equal(BlueDanClearGrade.GoldClear, BlueDanHelpers.GetPackedGrade(flags, 1));
        Assert.Equal(BlueDanClearGrade.NotClear, BlueDanHelpers.GetPackedGrade(flags, 2));
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
        var grades = new Dictionary<uint, BlueDanClearGrade>
        {
            [1] = BlueDanClearGrade.NormalClear,
            [2] = BlueDanClearGrade.NotClear,
            [5] = BlueDanClearGrade.GoldClear,
            [101] = BlueDanClearGrade.GoldClear
        };

        Assert.Equal(5u, BlueDanHelpers.GetGotDanMax(grades));
    }

    [Fact]
    public void BlueDanHelpers_NormalizesInvalidOrClearedDisplayDan()
    {
        var grades = new Dictionary<uint, BlueDanClearGrade>
        {
            [1] = BlueDanClearGrade.NormalClear,
            [2] = BlueDanClearGrade.GoldClear,
            [3] = BlueDanClearGrade.NotClear
        };

        Assert.Equal(3u, BlueDanHelpers.NormalizeDisplayDan(0, grades));
        Assert.Equal(3u, BlueDanHelpers.NormalizeDisplayDan(1, grades));
        Assert.Equal(4u, BlueDanHelpers.NormalizeDisplayDan(4, grades));
        Assert.Equal(3u, BlueDanHelpers.GetNextUnclearedNormalDan(grades));
        Assert.Equal(2u, BlueDanHelpers.GetDisplayDanAfterNormalClear(1));
    }
}
