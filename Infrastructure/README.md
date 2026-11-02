# Infrastructure

Concrete implementations of every port defined in
`Application/Abstractions/`. Owns EF Core, SQLite, the filesystem catalog,
JWT issuance, and the system clock.

## Role

The outer ring of the hexagonal architecture. Talks to the world (database,
filesystem, time, crypto) on behalf of `Application`. Also owns **all** EF
Core migrations under `Persistence/Migrations/`; the server applies them
automatically on startup.

## Dependencies

- Inbound: `Adapters.AdminApi`, `Adapters.AllnetMucha`, `Host`.
- Outbound: `Domain`, `Contracts.AdminApi`, `Application`.
- Notable packages: `Microsoft.EntityFrameworkCore.Sqlite`,
  `Microsoft.EntityFrameworkCore.Tools`, `BCrypt.Net-Next`,
  `EntityFrameworkCore.Exceptions.Sqlite`, `SharpZipLib`,
  `System.IdentityModel.Tokens.Jwt`,
  `Microsoft.AspNetCore.Authentication.JwtBearer`,
  `Yoh.Text.Json.NamingPolicies`.

## Key folders

- `Persistence/` — `TaikoDbContext` (implements `ITaikoDbContext`) +
  EF Core configurations + **all migrations** under
  `Persistence/Migrations/`.
- `GameDataCatalog/` — `FileGameDataCatalog` (implements
  `IGameDataCatalog`), `PathHelper` (resolves `wwwroot` next to the
  exe), `CatalogConstants`.
- `Identity/` — `JwtTokenService`, `ServerAccess` (mode, access rules, server
  state), session and API-token checks, the cabinet registry.
- `Time/` — `SystemClock` (implements `IClock`).
- `Settings/` — `IOptions`-bound settings types specific to
  Infrastructure concerns (file paths, JWT, etc.).
- `DependencyInjection.cs` — `AddInfrastructure(IConfiguration)`
  extension that wires up `TaikoDbContext`, the catalog, the JWT
  service, the clock, and Infrastructure-owned settings.

## When to add code here

- A new implementation of a port defined in `Application/Abstractions/`.
- A new EF Core entity configuration.
- A new EF Core migration: from the repo root,
  `dotnet ef migrations add <Name> --project Infrastructure --startup-project Infrastructure`.

Do **not** add: port **interfaces** (→ `Application/Abstractions/`), HTTP
controllers (→ `Adapters.*`), domain entities (→ `Domain`), or admin-API
DTOs (→ `Contracts.AdminApi`).

See the root `CLAUDE.md` for the full hexagonal layout.

## Eras

The catalog is per era: `FileGameDataCatalog` holds one catalog for each era enabled in `Host/Configurations/ServerSettings.json`, and `catalog.Nijiiro()` (Application) reaches Nijiiro's, `Nijiiro/NijiiroEraGameDataCatalog`, which reads `wwwroot/data` and `wwwroot/data/datatable` (where `extract` writes) and is the one the Game data editors, dan sets and reloads work on. The older eras read `wwwroot/data/<era>/`, and the names they share from `wwwroot/data/shared/`.

## AC15 Notes

- AC15 game data is read from each era's configured `GameDataPath`, normally `wwwroot/data/<era>/data`.
- Green loads `config/S11100-1/musicinfo.xml`, `config/S11100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
- Blue loads `config/S10100-1/musicinfo.xml`, `config/S10100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
- Yellow loads `config/ST9100-1/musicinfo.xml`, `config/ST9100-1/musicmedleyinfo.xml`, `config/ST9100-1/defmusic.bin`, and `fumen/tuning.bin`.
- Red loads `config/ST8100-1/musicinfo.xml`, `config/ST8100-1/musicmedleyinfo.xml`, `config/ST8100-1/defmusic.bin`, and `fumen/tuning.bin`.
- White loads `config/ST7100-1/musicinfo.xml`, `config/ST7100-1/musicmedleyinfo.xml`, `config/ST7100-1/defmusic.bin`, `config/ST7100-1/present.xml`, `config/ST7100-1/spacialbaid.xml`, and `fumen/tuning.bin`.
- Green, Blue, and Yellow item-shop JSON use the shared AC15 item-shop loader, including string-first enum values with numeric compatibility at the loader edge.
- Red and White Don Challenge JSON use the shared AC15 Don Challenge loader and feed era-owned progress/readback state.

## Era-Specific Notes

- `BlueEraGameDataCatalog` loads normal catalog data, optional JSON catalogs, item-shop data, customization data, and battle catalog data.
- Blue battle availability is enabled only when the five battle XML files under `config/S10100-1/battle` are present and parseable.
- Blue customization catalogs can be generated from Blue AC15 data when `AutoExtractCatalog` is enabled.
- Blue Tokkun persistence belongs to `UserSaveDataBlue` and `BlueTokkunStageResult`; do not add Banacoin wallet/payment tables for Tokkun compatibility.
- Blue battle persistence belongs to BlueBattle entities and migrations; do not reuse Green AI Battle state.
- White customization catalogs can be generated from White AC15 data when `AutoExtractCatalog` is enabled, with shared or override display-name data.
- White Tokkun persistence belongs to `UserSaveDataWhite` and `WhiteTokkunStageResult`; White Banacoin compatibility remains stateless.
