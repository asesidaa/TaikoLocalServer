# External Integrations

**Analysis Date:** 2026-05-28

## APIs & External Services

**Arcade cabinet game protocols:**
- Taiko game cabinet endpoints - Local ASP.NET Core controllers emulate multiple game-era HTTP/protobuf APIs.
  - SDK/Client: ASP.NET Core MVC plus `protobuf-net`; host registration is in `Host/Program.cs`.
  - Auth: Cabinet routes do not use the Admin API JWT policy; route availability is controlled by `ServerSettings:Eras` in `Application/Settings/ServerSettings.cs` and `Host/Program.cs`.
  - Nijiiro routes: `Adapters.GameProtocol.WwR08/Controllers/**` exposes `/v12r08_ww/chassis/...`; `Adapters.GameProtocol.CnR00/Controllers/**` exposes `/v12r00_cn/chassis/...`.
  - Green routes: `Adapters.GameProtocol.Green/Controllers/**` exposes `/v11r01/chassis/...`.
  - Blue routes: `Adapters.GameProtocol.Blue/Controllers/**` exposes `/v10r03/chassis/...`.
  - Shared AC15 routes: `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`, `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs`, and `Adapters.GameProtocol.Shared/Controllers/VerupCompleteController.cs` expose `/v01r00/chassis/...`.

**ALL.Net, Mucha, and GARM emulation:**
- ALL.Net PowerOn - `Adapters.AllnetMucha/Controllers/AmAuth/PowerOnController.cs` exposes `/sys/servlet/PowerOn` and returns game server locations from `AllnetSettings.GameUrl`.
  - SDK/Client: ASP.NET Core MVC form endpoint plus `Adapters.AllnetMucha/Middleware/AllNetRequestMiddleware.cs`.
  - Auth: Local form protocol; no external provider.
- Mucha updater - `Adapters.AllnetMucha/Controllers/AmUpdater/MuchaController.cs` exposes `/mucha_front/boardauth.do`, `/mucha_front/updatacheck.do`, `/mucha_front/downloadstate.do`, and `/mucha_front/downloaderror.do`.
  - SDK/Client: ASP.NET Core MVC form endpoints.
  - Auth: Local form protocol; URL fields are derived from `AllnetSettings.MuchaUrl`.
- Mucha activation - `Adapters.AllnetMucha/Controllers/MuchaActivation/OtkController.cs` and `Adapters.AllnetMucha/Controllers/MuchaActivation/SignatureController.cs` expose `/mucha_activation/otk` and `/mucha_activation/signature`.
  - SDK/Client: ASP.NET Core MVC JSON endpoints.
  - Auth: Local protocol; generated OTK/signature response data.
- GARM protobuf endpoints - `Adapters.AllnetMucha/Controllers/Garmc/PingController.cs`, `Adapters.AllnetMucha/Controllers/Garmc/RegisterSystemBoardController.cs`, and `Adapters.AllnetMucha/Controllers/Garmc/RegisterSystemBoardBillingController.cs` expose `/v1/s12-jp-dev/garm...` endpoints.
  - SDK/Client: `protobuf-net` deserialization/serialization over ASP.NET Core MVC.
  - Auth: Local protocol; no external identity provider.

**Admin API and WebUI:**
- Same-origin admin API - `Adapters.AdminApi/Controllers/**` exposes `api/Auth`, `api/Users`, `api/UserSettings`, `api/GameData`, `api/PlayData`, `api/PlayHistory`, `api/DanBestData`, `api/FavoriteSongs`, `api/SongLeaderboard`, `api/Cards`, and era-prefixed variants.
  - SDK/Client: Blazor WebAssembly `HttpClient` in `TaikoWebUI/Program.cs`, `TaikoWebUI/Services/AuthService.cs`, `TaikoWebUI/Services/GameDataService.cs`, and UI pages/components.
  - Auth: Custom JWT bearer tokens issued by `Infrastructure/Identity/JwtTokenService.cs` and validated in `Infrastructure/DependencyInjection.cs`.

**Outbound HTTP APIs:**
- Not detected in server-side code. `HttpClient` usage is limited to WebUI same-origin calls and tests under `TaikoWebUI/**` and `Tests/WebUi/GameDataServiceTests.cs`.

## Data Storage

**Databases:**
- SQLite
  - Connection: `DbFileName` setting read from configuration in `Infrastructure/DependencyInjection.cs`; default file name is `taiko.db3` from `Infrastructure/Persistence/PersistenceConstants.cs`.
  - Client: Entity Framework Core `TaikoDbContext` in `Infrastructure/Persistence/TaikoDbContext.cs`.
  - Migrations: `Infrastructure/Persistence/Migrations/**`.
  - Startup migration: `Host/Program.cs` calls `db.Database.Migrate()` during application startup.

**File Storage:**
- Local filesystem only.
  - Host config: `Host/Configurations/*.json`.
  - WebUI config: `TaikoWebUI/wwwroot/appsettings.json`.
  - Nijiiro operator/game data: `Host/wwwroot/data/nijiiro/**`.
  - Shared operator data: `Host/wwwroot/data/shared/**`.
  - Green AC15 data: `Host/wwwroot/data/green/data` for local `USRDIR/data`, with generated/curated JSON under `Host/wwwroot/data/green/**`.
  - Blue AC15 data: `Host/wwwroot/data/blue/data` for local `USRDIR/data`.
  - Protocol schemas and generated wire models: `proto/**`, `Adapters.GameProtocol.Green/Wire/**`, `Adapters.GameProtocol.Blue/Wire/**`, and `Adapters.AllnetMucha/Wire/**`.
  - Logs: Serilog console logging plus CSV output at `./Logs/HeadClerkLog-.csv` from `Host/Program.cs`.
- Browser local storage
  - WebUI JWT token key `authToken` is managed by `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
  - UI preferences are stored through Blazored.LocalStorage in files such as `TaikoWebUI/Components/MainLayout.razor` and `TaikoWebUI/Components/LanguageToggle.razor`.

**Caching:**
- In-process memory cache is registered with `builder.Services.AddMemoryCache()` in `Host/Program.cs`.
- Browser local storage is used for client-side UI/auth state in `TaikoWebUI/**`.
- External cache services were not detected.

## Authentication & Identity

**Auth Provider:**
- Custom local authentication.
  - Implementation: Admin users authenticate with access-code/password credentials stored in SQLite through `ITaikoDbContext` and `TaikoDbContext`.
  - Password hashing: `BCrypt.Net-Next` in `Adapters.AdminApi/Controllers/AuthController.cs`.
  - Token issuance: HMAC SHA-256 JWTs in `Infrastructure/Identity/JwtTokenService.cs`.
  - Token validation: JWT bearer middleware in `Infrastructure/DependencyInjection.cs`.
  - Authorization: `AuthPolicies.Admin` in contracts plus `AuthAwarePolicyEvaluator` in `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs`.
  - Local mode: when `AuthSettings.AuthenticationRequired` is false, server policy checks pass and the WebUI synthesizes a local admin principal in `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`.
  - OTP/invite codes: `Otp.NET` TOTP generation and verification in `Adapters.AdminApi/Controllers/AuthController.cs`.

## Monitoring & Observability

**Error Tracking:**
- None detected. No Sentry, Application Insights, OpenTelemetry exporter, or external error tracking integration was found.

**Logs:**
- Serilog bootstrap and request logging are configured in `Host/Program.cs`.
- Logging configuration is loaded from `Host/Configurations/Logging.json`.
- ASP.NET Core HTTP logging is enabled in `Host/Program.cs`.
- CSV head-clerk logs are written through `Host/Logging/CsvFormatter.cs` and `Serilog.Sinks.File.Header`.
- Unknown and unsuccessful requests are logged in `Host/Program.cs`.

## CI/CD & Deployment

**Hosting:**
- Local/self-hosted ASP.NET Core executable. `Host/Host.csproj` configures non-Debug publish output as self-contained single-file with native libraries bundled.
- The host serves both cabinet APIs and the Blazor WebAssembly UI through `Host/Program.cs`.
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
- Standard ASP.NET Core environment variables may still apply through `WebApplication.CreateBuilder(args)`, such as `ASPNETCORE_ENVIRONMENT`, but no app-specific environment variable contract was detected.

**Configuration keys/files:**
- `DbFileName` - Database file name read by `Infrastructure/DependencyInjection.cs`, normally supplied by `Host/Configurations/Database.json`.
- `AuthSettings:JwtKey`, `AuthSettings:JwtIssuer`, `AuthSettings:JwtAudience`, `AuthSettings:AuthenticationRequired`, `AuthSettings:OnlyAdmin`, and related auth flags - defined by `Infrastructure/Identity/Settings/AuthSettings.cs` and loaded from `Host/Configurations/AuthSettings.json`.
- `ServerSettings:EnableMoreSongs`, `ServerSettings:MoreSongsSize`, and `ServerSettings:Eras` - defined by `Application/Settings/ServerSettings.cs` and loaded from `Host/Configurations/ServerSettings.json`.
- `ServerSettings:MuchaUrl` and `ServerSettings:GameUrl` - bound to `Infrastructure/Settings/AllnetSettings.cs`.
- `DataSettings` file-name overrides - defined by `Infrastructure/GameDataCatalog/Settings/DataSettings.cs` and loaded from `Host/Configurations/DataSettings.json`.
- `WebUiSettings` - defined by `TaikoWebUI/Settings/WebUiSettings.cs` and loaded from `TaikoWebUI/wwwroot/appsettings.json`.

**Secrets location:**
- `Host/Configurations/AuthSettings.json` contains local authentication secret material such as `JwtKey`; values were not read or copied.
- No `.env`, `.env.*`, `*.env`, `*secret*`, or `*credential*` files were detected by filename scan.

## Webhooks & Callbacks

**Incoming:**
- No generic webhook endpoint was detected.
- Incoming routes are local cabinet/admin protocol APIs:
  - Admin JSON API under `api/**` in `Adapters.AdminApi/Controllers/**`.
  - Game cabinet protobuf APIs under `/v12r08_ww/chassis/**`, `/v12r00_cn/chassis/**`, `/v11r01/chassis/**`, `/v10r03/chassis/**`, and `/v01r00/chassis/**`.
  - ALL.Net/Mucha/GARM routes under `/sys/servlet/PowerOn`, `/mucha_front/**`, `/mucha_activation/**`, and `/v1/s12-jp-dev/garm...`.

**Outgoing:**
- None detected in application code. Runtime integrations are local file/database access and responses to inbound HTTP/protobuf/form requests.

---

*Integration audit: 2026-05-28*
