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

- Inbound: `Adapters.AdminApi`, `Adapters.AllnetMucha`, `Host`,
  `LocalSaveModScoreMigrator`.
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
- `Identity/` — `JwtTokenService` (implements `IJwtTokenService`) +
  helpers.
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
  `dotnet ef migrations add <Name> --project Infrastructure --startup-project Host`.

Do **not** add: port **interfaces** (→ `Application/Abstractions/`), HTTP
controllers (→ `Adapters.*`), domain entities (→ `Domain`), or admin-API
DTOs (→ `Contracts.AdminApi`).

See the root `CLAUDE.md` for the full hexagonal layout.
