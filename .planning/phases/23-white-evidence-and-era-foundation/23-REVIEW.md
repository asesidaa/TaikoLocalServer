---
phase: 23-white-evidence-and-era-foundation
reviewed: 2026-06-17T14:37:02Z
depth: standard
files_reviewed: 29
files_reviewed_list:
  - Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj
  - Adapters.GameProtocol.White/Controllers/BaidController.cs
  - Adapters.GameProtocol.White/Controllers/BookkeepingController.cs
  - Adapters.GameProtocol.White/Controllers/CrownsDataController.cs
  - Adapters.GameProtocol.White/Controllers/GetFolderController.cs
  - Adapters.GameProtocol.White/Controllers/GetTelopController.cs
  - Adapters.GameProtocol.White/Controllers/HeartbeatController.cs
  - Adapters.GameProtocol.White/Controllers/InitialDataCheckController.cs
  - Adapters.GameProtocol.White/Controllers/MyDonEntryController.cs
  - Adapters.GameProtocol.White/Controllers/PlayResultController.cs
  - Adapters.GameProtocol.White/Controllers/RecommendController.cs
  - Adapters.GameProtocol.White/Controllers/SelfBestController.cs
  - Adapters.GameProtocol.White/Controllers/TaikojukuController.cs
  - Adapters.GameProtocol.White/Controllers/TournamentCheckController.cs
  - Adapters.GameProtocol.White/Controllers/UserDataController.cs
  - Adapters.GameProtocol.White/DependencyInjection.cs
  - Adapters.GameProtocol.White/GlobalUsings.cs
  - Adapters.GameProtocol.White/MapperlyDefaults.cs
  - Adapters.GameProtocol.White/WhiteAdapterMarker.cs
  - Adapters.GameProtocol.White/Wire/Game.cs
  - Adapters.GameProtocol.White/Wire/VsInterface.cs
  - Domain/Enums/GameEra.cs
  - Host/Configurations/ServerSettings.json
  - Host/Host.csproj
  - Host/Program.cs
  - TaikoLocalServer.slnx
  - Tests/Tests.csproj
  - Tests/White/WhiteHostRouteGatingTests.cs
  - Tests/White/WhiteServerSettingsValidationTests.cs
findings:
  critical: 0
  warning: 1
  info: 0
  total: 1
status: issues_found
---

# Phase 23: Code Review Report

**Reviewed:** 2026-06-17T14:37:02Z
**Depth:** standard
**Files Reviewed:** 29
**Status:** issues_found

## Summary

Reviewed the Phase 23 White adapter scaffold, host gating/configuration changes, generated wire scope, and White tests against the foundation-only constraints. The source builds, the White-focused test slice passes, and the reviewed controllers stay within the approved `/v07r00/chassis/{suffix}.php` route set without adding catalog/profile/runtime/AdminApi/WebUI/EF/Mediator behavior.

One test-quality defect remains: the route-gating test does not exercise the production settings-driven gating path, so it can pass while disabled White routes are exposed by a host wiring regression.

## Narrative Findings (AI reviewer)

## Warnings

### WR-01: Route-gating test bypasses the production disabled-era path

**Classification:** WARNING

**File:** `Tests/White/WhiteHostRouteGatingTests.cs:54`

**Issue:** `DiscoverWhiteRoutes` builds its own MVC service collection, removes White with the test-local `RemoveWhiteApplicationPart`, and conditionally re-adds the White assembly. That proves the White controllers have the expected attributes, but it does not prove `Host/Program.cs` removes `TaikoLocalServer.Adapters.GameProtocol.White` when `ServerSettings:Eras:White:Enabled` is false. A regression in the production application-part filter, such as deleting the White branch or mistyping the assembly name, would still leave this test passing while disabled White routes become routable in the real host.

**Fix:** Exercise the host gating behavior from settings instead of reproducing it in the test. The strongest regression guard is a host-level request test that starts the app with White disabled and asserts a White route is 404, then starts with White enabled and asserts the same route is routable. If full host startup is too heavy, extract the production application-part filtering into a small host helper used by `Program.cs` and the test, rather than maintaining a separate `RemoveWhiteApplicationPart` implementation in the test.

```csharp
[Fact]
public async Task WhiteRoutesFollowHostServerSettings()
{
    await using var disabledHost = await WhiteHostFixture.StartAsync(whiteEnabled: false);
    var disabled = await disabledHost.Client.PostAsync(
        "/v07r00/chassis/heartbeat.php",
        ProtobufContent.Create(new HeartBeatRequest { ChassisId = "test", ShopId = "test" }));
    Assert.Equal(HttpStatusCode.NotFound, disabled.StatusCode);

    await using var enabledHost = await WhiteHostFixture.StartAsync(whiteEnabled: true);
    var enabled = await enabledHost.Client.PostAsync(
        "/v07r00/chassis/heartbeat.php",
        ProtobufContent.Create(new HeartBeatRequest { ChassisId = "test", ShopId = "test" }));
    Assert.True(enabled.IsSuccessStatusCode);
}
```

## Verification Notes

- `dotnet build Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj --no-restore` passed.
- `dotnet test Tests/Tests.csproj --no-restore --filter "FullyQualifiedName~White"` passed.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build" --no-restore` passed.
- `dotnet test Tests/Tests.csproj --no-restore --filter "FullyQualifiedName~White|FullyQualifiedName~RedServerSettingsValidationTests|FullyQualifiedName~StartupAuthController"` passed.
- `git diff --check d59e2c79^..HEAD -- ...` passed for the reviewed scope.

---

_Reviewed: 2026-06-17T14:37:02Z_
_Reviewer: the agent (gsd-code-reviewer)_
_Depth: standard_
