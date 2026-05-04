# PR2 Continuation State (resume point)

**Updated:** 2026-05-04 after T2 + reviews + push

This doc captures what's done, what's next, and the gotchas a fresh Claude Code session needs to continue PR2 of the clean architecture refactor for TaikoLocalServer.

---

## Branch state

- Branch: `clean-arch/pr2-app-infra`
- HEAD: `a634118`
- Pushed to origin: yes (last push at HEAD)
- Base: `dev` at `175c469` (PR1 merged into local dev; not pushed to origin/dev)

## Commits so far (PR2)

```
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

- `dotnet build Application/Application.csproj` — **6 errors**, all about `using TaikoLocalServer.Settings;` and `ServerSettings` references in `Application/Handlers/GetInitialDataQuery.cs` + `Application/Handlers/UserDataQuery.cs`. **These are T3's responsibility** (Phase 2.8 splits `ServerSettings` into Application's portion).
- `dotnet build TaikoWebUI/TaikoWebUI.csproj` — **0 errors**.
- `dotnet build` whole solution — broken in TaikoLocalServer (handlers folder gone, references broken). Expected; T3+ phases fix it.

---

## Critical context / deviations from the plan

These are decisions made during T2 that future tasks need to respect:

1. **ServerData stays in SharedProject for PR2.** The 8 files (`DanData`, `EventFolderData`, `MovieData`, `QRCodeData`, `ShopFolderData`, `IVerupNo`, `MusicDetail`, `SongIntroductionData`) are NOT moved to `Application/ServerData/`. Reason: TaikoWebUI (Blazor WASM SDK) cannot reference Application because Application has `<FrameworkReference Include="Microsoft.AspNetCore.App" />`. PR4's SharedProject split will handle this properly. **`Application/ServerData/` directory does not exist; do not recreate it in PR2.**

2. **`Application.csproj` has a temporary `<ProjectReference>` to `SharedProject`** (with an inline XML comment explaining). This keeps `IGameDataCatalog` resolving against `SharedProject.Models.X` types. Remove in PR4.

3. **`IGameDataCatalog` uses the method-based surface from `IGameDataService`**, NOT the spec §4 property-based surface. The spec §4 surface had gaps (missing `GetCostumeList`, `GetMusicDetailDictionary`, etc.) and referenced types that don't exist (`TokenData`, `GaidenData` as standalone classes). PR2 keeps the proven method surface; the property-based redesign is deferred to a follow-up PR.

4. **`Application/Handlers/GetAiScoreQuery.cs` has inlined LINQ** for the entity→Common DTO mapping. Reason: the original `AiScoreMappers.MapAsSuccess` lives in `TaikoLocalServer/Mappers/` which Application cannot reference (and Mapperly source generation needs to see the consumer). Code reviewer flagged this as Minor — acceptable for PR2.

5. **`Microsoft.Extensions.Logging` is a global using in `Application/GlobalUsings.cs`** because Application uses `Microsoft.NET.Sdk` (not `.Web`) and ILogger<T> doesn't come implicitly.

6. **TaikoLocalServer/GlobalUsings.cs still has `global using TaikoLocalServer.Models;` and `global using TaikoLocalServer.Models.WW08;`** — these stay until PR3 (Mucha wire types still in `TaikoLocalServer/Models/`).

7. **Branch naming**: use `clean-arch/X` style, NOT `dev/X` (the latter collides with `refs/heads/dev`).

8. **A route-count baseline log line was added to Program.cs in PR2.0.** When the user runs the server (admin required for ports 80/443), they should record the printed `Mapped {N} endpoints` value. PR2's verification gate 4 + PR3's gate 2 require this baseline to remain unchanged. **Not yet captured** — capture before/at smoke test.

---

## Remaining tasks

The plan file is `docs/superpowers/plans/2026-05-04-clean-arch-pr2-application-infrastructure.md`. The spec is `docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md`.

### T3 — Common utils + ServerSettings split + AddApplication (Phases 2.7, 2.8, 2.9)

**Phase 2.7** — Move 4 utilities into `Application/Common/`:
- `git mv TaikoLocalServer/Common/Utils/FlagCalculator.cs Application/Common/`
- `git mv TaikoLocalServer/Common/Utils/Extensions.cs Application/Common/`
- `git mv TaikoLocalServer/Common/OrderedSet.cs Application/Common/`
- `git mv SharedProject/Utils/ValueHelpers.cs Application/Common/`
- Update each file's namespace to `TaikoLocalServer.Application.Common`.
- Create `Application/Common/Constants.cs` with `public const string DateTimeFormat = "yyyyMMddHHmmss";`.
- Trim `TaikoLocalServer/Common/Constants.cs` to only `DefaultDbName` + the 6 `*BaseName` entries.
- Repoint consumers' usings.

**Phase 2.8** — Split `ServerSettings`:
- Create `Application/Settings/ServerSettings.cs` with just `EnableMoreSongs` + `MoreSongsSize` (default uses `TaikoLocalServer.Domain.DomainConstants.MusicIdMaxExpanded`). Namespace `TaikoLocalServer.Application.Settings`.
- Don't delete `TaikoLocalServer/Settings/ServerSettings.cs` yet — Program.cs still binds it. The split happens fully in Phase 2.18 where `AllnetSettings` (Infrastructure) gets bound to the same JSON section.

**Phase 2.9** — Create `Application/DependencyInjection.cs`:
```csharp
using Microsoft.Extensions.DependencyInjection;
namespace TaikoLocalServer.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(opt =>
        {
            opt.Namespace = "TaikoLocalServer.Application";
            opt.ServiceLifetime = ServiceLifetime.Scoped;
        });
        return services;
    }
}
```
- After this, build Application alone. Inspect `Application/obj/Debug/net10.0/generated/Mediator.SourceGenerator/` for handler registrations. If empty (Risk #2), invoke fallback at Phase 2.9.2 (move AddMediator to Host with explicit assembly markers).
- Commit cadence: 1-2 commits.

**Build expectation after T3**: Application builds clean. The 6 ServerSettings errors are gone. TaikoLocalServer still has wrapper-service errors (T8/T10 fix).

### T4 — Rename GameDatabase → TaikoLocalServer.Infrastructure (Phase 2.10)

**Critical**: this is a folder + csproj + namespace rename touching ~30 files.

- `git mv GameDatabase Infrastructure`
- `git mv Infrastructure/GameDatabase.csproj Infrastructure/Infrastructure.csproj`
- Edit `Infrastructure/Infrastructure.csproj`: `<RootNamespace>TaikoLocalServer.Infrastructure</RootNamespace>`, `<AssemblyName>TaikoLocalServer.Infrastructure</AssemblyName>`, ProjectReferences to `Application` + `Domain`, packages: `BCrypt.Net-Next`, `EntityFrameworkCore.Exceptions.Sqlite`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Tools` (PrivateAssets), `SharpZipLib`, `System.IdentityModel.Tokens.Jwt`, `Throw`, `Yoh.Text.Json.NamingPolicies`.
- Project-wide rename: `GameDatabase.Context` → `TaikoLocalServer.Infrastructure.Persistence`; `GameDatabase.Migrations` → `TaikoLocalServer.Infrastructure.Persistence.Migrations`; `GameDatabase` → `TaikoLocalServer.Infrastructure` (be careful with order so nested replacements don't trip).
- Move folders: `git mv Infrastructure/Context Infrastructure/Persistence` and `git mv Infrastructure/Migrations Infrastructure/Persistence/Migrations`.
- Update `.slnx` and consumer csprojs (`TaikoLocalServer.csproj`, `LocalSaveModScoreMigrator.csproj`).
- **Don't bulk-sed unrelated files** (line-ending normalization issue per PR1 incident memory).

**Build expectation after T4**: Infrastructure builds. TaikoLocalServer still has wrapper-service errors.

### T5 — TaikoDbContext interface + JwtTokenService + SystemClock + AllnetSettings (Phases 2.11, 2.12)

- Modify `Infrastructure/Persistence/TaikoDbContext.cs` (or `TaikoDbContextPartial.cs`, whichever has the class declaration): change `public partial class TaikoDbContext : DbContext` → `public partial class TaikoDbContext : DbContext, ITaikoDbContext`. Add `using TaikoLocalServer.Application.Abstractions;`.
- `git mv TaikoLocalServer/Settings/AuthSettings.cs Infrastructure/Identity/Settings/AuthSettings.cs`. Namespace: `TaikoLocalServer.Infrastructure.Identity.Settings`.
- Create `Infrastructure/Identity/JwtTokenService.cs` implementing `IJwtTokenService`. Lift `IssueToken` from inline `JwtSecurityTokenHandler` usage in `TaikoLocalServer/Controllers/Api/AuthController.cs`'s Login flow. Lift `ExtractTokenInfo` from `TaikoLocalServer/Services/AuthService.cs:190-230`. **CRITICAL: copy `Expires = DateTime.UtcNow.AddDays(1)` (or whatever AuthController.Login currently uses) verbatim** — wrong value silently changes session lifetime (Risk #3).
- Create `Infrastructure/Time/SystemClock.cs` implementing `IClock`.
- Create `Infrastructure/Settings/AllnetSettings.cs` (`MuchaUrl`, `GameUrl`).

### T6 — FileGameDataCatalog orchestrator (Phase 2.13)

- Copy `TaikoLocalServer/Services/GameDataService.cs` to `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`. Rename class. Implement `IGameDataCatalog` (which now has the 22 method surface from T2).
- `git mv SharedProject/Utils/PathHelper.cs Infrastructure/GameDataCatalog/PathHelper.cs`.
- `git mv TaikoLocalServer/Settings/DataSettings.cs Infrastructure/GameDataCatalog/Settings/DataSettings.cs`.
- Create `Infrastructure/GameDataCatalog/CatalogConstants.cs` (the 6 `*BaseName` entries).
- Trim `TaikoLocalServer/Common/Constants.cs` to just `DefaultDbName`.
- `git rm TaikoLocalServer/Services/GameDataService.cs` and `TaikoLocalServer/Services/Interfaces/IGameDataService.cs`.
- Repoint consumers (`Program.cs`, anyone else): `IGameDataService` → `IGameDataCatalog`.
- **Defer per-loader split** to a follow-up PR — leave a TODO comment in `FileGameDataCatalog.cs`.

### Phase 2.15 — Mediator generator sanity check (coordinator does this)

- After T3-T7 land, build Application alone, inspect `Application/obj/Debug/net10.0/generated/Mediator.SourceGenerator/` for `.g.cs` files listing all 16 handler registrations. If empty → invoke Risk #2 fallback (Phase 2.9.2). If 1-15 (partial) → fix non-conforming handlers (typically `record class` instead of `readonly record struct`).

### T7 — PersistenceConstants + AddInfrastructure DI (Phase 2.14)

- Create `Infrastructure/Persistence/PersistenceConstants.cs` with `public const string DefaultDbName = "taiko.db3";`.
- Repoint `Constants.DefaultDbName` consumers (mainly Program.cs).
- `git rm TaikoLocalServer/Common/Constants.cs` (if empty after).
- Create `Infrastructure/GlobalUsings.cs` (Microsoft.EntityFrameworkCore, TaikoLocalServer.Application.Abstractions, TaikoLocalServer.Domain.Entities).
- Create `Infrastructure/DependencyInjection.cs` with `AddInfrastructure(IConfiguration)`:
  - Bind `AuthSettings`, `DataSettings`, `ServerSettings` (Application's), `AllnetSettings` (Infrastructure's, on the same JSON section as ServerSettings).
  - `AddDbContext<TaikoDbContext>` with the SQLite + ConfigureWarnings setup verbatim from current Program.cs.
  - `AddScoped<ITaikoDbContext>(sp => sp.GetRequiredService<TaikoDbContext>())`.
  - `AddSingleton<IGameDataCatalog, FileGameDataCatalog>()`.
  - `AddScoped<IJwtTokenService, JwtTokenService>()`.
  - `AddAuthentication(...)` + `AddJwtBearer(...)` lifted from current Program.cs.
  - `AddSingleton<IClock, SystemClock>()`.

### T8 — Rewrite simple admin controllers Pattern A+B (Phase 2.16.2-2.16.6)

8 controllers stay in `TaikoLocalServer/Controllers/Api/` (PR3 relocates them):
- **Pattern A** (inject `ITaikoDbContext` + `IJwtTokenService` + `IOptions<AuthSettings>`): `UsersController`, `CardsController`, `FavoriteSongsController`, `DanBestDataController`, `PlayDataController`, `PlayHistoryController`, `SongLeaderboardController`, `UserSettingsController`.
- **Pattern B** (inject `IGameDataCatalog` only): `GameDataController`.
- Replace each `IUserDatumService.X(...)`, `ISongBestDatumService.X(...)`, etc. wrapper-service call with the equivalent direct `context.X.<query>` LINQ inlined. Read the wrapper service body for the query shape.
- The plan has a complete worked example for `UsersController` at Phase 2.16.2 lines 2070-2273. Use that template.
- `tokenInfo.Value.IsAdmin` (PascalCase) — match the new `JwtTokenInfo` record struct. Old code may have `.isAdmin` (lowercase from tuple).
- Save: `await context.SaveChangesAsync(HttpContext.RequestAborted);`.

### T9 — Rewrite AuthController (Phase 2.16.7)

- 271 LOC controller. Inject `ITaikoDbContext` + `IJwtTokenService` + `IOptions<AuthSettings>`. Replace inline `JwtSecurityTokenHandler` usage with `jwtTokens.IssueToken(baid, isAdmin)`. Replace `IAuthService.X` calls with direct DB queries. **Preserve OTP secret-handling logic line-by-line.** Risk #3 — capture baseline auth response shapes if possible.

### T10 — Delete wrappers + slim Program.cs + Migrator (Phases 2.17, 2.18, 2.19)

- `git rm` 13 wrapper service files: `AuthService.cs`, `UserDatumService.cs`, `SongBestDatumService.cs`, `SongPlayDatumService.cs`, `DanScoreDatumService.cs`, `SongLeaderboardService.cs`, `IAuthService.cs`, `IUserDatumService.cs`, `ISongBestDatumService.cs`, `ISongPlayDatumService.cs`, `IDanScoreDatumService.cs`, `ISongLeaderboardService.cs`, `Services/Extentions/ServiceExtensions.cs`. Then remove the empty `Services/` directories.
- Update `TaikoLocalServer/GlobalUsings.cs`: remove `using TaikoLocalServer.Services;` and `using TaikoLocalServer.Services.Interfaces;`. Add `using TaikoLocalServer.Application;` and `using TaikoLocalServer.Infrastructure;` if needed.
- **Slim Program.cs**: Replace the registration block (`AddMediator`, `AddSingleton<IGameDataService, ...>`, `AddDbContext`, `AddJwtBearer`, etc.) with:
  ```csharp
  builder.Services.AddOptions();
  builder.Services.AddApplication();
  builder.Services.AddInfrastructure(builder.Configuration);
  builder.Services.AddScoped<AuthorizeIfRequiredAttribute>();
  builder.Services.AddControllers().AddProtoBufNet();
  builder.Services.AddMemoryCache();
  builder.Services.AddCors(...);
  builder.Services.AddSingleton<SongBestResponseMapper>();
  ```
  And update `var gameDataService = app.Services.GetService<IGameDataService>()` → `var gameDataCatalog = app.Services.GetService<IGameDataCatalog>()`.
- Remove now-unused `using TaikoLocalServer.Services.Extentions;`, `using TaikoLocalServer.Settings;` from Program.cs.
- LocalSaveModScoreMigrator: drop GameDatabase + SharedProject project refs (already gone after T4 rename; just verify), add Infrastructure + Domain project refs. Update its using statements: `using GameDatabase.Context;` → `using TaikoLocalServer.Infrastructure.Persistence;`; `using GameDatabase.Entities;` → `using TaikoLocalServer.Domain.Entities;`; `using SharedProject.Enums;` → `using TaikoLocalServer.Domain.Enums;`; `using SharedProject.Utils;` → as needed.

### Phase 2.20 — User smoke gates (user, not Claude)

User runs the server with admin (ports 80/443) and exercises:
1. Build green.
2. Server starts; `Mapped {N} endpoints` matches PR2 baseline.
3. Game endpoint smoke (both `_ww` and `_cn`).
4. Admin API smoke (auth off): `GET /api/users`, `GET /api/users/{baid}`, `DELETE /api/users/{baid}`.
5. Admin API smoke (auth on): `POST /api/auth/login` returns JWT; cross-user baid rejected.
6. WebUI loads, Dashboard renders, login round-trips.
7. Migrator builds + starts.

---

## How to resume in a fresh session

In a new Claude Code session, paste this prompt as first message (or similar):

> Continuing PR2 of clean architecture refactor. Read `docs/superpowers/plans/PR2-CONTINUATION.md` for state. Branch is `clean-arch/pr2-app-infra` at HEAD `a634118` with T1+T2 complete. Use `superpowers:subagent-driven-development` to execute remaining tasks T3 through T10 + Phase 2.15 + Phase 2.20. Spec/plan files are under `docs/superpowers/`. Honor the deviations documented in PR2-CONTINUATION.md.

Then dispatch T3 first (Phases 2.7-2.9). Build will go green for Application after T3. Continue through T10 with two-stage review per task per the skill.

---

## TodoList state at handoff

- #11 [completed] Phase 2.0: Branch + route count log
- #10 [completed] T1: Application skeleton + 4 ports (2.1-2.2)
- #21 [completed] T2 + T2.6.fix: Move handlers + DTOs + Catalog VOs (2.3-2.6 + ServerData revert)
- #15 [pending] T3: Common utils + ServerSettings split + AddApplication (2.7-2.9)
- #19 [pending] T4: Rename GameDatabase → Infrastructure (2.10)
- #17 [pending] T5: TaikoDbContext + JwtTokenService + SystemClock + AllnetSettings (2.11-2.12)
- #16 [pending] T6: FileGameDataCatalog orchestrator (2.13)
- #22 [pending] T7: PersistenceConstants + AddInfrastructure DI (2.14)
- #18 [pending] Phase 2.15: Mediator generator sanity check
- #20 [pending] T8: Rewrite simple admin controllers Pattern A+B (2.16.2-2.16.6)
- #12 [pending] T9: Rewrite AuthController (2.16.7)
- #13 [pending] T10: Delete wrappers + slim Program.cs + Migrator (2.17-2.19)
- #14 [pending] Phase 2.20: User smoke gates

The IDs aren't ordered by execution; new session can recreate them with TaskCreate.
