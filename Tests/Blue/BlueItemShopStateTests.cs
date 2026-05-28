using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueItemShopStateTests
{
    [Fact]
    public async Task GetOrCreateBlueShopSeasonState_FirstTouchStartsAtZero()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.TotalGetDonmedal = 100;
        save.TotalUseDonmedal = 40;
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateBlueShopSeasonStateAsync(save, 2, CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Equal(2u, state.SeasonId);
        Assert.Equal(0u, state.TotalGetDonmedal);
        Assert.Equal(0u, state.TotalUseDonmedal);
    }

    [Fact]
    public async Task GetOrCreateActiveBlueShopSeasonState_DisabledShopDoesNotCreateState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.TotalGetDonmedal = 100;
        save.TotalUseDonmedal = 40;
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateActiveBlueShopSeasonStateAsync(
            save,
            BlueItemShopCatalog.Disabled,
            CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Null(state);
        Assert.False(await fixture.Context.BlueShopSeasonStates.AnyAsync());
    }

    [Fact]
    public async Task GetOrCreateActiveBlueShopSeasonState_EmptyActiveSeasonDoesNotCreateState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();

        var catalog = new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = new() { SeasonId = 2, Items = [] }
            }
        };

        var state = await fixture.Context.GetOrCreateActiveBlueShopSeasonStateAsync(
            save,
            catalog,
            CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Null(state);
        Assert.False(await fixture.Context.BlueShopSeasonStates.AnyAsync());
    }

    [Fact]
    public async Task GetUnlockedBlueShopItemsAsync_ReturnsOnlyUnlockedItemsForSeason()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.BlueShopItemStates.AddRange(
            Unlocked(1, 2, 3, 12),
            Unlocked(1, 3, 3, 9),
            new BlueShopItemState
            {
                Baid = 1,
                SeasonId = 2,
                ItemType = 3,
                ItemId = 10,
                ItemNo = 3,
                ItemPrice = 1500,
                Status = (BlueShopItemStatus)0,
                PurchasedAt = DateTime.UtcNow
            });
        await fixture.Context.SaveChangesAsync();

        var items = await fixture.Context.GetUnlockedBlueShopItemsAsync(1, 2, CancellationToken.None);

        Assert.Contains((3u, 12u), items);
        Assert.DoesNotContain((3u, 9u), items);
        Assert.DoesNotContain((3u, 10u), items);
    }

    [Fact]
    public void BlueShopUnlocks_SetAndClearBitsUseBlueFixedWidths()
    {
        var set = BlueShopUnlocks.SetBits([], [4, 255, 256], BlueProtocolBytes.CostumeFlagBytes);

        Assert.True(BlueShopUnlocks.HasBit(set, 4, BlueProtocolBytes.CostumeFlagBytes));
        Assert.True(BlueShopUnlocks.HasBit(set, 255, BlueProtocolBytes.CostumeFlagBytes));
        Assert.False(BlueShopUnlocks.HasBit(set, 256, BlueProtocolBytes.CostumeFlagBytes));

        var cleared = BlueShopUnlocks.ClearBits(set, [4], BlueProtocolBytes.CostumeFlagBytes);

        Assert.False(BlueShopUnlocks.HasBit(cleared, 4, BlueProtocolBytes.CostumeFlagBytes));
        Assert.True(BlueShopUnlocks.HasBit(cleared, 255, BlueProtocolBytes.CostumeFlagBytes));
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, cleared.Length);
    }

    private static BlueShopItemState Unlocked(uint baid, uint seasonId, uint itemType, uint itemId) => new()
    {
        Baid = baid,
        SeasonId = seasonId,
        ItemType = itemType,
        ItemId = itemId,
        ItemNo = itemId,
        ItemPrice = 1500,
        Status = BlueShopItemStatus.Unlocked,
        PurchasedAt = DateTime.UtcNow,
        UnlockedAt = DateTime.UtcNow
    };
}
