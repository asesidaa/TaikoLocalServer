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
                new BlueItemShopEntry { ItemNo = 1, ItemType = 1, ItemId = 101, Price = 1300 },
                new BlueItemShopEntry { ItemNo = 2, ItemType = 2, ItemId = 4, Price = 500 }
            ]
        };

        return new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason> { [2] = season }
        });
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
            Status = BlueShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        };

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
