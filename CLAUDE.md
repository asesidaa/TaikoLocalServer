# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A private server emulator for Taiko no Tatsujin Nijiiro (CHN `v12r00_cn` and 39.06 `v12r08_ww`). The same ASP.NET Core 10 process serves the game's protobuf endpoints **and** hosts the Blazor WebAssembly admin UI on the same Kestrel server, so there is no separate frontend deployment. The codebase follows a hexagonal/ports-and-adapters layout: a pure `Domain` core, an `Application` orchestration layer that depends only on ports, an `Infrastructure` project that owns the real EF Core context + filesystem catalog + JWT issuer + clock, and one adapter project per inbound surface (admin REST, AllNet/Mucha lifecycle, and one per game protocol version).

## Solution layout

All `net10.0` unless noted (`docs/agents/domain.md` maps the domain terms, eras and WebUI context):

- **Domain** — pure entities, value objects, enums, domain constants. No package or project references. Namespace `TaikoLocalServer.Domain.*`.
- **Contracts.AdminApi** — DTO-only project: ViewModels, Requests, Responses, the WebUI-shared ServerData JSON shapes (DanData, MusicDetail, IVerupNo), and the `PlaySettingConverter` that the admin REST API and TaikoWebUI both serialize/deserialize. References Domain only (for enums). No business logic.
- **Application** — orchestration. Mediator request/handler types, port interfaces (`ITaikoDbContext`, `IGameDataCatalog`, `IJwtTokenService`, `IServerAccess`, `IClock`), Common* DTOs, server-only ServerData JSON shapes (EventFolderData, MovieData, QRCodeData, ShopFolderData, SongIntroductionData), application-only utilities, settings (`ServerSettings`). References Domain + Contracts.AdminApi (the latter brings in DanData/MusicDetail/IVerupNo for catalog signatures and handlers).
- **Infrastructure** — EF Core 10 + SQLite (`TaikoDbContext` implementing `ITaikoDbContext`), filesystem catalog (`FileGameDataCatalog`), `JwtTokenService`, `SystemClock`. Owns **all migrations** (under `Infrastructure/Persistence/Migrations/`). Migrations are applied automatically on server startup.
- **Adapters.AdminApi** — admin REST controllers (`/api/...`) consumed by TaikoWebUI. JWT bearer auth scheme registered here. Protect them with `[Authorize]` (see Auth below).
- **Adapters.AllnetMucha** — AmAuth/AmUpdater/Garmc/MuchaActivation controllers, the `AllNetRequestMiddleware` (zlib-decompresses base64 form bodies for `/sys/servlet/PowerOn`), and the Mucha + Garm wire types.
- **Adapters.GameProtocol.Shared** — `BaseProtocolController`, gzip/header-strip helpers shared by both game-protocol adapters.
- **Adapters.GameProtocol.WwR08** — protobuf controllers + wire types + Mapperly mappers for the 39.06 WW client. Routes `/v12r08_ww/...`.
- **Adapters.GameProtocol.CnR00** — protobuf controllers + wire types + Mapperly mappers for the CHN client. Routes `/v12r00_cn/...`.
- **Adapters.GameProtocol.{Green,Blue,Yellow,Red,White,Murasaki,Kimidori,Momoiro}** — the older (AC15) games, one adapter per era, each with its own routes (`/v11r01/...` Green, `/v10r03/...` Blue, …; startup/version on the shared `/v01r00/...`). See Eras below.
- **Host** — ASP.NET Core 10 entry point. Owns `Program.cs`, `Configurations/`, `Certificates/`, `wwwroot/`, `Logging/CsvFormatter.cs`. Hosts the Blazor UI via `UseBlazorFrameworkFiles()` + `MapFallbackToFile("index.html")`. `<AssemblyName>TaikoLocalServer</AssemblyName>` preserves the published exe name (`TaikoLocalServer.exe`).
- **TaikoWebUI** — Blazor WebAssembly admin UI: Tailwind v4 and small hand-written components (`Components/Ui`), no component library. References Contracts.AdminApi only. Referenced as a `<ProjectReference>` by Host so the WASM build artifacts are bundled into Host's publish output (then served via `UseBlazorFrameworkFiles()`).
- **Tests** — xunit tests (`dotnet test Tests/Tests.csproj`); `Tests/StressSeed` is a separate console project that builds a large database for load testing.
- **GreenCatalogExtractor** — console utility that extracts Green's customization catalogs.

Outside the .NET solution, `tools/extract/` is a Rust CLI (`cargo build --release --manifest-path tools/extract/Cargo.toml`) that copies the game files the server needs from an operator's Nijiiro install into `wwwroot/`. See its README.

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

# Add an EF Core migration. Infrastructure owns the context and is also the startup project (it references EF Tools; Host doesn't have EF Design). Stop the dev server first: it locks the build output.
dotnet ef migrations add <Name> --project Infrastructure --startup-project Infrastructure

# Update database manually (normally not needed — server migrates on startup)
dotnet ef database update --project Infrastructure --startup-project Infrastructure
```

Tests: `dotnet test Tests/Tests.csproj`. Some read an older game's data from `Host/wwwroot/data/<era>/data` and fail without it.

## Configuration model

`Host/appsettings.json` is intentionally near-empty. **Real configuration lives in `Host/Configurations/`** (loaded explicitly in `Program.cs`):

- `Kestrel.json` — multi-port binding. The server listens on **multiple ports simultaneously** (5000 base, 80 AmAuth, 10122 Mucha, 54430/54431 game WW, 57402 game CN, 443 Garmc) using one cert at `Host/Certificates/cert.pfx`.
- `Database.json` — only sets `DbFileName`. The DB lives at `<exe-dir>/wwwroot/<DbFileName>` (default `taiko.db3`).
- `ServerSettings.json` — **`Mode`: `Local` (no accounts or login; every authorization check passes) or `Server` (accounts, login, admin role)**; `Eras` (each era's `Enabled` and `GameDataPath`: Nijiiro's is the install `extract` reads from; the older eras' default to `wwwroot/data/<era>/data`; at least one must be on); `AllowRemoteGameDir` (admins may send the game folder from their browser instead: only the files `extract --inputs` lists, checked by `ExtractRunner.Match`, into a temp folder that the run deletes); `ApiTokens` (allow admins' API tokens, default off; Access › API tokens); `RestrictChassisIds` (only cabinets an admin allowed may connect; Server Management › Cabinets); `MuchaUrl`/`GameUrl`; `AvatarBrowserPath` (optional; blank finds Edge, then Chrome); `EnableMoreSongs` raises `DomainConstants.MusicIdMax` from 1600 to 9000 (use with caution per the warning in `Program.cs`).
- **Not config:** the Server-mode access rules (`OnlyAdmin`, `SignUpMode`, `AllowUserDelete`, `AllowFreeProfileEditing`, `AllowCustomTitles`, the access-code limit, lockout, 2-step verification Off/Optional/RequiredForAdmins) live in the database (`AccessRules`, admin-editable via `api/AccessRules`), and the JWT signing key is generated at first start (`ServerSecrets`). So is the server state (Operational / Pre-maintenance / Maintenance / Offline, `api/ServerState`), which the heartbeat reports as `game_svr_stat` and `CabinetFilter` enforces when Offline. All three are loaded by `IServerAccess` (`Infrastructure/Identity/ServerAccess.cs`). A legacy `AuthSettings.json`, if present, is imported once (its rules, and its own JWT key unless it's the old shipped default), and its `AuthenticationRequired` stands in for `Mode` when `Mode` is unset. The data JSON file names are fixed (`Infrastructure/GameDataCatalog/DataFiles.cs`).
- **Avatar renderer.** `Infrastructure/Avatars/HeadlessAvatarRenderer.cs` (PuppeteerSharp) opens the WebUI's `don-render.html` in a headless Edge/Chrome only while avatars are queued (a miss on `GET api/Users/{baid}/Avatar` or `/Still`, or a costume save; never a pass over every player, so thousands of players or a fresh migration cost nothing until their avatars are shown) and closes it after 5 idle minutes. It signs in with an in-memory token that only grants `read:assets`. It is off until an admin turns it on (Server Management › Status, `ServerOptions.RenderAvatars`, `api/AvatarRenderer`); while off, avatars are the player's initial on their card colour (browsers never draw or upload them). Stored images are keyed by `AvatarKeys` (Contracts.AdminApi): after changing the framing (`AvatarFraming` / `StillFraming` in the renderer), bump the key's version.
- `Logging.json` — Serilog. There is also a side-channel CSV sink in `Program.cs` (`HeadClerkLog-*.csv`) that captures only log messages starting with `"CSV WRITE:"` from the HeadClerk2 controller in `Adapters.GameProtocol.{WwR08,CnR00}`.

These files are copied to the publish output via `<None Include=... CopyToOutputDirectory="PreserveNewest"/>` in `Host/Host.csproj`. WebUI configuration is fetched at runtime from `wwwroot/appsettings.json` over HTTP (see `TaikoWebUI/Program.cs`).

## Where data files live (and why it matters)

`Infrastructure.GameDataCatalog.PathHelper.GetRootPath()` returns the **`wwwroot` directory next to the running executable** — not the project source's `wwwroot`. Anything under `Host/wwwroot/data/` is copied to that location on build/publish:

- `wwwroot/data/*.json` — **server-owned** datatables that operators edit (`dan_data.json`, `event_folder_data.json`, `gaiden_data.json`, `intro_data.json`, `locked_*_data.json`, `movie_data.json`, `qrcode_data.json`, `shop_folder_data.json`, `token_data.json`, `special_songs_data.json`). Documented in `Host/README.md`.
- `wwwroot/data/datatable/*.bin` — **game-owned** binary datatables (`musicinfo.bin`, `music_order.bin`, `wordlist.bin`, `don_cos_reward.bin`, `shougou.bin`, `neiro.bin`). These ship with the game install and **must be copied in by the operator before first run** (see top-level `README.md`). Decoded `*.json` siblings may also be present.
- `wwwroot/data/<era>/` — the older games' files (`data/` is that game's `USRDIR/data`, plus the era's own JSON); `wwwroot/data/shared/` — costume, title and tone names they share. Use the `PathHelper` era helpers, not hardcoded paths.
- `wwwroot/taiko.db3` — SQLite DB.

`IGameDataCatalog` (singleton, `FileGameDataCatalog`) holds one catalog per enabled era (`catalog.For(era)`); `catalog.Nijiiro()` is Nijiiro's (`NijiiroEraGameDataCatalog`), which loads all of the Nijiiro files above at startup via `await catalog.InitializeAsync()` in `Program.cs`, and again on `ReloadAsync()` (Server Management › Game data, or after the editors save). A reload swaps whole collections, so never mutate what a getter returns, and don't hold one across requests. Each file loads on its own: one that fails to parse keeps its previous contents and reports the error in `GetFileStatus()`; missing game datatables no longer stop the server from starting. The editors save through `SaveDataFileAsync`, which runs the same parser first, writes atomically and keeps a `.bak`.

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
- **New persisted field?** Add it to the entity in `Domain/Entities/`, then `dotnet ef migrations add ...` against `Infrastructure` (also the startup project). Migrations live under `Infrastructure/Persistence/Migrations/`. Never edit existing migration files.
- **New game endpoint?** Implement both `_cn` and `_ww` route variants in their respective adapter projects (`Adapters.GameProtocol.CnR00` and `Adapters.GameProtocol.WwR08`), each mapping into the same `Common*` request via Mapperly. Send through Mediator; never query `ITaikoDbContext` directly from a controller — that's a handler's job.
- **New admin API endpoint?** Add it to `Adapters.AdminApi/Controllers/`. The DTO types it accepts/returns live in `Contracts.AdminApi/{Requests,Responses,ViewModels}` so the WebUI can deserialize them. Protect it with `[Authorize]` (see Auth below).
- **New port?** Define the interface in `Application/Abstractions/`, implement it in `Infrastructure/`, and register both in `Infrastructure/DependencyInjection.cs`'s `AddInfrastructure(IConfiguration)`. Handlers depend on the port, never on the implementation.
- **New ServerData JSON shape?** If TaikoWebUI consumes it, put it in `Contracts.AdminApi/ServerData/`. If it's server-only (loaded by `FileGameDataCatalog`, consumed by handlers/mappers), put it in `Application/ServerData/`.
- **GlobalUsings.** Each project has its own `GlobalUsings.cs`. The Host's globals re-export Mediator, ProtoBuf where relevant. The Contracts.AdminApi globals re-export Domain.Enums and the project's own ViewModels. Read the relevant project's `GlobalUsings.cs` before adding explicit `using` statements that might already be global.
- **Auth.** Use `[Authorize]` / `[Authorize(Policy = AuthPolicies.Admin)]` on admin API endpoints, and `this.AuthorizeOwnerOrAdmin(baid)` for per-user data. In Local mode `AuthAwarePolicyEvaluator` makes every policy succeed, so the same attributes work in both modes. Read rules from `IServerAccess` (`LoginRequired`, `Rules`), never from config. Answer an input mistake (wrong password, wrong code) with 400, never 401: a 401 on a signed-in request makes the WebUI ask for the password again and resend it (`TaikoWebUI/Services/AppStatusHandler.cs`). `ServerStateFilter` answers 503 to players while the server is Offline (everything) or in Maintenance (anything but GET); mark endpoints that must stay open, like signing in and out, with `[OpenInEveryServerState]`.
- **Settings: config file or database?** Boot-time things an operator sets before starting (mode, ports/cert, DB name, game URLs, eras and their game paths, logging) go in `Host/Configurations/`. Rules an admin changes while the server runs go in the database and are edited in the admin UI. Keep setup simple: prefer generating a value over asking the operator for it.
- **Mediator handler conventions** (martinothamar's library, registered via `AddApplication()` in `Application/DependencyInjection.cs`):
  - Request types are `readonly record struct`, not `record class`.
  - `IRequestHandler<TRequest, TResponse>.Handle` returns `ValueTask<TResponse>`, not `Task<TResponse>`. For void requests use `IRequestHandler<TRequest>` and return `ValueTask<Unit>` ending with `return Unit.Value;`.
  - Service lifetime is **scoped** (set by `opt.ServiceLifetime = ServiceLifetime.Scoped`) because handlers inject `ITaikoDbContext`. Don't change this without also switching to `IDbContextFactory<TaikoDbContext>` and reworking the handlers.
  - Controllers always pass `HttpContext.RequestAborted` as the second argument to `Mediator.Send(...)` for cancellation hygiene.

## Eras

`GameEra` selects persistence, catalog, handlers, routes, and WebUI URLs. Shared handlers dispatch by era in the unsuffixed file, while era-specific logic lives beside it in `.Nijiiro.cs`, `.Green.cs`, `.Blue.cs`, `.Yellow.cs`, `.Red.cs`, or `.White.cs` (and `.Murasaki.cs`, `.Kimidori.cs`, `.Momoiro.cs`) partial files. Admin routes take the era too: `/api/{era}/...`, parsed with `EraRoute.TryParse`; the unprefixed routes are Nijiiro's. Nijiiro's score tables are `*Nijiiro` (`SongPlayDataNijiiro`, …), and its player fields are in `UserSaveDataNijiiro`, not `UserDatum`.

Apply the same split to:

- `Common*` DTO partials under `Application/Dtos`
- `ITaikoDbContext` partials under `Application/Abstractions`
- EF Core context partials under `Infrastructure/Persistence`
- era entities under `Domain/Entities`
- era catalog loaders under `Infrastructure/GameDataCatalog/<Era>`
- adapter mappers and controllers under `Adapters.GameProtocol.<Era>`

When adding behavior, decide whether it is shared, Nijiiro-only, Green-only, Blue-only, Yellow-only, Red-only, or White-only before editing shared files.

### Blue Caveats

- Preserve Blue direct-protobuf game request handling. Do not wrap Blue `playresult.php` like Green.
- Preserve shared `/v01r00/chassis/*` startup/version route ownership for AC15.
- Keep Blue normal play state separate from Green and Nijiiro.
- Keep Blue battle state in `BlueBattle*` entities and handlers.
- Battle payloads are classified by release or stage battle sections, not by guessed mode semantics.
- Blue battle playresult should branch before normal score, crown, Dani, profile, favorite, and normal unlock writes.
- Allowed battle side effects into normal-facing state are active shop Don medals and recent songs.
- Store and echo unresolved battle values; do not invent server-side reward, token, boss, or stage graph effects.
- Runtime token ids and NPC ids are zero-based. Preserve the response-side `TokenId - 1` mapping with `0` guarded.
- New battle users get the known starter state only when no persisted battle rows exist.
- Treat title id `0` as the empty/default title.
- Keep `rewardexecution.php` as log-and-success unless newer evidence proves a state mutation contract.

### White Caveats

- Preserve White final `/v07r03/chassis/*` and legacy `/v07r00/chassis/*` as separate protocol surfaces. Final routes use `Adapters.GameProtocol.White.Wire`; legacy routes use `Adapters.GameProtocol.White.LegacyWire`.
- Preserve shared `/v01r00/chassis/*` startup/version route ownership for AC15.
- Keep White runtime state in White entities and handler partials. Do not read or write Blue, Green, Yellow, Red, Nijiiro, shop, battle, or ChallengeCompe state from White flows unless new evidence proves it.
- White Tokkun is supported only where proven: tutorial flag, raw append-only history rows, and recent-song upserts from practiced songs. Tokkun uploads must not fall through to normal/Dani/Don Challenge writes.
- White Banacoin-adjacent routes are stateless compatibility only; do not add wallet, balance, payment, coupon, or transaction persistence.
- White Don Challenge is server-side and stage-derived through `white_don_challenge_data.json` plus White-owned raw-fact/progress tables. It is exposed through dedicated AdminApi/WebUI contracts, not a standalone cabinet `challengecompe.php` route/readback.
- White catalog data is rooted at `config/ST7100-1`; later White-version behavior stays evidence-gated unless the final `/v07r03` quick task already proves it.

## Testing Rules

- Do not add tests just to satisfy a TDD checkbox. Every new test must protect a specific evidence-backed cabinet behavior, runtime state transition, parser/packing rule, AdminApi/WebUI workflow, or no-cross-era/no-cross-mode persistence boundary.
- Game-facing tests are regression guards after evidence, not proof of client compatibility. Cabinet/RPCS3/client acceptance remains the compatibility gate.
- Do not test generated protobuf output, generated `Wire/` type/property existence, route inventory, controller attribute lists, DI registration shape, enum numeric values, static config key presence, source text, project files, migrations, private methods, or "returns `Result = 1`" stateless echoes unless there is a demonstrated runtime failure that only that assertion can catch.
- Avoid assertions over `.cs`, `.csproj`, `Program.cs`, migrations, controller method bodies, class/file names, `Mediator.Send`, `SaveChanges`, reflection-only metadata, or other implementation strings.
- Useful tests exercise observable behavior: handler/service state changes, SQLite persistence and no-write boundaries, catalog/parser behavior, byte/bit packing owned by this repo, protocol payload classification backed by real captures/proto evidence, build/publish output when it affects deployed runtime files, API responses that drive WebUI behavior, and readback paths consumed by the cabinet.
- Mapper tests are allowed only when they protect nontrivial classification, omission, packing, or evidence-backed field placement. Do not write one-to-one copy/echo mapper tests.
