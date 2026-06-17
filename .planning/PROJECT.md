# TaikoLocalServer AC15 Era Support

## What This Is

TaikoLocalServer is a local ASP.NET Core server for Taiko no Tatsujin cabinet protocols, local SQLite persistence, era-specific game data catalogs, and a Blazor WebAssembly admin UI. This project continues the existing Blue-era support effort from the Superpowers roadmap in `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`, starting after completed stages A0-A5 and carrying the work through full Blue support.

Full Blue, Yellow, and Red support are complete. White 0.13 support is the active v1.4 milestone. The project now supports Nijiiro, Green AC15, Blue AC15, Yellow AC15, and Red AC15 in one process while preserving era-owned routes, wire DTOs, persistence, handlers, mappers, catalogs, tests, and AdminApi/WebUI routing.

## Core Value

AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Blue, Green, Yellow, Nijiiro, or shared identity state.

## Current State

v1.2 Yellow AC15 Support shipped on 2026-06-12. Yellow is a first-class AC15 era using `proto/yellow/yellow.proto`, generated Yellow wire DTOs, Yellow-owned persistence, and `Host/wwwroot/data/yellow/data`.

v1.3 Red AC15 Support shipped on 2026-06-16. Red uses `proto/red/taiko.proto`, `proto/red/vsinterface.proto`, and the local Red game-data symlink at `Host/wwwroot/data/red/data` as source evidence. Red support covers `/v08r01` game routes, shared `/v01r00` startup/version ownership, active `ST8100-1` root evidence, first-class Red adapter/wire/Host gating, catalog/profile binding, Red-owned normal runtime state, Dani, tutorial-only Tokkun readback, simple compatibility routes, shared older-AC15 ChallengeCompe/Don Challenge behavior, AdminApi/WebUI readback, and user-accepted runtime closeout.

v1.4 White AC15 0.13 Support has started. Phase 23 is complete: White route/root evidence is recorded, `/v07r00/chassis` is approved for fourteen no-state scaffold routes, startup/version ownership remains shared under `/v01r00/chassis`, and White now has first-class adapter/wire/Host gating foundation. A 2026-06-18 live log showed the local setup returning 405 while White was disabled; the setup was fixed by enabling White for the source Host setup, mapping `HddVer=700` to White, and recording user-confirmed cabinet/RPCS3 connection smoke before Phase 24. White uses `proto/white/taiko.proto`, `proto/white/vsinterface.proto`, and local game data at `Host/wwwroot/data/white/data` as source evidence, with the observed data root `config/ST7100-1`. The milestone targets the first White version, so later White-version behavior must be absent unless local White proto/data/log/IDA/client evidence proves it belongs in the 0.13 server contract.

Although Blue, Green, and Yellow wire surfaces contain challenge competition proto definitions and some compatibility routes, those newer versions do not meaningfully call the feature; do not treat their stubs as runtime behavior evidence. Challenge competition is meaningful scope only for Red and older AC15 versions.

Blue now supports normal, battle, and Tokkun play in the same process. Tokkun support is bounded to evidence-backed Blue protocol behavior: stateless Banacoin-adjacent compatibility, Tokkun playresult acceptance, Blue-owned raw Tokkun persistence, nullable tutorial readback, final contract documentation, and user-confirmed cabinet/RPCS3 runtime verification.

Yellow support reuses AC15 shared core behavior where it directly reduces duplicated normal-play, catalog, score, crown, Dani, shop, Tokkun, and admin behavior without merging era state or inventing unsupported routes.

## Current Planning State

Current milestone: **v1.4 White AC15 0.13 Support**

**Current milestone goal:** Add White 0.13 by composing existing AC15 capabilities with White config, limits, wire placement, typed persistence, collectable data, and White/older-version evidence while preserving era-owned protocol, catalog, state, admin, and verification boundaries.

## Requirements

### Validated

- [x] Existing multi-era TaikoLocalServer architecture: ASP.NET Core host, local SQLite persistence, game protocol adapters, era-aware application handlers, filesystem game-data catalogs, and Blazor WebAssembly admin UI.
- [x] Blue A0 evidence/bootstrap: Blue route prefix, direct protobuf transport assumptions, shared startup/verup route ownership, scoped missing-content-type fallback, and local Blue data layout are documented.
- [x] Blue A1 era foundation and adapter skeleton: Blue is a first-class enableable era with adapter project, generated wire types, route ownership, settings, dependency injection, and safe stub behavior.
- [x] Blue A2 catalog and data layout: Blue catalog interfaces, runtime data paths, AC15 loader reuse, startup validation, settings, and operator data documentation are established.
- [x] Blue A3 identity/profile/userdata: Blue card registration, login, default save state, initial data, and stable profile/userdata readback are implemented through Blue-owned state.
- [x] Blue A4 normal enso play result, self-best, crowns, and rewards: Blue normal song results persist and read back through Blue-specific handlers, mappers, byte helpers, and save state.
- [x] Blue A5 Dani Dojo: Blue taikojuku catalog responses, Dan result persistence, Dan readback, AdminApi support, and WebUI Dani behavior are implemented without reusing Green Dan state.
- [x] Blue A6 item shop and unlocking: Blue shop seasons, active shop selection, season-scoped medal state, purchases, rewardexecution no-op, configured item unlocks, and locked-item readback are implemented using Blue data and Blue save state.
- [x] Blue A7 basic AdminApi and WebUI parity: Blue profile, score history, favorites, Dani, customization, item-shop-relevant surfaces, and safe edit/readback behavior are exposed without writing Green state.
- [x] Blue A8 normal-mode cabinet smoke and hardening: normal Blue support has been user-confirmed complete before Track B battle evidence/design work.
- [x] Blue battle evidence and design: Phase 4 defined Track B from proto, local Blue battle XML inventory, IDA/client-equivalent evidence, and explicit field-width/default-state gates before runtime battle behavior.
- [x] Blue battle runtime support: Phase 5 added Blue-owned battle persistence, battleuserdata readback, data-derived initialdata battle advertisement, battle playresult persistence, reward/unlock store-echo behavior, and server-side verification without Green AI Battle dependencies.
- [x] Full Blue verification and release hardening: v1.0 closes with repeatable normal and battle smoke evidence externally confirmed by the user, full automated Blue/server verification, and updated operator/developer documentation.
- [x] Blue Tokkun evidence contract: Phase 7 defines evidence-tagged Tokkun protocol rows, classifier boundaries, no-runtime-write targets, and Phase 8-11 handoff gates before runtime Tokkun behavior changes.
- [x] Blue stateless Banacoin compatibility: Phase 8 adds Blue `getbanacoininfo.php` as a direct-protobuf, no-state compatibility route and verifies Banacoin-adjacent routes remain stateless and free of wallet/payment persistence.
- [x] Blue Tokkun playresult acceptance: Phase 9 classifies Tokkun uploads from `ary_tokkunstage_info`, preserves raw Tokkun stage facts, and returns success before normal, battle, Dani, favorite/recent, profile, unlock, medal, customization/title, or shop writes.
- [x] Blue Tokkun persistence and readback: Phase 10 adds `PlayMode.Tokkun = 3`, nullable raw `UserSaveDataBlue.TokkunTutorialFlg`, append-only `BlueTokkunStageResults`, Tokkun playresult persistence, and userdata tutorial readback without cross-mode state writes.
- [x] Blue Tokkun runtime verification and final contract: Phase 11 records full automated verification, user-confirmed cabinet/RPCS3 runtime proof, and the final route/state/semantic contract.
- [x] AC15 nullable wire generation and Mapperly projection: Phase 16.1 regenerates Green, Blue, and Yellow AC15 wire DTOs with nullable optional primitives where `protogen` supports them, and protocol mapping uses Mapperly for mechanical DTO projection with manual mapper code only for explicit protocol/domain transforms.
- [x] Yellow first-class era foundation: Yellow is an enableable first-class era with generated wire types, route ownership, host settings, direct-protobuf game transport, and shared AC15 startup/version routing where current client evidence supports it.
- [x] Yellow runtime catalog support: Yellow catalogs load from `Host/wwwroot/data/yellow/data`, including `config/ST9100-1/musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and supporting AC15 data through shared loaders where formats match.
- [x] Yellow normal cabinet flow: Yellow supports profile/login, BAID, mydon entry, userdata, initial data, self-best, crowns, recent/favorite songs, Dani Dojo, challenge/tournament/gacha surfaces, recommendations, telops, event folders, movies, and AdminApi/WebUI era routing where Yellow proto/data supports them.
- [x] Yellow item shop and medals: Yellow item-shop and medal behavior uses Yellow-owned state, including Don/Katsu medal accounting and Yellow shop response shape differences.
- [x] Yellow WaiWai handling: Current Yellow support is limited to tutorial flag write/readback and logging additional playresult fields; WaiWai is not treated as a special play mode.
- [x] Yellow Tokkun mode: Yellow has Yellow-owned Tokkun playresult classification, no-cross-mode write boundaries, nullable tutorial readback, and append-only raw protocol-backed history.
- [x] Yellow stateless Banacoin compatibility: Yellow Banacoin-adjacent routes are compatibility surfaces only and do not create wallet or payment authority.
- [x] Yellow no-battle absence contract: Blue-only battle behavior remains absent from Yellow; no Yellow battle routes, fields, persistence, or inferred runtime behavior are exposed without concrete Yellow evidence.
- [x] Yellow runtime verification and final contract: Phase 17 records full automated verification, user-confirmed RPCS3 runtime proof, and final route/state/semantic contract documentation.
- [x] Red evidence and first-class foundation: Phase 18 records Red `/v08r01` game route evidence, shared `/v01r00` startup/version ownership, active `ST8100-1` root evidence, Red enum/adapter/generated wire/Host gating, and no-state route probes with user-confirmed basic connection only.
- [x] Red catalog/profile binding: Red composes shared AC15 catalog/profile capabilities through Red config, limits, wire placement, and Red-owned sidecar data.
- [x] Red runtime state and readback: Red supports identity, userdata, initial data, self-best, crowns, recent/favorite songs, Dani Dojo, tutorial-only Tokkun, and simple compatibility routes through Red-owned state.
- [x] Red ChallengeCompe/Don Challenge: Red binds the shared older-AC15 ChallengeCompe capability through local sidecar data, Red-owned progress/reward state, cabinet `challengecompe.php` readback, and dedicated AdminApi/WebUI contracts.
- [x] Red AdminApi/WebUI and runtime closeout: Phase 22 records automated verification, temp-output Host build, and user-accepted manual runtime evidence before v1.3 archive.
- [x] White evidence and first-class foundation: Phase 23 records White `/v07r00/chassis` route evidence for fourteen no-state scaffold suffixes, shared `/v01r00/chassis` startup/version ownership, current nonzero White IDB evidence, `ST7100-1` root evidence, generated White wire DTOs, `GameEra.White`, Host settings/registration, exact content-type fallback, disabled-era application-part gating, the 2026-06-18 setup 405 fix, user-confirmed connection smoke, and no runtime/catalog/AdminApi/WebUI behavior.

### Active

- [ ] White binds only matching AC15 catalog/profile/runtime capabilities through White-owned persistence, protocol limits, and mechanical Mapperly projection.
- [ ] Features missing from White 0.13 remain absent unless local White evidence proves the cabinet expects compatibility behavior.
- [ ] White-scoped collectable data, including Don Challenge data if it falls inside the proven White 0.13 range, is collected and bound late in the milestone after core capability behavior is stable.
- [ ] White support closes only after automated verification, a temp-output Host build if needed, and cabinet/RPCS3 smoke evidence for implemented White flows.

### Out of Scope

- Real Banacoin balance, payment, settlement, receipt, coupon, deduction, BNID result, or transaction-history behavior beyond stateless compatibility routes needed for Blue Tokkun availability.
- AC15 versions earlier than White, or later White-version behavior beyond 0.13, unless started by a later milestone or pulled in by local White 0.13 evidence.
- Red WaiWai behavior; Red is planned as Yellow-like support without WaiWai unless local Red evidence proves otherwise.
- Yellow battle mode or Blue battle behavior mirrored into Yellow without concrete Yellow proto/log/client evidence.
- Blue battle behavior mirrored into Red without concrete Red proto/log/client evidence.
- White battle, item shop, Tokkun, gacha, tournament runtime, Banacoin wallet/payment, or later White update behavior without concrete White 0.13 proto/log/client evidence.
- Green AI Battle changes while implementing Blue battle mode; Green AI Battle is contrast material, not the Blue design source.
- Invented Tokkun rewards, score/crown persistence, paid-coin behavior, practice-time accounting, jump-point behavior, autoplay behavior, speed-change behavior, or song unlock side effects without concrete Blue evidence.
- Runtime scraping of wiki or official pages.
- Treating OCR output as authoritative official data without source/image provenance.
- Sharing Blue, Green, or Nijiiro persistence except for truly shared identity state.

## Context

- The current branch is `feat/green-version-support`; `.planning/codebase/` was generated on 2026-05-28 and describes the brownfield architecture.
- The solution uses .NET 10, ASP.NET Core, EF Core SQLite, protobuf-net, Mediator.SourceGenerator, MudBlazor, and xUnit.
- `Host/Program.cs` composes enabled era adapters and gates controller application parts so disabled-era routes are absent.
- `Domain/Enums/GameEra.cs`, `Application/Handlers/*.Blue.cs`, `Adapters.GameProtocol.Blue/`, `Infrastructure/GameDataCatalog/Blue/`, and `TaikoWebUI/Utilities/WebUiEra.cs` are key Blue support touch points.
- The Superpowers Blue roadmap split the effort into Track A normal support and Track B battle mode. A0-A5 are treated as completed prior work for this GSD project.
- Track A stages A6 item shop/unlocking, A7 AdminApi/WebUI parity, and A8 normal-mode cabinet smoke/hardening are complete.
- Track B battle mode is part of the full Blue project scope. Phase 4 produced strict battle design gates, and Phase 5 completed server-side runtime support using row-specific proof plus named user approvals where exact client defaults were not otherwise provable.
- v1.0 Blue support is shipped as of 2026-06-03. The user confirmed the remaining stale debug/UAT artifacts were externally fixed and resolved before milestone close.
- v1.1 Blue Tokkun support shipped on 2026-06-07. The user confirmed cabinet/RPCS3 runtime verification for Tokkun selection, Banacoin request sequence, gameplay entry, final upload, post-upload userdata behavior, and no unexpected blocking endpoint calls.
- v1.2 Yellow AC15 support shipped on 2026-06-12. The user confirmed Yellow support was manually tested in RPCS3 before closeout; final automated verification passed 683 tests and Host temp-output build passed with 0 warnings and 0 errors.
- v1.3 Red AC15 Support shipped on 2026-06-16. Red local protocol inputs are `proto/red/taiko.proto` and `proto/red/vsinterface.proto`; local game data is symlinked at `Host/wwwroot/data/red/data`.
- Red local data currently exposes versioned config roots including `config/ST5100-1`, `config/ST5100-7`, `config/ST7100-1`, and `config/ST8100-1`; the milestone must prove the runtime target/root before locking catalog paths.
- v1.4 White AC15 0.13 Support started on 2026-06-16. White local protocol inputs are `proto/white/taiko.proto` and `proto/white/vsinterface.proto`; local game data is under `Host/wwwroot/data/white/data`.
- White local data currently exposes `config/ST7100-1` as the observed config root, with `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, and `spacialbaid.xml` in that root.
- `.tools/white/EBOOT.ELF.i64` is currently present and nonzero in this checkout (`129893515` bytes), so superseded zero-byte notes are no longer current. Phase 23 IDA route extraction proves `/v07r00/chassis` plus fourteen no-state scaffold suffixes; additional White route, root-selection, and runtime assumptions still need local IDB, log, capture, data, or cabinet/RPCS3 evidence before implementation.
- The evidence hierarchy is repo code, proto files, SQLite state, cabinet/RPCS3 logs, IDA/client evidence, and only then public wiki pages for gameplay scoping.
- Yellow local protocol input is `proto/yellow/yellow.proto`; local game data is under `Host/wwwroot/data/yellow/data`, with the observed versioned config root `config/ST9100-1`.
- Yellow proto evidence includes Tokkun tutorial and stage-result fields, item shop and Banacoin-adjacent routes, Don/Katsu medal upload fields, and no Blue battle userdata or initialdata battle fields.
- Blue and Green wire surfaces include WaiWai tutorial and playresult fields. For Yellow, verify the current generated/local wire evidence before implementing; the intended behavior is tutorial flag persistence/readback plus playresult logging only, not a new mode.
- Crown readback compression must be proven per era. Blue/Green currently gzip `hash_crown_flg`, but older-version crown transport may differ.
- Public wiki context says Yellow started on 2017-03-15, introduced Don/Katsu medals, and later added "Issho ni Wai Wai Ensou"; this is scoping context only and does not outrank local protocol or runtime evidence.
- Public wiki context says Red was the active AC15 version from 2016-07-14 to 2017-03-14, and that Don Challenge effectively ended with Red before Yellow's reward-system change pause; this is scoping context only and does not outrank local protocol or runtime evidence.
- Public wiki context says White 0.13 started on 2015-12-10 and later White updates changed or reintroduced some features. Use that as product/version scoping only; local White proto, data, logs, IDA, and cabinet/RPCS3 evidence decide server behavior.
- Red proto evidence includes challenge competition readback through `ChallengeCompeRequest` / `ChallengeCompeResponse`, user-data `is_challengecompe`, and playresult challenge id arrays. Treat those as shared older-AC15 ChallengeCompe planning leads and Red binding evidence, not final semantics and not evidence that Blue/Green/Yellow challengecompe stubs are active, until local data/runtime traces prove behavior.
- White proto evidence initially exposes BAID, mydon, userdata, playresult, self-best, crowns, recommendations, folders, telops, Taikojuku, ChallengeCompe-like fields, and reward routes. It does not initially expose explicit battle, item-shop, Tokkun, gacha, tournament runtime, or Banacoin wallet/payment surfaces.
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md`, `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md`, and `docs/superpowers/plans/2026-06-07-ac15-core-extraction/` describe the approved capability-driven AC15 sharing direction for Blue, Green, and older AC15 support. Use those designs where they directly enable White, but keep era routes, wire DTOs, and persistence separate.

## Shipped Milestones

v1.0 Blue Support is complete. Blue is a first-class supported era with normal play, Dani, item shop/unlocking, AdminApi/WebUI readback, battle evidence/design, battle runtime persistence/protocol handling, final guardrails, and operator documentation.

v1.1 Blue Tokkun Mode Support is complete. Phases 7-11 established the Tokkun evidence contract, stateless Banacoin-adjacent compatibility, Tokkun playresult acceptance, Blue-owned Tokkun persistence/readback, final contract documentation, and runtime verification.

v1.2 Yellow AC15 Support is complete. Phases 12-17 plus inserted Phases 16.1 and 16.2 added Yellow as a first-class older AC15 era, using the local Yellow proto/data and Blue-equivalent behavior where Yellow supports it. The milestone also regenerated AC15 wire DTOs with nullable optional primitives and simplified shared AC15 core behavior without merging era state.

v1.3 Red AC15 Support is complete. Phases 18-22 added Red as a first-class older AC15 era with Red route/root evidence, generated Red wire DTOs, catalog/profile binding, Red-owned runtime state, tutorial-only Tokkun readback, shared older-AC15 ChallengeCompe/Don Challenge behavior, AdminApi/WebUI readback, and final runtime closeout evidence.

## Current Milestone: v1.4 White AC15 0.13 Support

**Goal:** Add White 0.13 by composing existing AC15 capabilities with White config, limits, wire placement, typed persistence, collectable data, and White/older-version evidence while preserving era-owned protocol, catalog, state, admin, and verification boundaries.

**Target features:**
- White evidence and first-class era foundation from `proto/white`, `Host/wwwroot/data/white/data`, route/root evidence, and safe Host gating.
- White catalog/profile binding through matching AC15 catalog loaders and White-specific sidecars only where White data proves a delta.
- White-owned runtime bindings for matching profile/userdata, normal play, self-best, crowns, recommendations, folders/telops, reward/profile fields, and Dani only if White 0.13 evidence proves runtime support.
- White-scoped collectable data, including Don Challenge data if present in the 0.13 range, collected and bound as the final functional step before closeout.
- AdminApi/WebUI readback for implemented White-owned surfaces only, followed by automated verification and RPCS3/cabinet smoke evidence.

## Constraints

- **Evidence**: New era semantics must be specified from proto, local data, logs, IDA/client evidence, or cabinet/RPCS3 traces before runtime implementation.
- **Architecture**: Treat each AC15 era as a composition root for supported capabilities, with era-owned wire DTOs, routes, persistence, catalog data, tests, config/limits, wire placement, and narrow era-specific helpers.
- **State separation**: Keep Blue, Green, Yellow, Red, White, and Nijiiro persistent state separate unless the data is truly shared identity state.
- **Transport safety**: Preserve known AC15 direct-protobuf and startup/verup assumptions only where current per-era client/proto evidence supports them.
- **Scope order**: Build foundation, capability profile/catalog binding, runtime capability bindings, shared older-AC15 ChallengeCompe, and verification before claiming full support for any new AC15 era.
- **Red scope**: Treat Red as an older-AC15 capability composition without WaiWai; ChallengeCompe is a shared older-AC15 capability whose first binding/proof point is Red.
- **White scope**: Treat White 0.13 as an older-AC15 capability composition with more missing features than Red; do not backfill later White update behavior without local 0.13 evidence.
- **Verification**: Done requires automated route/handler/catalog/persistence proof and repeatable cabinet/RPCS3 smoke evidence for the supported runtime flows, not only passing server tests.
- **Local data**: Blue runtime data under `Host/wwwroot/data/blue/data` is local/operator-supplied and may be gitignored.
- **Local data**: Yellow runtime data under `Host/wwwroot/data/yellow/data` is local/operator-supplied and may be gitignored.
- **Local data**: Red runtime data under `Host/wwwroot/data/red/data` is local/operator-supplied and may be gitignored.
- **Local data**: White runtime data under `Host/wwwroot/data/white/data` is local/operator-supplied and may be gitignored.
- **Build environment**: If `Host/bin/Debug/net10.0` is locked by a running server, verify Host builds with a temp output path.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Use GSD to finish full Blue support from the existing Superpowers roadmap | The roadmap already captures staged Blue support and evidence references; GSD should continue rather than restart discovery | Validated in v1.0 |
| Treat A0-A5 as validated prior work | The user stated the previous Superpowers work completed through A5 | Validated in v1.0 |
| Include Track B battle mode in current project scope | The user selected full Blue scope, not Track A only | Validated in v1.0 |
| Finish normal Track A before battle runtime implementation | Existing roadmap says battle starts after Track A is stable enough for normal cabinet smoke testing | Validated in v1.0 |
| Use strict battle evidence gates | The user selected strict evidence before battle implementation | Validated in v1.0 |
| Define done as cabinet/RPCS3-proven normal and battle flows | The user selected repeatable cabinet evidence as the full-support done condition | Validated in v1.0 |
| Complete Phase 5 battle runtime with data-derived defaults and bounded store/echo behavior | Phase 5 resolved required runtime rows through IDA/proto/local-data evidence plus named user approval for parsed-data battle initialdata unlocks | Validated by Phase 5 server verification and closed by v1.0 full Blue verification |
| Close stale Phase 05 debug/UAT artifacts at milestone completion | User confirmed the remaining audit-open records were stale and had been externally fixed and resolved before close | Resolved in v1.0 closeout |
| Reopen Blue Tokkun mode as v1.1 scope | Public wiki context says Tokkun ended before Blue, but user-provided Blue binary/protobuf evidence indicates the mode still exists and should be supported | Validated in v1.1 |
| Establish a Tokkun contract before runtime changes | Phase 7 created the evidence matrix, classifier policy, side-effect blocks, and Phase 8-11 gates before any Tokkun runtime implementation | Validated in Phase 7 |
| Keep Banacoin compatibility stateless | TaikoLocalServer is not a Banacoin authority, but Blue Tokkun availability needs permissive route compatibility | Validated in Phase 8 |
| Accept Tokkun playresults before battle or normal persistence | Tokkun-shaped payloads can include normal-looking or battle-looking material; Phase 9 proves Tokkun wins and leaves existing Blue state unchanged | Validated in Phase 9 |
| Persist only protocol-backed Tokkun state | Phase 10 stores nullable tutorial state and raw append-only history while preserving song order/duplicates and client protocol timestamps | Validated in Phase 10 |
| Close Tokkun only after runtime proof | Phase 11 records user-confirmed cabinet/RPCS3 Tokkun verification plus full automated test/build evidence | Validated in v1.1 |
| Start Yellow support as v1.2 | Yellow is the next older AC15 era and should build on the completed Blue/Green support rather than reset project numbering | Validated in v1.2 |
| Include AC15 shared-core extraction in Yellow work where it directly helps | The prior approved AC15 core plan was created specifically to avoid duplicating Blue/Green logic while adding Yellow/Red, but it must preserve separate era routes, wire DTOs, and persistence | Validated in v1.2 |
| Treat Yellow Tokkun as real and Yellow battle as absent | Yellow proto has Tokkun fields and lacks Blue battle fields/routes; runtime implementation should follow that evidence instead of copying Blue-only behavior | Validated in v1.2 |
| Insert AC15 mapper rewrite before Yellow closeout | Green/Blue/Yellow protocol mappers relied on hand-written projection and scattered protobuf presence helper calls despite Mapperly being introduced; repo-local `protogen` supports nullable optional primitives via `+nullablevaluetype=yes` | Completed in Phase 16.1 |
| Close Yellow only after RPCS3 runtime proof | Phase 17 records user-confirmed RPCS3 Yellow support verification plus full automated test/build evidence before v1.2 archive | Validated in v1.2 |
| Start Red support as v1.3 | Red is the next older AC15 era after Yellow, local Red proto/data are present, and user scope says behavior should mostly share with Yellow while excluding WaiWai | Validated in v1.3 |
| Treat ChallengeCompe as a shared older-AC15 capability with Red as the first binding | Red proto exposes challenge competition request/response and playresult/userdata fields, and public scoping context says Don Challenge is a Red-and-older AC15 behavior | Validated in v1.3 |
| Start White 0.13 support as v1.4 | White is the next older AC15 era after Red; local White proto/data are present and the user expects mostly assembling existing capabilities with correct White responses and limits | Validated in Phase 23 |
| Keep Phase 23 White foundation no-state and evidence-gated | White route proof approves only `/v07r00/chassis` plus fourteen scaffold suffixes; runtime catalog/profile/state/AdminApi/WebUI behavior remains owned by later phases | Validated in Phase 23 |
| Collect White collectable data late in the milestone | White collectable data such as Don Challenge should be gathered if it falls in the 0.13 range, but only after core era support is stable | Pending in v1.4 |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `$gsd-transition`):
1. Requirements invalidated? Move to Out of Scope with reason
2. Requirements validated? Move to Validated with phase reference
3. New requirements emerged? Add to Active
4. Decisions to log? Add to Key Decisions
5. "What This Is" still accurate? Update if drifted

**After each milestone** (via `$gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check - still the right priority?
3. Audit Out of Scope - reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-06-18 after Phase 23 White Evidence and Era Foundation connection smoke closeout*
