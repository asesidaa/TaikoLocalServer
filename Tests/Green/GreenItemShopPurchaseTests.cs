namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopPurchaseTests
{
    [Fact]
    public async Task ItemPurchase_ActiveSeasonSpendsDonmedalsAndCreatesPendingReward()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 1000;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, 2, 1, 865, 300), CancellationToken.None);

        var season = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
        var item = await fixture.Context.GreenShopItemStates.FindAsync(1u, 2u, 1u, 865u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(1000u, response.TotalGetDonmedal);
        Assert.Equal(300u, response.TotalUseDonmedal);
        Assert.Equal(300u, season!.TotalUseDonmedal);
        Assert.Equal(GreenShopItemStatus.PendingReward, item!.Status);
    }

    [Fact]
    public async Task ItemPurchase_RejectsDuplicatePendingWithoutDoubleSpend()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 1000;
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenShopSeasonStates.Add(new GreenShopSeasonState
        {
            Baid = 1,
            SeasonId = 2,
            TotalGetDonmedal = 1000,
            TotalUseDonmedal = 300,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        fixture.Context.GreenShopItemStates.Add(new GreenShopItemState
        {
            Baid = 1,
            SeasonId = 2,
            ItemType = 1,
            ItemId = 865,
            ItemNo = 2,
            ItemPrice = 300,
            Status = GreenShopItemStatus.PendingReward,
            PurchasedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, 2, 1, 865, 300), CancellationToken.None);

        var season = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(300u, season!.TotalUseDonmedal);
    }

    [Fact]
    public async Task ItemPurchase_RejectsMismatchedItemNo()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 1000;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, 1, 1, 865, 300), CancellationToken.None);

        Assert.Equal(0u, response.Result);
    }

    private static GreenHandlerFixture.TestGreenCatalog CreateShopCatalog()
    {
        var season = new GreenItemShopSeason
        {
            SeasonId = 2,
            StartDatetime = "20190314000000",
            EndDatetime = "20190626075959",
            Items =
            [
                new GreenItemShopEntry { ItemNo = 1, ItemType = 4, ItemId = 117, Price = 500 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = 1, ItemId = 865, Price = 300 }
            ]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }
}
