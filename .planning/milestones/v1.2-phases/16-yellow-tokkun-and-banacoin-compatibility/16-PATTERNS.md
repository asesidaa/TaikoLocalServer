# Phase 16: Yellow Tokkun and Banacoin Compatibility - Pattern Map

**Mapped:** 2026-06-08
**Files analyzed:** 30
**Analogs found:** 30 / 30

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs` | mapper | Yellow wire -> common Tokkun facts | Blue Phase 10 mapper, current Yellow mapper | exact |
| `Domain/Entities/YellowTokkunStageResult.cs` | entity | Yellow Tokkun upload -> append-only history | `Domain/Entities/BlueTokkunStageResult.cs` | exact |
| `Application/Abstractions/ITaikoDbContext.Yellow.cs` | persistence port | Application -> Yellow EF state | `ITaikoDbContext.Blue.cs` Blue Tokkun DbSet | exact |
| `Infrastructure/Persistence/TaikoDbContext.Yellow.cs` | EF mapping | Yellow entity -> SQLite table | `TaikoDbContext.Blue.cs` Blue Tokkun mapping | exact |
| `Infrastructure/Persistence/Migrations/*_AddYellowTokkunState.cs` | migration | model changes -> SQLite schema | Blue `AddBlueTokkunState` migration | exact |
| `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` | handler partial | playresult -> Yellow normal/Dan/shop/Tokkun branches | current Yellow handler and Blue Tokkun helper | exact |
| `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs` | handler helper | classified Tokkun -> tutorial/history writes | `UpdatePlayResultCommand.BlueTokkun.cs` | exact |
| `Application/Ac15/Ac15EraProfiles.cs` | profile | shared service feature placement | Blue profile Tokkun placement | exact |
| `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs` | mapper | common userdata -> Yellow optional wire field | Blue userdata Tokkun mapper | exact |
| `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` | controller host | Banacoin routes -> direct success response | Blue Banacoin controllers | role-match |
| `Tests/Yellow/YellowPlayResultHandlerTests.cs` | tests | mapper/handler proof | current Yellow playresult tests, Blue Tokkun tests | exact |
| `Tests/Yellow/YellowTokkunPersistenceTests.cs` | tests | EF reload/history proof | `BlueTokkunPersistenceTests.cs` | exact |
| `Tests/Yellow/YellowTokkunPersistenceShapeTests.cs` | tests | source-shape boundary proof | `BlueTokkunPersistenceShapeTests.cs` | exact |
| `Tests/Yellow/YellowUserDataProtocolTests.cs` | tests | query/mapper optional readback | current Yellow userdata tests, Blue userdata tests | exact |
| `Tests/Ac15/Ac15UserDataServiceTests.cs` | tests | shared AC15 service placement | existing AC15 profile/userdata tests | exact |
| `Tests/Yellow/YellowBanacoinCompatibilityTests.cs` | tests | route/source statelessness | Blue route skeleton and Banacoin source gates | role-match |
| `Tests/Yellow/YellowRouteSkeletonTests.cs` | tests | route ownership/no-Mediator guard | current Yellow route tests | exact |
| `Tests/Yellow/YellowPersistenceBoundaryTests.cs` | tests | allowed/forbidden Yellow tables | current Yellow boundary tests | exact |

## Pattern Assignments

### Yellow Tokkun Schema

Use `BlueTokkunStageResult` only as a shape analog. The Yellow entity should be named `YellowTokkunStageResult`, mapped to `YellowTokkunStageResults`, and exposed through Yellow DbSets. Store:

- `Id`
- `Baid`
- `PlayDatetime`
- `PlayMode`
- `BanacoinDatetime`
- `TokkunSongCnt`
- `TookunSongnoesJson`
- `TokkunSpeedchangeCnt`
- `TokkunAutoplayCnt`
- `TokkunJumpCnt`
- `UserDatum? Ba`

Do not add `UploadedAtUtc`, `CreatedAt`, unique keys, payment identity, reward, score, crown, unlock, or progress-derived columns.

### Yellow Tokkun Handler

Preserve the existing Yellow branch point:

```csharp
if (IsYellowTokkunShaped(playResultData))
{
    return 1;
}
```

Replace only the `return 1` placeholder with a helper call, keeping it after guest/unknown-user exits and before valid-stage filtering, shop medal state, profile counters, Dan handling, unlocks, favorites/recent, and normal save logic.

### Optional Userdata Readback

`YellowAc15UserDataAdapter.CreateSnapshot` already passes `saveData.TokkunTutorialFlg`. The needed pattern is:

- Change `Ac15EraProfiles.Yellow` to `HasTokkunTutorialFlagInUserData: true`.
- Keep `Green` false and `Blue` true.
- In Yellow `UserDataMappers.Map`, set `response.TokkunTutorialFlg` only when `common.TokkunTutorialFlg` is present.
- Tests must assert both absence and raw values such as `0`, `1`, and `7`.

### Banacoin Routes

Keep Yellow `balancecheck.php`, `banacoinpayment.php`, `banacoinerrorlog.php`, and `getbanacoininfo.php` as direct controller responses. Logging should use the full request object:

```csharp
Logger.LogInformation("Yellow BalanceCheck request: {@Request}", request);
```

Required `Personid` echo on `balancecheck.php` and `banacoinpayment.php` is protocol compatibility only. It is not identity authority and should not create state.

## Shared Patterns

### No-Cross-Write Assertion Buckets

Phase 16 handler tests should explicitly check:

- Yellow normal: `SongPlayDataYellow`, `SongBestDataYellow`, crown source rows.
- Yellow Dani: `DanScoreDataYellow`, `DanStageScoreDataYellow`.
- Yellow profile/save side effects: medals, counters, option/profile flags, unlock bitsets, `LastPlayDatetime`, current costume/title fields except `TokkunTutorialFlg`.
- Yellow shop: `YellowShopSeasonStates`, `YellowShopItemStates`.
- Yellow favorite/recent: `YellowFavoriteSongs`, `YellowRecentSongs`.
- Cross-era: Blue, Green, Nijiiro normal/Dan/shop rows; Blue battle rows; Blue Tokkun rows.
- Banacoin: no wallet/payment/coupon/receipt/transaction state or abstractions.

### Raw Song JSON

Use `System.Text.Json.JsonSerializer.Serialize(stage.TookunSongnoes)` and tests with `[101, 102, 101]` to prove order and duplicates are preserved.

## No Analog Found

No new architecture pattern is needed. The only Yellow-specific distinction is that Yellow Banacoin routes are currently grouped in `YellowScaffoldControllers.cs`; an implementation may split them into separate controller files only if it keeps route ownership and source tests updated.

## PATTERN MAPPING COMPLETE
