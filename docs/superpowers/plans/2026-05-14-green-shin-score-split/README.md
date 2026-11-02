# Green Shin Score Split Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Persist Green normal and Shin score results separately, fix the captured play-result rejection, and replay the rejected May 14, 2026 save into the local SQLite database.

**Architecture:** Use Green `stage_mode` as the score-mode discriminator: `0` means normal and `1` means Shin. Store the interpreted mode in `IsShin` on Green play and best rows, keep raw `StageMode` for audit, split `selfbest.php` into normal and Shin lookups, and keep crowns normal-only. Fix rejection by preserving Green stage fields, treating no-Dan `play_dan=0` as absent, and validating reward IDs by bitset range instead of requiring them to already be unlocked.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core 10, SQLite, protobuf-net, xUnit, Mapperly-style manual mapper code in the Green adapter.

---

## Worktree And Safety

This repo already has unrelated uncommitted Green audit edits. Do not revert them. Stage only files touched by the task being committed.

Before implementation, use the `superpowers:using-git-worktrees` skill if you want an isolated workspace. If working inline in this workspace, check `git status --short` before every commit.

## File Structure

- `Tests/Green/GreenPlayResultMapperTests.cs` - new mapper tests for `StageMode`, `IsPapamama`, and no-Dan `PlayDan` mapping.
- `Tests/Green/GreenPlayResultHandlerTests.cs` - handler tests for Shin split, reward acceptance/rejection, selfbest split, and crowns ignoring Shin rows.
- `Domain/Entities/SongBestDatumGreen.cs` - add `IsShin` for the best-score key.
- `Domain/Entities/SongPlayDatumGreen.cs` - add `IsShin` while retaining raw `StageMode`.
- `Infrastructure/Persistence/TaikoDbContext.Green.cs` - update Green best primary key.
- `Infrastructure/Persistence/Migrations/*AddGreenShinScoreSplit*` - generated EF migration for the new columns and primary key.
- `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` - generated EF snapshot update.
- `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs` - preserve `StageMode`, `IsPapamama`, and no-Dan `PlayDan` semantics.
- `Application/Handlers/UpdatePlayResultCommand.Green.cs` - validate stage mode and reward ranges, persist `IsShin`, split best upsert.
- `Application/Handlers/GetSelfBestQuery.Green.cs` - return normal and Shin saved bests separately.
- `Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs` - filter out Shin bests for crown response.
- `Host/bin/Debug/net10.0/wwwroot/taiko.db3` - local runtime database to replay the rejected save after code and migration are verified.

## Execution Order

1. [Task 1 - Mapper Presence Tests](01-mapper-presence-tests.md)
2. [Task 2 - Schema And Migration](02-schema-and-migration.md)
3. [Task 3 - Playresult Persistence](03-playresult-persistence.md)
4. [Task 4 - Selfbest And Crowns](04-selfbest-and-crowns.md)
5. [Task 5 - Reward Rejection Fix](05-reward-rejection-fix.md)
6. [Task 6 - Replay Failed Save](06-replay-failed-save.md)
7. [Task 7 - Final Verification](07-final-verification.md)

## Verification Commands

Run these before claiming completion:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Green
dotnet build
```

Expected result: both commands exit `0`.
