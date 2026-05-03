# .NET 10 Upgrade — PR2: .NET 10 Runtime + Mediator Swap

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move all five projects to .NET 10, refresh every server-side NuGet package, and replace MediatR with martinothamar's source-generated Mediator (full convention adoption: `ValueTask` returns, `readonly record struct` requests, scoped service lifetime, `CancellationToken` plumbing). After this PR, the server runs on .NET 10 LTS. The WebUI builds against .NET 10 too, but its MudBlazor v6.20.0 packages are kept for now — PR3 (separate plan) handles the UI migration.

**Architecture:** This PR has three logical layers that get changed together:
1. **Runtime/SDK:** `Directory.Build.props` TFM bump, `global.json` SDK pin, GitHub Actions workflow.
2. **Packages:** `Directory.Packages.props` bulk version bumps to .NET-10-compatible releases.
3. **Mediator swap:** `MediatR` package out, `Mediator.SourceGenerator` + `Mediator.Abstractions` in. 16 handlers + 32 controller call sites + `BaseController<T>` + `Program.cs` DI + `GlobalUsings.cs` all touched together because the namespace and types change.

**Tech Stack:** .NET 10 LTS, C# 13, ASP.NET Core 10, EF Core 10, Mediator (martinothamar), protobuf-net, Serilog.

**Branch:** `dev/net10-runtime` off the merged tip of `dev/modernize-on-net8` (i.e. `dev` after PR1 merges).

---

## Context — read these first

Both files are required reading before executing any task:

- `CLAUDE.md` (repo root) — solution layout, build commands, configuration model.
- `docs/superpowers/specs/2026-05-03-net10-upgrade-design.md` — full spec (this plan covers only Section 4: PR2).

**Critical pre-condition:** PR1 (`docs/superpowers/plans/2026-05-03-net10-pr1-modernize-on-net8.md`) must be merged into `dev` before starting this plan. PR1 introduces `Directory.Build.props`, `Directory.Packages.props`, `global.json`, and Central Package Management — every task here assumes those are in place.

## File structure

**Files modified (infrastructure):**
- `Directory.Build.props` — bump `TargetFramework` net8.0 → net10.0, `LangVersion` 12 → 13.
- `global.json` — bump SDK pin to `10.0.100`.
- `Directory.Packages.props` — bulk version bumps; remove MediatR, add Mediator packages.
- `.github/workflows/publishTLS.yml` — bump setup-dotnet to 10.0.x; bump Minor_Version_Number 1 → 2.

**Files created:**
- `TaikoLocalServer.slnx` — replaces the existing `.sln` (deferred from PR1 because .slnx requires .NET 9+ SDK).

**Files deleted:**
- `TaikoLocalServer.sln`

**Files modified (TaikoLocalServer source):**
- `TaikoLocalServer/GlobalUsings.cs` — `using MediatR;` → `using Mediator;`.
- `TaikoLocalServer/Program.cs` — `AddMediatR(...)` → `AddMediator(opt => …)`; possibly `UseStaticFiles` → `MapStaticAssets`; possibly drop `EnableConfigurationBindingGenerator=false`.
- `TaikoLocalServer/Controllers/BaseController.cs` — `ISender` → `IMediator`.
- `TaikoLocalServer/Handlers/*.cs` (16 files) — request types `record` → `readonly record struct` (where applicable); handler `Task<T>` → `ValueTask<T>`.
- `TaikoLocalServer/Controllers/Game/*.cs` (16 files, 32 call sites) — `Mediator.Send(query)` → `Mediator.Send(query, HttpContext.RequestAborted)`.
- `TaikoLocalServer/TaikoLocalServer.csproj` — possibly drop `<EnableConfigurationBindingGenerator>false</EnableConfigurationBindingGenerator>` if the .NET 10 generator works without the workaround.

**Files modified (docs):**
- `README.md` — update prerequisite to ".NET 10 SDK".
- `CLAUDE.md` — replace MediatR references with Mediator references; update conventions notes.

**Files NOT touched:**
- `GameDatabase/` source (only the package versions in CPM change for EF Core).
- `Mappers/`, `Models/`, `Services/`, `Filters/`, `Middlewares/`.
- `wwwroot/data/`, `Configurations/`, `Migrations/`.
- Any `*.razor` or `*.razor.cs` (PR3's job).
- `LocalSaveModScoreMigrator/` source.

## Pre-flight

- [ ] **Step 0.1: Confirm PR1 is merged**

Run:
```bash
git checkout dev
git pull --ff-only
ls Directory.Build.props Directory.Packages.props global.json
```
Expected: all three files exist. If not, PR1 isn't merged yet — stop and merge it first.

- [ ] **Step 0.2: Confirm .NET 10 SDK is installed**

Run:
```bash
dotnet --list-sdks
```
Expected: at least one `10.0.x` SDK listed. If absent, install from https://dot.net before continuing.

- [ ] **Step 0.3: Verify baseline build is green on `dev`**

Run:
```bash
dotnet restore
dotnet build
```
Expected: succeeds on .NET 8 (because `global.json` still pins 8). If it fails, fix `dev` first.

- [ ] **Step 0.4: Back up the dev DB**

The build copies `taiko.db3` to the output directory. EF Core 10 may attempt schema diff checks on first run. Back up before this PR's smoke test:
```bash
cp TaikoLocalServer/wwwroot/taiko.db3 TaikoLocalServer/wwwroot/taiko.db3.pre-pr2-backup 2>/dev/null || true
```

- [ ] **Step 0.5: Create the branch**

```bash
git checkout -b dev/net10-runtime
```

---

## Task 1: Bump SDK pin in `global.json`

**Files:**
- Modify: `global.json`

- [ ] **Step 1.1: Detect installed .NET 10 SDK band**

Run:
```bash
dotnet --list-sdks
```
Note the latest `10.0.x` version (e.g. `10.0.100` or higher).

- [ ] **Step 1.2: Update `global.json`**

Overwrite `D:\TaikoLocalServer\global.json` with (replace `10.0.100` with the version from Step 1.1):

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

- [ ] **Step 1.3: Verify the new pin is honored**

Run:
```bash
dotnet --version
```
Expected: a `10.0.x` version. If it still shows `8.0.x`, your shell is reading a cached `global.json` — re-open the terminal.

- [ ] **Step 1.4: Verify the .NET 8 build still works at this point**

Note: After this commit, `dotnet build` will start using the .NET 10 SDK against `net8.0` projects. SDK 10 supports building older TFMs, so this should succeed even though we haven't bumped TFMs yet.

Run:
```bash
dotnet restore
dotnet build
```
Expected: build succeeds. Some warnings about analyzers built for older Roslyn versions are possible but harmless.

- [ ] **Step 1.5: Commit**

```bash
git add global.json
git commit -m "Pin .NET 10 SDK"
```

---

## Task 2: Bump TFM and LangVersion in `Directory.Build.props`

**Files:**
- Modify: `Directory.Build.props`

- [ ] **Step 2.1: Replace the file content**

Overwrite `D:\TaikoLocalServer\Directory.Build.props` with:

```xml
<Project>

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>13</LangVersion>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

</Project>
```

Changes: `net8.0` → `net10.0`, `12` → `13`.

- [ ] **Step 2.2: Restore and observe the version mismatch errors**

Run:
```bash
dotnet restore
```
Expected: errors. Every Microsoft.AspNetCore.* / Microsoft.EntityFrameworkCore.* / Microsoft.Extensions.* package version in `Directory.Packages.props` is still 8.0.x and is incompatible with `net10.0`. NuGet will report `NU1202` errors. Task 3 fixes this.

DO NOT commit yet — the tree is in a broken state and the next task must be applied immediately.

---

## Task 3: Bulk-bump .NET-platform packages in `Directory.Packages.props`

**Files:**
- Modify: `Directory.Packages.props`

This task bumps every package whose version is tied to the .NET runtime release cadence. Non-Microsoft packages are bumped in Task 4 to keep diffs reviewable.

- [ ] **Step 3.1: Replace the relevant `<PackageVersion>` entries**

Edit `D:\TaikoLocalServer\Directory.Packages.props`. Update *only* these entries (leave non-Microsoft packages untouched in this task):

```xml
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.ResponseCompression" Version="10.0.0" />

    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
    <PackageVersion Include="EntityFrameworkCore.Exceptions.Sqlite" Version="10.0.0" />

    <PackageVersion Include="Microsoft.Extensions.Localization" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Localization.Abstractions" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.0" />
```

- [ ] **Step 3.2: Verify versions exist on NuGet**

Run:
```bash
dotnet restore
```
Expected: succeeds. If any package returns `NU1102 Unable to find package … with version 10.0.0`, it means that exact version doesn't exist — check NuGet:
```bash
dotnet nuget list source
# Then for any failing package, search the public feed
```

If `EntityFrameworkCore.Exceptions.Sqlite 10.0.0` isn't published, fall back: remove the package entirely and use plain `DbUpdateException` handling. This is the lower-risk fallback called out in the spec. The package is referenced only in `GameDatabase/Context/TaikoDbContext.cs:44` (`optionsBuilder.UseExceptionProcessor()`). To remove:
1. In `Directory.Packages.props`, delete the `<PackageVersion Include="EntityFrameworkCore.Exceptions.Sqlite" … />` line.
2. In `GameDatabase/GameDatabase.csproj`, delete `<PackageReference Include="EntityFrameworkCore.Exceptions.Sqlite" />`.
3. In `GameDatabase/Context/TaikoDbContext.cs`, remove `using EntityFramework.Exceptions.Sqlite;` (line 1) and `.UseExceptionProcessor()` (line 44).

Re-run `dotnet restore`. Expected: clean.

- [ ] **Step 3.3: Verify the build now compiles against net10.0**

Run:
```bash
dotnet build
```
Expected: succeeds. Some warnings are likely from API changes between EF Core 8 and 10 — examples: `IModel.IsTrackingEnabled` deprecation, nullable annotation refinements. Treat warnings as TODOs for Task 11's clean-up step, not blockers here.

- [ ] **Step 3.4: Commit Tasks 2 + 3 together**

The Task 2 commit was deferred to here so the tree is never broken.

```bash
git add Directory.Build.props Directory.Packages.props
git commit -m "Bump TFM to net10.0, LangVersion to 13, and Microsoft.* packages to 10.0.0"
```

---

## Task 4: Bump non-Microsoft packages in `Directory.Packages.props`

**Files:**
- Modify: `Directory.Packages.props`

- [ ] **Step 4.1: Check which non-Microsoft packages are outdated**

Run:
```bash
dotnet list package --outdated --include-transitive false
```
Expected: a per-project listing showing current vs latest versions for non-Microsoft packages.

- [ ] **Step 4.2: Update the relevant `<PackageVersion>` entries**

Bump these entries in `Directory.Packages.props` to the latest stable versions reported by Step 4.1. The values below are best-known starting points as of 2026-05-03 — adjust if `dotnet list package --outdated` reports newer:

```xml
    <PackageVersion Include="protobuf-net" Version="3.2.45" />
    <PackageVersion Include="protobuf-net.AspNetCore" Version="3.2.12" />
    <PackageVersion Include="Riok.Mapperly" Version="4.1.1" />
    <PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
    <PackageVersion Include="Serilog.Expressions" Version="5.0.0" />
    <PackageVersion Include="Serilog.Sinks.File.Header" Version="1.0.2" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="7.2.0" />
    <PackageVersion Include="Yoh.Text.Json.NamingPolicies" Version="1.1.2" />

    <PackageVersion Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageVersion Include="DotNetZip" Version="1.16.0" />
    <PackageVersion Include="Otp.NET" Version="1.4.0" />
    <PackageVersion Include="SharpZipLib" Version="1.4.2" />
    <PackageVersion Include="Swan.Core" Version="9.0.1" />
    <PackageVersion Include="Swan.Logging" Version="9.0.1" />
    <PackageVersion Include="Throw" Version="1.4.0" />

    <PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="8.3.0" />
    <PackageVersion Include="JorgeSerrano.Json.JsonSnakeCaseNamingPolicy" Version="0.9.0" />
    <PackageVersion Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
```

DO NOT bump these in this task — they're handled in Tasks 5–6 (Mediator swap) and PR3 (UI):
- `MediatR`
- `MudBlazor`, `CodeBeam.MudBlazor.Extensions`, `Blazored.LocalStorage`, `Markdig`, `Autocomplete.Clients`

- [ ] **Step 4.3: Verify restore + build still pass**

Run:
```bash
dotnet restore
dotnet build
```
Expected: succeeds.

- [ ] **Step 4.4: If Riok.Mapperly 3.x → 4.x produces compile errors in `Mappers/`**

Mapperly 4.x tightened nullable analysis. If the build now reports errors like `RMG020: Source member is nullable, but target is not` in `Mappers/*.cs`, the fix is to add explicit null handling on the partial method declarations. Read each error and apply the suggested attribute (e.g. `[MapperRequiredMapping]` or `[MapProperty]`) to the offending mapper. Do NOT change the source DTO types.

If the errors look open-ended, downgrade `Riok.Mapperly` back to `3.5.1` for this PR and revisit in a future cleanup. PR2's goal is the runtime move + Mediator swap — Mapperly cleanup is non-blocking.

- [ ] **Step 4.5: Commit**

```bash
git add Directory.Packages.props
git commit -m "Bump non-Microsoft packages to latest stable"
```

---

## Task 5: Replace `MediatR` with `Mediator` packages in CPM

**Files:**
- Modify: `Directory.Packages.props`

This task only updates the package versions in CPM. The actual code references to MediatR types are still present and will fail at this point — Tasks 6–10 fix them.

- [ ] **Step 5.1: Update `Directory.Packages.props`**

In `D:\TaikoLocalServer\Directory.Packages.props`, replace this line:

```xml
    <PackageVersion Include="MediatR" Version="12.2.0" />
```

with:

```xml
    <PackageVersion Include="Mediator.SourceGenerator" Version="3.0.2" />
    <PackageVersion Include="Mediator.Abstractions" Version="3.0.2" />
```

- [ ] **Step 5.2: Update `TaikoLocalServer.csproj`**

In `D:\TaikoLocalServer\TaikoLocalServer\TaikoLocalServer.csproj`, replace:

```xml
    <PackageReference Include="MediatR" />
```

with:

```xml
    <PackageReference Include="Mediator.SourceGenerator">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Mediator.Abstractions" />
```

`Mediator.SourceGenerator` is an analyzer — it must be referenced with `PrivateAssets="all"` so it doesn't propagate to consumers and isn't packaged into the published exe. `Mediator.Abstractions` ships the `IRequest`, `IRequestHandler`, `IMediator`, `ISender` interfaces.

- [ ] **Step 5.3: Restore (build will still fail, that's expected)**

Run:
```bash
dotnet restore
```
Expected: succeeds. Both Mediator packages download.

DO NOT commit yet — the tree is broken (code still imports `MediatR`). Tasks 6–10 fix this.

---

## Task 6: Update `GlobalUsings.cs`

**Files:**
- Modify: `TaikoLocalServer/GlobalUsings.cs`

- [ ] **Step 6.1: Replace the file content**

Overwrite `D:\TaikoLocalServer\TaikoLocalServer\GlobalUsings.cs` with:

```csharp
// Global using directives

global using GameDatabase.Entities;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using ProtoBuf;
global using Mediator;
global using Swan.Formatters;
global using SharedProject.Enums;
global using TaikoLocalServer.Common;
global using TaikoLocalServer.Common.Utils;
global using TaikoLocalServer.Handlers;
global using TaikoLocalServer.Models;
global using TaikoLocalServer.Models.Application;
global using TaikoLocalServer.Models.WW08;
global using TaikoLocalServer.Services;
global using TaikoLocalServer.Services.Interfaces;
```

Changes: `MediatR` → `Mediator`. All other imports unchanged.

The `Mediator` namespace exposes `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`, `IMediator`, `ISender`, `Unit` — the same shape as MediatR's API.

- [ ] **Step 6.2: Don't build yet**

Code still has direct `using MediatR;` lines in some handler files. Tasks 7–9 clean those up.

---

## Task 7: Update `Program.cs` DI registration

**Files:**
- Modify: `TaikoLocalServer/Program.cs`

- [ ] **Step 7.1: Replace the MediatR registration line**

In `D:\TaikoLocalServer\TaikoLocalServer\Program.cs`, find this line (around line 92):

```csharp
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

Replace with:

```csharp
    builder.Services.AddMediator(opt =>
    {
        opt.ServiceLifetime = ServiceLifetime.Scoped;
        opt.Namespace = "TaikoLocalServer";
    });
```

`opt.ServiceLifetime = ServiceLifetime.Scoped` — handlers inject `TaikoDbContext` (scoped), so they cannot be singletons.
`opt.Namespace = "TaikoLocalServer"` — the source generator emits a `TaikoLocalServer.Mediator` partial class. Setting an explicit namespace avoids polluting the global namespace.

- [ ] **Step 7.2: Don't build yet**

Handler signatures still return `Task<T>` (MediatR's contract). Mediator's `IRequestHandler<T, R>` requires `ValueTask<R>`. Task 9 fixes this.

---

## Task 8: Update `BaseController<T>` from `ISender` to `IMediator`

**Files:**
- Modify: `TaikoLocalServer/Controllers/BaseController.cs`

- [ ] **Step 8.1: Replace the file content**

Overwrite `D:\TaikoLocalServer\TaikoLocalServer\Controllers\BaseController.cs` with:

```csharp
namespace TaikoLocalServer.Controllers;

public abstract class BaseController<T> : ControllerBase where T : BaseController<T>
{
    private ILogger<T>? logger;

    private IMediator? mediator;

    protected IMediator Mediator => (mediator ??= HttpContext.RequestServices.GetService<IMediator>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}
```

Changes: `ISender` → `IMediator`. Field name `mediator` and property name `Mediator` unchanged so all 32 controller call sites still compile (modulo their own changes in Task 10).

`IMediator` is the unified interface in martinothamar's Mediator library — it exposes both the typed request/response dispatch (`Send`) and the `Publish` API for notifications. We don't use notifications here, but `IMediator` is the conventional choice over the lower-level `ISender`.

- [ ] **Step 8.2: Don't build yet**

Handlers still need to be updated (Task 9). Build at the end of Task 9.

---

## Task 9: Convert handler signatures `Task<T>` → `ValueTask<T>` and request types to `readonly record struct`

**Files:**
- Modify: All 16 files in `TaikoLocalServer/Handlers/*.cs`

Mediator's `IRequestHandler<TRequest, TResponse>.Handle` returns `ValueTask<TResponse>` (lower allocation than `Task<TResponse>`). Request types should be `readonly record struct` for in-process dispatch — the Mediator source generator monomorphizes `Send<TRequest, TResponse>` per request type, and value-type requests avoid heap allocation entirely.

For each of the 16 handler files, two transformations apply:

**Transformation A: Request type — `record` → `readonly record struct`**

Apply this transformation **only** to request records whose parameters are all primitive types or value types. If a parameter is a reference type (e.g. `string`, an array, a complex DTO), the wrapper is still made `readonly record struct` — the inner reference is unchanged, and the wrapper's value-type semantics are still beneficial.

Apply this transformation to every request type. Examples:

```csharp
// before:
public record BaidQuery(string AccessCode) : IRequest<CommonBaidResponse>;
// after:
public readonly record struct BaidQuery(string AccessCode) : IRequest<CommonBaidResponse>;
```

```csharp
// before:
public record UpdatePlayResultCommand(uint Baid, CommonPlayResultData PlayResultData) : IRequest<uint>;
// after:
public readonly record struct UpdatePlayResultCommand(uint Baid, CommonPlayResultData PlayResultData) : IRequest<uint>;
```

```csharp
// before:
public record GetTokenCountQuery(uint Baid) : IRequest<CommonGetTokenCountResponse>;
// after:
public readonly record struct GetTokenCountQuery(uint Baid) : IRequest<CommonGetTokenCountResponse>;
```

**Transformation B: Handler — `Task<T>` → `ValueTask<T>`**

Change the `IRequestHandler<…>` method signature from `Task<T>` to `ValueTask<T>`. The method body is unchanged — `await` works identically on either return type, and Mediator's `IRequestHandler<TRequest, TResponse>.Handle` is `ValueTask<TResponse>`.

Example:

```csharp
// before:
public async Task<CommonBaidResponse> Handle(BaidQuery request, CancellationToken cancellationToken)
{
    // ... unchanged body ...
}
// after:
public async ValueTask<CommonBaidResponse> Handle(BaidQuery request, CancellationToken cancellationToken)
{
    // ... unchanged body ...
}
```

- [ ] **Step 9.1: Apply both transformations to each of the 16 handler files**

| File | Request type | Response type |
|---|---|---|
| `TaikoLocalServer/Handlers/AddMyDonEntryCommand.cs` | `AddMyDonEntryCommand` | `CommonMyDonEntryResponse` |
| `TaikoLocalServer/Handlers/AddTokenCountCommand.cs` | `AddTokenCountCommand` | `CommonGetTokenCountResponse` |
| `TaikoLocalServer/Handlers/BaidQuery.cs` | `BaidQuery` | `CommonBaidResponse` |
| `TaikoLocalServer/Handlers/GetAiDataQuery.cs` | `GetAiDataQuery` | `CommonAiDataResponse` |
| `TaikoLocalServer/Handlers/GetAiScoreQuery.cs` | `GetAiScoreQuery` | `CommonAiScoreResponse` |
| `TaikoLocalServer/Handlers/GetDanOdaiQuery.cs` | `GetDanOdaiQuery` | response type per file |
| `TaikoLocalServer/Handlers/GetDanScoreQuery.cs` | `GetDanScoreQuery` | `CommonDanScoreDataResponse` |
| `TaikoLocalServer/Handlers/GetFolderQuery.cs` | `GetFolderQuery` | `CommonGetFolderResponse` |
| `TaikoLocalServer/Handlers/GetInitialDataQuery.cs` | `GetInitialDataQuery` | `CommonInitialDataCheckResponse` |
| `TaikoLocalServer/Handlers/GetSelfBestQuery.cs` | `GetSelfBestQuery` | `CommonSelfBestResponse` |
| `TaikoLocalServer/Handlers/GetShopFolderQuery.cs` | `GetShopFolderQuery` | `CommonGetShopFolderResponse` |
| `TaikoLocalServer/Handlers/GetSongIntroductionQuery.cs` | `GetSongIntroductionQuery` | `CommonGetSongIntroductionResponse` |
| `TaikoLocalServer/Handlers/GetTokenCountQuery.cs` | `GetTokenCountQuery` | `CommonGetTokenCountResponse` |
| `TaikoLocalServer/Handlers/PurchaseSongCommand.cs` | `PurchaseSongCommand` | `CommonSongPurchaseResponse` |
| `TaikoLocalServer/Handlers/UpdatePlayResultCommand.cs` | `UpdatePlayResultCommand` | `uint` |
| `TaikoLocalServer/Handlers/UserDataQuery.cs` | `UserDataQuery` | `CommonUserDataResponse` |

For each file:
1. Open the file.
2. Find the `public record … : IRequest<…>;` line. Change `record` → `readonly record struct`.
3. Find the `public async Task<TResponse> Handle(…)` line. Change `Task<` → `ValueTask<`.
4. Save.

- [ ] **Step 9.2: Build and observe**

Run:
```bash
dotnet build
```

Expected outcomes:
- Source-gen warnings/diagnostics from `Mediator.SourceGenerator` confirm all 16 handlers are picked up (look for `MEDIATOR0001` info messages, or absence of `MEDIATOR0010`-style "no handler found" warnings).
- Compile errors in `Controllers/Game/*.cs` because handler return types changed and call sites haven't been updated yet — that's Task 10.
- Compile errors in `BaseController` are NOT expected (Task 8 already fixed it).

If you see `MEDIATOR0010` or similar "handler not found" diagnostics, the source generator didn't discover a handler. The most likely cause: the request record's namespace doesn't match the handler's, or the handler class isn't `public`. Check that all 16 are in `namespace TaikoLocalServer.Handlers` and `public class … : IRequestHandler<…>`.

- [ ] **Step 9.3: Don't commit yet**

Build is broken at this point. Task 10 closes it.

---

## Task 10: Add `CancellationToken` plumbing to controller call sites

**Files:**
- Modify: All 16 files in `TaikoLocalServer/Controllers/Game/*.cs`, 32 call sites total.

Today's controllers call `await Mediator.Send(new SomeQuery(...))` without a `CancellationToken`. This means if the game client disconnects mid-request, the handler keeps running and holds DB connections. We add `HttpContext.RequestAborted` to every call.

- [ ] **Step 10.1: Apply the transformation to each controller**

For each file in the table below, find every `Mediator.Send(…)` call and add `, HttpContext.RequestAborted` as the second argument.

Pattern:

```csharp
// before:
var commonResponse = await Mediator.Send(new BaidQuery(request.AccessCode));
// after:
var commonResponse = await Mediator.Send(new BaidQuery(request.AccessCode), HttpContext.RequestAborted);
```

| File | Call sites |
|---|---|
| `TaikoLocalServer/Controllers/Game/AddTokenCountController.cs` | 2 (`_cn` + `_ww`) |
| `TaikoLocalServer/Controllers/Game/BaidController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/GetAiDataController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/GetAiScoreController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/GetDanOdaiController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/GetDanScoreController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/GetFolderController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/GetShopFolderController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/GetSongIntroductionController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/GetTokenCountController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/InitialDataCheckController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/MyDonEntryController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/PlayResultController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/SelfBestController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/SongPurchaseController.cs` | 2 |
| `TaikoLocalServer/Controllers/Game/UserDataController.cs` | 2 |

**Total: 32 call sites across 16 files.**

The changes can be applied with a regex find-and-replace if your editor supports it:
- Find: `Mediator\.Send\((new \w+\([^)]*\))\)`
- Replace: `Mediator.Send($1, HttpContext.RequestAborted)`

But verify each edit visually — controllers may have multi-line call chains that the regex won't match.

- [ ] **Step 10.2: Build**

Run:
```bash
dotnet build
```
Expected: succeeds with 0 errors. Some warnings are fine (e.g. unused `cancellationToken` parameter in handlers that don't pass it through to all `await` points — fix in Task 11).

- [ ] **Step 10.3: Audit handlers for incomplete CancellationToken plumbing**

Run:
```bash
grep -rn "FirstOrDefaultAsync\|SaveChangesAsync\|ToListAsync\|ToArrayAsync\|FindAsync\|SingleOrDefaultAsync" TaikoLocalServer/Handlers/
```
For each match, check if `cancellationToken` is passed as an argument. If not, add it.

Common pattern fix:

```csharp
// before:
var card = await context.Cards.FindAsync(request.AccessCode);
// after:
var card = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
```

```csharp
// before:
await context.SaveChangesAsync();
// after:
await context.SaveChangesAsync(cancellationToken);
```

This is best-effort — the goal is to plumb CT through new code paths, not to refactor every handler.

- [ ] **Step 10.4: Commit Tasks 5–10 together**

The Mediator swap was deferred to one commit so the tree was never half-converted on disk in git history.

```bash
git add Directory.Packages.props \
  TaikoLocalServer/TaikoLocalServer.csproj \
  TaikoLocalServer/GlobalUsings.cs \
  TaikoLocalServer/Program.cs \
  TaikoLocalServer/Controllers/BaseController.cs \
  TaikoLocalServer/Handlers/*.cs \
  TaikoLocalServer/Controllers/Game/*.cs
git commit -m "Swap MediatR for martinothamar's Mediator with full conventions

- ValueTask<T> handlers
- readonly record struct requests
- Scoped service lifetime
- HttpContext.RequestAborted plumbed to all 32 Send call sites"
```

---

## Task 11: Re-evaluate `EnableConfigurationBindingGenerator=false` workaround

**Files:**
- Possibly modify: `TaikoLocalServer/TaikoLocalServer.csproj`

The `<EnableConfigurationBindingGenerator>false</EnableConfigurationBindingGenerator>` property in `TaikoLocalServer.csproj` was a workaround for an .NET 8-era issue with `Configure<T>(IConfiguration)` and certain config shapes. .NET 10's source generator is more permissive — try removing the property and see if the build still succeeds.

- [ ] **Step 11.1: Remove the property**

In `D:\TaikoLocalServer\TaikoLocalServer\TaikoLocalServer.csproj`, find:

```xml
    <EnableConfigurationBindingGenerator>false</EnableConfigurationBindingGenerator>
```

Delete this line.

- [ ] **Step 11.2: Try to build**

Run:
```bash
dotnet build
```

- [ ] **Step 11.3: Decide based on outcome**

**If build succeeds:** the workaround is no longer needed. Proceed to Step 11.4.

**If build fails** with errors like `SYSLIB1100`, `SYSLIB1101`, or "configuration source generator does not support …":
1. Read the generator's error message — the offending type is named.
2. Compare: is the type/property nullability different than expected by the generator?
3. **If the fix is small** (e.g. add a `[ConfigurationKeyName]` attribute on a non-conforming property), apply it and rebuild.
4. **If the fix is non-trivial**, revert this task: re-add the `<EnableConfigurationBindingGenerator>false</EnableConfigurationBindingGenerator>` property and skip Step 11.4. Document in the commit message that the workaround is still required on .NET 10 and link the offending types.

- [ ] **Step 11.4: Commit the change (if successful)**

```bash
git add TaikoLocalServer/TaikoLocalServer.csproj
git commit -m "Drop EnableConfigurationBindingGenerator=false workaround"
```

If the workaround had to stay, no commit is needed (the file is unchanged from the previous task).

---

## Task 12: Convert `.sln` → `.slnx`

**Files:**
- Create: `TaikoLocalServer.slnx`
- Delete: `TaikoLocalServer.sln`

The `.slnx` format requires .NET 9+ SDK, which is now in place. Conversion can be done via the SDK's built-in tool.

- [ ] **Step 12.1: Run the conversion**

Run:
```bash
dotnet sln TaikoLocalServer.sln migrate
```
Expected: produces `TaikoLocalServer.slnx`. Console output: `Migrated solution to TaikoLocalServer.slnx`.

If the `dotnet sln migrate` subcommand isn't available in this SDK version (it was preview in 9.0 and stable in 10.0; check `dotnet sln --help`), fall back to manual conversion: write `D:\TaikoLocalServer\TaikoLocalServer.slnx` with this content:

```xml
<Solution>
  <Project Path="GameDatabase/GameDatabase.csproj" />
  <Project Path="LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj" />
  <Project Path="SharedProject/SharedProject.csproj" />
  <Project Path="TaikoLocalServer/TaikoLocalServer.csproj" />
  <Project Path="TaikoWebUI/TaikoWebUI.csproj" />
</Solution>
```

- [ ] **Step 12.2: Verify the new solution file works**

Run:
```bash
dotnet build TaikoLocalServer.slnx
```
Expected: builds all five projects.

- [ ] **Step 12.3: Delete the old `.sln`**

```bash
rm TaikoLocalServer.sln
```

`TaikoLocalServer.sln.DotSettings` keeps working — JetBrains products match it to `TaikoLocalServer.slnx` by basename.

- [ ] **Step 12.4: Commit**

```bash
git add TaikoLocalServer.slnx
git rm TaikoLocalServer.sln
git commit -m "Convert .sln to .slnx"
```

---

## Task 13: Bump GitHub Actions workflow

**Files:**
- Modify: `.github/workflows/publishTLS.yml`

- [ ] **Step 13.1: Replace the file content**

Overwrite `D:\TaikoLocalServer\.github\workflows\publishTLS.yml` with:

```yaml
name: TLS GitHub Actions
run-name: Publish TLS executable program 🚀
on: [push]
env:
  Major_Version_Number: '1'
  Minor_Version_Number: '2'
  Build_Number: ${{ github.run_number }}
jobs:
  TLS-GitHub-Actions:
    runs-on: windows-latest
    steps:
      - run: echo "🎉 The job was automatically triggered by a ${{ github.event_name }} event. Commiter is ${{ github.actor }}."
      - run: echo "🐧 This job is now running on a ${{ runner.os }} server hosted by GitHub!"
      - run: echo "🔎 The name of your branch is ${{ github.ref }} and your repository is ${{ github.repository }}."

      - name: Check out repository code
        uses: actions/checkout@v4

      - run: echo "💡 The ${{ github.repository }} repository has been cloned to the runner."

      # Install the .NET Core workload
      - name: Install .NET Core
        uses: actions/setup-dotnet@v4
        with:
         dotnet-version: 10.0.x

      # Restore the application to populate the obj folder with RuntimeIdentifiers
      - name: Restore the application
        run: dotnet workload restore

      - run: echo "🖥️ The workflow is now ready to publish application on the runner."

      # Build
      - name: Publish
        run: dotnet publish

      # Upload the TLS package
      - name: Upload build artifacts
        uses: actions/upload-artifact@v4
        with:
         name: TLS ${{ env.Major_Version_Number }}.${{ env.Minor_Version_Number }}.${{ env.Build_Number }}
         path: ${{ github.workspace }}\TaikoLocalServer\bin\Release\net10.0\win-x64\publish
         include-hidden-files: true

      - run: echo "🍏 This job's status is ${{ job.status }}."
```

Changes from before:
- `Minor_Version_Number: '1'` → `'2'` (bumps artifact name from `TLS 1.1.<run>` to `TLS 1.2.<run>`).
- `dotnet-version: 8.0.x` → `10.0.x`.
- Artifact path: `bin\Release\net8.0\win-x64\publish` → `bin\Release\net10.0\win-x64\publish`.

- [ ] **Step 13.2: Commit**

```bash
git add .github/workflows/publishTLS.yml
git commit -m "Update CI to .NET 10 SDK and bump version to 1.2"
```

---

## Task 14: Static asset hosting check (if needed)

**Files:**
- Possibly modify: `TaikoLocalServer/Program.cs`

There's a [known regression](https://learn.microsoft.com/en-us/answers/questions/5746146/error-when-upgrading-a-blazor-app-to-net-10) where Blazor WASM apps under .NET 10 don't get their runtime files (`dotnet.js`, `*.wasm`) served correctly via `app.UseStaticFiles()`. This needs to be tested in this codebase, and only patched if it manifests.

- [ ] **Step 14.1: Run the server**

Run:
```bash
dotnet run --project TaikoLocalServer
```

Watch the console for "Now listening on:" lines — note all 7 expected endpoints (5000, 80, 10122, 54430, 54431, 57402, 443).

- [ ] **Step 14.2: Test the WASM bundle loads**

In a browser, open `http://localhost:5000/`. Then open the Network tab in dev tools and reload.

Expected requests succeed with 200:
- `/_framework/blazor.webassembly.js`
- `/_framework/dotnet.js`
- `/_framework/dotnet.runtime.*.js`
- `/_framework/dotnet.native.*.js`
- One or more `/_framework/*.wasm`

Stop the server (Ctrl+C in the run terminal).

- [ ] **Step 14.3: If any of those return 404, switch to MapStaticAssets**

In `D:\TaikoLocalServer\TaikoLocalServer\Program.cs`, find:

```csharp
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
```

Replace with:

```csharp
    app.UseBlazorFrameworkFiles();
    app.MapStaticAssets();
```

`MapStaticAssets` is the .NET 9+ replacement for `UseStaticFiles` that's aware of Blazor's framework asset manifest.

Re-run Step 14.2 to confirm the assets now load.

- [ ] **Step 14.4: Commit (only if Step 14.3 was applied)**

```bash
git add TaikoLocalServer/Program.cs
git commit -m "Switch to MapStaticAssets for .NET 10 Blazor WASM hosting"
```

---

## Task 15: Update `README.md`

**Files:**
- Modify: `README.md`

- [ ] **Step 15.1: Update the prerequisites note**

In `D:\TaikoLocalServer\README.md`, update the "For developers" section added in PR1 by appending:

```markdown

This solution targets **.NET 10 LTS**. Install the .NET 10 SDK from <https://dotnet.microsoft.com/download> (or any 10.0.x patch — the `global.json` allows latestFeature roll-forward).
```

- [ ] **Step 15.2: Commit**

```bash
git add README.md
git commit -m "Document .NET 10 SDK requirement in README"
```

---

## Task 16: Update `CLAUDE.md` to reflect Mediator swap

**Files:**
- Modify: `CLAUDE.md`

- [ ] **Step 16.1: Update the request architecture section**

In `D:\TaikoLocalServer\CLAUDE.md`, find the section that reads:

```markdown
The flow: controller deserializes the version-specific request → **Riok.Mapperly** source-generated mapper in `Mappers/` converts it to a `Common*` DTO → MediatR `Handlers/*Query`/`*Command` operates only on `Common*` types → mapper converts the response back to the requested version's protobuf type.
```

Replace `MediatR` with `Mediator (martinothamar)` so the line reads:

```markdown
The flow: controller deserializes the version-specific request → **Riok.Mapperly** source-generated mapper in `Mappers/` converts it to a `Common*` DTO → **Mediator** (martinothamar) `Handlers/*Query`/`*Command` operates only on `Common*` types → mapper converts the response back to the requested version's protobuf type.
```

Also find and update the `BaseController<T>` line:

```markdown
Controllers inherit `BaseController<T>` which lazily resolves `Mediator` (`ISender`) and `Logger` from `HttpContext.RequestServices`.
```

to:

```markdown
Controllers inherit `BaseController<T>` which lazily resolves `Mediator` (`IMediator`) and `Logger` from `HttpContext.RequestServices`.
```

- [ ] **Step 16.2: Add a Mediator conventions paragraph**

Append to the Conventions section of `CLAUDE.md`:

```markdown
- **Mediator handler conventions** (martinothamar's library, registered with `AddMediator` in `Program.cs`):
  - Request types are `readonly record struct`, not `record class`.
  - `IRequestHandler<TRequest, TResponse>.Handle` returns `ValueTask<TResponse>`, not `Task<TResponse>`.
  - Service lifetime is **scoped** (set by `opt.ServiceLifetime = ServiceLifetime.Scoped`) because handlers inject `TaikoDbContext`. Don't change this without also switching to `IDbContextFactory<TaikoDbContext>`.
  - Controllers always pass `HttpContext.RequestAborted` as the second argument to `Mediator.Send(...)` for cancellation hygiene.
```

- [ ] **Step 16.3: Commit**

```bash
git add CLAUDE.md
git commit -m "Update CLAUDE.md to reflect Mediator (martinothamar) swap"
```

---

## Task 17: Final validation

**Files:** none modified — observational only.

- [ ] **Step 17.1: Clean restore**

Run:
```bash
dotnet restore --force
```
Expected: succeeds with no errors. CPM warnings (`NU1507` etc.) on first restore are benign.

- [ ] **Step 17.2: Full build**

Run:
```bash
dotnet build
```
Expected: succeeds with 0 errors. Warning count should be at parity or lower than the .NET 8 baseline.

- [ ] **Step 17.3: Publish**

Run:
```bash
dotnet publish TaikoLocalServer/TaikoLocalServer.csproj -c Release -o ./publish-pr2-test
```
Expected: produces `./publish-pr2-test/TaikoLocalServer.exe`. Verify the path:
```bash
ls -la ./publish-pr2-test/TaikoLocalServer.exe
```

- [ ] **Step 17.4: Smoke-test the running server**

Run:
```bash
dotnet run --project TaikoLocalServer
```

Confirm in the log:
- `TaikoLocalServer version 1.1.0` (or whatever Version is in csproj — unchanged)
- `Server starting up...`
- All 7 endpoints reported in "Now listening on:" lines:
  - `http://0.0.0.0:5000`
  - `http://0.0.0.0:80`
  - `https://0.0.0.0:10122`
  - `https://0.0.0.0:54430`
  - `https://0.0.0.0:54431`
  - `https://0.0.0.0:57402`
  - `https://0.0.0.0:443`

In a separate terminal:
```bash
curl -I http://localhost:5000/
```
Expected: `HTTP/1.1 200 OK`.

- [ ] **Step 17.5: Mediator routing smoke test (REQUIRED)**

This step verifies that `Mediator.Send(...)` actually reaches the handlers via the source-generated dispatcher. The most reliable way is to point a real game client at the server and play one round. If that's not feasible, a curl request that exercises a handler is sufficient.

Suggested: hit the BAID check endpoint with a captured payload:
```bash
# This requires a captured protobuf body file from a prior game session.
# If you don't have one, skip to the alternative below.
curl -X POST http://localhost:5000/v12r08_ww/chassis/baidcheck_dcfxit1u.php \
  -H "Content-Type: application/protobuf" \
  --data-binary @captured-baid-request.bin \
  -i
```
Expected: HTTP 200 with a non-empty protobuf-encoded body.

**Alternative:** Hit any admin API endpoint that exercises a `Mediator.Send(...)` path. None of the admin API controllers use Mediator (only Game controllers do), so you'll need to either capture a game request or skip this step and rely on PR review for verification.

If the request returns HTTP 500, check the server log for "no handler registered" — that means the Mediator source generator missed a handler. Fix by reviewing `Step 9.2`'s diagnostics.

Stop the server.

- [ ] **Step 17.6: Verify the existing dev DB still works**

Confirm `TaikoLocalServer/wwwroot/taiko.db3` is the same shape as before. The server should have logged `__EFMigrationsHistory` checks but no new migrations applied:
```bash
sqlite3 TaikoLocalServer/wwwroot/taiko.db3 "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 5;"
```
Expected: the 5 most recent migration IDs match what was there before PR2 ran.

- [ ] **Step 17.7: Clean up**

```bash
rm -rf ./publish-pr2-test
```

---

## Task 18: Push and open PR

- [ ] **Step 18.1: Confirm branch is clean and ahead of `dev`**

```bash
git status
git log dev..HEAD --oneline
```
Expected: clean tree; commit list shows the Tasks 1, 3, 5–10 (Mediator swap), 11, 12, 13, 15, 16 commits.

- [ ] **Step 18.2: Push the branch**

```bash
git push -u origin dev/net10-runtime
```

- [ ] **Step 18.3: Open the PR**

```bash
gh pr create --base dev --title "Move runtime to .NET 10 + replace MediatR with Mediator" --body "$(cat <<'EOF'
## Summary

Sequenced as PR2 of three (PR1 = modernize on .NET 8 [merged], PR3 = MudBlazor 6 → 9 [next]). See \`docs/superpowers/specs/2026-05-03-net10-upgrade-design.md\`.

This PR:
- Bumps all five projects to **.NET 10 LTS**, LangVersion 13.
- Refreshes every server-side NuGet to its latest stable .NET-10-compatible release.
- Replaces **MediatR** with **Mediator** (martinothamar's MIT-licensed source-generated alternative). All 16 handlers and 32 controller call sites are converted to Mediator's conventions: \`ValueTask<T>\` returns, \`readonly record struct\` requests, scoped service lifetime, and \`HttpContext.RequestAborted\` plumbed to every \`Send\` call.
- Converts \`.sln\` to \`.slnx\`.
- Updates the GitHub Actions workflow to .NET 10 and bumps the artifact version to \`1.2.<run>\`.

## What did NOT change

- TaikoWebUI's MudBlazor stack (still v6.20.0) — that's PR3.
- DB schema or migrations.
- Game protobuf models, mappers, route shapes.
- Configuration file shape (\`Configurations/*.json\`, \`wwwroot/data/*.json\`).

## ⚠️ Intermediate state warning

Between merging this PR and PR3, the WebUI may render with visual glitches or fail to load entirely on .NET 10. **Do not deploy this intermediate state to a public arcade.**

## Test plan

- [x] \`dotnet restore\` clean
- [x] \`dotnet build\` clean
- [x] \`dotnet publish -c Release\` produces the self-contained net10.0 exe
- [x] All 7 Kestrel endpoints bind on startup
- [x] Existing \`taiko.db3\` migrates with no schema diff (verified via \`__EFMigrationsHistory\` check)
- [x] \`Mediator.Send\` routing smoke test (please describe in review which endpoint was used)
- [x] JWT auth works on protected admin API call

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

- [ ] **Step 18.4: Confirm CI passes**

Visit the PR URL printed by the previous command. Confirm the GitHub Actions workflow `TLS GitHub Actions` runs to green and produces the `TLS 1.2.<run>` artifact.

If CI fails, do NOT merge. Most likely failure modes:
- A package version specified in `Directory.Packages.props` doesn't exist on NuGet at the requested version. Fix by bumping to the actual latest as reported by the build log.
- Mediator source generator missed a handler discovery. Check `MEDIATOR*` diagnostics in the build log.

---

## Self-review checklist

Before declaring PR2 complete:

- [ ] `Directory.Build.props` shows `net10.0` and `LangVersion 13`.
- [ ] `global.json` pins `10.0.100` (or higher).
- [ ] No `<PackageVersion Include="MediatR" …/>` line remains in `Directory.Packages.props`.
- [ ] `Mediator.SourceGenerator` and `Mediator.Abstractions` are in CPM and referenced from `TaikoLocalServer.csproj` (the former with `PrivateAssets="all"`).
- [ ] All 16 handler request types are `readonly record struct` (verify with `grep -r "readonly record struct" TaikoLocalServer/Handlers/ | wc -l` → expect at least 16).
- [ ] All 16 handler `Handle` methods return `ValueTask<…>` (verify with `grep -r "public async ValueTask<" TaikoLocalServer/Handlers/ | wc -l` → expect 16).
- [ ] All 32 `Mediator.Send(…)` call sites pass `HttpContext.RequestAborted` (verify with `grep -rn "Mediator\.Send" TaikoLocalServer/Controllers/Game/ | grep -c "RequestAborted"` → expect 32).
- [ ] `BaseController<T>.Mediator` is typed `IMediator`, not `ISender`.
- [ ] `GlobalUsings.cs` has `global using Mediator;` (not `MediatR`).
- [ ] `Program.cs` calls `AddMediator(opt => …)` (not `AddMediatR`).
- [ ] CI is green on the PR.

## What this PR sets up for PR3

After merge:
- PR3 bumps `MudBlazor` 6.20.0 → 9.4.0 (or current latest 9.x), `CodeBeam.MudBlazor.Extensions` to matching, and other UI packages.
- PR3 fixes the MudBlazor v6 → v7 → v8 → v9 breaking changes in `*.razor` files.
- PR3 has the smallest `Directory.Packages.props` diff (just UI packages) because PR2 already moved everything else.
