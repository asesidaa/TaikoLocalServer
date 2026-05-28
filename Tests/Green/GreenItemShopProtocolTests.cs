using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopProtocolTests
{
    [Fact]
    public async Task InitialData_AdvertisesActiveGreenItemShopSeason()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.True(response.IsItemshop);
        var info = Assert.Single(response.AryGreenItemShopDatas);
        Assert.Equal(2u, info.InfoId);
        Assert.Equal(9u, info.VerupNo);
        Assert.False(HasBit(response.DefaultSongFlg, 101));
    }

    [Fact]
    public async Task GetItemShopInfo_ReturnsActiveSeasonRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var response = await handler.Handle(new GetItemShopInfoQuery(GameEra.Green), CancellationToken.None);
        var wire = ItemShopMappers.Map(response);

        Assert.Equal(1u, wire.Result);
        Assert.Equal(2u, wire.SeasonId);
        Assert.Equal(9u, wire.VerupNo);
        Assert.Equal("Shop", wire.Telop);
        Assert.Equal("20190314000000", wire.StartDatetime);
        Assert.Equal("20190626075959", wire.EndDatetime);
        Assert.Equal(2, wire.AryItemshopDatas.Count);
        Assert.Equal(1u, wire.AryItemshopDatas[0].ItemNo);
        Assert.Equal(4u, wire.AryItemshopDatas[0].ItemType);
        Assert.Equal(117u, wire.AryItemshopDatas[0].ItemId);
        Assert.Equal(500u, wire.AryItemshopDatas[0].ItemPrice);
    }

    [Fact]
    public void ItemPurchaseCommandMap_PreservesOmittedOptionalDetails()
    {
        var request = new ItempurchaseRequest
        {
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            Baid = 1,
            ItemNo = 0
        };

        var command = ItemShopMappers.Map(request);

        Assert.Equal(1u, command.Baid);
        Assert.Equal(0u, command.ItemNo);
        Assert.Null(command.ItemType);
        Assert.Null(command.ItemId);
        Assert.Null(command.ItemPrice);
    }

    private static GreenHandlerFixture.TestGreenCatalog CreateShopCatalog()
    {
        var season = new GreenItemShopSeason
        {
            SeasonId = 2,
            VerupNo = 9,
            Telop = "Shop",
            StartDatetime = "20190314000000",
            EndDatetime = "20190626075959",
            AfterstartDays = 3,
            BeforecloseDays = 4,
            Items =
            [
                new GreenItemShopEntry { ItemNo = 1, ItemType = 4, ItemId = 117, Price = 500 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = 1, ItemId = 101, Price = 1300 }
            ]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
