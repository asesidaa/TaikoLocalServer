using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowEraFoundationTests
{
    [Fact]
    public void GameEra_Yellow_HasStableNumericValue()
    {
        var parsed = Enum.Parse<GameEra>("Yellow", ignoreCase: true);

        Assert.Equal(3, (int)parsed);
    }

    [Fact]
    public void ExistingEraNumericValues_RemainStable()
    {
        Assert.Equal(0, (int)GameEra.Nijiiro);
        Assert.Equal(1, (int)GameEra.Green);
        Assert.Equal(2, (int)GameEra.Blue);
    }
}
