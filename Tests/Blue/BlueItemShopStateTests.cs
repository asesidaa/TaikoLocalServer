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
                Status = (Ac15ShopItemStatus)0,
                PurchasedAt = DateTime.UtcNow
            });
        await fixture.Context.SaveChangesAsync();

        var items = await fixture.Context.GetUnlockedBlueShopItemsAsync(1, 2, CancellationToken.None);

        Assert.Contains((3u, 12u), items);
        Assert.DoesNotContain((3u, 9u), items);
        Assert.DoesNotContain((3u, 10u), items);
    }

    [Fact]
    public async Task UpdatePlayResult_WhenShopEnabled_AddsDonMedalsToActiveSeasonState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateSingleSongShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreatePlayResultHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            CreatePlayResult(getDonmedal: 25, getKatsumedal: 7)),
            CancellationToken.None);

        var state = await fixture.Context.BlueShopSeasonStates.FindAsync(1u, 2u);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.Equal(1u, result);
        Assert.Equal(25u, state!.TotalGetDonmedal);
        Assert.Equal(0u, state.TotalUseDonmedal);
        Assert.Equal(0u, save!.TotalGetDonmedal);
        Assert.Equal(7u, save.TotalGetKatsumedal);
    }

    [Fact]
    public async Task UpdatePlayResult_WhenShopDisabled_DoesNotCreateShopSeasonState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreatePlayResultHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            CreatePlayResult(getDonmedal: 25)),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.Equal(1u, result);
        Assert.False(await fixture.Context.BlueShopSeasonStates.AnyAsync());
        Assert.Equal(25u, save!.TotalGetDonmedal);
    }

    [Fact]
    public async Task UpdatePlayResult_WhenShopHasNoActiveSeason_DoesNotCreateShopSeasonState()
    {
        var catalog = new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 99,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = new()
                {
                    SeasonId = 2,
                    Items = [new BlueItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 101, Price = 1300 }]
                }
            }
        };
        await using var fixture = await BlueHandlerFixture.CreateAsync(
            new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: catalog));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreatePlayResultHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            CreatePlayResult(getDonmedal: 25)),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.Equal(1u, result);
        Assert.False(await fixture.Context.BlueShopSeasonStates.AnyAsync());
        Assert.Equal(25u, save!.TotalGetDonmedal);
    }

    private static BlueShopItemState Unlocked(uint baid, uint seasonId, uint itemType, uint itemId) => new()
    {
        Baid = baid,
        SeasonId = seasonId,
        ItemType = itemType,
        ItemId = itemId,
        ItemNo = itemId,
        ItemPrice = 1500,
        Status = Ac15ShopItemStatus.Unlocked,
        PurchasedAt = DateTime.UtcNow,
        UnlockedAt = DateTime.UtcNow
    };

    private static UpdatePlayResultCommandHandler CreatePlayResultHandler(BlueHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance,
            Options.Create(new ServerSettings()));

    private static CommonPlayResultData CreatePlayResult(uint getDonmedal, uint getKatsumedal = 0)
        => new()
        {
            Baid = 1,
            GetDonmedal = getDonmedal,
            GetKatsumedal = getKatsumedal,
            PlayDatetime = "2019-01-01T00:00:00Z",
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 1,
                    StageMode = 0,
                    PlayResult = 1,
                    PlayScore = 1000
                }
            ]
        };

    private static BlueHandlerFixture.TestBlueCatalog CreateSingleSongShopCatalog()
    {
        var season = new BlueItemShopSeason
        {
            SeasonId = 2,
            StartDatetime = "20181219070000",
            EndDatetime = "20190314020000",
            Items = [new BlueItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 101, Price = 1300 }]
        };

        return new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason> { [2] = season }
        });
    }
}
