# Phase 10: Evidence-Backed Tokkun State Persistence and Readback - Pattern Map

**Mapped:** 2026-06-06
**Scope:** Blue Tokkun classifier, storage, handler persistence, and userdata readback

## Summary

Phase 10 should follow the existing Blue-owned partial-file and EF Core patterns. The closest analogs are Blue battle persistence for schema ownership and reload tests, Phase 9 Tokkun playresult tests for no-cross-write behavior, and Blue userdata mapper/query tests for optional protocol readback.

## File Classification

| Planned File | Role | Closest Analog | Pattern To Reuse |
|--------------|------|----------------|------------------|
| `Domain/Enums/PlayMode.cs` | Named protocol constant | Existing `DanMode = 1`, `AiBattle = 6` entries | Add `Tokkun = 3`; do not change existing values. |
| `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` | Wire-to-common classifier and raw fact mapping | Existing Tokkun and battle mapping in same file | Keep generated wire DTOs out of handler logic; classify from `request.PlayMode == (uint)PlayMode.Tokkun`; preserve `TokkunstageData` as raw detail. |
| `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` | Blue-only common DTO extension | Existing Phase 9 Tokkun DTO partial | Keep Tokkun fields Blue-only and raw; avoid payment/reward/progression names. |
| `Domain/Entities/UserSaveDataBlue.cs` | Blue save/readback state | Existing `ItemshopTutorialFlg`, `WaiwaiTutorialFlg` fields | Add nullable raw `uint? TokkunTutorialFlg`; do not initialize a default value. |
| `Domain/Entities/BlueTokkunStageResult.cs` | Blue Tokkun append-only history row | `Domain/Entities/BlueBattleStageResult.cs` | Use `Id`, `Baid`, protocol/raw fields, and `UserDatum? Ba`; no server `CreatedAt`/`UploadedAtUtc`. |
| `Application/Abstractions/ITaikoDbContext.Blue.cs` | Application persistence port | Existing Blue battle DbSets | Add `DbSet<BlueTokkunStageResult> BlueTokkunStageResults { get; }`. |
| `Infrastructure/Persistence/TaikoDbContext.Blue.cs` | EF Core mapping | Existing Blue battle mappings | Configure table, generated `Id`, `Baid` FK, and indexes in `OnModelCreatingBlue`. |
| `Infrastructure/Persistence/Migrations/*_AddBlueTokkunState.cs` | Schema migration | `20260530185853_AddBlueBattleState.cs` and later battle migrations | Generate via EF; migration should add nullable tutorial column and create only `BlueTokkunStageResults`. |
| `Application/Handlers/UpdatePlayResultCommand.Blue.cs` or `.BlueTokkun.cs` | Tokkun persistence side effects | Current Tokkun success branch and Blue battle helper partial | Keep guest/unknown success first, then Tokkun branch before battle/normal; allowed writes only tutorial + history. |
| `Application/Dtos/CommonUserDataResponse.Blue.cs` | Blue-only common userdata extension | Existing `IsDevilBlue` partial | Add nullable `uint? TokkunTutorialFlg`. |
| `Application/Handlers/UserDataQuery.Blue.cs` | Blue userdata projection | Existing Blue save/profile field projection | Project `saveData.TokkunTutorialFlg` without defaulting. |
| `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs` | Common-to-wire optional response mapper | Existing manual `DispTaikojukuDan` handling and `BlueMapperTests` | Set `response.TokkunTutorialFlg` only when common value is present. |
| `Tests/Blue/BluePlayResultMapperTests.cs` | Mapper proof | Existing Tokkun Phase 9 tests | Update classifier expectations to `PlayMode.Tokkun = 3`; keep raw stage fact tests. |
| `Tests/Blue/BluePlayResultHandlerTests.cs` | Handler no-cross-write proof | Existing Tokkun existing/unknown/mixed tests | Update to assert allowed Tokkun writes and still-empty forbidden state. |
| `Tests/Blue/BlueMapperTests.cs` | Wire mapper optional field proof | `UserDataMapper_Blue_OmitsTokkunTutorialFlag` | Keep absence test and add presence/raw-value test. |
| `Tests/Blue/BlueUserDataTests.cs` | Query readback proof | Existing Blue userdata projection tests | Add persisted tutorial readback test. |
| `Tests/Blue/BlueTokkunPersistenceTests.cs` | Schema and reload proof | `BlueBattlePersistenceTests` | Use SQLite in-memory `EnsureCreatedAsync`, save/reload representative Tokkun rows, assert raw song order/duplicates. |
| `Tests/Blue/BlueTokkunPersistenceShapeTests.cs` | Source-level storage boundary proof | `BlueBattlePersistenceShapeTests` | Assert Blue-owned files/mappings and absence of forbidden Green/normal/battle/shop/Banacoin semantics in Tokkun entity/mapping. |

## Shared Patterns

### Blue-Owned Persistence Surface

Use the same four-layer state exposure pattern:

1. `Domain/Entities/{Entity}.cs`
2. `Application/Abstractions/ITaikoDbContext.Blue.cs`
3. `Infrastructure/Persistence/TaikoDbContext.Blue.cs`
4. `Infrastructure/Persistence/Migrations/*`

The entity and mapping should reference shared identity through `UserDatum.Baid`, not Green, Nijiiro, normal score, battle, shop, or Banacoin storage.

### Optional Raw Tutorial Value

`tokkun_tutorial_flg` must remain nullable until a Tokkun-classified upload with the optional field present is handled. Mapper and userdata tests should cover:

- absent common value -> `UserDataResponse.ShouldSerializeTokkunTutorialFlg()` is false
- present value `0` -> serializes `0`
- present value `1` -> serializes `1`
- present non-bool value such as `7` -> serializes `7`

### Raw Ordered Song List

`TookunSongnoes` should preserve client order and duplicates. Prefer a JSON string column such as `TookunSongnoesJson` serialized through `System.Text.Json`; tests should use a value like `[101, 102, 101]` and assert the same order after SQLite reload.

### No-Cross-Write Assertions

Retain and extend `AssertTokkunForbiddenBlueStateEmptyAsync` style checks. After Phase 10, forbidden state means all existing normal/battle/Dani/favorite/recent/shop/Banacoin-like tables remain empty or unchanged, while these allowed writes may occur:

- `UserSaveData_Blue.TokkunTutorialFlg`
- `BlueTokkunStageResults`

## No Analog Found

| Planned File / Concept | Why No Direct Analog | Required Handling |
|------------------------|----------------------|-------------------|
| `TookunSongnoesJson` | Existing Blue battle rows store scalar/byte-array state but not ordered numeric lists. | Use `System.Text.Json` and tests proving duplicate/order fidelity. |
| `PlayMode.Tokkun = 3` | Earlier phases deliberately avoided a numeric value. | Add only because Phase 10 context D-01/D-02 supersedes the old unknown guidance. |

## Forbidden Pattern References

Do not use these as Tokkun storage analogs:

- Green AI Battle tables or helpers
- Blue normal score/best tables
- Blue Dan tables
- Blue battle state tables except as EF shape precedent
- Blue shop season/item state
- Banacoin routes or payment-like state
- AdminApi/WebUI surfaces
- Source-word Tokkun guard tests

## Verification Pattern Map

| Concern | Test Pattern | Target Test File |
|---------|--------------|------------------|
| Mapper classifier and raw facts | Unit mapper tests with real `PlayResultMappers.Map` | `BluePlayResultMapperTests.cs` |
| Allowed Tokkun writes | SQLite handler test with real `UpdatePlayResultCommandHandler` | `BluePlayResultHandlerTests.cs` |
| Forbidden state writes | Table-count and unchanged-save assertions | `BluePlayResultHandlerTests.cs` |
| Schema reload | SQLite in-memory `EnsureCreatedAsync`, save, dispose/reload, assert | `BlueTokkunPersistenceTests.cs` |
| Source-shape boundary | Read entity/interface/DbContext/migration source and assert snippets | `BlueTokkunPersistenceShapeTests.cs` |
| Optional userdata field | `ShouldSerializeTokkunTutorialFlg()` absence/presence checks | `BlueMapperTests.cs` |
| Query readback | `UserDataQueryHandler` with persisted `UserSaveDataBlue` | `BlueUserDataTests.cs` |

## Planner Notes

- Plan 01 should define classifier/storage contracts and schema proof.
- Plan 02 should implement handler persistence and no-cross-write behavior.
- Plan 03 should implement userdata readback and optional protobuf serialization.
- Plan 02 and Plan 03 should depend on Plan 01. Plan 03 may also depend on Plan 02 if it includes end-to-end playresult-then-userdata proof.
