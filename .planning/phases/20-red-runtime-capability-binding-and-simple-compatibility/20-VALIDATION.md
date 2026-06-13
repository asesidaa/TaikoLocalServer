---
phase: 20
slug: red-runtime-capability-binding-and-simple-compatibility
status: passed
nyquist_compliant: true
created: 2026-06-13
---

# Phase 20 - Validation Strategy

## Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | xUnit on `Tests/Tests.csproj` |
| Focused command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red"` |
| Shared regression command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15|FullyQualifiedName~Yellow|FullyQualifiedName~Blue|FullyQualifiedName~Green"` |
| Full command | `dotnet test Tests/Tests.csproj` |
| Build command | `dotnet build TaikoLocalServer.slnx` |
| Host build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` |

## Per-Task Verification Map

| Task ID | Requirement | Behavior | Test Type | Automated Command |
|---------|-------------|----------|-----------|-------------------|
| 20-STATE | RSTATE-01, RSTATE-02, RSTATE-03 | Red identity, userdata, normal play, self-best, crowns, favorites, recent songs, counters, and Don points persist to Red-owned tables. | handler/SQLite behavior | Focused Red handler tests |
| 20-DANI | RSTATE-03 | Red Dan playresults persist Dan score/stage rows and update Red save-data Dan flags/display-Dan through shared Dani behavior. | handler/SQLite behavior | Focused Red Dani tests |
| 20-TOKKUN | RSTATE-04 | Red Tokkun is classified before normal/Dani/Challenge handling and updates only `TokkunTutorialFlg`. | no-cross-mode persistence | Focused Red Tokkun tests |
| 20-WIRE | RSTATE-01, RSTATE-02, RSTATE-05 | Red BAID, mydon, userdata, self-best, crowns, and playresult controllers map Red wire through common DTOs and Mediator. | mapper/controller behavior | Focused Red protocol tests |
| 20-COMPAT | RCOMP-01 | Reward-card, reward-execution, balance-check, Banacoin payment, and Banacoin error routes remain compatibility-only and do not create wallet/shop/unlock state. | guardrail review | Guardrail scan plus focused Red runtime tests |
| 20-PRESERVE | all | Red writes do not land in Green/Blue/Yellow/Nijiiro gameplay tables and shared AC15 behavior still passes. | regression/build | Focused shared AC15 and full test commands |

## Manual-Only Verifications

None required for Phase 20 close. Cabinet/RPCS3 acceptance remains deferred to Phase 22 after Phase 21 ChallengeCompe scope is decided.

## Sign-Off

- [x] Red runtime state is Red-owned and SQLite-backed.
- [x] Shared AC15 mechanisms are reused for normal play, self-best, crowns, userdata, and Dani.
- [x] Red Don points are not modeled as shop medals or wallet balance.
- [x] Red Tokkun writes only tutorial state.
- [x] Red ChallengeCompe state remains deferred to Phase 21.
- [x] Temp-output Host build and full tests pass.
