# Blue Support Research: Features

**Date:** 2026-05-28
**Scope:** Remaining full-Blue capabilities after completed Superpowers stages A0-A5.

## Already Delivered Inputs

The user stated stages A0-A5 are complete. Treat these as existing capability for planning:

- A0 evidence/bootstrap spec: `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md`
- A1 adapter skeleton: `docs/superpowers/specs/2026-05-27-blue-a1-era-foundation-adapter-skeleton-design.md`
- A2 catalog/data layout: `docs/superpowers/specs/2026-05-28-blue-a2-catalog-data-layout-design.md`
- A3 identity/profile/userdata: `docs/superpowers/specs/2026-05-28-blue-a3-identity-profile-userdata-design.md`
- A4 enso play result/self-best/crowns/rewards: `docs/superpowers/specs/2026-05-28-blue-a4-enso-play-result-score-crowns-rewards-design.md`
- A5 Dani Dojo: `docs/superpowers/specs/2026-05-28-blue-a5-dani-dojo-design.md`

## Remaining Track A Features

### A6 Item Shop And Unlocking

Table-stakes capabilities:

- Blue advertises the configured active shop season through `getitemshopinfo.php`.
- Blue `initialdatacheck.php` advertises item-shop version data only when the active shop is enabled and populated.
- Blue purchase requests validate `item_no`, `item_type`, `item_id`, and `item_price` against the active Blue catalog.
- Blue shop medal state is season-scoped and Blue-owned.
- Purchased/rewarded shop items unlock Blue save fields, not Green fields.
- Blue `rewardexecution.php` handles configured unlocks in the Blue save state.

Existing local inputs:

- `Application/Abstractions/IBlueCatalog.cs`
- `Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs`
- `Application/Catalog/Blue/BlueItemShopCatalog.cs`
- Green analogs in `Application/Handlers/ItemPurchaseCommand.Green.cs` and `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`

### A7 Basic AdminApi And WebUI Parity

Table-stakes capabilities:

- Blue users can be inspected through the same basic WebUI/AdminApi surfaces as Green where Blue data exists.
- Blue profile, scores, play history, favorites, customization catalogs, item-shop-relevant state, and Dani readback use Blue projections.
- Unsupported Blue edit surfaces are hidden or read-only rather than writing Green or Nijiiro state.
- WebUI era selection recognizes Blue without making Blue the default enabled era.

Existing local inputs:

- `TaikoWebUI/Utilities/WebUiEra.cs`
- `Adapters.AdminApi/Controllers/GameDataController.cs`
- `Adapters.AdminApi/Controllers/DanBestDataController.cs`
- Green WebUI design in `docs/superpowers/specs/2026-05-16-green-webui-support-design.md`

### A8 Normal Cabinet Smoke And Hardening

Table-stakes capabilities:

- Boot with Blue enabled and other eras disabled.
- New card registration.
- Known card login.
- Song list and normal play.
- Play result save.
- Self-best and crown readback.
- Dani flow.
- Item shop purchase and reward unlock.
- WebUI readback.
- Logs capture unexpected Blue-only endpoint calls without crashing.

Existing local inputs:

- `Host/README.md`
- `README.md`
- Blue tests under `Tests/Blue/`

## Track B Features

Battle-mode scope includes:

- `battleuserdata.php` response and persistence.
- `InitialdatacheckResponse.is_battleplay`, release flags, and battle caps.
- Blue `PlayResultRequest.StageData.BattleStageData` handling.
- NPC identity, costume, special moves, EXP, DPN, bonds level, stage unlocks, boss life, last stage state, tokens, and battle rewards.
- Evidence-driven decision on whether battle results affect normal scores/crowns.

Track B should begin with an evidence/design phase, not a runtime implementation phase.

## Deliberate Anti-Features

- Do not implement Tokkun behavior.
- Do not implement Banacoin/payment state unless new evidence proves Blue requires a safe non-payment behavior.
- Do not reuse Green AI Battle semantics as Blue battle truth.
- Do not share Blue shop, battle, Dan, score, or userdata persistence with Green.

