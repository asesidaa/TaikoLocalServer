---
phase: 04-blue-battle-evidence-and-design
plan: "04-01"
subsystem: blue-battle-evidence
tags: [blue, battle, evidence, proto, generated-wire, gsd]

requires:
  - phase: 03-blue-a8-normal-mode-cabinet-smoke-and-hardening
    provides: "Normal Blue support complete before Track B evidence/design"
provides:
  - "Battle route and equivalent-client evidence pack for BTEV-01"
  - "Proto/generated-wire ownership matrix for BTEV-02"
  - "Explicit 04-03 gate unknowns for unproven defaults, byte widths, and row counts"
affects: [04-02-battle-data-inventory, 04-03-blue-battle-design-gate, 05-blue-battle-runtime-support]

tech-stack:
  added: []
  patterns: [traceable-evidence-pack, proto-wire-ownership-matrix, unknowns-to-gate]

key-files:
  created:
    - .planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md
    - .planning/phases/04-blue-battle-evidence-and-design/04-01-SUMMARY.md
  modified: []

key-decisions:
  - "Preserved BattleUserDataController as a Phase 4 read-only stub reference."
  - "Routed unproven battle defaults, byte widths, route requirements, and row counts to the 04-03 gate instead of assuming values."

patterns-established:
  - "Every battle route or field claim is tied to source citations or marked UNKNOWN for gate approval."
  - "Generated proto presence is treated as schema evidence, not proof of safe runtime defaults."

requirements-completed: [BTEV-01, BTEV-02]

duration: 10 min
completed: 2026-05-30
---

# Phase 04 Plan 04-01: Battle Endpoint, Proto, And Client Evidence Summary

**Blue battle route evidence and proto/generated-wire ownership matrix with unproven runtime defaults routed to the Phase 5 gate**

## Performance

- **Duration:** 10 min
- **Started:** 2026-05-29T23:05:00Z
- **Completed:** 2026-05-29T23:15:01Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments

- Created `04-01-BATTLE-EVIDENCE.md` with the required route sequence for `initialdatacheck.php`, `battleuserdata.php`, and `playresult.php`.
- Recorded equivalent-client evidence from the Blue A0 IDB pass and the accessible local `.tools/blue/EBOOT.ELF.i64` artifact without claiming unproven battle semantics.
- Added a field-by-field proto/generated-wire ownership matrix for initialdata battle flags, battle userdata, nested NPC/token rows, battle-stage playresult data, and release-battle data.
- Preserved Phase 4 no-runtime-write guardrails and kept unknown defaults/widths/row counts explicit for 04-02/04-03.

## Task Commits

1. **Task 1: Create the route and equivalent-client evidence pack** - `948c8542` (docs)
2. **Task 2: Add the battle proto and generated-wire ownership matrix** - `22b0574a` (docs)

## Files Created/Modified

- `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` - Route, equivalent-client, current-state, unknown-gate, no-runtime-write, and proto/wire ownership evidence.
- `.planning/phases/04-blue-battle-evidence-and-design/04-01-SUMMARY.md` - Plan closeout summary.

## Decisions Made

- Preserved `BattleUserDataController` as a Phase 4 read-only success-shaped stub reference.
- Treated optional generated wire fields and repeated lists as schema evidence only; unproven runtime defaults remain omission candidates or 04-03 gate items.
- Did not require RPCS3/cabinet logs for BTEV-01 in Phase 4, matching D-01.

## Deviations from Plan

None - plan executed exactly as written.

**Total deviations:** 0 auto-fixed.
**Impact on plan:** No scope expansion; all work stayed in the allowed documentation surface.

## Issues Encountered

- IDA executables were not on PATH, so no fresh decompilation claims were made. The local `.i64` file was accessible and direct artifact search plus the existing A0 evidence were used.
- A transient Git index lock appeared when staging was attempted in parallel with a status read; it cleared immediately and staging was retried serially.

## Verification Results

- Task 1 automated token check: PASS.
- Task 2 automated token check: PASS.
- Field coverage spot-check for battle stage, NPC, token, and release fields: PASS.
- Plan verification command for route tokens: PASS.
- Plan verification command for `InitialdatacheckResponse`, `BattleUserDataResponse`, `BattleStageData`, and `ReleaseBattleData`: PASS.
- Runtime/source/test/proto no-write check across the task commits: PASS.
- Dotnet tests and Host build were not run because this was a docs-only plan and no source or test files changed.

## Known Stubs

None - the artifact documents unknown evidence gaps, but does not add code or UI stubs.

## Threat Flags

None - no new endpoints, auth paths, file access paths, schema changes, runtime loaders, or persistence surfaces were introduced.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `04-02`, which should inventory local battle data and prove or gate byte widths, defaults, and row counts. `04-03` must still approve or block the remaining unknown route/default assumptions before Phase 5 runtime work.

## Self-Check: PASSED

- Found `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md`.
- Found `.planning/phases/04-blue-battle-evidence-and-design/04-01-SUMMARY.md`.
- Found task commit `948c8542`.
- Found task commit `22b0574a`.

---
*Phase: 04-blue-battle-evidence-and-design*
*Completed: 2026-05-30*
