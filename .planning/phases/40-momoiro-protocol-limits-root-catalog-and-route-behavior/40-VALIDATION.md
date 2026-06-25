---
phase: 40
slug: momoiro-protocol-limits-root-catalog-and-route-behavior
status: draft
nyquist_compliant: true
wave_0_complete: false
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
- **Before phase verification:** Run focused Momoiro tests, full solution build, temp-output Host build, route-absence grep, and `git status --porcelain -- proto/momoiro`.
- **Max feedback latency:** one task or one wave, whichever is shorter.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 40-W0-01 | TBD | 0 | MOCAT-01 | T-40-01 | Momoiro root catalog loader requires root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`; parsed music count is 380 and song hash version is 538116869. | catalog parser/integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroCatalogLoaderTests"` | `Tests/Momoiro/MomoiroCatalogLoaderTests.cs` | pending |
| 40-W0-02 | TBD | 0 | MOCAT-03, MOCAT-05 | T-40-02 | Momoiro profile records explicit limits, `CrownPlacement = UserData`, disabled unsupported features, conservative inferred caps, and no dedicated crown route authority. | profile/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroProtocolLimitsTests"` | `Tests/Momoiro/MomoiroProtocolLimitsTests.cs` | pending |
| 40-W0-03 | TBD | 0 | MOCAT-04 | T-40-03 | Catalog-backed metadata routes return observable recommendation/default-song/songhash/telop responses and static heartbeat/bookkeeping stubs remain non-stateful. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests"` | `Tests/Momoiro/MomoiroMetadataRouteTests.cs` | pending |
| 40-W0-04 | TBD | 0 | MOCAT-02 | T-40-04 | Runtime code consumes `IMomoiroCatalog`/`IGameDataCatalog.For(GameEra.Momoiro)` and path helpers instead of handler-local hardcoded filesystem access. | grep plus behavior tests | `$hits = rg -n "Host/wwwroot/data/momoiro|wwwroot\\\\data\\\\momoiro|File\\.|Directory\\.|Path\\.Combine" Adapters.GameProtocol.Momoiro Application/Handlers Application/Ac15; if ($LASTEXITCODE -eq 0) { $hits; exit 1 }; if ($LASTEXITCODE -eq 1) { exit 0 }; exit $LASTEXITCODE` | source gates | pending |
| 40-W0-05 | TBD | 0 | MOCAT-04, MOCAT-05 | T-40-05 | Proto-only and deferred feature routes remain absent, including `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, `mainichisong.php`, and standalone `crownsdata.php`. | route discovery/grep | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroRouteSurface"` | `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` | green |

*Status: pending - green - red - flaky*

---

## Wave 0 Requirements

- [ ] `Tests/Momoiro/MomoiroCatalogLoaderTests.cs` protects root-level catalog loading, required-file validation, song count 380, song hash version 538116869, and 760-byte encoded song-hash table behavior.
- [ ] `Tests/Momoiro/MomoiroProtocolLimitsTests.cs` protects explicit Momoiro profile limits, userdata-owned crown placement, unsupported feature absences, conservative favorite/recent caps, and inferred crown/default/release wire lengths as documented assumptions.
- [ ] `Tests/Momoiro/MomoiroMetadataRouteTests.cs` protects catalog-backed `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, `gettelop.php` behavior plus static `heartbeat.php` and `bookkeeping.php` success.
- [ ] Existing `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` remains the absence guard for unsupported routes and shared startup/version ownership.

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

**Approval:** draft for planner consumption 2026-06-26
