# PR2 Continuation State (resume point)

**Updated:** 2026-05-04 after T3 + T4 + planning split

This doc captures what's done, what's next, and the gotchas a fresh Claude Code session needs to continue PR2 of the clean architecture refactor for TaikoLocalServer.

---

## Branch state

- Branch: `clean-arch/pr2-app-infra`
- HEAD: `fade919` (local, **2 commits ahead of origin** — push pending)
- Base: `dev` at `175c469` (PR1 merged into local dev; not pushed to origin/dev)

## Commits so far (PR2)

```
fade919 PR2.10: rename GameDatabase project to TaikoLocalServer.Infrastructure
854c2a1 PR2.7-2.9: move Common utils into Application.Common; split ServerSettings; AddApplication() DI
f0f94b0 PR2.handoff: continuation doc for next session
a634118 PR2.6.fix2: document temporary SharedProject ref in Application.csproj
6556987 PR2.6.fix: revert ServerData move; preserve SharedProject home until PR4 (Blazor WASM cannot ref Application)
03a53fc PR2.6: move ServerData shapes into Application.ServerData; expand IGameDataCatalog surface
9dbfad1 PR2.5: move Catalog VOs into Application.Catalog
751a08f PR2.4: move Common* DTOs into Application.Dtos
0d090fb PR2.3: move handlers into Application; switch to ITaikoDbContext + IGameDataCatalog injection
e409fec PR2.2: define the four Application ports (ITaikoDbContext, IGameDataCatalog stub, IJwtTokenService, IClock)
f4697a6 PR2.1: scaffold empty TaikoLocalServer.Application project
bd855bf PR2.0: log mapped endpoint count at startup
```

## Build state at HEAD

- `dotnet build` whole solution — **0 errors, 91 warnings** (warnings are pre-existing Mapperly RMG020/RMG012 + 2 SharedProject CS8618).
- Generated Mediator handlers: **16 handler registrations** in `Application/obj/Debug/net10.0/generated/Mediator.SourceGenerator/...` — Risk #2 non-firing.

---

## Critical context / deviations from the plan

These decisions made during T2-T4 that future tasks need to respect:

1. **ServerData stays in SharedProject for PR2.** The 8 files (`DanData`, `EventFolderData`, `MovieData`, `QRCodeData`, `ShopFolderData`, `IVerupNo`, `MusicDetail`, `SongIntroductionData`) are NOT moved to `Application/ServerData/`. Reason: TaikoWebUI (Blazor WASM SDK) cannot reference Application because Application has `<FrameworkReference Include="Microsoft.AspNetCore.App" />`. PR4's SharedProject split will handle this properly. **`Application/ServerData/` directory does not exist; do not recreate it in PR2.**

2. **`Application.csproj` has a temporary `<ProjectReference>` to `SharedProject`** (with an inline XML comment explaining). This keeps `IGameDataCatalog` resolving against `SharedProject.Models.X` types. Remove in PR4.

3. **`IGameDataCatalog` uses the method-based surface from `IGameDataService`**, NOT the spec §4 property-based surface. The spec §4 surface had gaps; PR2 keeps the proven method surface. Property-based redesign is deferred.

4. **`Application/Handlers/GetAiScoreQuery.cs` has inlined LINQ** for the entity→Common DTO mapping. Reason: original `AiScoreMappers.MapAsSuccess` lives in `TaikoLocalServer/Mappers/` which Application cannot reference. Code reviewer flagged Minor — acceptable for PR2.

5. **`Microsoft.Extensions.Logging` is a global using in `Application/GlobalUsings.cs`** because Application uses `Microsoft.NET.Sdk` (not `.Web`) and `ILogger<T>` doesn't come implicitly.

6. **Mediator.SourceGenerator was removed from `TaikoLocalServer.csproj`** in T4 (PR2.10 commit). Application owns Mediator; before T4, both projects generated `AddMediator` and the build had ambiguous-reference errors. The package stays in Application. Mediator.Abstractions stays in TaikoLocalServer.csproj.

7. **`using Constants = TaikoLocalServer.Common.Constants;` aliases were added to `TaikoLocalServer/Program.cs` and `TaikoLocalServer/Services/GameDataService.cs`** in T3/T4 to disambiguate from `TaikoLocalServer.Application.Common.Constants`. T6 deletes GameDataService.cs (alias gone with it); T7 changes Program.cs to use `PersistenceConstants.DefaultDbName` (alias removed).

8. **`Application/Handlers/GetSelfBestQuery.cs` uses `string.Join(", ", ...)` instead of `.Stringify()`** — Application doesn't reference Swan, so the Unosquare logger Stringify extension isn't available. Inline `string.Join` is the replacement (debug-log only — purely cosmetic in error path).

9. **TaikoLocalServer/GlobalUsings.cs after T3-T4** has `global using TaikoLocalServer.Application.Common;` (Constants/FlagCalculator/etc. live there now). The `global using TaikoLocalServer.Common;` and `global using TaikoLocalServer.Common.Utils;` were removed; explicit `using` statements were added in:
   - `TaikoLocalServer/Controllers/Game/CrownsDataController.cs` (GZipBytesUtil)
   - `TaikoLocalServer/Controllers/Game/GetScoreRankController.cs` (GZipBytesUtil)
   - `TaikoLocalServer/Controllers/Game/PlayResultController.cs` (GZipBytesUtil)
   - `TaikoLocalServer/Controllers/AmAuth/PowerOnController.cs` (FormOutputUtil)
   - `TaikoLocalServer/Controllers/AmUpdater/MuchaController.cs` (FormOutputUtil)

10. **TaikoLocalServer/GlobalUsings.cs still has `global using TaikoLocalServer.Models;` and `global using TaikoLocalServer.Models.WW08;`** — these stay until PR3 (Mucha + WW wire types still in `TaikoLocalServer/Models/`).

11. **Branch naming**: use `clean-arch/X` style, NOT `dev/X` (the latter collides with `refs/heads/dev`).

12. **Route-count baseline log line** added to Program.cs in PR2.0. When the user runs the server, they should record the printed `Mapped {N} endpoints` value. Phase 2.20 + PR3 gate 2 require this baseline to remain unchanged. **Not yet captured** — capture before/at smoke test.

---

## Remaining tasks (read each task file before executing)

Per-task plan files live in [`pr2-tasks/`](pr2-tasks/README.md). Each is self-contained — about 100-200 lines vs the 2700-line master plan.

| Task | Phase(s) | Plan file |
|------|----------|-----------|
| T5 | 2.11, 2.12 | [pr2-tasks/T5.md](pr2-tasks/T5.md) — TaikoDbContext implements ITaikoDbContext; create JwtTokenService, SystemClock, AllnetSettings |
| T6 | 2.13 | [pr2-tasks/T6.md](pr2-tasks/T6.md) — FileGameDataCatalog orchestrator |
| T7 | 2.14 | [pr2-tasks/T7.md](pr2-tasks/T7.md) — PersistenceConstants + AddInfrastructure() DI |
| T8 | 2.16.2–6 | [pr2-tasks/T8.md](pr2-tasks/T8.md) — rewrite 8 simple admin controllers + GameDataController |
| T9 | 2.16.7 | [pr2-tasks/T9.md](pr2-tasks/T9.md) — rewrite AuthController (preserve JWT Expires) |
| T10 | 2.17, 2.18, 2.19 | [pr2-tasks/T10.md](pr2-tasks/T10.md) — delete wrappers, slim Program.cs, finalize Migrator |
| Phase 2.20 | 2.20 | [pr2-tasks/Phase2.20.md](pr2-tasks/Phase2.20.md) — user smoke gates |

The master plan is `2026-05-04-clean-arch-pr2-application-infrastructure.md`. The spec is `../specs/2026-05-04-clean-architecture-refactor-design.md`. Read those only if a per-task file is unclear or a deviation needs revisiting.

---

## How to resume in a fresh session

In a new Claude Code session, paste this prompt as first message:

> Continuing PR2 of clean architecture refactor for TaikoLocalServer.
> 1. Read `docs/superpowers/plans/PR2-CONTINUATION.md` for branch state + deviations.
> 2. Read `docs/superpowers/plans/pr2-tasks/<NEXT>.md` for the next task only (don't load the master plan unless that file references it).
> 3. Use `superpowers:executing-plans` to execute. Honor the deviations in PR2-CONTINUATION.md. Branch is `clean-arch/pr2-app-infra`.

Replace `<NEXT>` with `T5` for the immediate next task.
