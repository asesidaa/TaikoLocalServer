# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A private server emulator for Taiko no Tatsujin Nijiiro (CHN `v12r00_cn` and 39.06 `v12r08_ww`). The same ASP.NET Core 8 process serves the game's protobuf endpoints **and** hosts the Blazor WebAssembly admin UI on the same Kestrel server, so there is no separate frontend deployment.

## Solution layout

5 projects, all `net8.0`:

- **TaikoLocalServer** — ASP.NET Core 8 host. Implements game endpoints (protobuf-net) and the admin REST API. Hosts the Blazor UI via `UseBlazorFrameworkFiles()` + `MapFallbackToFile("index.html")`.
- **TaikoWebUI** — Blazor WebAssembly admin UI (MudBlazor). Referenced as a project by TaikoLocalServer so its build artifacts are bundled.
- **SharedProject** — DTOs, enums, request/response models, and `PathHelper` shared between server and WebUI. WebUI's `GlobalUsings.cs` re-exports its `Models`/`Enums` namespaces.
- **GameDatabase** — EF Core 8 + SQLite. Owns `TaikoDbContext`, all entities, and **all migrations**. Migrations are applied automatically on server startup (`db.Database.Migrate()` in `Program.cs`).
- **LocalSaveModScoreMigrator** — Standalone CLI (System.CommandLine) for importing local-save-mod JSON dumps into `taiko.db3`. Not part of the runtime.

## Common commands

Run from the repo root:

```bash
# Build the whole solution
dotnet build

# Publish a self-contained single-file Windows exe (Release config sets PublishSingleFile/SelfContained)
dotnet publish
# Output: TaikoLocalServer/bin/Release/net8.0/win-x64/publish/

# Run the server in dev (auto-applies migrations)
dotnet run --project TaikoLocalServer

# Add an EF Core migration — note the split: GameDatabase owns the context, TaikoLocalServer is the startup project
dotnet ef migrations add <Name> --project GameDatabase --startup-project TaikoLocalServer

# Update database manually (normally not needed — server migrates on startup)
dotnet ef database update --project GameDatabase --startup-project TaikoLocalServer

# Run the score migrator
dotnet run --project LocalSaveModScoreMigrator -- --save-file-path <path> --baid <id>
```

There are no automated tests in this repository.

## Configuration model

`TaikoLocalServer/appsettings.json` is intentionally near-empty. **Real configuration lives in `TaikoLocalServer/Configurations/`** (loaded explicitly in `Program.cs`):

- `Kestrel.json` — multi-port binding. The server listens on **multiple ports simultaneously** (5000 base, 80 AmAuth, 10122 Mucha, 54430/54431 game WW, 57402 game CN, 443 Garmc) using one cert at `Certificates/cert.pfx`.
- `Database.json` — only sets `DbFileName`. The DB lives at `<exe-dir>/wwwroot/<DbFileName>` (default `taiko.db3`).
- `ServerSettings.json` — `EnableMoreSongs` raises `Constants.MusicIdMax` from 1600 to 9000 (use with caution per the warning in `Program.cs`).
- `DataSettings.json` — filenames for the server-owned JSON datatables under `wwwroot/data/`.
- `AuthSettings.json` — JWT issuer/audience/key. **`AuthenticationRequired: false` disables auth on all admin API endpoints** (the `AuthorizeIfRequiredAttribute` filter short-circuits).
- `Logging.json` — Serilog. There is also a side-channel CSV sink in `Program.cs` (`HeadClerkLog-*.csv`) that captures only log messages starting with `"CSV WRITE:"` from `HeadClerk2Controller`.

These files are copied to the publish output via `<None Include=... CopyToOutputDirectory="PreserveNewest"/>` in `TaikoLocalServer.csproj`. WebUI configuration is fetched at runtime from `wwwroot/appsettings.json` over HTTP (see `TaikoWebUI/Program.cs`).

## Where data files live (and why it matters)

`PathHelper.GetRootPath()` returns the **`wwwroot` directory next to the running executable** — not the project source's `wwwroot`. Anything under `TaikoLocalServer/wwwroot/data/` is copied to that location on build/publish:

- `wwwroot/data/*.json` — **server-owned** datatables that operators edit (`dan_data.json`, `event_folder_data.json`, `gaiden_data.json`, `intro_data.json`, `locked_*_data.json`, `movie_data.json`, `qrcode_data.json`, `shop_folder_data.json`, `token_data.json`, `special_songs_data.json`). Documented in `TaikoLocalServer/README.md`.
- `wwwroot/data/datatable/*.bin` — **game-owned** binary datatables (`musicinfo.bin`, `music_order.bin`, `wordlist.bin`, `don_cos_reward.bin`, `shougou.bin`, `neiro.bin`). These ship with the game install and **must be copied in by the operator before first run** (see top-level `README.md`). Decoded `*.json` siblings may also be present.
- `wwwroot/taiko.db3` — SQLite DB.

`IGameDataService` (singleton) loads all of the above once at startup via `await gameDataService.InitializeAsync()` in `Program.cs`. Treat its in-memory dictionaries as immutable for the lifetime of the process.

## Request architecture (game side)

The game protocol uses **protobuf-net** over HTTP. Requests are gzip-compressed and (for some endpoints) prefixed with a 32-byte header that controllers strip before deserializing.

Two game versions are supported in parallel and cleanly separated:

- `Models/cn_r00/` — protobuf models for the CHN client. Controller routes use `/v12r00_cn/...`.
- `Models/ww_r08/` — protobuf models for the 39.06 WW client. Controller routes use `/v12r08_ww/...`.
- `Models/Application/Common*` — version-agnostic DTOs.

The flow: controller deserializes the version-specific request → **Riok.Mapperly** source-generated mapper in `Mappers/` converts it to a `Common*` DTO → MediatR `Handlers/*Query`/`*Command` operates only on `Common*` types → mapper converts the response back to the requested version's protobuf type. Adding a new game version means adding a new model namespace + mapper overloads; handlers and DB code should not need to change.

Controllers inherit `BaseController<T>` which lazily resolves `Mediator` (`ISender`) and `Logger` from `HttpContext.RequestServices`. Don't inject these via constructor — match the existing pattern.

Controller folders are organized by audience, not by route prefix:

- `Controllers/Api/` — admin REST API consumed by TaikoWebUI (`/api/...`). Apply `[AuthorizeIfRequired]` on these.
- `Controllers/Game/` — protobuf game endpoints (most of the surface).
- `Controllers/AmAuth/`, `Controllers/AmUpdater/`, `Controllers/Garmc/`, `Controllers/MuchaActivation/` — Allnet/Mucha lifecycle. The `/sys/servlet/PowerOn` path goes through `AllNetRequestMiddleware` (zlib-decompresses base64 form bodies) — wired conditionally in `Program.cs`.

## Conventions to follow

- **Don't add config to `appsettings.json`.** Add a new section to one of the files in `Configurations/`, register it in `Program.cs`, and add a `<None Include="Configurations/Foo.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>` block to `TaikoLocalServer.csproj`.
- **New persisted field?** Add it to the entity in `GameDatabase/Entities/`, then `dotnet ef migrations add ...` against `GameDatabase` with `TaikoLocalServer` as the startup project. Never edit existing migration files.
- **New game endpoint?** Implement both `_cn` and `_ww` route variants in the same controller, mapping into the same `Common*` request via Mapperly. Send through MediatR; never query the DbContext directly from a controller.
- **GlobalUsings.** `TaikoLocalServer/GlobalUsings.cs` re-exports `GameDatabase.Entities`, `MediatR`, `ProtoBuf`, `SharedProject.Enums`, and the project's own `Common`/`Handlers`/`Models`/`Services` namespaces — don't add explicit `using`s for those.
- **Auth.** Use `[AuthorizeIfRequired]` (not `[Authorize]`) on admin API endpoints so the operator-controlled `AuthSettings.AuthenticationRequired` toggle works.
