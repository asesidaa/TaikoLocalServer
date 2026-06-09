using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
using TaikoLocalServer.Application;
using AppMovieData = TaikoLocalServer.Application.ServerData.MovieData;
using SharedStartupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthRequest;
using SharedStartupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthResponse;

namespace TaikoLocalServer.Tests.Green;

public sealed class StartupAuthControllerTests
{
    [Fact]
    public async Task StartupAuthController_UsesHddVersionToPopulateMovieInfo()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddApplication();
        services.Configure<ServerSettings>(settings =>
        {
            settings.Eras = new Dictionary<string, EraSettings>
            {
                [nameof(GameEra.Green)] = new() { Enabled = true }
            };
        });
        services.AddSingleton<IGameDataCatalog>(new FileGameDataCatalog(
        [
            new GreenHandlerFixture.TestGreenCatalog
            {
                Movies =
                [
                    new AppMovieData { MovieId = 100, EnableDays = 999 }
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
            HddVer = 1113,
            ShopId = "shop"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<SharedStartupAuthResponse>(ok.Value);
        var movie = Assert.Single(response.AryMovieInfoes);
        Assert.Equal(100u, movie.MovieId);
        Assert.Equal(999u, movie.EnableDays);
    }
}
