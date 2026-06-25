---
gsd_state_version: 1.0
milestone: v1.7
milestone_name: MOMOIRO AC15 0.11 Support
current_phase: 39
current_phase_name: MOMOIRO Evidence and Era Foundation
status: executing
stopped_at: Completed 39-03-PLAN.md
last_updated: "2026-06-25T21:03:44.861Z"
last_activity: 2026-06-26
last_activity_desc: Completed 39-03 Momoiro Host settings, DI, fallback, and application-part gating.
progress:
  total_phases: 6
  completed_phases: 0
  total_plans: 4
  completed_plans: 3
  percent: 75
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-25)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating MOMOIRO, KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Phase 39 - MOMOIRO Evidence and Era Foundation.

## Current Position

Phase: 39 of 44 (MOMOIRO Evidence and Era Foundation)
Plan: 4 of 4 (next: 39-04)
Status: Ready to execute
Last activity: 2026-06-26 - Completed 39-03 Momoiro Host settings, DI, fallback, and application-part gating.

Progress: [########--] 75%

## Performance Metrics

**Velocity:**

- Total plans completed in v1.7: 3
- Average duration: 14 min
- Total execution time: 42 min

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 39 | 3/4 | 42 min | 14 min |
| 40 | 0/TBD | - | - |
| 41 | 0/TBD | - | - |
| 42 | 0/TBD | - | - |
| 43 | 0/TBD | - | - |
| 44 | 0/TBD | - | - |

**Recent Trend:**

- v1.6 KIMIDORI shipped on 2026-06-25 with Phases 35-38 complete and cabinet/RPCS3 acceptance recorded.
- v1.7 MOMOIRO starts at Phase 39 and follows a six-phase shape after folding route behavior into catalog/limits work.

| Phase 39 P01 | 7 min | 3 tasks | 4 files |
| Phase 39 P02 | 18 min | 3 tasks | 11 files |
| Phase 39 P03 | 17 min | 3 tasks | 5 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table. Recent decisions affecting current work:

- Start MOMOIRO support as v1.7 because it is the next older AC15 era after KIMIDORI and local MOMOIRO proto, binary evidence, and linked root-level game data are present.
- Define MOMOIRO features from `proto/momoiro` plus binary/client `.php` route evidence; unsupported feature families remain absent.
- Treat MOMOIRO crowns as userdata-owned through `hash_crown_flg` unless new local evidence proves a separate crown contract.
- Treat MOMOIRO data as root-level era data under `Host/wwwroot/data/momoiro/data`, not a newer `config/STxxxx-*` layout.
- Treat the supplied MOMOIRO route list as the active route inventory: startup/version use `/v01r00`, game routes use `/v04r00`, and proto-only route families remain absent.
- MOMOIRO routes should be feature-complete where they carry feature behavior, or explicit static-result operational stubs where the route role is static; do not create a separate route-readback phase.
- Plan 39-01 records the route inventory as supplied and locked by Phase 39 context; fresh IDA route-string offsets were not recaptured in that plan.
- Wave 0 Momoiro tests are intentionally red until later Phase 39 plans add `GameEra.Momoiro`, settings, adapter identity, and application-part wiring.
- [Phase 39]: Plan 39-02 kept Momoiro limited to adapter identity and generated wire; Host registration, application-part gating, controllers, runtime state, AdminApi, and WebUI stay deferred. Rationale: Matches the plan file and preserves the Phase 39 Wave 1 boundary.
- [Phase 39]: Plan 39-02 generated Momoiro wire from proto/momoiro with repo-local protogen and left proto inputs untouched. Rationale: MOFND-04 requires generated wire to be evidence input without turning proto-only families into route behavior.
- [Phase 39]: Plan 39-03 moved the Host Momoiro project reference into the Host-registration task. Rationale: Program.cs could not compile against the Momoiro adapter namespace until Host.csproj referenced the adapter project.
- [Phase 39]: Plan 39-03 keeps Momoiro protobuf fallback exact to /v04r00/chassis. Rationale: Momoiro game posts use MomoiroRoutePrefixes.Game while shared /v01r00 startup/version fallback remains unchanged.
- [Phase 39]: Plan 39-03 uses shared application-part removal as the Momoiro disabled-route gate. Rationale: Keeps route exposure centralized in GameProtocolApplicationParts instead of duplicating era checks in controllers.

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

Last session: 2026-06-25T21:01:22.043Z
Stopped at: Completed 39-03-PLAN.md
Resume file: None

## Operator Next Steps

- Execute Phase 39 plan `39-04-PLAN.md`.
