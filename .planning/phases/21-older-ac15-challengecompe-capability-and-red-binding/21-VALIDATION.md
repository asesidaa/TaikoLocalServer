---
phase: "21"
slug: older-ac15-challengecompe-capability-and-red-binding
status: ready
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-14
---

# Phase 21 - Validation Strategy

## Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | xUnit on `Tests/Tests.csproj` |
| Focused command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~RedPlayResultHandlerTests|FullyQualifiedName~RedProtocolMapperTests|FullyQualifiedName~Ac15UserDataService"` |
| Red regression command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red"` |
| Full suite command | `dotnet test Tests/Tests.csproj` |
| Build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` |

## Sampling Rate

- After catalog/evidence work: run focused catalog/parser tests and temp-output Host build.
- After persistence/progress work: run focused Red ChallengeCompe and Red playresult handler tests.
- After reward/userdata/readback work: run focused Red ChallengeCompe, Red protocol mapper, and `Ac15UserDataService` tests.
- Before closeout: run Red regression, full test suite, and temp-output Host build.
- If a plan records that stateful evidence is insufficient, stop stateful execution and verify the gap artifact plus unchanged empty compatibility behavior.

## Per-Task Verification Map

| Task ID | Plan | Requirement | Behavior | Test Type | Automated Command |
|---------|------|-------------|----------|-----------|-------------------|
| 21-EVIDENCE-CATALOG | 01 | RCOMP-02, RCHAL-01 | Evidence artifact separates product context from local protocol authority; Red sidecar can be disabled or parsed through era catalog paths. | doc review + catalog parser | Focused catalog test subset, build |
| 21-PERSIST-PROGRESS | 02 | RCOMP-02, RCHAL-02 | Red-owned raw fact and progress rows persist only eligible non-Tokkun challenge facts. | SQLite handler tests | Focused command |
| 21-REWARD-LOCK | 03 | RCHAL-02 | Active configured reward songs lock until earned; song/title rewards mutate Red release/title flags only. | service/handler tests | Focused command |
| 21-READBACK | 04 | RCOMP-02, RCHAL-02 | Red `challengecompe.php` returns active progress in `ary_challenge_stat`, leaves user/bng stats empty, and does not mutate opt-in. | handler/controller/wire tests | Focused command |
| 21-CLOSEOUT | 05 | RCOMP-02, RCHAL-01, RCHAL-02 | Verification and evidence record final state or explicit evidence gap. | regression + doc review | Red regression, full suite, build |

## Wave 0 Requirements

- [ ] Add no source-text, controller attribute, route inventory, generated-wire existence, enum numeric value, or migration-shape tests.
- [ ] Add tests only for observable behavior: catalog parsing, SQLite state changes/no-writes, lock/readback behavior, protocol payload classification, and no-cross-era/no-cross-mode boundaries.
- [ ] Keep product context in an evidence artifact, not in assertions that attempt to prove client compatibility.
- [ ] Preserve `Host/wwwroot/data/red/red_telop_data.json` if it is dirty before execution; do not include unrelated local data edits in Phase 21 commits.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Evidence authority split | RCOMP-02, RCHAL-01, RCHAL-02 | Public DonChare pages are product context only; local protocol authority must be explicitly recorded. | Review `21-CHALLENGECOMPE-EVIDENCE.md` and confirm every stateful behavior cites Red proto/runtime/client/local data evidence. |
| Stateful contract gap | RCHAL-02 | The roadmap requires stateful behavior to remain absent if the contract is not proven. | If execution cannot prove accepted Red request/response/state shape beyond empty success, record the gap and skip stateful mutation/readback plans. |
| Cabinet/RPCS3 acceptance | RCHAL-02 | Automated tests are regression guards, not client compatibility proof. | Phase 22 closeout should record cabinet/RPCS3 smoke evidence for implemented Red ChallengeCompe behavior or explicit absence. |

## Validation Sign-Off

- [ ] Evidence artifact exists and separates product context from protocol authority.
- [ ] Red catalog sidecar parser tests pass.
- [ ] Red SQLite persistence/no-write tests pass.
- [ ] Red reward lock/grant readback tests pass.
- [ ] Red `challengecompe.php` readback tests pass if stateful behavior is implemented.
- [ ] Red regression command passes.
- [ ] Full `Tests/Tests.csproj` passes.
- [ ] Temp-output Host build passes.
- [ ] Any evidence gap is recorded before marking the phase complete.

Approval: pending.
