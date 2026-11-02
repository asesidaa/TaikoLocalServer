---
gsd_state_version: 1.0
milestone: v1.7
milestone_name: MOMOIRO AC15 0.11 Support
current_phase: 43
current_phase_name: MOMOIRO AdminApi and WebUI Routing
status: complete
stopped_at: Completed quick task 260629-6k9
last_updated: "2026-06-29T04:44:00+08:00"
last_activity: 2026-06-29
last_activity_desc: Completed quick task 260629-6k9: Execute AC15 mapper nullability review fix
progress:
  total_phases: 6
  completed_phases: 5
  total_plans: 22
  completed_plans: 22
  percent: 100
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-25)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating MOMOIRO, KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.
**Current focus:** Phase 43 complete - MOMOIRO AdminApi and WebUI Routing; quick task 260629-6k9 completed AC15 generated optional value mapping cleanup

## Current Position

Phase: 43 of 44 (MOMOIRO AdminApi and WebUI Routing)
Plan: 1 of 1 in current phase
Status: Complete
Last activity: 2026-06-29 - Completed quick task 260629-6k9: Execute AC15 mapper nullability review fix

Progress: [##########] 100%

## Performance Metrics

**Velocity:**

- Total plans completed in v1.7: 22
- Average duration: 15 min
- Total execution time: 122 min

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 39 | 4 | - | - |
| 40 | 5 | - | - |
| 41 | 5/5 | 67 min | 13 min |
| 42 | 7/7 | 98 min | 14 min |
| 43 | 1/1 | 15 min | 15 min |
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
| Phase 42 P01 | 14min | 3 tasks | 5 files |
| Phase 42 P02 | 12min | 3 tasks | 11 files |
| Phase 42 P03 | 14min | 3 tasks | 5 files |
| Phase 42 P04 | 14min | 3 tasks | 7 files |
| Phase 42 P05 | 13min | 3 tasks | 3 files |
| Phase 42 P06 | 17min | 3 tasks | 3 files |
| Phase 42 P07 | 14min | 3 tasks | 4 files |
| Phase 43 P01 | 15min | 5 tasks | 28 files |

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
- [Phase 42]: Plan 42-01 remains RED-only: no production Momoiro playresult dispatch, schema, controller mapping, mapper, migration, or proto code was implemented. — Wave 0 scope is bounded to contracts for later implementation plans.
- [Phase 42]: Momoiro favorite playresult contracts preserve Phase 41 DisplayOrder semantics by appending new favorites after the current max display order. — This guards against reverting to raw song-number ordering during playresult mutation implementation.
- [Phase 42]: Challenge-shaped arrays are contractually accepted, mapped, and dropped without Don Challenge, ChallengeCompe, reward-management, or raw future challenge persistence. — The active MOMOIRO scope allows compatibility handling but not unsupported challenge feature authority.
- [Phase 42]: Plan 42-02 adds Momoiro-owned play-history and bounded Dan persistence schema only. — Later plans own playresult handler/controller runtime behavior, so schema tables are available without marking MORUN runtime requirements complete.
- [Phase 42]: Plan 42-02 validates unsupported feature scope against the active migration body, not EF designer or full snapshot metadata. — The active migration creates only SongPlayDatum_Momoiro, DanScoreDatum_Momoiro, and DanStageScoreDatum_Momoiro; designer snapshots naturally contain existing adjacent-era terms.
- [Phase 42]: Momoiro favorite creation uses an optional normal-play table-bundle factory so existing AC15 eras keep default favorite-row creation while Momoiro can append after the current DisplayOrder max.
- [Phase 42]: Momoiro normal play mapping remains Mapperly source-generator driven; Ac15NormalPlayMapper.g.cs was emitted and inspected for Momoiro play and best assignments.
- [Phase 42]: Momoiro unlock and profile-counter mutation surfaces are limited to UserSaveDataMomoiro fields and Ac15EraProfiles.Momoiro limits; handler/controller behavior remains later Phase 42 scope.
- [Phase 42]: Momoiro exposes bounded Dan course order as DaniFileOrder, not TaikojukuFileOrder. — Plan 42-04 supports playresult/BAID/userdata Dan compatibility without adding Taikojuku route, practice-folder, AdminApi, or WebUI behavior.
- [Phase 42]: Momoiro Features.Dani is enabled only for bounded Dan compatibility while Taikojuku, folders, and item shop remain disabled. — This preserves the unsupported-route boundary and keeps MORUN-05 scope intact.
- [Phase 42]: Momoiro Dan Mapperly projections remain source-generator driven and were verified in emitted Ac15DaniMapper.g.cs. — Mapperly generated source confirms score and stage field assignment for Momoiro Dan rows without handwritten mapper bodies.
- [Phase 42]: Momoiro playresult mutation stays in the AC15 Application command path; no adapter/controller or Mapperly route changes were made. — Plan 42-05 is scoped to Application-layer mutation; 42-06 owns controller mapping.
- [Phase 42]: Momoiro favorites added by playresult append after the current maximum DisplayOrder across persisted and tracked rows. — Preserves the Phase 41 favorite readback order contract.
- [Phase 42]: Momoiro challenge-shaped arrays are diagnostics only in plan 42-05: they are logged and not persisted. — MOMOIRO evidence does not prove Don Challenge, ChallengeCompe, or reward-management authority.
- [Phase 42]: MORUN requirement checkboxes remain pending after plan 42-05. — 42-06 and 42-07 still own controller mapping and final verification closeout.
- [Phase 42]: Momoiro playresult request mapping remains Mapperly source-generator driven. — Generated PlayResultMappers.g.cs assigns release, reward, Dan, and challenge facts.
- [Phase 42]: MORUN requirement checkboxes remain pending after plan 42-06. — The user assigned final Phase 42 verification and closeout to 42-07.
- [Phase 42]: Momoiro playresult.php now dispatches UpdateAc15PlayResultCommand with GameEra.Momoiro. — Adapter code only logs, maps, calls Mediator, and maps the response; persistence stays in Application.
- [Phase 42]: Phase 42 closes MORUN-01 through MORUN-05 as automated server-side verification only. — Focused/full tests, builds, Mapperly generated-source inspection, and source gates passed; AdminApi/WebUI remains Phase 43 and cabinet/RPCS3 acceptance remains Phase 44.
- [Quick 260628-3ef]: MOMOIRO startup movies are enabled server-side through `momoiro_movie_data.json`, raw `data/movie` discovery, and `hdd_ver` 4xx startup dispatch. Rationale: This closes the movie-data blocker found after Phase 42 before Phase 43 AdminApi/WebUI planning proceeds.
- [Phase 43]: Momoiro AdminApi/WebUI routing reuses existing older-AC15 surfaces and points only at Momoiro-owned state. Rationale: Phase 43 closes MOADMIN-01 and MOADMIN-02 without adding unsupported Momoiro feature authority.
- [Phase 43]: Momoiro customization catalog endpoints return Momoiro-owned empty runtime catalogs until Momoiro-specific customization provenance exists. Rationale: The WebUI profile editor can route successfully without borrowing KIMIDORI, Murasaki, or Nijiiro catalog data.
- [Phase 43]: Exact solution build was environment-blocked by running `TaikoLocalServer (14536)`, while the temp-output Host build passed with 0 warnings and 0 errors. Rationale: The running server's output lock should not be treated as a compile failure or stopped implicitly.
- [Quick 260629-40w]: AC15 mapper pure normalization primitives are centralized in `Ac15MapperNormalization` and referenced with Mapperly direct external `Use = nameof(@Ac15MapperNormalization.X)` mappings. Rationale: Pure one-line helpers should not be repeated across era mappers, while era-limit wrappers stay local where the boundary matters.
- [Quick 260629-5j1]: AC15 generated wire should be regenerated through a tracked `.tools/protogen.exe` entrypoint that includes `+nullablevaluetype=yes`; playresult mapper cleanup should follow generated wire regeneration. Rationale: Current White/Murasaki/Kimidori wire surfaces expose some optional value fields as non-nullable public properties with `ShouldSerialize*`, while direct protogen probes show nullable-value generation removes the mapper pressure without changing required proto fields.
- [Quick 260629-6k9]: AC15 game-wire regeneration is tracked through `tools/generate-ac15-game-wire.ps1`, with reviewed surfaces regenerated using `+nullablevaluetype=yes` and mapper boundaries defaulting absent optional values only where the Application DTO is non-nullable. Rationale: Keeps generated wire source-driven, preserves proto-required fields, and removes ad hoc `ShouldSerialize*` value helpers from playresult mappers.

### Pending Todos

- 0 pending todos.
- Phase 43 is complete. Phase 44 verification and acceptance remains not started.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260628-ama | Check White Taikojuku patch portability for White final and Murasaki variants | 2026-06-27 | eab33d5b | [260628-ama-now-we-have-finished-the-patch-for-white](./quick/260628-ama-now-we-have-finished-the-patch-for-white/) |
| 260629-29p | Add KIMIDORI final support | 2026-06-29 | c873b5bf | [260629-29p-now-let-s-add-support-for-kimidori-final](./quick/260629-29p-now-let-s-add-support-for-kimidori-final/) |
| 260629-40w | Implement AC15 mapper helper normalization extraction | 2026-06-29 | 4c031b09 | [260629-40w-implement-ac15-mapper-helper-normalizati](./quick/260629-40w-implement-ac15-mapper-helper-normalizati/) |
| 260629-5j1 | Review AC15 mapper nullability and playresult helper cleanup | 2026-06-29 | review-only | [260629-5j1-now-perform-a-code-review-on-the-mappers](./quick/260629-5j1-now-perform-a-code-review-on-the-mappers/) |
| 260629-6k9 | Execute AC15 mapper nullability review fix | 2026-06-29 | 579be2ac | [260629-6k9-execute-ac15-mapper-nullability-review-f](./quick/260629-6k9-execute-ac15-mapper-nullability-review-f/) |

### Blockers/Concerns

- Hard guardrail: `Host/wwwroot/data/<era>/data` entries are local operator game-data links. Agents must never delete, move, copy over, clean, or repair them by copying data. If a link is missing or broken, stop and ask; only explicit symlink/junction and `.gitignore` actions are allowed.
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

Last session: 2026-06-29T02:10:15+08:00
Stopped at: Completed quick task 260629-29p
Resume file: None

## Operator Next Steps

- Plan Phase 44 verification and acceptance when ready. Do not treat Phase 43 automated AdminApi/WebUI verification as cabinet/RPCS3 acceptance.
