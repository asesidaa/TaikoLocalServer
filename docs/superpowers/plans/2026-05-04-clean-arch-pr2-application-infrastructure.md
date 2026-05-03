# PR2 — Application + Infrastructure Carve-Out Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract two new projects — `TaikoLocalServer.Application` (handlers, DTOs, ports) and `TaikoLocalServer.Infrastructure` (DbContext implementation, game data catalog, JWT, clock). Define the four ports from spec §4. Drop the wrapper data-access services and rewrite admin controllers to inject `ITaikoDbContext` + `IJwtTokenService` directly. The 566-line `GameDataService` is split into `FileGameDataCatalog` + 18 single-table loaders. The 230-line `AuthService` is bisected: JWT methods migrate to `JwtTokenService`; data-access methods are absorbed into the rewritten controllers.

**Architecture:** This PR is the structural heart of the refactor. After it, the dependency graph reads `Domain ← Application ← Infrastructure ← TaikoLocalServer (host placeholder)`. Adapter projects don't exist yet — controllers, mappers, wire types still live under `TaikoLocalServer/`. PR3 physically relocates those into adapter projects. Everything in PR2 happens **without moving controllers' folder location**: their dependency-injection surface flips, but their `TaikoLocalServer/Controllers/...` paths are unchanged. This keeps the controller rewrite reviewable in isolation from the adapter extraction.

**Tech Stack:** .NET 10 SDK, EF Core 10, Mediator 3.0.2 (martinothamar) source generator, JwtBearer, BCrypt.Net-Next, SharpZipLib, Throw.

**Spec reference:** `docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md` — see §3 (Application + Infrastructure subsections), §4 (port surface + admin-controller flow table), §6 (Mediator namespace, Settings POCO distribution, Constants.cs split rows for Application/Infrastructure), §7 (PR2 verification gates), §8 risks #2 and #3.

**Prerequisites:** PR1 (`dev/clean-arch-domain`) is merged into `dev`. Run:

```bash
git checkout dev
git pull --ff-only origin dev
```

Confirm `dotnet build` succeeds and `Domain/` is present in the working tree.

---

## How this plan works

This repo has no automated tests. Verification is build-and-smoke. Every multi-step task ends with a `dotnet build` (and where applicable a server smoke test), then a commit. Frequent commits are encouraged — each phase below ends in a green build.

All commands assume the working directory is the repo root (`D:\TaikoLocalServer\`).

## Final layout this PR produces

```
Domain/                                  (unchanged from PR1)
Application/                             ← NEW
  Application.csproj                     <RootNamespace>=TaikoLocalServer.Application
  Abstractions/
    ITaikoDbContext.cs
    IGameDataCatalog.cs
    IJwtTokenService.cs   (+ JwtTokenInfo record)
    IClock.cs
  Handlers/                              (moved from TaikoLocalServer/Handlers/)
  Dtos/                                  (Common*.cs from TaikoLocalServer/Models/Application/)
  Catalog/                               (read-side VOs from TaikoLocalServer/Models/)
  ServerData/                            (strongly-typed shapes from SharedProject/Models/)
  Common/
    Constants.cs                         (DateTimeFormat only)
    FlagCalculator.cs, Extensions.cs, OrderedSet.cs, ValueHelpers.cs
  Settings/
    ServerSettings.cs                    (EnableMoreSongs, MoreSongsSize portion)
  GlobalUsings.cs
  DependencyInjection.cs                 (AddApplication())

Infrastructure/                          ← renamed from GameDatabase/
  Infrastructure.csproj                  <RootNamespace>=TaikoLocalServer.Infrastructure
  Persistence/
    TaikoDbContext.cs                    (": DbContext, ITaikoDbContext")
    TaikoDbContextPartial.cs
    Migrations/                          (verbatim move from GameDatabase/Migrations/)
    PersistenceConstants.cs              (DefaultDbName)
  GameDataCatalog/
    FileGameDataCatalog.cs               (impl of IGameDataCatalog; aggregates loaders)
    Loaders/                             (one per source datatable, ~18 files)
    PathHelper.cs                        (moved from SharedProject/Utils/)
    CatalogConstants.cs                  (*BaseName entries)
    Settings/DataSettings.cs             (moved from TaikoLocalServer/Settings/)
  Identity/
    JwtTokenService.cs                   (impl of IJwtTokenService)
    Settings/AuthSettings.cs             (moved from TaikoLocalServer/Settings/)
  Time/
    SystemClock.cs                       (impl of IClock)
  Settings/
    AllnetSettings.cs                    (MuchaUrl + GameUrl from old ServerSettings)
  GlobalUsings.cs
  DependencyInjection.cs                 (AddInfrastructure(IConfiguration))

TaikoLocalServer/                        (still the host project, soon-to-be-renamed in PR3)
  Program.cs                             (uses AddApplication() + AddInfrastructure(...))
  Controllers/Api/*                       (rewritten to inject ITaikoDbContext + IJwtTokenService)
  Controllers/Game/*                      (handlers now resolve via Application namespace)
  Services/                              GONE (deleted; only AuthService.cs gone, no successor here)
  Settings/                              empty (AuthSettings + DataSettings + ServerSettings moved away;
                                          ServerSettings split between Application + Infrastructure)
  GlobalUsings.cs                        (updated to import Application + Infrastructure namespaces)
  TaikoLocalServer.csproj                (refs Application + Infrastructure)

SharedProject/                           ← contents shrink (ServerData shapes moved to Application;
                                            PathHelper moved to Infrastructure)
                                          Models/{Requests,Responses,ViewModels,Costume,Title,etc.} stay
                                          Utils/{PlaySettingConverter, ValueHelpers} stay
                                          (full SharedProject deletion happens in PR4)

LocalSaveModScoreMigrator/               (refs swap from GameDatabase → Infrastructure;
                                          Domain ref still required)
TaikoWebUI/                              (mostly unchanged; only consequential update is namespace
                                          rewires for ServerData shapes that moved into Application —
                                          most of these aren't WebUI-relevant; SharedProject is its
                                          contract until PR4)
```

`.slnx` count goes from 6 → 8 projects (adds Application, adds Infrastructure, removes GameDatabase since it's renamed in place).

---

## Phase 2.0 — Branch and baseline

### Task 2.0.1: Create the PR2 branch

**Files:** none.

- [ ] **Step 1: Create and check out the PR2 branch**

```bash
git checkout dev
git pull --ff-only origin dev
git checkout -b dev/clean-arch-app-infra
```

- [ ] **Step 2: Sanity-check the starting state**

```bash
dotnet build
```

Expected: build succeeds, 0 errors. PR1's empty `EntityNamespaceMove` migration applies cleanly on startup (the test DB will already have it from PR1 smoke).

### Task 2.0.2: Add a route-count log line for PR2/3 baselines

**Files:**
- Modify: `TaikoLocalServer/Program.cs`

PR2 verification gate 4 + PR3 gate 2 reference a baseline route count. Add a log line right after `app.MapControllers();` so it's visible at startup.

- [ ] **Step 1: Add the route-count log**

In `TaikoLocalServer/Program.cs`, find the line `app.MapControllers();`. Replace with:

```csharp
    app.MapControllers();
    var routeCount = app.Services.GetRequiredService<EndpointDataSource>().Endpoints.Count;
    Log.Information("Mapped {RouteCount} endpoints", routeCount);
```

- [ ] **Step 2: Build and capture the baseline**

```bash
dotnet build
dotnet run --project TaikoLocalServer
```

Watch the startup log for: `Mapped <N> endpoints` (e.g., `Mapped 47 endpoints`). **Record this number.** It must remain unchanged through the end of PR3.

Stop the server with `Ctrl+C` once you've captured the count.

- [ ] **Step 3: Commit**

```bash
git add TaikoLocalServer/Program.cs
git commit -m "PR2.0: log mapped endpoint count at startup"
```

---

## Phase 2.1 — Application project skeleton

### Task 2.1.1: Create Application/Application.csproj

**Files:**
- Create: `Application/Application.csproj`
- Create: `Application/GlobalUsings.cs`
- Modify: `TaikoLocalServer.slnx`

- [ ] **Step 1: Create directories**

```bash
mkdir Application
mkdir Application/Abstractions
mkdir Application/Handlers
mkdir Application/Dtos
mkdir Application/Catalog
mkdir Application/ServerData
mkdir Application/Common
mkdir Application/Settings
```

- [ ] **Step 2: Write Application.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Application</RootNamespace>
    <AssemblyName>TaikoLocalServer.Application</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Domain\Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Mediator.Abstractions" />
    <PackageReference Include="Mediator.SourceGenerator">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Throw" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Write Application/GlobalUsings.cs (initial)**

```csharp
// Global using directives

global using Mediator;
global using Microsoft.EntityFrameworkCore;
global using TaikoLocalServer.Domain.Entities;
global using TaikoLocalServer.Domain.Enums;
global using Throw;
```

(Sub-namespaces like `TaikoLocalServer.Application.Abstractions` are added as files arrive.)

- [ ] **Step 4: Add to solution**

Edit `TaikoLocalServer.slnx`:

```xml
<Solution>
  <Project Path="Application/Application.csproj" />
  <Project Path="Domain/Domain.csproj" />
  <Project Path="GameDatabase/GameDatabase.csproj" />
  <Project Path="LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj" />
  <Project Path="SharedProject/SharedProject.csproj" />
  <Project Path="TaikoLocalServer/TaikoLocalServer.csproj" />
  <Project Path="TaikoWebUI/TaikoWebUI.csproj" />
</Solution>
```

- [ ] **Step 5: Verify Application builds (empty)**

```bash
dotnet build Application/Application.csproj
```

Expected: success.

- [ ] **Step 6: Verify whole solution builds**

```bash
dotnet build
```

Expected: 7 projects build, 0 errors.

- [ ] **Step 7: Commit**

```bash
git add Application/Application.csproj Application/GlobalUsings.cs TaikoLocalServer.slnx
git commit -m "PR2.1: scaffold empty TaikoLocalServer.Application project"
```

---

## Phase 2.2 — Define the four ports

### Task 2.2.1: ITaikoDbContext

**Files:**
- Create: `Application/Abstractions/ITaikoDbContext.cs`

- [ ] **Step 1: Write the interface**

```csharp
namespace TaikoLocalServer.Application.Abstractions;

public interface ITaikoDbContext
{
    DbSet<UserDatum> UserData { get; }
    DbSet<Card> Cards { get; }
    DbSet<Credential> Credentials { get; }
    DbSet<Token> Tokens { get; }
    DbSet<SongBestDatum> SongBestData { get; }
    DbSet<SongPlayDatum> SongPlayData { get; }
    DbSet<DanScoreDatum> DanScoreData { get; }
    DbSet<DanStageScoreDatum> DanStageScoreData { get; }
    DbSet<AiScoreDatum> AiScoreData { get; }
    DbSet<AiSectionScoreDatum> AiSectionScoreData { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

The `using Microsoft.EntityFrameworkCore;` and `using TaikoLocalServer.Domain.Entities;` resolve via Application's GlobalUsings.

- [ ] **Step 2: Build**

```bash
dotnet build Application/Application.csproj
```

Expected: success.

### Task 2.2.2: IGameDataCatalog

**Files:**
- Create: `Application/Abstractions/IGameDataCatalog.cs`

The interface mirrors what the current 566-line `GameDataService` exposes. Open `TaikoLocalServer/Services/GameDataService.cs` and `TaikoLocalServer/Services/Interfaces/IGameDataService.cs` side-by-side; the surface here is identical to `IGameDataService`, just in a new namespace and using new `Catalog/` value types (which haven't moved yet — that's Phase 2.5).

- [ ] **Step 1: Write the interface**

The exact members come from `IGameDataService.cs`. Open it and transcribe each member into the new file with two changes: namespace, and references to `MusicInfoEntry`/`MusicOrderEntry`/`NeiroEntry`/`ShougouEntry`/`WordListEntry`/`DonCosRewardEntry`/etc. should resolve to `TaikoLocalServer.Application.Catalog.*` (after Phase 2.5 moves them).

For initial wiring, write a stub matching the spec §4 surface:

```csharp
using TaikoLocalServer.Application.Catalog;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Application.Abstractions;

public interface IGameDataCatalog
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    IReadOnlyDictionary<uint, MusicInfoEntry> MusicInfoes { get; }
    IReadOnlyDictionary<uint, MusicOrderEntry> MusicOrders { get; }
    IReadOnlyList<uint> EnabledSongIds { get; }
    bool IsSongLocked(uint songId);

    IReadOnlyDictionary<uint, DanData> DanMap { get; }
    IReadOnlyDictionary<uint, DanData> GaidenMap { get; }

    IReadOnlyList<EventFolderData> EventFolderData { get; }
    IReadOnlyList<ShopFolderData> ShopFolderData { get; }
    IReadOnlyList<MovieData> MovieData { get; }
    IReadOnlyList<SongIntroductionData> IntroData { get; }
    IReadOnlyList<QRCodeData> QrCodeData { get; }
    IReadOnlyList<TokenData> TokenData { get; }
    IReadOnlySet<uint> LockedSongs { get; }
    IReadOnlySet<uint> LockedCostumes { get; }
    IReadOnlySet<uint> LockedTitles { get; }
    IReadOnlyList<uint> SpecialSongs { get; }
    IReadOnlyList<GaidenData> GaidenData { get; }

    IReadOnlyDictionary<uint, NeiroEntry> Neiros { get; }
    IReadOnlyDictionary<uint, ShougouEntry> Shougous { get; }
    IReadOnlyDictionary<uint, WordListEntry> WordList { get; }
    IReadOnlyDictionary<uint, DonCosRewardEntry> DonCosRewards { get; }
}
```

Note: `TokenData` and `GaidenData` are server-data types (live in `Application.ServerData/`), not Catalog VOs — same for `DanData`, `MovieData`, `ShopFolderData`, `EventFolderData`, `QRCodeData`, `SongIntroductionData`. `MusicInfoEntry` etc. live in `Catalog/`. After Phase 2.5+2.6 move those types, the `using` lines and `using` statements at top resolve.

**Build will fail at this step** because the referenced types don't exist yet. That's expected — the build returns to green at the end of Phase 2.6.

### Task 2.2.3: IJwtTokenService

**Files:**
- Create: `Application/Abstractions/IJwtTokenService.cs`

- [ ] **Step 1: Write the interface**

```csharp
using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Application.Abstractions;

public readonly record struct JwtTokenInfo(uint Baid, bool IsAdmin);

public interface IJwtTokenService
{
    string IssueToken(uint baid, bool isAdmin);

    JwtTokenInfo? ExtractTokenInfo(HttpContext httpContext);
}
```

The `Microsoft.AspNetCore.Http` reference brings in `HttpContext`. Application doesn't currently ref AspNetCore. Add the framework reference to Application.csproj — but at the **abstraction layer** the cleanest is `Microsoft.Extensions.Http.Abstractions`-style, but `HttpContext` only lives in `Microsoft.AspNetCore.Http`. The lowest-cost addition is adding `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to Application.csproj. This pulls in the ASP.NET Core shared framework — needed because Mediator handlers also routinely accept `HttpContext` indirectly, and Application needs `[FromBody]` etc. to type-check on adapters down the road.

Update `Application/Application.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Application</RootNamespace>
    <AssemblyName>TaikoLocalServer.Application</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Domain\Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Mediator.Abstractions" />
    <PackageReference Include="Mediator.SourceGenerator">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Throw" />
  </ItemGroup>

</Project>
```

> Tradeoff acknowledged: spec §3 says Application "Pulls `Microsoft.EntityFrameworkCore` (abstractions)" and doesn't mention AspNetCore. Pulling the AspNetCore shared framework into Application is a small-but-real expansion; a stricter alternative is changing `IJwtTokenService.ExtractTokenInfo` to accept the raw `Authorization` header value instead of `HttpContext`. **For PR2, take the FrameworkReference shortcut** — it matches the existing AuthService surface exactly. If reviewers push back, refactor to a header-string signature in a follow-up.

### Task 2.2.4: IClock

**Files:**
- Create: `Application/Abstractions/IClock.cs`

- [ ] **Step 1: Write the interface**

```csharp
namespace TaikoLocalServer.Application.Abstractions;

public interface IClock
{
    DateTime Now { get; }

    DateTime UtcNow { get; }
}
```

### Task 2.2.5: Build the four ports together

- [ ] **Step 1: Build**

```bash
dotnet build Application/Application.csproj
```

Expected: build **fails** because `IGameDataCatalog` references types in `Application.Catalog/` and `Application.ServerData/` that don't exist yet. **Comment out** the `IGameDataCatalog` body for now — uncomment after Phase 2.6:

```csharp
namespace TaikoLocalServer.Application.Abstractions;

public interface IGameDataCatalog
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    // Members re-added after Catalog/ + ServerData/ are populated in Phases 2.5, 2.6.
}
```

Re-run `dotnet build Application/Application.csproj`. Expected: success.

- [ ] **Step 2: Commit**

```bash
git add Application/Abstractions Application/Application.csproj
git commit -m "PR2.2: define the four Application ports (ITaikoDbContext, IGameDataCatalog stub, IJwtTokenService, IClock)"
```

(`IGameDataCatalog` is filled in fully in Phase 2.6.)

---

## Phase 2.3 — Move handlers into Application

### Task 2.3.1: Move all 16 handler files

**Files:**
- Move: `TaikoLocalServer/Handlers/*.cs` → `Application/Handlers/*.cs` (16 files)

The 16 files (per `Glob TaikoLocalServer/Handlers/**/*.cs`):

```
AddMyDonEntryCommand.cs, AddTokenCountCommand.cs, BaidQuery.cs,
GetAiDataQuery.cs, GetAiScoreQuery.cs, GetDanOdaiQuery.cs,
GetDanScoreQuery.cs, GetFolderQuery.cs, GetInitialDataQuery.cs,
GetSelfBestQuery.cs, GetShopFolderQuery.cs, GetSongIntroductionQuery.cs,
GetTokenCountQuery.cs, PurchaseSongCommand.cs, UpdatePlayResultCommand.cs,
UserDataQuery.cs
```

- [ ] **Step 1: Move the files**

```bash
git mv TaikoLocalServer/Handlers/AddMyDonEntryCommand.cs        Application/Handlers/
git mv TaikoLocalServer/Handlers/AddTokenCountCommand.cs        Application/Handlers/
git mv TaikoLocalServer/Handlers/BaidQuery.cs                   Application/Handlers/
git mv TaikoLocalServer/Handlers/GetAiDataQuery.cs              Application/Handlers/
git mv TaikoLocalServer/Handlers/GetAiScoreQuery.cs             Application/Handlers/
git mv TaikoLocalServer/Handlers/GetDanOdaiQuery.cs             Application/Handlers/
git mv TaikoLocalServer/Handlers/GetDanScoreQuery.cs            Application/Handlers/
git mv TaikoLocalServer/Handlers/GetFolderQuery.cs              Application/Handlers/
git mv TaikoLocalServer/Handlers/GetInitialDataQuery.cs         Application/Handlers/
git mv TaikoLocalServer/Handlers/GetSelfBestQuery.cs            Application/Handlers/
git mv TaikoLocalServer/Handlers/GetShopFolderQuery.cs          Application/Handlers/
git mv TaikoLocalServer/Handlers/GetSongIntroductionQuery.cs    Application/Handlers/
git mv TaikoLocalServer/Handlers/GetTokenCountQuery.cs          Application/Handlers/
git mv TaikoLocalServer/Handlers/PurchaseSongCommand.cs         Application/Handlers/
git mv TaikoLocalServer/Handlers/UpdatePlayResultCommand.cs     Application/Handlers/
git mv TaikoLocalServer/Handlers/UserDataQuery.cs               Application/Handlers/
```

- [ ] **Step 2: Update namespace in every handler file**

In each `Application/Handlers/*.cs`, change:

```csharp
namespace TaikoLocalServer.Handlers;
```

to:

```csharp
namespace TaikoLocalServer.Application.Handlers;
```

- [ ] **Step 3: Replace `TaikoDbContext` injection with `ITaikoDbContext`**

Every handler currently has a constructor like:

```csharp
public class FooHandler(TaikoDbContext context, ILogger<FooHandler> logger) : IRequestHandler<FooQuery, FooResponse>
```

Inspect each handler. Replace `TaikoDbContext` with `ITaikoDbContext`:

```csharp
public class FooHandler(ITaikoDbContext context, ILogger<FooHandler> logger) : IRequestHandler<FooQuery, FooResponse>
```

Add `using TaikoLocalServer.Application.Abstractions;` to the top of any handler that doesn't already get it via a global using.

The handlers' bodies use `context.UserData.FindAsync(...)`, `context.SongBestData...`, `context.SaveChangesAsync(ct)` — every one of these maps 1:1 to a member of `ITaikoDbContext`. **No body changes needed** for the handlers we already inspected (`UpdatePlayResultCommand`).

If any handler uses a `DbSet<T>` not in `ITaikoDbContext`, **stop** — either add the DbSet to `ITaikoDbContext` (rare but possible) or revisit. The 10 entities listed in §4 are the complete set today.

If any handler uses methods on the concrete `TaikoDbContext` that aren't in `ITaikoDbContext` (e.g., `context.Update(x)`, `context.Add(x)`, `context.Set<T>()`), they'll fail to compile. The fix:
- `context.Add(x)` / `context.Update(x)` / `context.Remove(x)` → call the type-safe variant: `context.UserData.Add(x)`, `context.UserData.Update(x)`, etc.
- `context.Set<T>()` → use the typed property.
- `context.AiScoreData.Local` (used by `UpdatePlayResultCommand`) → expose `Local` indirectly via the typed `DbSet` getter — `context.AiScoreData` returns `DbSet<AiScoreDatum>`, which has `.Local`. **This already works** because we're not abstracting away `DbSet<T>`; we're abstracting away `DbContext`.

- [ ] **Step 4: Application doesn't yet have the Common DTOs / Catalog VOs**

Many handler files reference `TaikoLocalServer.Models.Application.CommonX` types and `TaikoLocalServer.Models.MusicInfoEntry` etc. These move in Phase 2.4 and 2.5. After Phase 2.4–2.6, all the `using` rows in handlers should be:

- `using TaikoLocalServer.Application.Abstractions;`
- `using TaikoLocalServer.Application.Dtos;`
- `using TaikoLocalServer.Application.Catalog;`
- `using TaikoLocalServer.Application.ServerData;`
- `using TaikoLocalServer.Application.Common;`

Add a placeholder `using TaikoLocalServer.Application.Abstractions;` to each handler now; the rest follow.

- [ ] **Step 5: Build attempt**

```bash
dotnet build Application/Application.csproj
```

Expected: **fails** due to missing `Common*` types, `MusicInfoEntry` etc. Continue to Phase 2.4.

- [ ] **Step 6: Stop the partial commit here — DON'T commit yet**

Phase 2.3 will return to green at end of Phase 2.6.

### Task 2.3.2: Globally remove the `TaikoLocalServer.Handlers` global using from the host

**Files:**
- Modify: `TaikoLocalServer/GlobalUsings.cs`

- [ ] **Step 1: Update**

`TaikoLocalServer/GlobalUsings.cs` after PR1 currently reads:

```csharp
// Global using directives

global using TaikoLocalServer.Domain.Entities;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using ProtoBuf;
global using Mediator;
global using Swan.Formatters;
global using TaikoLocalServer.Domain.Enums;
global using TaikoLocalServer.Common;
global using TaikoLocalServer.Domain;
global using TaikoLocalServer.Common.Utils;
global using TaikoLocalServer.Handlers;
global using TaikoLocalServer.Models;
global using TaikoLocalServer.Models.Application;
global using TaikoLocalServer.Models.WW08;
global using TaikoLocalServer.Services;
global using TaikoLocalServer.Services.Interfaces;
```

Change `global using TaikoLocalServer.Handlers;` → `global using TaikoLocalServer.Application.Handlers;`. Keep the others for now.

- [ ] **Step 2: Add Application ref to TaikoLocalServer.csproj**

In `TaikoLocalServer/TaikoLocalServer.csproj`:

```xml
  <ItemGroup>
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Domain\Domain.csproj" />
    <ProjectReference Include="..\GameDatabase\GameDatabase.csproj" />
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    <ProjectReference Include="..\TaikoWebUI\TaikoWebUI.csproj" />
  </ItemGroup>
```

(`Application` first for alphabetical order; this is purely cosmetic but consistent with PR1.)

- [ ] **Step 3: Don't build yet**

Build will fail until Phase 2.6 wraps up. Continue.

---

## Phase 2.4 — Move Common* DTOs into Application/Dtos

### Task 2.4.1: Move all 15 Common*.cs files

**Files:**
- Move: `TaikoLocalServer/Models/Application/Common*.cs` → `Application/Dtos/Common*.cs` (15 files)

The 15 files (per Glob):

```
CommonAddTokenCountRequest.cs, CommonAiDataResponse.cs,
CommonAiScoreResponse.cs, CommonBaidResponse.cs,
CommonDanScoreDataResponse.cs, CommonGetFolderResponse.cs,
CommonGetShopFolderResponse.cs, CommonGetSongIntroductionResponse.cs,
CommonGetTokenCountResponse.cs, CommonInitialDataCheckResponse.cs,
CommonMyDonEntryResponse.cs, CommonPlayResultData.cs,
CommonSelfBestResponse.cs, CommonSongPurchaseResponse.cs,
CommonUserDataResponse.cs
```

- [ ] **Step 1: Move**

```bash
git mv TaikoLocalServer/Models/Application/CommonAddTokenCountRequest.cs           Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonAiDataResponse.cs                 Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonAiScoreResponse.cs                Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonBaidResponse.cs                   Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonDanScoreDataResponse.cs           Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonGetFolderResponse.cs              Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonGetShopFolderResponse.cs          Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonGetSongIntroductionResponse.cs    Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonGetTokenCountResponse.cs          Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonInitialDataCheckResponse.cs       Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonMyDonEntryResponse.cs             Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonPlayResultData.cs                 Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonSelfBestResponse.cs               Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonSongPurchaseResponse.cs           Application/Dtos/
git mv TaikoLocalServer/Models/Application/CommonUserDataResponse.cs               Application/Dtos/
```

The `TaikoLocalServer/Models/Application/` folder is now empty. Remove it:

```bash
rmdir TaikoLocalServer/Models/Application
```

(`git rm -r` is unnecessary; `git mv` already removed tracked files.)

- [ ] **Step 2: Update namespaces**

In each `Application/Dtos/Common*.cs`, change:

```csharp
namespace TaikoLocalServer.Models.Application;
```

to:

```csharp
namespace TaikoLocalServer.Application.Dtos;
```

- [ ] **Step 3: Update consumers**

Find:

```bash
grep -rln "TaikoLocalServer\.Models\.Application" TaikoLocalServer Application Domain SharedProject TaikoWebUI
```

Expected matches: handler files (already moved to Application/Handlers/), mapper files (TaikoLocalServer/Mappers/), controller files (TaikoLocalServer/Controllers/Game/), `TaikoLocalServer/GlobalUsings.cs`.

For each, replace:

- `using TaikoLocalServer.Models.Application;` → `using TaikoLocalServer.Application.Dtos;`
- `TaikoLocalServer.Models.Application.X` → `TaikoLocalServer.Application.Dtos.X`

In `TaikoLocalServer/GlobalUsings.cs`, change:

```csharp
global using TaikoLocalServer.Models.Application;
```

to:

```csharp
global using TaikoLocalServer.Application.Dtos;
```

- [ ] **Step 4: Build Application alone**

```bash
dotnet build Application/Application.csproj
```

Expected: still failing on Catalog/ServerData types — that's Phase 2.5/2.6.

---

## Phase 2.5 — Move Catalog read-side value objects into Application/Catalog

### Task 2.5.1: Move 12 catalog VO files

**Files:**
- Move: `TaikoLocalServer/Models/{MusicInfoEntry, MusicOrderEntry, NeiroEntry, ShougouEntry, WordListEntry, DonCosRewardEntry, DonCosRewards, MusicInfos, MusicOrder, Neiros, Shougous, WordList}.cs` → `Application/Catalog/`

Per Glob, the bare-Models files (not under cn_r00/ww_r08) are:

```
DonCosRewardEntry.cs, DonCosRewards.cs, MusicInfoEntry.cs, MusicInfos.cs,
MusicOrder.cs, MusicOrderEntry.cs, NeiroEntry.cs, Neiros.cs,
ShougouEntry.cs, Shougous.cs, WordList.cs, WordListEntry.cs
```

Also in `TaikoLocalServer/Models/`: `MuchaBoardAuthRequest.cs`, `MuchaUpdateCheckRequest.cs`, `PowerOnRequest.cs` — these are AllnetMucha wire types; **leave them** (PR3 moves them).

- [ ] **Step 1: Move the 12 catalog files**

```bash
git mv TaikoLocalServer/Models/DonCosRewardEntry.cs   Application/Catalog/
git mv TaikoLocalServer/Models/DonCosRewards.cs       Application/Catalog/
git mv TaikoLocalServer/Models/MusicInfoEntry.cs      Application/Catalog/
git mv TaikoLocalServer/Models/MusicInfos.cs          Application/Catalog/
git mv TaikoLocalServer/Models/MusicOrder.cs          Application/Catalog/
git mv TaikoLocalServer/Models/MusicOrderEntry.cs     Application/Catalog/
git mv TaikoLocalServer/Models/NeiroEntry.cs          Application/Catalog/
git mv TaikoLocalServer/Models/Neiros.cs              Application/Catalog/
git mv TaikoLocalServer/Models/ShougouEntry.cs        Application/Catalog/
git mv TaikoLocalServer/Models/Shougous.cs            Application/Catalog/
git mv TaikoLocalServer/Models/WordList.cs            Application/Catalog/
git mv TaikoLocalServer/Models/WordListEntry.cs       Application/Catalog/
```

- [ ] **Step 2: Update namespaces**

In each `Application/Catalog/*.cs`, change:

```csharp
namespace TaikoLocalServer.Models;
```

to:

```csharp
namespace TaikoLocalServer.Application.Catalog;
```

- [ ] **Step 3: Update consumers**

Find:

```bash
grep -rln "using TaikoLocalServer\.Models;" TaikoLocalServer Application Domain SharedProject TaikoWebUI
```

For each consumer that needs the catalog types, replace `using TaikoLocalServer.Models;` with `using TaikoLocalServer.Application.Catalog;`. (This is approximate — some files using `TaikoLocalServer.Models` may use the AllnetMucha wire types from the same folder; in that case both `using` rows are needed during PR2, and PR3 cleans up.)

In `TaikoLocalServer/GlobalUsings.cs`, change:

```csharp
global using TaikoLocalServer.Models;
```

to:

```csharp
global using TaikoLocalServer.Application.Catalog;
global using TaikoLocalServer.Models;  // keep until PR3 — still used for Mucha wire types
```

- [ ] **Step 4: Build Application alone**

```bash
dotnet build Application/Application.csproj
```

Expected: still failing on ServerData types (next phase).

---

## Phase 2.6 — Move ServerData strongly-typed shapes into Application/ServerData

### Task 2.6.1: Move 8 ServerData files

**Files:**
- Move: `SharedProject/Models/{DanData, EventFolderData, MovieData, QRCodeData, ShopFolderData, IVerupNo, MusicDetail, SongIntroductionData}.cs` → `Application/ServerData/`
- Also: where do `TokenData` and `GaidenData` live currently? They aren't in the Glob output — search.

- [ ] **Step 1: Locate `TokenData` and `GaidenData`**

```bash
grep -rln "class TokenData" TaikoLocalServer SharedProject Application Domain
grep -rln "class GaidenData" TaikoLocalServer SharedProject Application Domain
```

If they live in `SharedProject/Models/`, add them to the move list. If they don't exist as standalone files (they may be defined inline elsewhere), record their current home and update the move list.

For the planning purpose, assume both live in `SharedProject/Models/TokenData.cs` and `SharedProject/Models/GaidenData.cs` — verify with `Glob SharedProject/Models/*.cs` (already done; their absence in the earlier listing means they are NOT in standalone files in SharedProject/Models). Search `TaikoLocalServer/Models/` and `TaikoLocalServer/Common/`:

```bash
grep -rln "class TokenData\|class GaidenData" TaikoLocalServer
```

If found inside e.g. `TaikoLocalServer/Common/Utils/`, treat them as `Application/ServerData/` candidates and add to the move list.

- [ ] **Step 2: Move the SharedProject server-data files**

```bash
git mv SharedProject/Models/DanData.cs                Application/ServerData/
git mv SharedProject/Models/EventFolderData.cs        Application/ServerData/
git mv SharedProject/Models/MovieData.cs              Application/ServerData/
git mv SharedProject/Models/QRCodeData.cs             Application/ServerData/
git mv SharedProject/Models/ShopFolderData.cs         Application/ServerData/
git mv SharedProject/Models/IVerupNo.cs               Application/ServerData/
git mv SharedProject/Models/MusicDetail.cs            Application/ServerData/
git mv SharedProject/Models/SongIntroductionData.cs   Application/ServerData/
```

If Step 1 found `TokenData.cs` / `GaidenData.cs` elsewhere, `git mv` them too.

- [ ] **Step 3: Update namespaces**

In each `Application/ServerData/*.cs`, change `namespace SharedProject.Models;` to `namespace TaikoLocalServer.Application.ServerData;`.

- [ ] **Step 4: Update consumers**

```bash
grep -rln "SharedProject\.Models" TaikoLocalServer Application Domain SharedProject TaikoWebUI
```

For consumers of the moved types only, replace `using SharedProject.Models;` with `using TaikoLocalServer.Application.ServerData;` — but **only when that file uses the moved subset.** If a file also uses `SharedProject.Models.User`, `SharedProject.Models.UserCredential`, `SharedProject.Models.PlaySetting` etc., keep `using SharedProject.Models;` AND add `using TaikoLocalServer.Application.ServerData;`.

The moved types: `DanData`, `EventFolderData`, `MovieData`, `QRCodeData`, `ShopFolderData`, `IVerupNo`, `MusicDetail`, `SongIntroductionData` (+ TokenData/GaidenData if found in Step 1).

What stays in `SharedProject.Models` (PR4 splits these):

```
AiSectionBestData.cs, Costume.cs, DanBestData.cs, DanBestStageData.cs,
PlaySetting.cs, SongBestData.cs, SongHistoryData.cs, SongLeaderboard.cs,
SongPlayDatumDto.cs, Title.cs, User.cs, UserCredential.cs, UserSetting.cs,
+ Requests/ + Responses/
```

- [ ] **Step 5: Restore IGameDataCatalog members**

Now that Catalog and ServerData types exist, expand `Application/Abstractions/IGameDataCatalog.cs` to the full surface from Task 2.2.2 Step 1.

- [ ] **Step 6: Add Application's GlobalUsings entries**

Edit `Application/GlobalUsings.cs`:

```csharp
// Global using directives

global using Mediator;
global using Microsoft.EntityFrameworkCore;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Application.Catalog;
global using TaikoLocalServer.Application.Common;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.ServerData;
global using TaikoLocalServer.Domain;
global using TaikoLocalServer.Domain.Entities;
global using TaikoLocalServer.Domain.Enums;
global using Throw;
```

(`TaikoLocalServer.Application.Common` is empty for now — populated in Phase 2.7.)

- [ ] **Step 7: Build Application**

```bash
dotnet build Application/Application.csproj
```

Expected: **success**. If errors persist:
- Common DTOs reference enums/entities → confirm Domain ref is healthy (`dotnet build Domain/Domain.csproj`).
- Handlers reference `TaikoLocalServer.Common.Utils.X` types not yet in Application — Phase 2.7 fixes.

If a handler still fails for type-not-found on `Constants.DateTimeFormat` or similar, **temporarily** add `using TaikoLocalServer.Common;` at the top — the constant moves into Application/Common in Phase 2.7.

- [ ] **Step 8: Build the whole solution**

```bash
dotnet build
```

Expected: TaikoLocalServer fails because `using TaikoLocalServer.Handlers;` no longer resolves (handlers moved). Already fixed by Task 2.3.2 Step 1's `global using TaikoLocalServer.Application.Handlers;`. If TaikoLocalServer fails for a different reason, inspect the error and fix the offending `using` row in TaikoLocalServer source.

- [ ] **Step 9: Commit**

```bash
git add Application TaikoLocalServer/Models TaikoLocalServer/GlobalUsings.cs SharedProject/Models
git commit -m "PR2.3-2.6: move handlers + DTOs + Catalog VOs + ServerData shapes into TaikoLocalServer.Application"
```

---

## Phase 2.7 — Move Common utilities into Application/Common

### Task 2.7.1: Move FlagCalculator, Extensions, OrderedSet, ValueHelpers + DateTimeFormat constant

**Files:**
- Move: `TaikoLocalServer/Common/Utils/FlagCalculator.cs` → `Application/Common/FlagCalculator.cs`
- Move: `TaikoLocalServer/Common/Utils/Extensions.cs` → `Application/Common/Extensions.cs`
- Move: `TaikoLocalServer/Common/OrderedSet.cs` → `Application/Common/OrderedSet.cs`
- Move: `SharedProject/Utils/ValueHelpers.cs` → `Application/Common/ValueHelpers.cs`
- Create: `Application/Common/Constants.cs` (with only `DateTimeFormat`)
- Modify: `TaikoLocalServer/Common/Constants.cs` (remove `DateTimeFormat`)

- [ ] **Step 1: Move utilities**

```bash
git mv TaikoLocalServer/Common/Utils/FlagCalculator.cs   Application/Common/
git mv TaikoLocalServer/Common/Utils/Extensions.cs       Application/Common/
git mv TaikoLocalServer/Common/OrderedSet.cs             Application/Common/
git mv SharedProject/Utils/ValueHelpers.cs               Application/Common/
```

- [ ] **Step 2: Update namespaces**

In each:

- `FlagCalculator.cs` and `Extensions.cs`: `namespace TaikoLocalServer.Common.Utils;` → `namespace TaikoLocalServer.Application.Common;`
- `OrderedSet.cs`: `namespace TaikoLocalServer.Common;` → `namespace TaikoLocalServer.Application.Common;`
- `ValueHelpers.cs`: `namespace SharedProject.Utils;` → `namespace TaikoLocalServer.Application.Common;`

- [ ] **Step 3: Create Application/Common/Constants.cs**

```csharp
namespace TaikoLocalServer.Application.Common;

public static class Constants
{
    public const string DateTimeFormat = "yyyyMMddHHmmss";
}
```

- [ ] **Step 4: Trim TaikoLocalServer/Common/Constants.cs**

After this, the file should contain only the basenames + `DefaultDbName` (the latter moves in Phase 2.10):

```csharp
namespace TaikoLocalServer.Common;

public static class Constants
{
	public const string DefaultDbName = "taiko.db3";

	public const string MusicInfoBaseName = "musicinfo";
	public const string WordlistBaseName = "wordlist";
	public const string MusicOrderBaseName = "music_order";
	public const string DonCosRewardBaseName = "don_cos_reward";
	public const string ShougouBaseName = "shougou";
	public const string NeiroBaseName = "neiro";
}
```

- [ ] **Step 5: Repoint consumers**

```bash
grep -rln "Constants\.DateTimeFormat" TaikoLocalServer Application
```

For matches in TaikoLocalServer/, change `using TaikoLocalServer.Common;` to `using TaikoLocalServer.Application.Common;` for the constants reference. **Note:** the file may still need `using TaikoLocalServer.Common;` for the `*BaseName` constants. Both can coexist.

- [ ] **Step 6: Repoint consumers of moved utilities**

```bash
grep -rln "TaikoLocalServer\.Common\.Utils" TaikoLocalServer Application
grep -rln "SharedProject\.Utils" TaikoLocalServer Application SharedProject TaikoWebUI
```

For each file using `FlagCalculator`, `Extensions`, `OrderedSet`, or `ValueHelpers`, change the `using` from the old namespace to `using TaikoLocalServer.Application.Common;`.

- [ ] **Step 7: Build**

```bash
dotnet build
```

Expected: **mostly green for Application; TaikoLocalServer may still have residual errors** because more service-side moves haven't happened yet. If errors are confined to wrapper services (which are deleted in Phase 2.16 anyway), proceed. If errors are in controllers, fix the offending `using`s.

- [ ] **Step 8: Commit**

```bash
git add Application/Common TaikoLocalServer/Common SharedProject/Utils TaikoLocalServer
git commit -m "PR2.7: move FlagCalculator + Extensions + OrderedSet + ValueHelpers + DateTimeFormat into Application.Common"
```

---

## Phase 2.8 — Settings split (ServerSettings)

`ServerSettings` currently has 4 fields: `MuchaUrl`, `GameUrl` (Allnet/Mucha), `EnableMoreSongs`, `MoreSongsSize` (catalog/Application). Per spec §6, it splits.

### Task 2.8.1: Split ServerSettings

**Files:**
- Create: `Application/Settings/ServerSettings.cs` (kept as `ServerSettings` name to keep JSON-binding key `ServerSettings` unchanged; only the 2 Application-relevant fields)
- Will be created in Phase 2.13: `Infrastructure/Settings/AllnetSettings.cs`

- [ ] **Step 1: Write Application/Settings/ServerSettings.cs**

```csharp
namespace TaikoLocalServer.Application.Settings;

public class ServerSettings
{
    public bool EnableMoreSongs { get; set; }

    public int MoreSongsSize { get; set; } = TaikoLocalServer.Domain.DomainConstants.MusicIdMaxExpanded;
}
```

(The `MoreSongsSize` default uses `DomainConstants.MusicIdMaxExpanded` instead of the older `Constants.MusicIdMaxExpanded` per Phase 1.4.)

- [ ] **Step 2: Don't delete the old TaikoLocalServer/Settings/ServerSettings.cs yet**

It's still bound by the existing `Program.cs`. The migration of binding code happens in Phase 2.18 (Program.cs update) where it's bound twice (once into Application's `ServerSettings` for `EnableMoreSongs` portion, once into Infrastructure's `AllnetSettings` for the URL portion).

- [ ] **Step 3: Don't build yet**

This phase produces an unused file; the binding wiring comes later. Continue.

---

## Phase 2.9 — Application DependencyInjection.cs

### Task 2.9.1: AddApplication() extension method

**Files:**
- Create: `Application/DependencyInjection.cs`

- [ ] **Step 1: Write the extension method**

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

**Important:** the `AddMediator` method here triggers the source generator to emit handler registration code into the **Application** assembly (where the generator package lives). The Host project also references `Mediator.SourceGenerator` (added in Phase 2.18) to ensure cross-assembly registration works — that's the Risk #2 mitigation.

- [ ] **Step 2: Build Application alone**

```bash
dotnet build Application/Application.csproj
```

Expected: success. Inspect the generated registration:

```bash
ls Application/obj/Debug/net10.0/generated/Mediator.SourceGenerator/Mediator.SourceGenerator.MediatorGenerator/
```

Expected: a `Mediator.g.cs` (or similar) file exists. **Open it in an editor** and confirm it lists every handler from `Application.Handlers/*.cs`. If empty, this is Risk #2 firing — proceed to Task 2.9.2 fallback. If populated, you're good.

### Task 2.9.2: Risk #2 fallback (if generator misses handlers)

**Only run this task if Task 2.9.1 Step 2 produced empty generator output.**

**Files:**
- Modify: `Application/DependencyInjection.cs`

- [ ] **Step 1: Switch to assembly-marker registration**

If Mediator's source generator finds zero handlers when running in Application but `AddMediator` is called from Host (in a future phase), the workaround is to call `AddMediator` from Host directly with explicit assembly markers:

Move the `AddMediator(...)` call from `Application/DependencyInjection.cs` to a comment-block in there, and instead expose:

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Mediator registration is colocated in the Host project — see Program.cs.
        // The source generator must run in the assembly that calls AddMediator;
        // moving the call to Host is the supported fallback for cross-assembly
        // discovery. Application still owns the handlers themselves.
        return services;
    }
}
```

And in Phase 2.18 (Program.cs update), call:

```csharp
builder.Services.AddMediator(opt =>
{
    opt.Namespace = "TaikoLocalServer.Application";
    opt.ServiceLifetime = ServiceLifetime.Scoped;
});
```

directly from Host. The Host already has `Mediator.SourceGenerator` referenced.

This fallback adds one line to Host but otherwise leaves the architecture intact.

### Task 2.9.3: Commit

- [ ] **Step 1: Commit**

```bash
git add Application/DependencyInjection.cs Application/Settings
git commit -m "PR2.9: AddApplication() extension method + ServerSettings split"
```

---

## Phase 2.10 — Rename GameDatabase to TaikoLocalServer.Infrastructure

This is a folder + csproj + namespace rename. The simplest reliable approach is **rename the folder, edit the csproj, do a project-wide namespace rewrite**.

### Task 2.10.1: Rename the folder and csproj

**Files:**
- Rename: `GameDatabase/` → `Infrastructure/`
- Rename: `Infrastructure/GameDatabase.csproj` → `Infrastructure/Infrastructure.csproj`

- [ ] **Step 1: Rename the directory**

```bash
git mv GameDatabase Infrastructure
git mv Infrastructure/GameDatabase.csproj Infrastructure/Infrastructure.csproj
```

- [ ] **Step 2: Update Infrastructure.csproj**

Replace contents of `Infrastructure/Infrastructure.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <RootNamespace>TaikoLocalServer.Infrastructure</RootNamespace>
        <AssemblyName>TaikoLocalServer.Infrastructure</AssemblyName>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\Application\Application.csproj" />
        <ProjectReference Include="..\Domain\Domain.csproj" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="BCrypt.Net-Next" />
        <PackageReference Include="EntityFrameworkCore.Exceptions.Sqlite" />
        <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
        <PackageReference Include="Microsoft.EntityFrameworkCore" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include="SharpZipLib" />
        <PackageReference Include="System.IdentityModel.Tokens.Jwt" />
        <PackageReference Include="Throw" />
        <PackageReference Include="Yoh.Text.Json.NamingPolicies" />
    </ItemGroup>

</Project>
```

(Note: `System.IdentityModel.Tokens.Jwt` may need to be added to `Directory.Packages.props` — check; if not present, add `<PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="8.3.0" />` near the WebUI block. Per current packages.props readout, it's already there.)

- [ ] **Step 3: Update solution and downstream csprojs**

`TaikoLocalServer.slnx`:

```xml
<Solution>
  <Project Path="Application/Application.csproj" />
  <Project Path="Domain/Domain.csproj" />
  <Project Path="Infrastructure/Infrastructure.csproj" />
  <Project Path="LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj" />
  <Project Path="SharedProject/SharedProject.csproj" />
  <Project Path="TaikoLocalServer/TaikoLocalServer.csproj" />
  <Project Path="TaikoWebUI/TaikoWebUI.csproj" />
</Solution>
```

`TaikoLocalServer/TaikoLocalServer.csproj`:

```xml
  <ItemGroup>
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Domain\Domain.csproj" />
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    <ProjectReference Include="..\TaikoWebUI\TaikoWebUI.csproj" />
  </ItemGroup>
```

`LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`:

Replace `..\GameDatabase\GameDatabase.csproj` with `..\Infrastructure\Infrastructure.csproj`.

- [ ] **Step 4: Project-wide namespace rewrite**

```bash
# bash:
grep -rl "GameDatabase\.Context" TaikoLocalServer Infrastructure Application Domain SharedProject TaikoWebUI LocalSaveModScoreMigrator | \
  xargs sed -i 's/GameDatabase\.Context/TaikoLocalServer.Infrastructure.Persistence/g'

# Inside Infrastructure files only, also rewrite the namespace declarations:
find Infrastructure -name '*.cs' | xargs sed -i 's/^namespace GameDatabase\.Context;/namespace TaikoLocalServer.Infrastructure.Persistence;/'
find Infrastructure -name '*.cs' | xargs sed -i 's/^namespace GameDatabase\.Migrations;/namespace TaikoLocalServer.Infrastructure.Persistence.Migrations;/'
find Infrastructure -name '*.cs' | xargs sed -i 's/^namespace GameDatabase;/namespace TaikoLocalServer.Infrastructure;/'
```

(PowerShell equivalent: use `Get-ChildItem ... | ForEach-Object { (Get-Content $_) -replace ... | Set-Content $_ }`.)

The Migrations folder also has metadata strings like `"GameDatabase.Context.TaikoDbContext"` inside `[DbContext(typeof(...))]` attributes — those were targeted by the Step 4 sed. Verify with a follow-up grep:

```bash
grep -rln "GameDatabase" Infrastructure
```

Expected: zero matches. If matches remain, manually edit (likely `.Designer.cs` files have `[DbContext(typeof(GameDatabase.Context.TaikoDbContext))]`).

Also rename the files:

```bash
git mv Infrastructure/Context Infrastructure/Persistence
```

(The `Migrations/` subfolder stays directly under `Infrastructure/`; per spec §3, it lives at `Infrastructure/Persistence/Migrations/`. Move it:)

```bash
git mv Infrastructure/Migrations Infrastructure/Persistence/Migrations
```

(So the layout is `Infrastructure/Persistence/{TaikoDbContext.cs, TaikoDbContextPartial.cs, Migrations/}`.)

- [ ] **Step 5: Build Infrastructure alone**

```bash
dotnet build Infrastructure/Infrastructure.csproj
```

Expected: success. If errors are namespace-related, repeat the sed; if errors are about EF Core types, leave as-is (we're about to add `: ITaikoDbContext` in Phase 2.11).

- [ ] **Step 6: Build solution**

```bash
dotnet build
```

Expected: TaikoLocalServer fails because `using GameDatabase.Context;` is gone. Sweep:

```bash
grep -rln "GameDatabase\.Context\|GameDatabase\.Entities\|using GameDatabase;" \
  TaikoLocalServer Application SharedProject TaikoWebUI LocalSaveModScoreMigrator
```

Replace each with the new namespace. `using GameDatabase.Context;` → `using TaikoLocalServer.Infrastructure.Persistence;`.

- [ ] **Step 7: Build solution again**

```bash
dotnet build
```

Expected: success (modulo wrapper service errors in TaikoLocalServer that we'll handle in Phase 2.16).

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "PR2.10: rename GameDatabase project to TaikoLocalServer.Infrastructure"
```

---

## Phase 2.11 — Implement TaikoDbContext : ITaikoDbContext

### Task 2.11.1: Make TaikoDbContext implement ITaikoDbContext

**Files:**
- Modify: `Infrastructure/Persistence/TaikoDbContext.cs`
- Modify: `Infrastructure/Persistence/TaikoDbContextPartial.cs`

- [ ] **Step 1: Read both files**

```bash
cat Infrastructure/Persistence/TaikoDbContext.cs
cat Infrastructure/Persistence/TaikoDbContextPartial.cs
```

Confirm `TaikoDbContext` declares `: DbContext`. The class is partial across these two files.

- [ ] **Step 2: Add the interface to the class declaration**

In whichever of the two files declares `public partial class TaikoDbContext : DbContext`, change to:

```csharp
public partial class TaikoDbContext : DbContext, ITaikoDbContext
```

Add `using TaikoLocalServer.Application.Abstractions;` to the top of that file.

- [ ] **Step 3: Build Infrastructure**

```bash
dotnet build Infrastructure/Infrastructure.csproj
```

Expected: success. The DbContext already exposes the 10 `DbSet<>` properties needed by the interface; `SaveChangesAsync` is on the base `DbContext` — both satisfy the interface.

- [ ] **Step 4: Commit**

```bash
git add Infrastructure/Persistence
git commit -m "PR2.11: TaikoDbContext implements ITaikoDbContext"
```

---

## Phase 2.12 — Carve JwtTokenService and add SystemClock

### Task 2.12.1: Create Infrastructure/Identity/JwtTokenService.cs

**Files:**
- Create: `Infrastructure/Identity/JwtTokenService.cs`
- Create: `Infrastructure/Identity/Settings/AuthSettings.cs` (moved from TaikoLocalServer/Settings/)

- [ ] **Step 1: Create Identity directory**

```bash
mkdir -p Infrastructure/Identity/Settings
```

- [ ] **Step 2: Move AuthSettings**

```bash
git mv TaikoLocalServer/Settings/AuthSettings.cs Infrastructure/Identity/Settings/AuthSettings.cs
```

Edit `Infrastructure/Identity/Settings/AuthSettings.cs`: change `namespace TaikoLocalServer.Settings;` → `namespace TaikoLocalServer.Infrastructure.Identity.Settings;`.

- [ ] **Step 3: Write JwtTokenService.cs**

The two methods come from:

- `IssueToken` — currently inline in `TaikoLocalServer/Controllers/Api/AuthController.cs` login flow (search for `JwtSecurityTokenHandler` + `WriteToken` calls).
- `ExtractTokenInfo` — the body of `AuthService.ExtractTokenInfo(HttpContext)` (lines 190–230 of `TaikoLocalServer/Services/AuthService.cs`).

Open `TaikoLocalServer/Controllers/Api/AuthController.cs` and locate the `Login` action's token-creation section. The pattern is approximately:

```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(authSettings.JwtKey ?? throw new InvalidOperationException());
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, baid.ToString()),
        new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
    }),
    Expires = DateTime.UtcNow.AddDays(1), // OR similar — copy whatever AuthController uses
    Issuer = authSettings.JwtIssuer,
    Audience = authSettings.JwtAudience,
    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
};
var token = tokenHandler.CreateToken(tokenDescriptor);
return tokenHandler.WriteToken(token);
```

Lift this verbatim into the new method. Write the file:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Infrastructure.Identity;

public class JwtTokenService(IOptions<AuthSettings> options) : IJwtTokenService
{
    private readonly AuthSettings authSettings = options.Value;

    public string IssueToken(uint baid, bool isAdmin)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(authSettings.JwtKey ?? throw new InvalidOperationException());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, baid.ToString()),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = authSettings.JwtIssuer,
            Audience = authSettings.JwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public JwtTokenInfo? ExtractTokenInfo(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return null;
        }

        var jwtToken = handler.ReadJwtToken(token);
        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            return null;
        }

        var claimBaid = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var claimRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (claimBaid == null || claimRole == null)
        {
            return null;
        }

        if (!uint.TryParse(claimBaid, out var baid))
        {
            return null;
        }

        var isAdmin = claimRole == "Admin";
        return new JwtTokenInfo(baid, isAdmin);
    }
}
```

**Verify your `Expires`** matches what `AuthController.Login` uses. Common values: `AddDays(1)`, `AddHours(8)`, etc. Copy verbatim from the controller.

The Expires copy is the highest-risk diff — getting it wrong silently changes session lifetime. Spec §8 Risk #3 calls this out.

- [ ] **Step 4: Build Infrastructure**

```bash
dotnet build Infrastructure/Infrastructure.csproj
```

Expected: success. If `IJwtTokenService` symbol unresolved, double-check `using TaikoLocalServer.Application.Abstractions;` and that `Infrastructure.csproj` has `<ProjectReference Include="..\Application\Application.csproj" />`.

### Task 2.12.2: SystemClock

**Files:**
- Create: `Infrastructure/Time/SystemClock.cs`

- [ ] **Step 1: Create directory and file**

```bash
mkdir Infrastructure/Time
```

```csharp
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Infrastructure.Time;

public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Infrastructure/Infrastructure.csproj
```

Expected: success.

### Task 2.12.3: AllnetSettings

**Files:**
- Create: `Infrastructure/Settings/AllnetSettings.cs`

- [ ] **Step 1: Create directory and file**

```bash
mkdir Infrastructure/Settings
```

```csharp
namespace TaikoLocalServer.Infrastructure.Settings;

public class AllnetSettings
{
    public string MuchaUrl { get; set; } = string.Empty;

    public string GameUrl { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Infrastructure/Infrastructure.csproj
```

Expected: success.

- [ ] **Step 3: Commit**

```bash
git add Infrastructure
git commit -m "PR2.12: JwtTokenService + SystemClock + AllnetSettings"
```

---

## Phase 2.13 — Split GameDataService into FileGameDataCatalog + Loaders

This is the single largest LoC move in PR2. The current 566-line `TaikoLocalServer/Services/GameDataService.cs` becomes:

- `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs` — orchestrator implementing `IGameDataCatalog`. Stores aggregated dictionaries; calls each loader in `InitializeAsync`.
- `Infrastructure/GameDataCatalog/Loaders/*.cs` — one loader per source datatable. Each loader: takes a path + filename, returns a `Dictionary<...>` or `List<...>` ready to be read.
- `Infrastructure/GameDataCatalog/PathHelper.cs` — moved from `SharedProject/Utils/`.
- `Infrastructure/GameDataCatalog/CatalogConstants.cs` — `*BaseName` constants.
- `Infrastructure/GameDataCatalog/Settings/DataSettings.cs` — moved from `TaikoLocalServer/Settings/`.

### Task 2.13.1: Read GameDataService.cs end-to-end

**Files:** none.

- [ ] **Step 1: Read it**

```bash
cat TaikoLocalServer/Services/GameDataService.cs
```

Identify:

- Constructor — what does it inject? (Likely `IConfiguration`, `IOptions<DataSettings>`, `ILogger<GameDataService>`.)
- `InitializeAsync` — the master entry point. Each datatable gets loaded sequentially.
- Per-table loader sections — typically a private method per `*BaseName` table or per JSON file (musicinfo, music_order, neiro, shougou, wordlist, don_cos_reward, dan_data, event_folder_data, movie_data, shop_folder_data, intro_data, qrcode_data, token_data, gaiden_data, locked_songs, locked_costume, locked_title, special_songs).
- Public read-only state — the `IGameDataCatalog` surface.

Make a list of each private loader method and what it returns.

### Task 2.13.2: Create the GameDataCatalog folder skeleton

**Files:**
- Create: `Infrastructure/GameDataCatalog/Loaders/` (directory)
- Create: `Infrastructure/GameDataCatalog/Settings/` (directory)
- Move: `SharedProject/Utils/PathHelper.cs` → `Infrastructure/GameDataCatalog/PathHelper.cs`
- Move: `TaikoLocalServer/Settings/DataSettings.cs` → `Infrastructure/GameDataCatalog/Settings/DataSettings.cs`
- Create: `Infrastructure/GameDataCatalog/CatalogConstants.cs`

- [ ] **Step 1: Move PathHelper**

```bash
git mv SharedProject/Utils/PathHelper.cs Infrastructure/GameDataCatalog/PathHelper.cs
```

Edit: change `namespace SharedProject.Utils;` → `namespace TaikoLocalServer.Infrastructure.GameDataCatalog;`.

- [ ] **Step 2: Move DataSettings**

```bash
git mv TaikoLocalServer/Settings/DataSettings.cs Infrastructure/GameDataCatalog/Settings/DataSettings.cs
```

Edit: change `namespace TaikoLocalServer.Settings;` → `namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Settings;`.

- [ ] **Step 3: Create CatalogConstants.cs**

```csharp
namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

public static class CatalogConstants
{
    public const string MusicInfoBaseName = "musicinfo";
    public const string WordlistBaseName = "wordlist";
    public const string MusicOrderBaseName = "music_order";
    public const string DonCosRewardBaseName = "don_cos_reward";
    public const string ShougouBaseName = "shougou";
    public const string NeiroBaseName = "neiro";
}
```

- [ ] **Step 4: Trim TaikoLocalServer/Common/Constants.cs to just `DefaultDbName`**

```csharp
namespace TaikoLocalServer.Common;

public static class Constants
{
	public const string DefaultDbName = "taiko.db3";
}
```

(`DefaultDbName` moves to Infrastructure/Persistence/PersistenceConstants.cs in Phase 2.14.)

- [ ] **Step 5: Build attempt**

```bash
dotnet build
```

Expected: errors about `Constants.MusicInfoBaseName` etc. in GameDataService. We're about to delete that file in next steps; ignore for now if errors confined there.

### Task 2.13.3: Carve out the orchestrator + loaders

**Files:**
- Create: `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`
- Create: per-table loader files (count varies — see spec §3 for the 18-file list).
- Delete (later): `TaikoLocalServer/Services/GameDataService.cs`

> **Approach:** the cleanest path is incremental. **Copy** GameDataService.cs into FileGameDataCatalog.cs (same logic, new namespace, implementing `IGameDataCatalog` not `IGameDataService`). Get it building. Then **carve loaders out one by one** in subsequent tasks, replacing the inline private methods with calls to standalone loader classes. This keeps each commit small.

- [ ] **Step 1: Copy GameDataService.cs to FileGameDataCatalog.cs**

```bash
cp TaikoLocalServer/Services/GameDataService.cs Infrastructure/GameDataCatalog/FileGameDataCatalog.cs
```

- [ ] **Step 2: Modify FileGameDataCatalog.cs**

Open `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`. Edit:

- Line: `namespace TaikoLocalServer.Services;` → `namespace TaikoLocalServer.Infrastructure.GameDataCatalog;`.
- Class declaration: `public class GameDataService : IGameDataService` → `public class FileGameDataCatalog : IGameDataCatalog`.
- Constructor name: `GameDataService(...)` → `FileGameDataCatalog(...)`.
- Add `using` for the new namespaces:
  - `using TaikoLocalServer.Application.Abstractions;`
  - `using TaikoLocalServer.Application.Catalog;`
  - `using TaikoLocalServer.Application.ServerData;`
  - `using TaikoLocalServer.Infrastructure.GameDataCatalog.Settings;`
- Replace `Constants.MusicInfoBaseName` → `CatalogConstants.MusicInfoBaseName` (and same for the other five `*BaseName` constants). If `using TaikoLocalServer.Common;` was the only source for them, remove it.
- Replace `PathHelper.GetRootPath()` references — these resolve via the same `using TaikoLocalServer.Infrastructure.GameDataCatalog;` namespace.

- [ ] **Step 3: Build Infrastructure**

```bash
dotnet build Infrastructure/Infrastructure.csproj
```

Iterate until clean. Expected friction:
- Missing `using` rows for moved types (Catalog VOs, ServerData shapes, DataSettings) → add.
- Lingering references to `IGameDataService` in `FileGameDataCatalog.cs` → replace with `IGameDataCatalog`.
- Loader logic that reads JSON: keep `using Swan.Formatters;` if needed (though Swan is for the Stringify pattern in controllers — verify before removing).

- [ ] **Step 4: Delete GameDataService.cs (and its interface)**

```bash
git rm TaikoLocalServer/Services/GameDataService.cs
git rm TaikoLocalServer/Services/Interfaces/IGameDataService.cs
```

- [ ] **Step 5: Repoint consumers from `IGameDataService` → `IGameDataCatalog`**

```bash
grep -rln "IGameDataService" TaikoLocalServer Application Infrastructure
```

For each match, change `IGameDataService` → `IGameDataCatalog`. Add `using TaikoLocalServer.Application.Abstractions;` if needed.

In `TaikoLocalServer/Program.cs`:
- `builder.Services.AddSingleton<IGameDataService, GameDataService>();` → leave for Phase 2.14 (registration moves to AddInfrastructure).
- `var gameDataService = app.Services.GetService<IGameDataService>();` → `var gameDataCatalog = app.Services.GetService<IGameDataCatalog>();`
- `await gameDataService.InitializeAsync();` → `await gameDataCatalog.InitializeAsync();`
- Update the local var name to match.

- [ ] **Step 6: Build the solution**

```bash
dotnet build
```

Expected: success aside from wrapper-service residuals (deleted in Phase 2.16).

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "PR2.13.3: rename GameDataService -> FileGameDataCatalog (orchestrator only; loaders not yet split)"
```

### Task 2.13.4: Carve out per-table loaders

This task is **optional in PR2** — the spec calls for an 18-file `Loaders/` split, but the orchestrator works without that split. **For PR2, the structural goal is "Infrastructure owns IGameDataCatalog implementation."** A future cleanup PR can break out loaders.

If you want to proceed in PR2 anyway:

- [ ] **Step 1: For each private load-method in FileGameDataCatalog.cs, extract to a standalone class**

Pattern:

```csharp
// FileGameDataCatalog.cs (before)
private async Task<Dictionary<uint, MusicInfoEntry>> LoadMusicInfo()
{
    var path = Path.Combine(PathHelper.GetRootPath(), "data", "datatable", $"{CatalogConstants.MusicInfoBaseName}.bin");
    // ... load + decode ...
    return result;
}

// FileGameDataCatalog.cs (after)
private readonly MusicInfoLoader musicInfoLoader = new();
// In InitializeAsync:
this.MusicInfoes = await musicInfoLoader.LoadAsync(cancellationToken);

// Loaders/MusicInfoLoader.cs (new)
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Loaders;

public class MusicInfoLoader
{
    public async Task<Dictionary<uint, MusicInfoEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetRootPath(), "data", "datatable", $"{CatalogConstants.MusicInfoBaseName}.bin");
        // ... load + decode ...
        return result;
    }
}
```

The full set of 18 loaders (per spec §3):

```
MusicInfoLoader, MusicOrderLoader, NeiroLoader, ShougouLoader,
WordlistLoader, DonCosRewardLoader, DanDataLoader, EventFolderDataLoader,
MovieDataLoader, ShopFolderDataLoader, IntroDataLoader, QrCodeDataLoader,
TokenDataLoader, GaidenDataLoader, LockedSongsLoader, LockedCostumeLoader,
LockedTitleLoader, SpecialSongsLoader
```

- [ ] **Step 2: Build after each loader extraction**

```bash
dotnet build Infrastructure/Infrastructure.csproj
```

- [ ] **Step 3: Commit per loader (or batch)**

```bash
git add Infrastructure/GameDataCatalog
git commit -m "PR2.13.4: extract MusicInfoLoader (and ${others})"
```

If this work doesn't fit comfortably in PR2's scope, **defer**. Add a TODO note in `FileGameDataCatalog.cs`:

```csharp
// TODO: extract per-table loaders into Loaders/*.cs; tracked in follow-up.
```

and proceed to Phase 2.14. The spec acknowledges the split as enabled-by-this-refactor, not delivered-by-it.

---

## Phase 2.14 — PersistenceConstants and Infrastructure DependencyInjection

### Task 2.14.1: Persistence constants

**Files:**
- Create: `Infrastructure/Persistence/PersistenceConstants.cs`
- Modify: `TaikoLocalServer/Common/Constants.cs` (delete the file if empty)

- [ ] **Step 1: Write PersistenceConstants.cs**

```csharp
namespace TaikoLocalServer.Infrastructure.Persistence;

public static class PersistenceConstants
{
    public const string DefaultDbName = "taiko.db3";
}
```

- [ ] **Step 2: Repoint consumers**

```bash
grep -rln "Constants\.DefaultDbName" TaikoLocalServer Application Infrastructure
```

For each match, change `Constants.DefaultDbName` → `PersistenceConstants.DefaultDbName`. Add `using TaikoLocalServer.Infrastructure.Persistence;` to the file.

In `TaikoLocalServer/Program.cs`, the `dbName = Constants.DefaultDbName` line becomes `dbName = PersistenceConstants.DefaultDbName;`.

- [ ] **Step 3: Delete TaikoLocalServer/Common/Constants.cs (now empty)**

```bash
git rm TaikoLocalServer/Common/Constants.cs
```

If the file isn't empty (perhaps a forgotten constant), inspect — relocate or keep. After this PR the file should be gone.

Likewise:

```bash
rmdir TaikoLocalServer/Common/Utils
rmdir TaikoLocalServer/Common
```

(Only if directories are empty — `git status` will tell you.)

- [ ] **Step 4: Update TaikoLocalServer/GlobalUsings.cs**

Remove the obsolete:
- `global using TaikoLocalServer.Common;`
- `global using TaikoLocalServer.Common.Utils;`

The current state should be roughly:

```csharp
// Global using directives

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using ProtoBuf;
global using Mediator;
global using Swan.Formatters;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Application.Catalog;
global using TaikoLocalServer.Application.Common;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Handlers;
global using TaikoLocalServer.Application.ServerData;
global using TaikoLocalServer.Domain;
global using TaikoLocalServer.Domain.Entities;
global using TaikoLocalServer.Domain.Enums;
global using TaikoLocalServer.Models;            // Mucha wire types still here pre-PR3
global using TaikoLocalServer.Models.WW08;       // ww_r08 wire types pre-PR3
global using TaikoLocalServer.Services;          // wrapper services (pre-deletion)
global using TaikoLocalServer.Services.Interfaces;
```

(Keep `Models` and `Models.WW08` until PR3.)

- [ ] **Step 5: Build solution**

```bash
dotnet build
```

Expected: clean enough to proceed (any failures should be related to wrapper services that we delete next).

### Task 2.14.2: AddInfrastructure() extension

**Files:**
- Create: `Infrastructure/DependencyInjection.cs`
- Create: `Infrastructure/GlobalUsings.cs`

- [ ] **Step 1: Write Infrastructure/GlobalUsings.cs**

```csharp
// Global using directives

global using Microsoft.EntityFrameworkCore;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Domain.Entities;
```

- [ ] **Step 2: Write Infrastructure/DependencyInjection.cs**

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Infrastructure.GameDataCatalog;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Settings;
using TaikoLocalServer.Infrastructure.Identity;
using TaikoLocalServer.Infrastructure.Identity.Settings;
using TaikoLocalServer.Infrastructure.Persistence;
using TaikoLocalServer.Infrastructure.Settings;
using TaikoLocalServer.Infrastructure.Time;

namespace TaikoLocalServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Settings
        services.Configure<AuthSettings>(configuration.GetSection(nameof(AuthSettings)));
        services.Configure<DataSettings>(configuration.GetSection(nameof(DataSettings)));
        services.Configure<ServerSettings>(configuration.GetSection(nameof(ServerSettings)));   // Application's portion
        services.Configure<AllnetSettings>(configuration.GetSection(nameof(ServerSettings)));    // Infrastructure's portion (same JSON section, different POCO)

        // Persistence
        services.AddDbContext<TaikoDbContext>(option =>
        {
            var dbName = configuration["DbFileName"];
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = PersistenceConstants.DefaultDbName;
            }

            var path = Path.Combine(PathHelper.GetRootPath(), dbName);
            option
                .UseSqlite($"Data Source={path}")
                .ConfigureWarnings(warnings => warnings
                    .Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.NonTransactionalMigrationOperationWarning)
                    .Ignore(Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal.SqliteEventId.TableRebuildPendingWarning));
        });
        services.AddScoped<ITaikoDbContext>(sp => sp.GetRequiredService<TaikoDbContext>());

        // Game data catalog (singleton — initialized once at startup)
        services.AddSingleton<IGameDataCatalog, FileGameDataCatalog>();

        // Identity
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var authSection = configuration.GetSection(nameof(AuthSettings));
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authSection["JwtIssuer"],
                ValidAudience = authSection["JwtAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSection["JwtKey"] ?? throw new InvalidOperationException()))
            };
        });

        // Time
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
```

(The exact `Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal.SqliteEventId` namespace path comes from the current Program.cs `ConfigureWarnings` block — copy verbatim.)

- [ ] **Step 3: Build Infrastructure**

```bash
dotnet build Infrastructure/Infrastructure.csproj
```

Expected: success.

- [ ] **Step 4: Commit**

```bash
git add Infrastructure
git commit -m "PR2.14: AddInfrastructure() extension method + PersistenceConstants"
```

---

## Phase 2.15 — Mediator generator sanity check (Risk #2 gate)

### Task 2.15.1: Confirm handler registrations after orchestrator wired

**Files:** none.

- [ ] **Step 1: Build Application after handlers + DI extension are in place**

```bash
dotnet build Application/Application.csproj
```

- [ ] **Step 2: Inspect generator output**

```bash
ls Application/obj/Debug/net10.0/generated/Mediator.SourceGenerator/
```

Open the generated `.g.cs` files. **Confirm every handler from `Application/Handlers/*.cs` appears** in a registration call. If 16 handlers are present (one per file as cataloged in Phase 2.3), Risk #2 is non-firing.

If 0 handlers present, **invoke Task 2.9.2 fallback** before continuing — the rewrite of Program.cs in Phase 2.18 needs to know which path Mediator takes.

If 1–15 handlers present (partial), some handler's filter doesn't match `opt.Namespace`. Inspect each missing one — namespace declared as `TaikoLocalServer.Application.Handlers`? If a handler's request type uses `record class` instead of `readonly record struct` (per CLAUDE.md convention), the generator may skip it. Match the existing handler conventions.

---

## Phase 2.16 — Rewrite admin controllers in place (no folder moves)

This is the single highest-LoC task in PR2, but it's **mechanical** — every controller follows the same template:

> **Old:** `(IUserDatumService userDatumService, IAuthService authService, ...)` → injected wrapper services.
> **New:** `(ITaikoDbContext context, IJwtTokenService jwtTokens, ...)` → port + JWT service.

The controllers stay in `TaikoLocalServer/Controllers/Api/` — only their constructor signatures and method bodies change. PR3 relocates them.

### Task 2.16.1: Read every admin controller

**Files:** none.

- [ ] **Step 1: List the 10 controllers**

Per Glob: `AuthController.cs`, `CardsController.cs`, `DanBestDataController.cs`, `FavoriteSongsController.cs`, `GameDataController.cs`, `PlayDataController.cs`, `PlayHistoryController.cs`, `SongLeaderboardController.cs`, `UserSettingsController.cs`, `UsersController.cs`.

- [ ] **Step 2: For each controller, document**

Make a checklist (mental or written) of which wrapper services and AuthSettings each one currently injects. Reference §4 of the spec for the new flow.

### Task 2.16.2: Rewrite UsersController

**Files:**
- Modify: `TaikoLocalServer/Controllers/Api/UsersController.cs`

The current controller (per Read in this session) injects `IUserDatumService userDatumService, IAuthService authService, IOptions<AuthSettings> settings`. The new version uses `ITaikoDbContext context, IJwtTokenService jwtTokens, IOptions<AuthSettings> settings`.

- [ ] **Step 1: Write the rewritten UsersController.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Models.Responses;
using SharedProject.Utils;
using Swan.Mapping;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IOptions<AuthSettings> settings) : BaseController<UsersController>
{
    private readonly AuthSettings authSettings = settings.Value;

    [HttpGet("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<User?> GetUser(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return null;
            }

            if (!tokenInfo.Value.IsAdmin && tokenInfo.Value.Baid != baid)
            {
                return null;
            }
        }

        var userDatum = await context.UserData.FindAsync(baid);
        if (userDatum == null) return null;

        var cardEntries = await context.Cards.Where(card => card.Baid == baid).ToListAsync();
        return new User
        {
            Baid = userDatum.Baid,
            AccessCodes = cardEntries.Select(card => card.AccessCode).ToList(),
            IsAdmin = userDatum.IsAdmin
        };
    }

    [HttpGet]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<UsersResponse>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? searchTerm = null)
    {
        if (page < 1)
        {
            return BadRequest(new { Message = "Page number cannot be less than 1." });
        }

        if (limit > 200)
        {
            return BadRequest(new { Message = "Limit cannot be greater than 200." });
        }

        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return new UsersResponse();
            }

            if (!tokenInfo.Value.IsAdmin)
            {
                return new UsersResponse();
            }
        }

        var users = new List<User>();
        var cardEntries = await context.Cards.ToListAsync();
        var userEntriesQuery = context.UserData.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerCaseSearchTerm = searchTerm.ToLower();
            userEntriesQuery = userEntriesQuery.Where(user => user.Baid.ToString() == lowerCaseSearchTerm
                || user.MyDonName.ToLower().Contains(lowerCaseSearchTerm)
                || context.Cards.Any(card => card.Baid == user.Baid && card.AccessCode.ToLower().Contains(lowerCaseSearchTerm)));
        }

        var totalUsers = await userEntriesQuery.CountAsync();
        var totalPages = totalUsers / limit;
        if (totalUsers % limit > 0)
        {
            totalPages++;
        }

        var userEntries = await userEntriesQuery
            .OrderBy(user => user.Baid)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        foreach (var user in userEntries)
        {
            List<List<uint>> costumeUnlockData =
                [user.UnlockedKigurumi, user.UnlockedHead, user.UnlockedBody, user.UnlockedFace, user.UnlockedPuchi];

            var unlockedTitle = user.TitleFlgArray.ToList();

            for (var i = 0; i < 5; i++)
            {
                if (!costumeUnlockData[i].Contains(0))
                {
                    costumeUnlockData[i].Add(0);
                }
            }

            var userSetting = new UserSetting
            {
                Baid = user.Baid,
                AchievementDisplayDifficulty = user.AchievementDisplayDifficulty,
                IsDisplayAchievement = user.DisplayAchievement,
                IsDisplayDanOnNamePlate = user.DisplayDan,
                DifficultySettingCourse = user.DifficultySettingCourse,
                DifficultySettingStar = user.DifficultySettingStar,
                DifficultySettingSort = user.DifficultySettingSort,
                IsVoiceOn = user.IsVoiceOn,
                IsSkipOn = user.IsSkipOn,
                NotesPosition = user.NotesPosition,
                PlaySetting = PlaySettingConverter.ShortToPlaySetting(user.OptionSetting),
                ToneId = user.SelectedToneId,
                MyDonName = user.MyDonName,
                MyDonNameLanguage = user.MyDonNameLanguage,
                Title = user.Title,
                TitlePlateId = user.TitlePlateId,
                Kigurumi = user.CurrentKigurumi,
                Head = user.CurrentHead,
                Body = user.CurrentBody,
                Face = user.CurrentFace,
                Puchi = user.CurrentPuchi,
                UnlockedKigurumi = costumeUnlockData[0],
                UnlockedHead = costumeUnlockData[1],
                UnlockedBody = costumeUnlockData[2],
                UnlockedFace = costumeUnlockData[3],
                UnlockedPuchi = costumeUnlockData[4],
                UnlockedTitle = unlockedTitle,
                BodyColor = user.ColorBody,
                FaceColor = user.ColorFace,
                LimbColor = user.ColorLimb,
                LastPlayDateTime = user.LastPlayDatetime
            };

            users.Add(new User
            {
                Baid = user.Baid,
                AccessCodes = cardEntries.Where(card => card.Baid == user.Baid).Select(card => card.AccessCode).ToList(),
                IsAdmin = user.IsAdmin,
                UserSetting = userSetting
            });
        }

        return new UsersResponse
        {
            Users = users,
            Page = page,
            TotalPages = totalPages,
            TotalUsers = totalUsers
        };
    }

    [HttpDelete("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> DeleteUser(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.IsAdmin && tokenInfo.Value.Baid != baid)
            {
                return Forbid();
            }
        }

        var userDatum = await context.UserData.FindAsync(baid);
        if (userDatum == null) return NotFound();

        context.UserData.Remove(userDatum);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
```

Note: `tokenInfo.Value.IsAdmin` (PascalCase) replaces `tokenInfo.Value.isAdmin` (lowercase) because we're using the new `JwtTokenInfo` record's properties.

- [ ] **Step 2: Build**

```bash
dotnet build TaikoLocalServer/TaikoLocalServer.csproj
```

Iterate.

- [ ] **Step 3: Commit**

```bash
git add TaikoLocalServer/Controllers/Api/UsersController.cs
git commit -m "PR2.16.2: rewrite UsersController to use ITaikoDbContext + IJwtTokenService"
```

### Tasks 2.16.3 through 2.16.10: Rewrite the other 9 controllers

**Files:** one per controller — `TaikoLocalServer/Controllers/Api/<X>Controller.cs`.

For each remaining controller (`AuthController`, `CardsController`, `DanBestDataController`, `FavoriteSongsController`, `GameDataController`, `PlayDataController`, `PlayHistoryController`, `SongLeaderboardController`, `UserSettingsController`):

**Pattern A: simple data access (Cards, FavoriteSongs, DanBestData, PlayData, PlayHistory, SongLeaderboard, UserSettings)**

- Constructor signature: `(ITaikoDbContext context, IJwtTokenService jwtTokens, IOptions<AuthSettings> settings) : BaseController<...>`
- Replace each `IUserDatumService.X(...)` / `ISongBestDatumService.X(...)` / etc. call with the equivalent direct `context.X.<query>` call. The wrapper service bodies (read in `TaikoLocalServer/Services/*.cs`) tell you exactly what query to inline.
- Save: `await context.SaveChangesAsync(HttpContext.RequestAborted);`

**Pattern B: GameDataController** — injects `IGameDataCatalog`, no DB access:

- Constructor: `(IGameDataCatalog catalog) : BaseController<GameDataController>`
- Method bodies access `catalog.MusicInfoes`, `catalog.LockedSongs`, etc. — same surface.

**Pattern C: AuthController (271 LOC)** — injects `ITaikoDbContext context, IJwtTokenService jwtTokens, IOptions<AuthSettings> settings`. The token-issuance section (was inline) calls `jwtTokens.IssueToken(baid, isAdmin)`. The remaining flows (Login, Register, ChangePassword, OTP) read from `context.Cards`, `context.Credentials`, `context.UserData` directly. **Take care to preserve OTP secret-handling logic exactly** — copy line by line from the original.

For each controller:

- [ ] **Step 1: Read the old controller and the wrapper services it consumes**
- [ ] **Step 2: Rewrite using the patterns above**
- [ ] **Step 3: Build TaikoLocalServer**
- [ ] **Step 4: Commit per controller (or in groups of 2–3)**

Suggested commit cadence:

```bash
git commit -m "PR2.16.3: rewrite CardsController + FavoriteSongsController to use ITaikoDbContext"
git commit -m "PR2.16.4: rewrite DanBestDataController + PlayDataController + PlayHistoryController"
git commit -m "PR2.16.5: rewrite SongLeaderboardController + UserSettingsController"
git commit -m "PR2.16.6: rewrite GameDataController to use IGameDataCatalog"
git commit -m "PR2.16.7: rewrite AuthController (Login/Register/ChangePassword/OTP) to use ITaikoDbContext + IJwtTokenService"
```

---

## Phase 2.17 — Delete wrapper services

### Task 2.17.1: Delete the 10 wrapper service files

**Files:**
- Delete: `TaikoLocalServer/Services/AuthService.cs`
- Delete: `TaikoLocalServer/Services/UserDatumService.cs`
- Delete: `TaikoLocalServer/Services/SongBestDatumService.cs`
- Delete: `TaikoLocalServer/Services/SongPlayDatumService.cs`
- Delete: `TaikoLocalServer/Services/DanScoreDatumService.cs`
- Delete: `TaikoLocalServer/Services/SongLeaderboardService.cs`
- Delete: `TaikoLocalServer/Services/Interfaces/IAuthService.cs`
- Delete: `TaikoLocalServer/Services/Interfaces/IUserDatumService.cs`
- Delete: `TaikoLocalServer/Services/Interfaces/ISongBestDatumService.cs`
- Delete: `TaikoLocalServer/Services/Interfaces/ISongPlayDatumService.cs`
- Delete: `TaikoLocalServer/Services/Interfaces/IDanScoreDatumService.cs`
- Delete: `TaikoLocalServer/Services/Interfaces/ISongLeaderboardService.cs`
- Delete: `TaikoLocalServer/Services/Extentions/ServiceExtensions.cs`
- Delete: `TaikoLocalServer/Services/` directory and subdirectories (after files removed)

- [ ] **Step 1: Delete files**

```bash
git rm TaikoLocalServer/Services/AuthService.cs
git rm TaikoLocalServer/Services/UserDatumService.cs
git rm TaikoLocalServer/Services/SongBestDatumService.cs
git rm TaikoLocalServer/Services/SongPlayDatumService.cs
git rm TaikoLocalServer/Services/DanScoreDatumService.cs
git rm TaikoLocalServer/Services/SongLeaderboardService.cs
git rm TaikoLocalServer/Services/Interfaces/IAuthService.cs
git rm TaikoLocalServer/Services/Interfaces/IUserDatumService.cs
git rm TaikoLocalServer/Services/Interfaces/ISongBestDatumService.cs
git rm TaikoLocalServer/Services/Interfaces/ISongPlayDatumService.cs
git rm TaikoLocalServer/Services/Interfaces/IDanScoreDatumService.cs
git rm TaikoLocalServer/Services/Interfaces/ISongLeaderboardService.cs
git rm TaikoLocalServer/Services/Extentions/ServiceExtensions.cs
```

After this:

```bash
rmdir TaikoLocalServer/Services/Interfaces
rmdir TaikoLocalServer/Services/Extentions
rmdir TaikoLocalServer/Services
```

- [ ] **Step 2: Update GlobalUsings.cs**

Remove the obsolete:
- `global using TaikoLocalServer.Services;`
- `global using TaikoLocalServer.Services.Interfaces;`

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: success. If errors are like "ISongLeaderboardService not found" in some controller, the controller wasn't fully rewritten in Phase 2.16 — return there.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "PR2.17: delete wrapper data-access services (10 files + 2 dirs); ITaikoDbContext is the new contract"
```

---

## Phase 2.18 — Slim Program.cs

### Task 2.18.1: Refactor Program.cs to call AddApplication / AddInfrastructure

**Files:**
- Modify: `TaikoLocalServer/Program.cs`

- [ ] **Step 1: Read the current file**

```bash
cat TaikoLocalServer/Program.cs
```

- [ ] **Step 2: Replace registration block**

Find the section (currently lines ~93–158 per pre-PR2 read):

```csharp
    // Add services to the container.
    builder.Services.AddMediator(opt =>
    {
        opt.ServiceLifetime = ServiceLifetime.Scoped;
        opt.Namespace = "TaikoLocalServer";
    });
    builder.Services.AddOptions();
    builder.Services.AddSingleton<IGameDataService, GameDataService>();
    builder.Services.AddScoped<ISongLeaderboardService, SongLeaderboardService>();
    builder.Services.Configure<ServerSettings>(builder.Configuration.GetSection(nameof(ServerSettings)));
    builder.Services.Configure<DataSettings>(builder.Configuration.GetSection(nameof(DataSettings)));
    builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(nameof(AuthSettings)));

    // Add Authentication with JWT
    builder.Services.AddAuthentication(...)
    .AddJwtBearer(options => { ... });

    builder.Services.AddScoped<AuthorizeIfRequiredAttribute>();

    builder.Services.AddControllers().AddProtoBufNet();
    builder.Services.AddDbContext<TaikoDbContext>(option => { ... });
    builder.Services.AddMemoryCache();
    builder.Services.AddCors(...);
    builder.Services.AddTaikoDbServices();
    builder.Services.AddSingleton<SongBestResponseMapper>();
```

Replace with:

```csharp
    // Add services to the container.
    builder.Services.AddOptions();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddScoped<AuthorizeIfRequiredAttribute>();

    builder.Services.AddControllers().AddProtoBufNet();
    builder.Services.AddMemoryCache();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllCorsPolicy", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
    builder.Services.AddSingleton<SongBestResponseMapper>();
```

- [ ] **Step 3: Add the Application/Infrastructure usings to Program.cs**

At the top of `Program.cs`, add:

```csharp
using TaikoLocalServer.Application;
using TaikoLocalServer.Infrastructure;
```

- [ ] **Step 4: Update the gameDataService lookup**

In the `app.Services.GetService<IGameDataService>()` block (already touched in Phase 2.13):

```csharp
    var gameDataCatalog = app.Services.GetService<IGameDataCatalog>();
    gameDataCatalog.ThrowIfNull();
    await gameDataCatalog.InitializeAsync();
```

- [ ] **Step 5: Remove obsolete using rows**

Drop these from Program.cs's top imports if present:
- `using TaikoLocalServer.Services.Extentions;`
- `using TaikoLocalServer.Settings;`

(Replaced by Application/Infrastructure usings.)

- [ ] **Step 6: Build solution**

```bash
dotnet build
```

Expected: success.

- [ ] **Step 7: Commit**

```bash
git add TaikoLocalServer/Program.cs
git commit -m "PR2.18: slim Program.cs — composition via AddApplication() + AddInfrastructure()"
```

---

## Phase 2.19 — LocalSaveModScoreMigrator update

### Task 2.19.1: Update LocalSaveModScoreMigrator references

**Files:**
- Modify: `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`
- Modify: `LocalSaveModScoreMigrator/*.cs` (using-statement updates)

- [ ] **Step 1: Read csproj**

```bash
cat LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj
```

- [ ] **Step 2: Update project refs**

Drop `..\GameDatabase\GameDatabase.csproj` (already gone, Phase 2.10) and `..\SharedProject\SharedProject.csproj`. Add `..\Infrastructure\Infrastructure.csproj`. The csproj should list:

```xml
    <ProjectReference Include="..\Domain\Domain.csproj" />
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
```

(Domain is also reachable transitively via Infrastructure, but having it explicit per spec §2 is intentional.)

- [ ] **Step 3: Update using statements**

```bash
grep -rln "using GameDatabase\|using SharedProject" LocalSaveModScoreMigrator
```

For each file, replace:
- `using GameDatabase.Context;` → `using TaikoLocalServer.Infrastructure.Persistence;`
- `using GameDatabase.Entities;` → `using TaikoLocalServer.Domain.Entities;`
- `using SharedProject.Enums;` → `using TaikoLocalServer.Domain.Enums;`
- `using SharedProject.Utils;` → as needed (likely `PathHelper` from Infrastructure, or `ValueHelpers` from Application).

- [ ] **Step 4: Build**

```bash
dotnet build LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj
```

Expected: success.

- [ ] **Step 5: Smoke-run migrator**

```bash
dotnet run --project LocalSaveModScoreMigrator
```

Expected: argparse error or help banner (since no args were passed). Confirms it builds and starts.

- [ ] **Step 6: Commit**

```bash
git add LocalSaveModScoreMigrator
git commit -m "PR2.19: LocalSaveModScoreMigrator targets Domain + Infrastructure"
```

---

## Phase 2.20 — Smoke gates and PR

### Task 2.20.1: Run PR2 smoke gates

Per spec §7 PR2:

- [ ] **Step 1: Build green**

```bash
dotnet build
```

Expected: 7 projects build, 0 errors. Inspect:

```bash
ls Application/obj/Debug/net10.0/generated/Mediator.SourceGenerator/
```

Expected: handler registration .g.cs files present.

- [ ] **Step 2: Server starts; migrates DB; binds Kestrel**

```bash
dotnet run --project TaikoLocalServer
```

Watch for:
- `Server starting up...`
- `Mapped <N> endpoints` — same N as the PR2 baseline from Task 2.0.2.
- `Now listening on:` lines for all configured ports.

If the route count differs from baseline, **stop and investigate** — likely a controller's routing was lost during rewrite.

- [ ] **Step 3: Game endpoint smoke (both `_ww` and `_cn`)**

```bash
# Replace localhost:54430 with whatever Kestrel.json binds for ww
curl -X POST http://localhost:54430/v12r08_ww/chassis/initialdatacheck.php \
  -H 'Content-Type: application/protobuf' \
  --data-binary @path/to/captured-ww-request.bin -i

# Replace localhost:57402 with cn binding
curl -X POST http://localhost:57402/v12r00_cn/chassis/initialdatacheck.php \
  -H 'Content-Type: application/protobuf' \
  --data-binary @path/to/captured-cn-request.bin -i
```

If you don't have captured request fixtures, simply verifying the route exists is acceptable:

```bash
curl -o /dev/null -s -w "%{http_code}\n" -X POST http://localhost:54430/v12r08_ww/chassis/initialdatacheck.php
```

Expected: NOT 404. (Likely 400 or 500 because no body, but routing reaches the controller.)

- [ ] **Step 4: Admin API smoke (auth off)**

Confirm `Configurations/AuthSettings.json` has `"AuthenticationRequired": false`.

```bash
curl http://localhost:5000/api/users -i
curl http://localhost:5000/api/users/1000 -i
curl -X DELETE http://localhost:5000/api/users/99999999 -i
```

Expected: HTTP 200 (or empty array/empty user object), 200 (or null), 404 (no such user). All routes resolve.

- [ ] **Step 5: Admin API smoke (auth on)**

In a SEPARATE terminal, edit `TaikoLocalServer/Configurations/AuthSettings.json` to set `"AuthenticationRequired": true`. Restart the server.

```bash
# Login (replace username/password with a registered admin from your DB)
curl -X POST http://localhost:5000/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"AccessCode":"<your-card-access-code>","Password":"<your-password>"}' \
  -i
```

Expected: HTTP 200 with a JWT in the response body.

```bash
# Use the JWT
TOKEN="<paste-jwt-here>"
curl http://localhost:5000/api/users/1000 -H "Authorization: Bearer $TOKEN" -i
# As admin: should succeed
# As non-admin baid 1000 calling /api/users/2000 (different baid): should return null
```

- [ ] **Step 6: WebUI smoke**

Browse to `http://localhost:5000/` and:
- Dashboard renders.
- Login page accepts credentials.
- After login, returns to Dashboard with auth context.
- Browser DevTools console: no red errors.

- [ ] **Step 7: Migrator smoke**

```bash
dotnet run --project LocalSaveModScoreMigrator
```

Expected: argparse banner or "missing required argument" error. Builds + starts.

- [ ] **Step 8: Restore AuthenticationRequired to its previous value**

If you changed it for Step 5, restore.

### Task 2.20.2: Push and (optionally) open PR

- [ ] **Step 1: Confirm clean tree**

```bash
git status
```

- [ ] **Step 2: Push**

```bash
git push -u origin dev/clean-arch-app-infra
```

- [ ] **Step 3: Open PR (only if user authorized)**

Ask before running `gh pr create`. If approved:

```bash
gh pr create --title "Clean architecture refactor PR2: Application + Infrastructure" --body "$(cat <<'EOF'
## Summary

Extracts `TaikoLocalServer.Application` and `TaikoLocalServer.Infrastructure`. Defines the four ports (`ITaikoDbContext`, `IGameDataCatalog`, `IJwtTokenService`, `IClock`). Drops the wrapper data-access services and rewrites admin controllers to inject `ITaikoDbContext` + `IJwtTokenService` directly. Renames `GameDatabase` → `TaikoLocalServer.Infrastructure`. Slims `Program.cs` to composition via `AddApplication()` + `AddInfrastructure()`.

Spec: docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md (§7 PR2)

## Test plan

- [x] `dotnet build` green; Mediator generator produces handler registrations in Application
- [x] Server starts; migrates DB; route count unchanged
- [x] Game endpoint smoke (`_ww` and `_cn`)
- [x] Admin API smoke (auth off): `GET /api/users`, `GET /api/users/{baid}`, `DELETE /api/users/{baid}`
- [x] Admin API smoke (auth on): `POST /api/auth/login` returns JWT; cross-user baid is rejected
- [x] WebUI loads, Dashboard renders, login round-trips
- [x] LocalSaveModScoreMigrator builds + starts

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)" --base dev
```

---

## Risk register specific to PR2

From spec §8:

| # | Risk | Mitigation |
|---|---|---|
| 2 | Mediator source generator finds zero handlers after namespace change | Phase 2.15 prototype check. If zero registrations, switch to Task 2.9.2 Host-colocated `AddMediator` fallback. |
| 3 | AdminApi controller rewrite changes auth behavior | Phase 2.20 Step 5 explicitly exercises both auth-on and auth-off paths with `AuthController`. Compare wire-level responses (HTTP code + JWT structure) against a captured baseline from `dev` if available. |

If Risk #3 fires (wrong auth response in any rewritten controller), inspect the diff against the original AuthService method body — likely the `Expires` value in `IssueToken` doesn't match the original token-creation block, or a missing `[ServiceFilter]` attribute on a method, or a `tokenInfo.Value.IsAdmin` (PascalCase) lookup that should have been `tokenInfo.Value.isAdmin` if you missed the property-name change.

---

## Definition of done for PR2

- [ ] All 7 projects build via `dotnet build` from repo root with 0 errors.
- [ ] `Application/` exists with: Abstractions/ (4 ports), Handlers/ (16 files), Dtos/ (15 Common* files), Catalog/ (12 VOs), ServerData/ (8+ shapes), Common/ (4 utils + Constants.cs), Settings/ (ServerSettings.cs), DependencyInjection.cs.
- [ ] `Infrastructure/` exists with: Persistence/ (TaikoDbContext + TaikoDbContextPartial + Migrations/ + PersistenceConstants.cs), GameDataCatalog/ (FileGameDataCatalog + PathHelper + CatalogConstants + Settings/DataSettings), Identity/ (JwtTokenService + Settings/AuthSettings), Time/SystemClock, Settings/AllnetSettings, DependencyInjection.cs.
- [ ] `TaikoLocalServer/Services/` is gone.
- [ ] `TaikoLocalServer/Settings/` is empty (or gone if you removed it).
- [ ] `TaikoLocalServer/Common/` is gone.
- [ ] `TaikoLocalServer/Models/Application/` is gone; `TaikoLocalServer/Handlers/` is gone.
- [ ] `Program.cs` is roughly 80-90 lines and uses `AddApplication()` / `AddInfrastructure()`.
- [ ] Mediator source generator emits handler registrations in `Application/obj/.../generated/`.
- [ ] Server's mapped endpoint count matches the Phase 2.0 baseline.
- [ ] Game `_ww`, game `_cn`, admin auth-off, admin auth-on, WebUI login, and migrator smokes all pass.
- [ ] Branch `dev/clean-arch-app-infra` is pushed and (if user-authorized) a PR is open.

After merge, proceed to PR3 with `git checkout dev && git pull --ff-only` and **start a new Claude Code session for PR3 from the next plan file:** `docs/superpowers/plans/2026-05-04-clean-arch-pr3-adapters.md`.
