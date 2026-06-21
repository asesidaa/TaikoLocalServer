# Project Research Summary

**Project:** TaikoLocalServer AC15 Era Support - v1.5 Murasaki AC15 Support  
**Domain:** Brownfield ASP.NET Core cabinet protocol adapter, catalog, persistence, and AdminApi/WebUI milestone  
**Researched:** 2026-06-21  
**Confidence:** MEDIUM

## Executive Summary

Murasaki should be added as a first-class AC15 era in the existing TaikoLocalServer process, not as a White variant and not as a new runtime stack. The recommended approach is to reuse the established ASP.NET Core 10 host, EF Core SQLite persistence, protobuf-net generated wire DTOs, Mapperly source-generated projections, Mediator handlers, MudBlazor Admin UI, and existing AC15 capability services only where local Murasaki proto/data/client evidence proves compatible semantics.

The strongest architectural conclusion is that Murasaki has its own protocol shape. Startup/version traffic stays on shared `/v01r00/chassis/*`, game traffic is expected on `/v06r00/chassis/*.php`, request bodies remain direct protobuf, and the active catalog root is `ST6100-1`. Murasaki lacks White-style `initialdatacheck.php`; metadata must be modeled as first-class split endpoints such as `defaultsong`, `mainichisong`, `foldercheck/getfolder`, `telopcheck/gettelop`, `songhash`, and `bestscore`.

The main risk is overreach: proto names and White similarity make it easy to invent unsupported global high-score, song-hash, shopping, challenge, or reserved-byte semantics. Mitigate this by separating evidence capture from runtime implementation, keeping state Murasaki-owned, inspecting Mapperly generated source for nontrivial mappings, and closing only after automated verification plus user-observed cabinet/RPCS3 and WebUI acceptance.

## Key Findings

### Recommended Stack

No new runtime stack is warranted. Murasaki is a brownfield adapter/catalog/persistence milestone inside the current .NET solution. New work should be project, wire, catalog, state, mapper, handler, and UI registration work, with `ida-cli` used only for binary evidence extraction from `.tools/murasaki/EBOOT.ELF.i64`.

**Core technologies:**
- ASP.NET Core 10 / C# 13: existing host, controller, DI, routing, and static WebUI foundation.
- EF Core SQLite: existing local persistence model; add Murasaki-owned entities/tables instead of sharing White/Red state.
- protobuf-net and `protogen`: direct-protobuf cabinet transport with adapter-local generated DTOs from `proto/murasaki`.
- Mapperly 4.3.1: source-generated protocol-to-application projections; inspect emitted `.g.cs` when mappings are nontrivial.
- Mediator: existing Application handler dispatch boundary; controllers should deserialize, map, send, and map back.
- MudBlazor / Blazor WebAssembly: extend existing era-routed Admin UI only for implemented Murasaki-owned state.
- `ida-cli`: planning/evidence tooling only, not a runtime dependency.

### Expected Features

**Must have for v1.5 launch:**
- First-class `GameEra.Murasaki`, adapter project, Host gating, generated Murasaki wire DTOs, and direct-protobuf `.php` route handling.
- Evidence-backed route/root baseline: shared startup `/v01r00`, Murasaki game `/v06r00`, active `ST6100-1`, and no White-style `initialdatacheck.php`.
- Murasaki catalog/profile binding over `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `fumen/tuning.bin`.
- BAID, mydon entry, userdata, normal profile readback, favorites/recents, self-best, crowns, normal playresult, reward/Don Point fields, and Dani/Taikojuku where Murasaki evidence matches existing AC15 behavior.
- Split metadata endpoints as first-class Murasaki work: default song, mainichi song, folder, telop, recommendation/readiness, and related catalog-backed responses.
- AdminApi/WebUI parity only for state actually implemented and persisted as Murasaki-owned data.

**Should have after the core loop is stable:**
- Evidence-backed song-hash/default/mainichi byte generation.
- Real `bestscore.php` or global/local ranking behavior after route, sequence, scope, and ranking semantics are proven.
- Proven `shoppingresult.php` mutation only after Don Point spending/unlock authority is understood.
- Murasaki challenge or Don Challenge-like behavior only after local Murasaki data and client read/write semantics prove a server contract.

**Defer or keep absent unless evidence appears:**
- Banacoin wallet/payment, Tokkun, Blue battle, Yellow shop seasons, White-final-only behavior, standalone Red-style `challengecompe.php`, and any invented reserved-byte or content-info semantics.

### Architecture Approach

Murasaki should follow the existing era composition pattern: adapter-local wire DTOs and controllers at the edge, Application handlers and AC15 capability services in the middle, and Murasaki-owned EF/catalog infrastructure underneath. Capability reuse is encouraged, but only through Murasaki mappers, Murasaki catalog/profile inputs, and typed Murasaki persistence.

**Major components:**
1. `Adapters.GameProtocol.Murasaki` - `/v06r00/chassis/*.php` game routes, generated `Wire/`, Mapperly mappers, and direct-protobuf controller edge.
2. Shared `/v01r00/chassis/*` startup/version routes - reused only where `vsinterface` and client evidence confirm compatibility.
3. `Application/Handlers/*.Murasaki.cs` and `Application/Ac15` - Murasaki behavior dispatch plus capability-owned AC15 reuse.
4. `Infrastructure/GameDataCatalog/Murasaki` - active `ST6100-1` catalog loading and required-file validation.
5. `Domain` and EF model - `UserSaveDataMurasaki` and Murasaki-owned score, play, favorite, recent, Dani, reward, and later proven capability tables.
6. `Adapters.AdminApi` and `TaikoWebUI` - era-routed operator surfaces for implemented Murasaki state only.

### Critical Pitfalls

1. **Treating Murasaki as renamed White** - avoid by building Murasaki wire/controllers/state and implementing split metadata instead of copying White `initialdatacheck.php`.
2. **Guessing route prefixes or catalog roots** - avoid by recording route/root evidence before runtime binding; current research points to `/v01r00`, `/v06r00`, `.php` suffixes, and `ST6100-1`.
3. **Treating unknown byte fields as flags** - avoid by evidence-gating `song_hash_tbl`, default/mainichi hashes, release/crown flags, `content_info`, `default_option_setting`, and `reserved`.
4. **Conflating global best score with self-best** - keep `bestscore`/song-hash/global ranking as a separate capability; do not fill it from per-user self-best or fake local data.
5. **Sharing White/Red persistence or wire DTOs** - keep Murasaki state and generated wire local to Murasaki, sharing only proven algorithms through explicit capability inputs.
6. **Overclaiming verification** - distinguish build/test/generated-source proof from cabinet/RPCS3 and WebUI acceptance.

## Implications for Roadmap

Suggested phase numbering continues after v1.4, likely starting at Phase 28 unless the roadmapper chooses otherwise from current MILESTONES/ROADMAP state.

### Phase 28: Murasaki Evidence and Era Foundation

**Rationale:** Route, root, transport, and wire ownership decisions determine every later feature. They must be locked before persistence or catalog behavior spreads through the codebase.  
**Delivers:** Murasaki evidence artifact, `GameEra.Murasaki`, generated adapter-local wire DTOs, adapter project, route prefix constants, Host registration/gating, exact content-type fallback, and no-state probes for proven suffixes.  
**Addresses:** First-class era foundation; `/v01r00` startup and `/v06r00` game route baseline; `.php` direct-protobuf shape; active `ST6100-1` decision.  
**Avoids:** White-variant assumptions, route/root guessing, generated-wire edits, and broad route fallback.

### Phase 29: Catalog and AC15 Profile Binding

**Rationale:** Catalog/profile facts drive response sizes, favorites, rewards, Dani, folders, telops, and byte packing. This should precede runtime state.  
**Delivers:** `IMurasakiCatalog`, `MurasakiGameDataPaths`, required-file validation for `ST6100-1`, `Ac15EraProfiles.Murasaki`, catalog snapshot binding, Murasaki limits, and any committed empty/default sidecars required by existing packaging patterns.  
**Uses:** Existing filesystem catalog loaders, PathHelper/era data path patterns, EF/Host settings, and AC15 profile composition.  
**Avoids:** Selecting `ST5100-*` by directory presence, hardcoded `wwwroot` access in handlers, and unbounded byte helper reuse.

### Phase 30: Split Metadata Readback

**Rationale:** Murasaki metadata is split across endpoint families and may be required before normal play can progress. This is the biggest protocol difference from White.  
**Delivers:** First-class Murasaki handlers/mappers/routes for `defaultsong`, `mainichisong`, `foldercheck`, `getfolder`, `telopcheck`, `gettelop`, and related catalog-backed readiness endpoints where evidence supports them.  
**Addresses:** No `initialdatacheck.php`, feature folders, telops, default/mainichi song readback, recommendation/readiness surfaces.  
**Avoids:** Faking a White initial-data contract or inventing song-hash byte semantics beyond evidence.

### Phase 31: Identity, Userdata, Self-Best, and Normal Read Paths

**Rationale:** Once catalog and metadata are stable, the server can safely expose card/profile state without writing playresult mutations yet.  
**Delivers:** Murasaki-owned save defaults, BAID, mydon entry, userdata, self-best, crowns, favorites/recents, profile options/readback, and AdminApi read paths for implemented state.  
**Uses:** Existing AC15 normal-userdata algorithms through Murasaki mappers, Murasaki catalog snapshot, and Murasaki EF rows.  
**Avoids:** Cross-era reads, unsupported UI controls, and wire DTO leakage into Application.

### Phase 32: Normal Playresult, Dani, Reward, and Don Point Mutation

**Rationale:** Mutating flows should come after readback contracts and no-cross-era boundaries are in place.  
**Delivers:** Murasaki playresult classification, score/best/crown/favorite/recent writes, profile counters, Dani/Taikojuku persistence where proven, reward progress, Don Point totals, focused persistence tests, and no-cross-era regression coverage.  
**Addresses:** Normal cabinet play loop and server-owned state updates.  
**Avoids:** Writing White/Red tables, treating unsupported modes as normal play, and mutating shopping/challenge state from proto names alone.

### Phase 33: Evidence-Gated Special Capabilities

**Rationale:** Global high-score, song-hash, shopping, challenge arrays, and reserved bytes are high-risk surfaces that need their own evidence pass after core compatibility works.  
**Delivers:** Targeted binary/log/cabinet evidence and, only where proven, implementation for `songhash`, `bestscore`, shopping result mutation, local ranking, challenge arrays, or raw byte preservation.  
**Addresses:** Later capability work without conflating it with self-best or fake data.  
**Avoids:** Fake global rankings, guessed byte layouts, and accidental currency/unlock authority.

### Phase 34: AdminApi/WebUI and Runtime Closeout

**Rationale:** Operator surfaces should reflect implemented server behavior, and final support requires both automated and user-observed verification.  
**Delivers:** Murasaki WebUI era support for implemented profile/score/favorite/Dani/reward state, API parity, docs, focused tests, solution/Host builds, Mapperly generated-source inspection, and recorded cabinet/RPCS3 plus WebUI acceptance.  
**Avoids:** Exposing unsupported Don Challenge/global-score/shopping controls and overclaiming runtime verification from tests alone.

### Phase Ordering Rationale

- Route/root/wire foundation comes first because mistakes there would force broad controller, catalog, and persistence rework.
- Catalog/profile work comes before runtime writes because Murasaki limits, `ST6100-1`, and byte widths affect every response.
- Split metadata is isolated because it is the major Murasaki-specific protocol difference and should not be hidden inside White/Red-style initial-data code.
- Read paths precede mutating playresult support so no-cross-era state boundaries can be tested before writes are introduced.
- Special capabilities are late because research agrees they are evidence-gated and must not be treated as self-best, fake rankings, or guessed shopping state.

### Research Flags

Phases likely needing `$gsd-plan-phase --research-phase <N>`:
- **Phase 28:** Route table, prefix, suffix allowlist, direct-protobuf behavior, and active-root proof are foundational.
- **Phase 30:** Split metadata byte payloads and endpoint sequencing need route/log/client evidence.
- **Phase 33:** Global high-score/song-hash/shopping/challenge/reserved-byte semantics are unresolved and high risk.
- **Phase 32:** Use research only if playresult classification or Dani/reward field semantics remain unclear after Phases 28-31.

Phases with standard patterns where a full research phase can usually be skipped:
- **Phase 29:** Catalog/profile binding follows established AC15 patterns once `ST6100-1` is fixed.
- **Phase 31:** Identity/userdata/self-best readback follows existing AC15 capability composition with Murasaki-owned state.
- **Phase 34:** AdminApi/WebUI closeout follows prior Blue/Yellow/Red/White patterns, though manual acceptance remains required.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Existing repo stack and package versions are clear; no new runtime libraries are justified. |
| Features | MEDIUM | Proto/data evidence is strong for message inventory and visible flows, but byte semantics, shopping, global score, and challenge behavior remain unresolved. |
| Architecture | MEDIUM | Brownfield patterns are well established and Murasaki IDA/data evidence supports `/v06r00`, `/v01r00`, and `ST6100-1`; some route suffix and runtime semantics still need cabinet/log proof. |
| Pitfalls | MEDIUM | Risks are consistent with prior AC15 milestones and current Murasaki research, but several mitigations depend on future evidence artifacts. |

**Overall confidence:** MEDIUM

### Gaps to Address

- Final route suffix allowlist and call ordering: prove with IDA route tables, request logs, captures, or cabinet/RPCS3 behavior.
- Byte table shapes: define length, bit ordering, defaults, and consumer behavior for song-hash, default/mainichi, release-song, crown, content, option, and reserved fields before modeling semantics.
- `bestscore` and global/local ranking: prove sequence, scope, ranking rows, defaults, and whether `localranking.bin` is involved.
- `shoppingresult` and Don Point mutation: prove whether the server is authoritative before changing unlocks or balances.
- Challenge arrays and Don Challenge-like behavior: keep compatibility/defaults until Murasaki-specific data and client read/write behavior prove more.
- Runtime acceptance: automated tests/builds are not enough; cabinet/RPCS3 and WebUI closeout must be recorded separately.

## Sources

### Primary (HIGH confidence)

- `.planning/PROJECT.md` - active v1.5 Murasaki scope, constraints, prior milestone context, and evidence hierarchy.
- `.planning/research/STACK.md` - no-new-stack decision, versions, tooling, and Mapperly/protobuf requirements.
- `.planning/research/ARCHITECTURE.md` - first-class Murasaki adapter architecture, route/root evidence, split metadata pattern, and phase order.
- `.planning/research/PITFALLS.md` - critical risks and phase-level mitigations.
- `proto/murasaki/taiko.proto` and `proto/murasaki/vsinterface.proto` - local protocol inputs and split request-family evidence.
- `Host/wwwroot/data/murasaki/data/config` - local roots including `ST5100-1`, `ST5100-7`, and `ST6100-1`.
- `.tools/murasaki/EBOOT.ELF.i64` evidence reported by architecture research - route/root string and IDA-backed active-root leads.

### Secondary (MEDIUM confidence)

- `.planning/research/FEATURES.md` - feature prioritization, MVP definition, anti-features, and dependencies.
- Existing Blue/Yellow/Red/White adapters, handlers, catalogs, and AdminApi/WebUI patterns - implementation precedent for first-class AC15 eras.
- AC15 capability composition specs under `docs/superpowers/specs/` - approved reuse model: share behavior, not routes, wire, or persistence.

### Tertiary (LOW confidence)

- Public wiki/product context cited by feature research - useful for scoping Murasaki-visible features such as favorite cap, Don Point cap, and feature folders, but not authoritative for server route, wire, byte, or persistence behavior.

---
*Research completed: 2026-06-21*  
*Ready for roadmap: yes*
