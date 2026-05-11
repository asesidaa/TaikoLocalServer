# 06 - Green Ghost, Reward, And Shop Endpoints

**Surface:** Persist fields that round-trip through Green ghost/reward/shop endpoints and replace success-only stubs with deterministic behavior.

**Why after 05:** Ghost section stats are uploaded by play result and read by ghost endpoints.

**Files:**
- Modify: `Application/Handlers/GetGhostDataQuery.Green.cs`
- Modify: `Application/Handlers/GetGhostScoreQuery.Green.cs`
- Modify: `Application/Handlers/RewardExecutionCommand.Green.cs`
- Modify: `Application/Handlers/RewardCardCheckQuery.Green.cs`
- Modify: `Application/Handlers/ItemPurchaseCommand.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/GetGhostDataController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/GetGhostScoreController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/RewardExecutionController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/RewardCardCheckController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/ItemPurchaseController.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/GhostMappers.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`
- Create: `Tests/Green/GreenGhostRewardTests.cs`

---

## Task 06.1: Implement Ghost Data Readback

**Acceptance Criteria:**
- [ ] `getghostdata` returns saved flags, perf, rank, winnings, and tokens.
- [ ] Fixed byte fields are returned at IDA-confirmed widths.

**Steps:**

- [ ] **Step 1: Add test**

Create `Tests/Green/GreenGhostRewardTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaikoLocalServer.Application.Common;
using TaikoLocalServer.Application.Handlers;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenGhostRewardTests
{
    [Fact]
    public async Task GetGhostData_ReturnsSavedGreenGhostState()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.GhostInputMedian = -12;
        save.GhostInputVariance = 34;
        save.GhostRankId = 5;
        save.GhostWinPoint = 6;
        save.GhostCertifiedLevelId = 7;
        save.GhostTotalWinnings = 8;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenGhostTokens.Add(new GreenGhostTokens { Baid = 1, TokenId = 9, TokenValue = 10 });
        fixture.Context.GreenGhostWinnings.Add(new GreenGhostWinnings { Baid = 1, LevelId = 11, Winnings = 12 });
        await fixture.Context.SaveChangesAsync();

        var handler = new GetGhostDataQueryHandler(
            fixture.Context,
            NullLogger<GetGhostDataQueryHandler>.Instance);

        var response = await handler.Handle(new GetGhostDataQuery(1), CancellationToken.None);

        Assert.Equal(GreenProtocolBytes.GhostReleaseInfoBytes, response.ReleaseInfoFlag.Length);
        Assert.Equal(GreenProtocolBytes.GhostPlayedSongBytes, response.PlayedSongFlag.Length);
        Assert.Equal((uint)8, response.TotalWinnings);
        Assert.Single(response.AryTokenData);
        Assert.Single(response.GhostRecordData.AryWinningsData);
    }
}
```

Before adding this test, extract the fixture from `GreenIdentityHandlerTests` into `Tests/Green/GreenHandlerFixture.cs` and make it `internal sealed class GreenHandlerFixture`. Keep `TestGreenCatalog` in the same file so all Green handler tests share the same catalog and SQLite setup.

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter GetGhostData_ReturnsSavedGreenGhostState`

Expected: FAIL because handler is still empty.

- [ ] **Step 3: Implement `GetGhostDataQuery.Green.cs`**

Update `GetGhostDataQuery.cs` so the primary constructor injects `ITaikoDbContext`:

```csharp
public partial class GetGhostDataQueryHandler(
    ITaikoDbContext context,
    ILogger<GetGhostDataQueryHandler> logger)
    : IRequestHandler<GetGhostDataQuery, CommonGhostDataResponse>
```

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetGhostDataQueryHandler
{
    public partial async ValueTask<CommonGhostDataResponse> Handle(
        GetGhostDataQuery request,
        CancellationToken cancellationToken)
    {
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var tokens = await context.GreenGhostTokens
            .Where(row => row.Baid == request.Baid)
            .Select(row => new CommonGhostDataResponse.GhostTokenData
            {
                TokenId = row.TokenId,
                TokenValue = row.TokenValue
            })
            .ToListAsync(cancellationToken);
        var winnings = await context.GreenGhostWinnings
            .Where(row => row.Baid == request.Baid)
            .Select(row => new CommonGhostDataResponse.GhostWinningsData
            {
                LevelId = row.LevelId,
                Winnings = row.Winnings
            })
            .ToListAsync(cancellationToken);

        return new CommonGhostDataResponse
        {
            Result = 1,
            ReleaseInfoFlag = GreenProtocolBytes.FixedOrZero(saveData.GhostReleaseInfoFlag, GreenProtocolBytes.GhostReleaseInfoBytes),
            PlayedSongFlag = GreenProtocolBytes.FixedOrZero(saveData.GhostPlayedSongFlag, GreenProtocolBytes.GhostPlayedSongBytes),
            TotalWinnings = saveData.GhostTotalWinnings,
            GhostPerfData = new CommonGhostDataResponse.GhostPerfDataInfo
            {
                InputMedian = saveData.GhostInputMedian,
                InputVariance = saveData.GhostInputVariance
            },
            GhostRecordData = new CommonGhostDataResponse.GhostRankData
            {
                RankId = saveData.GhostRankId,
                WinPoint = saveData.GhostWinPoint,
                CertifiedLevelId = saveData.GhostCertifiedLevelId,
                AryWinningsData = winnings
            },
            AryTokenData = tokens
        };
    }
}
```

`CommonGhostDataResponse` already has `GhostPerfDataInfo`, `GhostRankData`, `GhostWinningsData`, and `GhostTokenData`; use those existing names.

- [ ] **Step 4: Update `GhostMappers.cs` and controller**

Map `CommonGhostDataResponse` to `GetghostdataResponse`, including:

```csharp
ReleaseInfoFlag = common.ReleaseInfoFlag,
PlayedSongFlag = common.PlayedSongFlag,
TotalWinnings = common.TotalWinnings,
ghost_perf_data = new GetghostdataResponse.GhostPerfData
{
    InputMedian = common.GhostPerfData.InputMedian,
    InputVariance = common.GhostPerfData.InputVariance
},
GhostRecordData = new GetghostdataResponse.GhostRankData
{
    RankId = common.GhostRecordData.RankId,
    WinPoint = common.GhostRecordData.WinPoint,
    CertifiedLevelId = common.GhostRecordData.CertifiedLevelId
}
```

Then append winnings and tokens to generated repeated collections.

Controller:

```csharp
var common = await Mediator.Send(new GetGhostDataQuery(request.Baid), HttpContext.RequestAborted);
return Ok(GhostMappers.Map(common));
```

- [ ] **Step 5: Run focused test**

Run: `dotnet test --filter GetGhostData_ReturnsSavedGreenGhostState`

Expected: PASS.

---

## Task 06.2: Implement Ghost Score Readback

**Acceptance Criteria:**
- [ ] `getghostscore` returns saved stage section rows for requested baid/song/level.

**Steps:**

- [ ] **Step 1: Add test**

```csharp
[Fact]
public async Task GetGhostScore_ReturnsSavedSections()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var play = new SongPlayDatumGreen
    {
        Baid = 1,
        SongId = 101,
        Difficulty = Difficulty.Easy,
        PlayTime = DateTime.UtcNow
    };
    fixture.Context.SongPlayDataGreen.Add(play);
    await fixture.Context.SaveChangesAsync();
    fixture.Context.GhostStageSectionDataGreen.Add(new GhostStageSectionDatumGreen
    {
        PlayId = play.Id,
        SectionNo = 1,
        GoodCount = 10,
        OkCount = 2,
        MissCount = 1,
        PoundCount = 3
    });
    await fixture.Context.SaveChangesAsync();

    var handler = new GetGhostScoreQueryHandler(
        fixture.Context,
        NullLogger<GetGhostScoreQueryHandler>.Instance);

    var response = await handler.Handle(new GetGhostScoreQuery(1, 101, 0), CancellationToken.None);

    Assert.Single(response.AryBestSectionData);
}
```

Use actual entity property names in `GhostStageSectionDatumGreen`.

- [ ] **Step 2: Implement `GetGhostScoreQuery.Green.cs`**

Update `GetGhostScoreQuery.cs` so the primary constructor injects `ITaikoDbContext`:

```csharp
public partial class GetGhostScoreQueryHandler(
    ITaikoDbContext context,
    ILogger<GetGhostScoreQueryHandler> logger)
    : IRequestHandler<GetGhostScoreQuery, CommonGhostScoreResponse>
```

Query latest matching `SongPlayDatumGreen` by `Baid`, `SongId`, and mapped difficulty. Include `GhostStageSectionDataGreen` or join by play ID. Return `CommonGhostScoreResponse` with `Result = 1` and section rows.

- [ ] **Step 3: Update controller and mapper**

Controller sends:

```csharp
new GetGhostScoreQuery(request.Baid, request.SongNo, request.Level)
```

Mapper appends `GetghostscoreResponse.GhostBestSectionData` rows.

- [ ] **Step 4: Run tests**

Run: `dotnet test --filter GetGhostScore_ReturnsSavedSections`

Expected: PASS.

---

## Task 06.3: Implement Reward Execution And Card Check

**Acceptance Criteria:**
- [ ] Reward execution persists song/tone/costume/title unlock bytes.
- [ ] Reward card check resolves known card to baid.

**Steps:**

- [ ] **Step 1: Implement `RewardExecutionCommand.Green.cs`**

Update `RewardExecutionCommand.cs` so the primary constructor injects `ITaikoDbContext`:

```csharp
public partial class RewardExecutionCommandHandler(
    ITaikoDbContext context,
    ILogger<RewardExecutionCommandHandler> logger)
    : IRequestHandler<RewardExecutionCommand, CommonRewardExecutionResponse>
```

Use the same bit-setting policy from play result unlock handling:

```csharp
var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
SetBits(saveData.ToneFlg, request.GetToneNoes, GreenProtocolBytes.ToneFlagBytes);
SetBits(saveData.CostumeFlg1, request.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes);
SetBits(saveData.CostumeFlg2, request.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes);
SetBits(saveData.CostumeFlg3, request.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes);
SetBits(saveData.CostumeFlg4, request.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes);
SetBits(saveData.CostumeFlg5, request.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes);
SetBits(saveData.TitleFlg, request.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes);
await context.SaveChangesAsync(cancellationToken);
return new CommonRewardExecutionResponse { Result = 1 };
```

Add a private bounded `SetBits` helper that calls `GreenProtocolBytes.FixedOrZero` before setting bits.

- [ ] **Step 2: Implement `RewardCardCheckQuery.Green.cs`**

Update `RewardCardCheckQuery.cs` so the primary constructor injects `ITaikoDbContext`:

```csharp
public partial class RewardCardCheckQueryHandler(
    ITaikoDbContext context,
    ILogger<RewardCardCheckQueryHandler> logger)
    : IRequestHandler<RewardCardCheckQuery, CommonRewardCardCheckResponse>
```

Find `Card` by access code. Return `Result = 1` and `Baid` for known cards; return `Result = 1` and `Baid = 0` for unknown cards.

- [ ] **Step 3: Update controllers**

`RewardExecutionController` maps request arrays to `RewardExecutionCommand` and returns mapped response.

`RewardCardCheckController` sends `RewardCardCheckQuery` using request access code and returns `RewardcardcheckResponse`.

- [ ] **Step 4: Run build**

Run: `dotnet build`

Expected: PASS.

---

## Task 06.4: Implement Item Purchase And Deterministic Empty Extras

**Acceptance Criteria:**
- [ ] `itempurchase` updates medal spent when the user has enough medals.
- [ ] Catalog-light endpoints keep returning success and do not crash.

**Steps:**

- [ ] **Step 1: Implement `ItemPurchaseCommand.Green.cs`**

Update `ItemPurchaseCommand.cs` so the primary constructor injects `ITaikoDbContext`:

```csharp
public partial class ItemPurchaseCommandHandler(
    ITaikoDbContext context,
    ILogger<ItemPurchaseCommandHandler> logger)
    : IRequestHandler<ItemPurchaseCommand, CommonItemPurchaseResponse>
```

```csharp
var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
if (request.ItemPrice > 0 && saveData.TotalGetDonmedal >= saveData.TotalUseDonmedal + request.ItemPrice)
{
    saveData.TotalUseDonmedal += request.ItemPrice;
}
await context.SaveChangesAsync(cancellationToken);
return new CommonItemPurchaseResponse
{
    Result = 1,
    TotalGetDonmedal = saveData.TotalGetDonmedal,
    TotalUseDonmedal = saveData.TotalUseDonmedal
};
```

- [ ] **Step 2: Update `ItemPurchaseController.cs`**

Send `ItemPurchaseCommand` through Mediator and map totals to `ItempurchaseResponse`.

- [ ] **Step 3: Check deterministic empty endpoints**

Verify these controllers return `Result = 1` and log request summaries without hard-coded user state:

- `GetItemShopInfoController`
- `GetFolderController`
- `GetTelopController`
- `TournamentCheckController`
- `ChallengeCompeController`
- `RecommendController`
- `BookkeepingController`
- `HeadClerk2Controller`
- `HeartbeatController`
- `StartupAuthController`
- `VerupAuthController`
- `VerupCompleteController`

For `GetGhostDataController`, keep required nested objects non-null. For the other controllers in this list, the generated response types only require `Result` or simple repeated fields, so a `Result = 1` response is valid.

- [ ] **Step 4: Run tests and build**

Run:

```bash
dotnet test --filter GreenGhostRewardTests
dotnet build
```

Expected: both PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Handlers Application/Dtos Adapters.GameProtocol.Green Tests/Green/GreenGhostRewardTests.cs
git commit -m "feat(green): implement ghost reward and shop state"
```
