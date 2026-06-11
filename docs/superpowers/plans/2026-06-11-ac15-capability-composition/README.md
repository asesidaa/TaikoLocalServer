# AC15 Capability Composition Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reshape the Phase 16.2 AC15 shared core into switch-free capability modules while preserving Blue, Green, and Yellow routes, wire DTOs, and EF tables.

**Architecture:** Era handlers remain the composition roots and bind concrete `DbSet`s, save rows, Mapperly delegates, and explicit policies. Shared `Application/Ac15` modules receive those capabilities and never choose Blue, Green, or Yellow tables internally.

**Tech Stack:** C# 13, .NET 10, ASP.NET Core controllers, Mediator handlers, EF Core SQLite, Mapperly, protobuf-net, xUnit.

---

## Spec

Approved design spec: [`../../specs/2026-06-11-ac15-capability-composition-design.md`](../../specs/2026-06-11-ac15-capability-composition-design.md)

Amended earlier design: [`../../specs/2026-06-07-ac15-core-extraction-design.md`](../../specs/2026-06-07-ac15-core-extraction-design.md)

## Scope Split

The design spans game playresult runtime behavior, Dani save/readback, item-shop purchase, AdminApi settings, catalog projection, and review closeout. This directory splits the work into stage folders so each stage is testable on its own and can be executed with a separate subagent.

## File Structure

Plan files:

- `01-normal-play-blockers/PLAN.md` - fixes the two review blockers: shared timestamp parsing and Green log-skip-success stage filtering.
- `02-normal-play-capabilities/PLAN.md` - replaces `Ac15NormalPlayService` table switches with passed-in save/table capabilities.
- `03-dani-capabilities/PLAN.md` - converts Dani save/readback to generic table bundles passed by era handlers.
- `04-item-shop-capabilities/PLAN.md` - converts item-shop purchase to generic table bundles plus explicit unlock policies.
- `05-adminapi-user-settings/PLAN.md` - moves AC15 user settings behavior from AdminApi controller partials into an Application service.
- `06-catalog-closeout/PLAN.md` - simplifies catalog projection and updates the Phase 16.2 review artifact.

Primary implementation areas:

- `Application/Ac15/` - switch-free capability modules, policies, table bundles, and access records.
- `Domain/Entities/` - narrow row-shape and save capability interfaces only.
- `Application/Handlers/UpdatePlayResultCommand.*.cs` - era orchestration roots for normal play, special-mode ordering, and concrete capability binding.
- `Application/Handlers/GetDanScoreQuery.*.cs` - era Dani readback table binding.
- `Application/Handlers/ItemPurchaseCommand.*.cs` - era item-shop table and unlock policy binding.
- `Adapters.AdminApi/Controllers/UserSettingsController.*.cs` - transport-only controller partials after AC15 settings service extraction.
- `Tests/Ac15/`, `Tests/Blue/`, `Tests/Green/`, `Tests/Yellow/`, and `Tests/Adapters.AdminApi/` - behavior coverage.
- `.planning/phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup/16.2-REVIEW.md` - review status update after implementation verification.

## Execution Order

1. [Normal play blockers](01-normal-play-blockers/PLAN.md)
2. [Normal play capabilities](02-normal-play-capabilities/PLAN.md)
3. [Dani capabilities](03-dani-capabilities/PLAN.md)
4. [Item shop capabilities](04-item-shop-capabilities/PLAN.md)
5. [AdminApi user settings](05-adminapi-user-settings/PLAN.md)
6. [Catalog projection and closeout](06-catalog-closeout/PLAN.md)

## Execution Rules

- Execute stages in order.
- Use one commit per stage folder.
- Do not edit generated `Adapters.GameProtocol.*\Wire` files.
- Do not add Red routes, Red wire, or unsupported feature stubs.
- Do not add shared gameplay EF tables or discriminator tables.
- Do not add repository-shaped persistence adapters.
- Keep direct `ITaikoDbContext` and concrete `DbSet` binding visible in era handlers.
- Keep Blue battle, Blue Tokkun, Yellow Tokkun, Yellow WaiWai logging, and Green ghost/AI behavior explicit.
- Use behavior tests, persistence assertions, and protocol readback checks. Do not add source-shape, route-inventory, generated-protobuf, DI-shape, or migration-body tests.
- If `Host/bin/Debug/net10.0` is locked, use `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`.

## Final Verification

Run after stage 6:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Yellow"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserSettings"
dotnet test Tests/Tests.csproj
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected final result: every command exits `0`.

## Self-Review

- Spec coverage: stage 1 covers review blockers and stage filtering; stage 2 covers normal play shared modules and save mutation; stage 3 covers Dani save/readback; stage 4 covers item-shop purchase; stage 5 covers AdminApi settings; stage 6 covers catalog projection, final verification, and review artifact update.
- Non-goals honored: no merged EF table, no custom source generator, no unsupported route stubs, no broad persistence repositories, no source-shape verification strategy.
- Type consistency: all stage plans use `Ac15NormalStagePolicy`, `Ac15NormalPlayTables<TPlay,TBest,TFavorite,TRecent>`, `Ac15DaniTables<TScore,TStage>`, `Ac15ItemShopPurchaseTables<TSeason,TItem>`, and `Ac15UserSettingsAccess<TSave>` with the same names throughout.
