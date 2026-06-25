---
phase: 39
slug: momoiro-evidence-and-era-foundation
status: draft
nyquist_compliant: true
wave_0_complete: true
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
| 39-W0-01 | 39-01 | 0 | MOFND-01 | T-39-01 | Evidence matrix documents supported and absent route families before runtime behavior is claimed. | artifact review | `rg -n "shoppingresult|bestscore|communicationlog|mainichisong|v04r00|v01r00" .planning/phases/39-momoiro-evidence-and-era-foundation/39-MOMOIRO-EVIDENCE.md` | `39-MOMOIRO-EVIDENCE.md` | green |
| 39-W0-02 | 39-01 | 0 | MOFND-02 | T-39-02 | Disabled Momoiro removes the future Momoiro adapter assembly from MVC application parts. | unit/integration-light | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroApplicationPart"` | `Tests/Momoiro/MomoiroApplicationPartTests.cs` | red until `GameEra.Momoiro` and the disabled-adapter removal branch exist |
| 39-W0-03 | 39-01 | 0 | MOFND-03 | T-39-03 | Momoiro can be enabled in `ServerSettings:Eras` without shop or Don Challenge settings while shared startup/version ownership remains recorded in evidence. | options validation | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroServerSettingsValidation"` | `Tests/Momoiro/MomoiroServerSettingsValidationTests.cs` | red until `GameEra.Momoiro` and Host settings wiring exist |
| 39-W0-04 | 39-01 | 0 | MOFND-04 | T-39-04 | Proto-only route families remain absent before route-surface implementation. | artifact review | `rg -n "shoppingresult.php|bestscore.php|communicationlog.php|mainichisong.php" .planning/phases/39-momoiro-evidence-and-era-foundation/39-MOMOIRO-EVIDENCE.md` | `39-MOMOIRO-EVIDENCE.md` | green |

*Status: pending - green - red - flaky*

---

## Wave 0 Requirements

- [x] `Tests/Momoiro/MomoiroApplicationPartTests.cs` verifies enabled/disabled application-part discovery for the future Momoiro adapter assembly without asserting source strings; current status is red until production wiring exists.
- [x] `Tests/Momoiro/MomoiroServerSettingsValidationTests.cs` verifies Momoiro can be enabled without shop or Don Challenge settings; current status is red until production wiring exists.
- [x] `Tests/Tests.csproj` remains unchanged because the Wave 0 tests use a test-owned assembly part and do not reference `Adapters.GameProtocol.Momoiro`.

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
