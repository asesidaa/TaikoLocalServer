# Requirements: TaikoLocalServer Blue Tokkun Mode Support

**Defined:** 2026-06-03
**Milestone:** v1.1 Blue Tokkun Mode Support
**Core Value:** A Blue cabinet can use TaikoLocalServer for normal, battle, and Tokkun play with repeatable cabinet/RPCS3 evidence, without corrupting or conflating Green, Nijiiro, or shared identity state.

## v1.1 Requirements

Requirements for the current milestone. Each maps to roadmap phases.

### Tokkun Evidence Contract

- [x] **TKEV-01**: Operator/developer can review an evidence-tagged Blue Tokkun protocol contract covering proven, observed, and deliberately ignored fields.
- [x] **TKEV-02**: Blue Tokkun classification does not depend on a guessed numeric `play_mode`; the numeric value remains unknown until proven by RPCS3/cabinet logs or deeper IDA evidence.
- [x] **TKEV-03**: The old source guard that banned Tokkun terms is replaced by bounded guards that allow only named Blue Tokkun support paths and still block invented reward, score, Banacoin, or battle semantics.

### Banacoin Compatibility

- [x] **TKBC-01**: Blue Banacoin-adjacent endpoints needed for Tokkun entry return stateless, permissive success-shaped responses sufficient for Tokkun to remain playable.
- [x] **TKBC-02**: Blue Banacoin compatibility does not persist balance, coupons, payments, deductions, transaction history, `chid`, BNID result state, or wallet-like state.
- [x] **TKBC-03**: `getbanacoininfo.php` is added only if cabinet/RPCS3 logs or IDA route evidence proves Blue Tokkun calls it and current absence blocks play.

### Tokkun Playresult Acceptance

- [x] **TKPR-01**: Blue `playresult.php` accepts and classifies Tokkun uploads from proven Tokkun fields such as `ary_tokkunstage_info` and `tokkun_tutorial_flg`.
- [x] **TKPR-02**: Blue Tokkun uploads return success without writing normal score, crown, Dani, battle, favorite, recent-song, profile, unlock, medal, customization, title, or shop state.
- [x] **TKPR-03**: Unknown or mixed Tokkun shapes are logged with bounded request context and return success unless concrete client evidence proves a failure response is required.

### Tokkun State

- [ ] **TKST-01**: Blue persists Tokkun tutorial state from `tokkun_tutorial_flg` and reads it back through Blue userdata when Tokkun first-run/readback behavior requires it.
- [ ] **TKST-02**: Blue persists Tokkun summary/progress facts carried by the Tokkun payload, including `banacoin_datetime`, song count/list, speed-change count, autoplay count, jump count, and upload time, as Blue-owned Tokkun data.
- [ ] **TKST-03**: Blue Tokkun persisted data remains separate from Green, Nijiiro, normal score, Dani, battle, item shop, and Banacoin storage.
- [ ] **TKST-04**: Tokkun persistence stores raw/protocol-backed Tokkun facts only; it does not infer rankings, rewards, score progression, payment history, or practice-time rules without concrete Blue evidence.

### Verification

- [ ] **TKVF-01**: Automated tests cover Tokkun classification, mapper DTO fields, safe playresult acceptance, no-cross-write behavior, Banacoin statelessness, Tokkun persistence, and replacement source guards.
- [ ] **TKVF-02**: Cabinet/RPCS3 smoke evidence covers Tokkun selection, Banacoin request sequence, gameplay entry, final upload, post-upload userdata behavior, and any unexpected endpoint calls.
- [ ] **TKVF-03**: Final docs record confirmed Tokkun constants/routes, persisted Tokkun fields, unresolved research flags, and the exact boundary between supported Tokkun behavior and out-of-scope Banacoin/payment semantics.

## Future Requirements

Deferred to future milestones. Tracked but not in the current roadmap.

### Admin and Developer Surfaces

- **TKUI-01**: Operator/developer can inspect Blue Tokkun history or debug state through AdminApi/WebUI after cabinet-play support and backend Tokkun persistence are verified.

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Real Banacoin wallet, balance, payment, settlement, receipt, coupon, deduction, BNID result, or transaction-history behavior | TaikoLocalServer is not a Banacoin authority; the repo should return only enough stateless compatibility for Tokkun to remain playable. |
| Score, ranking, crown, reward, song unlock, medal, favorite, recent-song, profile-counter, customization, title, or shop reflection from Tokkun | Wiki gameplay context and current Blue evidence do not support normal progression side effects for Tokkun. |
| Non-Blue-era Tokkun support | v1.1 is scoped to Blue AC15. |
| `PlayMode.Tokkun` with a guessed numeric value | The enum value is added only after cabinet/RPCS3 logs or deeper IDA evidence proves the number. |
| Runtime scraping of wiki or official pages | Wiki context is gameplay context only; local protocol, binary, logs, IDA, and cabinet/RPCS3 evidence decide server behavior. |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| TKEV-01 | Phase 7 | Complete |
| TKEV-02 | Phase 7 | Complete |
| TKEV-03 | Phase 7 | Complete |
| TKBC-01 | Phase 8 | Complete |
| TKBC-02 | Phase 8 | Complete |
| TKBC-03 | Phase 8 | Complete |
| TKPR-01 | Phase 9 | Complete |
| TKPR-02 | Phase 9 | Complete |
| TKPR-03 | Phase 9 | Complete |
| TKST-01 | Phase 10 | Pending |
| TKST-02 | Phase 10 | Pending |
| TKST-03 | Phase 10 | Pending |
| TKST-04 | Phase 10 | Pending |
| TKVF-01 | Phase 11 | Pending |
| TKVF-02 | Phase 11 | Pending |
| TKVF-03 | Phase 11 | Pending |
| TKUI-01 | Future milestone | Deferred |

**Coverage:**

- v1.1 requirements: 16 total
- Mapped to phases: 16
- Unmapped: 0
- Future requirement TKUI-01 is intentionally not mapped to this roadmap.

---
*Requirements defined: 2026-06-03*
*Last updated: 2026-06-03 after roadmap creation*
