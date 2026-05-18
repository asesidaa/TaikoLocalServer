namespace TaikoLocalServer.Tests.Green;

public sealed class GreenStageModeInterpreterTests
{
    [Theory]
    [InlineData(0u, false)]
    [InlineData(1u, true)]
    [InlineData(2u, false)]
    [InlineData(3u, false)]
    [InlineData(4u, true)]
    [InlineData(5u, false)]
    public void IsShin_ReturnsTrueForOneAndFour(uint stageMode, bool expected)
    {
        Assert.Equal(expected, GreenStageModeInterpreter.IsShin(stageMode));
    }

    [Theory]
    [InlineData(0u, false)]
    [InlineData(1u, false)]
    [InlineData(2u, false)]
    [InlineData(3u, true)]
    [InlineData(4u, true)]
    [InlineData(5u, false)]
    public void IsAiBattle_ReturnsTrueForThreeAndFour(uint stageMode, bool expected)
    {
        Assert.Equal(expected, GreenStageModeInterpreter.IsAiBattle(stageMode));
    }
}
