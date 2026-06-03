---
phase: 07
slug: tokkun-evidence-contract-and-guardrail-reset
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-03
---

# Phase 07 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit via `Tests/Tests.csproj` |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA4SourceGuardTests` |
| **Blue-focused command** | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Estimated runtime** | Quick: under 30 seconds; Blue-focused/full suite depends on local build cache |

## Sampling Rate

- **After every task commit touching tests:** Run `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA4SourceGuardTests`.
- **After every plan wave:** Run `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue`.
- **Before `$gsd-verify-work`:** Run `dotnet test Tests/Tests.csproj` or explicitly record why full-suite execution was not possible.
- **Build fallback:** Use `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` if a running server locks normal build output.

## Requirement Verification Map

| Requirement | Behavior | Test Type | Automated Command | File Exists | Status |
|-------------|----------|-----------|-------------------|-------------|--------|
| TKEV-01 | `07-01-TOKKUN-EVIDENCE-CONTRACT.md` exists and defines the four status buckets: `proven`, `observed`, `deliberately ignored`, and `unknown/blocked`. | docs/source check | `Test-Path .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` plus `Select-String` checks for each status bucket | No - Wave 0 | pending |
| TKEV-02 | Contract states Blue Tokkun classification does not depend on guessed numeric `PlayMode.Tokkun`, and numeric value remains unknown until RPCS3/cabinet logs or deeper IDA prove it. | docs/source check | `Select-String .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md -Pattern 'PlayMode.Tokkun','numeric','unknown'` | No - Wave 0 | pending |
| TKEV-03 | Stale Tokkun-term source guard is removed without adding a replacement Tokkun word-scan guard. | unit/source check | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA4SourceGuardTests` | Yes | pending |

## Wave 0 Requirements

- [ ] `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` covers TKEV-01 and TKEV-02.
- [ ] `Tests/Blue/BlueA4SourceGuardTests.cs` removes the stale `BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics` Tokkun-term guard.
- [ ] No new Tokkun-specific source-scanning guard test is added in Phase 7.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Cabinet/RPCS3 Tokkun route and payload proof | TKVF-02 | Deferred to Phase 11 by roadmap scope | Do not attempt in Phase 7. Record cabinet/RPCS3 proof needs as unknown/blocked rows in the contract. |
| Numeric Tokkun `play_mode` value | TKEV-02 | Current evidence does not prove a value | Contract must state this remains unknown until RPCS3/cabinet logs or deeper IDA evidence prove it. |

## Validation Sign-Off

- [ ] All plans include automated docs/source or xUnit verification steps.
- [ ] Sampling continuity: every test-changing task runs the focused source-guard test before completion.
- [ ] Wave 0 covers missing contract artifact creation.
- [ ] No watch-mode flags.
- [ ] `nyquist_compliant: true` remains set in frontmatter.

**Approval:** pending
