# Technology Stack

**Analysis Date:** 2026-05-28

## Languages

**Primary:**
- C# 13 - Main application language for the ASP.NET Core host, application/domain layers, protocol adapters, infrastructure, Blazor WebAssembly UI, tests, and console tools. The language version is set in `Directory.Build.props`; project files include `Host/Host.csproj`, `Application/Application.csproj`, `Infrastructure/Infrastructure.csproj`, `TaikoWebUI/TaikoWebUI.csproj`, `Tests/Tests.csproj`, `GreenCatalogExtractor/GreenCatalogExtractor.csproj`, and `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`.

**Secondary:**
- Razor - Blazor WebAssembly pages and components live in `TaikoWebUI/Pages`, `TaikoWebUI/Components`, and related `.razor` files such as `TaikoWebUI/Pages/Profile.razor` and `TaikoWebUI/Components/NavMenu.razor`.
- Protocol Buffers schema files - Game and ALL.Net/Mucha wire contracts are represented by `.proto` inputs under `proto/green/green.proto`, `proto/green/vsinterface.proto`, `proto/blue/taiko.proto`, and `proto/blue/vsinterface.proto`; generated or committed wire models live in paths such as `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, and `Adapters.AllnetMucha/Wire/types.cs`.
- JSON - Runtime configuration and operator/catalog data are stored in `Host/Configurations/*.json`, `TaikoWebUI/wwwroot/appsettings.json`, and `Host/wwwroot/data/**`.
- YAML - GitHub Actions workflow configuration lives in `.github/workflows/publishTLS.yml`.
- Markdown - Developer and planning documentation lives in `README.md`, `Host/README.md`, `TaikoWebUI/README.md`, and `docs/**`.

## Runtime

**Environment:**
- .NET SDK 10.0.100 - Pinned by `global.json` with `rollForward` set to `latestFeature`.
- .NET target framework `net10.0` - Set centrally in `Directory.Build.props`.
- ASP.NET Core/Kestrel - The executable host is `Host/Program.cs` and `Host/Host.csproj`.
- Blazor WebAssembly - The admin UI is `TaikoWebUI/TaikoWebUI.csproj`, hosted by the ASP.NET Core host through `Host/Program.cs`.
- Console tools - `GreenCatalogExtractor/Program.cs` and `LocalSaveModScoreMigrator/Program.cs` are command-line utilities.

**Package Manager:**
- NuGet - Package versions are centrally managed in `Directory.Packages.props`.
- Lockfile: missing. No `packages.lock.json` was detected outside build output.

## Frameworks

**Core:**
- ASP.NET Core 10.0.7 - MVC controllers, middleware, static file hosting, CORS, response compression, authorization, and Blazor hosting in `Host/Program.cs` and `Host/Host.csproj`.
- Blazor WebAssembly 10.0.7 - Browser UI in `TaikoWebUI/Program.cs` and `TaikoWebUI/TaikoWebUI.csproj`.
- Entity Framework Core 10.0.7 - Persistence and migrations in `Infrastructure/Persistence/TaikoDbContext.cs` and `Infrastructure/Persistence/Migrations`.
- SQLite - EF Core provider configured in `Infrastructure/DependencyInjection.cs` and `Infrastructure/Persistence/TaikoDbContext.cs`.
- protobuf-net 3.2.x - Protobuf serialization for game, GARM, and shared protocol endpoints in `Host/Program.cs`, `Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj`, and adapter wire files.
- Mediator.SourceGenerator 3.0.2 - Request/command handler pattern in `Application/Application.csproj` and `Application/Handlers/**`.
- MudBlazor 9.4.0 - Admin UI component framework in `TaikoWebUI/TaikoWebUI.csproj`.

**Testing:**
- xUnit 2.9.3 - Test framework in `Tests/Tests.csproj`.
- Microsoft.NET.Test.Sdk 17.14.1 - .NET test runner integration in `Tests/Tests.csproj`.

**Build/Dev:**
- MSBuild/.NET CLI - Solution file is `TaikoLocalServer.slnx`; shared build defaults are in `Directory.Build.props`.
- GitHub Actions - Windows publish workflow in `.github/workflows/publishTLS.yml`.
- Riok.Mapperly 4.3.1 - Source-generated mapping in adapter projects such as `Adapters.AdminApi/Adapters.AdminApi.csproj`.
- System.CommandLine 2.0.0-beta4.22272.1 - CLI argument handling in `GreenCatalogExtractor/Program.cs` and `LocalSaveModScoreMigrator/Program.cs`.
- Swashbuckle.AspNetCore 10.1.7 - Referenced by `Host/Host.csproj`; no Swagger middleware or endpoint registration was detected in `Host/Program.cs`.

## Key Dependencies

**Critical:**
- `Microsoft.EntityFrameworkCore.Sqlite` 10.0.7 - SQLite persistence for user data, scores, credentials, and era-specific save state in `Infrastructure/Infrastructure.csproj`.
- `protobuf-net` 3.2.56 and `protobuf-net.AspNetCore` 3.2.52 - Protobuf request/response handling for cabinet and GARM endpoints in `Host/Program.cs` and adapter projects.
- `Mediator.Abstractions` / `Mediator.SourceGenerator` 3.0.2 - Application command/query dispatching in `Application/Handlers/**`.
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.7 and `System.IdentityModel.Tokens.Jwt` 8.3.0 - Admin API and WebUI bearer-token authentication in `Infrastructure/DependencyInjection.cs`, `Infrastructure/Identity/JwtTokenService.cs`, and `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
- `BCrypt.Net-Next` 4.1.0 - Password hashing in `Adapters.AdminApi/Controllers/AuthController.cs`.
- `Otp.NET` 1.4.1 - Invite/admin OTP generation in `Adapters.AdminApi/Controllers/AuthController.cs`.
- `Serilog.AspNetCore` 10.0.0, `Serilog.Expressions` 5.0.0, and `Serilog.Sinks.File.Header` 1.0.2 - Structured request logging and CSV head-clerk log output in `Host/Program.cs` and `Host/Logging/CsvFormatter.cs`.
- `MudBlazor` 9.4.0 and `CodeBeam.MudBlazor.Extensions` 9.0.5 - UI components in `TaikoWebUI/TaikoWebUI.csproj`.

**Infrastructure:**
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

**Environment:**
- Host startup reads JSON configuration from `Host/Configurations/Kestrel.json`, `Host/Configurations/Logging.json`, `Host/Configurations/Database.json`, `Host/Configurations/ServerSettings.json`, `Host/Configurations/DataSettings.json`, and `Host/Configurations/AuthSettings.json` in `Host/Program.cs`.
- Required host JSON files at startup are `Host/Configurations/Logging.json`, `Host/Configurations/Database.json`, and `Host/Configurations/ServerSettings.json`.
- Optional host JSON files are `Host/Configurations/Kestrel.json`, `Host/Configurations/DataSettings.json`, and `Host/Configurations/AuthSettings.json`.
- Strongly typed settings are defined in `Infrastructure/Identity/Settings/AuthSettings.cs`, `Infrastructure/GameDataCatalog/Settings/DataSettings.cs`, `Application/Settings/ServerSettings.cs`, `Infrastructure/Settings/AllnetSettings.cs`, and `TaikoWebUI/Settings/WebUiSettings.cs`.
- The WebUI fetches server auth policy from `GET api/Auth/Config` and loads UI-only settings from `TaikoWebUI/wwwroot/appsettings.json` in `TaikoWebUI/Program.cs`.
- No `.env`, `.env.*`, or `*.env` files were detected by filename scan.

**Build:**
- SDK pin: `global.json`.
- Shared MSBuild defaults: `Directory.Build.props`.
- Central package versions: `Directory.Packages.props`.
- Solution: `TaikoLocalServer.slnx`.
- Host publish settings: `Host/Host.csproj` publishes non-Debug builds as self-contained single-file output.
- CI publish workflow: `.github/workflows/publishTLS.yml`.
- Launch profiles: `Host/Properties/launchSettings.json` and `TaikoWebUI/Properties/launchSettings.json`.

## Platform Requirements

**Development:**
- Install .NET 10 SDK matching `global.json`.
- Use NuGet restore through `dotnet restore`, `dotnet build`, or `dotnet test`.
- Provide operator/game data under `Host/wwwroot/data/nijiiro/datatable`, `Host/wwwroot/data/green/data`, and/or `Host/wwwroot/data/blue/data` when the corresponding era is enabled; required data layout is documented in `README.md` and `Host/README.md`.
- Green and Blue AC15 game-data directories are expected to be copied or symlinked from local game installs, as documented in `README.md` and `Host/README.md`.

**Production:**
- Deployment target is a local ASP.NET Core executable that hosts the game APIs and Blazor WebAssembly UI.
- Release publishing targets `win-x64` self-contained output in `.github/workflows/publishTLS.yml`.
- Runtime storage is local filesystem plus SQLite under the hosted `wwwroot` root; the default database name is `taiko.db3` from `Infrastructure/Persistence/PersistenceConstants.cs`.
- Docker, IIS deployment, and cloud hosting configuration were not detected.

---

*Stack analysis: 2026-05-28*
