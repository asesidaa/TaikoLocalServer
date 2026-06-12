# Domain Pitfalls

**Domain:** Red AC15 support in TaikoLocalServer
**Researched:** 2026-06-12
**Confidence:** HIGH for repo/proto/codebase pitfalls; MEDIUM for Red runtime semantics until Red cabinet/RPCS3 traces confirm route cadence and Don Challenge behavior

## Correction Note

The requirements review removed several over-broad pitfalls. The current guidance is: absence in Red proto is not a requirement; previous eras should remain untouched; Don Challenge product behavior comes from wiki, while implementation details require runtime/proto/client proof; local Red challenge assets/field names are not enough to require stateful challenge support.

## Critical Context

Blue, Green, and Yellow contain challenge competition proto fields and some controller/mapper surfaces, but those newer eras do not meaningfully call the feature. Do not treat their presence as active behavior. For this milestone, Don Challenge / challenge competition is meaningful only for Red and older AC15 versions.

Red should be planned as a first-class older AC15 era with Yellow-like reuse where local Red evidence matches, not as a Yellow clone. The Red proto exposes `ChallengeCompeRequest` / `ChallengeCompeResponse`, `UserDataResponse.is_challengecompe`, and per-stage playresult arrays `ary_challenge_id`, `ary_user_compe_id`, and `ary_bng_compe_id`; those are planning leads, not complete semantics.

## Critical Pitfalls

### Pitfall 1: Treating Newer-Era Challenge Surfaces As Runtime Evidence

**What goes wrong:** Red challenge competition is implemented by copying Green/Yellow/Blue empty stubs or tests because those eras have `challengecompe.php`, generated DTOs, mapper fields, and `IsChallengeCompe` save flags.

**Why it happens:** The surface looks shared across AC15. In this repo, however, Green and Yellow `GetChallengeCompeQuery` handlers explicitly return empty responses, and Blue/Green/Yellow route/controller presence is compatibility residue rather than proof of client use.

**Consequences:** Red appears "supported" in tests while the client receives empty Don Challenge state. Worse, the roadmap may skip the actual Red/older behavior phase because the endpoint already returns `Result = 1`.

**Prevention:** Make the user correction a phase-0 rule: newer-era challenge proto/controller presence is not evidence. Red challenge behavior must be defined from `proto/red/taiko.proto`, Red local data, request logs, and runtime traces. Treat Green/Yellow handlers as anti-examples for Red runtime behavior.

**Detection:** Warning signs include a Red `GetChallengeCompeQuery` that returns `new CommonChallengeCompeResponse()` unconditionally, tests that only assert `Result = 1`, or implementation notes citing Yellow challenge route presence as proof.

**Phase implications:** The Red evidence/foundation phase must inventory challenge fields and route cadence. A dedicated Don Challenge phase should follow normal/cabinet foundation, not be hidden inside metadata scaffolding.

### Pitfall 2: Modeling Don Challenge As A Boolean Instead Of State

**What goes wrong:** Red support sets `UserSaveDataRed.IsChallengeCompe = true` and maps it to userdata, but never persists challenge IDs, track numbers, per-track high scores, option flags, stage mode, or the three response groups from `ChallengeCompeResponse`.

**Why it happens:** Existing Blue/Green/Yellow save rows already have `IsChallengeCompe`, and shared `Ac15UserDataService` carries that counter through the canonical userdata response. The Red proto has a much richer challenge readback surface than that boolean.

**Consequences:** The client may expose a Don Challenge menu or state flag, then receive empty `ary_challenge_stat`, `ary_user_compe_stat`, and `ary_bng_compe_stat` rows. That is a broken runtime contract disguised as a successful userdata response.

**Prevention:** Treat `is_challengecompe` as only an availability/readback flag. Model challenge competition persistence separately, likely Red-owned rows for competition group, competition id, track number, song, level, option flag, stage mode, high score, and source category. Only promote shared older-AC15 logic after Red proves the shape.

**Detection:** Red has `IsChallengeCompe` in save data but no Red challenge tables, no challenge-stage ingestion from playresult, and no non-empty `CommonChallengeCompeResponse` path.

**Phase implications:** Userdata foundation can expose the flag only after the challenge phase has a source for it, or it should remain false until Red challenge state is implemented.

### Pitfall 3: Letting Shared Normal Play Drop Challenge Facts

**What goes wrong:** Red playresult mapping captures `AryChallengeIds`, `AryUserCompeIds`, and `AryBngCompeIds` into `CommonPlayResultData.StageData`, then the Red handler sends stages through `Ac15NormalPlayWriter.SaveAsync`; the shared writer persists normal play rows, bests, favorites, and recent songs but ignores challenge arrays.

**Why it happens:** For Green/Blue/Yellow those arrays have not mattered. The shared normal writer is intentionally about normal score rows, and challenge side effects are outside its current responsibilities.

**Consequences:** Red normal play, score, crown, favorites, and recent rows may look correct, but Don Challenge progress/high scores are silently lost. Later adding challenge persistence becomes a backfill problem with no raw facts.

**Prevention:** In the Red playresult handler, classify and preserve challenge stage facts before or alongside shared normal writes. Decide from evidence whether challenge stages also count as normal play; either way, do not rely on the normal writer to retain competition metadata.

**Detection:** Red playresult tests include challenge arrays but only assert normal score/best rows. No Red table stores `CompeId` plus `TrackNo`. The only use of challenge arrays remains in mappers.

**Phase implications:** Red normal play should land before challenge only if the plan explicitly preserves/logs challenge facts for replay, or challenge handling should be part of the same phase slice.

### Pitfall 4: Copying Yellow Medal, Shop, Or WaiWai Semantics Into Red

**What goes wrong:** Red receives Yellow Don/Katsu medal state, Yellow item-shop purchase flow, or Yellow WaiWai fields just because Red is "Yellow-like."

**Why it happens:** Yellow completed immediately before Red and has a rich AC15 implementation. But Red proto shows `get_donpoint`, `total_get_donpoint`, `total_use_donpoint`, `reward_ptn`, and `reward_progress`; it does not show Yellow WaiWai fields, and `proto/red/taiko.proto` does not expose the Yellow item-shop purchase surface.

**Consequences:** Red persistence gains fake fields, the WebUI may show unsupported shop/medal concepts, and the cabinet may receive fields/routes it never asked for.

**Prevention:** Start Red from proto/data inventory, then opt into shared Yellow-compatible behavior one capability at a time. Add Red save capabilities only for fields the Red wire and runtime actually use. Keep reward/donpoint behavior separate from Yellow Don/Katsu medals until evidence says otherwise.

**Detection:** Red entities contain `WaiwaiTutorialFlg`, Don/Katsu medal totals, or Yellow shop tables before a Red route/proto/data proof exists. Red route scaffolding includes `getitemshopinfo.php` or `itempurchase.php` without a Red proto message and runtime trace.

**Phase implications:** The foundation phase must explicitly mark absent Red WaiWai and absent Yellow item-shop purchase behavior. Reward/donpoint should have its own evidence-backed requirement instead of being folded into Yellow shop reuse.

### Pitfall 5: Guessing The Red Runtime Data Root

**What goes wrong:** Catalog support hardcodes the first or most familiar Red config root and later fails on real startup/version behavior.

**Why it happens:** Local Red data currently exposes multiple versioned roots: `ST5100-1`, `ST5100-7`, `ST7100-1`, and `ST8100-1`. Public era names and file naming are tempting, but Red runtime target/root still needs proof.

**Consequences:** Music, Taikojuku, folders, telops, tournaments, rewards, or challenge data can be loaded from the wrong version. Tests pass against a chosen fixture while RPCS3 requests mismatch the catalog.

**Prevention:** Record the Red route/version/root decision before catalog implementation. Use startup/version logs, `hddVer`/request evidence, and local file inventory to pick the active root. Keep root selection configurable through existing path helpers and era catalog helpers.

**Detection:** `ST5100-1`, `ST7100-1`, or `ST8100-1` appears hardcoded in handlers or controllers. Catalog tests only prove files load from a single guessed folder.

**Phase implications:** Red foundation should include a route/version/data-root evidence artifact before Red catalog and normal play phases.

### Pitfall 6: Polluting Shared AC15 Modules With Red Table Switches

**What goes wrong:** Red support adds `GameEra.Red` cases inside shared `Application/Ac15` modules for normal play, Dani, item shop, userdata, or catalog behavior.

**Why it happens:** Red is the fourth AC15 era, and adding another switch looks faster than binding Red concrete tables and policies at the handler composition edge.

**Consequences:** Phase 16.2's capability-composition cleanup regresses. Shared modules become era routers again, and future older-era support becomes harder to audit for no-cross-era writes.

**Prevention:** Follow the post-review AC15 shape: Red handlers bind concrete Red `DbSet`s, Mapperly delegates, limits, stage policies, and save capabilities; switch-free generic helpers do the identical work. Keep real Red-only behavior in the Red handler or a narrow Red helper.

**Detection:** `GameEra.Red` appears inside shared modules whose job is table-independent behavior, or new methods named `SaveRedAsync`, `PurchaseRedAsync`, or `BuildRed...` appear in shared AC15 services.

**Phase implications:** Every phase that touches `Application/Ac15` should include a design check for "shared behavior module or era composition root?"

### Pitfall 7: Reusing Another Era's Persistence Table

**What goes wrong:** Red uses Yellow, Green, or Blue save/score/Dani/favorite/recent/shop/Tokkun rows because the shapes are similar.

**Why it happens:** The model snapshot shows repeated Blue/Green/Yellow table shapes, including `IsChallengeCompe`, Tokkun fields, normal score rows, and shop rows. It can feel wasteful to add another table family.

**Consequences:** Red writes corrupt another era's user history, AdminApi/WebUI readback becomes ambiguous, and no-cross-era regression tests lose meaning.

**Prevention:** Add Red-owned EF entities, `DbSet`s, mappings, and migrations for every Red gameplay state surface. Share algorithms through row-shape interfaces and generic helpers, not shared gameplay tables.

**Detection:** Red code references `UserSaveDataYellow`, `SongPlayDatumYellow`, `YellowTokkunStageResults`, or any non-Red gameplay `DbSet`. Migration diffs do not add Red tables for new Red state.

**Phase implications:** Red identity/normal play/Tokkun/challenge phases must include schema review and no-cross-era persistence tests for the specific state they add.

### Pitfall 8: Treating Banacoin Or Donpoint As Wallet Authority

**What goes wrong:** Red Banacoin-adjacent routes or Don Challenge rewards create wallet, balance, coupon, transaction, or payment authority state.

**Why it happens:** Red proto contains `Balancecheck`, `Banacoinpayment`, `Banacoinerrorlog`, `Getbanacoininfo`, reward execution, and donpoint fields. The presence of those messages can look like a complete economy.

**Consequences:** The repo becomes a fake Banacoin/payment authority, and Don Challenge or reward behavior can start mutating unrelated balances.

**Prevention:** Keep Banacoin-adjacent routes stateless compatibility unless concrete Red runtime evidence proves a server-owned state role. Treat donpoint/reward progression as Red gameplay state only where Red proto/data/runtime traces prove it, not as money.

**Detection:** New tables named wallet, balance, transaction, coupon, payment, receipt, or BNID state appear. Banacoin routes update Red donpoint/challenge rows without evidence.

**Phase implications:** Banacoin compatibility should be a bounded compatibility phase or subtask, separate from Red Don Challenge and reward progression semantics.

### Pitfall 9: Editing Dumped Proto Or Hand-Cleaning Generated Wire

**What goes wrong:** `proto/red/taiko.proto` is modified to make generation or mapping easier, or generated Red `Wire/` files are manually cleaned up after generation.

**Why it happens:** Red proto2 optional fields, nullable primitive generation, and mapper presence semantics can be inconvenient. Prior Yellow work already showed this is a high-risk temptation.

**Consequences:** Source evidence is destroyed, generated wire stops representing the cabinet contract, and optional zero-vs-absent behavior becomes untrustworthy.

**Prevention:** Treat `proto/red/` as immutable local evidence unless `protogen` compatibility absolutely forces a documented workaround. Regenerate Red wire into the Red adapter with the repo-local nullable optional primitive conventions, then fix mappers and Application DTOs rather than proto source.

**Detection:** Diffs under `proto/red/`, hand edits inside adapter `Wire/` files unrelated to regeneration, or production code using generated `ShouldSerialize*` calls instead of explicit mapper contracts.

**Phase implications:** The Red foundation phase should make wire generation a first-class task with exact command, output target, and mapper-presence verification.

## Moderate Pitfalls

### Pitfall 10: Route Prefix And Startup Ownership Drift

**What goes wrong:** Red game routes, shared startup/version routes, and Host fallback routing are copied from Yellow without proof.

**Prevention:** Red route prefix, enabled-era application part gating, and shared `/v01r00/chassis/*` startup/version behavior should be proven from Red request evidence. Do not duplicate shared startup routes under a Red prefix unless the client actually calls them there.

**Warning signs:** A Red adapter is added with a guessed `/v08...` or `/v05...` prefix and route inventory tests before any route evidence artifact exists.

### Pitfall 11: Test Coverage That Proves Shape, Not Behavior

**What goes wrong:** Tests assert route attributes, generated property existence, `Result = 1`, or controller method names while missing state transitions and no-cross-era boundaries.

**Prevention:** For Red, tests should protect observable behavior: Red-owned rows, challenge readback data, normal/Tokkun/challenge no-cross-mode writes, catalog parser output, protocol packing, and runtime output copy. Mapper tests are useful only for nontrivial classification, omission, optional presence, or field placement.

**Warning signs:** A Red test would still pass if `GetChallengeCompeQuery.Red` returned an empty response, or if Red playresult wrote Yellow tables.

### Pitfall 12: Losing Optional Presence Semantics

**What goes wrong:** Red proto2 optional fields are represented as default zeros in common DTOs or save rows where absence matters.

**Prevention:** Keep optional primitive presence at the adapter boundary through generated nullable option support where available and explicit mapper contracts. Use nullable Application fields for protocol-backed optional readback such as Tokkun tutorial or other Red-only optional state.

**Warning signs:** Red mappers collapse optional fields to `0` before handler logic, or production code spreads generated presence helpers through business logic.

### Pitfall 13: Assuming Crown Encoding Or Bit Widths From Blue/Yellow

**What goes wrong:** Red crown flags, release-song flags, tone/title/costume widths, or gzip/raw response behavior are copied from Blue/Green/Yellow without proof.

**Prevention:** Reuse `Ac15ProtocolBytes` and `Ac15CrownService` only after Red profile limits and wire placement are proven. Red crown response encoding should have a focused exact-wire test before runtime closeout.

**Warning signs:** Red profile limits are pasted from Yellow with no proto/data note, or crown tests only inspect inflated helper bytes.

### Pitfall 14: Sidecar Data Exists In Source But Not Runtime Output

**What goes wrong:** Red committed JSON sidecars for folders, telops, recommendations, rewards, movies, or challenge metadata are created but not copied into Host build/publish output.

**Prevention:** For every Red server-authored sidecar, update Host output copy rules and verify with a temp-output Host build when `Host/bin` may be locked. Empty JSON is acceptable only when evidence says the dataset is intentionally empty.

**Warning signs:** Catalog loader logs empty data even though source JSON exists, or tests load source paths while runtime uses build output.

### Pitfall 15: Hiding Red Challenge Behind Generic "Special Mode" Machinery

**What goes wrong:** Don Challenge, Tokkun, Blue battle, Green AI/ghost, and Yellow WaiWai are collapsed into a broad special-mode abstraction.

**Prevention:** Keep Red challenge behavior explicit until its state transitions are known. Shared code may handle identical row writing or readback projection later, but mode classification and side effects belong at the era boundary.

**Warning signs:** A generic special-mode enum starts deciding challenge, battle, Tokkun, and WaiWai writes from field presence alone.

## Minor Pitfalls

### Pitfall 16: AdminApi/WebUI Era Routing Without Red-Owned Backing State

**What goes wrong:** Red appears in WebUI/AdminApi selectors before Red-owned handlers can safely read/write Red rows.

**Prevention:** Add Red UI routing only with Red-owned readback paths and no-cross-era tests for edited settings, profile, score, Dani, favorites/recent, Tokkun, and any challenge visibility.

### Pitfall 17: Over-Documenting Unsupported Features As Deferred Rather Than Absent

**What goes wrong:** Red docs say unsupported Blue battle, Yellow WaiWai, or Yellow item shop behavior is "future" even when current Red evidence says absent.

**Prevention:** Mark unsupported features absent unless new Red evidence appears. Do not create routes/controllers/stubs for absent features.

### Pitfall 18: Ignoring Existing Dirty Worktree Or Locked Host Output

**What goes wrong:** Planning or implementation commits include unrelated local data/docs, or `dotnet build Host` fails because the running server locks normal debug output.

**Prevention:** Keep commits path-limited and use `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` for build verification when needed.

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|----------------|------------|
| Red evidence and foundation | Challenge competition treated as already solved because Yellow/Green/Blue have surfaces | Prominently record the user correction and require Red-specific route/proto/data/runtime evidence |
| Red wire generation | Dumped proto modified or generated wire hand-edited | Keep `proto/red/` immutable; regenerate adapter wire and adapt mappers |
| Red route/version setup | Route prefix or startup route ownership guessed | Capture route/version/root evidence before adding broad controllers |
| Red catalog bootstrap | Wrong `ST*` config root selected | Prove active root from request/version evidence and keep path resolution through helpers |
| Red identity/userdata | `IsChallengeCompe` exposed without challenge backing state | Keep false until challenge state/readback exists, or implement the challenge state in the same slice |
| Red normal play | Shared normal writer drops challenge arrays | Preserve/log/persist challenge facts before or beside normal writes |
| Red Don Challenge | Empty `ChallengeCompeResponse` passes tests | Require non-empty readback cases from persisted Red challenge rows and mapper field-placement tests |
| Red rewards/donpoint | Yellow medals or Banacoin wallet state copied | Model only Red proto-backed donpoint/reward facts and keep Banacoin stateless |
| Red Tokkun | Tokkun-shaped uploads fall into normal/challenge writes | Add Red classifier before normal save and assert no normal/challenge/shop writes unless evidence says otherwise |
| Red AdminApi/WebUI | UI exposes Red via another era's data path | Route through Red-owned AdminApi handlers and Red tables only |
| Runtime closeout | Server tests treated as compatibility proof | Require repeatable cabinet/RPCS3 smoke evidence for Red normal, Tokkun if supported, and Don Challenge if in scope |

## Looks Done But Is Not Checklist

- [ ] Red route/version/root evidence exists before catalog and runtime phases.
- [ ] `proto/red/` remains unchanged; generated Red wire is adapter-local.
- [ ] Red has Red-owned EF rows for every gameplay state it writes.
- [ ] No Red handler writes Blue, Green, Yellow, or Nijiiro gameplay tables.
- [ ] `GetChallengeCompeQuery.Red` is not an empty stub once Don Challenge is in scope.
- [ ] Red playresult challenge arrays are not discarded by the normal save path.
- [ ] Red `is_challengecompe` readback is backed by real Red challenge availability/state.
- [ ] Red does not expose Yellow WaiWai fields or Yellow item-shop purchase routes without Red evidence.
- [ ] Red donpoint/reward behavior is separate from Banacoin compatibility and Yellow medals.
- [ ] Red Tokkun, if implemented, classifies before normal/challenge writes and persists only protocol-backed facts.
- [ ] Exact Red crown/bitset wire encoding is verified before closeout.
- [ ] Host build/publish output contains required Red sidecar JSON files.
- [ ] Runtime closeout records cabinet/RPCS3 smoke evidence, not just server tests.

## Sources

- `.planning/PROJECT.md` - Red v1.3 scope, evidence hierarchy, and challenge competition correction.
- `.planning/STATE.md` - current milestone state and Phase 16.2 shared-core decisions.
- `.planning/milestones/v1.2-REQUIREMENTS.md` - Yellow shipped requirements and boundaries to reuse or avoid.
- `AGENTS.md` - repo architecture, testing rules, era-state separation, and data caveats.
- `proto/red/taiko.proto` - Red protocol fields for userdata, playresult, rewards, Banacoin-adjacent routes, and challenge competition.
- `Application/Handlers/GetChallengeCompeQuery.Yellow.cs` and `Application/Handlers/GetChallengeCompeQuery.Green.cs` - newer-era empty challenge stubs that Red must not copy as behavior.
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` and `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs` - Yellow special-mode gate and no-cross-write pattern.
- `Application/Ac15/Ac15NormalPlayWriter.cs` - shared normal writer that intentionally does not persist challenge arrays.
- `Application/Ac15/Ac15ItemShopPurchase.cs` - shared item-shop workflow to reuse only when Red has a matching route/protocol contract.
- `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` - existing Blue/Green/Yellow separate table patterns and `IsChallengeCompe` fields.
- `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md` - current AC15 shared-core architecture after Phase 16.2 review follow-up.

---
*Pitfalls research for: v1.3 Red AC15 Support*
*Researched: 2026-06-12*
