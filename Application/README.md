# Application

Orchestration layer. Owns the Mediator request/handler pipeline, port
interfaces, and version-agnostic Common* DTOs. Knows about the Domain but
not about EF Core, the filesystem, JWT, or HTTP wire types.

## Role

The "use case" layer of the hexagonal architecture. Handlers receive
`Common*` DTOs (mapped from version-specific protobuf in the
`Adapters.GameProtocol.*` projects) and orchestrate calls to ports
(`ITaikoDbContext`, `IGameDataCatalog`, `IServerAccess`, `IClock`, ...).
Concrete implementations of those ports live in `Infrastructure`.

## Dependencies

- Inbound: `Infrastructure`, `Adapters.AdminApi`, `Adapters.AllnetMucha`,
  `Adapters.GameProtocol.Shared`, `Adapters.GameProtocol.WwR08`,
  `Adapters.GameProtocol.CnR00`, `Host`.
- Outbound: `Domain`, `Contracts.AdminApi`. Plus the `Microsoft.AspNetCore.App`
  framework reference (not for routing / hosting).
- Mediator namespace: set to `TaikoLocalServer.Application` in
  `DependencyInjection.cs`.

## Key folders

- `Abstractions/` — port interfaces (`ITaikoDbContext`, `IGameDataCatalog`,
  `IJwtTokenService`, `IServerAccess`, `ICabinetRegistry`,
  `IExtractRunner`, `ILogBuffer`, `IClock`).
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
- `Settings/` — `IOptions`-bound settings types (`ServerSettings`, with
  `Mode` and `Eras`: each era's `Enabled` and `GameDataPath`).
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

## Eras

Nijiiro and AC15 behaviors use the repo's partial-file pattern: shared dispatch in the unsuffixed file and era-specific behavior in `.Nijiiro.cs`, `.Green.cs`, `.Blue.cs`, `.Yellow.cs`, `.Red.cs`, or `.White.cs`.

## AC15 Notes

- Keep Blue normal, Tokkun, and battle behavior in Blue partial handlers.
- Handle Blue Tokkun playresults before battle or normal save logic. Tokkun persistence is limited to nullable tutorial state, append-only raw protocol history rows, and recent-song upserts from practiced Tokkun song numbers.
- Keep battle state store-and-echo unless a field has concrete client, log, proto, or IDA evidence.
- Do not let battle-classified playresults fall through to normal Blue score, crown, Dani, profile, favorite, or normal unlock writes.
- Do not let Tokkun-classified playresults fall through to normal, battle, Dani, profile, favorite, unlock, or shop writes. Recent-song upserts from Tokkun stage data are the allowed exception.
- Use Blue-owned DTO fields and byte helpers for fixed-width Blue payloads.
- Keep Yellow, Red, and White runtime behavior in their own partial handlers. Shared AC15 services may remove duplicated algorithms, but concrete `ITaikoDbContext` DbSet selection and save-row ownership stay era-specific.
- White Tokkun writes only White tutorial/history facts and recent-song rows from practiced songs. It must not fall through to White normal, Dani, Don Challenge, profile, favorite, unlock, or cross-era state.
- White Don Challenge is stage-derived server behavior through `Ac15DonChallenge` helpers and White-owned tables; it is not a ChallengeCompe protocol readback path.
