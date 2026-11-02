# AC15 Item Shop Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move shared item-shop preflight, purchase validation, Don medal spend, duplicate rejection, and season-state response logic into an AC15 service while keeping Blue and Green shop tables separate.

**Architecture:** Introduce a narrow `IAc15ItemShopPersistence` port plus era-specific adapters. Unlock application stays explicit through `IAc15ItemShopUnlockPolicy`, so Blue song items can set release bits and Green song items can keep current no-op behavior.

**Tech Stack:** C# 13, .NET 10, EF Core through existing `ITaikoDbContext`, Mediator handlers, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15ItemShopRecords.cs` - canonical season state, item identity, purchase request, and purchase outcome records.
- `Application/Ac15/IAc15ItemShopPersistence.cs` - typed persistence port for item-shop state only.
- `Application/Ac15/IAc15ItemShopUnlockPolicy.cs` - era-specific save mutation policy for purchased items.
- `Application/Ac15/Ac15ItemShopService.cs` - shared purchase workflow.
- `Application/Ac15/BlueAc15ItemShopAdapter.cs` - Blue item-shop persistence and unlock policy.
- `Application/Ac15/GreenAc15ItemShopAdapter.cs` - Green item-shop persistence and unlock policy.
- `Tests/Ac15/Ac15ItemShopServiceTests.cs`

Modify:

- `Application/Handlers/ItemPurchaseCommand.Blue.cs`
- `Application/Handlers/ItemPurchaseCommand.Green.cs`
- `Application/Handlers/ItemPurchaseCommand.cs` if shared helper methods become unused.
- Existing Blue/Green item-shop tests only when constructor setup needs adapter dependencies.

## Task 1: Shared Item-Shop Service

**Files:**
- Create: `Tests/Ac15/Ac15ItemShopServiceTests.cs`
- Create: `Application/Ac15/Ac15ItemShopRecords.cs`
- Create: `Application/Ac15/IAc15ItemShopPersistence.cs`
- Create: `Application/Ac15/IAc15ItemShopUnlockPolicy.cs`
- Create: `Application/Ac15/Ac15ItemShopService.cs`

- [ ] **Step 1: Write failing service tests**

Create `Tests/Ac15/Ac15ItemShopServiceTests.cs`:

```csharp
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
            new Ac15ItemShopPurchaseRequest(baid: 1, itemNo: 0, itemType: null, itemId: null, itemPrice: null),
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
            new Ac15ItemShopPurchaseRequest(1, itemNo: 1, itemType: 2, itemId: 999, itemPrice: 200),
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
            new Ac15ItemShopPurchaseRequest(1, itemNo: 1, itemType: Ac15ShopItemType.Tone.ToProtocolValue(), itemId: 44, itemPrice: 200),
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
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ItemShopServiceTests"
```

Expected: compile failure for missing item-shop core types.

- [ ] **Step 3: Add item-shop records and ports**

Create `Application/Ac15/Ac15ItemShopRecords.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15ItemShopPurchaseRequest(
    uint Baid,
    uint ItemNo,
    uint? ItemType,
    uint? ItemId,
    uint? ItemPrice);

public sealed record Ac15ShopSeasonState(
    uint Baid,
    uint SeasonId,
    uint TotalGetDonmedal,
    uint TotalUseDonmedal)
{
    public uint TotalGetDonmedal { get; set; } = TotalGetDonmedal;
    public uint TotalUseDonmedal { get; set; } = TotalUseDonmedal;
}

public sealed record Ac15PurchasedShopItem(
    uint Baid,
    uint SeasonId,
    uint ItemType,
    uint ItemId,
    uint ItemNo,
    uint ItemPrice);
```

Create `Application/Ac15/IAc15ItemShopPersistence.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public interface IAc15ItemShopPersistence
{
    ValueTask<Ac15ShopSeasonState?> GetOrCreateActiveSeasonStateAsync(
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken);

    ValueTask<bool> HasPurchasedItemAsync(
        uint baid,
        uint seasonId,
        uint itemType,
        uint itemId,
        CancellationToken cancellationToken);

    ValueTask AddPurchasedItemAsync(Ac15PurchasedShopItem item, CancellationToken cancellationToken);

    ValueTask SaveChangesAsync(CancellationToken cancellationToken);
}
```

Create `Application/Ac15/IAc15ItemShopUnlockPolicy.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public interface IAc15ItemShopUnlockPolicy
{
    void ApplyUnlock(Ac15ShopItemType itemType, uint itemId);
}
```

- [ ] **Step 4: Add item-shop service**

Create `Application/Ac15/Ac15ItemShopService.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ItemShopService
{
    public static async ValueTask<CommonItemPurchaseResponse> PurchaseAsync(
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        IAc15ItemShopPersistence persistence,
        IAc15ItemShopUnlockPolicy unlockPolicy,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var activeSeason = catalog.IsEnabled ? catalog.ActiveSeason : null;
        if (activeSeason is null || activeSeason.Items.Count == 0)
        {
            return new CommonItemPurchaseResponse { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };
        }

        var seasonState = await persistence.GetOrCreateActiveSeasonStateAsync(
            request.Baid,
            activeSeason.SeasonId,
            cancellationToken);
        if (seasonState is null)
        {
            return new CommonItemPurchaseResponse { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };
        }

        if (IsPreflight(request))
        {
            await persistence.SaveChangesAsync(cancellationToken);
            return Success(seasonState);
        }

        if (!activeSeason.ItemsByNo.TryGetValue(request.ItemNo, out var item)
            || request.ItemType != item.ItemType.ToProtocolValue()
            || request.ItemId != item.ItemId
            || request.ItemPrice != item.Price
            || item.Price == 0)
        {
            return Failure(seasonState);
        }

        var available = seasonState.TotalGetDonmedal >= seasonState.TotalUseDonmedal
            ? seasonState.TotalGetDonmedal - seasonState.TotalUseDonmedal
            : 0;

        var itemTypeValue = item.ItemType.ToProtocolValue();
        if (await persistence.HasPurchasedItemAsync(request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId, cancellationToken))
        {
            return Failure(seasonState);
        }

        if (item.Price > available || !CanAdd(seasonState.TotalUseDonmedal, item.Price))
        {
            return Failure(seasonState);
        }

        seasonState.TotalUseDonmedal += item.Price;
        unlockPolicy.ApplyUnlock(item.ItemType, item.ItemId);
        await persistence.AddPurchasedItemAsync(
            new Ac15PurchasedShopItem(request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId, item.ItemNo, item.Price),
            cancellationToken);
        await persistence.SaveChangesAsync(cancellationToken);

        return Success(seasonState);
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static bool IsPreflight(Ac15ItemShopPurchaseRequest request)
        => request.ItemNo == 0
           && request.ItemType is null
           && request.ItemId is null
           && request.ItemPrice is null;

    private static CommonItemPurchaseResponse Success(Ac15ShopSeasonState state)
        => new()
        {
            Result = 1,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };

    private static CommonItemPurchaseResponse Failure(Ac15ShopSeasonState state)
        => new()
        {
            Result = 0,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };
}
```

- [ ] **Step 5: Run item-shop core tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ItemShopServiceTests"
```

Expected: PASS.

## Task 2: Blue And Green Item-Shop Adapters

**Files:**
- Create: `Application/Ac15/BlueAc15ItemShopAdapter.cs`
- Create: `Application/Ac15/GreenAc15ItemShopAdapter.cs`
- Modify: `Application/Handlers/ItemPurchaseCommand.Blue.cs`
- Modify: `Application/Handlers/ItemPurchaseCommand.Green.cs`

- [ ] **Step 1: Add Blue adapter**

Create `Application/Ac15/BlueAc15ItemShopAdapter.cs`:

```csharp
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Application.Ac15;

public sealed class BlueAc15ItemShopAdapter(ITaikoDbContext context, UserSaveDataBlue saveData)
    : IAc15ItemShopPersistence, IAc15ItemShopUnlockPolicy
{
    public async ValueTask<Ac15ShopSeasonState?> GetOrCreateActiveSeasonStateAsync(
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken)
    {
        var state = await context.GetOrCreateBlueShopSeasonStateAsync(baid, seasonId, cancellationToken);
        return new Ac15ShopSeasonState(state.Baid, state.SeasonId, state.TotalGetDonmedal, state.TotalUseDonmedal);
    }

    public async ValueTask<bool> HasPurchasedItemAsync(
        uint baid,
        uint seasonId,
        uint itemType,
        uint itemId,
        CancellationToken cancellationToken)
        => await context.BlueShopItemStates.FindAsync([baid, seasonId, itemType, itemId], cancellationToken) is not null;

    public async ValueTask AddPurchasedItemAsync(Ac15PurchasedShopItem item, CancellationToken cancellationToken)
    {
        var state = await context.GetOrCreateBlueShopSeasonStateAsync(item.Baid, item.SeasonId, cancellationToken);
        state.TotalUseDonmedal += item.ItemPrice;
        state.UpdatedAt = DateTime.UtcNow;
        context.BlueShopItemStates.Add(new BlueShopItemState
        {
            Baid = item.Baid,
            SeasonId = item.SeasonId,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            ItemNo = item.ItemNo,
            ItemPrice = item.ItemPrice,
            Status = BlueShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        });
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => new(context.SaveChangesAsync(cancellationToken));

    public void ApplyUnlock(Ac15ShopItemType itemType, uint itemId)
    {
        switch (itemType)
        {
            case Ac15ShopItemType.Song:
                saveData.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(saveData.ReleaseSongFlg, [itemId], BlueProtocolBytes.SongFlagBytes);
                return;
            case Ac15ShopItemType.Tone:
                saveData.ToneFlg = Ac15ProtocolBytes.SetBits(saveData.ToneFlg, [itemId], BlueProtocolBytes.ToneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            default:
                throw new InvalidOperationException($"Unsupported Blue item shop item type {itemType}.");
        }
    }
}
```

- [ ] **Step 2: Add Green adapter**

Create `Application/Ac15/GreenAc15ItemShopAdapter.cs`:

```csharp
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Application.Ac15;

public sealed class GreenAc15ItemShopAdapter(ITaikoDbContext context, UserSaveDataGreen saveData)
    : IAc15ItemShopPersistence, IAc15ItemShopUnlockPolicy
{
    public async ValueTask<Ac15ShopSeasonState?> GetOrCreateActiveSeasonStateAsync(
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken)
    {
        var state = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, seasonId, cancellationToken);
        return new Ac15ShopSeasonState(state.Baid, state.SeasonId, state.TotalGetDonmedal, state.TotalUseDonmedal);
    }

    public async ValueTask<bool> HasPurchasedItemAsync(
        uint baid,
        uint seasonId,
        uint itemType,
        uint itemId,
        CancellationToken cancellationToken)
        => await context.GreenShopItemStates.FindAsync([baid, seasonId, itemType, itemId], cancellationToken) is not null;

    public async ValueTask AddPurchasedItemAsync(Ac15PurchasedShopItem item, CancellationToken cancellationToken)
    {
        var state = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, item.SeasonId, cancellationToken);
        state.TotalUseDonmedal += item.ItemPrice;
        state.UpdatedAt = DateTime.UtcNow;
        context.GreenShopItemStates.Add(new GreenShopItemState
        {
            Baid = item.Baid,
            SeasonId = item.SeasonId,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            ItemNo = item.ItemNo,
            ItemPrice = item.ItemPrice,
            Status = GreenShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        });
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => new(context.SaveChangesAsync(cancellationToken));

    public void ApplyUnlock(Ac15ShopItemType itemType, uint itemId)
    {
        switch (itemType)
        {
            case Ac15ShopItemType.Song:
                return;
            case Ac15ShopItemType.Tone:
                saveData.ToneFlg = Ac15ProtocolBytes.SetBits(saveData.ToneFlg, [itemId], GreenProtocolBytes.ToneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            default:
                throw new InvalidOperationException($"Unsupported Green item shop item type {itemType}.");
        }
    }
}
```

- [ ] **Step 3: Update item-purchase handlers**

In Blue, replace the duplicated purchase body after `saveData` and `catalog` are resolved with:

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromBlue(gameDataService.Blue());
var adapter = new BlueAc15ItemShopAdapter(context, saveData);
return await Ac15ItemShopService.PurchaseAsync(
    new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
    snapshot.ItemShopCatalog,
    adapter,
    adapter,
    cancellationToken);
```

In Green, use:

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromGreen(gameDataService.Green());
var adapter = new GreenAc15ItemShopAdapter(context, saveData);
return await Ac15ItemShopService.PurchaseAsync(
    new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
    snapshot.ItemShopCatalog,
    adapter,
    adapter,
    cancellationToken);
```

Keep the existing `logger.LogDebug` line at the top of both handlers.

- [ ] **Step 4: Run item-shop regression tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ItemShopServiceTests|FullyQualifiedName~BlueItemShopPurchaseTests|FullyQualifiedName~BlueItemShopStateTests|FullyQualifiedName~GreenItemShopPurchaseTests|FullyQualifiedName~GreenItemShopStateTests"
```

Expected: PASS. The service increments the canonical response state, and each adapter increments the tracked EF season state once in `AddPurchasedItemAsync`; do not add a second EF spend update in the handler.

- [ ] **Step 5: Commit stage 4**

Run:

```powershell
git add Application/Ac15 Application/Handlers/ItemPurchaseCommand.Blue.cs Application/Handlers/ItemPurchaseCommand.Green.cs Tests/Ac15 Tests/Blue/BlueItemShopPurchaseTests.cs Tests/Blue/BlueItemShopStateTests.cs Tests/Green/GreenItemShopPurchaseTests.cs Tests/Green/GreenItemShopStateTests.cs
git commit -m "Extract AC15 item shop purchase core"
```
