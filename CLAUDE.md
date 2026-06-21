# CLAUDE.md

This file provides repo guidance for Claude Code when working in TaikoLocalServer.

## Agent Skills

Issues and PRDs live as local markdown under `.scratch/<feature-slug>/`; see `docs/agents/issue-tracker.md`.

Use the canonical triage labels in `docs/agents/triage-labels.md`.

Use the domain map in `docs/agents/domain.md` for root, adapter-era, and WebUI context.

## What This Is

TaikoLocalServer is a private Taiko no Tatsujin server for multiple cabinet eras. One ASP.NET Core 10 process serves game protobuf endpoints, AllNet/Mucha lifecycle endpoints, AdminApi routes, SQLite persistence, filesystem catalogs, and the Blazor WebAssembly admin UI.

Supported eras:

- Nijiiro CHN under `/v12r00_cn/*`
- Nijiiro WW under `/v12r08_ww/*`
- Green AC15 under `/v11r01/*`
- Blue AC15 under `/v10r03/*`
- Yellow AC15 under `/v09r02/*`
- Red AC15 under `/v08r01/*`
- White final AC15 under `/v07r03/*`, with legacy White compatibility under `/v07r00/*`
- Shared AC15 startup/version endpoints under `/v01r00/*`

Blue, Yellow, Red, and White are implemented as their own eras with era-owned persistence, handlers, DTO fields, catalogs, mappers, byte helpers, AdminApi routing, and WebUI routing.

## Solution Layout

- `Domain` - pure entities, enums, and constants.
- `Contracts.AdminApi` - DTOs shared by AdminApi and TaikoWebUI.
- `Application` - Mediator requests and handlers, ports, common DTOs, catalog contracts, settings, and protocol byte helpers.
- `Infrastructure` - EF Core SQLite, migrations, filesystem catalogs, JWT issuance, clock, and settings implementations.
- `Adapters.AdminApi` - `/api/...` controllers consumed by the WebUI.
- `Adapters.AllnetMucha` - AllNet, Mucha, updater, activation, and GARM endpoints.
- `Adapters.GameProtocol.Shared` - protocol controller base types and shared compression/header helpers.
- `Adapters.GameProtocol.WwR08` - Nijiiro WW protocol adapter.
- `Adapters.GameProtocol.CnR00` - Nijiiro CN protocol adapter.
- `Adapters.GameProtocol.Green` - Green AC15 protocol adapter.
- `Adapters.GameProtocol.Blue` - Blue AC15 protocol adapter.
- `Adapters.GameProtocol.Yellow` - Yellow AC15 protocol adapter.
- `Adapters.GameProtocol.Red` - Red AC15 protocol adapter.
- `Adapters.GameProtocol.White` - White AC15 protocol adapter.
- `Host` - runtime composition root and hosted WebUI files.
- `TaikoWebUI` - MudBlazor WebAssembly admin interface.
- `GreenCatalogExtractor` - Green catalog extraction utility.
- `LocalSaveModScoreMigrator` - local-save import utility.

Dependency direction stays inward: `Domain` has no outbound references; `Application` owns ports; `Infrastructure` implements ports; adapters translate inbound protocols; `Host` composes everything.

## Era Architecture

`GameEra` selects persistence, catalog, handlers, routes, and WebUI URLs. Shared handlers dispatch by era in the unsuffixed file, while era-specific logic lives beside it in `.Nijiiro.cs`, `.Green.cs`, `.Blue.cs`, `.Yellow.cs`, `.Red.cs`, or `.White.cs` partial files.

Apply the same split to:

- `Common*` DTO partials under `Application/Dtos`
- `ITaikoDbContext` partials under `Application/Abstractions`
- EF Core context partials under `Infrastructure/Persistence`
- era entities under `Domain/Entities`
- era catalog loaders under `Infrastructure/GameDataCatalog/<Era>`
- adapter mappers and controllers under `Adapters.GameProtocol.<Era>`

When adding behavior, decide whether it is shared, Nijiiro-only, Green-only, Blue-only, Yellow-only, Red-only, or White-only before editing shared files.

## Blue Caveats

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

## White Caveats

- Preserve White final `/v07r03/chassis/*` and legacy `/v07r00/chassis/*` as separate protocol surfaces. Final routes use `Adapters.GameProtocol.White.Wire`; legacy routes use `Adapters.GameProtocol.White.LegacyWire`.
- Preserve shared `/v01r00/chassis/*` startup/version route ownership for AC15.
- Keep White runtime state in White entities and handler partials. Do not read or write Blue, Green, Yellow, Red, Nijiiro, shop, battle, or ChallengeCompe state from White flows unless new evidence proves it.
- White Tokkun is supported only where proven: tutorial flag, raw append-only history rows, and recent-song upserts from practiced songs. Tokkun uploads must not fall through to normal/Dani/Don Challenge writes.
- White Banacoin-adjacent routes are stateless compatibility only; do not add wallet, balance, payment, coupon, or transaction persistence.
- White Don Challenge is server-side and stage-derived through `white_don_challenge_data.json` plus White-owned raw-fact/progress tables. It is exposed through dedicated AdminApi/WebUI contracts, not a standalone cabinet `challengecompe.php` route/readback.
- White catalog data is rooted at `config/ST7100-1`; later White-version behavior stays evidence-gated unless the final `/v07r03` quick task already proves it.

## Data Layout

Runtime data resolves beside the running executable:

- `wwwroot/data/nijiiro/` - Nijiiro operator JSON and game datatables.
- `wwwroot/data/green/data/` - Green AC15 `USRDIR/data`.
- `wwwroot/data/blue/data/` - Blue AC15 `USRDIR/data`.
- `wwwroot/data/yellow/data/` - Yellow AC15 `USRDIR/data`.
- `wwwroot/data/red/data/` - Red AC15 `USRDIR/data`.
- `wwwroot/data/white/data/` - White AC15 `USRDIR/data`.
- `wwwroot/data/shared/` - shared token, QR, and customization name data.
- `wwwroot/taiko.db3` - SQLite database.

Blue normal data requires:

- `config/S10100-1/musicinfo.xml`
- `config/S10100-1/musicmedleyinfo.xml`
- `fumen/tuning.bin`

Blue battle availability also requires the five parsed battle XML files under `config/S10100-1/battle`.

Yellow data requires `config/ST9100-1/musicinfo.xml`, `config/ST9100-1/musicmedleyinfo.xml`, `config/ST9100-1/defmusic.bin`, and `fumen/tuning.bin`.

Red data requires `config/ST8100-1/musicinfo.xml`, `config/ST8100-1/musicmedleyinfo.xml`, `config/ST8100-1/defmusic.bin`, and `fumen/tuning.bin`.

White data requires `config/ST7100-1/musicinfo.xml`, `config/ST7100-1/musicmedleyinfo.xml`, `config/ST7100-1/defmusic.bin`, `config/ST7100-1/present.xml`, `config/ST7100-1/spacialbaid.xml`, and `fumen/tuning.bin`.

Use `PathHelper.GetDataPath(GameEra era)` and era path helpers instead of hardcoded data paths.

## Common Commands

Run from the repo root:

```powershell
dotnet build TaikoLocalServer.slnx
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
dotnet run --project Host
dotnet publish Host/Host.csproj
dotnet ef migrations add <Name> --project Infrastructure --startup-project Host
dotnet ef database update --project Infrastructure --startup-project Host
dotnet run --project LocalSaveModScoreMigrator -- --save-file-path <path> --baid <id>
```

Use a temp output path for Host builds when a running server has locked `Host/bin/Debug/net10.0`.

## Implementation Rules

- Controllers deserialize, map, call Mediator, and map responses. Put runtime behavior in handlers.
- Do not query `ITaikoDbContext` directly from game protocol controllers.
- Do not add config to `Host/appsettings.json`; use `Host/Configurations/*.json`.
- New persisted fields need a domain entity change, EF configuration when needed, and an Infrastructure migration.
- AdminApi DTOs that the WebUI consumes belong in `Contracts.AdminApi`.
- Server-only data shapes belong in `Application/ServerData`.
- WebUI route construction should use `TaikoWebUI.Utilities.WebUiEra`.
- Keep generated protocol wire files out of manual edits unless regenerating them.

## Testing Rules

- Do not add tests just to satisfy a TDD checkbox. Every new test must protect a specific evidence-backed cabinet behavior, runtime state transition, parser/packing rule, AdminApi/WebUI workflow, or no-cross-era/no-cross-mode persistence boundary.
- Game-facing tests are regression guards after evidence, not proof of client compatibility. Cabinet/RPCS3/client acceptance remains the compatibility gate.
- Do not test generated protobuf output, generated `Wire/` type/property existence, route inventory, controller attribute lists, DI registration shape, enum numeric values, static config key presence, source text, project files, migrations, private methods, or "returns `Result = 1`" stateless echoes unless there is a demonstrated runtime failure that only that assertion can catch.
- Avoid assertions over `.cs`, `.csproj`, `Program.cs`, migrations, controller method bodies, class/file names, `Mediator.Send`, `SaveChanges`, reflection-only metadata, or other implementation strings.
- Useful tests exercise observable behavior: handler/service state changes, SQLite persistence and no-write boundaries, catalog/parser behavior, byte/bit packing owned by this repo, protocol payload classification backed by real captures/proto evidence, build/publish output when it affects deployed runtime files, API responses that drive WebUI behavior, and readback paths consumed by the cabinet.
- Mapper tests are allowed only when they protect nontrivial classification, omission, packing, or evidence-backed field placement. Do not write one-to-one copy/echo mapper tests.
