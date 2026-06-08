# Phase 15: Yellow Dani, Shop, Medals, WaiWai, and Admin - Pattern Map

**Mapped:** 2026-06-08
**Files analyzed:** 64
**Analogs found:** 64 / 64

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `Domain/Entities/DanScoreDatumYellow.cs` | entity | Yellow Dan playresult -> Dan readback/AdminApi | `DanScoreDatumBlue.cs`, `DanScoreDatumGreen.cs` | exact |
| `Domain/Entities/DanStageScoreDatumYellow.cs` | entity | Yellow Dan stage facts -> Dan score data | `DanStageScoreDatumBlue.cs`, `DanStageScoreDatumGreen.cs` | exact |
| `Application/Common/YellowDanHelpers.cs` | helper | Dan ids/grades -> save flags | `BlueDanHelpers.cs`, `GreenDanHelpers.cs` | exact |
| `Application/Handlers/GetDanScoreQuery.Yellow.cs` | handler partial | Yellow Dan rows -> common Dan response | `GetDanScoreQuery.Blue.cs`, `GetDanScoreQuery.Green.cs` | exact |
| `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` | handler partial | Yellow playresult -> normal/Dan/shop medal state | Blue/Green partials | role-match |
| `Domain/Entities/YellowShopSeasonState.cs` | entity | Yellow playresult/purchase -> season balances | `BlueShopSeasonState.cs`, `GreenShopSeasonState.cs` | exact |
| `Domain/Entities/YellowShopItemState.cs` | entity | Yellow purchase -> purchased item row | `BlueShopItemState.cs`, `GreenShopItemState.cs` | exact |
| `Application/Common/YellowShopStateExtensions.cs` | helper | context -> active season/unlocked items | `BlueShopStateExtensions.cs`, `GreenShopStateExtensions.cs` | exact |
| `Application/Ac15/YellowAc15ItemShopAdapter.cs` | adapter | AC15 shop service -> Yellow state/unlocks | `BlueAc15ItemShopAdapter.cs`, `GreenAc15ItemShopAdapter.cs` | exact |
| `Application/Handlers/ItemPurchaseCommand.Yellow.cs` | handler partial | purchase command -> Yellow shop service | `ItemPurchaseCommand.Blue.cs`, `ItemPurchaseCommand.Green.cs` | exact |
| `Adapters.GameProtocol.Yellow/Mappers/ItemShopMappers.cs` | wire mapper | Yellow wire purchase/info -> common DTOs | Blue/Green item shop mappers | exact |
| `Application/Ac15/YellowAc15UserDataAdapter.cs` | adapter | Yellow save/shop rows -> userdata locks | current Yellow adapter plus Blue shop adapter | exact |
| `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` | route host | Yellow route -> Mediator/compat response | current Yellow controller and Blue/Green purchase controllers | modify/split |
| `Application/Dtos/CommonPlayResultData.Yellow.cs` | optional DTO partial | Yellow-only logging facts | `CommonPlayResultData.Green.cs`, `CommonPlayResultData.BlueTokkun.cs` | role-match |
| `Adapters.AdminApi/Controllers/*` | API | Yellow tables/catalog -> WebUI DTOs | Blue/Green AdminApi branches | exact |
| `TaikoWebUI/Utilities/WebUiEra.cs` | WebUI era helper | enabled eras -> URL/API routing | existing Green/Blue branches | exact |
| `TaikoWebUI/Services/GameDataService.cs` | WebUI data client | Yellow API routes -> cache | existing Green/Blue support | exact |
| `Tests/Yellow/YellowDaniTests.cs` | tests | Dan persistence/readback | Blue/Green Dan tests | exact |
| `Tests/Yellow/YellowItemShopPurchaseTests.cs` | tests | purchase flow | Blue/Green item shop tests | exact |
| `Tests/Yellow/YellowWaiWaiTests.cs` | tests | evidence-bound WaiWai behavior | Yellow playresult/source guard tests | role-match |
| `Tests/Yellow/YellowAdminApiTests.cs` | tests | API/WebUI era routing | `BlueAdminApiDaniTests.cs`, WebUI tests | exact |

## Pattern Assignments

### Yellow Dani

Use Green/Blue Dan persistence shape, but create Yellow-owned types:

- `DanScoreDatumYellow`
- `DanStageScoreDatumYellow`
- `YellowDanClearGrade`
- `YellowDanHelpers`
- `DanScoreDataYellow`
- `DanStageScoreDataYellow`

The save summary update should mirror the proven AC15 rules:

- Normal Dan ids: `1..25`.
- Extra Dan ids: `101..128`.
- Packed grade flags use two bits per Dan id.
- `GotDanMax` is the highest cleared normal Dan id.
- `DispTaikojukuDan` advances to next uncleared normal Dan after a normal clear, otherwise normalizes to the next uncleared valid Dan.
- Dan costume id `36` may be applied only where Yellow save fields and AC15 policy support it.

### Yellow Shop

Use `Ac15ItemShopService` behind Yellow-owned state:

- `YellowShopSeasonState` maps to `YellowShopSeasonStates`.
- `YellowShopItemState` maps to `YellowShopItemStates`.
- `YellowShopStateExtensions.GetOrCreateActiveYellowShopSeasonStateAsync` should initialize the first active season from `UserSaveDataYellow.TotalGetDonmedal`/`TotalUseDonmedal` so Phase 14 medal totals are not stranded.
- `YellowAc15ItemShopAdapter` applies supported unlocks to `ReleaseSongFlg`, `ToneFlg`, and costume flag slots using `Ac15EraProfiles.Yellow.Limits`.

Yellow `getitemshopinfo.php` already uses the correct catalog-backed Yellow response shape. Yellow `itempurchase.php` should switch from no-state success to Mediator and map `ItempurchaseResponse` totals.

### Medals

Plan 02 should adjust Yellow normal playresult accounting:

- When an active Yellow item-shop season exists, add `GetDonmedal` to `YellowShopSeasonState.TotalGetDonmedal`.
- When no active season exists, keep Phase 14 behavior and add `GetDonmedal` to `UserSaveDataYellow.TotalGetDonmedal`.
- Always add `GetKatsumedal` to `UserSaveDataYellow.TotalGetKatsumedal`.
- Do not spend Katsu medals.
- Do not create Banacoin balance/payment/coupon/transaction rows.

### WaiWai

Current generated Yellow wire does not expose a `waiwai_tutorial_flg` field. Add tests that make this explicit. If implementation later finds a real field, mapper/readback can be added with test evidence in the same plan. Otherwise:

- Keep `UserSaveDataYellow.WaiwaiTutorialFlg` untouched except existing normal-field preservation when mapped.
- Preserve stage `WaiwaiResult`/`WaiwaiGauge` in play-history rows only if already mapped through common DTOs.
- Add diagnostic logs for protocol-backed fields only.
- Add a source guard that forbids `PlayMode.WaiWai`, `HandleYellowWaiWai`, or separate WaiWai classification.

### AdminApi/WebUI

Extend existing era-aware switches:

- `UserSettingsController`: Yellow partial over `UserSaveDataYellow`, `DanScoreDataYellow`, Yellow bitset sizes, and Yellow-owned tables.
- `SongLeaderboardController`: `SongBestDataYellow`.
- `PlayHistoryController`: `SongPlayDataYellow` plus `YellowFavoriteSongs`.
- `DanBestDataController`: `DanScoreDataYellow`.
- `GameDataController`: `catalog.Yellow()` music and Dan data.
- `WebUiEra`: add `Yellow` to `Supported`, `Known`, and `IsAc15`.
- `GameDataService`: existing normalized era path should work once Yellow is supported; add tests.

## Code Excerpts

### Purchase Service Shape

```csharp
return await Ac15ItemShopService.PurchaseAsync(
    new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
    snapshot.ItemShopCatalog,
    adapter,
    adapter,
    cancellationToken);
```

### Yellow Playresult Normal Guard to Preserve

```csharp
if (IsYellowTokkunShaped(playResultData))
{
    return 1;
}

var validStages = playResultData.AryStageInfoes
    .Where(stage => IsSupportedYellowNormalStage(request.Baid, stage))
    .ToList();
```

Plan 01/02 must preserve this order and must not mutate save/shop/Dan state before invalid-stage filtering.

### AdminApi Switch Shape

```csharp
return gameEra switch
{
    GameEra.Nijiiro => ...,
    GameEra.Green => ...,
    GameEra.Blue => ...,
    GameEra.Yellow => ...,
    _ => EraRoute.BadEra(era)
};
```

## Boundary Guard Patterns

Use tests/source scans to prove:

- Yellow production files do not query `DanScoreDataBlue`, `DanScoreDataGreen`, `BlueShop*`, or `GreenShop*`.
- Yellow AdminApi branches do not fall back to Blue/Green tables.
- `battleuserdata.php` remains absent.
- Yellow Banacoin routes remain log/success and no wallet/payment/transaction tables exist.
- Phase 16 Tokkun tables/history/readback are not introduced.
- WaiWai is not represented as a `PlayMode` or special handler branch.

## PATTERN MAPPING COMPLETE

