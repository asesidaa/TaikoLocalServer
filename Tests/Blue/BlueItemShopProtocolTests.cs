using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueItemShopProtocolTests
{
    [Fact]
    public async Task GetItemShopInfo_DisabledShopReturnsSuccessWithoutRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var response = await handler.Handle(new GetItemShopInfoQuery(GameEra.Blue), CancellationToken.None);
        var wire = ItemShopMappers.Map(response);

        Assert.Equal(1u, wire.Result);
        Assert.Empty(wire.AryItemshopDatas);
        Assert.Equal(0u, wire.SeasonId);
        Assert.Equal(0u, wire.VerupNo);
    }

    [Fact]
    public async Task GetItemShopInfo_InactiveShopReturnsSuccessWithoutRows()
    {
        var catalog = new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 99,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = CreateSeason()
            }
        };
        await using var fixture = await BlueHandlerFixture.CreateAsync(
            new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: catalog));
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var response = await handler.Handle(new GetItemShopInfoQuery(GameEra.Blue), CancellationToken.None);
        var wire = ItemShopMappers.Map(response);

        Assert.Equal(1u, wire.Result);
        Assert.Empty(wire.AryItemshopDatas);
        Assert.Equal(0u, wire.SeasonId);
        Assert.Equal(0u, wire.VerupNo);
    }

    [Fact]
    public async Task GetItemShopInfo_EnabledShopReturnsActiveSeasonRowsOrderedByItemNo()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var response = await handler.Handle(new GetItemShopInfoQuery(GameEra.Blue), CancellationToken.None);
        var wire = ItemShopMappers.Map(response);

        Assert.Equal(1u, wire.Result);
        Assert.Equal(2u, wire.SeasonId);
        Assert.Equal(20170404u, wire.VerupNo);
        Assert.Equal("Blue Shop", wire.Telop);
        Assert.Equal("20181219070000", wire.StartDatetime);
        Assert.Equal("20190314020000", wire.EndDatetime);
        Assert.Equal(30u, wire.AfterstartDays);
        Assert.Equal(0u, wire.BeforecloseDays);
        Assert.Equal(2, wire.AryItemshopDatas.Count);
        Assert.Equal(1u, wire.AryItemshopDatas[0].ItemNo);
        Assert.Equal(3u, wire.AryItemshopDatas[0].ItemType);
        Assert.Equal(12u, wire.AryItemshopDatas[0].ItemId);
        Assert.Equal(1300u, wire.AryItemshopDatas[0].ItemPrice);
        Assert.Equal(2u, wire.AryItemshopDatas[1].ItemNo);
    }

    [Fact]
    public async Task GetItemShopInfo_UnsupportedEraThrows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(new GetItemShopInfoQuery((GameEra)999), CancellationToken.None));

        Assert.Contains("Unsupported era", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BlueGetItemShopInfoMap_UsesBlueEra()
    {
        var query = ItemShopMappers.Map(new GetitemshopinfoRequest
        {
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123"
        });

        Assert.Equal(GameEra.Blue, query.Era);
    }

    private static BlueHandlerFixture.TestBlueCatalog CreateShopCatalog()
    {
        return new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = CreateSeason()
            }
        });
    }

    private static BlueItemShopSeason CreateSeason()
        => new()
        {
            SeasonId = 2,
            VerupNo = 20170404,
            Telop = "Blue Shop",
            StartDatetime = "20181219070000",
            EndDatetime = "20190314020000",
            AfterstartDays = 30,
            BeforecloseDays = 0,
            Items =
            [
                new BlueItemShopEntry { ItemNo = 2, ItemType = 3, ItemId = 7, Price = 1500 },
                new BlueItemShopEntry { ItemNo = 1, ItemType = 3, ItemId = 12, Price = 1300 }
            ]
        };
}
