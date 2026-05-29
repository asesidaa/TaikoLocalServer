---
phase: 04-blue-battle-evidence-and-design
plan: "04-02"
subsystem: evidence
tags: [blue, battle, evidence, xml-inventory, protobuf, defaults]

requires:
  - phase: 04-blue-battle-evidence-and-design
    provides: 04-01 battle route and proto/wire evidence context
provides:
  - Local Blue battle XML inventory with hashes, counts, key fields, and candidate-only classifications
  - Battle byte/default/repeated-row proof matrix routing unsafe assumptions to the 04-03 gate
affects: [phase-05-blue-battle-runtime, BTEV-03, BTEV-04]

tech-stack:
  added: []
  patterns:
    - Structured XML parsing for local evidence inventory
    - Optional protobuf battle fields omitted by default until width/default proof exists

key-files:
  created:
    - .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md
    - .planning/phases/04-blue-battle-evidence-and-design/04-02-SUMMARY.md
  modified: []

key-decisions:
  - "All five local Blue battle XML files remain candidate data only until IDA/client proof establishes runtime role."
  - "Unproven battle widths, defaults, row counts, stage assignments, token/NPC semantics, boss-life defaults, and last-stage behavior are routed to 04-03 case-by-case approval."
  - "Optional battle protobuf fields with unproven defaults stay omitted by default using generated presence semantics."

patterns-established:
  - "Phase 4 battle evidence artifacts record hashes/counts and proof status without creating loaders or committed runtime data."
  - "Phase 5 battle planning must name ShouldSerialize, byte-length, repeated-row, and Green AI Battle source-guard tests before implementation."

requirements-completed: [BTEV-03, BTEV-04]

duration: 10 min
completed: 2026-05-29
---

# Phase 04 Plan 04-02: Blue Battle Data Inventory Summary

**Local Blue battle XML inventory and default/width proof matrix that blocks unsafe Phase 5 battle assumptions**

## Performance

- **Duration:** 10 min
- **Started:** 2026-05-29T23:18:45Z
- **Completed:** 2026-05-29T23:28:45Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments

- Inventoried `battleadjsetting.xml`, `battlenpcinfo.xml`, `battlestageinfo.xml`, `battlesupportinfo.xml`, and `battletokeninfo.xml` with sizes, SHA-256 hashes, structured parse counts, key fields, candidate roles, and menu-entry classifications.
- Added a field-by-field proof matrix for initialdata, battleuserdata, playresult `BattleStageData`, `ReleaseBattleData`, NPC/token rows, stage id `33`, reward `type` values `0` and `1`, boss-life, first/next-stage, and last-stage concerns.
- Preserved Phase 4 scope: no runtime loaders, committed battle JSON, EF migrations, source/test changes, generated data, or local game-data edits.

## Task Commits

1. **Task 1: Create the local battle XML inventory** - `c730272d` (docs)
2. **Task 2: Build the byte/default and repeated-row proof matrix** - `02e9b22f` (docs)

## Files Created/Modified

- `.planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md` - Local battle XML inventory and proof matrix.
- `.planning/phases/04-blue-battle-evidence-and-design/04-02-SUMMARY.md` - Plan closeout summary.

## Decisions Made

- All five XML files are classified as `UNKNOWN for battle menu entry until IDA/client proof`.
- Local XML counts are evidence of file shape only; they do not prove response row counts, byte widths, or runtime defaults.
- The 04-03 gate must keep Phase 5 `BLOCKED` or `APPROVED_WITH_USER_EXCEPTIONS` until each unresolved battle default is proven or approved individually.

## Verification Results

- `Get-ChildItem Host/wwwroot/data/blue/data/config/S10100-1/battle -Filter *.xml | Select-Object Name,Length` passed and showed all five required XML files with expected lengths.
- `Select-String -Path .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md -Pattern "battleadjsetting.xml","battlenpcinfo.xml","battlestageinfo.xml","battlesupportinfo.xml","battletokeninfo.xml"` passed.
- `Select-String -Path .planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md -Pattern "UNKNOWN","APPROVED_WITH_USER_EXCEPTIONS","release_info_flg","npc_costume_flg"` passed.
- Task 1 and Task 2 automated verification blocks from `04-02-PLAN.md` passed.
- Dotnet tests and Host build were not run because this was docs-only and no source/test/runtime files changed.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## Known Stubs

None. The document intentionally records unresolved evidence as gate unknowns, not implementation stubs.

## Auth Gates

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Plan 04-03 to make the final Blue battle design gate decision. Phase 5 remains blocked for battle runtime defaults until 04-03 approves or records explicit user exceptions for unresolved widths, defaults, row counts, stage assignments, token/NPC semantics, boss-life defaults, and last-stage behavior.

## Self-Check: PASSED

- Found `.planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md`.
- Found `.planning/phases/04-blue-battle-evidence-and-design/04-02-SUMMARY.md`.
- Found task commit `c730272d`.
- Found task commit `02e9b22f`.

---
*Phase: 04-blue-battle-evidence-and-design*
*Completed: 2026-05-29*
