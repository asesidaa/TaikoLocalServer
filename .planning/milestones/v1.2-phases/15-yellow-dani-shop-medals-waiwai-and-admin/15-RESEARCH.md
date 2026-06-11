# Phase 15: Yellow Dani, Shop, Medals, WaiWai, and Admin - Research

**Researched:** 2026-06-08
**Status:** complete

## Phase Scope

Phase 15 finishes the remaining Yellow normal-era surfaces before Phase 16 Tokkun/Banacoin work:

- Yellow-owned Dani result persistence and readback.
- Yellow item-shop purchase state, Don medal spend state, and unlock writes.
- Don/Katsu medal accounting that remains separate from Banacoin.
- WaiWai evidence-bound tutorial/readback/logging behavior without treating WaiWai as a mode.
- AdminApi/WebUI Yellow readback over profile, score/history, favorite/recent, Dani, shop-relevant, Tokkun placeholder, and catalog state.

The phase must not add Yellow battle, real Banacoin state, Tokkun persistence/classification/readback beyond already existing Phase 14 no-write protection, or Blue/Green/Nijiiro gameplay table reuse.

## Current Source Baseline

### Yellow Protocol Evidence

`proto/yellow/yellow.proto` and `Adapters.GameProtocol.Yellow/Wire/Game.cs` expose the Phase 15 protocol anchors:

- `TaikojukuRequest.get_dan`, `TaikojukuResponse.ary_taikojuku_data`, and `TaikojukuResponse.TaikojukuData.get_dan`.
- `PlayResultRequest.play_mode`, per-stage `play_dan`, `dan_result`, `get_donmedal`, `get_katsumedal`, `itemshop_tutorial_flg`, Tokkun fields, and normal stage fields.
- `UserDataResponse.disp_taikojuku_dan`, `got_dan_max`, `got_dan_flg`, `got_danextra_flg`, medal totals, and `itemshop_tutorial_flg`.
- `GetitemshopinfoResponse.ary_itemshop_data` at Yellow field 5 with no Blue-only timing requirement in Yellow planning.
- `ItempurchaseRequest`/`ItempurchaseResponse` with `total_get_donmedal` and `total_use_donmedal`.
- `Rewardcardcheck*` and `Rewardexecution*` success-shaped compatibility messages.

The scan did not find a generated Yellow `waiwai_tutorial_flg` field in `UserDataResponse` or `PlayResultRequest`. Current Yellow persistence has `UserSaveDataYellow.WaiwaiTutorialFlg` inherited from Phase 14 planning, but Phase 15 must treat any Yellow WaiWai protocol readback as an evidence gap unless implementation finds a real Yellow wire field. Existing common stage history can preserve `WaiwaiResult` and `WaiwaiGauge` where the mapper exposes them.

### Phase 13/14 Verified Foundation

Phase 13 verified:

- Yellow `/v09r00/chassis` route ownership and direct protobuf transport.
- Yellow catalog and `ST9100-1` data loading through `PathHelper` and `IYellowCatalog`.
- Catalog-backed `taikojuku.php` and `getitemshopinfo.php` readback.
- Deferred runtime routes stayed scaffolded until later phases.

Phase 14 verified:

- Yellow-owned `UserSaveDataYellow`, `SongBestDatumYellow`, `SongPlayDatumYellow`, `YellowFavoriteSongs`, and `YellowRecentSongs`.
- Yellow BAID/mydon/userdata/playresult/self-best/crowns route behavior.
- Yellow normal playresult writes only Yellow-owned normal state.
- Raw Yellow crown field-3 readback.
- Tokkun-shaped Yellow uploads return success without normal writes.
- Invalid Yellow stages are filtered before save/profile mutation.

Phase 15 must preserve the Phase 14 invalid-stage guard, Tokkun-shaped no-write guard, no-battle absence, and no-cross-era-write tests.

## Existing Code Patterns

### Dani

Closest analogs:

- `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- `Application/Common/GreenDanHelpers.cs`
- `Application/Common/BlueDanHelpers.cs`
- `Domain/Entities/DanScoreDatumGreen.cs`
- `Domain/Entities/DanStageScoreDatumGreen.cs`
- `Domain/Entities/DanScoreDatumBlue.cs`
- `Domain/Entities/DanStageScoreDatumBlue.cs`
- `Application/Handlers/GetDanScoreQuery.Green.cs`
- `Application/Handlers/GetDanScoreQuery.Blue.cs`
- `Adapters.AdminApi/Controllers/DanBestDataController.cs`

Green/Blue persist Dan rows only when:

- `playResultData.PlayMode == (uint)PlayMode.DanMode`.
- Exactly one distinct nonzero `PlayDan` exists across stages.
- `DanResult` is within the known clear-grade range.
- The catalog has the requested challenge level.
- The Dan id is known normal `1..25` or extra `101..128`.

They store one Dan best row plus per-stage best facts, then update save summary fields: `GotDanFlg`, `GotDanExtraFlg`, `GotDanMax`, `DispTaikojukuDan`, and optional Dan costume behavior. Yellow should follow the same policy where the Yellow proto/catalog matches, but with Yellow-owned helpers, entities, DbSets, mappings, and tests.

### Item Shop and Medals

Closest analogs:

- `Application/Ac15/Ac15ItemShopService.cs`
- `Application/Ac15/BlueAc15ItemShopAdapter.cs`
- `Application/Ac15/GreenAc15ItemShopAdapter.cs`
- `Application/Ac15/IAc15ItemShopPersistence.cs`
- `Application/Ac15/IAc15ItemShopUnlockPolicy.cs`
- `Application/Handlers/ItemPurchaseCommand.Blue.cs`
- `Application/Handlers/ItemPurchaseCommand.Green.cs`
- `Application/Common/BlueShopStateExtensions.cs`
- `Application/Common/GreenShopStateExtensions.cs`
- `Domain/Entities/BlueShopSeasonState.cs`
- `Domain/Entities/BlueShopItemState.cs`
- `Domain/Entities/GreenShopSeasonState.cs`
- `Domain/Entities/GreenShopItemState.cs`
- `Adapters.GameProtocol.Blue/Mappers/ItemShopMappers.cs`
- `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`

`Ac15ItemShopService.PurchaseAsync` already performs the shared validation spine:

- Disabled or empty active shop returns success-shaped zero/balance state.
- Preflight request (`ItemNo == 0` and absent item tuple fields) returns current season balances without spending.
- Forged item no/type/id/price tuples fail.
- Zero-price rows fail.
- Duplicate purchased rows fail.
- Insufficient Don medals fail.
- Success increments `TotalUseDonmedal`, applies unlocks, writes a purchased item row, saves, and returns totals.

Yellow should use this service behind a Yellow adapter and Yellow state tables. For playresult Don medal accumulation, Green/Blue already route active-season medals into season state when an active shop exists. Yellow should move Phase 14 direct Don medal accumulation into active Yellow season state where available, while Katsu medals remain profile/readback-only unless Yellow evidence proves a Katsu spend path.

### Userdata Shop Locks

`YellowAc15UserDataAdapter.CreateSnapshot` already accepts `unlockedShopItems` and computes locked song/tone ids from the active item-shop catalog. Phase 14 passes no real Yellow shop state. Phase 15 should feed Yellow purchased item rows into this adapter so Yellow userdata can hide locked shop songs/tones until purchased.

### WaiWai

Current evidence supports only conservative handling:

- Do not add a `PlayMode.WaiWai`.
- Do not route WaiWai before normal/Tokkun as a special mode.
- Keep normal play history preservation of stage `WaiwaiResult` and `WaiwaiGauge` where Yellow mapper/common DTO exposes them.
- Persist/read back a tutorial flag only if a real Yellow wire field exists. Current generated Yellow wire scan did not show one.
- Additional facts should be diagnostic/append-only only when protocol-backed.

### AdminApi/WebUI

Closest analogs:

- `Adapters.AdminApi/Controllers/EraRoute.cs`
- `Adapters.AdminApi/Controllers/UserSettingsController.cs`
- `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`
- `Adapters.AdminApi/Controllers/UserSettingsController.Blue.cs`
- `Adapters.AdminApi/Controllers/SongLeaderboardController.cs`
- `Adapters.AdminApi/Controllers/PlayHistoryController.cs`
- `Adapters.AdminApi/Controllers/DanBestDataController.cs`
- `Adapters.AdminApi/Controllers/GameDataController.cs`
- `TaikoWebUI/Utilities/WebUiEra.cs`
- `TaikoWebUI/Services/GameDataService.cs`
- `Tests/Blue/BlueAdminApiDaniTests.cs`
- `Tests/WebUi/GameDataServiceTests.cs`

Admin controllers already support `/api/{era}/...` through `EraRoute.TryParse`, but many switches return only Nijiiro/Green/Blue. WebUI `WebUiEra.Supported` and `IsAc15` do not include Yellow. Phase 15 should add Yellow read paths to existing era-aware controllers, not add legacy Yellow routes, and should keep WebUI flows generic rather than building Yellow-only pages.

## Recommended Plan Slicing

1. `15-01`: Yellow Dani state and readback. This creates Dan entities/DbSets/migration/helper logic and wires Yellow Dan playresult/readback into existing route/handler surfaces. It unlocks Dani AdminApi readback later.
2. `15-02`: Yellow shop and Don medal state. This creates Yellow shop state, adapter, purchase route behavior, active-season Don medal accumulation, and userdata shop locks. It unlocks shop-relevant AdminApi readback later.
3. `15-03`: Yellow WaiWai evidence/logging and boundary tests. This is narrow and evidence-bound. It preserves current normal row facts and records the current lack of Yellow wire tutorial readback if no field exists.
4. `15-04`: AdminApi/WebUI Yellow readback. This depends on plans 01/02/03 so it can expose the state surfaces without cross-era fallbacks.

## Validation Architecture

Phase 15 requires focused automated validation plus normal repository build checks during execution:

- Dani: handler, helper, EF, route/mapper, userdata readback, and AdminApi Dan tests.
- Shop/medals: item-shop mapper/controller, purchase handler, adapter, EF state, userdata locked item readback, and no Banacoin/table-cross-write tests.
- WaiWai: source/protocol evidence tests, normal-history preservation tests, no special-mode branch/source guard.
- AdminApi/WebUI: `/api/Yellow/...` controller tests, `WebUiEra` and `GameDataService` tests, plus route tests proving legacy era routes are preserved.
- Regression: focused `FullyQualifiedName~Yellow`, relevant `FullyQualifiedName~ItemShop|Dan|WebUi`, full `dotnet test Tests/Tests.csproj`, temp-output Host build, and `git diff --check`.

Runtime cabinet/RPCS3 smoke remains Phase 17.

## Risks

| Risk | Mitigation |
|------|------------|
| Copying Green/Blue Dan state into shared tables | Require Yellow-owned entities, DbSets, migrations, and no-cross-era tests. |
| Double-spending Don medals between save totals and shop season state | Make active Yellow shop season state the spend source when active; test preflight, playresult accumulation, purchase, and duplicate flows. |
| Treating Katsu medals as shop currency or Banacoin | Keep Katsu on `UserSaveDataYellow` until concrete Yellow evidence proves a spend path. |
| Inventing WaiWai wire/readback fields | Add source/protocol test that records the current absence; only map real fields. |
| Starting Phase 16 Tokkun/Banacoin work | Source guards for no Yellow Tokkun history/persistence and no Banacoin wallet/payment/transaction tables. |
| AdminApi silently falling back to Blue/Green | Controller tests seed conflicting cross-era rows and assert Yellow routes read only Yellow tables. |

## RESEARCH COMPLETE

