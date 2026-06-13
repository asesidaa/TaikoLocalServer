using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueItemShopLockingTests
{
    [Fact]
    public async Task UserData_LocksActiveShopSongAndToneUntilUnlocked()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.ToneFlg = BlueProtocolBytes.CreateFixedBitset([0, 4], BlueProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.False(HasBit(response.ReleaseSongFlg, 101));
        Assert.False(HasBit(response.ToneFlg, 4));
    }

    [Fact]
    public async Task UserData_RestoresUnlockedActiveShopSongAndTone()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.ToneFlg = BlueProtocolBytes.CreateFixedBitset([0, 4], BlueProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataBlue.Add(save);
        fixture.Context.BlueShopItemStates.AddRange(
            Unlocked(1, 2, 1, 101),
            Unlocked(1, 2, 2, 4));
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.True(HasBit(response.ReleaseSongFlg, 101));
        Assert.True(HasBit(response.ToneFlg, 4));
    }

    [Fact]
    public async Task Baid_LocksActiveShopCostumesUntilUnlocked()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        AddBlueUser(fixture);
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Blue, "abc"), CancellationToken.None);

        Assert.False(HasBit(response.CostumeFlags!.CostumeFlg1, 12));
        Assert.False(HasBit(response.CostumeFlags.CostumeFlg2, 117));
        Assert.False(HasBit(response.CostumeFlags.CostumeFlg3, 146));
        Assert.False(HasBit(response.CostumeFlags.CostumeFlg4, 6));
        Assert.False(HasBit(response.CostumeFlags.CostumeFlg5, 7));
    }

    [Fact]
    public async Task Baid_RestoresUnlockedActiveShopCostumes()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        AddBlueUser(fixture);
        fixture.Context.BlueShopItemStates.AddRange(
            Unlocked(1, 2, 3, 12),
            Unlocked(1, 2, 5, 117),
            Unlocked(1, 2, 4, 146),
            Unlocked(1, 2, 6, 6),
            Unlocked(1, 2, 7, 7));
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Blue, "abc"), CancellationToken.None);

        Assert.True(HasBit(response.CostumeFlags!.CostumeFlg1, 12));
        Assert.True(HasBit(response.CostumeFlags.CostumeFlg2, 117));
        Assert.True(HasBit(response.CostumeFlags.CostumeFlg3, 146));
        Assert.True(HasBit(response.CostumeFlags.CostumeFlg4, 6));
        Assert.True(HasBit(response.CostumeFlags.CostumeFlg5, 7));
    }

    [Fact]
    public async Task Baid_EnabledShopReportsActiveSeasonTotals()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        var save = AddBlueUser(fixture);
        save.TotalGetDonmedal = 999;
        save.TotalUseDonmedal = 555;
        fixture.Context.BlueShopSeasonStates.Add(new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 2,
            TotalGetDonmedal = 80,
            TotalUseDonmedal = 30,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Blue, "abc"), CancellationToken.None);

        Assert.Equal(80u, response.ShopMedals!.TotalGetDonmedal);
        Assert.Equal(30u, response.ShopMedals.TotalUseDonmedal);
    }

    [Fact]
    public async Task Baid_DisabledShopReportsZeroMedalTotals()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var save = AddBlueUser(fixture);
        save.TotalGetDonmedal = 999;
        save.TotalUseDonmedal = 555;
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Blue, "abc"), CancellationToken.None);

        Assert.Equal(0u, response.ShopMedals!.TotalGetDonmedal);
        Assert.Equal(0u, response.ShopMedals.TotalUseDonmedal);
    }

    [Fact]
    public async Task Readback_HidesActiveShopItemsUntilPurchasedThroughBlueItemPurchase()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        AddBlueUser(fixture);
        fixture.Context.BlueShopSeasonStates.Add(new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 2,
            TotalGetDonmedal = 4000,
            TotalUseDonmedal = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();
        var userDataHandler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));
        var baidHandler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);
        var purchaseHandler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var lockedUserData = await userDataHandler.Handle(new UserDataQuery(1, GameEra.Blue), CancellationToken.None);
        var lockedBaid = await baidHandler.Handle(new Ac15BaidQuery(GameEra.Blue, "abc"), CancellationToken.None);

        Assert.False(HasBit(lockedUserData.ReleaseSongFlg, 101));
        Assert.False(HasBit(lockedUserData.ToneFlg, 4));
        Assert.False(HasBit(lockedBaid.CostumeFlags!.CostumeFlg1, 12));

        Assert.Equal(1u, (await purchaseHandler.Handle(
            new ItemPurchaseCommand(1, GameEra.Blue, 1, 1, 101, 1300),
            CancellationToken.None)).Result);
        Assert.Equal(1u, (await purchaseHandler.Handle(
            new ItemPurchaseCommand(1, GameEra.Blue, 2, 2, 4, 500),
            CancellationToken.None)).Result);
        Assert.Equal(1u, (await purchaseHandler.Handle(
            new ItemPurchaseCommand(1, GameEra.Blue, 3, 3, 12, 1300),
            CancellationToken.None)).Result);

        var unlockedUserData = await userDataHandler.Handle(new UserDataQuery(1, GameEra.Blue), CancellationToken.None);
        var unlockedBaid = await baidHandler.Handle(new Ac15BaidQuery(GameEra.Blue, "abc"), CancellationToken.None);

        Assert.True(HasBit(unlockedUserData.ReleaseSongFlg, 101));
        Assert.True(HasBit(unlockedUserData.ToneFlg, 4));
        Assert.True(HasBit(unlockedBaid.CostumeFlags!.CostumeFlg1, 12));
    }

    private static BlueHandlerFixture.TestBlueCatalog CreateShopCatalog()
    {
        var season = new BlueItemShopSeason
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
                new BlueItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 101, Price = 1300 },
                new BlueItemShopEntry { ItemNo = 2, ItemType = Ac15ShopItemType.Tone, ItemId = 4, Price = 500 },
                new BlueItemShopEntry { ItemNo = 3, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 },
                new BlueItemShopEntry { ItemNo = 4, ItemType = Ac15ShopItemType.Head, ItemId = 117, Price = 500 },
                new BlueItemShopEntry { ItemNo = 5, ItemType = Ac15ShopItemType.Body, ItemId = 146, Price = 500 },
                new BlueItemShopEntry { ItemNo = 6, ItemType = Ac15ShopItemType.Face, ItemId = 6, Price = 500 },
                new BlueItemShopEntry { ItemNo = 7, ItemType = Ac15ShopItemType.Puchi, ItemId = 7, Price = 500 }
            ]
        };

        return new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason> { [2] = season }
        });
    }

    private static UserSaveDataBlue AddBlueUser(BlueHandlerFixture fixture)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 1, AccessCode = "abc" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.CostumeFlg1 = BlueProtocolBytes.CreateFixedBitset([0, 12], BlueProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg2 = BlueProtocolBytes.CreateFixedBitset([0, 117], BlueProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg3 = BlueProtocolBytes.CreateFixedBitset([0, 146], BlueProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg4 = BlueProtocolBytes.CreateFixedBitset([0, 6], BlueProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg5 = BlueProtocolBytes.CreateFixedBitset([0, 7], BlueProtocolBytes.CostumeFlagBytes);
        fixture.Context.UserSaveDataBlue.Add(save);
        return save;
    }

    private static BlueShopItemState Unlocked(uint baid, uint seasonId, uint itemType, uint itemId)
        => new()
        {
            Baid = baid,
            SeasonId = seasonId,
            ItemType = itemType,
            ItemId = itemId,
            ItemNo = itemId,
            ItemPrice = 1,
            Status = Ac15ShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        };

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
