# Project Research Summary

**Project:** TaikoLocalServer AC15 Era Support - v1.7 MOMOIRO AC15 0.11 Support
**Domain:** Brownfield ASP.NET Core cabinet protocol support for an evidence-driven older AC15 era
**Researched:** 2026-06-25
**Confidence:** MEDIUM

## Executive Summary

MOMOIRO v1.7 is not generic product work and should not introduce a new runtime stack. It is a brownfield AC15 era integration inside TaikoLocalServer: direct-protobuf `.php` cabinet endpoints, generated adapter-local wire DTOs, Mapperly mapping into Application/Common DTOs, MOMOIRO-owned persistence, root-level catalog loading, and existing AdminApi/WebUI era routing. Shared startup/version traffic belongs under `/v01r00/chassis/*.php`; MOMOIRO game traffic belongs under `/v04r00/chassis/*.php`.

The recommended approach is to add MOMOIRO as a first-class era, not as a KIMIDORI or Murasaki alias. Build the route matrix and protocol limit evidence first from `proto/momoiro`, `Host/wwwroot/data/momoiro/data`, `.tools/momoiro/EBOOT.ELF.i64`, logs/captures, and cabinet/RPCS3 behavior. Public wiki facts are useful only as secondary scoping context for MOMOIRO 0.11; local proto, data, binary, generated wire, logs, and cabinet evidence outrank them for every implementation decision.

The main risks are route/proto mismatch, copied adjacent-era limits, wrong crown placement, incorrect root data layout, cross-era writes, and overclaiming compatibility from build/tests alone. Mitigation is straightforward but non-negotiable: require proto plus binary/client evidence for routes, prove changed limits before byte-packed runtime behavior, put crowns in `userdata.php` through `hash_crown_flg`, keep gameplay state MOMOIRO-owned, inspect Mapperly generated source, and keep cabinet/RPCS3 acceptance separate from automated verification.

## Key Findings

### Recommended Stack

Use the existing TaikoLocalServer stack. MOMOIRO needs new era artifacts, not new infrastructure: an adapter project, generated wire DTOs, controllers, mappers, catalog bindings, EF entities/tables, AdminApi/WebUI era routing, and focused verification.

**Core technologies:**
- .NET SDK `10.0.100` / `net10.0`: existing repo target; changing it adds risk without solving a MOMOIRO issue.
- ASP.NET Core MVC: cabinet `.php` routes, Host composition, and AdminApi hosting.
- EF Core SQLite: MOMOIRO-owned save, score, favorite, recent, unlock, reward, Dan, and compatibility state where proven.
- `protobuf-net` and local `protogen`: generate tracked `Adapters.GameProtocol.Momoiro/Wire` DTOs from `proto/momoiro`.
- Mapperly `4.3.1`: source-generated projections only; inspect emitted `.g.cs` for nontrivial MOMOIRO mappings.
- Mediator `3.0.2`: controllers deserialize/map, call Application handlers, and map back.
- Blazor WebAssembly + MudBlazor: extend existing era-routed admin surfaces only after backend state exists.

**Critical version/tooling requirements:**
- Preserve direct-protobuf transport unless current MOMOIRO client evidence proves otherwise.
- Generate from `proto/momoiro/taiko.proto` and `proto/momoiro/vsinterface.proto` using the repo-local protogen workflow with `+nullablevaluetype=yes`.
- Do not edit `proto/momoiro` or hand-edit generated `Wire/` files.
- Verify Mapperly output with `dotnet build ... /p:EmitCompilerGeneratedFiles=true` and inspect generated sources.

### Expected Features

MOMOIRO launch scope should implement only route/proto/data/binary-supported feature families. No Banacoin, Tokkun, battle, tournament, gacha, WaiWai, Taikojuku, event-folder, Don Challenge, or newer item-shop family should be added unless later local evidence proves both protocol shape and route/client behavior.

**Must have (table stakes):**
- First-class `GameEra.Momoiro`, settings, Host registration, disabled-era gating, generated wire DTOs, and adapter project.
- Shared `/v01r00/chassis/*.php` startup/version flow and `/v04r00/chassis/*.php` MOMOIRO game routes.
- Root-level catalog loading from `Host/wwwroot/data/momoiro/data` using `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- BAID/MyDon/profile creation and readback through MOMOIRO-owned save data.
- `userdata.php`, `playresult.php`, `selfbest.php`, favorites/recent, score/best, song unlock/readback, Don Point/reward fields, and Dan fields where binary-backed limits and semantics are proven.
- Crowns inside `UserDataResponse.hash_crown_flg`; do not add a standalone `crownsdata.php`.
- AdminApi/WebUI readback for implemented MOMOIRO-owned state.

**Should have (after proof or as bounded compatibility):**
- `songhash`, `defaultsong`, `mainichisong`, telop, recommendation, bookkeeping, communication log, heartbeat, and best-score compatibility where route and payload evidence exists.
- Shopping-result compatibility for Don Point totals and release-song flags after binary/client proof.
- Challenge-shaped arrays only as bounded protocol/readback data if proven; do not promote them into a new challenge product.
- AdminApi/WebUI editing for packed fields only after limits and packing are stable.

**Defer (v2+ or separate evidence milestone):**
- Later MOMOIRO versions beyond 0.11.
- Standalone challenge routes, Taikojuku, live-service shop/gasha/Donder Hiroba behavior, battle, Tokkun, Banacoin, tournament, newer item-shop authority, and unsupported folder surfaces.

### Architecture Approach

Use adapter-owned wire/routes, Application-owned behavior, Infrastructure-owned data/catalog persistence, and existing era-routed AdminApi/WebUI integration. Shared AC15 logic is acceptable only as switch-free capability composition after MOMOIRO evidence supplies its own profile, limits, catalog snapshot, and concrete DbSets.

**Major components:**
1. `Adapters.GameProtocol.Momoiro`: `/v04r00/chassis/*.php` game controllers, direct-protobuf transport, generated `Wire/Game.cs` and `Wire/VsInterface.cs`, Mapperly edge mappers, and route prefix constants.
2. `Adapters.GameProtocol.Shared`: existing `/v01r00/chassis/*.php` startup/version routes; MOMOIRO should not duplicate these under `/v04r00`.
3. `Application/Handlers/*.Momoiro.cs`: BAID, MyDon, userdata, playresult, self-best, catalog metadata, rewards/unlocks, and Dan orchestration over MOMOIRO-owned data.
4. `Application/Ac15`: shared profile-driven capability modules, including explicit MOMOIRO crown placement in userdata and MOMOIRO-specific limits.
5. `Domain` / `Infrastructure`: `UserSaveDataMomoiro`, MOMOIRO score/best/favorite/recent/Dan/unlock rows, typed DbSets, migration, and `IMomoiroCatalog`.
6. `Infrastructure/GameDataCatalog/Momoiro`: root-level path helpers, required-file checks, catalog loader, and MOMOIRO sidecar JSON where needed.
7. `Adapters.AdminApi` / `TaikoWebUI`: era-routed readback and editing only for implemented MOMOIRO-owned surfaces.

**Key patterns:**
- A supported route needs both `proto/momoiro` shape and MOMOIRO binary/log/cabinet route evidence.
- Catalog paths must resolve through era data helpers, not hardcoded `wwwroot/data/momoiro/data` strings in handlers.
- Generated wire DTOs stay adapter-local; persistence never stores wire types directly.
- Absence is architecture: unsupported feature families remain absent, not stubbed as if implemented.

### Critical Pitfalls

1. **Copying KIMIDORI or Murasaki behavior:** Avoid by creating MOMOIRO-owned profile, limits, wire, tables, handlers, and evidence notes before runtime behavior.
2. **Treating wiki context as protocol authority:** Avoid by marking wiki facts LOW confidence and requiring local proto plus binary/log/cabinet proof for implementation.
3. **Missing crowns-in-userdata semantics:** Avoid `crownsdata.php`; place crown bytes in `UserDataResponse.hash_crown_flg` after binary-backed packing proof.
4. **Guessing unlock/hash/crown limits:** Avoid by producing a protocol-limits artifact for byte widths, indexing, default fill, and route readback before stateful implementation.
5. **Assuming later `config/STxxxx-*` data roots:** Avoid by implementing a MOMOIRO root-level loader and tests over the real inventory shape.
6. **Accidental cross-era gameplay writes:** Avoid by adding MOMOIRO-owned entities/DbSets/migrations and no-cross-era persistence tests around runtime writes and AdminApi edits.
7. **Overclaiming runtime verification:** Avoid by separating build/test/generated-source evidence from cabinet/RPCS3 acceptance.

## Implications for Roadmap

Suggested phase numbering starts at Phase 39 unless `.planning/ROADMAP.md` changes before roadmap creation.

### Phase 39: MOMOIRO Evidence and Era Foundation

**Rationale:** This must come first because route ownership, direct-protobuf transport, changed limits, and supported route suffixes are evidence gates for all later work.
**Delivers:** MOMOIRO evidence matrix, route/proto matrix, generated wire DTOs, `GameEra.Momoiro`, adapter project, `/v04r00/chassis` prefix, Host registration, settings, disabled-era gating, direct-protobuf fallback, and initial Mapperly generated-source inspection.
**Addresses:** First-class era foundation, shared `/v01r00` startup/version, `/v04r00` game route skeleton.
**Avoids:** Route/proto mismatch, wiki-driven implementation, adjacent-era copy behavior, Mapperly blind spots.
**Research flag:** Needs `$gsd-plan-phase --research-phase 39`. IDA/log route inventory and changed-limit evidence are required, not optional.

### Phase 40: MOMOIRO Protocol Limits and Root Catalog Binding

**Rationale:** Catalog ordering and protocol limits drive crowns, unlock flags, favorites/recent, default/mainichi hashes, song hash tables, and safe WebUI editing.
**Delivers:** `IMomoiroCatalog`, root-level path helpers, required-file validation, root-level catalog loader, MOMOIRO sidecar naming, `Ac15EraProfiles.Momoiro`, protocol-limits artifact, catalog snapshot binding, and explicit crown placement profile.
**Addresses:** Root-level catalog loading, song/crown/unlock limit proof, favorites/recent limits, no later-era config root.
**Avoids:** Wrong data root, hardcoded `config/STxxxx-*`, guessed byte widths, copied `CreateCommonLimits()` behavior.
**Research flag:** Needs `$gsd-plan-phase --research-phase 40` for binary-backed limits and byte-payload contracts.

### Phase 41: MOMOIRO Catalog and No-State Route Readback

**Rationale:** The cabinet often needs metadata and compatibility routes before state mutation is complete, but these can be bounded to catalog/log/readback behavior.
**Delivers:** Proven controllers/handlers for heartbeat, bookkeeping, communication log, `defaultsong`, `mainichisong`, `songhash`, telop check/get, recommend, and other catalog-backed routes where route proof exists.
**Addresses:** Metadata routes, safe compatibility responses, catalog-backed payloads.
**Avoids:** Early state mutation, unsupported feature-family stubs, best-score/ranking assumptions before semantics are proven.
**Research flag:** Use focused research only for byte payloads that were not resolved in Phase 40; otherwise this follows standard adapter patterns.

### Phase 42: MOMOIRO Identity, Userdata, Self-Best, and Crown Readback

**Rationale:** User identity and readback are the core cabinet loop and must be correct before normal play writes can be meaningfully verified.
**Delivers:** MOMOIRO save/best/play/favorite/recent tables, BAID/MyDon/profile defaults, `userdata.php`, `selfbest.php`, canonical userdata adapter, crown bytes in `hash_crown_flg`, and readback tests over MOMOIRO-owned state.
**Addresses:** BAID/MyDon/profile, userdata, self-best, favorites/recent readback, crowns in userdata.
**Avoids:** Fake crown endpoint, cross-era reads/writes, null or wrong-width crown/release flags, AdminApi exposure before backend state exists.
**Research flag:** Needs `$gsd-plan-phase --research-phase 42` if crown packing, recent/favorite counts, or self-best response limits remain unresolved.

### Phase 43: MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility

**Rationale:** Stateful writes have the highest corruption risk and depend on Phase 40 limits plus Phase 42 readback paths.
**Delivers:** Normal playresult classification, score/best/crown/favorite/recent writes, release-song mutation and readback, Don Point/reward totals, shopping-result compatibility, bounded challenge-array handling, and Dan/Dani persistence only where evidence proves semantics.
**Addresses:** Normal play persistence, song unlocking, rewards, shopping result, Dan fields, challenge-compatible arrays.
**Avoids:** New feature families, newer item-shop authority, challengecompe/Don Challenge invention, KIMIDORI/Murasaki behavior copy, cross-era writes.
**Research flag:** Needs `$gsd-plan-phase --research-phase 43` for unlock mutation, shopping-result effects, Don Point cap, challenge arrays, and Dan result semantics.

### Phase 44: MOMOIRO AdminApi and WebUI Routing

**Rationale:** UI should trail backend behavior so it exposes only state the cabinet actually reads or writes.
**Delivers:** `/api/momoiro/...` AdminApi partials, `WebUiEra.Momoiro`, era selector support, profile/play-data/favorites/history/leaderboard/customization/Dan surfaces where implemented, and hidden unsupported panels.
**Addresses:** AdminApi/WebUI readback and selective editing for implemented MOMOIRO-owned state.
**Avoids:** UI for absent Tokkun/Banacoin/battle/Taikojuku/Don Challenge/folder features, editing packed fields before limits are stable.
**Research flag:** Standard patterns; skip research-phase unless Phase 43 leaves unresolved packed-field edit semantics.

### Phase 45: MOMOIRO Verification and Acceptance

**Rationale:** AC15 cabinet support is not complete until automated checks and runtime evidence are both recorded.
**Delivers:** focused tests, full solution build, temp-output Host build if needed, Mapperly generated-source review, route smoke/log evidence, startup/login/userdata/catalog/playresult/self-best/crown/AdminApi/WebUI verification, and cabinet/RPCS3 acceptance notes.
**Addresses:** closeout quality, runtime acceptance, known manual gates.
**Avoids:** claiming cabinet compatibility from build/tests alone.
**Research flag:** Standard verification workflow, but cabinet/RPCS3 evidence remains a manual/runtime gate.

### Phase Ordering Rationale

- Evidence and route/proto proof must precede implementation because MOMOIRO scope is explicitly local-evidence driven.
- Catalog and limits come before userdata/playresult because song ordering, byte widths, crown packing, favorites/recent limits, and unlock hashes depend on them.
- No-state metadata routes can be built before persistence, but only after route evidence and catalog data shape are known.
- Readback comes before writes so playresult changes can be verified through the same cabinet-consumed surfaces.
- AdminApi/WebUI trails backend behavior to avoid exposing unsupported or corrupting controls.
- Verification is its own closeout because build/test evidence and cabinet/RPCS3 acceptance answer different questions.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 39:** IDA route suffixes, `/v01r00` versus `/v04r00` ownership, and transport proof.
- **Phase 40:** binary-backed protocol limits, byte widths, indexing, and catalog ordering.
- **Phase 42:** crown-in-userdata packing, self-best limits, favorites/recent limits.
- **Phase 43:** release-song mutation, shopping result, Don Point caps, challenge arrays, and Dan semantics.

Phases with standard patterns where research can usually be skipped:
- **Phase 41:** standard controller/handler/catalog route pattern once route and payload evidence exist.
- **Phase 44:** established AdminApi/WebUI era-routing pattern, unless packed-field editing remains unresolved.
- **Phase 45:** established verification/closeout mechanics, with cabinet/RPCS3 as a manual gate.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Current repo stack, package versions, local protogen, generated-wire convention, and Mapperly workflow are well established. |
| Features | MEDIUM | Proto and local data define candidate surfaces, but route inventory, changed limits, crown packing, unlock behavior, and cabinet acceptance still need local binary/log/runtime evidence. |
| Architecture | HIGH | Existing TaikoLocalServer era architecture directly supports a first-class MOMOIRO adapter, MOMOIRO-owned persistence, root-level catalog loader, and shared AC15 capability composition. |
| Pitfalls | MEDIUM | Repo/proto/data pitfalls are high confidence; binary-specific route/limit pitfalls are intentionally gated until IDA/runtime research is recorded. |

**Overall confidence:** MEDIUM

### Gaps to Address

- **Route inventory:** Extract and record `/v04r00/chassis/*.php` and shared `/v01r00/chassis/*.php` route evidence from `.tools/momoiro/EBOOT.ELF.i64`, logs, captures, or cabinet/RPCS3 traces.
- **Protocol limits:** Prove byte widths, indexing, defaults, and readback routes for `hash_crown_flg`, `hash_release_song_flg`, `song_hash_tbl`, default/mainichi flags, favorites/recent, Don Point, and related arrays.
- **Crowns in userdata:** Confirm packing, song count, difficulty placement, ura/shin handling, and absent/default behavior before declaring crown support complete.
- **Unlocks and shopping result:** Prove how `release_song_no`, `hash_release_song_flg`, `song_hash_ver`, `use_donpoint`, `total_get_donpoint`, `total_use_donpoint`, and shopping song arrays interact.
- **Challenge and Dan semantics:** Treat arrays/fields as protocol facts only until client/binary evidence proves persistence, readback, and mode classification.
- **Cabinet acceptance:** Automated tests and builds are regression guards; runtime compatibility needs captured client behavior or user-observed cabinet/RPCS3 acceptance.

## Sources

### Primary (HIGH confidence)
- `.planning/research/STACK.md`: stack recommendation, package/tool versions, protogen workflow, Mapperly verification, data inventory, and evidence hierarchy.
- `.planning/research/FEATURES.md`: table-stakes features, anti-features, dependencies, MVP shape, and research gaps.
- `.planning/research/ARCHITECTURE.md`: first-class MOMOIRO architecture, component responsibilities, data flow, reuse boundaries, and phase order.
- `.planning/research/PITFALLS.md`: critical risks, prevention strategies, integration gotchas, and phase mapping.
- `.planning/PROJECT.md`: active v1.7 MOMOIRO scope and user constraints as cited by the research files.
- `proto/momoiro/taiko.proto`: local MOMOIRO game protocol message inventory, including `UserDataResponse.hash_crown_flg`.
- `proto/momoiro/vsinterface.proto`: local startup/version message inventory.
- `Host/wwwroot/data/momoiro/data`: root-level MOMOIRO data layout and required raw files.
- `.tools/momoiro/EBOOT.ELF.i64`: local binary evidence source for route inventory, limits, unlocks, and crown packing research.

### Secondary (MEDIUM confidence)
- Existing KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, and shared AC15 adapter/application/infrastructure patterns: architecture precedent only, not MOMOIRO behavior proof.
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` and `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md`: shared AC15 capability and boundary guidance.
- Current Mapperly and protobuf-net documentation as cited in research: tooling guidance, with final mapper authority coming from emitted repo generated source.

### Tertiary (LOW confidence)
- WikiWiki AC15 history and MOMOIRO update pages: secondary release/version scoping context only. They do not prove cabinet routes, field placement, byte widths, persistence semantics, or TaikoLocalServer feature scope.

---
*Research completed: 2026-06-25*
*Ready for roadmap: yes*
