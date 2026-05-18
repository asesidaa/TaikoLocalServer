namespace TaikoLocalServer.Tests.Green;

public sealed class GreenAiBattleLevelsTests
{
    [Theory]
    [InlineData(1u, 0u, true)]
    [InlineData(2u, 0u, true)]
    [InlineData(3u, 0u, true)]
    [InlineData(4u, 0u, true)]
    [InlineData(1u, 1u, false)]
    [InlineData(2u, 1u, false)]
    [InlineData(3u, 2u, false)]
    [InlineData(4u, 9u, false)]
    [InlineData(5u, 0u, true)]
    [InlineData(5u, 1u, true)]
    [InlineData(5u, 9u, true)]
    public void AllowsCrown_ReturnsTrueForUsualLevelsAndAllUra(uint courseLevel, uint supportLevel, bool expected)
    {
        Assert.Equal(expected, GreenAiBattleLevels.AllowsCrown(courseLevel, supportLevel));
    }
}
