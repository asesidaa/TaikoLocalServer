---
status: partial
phase: 05-blue-battle-runtime-support
source:
  - .planning/phases/05-blue-battle-runtime-support/05-02-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-07-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-08-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-11-SUMMARY.md
  - .tools/crash1.txt
started: 2026-05-31T17:36:33+08:00
updated: 2026-05-31T17:52:56+08:00
---

## Current Test

number: 1
name: First-Time Blue Battle User Request
expected: |
  A new Blue user can enter the first battle flow; `battleuserdata.php` returns client-safe first-use battle state, and RPCS3 does not freeze or crash after the battle user request.
awaiting: RPCS3 retest after server fix

## Tests

### 1. First-Time Blue Battle User Request
expected: A new Blue user can enter the first battle flow; `battleuserdata.php` returns client-safe first-use battle state, and RPCS3 does not freeze or crash after the battle user request.
result: [pending]
previous_issue: "Now the game crashes on first time battle user request. The crash log from RPCS3 is in .tools/crash1.txt."
fix: "Server fix implemented: first-time `battleuserdata.php` now emits a catalog-derived `last_npc_id` when parsed battle XML advertises battle and no persisted value exists."
severity: blocker

## Summary

total: 1
passed: 0
issues: 0
pending: 1
skipped: 0
blocked: 0

## Gaps

- truth: "A new Blue user can enter the first battle flow; battleuserdata.php returns client-safe first-use battle state, and RPCS3 does not freeze or crash after the battle user request."
  status: fixed-pending-retest
  reason: "User reported: Now the game crashes on first time battle user request. The crash log from RPCS3 is in .tools/crash1.txt."
  severity: blocker
  test: 1
  root_cause: "Phase 05 advertises battle from parsed XML, but first-time battleuserdata still returns result=1 with omitted last_npc_id and empty npc_data. IDA shows omitted last_npc_id defaults to 0, while local battlenpcinfo.xml contains npc id 1. The RPCS3 crash later occurs in the skin NUD draw request path with freed-memory sentinel values, consistent with invalid battle NPC/model state after a success-shaped response."
  artifacts:
    - path: ".tools/crash1.txt"
      issue: "RPCS3 access violation in NU::Draw::RequestManager at 0x005997b0 reading 0xddddde09."
    - path: ".tools/blue/EBOOT.ELF.i64"
      issue: "IDA proves sub_5997B0 is nuRequestNud20DrawSkinPs3 draw request and sub_7497C copies default-zero last_npc_id on successful battleuserdata."
    - path: "Application/Handlers/GetBattleUserDataQuery.Blue.cs"
      issue: "New users return success without persisted LastNpcId or NPC rows."
    - path: "Host/wwwroot/data/blue/data/config/S10100-1/battle/battlenpcinfo.xml"
      issue: "Local parsed battle NPC id is 1, not 0."
  missing:
    - "Repeat RPCS3 first-time battle smoke after the server fix."
  fix:
    summary: "Exposed parsed Blue battle NPC ids through `BlueBattleCatalog`; `GetBattleUserDataQueryHandler` now falls back to the first catalog NPC id for new users only when battle data is advertised."
    tests:
      - "dotnet test Tests/Tests.csproj --filter \"FullyQualifiedName~BlueBattleUserDataTests|FullyQualifiedName~BlueBattleCatalogLoaderTests\" --no-restore"
      - "dotnet test Tests/Tests.csproj --filter BlueBattle --no-restore"
      - "dotnet test Tests/Tests.csproj --no-restore"
      - "dotnet build Host/Host.csproj -o \"$env:TEMP\\TaikoLocalServer-host-build-blue-battle-crash-fix\" --no-restore"
  retest_needed: "Run the same first-time RPCS3 battle flow. If `last_npc_id` alone still crashes, the next hypothesis is a conservative starter `npc_data` row for the same parsed NPC id."
  debug_session: ".planning/debug/blue-battle-first-time-user-crash.md"
