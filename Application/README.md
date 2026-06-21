# Application

Application is the use-case layer. It owns Mediator requests and handlers, port interfaces, version-agnostic `Common*` DTOs, server data shapes, settings, and protocol byte helpers.

## Role

Protocol adapters map their wire DTOs into `Common*` DTOs, then send Mediator requests into this project. Handlers orchestrate ports such as `ITaikoDbContext`, `IGameDataCatalog`, `IJwtTokenService`, and `IClock`. Concrete implementations live in `Infrastructure`.

Nijiiro and AC15 behaviors use the repo's partial-file pattern: shared dispatch in the unsuffixed file and era-specific behavior in `.Nijiiro.cs`, `.Green.cs`, `.Blue.cs`, `.Yellow.cs`, `.Red.cs`, or `.White.cs`.

## Key Folders

- `Abstractions/` - port interfaces and era catalog contracts.
- `Handlers/` - Mediator request and handler types.
- `Common/` - shared handler utilities and protocol byte helpers.
- `Dtos/` - version-agnostic request/response DTOs passed between adapters and handlers.
- `Catalog/` - immutable in-memory catalog models consumed by handlers.
- `ServerData/` - server-only JSON shapes loaded by the filesystem catalog.
- `Settings/` - `IOptions`-bound settings such as `ServerSettings`.
- `DependencyInjection.cs` - registers Mediator and application settings.

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

## When To Add Code Here

- Add a new `*Query` or `*Command` and handler for a use case.
- Add a port interface before implementing it in `Infrastructure`.
- Add a server-only data shape that is not consumed directly by the WebUI.
- Add or extend a `Common*` DTO used by protocol mappers.

Do not add EF Core context code, migrations, HTTP controllers, JWT issuance, or filesystem I/O here.
