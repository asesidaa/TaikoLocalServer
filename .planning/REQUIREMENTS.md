# Requirements: Red AC15 Support

**Defined:** 2026-06-12
**Milestone:** v1.3 Red AC15 Support
**Core Value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Blue, Green, Yellow, Nijiiro, or shared identity state.

## v1.3 Requirements

### Red Foundation

- [x] **RFND-01**: Developer can review a Red route/version evidence record that identifies supported route prefix, direct-protobuf transport expectations, shared startup/version ownership, HDD/version mapping, active data root, and unresolved client-evidence gaps before routes are finalized.
- [x] **RFND-02**: Red is served by a first-class enableable `GameEra.Red` adapter with generated Red wire DTOs from `proto/red`, era settings, Host/DI registration, route ownership, and enabled-era gating.
- [x] **RFND-03**: Red work preserves current supported-era behavior except where shared code changes are required and existing behavior remains covered.

### Red Capability Profile And Catalog Binding

- [ ] **RCAT-01**: Red catalog initialization binds shared AC15 catalog capabilities for matching music, tuning, Taikojuku/Dani, folder, telop, recommendation, movie, and customization data, with Red-specific parser work only where Red local data proves a delta.
- [ ] **RCAT-02**: Red has an explicit AC15 capability/profile model that composes supported behavior modules with Red config roots, protocol limits, wire placement, and typed persistence boundaries without inheriting unsupported later-era behavior.

### Red Runtime Capability Binding

- [ ] **RSTATE-01**: Red binds the shared identity/userdata capability to Red-owned save/profile tables so the cabinet can register or find a card, create mydon/profile data, and read/write userdata without writing another era's gameplay tables.
- [ ] **RSTATE-02**: Red binds the shared normal-play capability to Red-owned score, self-best, crown, favorite, recent-song, and profile-counter tables where protocol and data shapes match.
- [ ] **RSTATE-03**: Red binds the shared Taikojuku/Dani capability to Red-owned Dan state and Red protocol limits where behavior matches.

### Red Tokkun And Simple Compatibility

- [ ] **RSTATE-04**: Red reward card, reward execution, and Don point fields are handled only as simple Red protocol/profile compatibility when runtime evidence requires them, with no item-shop, medal, or unlock semantics.
- [ ] **RSTATE-05**: Red Tokkun playresults are classified before normal handling and persist/read back only Red-owned tutorial state; no Tokkun raw history, score, crown, reward, unlock, or challenge state is invented.
- [ ] **RCOMP-01**: Red Banacoin-adjacent routes are implemented only as stateless compatibility when observed or required by Red runtime flow, with no wallet, balance, payment, coupon, settlement, receipt, or transaction persistence.

### Older-AC15 ChallengeCompe Capability

- [ ] **RCOMP-02**: ChallengeCompe is modeled as a shared older-AC15 capability, with Red as the first binding; protocol compatibility is supported only at the minimal level proven by Red client/runtime evidence until the shared endpoint, payload, and response contract is known.
- [ ] **RCHAL-01**: Don Challenge product scope is documented from wiki context for the shared older-AC15 capability: monthly tasks, individual/community distinction, normal-play completion, Tokkun exclusion, and song/title reward thresholds/timing.
- [ ] **RCHAL-02**: Stateful ChallengeCompe progress, reward, task-list, or community behavior is implemented as a shared older-AC15 capability only after Red runtime/proto/client evidence identifies the endpoint, payload, state shape, and accepted response contract.

### Admin, Verification, And Contract

- [ ] **RVER-01**: AdminApi and WebUI expose only implemented Red-owned readback surfaces and do not read or write another era's gameplay state.
- [ ] **RVER-02**: Automated tests cover Red observable route, handler, catalog, persistence, mapper/classifier, and no-cross-era/no-cross-mode behavior.
- [ ] **RVER-03**: Red support closes only after full automated verification, a temp-output Host build, and cabinet/RPCS3 smoke evidence for implemented normal, Tokkun tutorial, simple compatibility, and ChallengeCompe flows.

## Future Requirements

### Older AC15 Reuse

- **OLDAC15-01**: Older-than-Red support can bind the shared ChallengeCompe capability only after Red proves the shared client contract and the later milestone supplies its own proto/data/config evidence.

### Don Challenge Tooling

- **RCHAL-03**: Admin editing, operator-authored schedules, global/community challenge simulation, or challenge reward management can be added only after the runtime contract is proven and a stateful challenge implementation exists.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Stateful ChallengeCompe behavior without runtime/proto/client proof | Wiki establishes product scope, not endpoint, schema, database, or response semantics. |
| ChallengeCompe hardcoded as Red-only behavior | ChallengeCompe is a shared older-AC15 capability; Red is the first binding, not the capability boundary. |
| Red WaiWai, battle, item-shop, medal, AI/ghost, token-count, shop-folder, or non-challenge unlock behavior without Red evidence | These are not Red requirements just because other eras or later versions have nearby surfaces. |
| Existing supported-era gameplay behavior changes for Red | v1.3 adds Red while preserving existing supported era contracts. |
| Shared cross-era gameplay persistence tables | Shared AC15 behavior must still use era-owned rows and explicit typed boundaries. |
| Real Banacoin balance, payment, settlement, receipt, coupon, deduction, BNID result, or transaction-history behavior | TaikoLocalServer is not a Banacoin authority for this milestone. |
| Red Tokkun state beyond tutorial readback, including raw history, rewards, score/crown writes, paid-coin behavior, practice-time accounting, jump-point behavior, autoplay behavior, speed-change behavior, or unlock side effects | Red Tokkun is intentionally narrow unless Red evidence proves more than tutorial state. |
| Runtime scraping of wiki or official pages | Public pages are scoping context only; local protocol, binary, logs, IDA, and cabinet/RPCS3 evidence decide server behavior. |

## Traceability

Roadmap phase mapping for v1.3 Red AC15 Support.

| Requirement | Phase | Status |
|-------------|-------|--------|
| RFND-01 | Phase 18 | Complete |
| RFND-02 | Phase 18 | Complete |
| RFND-03 | Phase 18 | Complete |
| RCAT-01 | Phase 19 | Pending |
| RCAT-02 | Phase 19 | Pending |
| RSTATE-01 | Phase 20 | Pending |
| RSTATE-02 | Phase 20 | Pending |
| RSTATE-03 | Phase 20 | Pending |
| RSTATE-04 | Phase 20 | Pending |
| RSTATE-05 | Phase 20 | Pending |
| RCOMP-01 | Phase 20 | Pending |
| RCOMP-02 | Phase 21 | Pending |
| RCHAL-01 | Phase 21 | Pending |
| RCHAL-02 | Phase 21 | Pending |
| RVER-01 | Phase 22 | Pending |
| RVER-02 | Phase 22 | Pending |
| RVER-03 | Phase 22 | Pending |
| OLDAC15-01 | Future milestone | Deferred |
| RCHAL-03 | Future milestone | Deferred |

**Coverage:**

- v1.3 requirements: 17 total
- Mapped to phases: 17
- Unmapped: 0
- Future requirements: 2 deferred

---
*Requirements defined: 2026-06-12*
