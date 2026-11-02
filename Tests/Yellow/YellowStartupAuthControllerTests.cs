using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
using TaikoLocalServer.Application;
using AppMovieData = TaikoLocalServer.Application.ServerData.MovieData;
using SharedStartupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthRequest;
using SharedStartupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthResponse;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowStartupAuthControllerTests
{
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
}
