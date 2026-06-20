---
status: complete
quick_id: 260620-ub3
task: support-white-final-version-11-01-under-v07r03
completed: 2026-06-20
commits:
  - 2d618f44
  - aada5791
  - ebd32377
  - 19cf0643
  - 87616f0e
---

# Quick Task 260620-ub3 Summary

White final 11.01 direct-protobuf support now uses `proto/white-final/taiko.proto`, exposes existing White handlers under `/v07r03/chassis/*` while preserving `/v07r00/chassis/*`, and adds White-owned Tokkun, stateless Banacoin compatibility, plus difficulty panel persistence/readback.

## Completed Tasks

| Task | Result | Commit |
|---|---|---|
| Regenerate White final wire DTOs | `Adapters.GameProtocol.White/Wire/Game.cs` regenerated from `proto/white-final/taiko.proto` with the White adapter namespace | `2d618f44` |
| Add `/v07r03` aliases | Existing White controllers expose both final and compatibility prefixes; Host protobuf fallback accepts both | `aada5791` |
| Add White Tokkun state | Added nullable `TokkunTutorialFlg`, `WhiteTokkunStageResults`, EF migration, handler routing, mapper coverage, and no-cross-mode tests | `ebd32377` |
| Wire difficulty panel fields | Normal White playresults persist difficulty tutorial/course/star with presence checks; userdata returns saved fields | `19cf0643` |
| Add Banacoin compatibility routes | Added stateless `getbanacoininfo.php`, `balancecheck.php`, `banacoinpayment.php`, and `banacoinerrorlog.php` under both White prefixes | `87616f0e` |

## Key Files

- `Adapters.GameProtocol.White/Wire/Game.cs`
- `Adapters.GameProtocol.White/WhiteRoutePrefixes.cs`
- `Adapters.GameProtocol.White/Controllers/BalanceCheckController.cs`
- `Adapters.GameProtocol.White/Controllers/BanacoinErrorLogController.cs`
- `Adapters.GameProtocol.White/Controllers/BanacoinPaymentController.cs`
- `Adapters.GameProtocol.White/Controllers/GetBanacoinInfoController.cs`
- `Adapters.GameProtocol.White/Mappers/PlayResultMappers.cs`
- `Adapters.GameProtocol.White/Mappers/UserDataMappers.cs`
- `Application/Handlers/UpdatePlayResultCommand.White.cs`
- `Application/Handlers/UpdatePlayResultCommand.WhiteTokkun.cs`
- `Application/Handlers/UserDataQuery.White.cs`
- `Domain/Entities/UserSaveDataWhite.cs`
- `Domain/Entities/WhiteTokkunStageResult.cs`
- `Infrastructure/Persistence/Migrations/20260620141329_AddWhiteFinalTokkunState.cs`
- `Tests/White/WhiteRuntimeHandlerTests.cs`
- `Tests/White/WhiteTokkunPersistenceTests.cs`
- `Tests/White/WhiteBanacoinCompatibilityTests.cs`

## Verification

- `dotnet build Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj` - passed.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WhiteHostRouteGatingTests"` - passed, 2 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WhiteRuntimeHandlerTests"` - passed, 12 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WhiteTokkunPersistenceTests|FullyQualifiedName~WhiteRuntimeHandlerTests"` - passed, 14 tests.
- `dotnet build Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj /p:EmitCompilerGeneratedFiles=true` - passed, 0 warnings/errors.
- Inspected generated Mapperly files under `Adapters.GameProtocol.White/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/` for `PlayResultMappers.g.cs` and `UserDataMappers.g.cs`; generated assignments map White difficulty/Tokkun fields and keep unsupported mode sections null.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~White"` - passed, 41 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~RedPlayResultHandlerTests"` - passed, 13 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed, 0 warnings/errors.
- Follow-up Banacoin fix:
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WhiteBanacoinCompatibilityTests|FullyQualifiedName~WhiteHostRouteGatingTests"` - passed, 6 tests.
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~White"` - passed, 45 tests.
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~RedPlayResultHandlerTests"` - passed, 13 tests.
  - `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed, 0 warnings/errors.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Regenerated White final wire with explicit namespace**
- **Found during:** Task 2 verification.
- **Issue:** Initial `protogen` output without `--package` emitted types into the global namespace, causing unrelated Blue/Green test compile errors.
- **Fix:** Regenerated with `--package=TaikoLocalServer.Adapters.GameProtocol.White.Wire` and amended the wire commit.
- **Files modified:** `Adapters.GameProtocol.White/Wire/Game.cs`
- **Commit:** `2d618f44`

**2. [Rule 3 - Blocking] Updated test-only `ITaikoDbContext` stub**
- **Found during:** Task 3 verification.
- **Issue:** `GreenAuthConfigTests.ThrowingTaikoDbContext` needed the new `WhiteTokkunStageResults` member after the White context contract changed.
- **Fix:** Added a throwing `DbSet<WhiteTokkunStageResult>` property to the test stub.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Commit:** `ebd32377`

**3. [Review follow-up] Added White final Banacoin compatibility endpoints**
- **Found during:** User review after quick-task closeout.
- **Issue:** The first pass reused the older White 0.13 absence allowlist even though `proto/white-final/taiko.proto` defines Banacoin-adjacent final White messages.
- **Fix:** Added stateless success-shaped White controllers for `getbanacoininfo.php`, `balancecheck.php`, `banacoinpayment.php`, and `banacoinerrorlog.php`, with route coverage under `/v07r03/chassis/*` and `/v07r00/chassis/*`.
- **Files modified:** `Adapters.GameProtocol.White/Controllers/*Banacoin*.cs`, `Adapters.GameProtocol.White/Controllers/BalanceCheckController.cs`, `Tests/White/WhiteBanacoinCompatibilityTests.cs`, `Tests/White/WhiteHostRouteGatingTests.cs`
- **Commit:** `87616f0e`

## Known Stubs

None. Stub scan matches were generated protobuf optional-member helpers and intentional entity/default empty-array initializers, not unfinished runtime behavior.

## Manual Gate

Manual RPCS3/cabinet verification and WebUI review remain the final compatibility gate. Automated tests and builds do not prove cabinet compatibility.
