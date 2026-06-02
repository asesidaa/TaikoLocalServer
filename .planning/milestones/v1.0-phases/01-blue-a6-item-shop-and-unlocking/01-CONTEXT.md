# Phase 1: Blue A6 Item Shop And Unlocking - Context

**Gathered:** 2026-05-29
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase delivers Blue-owned item shop support for normal Blue cabinet flow: active season advertisement, default shop data derived from the local official cache, item purchase, season-scoped Don medal accounting, immediate item unlocks, and locked-item readback through Blue protocol responses.

This phase does not deliver AdminApi/WebUI parity, cabinet smoke execution, battle mode, Tokkun, Banacoin/payment behavior, or general reward-execution semantics.

</domain>

<decisions>
## Implementation Decisions

### Purchase vs rewardexecution flow
- **D-01:** `itempurchase.php` owns Blue item-shop purchase and unlock behavior.
- **D-02:** `rewardexecution.php` is not related to item purchase. In Phase 1 it should log if called, return success, and make no shop-state or save-state changes.
- **D-03:** Blue purchase preflight mirrors Green: `item_no == 0` with omitted `item_type`, `item_id`, and `item_price` returns active-season medal totals without spending or unlocking.
- **D-04:** Successful Blue purchase unlocks immediately inside `itempurchase.php`: spend active-season Don medals, persist the purchased item, apply Blue save bits, and return success.

### Blue shop data source and committed defaults
- **D-05:** Phase 1 should provide default Blue shop values from the official local cache at `H:\taiko\blue\rewardshopdata.bin`; other shop data remains operator-supplied.
- **D-06:** The derived `blue_item_shop_data.json` should be committed, and parser/tests should prove it matches the local official cache.
- **D-07:** Do not commit or copy `rewardshopdata.bin` into the repo. Keep it as local evidence/source material and use repo-friendly expected values in tests.
- **D-08:** If the official cache cannot be fully parsed or resolved to known catalog IDs, treat that as a parser/implementation issue and stop to discuss. Do not commit partial defaults or placeholder rows.

### Season medal state and first-touch seeding
- **D-09:** Blue shop season state always starts at zero. Do not seed the first active season from `UserSaveDataBlue.TotalGetDonmedal` or `TotalUseDonmedal`.
- **D-10:** When Blue shop is enabled, newly earned `GetDonmedal` from `playresult.php` goes to the active Blue shop season state only.
- **D-11:** `UserSaveDataBlue.TotalGetDonmedal` and `TotalUseDonmedal` should not be used as the Blue shop accounting path.
- **D-12:** When Blue shop is disabled, do not track or update Blue shop Don medal totals.
- **D-13:** Blue BAID and `itempurchase.php` responses report active-season `TotalGetDonmedal` / `TotalUseDonmedal` when shop is enabled, and `0/0` when shop is disabled.

### Unlock domains and title handling
- **D-14:** Phase 1 item-shop purchase supports AC15 shop item types `1..7`: song, tone, kigurumi, body, head, face, and puchi.
- **D-15:** Title shop purchases are out of scope unless the official cache proves a title domain; if that happens, stop to discuss the mapping.
- **D-16:** Blue locking mirrors Green: remove active-season shop songs from `initialdatacheck.php` default song flags, and remove locked active-season songs, tones, and costume slots from `userdata` / BAID until purchased.
- **D-17:** Unsupported/future shop item types must fail catalog load. Do not advertise, purchase, or silently drop unknown rows.
- **D-18:** Use the proven AC15 shop mapping for Blue item types `1..7`, but write Blue save fields with `BlueProtocolBytes` widths:
  - `1 -> ReleaseSongFlg`
  - `2 -> ToneFlg`
  - `3 -> CostumeFlg1`
  - `4 -> CostumeFlg3`
  - `5 -> CostumeFlg2`
  - `6 -> CostumeFlg4`
  - `7 -> CostumeFlg5`

### the agent's Discretion
None. The user made explicit decisions for all discussed gray areas.

</decisions>

<canonical_refs>
## Canonical References

Downstream agents MUST read these before planning or implementing.

### Project and Phase Scope
- `.planning/PROJECT.md` - Full Blue scope, A0-A5 treated as complete, normal support before battle runtime, and Blue-owned state constraints.
- `.planning/REQUIREMENTS.md` - SHOP-01 through SHOP-08 are the Phase 1 requirement set.
- `.planning/ROADMAP.md` - Phase 1 goal, success criteria, and planned work breakdown.
- `.planning/research/ARCHITECTURE.md` - A6 architecture constraints and Blue-owned implementation shape.
- `.planning/research/PITFALLS.md` - A6 pitfalls around Green leakage, item type mapping, stubs, and locking.
- `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md` - Original Blue roadmap and Stage A6 references.
- `docs/superpowers/specs/2026-05-26-green-item-shop-support-design.md` - Nearest local item-shop design reference; apply only where not contradicted by this context.

### Blue Protocol and Runtime Touch Points
- `proto/blue/taiko.proto` - Blue `Getitemshopinfo*`, `Itempurchase*`, `Rewardexecution*`, `BAID*`, `UserData*`, and `PlayResult*` wire fields.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - Generated Blue wire types and optional-field presence helpers.
- `Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs` - Current Blue shop-info stub to replace with mediator-backed behavior.
- `Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs` - Current Blue purchase stub to replace with mediator-backed behavior.
- `Adapters.GameProtocol.Blue/Controllers/RewardExecutionController.cs` - Keep log-and-success behavior for Phase 1; do not add shop mutation.
- `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs` - Existing Blue initial-data mapper for shop advertisement fields.

### Catalog and Data
- `Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs` - Existing Blue loader shell for `blue_item_shop_data.json`.
- `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs` - Shared AC15 JSON loader and validation rules.
- `Application/Catalog/Blue/BlueItemShopCatalog.cs` - Blue item-shop catalog contract.
- `Application/Catalog/Blue/BlueItemShopSeason.cs` - Blue item-shop season contract.
- `Application/Catalog/Blue/BlueItemShopEntry.cs` - Blue item-shop row contract.
- `H:\taiko\blue\rewardshopdata.bin` - Local official cache source for default Blue shop data. Verified present on 2026-05-29, 179 bytes. Do not commit this binary.

### Blue State and Handlers
- `Application/Handlers/GetInitialDataQuery.Blue.cs` - Existing Blue initial-data behavior and shop advertisement hook.
- `Application/Handlers/BaidQuery.Blue.cs` - Existing Blue BAID readback where shop totals and locked costume flags must be handled.
- `Application/Handlers/UserDataQuery.Blue.cs` - Existing Blue userdata readback where shop song/tone locks must be handled.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` - Existing Blue playresult path where enabled-shop Don medal accounting must move to active season state.
- `Application/Common/BlueProtocolBytes.cs` - Blue bitset widths and helpers for song/tone/title/costume data.
- `Application/Common/UserSaveDataBlueExtensions.cs` - Default Blue save data creation.
- `Domain/Entities/UserSaveDataBlue.cs` - Blue save fields and existing medal columns that must not become shop accounting.
- `Application/Abstractions/ITaikoDbContext.Blue.cs` - Blue persistence port partial to extend with shop state DbSets.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs` - Blue EF mapping partial to extend with shop state entities.

### Green Reference Code
- `Application/Handlers/ItemPurchaseCommand.Green.cs` - Current direct purchase/unlock runtime model; use as shape, not as Green state.
- `Application/Handlers/GetItemShopInfoQuery.Green.cs` - Existing shop-info query behavior to make era-aware or mirror for Blue.
- `Application/Common/GreenShopStateExtensions.cs` - Green first-touch seeding reference; do not copy the seeding rule for Blue.
- `Application/Handlers/UserDataQuery.Green.cs` - Green lock readback reference.
- `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs` - Mapper reference for optional purchase fields and shop rows.
- `Tests/Green/GreenItemShopPurchaseTests.cs` - Purchase/preflight/duplicate behavior reference.
- `Tests/Green/GreenItemShopLockingTests.cs` - Locking behavior reference.
- `Tests/Green/GreenItemShopStateTests.cs` - Medal-state reference; adapt without Green first-season seeding.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `BlueItemShopLoader` already maps the shared AC15 item-shop catalog shape into Blue catalog types.
- `Ac15ItemShopLoader` already validates enablement, active season selection, date shape, row count, duplicate item identities, and `item_type` range.
- `BlueProtocolBytes` already provides Blue bitset widths and normalization helpers for song, tone, title, costume, Dan, and crown data.
- Green item-shop tests provide a strong behavioral template for purchase, preflight, duplicate handling, locking, and medal-state coverage.

### Established Patterns
- Era behavior lives in partial application handlers such as `*.Blue.cs`; do not put Blue-only state changes in shared handler bodies.
- Protocol adapters map wire DTOs to common application DTOs before invoking Mediator handlers.
- Era-owned persistence is exposed through `ITaikoDbContext.<Era>.cs` and mapped in `TaikoDbContext.<Era>.cs`.
- Enabled-era runtime data is loaded through `IGameDataCatalog` and the era catalog implementations during Host startup.

### Integration Points
- Add Blue shop state entities, DbSets, EF mappings, and migration under Blue-owned names.
- Add or make era-aware item-shop application handlers for shop info and purchase.
- Replace Blue `getitemshopinfo.php` and `itempurchase.php` stubs with mediator-backed controller paths.
- Keep Blue `rewardexecution.php` as logging plus success response for this phase.
- Update Blue initialdata, BAID, userdata, and playresult paths for enabled-shop advertisement, locking, totals, and active-season medal accrual.
- Add parser/tooling or loader support to derive committed default JSON from `H:\taiko\blue\rewardshopdata.bin`, without committing the binary.

</code_context>

<specifics>
## Specific Ideas

- The local official cache at `H:\taiko\blue\rewardshopdata.bin` is the source of truth for committed default Blue shop values. The binary was present during discussion and measured at 179 bytes.
- If parsing the cache reveals an unexpected title domain or unresolved official rows, stop and discuss before continuing.
- Blue starts clean for shop state. There is no legacy Blue shop state migration need, so Green's first-season global medal seeding rule does not apply.

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope.

</deferred>

---

*Phase: 1-Blue A6 Item Shop And Unlocking*
*Context gathered: 2026-05-29*
