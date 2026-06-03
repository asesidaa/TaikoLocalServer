namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopLockingTests
{
    [Fact]
    public async Task UserData_LocksActiveShopSongAndToneUntilUnlocked()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.ToneFlg = GreenProtocolBytes.CreateFixedBitset([0, 4], GreenProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.False(HasBit(response.ReleaseSongFlg, 101));
        Assert.False(HasBit(response.ToneFlg, 4));
    }

    [Fact]
    public async Task UserData_RestoresUnlockedActiveShopSongAndTone()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.ToneFlg = GreenProtocolBytes.CreateFixedBitset([0, 4], GreenProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenShopItemStates.AddRange(
            Unlocked(1, 2, 1, 101),
            Unlocked(1, 2, 2, 4));
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.True(HasBit(response.ReleaseSongFlg, 101));
        Assert.True(HasBit(response.ToneFlg, 4));
    }

    [Fact]
    public async Task Baid_LocksActiveShopCostumesUntilUnlocked()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 1, AccessCode = "abc" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.CostumeFlg2 = GreenProtocolBytes.CreateFixedBitset([0, 117], GreenProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg3 = GreenProtocolBytes.CreateFixedBitset([0, 146], GreenProtocolBytes.CostumeFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Green, "abc"), CancellationToken.None);

        Assert.False(HasBit(response.CostumeFlg2!, 117));
        Assert.False(HasBit(response.CostumeFlg3!, 146));
    }

    [Fact]
    public async Task Baid_RestoresUnlockedActiveShopCostumes()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 1, AccessCode = "abc" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.CostumeFlg2 = GreenProtocolBytes.CreateFixedBitset([0, 117], GreenProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg3 = GreenProtocolBytes.CreateFixedBitset([0, 146], GreenProtocolBytes.CostumeFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenShopItemStates.AddRange(
            Unlocked(1, 2, 5, 117),
            Unlocked(1, 2, 4, 146));
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Green, "abc"), CancellationToken.None);

        Assert.True(HasBit(response.CostumeFlg2!, 117));
        Assert.True(HasBit(response.CostumeFlg3!, 146));
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
            Items =
            [
                new GreenItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 101, Price = 1300 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = Ac15ShopItemType.Tone, ItemId = 4, Price = 500 },
                new GreenItemShopEntry { ItemNo = 3, ItemType = Ac15ShopItemType.Head, ItemId = 117, Price = 500 },
                new GreenItemShopEntry { ItemNo = 4, ItemType = Ac15ShopItemType.Body, ItemId = 146, Price = 500 }
            ]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }

    private static GreenShopItemState Unlocked(uint baid, uint seasonId, uint itemType, uint itemId)
        => new()
        {
            Baid = baid,
            SeasonId = seasonId,
            ItemType = itemType,
            ItemId = itemId,
            ItemNo = 1,
            ItemPrice = 1,
            Status = GreenShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        };

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
