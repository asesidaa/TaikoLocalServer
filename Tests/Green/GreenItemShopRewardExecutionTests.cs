namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopRewardExecutionTests
{
    [Fact]
    public async Task RewardExecution_UnlocksPendingSongToneBodyAndHead()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.GreenShopItemStates.AddRange(
            Pending(1, 2, 1, 101),
            Pending(1, 2, 2, 4),
            Pending(1, 2, 4, 146),
            Pending(1, 2, 5, 117));
        await fixture.Context.SaveChangesAsync();

        var handler = new RewardExecutionCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<RewardExecutionCommandHandler>.Instance);

        var response = await handler.Handle(new RewardExecutionCommand(
            1,
            [101],
            [4],
            [],
            [117],
            [146],
            [],
            [],
            []), CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.Equal(1u, response.Result);
        Assert.True(HasBit(save!.ToneFlg, 4));
        Assert.True(HasBit(save.CostumeFlg2, 117));
        Assert.True(HasBit(save.CostumeFlg3, 146));
        Assert.All(await fixture.Context.GreenShopItemStates.ToListAsync(), row => Assert.Equal(GreenShopItemStatus.Unlocked, row.Status));
    }

    [Fact]
    public async Task RewardExecution_RejectsForgedShopReward()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new RewardExecutionCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<RewardExecutionCommandHandler>.Instance);

        var response = await handler.Handle(new RewardExecutionCommand(
            1,
            [101],
            [],
            [],
            [],
            [],
            [],
            [],
            []), CancellationToken.None);

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
                new GreenItemShopEntry { ItemNo = 1, ItemType = 1, ItemId = 101, Price = 1300 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = 2, ItemId = 4, Price = 500 },
                new GreenItemShopEntry { ItemNo = 3, ItemType = 4, ItemId = 146, Price = 500 },
                new GreenItemShopEntry { ItemNo = 4, ItemType = 5, ItemId = 117, Price = 500 }
            ]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }

    private static GreenShopItemState Pending(uint baid, uint seasonId, uint itemType, uint itemId)
        => new()
        {
            Baid = baid,
            SeasonId = seasonId,
            ItemType = itemType,
            ItemId = itemId,
            ItemNo = 1,
            ItemPrice = 1,
            Status = GreenShopItemStatus.PendingReward,
            PurchasedAt = DateTime.UtcNow
        };

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
