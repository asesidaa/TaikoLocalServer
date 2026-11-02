# Codebase Structure

**Analysis Date:** 2026-06-11

## Directory Layout

```text
TaikoLocalServer/
|-- Adapters.AdminApi/              # Admin REST API consumed by Blazor WebUI
|-- Adapters.AllnetMucha/           # AllNet, Mucha, activation, updater, and GARM endpoints
|-- Adapters.GameProtocol.Shared/   # Shared game-protocol base controllers, startup/version routes, compression
|-- Adapters.GameProtocol.WwR08/    # Nijiiro WW protobuf routes under /v12r08_ww
|-- Adapters.GameProtocol.CnR00/    # Nijiiro CN protobuf routes under /v12r00_cn
|-- Adapters.GameProtocol.Green/    # Green AC15 routes, wire DTOs, mappers under /v11r01
|-- Adapters.GameProtocol.Blue/     # Blue AC15 routes, wire DTOs, mappers under /v10r03
|-- Adapters.GameProtocol.Yellow/   # Yellow AC15 routes, wire DTOs, mappers under /v09r02
|-- Adapters.GameProtocol.Red/      # Red AC15 adapter shell and generated wire DTOs
|-- Application/                    # Mediator use cases, ports, Common DTOs, AC15 shared core
|   |-- Ac15/                       # Shared AC15 services, profiles, records, Mapperly projections
|   |-- Handlers/                   # Use-case dispatchers and era partial implementations
|   |-- Dtos/                       # Common protocol DTOs between adapters and handlers
|   |-- Catalog/                    # Catalog contracts and typed catalog DTOs
|   `-- Abstractions/               # Ports such as ITaikoDbContext and IGameDataCatalog
|-- Contracts.AdminApi/             # DTO contracts shared by AdminApi and WebUI
|-- Domain/                         # Persistent entities, enums, constants, AC15 row-shape interfaces
|-- Infrastructure/                 # EF Core, filesystem catalogs, settings, identity, clock, migrations
|   |-- Persistence/                # TaikoDbContext partials and EF migrations
|   `-- GameDataCatalog/            # Era catalogs plus shared AC15 loaders
|-- Host/                           # ASP.NET Core host, runtime config, certificates, wwwroot data
|-- TaikoWebUI/                     # Blazor WebAssembly admin UI
|-- Tests/                          # xUnit tests grouped by AC15 shared behavior and era/subsystem
|-- GreenCatalogExtractor/          # Green catalog extraction CLI
|-- LocalSaveModScoreMigrator/      # Local-save-mod score import CLI
|-- proto/                          # Protocol schema inputs for generated Wire models
|-- docs/                           # Agent docs, specs, evidence, plans
|-- tools/                          # Checked-in helper tools
|-- .tools/                         # Local reverse-engineering material and analysis output
|-- .planning/codebase/             # Generated codebase maps
|-- Directory.Build.props           # Shared .NET build defaults
|-- Directory.Packages.props        # Central NuGet package versions
|-- global.json                     # .NET SDK pin/roll-forward policy
`-- TaikoLocalServer.slnx           # Solution project list
```

## Directory Purposes

**`Application/Ac15/`:**
- Purpose: Put reusable AC15 behavior here only when Green, Blue, and Yellow semantics match.
- Contains: Shared services (`Ac15NormalPlayWriter`, `Ac15DaniWriter`, `Ac15UserDataService`, `Ac15InitialDataService`), profile records, table records, Mapperly projections, and era user-data adapters.
- Key files: `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Ac15/Ac15DaniWriter.cs`, `Application/Ac15/Ac15ItemShopPurchase.cs`, `Application/Ac15/Ac15CatalogSnapshotFactory.cs`

**`Application/Handlers/`:**
- Purpose: Put Mediator requests, use-case dispatch, and era partial behavior here.
- Contains: Unsuffixed dispatch files such as `Application/Handlers/UpdatePlayResultCommand.cs` plus `.Green.cs`, `.Blue.cs`, `.Yellow.cs`, and `.Nijiiro.cs` partial files.
- Key files: `Application/Handlers/UserDataQuery.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`

**`Application/Dtos/`:**
- Purpose: Put adapter-neutral `Common*` DTOs that protocol mappers pass into handlers.
- Contains: Common request/response models and partial extensions for era-specific facts.
- Key files: `Application/Dtos/CommonPlayResultData.cs`, `Application/Dtos/CommonUserDataResponse.cs`, `Application/Dtos/CommonInitialDataCheckResponse.cs`

**`Application/Abstractions/`:**
- Purpose: Put application ports for persistence, catalogs, identity, and time.
- Contains: `ITaikoDbContext` partials, catalog interfaces, `IGameDataCatalog`, `IClock`, `IJwtTokenService`.
- Key files: `Application/Abstractions/ITaikoDbContext.Green.cs`, `Application/Abstractions/ITaikoDbContext.Blue.cs`, `Application/Abstractions/ITaikoDbContext.Yellow.cs`, `Application/Abstractions/IGameDataCatalog.cs`

**`Application/Catalog/`:**
- Purpose: Put typed catalog DTOs used by application handlers and AdminApi.
- Contains: Shared AC15 catalog DTOs under `Application/Catalog/Ac15/`, plus typed Green, Blue, Yellow, Nijiiro catalog DTOs.
- Key files: `Application/Catalog/Ac15/Ac15ItemShopCatalog.cs`, `Application/Catalog/Blue/BlueBattleCatalog.cs`, `Application/Catalog/Yellow/YellowMusicInfoEntry.cs`

**`Domain/`:**
- Purpose: Put persistent entities, canonical enum values, and entity shape contracts here.
- Contains: `Domain/Entities/`, `Domain/Enums/`, `Domain/DomainConstants.cs`
- Key files: `Domain/Entities/UserSaveDataGreen.cs`, `Domain/Entities/UserSaveDataBlue.cs`, `Domain/Entities/UserSaveDataYellow.cs`, `Domain/Entities/IAc15SongPlayDatum.cs`, `Domain/Enums/GameEra.cs`

**`Infrastructure/Persistence/`:**
- Purpose: Put EF Core context, mappings, and migrations here.
- Contains: `TaikoDbContext` partials split by shared identity, Nijiiro, Green, Blue, Yellow, and EF migrations.
- Key files: `Infrastructure/Persistence/TaikoDbContext.cs`, `Infrastructure/Persistence/TaikoDbContext.Green.cs`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, `Infrastructure/Persistence/TaikoDbContext.Yellow.cs`

**`Infrastructure/GameDataCatalog/Ac15/`:**
- Purpose: Put reusable filesystem parsers/loaders for AC15-compatible catalog files.
- Contains: AC15 loaders for musicinfo, tuning, Taikojuku, item shop, event folders, telops, recommendations, movies, and customization composition.
- Key files: `Infrastructure/GameDataCatalog/Ac15/Ac15MusicInfoLoader.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15TaikojukuLoader.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15EventFolderLoader.cs`

**`Infrastructure/GameDataCatalog/Green/`:**
- Purpose: Put Green catalog orchestration, Green path resolution, required-file checks, and Green-only loaders/extractors here.
- Contains: `GreenEraGameDataCatalog`, Green loaders, Green extractor code, Green path helpers.
- Key files: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`, `Infrastructure/GameDataCatalog/Green/GreenGameDataPaths.cs`

**`Infrastructure/GameDataCatalog/Blue/`:**
- Purpose: Put Blue catalog orchestration, Blue path resolution, Blue battle data loading, and Blue customization bootstrap here.
- Contains: `BlueEraGameDataCatalog`, Blue loaders, Blue battle parser/loader, Blue path helpers.
- Key files: `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Blue/BlueBattleDataLoader.cs`, `Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs`

**`Infrastructure/GameDataCatalog/Yellow/`:**
- Purpose: Put Yellow catalog orchestration, Yellow path resolution, and Yellow AC15 loader wrappers here.
- Contains: `YellowEraGameDataCatalog`, Yellow loaders, Yellow required-file checks, Yellow path helpers.
- Key files: `Infrastructure/GameDataCatalog/Yellow/YellowEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Yellow/YellowRequiredDataFiles.cs`, `Infrastructure/GameDataCatalog/Yellow/YellowGameDataPaths.cs`

**`Adapters.GameProtocol.Shared/`:**
- Purpose: Put protocol code genuinely shared across game adapters.
- Contains: `/v01r00/chassis/*` controllers, protocol controller base class, compression helpers, shared startup wire models.
- Key files: `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`, `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs`, `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`

**`Adapters.GameProtocol.Green/`:**
- Purpose: Put Green AC15 transport code here.
- Contains: Green controllers, Green generated `Wire/Game.cs`, Green mapper classes, Green compressed playresult decoder.
- Key files: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Green/GreenPlayResultPayloadDecoder.cs`, `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`

**`Adapters.GameProtocol.Blue/`:**
- Purpose: Put Blue AC15 transport code here.
- Contains: Blue controllers, Blue generated `Wire/Game.cs`, Blue mapper classes, Blue battle/Banacoin/Tokkun route surface.
- Key files: `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs`, `Adapters.GameProtocol.Blue/Mappers/BattleUserDataMappers.cs`

**`Adapters.GameProtocol.Yellow/`:**
- Purpose: Put Yellow AC15 transport code here.
- Contains: Yellow controllers, Yellow generated `Wire/Game.cs`, Yellow mapper classes, Yellow stateless Banacoin-adjacent route surface.
- Key files: `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Yellow/Controllers/UserDataController.cs`, `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`

**`Adapters.GameProtocol.Red/`:**
- Purpose: Put Red AC15 transport code here as it is added.
- Contains: Red adapter project shell and generated Red `Wire/Game.cs` / `Wire/VsInterface.cs` from `proto/red`; Host binding and route controllers are intentionally deferred from the initial shell.
- Key files: `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj`, `Adapters.GameProtocol.Red/Wire/Game.cs`, `Adapters.GameProtocol.Red/Wire/VsInterface.cs`

**`Adapters.AdminApi/`:**
- Purpose: Put WebUI/admin HTTP endpoints and admin-specific mapping here.
- Contains: Controllers, authorization helpers, Mapperly admin mapping, `EraRoute`.
- Key files: `Adapters.AdminApi/Controllers/EraRoute.cs`, `Adapters.AdminApi/Controllers/UserSettingsController.cs`, `Adapters.AdminApi/Controllers/GameDataController.cs`

**`Contracts.AdminApi/`:**
- Purpose: Put DTOs shared by AdminApi and WebUI.
- Contains: `Requests/`, `Responses/`, `ViewModels/`, `ServerData/`, `Converters/`, `Authorization/`.
- Key files: `Contracts.AdminApi/Contracts.AdminApi.csproj`, `Contracts.AdminApi/ViewModels/UserSetting.cs`, `Contracts.AdminApi/Authorization/AuthPolicies.cs`

**`Host/`:**
- Purpose: Put the runnable ASP.NET Core process, runtime configuration, certificates, static data, and publish rules here.
- Contains: `Host/Program.cs`, `Host/Configurations/`, `Host/wwwroot/`, `Host/Certificates/`, `Host/Logging/`.
- Key files: `Host/Host.csproj`, `Host/Program.cs`, `Host/Configurations/ServerSettings.json`, `Host/wwwroot/data/`

**`TaikoWebUI/`:**
- Purpose: Put Blazor WebAssembly pages, components, services, localization, utilities, and static assets here.
- Contains: `Pages/`, `Components/`, `Shared/`, `Services/`, `Authorization/`, `Utilities/`, `Localization/`, `wwwroot/`.
- Key files: `TaikoWebUI/Program.cs`, `TaikoWebUI/Utilities/WebUiEra.cs`, `TaikoWebUI/Services/GameDataService.cs`

**`Tests/`:**
- Purpose: Put xUnit tests here, grouped by shared AC15 behavior, era, or subsystem.
- Contains: `Tests/Ac15/`, `Tests/Green/`, `Tests/Blue/`, `Tests/Yellow/`, `Tests/WebUi/`, `Tests/AllnetMucha/`, `Tests/Architecture/`.
- Key files: `Tests/Ac15/Ac15NormalPlayWriterTests.cs`, `Tests/Ac15/Ac15DaniCapabilityTests.cs`, `Tests/Blue/BlueHandlerFixture.cs`, `Tests/Yellow/YellowHandlerFixture.cs`

**`proto/`:**
- Purpose: Put protocol schema inputs used to generate adapter `Wire/` models.
- Contains: Protobuf schema evidence and generation inputs.
- Key files: `proto/`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`

**`.tools/`:**
- Purpose: Keep local reverse-engineering and analysis material outside runtime code.
- Contains: Local Blue/Yellow evidence and one-off analysis output.
- Key files: `.tools/blue/`, `.tools/yellow/`

## Key File Locations

**Entry Points:**
- `Host/Program.cs`: ASP.NET Core composition root and runtime entry point.
- `TaikoWebUI/Program.cs`: Blazor WebAssembly startup.
- `GreenCatalogExtractor/Program.cs`: Green catalog extraction CLI.
- `LocalSaveModScoreMigrator/Program.cs`: local-save-mod import CLI.

**Configuration:**
- `Directory.Build.props`: shared target framework, nullable, implicit using, and language-version defaults.
- `Directory.Packages.props`: central NuGet package versions.
- `global.json`: .NET SDK pin/roll-forward configuration.
- `Host/Configurations/Kestrel.json`: host binding configuration; note existence only.
- `Host/Configurations/Database.json`: database file configuration; note existence only.
- `Host/Configurations/ServerSettings.json`: era enablement and runtime settings; note existence only.
- `Host/Configurations/DataSettings.json`: runtime data filename settings; note existence only.
- `Host/Configurations/AuthSettings.json`: auth/JWT settings; secret-bearing, note existence only.
- `Host/Configurations/Logging.json`: Serilog settings; note existence only.
- `TaikoWebUI/wwwroot/appsettings.json`: WebUI client settings.

**AC15 Shared Core:**
- `Application/Ac15/Ac15EraProfiles.cs`: AC15 feature, limit, and wire-placement profiles for Green, Blue, Yellow.
- `Application/Ac15/Ac15NormalPlayWriter.cs`: generic normal play, best, favorite, recent write behavior over era rows.
- `Application/Ac15/Ac15DaniWriter.cs`: generic Dani score/stage persistence and save-row update calculation.
- `Application/Ac15/Ac15ItemShopPurchase.cs`: generic AC15 item shop purchase flow.
- `Application/Ac15/Ac15CatalogSnapshotFactory.cs`: projections from typed Green/Blue/Yellow catalogs to shared `Ac15CatalogSnapshot`.
- `Application/Ac15/Ac15UserDataService.cs`: common AC15 userdata response assembly.
- `Application/Ac15/Ac15NormalPlayMapper.cs`, `Application/Ac15/Ac15DaniMapper.cs`, `Application/Ac15/Ac15ItemShopMapper.cs`: Mapperly projections to era-owned entities.

**Era Behavior:**
- `Application/Handlers/UpdatePlayResultCommand.Green.cs`: Green normal play, Dani, ghost side effects, Green stage policy.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`: Blue normal play, Tokkun gate, battle gate, Dani, shop side effects.
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`: Yellow normal play, Tokkun-shaped gate, WaiWai fact logging, Dani.
- `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`: Blue battle playresult behavior.
- `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs`: Blue Tokkun persistence behavior.
- `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs`: Yellow Tokkun persistence behavior.

**Persistence:**
- `Application/Abstractions/ITaikoDbContext.Shared.cs`: shared identity tables.
- `Application/Abstractions/ITaikoDbContext.Green.cs`: Green-owned gameplay tables.
- `Application/Abstractions/ITaikoDbContext.Blue.cs`: Blue-owned gameplay, battle, and Tokkun tables.
- `Application/Abstractions/ITaikoDbContext.Yellow.cs`: Yellow-owned gameplay and Tokkun tables.
- `Infrastructure/Persistence/TaikoDbContext.Green.cs`: Green EF table mappings.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs`: Blue EF table mappings.
- `Infrastructure/Persistence/TaikoDbContext.Yellow.cs`: Yellow EF table mappings.
- `Infrastructure/Persistence/Migrations/`: EF Core migration history.

**Protocol Adapters:**
- `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`: shared `/v01r00/chassis/startupauth.php` route.
- `Adapters.GameProtocol.Green/Controllers/`: Green `/v11r01/chassis/*` routes.
- `Adapters.GameProtocol.Blue/Controllers/`: Blue `/v10r03/chassis/*` routes.
- `Adapters.GameProtocol.Yellow/Controllers/`: Yellow `/v09r02/chassis/*` routes.
- `Adapters.GameProtocol.Red/Wire/Game.cs`, `Adapters.GameProtocol.Red/Wire/VsInterface.cs`: generated Red protocol models; Red route controllers are not added until Host binding/probe plans.
- `Adapters.GameProtocol.Green/Mappers/`, `Adapters.GameProtocol.Blue/Mappers/`, `Adapters.GameProtocol.Yellow/Mappers/`: adapter-local wire/Common DTO mapping.
- `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`: generated protocol models.

**Catalogs and Data:**
- `Application/Abstractions/IGreenCatalog.cs`, `Application/Abstractions/IBlueCatalog.cs`, `Application/Abstractions/IYellowCatalog.cs`: typed era catalog contracts.
- `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`: enabled-era catalog multiplexer.
- `Infrastructure/GameDataCatalog/Ac15/`: shared AC15 parsers and loaders.
- `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`: Green catalog orchestration.
- `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs`: Blue catalog orchestration.
- `Infrastructure/GameDataCatalog/Yellow/YellowEraGameDataCatalog.cs`: Yellow catalog orchestration.
- `Host/wwwroot/data/green/`, `Host/wwwroot/data/blue/`, `Host/wwwroot/data/yellow/`: era runtime data roots.

**Admin and WebUI:**
- `Adapters.AdminApi/Controllers/EraRoute.cs`: `/api/{era}/...` route validation.
- `Adapters.AdminApi/Controllers/UserSettingsController.cs`: user settings legacy and era route dispatcher.
- `Adapters.AdminApi/Controllers/GameDataController.cs`: era catalog readback for WebUI.
- `TaikoWebUI/Utilities/WebUiEra.cs`: WebUI era normalization and URL construction.
- `Contracts.AdminApi/ViewModels/`, `Contracts.AdminApi/Responses/`, `Contracts.AdminApi/Requests/`: shared AdminApi/WebUI DTOs.

**Testing:**
- `Tests/Ac15/`: shared AC15 behavior tests.
- `Tests/Green/`: Green adapter, handler, catalog, ghost, and protocol tests.
- `Tests/Blue/`: Blue adapter, handler, battle, Tokkun, catalog, and protocol tests.
- `Tests/Yellow/`: Yellow adapter, handler, Tokkun, catalog, AdminApi, and protocol tests.
- `Tests/WebUi/`: WebUI service/page behavior tests.

## Naming Conventions

**Files:**
- Use one public type per `.cs` file where practical: `Application/Ac15/Ac15DaniWriter.cs`, `Domain/Entities/UserSaveDataYellow.cs`, `Adapters.GameProtocol.Yellow/Controllers/UserDataController.cs`.
- Use unsuffixed dispatcher files plus era partials: `Application/Handlers/UserDataQuery.cs`, `Application/Handlers/UserDataQuery.Green.cs`, `Application/Handlers/UserDataQuery.Blue.cs`, `Application/Handlers/UserDataQuery.Yellow.cs`.
- Use `Ac15*` for shared AC15 services, records, helpers, profiles, and projections: `Application/Ac15/Ac15NormalPlayWriter.cs`.
- Use `IAc15*` for narrow Domain row-shape interfaces: `Domain/Entities/IAc15SongBestDatum.cs`.
- Use `Common*` for application DTOs crossing adapter/handler boundaries: `Application/Dtos/CommonPlayResultData.cs`.
- Use `*Mappers.cs` or `*Mapper.cs` for Mapperly/manual projections: `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs`, `Application/Ac15/Ac15DaniMapper.cs`.
- Use `*Loader.cs` under `Infrastructure/GameDataCatalog/` for filesystem catalog loaders.
- Use timestamped EF migration names under `Infrastructure/Persistence/Migrations/`.

**Directories:**
- Use project-per-layer directories at repo root: `Domain/`, `Application/`, `Infrastructure/`, `Host/`, `TaikoWebUI/`, `Adapters.*`.
- Use adapter directory names of the form `Adapters.GameProtocol.<EraOrVersion>/`.
- Use `Controllers/`, `Mappers/`, and `Wire/` inside protocol adapters.
- Use era subdirectories for catalog orchestration: `Infrastructure/GameDataCatalog/Green/`, `Infrastructure/GameDataCatalog/Blue/`, `Infrastructure/GameDataCatalog/Yellow/`.
- Use `Infrastructure/GameDataCatalog/Ac15/` for shared AC15 loader code, not for era orchestration.
- Use test subdirectories by behavior or era: `Tests/Ac15/`, `Tests/Green/`, `Tests/Blue/`, `Tests/Yellow/`.

## Where to Add New Code

**New AC15 Shared Behavior:**
- Primary code: `Application/Ac15/`
- Inputs/contracts: `Application/Dtos/Common*.cs`, `Application/Catalog/Ac15/`, `Domain/Entities/IAc15*.cs`
- Handler integration: `Application/Handlers/<UseCase>.Green.cs`, `Application/Handlers/<UseCase>.Blue.cs`, `Application/Handlers/<UseCase>.Yellow.cs`
- Tests: `Tests/Ac15/`

**New Era-Specific AC15 Behavior:**
- Dispatch declaration: `Application/Handlers/<UseCase>.cs`
- Era implementation: `Application/Handlers/<UseCase>.Green.cs`, `Application/Handlers/<UseCase>.Blue.cs`, or `Application/Handlers/<UseCase>.Yellow.cs`
- Era-only DTO fields: `Application/Dtos/<CommonDto>.<Era>.cs` when the application must preserve era-only facts.
- Tests: `Tests/Green/`, `Tests/Blue/`, or `Tests/Yellow/`

**New AC15 Protocol Endpoint:**
- Controller: `Adapters.GameProtocol.Green/Controllers/`, `Adapters.GameProtocol.Blue/Controllers/`, `Adapters.GameProtocol.Yellow/Controllers/`, or `Adapters.GameProtocol.Red/Controllers/` once Red route probes/runtime endpoints are in scope.
- Wire request/response: generated `Adapters.GameProtocol.<Era>/Wire/Game.cs` from `proto/`
- Mapper: `Adapters.GameProtocol.<Era>/Mappers/`
- Application request/handler: `Application/Handlers/`
- Common DTO: `Application/Dtos/` when shared handler logic needs a transport-neutral shape.

**New AC15 Persistence State:**
- Entity: `Domain/Entities/<Era><State>.cs`
- Row-shape interface: `Domain/Entities/IAc15*.cs` only when a shared algorithm needs the shape.
- DbContext port: `Application/Abstractions/ITaikoDbContext.<Era>.cs`
- EF mapping: `Infrastructure/Persistence/TaikoDbContext.<Era>.cs`
- Migration: `Infrastructure/Persistence/Migrations/`
- Tests: era folder plus `Tests/Ac15/` only for shared behavior.

**New AC15 Mapperly Projection:**
- Shared canonical-to-entity projection: `Application/Ac15/*Mapper.cs`
- Adapter wire-to-common projection: `Adapters.GameProtocol.<Era>/Mappers/*Mappers.cs`
- Mapper defaults: `Adapters.GameProtocol.<Era>/MapperlyDefaults.cs` for adapter-wide Mapperly policy.
- Do not put wire DTO persistence mapping into `Infrastructure/Persistence/`.

**New AC15 Catalog Data or Parser:**
- Shared parser/data shape: `Infrastructure/GameDataCatalog/Ac15/` and `Application/Catalog/Ac15/`
- Era wrapper/contract: `Infrastructure/GameDataCatalog/<Era>/`, `Application/Catalog/<Era>/`, `Application/Abstractions/I<Era>Catalog.cs`
- Runtime data files: `Host/wwwroot/data/<era>/`
- Required-file checks: `Infrastructure/GameDataCatalog/<Era>/<Era>RequiredDataFiles.cs`

**New Admin API Surface:**
- Controller: `Adapters.AdminApi/Controllers/`
- Era route parsing: `Adapters.AdminApi/Controllers/EraRoute.cs`
- DTOs: `Contracts.AdminApi/Requests/`, `Contracts.AdminApi/Responses/`, `Contracts.AdminApi/ViewModels/`
- WebUI caller: `TaikoWebUI/Services/` or page code-behind under `TaikoWebUI/Pages/`
- Tests: `Tests/WebUi/` for client behavior and era folders for server data behavior.

**New WebUI Era-Aware Page or Component:**
- Page: `TaikoWebUI/Pages/`
- Reusable component: `TaikoWebUI/Components/` or `TaikoWebUI/Shared/`
- URL helper usage: `TaikoWebUI/Utilities/WebUiEra.cs`
- API service: `TaikoWebUI/Services/`
- Shared DTOs: `Contracts.AdminApi/`

**New Shared Protocol Helper:**
- Cross-adapter HTTP/protobuf helper: `Adapters.GameProtocol.Shared/`
- Cross-handler/application helper: `Application/Common/` or `Application/Ac15/`
- Era-specific protocol byte helper: keep in era-aware code such as `Application/Common/GreenProtocolBytes.cs`, `Application/Common/BlueProtocolBytes.cs`, or an explicit AC15 helper when values match.

**New CLI Utility:**
- Green catalog extraction additions: `GreenCatalogExtractor/`
- Score import additions: `LocalSaveModScoreMigrator/`
- General checked-in scripts: `tools/`
- Local one-off reverse-engineering artifacts: `.tools/`

## Special Directories

**`Host/Configurations/`:**
- Purpose: Runtime JSON configuration loaded explicitly by `Host/Program.cs`.
- Generated: No.
- Committed: Yes. Treat `Host/Configurations/AuthSettings.json` as secret-bearing and do not quote contents.

**`Host/wwwroot/data/`:**
- Purpose: Runtime game data, committed sidecars, shared data, and SQLite database location.
- Generated: Mixed.
- Committed: Mixed. Era folders include `Host/wwwroot/data/green/`, `Host/wwwroot/data/blue/`, `Host/wwwroot/data/yellow/`, `Host/wwwroot/data/nijiiro/`, and `Host/wwwroot/data/shared/`.

**`Host/Certificates/`:**
- Purpose: Local certificate files used by configured Kestrel endpoints.
- Generated: No.
- Committed: Present in tree. Treat certificate/private-key material as secret-bearing.

**`Infrastructure/Persistence/Migrations/`:**
- Purpose: EF Core migration history for `TaikoDbContext`.
- Generated: Yes by EF tooling.
- Committed: Yes.

**`Adapters.GameProtocol.*/Wire/`:**
- Purpose: Generated protocol DTO models for each game adapter.
- Generated: Yes from `proto/`.
- Committed: Yes.

**`proto/`:**
- Purpose: Protocol schema inputs and local wire-generation source material.
- Generated: Mixed; schema inputs are source material, generated C# belongs in adapter `Wire/` folders.
- Committed: Yes when schema evidence is part of the repo.

**`TaikoWebUI/wwwroot/`:**
- Purpose: Blazor client static assets, appsettings, CSS, images, JavaScript, and client data assets.
- Generated: Mixed; build output is not committed, source static assets are committed.
- Committed: Yes for source assets.

**`.planning/codebase/`:**
- Purpose: Generated codebase maps consumed by GSD planning/execution commands.
- Generated: Yes by codebase mapping.
- Committed: Yes when map refreshes are part of project history.

**`.tools/`:**
- Purpose: Local reverse-engineering and analysis helper state.
- Generated: Yes/local.
- Committed: No by default. Treat contents as evidence, not runtime code.

**`.worktrees/`:**
- Purpose: Local git worktrees for isolated feature work.
- Generated: Yes/local.
- Committed: No.

**`artifacts/` and `runs/`:**
- Purpose: Local debug, smoke, review, and run artifacts.
- Generated: Yes/local.
- Committed: No by default.

---

*Structure analysis: 2026-06-11*
