# Blue A1 Era Foundation Adapter Skeleton Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Blue as an enableable first-class game-protocol era with generated wire types, shared `/v01r00` startup/version route ownership, `/v10r03` game-route stubs, and scoped protobuf content-type handling.

**Architecture:** Blue gets its own adapter assembly beside Green, with generated protobuf-net game wire types and controller stubs only. Shared Blue/Green `vsinterface.proto` behavior stays in `Adapters.GameProtocol.Shared`; Blue game behavior stays shallow until A2 and later stages define catalog, identity, persistence, and gameplay semantics.

**Tech Stack:** .NET 10, ASP.NET Core controllers, protobuf-net/protogen, xUnit, PowerShell, `TaikoLocalServer.slnx`.

---

## Preconditions

- Start from the repo root: `H:\TaikoLocalServer`.
- Inspect the current worktree before editing:

```powershell
git status --short
git diff --stat
git diff --cached --name-status
```

Expected at plan-writing time: `Host/.gitignore` may be modified and `Host/wwwroot/data/blue/.gitkeep` may already be staged. Do not stage or commit those files unless the user explicitly asks.

## File Structure

- `Domain/Enums/GameEra.cs` owns stable era ids.
- `Host/Configurations/ServerSettings.json` declares enabled/disabled eras for local server startup.
- `TaikoLocalServer.slnx`, `Host/Host.csproj`, and `Tests/Tests.csproj` wire the new Blue project into build and test flows.
- `Adapters.GameProtocol.Blue/` owns Blue game-protocol routes and Blue game wire DTOs.
- `Adapters.GameProtocol.Shared/Wire/StartupAuth.cs` already owns shared startup wire DTOs and will also own shared `VerupAuth*` and `VerupComplete*` DTOs.
- `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs` and `VerupCompleteController.cs` will own `/v01r00/chassis/verupauth.php` and `/v01r00/chassis/verupcomplete.php`.
- `Host/Program.cs` wires Blue registration, Blue application-part removal, and `/v10r03/chassis` missing-content-type handling.
- `Tests/Blue/` contains focused A1 tests. Source-level tests are acceptable for `Program.cs` because it is a top-level program with private local helper functions.

---

### Task 1: Era Enum And Shipped Settings

**Files:**
- Create: `Tests/Blue/BlueEraFoundationTests.cs`
- Modify: `Domain/Enums/GameEra.cs`
- Modify: `Host/Configurations/ServerSettings.json`

- [ ] **Step 1: Write failing era/settings tests**

Create `Tests/Blue/BlueEraFoundationTests.cs`:

```csharp
using Microsoft.Extensions.Configuration;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueEraFoundationTests
{
    [Fact]
    public void GameEra_Blue_HasStableNumericValue()
    {
        Assert.Equal(2, (int)GameEra.Blue);
        Assert.Equal(GameEra.Blue, Enum.Parse<GameEra>("Blue", ignoreCase: true));
    }

    [Fact]
    public void ShippedServerSettings_DeclaresBlueDisabledByDefault()
    {
        var path = FindServerSettingsPath();
        Assert.NotNull(path);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        var blue = configuration.GetSection("ServerSettings:Eras:Blue");

        Assert.True(blue.Exists());
        Assert.True(blue.GetSection("Enabled").Exists());
        Assert.False(blue.GetValue<bool>("Enabled"));
    }

    private static string? FindServerSettingsPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(
                directory.FullName,
                "Host",
                "Configurations",
                "ServerSettings.json");

            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
```

- [ ] **Step 2: Run the failing test**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueEraFoundationTests
```

Expected: fail to compile because `GameEra.Blue` does not exist, or fail because the shipped settings do not declare Blue.

- [ ] **Step 3: Add Blue to `GameEra`**

Change `Domain/Enums/GameEra.cs` to:

```csharp
namespace TaikoLocalServer.Domain.Enums;

public enum GameEra
{
    Nijiiro = 0,
    Green = 1,
    Blue = 2
}
```

- [ ] **Step 4: Add disabled Blue settings**

In `Host/Configurations/ServerSettings.json`, add this sibling under `ServerSettings:Eras`:

```json
"Blue": {
  "Enabled": false
}
```

Keep existing Green/Nijiiro local values unchanged. If the file already has local operator settings, make the smallest valid JSON edit that only adds the Blue block.

- [ ] **Step 5: Verify the test passes**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueEraFoundationTests
```

Expected: PASS.

- [ ] **Step 6: Commit**

Run:

```powershell
git diff -- Domain/Enums/GameEra.cs Host/Configurations/ServerSettings.json Tests/Blue/BlueEraFoundationTests.cs
git add -- Domain/Enums/GameEra.cs Host/Configurations/ServerSettings.json Tests/Blue/BlueEraFoundationTests.cs
git commit -m "Add Blue era foundation"
```

---

### Task 2: Blue Adapter Project Skeleton

**Files:**
- Create: `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj`
- Create: `Adapters.GameProtocol.Blue/BlueAdapterMarker.cs`
- Create: `Adapters.GameProtocol.Blue/DependencyInjection.cs`
- Create: `Adapters.GameProtocol.Blue/GlobalUsings.cs`
- Modify: `TaikoLocalServer.slnx`
- Modify: `Host/Host.csproj`
- Modify: `Tests/Tests.csproj`

- [ ] **Step 1: Create the Blue adapter project file**

Create `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.Blue</RootNamespace>
    <AssemblyName>TaikoLocalServer.Adapters.GameProtocol.Blue</AssemblyName>
    <NoWarn>$(NoWarn);RMG020;RMG012</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Contracts.AdminApi\Contracts.AdminApi.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="protobuf-net" />
    <PackageReference Include="Riok.Mapperly" />
    <PackageReference Include="Swan.Core" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create Blue globals and marker**

Create `Adapters.GameProtocol.Blue/GlobalUsings.cs`:

```csharp
// Global using directives

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using ProtoBuf;
global using Swan.Formatters;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Domain.Enums;
```

Create `Adapters.GameProtocol.Blue/BlueAdapterMarker.cs`:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue;

internal sealed class BlueAdapterMarker;
```

Create `Adapters.GameProtocol.Blue/DependencyInjection.cs`:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Blue;

    public static IServiceCollection AddGameProtocolBlue(this IServiceCollection services)
    {
        return services;
    }
}
```

- [ ] **Step 3: Add project references**

Insert this project entry in `TaikoLocalServer.slnx` beside Green:

```xml
<Project Path="Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj" />
```

Insert this `ProjectReference` in `Host/Host.csproj` beside the other protocol adapters:

```xml
<ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
```

Insert this `ProjectReference` in `Tests/Tests.csproj` beside the other protocol adapters:

```xml
<ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
```

- [ ] **Step 4: Build the empty adapter**

Run:

```powershell
dotnet build Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj
```

Expected: PASS. The Blue project has no controllers or wire globals yet.

- [ ] **Step 5: Commit after Task 4 if needed**

Commit:

```powershell
git add -- Adapters.GameProtocol.Blue TaikoLocalServer.slnx Host/Host.csproj Tests/Tests.csproj
git commit -m "Add Blue game protocol adapter skeleton"
```

---

### Task 3: Shared `/v01r00` Version Routes

**Files:**
- Create: `Tests/Blue/ProtocolRouteTestHelper.cs`
- Create: `Tests/Blue/BlueSharedVersionRouteTests.cs`
- Modify: `Adapters.GameProtocol.Shared/Wire/StartupAuth.cs`
- Create: `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs`
- Create: `Adapters.GameProtocol.Shared/Controllers/VerupCompleteController.cs`

- [ ] **Step 1: Add reusable route-test helper**

Create `Tests/Blue/ProtocolRouteTestHelper.cs`:

```csharp
using System.Reflection;

namespace TaikoLocalServer.Tests.Blue;

internal static class ProtocolRouteTestHelper
{
    public static IEnumerable<RouteInfo> FindPostRoutes(params Assembly[] assemblies)
    {
        foreach (var type in assemblies.SelectMany(assembly => assembly.DefinedTypes))
        {
            if (!type.IsClass || type.IsAbstract || !type.Name.EndsWith("Controller", StringComparison.Ordinal))
            {
                continue;
            }

            var controllerRoutes = GetRouteTemplates(type).ToArray();
            if (controllerRoutes.Length == 0)
            {
                continue;
            }

            var postActions = type.DeclaredMethods.Where(HasHttpPostAttribute).ToArray();
            foreach (var action in postActions)
            {
                var actionRoutes = GetRouteTemplates(action).ToArray();
                foreach (var controllerRoute in controllerRoutes)
                {
                    if (actionRoutes.Length == 0)
                    {
                        yield return new RouteInfo(type.Assembly.GetName().Name ?? string.Empty, controllerRoute);
                        continue;
                    }

                    foreach (var actionRoute in actionRoutes)
                    {
                        yield return new RouteInfo(
                            type.Assembly.GetName().Name ?? string.Empty,
                            CombineRoutes(controllerRoute, actionRoute));
                    }
                }
            }
        }
    }

    private static IEnumerable<string> GetRouteTemplates(MemberInfo member)
    {
        foreach (var attribute in CustomAttributeData.GetCustomAttributes(member))
        {
            if (attribute.AttributeType.FullName != "Microsoft.AspNetCore.Mvc.RouteAttribute")
            {
                continue;
            }

            if (attribute.ConstructorArguments is [{ Value: string template }])
            {
                yield return template;
            }
        }
    }

    private static bool HasHttpPostAttribute(MemberInfo member)
    {
        return CustomAttributeData.GetCustomAttributes(member)
            .Any(attribute => attribute.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.HttpPostAttribute");
    }

    private static string CombineRoutes(string controllerRoute, string actionRoute)
    {
        if (actionRoute.StartsWith('/'))
        {
            return actionRoute;
        }

        return $"{controllerRoute.TrimEnd('/')}/{actionRoute.TrimStart('/')}";
    }
}

internal sealed record RouteInfo(string AssemblyName, string Template);
```

- [ ] **Step 2: Write failing shared version-route tests**

Create `Tests/Blue/BlueSharedVersionRouteTests.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
using GreenVerupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.VerupAuthRequest;
using GreenVerupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.VerupAuthResponse;
using GreenVerupCompleteRequest = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.VerupCompleteRequest;
using GreenVerupCompleteResponse = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.VerupCompleteResponse;
using SharedVerupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupAuthRequest;
using SharedVerupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupAuthResponse;
using SharedVerupCompleteRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupCompleteRequest;
using SharedVerupCompleteResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.VerupCompleteResponse;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueSharedVersionRouteTests
{
    [Theory]
    [InlineData("/v01r00/chassis/startupauth.php")]
    [InlineData("/v01r00/chassis/verupauth.php")]
    [InlineData("/v01r00/chassis/verupcomplete.php")]
    public void VersionRoutes_AreOwnedBySharedProtocolAdapter(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(BaseProtocolController<>).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        var route = Assert.Single(routes);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Shared", route.AssemblyName);
    }

    [Theory]
    [InlineData("/v10r03/chassis/startupauth.php")]
    [InlineData("/v10r03/chassis/verupauth.php")]
    [InlineData("/v10r03/chassis/verupcomplete.php")]
    public void BlueAdapter_DoesNotOwnVersionRoutesUnderGamePrefix(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        Assert.Empty(routes);
    }

    [Fact]
    public void SharedVerupAuthRequest_ReadsGreenVsInterfacePayload()
    {
        var request = new GreenVerupAuthRequest
        {
            ChassisId = "chassis",
            UsbmemKey = "usb",
            HddVer = 123,
            UsbmemVer = 456,
            ShopId = "shop",
            RackId = "rack",
            CountryId = "JPN"
        };

        var shared = Deserialize<SharedVerupAuthRequest>(Serialize(request));

        Assert.Equal("chassis", shared.ChassisId);
        Assert.Equal("usb", shared.UsbmemKey);
        Assert.Equal(123u, shared.HddVer);
        Assert.Equal(456u, shared.UsbmemVer);
        Assert.Equal("shop", shared.ShopId);
        Assert.Equal("rack", shared.RackId);
        Assert.Equal("JPN", shared.CountryId);
    }

    [Fact]
    public void SharedVerupAuthResponse_WritesVsInterfacePayload()
    {
        var green = Deserialize<GreenVerupAuthResponse>(
            Serialize(new SharedVerupAuthResponse { Result = 1 }));

        Assert.Equal(1u, green.Result);
    }

    [Fact]
    public void SharedVerupCompleteRequest_ReadsGreenVsInterfacePayload()
    {
        var request = new GreenVerupCompleteRequest
        {
            ChassisId = "chassis",
            UsbmemKey = "usb",
            HddVer = 123,
            UsbmemVer = 456,
            ShopId = "shop",
            RackId = "rack",
            CountryId = "JPN"
        };

        var shared = Deserialize<SharedVerupCompleteRequest>(Serialize(request));

        Assert.Equal("chassis", shared.ChassisId);
        Assert.Equal("usb", shared.UsbmemKey);
        Assert.Equal(123u, shared.HddVer);
        Assert.Equal(456u, shared.UsbmemVer);
        Assert.Equal("shop", shared.ShopId);
        Assert.Equal("rack", shared.RackId);
        Assert.Equal("JPN", shared.CountryId);
    }

    [Fact]
    public void SharedVerupCompleteResponse_WritesVsInterfacePayload()
    {
        var green = Deserialize<GreenVerupCompleteResponse>(
            Serialize(new SharedVerupCompleteResponse { Result = 1 }));

        Assert.Equal(1u, green.Result);
    }

    private static byte[] Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        ProtoBuf.Serializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static T Deserialize<T>(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return ProtoBuf.Serializer.Deserialize<T>(stream);
    }
}
```

- [ ] **Step 3: Run the failing shared-route tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueSharedVersionRouteTests
```

Expected: fail to compile because shared `VerupAuth*` and `VerupComplete*` wire types/controllers do not exist.

- [ ] **Step 4: Add shared version wire types**

Append these types to `Adapters.GameProtocol.Shared/Wire/StartupAuth.cs` in the same namespace:

```csharp
[ProtoContract]
public sealed class VerupAuthRequest
{
    [ProtoMember(1, Name = @"chassis_id", IsRequired = true)]
    public string ChassisId { get; set; } = string.Empty;

    [ProtoMember(2, Name = @"usbmem_key", IsRequired = true)]
    public string UsbmemKey { get; set; } = string.Empty;

    [ProtoMember(3, Name = @"hdd_ver", IsRequired = true)]
    public uint HddVer { get; set; }

    [ProtoMember(4, Name = @"usbmem_ver", IsRequired = true)]
    public uint UsbmemVer { get; set; }

    [ProtoMember(5, Name = @"shop_id", IsRequired = true)]
    public string ShopId { get; set; } = string.Empty;

    [ProtoMember(6, Name = @"rack_id")]
    public string? RackId { get; set; }

    [ProtoMember(7, Name = @"country_id")]
    public string? CountryId { get; set; }
}

[ProtoContract]
public sealed class VerupAuthResponse
{
    [ProtoMember(1, Name = @"result", IsRequired = true)]
    public uint Result { get; set; }
}

[ProtoContract]
public sealed class VerupCompleteRequest
{
    [ProtoMember(1, Name = @"chassis_id", IsRequired = true)]
    public string ChassisId { get; set; } = string.Empty;

    [ProtoMember(2, Name = @"usbmem_key", IsRequired = true)]
    public string UsbmemKey { get; set; } = string.Empty;

    [ProtoMember(3, Name = @"hdd_ver", IsRequired = true)]
    public uint HddVer { get; set; }

    [ProtoMember(4, Name = @"usbmem_ver", IsRequired = true)]
    public uint UsbmemVer { get; set; }

    [ProtoMember(5, Name = @"shop_id", IsRequired = true)]
    public string ShopId { get; set; } = string.Empty;

    [ProtoMember(6, Name = @"rack_id")]
    public string? RackId { get; set; }

    [ProtoMember(7, Name = @"country_id")]
    public string? CountryId { get; set; }
}

[ProtoContract]
public sealed class VerupCompleteResponse
{
    [ProtoMember(1, Name = @"result", IsRequired = true)]
    public uint Result { get; set; }
}
```

- [ ] **Step 5: Add shared version controllers**

Create `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Shared.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

[ApiController]
[Route("/v01r00/chassis/verupauth.php")]
public sealed class VerupAuthController : BaseProtocolController<VerupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupAuth([FromBody] VerupAuthRequest request)
    {
        Logger.LogInformation("VerupAuth request: {Request}", request.Stringify());
        return Ok(new VerupAuthResponse { Result = 1 });
    }
}
```

Create `Adapters.GameProtocol.Shared/Controllers/VerupCompleteController.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Shared.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

[ApiController]
[Route("/v01r00/chassis/verupcomplete.php")]
public sealed class VerupCompleteController : BaseProtocolController<VerupCompleteController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupComplete([FromBody] VerupCompleteRequest request)
    {
        Logger.LogInformation("VerupComplete request: {Request}", request.Stringify());
        return Ok(new VerupCompleteResponse { Result = 1 });
    }
}
```

- [ ] **Step 6: Verify shared-route tests pass**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueSharedVersionRouteTests
```

Expected: PASS.

- [ ] **Step 7: Commit**

Run:

```powershell
git add -- Tests/Blue/ProtocolRouteTestHelper.cs Tests/Blue/BlueSharedVersionRouteTests.cs Adapters.GameProtocol.Shared/Wire/StartupAuth.cs Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs Adapters.GameProtocol.Shared/Controllers/VerupCompleteController.cs
git commit -m "Add shared v01r00 version route stubs"
```

---

### Task 4: Generated Blue Game Wire Types

**Files:**
- Create: `Tests/Blue/BlueWireGenerationTests.cs`
- Create: `Adapters.GameProtocol.Blue/Wire/Game.cs`

- [ ] **Step 1: Write failing wire-generation tests**

Create `Tests/Blue/BlueWireGenerationTests.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueWireGenerationTests
{
    [Fact]
    public void BlueWireTypes_ContainBlueOnlyA0Messages()
    {
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(CoinsettingRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BalancecheckRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BanacoinpaymentRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BanacoinerrorlogRequest).Namespace);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BattleUserDataRequest).Namespace);
    }

    [Fact]
    public void BluePlayResultRequest_IsDirectRequestShape()
    {
        Assert.NotNull(typeof(PlayResultRequest).GetProperty(nameof(PlayResultRequest.Baid)));
        Assert.NotNull(typeof(PlayResultRequest).GetProperty(nameof(PlayResultRequest.AryStageInfoes)));
        Assert.Null(typeof(PlayResultRequest).GetProperty("PlayresultData"));
    }
}
```

- [ ] **Step 2: Run the failing wire test**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueWireGenerationTests
```

Expected: fail because `Adapters.GameProtocol.Blue.Wire.Game.cs` does not exist.

- [ ] **Step 3: Generate Blue wire types**

Check whether `protogen` is available:

```powershell
protogen --version
```

Expected if installed: a protobuf-net protogen version line. If PowerShell reports that `protogen` is not recognized, install the tool:

```powershell
dotnet tool install --global protobuf-net.Protogen
```

Generate the file:

```powershell
New-Item -ItemType Directory -Force 'Adapters.GameProtocol.Blue/Wire'
protogen --csharp_out=Adapters.GameProtocol.Blue/Wire --proto_path=proto/blue taiko.proto
Move-Item -Force 'Adapters.GameProtocol.Blue/Wire/taiko.cs' 'Adapters.GameProtocol.Blue/Wire/Game.cs'
```

If protogen emits a different filename, list the directory and move the generated Blue game file to `Game.cs`:

```powershell
Get-ChildItem 'Adapters.GameProtocol.Blue/Wire'
```

- [ ] **Step 4: Set the generated namespace**

Edit only the namespace declaration in `Adapters.GameProtocol.Blue/Wire/Game.cs` so it reads:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Wire
```

Keep the generated message bodies intact.

Add the Blue wire global using to `Adapters.GameProtocol.Blue/GlobalUsings.cs`:

```csharp
global using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
```

- [ ] **Step 5: Verify wire tests and adapter build**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueWireGenerationTests
dotnet build Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj
```

Expected: both PASS.

- [ ] **Step 6: Commit skeleton plus wire files**

Commit:

```powershell
git add -- Adapters.GameProtocol.Blue/Wire/Game.cs Adapters.GameProtocol.Blue/GlobalUsings.cs Tests/Blue/BlueWireGenerationTests.cs
git commit -m "Add Blue generated game wire types"
```

---

### Task 5: Blue `/v10r03` Stub Controllers

**Files:**
- Create: `Tests/Blue/BlueRouteSkeletonTests.cs`
- Create: all controller files under `Adapters.GameProtocol.Blue/Controllers/`

- [ ] **Step 1: Write failing route skeleton tests**

Create `Tests/Blue/BlueRouteSkeletonTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueRouteSkeletonTests
{
    private static readonly string[] ExpectedBlueGameRoutes =
    [
        "/v10r03/chassis/initialdatacheck.php",
        "/v10r03/chassis/tournamentcheck.php",
        "/v10r03/chassis/bookkeeping.php",
        "/v10r03/chassis/coinsetting.php",
        "/v10r03/chassis/gettelop.php",
        "/v10r03/chassis/getfolder.php",
        "/v10r03/chassis/taikojuku.php",
        "/v10r03/chassis/getitemshopinfo.php",
        "/v10r03/chassis/headclerk2.php",
        "/v10r03/chassis/playresult.php",
        "/v10r03/chassis/banacoinerrorlog.php",
        "/v10r03/chassis/baidcheck.php",
        "/v10r03/chassis/mydonentry.php",
        "/v10r03/chassis/userdata.php",
        "/v10r03/chassis/challengecompe.php",
        "/v10r03/chassis/balancecheck.php",
        "/v10r03/chassis/banacoinpayment.php",
        "/v10r03/chassis/crownsdata.php",
        "/v10r03/chassis/recommend.php",
        "/v10r03/chassis/selfbest.php",
        "/v10r03/chassis/heartbeat.php",
        "/v10r03/chassis/itempurchase.php",
        "/v10r03/chassis/battleuserdata.php",
        "/v10r03/chassis/rewardcardcheck.php",
        "/v10r03/chassis/rewardexecution.php"
    ];

    [Fact]
    public void BlueGameRoutes_AreOwnedByBlueAdapter()
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
            .Where(route => route.Template.StartsWith("/v10r03/chassis", StringComparison.OrdinalIgnoreCase))
            .Select(route => route.Template)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedBlueGameRoutes.Order(StringComparer.Ordinal).ToArray(), routes);
    }

    [Theory]
    [InlineData("/v10r03/chassis/getreitai.php")]
    [InlineData("/v10r03/chassis/getbanacoininfo.php")]
    [InlineData("/v10r03/chassis/startupauth.php")]
    [InlineData("/v10r03/chassis/verupauth.php")]
    [InlineData("/v10r03/chassis/verupcomplete.php")]
    public void BlueAdapter_DoesNotOwnExcludedOrSharedRoutes(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        Assert.Empty(routes);
    }

    [Fact]
    public void BlueStubControllers_DoNotCallMediator()
    {
        var root = FindRepoRoot();
        var controllersRoot = Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers");

        foreach (var file in Directory.EnumerateFiles(controllersRoot, "*.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("Mediator.Send", source, StringComparison.Ordinal);
        }
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
```

- [ ] **Step 2: Run the failing route tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRouteSkeletonTests
```

Expected: fail because Blue controllers do not exist.

- [ ] **Step 3: Add simple `Result = 1` route stubs**

Create the following controller files. Each file uses namespace `TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers`, inherits `BaseProtocolController<TController>`, logs the request with `request.Stringify()`, and returns the response listed here:

| File | Route | Method | Request | Response expression |
|---|---|---|---|---|
| `BookkeepingController.cs` | `/v10r03/chassis/bookkeeping.php` | `Bookkeeping` | `BookKeepingRequest` | `new BookKeepingResponse { Result = 1 }` |
| `CoinSettingController.cs` | `/v10r03/chassis/coinsetting.php` | `CoinSetting` | `CoinsettingRequest` | `new CoinsettingResponse { Result = 1 }` |
| `HeadClerk2Controller.cs` | `/v10r03/chassis/headclerk2.php` | `HeadClerk2` | `HeadClerk2Request` | `new HeadClerk2Response { Result = 1 }` |
| `PlayResultController.cs` | `/v10r03/chassis/playresult.php` | `PlayResult` | `PlayResultRequest` | `new PlayResultResponse { Result = 1 }` |
| `BanacoinErrorLogController.cs` | `/v10r03/chassis/banacoinerrorlog.php` | `BanacoinErrorLog` | `BanacoinerrorlogRequest` | `new BanacoinerrorlogResponse { Result = 1 }` |
| `ChallengeCompeController.cs` | `/v10r03/chassis/challengecompe.php` | `ChallengeCompe` | `ChallengeCompeRequest` | `new ChallengeCompeResponse { Result = 1 }` |
| `CrownsDataController.cs` | `/v10r03/chassis/crownsdata.php` | `CrownsData` | `CrownsDataRequest` | `new CrownsDataResponse { Result = 1 }` |
| `RecommendController.cs` | `/v10r03/chassis/recommend.php` | `Recommend` | `RecommendRequest` | `new RecommendResponse { Result = 1 }` |
| `SelfBestController.cs` | `/v10r03/chassis/selfbest.php` | `SelfBest` | `SelfBestRequest` | `new SelfBestResponse { Result = 1 }` |
| `ItemPurchaseController.cs` | `/v10r03/chassis/itempurchase.php` | `ItemPurchase` | `ItempurchaseRequest` | `new ItempurchaseResponse { Result = 1 }` |
| `RewardCardCheckController.cs` | `/v10r03/chassis/rewardcardcheck.php` | `RewardCardCheck` | `RewardcardcheckRequest` | `new RewardcardcheckResponse { Result = 1 }` |
| `RewardExecutionController.cs` | `/v10r03/chassis/rewardexecution.php` | `RewardExecution` | `RewardexecutionRequest` | `new RewardexecutionResponse { Result = 1 }` |
| `GetFolderController.cs` | `/v10r03/chassis/getfolder.php` | `GetFolder` | `GetfolderRequest` | `new GetfolderResponse { Result = 1 }` |
| `GetItemShopInfoController.cs` | `/v10r03/chassis/getitemshopinfo.php` | `GetItemShopInfo` | `GetitemshopinfoRequest` | `new GetitemshopinfoResponse { Result = 1 }` |
| `GetTelopController.cs` | `/v10r03/chassis/gettelop.php` | `GetTelop` | `GettelopRequest` | `new GettelopResponse { Result = 1 }` |
| `InitialDataCheckController.cs` | `/v10r03/chassis/initialdatacheck.php` | `InitialDataCheck` | `InitialdatacheckRequest` | `new InitialdatacheckResponse { Result = 1 }` |
| `TaikojukuController.cs` | `/v10r03/chassis/taikojuku.php` | `Taikojuku` | `TaikojukuRequest` | `new TaikojukuResponse { Result = 1 }` |
| `TournamentCheckController.cs` | `/v10r03/chassis/tournamentcheck.php` | `TournamentCheck` | `TournamentcheckRequest` | `new TournamentcheckResponse { Result = 1 }` |
| `UserDataController.cs` | `/v10r03/chassis/userdata.php` | `UserData` | `UserDataRequest` | `new UserDataResponse { Result = 1 }` |
| `BattleUserDataController.cs` | `/v10r03/chassis/battleuserdata.php` | `BattleUserData` | `BattleUserDataRequest` | `new BattleUserDataResponse { Result = 1 }` |

Use this exact file pattern, replacing the route, class, method, request, and response expression from the table:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/bookkeeping.php")]
public class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Blue Bookkeeping request: {Request}", request.Stringify());
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
```

- [ ] **Step 4: Add heartbeat and identity stubs**

Create `Adapters.GameProtocol.Blue/Controllers/HeartbeatController.cs`:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/heartbeat.php")]
public class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Blue Heartbeat request: {Request}", request.Stringify());
        return Ok(new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1,
            BnidSvrStat = 1,
            BanacoinStat = 1
        });
    }
}
```

Create `Adapters.GameProtocol.Blue/Controllers/BaidController.cs`:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Blue BAID request: {Request}", request.Stringify());
        return Ok(new BAIDResponse
        {
            Result = 1,
            PlayerType = 1,
            AccessCode = request.AccessCode,
            IsPublish = true
        });
    }
}
```

Create `Adapters.GameProtocol.Blue/Controllers/MyDonEntryController.cs`:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/mydonentry.php")]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult MyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Blue MyDonEntry request: {Request}", request.Stringify());
        return Ok(new MydonEntryResponse
        {
            Result = 1,
            AccessCode = request.AccessCode,
            IsPublish = true,
            MydonName = request.MydonName
        });
    }
}
```

- [ ] **Step 5: Add Banacoin response stubs with required `personid`**

Create `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs`:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/balancecheck.php")]
public class BalanceCheckController : BaseProtocolController<BalanceCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BalanceCheck([FromBody] BalancecheckRequest request)
    {
        Logger.LogInformation("Blue BalanceCheck request: {Request}", request.Stringify());
        return Ok(new BalancecheckResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = string.Empty,
            CoinCoupon = 0
        });
    }
}
```

Create `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs`:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/banacoinpayment.php")]
public class BanacoinPaymentController : BaseProtocolController<BanacoinPaymentController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinPayment([FromBody] BanacoinpaymentRequest request)
    {
        Logger.LogInformation("Blue BanacoinPayment request: {Request}", request.Stringify());
        return Ok(new BanacoinpaymentResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = string.Empty,
            Chid = string.Empty
        });
    }
}
```

- [ ] **Step 6: Verify route skeleton tests pass**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRouteSkeletonTests
dotnet build Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj
```

Expected: PASS.

- [ ] **Step 7: Commit**

Run:

```powershell
git add -- Tests/Blue/BlueRouteSkeletonTests.cs Adapters.GameProtocol.Blue/Controllers
git commit -m "Add Blue v10r03 route stubs"
```

---

### Task 6: Host Registration And Scoped Content-Type Handling

**Files:**
- Create: `Tests/Blue/BlueHostProgramSourceTests.cs`
- Modify: `Host/Program.cs`

- [ ] **Step 1: Write failing Host wiring tests**

Create `Tests/Blue/BlueHostProgramSourceTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueHostProgramSourceTests
{
    [Fact]
    public void Program_RegistersAndFiltersBlueAdapter()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "Program.cs"));

        Assert.Contains("using TaikoLocalServer.Adapters.GameProtocol.Blue;", source, StringComparison.Ordinal);
        Assert.Contains("enabledEras.Contains(GameEra.Blue)", source, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddGameProtocolBlue();", source, StringComparison.Ordinal);
        Assert.Contains("RemoveApplicationPart(apm, \"TaikoLocalServer.Adapters.GameProtocol.Blue\");", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_AssumesProtobufForBlueGameRoutes()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "Program.cs"));

        Assert.Contains("path.StartsWithSegments(\"/v10r03/chassis\", StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("path.StartsWithSegments(\"/v10r03\", StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
    }

    private static string FindRepoFile(params string[] pathParts)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Could not find {Path.Combine(pathParts)}.");
    }
}
```

- [ ] **Step 2: Run failing Host wiring tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueHostProgramSourceTests
```

Expected: fail because Host does not import/register/filter Blue and does not include `/v10r03/chassis`.

- [ ] **Step 3: Add Blue Host import and registration**

In `Host/Program.cs`, add:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Blue;
```

After the Green registration block, add:

```csharp
if (enabledEras.Contains(GameEra.Blue))
{
    builder.Services.AddGameProtocolBlue();
}
```

Inside `ConfigureApplicationPartManager`, add this disabled-era removal block:

```csharp
if (!enabledEras.Contains(GameEra.Blue))
{
    RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Blue");
}
```

Update the zero-enabled fatal log text so it names Blue:

```csharp
Log.Fatal("ServerSettings.Eras has no enabled era. At least one era (Nijiiro, Green, or Blue) must be enabled in Host/Configurations/ServerSettings.json. Refusing to start.");
```

- [ ] **Step 4: Add Blue scoped missing-content-type fallback**

In `ShouldAssumeProtobufRequest`, add Blue's game route prefix:

```csharp
return path.StartsWithSegments("/v11r01/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v10r03/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v12r08_ww/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v12r00_cn/chassis", StringComparison.OrdinalIgnoreCase);
```

- [ ] **Step 5: Verify Host wiring tests pass**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueHostProgramSourceTests
```

Expected: PASS.

- [ ] **Step 6: Build Host**

Run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a1"
```

Expected: PASS. Use the temp output path even if normal `Host/bin` is locked.

- [ ] **Step 7: Commit**

Run:

```powershell
git add -- Tests/Blue/BlueHostProgramSourceTests.cs Host/Program.cs
git commit -m "Wire Blue adapter into host startup"
```

---

### Task 7: Final Verification And Guardrails

**Files:**
- No source files unless a previous task missed a required edit.

- [ ] **Step 1: Run focused Blue tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
```

Expected: PASS.

- [ ] **Step 2: Build Blue adapter**

Run:

```powershell
dotnet build Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj
```

Expected: PASS.

- [ ] **Step 3: Build Host through a temp output path**

Run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a1"
```

Expected: PASS.

- [ ] **Step 4: Verify no forbidden Blue routes exist**

Run:

```powershell
rg -n "getreitai|getbanacoininfo|startupauth|verupauth|verupcomplete" Adapters.GameProtocol.Blue
```

Expected: no output. The Blue adapter must not own proto-only or shared version routes.

- [ ] **Step 5: Verify Blue stubs do not call Mediator**

Run:

```powershell
rg -n "Mediator\\.Send" Adapters.GameProtocol.Blue
```

Expected: no output.

- [ ] **Step 6: Verify only intended files are staged for any final commit**

Run:

```powershell
git status --short
git diff --cached --name-status
```

Expected: no uncommitted A1 source/doc changes remain. Pre-existing unrelated local changes, such as `Host/.gitignore` or `Host/wwwroot/data/blue/.gitkeep`, may still appear if they were present before execution.
