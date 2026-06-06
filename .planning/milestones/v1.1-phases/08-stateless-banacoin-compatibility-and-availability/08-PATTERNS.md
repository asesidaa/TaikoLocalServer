# Phase 8 Pattern Map: Stateless Banacoin Compatibility

**Mapped:** 2026-06-04
**Status:** Ready for planning

## Target Files

| Target | Role | Closest Pattern | Notes |
|--------|------|-----------------|-------|
| `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs` | New stateless Blue direct-protobuf controller | `BanacoinPaymentController.cs`, `BanacoinErrorLogController.cs`, `BalanceCheckController.cs` | Use `[ApiController]`, exact `/v10r03/chassis/getbanacoininfo.php` route, `[HttpPost]`, `[Produces("application/protobuf")]`, `request.Stringify()` logging, and direct `Ok(...)` response. |
| `Tests/Blue/BlueRouteSkeletonTests.cs` | Route ownership test update | Existing `ExpectedBlueGameRoutes` and excluded/shared route theory | Move `getbanacoininfo.php` from excluded route list into the owned Blue route list. Preserve exact-route assertions. |

## Controller Pattern

Existing stateless Blue Banacoin controllers do not call Mediator and do not touch persistence:

```csharp
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

Phase 8 should follow the same pattern but return only the required field:

```csharp
return Ok(new GetbanacoininfoResponse { Result = 1 });
```

Do not set optional `PlayerType`, `ComSvrResult`, `MbId`, `Baid`, `AccessCode`, `IsPublish`, `CardOwnNum`, `RegCountryId`, `PurposeId`, `RegionId`, or `Personid` unless new client evidence proves a required value.

## Test Pattern

`BlueRouteSkeletonTests.BlueGameRoutes_AreOwnedByBlueAdapter` is an exact set check for Blue-owned `/v10r03/chassis/*` routes. Adding `getbanacoininfo.php` requires:

- Add `/v10r03/chassis/getbanacoininfo.php` to `ExpectedBlueGameRoutes`.
- Remove `[InlineData("/v10r03/chassis/getbanacoininfo.php")]` from `BlueAdapter_DoesNotOwnExcludedOrSharedRoutes`.

`BlueControllers_DoNotCallMediatorOutsideImplementedEndpoints` already guards stateless controllers against Mediator usage as long as the new controller is not added to `mediatorBackedControllers`.

## Guard Pattern

Phase 8 should not add a new source-scan suite. Use focused PowerShell checks during execution:

- New controller source must not contain `Mediator.Send`.
- New controller source must not contain `DbContext`, `ITaikoDbContext`, `SaveChanges`, or `AddMigration`.
- Existing Banacoin controllers must remain stateless.

These checks support TKBC-02 without inventing low-value response-constant tests.

## Verification Pattern

Recommended execution verification:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueRouteSkeletonTests
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase8"
```

Use a temp-output Host build because a running server may lock `Host/bin/Debug/net10.0`.

## Pattern Mapping Complete
