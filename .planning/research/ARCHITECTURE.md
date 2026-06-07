# Architecture Research

**Domain:** Yellow AC15 era support in TaikoLocalServer
**Researched:** 2026-06-07
**Confidence:** HIGH for repo architecture, MEDIUM for Yellow route prefix until client evidence is captured

## Recommended Architecture

Yellow should follow the existing era-owned outer adapter model and use the approved AC15 core only behind canonical DTO boundaries.

```text
Yellow controller route + direct protobuf transport
  -> Yellow wire DTO
  -> Yellow mapper
  -> Common/AC15 canonical request
  -> Mediator handler dispatch by GameEra.Yellow
  -> Yellow profile + Yellow persistence/catalog adapters
  -> AC15 core service where behavior is shared
  -> Yellow mapper to generated wire response
```

## Component Responsibilities

| Component | Responsibility | Yellow Notes |
|-----------|----------------|--------------|
| `Adapters.GameProtocol.Yellow` | Own Yellow route prefix, controllers, generated wire DTOs, logging, and protobuf transport. | Route prefix must be proven from client evidence; do not infer from `ST9100-1`. |
| `Application/Handlers/*.Yellow.cs` | Era-specific dispatch and boundary behavior. | Use partial-file pattern already used for Blue/Green. |
| `Application/Ac15` | Shared AC15 profiles, canonical records, and services. | Use for crowns, self-best, Dani, catalog readback, item shop, userdata, and normal play where Yellow matches. |
| `Infrastructure/GameDataCatalog/Yellow` | Yellow catalog implementation. | Wrap `ST9100-1` data and shared AC15 loaders. |
| `Domain` / EF entities | Yellow-owned persistence schema. | Separate Yellow save, score, best, favorite, recent, Dan, shop, medal, and Tokkun history tables. |
| `Adapters.AdminApi` / `TaikoWebUI` | Era-aware admin readback/editing. | Extend current era routing without Green/Blue table reuse. |

## Internal Boundaries

| Boundary | Correct Pattern | Avoid |
|----------|-----------------|-------|
| Wire -> Application | Map generated Yellow DTOs to common/canonical DTOs. | Persist generated wire DTOs or pass them deep into handlers. |
| Application -> Persistence | Typed Yellow persistence adapter. | Reflection-heavy generic EF over shared tables. |
| Catalog -> Handler | `IGameDataCatalog.For(GameEra.Yellow)` and Yellow catalog contracts. | Hardcoded `wwwroot/data/yellow/data` paths in handlers. |
| Shared behavior -> Era quirks | `Ac15EraProfile`, feature set, wire placement, and hooks. | `if BlueLike` or broad inheritance that hides unsupported features. |
| Unsupported features | No route/controller/service call. | Log-and-success stubs just because Blue has a route. |

## Phase Architecture Implications

1. Foundation phase should establish `GameEra.Yellow`, adapter project, generated wire, host settings, and route skeleton tests.
2. Evidence/catalog phase should prove route/version assumptions and load `ST9100-1` catalog data.
3. AC15 core phases should extract only the services needed before Yellow consumes them.
4. Yellow persistence phases should introduce separate EF entities/tables before runtime write paths.
5. Tokkun phase should use a Yellow-specific hook/classifier before normal persistence, matching the Blue no-cross-write lesson without reusing Blue tables.
6. Admin/WebUI and smoke phase should close the loop only after server behavior is observable.

## Anti-Patterns

### Blue Clone Adapter

Copying Blue controllers, handlers, and persistence wholesale would be fast initially, but it would import Blue battle behavior and Blue protocol field placement into Yellow. Start from the Yellow proto route/message list and use AC15 core services only at canonical boundaries.

### Route Prefix Guessing

`ST9100-1` proves a data/config version root, not the HTTP route prefix. A Yellow foundation phase should require route evidence from client config, request logs, IDA, or other local runtime evidence.

### Generic AC15 Database

A single `Ac15UserSave` table with era discriminator would reduce schema count but violate the project state-separation rule and make future protocol audits harder. Use shared canonical records plus Yellow-owned EF tables.

## Sources

- `.planning/PROJECT.md`
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md`
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/README.md`
- `proto/yellow/yellow.proto`
- `Host/wwwroot/data/yellow/data/config/ST9100-1`

---
*Architecture research for: Yellow AC15 Support*
*Researched: 2026-06-07*
