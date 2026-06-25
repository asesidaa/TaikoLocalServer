---
gsd_state_version: 1.0
milestone: v1.7
milestone_name: MOMOIRO AC15 0.11 Support
current_phase: 39
current_phase_name: MOMOIRO Evidence and Era Foundation
status: executing
stopped_at: v1.7 roadmap revised and ready for Phase 39 planning.
last_updated: "2026-06-25T20:08:19.682Z"
last_activity: 2026-06-26
last_activity_desc: Revised v1.7 roadmap to 6 phases using the supplied MOMOIRO binary route inventory.
progress:
  total_phases: 6
  completed_phases: 0
  total_plans: 0
  completed_plans: 0
  percent: 0
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-25)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating MOMOIRO, KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Phase 39 - MOMOIRO Evidence and Era Foundation.

## Current Position

Phase: 39 of 44 (MOMOIRO Evidence and Era Foundation)
Plan: TBD
Status: Ready to execute
Last activity: 2026-06-26 - Revised v1.7 roadmap to 6 phases using the supplied MOMOIRO binary route inventory.

Progress: [----------] 0%

## Performance Metrics

**Velocity:**

- Total plans completed in v1.7: 0
- Average duration: n/a
- Total execution time: n/a

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 39 | 0/TBD | - | - |
| 40 | 0/TBD | - | - |
| 41 | 0/TBD | - | - |
| 42 | 0/TBD | - | - |
| 43 | 0/TBD | - | - |
| 44 | 0/TBD | - | - |

**Recent Trend:**

- v1.6 KIMIDORI shipped on 2026-06-25 with Phases 35-38 complete and cabinet/RPCS3 acceptance recorded.
- v1.7 MOMOIRO starts at Phase 39 and follows a six-phase shape after folding route behavior into catalog/limits work.

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table. Recent decisions affecting current work:

- Start MOMOIRO support as v1.7 because it is the next older AC15 era after KIMIDORI and local MOMOIRO proto, binary evidence, and linked root-level game data are present.
- Define MOMOIRO features from `proto/momoiro` plus binary/client `.php` route evidence; unsupported feature families remain absent.
- Treat MOMOIRO crowns as userdata-owned through `hash_crown_flg` unless new local evidence proves a separate crown contract.
- Treat MOMOIRO data as root-level era data under `Host/wwwroot/data/momoiro/data`, not a newer `config/STxxxx-*` layout.
- Treat the supplied MOMOIRO route list as the active route inventory: startup/version use `/v01r00`, game routes use `/v04r00`, and proto-only route families remain absent.
- MOMOIRO routes should be feature-complete where they carry feature behavior, or explicit static-result operational stubs where the route role is static; do not create a separate route-readback phase.

### Pending Todos

- None recorded.

### Blockers/Concerns

- Phase 39-42 planning must preserve evidence gates for song unlocking, crown packing, and changed limits before stateful implementation relies on those facts.
- Future requirements MOLATER-01, MOSPEC-01, and MOSPEC-02 remain deferred and are not active v1.7 phase work.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Later MOMOIRO versions | MOLATER-01: later update behavior or multi-version MOMOIRO route/catalog selection | Future requirement | v1.7 requirements |
| Special capability expansion | MOSPEC-01: Taikojuku, Tokkun, Banacoin, battle, gacha, tournament, Don Challenge, ChallengeCompe, event-folder, newer shop authority, or live-service shop behavior | Future requirement | v1.7 requirements |
| Special capability expansion | MOSPEC-02: rich AdminApi/WebUI editing for packed crown, release-song, favorite/recent, challenge, or Dan state after limits stabilize | Future requirement | v1.7 requirements |

## Session Continuity

Last session: 2026-06-26
Stopped at: v1.7 roadmap revised and ready for Phase 39 planning.
Resume file: `.planning/ROADMAP.md`

## Operator Next Steps

- Review the roadmap, then plan Phase 39 with `$gsd-plan-phase 39`.
