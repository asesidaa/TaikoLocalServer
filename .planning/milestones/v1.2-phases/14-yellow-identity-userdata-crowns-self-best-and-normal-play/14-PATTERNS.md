# Phase 14: Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play - Pattern Map

**Mapped:** 2026-06-08
**Files analyzed:** 50
**Analogs found:** 50 / 50

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `Domain/Entities/UserSaveDataYellow.cs` | entity | shared identity -> Yellow save | `UserSaveDataBlue.cs`, `UserSaveDataGreen.cs` | exact |
| `Domain/Entities/SongBestDatumYellow.cs` | entity | Yellow playresult -> selfbest/crowns | `SongBestDatumBlue.cs`, `SongBestDatumGreen.cs` | exact |
| `Domain/Entities/SongPlayDatumYellow.cs` | entity | Yellow playresult history | `SongPlayDatumBlue.cs`, `SongPlayDatumGreen.cs` | exact |
| `Domain/Entities/YellowFavoriteSongs.cs` | entity | playresult -> userdata | `BlueFavoriteSongs.cs`, `GreenFavoriteSongs.cs` | exact |
| `Domain/Entities/YellowRecentSongs.cs` | entity | playresult -> userdata | `BlueRecentSongs.cs`, `GreenRecentSongs.cs` | exact |
| `Application/Abstractions/ITaikoDbContext.Yellow.cs` | contract | Application -> EF | `ITaikoDbContext.Blue.cs`, `ITaikoDbContext.Green.cs` | exact |
| `Infrastructure/Persistence/TaikoDbContext.Yellow.cs` | EF mapping | entities -> SQLite | `TaikoDbContext.Blue.cs`, `TaikoDbContext.Green.cs` | exact |
| `Application/Common/UserSaveDataYellowExtensions.cs` | default helper | BAID/mydon/userdata -> save | `UserSaveDataBlueExtensions.cs`, `UserSaveDataGreenExtensions.cs` | exact |
| `Application/Handlers/BaidQuery.Yellow.cs` | handler partial | card access -> BAID profile | `BaidQuery.Blue.cs`, `BaidQuery.Green.cs` | exact |
| `Application/Handlers/AddMyDonEntryCommand.Yellow.cs` | handler partial | mydon entry -> identity/save | `AddMyDonEntryCommand.Blue.cs`, `.Green.cs` | exact |
| `Application/Handlers/UserDataQuery.Yellow.cs` | handler partial | Yellow save/catalog -> common userdata | `UserDataQuery.Blue.cs`, `UserDataQuery.Green.cs` | exact |
| `Application/Ac15/YellowAc15UserDataAdapter.cs` | adapter | Yellow save -> AC15 snapshot | `BlueAc15UserDataAdapter.cs`, `GreenAc15UserDataAdapter.cs` | exact |
| `Application/Handlers/GetSelfBestQuery.Yellow.cs` | handler partial | Yellow best rows -> common selfbest | `GetSelfBestQuery.Blue.cs`, `.Green.cs` | exact |
| `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` | handler partial | common playresult -> Yellow state | `UpdatePlayResultCommand.Green.cs`, `.Blue.cs` | role-match |
| `Application/Ac15/YellowAc15NormalPlayAdapter.cs` | persistence adapter | AC15 normal loop -> Yellow tables | `BlueAc15NormalPlayAdapter.cs`, `GreenAc15NormalPlayAdapter.cs` | exact |
| `Adapters.GameProtocol.Yellow/Mappers/BaidResponseMapper.cs` | mapper | common BAID -> Yellow wire | `Blue/Mappers/BaidResponseMapper.cs`, `Green/Mappers/BaidResponseMapper.cs` | exact |
| `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs` | mapper | common userdata -> Yellow wire | Blue/Green userdata mappers | exact |
| `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs` | mapper | Yellow wire -> common playresult | Blue/Green playresult mappers | exact |
| `Adapters.GameProtocol.Yellow/Mappers/SelfBestMappers.cs` | mapper | common selfbest -> Yellow wire | Blue/Green selfbest mappers | exact |
| `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` | controller scaffold | Yellow routes -> Mediator | Current Yellow metadata route replacements | modify/split |
| `Tests/Yellow/YellowIdentityHandlerTests.cs` | tests | identity/default save | `BlueIdentityHandlerTests.cs`, `GreenIdentityHandlerTests.cs` | exact |
| `Tests/Yellow/YellowUserDataProtocolTests.cs` | tests | userdata mapper/route | Blue/Green identity/userdata tests | role-match |
| `Tests/Yellow/YellowPlayResultHandlerTests.cs` | tests | normal play writes | `BluePlayResultHandlerTests.cs`, Green playresult tests | exact |
| `Tests/Yellow/YellowSelfBestTests.cs` | tests | self-best route/mapper | `BlueSelfBestTests.cs` | exact |
| `Tests/Yellow/YellowCrownsDataTests.cs` | tests | crown packing/wire bytes | `BlueCrownsDataTests.cs`, `Ac15CrownServiceTests.cs` | exact plus Yellow-specific |
| `Tests/Yellow/YellowPersistenceBoundaryTests.cs` | tests | no cross-era writes | Blue battle/Tokkun no-cross-write tests | role-match |

## Pattern Assignments

### Yellow EF Slice

**Analog:** `TaikoDbContext.Blue.cs` / `TaikoDbContext.Green.cs`

Add a Yellow persistence partial that maps:

- `UserSaveDataYellow` -> `UserSaveData_Yellow`
- `SongBestDatumYellow` -> `SongBestDatum_Yellow`
- `SongPlayDatumYellow` -> `SongPlayDatum_Yellow`
- `YellowFavoriteSongs` -> `YellowFavoriteSongs`
- `YellowRecentSongs` -> `YellowRecentSongs`

Use the same composite keys and cascade-to-`UserDatum` pattern as Blue/Green. Do not add Yellow Dan, shop, Tokkun, Banacoin, AdminApi/WebUI, or battle tables in Phase 14.

### Default Save Helper

**Analog:** `UserSaveDataBlueExtensions.CreateDefaultBlueSaveData`

Use Yellow profile limits from `Ac15EraProfiles.Yellow.Limits` or current AC15 byte helpers for flag sizes. Defaults should include:

- empty title/titleplate 0
- default colors 0/1/3
- costume slots 0
- costume/tone flags with default 0 unlocked
- title/release/default option byte arrays
- `IsAutoCostumeOn = true`
- `IsTojiru = true`
- `LastPlayDatetime = DateTime.UnixEpoch`
- nullable Tokkun tutorial only if added as schema placeholder; no Tokkun behavior

### Handler Dispatch Shape

**Analog:** existing dispatcher switch files

Add `GameEra.Yellow => HandleYellow(...)` branches and matching partial declarations to:

- `BaidQuery.cs`
- `AddMyDonEntryCommand.cs`
- `UserDataQuery.cs`
- `UpdatePlayResultCommand.cs`
- `GetSelfBestQuery.cs`

### Controller Replacement Shape

**Analog:** Phase 13 metadata replacements in `YellowScaffoldControllers.cs`

Only these six Yellow route actions should switch from no-state scaffold to Mediator:

- `baidcheck.php`
- `mydonentry.php`
- `userdata.php`
- `playresult.php`
- `selfbest.php`
- `crownsdata.php`

Keep Phase 15/16 routes as no-state scaffolds.

## Code Excerpts

### Normal Play Reuse Shape

```csharp
return await Ac15NormalPlayService.SaveAsync(
    request.Baid,
    playResultData,
    Ac15EraProfiles.Yellow,
    new YellowAc15NormalPlayAdapter(context),
    DefaultAc15EraHooks.Instance,
    cancellationToken);
```

### Self-Best Query Shape

```csharp
var canonicalRows = bestRows.Select(row => new Ac15BestRow(
    row.SongId,
    row.Difficulty,
    row.IsShin,
    row.BestScore,
    row.BestRate,
    row.BestCrown));

return Ac15SelfBestService.BuildResponse(request.Difficulty, requestedSongs, canonicalRows);
```

### Crown Response Proof Requirement

Yellow generated wire proves:

```text
CrownsDataResponse field 3 = hash_crown_flg
```

Tests must serialize a Yellow `CrownsDataResponse` and assert field 3 payload bytes match the selected raw/gzip contract.

## Boundary Guard Patterns

Use source tests to prove:

- Yellow production code does not reference `UserSaveDataBlue`, `UserSaveDataGreen`, `SongBestDataBlue`, `SongBestDataGreen`, `SongPlayDataBlue`, or `SongPlayDataGreen` from Yellow handlers/adapters.
- Yellow route replacements are exactly the six Phase 14 endpoints.
- `itempurchase.php`, `rewardcardcheck.php`, `rewardexecution.php`, Banacoin-adjacent routes, Dani/shop routes, heartbeat/headclerk/bookkeeping/coinsetting, and Tokkun surfaces remain no-state unless Phase 13 already made them catalog-backed metadata.
- No `YellowBattle`, `battleuserdata.php`, or Blue battle DTO references appear.

## PATTERN MAPPING COMPLETE
