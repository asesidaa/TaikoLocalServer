# PR3 — Adapters Extraction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Physically relocate controllers, mappers, wire types, middleware, certificates, configurations, logging, and `wwwroot/` into five new adapter projects + a renamed Host project. Mostly mechanical — no behavior changes. Each file moved was already restructured in PR1 or PR2.

**Architecture:** This PR realizes the hexagonal layout. Outcome:

- `TaikoLocalServer.Adapters.AdminApi` (admin REST controllers, JWT auth scheme registration)
- `TaikoLocalServer.Adapters.AllnetMucha` (AmAuth/AmUpdater/Garmc/MuchaActivation + AllNetRequestMiddleware + Mucha wire types)
- `TaikoLocalServer.Adapters.GameProtocol.Shared` (BaseProtocolController, GZipBytesUtil, header strip)
- `TaikoLocalServer.Adapters.GameProtocol.WwR08` (`_ww` controllers + `Models/ww_r08/` wire types + WW mapper methods)
- `TaikoLocalServer.Adapters.GameProtocol.CnR00` (`_cn` controllers + `Models/CN00/` wire types + CN mapper methods)
- `TaikoLocalServer.Host` (renamed from `TaikoLocalServer/`; owns `Program.cs`, `Configurations/`, `Certificates/`, `wwwroot/`, `Logging/CsvFormatter`)

`<AssemblyName>TaikoLocalServer</AssemblyName>` on Host preserves the published exe name. `Program.cs` slims to `AddAdminApi() + AddAllnetMucha() + AddGameProtocolWwR08() + AddGameProtocolCnR00()`.

**Tech Stack:** unchanged (.NET 10, ASP.NET Core 10, protobuf-net, Mapperly, MVC).

**Spec reference:** `docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md` — see §3 (per-adapter file lists), §5 (per-version pluggability), §6 (filesystem paths, Serilog CSV side-channel, GlobalUsings split), §7 (PR3 verification gates), §8 risks #4–#9.

**Prerequisites:** PR2 (`dev/clean-arch-app-infra`) is merged into `dev`. Run:

```bash
git checkout dev
git pull --ff-only origin dev
```

Confirm `dotnet build` succeeds and `Application/` + `Infrastructure/` are present.

---

## How this plan works

This PR is **mostly file relocation with minor csproj construction**. There's almost no logic change — every piece of code was already restructured in PR2 to use port abstractions; PR3 just gives each piece a permanent home in the new project layout.

Verification rhythm: after each adapter project lands, the full `dotnet build` should still go green and the route count should be unchanged.

All commands assume the working directory is the repo root (`D:\TaikoLocalServer\`).

## Final layout this PR produces

```
Domain/                                  (unchanged from PR1)
Application/                             (unchanged from PR2)
Infrastructure/                          (unchanged from PR2)

Adapters.AdminApi/                       ← NEW
  Adapters.AdminApi.csproj               <RootNamespace>=TaikoLocalServer.Adapters.AdminApi
  Controllers/                           (10 controllers from TaikoLocalServer/Controllers/Api/)
  BaseAdminController.cs                 (split from BaseController<T>)
  Filters/AuthorizeIfRequiredAttribute.cs
  DependencyInjection.cs                 (AddAdminApi(IConfiguration))
  GlobalUsings.cs

Adapters.AllnetMucha/                    ← NEW
  Adapters.AllnetMucha.csproj            <RootNamespace>=TaikoLocalServer.Adapters.AllnetMucha
  Controllers/{AmAuth,AmUpdater,Garmc,MuchaActivation}/
  Middleware/AllNetRequestMiddleware.cs
  Wire/{MuchaBoardAuthRequest, MuchaUpdateCheckRequest, PowerOnRequest}.cs
  Common/FormOutputUtil.cs
  DependencyInjection.cs                 (AddAllnetMucha() + UseAllnetMucha())
  GlobalUsings.cs

Adapters.GameProtocol.Shared/            ← NEW
  Adapters.GameProtocol.Shared.csproj
  Controllers/BaseProtocolController.cs  (split from BaseController<T>)
  Compression/GZipBytesUtil.cs
  Compression/HeaderStripUtil.cs         (extracts the inline Skip(32).ToArray() pattern)
  Marker.cs                              (empty, for assembly identity)
  GlobalUsings.cs

Adapters.GameProtocol.WwR08/             ← NEW
  Adapters.GameProtocol.WwR08.csproj
  Wire/                                  (from TaikoLocalServer/Models/ww_r08/)
  Mappers/                               (only _ww methods of TaikoLocalServer/Mappers/)
  Controllers/                           (only [HttpPost("/v12r08_ww/...")] methods)
  DependencyInjection.cs                 (AddGameProtocolWwR08() — no-op for now)
  GlobalUsings.cs

Adapters.GameProtocol.CnR00/             ← NEW
  Adapters.GameProtocol.CnR00.csproj
  Wire/                                  (from TaikoLocalServer/Models/cn_r00/Game.cs;
                                            namespace TaikoLocalServer.Models.CN00 → renamed)
  Mappers/                               (only _cn methods)
  Controllers/                           (only [HttpPost("/v12r00_cn/...")] methods)
  DependencyInjection.cs                 (AddGameProtocolCnR00() — no-op for now)
  GlobalUsings.cs

Host/                                    ← renamed from TaikoLocalServer/
  Host.csproj                            <AssemblyName>TaikoLocalServer</AssemblyName>
  Program.cs                             (slimmed to ~80-90 LOC)
  GlobalUsings.cs
  Logging/CsvFormatter.cs
  Configurations/{Auth, Database, DataSettings, Kestrel, Logging, Server}.json
  Certificates/{cert, root}.pfx
  Properties/launchSettings.json
  app.manifest
  wwwroot/                               (data/, taiko.db3 on operator's machine)
```

`.slnx` count: 7 (after PR2) → 12 (after PR3): adds 5 adapter projects, renames the host. `Application` + `Infrastructure` + `Domain` unchanged. `SharedProject` still present (deletion is PR4). Post-PR3 list: Domain, Application, Infrastructure, Adapters.AdminApi, Adapters.AllnetMucha, Adapters.GameProtocol.{Shared,WwR08,CnR00}, Host, TaikoWebUI, SharedProject, LocalSaveModScoreMigrator.

---

## Phase 3.0 — Branch and baseline

### Task 3.0.1: Create the PR3 branch

**Files:** none.

- [ ] **Step 1: Create and check out the PR3 branch**

```bash
git checkout dev
git pull --ff-only origin dev
git checkout -b dev/clean-arch-adapters
```

- [ ] **Step 2: Sanity check**

```bash
dotnet build
dotnet run --project TaikoLocalServer
```

Watch the startup log for `Mapped <N> endpoints`. **Record this number** — it must remain the same through PR3. Stop the server.

---

## Phase 3.1 — Adapters.GameProtocol.Shared

### Task 3.1.1: Create the project skeleton

**Files:**
- Create: `Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj`
- Create: `Adapters.GameProtocol.Shared/GlobalUsings.cs`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Adapters.GameProtocol.Shared/Controllers
mkdir -p Adapters.GameProtocol.Shared/Compression
```

- [ ] **Step 2: Write the csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.Shared</RootNamespace>
    <AssemblyName>TaikoLocalServer.Adapters.GameProtocol.Shared</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Application\Application.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="protobuf-net" />
    <PackageReference Include="protobuf-net.AspNetCore" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Write GlobalUsings.cs**

```csharp
// Global using directives

global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using ProtoBuf;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Handlers;
```

- [ ] **Step 4: Add to solution**

```xml
<Solution>
  <Project Path="Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj" />
  <Project Path="Application/Application.csproj" />
  <Project Path="Domain/Domain.csproj" />
  <Project Path="Infrastructure/Infrastructure.csproj" />
  <Project Path="LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj" />
  <Project Path="SharedProject/SharedProject.csproj" />
  <Project Path="TaikoLocalServer/TaikoLocalServer.csproj" />
  <Project Path="TaikoWebUI/TaikoWebUI.csproj" />
</Solution>
```

- [ ] **Step 5: Build (empty)**

```bash
dotnet build Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj
```

Expected: success.

### Task 3.1.2: Move GZipBytesUtil into Compression/

**Files:**
- Move: `TaikoLocalServer/Common/Utils/GZipBytesUtil.cs` → `Adapters.GameProtocol.Shared/Compression/GZipBytesUtil.cs`

> **Note:** `TaikoLocalServer/Common/Utils/` was emptied/deleted in PR2 Phase 2.14. If the file isn't there, look for `GZipBytesUtil.cs` elsewhere (it may have stayed in `TaikoLocalServer/`). The Glob done at the start of PR3 will tell you.

- [ ] **Step 1: Locate GZipBytesUtil.cs**

```bash
find . -name "GZipBytesUtil.cs"
```

(If not found, the file was deleted prematurely; recover from PR2's git history or recreate from the original — it's the gzip wrapper used by every game-protocol controller for `DecompressGZipBytes`/`CompressGZipBytes`.)

- [ ] **Step 2: Move it**

```bash
git mv <found-path> Adapters.GameProtocol.Shared/Compression/GZipBytesUtil.cs
```

- [ ] **Step 3: Update namespace**

In `Adapters.GameProtocol.Shared/Compression/GZipBytesUtil.cs`, change the namespace declaration to:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Compression;
```

- [ ] **Step 4: Build adapter**

```bash
dotnet build Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj
```

Expected: success.

### Task 3.1.3: Add HeaderStripUtil

Per spec §3 ("HeaderStripUtil.cs ... extracts the inline Skip(32).ToArray() pattern; not all endpoints use it"). The pattern is in many `_ww` controllers — search for `Skip(32).ToArray()`:

```bash
grep -rln "Skip(32).ToArray()" TaikoLocalServer
```

- [ ] **Step 1: Write the helper**

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Compression;

public static class HeaderStripUtil
{
    /// <summary>
    /// Strips the 32-byte header used by the WW R08 protocol's request envelopes.
    /// </summary>
    public static byte[] StripWwHeader(byte[] payload)
    {
        return payload.Skip(32).ToArray();
    }
}
```

- [ ] **Step 2: Build adapter**

```bash
dotnet build Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj
```

Expected: success.

> **In PR3, controllers can still keep the `Skip(32).ToArray()` inline.** Replacing every call site with `HeaderStripUtil.StripWwHeader(...)` is a follow-up cleanup. Just having the helper available counts.

### Task 3.1.4: BaseProtocolController split from BaseController<T>

**Files:**
- Create: `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`

- [ ] **Step 1: Read current BaseController.cs**

```bash
cat TaikoLocalServer/Controllers/BaseController.cs
```

- [ ] **Step 2: Write BaseProtocolController.cs as a copy with new namespace**

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

public abstract class BaseProtocolController<T> : ControllerBase where T : BaseProtocolController<T>
{
    private ILogger<T>? logger;

    private IMediator? mediator;

    protected IMediator Mediator => (mediator ??= HttpContext.RequestServices.GetService<IMediator>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}
```

(The `Microsoft.Extensions.Logging` types are pulled in via `Microsoft.AspNetCore.App` framework reference; `IMediator` via Application.)

- [ ] **Step 3: Add Marker.cs**

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Shared;

internal static class Marker
{
    // Empty class for assembly identity / typeof(Marker).Assembly references.
}
```

Save as `Adapters.GameProtocol.Shared/Marker.cs`.

- [ ] **Step 4: Build the whole solution**

```bash
dotnet build
```

Expected: success. (Game-protocol controllers in `TaikoLocalServer/Controllers/Game/` still inherit `BaseController<T>` — that swap to `BaseProtocolController<T>` happens in Phase 3.4/3.5.)

- [ ] **Step 5: Commit**

```bash
git add Adapters.GameProtocol.Shared TaikoLocalServer.slnx
git commit -m "PR3.1: scaffold Adapters.GameProtocol.Shared (Compression + BaseProtocolController + Marker)"
```

---

## Phase 3.2 — Adapters.AllnetMucha

### Task 3.2.1: Create the project skeleton

**Files:**
- Create: `Adapters.AllnetMucha/Adapters.AllnetMucha.csproj`
- Create: `Adapters.AllnetMucha/GlobalUsings.cs`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Adapters.AllnetMucha/Controllers/AmAuth
mkdir -p Adapters.AllnetMucha/Controllers/AmUpdater
mkdir -p Adapters.AllnetMucha/Controllers/Garmc
mkdir -p Adapters.AllnetMucha/Controllers/MuchaActivation
mkdir -p Adapters.AllnetMucha/Middleware
mkdir -p Adapters.AllnetMucha/Wire
mkdir -p Adapters.AllnetMucha/Common
```

- [ ] **Step 2: Write the csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Adapters.AllnetMucha</RootNamespace>
    <AssemblyName>TaikoLocalServer.Adapters.AllnetMucha</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="DotNetZip" />
    <PackageReference Include="Otp.NET" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: GlobalUsings.cs**

```csharp
// Global using directives

global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Compression;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Infrastructure.Settings;
```

- [ ] **Step 4: Add to .slnx**

Update `TaikoLocalServer.slnx` to insert `<Project Path="Adapters.AllnetMucha/Adapters.AllnetMucha.csproj" />` after `Adapters.GameProtocol.Shared`.

- [ ] **Step 5: Build (empty)**

```bash
dotnet build Adapters.AllnetMucha/Adapters.AllnetMucha.csproj
```

Expected: success.

### Task 3.2.2: Move AllnetMucha controllers + middleware + wire types + utils

**Files:**
- Move: `TaikoLocalServer/Controllers/AmAuth/PowerOnController.cs` → `Adapters.AllnetMucha/Controllers/AmAuth/`
- Move: `TaikoLocalServer/Controllers/AmUpdater/MuchaController.cs` → `Adapters.AllnetMucha/Controllers/AmUpdater/`
- Move: `TaikoLocalServer/Controllers/Garmc/{Ping,RegisterSystemBoardBilling,RegisterSystemBoard}Controller.cs` → `Adapters.AllnetMucha/Controllers/Garmc/`
- Move: `TaikoLocalServer/Controllers/MuchaActivation/{Otk,Signature}Controller.cs` → `Adapters.AllnetMucha/Controllers/MuchaActivation/`
- Move: `TaikoLocalServer/Middlewares/AllNetRequestMiddleware.cs` → `Adapters.AllnetMucha/Middleware/`
- Move: `TaikoLocalServer/Models/{MuchaBoardAuthRequest, MuchaUpdateCheckRequest, PowerOnRequest}.cs` → `Adapters.AllnetMucha/Wire/`
- Move: `TaikoLocalServer/Common/Utils/FormOutputUtil.cs` → `Adapters.AllnetMucha/Common/` (if it survived PR2 cleanup; if PR2 deleted Utils/, locate and recover from history)

- [ ] **Step 1: Move controllers**

```bash
git mv TaikoLocalServer/Controllers/AmAuth/PowerOnController.cs                          Adapters.AllnetMucha/Controllers/AmAuth/
git mv TaikoLocalServer/Controllers/AmUpdater/MuchaController.cs                         Adapters.AllnetMucha/Controllers/AmUpdater/
git mv TaikoLocalServer/Controllers/Garmc/PingController.cs                              Adapters.AllnetMucha/Controllers/Garmc/
git mv TaikoLocalServer/Controllers/Garmc/RegisterSystemBoardBillingController.cs        Adapters.AllnetMucha/Controllers/Garmc/
git mv TaikoLocalServer/Controllers/Garmc/RegisterSystemBoardController.cs               Adapters.AllnetMucha/Controllers/Garmc/
git mv TaikoLocalServer/Controllers/MuchaActivation/OtkController.cs                     Adapters.AllnetMucha/Controllers/MuchaActivation/
git mv TaikoLocalServer/Controllers/MuchaActivation/SignatureController.cs               Adapters.AllnetMucha/Controllers/MuchaActivation/
```

- [ ] **Step 2: Move middleware + wire types + utils**

```bash
git mv TaikoLocalServer/Middlewares/AllNetRequestMiddleware.cs   Adapters.AllnetMucha/Middleware/
git mv TaikoLocalServer/Models/MuchaBoardAuthRequest.cs          Adapters.AllnetMucha/Wire/
git mv TaikoLocalServer/Models/MuchaUpdateCheckRequest.cs        Adapters.AllnetMucha/Wire/
git mv TaikoLocalServer/Models/PowerOnRequest.cs                 Adapters.AllnetMucha/Wire/

# FormOutputUtil — locate first
find . -name "FormOutputUtil.cs"
# then move:
git mv <found-path> Adapters.AllnetMucha/Common/FormOutputUtil.cs
```

- [ ] **Step 3: Update namespaces in moved files**

For each moved file, change the namespace declaration:

| Old namespace | New namespace |
|---|---|
| `TaikoLocalServer.Controllers.AmAuth` | `TaikoLocalServer.Adapters.AllnetMucha.Controllers.AmAuth` |
| `TaikoLocalServer.Controllers.AmUpdater` | `TaikoLocalServer.Adapters.AllnetMucha.Controllers.AmUpdater` |
| `TaikoLocalServer.Controllers.Garmc` | `TaikoLocalServer.Adapters.AllnetMucha.Controllers.Garmc` |
| `TaikoLocalServer.Controllers.MuchaActivation` | `TaikoLocalServer.Adapters.AllnetMucha.Controllers.MuchaActivation` |
| `TaikoLocalServer.Middlewares` | `TaikoLocalServer.Adapters.AllnetMucha.Middleware` |
| `TaikoLocalServer.Models` | `TaikoLocalServer.Adapters.AllnetMucha.Wire` |
| `TaikoLocalServer.Common.Utils` | `TaikoLocalServer.Adapters.AllnetMucha.Common` |

A scripted approach (bash):

```bash
sed -i 's|^namespace TaikoLocalServer\.Controllers\.AmAuth;|namespace TaikoLocalServer.Adapters.AllnetMucha.Controllers.AmAuth;|' Adapters.AllnetMucha/Controllers/AmAuth/*.cs
sed -i 's|^namespace TaikoLocalServer\.Controllers\.AmUpdater;|namespace TaikoLocalServer.Adapters.AllnetMucha.Controllers.AmUpdater;|' Adapters.AllnetMucha/Controllers/AmUpdater/*.cs
sed -i 's|^namespace TaikoLocalServer\.Controllers\.Garmc;|namespace TaikoLocalServer.Adapters.AllnetMucha.Controllers.Garmc;|' Adapters.AllnetMucha/Controllers/Garmc/*.cs
sed -i 's|^namespace TaikoLocalServer\.Controllers\.MuchaActivation;|namespace TaikoLocalServer.Adapters.AllnetMucha.Controllers.MuchaActivation;|' Adapters.AllnetMucha/Controllers/MuchaActivation/*.cs
sed -i 's|^namespace TaikoLocalServer\.Middlewares;|namespace TaikoLocalServer.Adapters.AllnetMucha.Middleware;|' Adapters.AllnetMucha/Middleware/*.cs
sed -i 's|^namespace TaikoLocalServer\.Models;|namespace TaikoLocalServer.Adapters.AllnetMucha.Wire;|' Adapters.AllnetMucha/Wire/*.cs
sed -i 's|^namespace TaikoLocalServer\.Common\.Utils;|namespace TaikoLocalServer.Adapters.AllnetMucha.Common;|' Adapters.AllnetMucha/Common/*.cs
```

- [ ] **Step 4: Replace base class references**

Each moved controller currently inherits `BaseController<T>` from `TaikoLocalServer.Controllers`. The Allnet/Mucha controllers don't deal with protobuf but they still want lazy IMediator/ILogger access. **For PR3, change them to inherit `BaseProtocolController<T>`** from `Adapters.GameProtocol.Shared`. Long-term we may want a `BaseAdapterController` shared by both AllnetMucha and AdminApi, but that's overkill for now.

In each moved Allnet/Mucha controller:

- Change `: BaseController<X>` → `: BaseProtocolController<X>`
- Add `using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;` at the top.

If a controller doesn't use Mediator at all (e.g., simple Garmc handlers that return constants), simply inherit `ControllerBase` and drop the BaseController dependency. Decide per file.

- [ ] **Step 5: Update consumers of moved Mucha wire types**

```bash
grep -rln "TaikoLocalServer\.Models\.MuchaBoardAuthRequest\|TaikoLocalServer\.Models\.MuchaUpdateCheckRequest\|TaikoLocalServer\.Models\.PowerOnRequest" .
```

These types lived in the `TaikoLocalServer.Models` namespace before the move. Most consumers were the controllers themselves (now also moved). Any leftover references in TaikoLocalServer/Program.cs's `UseAllnetMucha` wiring → fix.

- [ ] **Step 6: Add UseAllnetMucha helper**

Per spec §3, `Adapters.AllnetMucha/DependencyInjection.cs`:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AllnetMucha.Middleware;

namespace TaikoLocalServer.Adapters.AllnetMucha;

public static class DependencyInjection
{
    public static IServiceCollection AddAllnetMucha(this IServiceCollection services)
    {
        // No DI registrations specific to Allnet/Mucha at present;
        // controllers + middleware are picked up via assembly scanning.
        return services;
    }

    public static IApplicationBuilder UseAllnetMucha(this WebApplication app)
    {
        // Conditional wiring for /sys/servlet/PowerOn — preserved verbatim from Program.cs.
        app.UseWhen(
            context => context.Request.Path.StartsWithSegments("/sys/servlet/PowerOn", StringComparison.InvariantCulture),
            applicationBuilder => applicationBuilder.UseAllNetRequestMiddleware());

        return app;
    }
}
```

(`UseAllNetRequestMiddleware` is the existing extension on `IApplicationBuilder` — verify the method signature in `AllNetRequestMiddleware.cs` after the move; if it's a static class with a `UseAllNetRequestMiddleware` extension, it works. If not, wrap with `app.UseMiddleware<AllNetRequestMiddleware>()` instead.)

- [ ] **Step 7: Update TaikoLocalServer's Program.cs**

In `TaikoLocalServer/Program.cs`, find:

```csharp
    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/sys/servlet/PowerOn", StringComparison.InvariantCulture),
        applicationBuilder => applicationBuilder.UseAllNetRequestMiddleware());
```

Replace with:

```csharp
    app.UseAllnetMucha();
```

Add the using:

```csharp
using TaikoLocalServer.Adapters.AllnetMucha;
```

In the registration block, add:

```csharp
    builder.Services.AddAllnetMucha();
```

(After `builder.Services.AddInfrastructure(...)`.)

- [ ] **Step 8: Update TaikoLocalServer.csproj to ref the new adapter**

```xml
  <ItemGroup>
    <ProjectReference Include="..\Adapters.AllnetMucha\Adapters.AllnetMucha.csproj" />
    <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Domain\Domain.csproj" />
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    <ProjectReference Include="..\TaikoWebUI\TaikoWebUI.csproj" />
  </ItemGroup>
```

- [ ] **Step 9: Build solution**

```bash
dotnet build
```

Expected: success.

- [ ] **Step 10: Smoke check**

```bash
dotnet run --project TaikoLocalServer
```

Watch for `Mapped <N> endpoints` — must equal the Phase 3.0 baseline. Stop the server.

- [ ] **Step 11: Commit**

```bash
git add Adapters.AllnetMucha TaikoLocalServer
git commit -m "PR3.2: extract Adapters.AllnetMucha (controllers + middleware + Mucha wire types + FormOutputUtil)"
```

---

## Phase 3.3 — Adapters.AdminApi

### Task 3.3.1: Create the project skeleton

**Files:**
- Create: `Adapters.AdminApi/Adapters.AdminApi.csproj`
- Create: `Adapters.AdminApi/GlobalUsings.cs`
- Create: `Adapters.AdminApi/BaseAdminController.cs`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Adapters.AdminApi/Controllers
mkdir -p Adapters.AdminApi/Filters
```

- [ ] **Step 2: Write the csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Adapters.AdminApi</RootNamespace>
    <AssemblyName>TaikoLocalServer.Adapters.AdminApi</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
  </ItemGroup>

</Project>
```

> Why ref SharedProject here? AdminApi controllers return `User`, `UsersResponse`, etc. — those still live in SharedProject until PR4 splits them into `Contracts.AdminApi`. PR4 swaps this `<ProjectReference>` from SharedProject to Contracts.AdminApi.

- [ ] **Step 3: GlobalUsings.cs**

```csharp
// Global using directives

global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using SharedProject.Models;
global using SharedProject.Models.Requests;
global using SharedProject.Models.Responses;
global using TaikoLocalServer.Adapters.AdminApi.Filters;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Domain.Entities;
global using TaikoLocalServer.Infrastructure.Identity.Settings;
```

- [ ] **Step 4: BaseAdminController.cs**

```csharp
namespace TaikoLocalServer.Adapters.AdminApi;

public abstract class BaseAdminController<T> : ControllerBase where T : BaseAdminController<T>
{
    private ILogger<T>? logger;

    private IMediator? mediator;

    protected IMediator Mediator => (mediator ??= HttpContext.RequestServices.GetService<IMediator>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}
```

- [ ] **Step 5: Add to .slnx**

Insert `<Project Path="Adapters.AdminApi/Adapters.AdminApi.csproj" />` (alphabetical placement).

- [ ] **Step 6: Build (empty)**

```bash
dotnet build Adapters.AdminApi/Adapters.AdminApi.csproj
```

Expected: success.

### Task 3.3.2: Move admin controllers + filter

**Files:**
- Move: `TaikoLocalServer/Controllers/Api/*.cs` (10 files) → `Adapters.AdminApi/Controllers/`
- Move: `TaikoLocalServer/Filters/AuthorizeIfRequiredAttribute.cs` → `Adapters.AdminApi/Filters/`

- [ ] **Step 1: Move controllers**

```bash
git mv TaikoLocalServer/Controllers/Api/AuthController.cs               Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/CardsController.cs              Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/DanBestDataController.cs        Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/FavoriteSongsController.cs      Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/GameDataController.cs           Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/PlayDataController.cs           Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/PlayHistoryController.cs        Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/SongLeaderboardController.cs    Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/UserSettingsController.cs       Adapters.AdminApi/Controllers/
git mv TaikoLocalServer/Controllers/Api/UsersController.cs              Adapters.AdminApi/Controllers/

rmdir TaikoLocalServer/Controllers/Api
```

- [ ] **Step 2: Move the filter**

```bash
git mv TaikoLocalServer/Filters/AuthorizeIfRequiredAttribute.cs Adapters.AdminApi/Filters/
rmdir TaikoLocalServer/Filters
```

- [ ] **Step 3: Update namespaces**

```bash
sed -i 's|^namespace TaikoLocalServer\.Controllers\.Api;|namespace TaikoLocalServer.Adapters.AdminApi.Controllers;|' Adapters.AdminApi/Controllers/*.cs
sed -i 's|^namespace TaikoLocalServer\.Filters;|namespace TaikoLocalServer.Adapters.AdminApi.Filters;|' Adapters.AdminApi/Filters/*.cs
```

- [ ] **Step 4: Swap BaseController<T> → BaseAdminController<T>**

In each moved controller, change inherited base. Run:

```bash
sed -i 's|: BaseController<\([A-Za-z0-9_]\+\)>|: BaseAdminController<\1>|g' Adapters.AdminApi/Controllers/*.cs
```

(or do it manually in each file).

- [ ] **Step 5: Add `using` rows in moved controllers**

If any controller has `using TaikoLocalServer.Filters;`, change to `using TaikoLocalServer.Adapters.AdminApi.Filters;` (or rely on the GlobalUsings alias). Same for `using TaikoLocalServer.Settings;` → `using TaikoLocalServer.Infrastructure.Identity.Settings;`.

Run:

```bash
sed -i 's|using TaikoLocalServer\.Filters;|using TaikoLocalServer.Adapters.AdminApi.Filters;|g' Adapters.AdminApi/Controllers/*.cs
sed -i 's|using TaikoLocalServer\.Settings;|using TaikoLocalServer.Infrastructure.Identity.Settings;|g' Adapters.AdminApi/Controllers/*.cs
```

- [ ] **Step 6: Move the JWT auth registration into AddAdminApi**

Per spec §3, `AddAuthentication().AddJwtBearer(...)` moves out of `Program.cs` (currently registered in Infrastructure's `AddInfrastructure` per PR2 Phase 2.14) into `AddAdminApi`. **Decision for PR3:** keep JWT registration in `AddInfrastructure` for now to minimize churn. Spec §3 says JWT belongs to AdminApi — but moving it later is trivial (one-line copy). For PR3, leave it in Infrastructure; PR4 can revisit.

> If you want to move it now: cut the entire `services.AddAuthentication()...AddJwtBearer(...)` block from `Infrastructure/DependencyInjection.cs` and paste into a new `Adapters.AdminApi/DependencyInjection.cs`'s `AddAdminApi(IConfiguration)`. Both work; choose one.

- [ ] **Step 7: Write Adapters.AdminApi/DependencyInjection.cs**

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Filters;

namespace TaikoLocalServer.Adapters.AdminApi;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuthorizeIfRequiredAttribute>();

        // JWT scheme registration stays in Infrastructure for now (per PR3 plan).

        return services;
    }
}
```

- [ ] **Step 8: Update Program.cs to use AddAdminApi**

In `TaikoLocalServer/Program.cs`, find:

```csharp
    builder.Services.AddScoped<AuthorizeIfRequiredAttribute>();
```

Replace with:

```csharp
    builder.Services.AddAdminApi(builder.Configuration);
```

Add `using TaikoLocalServer.Adapters.AdminApi;`.

- [ ] **Step 9: Update TaikoLocalServer.csproj**

Add `<ProjectReference Include="..\Adapters.AdminApi\Adapters.AdminApi.csproj" />` (alphabetical).

- [ ] **Step 10: Build**

```bash
dotnet build
```

Iterate until clean. Common issues:
- `AuthorizeIfRequiredAttribute` not found in moved controllers → ensure global using `TaikoLocalServer.Adapters.AdminApi.Filters` is in GlobalUsings.cs (already added in Step 3 of 3.3.1) or add per-file using.

- [ ] **Step 11: Smoke run**

```bash
dotnet run --project TaikoLocalServer
```

Verify route count matches baseline. Stop.

- [ ] **Step 12: Commit**

```bash
git add Adapters.AdminApi TaikoLocalServer
git commit -m "PR3.3: extract Adapters.AdminApi (10 controllers + filter + AddAdminApi)"
```

---

## Phase 3.4 — Adapters.GameProtocol.WwR08

### Task 3.4.1: Create the project skeleton

**Files:**
- Create: `Adapters.GameProtocol.WwR08/Adapters.GameProtocol.WwR08.csproj`
- Create: `Adapters.GameProtocol.WwR08/GlobalUsings.cs`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Adapters.GameProtocol.WwR08/Wire
mkdir -p Adapters.GameProtocol.WwR08/Mappers
mkdir -p Adapters.GameProtocol.WwR08/Controllers
```

- [ ] **Step 2: csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.WwR08</RootNamespace>
    <AssemblyName>TaikoLocalServer.Adapters.GameProtocol.WwR08</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
    <ProjectReference Include="..\Application\Application.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Riok.Mapperly" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: GlobalUsings.cs**

```csharp
// Global using directives

global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using ProtoBuf;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Compression;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Handlers;
global using TaikoLocalServer.Domain.Enums;
```

- [ ] **Step 4: Add to .slnx**

### Task 3.4.2: Move ww_r08 wire types

**Files:**
- Move: `TaikoLocalServer/Models/ww_r08/*.cs` → `Adapters.GameProtocol.WwR08/Wire/`

The 10 files (per Glob): `Game.cs`, `any.cs`, `date.cs`, `descriptor.cs`, `latLng.cs`, `monitoring.cs`, `status.cs`, `systemboard.cs`, `types.cs`, `VsInterface.cs`.

- [ ] **Step 1: Move**

```bash
git mv TaikoLocalServer/Models/ww_r08/*.cs Adapters.GameProtocol.WwR08/Wire/
rmdir TaikoLocalServer/Models/ww_r08
```

- [ ] **Step 2: Update namespaces**

The current namespace is `TaikoLocalServer.Models.WW08` (note: namespace ≠ folder name). Change to `TaikoLocalServer.Adapters.GameProtocol.WwR08.Wire`:

```bash
sed -i 's|namespace TaikoLocalServer\.Models\.WW08|namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Wire|g' Adapters.GameProtocol.WwR08/Wire/*.cs
```

(Some files may have `namespace TaikoLocalServer.Models.WW08;` and others may be inside a block-style namespace. Verify with grep:)

```bash
grep -l "namespace TaikoLocalServer\.Models\.WW08" Adapters.GameProtocol.WwR08/Wire/*.cs
```

If any matches survive, repeat the sed.

### Task 3.4.3: Split mapper files (WW portion)

The current `TaikoLocalServer/Mappers/` has 16 files. **Some have only WW methods, some have only CN methods, some are mixed.** Per spec §3, "Files mixing both versions split."

The visible mixed file is `PlayResultMappers.cs` (has both `Map(PlayResultDataRequest)` for WW and `Map(Models.CN00.PlayResultDataRequest)` for CN). Other mappers may also be mixed.

- [ ] **Step 1: Inspect every mapper to classify it**

For each `TaikoLocalServer/Mappers/*.cs`:

```bash
for f in TaikoLocalServer/Mappers/*.cs; do
  echo "=== $f ==="
  grep -E "Models\.CN00|partial \w+ Map" "$f"
done
```

Classify:
- **WW-only:** ✅ move to `Adapters.GameProtocol.WwR08/Mappers/` as-is, namespace updated.
- **CN-only:** ✅ move to `Adapters.GameProtocol.CnR00/Mappers/` (next phase).
- **Mixed:** split into two files — one in WwR08, one in CnR00.

Spec §3 names this rule but doesn't enumerate; classification is per-file inspection.

- [ ] **Step 2: Move WW-only mappers**

For each WW-only mapper:

```bash
git mv TaikoLocalServer/Mappers/<File>.cs Adapters.GameProtocol.WwR08/Mappers/<File>.cs
sed -i 's|namespace TaikoLocalServer\.Mappers;|namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;|' Adapters.GameProtocol.WwR08/Mappers/<File>.cs
```

- [ ] **Step 3: Split mixed mappers (e.g., `PlayResultMappers.cs`)**

For `PlayResultMappers.cs`:

```csharp
// Original (TaikoLocalServer/Mappers/PlayResultMappers.cs):
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static partial CommonPlayResultData Map(PlayResultDataRequest request);

    public static partial CommonPlayResultData Map(Models.CN00.PlayResultDataRequest request);
}
```

Becomes two files:

```csharp
// Adapters.GameProtocol.WwR08/Mappers/PlayResultMappers.cs
using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Adapters.GameProtocol.WwR08.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static partial CommonPlayResultData Map(PlayResultDataRequest request);
}
```

```csharp
// Adapters.GameProtocol.CnR00/Mappers/PlayResultMappers.cs   (created in Phase 3.5)
using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Adapters.GameProtocol.CnR00.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static partial CommonPlayResultData Map(PlayResultDataRequest request);
}
```

> Note the class name `PlayResultMappers` is the same in both — they live in different namespaces so it's fine. Calling code disambiguates via `using`.

For PR3, **only do the WW half here**. The CN half is Phase 3.5.

- [ ] **Step 4: Build WwR08**

```bash
dotnet build Adapters.GameProtocol.WwR08/Adapters.GameProtocol.WwR08.csproj
```

Expected: success (assuming all referenced types now resolve).

### Task 3.4.4: Move/split game controllers (WW portion)

The 22 controllers in `TaikoLocalServer/Controllers/Game/` (per Glob). Each has zero or more WW route handlers (`[HttpPost("/v12r08_ww/...")]`) and zero or more CN route handlers (`[HttpPost("/v12r00_cn/...")]`).

- [ ] **Step 1: Inspect every game controller and classify**

```bash
for f in TaikoLocalServer/Controllers/Game/*.cs; do
  ww=$(grep -c "v12r08_ww" "$f" || true)
  cn=$(grep -c "v12r00_cn" "$f" || true)
  echo "$(basename $f) — ww:$ww  cn:$cn"
done
```

- **WW-only (cn:0):** move file as-is to `Adapters.GameProtocol.WwR08/Controllers/`.
- **CN-only (ww:0):** move file as-is to `Adapters.GameProtocol.CnR00/Controllers/` (Phase 3.5).
- **Mixed (ww>0 and cn>0):** split — copy file twice, keep only the corresponding routes in each copy.

We confirmed `PlayResultController.cs` is mixed (one `_ww` method, one `_cn` method). It splits.

- [ ] **Step 2: Move WW-only controllers (verbatim)**

```bash
# Example for hypothetical WW-only files:
git mv TaikoLocalServer/Controllers/Game/<WwOnlyFile>.cs Adapters.GameProtocol.WwR08/Controllers/
sed -i 's|namespace TaikoLocalServer\.Controllers\.Game;|namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;|' Adapters.GameProtocol.WwR08/Controllers/<WwOnlyFile>.cs
sed -i 's|: BaseController<\([A-Za-z0-9_]\+\)>|: BaseProtocolController<\1>|g' Adapters.GameProtocol.WwR08/Controllers/<WwOnlyFile>.cs
```

- [ ] **Step 3: Split mixed controllers (PlayResultController and others identified in Step 1)**

For `PlayResultController.cs`:

Create `Adapters.GameProtocol.WwR08/Controllers/PlayResultController.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.WwR08.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost("/v12r08_ww/chassis/playresult_r3ky4a4z.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UploadPlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("PlayResult request : {Request}", request.Stringify());

        var truncated = request.PlayresultData.Skip(32).ToArray();
        var decompressed = GZipBytesUtil.DecompressGZipBytes(truncated);
        var playResultData = Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(decompressed));
        Logger.LogInformation("Play result data {Data}", playResultData.Stringify());

        var commonRequest = PlayResultMappers.Map(playResultData);
        var commonResponse = await Mediator.Send(new UpdatePlayResultCommand(request.BaidConf, commonRequest), HttpContext.RequestAborted);
        var response = new PlayResultResponse
        {
            Result = commonResponse
        };
        return Ok(response);
    }
}
```

(`request.Stringify()` resolves via Swan formatters — keep the `using` row from the original.)

The CN counterpart goes into `Adapters.GameProtocol.CnR00/Controllers/PlayResultController.cs` in Phase 3.5.

After splitting, **delete the original mixed file**:

```bash
git rm TaikoLocalServer/Controllers/Game/PlayResultController.cs
```

- [ ] **Step 4: Repeat for every other mixed controller**

Inspect each from Step 1 with mixed counts and apply the same pattern.

- [ ] **Step 5: Update consumers**

Any file that referenced types in `TaikoLocalServer.Controllers.Game.*` (rare) or `TaikoLocalServer.Mappers.*` needs the namespace update.

In `TaikoLocalServer/GlobalUsings.cs`, drop the obsolete `global using TaikoLocalServer.Models.WW08;` row (the type lives in `Adapters.GameProtocol.WwR08.Wire` now and is only used inside that adapter).

- [ ] **Step 6: Update TaikoLocalServer.csproj**

Add `<ProjectReference Include="..\Adapters.GameProtocol.WwR08\Adapters.GameProtocol.WwR08.csproj" />`.

- [ ] **Step 7: Add `AddGameProtocolWwR08()` extension**

`Adapters.GameProtocol.WwR08/DependencyInjection.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08;

public static class DependencyInjection
{
    public static IServiceCollection AddGameProtocolWwR08(this IServiceCollection services)
    {
        // No DI registrations specific to this game-protocol version at present;
        // controllers are picked up via assembly scanning. Kept as a future hook.
        return services;
    }
}
```

In Program.cs, add:

```csharp
    builder.Services.AddGameProtocolWwR08();
```

(After `AddAllnetMucha`.)

- [ ] **Step 8: Build**

```bash
dotnet build
```

Iterate.

- [ ] **Step 9: Smoke**

```bash
dotnet run --project TaikoLocalServer
```

Route count matches baseline.

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "PR3.4: extract Adapters.GameProtocol.WwR08 (wire + WW-only mappers + WW route controllers)"
```

---

## Phase 3.5 — Adapters.GameProtocol.CnR00

### Task 3.5.1: Create skeleton

Mirror Phase 3.4. Csproj:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.CnR00</RootNamespace>
    <AssemblyName>TaikoLocalServer.Adapters.GameProtocol.CnR00</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
    <ProjectReference Include="..\Application\Application.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Riok.Mapperly" />
  </ItemGroup>

</Project>
```

GlobalUsings.cs same as WwR08 but with `CnR00` namespaces.

### Task 3.5.2: Move CN wire types

**Files:**
- Move: `TaikoLocalServer/Models/cn_r00/Game.cs` → `Adapters.GameProtocol.CnR00/Wire/Game.cs`

The folder name is `cn_r00`; the **declared C# namespace** in the file is `TaikoLocalServer.Models.CN00` (an inconsistency from the original code — verified by reading the file).

- [ ] **Step 1: Move**

```bash
mkdir -p Adapters.GameProtocol.CnR00/Wire
git mv TaikoLocalServer/Models/cn_r00/Game.cs Adapters.GameProtocol.CnR00/Wire/Game.cs
rmdir TaikoLocalServer/Models/cn_r00
```

If `TaikoLocalServer/Models/` is now empty, remove it too:

```bash
rmdir TaikoLocalServer/Models
```

(`git status` should show nothing if all moves committed cleanly.)

- [ ] **Step 2: Update namespace**

In `Adapters.GameProtocol.CnR00/Wire/Game.cs`, change `namespace TaikoLocalServer.Models.CN00` to `namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Wire`. The file may be very large (a single Game.cs containing all CN types); a single namespace replacement at the top should suffice.

```bash
sed -i 's|namespace TaikoLocalServer\.Models\.CN00|namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Wire|g' Adapters.GameProtocol.CnR00/Wire/Game.cs
```

### Task 3.5.3: CN-only mappers + CN halves of mixed mappers

For each CN-only mapper file from Phase 3.4 Task 3.4.3 Step 1:

```bash
git mv TaikoLocalServer/Mappers/<CnFile>.cs Adapters.GameProtocol.CnR00/Mappers/<CnFile>.cs
sed -i 's|namespace TaikoLocalServer\.Mappers;|namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;|' Adapters.GameProtocol.CnR00/Mappers/<CnFile>.cs
```

For each CN half of a mixed mapper from Phase 3.4 (e.g., `PlayResultMappers.cs`), create the new file as shown in Phase 3.4 Task 3.4.3 Step 3.

- [ ] **Step 1: After all moves, the original `TaikoLocalServer/Mappers/` should be empty**

```bash
ls TaikoLocalServer/Mappers/
```

If empty:

```bash
rmdir TaikoLocalServer/Mappers
```

If not empty, finish moving the residue (each leftover file should belong to either WwR08, CnR00, or be obsolete).

### Task 3.5.4: CN-only controllers + CN halves of mixed controllers

Mirror Phase 3.4 Task 3.4.4 with CN. After all moves:

- [ ] **Step 1: `TaikoLocalServer/Controllers/Game/` should be empty**

```bash
ls TaikoLocalServer/Controllers/Game/
rmdir TaikoLocalServer/Controllers/Game
```

If `TaikoLocalServer/Controllers/` is now also empty (BaseController.cs gone? — it's still there pre-3.6), defer the parent rmdir to Phase 3.6.

### Task 3.5.5: Wire it up

- [ ] **Step 1: Add to .slnx**

```xml
  <Project Path="Adapters.GameProtocol.CnR00/Adapters.GameProtocol.CnR00.csproj" />
```

- [ ] **Step 2: Add `AddGameProtocolCnR00()` extension**

Mirror Phase 3.4 Step 7.

- [ ] **Step 3: Add to Program.cs**

```csharp
    builder.Services.AddGameProtocolCnR00();
```

- [ ] **Step 4: Add ProjectReference in TaikoLocalServer.csproj**

- [ ] **Step 5: Build**

```bash
dotnet build
```

- [ ] **Step 6: Smoke**

```bash
dotnet run --project TaikoLocalServer
```

Route count matches baseline. Stop.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "PR3.5: extract Adapters.GameProtocol.CnR00 (wire + CN mappers + CN route controllers)"
```

---

## Phase 3.6 — Rename TaikoLocalServer → Host

This is the final structural step in PR3. The folder `TaikoLocalServer/` becomes `Host/`. The csproj is renamed to `Host.csproj` but with `<AssemblyName>TaikoLocalServer</AssemblyName>` so the produced exe stays `TaikoLocalServer.exe`.

### Task 3.6.1: Rename folder + csproj

**Files:**
- Rename: `TaikoLocalServer/` → `Host/`
- Rename: `Host/TaikoLocalServer.csproj` → `Host/Host.csproj`

- [ ] **Step 1: Rename**

```bash
git mv TaikoLocalServer Host
git mv Host/TaikoLocalServer.csproj Host/Host.csproj
```

- [ ] **Step 2: Update Host.csproj**

Replace contents with:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Host</RootNamespace>
    <AssemblyName>TaikoLocalServer</AssemblyName>
    <Version>1.1.0</Version>
    <ApplicationManifest>app.manifest</ApplicationManifest>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)'!='Debug'">
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>

  <PropertyGroup>
    <BuildTime>$([System.DateTime]::UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))</BuildTime>
  </PropertyGroup>

  <ItemGroup>
    <AssemblyAttribute Include="System.Reflection.AssemblyMetadataAttribute">
      <_Parameter1>BuildTime</_Parameter1>
      <_Parameter2>$(BuildTime)</_Parameter2>
    </AssemblyAttribute>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Adapters.AdminApi\Adapters.AdminApi.csproj" />
    <ProjectReference Include="..\Adapters.AllnetMucha\Adapters.AllnetMucha.csproj" />
    <ProjectReference Include="..\Adapters.GameProtocol.CnR00\Adapters.GameProtocol.CnR00.csproj" />
    <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
    <ProjectReference Include="..\Adapters.GameProtocol.WwR08\Adapters.GameProtocol.WwR08.csproj" />
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Domain\Domain.csproj" />
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    <ProjectReference Include="..\TaikoWebUI\TaikoWebUI.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Server" />
    <PackageReference Include="Microsoft.AspNetCore.ResponseCompression" />
    <PackageReference Include="Mediator.SourceGenerator">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Mediator.Abstractions" />
    <PackageReference Include="protobuf-net.AspNetCore" />
    <PackageReference Include="Serilog.AspNetCore" />
    <PackageReference Include="Serilog.Expressions" />
    <PackageReference Include="Serilog.Sinks.File.Header" />
    <PackageReference Include="Swashbuckle.AspNetCore" />
    <PackageReference Include="Throw" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="Templates\TemplateController.cs" />
  </ItemGroup>

  <ItemGroup>
    <!--Certificates-->
    <None Update="Certificates\cert.pfx">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="Certificates\root.pfx">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>

    <!--Configuration-->
    <Content Remove="Configurations\AuthSettings.json" />
    <None Include="Configurations\AuthSettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\ServerSettings.json" />
    <None Include="Configurations\ServerSettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\Database.json" />
    <None Include="Configurations\Database.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\DataSettings.json" />
    <None Include="Configurations\DataSettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\Kestrel.json" />
    <None Include="Configurations\Kestrel.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\Logging.json" />
    <None Include="Configurations\Logging.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>

    <!--Server Datatables-->
    <Content Update="wwwroot\data\locked_songs_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\locked_costume_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\locked_title_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <Content Update="wwwroot\data\intro_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\movie_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\shop_folder_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\dan_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\event_folder_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <Content Update="wwwroot\data\token_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\gaiden_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\qrcode_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\special_songs_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <!--Encrypted Game Datatables-->
    <Content Update="wwwroot\data\datatable\wordlist.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\musicinfo.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\music_order.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <Content Update="wwwroot\data\datatable\neiro.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\shougou.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\don_cos_reward.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <!--Decrypted Game Datatables-->
    <Content Update="wwwroot\data\datatable\wordlist.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\musicinfo.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\music_order.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <Content Update="wwwroot\data\datatable\neiro.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\shougou.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\don_cos_reward.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```

(Note: BCrypt.Net-Next, JwtBearer, EFCore packages move to Infrastructure; protobuf-net moves to GameProtocol.Shared; Riok.Mapperly moves to the version adapters; Yoh.Text.Json.NamingPolicies and SharpZipLib move to Infrastructure. Kept here: Serilog, ResponseCompression, Components.WebAssembly.Server (for Blazor hosting), protobuf-net.AspNetCore (the call to `.AddProtoBufNet()`), Throw, Swashbuckle, Mediator (the source generator must run here per Risk #2 fallback, plus abstractions for the `AddMediator` call site if it's colocated in Host).)

- [ ] **Step 3: Update .slnx**

Replace `<Project Path="TaikoLocalServer/TaikoLocalServer.csproj" />` with `<Project Path="Host/Host.csproj" />`.

- [ ] **Step 4: Move BaseController.cs to obsolete (it's been replaced by BaseAdminController + BaseProtocolController)**

```bash
git rm Host/Controllers/BaseController.cs
rmdir Host/Controllers
```

- [ ] **Step 5: Update GlobalUsings.cs**

`Host/GlobalUsings.cs`:

```csharp
// Global using directives

global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Serilog;
global using Swan.Formatters;
global using TaikoLocalServer.Adapters.AdminApi;
global using TaikoLocalServer.Adapters.AllnetMucha;
global using TaikoLocalServer.Adapters.GameProtocol.CnR00;
global using TaikoLocalServer.Adapters.GameProtocol.WwR08;
global using TaikoLocalServer.Application;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Domain;
global using TaikoLocalServer.Infrastructure;
```

- [ ] **Step 6: Move Logging/CsvFormatter.cs**

It's already in `Host/Logging/CsvFormatter.cs` (same filesystem location, just under the renamed parent). Update its namespace declaration if it was `TaikoLocalServer.Logging` to `TaikoLocalServer.Host.Logging` (or keep — it's referenced from Program.cs via `using TaikoLocalServer.Logging;`).

- [ ] **Step 7: Update Program.cs's namespace usings**

```bash
grep "namespace TaikoLocalServer" Host/Program.cs
grep "using TaikoLocalServer" Host/Program.cs
```

Update any `using TaikoLocalServer.Logging;` → `using TaikoLocalServer.Host.Logging;` if you renamed it; otherwise leave.

- [ ] **Step 8: Update LocalSaveModScoreMigrator and TaikoWebUI**

Neither directly references `TaikoLocalServer/` source — they ref Domain/Infrastructure/SharedProject. **No edits needed.**

But the `TaikoLocalServer.csproj` ProjectReference in TaikoWebUI.csproj is gone now (it never referenced TaikoLocalServer.csproj — verify). If anything refs `TaikoLocalServer.csproj`, change to `..\Host\Host.csproj`.

- [ ] **Step 9: Build**

```bash
dotnet build
```

Common errors:
- "The type or namespace `TaikoLocalServer.Filters` does not exist" — leftover reference; replace with `TaikoLocalServer.Adapters.AdminApi.Filters`.
- Path errors in ApplicationManifest, Properties/launchSettings.json — should still resolve relative to the new Host folder. Verify launchSettings.json references the project name only (not a path).

- [ ] **Step 10: Smoke**

```bash
dotnet run --project Host
```

Verify:
- `Server starting up...`
- `Mapped <N> endpoints` matches the Phase 3.0 baseline.
- All Kestrel ports bound.

Test route resolution for one endpoint per adapter:

```bash
curl -X POST http://localhost:80/sys/servlet/PowerOn -i        # AllnetMucha
curl http://localhost:5000/api/users -i                          # AdminApi
curl -X POST http://localhost:54430/v12r08_ww/chassis/heartbeat.php -i   # WwR08
curl -X POST http://localhost:57402/v12r00_cn/chassis/heartbeat.php -i   # CnR00
```

(Adjust port numbers per `Host/Configurations/Kestrel.json`.)

Expected: each route is reachable (NOT 404). 4xx/5xx for body issues is fine; 404 means routing failed.

- [ ] **Step 11: HeadClerk CSV log smoke**

Hit a HeadClerk2 endpoint:

```bash
curl -X POST http://localhost:54430/v12r08_ww/chassis/headclerk2.php -i
```

Then check that `Host/Logs/HeadClerkLog-*.csv` got a write (or that the existing CSV has a new row). The Serilog filter `StartsWith(@m, 'CSV WRITE:')` is content-based, not type-based, so the move shouldn't break it.

- [ ] **Step 12: Stop server**

`Ctrl+C`.

- [ ] **Step 13: Commit**

```bash
git add -A
git commit -m "PR3.6: rename TaikoLocalServer host project to Host (with AssemblyName=TaikoLocalServer)"
```

---

## Phase 3.7 — Slim Program.cs to extension-method composition

After PR2 + PR3.6, Program.cs already calls `AddApplication() + AddInfrastructure() + AddAdminApi() + AddAllnetMucha() + AddGameProtocolWwR08() + AddGameProtocolCnR00()`. Time to verify the result matches spec §6's "~80 lines."

### Task 3.7.1: Final Program.cs trim

**Files:**
- Modify: `Host/Program.cs`

- [ ] **Step 1: Inspect**

```bash
wc -l Host/Program.cs
```

Aim for ~85-100 LOC. Most of the size comes from configuration loading + Serilog setup + middleware wiring + endpoint logging — all of which stays.

- [ ] **Step 2: Confirm the registration block is the slim form**

The registration block should look like:

```csharp
    builder.Services.AddOptions();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddAdminApi(builder.Configuration);
    builder.Services.AddAllnetMucha();
    builder.Services.AddGameProtocolWwR08();
    builder.Services.AddGameProtocolCnR00();

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

    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.All;
        options.RequestBodyLogLimit = 32768;
        options.ResponseBodyLogLimit = 32768;
    });

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<GzipCompressionProvider>();
        options.Providers.Add<BrotliCompressionProvider>();
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    builder.Services.AddSingleton<SongBestResponseMapper>();
```

If the `AddDbContext`, `AddAuthentication`, or `Configure<AuthSettings>` calls are still inline in Program.cs (forgotten in PR2's Phase 2.18), move them to `AddInfrastructure` now.

- [ ] **Step 3: Verify endpoint pipeline preserved**

The middleware pipeline should be **byte-for-byte identical** to the pre-refactor version. Diff against the PR1 baseline if needed.

- [ ] **Step 4: Build + smoke**

```bash
dotnet build
dotnet run --project Host
```

Route count matches. Stop.

- [ ] **Step 5: Commit (only if changes were made)**

```bash
git add Host/Program.cs
git commit -m "PR3.7: final Program.cs trim — composition root via extension methods"
```

If no changes were needed (Program.cs was already at the target shape post-PR3.6), skip this commit.

---

## Phase 3.8 — Publish smoke (Risk #5)

### Task 3.8.1: dotnet publish

**Files:** none.

- [ ] **Step 1: Publish in Release mode**

```bash
dotnet publish -c Release Host/Host.csproj
```

Expected: success. Output directory:

```
Host/bin/Release/net10.0/win-x64/publish/
```

- [ ] **Step 2: Inspect output**

```bash
ls Host/bin/Release/net10.0/win-x64/publish/
```

Expected entries:
- `TaikoLocalServer.exe` (single-file self-contained)
- `Configurations/AuthSettings.json`, `Database.json`, `DataSettings.json`, `Kestrel.json`, `Logging.json`, `ServerSettings.json`
- `Certificates/cert.pfx`, `Certificates/root.pfx`
- `wwwroot/` directory
- `wwwroot/data/dan_data.json` etc. (12 server-owned JSONs)
- `wwwroot/data/datatable/` directory (may be empty depending on operator-supplied bins; verify the directory structure exists)

If anything is missing — likely the `<Content Update="..." />` entry didn't survive the csproj rewrite. Diff the new Host.csproj against the original TaikoLocalServer.csproj (visible in git history) for the missing block and re-add.

- [ ] **Step 3: Run the published exe**

```bash
cd Host/bin/Release/net10.0/win-x64/publish/
./TaikoLocalServer.exe
```

Expected:
- Same `Server starting up...` + `Mapped <N> endpoints` + `Now listening on:` lines.
- Repeat smoke tests from Phase 3.6 Step 10 against the **published** instance.

- [ ] **Step 4: Stop**

`Ctrl+C`. Return to repo root:

```bash
cd <back-to-repo-root>
```

- [ ] **Step 5: Verify the binary name didn't drift**

```bash
ls -la Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe
```

Expected: file exists. If it's `Host.exe` instead, `<AssemblyName>TaikoLocalServer</AssemblyName>` didn't take effect — double-check Host.csproj.

- [ ] **Step 6: Commit (none expected)**

The publish is an out-of-tree artifact — no commit needed unless csproj edits were required.

---

## Phase 3.9 — Final smoke gate

Per spec §7 PR3 verification gates:

### Task 3.9.1: Final smoke checklist

- [ ] **Step 1: Build green**

```bash
dotnet build
```

Expected: all 12 projects build, 0 errors.

- [ ] **Step 2: Project count**

```bash
grep -c "<Project Path=" TaikoLocalServer.slnx
```

Expected: 13.

- [ ] **Step 3: Server starts; route count matches PR2 baseline**

```bash
dotnet run --project Host
```

Watch `Mapped <N> endpoints`. Same N as PR2 baseline.

- [ ] **Step 4: One endpoint per game adapter**

```bash
curl -X POST http://localhost:54430/v12r08_ww/chassis/heartbeat.php -i
curl -X POST http://localhost:57402/v12r00_cn/chassis/heartbeat.php -i
```

Both: NOT 404.

- [ ] **Step 5: AllnetMucha smoke**

```bash
curl -X POST http://localhost:80/sys/servlet/PowerOn \
  --data 'mainid=AAVE-01A12345678&serial=ATKK00&placeid=&storeid=&countrycd=JPN&dvcid=' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -i
```

Expected: HTTP 200 + binary response.

- [ ] **Step 6: AdminApi smokes from PR2**

Same as PR2 Phase 2.20.4 (auth off): `GET /api/users`, `GET /api/users/{baid}`, `DELETE /api/users/{baid}`.

- [ ] **Step 7: HeadClerk2 CSV write**

After hitting any HeadClerk2 endpoint, verify `Logs/HeadClerkLog-*.csv` got a write.

- [ ] **Step 8: Blazor UI loads**

Browse to `http://localhost:5000/`. Dashboard renders. Console clean.

- [ ] **Step 9: Published exe smoke (already done in Phase 3.8)**

Confirmed.

- [ ] **Step 10: Stop server**

`Ctrl+C`.

---

## Phase 3.10 — Push and (optionally) open PR

### Task 3.10.1: Push the branch

- [ ] **Step 1: Confirm clean tree**

```bash
git status
```

- [ ] **Step 2: Push**

```bash
git push -u origin dev/clean-arch-adapters
```

- [ ] **Step 3: Open PR (only if user authorized)**

Ask before running `gh pr create`. If approved:

```bash
gh pr create --title "Clean architecture refactor PR3: Adapters extraction" --body "$(cat <<'EOF'
## Summary

Physically relocates controllers, mappers, wire types, middleware, and the host project into adapter-pattern projects:
- `TaikoLocalServer.Adapters.AdminApi`
- `TaikoLocalServer.Adapters.AllnetMucha`
- `TaikoLocalServer.Adapters.GameProtocol.{Shared, WwR08, CnR00}`
- `TaikoLocalServer.Host` (renamed from `TaikoLocalServer/`; `<AssemblyName>TaikoLocalServer</AssemblyName>` preserves the published exe name)

Spec: docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md (§7 PR3)

## Test plan

- [x] `dotnet build` green; .slnx contains 12 projects
- [x] Mapped endpoint count matches PR2 baseline
- [x] Both `_ww` and `_cn` game endpoints reachable
- [x] AllnetMucha PowerOn returns binary response
- [x] AdminApi smokes: `GET /api/users`, `GET /api/users/{baid}`, `DELETE /api/users/{baid}`
- [x] HeadClerk2 endpoint writes to `Logs/HeadClerkLog-*.csv`
- [x] Blazor UI loads from `wwwroot/`
- [x] `dotnet publish -c Release` produces `Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe` with `wwwroot/`, `Configurations/`, `Certificates/`; published exe smoke-tests pass

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)" --base dev
```

---

## Risk register specific to PR3

From spec §8:

| # | Risk | Mitigation |
|---|---|---|
| 4 | Game-side controller assemblies not discovered by `app.MapControllers()` | Phase 3.0 captures route count; every smoke step compares against it. |
| 5 | `dotnet publish` layout regression — missing `wwwroot/`, `Configurations/`, certs | Phase 3.8 explicitly inspects output directory before declaring done. |
| 6 | Stale `TaikoLocalServer.sln` confuses tooling/CI | Repo uses `.slnx`. Verify CI targets repo root or `.slnx` explicitly (read `.github/workflows/*.yml` if present). |
| 7 | Operator's existing shortcuts/scripts pointing at `TaikoLocalServer.exe` break after rename | `<AssemblyName>TaikoLocalServer</AssemblyName>` in Host.csproj preserves the binary name. Verified in Phase 3.8 Step 5. |
| 8 | Common DTO namespace collisions (`PlayResultDataRequest` in WwR08 wire vs `Common*` in Application) | Distinct namespaces resolve this. Mapperly will surface using-statement ambiguity at compile time. |
| 9 | Operator-supplied `wwwroot/data/datatable/*.bin` files accidentally not copied to publish output after wwwroot relocation | Phase 3.8 Step 2 verifies the empty `wwwroot/data/datatable/` directory survives in published output (operators add bins themselves; behavior preserved). |

---

## Definition of done for PR3

- [ ] All 12 projects build via `dotnet build` from repo root with 0 errors.
- [ ] `Adapters.AdminApi/`, `Adapters.AllnetMucha/`, `Adapters.GameProtocol.Shared/`, `Adapters.GameProtocol.WwR08/`, `Adapters.GameProtocol.CnR00/`, and `Host/` exist with the contents listed in §3.
- [ ] `TaikoLocalServer/` folder is gone (replaced by `Host/`).
- [ ] `TaikoLocalServer.csproj` is gone (replaced by `Host/Host.csproj` with `<AssemblyName>TaikoLocalServer</AssemblyName>`).
- [ ] Server's mapped endpoint count matches the Phase 3.0 baseline.
- [ ] One endpoint per adapter smoke-tested.
- [ ] `dotnet publish -c Release` produces `Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe` with `wwwroot/`, `Configurations/`, `Certificates/`; published smoke passes.
- [ ] Branch `dev/clean-arch-adapters` is pushed and (if user-authorized) a PR is open.

After merge, proceed to PR4 with `git checkout dev && git pull --ff-only` and **start a new Claude Code session for PR4 from the next plan file:** `docs/superpowers/plans/2026-05-04-clean-arch-pr4-contracts-webui.md`.
