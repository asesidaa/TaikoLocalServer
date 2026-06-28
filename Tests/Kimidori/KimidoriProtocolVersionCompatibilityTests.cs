using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application;
using TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;
using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

namespace TaikoLocalServer.Tests.Kimidori;

public sealed class KimidoriProtocolVersionCompatibilityTests
{
    [Fact]
    public async Task Taikojuku_FinalRouteReturnsCatalogPacks()
    {
        var catalog = new FileGameDataCatalog([CreateTaikojukuCatalog(1)]);
        await using var services = new ServiceCollection()
            .AddApplication()
            .AddLogging()
            .AddSingleton<IGameDataCatalog>(catalog)
            .BuildServiceProvider();

        var response = await InvokeAsync(
            ActivatorUtilities.CreateInstance<TaikojukuController>(services),
            controller => controller.Taikojuku(new FinalWire.TaikojukuRequest
            {
                ChassisId = "268410000000",
                ShopId = "JPN0JPN0123",
                GetDans = [1]
            }),
            value => Assert.IsType<FinalWire.TaikojukuResponse>(value),
            services);

        Assert.Equal(1u, response.Result);
        var pack = Assert.Single(response.AryJukupackDatas);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal([101u, 102u], pack.AryJukusongDatas.Select(song => song.SongNo).ToArray());
    }

    private static KimidoriHandlerFixture.TestKimidoriCatalog CreateTaikojukuCatalog(params uint[] challengeLevels)
        => new()
        {
            TaikojukuFileOrder = challengeLevels
                .Select((dan, index) => new Ac15TaikojukuEntry
                {
                    UniqueId = 20001u + (uint)index,
                    ChallengeLevel = dan,
                    DanLevel = dan,
                    Name = $"Dan {dan}",
                    Songs =
                    [
                        new Ac15TaikojukuSong { SongNo = 101, Level = Difficulty.Easy },
                        new Ac15TaikojukuSong { SongNo = 102, Level = Difficulty.Normal }
                    ]
                })
                .ToArray()
        };

    private static async Task<TResponse> InvokeAsync<TController, TResponse>(
        TController controller,
        Func<TController, Task<IActionResult>> action,
        Func<object?, TResponse> assertResponse,
        IServiceProvider services)
        where TController : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services }
        };

        var ok = Assert.IsType<OkObjectResult>(await action(controller));
        return assertResponse(ok.Value);
    }
}
