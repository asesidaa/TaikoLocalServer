# Adapters.GameProtocol.CnR00

Inbound adapter for the **CHN** game client (`v12r00_cn`). Owns the
protobuf wire types, Mapperly mappers, and HTTP controllers for that
game version. Routes are under `/v12r00_cn/...`.

## Role

Translates incoming protobuf-net requests into the version-agnostic
`Common*` DTOs in `Application/Dtos/`, sends them through `IMediator`,
and maps the response back to the version-specific protobuf wire type.
This is one of two parallel game-protocol adapters;
`Adapters.GameProtocol.WwR08` is the sibling for the 39.06 WW client.

## Dependencies

- Inbound: `Host` only.
- Outbound: `Adapters.GameProtocol.Shared`, `Application`,
  `Contracts.AdminApi`.
- Notable packages: `protobuf-net`, `Riok.Mapperly`, `Swan.Core`.

## Key folders

- `Controllers/` — per-endpoint controllers, inheriting
  `BaseProtocolController` (from `Adapters.GameProtocol.Shared`). The
  controllers strip the 32-byte header (when present), gzip-decompress,
  deserialize the protobuf request, send a Mapperly-converted Common*
  DTO through Mediator, then map the response back.
- `Wire/` — protobuf-net `[ProtoContract]` types for the CnR00 wire
  format (request and response).
- `Mappers/` — `[Mapper]` partial classes generating the bidirectional
  CnR00 ↔ Common* conversion.
- `DependencyInjection.cs` — `AddGameProtocolCnR00()` extension that
  registers the controllers and any version-specific services.

## When to add code here

- A new endpoint that exists in the CnR00 client. Add the controller
  here, the wire types under `Wire/`, the mapper under `Mappers/`, and
  the corresponding handler in `Application/Handlers/` operating on a
  Common* DTO. Mirror in `Adapters.GameProtocol.WwR08` if the WW
  client also speaks the endpoint.

Do **not** add: business logic (→ `Application/Handlers/`), shared
plumbing (→ `Adapters.GameProtocol.Shared`), or DB access from
controllers (handlers do that).

See the root `CLAUDE.md` for the full hexagonal layout.
