using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ItemShopServiceTests
{
    [Fact]
    public async Task Purchase_PreflightCreatesSeasonStateAndReturnsCurrentTotals()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        var saveData = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        database.Context.UserSaveDataBlue.Add(saveData);
        database.Context.BlueShopSeasonStates.Add(new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 7,
            TotalGetDonmedal = 300,
            TotalUseDonmedal = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await database.Context.SaveChangesAsync();

        var response = await Ac15ItemShopService.PurchaseBlueAsync(
            database.Context,
            new Ac15ItemShopPurchaseRequest(1, 0, null, null, null),
            Catalog(),
            saveData,
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(300u, response.TotalGetDonmedal);
        Assert.Equal(100u, response.TotalUseDonmedal);
    }

    [Fact]
    public async Task Purchase_RejectsMismatchedTupleWithoutSpending()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        var saveData = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        database.Context.UserSaveDataBlue.Add(saveData);
        database.Context.BlueShopSeasonStates.Add(new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 7,
            TotalGetDonmedal = 300,
            TotalUseDonmedal = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await database.Context.SaveChangesAsync();

        var response = await Ac15ItemShopService.PurchaseBlueAsync(
            database.Context,
            new Ac15ItemShopPurchaseRequest(1, 1, 2, 999, 200),
            Catalog(),
            saveData,
            CancellationToken.None);

        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, response.TotalUseDonmedal);
        Assert.Empty(await database.Context.BlueShopItemStates.ToListAsync());
    }

    [Fact]
    public async Task Purchase_ValidItemSpendsAndAppliesUnlock()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        var saveData = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        database.Context.UserSaveDataBlue.Add(saveData);
        database.Context.BlueShopSeasonStates.Add(new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 7,
            TotalGetDonmedal = 300,
            TotalUseDonmedal = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await database.Context.SaveChangesAsync();

        var response = await Ac15ItemShopService.PurchaseBlueAsync(
            database.Context,
            new Ac15ItemShopPurchaseRequest(1, 1, Ac15ShopItemType.Tone.ToProtocolValue(), 44, 200),
            Catalog(),
            saveData,
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(200u, response.TotalUseDonmedal);
        var item = await database.Context.BlueShopItemStates.SingleAsync();
        Assert.Equal(44u, item.ItemId);
        Assert.Equal(1u, item.ItemNo);
        Assert.Equal(200u, item.ItemPrice);
        Assert.Equal(Ac15ShopItemStatus.Unlocked, item.Status);
        Assert.NotEqual(0, saveData.ToneFlg[44 >> 3] & (1 << (44 & 7)));
    }

    private static Ac15ItemShopCatalog Catalog() => new()
    {
        IsEnabled = true,
        ActiveSeasonId = 7,
        Seasons = new Dictionary<uint, Ac15ItemShopSeason>
        {
            [7] = new()
            {
                SeasonId = 7,
                Items =
                [
                    new() { ItemNo = 1, ItemType = Ac15ShopItemType.Tone, ItemId = 44, Price = 200 }
                ]
            }
        }
    };

    private sealed class SchemaDatabase(SqliteConnection connection) : IAsyncDisposable
    {
        public TaikoDbContext Context { get; } = CreateContext(connection);

        public static async Task<SchemaDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var database = new SchemaDatabase(connection);
            await database.Context.Database.EnsureCreatedAsync();
            return database;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }

        private static TaikoDbContext CreateContext(SqliteConnection connection)
            => new(new DbContextOptionsBuilder<TaikoDbContext>()
                .UseSqlite(connection)
                .Options);
    }
}
