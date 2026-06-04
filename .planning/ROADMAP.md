# Roadmap: TaikoLocalServer

## Milestones

- [x] **v1.0 Blue Support** - Phases 1-6 shipped on 2026-06-03. See [v1.0 roadmap archive](milestones/v1.0-ROADMAP.md), [v1.0 requirements archive](milestones/v1.0-REQUIREMENTS.md), and [v1.0 phase artifacts](milestones/v1.0-phases/).
- [ ] **v1.1 Blue Tokkun Mode Support** - Phases 7-11 active.

## Overview

v1.1 adds evidence-backed Blue Tokkun support after the shipped v1.0 normal and battle work. The milestone keeps Blue Tokkun behavior Blue-owned, accepts and persists only protocol-backed Tokkun facts, keeps Banacoin compatibility stateless and permissive, and finishes only when automated verification and cabinet/RPCS3 smoke evidence agree.

Real Banacoin wallet, payment, balance, coupon, receipt, deduction, BNID result, or transaction-history behavior is out of scope for this repo. It is not planned as a future phase in this roadmap.

## Phases

**Phase Numbering:**

- Phases 1-6 are historical v1.0 work and remain archived.
- v1.1 continues numbering from Phase 7.
- Decimal phases are reserved for urgent insertions after planning.

- [x] **Phase 7: Tokkun Evidence Contract and Guardrail Reset** - Establish evidence-tagged Tokkun protocol boundaries and replace stale no-Tokkun guards. (completed 2026-06-03)
- [x] **Phase 8: Stateless Banacoin Compatibility and Availability** - Keep required Banacoin-adjacent endpoints permissive and stateless so Tokkun entry is not blocked. (completed 2026-06-04)
- [ ] **Phase 9: Tokkun Mapper and Safe Playresult Acceptance** - Accept and classify Tokkun playresults safely without gameplay-state contamination.
- [ ] **Phase 10: Evidence-Backed Tokkun State Persistence and Readback** - Persist and read back protocol-backed Blue Tokkun tutorial and summary facts.
- [ ] **Phase 11: Cabinet/RPCS3 Smoke and Contract Tightening** - Prove Tokkun on cabinet/RPCS3 and lock tests/docs around the final contract.

## Phase Details

### Phase 7: Tokkun Evidence Contract and Guardrail Reset

**Goal**: Operator/developer has a bounded Blue Tokkun protocol contract before runtime behavior changes.
**Depends on**: Phase 6 / v1.0 Blue Support shipped
**Requirements**: TKEV-01, TKEV-02, TKEV-03
**Success Criteria** (what must be TRUE):

  1. Operator/developer can inspect an evidence-tagged Blue Tokkun contract that separates proven, observed, deliberately ignored, and unknown protocol fields.
  2. Blue Tokkun classification policy is documented without assigning a guessed numeric `PlayMode.Tokkun` value.
  3. Source guardrails allow only named Blue Tokkun support paths while still blocking invented reward, score, Banacoin, battle, and normal-progression semantics.
  4. Follow-on phase planners can see the accepted classifier inputs, logging boundaries, persistence boundaries, and route-surface unknowns.

**Plans**: 1 plan
Plans:

- [x] 07-01-PLAN.md - Create the Tokkun evidence contract and remove the stale Tokkun source guard

### Phase 8: Stateless Banacoin Compatibility and Availability

**Goal**: Blue Tokkun entry is not blocked by Banacoin-adjacent endpoints, and no Banacoin state is stored.
**Depends on**: Phase 7
**Requirements**: TKBC-01, TKBC-02, TKBC-03
**Success Criteria** (what must be TRUE):

  1. Cabinet/RPCS3 Tokkun entry receives permissive success-shaped responses from every proven required Banacoin-adjacent endpoint.
  2. Repeated Banacoin-adjacent requests leave no Blue, Green, Nijiiro, or shared wallet-like balance, payment, coupon, deduction, `chid`, BNID, or transaction state behind.
  3. `getbanacoininfo.php` exists only if cabinet/RPCS3 logs or IDA route evidence proves Blue Tokkun calls it and current absence blocks play.
  4. Operator/developer can inspect logs that show the Banacoin request sequence without any real payment model side effects.

**Plans**: TBD

### Phase 9: Tokkun Mapper and Safe Playresult Acceptance

**Goal**: Blue Tokkun playresult uploads are accepted from proven Tokkun fields without contaminating existing Blue gameplay state.
**Depends on**: Phase 8
**Requirements**: TKPR-01, TKPR-02, TKPR-03
**Success Criteria** (what must be TRUE):

  1. Blue `playresult.php` returns success for Tokkun uploads classified from proven Tokkun fields such as `ary_tokkunstage_info` and `tokkun_tutorial_flg`.
  2. Tokkun uploads do not write normal score, crown, Dani, battle, favorite, recent-song, profile, unlock, medal, customization, title, or shop state.
  3. Unknown or mixed Tokkun-shaped uploads are logged with bounded request context and still return success unless concrete client evidence proves a failure response is required.
  4. Mapper DTOs preserve protocol-backed Tokkun fields and optional-field presence for downstream Tokkun handling.

**Plans**: 1 plan
Plans:

- [ ] 09-01-PLAN.md - Preserve Tokkun mapper facts and accept Tokkun playresults without state contamination

### Phase 10: Evidence-Backed Tokkun State Persistence and Readback

**Goal**: Blue stores and reads back protocol-backed Tokkun tutorial and summary state without inventing progression, reward, or payment semantics.
**Depends on**: Phase 9
**Requirements**: TKST-01, TKST-02, TKST-03, TKST-04
**Success Criteria** (what must be TRUE):

  1. Blue Tokkun tutorial state from `tokkun_tutorial_flg` is persisted in Blue-owned state and read back through Blue userdata according to the Tokkun protocol contract.
  2. Blue Tokkun summary/progress facts from the payload are stored as Blue-owned Tokkun records, including `banacoin_datetime`, song count/list, speed-change count, autoplay count, jump count, and upload time.
  3. Tokkun persistence remains separate from Green, Nijiiro, normal score, Dani, battle, item shop, and Banacoin storage.
  4. Stored Tokkun records contain raw/protocol-backed Tokkun facts only and do not infer rankings, rewards, score progression, payment history, practice-time rules, or unlocks.

**Plans**: TBD

### Phase 11: Cabinet/RPCS3 Smoke and Contract Tightening

**Goal**: Blue Tokkun support is proven end to end and documented with final verified boundaries.
**Depends on**: Phase 10
**Requirements**: TKVF-01, TKVF-02, TKVF-03
**Success Criteria** (what must be TRUE):

  1. Automated tests cover Tokkun classification, mapper fields, safe playresult acceptance, no-cross-write behavior, Banacoin statelessness, Tokkun persistence, and replacement source guards.
  2. Cabinet/RPCS3 smoke evidence covers Tokkun selection, Banacoin request sequence, gameplay entry, final upload, post-upload userdata behavior, and unexpected endpoint calls.
  3. Final docs record confirmed Tokkun constants/routes, persisted Tokkun fields, unresolved research flags, and the boundary between supported Tokkun behavior and out-of-scope Banacoin/payment semantics.
  4. Done remains blocked until automated tests and cabinet/RPCS3 evidence confirm Tokkun does not contaminate normal, battle, Dani, profile, favorite, recent-song, unlock, shop, or Banacoin state.

**Plans**: TBD

## Requirement Coverage

| Requirement | Phase |
|-------------|-------|
| TKEV-01 | Phase 7 |
| TKEV-02 | Phase 7 |
| TKEV-03 | Phase 7 |
| TKBC-01 | Phase 8 |
| TKBC-02 | Phase 8 |
| TKBC-03 | Phase 8 |
| TKPR-01 | Phase 9 |
| TKPR-02 | Phase 9 |
| TKPR-03 | Phase 9 |
| TKST-01 | Phase 10 |
| TKST-02 | Phase 10 |
| TKST-03 | Phase 10 |
| TKST-04 | Phase 10 |
| TKVF-01 | Phase 11 |
| TKVF-02 | Phase 11 |
| TKVF-03 | Phase 11 |

**Coverage:** 16/16 v1.1 requirements mapped. No orphaned requirements. Future requirement TKUI-01 is intentionally not mapped.

## Progress

**Execution Order:**
Phases execute in numeric order: 7 -> 8 -> 9 -> 10 -> 11.

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 7. Tokkun Evidence Contract and Guardrail Reset | v1.1 | 1/1 | Complete    | 2026-06-03 |
| 8. Stateless Banacoin Compatibility and Availability | v1.1 | 1/1 | Complete    | 2026-06-04 |
| 9. Tokkun Mapper and Safe Playresult Acceptance | v1.1 | 0/1 | Not started | - |
| 10. Evidence-Backed Tokkun State Persistence and Readback | v1.1 | 0/TBD | Not started | - |
| 11. Cabinet/RPCS3 Smoke and Contract Tightening | v1.1 | 0/TBD | Not started | - |
