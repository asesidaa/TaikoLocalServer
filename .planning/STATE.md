---
gsd_state_version: 1.0
milestone: v1.7
milestone_name: MOMOIRO AC15 0.11 Support
current_phase: 42
current_phase_name: MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility
status: executing
stopped_at: Phase 42 planned; 7 plans ready
last_updated: "2026-06-26T13:04:07.854Z"
last_activity: 2026-06-26
last_activity_desc: Phase 42 planning complete; 7 plans ready
progress:
  total_phases: 6
  completed_phases: 3
  total_plans: 21
  completed_plans: 14
  percent: 67
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-25)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating MOMOIRO, KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Phase 42 - MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility

## Current Position

Phase: 42 of 44 (MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility)
Plan: 7 plans ready; next execute Phase 42 Wave 0
Status: Ready to execute
Last activity: 2026-06-26 - Phase 42 planning complete; 7 plans ready

Progress: [#######---] 67%

## Performance Metrics

**Velocity:**

- Total plans completed in v1.7: 14
- Average duration: 17 min
- Total execution time: 93 min

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 39 | 4 | - | - |
| 40 | 5 | - | - |
| 41 | 5/5 | 67 min | 13 min |
| 42 | 0/7 planned | - | - |
| 43 | 0/TBD | - | - |
| 44 | 0/TBD | - | - |

**Recent Trend:**

- v1.6 KIMIDORI shipped on 2026-06-25 with Phases 35-38 complete and cabinet/RPCS3 acceptance recorded.
- v1.7 MOMOIRO starts at Phase 39 and follows a six-phase shape after folding route behavior into catalog/limits work.

| Phase 39 P01 | 7 min | 3 tasks | 4 files |
| Phase 39 P02 | 18 min | 3 tasks | 11 files |
| Phase 39 P03 | 17 min | 3 tasks | 5 files |
| Phase 39 P04 | 28 min | 3 tasks | 14 files |
| Phase 41 P01 | 14 min | 3 tasks | 5 files |
| Phase 41 P02 | 12min | 2 tasks | 12 files |
| Phase 41 P03 | 13min | 3 tasks | 12 files |
| Phase 41 P04 | 18min | 3 tasks | 7 files |
| Phase 41 P05 | 10min | 3 tasks | 2 files |

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
- [Phase 39]: Plan 39-04 uses generated Momoiro response objects directly in no-state controllers - Phase 39 proves route ownership only; runtime semantics are deferred to Phases 40-42, so controllers must not call Mediator, EF, catalogs, AdminApi, WebUI, or adjacent-era handlers.
- [Phase 39]: Plan 39-04 records Momoiro controllers as intentional no-state scaffolds - Catalog, identity, userdata, score, playresult, recommendation, telop, song-hash, AdminApi, WebUI, and persistence behavior remain future phase scope.
- [Phase 39]: Plan 39-04 uses MVC ApplicationPartManager controller discovery for route-surface tests - The tests inspect runtime discovery surfaces and avoid source-text or generated-wire assertions.
- [Phase 41]: Plan 41-01 added RED-only Momoiro readback contracts; MORDB runtime requirements remain pending until later implementation plans. Rationale: The user explicitly scoped Wave 0 to tests only, so requirements checkboxes were not marked complete.
- [Phase 41]: Plan 41-02 persists Momoiro favorite order as DisplayOrder with (Baid, DisplayOrder) index while keeping (Baid, SongNo) as the row key. Rationale: Phase 41 readback must return ary_favorite_song_no by user-facing order, not song-number sorting.
- [Phase 41]: Plan 41-02 adds only the four Momoiro readback tables and leaves unsupported mutation/Admin/proto surfaces absent. Rationale: Matches the persistence-only plan and user constraints.
- [Phase 41]: Plan 41-02 completed the persistence/schema slice only; MORDB runtime requirement checkboxes remain pending until handler/controller readback plans complete. Rationale: Avoids overclaiming runtime readback while focused tests still fail on unsupported Momoiro dispatch.
- [Phase 41]: Plan 41-03 packs Momoiro userdata crowns by catalog/file order instead of raw song-id index. Rationale: Momoiro high raw song IDs can exceed CrownSongCount while still belonging at a valid catalog ordinal.
- [Phase 41]: Plan 41-03 leaves Momoiro controller and Mapperly crown serialization failures to plan 41-04. Rationale: The user scoped 41-03 to Application-layer handlers, and the remaining failures are in the still-scaffolded Momoiro UserDataController.
- [Phase 41]: Plan 41-03 keeps shared Ac15SelfBestService unchanged and trims empty Momoiro shin rows only in the Momoiro handler. Rationale: Other eras already depend on shared self-best response shape, while the Momoiro RED contract expects only populated shin rows.
- [Phase 41]: Momoiro protocol controllers now route BAID, MyDon, userdata, and selfbest through Application handlers instead of scaffold responses. Rationale: Plan 41-04 replaces route scaffolds with thin Mediator-backed controllers for the proven Momoiro route set.
- [Phase 41]: Momoiro userdata maps Ac15UserDataResponse.HashCrownFlg to wire UserDataResponse.HashCrownFlg only when Application supplies the bytes. Rationale: Crowns are userdata-owned for Momoiro and the Application handler already builds the catalog-order compact payload.
- [Phase 41]: Unsupported Momoiro challenge, friend, auto-title, and proto-only route fields remain unassigned. Rationale: Phase 41 supports only the proven BAID, MyDon, userdata, and selfbest readback fields and must not infer unsupported route families from generated wire presence.
- [Phase 41]: Phase 41 closes MORDB-01 through MORDB-05 as automated server-side Momoiro readback verification only. Rationale: Focused tests, full serialized tests, builds, proto cleanliness, source gates, and Mapperly generated-source inspection all passed; cabinet/RPCS3 acceptance remains Phase 44.
- [Phase 41]: Mapperly generated-source inspection is the evidence for Momoiro BAID, userdata, and selfbest wire assignments. Rationale: BaidResponseMapper.g.cs, UserDataMappers.g.cs, and SelfBestMappers.g.cs were emitted and inspected after an EmitCompilerGeneratedFiles=true adapter build.
- [Phase 41]: EF migration designer full-snapshot grep hits are not treated as unsupported Momoiro state unless the Momoiro-specific schema surface contains those tables. Rationale: The broad source gate produced adjacent-era false positives, while intent-focused Momoiro unsupported-state and route gates passed.

### Pending Todos

- Execute Phase 42 plans 42-01 through 42-07.

### Blockers/Concerns

- Phase 41 implementation must make the 41-01 RED tests pass without treating those tests as cabinet/RPCS3 acceptance.
- Phase 39-42 planning must preserve evidence gates for song unlocking, crown packing, and changed limits before stateful implementation relies on those facts.
- Future requirements MOLATER-01, MOSPEC-01, and MOSPEC-02 remain deferred and are not active v1.7 phase work.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Later MOMOIRO versions | MOLATER-01: later update behavior or multi-version MOMOIRO route/catalog selection | Future requirement | v1.7 requirements |
| Special capability expansion | MOSPEC-01: Taikojuku, Tokkun, Banacoin, battle, gacha, tournament, Don Challenge, ChallengeCompe, event-folder, newer shop authority, or live-service shop behavior | Future requirement | v1.7 requirements |
| Special capability expansion | MOSPEC-02: rich AdminApi/WebUI editing for packed crown, release-song, favorite/recent, challenge, or Dan state after limits stabilize | Future requirement | v1.7 requirements |

## Session Continuity

Last session: 2026-06-26T09:41:42.973Z
Stopped at: Completed 41-05-PLAN.md
Resume file: None

## Operator Next Steps

- Execute Phase 42 for evidence-backed Momoiro normal playresult mutation, unlocks, rewards, and Dan compatibility.
