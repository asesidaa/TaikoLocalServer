# Adapters.AllnetMucha

Inbound adapter for the Allnet/Mucha lifecycle: AmAuth, AmUpdater, Garmc,
and MuchaActivation. Handles the bootstrap chatter the cabinet does
before the game protocol kicks in.

## Role

Implements the proprietary Allnet/Mucha endpoints (`/sys/servlet/PowerOn`
and friends) that authenticate the cabinet and hand it the game-server
endpoint. Owns the `AllNetRequestMiddleware`, which zlib-decompresses
base64-encoded form bodies before the controller sees them.

## Dependencies

- Inbound: `Host` only.
- Outbound: `Adapters.GameProtocol.Shared` (for shared compression and
  controller scaffolding), `Application`, `Infrastructure`.
- Notable packages: `SharpZipLib`.

## Key folders

- `Controllers/` — AmAuth / AmUpdater / Garmc / MuchaActivation
  controllers.
- `Middleware/` — `AllNetRequestMiddleware` (only wired conditionally in
  `Program.cs`; see `Host/Program.cs`).
- `Wire/` — Mucha and Garm wire types (request/response payloads).
- `Common/` — shared helpers used across this adapter only.
- `DependencyInjection.cs` — `AddAllnetMucha()` /
  `UseAllnetMucha(IApplicationBuilder)` extensions that register the
  controllers + middleware.

## When to add code here

- A new Allnet/Mucha lifecycle endpoint or wire type.
- Changes to `AllNetRequestMiddleware` (zlib decoding, header
  handling).

Do **not** add: game protocol endpoints (→ `Adapters.GameProtocol.*`),
admin API endpoints (→ `Adapters.AdminApi`), or business logic (→
`Application/Handlers/`).

See the root `CLAUDE.md` for the full hexagonal layout.
