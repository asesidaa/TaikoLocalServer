---
gsd_state_version: 1.0
milestone: v1.5
milestone_name: Murasaki AC15 Support
status: in_progress
last_updated: "2026-06-21T21:20:00+08:00"
last_activity: 2026-06-21
progress:
  total_phases: 7
  completed_phases: 5
  total_plans: 5
  completed_plans: 5
  percent: 71
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-21)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Phase 33 - Evidence-Gated Special Capabilities

## Current Position

Phase: 33 of 34 (6 of 7 in v1.5)
Plan: Not planned yet
Status: Paused after `$gsd-autonomous --to 32`; ready to plan Phase 33
Last activity: 2026-06-21 - Murasaki phases 28-32 implemented and verified

Progress: [#######---] 71%

## Performance Metrics

**Velocity:**
- Total plans completed in v1.5: 5
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
| 33 | 0/TBD | - | - |
| 34 | 0/TBD | - | - |

**Recent Trend:**
- Phases 28-32 executed in one autonomous range and verified with solution build plus 845 tests.
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

- Murasaki special surfaces such as `bestscore.php`, `songhash.php`, `shoppingresult.php`, challenge arrays, `content_info`, `default_option_setting`, and reserved bytes remain evidence-gated until Phase 33.
- Cabinet/RPCS3 and WebUI acceptance are closeout gates for Phase 34 and must not be claimed from automated tests alone.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Later Murasaki versions | MLATER-01: later update behavior or multi-root version selection | Future requirement | v1.5 requirements |
| Special capability expansion | MSPEC-04: full global ranking, shopping authority, challenge scheduling/management, or operator-authored challenge behavior | Future requirement | v1.5 requirements |

## Session Continuity

Last session: 2026-06-21
Stopped at: `$gsd-autonomous --to 32` completed; Phase 33 is ready for `$gsd-plan-phase 33`
Resume file: `.planning/ROADMAP.md`
