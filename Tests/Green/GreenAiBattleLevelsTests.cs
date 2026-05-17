namespace TaikoLocalServer.Tests.Green;

public sealed class GreenAiBattleLevelsTests
{
    [Theory]
    [InlineData(0u, false)]
    [InlineData(1u, true)]
    [InlineData(2u, false)]
    [InlineData(3u, false)]
    [InlineData(4u, false)]
    [InlineData(5u, true)]
    [InlineData(6u, false)]
    [InlineData(7u, false)]
    [InlineData(8u, false)]
    [InlineData(9u, true)]
    [InlineData(10u, false)]
    [InlineData(11u, false)]
    [InlineData(12u, false)]
    [InlineData(13u, true)]
    [InlineData(14u, false)]
    [InlineData(100u, false)]
    public void IsCertifiedLevel_ReturnsTrueForCanonicalLevels(uint sdCertifiedLevelId, bool expected)
    {
        Assert.Equal(expected, GreenAiBattleLevels.IsCertifiedLevel(sdCertifiedLevelId));
    }
}
