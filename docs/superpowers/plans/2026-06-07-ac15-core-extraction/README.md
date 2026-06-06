# AC15 Core Extraction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract a shared AC15 gameplay core for Blue, Green, and later Yellow/Red support while preserving separate era routes, wire DTOs, hooks, and persistence tables.

**Architecture:** Add `Application/Ac15` as a pure application core behind existing Mediator handlers and adapter controllers. Blue and Green continue to own route prefixes, generated `Wire/` classes, and EF entities; the AC15 core receives canonical records, explicit era profiles, and typed era adapters.

**Tech Stack:** C# 13, .NET 10, ASP.NET Core 10 controller adapters, Mediator, EF Core with SQLite, protobuf-net wire DTOs, xUnit.

---

## Spec

Approved design spec: [`../../specs/2026-06-07-ac15-core-extraction-design.md`](../../specs/2026-06-07-ac15-core-extraction-design.md)

## Scope Split

The design spans profiles, pure byte algorithms, catalog readback, item-shop workflows, userdata composition, and normal play persistence. This directory splits the work into ordered stage plans so each stage produces working, testable software on its own.

## File Structure

Create:

- `Application/Ac15/` - shared AC15 profiles, limits, wire placement, hooks, canonical records, core services, and typed persistence/catalog adapter interfaces.
- `Tests/Ac15/` - shared AC15 unit tests and profile contract tests.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/*.md` - executable stage plans.

Modify in later stages:

- `Application/Common/BlueProtocolBytes.cs` and `Application/Common/GreenProtocolBytes.cs` - delegate duplicated AC15 byte helpers to `Ac15ProtocolBytes`.
- `Application/Common/BlueCrownResponseBuilder.cs` and `Application/Common/GreenCrownResponseBuilder.cs` - delegate crown packing to `Ac15CrownService`.
- `Application/Handlers/GetSelfBestQuery.Blue.cs` and `Application/Handlers/GetSelfBestQuery.Green.cs` - use `Ac15SelfBestService`.
- `Application/Handlers/GetTaikojukuQuery.Blue.cs` and `Application/Handlers/GetTaikojukuQuery.Green.cs` - use `Ac15TaikojukuService`.
- `Application/Handlers/GetInitialDataQuery.Blue.cs`, `Application/Handlers/GetInitialDataQuery.Green.cs`, `GetFolderQuery.*`, `GetTelopQuery.*`, and item-shop handlers - route shared logic through AC15 services while keeping era extras local.
- `Application/Handlers/UserDataQuery.Blue.cs`, `Application/Handlers/UserDataQuery.Green.cs`, `UpdatePlayResultCommand.Blue.cs`, and `UpdatePlayResultCommand.Green.cs` - last-stage migrations through typed persistence adapters and era hooks.
- Blue/Green controller mapper files only when a controller currently owns duplicated business logic, such as `CrownsDataController`.

## Execution Order

1. [Core contracts and profiles](01-core-contracts-and-profiles.md)
2. [Pure utilities, crowns, self-best, and Taikojuku](02-pure-utilities-and-low-risk-readback.md)
3. [Catalog readback and initial data](03-catalog-readback-and-initial-data.md)
4. [Item shop core](04-item-shop-core.md)
5. [Userdata core and typed era adapters](05-userdata-core-and-era-adapters.md)
6. [Normal play core and special-mode hooks](06-normal-play-core-and-hooks.md)

## Execution Rules

- Execute stages in order.
- Each stage ends with a commit.
- Stage only paths listed by the current stage; this repo often has unrelated local state.
- Keep Blue and Green EF tables separate.
- Keep generated `Adapters.GameProtocol.*.Wire` classes out of `Application/Ac15`.
- Keep controllers responsible for route ownership, transport, logging, gzip, and wire mapping.
- Keep Blue battle, Blue Tokkun, and Green AI battle as explicit era hook behavior.
- Use `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` when a running server locks `Host/bin/Debug/net10.0`.

## Final Verification

Run these after the final stage:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected final result: each command exits `0`.

## Self-Review

- Spec coverage: profiles and capabilities are covered in stage 1; pure algorithms and low-risk modules in stage 2; catalog readback and wire placement in stage 3; item-shop behavior in stage 4; typed era persistence boundaries in stages 5 and 6; special modes in stage 6.
- Non-goals honored: no shared AC15 EF table, no generated shared wire assembly, no Yellow/Red implementation, no route inference from proto similarity.
- Type consistency: all later stages depend on `Ac15EraProfile`, `Ac15FeatureSet`, `Ac15ProtocolLimits`, `Ac15WirePlacement`, `IAc15EraHooks`, and canonical row records introduced in stage 1 or stage 2.

