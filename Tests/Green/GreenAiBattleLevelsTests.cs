namespace TaikoLocalServer.Tests.Green;

public sealed class GreenAiBattleLevelsTests
{
    [Theory]
    [InlineData(Difficulty.Easy, 0u, true)]
    [InlineData(Difficulty.Normal, 0u, true)]
    [InlineData(Difficulty.Hard, 0u, true)]
    [InlineData(Difficulty.Oni, 0u, true)]
    [InlineData(Difficulty.Easy, 1u, false)]
    [InlineData(Difficulty.Normal, 1u, false)]
    [InlineData(Difficulty.Hard, 2u, false)]
    [InlineData(Difficulty.Oni, 9u, false)]
    [InlineData(Difficulty.UraOni, 0u, true)]
    [InlineData(Difficulty.UraOni, 1u, true)]
    [InlineData(Difficulty.UraOni, 9u, true)]
    public void AllowsCrown_ReturnsTrueForUsualLevelsAndAllUra(Difficulty courseLevel, uint supportLevel, bool expected)
    {
        Assert.Equal(expected, GreenAiBattleLevels.AllowsCrown(courseLevel, supportLevel));
    }
}
