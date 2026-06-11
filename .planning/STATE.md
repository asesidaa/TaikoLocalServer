---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: Phase Summary
status: executing
stopped_at: Completed 16.2-11-PLAN.md
last_updated: "2026-06-11T05:48:09.340Z"
last_activity: 2026-06-11
progress:
  total_phases: 8
  completed_phases: 6
  total_plans: 37
  completed_plans: 33
  percent: 89
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-07)

**Core value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Blue, Green, Yellow, Nijiiro, or shared identity state.
**Current focus:** Phase 16.2 — ac15-shared-core-simplification-and-reuse-cleanup

## Current Position

Phase: 16.2 (ac15-shared-core-simplification-and-reuse-cleanup) — EXECUTING
Plan: 11 of 14
Status: Ready to execute
Last activity: 2026-06-11

## Performance Metrics

**Velocity:**

- Total plans completed: 23 in v1.2
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

**Recent Trend:**

- Last 6 plans: 15-09 completed in 19 min; 16-01 completed in 20 min; 16-02 completed in 19 min; 16-03 completed in 12 min; 16-04 completed in 20 min; 16.1-01 completed in 95 min.
- Trend: Phase 16.1 mapper architecture rewrite completed after review feedback; Phase 16.2 shared-core simplification is now planned in 13 scoped plans before Phase 17 runtime verification and contract closeout.

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

## Accumulated Context

### Roadmap Evolution

- Phase 16.1 inserted after Phase 16 and completed before Phase 17: AC15 Mapperly Mapper Rewrite and Presence Semantics; includes `protogen +nullablevaluetype=yes` wire regeneration as a first-class refactor point.
- Phase 16.2 inserted after Phase 16.1 and before Phase 17: AC15 Shared Core Simplification and Reuse Cleanup (URGENT)

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

### Pending Todos

None recorded.

### Blockers/Concerns

None recorded.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260609-7gk | Regenerate Yellow final wire support, add final-version field handling, and move Yellow game routes to `/v09r02` | 2026-06-08 | 39e49294 | [260609-7gk-now-let-s-execute-a-quick-task-we-have-c](./quick/260609-7gk-now-let-s-execute-a-quick-task-we-have-c/) |

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Admin/developer UI | TKUI-01: inspect Blue Tokkun history or debug state through AdminApi/WebUI | Future requirement, not mapped to v1.1 roadmap | v1.1 requirements |
| Runtime verification | Yellow normal and Tokkun RPCS3/cabinet smoke | Deferred to Phase 17/end-of-range after Phase 16.1 mapper rewrite and Phase 16.2 shared-core simplification | Phase 13 verification |

## Session Continuity

Last session: 2026-06-11T05:48:09.340Z
Stopped at: Completed 16.2-11-PLAN.md
Resume file: None

## Operator Next Steps

- Phase 16.1 verification is complete and passed; Green/Blue/Yellow AC15 wire DTOs now use nullable optional primitives and protocol mappers use real Mapperly generation for mechanical projection.
- Next coordinator-owned step: execute remaining Phase 16.2 plans starting with `16.2-12-PLAN.md`, `16.2-13-PLAN.md`, then `16.2-09-PLAN.md` and `16.2-10-PLAN.md`; do not start Phase 17 runtime verification closeout until Phase 16.2 is executed or explicitly skipped.
