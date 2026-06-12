---
phase: 18
slug: red-evidence-and-capability-foundation
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-13
---

# Phase 18 - Validation Strategy

Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | xUnit on `Tests/Tests.csproj` |
| Config file | `Tests/Tests.csproj`; central versions in `Directory.Packages.props` |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15|FullyQualifiedName~YellowStartupAuth"` |
| Full suite command | `dotnet test Tests/Tests.csproj` |
| Build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` |
| Estimated runtime | quick filter: under 2 minutes after Red tests exist; full suite/build: several minutes |

---

## Sampling Rate

- After every task commit: run the narrow command for touched Red/AC15/shared surfaces, or at minimum `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` after scaffold changes.
- After every plan wave: run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15|FullyQualifiedName~Blue|FullyQualifiedName~Green|FullyQualifiedName~Yellow"` when shared code changes; otherwise run route-probe tests plus the temp Host build.
- Before `$gsd-verify-work`: run `dotnet test Tests/Tests.csproj`, the temp-output Host build, and user-run manual RPCS3/cabinet routing smoke.
- Max feedback latency: no more than one task commit without an automated build or focused behavior check.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 18-EVIDENCE | TBD | 1 | RFND-01 | N/A | Evidence artifact records route/version/transport/data-root findings and unresolved gaps before behavior is treated as final. | review/static artifact | Review `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` or planner-chosen equivalent; avoid source-text tests per project rules. | no | pending |
| 18-WIRE | TBD | 1 | RFND-02 | T-18-03, T-18-05 | Red wire is generated from immutable `proto/red` inputs with repo-local `protogen`; no dumped proto edits or hand-authored field drift. | build/compile | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | no | pending |
| 18-GATING | TBD | 1-2 | RFND-02, RFND-03 | T-18-01 | Red adapter routes are enabled only when Red is configured; disabled-era app-part filtering still protects other eras. | focused behavior/integration | Add only behavior tests that exercise actual enabled-era route exposure or Host configuration behavior; avoid route-inventory reflection tests unless a runtime failure justifies them. | no | pending |
| 18-SHARED-STARTUP | TBD | 2 | RFND-01, RFND-02 | T-18-01 | Shared `/v01r00` startup/version ownership remains shared while Red HDD/version mapping is explicit or recorded as a gap. | handler/controller behavior | Focused startup movie/version behavior tests after Red placeholder catalog or mapping exists; preserve existing Yellow startup tests. | partial | pending |
| 18-PRESERVE | TBD | 2 | RFND-03 | T-18-02, T-18-04 | Route probes write no gameplay, wallet, payment, coupon, transaction, or EF state; existing supported-era behavior remains unchanged. | regression/build | `dotnet test Tests/Tests.csproj`; `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | existing suites | pending |

---

## Threat References

| ID | Threat | Mitigation Required In Plans |
|----|--------|------------------------------|
| T-18-01 | Disabled Red routes accidentally exposed. | Use existing Host enabled-era DI and application-part filtering patterns for Red. |
| T-18-02 | Phase 18 probe endpoint mutates gameplay state. | Red route probes must not call gameplay Mediator handlers or EF-backed save/write paths. |
| T-18-03 | Request body parsing drift from hand parsing or stale DTOs. | Use generated protobuf-net DTOs and existing direct-protobuf fallback; do not manually parse binary bodies. |
| T-18-04 | Compatibility logs mistaken for Banacoin/payment authority. | Banacoin-adjacent surfaces, if any, remain no-state compatibility/logging only unless later evidence proves authority. |
| T-18-05 | Dumped proto evidence is damaged during generation. | Treat `proto/red` as immutable input; generate to temp first, then replace generated Red `Wire/` outputs only. |

---

## Wave 0 Requirements

- [ ] `Tests/Red/` or equivalent focused test location for Red scaffold behavior, only where it protects observable route/gating/build behavior.
- [ ] Red evidence artifact, likely `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md`.
- [ ] Red wire and adapter compile path from `proto/red` through `Adapters.GameProtocol.Red/Wire/`.
- [ ] Host Red settings, DI, application-part gating, and temp-output build validation.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Red `/v08r01/chassis/*` route request-routing smoke | RFND-01, RFND-02 | Automated tests cannot prove the actual cabinet/RPCS3 runtime sequence or whether minimal probe responses keep the client moving. | User runs Red cabinet/RPCS3 against the server, captures route/log evidence, and records remaining response-shape gaps before Phase 18 closes. |

---

## Validation Sign-Off

- [ ] All plans include concrete automated verification or explicit manual-only justification.
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify.
- [ ] Wave 0 covers missing Red test, evidence, wire, and Host gating surfaces.
- [ ] No watch-mode flags.
- [ ] Full phase gate includes `dotnet test Tests/Tests.csproj`, temp-output Host build, and user-run RPCS3/cabinet routing smoke.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** pending
