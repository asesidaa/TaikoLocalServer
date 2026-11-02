---
phase: 40
slug: momoiro-protocol-limits-root-catalog-and-route-behavior
status: complete
nyquist_compliant: true
wave_0_complete: true
created: 2026-06-26
---

# Phase 40 - Validation Strategy

> Per-phase validation contract for MOMOIRO catalog/profile/metadata-route execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro|FullyQualifiedName~Ac15SongHashCodec|FullyQualifiedName~Ac15CatalogReadbackService" --no-restore` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Estimated runtime** | focused under 2 minutes; full suite varies by machine |

---

## Sampling Rate

- **After every task commit:** Run the focused Momoiro catalog/profile/metadata test filter once relevant tests exist.
- **After each plan wave:** Run the plan-specific tests plus `dotnet build TaikoLocalServer.slnx`.
- **Before phase verification:** Run focused Momoiro tests, full solution build, temp-output Host build, explicit Momoiro sidecar output check, route-absence grep, and `git status --porcelain -- proto/momoiro`.
- **Max feedback latency:** one task or one wave, whichever is shorter.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 40-W0-01 | 40-01 | 0 | MOCAT-01 | T-40-01 | Momoiro root catalog loader requires root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`; parsed music count is 380 and song hash version is 538116869. | catalog parser/integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroCatalogLoaderTests"` | `Tests/Momoiro/MomoiroCatalogLoaderTests.cs` | green - passed `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroCatalogLoaderTests" --no-restore` (2/2); also covered by focused aggregate with disabled VSTest parallelization (27/27) |
| 40-W0-02 | 40-01 | 0 | MOCAT-03, MOCAT-05 | T-40-02 | Momoiro profile records explicit limits, `CrownPlacement = UserData`, disabled unsupported features, conservative inferred caps, and no dedicated crown route authority. | profile/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroProtocolLimitsTests"` | `Tests/Momoiro/MomoiroProtocolLimitsTests.cs` | green - passed focused aggregate `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro|FullyQualifiedName~Ac15SongHashCodec|FullyQualifiedName~Ac15CatalogReadbackService" --no-restore -- RunConfiguration.DisableParallelization=true` (27/27) and route/profile/shared slice (16/16) |
| 40-W0-03 | 40-01 | 0 | MOCAT-04 | T-40-03 | Catalog-backed metadata routes return observable recommendation/default-song/songhash/telop responses and static heartbeat/bookkeeping stubs remain non-stateful. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests"` | `Tests/Momoiro/MomoiroMetadataRouteTests.cs` | green - passed `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests" --no-restore` (6/6); also covered by focused aggregate with disabled VSTest parallelization (27/27) |
| 40-W0-04 | 40-01 | 0 | MOCAT-02 | T-40-04 | Runtime code consumes catalog/path abstractions such as `IGameDataCatalog.For(GameEra.Momoiro)` and path helpers instead of handler-local hardcoded filesystem access. | grep plus behavior tests | `$hits = rg -n "Host/wwwroot/data/momoiro|wwwroot\\\\data\\\\momoiro|File\\.|Directory\\.|Path\\.Combine" Adapters.GameProtocol.Momoiro Application/Handlers Application/Ac15; if ($LASTEXITCODE -eq 0) { $hits; exit 1 }; if ($LASTEXITCODE -eq 1) { exit 0 }; exit $LASTEXITCODE` | source gates plus `Tests/Momoiro/MomoiroCatalogLoaderTests.cs` and `Tests/Momoiro/MomoiroMetadataRouteTests.cs` | green - source gate passed with no hits; focused catalog and metadata route slices passed (2/2 and 6/6) |
| 40-W0-05 | 40-01 | 0 | MOCAT-04, MOCAT-05 | T-40-05 | Proto-only and deferred feature routes remain absent, including `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, `mainichisong.php`, and standalone `crownsdata.php`. | route discovery/grep | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroRouteSurface"` | `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` | green - route/profile/shared slice passed (16/16); unsupported-route grep including `crownsdata.php` passed with no hits |
| 40-FINAL-01 | 40-05 | 4 | MOCAT-02, MOCAT-04 | T-40-18 | Temp-output Host build carries the committed Momoiro telop sidecar while raw operator data remains excluded/junctioned. | build artifact check | `Test-Path "$env:TEMP\TaikoLocalServer-host-build\wwwroot\data\momoiro\momoiro_telop_data.json"` | `Host/Host.csproj`, `Host/wwwroot/data/momoiro/momoiro_telop_data.json` | green - temp-output Host build passed 0 warnings/0 errors; `Test-Path` returned `True` |

*Status: pending - green - red - flaky*

---

## Wave 0 Requirements

- [x] `Tests/Momoiro/MomoiroCatalogLoaderTests.cs` protects root-level catalog loading, required-file validation, song count 380, song hash version 538116869, and 760-byte encoded song-hash table behavior. Current status: green; passed 2/2 in the final 40-05 slice.
- [x] `Tests/Momoiro/MomoiroProtocolLimitsTests.cs` protects explicit Momoiro profile limits, userdata-owned crown placement, unsupported feature absences, conservative favorite/recent caps, and inferred crown/default/release wire lengths as documented assumptions. Current status: green; included in the final 40-05 focused aggregate.
- [x] `Tests/Momoiro/MomoiroMetadataRouteTests.cs` protects catalog-backed `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, `gettelop.php` behavior plus static `heartbeat.php` and `bookkeeping.php` success. Current status: green; passed 6/6 in the final 40-05 slice.
- [x] Existing `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` remains the absence guard for unsupported routes and shared startup/version ownership, with 40-01 adding explicit `crownsdata.php` absence.

---

## Final Phase 40 Automated Verification

| Gate | Command | Result |
|------|---------|--------|
| Focused Momoiro/AC15 aggregate, plan command as written | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro\|FullyQualifiedName~Ac15SongHashCodec\|FullyQualifiedName~Ac15CatalogReadbackService" --no-restore` | FAILED due test-output race between Momoiro tests copying/deleting `Tests/bin/Debug/net10.0/wwwroot/data/momoiro/data`; first run 24/27 passed, second run 25/27 passed. No behavior assertions failed. |
| Focused Momoiro/AC15 aggregate, serialized runner | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro\|FullyQualifiedName~Ac15SongHashCodec\|FullyQualifiedName~Ac15CatalogReadbackService" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS - 27/27. |
| Focused catalog slice | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroCatalogLoaderTests" --no-restore` | PASS - 2/2. |
| Focused metadata route slice | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests" --no-restore` | PASS - 6/6. |
| Focused route/profile/shared AC15 slice | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroProtocolLimitsTests\|FullyQualifiedName~MomoiroRouteSurfaceTests\|FullyQualifiedName~Ac15SongHashCodec\|FullyQualifiedName~Ac15CatalogReadbackService" --no-restore` | PASS - 16/16. |
| Momoiro adapter build with generated-source emission | `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | PASS - 0 warnings, 0 errors. |
| Full test suite, plan command as written | `dotnet test Tests/Tests.csproj` | FAILED due the same Momoiro shared-output race; 906/908 passed. No behavior assertions failed. |
| Full test suite, serialized runner | `dotnet test Tests/Tests.csproj -- RunConfiguration.DisableParallelization=true` | PASS - 908/908. |
| Full solution build | `dotnet build TaikoLocalServer.slnx` | PASS - 0 warnings, 0 errors. |
| Temp-output Host build | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | PASS - 0 warnings, 0 errors. |
| Momoiro telop sidecar output | `Test-Path "$env:TEMP\TaikoLocalServer-host-build\wwwroot\data\momoiro\momoiro_telop_data.json"` | PASS - `True`. |
| Proto cleanliness | `git status --porcelain -- proto\momoiro` | PASS - no output. |
| Unsupported route absence | `$hits = rg -n "shoppingresult\.php\|bestscore\.php\|communicationlog\.php\|mainichisong\.php\|crownsdata\.php" Adapters.GameProtocol.Momoiro/Controllers; if ($LASTEXITCODE -eq 0) { $hits; exit 1 }; if ($LASTEXITCODE -eq 1) { exit 0 }; exit $LASTEXITCODE` | PASS - no hits. |
| Handler/controller path abstraction | `$hits = rg -n "Host/wwwroot/data/momoiro\|wwwroot\\data\\momoiro\|File\.\|Directory\.\|Path\.Combine" Adapters.GameProtocol.Momoiro Application/Handlers Application/Ac15; if ($LASTEXITCODE -eq 0) { $hits; exit 1 }; if ($LASTEXITCODE -eq 1) { exit 0 }; exit $LASTEXITCODE` | PASS - no hits. |

### Mapperly Generated Source Inspection

- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/RecommendMappers.g.cs`
  - `Map(CommonRecommendResponse)` assigns `Result`, `RecommendSong`, and `RecommendBestSongs` to the Momoiro wire response.
- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/GetTelopMappers.g.cs`
  - `Map(CommonGetTelopResponse)` assigns `Result`, `StartDatetime`, `EndDatetime`, and `Telop` to the Momoiro wire response.
  - `VerupNo` is not assigned to the Momoiro response, matching the explicit source ignore.

### Scope Guard

- No cabinet/RPCS3 acceptance is claimed for Phase 40. Runtime acceptance remains Phase 44 scope.
- MOMOIRO identity, userdata, self-best, playresult mutation, AdminApi, and WebUI behavior remain later-phase work.
- No production behavior, generated wire, or `proto/momoiro` inputs were changed during 40-05.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Native favorite max, exact native crown byte constant, and exact Don Point/reward cap | MOCAT-03 | IDA research did not tie these numeric constants to instruction/data flow. Mutation and full userdata readback are deferred, so this does not block Phase 40. | If new binary/client evidence is found, record it in a follow-up evidence artifact and adjust `Ac15EraProfiles.Momoiro` before Phase 41/42 behavior depends on it. |
| Cabinet/RPCS3 metadata-route acceptance | MOCAT-04 | Phase 40 is server-side implementation and regression coverage; milestone cabinet acceptance is Phase 44. | Do not claim cabinet acceptance in Phase 40 summaries unless the user explicitly provides runtime evidence. |

---

## Validation Sign-Off

- [x] All Phase 40 requirements have automated verify targets or explicit manual-only rationale.
- [x] Sampling continuity: no 3 consecutive tasks without automated verify.
- [x] Wave 0 covers missing tests before production implementation.
- [x] No watch-mode flags.
- [x] Feedback latency bounded to one task or one wave.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** complete for Phase 40 server-side closeout 2026-06-26. Cabinet/RPCS3 acceptance is not claimed and remains Phase 44 scope.
