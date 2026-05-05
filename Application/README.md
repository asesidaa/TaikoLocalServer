# Application

Orchestration layer. Owns the Mediator request/handler pipeline, port
interfaces, and version-agnostic Common* DTOs. Knows about the Domain but
not about EF Core, the filesystem, JWT, or HTTP wire types.

## Role

The "use case" layer of the hexagonal architecture. Handlers receive
`Common*` DTOs (mapped from version-specific protobuf in the
`Adapters.GameProtocol.*` projects) and orchestrate calls to ports
(`ITaikoDbContext`, `IGameDataCatalog`, `IJwtTokenService`, `IClock`).
Concrete implementations of those ports live in `Infrastructure`.

## Dependencies

- Inbound: `Infrastructure`, `Adapters.AdminApi`, `Adapters.AllnetMucha`,
  `Adapters.GameProtocol.Shared`, `Adapters.GameProtocol.WwR08`,
  `Adapters.GameProtocol.CnR00`, `Host`.
- Outbound: `Domain`, `Contracts.AdminApi`. Plus `Microsoft.AspNetCore.App`
  framework reference (for `IFormFile` on a few admin handlers — not for
  routing / hosting).
- Mediator namespace: set to `TaikoLocalServer.Application` in
  `DependencyInjection.cs`.

## Key folders

- `Abstractions/` — port interfaces (`ITaikoDbContext`,
  `IGameDataCatalog`, `IJwtTokenService`, `IClock`).
- `Handlers/` — Mediator request/handler types. Convention:
  `readonly record struct *Query : IRequest<TResponse>` +
  `*QueryHandler : IRequestHandler<*Query, TResponse>` returning
  `ValueTask<TResponse>`.
- `Common/` — common helper utilities used by handlers.
- `Dtos/` — version-agnostic Common* DTOs (`CommonBaidResponse`,
  `CommonScoreData`, ...).
- `Catalog/` — types describing in-memory catalog content used by
  `IGameDataCatalog` consumers.
- `ServerData/` — server-only JSON shapes (loaded by `FileGameDataCatalog`
  but never sent to the WebUI: `EventFolderData`, `MovieData`,
  `QRCodeData`, `ShopFolderData`, `SongIntroductionData`, ...).
- `Settings/` — `IOptions`-bound settings types (`ServerSettings`,
  `DataSettings`).
- `DependencyInjection.cs` — `AddApplication()` extension that registers
  Mediator + handlers + settings.

## When to add code here

- New use case → add a `*Query`/`*Command` + `*Handler` under `Handlers/`.
- New port interface → add to `Abstractions/`, register implementation in
  `Infrastructure/DependencyInjection.cs`'s `AddInfrastructure(...)`.
- Server-only `ServerData` JSON shape (not consumed by the WebUI).
- New version-agnostic Common* DTO.

Do **not** add: EF Core context or migrations (→ `Infrastructure`),
admin-API DTOs the WebUI binds (→ `Contracts.AdminApi`), HTTP controllers
(→ `Adapters.*`), JWT issuance / SQLite / catalog file I/O
(→ `Infrastructure`).

See the root `CLAUDE.md` for the full hexagonal layout.
