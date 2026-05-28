# Blue Support Research: Stack

**Date:** 2026-05-28
**Scope:** Constrained research for finishing Blue support. This file records existing stack decisions relevant to A6-A8 and Track B. No broad external ecosystem research was needed because the project is brownfield and the stack is already fixed.

## Existing Stack

- Runtime: .NET 10 / ASP.NET Core host in `Host/Program.cs`.
- Protocol serialization: protobuf-net through game protocol adapters, with Blue wire types in `Adapters.GameProtocol.Blue/Wire/Game.cs`.
- Application dispatch: Mediator handlers under `Application/Handlers/`, with era-specific partial files such as `Application/Handlers/UserDataQuery.Blue.cs` and `Application/Handlers/UpdatePlayResultCommand.Blue.cs`.
- Persistence: EF Core SQLite through `Infrastructure/Persistence/TaikoDbContext.cs` and era partials such as `Infrastructure/Persistence/TaikoDbContext.Blue.cs`.
- Catalog loading: filesystem-backed era catalogs through `IGameDataCatalog`; Blue catalog surface is `Application/Abstractions/IBlueCatalog.cs`.
- Admin UI: Blazor WebAssembly and AdminApi contracts under `TaikoWebUI/`, `Adapters.AdminApi/`, and `Contracts.AdminApi/`.
- Tests: xUnit in `Tests/`, with current Blue coverage under `Tests/Blue/`.

## Reuse Decisions

- Use the existing Blue adapter, not a new protocol host.
- Use existing AC15 catalog loader sharing for Blue item shop input; `Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs` already maps `blue_item_shop_data.json` through the shared AC15 loader.
- Use Green item-shop implementation only as a structural analog. The Blue implementation needs Blue-owned entities, DbSets, handlers, byte helpers, and tests.
- Use existing WebUI era-routing helpers from `TaikoWebUI/Utilities/WebUiEra.cs`; extend surfaces, not the application shell.
- Use cabinet/RPCS3 logs, proto files, local SQLite state, and IDA/client evidence before implementing battle semantics.

## Stack Constraints For Roadmap

- A6 should stay in the existing handler/controller/mapper pattern:
  - `GetItemShopInfoQuery.Blue.cs`
  - `ItemPurchaseCommand.Blue.cs`
  - Blue item-shop mappers under `Adapters.GameProtocol.Blue/Mappers/`
  - Blue shop entities and EF migration
- A7 should extend existing AdminApi/WebUI surfaces by era, following the Green and current Blue Dani patterns.
- A8 should be verification-heavy and may mostly produce docs, tests, scripts, and log evidence rather than new feature code.
- Track B must add battle-specific domain/application concepts only after the strict evidence gate is satisfied.

## Not Researched Further

- No package/framework alternatives were researched because changing stack is out of scope.
- No runtime wiki scraping or official page scraping was done because requirements can be defined from the existing Superpowers roadmap and repo state.
- No battle field defaults were inferred from training data; those remain evidence tasks.

