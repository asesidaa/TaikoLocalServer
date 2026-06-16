---
phase: 23
slug: white-evidence-and-era-foundation
status: approved
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-17
---

# Phase 23 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~White|FullyQualifiedName~RedServerSettingsValidationTests|FullyQualifiedName~StartupAuthController"` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Estimated runtime** | ~300 seconds for focused test slices; full suite may be longer |

---

## Sampling Rate

- **After every task commit:** Run the most local compile/test slice for touched files, such as adapter project build after wire generation or settings tests after `ServerSettings` edits.
- **After every plan wave:** Run `dotnet build TaikoLocalServer.slnx` plus focused existing-era tests affected by shared changes.
- **Before `$gsd-verify-work`:** Run `dotnet test Tests/Tests.csproj`, `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`, and `git status --porcelain -- proto/white`.
- **Max feedback latency:** 300 seconds for focused checks unless the full suite is intentionally running.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 23-W0-01 | TBD | 1 | WFND-01 | N/A | Route/root/transport claims are evidence-tagged before route scaffolds are final. | artifact review | `git diff -- .planning/phases/23-white-evidence-and-era-foundation` | No - plan creates artifact | pending |
| 23-W0-02 | TBD | 1 | WFND-02 | T-23-01 | White adapter route exposure remains controlled by enabled-era gating. | behavior/build | `dotnet build TaikoLocalServer.slnx` plus White route-gating test slice | No - plan creates test | pending |
| 23-W0-03 | TBD | 1 | WFND-03 | T-23-02 | Existing supported-era route/fallback behavior is preserved after shared Host/settings edits. | regression | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~ServerSettingsValidationTests|FullyQualifiedName~StartupAuthController"` | Existing coverage partial | pending |

*Status: pending | green | red | flaky*

---

## Wave 0 Requirements

- [ ] `23-WHITE-EVIDENCE.md` - records live White IDB size, stale zero-byte correction, accepted `ST7100-1` root, route proof status, direct-protobuf transport expectation, and unresolved gaps.
- [ ] `23-WHITE-FEATURE-INVENTORY.md` or an equivalent section inside `23-WHITE-EVIDENCE.md` - separates White 0.13 proven surfaces, later White-only leads, other-era behavior, and unknowns.
- [ ] `Tests/White/WhiteServerSettingsValidationTests.cs` or equivalent behavior-facing coverage - proves White enabled settings do not require shop/challenge settings in Phase 23.
- [ ] `Tests/White/WhiteHostRouteGatingTests.cs` or equivalent behavior-facing coverage - proves White routes are absent when White is disabled and present only when enabled.
- [ ] Build-output/debug-junction check if Host project data rules are touched.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| White route suffix extraction from the current IDB or replacement request/cabinet capture | WFND-01 | Current IDA-CLI probe was blocked by locked IDA sidecars, and raw binary scans did not prove route strings. | Export route strings from the running IDA session, close/retry IDA-CLI safely, or provide request/cabinet capture evidence before finalizing concrete White route attributes. |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 300s for focused checks
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** approved - validation commands are execution-ready; Wave 0 execution remains future work.
