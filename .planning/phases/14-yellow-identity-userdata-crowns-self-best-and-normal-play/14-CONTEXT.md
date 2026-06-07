# Phase 14: Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play - Context

**Gathered:** 2026-06-08
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 14 turns the Phase 12/13 Yellow no-state runtime scaffolds into a Yellow-owned normal-play loop: BAID/profile/default save creation, mydon entry, userdata readback, normal playresult persistence, self-best readback, and crown response proof. It must use Yellow-owned persistence and adapter-local Yellow wire mapping while reusing shared AC15 helpers only where the existing shared core already matches Yellow protocol/data evidence.

This phase does not implement Yellow Dani runtime persistence beyond normal-play field boundaries, item purchase/shop spend semantics, Don/Katsu medal shop accounting beyond normal playresult fields, WaiWai beyond preserving/logging protocol fields needed by normal play, Tokkun classification/persistence, Banacoin compatibility, AdminApi/WebUI readback, runtime cabinet/RPCS3 smoke, or any Yellow battle behavior.

</domain>

<decisions>
## Implementation Decisions

### Identity and Default Save State
- **D-01:** Implement Yellow BAID and mydon flows as first-class Yellow routes under `/v09r00/chassis/*`, replacing the Phase 12 no-state `baidcheck.php` and `mydonentry.php` scaffolds with Mediator-backed behavior.
- **D-02:** Reuse shared card/access-code identity only as shared identity state; create and read Yellow gameplay/profile state through Yellow-owned save tables, entities, migrations, `ITaikoDbContext` partials, and `GetOrCreateYellowSaveDataAsync`-style helpers.
- **D-03:** Default Yellow save state should mirror proven AC15 defaults from Blue/Green only where Yellow proto fields support them: mydon name, title/titleplate, colors/costumes, costume flags, tone/title flags, auto-costume flag, display/default settings, medal counters, tutorial flags, and last-play timestamps.
- **D-04:** A new card response may follow the existing Green/Blue `PlayerType = 1` create path, but existing-card responses must map full Yellow profile/default-save state into Yellow `BAIDResponse` and `MydonEntryResponse` fields, not Green/Blue wire DTOs.

### Userdata Readback Contract
- **D-05:** Replace Yellow `userdata.php` no-state scaffold with a Yellow `UserDataQuery` partial and Yellow adapter mapper that compose `UserDataResponse` from Yellow-owned save state, Yellow catalog snapshot, favorites, recent songs, and unlock flags.
- **D-06:** Use `Ac15UserDataService` and `Ac15UserDataSnapshot` for shared profile/readback composition where fields match, with a Yellow adapter/snapshot that supplies Yellow save flags, catalog release rows, locked shop/catalog rows, favorites, recent songs, recommendation rows, counters, display Dan fallback, and tutorial flags.
- **D-07:** Read back only Yellow-supported Phase 14 fields now: profile/settings, release song flags, tone/title flags, option/default settings, favorites, recent songs, category/play counters, recommendation fields, challenge/tojiru flags, `difficulty_played_course`, `difficulty_played_star`, and currently stored tutorial flags.
- **D-08:** `tokkun_tutorial_flg` exists in Yellow proto but Phase 16 owns Tokkun persistence/readback. Phase 14 may include a nullable Yellow save column or response mapping only if needed for schema shape, but it must not classify or persist Tokkun playresults.
- **D-09:** Fields whose semantics belong to Phase 15/16, including Dani clear state, item-purchase/shop unlock state, WaiWai tutorial semantics beyond normal play logging, and Tokkun history, should remain absent, defaulted, or explicitly deferred unless Phase 14 normal-play requirements need a protocol-safe placeholder.

### Normal Playresult Side Effects
- **D-10:** Replace Yellow `playresult.php` no-state scaffold with a Yellow playresult mapper, `UpdatePlayResultCommand.Yellow` handler, and Yellow normal-play persistence adapter.
- **D-11:** Normal Yellow playresults should persist Yellow-owned play history rows, best-score rows, profile counters, unlock bitsets, favorites, and recent songs through Yellow tables only. Add no writes to Blue, Green, Nijiiro, battle, Tokkun, or shop-purchase tables.
- **D-12:** Use `Ac15NormalPlayService`, `IAc15NormalPlayPersistence`, `DefaultAc15EraHooks`, and `Ac15EraProfiles.Yellow` for the core save loop if Yellow stage modes/course limits match the shared AC15 profile. Yellow-specific validation belongs in a Yellow handler/adapter, not in Blue/Green code.
- **D-13:** Treat `play_mode` and `stage_mode` conservatively: Phase 14 handles normal play only. Tokkun-shaped uploads must not be consumed as normal score/crown writes; if the mapper can detect Tokkun evidence, leave classification/no-write behavior for Phase 16 and add guard tests that Phase 14 does not invent Tokkun writes.
- **D-14:** Normal playresult fields for Don/Katsu medal totals can be captured in Yellow-owned save state when they are direct upload counters, but item purchase, spending, shop season state, and duplicate-purchase behavior remain Phase 15.
- **D-15:** Preserve raw Yellow song order/duplicates only where a Phase 14 normal-history table naturally records each stage row. Do not use Tokkun-style append-only history semantics for normal play.

### Self-Best and Crown Encoding Proof
- **D-16:** Replace Yellow `selfbest.php` no-state scaffold with a Mediator-backed query that returns Yellow-owned best rows for requested songs and difficulty through a Yellow mapper.
- **D-17:** Use `Ac15SelfBestService` for response row construction, but verify Yellow wire mapping explicitly because Yellow `SelfBestData` includes `self_best_score` and `ura_best_score` in each repeated row plus a separate `ary_shin_selfbest_score` collection.
- **D-18:** Replace Yellow `crownsdata.php` no-state scaffold with a Yellow crown readback route that uses Yellow best rows, Yellow catalog song numbers, `Ac15CrownService`, and `Ac15EraProfiles.Yellow.Limits`.
- **D-19:** YCRN-01 requires explicit proof of both shared crown packing and exact Yellow response placement/encoding. Planning must include a raw protobuf/byte-level test for `CrownsDataResponse.hash_crown_flg` and an explicit compression-vs-raw assertion instead of assuming Blue/Green gzip behavior.
- **D-20:** If Yellow crown transport is not proven by existing local proto/wire tests alone, downstream research must inspect local Yellow runtime/client evidence before choosing gzip or raw `hash_crown_flg`. Do not silently copy Blue/Green `GZipBytesUtil.GetGZipBytes` behavior.

### the agent's Discretion
- The coordinator selected all four gray areas because each maps directly to Phase 14 requirements and success criteria.
- The agent may choose the smallest Yellow persistence/entity surface that satisfies YUSR-01, YUSR-02, YPLY-01, YPLY-02, and YCRN-01, but must keep table names, handlers, route mappers, and tests auditable as Yellow-owned.
- The agent may extract additional shared AC15 helpers only when they reduce real duplication and preserve era-owned wire DTOs, routes, and persistence adapters.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope and Requirements
- `.planning/ROADMAP.md` - Phase 14 goal, success criteria, and Phase 15/16/17 boundaries.
- `.planning/REQUIREMENTS.md` - YUSR-01, YUSR-02, YPLY-01, YPLY-02, YCRN-01, and Yellow out-of-scope constraints.
- `.planning/PROJECT.md` - Yellow milestone evidence hierarchy, state-separation constraints, and current project decisions.
- `AGENTS.md` - Repo architecture rules, Blue/Yellow state caveats, GSD workflow expectations, and no-cross-era behavior rules.

### Prior Yellow Evidence
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` - `/v09r00` Yellow route prefix, direct-protobuf scaffold expectations, supported route suffixes, shared startup/version ownership, and no-battle absence contract.
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-CONTEXT.md` - First-class Yellow adapter, adapter-local wire, transport, and evidence-boundary decisions.
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-VERIFICATION.md` - Verified Phase 12 scaffold/route/wire/no-battle state.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-CONTEXT.md` - Yellow catalog/core decisions and explicit deferral of identity, userdata, normal play, self-best, crowns, and gameplay writes to Phase 14.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-RESEARCH.md` - Yellow catalog/profile/core research, shared AC15 service anchors, and no-persistence guardrails.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-PATTERNS.md` - Yellow catalog patterns, route replacement boundaries, and Phase 13 no-state deferred route list.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-VERIFICATION.md` - Verified Yellow catalog/core implementation and deferred runtime-route status.

### Protocol and Wire Evidence
- `proto/yellow/yellow.proto` - Yellow `BAID*`, `MydonEntry*`, `UserData*`, `PlayResult*`, `SelfBest*`, and `CrownsData*` field contracts.
- `Adapters.GameProtocol.Yellow/Wire/Game.cs` - Generated Yellow DTO placement for `hash_crown_flg`, self-best rows, userdata fields, and normal playresult fields.
- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` - Current Phase 13 route state: metadata routes are Mediator-backed; Phase 14-owned routes remain no-state scaffolds.

### AC15 Shared-Core Direction
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - Approved capability-driven AC15 sharing design for Blue/Green/future Yellow/Red while keeping era state separate.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/README.md` - Plan-directory overview for AC15 shared core extraction.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/05-userdata-core-and-era-adapters.md` - Userdata core and era adapter sharing guidance.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/06-normal-play-core-and-hooks.md` - Normal-play core, hooks, and persistence-adapter guidance.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Application/Ac15/Ac15EraProfiles.cs` and `Application/Ac15/Ac15ProtocolLimits.cs`: Yellow profile already exists with normal play, userdata, self-best, crowns, and dedicated crown endpoint capabilities.
- `Application/Ac15/Ac15UserDataService.cs` and `Application/Ac15/Ac15UserDataRecords.cs`: shared userdata response composition and snapshot shape for profile counters, flags, favorites, recent songs, recommendations, and tutorial flag placement.
- `Application/Ac15/Ac15SelfBestService.cs`: shared self-best response construction for requested songs and difficulty.
- `Application/Ac15/Ac15CrownService.cs` and `Application/Ac15/Ac15ProtocolBytes.cs`: shared crown body packing and fixed-width bitset helpers.
- `Application/Ac15/BlueAc15NormalPlayAdapter.cs` and `Application/Ac15/GreenAc15NormalPlayAdapter.cs`: concrete normal-play persistence adapter patterns for history rows, best rows, favorites, and recent songs.
- `Application/Handlers/BaidQuery.Blue.cs`, `BaidQuery.Green.cs`, `UserDataQuery.Blue.cs`, `UserDataQuery.Green.cs`, `UpdatePlayResultCommand.Blue.cs`, and `UpdatePlayResultCommand.Green.cs`: existing AC15 identity/userdata/playresult handler analogs.
- `Adapters.GameProtocol.Blue/Mappers/*` and `Adapters.GameProtocol.Green/Mappers/*`: adapter-local BAID, userdata, playresult, self-best, and crown route mapping analogs.
- `Tests/Ac15/Ac15UserDataServiceTests.cs`, `Ac15SelfBestServiceTests.cs`, `Ac15CrownServiceTests.cs`, and `Ac15ProtocolBytesTests.cs`: shared helper test anchors to extend for Yellow-specific proof.
- `Tests/Yellow/YellowCatalogBoundaryTests.cs`, `YellowRouteSkeletonTests.cs`, and `YellowNoBattleSourceGuardTests.cs`: Yellow route/source guard patterns to update as Phase 14 replaces no-state scaffolds.

### Established Patterns
- Era-specific runtime behavior uses unsuffixed dispatcher files plus `.Blue.cs`, `.Green.cs`, or future `.Yellow.cs` partials.
- Protocol controllers deserialize Yellow wire DTOs, map to common DTOs/commands, call Mediator, and map back to Yellow wire DTOs.
- Generated wire DTOs remain adapter-local; Yellow must not import Blue/Green wire DTOs or create a shared generated AC15 wire assembly.
- EF state stays physically separate by era. Shared identity can use existing card/user identity rows, but gameplay/save/profile state must be Yellow-owned.
- AC15 shared services are acceptable only behind era profiles, snapshots, hooks, and persistence adapters.
- Disabled-era route absence is handled through Host application-part filtering; Yellow route changes must preserve enabled-era behavior and disabled-route tests.

### Integration Points
- Add Yellow DbSets and migrations through `Domain/Entities`, `Application/Abstractions/ITaikoDbContext.Yellow.cs`, and `Infrastructure/Persistence/TaikoDbContext*.Yellow.cs` patterns.
- Add Yellow save-data creation/default helpers alongside `UserSaveDataBlueExtensions` and `UserSaveDataGreenExtensions`.
- Add Yellow handler partials for `BaidQuery`, `UserDataQuery`, `UpdatePlayResultCommand`, and `GetSelfBestQuery`.
- Add Yellow adapter mappers/controllers for BAID, mydon, userdata, playresult, self-best, and crowns in `Adapters.GameProtocol.Yellow`.
- Update Yellow route/source tests to prove Phase 14-owned routes are no longer no-state scaffolds while Phase 15/16 routes remain deferred.
- Add no-cross-era-write tests that seed Blue/Green/Nijiiro state, exercise Yellow normal flow, and prove only Yellow-owned tables changed.

</code_context>

<specifics>
## Specific Ideas

Coordinator selected all four discuss areas because they map directly to the Phase 14 success criteria:

- Identity/default save state supports success criterion 1 and YUSR-01.
- Userdata readback supports success criterion 2 and YUSR-02.
- Normal playresult side effects support success criterion 3 and YPLY-01.
- Self-best and crown encoding proof support success criteria 4-5 and YPLY-02/YCRN-01.

Use Yellow local protocol evidence as the field source. In particular, Yellow proto exposes:

- `BAIDResponse` and `MydonEntryResponse` profile/costume/medal/Dan/content fields.
- `UserDataResponse` favorite/recent, release song, tone/title, counters, default settings, `disp_taikojuku_dan`, difficulty-played, Tokkun tutorial, challenge, and tojiru fields.
- `PlayResultRequest` normal stage rows, unlock arrays, Don/Katsu medal counters, costume/current costume, `play_mode`, area, Dan result, WaiWai/Tokkun-adjacent fields later in the message.
- `SelfBestResponse` normal/ura score fields plus `ary_shin_selfbest_score`.
- `CrownsDataResponse.hash_crown_flg` as field 3.

</specifics>

<deferred>
## Deferred Ideas

- Yellow Dani persistence/readback, Dan playresult semantics, and display-Dan behavior beyond Phase 14 safe placeholders belong to Phase 15.
- Yellow item purchase, shop season state, duplicate prevention, spend/use medal accounting, and shop unlock application belong to Phase 15.
- Yellow WaiWai tutorial/logging beyond normal-play field preservation belongs to Phase 15 unless Phase 14 needs a no-op/default mapping to keep userdata safe.
- Yellow Tokkun classification, no-cross-mode write tests, nullable tutorial persistence/readback semantics, and append-only raw stage history belong to Phase 16.
- Yellow Banacoin-adjacent compatibility belongs to Phase 16.
- AdminApi/WebUI Yellow readback belongs to Phase 15 per roadmap, not this normal protocol phase.
- Yellow cabinet/RPCS3 runtime smoke and final contract closeout belong to Phase 17.
- Yellow battle remains out of scope unless new concrete Yellow proto/log/client evidence appears.

</deferred>

---

*Phase: 14-Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play*
*Context gathered: 2026-06-08*
