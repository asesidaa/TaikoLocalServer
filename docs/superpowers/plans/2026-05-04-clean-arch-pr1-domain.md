# PR1 — Domain Carve-Out Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract a new `TaikoLocalServer.Domain` project containing entity classes, shared enums, and pure-domain constants. No behavior changes; only file relocations + namespace updates + project-reference rewiring + an empty EF Core migration to refresh the model snapshot.

**Architecture:** Domain becomes the only zero-dependency leaf of the dependency graph. `GameDatabase`, `SharedProject`, `TaikoLocalServer`, `TaikoWebUI`, and `LocalSaveModScoreMigrator` all gain a `<ProjectReference>` to it. Entities move out of `GameDatabase/Entities/` (and stay there from EF's perspective via the new project ref). Enums move out of `SharedProject/Enums/`. The catch-all `Constants.cs` is split: domain constants land in Domain; everything else stays in `TaikoLocalServer/Common/Constants.cs` for now (further split happens in PR2).

**Tech Stack:** .NET 10 SDK, ASP.NET Core 10, EF Core 10 (SQLite), `.slnx` solution format, Central Package Management via `Directory.Packages.props`.

**Spec reference:** `docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md` — see §1 (decisions), §2 (project list), §3 ("TaikoLocalServer.Domain" subsection), §6 ("Constants.cs split" table), §7 ("PR1 — Domain carve-out"), §8 risks #1.

---

## How this plan works

This repository has **no automated tests** (per `CLAUDE.md`). The standard TDD bite-sized loop is replaced with **build-and-smoke-verify** loops:

- "Write the failing test" → "Make the change"
- "Run test, expect fail" → (skipped — there's no test)
- "Make test pass" → "`dotnet build` green"
- "Run test, expect pass" → "Run server / smoke endpoint manually"
- "Commit"

Verification commands assume the working directory is the repo root (`D:\TaikoLocalServer\`). All paths in this plan are repository-relative unless absolute paths are needed for tools.

## Pre-conditions

- Branch `dev` is checked out and clean (`git status` reports nothing).
- `dotnet --version` reports a 10.x SDK.
- `dotnet build` from the repo root currently succeeds. **If it doesn't, stop — fix the build first.**
- The current `taiko.db3` (or a copy) is available at `TaikoLocalServer/wwwroot/taiko.db3`. If absent, the migration smoke test will create one fresh, which is also fine.

## Final layout this PR produces

```
Domain/                                  ← NEW project
  Domain.csproj
  Entities/
    AiScoreDatum.cs, AiSectionScoreDatum.cs, Card.cs, Credential.cs,
    DanScoreDatum.cs, DanStageScoreDatum.cs, SongBestDatum.cs,
    SongBestDatumMethods.cs, SongPlayDatum.cs, Token.cs, UserDatum.cs
  Enums/
    CrownType.cs, DanBorderType.cs, DanClearState.cs, DanConditionType.cs,
    DanType.cs, Difficulty.cs, NameLanguage.cs, PlayMode.cs, RandomType.cs,
    ScoreRank.cs, SongGenre.cs
  DomainConstants.cs

GameDatabase/                            ← unchanged folder layout, refs Domain
  GameDatabase.csproj  (now refs Domain; Entities/ folder is empty)
  Context/  (TaikoDbContext.cs uses `using Domain.Entities;`)
  Migrations/  (+ new empty EntityNamespaceMove)

SharedProject/                           ← Enums/ now empty; refs Domain
  SharedProject.csproj  (now refs Domain)
  Models/, Utils/  (unchanged)

TaikoLocalServer/                        ← refs Domain transitively via GameDatabase + SharedProject
  Common/Constants.cs  (unchanged this PR — split happens in PR2)
  GlobalUsings.cs  (Domain.Entities + Domain.Enums replace prior using rows)

TaikoWebUI/                              ← refs Domain transitively via SharedProject; GlobalUsings updated
LocalSaveModScoreMigrator/               ← refs Domain transitively via GameDatabase + SharedProject
```

The folder name `Domain/` matches the project name `Domain` from the csproj. The full project name `TaikoLocalServer.Domain` (per spec §2 naming rule) is realized by `<RootNamespace>` and `<AssemblyName>` overrides in the csproj — folder/csproj basename stay short to keep paths reasonable, but every public type lives under `namespace TaikoLocalServer.Domain.*`.

The actual project list in `.slnx` ends PR1 at 6 projects (5 existing + Domain).

---

## Phase 1.0 — Branch and baseline

### Task 1.0.1: Create the PR1 branch

**Files:** none.

- [ ] **Step 1: Create and check out the PR1 branch**

```bash
git checkout dev
git pull --ff-only origin dev
git checkout -b dev/clean-arch-domain
```

- [ ] **Step 2: Sanity-check the starting state**

```bash
dotnet build
```

Expected: build succeeds, 0 errors, warnings as currently published.

### Task 1.0.2: Capture a route-count baseline

**Files:** none — this is a one-off measurement saved as a comment for later phases.

- [ ] **Step 1: Start the server briefly and record route count**

```bash
dotnet run --project TaikoLocalServer
```

Watch the startup logs. The number to remember is implicit in `app.MapControllers()` — there's no log line for it today. **Skip this step in PR1 if the server runs fine without errors.** PR2 introduces an explicit route-count log; PR1 only needs "server starts and migrates DB".

Stop the server with `Ctrl+C` once startup completes (you should see `Now listening on:` lines for the configured Kestrel ports).

---

## Phase 1.1 — Create the Domain project skeleton

### Task 1.1.1: Create the Domain folder and csproj

**Files:**
- Create: `Domain/Domain.csproj`

- [ ] **Step 1: Create the directory**

```bash
mkdir Domain
mkdir Domain/Entities
mkdir Domain/Enums
```

- [ ] **Step 2: Write the csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Domain</RootNamespace>
    <AssemblyName>TaikoLocalServer.Domain</AssemblyName>
  </PropertyGroup>

</Project>
```

Note: no `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`, or `<LangVersion>` — those come from `Directory.Build.props` already.

Save as `Domain/Domain.csproj`.

- [ ] **Step 3: Add Domain to the solution**

Edit `TaikoLocalServer.slnx`. After the existing `<Project Path="GameDatabase/GameDatabase.csproj" />` line, add:

```xml
  <Project Path="Domain/Domain.csproj" />
```

The complete file should now read:

```xml
<Solution>
  <Project Path="Domain/Domain.csproj" />
  <Project Path="GameDatabase/GameDatabase.csproj" />
  <Project Path="LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj" />
  <Project Path="SharedProject/SharedProject.csproj" />
  <Project Path="TaikoLocalServer/TaikoLocalServer.csproj" />
  <Project Path="TaikoWebUI/TaikoWebUI.csproj" />
</Solution>
```

- [ ] **Step 4: Verify Domain builds (empty)**

```bash
dotnet build Domain/Domain.csproj
```

Expected: build succeeds. There are no source files yet — this confirms the csproj is well-formed.

- [ ] **Step 5: Verify the solution still builds**

```bash
dotnet build
```

Expected: 6 projects build, 0 errors.

- [ ] **Step 6: Commit**

```bash
git add Domain/Domain.csproj TaikoLocalServer.slnx
git commit -m "PR1.1: scaffold empty TaikoLocalServer.Domain project"
```

---

## Phase 1.2 — Move entities

The 11 entity files move verbatim from `GameDatabase/Entities/` to `Domain/Entities/`. The only change is the namespace: `GameDatabase.Entities` → `TaikoLocalServer.Domain.Entities`.

### Task 1.2.1: Move all entity files in one batch

**Files:**
- Move: `GameDatabase/Entities/*.cs` → `Domain/Entities/*.cs` (11 files)

The 11 files:

```
AiScoreDatum.cs, AiSectionScoreDatum.cs, Card.cs, Credential.cs,
DanScoreDatum.cs, DanStageScoreDatum.cs, SongBestDatum.cs,
SongBestDatumMethods.cs, SongPlayDatum.cs, Token.cs, UserDatum.cs
```

- [ ] **Step 1: Move the files (preserves git history)**

```bash
git mv GameDatabase/Entities/AiScoreDatum.cs        Domain/Entities/AiScoreDatum.cs
git mv GameDatabase/Entities/AiSectionScoreDatum.cs Domain/Entities/AiSectionScoreDatum.cs
git mv GameDatabase/Entities/Card.cs                Domain/Entities/Card.cs
git mv GameDatabase/Entities/Credential.cs          Domain/Entities/Credential.cs
git mv GameDatabase/Entities/DanScoreDatum.cs       Domain/Entities/DanScoreDatum.cs
git mv GameDatabase/Entities/DanStageScoreDatum.cs  Domain/Entities/DanStageScoreDatum.cs
git mv GameDatabase/Entities/SongBestDatum.cs       Domain/Entities/SongBestDatum.cs
git mv GameDatabase/Entities/SongBestDatumMethods.cs Domain/Entities/SongBestDatumMethods.cs
git mv GameDatabase/Entities/SongPlayDatum.cs       Domain/Entities/SongPlayDatum.cs
git mv GameDatabase/Entities/Token.cs               Domain/Entities/Token.cs
git mv GameDatabase/Entities/UserDatum.cs           Domain/Entities/UserDatum.cs
```

- [ ] **Step 2: Update the namespace declaration in every moved file**

Each moved file currently starts with `namespace GameDatabase.Entities;`. Replace with `namespace TaikoLocalServer.Domain.Entities;`.

For each file in `Domain/Entities/*.cs`, find the line:

```csharp
namespace GameDatabase.Entities;
```

Replace with:

```csharp
namespace TaikoLocalServer.Domain.Entities;
```

If a file uses block-style namespace (`namespace GameDatabase.Entities { ... }`), update to `namespace TaikoLocalServer.Domain.Entities { ... }` and preserve braces.

If any file `using`s another local entity (e.g., `SongBestDatumMethods.cs` calls into `SongBestDatum`), no `using` row is needed because they're in the same namespace.

- [ ] **Step 3: Verify Domain builds with the new types**

```bash
dotnet build Domain/Domain.csproj
```

Expected: build succeeds. If a file uses `Microsoft.EntityFrameworkCore.*` types (e.g., `[Index]` attribute) or `SharedProject.Enums.*`, the build fails here — handle in next steps.

- [ ] **Step 4: Add EntityFrameworkCore reference to Domain (if needed)**

Inspect any error messages from Step 3. If errors mention `IndexAttribute`, `OwnsMany`, or other EF types **used as attributes on entity classes**, edit `Domain/Domain.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Domain</RootNamespace>
    <AssemblyName>TaikoLocalServer.Domain</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
  </ItemGroup>

</Project>
```

Per spec §3 (Domain "No `<PackageReference>`"), entities should be POCOs. **If they reference EF attributes, leave EF in Domain for this PR and note it; the cleanup is out of scope for PR1.** Re-evaluating fluent vs attribute config is a follow-up.

Re-run `dotnet build Domain/Domain.csproj`. Expected: succeeds.

- [ ] **Step 5: Add SharedProject.Enums dependency wiring**

If entity files use `SharedProject.Enums.*` (likely — e.g., `Difficulty`, `CrownType`), the file currently relies on a `using SharedProject.Enums;` row (or the global one in `GameDatabase/obj/.../GameDatabase.GlobalUsings.g.cs` which references nothing in Shared). **Search each entity file for usages of enum types** and confirm an explicit `using SharedProject.Enums;` row at the top.

In PR1, enums haven't moved yet (Phase 1.3 below), so the `using SharedProject.Enums;` directive points at SharedProject's existing namespace. After Phase 1.3 these `using`s become `using TaikoLocalServer.Domain.Enums;`. For now, add the reference Domain → SharedProject:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Domain</RootNamespace>
    <AssemblyName>TaikoLocalServer.Domain</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
  </ItemGroup>

</Project>
```

Re-run `dotnet build Domain/Domain.csproj`. Expected: succeeds.

> **Note:** This temporarily makes Domain ref SharedProject — a circular-feeling situation, but Domain → SharedProject is fine because Phase 1.3 reverses it (SharedProject → Domain) by moving enums into Domain and dropping this ref. Don't commit yet — this is a working-tree intermediate.

- [ ] **Step 6: Wire GameDatabase to ref Domain**

Edit `GameDatabase/GameDatabase.csproj`. Replace:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <ItemGroup>
        <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="BCrypt.Net-Next" />
        <PackageReference Include="EntityFrameworkCore.Exceptions.Sqlite" />
        <PackageReference Include="Microsoft.EntityFrameworkCore" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
    </ItemGroup>

</Project>
```

With:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <ItemGroup>
        <ProjectReference Include="..\Domain\Domain.csproj" />
        <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="BCrypt.Net-Next" />
        <PackageReference Include="EntityFrameworkCore.Exceptions.Sqlite" />
        <PackageReference Include="Microsoft.EntityFrameworkCore" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
    </ItemGroup>

</Project>
```

(`Domain` ref added at the top of the same `ItemGroup`.)

- [ ] **Step 7: Update GameDatabase using statements**

Two files in `GameDatabase/Context/` previously declared `using GameDatabase.Entities;` (or relied on it implicitly via the project's own root namespace).

Edit `GameDatabase/Context/TaikoDbContext.cs`: at the top of the file, replace any `using GameDatabase.Entities;` with `using TaikoLocalServer.Domain.Entities;`. If no such line exists (because the entities lived in the same project namespace), **add** `using TaikoLocalServer.Domain.Entities;` after the other top-of-file `using`s.

Repeat for `GameDatabase/Context/TaikoDbContextPartial.cs`.

- [ ] **Step 8: Update Migrations folder using statements**

Every file under `GameDatabase/Migrations/*.cs` and `GameDatabase/Migrations/*.Designer.cs` may reference entity types via fully-qualified `GameDatabase.Entities.<EntityName>` strings inside `[Migration]`/`ModelBuilder` calls. These are typically in metadata strings, not C# `using`s, so a string-level refactor is needed.

Use a project-wide find-and-replace **inside `GameDatabase/Migrations/` only**:

- Find: `GameDatabase.Entities.`
- Replace with: `TaikoLocalServer.Domain.Entities.`

If your editor doesn't scope replacements, run from a shell:

```bash
# PowerShell on Windows:
Get-ChildItem -Path GameDatabase/Migrations -Filter *.cs -Recurse | ForEach-Object {
  (Get-Content $_.FullName) -replace 'GameDatabase\.Entities\.', 'TaikoLocalServer.Domain.Entities.' |
    Set-Content $_.FullName
}
```

Or, in bash (git-bash):

```bash
find GameDatabase/Migrations -name '*.cs' -print0 | \
  xargs -0 sed -i 's/GameDatabase\.Entities\./TaikoLocalServer.Domain.Entities./g'
```

These migration files are also regenerated by Phase 1.5's empty migration, so getting them syntactically right is sufficient — EF will overwrite the snapshot.

- [ ] **Step 9: Verify GameDatabase still builds**

```bash
dotnet build GameDatabase/GameDatabase.csproj
```

Expected: build succeeds.

- [ ] **Step 10: Verify the whole solution builds**

```bash
dotnet build
```

Expected: success. **The TaikoLocalServer project will likely fail** because of `using GameDatabase.Entities;` in `GlobalUsings.cs` and other places — those updates are in Task 1.2.2.

If TaikoLocalServer fails with errors like "type or namespace `Entities` does not exist in `GameDatabase`", that's the expected state — proceed to 1.2.2.

If GameDatabase itself fails, **stop and fix** before continuing.

- [ ] **Step 11: Commit**

```bash
git add Domain/Entities Domain/Domain.csproj GameDatabase/GameDatabase.csproj GameDatabase/Context GameDatabase/Migrations
git commit -m "PR1.2.1: move entities into TaikoLocalServer.Domain"
```

### Task 1.2.2: Update consumers of `GameDatabase.Entities`

**Files:**
- Modify: `TaikoLocalServer/GlobalUsings.cs`
- Modify: any source file under `TaikoLocalServer/` that has explicit `using GameDatabase.Entities;`
- Modify: `LocalSaveModScoreMigrator/Program.cs` (and other files) if they reference `GameDatabase.Entities.*`
- Modify: `TaikoWebUI/GlobalUsings.cs` if it references entities (it shouldn't — WebUI uses ViewModels — but verify)

- [ ] **Step 1: Find all consumers**

```bash
# bash / git-bash
grep -rln "GameDatabase\.Entities" TaikoLocalServer TaikoWebUI LocalSaveModScoreMigrator SharedProject
```

(or `Select-String -Path ... -Pattern "GameDatabase\.Entities" -List` in PowerShell)

Expected matches (approximate): `TaikoLocalServer/GlobalUsings.cs`, possibly some handlers / services that explicitly import.

- [ ] **Step 2: Update `TaikoLocalServer/GlobalUsings.cs`**

The current file is:

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

Replace the `global using GameDatabase.Entities;` line with:

```csharp
global using TaikoLocalServer.Domain.Entities;
```

Final file content:

```csharp
// Global using directives

global using TaikoLocalServer.Domain.Entities;
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

(`SharedProject.Enums` row stays — Phase 1.3 will replace it.)

- [ ] **Step 3: Make TaikoLocalServer ref Domain explicitly**

Even though Domain is reachable transitively via GameDatabase/SharedProject, an explicit ref is required for `global using TaikoLocalServer.Domain.Entities;` to resolve in the compilation.

Edit `TaikoLocalServer/TaikoLocalServer.csproj`. In the `<ItemGroup>` containing the existing `<ProjectReference>` lines, add `Domain`:

```xml
  <ItemGroup>
    <ProjectReference Include="..\Domain\Domain.csproj" />
    <ProjectReference Include="..\GameDatabase\GameDatabase.csproj" />
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    <ProjectReference Include="..\TaikoWebUI\TaikoWebUI.csproj" />
  </ItemGroup>
```

- [ ] **Step 4: Update other in-tree consumers**

For every file matched by Step 1 except `TaikoLocalServer/GlobalUsings.cs`, replace `using GameDatabase.Entities;` (or `GameDatabase.Entities.`-prefixed type references) with the Domain equivalent.

Per the grep in step 1, expected files include any handler or service file that doesn't get the entity types from the global using (rare). Inspect each match and update.

- [ ] **Step 5: Verify the solution builds**

```bash
dotnet build
```

Expected: 6 projects build, 0 errors. If errors persist, they're typically:
- Missing `using TaikoLocalServer.Domain.Entities;` in a file that explicitly used `GameDatabase.Entities.X` → fix file by file.
- `LocalSaveModScoreMigrator` references `GameDatabase.Entities.*` → repeat the find-replace there.

For LocalSaveModScoreMigrator specifically, if its csproj already refs GameDatabase, the type is reachable transitively — only the `using` needs updating.

If `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj` does NOT yet ref Domain, leave it as-is for now — its ref update is part of Task 1.6.

- [ ] **Step 6: Commit**

```bash
git add TaikoLocalServer/GlobalUsings.cs TaikoLocalServer/TaikoLocalServer.csproj
# plus any other files touched in Step 4
git commit -m "PR1.2.2: repoint TaikoLocalServer at TaikoLocalServer.Domain.Entities"
```

---

## Phase 1.3 — Move enums

The 11 enum files move verbatim from `SharedProject/Enums/` to `Domain/Enums/`. Namespace: `SharedProject.Enums` → `TaikoLocalServer.Domain.Enums`.

### Task 1.3.1: Move all enum files in one batch

**Files:**
- Move: `SharedProject/Enums/*.cs` → `Domain/Enums/*.cs` (11 files)

The 11 files:

```
CrownType.cs, DanBorderType.cs, DanClearState.cs, DanConditionType.cs,
DanType.cs, Difficulty.cs, NameLanguage.cs, PlayMode.cs, RandomType.cs,
ScoreRank.cs, SongGenre.cs
```

- [ ] **Step 1: Move the files (preserves git history)**

```bash
git mv SharedProject/Enums/CrownType.cs        Domain/Enums/CrownType.cs
git mv SharedProject/Enums/DanBorderType.cs    Domain/Enums/DanBorderType.cs
git mv SharedProject/Enums/DanClearState.cs    Domain/Enums/DanClearState.cs
git mv SharedProject/Enums/DanConditionType.cs Domain/Enums/DanConditionType.cs
git mv SharedProject/Enums/DanType.cs          Domain/Enums/DanType.cs
git mv SharedProject/Enums/Difficulty.cs       Domain/Enums/Difficulty.cs
git mv SharedProject/Enums/NameLanguage.cs     Domain/Enums/NameLanguage.cs
git mv SharedProject/Enums/PlayMode.cs         Domain/Enums/PlayMode.cs
git mv SharedProject/Enums/RandomType.cs       Domain/Enums/RandomType.cs
git mv SharedProject/Enums/ScoreRank.cs        Domain/Enums/ScoreRank.cs
git mv SharedProject/Enums/SongGenre.cs        Domain/Enums/SongGenre.cs
```

- [ ] **Step 2: Update namespace in every moved file**

In each `Domain/Enums/*.cs`, change:

```csharp
namespace SharedProject.Enums;
```

to:

```csharp
namespace TaikoLocalServer.Domain.Enums;
```

(Preserve block-style braces if any file uses them — none do based on the current code style, but check.)

- [ ] **Step 3: Drop Domain → SharedProject reference (introduced in Task 1.2.1 Step 5)**

Edit `Domain/Domain.csproj`. The `<ItemGroup>` with `ProjectReference Include="..\SharedProject\SharedProject.csproj"` should be removed entirely:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Domain</RootNamespace>
    <AssemblyName>TaikoLocalServer.Domain</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
  </ItemGroup>

</Project>
```

(EF Core ref kept from Task 1.2.1; remove if Step 6 confirmed entities don't need it.)

- [ ] **Step 4: Add SharedProject → Domain reference**

Edit `SharedProject/SharedProject.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <ItemGroup>
      <ProjectReference Include="..\Domain\Domain.csproj" />
    </ItemGroup>

    <ItemGroup>
      <PackageReference Include="Throw" />
    </ItemGroup>

</Project>
```

This way, anything that previously got enums from `SharedProject` continues to compile because SharedProject's models that reference enums see them via the transitive Domain ref.

- [ ] **Step 5: Update entity files' `using` rows**

Now that enums live in Domain, entities (which moved in Phase 1.2) need:

```csharp
using TaikoLocalServer.Domain.Enums;
```

For every `Domain/Entities/*.cs` file, find any existing `using SharedProject.Enums;` and replace with `using TaikoLocalServer.Domain.Enums;`. If the file relies on the `global using SharedProject.Enums;` from `TaikoLocalServer/GlobalUsings.cs`, the entity files won't compile from Domain (Domain doesn't see TaikoLocalServer's globals). Add the explicit using row.

- [ ] **Step 6: Build Domain alone**

```bash
dotnet build Domain/Domain.csproj
```

Expected: success. If "type or namespace `Difficulty`/`CrownType`/etc. could not be found" — go back and add the `using TaikoLocalServer.Domain.Enums;` row to the offending entity file.

- [ ] **Step 7: Update consumers of `SharedProject.Enums`**

Find them:

```bash
grep -rln "SharedProject\.Enums" TaikoLocalServer SharedProject TaikoWebUI LocalSaveModScoreMigrator GameDatabase Domain
```

Replace `using SharedProject.Enums;` with `using TaikoLocalServer.Domain.Enums;` and `SharedProject.Enums.X` with `TaikoLocalServer.Domain.Enums.X`.

Key locations:
- `TaikoLocalServer/GlobalUsings.cs` — change `global using SharedProject.Enums;` to `global using TaikoLocalServer.Domain.Enums;`.
- `TaikoWebUI/GlobalUsings.cs` — same swap if present.
- Any `SharedProject/Models/*.cs` that did `using SharedProject.Enums;` (PlaySetting.cs, SongBestData.cs, etc.) — swap to `using TaikoLocalServer.Domain.Enums;`.
- Any `TaikoLocalServer/Handlers/*.cs`, `Mappers/*.cs`, `Services/*.cs`, `Controllers/*.cs` files with explicit `using SharedProject.Enums;` rows.

- [ ] **Step 8: Verify the solution builds**

```bash
dotnet build
```

Expected: success.

If errors come back, common causes:
1. A WebUI razor file (`.razor`) uses an enum without a `@using` directive. Search `TaikoWebUI/**/*.razor` for enum names; add `@using TaikoLocalServer.Domain.Enums` to `_Imports.razor` if needed.
2. A test or migration file references `SharedProject.Enums.X` directly — sed/regex one more pass.

- [ ] **Step 9: Commit**

```bash
git add Domain SharedProject Domain/Domain.csproj SharedProject/SharedProject.csproj TaikoLocalServer/GlobalUsings.cs
# plus any consumers updated in Step 7
git commit -m "PR1.3: move enums into TaikoLocalServer.Domain"
```

---

## Phase 1.4 — Add DomainConstants

Per spec §6 ("Constants.cs split"), the **only** constants that move to Domain in PR1 are:

- `MusicIdMax`
- `MusicIdMaxExpanded`
- `ShopVerupMasterType`, `DanVerupMasterType`, `GaidenVerupMasterType`, `FolderVerupMasterType`, `IntroVerupMasterType`
- `FunctionIdDaniFolderAvailable`, `FunctionIdDaniAvailable`, `FunctionIdAiBattleAvailable`

The other constants in `TaikoLocalServer/Common/Constants.cs` (`DateTimeFormat`, `DefaultDbName`, `*BaseName`) **stay where they are** — their moves happen in PR2/PR3 along with the projects that consume them.

### Task 1.4.1: Create DomainConstants.cs

**Files:**
- Create: `Domain/DomainConstants.cs`
- Modify: `TaikoLocalServer/Common/Constants.cs` (remove migrated constants)

- [ ] **Step 1: Write Domain/DomainConstants.cs**

```csharp
namespace TaikoLocalServer.Domain;

public static class DomainConstants
{
    public const int MusicIdMax = 1600;

    public const int MusicIdMaxExpanded = 9000;

    // Verup1
    public const uint ShopVerupMasterType = 104;

    // Verup2
    public const uint DanVerupMasterType = 101;
    public const uint GaidenVerupMasterType = 102;
    public const uint FolderVerupMasterType = 103;
    public const uint IntroVerupMasterType = 105;

    public const uint FunctionIdDaniFolderAvailable = 1;
    public const uint FunctionIdDaniAvailable = 2;
    public const uint FunctionIdAiBattleAvailable = 3;
}
```

- [ ] **Step 2: Trim Constants.cs to keep only the not-yet-migrated entries**

Edit `TaikoLocalServer/Common/Constants.cs`. After PR1 it should read:

```csharp
namespace TaikoLocalServer.Common;

public static class Constants
{
	public const string DateTimeFormat = "yyyyMMddHHmmss";

	public const string DefaultDbName = "taiko.db3";

	public const string MusicInfoBaseName = "musicinfo";
	public const string WordlistBaseName = "wordlist";
	public const string MusicOrderBaseName = "music_order";
	public const string DonCosRewardBaseName = "don_cos_reward";
	public const string ShougouBaseName = "shougou";
	public const string NeiroBaseName = "neiro";
}
```

Tabs/spaces preserved as in the original.

- [ ] **Step 3: Repoint consumers of the migrated constants**

Find them:

```bash
grep -rln "Constants\.MusicIdMax\|Constants\.MusicIdMaxExpanded\|Constants\.ShopVerupMasterType\|Constants\.DanVerupMasterType\|Constants\.GaidenVerupMasterType\|Constants\.FolderVerupMasterType\|Constants\.IntroVerupMasterType\|Constants\.FunctionIdDaniFolderAvailable\|Constants\.FunctionIdDaniAvailable\|Constants\.FunctionIdAiBattleAvailable" \
  TaikoLocalServer TaikoWebUI SharedProject GameDatabase LocalSaveModScoreMigrator
```

For each match, change the qualifier from `Constants.X` to `DomainConstants.X`. If the file already has `using TaikoLocalServer.Common;` (or relies on `global using TaikoLocalServer.Common;`), it needs an additional `using TaikoLocalServer.Domain;` row (or simply replace the global). Decide per file.

Recommended: one global change in `TaikoLocalServer/GlobalUsings.cs`. Add:

```csharp
global using TaikoLocalServer.Domain;
```

immediately after the existing `global using TaikoLocalServer.Common;` line. Then the inline references `DomainConstants.MusicIdMax` will resolve everywhere TaikoLocalServer source compiles.

For TaikoWebUI / SharedProject / others, add `using TaikoLocalServer.Domain;` per file as needed.

- [ ] **Step 4: Verify the solution builds**

```bash
dotnet build
```

Expected: success.

- [ ] **Step 5: Commit**

```bash
git add Domain/DomainConstants.cs TaikoLocalServer/Common/Constants.cs TaikoLocalServer/GlobalUsings.cs
# plus any consumer updates from Step 3
git commit -m "PR1.4: split DomainConstants into TaikoLocalServer.Domain"
```

---

## Phase 1.5 — EF Core empty migration to refresh model snapshot

Spec §7 PR1 + Risk #1: when entities change namespace, EF Core's `TaikoDbContextModelSnapshot.cs` needs regeneration so the snapshot's fully-qualified type strings match the new namespace. We do this with an **empty migration** — `Up`/`Down` should be no-ops; only the snapshot file changes.

### Task 1.5.1: Generate the EntityNamespaceMove migration

**Files:**
- Create: `GameDatabase/Migrations/<timestamp>_EntityNamespaceMove.cs` (empty Up/Down)
- Create: `GameDatabase/Migrations/<timestamp>_EntityNamespaceMove.Designer.cs` (regenerated)
- Modify: `GameDatabase/Migrations/TaikoDbContextModelSnapshot.cs` (regenerated)

- [ ] **Step 1: Generate the migration**

```bash
dotnet ef migrations add EntityNamespaceMove \
  --project GameDatabase \
  --startup-project TaikoLocalServer
```

Expected output:

```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

The new files: `GameDatabase/Migrations/<UTC-timestamp>_EntityNamespaceMove.cs` + `.Designer.cs`. The snapshot is regenerated.

- [ ] **Step 2: Inspect the migration's Up/Down**

Open the `.cs` file (NOT the `.Designer.cs`):

```bash
# Replace with the actual generated filename:
cat GameDatabase/Migrations/*_EntityNamespaceMove.cs
```

Both `Up(MigrationBuilder migrationBuilder)` and `Down(MigrationBuilder migrationBuilder)` method bodies **must be empty** (i.e., `{ }` or contain only whitespace/comments).

If they're empty: the namespace change had no schema implications, as expected.

If they're **not empty** (i.e., EF emitted DROPs/CREATEs/ALTER TABLE etc.): something else changed — perhaps an entity's `[Column]`, `[Index]`, or relationship attribute moved or fluent config now binds differently. Stop and investigate. Likely fix: re-check that no entity attributes were dropped during the namespace edit.

- [ ] **Step 3: Verify the migration produces no SQL**

```bash
dotnet ef migrations script <preceding-migration-id> EntityNamespaceMove \
  --project GameDatabase \
  --startup-project TaikoLocalServer
```

Replace `<preceding-migration-id>` with the immediately-prior migration name (the one above `EntityNamespaceMove` in chronological order — based on current state, this is `AddOptionSettingColumnInSongPlayData`).

Expected output: the script contains an empty migration record like:

```sql
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('<timestamp>_EntityNamespaceMove', '...');
```

with no `CREATE` / `DROP` / `ALTER` between PRAGMA statements. If it does contain DDL, **back out** (`dotnet ef migrations remove --project GameDatabase --startup-project TaikoLocalServer`) and revisit entity changes.

- [ ] **Step 4: Apply the migration to a copy of the live DB**

```bash
# Make a backup first
cp TaikoLocalServer/wwwroot/taiko.db3 TaikoLocalServer/wwwroot/taiko.db3.pr1-backup
```

Then run the server briefly:

```bash
dotnet run --project TaikoLocalServer
```

Watch the logs. EF Core should log `Applying migration '<timestamp>_EntityNamespaceMove'.` once. After "`Now listening on:`" lines appear, stop the server with `Ctrl+C`.

If the server fails with a model-snapshot mismatch error, run `dotnet ef migrations remove ...` and inspect what entity attribute changed.

- [ ] **Step 5: Commit**

```bash
git add GameDatabase/Migrations/*_EntityNamespaceMove.cs \
        GameDatabase/Migrations/*_EntityNamespaceMove.Designer.cs \
        GameDatabase/Migrations/TaikoDbContextModelSnapshot.cs
git commit -m "PR1.5: empty EntityNamespaceMove migration to refresh model snapshot"
```

---

## Phase 1.6 — LocalSaveModScoreMigrator + WebUI: ref Domain explicitly

The `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj` and `TaikoWebUI/TaikoWebUI.csproj` may already see Domain transitively, but per spec §2's project-list table they should ref Domain directly so the dependency is intentional.

### Task 1.6.1: LocalSaveModScoreMigrator csproj refs

**Files:**
- Modify: `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`

- [ ] **Step 1: Read the current csproj**

```bash
cat LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj
```

- [ ] **Step 2: Add Domain ProjectReference**

In the `<ItemGroup>` containing existing `<ProjectReference>` entries (likely `GameDatabase` and/or `SharedProject`), add at the top:

```xml
    <ProjectReference Include="..\Domain\Domain.csproj" />
```

If no `<ItemGroup>` for project refs exists, create one.

- [ ] **Step 3: Verify the migrator builds**

```bash
dotnet build LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj
```

Expected: success.

- [ ] **Step 4: Verify the migrator still starts**

```bash
dotnet run --project LocalSaveModScoreMigrator
```

Expected: argparse output ("Required argument missing for option..." or System.CommandLine help banner). Press Ctrl+C if it hangs.

This confirms the migrator at least starts up — full E2E (`--save-file-path` + `--baid`) verification is out of scope for PR1.

### Task 1.6.2: TaikoWebUI csproj refs (read-only verification)

**Files:** `TaikoWebUI/TaikoWebUI.csproj` (verify only — likely no edit needed)

- [ ] **Step 1: Read the csproj**

```bash
cat TaikoWebUI/TaikoWebUI.csproj
```

If it already refs `..\SharedProject\SharedProject.csproj`, that's enough — Domain is transitive. **No explicit Domain ref needed in PR1**; PR4 swaps SharedProject → Contracts.AdminApi and Domain ref will become explicit there.

- [ ] **Step 2: Confirm `TaikoWebUI/_Imports.razor` and `TaikoWebUI/GlobalUsings.cs` still resolve enums**

Open both files and look for any `using SharedProject.Enums;` lines. After Phase 1.3 those need to be `using TaikoLocalServer.Domain.Enums;`.

If a `using TaikoLocalServer.Domain.Enums;` row is missing, add it. If `_Imports.razor` has `@using SharedProject.Enums`, change to `@using TaikoLocalServer.Domain.Enums`.

- [ ] **Step 3: Verify WebUI builds**

```bash
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

Expected: success. (Razor compilation errors on missing types appear here.)

- [ ] **Step 4: Commit consumer updates (if any)**

```bash
# Only if Step 2 produced edits:
git add TaikoWebUI/_Imports.razor TaikoWebUI/GlobalUsings.cs LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj
git commit -m "PR1.6: explicit Domain ref in migrator + repoint WebUI imports at Domain.Enums"
```

If Step 2 produced no edits, just commit the migrator csproj alone:

```bash
git add LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj
git commit -m "PR1.6: explicit Domain ref in LocalSaveModScoreMigrator"
```

---

## Phase 1.7 — Final smoke gate

Per spec §7 PR1 verification gates:

### Task 1.7.1: Run the PR1 smoke gates

**Files:** none.

- [ ] **Step 1: Full solution build**

```bash
dotnet build
```

Expected: 6 projects build, 0 errors. Warnings should match the pre-PR baseline (no new ones).

- [ ] **Step 2: Migration script is empty SQL (no-op)**

Re-run from Phase 1.5 Step 3 if the migration filename changed:

```bash
dotnet ef migrations script $(prior-id) EntityNamespaceMove \
  --project GameDatabase \
  --startup-project TaikoLocalServer
```

Expected: output contains the `__EFMigrationsHistory` insert and no DDL.

- [ ] **Step 3: Server starts and migrates**

```bash
dotnet run --project TaikoLocalServer
```

Watch for:
- `Server starting up...`
- EF Core "Applying migration ... EntityNamespaceMove" (or already-applied — depending on Phase 1.5 Step 4 state)
- `IGameDataCatalog` initialization (currently `IGameDataService.InitializeAsync`)
- Multiple `Now listening on: ...` lines for the configured Kestrel ports

If anything throws on startup, **stop and fix**.

- [ ] **Step 4: Hand-test one game endpoint**

In a second terminal:

```bash
# Health-style ping at one of the Kestrel ports — adapt to your Kestrel.json bindings.
# The /sys/servlet/PowerOn endpoint is a useful smoke target because it doesn't need
# a valid baid; it just exercises middleware + decompression + serializer routing.
curl -X POST http://localhost:80/sys/servlet/PowerOn \
  --data 'mainid=AAVE-01A12345678&serial=ATKK00&placeid=&storeid=&countrycd=JPN&dvcid=' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -i
```

Expected: HTTP 200 with the PowerOn response payload (binary). If 404 or 5xx, **stop and investigate** — likely a routing/middleware issue from a missed namespace update.

- [ ] **Step 5: Hand-test one admin endpoint**

```bash
# With AuthenticationRequired: false (default in Configurations/AuthSettings.json)
curl http://localhost:5000/api/users -i
```

Expected: HTTP 200, JSON list of users (or `[]` if none). If 404, check route mapping; if 5xx, check the AdminApi controller path.

- [ ] **Step 6: Blazor admin UI loads**

Open `http://localhost:5000/` in a browser. The admin UI (Dashboard) should render. Check the browser's DevTools Console: no red errors. (Yellow / hot-reload warnings are fine.)

- [ ] **Step 7: Stop the server**

`Ctrl+C` in the server terminal.

---

## Phase 1.8 — Push and open PR1

### Task 1.8.1: Restore the backup if all gates passed

**Files:** none.

- [ ] **Step 1: Confirm gates passed**

If any of Phase 1.7 step 1-6 failed, **DO NOT proceed**. Fix before pushing.

- [ ] **Step 2: (optional) Discard or keep the backup DB**

If `taiko.db3.pr1-backup` was created and gates passed, the live DB is fine — you can:

```bash
# Either keep the backup as-is (not tracked by git):
ls TaikoLocalServer/wwwroot/taiko.db3.pr1-backup

# Or remove it:
rm TaikoLocalServer/wwwroot/taiko.db3.pr1-backup
```

This file is gitignored (`*.db3.pr1-backup` matches no pattern by default — verify with `git status`). If `git status` shows it as untracked, leave it untracked or remove.

### Task 1.8.2: Push and create PR

**Files:** none.

- [ ] **Step 1: Confirm working tree is clean**

```bash
git status
```

Expected: "nothing to commit, working tree clean" plus possibly the `taiko.db3.pr1-backup` untracked entry which is fine.

- [ ] **Step 2: Push the branch**

```bash
git push -u origin dev/clean-arch-domain
```

- [ ] **Step 3: Open PR (only if user authorized)**

The user has not pre-authorized PR creation. **Ask the user** before running `gh pr create`. If approved, use:

```bash
gh pr create --title "Clean architecture refactor PR1: Domain carve-out" --body "$(cat <<'EOF'
## Summary

Extracts a new `TaikoLocalServer.Domain` project containing entity classes, shared enums, and pure-domain constants. No behavior changes; namespace updates + project-reference rewiring + an empty EF Core migration to refresh the model snapshot.

Spec: docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md (§7 PR1)

## Test plan

- [x] `dotnet build` green across the 6-project solution
- [x] `dotnet ef migrations script` for `EntityNamespaceMove` produces no DDL
- [x] Server starts; existing `taiko.db3` migrates; `Now listening on:` for all Kestrel ports
- [x] One game endpoint hand-tested (PowerOn)
- [x] One admin endpoint hand-tested (`GET /api/users` with auth off)
- [x] Blazor admin UI loads, no console errors

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)" --base dev
```

---

## Risk register specific to PR1

From spec §8:

| # | Risk | Mitigation |
|---|---|---|
| 1 | EF Core model snapshot regen produces non-empty migration when entities move namespaces | Add the empty `EntityNamespaceMove` migration intentionally (Phase 1.5). Inspect `Up/Down` are empty (Step 2). If not, back out and hand-edit the snapshot instead. |

If Phase 1.5 Step 2 reveals a non-empty Up/Down, the most likely cause is an entity attribute (`[Index]`, `[Column]`) inadvertently dropped during the namespace edit. Inspect the diff of every `Domain/Entities/*.cs` against its prior `GameDatabase/Entities/*.cs` form (use `git log --follow --all -p -- 'Domain/Entities/UserDatum.cs'` etc.) to find the culprit.

If a non-empty migration is intentional and acceptable (e.g., a fluent-config change snuck in), name it more descriptively before committing — `EntityNamespaceMove` should remain empty by convention.

---

## Definition of done for PR1

- [ ] All 6 projects build via `dotnet build` from repo root with 0 errors.
- [ ] `Domain/` contains 11 entity files, 11 enum files, `DomainConstants.cs`, and `Domain.csproj`.
- [ ] `GameDatabase/Entities/` is empty.
- [ ] `SharedProject/Enums/` is empty.
- [ ] `TaikoLocalServer/Common/Constants.cs` retains only `DateTimeFormat`, `DefaultDbName`, and the six `*BaseName` entries.
- [ ] `GameDatabase/Migrations/` contains the new `*_EntityNamespaceMove.cs` (`Up`/`Down` empty) and `*_EntityNamespaceMove.Designer.cs`; `TaikoDbContextModelSnapshot.cs` is regenerated.
- [ ] `dotnet ef migrations script <prior-id> EntityNamespaceMove ...` produces SQL with no DDL between PRAGMAs.
- [ ] Server starts, migrates DB, and serves both the smoke game endpoint and the smoke admin endpoint.
- [ ] Branch `dev/clean-arch-domain` is pushed and (if user-authorized) a PR is open.

After merge, proceed to PR2 with a fresh `git checkout dev && git pull --ff-only` and **start a new Claude Code session for PR2 from the next plan file:** `docs/superpowers/plans/2026-05-04-clean-arch-pr2-application-infrastructure.md`.
