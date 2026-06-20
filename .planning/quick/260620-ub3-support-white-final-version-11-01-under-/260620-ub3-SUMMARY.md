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
  - dbd37c3a
---

# Quick Task 260620-ub3 Summary

White final 11.01 direct-protobuf support now uses `proto/white-final/taiko.proto` under `/v07r03/chassis/*`, while `/v07r00/chassis/*` is served through regenerated legacy White wire DTOs from `proto/white/taiko.proto`. Both versions reuse the same Application handlers where behavior is compatible, but they do not share protocol controllers or response DTOs. Final White adds White-owned Tokkun, stateless Banacoin compatibility, plus difficulty panel persistence/readback.

## Completed Tasks

| Task | Result | Commit |
|---|---|---|
| Regenerate White final wire DTOs | `Adapters.GameProtocol.White/Wire/Game.cs` regenerated from `proto/white-final/taiko.proto` with the White adapter namespace | `2d618f44` |
| Add `/v07r03` final routes | Final White controllers expose `/v07r03/chassis/*`; `/v07r00/chassis/*` was later split back to legacy controllers and wire DTOs | `aada5791`, `dbd37c3a` |
| Add White Tokkun state | Added nullable `TokkunTutorialFlg`, `WhiteTokkunStageResults`, EF migration, handler routing, mapper coverage, and no-cross-mode tests | `ebd32377` |
| Wire difficulty panel fields | Normal White playresults persist difficulty tutorial/course/star with presence checks; userdata returns saved fields | `19cf0643` |
| Add Banacoin compatibility routes | Added stateless final White `getbanacoininfo.php`, `balancecheck.php`, `banacoinpayment.php`, and `banacoinerrorlog.php`; legacy `/v07r00` remains on the old non-Banacoin schema | `87616f0e`, `dbd37c3a` |
| Split final and legacy protocol layers | Added `LegacyWire` DTOs/controllers/mappers for `/v07r00`, removed compatibility routes from final controllers, and made final heartbeat emit Banacoin status fields 4/5 | `dbd37c3a` |

## Key Files

- `Adapters.GameProtocol.White/Wire/Game.cs`
- `Adapters.GameProtocol.White/LegacyWire/Game.cs`
- `Adapters.GameProtocol.White/LegacyWire/LegacyControllers.cs`
- `Adapters.GameProtocol.White/LegacyWire/LegacyMappers.cs`
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
- `Tests/White/WhiteProtocolVersionCompatibilityTests.cs`

## Verification

- `dotnet build Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj` - passed.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WhiteRuntimeHandlerTests"` - passed, 12 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WhiteTokkunPersistenceTests|FullyQualifiedName~WhiteRuntimeHandlerTests"` - passed, 14 tests.
- `dotnet build Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj /p:EmitCompilerGeneratedFiles=true` - passed, 0 warnings/errors.
- Inspected generated Mapperly files under `Adapters.GameProtocol.White/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/` for `PlayResultMappers.g.cs` and `UserDataMappers.g.cs`; generated assignments map White difficulty/Tokkun fields and keep unsupported mode sections null.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~White"` - passed, 41 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~RedPlayResultHandlerTests"` - passed, 13 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed, 0 warnings/errors.
- Follow-up Banacoin fix:
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~White"` - passed, 45 tests.
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~RedPlayResultHandlerTests"` - passed, 13 tests.
  - `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed, 0 warnings/errors.
- Critical protocol split correction:
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WhiteProtocolVersionCompatibilityTests"` - passed, 2 tests.
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~White"` - passed, 45 tests.
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~RedPlayResultHandlerTests"` - passed, 13 tests.
  - `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed, 0 warnings/errors.
  - `dotnet build Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj /p:EmitCompilerGeneratedFiles=true` - passed, 0 warnings/errors.
  - Inspected generated Mapperly files under `Adapters.GameProtocol.White/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/` for `LegacyPlayResultMappers.g.cs` and `LegacyUserDataMappers.g.cs`; legacy playresult maps unsupported mode sections to null and legacy userdata omits final-only tutorial/difficulty fields.

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
- **Files modified:** `Adapters.GameProtocol.White/Controllers/*Banacoin*.cs`, `Adapters.GameProtocol.White/Controllers/BalanceCheckController.cs`, `Tests/White/WhiteBanacoinCompatibilityTests.cs`
- **Commit:** `87616f0e`

**4. [Critical review follow-up] Split `/v07r03` final protocol from `/v07r00` legacy protocol**
- **Found during:** User review after Banacoin follow-up.
- **Issue:** The previous fix shared final White controllers and generated final DTOs with `/v07r00`, which made legacy White serialize final-only fields such as heartbeat Banacoin status fields.
- **Fix:** Regenerated the old White proto into `LegacyWire`, added compatibility-only legacy controllers/mappers, removed compatibility routes from final controllers, and replaced the rejected route/source-shape test with a protobuf byte-presence behavior test.
- **Files modified:** `Adapters.GameProtocol.White/Controllers/*`, `Adapters.GameProtocol.White/LegacyWire/*`, `Tests/White/WhiteProtocolVersionCompatibilityTests.cs`, superseded route-gating test deleted
- **Commit:** `dbd37c3a`

## Known Stubs

None. Stub scan matches were generated protobuf optional-member helpers and intentional entity/default empty-array initializers, not unfinished runtime behavior.

## Manual Gate

Manual RPCS3/cabinet verification and WebUI review remain the final compatibility gate. Automated tests and builds do not prove cabinet compatibility.
