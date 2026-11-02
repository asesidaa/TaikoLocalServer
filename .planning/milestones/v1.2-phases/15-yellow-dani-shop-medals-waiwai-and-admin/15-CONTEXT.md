# Phase 15: Yellow Dani, Shop, Medals, WaiWai, and Admin - Context

**Gathered:** 2026-06-08
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 15 completes the remaining Yellow normal-era runtime/admin surfaces before Tokkun and Banacoin compatibility: Yellow-owned Dani/Taikojuku state, Yellow item-shop purchase and reward compatibility behavior, Don/Katsu medal accounting, WaiWai tutorial/readback and playresult logging where the Yellow wire exposes fields, and AdminApi/WebUI Yellow readback for the supported profile, score, recent/favorite, Dani, shop-relevant, Tokkun placeholder, and catalog surfaces.

This phase builds on Phase 13 catalog-backed Yellow metadata and Phase 14 Yellow-owned identity/userdata/normal play state. It must not start Phase 16 Tokkun persistence/classification/readback or Banacoin compatibility, must not add Yellow battle behavior, and must not read or write Blue/Green/Nijiiro gameplay tables.

</domain>

<decisions>
## Implementation Decisions

### Yellow Dani Runtime
- **D-01:** Treat Yellow Dani as an era-owned stateful runtime surface under `/v09r00/chassis/taikojuku.php`, `playresult.php`, and `userdata.php`; do not reuse Green/Blue Dan tables, wire DTOs, or persistence helpers directly.
- **D-02:** Keep Phase 13 catalog-backed `taikojuku.php` request/readback as the catalog source for Dan packs, then add Yellow Dan result persistence through Yellow-owned `DanScoreDatumYellow` and `DanStageScoreDatumYellow`-style tables.
- **D-03:** Handle Yellow Dan playresults only when the existing Yellow playresult mapper exposes Dan evidence (`PlayMode == DanMode`, `DanResult`, and stage `PlayDan` values). Unknown Dan ids or invalid clear grades should return protocol success without writing Dan rows, matching the proven Blue/Green safety pattern.
- **D-04:** Update Yellow save summary fields from valid Dan clears only: packed got-Dan flags, `GotDanMax`, `DispTaikojukuDan`, display-Dan fallback, and supported Dan costume/auto-costume behavior where Yellow save fields and existing AC15 policy support it.
- **D-05:** Dan playresults may still produce normal stage rows through the shared AC15 normal-play service where Phase 14 already allows Dan-mode normal rows, but they must not write shop purchases, Tokkun, Banacoin, battle, or cross-era state.

### Yellow Shop And Reward Routes
- **D-06:** Keep `getitemshopinfo.php` catalog-backed through the Phase 13 Yellow item-shop mapper, preserving Yellow response placement (`GetitemshopinfoResponse.ary_itemshop_data` at Yellow field 5) instead of copying Blue-only wire assumptions.
- **D-07:** Replace Yellow `itempurchase.php` no-state success with Mediator-backed Yellow purchase behavior using `Ac15ItemShopService` plus a Yellow-owned `IAc15ItemShopPersistence`/unlock adapter.
- **D-08:** Add Yellow-owned shop season and purchased-item state tables rather than sharing Green/Blue shop tables or adding a shared discriminator table.
- **D-09:** Purchases must validate the active Yellow shop season/item row, item type/id/price, available Yellow Don medals, and duplicate purchase state before spending medals or applying unlocks.
- **D-10:** Supported shop unlock writes apply only to Yellow save flags for songs, tones, titles, costumes/body/head/face/puchi/kigurumi-style parts where the shared AC15 item type contract and Yellow save fields support them. Unknown item types should fail safely or be stored only when evidence-backed.
- **D-11:** Keep `rewardcardcheck.php` and `rewardexecution.php` as log-and-success compatibility unless local Yellow evidence proves a state-changing role. They must not duplicate purchase effects or mutate Banacoin/payment state.

### Don/Katsu Medal Accounting
- **D-12:** Preserve Phase 14 direct playresult accumulation of Yellow `TotalGetDonmedal` and `TotalGetKatsumedal` on Yellow save data.
- **D-13:** Introduce Yellow shop-season Don medal state for purchase availability and spend tracking, seeded/updated from Yellow-owned save/playresult state using the existing Green/Blue AC15 shop-state pattern where it matches.
- **D-14:** Katsu medals remain Yellow-owned profile/readback state unless current Yellow proto/code evidence proves a Katsu shop spend path. Do not fold Katsu medals into Don medal spend accounting.
- **D-15:** Medal state is separate from Banacoin compatibility. No Yellow Banacoin balance, payment, coupon, settlement, receipt, wallet, or transaction persistence belongs in Phase 15.

### WaiWai Tutorial And Logging
- **D-16:** WaiWai is not a play mode. Do not branch Yellow playresult handling by a new WaiWai mode or treat WaiWai as Tokkun/battle-like classification.
- **D-17:** Persist and read back only the Yellow WaiWai tutorial flag when the current Yellow wire/runtime evidence exposes the field. If Yellow generated wire lacks a `waiwai_tutorial_flg` field, record that as a current evidence gap and avoid inventing a response surface.
- **D-18:** Log additional WaiWai playresult facts that are present in Yellow wire/common DTOs, but store only protocol-backed fields and keep logging append-only/diagnostic rather than gameplay-authoritative unless evidence proves semantics.
- **D-19:** Continue preserving normal-play stage `WaiwaiResult` and `WaiwaiGauge` values in Yellow play-history rows through the existing shared `CommonPlayResultData.Green` partial shape if Yellow mapper exposes them; do not make those fields unlock, score, crown, shop, or Dan authorities.

### AdminApi And WebUI Yellow Readback
- **D-20:** Add Yellow to AdminApi era routing and WebUI supported-era helpers without changing legacy Nijiiro/Green/Blue routes. Preserve `/api/{era}/...` parsing through `EraRoute.TryParse` and WebUI URL construction through `WebUiEra`.
- **D-21:** AdminApi/WebUI readback must use Yellow-owned tables for profile/settings, scores, play history, favorites/recent songs, Dan best data, shop-relevant state, and supported catalog data. No Yellow admin route may fall back to Blue/Green gameplay tables.
- **D-22:** Yellow catalog readback should use `IYellowCatalog`/`IGameDataCatalog.For(GameEra.Yellow)` and existing `GameDataService`/catalog DTO patterns for music, titles, tones, costumes, Dan data, and supported metadata.
- **D-23:** Yellow Tokkun admin visibility in Phase 15 is limited to routing/readback readiness or explicitly empty/unsupported state where Phase 16 data does not exist yet. Do not implement Tokkun persistence, history, tutorial readback, or Banacoin compatibility in Phase 15.
- **D-24:** WebUI should treat Yellow as an AC15 supported era for the same existing profile, high-score, play-history, song-list/song-detail, Dani, and favorite workflows already generalized for Green/Blue, with focused adjustments only where Yellow DTO/state differs.

### the agent's Discretion
- The coordinator selected all Phase 15 areas because they map directly to YDAN-01, YSHOP-01, YSHOP-02, YMED-01, YWAI-01, and YUI-01. No narrowing prompt was needed.
- Downstream agents may choose the smallest Yellow persistence and mapper surface that satisfies the requirements, and may extract shared AC15 helpers only when they reduce real duplication while keeping Yellow routes, generated wire DTOs, tables, and admin routing auditable as Yellow-owned.
- Downstream agents should decide exact plan slicing, test filters, and whether to split existing large Yellow controller files during planning/execution. This context does not authorize implementation yet.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope and Requirements
- `.planning/ROADMAP.md` - Phase 15 goal, requirements, success criteria, and Phase 16/17 boundaries.
- `.planning/REQUIREMENTS.md` - YDAN-01, YSHOP-01, YSHOP-02, YMED-01, YWAI-01, YUI-01, future YWAI-02/YBAN-02, and Yellow out-of-scope constraints.
- `.planning/STATE.md` - Current milestone position and Phase 14 completion handoff.
- `.planning/PROJECT.md` - Yellow milestone evidence hierarchy, state separation, AC15 shared-core direction, WaiWai/Banacoin boundaries, and no-battle contract.
- `AGENTS.md` - Repo architecture rules, era-owned state rules, Yellow/Blue caveats, and common commands.

### Prior Yellow Evidence and Implementation
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` - `/v09r00` Yellow route prefix, direct-protobuf expectations, supported route suffixes, shared startup/version ownership, no-battle absence contract, and deferred runtime scope.
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-RESEARCH.md` - Yellow proto inventory, route gaps, no-battle contrast, and scaffold guardrail patterns.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-CONTEXT.md` - Yellow catalog/core decisions, item-shop readback-only boundary, and Phase 15 deferrals.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-RESEARCH.md` - Yellow catalog/profile/core research, shared AC15 service anchors, item-shop catalog shape, and metadata route boundaries.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-PATTERNS.md` - Yellow catalog and metadata route replacement patterns, including deferred Phase 15 shop purchase and reward routes.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-VERIFICATION.md` - Verified Yellow catalog-backed metadata routes and deferred runtime route status.
- `.planning/phases/14-yellow-identity-userdata-crowns-self-best-and-normal-play/14-CONTEXT.md` - Yellow identity/userdata/normal-play decisions and explicit Phase 15/16 deferrals.
- `.planning/phases/14-yellow-identity-userdata-crowns-self-best-and-normal-play/14-RESEARCH.md` - Yellow EF/userdata/playresult/crown research, normal-play side-effect boundaries, and shared AC15 service anchors.
- `.planning/phases/14-yellow-identity-userdata-crowns-self-best-and-normal-play/14-VERIFICATION.md` - Verified Phase 14 stateful Yellow routes, no-cross-era writes, raw crown proof, and post-fix status.
- `.planning/phases/14-yellow-identity-userdata-crowns-self-best-and-normal-play/14-REVIEW.md` - Clean Phase 14 re-review and resolved warnings to preserve during Phase 15 work.

### Protocol and Current Code
- `proto/yellow/yellow.proto` - Yellow `Taikojuku*`, `Getitemshopinfo*`, `Itempurchase*`, `Rewardcardcheck*`, `Rewardexecution*`, `UserData*`, and `PlayResult*` field contracts.
- `Adapters.GameProtocol.Yellow/Wire/Game.cs` - Generated Yellow DTO placement for Dan fields, item-shop response shape, medal fields, Tokkun fields, and current absence/presence of WaiWai fields.
- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` - Current Yellow route state: Phase 13/14 routes are Mediator-backed; `itempurchase.php`, `rewardcardcheck.php`, and `rewardexecution.php` remain no-state scaffolds entering Phase 15.
- `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs` - Current Yellow direct-protobuf playresult mapping into shared `CommonPlayResultData`.
- `Adapters.GameProtocol.Yellow/Mappers/ItemShopMappers.cs` - Current Yellow item-shop info mapping and Yellow response shape.
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` - Current Yellow normal-play handler, medal accumulation, Tokkun-shaped no-write guard, and WaiWai/tutorial field handling inherited from the common shape.
- `Application/Ac15/YellowAc15UserDataAdapter.cs` - Current Yellow userdata snapshot, shop-lock readback hook, and Tokkun tutorial field currently omitted by Yellow profile placement.
- `Application/Ac15/Ac15ItemShopService.cs` - Shared AC15 purchase validation, duplicate prevention, spend, and unlock policy flow to reuse behind a Yellow adapter.
- `Application/Dtos/CommonPlayResultData.cs`, `Application/Dtos/CommonPlayResultData.Green.cs`, and `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` - Current common playresult field containers. There is no Yellow-specific `CommonPlayResultData.Yellow.cs` at context time.
- `TaikoWebUI/Utilities/WebUiEra.cs` - Current WebUI era list supports Nijiiro, Green, and Blue only; Yellow must be added deliberately.
- `Adapters.AdminApi/Controllers/DanBestDataController.cs` - Current AdminApi Dan readback supports Nijiiro, Green, and Blue only; Yellow must use Yellow-owned Dan rows.

### AC15 Shared-Core Direction
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - Approved capability-driven AC15 sharing design while preserving era-owned routes, wire DTOs, and persistence.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/README.md` - Shared-core plan overview for deciding whether a Phase 15 extraction is justified.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/05-userdata-core-and-era-adapters.md` - Userdata/admin readback sharing guidance.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/06-normal-play-core-and-hooks.md` - Normal-play hooks and persistence-adapter guidance relevant to Dani-mode side effects.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Application/Ac15/Ac15ItemShopService.cs`: shared purchase preflight, active-season lookup, item validation, duplicate prevention, spend tracking, unlock hook, and response totals.
- `Application/Ac15/IAc15ItemShopPersistence.cs` and `IAc15ItemShopUnlockPolicy.cs`: extension points for a Yellow shop adapter.
- `Application/Ac15/BlueAc15ItemShopAdapter.cs` and `GreenAc15ItemShopAdapter.cs`: analogs for Yellow shop season/item state and save-flag unlock writes.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` and Green/Blue Dan tests: closest analogs for Dan-mode persistence, clear-grade packing, stage rows, invalid Dan guards, and display-Dan updates.
- `Application/Ac15/Ac15NormalPlayService.cs`: existing normal/Dan-mode stage row path used by Phase 14 Yellow playresults.
- `Application/Ac15/YellowAc15NormalPlayAdapter.cs`: current Yellow normal-play persistence adapter that already records stage `WaiwaiResult`/`WaiwaiGauge` values from the shared common DTO.
- `Application/Ac15/YellowAc15UserDataAdapter.cs` and `Ac15UserDataService`: existing path for Yellow userdata readback and shop-locked song/tone masking.
- `Adapters.AdminApi/Controllers/*`, `EraRoute`, and `TaikoWebUI/Utilities/WebUiEra.cs`: current admin/WebUI era routing surfaces to extend for Yellow.
- `Tests/Yellow/YellowHandlerFixture.cs`: focused Yellow fixture with Yellow catalog, EF, and handler wiring to reuse in Phase 15 tests.

### Established Patterns
- Use unsuffixed dispatcher files plus `.Yellow.cs` partials for era behavior.
- Keep controllers thin: deserialize Yellow wire DTOs, map to common/application commands, call Mediator, and map back to Yellow wire DTOs.
- Generated Yellow wire files are read-only evidence unless protocol generation is intentionally rerun.
- Yellow state belongs in Yellow EF entities/DbSets/migrations. Shared identity rows are allowed; gameplay, Dan, shop, medal, favorite/recent, score, and Tokkun state must stay era-owned.
- Shared AC15 services are acceptable behind Yellow adapters, profiles, snapshots, hooks, and persistence policies.
- Source/route/boundary tests should prove no Yellow battle, no Blue/Green shop/Dan state reads/writes, and no Phase 16 Tokkun/Banacoin implementation creep.

### Integration Points
- Add Yellow Dan state through Domain entities, `ITaikoDbContext.Yellow.cs`, `TaikoDbContext.Yellow.cs`, migrations, Yellow handler partials, and Yellow AdminApi mapping.
- Add Yellow shop state through Yellow shop season/item entities, Yellow item-shop adapter, `ItemPurchaseCommand.Yellow`, `ItemShopMappers.Map(ItempurchaseRequest)`, and route/controller replacement.
- Wire Yellow shop unlock readback into `YellowAc15UserDataAdapter` by feeding actual Yellow purchased item rows instead of empty/unimplemented state.
- Extend Yellow playresult handling for Dan and WaiWai logging without removing Phase 14 invalid-stage guards, Tokkun-shaped no-write guard, or raw crown behavior.
- Extend AdminApi/WebUI era support for Yellow profile/user settings, play data/history, favorites, Dan best data, catalog game data, and supported Yellow pages.

</code_context>

<specifics>
## Specific Ideas

The coordinator required all Phase 15 areas to be covered because they correspond exactly to the phase requirements and success criteria:

- Dani/Taikojuku state maps to YDAN-01 and success criterion 1.
- Item-shop info/purchase/unlocks map to YSHOP-01/YSHOP-02 and success criterion 2.
- Don/Katsu medals map to YMED-01 and success criterion 3.
- WaiWai tutorial/logging maps to YWAI-01 and success criterion 4.
- AdminApi/WebUI Yellow readback maps to YUI-01 and success criterion 5.

Current-source detail to preserve: Yellow uses `CommonPlayResultData` plus the Green common partial for AC15 medal/WaiWai/tutorial fields; no Yellow-specific common playresult partial exists at context time. If downstream work adds one, it should be because Yellow needs distinct field ownership, not because the file already exists.

</specifics>

<deferred>
## Deferred Ideas

- Yellow Tokkun playresult classification, no-cross-mode write tests, nullable Tokkun tutorial persistence/readback, append-only raw Tokkun history, and Tokkun AdminApi history/debug readback belong to Phase 16 unless Phase 15 only adds route/readback placeholders.
- Yellow Banacoin-adjacent compatibility and all wallet/payment/transaction non-goals belong to Phase 16.
- Yellow normal and Tokkun cabinet/RPCS3 runtime smoke, full Yellow contract documentation, and final closeout belong to Phase 17.
- Yellow battle remains out of scope unless new concrete Yellow proto/log/client evidence appears.

</deferred>

---

*Phase: 15-Yellow Dani, Shop, Medals, WaiWai, and Admin*
*Context gathered: 2026-06-08*
