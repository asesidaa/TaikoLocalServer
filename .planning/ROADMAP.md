# Roadmap: TaikoLocalServer Blue Support

## Overview

This roadmap continues Blue support after completed Superpowers stages A0-A5. The remaining work finishes normal Blue support first, proves it with cabinet/RPCS3 evidence, then moves into strict battle evidence and battle runtime implementation. The final phase hardens full Blue support and records repeatable evidence for normal and battle flows.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: Blue A6 Item Shop And Unlocking** - Implement Blue-owned shop advertisement, purchase, medal, rewardexecution no-op, and unlock behavior.
- [ ] **Phase 2: Blue A7 AdminApi And WebUI Parity** - Expose supported Blue data through AdminApi and WebUI without Green/Nijiiro state leakage.
- [ ] **Phase 3: Blue A8 Normal-Mode Cabinet Smoke And Hardening** - Prove normal Blue support on cabinet/RPCS3 and harden unresolved normal-mode gaps.
- [ ] **Phase 4: Blue Battle Evidence And Design** - Gather strict battle evidence and write the implementation design before battle runtime code.
- [ ] **Phase 5: Blue Battle Runtime Support** - Implement Blue-owned battle persistence, protocol behavior, playresult handling, rewards, and tests.
- [ ] **Phase 6: Full Blue Verification And Release Hardening** - Prove full Blue support with repeatable normal and battle smoke evidence, docs, and final guardrails.

## Phase Details

### Phase 1: Blue A6 Item Shop And Unlocking
**Goal**: Blue item shop uses Blue catalog data, Blue season-scoped medal state, and Blue save-state unlocks for shop advertisement, purchase, Phase 1 rewardexecution no-op handling, and userdata locking.
**Depends on**: Completed prior Blue A0-A5 work
**Requirements**: [SHOP-01, SHOP-02, SHOP-03, SHOP-04, SHOP-05, SHOP-06, SHOP-07, SHOP-08]
**Canonical refs**:
- `.planning/research/ARCHITECTURE.md`
- `.planning/research/PITFALLS.md`
- `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`
- `docs/superpowers/specs/2026-05-26-green-item-shop-support-design.md`
- `Application/Handlers/ItemPurchaseCommand.Green.cs`
- `Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs`
**Success Criteria** (what must be TRUE):
  1. Blue shop endpoints advertise and return only the configured active Blue shop season.
  2. Blue purchase updates Blue-owned medal, shop, and unlock state, while `rewardexecution.php` remains success no-op for Phase 1 per D-02.
  3. Blue userdata hides configured locked shop items until purchased.
  4. Tests and source guards prove Blue item shop does not use Green shop state or Green protocol constants.
**Plans**: 6 plans

Plans:
- [ ] 01-01-PLAN.md - Parser-proven Blue shop default data
- [ ] 01-02-PLAN.md - Blue shop persistence and state helpers
- [ ] 01-03-PLAN.md - Blue shop advertisement and protocol mappers
- [ ] 01-04-PLAN.md - Blue purchase, rewardexecution no-op, and playresult medals
- [ ] 01-05-PLAN.md - Blue BAID and userdata locking
- [ ] 01-06-PLAN.md - Blue item-shop verification, docs, and source guards

### Phase 2: Blue A7 AdminApi And WebUI Parity
**Goal**: Blue users can be inspected and managed through the same basic AdminApi/WebUI surfaces as Green where Blue data exists, while unsafe write/edit surfaces stay hidden or read-only.
**Depends on**: Phase 1
**Requirements**: [WEB-01, WEB-02, WEB-03, WEB-04, WEB-05, WEB-06]
**Canonical refs**:
- `.planning/research/FEATURES.md`
- `docs/superpowers/specs/2026-05-16-green-webui-support-design.md`
- `Adapters.AdminApi/Controllers/`
- `Contracts.AdminApi/`
- `TaikoWebUI/Utilities/WebUiEra.cs`
- `TaikoWebUI/Pages/`
**Success Criteria** (what must be TRUE):
  1. AdminApi returns Blue profile, score/history, favorites, Dani, customization, and item-shop-relevant readback where supported.
  2. WebUI can navigate Blue era routes and construct Blue AdminApi URLs without fallback to another era.
  3. Unsupported Blue edit surfaces are hidden or read-only instead of writing unproven state.
  4. Tests prove Blue WebUI/AdminApi readback does not query or mutate Green state.
**Plans**: 3 plans

Plans:
- [ ] 02-01: Blue AdminApi projection coverage
- [ ] 02-02: Blue WebUI routing and readback surfaces
- [ ] 02-03: WebUI/AdminApi regression tests and unsafe-edit guardrails

### Phase 3: Blue A8 Normal-Mode Cabinet Smoke And Hardening
**Goal**: Normal Blue support is proven on cabinet/RPCS3, with smoke evidence and hardening for boot, registration, login, normal play, readback, Dani, item shop, rewards, and WebUI.
**Depends on**: Phase 2
**Requirements**: [SMOKE-01, SMOKE-02, SMOKE-03, SMOKE-04, SMOKE-05]
**Canonical refs**:
- `.planning/research/SUMMARY.md`
- `Host/README.md`
- `README.md`
- `Tests/Blue/`
- `Host/Program.cs`
**Success Criteria** (what must be TRUE):
  1. A normal-mode Blue smoke checklist exists and covers every Track A user-visible flow.
  2. Cabinet/RPCS3 evidence records pass/fail results for boot, card, play, readback, Dani, item shop, rewards, and WebUI.
  3. Unexpected Blue endpoint calls are bounded in logs and routed to Track B, a follow-up, or an explicit non-goal.
  4. Full server tests and a temp-output Host build pass after hardening.
**Plans**: 3 plans

Plans:
- [ ] 03-01: Normal-mode smoke checklist and log capture setup
- [ ] 03-02: Cabinet/RPCS3 smoke execution and hardening fixes
- [ ] 03-03: Normal-mode regression verification and unresolved-item triage

### Phase 4: Blue Battle Evidence And Design
**Goal**: Battle runtime implementation is unblocked by concrete evidence for battle routes, data files, byte widths, default state, playresult effects, and safe client behavior.
**Depends on**: Phase 3
**Requirements**: [BTEV-01, BTEV-02, BTEV-03, BTEV-04, BTEV-05, BTEV-06]
**Canonical refs**:
- `.planning/research/PITFALLS.md`
- `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`
- `proto/blue/taiko.proto`
- `Adapters.GameProtocol.Blue/Wire/Game.cs`
- `Host/wwwroot/data/blue/data/config/S10100-1/battle`
**Success Criteria** (what must be TRUE):
  1. Battle menu entry and attempted battle flow have captured cabinet/RPCS3 or equivalent client evidence.
  2. Blue battle proto/wire fields and local battle files are mapped to an approved design.
  3. Required byte widths, defaults, repeated rows, and battle playresult effects are documented from evidence.
  4. Battle implementation remains blocked until the evidence checklist is satisfied or explicitly revised.
**Plans**: 3 plans

Plans:
- [ ] 04-01: Battle endpoint, proto, and cabinet-log evidence capture
- [ ] 04-02: Battle data-file inventory and byte/default-state analysis
- [ ] 04-03: Blue battle design spec and implementation gate review

### Phase 5: Blue Battle Runtime Support
**Goal**: Blue battle mode has Blue-owned persistence and protocol behavior for battle userdata, initial data, battle playresult, progression, unlocks, rewards, and readback.
**Depends on**: Phase 4
**Requirements**: [BTL-01, BTL-02, BTL-03, BTL-04, BTL-05, BTL-06]
**Canonical refs**:
- `.planning/research/ARCHITECTURE.md`
- `proto/blue/taiko.proto`
- `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs`
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
**Success Criteria** (what must be TRUE):
  1. Blue battle state is persisted in Blue-owned tables with evidence-backed defaults.
  2. `battleuserdata.php` and `initialdatacheck.php` return safe persisted battle state.
  3. Blue battle playresults and rewards update only the state approved by the battle design.
  4. Tests and source guards prove battle code does not treat Green AI Battle as protocol truth.
**Plans**: 4 plans

Plans:
- [ ] 05-01: Blue battle persistence and catalog/data foundations
- [ ] 05-02: Battle userdata and initial-data protocol behavior
- [ ] 05-03: Battle playresult, progression, rewards, and unlocks
- [ ] 05-04: Battle regression tests, source guards, and verification

### Phase 6: Full Blue Verification And Release Hardening
**Goal**: Full Blue support is repeatably proven for normal and battle flows, documented for operators, and guarded against cross-era regressions.
**Depends on**: Phase 5
**Requirements**: [FULL-01, FULL-02, FULL-03, FULL-04, FULL-05]
**Canonical refs**:
- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/research/SUMMARY.md`
- `README.md`
- `Host/README.md`
- `Tests/Blue/`
**Success Criteria** (what must be TRUE):
  1. Normal and battle Blue smoke evidence is repeatable and records dates, enabled eras, data paths, and observed endpoints.
  2. Automated tests cover all Blue requirement groups and final cross-era guardrails.
  3. Operator docs explain Blue data, shop setup, battle evidence expectations, and smoke steps.
  4. Every unresolved Blue issue is classified as follow-up, future enhancement, non-goal, or environment limitation.
**Plans**: 3 plans

Plans:
- [ ] 06-01: Full Blue smoke rerun and evidence packaging
- [ ] 06-02: Final automated regression and source-guard pass
- [ ] 06-03: Operator documentation and unresolved-item closure

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4 -> 5 -> 6

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Blue A6 Item Shop And Unlocking | 0/6 | Not started | - |
| 2. Blue A7 AdminApi And WebUI Parity | 0/3 | Not started | - |
| 3. Blue A8 Normal-Mode Cabinet Smoke And Hardening | 0/3 | Not started | - |
| 4. Blue Battle Evidence And Design | 0/3 | Not started | - |
| 5. Blue Battle Runtime Support | 0/4 | Not started | - |
| 6. Full Blue Verification And Release Hardening | 0/3 | Not started | - |
