# AC15 Item Shop Capabilities Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace `PurchaseBlueAsync`, `PurchaseGreenAsync`, `PurchaseYellowAsync`, save-row union records, and item-shop table switches with one generic purchase workflow plus explicit unlock policies.

**Architecture:** Era handlers bind the active catalog, save row, concrete shop item table, season-state factory, Mapperly item-state factory, and unlock policy. Shared item-shop code performs validation, duplicate detection, spend mutation, item-state insertion, and response totals without choosing tables by era.

**Tech Stack:** C# 13, .NET 10, EF Core SQLite, Mapperly, Mediator handlers, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15ItemShopPurchase.cs` - switch-free purchase workflow.
- `Application/Ac15/Ac15ItemShopUnlockPolicies.cs` - Blue, Green, and Yellow explicit unlock policies.

Modify:

- `Application/Ac15/Ac15ItemShopRecords.cs` - add table and policy records.
- `Application/Ac15/Ac15ItemShopService.cs` - delete after callers move.
- `Application/Handlers/ItemPurchaseCommand.Blue.cs`
- `Application/Handlers/ItemPurchaseCommand.Green.cs`
- `Application/Handlers/ItemPurchaseCommand.Yellow.cs`
- `Tests/Ac15/Ac15ItemShopServiceTests.cs`
- Existing Blue, Green, and Yellow item-shop purchase tests.

## Task 1: Generic Purchase Workflow

**Files:**
- Modify: `Application/Ac15/Ac15ItemShopRecords.cs`
- Create: `Application/Ac15/Ac15ItemShopPurchase.cs`
- Create: `Application/Ac15/Ac15ItemShopUnlockPolicies.cs`
- Modify: `Tests/Ac15/Ac15ItemShopServiceTests.cs`

- [ ] **Step 1: Write generic purchase tests against bound Blue tables**

In `Tests/Ac15/Ac15ItemShopServiceTests.cs`, rename the class to `Ac15ItemShopPurchaseTests` and replace calls to `Ac15ItemShopService.PurchaseBlueAsync` with this helper:

```csharp
private static ValueTask<CommonItemPurchaseResponse> PurchaseBlueAsync(
    TaikoDbContext context,
    UserSaveDataBlue saveData,
    Ac15ItemShopPurchaseRequest request)
    => Ac15ItemShopPurchase.PurchaseAsync(
        context,
        request,
        Catalog(),
        saveData,
        new Ac15ItemShopPurchaseTables<BlueShopSeasonState, BlueShopItemState>(
            context.BlueShopItemStates,
            (seasonId, token) => context.GetOrCreateBlueShopSeasonStateAsync(saveData, seasonId, token),
            Ac15ItemShopMapper.ToBlueShopItemState),
        Ac15ItemShopUnlockPolicies.Blue,
        CancellationToken.None);
```

Update the three existing tests to call:

```csharp
var response = await PurchaseBlueAsync(
    database.Context,
    saveData,
    new Ac15ItemShopPurchaseRequest(1, 0, null, null, null));
```

Add this test to the same file:

```csharp
[Fact]
public async Task Purchase_GreenSongPurchaseSpendsWithoutReleaseSongUnlock()
{
    await using var database = await SchemaDatabase.CreateAsync();
    database.Context.UserData.Add(new UserDatum { Baid = 1 });
    var saveData = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    database.Context.UserSaveDataGreen.Add(saveData);
    database.Context.GreenShopSeasonStates.Add(new GreenShopSeasonState
    {
        Baid = 1,
        SeasonId = 7,
        TotalGetDonmedal = 300,
        TotalUseDonmedal = 0,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    });
    await database.Context.SaveChangesAsync();

    var response = await Ac15ItemShopPurchase.PurchaseAsync(
        database.Context,
        new Ac15ItemShopPurchaseRequest(1, 1, Ac15ShopItemType.Song.ToProtocolValue(), 101, 200),
        new Ac15ItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 7,
            Seasons = new Dictionary<uint, Ac15ItemShopSeason>
            {
                [7] = new()
                {
                    SeasonId = 7,
                    Items = [new() { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 101, Price = 200 }]
                }
            }
        },
        saveData,
        new Ac15ItemShopPurchaseTables<GreenShopSeasonState, GreenShopItemState>(
            database.Context.GreenShopItemStates,
            (seasonId, token) => database.Context.GetOrCreateGreenShopSeasonStateAsync(saveData, seasonId, token),
            Ac15ItemShopMapper.ToGreenShopItemState),
        Ac15ItemShopUnlockPolicies.Green,
        CancellationToken.None);

    Assert.Equal(1u, response.Result);
    Assert.Equal(200u, response.TotalUseDonmedal);
    Assert.Single(await database.Context.GreenShopItemStates.ToListAsync());
}
```

- [ ] **Step 2: Run item-shop tests and verify compile failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ItemShopPurchaseTests"
```

Expected: compile failure because `Ac15ItemShopPurchase`, `Ac15ItemShopPurchaseTables<TSeason,TItem>`, and `Ac15ItemShopUnlockPolicies` do not exist.

- [ ] **Step 3: Add purchase table and policy records**

Add these records to `Application/Ac15/Ac15ItemShopRecords.cs`:

```csharp
public sealed record Ac15ItemShopPurchaseTables<TSeason, TItem>(
    DbSet<TItem> ItemStates,
    Func<uint, CancellationToken, ValueTask<TSeason?>> GetOrCreateSeason,
    Func<Ac15PurchasedShopItem, DateTime, TItem> CreateItem)
    where TSeason : class, IAc15ShopSeasonState
    where TItem : class, IAc15ShopItemState;

public sealed record Ac15ItemShopUnlockPolicy<TSave>(
    Func<Ac15ShopItemType, bool> IsSupported,
    Action<TSave, Ac15ShopItemType, uint> ApplyUnlock);
```

- [ ] **Step 4: Add explicit unlock policies**

Create `Application/Ac15/Ac15ItemShopUnlockPolicies.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ItemShopUnlockPolicies
{
    public static Ac15ItemShopUnlockPolicy<UserSaveDataBlue> Blue { get; } = new(
        IsSupportedItemType,
        (save, itemType, itemId) => ApplyUnlock(save, itemType, itemId, BlueProtocolBytes.SongFlagBytes, BlueProtocolBytes.ToneFlagBytes, BlueProtocolBytes.CostumeFlagBytes));

    public static Ac15ItemShopUnlockPolicy<UserSaveDataGreen> Green { get; } = new(
        IsSupportedItemType,
        (save, itemType, itemId) =>
        {
            if (itemType == Ac15ShopItemType.Song)
            {
                return;
            }

            ApplyUnlock(save, itemType, itemId, 0, GreenProtocolBytes.ToneFlagBytes, GreenProtocolBytes.CostumeFlagBytes);
        });

    public static Ac15ItemShopUnlockPolicy<UserSaveDataYellow> Yellow { get; } = new(
        IsSupportedItemType,
        (save, itemType, itemId) => ApplyUnlock(save, itemType, itemId, Ac15EraProfiles.Yellow.Limits.SongFlagBytes, Ac15EraProfiles.Yellow.Limits.ToneFlagBytes, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes));

    private static bool IsSupportedItemType(Ac15ShopItemType itemType)
        => itemType is Ac15ShopItemType.Song
            or Ac15ShopItemType.Tone
            or Ac15ShopItemType.Kigurumi
            or Ac15ShopItemType.Body
            or Ac15ShopItemType.Head
            or Ac15ShopItemType.Face
            or Ac15ShopItemType.Puchi;

    private static void ApplyUnlock(UserSaveDataBlue save, Ac15ShopItemType itemType, uint itemId, int songFlagBytes, int toneFlagBytes, int costumeFlagBytes)
    {
        switch (itemType)
        {
            case Ac15ShopItemType.Song:
                save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, [itemId], songFlagBytes);
                return;
            case Ac15ShopItemType.Tone:
                save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, [itemId], toneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, [itemId], costumeFlagBytes);
                return;
        }
    }

    private static void ApplyUnlock(UserSaveDataGreen save, Ac15ShopItemType itemType, uint itemId, int songFlagBytes, int toneFlagBytes, int costumeFlagBytes)
    {
        switch (itemType)
        {
            case Ac15ShopItemType.Tone:
                save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, [itemId], toneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, [itemId], costumeFlagBytes);
                return;
        }
    }

    private static void ApplyUnlock(UserSaveDataYellow save, Ac15ShopItemType itemType, uint itemId, int songFlagBytes, int toneFlagBytes, int costumeFlagBytes)
    {
        switch (itemType)
        {
            case Ac15ShopItemType.Song:
                save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, [itemId], songFlagBytes);
                return;
            case Ac15ShopItemType.Tone:
                save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, [itemId], toneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, [itemId], costumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, [itemId], costumeFlagBytes);
                return;
        }
    }
}
```

- [ ] **Step 5: Add generic purchase workflow**

Create `Application/Ac15/Ac15ItemShopPurchase.cs`:

```csharp
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ItemShopPurchase
{
    public static async ValueTask<CommonItemPurchaseResponse> PurchaseAsync<TSave, TSeason, TItem>(
        ITaikoDbContext context,
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        TSave saveData,
        Ac15ItemShopPurchaseTables<TSeason, TItem> tables,
        Ac15ItemShopUnlockPolicy<TSave> unlockPolicy,
        CancellationToken cancellationToken)
        where TSeason : class, IAc15ShopSeasonState
        where TItem : class, IAc15ShopItemState
    {
        cancellationToken.ThrowIfCancellationRequested();

        var activeSeason = catalog.IsEnabled ? catalog.ActiveSeason : null;
        if (activeSeason is null)
        {
            return new CommonItemPurchaseResponse { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };
        }

        var seasonState = await tables.GetOrCreateSeason(activeSeason.SeasonId, cancellationToken);
        if (seasonState is null)
        {
            return new CommonItemPurchaseResponse { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };
        }

        if (activeSeason.Items.Count == 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            return IsPreflight(request) ? Success(seasonState) : Failure(seasonState);
        }

        if (IsPreflight(request))
        {
            await context.SaveChangesAsync(cancellationToken);
            return Success(seasonState);
        }

        if (!activeSeason.ItemsByNo.TryGetValue(request.ItemNo, out var item)
            || !unlockPolicy.IsSupported(item.ItemType)
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
        if (item.Price > available || !CanAdd(seasonState.TotalUseDonmedal, item.Price))
        {
            return Failure(seasonState);
        }

        var itemTypeValue = item.ItemType.ToProtocolValue();
        if (await tables.ItemStates.FindAsync([request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId], cancellationToken) is not null)
        {
            return Failure(seasonState);
        }

        var now = DateTime.UtcNow;
        seasonState.TotalUseDonmedal += item.Price;
        seasonState.UpdatedAt = now;
        unlockPolicy.ApplyUnlock(saveData, item.ItemType, item.ItemId);
        tables.ItemStates.Add(tables.CreateItem(
            new Ac15PurchasedShopItem(request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId, item.ItemNo, item.Price),
            now));
        await context.SaveChangesAsync(cancellationToken);

        return Success(seasonState);
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static bool IsPreflight(Ac15ItemShopPurchaseRequest request)
        => request.ItemNo == 0
           && request.ItemType is null
           && request.ItemId is null
           && request.ItemPrice is null;

    private static CommonItemPurchaseResponse Success(IAc15ShopSeasonState state)
        => new() { Result = 1, TotalGetDonmedal = state.TotalGetDonmedal, TotalUseDonmedal = state.TotalUseDonmedal };

    private static CommonItemPurchaseResponse Failure(IAc15ShopSeasonState state)
        => new() { Result = 0, TotalGetDonmedal = state.TotalGetDonmedal, TotalUseDonmedal = state.TotalUseDonmedal };
}
```

- [ ] **Step 6: Run generic purchase tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ItemShopPurchaseTests"
```

Expected: PASS.

## Task 2: Bind Item-Shop Capabilities In Era Handlers

**Files:**
- Modify: `Application/Handlers/ItemPurchaseCommand.Blue.cs`
- Modify: `Application/Handlers/ItemPurchaseCommand.Green.cs`
- Modify: `Application/Handlers/ItemPurchaseCommand.Yellow.cs`
- Delete: `Application/Ac15/Ac15ItemShopService.cs`

- [ ] **Step 1: Replace Blue purchase call**

In `Application/Handlers/ItemPurchaseCommand.Blue.cs`, replace `Ac15ItemShopService.PurchaseBlueAsync(...)` with:

```csharp
return await Ac15ItemShopPurchase.PurchaseAsync(
    context,
    new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
    snapshot.ItemShopCatalog,
    saveData,
    new Ac15ItemShopPurchaseTables<BlueShopSeasonState, BlueShopItemState>(
        context.BlueShopItemStates,
        (seasonId, token) => context.GetOrCreateBlueShopSeasonStateAsync(saveData, seasonId, token),
        Ac15ItemShopMapper.ToBlueShopItemState),
    Ac15ItemShopUnlockPolicies.Blue,
    cancellationToken);
```

- [ ] **Step 2: Replace Green purchase call and remove Green preflight fork**

In `Application/Handlers/ItemPurchaseCommand.Green.cs`, remove the active-season and empty-season special branch. The handler body after loading `saveData`, `green`, and `snapshot` should call:

```csharp
return await Ac15ItemShopPurchase.PurchaseAsync(
    context,
    new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
    snapshot.ItemShopCatalog,
    saveData,
    new Ac15ItemShopPurchaseTables<GreenShopSeasonState, GreenShopItemState>(
        context.GreenShopItemStates,
        (seasonId, token) => context.GetOrCreateGreenShopSeasonStateAsync(saveData, seasonId, token),
        Ac15ItemShopMapper.ToGreenShopItemState),
    Ac15ItemShopUnlockPolicies.Green,
    cancellationToken);
```

Delete the now-unused private `Failure(UserSaveDataGreen)`, `Failure(GreenShopSeasonState)`, and `Success(GreenShopSeasonState)` methods.

- [ ] **Step 3: Replace Yellow purchase call and unsupported item precheck**

In `Application/Handlers/ItemPurchaseCommand.Yellow.cs`, remove `TryGetUnsupportedRequestedItem`. The generic purchase workflow rejects unsupported item types before mutation.

Replace `Ac15ItemShopService.PurchaseYellowAsync(...)` with:

```csharp
return await Ac15ItemShopPurchase.PurchaseAsync(
    context,
    new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
    snapshot.ItemShopCatalog,
    saveData,
    new Ac15ItemShopPurchaseTables<YellowShopSeasonState, YellowShopItemState>(
        context.YellowShopItemStates,
        (seasonId, token) => context.GetOrCreateYellowShopSeasonStateAsync(saveData, seasonId, token),
        Ac15ItemShopMapper.ToYellowShopItemState),
    Ac15ItemShopUnlockPolicies.Yellow,
    cancellationToken);
```

Delete the now-unused private `Failure(YellowShopSeasonState)` method.

- [ ] **Step 4: Delete switched service**

Delete `Application/Ac15/Ac15ItemShopService.cs` after no callers remain.

- [ ] **Step 5: Run item-shop behavior tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ItemShopPurchaseTests|FullyQualifiedName~BlueItemShopPurchaseTests|FullyQualifiedName~GreenItemShopPurchaseTests|FullyQualifiedName~YellowItemShopPurchaseTests"
```

Expected: PASS. The Green song purchase tests must prove purchase state is stored while release-song flags remain absent.

- [ ] **Step 6: Run AC15 and era slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Yellow"
```

Expected: PASS for all four commands.

- [ ] **Step 7: Commit stage 4**

Run:

```powershell
git add Application/Ac15/Ac15ItemShopRecords.cs Application/Ac15/Ac15ItemShopPurchase.cs Application/Ac15/Ac15ItemShopUnlockPolicies.cs Application/Ac15/Ac15ItemShopService.cs Application/Handlers/ItemPurchaseCommand.Blue.cs Application/Handlers/ItemPurchaseCommand.Green.cs Application/Handlers/ItemPurchaseCommand.Yellow.cs Tests/Ac15/Ac15ItemShopServiceTests.cs Tests/Blue Tests/Green Tests/Yellow
git commit -m "Compose AC15 item shop purchase through capabilities"
```

## Self-Review

- Spec coverage: duplicate purchase detection, active-season handling, spend mutation, item insertion, Green song no-op, Blue/Yellow song unlocks, and unsupported item rejection are covered.
- Non-goals honored: no Banacoin authority state, no shared shop EF table, no nullable save-row union record, no route changes.
- Type consistency: `Ac15ItemShopPurchaseTables<TSeason,TItem>`, `Ac15ItemShopUnlockPolicy<TSave>`, and `Ac15ItemShopPurchase.PurchaseAsync` are the stable item-shop entry points.
