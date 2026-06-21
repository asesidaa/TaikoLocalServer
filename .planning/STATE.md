---
gsd_state_version: 1.0
milestone: v1.5
milestone_name: Murasaki AC15 Support
status: planning
last_updated: "2026-06-21T18:30:02.8358586+08:00"
last_activity: 2026-06-21
progress:
  total_phases: 7
  completed_phases: 0
  total_plans: 0
  completed_plans: 0
  percent: 0
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-21)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Phase 28 - Murasaki Evidence and Era Foundation

## Current Position

Phase: 28 of 34 (1 of 7 in v1.5)
Plan: Not planned yet
Status: Ready to plan Phase 28
Last activity: 2026-06-21 - v1.5 roadmap created from Murasaki requirements

Progress: [----------] 0%

## Performance Metrics

**Velocity:**
- Total plans completed in v1.5: 0
- Average duration: n/a
- Total execution time: n/a

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 28 | 0/TBD | - | - |
| 29 | 0/TBD | - | - |
| 30 | 0/TBD | - | - |
| 31 | 0/TBD | - | - |
| 32 | 0/TBD | - | - |
| 33 | 0/TBD | - | - |
| 34 | 0/TBD | - | - |

**Recent Trend:**
- No v1.5 plans have executed yet.
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
Stopped at: v1.5 roadmap created; Phase 28 is ready for `$gsd-plan-phase 28`
Resume file: `.planning/ROADMAP.md`
