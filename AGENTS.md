<!-- GSD:project-start source:PROJECT.md -->
## Project

**TaikoLocalServer Blue Support**

TaikoLocalServer is a local ASP.NET Core server for Taiko no Tatsujin cabinet protocols, local SQLite persistence, era-specific game data catalogs, and a Blazor WebAssembly admin UI. This project continues the existing Blue-era support effort from the Superpowers roadmap in `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`, starting after completed stages A0-A5 and carrying the work through full Blue support.

Full Blue support means normal Blue cabinet flows are completed and hardened first, then Blue battle mode is specified and implemented from concrete evidence. Blue must remain a first-class era with Blue-owned persistence, handlers, mappers, catalogs, tests, and AdminApi/WebUI routing rather than being treated as a Green flag.

**Core Value:** A Blue cabinet can use TaikoLocalServer for normal and battle play with repeatable cabinet/RPCS3 evidence, without corrupting or conflating Green, Nijiiro, or shared identity state.

### Constraints

- **Evidence**: Battle mode must be specified from proto, logs, IDA/client evidence, or cabinet/RPCS3 traces before runtime implementation.
- **Architecture**: Treat Blue as its own era with Blue-owned partial handlers, DTO fields, mappers, persistence, catalog data, tests, and routes.
- **State separation**: Keep Blue, Green, and Nijiiro persistent state separate unless the data is truly shared identity state.
- **Transport safety**: Preserve known Blue direct-protobuf and startup/verup assumptions unless newer client evidence contradicts them.
- **Scope order**: Finish normal Track A A6-A8 before battle implementation; battle evidence/spec work may prepare Track B, but runtime battle behavior should not jump ahead of normal hardening.
- **Verification**: Done requires repeatable cabinet/RPCS3 smoke evidence for normal and battle flows, not only passing server tests.
- **Local data**: Blue runtime data under `Host/wwwroot/data/blue/data` is local/operator-supplied and may be gitignored.
- **Build environment**: If `Host/bin/Debug/net10.0` is locked by a running server, verify Host builds with a temp output path.
<!-- GSD:project-end -->

<!-- GSD:stack-start source:codebase/STACK.md -->
## Technology Stack

## Languages
- C# 13 - Main application language for the ASP.NET Core host, application/domain layers, protocol adapters, infrastructure, Blazor WebAssembly UI, tests, and console tools. The language version is set in `Directory.Build.props`; project files include `Host/Host.csproj`, `Application/Application.csproj`, `Infrastructure/Infrastructure.csproj`, `TaikoWebUI/TaikoWebUI.csproj`, `Tests/Tests.csproj`, `GreenCatalogExtractor/GreenCatalogExtractor.csproj`, and `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`.
- Razor - Blazor WebAssembly pages and components live in `TaikoWebUI/Pages`, `TaikoWebUI/Components`, and related `.razor` files such as `TaikoWebUI/Pages/Profile.razor` and `TaikoWebUI/Components/NavMenu.razor`.
- Protocol Buffers schema files - Game and ALL.Net/Mucha wire contracts are represented by `.proto` inputs under `proto/green/green.proto`, `proto/green/vsinterface.proto`, `proto/blue/taiko.proto`, and `proto/blue/vsinterface.proto`; generated or committed wire models live in paths such as `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, and `Adapters.AllnetMucha/Wire/types.cs`.
- JSON - Runtime configuration and operator/catalog data are stored in `Host/Configurations/*.json`, `TaikoWebUI/wwwroot/appsettings.json`, and `Host/wwwroot/data/**`.
- YAML - GitHub Actions workflow configuration lives in `.github/workflows/publishTLS.yml`.
- Markdown - Developer and planning documentation lives in `README.md`, `Host/README.md`, `TaikoWebUI/README.md`, and `docs/**`.
## Runtime
- .NET SDK 10.0.100 - Pinned by `global.json` with `rollForward` set to `latestFeature`.
- .NET target framework `net10.0` - Set centrally in `Directory.Build.props`.
- ASP.NET Core/Kestrel - The executable host is `Host/Program.cs` and `Host/Host.csproj`.
- Blazor WebAssembly - The admin UI is `TaikoWebUI/TaikoWebUI.csproj`, hosted by the ASP.NET Core host through `Host/Program.cs`.
- Console tools - `GreenCatalogExtractor/Program.cs` and `LocalSaveModScoreMigrator/Program.cs` are command-line utilities.
- NuGet - Package versions are centrally managed in `Directory.Packages.props`.
- Lockfile: missing. No `packages.lock.json` was detected outside build output.
## Frameworks
- ASP.NET Core 10.0.7 - MVC controllers, middleware, static file hosting, CORS, response compression, authorization, and Blazor hosting in `Host/Program.cs` and `Host/Host.csproj`.
- Blazor WebAssembly 10.0.7 - Browser UI in `TaikoWebUI/Program.cs` and `TaikoWebUI/TaikoWebUI.csproj`.
- Entity Framework Core 10.0.7 - Persistence and migrations in `Infrastructure/Persistence/TaikoDbContext.cs` and `Infrastructure/Persistence/Migrations`.
- SQLite - EF Core provider configured in `Infrastructure/DependencyInjection.cs` and `Infrastructure/Persistence/TaikoDbContext.cs`.
- protobuf-net 3.2.x - Protobuf serialization for game, GARM, and shared protocol endpoints in `Host/Program.cs`, `Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj`, and adapter wire files.
- Mediator.SourceGenerator 3.0.2 - Request/command handler pattern in `Application/Application.csproj` and `Application/Handlers/**`.
- MudBlazor 9.4.0 - Admin UI component framework in `TaikoWebUI/TaikoWebUI.csproj`.
- xUnit 2.9.3 - Test framework in `Tests/Tests.csproj`.
- Microsoft.NET.Test.Sdk 17.14.1 - .NET test runner integration in `Tests/Tests.csproj`.
- MSBuild/.NET CLI - Solution file is `TaikoLocalServer.slnx`; shared build defaults are in `Directory.Build.props`.
- GitHub Actions - Windows publish workflow in `.github/workflows/publishTLS.yml`.
- Riok.Mapperly 4.3.1 - Source-generated mapping in adapter projects such as `Adapters.AdminApi/Adapters.AdminApi.csproj`.
- System.CommandLine 2.0.0-beta4.22272.1 - CLI argument handling in `GreenCatalogExtractor/Program.cs` and `LocalSaveModScoreMigrator/Program.cs`.
- Swashbuckle.AspNetCore 10.1.7 - Referenced by `Host/Host.csproj`; no Swagger middleware or endpoint registration was detected in `Host/Program.cs`.
## Key Dependencies
- `Microsoft.EntityFrameworkCore.Sqlite` 10.0.7 - SQLite persistence for user data, scores, credentials, and era-specific save state in `Infrastructure/Infrastructure.csproj`.
- `protobuf-net` 3.2.56 and `protobuf-net.AspNetCore` 3.2.52 - Protobuf request/response handling for cabinet and GARM endpoints in `Host/Program.cs` and adapter projects.
- `Mediator.Abstractions` / `Mediator.SourceGenerator` 3.0.2 - Application command/query dispatching in `Application/Handlers/**`.
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.7 and `System.IdentityModel.Tokens.Jwt` 8.3.0 - Admin API and WebUI bearer-token authentication in `Infrastructure/DependencyInjection.cs`, `Infrastructure/Identity/JwtTokenService.cs`, and `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
- `BCrypt.Net-Next` 4.1.0 - Password hashing in `Adapters.AdminApi/Controllers/AuthController.cs`.
- `Otp.NET` 1.4.1 - Invite/admin OTP generation in `Adapters.AdminApi/Controllers/AuthController.cs`.
- `Serilog.AspNetCore` 10.0.0, `Serilog.Expressions` 5.0.0, and `Serilog.Sinks.File.Header` 1.0.2 - Structured request logging and CSV head-clerk log output in `Host/Program.cs` and `Host/Logging/CsvFormatter.cs`.
- `MudBlazor` 9.4.0 and `CodeBeam.MudBlazor.Extensions` 9.0.5 - UI components in `TaikoWebUI/TaikoWebUI.csproj`.
- `EntityFrameworkCore.Exceptions.Sqlite` 10.0.0 - SQLite exception processing in `Infrastructure/Persistence/TaikoDbContext.cs`.
- `SharpZipLib` 1.4.2 - Compression/decompression for protocol payloads and migration tooling in `Adapters.GameProtocol.Shared/Compression` and `LocalSaveModScoreMigrator/Program.cs`.
- `Blazored.LocalStorage` 4.5.0 - Browser local storage for UI preferences and JWT token state in `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs` and `TaikoWebUI/Components/MainLayout.razor`.
- `QRCoder` 1.6.0 - QR code functionality referenced by `TaikoWebUI/TaikoWebUI.csproj`.
- `Markdig` 0.38.0 - Markdown rendering support referenced by `TaikoWebUI/TaikoWebUI.csproj`.
- `Throw` 1.4.0 - Guard clauses used across host/application/contract projects.
- `Swan.Core` 7.0.0-beta.2 - Utility/mapping support in adapter and UI projects.
- `Yoh.Text.Json.NamingPolicies` 1.1.3 - JSON naming policies in `Infrastructure/Infrastructure.csproj`.
- `JorgeSerrano.Json.JsonSnakeCaseNamingPolicy` 0.9.0 - Local save import JSON parsing in `LocalSaveModScoreMigrator/Program.cs`.
## Configuration
- Host startup reads JSON configuration from `Host/Configurations/Kestrel.json`, `Host/Configurations/Logging.json`, `Host/Configurations/Database.json`, `Host/Configurations/ServerSettings.json`, `Host/Configurations/DataSettings.json`, and `Host/Configurations/AuthSettings.json` in `Host/Program.cs`.
- Required host JSON files at startup are `Host/Configurations/Logging.json`, `Host/Configurations/Database.json`, and `Host/Configurations/ServerSettings.json`.
- Optional host JSON files are `Host/Configurations/Kestrel.json`, `Host/Configurations/DataSettings.json`, and `Host/Configurations/AuthSettings.json`.
- Strongly typed settings are defined in `Infrastructure/Identity/Settings/AuthSettings.cs`, `Infrastructure/GameDataCatalog/Settings/DataSettings.cs`, `Application/Settings/ServerSettings.cs`, `Infrastructure/Settings/AllnetSettings.cs`, and `TaikoWebUI/Settings/WebUiSettings.cs`.
- The WebUI fetches server auth policy from `GET api/Auth/Config` and loads UI-only settings from `TaikoWebUI/wwwroot/appsettings.json` in `TaikoWebUI/Program.cs`.
- No `.env`, `.env.*`, or `*.env` files were detected by filename scan.
- SDK pin: `global.json`.
- Shared MSBuild defaults: `Directory.Build.props`.
- Central package versions: `Directory.Packages.props`.
- Solution: `TaikoLocalServer.slnx`.
- Host publish settings: `Host/Host.csproj` publishes non-Debug builds as self-contained single-file output.
- CI publish workflow: `.github/workflows/publishTLS.yml`.
- Launch profiles: `Host/Properties/launchSettings.json` and `TaikoWebUI/Properties/launchSettings.json`.
## Platform Requirements
- Install .NET 10 SDK matching `global.json`.
- Use NuGet restore through `dotnet restore`, `dotnet build`, or `dotnet test`.
- Provide operator/game data under `Host/wwwroot/data/nijiiro/datatable`, `Host/wwwroot/data/green/data`, and/or `Host/wwwroot/data/blue/data` when the corresponding era is enabled; required data layout is documented in `README.md` and `Host/README.md`.
- Green and Blue AC15 game-data directories are expected to be copied or symlinked from local game installs, as documented in `README.md` and `Host/README.md`.
- Deployment target is a local ASP.NET Core executable that hosts the game APIs and Blazor WebAssembly UI.
- Release publishing targets `win-x64` self-contained output in `.github/workflows/publishTLS.yml`.
- Runtime storage is local filesystem plus SQLite under the hosted `wwwroot` root; the default database name is `taiko.db3` from `Infrastructure/Persistence/PersistenceConstants.cs`.
- Docker, IIS deployment, and cloud hosting configuration were not detected.
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

## Naming Patterns
- Use one primary type per `.cs` file, named after the type: `Application/Common/BitsetCodec.cs`, `Adapters.AdminApi/Controllers/EraRoute.cs`, `Domain/Entities/UserSaveDataGreen.cs`.
- Use era suffixes on partial files when behavior branches by game era: `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Dtos/CommonUserDataResponse.Green.cs`.
- Use feature or adapter folders as namespaces: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Tests/Green/GreenPlayResultHandlerTests.cs`.
- Generated protobuf files live under `Adapters.GameProtocol.* /Wire/` and are treated as generated extension points: `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.WwR08/Wire/VsInterface.cs`.
- Razor pages use `.razor` plus optional `.razor.cs` code-behind: `TaikoWebUI/Pages/DaniDojo.razor`, `TaikoWebUI/Pages/DaniDojo.razor.cs`, `TaikoWebUI/Components/Song/SongLeaderboardCard.razor.cs`.
- Use PascalCase for methods, including private helpers: `HandleGreen(...)` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `MapStage(...)` in `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, `IsDate(...)` in `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`.
- Use `Try*` names for non-throwing parse/lookup methods with `out` parameters: `EraRoute.TryParse(...)` in `Adapters.AdminApi/Controllers/EraRoute.cs`, `TryGetSelectedDan(...)` in `TaikoWebUI/Pages/DaniDojo.razor.cs`.
- Use `Map*`, `Build*`, `Apply*`, and `Upsert*` verbs for transformations and persistence steps: `MapGreenClearGrade(...)` in `Adapters.AdminApi/Controllers/DanBestDataController.cs`, `ApplyUnlockBits(...)` and `UpsertBestAsync(...)` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`.
- Handler entry methods implement `Handle(...)` for mediator requests and delegate to era-specific partial handlers: `Application/Handlers/UpdatePlayResultCommand.cs`.
- Use camelCase for locals and parameters: `activeShopSeason`, `shopSeasonState`, `playResultData` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`.
- Use private readonly fields in camelCase without an underscore: `connection` in `Tests/Green/GreenHandlerFixture.cs`, `client` and cache dictionaries in `TaikoWebUI/Services/GameDataService.cs`.
- Some WebUI state fields use leading underscores for backing maps such as `_bestDataMap` in `TaikoWebUI/Pages/DaniDojo.razor.cs`; keep this local to component backing state rather than spreading it to service or handler code.
- Use domain names from the protocol/database model as-is even when terse: `Baid`, `SongNo`, `DanId`, `VerupNo` in `Domain/Entities/UserSaveDataGreen.cs`, `Application/Catalog/Green/GreenItemShopEntry.cs`, and `Adapters.GameProtocol.Green/Wire/Game.cs`.
- Use PascalCase for classes, records, enums, and request/response DTOs: `GreenItemShopCatalog` in `Application/Catalog/Green/GreenItemShopCatalog.cs`, `CommonPlayResultData` in `Application/Dtos/CommonPlayResultData.cs`, `GameEra` in `Domain/Enums/GameEra.cs`.
- Use `I` prefixes for interfaces: `IGameDataCatalog`, `IGreenCatalog`, and `ITaikoDbContext` in `Application/Abstractions/`.
- Use `readonly record struct` for small immutable mediator request values: `UpdatePlayResultCommand` in `Application/Handlers/UpdatePlayResultCommand.cs`.
- Use `sealed` on helper, fixture, and loader classes where inheritance is not intended: `BlueHandlerFixture` in `Tests/Blue/BlueHandlerFixture.cs`, `GreenMusicInfoLoader` in `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs`.
## Code Style
- No `.editorconfig` is detected. Follow the style already enforced by examples in `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Adapters.AdminApi/Controllers/DanBestDataController.cs`, and `Tests/Green/GreenPlayResultHandlerTests.cs`.
- C# defaults are centralized in `Directory.Build.props`: `net10.0`, nullable enabled, implicit usings enabled, `LangVersion` 13, and warnings not treated as errors.
- Package versions are centralized in `Directory.Packages.props`; project files such as `Application/Application.csproj` and `Tests/Tests.csproj` use versionless `<PackageReference />` entries.
- Use file-scoped namespaces: `namespace TaikoLocalServer.Application.Common;` in `Application/Common/BitsetCodec.cs` and `namespace TaikoLocalServer.Tests.Green;` in `Tests/Green/GreenProtocolBytesTests.cs`.
- Prefer target-typed `new`, collection expressions, and null-coalescing defaults where the codebase uses them: `new()` and `[]` in `Application/Catalog/Green/GreenItemShopCatalog.cs`, `Tests/Green/GreenHandlerFixture.cs`, and `Application/Dtos/CommonUserDataResponse.Green.cs`.
- Keep object initializers explicit for DTO and entity projection code: `CommonInitialDataCheckResponse` in `Application/Handlers/GetInitialDataQuery.Green.cs`, `DanBestDataResponse` in `Adapters.AdminApi/Controllers/DanBestDataController.cs`.
- Keep generated wire files out of style cleanup unless regenerating the protocol output: `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`.
- No analyzer config, StyleCop, Roslynator, or `dotnet format` config is detected. `Directory.Build.props` explicitly sets `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.
- Mapperly warnings `RMG020` and `RMG012` are suppressed per adapter project where generated mapper behavior is accepted: `Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj`, `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj`, `Adapters.AdminApi/Adapters.AdminApi.csproj`.
- Use `dotnet build TaikoLocalServer.slnx` as the broad compile quality gate and `dotnet build Host/Host.csproj -o <temp-output>` when avoiding locked `Host/bin` output.
## Import Organization
- No C# path aliasing is configured. Use project references from `.csproj` files and normal namespaces, as in `Application/Application.csproj`, `Host/Host.csproj`, and `Tests/Tests.csproj`.
- For Blazor, use `_Imports.razor` and project `GlobalUsings.cs` instead of repeating common imports in every component: `TaikoWebUI/_Imports.razor`, `TaikoWebUI/GlobalUsings.cs`.
## Error Handling
- Domain/service code throws `InvalidOperationException` for unsupported eras or impossible state: `Application/Handlers/UpdatePlayResultCommand.cs`, `Application/Handlers/GetInitialDataQuery.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`.
- Data loader code throws `InvalidDataException` or `FileNotFoundException` with file paths and row context for malformed required inputs: `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15MusicInfoLoader.cs`, `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`.
- Optional or disabled catalog features return empty/disabled models instead of throwing: `Ac15ItemShopCatalog.Disabled` via `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, missing event folders via `Infrastructure/GameDataCatalog/Ac15/Ac15EventFolderLoader.cs`.
- Protocol controllers catch decode/processing exceptions when the cabinet expects success-shaped protobuf responses and return protocol result `0`: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`.
- Admin API controllers return HTTP results (`BadRequest`, `NotFound`, `Unauthorized`, `Forbid`) for caller-visible validation and authorization failures: `Adapters.AdminApi/Controllers/AuthController.cs`, `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`, `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`.
- Use `Throw` package guards where existing code uses them for compact validation: `Contracts.AdminApi/Converters/PlaySettingConverter.cs`, `TaikoWebUI/Pages/DaniDojo.razor.cs`, `Application/Handlers/PurchaseSongCommand.Nijiiro.cs`.
- Use BCL argument helpers for shared utilities: `ArgumentOutOfRangeException.ThrowIfNegative(...)` in `Application/Common/BitsetCodec.cs`.
## Logging
- Use structured message templates with named properties: `logger.LogWarning("Rejecting invalid Green playresult payload for baid {Baid}", request.Baid)` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`.
- Use controller `Logger` from the base controller for protocol request logging: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`.
- Use boot and request logging through Serilog setup in `Host/Program.cs`, including fatal startup errors, unknown 404s, and unsuccessful non-401 responses.
- Use warning logs for recoverable catalog/runtime fallbacks: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15MovieLoader.cs`.
- Tests use `NullLogger<T>.Instance` for quiet handler tests and local recording loggers only when assertions inspect log events: `Tests/Green/GreenPlayResultHandlerTests.cs`, `Tests/Green/GreenCatalogLoaderTests.cs`.
## Comments
- Comment non-obvious runtime or deployment behavior, not routine assignments: disabled adapter filtering and cache behavior in `Host/Program.cs`, Green game-data copy behavior in `Host/Host.csproj`.
- Keep comments on protocol-specific semantic exceptions where a future maintainer needs the rule: Green Dani normal scoring comment in `Application/Handlers/UpdatePlayResultCommand.Green.cs`.
- Avoid adding comments to straightforward DTO/entity properties such as `Domain/Entities/UserSaveDataGreen.cs` or `Application/Dtos/CommonUserDataResponse.Green.cs`.
- XML documentation is not a dominant pattern. Prefer clear type and method names in `Application/Handlers/`, `Infrastructure/GameDataCatalog/`, and `Adapters.*`.
- Generated wire files may include generator comments; do not extend them manually in `Adapters.GameProtocol.Green/Wire/Game.cs` or `Adapters.GameProtocol.Blue/Wire/Game.cs`.
## Function Design
## Module Design
- Put shared request records and era dispatch in the unsuffixed file: `Application/Handlers/UpdatePlayResultCommand.cs`.
- Put era-specific behavior in `.Nijiiro.cs`, `.Green.cs`, and `.Blue.cs` partial files: `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`.
- Throw on unsupported eras in shared dispatch rather than silently falling through: `Application/Handlers/GetTaikojukuQuery.cs`, `Application/Handlers/GetSelfBestQuery.cs`.
- Use Mapperly `[Mapper]` static partial classes for adapter mappers, but keep custom field semantics explicit in hand-written methods: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, `Adapters.AdminApi/Mapping/AuthConfigMapper.cs`.
- Do not map generated protobuf DTOs directly into persistence entities; map through common DTOs in `Application/Dtos/`, as shown by `CommonPlayResultData` and `PlayResultMappers` in `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`.
- No repository-local `.codex/skills/` or `.agents/skills/` directories are detected, so no additional project-skill conventions apply.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

## System Overview
```text
|                                Host process                                    |
|                         `Host/Program.cs`                                      |
| Admin REST API       | Game protocol adapters | AllNet/Mucha lifecycle        |
| `Adapters.AdminApi`  | `Adapters.GameProtocol.*` | `Adapters.AllnetMucha`     |
|                           Application use cases                                |
|      Mediator handlers, ports, Common* DTOs, settings in `Application/`        |
|                         Domain model and contracts                             |
|         `Domain/` entities/enums/constants; `Contracts.AdminApi/` DTOs         |
|                             Infrastructure                                     |
|       EF Core SQLite, file catalog, JWT, clock in `Infrastructure/`            |
|                        SQLite and filesystem data                              |
|     `Host/wwwroot/taiko.db3`, `Host/wwwroot/data/`, `Host/Configurations/`     |
|                         Blazor WebAssembly admin UI                            |
|       `TaikoWebUI/` is built into and served by `Host/Program.cs`              |
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
- Keep the domain core independent: `Domain/Domain.csproj` has no project references and `Application/Application.csproj` defines ports instead of depending on `Infrastructure/`.
- Compose all runtime dependencies in `Host/Program.cs`; adapters expose `DependencyInjection.cs` extension methods such as `Adapters.GameProtocol.Green/DependencyInjection.cs` and `Adapters.AllnetMucha/DependencyInjection.cs`.
- Use era-aware partial files for shared use cases: the central dispatcher in `Application/Handlers/BaidQuery.cs` switches on `GameEra`, while `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/BaidQuery.Blue.cs`, and `Application/Handlers/BaidQuery.Nijiiro.cs` own era-specific behavior.
- Use adapter-local wire models and mappers: `Adapters.GameProtocol.Green/Wire/` and `Adapters.GameProtocol.Green/Mappers/` should translate to `Application/Dtos/Common*.cs` before crossing into handlers.
- Keep WebUI DTO compatibility through `Contracts.AdminApi/`; `TaikoWebUI/TaikoWebUI.csproj` references `Contracts.AdminApi/Contracts.AdminApi.csproj` only.
## Layers
- Purpose: Own process startup, configuration loading, middleware order, static file hosting, controller registration, migration execution, and era enablement.
- Location: `Host/`
- Contains: `Host/Program.cs`, `Host/Configurations/`, `Host/wwwroot/`, `Host/Logging/CsvFormatter.cs`, `Host/Host.csproj`
- Depends on: `Adapters.*`, `Application`, `Domain`, `Infrastructure`, `TaikoWebUI`
- Used by: Operators and development commands such as `dotnet run --project Host`
- Purpose: Define persistent business entities, enums, and constants.
- Location: `Domain/`
- Contains: `Domain/Entities/`, `Domain/Enums/`, `Domain/DomainConstants.cs`
- Depends on: None at the project-reference level via `Domain/Domain.csproj`
- Used by: `Application/`, `Infrastructure/`, `Contracts.AdminApi/`, adapters, tests, and tools
- Purpose: Define admin API and WebUI DTOs, converters, auth helpers, and shared server-data shapes.
- Location: `Contracts.AdminApi/`
- Contains: `Contracts.AdminApi/Requests/`, `Contracts.AdminApi/Responses/`, `Contracts.AdminApi/ViewModels/`, `Contracts.AdminApi/ServerData/`, `Contracts.AdminApi/Converters/`, `Contracts.AdminApi/Authorization/`
- Depends on: `Domain/Domain.csproj`
- Used by: `Adapters.AdminApi/`, `Application/`, `Infrastructure/`, `TaikoWebUI/`, and `Tests/`
- Purpose: Orchestrate game protocol use cases through Mediator and expose ports for persistence, catalog, auth, and time.
- Location: `Application/`
- Contains: `Application/Handlers/`, `Application/Abstractions/`, `Application/Dtos/`, `Application/Common/`, `Application/Catalog/`, `Application/ServerData/`, `Application/Settings/`
- Depends on: `Domain/Domain.csproj`, `Contracts.AdminApi/Contracts.AdminApi.csproj`, `Mediator.Abstractions`, `Microsoft.EntityFrameworkCore`
- Used by: `Infrastructure/`, `Adapters.*`, `Host/`, tests, and tools
- Purpose: Implement application ports and own all durable I/O.
- Location: `Infrastructure/`
- Contains: `Infrastructure/Persistence/`, `Infrastructure/Persistence/Migrations/`, `Infrastructure/GameDataCatalog/`, `Infrastructure/Identity/`, `Infrastructure/Time/`, `Infrastructure/Settings/`
- Depends on: `Application/`, `Contracts.AdminApi/`, `Domain/`
- Used by: `Host/Program.cs`, `Adapters.AdminApi/`, `Adapters.AllnetMucha/`, `GreenCatalogExtractor/`, `LocalSaveModScoreMigrator/`
- Purpose: Translate HTTP requests from game clients, WebUI, and AllNet/Mucha into application/domain work.
- Location: `Adapters.AdminApi/`, `Adapters.AllnetMucha/`, `Adapters.GameProtocol.Shared/`, `Adapters.GameProtocol.WwR08/`, `Adapters.GameProtocol.CnR00/`, `Adapters.GameProtocol.Green/`, `Adapters.GameProtocol.Blue/`
- Contains: `Controllers/`, `Mappers/`, `Wire/`, adapter `DependencyInjection.cs`, middleware and compression helpers
- Depends on: `Application/`; game adapters also use `Adapters.GameProtocol.Shared/`; admin and AllNet adapters use `Infrastructure/`
- Used by: ASP.NET Core application parts registered from `Host/Program.cs`
- Purpose: Provide the Blazor WASM admin interface hosted by the same ASP.NET Core process.
- Location: `TaikoWebUI/`
- Contains: `TaikoWebUI/Pages/`, `TaikoWebUI/Components/`, `TaikoWebUI/Services/`, `TaikoWebUI/Authorization/`, `TaikoWebUI/Utilities/`, `TaikoWebUI/wwwroot/`
- Depends on: `Contracts.AdminApi/Contracts.AdminApi.csproj`
- Used by: `Host/Host.csproj` as a project reference and `Host/Program.cs` via `UseBlazorFrameworkFiles()`
## Data Flow
### Host Startup Path
### Game Protocol Request Path
### Admin WebUI Request Path
### Catalog Initialization Flow
- EF Core state is scoped per request through `Infrastructure/DependencyInjection.cs:43` and exposed through the `ITaikoDbContext` port at `Infrastructure/DependencyInjection.cs:58`.
- Catalog state is singleton, initialized once at startup, and accessed through `IGameDataCatalog` in `Application/Abstractions/IGameDataCatalog.cs`.
- WebUI client-side cached state lives in singleton services such as `TaikoWebUI/Services/GameDataService.cs`; authentication state lives in `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
- Global logging state is initialized through Serilog in `Host/Program.cs`; CSV side-channel formatting is implemented in `Host/Logging/CsvFormatter.cs`.
## Key Abstractions
- Purpose: Selects era-specific routes, catalogs, handlers, DB sets, and WebUI APIs.
- Examples: `Domain/Enums/GameEra.cs`, `Host/Program.cs`, `Application/Handlers/BaidQuery.cs`, `TaikoWebUI/Utilities/WebUiEra.cs`
- Pattern: Enum-driven dispatch with partial implementation files and application-part gating.
- Purpose: Encapsulates game protocol use cases behind `IRequest<T>` and scoped handlers.
- Examples: `Application/Handlers/BaidQuery.cs`, `Application/Handlers/UpdatePlayResultCommand.cs`, `Application/DependencyInjection.cs`
- Pattern: `readonly record struct` request plus handler; central file dispatches by era and era partials implement behavior.
- Purpose: Provide version-agnostic shapes between protocol adapters and application handlers.
- Examples: `Application/Dtos/CommonBaidResponse.cs`, `Application/Dtos/CommonPlayResultData.cs`, `Application/Dtos/CommonUserDataResponse.cs`
- Pattern: Shared base files plus optional `.Nijiiro.cs`, `.Green.cs`, and `.Blue.cs` partial extensions.
- Purpose: Application-facing persistence port exposing era-partitioned DbSets.
- Examples: `Application/Abstractions/ITaikoDbContext.cs`, `Application/Abstractions/ITaikoDbContext.Green.cs`, `Infrastructure/Persistence/TaikoDbContext.cs`
- Pattern: Partial interface mirrored by partial EF Core implementation files.
- Purpose: Multiplexes immutable era-specific filesystem catalogs.
- Examples: `Application/Abstractions/IGameDataCatalog.cs`, `Application/Abstractions/INijiiroCatalog.cs`, `Application/Abstractions/IGreenCatalog.cs`, `Application/Abstractions/IBlueCatalog.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`
- Pattern: `For(GameEra)` plus typed convenience extensions in `Application/Common/CatalogExtensions.cs`.
- Purpose: Standardize lazy access to `IMediator` and logging for controllers.
- Examples: `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`, `Adapters.AdminApi/BaseAdminController.cs`
- Pattern: Generic base controller with `HttpContext.RequestServices` lookup.
- Purpose: Translate wire/contracts DTOs to application DTOs without reflection-based runtime mapping.
- Examples: `Adapters.GameProtocol.WwR08/Mappers/BaidResponseMapper.cs`, `Adapters.GameProtocol.Green/Mappers/BaidResponseMapper.cs`, `Adapters.AdminApi/Mapping/AuthConfigMapper.cs`
- Pattern: `[Mapper] public static partial class ...` with generated or manually post-processed mapping methods.
## Entry Points
- Location: `Host/Program.cs`
- Triggers: `dotnet run --project Host`, published `TaikoLocalServer.exe`, or host process startup.
- Responsibilities: Configuration, DI, migrations, catalogs, middleware, controllers, Blazor static files, and fallback routing.
- Location: `TaikoWebUI/Program.cs`
- Triggers: Browser loads static files served by `Host/Program.cs`.
- Responsibilities: Fetch WebUI config and auth config, configure MudBlazor/localization/auth, initialize game-data caches, and run routed pages.
- Location: `Adapters.GameProtocol.WwR08/Controllers/`, `Adapters.GameProtocol.CnR00/Controllers/`, `Adapters.GameProtocol.Green/Controllers/`, `Adapters.GameProtocol.Blue/Controllers/`
- Triggers: Game cabinet HTTP POSTs under `/v12r08_ww/chassis`, `/v12r00_cn/chassis`, `/v11r01/chassis`, `/v10r03/chassis`, and shared `/v01r00/chassis`.
- Responsibilities: Deserialize wire payloads, map to common DTOs or commands, send through Mediator, and serialize protobuf responses.
- Location: `Adapters.AdminApi/Controllers/`
- Triggers: `TaikoWebUI/` HTTP calls under `/api/...`.
- Responsibilities: Serve users, credentials, settings, play history, game data, customization catalogs, favorites, auth config, login/register, and era-specific admin data.
- Location: `Adapters.AllnetMucha/Controllers/`, `Adapters.AllnetMucha/Middleware/AllNetRequestMiddleware.cs`
- Triggers: Cabinet lifecycle endpoints under `/sys/servlet/PowerOn`, `/mucha_front/...`, `/mucha_activation/...`, and `/v1/s12-jp-dev/garm...`.
- Responsibilities: Decode AllNet PowerOn form data, answer lifecycle/updater calls, and use configured Mucha/Game URLs.
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
### Adapter-to-Adapter Coupling
### Direct Host Path Construction
### Config in the Wrong Surface
## Error Handling
- Startup refuses invalid enabled-era configuration in `Host/Program.cs` before registering runtime routes.
- Unknown routes and non-401 client failures are logged after request execution in `Host/Program.cs`.
- Disabled catalog access throws `InvalidOperationException` from `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs:19`.
- Admin controllers return `BadRequest`, `NotFound`, `Forbid`, or `NoContent` directly, as shown in `Adapters.AdminApi/Controllers/UsersController.cs`.
- Game protocol handlers throw for unsupported `GameEra` values in dispatcher files such as `Application/Handlers/BaidQuery.cs:18`.
## Cross-Cutting Concerns
<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->
## Project Skills

No project skills found. Add skills to any of: `.claude/skills/`, `.agents/skills/`, `.cursor/skills/`, `.github/skills/`, or `.codex/skills/` with a `SKILL.md` index file.
<!-- GSD:skills-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd-debug` for investigation and bug fixing
- `/gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
