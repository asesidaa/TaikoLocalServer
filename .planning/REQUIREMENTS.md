# Requirements: TaikoLocalServer MOMOIRO AC15 0.11 Support

**Defined:** 2026-06-25
**Milestone:** v1.7 MOMOIRO AC15 0.11 Support
**Core Value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating MOMOIRO, KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.

## v1.7 Requirements

### Evidence and Foundation

- [ ] **MOFND-01**: MOMOIRO support proves startup/version route ownership, game route prefix, direct-protobuf transport, `.php` route inventory, binary evidence handles, and unresolved route/limit gaps before runtime behavior is claimed.
- [ ] **MOFND-02**: MOMOIRO is a first-class `GameEra.Momoiro` with era-owned adapter registration, generated wire DTOs, Host settings, DI, application-part gating, and no enabled routes when the era is disabled.
- [ ] **MOFND-03**: MOMOIRO startup/version endpoints use shared `/v01r00/chassis/*.php` behavior while MOMOIRO game endpoints are served under `/v04r00/chassis/*.php`.
- [ ] **MOFND-04**: MOMOIRO feature support requires both protocol message presence in `proto/momoiro` and corresponding binary/client `.php` route evidence; features failing either condition stay absent.

### Catalog, Metadata, and Limits

- [ ] **MOCAT-01**: MOMOIRO catalog loading supports the root-level layout under `Host/wwwroot/data/momoiro/data`, including `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- [ ] **MOCAT-02**: MOMOIRO data paths are resolved through existing path/settings abstractions instead of hardcoded runtime filesystem access in handlers.
- [ ] **MOCAT-03**: MOMOIRO has an explicit AC15 profile/limits model for byte widths, song ordering, favorite/recent limits, default/mainichi flags, song hash, release flags, crown placement, Don Point/reward limits, and absent feature flags, backed by local evidence.
- [ ] **MOCAT-04**: MOMOIRO metadata and compatibility routes expose only proto-and-route-backed route families such as `songhash`, `defaultsong`, `mainichisong`, telops, recommendations, heartbeat, bookkeeping, communication logs, and best-score compatibility where evidence exists.
- [ ] **MOCAT-05**: MOMOIRO crown readback is modeled as userdata-owned through `UserDataResponse.hash_crown_flg`; no standalone `crownsdata.php` contract is added unless new MOMOIRO evidence proves one.

### Runtime Readback

- [ ] **MORDB-01**: MOMOIRO card registration, login, mydon entry, and userdata readback use MOMOIRO-owned save state while sharing only true identity data across eras.
- [ ] **MORDB-02**: MOMOIRO self-best readback returns MOMOIRO-owned score state with era-correct normal, ura, and shin handling where proven.
- [ ] **MORDB-03**: MOMOIRO favorite and recent song readback uses binary/client-backed limits, ordering, truncation, and duplicate behavior.
- [ ] **MORDB-04**: MOMOIRO crown bytes in `userdata.php` use binary/client-backed packing, song count, difficulty placement, and default behavior.
- [ ] **MORDB-05**: MOMOIRO release-song and song-hash readback uses MOMOIRO catalog order and binary-backed `song_hash_ver`, `song_hash_tbl`, and `hash_release_song_flg` semantics.

### Runtime Mutation

- [ ] **MORUN-01**: MOMOIRO normal playresults persist scores, self-best, crowns, profile counters, recent songs, favorite songs, and related normal-play state only to MOMOIRO-owned tables.
- [ ] **MORUN-02**: MOMOIRO song unlock, Don Point, reward, and `shoppingresult.php` behavior mutates only evidence-backed MOMOIRO-owned fields and does not create newer item-shop, wallet, payment, or shop-season authority.
- [ ] **MORUN-03**: MOMOIRO Dan/Dani fields are persisted and read back only where MOMOIRO playresult, userdata, and binary/client evidence prove the normal Dan contract; Taikojuku practice-folder behavior remains separate and absent.
- [ ] **MORUN-04**: MOMOIRO challenge-shaped arrays are accepted, stored, echoed, or omitted only according to MOMOIRO-specific evidence and do not create Don Challenge, ChallengeCompe, or reward-management behavior by assumption.
- [ ] **MORUN-05**: MOMOIRO runtime writes do not touch KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, Tokkun, battle, Banacoin, or unsupported feature state.

### Admin, WebUI, and Verification

- [ ] **MOADMIN-01**: AdminApi routes expose MOMOIRO-owned implemented state through `/api/momoiro/...` and applicable era-routed contracts without reading or writing another era's gameplay state.
- [ ] **MOADMIN-02**: The WebUI exposes MOMOIRO as a supported era with read/edit surfaces only for implemented MOMOIRO-owned state and without controls for unsupported MOMOIRO features.
- [ ] **MOVFY-01**: Automated verification covers MOMOIRO route ownership, generated mapper behavior, root-level catalog parsing, persistence/no-cross-era boundaries, byte packing, AdminApi/WebUI readback, and absence of unsupported routes or state writes.
- [ ] **MOVFY-02**: MOMOIRO verification includes Mapperly generated-source inspection and a full build/temp-output Host build where needed before runtime closeout.
- [ ] **MOVFY-03**: Milestone closeout records repeatable cabinet/RPCS3 MOMOIRO smoke evidence for supported startup, login, userdata, catalog/readback, playresult, self-best/crown, AdminApi, and WebUI flows before full support is claimed.

## Future Requirements

### Later MOMOIRO Versions

- **MOLATER-01**: Later MOMOIRO update behavior or multi-version MOMOIRO route/catalog selection can be added only after a later milestone supplies local proto/data/config/runtime evidence for that specific version range.

### Special Capability Expansion

- **MOSPEC-01**: MOMOIRO Taikojuku, Tokkun, Banacoin, battle, gacha, tournament, Don Challenge, ChallengeCompe, event-folder, newer item-shop authority, or live-service shop behavior can be added only after MOMOIRO-specific proto, binary route, data, log, or cabinet/RPCS3 evidence proves concrete runtime semantics.
- **MOSPEC-02**: Rich AdminApi/WebUI editing for packed MOMOIRO crown, release-song, favorite/recent, challenge, or Dan state can be added after byte limits and runtime readback are proven stable.

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Standalone `crownsdata.php` | MOMOIRO proto exposes crowns through `UserDataResponse.hash_crown_flg`; a separate crown route would invent a contract unless binary/client evidence proves one. |
| KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, or Nijiiro gameplay persistence reuse | MOMOIRO gameplay state must be MOMOIRO-owned; shared AC15 code can share algorithms, not gameplay tables. |
| `config/STxxxx-*` MOMOIRO catalog layout | The local MOMOIRO data layout is root-level under `Host/wwwroot/data/momoiro/data`; newer config-root assumptions are not evidence. |
| Taikojuku practice-folder behavior | Local MOMOIRO proto has Dan fields but no Taikojuku request/response family; Dani/Dan support does not imply Taikojuku routes. |
| Don Challenge or ChallengeCompe semantics | Challenge-shaped arrays are protocol facts only; they do not prove standalone challenge routes, state authority, or reward behavior. |
| Battle, Tokkun, Banacoin, WaiWai, gacha, tournament, event-folder, or newer item-shop authority | These are adjacent-era or later-era capabilities and must not be copied into MOMOIRO without MOMOIRO-specific proto and binary route proof. |
| Real payment, wallet, settlement, receipt, coupon, BNID, or transaction-history behavior | TaikoLocalServer is not a live payment authority, and MOMOIRO 0.11 scope does not prove those surfaces. |
| Later MOMOIRO updates beyond 0.11 | The milestone targets MOMOIRO 0.11; later update behavior is future scope unless current local evidence pulls it in. |
| Runtime scraping of wiki or official pages | Public pages can scope investigation, but local proto, data, logs, IDA, and cabinet/RPCS3 evidence decide server behavior. |
| Mapper-side or controller-side business behavior | Controllers deserialize, map, call Mediator, and map back; business behavior belongs in Application handlers/services. |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| MOFND-01 | TBD | Pending |
| MOFND-02 | TBD | Pending |
| MOFND-03 | TBD | Pending |
| MOFND-04 | TBD | Pending |
| MOCAT-01 | TBD | Pending |
| MOCAT-02 | TBD | Pending |
| MOCAT-03 | TBD | Pending |
| MOCAT-04 | TBD | Pending |
| MOCAT-05 | TBD | Pending |
| MORDB-01 | TBD | Pending |
| MORDB-02 | TBD | Pending |
| MORDB-03 | TBD | Pending |
| MORDB-04 | TBD | Pending |
| MORDB-05 | TBD | Pending |
| MORUN-01 | TBD | Pending |
| MORUN-02 | TBD | Pending |
| MORUN-03 | TBD | Pending |
| MORUN-04 | TBD | Pending |
| MORUN-05 | TBD | Pending |
| MOADMIN-01 | TBD | Pending |
| MOADMIN-02 | TBD | Pending |
| MOVFY-01 | TBD | Pending |
| MOVFY-02 | TBD | Pending |
| MOVFY-03 | TBD | Pending |

**Coverage:**

- v1.7 requirements: 24 total
- Mapped to phases: 0
- Unmapped: 24

---
*Requirements defined: 2026-06-25*
*Last updated: 2026-06-25 after requirements definition*
