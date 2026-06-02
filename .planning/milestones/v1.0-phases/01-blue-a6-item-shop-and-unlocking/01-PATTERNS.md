# Phase 01: Blue A6 Item Shop And Unlocking - Pattern Map

**Mapped:** 2026-05-29
**Files analyzed:** 35
**Analogs found:** 34 / 35

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `Host/wwwroot/data/blue/blue_item_shop_data.json` | config | file-I/O | `Host/wwwroot/data/green/green_item_shop_data.json` | exact-shape |
| `Infrastructure/GameDataCatalog/Blue/BlueRewardShopDataParser.cs` | utility | file-I/O, transform | `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs` | partial |
| `Tests/Blue/BlueRewardShopDataParserTests.cs` | test | file-I/O, transform | `Tests/Green/GreenItemShopLoaderTests.cs` | role-match |
| `Tests/Blue/BlueItemShopLoaderTests.cs` | test | file-I/O | `Tests/Green/GreenItemShopLoaderTests.cs` | exact |
| `Domain/Enums/BlueShopItemStatus.cs` | model | CRUD | `Domain/Enums/GreenShopItemStatus.cs` | exact |
| `Domain/Entities/BlueShopSeasonState.cs` | model | CRUD | `Domain/Entities/GreenShopSeasonState.cs` | exact |
| `Domain/Entities/BlueShopItemState.cs` | model | CRUD | `Domain/Entities/GreenShopItemState.cs` | exact |
| `Application/Abstractions/ITaikoDbContext.Blue.cs` | config | CRUD | `Application/Abstractions/ITaikoDbContext.Green.cs` | exact |
| `Infrastructure/Persistence/TaikoDbContext.Blue.cs` | model | CRUD | `Infrastructure/Persistence/TaikoDbContext.Green.cs` | exact |
| `Infrastructure/Persistence/Migrations/<timestamp>_AddBlueItemShopState.cs` | migration | CRUD | `Infrastructure/Persistence/Migrations/20260525210125_AddGreenItemShopState.cs` | exact |
| `Infrastructure/Persistence/Migrations/<timestamp>_AddBlueItemShopState.Designer.cs` | migration | CRUD | `Infrastructure/Persistence/Migrations/20260525210125_AddGreenItemShopState.Designer.cs` | generated |
| `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` | migration | CRUD | `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` | generated |
| `Application/Common/BlueShopStateExtensions.cs` | utility | CRUD | `Application/Common/GreenShopStateExtensions.cs` | role-match |
| `Application/Common/BlueShopUnlocks.cs` | utility | transform | `Application/Common/GreenShopUnlocks.cs` | role-match |
| `Application/Handlers/GetItemShopInfoQuery.cs` | service | request-response | `Application/Handlers/UpdatePlayResultCommand.cs` | role-match |
| `Application/Handlers/GetItemShopInfoQuery.Green.cs` | service | request-response | `Application/Handlers/GetItemShopInfoQuery.Green.cs` | existing-to-adapt |
| `Application/Handlers/GetItemShopInfoQuery.Blue.cs` | service | request-response | `Application/Handlers/GetItemShopInfoQuery.Green.cs` | exact-shape |
| `Application/Handlers/ItemPurchaseCommand.cs` | service | request-response, CRUD | `Application/Handlers/UpdatePlayResultCommand.cs` | role-match |
| `Application/Handlers/ItemPurchaseCommand.Green.cs` | service | request-response, CRUD | `Application/Handlers/ItemPurchaseCommand.Green.cs` | existing-to-adapt |
| `Application/Handlers/ItemPurchaseCommand.Blue.cs` | service | request-response, CRUD | `Application/Handlers/ItemPurchaseCommand.Green.cs` | exact-shape |
| `Application/Handlers/GetInitialDataQuery.Blue.cs` | service | request-response | `Application/Handlers/GetInitialDataQuery.Green.cs` | exact-shape |
| `Application/Handlers/BaidQuery.Blue.cs` | service | request-response, CRUD | `Application/Handlers/BaidQuery.Green.cs` | exact-shape |
| `Application/Handlers/UserDataQuery.Blue.cs` | service | request-response, CRUD | `Application/Handlers/UserDataQuery.Green.cs` | exact-shape |
| `Application/Handlers/UpdatePlayResultCommand.Blue.cs` | service | request-response, CRUD | `Application/Handlers/UpdatePlayResultCommand.Green.cs` | role-match |
| `Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs` | utility | transform | `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs` | exact-shape |
| `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs` | utility | transform | `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs` | existing-to-adapt |
| `Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs` | controller | request-response | `Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs` | exact |
| `Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs` | controller | request-response | `Adapters.GameProtocol.Green/Controllers/ItemPurchaseController.cs` | exact |
| `Adapters.GameProtocol.Blue/Controllers/RewardExecutionController.cs` | controller | request-response | current Blue stub | exact-retain |
| `Tests/Blue/BlueItemShopProtocolTests.cs` | test | request-response, transform | `Tests/Green/GreenItemShopProtocolTests.cs` | exact-shape |
| `Tests/Blue/BlueItemShopPurchaseTests.cs` | test | request-response, CRUD | `Tests/Green/GreenItemShopPurchaseTests.cs` | exact-shape |
| `Tests/Blue/BlueItemShopStateTests.cs` | test | CRUD | `Tests/Green/GreenItemShopStateTests.cs` | role-match |
| `Tests/Blue/BlueItemShopLockingTests.cs` | test | request-response, CRUD | `Tests/Green/GreenItemShopLockingTests.cs` | exact-shape |
| `Tests/Blue/BlueA6SourceGuardTests.cs` | test | batch/static-analysis | `Tests/Blue/BlueA3SourceGuardTests.cs` | exact-shape |
| `Tests/Blue/BlueHandlerFixture.cs` | test utility | CRUD, request-response | current Blue fixture | exact |

## Pattern Assignments

### Blue default data and catalog parser

**Applies to:** `Host/wwwroot/data/blue/blue_item_shop_data.json`, `Infrastructure/GameDataCatalog/Blue/BlueRewardShopDataParser.cs`, `Tests/Blue/BlueRewardShopDataParserTests.cs`, `Tests/Blue/BlueItemShopLoaderTests.cs`

**Analog:** `Host/wwwroot/data/green/green_item_shop_data.json`

**JSON shape** (lines 1-16):
```json
{
  "seasons": [
    {
      "season_id": 1,
      "verup_no": 1,
      "telop": "Spring reward shop",
      "start_datetime": "20190314000000",
      "end_datetime": "20190529235959",
      "afterstart_days": 7,
      "beforeclose_days": 7,
      "items": [
        { "item_type": 1, "item_id": 799, "item_price": 1300 }
      ]
    }
  ]
}
```

**Blue loader entry point** from `Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs` (lines 11-25):
```csharp
public const string FileName = "blue_item_shop_data.json";

var path = Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName);
var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
    path,
    blueSettings.EnableShop == true,
    blueSettings.ActiveShopSeasonId,
    nameof(GameEra.Blue),
    cancellationToken);
```

**Validation pattern** from `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs` (lines 25-38, 77-151):
```csharp
if (!isEnabled)
{
    return Ac15ItemShopCatalog.Disabled;
}

if (!File.Exists(path))
{
    throw new InvalidDataException($"{eraName} item shop is enabled but data file was not found: {path}");
}

if (raw.ItemType is < 1 or > 7)
{
    throw new InvalidDataException($"{eraName} item shop season {seasonId} item {itemNo} has unsupported item_type {raw.ItemType}.");
}
```

**Loader tests** from `Tests/Green/GreenItemShopLoaderTests.cs` (lines 20-61, 93-194):
```csharp
var catalog = await GreenItemShopLoader.LoadFromFileAsync(
    path,
    new EraSettings { EnableShop = true, ActiveShopSeasonId = 2 },
    CancellationToken.None);

Assert.True(catalog.IsEnabled);
Assert.Equal(2u, catalog.ActiveSeason!.SeasonId);
Assert.Equal(1u, catalog.ActiveSeason.Items[0].ItemNo);
```

**Planner note:** The parser has no exact in-repo analog. Use the AC15 loader's fail-fast style, but the parser must prove the local `H:\taiko\blue\rewardshopdata.bin` rows, season envelope, item type, item id, price, and catalog resolution before committing `blue_item_shop_data.json`. Do not commit the binary.

---

### Blue-owned persistence

**Applies to:** `Domain/Enums/BlueShopItemStatus.cs`, `Domain/Entities/BlueShopSeasonState.cs`, `Domain/Entities/BlueShopItemState.cs`, `Application/Abstractions/ITaikoDbContext.Blue.cs`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, migration files

**Analogs:** `Domain/Entities/GreenShopSeasonState.cs`, `Domain/Entities/GreenShopItemState.cs`, `Infrastructure/Persistence/TaikoDbContext.Green.cs`

**Season entity pattern** (lines 3-18):
```csharp
public sealed class GreenShopSeasonState
{
    public uint Baid { get; set; }
    public uint SeasonId { get; set; }
    public uint TotalGetDonmedal { get; set; }
    public uint TotalUseDonmedal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDatum? Ba { get; set; }
}
```

**Item entity pattern** from `Domain/Entities/GreenShopItemState.cs` (lines 5-25):
```csharp
public sealed class GreenShopItemState
{
    public uint Baid { get; set; }
    public uint SeasonId { get; set; }
    public uint ItemType { get; set; }
    public uint ItemId { get; set; }
    public uint ItemNo { get; set; }
    public uint ItemPrice { get; set; }
    public GreenShopItemStatus Status { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public UserDatum? Ba { get; set; }
}
```

**Status enum pattern** from `Domain/Enums/GreenShopItemStatus.cs` (lines 3-6):
```csharp
public enum GreenShopItemStatus : uint
{
    Unlocked = 2
}
```

**DbSet pattern** from `Application/Abstractions/ITaikoDbContext.Green.cs` (lines 16-17):
```csharp
DbSet<GreenShopSeasonState> GreenShopSeasonStates { get; }
DbSet<GreenShopItemState> GreenShopItemStates { get; }
```

**EF mapping pattern** from `Infrastructure/Persistence/TaikoDbContext.Green.cs` (lines 162-187):
```csharp
modelBuilder.Entity<GreenShopSeasonState>(entity =>
{
    entity.ToTable("GreenShopSeasonStates");
    entity.HasKey(e => new { e.Baid, e.SeasonId });
    entity.Property(e => e.CreatedAt).HasColumnType("datetime");
    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
    entity.HasOne(d => d.Ba)
        .WithMany()
        .HasPrincipalKey(p => p.Baid)
        .HasForeignKey(d => d.Baid)
        .OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<GreenShopItemState>(entity =>
{
    entity.ToTable("GreenShopItemStates");
    entity.HasKey(e => new { e.Baid, e.SeasonId, e.ItemType, e.ItemId });
    entity.Property(e => e.Status).HasConversion<uint>();
});
```

**Migration pattern** from `20260525210125_AddGreenItemShopState.cs` (lines 14-59):
```csharp
migrationBuilder.CreateTable(
    name: "GreenShopItemStates",
    columns: table => new
    {
        Baid = table.Column<uint>(type: "INTEGER", nullable: false),
        SeasonId = table.Column<uint>(type: "INTEGER", nullable: false),
        ItemType = table.Column<uint>(type: "INTEGER", nullable: false),
        ItemId = table.Column<uint>(type: "INTEGER", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_GreenShopItemStates", x => new { x.Baid, x.SeasonId, x.ItemType, x.ItemId });
    });
```

**Planner note:** Copy shape only. Names must be `BlueShopSeasonState`, `BlueShopItemState`, `BlueShopItemStatus`, `BlueShopSeasonStates`, and `BlueShopItemStates`. Do not reference Green shop entities, Green enum, or Green DbSets from Blue files.

---

### Blue shop state and unlock helpers

**Applies to:** `Application/Common/BlueShopStateExtensions.cs`, `Application/Common/BlueShopUnlocks.cs`

**Analogs:** `Application/Common/GreenShopStateExtensions.cs`, `Application/Common/GreenShopUnlocks.cs`, `Application/Common/BlueProtocolBytes.cs`

**State helper shape** from `GreenShopStateExtensions.cs` (lines 5-31):
```csharp
public static async ValueTask<GreenShopSeasonState> GetOrCreateGreenShopSeasonStateAsync(
    this ITaikoDbContext context,
    UserSaveDataGreen saveData,
    uint seasonId,
    CancellationToken cancellationToken = default)
{
    var existing = await context.GreenShopSeasonStates.FindAsync([saveData.Baid, seasonId], cancellationToken);
    if (existing is not null)
    {
        return existing;
    }

    var now = DateTime.UtcNow;
    var state = new GreenShopSeasonState
    {
        Baid = saveData.Baid,
        SeasonId = seasonId,
        CreatedAt = now,
        UpdatedAt = now
    };
}
```

**Important Blue override:** Do not copy Green's first-season seeding from lines 17-25. Blue state always starts with `TotalGetDonmedal = 0` and `TotalUseDonmedal = 0`.

**Unlock helper shape** from `GreenShopUnlocks.cs` (lines 5-47):
```csharp
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
```

**Blue byte widths** from `BlueProtocolBytes.cs` (lines 5-24):
```csharp
public const int SongFlagBytes = 128;
public const int ToneFlagBytes = 16;
public const int TitleFlagBytes = 128;
public const int CostumeFlagBytes = 32;

public static byte[] FixedOrZero(byte[]? source, int byteCount)
{
    return BitsetCodec.Normalize(source, byteCount);
}
```

**Planner note:** `BlueShopUnlocks` should be structurally identical to `GreenShopUnlocks` but must call `BlueProtocolBytes.FixedOrZero`.

---

### Era-aware shop commands and queries

**Applies to:** `Application/Handlers/GetItemShopInfoQuery.cs`, `Application/Handlers/GetItemShopInfoQuery.Green.cs`, `Application/Handlers/GetItemShopInfoQuery.Blue.cs`, `Application/Handlers/ItemPurchaseCommand.cs`, `Application/Handlers/ItemPurchaseCommand.Green.cs`, `Application/Handlers/ItemPurchaseCommand.Blue.cs`

**Analog:** `Application/Handlers/UpdatePlayResultCommand.cs`

**Era dispatch pattern** (lines 6-27):
```csharp
public readonly record struct UpdatePlayResultCommand(uint Baid, GameEra Era, CommonPlayResultData PlayResultData) : IRequest<uint>;

public ValueTask<uint> Handle(UpdatePlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};
```

**Current command shape to adapt** from `ItemPurchaseCommand.cs` (lines 3-17):
```csharp
public readonly record struct ItemPurchaseCommand(
    uint Baid,
    uint ItemNo,
    uint? ItemType,
    uint? ItemId,
    uint? ItemPrice
) : IRequest<CommonItemPurchaseResponse>;

public partial ValueTask<CommonItemPurchaseResponse> Handle(ItemPurchaseCommand request, CancellationToken cancellationToken);
```

**Green purchase core** from `ItemPurchaseCommand.Green.cs` (lines 18-71):
```csharp
var seasonState = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeSeason.SeasonId, cancellationToken);
if (IsPreflight(request))
{
    await context.SaveChangesAsync(cancellationToken);
    return Success(seasonState);
}

if (!activeSeason.ItemsByNo.TryGetValue(request.ItemNo, out var item)
    || request.ItemType != item.ItemType
    || request.ItemId != item.ItemId
    || request.ItemPrice != item.Price
    || item.Price == 0)
{
    return Failure(seasonState);
}

var existingItem = await context.GreenShopItemStates.FindAsync(
    [request.Baid, activeSeason.SeasonId, item.ItemType, item.ItemId],
    cancellationToken);
```

**Green unlock switch** from `ItemPurchaseCommand.Green.cs` (lines 83-109):
```csharp
switch (item.ItemType)
{
    case 2:
        saveData.ToneFlg = GreenShopUnlocks.SetBits(saveData.ToneFlg, [item.ItemId], GreenProtocolBytes.ToneFlagBytes);
        return;
    case 3:
        saveData.CostumeFlg1 = GreenShopUnlocks.SetBits(saveData.CostumeFlg1, [item.ItemId], GreenProtocolBytes.CostumeFlagBytes);
        return;
    case 4:
        saveData.CostumeFlg3 = GreenShopUnlocks.SetBits(saveData.CostumeFlg3, [item.ItemId], GreenProtocolBytes.CostumeFlagBytes);
        return;
}
```

**Blue-specific unlock mapping:** copy the switch shape, but implement all phase item types with Blue fields and widths:
`1 -> ReleaseSongFlg`, `2 -> ToneFlg`, `3 -> CostumeFlg1`, `4 -> CostumeFlg3`, `5 -> CostumeFlg2`, `6 -> CostumeFlg4`, `7 -> CostumeFlg5`.

**Shop-info query core** from `GetItemShopInfoQuery.Green.cs` (lines 5-34):
```csharp
var season = gameDataService.Green().ItemShopCatalog.ActiveSeason;
if (season is null)
{
    logger.LogInformation("Green GetItemShopInfo returning empty because item shop is disabled");
    return ValueTask.FromResult(new CommonItemShopInfoResponse { Result = 1 });
}

return ValueTask.FromResult(new CommonItemShopInfoResponse
{
    Result = 1,
    VerupNo = season.VerupNo,
    SeasonId = season.SeasonId,
    Telop = season.Telop,
    AryItemshopData = season.Items.Select(item => new CommonItemShopInfoResponse.ItemShopData
    {
        ItemNo = item.ItemNo,
        ItemType = item.ItemType,
        ItemId = item.ItemId,
        ItemPrice = item.Price
    }).ToList()
});
```

**Planner note:** Add `GameEra Era` to both request records and update Green mapper calls with `GameEra.Green`; Blue mapper calls must pass `GameEra.Blue`.

---

### Blue protocol controllers and item-shop mapper

**Applies to:** `Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs`, `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`, `Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs`, `Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs`, `Adapters.GameProtocol.Blue/Controllers/RewardExecutionController.cs`

**Analog:** `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`

**Optional request field mapping** (lines 8-15):
```csharp
return new ItemPurchaseCommand(
    request.Baid,
    request.ItemNo,
    request.ShouldSerializeItemType() ? request.ItemType : null,
    request.ShouldSerializeItemId() ? request.ItemId : null,
    request.ShouldSerializeItemPrice() ? request.ItemPrice : null);
```

**Response row mapping** (lines 18-40):
```csharp
var response = new GetitemshopinfoResponse
{
    Result = common.Result,
    VerupNo = common.VerupNo,
    SeasonId = common.SeasonId,
    Telop = common.Telop,
    StartDatetime = common.StartDatetime,
    EndDatetime = common.EndDatetime,
    AfterstartDays = common.AfterstartDays,
    BeforecloseDays = common.BeforecloseDays
};

response.AryItemshopDatas.AddRange(common.AryItemshopData.Select(item => new GetitemshopinfoResponse.ItemshopData
{
    ItemNo = item.ItemNo,
    ItemType = item.ItemType,
    ItemId = item.ItemId,
    ItemPrice = item.ItemPrice
}));
```

**Controller pattern** from Green controllers (GetItemShopInfo lines 7-14; ItemPurchase lines 7-14):
```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> ItemPurchase([FromBody] ItempurchaseRequest request)
{
    Logger.LogInformation("Green ItemPurchase request: {Request}", request.Stringify());
    var common = await Mediator.Send(ItemShopMappers.Map(request), HttpContext.RequestAborted);
    return Ok(ItemShopMappers.Map(common));
}
```

**Blue stubs to replace** from current Blue controllers (GetItemShopInfo lines 9-13; ItemPurchase lines 9-13):
```csharp
Logger.LogInformation("Blue GetItemShopInfo request: {Request}", request.Stringify());
return Ok(new GetitemshopinfoResponse { Result = 1 });
```

**Reward execution no-op to preserve** from `RewardExecutionController.cs` (lines 9-13):
```csharp
public IActionResult RewardExecution([FromBody] RewardexecutionRequest request)
{
    Logger.LogInformation("Blue RewardExecution request: {Request}", request.Stringify());
    return Ok(new RewardexecutionResponse { Result = 1 });
}
```

**Blue wire contract excerpts** from `Adapters.GameProtocol.Blue/Wire/Game.cs`:
```csharp
// lines 700-720
public List<ItemshopData> AryItemshopDatas { get; } = new();
public uint ItemNo { get; set; }
public uint ItemType { get; set; }
public uint ItemId { get; set; }
public uint ItemPrice { get; set; }

// lines 3263-3289
public bool ShouldSerializeItemType() => __pbn__ItemType != null;
public bool ShouldSerializeItemId() => __pbn__ItemId != null;
public bool ShouldSerializeItemPrice() => __pbn__ItemPrice != null;

// lines 3302-3323
public uint Result { get; set; }
public uint TotalGetDonmedal { get; set; }
public uint TotalUseDonmedal { get; set; }
```

**Planner note:** Do not import `Adapters.GameProtocol.Green` from Blue mapper/controller files. Add `Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs` with Blue wire namespaces.

---

### Blue initialdata, BAID, userdata, and playresult locking

**Applies to:** `Application/Handlers/GetInitialDataQuery.Blue.cs`, `Application/Handlers/BaidQuery.Blue.cs`, `Application/Handlers/UserDataQuery.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`

**Analogs:** `Application/Handlers/GetInitialDataQuery.Green.cs`, `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/UserDataQuery.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`

**Existing Blue initialdata shop advertisement** from `GetInitialDataQuery.Blue.cs` (lines 9-28):
```csharp
var activeShop = blue.ItemShopCatalog.ActiveSeason;
var shopSongIds = blue.ItemShopCatalog.IsEnabled && activeShop is not null
    ? activeShop.Items.Where(item => item.ItemType == 1).Select(item => item.ItemId).ToHashSet()
    : [];

DefaultSongFlg = BlueProtocolBytes.CreateFixedBitset(allSongs, BlueProtocolBytes.SongFlagBytes),
IsItemshop = blue.ItemShopCatalog.IsEnabled && activeShop is not null && activeShop.Items.Count > 0,
AryBlueItemShopDatas = activeShop is null ? [] : [new() { InfoId = activeShop.SeasonId, VerupNo = activeShop.VerupNo }]
```

**BAID season totals and costume locks** from `BaidQuery.Green.cs` (lines 44-85):
```csharp
var activeShopSeason = gameDataService.Green().ItemShopCatalog.ActiveSeason;
var shopSeasonState = activeShopSeason is null
    ? null
    : await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeShopSeason.SeasonId, cancellationToken);

IEnumerable<uint> LockedIds(uint itemType) => activeShopSeason?.Items
    .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType, item.ItemId)))
    .Select(item => item.ItemId) ?? [];

CostumeFlg1 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg1, LockedIds(3), GreenProtocolBytes.CostumeFlagBytes),
TotalGetDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal,
TotalUseDonmedal = shopSeasonState?.TotalUseDonmedal ?? saveData.TotalUseDonmedal,
```

**Userdata song/tone locks** from `UserDataQuery.Green.cs` (lines 13-25, 42-56):
```csharp
var unlockedShopItems = activeShopSeason is null
    ? new HashSet<(uint ItemType, uint ItemId)>()
    : await context.GreenShopItemStates
        .Where(row => row.Baid == request.Baid
            && row.SeasonId == activeShopSeason.SeasonId
            && row.Status == GreenShopItemStatus.Unlocked)
        .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
        .ToHashSetAsync(cancellationToken);

ReleaseSongFlg = GreenShopUnlocks.ClearBits(
    GreenProtocolBytes.CreateFixedBitset(green.MusicInfoFileOrder.Select(song => song.SongNo), GreenProtocolBytes.SongFlagBytes),
    LockedIds(1),
    GreenProtocolBytes.SongFlagBytes),
ToneFlg = GreenShopUnlocks.ClearBits(saveData.ToneFlg, LockedIds(2), GreenProtocolBytes.ToneFlagBytes),
```

**Playresult active-season medals** from `UpdatePlayResultCommand.Green.cs` (lines 31-58):
```csharp
var activeShopSeason = green.ItemShopCatalog.ActiveSeason;
var shopSeasonState = activeShopSeason is null
    ? null
    : await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeShopSeason.SeasonId, cancellationToken);

var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;

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

**Current Blue surfaces to adapt**:
```csharp
// UpdatePlayResultCommand.Blue.cs lines 61-63
saveData.TotalGetDonmedal += playResultData.GetDonmedal;
saveData.TotalGetKatsumedal += playResultData.GetKatsumedal;

// UserDataQuery.Blue.cs lines 35-39
ReleaseSongFlg = BlueProtocolBytes.OrBitsets(catalogReleaseFlags, saveData.ReleaseSongFlg, BlueProtocolBytes.SongFlagBytes),
ToneFlg = BlueProtocolBytes.FixedOrZero(saveData.ToneFlg, BlueProtocolBytes.ToneFlagBytes),

// BaidQuery.Blue.cs lines 53-59
CostumeFlg1 = BlueProtocolBytes.FixedOrZero(saveData.CostumeFlg1, BlueProtocolBytes.CostumeFlagBytes),
TotalGetDonmedal = saveData.TotalGetDonmedal,
TotalUseDonmedal = saveData.TotalUseDonmedal,
```

**Planner note:** Blue behavior differs from Green when shop is disabled: do not create or update Blue shop state; BAID and itempurchase report `0/0` when disabled. When enabled, playresult Don medals go only to active Blue shop season state, not `UserSaveDataBlue.TotalGetDonmedal`.

---

### Blue tests and source guards

**Applies to:** `Tests/Blue/BlueItemShopProtocolTests.cs`, `Tests/Blue/BlueItemShopPurchaseTests.cs`, `Tests/Blue/BlueItemShopStateTests.cs`, `Tests/Blue/BlueItemShopLockingTests.cs`, `Tests/Blue/BlueA6SourceGuardTests.cs`, `Tests/Blue/BlueHandlerFixture.cs`

**Analogs:** `Tests/Green/GreenItemShop*.cs`, `Tests/Blue/BlueA3SourceGuardTests.cs`, `Tests/Blue/BlueHandlerFixture.cs`

**Fixture shape** from `Tests/Blue/BlueHandlerFixture.cs` (lines 22-34, 48-62):
```csharp
public static async Task<BlueHandlerFixture> CreateAsync(IBlueCatalog? blueCatalog = null)
{
    var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();
    var context = new TaikoDbContext(options);
    await context.Database.EnsureCreatedAsync();
    var catalog = new FileGameDataCatalog([blueCatalog ?? new TestBlueCatalog()]);
    return new BlueHandlerFixture(connection, context, catalog);
}

public TestBlueCatalog(..., BlueItemShopCatalog? itemShopCatalog = null, ...)
{
    ItemShopCatalog = itemShopCatalog ?? BlueItemShopCatalog.Disabled;
    ItemShop = ItemShopCatalog.ActiveItemsByNo;
}
```

**Purchase test pattern** from `GreenItemShopPurchaseTests.cs` (lines 6-30, 32-64, 90-151):
```csharp
var response = await handler.Handle(new ItemPurchaseCommand(1, 2, 1, 865, 300), CancellationToken.None);

var season = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
var item = await fixture.Context.GreenShopItemStates.FindAsync(1u, 2u, 1u, 865u);
Assert.Equal(1u, response.Result);
Assert.Equal(300u, response.TotalUseDonmedal);
Assert.Equal(GreenShopItemStatus.Unlocked, item!.Status);

var preflight = await handler.Handle(new ItemPurchaseCommand(1, 0, null, null, null), CancellationToken.None);
Assert.False(await fixture.Context.GreenShopItemStates.AnyAsync());
```

**Locking test pattern** from `GreenItemShopLockingTests.cs` (lines 6-25, 52-99):
```csharp
var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);
Assert.False(HasBit(response.ReleaseSongFlg, 101));
Assert.False(HasBit(response.ToneFlg, 4));

var baid = await handler.Handle(new BaidQuery(GameEra.Green, "abc"), CancellationToken.None);
Assert.False(HasBit(baid.CostumeFlg2!, 117));
Assert.False(HasBit(baid.CostumeFlg3!, 146));
```

**State test pattern, with Blue override** from `GreenItemShopStateTests.cs` (lines 52-91):
```csharp
var result = await handler.Handle(new UpdatePlayResultCommand(
    1,
    GameEra.Green,
    new CommonPlayResultData { Baid = 1, GetDonmedal = 25, AryStageInfoes = [...] }),
    CancellationToken.None);

var state = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
Assert.Equal(25u, state!.TotalGetDonmedal);
Assert.Equal(0u, save!.TotalGetDonmedal);
```

**Protocol test pattern** from `GreenItemShopProtocolTests.cs` (lines 9-68):
```csharp
Assert.True(response.IsItemshop);
var info = Assert.Single(response.AryGreenItemShopDatas);
Assert.Equal(2u, info.InfoId);
Assert.False(HasBit(response.DefaultSongFlg, 101));

var wire = ItemShopMappers.Map(response);
Assert.Equal(2u, wire.SeasonId);
Assert.Equal(1u, wire.AryItemshopDatas[0].ItemNo);

Assert.Null(command.ItemType);
Assert.Null(command.ItemId);
Assert.Null(command.ItemPrice);
```

**Source guard pattern** from `BlueA3SourceGuardTests.cs` (lines 19-28) and `BlueA4SourceGuardTests.cs` (lines 44-49):
```csharp
foreach (var file in files)
{
    var source = File.ReadAllText(file);
    Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
    Assert.DoesNotContain("Adapters.GameProtocol.Green", source, StringComparison.Ordinal);
    Assert.DoesNotContain("UserSaveDataGreen", source, StringComparison.Ordinal);
    Assert.DoesNotContain("GreenShop", source, StringComparison.Ordinal);
}
```

**Planner note:** Add guards over new Blue shop handlers, helpers, entities, mapper, controllers, and tests. Include `GreenShop`, `GreenShopItemStatus`, `GreenProtocolBytes`, `Adapters.GameProtocol.Green`, `UserSaveDataGreen`, and Green shop DbSet names.

## Shared Patterns

### Era Dispatch

**Source:** `Application/Handlers/UpdatePlayResultCommand.cs` lines 6-27
**Apply to:** `GetItemShopInfoQuery.cs`, `ItemPurchaseCommand.cs`

Add `GameEra Era` to the request record and route through `HandleGreen` / `HandleBlue` partial methods. Throw `InvalidOperationException` for unsupported eras.

### Blue State Separation

**Source:** `Infrastructure/Persistence/TaikoDbContext.Blue.cs` lines 17-109 and `TaikoDbContext.Green.cs` lines 162-187
**Apply to:** Blue entities, DbSets, EF mappings, migration, handler queries

Use Blue table/entity names and the existing Blue persistence partial. Do not read or write `GreenShopSeasonStates` or `GreenShopItemStates` from Blue code.

### Optional Protobuf Fields

**Source:** `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs` lines 8-15 and `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3263-3289
**Apply to:** `Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs`, protocol tests

Map `item_type`, `item_id`, and `item_price` as nullable command values using `ShouldSerialize*()` so `item_no == 0` preflight can distinguish omitted fields from zero values.

### Shop Locking

**Source:** `Application/Handlers/UserDataQuery.Green.cs` lines 13-56 and `Application/Handlers/BaidQuery.Green.cs` lines 53-85
**Apply to:** Blue BAID/userdata/initialdata/readback tests

Build active-season locked item ids from the Blue catalog and Blue purchased item state; clear locked song, tone, and costume bits before returning response DTOs.

### RewardExecution Phase Rule

**Source:** `Adapters.GameProtocol.Blue/Controllers/RewardExecutionController.cs` lines 9-13
**Apply to:** `RewardExecutionController.cs`, protocol tests/source guard

Keep log-and-success behavior for Phase 1. Do not mutate shop state or save state from `rewardexecution.php`.

### Fail-Fast Catalog/Data Validation

**Source:** `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs` lines 30-67, 77-151
**Apply to:** Blue loader/default parser/tests

Enabled Blue shop must fail on missing `blue_item_shop_data.json`, missing/unknown active season, invalid date fields, empty item lists, duplicate item identities, unsupported item types, zero item ids, and zero prices.

## No Analog Found

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| `Infrastructure/GameDataCatalog/Blue/BlueRewardShopDataParser.cs` | utility | file-I/O, transform | No existing parser reads a Boost-style `rewardshopdata.bin`; use AC15 loader validation and write parser tests from local evidence before committing JSON. |

## Metadata

**Analog search scope:** `Application/Handlers`, `Application/Common`, `Application/Catalog`, `Adapters.GameProtocol.Blue`, `Adapters.GameProtocol.Green`, `Domain/Entities`, `Domain/Enums`, `Infrastructure/GameDataCatalog`, `Infrastructure/Persistence`, `Tests/Blue`, `Tests/Green`, `Host/wwwroot/data`.

**Files scanned:** 65+ targeted files via `rg --files`, `rg`, and line-numbered `Get-Content`.

**Pattern extraction date:** 2026-05-29

**Project-skill note:** `.codex/skills` contains GSD workflow skills; no phase-specific `rules/*.md` were found in the project instructions. Pattern choices follow `AGENTS.md`, `01-CONTEXT.md`, and `01-RESEARCH.md`.
