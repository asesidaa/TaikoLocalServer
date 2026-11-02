# Adapters.GameProtocol.Shared

Common scaffolding shared by the per-version game-protocol adapters
(`Adapters.GameProtocol.WwR08`, `Adapters.GameProtocol.CnR00`).

## Role

Hosts the bits that are identical across game versions: the
`BaseProtocolController` controllers inherit from, gzip/header-strip
helpers, protobuf-net configuration, and any other game-protocol-wide
plumbing.

## Dependencies

- Inbound: `Adapters.AllnetMucha`, `Adapters.GameProtocol.WwR08`,
  `Adapters.GameProtocol.CnR00`.
- Outbound: `Application` only.
- Notable packages: `protobuf-net`, `protobuf-net.AspNetCore`,
  `SharpZipLib`.

## Key folders

- `Controllers/` — `BaseProtocolController` (shared base for both
  per-version adapters' controllers).
- `Compression/` — gzip / 32-byte-header strip / decompression helpers.
- `Cabinets/` — `CabinetFilter` (records each cabinet, refuses blocked or
  unapproved ones) and the heartbeat's server-state value.

## When to add code here

- A helper or base class that genuinely applies to **both** game-protocol
  versions identically.
- Shared protobuf-net configuration.

Do **not** add: per-version wire types or mappers (→
`Adapters.GameProtocol.WwR08/Wire/`,
`Adapters.GameProtocol.CnR00/Wire/`), per-version controllers (→ same),
admin-API code (→ `Adapters.AdminApi`).

See the root `CLAUDE.md` for the full hexagonal layout.

## Eras

Use this project only for behavior that genuinely applies across protocol adapters. Per-era wire types, mappers, controllers, and runtime semantics belong in their own adapter or Application era partials.

AC15 eras share `/v01r00/chassis/*` startup/version behavior where current evidence supports it, while game endpoints remain era-owned: Green `/v11r01/chassis/*`, Blue `/v10r03/chassis/*`, Yellow `/v09r02/chassis/*`, Red `/v08r01/chassis/*`, White final `/v07r03/chassis/*`, White legacy compatibility `/v07r00/chassis/*`, and Murasaki `/v06r00/chassis/*`.
