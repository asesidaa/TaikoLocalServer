---
status: complete
phase: 05-blue-battle-runtime-support
source:
  - .planning/phases/05-blue-battle-runtime-support/05-02-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-07-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-08-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-11-SUMMARY.md
  - .tools/crash1.txt
  - .tools/RPCS3.log
  - .tools/blue/EBOOT.ELF.i64
started: 2026-05-31T17:36:33+08:00
updated: 2026-06-03T00:00:00+08:00
---

## Current Test

[testing complete]

## Tests

### 1. First-Time Blue Battle User Request
expected: A new Blue user can enter the first battle flow; `battleuserdata.php` returns client-safe first-use battle state, and RPCS3 does not freeze or crash after the battle user request.
result: pass
previous_issue: "Now the game crashes on first time battle user request. The crash log from RPCS3 is in .tools/crash1.txt."
latest_issue: "RPCS3 retest still crashes after the explicit first-stage assignment fix. Latest crash log is .tools/RPCS3.log."
latest_diagnosis: "2026-06-01 reanalysis used the known-working screenshot as the first-use source of truth and treated prior server assumptions as suspect. Fresh IDA daemon/subagent evidence shows the byte-array lengths were not the direct issue: `OnBattleUserDataResponse` consumes only 16/8/4/16-byte slices and optional zero scalars behave like omitted defaults. The later played session proves persistence/readback is required after battle and proves the first runtime NPC id is 0 even though `battlenpcinfo.xml` stores `<id>1</id>`. The special-attack note is also confirmed by IDA: selected specials are zeroed unless their bit survives the nested special mask AND initialdata special mask."
fix: "Superseding server fix implemented: `battleuserdata.php` now returns an IDA-backed safe starter only when no persisted state exists, and after playresult reads back persisted Blue battle state: runtime NPC id 0, total exp/max DPN, selected specials, release info/stage assignment, tokens, and selected-special masks with raw bit 120."
latest_fix: "Initialdata `release_battle_special_flg` also uses the shared battle special bitset builder, so special bit 1 and bit 120 are present when battle is advertised. This keeps `LastSelectSpecial1=1` backed by an unlocked special and preserves the row-retention bit."
severity: blocker

## Summary

total: 1
passed: 1
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

- truth: "A new Blue user can enter the first battle flow; battleuserdata.php returns client-safe first-use battle state, and RPCS3 does not freeze or crash after the battle user request."
  status: resolved
  reason: "Resolved externally before v1.0 milestone close; user confirmed all stale debug and UAT artifacts were fixed and resolved on 2026-06-03."
  severity: blocker
  test: 1
  root_cause: "Fresh 2026-06-01 evidence shows the previous catalog-derived/persisted starter contract was not proven safe. `sub_7497C` copies fixed byte slices only and validates selected specials against the effective 16-byte special mask after ANDing nested `release_special_flg` with initialdata `release_battle_special_flg`. If the selected special is missing from either mask, the client clears it; all-zero specials are therefore menu-safe at best and can crash when a special attack is used. Nonzero catalog NPC/stage/special values are also unproven because the known-working response uses default NPC/stage state."
  artifacts:
    - path: ".tools/crash1.txt"
      issue: "RPCS3 access violation in NU::Draw::RequestManager at 0x005997b0 reading 0xddddde09."
    - path: ".tools/RPCS3.log"
      issue: "Latest crash log has no nearby protobuf parse, HTTP status, or missing battle asset error; after `battle/battlesupportinfo.xml` opens and the final HTTP worker closes, the client faults in `PPU[0x100000a] Thread (NU::Draw::RequestManager)` at `0x005997b0`, reading `0xddddde09`."
    - path: ".tools/blue/EBOOT.ELF.i64"
      issue: "IDA proves `sub_5997B0` is the NUD skin draw request; `sub_7497C` copies battleuserdata release info, stage, NPC costume, and NPC special bytes; `sub_799224` consumes a 20-bit LSB-first NPC costume availability mask from the retained NPC row path used by battle support/model setup."
    - path: ".planning/debug/phase-05-blue-battle-crash.md"
      issue: "Crash-trace agent found the prior bit-120 mechanism remains valid but is not confirmed as the current served response bug; remaining suspects need downstream IDA proof before server behavior changes."
    - path: ".planning/debug/blue-battle-byte-arrays.md"
      issue: "Byte-array downstream agent mapped each battle byte array to server writer, proto tag, width, parser, and consumer; highest proven gate remains initialdata and nested NPC special bit 120, while NPC costume flag semantics are not mapped."
    - path: "Application/Handlers/GetBattleUserDataQuery.Blue.cs"
      issue: "Battleuserdata now emits the safe starter contract only for no-state users, then reads back persisted BlueBattle user/NPC/token rows after playresult while preserving selected special bits and raw bit 120."
    - path: "Tests/Blue/BlueBattleUserDataTests.cs"
      issue: "Regression coverage asserts first-use starter response and persisted runtime NPC id 0 readback with release info bit 1, last/assign stage 1, selected specials 1/1/1, and token fallback."
    - path: "Host/wwwroot/data/blue/data/config/S10100-1/battle/battlenpcinfo.xml"
      issue: "Local parsed battle NPC id is 1 with start_exp 0 and first atk 30."
    - path: "Host/wwwroot/data/blue/data/config/S10100-1/battle/battlestageinfo.xml"
      issue: "Local parsed first battle stage is 1; this matches the IDA-observed omitted/zero assignment fallback."
    - path: "Host/wwwroot/data/blue/data/config/S10100-1/battle/battletokeninfo.xml"
      issue: "Local parsed special reward ids provide the release-special bitset and first valid default selected specials."
  missing:
    - "Rerun the same first-time RPCS3 battle flow and inspect whether the crash advances past the `battlesupportinfo.xml`/draw-thread failure point."
    - "If RPCS3 still crashes, confirm the actual served first-use NPC row contains zero costume flags and selected special 1, then confirm the next `battleuserdata.php` after playresult echoes persisted NPC id 0 and progress instead of resetting to the starter."
  fix:
    summary: "First-time battleuserdata emits the IDA-safe starter response, while later battleuserdata echoes persisted client-reported BlueBattle state. Initialdata keeps special bit 1 and bit 120 when battle is advertised, and `battlenpcinfo.xml` NPC ids are normalized from one-based XML ids to zero-based runtime ids."
    tests:
      - "dotnet test Tests/Tests.csproj --filter \"FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueInitialDataTests\" --no-restore"
      - "dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore"
      - "dotnet test Tests/Tests.csproj --no-restore"
      - "dotnet build Host/Host.csproj -o \"$env:TEMP\\TaikoLocalServer-host-build-blue-battle-special-gate-fix\" --no-restore"
      - "git diff --check"
      - "dotnet test Tests/Tests.csproj --filter \"FullyQualifiedName~BlueBattleUserDataTests\" --no-restore"
      - "dotnet test Tests/Tests.csproj --filter \"FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueBattleCatalogLoaderTests\" --no-restore"
      - "dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore"
      - "dotnet test Tests/Tests.csproj --no-restore"
      - "dotnet build Host/Host.csproj -o \"$env:TEMP\\TaikoLocalServer-host-build-blue-battle-assign-stage-fix\" --no-restore"
      - "git diff --check"
      - "dotnet test Tests/Tests.csproj --filter \"FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueBattleCatalogLoaderTests\" --no-restore"
      - "dotnet test Tests/Tests.csproj --filter \"FullyQualifiedName~BlueBattleUserDataTests\" --no-restore"
      - "dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore"
      - "dotnet test Tests/Tests.csproj --no-restore"
      - "dotnet build Host/Host.csproj -o \"$env:TEMP\\TaikoLocalServer-host-build-blue-battle-costume-flag-fix\" --no-restore"
      - "dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore"
      - "dotnet test Tests/Tests.csproj --no-restore"
      - "dotnet build Host/Host.csproj -o \"$env:TEMP\\TaikoLocalServer-host-build-blue-battle-persisted-readback\" --no-restore"
      - "git diff --check"
  retest_needed: "Closed at v1.0 milestone cleanup after external verification confirmation."
  debug_session: ".planning/debug/resolved/blue-battle-first-time-user-crash.md"
  latest_debug_sessions:
    - ".planning/debug/resolved/phase-05-blue-battle-crash.md"
    - ".planning/debug/resolved/blue-battle-byte-arrays.md"
