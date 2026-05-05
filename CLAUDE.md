# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A private server emulator for Taiko no Tatsujin Nijiiro (CHN `v12r00_cn` and 39.06 `v12r08_ww`). The same ASP.NET Core 10 process serves the game's protobuf endpoints **and** hosts the Blazor WebAssembly admin UI on the same Kestrel server, so there is no separate frontend deployment. The codebase follows a hexagonal/ports-and-adapters layout: a pure `Domain` core, an `Application` orchestration layer that depends only on ports, an `Infrastructure` project that owns the real EF Core context + filesystem catalog + JWT issuer + clock, and one adapter project per inbound surface (admin REST, AllNet/Mucha lifecycle, and one per game protocol version).

## Solution layout

12 projects, all `net10.0` unless noted:

- **Domain** — pure entities, value objects, enums, domain constants. No package or project references. Namespace `TaikoLocalServer.Domain.*`.
- **Contracts.AdminApi** — DTO-only project: ViewModels, Requests, Responses, the WebUI-shared ServerData JSON shapes (DanData, MusicDetail, IVerupNo), and the `PlaySettingConverter` that the admin REST API and TaikoWebUI both serialize/deserialize. References Domain only (for enums). No business logic.
- **Application** — orchestration. Mediator request/handler types, port interfaces (`ITaikoDbContext`, `IGameDataCatalog`, `IJwtTokenService`, `IClock`), Common* DTOs, server-only ServerData JSON shapes (EventFolderData, MovieData, QRCodeData, ShopFolderData, SongIntroductionData), application-only utilities, settings (`ServerSettings`, `DataSettings`). References Domain + Contracts.AdminApi (the latter brings in DanData/MusicDetail/IVerupNo for catalog signatures and handlers).
- **Infrastructure** — EF Core 10 + SQLite (`TaikoDbContext` implementing `ITaikoDbContext`), filesystem catalog (`FileGameDataCatalog`), `JwtTokenService`, `SystemClock`. Owns **all migrations** (under `Infrastructure/Persistence/Migrations/`). Migrations are applied automatically on server startup.
- **Adapters.AdminApi** — admin REST controllers (`/api/...`) consumed by TaikoWebUI. JWT bearer auth scheme registered here. Apply `[AuthorizeIfRequired]` on these.
- **Adapters.AllnetMucha** — AmAuth/AmUpdater/Garmc/MuchaActivation controllers, the `AllNetRequestMiddleware` (zlib-decompresses base64 form bodies for `/sys/servlet/PowerOn`), and the Mucha + Garm wire types.
- **Adapters.GameProtocol.Shared** — `BaseProtocolController`, gzip/header-strip helpers shared by both game-protocol adapters.
- **Adapters.GameProtocol.WwR08** — protobuf controllers + wire types + Mapperly mappers for the 39.06 WW client. Routes `/v12r08_ww/...`.
- **Adapters.GameProtocol.CnR00** — protobuf controllers + wire types + Mapperly mappers for the CHN client. Routes `/v12r00_cn/...`.
- **Host** — ASP.NET Core 10 entry point. Owns `Program.cs`, `Configurations/`, `Certificates/`, `wwwroot/`, `Logging/CsvFormatter.cs`. Hosts the Blazor UI via `UseBlazorFrameworkFiles()` + `MapFallbackToFile("index.html")`. `<AssemblyName>TaikoLocalServer</AssemblyName>` preserves the published exe name (`TaikoLocalServer.exe`).
- **TaikoWebUI** — Blazor WebAssembly admin UI (MudBlazor 9). References Contracts.AdminApi only. Referenced as a `<ProjectReference>` by Host so the WASM build artifacts are bundled into Host's publish output (then served via `UseBlazorFrameworkFiles()`).
- **LocalSaveModScoreMigrator** — standalone CLI (System.CommandLine) for importing local-save-mod JSON dumps into `taiko.db3`. References Infrastructure to reuse the EF Core context. Not part of the runtime.

The dependency direction is strictly: `Domain ← Contracts.AdminApi ← Application ← Infrastructure ← Host`, with the adapters slotting in at the right layer (`Adapters.* → Application` for game/admin/AllNet logic; Host references all adapters and Infrastructure to compose the runtime). Domain has zero outbound references.

## Adapter contract

Every adapter project ships a `DependencyInjection.cs` that defines exactly one extension method on `IServiceCollection` (or `IApplicationBuilder` if it needs middleware wiring), e.g. `AddAdminApi(IConfiguration)`, `AddAllnetMucha()` / `UseAllnetMucha()`, `AddGameProtocolWwR08()`, `AddGameProtocolCnR00()`. Adding a new game version means: create `Adapters.GameProtocol.<Version>/`, add one `<ProjectReference>` to Host, add one `Add...()` call in `Program.cs`. **Adapters never reference each other** (except `GameProtocol.WwR08` and `GameProtocol.CnR00` may both reference `GameProtocol.Shared`). Cross-adapter coordination flows through the Application layer.

## Common commands

Run from the repo root:

```bash
# Build the whole solution
dotnet build

# Publish a self-contained single-file Windows exe (Release config sets PublishSingleFile/SelfContained)
dotnet publish Host/Host.csproj
# Output: Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe

# Run the server in dev (auto-applies migrations)
dotnet run --project Host

# Add an EF Core migration — note the split: Infrastructure owns the context, Host is the startup project
dotnet ef migrations add <Name> --project Infrastructure --startup-project Host

# Update database manually (normally not needed — server migrates on startup)
dotnet ef database update --project Infrastructure --startup-project Host

# Run the score migrator
dotnet run --project LocalSaveModScoreMigrator -- --save-file-path <path> --baid <id>
```

There are no automated tests in this repository.

## Configuration model

`Host/appsettings.json` is intentionally near-empty. **Real configuration lives in `Host/Configurations/`** (loaded explicitly in `Program.cs`):

- `Kestrel.json` — multi-port binding. The server listens on **multiple ports simultaneously** (5000 base, 80 AmAuth, 10122 Mucha, 54430/54431 game WW, 57402 game CN, 443 Garmc) using one cert at `Host/Certificates/cert.pfx`.
- `Database.json` — only sets `DbFileName`. The DB lives at `<exe-dir>/wwwroot/<DbFileName>` (default `taiko.db3`).
- `ServerSettings.json` — `EnableMoreSongs` raises `DomainConstants.MusicIdMax` from 1600 to 9000 (use with caution per the warning in `Program.cs`).
- `DataSettings.json` — filenames for the server-owned JSON datatables under `wwwroot/data/`.
- `AuthSettings.json` — JWT issuer/audience/key. **`AuthenticationRequired: false` disables auth on all admin API endpoints** (the `AuthorizeIfRequiredAttribute` filter short-circuits).
- `Logging.json` — Serilog. There is also a side-channel CSV sink in `Program.cs` (`HeadClerkLog-*.csv`) that captures only log messages starting with `"CSV WRITE:"` from the HeadClerk2 controller in `Adapters.GameProtocol.{WwR08,CnR00}`.

These files are copied to the publish output via `<None Include=... CopyToOutputDirectory="PreserveNewest"/>` in `Host/Host.csproj`. WebUI configuration is fetched at runtime from `wwwroot/appsettings.json` over HTTP (see `TaikoWebUI/Program.cs`).

## Where data files live (and why it matters)

`Infrastructure.GameDataCatalog.PathHelper.GetRootPath()` returns the **`wwwroot` directory next to the running executable** — not the project source's `wwwroot`. Anything under `Host/wwwroot/data/` is copied to that location on build/publish:

- `wwwroot/data/*.json` — **server-owned** datatables that operators edit (`dan_data.json`, `event_folder_data.json`, `gaiden_data.json`, `intro_data.json`, `locked_*_data.json`, `movie_data.json`, `qrcode_data.json`, `shop_folder_data.json`, `token_data.json`, `special_songs_data.json`). Documented in `Host/README.md`.
- `wwwroot/data/datatable/*.bin` — **game-owned** binary datatables (`musicinfo.bin`, `music_order.bin`, `wordlist.bin`, `don_cos_reward.bin`, `shougou.bin`, `neiro.bin`). These ship with the game install and **must be copied in by the operator before first run** (see top-level `README.md`). Decoded `*.json` siblings may also be present.
- `wwwroot/taiko.db3` — SQLite DB.

`IGameDataCatalog` (singleton, implemented in Infrastructure as `FileGameDataCatalog`) loads all of the above once at startup via `await catalog.InitializeAsync()` in `Program.cs`. Treat its in-memory dictionaries as immutable for the lifetime of the process.

## Request architecture (game side)

The game protocol uses **protobuf-net** over HTTP. Requests are gzip-compressed and (for some endpoints) prefixed with a 32-byte header that controllers strip before deserializing.

Two game versions are supported in parallel and cleanly separated:

- `Adapters.GameProtocol.CnR00/Wire/` — protobuf wire types for the CHN client. Controller routes use `/v12r00_cn/...`.
- `Adapters.GameProtocol.WwR08/Wire/` — protobuf wire types for the 39.06 WW client. Controller routes use `/v12r08_ww/...`.
- `Application/Dtos/` — version-agnostic Common* DTOs.

The flow: adapter controller deserializes the version-specific request → **Riok.Mapperly** source-generated mapper (in the same adapter project) converts it to a `Common*` DTO in Application → **Mediator** (martinothamar) `Handlers/*Query`/`*Command` in Application operates only on `Common*` types → mapper converts the response back to the requested version's protobuf type. Adding a new game version means adding a new `Adapters.GameProtocol.<Version>/` project (Wire types + mappers + controllers + a `DependencyInjection.cs`); handlers and DB code do not change.

Controllers in `Adapters.GameProtocol.*` inherit `BaseProtocolController` (in `Adapters.GameProtocol.Shared`). Controllers in `Adapters.AdminApi` inherit `BaseAdminController`. Both lazily resolve `IMediator` and `Logger` from `HttpContext.RequestServices`. Don't inject these via constructor — match the existing pattern.

## Conventions to follow

- **Don't add config to `Host/appsettings.json`.** Add a new section to one of the files in `Host/Configurations/`, register it in `Program.cs` (or in the relevant adapter's `DependencyInjection.cs`), and add a `<None Include="Configurations/Foo.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>` block to `Host/Host.csproj`.
- **New persisted field?** Add it to the entity in `Domain/Entities/`, then `dotnet ef migrations add ...` against `Infrastructure` with `Host` as the startup project. Migrations live under `Infrastructure/Persistence/Migrations/`. Never edit existing migration files.
- **New game endpoint?** Implement both `_cn` and `_ww` route variants in their respective adapter projects (`Adapters.GameProtocol.CnR00` and `Adapters.GameProtocol.WwR08`), each mapping into the same `Common*` request via Mapperly. Send through Mediator; never query `ITaikoDbContext` directly from a controller — that's a handler's job.
- **New admin API endpoint?** Add it to `Adapters.AdminApi/Controllers/`. The DTO types it accepts/returns live in `Contracts.AdminApi/{Requests,Responses,ViewModels}` so the WebUI can deserialize them. Apply `[AuthorizeIfRequired]`.
- **New port?** Define the interface in `Application/Abstractions/`, implement it in `Infrastructure/`, and register both in `Infrastructure/DependencyInjection.cs`'s `AddInfrastructure(IConfiguration)`. Handlers depend on the port, never on the implementation.
- **New ServerData JSON shape?** If TaikoWebUI consumes it, put it in `Contracts.AdminApi/ServerData/`. If it's server-only (loaded by `FileGameDataCatalog`, consumed by handlers/mappers), put it in `Application/ServerData/`.
- **GlobalUsings.** Each project has its own `GlobalUsings.cs`. The Host's globals re-export Mediator, ProtoBuf where relevant. The Contracts.AdminApi globals re-export Domain.Enums and the project's own ViewModels. Read the relevant project's `GlobalUsings.cs` before adding explicit `using` statements that might already be global.
- **Auth.** Use `[AuthorizeIfRequired]` (in `Adapters.AdminApi/Filters/`) on admin API endpoints so the operator-controlled `AuthSettings.AuthenticationRequired` toggle works.
- **Mediator handler conventions** (martinothamar's library, registered via `AddApplication()` in `Application/DependencyInjection.cs`):
  - Request types are `readonly record struct`, not `record class`.
  - `IRequestHandler<TRequest, TResponse>.Handle` returns `ValueTask<TResponse>`, not `Task<TResponse>`. For void requests use `IRequestHandler<TRequest>` and return `ValueTask<Unit>` ending with `return Unit.Value;`.
  - Service lifetime is **scoped** (set by `opt.ServiceLifetime = ServiceLifetime.Scoped`) because handlers inject `ITaikoDbContext`. Don't change this without also switching to `IDbContextFactory<TaikoDbContext>` and reworking the handlers.
  - Controllers always pass `HttpContext.RequestAborted` as the second argument to `Mediator.Send(...)` for cancellation hygiene.
