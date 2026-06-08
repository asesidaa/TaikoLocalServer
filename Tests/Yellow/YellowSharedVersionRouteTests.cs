using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
using TaikoLocalServer.Application;
using AppMovieData = TaikoLocalServer.Application.ServerData.MovieData;
using TaikoLocalServer.Tests.Blue;
using SharedStartupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthRequest;
using SharedStartupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthResponse;
using SharedVerupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupAuthRequest;
using SharedVerupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupAuthResponse;
using SharedVerupCompleteRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupCompleteRequest;
using SharedVerupCompleteResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupCompleteResponse;
using YellowStartupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire.StartupAuthRequest;
using YellowStartupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire.StartupAuthResponse;
using YellowVerupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire.VerupAuthRequest;
using YellowVerupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire.VerupAuthResponse;
using YellowVerupCompleteRequest = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire.VerupCompleteRequest;
using YellowVerupCompleteResponse = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire.VerupCompleteResponse;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowSharedVersionRouteTests
{
    [Theory]
    [InlineData("/v01r00/chassis/startupauth.php")]
    [InlineData("/v01r00/chassis/verupauth.php")]
    [InlineData("/v01r00/chassis/verupcomplete.php")]
    public void VersionRoutes_AreOwnedBySharedProtocolAdapter(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(
                typeof(BaseProtocolController<>).Assembly,
                typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        var route = Assert.Single(routes);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Shared", route.AssemblyName);
    }

    [Theory]
    [InlineData("/v09r02/chassis/startupauth.php")]
    [InlineData("/v09r02/chassis/verupauth.php")]
    [InlineData("/v09r02/chassis/verupcomplete.php")]
    public void YellowAdapter_DoesNotOwnVersionRoutesUnderGamePrefix(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        Assert.Empty(routes);
    }

    [Fact]
    public void SharedStartupAuthRequest_ReadsYellowVsInterfacePayload()
    {
        var request = new YellowStartupAuthRequest
        {
            ChassisId = "chassis",
            UsbmemKey = "usb",
            HddVer = 123,
            UsbmemVer = 456,
            ShopId = "shop",
            RackId = "rack",
            CountryId = "JPN"
        };
        request.AryOperationInfoes.Add(new YellowStartupAuthRequest.OperationData
        {
            KeyData = 7,
            ValueData = [1, 2, 3]
        });

        var shared = Deserialize<SharedStartupAuthRequest>(Serialize(request));

        Assert.Equal("chassis", shared.ChassisId);
        Assert.Equal("usb", shared.UsbmemKey);
        Assert.Equal(123u, shared.HddVer);
        Assert.Equal(456u, shared.UsbmemVer);
        Assert.Equal("shop", shared.ShopId);
        Assert.Equal("rack", shared.RackId);
        Assert.Equal("JPN", shared.CountryId);
        var operation = Assert.Single(shared.AryOperationInfoes);
        Assert.Equal(7u, operation.KeyData);
        Assert.Equal([1, 2, 3], operation.ValueData);
    }

    [Fact]
    public void SharedStartupAuthResponse_WritesYellowVsInterfacePayload()
    {
        var response = new SharedStartupAuthResponse { Result = 1 };
        response.AryOperationInfoes.Add(new SharedStartupAuthResponse.OperationData
        {
            KeyData = 9,
            ValueData = [4, 5, 6]
        });
        response.AryMovieInfoes.Add(new SharedStartupAuthResponse.MovieData
        {
            MovieId = 100,
            EnableDays = 999
        });

        var yellow = Deserialize<YellowStartupAuthResponse>(Serialize(response));

        Assert.Equal(1u, yellow.Result);
        Assert.Equal(9u, Assert.Single(yellow.AryOperationInfoes).KeyData);
        Assert.Equal([4, 5, 6], Assert.Single(yellow.AryOperationInfoes).ValueData);
        Assert.Equal(100u, Assert.Single(yellow.AryMovieInfoes).MovieId);
        Assert.Equal(999u, Assert.Single(yellow.AryMovieInfoes).EnableDays);
    }

    [Fact]
    public async Task StartupAuthController_UsesYellowHddVersionToPopulateMovieInfo()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddApplication();
        services.Configure<ServerSettings>(settings =>
        {
            settings.Eras = new Dictionary<string, EraSettings>
            {
                [nameof(GameEra.Yellow)] = new() { Enabled = true }
            };
        });
        services.AddSingleton<IGameDataCatalog>(new FileGameDataCatalog(
        [
            new YellowHandlerFixture.TestYellowCatalog
            {
                Movies =
                [
                    new AppMovieData { MovieId = 909, EnableDays = 777 }
                ]
            }
        ]));

        using var provider = services.BuildServiceProvider();
        var controller = new StartupAuthController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = provider
                }
            }
        };

        var result = await controller.StartupAuth(new SharedStartupAuthRequest
        {
            ChassisId = "chassis",
            HddVer = 913,
            ShopId = "shop"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<SharedStartupAuthResponse>(ok.Value);
        var movie = Assert.Single(response.AryMovieInfoes);
        Assert.Equal(909u, movie.MovieId);
        Assert.Equal(777u, movie.EnableDays);
    }

    [Fact]
    public void SharedVerupAuthRequest_ReadsYellowVsInterfacePayload()
    {
        var request = new YellowVerupAuthRequest
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
    public void SharedVerupAuthResponse_WritesYellowVsInterfacePayload()
    {
        var yellow = Deserialize<YellowVerupAuthResponse>(
            Serialize(new SharedVerupAuthResponse { Result = 1 }));

        Assert.Equal(1u, yellow.Result);
    }

    [Fact]
    public void SharedVerupCompleteRequest_ReadsYellowVsInterfacePayload()
    {
        var request = new YellowVerupCompleteRequest
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
    public void SharedVerupCompleteResponse_WritesYellowVsInterfacePayload()
    {
        var yellow = Deserialize<YellowVerupCompleteResponse>(
            Serialize(new SharedVerupCompleteResponse { Result = 1 }));

        Assert.Equal(1u, yellow.Result);
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
