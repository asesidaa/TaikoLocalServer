---
phase: 39
slug: momoiro-evidence-and-era-foundation
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-26
---

# Phase 39 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro|FullyQualifiedName~GameProtocolApplicationParts"` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Estimated runtime** | ~90 seconds focused; full suite varies by machine |

---

## Sampling Rate

- **After every task commit:** Run the focused Momoiro/application-part test filter when tests exist.
- **After every plan wave:** Run `dotnet build TaikoLocalServer.slnx`.
- **Before phase verification:** Run the focused Momoiro/application-part tests plus `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`.
- **Max feedback latency:** one task or one wave, whichever is shorter.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 39-W0-01 | TBD | 0 | MOFND-01 | T-39-01 | Evidence matrix documents supported and absent route families before runtime behavior is claimed. | artifact review | `rg -n "shoppingresult|bestscore|communicationlog|mainichisong|v04r00|v01r00" .planning/phases/39-momoiro-evidence-and-era-foundation` | no W0 | pending |
| 39-W0-02 | TBD | 0 | MOFND-02 | T-39-02 | Disabled Momoiro removes Momoiro adapter controllers from MVC application parts. | unit/integration-light | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroApplicationPart"` | no W0 | pending |
| 39-W0-03 | TBD | 0 | MOFND-03 | T-39-03 | Shared startup/version remains shared; Momoiro game prefix is `/v04r00/chassis`. | build plus route probe | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | no W0 | pending |
| 39-W0-04 | TBD | 0 | MOFND-04 | T-39-04 | Proto-only route families remain absent from Momoiro controller discovery. | application-part/controller discovery | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroRouteSurface"` | no W0 | pending |

*Status: pending - green - red - flaky*

---

## Wave 0 Requirements

- [ ] `Tests/Momoiro/MomoiroApplicationPartTests.cs` verifies enabled/disabled application-part discovery and absent proto-only controller types without asserting source strings.
- [ ] `Tests/Momoiro/MomoiroServerSettingsValidationTests.cs` verifies Momoiro can be enabled without shop or Don Challenge settings.
- [ ] `Tests/Tests.csproj` references `Adapters.GameProtocol.Momoiro` if the Momoiro tests need direct access to adapter types.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Exact MOMOIRO IDA route-string offsets | MOFND-01 | The current phase context supplies the route inventory; this validation plan does not require fresh IDA offset extraction unless the planner adds it as an evidence task. | If added, record the exact `.tools/momoiro/EBOOT.ELF.i64` route-string handles in the Phase 39 evidence matrix. |

---

## Validation Sign-Off

- [x] All tasks have automated verify targets or Wave 0 dependencies.
- [x] Sampling continuity: no 3 consecutive tasks without automated verify.
- [x] Wave 0 covers all missing test references.
- [x] No watch-mode flags.
- [x] Feedback latency bounded to one task or one wave.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** approved 2026-06-26
