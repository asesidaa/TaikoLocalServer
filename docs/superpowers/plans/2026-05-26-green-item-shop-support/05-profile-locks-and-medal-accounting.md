# 05 - Profile Locks And Medal Accounting

**Goal:** Apply profile-specific locks for active shop items and move Don medal earning/reporting to active season state when shop is enabled.

**Files:**

- Create: `Application/Common/GreenShopUnlocks.cs`
- Modify: `Application/Handlers/BaidQuery.Green.cs`
- Modify: `Application/Handlers/UserDataQuery.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: handler tests that construct `UpdatePlayResultCommandHandler`
- Create: `Tests/Green/GreenItemShopLockingTests.cs`
- Update: `Tests/Green/GreenItemShopStateTests.cs`

## Acceptance Criteria

- [ ] BAID response reports active-season Don medal totals when shop is enabled.
- [ ] Play-result `GetDonmedal` increments active-season total when shop is enabled.
- [ ] Active shop songs and tones are hidden from userdata until unlocked.
- [ ] Active shop costumes are hidden from BAID costume flags until unlocked.
- [ ] `item_type=4` locks body (`CostumeFlg3`) and `item_type=5` locks head (`CostumeFlg2`).
- [ ] `ItemshopTutorialFlg` remains global.

## Steps

- [ ] **Step 1: Add locking tests**

Create `Tests/Green/GreenItemShopLockingTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopLockingTests
{
    [Fact]
    public async Task UserData_LocksActiveShopSongAndToneUntilUnlocked()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.ToneFlg = GreenProtocolBytes.CreateFixedBitset([0, 4], GreenProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.False(HasBit(response.ReleaseSongFlg, 101));
        Assert.False(HasBit(response.ToneFlg, 4));
    }

    [Fact]
    public async Task UserData_RestoresUnlockedActiveShopSongAndTone()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.ToneFlg = GreenProtocolBytes.CreateFixedBitset([0, 4], GreenProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenShopItemStates.AddRange(
            Unlocked(1, 2, 1, 101),
            Unlocked(1, 2, 2, 4));
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.True(HasBit(response.ReleaseSongFlg, 101));
        Assert.True(HasBit(response.ToneFlg, 4));
    }

    [Fact]
    public async Task Baid_LocksActiveShopCostumesUntilUnlocked()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 1, AccessCode = "abc" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.CostumeFlg2 = GreenProtocolBytes.CreateFixedBitset([0, 117], GreenProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg3 = GreenProtocolBytes.CreateFixedBitset([0, 146], GreenProtocolBytes.CostumeFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Green, "abc"), CancellationToken.None);

        Assert.False(HasBit(response.CostumeFlg2!, 117));
        Assert.False(HasBit(response.CostumeFlg3!, 146));
    }

    [Fact]
    public async Task Baid_RestoresUnlockedActiveShopCostumes()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 1, AccessCode = "abc" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.CostumeFlg2 = GreenProtocolBytes.CreateFixedBitset([0, 117], GreenProtocolBytes.CostumeFlagBytes);
        save.CostumeFlg3 = GreenProtocolBytes.CreateFixedBitset([0, 146], GreenProtocolBytes.CostumeFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenShopItemStates.AddRange(
            Unlocked(1, 2, 5, 117),
            Unlocked(1, 2, 4, 146));
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Green, "abc"), CancellationToken.None);

        Assert.True(HasBit(response.CostumeFlg2!, 117));
        Assert.True(HasBit(response.CostumeFlg3!, 146));
    }

    private static GreenHandlerFixture.TestGreenCatalog CreateShopCatalog()
    {
        var season = new GreenItemShopSeason
        {
            SeasonId = 2,
            VerupNo = 9,
            Telop = "Shop",
            StartDatetime = "20190314000000",
            EndDatetime = "20190626075959",
            Items =
            [
                new GreenItemShopEntry { ItemNo = 1, ItemType = 1, ItemId = 101, Price = 1300 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = 2, ItemId = 4, Price = 500 },
                new GreenItemShopEntry { ItemNo = 3, ItemType = 5, ItemId = 117, Price = 500 },
                new GreenItemShopEntry { ItemNo = 4, ItemType = 4, ItemId = 146, Price = 500 }
            ]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }

    private static GreenShopItemState Unlocked(uint baid, uint seasonId, uint itemType, uint itemId)
        => new()
        {
            Baid = baid,
            SeasonId = seasonId,
            ItemType = itemType,
            ItemId = itemId,
            ItemNo = 1,
            ItemPrice = 1,
            Status = GreenShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        };

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
```

- [ ] **Step 2: Add active-season medal test**

Append to `Tests/Green/GreenItemShopStateTests.cs`:

```csharp
[Fact]
public async Task UpdatePlayResult_WhenShopEnabled_AddsDonMedalsToActiveSeasonState()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync(CreateSingleSongShopCatalog());
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance,
        Options.Create(new ServerSettings()));

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            GetDonmedal = 25,
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 102,
                    Level = 1,
                    StageMode = 0,
                    PlayResult = 1,
                    PlayScore = 1000
                }
            ]
        }),
        CancellationToken.None);

    var state = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.Equal(1u, result);
    Assert.Equal(25u, state!.TotalGetDonmedal);
    Assert.Equal(0u, save!.TotalGetDonmedal);
}

private static GreenHandlerFixture.TestGreenCatalog CreateSingleSongShopCatalog()
{
    var season = new GreenItemShopSeason
    {
        SeasonId = 2,
        StartDatetime = "20190314000000",
        EndDatetime = "20190626075959",
        Items = [new GreenItemShopEntry { ItemNo = 1, ItemType = 1, ItemId = 101, Price = 1300 }]
    };

    return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
    {
        IsEnabled = true,
        ActiveSeasonId = 2,
        Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
    });
}
```

- [ ] **Step 3: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopLockingTests Or FullyQualifiedName~UpdatePlayResult_WhenShopEnabled_AddsDonMedalsToActiveSeasonState"
```

Expected: fails because locks and season medal accounting are not implemented.

- [ ] **Step 4: Add unlock helper**

Create `Application/Common/GreenShopUnlocks.cs`:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenShopUnlocks
{
    public static byte[] ClearBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = GreenProtocolBytes.FixedOrZero(source, byteCount);
        foreach (var id in ids)
        {
            if (id >= byteCount * 8)
            {
                continue;
            }

            result[id >> 3] &= (byte)~(1 << ((int)id & 7));
        }

        return result;
    }

    public static byte[] SetBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = GreenProtocolBytes.FixedOrZero(source, byteCount);
        foreach (var id in ids)
        {
            if (id >= byteCount * 8)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    public static bool HasBit(byte[] source, uint id, int byteCount)
    {
        if (id >= byteCount * 8)
        {
            return false;
        }

        var fixedBytes = GreenProtocolBytes.FixedOrZero(source, byteCount);
        return (fixedBytes[id >> 3] & (1 << ((int)id & 7))) != 0;
    }
}
```

- [ ] **Step 5: Add settings to play result handler**

Modify `Application/Handlers/UpdatePlayResultCommand.cs` constructor:

```csharp
public partial class UpdatePlayResultCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<UpdatePlayResultCommandHandler> logger,
    IOptions<ServerSettings> settings)
    : IRequestHandler<UpdatePlayResultCommand, uint>
{
    private readonly ServerSettings settings = settings.Value;
```

Update every test construction of `UpdatePlayResultCommandHandler` to pass `Options.Create(new ServerSettings())`.

- [ ] **Step 6: Update Green medal accounting**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, after `var green = gameDataService.Green();`, add:

```csharp
var activeShopSeason = green.ItemShopCatalog.ActiveSeason;
var shopSeasonState = activeShopSeason is null
    ? null
    : await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeShopSeason.SeasonId, cancellationToken);
```

Replace the Don medal overflow guard with:

```csharp
var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;
if (!CanAdd(currentDonmedal, playResultData.GetDonmedal)
    || !CanAdd(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal)
    || playResultData.AryStageInfoes.Any(stage => !IsValidGreenStage(stage)))
```

Replace the Don medal write with:

```csharp
if (shopSeasonState is null)
{
    saveData.TotalGetDonmedal += playResultData.GetDonmedal;
}
else
{
    shopSeasonState.TotalGetDonmedal += playResultData.GetDonmedal;
    shopSeasonState.UpdatedAt = DateTime.UtcNow;
}
```

Keep Katsumedal and tutorial flag writes on `saveData`.

- [ ] **Step 7: Report active-season totals in BAID**

In `Application/Handlers/BaidQuery.Green.cs`, after `dispDanType`, load active shop state and unlocked shop costume identities:

```csharp
var activeShopSeason = gameDataService.Green().ItemShopCatalog.ActiveSeason;
var shopSeasonState = activeShopSeason is null
    ? null
    : await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeShopSeason.SeasonId, cancellationToken);
if (activeShopSeason is not null)
{
    await context.SaveChangesAsync(cancellationToken);
}

var unlockedShopItems = activeShopSeason is null
    ? new HashSet<(uint ItemType, uint ItemId)>()
    : await context.GreenShopItemStates
        .Where(row => row.Baid == card.Baid
            && row.SeasonId == activeShopSeason.SeasonId
            && row.Status == GreenShopItemStatus.Unlocked)
        .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
        .ToHashSetAsync(cancellationToken);

IEnumerable<uint> LockedIds(uint itemType) => activeShopSeason?.Items
    .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType, item.ItemId)))
    .Select(item => item.ItemId) ?? [];
```

Use these values in `CommonBaidResponse`:

```csharp
CostumeFlg1 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg1, LockedIds(3), GreenProtocolBytes.CostumeFlagBytes),
CostumeFlg2 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg2, LockedIds(5), GreenProtocolBytes.CostumeFlagBytes),
CostumeFlg3 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg3, LockedIds(4), GreenProtocolBytes.CostumeFlagBytes),
CostumeFlg4 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg4, LockedIds(6), GreenProtocolBytes.CostumeFlagBytes),
CostumeFlg5 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg5, LockedIds(7), GreenProtocolBytes.CostumeFlagBytes),
TotalGetDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal,
TotalUseDonmedal = shopSeasonState?.TotalUseDonmedal ?? saveData.TotalUseDonmedal,
```

- [ ] **Step 8: Apply song and tone locks in userdata**

In `Application/Handlers/UserDataQuery.Green.cs`, load unlocked active-season shop item identities:

```csharp
var activeShopSeason = green.ItemShopCatalog.ActiveSeason;
var unlockedShopItems = activeShopSeason is null
    ? new HashSet<(uint ItemType, uint ItemId)>()
    : await context.GreenShopItemStates
        .Where(row => row.Baid == request.Baid
            && row.SeasonId == activeShopSeason.SeasonId
            && row.Status == GreenShopItemStatus.Unlocked)
        .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
        .ToHashSetAsync(cancellationToken);

IEnumerable<uint> LockedIds(uint itemType) => activeShopSeason?.Items
    .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType, item.ItemId)))
    .Select(item => item.ItemId) ?? [];
```

Use locked ids when building the response. Userdata carries song and tone flags; costume flags are returned by BAID and were handled in Step 7:

```csharp
ReleaseSongFlg = GreenShopUnlocks.ClearBits(
    GreenProtocolBytes.CreateFixedBitset(
        green.MusicInfoFileOrder.Select(song => song.SongNo),
        GreenProtocolBytes.SongFlagBytes),
    LockedIds(1),
    GreenProtocolBytes.SongFlagBytes),
ToneFlg = GreenShopUnlocks.ClearBits(
    saveData.ToneFlg,
    LockedIds(2),
    GreenProtocolBytes.ToneFlagBytes),
```

- [ ] **Step 9: Run focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopLockingTests Or FullyQualifiedName~UpdatePlayResult_WhenShopEnabled_AddsDonMedalsToActiveSeasonState"
```

Expected: pass.

- [ ] **Step 10: Run wider Green tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
```

Expected: pass or only unrelated pre-existing failures. Fix constructor compile errors before continuing.

- [ ] **Step 11: Commit**

```powershell
git status --short
git add -- Application/Common/GreenShopUnlocks.cs Application/Handlers/BaidQuery.Green.cs Application/Handlers/UserDataQuery.Green.cs Application/Handlers/UpdatePlayResultCommand.cs Application/Handlers/UpdatePlayResultCommand.Green.cs Tests/Green/GreenItemShopLockingTests.cs Tests/Green/GreenItemShopStateTests.cs Tests/Green
git commit -m "Apply Green item shop locks and season medals"
```
