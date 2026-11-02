namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopPurchaseTests
{
    [Fact]
    public async Task ItemPurchase_ActiveSeasonSpendsDonmedalsAndUnlocksReward()
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

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Green, 2, 1, 865, 300), CancellationToken.None);

        var season = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
        var item = await fixture.Context.GreenShopItemStates.FindAsync(1u, 2u, 1u, 865u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(1000u, response.TotalGetDonmedal);
        Assert.Equal(300u, response.TotalUseDonmedal);
        Assert.Equal(300u, season!.TotalUseDonmedal);
        Assert.Equal(Ac15ShopItemStatus.Unlocked, item!.Status);
        Assert.NotNull(item.UnlockedAt);
    }

    [Fact]
    public async Task ItemPurchase_PreflightReturnsSeasonBalanceWithoutSpending()
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
            TotalGetDonmedal = 700,
            TotalUseDonmedal = 200,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Green, 0, null, null, null), CancellationToken.None);

        var season = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(700u, response.TotalGetDonmedal);
        Assert.Equal(200u, response.TotalUseDonmedal);
        Assert.Equal(200u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.GreenShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_UnlocksPurchasedCostumeImmediately()
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

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Green, 3, 5, 117, 500), CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        var item = await fixture.Context.GreenShopItemStates.FindAsync(1u, 2u, 5u, 117u);
        Assert.Equal(1u, response.Result);
        Assert.True(HasBit(reloaded!.CostumeFlg2, 117));
        Assert.Equal(Ac15ShopItemStatus.Unlocked, item!.Status);
    }

    [Fact]
    public async Task ItemPurchase_RejectsDuplicateUnlockedWithoutDoubleSpend()
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
            Status = Ac15ShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Green, 2, 1, 865, 300), CancellationToken.None);

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

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Green, 1, 1, 865, 300), CancellationToken.None);

        Assert.Equal(0u, response.Result);
    }

    private static GreenHandlerFixture.TestGreenCatalog CreateShopCatalog()
    {
        var season = new Ac15ItemShopSeason
        {
            SeasonId = 2,
            StartDatetime = "20190314000000",
            EndDatetime = "20190626075959",
            Items =
            [
                new Ac15ItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Body, ItemId = 117, Price = 500 },
                new Ac15ItemShopEntry { ItemNo = 2, ItemType = Ac15ShopItemType.Song, ItemId = 865, Price = 300 },
                new Ac15ItemShopEntry { ItemNo = 3, ItemType = Ac15ShopItemType.Head, ItemId = 117, Price = 500 }
            ]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new Ac15ItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, Ac15ItemShopSeason> { [2] = season }
        });
    }

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
