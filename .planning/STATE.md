---
gsd_state_version: 1.0
milestone: v1.3
milestone_name: Phase Summary
status: executing
stopped_at: Completed 20.1-02-PLAN.md
last_updated: "2026-06-14T08:43:55.535Z"
last_activity: 2026-06-14 -- Phase 20.1 execution started
progress:
  total_phases: 6
  completed_phases: 2
  total_plans: 20
  completed_plans: 10
  percent: 50
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-13)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Blue, Green, Yellow, Nijiiro, or shared identity state.
**Current focus:** Phase 20.1 — ac15-capability-dto-and-mapper-boundary-refactor

## Current Position

Phase: 20.1 (ac15-capability-dto-and-mapper-boundary-refactor) — EXECUTING
Plan: 3 of 9
Status: Ready to execute
Last activity: 2026-06-14 -- Phase 20.1 execution started

## Performance Metrics

**Velocity:**

- Total plans completed: 42 in v1.2
- Average duration: 23.5 min
- Total execution time: 541 min

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 7 | 1/1 complete | 10 min | 10 min |
| 8 | 1/1 complete | 5 min | 5 min |
| 9 | 1/1 complete | 26 min | 26 min |
| 10 | 3/3 complete | 33 min | 11 min |
| 11 | 1/1 complete | 3 min | 3 min |
| 12 | 3/3 complete | 25 min | 8 min |
| 13 | 3/3 complete | 80 min | 27 min |
| 14 | 3/3 complete | 123 min | 41 min |
| 15 | 9/9 complete | 198 min | 22 min |
| 16 | 4/4 complete | 71 min | 18 min |
| 16.1 | 1/1 complete | 95 min | 95 min |
| 16.2 | 14 | - | - |
| 17 | 1/1 complete | closeout | closeout |
| 18 | 4 | - | - |

**Recent Trend:**

- Last 6 implementation plans before closeout: 16.2-08, 16.2-09, 16.2-10, 16.2-11, 16.2-12, and 16.2-13 completed; Phase 17 recorded runtime verification and final contract closeout.
- Trend: Phase 16.1 mapper architecture rewrite and Phase 16.2 shared-core simplification were completed before final Yellow runtime verification and contract closeout.

| Phase 11 P01 | 3 min | 4 tasks | 4 files |
| Phase 12 P01 | 12 min | 2 tasks | 13 files |
| Phase 12 P02 | 9 min | 3 tasks | 7 files |
| Phase 12 P03 | 4 min | 2 tasks | 2 files |
| Phase 13 P01 | 45 min | 2 tasks | 28 files |
| Phase 13 P02 | 15 min | 2 tasks | 10 files |
| Phase 13 P03 | 20 min | 2 tasks | 27 files |
| Phase 14 P01 | 72 min | 2 tasks | 24 files |
| Phase 14 P02 | 27 min | 3 tasks | 12 files |
| Phase 14 P03 | 24 min | 3 tasks | 8 files |
| Phase 15 P01 | 25 min | 2 tasks | 12 files |
| Phase 15 P02 | 29 min | 2 tasks | 6 files |
| Phase 15 P03 | 31 min | 2 tasks | 12 files |
| Phase 15 P04 | 25 min | 2 tasks | 8 files |
| Phase 15 P05 | 16 min | 2 tasks | 6 files |
| Phase 15 P06 | 16 min | 2 tasks | 4 files |
| Phase 15 P07 | 23 min | 2 tasks | 9 files |
| Phase 15 P08 | 18 min | 2 tasks | 6 files |
| Phase 15 P09 | 19 min | 2 tasks | 3 files |
| Phase 16 P01 | 20 min | 2 tasks | 11 files |
| Phase 16 P02 | 19 min | 2 tasks | 3 files |
| Phase 16 P03 | 12 min | 2 tasks | 5 files |
| Phase 16 P04 | 20 min | 2 tasks | 3 files |
| Phase 16.2 P01 | 7 min | 2 tasks | 50 files |
| Phase 16.2 P02 | 13 min | 2 tasks | 35 files |
| Phase 16.2 P04 | 5 min | 1 tasks | 4 files |
| Phase 16.2 P06 | 12 min | 1 tasks | 5 files |
| Phase 16.2 P03 | 10 min | 1 tasks | 7 files |
| Phase 16.2 P05 | 6 min | 1 tasks | 8 files |
| Phase 16.2 P07 | 13 min | 1 tasks | 5 files |
| Phase 16.2 P00 | 16 min | 2 tasks | 31 files |
| Phase 16.2 P08 | 9 min | 2 tasks | 27 files |
| Phase 16.2 P11 | 12 min | 2 tasks | 19 files |
| Phase 16.2 P12 | 7 min | 1 tasks | 1 files |
| Phase 16.2 P13 | 8 min | 1 tasks | 4 files |
| Phase 16.2 P09 | 9 min | 1 tasks | 5 files |
| Phase 16.2 P10 | 22 min | 1 tasks | 4 files |
| Phase 17 P01 | closeout | 4 requirements | planning artifacts |
| Phase 18 P01 | 7 min | 2 tasks | 1 files |
| Phase 18 P02 | 24 min | 1 tasks | 9 files |
| Phase 18 P03 | 8 min | 2 tasks | 4 files |
| Phase 18 P04 | 49 | 3 tasks | 27 files |
| Phase 20.1 rejected P01 | 35 | 3 tasks | 25 files |
| Phase 20.1 rejected P02 | 26 | 3 tasks | 25 files |
| Phase 20.1 rejected P03 | 13 | 3 tasks | 13 files |
| Phase 20.1 P01 | 40 min | 2 tasks | 7 files |
| Phase 20.1 P02 | 24 min | 2 tasks | 9 files |

## Accumulated Context

### Roadmap Evolution

- v1.3 Red AC15 Support roadmap corrected as Phases 18-22 around capability composition: Red evidence/capability foundation, Red capability profile/catalog binding, Red runtime capability binding/simple compatibility, shared older-AC15 ChallengeCompe capability with Red binding, and AdminApi/WebUI/runtime closeout.
- Phase 16.1 inserted after Phase 16 and completed before Phase 17: AC15 Mapperly Mapper Rewrite and Presence Semantics; includes `protogen +nullablevaluetype=yes` wire regeneration as a first-class refactor point.
- Phase 16.2 inserted after Phase 16.1 and before Phase 17: AC15 Shared Core Simplification and Reuse Cleanup (URGENT)
- Phase 17 completed v1.2 Yellow runtime verification and contract closeout from full automated verification plus user-confirmed RPCS3 smoke.
- Phase 20.1 inserted after Phase 20: AC15 Capability DTO and Mapper Boundary Refactor (URGENT)

### Decisions

Decisions are logged in PROJECT.md Key Decisions table. Recent decisions affecting current work:

- v1.1 started at Phase 7 because v1.0 completed Phases 1-6.
- Real Banacoin wallet/payment behavior is out of scope for this repo, not deferred.
- Banacoin compatibility is stateless and permissive only so Tokkun can remain playable.
- Protocol-backed Tokkun tutorial and summary/progress persistence is in scope for v1.1.
- `PlayMode.Tokkun = 3` is confirmed by runtime evidence and used as the Blue Tokkun classifier.
- Cabinet/RPCS3 smoke evidence was required before v1.1 could be called done.
- Phase 7 established the Tokkun evidence contract and removed the stale Tokkun source-scan guard without adding runtime Tokkun behavior.
- Phase 8 added Blue `getbanacoininfo.php` as a stateless direct-protobuf route that returns only `Result = 1`.
- Phase 9 added Blue Tokkun playresult mapper/classifier fields and accepted Tokkun uploads before battle/normal write paths.
- Phase 10 added Blue Tokkun persistence/readback for nullable tutorial state and append-only raw history rows.
- Phase 11 recorded user-confirmed cabinet/RPCS3 runtime verification, full automated test/build evidence, and the final Blue Tokkun contract.
- [Phase 12]: Yellow concrete game routes use the user-approved `/v09r00/chassis` prefix from `12-YELLOW-EVIDENCE.md`.
- [Phase 12]: Phase 12 Yellow controllers are no-state success scaffolds only; runtime persistence, catalog behavior, shop semantics, and battle behavior remain absent.
- [Phase 12]: Yellow battle remains an absence contract: no battle route, BattleUserData surface, Blue battle fields, Yellow battle persistence, or Blue battle fallback.
- [Phase 12]: Yellow startup/version ownership remains shared under `/v01r00/chassis`; Yellow must not duplicate those routes under `/v09r00/chassis`.
- [Phase 13]: Yellow catalog/core support is catalog-only: no Yellow EF entities, migrations, gameplay writes, AdminApi/WebUI, Tokkun, Banacoin wallet/payment, or battle behavior were introduced.
- [Phase 13]: Yellow metadata routes are Mediator/catalog-backed only for the eight Phase 13-owned endpoints; deferred runtime routes remain no-state scaffolds.
- [Phase 14]: Plan 01 added Yellow-owned save, best, play-history, favorite, and recent-song tables plus Mediator-backed Yellow BAID/mydon routes for identity/default save behavior.
- [Phase 14]: Plan 02 added Yellow userdata and self-best readback plus raw Yellow crown field-3 proof/readback from Yellow-owned state.
- [Phase 14]: Plan 03 added Mediator-backed Yellow playresult routing and Yellow-owned normal play persistence that feeds userdata, self-best, and crown readback while leaving Tokkun-shaped uploads as success/no-write for Phase 16.
- [Phase 15]: Plan 01 added Yellow-owned Dan score/stage score tables plus Yellow-specific packed-grade helper rules without Blue/Green Dan table/helper reuse.
- [Phase 15]: Plan 02 added Yellow Dan playresult persistence, Yellow-owned Dan score readback, and Yellow userdata display-Dan normalization from Yellow Dan rows.
- [Phase 15]: Plan 03 added Yellow-owned shop season/item tables plus active-season helpers that seed from Yellow save Don medal totals and ignore Blue/Green shop rows.
- [Phase 15]: Plan 04 replaced Yellow itempurchase scaffolding with a Mediator-backed AC15 purchase flow using Yellow-owned shop state and kept reward routes stateless.
- [Phase 15]: Plan 05 routed Yellow playresult Don medals into active Yellow shop seasons and fed Yellow purchased shop rows into userdata lock readback.
- [Phase 15]: Plan 06 constrained Yellow WaiWai to current protocol evidence, removed unbacked tutorial mutation, and kept stage WaiWai facts diagnostic/play-history-only.
- [Phase 15]: Plan 07 added Yellow AdminApi profile, score, history, and favorite routes over Yellow-owned rows only.
- [Phase 15]: Plan 08 added Yellow AdminApi leaderboard, Dani, game-data, and customization catalog readback through Yellow-owned rows/catalogs only.
- [Phase 15]: Plan 09 added Yellow WebUI era support and route tests proving existing generic pages/services use Yellow AdminApi paths without Yellow-only Tokkun, Banacoin, or shop-management UI.
- [Phase 16]: Plan 01 added Yellow Tokkun mapper contract tests plus a Yellow-owned append-only raw history table/schema proof without handler writes or userdata readback.
- [Phase 16]: Plan 02 replaced the Yellow Tokkun playresult placeholder with an early Yellow-only helper that writes only nullable tutorial state and append-only raw history, with behavior tests for unknown-user no rows, mixed-payload no-cross-write, and tutorial-only non-classification.
- [Phase 16]: Plan 03 enabled Yellow userdata readback for only optional `tokkun_tutorial_flg`, preserving Blue/Green placement behavior and keeping Tokkun history/server-side facts out of userdata.
- [Phase 16]: Plan 04 tightened Yellow Banacoin-adjacent compatibility to full request logging plus stateless success-only routes, with tests proving no wallet/payment/coupon/transaction authority surfaces.
- [Phase 16.2]: Canonicalized only value-identical AC15 Dan clear-grade and shop item-status enum types. — Blue, Green, and Yellow protocol values matched exactly, while EF entities and DbSets remain era-owned.
- [Phase 16.2]: Kept existing era Dan helper and shop adapter behavior in place for later Phase 16.2 extraction plans. — Plan 01 only canonicalizes domain enum values; wider helper, Dani, shop, and playresult extraction remains owned by subsequent scoped plans.
- [Phase 16.2]: Dan id ranges, packed two-bit flags, GotDanMax, display-Dan normalization, and readback filtering now use Ac15DanHelpers with Ac15ProtocolLimits. — Plan 16.2-02 centralized duplicate Blue, Green, and Yellow Dan helper behavior behind profile-driven protocol limits.
- [Phase 16.2]: Blue battle and Green ghost protocol constants stayed era-local while shared Dan/crown behavior moved to Application/Ac15. — The cleanup removed duplicated helper wrappers without widening protocol placement semantics across eras.
- [Phase 16.2]: Duplicate helper-only tests were replaced by canonical AC15 helper behavior tests; era tests continue to verify handler and protocol behavior. — Repository test rules prefer observable behavior and no-cross-era boundaries over implementation-string or wrapper-existence assertions.
- [Phase 16.2]: Shop season seed behavior now uses explicit Ac15ShopSeasonPolicy. — Plan 16.2-04 encoded Blue zero-seed and Green/Yellow first-season save-seed behavior while preserving separate era shop tables and Banacoin non-authority boundaries.
- [Phase 16.2]: Shared Dani behavior now has a contract-first adapter boundary. — Plan 16.2-06 added canonical AC15 Dani records and `IAc15DaniPersistence` without creating a shared Dan EF table.
- [Phase 16.2]: Blue, Green, and Yellow Dani adapters target only typed era-owned Dan DbSets. — The new adapters preserve separate Dan score and stage-score persistence for each era.
- [Phase 16.2]: Known Dan challenge levels are part of the Dani persistence contract. — Later shared readback can preserve the existing era catalog filter, including Yellow filtering, while using canonical records.
- [Phase 16.2]: Profile counter updates now use Ac15ProfileCounterUpdater with explicit typed save-field delegates for Blue, Green, and Yellow. — Plan 16.2-03 centralized duplicated Blue/Green helper and Yellow inline logic while preserving observable counter behavior.
- [Phase 16.2]: Blue, Green, and Yellow profile save rows remain era-owned; the refactor adds no shared EF table, reflection adapter, route, or wire change. — D-20 and D-24 require typed save ownership and forbid broad generic EF adapters or shared gameplay tables.
- [Phase 16.2]: Existing Green helper coverage was moved to canonical AC15 counter updater tests, with additional Blue and Yellow typed-access checks. — Repository test rules prefer observable behavior coverage over source-shape tests for reuse cleanup.
- [Phase 16.2]: Factored AC15 item-shop purchased item plumbing through typed era DbSets. — Plan 16.2-05 shares duplicate lookup, row creation, and save shape while keeping Blue, Green, and Yellow shop item tables and unlock policies explicit.
- [Phase 16.2]: Ac15DaniService now owns common Dan-mode save mutation while Blue, Green, and Yellow handlers retain mode classification and save-field ownership. — Plan 16.2-07 moved duplicated Dan validation, score/stage aggregation, packed flags, GotDanMax, display Dan, and Dan costume decisions into a shared service without moving special-mode gates.
- [Phase 16.2]: Dani save uses typed era adapters plus canonical Ac15DaniChallenge records instead of depending on era catalog classes or shared Dan EF tables. — The service consumes canonical challenge facts and IAc15DaniPersistence, so Blue, Green, and Yellow Dan entity construction stays inside typed adapters.
- [Phase 16.2]: Dan costume application remains an explicit handler-provided save update so era-specific costume flag writes stay typed. — Ac15DaniService emits Ac15DaniSaveUpdate, and each handler applies the update to its own save row and protocol byte helper.
- [Phase 16.2]: Phase 16.2 architecture correction removed AC15 repository-shaped persistence interfaces/adapters and kept ITaikoDbContext as the traceable persistence boundary while Mapperly owns Application/Ac15 projections. — Phase 16.2 architecture correction removed AC15 repository-shaped persistence interfaces/adapters and kept ITaikoDbContext as the traceable persistence boundary while Mapperly owns Application/Ac15 projections.
- [Phase 16.2]: Post-review AC15 storage reuse will use narrow Domain entity-shape interfaces plus generic EF helpers, with Mapperly as the only source-generation layer. — Direct `ITaikoDbContext` remains the persistence boundary; future AC15 eras should implement row-shape contracts instead of copying algorithms or adding repository/adapter wrappers.

- [Phase 16.2]: Plan 08 locked the hybrid AC15 persistence architecture in code. Narrow Domain row-shape interfaces plus generic EF helpers now remove duplicated Dani, normal-play, and item-shop storage algorithms while direct `ITaikoDbContext`/concrete `DbSet` selection and Mapperly projections remain explicit.
- [Phase 16.2]: Plan 11 canonicalized shared AC15 Common DTO fields. Userdata and initial-data shared fields now live on canonical application DTO members, while Blue/Green/Yellow Mapperly mappers keep adapter-local wire placement and Blue battle/userdata era-only fields stay in partials.
- [Phase 16.2]: Plan 12 kept folder/telop route ownership unchanged and collapsed identical Blue/Green/Yellow telop snapshot projection behind a shared helper in `Ac15CatalogSnapshotFactory`.
- [Phase 16.2]: Plan 13 moved shared AC15 initial-data list population into `Ac15InitialDataService`, leaving Blue battle and Green ghost advertisement as explicit era-local handler fields.
- [Phase 16.2]: Plan 09 added `IAc15SongPlayDatum` and shared AC15 normal-play row insertion through a generic helper over concrete era DbSets, while Green `SupportLevel` and ghost section persistence remain era-local.
- [Phase 17]: Yellow runtime closeout accepted user-confirmed RPCS3 smoke evidence, full `dotnet test Tests/Tests.csproj` (683 passed), and temp-output Host build (0 warnings/errors) as the v1.2 completion gate.
- [Phase 17]: Final Yellow contract records `/v09r02/chassis/*` game routes, shared `/v01r00/chassis/*` startup/version ownership, Yellow-owned state, no Yellow battle, and no Banacoin authority.
- [Phase 18]: Plan 18-01 records Red game routes as /v08r01 while shared startup/version remains /v01r00. — IDA-backed route strings prove separate game and startup/version prefixes.
- [Phase 18]: Plan 18-01 records ST8100-1 as the active Red runtime root and ST5100-* / ST7100-1 as inactive or historical. — IDA root strings identify ST8100-1; local data contains older roots that must not be selected by filename guessing.
- [Phase 18]: Plan 18-01 classifies ChallengeCompe as a shared older-AC15 candidate rather than Red-only stateful behavior. — Red route/proto evidence proves a candidate surface, while Phase 21 owns the shared contract and semantics.
- [Phase 18]: Plan 18-01 leaves Red gameplay persistence, EF migrations, Ac15EraProfiles.Red, AdminApi, and WebUI out of scope. — Later phases own runtime state, profile binding, admin, and UI only after evidence-backed contracts exist.
- [Phase 18]: Red is introduced as a first-class adapter and enum identity only. — Host runtime binding, route probes, catalog/profile binding, gameplay state, tests, AdminApi, and WebUI remain deferred to their planned follow-up work.
- [Phase 18]: Generated Red wire uses protogen default package routing for the adapter namespace. — The dumped Red proto inputs have no package declarations; using --package preserved proto immutability while producing TaikoLocalServer.Adapters.GameProtocol.Red.Wire DTOs.

- [Phase 18]: Plan 18-03 wires Red Host support through enabled-era gating only. Red adapter DI registration occurs only when `GameEra.Red` is enabled, and the Red application part is removed when disabled.
- [Phase 18]: Plan 18-03 keeps Red outside AC15 shop validation. Red settings intentionally omit `EnableShop` and `ActiveShopSeasonId`; Green, Blue, and Yellow remain the only eras requiring shop settings.
- [Phase 18]: Plan 18-03 scopes Red direct-protobuf fallback to `/v08r01/chassis`. The fallback does not broaden to all `/v08r01` paths.
- [Phase 19]: Red catalog/profile binding composes shared AC15 loaders, profiles, projection services, and Mapperly route mappers instead of adding Red copies of Green/Blue/Yellow mechanisms.
- [Phase 19]: Red uses active `ST8100-1` game data through `PathHelper`, AC15 catalog sidecars under `Host/wwwroot/data/red`, and `Ac15EraProfiles.Red` with item shop disabled.
- [Phase 19]: Only Red metadata probes for initial data, folders, telops, recommendations, and Taikojuku were made catalog-backed; Red gameplay/profile state remains absent until Phase 20.
- [Phase 20]: Red runtime state is Red-owned and bound through shared AC15 mechanisms for identity, userdata, normal play, self-best, crowns, favorites, recent songs, and Dani.
- [Phase 20]: Red Don points are Red profile fields, not shop medals, shop-season balances, wallet balances, coupons, payments, receipts, or transactions.
- [Phase 20]: Red Tokkun playresults classify before normal/Dani/Challenge handling and write back only nullable `TokkunTutorialFlg`; no raw Tokkun history, reward, unlock, normal-play, or Dani writes are made from Tokkun uploads.
- [Phase 20]: Red ChallengeCompe arrays are mapped as playresult facts for Phase 21, but no ChallengeCompe state or readback contract was implemented.
- [Phase 20.1]: Previous Plans 01-04 are superseded by corrected context because the architecture was rejected. Their verification proves only that behavior did not regress, not that the mapper boundary was accepted.
- [Phase 20.1]: Corrected context requires sectioned BAID/userdata responses, handler-owned semantic sections, controller-owned final assembly, generated Mapperly existing-target apply mappings, and no handwritten response aggregation in mapper classes.
- [Phase 20.1]: Compact design contract is `.planning/phases/20.1-ac15-capability-dto-and-mapper-boundary-refactor/20.1-DESIGN-CONTRACT.md`; controllers control final assembly, handlers return semantic sections, and mappers stay mechanical.

### Pending Todos

None recorded.

### Blockers/Concerns

None recorded.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260613-ny9 | Support older Red `/v08r00` compatibility with old BAID wire shape | 2026-06-13 | d7032433 | [260613-ny9-before-we-continue-to-next-phase-support](./quick/260613-ny9-before-we-continue-to-next-phase-support/) |
| 260609-7gk | Regenerate Yellow final wire support, add final-version field handling, and move Yellow game routes to `/v09r02` | 2026-06-08 | 39e49294 | [260609-7gk-now-let-s-execute-a-quick-task-we-have-c](./quick/260609-7gk-now-let-s-execute-a-quick-task-we-have-c/) |

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Admin/developer UI | TKUI-01: inspect Blue Tokkun history or debug state through AdminApi/WebUI | Future requirement, not mapped to v1.1 roadmap | v1.1 requirements |

## Session Continuity

Last session: 2026-06-14T08:43:55.528Z
Stopped at: Completed 20.1-02-PLAN.md
Resume file: None

## Operator Next Steps

- Continue Phase 20.1 execution with `20.1-03-PLAN.md` (Yellow/Red BAID migration).
- Do not start Phase 21 until Phase 20.1 corrective rewrite has fresh verification and closeout.
