# Phase 29: Catalog And AC15 Profile Binding - Context

**Gathered:** 2026-06-21
**Status:** Ready for planning

## Boundary

Bind Murasaki catalog/profile facts through Murasaki-owned infrastructure. Use `ST6100-1` as the active root from local evidence. Keep raw operator data under `Host/wwwroot/data/murasaki/data` and committed server-authored sidecars under `Host/wwwroot/data/murasaki`.

## Decisions

- Required catalog inputs are `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `fumen/tuning.bin`.
- Murasaki catalog uses shared AC15 loaders where formats match.
- Profile limits reuse current AC15 limits only where Murasaki evidence and existing protocol shape support them, including favorite cap 10.
- Missing implemented-feature sidecars are committed as empty conservative defaults.
