---
phase: 12
slug: yellow-evidence-and-era-foundation
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-07
---

# Phase 12 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 via `Tests/Tests.csproj` |
| **Config file** | `Tests/Tests.csproj`, `Directory.Build.props`, `Directory.Packages.props` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Build command** | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` |
| **Estimated runtime** | Focused Yellow tests: under 60 seconds; full suite/build depends on local machine state |

---

## Sampling Rate

- **After every task commit:** Run the focused Yellow test filter relevant to that task.
- **After every plan wave:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` and the temp-output Host build.
- **Before phase verification:** Full `dotnet test Tests/Tests.csproj` and temp-output Host build should be green unless a documented pre-existing external lock blocks them.
- **Max feedback latency:** Keep focused task checks under 60 seconds where possible.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 12-01-01 | 01 | 1 | YFND-01 | T-12-01 | Evidence record documents route/version/transport proof and gaps instead of inventing behavior | docs/source | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowEvidence"` | W0 | pending |
| 12-01-02 | 01 | 1 | YFND-02 | T-12-02 | Generated Yellow DTOs compile in a Yellow-owned adapter namespace | wire/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowWireGenerationTests"` | W0 | pending |
| 12-02-01 | 02 | 2 | YFND-02, YFND-03 | T-12-03 | Yellow routes are adapter-owned and enabled only through Yellow configuration | route/source | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowRouteSkeletonTests|FullyQualifiedName~YellowHostProgramSourceTests"` | W0 | pending |
| 12-02-02 | 02 | 2 | YFND-03 | T-12-03 | Host registration and application-part filtering include Yellow without exposing disabled Yellow routes | source/build | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | existing | pending |
| 12-03-01 | 03 | 3 | YFND-04 | T-12-04 | Yellow exposes no Blue battle route, fields, persistence, or fallback | proto/source/route | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowNoBattleSourceGuardTests"` | W0 | pending |
| 12-03-02 | 03 | 3 | YFND-01, YFND-04 | T-12-01 / T-12-04 | Shared startup/version ownership is proven separately from Yellow battle absence | route/wire | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowSharedVersionRouteTests|FullyQualifiedName~YellowNoBattleSourceGuardTests"` | W0 | pending |

---

## Wave 0 Requirements

- [ ] `Tests/Yellow/YellowEvidenceTests.cs` - evidence artifact coverage for YFND-01.
- [ ] `Tests/Yellow/YellowWireGenerationTests.cs` - generated Yellow wire namespace and no-battle DTO coverage for YFND-02/YFND-04.
- [ ] `Tests/Yellow/YellowRouteSkeletonTests.cs` - Yellow supported/unsupported route reflection coverage for YFND-02/YFND-04.
- [ ] `Tests/Yellow/YellowHostProgramSourceTests.cs` - Host Yellow registration, disabled application-part filtering, and content-type fallback scope coverage for YFND-03.
- [ ] `Tests/Yellow/YellowSharedVersionRouteTests.cs` - shared `/v01r00/chassis/*` startup/version ownership coverage for YFND-01.
- [ ] `Tests/Yellow/YellowNoBattleSourceGuardTests.cs` - proto/source/route/persistence absence coverage for YFND-04.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Yellow cabinet/RPCS3 runtime route prefix and HTTP framing | YFND-01 | No local Yellow runtime log or client capture is currently available in the repo. Runtime proof is scheduled for Phase 17. | Record as an evidence gap in Phase 12, then verify with cabinet/RPCS3 logs in Phase 17. |

---

## Validation Sign-Off

- [x] All planned tasks have an automated verify command or Wave 0 test dependency.
- [x] Sampling continuity: no 3 consecutive implementation tasks without automated verify.
- [x] Wave 0 covers all missing test references.
- [x] No watch-mode flags.
- [x] Feedback latency target documented.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** draft 2026-06-07
