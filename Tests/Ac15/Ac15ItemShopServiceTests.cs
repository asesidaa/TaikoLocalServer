using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ItemShopServiceTests
{
    [Fact]
    public async Task Purchase_PreflightCreatesSeasonStateAndReturnsCurrentTotals()
    {
        var persistence = new FakePersistence(totalGet: 300, totalUse: 100);
        var response = await Ac15ItemShopService.PurchaseAsync(
            new Ac15ItemShopPurchaseRequest(1, 0, null, null, null),
            Catalog(),
            persistence,
            FakeUnlockPolicy.Instance,
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(300u, response.TotalGetDonmedal);
        Assert.Equal(100u, response.TotalUseDonmedal);
        Assert.True(persistence.SaveWasCalled);
    }

    [Fact]
    public async Task Purchase_RejectsMismatchedTupleWithoutSpending()
    {
        var persistence = new FakePersistence(totalGet: 300, totalUse: 0);
        var response = await Ac15ItemShopService.PurchaseAsync(
            new Ac15ItemShopPurchaseRequest(1, 1, 2, 999, 200),
            Catalog(),
            persistence,
            FakeUnlockPolicy.Instance,
            CancellationToken.None);

        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, response.TotalUseDonmedal);
        Assert.Empty(persistence.PurchasedItems);
    }

    [Fact]
    public async Task Purchase_ValidItemSpendsAndAppliesUnlock()
    {
        var persistence = new FakePersistence(totalGet: 300, totalUse: 0);
        var unlocks = new FakeUnlockPolicy();
        var response = await Ac15ItemShopService.PurchaseAsync(
            new Ac15ItemShopPurchaseRequest(1, 1, Ac15ShopItemType.Tone.ToProtocolValue(), 44, 200),
            Catalog(),
            persistence,
            unlocks,
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(200u, response.TotalUseDonmedal);
        Assert.Contains(new Ac15PurchasedShopItem(1, 7, Ac15ShopItemType.Tone.ToProtocolValue(), 44, 1, 200), persistence.PurchasedItems);
        Assert.Equal([(Ac15ShopItemType.Tone, 44u)], unlocks.Applied);
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

    private sealed class FakePersistence(uint totalGet, uint totalUse) : IAc15ItemShopPersistence
    {
        public bool SaveWasCalled { get; private set; }
        public List<Ac15PurchasedShopItem> PurchasedItems { get; } = [];
        private Ac15ShopSeasonState State { get; } = new(1, 7, totalGet, totalUse);

        public ValueTask<Ac15ShopSeasonState?> GetOrCreateActiveSeasonStateAsync(uint baid, uint seasonId, CancellationToken cancellationToken)
            => ValueTask.FromResult<Ac15ShopSeasonState?>(State);

        public ValueTask<bool> HasPurchasedItemAsync(uint baid, uint seasonId, uint itemType, uint itemId, CancellationToken cancellationToken)
            => ValueTask.FromResult(PurchasedItems.Any(item => item.Baid == baid && item.SeasonId == seasonId && item.ItemType == itemType && item.ItemId == itemId));

        public ValueTask AddPurchasedItemAsync(Ac15PurchasedShopItem item, CancellationToken cancellationToken)
        {
            PurchasedItems.Add(item);
            return ValueTask.CompletedTask;
        }

        public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveWasCalled = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeUnlockPolicy : IAc15ItemShopUnlockPolicy
    {
        public static FakeUnlockPolicy Instance { get; } = new();
        public List<(Ac15ShopItemType ItemType, uint ItemId)> Applied { get; } = [];

        public void ApplyUnlock(Ac15ShopItemType itemType, uint itemId)
        {
            Applied.Add((itemType, itemId));
        }
    }
}
