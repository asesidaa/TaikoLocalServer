# Phase 01: Blue A6 Item Shop And Unlocking - Research

**Researched:** 2026-05-29
**Domain:** ASP.NET Core game protocol adapter, EF Core SQLite persistence, AC15 item shop catalog data, Blue protocol unlock state
**Confidence:** HIGH for architecture and code touch points; MEDIUM for official-cache parser details until implementation validates the binary format

<user_constraints>
## User Constraints (from CONTEXT.md)

Source for all entries in this section: [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

### Locked Decisions

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

### Deferred Ideas (OUT OF SCOPE)

## Deferred Ideas

None - discussion stayed within phase scope.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| SHOP-01 | Blue `initialdatacheck.php` advertises item-shop version data only when Blue shop is enabled and the active Blue shop season has rows. | Existing Blue initial data already checks `blue.ItemShopCatalog.IsEnabled`, active season, rows, and removes active shop songs from default flags; plan should add regression coverage and preserve this behavior. [VERIFIED: Application/Handlers/GetInitialDataQuery.Blue.cs; .planning/REQUIREMENTS.md] |
| SHOP-02 | Blue `getitemshopinfo.php` returns the configured active Blue shop season, including season fields and ordered item rows with protocol `item_no` values. | Current Blue controller is a success stub; Green has the handler/mapper shape that returns season envelope and ordered `AryItemshopDatas`. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs; Application/Handlers/GetItemShopInfoQuery.Green.cs; Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs] |
| SHOP-03 | Blue item-shop catalog loading uses `blue_item_shop_data.json` and fails fast when shop is enabled but the file, active season, or required season fields are invalid. | `BlueItemShopLoader` already delegates to `Ac15ItemShopLoader`, which validates missing enabled file, active season, date shape, row count, duplicate identities, item type range, nonzero item id, and nonzero price. [VERIFIED: Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs; Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs] |
| SHOP-04 | Blue item purchases validate `item_no`, `item_type`, `item_id`, and `item_price` against the active Blue catalog before spending medals or changing save state. | Green validates the full tuple and preflight optional-field absence; Blue needs its own command handler and Blue mapper. [VERIFIED: Application/Handlers/ItemPurchaseCommand.Green.cs; Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs; proto/blue/taiko.proto] |
| SHOP-05 | Blue shop medal totals are stored by BAID and Blue shop season without using Green shop tables or global Green medal state. | Blue currently has no `BlueShop*` DbSets; Green table shape gives the model, but D-09 through D-13 require zero-start Blue season rows and no global Blue medal accounting when shop is enabled. [VERIFIED: Application/Abstractions/ITaikoDbContext.Blue.cs; Infrastructure/Persistence/TaikoDbContext.Blue.cs; Domain/Entities/GreenShopSeasonState.cs; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| SHOP-06 | Blue purchase and reward execution unlock configured song, tone, costume, title, or related Blue save-state fields without writing Green or Nijiiro state. | For this phase, D-01, D-02, D-04, D-15, and D-18 narrow this requirement: `itempurchase.php` unlocks supported item types 1..7 immediately, title handling stops for discussion if discovered, and `rewardexecution.php` logs success without mutation. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md; Application/Common/BlueProtocolBytes.cs] |
| SHOP-07 | Blue userdata readback hides configured locked shop items until the player purchases or receives the matching reward. | Green locking clears active-season locked song, tone, and costume bits; Blue needs equivalent filtering in `UserDataQuery.Blue.cs` and `BaidQuery.Blue.cs` using `BlueProtocolBytes`. [VERIFIED: Application/Handlers/UserDataQuery.Green.cs; Application/Handlers/BaidQuery.Green.cs; Application/Handlers/UserDataQuery.Blue.cs; Application/Handlers/BaidQuery.Blue.cs] |
| SHOP-08 | Blue item shop implementation has source guards and regression tests proving it does not reference Green shop state, Green protocol constants, or Green wire models. | Existing Blue A3-A5 source guards search touched Blue application and adapter files for Green dependencies; A6 should add a similar guard covering shop files. [VERIFIED: Tests/Blue/BlueA3SourceGuardTests.cs; Tests/Blue/BlueA4SourceGuardTests.cs; Tests/Blue/BlueA5SourceGuardTests.cs] |
</phase_requirements>

## Summary

Phase 01 should implement Blue item shop by mirroring Green's mature AC15 shop structure at the pattern level only: Blue-owned entities, DbSets, EF mappings, helpers, handlers, mapper, controllers, tests, and source guards. [VERIFIED: .planning/research/ARCHITECTURE.md; Application/Handlers/ItemPurchaseCommand.Green.cs; Infrastructure/Persistence/TaikoDbContext.Green.cs] The planner should not reuse `GreenShopSeasonState`, `GreenShopItemState`, `GreenProtocolBytes`, Green wire types, or Green first-touch medal seeding. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md; .planning/research/PITFALLS.md]

The existing codebase already has most catalog infrastructure: `BlueItemShopLoader` consumes `blue_item_shop_data.json`, maps through the shared AC15 loader, and exposes `BlueItemShopCatalog` through `IBlueCatalog`. [VERIFIED: Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs; Application/Catalog/Blue/BlueItemShopCatalog.cs; Application/Abstractions/IBlueCatalog.cs] The missing runtime pieces are Blue shop state persistence, Blue shop state helpers, Blue item purchase handling, Blue `getitemshopinfo.php` mapper/controller wiring, active-season medal accrual in Blue playresult, BAID/userdata locking, parser-backed default data, and A6 source guards. [VERIFIED: Application/Handlers/BaidQuery.Blue.cs; Application/Handlers/UserDataQuery.Blue.cs; Application/Handlers/UpdatePlayResultCommand.Blue.cs; Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs; Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs]

The official local cache `H:\taiko\blue\rewardshopdata.bin` exists and is 179 bytes, with a Boost serialization header and an apparent four-row reward-shop payload. [VERIFIED: Get-Item H:\taiko\blue\rewardshopdata.bin; Format-Hex H:\taiko\blue\rewardshopdata.bin] The row interpretation is not yet a locked truth: it appears to contain four item rows that may decode as `item_type=3`, item ids `12, 7, 9, 10`, and prices `1300, 1500, 1500, 1500`, but the planner must require parser tests that fully decode season metadata and catalog ID resolution before committing derived JSON. [ASSUMED]

**Primary recommendation:** Plan four implementation waves: parser/default JSON gate, Blue persistence/state helpers, Blue protocol purchase/shop-info wiring, then locking/source-guard/regression verification. [VERIFIED: .planning/ROADMAP.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Official Blue shop cache derivation | Tooling / Tests | Filesystem data | The binary is local evidence and must produce committed JSON with tests, while runtime reads `blue_item_shop_data.json`, not the binary. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md; Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs] |
| Blue shop catalog load and validation | Infrastructure | Application catalog contracts | `BlueItemShopLoader` and `Ac15ItemShopLoader` own filesystem JSON parsing, validation, and mapping into `BlueItemShopCatalog`. [VERIFIED: Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs; Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs] |
| Blue shop state persistence | Database / Storage | Infrastructure | Season totals and purchased items need Blue-owned EF entities, DbSets, mappings, and migration. [VERIFIED: Application/Abstractions/ITaikoDbContext.Blue.cs; Infrastructure/Persistence/TaikoDbContext.Blue.cs; .planning/research/ARCHITECTURE.md] |
| Shop purchase validation and medal spend | Application / Backend | Database / Storage | The application handler must validate the request tuple against the active catalog before changing Blue state. [VERIFIED: Application/Handlers/ItemPurchaseCommand.Green.cs; proto/blue/taiko.proto] |
| Blue unlock bit application | Application / Backend | Protocol adapter | Unlocks are save-state mutations using `BlueProtocolBytes` widths, then mapped to Blue wire responses. [VERIFIED: Application/Common/BlueProtocolBytes.cs; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| Blue `getitemshopinfo.php` and `itempurchase.php` endpoints | Game Protocol Adapter | Application / Backend | Controllers deserialize Blue wire types, map optional fields, call Mediator, and serialize Blue wire responses. [VERIFIED: Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs; Adapters.GameProtocol.Green/Controllers/ItemPurchaseController.cs; Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs; Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs] |
| Blue BAID/userdata locking readback | Application / Backend | Game Protocol Adapter | BAID and userdata handlers decide which active-season bits are visible; mappers only emit fixed-width Blue byte arrays. [VERIFIED: Application/Handlers/BaidQuery.Green.cs; Application/Handlers/UserDataQuery.Green.cs; Adapters.GameProtocol.Blue/Mappers/BaidResponseMapper.cs; Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs] |
| AdminApi/WebUI shop parity | Deferred | - | Phase 02 owns broader AdminApi/WebUI parity, not this phase. [VERIFIED: .planning/ROADMAP.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |

## Project Constraints (from AGENTS.md)

- Blue must remain a first-class era with Blue-owned persistence, handlers, mappers, catalogs, tests, and routes rather than a Green flag. [VERIFIED: AGENTS.md supplied in thread; H:/TaikoLocalServer/AGENTS.md]
- Keep Blue, Green, and Nijiiro persistent state separate unless the data is truly shared identity state. [VERIFIED: AGENTS.md supplied in thread; H:/TaikoLocalServer/AGENTS.md]
- Finish normal Track A A6-A8 before battle runtime implementation; battle evidence/spec work may prepare Track B, but runtime battle behavior should not jump ahead. [VERIFIED: AGENTS.md supplied in thread; H:/TaikoLocalServer/AGENTS.md]
- Preserve known Blue direct-protobuf and startup/verup assumptions unless newer client evidence contradicts them. [VERIFIED: AGENTS.md supplied in thread; H:/TaikoLocalServer/AGENTS.md]
- Blue runtime data under `Host/wwwroot/data/blue/data` is local/operator-supplied and may be gitignored. [VERIFIED: AGENTS.md supplied in thread; H:/TaikoLocalServer/AGENTS.md]
- If `Host/bin/Debug/net10.0` is locked by a running server, verify Host builds with a temp output path. [VERIFIED: AGENTS.md supplied in thread; H:/TaikoLocalServer/AGENTS.md]
- Use C# file-scoped namespaces, central package versions, era suffix partial files, Mapperly mappers, and one primary type per `.cs` file. [VERIFIED: AGENTS.md supplied in thread; H:/TaikoLocalServer/AGENTS.md]
- Do not map generated protobuf DTOs directly into persistence entities; map through application DTOs/commands. [VERIFIED: AGENTS.md supplied in thread; H:/TaikoLocalServer/AGENTS.md]
- Project-local skill discovery found `.codex/skills` with GSD workflow skills, but no phase-specific `rules/*.md` directories for this project. [VERIFIED: Get-ChildItem .codex/skills; Get-ChildItem .codex/skills/gsd-plan-phase]

## Standard Stack

### Core

| Library / Component | Version | Purpose | Why Standard |
|---------------------|---------|---------|--------------|
| .NET SDK / C# | SDK 10.0.201 available; `global.json` pins 10.0.100 with `latestFeature` roll-forward | Build and test all application, adapter, infrastructure, and test projects | Existing solution is `net10.0` and uses central C# settings. [VERIFIED: dotnet --version; global.json; Directory.Build.props] |
| ASP.NET Core MVC | 10.0.7 package family in central props | Blue protocol controllers under `/v10r03/chassis/*` | Existing adapters use controllers and protobuf response negotiation. [VERIFIED: Directory.Packages.props; Adapters.GameProtocol.Blue/Controllers] |
| EF Core SQLite | 10.0.7 | Blue shop tables, migration, and DbSet-backed queries | Existing persistence layer and Green shop state use EF Core SQLite. [VERIFIED: Directory.Packages.props; Infrastructure/Persistence/TaikoDbContext.Green.cs] |
| Mediator.SourceGenerator / Mediator.Abstractions | 3.0.2 | Application command/query handlers | Existing game protocol flows use Mediator request handlers with era-specific partials. [VERIFIED: Directory.Packages.props; Application/Handlers] |
| protobuf-net / protobuf-net.AspNetCore | 3.2.56 / 3.2.52 | Serialize Blue wire request/response types | Existing Blue/Green game adapters use generated protobuf-net wire models. [VERIFIED: Directory.Packages.props; Adapters.GameProtocol.Blue/Wire/Game.cs] |
| xUnit / Microsoft.NET.Test.Sdk | 2.9.3 / 17.14.1 | Unit, integration, source guard, and mapper tests | Existing `Tests/Tests.csproj` uses xUnit and references all relevant projects. [VERIFIED: Directory.Packages.props; Tests/Tests.csproj] |

### Supporting

| Library / Component | Version | Purpose | When to Use |
|---------------------|---------|---------|-------------|
| Shared AC15 item-shop loader | In-repo | JSON validation and 1-based item number inference for Green/Blue | Use for runtime `blue_item_shop_data.json`; extend only if official Blue defaults need stricter validation. [VERIFIED: Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs] |
| `BlueProtocolBytes` | In-repo | Fixed byte widths and bitset normalization for Blue save/readback | Use for all Blue song/tone/title/costume bits; do not use `GreenProtocolBytes`. [VERIFIED: Application/Common/BlueProtocolBytes.cs; .planning/research/PITFALLS.md] |
| `dotnet ef` | 10.0.7 available | Generate Blue shop EF migration | Use for adding `BlueShopSeasonState` and `BlueShopItemState` schema. [VERIFIED: dotnet ef --version] |
| Python | Miniconda and Python 3.13 launchers available | Optional read-only parser/probe for `rewardshopdata.bin` during Wave 0 | Use only as local tooling/tests if a C# parser is not yet warranted. [VERIFIED: where.exe python] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Blue-owned EF tables | Green shop tables | Not allowed because state separation is a locked constraint and source guards must reject Green shop state in Blue implementation. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md; .planning/research/PITFALLS.md] |
| Runtime binary parsing of `rewardshopdata.bin` | Committed `blue_item_shop_data.json` derived from the binary | Runtime binary parsing would depend on local evidence files and violates the decision to commit derived JSON while not copying the binary. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| Rewardexecution mutation | Log-and-success no-op | D-02 explicitly makes rewardexecution non-mutating in Phase 1 despite the broader SHOP-06 wording. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md; .planning/REQUIREMENTS.md] |

**Installation:** No new external package installs are recommended for this phase. [VERIFIED: Directory.Packages.props; codebase grep for existing shop dependencies]

**Version verification:** Existing package versions were verified from `Directory.Packages.props`, and CLI availability was verified with `dotnet --version` and `dotnet ef --version`; no registry freshness check or slopcheck gate is required because no new packages are recommended. [VERIFIED: Directory.Packages.props; dotnet --version; dotnet ef --version]

## Package Legitimacy Audit

This phase should not install external packages. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md; Directory.Packages.props]

| Package | Registry | Age | Downloads | Source Repo | slopcheck | Disposition |
|---------|----------|-----|-----------|-------------|-----------|-------------|
| none | - | - | - | - | not run | No package install planned. [VERIFIED: codebase grep; Directory.Packages.props] |

**Packages removed due to slopcheck [SLOP] verdict:** none. [VERIFIED: no external packages planned]
**Packages flagged as suspicious [SUS]:** none. [VERIFIED: no external packages planned]

## Architecture Patterns

### System Architecture Diagram

```text
Local evidence cache
H:\taiko\blue\rewardshopdata.bin
  -> Wave 0 parser/test gate
  -> committed Host/wwwroot/data/blue/blue_item_shop_data.json
  -> BlueItemShopLoader -> Ac15ItemShopLoader validation
  -> IBlueCatalog.ItemShopCatalog

Cabinet request /v10r03/chassis/getitemshopinfo.php
  -> Blue GetItemShopInfoController
  -> GetItemShopInfoQuery(GameEra.Blue or Blue-specific handler)
  -> active Blue season response
  -> Blue ItemShopMappers -> GetitemshopinfoResponse

Cabinet request /v10r03/chassis/itempurchase.php
  -> Blue ItemPurchaseController
  -> Blue ItemShopMappers preserve optional fields
  -> ItemPurchaseCommand with Blue dispatch
  -> validate active catalog row and season balance
  -> BlueShopSeasonState + BlueShopItemState + UserSaveDataBlue bitsets
  -> Blue ItempurchaseResponse totals

Cabinet request /v10r03/chassis/playresult.php
  -> UpdatePlayResultCommand.Blue
  -> if shop enabled: active Blue season TotalGetDonmedal
  -> if shop disabled: no Blue shop medal tracking

Cabinet BAID / userdata / initialdatacheck
  -> Blue handlers load active season and unlocked item rows
  -> clear locked song/tone/costume bits
  -> BlueProtocolBytes fixed-width response arrays
```

Diagram source: [VERIFIED: Adapters.GameProtocol.Blue/Controllers; Application/Handlers/*.Blue.cs; Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

### Recommended Project Structure

```text
Domain/Entities/
  BlueShopSeasonState.cs          # BAID + season medal totals
  BlueShopItemState.cs            # BAID + season + item identity unlock row

Domain/Enums/
  BlueShopItemStatus.cs           # Unlocked status, Blue-owned

Application/Common/
  BlueShopStateExtensions.cs      # zero-start season state helper
  BlueShopUnlocks.cs              # BlueProtocolBytes bit set/clear helpers

Application/Handlers/
  GetItemShopInfoQuery.cs         # add era dispatch if shared
  GetItemShopInfoQuery.Blue.cs    # Blue active-season response
  ItemPurchaseCommand.cs          # add era dispatch if shared
  ItemPurchaseCommand.Blue.cs     # Blue preflight, validation, spend, unlock

Adapters.GameProtocol.Blue/Mappers/
  ItemShopMappers.cs              # Blue wire <-> common command/response

Tests/Blue/
  BlueItemShopLoaderTests.cs
  BlueRewardShopDataParserTests.cs
  BlueItemShopStateTests.cs
  BlueItemShopProtocolTests.cs
  BlueItemShopPurchaseTests.cs
  BlueItemShopLockingTests.cs
  BlueA6SourceGuardTests.cs

Host/wwwroot/data/blue/
  blue_item_shop_data.json        # committed derived defaults
```

Structure source: [VERIFIED: .planning/ROADMAP.md; .planning/research/ARCHITECTURE.md; existing Green shop file layout]

### Pattern 1: Era-Owned Persistence Partial

**What:** Add Blue shop DbSets to `ITaikoDbContext.Blue.cs` and `TaikoDbContext.Blue.cs`, with table names such as `BlueShopSeasonStates` and `BlueShopItemStates`. [VERIFIED: Application/Abstractions/ITaikoDbContext.Blue.cs; Infrastructure/Persistence/TaikoDbContext.Blue.cs]

**When to use:** Use this for every durable shop state row because Blue shop balances and item unlocks are era-owned state. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Example:**

```csharp
// Source pattern: Infrastructure/Persistence/TaikoDbContext.Green.cs
modelBuilder.Entity<BlueShopSeasonState>(entity =>
{
    entity.ToTable("BlueShopSeasonStates");
    entity.HasKey(e => new { e.Baid, e.SeasonId });
    entity.Property(e => e.CreatedAt).HasColumnType("datetime");
    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
});
```

### Pattern 2: Zero-Start Blue Season Helper

**What:** Create `GetOrCreateBlueShopSeasonStateAsync` that always initializes `TotalGetDonmedal` and `TotalUseDonmedal` to zero for new season rows. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**When to use:** BAID totals, preflight, purchase, and enabled-shop playresult accrual all need the same active-season state row. [VERIFIED: Application/Handlers/BaidQuery.Green.cs; Application/Handlers/ItemPurchaseCommand.Green.cs; Application/Handlers/UpdatePlayResultCommand.Green.cs]

**Example:**

```csharp
// Source pattern: Application/Common/GreenShopStateExtensions.cs
// Blue difference: no first-season seeding from UserSaveDataBlue totals.
var state = new BlueShopSeasonState
{
    Baid = saveData.Baid,
    SeasonId = seasonId,
    TotalGetDonmedal = 0,
    TotalUseDonmedal = 0,
    CreatedAt = now,
    UpdatedAt = now
};
```

### Pattern 3: Optional Field Preservation in Purchase Mapper

**What:** Blue `ItemShopMappers.Map(ItempurchaseRequest)` must use generated `ShouldSerializeItemType`, `ShouldSerializeItemId`, and `ShouldSerializeItemPrice` helpers so preflight can distinguish omitted fields from explicit zero. [VERIFIED: Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs; Adapters.GameProtocol.Blue/Wire/Game.cs]

**When to use:** Required for D-03 preflight semantics. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Example:**

```csharp
// Source pattern: Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs
return new ItemPurchaseCommand(
    GameEra.Blue,
    request.Baid,
    request.ItemNo,
    request.ShouldSerializeItemType() ? request.ItemType : null,
    request.ShouldSerializeItemId() ? request.ItemId : null,
    request.ShouldSerializeItemPrice() ? request.ItemPrice : null);
```

### Pattern 4: Lock Readback by Clearing Active-Season Bits

**What:** Compute unlocked active-season item identities from Blue shop item state, then clear not-yet-unlocked active-season bits from userdata and BAID responses. [VERIFIED: Application/Handlers/UserDataQuery.Green.cs; Application/Handlers/BaidQuery.Green.cs]

**When to use:** Required for SHOP-07 and D-16. [VERIFIED: .planning/REQUIREMENTS.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Example:**

```csharp
// Source pattern: Application/Handlers/UserDataQuery.Green.cs
IEnumerable<uint> LockedIds(uint itemType) => activeShopSeason?.Items
    .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType, item.ItemId)))
    .Select(item => item.ItemId) ?? [];
```

### Anti-Patterns to Avoid

- **Green state reuse:** Reusing `GreenShopSeasonState`, `GreenShopItemState`, or `GreenShopItemStatus` breaks era state separation. [VERIFIED: .planning/research/PITFALLS.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]
- **Global Blue medal accounting while shop is enabled:** Updating `UserSaveDataBlue.TotalGetDonmedal` for shop earn/spend contradicts D-10 and D-11. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md; Application/Handlers/UpdatePlayResultCommand.Blue.cs]
- **Rewardexecution mutation in Phase 1:** Implementing rewardexecution unlock behavior contradicts D-02. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]
- **Title support by guess:** Title purchases are out of scope unless the cache proves a title domain. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]
- **Silent partial official defaults:** D-08 requires stopping to discuss if cache parsing or ID resolution is incomplete. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Shop JSON validation | A second ad hoc JSON parser | `Ac15ItemShopLoader` | It already enforces active-season, date, row count, duplicate, type, id, and price rules. [VERIFIED: Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs] |
| Blue bitset sizing | Manual byte arrays with guessed lengths | `BlueProtocolBytes` and `BitsetCodec` | Blue song/tone/title/costume widths differ from Green and are already centralized. [VERIFIED: Application/Common/BlueProtocolBytes.cs] |
| Optional protobuf presence | Treat missing optional fields as zero | Generated `ShouldSerialize*` helpers | Preflight depends on omitted optional purchase details. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| Database schema mutation | Raw SQL patches | EF Core migration through `dotnet ef` | Existing persistence uses EF Core migrations and model snapshots. [VERIFIED: Infrastructure/Persistence/Migrations; dotnet ef --version] |
| Cross-era guard review | Manual review only | A6 source guard tests | Existing Blue phases use source guards to catch Green dependency leakage. [VERIFIED: Tests/Blue/BlueA3SourceGuardTests.cs; Tests/Blue/BlueA4SourceGuardTests.cs; Tests/Blue/BlueA5SourceGuardTests.cs] |
| Runtime official-data scraping | Web/page/OCR lookup during startup | Committed `blue_item_shop_data.json` derived offline from local cache | Runtime data must be repo-friendly JSON; binary cache stays evidence-only. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |

**Key insight:** The hard parts are not algorithms; they are boundary discipline and protocol evidence. Reuse the proven loader/handler/test patterns, but keep every durable and byte-level decision Blue-owned. [VERIFIED: .planning/research/ARCHITECTURE.md; .planning/research/PITFALLS.md]

## Runtime State Inventory

| Category | Items Found | Action Required |
|----------|-------------|-----------------|
| Stored data | Existing SQLite schema has `UserSaveData_Blue`, Blue score/play/Dani tables, and Green shop tables, but no Blue shop season/item tables. [VERIFIED: Infrastructure/Persistence/TaikoDbContext.Blue.cs; Infrastructure/Persistence/TaikoDbContext.Green.cs] | Add Blue EF entities, DbSets, mapping, and migration; do not backfill from `UserSaveDataBlue.TotalGetDonmedal` or `TotalUseDonmedal`. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| Live service config | `ServerSettings:Eras:Blue` declares `EnableShop` and `ActiveShopSeasonId`; Blue shop is disabled by shipped config. [VERIFIED: Tests/Blue/BlueServerSettingsValidationTests.cs; Host/Configurations/ServerSettings.json via test] | Planner should include tests for enabled/disabled behavior and note operator must enable Blue shop for runtime. [VERIFIED: Tests/Blue/BlueServerSettingsValidationTests.cs] |
| OS-registered state | None found for A6; the phase uses repo files, SQLite, and ASP.NET Core runtime only. [VERIFIED: .planning/PROJECT.md; AGENTS.md supplied in thread] | No OS re-registration task. [VERIFIED: no OS service dependency in project context] |
| Secrets/env vars | No `.env` files were detected in the supplied AGENTS stack scan, and A6 adds no secrets. [VERIFIED: AGENTS.md supplied in thread] | No secret/env rename or creation task. [VERIFIED: phase scope] |
| Build artifacts | `Host/bin/Debug/net10.0` may be locked by a running server; `rewardshopdata.bin` is outside the repo and must remain uncommitted. [VERIFIED: AGENTS.md supplied in thread; Get-Item H:\taiko\blue\rewardshopdata.bin] | Use temp Host output for build verification if needed; add/verify gitignore coverage so the binary is not copied. [VERIFIED: AGENTS.md supplied in thread; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |

**Nothing found in category:** OS-registered state and secrets/env vars have no A6-specific runtime state to migrate. [VERIFIED: .planning/PROJECT.md; AGENTS.md supplied in thread]

## Common Pitfalls

### Pitfall 1: Green Shop Leakage

**What goes wrong:** Blue code compiles by using Green shop entities, status enum, protocol bytes, or mappers. [VERIFIED: .planning/research/PITFALLS.md]

**Why it happens:** Green has complete shop code and Blue currently has stubs, making copy/paste tempting. [VERIFIED: Application/Handlers/ItemPurchaseCommand.Green.cs; Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs]

**How to avoid:** Create Blue-named entities, helpers, mappers, and source guards before broad handler edits. [VERIFIED: Tests/Blue/BlueA5SourceGuardTests.cs]

**Warning signs:** `GreenShop`, `GreenProtocolBytes`, `Adapters.GameProtocol.Green`, or `UserSaveDataGreen` appears in A6 Blue files. [VERIFIED: Tests/Blue/BlueA4SourceGuardTests.cs]

### Pitfall 2: First-Season Medal Seeding

**What goes wrong:** Blue copies Green's first-touch seed from global save medal totals. [VERIFIED: Application/Common/GreenShopStateExtensions.cs]

**Why it happens:** Green's migration compatibility rule is embedded in the helper. [VERIFIED: Tests/Green/GreenItemShopStateTests.cs]

**How to avoid:** Blue helper always starts new season rows at zero and playresult only accrues shop medals when the Blue shop is enabled. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Warning signs:** Blue helper reads `saveData.TotalGetDonmedal` or `saveData.TotalUseDonmedal` when constructing a new shop season row. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

### Pitfall 3: Treating Rewardexecution as Purchase Completion

**What goes wrong:** Planner implements pending rewards and rewardexecution mutation from the older Green design. [VERIFIED: docs/superpowers/specs/2026-05-26-green-item-shop-support-design.md]

**Why it happens:** SHOP-06 uses broad wording, but Phase 1 context overrides it for Blue. [VERIFIED: .planning/REQUIREMENTS.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**How to avoid:** Implement immediate purchase unlock in `itempurchase.php`; keep Blue `rewardexecution.php` log-and-success with no state changes. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Warning signs:** New Blue reward command changes `BlueShopItemState`, `UserSaveDataBlue`, or medal totals. [VERIFIED: D-02 in CONTEXT.md]

### Pitfall 4: Optional Purchase Fields Collapsed to Zero

**What goes wrong:** Preflight request is rejected because omitted optional fields become zero-valued details. [VERIFIED: Tests/Green/GreenItemShopProtocolTests.cs; Adapters.GameProtocol.Blue/Wire/Game.cs]

**Why it happens:** Generated proto properties expose default values unless `ShouldSerialize*` is checked. [VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs]

**How to avoid:** Mapper returns nullable `ItemType`, `ItemId`, and `ItemPrice` based on `ShouldSerialize*`. [VERIFIED: Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs]

**Warning signs:** Mapper directly passes `request.ItemType`, `request.ItemId`, or `request.ItemPrice` without presence checks. [VERIFIED: Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs]

### Pitfall 5: Head/Body Costume Mapping Reversal

**What goes wrong:** `item_type=4` and `item_type=5` write the wrong costume save fields. [VERIFIED: docs/superpowers/specs/2026-05-26-green-item-shop-support-design.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Why it happens:** Display naming order and save-field order are not intuitive. [VERIFIED: .planning/research/PITFALLS.md]

**How to avoid:** Encode D-18 exactly and add tests for `4 -> CostumeFlg3` and `5 -> CostumeFlg2`. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Warning signs:** Switch statement maps item type 4 to `CostumeFlg2` or type 5 to `CostumeFlg3`. [VERIFIED: D-18 in CONTEXT.md]

### Pitfall 6: Partial Official Defaults

**What goes wrong:** A committed `blue_item_shop_data.json` contains guessed season metadata, unresolved item ids, or placeholder rows. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Why it happens:** The cache is small but not self-documenting; hexdump inspection alone is not full parsing. [VERIFIED: Format-Hex H:\taiko\blue\rewardshopdata.bin]

**How to avoid:** Start with parser tests that prove the derived JSON matches the binary and that item IDs resolve to known catalog domains; stop for discussion if not. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

**Warning signs:** Test asserts only file length or row count, not season fields, item type, ids, prices, and catalog resolution. [VERIFIED: D-06 and D-08 in CONTEXT.md]

## Code Examples

Verified and recommended patterns:

### Blue Purchase Dispatch Shape

```csharp
// Source: Application/Handlers/ItemPurchaseCommand.cs and existing era dispatch patterns.
public readonly record struct ItemPurchaseCommand(
    GameEra Era,
    uint Baid,
    uint ItemNo,
    uint? ItemType,
    uint? ItemId,
    uint? ItemPrice
) : IRequest<CommonItemPurchaseResponse>;
```

This adds explicit era dispatch so Green and Blue can remain separate partial handlers. [VERIFIED: Application/Handlers/UpdatePlayResultCommand.cs; Application/Handlers/ItemPurchaseCommand.cs]

### Blue Unlock Mapping

```csharp
// Source: D-18 in CONTEXT.md and Application/Common/BlueProtocolBytes.cs.
private static void ApplyBlueShopUnlock(UserSaveDataBlue saveData, BlueShopItemState item)
{
    switch (item.ItemType)
    {
        case 1:
            saveData.ReleaseSongFlg = BlueShopUnlocks.SetBits(saveData.ReleaseSongFlg, [item.ItemId], BlueProtocolBytes.SongFlagBytes);
            return;
        case 2:
            saveData.ToneFlg = BlueShopUnlocks.SetBits(saveData.ToneFlg, [item.ItemId], BlueProtocolBytes.ToneFlagBytes);
            return;
        case 3:
            saveData.CostumeFlg1 = BlueShopUnlocks.SetBits(saveData.CostumeFlg1, [item.ItemId], BlueProtocolBytes.CostumeFlagBytes);
            return;
        case 4:
            saveData.CostumeFlg3 = BlueShopUnlocks.SetBits(saveData.CostumeFlg3, [item.ItemId], BlueProtocolBytes.CostumeFlagBytes);
            return;
        case 5:
            saveData.CostumeFlg2 = BlueShopUnlocks.SetBits(saveData.CostumeFlg2, [item.ItemId], BlueProtocolBytes.CostumeFlagBytes);
            return;
    }
}
```

Include item types 6 and 7 for `CostumeFlg4` and `CostumeFlg5` in the implementation. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

### A6 Source Guard

```csharp
// Source: Tests/Blue/BlueA5SourceGuardTests.cs.
Assert.DoesNotContain("GreenShop", source, StringComparison.Ordinal);
Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
Assert.DoesNotContain("Adapters.GameProtocol.Green", source, StringComparison.Ordinal);
Assert.DoesNotContain("UserSaveDataGreen", source, StringComparison.Ordinal);
```

Guard files should include new Blue shop handlers, helpers, mappers, controllers, entities, and tests where appropriate. [VERIFIED: Tests/Blue/BlueA5SourceGuardTests.cs]

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Blue A1 shop routes returned success-only stubs | A6 must replace `getitemshopinfo.php` and `itempurchase.php` with mediator-backed behavior | Phase 01 scope | Prevents false-positive cabinet success with no server state mutation. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs; Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs; .planning/ROADMAP.md] |
| Green shop first season seeds from global Green medals | Blue shop seasons start at zero and do not use `UserSaveDataBlue` medal totals for shop accounting | D-09 through D-13 | Avoids corrupting Blue global medals and enforces season separation. [VERIFIED: Application/Common/GreenShopStateExtensions.cs; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| Green design used purchase plus rewardexecution pending/unlocked flow | Blue Phase 1 purchase unlocks immediately; rewardexecution is no-op success | D-01 through D-04 | Planner must not import older pending reward design into Blue A6. [VERIFIED: docs/superpowers/specs/2026-05-26-green-item-shop-support-design.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| Operator-only Blue shop data placeholder | Committed `blue_item_shop_data.json` derived from local official cache | D-05 through D-08 | Planner needs parser/default-data validation before runtime implementation relies on defaults. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md; H:\taiko\blue\rewardshopdata.bin] |

**Deprecated/outdated:**
- Treating Blue shop routes as safe stubs is outdated for A6. [VERIFIED: .planning/ROADMAP.md; Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs]
- Treating Green item-shop state as a reusable implementation is forbidden for Blue. [VERIFIED: .planning/research/PITFALLS.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | The local binary appears to contain four shop rows that decode as `item_type=3`, item ids `12, 7, 9, 10`, and prices `1300, 1500, 1500, 1500`; this is inferred from hexdump structure and is not a locked parser result. | Summary, Open Questions | Wrong defaults could be committed or wrong unlock bits could be applied; Wave 0 parser tests must prove or reject this interpretation. |

## Open Questions

1. **Official cache season envelope and ID resolution**
   - What we know: `H:\taiko\blue\rewardshopdata.bin` exists, is 179 bytes, includes `serialization::archive`, and has a compact payload with apparent item rows. [VERIFIED: Get-Item H:\taiko\blue\rewardshopdata.bin; Format-Hex H:\taiko\blue\rewardshopdata.bin]
   - What's unclear: The exact season metadata fields and catalog ID resolution were not fully decoded in this research pass. [ASSUMED]
   - Recommendation: Plan Wave 0 parser/test work before committing `blue_item_shop_data.json`; stop to discuss if full parse or ID resolution fails. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

2. **SHOP-06 wording versus D-02**
   - What we know: SHOP-06 mentions purchase and reward execution unlocks, while D-02 says Blue `rewardexecution.php` must log success and make no shop/save mutations in Phase 1. [VERIFIED: .planning/REQUIREMENTS.md; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]
   - What's unclear: A later plan checker may treat the broader requirement wording as binding unless the plan explicitly cites the context override. [ASSUMED]
   - Recommendation: Satisfy SHOP-06 through immediate purchase unlocks and document rewardexecution no-op as the locked Phase 1 interpretation. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build, test, EF tooling | yes | 10.0.201 available; `global.json` pins 10.0.100 with latestFeature roll-forward | Install matching .NET 10 SDK if unavailable. [VERIFIED: dotnet --version; global.json] |
| `dotnet ef` | EF migration | yes | 10.0.7 | Manual migration only if CLI unavailable, but current environment has CLI. [VERIFIED: dotnet ef --version] |
| Git | Source guard and commits | yes | 2.52.0.windows.1 | None needed. [VERIFIED: git --version] |
| Python | Optional binary parser probe | yes | Multiple Python launchers found | Write parser in C# tests if Python is undesirable. [VERIFIED: where.exe python] |
| `H:\taiko\blue\rewardshopdata.bin` | Default Blue shop data derivation | yes | 179 bytes, LastWriteTime 1981-03-15 02:41:51 local timestamp | Stop to discuss if absent during implementation. [VERIFIED: Get-Item H:\taiko\blue\rewardshopdata.bin; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| Blue runtime data directory | Local operator Blue game data | yes | `Host/wwwroot/data/blue/data` exists locally | Tests that depend on operator data should skip when absent. [VERIFIED: Get-ChildItem Host/wwwroot/data/blue; Tests/Blue/BlueCatalogLoaderTests.cs] |

**Missing dependencies with no fallback:**
- None detected for planning. [VERIFIED: environment probes above]

**Missing dependencies with fallback:**
- None detected for planning. [VERIFIED: environment probes above]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1. [VERIFIED: Directory.Packages.props; Tests/Tests.csproj] |
| Config file | `Tests/Tests.csproj`; no separate xUnit config detected in the scanned test project. [VERIFIED: Tests/Tests.csproj] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShop|FullyQualifiedName~BlueA6|FullyQualifiedName~Ac15"` |
| Full suite command | `dotnet test Tests/Tests.csproj` |
| Host build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a6"` when default Host bin output may be locked. [VERIFIED: AGENTS.md supplied in thread] |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| SHOP-01 | Blue initial data advertises item shop only when enabled and active season has rows; default song flags exclude active shop songs. | unit | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopProtocolTests` | No, Wave 0. [VERIFIED: Tests/Blue file listing] |
| SHOP-02 | Blue `getitemshopinfo.php` returns active season fields and ordered rows. | unit/mapper/controller | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopProtocolTests` | No, Wave 0. [VERIFIED: Tests/Blue file listing] |
| SHOP-03 | Blue loader uses `blue_item_shop_data.json`, fails fast when enabled data invalid, and derived defaults match official cache. | unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShopLoaderTests|FullyQualifiedName~BlueRewardShopDataParserTests"` | No dedicated files; existing `BlueCatalogLoaderTests` exists. [VERIFIED: Tests/Blue/BlueCatalogLoaderTests.cs] |
| SHOP-04 | Purchase validates `item_no`, optional `item_type`, `item_id`, and `item_price` before spend/unlock. | unit/mapper | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopPurchaseTests` | No, Wave 0. [VERIFIED: Tests/Blue file listing] |
| SHOP-05 | Medal totals persist by BAID and Blue season without Green tables/global Blue totals. | unit/integration | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopStateTests` | No, Wave 0. [VERIFIED: Tests/Blue file listing] |
| SHOP-06 | Purchase applies Blue save bits for supported item types and rewardexecution remains no-op success in Phase 1. | unit/controller | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShopPurchaseTests|FullyQualifiedName~BlueRewardExecution"` | No, Wave 0. [VERIFIED: Tests/Blue file listing] |
| SHOP-07 | Userdata and BAID hide locked active-season songs, tones, and costumes until purchased. | unit | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopLockingTests` | No, Wave 0. [VERIFIED: Tests/Blue file listing] |
| SHOP-08 | Source guards reject Green shop state, Green protocol constants, and Green wire models in A6 Blue files. | source guard | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA6SourceGuardTests` | No, Wave 0. [VERIFIED: Tests/Blue file listing] |

### Sampling Rate

- **Per task commit:** Run the targeted test file for the touched layer plus `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA6SourceGuardTests` after Blue shop files exist. [VERIFIED: Tests/Blue source guard pattern]
- **Per wave merge:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Blue|FullyQualifiedName~Ac15|FullyQualifiedName~GreenItemShop"`. [VERIFIED: existing Tests/Blue, Tests/Green, Tests/Ac15 layout]
- **Phase gate:** Run full `dotnet test Tests/Tests.csproj` and temp-output Host build before verification. [VERIFIED: AGENTS.md supplied in thread; Tests/Tests.csproj]

### Wave 0 Gaps

- [ ] `Tests/Blue/BlueRewardShopDataParserTests.cs` - proves official cache parser output and blocks partial defaults. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md]
- [ ] `Tests/Blue/BlueItemShopLoaderTests.cs` - Blue-specific default JSON load and invalid data checks. [VERIFIED: Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs]
- [ ] `Tests/Blue/BlueItemShopStateTests.cs` - zero-start season state and enabled/disabled playresult medal behavior. [VERIFIED: Tests/Green/GreenItemShopStateTests.cs; D-09 through D-13]
- [ ] `Tests/Blue/BlueItemShopProtocolTests.cs` - initialdata, getitemshopinfo, mapper optional fields. [VERIFIED: Tests/Green/GreenItemShopProtocolTests.cs]
- [ ] `Tests/Blue/BlueItemShopPurchaseTests.cs` - purchase preflight, validation, duplicate, insufficient balance, immediate unlock. [VERIFIED: Tests/Green/GreenItemShopPurchaseTests.cs; D-01 through D-04]
- [ ] `Tests/Blue/BlueItemShopLockingTests.cs` - userdata/BAID lock and unlock readback. [VERIFIED: Tests/Green/GreenItemShopLockingTests.cs; D-16]
- [ ] `Tests/Blue/BlueA6SourceGuardTests.cs` - Green dependency guard for A6 files. [VERIFIED: Tests/Blue/BlueA5SourceGuardTests.cs]

## Security Domain

Security enforcement is enabled in `.planning/config.json`; ASVS 5.0.0 is the current stable ASVS release per OWASP, and ASVS categories include authentication, session management, access control, validation/sanitization/encoding, and stored cryptography. [VERIFIED: .planning/config.json; CITED: https://owasp.org/www-project-application-security-verification-standard/; CITED: https://devguide.owasp.org/en/03-requirements/05-asvs/]

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | No new auth | Cabinet protocol identity is existing BAID/card flow; A6 does not add admin authentication. [VERIFIED: Application/Handlers/BaidQuery.Blue.cs; .planning/ROADMAP.md] |
| V3 Session Management | No new session | A6 does not add WebUI/admin sessions. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| V4 Access Control | Yes, BAID-scoped state | Validate and mutate only rows for the request BAID and active Blue season; never query Green shop state for Blue. [VERIFIED: Application/Handlers/ItemPurchaseCommand.Green.cs pattern; .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| V5 Validation, Sanitization and Encoding | Yes | Server-side catalog validation, optional field presence checks, price/id/type tuple validation, fixed byte widths, and fail-fast binary/default parsing. [VERIFIED: Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs; Adapters.GameProtocol.Blue/Wire/Game.cs; Application/Common/BlueProtocolBytes.cs] |
| V6 Stored Cryptography | No new crypto | A6 stores shop state and byte arrays; it does not add secrets, tokens, or cryptographic material. [VERIFIED: Domain/Entities/UserSaveDataBlue.cs; proposed Blue shop state scope] |

### Known Threat Patterns for Blue Shop

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Client-forged item price, type, or id | Tampering | Compare request tuple against active Blue catalog row before spend/unlock. [VERIFIED: Application/Handlers/ItemPurchaseCommand.Green.cs; D-04] |
| Duplicate purchase double-spend | Tampering | Key Blue shop item state by `(Baid, SeasonId, ItemType, ItemId)` and reject existing unlocked rows. [VERIFIED: Domain/Entities/GreenShopItemState.cs; Tests/Green/GreenItemShopPurchaseTests.cs] |
| Cross-era state mutation | Tampering | A6 source guards and Blue-specific DbSets/entities. [VERIFIED: Tests/Blue/BlueA5SourceGuardTests.cs; .planning/research/PITFALLS.md] |
| Malformed committed shop JSON | Tampering / DoS | Fail-fast AC15 loader validation for enabled shop. [VERIFIED: Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs] |
| Malformed official cache parser output | Tampering / Integrity | Parser tests compare binary-derived expected values before committing JSON; stop if unresolved. [VERIFIED: .planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md] |
| Integer overflow in medal totals | Tampering / DoS | Reuse `CanAdd` style overflow checks before incrementing totals. [VERIFIED: Application/Handlers/ItemPurchaseCommand.Green.cs; Application/Handlers/UpdatePlayResultCommand.Blue.cs] |

## Sources

### Primary (HIGH confidence)

- `.planning/phases/01-blue-a6-item-shop-and-unlocking/01-CONTEXT.md` - user decisions D-01 through D-18, phase boundary, canonical refs. [VERIFIED: repo file]
- `.planning/REQUIREMENTS.md` - SHOP-01 through SHOP-08 requirement wording. [VERIFIED: repo file]
- `.planning/ROADMAP.md` - Phase 01 success criteria and plan breakdown. [VERIFIED: repo file]
- `.planning/research/ARCHITECTURE.md` - A6 architecture constraints. [VERIFIED: repo file]
- `.planning/research/PITFALLS.md` - A6 pitfalls and guardrails. [VERIFIED: repo file]
- `Application/Handlers/ItemPurchaseCommand.Green.cs`, `GetItemShopInfoQuery.Green.cs`, `UserDataQuery.Green.cs`, `BaidQuery.Green.cs`, `UpdatePlayResultCommand.Green.cs` - Green shop reference behavior. [VERIFIED: repo file]
- `Application/Handlers/*.Blue.cs`, `Application/Common/BlueProtocolBytes.cs`, `Domain/Entities/UserSaveDataBlue.cs` - Blue state and byte-width surfaces. [VERIFIED: repo file]
- `Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Application/Catalog/Blue/*ItemShop*.cs` - Blue/AC15 catalog stack. [VERIFIED: repo file]
- `Adapters.GameProtocol.Blue/Wire/Game.cs`, `proto/blue/taiko.proto` - Blue wire field and optional-presence contract. [VERIFIED: repo file]
- `Tests/Green/GreenItemShop*.cs`, `Tests/Blue/BlueA*SourceGuardTests.cs`, `Tests/Blue/BlueServerSettingsValidationTests.cs` - test patterns. [VERIFIED: repo file]
- `H:\taiko\blue\rewardshopdata.bin` - local official cache presence, size, and hexdump. [VERIFIED: Get-Item; Format-Hex]

### Secondary (MEDIUM confidence)

- `docs/superpowers/specs/2026-05-26-green-item-shop-support-design.md` - historical Green item shop design and IDA-backed AC15 mapping reference where not contradicted by Blue context. [VERIFIED: repo file]
- OWASP ASVS project page - ASVS purpose and stable 5.0.0 release. [CITED: https://owasp.org/www-project-application-security-verification-standard/]
- OWASP Developer Guide ASVS page - ASVS category list. [CITED: https://devguide.owasp.org/en/03-requirements/05-asvs/]

### Tertiary (LOW confidence)

- Inferred `rewardshopdata.bin` item-row interpretation from hexdump structure; must be validated by parser tests before implementation locks defaults. [ASSUMED]

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - all libraries and tools are existing repo dependencies or local CLI probes. [VERIFIED: Directory.Packages.props; dotnet --version; dotnet ef --version]
- Architecture: HIGH - phase context and codebase agree on Blue-owned persistence, handlers, mappers, catalogs, and tests. [VERIFIED: .planning/research/ARCHITECTURE.md; codebase grep]
- Pitfalls: HIGH - most pitfalls are documented in A6 context and existing Green/Blue tests. [VERIFIED: .planning/research/PITFALLS.md; Tests/Green; Tests/Blue]
- Official cache defaults: MEDIUM - file presence and bytes are verified, but parser semantics need implementation-time proof. [VERIFIED: Get-Item; Format-Hex; ASSUMED row interpretation]

**Research date:** 2026-05-29
**Valid until:** 2026-06-05, because this is an active phase and Blue implementation files may change quickly. [ASSUMED]
