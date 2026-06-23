---
gsd_state_version: 1.0
milestone: v1.5
milestone_name: Murasaki AC15 Support
current_phase: null
current_phase_name: null
status: awaiting_next_milestone
stopped_at: Milestone v1.5 completed and archived; awaiting next milestone definition
last_updated: "2026-06-23T21:39:34+08:00"
last_activity: 2026-06-23
last_activity_desc: Milestone v1.5 completed and archived
progress:
  total_phases: 7
  completed_phases: 7
  total_plans: 7
  completed_plans: 7
  percent: 100
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-23)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Between milestones; define the next milestone with `$gsd-new-milestone`.

## Current Position

Milestone v1.5 Murasaki AC15 Support is complete and archived.

Status: awaiting next milestone
Last activity: 2026-06-23 - Milestone v1.5 completed and archived

Progress: [##########] 100%

## Performance Metrics

**Velocity:**

- Total plans completed in v1.5: 7
- Quick tasks completed during v1.5 closeout: 1
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
| 34 | 1/1 | - | - |

**Recent Trend:**

- Phases 28-32 executed in one autonomous range and verified with solution build plus 845 tests.
- Phase 33 closed unsupported Murasaki special surfaces with route/byte evidence and 847-test verification.
- Phase 34 implemented Murasaki AdminApi/WebUI parity for supported surfaces and passed focused verification.
- Quick task 260623-2ff added final `/v06r01` Murasaki route support while preserving `/v06r00` compatibility.
- Final closeout on 2026-06-23 recorded user-observed Murasaki in-game acceptance, no vulnerable packages, 865 passing tests, and a full solution build with 0 warnings and 0 errors.

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table. Recent decisions affecting future work:

- Start Murasaki support as v1.5 because it is the next older AC15 era after White and local Murasaki proto/data are present.
- Treat Murasaki changed wire shape as a first-class evidence gate; split metadata/global-score request families must not be hidden behind White `initialdatacheck.php`.
- Preserve final `/v06r01` route parity where final binary/proto evidence supports the surface, including Dani/Taikojuku.
- Keep unsupported Murasaki special surfaces absent until route/cabinet/log/IDA evidence proves specific server contracts.

### Pending Todos

- None for v1.5 closeout.

### Blockers/Concerns

- Unsupported Murasaki special surfaces such as `bestscore.php`, `songhash.php`, `shoppingresult.php`, ChallengeCompe, Don Challenge, Yellow shop, Banacoin, and global-score persistence remain future evidence-gated work.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260621-w8x | Improve WebUI era selection and capability visibility | 2026-06-21 | 7007f7cb | [260621-w8x-let-s-have-a-quick-pass-to-improve-web-u](./quick/260621-w8x-let-s-have-a-quick-pass-to-improve-web-u/) |
| 260623-2ff | Add Murasaki final /v06r01 support | 2026-06-23 | 30552da7 | [260623-2ff-now-let-s-add-support-for-murasaki-final](./quick/260623-2ff-now-let-s-add-support-for-murasaki-final/) |

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Later Murasaki versions | MLATER-01: later update behavior or multi-root version selection | Future requirement | v1.5 requirements |
| Special capability expansion | MSPEC-04: full global ranking, shopping authority, challenge scheduling/management, or operator-authored challenge behavior | Future requirement | v1.5 requirements |

## Session Continuity

Last session: 2026-06-23
Stopped at: Milestone v1.5 completed and archived; awaiting next milestone definition
Resume file: `.planning/ROADMAP.md`

## Operator Next Steps

- Start the next milestone with `$gsd-new-milestone`.
