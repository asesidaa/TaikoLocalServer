using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
using TaikoLocalServer.Application;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueTaikojukuTests
{
    [Fact]
    public async Task GetTaikojuku_Blue_ReturnsRequestedDanSlotPack()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery(GameEra.Blue, [1]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        var pack = Assert.Single(response.Packs);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(3, pack.Songs.Count);
    }

    [Fact]
    public async Task GetTaikojuku_Blue_DoesNotTreatUniqueIdAsDanSlot()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery(GameEra.Blue, [20001]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Single(response.Packs);
        Assert.DoesNotContain(response.Packs, pack => pack.GetDan == 20001);
        Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    [Fact]
    public async Task GetTaikojuku_Blue_AllInvalidRequestSlotsFallbackIsCappedToEleven()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(
            new GetTaikojukuQuery(GameEra.Blue, Enumerable.Range(101, 25).Select(value => (uint)value).ToArray()),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(11, response.Packs.Count);
        Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    [Fact]
    public async Task TaikojukuController_Blue_ReturnsMappedCatalogPacks()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var controller = new TaikojukuController
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.RequestServices = new ServiceCollection()
            .AddApplication()
            .AddLogging()
            .AddSingleton(fixture.Catalog)
            .BuildServiceProvider();

        var result = await controller.Taikojuku(new TaikojukuRequest
        {
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            GetDans = [1]
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TaikojukuResponse>(ok.Value);
        Assert.Equal(1u, response.Result);
        Assert.Contains(response.AryJukupackDatas, pack => pack.GetDan == 1);
    }
}
