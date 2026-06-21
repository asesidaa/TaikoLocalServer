# Adapters.GameProtocol.Shared

Shared game-protocol scaffolding used by the per-era adapters.

## Role

This project contains controller base types, protobuf helpers, compression helpers, header handling, and shared startup/version controllers used by cabinet protocol adapters.

## Key Folders

- `Controllers/` - `BaseProtocolController` and shared AC15 startup/version surfaces.
- `Compression/` - gzip, decompression, and header-strip helpers.

## Adapter Boundary

Use this project only for behavior that genuinely applies across protocol adapters. Per-era wire types, mappers, controllers, and runtime semantics belong in their own adapter or Application era partials.

AC15 eras share `/v01r00/chassis/*` startup/version behavior where current evidence supports it, while game endpoints remain era-owned: Green `/v11r01/chassis/*`, Blue `/v10r03/chassis/*`, Yellow `/v09r02/chassis/*`, Red `/v08r01/chassis/*`, White final `/v07r03/chassis/*`, and White legacy compatibility `/v07r00/chassis/*`.

## When To Add Code Here

- Add shared controller plumbing.
- Add shared compression or protobuf request handling.
- Add behavior that is identical across the relevant cabinet adapters.

Do not add per-era wire models, mappers, or game endpoint behavior here.
