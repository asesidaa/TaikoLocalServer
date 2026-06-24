---
gsd_state_version: 1.0
milestone: v1.6
milestone_name: KIMIDORI AC15 Support
current_phase: 6
status: Awaiting next milestone
stopped_at: KIMIDORI v1.6 milestone archived; ready for next milestone
last_updated: "2026-06-25T04:00:00+08:00"
last_activity: 2026-06-25
last_activity_desc: Milestone v1.6 completed and archived
progress:
  total_phases: 4
  completed_phases: 4
  total_plans: 4
  completed_plans: 4
  percent: 100
current_phase_name: AdminApi, WebUI, and Runtime Closeout
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-25)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Planning the next milestone.

## Current Position

Phase: Milestone v1.6 complete
Plan: —
Status: Awaiting next milestone
Last activity: 2026-06-25 — Milestone v1.6 completed and archived

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
| 35 | 1/1 | - | - |
| 36 | 1/1 | - | - |
| 37 | 1/1 | - | - |
| 38 | 1/1 | complete | 2026-06-25 |

**Recent Trend:**

- Phases 28-32 executed in one autonomous range and verified with solution build plus 845 tests.
- Phase 33 closed unsupported Murasaki special surfaces with route/byte evidence and 847-test verification.
- Phase 34 implemented Murasaki AdminApi/WebUI parity for supported surfaces and passed focused verification.
- Quick task 260623-2ff added final `/v06r01` Murasaki route support while preserving `/v06r00` compatibility.
- Final closeout on 2026-06-23 recorded user-observed Murasaki in-game acceptance, no vulnerable packages, 865 passing tests, and a full solution build with 0 warnings and 0 errors.
- v1.6 KIMIDORI roadmap defines 4 phases with all 19 active requirements mapped.
- Phases 35-37 completed automated implementation for KIMIDORI evidence/foundation, root-level catalog/metadata binding, and KIMIDORI-owned runtime state.
- Phase 38 completed automated AdminApi/WebUI implementation and verification, and user-observed cabinet/RPCS3 runtime smoke acceptance was recorded on 2026-06-25.

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table. Recent decisions affecting future work:

- Start Murasaki support as v1.5 because it is the next older AC15 era after White and local Murasaki proto/data are present.
- Treat Murasaki changed wire shape as a first-class evidence gate; split metadata/global-score request families must not be hidden behind White `initialdatacheck.php`.
- Preserve final `/v06r01` route parity where final binary/proto evidence supports the surface, including Dani/Taikojuku.
- Keep unsupported Murasaki special surfaces absent until route/cabinet/log/IDA evidence proves specific server contracts.
- Start KIMIDORI support as v1.6, using `proto/kimidori`, `.tools/kimidori`, linked root-level KIMIDORI data, `/v01r00` startup/version routes, and `/v05r00` game routes.
- Define KIMIDORI 0.12 features only from protocol presence plus binary `.php` route evidence; if `proto/kimidori` lacks a feature, treat it as missing.
- Treat KIMIDORI game data as root-level era data rather than assuming a newer `config/STxxxx-*` catalog layout.
- Keep KIMIDORI Dani Dojo support separate from Taikojuku practice-folder behavior; missing Taikojuku proto/route evidence does not remove proven Dani result/state support.

### Pending Todos

- None recorded.

### Blockers/Concerns

- KIMIDORI Phase 38 runtime acceptance is recorded; unsupported surfaces remain evidence-gated after closeout.
- KIMIDORI unsupported surfaces remain evidence-gated: Taikojuku, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, and full shop-authority controls.
- Unsupported copied KIMIDORI scaffold files were moved to `.planning/batch-delete/kimidori-scaffold/` for user-managed cleanup; do not delete them directly.

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

Last session: 2026-06-25
Stopped at: KIMIDORI v1.6 milestone archived; ready for next milestone
Resume file: `.planning/ROADMAP.md`

## Operator Next Steps

- Start the next milestone with $gsd-new-milestone
