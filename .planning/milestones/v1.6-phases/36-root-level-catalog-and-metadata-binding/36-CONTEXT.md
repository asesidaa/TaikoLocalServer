# Phase 36: Root-Level Catalog and Metadata Binding - Context

**Gathered:** 2026-06-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Load KIMIDORI root-level catalog inputs and bind only proto-and-route-backed metadata routes before any runtime state depends on KIMIDORI catalog behavior.

</domain>

<decisions>
## Implementation Decisions

### Data Layout
- KIMIDORI raw operator data lives under `Host/wwwroot/data/kimidori/data`.
- Required root-level inputs are `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- Resolve paths through existing settings/path abstractions and KIMIDORI catalog helpers, not hardcoded handler filesystem access.

### Metadata Surface
- Implement catalog-backed or conservative no-state metadata responses for proven KIMIDORI request families.
- KIMIDORI favorite limit defaults to five unless local evidence proves a different cap.
- Commit intentional sidecar JSON files for implemented KIMIDORI server-authored data, even when empty.

### Unsupported Surfaces
- Do not copy Taikojuku sidecar loading into KIMIDORI.
- Do not infer later-era shop, challenge, Tokkun, Banacoin, battle, or Don Challenge data behavior.

### the agent's Discretion
- Reuse AC15 catalog snapshot and parser infrastructure where shape-compatible, adding KIMIDORI-specific adapters only where the root-level layout requires it.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Infrastructure/GameDataCatalog/Murasaki` is the nearest full catalog loader.
- `Application/Ac15/Ac15CatalogSnapshotFactory` centralizes shared metadata snapshots.
- Existing sidecar loaders cover telops, movies, event folders, recommendations, and Taikojuku where supported.

### Established Patterns
- Era catalogs implement era-specific catalog interfaces and are reached through `IGameDataCatalog.For(GameEra)`.
- Host output copy rules must be explicit for server-authored JSON files.

### Integration Points
- Add `IKimidoriCatalog`, KIMIDORI game-data paths, required files, catalog loader, sidecars, and Host copy rules.

</code_context>

<specifics>
## Specific Ideas

KIMIDORI is Murasaki-like for many metadata routes but uses root-level catalog files and excludes Taikojuku.

</specifics>

<deferred>
## Deferred Ideas

Later KIMIDORI update roots or Taikojuku practice-folder data belong to future version-specific work.

</deferred>
