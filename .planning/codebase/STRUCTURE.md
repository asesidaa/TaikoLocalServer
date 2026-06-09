# Codebase Structure

**Analysis Date:** 2026-05-28

## Directory Layout

```text
TaikoLocalServer/
|-- Adapters.AdminApi/              # Admin REST API consumed by Blazor WebUI
|-- Adapters.AllnetMucha/           # AllNet, Mucha, activation, and Garmc HTTP surfaces
|-- Adapters.GameProtocol.Shared/   # Shared game-protocol base controllers, startup auth, compression
|-- Adapters.GameProtocol.WwR08/    # Nijiiro 39.06 WW protobuf routes under /v12r08_ww
|-- Adapters.GameProtocol.CnR00/    # Nijiiro CHN protobuf routes under /v12r00_cn
|-- Adapters.GameProtocol.Green/    # Green AC15 protobuf routes under /v11r01
|-- Adapters.GameProtocol.Blue/     # Blue AC15 protobuf routes under /v10r03
|-- Application/                    # Use cases, ports, Common* DTOs, catalog contracts, settings
|-- Contracts.AdminApi/             # Admin/WebUI DTO contracts and auth helpers
|-- Domain/                         # Entities, enums, constants; no outbound project references
|-- Infrastructure/                 # EF Core, SQLite, filesystem catalog, JWT, time, migrations
|-- Host/                           # ASP.NET Core host, config, certificates, wwwroot runtime data
|-- TaikoWebUI/                     # Blazor WebAssembly admin UI
|-- Tests/                          # xUnit tests grouped by feature/era
|-- GreenCatalogExtractor/          # CLI for deriving Green customization/catalog files
|-- LocalSaveModScoreMigrator/      # CLI for importing local-save-mod JSON into SQLite
|-- docs/                           # Agent docs, evidence, specs, plans
|-- tools/                          # Small checked-in helper tools
|-- .planning/codebase/             # Generated codebase maps
|-- Directory.Build.props           # Shared .NET build defaults
|-- Directory.Packages.props        # Central package versions
|-- global.json                     # Pinned .NET SDK band
`-- TaikoLocalServer.slnx           # Solution project list
```

## Directory Purposes

**`Domain/`:**
- Purpose: Put persistent entities, enums, and domain constants here.
- Contains: `Domain/Entities/`, `Domain/Enums/`, `Domain/DomainConstants.cs`, `Domain/Domain.csproj`
- Key files: `Domain/Enums/GameEra.cs`, `Domain/Entities/UserDatum.cs`, `Domain/Entities/UserSaveDataGreen.cs`, `Domain/Entities/UserSaveDataBlue.cs`

**`Contracts.AdminApi/`:**
- Purpose: Put DTOs shared by admin API and WebUI here.
- Contains: `Contracts.AdminApi/Requests/`, `Contracts.AdminApi/Responses/`, `Contracts.AdminApi/ViewModels/`, `Contracts.AdminApi/ServerData/`, `Contracts.AdminApi/Converters/`, `Contracts.AdminApi/Authorization/`
- Key files: `Contracts.AdminApi/Contracts.AdminApi.csproj`, `Contracts.AdminApi/Authorization/AuthPolicies.cs`, `Contracts.AdminApi/Converters/PlaySettingConverter.cs`

**`Application/`:**
- Purpose: Put use-case handlers, application ports, common protocol DTOs, catalog interfaces, application settings, and server-only JSON shapes here.
- Contains: `Application/Handlers/`, `Application/Abstractions/`, `Application/Dtos/`, `Application/Common/`, `Application/Catalog/`, `Application/ServerData/`, `Application/Settings/`
- Key files: `Application/DependencyInjection.cs`, `Application/Handlers/BaidQuery.cs`, `Application/Abstractions/ITaikoDbContext.cs`, `Application/Abstractions/IGameDataCatalog.cs`, `Application/Common/CatalogExtensions.cs`

**`Infrastructure/`:**
- Purpose: Put concrete implementations of application ports and all durable I/O here.
- Contains: `Infrastructure/Persistence/`, `Infrastructure/Persistence/Migrations/`, `Infrastructure/GameDataCatalog/`, `Infrastructure/Identity/`, `Infrastructure/Time/`, `Infrastructure/Settings/`
- Key files: `Infrastructure/DependencyInjection.cs`, `Infrastructure/Persistence/TaikoDbContext.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/PathHelper.cs`, `Infrastructure/Identity/JwtTokenService.cs`

**`Adapters.AdminApi/`:**
- Purpose: Put `/api/...` admin HTTP endpoints and admin-specific mapping here.
- Contains: `Adapters.AdminApi/Controllers/`, `Adapters.AdminApi/Authorization/`, `Adapters.AdminApi/Mapping/`, `Adapters.AdminApi/BaseAdminController.cs`
- Key files: `Adapters.AdminApi/Controllers/AuthController.cs`, `Adapters.AdminApi/Controllers/GameDataController.cs`, `Adapters.AdminApi/Controllers/UsersController.cs`, `Adapters.AdminApi/Controllers/EraRoute.cs`

**`Adapters.GameProtocol.Shared/`:**
- Purpose: Put game-protocol code that is genuinely shared across game adapters.
- Contains: `Adapters.GameProtocol.Shared/Controllers/`, `Adapters.GameProtocol.Shared/Compression/`, `Adapters.GameProtocol.Shared/Wire/`
- Key files: `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`, `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`, `Adapters.GameProtocol.Shared/Compression/GZipBytesUtil.cs`

**`Adapters.GameProtocol.WwR08/`:**
- Purpose: Put Nijiiro 39.06 WW wire types, controllers, and mappers here.
- Contains: `Adapters.GameProtocol.WwR08/Controllers/`, `Adapters.GameProtocol.WwR08/Mappers/`, `Adapters.GameProtocol.WwR08/Wire/`
- Key files: `Adapters.GameProtocol.WwR08/Controllers/BaidController.cs`, `Adapters.GameProtocol.WwR08/Mappers/BaidResponseMapper.cs`, `Adapters.GameProtocol.WwR08/Wire/Game.cs`

**`Adapters.GameProtocol.CnR00/`:**
- Purpose: Put Nijiiro CHN wire types, controllers, and mappers here.
- Contains: `Adapters.GameProtocol.CnR00/Controllers/`, `Adapters.GameProtocol.CnR00/Mappers/`, `Adapters.GameProtocol.CnR00/Wire/`
- Key files: `Adapters.GameProtocol.CnR00/Controllers/BaidController.cs`, `Adapters.GameProtocol.CnR00/Mappers/BaidResponseMapper.cs`, `Adapters.GameProtocol.CnR00/Wire/Game.cs`

**`Adapters.GameProtocol.Green/`:**
- Purpose: Put Green AC15 wire types, controllers, mappers, and Green-specific protocol decoding here.
- Contains: `Adapters.GameProtocol.Green/Controllers/`, `Adapters.GameProtocol.Green/Mappers/`, `Adapters.GameProtocol.Green/Wire/`
- Key files: `Adapters.GameProtocol.Green/Controllers/BaidController.cs`, `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Green/GreenPlayResultPayloadDecoder.cs`

**`Adapters.GameProtocol.Blue/`:**
- Purpose: Put Blue AC15 wire types, controllers, and mappers here.
- Contains: `Adapters.GameProtocol.Blue/Controllers/`, `Adapters.GameProtocol.Blue/Mappers/`, `Adapters.GameProtocol.Blue/Wire/`
- Key files: `Adapters.GameProtocol.Blue/Controllers/BaidController.cs`, `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`

**`Adapters.AllnetMucha/`:**
- Purpose: Put AllNet/Mucha/Garmc lifecycle endpoints and middleware here.
- Contains: `Adapters.AllnetMucha/Controllers/AmAuth/`, `Adapters.AllnetMucha/Controllers/AmUpdater/`, `Adapters.AllnetMucha/Controllers/Garmc/`, `Adapters.AllnetMucha/Controllers/MuchaActivation/`, `Adapters.AllnetMucha/Middleware/`, `Adapters.AllnetMucha/Wire/`
- Key files: `Adapters.AllnetMucha/DependencyInjection.cs`, `Adapters.AllnetMucha/Middleware/AllNetRequestMiddleware.cs`, `Adapters.AllnetMucha/Controllers/AmAuth/PowerOnController.cs`

**`Host/`:**
- Purpose: Put the runnable ASP.NET Core process, runtime configuration, certificates, static data, and publish content rules here.
- Contains: `Host/Program.cs`, `Host/Configurations/`, `Host/Certificates/`, `Host/Logging/`, `Host/wwwroot/`, `Host/Properties/`
- Key files: `Host/Host.csproj`, `Host/Program.cs`, `Host/Configurations/ServerSettings.json`, `Host/Configurations/AuthSettings.json`, `Host/wwwroot/data/`

**`TaikoWebUI/`:**
- Purpose: Put Blazor WASM pages, components, services, localization, WebUI settings, utilities, and static assets here.
- Contains: `TaikoWebUI/Pages/`, `TaikoWebUI/Pages/Dialogs/`, `TaikoWebUI/Components/`, `TaikoWebUI/Shared/`, `TaikoWebUI/Services/`, `TaikoWebUI/Authorization/`, `TaikoWebUI/Utilities/`, `TaikoWebUI/Localization/`, `TaikoWebUI/wwwroot/`
- Key files: `TaikoWebUI/Program.cs`, `TaikoWebUI/App.razor`, `TaikoWebUI/Services/GameDataService.cs`, `TaikoWebUI/Utilities/WebUiEra.cs`

**`Tests/`:**
- Purpose: Put xUnit tests here, grouped by era or subsystem.
- Contains: `Tests/Green/`, `Tests/Blue/`, `Tests/WebUi/`, `Tests/AllnetMucha/`, `Tests/Ac15/`
- Key files: `Tests/Tests.csproj`, `Tests/Green/GreenHandlerFixture.cs`, `Tests/Blue/BlueHandlerFixture.cs`, `Tests/WebUi/GameDataServiceTests.cs`

**`GreenCatalogExtractor/`:**
- Purpose: Put the Green catalog extraction CLI here.
- Contains: `GreenCatalogExtractor/Program.cs`, `GreenCatalogExtractor/GreenCatalogExtractor.csproj`
- Key files: `GreenCatalogExtractor/Program.cs`

**`LocalSaveModScoreMigrator/`:**
- Purpose: Put the local-save-mod score import CLI here.
- Contains: `LocalSaveModScoreMigrator/Program.cs`, `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`
- Key files: `LocalSaveModScoreMigrator/Program.cs`

**`docs/`:**
- Purpose: Put agent setup docs, evidence, specs, and plans here.
- Contains: `docs/agents/`, `docs/green-client-evidence/`, `docs/superpowers/specs/`, `docs/superpowers/plans/`
- Key files: `docs/agents/domain.md`, `docs/superpowers/plans/`, `docs/superpowers/specs/`

**`.tools/`, `.worktrees/`, `artifacts/`, `runs/`:**
- Purpose: Keep local analysis output, local worktrees, smoke/debug artifacts, and run output out of production code.
- Contains: Machine-local reverse-engineering/tool state and generated artifacts.
- Key files: `.gitignore` ignores `.tools/`, `.worktrees/`, and `artifacts/`; do not add implementation code here.

## Key File Locations

**Entry Points:**
- `Host/Program.cs`: ASP.NET Core composition root and runtime entry point.
- `TaikoWebUI/Program.cs`: Blazor WASM client startup.
- `GreenCatalogExtractor/Program.cs`: Green extraction CLI entry point.
- `LocalSaveModScoreMigrator/Program.cs`: score migration CLI entry point.

**Configuration:**
- `Directory.Build.props`: shared target framework, nullable, implicit using, and language version defaults.
- `Directory.Packages.props`: central NuGet package versions.
- `global.json`: .NET SDK roll-forward/pin configuration.
- `Host/Configurations/Kestrel.json`: host binding configuration; existence only, do not quote local values into generated docs.
- `Host/Configurations/Database.json`: database file configuration; existence only, do not quote local values into generated docs.
- `Host/Configurations/ServerSettings.json`: runtime feature/era settings; existence only, do not quote local values into generated docs.
- `Host/Configurations/DataSettings.json`: data filename settings; existence only, do not quote local values into generated docs.
- `Host/Configurations/AuthSettings.json`: auth/JWT settings; secret-bearing, note existence only.
- `Host/Configurations/Logging.json`: Serilog settings; existence only.
- `TaikoWebUI/wwwroot/appsettings.json`: WebUI client settings fetched by `TaikoWebUI/Program.cs`.

**Core Logic:**
- `Application/Handlers/`: game protocol use cases.
- `Application/Abstractions/`: application ports and catalog interfaces.
- `Application/Dtos/`: `Common*` DTOs shared between adapters and handlers.
- `Application/Common/`: era helpers, byte codecs, catalog extensions, save-data defaults, and protocol utilities.
- `Infrastructure/Persistence/TaikoDbContext.cs`: EF Core context implementation.
- `Infrastructure/Persistence/Migrations/`: EF Core migrations.
- `Infrastructure/GameDataCatalog/`: filesystem catalog loaders and era catalog implementations.
- `Adapters.GameProtocol.*/Controllers/`: game client endpoint controllers.
- `Adapters.GameProtocol.*/Mappers/`: wire-to-common and common-to-wire mapping.
- `Adapters.AdminApi/Controllers/`: WebUI/admin REST endpoints.
- `TaikoWebUI/Pages/`: page route components and code-behind files.
- `TaikoWebUI/Services/`: WebUI API/auth/data services.

**Testing:**
- `Tests/Tests.csproj`: test project references all runtime layers.
- `Tests/Green/`: Green protocol-byte, catalog/parser, mapper-classification, handler, and persistence-boundary tests.
- `Tests/Blue/`: Blue catalog/parser, mapper-classification, handler, and persistence-boundary tests.
- `Tests/WebUi/`: WebUI services and page behavior tests.
- `Tests/AllnetMucha/`: AllNet/Mucha controller tests.
- `Tests/Ac15/`: shared AC15 catalog loader tests.

## Naming Conventions

**Files:**
- Use one public type per `.cs` file where practical: `Application/Handlers/BaidQuery.cs`, `Domain/Entities/Card.cs`, `Adapters.AdminApi/Controllers/UsersController.cs`.
- Use era suffixes for era-specific types and partials: `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/BaidQuery.Blue.cs`, `Application/Abstractions/ITaikoDbContext.Nijiiro.cs`, `Infrastructure/Persistence/TaikoDbContext.Green.cs`.
- Use `Common*` for adapter-neutral application DTOs: `Application/Dtos/CommonBaidResponse.cs`, `Application/Dtos/CommonPlayResultData.Green.cs`.
- Use `*Controller.cs` under adapter `Controllers/`: `Adapters.GameProtocol.Green/Controllers/BaidController.cs`, `Adapters.AdminApi/Controllers/GameDataController.cs`.
- Use `*Mapper.cs` under adapter `Mappers/` or `Mapping/`: `Adapters.GameProtocol.WwR08/Mappers/BaidResponseMapper.cs`, `Adapters.AdminApi/Mapping/AuthConfigMapper.cs`.
- Use `*Loader.cs` for filesystem catalog loaders: `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs`, `Infrastructure/GameDataCatalog/Blue/BlueMusicInfoLoader.cs`.
- Use timestamped EF migration names under `Infrastructure/Persistence/Migrations/`.

**Directories:**
- Use project-per-layer directories at the repo root: `Domain/`, `Application/`, `Infrastructure/`, `Host/`, `TaikoWebUI/`, `Adapters.*`.
- Use adapter directory names of the form `Adapters.<Surface>` or `Adapters.GameProtocol.<Version>`.
- Use `Controllers/`, `Mappers/`, and `Wire/` inside protocol adapters.
- Use `Requests/`, `Responses/`, `ViewModels/`, and `ServerData/` inside `Contracts.AdminApi/`.
- Use era subdirectories for catalog implementation code: `Infrastructure/GameDataCatalog/Nijiiro/`, `Infrastructure/GameDataCatalog/Green/`, `Infrastructure/GameDataCatalog/Blue/`, `Infrastructure/GameDataCatalog/Ac15/`.
- Use test subdirectories by era/subsystem: `Tests/Green/`, `Tests/Blue/`, `Tests/WebUi/`.

## Where to Add New Code

**New Game Protocol Endpoint:**
- Controller: `Adapters.GameProtocol.<EraOrVersion>/Controllers/`
- Wire request/response: `Adapters.GameProtocol.<EraOrVersion>/Wire/`
- Mapper: `Adapters.GameProtocol.<EraOrVersion>/Mappers/`
- Application request/handler: `Application/Handlers/`
- Common DTO: `Application/Dtos/`
- Tests: matching folder under `Tests/Green/`, `Tests/Blue/`, or Nijiiro-focused tests in `Tests/`

**New Era-Specific Handler Behavior:**
- Central dispatch: `Application/Handlers/<UseCase>.cs`
- Era implementation: `Application/Handlers/<UseCase>.Nijiiro.cs`, `Application/Handlers/<UseCase>.Green.cs`, or `Application/Handlers/<UseCase>.Blue.cs`
- Era-only DTO fields: `Application/Dtos/<CommonDto>.<Era>.cs`
- Era persistence port: `Application/Abstractions/ITaikoDbContext.<Era>.cs`
- EF implementation: `Infrastructure/Persistence/TaikoDbContext.<Era>.cs`

**New Admin API Endpoint:**
- Controller: `Adapters.AdminApi/Controllers/`
- Request/response/view model DTOs: `Contracts.AdminApi/Requests/`, `Contracts.AdminApi/Responses/`, `Contracts.AdminApi/ViewModels/`
- WebUI caller: `TaikoWebUI/Services/` or page code-behind in `TaikoWebUI/Pages/`
- Tests: `Tests/WebUi/` for client behavior or era/subsystem folders for server behavior

**New WebUI Page or Component:**
- Page markup/code-behind: `TaikoWebUI/Pages/`
- Dialogs: `TaikoWebUI/Pages/Dialogs/`
- Reusable visual components: `TaikoWebUI/Components/` or `TaikoWebUI/Shared/`
- Client service/API cache: `TaikoWebUI/Services/`
- Era URL logic: `TaikoWebUI/Utilities/WebUiEra.cs`

**New Domain Entity or Persisted Field:**
- Entity: `Domain/Entities/`
- Enum: `Domain/Enums/`
- DbContext port: `Application/Abstractions/ITaikoDbContext*.cs`
- EF model configuration: `Infrastructure/Persistence/TaikoDbContext*.cs`
- Migration: `Infrastructure/Persistence/Migrations/`

**New Filesystem Catalog Data:**
- Interface/contract: `Application/Abstractions/` and `Application/Catalog/`
- Server-only data shape: `Application/ServerData/`
- WebUI-consumed data shape: `Contracts.AdminApi/ServerData/`
- Loader and path code: `Infrastructure/GameDataCatalog/<Era>/`
- Static data files: `Host/wwwroot/data/<era>/` or `Host/wwwroot/data/shared/`

**New Port/External Implementation:**
- Port interface: `Application/Abstractions/`
- Implementation: `Infrastructure/`
- Registration: `Infrastructure/DependencyInjection.cs`
- Settings type: `Application/Settings/` or `Infrastructure/Settings/`
- Runtime config file/copy rule: `Host/Configurations/` and `Host/Host.csproj`

**New Shared Protocol Helper:**
- Shared across multiple game adapters: `Adapters.GameProtocol.Shared/`
- Shared across handlers/application logic: `Application/Common/`
- Era-specific protocol byte helper: `Application/Common/GreenProtocolBytes.cs` or `Application/Common/BlueProtocolBytes.cs`

**New CLI Utility:**
- Green catalog extraction additions: `GreenCatalogExtractor/`
- Score import additions: `LocalSaveModScoreMigrator/`
- General small checked-in scripts: `tools/`
- Local one-off reverse-engineering artifacts: `.tools/` only when intentionally untracked

## Special Directories

**`Host/Configurations/`:**
- Purpose: Runtime JSON configuration loaded explicitly by `Host/Program.cs`.
- Generated: No.
- Committed: Yes, but `AuthSettings.json` is secret-bearing and should not be quoted in documentation.

**`Host/Certificates/`:**
- Purpose: Local certificate files used by configured Kestrel endpoints.
- Generated: No.
- Committed: Present in the tree; treat `.pfx` contents as secret-bearing and do not quote.

**`Host/wwwroot/data/`:**
- Purpose: Runtime game data, shared data, and SQLite DB location.
- Generated: Mixed; source-controlled JSON exists alongside operator-provided and generated files.
- Committed: Mixed. `.gitignore` excludes `Host/wwwroot/data/green/data` and generated Green customization JSON files.

**`Infrastructure/Persistence/Migrations/`:**
- Purpose: EF Core migration history for `TaikoDbContext`.
- Generated: Yes, by EF tooling.
- Committed: Yes.

**`TaikoWebUI/wwwroot/`:**
- Purpose: Blazor client static assets, appsettings, CSS, images, JavaScript, and client data assets.
- Generated: Mixed; build output is not committed, source static assets are committed.
- Committed: Yes for source assets.

**`.tools/`:**
- Purpose: Local reverse-engineering and analysis helper state.
- Generated: Yes/local.
- Committed: No, ignored by `.gitignore`.

**`.worktrees/`:**
- Purpose: Local git worktrees for isolated feature work.
- Generated: Yes/local.
- Committed: No, ignored by `.gitignore`.

**`artifacts/`:**
- Purpose: Local debug/smoke/review artifacts.
- Generated: Yes/local.
- Committed: No, ignored by `.gitignore`.

**`proto/`:**
- Purpose: Generated or local protobuf-related artifacts.
- Generated: Yes/local.
- Committed: No, ignored by `.gitignore`.

**`docs/superpowers/`:**
- Purpose: Planning and specification artifacts used by workflow agents.
- Generated: Yes by planning workflows.
- Committed: Yes when workflow artifacts are part of project history.

---

*Structure analysis: 2026-05-28*
