---
phase: 22
slug: red-adminapi-webui-and-runtime-closeout
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-15
---

# Phase 22 - Validation Strategy

Per-phase validation contract for Red AdminApi/WebUI and runtime closeout.

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit in `Tests/Tests.csproj` |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedAdminApi|FullyQualifiedName~RedDonChallenge|FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~GameDataServiceTests"` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Estimated runtime** | ~90-180 seconds for full suite, focused filters vary by touched area |

## Sampling Rate

- **After every task commit:** Run the smallest meaningful focused filter for the touched behavior.
- **After every plan wave:** Run Red regression plus affected WebUI service tests.
- **Before closeout:** Full suite and temp-output Host build must pass.
- **Max feedback latency:** Keep focused feedback under 60 seconds where practical; use full-suite latency only at wave and closeout gates.

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 22-normal-adminapi | TBD | TBD | RVER-01, RVER-02 | N/A | Red AdminApi reads/writes only Red-owned gameplay rows and shared identity where already allowed. | controller + SQLite | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedAdminApi"` | No | pending |
| 22-webui-red-routing | TBD | TBD | RVER-01, RVER-02 | N/A | WebUI keeps Red as a supported AC15 era and requests `/api/Red/...` routes without normalizing to another era. | service/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GameDataServiceTests"` | Yes | pending |
| 22-don-challenge-api | TBD | TBD | RVER-01, RVER-02 | N/A | Don Challenge API exposes active configured Red task/progress/reward status without raw uploaded facts or other era rows. | controller + SQLite | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedDonChallenge"` | No | pending |
| 22-no-opt-in | TBD | TBD | RVER-01, RVER-02 | N/A | `UserSaveDataRed.IsChallengeCompe` does not gate Don Challenge mutation, reward locks, reward grants, or UI/API presentation. | handler + SQLite | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe"` | Yes | pending |
| 22-closeout | TBD | TBD | RVER-03 | N/A | Final record separates automated proof from manual cabinet/RPCS3 compatibility proof. | command evidence + manual record | `dotnet test Tests/Tests.csproj`; `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | No | pending |

## Wave 0 Requirements

- [ ] Add focused Red AdminApi tests when implementing Red AdminApi parity.
- [ ] Add focused Don Challenge AdminApi/WebUI tests when implementing the dedicated Don Challenge surface.
- [ ] Update existing Red ChallengeCompe tests for no-opt-in behavior only where they protect observable state/readback.
- [ ] Keep existing `Tests/WebUi/GameDataServiceTests.cs` as the route-normalization guard and update it for Red.

Existing xUnit, SQLite fixture, and manual `HttpMessageHandler` infrastructure cover this phase. No framework installation is required.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Red normal cabinet/RPCS3 flow | RVER-03 | Cabinet/client compatibility cannot be proven by server tests alone. | Record user-reported smoke for implemented Red normal login/userdata/playresult/readback flow. |
| Red Tokkun tutorial flow | RVER-03 | Runtime mode selection/upload acceptance is cabinet/client behavior. | Record user-reported smoke for Red Tokkun entry and tutorial readback. |
| Red simple compatibility routes | RVER-03 | Compatibility sequence is client-driven. | Record user-reported smoke/log notes for implemented simple compatibility route sequence. |
| Red ChallengeCompe / Don Challenge flow | RVER-03 | Non-empty client acceptance and Don Challenge behavior require cabinet/RPCS3 confirmation. | Record user-reported smoke for configured ChallengeCompe progress/reward/readback or explicit remaining evidence-gated gap. |

## Validation Sign-Off

- [ ] Every implementation plan includes at least one focused automated verify command.
- [ ] No plan relies on route inventory, source text, generated wire existence, or superficial component tests as primary proof.
- [ ] No three consecutive implementation tasks proceed without focused automated verification.
- [ ] Full `dotnet test Tests/Tests.csproj` is green before closeout.
- [ ] Temp-output Host build is green before closeout.
- [ ] Manual cabinet/RPCS3 smoke evidence is recorded before Phase 22 and v1.3 are marked complete.

**Approval:** pending

