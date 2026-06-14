<!-- refreshed: 2026-06-11 -->
# Architecture

**Analysis Date:** 2026-06-11

## System Overview

```text
+--------------------------------------------------------------------------------+
|                                 Host process                                   |
|                              `Host/Program.cs`                                 |
+----------------------+----------------------+----------------------------------+
| Admin REST API       | Game protocol HTTP   | AllNet/Mucha lifecycle           |
| `Adapters.AdminApi/` | `Adapters.GameProtocol.*` | `Adapters.AllnetMucha/`    |
+----------+-----------+----------+-----------+------------------+---------------+
           |                      |                              |
           v                      v                              v
+--------------------------------------------------------------------------------+
|                              Application layer                                |
| `Application/Handlers/` dispatches by `GameEra` through partial files          |
| `Application/Ac15/` owns shared AC15 capability services and projections       |
+-----------------------------------+--------------------------------------------+
                                    |
                                    v
+--------------------------------------------------------------------------------+
|                           Domain and contracts                                |
| `Domain/Entities/` era rows plus `IAc15*` row-shape interfaces                 |
| `Contracts.AdminApi/` shared AdminApi/WebUI DTOs                               |
+-----------------------------------+--------------------------------------------+
                                    |
                                    v
+--------------------------------------------------------------------------------+
|                              Infrastructure                                   |
| `Infrastructure/Persistence/` EF Core SQLite partial context                   |
| `Infrastructure/GameDataCatalog/` era catalogs and AC15 filesystem loaders     |
+-----------------------------------+--------------------------------------------+
                                    |
                                    v
+--------------------------------------------------------------------------------+
|                         SQLite and filesystem data                             |
| `Host/wwwroot/taiko.db3`, `Host/wwwroot/data/{green,blue,yellow}/`, configs    |
+--------------------------------------------------------------------------------+

+--------------------------------------------------------------------------------+
|                         Blazor WebAssembly admin UI                            |
| `TaikoWebUI/` calls `Adapters.AdminApi/` through `Contracts.AdminApi/` DTOs     |
+--------------------------------------------------------------------------------+
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| Host composition root | Loads split configuration, resolves enabled eras, registers adapter assemblies, removes disabled era controllers, runs migrations, initializes catalogs, serves WebUI files, and maps HTTP endpoints. | `Host/Program.cs` |
| Application handlers | Own use-case dispatch through `GameEra` switches and era partial implementations. | `Application/Handlers/UpdatePlayResultCommand.cs`, `Application/Handlers/UserDataQuery.cs` |
| AC15 shared core | Own reusable AC15 behavior for normal play, Dani, user data, initial data, catalog readback, crowns, item shop, profile counters, ChallengeCompe catalog contracts, and user settings without owning a shared EF table. | `Application/Ac15/` |
| Domain model | Own persistent entities, era-owned save rows, canonical AC15 enum values, and narrow `IAc15*` row-shape interfaces. | `Domain/Entities/`, `Domain/Enums/Ac15DanClearGrade.cs`, `Domain/Enums/Ac15ShopItemType.cs` |
| Persistence port | Exposes direct, era-partitioned `DbSet` properties to application code. | `Application/Abstractions/ITaikoDbContext.cs`, `Application/Abstractions/ITaikoDbContext.Blue.cs`, `Application/Abstractions/ITaikoDbContext.Green.cs`, `Application/Abstractions/ITaikoDbContext.Yellow.cs` |
| EF persistence | Maps SQLite tables through partial `TaikoDbContext` files by shared identity, Nijiiro, Green, Blue, and Yellow state. | `Infrastructure/Persistence/TaikoDbContext.cs`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, `Infrastructure/Persistence/TaikoDbContext.Green.cs`, `Infrastructure/Persistence/TaikoDbContext.Yellow.cs` |
| Game data catalogs | Load immutable startup/catalog data from filesystem data roots; AC15 reusable parsers live under shared infrastructure while era catalogs expose typed contracts. | `Infrastructure/GameDataCatalog/Ac15/`, `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Yellow/YellowEraGameDataCatalog.cs` |
| Shared game protocol adapter | Owns `/v01r00/chassis/*` startup/version routes and protocol controller/compression helpers. | `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`, `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs` |
| AC15 game protocol adapters | Own era route prefixes, generated wire models, and adapter-local Mapperly/manual mappers for Green, Blue, and Yellow. | `Adapters.GameProtocol.Green/`, `Adapters.GameProtocol.Blue/`, `Adapters.GameProtocol.Yellow/` |
| Nijiiro protocol adapters | Own WW and CN Nijiiro route surfaces independent from AC15 shared behavior. | `Adapters.GameProtocol.WwR08/`, `Adapters.GameProtocol.CnR00/` |
| Admin API adapter | Serves `/api/...` and `/api/{era}/...` routes for WebUI using `ITaikoDbContext`, `IGameDataCatalog`, and shared AC15 services where behavior matches. | `Adapters.AdminApi/Controllers/`, `Adapters.AdminApi/Controllers/EraRoute.cs` |
| WebUI | Provides an era-aware Blazor WebAssembly admin experience using shared contracts and `WebUiEra` URL helpers. | `TaikoWebUI/Program.cs`, `TaikoWebUI/Utilities/WebUiEra.cs` |
| Tests | Protect shared AC15 behavior in `Tests/Ac15/` and era-specific protocol/persistence boundaries in `Tests/Green/`, `Tests/Blue/`, and `Tests/Yellow/`. | `Tests/Ac15/`, `Tests/Green/`, `Tests/Blue/`, `Tests/Yellow/` |

## Pattern Overview

**Overall:** Ports-and-adapters ASP.NET Core solution with era-owned protocol surfaces and a capability-based AC15 application core.

**Key Characteristics:**
- Keep inbound transport era-local: Green routes stay in `Adapters.GameProtocol.Green/`, Blue routes stay in `Adapters.GameProtocol.Blue/`, Yellow routes stay in `Adapters.GameProtocol.Yellow/`, and shared startup/version routes stay in `Adapters.GameProtocol.Shared/`.
- Dispatch by `GameEra` in unsuffixed handler files such as `Application/Handlers/UpdatePlayResultCommand.cs:17` and implement era behavior in `.Green.cs`, `.Blue.cs`, and `.Yellow.cs` partials.
- Put behavior that is actually identical across AC15 eras in `Application/Ac15/`, using `Ac15EraProfile`, `Ac15FeatureSet`, `Ac15ProtocolLimits`, `Ac15WirePlacement`, and transport-agnostic catalog contracts such as `Application/Ac15/ChallengeCompe/`.
- Keep persistence explicit and traceable through `ITaikoDbContext`; shared AC15 services accept concrete `DbSet` handles and mapping delegates through records such as `Application/Ac15/Ac15NormalPlayRecords.cs` and `Application/Ac15/Ac15DaniRecords.cs`.
- Use narrow Domain row-shape interfaces such as `Domain/Entities/IAc15SongPlayDatum.cs`, `Domain/Entities/IAc15DanScoreDatum.cs`, and `Domain/Entities/IAc15ShopItemState.cs` to share algorithms without introducing shared gameplay tables.
- Use Mapperly for mechanical projections in `Application/Ac15/Ac15NormalPlayMapper.cs`, `Application/Ac15/Ac15DaniMapper.cs`, `Application/Ac15/Ac15ItemShopMapper.cs`, and adapter `Mappers/` folders; keep protocol placement and presence semantics in adapter-local mappers.
- Leave duplication era-local when semantics differ: Green ghost battle updates stay in `Application/Handlers/UpdatePlayResultCommand.Green.cs`, Blue battle/Tokkun gates stay in `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, and Yellow Tokkun/WaiWai handling stays in `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`.

## Layers

**Host Layer:**
- Purpose: Compose the process, read configuration, register enabled era adapters, run migrations, initialize catalogs, serve Blazor files, and provide middleware.
- Location: `Host/`
- Contains: `Host/Program.cs`, `Host/Configurations/`, `Host/wwwroot/`, `Host/Logging/CsvFormatter.cs`
- Depends on: `Application/`, `Infrastructure/`, `Adapters.*`, `TaikoWebUI/`
- Used by: `dotnet run --project Host`, published server deployments, and local cabinet/RPCS3 smoke runs

**Inbound Adapter Layer:**
- Purpose: Translate HTTP/protobuf/AdminApi requests into application requests and map application responses back to transport DTOs.
- Location: `Adapters.AdminApi/`, `Adapters.AllnetMucha/`, `Adapters.GameProtocol.Shared/`, `Adapters.GameProtocol.WwR08/`, `Adapters.GameProtocol.CnR00/`, `Adapters.GameProtocol.Green/`, `Adapters.GameProtocol.Blue/`, `Adapters.GameProtocol.Yellow/`
- Contains: `Controllers/`, `Mappers/`, generated `Wire/`, adapter `DependencyInjection.cs`, compression helpers, middleware
- Depends on: `Application/`; AdminApi and AllNet also reference `Infrastructure/` where the existing adapter code reads concrete persistence/catalog services
- Used by: MVC application parts in `Host/Program.cs:134`

**Application Handler Layer:**
- Purpose: Own Mediator requests, use-case dispatch, no-cross-era routing, and cabinet-visible behavior.
- Location: `Application/Handlers/`
- Contains: `*Command.cs`, `*Query.cs`, and era partials such as `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Depends on: `Application/Ac15/`, `Application/Abstractions/`, `Application/Dtos/`, `Domain/`
- Used by: Protocol adapters, AdminApi services, tests

**AC15 Shared Core Layer:**
- Purpose: Share value-identical AC15 algorithms while keeping era-specific route, wire, catalog, and EF ownership visible.
- Location: `Application/Ac15/`
- Contains: `Ac15NormalPlayWriter`, `Ac15DaniWriter`, `Ac15ItemShopPurchase`, `Ac15UserDataService`, `Ac15InitialDataService`, `Ac15CatalogReadbackService`, `Ac15*Mapper` projection classes, profile/limit records, and era user-data adapters.
- Depends on: `Domain/Entities/IAc15*.cs`, `Application/Dtos/Common*.cs`, `ITaikoDbContext`, EF `DbSet`
- Used by: Green, Blue, and Yellow handler partials plus AC15 AdminApi user settings

**Domain Layer:**
- Purpose: Define persistent entities, enum values, row-shape contracts, and shared identity model.
- Location: `Domain/`
- Contains: `Domain/Entities/`, `Domain/Enums/`, `Domain/DomainConstants.cs`
- Depends on: No project references from `Domain/Domain.csproj`
- Used by: `Application/`, `Infrastructure/`, adapters, contracts, and tests

**Infrastructure Layer:**
- Purpose: Implement persistence, catalog loading, identity, settings, clock, and migrations.
- Location: `Infrastructure/`
- Contains: `Infrastructure/Persistence/`, `Infrastructure/GameDataCatalog/`, `Infrastructure/Identity/`, `Infrastructure/Settings/`, `Infrastructure/Time/`
- Depends on: `Application/`, `Contracts.AdminApi/`, `Domain/`
- Used by: `Host/Program.cs`, `Adapters.AdminApi/`, `Adapters.AllnetMucha/`, tools

**WebUI Layer:**
- Purpose: Provide the Blazor WebAssembly admin UI over AdminApi contracts.
- Location: `TaikoWebUI/`
- Contains: `Pages/`, `Components/`, `Services/`, `Utilities/WebUiEra.cs`, `Authorization/`, `wwwroot/`
- Depends on: `Contracts.AdminApi/`
- Used by: `Host/Program.cs:226` through Blazor static file hosting

**Runtime Data Layer:**
- Purpose: Store SQLite, operator data, server-authored JSON sidecars, static WebUI assets, logs, and local certificates.
- Location: `Host/wwwroot/`, `Host/Configurations/`, `Host/Certificates/`
- Contains: `Host/wwwroot/data/green/`, `Host/wwwroot/data/blue/`, `Host/wwwroot/data/yellow/`, `Host/wwwroot/data/shared/`
- Depends on: Filesystem paths resolved by `Infrastructure/GameDataCatalog/PathHelper.cs`
- Used by: `Infrastructure/GameDataCatalog/*` loaders and host startup

## Data Flow

### Host Startup Path

1. `Host/Program.cs:48` through `Host/Program.cs:53` load split JSON configuration from `Host/Configurations/`.
2. `Host/Program.cs:75` reads enabled eras from `ServerSettings:Eras`; `Host/Program.cs:81` refuses startup when no era is enabled.
3. `Host/Program.cs:112` registers infrastructure with enabled-era context; `Host/Program.cs:115` through `Host/Program.cs:130` register only enabled game protocol adapter services.
4. `Host/Program.cs:134` registers protobuf MVC formatters; `Host/Program.cs:139` through `Host/Program.cs:154` remove disabled adapter assemblies from MVC application parts.
5. `Host/Program.cs:174` applies EF Core migrations for `Infrastructure/Persistence/TaikoDbContext.cs`.
6. `Host/Program.cs:189` initializes `IGameDataCatalog`; `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs:25` initializes Nijiiro first and AC15 era catalogs in parallel.
7. `Host/Program.cs:226` serves Blazor files, `Host/Program.cs:253` maps controllers, and `Host/Program.cs:256` adds AllNet/Mucha middleware.

### AC15 Protocol Request Path

1. A cabinet posts protobuf to an era-owned route: Green `/v11r01/chassis/*` in `Adapters.GameProtocol.Green/Controllers/`, Blue `/v10r03/chassis/*` in `Adapters.GameProtocol.Blue/Controllers/`, or Yellow `/v09r02/chassis/*` in `Adapters.GameProtocol.Yellow/Controllers/`.
2. Shared startup/version requests use `/v01r00/chassis/*` in `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs:6` and `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs:6`.
3. The controller maps wire DTOs to `Application/Dtos/Common*.cs`; direct playresult routes show this in `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs:12` and `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs:12`, while Green decodes its compressed payload before mapping in `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs:26`.
4. Controllers send Mediator requests with explicit era values, for example `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs:15`.
5. The unsuffixed handler dispatches by `GameEra`, for example `Application/Handlers/UpdatePlayResultCommand.cs:17`.
6. Era partials guard mode-specific boundaries: Blue checks Tokkun and battle before normal play in `Application/Handlers/UpdatePlayResultCommand.Blue.cs:26` and `Application/Handlers/UpdatePlayResultCommand.Blue.cs:31`; Yellow checks Tokkun-shaped data in `Application/Handlers/UpdatePlayResultCommand.Yellow.cs:28`; Green applies ghost updates in `Application/Handlers/UpdatePlayResultCommand.Green.cs:115`.
7. Shared AC15 services run identical behavior through era-supplied tables, profiles, and mappers: normal play uses `Application/Ac15/Ac15NormalPlayWriter.cs:25`, Dani uses `Application/Ac15/Ac15DaniWriter.cs:5`, and item purchase uses `Application/Ac15/Ac15ItemShopPurchase.cs:5`.
8. The same request writes only era-owned rows, for example `Application/Handlers/UpdatePlayResultCommand.Blue.cs:100`, `Application/Handlers/UpdatePlayResultCommand.Green.cs:93`, and `Application/Handlers/UpdatePlayResultCommand.Yellow.cs:94` each provide concrete `DbSet` tables to the shared writer.

### AC15 User Data Readback Path

1. `userdata.php` controllers in `Adapters.GameProtocol.Green/Controllers/UserDataController.cs`, `Adapters.GameProtocol.Blue/Controllers/UserDataController.cs`, and `Adapters.GameProtocol.Yellow/Controllers/UserDataController.cs` send `UserDataQuery` with the adapter era.
2. `Application/Handlers/UserDataQuery.cs:14` dispatches to the correct era partial.
3. Each era partial reads its own save, best, favorite, recent, shop, and Dani rows through `ITaikoDbContext.*`, then builds a typed catalog snapshot through `Application/Ac15/Ac15CatalogSnapshotFactory.cs:22`, `Application/Ac15/Ac15CatalogSnapshotFactory.cs:39`, or `Application/Ac15/Ac15CatalogSnapshotFactory.cs:56`.
4. Era adapters flatten typed save/catalog state into `Ac15UserDataSnapshot` in `Application/Ac15/BlueAc15UserDataAdapter.cs:5`, `Application/Ac15/GreenAc15UserDataAdapter.cs:5`, and `Application/Ac15/YellowAc15UserDataAdapter.cs:5`.
5. `Application/Ac15/Ac15UserDataService.cs:5` builds the common response; wire placement controls Tokkun tutorial readback at `Application/Ac15/Ac15UserDataService.cs:55`.
6. Era partials append wire-era fields such as `IsDevilGreen`, `IsDevilBlue`, and `IsExplainYellow` in `Application/Handlers/UserDataQuery.Green.cs:49`, `Application/Handlers/UserDataQuery.Blue.cs:49`, and `Application/Handlers/UserDataQuery.Yellow.cs:46`.

### AC15 Catalog and Initial Data Flow

1. Infrastructure registers enabled typed catalogs in `Infrastructure/DependencyInjection.cs:72`, `Infrastructure/DependencyInjection.cs:79`, and `Infrastructure/DependencyInjection.cs:86`.
2. Era catalogs load AC15-compatible raw files and sidecars from era data roots: Green at `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs:71`, Blue at `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs:73`, and Yellow at `Infrastructure/GameDataCatalog/Yellow/YellowEraGameDataCatalog.cs:68`.
3. Reusable parsers live under `Infrastructure/GameDataCatalog/Ac15/`, such as `Infrastructure/GameDataCatalog/Ac15/Ac15MusicInfoLoader.cs:8`, `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs:21`, `Infrastructure/GameDataCatalog/Ac15/Ac15TelopLoader.cs:16`, and `Infrastructure/GameDataCatalog/Ac15/Ac15TaikojukuLoader.cs:17`.
4. `Application/Ac15/Ac15CatalogSnapshotFactory.cs:9` projects typed era catalogs to `Ac15CatalogSnapshot`.
5. `Application/Ac15/Ac15InitialDataService.cs:5` builds the shared initial-data body, then era partials add only era-specific flags such as Green ghost advertisement in `Application/Handlers/GetInitialDataQuery.Green.cs:9` and Blue battle advertisement in `Application/Handlers/GetInitialDataQuery.Blue.cs:14`.

### Admin API and WebUI Path

1. `TaikoWebUI/Utilities/WebUiEra.cs:9` lists supported eras and `TaikoWebUI/Utilities/WebUiEra.cs:55` constructs `/api/{era}/...` URLs.
2. Admin controllers validate route era strings through `Adapters.AdminApi/Controllers/EraRoute.cs:5`.
3. Controllers preserve legacy Nijiiro routes and expose era routes such as `Adapters.AdminApi/Controllers/UserSettingsController.cs:36` and `Adapters.AdminApi/Controllers/GameDataController.cs:14`.
4. AC15 AdminApi profile settings reuse `Application/Ac15/Ac15UserSettingsService.cs`; each partial supplies the concrete save row, Dani `DbSet`, access policy, and limits in `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs:16`, `Adapters.AdminApi/Controllers/UserSettingsController.Blue.cs:16`, and `Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs:16`.
5. Other AdminApi surfaces intentionally read era-owned tables directly, such as Yellow play data in `Adapters.AdminApi/Controllers/PlayDataController.Yellow.cs:7`.

**State Management:**
- SQLite state is scoped per request through `Infrastructure/DependencyInjection.cs:53` and exposed as direct `ITaikoDbContext` `DbSet`s.
- Shared identity tables live in `Application/Abstractions/ITaikoDbContext.Shared.cs`; Green, Blue, Yellow, and Nijiiro gameplay rows live in era-specific partial interfaces and EF partials.
- Catalog state is singleton and initialized once through `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`.
- `Application/Ac15/` services are stateless; behavior depends on supplied profiles, catalog snapshots, `DbSet`s, and mapper delegates.
- WebUI client state is cached in `TaikoWebUI/Services/` and route construction is centralized in `TaikoWebUI/Utilities/WebUiEra.cs`.

## Key Abstractions

**`GameEra`:**
- Purpose: Select enabled adapters, catalogs, handler partials, persistence sets, AdminApi routes, and WebUI URLs.
- Examples: `Domain/Enums/GameEra.cs`, `Host/Program.cs:75`, `Application/Handlers/UserDataQuery.cs:14`, `TaikoWebUI/Utilities/WebUiEra.cs:9`
- Pattern: Enum-driven dispatch with partial implementation files and host application-part gating.

**`ITaikoDbContext`:**
- Purpose: Application-facing persistence port with explicit era-owned `DbSet` properties.
- Examples: `Application/Abstractions/ITaikoDbContext.Shared.cs`, `Application/Abstractions/ITaikoDbContext.Green.cs`, `Application/Abstractions/ITaikoDbContext.Blue.cs`, `Application/Abstractions/ITaikoDbContext.Yellow.cs`
- Pattern: Partial interface mirrored by partial EF implementation files in `Infrastructure/Persistence/TaikoDbContext*.cs`.

**AC15 row-shape interfaces:**
- Purpose: Allow generic storage algorithms over separate Green, Blue, and Yellow entities.
- Examples: `Domain/Entities/IAc15SongPlayDatum.cs`, `Domain/Entities/IAc15SongBestDatum.cs`, `Domain/Entities/IAc15DanScoreDatum.cs`, `Domain/Entities/IAc15ShopSeasonState.cs`
- Pattern: Narrow entity capability interfaces, not repositories or shared gameplay rows.

**AC15 profiles and capability records:**
- Purpose: Describe era limits, feature availability, and protocol placement for shared services.
- Examples: `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15FeatureSet.cs`, `Application/Ac15/Ac15WirePlacement.cs`
- Pattern: Immutable profile records passed to services such as `Ac15UserDataService`, `Ac15InitialDataService`, and `Ac15DaniWriter`.

**AC15 table records:**
- Purpose: Bind shared algorithms to concrete era `DbSet`s and projections.
- Examples: `Application/Ac15/Ac15NormalPlayRecords.cs`, `Application/Ac15/Ac15DaniRecords.cs`, `Application/Ac15/Ac15ItemShopRecords.cs`
- Pattern: Generic table/delegate records supplied by era partial handlers; no hidden persistence adapter.

**Mapperly projections:**
- Purpose: Generate mechanical mappings between canonical AC15 records and concrete era rows or wire DTOs.
- Examples: `Application/Ac15/Ac15NormalPlayMapper.cs`, `Application/Ac15/Ac15DaniMapper.cs`, `Application/Ac15/Ac15ItemShopMapper.cs`, `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`
- Pattern: `[Mapper]` partial classes with explicit ignores and hand-written glue for protocol presence or derived fields.

**Common DTOs:**
- Purpose: Carry adapter-neutral request/response shapes between protocol adapters and application handlers.
- Examples: `Application/Dtos/CommonPlayResultData.cs`, `Application/Dtos/CommonUserDataResponse.cs`, `Application/Dtos/CommonInitialDataCheckResponse.cs`
- Pattern: Common DTOs plus era partial fields when the application needs to hold era-specific facts.

**Catalog multiplexer:**
- Purpose: Provide enabled era catalogs through a single application abstraction.
- Examples: `Application/Abstractions/IGameDataCatalog.cs`, `Application/Common/CatalogExtensions.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`
- Pattern: `For(GameEra)` plus typed extension casts such as `catalog.Blue()` and `catalog.Yellow()`.

**Controller bases and route helpers:**
- Purpose: Centralize lazy Mediator/logger access and era route parsing.
- Examples: `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`, `Adapters.AdminApi/BaseAdminController.cs`, `Adapters.AdminApi/Controllers/EraRoute.cs`
- Pattern: Thin controllers that deserialize, map, send Mediator requests or direct AdminApi reads, and map responses.

## Entry Points

**ASP.NET Core Host:**
- Location: `Host/Program.cs`
- Triggers: `dotnet run --project Host`, published executable startup, service launch
- Responsibilities: Configuration, DI, migrations, catalog initialization, middleware, controllers, Blazor hosting, route gating

**AC15 Shared Startup and Version Routes:**
- Location: `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`, `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs`, `Adapters.GameProtocol.Shared/Controllers/VerupCompleteController.cs`
- Triggers: Cabinet requests under `/v01r00/chassis/*`
- Responsibilities: Shared startupauth movie/operation echo and version/auth compatibility routes

**Green Protocol Adapter:**
- Location: `Adapters.GameProtocol.Green/Controllers/`
- Triggers: Cabinet requests under `/v11r01/chassis/*`
- Responsibilities: Green wire mapping, compressed playresult decode, Green ghost behavior, Green route-only protocol details

**Blue Protocol Adapter:**
- Location: `Adapters.GameProtocol.Blue/Controllers/`
- Triggers: Cabinet requests under `/v10r03/chassis/*`
- Responsibilities: Blue direct protobuf mapping, battle routes, Tokkun/Banacoin-adjacent compatibility, Blue route-only protocol details

**Yellow Protocol Adapter:**
- Location: `Adapters.GameProtocol.Yellow/Controllers/`
- Triggers: Cabinet requests under `/v09r02/chassis/*`
- Responsibilities: Yellow direct protobuf mapping, Yellow Tokkun compatibility, Yellow no-battle route surface, Yellow route-only protocol details

**Nijiiro Protocol Adapters:**
- Location: `Adapters.GameProtocol.WwR08/Controllers/`, `Adapters.GameProtocol.CnR00/Controllers/`
- Triggers: Cabinet requests under `/v12r08_ww/chassis/*` and `/v12r00_cn/chassis/*`
- Responsibilities: Nijiiro WW/CN protocol handling outside the AC15 shared core

**Admin API:**
- Location: `Adapters.AdminApi/Controllers/`
- Triggers: WebUI and REST calls under `/api/...` and `/api/{era}/...`
- Responsibilities: Users, credentials, settings, game data, favorites, play history, leaderboard, Dani, and customization surfaces

**Blazor WebUI:**
- Location: `TaikoWebUI/Program.cs`
- Triggers: Browser loads static assets served by `Host/Program.cs`
- Responsibilities: Auth config, localization, era-aware API calls, pages/components/services for admin workflows

**CLI Tools:**
- Location: `GreenCatalogExtractor/Program.cs`, `LocalSaveModScoreMigrator/Program.cs`
- Triggers: `dotnet run --project GreenCatalogExtractor`, `dotnet run --project LocalSaveModScoreMigrator`
- Responsibilities: Green catalog extraction and local-save score import utility flows

## Architectural Constraints

- **Threading:** ASP.NET Core handles concurrent requests; `TaikoDbContext` is scoped in `Infrastructure/DependencyInjection.cs:53`, while catalog implementations are singleton and initialized once in `Host/Program.cs:189`.
- **Global state:** Serilog `Log.Logger` is process-global in `Host/Program.cs`; `IGameDataCatalog` is singleton in `Infrastructure/DependencyInjection.cs:90`; WebUI caches are client-side services under `TaikoWebUI/Services/`.
- **Circular imports:** Keep dependencies flowing inward to `Application/Abstractions/` and outward through `Infrastructure/`; protocol adapters must not reference each other.
- **Era enablement:** `Host/Program.cs:139` through `Host/Program.cs:154` remove disabled era adapter assemblies from MVC routing, so new controllers must live in the correct adapter assembly.
- **AC15 persistence:** Do not add shared Green/Blue/Yellow gameplay tables or repository-shaped persistence adapters; use direct `ITaikoDbContext` plus `Domain/Entities/IAc15*.cs` row-shape interfaces and Mapperly projections.
- **Route ownership:** Keep Green under `/v11r01/chassis`, Blue under `/v10r03/chassis`, Yellow under `/v09r02/chassis`, shared startup/version under `/v01r00/chassis`, and Nijiiro under `/v12r08_ww/chassis` or `/v12r00_cn/chassis`.
- **Generated wire:** Treat `Adapters.GameProtocol.*/Wire/` as generated protocol output unless regenerating from `proto/`; place manual protocol decisions in adapter mappers or handlers.
- **Data roots:** Resolve runtime data through `Infrastructure/GameDataCatalog/PathHelper.cs` and era path helpers such as `Infrastructure/GameDataCatalog/Yellow/YellowGameDataPaths.cs`; do not hardcode source-checkout paths in handlers.

## Anti-Patterns

### Shared AC15 Repository Layer

**What happens:** A new `IAc15Repository` or generic persistence adapter hides `ITaikoDbContext` and concrete era `DbSet`s from handlers.
**Why it's wrong:** The current architecture intentionally keeps persistence traceable through `Application/Abstractions/ITaikoDbContext.*.cs`, `Infrastructure/Persistence/TaikoDbContext.*.cs`, and shared table records in `Application/Ac15/Ac15*Records.cs`.
**Do this instead:** Pass concrete era `DbSet`s and Mapperly delegates to shared services, following `Application/Handlers/UpdatePlayResultCommand.Blue.cs:100` and `Application/Handlers/UpdatePlayResultCommand.Yellow.cs:94`.

### Cross-Era Protocol Coupling

**What happens:** Yellow imports Blue wire types, Green routes reuse Blue controllers, or protocol placement rules move into `Application/Ac15/` without an era profile boundary.
**Why it's wrong:** Routes, generated `Wire/` DTOs, and protocol field presence are era-owned and are gated by adapter assemblies in `Host/Program.cs`.
**Do this instead:** Put shared behavior in `Application/Ac15/` only after mapping to `Common*` DTOs, and keep era transport mapping in `Adapters.GameProtocol.<Era>/Mappers/`.

### Shared Core Owning Era-Only Semantics

**What happens:** Green ghost, Blue battle, Blue/Yellow Tokkun, Yellow WaiWai diagnostics, or Banacoin-adjacent compatibility is generalized as a default AC15 behavior.
**Why it's wrong:** These semantics are not value-identical across Green, Blue, and Yellow; moving them to shared code can introduce cross-mode or cross-era writes.
**Do this instead:** Leave era-only branches in files such as `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`, `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs`, and `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs`.

### Wire DTO Persistence

**What happens:** A generated request/response type from `Adapters.GameProtocol.* /Wire/` is saved directly into `Domain/Entities/` or EF rows.
**Why it's wrong:** Wire DTOs are protocol placement artifacts; persistence rows are era-owned domain state.
**Do this instead:** Map wire DTOs to `Application/Dtos/Common*.cs` in adapter mappers, then map canonical application records to era rows through `Application/Ac15/*Mapper.cs`.

### Hardcoded Runtime Paths

**What happens:** New code constructs `Host/wwwroot/data/<era>` or assumes source checkout paths inside application handlers.
**Why it's wrong:** Source checkout and published runtime layouts differ; catalog code already owns path resolution.
**Do this instead:** Use `Infrastructure/GameDataCatalog/PathHelper.cs` and era path helpers such as `Infrastructure/GameDataCatalog/Blue/BlueGameDataPaths.cs`, `Infrastructure/GameDataCatalog/Green/GreenGameDataPaths.cs`, and `Infrastructure/GameDataCatalog/Yellow/YellowGameDataPaths.cs`.

## Error Handling

**Strategy:** Fail fast for invalid startup/configuration, throw on unsupported internal era states, and return cabinet-compatible protobuf success/failure bodies where protocol behavior requires it.

**Patterns:**
- Startup refuses empty enabled-era configuration in `Host/Program.cs:81`.
- Disabled catalog access throws from `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs:15`.
- Handler dispatchers throw for unsupported `GameEra` values, for example `Application/Handlers/UpdatePlayResultCommand.cs:23`.
- Protocol controllers return protobuf failure bodies for decode or validation failures, such as Green playresult decode failures in `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`.
- Admin controllers return `BadRequest`, `NotFound`, `Forbid`, `Ok`, or `NoContent` directly through `Adapters.AdminApi/Controllers/`.

## Cross-Cutting Concerns

**Logging:** Serilog is configured in `Host/Program.cs`; game controllers inherit `Logger` through `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`, AdminApi controllers inherit `BaseAdminController`, and CSV logging uses `Host/Logging/CsvFormatter.cs`.

**Validation:** Startup settings validation is registered in `Infrastructure/DependencyInjection.cs:38`; AdminApi era validation uses `Adapters.AdminApi/Controllers/EraRoute.cs`; AC15 services validate protocol limits and state changes in `Application/Ac15/`.

**Authentication:** JWT bearer auth, admin policy registration, and local-mode policy bypass live in `Infrastructure/DependencyInjection.cs` and `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs`; WebUI reads auth config through `TaikoWebUI/Program.cs`.

**Serialization and Compression:** Protobuf MVC formatters are registered in `Host/Program.cs:134`; missing game request content types are normalized in `Host/Program.cs:280`; compression helpers live in `Adapters.GameProtocol.Shared/Compression/`.

**Era Routing:** Runtime adapter routing is controlled by `Host/Program.cs`; AdminApi era parsing is centralized in `Adapters.AdminApi/Controllers/EraRoute.cs`; WebUI URLs are built with `TaikoWebUI/Utilities/WebUiEra.cs`.

---

*Architecture analysis: 2026-06-11*
