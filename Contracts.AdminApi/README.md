# Contracts.AdminApi

DTO-only project shared between the admin REST API and the Blazor
WebAssembly UI. No business logic, no EF Core, no I/O.

## Role

Defines the exact JSON shapes that cross the wire between
`Adapters.AdminApi` (server) and `TaikoWebUI` (client) so both can
serialize/deserialize the same types without duplication. Also hosts the
WebUI-shared `ServerData` JSON shapes (`DanData`, `MusicDetail`,
`IVerupNo`) and the `PlaySettingConverter` used by both sides.

## Dependencies

- Inbound: `Application`, `Infrastructure`, `Adapters.AdminApi`,
  `Adapters.GameProtocol.WwR08`, `Adapters.GameProtocol.CnR00`,
  `TaikoWebUI`.
- Outbound: `Domain` only (for enums).

## Key folders

- `Requests/` — request DTOs accepted by `Adapters.AdminApi/Controllers/`.
- `Responses/` — response DTOs returned by the same.
- `ViewModels/` — UI-facing view models bound by MudBlazor pages.
- `ServerData/` — operator-edited JSON shapes that the WebUI displays
  (`DanData`, `MusicDetail`, `IVerupNo`).
- `Converters/` — `JsonConverter` types used in both server and client
  serialization (`PlaySettingConverter`).

## When to add code here

- New admin-API request, response, or view model that the WebUI binds to.
- A `ServerData` JSON shape that the WebUI **also** needs to read.
- A `JsonConverter` that both server and WebUI must apply identically.

Do **not** add: business logic, EF Core entities (→ `Domain`), server-only
JSON shapes (→ `Application/ServerData/`), or anything depending on
`Application`/`Infrastructure`.

See the root `CLAUDE.md` for the full hexagonal layout.
