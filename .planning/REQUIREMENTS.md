# Requirements: Murasaki AC15 Support

**Defined:** 2026-06-21
**Milestone:** v1.5 Murasaki AC15 Support
**Core Value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.

## v1.5 Requirements

### Murasaki Foundation

- [x] **MFND-01**: Developer can review a Murasaki evidence record that identifies startup and game route prefixes, `.php` route suffixes, direct-protobuf transport expectations, active data root, IDA/binary evidence handles, absent `initialdatacheck.php` evidence, and unresolved gaps before Murasaki routes are finalized.
- [x] **MFND-02**: Murasaki is served by a first-class enableable `GameEra.Murasaki` adapter with generated Murasaki wire DTOs from `proto/murasaki`, era settings, Host/DI registration, route ownership for `/v06r00/chassis/*.php`, shared `/v01r00/chassis/*` startup/version use only where evidence supports it, and enabled-era gating.
- [x] **MFND-03**: Murasaki work preserves existing supported-era behavior and dumped proto immutability; any shared code changes remain capability-owned and covered by observable regression checks.

### Murasaki Catalog And Profile

- [x] **MCAT-01**: Murasaki catalog initialization loads the proven active Murasaki data root and binds matching AC15 music, medley, tuning, folder, telop, recommendation, Taikojuku, present, and special-BAID data through shared loaders where file formats match.
- [x] **MCAT-02**: Murasaki has an explicit AC15 capability/profile model with Murasaki protocol limits, favorite cap 10 where proven, Don Point/root limits where proven, feature flags, wire placement, split metadata support, and typed persistence boundaries while absent surfaces remain disabled.
- [x] **MCAT-03**: Murasaki server-authored sidecar data exists and is copied for every implemented Murasaki feature that needs committed server data outside raw operator files, including intentionally empty sidecars where absence or conservative default behavior is the contract.

### Murasaki Split Metadata

- [x] **MMETA-01**: Murasaki split metadata routes for proven `defaultsong.php`, `mainichisong.php`, `foldercheck.php`, `getfolder.php`, `telopcheck.php`, `gettelop.php`, and related catalog-backed readback use dedicated Application queries and Murasaki mappers instead of copying a White-style `initialdatacheck.php` contract.
- [x] **MMETA-02**: Murasaki song-hash, default-song, mainichi-song, release-song, crown, content, option, and reserved byte payloads are generated only from proven Murasaki limits, existing packers whose compatibility is verified, or conservative documented defaults; guessed byte semantics are not accepted.
- [x] **MMETA-03**: Murasaki operational route surfaces such as `heartbeat.php`, `bookkeeping.php`, `communicationlog.php`, and `headclerk2.php` are implemented only as route-evidence-backed catalog, log-success, or no-state compatibility endpoints and do not create unintended economy, audit, or gameplay persistence.

### Murasaki Runtime Binding

- [x] **MSTATE-01**: Murasaki binds shared identity/userdata behavior to Murasaki-owned save/profile tables so the cabinet can register or find a card, create mydon/profile data, and read/write userdata without writing another era's gameplay state.
- [x] **MSTATE-02**: Murasaki binds matching normal-play behavior to Murasaki-owned score, self-best, crown, favorite, recent-song, unlock, reward/progress, and profile-counter tables where Murasaki protocol and data shapes match.
- [x] **MSTATE-03**: Murasaki self-best, crown, favorite, recent-song, and release-song readback uses Murasaki protocol byte and array limits, including favorite cap 10 where proven, and does not use `bestscore.php` or global ranking data as a substitute for per-user self-best.
- [x] **MSTATE-04**: Murasaki Taikojuku/Dani runtime behavior writes and reads only Murasaki-owned Dan state when Murasaki payload and catalog evidence proves the same contract; otherwise the unsupported runtime write/readback gap is documented instead of invented.

### Murasaki Special Capabilities

- [x] **MSPEC-01**: Murasaki reward, present, special-BAID, and Don Point behavior uses Murasaki-owned profile/unlock flags plus local `present.xml`, `spacialbaid.xml`, and protocol evidence without creating Yellow item-shop, medal, Banacoin wallet/payment, or unrelated unlock semantics.
- [x] **MSPEC-02**: Murasaki `bestscore.php`, `songhash.php`, `shoppingresult.php`, challenge arrays, `content_info`, `default_option_setting`, and reserved bytes get targeted binary/log/cabinet evidence and a bounded implementation only where that evidence defines route sequence, byte sizes, defaults, and state authority; otherwise their conservative behavior is documented.
- [x] **MSPEC-03**: Murasaki Don Challenge-like behavior is implemented only if local Murasaki data and client read/write semantics prove a server-side contract; Red `challengecompe.php` routing and White Don Challenge behavior are not copied by assumption.

### Admin, Verification, And Closeout

- [ ] **MVER-01**: AdminApi and WebUI expose only implemented Murasaki-owned readback and edit surfaces through existing era-routed contracts and do not read or write another era's gameplay state.
- [ ] **MVER-02**: Automated verification covers Murasaki observable route, handler, catalog, persistence, mapper/classifier, protocol packing, build-output copy, and no-cross-era/no-cross-mode behavior without adding implementation-shape tests that do not protect cabinet behavior.
- [ ] **MVER-03**: Murasaki support closes only after full automated verification, Mapperly generated-source inspection for nontrivial mappings, a Host build using temp output if needed, and user-accepted cabinet/RPCS3 and WebUI evidence for implemented Murasaki flows.

## Future Requirements

### Later Murasaki Versions

- **MLATER-01**: Later Murasaki update behavior or multi-root Murasaki version selection can be added only after a later milestone supplies local proto/data/config/runtime evidence for that specific version range.

### Murasaki Special Capability Expansion

- **MSPEC-04**: Full global ranking authoring, shopping authority, challenge scheduling/management, or operator-authored challenge behavior can be added only after Murasaki runtime evidence proves the data, route, and readback contracts.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Later Murasaki update behavior outside the initial local target | v1.5 targets the current local Murasaki evidence; wiki/update-history context scopes investigation but does not define runtime contracts. |
| White-style `initialdatacheck.php` | Murasaki proto and IDA research indicate split metadata requests instead of a monolithic initial-data endpoint. |
| Fake global rankings from local self-best data | `bestscore.php` is a distinct sequence/ranking contract and must not be satisfied with per-user self-best rows by assumption. |
| Guessed song-hash, default-song, mainichi-song, content, option, or reserved byte layouts | Byte-heavy protocol fields require binary/client/log proof before semantics are modeled. |
| Standalone Red-style `challengecompe.php` without Murasaki evidence | Murasaki has embedded challenge arrays but no proven standalone ChallengeCompe request/response route. |
| White Don Challenge behavior copied into Murasaki without proof | Don Challenge-like behavior must be Murasaki-specific and evidence-backed. |
| Yellow item shop, Don/Katsu medals, shop seasons, or Yellow shop UI | Murasaki `shoppingresult.php` is not evidence for Yellow shop semantics. |
| Banacoin wallet, payment, coupon, balance, settlement, receipt, BNID, or transaction state | Current Murasaki proto evidence does not define Banacoin authority or payment routes. |
| Blue battle, Green AI battle, Tokkun, WaiWai, gacha runtime, tournaments, or White-final-only behavior | Missing or unrelated surfaces stay absent unless local Murasaki evidence proves them. |
| Shared cross-era gameplay persistence tables | Murasaki state must remain Murasaki-owned; shared AC15 code can share algorithms, not gameplay tables. |
| Runtime scraping of wiki or official pages | Public pages can scope investigation, but local proto, data, logs, IDA, and cabinet/RPCS3 evidence decide server behavior. |
| Mapper-side or controller-side business behavior | Controllers deserialize/map/call Mediator/map back; business behavior belongs in Application handlers/services. |

## Traceability

Roadmap phase mapping for v1.5 Murasaki AC15 Support. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| MFND-01 | Phase 28 | Complete |
| MFND-02 | Phase 28 | Complete |
| MFND-03 | Phase 28 | Complete |
| MCAT-01 | Phase 29 | Complete |
| MCAT-02 | Phase 29 | Complete |
| MCAT-03 | Phase 29 | Complete |
| MMETA-01 | Phase 30 | Complete |
| MMETA-02 | Phase 30 | Complete |
| MMETA-03 | Phase 30 | Complete |
| MSTATE-01 | Phase 31 | Complete |
| MSTATE-02 | Phase 32 | Complete |
| MSTATE-03 | Phase 31 | Complete |
| MSTATE-04 | Phase 32 | Complete |
| MSPEC-01 | Phase 32 | Complete |
| MSPEC-02 | Phase 33 | Complete |
| MSPEC-03 | Phase 33 | Complete |
| MVER-01 | Phase 34 | Pending |
| MVER-02 | Phase 34 | Pending |
| MVER-03 | Phase 34 | Pending |
| MLATER-01 | Future milestone | Deferred |
| MSPEC-04 | Future milestone | Deferred |

**Coverage:**

- v1.5 requirements: 19 total
- Mapped to phases: 19
- Unmapped: 0
- Future requirements: 2 deferred

---
*Requirements defined: 2026-06-21*
*Last updated: 2026-06-21 after phases 28-32 implementation*
