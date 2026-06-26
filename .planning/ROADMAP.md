# Roadmap: TaikoLocalServer

## Milestones

- [x] **v1.0 Blue Support** - Phases 1-6 shipped on 2026-06-03. See [v1.0 roadmap archive](milestones/v1.0-ROADMAP.md), [v1.0 requirements archive](milestones/v1.0-REQUIREMENTS.md), and [v1.0 phase artifacts](milestones/v1.0-phases/).
- [x] **v1.1 Blue Tokkun Mode Support** - Phases 7-11 shipped on 2026-06-07. See [v1.1 roadmap archive](milestones/v1.1-ROADMAP.md), [v1.1 requirements archive](milestones/v1.1-REQUIREMENTS.md), and [v1.1 phase artifacts](milestones/v1.1-phases/).
- [x] **v1.2 Yellow AC15 Support** - Phases 12-17 plus inserted Phases 16.1 and 16.2 shipped on 2026-06-12. See [v1.2 roadmap archive](milestones/v1.2-ROADMAP.md), [v1.2 requirements archive](milestones/v1.2-REQUIREMENTS.md), and [v1.2 phase artifacts](milestones/v1.2-phases/).
- [x] **v1.3 Red AC15 Support** - Phases 18-22 shipped on 2026-06-16. See [v1.3 roadmap archive](milestones/v1.3-ROADMAP.md), [v1.3 requirements archive](milestones/v1.3-REQUIREMENTS.md), and [v1.3 phase artifacts](milestones/v1.3-phases/).
- [x] **v1.4 White AC15 0.13 Support** - Phases 23-27 plus inserted Phase 23.1 shipped on 2026-06-21. See [v1.4 roadmap archive](milestones/v1.4-ROADMAP.md), [v1.4 requirements archive](milestones/v1.4-REQUIREMENTS.md), and [v1.4 phase artifacts](milestones/v1.4-phases/).
- [x] **v1.5 Murasaki AC15 Support** - Phases 28-34 plus quick final `/v06r01` route support shipped on 2026-06-23. See [v1.5 roadmap archive](milestones/v1.5-ROADMAP.md), [v1.5 requirements archive](milestones/v1.5-REQUIREMENTS.md), [v1.5 milestone audit](milestones/v1.5-MILESTONE-AUDIT.md), and [v1.5 phase artifacts](milestones/v1.5-phases/).
- [x] **v1.6 KIMIDORI AC15 Support** - Phases 35-38 shipped on 2026-06-25. See [v1.6 roadmap archive](milestones/v1.6-ROADMAP.md), [v1.6 requirements archive](milestones/v1.6-REQUIREMENTS.md), and [v1.6 phase artifacts](milestones/v1.6-phases/).
- [ ] **v1.7 MOMOIRO AC15 0.11 Support** - Phases 39-44 are active planning scope.

## Current Planning State

v1.7 MOMOIRO AC15 0.11 Support is active. The milestone adds MOMOIRO as a first-class older AC15 era through the binary-proven `/v04r00/chassis/*.php` game route set, shared `/v01r00/chassis/*.php` startup/version routing, root-level catalog binding, MOMOIRO-owned persistence, AdminApi/WebUI readback, and cabinet/RPCS3 acceptance.

**Granularity:** standard
**Requirement coverage:** 24/24 active v1.7 requirements mapped
**Future requirements:** MOLATER-01, MOSPEC-01, and MOSPEC-02 remain deferred.

## Phases

**Phase Numbering:** Continuous from prior milestones. v1.6 ended at Phase 38; v1.7 starts at Phase 39.

- [x] **Phase 39: MOMOIRO Evidence and Era Foundation** - Record the exact route inventory and add evidence-gated first-class MOMOIRO scaffolding. (completed 2026-06-26)
- [x] **Phase 40: MOMOIRO Protocol Limits, Root Catalog, and Route Behavior** - Bind root-level catalog data, protocol limits, and binary-proven route behavior before runtime mutation. (completed 2026-06-26)
- [ ] **Phase 41: MOMOIRO Identity, Userdata, Self-Best, and Crown Readback** - Read MOMOIRO-owned identity, profile, score, recent/favorite, release, hash, and crown state.
- [ ] **Phase 42: MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility** - Mutate only evidence-backed MOMOIRO-owned normal-play, unlock, reward, Dan, and challenge-compatible state.
- [ ] **Phase 43: MOMOIRO AdminApi and WebUI Routing** - Expose implemented MOMOIRO-owned state through admin and browser surfaces while hiding unsupported controls.
- [ ] **Phase 44: MOMOIRO Verification and Acceptance** - Record automated verification, generated-source/build evidence, and repeatable cabinet/RPCS3 acceptance.

## Phase Details

### Phase 39: MOMOIRO Evidence and Era Foundation

**Goal**: MOMOIRO exists as an enabled, evidence-gated first-class era with the supplied binary route inventory recorded before runtime state behavior is claimed.
**Depends on**: Phase 38
**Requirements**: MOFND-01, MOFND-02, MOFND-03, MOFND-04
**Success Criteria** (what must be TRUE):

  1. A developer can inspect the MOMOIRO evidence matrix and see shared `/v01r00/chassis/startupauth.php`, `/verupauth.php`, and `/verupcomplete.php`, plus `/v04r00/chassis/playresult.php`, `/baidcheck.php`, `/mydonentry.php`, `/userdata.php`, `/recommend.php`, `/selfbest.php`, `/heartbeat.php`, `/defaultsong.php`, `/bookkeeping.php`, `/songhash.php`, `/telopcheck.php`, and `/gettelop.php`.
  2. When MOMOIRO is enabled, binary-proven game routes are registered under `/v04r00/chassis/*.php` while shared startup/version routes remain under `/v01r00/chassis/*.php`.
  3. When MOMOIRO is disabled, MOMOIRO game routes are not exposed and other era routes continue to behave as before.
  4. Proto-only MOMOIRO route families outside the binary route list, including `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, and `mainichisong.php`, remain absent instead of becoming route stubs.

**Plans**: 4/4 plans complete
Plans:
**Wave 1**

- [x] 39-01-PLAN.md - Evidence matrix and Wave 0 validation contracts
- [x] 39-02-PLAN.md - First-class Momoiro adapter identity and generated wire

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 39-03-PLAN.md - Host settings, DI, direct-protobuf fallback, and application-part gating

**Wave 3** *(blocked on Wave 2 completion)*

- [x] 39-04-PLAN.md - Evidence-gated Momoiro game route surface and build gates

### Phase 40: MOMOIRO Protocol Limits, Root Catalog, and Route Behavior

**Goal**: MOMOIRO catalog data, protocol limits, and binary-proven route behavior are explicit, evidence-backed inputs for later readback and mutation.
**Depends on**: Phase 39
**Requirements**: MOCAT-01, MOCAT-02, MOCAT-03, MOCAT-04, MOCAT-05
**Success Criteria** (what must be TRUE):

  1. MOMOIRO catalog loading succeeds from `Host/wwwroot/data/momoiro/data` using root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
  2. Runtime code resolves MOMOIRO data through era path/settings abstractions instead of handler-local hardcoded filesystem paths.
  3. MOMOIRO has an explicit profile for byte widths, song ordering, favorite/recent limits, default-song flags, song hash, release flags, crown placement, Don Point/reward limits, and absent feature flags.
  4. `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php` are feature-complete for their binary-proven MOMOIRO route role, while `heartbeat.php` and `bookkeeping.php` are explicit static-result operational stubs if no stateful role is proven.
  5. Crown support is modeled as userdata-owned `hash_crown_flg`; no standalone `crownsdata.php` route is part of active MOMOIRO scope.

**Plans**: 5 plans
Plans:
**Wave 0**

- [x] 40-01-PLAN.md - Wave 0 catalog/profile/metadata route validation contracts

**Wave 1** *(blocked on Wave 0 completion)*

- [x] 40-02-PLAN.md - Momoiro root catalog, profile limits, and snapshot projection

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 40-03-PLAN.md - Momoiro Application metadata handler dispatch

**Wave 3** *(blocked on Wave 2 completion)*

- [x] 40-04-PLAN.md - Momoiro catalog-backed metadata controllers and Mapperly mappers

**Wave 4** *(blocked on Wave 3 completion)*

- [x] 40-05-PLAN.md - Final Phase 40 verification and source-audit gates

### Phase 41: MOMOIRO Identity, Userdata, Self-Best, and Crown Readback

**Goal**: MOMOIRO users can log in and read back MOMOIRO-owned profile, score, release, hash, favorite/recent, and crown state.
**Depends on**: Phase 40
**Requirements**: MORDB-01, MORDB-02, MORDB-03, MORDB-04, MORDB-05
**Success Criteria** (what must be TRUE):

  1. A MOMOIRO card can register, log in, enter MyDon/profile flow, and read userdata through MOMOIRO-owned save state while sharing only true identity data.
  2. `userdata.php` and `selfbest.php` read back MOMOIRO-owned score state with era-correct normal, ura, and shin handling where proven.
  3. Favorite and recent song readback follows binary/client-backed MOMOIRO limits, ordering, truncation, and duplicate behavior.
  4. Crown bytes in `userdata.php` reflect proven MOMOIRO packing, song count, difficulty placement, and default behavior through `hash_crown_flg`.
  5. Release-song and song-hash readback follow MOMOIRO catalog order and proven `song_hash_ver`, `song_hash_tbl`, and `hash_release_song_flg` semantics.

**Plans**: 2/5 plans executed
Plans:
**Wave 0**

- [x] 41-01-PLAN.md - Wave 0 readback tests and validation ownership

**Wave 1** *(blocked on Wave 0 completion)*

- [x] 41-02-PLAN.md - Momoiro readback persistence and EF migration

**Wave 2** *(blocked on Wave 1 completion)*

- [ ] 41-03-PLAN.md - Momoiro Application readback handlers and crown builder

**Wave 3** *(blocked on Wave 2 completion)*

- [ ] 41-04-PLAN.md - Momoiro readback controllers and Mapperly mappers

**Wave 4** *(blocked on Wave 3 completion)*

- [ ] 41-05-PLAN.md - Final Phase 41 verification and source audit

### Phase 42: MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility

**Goal**: MOMOIRO normal play can persist and read back only evidence-backed MOMOIRO-owned gameplay mutations.
**Depends on**: Phase 41
**Requirements**: MORUN-01, MORUN-02, MORUN-03, MORUN-04, MORUN-05
**Success Criteria** (what must be TRUE):

  1. MOMOIRO normal playresults update MOMOIRO-owned scores, self-best, crowns, profile counters, recent songs, favorites, and related normal-play state that can be read back through cabinet routes.
  2. Song unlock, Don Point, and reward behavior mutates only evidence-backed MOMOIRO fields and does not create `shoppingresult.php`, newer item-shop, wallet, payment, or shop-season authority.
  3. MOMOIRO Dan/Dani fields persist and read back only where playresult, userdata, and binary/client evidence prove the normal Dan contract; Taikojuku practice-folder behavior remains absent.
  4. Challenge-shaped arrays are accepted, stored, echoed, or omitted only according to MOMOIRO evidence and do not imply Don Challenge, ChallengeCompe, or reward-management behavior.
  5. MOMOIRO runtime writes do not touch KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, Tokkun, battle, Banacoin, or unsupported feature state.

**Plans**: TBD

### Phase 43: MOMOIRO AdminApi and WebUI Routing

**Goal**: Admin users can inspect and edit implemented MOMOIRO-owned state without exposing unsupported MOMOIRO features.
**Depends on**: Phase 42
**Requirements**: MOADMIN-01, MOADMIN-02
**Success Criteria** (what must be TRUE):

  1. AdminApi exposes MOMOIRO-owned implemented state through `/api/momoiro/...` and era-routed contracts without reading or writing another era's gameplay state.
  2. The WebUI lets an admin select MOMOIRO and use read/edit surfaces only for implemented MOMOIRO-owned state.
  3. Unsupported MOMOIRO controls for Taikojuku, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, event folders, newer item-shop authority, and proto-only route families are not shown as active features.
  4. AdminApi and WebUI edits round-trip through the same MOMOIRO-owned state that cabinet readback consumes.

**Plans**: TBD
**UI hint**: yes

### Phase 44: MOMOIRO Verification and Acceptance

**Goal**: MOMOIRO support is verified through automated checks and repeatable cabinet/RPCS3 acceptance before full support is claimed.
**Depends on**: Phase 43
**Requirements**: MOVFY-01, MOVFY-02, MOVFY-03
**Success Criteria** (what must be TRUE):

  1. Automated verification covers MOMOIRO route ownership, generated mapper behavior, root-level catalog parsing, persistence/no-cross-era boundaries, byte packing, AdminApi/WebUI readback, and unsupported route/state absence.
  2. Mapperly generated-source inspection and a full build, including a temp-output Host build when needed, are recorded before closeout.
  3. Repeatable cabinet/RPCS3 evidence covers supported MOMOIRO startup, login, userdata, catalog/readback, playresult, self-best/crown, AdminApi, and WebUI flows.
  4. Closeout records the difference between automated verification, user-observed runtime acceptance, and future/deferred MOMOIRO feature scope.

**Plans**: TBD
**UI hint**: yes

## Progress

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 35. KIMIDORI Evidence and Era Foundation | v1.6 | 1/1 | Complete | 2026-06-23 |
| 36. Root-Level Catalog and Metadata Binding | v1.6 | 1/1 | Complete | 2026-06-23 |
| 37. KIMIDORI Runtime State, Dani Dojo, and Normal Play | v1.6 | 1/1 | Complete | 2026-06-23 |
| 38. AdminApi, WebUI, and Runtime Closeout | v1.6 | 1/1 | Complete | 2026-06-25 |
| 39. MOMOIRO Evidence and Era Foundation | v1.7 | 4/4 | Complete    | 2026-06-26 |
| 40. MOMOIRO Protocol Limits, Root Catalog, and Route Behavior | v1.7 | 5/5 | Complete    | 2026-06-26 |
| 41. MOMOIRO Identity, Userdata, Self-Best, and Crown Readback | v1.7 | 2/5 | In Progress|  |
| 42. MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility | v1.7 | 0/TBD | Not started | - |
| 43. MOMOIRO AdminApi and WebUI Routing | v1.7 | 0/TBD | Not started | - |
| 44. MOMOIRO Verification and Acceptance | v1.7 | 0/TBD | Not started | - |

## Milestone Progress

| Milestone | Phases | Plans | Status | Shipped |
|-----------|--------|-------|--------|---------|
| v1.0 Blue Support | 1-6 | 30 roadmap plans | Shipped | 2026-06-03 |
| v1.1 Blue Tokkun Mode Support | 7-11 | 7 GSD plans | Shipped | 2026-06-07 |
| v1.2 Yellow AC15 Support | 12-17 plus 16.1 and 16.2 | 38 GSD plans | Shipped | 2026-06-12 |
| v1.3 Red AC15 Support | 18-22 | 29 GSD plans | Shipped | 2026-06-16 |
| v1.4 White AC15 0.13 Support | 23-27 plus 23.1 | 10 GSD plans | Shipped | 2026-06-21 |
| v1.5 Murasaki AC15 Support | 28-34 plus quick 260623-2ff | 7 GSD plans plus final-route quick task | Shipped | 2026-06-23 |
| v1.6 KIMIDORI AC15 Support | 35-38 | 4/4 GSD plans | Shipped | 2026-06-25 |
| v1.7 MOMOIRO AC15 0.11 Support | 39-44 | TBD | Planning | - |
