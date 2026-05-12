# 05 — `Adapters.GameProtocol.Green` project scaffold

**Surface:** Create the new adapter project. Generate protobuf-net wire types from `proto/green/green.proto` and `proto/green/vsinterface.proto`. Stub Mapperly mappers. Stub controllers for all 25 `/v11r01/chassis/*.php` routes.

**Why this comes after 04:** The catalog multiplex is in place, so Green controllers can lazily reference `catalog.Green()` in stub bodies (even if the calls return empty data today). Mediator request types from 03 are wired and ready to receive Green calls.

**Verification cadence:** `dotnet build` after each task. End-of-file verification: the Green adapter compiles, its controllers are visible to MVC's ApplicationPartManager, and stub bodies return success-shaped protobuf responses.

---

## Task 05.1: Create the `Adapters.GameProtocol.Green` project skeleton

**Goal:** New `.csproj`, project structure, references, `GlobalUsings.cs`, marker class, and minimal `DependencyInjection.cs`.

**Files:**
- Create: `Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj`
- Create: `Adapters.GameProtocol.Green/GlobalUsings.cs`
- Create: `Adapters.GameProtocol.Green/GreenAdapterMarker.cs`
- Create: `Adapters.GameProtocol.Green/DependencyInjection.cs`
- Modify: `TaikoLocalServer.slnx` (add the new project to the solution)
- Modify: `Host/Host.csproj` (`<ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />`)

**Acceptance Criteria:**
- [ ] `Adapters.GameProtocol.Green` builds standalone.
- [ ] `dotnet build` of the full solution succeeds.
- [ ] `Host` references the new project.
- [ ] `dotnet sln list` shows the new project.

**Verify:**
```bash
dotnet build
dotnet sln list | grep Adapters.GameProtocol.Green
```
Expected: build PASSES; sln list shows the new project.

**Steps:**

- [ ] **Step 1: Create `Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj`**

Copy the structure from `Adapters.GameProtocol.WwR08/Adapters.GameProtocol.WwR08.csproj`. The key elements:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.Green</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
    <ProjectReference Include="..\Application\Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <!-- Match WwR08's package references: protobuf-net, Mapperly, Mediator, ASP.NET Core MVC -->
    <PackageReference Include="protobuf-net" />
    <PackageReference Include="protobuf-net.AspNetCore" />
    <PackageReference Include="Riok.Mapperly" />
    <PackageReference Include="Mediator.SourceGenerator" PrivateAssets="all" />
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
</Project>
```

Reference the exact package versions used in `Adapters.GameProtocol.WwR08.csproj`. If the repo uses Central Package Management (`Directory.Packages.props`), versions live there and the csproj only names the packages.

- [ ] **Step 2: Create `Adapters.GameProtocol.Green/GlobalUsings.cs`**

Mirror `Adapters.GameProtocol.WwR08/GlobalUsings.cs`:

```csharp
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging;

global using Mediator;
global using ProtoBuf;

global using TaikoLocalServer.Adapters.GameProtocol.Shared;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Handlers;
global using TaikoLocalServer.Domain.Enums;

// Wire types (added in Task 05.2)
global using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;
```

- [ ] **Step 3: Create the marker class**

```csharp
// Adapters.GameProtocol.Green/GreenAdapterMarker.cs
namespace TaikoLocalServer.Adapters.GameProtocol.Green;

/// <summary>
/// Marker class used by Host/Program.cs ApplicationPart filter to identify this assembly.
/// </summary>
internal sealed class GreenAdapterMarker;
```

- [ ] **Step 4: Create `Adapters.GameProtocol.Green/DependencyInjection.cs`**

```csharp
// Adapters.GameProtocol.Green/DependencyInjection.cs
namespace TaikoLocalServer.Adapters.GameProtocol.Green;

using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Domain.Enums;

public static class GameProtocolGreen
{
    public const GameEra Era = GameEra.Green;

    public static IServiceCollection AddGameProtocolGreen(this IServiceCollection services)
    {
        // Mapperly mappers are source-generated and use static methods, so no DI registration needed.
        // Controllers are auto-discovered by ASP.NET Core's default ApplicationPart scanning;
        // Host/Program.cs is responsible for REMOVING this assembly's ApplicationPart when Green is disabled.
        return services;
    }
}
```

- [ ] **Step 5: Register the project with the solution**

Run:
```bash
dotnet sln TaikoLocalServer.slnx add Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj
```

Or if the `.slnx` format isn't supported by your `dotnet sln` version, edit `TaikoLocalServer.slnx` manually to add the project entry alongside `Adapters.GameProtocol.WwR08`.

- [ ] **Step 6: Add the ProjectReference to `Host/Host.csproj`**

```xml
<ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
```

- [ ] **Step 7: Build**

Run: `dotnet build`
Expected: PASS (project exists, no controllers yet, so no MVC discovery happens — but the project is in the build).

- [ ] **Step 8: Commit**

```bash
git add Adapters.GameProtocol.Green TaikoLocalServer.slnx Host/Host.csproj
git commit -m "feat(adapter): scaffold Adapters.GameProtocol.Green project skeleton"
```

---

## Task 05.2: Generate protobuf-net wire types from `proto/green/green.proto` and `proto/green/vsinterface.proto`

**Goal:** Generate the C# protobuf-net wire types and place them under `Adapters.GameProtocol.Green/Wire/`.

**Files:**
- Create: `Adapters.GameProtocol.Green/Wire/Game.cs` — generated from `proto/green/green.proto`
- Create: `Adapters.GameProtocol.Green/Wire/VsInterface.cs` — generated from `proto/green/vsinterface.proto`

**Approach:** Use the same `protogen` tool the existing `WwR08` adapter's `Wire/Game.cs` was generated from. The tool is part of protobuf-net's package. Compare the existing `Adapters.GameProtocol.WwR08/Wire/Game.cs` header for the exact command line / pragmas used.

**Acceptance Criteria:**
- [ ] `Adapters.GameProtocol.Green/Wire/Game.cs` exists and declares every message from `proto/green/green.proto` (BookKeepingRequest/Response, HeartBeatRequest/Response, BAIDRequest/Response, MydonEntryRequest/Response, UserDataRequest/Response, PlayResultRequest/Response, PlayResultDataRequest, SelfBestRequest/Response, RecommendRequest/Response, CrownsDataRequest/Response, GetfolderRequest/Response, GettelopRequest/Response, GetitemshopinfoRequest/Response, TaikojukuRequest/Response, InitialdatacheckRequest/Response, TournamentcheckRequest/Response, ChallengeCompeRequest/Response, ItempurchaseRequest/Response, GetghostdataRequest/Response, GetghostscoreRequest/Response, RewardcardcheckRequest/Response, RewardexecutionRequest/Response, HeadClerk2Request/Response, GetreitaiRequest/Response).
- [ ] `Adapters.GameProtocol.Green/Wire/VsInterface.cs` exists and declares StartupAuthRequest/Response, VerupAuthRequest/Response, VerupCompleteRequest/Response from `proto/green/vsinterface.proto`.
- [ ] Namespaces are `TaikoLocalServer.Adapters.GameProtocol.Green.Wire` (or similar — match what protogen produces; rename to match WwR08's convention).
- [ ] `dotnet build` succeeds.

**Steps:**

- [ ] **Step 1: Inspect how WwR08's Wire files were generated**

```bash
head -20 Adapters.GameProtocol.WwR08/Wire/Game.cs
```

Note any tool-generated comment (e.g. `// <auto-generated> ... protogen 3.x.y ... </auto-generated>`) or pragma like `[global::ProtoBuf.ProtoContract(...)]`. This tells you protogen was used.

- [ ] **Step 2: Run `protogen` against `proto/green/green.proto`**

If `protogen` is a dotnet tool:
```bash
dotnet tool install --global protobuf-net.Protogen   # if not already installed
protogen --csharp_out=Adapters.GameProtocol.Green/Wire/ --proto_path=proto/green/ proto/green/green.proto
mv Adapters.GameProtocol.Green/Wire/green.cs Adapters.GameProtocol.Green/Wire/Game.cs
```

If it's a Visual Studio extension, run the same command via the equivalent CLI (`protogen-csharp` etc.) — match what the team did for WwR08.

- [ ] **Step 3: Run `protogen` against `proto/green/vsinterface.proto`**

```bash
protogen --csharp_out=Adapters.GameProtocol.Green/Wire/ --proto_path=proto/green/ proto/green/vsinterface.proto
mv Adapters.GameProtocol.Green/Wire/vsinterface.cs Adapters.GameProtocol.Green/Wire/VsInterface.cs
```

- [ ] **Step 4: Edit the generated files**

- Change top-of-file `namespace` declarations to `TaikoLocalServer.Adapters.GameProtocol.Green.Wire` for both files.
- Confirm no top-level naming clashes with the WwR08 wire types (each adapter has its own namespace, so no collisions).
- If protogen emits `using` statements like `using ProtoBuf;` at the top of each file, leave them; the global usings in `GlobalUsings.cs` cover it but explicit `using` doesn't hurt.

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add Adapters.GameProtocol.Green/Wire
git commit -m "feat(adapter): add Green wire types generated from proto/green/{green,vsinterface}.proto"
```

---

## Task 05.3: Stub Mapperly mappers

**Goal:** Declare every Mapperly mapper class as a `[Mapper] static partial class` with method signatures only. Bodies are partial declarations that the Mapperly source generator will fill in. For mappings where the Common DTO union has nullable fields the Mapperly default mapping doesn't fit, declare an explicit user-implemented method body returning `null` / default with a `// TODO iter 2` comment.

**Files:**
- Create: `Adapters.GameProtocol.Green/Mappers/BaidResponseMapper.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/SelfBestMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/InitialDataMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/FolderDataMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/TaikojukuMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/ChallengeCompeMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/GhostMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/TournamentMappers.cs`
- Create: `Adapters.GameProtocol.Green/Mappers/RecommendMappers.cs`

**Acceptance Criteria:**
- [ ] Each file declares a `[Mapper] public static partial class <Name>` with `public static partial` method declarations covering the wire-↔-Common pairings for that endpoint.
- [ ] `dotnet build` succeeds — Mapperly's source generator either emits the bodies automatically (for trivial maps) or skips the method (the partial declaration alone compiles).

**Steps:**

- [ ] **Step 1: For each mapper, declare the partial signatures only**

Example — `PlayResultMappers.cs`:

```csharp
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    // Wire PlayResultDataRequest → Common DTO. Mapperly auto-maps fields with matching names;
    // Green-only fields on CommonPlayResultData.Green.cs receive their Green-specific data here.
    // TODO iter 2: review generated diagnostics and fill in custom map methods for fields
    // that don't match by name.
    public static partial CommonPlayResultData Map(PlayResultDataRequest request);

    public static partial PlayResultResponse Map(uint result);
}
```

Example — `BaidResponseMapper.cs`:

```csharp
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class BaidResponseMapper
{
    public static partial BAIDResponse Map(CommonBaidResponse common);
}
```

Apply the same minimal-stub pattern to the rest. Where the Mapperly generator can't auto-map (mismatched names / nested unions / nullable handling), add a single `[MapperIgnoreSource(nameof(SomeField))]` / `[MapProperty(nameof(Src), nameof(Dst))]` attribute as needed. In stub posture, prefer ignoring rather than mapping — the goal is to compile, not to produce correct data.

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: PASS. Mapperly will emit warnings about unmapped source/destination members — that's expected for stubs. The build succeeds because warnings aren't errors. If the generator emits errors (rare — usually only for genuinely incompatible types), add `[MapperIgnoreSource]` or `[MapperIgnoreTarget]` attributes to silence them for now.

- [ ] **Step 3: Commit**

```bash
git add Adapters.GameProtocol.Green/Mappers
git commit -m "feat(adapter): stub Mapperly mappers for Green wire ↔ Common DTO maps"
```

---

## Task 05.4: Stub controllers for all 25 `/v11r01/chassis/*.php` routes

**Goal:** One controller per route. Each is a `BaseProtocolController<TController>` subclass with the right route attribute and a stub body returning a success-shaped protobuf response.

**Files (under `Adapters.GameProtocol.Green/Controllers/`):**
- `HeartbeatController.cs`
- `StartupAuthController.cs`
- `VerupAuthController.cs`
- `VerupCompleteController.cs`
- `BookkeepingController.cs`
- `InitialDataCheckController.cs`
- `TournamentCheckController.cs`
- `GetTelopController.cs`
- `GetFolderController.cs`
- `BaidController.cs`
- `MyDonEntryController.cs`
- `UserDataController.cs`
- `PlayResultController.cs`
- `SelfBestController.cs`
- `CrownsDataController.cs`
- `ChallengeCompeController.cs`
- `RecommendController.cs`
- `ItemPurchaseController.cs`
- `GetItemShopInfoController.cs`
- `GetGhostDataController.cs`
- `GetGhostScoreController.cs`
- `RewardCardCheckController.cs`
- `RewardExecutionController.cs`
- `TaikojukuController.cs`
- `HeadClerk2Controller.cs`

**Acceptance Criteria:**
- [ ] Each file routes its endpoint to `/v11r01/chassis/<name>.php`.
- [ ] Each controller inherits `BaseProtocolController<TController>`.
- [ ] Each endpoint accepts the right protobuf request type and returns a success-shaped response (e.g. `new XResponse { Result = 1 }`).
- [ ] No `Mediator.Send` calls in stub bodies — keep them entirely local for now. (Real wiring through Mediator arrives in iter 2.)
- [ ] `dotnet build` succeeds.

**Steps:**

- [ ] **Step 1: Heartbeat controller (no Mediator, simplest example)**

```csharp
// Adapters.GameProtocol.Green/Controllers/HeartbeatController.cs
namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/heartbeat.php")]
public class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Green Heartbeat: chassis={ChassisId} shop={ShopId}", request.ChassisId, request.ShopId);
        var response = new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1,
            BnidSvrStat = 1,
            BanacoinStat = 1
        };
        return Ok(response);
    }
}
```

- [ ] **Step 2: VsInterface controllers (StartupAuth, VerupAuth, VerupComplete)**

`StartupAuthController.cs` follows the existing WwR08 `StartupAuthController.cs` near-verbatim, with the route changed and namespaces updated:

```csharp
// Adapters.GameProtocol.Green/Controllers/StartupAuthController.cs
namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/startupauth.php")]
public class StartupAuthController : BaseProtocolController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth([FromBody] StartupAuthRequest request)
    {
        Logger.LogInformation("Green StartupAuth request: {Request}", request.Stringify());
        var response = new StartupAuthResponse { Result = 1 };
        // Echo operation info back (same pattern as WwR08's StartupAuth)
        var info = request.AryOperationInfoes.ConvertAll(input => new StartupAuthResponse.OperationData
        {
            KeyData = input.KeyData,
            ValueData = input.ValueData
        });
        response.AryOperationInfoes.AddRange(info);
        return Ok(response);
    }
}
```

`VerupAuthController.cs`:
```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/verupauth.php")]
public class VerupAuthController : BaseProtocolController<VerupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupAuth([FromBody] VerupAuthRequest request)
    {
        Logger.LogInformation("Green VerupAuth request: {Request}", request.Stringify());
        return Ok(new VerupAuthResponse { Result = 1 });
    }
}
```

`VerupCompleteController.cs`: same pattern with `VerupCompleteRequest` / `VerupCompleteResponse`.

- [ ] **Step 3: Game-protocol controllers — single-message endpoints**

Most controllers follow this template. Example for `BaidController.cs`:

```csharp
// Adapters.GameProtocol.Green/Controllers/BaidController.cs
namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Green Baid request: {Request}", request.Stringify());
        // TODO iter 2: dispatch to Mediator BaidQuery(GameEra.Green, ...) and map response.
        // For now: minimal success response — Result=1 means baid auth succeeded.
        // Use a fixed placeholder baid (1) so the cabinet proceeds through its flow.
        return Ok(new BAIDResponse
        {
            Result = 1,
            Baid = 1,
            // Other optional fields left at default — Green cabinet treats missing optionals as defaults.
        });
    }
}
```

- [ ] **Step 4: PlayResult — special wire shape (gzip + 32-byte header)**

Mirror the existing `Adapters.GameProtocol.WwR08/Controllers/PlayResultController.cs` decompression logic:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Green PlayResult request: baid={Baid} chassis={ChassisId}", request.BaidConf, request.ChassisIdConf);
        // The PlayresultData blob is gzip-compressed with a 32-byte header (per existing
        // WwR08 convention). For the stub, do NOT decode — just acknowledge.
        // TODO iter 2: truncate-32, gunzip, deserialize PlayResultDataRequest, map to
        // CommonPlayResultData (Green partial), and dispatch UpdatePlayResultCommand(Era.Green).
        return Ok(new PlayResultResponse { Result = 1 });
    }
}
```

- [ ] **Step 5: Pure-stub controllers for the rest**

For each remaining controller, follow this pattern:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/<endpoint>.php")]
public class <Name>Controller : BaseProtocolController<<Name>Controller>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult <Method>([FromBody] <Request> request)
    {
        Logger.LogInformation("Green <Name>: chassis={ChassisId}", request.ChassisId);
        return Ok(new <Response> { Result = 1 });
    }
}
```

Cover the remaining 18 controllers (all the ones not handled in Steps 1–4) following this template. Fill the `<endpoint>` / `<Method>` / `<Request>` / `<Response>` slots:

| Controller | Endpoint | Request | Response |
|---|---|---|---|
| `BookkeepingController` | `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` |
| `InitialDataCheckController` | `initialdatacheck.php` | `InitialdatacheckRequest` | `InitialdatacheckResponse` |
| `TournamentCheckController` | `tournamentcheck.php` | `TournamentcheckRequest` | `TournamentcheckResponse` |
| `GetTelopController` | `gettelop.php` | `GettelopRequest` | `GettelopResponse` |
| `GetFolderController` | `getfolder.php` | `GetfolderRequest` | `GetfolderResponse` |
| `MyDonEntryController` | `mydonentry.php` | `MydonEntryRequest` | `MydonEntryResponse` |
| `UserDataController` | `userdata.php` | `UserDataRequest` | `UserDataResponse` |
| `SelfBestController` | `selfbest.php` | `SelfBestRequest` | `SelfBestResponse` |
| `CrownsDataController` | `crownsdata.php` | `CrownsDataRequest` | `CrownsDataResponse` |
| `ChallengeCompeController` | `challengecompe.php` | `ChallengeCompeRequest` | `ChallengeCompeResponse` |
| `RecommendController` | `recommend.php` | `RecommendRequest` | `RecommendResponse` |
| `ItemPurchaseController` | `itempurchase.php` | `ItempurchaseRequest` | `ItempurchaseResponse` |
| `GetItemShopInfoController` | `getitemshopinfo.php` | `GetitemshopinfoRequest` | `GetitemshopinfoResponse` |
| `GetGhostDataController` | `getghostdata.php` | `GetghostdataRequest` | `GetghostdataResponse` |
| `GetGhostScoreController` | `getghostscore.php` | `GetghostscoreRequest` | `GetghostscoreResponse` |
| `RewardCardCheckController` | `rewardcardcheck.php` | `RewardcardcheckRequest` | `RewardcardcheckResponse` |
| `RewardExecutionController` | `rewardexecution.php` | `RewardexecutionRequest` | `RewardexecutionResponse` |
| `TaikojukuController` | `taikojuku.php` | `TaikojukuRequest` | `TaikojukuResponse` |
| `HeadClerk2Controller` | `headclerk2.php` | `HeadClerk2Request` | `HeadClerk2Response` |

Note: protobuf type names follow `proto/green/green.proto` — `Mydon`/`BookKeeping`/etc. capitalization matches the proto exactly. After protogen, the types may be CamelCased differently; use what protogen produced.

- [ ] **Step 6: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add Adapters.GameProtocol.Green/Controllers
git commit -m "feat(adapter): add 25 stub controllers for /v11r01/chassis/*.php routes"
```
