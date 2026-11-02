# 06 - Purchase Flow

**Goal:** Validate purchases against the active season, spend active-season Don medals, and create pending item state without double spending.

**Files:**

- Modify: `Application/Handlers/ItemPurchaseCommand.Green.cs`
- Create: `Tests/Green/GreenItemShopPurchaseTests.cs`
- Update: `Tests/Green/GreenGhostRewardTests.cs` if existing purchase tests need active-season expectations.

## Acceptance Criteria

- [ ] Purchase rejects when the active shop is disabled or empty.
- [ ] Purchase validates inferred `item_no`, `item_type`, `item_id`, and `item_price`.
- [ ] Purchase spends from `GreenShopSeasonState`.
- [ ] Purchase creates `GreenShopItemState` with `PendingReward`.
- [ ] Duplicate pending/unlocked purchase rejects without double spending.

## Steps

- [ ] **Step 1: Add purchase tests**

Create `Tests/Green/GreenItemShopPurchaseTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopPurchaseTests
{
    [Fact]
    public async Task ItemPurchase_ActiveSeasonSpendsDonmedalsAndCreatesPendingReward()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 1000;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, 2, 1, 865, 300), CancellationToken.None);

        var season = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
        var item = await fixture.Context.GreenShopItemStates.FindAsync(1u, 2u, 1u, 865u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(1000u, response.TotalGetDonmedal);
        Assert.Equal(300u, response.TotalUseDonmedal);
        Assert.Equal(300u, season!.TotalUseDonmedal);
        Assert.Equal(GreenShopItemStatus.PendingReward, item!.Status);
    }

    [Fact]
    public async Task ItemPurchase_RejectsDuplicatePendingWithoutDoubleSpend()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 1000;
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenShopSeasonStates.Add(new GreenShopSeasonState
        {
            Baid = 1,
            SeasonId = 2,
            TotalGetDonmedal = 1000,
            TotalUseDonmedal = 300,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        fixture.Context.GreenShopItemStates.Add(new GreenShopItemState
        {
            Baid = 1,
            SeasonId = 2,
            ItemType = 1,
            ItemId = 865,
            ItemNo = 2,
            ItemPrice = 300,
            Status = GreenShopItemStatus.PendingReward,
            PurchasedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, 2, 1, 865, 300), CancellationToken.None);

        var season = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(300u, season!.TotalUseDonmedal);
    }

    [Fact]
    public async Task ItemPurchase_RejectsMismatchedItemNo()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 1000;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, 1, 1, 865, 300), CancellationToken.None);

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
                new GreenItemShopEntry { ItemNo = 1, ItemType = 4, ItemId = 117, Price = 500 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = 1, ItemId = 865, Price = 300 }
            ]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopPurchaseTests"
```

Expected: fails because purchase still uses global totals and does not create item state.

- [ ] **Step 3: Implement purchase flow**

Replace the body of `Application/Handlers/ItemPurchaseCommand.Green.cs` with:

```csharp
public partial async ValueTask<CommonItemPurchaseResponse> Handle(
    ItemPurchaseCommand request,
    CancellationToken cancellationToken)
{
    logger.LogDebug("Applying Green item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
    var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
    var green = gameDataService.Green();
    var activeSeason = green.ItemShopCatalog.ActiveSeason;
    if (activeSeason is null
        || !activeSeason.ItemsByNo.TryGetValue(request.ItemNo, out var item)
        || request.ItemType != item.ItemType
        || request.ItemId != item.ItemId
        || request.ItemPrice != item.Price
        || item.Price == 0)
    {
        return Failure(saveData);
    }

    var seasonState = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeSeason.SeasonId, cancellationToken);
    var available = seasonState.TotalGetDonmedal >= seasonState.TotalUseDonmedal
        ? seasonState.TotalGetDonmedal - seasonState.TotalUseDonmedal
        : 0;

    var existingItem = await context.GreenShopItemStates.FindAsync(
        [request.Baid, activeSeason.SeasonId, item.ItemType, item.ItemId],
        cancellationToken);

    if (existingItem is not null || item.Price > available || !CanAdd(seasonState.TotalUseDonmedal, item.Price))
    {
        return Failure(seasonState);
    }

    var now = DateTime.UtcNow;
    seasonState.TotalUseDonmedal += item.Price;
    seasonState.UpdatedAt = now;
    context.GreenShopItemStates.Add(new GreenShopItemState
    {
        Baid = request.Baid,
        SeasonId = activeSeason.SeasonId,
        ItemType = item.ItemType,
        ItemId = item.ItemId,
        ItemNo = item.ItemNo,
        ItemPrice = item.Price,
        Status = GreenShopItemStatus.PendingReward,
        PurchasedAt = now
    });

    await context.SaveChangesAsync(cancellationToken);
    return Success(seasonState);
}

private static bool CanAdd(uint current, uint delta)
    => delta <= uint.MaxValue - current;

private static CommonItemPurchaseResponse Failure(UserSaveDataGreen saveData)
    => new()
    {
        Result = 0,
        TotalGetDonmedal = saveData.TotalGetDonmedal,
        TotalUseDonmedal = saveData.TotalUseDonmedal
    };

private static CommonItemPurchaseResponse Failure(GreenShopSeasonState state)
    => new()
    {
        Result = 0,
        TotalGetDonmedal = state.TotalGetDonmedal,
        TotalUseDonmedal = state.TotalUseDonmedal
    };

private static CommonItemPurchaseResponse Success(GreenShopSeasonState state)
    => new()
    {
        Result = 1,
        TotalGetDonmedal = state.TotalGetDonmedal,
        TotalUseDonmedal = state.TotalUseDonmedal
    };
```

- [ ] **Step 4: Run focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopPurchaseTests"
```

Expected: pass.

- [ ] **Step 5: Run legacy purchase tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenGhostRewardTests"
```

Expected: update old purchase tests to either use disabled-shop rejection expectations or the new active shop catalog/state behavior. Keep ghost and card-check tests unchanged.

- [ ] **Step 6: Commit**

```powershell
git status --short
git add -- Application/Handlers/ItemPurchaseCommand.Green.cs Tests/Green/GreenItemShopPurchaseTests.cs Tests/Green/GreenGhostRewardTests.cs
git commit -m "Implement Green active-season item purchases"
```

