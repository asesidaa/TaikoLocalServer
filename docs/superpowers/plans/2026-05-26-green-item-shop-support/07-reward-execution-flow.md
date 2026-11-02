# 07 - Reward Execution Flow

**Goal:** Replace the current already-unlocked-only behavior with pending-purchase validation and apply item_type unlocks to the correct Green bitsets.

**Files:**

- Modify: `Application/Handlers/RewardExecutionCommand.cs`
- Modify: `Application/Handlers/RewardExecutionCommand.Green.cs`
- Modify: existing tests constructing `RewardExecutionCommandHandler`
- Create: `Tests/Green/GreenItemShopRewardExecutionTests.cs`

## Acceptance Criteria

- [ ] Reward execution accepts only active-season pending purchased items or already-unlocked retries.
- [ ] Pending rows become `Unlocked`.
- [ ] Songs, tones, and costume slots unlock through the correct bitset.
- [ ] `item_type=4` unlocks `CostumeFlg3` body.
- [ ] `item_type=5` unlocks `CostumeFlg2` head.
- [ ] Forged reward ids reject.

## Steps

- [ ] **Step 1: Add reward execution tests**

Create `Tests/Green/GreenItemShopRewardExecutionTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopRewardExecutionTests"
```

Expected: fails because reward execution rejects new unlocks and does not read pending state.

- [ ] **Step 3: Inject catalog into reward handler**

Modify `Application/Handlers/RewardExecutionCommand.cs`:

```csharp
public partial class RewardExecutionCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<RewardExecutionCommandHandler> logger)
    : IRequestHandler<RewardExecutionCommand, CommonRewardExecutionResponse>
```

Update existing tests that construct `RewardExecutionCommandHandler` to pass `fixture.Catalog`.

- [ ] **Step 4: Implement item_type request flattening**

In `Application/Handlers/RewardExecutionCommand.Green.cs`, add:

```csharp
private static IEnumerable<(uint ItemType, uint ItemId)> RequestedShopItems(RewardExecutionCommand request)
{
    foreach (var id in request.ReleaseSongNoes) yield return (1, id);
    foreach (var id in request.GetToneNoes) yield return (2, id);
    foreach (var id in request.GetCostumeNo1s) yield return (3, id);
    foreach (var id in request.GetCostumeNo2s) yield return (5, id);
    foreach (var id in request.GetCostumeNo3s) yield return (4, id);
    foreach (var id in request.GetCostumeNo4s) yield return (6, id);
    foreach (var id in request.GetCostumeNo5s) yield return (7, id);
}
```

Do not include title ids.

- [ ] **Step 5: Replace validation and unlock logic**

In `Handle`, after loading save data:

```csharp
var activeSeason = gameDataService.Green().ItemShopCatalog.ActiveSeason;
if (activeSeason is null)
{
    return new CommonRewardExecutionResponse { Result = 0 };
}

var requested = RequestedShopItems(request).Distinct().ToArray();
var activeCatalogKeys = activeSeason.Items
    .Select(item => (item.ItemType, item.ItemId))
    .ToHashSet();

if (requested.Any(item => !activeCatalogKeys.Contains(item)))
{
    logger.LogWarning("Rejecting forged Green shop reward ids for baid {Baid}", request.Baid);
    return new CommonRewardExecutionResponse { Result = 0 };
}

var states = await context.GreenShopItemStates
    .Where(row => row.Baid == request.Baid && row.SeasonId == activeSeason.SeasonId)
    .ToDictionaryAsync(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId), cancellationToken);

foreach (var item in requested)
{
    if (!states.TryGetValue(item, out var state)
        || state.Status is not (GreenShopItemStatus.PendingReward or GreenShopItemStatus.Unlocked))
    {
        logger.LogWarning("Rejecting Green shop reward without pending purchase for baid {Baid}", request.Baid);
        return new CommonRewardExecutionResponse { Result = 0 };
    }
}
```

Then apply bits:

```csharp
saveData.ToneFlg = GreenShopUnlocks.SetBits(saveData.ToneFlg, request.GetToneNoes, GreenProtocolBytes.ToneFlagBytes);
saveData.CostumeFlg1 = GreenShopUnlocks.SetBits(saveData.CostumeFlg1, request.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes);
saveData.CostumeFlg2 = GreenShopUnlocks.SetBits(saveData.CostumeFlg2, request.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes);
saveData.CostumeFlg3 = GreenShopUnlocks.SetBits(saveData.CostumeFlg3, request.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes);
saveData.CostumeFlg4 = GreenShopUnlocks.SetBits(saveData.CostumeFlg4, request.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes);
saveData.CostumeFlg5 = GreenShopUnlocks.SetBits(saveData.CostumeFlg5, request.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes);

var now = DateTime.UtcNow;
foreach (var item in requested)
{
    var state = states[item];
    state.Status = GreenShopItemStatus.Unlocked;
    state.UnlockedAt ??= now;
}

await context.SaveChangesAsync(cancellationToken);
return new CommonRewardExecutionResponse { Result = 1 };
```

Remove the old `AllAlreadyUnlocked`, `HasBit`, and local `SetBits` helpers if they are no longer used.

- [ ] **Step 6: Run focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopRewardExecutionTests"
```

Expected: pass.

- [ ] **Step 7: Run legacy reward tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenGhostRewardTests"
```

Expected: update the old unknown reward test to match active-shop pending-purchase semantics.

- [ ] **Step 8: Commit**

```powershell
git status --short
git add -- Application/Handlers/RewardExecutionCommand.cs Application/Handlers/RewardExecutionCommand.Green.cs Tests/Green/GreenItemShopRewardExecutionTests.cs Tests/Green/GreenGhostRewardTests.cs
git commit -m "Unlock Green shop rewards after purchase"
```
