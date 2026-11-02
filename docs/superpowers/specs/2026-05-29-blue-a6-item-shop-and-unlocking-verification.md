# Blue A6 Item Shop And Unlocking Verification

## Scope

Blue A6 closes normal-mode item shop support for Blue: parser-proven default shop data, active-season advertisement, shop-info readback, season-scoped medal accounting, purchase validation, immediate purchase unlocks, Phase 1 rewardexecution success no-op behavior, userdata/BAID locking, and source guards.

This phase does not add AdminApi/WebUI parity, battle runtime behavior, Tokkun behavior, Banacoin behavior, or cabinet/RPCS3 smoke execution. Phase 3 owns repeatable cabinet/RPCS3 normal-mode smoke evidence.

## Provenance

- `Host/wwwroot/data/blue/blue_item_shop_data.json` is derived from local evidence at `H:\taiko\blue\rewardshopdata.bin`.
- `rewardshopdata.bin` is not committed, copied, or required at runtime.
- Parser tests prove the committed JSON matches the local cache values: season `1`, `verup_no` `20170404`, dates `20181219070000` to `20190314020000`, `afterstart_days` `30`, `beforeclose_days` `0`, and four kigurumi rows.
- Runtime loading uses the committed JSON through `BlueItemShopLoader`, not the binary.

## Item Domains

Blue A6 supports AC15 shop item types `1..7`:

| Item type | Domain | Blue save field |
|-----------|--------|-----------------|
| 1 | song | `ReleaseSongFlg` |
| 2 | tone | `ToneFlg` |
| 3 | kigurumi | `CostumeFlg1` |
| 4 | body | `CostumeFlg3` |
| 5 | head | `CostumeFlg2` |
| 6 | face | `CostumeFlg4` |
| 7 | puchi | `CostumeFlg5` |

Unsupported item types fail catalog/parser validation. Title-domain shop rows remain out of scope for Blue A6; if concrete cache or client evidence exposes a title domain, stop and discuss the mapping before runtime support.

## State Separation

- Blue shop totals are stored in `BlueShopSeasonStates` by BAID and season.
- Blue purchased items are stored in `BlueShopItemStates` by BAID, season, item type, and item id.
- Enabled-shop Blue playresult Don medals accrue to the active Blue shop season state.
- Blue purchase spends season Don medals and applies Blue save bits immediately.
- Disabled or inactive Blue shop flows do not create shop state.
- Blue A6 source guards reject Green shop state, Green protocol constants, Green wire model references, and Green save-data references in guarded Blue A6 files.

## Decision Coverage

| Decision | Coverage |
|----------|----------|
| D-01 | `itempurchase.php` is mediator-backed and owns Blue purchase/unlock behavior. |
| D-02 | `rewardexecution.php` returns success and mutates no shop or save state. |
| D-03 | Purchase preflight uses `item_no == 0` with omitted optional fields and returns season totals. |
| D-04 | Successful purchase spends Don medals, persists `BlueShopItemState`, applies Blue save bits, and returns success. |
| D-05 | Default values are derived from `H:\taiko\blue\rewardshopdata.bin`. |
| D-06 | `blue_item_shop_data.json` is committed and parser-proven. |
| D-07 | `rewardshopdata.bin` remains local-only and untracked. |
| D-08 | Parser and loader tests block partial, malformed, duplicate, unsupported, zero-id, or zero-price defaults. |
| D-09 | Blue shop season state starts at zero and never seeds from global Blue medal totals. |
| D-10 | Enabled-shop playresult Don medals accrue to active Blue shop season state. |
| D-11 | `UserSaveDataBlue.TotalGetDonmedal` and `TotalUseDonmedal` are not the enabled-shop accounting path. |
| D-12 | Disabled Blue shop does not create or update Blue shop Don medal state. |
| D-13 | BAID and purchase responses report active season totals when enabled and `0/0` when disabled. |
| D-14 | Item types `1..7` are supported. |
| D-15 | Title purchases are blocked pending concrete evidence and mapping discussion. |
| D-16 | Initialdata, userdata, and BAID hide active-season shop content until purchased. |
| D-17 | Unsupported item types fail validation instead of being advertised, purchased, or silently dropped. |
| D-18 | Blue save-field mapping uses `BlueProtocolBytes` widths for item types `1..7`. |

## Requirement Coverage

| Requirement | Automated coverage |
|-------------|--------------------|
| SHOP-01 | `BlueItemShopProtocolTests` covers disabled, missing, empty, and enabled active-season initialdata advertisement. |
| SHOP-02 | `BlueItemShopProtocolTests` covers active-season shop-info envelope and ordered item rows. |
| SHOP-03 | `BlueRewardShopDataParserTests` and `BlueItemShopLoaderTests` cover parser parity, default JSON loading, and fail-fast loader behavior. |
| SHOP-04 | `BlueItemShopPurchaseTests` covers forged tuple rejection before mutation. |
| SHOP-05 | `BlueItemShopStateTests`, `BlueItemShopPurchaseTests`, and `BlueItemShopLockingTests` cover season-scoped Blue medal state and no global Blue shop accounting. |
| SHOP-06 | `BlueItemShopPurchaseTests` covers immediate purchase unlocks, and `BlueItemShopProtocolTests` covers D-02 rewardexecution no-op success. |
| SHOP-07 | `BlueItemShopLockingTests` covers locked readback and purchased-visible readback. |
| SHOP-08 | `BlueA6SourceGuardTests` covers Green dependency separation for Blue A6 code and tests. |

## Final Gate Commands

Run these commands from the repository root:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShop|FullyQualifiedName~BlueRewardShopDataParser|FullyQualifiedName~BlueA6SourceGuard|FullyQualifiedName~BlueRewardExecution"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Blue|FullyQualifiedName~Ac15|FullyQualifiedName~GreenItemShop"
dotnet test Tests/Tests.csproj
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a6"
```

If default `Host/bin/Debug/net10.0` output is locked by a running server, the temp-output build above is the required verification path.

## Cabinet Smoke Deferral

No cabinet/RPCS3 smoke execution is part of Blue A6 closeout. Phase 3 must record date, enabled eras, Blue data path, observed endpoint logs, purchase request/response, and client readback behavior for normal-mode Blue smoke.
