# Technology Stack

**Analysis Date:** 2026-06-11

## Languages

**Primary:**
- C# 13 - Main application language for the ASP.NET Core host, application/domain layers, protocol adapters, infrastructure, Blazor WebAssembly UI, tests, and console tools. The language version is set in `Directory.Build.props`; projects include `Host/Host.csproj`, `Application/Application.csproj`, `Infrastructure/Infrastructure.csproj`, `TaikoWebUI/TaikoWebUI.csproj`, `Tests/Tests.csproj`, `GreenCatalogExtractor/GreenCatalogExtractor.csproj`, and `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`.

**Secondary:**
- Razor - Blazor WebAssembly pages and components live under `TaikoWebUI/Pages`, `TaikoWebUI/Components`, and files such as `TaikoWebUI/App.razor`.
- Protocol Buffers schema files - Wire-contract inputs live under `proto/green/green.proto`, `proto/green/vsinterface.proto`, `proto/blue/taiko.proto`, `proto/blue/vsinterface.proto`, `proto/yellow/yellow-final.proto`, `proto/yellow/yellow-00.proto`, `proto/yellow/vsinterface.proto`, `proto/red/taiko.proto`, `proto/red/vsinterface.proto`, and `proto/3906/Source/App/NetWork/**`; generated or committed C# wire models live in `Adapters.GameProtocol.Green/Wire/**`, `Adapters.GameProtocol.Blue/Wire/**`, `Adapters.GameProtocol.Yellow/Wire/**`, `Adapters.GameProtocol.Red/Wire/**`, and `Adapters.AllnetMucha/Wire/**`.
- JSON - Host configuration and server-authored catalog data live in `Host/Configurations/*.json`, `TaikoWebUI/wwwroot/appsettings.json`, and `Host/wwwroot/data/**`.
- XML/BIN game data - AC15 operator-supplied data is loaded from era data roots such as `Host/wwwroot/data/green/data`, `Host/wwwroot/data/blue/data`, and `Host/wwwroot/data/yellow/data`.
- YAML - GitHub Actions workflow configuration lives in `.github/workflows/publishTLS.yml`.
- Markdown - Developer, evidence, and planning documentation lives in `README.md`, `Host/README.md`, `docs/**`, and `.planning/**`.

## Runtime

**Environment:**
- .NET SDK 10.0.100 - Pinned by `global.json` with `rollForward` set to `latestFeature`.
- .NET target framework `net10.0` - Set centrally in `Directory.Build.props`.
- ASP.NET Core/Kestrel - The executable host is `Host/Program.cs` and `Host/Host.csproj`.
- Blazor WebAssembly - The admin UI is `TaikoWebUI/TaikoWebUI.csproj`, hosted by `Host/Program.cs`.
- Console tools - `GreenCatalogExtractor/Program.cs` and `LocalSaveModScoreMigrator/Program.cs` are command-line utilities.

**Package Manager:**
- NuGet - Package versions are centrally managed in `Directory.Packages.props`.
- Lockfile: missing. No `packages.lock.json` was detected.
- Node, Python, Rust, and Go package manifests were not detected at the repository root.

## Frameworks

**Core:**
- ASP.NET Core 10.0.7 - MVC controllers, middleware, static file hosting, CORS, response compression, authorization, protobuf formatters, and Blazor hosting are configured in `Host/Program.cs` and `Host/Host.csproj`.
- Blazor WebAssembly 10.0.7 - Browser UI in `TaikoWebUI/Program.cs` and `TaikoWebUI/TaikoWebUI.csproj`.
- Entity Framework Core 10.0.7 - SQLite persistence and migrations in `Infrastructure/Persistence/TaikoDbContext.cs`, `Infrastructure/Persistence/TaikoDbContext.*.cs`, and `Infrastructure/Persistence/Migrations/**`.
- SQLite - EF Core provider configured in `Infrastructure/DependencyInjection.cs`; default database file name is defined in `Infrastructure/Persistence/PersistenceConstants.cs`.
- protobuf-net 3.2.x - Protobuf serialization for game, GARM, and shared protocol endpoints in `Host/Program.cs`, `Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj`, and adapter `Wire/` files.
- Mediator.SourceGenerator 3.0.2 - Request/command handler dispatch in `Application/Application.csproj`, `Application/DependencyInjection.cs`, and `Application/Handlers/**`.
- Riok.Mapperly 4.3.1 - Source-generated mapping in `Application/Ac15/*Mapper.cs`, `Adapters.GameProtocol.Blue/Mappers/**`, `Adapters.GameProtocol.Green/Mappers/**`, `Adapters.GameProtocol.Yellow/Mappers/**`, `Adapters.GameProtocol.WwR08/Mappers/**`, `Adapters.GameProtocol.CnR00/Mappers/**`, and `Adapters.AdminApi/Mapping/**`.
- MudBlazor 9.4.0 - Admin UI component framework in `TaikoWebUI/TaikoWebUI.csproj`.

**Testing:**
- xUnit 2.9.3 - Test framework in `Tests/Tests.csproj`.
- Microsoft.NET.Test.Sdk 17.14.1 - .NET test runner integration in `Tests/Tests.csproj`.

**Build/Dev:**
- MSBuild/.NET CLI - Solution file is `TaikoLocalServer.slnx`; shared build defaults are in `Directory.Build.props`.
- Central Package Management - Enabled by `Directory.Packages.props`; individual `.csproj` files use versionless `PackageReference` entries.
- GitHub Actions - Windows publish workflow in `.github/workflows/publishTLS.yml`.
- Self-contained single-file publishing - Non-Debug Host publishes are configured in `Host/Host.csproj`.
- Debug AC15 data junctions - Debug builds create output junctions for `Host/wwwroot/data/green/data`, `Host/wwwroot/data/blue/data`, and `Host/wwwroot/data/yellow/data` when those source paths exist; the MSBuild targets live in `Host/Host.csproj`.
- Repo-local GSD workflow skills - `.codex/skills/**/SKILL.md` contains planning/execution workflows used by project agents; these are developer workflow assets, not application runtime dependencies.

## Key Dependencies

**Critical:**
- `Microsoft.EntityFrameworkCore.Sqlite` 10.0.7 - SQLite persistence for shared identity, Nijiiro rows, and era-owned AC15 save/score/shop/Tokkun/battle rows in `Infrastructure/Infrastructure.csproj`.
- `protobuf-net` 3.2.56 and `protobuf-net.AspNetCore` 3.2.52 - Direct protobuf request/response handling for cabinet and GARM endpoints in `Host/Program.cs`, `Adapters.GameProtocol.Shared/**`, and adapter `Wire/` folders.
- `Mediator.Abstractions` / `Mediator.SourceGenerator` 3.0.2 - Application command/query dispatching through `Application/DependencyInjection.cs` and `Application/Handlers/**`.
- `Riok.Mapperly` 4.3.1 - Mechanical projections between wire DTOs, Admin API DTOs, and AC15 canonical records in `Application/Ac15/**` and adapter `Mappers/**`.
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.7 and `System.IdentityModel.Tokens.Jwt` 8.3.0 - Admin API and WebUI bearer-token authentication in `Infrastructure/DependencyInjection.cs`, `Infrastructure/Identity/JwtTokenService.cs`, and `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
- `BCrypt.Net-Next` 4.1.0 - Password hashing in `Adapters.AdminApi/Controllers/AuthController.cs`.
- `Otp.NET` 1.4.1 - Invite/admin OTP generation and verification in `Adapters.AdminApi/Controllers/AuthController.cs` and UI OTP flows in `TaikoWebUI/**`.
- `Serilog.AspNetCore` 10.0.0, `Serilog.Expressions` 5.0.0, and `Serilog.Sinks.File.Header` 1.0.2 - Structured request logging and CSV head-clerk log output in `Host/Program.cs` and `Host/Logging/CsvFormatter.cs`.

**Infrastructure:**
- `EntityFrameworkCore.Exceptions.Sqlite` 10.0.0 - SQLite exception processing in `Infrastructure/Persistence/TaikoDbContext.cs`.
- `SharpZipLib` 1.4.2 - GZip/deflate handling for protocol payloads and migration tooling in `Adapters.GameProtocol.Shared/Compression/GZipBytesUtil.cs`, `Adapters.AllnetMucha/Middleware/AllNetRequestMiddleware.cs`, and `LocalSaveModScoreMigrator/Program.cs`.
- `BouncyCastle.Cryptography` 2.6.2 - Mucha crypto helpers in `Adapters.AllnetMucha/Common/MuchaCrypto.cs`.
- `Blazored.LocalStorage` 4.5.0 - Browser local storage for JWT token state and UI preferences in `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`, `TaikoWebUI/Components/MainLayout.razor`, and related UI files.
- `MudBlazor` 9.4.0 and `CodeBeam.MudBlazor.Extensions` 9.0.5 - WebUI components in `TaikoWebUI/TaikoWebUI.csproj`.
- `Markdig` 0.38.0 - Markdown rendering support referenced by `TaikoWebUI/TaikoWebUI.csproj`.
- `QRCoder` 1.6.0 - QR code functionality referenced by `TaikoWebUI/TaikoWebUI.csproj`.
- `System.CommandLine` 2.0.0-beta4.22272.1 - CLI argument handling in `GreenCatalogExtractor/GreenCatalogExtractor.csproj` and `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`.
- `Yoh.Text.Json.NamingPolicies` 1.1.3 and `JorgeSerrano.Json.JsonSnakeCaseNamingPolicy` 0.9.0 - JSON naming support in `Infrastructure/Infrastructure.csproj` and `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`.
- `Throw` 1.4.0 - Guard clauses in `Host/Host.csproj`, `Application/Application.csproj`, `Contracts.AdminApi/Contracts.AdminApi.csproj`, `Infrastructure/Infrastructure.csproj`, and `Adapters.AdminApi/Adapters.AdminApi.csproj`.
- `Swashbuckle.AspNetCore` 10.1.7 - Referenced by `Host/Host.csproj`; no Swagger endpoint registration was detected in `Host/Program.cs`.

## Configuration

**Environment:**
- Host startup reads JSON configuration from `Host/Configurations/Kestrel.json`, `Host/Configurations/Logging.json`, `Host/Configurations/Database.json`, `Host/Configurations/ServerSettings.json`, `Host/Configurations/DataSettings.json`, and `Host/Configurations/AuthSettings.json` in `Host/Program.cs`.
- Required host JSON files at startup are `Host/Configurations/Logging.json`, `Host/Configurations/Database.json`, and `Host/Configurations/ServerSettings.json`.
- Optional host JSON files are `Host/Configurations/Kestrel.json`, `Host/Configurations/DataSettings.json`, and `Host/Configurations/AuthSettings.json`.
- `Host/Configurations/AuthSettings.json` is copied to output by `Host/Host.csproj` and contains local JWT/auth settings; do not quote its values in documentation or logs.
- Strongly typed settings live in `Infrastructure/Identity/Settings/AuthSettings.cs`, `Infrastructure/GameDataCatalog/Settings/DataSettings.cs`, `Application/Settings/ServerSettings.cs`, `Infrastructure/Settings/AllnetSettings.cs`, and `TaikoWebUI/Settings/WebUiSettings.cs`.
- Era enablement is controlled by `ServerSettings:Eras` in `Host/Configurations/ServerSettings.json`; `Host/Program.cs` registers only enabled adapters and removes disabled adapter assemblies from MVC routing.
- The current configuration includes `Green`, `Blue`, and `Yellow` AC15 era settings with `GameDataPath`, `AutoExtractCatalog`, `EnableShop`, and `ActiveShopSeasonId` entries in `Host/Configurations/ServerSettings.json`.
- The WebUI fetches server auth/era policy from `GET api/Auth/Config` in `TaikoWebUI/Program.cs` and loads UI-only settings from `TaikoWebUI/wwwroot/appsettings.json`.
- No `.env`, `.env.*`, or `*.env` files were detected by filename scan.

**Build:**
- SDK pin: `global.json`.
- Shared MSBuild defaults: `Directory.Build.props`.
- Central package versions: `Directory.Packages.props`.
- Solution: `TaikoLocalServer.slnx`.
- Project list includes era adapters `Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj`, `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj`, `Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj`, and `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj`.
- Host publish settings: `Host/Host.csproj` publishes non-Debug builds as self-contained single-file output and excludes operator AC15 `wwwroot/data/<era>/data/**` trees from publish.
- CI publish workflow: `.github/workflows/publishTLS.yml`.
- Launch profiles: `Host/Properties/launchSettings.json` and `TaikoWebUI/Properties/launchSettings.json`.

## Platform Requirements

**Development:**
- Install .NET 10 SDK matching `global.json`.
- Use NuGet restore through `dotnet restore`, `dotnet build`, or `dotnet test`.
- Provide operator/game data under `Host/wwwroot/data/nijiiro/datatable`, `Host/wwwroot/data/green/data`, `Host/wwwroot/data/blue/data`, and/or `Host/wwwroot/data/yellow/data` when the corresponding era is enabled.
- AC15 catalog loaders require era-specific `config/<version>/musicinfo.xml`, `config/<version>/musicmedleyinfo.xml`, and `fumen/tuning.bin` paths defined by `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`, `Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs`, and `Infrastructure/GameDataCatalog/Yellow/YellowRequiredDataFiles.cs`.
- Blue battle availability requires parsed XML files under `Host/wwwroot/data/blue/data/config/S10100-1/battle`, loaded through `Infrastructure/GameDataCatalog/Blue/BlueBattleDataLoader.cs`.

**Production:**
- Deployment target is a local ASP.NET Core executable that hosts the game APIs and Blazor WebAssembly UI.
- Release publishing targets `win-x64` self-contained output in `.github/workflows/publishTLS.yml`.
- Runtime storage is local filesystem plus SQLite under the hosted root path; the default database name is `taiko.db3` from `Infrastructure/Persistence/PersistenceConstants.cs`.
- Docker, IIS deployment, and cloud hosting configuration were not detected.

---

*Stack analysis: 2026-06-11*
