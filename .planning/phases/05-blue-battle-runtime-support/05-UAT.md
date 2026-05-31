---
status: diagnosed
phase: 05-blue-battle-runtime-support
source:
  - .planning/phases/05-blue-battle-runtime-support/05-02-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-07-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-08-SUMMARY.md
  - .planning/phases/05-blue-battle-runtime-support/05-11-SUMMARY.md
  - .tools/crash1.txt
started: 2026-05-31T17:36:33+08:00
updated: 2026-05-31T17:36:33+08:00
---

## Current Test

[testing complete]

## Tests

### 1. First-Time Blue Battle User Request
expected: A new Blue user can enter the first battle flow; `battleuserdata.php` returns client-safe first-use battle state, and RPCS3 does not freeze or crash after the battle user request.
result: issue
reported: "Now the game crashes on first time battle user request. The crash log from RPCS3 is in .tools/crash1.txt."
severity: blocker

## Summary

total: 1
passed: 0
issues: 1
pending: 0
skipped: 0
blocked: 0

## Gaps

- truth: "A new Blue user can enter the first battle flow; battleuserdata.php returns client-safe first-use battle state, and RPCS3 does not freeze or crash after the battle user request."
  status: failed
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
    - "Expose parsed Blue battle NPC ids from BlueBattleDataLoader/BlueBattleCatalog."
    - "Return a catalog-derived first-time LastNpcId when battle data is available and no persisted value exists."
    - "Add focused tests proving the default is catalog-derived and unavailable catalog behavior remains conservative."
    - "Repeat RPCS3 first-time battle smoke after the server fix."
  debug_session: ".planning/debug/blue-battle-first-time-user-crash.md"
