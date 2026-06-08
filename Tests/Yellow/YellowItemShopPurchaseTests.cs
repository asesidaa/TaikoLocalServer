using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowItemShopPurchaseTests
{
    [Fact]
    public async Task GetOrCreateActiveYellowShopSeasonState_FirstTouchSeedsYellowSaveDonMedalTotals()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        save.TotalGetDonmedal = 120;
        save.TotalUseDonmedal = 45;
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateActiveYellowShopSeasonStateAsync(
            save,
            CreateSingleItemShopCatalog(),
            CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.NotNull(state);
        Assert.Equal(2u, state!.SeasonId);
        Assert.Equal(120u, state.TotalGetDonmedal);
        Assert.Equal(45u, state.TotalUseDonmedal);
    }

    [Fact]
    public async Task GetOrCreateActiveYellowShopSeasonState_DisabledShopDoesNotCreateState()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        save.TotalGetDonmedal = 120;
        save.TotalUseDonmedal = 45;
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateActiveYellowShopSeasonStateAsync(
            save,
            YellowItemShopCatalog.Disabled,
            CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Null(state);
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.ToListAsync());
    }

    [Fact]
    public async Task GetOrCreateActiveYellowShopSeasonState_EmptyActiveSeasonDoesNotCreateState()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();

        var catalog = new YellowItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, YellowItemShopSeason>
            {
                [2] = new() { SeasonId = 2, Items = [] }
            }
        };

        var state = await fixture.Context.GetOrCreateActiveYellowShopSeasonStateAsync(
            save,
            catalog,
            CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Null(state);
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.ToListAsync());
    }

    [Fact]
    public async Task GetUnlockedYellowShopItemsAsync_ReturnsOnlyYellowUnlockedItemsForSeason()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.YellowShopItemStates.AddRange(
            Unlocked(1, 2, 3, 12),
            Unlocked(1, 3, 3, 9),
            new YellowShopItemState
            {
                Baid = 1,
                SeasonId = 2,
                ItemType = 3,
                ItemId = 10,
                ItemNo = 3,
                ItemPrice = 1500,
                Status = (YellowShopItemStatus)0,
                PurchasedAt = DateTime.UtcNow
            });
        fixture.Context.BlueShopItemStates.Add(new BlueShopItemState
        {
            Baid = 1,
            SeasonId = 2,
            ItemType = 3,
            ItemId = 99,
            ItemNo = 99,
            ItemPrice = 1500,
            Status = BlueShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow
        });
        fixture.Context.GreenShopItemStates.Add(new GreenShopItemState
        {
            Baid = 1,
            SeasonId = 2,
            ItemType = 3,
            ItemId = 100,
            ItemNo = 100,
            ItemPrice = 1500,
            Status = GreenShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var items = await fixture.Context.GetUnlockedYellowShopItemsAsync(1, 2, CancellationToken.None);

        Assert.Contains((3u, 12u), items);
        Assert.DoesNotContain((3u, 9u), items);
        Assert.DoesNotContain((3u, 10u), items);
        Assert.DoesNotContain((3u, 99u), items);
        Assert.DoesNotContain((3u, 100u), items);
    }

    private static YellowShopItemState Unlocked(uint baid, uint seasonId, uint itemType, uint itemId) => new()
    {
        Baid = baid,
        SeasonId = seasonId,
        ItemType = itemType,
        ItemId = itemId,
        ItemNo = itemId,
        ItemPrice = 1500,
        Status = YellowShopItemStatus.Unlocked,
        PurchasedAt = DateTime.UtcNow,
        UnlockedAt = DateTime.UtcNow
    };

    private static YellowItemShopCatalog CreateSingleItemShopCatalog()
    {
        var season = new YellowItemShopSeason
        {
            SeasonId = 2,
            Items = [new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 101, Price = 1300 }]
        };

        return new YellowItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, YellowItemShopSeason> { [2] = season }
        };
    }
}
