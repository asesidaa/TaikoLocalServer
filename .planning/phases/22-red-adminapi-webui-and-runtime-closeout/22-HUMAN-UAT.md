---
status: partial
phase: 22-red-adminapi-webui-and-runtime-closeout
source:
  - 22-VERIFICATION.md
started: 2026-06-15T23:23:14+08:00
updated: 2026-06-15T23:23:14+08:00
---

# Phase 22 Human UAT

## Current Test

Awaiting cabinet/RPCS3 smoke evidence for Phase 22 runtime closeout.

## Tests

### 1. Red normal runtime smoke

expected: Red cabinet/RPCS3 can complete profile/login/userdata/playresult/readback against the implemented Red normal runtime state.
result: pending

### 2. Red Tokkun tutorial smoke

expected: Red cabinet/RPCS3 can upload Tokkun tutorial state and read back the tutorial flag through userdata without normal/Dani/Challenge cross-writes.
result: pending

### 3. Red simple compatibility route smoke

expected: The simple compatibility routes expected by Red respond successfully in the real client sequence.
result: pending

### 4. Don Challenge progress and AdminApi/WebUI smoke

expected: A real Red run can persist Don Challenge progress/rewards, and the AdminApi/WebUI read model shows the resulting configured task and reward state.
result: pending

### 5. Cabinet ChallengeCompe boundary smoke

expected: Cabinet `challengecompe.php` behavior is confirmed separately from the AdminApi/WebUI Don Challenge read model, with no unsupported user/BNG/global semantics claimed.
result: pending

## Summary

total: 5
passed: 0
issues: 0
pending: 5
skipped: 0
blocked: 0

## Gaps

None reported yet. Manual smoke is pending.
