---
gsd_state_version: 1.0
milestone: v1.5
milestone_name: Murasaki AC15 Support
current_phase: 34
current_phase_name: AdminApi/WebUI and Runtime Closeout
status: in_progress
stopped_at: Phase 33 complete; Phase 34 ready for planning
last_updated: "2026-06-21T22:20:37+08:00"
last_activity: 2026-06-21
last_activity_desc: Phase 33 complete, transitioned to Phase 34
progress:
  total_phases: 7
  completed_phases: 6
  total_plans: 6
  completed_plans: 6
  percent: 86
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-21)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Phase 34 - AdminApi/WebUI and Runtime Closeout

## Current Position

Phase: 34 of 34 (AdminApi/WebUI and Runtime Closeout)
Plan: Not started
Status: Phase 33 complete; ready to plan Phase 34
Last activity: 2026-06-21 - Phase 33 complete, transitioned to Phase 34

Progress: [#########-] 86%

## Performance Metrics

**Velocity:**

- Total plans completed in v1.5: 6
- Average duration: n/a
- Total execution time: n/a

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 28 | 1/1 | - | - |
| 29 | 1/1 | - | - |
| 30 | 1/1 | - | - |
| 31 | 1/1 | - | - |
| 32 | 1/1 | - | - |
| 33 | 1/1 | - | - |
| 34 | 0/TBD | - | - |

**Recent Trend:**

- Phases 28-32 executed in one autonomous range and verified with solution build plus 845 tests.
- Phase 33 closed unsupported Murasaki special surfaces with route/byte evidence and 847-test verification.
- Prior milestone v1.4 closed on 2026-06-21 after automated verification and user-accepted runtime/WebUI evidence.

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table. Recent decisions affecting current work:

- Start Murasaki support as v1.5 because it is the next older AC15 era after White and local Murasaki proto/data are present.
- Treat Murasaki changed wire shape as a first-class evidence gate; split metadata/global-score request families must not be hidden behind White `initialdatacheck.php`.
- Reuse existing AC15/White-like capabilities only where Murasaki proto, data, binary/client, logs, or cabinet/RPCS3 evidence proves matching behavior and limits.

### Pending Todos

None.

### Blockers/Concerns

- Unsupported Murasaki special surfaces such as `bestscore.php`, `songhash.php`, `shoppingresult.php`, ChallengeCompe, Don Challenge, Yellow shop, Banacoin, and global-score persistence remain future evidence-gated work.
- Cabinet/RPCS3 and WebUI acceptance are closeout gates for Phase 34 and must not be claimed from automated tests alone.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Later Murasaki versions | MLATER-01: later update behavior or multi-root version selection | Future requirement | v1.5 requirements |
| Special capability expansion | MSPEC-04: full global ranking, shopping authority, challenge scheduling/management, or operator-authored challenge behavior | Future requirement | v1.5 requirements |

## Session Continuity

Last session: 2026-06-21
Stopped at: Phase 33 complete; Phase 34 is ready for `$gsd-plan-phase 34`
Resume file: `.planning/ROADMAP.md`
