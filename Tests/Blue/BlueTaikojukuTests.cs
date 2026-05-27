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
    public void TaikojukuMapper_Blue_DropsInvalidPackSlotsAndCapsSongsAtTen()
    {
        var response = TaikojukuMappers.Map(new CommonTaikojukuResponse
        {
            Result = 1,
            Packs =
            [
                new CommonTaikojukuResponse.Pack
                {
                    GetDan = 0,
                    Songs = [new CommonTaikojukuResponse.Song { SongNo = 101, Level = 0 }]
                },
                new CommonTaikojukuResponse.Pack
                {
                    GetDan = 1,
                    Songs =
                    [
                        new CommonTaikojukuResponse.Song { SongNo = 0, Level = 0 },
                        new CommonTaikojukuResponse.Song { SongNo = 1024, Level = 0 },
                        new CommonTaikojukuResponse.Song { SongNo = 101, Level = 5 },
                        new CommonTaikojukuResponse.Song { SongNo = 101, Level = 0 },
                        new CommonTaikojukuResponse.Song { SongNo = 102, Level = 1 },
                        new CommonTaikojukuResponse.Song { SongNo = 103, Level = 2 },
                        new CommonTaikojukuResponse.Song { SongNo = 104, Level = 3 },
                        new CommonTaikojukuResponse.Song { SongNo = 105, Level = 4 },
                        new CommonTaikojukuResponse.Song { SongNo = 106, Level = 0 },
                        new CommonTaikojukuResponse.Song { SongNo = 107, Level = 1 },
                        new CommonTaikojukuResponse.Song { SongNo = 108, Level = 2 },
                        new CommonTaikojukuResponse.Song { SongNo = 109, Level = 3 },
                        new CommonTaikojukuResponse.Song { SongNo = 110, Level = 4 },
                        new CommonTaikojukuResponse.Song { SongNo = 111, Level = 0 }
                    ]
                }
            ]
        });

        var pack = Assert.Single(response.AryJukupackDatas);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(10, pack.AryJukusongDatas.Count);
        Assert.DoesNotContain(pack.AryJukusongDatas, song => song.SongNo is 0 or 1024);
        Assert.All(pack.AryJukusongDatas, song => Assert.InRange(song.Level, 0u, 4u));
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
