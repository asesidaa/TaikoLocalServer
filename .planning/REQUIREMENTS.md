# Requirements: Red AC15 Support

**Defined:** 2026-06-12
**Milestone:** v1.3 Red AC15 Support
**Core Value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Blue, Green, Yellow, Nijiiro, or shared identity state.

## v1.3 Requirements

### Red Foundation

- [ ] **RFND-01**: Developer can review a Red route/version evidence record that identifies supported route prefix, direct-protobuf transport expectations, shared startup/version ownership, HDD/version mapping, active data root, and unresolved client-evidence gaps before routes are finalized.
- [ ] **RFND-02**: Red is served by a first-class enableable `GameEra.Red` adapter with generated Red wire DTOs from `proto/red`, era settings, Host/DI registration, route ownership, and enabled-era gating.
- [ ] **RFND-03**: Red work preserves current supported-era behavior except where shared code changes are required and existing behavior remains covered.

### Red Catalog And Shared AC15 Profile

- [ ] **RCAT-01**: Red catalog initialization uses shared AC15 catalog loaders and services for matching music, tuning, Taikojuku/Dani, folder, telop, recommendation, movie, and customization data, with Red-specific parser work only where Red local data proves a delta.
- [ ] **RCAT-02**: Red has an explicit AC15 capability/profile model that matches Red proto-supported surfaces and shared AC15 behavior without inheriting unsupported later-era behavior.

### Red Identity, Normal Play, And Dani

- [ ] **RSTATE-01**: Red cabinet can register or find a card, create mydon/profile data, and read/write Red-owned userdata without writing another era's gameplay tables.
- [ ] **RSTATE-02**: Red normal playresults persist and read back Red-owned scores, self-best rows, crowns, favorites, recent songs, and profile counters through shared AC15 services where protocol and data shapes match.
- [ ] **RSTATE-03**: Red Taikojuku/Dani requests and Dan playresults persist and read back Red-owned Dan state through shared AC15 Dani logic where behavior matches.

### Red Reward, Tokkun, And Compatibility

- [ ] **RSTATE-04**: Red reward/progression compatibility handles `reward_ptn`, `reward_progress`, Don point fields, `rewardcardcheck`, and `rewardexecution` as Red protocol state without item-shop or medal semantics.
- [ ] **RSTATE-05**: Red Tokkun playresults are classified before normal handling and persist only Red-owned protocol-backed Tokkun tutorial/raw history state.
- [ ] **RCOMP-01**: Red Banacoin-adjacent routes are implemented only as stateless compatibility when observed or required by Red runtime flow, with no wallet, balance, payment, coupon, settlement, receipt, or transaction persistence.

### Don Challenge And Challenge Competition

- [ ] **RCOMP-02**: Red ChallengeCompe protocol compatibility is supported at the minimal level proven by Red client/runtime evidence; until evidence exists, the server does not invent challenge catalog, progress, or reward schema.
- [ ] **RCHAL-01**: Don Challenge product scope is documented from wiki context: monthly tasks, individual/community distinction, normal-play completion, Tokkun exclusion, and song/title reward thresholds/timing.
- [ ] **RCHAL-02**: Stateful Don Challenge progress, reward, task-list, or community behavior is implemented only after Red runtime/proto/client evidence identifies the endpoint, payload, and accepted response contract.

### Admin, Verification, And Contract

- [ ] **RVER-01**: AdminApi and WebUI expose only implemented Red-owned readback surfaces and do not read or write another era's gameplay state.
- [ ] **RVER-02**: Automated tests cover Red observable route, handler, catalog, persistence, mapper/classifier, and no-cross-era/no-cross-mode behavior.
- [ ] **RVER-03**: Red support closes only after full automated verification, a temp-output Host build, and cabinet/RPCS3 smoke evidence for implemented normal, Tokkun, compatibility, and challenge flows.

## Future Requirements

### Older AC15 Reuse

- **OLDAC15-01**: Older-than-Red support can reuse Red/ChallengeCompe behavior only after Red proves the shared client contract and the later milestone supplies its own proto/data evidence.

### Don Challenge Tooling

- **RCHAL-03**: Admin editing, operator-authored schedules, global/community challenge simulation, or challenge reward management can be added only after the runtime contract is proven and a stateful challenge implementation exists.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Stateful Don Challenge behavior without runtime/proto/client proof | Wiki establishes product scope, not endpoint, schema, database, or response semantics. |
| Red WaiWai, battle, item-shop, medal, AI/ghost, token-count, or shop-folder behavior without Red evidence | These are not Red requirements just because other eras or later versions have nearby surfaces. |
| Existing supported-era gameplay behavior changes for Red | v1.3 adds Red while preserving existing supported era contracts. |
| Shared cross-era gameplay persistence tables | Shared AC15 behavior must still use era-owned rows and explicit typed boundaries. |
| Real Banacoin balance, payment, settlement, receipt, coupon, deduction, BNID result, or transaction-history behavior | TaikoLocalServer is not a Banacoin authority for this milestone. |
| Invented Tokkun rewards, score/crown writes, paid-coin behavior, practice-time accounting, jump-point behavior, autoplay behavior, speed-change behavior, or unlock side effects | Tokkun state remains protocol-backed and separated from normal progression unless Red evidence proves otherwise. |
| Runtime scraping of wiki or official pages | Public pages are scoping context only; local protocol, binary, logs, IDA, and cabinet/RPCS3 evidence decide server behavior. |

## Traceability

Roadmap phase mapping for v1.3 Red AC15 Support.

| Requirement | Phase | Status |
|-------------|-------|--------|
| RFND-01 | Phase 18 | Pending |
| RFND-02 | Phase 18 | Pending |
| RFND-03 | Phase 18 | Pending |
| RCAT-01 | Phase 19 | Pending |
| RCAT-02 | Phase 19 | Pending |
| RSTATE-01 | Phase 20 | Pending |
| RSTATE-02 | Phase 20 | Pending |
| RSTATE-03 | Phase 20 | Pending |
| RSTATE-04 | Phase 21 | Pending |
| RSTATE-05 | Phase 21 | Pending |
| RCOMP-01 | Phase 21 | Pending |
| RCOMP-02 | Phase 22 | Pending |
| RCHAL-01 | Phase 22 | Pending |
| RCHAL-02 | Phase 22 | Pending |
| RVER-01 | Phase 23 | Pending |
| RVER-02 | Phase 23 | Pending |
| RVER-03 | Phase 23 | Pending |
| OLDAC15-01 | Future milestone | Deferred |
| RCHAL-03 | Future milestone | Deferred |

**Coverage:**

- v1.3 requirements: 17 total
- Mapped to phases: 17
- Unmapped: 0
- Future requirements: 2 deferred

---
*Requirements defined: 2026-06-12*
