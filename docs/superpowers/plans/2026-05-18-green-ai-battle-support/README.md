# Green AI Battle (Ghost) Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers-extended-cc:subagent-driven-development (recommended) or superpowers-extended-cc:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Unblock Green AI Battle (`PlayMode = 6`) play-result submissions. The current handler rejects every credit because `StageMode = 3/4` is outside the allowed `{0, 1}` set. After this plan: AI Battle plays are accepted, the per-stage ghost section graph and the per-credit rank / winnings / tokens / perf data are persisted, `GhostPlayedSongFlag` reflects every song played in AI Battle, and crowns honor the wiki's 正規-level caveat.

**Architecture:** Two small pure-function helpers in `Application/Common/` interpret `StageMode` (`IsShin`, `IsAiBattle`) and identify 正規 AI difficulty levels (`IsCertifiedLevel`). The Green play-result handler — `Application/Handlers/UpdatePlayResultCommand.Green.cs` — is the only behavioral file edited: it loosens the StageMode predicate, replaces inline boolean checks with the helpers, gates `BestCrown` mutation, and adds a small private static that sets bits in `GhostPlayedSongFlag`. The existing `ApplyGhostUpdates` already handles rank / winnings / tokens / perf data and needs no change. The plan does NOT touch the database schema, mappers, controllers, or WebUI.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core 10, SQLite (in-memory for tests), protobuf-net, Mediator, xUnit.

**Reference:** [docs/superpowers/specs/2026-05-18-green-ai-battle-support-design.md](../../specs/2026-05-18-green-ai-battle-support-design.md) (commit `531fa9bb`).

---

## Worktree And Safety

The repo has unrelated working-tree changes in `Adapters.AllnetMucha/`, `Host/README.md`, `Infrastructure/GameDataCatalog/Green/Extractor/Merging/`, `Tests/Green/GreenCustomizationExtractorTests.cs`, and `README.md`. Do not revert them. Stage only files explicitly listed in each task. Run `git status --short` before every commit.

## File Structure

- **Add** `Application/Common/GreenStageModeInterpreter.cs` — static `IsShin(uint)` and `IsAiBattle(uint)` over the `{0,1,3,4}` StageMode enum.
- **Add** `Application/Common/GreenAiBattleLevels.cs` — static `IsCertifiedLevel(uint)` over the 正規 set `{1, 5, 9, 13}`.
- **Modify** `Application/Handlers/UpdatePlayResultCommand.Green.cs`:
  - Remove `MaxGreenStageMode` constant.
  - In `IsValidGreenStage`, replace `stage.StageMode <= MaxGreenStageMode` with `stage.StageMode is 0 or 1 or 3 or 4`.
  - In `SaveStageAsync`, compute `isShin` via the interpreter and compute `allowCrownUpdate` from the chart-vs-AI rule (see Task 2).
  - Thread `allowCrownUpdate` into `UpsertBestAsync`.
  - Add `ApplyGhostPlayedSongBits(saveData, playResultData)` and call it from `HandleGreen` after the per-stage loop, before `SaveChangesAsync`.
- **Add** `Tests/Green/GreenStageModeInterpreterTests.cs` — table-driven `[Theory]` over stage modes 0..5.
- **Add** `Tests/Green/GreenAiBattleLevelsTests.cs` — table-driven `[Theory]` over 0..14 plus an out-of-range case.
- **Add** `Tests/Green/GreenAiBattlePlayResultTests.cs` — seven `[Fact]`s exercising the full handler flow through `GreenHandlerFixture` (in-memory SQLite).

## Execution Order

1. [Task 1 — Helpers + Unit Tests](01-helpers.md) — both helpers and both helper-test files in one commit.
2. [Task 2 — Handler Edits](02-handler-edits.md) — edit `UpdatePlayResultCommand.Green.cs`; existing Green tests must still pass.
3. [Task 3 — AI Battle Integration Tests](03-integration-tests.md) — add `GreenAiBattlePlayResultTests.cs` covering accept / Shin routing / crown gating / Ura / unknown-stage-mode / non-AI play.

Tasks are strictly ordered. Task 2 depends on Task 1's helpers. Task 3's integration tests assert behavior introduced by Task 2.

## Verification Commands

Run before claiming the plan complete:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
dotnet build
```

Expected: both exit `0`. The `~Green` filter is broad enough to catch any regression in adjacent Green areas (Dani, customization, selfbest, crowns, etc.).

End-to-end (manual, run by the user per CLAUDE.md): replay the captured AI Battle payload via the running server and observe:

- No `Rejecting invalid Green playresult payload` warning.
- `getghostdata.php` response reflects persisted `total_winnings`, `ghost_record_data`, `ary_token_data`, and the played-song flag bits.
- `getghostscore.php` for a played song returns the section data persisted from the play.
