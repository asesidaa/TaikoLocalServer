<!-- refreshed: 2026-05-28 -->
# Architecture

**Analysis Date:** 2026-05-28

## System Overview

```text
+--------------------------------------------------------------------------------+
|                                Host process                                    |
|                         `Host/Program.cs`                                      |
+----------------------+------------------------+-------------------------------+
| Admin REST API       | Game protocol adapters | AllNet/Mucha lifecycle        |
| `Adapters.AdminApi`  | `Adapters.GameProtocol.*` | `Adapters.AllnetMucha`     |
+----------+-----------+------------+-----------+---------------+---------------+
           |                        |                           |
           |                        v                           |
           |          +-----------------------------+            |
           |          | Shared protocol scaffolding |            |
           |          | `Adapters.GameProtocol.Shared`           |
           |          +-----------------------------+            |
           |                        |                           |
           v                        v                           v
+--------------------------------------------------------------------------------+
|                           Application use cases                                |
|      Mediator handlers, ports, Common* DTOs, settings in `Application/`        |
+-----------------------------------+--------------------------------------------+
                                    |
                                    v
+--------------------------------------------------------------------------------+
|                         Domain model and contracts                             |
|         `Domain/` entities/enums/constants; `Contracts.AdminApi/` DTOs         |
+-----------------------------------+--------------------------------------------+
                                    |
                                    v
+--------------------------------------------------------------------------------+
|                             Infrastructure                                     |
|       EF Core SQLite, file catalog, JWT, clock in `Infrastructure/`            |
+-----------------------------------+--------------------------------------------+
                                    |
                                    v
+--------------------------------------------------------------------------------+
|                        SQLite and filesystem data                              |
|     `Host/wwwroot/taiko.db3`, `Host/wwwroot/data/`, `Host/Configurations/`     |
+--------------------------------------------------------------------------------+

+--------------------------------------------------------------------------------+
|                         Blazor WebAssembly admin UI                            |
|       `TaikoWebUI/` is built into and served by `Host/Program.cs`              |
+--------------------------------------------------------------------------------+
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| Host composition root | Loads split configuration, registers layers, gates era adapters, runs migrations, initializes catalogs, and maps controllers/static Blazor files. | `Host/Program.cs` |
| Domain | Owns persistent entities, enums, and constants without outbound project references. | `Domain/Domain.csproj`, `Domain/Entities/`, `Domain/Enums/GameEra.cs` |
| Admin contracts | Owns WebUI/admin API request, response, view model, authorization, converter, and shared server-data DTOs. | `Contracts.AdminApi/Contracts.AdminApi.csproj`, `Contracts.AdminApi/ViewModels/` |
| Application | Owns Mediator requests/handlers, port interfaces, version-agnostic `Common*` DTOs, catalog contracts, and settings models. | `Application/Application.csproj`, `Application/Handlers/`, `Application/Abstractions/` |
| Infrastructure | Implements application ports for EF Core SQLite, filesystem catalogs, JWT issuance, auth policy evaluation, and time. | `Infrastructure/DependencyInjection.cs`, `Infrastructure/Persistence/TaikoDbContext.cs` |
| Admin API adapter | Serves `/api/...` endpoints consumed by `TaikoWebUI`; current controllers primarily use `ITaikoDbContext` and `IGameDataCatalog` directly. | `Adapters.AdminApi/Controllers/`, `Adapters.AdminApi/BaseAdminController.cs` |
| Game protocol adapters | Serve game client protobuf endpoints by era/version and map wire DTOs to `Application` `Common*` DTOs. | `Adapters.GameProtocol.WwR08/`, `Adapters.GameProtocol.CnR00/`, `Adapters.GameProtocol.Green/`, `Adapters.GameProtocol.Blue/` |
| Shared game protocol adapter | Owns shared `/v01r00` startup/version endpoints, controller base class, gzip/header helpers, and shared wire types. | `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`, `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs` |
| AllNet/Mucha adapter | Serves cabinet lifecycle, activation, updater, and Garmc endpoints; applies PowerOn body middleware. | `Adapters.AllnetMucha/Controllers/`, `Adapters.AllnetMucha/DependencyInjection.cs` |
| WebUI | Blazor WASM admin interface using MudBlazor, admin contracts, era-aware API URL helpers, auth state, and cached game-data service. | `TaikoWebUI/Program.cs`, `TaikoWebUI/Utilities/WebUiEra.cs`, `TaikoWebUI/Services/GameDataService.cs` |
| Runtime data | Stores operator JSON/binary game data, generated/ignored local era data, certificates, logs, and SQLite DB next to the host executable. | `Host/wwwroot/data/`, `Host/Configurations/`, `Host/Certificates/` |
| Tools and tests | CLI utilities and xUnit coverage for protocol, catalog, mapper, WebUI, and startup behavior. | `GreenCatalogExtractor/`, `LocalSaveModScoreMigrator/`, `Tests/` |

## Pattern Overview

**Overall:** Hexagonal / ports-and-adapters .NET solution with a modular ASP.NET Core host.

**Key Characteristics:**
- Keep the domain core independent: `Domain/Domain.csproj` has no project references and `Application/Application.csproj` defines ports instead of depending on `Infrastructure/`.
- Compose all runtime dependencies in `Host/Program.cs`; adapters expose `DependencyInjection.cs` extension methods such as `Adapters.GameProtocol.Green/DependencyInjection.cs` and `Adapters.AllnetMucha/DependencyInjection.cs`.
- Use era-aware partial files for shared use cases: the central dispatcher in `Application/Handlers/BaidQuery.cs` switches on `GameEra`, while `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/BaidQuery.Blue.cs`, and `Application/Handlers/BaidQuery.Nijiiro.cs` own era-specific behavior.
- Use adapter-local wire models and mappers: `Adapters.GameProtocol.Green/Wire/` and `Adapters.GameProtocol.Green/Mappers/` should translate to `Application/Dtos/Common*.cs` before crossing into handlers.
- Keep WebUI DTO compatibility through `Contracts.AdminApi/`; `TaikoWebUI/TaikoWebUI.csproj` references `Contracts.AdminApi/Contracts.AdminApi.csproj` only.

## Layers

**Host Layer:**
- Purpose: Own process startup, configuration loading, middleware order, static file hosting, controller registration, migration execution, and era enablement.
- Location: `Host/`
- Contains: `Host/Program.cs`, `Host/Configurations/`, `Host/wwwroot/`, `Host/Logging/CsvFormatter.cs`, `Host/Host.csproj`
- Depends on: `Adapters.*`, `Application`, `Domain`, `Infrastructure`, `TaikoWebUI`
- Used by: Operators and development commands such as `dotnet run --project Host`

**Domain Layer:**
- Purpose: Define persistent business entities, enums, and constants.
- Location: `Domain/`
- Contains: `Domain/Entities/`, `Domain/Enums/`, `Domain/DomainConstants.cs`
- Depends on: None at the project-reference level via `Domain/Domain.csproj`
- Used by: `Application/`, `Infrastructure/`, `Contracts.AdminApi/`, adapters, tests, and tools

**Contracts Layer:**
- Purpose: Define admin API and WebUI DTOs, converters, auth helpers, and shared server-data shapes.
- Location: `Contracts.AdminApi/`
- Contains: `Contracts.AdminApi/Requests/`, `Contracts.AdminApi/Responses/`, `Contracts.AdminApi/ViewModels/`, `Contracts.AdminApi/ServerData/`, `Contracts.AdminApi/Converters/`, `Contracts.AdminApi/Authorization/`
- Depends on: `Domain/Domain.csproj`
- Used by: `Adapters.AdminApi/`, `Application/`, `Infrastructure/`, `TaikoWebUI/`, and `Tests/`

**Application Layer:**
- Purpose: Orchestrate game protocol use cases through Mediator and expose ports for persistence, catalog, auth, and time.
- Location: `Application/`
- Contains: `Application/Handlers/`, `Application/Abstractions/`, `Application/Dtos/`, `Application/Common/`, `Application/Catalog/`, `Application/ServerData/`, `Application/Settings/`
- Depends on: `Domain/Domain.csproj`, `Contracts.AdminApi/Contracts.AdminApi.csproj`, `Mediator.Abstractions`, `Microsoft.EntityFrameworkCore`
- Used by: `Infrastructure/`, `Adapters.*`, `Host/`, tests, and tools

**Infrastructure Layer:**
- Purpose: Implement application ports and own all durable I/O.
- Location: `Infrastructure/`
- Contains: `Infrastructure/Persistence/`, `Infrastructure/Persistence/Migrations/`, `Infrastructure/GameDataCatalog/`, `Infrastructure/Identity/`, `Infrastructure/Time/`, `Infrastructure/Settings/`
- Depends on: `Application/`, `Contracts.AdminApi/`, `Domain/`
- Used by: `Host/Program.cs`, `Adapters.AdminApi/`, `Adapters.AllnetMucha/`, `GreenCatalogExtractor/`, `LocalSaveModScoreMigrator/`

**Inbound Adapter Layer:**
- Purpose: Translate HTTP requests from game clients, WebUI, and AllNet/Mucha into application/domain work.
- Location: `Adapters.AdminApi/`, `Adapters.AllnetMucha/`, `Adapters.GameProtocol.Shared/`, `Adapters.GameProtocol.WwR08/`, `Adapters.GameProtocol.CnR00/`, `Adapters.GameProtocol.Green/`, `Adapters.GameProtocol.Blue/`
- Contains: `Controllers/`, `Mappers/`, `Wire/`, adapter `DependencyInjection.cs`, middleware and compression helpers
- Depends on: `Application/`; game adapters also use `Adapters.GameProtocol.Shared/`; admin and AllNet adapters use `Infrastructure/`
- Used by: ASP.NET Core application parts registered from `Host/Program.cs`

**WebUI Layer:**
- Purpose: Provide the Blazor WASM admin interface hosted by the same ASP.NET Core process.
- Location: `TaikoWebUI/`
- Contains: `TaikoWebUI/Pages/`, `TaikoWebUI/Components/`, `TaikoWebUI/Services/`, `TaikoWebUI/Authorization/`, `TaikoWebUI/Utilities/`, `TaikoWebUI/wwwroot/`
- Depends on: `Contracts.AdminApi/Contracts.AdminApi.csproj`
- Used by: `Host/Host.csproj` as a project reference and `Host/Program.cs` via `UseBlazorFrameworkFiles()`

## Data Flow

### Host Startup Path

1. `Host/Program.cs:47` loads split JSON configuration from `Host/Configurations/` without using `Host/appsettings.json` as the primary configuration surface.
2. `Host/Program.cs:110` registers application services, `Host/Program.cs:111` registers infrastructure, and `Host/Program.cs:116` through `Host/Program.cs:125` register enabled game protocol adapters.
3. `Host/Program.cs:128` adds MVC controllers and `Host/Program.cs:136` through `Host/Program.cs:145` remove disabled-era application parts so disabled adapter routes are not routable.
4. `Host/Program.cs:167` applies EF Core migrations for `Infrastructure/Persistence/TaikoDbContext.cs`.
5. `Host/Program.cs:182` initializes `IGameDataCatalog`, which calls `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs:25`.
6. `Host/Program.cs:219` serves Blazor WASM files, `Host/Program.cs:246` maps controllers, and `Host/Program.cs:249` adds AllNet PowerOn middleware.

### Game Protocol Request Path

1. A game client posts protobuf data to an adapter route such as `Adapters.GameProtocol.Green/Controllers/BaidController.cs:4` or `Adapters.GameProtocol.Blue/Controllers/BaidController.cs:4`.
2. The controller inherits lazy `IMediator` and `ILogger` access from `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`.
3. The controller sends a request with an explicit `GameEra`, for example `Adapters.GameProtocol.Green/Controllers/BaidController.cs:12`.
4. The application dispatcher in `Application/Handlers/BaidQuery.cs:15` through `Application/Handlers/BaidQuery.cs:17` routes to `HandleNijiiro`, `HandleGreen`, or `HandleBlue`.
5. The era partial handler reads `ITaikoDbContext` and era catalogs, for example `Application/Handlers/BaidQuery.Green.cs` reads Green save data and `Application/Handlers/BaidQuery.Blue.cs` reads Blue save data.
6. The adapter mapper converts `CommonBaidResponse` back to the wire DTO, for example `Adapters.GameProtocol.Green/Mappers/BaidResponseMapper.cs` or `Adapters.GameProtocol.Blue/Mappers/BaidResponseMapper.cs`.
7. The ASP.NET Core protobuf formatter registered in `Host/Program.cs:128` serializes the response as `application/protobuf`.

### Admin WebUI Request Path

1. `TaikoWebUI/Program.cs:15` starts the WASM client and `TaikoWebUI/Program.cs:26` fetches `api/Auth/Config` from `Adapters.AdminApi/Controllers/AuthController.cs`.
2. `TaikoWebUI/Program.cs:41` registers `TaikoWebUI/Services/GameDataService.cs` and `TaikoWebUI/Program.cs:85` initializes WebUI game-data caches through admin API routes.
3. WebUI calls are built with era-aware helpers in `TaikoWebUI/Utilities/WebUiEra.cs`; `WebUiEra.Api(...)` routes to endpoints such as `Adapters.AdminApi/Controllers/GameDataController.cs:13`.
4. Admin controllers use `[Authorize]` at controller level such as `Adapters.AdminApi/Controllers/UsersController.cs:8`; `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs` short-circuits policy checks when auth is disabled.
5. Current admin controllers often access `ITaikoDbContext` or `IGameDataCatalog` directly, as shown by `Adapters.AdminApi/Controllers/UsersController.cs:9` and `Adapters.AdminApi/Controllers/GameDataController.cs:8`.

### Catalog Initialization Flow

1. `Infrastructure/DependencyInjection.cs:63`, `Infrastructure/DependencyInjection.cs:70`, and `Infrastructure/DependencyInjection.cs:77` register per-era catalog implementations only for enabled eras.
2. `Infrastructure/DependencyInjection.cs:86` registers `FileGameDataCatalog` as the `IGameDataCatalog` multiplexer.
3. `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs:12` indexes catalogs by `GameEra`.
4. `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs:25` initializes enabled catalogs; Nijiiro initializes first, then non-Nijiiro eras initialize in parallel at `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs:32`.
5. Handlers and admin controllers retrieve era catalogs through `Application/Common/CatalogExtensions.cs`.

**State Management:**
- EF Core state is scoped per request through `Infrastructure/DependencyInjection.cs:43` and exposed through the `ITaikoDbContext` port at `Infrastructure/DependencyInjection.cs:58`.
- Catalog state is singleton, initialized once at startup, and accessed through `IGameDataCatalog` in `Application/Abstractions/IGameDataCatalog.cs`.
- WebUI client-side cached state lives in singleton services such as `TaikoWebUI/Services/GameDataService.cs`; authentication state lives in `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
- Global logging state is initialized through Serilog in `Host/Program.cs`; CSV side-channel formatting is implemented in `Host/Logging/CsvFormatter.cs`.

## Key Abstractions

**GameEra:**
- Purpose: Selects era-specific routes, catalogs, handlers, DB sets, and WebUI APIs.
- Examples: `Domain/Enums/GameEra.cs`, `Host/Program.cs`, `Application/Handlers/BaidQuery.cs`, `TaikoWebUI/Utilities/WebUiEra.cs`
- Pattern: Enum-driven dispatch with partial implementation files and application-part gating.

**Mediator Request/Handler:**
- Purpose: Encapsulates game protocol use cases behind `IRequest<T>` and scoped handlers.
- Examples: `Application/Handlers/BaidQuery.cs`, `Application/Handlers/UpdatePlayResultCommand.cs`, `Application/DependencyInjection.cs`
- Pattern: `readonly record struct` request plus handler; central file dispatches by era and era partials implement behavior.

**Common DTOs:**
- Purpose: Provide version-agnostic shapes between protocol adapters and application handlers.
- Examples: `Application/Dtos/CommonBaidResponse.cs`, `Application/Dtos/CommonPlayResultData.cs`, `Application/Dtos/CommonUserDataResponse.cs`
- Pattern: Shared base files plus optional `.Nijiiro.cs`, `.Green.cs`, and `.Blue.cs` partial extensions.

**ITaikoDbContext:**
- Purpose: Application-facing persistence port exposing era-partitioned DbSets.
- Examples: `Application/Abstractions/ITaikoDbContext.cs`, `Application/Abstractions/ITaikoDbContext.Green.cs`, `Infrastructure/Persistence/TaikoDbContext.cs`
- Pattern: Partial interface mirrored by partial EF Core implementation files.

**IGameDataCatalog:**
- Purpose: Multiplexes immutable era-specific filesystem catalogs.
- Examples: `Application/Abstractions/IGameDataCatalog.cs`, `Application/Abstractions/INijiiroCatalog.cs`, `Application/Abstractions/IGreenCatalog.cs`, `Application/Abstractions/IBlueCatalog.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`
- Pattern: `For(GameEra)` plus typed convenience extensions in `Application/Common/CatalogExtensions.cs`.

**Controller Bases:**
- Purpose: Standardize lazy access to `IMediator` and logging for controllers.
- Examples: `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`, `Adapters.AdminApi/BaseAdminController.cs`
- Pattern: Generic base controller with `HttpContext.RequestServices` lookup.

**Mapperly Mappers:**
- Purpose: Translate wire/contracts DTOs to application DTOs without reflection-based runtime mapping.
- Examples: `Adapters.GameProtocol.WwR08/Mappers/BaidResponseMapper.cs`, `Adapters.GameProtocol.Green/Mappers/BaidResponseMapper.cs`, `Adapters.AdminApi/Mapping/AuthConfigMapper.cs`
- Pattern: `[Mapper] public static partial class ...` with generated or manually post-processed mapping methods.

## Entry Points

**ASP.NET Core Host:**
- Location: `Host/Program.cs`
- Triggers: `dotnet run --project Host`, published `TaikoLocalServer.exe`, or host process startup.
- Responsibilities: Configuration, DI, migrations, catalogs, middleware, controllers, Blazor static files, and fallback routing.

**Blazor WebAssembly Client:**
- Location: `TaikoWebUI/Program.cs`
- Triggers: Browser loads static files served by `Host/Program.cs`.
- Responsibilities: Fetch WebUI config and auth config, configure MudBlazor/localization/auth, initialize game-data caches, and run routed pages.

**Game Protocol Controllers:**
- Location: `Adapters.GameProtocol.WwR08/Controllers/`, `Adapters.GameProtocol.CnR00/Controllers/`, `Adapters.GameProtocol.Green/Controllers/`, `Adapters.GameProtocol.Blue/Controllers/`
- Triggers: Game cabinet HTTP POSTs under `/v12r08_ww/chassis`, `/v12r00_cn/chassis`, `/v11r01/chassis`, `/v10r03/chassis`, and shared `/v01r00/chassis`.
- Responsibilities: Deserialize wire payloads, map to common DTOs or commands, send through Mediator, and serialize protobuf responses.

**Admin API Controllers:**
- Location: `Adapters.AdminApi/Controllers/`
- Triggers: `TaikoWebUI/` HTTP calls under `/api/...`.
- Responsibilities: Serve users, credentials, settings, play history, game data, customization catalogs, favorites, auth config, login/register, and era-specific admin data.

**AllNet/Mucha Controllers and Middleware:**
- Location: `Adapters.AllnetMucha/Controllers/`, `Adapters.AllnetMucha/Middleware/AllNetRequestMiddleware.cs`
- Triggers: Cabinet lifecycle endpoints under `/sys/servlet/PowerOn`, `/mucha_front/...`, `/mucha_activation/...`, and `/v1/s12-jp-dev/garm...`.
- Responsibilities: Decode AllNet PowerOn form data, answer lifecycle/updater calls, and use configured Mucha/Game URLs.

**CLI Tools:**
- Location: `GreenCatalogExtractor/Program.cs`, `LocalSaveModScoreMigrator/Program.cs`
- Triggers: `dotnet run --project GreenCatalogExtractor` and `dotnet run --project LocalSaveModScoreMigrator`.
- Responsibilities: Extract Green catalogs and import local-save-mod score dumps using shared infrastructure.

## Architectural Constraints

- **Threading:** ASP.NET Core handles concurrent requests; `TaikoDbContext` is scoped in `Infrastructure/DependencyInjection.cs:43`, while catalog implementations are singleton and initialized through `Host/Program.cs:182`.
- **Global state:** `Log.Logger` is process-global in `Host/Program.cs`; `IGameDataCatalog` is singleton in `Infrastructure/DependencyInjection.cs`; WebUI uses singleton cached services in `TaikoWebUI/Program.cs:41`.
- **Circular imports:** Project-reference cycles are not detected in `TaikoLocalServer.slnx` or the `.csproj` references; keep dependencies flowing inward through `Application/Abstractions/` and outward through `Infrastructure/`.
- **Era enablement:** `Host/Program.cs:136` through `Host/Program.cs:145` strips disabled adapter assemblies from MVC application parts, so disabled-era controllers must not be assumed routable.
- **Configuration:** Runtime configuration files are split under `Host/Configurations/`; do not move operational settings into `Host/appsettings.json`.
- **Local data:** `Host/wwwroot/data/green/data` and generated Green catalog JSON files are ignored by `.gitignore`; code should tolerate operator-local data paths and use `Infrastructure/GameDataCatalog/PathHelper.cs`.
- **Serialization:** Game protocol endpoints use protobuf-net registered by `Host/Program.cs:128`; some Green and Blue helpers use fixed-width byte contracts in `Application/Common/GreenProtocolBytes.cs` and `Application/Common/BlueProtocolBytes.cs`.

## Anti-Patterns

### Cross-Era Logic Leakage

**What happens:** Era-specific fields or behavior are placed in shared DTO/handler files such as `Application/Dtos/CommonPlayResultData.cs` or the central dispatcher in `Application/Handlers/UpdatePlayResultCommand.cs`.
**Why it's wrong:** Shared files are used by Nijiiro, Green, and Blue paths; era-only protocol fields can corrupt another era's behavior.
**Do this instead:** Put era-only fields and behavior in matching partial files such as `Application/Dtos/CommonPlayResultData.Green.cs`, `Application/Dtos/CommonPlayResultData.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, or `Application/Handlers/UpdatePlayResultCommand.Blue.cs`.

### Adapter-to-Adapter Coupling

**What happens:** A protocol adapter imports another era adapter directly, for example Green code referencing `Adapters.GameProtocol.Blue/` or Nijiiro code referencing `Adapters.GameProtocol.Green/`.
**Why it's wrong:** `Host/Program.cs` can remove disabled-era application parts; cross-adapter references make era opt-in brittle.
**Do this instead:** Move shared protocol plumbing to `Adapters.GameProtocol.Shared/` or shared use-case contracts to `Application/`, then map through adapter-local mappers.

### Direct Host Path Construction

**What happens:** Code hardcodes `Host/wwwroot/data/<era>` or relative `wwwroot` paths outside catalog/path helpers.
**Why it's wrong:** Published runtime data lives beside the executable, and source-checkout paths differ from publish paths.
**Do this instead:** Resolve data roots through `Infrastructure/GameDataCatalog/PathHelper.cs`, `Infrastructure/GameDataCatalog/Green/GreenGameDataPaths.cs`, or `Infrastructure/GameDataCatalog/Blue/BlueGameDataPaths.cs`.

### Config in the Wrong Surface

**What happens:** New server configuration is added to `Host/appsettings.json` or inline controller constants while the rest of the host loads `Host/Configurations/*.json`.
**Why it's wrong:** `Host/Program.cs:47` through `Host/Program.cs:52` explicitly load operational config files and `Host/Host.csproj` copies those files to publish output.
**Do this instead:** Add a typed settings model under `Application/Settings/` or `Infrastructure/Settings/`, bind it in the relevant `DependencyInjection.cs`, and add the copied config file entry in `Host/Host.csproj`.

## Error Handling

**Strategy:** Fail fast for invalid startup state, return HTTP errors for invalid admin input, and let unsupported internal era states throw explicit exceptions.

**Patterns:**
- Startup refuses invalid enabled-era configuration in `Host/Program.cs` before registering runtime routes.
- Unknown routes and non-401 client failures are logged after request execution in `Host/Program.cs`.
- Disabled catalog access throws `InvalidOperationException` from `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs:19`.
- Admin controllers return `BadRequest`, `NotFound`, `Forbid`, or `NoContent` directly, as shown in `Adapters.AdminApi/Controllers/UsersController.cs`.
- Game protocol handlers throw for unsupported `GameEra` values in dispatcher files such as `Application/Handlers/BaidQuery.cs:18`.

## Cross-Cutting Concerns

**Logging:** Serilog is configured in `Host/Program.cs`; controllers use `ILogger<T>` from `BaseProtocolController<T>` and `BaseAdminController<T>`, and CSV logging uses `Host/Logging/CsvFormatter.cs`.

**Validation:** Startup options validation is wired through `Infrastructure/DependencyInjection.cs` with `Application/Settings/ServerSettingsOptionsValidationExtensions.cs`; route-era parsing uses `Adapters.AdminApi/Controllers/EraRoute.cs`; controller input checks live in controllers such as `Adapters.AdminApi/Controllers/UsersController.cs`.

**Authentication:** JWT bearer auth and admin policies are registered in `Infrastructure/DependencyInjection.cs`; `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs` disables policy enforcement when local-mode auth is off; WebUI mirrors the server auth config in `TaikoWebUI/Program.cs`.

**Compression and Protobuf:** Protobuf formatters are registered in `Host/Program.cs`; gzip/header helpers live in `Adapters.GameProtocol.Shared/Compression/`; content-type fallback for game protobuf POSTs lives in `Host/Program.cs:273`.

**Era Routing:** Runtime route registration is controlled by `Host/Program.cs`; admin era parsing is centralized in `Adapters.AdminApi/Controllers/EraRoute.cs`; WebUI API/user URLs should be built with `TaikoWebUI/Utilities/WebUiEra.cs`.

---

*Architecture analysis: 2026-05-28*
