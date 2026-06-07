using YellowWire = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowWireGenerationTests
{
    [Fact]
    public void YellowGameWireTypes_LiveUnderYellowAdapterNamespace()
    {
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire", typeof(YellowWire.InitialdatacheckRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire", typeof(YellowWire.PlayResultRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire", typeof(YellowWire.UserDataRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire", typeof(YellowWire.GetbanacoininfoRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire", typeof(YellowWire.GetbanacoininfoResponse).Namespace);
    }

    [Fact]
    public void YellowPlayResultRequest_IsDirectRequestShape()
    {
        Assert.NotNull(typeof(YellowWire.PlayResultRequest).GetProperty(nameof(YellowWire.PlayResultRequest.Baid)));
        Assert.NotNull(typeof(YellowWire.PlayResultRequest).GetProperty(nameof(YellowWire.PlayResultRequest.AryStageInfoes)));
        Assert.Null(typeof(YellowWire.PlayResultRequest).GetProperty("PlayresultData"));
    }

    [Theory]
    [InlineData("BattleUserDataRequest")]
    [InlineData("BattleUserDataResponse")]
    public void YellowWire_DoesNotContainBlueBattleTypes(string typeName)
    {
        var type = typeof(YellowWire.PlayResultRequest).Assembly.GetType(
            $"TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire.{typeName}",
            throwOnError: false);

        Assert.Null(type);
    }

    [Theory]
    [InlineData("IsBattleplay")]
    [InlineData("ReleaseBattleStageFlg")]
    [InlineData("ReleaseBattleSpecialFlg")]
    [InlineData("BattleBondsLvCap")]
    public void YellowInitialData_DoesNotContainBlueBattleFields(string propertyName)
    {
        Assert.Null(typeof(YellowWire.InitialdatacheckResponse).GetProperty(propertyName));
    }

    [Theory]
    [InlineData("AryReleaseBattledata")]
    public void YellowPlayResult_DoesNotContainBlueBattleFields(string propertyName)
    {
        Assert.Null(typeof(YellowWire.PlayResultRequest).GetProperty(propertyName));
    }
}
