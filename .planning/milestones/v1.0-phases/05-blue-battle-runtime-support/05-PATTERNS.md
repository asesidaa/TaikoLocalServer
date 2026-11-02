# Phase 05: Blue Battle Runtime Support - Pattern Map

**Mapped:** 2026-05-30
**Files analyzed:** 17 module families
**Analogs found:** 16 / 17

## File Classification

| New/Modified File Or Family | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `.planning/phases/05-blue-battle-runtime-support/05-RESOLUTION.md` or equivalent row-resolution artifact | planning artifact | batch/evidence-gated | `.planning/phases/05-blue-battle-runtime-support/05-RESEARCH.md` rows 93-117 | artifact-match |
| `Application/Dtos/CommonPlayResultData.BlueBattle.cs` | model / DTO | transform, event-driven | `Application/Dtos/CommonPlayResultData.Green.cs` | role-match, style-only |
| `Application/Dtos/CommonBattleUserDataResponse.cs` or equivalent | model / DTO | request-response | `Application/Dtos/CommonInitialDataCheckResponse.Blue.cs` | role-match |
| `Application/Handlers/GetBattleUserDataQuery.Blue.cs` | service / query | CRUD, request-response | `Application/Handlers/UserDataQuery.cs`, `Application/Handlers/UserDataQuery.Blue.cs` | role-match |
| `Application/Handlers/UpdatePlayResultCommand.Blue.cs` and/or `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs` | service / command | event-driven, CRUD | `Application/Handlers/UpdatePlayResultCommand.Blue.cs` | exact for normal path, branch point required |
| `Domain/Entities/*Battle*Blue.cs` | model | CRUD | `Domain/Entities/BlueShopItemState.cs`, `Domain/Entities/UserSaveDataBlue.cs` | role-match |
| `Application/Abstractions/ITaikoDbContext.Blue.cs` | db contract | CRUD | `Application/Abstractions/ITaikoDbContext.Blue.cs` | exact |
| `Infrastructure/Persistence/TaikoDbContext.Blue.cs` / `TaikoDbContext.BlueBattle.cs` | EF mapping | CRUD | `Infrastructure/Persistence/TaikoDbContext.Blue.cs` | exact |
| `Infrastructure/Persistence/Migrations/*AddBlueBattle*.cs` and model snapshot | migration | batch, CRUD | `Infrastructure/Persistence/Migrations/20260528181315_AddBlueItemShopState.cs` | role-match |
| `Application/Abstractions/IBlueCatalog.cs` | catalog contract | file-I/O | `Application/Abstractions/IBlueCatalog.cs` | exact |
| `Infrastructure/GameDataCatalog/Blue/BlueBattle*Loader.cs` | loader / utility | file-I/O, transform | `Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs` | role-match |
| `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs` | catalog service | file-I/O, batch init | `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs` | exact |
| `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs` | controller | request-response | `Adapters.GameProtocol.Blue/Controllers/UserDataController.cs` | role-match |
| `Adapters.GameProtocol.Blue/Mappers/BattleUserDataMappers.cs` | mapper | request-response, transform | `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs`, `UserDataMappers.cs` | role-match |
| `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs` / `GetInitialDataQuery.Blue.cs` | mapper / query | request-response | current same files | exact |
| `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` / `BattlePlayResultMappers.cs` | mapper | event-driven, transform | `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` | exact, currently omission-only for battle |
| `Tests/Blue/BlueBattle*Tests.cs` and route/source guards | test | CRUD, event-driven, request-response | `Tests/Blue/BlueHandlerFixture.cs`, `BluePlayResultHandlerTests.cs`, `BlueA6SourceGuardTests.cs` | role-match |

## Pattern Assignments

### Phase 5 Row-Resolution Artifact (planning artifact, batch/evidence-gated)

**Analog:** `.planning/phases/05-blue-battle-runtime-support/05-RESEARCH.md`

**Gate rule source** (`05-CONTEXT.md` lines 19-23):

```markdown
- **D-01:** Phase 5 must resolve every one of the 26 Phase 4 `MISSING_EVIDENCE` rows before runtime behavior relies on that row.
- **D-02:** Each row is resolved only by concrete proof or by a named user approval. No blanket approval and no broad assumption set is allowed.
- **D-03:** If any row remains unknown, Phase 5 planning blocks until that row is addressed. Do not work around unresolved rows with stub placeholders or partial runtime assumptions.
- **D-04:** Record row-by-row resolutions in a Phase 5 resolution matrix that cites the Phase 4 gate. Leave the completed Phase 4 gate artifact as history.
- **D-05:** Evidence standard: proto/generated wire can prove message shape; IDA/client/log/cabinet proof is authoritative for runtime mechanics; local XML remains candidate data unless proven consumed; the Wiki page is accepted for gameplay semantics and likely proto-field intent.
```

**Matrix pattern:** Copy the row/status/evidence/plan-implication shape from `05-RESEARCH.md` lines 93-117. Keep unresolved rows explicit; do not convert `STILL_MISSING_*`, `NEEDS_USER_APPROVAL`, or `DEFER_RUNTIME_USE` into implementation defaults.

**Apply to:** Any plan that emits battleuserdata fields, sets `is_battleplay`, persists derived battle progression, grants rewards/tokens, computes stage assignment, or mirrors releases into normal unlocks.

---

### `Domain/Entities/*Battle*Blue.cs` (model, CRUD)

**Analog:** `Domain/Entities/BlueShopItemState.cs`

**Entity pattern** (lines 5-25):

```csharp
public sealed class BlueShopItemState
{
    public uint Baid { get; set; }
    public uint SeasonId { get; set; }
    public uint ItemType { get; set; }
    public uint ItemId { get; set; }
    public uint ItemNo { get; set; }
    public uint ItemPrice { get; set; }
    public BlueShopItemStatus Status { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public UserDatum? Ba { get; set; }
}
```

**Blue save root pattern:** `Domain/Entities/UserSaveDataBlue.cs` lines 3-65 is the current Blue-owned root entity. Add battle roots as separate Blue-owned entities instead of adding Green AI Battle fields or normal score fields.

**Planner note:** Use explicit columns for raw client-reported battle values. For unresolved byte arrays or row counts, nullable columns or absent rows are safer than seeded defaults. Do not derive first stage, boss life, token semantics, or stage `33` behavior from XML.

---

### `ITaikoDbContext.Blue.cs` and `TaikoDbContext.Blue*.cs` (db contract / EF mapping, CRUD)

**Analogs:** `Application/Abstractions/ITaikoDbContext.Blue.cs`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`

**DbSet contract pattern** (`ITaikoDbContext.Blue.cs` lines 3-13):

```csharp
public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataBlue> UserSaveDataBlue { get; }
    DbSet<SongBestDatumBlue> SongBestDataBlue { get; }
    DbSet<SongPlayDatumBlue> SongPlayDataBlue { get; }
    DbSet<BlueFavoriteSongs> BlueFavoriteSongs { get; }
    DbSet<BlueRecentSongs> BlueRecentSongs { get; }
    DbSet<DanScoreDatumBlue> DanScoreDataBlue { get; }
    DbSet<DanStageScoreDatumBlue> DanStageScoreDataBlue { get; }
    DbSet<BlueShopSeasonState> BlueShopSeasonStates { get; }
    DbSet<BlueShopItemState> BlueShopItemStates { get; }
}
```

**DbContext registration pattern** (`TaikoDbContext.Blue.cs` lines 9-17):

```csharp
public virtual DbSet<UserSaveDataBlue> UserSaveDataBlue { get; set; } = null!;
public virtual DbSet<SongBestDatumBlue> SongBestDataBlue { get; set; } = null!;
public virtual DbSet<SongPlayDatumBlue> SongPlayDataBlue { get; set; } = null!;
public virtual DbSet<BlueFavoriteSongs> BlueFavoriteSongs { get; set; } = null!;
public virtual DbSet<BlueRecentSongs> BlueRecentSongs { get; set; } = null!;
public virtual DbSet<DanScoreDatumBlue> DanScoreDataBlue { get; set; } = null!;
public virtual DbSet<DanStageScoreDatumBlue> DanStageScoreDataBlue { get; set; } = null!;
public virtual DbSet<BlueShopSeasonState> BlueShopSeasonStates { get; set; } = null!;
public virtual DbSet<BlueShopItemState> BlueShopItemStates { get; set; } = null!;
```

**Mapping pattern** (`TaikoDbContext.Blue.cs` lines 126-138):

```csharp
modelBuilder.Entity<BlueShopItemState>(entity =>
{
    entity.ToTable("BlueShopItemStates");
    entity.HasKey(e => new { e.Baid, e.SeasonId, e.ItemType, e.ItemId });
    entity.Property(e => e.Status).HasConversion<uint>();
    entity.Property(e => e.PurchasedAt).HasColumnType("datetime");
    entity.Property(e => e.UnlockedAt).HasColumnType("datetime");
    entity.HasOne(d => d.Ba)
        .WithMany()
        .HasPrincipalKey(p => p.Baid)
        .HasForeignKey(d => d.Baid)
        .OnDelete(DeleteBehavior.Cascade);
});
```

**Partial hook pattern:** `Infrastructure/Persistence/TaikoDbContext.cs` lines 41-54 calls `OnModelCreatingBlue(modelBuilder)`. Because `OnModelCreatingBlue` already has an implementation, either add battle mappings in `TaikoDbContext.Blue.cs` or add a new helper call from that method to a new `TaikoDbContext.BlueBattle.cs`.

---

### `Infrastructure/Persistence/Migrations/*AddBlueBattle*.cs` (migration, batch CRUD)

**Analog:** `Infrastructure/Persistence/Migrations/20260528181315_AddBlueItemShopState.cs`

**Create table pattern** (lines 14-37):

```csharp
migrationBuilder.CreateTable(
    name: "BlueShopItemStates",
    columns: table => new
    {
        Baid = table.Column<uint>(type: "INTEGER", nullable: false),
        SeasonId = table.Column<uint>(type: "INTEGER", nullable: false),
        ItemType = table.Column<uint>(type: "INTEGER", nullable: false),
        ItemId = table.Column<uint>(type: "INTEGER", nullable: false),
        ItemNo = table.Column<uint>(type: "INTEGER", nullable: false),
        ItemPrice = table.Column<uint>(type: "INTEGER", nullable: false),
        Status = table.Column<uint>(type: "INTEGER", nullable: false),
        PurchasedAt = table.Column<DateTime>(type: "datetime", nullable: false),
        UnlockedAt = table.Column<DateTime>(type: "datetime", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_BlueShopItemStates", x => new { x.Baid, x.SeasonId, x.ItemType, x.ItemId });
        table.ForeignKey(
            name: "FK_BlueShopItemStates_UserData_Baid",
            column: x => x.Baid,
            principalTable: "UserData",
            principalColumn: "Baid",
            onDelete: ReferentialAction.Cascade);
    });
```

**Down pattern** (lines 63-69):

```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(
        name: "BlueShopItemStates");

    migrationBuilder.DropTable(
        name: "BlueShopSeasonStates");
}
```

**Planner note:** Keep battle table names Blue-specific, e.g. `BlueBattle...`. Do not reuse `AiScoreDatum`, `GreenGhost...`, `SongPlayDatum_Blue`, `SongBestDatum_Blue`, `DanScoreDatum_Blue`, `BlueRecentSongs`, or `BlueFavoriteSongs` for battle-owned state.

---

### `CommonBattleUserDataResponse` / battle DTOs (model / DTO, request-response)

**Analogs:** `Application/Dtos/CommonInitialDataCheckResponse.Blue.cs`, `Application/Dtos/CommonPlayResultData.Green.cs`

**Optional DTO pattern** (`CommonInitialDataCheckResponse.Blue.cs` lines 3-11):

```csharp
public partial class CommonInitialDataCheckResponse
{
    public bool? IsBattleplay { get; set; }

    public byte[]? ReleaseBattleStageFlg { get; set; }

    public byte[]? ReleaseBattleSpecialFlg { get; set; }

    public uint? BattleBondsLvCap { get; set; }
```

**Nested partial DTO style only** (`CommonPlayResultData.Green.cs` lines 29-38):

```csharp
public partial class StageData
{
    public uint? WaiwaiResult { get; set; }
    public uint? WaiwaiGauge  { get; set; }
    public uint? SoulGauge    { get; set; }
    public uint? HitCount     { get; set; }
    public uint? PlayDan      { get; set; }
    public GhostStageData? GhostStageData { get; set; }
    public bool IsPushed { get; set; }
}
```

**Planner note:** Copy the partial/nested DTO style, not Green AI Battle semantics. For `BattleStageData` and `ReleaseBattleData`, add Blue battle-specific DTOs/partials so normal `StageData` remains usable for normal Blue without hidden side effects.

---

### `GetBattleUserDataQuery.Blue.cs` (service / query, CRUD request-response)

**Analogs:** `Application/Handlers/UserDataQuery.cs`, `Application/Handlers/UserDataQuery.Blue.cs`

**Mediator query pattern** (`UserDataQuery.cs` lines 6-24):

```csharp
public readonly record struct UserDataQuery(uint Baid, GameEra Era) : IRequest<CommonUserDataResponse>;

public partial class UserDataQueryHandler(ITaikoDbContext context, IGameDataCatalog gameDataService, ILogger<UserDataQueryHandler> logger, IOptions<ServerSettings> settings) 
    : IRequestHandler<UserDataQuery, CommonUserDataResponse>
{
    public ValueTask<CommonUserDataResponse> Handle(UserDataQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };
```

**Blue query read pattern** (`UserDataQuery.Blue.cs` lines 9-21):

```csharp
_ = await context.UserData.FindAsync([request.Baid], cancellationToken)
    ?? throw new InvalidOperationException($"User not found for Blue baid {request.Baid}.");
var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
var blue = gameDataService.Blue();
var activeShopSeason = blue.ItemShopCatalog.ActiveSeason;
var unlockedShopItems = activeShopSeason is null
    ? new HashSet<(uint ItemType, uint ItemId)>()
    : await context.BlueShopItemStates
        .Where(row => row.Baid == request.Baid
            && row.SeasonId == activeShopSeason.SeasonId
            && row.Status == BlueShopItemStatus.Unlocked)
```

**Default creation pattern:** `Application/Common/UserSaveDataBlueExtensions.cs` lines 5-18 uses `FindAsync`, returns existing, creates default, adds to context, and lets caller save. Use the same pattern only for values with approved defaults.

---

### `UpdatePlayResultCommand.BlueBattle.cs` (service / command, event-driven CRUD)

**Analog:** `Application/Handlers/UpdatePlayResultCommand.Blue.cs`

**Current normal Blue write surface** (lines 81-98):

```csharp
ApplyUnlockBits(saveData, playResultData);

foreach (var stage in playResultData.AryStageInfoes)
{
    if (!IsSupportedBlueStage(request.Baid, stage))
    {
        continue;
    }

    BlueProfileCounters.ApplyStage(saveData, stage);
    await SaveBlueStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
}

await SaveBlueDanAsync(saveData, playResultData, blue, cancellationToken);

await context.SaveChangesAsync(cancellationToken);
await TrimBlueRecentSongsAsync(request.Baid, cancellationToken);
return 1;
```

**Normal table write methods to bypass for battle:**

- `SaveBlueStageAsync(...)` starts at `UpdatePlayResultCommand.Blue.cs` line 154.
- Normal play rows are added at lines 165-196.
- Normal best upsert is called at lines 198-200 and implemented at lines 206-240.
- Recent/favorite upsert is called at line 203 and implemented from line 242.
- Dani save starts at line 305 and writes `DanScoreDataBlue` / `DanStageScoreDataBlue` from lines 343-361.

**Planner instruction:** Branch before `ApplyUnlockBits` and before the `foreach` normal-stage loop when D-10 is true:

```csharp
var isBattlePlayResult = playResultData.BattleReleaseData is not null
    || playResultData.AryStageInfoes.Any(stage => stage.BattleStageData is not null);
```

Then persist only Blue battle-owned raw/resolved state. Do not call `BlueProfileCounters.ApplyStage`, `SaveBlueStageAsync`, `UpsertBlueFavoriteAndRecentAsync`, `UpsertBestAsync`, or `SaveBlueDanAsync` for battle-classified payloads.

---

### `BattleUserDataController.cs` and `BattleUserDataMappers.cs` (controller / mapper, request-response)

**Analogs:** `BattleUserDataController.cs`, `UserDataController.cs`, `InitialDataMappers.cs`, generated wire.

**Current stub route to replace after row resolution** (`BattleUserDataController.cs` lines 3-13):

```csharp
[ApiController]
[Route("/v10r03/chassis/battleuserdata.php")]
public class BattleUserDataController : BaseProtocolController<BattleUserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BattleUserData([FromBody] BattleUserDataRequest request)
    {
        Logger.LogInformation("Blue BattleUserData request: {Request}", request.Stringify());
        return Ok(new BattleUserDataResponse { Result = 1 });
    }
}
```

**Mediator controller pattern** (`UserDataController.cs` lines 9-13):

```csharp
public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
{
    Logger.LogInformation("Blue UserData request: {Request}", request.Stringify());
    var common = await Mediator.Send(new UserDataQuery(request.Baid, GameEra.Blue), HttpContext.RequestAborted);
    return Ok(UserDataMappers.Map(common));
}
```

**Mapper optional-emission pattern** (`InitialDataMappers.cs` lines 29-47):

```csharp
if (common.IsBattleplay is { } isBattleplay)
{
    response.IsBattleplay = isBattleplay;
}

if (common.ReleaseBattleStageFlg is not null)
{
    response.ReleaseBattleStageFlg = common.ReleaseBattleStageFlg;
}

if (common.ReleaseBattleSpecialFlg is not null)
{
    response.ReleaseBattleSpecialFlg = common.ReleaseBattleSpecialFlg;
}

if (common.BattleBondsLvCap is { } battleBondsLvCap)
{
    response.BattleBondsLvCap = battleBondsLvCap;
}
```

**Generated battleuserdata shape** (`proto/blue/taiko.proto` lines 705-733) and generated presence helpers (`Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3346-3479) define the only safe wire fields. Use `ShouldSerialize*` tests to prove omission. Do not fill `ReleaseInfoFlg`, `ReleaseBattleStageFlg`, `LastBattleStageId`, `LastBossLife`, `LastNpcId`, `NpcDatas`, `AryTokenDatas`, `AssignStageId`, nested `NpcCostumeFlg`, or `ReleaseSpecialFlg` unless the row-resolution artifact clears that exact row.

---

### `InitialDataMappers.cs` / `GetInitialDataQuery.Blue.cs` (mapper / query, request-response)

**Analogs:** current same files.

**Blue initialdata query pattern** (`GetInitialDataQuery.Blue.cs` lines 21-66):

```csharp
return ValueTask.FromResult(new CommonInitialDataCheckResponse
{
    Result = 1,
    DefaultSongFlg = BlueProtocolBytes.CreateFixedBitset(allSongs, BlueProtocolBytes.SongFlagBytes),
    AchievementSongBit = new byte[BlueProtocolBytes.SongFlagBytes],
    UraReleaseBit = new byte[BlueProtocolBytes.SongFlagBytes],
    SongHashVer = blue.SongHashVersion,
    IsDanplay = true,
    IsClose = false,
    IsItemshop = activeShopWithRows is not null,
    AryBlueItemShopDatas = activeShopWithRows is null
        ? []
        :
        [
            new CommonInitialDataCheckResponse.InformationData
            {
                InfoId = activeShopWithRows.SeasonId,
                VerupNo = activeShopWithRows.VerupNo
            }
        ],
```

**Presence helper source** (`Adapters.GameProtocol.Blue/Wire/Game.cs` lines 423-460):

```csharp
[global::ProtoBuf.ProtoMember(14, Name = @"is_battleplay")]
public bool IsBattleplay
{
    get => __pbn__IsBattleplay.GetValueOrDefault();
    set => __pbn__IsBattleplay = value;
}
public bool ShouldSerializeIsBattleplay() => __pbn__IsBattleplay != null;
...
public bool ShouldSerializeReleaseBattleStageFlg() => __pbn__ReleaseBattleStageFlg != null;
...
public bool ShouldSerializeReleaseBattleSpecialFlg() => __pbn__ReleaseBattleSpecialFlg != null;
...
public bool ShouldSerializeBattleBondsLvCap() => __pbn__BattleBondsLvCap != null;
```

**Existing omission test** (`Tests/Blue/BlueInitialDataTests.cs` lines 30-33):

```csharp
Assert.False(wire.ShouldSerializeIsBattleplay());
Assert.False(wire.ShouldSerializeReleaseBattleStageFlg());
Assert.False(wire.ShouldSerializeReleaseBattleSpecialFlg());
Assert.False(wire.ShouldSerializeBattleBondsLvCap());
```

**Planner instruction:** `is_battleplay` stays omitted/false until D-07 is satisfied. IDA proves 8-byte initial stage and 16-byte initial special copy behavior, but defaults are still gated.

---

### `PlayResultMappers.cs` / `BattlePlayResultMappers.cs` (mapper, event-driven transform)

**Analogs:** `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`, `proto/blue/taiko.proto`

**Current mapper pattern** (`PlayResultMappers.cs` lines 8-56):

```csharp
public static CommonPlayResultData Map(PlayResultRequest request)
{
    return new CommonPlayResultData
    {
        Baid = request.Baid,
        ChassisId = request.ChassisId,
        ShopId = request.ShopId,
        PlayDatetime = request.PlayDatetime,
        IsRight = request.IsRight,
        CardType = request.CardType,
        IsTwoPlayers = request.IsTwoPlayers,
        AryStageInfoes = request.AryStageInfoes.Select(MapStage).ToList(),
        ReleaseSongNoes = (request.ReleaseSongNoes ?? []).ToList(),
```

**Current stage mapping pattern** (`PlayResultMappers.cs` lines 64-94):

```csharp
private static CommonPlayResultData.StageData MapStage(PlayResultRequest.StageData stage)
{
    return new CommonPlayResultData.StageData
    {
        SongNo = stage.SongNo,
        Level = stage.Level,
        PlayResult = stage.PlayResult,
        PlayScore = stage.PlayScore,
        GoodCnt = stage.GoodCnt,
        OkCnt = stage.OkCnt,
        NgCnt = stage.NgCnt,
        PoundCnt = stage.PoundCnt,
        ComboCnt = stage.ComboCnt,
        HitCnt = stage.HitCnt,
        OptionFlg = stage.OptionFlg ?? [],
        ToneFlg = stage.ToneFlg ?? [],
```

**Battle schema anchors:**

- `PlayResultRequest.StageData.BattleStageData` is in `proto/blue/taiko.proto` lines 365-388.
- `PlayResultRequest.ReleaseBattleData` is in `proto/blue/taiko.proto` lines 456-470.
- Generated `AryReleaseBattledata` is in `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 2050-2051.
- Generated `AryBattlestagedata` and nested battle types are in `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 2194-2279.

**Existing omission/fixture test** (`Tests/Blue/BluePlayResultMapperTests.cs` lines 61-84 and 147-170):

```csharp
request.AryReleaseBattledata = new PlayResultRequest.ReleaseBattleData
{
    AssignNextStageId = 2
};
request.AryStageInfoes.Add(CreateStage(101, 1, 0, includeBattle: true));

var common = PlayResultMappers.Map(request);

Assert.Equal(1u, common.Baid);
Assert.Single(common.AryStageInfoes);
Assert.Equal(101u, common.AryStageInfoes[0].SongNo);
```

**Planner instruction:** Replace omission-only behavior with explicit Blue battle DTO mapping only after branch shape is defined. Trigger battle branch by `request.AryReleaseBattledata is not null` or any `stage.AryBattlestagedata is not null`; do not use Green `StageMode` rules as Blue truth.

---

### `Infrastructure/GameDataCatalog/Blue/BlueBattle*Loader.cs` (loader / utility, file-I/O transform)

**Analogs:** `BlueGameDataPaths.cs`, `BlueItemShopLoader.cs`, `Ac15ItemShopLoader.cs`, `Ac15EventFolderLoader.cs`, `BlueEraGameDataCatalog.cs`

**Path pattern** (`BlueGameDataPaths.cs` lines 7-20):

```csharp
private const string ConfigDirectory = "S10100-1";

public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Blue), "data");

public static string ConfigRoot => Path.Combine(GameDataRoot, "config", ConfigDirectory);

public static string MusicInfoXml => Path.Combine(ConfigRoot, "musicinfo.xml");
```

Add `BattleRoot => Path.Combine(ConfigRoot, "battle")` and file-specific paths only after the plan resolves loader scope.

**Era wrapper loader pattern** (`BlueItemShopLoader.cs` lines 13-25):

```csharp
var path = Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName);
var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
    path,
    blueSettings.EnableShop == true,
    blueSettings.ActiveShopSeasonId,
    nameof(GameEra.Blue),
    cancellationToken);

return Map(catalog);
```

**Fail-fast enabled loader pattern** (`Ac15ItemShopLoader.cs` lines 16-75): disabled returns a disabled catalog, missing enabled file throws `InvalidDataException`, duplicate/invalid rows throw with era and row context.

**Optional missing-file pattern** (`Ac15EventFolderLoader.cs` lines 24-39):

```csharp
if (!File.Exists(path))
{
    return new Dictionary<uint, EventFolderData>();
}

RawEventFolder[] raw;
try
{
    await using var stream = File.OpenRead(path);
    raw = await JsonSerializer.DeserializeAsync<RawEventFolder[]>(stream, JsonOptions, cancellationToken)
          ?? [];
}
catch (JsonException ex)
{
    throw new InvalidDataException($"{eraName} event folder data is malformed: {path}", ex);
}
```

**Catalog init pattern** (`BlueEraGameDataCatalog.cs` lines 70-128): call `BlueRequiredDataFiles.ThrowIfMissing()`, then load each Blue-owned file once into immutable/list/dictionary catalog state.

**Planner instruction:** IDA proves all five battle XML files are consumed, but field semantics are not proven. Loader plans may parse and expose rows as local inputs; they must not infer first-stage, next-stage, last-stage, boss-life, stage `33`, reward type, or token effects without row-level resolution.

---

### `Application/Abstractions/IBlueCatalog.cs` (catalog contract, file-I/O)

**Analog:** current same file.

**Contract pattern** (`IBlueCatalog.cs` lines 5-29):

```csharp
public interface IBlueCatalog : IEraGameDataCatalog
{
    IReadOnlyList<BlueMusicInfoEntry> MusicInfoFileOrder { get; }
    uint SongHashVersion { get; }
    IReadOnlyDictionary<uint, BlueMusicInfoEntry> BlueMusicInfos { get; }
    IReadOnlyList<BlueTaikojukuEntry> TaikojukuFileOrder { get; }
    IReadOnlyDictionary<uint, BlueTaikojukuEntry> Taikojuku { get; }
    BlueItemShopCatalog ItemShopCatalog { get; }
    IReadOnlyDictionary<uint, BlueItemShopEntry> ItemShop { get; }
    IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }
```

**DI/catalog registration pattern:** `Infrastructure/DependencyInjection.cs` lines 75-83 registers `BlueEraGameDataCatalog` as `IBlueCatalog` and `IEraGameDataCatalog`, then builds `FileGameDataCatalog`. Extend the Blue catalog object; do not add a parallel Green-derived catalog.

---

### `Tests/Blue/BlueBattle*Tests.cs` (tests, CRUD/event-driven/request-response)

**Analogs:** `BlueHandlerFixture.cs`, `BlueItemShopStateTests.cs`, `BluePlayResultHandlerTests.cs`, `BlueInitialDataTests.cs`, `BlueRouteSkeletonTests.cs`

**In-memory EF fixture pattern** (`BlueHandlerFixture.cs` lines 22-34):

```csharp
public static async Task<BlueHandlerFixture> CreateAsync(IBlueCatalog? blueCatalog = null)
{
    var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<TaikoDbContext>()
        .UseSqlite(connection)
        .Options;
    var context = new TaikoDbContext(options);
    await context.Database.EnsureCreatedAsync();

    var catalog = new FileGameDataCatalog([blueCatalog ?? new TestBlueCatalog()]);
    return new BlueHandlerFixture(connection, context, catalog);
}
```

**Persistence first-touch pattern** (`BlueItemShopStateTests.cs` lines 7-24): create user/save data, call extension, save, assert Blue-owned row defaults.

**Normal-state protection analogs:**

- `BluePlayResultHandlerTests.cs` lines 14-27 asserts guest Blue playresult returns success and leaves `SongPlayDataBlue` / `SongBestDataBlue` empty.
- Lines 196-198 assert unsupported stages leave normal Blue play/best tables empty.
- Lines 225-227 assert normal favorite/recent use only Blue tables.
- Lines 258-272 assert Dani writes Dan rows but keeps normal best null.

**Route skeleton pattern** (`BlueRouteSkeletonTests.cs` lines 5-31): update expected route inventory only when BattleUserData becomes mediator-backed; lines 61-90 enforce mediator calls only in implemented endpoints.

**Validation commands:** Use `05-VALIDATION.md` lines 35-38 task filters for focused Phase 5 tests and lines 26-29 for sampling cadence.

---

### `Tests/Blue/BlueBattleSourceGuardTests.cs` (source guard, cross-cutting)

**Analogs:** `BlueA4SourceGuardTests.cs`, `BlueA5SourceGuardTests.cs`, `BlueA6SourceGuardTests.cs`

**Forbidden reference pattern** (`BlueA6SourceGuardTests.cs` lines 5-14):

```csharp
private static readonly string[] ForbiddenGreenShopReferences =
[
    "GreenShop",
    "GreenShopItemStatus",
    "GreenProtocolBytes",
    "Adapters.GameProtocol.Green",
    "UserSaveDataGreen",
    "GreenShopSeasonStates",
    "GreenShopItemStates"
];
```

**File scan pattern** (`BlueA6SourceGuardTests.cs` lines 101-110):

```csharp
private static void AssertFilesDoNotContainForbiddenReferences(IEnumerable<string> files)
{
    foreach (var file in files)
    {
        var source = File.ReadAllText(file);
        foreach (var forbidden in ForbiddenGreenShopReferences)
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
        }
    }
}
```

**Existing Blue protocol guard** (`BlueA4SourceGuardTests.cs` lines 20-31): rejects `GreenProtocolBytes`, `Adapters.GameProtocol.Green`, Green song/save/shop/ghost/Dan symbols in Blue playresult code.

**Planner instruction:** Add a Phase 5 guard with forbidden strings such as `GreenStageModeInterpreter`, `GreenAiBattleLevels`, `GreenGhost`, `GhostStageData`, `AiBattle`, `SongPlayDatumBlue` writes from battle branch, and XML-default phrases like "row count implies" if implementation comments start making unsafe claims.

## Shared Patterns

### Blue-Owned State Separation

**Source:** `05-CONTEXT.md` lines 104-114

**Apply to:** entities, DbSets, migrations, handlers, mappers, tests

Blue battle state belongs in Blue-owned controllers, mappers, DTO partials, handler partials, entities, DbSets, EF mappings, migrations, tests, and source guards. Battle playresult branch must bypass normal Blue score, crown, history, recent/favorite, profile counter, self-best, and Dani updates.

### Optional Protobuf Presence

**Source:** `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 423-460 and 3346-3479

**Apply to:** initialdata and battleuserdata mappers/tests

Set optional wire fields only when the application DTO has a resolved value. Assert `ShouldSerialize*()` false for unresolved fields. Do not write zero-filled defaults to prove "empty" unless the row-resolution artifact approves exact width/default.

### Controller / Mediator Pattern

**Source:** `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs` lines 3-11 and `UserDataController.cs` lines 9-13

**Apply to:** `BattleUserDataController` once it stops being a stub

Controllers log `request.Stringify()`, send a mediator request through `HttpContext.RequestAborted`, then map common DTO to Blue wire response.

### Catalog Loader Error Handling

**Source:** `Ac15ItemShopLoader.cs` lines 16-75 and `Ac15EventFolderLoader.cs` lines 24-39

**Apply to:** battle XML loaders

Use file paths and row context in `InvalidDataException`. Optional inputs may return empty only when the feature is explicitly optional; enabled/required inputs fail fast. Do not translate XML rows into runtime semantics without proof.

### Test Fixture And Source Guards

**Source:** `BlueHandlerFixture.cs` lines 22-34, `BlueA6SourceGuardTests.cs` lines 101-110

**Apply to:** all `BlueBattle*Tests.cs`

Use in-memory SQLite for EF behavior, `FileGameDataCatalog` with a `TestBlueCatalog`, and source guards over concrete files. Focused command from validation: `dotnet test Tests/Tests.csproj --filter BlueBattle`.

## IDA Evidence Anchors For Planning

**Source:** `.planning/phases/05-blue-battle-runtime-support/05-RESEARCH.md` lines 120-149

| Current Symbol | Address | Planning Use |
|---|---:|---|
| `sub_143720` | `0x143720` | Initialdata response handler consumes optional battle fields and calls `sub_13BFC8`; supports mapper/presence tests, not defaults. |
| `sub_13BFC8` | `0x13BFC8` | Initialdata battle stage copy up to 8 bytes and battle special copy up to 16 bytes; defaults still not proved. |
| `sub_250F04` | `0x250F04` | Battle menu availability candidate uses stored `is_battleplay` plus additional state checks. |
| `sub_2DF364` | `0x2DF364` | `battleuserdata.php` is in the user-state route group after `itempurchase.php`. |
| `sub_2E0CA8` | `0x2E0CA8` | Direct-protobuf `BattleUserDataRequest` request pipeline; does not prove response defaults. |
| `sub_2DB31C` / `sub_2DE860` | `0x2DB31C` / `0x2DE860` | Direct-protobuf `playresult.php` route setup/check. |
| `sub_2E3A60` | `0x2E3A60` | `initialdatacheck.php` startup route group; `battleuserdata.php` is not in this group. |
| `sub_12F44` | `0x12F44` | Loads `battleadjsetting.xml`, `battlenpcinfo.xml`, `battlestageinfo.xml`, `battletokeninfo.xml`; field semantics unresolved. |
| `sub_796C24` | `0x796C24` | Loads `battlesupportinfo.xml`; field semantics unresolved. |
| `sub_38410` / `sub_385AC` | `0x38410` / `0x385AC` | Selected NPC/support level write helpers; useful for persisted observed values, not startup defaults. |
| stage `33` asset string | string xref only | Asset exists; menu/progression role not proved. |

Do not mutate the IDB during Phase 5 planning. These anchors are planning/evidence references only.

## No Analog Found

| File / Behavior | Role | Data Flow | Reason |
|---|---|---|---|
| Phase 5 row-resolution artifact exact filename | planning artifact | batch/evidence-gated | No existing Phase 5 artifact yet; use the `05-RESEARCH.md` matrix shape and `05-CONTEXT.md` gate rules. |
| Blue battle progression/reward runtime semantics | service | event-driven | Existing code has no Blue battle implementation, and Green AI Battle is contrast/source-guard material only. Runtime effects remain gated. |
| Battle XML semantic defaults | loader/service | file-I/O, transform | Loaders exist for other Blue/AC15 data, but no analog proves battle first-stage, next-stage, boss-life, token, reward type, or stage `33` semantics. |

## Metadata

**Analog search scope:** `Application/`, `Adapters.GameProtocol.Blue/`, `Adapters.GameProtocol.Shared/`, `Infrastructure/`, `Domain/`, `Tests/Blue/`, `Tests/Green/`, `proto/blue/`, Phase 5 artifacts.
**Files scanned:** local `rg` over C# / proto / phase markdown plus targeted line-number reads of strong analogs.
**Pattern extraction date:** 2026-05-30
