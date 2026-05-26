using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
using GreenVerupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.VerupAuthRequest;
using GreenVerupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.VerupAuthResponse;
using GreenVerupCompleteRequest = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.VerupCompleteRequest;
using GreenVerupCompleteResponse = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.VerupCompleteResponse;
using SharedVerupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupAuthRequest;
using SharedVerupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupAuthResponse;
using SharedVerupCompleteRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupCompleteRequest;
using SharedVerupCompleteResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupCompleteResponse;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueSharedVersionRouteTests
{
    [Theory]
    [InlineData("/v01r00/chassis/startupauth.php")]
    [InlineData("/v01r00/chassis/verupauth.php")]
    [InlineData("/v01r00/chassis/verupcomplete.php")]
    public void VersionRoutes_AreOwnedBySharedProtocolAdapter(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(BaseProtocolController<>).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        var route = Assert.Single(routes);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Shared", route.AssemblyName);
    }

    [Theory]
    [InlineData("/v10r03/chassis/startupauth.php")]
    [InlineData("/v10r03/chassis/verupauth.php")]
    [InlineData("/v10r03/chassis/verupcomplete.php")]
    public void BlueAdapter_DoesNotOwnVersionRoutesUnderGamePrefix(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        Assert.Empty(routes);
    }

    [Fact]
    public void SharedVerupAuthRequest_ReadsGreenVsInterfacePayload()
    {
        var request = new GreenVerupAuthRequest
        {
            ChassisId = "chassis",
            UsbmemKey = "usb",
            HddVer = 123,
            UsbmemVer = 456,
            ShopId = "shop",
            RackId = "rack",
            CountryId = "JPN"
        };

        var shared = Deserialize<SharedVerupAuthRequest>(Serialize(request));

        Assert.Equal("chassis", shared.ChassisId);
        Assert.Equal("usb", shared.UsbmemKey);
        Assert.Equal(123u, shared.HddVer);
        Assert.Equal(456u, shared.UsbmemVer);
        Assert.Equal("shop", shared.ShopId);
        Assert.Equal("rack", shared.RackId);
        Assert.Equal("JPN", shared.CountryId);
    }

    [Fact]
    public void SharedVerupAuthResponse_WritesVsInterfacePayload()
    {
        var green = Deserialize<GreenVerupAuthResponse>(
            Serialize(new SharedVerupAuthResponse { Result = 1 }));

        Assert.Equal(1u, green.Result);
    }

    [Fact]
    public void SharedVerupCompleteRequest_ReadsGreenVsInterfacePayload()
    {
        var request = new GreenVerupCompleteRequest
        {
            ChassisId = "chassis",
            UsbmemKey = "usb",
            HddVer = 123,
            UsbmemVer = 456,
            ShopId = "shop",
            RackId = "rack",
            CountryId = "JPN"
        };

        var shared = Deserialize<SharedVerupCompleteRequest>(Serialize(request));

        Assert.Equal("chassis", shared.ChassisId);
        Assert.Equal("usb", shared.UsbmemKey);
        Assert.Equal(123u, shared.HddVer);
        Assert.Equal(456u, shared.UsbmemVer);
        Assert.Equal("shop", shared.ShopId);
        Assert.Equal("rack", shared.RackId);
        Assert.Equal("JPN", shared.CountryId);
    }

    [Fact]
    public void SharedVerupCompleteResponse_WritesVsInterfacePayload()
    {
        var green = Deserialize<GreenVerupCompleteResponse>(
            Serialize(new SharedVerupCompleteResponse { Result = 1 }));

        Assert.Equal(1u, green.Result);
    }

    private static byte[] Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        ProtoBuf.Serializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static T Deserialize<T>(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return ProtoBuf.Serializer.Deserialize<T>(stream);
    }
}
