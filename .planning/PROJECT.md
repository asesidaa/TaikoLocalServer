# TaikoLocalServer Blue Support

## What This Is

TaikoLocalServer is a local ASP.NET Core server for Taiko no Tatsujin cabinet protocols, local SQLite persistence, era-specific game data catalogs, and a Blazor WebAssembly admin UI. This project continues the existing Blue-era support effort from the Superpowers roadmap in `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`, starting after completed stages A0-A5 and carrying the work through full Blue support.

Full Blue support means normal Blue cabinet flows are completed and hardened first, then Blue battle mode is specified and implemented from concrete evidence. Blue must remain a first-class era with Blue-owned persistence, handlers, mappers, catalogs, tests, and AdminApi/WebUI routing rather than being treated as a Green flag.

## Core Value

A Blue cabinet can use TaikoLocalServer for normal and battle play with repeatable cabinet/RPCS3 evidence, without corrupting or conflating Green, Nijiiro, or shared identity state.

## Requirements

### Validated

- [x] Existing multi-era TaikoLocalServer architecture: ASP.NET Core host, local SQLite persistence, game protocol adapters, era-aware application handlers, filesystem game-data catalogs, and Blazor WebAssembly admin UI.
- [x] Blue A0 evidence/bootstrap: Blue route prefix, direct protobuf transport assumptions, shared startup/verup route ownership, scoped missing-content-type fallback, and local Blue data layout are documented.
- [x] Blue A1 era foundation and adapter skeleton: Blue is a first-class enableable era with adapter project, generated wire types, route ownership, settings, dependency injection, and safe stub behavior.
- [x] Blue A2 catalog and data layout: Blue catalog interfaces, runtime data paths, AC15 loader reuse, startup validation, settings, and operator data documentation are established.
- [x] Blue A3 identity/profile/userdata: Blue card registration, login, default save state, initial data, and stable profile/userdata readback are implemented through Blue-owned state.
- [x] Blue A4 normal enso play result, self-best, crowns, and rewards: Blue normal song results persist and read back through Blue-specific handlers, mappers, byte helpers, and save state.
- [x] Blue A5 Dani Dojo: Blue taikojuku catalog responses, Dan result persistence, Dan readback, AdminApi support, and WebUI Dani behavior are implemented without reusing Green Dan state.

### Active

- [ ] Blue A6 item shop and unlocking: support Blue shop seasons, active shop selection, season-scoped medal state, purchases, reward execution, and configured item unlocks using Blue data and Blue save state.
- [ ] Blue A7 basic AdminApi and WebUI parity: expose Blue profile, score history, favorites, Dani, customization, item-shop-relevant surfaces, and safe edit/readback behavior without writing Green state.
- [ ] Blue A8 normal-mode cabinet smoke and hardening: produce repeatable cabinet/RPCS3 evidence for boot, new card registration, known card login, song list, normal play, playresult save, self-best/crown readback, Dani, item shop purchase/reward unlock, and WebUI readback.
- [ ] Blue battle evidence and design: define Track B from proto, local logs, IDA/client evidence, cabinet/RPCS3 traces, and explicit field-width/default-state findings before implementing runtime battle behavior.
- [ ] Blue battle mode implementation: support battle userdata, battle entry state, battle playresult data, progression/unlocks, rewards, and readback only after the strict evidence gate is satisfied.
- [ ] Full Blue verification: normal and battle flows have repeatable cabinet/RPCS3 smoke evidence, server-side regression tests, source guardrails, and documented unresolved items.

### Out of Scope

- Tokkun mode behavior for Blue, because Tokkun ended before Blue service and the existing roadmap marks it as non-goal.
- Banacoin balance, payment, error, and info behavior, unless new cabinet evidence proves a non-payment stub is required for Blue progression.
- Yellow or earlier era support.
- Green AI Battle changes while implementing Blue battle mode; Green AI Battle is contrast material, not the Blue design source.
- Runtime scraping of wiki or official pages.
- Treating OCR output as authoritative official data without source/image provenance.
- Sharing Blue, Green, or Nijiiro persistence except for truly shared identity state.

## Context

- The current branch is `feat/green-version-support`; `.planning/codebase/` was generated on 2026-05-28 and describes the brownfield architecture.
- The solution uses .NET 10, ASP.NET Core, EF Core SQLite, protobuf-net, Mediator.SourceGenerator, MudBlazor, and xUnit.
- `Host/Program.cs` composes enabled era adapters and gates controller application parts so disabled-era routes are absent.
- `Domain/Enums/GameEra.cs`, `Application/Handlers/*.Blue.cs`, `Adapters.GameProtocol.Blue/`, `Infrastructure/GameDataCatalog/Blue/`, and `TaikoWebUI/Utilities/WebUiEra.cs` are key Blue support touch points.
- The Superpowers Blue roadmap split the effort into Track A normal support and Track B battle mode. A0-A5 are treated as completed prior work for this GSD project.
- Track A remaining stages are A6 item shop/unlocking, A7 AdminApi/WebUI parity, and A8 normal-mode cabinet smoke/hardening.
- Track B battle mode is part of the full Blue project scope, but starts after Track A is stable enough for normal cabinet smoke testing.
- The evidence hierarchy is repo code, proto files, SQLite state, cabinet/RPCS3 logs, IDA/client evidence, and only then public wiki pages for gameplay scoping.

## Constraints

- **Evidence**: Battle mode must be specified from proto, logs, IDA/client evidence, or cabinet/RPCS3 traces before runtime implementation.
- **Architecture**: Treat Blue as its own era with Blue-owned partial handlers, DTO fields, mappers, persistence, catalog data, tests, and routes.
- **State separation**: Keep Blue, Green, and Nijiiro persistent state separate unless the data is truly shared identity state.
- **Transport safety**: Preserve known Blue direct-protobuf and startup/verup assumptions unless newer client evidence contradicts them.
- **Scope order**: Finish normal Track A A6-A8 before battle implementation; battle evidence/spec work may prepare Track B, but runtime battle behavior should not jump ahead of normal hardening.
- **Verification**: Done requires repeatable cabinet/RPCS3 smoke evidence for normal and battle flows, not only passing server tests.
- **Local data**: Blue runtime data under `Host/wwwroot/data/blue/data` is local/operator-supplied and may be gitignored.
- **Build environment**: If `Host/bin/Debug/net10.0` is locked by a running server, verify Host builds with a temp output path.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Use GSD to finish full Blue support from the existing Superpowers roadmap | The roadmap already captures staged Blue support and evidence references; GSD should continue rather than restart discovery | Pending |
| Treat A0-A5 as validated prior work | The user stated the previous Superpowers work completed through A5 | Pending |
| Include Track B battle mode in current project scope | The user selected full Blue scope, not Track A only | Pending |
| Finish normal Track A before battle runtime implementation | Existing roadmap says battle starts after Track A is stable enough for normal cabinet smoke testing | Pending |
| Use strict battle evidence gates | The user selected strict evidence before battle implementation | Pending |
| Define done as cabinet/RPCS3-proven normal and battle flows | The user selected repeatable cabinet evidence as the full-support done condition | Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `$gsd-transition`):
1. Requirements invalidated? Move to Out of Scope with reason
2. Requirements validated? Move to Validated with phase reference
3. New requirements emerged? Add to Active
4. Decisions to log? Add to Key Decisions
5. "What This Is" still accurate? Update if drifted

**After each milestone** (via `$gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check - still the right priority?
3. Audit Out of Scope - reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-05-28 after initialization*
