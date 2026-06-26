---
phase: 42
slug: momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil
status: planned
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-26
---

# Phase 42 - Validation Strategy

> Per-phase validation contract for MOMOIRO normal playresult mutation, unlocks, rewards, and bounded Dan compatibility.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResult|FullyQualifiedName~MomoiroReadback|FullyQualifiedName~Ac15NormalPlayWriter|FullyQualifiedName~Ac15Dani" --no-restore -- RunConfiguration.DisableParallelization=true` |
| **Full suite command** | `dotnet test Tests/Tests.csproj --no-restore -- RunConfiguration.DisableParallelization=true` |
| **Mapper verification command** | `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true --no-restore`, then inspect `obj/.../generated/.../Riok.Mapperly/*.g.cs` |

---

## Sampling Rate

- **After every task commit:** Run the focused Momoiro playresult/readback, AC15 normal-play writer, or AC15 Dani slice relevant to the changed code.
- **After each plan wave:** Run the plan-specific tests plus the project or adapter build listed in that plan.
- **Before phase verification:** Run focused Momoiro playresult/controller/readback tests, full serialized suite, solution build, temp-output Host build, Mapperly generated-source inspection, proto cleanliness, unsupported route/state gates, path-abstraction gate, and `Host/.gitignore` unchanged/staged-clean checks.
- **Max feedback latency:** one task or one wave, whichever is shorter.

---

## Dirty-File Baseline

`Host/.gitignore` was already modified before Phase 42 execution. Phase 42 executors must preserve that pre-existing diff exactly and must not stage it.

| File | Baseline Command | Baseline SHA-256 | Required Phase 42 Behavior |
|------|------------------|------------------|----------------------------|
| `Host/.gitignore` | `git diff -- Host/.gitignore` | `73190e8a4bb993eadc0a9364cac7e1b0336f45192911fb68d9ab1ebb87cac782` | Diff hash unchanged and no staged entry for `Host/.gitignore`. |

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------------|-----------|-------------------|-------------|--------|
| 42-W0-01 | 42-01 | 0 | MORUN-01 | Normal playresult writes only Momoiro play-history, best/crown source, counters, recents, favorites with append-only `DisplayOrder`, and no adjacent-era rows. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~MomoiroPlayResultController" --no-restore -- RunConfiguration.DisableParallelization=true` | Planned: `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs`; `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` | PENDING - Wave 0 adds RED contracts before production behavior. |
| 42-W0-02 | 42-01 | 0 | MORUN-02 | Release-song IDs, Don Point, and reward fields update only Momoiro save/readback state; no shop, wallet, payment, or spend authority is created. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~MomoiroReadback" --no-restore -- RunConfiguration.DisableParallelization=true` | Planned: `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs`; existing readback tests | PENDING - Wave 0 adds RED contracts before production behavior. |
| 42-W0-03 | 42-01/42-04 | 0/2 | MORUN-03 | Dan persistence is Momoiro-owned and bounded to playresult/BAID/userdata compatibility; no Taikojuku route, practice folder, or broader feature family is exposed. | handler/unit/profile | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~Ac15DaniCapabilityTests" --no-restore -- RunConfiguration.DisableParallelization=true` | Planned handler tests; existing `Tests/Ac15/Ac15DaniCapabilityTests.cs` | PENDING - Wave 0 adds RED contracts; Wave 2 adds bounded implementation checks. |
| 42-W0-04 | 42-01 | 0 | MORUN-04 | Challenge-shaped arrays are accepted/mapped for diagnostics and dropped; no Don Challenge, ChallengeCompe, reward-management, or raw future-proof challenge table is written. | negative persistence/integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~MomoiroPlayResultController" --no-restore -- RunConfiguration.DisableParallelization=true` | Planned: `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs`; `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` | PENDING - Wave 0 adds RED contracts before production behavior. |
| 42-W0-05 | 42-01/42-07 | 0/final | MORUN-05 | Runtime writes are Momoiro-owned only; unsupported Momoiro route families, proto edits, unsupported feature state, hardcoded data paths, and `Host/.gitignore` edits are absent. | integration/source gate | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~MomoiroRouteSurface" --no-restore -- RunConfiguration.DisableParallelization=true`; source gates in 42-07 | Planned handler tests; existing `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` | PENDING - Wave 0 adds RED contracts and final gate records green evidence. |

*Status: pending - red - green - flaky*

---

## Wave 0 Requirements

- [ ] `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs` covers MORUN-01 through MORUN-05 through handler/database/readback behavior.
- [ ] `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` covers direct protobuf controller mapping without source-text route assertions.
- [ ] `Tests/Momoiro/MomoiroHandlerFixture.cs` supports play-history and Dan table helpers without relying on future production symbols before schema lands.
- [ ] Momoiro Dan tests prove bounded Dan compatibility and absence of Taikojuku route/practice-folder behavior.
- [ ] Challenge-array tests prove accepted route payloads do not create Don Challenge, ChallengeCompe, reward-management, or adjacent-era state.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Exact cabinet-visible interpretation of Momoiro challenge arrays | MORUN-04 | Binary/proto evidence proves field presence, not stateful server semantics. Phase 42 intentionally logs/drops without persistence. | If request logs or cabinet evidence prove readback semantics, add a later phase before changing this behavior. |
| Exact native favorite insertion order | MORUN-01 | Phase 41 established persisted `DisplayOrder`; Phase 42 appends after the max as the conservative server contract. | If native evidence proves a different order, update favorite mutation and readback tests together. |
| Cabinet/RPCS3 acceptance of playresult mutation | MORUN-01..05 | Phase 42 is server-side automated verification; cabinet acceptance remains Phase 44. | Do not claim cabinet acceptance in Phase 42 summaries unless the user provides runtime evidence. |

---

## Validation Sign-Off

- [x] All Phase 42 requirements have automated verify targets or explicit manual-only rationale.
- [x] Sampling continuity: no 3 consecutive tasks without automated verify.
- [x] Wave 0 covers missing tests before production implementation.
- [x] No watch-mode flags.
- [x] Feedback latency bounded to one task or one wave.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** planned for executor consumption 2026-06-26.
