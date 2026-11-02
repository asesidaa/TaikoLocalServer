# 04 - Protocol Response And Initial Locks

**Goal:** Advertise the active shop season, return `getitemshopinfo` rows through the handler/mapper path, and remove shop songs from no-card default song flags.

**Files:**

- Modify: `Application/Handlers/GetInitialDataQuery.Green.cs`
- Modify: `Application/Handlers/GetItemShopInfoQuery.cs`
- Modify: `Application/Handlers/GetItemShopInfoQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`
- Create: `Tests/Green/GreenItemShopProtocolTests.cs`

## Acceptance Criteria

- [ ] `initialdatacheck.php` advertises only the active season id/version when shop is enabled.
- [ ] Shop songs are removed from no-card default song flags.
- [ ] `getitemshopinfo.php` returns season fields and ordered item rows.
- [ ] Controller no longer bypasses the handler.

## Steps

- [ ] **Step 1: Add protocol tests**

Create `Tests/Green/GreenItemShopProtocolTests.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopProtocolTests
{
    [Fact]
    public async Task InitialData_AdvertisesActiveGreenItemShopSeason()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.True(response.IsItemshop);
        var info = Assert.Single(response.AryGreenItemShopDatas);
        Assert.Equal(2u, info.InfoId);
        Assert.Equal(9u, info.VerupNo);
        Assert.False(HasBit(response.DefaultSongFlg, 101));
    }

    [Fact]
    public async Task GetItemShopInfo_ReturnsActiveSeasonRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateShopCatalog());
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var response = await handler.Handle(new GetItemShopInfoQuery(), CancellationToken.None);
        var wire = ItemShopMappers.Map(response);

        Assert.Equal(1u, wire.Result);
        Assert.Equal(2u, wire.SeasonId);
        Assert.Equal(9u, wire.VerupNo);
        Assert.Equal("Shop", wire.Telop);
        Assert.Equal("20190314000000", wire.StartDatetime);
        Assert.Equal("20190626075959", wire.EndDatetime);
        Assert.Equal(2, wire.AryItemshopDatas.Count);
        Assert.Equal(1u, wire.AryItemshopDatas[0].ItemNo);
        Assert.Equal(4u, wire.AryItemshopDatas[0].ItemType);
        Assert.Equal(117u, wire.AryItemshopDatas[0].ItemId);
        Assert.Equal(500u, wire.AryItemshopDatas[0].ItemPrice);
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
            AfterstartDays = 3,
            BeforecloseDays = 4,
            Items =
            [
                new GreenItemShopEntry { ItemNo = 1, ItemType = 4, ItemId = 117, Price = 500 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = 1, ItemId = 101, Price = 1300 }
            ]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopProtocolTests"
```

Expected: fails because initial data does not use the new catalog, item shop query is stubbed, and mapper does not map rows.

- [ ] **Step 3: Update initial data**

Modify `Application/Handlers/GetInitialDataQuery.Green.cs`:

```csharp
var activeShop = green.ItemShopCatalog.ActiveSeason;
var shopSongIds = green.ItemShopCatalog.IsEnabled && activeShop is not null
    ? activeShop.Items.Where(item => item.ItemType == 1).Select(item => item.ItemId).ToHashSet()
    : [];
var allSongs = green.MusicInfoFileOrder
    .Select(song => song.SongNo)
    .Where(songNo => !shopSongIds.Contains(songNo));
```

Set shop fields in the response:

```csharp
IsItemshop = green.ItemShopCatalog.IsEnabled && activeShop is not null && activeShop.Items.Count > 0,
AryGreenItemShopDatas = activeShop is null
    ? []
    :
    [
        new CommonInitialDataCheckResponse.InformationData
        {
            InfoId = activeShop.SeasonId,
            VerupNo = activeShop.VerupNo
        }
    ],
```

Keep the existing telop and taikojuku assignments unchanged.

- [ ] **Step 4: Inject catalog into item shop query handler**

Modify `Application/Handlers/GetItemShopInfoQuery.cs`:

```csharp
public partial class GetItemShopInfoQueryHandler(
    IGameDataCatalog gameDataService,
    ILogger<GetItemShopInfoQueryHandler> logger)
    : IRequestHandler<GetItemShopInfoQuery, CommonItemShopInfoResponse>
```

- [ ] **Step 5: Implement query**

Replace `Application/Handlers/GetItemShopInfoQuery.Green.cs` with:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetItemShopInfoQueryHandler
{
    public partial ValueTask<CommonItemShopInfoResponse> Handle(
        GetItemShopInfoQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
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
            StartDatetime = season.StartDatetime,
            EndDatetime = season.EndDatetime,
            AfterstartDays = season.AfterstartDays,
            BeforecloseDays = season.BeforecloseDays,
            AryItemshopData = season.Items
                .Select(item => new CommonItemShopInfoResponse.ItemShopData
                {
                    ItemNo = item.ItemNo,
                    ItemType = item.ItemType,
                    ItemId = item.ItemId,
                    ItemPrice = item.Price
                })
                .ToList()
        });
    }
}
```

- [ ] **Step 6: Map shop rows**

Modify `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`:

```csharp
public static GetitemshopinfoResponse Map(CommonItemShopInfoResponse common)
{
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

    return response;
}
```

- [ ] **Step 7: Route controller through Mediator**

Modify `Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs`:

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> GetItemShopInfo([FromBody] GetitemshopinfoRequest request)
{
    Logger.LogInformation("Green GetItemShopInfo request: {Request}", request.Stringify());
    var common = await Mediator.Send(new GetItemShopInfoQuery(), HttpContext.RequestAborted);
    return Ok(ItemShopMappers.Map(common));
}
```

- [ ] **Step 8: Run focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopProtocolTests"
```

Expected: pass.

- [ ] **Step 9: Commit**

```powershell
git status --short
git add -- Application/Handlers/GetInitialDataQuery.Green.cs Application/Handlers/GetItemShopInfoQuery.cs Application/Handlers/GetItemShopInfoQuery.Green.cs Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs Tests/Green/GreenItemShopProtocolTests.cs
git commit -m "Return Green item shop protocol data"
```

