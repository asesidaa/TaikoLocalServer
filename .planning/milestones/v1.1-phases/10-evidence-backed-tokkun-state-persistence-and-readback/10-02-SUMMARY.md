---
phase: 10-evidence-backed-tokkun-state-persistence-and-readback
plan: "02"
subsystem: application
tags: [blue, tokkun, playresult, persistence, sqlite]

requires:
  - phase: 10-evidence-backed-tokkun-state-persistence-and-readback
    provides: PlayMode.Tokkun classifier and Blue Tokkun persistence schema from plan 01
provides:
  - Blue Tokkun playresult branch that persists nullable tutorial state
  - Append-only BlueTokkunStageResults rows from classified Tokkun uploads
  - Handler tests proving repeated uploads append and forbidden state remains untouched
affects: [phase-10, phase-11, blue-playresult, blue-userdata]

tech-stack:
  added: []
  patterns: [Blue handler partial, raw protocol fact persistence, no-cross-write handler assertions]

key-files:
  created:
    - Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs
  modified:
    - Application/Handlers/UpdatePlayResultCommand.Blue.cs
    - Tests/Blue/BluePlayResultHandlerTests.cs

key-decisions:
  - "Classified Tokkun uploads update only nullable tutorial state and optional raw history rows."
  - "Repeated Tokkun uploads append history rows even when client timestamps repeat."

patterns-established:
  - "Tokkun branch stays after guest/unknown-user exits and before battle/normal write paths."
  - "Non-Tokkun tutorial-only payloads leave Tokkun tutorial state unchanged."

requirements-completed:
  - TKST-01
  - TKST-02
  - TKST-03
  - TKST-04

duration: 8 min
completed: 2026-06-06
---

# Phase 10 Plan 02: Handler Persistence Summary

**Classified Blue Tokkun uploads now persist only raw tutorial and stage-history facts while preserving Phase 9 no-cross-write boundaries**

## Performance

- **Duration:** 8 min
- **Started:** 2026-06-06T15:33:30Z
- **Completed:** 2026-06-06T15:40:52Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments

- Replaced the Blue Tokkun log-only branch with `HandleBlueTokkun`.
- Persisted `TokkunTutorialFlg` only when a classified Tokkun upload includes the optional value.
- Appended `BlueTokkunStageResult` rows from `TokkunStageData`, preserving `PlayDatetime`, `BanacoinDatetime`, raw counters, and JSON song order/duplicates.
- Updated handler tests for existing-user, unknown-user, mixed-payload, repeated-upload, and non-Tokkun tutorial-only boundaries.

## Task Commits

1. **Tasks 1-2: Persist tutorial/summary facts and preserve no-write boundaries** - `687051de` (feat)

## Files Created/Modified

- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - Delegates classified Tokkun uploads to the Blue Tokkun helper before battle/normal writes.
- `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs` - Saves nullable tutorial and optional raw history state.
- `Tests/Blue/BluePlayResultHandlerTests.cs` - Proves allowed writes, repeated append behavior, unknown-user no-write behavior, mixed payload branch ordering, and non-Tokkun tutorial isolation.

## Decisions Made

The handler serializes `TokkunStageData.TookunSongnoes` with `System.Text.Json` at the write boundary, matching the schema proof from plan 01 and avoiding ad hoc parsing.

## Deviations from Plan

None - plan executed exactly as written.

**Total deviations:** 0 auto-fixed.
**Impact on plan:** None.

## Issues Encountered

- The explicit non-Tokkun tutorial-only regression already passed after the Task 1 helper implementation because the normal branch does not consume `TokkunTutorialFlg`. It remains as boundary coverage.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Plan 10-03 can project the persisted nullable tutorial value through Blue userdata while keeping summary/history server-side only.

---
*Phase: 10-evidence-backed-tokkun-state-persistence-and-readback*
*Completed: 2026-06-06*
