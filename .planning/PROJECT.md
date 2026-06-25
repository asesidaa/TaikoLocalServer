# TaikoLocalServer AC15 Era Support

## What This Is

TaikoLocalServer is a local ASP.NET Core server for Taiko no Tatsujin cabinet protocols, local SQLite persistence, era-specific game data catalogs, and a Blazor WebAssembly admin UI. This project continues the existing Blue-era support effort from the Superpowers roadmap in `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`, starting after completed stages A0-A5 and carrying the work through full Blue support.

Full Blue, Yellow, Red, White 0.13, Murasaki, and KIMIDORI 0.12 support are complete. MOMOIRO 0.11 support is the active milestone. The project supports Nijiiro, Green AC15, Blue AC15, Yellow AC15, Red AC15, White AC15, Murasaki AC15, and KIMIDORI AC15 in one process while preserving era-owned routes, wire DTOs, persistence, handlers, mappers, catalogs, and tests.

## Core Value

AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating MOMOIRO, KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, or shared identity state.

## Current State

v1.2 Yellow AC15 Support shipped on 2026-06-12. Yellow is a first-class AC15 era using `proto/yellow/yellow.proto`, generated Yellow wire DTOs, Yellow-owned persistence, and `Host/wwwroot/data/yellow/data`.

v1.3 Red AC15 Support shipped on 2026-06-16. Red uses `proto/red/taiko.proto`, `proto/red/vsinterface.proto`, and the local Red game-data symlink at `Host/wwwroot/data/red/data` as source evidence. Red support covers `/v08r01` game routes, shared `/v01r00` startup/version ownership, active `ST8100-1` root evidence, first-class Red adapter/wire/Host gating, catalog/profile binding, Red-owned normal runtime state, Dani, tutorial-only Tokkun readback, simple compatibility routes, server-side Don Challenge progress/rewards, separate stubbed `challengecompe.php` protocol compatibility, AdminApi/WebUI readback, and user-accepted runtime closeout.

v1.4 White AC15 0.13 Support shipped on 2026-06-21. White uses `proto/white/taiko.proto`, `proto/white/vsinterface.proto`, and local game data at `Host/wwwroot/data/white/data` as source evidence, with observed `config/ST7100-1` data. White support covers `/v07r00/chassis` game routes, shared `/v01r00/chassis` startup/version ownership, first-class White adapter/wire/Host gating, catalog/profile binding, White-owned runtime state, normal play, Dani where proven, reward/Don Point state, present/special-BAID provenance, server-side Don Challenge progress/rewards through dedicated AdminApi/WebUI readback, and accepted runtime/WebUI closeout. Later White-version behavior remains evidence-gated. The follow-up White final 11.01 quick work split `/v07r03` final protocol support from legacy `/v07r00` compatibility and added proven Tokkun/Banacoin/difficulty panel handling without changing the v1.4 White 0.13 milestone scope.

Although Blue, Green, and Yellow wire surfaces contain challenge competition proto definitions and some compatibility routes, those newer versions do not meaningfully call the feature; do not treat their stubs as runtime behavior evidence. Challenge competition is meaningful scope only for Red and older AC15 versions.

Blue now supports normal, battle, and Tokkun play in the same process. Tokkun support is bounded to evidence-backed Blue protocol behavior: stateless Banacoin-adjacent compatibility, Tokkun playresult acceptance, Blue-owned raw Tokkun persistence, nullable tutorial readback, final contract documentation, and user-confirmed cabinet/RPCS3 runtime verification.

Yellow support reuses AC15 shared core behavior where it directly reduces duplicated normal-play, catalog, score, crown, Dani, shop, Tokkun, and admin behavior without merging era state or inventing unsupported routes.

## Current Planning State

v1.7 MOMOIRO AC15 0.11 Support is active. v1.6 KIMIDORI AC15 Support shipped on 2026-06-25 and is archived.

## Current Milestone: v1.7 MOMOIRO AC15 0.11 Support

**Goal:** Add first-class MOMOIRO 0.11 support by composing existing older-AC15 capabilities around MOMOIRO-owned proto, `/v04r00` game routes, root-level data, binary-confirmed limits, persistence, AdminApi/WebUI surfaces, and verification.

**Target features:**
- First-class `GameEra.Momoiro` foundation with generated wire DTOs from `proto/momoiro`, `/v04r00/chassis/*.php` game routes, shared `/v01r00/chassis/*.php` startup/version routes, Host settings, DI, application-part gating, and direct-protobuf transport where local evidence supports it.
- Route/root evidence from `.tools/momoiro/EBOOT.ELF.i64`, `proto/momoiro`, and linked MOMOIRO game data before locking route handlers, catalog roots, and supported route behavior. The current MOMOIRO binary route inventory is shared startup/version `startupauth.php`, `verupauth.php`, and `verupcomplete.php` plus game routes `playresult.php`, `baidcheck.php`, `mydonentry.php`, `userdata.php`, `recommend.php`, `selfbest.php`, `heartbeat.php`, `defaultsong.php`, `bookkeeping.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php`.
- Root-level MOMOIRO catalog loading from `Host/wwwroot/data/momoiro/data` without assuming newer `config/STxxxx-*` data layout.
- Catalog/profile/runtime binding through existing AC15/KIMIDORI-style capabilities where MOMOIRO proto and binary route evidence both prove the feature: BAID/mydon, userdata, normal play, self-best, crowns inside userdata, favorites/recent, default-song/song-hash/telop surfaces, recommendations, Don Point/reward fields, challenge arrays where proven through `playresult.php` and `userdata.php`, and AdminApi/WebUI routing.
- Binary research for song unlocking, crown byte placement/packing, and changed protocol limits before implementing persistence/readback semantics.
- Explicit absence handling for features missing from `proto/momoiro` or without a corresponding binary `.php` route, including later-era route families and stateful behavior unless new local evidence proves otherwise.

## Completed Milestone: v1.6 KIMIDORI AC15 Support

**Goal:** Add first-class KIMIDORI 0.12 support by composing existing older-AC15 capabilities around KIMIDORI-owned proto, route, root-level data, persistence, AdminApi/WebUI surfaces, and verification.

**Target features:**
- First-class `GameEra.Kimidori` foundation with generated wire DTOs from `proto/kimidori`, `/v05r00/chassis/*` game routes, shared `/v01r00/chassis/*` startup/version routes, Host settings, DI, application-part gating, and direct-protobuf transport where local evidence supports it.
- Route/root evidence from `.tools/kimidori/EBOOT.ELF.i64`, `proto/kimidori`, and linked KIMIDORI game data before locking route handlers, route inventory, and catalog roots.
- Root-level KIMIDORI catalog loading from `Host/wwwroot/data/kimidori/data` without assuming newer `config/STxxxx-*` data layout.
- Catalog/profile/runtime binding through existing AC15/Murasaki-style capabilities where KIMIDORI proto and binary route evidence both prove the feature: BAID/mydon, userdata, normal play, Dani Dojo, self-best, crowns, favorites/recent, folders/telops/default/mainichi/song-hash surfaces, recommendations, Don Point/reward fields, shopping-result compatibility, and AdminApi/WebUI routing.
- Explicit absence handling for features missing from `proto/kimidori` or without a corresponding binary `.php` route, including Taikojuku practice-folder behavior for KIMIDORI 0.12 unless new local evidence proves otherwise.
- Runtime acceptance was recorded on 2026-06-25, and v1.6 is archived under `.planning/milestones/`.

## Completed Milestone: v1.5 Murasaki AC15 Support

**Goal:** Add first-class Murasaki support by assembling White-like older AC15 capabilities where local Murasaki proto/data proves compatibility, while evidence-gating changed wire shapes and new request families.

**Target features:**
- First-class `GameEra.Murasaki` foundation with Murasaki-owned adapter, generated wire DTOs from `proto/murasaki`, Host gating, route handling, direct-protobuf transport where proven, and shared startup/version ownership only where `proto/murasaki/vsinterface.proto` and client evidence agree. Implemented through Phase 28.
- Route/root evidence from local Murasaki binary/client data before locking the game route prefix and active config root. Local data currently exposes `ST5100-1`, `ST5100-7`, and `ST6100-1`; runtime root must not be guessed from filenames.
- Catalog/profile/runtime binding through existing AC15/White-era capabilities where wire/data semantics match: identity, BAID/mydon, userdata, normal play, self-best/crowns, favorites/recent, folders/telops/recommendations, Dani/Taikojuku, and reward/Don Point fields. Implemented through Phase 32.
- Murasaki-specific protocol mapping for changed wire shape: the local `taiko.proto` does not expose the White-style monolithic `initialdatacheck` message and instead splits metadata across request families. Implemented split metadata for `defaultsong`, `mainichisong`, `foldercheck`, `getfolder`, `telopcheck`, and `gettelop`.
- Phase 33 closed unsupported special surfaces conservatively: `bestscore.php`, `songhash.php`, and `shoppingresult.php` remain absent because no current route evidence exists; challenge arrays do not enable Don Challenge or ChallengeCompe behavior.
- AdminApi/WebUI parity for implemented Murasaki-owned state shipped with automated verification and user-observed in-game acceptance recorded on 2026-06-23.
- Final `/v06r01/chassis` route support shipped through quick task 260623-2ff, preserving `/v06r00` compatibility and binary-supported Dani/Taikojuku behavior.

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
- [x] Red Don Challenge: Red binds server-side Don Challenge through local sidecar data, Red-owned progress/reward state, normal playresult stage matching, and dedicated AdminApi/WebUI contracts. Cabinet `challengecompe.php` remains a separate protocol compatibility stub.
- [x] Red AdminApi/WebUI and runtime closeout: Phase 22 records automated verification, temp-output Host build, and user-accepted manual runtime evidence before v1.3 archive.
- [x] White evidence and first-class foundation: Phase 23 records White `/v07r00/chassis` route evidence for fourteen no-state scaffold suffixes, shared `/v01r00/chassis` startup/version ownership, current nonzero White IDB evidence, `ST7100-1` root evidence, generated White wire DTOs, `GameEra.White`, Host settings/registration, exact content-type fallback, disabled-era application-part gating, the 2026-06-18 setup 405 fix, user-confirmed connection smoke, and no runtime/catalog/AdminApi/WebUI behavior.
- [x] White catalog/profile/runtime binding: White binds matching AC15 catalog, profile, userdata, normal play, self-best, crowns, favorites, recent songs, reward/Don Point, and Dani behavior through White-owned state, explicit White protocol limits, and mechanical Mapperly projection.
- [x] White collectable and Don Challenge support: White present/special-BAID provenance is collected, and White Don Challenge is implemented only as server-side stage-derived progress with White-owned data/state/AdminApi/WebUI readback.
- [x] White AdminApi/WebUI and runtime closeout: Phase 27 records White readback/edit surfaces over implemented White-owned state only, automated verification, generated-source inspection, temp-output Host build, and user-accepted RPCS3/cabinet/WebUI closeout.
- [x] Murasaki AC15 support: Phases 28-34 plus quick task 260623-2ff add first-class Murasaki routes, catalog/profile binding, split metadata, runtime state, normal play, Dani/Taikojuku, reward/Don Point behavior, AdminApi/WebUI parity, final `/v06r01` route support, and accepted in-game closeout.

### Active

- [ ] Add first-class MOMOIRO 0.11 route, wire, Host, settings, and era-foundation support from local proto and binary evidence.
- [ ] Load MOMOIRO root-level catalog data from `Host/wwwroot/data/momoiro/data` and preserve evidence-backed song hash, release-song, default-song, telop, and recommendation behavior.
- [ ] Implement MOMOIRO-owned identity, userdata, self-best, crowns-in-userdata, favorites/recent, normal play, Don Point/reward, recommendation, and challenge-compatible state only where proto plus binary evidence prove the route and fields.
- [ ] Expose implemented MOMOIRO state through AdminApi/WebUI without writing KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, or Nijiiro gameplay state.
- [ ] Close with automated verification plus repeatable cabinet/RPCS3 runtime evidence for supported MOMOIRO flows.

### Out of Scope

- Real Banacoin balance, payment, settlement, receipt, coupon, deduction, BNID result, or transaction-history behavior beyond stateless compatibility routes needed for Blue Tokkun availability.
- AC15 versions earlier than MOMOIRO, MOMOIRO updates beyond 0.11, later KIMIDORI updates beyond 0.12, or later White-version behavior beyond 0.13, unless started by a later milestone or pulled in by local per-era evidence.
- Red WaiWai behavior; Red is planned as Yellow-like support without WaiWai unless local Red evidence proves otherwise.
- Yellow battle mode or Blue battle behavior mirrored into Yellow without concrete Yellow proto/log/client evidence.
- Blue battle behavior mirrored into Red without concrete Red proto/log/client evidence.
- White battle, item shop, Tokkun, gacha, tournament runtime, Banacoin wallet/payment, or later White update behavior without concrete White 0.13 proto/log/client evidence.
- Later Murasaki update behavior beyond the shipped v1.5 final `/v06r01` support unless local Murasaki proto, data, binary/client evidence, logs, or cabinet/RPCS3 behavior pulls it into scope.
- Invented Murasaki global high-score, song-hash, default-song, mainichi-song, shopping, or reserved-byte semantics without concrete local evidence.
- KIMIDORI Taikojuku practice-folder behavior, Don Challenge/ChallengeCompe, battle, Tokkun, Banacoin, or full shop authority unless `proto/kimidori` contains the feature and the local binary proves the corresponding `.php` route.
- MOMOIRO Taikojuku, Tokkun, Banacoin, battle, newer item-shop authority, `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, `mainichisong.php`, separate `crownsdata.php`, separate Don Challenge/ChallengeCompe management, or later-era feature folders unless `proto/momoiro` contains the feature and the local binary proves the corresponding `.php` route and runtime contract.
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
- v1.5 Murasaki AC15 Support started on 2026-06-21. Murasaki local protocol inputs are `proto/murasaki/taiko.proto` and `proto/murasaki/vsinterface.proto`; local game data is under `Host/wwwroot/data/murasaki/data`.
- Murasaki Phase 28 evidence records exact `v01r00`, `v06r00`, and `.php` strings from local IDA evidence. Shared startup/version stays on `v01r00`; game routes are under `v06r00`.
- Murasaki catalog binding currently uses `config/ST6100-1` with required `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `fumen/tuning.bin`.
- Murasaki phases 28-34 plus quick task 260623-2ff are complete. Final closeout verification on 2026-06-23 reported no vulnerable packages, `dotnet test Tests/Tests.csproj --no-build` passed 865 tests, `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true` passed with 0 warnings and 0 errors, and the user reported the in-game Murasaki flow works.
- v1.6 KIMIDORI AC15 Support started on 2026-06-23. KIMIDORI local protocol inputs are `proto/kimidori/taiko.proto` and `proto/kimidori/vsinterface.proto`; local reverse-engineering evidence is under `.tools/kimidori/`.
- KIMIDORI uses shared `/v01r00/chassis` startup/version routing and `/v05r00/chassis` game routing for this milestone.
- KIMIDORI local game data is linked at `Host/wwwroot/data/kimidori/data` and currently uses root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin` instead of a versioned `config/STxxxx-*` directory.
- KIMIDORI feature support must require both protocol presence and binary `.php` route presence. If the proto does not contain a feature, treat that feature as missing for KIMIDORI 0.12 even if adjacent AC15 eras support it.
- v1.7 MOMOIRO AC15 0.11 Support started on 2026-06-25. MOMOIRO local protocol inputs are `proto/momoiro/taiko.proto` and `proto/momoiro/vsinterface.proto`; local reverse-engineering evidence is under `.tools/momoiro/`.
- MOMOIRO uses shared `/v01r00/chassis` startup/version routing and `/v04r00/chassis` game routing for this milestone. All MOMOIRO route handlers should remain `.php` routes.
- The current MOMOIRO binary route inventory is `startupauth.php`, `verupauth.php`, and `verupcomplete.php` under shared `/v01r00/chassis`, plus `playresult.php`, `baidcheck.php`, `mydonentry.php`, `userdata.php`, `recommend.php`, `selfbest.php`, `heartbeat.php`, `defaultsong.php`, `bookkeeping.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php` under `/v04r00/chassis`.
- MOMOIRO local game data is linked at `Host/wwwroot/data/momoiro/data` and currently uses root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin` instead of a versioned `config/STxxxx-*` directory.
- MOMOIRO proto evidence exposes crowns through `UserDataResponse.hash_crown_flg`; do not add a separate `crownsdata.php` contract unless binary/client evidence proves one.
- MOMOIRO feature support must require both protocol presence and binary `.php` route presence. Routes from the current binary inventory should become either feature-complete handlers or explicit static-result operational stubs; proto-only route families stay absent. Binary research is still required before locking song unlocking, crown byte packing/placement, and changed protocol limits.
- The evidence hierarchy is repo code, proto files, SQLite state, cabinet/RPCS3 logs, IDA/client evidence, and only then public wiki pages for gameplay scoping.
- Yellow local protocol input is `proto/yellow/yellow.proto`; local game data is under `Host/wwwroot/data/yellow/data`, with the observed versioned config root `config/ST9100-1`.
- Yellow proto evidence includes Tokkun tutorial and stage-result fields, item shop and Banacoin-adjacent routes, Don/Katsu medal upload fields, and no Blue battle userdata or initialdata battle fields.
- Blue and Green wire surfaces include WaiWai tutorial and playresult fields. For Yellow, verify the current generated/local wire evidence before implementing; the intended behavior is tutorial flag persistence/readback plus playresult logging only, not a new mode.
- Crown readback compression must be proven per era. Blue/Green currently gzip `hash_crown_flg`, but older-version crown transport may differ.
- Public wiki context says Yellow started on 2017-03-15, introduced Don/Katsu medals, and later added "Issho ni Wai Wai Ensou"; this is scoping context only and does not outrank local protocol or runtime evidence.
- Public wiki context says Red was the active AC15 version from 2016-07-14 to 2017-03-14, and that Don Challenge effectively ended with Red before Yellow's reward-system change pause; this is scoping context only and does not outrank local protocol or runtime evidence.
- Public wiki context says White 0.13 started on 2015-12-10 and later White updates changed or reintroduced some features. Use that as product/version scoping only; local White proto, data, logs, IDA, and cabinet/RPCS3 evidence decide server behavior.
- Public wiki context says Murasaki started on 2015-03-11, expanded favorite songs from 5 to 10, introduced Murasaki-era feature folders, and used Don Point behavior with a 30000-point cap. This is product/version scoping only; local Murasaki proto, data, logs, binary/client evidence, and cabinet/RPCS3 behavior decide server behavior.
- Public wiki context says KIMIDORI 0.12 started on 2014-07-16, introduced KIMIDORI-era feature folders, and kept favorite folders at 5 songs before Murasaki expanded the limit to 10. This is product/version scoping only; local KIMIDORI proto, data, logs, binary/client evidence, and cabinet/RPCS3 behavior decide server behavior.
- Public wiki context says MOMOIRO 0.11 started on 2013-12-11, used a 30000 Don Point/shop-point cap at launch, changed customization/shop context, and had crown-count caveats around SORAIRO-era removed songs. This is product/version scoping only; local MOMOIRO proto, data, logs, binary/client evidence, and cabinet/RPCS3 behavior decide server behavior.
- Red proto evidence includes challenge competition readback through `ChallengeCompeRequest` / `ChallengeCompeResponse`, user-data `is_challengecompe`, and playresult challenge id arrays. Treat those as protocol facts only; they are not Don Challenge semantics and are not evidence that Blue/Green/Yellow challengecompe stubs are active.
- White proto evidence initially exposes BAID, mydon, userdata, playresult, self-best, crowns, recommendations, folders, telops, Taikojuku, embedded challenge stat fields, and reward routes. It does not initially expose explicit battle, item-shop, Tokkun, gacha, tournament runtime, or Banacoin wallet/payment surfaces.
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md`, `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md`, and `docs/superpowers/plans/2026-06-07-ac15-core-extraction/` describe the approved capability-driven AC15 sharing direction for Blue, Green, and older AC15 support. Use those designs where they directly enable White, but keep era routes, wire DTOs, and persistence separate.

## Shipped Milestones

v1.0 Blue Support is complete. Blue is a first-class supported era with normal play, Dani, item shop/unlocking, AdminApi/WebUI readback, battle evidence/design, battle runtime persistence/protocol handling, final guardrails, and operator documentation.

v1.1 Blue Tokkun Mode Support is complete. Phases 7-11 established the Tokkun evidence contract, stateless Banacoin-adjacent compatibility, Tokkun playresult acceptance, Blue-owned Tokkun persistence/readback, final contract documentation, and runtime verification.

v1.2 Yellow AC15 Support is complete. Phases 12-17 plus inserted Phases 16.1 and 16.2 added Yellow as a first-class older AC15 era, using the local Yellow proto/data and Blue-equivalent behavior where Yellow supports it. The milestone also regenerated AC15 wire DTOs with nullable optional primitives and simplified shared AC15 core behavior without merging era state.

v1.3 Red AC15 Support is complete. Phases 18-22 added Red as a first-class older AC15 era with Red route/root evidence, generated Red wire DTOs, catalog/profile binding, Red-owned runtime state, tutorial-only Tokkun readback, server-side Don Challenge behavior, separate stubbed ChallengeCompe protocol compatibility, AdminApi/WebUI readback, and final runtime closeout evidence.

v1.4 White AC15 0.13 Support is complete. Phases 23-27 plus inserted Phase 23.1 added White as a first-class older AC15 era with White route/root evidence, generated White wire DTOs, catalog/profile binding, White-owned runtime state, present/special-BAID provenance, server-side Don Challenge behavior, AdminApi/WebUI readback, and accepted runtime closeout evidence.

v1.5 Murasaki AC15 Support is complete. Phases 28-34 plus quick task 260623-2ff added Murasaki as a first-class older AC15 era with Murasaki route/root evidence, generated wire DTOs, catalog/profile binding, split metadata readback, Murasaki-owned runtime state, normal play, Dani/Taikojuku, reward/Don Point behavior, AdminApi/WebUI readback, final `/v06r01` protocol support, and accepted in-game closeout evidence.

v1.6 KIMIDORI AC15 Support is complete. Phases 35-38 added KIMIDORI as a first-class older AC15 era with KIMIDORI route/root evidence, generated wire DTOs, root-level catalog loading, KIMIDORI-owned runtime state, normal play, Dani Dojo, songhash-backed catalog enablement, AdminApi/WebUI readback, and accepted cabinet/RPCS3 closeout evidence.

## Next Milestone

v1.7 MOMOIRO AC15 0.11 Support is being defined through requirements and roadmap.

## Constraints

- **Evidence**: New era semantics must be specified from proto, local data, logs, IDA/client evidence, or cabinet/RPCS3 traces before runtime implementation.
- **Architecture**: Treat each AC15 era as a composition root for supported capabilities, with era-owned wire DTOs, routes, persistence, catalog data, tests, config/limits, wire placement, and narrow era-specific helpers.
- **State separation**: Keep Blue, Green, Yellow, Red, White, Murasaki, KIMIDORI, MOMOIRO, and Nijiiro persistent state separate unless the data is truly shared identity state.
- **Transport safety**: Preserve known AC15 direct-protobuf and startup/verup assumptions only where current per-era client/proto evidence supports them.
- **Scope order**: Build foundation, capability profile/catalog binding, runtime capability bindings, Don Challenge where era data proves it, and verification before claiming full support for any new AC15 era.
- **Red scope**: Treat Red as an older-AC15 capability composition without WaiWai; Don Challenge is server-side progress/rewards, while ChallengeCompe remains a separate protocol surface.
- **White scope**: Treat White 0.13 as an older-AC15 capability composition with more missing features than Red; do not backfill later White update behavior without local 0.13 evidence.
- **Murasaki scope**: Treat initial Murasaki as an older-AC15 capability composition that is feature-similar to White only where local Murasaki evidence proves matching limits and wire placement.
- **KIMIDORI scope**: Treat KIMIDORI 0.12 as an older-AC15 capability composition whose feature set is determined by `proto/kimidori` plus binary route evidence, not by copying Murasaki or later AC15 behavior.
- **MOMOIRO scope**: Treat MOMOIRO 0.11 as an older-AC15 capability composition whose feature set is determined by `proto/momoiro` plus binary route evidence, not by copying KIMIDORI, Murasaki, or later AC15 behavior.
- **Verification**: Done requires automated route/handler/catalog/persistence proof and repeatable cabinet/RPCS3 smoke evidence for the supported runtime flows, not only passing server tests.
- **Local data**: Blue runtime data under `Host/wwwroot/data/blue/data` is local/operator-supplied and may be gitignored.
- **Local data**: Yellow runtime data under `Host/wwwroot/data/yellow/data` is local/operator-supplied and may be gitignored.
- **Local data**: Red runtime data under `Host/wwwroot/data/red/data` is local/operator-supplied and may be gitignored.
- **Local data**: White runtime data under `Host/wwwroot/data/white/data` is local/operator-supplied and may be gitignored.
- **Local data**: Murasaki runtime data under `Host/wwwroot/data/murasaki/data` is local/operator-supplied and may be gitignored.
- **Local data**: KIMIDORI runtime data under `Host/wwwroot/data/kimidori/data` is local/operator-supplied and may be gitignored.
- **Local data**: MOMOIRO runtime data under `Host/wwwroot/data/momoiro/data` is local/operator-supplied and may be gitignored.
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
| Treat Don Challenge as server-side stage-derived progress, not ChallengeCompe protocol state | Red/White behavior is driven from normal playresult stages and dedicated AdminApi/WebUI readback; `challengecompe.php` remains a separate protocol surface | Corrected in v1.4 |
| Start White 0.13 support as v1.4 | White is the next older AC15 era after Red; local White proto/data are present and the user expects mostly assembling existing capabilities with correct White responses and limits | Validated in Phase 23 |
| Keep Phase 23 White foundation no-state and evidence-gated | White route proof approves only `/v07r00/chassis` plus fourteen scaffold suffixes; runtime catalog/profile/state/AdminApi/WebUI behavior remains owned by later phases | Validated in Phase 23 |
| Collect White collectable data late in the milestone | White collectable data such as Don Challenge should be gathered if it falls in the 0.13 range, but only after core era support is stable | Validated in v1.4 |
| Implement White Don Challenge as server-side stage-derived progress only | White data/range evidence supports dedicated server-side Don Challenge readback, while standalone ChallengeCompe cabinet route/readback remains absent | Validated in v1.4 |
| Close White after accepted runtime/WebUI verification | Automated tests/builds and generated-source inspection are necessary but not sufficient; milestone close records user-accepted RPCS3/cabinet/WebUI verification | Validated in v1.4 |
| Split White final `/v07r03` from legacy `/v07r00` protocol support | Final 11.01 behavior uses different generated wire and proven routes; compatibility must not force final fields onto legacy White wire | Validated by quick task 260620-ub3 |
| Start Murasaki support as v1.5 | Murasaki is the next older AC15 era after White; local Murasaki proto/data are present, features look White-like, and the user expects reuse of existing capabilities where wire/data semantics match | Validated in v1.5 |
| Treat Murasaki changed wire shape as a first-class evidence gate | Murasaki lacks the White-style monolithic initial-data request and adds split metadata/global-score request families, so capability reuse must be mediated through Murasaki-owned wire mapping and binary/client evidence for unknown byte fields | Validated in v1.5; unproven special/global surfaces remain future evidence-gated work |
| Preserve Murasaki final `/v06r01` parity where the final binary still supports the route surface | Final proto/binary evidence supports final route parity and binary-supported Dani/Taikojuku behavior despite changelog wording that could be over-read as a server-disable signal | Validated by quick task 260623-2ff and v1.5 closeout |
| Start KIMIDORI support as v1.6 | KIMIDORI is the next older AC15 era after Murasaki; local KIMIDORI proto, binary evidence, and linked game data are present, and the user expects straightforward capability composition for version 0.12 | Validated in v1.6 |
| Define KIMIDORI features from proto plus binary route evidence | If `proto/kimidori` lacks a feature, it is missing; if the binary lacks a corresponding `.php` route, the server should not invent that surface | Validated in v1.6 |
| Treat KIMIDORI game data as root-level era data | The linked KIMIDORI `USRDIR/data` puts core files at the data root rather than under `config/STxxxx-*`, so catalog loading must adapt instead of hardcoding newer AC15 layout assumptions | Validated in v1.6 |
| Separate KIMIDORI Dani Dojo from Taikojuku practice-folder behavior | Dani Dojo is normal/Dan state surfaced through KIMIDORI runtime fields, while Taikojuku is a separate practice-folder route family and must not be conflated with Dani support | Validated in v1.6 |
| Start MOMOIRO support as v1.7 | MOMOIRO is the next older AC15 era after KIMIDORI; local MOMOIRO proto, binary evidence, and linked root-level game data are present, and the user expects no invented new features beyond evidence-backed capability composition for version 0.11 | Pending |
| Define MOMOIRO features from proto plus binary route evidence | If `proto/momoiro` lacks a feature, it is missing; if the binary lacks a corresponding `.php` route, the server should not invent that surface | Pending |
| Treat MOMOIRO crowns as userdata-owned unless proven otherwise | The MOMOIRO proto exposes `hash_crown_flg` in `UserDataResponse`; crown packing and readback limits still need binary/client proof before implementation | Pending |
| Treat MOMOIRO game data as root-level era data | The linked MOMOIRO `USRDIR/data` puts core files at the data root rather than under `config/STxxxx-*`, so catalog loading must adapt instead of hardcoding newer AC15 layout assumptions | Pending |

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
*Last updated: 2026-06-26 after recording MOMOIRO binary route inventory*
