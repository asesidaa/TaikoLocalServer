# Agent Notes

TaikoLocalServer is an ASP.NET Core 10 host for Taiko cabinet protocol endpoints, SQLite persistence, era-specific game-data catalogs, and the Blazor WebAssembly admin UI. The repo now supports Nijiiro, Green AC15, and Blue AC15 in the same process. Blue is a first-class era, not a Green variant.

## Current Blue State

- Blue game routes live under `/v10r03/chassis/*`.
- Shared AC15 startup and version routes remain under `/v01r00/chassis/*`.
- Blue request bodies are direct protobuf for game endpoints; preserve that transport unless current client evidence proves otherwise.
- Blue normal play supports profile/login, userdata, initial data, self-best, crowns, recent/favorite songs, Dani Dojo, item shop purchase/unlock state, event folders, telops, movies, recommendations, tournaments, gacha data, and WebUI/AdminApi era routing.
- Blue battle support uses Blue-owned persistence and handlers for `battleuserdata.php`, `initialdatacheck.php`, and battle-classified `playresult.php` payloads.
- Blue battle runtime state is store-and-echo where semantics are not proven. Do not invent token rewards, boss completion, stage graph behavior, stage 33 behavior, or normal unlock mirrors without concrete client/log/proto/IDA evidence.
- Battle playresults must not write normal Blue score, crown, Dani, profile, favorite, or normal unlock state. Active shop Don medals and recent songs are allowed Blue side effects.
- Preserve known battle ID contracts: runtime token ids and NPC ids are zero-based; response-side persisted token rows use the `TokenId - 1` mapping with `0` guarded.
- New users receive the IDA-backed starter battle state only when no persisted Blue battle state exists; after playresult, `battleuserdata.php` reads back persisted BlueBattle rows.

## Repo Layout

- `Host/` composes the ASP.NET Core process, loads `Host/Configurations/*.json`, applies migrations, initializes catalogs, registers enabled-era adapters, hosts static WebUI files, and serves fallback routing.
- `Domain/` owns entities, enums, and constants. It has no project references.
- `Contracts.AdminApi/` owns DTOs shared by the admin API and WebUI.
- `Application/` owns Mediator requests, handlers, ports, `Common*` DTOs, server data shapes, and protocol byte helpers.
- `Infrastructure/` implements persistence, filesystem catalogs, identity, time, settings, and EF migrations.
- `Adapters.AdminApi/` serves `/api/...` routes for the WebUI.
- `Adapters.AllnetMucha/` serves cabinet lifecycle, Mucha, updater, activation, and GARM endpoints.
- `Adapters.GameProtocol.Shared/` owns shared protocol controller and compression helpers.
- `Adapters.GameProtocol.WwR08/` serves Nijiiro WW routes under `/v12r08_ww/*`.
- `Adapters.GameProtocol.CnR00/` serves Nijiiro CN routes under `/v12r00_cn/*`.
- `Adapters.GameProtocol.Green/` serves Green AC15 routes under `/v11r01/*`.
- `Adapters.GameProtocol.Blue/` serves Blue AC15 routes under `/v10r03/*`.
- `TaikoWebUI/` is the MudBlazor WebAssembly admin UI hosted by `Host`.
- `GreenCatalogExtractor/` and `LocalSaveModScoreMigrator/` are standalone utilities.
- `proto/` contains protocol schema inputs; generated wire models live in adapter `Wire/` folders.
- `.tools/blue/` contains local Blue reverse-engineering material. Treat it as local evidence, not runtime code.

## Architecture Rules

- Keep era state separate. Blue, Green, and Nijiiro persistence must remain separate unless the state is truly shared identity data such as card/user identity.
- Use the existing partial-file pattern for era behavior: shared dispatcher in the unsuffixed file, era implementation in `.Nijiiro.cs`, `.Green.cs`, or `.Blue.cs`.
- Map generated protobuf DTOs through `Application/Dtos/Common*` shapes before handler logic. Do not persist wire DTOs directly.
- Controllers should deserialize, map, call Mediator, and map back. Put business behavior in `Application/Handlers`.
- Use `IGameDataCatalog.For(GameEra)` and era catalog interfaces instead of hardcoded filesystem access from handlers.
- Resolve runtime data roots through `PathHelper` and era data path helpers. Do not hardcode `wwwroot/data/<era>` in new runtime code.
- For AdminApi era routes, preserve both legacy routes where they exist and `/api/{era}/...` routes validated by `EraRoute.TryParse`.
- Keep generated `Wire/` files out of manual cleanup unless regenerating protocol output.

## Data Caveats

- Blue and Green AC15 game data is operator-supplied under `Host/wwwroot/data/<era>/data` in source checkouts, or `wwwroot/data/<era>/data` in published folders.
- Blue normal catalog data comes from `config/S10100-1/musicinfo.xml`, `config/S10100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
- Blue battle availability requires the five parsed files under `config/S10100-1/battle`: `battleadjsetting.xml`, `battlenpcinfo.xml`, `battlestageinfo.xml`, `battlesupportinfo.xml`, and `battletokeninfo.xml`.
- Blue item shop data is committed JSON under `Host/wwwroot/data/blue/blue_item_shop_data.json`; `rewardshopdata.bin` remains local provenance and is not a runtime dependency.
- Blue customization JSON can be bootstrapped from Blue AC15 data when `AutoExtractCatalog` is enabled, with display names composed from shared and optional override name data.
- Treat title id `0` as the explicit empty/default title state.
- `rewardexecution.php` is log-and-success for Blue item-shop flow unless newer evidence proves a state-changing role.

## Common Commands

Run from the repo root:

```powershell
dotnet build TaikoLocalServer.slnx
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
dotnet run --project Host
dotnet publish Host/Host.csproj
dotnet ef migrations add <Name> --project Infrastructure --startup-project Host
dotnet ef database update --project Infrastructure --startup-project Host
```

Use the temp-output Host build when a running server locks `Host/bin/Debug/net10.0`.
