# AC15 Playresult Capability Input Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace AC15 playresult mapping into `CommonPlayResultData` with capability-shaped AC15 input records while preserving current Blue, Green, Yellow, and Red behavior.

**Architecture:** Controllers continue to deserialize wire DTOs, map, send Mediator commands, and map responses. AC15 controllers send a dedicated `UpdateAc15PlayResultCommand` carrying `Ac15PlayResultEnvelope`; Nijiiro stays on `UpdatePlayResultCommand` and `CommonPlayResultData`. Era handlers remain explicit workflow roots, and shared `Application/Ac15` writers consume capability records without choosing an era internally.

**Tech Stack:** C# 13, .NET 10, ASP.NET Core controllers, Mediator, EF Core SQLite, Mapperly 4.3.1, protobuf-net, xUnit.

---

## Spec

Approved design spec: [`../../specs/2026-06-13-ac15-playresult-capability-input-design.md`](../../specs/2026-06-13-ac15-playresult-capability-input-design.md)

## Scope Split

The redesign touches adapter mappers, controller command shape, shared AC15 writers, special-mode helpers, and tests. This directory splits the work into checkpoints that can be reviewed and committed independently.

## File Structure

Plan files:

- `01-boundary-and-command/PLAN.md` - adds AC15 capability records and dedicated AC15 command boundary.
- `02-adapter-mappers/PLAN.md` - migrates Blue, Green, Yellow, and Red playresult mappers/controllers to the AC15 envelope, with a temporary internal bridge only to keep behavior compiling during the transition.
- `03-shared-writers/PLAN.md` - migrates normal stage filtering, profile mutation, normal play writing, Dani writing, and counter updates from `CommonPlayResultData` to capability records.
- `04-special-modes/PLAN.md` - migrates Blue Tokkun, Yellow Tokkun, Red Tokkun, Blue battle, Green ghost, and Red ChallengeCompe fact paths to capability records.
- `05-cleanup-and-verification/PLAN.md` - removes the temporary bridge, removes AC15 shared-writer dependency on `CommonPlayResultData`, checks Mapperly warning cleanup, and runs final verification.

Primary implementation areas:

- `Application/Dtos/Ac15/` - new capability input records and temporary bridge removed by checkpoint 5.
- `Application/Handlers/UpdatePlayResultCommand*.cs` - command split plus AC15 era handler signatures and branch ordering.
- `Application/Ac15/` - shared writer signatures and record consumption.
- `Application/Common/BlueBattleStateExtensions.cs` - Blue battle persistence helper input migration.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`
- `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
- `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`
- `Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs`
- `Adapters.GameProtocol.*.Controllers/PlayResultController.cs` for Blue, Green, Yellow, and Red.
- `Tests/Ac15/`, `Tests/Blue/`, `Tests/Green/`, `Tests/Yellow/`, and `Tests/Red/` - behavior and mapper boundary coverage.

## Execution Order

1. [Boundary and command](01-boundary-and-command/PLAN.md)
2. [Adapter mappers](02-adapter-mappers/PLAN.md)
3. [Shared writers](03-shared-writers/PLAN.md)
4. [Special modes](04-special-modes/PLAN.md)
5. [Cleanup and verification](05-cleanup-and-verification/PLAN.md)

## Execution Rules

- Execute checkpoints in order.
- Use one commit per checkpoint.
- Do not edit generated `Wire/` files or `proto/` inputs.
- Do not change Nijiiro mapper, controller, handler, or DTO behavior except where compile errors require imports.
- Do not add shared gameplay EF tables, repository-shaped persistence abstractions, or `GameEra` switches inside shared AC15 writers.
- Keep Mapperly strict target mapping enabled.
- Keep branch ordering visible in handlers: Blue Tokkun before battle before normal; Yellow and Red Tokkun before normal; Green ghost enrichment only in Green normal handling.
- Keep temporary compatibility code internal, untested as a contract, and delete it in checkpoint 5.
- Use behavior tests, SQLite persistence assertions, no-cross-era checks, and no-cross-mode checks. Do not add route-inventory, source-text, controller-attribute, generated-wire, or project-file tests.
- If `Host/bin/Debug/net10.0` is locked, verify Host with `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`.

## Final Verification

Run after checkpoint 5:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Yellow"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Red"
dotnet test Tests/Tests.csproj
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected final result: every command exits `0`, and the Host build contains no Mapperly `RMG012` warnings from Blue, Green, Yellow, or Red AC15 playresult mappers.

## Self-Review

- Spec coverage: checkpoint 1 covers capability records and command shape; checkpoint 2 covers adapter mappers, controller command use, and Mapperly target correction; checkpoint 3 covers shared writer migration; checkpoint 4 covers special-mode capability paths and branch order; checkpoint 5 covers bridge removal, Common DTO cleanup boundary, warning cleanup, and verification.
- Red-flag wording scan: this plan directory avoids incomplete-action markers and vague future-work language.
- Type consistency: all checkpoint plans use `Ac15PlayResultEnvelope`, `Ac15StageResult`, `Ac15ProfileMutationFacts`, `Ac15TokkunPlayResult`, `Ac15BlueBattlePlayResult`, `Ac15GreenGhostPlayResult`, `Ac15RedChallengeCompeFacts`, and `UpdateAc15PlayResultCommand` consistently.
