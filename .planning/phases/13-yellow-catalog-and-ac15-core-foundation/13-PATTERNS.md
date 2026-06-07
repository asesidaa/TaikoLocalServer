# Phase 13: Yellow Catalog and AC15 Core Foundation - Pattern Map

**Mapped:** 2026-06-08
**Files analyzed:** 44
**Analogs found:** 44 / 44

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `Application/Abstractions/IYellowCatalog.cs` | application contract | catalog read | `IBlueCatalog.cs`, `IGreenCatalog.cs` | exact |
| `Application/Common/CatalogExtensions.cs` | extension | catalog dispatch | existing `Blue()` / `Green()` methods | exact |
| `Application/Catalog/Yellow/*.cs` | DTO/catalog entries | loaded catalog state | `Application/Catalog/Blue`, `Application/Catalog/Green` | exact |
| `Infrastructure/GameDataCatalog/Yellow/YellowGameDataPaths.cs` | path helper | data root resolution | `BlueGameDataPaths.cs`, `GreenGameDataPaths.cs` | exact |
| `Infrastructure/GameDataCatalog/Yellow/YellowRequiredDataFiles.cs` | guard | startup required files | `BlueRequiredDataFiles.cs`, `GreenRequiredDataFiles.cs` | exact |
| `Infrastructure/GameDataCatalog/Yellow/Yellow*Loader.cs` | loader wrappers | file -> Yellow entries | `Blue*Loader.cs`, `Green*Loader.cs` | exact |
| `Infrastructure/GameDataCatalog/Yellow/YellowEraGameDataCatalog.cs` | runtime catalog | loaders -> `IYellowCatalog` | `BlueEraGameDataCatalog.cs`, `GreenEraGameDataCatalog.cs` | exact |
| `Infrastructure/DependencyInjection.cs` | DI registration | enabled era -> services | Blue/Green catalog branches | exact |
| `Application/Ac15/Ac15EraProfiles.cs` | profile | capability contract | Blue/Green profile definitions | exact |
| `Application/Ac15/Ac15CatalogSnapshotFactory.cs` | mapper | Yellow catalog -> AC15 snapshot | `FromBlue`, `FromGreen` | exact |
| `Application/Dtos/CommonInitialDataCheckResponse.Yellow.cs` | common DTO partial | handler -> mapper | `CommonInitialDataCheckResponse.Blue.cs` | role-match |
| `Application/Handlers/GetInitialDataQuery.Yellow.cs` | handler partial | catalog -> common DTO | Blue/Green initial data partials | exact |
| `Application/Handlers/GetTelopQuery.Yellow.cs` | handler partial | catalog -> telop DTO | Blue/Green telop partials | exact |
| `Application/Handlers/GetFolderQuery.Yellow.cs` | handler partial | catalog -> folder DTO | Blue/Green folder partials | exact |
| `Application/Handlers/GetTaikojukuQuery.Yellow.cs` | handler partial | catalog -> Taikojuku DTO | Blue/Green Taikojuku partials | exact |
| `Application/Handlers/GetItemShopInfoQuery.Yellow.cs` | handler partial | catalog -> item shop DTO | Blue/Green item shop partials | exact |
| `Application/Handlers/GetRecommendQuery.Yellow.cs` | handler partial | catalog -> recommend DTO | Green recommend handler shape | role-match |
| `Application/Handlers/TournamentCheckQuery.Yellow.cs` | handler partial | catalog -> tournament/gacha DTO | Green stub + catalog DTO | role-match |
| `Application/Handlers/GetChallengeCompeQuery.Yellow.cs` | handler partial | catalog -> challenge DTO | Green stub | role-match |
| `Adapters.GameProtocol.Yellow/Mappers/*.cs` | wire mapper | common DTO -> Yellow wire | Blue/Green mappers | exact |
| `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` | controller scaffold | route -> response | Phase 12 scaffold | modify/split |
| `Tests/Yellow/*Catalog*.cs` | tests | catalog proof | Blue/Green catalog tests, Ac15 tests | exact |
| `Tests/Yellow/*Protocol*.cs` | tests | handler/mapper proof | Blue/Green protocol tests | role-match |
| `Tests/Yellow/YellowCatalogBoundaryTests.cs` | source guard | scope proof | Yellow no-battle/source guards | exact |

## Pattern Assignments

### Yellow Era Catalog Contract

**Analog:** `Application/Abstractions/IBlueCatalog.cs` and `IGreenCatalog.cs`

Use Yellow-owned catalog properties for music info file order, song hash version, Taikojuku file order, item shop catalog, event folders, telops, gacha/tournament dictionaries, recommend entry, and movies. Do not expose Blue/Green entry types through Yellow.

### Required Data Paths

**Analog:** `BlueGameDataPaths.cs`

Yellow should define:

```csharp
private const string ConfigDirectory = "ST9100-1";
public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), "data");
public static string ConfigRoot => Path.Combine(GameDataRoot, "config", ConfigDirectory);
public static string MusicInfoXml => Path.Combine(ConfigRoot, "musicinfo.xml");
public static string MusicMedleyInfoXml => Path.Combine(ConfigRoot, "musicmedleyinfo.xml");
public static string DefMusicBin => Path.Combine(ConfigRoot, "defmusic.bin");
public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");
public static string MovieDirectory => Path.Combine(GameDataRoot, "movie");
```

`YellowRequiredDataFiles` should include only the four Phase 13 required files. Optional sidecars are loaded separately and must not be included in required paths.

### Shared AC15 Loader Wrappers

**Analog:** `BlueMusicInfoLoader.cs`, `GreenTaikojukuLoader.cs`, `BlueItemShopLoader.cs`

Create Yellow wrappers around shared AC15 loaders:

- `YellowMusicInfoLoader` maps `Ac15MusicInfoEntry` into `YellowMusicInfoEntry`.
- `YellowTaikojukuLoader` maps `Ac15TaikojukuEntry` into `YellowTaikojukuEntry`.
- `YellowTuningLoader` can call `Ac15TuningLoader` directly or wrap it for era naming.
- `YellowItemShopLoader` calls `Ac15ItemShopLoader` with Yellow settings and `yellow_item_shop_data.json`.
- `YellowTelopLoader`, `YellowRecommendLoader`, `YellowMovieLoader`, `YellowGachaLoader`, and `YellowTournamentLoader` mirror Blue/Green optional behavior.

### AC15 Snapshot Mapping

**Analog:** `Ac15CatalogSnapshotFactory.FromBlue` / `FromGreen`

Add `FromYellow(IYellowCatalog yellow)` using Yellow entries. This is the main bridge that lets `Ac15InitialDataService`, `Ac15CatalogReadbackService`, and `Ac15TaikojukuService` remain shared.

### Yellow Initial Data

**Analog:** `GetInitialDataQuery.Blue.cs`, `GetInitialDataQuery.Green.cs`, and `InitialDataMappers`

Yellow proto supports `ary_telop_data`, `ary_eventfolder_data`, `ary_taikojuku_data`, `ary_itemshop_data`, `ary_legalterms_data`, `is_danplay`, `is_close`, and `is_itemshop`. Add a Yellow common DTO partial for these row lists if needed. Mapper should not emit Blue battle fields and should not advertise rows unless the snapshot has rows.

### Yellow Metadata Routes

**Analog:** Blue/Green `GetFolderController`, `GetTelopController`, `GetItemShopInfoController`, `TaikojukuController`

Replace the Phase 12 no-state scaffold actions with async Mediator actions only for:

- `initialdatacheck.php`
- `gettelop.php`
- `getfolder.php`
- `taikojuku.php`
- `getitemshopinfo.php`
- `recommend.php`
- `tournamentcheck.php`
- `challengecompe.php`

Keep the following no-state or deferred in Phase 13:

- `baidcheck.php`
- `mydonentry.php`
- `userdata.php`
- `playresult.php`
- `selfbest.php`
- `crownsdata.php`
- `itempurchase.php`
- `rewardcardcheck.php`
- `rewardexecution.php`
- `balancecheck.php`
- `banacoinpayment.php`
- `banacoinerrorlog.php`
- `getbanacoininfo.php`
- `bookkeeping.php`
- `coinsetting.php`
- `headclerk2.php`
- `heartbeat.php`

## Code Excerpts

### Handler Dispatch Shape

From `GetTelopQuery.cs`:

```csharp
public ValueTask<CommonGetTelopResponse> Handle(GetTelopQuery request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    _ => throw new InvalidOperationException($"GetTelopQuery is not implemented for era: {request.Era}")
};
```

Phase 13 should add `GameEra.Yellow => HandleYellow(...)` and a `.Yellow.cs` partial.

### Controller Shape

From `GetFolderController` Green:

```csharp
var common = await Mediator.Send(
    new GetFolderQuery(GameEra.Green, request.FolderIds ?? []),
    HttpContext.RequestAborted);
return Ok(FolderDataMappers.Map(common));
```

Yellow should use the same shape with `GameEra.Yellow` and Yellow mappers.

## Boundary Guard Patterns

Use source tests to prove:

- Yellow catalog files do not reference `GameEra.Green` or `GameEra.Blue` except in test analog names or shared comparisons.
- Yellow adapter routes for deferred gameplay/persistence endpoints remain no-state.
- No `Yellow*` EF entities, migrations, `TaikoDbContext` `DbSet`, `SaveChanges`, `UpdatePlayResultCommand.Yellow`, or `ItemPurchaseCommand.Yellow` appears in Phase 13.
- `getreitai.php` and `battleuserdata.php` remain absent.

## PATTERN MAPPING COMPLETE
