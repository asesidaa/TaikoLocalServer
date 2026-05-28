using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueItemShopPurchaseTests
{
    [Fact]
    public async Task ItemPurchase_PreflightReturnsSeasonBalanceWithoutSpending()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog(
            new BlueItemShopEntry { ItemNo = 1, ItemType = 3, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 700, totalUseDonmedal: 200);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Blue, 0, null, null, null), CancellationToken.None);

        var season = await fixture.Context.BlueShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(700u, response.TotalGetDonmedal);
        Assert.Equal(200u, response.TotalUseDonmedal);
        Assert.Equal(200u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.BlueShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_DisabledShopReturnsZeroTotalsWithoutCreatingState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        await AddUserAsync(fixture);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Blue, 0, null, null, null), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(0u, response.TotalGetDonmedal);
        Assert.Equal(0u, response.TotalUseDonmedal);
        Assert.False(await fixture.Context.BlueShopSeasonStates.AnyAsync());
        Assert.False(await fixture.Context.BlueShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_RejectsMismatchedCatalogTupleWithoutMutation()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog(
            new BlueItemShopEntry { ItemNo = 1, ItemType = 3, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Blue, 1, 3, 12, 1500), CancellationToken.None);

        var season = await fixture.Context.BlueShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.BlueShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_RejectsZeroPriceRowsWithoutMutation()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog(
            new BlueItemShopEntry { ItemNo = 1, ItemType = 3, ItemId = 12, Price = 0 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Blue, 1, 3, 12, 0), CancellationToken.None);

        var season = await fixture.Context.BlueShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.BlueShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_RejectsInsufficientMedalsWithoutMutation()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog(
            new BlueItemShopEntry { ItemNo = 1, ItemType = 3, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 1200);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Blue, 1, 3, 12, 1300), CancellationToken.None);

        var season = await fixture.Context.BlueShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.BlueShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_RejectsDuplicateUnlockedWithoutDoubleSpend()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog(
            new BlueItemShopEntry { ItemNo = 1, ItemType = 3, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000, totalUseDonmedal: 1300);
        fixture.Context.BlueShopItemStates.Add(new BlueShopItemState
        {
            Baid = 1,
            SeasonId = 2,
            ItemType = 3,
            ItemId = 12,
            ItemNo = 1,
            ItemPrice = 1300,
            Status = BlueShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Blue, 1, 3, 12, 1300), CancellationToken.None);

        var season = await fixture.Context.BlueShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(1300u, season!.TotalUseDonmedal);
        Assert.Single(await fixture.Context.BlueShopItemStates.ToListAsync());
    }

    [Fact]
    public async Task ItemPurchase_ActiveSeasonSpendsDonmedalsAndPersistsUnlockedItem()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog(
            new BlueItemShopEntry { ItemNo = 1, ItemType = 3, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Blue, 1, 3, 12, 1300), CancellationToken.None);

        var season = await fixture.Context.BlueShopSeasonStates.FindAsync(1u, 2u);
        var item = await fixture.Context.BlueShopItemStates.FindAsync(1u, 2u, 3u, 12u);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(2000u, response.TotalGetDonmedal);
        Assert.Equal(1300u, response.TotalUseDonmedal);
        Assert.Equal(1300u, season!.TotalUseDonmedal);
        Assert.Equal(BlueShopItemStatus.Unlocked, item!.Status);
        Assert.Equal(1u, item.ItemNo);
        Assert.Equal(1300u, item.ItemPrice);
        Assert.NotNull(item.UnlockedAt);
        Assert.True(HasBit(save!.CostumeFlg1, 12));
    }

    [Theory]
    [InlineData(1, 101, nameof(UserSaveDataBlue.ReleaseSongFlg))]
    [InlineData(2, 4, nameof(UserSaveDataBlue.ToneFlg))]
    [InlineData(3, 12, nameof(UserSaveDataBlue.CostumeFlg1))]
    [InlineData(4, 13, nameof(UserSaveDataBlue.CostumeFlg3))]
    [InlineData(5, 14, nameof(UserSaveDataBlue.CostumeFlg2))]
    [InlineData(6, 15, nameof(UserSaveDataBlue.CostumeFlg4))]
    [InlineData(7, 16, nameof(UserSaveDataBlue.CostumeFlg5))]
    public async Task ItemPurchase_UnlocksSupportedItemTypesInExactBlueSaveField(
        uint itemType,
        uint itemId,
        string expectedField)
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog(
            new BlueItemShopEntry { ItemNo = 1, ItemType = itemType, ItemId = itemId, Price = 100 }));
        var save = await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var before = SnapshotUnlockFields(save);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Blue, 1, itemType, itemId, 100), CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.Equal(1u, response.Result);
        foreach (var (field, bytes) in before)
        {
            var current = GetUnlockField(reloaded!, field);
            if (field == expectedField)
            {
                Assert.True(HasBit(current, itemId));
                Assert.NotEqual(bytes, current);
            }
            else
            {
                Assert.Equal(bytes, current);
            }
        }
    }

    private static ItemPurchaseCommandHandler CreateHandler(BlueHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

    private static async Task AddUserAsync(BlueHandlerFixture fixture)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.TotalGetDonmedal = 999;
        save.TotalUseDonmedal = 111;
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
    }

    private static async Task<UserSaveDataBlue> AddUserWithSeasonAsync(
        BlueHandlerFixture fixture,
        uint totalGetDonmedal,
        uint totalUseDonmedal = 0)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        fixture.Context.UserSaveDataBlue.Add(save);
        fixture.Context.BlueShopSeasonStates.Add(new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 2,
            TotalGetDonmedal = totalGetDonmedal,
            TotalUseDonmedal = totalUseDonmedal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();
        return save;
    }

    private static BlueHandlerFixture.TestBlueCatalog CreateShopCatalog(params BlueItemShopEntry[] items)
        => new(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = new()
                {
                    SeasonId = 2,
                    VerupNo = 20170404,
                    StartDatetime = "20181219070000",
                    EndDatetime = "20190314020000",
                    Items = items
                }
            }
        });

    private static Dictionary<string, byte[]> SnapshotUnlockFields(UserSaveDataBlue saveData)
        => new()
        {
            [nameof(UserSaveDataBlue.ReleaseSongFlg)] = saveData.ReleaseSongFlg.ToArray(),
            [nameof(UserSaveDataBlue.ToneFlg)] = saveData.ToneFlg.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg1)] = saveData.CostumeFlg1.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg2)] = saveData.CostumeFlg2.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg3)] = saveData.CostumeFlg3.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg4)] = saveData.CostumeFlg4.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg5)] = saveData.CostumeFlg5.ToArray()
        };

    private static byte[] GetUnlockField(UserSaveDataBlue saveData, string field)
        => field switch
        {
            nameof(UserSaveDataBlue.ReleaseSongFlg) => saveData.ReleaseSongFlg,
            nameof(UserSaveDataBlue.ToneFlg) => saveData.ToneFlg,
            nameof(UserSaveDataBlue.CostumeFlg1) => saveData.CostumeFlg1,
            nameof(UserSaveDataBlue.CostumeFlg2) => saveData.CostumeFlg2,
            nameof(UserSaveDataBlue.CostumeFlg3) => saveData.CostumeFlg3,
            nameof(UserSaveDataBlue.CostumeFlg4) => saveData.CostumeFlg4,
            nameof(UserSaveDataBlue.CostumeFlg5) => saveData.CostumeFlg5,
            _ => throw new InvalidOperationException($"Unsupported Blue unlock field {field}.")
        };

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
