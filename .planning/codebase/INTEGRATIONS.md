# External Integrations

**Analysis Date:** 2026-06-11

## APIs & External Services

**Arcade cabinet game protocols:**
- Taiko game cabinet endpoints - Local ASP.NET Core controllers emulate multiple game-era HTTP/protobuf APIs.
  - SDK/Client: ASP.NET Core MVC plus `protobuf-net`; host registration is in `Host/Program.cs`.
  - Auth: Cabinet routes do not use the Admin API JWT policy; route availability is controlled by `ServerSettings:Eras` in `Host/Configurations/ServerSettings.json`, `Application/Settings/ServerSettings.cs`, and `Host/Program.cs`.
  - Nijiiro WW routes: `Adapters.GameProtocol.WwR08/Controllers/**` exposes `/v12r08_ww/chassis/*`.
  - Nijiiro CN routes: `Adapters.GameProtocol.CnR00/Controllers/**` exposes `/v12r00_cn/chassis/*`.
  - Green AC15 routes: `Adapters.GameProtocol.Green/Controllers/**` exposes `/v11r01/chassis/*`.
  - Blue AC15 routes: `Adapters.GameProtocol.Blue/Controllers/**` exposes `/v10r03/chassis/*`, including Blue-only `battleuserdata.php` and Blue stateless Banacoin-adjacent compatibility routes.
  - Yellow AC15 routes: `Adapters.GameProtocol.Yellow/Controllers/**` exposes `/v09r02/chassis/*`, including normal play, shop, Tokkun-compatible playresult/userdata, and stateless Banacoin-adjacent compatibility routes.
  - Shared AC15 startup/version routes: `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`, `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs`, and `Adapters.GameProtocol.Shared/Controllers/VerupCompleteController.cs` expose `/v01r00/chassis/*`.

**AC15 shared-core capability surface:**
- Shared Green/Blue/Yellow behavior lives in `Application/Ac15/**` and is consumed by era partial handlers in `Application/Handlers/*.Green.cs`, `Application/Handlers/*.Blue.cs`, and `Application/Handlers/*.Yellow.cs`.
  - SDK/Client: Internal application services and Mapperly projections; no external network client.
  - Auth: Not applicable.
  - Shared capability examples: `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Ac15/Ac15DaniWriter.cs`, `Application/Ac15/Ac15ItemShopPurchase.cs`, `Application/Ac15/Ac15CatalogSnapshotFactory.cs`, `Application/Ac15/Ac15UserDataService.cs`, and `Application/Ac15/Ac15InitialDataService.cs`.
  - Persistence boundary: direct `ITaikoDbContext` plus concrete era `DbSet` selection in `Application/Abstractions/ITaikoDbContext.*.cs`; generic helpers operate over narrow Domain row-shape interfaces such as `Domain/Entities/IAc15SongPlayDatum.cs`, `Domain/Entities/IAc15ShopItemState.cs`, and `Domain/Entities/IAc15SaveDataCapabilities.cs`.

**ALL.Net, Mucha, and GARM emulation:**
- ALL.Net PowerOn - `Adapters.AllnetMucha/Controllers/AmAuth/PowerOnController.cs` exposes `/sys/servlet/PowerOn` and returns game server locations from `AllnetSettings.GameUrl`.
  - SDK/Client: ASP.NET Core MVC form endpoint plus `Adapters.AllnetMucha/Middleware/AllNetRequestMiddleware.cs`.
  - Auth: Local form protocol; no external provider.
- Mucha updater - `Adapters.AllnetMucha/Controllers/AmUpdater/MuchaController.cs` exposes `/mucha_front/boardauth.do`, `/mucha_front/updatacheck.do`, `/mucha_front/downloadstate.do`, `/mucha_front/downloaderror.do`, `/mucha_front/regiauth.do`, `/mucha_front/tokenstate.do`, and `/mucha_front/tokenmarginstate.do`.
  - SDK/Client: ASP.NET Core MVC form endpoints; `BouncyCastle.Cryptography` and `SharpZipLib` support local crypto/compression helpers in `Adapters.AllnetMucha/Common/MuchaCrypto.cs` and `Adapters.AllnetMucha/Middleware/AllNetRequestMiddleware.cs`.
  - Auth: Local form protocol; URL fields are derived from `AllnetSettings.MuchaUrl`.
- Mucha activation - `Adapters.AllnetMucha/Controllers/MuchaActivation/OtkController.cs` and `Adapters.AllnetMucha/Controllers/MuchaActivation/SignatureController.cs` expose `/mucha_activation/otk` and `/mucha_activation/signature`.
  - SDK/Client: ASP.NET Core MVC JSON endpoints.
  - Auth: Local protocol; generated OTK/signature response data.
- GARM protobuf endpoints - `Adapters.AllnetMucha/Controllers/Garmc/PingController.cs`, `Adapters.AllnetMucha/Controllers/Garmc/RegisterSystemBoardController.cs`, and `Adapters.AllnetMucha/Controllers/Garmc/RegisterSystemBoardBillingController.cs` expose `/v1/s12-jp-dev/garm...` endpoints.
  - SDK/Client: `protobuf-net` deserialization/serialization over ASP.NET Core MVC.
  - Auth: Local protocol; no external identity provider.

**Admin API and WebUI:**
- Same-origin admin API - `Adapters.AdminApi/Controllers/**` exposes legacy `/api/...` routes and era-prefixed `/api/{era}/...` routes for profile, score, history, favorite, leaderboard, Dani, settings, game-data, customization, cards, and authentication workflows.
  - SDK/Client: Blazor WebAssembly `HttpClient` in `TaikoWebUI/Program.cs`, `TaikoWebUI/Services/AuthService.cs`, `TaikoWebUI/Services/GameDataService.cs`, and UI pages/components.
  - Auth: Custom JWT bearer tokens issued by `Infrastructure/Identity/JwtTokenService.cs` and validated in `Infrastructure/DependencyInjection.cs`.
  - Era routing: `Adapters.AdminApi/Controllers/EraRoute.cs` parses Admin API era route segments, and `TaikoWebUI/Utilities/WebUiEra.cs` composes WebUI routes/API paths for `Nijiiro`, `Green`, `Blue`, and `Yellow`.
  - Auth/era bootstrap: `Adapters.AdminApi/Controllers/AuthController.cs` returns enabled eras through `GET api/Auth/Config`, consumed by `TaikoWebUI/Program.cs`.

**Outbound HTTP APIs:**
- Not detected in server-side runtime code. `HttpClient` usage is limited to WebUI same-origin calls and tests in `TaikoWebUI/**` and `Tests/WebUi/GameDataServiceTests.cs`.
- External-looking `MuchaUrl` and `GameUrl` values in `Host/Configurations/ServerSettings.json` are configuration values returned to cabinet protocols through `Infrastructure/Settings/AllnetSettings.cs`; no outbound runtime call site was detected.
- `Infrastructure/GameDataCatalog/Green/Extractor/Sources/WikiScraper.cs` is a stub that returns an empty dictionary and does not perform network I/O.

## Data Storage

**Databases:**
- SQLite
  - Connection: `DbFileName` setting read from configuration in `Infrastructure/DependencyInjection.cs`; default file name is `taiko.db3` from `Infrastructure/Persistence/PersistenceConstants.cs`.
  - Client: Entity Framework Core `TaikoDbContext` in `Infrastructure/Persistence/TaikoDbContext.cs`.
  - Shared identity tables: `Cards`, `Credentials`, `UserData`, and `Tokens` are exposed in `Infrastructure/Persistence/TaikoDbContext.Shared.cs` and `Application/Abstractions/ITaikoDbContext.Shared.cs`.
  - Nijiiro tables: `Infrastructure/Persistence/TaikoDbContext.Nijiiro.cs`.
  - Green AC15 tables: `Infrastructure/Persistence/TaikoDbContext.Green.cs`.
  - Blue AC15 tables: `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, including Blue battle and Blue Tokkun state.
  - Yellow AC15 tables: `Infrastructure/Persistence/TaikoDbContext.Yellow.cs`, including Yellow normal, Dani, shop, favorite/recent, and Tokkun state.
  - Migrations: `Infrastructure/Persistence/Migrations/**`.
  - Startup migration: `Host/Program.cs` calls `db.Database.Migrate()` during application startup.

**File Storage:**
- Local filesystem only.
  - Host config: `Host/Configurations/*.json`.
  - WebUI config: `TaikoWebUI/wwwroot/appsettings.json`.
  - Nijiiro operator/server data: `Host/wwwroot/data/nijiiro/**`.
  - Shared operator/server data: `Host/wwwroot/data/shared/**`.
  - Green AC15 server JSON and local `USRDIR/data` root: `Host/wwwroot/data/green/**`.
  - Blue AC15 server JSON and local `USRDIR/data` root: `Host/wwwroot/data/blue/**`.
  - Yellow AC15 server JSON and local `USRDIR/data` root: `Host/wwwroot/data/yellow/**`.
  - AC15 shared catalog loaders: `Infrastructure/GameDataCatalog/Ac15/**`.
  - Era catalog loaders: `Infrastructure/GameDataCatalog/Green/**`, `Infrastructure/GameDataCatalog/Blue/**`, and `Infrastructure/GameDataCatalog/Yellow/**`.
  - Protocol schemas and generated wire models: `proto/**`, `Adapters.GameProtocol.Green/Wire/**`, `Adapters.GameProtocol.Blue/Wire/**`, `Adapters.GameProtocol.Yellow/Wire/**`, and `Adapters.AllnetMucha/Wire/**`.
  - Logs: Serilog console logging plus text and CSV files under `./Logs/` from `Host/Program.cs` and `Host/Logging/CsvFormatter.cs`.
- Browser local storage
  - WebUI JWT token key `authToken` is managed by `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
  - UI preferences are stored through Blazored.LocalStorage in files such as `TaikoWebUI/Components/MainLayout.razor`, `TaikoWebUI/Components/LanguageToggle.razor`, and score/history pages.

**Caching:**
- In-process memory cache is registered with `builder.Services.AddMemoryCache()` in `Host/Program.cs`.
- Era game-data catalogs are singleton services initialized once at startup through `IGameDataCatalog.InitializeAsync()` in `Host/Program.cs` and implemented by `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`.
- Browser local storage is used for client-side UI/auth state in `TaikoWebUI/**`.
- External cache services such as Redis or Memcached were not detected.

## Authentication & Identity

**Auth Provider:**
- Custom local authentication.
  - Implementation: Admin users authenticate with access-code/password credentials stored in SQLite through `ITaikoDbContext` and `TaikoDbContext`.
  - Password hashing: `BCrypt.Net-Next` in `Adapters.AdminApi/Controllers/AuthController.cs`.
  - Token issuance: HMAC SHA-256 JWTs in `Infrastructure/Identity/JwtTokenService.cs`.
  - Token validation: JWT bearer middleware in `Infrastructure/DependencyInjection.cs`.
  - Authorization: `AuthPolicies.Admin` in `Contracts.AdminApi/Authorization/AuthPolicies.cs` plus `AuthAwarePolicyEvaluator` in `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs`.
  - Local mode: when `AuthSettings.AuthenticationRequired` is false, server policy checks pass and the WebUI synthesizes a local admin principal in `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
  - OTP/invite codes: `Otp.NET` TOTP generation and verification in `Adapters.AdminApi/Controllers/AuthController.cs`.
  - Configuration: settings shape is defined in `Infrastructure/Identity/Settings/AuthSettings.cs`; local values live in `Host/Configurations/AuthSettings.json` and should be treated as secret-bearing.

## Monitoring & Observability

**Error Tracking:**
- None detected. No Sentry, Application Insights, OpenTelemetry exporter, Datadog, New Relic, Prometheus, or external error tracking integration was found in project files or runtime registration.

**Logs:**
- Serilog bootstrap and request logging are configured in `Host/Program.cs`.
- Logging configuration is loaded from `Host/Configurations/Logging.json`.
- ASP.NET Core HTTP logging is enabled in `Host/Program.cs`.
- CSV head-clerk logs are written through `Host/Logging/CsvFormatter.cs` and `Serilog.Sinks.File.Header`.
- Unknown and unsuccessful requests are logged in `Host/Program.cs`.

## CI/CD & Deployment

**Hosting:**
- Local/self-hosted ASP.NET Core executable. `Host/Host.csproj` configures non-Debug publish output as self-contained single-file with native libraries bundled.
- The host serves cabinet APIs, Admin API, static files, and the Blazor WebAssembly UI through `Host/Program.cs`.
- Kestrel endpoints are configured by `Host/Configurations/Kestrel.json`; certificate values are configuration data and should not be copied into documentation.
- Docker and IIS deployment configuration were not detected.

**CI Pipeline:**
- GitHub Actions workflow in `.github/workflows/publishTLS.yml`.
  - Runner: `windows-latest`.
  - SDK setup: `actions/setup-dotnet@v5` with .NET `10.0.x`.
  - Build: `dotnet workload restore Host/Host.csproj` and `dotnet publish Host/Host.csproj -c Release -r win-x64 --self-contained true --nologo`.
  - Artifacts: `actions/upload-artifact@v7` uploads publish output on `dev`.
  - Releases: `gh release create` publishes same-day nightly prereleases on `dev` using `${{ github.token }}`.

## Environment Configuration

**Required env vars:**
- Not detected for application runtime. No explicit `Environment.GetEnvironmentVariable(...)` calls were found.
- Standard ASP.NET Core environment variables may still apply through `WebApplication.CreateBuilder(args)`, and development launch profiles set `ASPNETCORE_ENVIRONMENT` in `Host/Properties/launchSettings.json` and `TaikoWebUI/Properties/launchSettings.json`.

**Configuration keys/files:**
- `DbFileName` - Database file name read by `Infrastructure/DependencyInjection.cs`, normally supplied by `Host/Configurations/Database.json`.
- `AuthSettings:JwtKey`, `AuthSettings:JwtIssuer`, `AuthSettings:JwtAudience`, `AuthSettings:AuthenticationRequired`, `AuthSettings:OnlyAdmin`, and related auth flags - defined by `Infrastructure/Identity/Settings/AuthSettings.cs` and loaded from `Host/Configurations/AuthSettings.json`.
- `ServerSettings:EnableMoreSongs`, `ServerSettings:MoreSongsSize`, and `ServerSettings:Eras` - defined by `Application/Settings/ServerSettings.cs` and loaded from `Host/Configurations/ServerSettings.json`.
- `ServerSettings:Eras:<Era>:Enabled` - controls adapter registration and MVC application parts in `Host/Program.cs`.
- `ServerSettings:Eras:<Era>:GameDataPath`, `AutoExtractCatalog`, `CustomizationNameDataPath`, `EnableShop`, and `ActiveShopSeasonId` - AC15 catalog/shop settings consumed by `Infrastructure/GameDataCatalog/**` and validated by `Application/Settings/ServerSettingsOptionsValidationExtensions.cs`.
- `ServerSettings:MuchaUrl` and `ServerSettings:GameUrl` - bound to `Infrastructure/Settings/AllnetSettings.cs`.
- `DataSettings` file-name overrides - defined by `Infrastructure/GameDataCatalog/Settings/DataSettings.cs` and loaded from `Host/Configurations/DataSettings.json`.
- `WebUiSettings` - defined by `TaikoWebUI/Settings/WebUiSettings.cs` and loaded from `TaikoWebUI/wwwroot/appsettings.json`.

**Secrets location:**
- `Host/Configurations/AuthSettings.json` contains local authentication secret material such as JWT signing configuration; values were not copied into this document.
- `Host/Configurations/Kestrel.json` contains local TLS certificate configuration; values were not copied into this document.
- No `.env`, `.env.*`, or `*.env` files were detected by filename scan.

## Webhooks & Callbacks

**Incoming:**
- No generic webhook endpoint was detected.
- Incoming routes are local cabinet/admin protocol APIs:
  - Admin JSON API under `/api/**` and `/api/{era}/**` in `Adapters.AdminApi/Controllers/**`.
  - Game cabinet protobuf APIs under `/v12r08_ww/chassis/**`, `/v12r00_cn/chassis/**`, `/v11r01/chassis/**`, `/v10r03/chassis/**`, `/v09r02/chassis/**`, and `/v01r00/chassis/**`.
  - ALL.Net/Mucha/GARM routes under `/sys/servlet/PowerOn`, `/mucha_front/**`, `/mucha_activation/**`, and `/v1/s12-jp-dev/garm...`.

**Outgoing:**
- None detected in application runtime code. Runtime integrations are local file/database access and responses to inbound HTTP/protobuf/form requests.
- WebUI outgoing HTTP is same-origin to the server's own `/api/**` routes through `TaikoWebUI/Program.cs` and `TaikoWebUI/Services/**`.

---

*Integration audit: 2026-06-11*
