using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueWireGenerationTests
{
    [Fact]
    public void BlueWireTypes_ContainBlueOnlyA0Messages()
    {
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(CoinsettingRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BalancecheckRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BanacoinpaymentRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BanacoinerrorlogRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BattleUserDataRequest).Namespace);
    }

    [Fact]
    public void BluePlayResultRequest_IsDirectRequestShape()
    {
        Assert.NotNull(typeof(PlayResultRequest).GetProperty(nameof(PlayResultRequest.Baid)));
        Assert.NotNull(typeof(PlayResultRequest).GetProperty(nameof(PlayResultRequest.AryStageInfoes)));
        Assert.Null(typeof(PlayResultRequest).GetProperty("PlayresultData"));
    }
}
