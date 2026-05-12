# Green (AC15) version support — implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers-extended-cc:subagent-driven-development (recommended) or superpowers-extended-cc:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Spec:** [`../../specs/2026-05-12-green-version-support-design.md`](../../specs/2026-05-12-green-version-support-design.md)

**Goal:** Land the structural skeleton for Green (AC15) version support — per-era DB schema, partial-file refactors, per-era catalog multiplex, new `Adapters.GameProtocol.Green` project with stub controllers — so a Green cabinet can complete its protocol lifecycle against `TaikoLocalServer` returning success-shaped empty responses, without disturbing existing Nijiiro behavior.

**Architecture:** Hexagonal layout already in place. Add `GameEra` discriminator at the Domain layer, plumb it through `Common*` DTOs / Mediator requests / catalog interface. Per-era persistence via separate tables in one `taiko.db3`. Per-era partial-file split (`*.Shared.cs`, `*.Nijiiro.cs`, `*.Green.cs`) for any type that grows by era. Stub-first posture: every Green handler / mapper / catalog loader ships as a stub.

**Tech Stack:** .NET 10, ASP.NET Core 10, EF Core 10 + SQLite, protobuf-net, martinothamar.Mediator, Riok.Mapperly.

**Branch:** `feat/green-version-support` (already checked out). Do not push. The user runs the server and does smoke testing.

---

## File ordering & dependencies

```
01 ─┬─→ 02 ─→ 03 ─┬─→ 04 ─┬─→ 05 ─→ 06 ─→ 08
    │            │       │
    └────────────┴───────┘
                 │
                 └─→ 07 (independent operator-doc work)
```

| # | File | Surface | Blocked by |
|---|---|---|---|
| 01 | [`01-prep-game-era-and-dbcontext-partials.md`](01-prep-game-era-and-dbcontext-partials.md) | `GameEra` enum, pure refactor of `TaikoDbContext` + `ITaikoDbContext` into partial files (no schema change yet) | — |
| 02 | [`02-schema-entities-and-migration.md`](02-schema-entities-and-migration.md) | New entity types (renamed Nijiiro + new Green), wire them into the DbContext partials, slim `UserDatum`, generate the `AddGreenEraSupport` EF migration | 01 |
| 03 | [`03-common-dto-and-handlers.md`](03-common-dto-and-handlers.md) | Split `Common*` DTOs into partial files. Add `GameEra Era` to every Mediator request. Split handlers into per-era partials. Green stubs return success defaults. Refactor existing Nijiiro callers to thread `Era`. | 02 |
| 04 | [`04-catalog-multiplex.md`](04-catalog-multiplex.md) | Introduce `IEraGameDataCatalog` and the `IGameDataCatalog.For(GameEra)` multiplex. Move existing catalog into a `NijiiroEraGameDataCatalog` impl. Add a stub `GreenEraGameDataCatalog`. Update consumers. | 03 |
| 05 | [`05-green-adapter-scaffold.md`](05-green-adapter-scaffold.md) | New `Adapters.GameProtocol.Green` project with wire types compiled from `proto/green/green.proto`, stub Mapperly mappers, stub controllers for all 25 `/v11r01/chassis/*.php` routes. | 04 |
| 06 | [`06-host-wiring-and-server-settings.md`](06-host-wiring-and-server-settings.md) | Extend `ServerSettings.Eras`. Conditional adapter registration in `Program.cs`. ApplicationPart filter removes disabled-era adapter assemblies. Conditional catalog construction. | 05 |
| 07 | [`07-on-disk-file-move.md`](07-on-disk-file-move.md) | Update `Host/README.md` documenting the per-era `wwwroot/data/<era>/` layout. Move existing committed-to-Host data files into `wwwroot/data/nijiiro/` / `wwwroot/data/shared/`. | 02 (parallel-safe with 03–06) |
| 08 | [`08-smoke-checklist.md`](08-smoke-checklist.md) | Final compile check, EF migration verification against the existing dev DB, server startup smoke (Nijiiro+Green enabled), Green stub endpoint responses. The **user** runs these — Claude documents the steps. | 06, 07 |

## Iteration scope (recap)

**In scope (this iteration, stub-first):**
- Full file/project structure (every file mentioned in the spec exists)
- Real EF migration (renames + creates per-era tables, slims `UserDatum`)
- Real `GameEra` enum, real per-era partial-file refactors, real DI wiring (era opt-in honored)
- Stub handler bodies for everything Green (return success-shaped empty responses, no DB read/write)
- Stub Mapperly mappers (signatures only with `// TODO iter 2` for fields that need wire ↔ Common mapping)
- Stub catalog loaders (`InitializeAsync` is a no-op; entries are empty)

**Out of scope (explicit follow-ups, named in spec §11):**
- Real Green handler logic
- Real Mapperly bodies for Green
- Real catalog loaders for Green (`musicinfo.bin` parser, taikojuku, itemshop, etc.)
- TaikoWebUI / admin API Green surfaces
- `LocalSaveModScoreMigrator` Green support

## How to execute this plan

This plan was generated with the `writing-plans` skill. The next step is choosing how to execute:

- **Subagent-Driven (this session)** — dispatch a fresh subagent per task, review between tasks, fast iteration. Recommended for cohesive single-session work.
- **Parallel Session (separate)** — open a new session in the worktree with `executing-plans`, batched execution with checkpoints. Recommended when the work spans multiple days.

Task persistence lives in [`tasks.json`](tasks.json) at the directory root. Either execution mode reads from there.

## Verification cadence

After each plan file's tasks are complete:
1. `dotnet build` (full solution) — must succeed with zero new warnings.
2. If the file changes the DB schema (`02`) or migrations (`02`): inspect `dotnet ef migrations script <previous> <new> --project Infrastructure --startup-project Host` output for the expected ops.
3. Commit per-task as instructed. The branch stays unpushed until `08-smoke-checklist.md` is fully signed off by the user.

**Before Task 08 (smoke testing):** if you have a local working `wwwroot/data/datatable/` folder with operator-supplied binaries (`musicinfo.bin`, `music_order.bin`, etc.) that are not in git, move them to `wwwroot/data/nijiiro/datatable/` before starting the server. The Nijiiro catalog loader now looks for them at the per-era path.
