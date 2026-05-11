# Green Real Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Spec:** [`../../specs/2026-05-12-green-real-support-design.md`](../../specs/2026-05-12-green-real-support-design.md)

**Goal:** Turn the phase-1 Green scaffold into practical AC15 Green support: parse Green datatables, unlock test song sets, persist core uploads, return real score/crown/profile/ghost data, and provide deterministic fake Dan and fake score/crown data for cabinet verification.

**Architecture:** Keep the existing per-era adapter, catalog, handler, and EF table split. Add focused Green-only helpers for byte packing, zlib crown output, XML datatable parsing, default save data, and seeded test data. Controllers should stay thin and delegate to Mediator handlers plus Green mappers.

**Tech Stack:** .NET 10, ASP.NET Core 10, EF Core 10 + SQLite, protobuf-net, martinothamar.Mediator, Riok.Mapperly, xUnit test project added in this plan.

**Branch:** `feat/green-version-support`. Do not push.

---

## File Ordering And Dependencies

```
01 -> 02 -> 03 -> 04 -> 05 -> 08
            |      |      |
            |      |      +-> 06
            |      +--------> 07
            +----------------> 07
```

| # | File | Surface | Blocked by |
|---|---|---|---|
| 01 | [`01-test-harness-and-green-protocol-helpers.md`](01-test-harness-and-green-protocol-helpers.md) | Create tests project; add Green bitset, dan packing, crown packing, and zlib helper tests and implementation | none |
| 02 | [`02-green-catalog-parsers.md`](02-green-catalog-parsers.md) | Parse `musicinfo.xml` and `musicmedleyinfo.xml`; fail startup when required Green files are missing; add catalog tests | 01 |
| 03 | [`03-green-default-save-and-seeding.md`](03-green-default-save-and-seeding.md) | Create default Green save data, seeded fake scores/crowns, first-Dan grant helper, and tests | 01, 02 |
| 04 | [`04-green-identity-userdata-initialdata.md`](04-green-identity-userdata-initialdata.md) | Replace hard-coded `baidcheck`, `mydonentry`, `userdata`, and `initialdatacheck` with real Mediator/catalog-backed logic | 03 |
| 05 | [`05-green-playresult-selfbest-crowns.md`](05-green-playresult-selfbest-crowns.md) | Deserialize Green play result, persist plays/bests/options/unlocks, return `selfbest` and zlib `crownsdata` | 03, 04 |
| 06 | [`06-green-ghost-reward-shop.md`](06-green-ghost-reward-shop.md) | Persist and read back ghost data; implement reward, card check, item purchase, and deterministic catalog-empty endpoints | 04, 05 |
| 07 | [`07-green-taikojuku-and-fake-dan.md`](07-green-taikojuku-and-fake-dan.md) | Return Taikojuku packs from medley data or deterministic fallback; grant first fake Dan on second login | 02, 03, 04 |
| 08 | [`08-build-smoke-and-docs.md`](08-build-smoke-and-docs.md) | Full build/test pass, endpoint smoke notes, and operator/cabinet smoke checklist | 05, 06, 07 |

## Verification Cadence

After each task file:

1. `dotnet test`
2. `dotnet build`
3. Commit the task's files only.

If a task intentionally adds failing tests before implementation, run the exact focused test command in that step and verify the expected failure before continuing.

## Test Project Convention

This repository currently has no test project. Task 01 creates `Tests/Tests.csproj` and adds it to `TaikoLocalServer.slnx`. All new tests live under `Tests/` and use xUnit.

## Scope Notes

- Do not add admin API or WebUI Green surfaces.
- Do not add Green Dan score tables.
- Do not archive every raw protobuf upload.
- Do not change Nijiiro semantics except through shared helpers that preserve existing behavior.
- Treat Green `option_flg` as real option state, preserving full bytes and decoding only known prefixes.
- Keep byte arrays bounded in logs.
