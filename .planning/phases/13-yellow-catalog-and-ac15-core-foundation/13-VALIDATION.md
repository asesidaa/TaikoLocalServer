---
phase: 13
slug: yellow-catalog-and-ac15-core-foundation
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-08
---

# Phase 13 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 via `Tests/Tests.csproj` |
| **Config file** | `Tests/Tests.csproj`, `Directory.Build.props`, `Directory.Packages.props` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowCatalog|FullyQualifiedName~YellowInitialData|FullyQualifiedName~Ac15EraProfile"` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Build command** | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase13-yellow"` |
| **Estimated runtime** | Focused Yellow/AC15 tests: under 90 seconds; full suite/build depends on local machine state |

---

## Sampling Rate

- **After every task commit:** Run the focused test filter relevant to that task.
- **After every plan wave:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` and the temp-output Host build.
- **Before phase verification:** Full `dotnet test Tests/Tests.csproj` and temp-output Host build should be green unless a documented pre-existing external lock blocks them.
- **Max feedback latency:** Keep focused task checks under 90 seconds where possible.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 13-01-01 | 01 | 1 | YCAT-01 | T-13-01 | Required Yellow data paths resolve through `PathHelper` and missing required files fail with concrete Yellow paths | catalog/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowCatalogLoaderTests"` | W0 | pending |
| 13-01-02 | 01 | 1 | YCAT-02 | T-13-02 | Yellow catalog uses Yellow-owned contracts while reusing matching AC15 loaders | catalog/unit/source | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowCatalogContractTests|FullyQualifiedName~YellowInfrastructureRegistrationTests"` | W0 | pending |
| 13-02-01 | 02 | 2 | YCAT-02, YCAT-03 | T-13-03 | `Ac15EraProfiles.Yellow` and Yellow initial-data advertise only proven Yellow catalog rows | AC15/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15EraProfileTests|FullyQualifiedName~YellowInitialDataProtocolTests"` | W0 | pending |
| 13-02-02 | 02 | 2 | YCAT-03 | T-13-03 | Taikojuku/catalog readback maps through Yellow snapshot and Yellow wire without persistence | handler/mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowInitialDataProtocolTests|FullyQualifiedName~YellowTaikojukuProtocolTests"` | W0 | pending |
| 13-03-01 | 03 | 3 | YCAT-04 | T-13-04 | Yellow metadata routes are Mediator/catalog-backed only for Phase 13-owned surfaces | route/mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowMetadataRouteTests"` | W0 | pending |
| 13-03-02 | 03 | 3 | YCAT-04 | T-13-05 | Deferred Yellow runtime routes remain no-state and no Yellow EF/gameplay writes are introduced | source guard | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowCatalogBoundaryTests|FullyQualifiedName~YellowNoBattleSourceGuardTests"` | W0 | pending |

---

## Wave 0 Requirements

- [ ] `Tests/Yellow/YellowCatalogLoaderTests.cs` - required Yellow files, optional sidecars, and runtime catalog initialization.
- [ ] `Tests/Yellow/YellowCatalogContractTests.cs` - Yellow-owned catalog contract and shared AC15 loader reuse proof.
- [ ] `Tests/Yellow/YellowInfrastructureRegistrationTests.cs` - `IYellowCatalog`/`IEraGameDataCatalog` registration when Yellow is enabled.
- [ ] `Tests/Yellow/YellowInitialDataProtocolTests.cs` - Yellow profile, initial-data rows, item-shop advertisement, and no battle/fake rows.
- [ ] `Tests/Yellow/YellowTaikojukuProtocolTests.cs` - Yellow Taikojuku request/readback mapping without persistence.
- [ ] `Tests/Yellow/YellowMetadataRouteTests.cs` - Yellow folder/telop/recommend/item-shop/tournament/challenge metadata routes are catalog-backed where Phase 13 owns behavior.
- [ ] `Tests/Yellow/YellowCatalogBoundaryTests.cs` - deferred route and no-persistence source guard.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Yellow cabinet/RPCS3 runtime metadata smoke | YCAT-04 | Runtime smoke is deferred to Phase 17 by roadmap/context. | Keep Phase 13 automated only; Phase 17 records cabinet/RPCS3 evidence. |

---

## Validation Sign-Off

- [x] All planned tasks have an automated verify command or Wave 0 test dependency.
- [x] Sampling continuity: no 3 consecutive implementation tasks without automated verify.
- [x] Wave 0 covers all missing test references.
- [x] No watch-mode flags.
- [x] Feedback latency target documented.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** draft 2026-06-08
