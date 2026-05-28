# Blue Support Research: Architecture

**Date:** 2026-05-28
**Scope:** Architecture implications for the remaining Blue roadmap.

## Existing Architecture To Preserve

TaikoLocalServer is a ports-and-adapters .NET application:

- `Host/Program.cs` composes enabled eras and controls route availability.
- `Adapters.GameProtocol.Blue/Controllers/` owns Blue cabinet endpoints.
- `Adapters.GameProtocol.Blue/Mappers/` converts Blue protobuf wire types to common application DTOs.
- `Application/Handlers/*.Blue.cs` owns Blue behavior behind era-aware dispatcher files.
- `Domain/Entities/*Blue.cs` and `Infrastructure/Persistence/TaikoDbContext.Blue.cs` own Blue persistent state.
- `Infrastructure/GameDataCatalog/Blue/` owns Blue catalog loading.
- `Adapters.AdminApi/`, `Contracts.AdminApi/`, and `TaikoWebUI/` expose management and readback surfaces.

## A6 Architecture

A6 should mirror the successful Green item-shop shape but with Blue ownership:

- Add Blue shop state entities instead of reusing `GreenShopSeasonState` or `GreenShopItemState`.
- Add Blue DbSets and EF mapping in the Blue persistence partials.
- Add Blue helper methods analogous to `Application/Common/GreenShopStateExtensions.cs`, but typed around `UserSaveDataBlue`.
- Add Blue unlock helpers that use `BlueProtocolBytes` and Blue save fields.
- Add `GetItemShopInfoQuery.Blue.cs` or make the existing query explicitly era-aware before adding Blue dispatch.
- Add `ItemPurchaseCommand.Blue.cs` for Blue validation, spending, and unlock effects.
- Wire Blue `getitemshopinfo.php`, `itempurchase.php`, `rewardcardcheck.php`, and `rewardexecution.php` through mediator-backed controller paths where applicable.

## A7 Architecture

A7 should make AdminApi and WebUI parity broader without forking the UI:

- Extend existing AdminApi controllers with `GameEra.Blue` branches where the resource has Blue data.
- Keep projection logic server-side so WebUI routes remain era-aware but not table-aware.
- Reuse existing `Contracts.AdminApi` models where concepts match; add optional/era-aware fields only when the UI needs them.
- Keep unsupported settings/edit surfaces read-only or hidden until Blue-specific write semantics exist.
- Add tests for API URL construction, route normalization, and no accidental Green writes.

## A8 Architecture

A8 should be a hardening and evidence phase:

- Add a cabinet smoke checklist under `.planning/` or `docs/`.
- Add log capture guidance for unexpected Blue calls.
- Add source guard tests for remaining cross-era boundaries.
- Add focused regression tests for A6/A7 user-visible flows.
- Add explicit unresolved-items routing: Track B, follow-up bug, or non-goal.

## Track B Architecture

Track B needs its own battle model instead of adapting Green AI Battle:

- Start with a battle evidence/spec phase.
- Define battle catalog surfaces only after examining `config/S10100-1/battle` and client expectations.
- Add Blue battle save entities only after required byte widths/defaults are known.
- Keep battle playresult effects isolated until evidence says whether normal score/crown state should be updated.
- Treat `Application/Common/GreenAiBattleLevels.cs` and Green AI Battle handlers as contrast material, not a reusable implementation.

## Architectural Risks

- Shared `Common*` DTO files are cross-era surfaces; Blue-only fields belong in `.Blue.cs` partials where possible.
- Controller stubs from A1 can hide missing application behavior if A6/Track B forgets to replace them.
- Green item-shop code is tempting to copy but contains Green type names, Green save fields, and Green state assumptions.
- Battle fields include byte arrays and nested repeated state where unsafe zero defaults could crash or mislead the client.

