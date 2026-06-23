# Requirements: TaikoLocalServer KIMIDORI AC15 Support

**Defined:** 2026-06-23
**Core Value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.

## v1.6 Requirements

### Evidence and Foundation

- [ ] **KFOUND-01**: KIMIDORI support proves the startup/version route ownership, game route prefix, direct-protobuf transport, and `.php` route inventory from `proto/kimidori`, linked game data, user-provided version guidance, and local binary/client evidence before runtime behavior is claimed.
- [ ] **KFOUND-02**: KIMIDORI is represented as a first-class `GameEra.Kimidori` with era-owned adapter registration, generated wire DTOs, Host settings, DI, application-part gating, and no enabled routes when the era is disabled.
- [ ] **KFOUND-03**: KIMIDORI startup and version endpoints use shared `/v01r00/chassis/*` behavior while KIMIDORI game endpoints are served under `/v05r00/chassis/*`.
- [ ] **KFOUND-04**: KIMIDORI feature support requires both protocol message presence in `proto/kimidori` and a corresponding binary `.php` route; features failing either condition are absent rather than stubbed by adjacent-era assumption.

### Catalog and Metadata

- [ ] **KCAT-01**: KIMIDORI catalog loading supports the linked root-level data layout under `Host/wwwroot/data/kimidori/data`, including at minimum `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- [ ] **KCAT-02**: KIMIDORI root-level data paths are resolved through existing path/settings abstractions instead of hardcoded `wwwroot/data/kimidori` filesystem access in handlers.
- [ ] **KCAT-03**: KIMIDORI metadata routes expose only proto-and-route-backed metadata families such as default songs, mainichi songs, folders, telops, song hash, heartbeat, bookkeeping, recommendations, and movies where binary route evidence exists.
- [ ] **KCAT-04**: KIMIDORI favorite-song limits and feature-folder behavior match KIMIDORI 0.12 evidence, including the five-song favorite cap unless local protocol or cabinet evidence proves otherwise.

### Runtime State

- [ ] **KRUN-01**: KIMIDORI card registration, login, mydon entry, and userdata readback use KIMIDORI-owned save state while sharing only true identity data across eras.
- [ ] **KRUN-02**: KIMIDORI normal playresults persist scores, crowns, profile counters, recent songs, favorite songs, rewards, Don Point totals, unlock flags, and release-song state only to KIMIDORI-owned tables.
- [ ] **KRUN-03**: KIMIDORI self-best and crown readback return persisted KIMIDORI state with era-correct byte packing and compression verified against current wire/data behavior.
- [ ] **KRUN-04**: KIMIDORI shopping-result compatibility accepts only proto-and-route-backed shopping uploads and updates KIMIDORI-owned Don Point/unlock/readback state without adding unsupported shop authority.
- [ ] **KRUN-05**: KIMIDORI challenge arrays in userdata or playresult payloads do not create Don Challenge, ChallengeCompe, battle, Tokkun, Banacoin, Taikojuku practice-folder, or cross-era state unless KIMIDORI-specific proto and route evidence proves that behavior.

### Dani Dojo

- [ ] **KDANI-01**: KIMIDORI Dani Dojo playresult and readback behavior persists only KIMIDORI-owned Dan state where `proto/kimidori` fields and binary route flow prove the normal Dan contract.
- [ ] **KDANI-02**: KIMIDORI Dani Dojo support stays separate from Taikojuku practice-folder behavior; absence of a Taikojuku proto/route does not remove proven Dani result persistence or readback.

### Admin and Verification

- [ ] **KADMIN-01**: AdminApi routes expose KIMIDORI-owned implemented state through both legacy-era-compatible routes where applicable and `/api/kimidori/...` routes validated by `EraRoute.TryParse`.
- [ ] **KADMIN-02**: The WebUI exposes KIMIDORI as a supported era with read/edit surfaces only for implemented KIMIDORI-owned state and without controls for unsupported KIMIDORI features.
- [ ] **KVERIFY-01**: Automated verification covers KIMIDORI route ownership, generated mapper behavior where relevant, root-level catalog parsing, persistence/no-cross-era boundaries, AdminApi/WebUI readback, and absence of unsupported routes or state writes.
- [ ] **KVERIFY-02**: Milestone closeout records user-observed cabinet/RPCS3 KIMIDORI smoke evidence for the supported runtime flow before full support is claimed.

## Future Requirements

### Later KIMIDORI Versions

- **KLATER-01**: Later KIMIDORI update behavior, including Taikojuku practice-folder behavior if proven by later proto/binary evidence, can be added as a future version-specific extension.
- **KLATER-02**: KIMIDORI multi-version route or catalog-root selection can be added if multiple linked versions or binary/protocol evidence require it.

### Special Capability Expansion

- **KSPEC-01**: KIMIDORI Don Challenge, ChallengeCompe, battle, Tokkun, Banacoin, global ranking, or full shop-authority behavior can be added only after KIMIDORI-specific proto, binary route, data, log, or cabinet/RPCS3 evidence proves concrete runtime semantics.

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Taikojuku practice-folder behavior for KIMIDORI 0.12 | `proto/kimidori` does not expose Taikojuku messages; missing proto means missing feature unless new local evidence proves otherwise. This does not exclude Dani Dojo result/state behavior. |
| Don Challenge or ChallengeCompe semantics | Challenge-shaped arrays in older AC15 protos are not enough to infer server-side challenge behavior without KIMIDORI route and runtime evidence. |
| Battle, Tokkun, WaiWai, or Banacoin behavior | These are adjacent-era capabilities and must not be copied into KIMIDORI without KIMIDORI-specific proto and binary route proof. |
| Full shop/payment authority | KIMIDORI may accept shopping-result compatibility where proven, but TaikoLocalServer should not invent wallet/payment/settlement authority. |
| Later KIMIDORI updates beyond 0.12 | The milestone targets KIMIDORI 0.12; later update behavior is future scope unless current local evidence pulls it in. |
| AC15 eras earlier than KIMIDORI | This milestone is KIMIDORI-only and does not start older era support. |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| KFOUND-01 | Phase 35 | Pending |
| KFOUND-02 | Phase 35 | Pending |
| KFOUND-03 | Phase 35 | Pending |
| KFOUND-04 | Phase 35 | Pending |
| KCAT-01 | Phase 36 | Pending |
| KCAT-02 | Phase 36 | Pending |
| KCAT-03 | Phase 36 | Pending |
| KCAT-04 | Phase 36 | Pending |
| KRUN-01 | Phase 37 | Pending |
| KRUN-02 | Phase 37 | Pending |
| KRUN-03 | Phase 37 | Pending |
| KRUN-04 | Phase 37 | Pending |
| KRUN-05 | Phase 37 | Pending |
| KDANI-01 | Phase 37 | Pending |
| KDANI-02 | Phase 37 | Pending |
| KADMIN-01 | Phase 38 | Pending |
| KADMIN-02 | Phase 38 | Pending |
| KVERIFY-01 | Phase 38 | Pending |
| KVERIFY-02 | Phase 38 | Pending |

**Coverage:**
- v1.6 requirements: 19 total
- Mapped to phases: 19
- Unmapped: 0

---
*Requirements defined: 2026-06-23*
*Last updated: 2026-06-23 after roadmap creation*
