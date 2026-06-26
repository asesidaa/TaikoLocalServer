# Phase 41: MOMOIRO Identity, Userdata, Self-Best, and Crown Readback - Pattern Map

**Mapped:** 2026-06-26
**Files analyzed:** 31
**Analogs found:** 30 / 31

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `Domain/Entities/UserSaveDataMomoiro.cs` | model | CRUD/readback | `Domain/Entities/UserSaveDataKimidori.cs` | exact, reduce unsupported fields only with care |
| `Domain/Entities/SongBestDatumMomoiro.cs` | model | CRUD/readback | `Domain/Entities/SongBestDatumKimidori.cs` | exact |
| `Domain/Entities/MomoiroFavoriteSongs.cs` | model | CRUD/readback | `Domain/Entities/KimidoriFavoriteSongs.cs` | exact |
| `Domain/Entities/MomoiroRecentSongs.cs` | model | CRUD/readback | `Domain/Entities/KimidoriRecentSongs.cs` | exact |
| `Application/Common/UserSaveDataMomoiroExtensions.cs` | utility | CRUD/defaults | `Application/Common/UserSaveDataKimidoriExtensions.cs` | exact |
| `Application/Abstractions/ITaikoDbContext.Momoiro.cs` | port/interface | CRUD | `Application/Abstractions/ITaikoDbContext.Kimidori.cs` | exact, omit Dan/play rows unless scoped |
| `Infrastructure/Persistence/TaikoDbContext.cs` | config | EF model hook | existing Kimidori/Murasaki partial hook calls | exact |
| `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs` | config/model | EF mapping | `Infrastructure/Persistence/TaikoDbContext.Kimidori.cs` | exact, omit unsupported tables |
| `Infrastructure/Persistence/Migrations/*_AddMomoiroReadbackState.cs` | migration | schema CRUD | `20260623155010_AddKimidoriRuntimeState.cs` | role-match, trim Dan/playresult tables |
| `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` | migration snapshot | schema metadata | generated EF snapshot | generated |
| `Application/Handlers/BaidQuery.cs` | handler dispatcher | request-response | Kimidori dispatch arm in same file | exact |
| `Application/Handlers/BaidQuery.Momoiro.cs` | handler | request-response | `Application/Handlers/BaidQuery.Kimidori.cs` | exact |
| `Application/Handlers/AddMyDonEntryCommand.cs` | handler dispatcher | request-response | Kimidori dispatch arm in same file | exact |
| `Application/Handlers/AddMyDonEntryCommand.Momoiro.cs` | handler | request-response/CRUD | `Application/Handlers/AddMyDonEntryCommand.Kimidori.cs` | exact |
| `Application/Handlers/UserDataQuery.cs` | handler dispatcher | request-response | Kimidori dispatch arm in same file | exact |
| `Application/Handlers/UserDataQuery.Momoiro.cs` | handler | request-response/readback | `Application/Handlers/UserDataQuery.Kimidori.cs` | exact, add crown readback |
| `Application/Handlers/GetSelfBestQuery.cs` | handler dispatcher | request-response | Kimidori dispatch arm in same file | exact |
| `Application/Handlers/GetSelfBestQuery.Momoiro.cs` | handler | request-response/readback | `Application/Handlers/GetSelfBestQuery.Kimidori.cs` | exact |
| `Application/Ac15/MomoiroAc15UserDataAdapter.cs` | adapter/service | transform | `Application/Ac15/KimidoriAc15UserDataAdapter.cs` | exact |
| `Application/Ac15/Ac15UserDataRecords.cs` | DTO/model | transform | existing `Ac15UserDataSnapshot` | role-match, add optional crown payload |
| `Application/Ac15/Ac15UserDataService.cs` | service | transform | existing release/favorites response builder | role-match, preserve existing callers |
| `Adapters.GameProtocol.Momoiro/Mappers/BaidResponseMapper.cs` | mapper | transform | `Adapters.GameProtocol.Kimidori/Mappers/BaidResponseMapper.cs` | exact with Momoiro wire |
| `Adapters.GameProtocol.Momoiro/Mappers/UserDataMappers.cs` | mapper | transform | `Adapters.GameProtocol.Kimidori/Mappers/UserDataMappers.cs` | role-match, add `HashCrownFlg` mapping |
| `Adapters.GameProtocol.Momoiro/Mappers/SelfBestMappers.cs` | mapper | transform | `Adapters.GameProtocol.Kimidori/Mappers/SelfBestMappers.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Controllers/BaidController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/BaidController.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Controllers/MyDonEntryController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/MyDonEntryController.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Controllers/UserDataController.cs` | controller | request-response/readback | `Adapters.GameProtocol.Kimidori/Controllers/UserDataController.cs` | role-match, user-data-owned crown |
| `Adapters.GameProtocol.Momoiro/Controllers/SelfBestController.cs` | controller | request-response/readback | `Adapters.GameProtocol.Kimidori/Controllers/SelfBestController.cs` | exact |
| `Tests/Momoiro/MomoiroHandlerFixture.cs` | test fixture | CRUD/request-response | `Tests/Murasaki/MurasakiHandlerFixture.cs`, `Tests/Kimidori/KimidoriHandlerFixture.cs` | role-match |
| `Tests/Momoiro/MomoiroRuntimeHandlerTests.cs` | test | request-response/CRUD | `Tests/Murasaki/MurasakiRuntimeHandlerTests.cs` | exact behavior shape |
| `Tests/Momoiro/MomoiroControllerReadbackTests.cs` or extend `MomoiroMetadataRouteTests.cs` | test | request-response | `Tests/Momoiro/MomoiroMetadataRouteTests.cs` | role-match |
| `Tests/Ac15/Ac15UserDataServiceTests.cs` | test | transform | existing release/list/counter tests | role-match if shared crown DTO changes |
| `Tests/Ac15/Ac15CrownServiceTests.cs` / `Ac15SongHashCodecTests.cs` | test | transform/bit packing | existing crown/hash tests | exact, add Momoiro-specific compact length if needed |
| `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` | test | route discovery | existing Momoiro route-surface guard | exact guard, keep crown route absent |

## Pattern Assignments

### `Domain/Entities/UserSaveDataMomoiro.cs` (model, CRUD/readback)

**Analog:** `Domain/Entities/UserSaveDataKimidori.cs`

**Entity shape** (lines 3-8):
```csharp
public partial class UserSaveDataKimidori :
    IAc15DonPointSaveData,
    IAc15PlayTutorialSaveData,
    IAc15ProfileSettingsSaveData,
    IAc15SongUnlockSaveData
```

**Readback fields to copy for Phase 41** (lines 9-68):
```csharp
public uint Baid { get; set; }
public string Title { get; set; } = string.Empty;
public uint TitleplateId { get; set; }
public uint ColorBody { get; set; }
public uint ColorFace { get; set; }
public uint ColorLimb { get; set; }
public uint Costume1 { get; set; }
public uint Costume2 { get; set; }
public uint Costume3 { get; set; }
public uint Costume4 { get; set; }
public uint Costume5 { get; set; }
public byte[] CostumeFlg1 { get; set; } = [];
public byte[] CostumeFlg2 { get; set; } = [];
public byte[] CostumeFlg3 { get; set; } = [];
public byte[] CostumeFlg4 { get; set; } = [];
public byte[] CostumeFlg5 { get; set; } = [];
public byte[] ToneFlg { get; set; } = [];
public byte[] TitleFlg { get; set; } = [];
public byte[] ReleaseSongFlg { get; set; } = [];
public byte[] OptionFlg { get; set; } = [];
public byte[] DefaultOptionSetting { get; set; } = [];
public bool DefaultShinSetting { get; set; }
public uint DefaultToneSetting { get; set; }
public uint SongFavoriteCnt { get; set; }
public uint SongRecentCnt { get; set; }
public uint DispLevelSelf { get; set; }
public bool IsDevil { get; set; }
public bool IsExplain { get; set; }
public DateTime LastPlayDatetime { get; set; }
public virtual UserDatum? Ba { get; set; }
```

**Apply to Momoiro:** copy the Kimidori/Murasaki save-data shape for profile, release flags, counters, favorites/recent counters, mode flags, difficulty tutorial flag, reward fields, and `LastPlayDatetime`. Keep it MOMOIRO-owned as `UserSaveData_Momoiro`. Do not add battle, Tokkun, Banacoin, Dan progress, Taikojuku, Don Challenge, or item-shop authority fields unless a later phase proves them.

### `Domain/Entities/SongBestDatumMomoiro.cs` (model, CRUD/readback)

**Analog:** `Domain/Entities/SongBestDatumKimidori.cs`

**Best-row shape** (lines 5-14):
```csharp
public partial class SongBestDatumKimidori : IAc15SongBestDatum
{
    public uint Baid { get; set; }
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public bool IsShin { get; set; }
    public uint BestScore { get; set; }
    public uint BestRate { get; set; }
    public CrownType BestCrown { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

**Apply to Momoiro:** this is the Phase 41 score/crown readback table. `selfbest.php` and `userdata.php` crown packing can both read from `SongBestDataMomoiro`. A separate `SongPlayDatumMomoiro` table is not necessary for Phase 41 unless the planner intentionally reserves play-history storage; playresult mutation is Phase 42.

### `Domain/Entities/MomoiroFavoriteSongs.cs` and `MomoiroRecentSongs.cs` (model, CRUD/readback)

**Analogs:** `Domain/Entities/KimidoriFavoriteSongs.cs`, `Domain/Entities/KimidoriRecentSongs.cs`

**Favorite row** (lines 3-8):
```csharp
public partial class KimidoriFavoriteSongs : IAc15FavoriteSong
{
    public uint Baid { get; set; }
    public uint SongNo { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

**Recent row** (lines 3-9):
```csharp
public partial class KimidoriRecentSongs : IAc15RecentSong
{
    public uint Baid { get; set; }
    public uint SongNo { get; set; }
    public DateTime LastPlayed { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

**Apply to Momoiro:** copy the table shapes exactly. Userdata should read favorites as stored and recents ordered by `LastPlayed` descending, capped by `Ac15EraProfiles.Momoiro.Limits.MaxRecentSongs`.

### `Application/Common/UserSaveDataMomoiroExtensions.cs` (utility, CRUD/defaults)

**Analog:** `Application/Common/UserSaveDataKimidoriExtensions.cs`

**Get-or-create pattern** (lines 7-20):
```csharp
public static async ValueTask<UserSaveDataKimidori> GetOrCreateKimidoriSaveDataAsync(
    this ITaikoDbContext context,
    uint baid,
    CancellationToken cancellationToken = default)
{
    var saveData = await context.UserSaveDataKimidori.FindAsync([baid], cancellationToken);
    if (saveData is not null)
    {
        return saveData;
    }

    saveData = CreateDefaultKimidoriSaveData(baid);
    context.UserSaveDataKimidori.Add(saveData);
    return saveData;
}
```

**Default bytes and profile limits** (lines 23-64):
```csharp
var limits = Ac15EraProfiles.Kimidori.Limits;
return new UserSaveDataKimidori
{
    Baid = baid,
    Title = string.Empty,
    ColorFace = 0,
    ColorBody = 1,
    ColorLimb = 3,
    CostumeFlg1 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
    CostumeFlg2 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
    CostumeFlg3 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
    CostumeFlg4 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
    CostumeFlg5 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
    ToneFlg = Ac15ProtocolBytes.CreateFixedBitset([0], limits.ToneFlagBytes),
    TitleFlg = new byte[limits.TitleFlagBytes],
    ReleaseSongFlg = new byte[limits.SongFlagBytes],
    DefaultOptionSetting = new byte[2],
    DispDanType = 1,
    GotDanFlg = new byte[limits.DanFlagBytes],
    GotDanExtraFlg = new byte[limits.DanExtraFlagBytes],
    IsAutoCostumeOn = true,
    IsTojiru = true,
    LastPlayDatetime = DateTime.UnixEpoch
};
```

**Apply to Momoiro:** replace profile reference with `Ac15EraProfiles.Momoiro`. Keep `ReleaseSongFlg` at `limits.SongFlagBytes` (currently 128 internal bytes from Phase 40), while `userdata.php` compacts release flags through the MOMOIRO song hash table for wire output. Defaults should create MOMOIRO save state only; they must not create adjacent-era save rows.

### `Application/Abstractions/ITaikoDbContext.Momoiro.cs` (port/interface, CRUD)

**Analog:** `Application/Abstractions/ITaikoDbContext.Kimidori.cs`

**DbSet pattern** (lines 3-12):
```csharp
public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataKimidori> UserSaveDataKimidori { get; }
    DbSet<SongBestDatumKimidori> SongBestDataKimidori { get; }
    DbSet<SongPlayDatumKimidori> SongPlayDataKimidori { get; }
    DbSet<KimidoriFavoriteSongs> KimidoriFavoriteSongs { get; }
    DbSet<KimidoriRecentSongs> KimidoriRecentSongs { get; }
    DbSet<DanScoreDatumKimidori> DanScoreDataKimidori { get; }
    DbSet<DanStageScoreDatumKimidori> DanStageScoreDataKimidori { get; }
}
```

**Apply to Momoiro:** expose only Phase 41-owned sets:
```csharp
DbSet<UserSaveDataMomoiro> UserSaveDataMomoiro { get; }
DbSet<SongBestDatumMomoiro> SongBestDataMomoiro { get; }
DbSet<MomoiroFavoriteSongs> MomoiroFavoriteSongs { get; }
DbSet<MomoiroRecentSongs> MomoiroRecentSongs { get; }
```

Do not copy KIMIDORI Dan sets into MOMOIRO. Do not add `SongPlayDataMomoiro` unless the Phase 41 plan deliberately reserves future playresult history; readback can be implemented from best/favorite/recent/save state.

### `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs` and `TaikoDbContext.cs` (EF mapping, CRUD)

**Analogs:** `Infrastructure/Persistence/TaikoDbContext.Kimidori.cs`, `Infrastructure/Persistence/TaikoDbContext.cs`

**DbSet declarations and save-data FK** (lines 7-28):
```csharp
public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataKimidori> UserSaveDataKimidori { get; set; } = null!;
    public virtual DbSet<SongBestDatumKimidori> SongBestDataKimidori { get; set; } = null!;
    public virtual DbSet<KimidoriFavoriteSongs> KimidoriFavoriteSongs { get; set; } = null!;
    public virtual DbSet<KimidoriRecentSongs> KimidoriRecentSongs { get; set; } = null!;

    partial void OnModelCreatingKimidori(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSaveDataKimidori>(entity =>
        {
            entity.ToTable("UserSaveData_Kimidori");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
```

**Best/favorite/recent mapping** (lines 31-84):
```csharp
modelBuilder.Entity<SongBestDatumKimidori>(entity =>
{
    entity.ToTable("SongBestDatum_Kimidori");
    entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty, e.IsShin });
    entity.HasIndex(e => new { e.SongId, e.Difficulty, e.BestScore });
    entity.Property(e => e.Difficulty).HasConversion<uint>();
    entity.Property(e => e.BestCrown).HasConversion<uint>();
});

modelBuilder.Entity<KimidoriFavoriteSongs>(entity =>
{
    entity.ToTable("KimidoriFavoriteSongs");
    entity.HasKey(e => new { e.Baid, e.SongNo });
});

modelBuilder.Entity<KimidoriRecentSongs>(entity =>
{
    entity.ToTable("KimidoriRecentSongs");
    entity.HasKey(e => new { e.Baid, e.SongNo });
    entity.Property(e => e.LastPlayed).HasColumnType("datetime");
});
```

**Main context hook** from `TaikoDbContext.cs` (lines 41-64):
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    OnModelCreatingShared(modelBuilder);
    OnModelCreatingNijiiro(modelBuilder);
    OnModelCreatingGreen(modelBuilder);
    OnModelCreatingBlue(modelBuilder);
    OnModelCreatingYellow(modelBuilder);
    OnModelCreatingRed(modelBuilder);
    OnModelCreatingWhite(modelBuilder);
    OnModelCreatingMurasaki(modelBuilder);
    OnModelCreatingKimidori(modelBuilder);
    OnModelCreatingPartial(modelBuilder);
}

partial void OnModelCreatingKimidori(ModelBuilder modelBuilder);
partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
```

**Apply to Momoiro:** add `OnModelCreatingMomoiro(modelBuilder)` before `OnModelCreatingPartial`, declare `partial void OnModelCreatingMomoiro(ModelBuilder modelBuilder);`, and map `UserSaveData_Momoiro`, `SongBestDatum_Momoiro`, `MomoiroFavoriteSongs`, and `MomoiroRecentSongs`. Keep all FKs to shared `UserData.Baid` with cascade delete.

### `Infrastructure/Persistence/Migrations/*_AddMomoiroReadbackState.cs` (migration, schema CRUD)

**Analog:** `Infrastructure/Persistence/Migrations/20260623155010_AddKimidoriRuntimeState.cs`

**Favorite/recent/best table pattern** (lines 38-96):
```csharp
migrationBuilder.CreateTable(
    name: "KimidoriFavoriteSongs",
    columns: table => new
    {
        Baid = table.Column<uint>(type: "INTEGER", nullable: false),
        SongNo = table.Column<uint>(type: "INTEGER", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_KimidoriFavoriteSongs", x => new { x.Baid, x.SongNo });
        table.ForeignKey(
            name: "FK_KimidoriFavoriteSongs_UserData_Baid",
            column: x => x.Baid,
            principalTable: "UserData",
            principalColumn: "Baid",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "SongBestDatum_Kimidori",
    columns: table => new
    {
        Baid = table.Column<uint>(type: "INTEGER", nullable: false),
        SongId = table.Column<uint>(type: "INTEGER", nullable: false),
        Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
        IsShin = table.Column<bool>(type: "INTEGER", nullable: false),
        BestScore = table.Column<uint>(type: "INTEGER", nullable: false),
        BestRate = table.Column<uint>(type: "INTEGER", nullable: false),
        BestCrown = table.Column<uint>(type: "INTEGER", nullable: false)
    },
```

**User save table pattern** (lines 145-217):
```csharp
migrationBuilder.CreateTable(
    name: "UserSaveData_Kimidori",
    columns: table => new
    {
        Baid = table.Column<uint>(type: "INTEGER", nullable: false),
        Title = table.Column<string>(type: "TEXT", nullable: false),
        CostumeFlg1 = table.Column<byte[]>(type: "BLOB", nullable: false),
        ToneFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
        TitleFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
        ReleaseSongFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
        DefaultOptionSetting = table.Column<byte[]>(type: "BLOB", nullable: false),
        SongFavoriteCnt = table.Column<uint>(type: "INTEGER", nullable: false),
        SongRecentCnt = table.Column<uint>(type: "INTEGER", nullable: false),
        DispLevelSelf = table.Column<uint>(type: "INTEGER", nullable: false),
        IsDevil = table.Column<bool>(type: "INTEGER", nullable: false),
        IsExplain = table.Column<bool>(type: "INTEGER", nullable: false),
        LastPlayDatetime = table.Column<DateTime>(type: "datetime", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_UserSaveData_Kimidori", x => x.Baid);
        table.ForeignKey(
            name: "FK_UserSaveData_Kimidori_UserData_Baid",
            column: x => x.Baid,
            principalTable: "UserData",
            principalColumn: "Baid",
            onDelete: ReferentialAction.Cascade);
    });
```

**Apply to Momoiro:** generate through `dotnet ef migrations add AddMomoiroReadbackState --project Infrastructure --startup-project Host`, then inspect and trim expectations in the plan. The migration should not create Dan tables, Tokkun tables, shop tables, Don Challenge tables, battle tables, or ChallengeCompe tables for Phase 41.

### `Application/Handlers/BaidQuery*.cs` (handler dispatcher and MOMOIRO handler, request-response)

**Analogs:** `Application/Handlers/BaidQuery.cs`, `Application/Handlers/BaidQuery.Kimidori.cs`

**Dispatcher arm pattern** (lines 22-41):
```csharp
public ValueTask<Ac15BaidResponse> Handle(Ac15BaidQuery request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    GameEra.Yellow => HandleYellow(request, cancellationToken),
    GameEra.Red => HandleRed(request, cancellationToken),
    GameEra.White => HandleWhite(request, cancellationToken),
    GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
    GameEra.Kimidori => HandleKimidori(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};
```

**New-user and existing-save branch** (Kimidori lines 12-39):
```csharp
var card = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
if (card is null)
{
    var nextBaid = await context.Cards.Select(existing => existing.Baid)
        .DefaultIfEmpty()
        .MaxAsync(cancellationToken) + 1;

    return new Ac15BaidResponse
    {
        Result = 1,
        IsNewUser = true,
        Baid = nextBaid
    };
}

var saveData = await context.UserSaveDataKimidori.FindAsync([card.Baid], cancellationToken);
if (saveData is null)
{
    return new Ac15BaidResponse
    {
        Result = 1,
        IsNewUser = true,
        Baid = card.Baid
    };
}
```

**Existing profile assembly** (Kimidori lines 41-84):
```csharp
var limits = Ac15EraProfiles.Kimidori.Limits;
var mydonProfile = new Ac15BaidProfile
{
    Title = saveData.Title,
    ColorFace = saveData.ColorFace,
    ColorBody = saveData.ColorBody,
    ColorLimb = saveData.ColorLimb,
    SelectedCostume = new Ac15CostumeFacts(
        saveData.Costume1,
        saveData.Costume2,
        saveData.Costume3,
        saveData.Costume4,
        saveData.Costume5),
    IsAutoCostumeOn = saveData.IsAutoCostumeOn,
    DefaultToneSetting = saveData.DefaultToneSetting,
    LastPlayDatetime = saveData.LastPlayDatetime == DateTime.UnixEpoch
        ? DateTime.Now.ToString(Constants.DateTimeFormat)
        : saveData.LastPlayDatetime.ToString(Constants.DateTimeFormat)
};
...
return new Ac15BaidResponse
{
    Result = 1,
    IsNewUser = false,
    Baid = card.Baid,
    Identity = new Ac15BaidIdentity(userData.MyDonName, userData.MyDonNameLanguage),
    MydonProfile = mydonProfile,
    CustomizationInventory = customizationInventory,
    DanStatus = danStatus,
    RewardProgress = rewardProgress
};
```

**Apply to Momoiro:** add a `GameEra.Momoiro` arm and `HandleMomoiro`. Use `context.UserSaveDataMomoiro`, `Ac15EraProfiles.Momoiro`, and MOMOIRO error messages. For Phase 41, do not return shop medals, Tokkun flags, battle state, Banacoin state, or ChallengeCompe fields.

### `Application/Handlers/AddMyDonEntryCommand*.cs` (handler dispatcher and MOMOIRO handler, request-response/CRUD)

**Analogs:** `Application/Handlers/AddMyDonEntryCommand.cs`, `Application/Handlers/AddMyDonEntryCommand.Kimidori.cs`, `Application/Ac15/Ac15MyDonEntryService.cs`

**Dispatcher pattern** (lines 10-30):
```csharp
public ValueTask<CommonMyDonEntryResponse> Handle(AddMyDonEntryCommand request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    GameEra.Yellow => HandleYellow(request, cancellationToken),
    GameEra.Red => HandleRed(request, cancellationToken),
    GameEra.White => HandleWhite(request, cancellationToken),
    GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
    GameEra.Kimidori => HandleKimidori(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};
```

**Era partial handler** (Kimidori lines 7-20):
```csharp
return await Ac15MyDonEntryService.HandleAsync(
    context,
    request.AccessCode,
    request.Name,
    request.Language,
    context.UserSaveDataKimidori,
    UserSaveDataKimidoriExtensions.CreateDefaultKimidoriSaveData,
    logger,
    nameof(GameEra.Kimidori),
    cancellationToken);
```

**Shared identity creation** from `Ac15MyDonEntryService.cs` (lines 17-56):
```csharp
var existingCard = await context.Cards.FindAsync([accessCode], cancellationToken);
var baid = existingCard?.Baid
    ?? await context.Cards.Select(card => card.Baid)
        .DefaultIfEmpty()
        .MaxAsync(cancellationToken) + 1;

var userData = await context.UserData.FindAsync([baid], cancellationToken);
if (userData is null)
{
    context.UserData.Add(new UserDatum
    {
        Baid = baid,
        MyDonName = name,
        MyDonNameLanguage = language
    });
}
...
if (await saveDataSet.FindAsync([baid], cancellationToken) is null)
{
    saveDataSet.Add(createDefaultSaveData(baid));
}
```

**Apply to Momoiro:** call `Ac15MyDonEntryService.HandleAsync` with `context.UserSaveDataMomoiro` and `CreateDefaultMomoiroSaveData`. This is the required identity boundary: shared `Cards`, `UserData`, and `Credentials`; MOMOIRO-owned save only.

### `Application/Handlers/UserDataQuery.Momoiro.cs` (handler, request-response/readback)

**Analog:** `Application/Handlers/UserDataQuery.Kimidori.cs`

**Core readback pattern** (lines 12-33):
```csharp
_ = await context.UserData.FindAsync([request.Baid], cancellationToken)
    ?? throw new InvalidOperationException($"User not found for Kimidori baid {request.Baid}.");
var saveData = await context.GetOrCreateKimidoriSaveDataAsync(request.Baid, cancellationToken);
var kimidori = gameDataService.Kimidori();
var favorites = await context.KimidoriFavoriteSongs
    .Where(song => song.Baid == request.Baid)
    .Select(song => song.SongNo)
    .ToArrayAsync(cancellationToken);
var recent = await context.KimidoriRecentSongs
    .Where(song => song.Baid == request.Baid)
    .OrderByDescending(song => song.LastPlayed)
    .Select(song => song.SongNo)
    .Take(Ac15EraProfiles.Kimidori.Limits.MaxRecentSongs)
    .ToArrayAsync(cancellationToken);

var snapshot = Ac15CatalogSnapshotFactory.FromKimidori(kimidori);
var userdata = KimidoriAc15UserDataAdapter.CreateSnapshot(saveData, snapshot, favorites, recent);
var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Kimidori);
```

**Optional section pattern** (lines 34-43):
```csharp
return response with
{
    Display = response.Display with { DispTaikojukuDan = GetSafeKimidoriDisplayDan(displayDan) },
    ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, saveData.IsExplain),
    Tutorial = new Ac15UserDataTutorial(null, saveData.DifficultyTutorialFlg),
    Reward = new Ac15UserDataReward(
        saveData.TotalGetDonpoint,
        saveData.TotalUseDonpoint,
        saveData.RewardProgress)
};
```

**Apply to Momoiro:** use `context.GetOrCreateMomoiroSaveDataAsync`, `gameDataService.Momoiro()`, `context.MomoiroFavoriteSongs`, `context.MomoiroRecentSongs`, `Ac15EraProfiles.Momoiro`, and `MomoiroAc15UserDataAdapter`. Omit Dan-grade normalization unless the implemented save data keeps the fields solely for mapper compatibility; MOMOIRO Phase 41 has no Dani/Taikojuku support. Add crown readback by querying `SongBestDataMomoiro` and building an inflated crown body through `Ac15CrownService.BuildInflatedBody`, then compact it in the controller through `Ac15SongHashCodec.CompactTenBitValues`.

### `Application/Handlers/GetSelfBestQuery.Momoiro.cs` (handler, request-response/readback)

**Analog:** `Application/Handlers/GetSelfBestQuery.Kimidori.cs`

**Self-best query pattern** (lines 11-28):
```csharp
var difficulties = Ac15SelfBestService.GetRequestedDifficulties(request.Difficulty);
var requestedSongs = request.SongIdList ?? [];
var requestedSet = requestedSongs.ToHashSet();
var bestRows = await context.SongBestDataKimidori
    .Where(row => row.Baid == request.Baid
        && difficulties.Contains(row.Difficulty)
        && requestedSet.Contains(row.SongId))
    .ToListAsync(cancellationToken);

var canonicalRows = bestRows.Select(row => new Ac15BestRow(
    row.SongId,
    row.Difficulty,
    row.IsShin,
    row.BestScore,
    row.BestRate,
    row.BestCrown));

return Ac15SelfBestService.BuildResponse(request.Difficulty, requestedSongs, canonicalRows);
```

**Shared self-best response behavior** from `Ac15SelfBestService.cs` (lines 5-18, 22-27, 39-45):
```csharp
return new CommonSelfBestResponse
{
    Result = 1,
    Level = requestedDifficulty,
    ArySelfbestScores = requestedSongs.Select(songNo => BuildRow(songNo, difficulty, isShin: false, rowsByKey)).ToList(),
    AryShinSelfbestScores = requestedSongs.Select(songNo => BuildRow(songNo, difficulty, isShin: true, rowsByKey)).ToList()
};

return difficulty == Difficulty.Oni
    ? [Difficulty.Oni, Difficulty.UraOni]
    : [difficulty];

SelfBestScore = best?.BestScore ?? 0,
UraBestScore = difficulty == Difficulty.Oni ? uraBest?.BestScore ?? 0 : 0,
```

**Apply to Momoiro:** change only the DbSet and era names. The existing shared service already handles normal/ura/shin readback for the generated MOMOIRO `ary_selfbest_score` and `ary_shin_selfbest_score` fields.

### `Application/Ac15/MomoiroAc15UserDataAdapter.cs` and shared userdata records/service (adapter/service, transform)

**Analogs:** `Application/Ac15/KimidoriAc15UserDataAdapter.cs`, `Application/Ac15/Ac15UserDataRecords.cs`, `Application/Ac15/Ac15UserDataService.cs`

**Adapter projection** (Kimidori lines 5-23):
```csharp
public static Ac15UserDataSnapshot CreateSnapshot(
    UserSaveDataKimidori saveData,
    Ac15CatalogSnapshot catalog,
    IReadOnlyList<uint> favorites,
    IReadOnlyList<uint> recent)
    => new(
        catalog.SongHashVersion,
        catalog.SongNoesInFileOrder,
        saveData.ReleaseSongFlg,
        saveData.ToneFlg,
        saveData.TitleFlg,
        saveData.DefaultOptionSetting,
        saveData.OptionFlg,
        favorites,
        recent,
        Counters(saveData),
        saveData.DispTaikojukuDan,
        LockedSongIds: [],
        LockedToneIds: []);
```

**Current shared snapshot shape** (lines 3-16):
```csharp
public sealed record Ac15UserDataSnapshot(
    uint SongHashVersion,
    IReadOnlyList<uint> CatalogReleaseSongNoes,
    byte[] SaveReleaseSongFlg,
    byte[] ToneFlg,
    byte[] TitleFlg,
    byte[] DefaultOptionSetting,
    byte[] OptionFlg,
    IReadOnlyList<uint> Favorites,
    IReadOnlyList<uint> Recent,
    Ac15ProfileCounters Counters,
    uint DisplayDan,
    IReadOnlyList<uint> LockedSongIds,
    IReadOnlyList<uint> LockedToneIds);
```

**Response build pattern** (lines 11-33):
```csharp
var release = Ac15ProtocolBytes.OrBitsets(
    Ac15ProtocolBytes.CreateFixedBitset(snapshot.CatalogReleaseSongNoes, profile.Limits.SongFlagBytes),
    snapshot.SaveReleaseSongFlg,
    profile.Limits.SongFlagBytes);

return new Ac15UserDataResponse
{
    Result = 1,
    SongFlags = new Ac15UserDataSongFlags
    {
        SongHashVer = snapshot.SongHashVersion,
        ReleaseSongFlg = release,
        ToneFlg = ClearBits(snapshot.ToneFlg, snapshot.LockedToneIds, profile.Limits.ToneFlagBytes),
        TitleFlg = Ac15ProtocolBytes.FixedOrZero(snapshot.TitleFlg, profile.Limits.TitleFlagBytes),
        OptionFlg = snapshot.OptionFlg
    },
    SongLists = new Ac15UserDataSongLists
    {
        AryFavoriteSongNoes = snapshot.Favorites.ToArray(),
        AryRecentSongNoes = snapshot.Recent.ToArray()
    },
```

**Apply to Momoiro:** add the smallest shared extension needed for crown readback. Prefer one of these shapes:

```csharp
public sealed record Ac15UserDataCrownFlags(byte[] InflatedCrownFlg);
```

or add a nullable/empty crown byte array to `Ac15UserDataResponse` / `Ac15UserDataSongFlags`, then map only in MOMOIRO. Existing dedicated `crownsdata.php` controllers for other eras must keep working and should not start emitting crown bytes inside userdata.

### `Application/Ac15/Ac15CrownService.cs` and `Ac15SongHashCodec.cs` (service, transform/bit-packing)

**Analogs:** existing shared crown and hash codecs.

**Crown body builder** (lines 5-18, 28-54):
```csharp
public static byte[] BuildInflatedBody(
    IEnumerable<Ac15BestRow> bestRows,
    IEnumerable<uint> validSongNoes,
    Ac15ProtocolLimits limits)
{
    var values = new ushort[limits.CrownSongCount];
    var validSongs = validSongNoes
        .Where(songNo => songNo < limits.CrownSongCount)
        .ToHashSet();

    foreach (var group in bestRows.GroupBy(row => row.SongId))
    {
        if (!validSongs.Contains(group.Key))
        {
            continue;
        }
        ...
        values[group.Key] = Ac15ProtocolBytes.BuildCrownValue(easy, normal, hard, oni, ura);
    }

    return Ac15ProtocolBytes.PackTenBitValues(values, limits.CrownPackedBytes, limits.CrownSongCount);
}
```

**Song-hash compaction** (lines 21-25, 37-44):
```csharp
public static byte[] CompactBitset(byte[] inflated, IReadOnlyList<ushort> table)
    => CompactValues(inflated, table, bitsPerValue: 1);

public static byte[] CompactTenBitValues(byte[] inflated, IReadOnlyList<ushort> table)
    => CompactValues(inflated, table, bitsPerValue: 10);

var bitCount = checked(table.Count * bitsPerValue);
var result = new byte[(bitCount + 7) / 8];
for (var ordinal = 0; ordinal < table.Count; ordinal++)
{
    var sourceBitOffset = checked(table[ordinal] * bitsPerValue);
    var value = ReadBits(inflated, sourceBitOffset, bitsPerValue);
    WriteBits(result, ordinal * bitsPerValue, bitsPerValue, value);
}
```

**Apply to Momoiro:** build inflated crowns with `Ac15EraProfiles.Momoiro.Limits` (`CrownSongCount = 380`, `CrownPackedBytes = 475` from Phase 40), then compact the inflated crown body using `gameDataService.Momoiro().SongHashTable` before assigning `UserDataResponse.HashCrownFlg`. Do not create a MOMOIRO-only crown route or codec unless tests disprove the shared 10-bit/hash-order path.

### `Adapters.GameProtocol.Momoiro/Controllers/BaidController.cs` (controller, request-response)

**Analog:** `Adapters.GameProtocol.Kimidori/Controllers/BaidController.cs`

**Controller pattern** (lines 6-22, 25-41):
```csharp
[HttpPost(KimidoriRoutePrefixes.Game + "/baidcheck.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> BaidCheck([FromBody] BAIDRequest request)
{
    Logger.LogInformation("Kimidori BAID request: {@Request}", request);
    var common = await Mediator.Send(new Ac15BaidQuery(GameEra.Kimidori, request.AccessCode), HttpContext.RequestAborted);

    if (common.IsNewUser)
    {
        return Ok(new BAIDResponse
        {
            Result = 1,
            PlayerType = 1,
            Baid = common.Baid
        });
    }

    var response = new BAIDResponse
    {
        Result = common.Result,
        Baid = common.Baid,
        AccessCode = request.AccessCode,
        IsPublish = true,
        PlayerType = 0,
        ComSvrResult = 1,
        RegCountryId = "JPN",
        MbId = 1,
        PurposeId = 1,
        RegionId = 1,
        ContentInfo = new byte[Ac15EraProfiles.Kimidori.Limits.ContentInfoBytes]
    };
    ApplySections(common, response);
```

**MOMOIRO scaffold to replace** (lines 8-18):
```csharp
[HttpPost(MomoiroRoutePrefixes.Game + "/baidcheck.php")]
[Produces("application/protobuf")]
public IActionResult BaidCheck([FromBody] BAIDRequest request)
{
    Logger.LogInformation(
        "Momoiro baidcheck.php scaffold request: ChassisId={ChassisId}, ShopId={ShopId}, CountryId={CountryId}",
        request.ChassisId,
        request.ShopId,
        request.CountryId);

    return Ok(new BAIDResponse { Result = 1 });
}
```

**Apply to Momoiro:** make the action async, send `Ac15BaidQuery(GameEra.Momoiro, request.AccessCode)`, and copy the section-application pattern. In MOMOIRO wire, `BAIDResponse` has `PlayerType`, `Baid`, `AccessCode`, `IsPublish`, `RegCountryId`, `PurposeId`, `RegionId`, `MydonName`, costume flags, `RewardPtn`, `GotDanFlg`, and `ContentInfo` fields (generated lines 40-329). Set only fields backed by Phase 41 shared identity/MOMOIRO save state.

### `Adapters.GameProtocol.Momoiro/Controllers/MyDonEntryController.cs` (controller, request-response)

**Analog:** `Adapters.GameProtocol.Kimidori/Controllers/MyDonEntryController.cs`

**Controller pattern** (lines 6-30):
```csharp
[HttpPost(KimidoriRoutePrefixes.Game + "/mydonentry.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> MydonEntry([FromBody] MydonEntryRequest request)
{
    Logger.LogInformation("Kimidori MyDonEntry request: {@Request}", request);

    var common = await Mediator.Send(
        new AddMyDonEntryCommand(GameEra.Kimidori, request.AccessCode, request.MydonName, 0),
        HttpContext.RequestAborted);

    return Ok(new MydonEntryResponse
    {
        Result = common.Result,
        ComSvrResult = common.ComSvrResult,
        MbId = 1,
        Baid = common.Baid,
        AccessCode = common.AccessCode,
        IsPublish = true,
        RegCountryId = "JPN",
        PurposeId = 1,
        RegionId = 1,
        MydonName = common.MydonName,
        RewardPtn = request.RewardPtn,
        ContentInfo = new byte[Ac15EraProfiles.Kimidori.Limits.ContentInfoBytes]
    });
}
```

**MOMOIRO wire fields** (generated lines 764-786, 797-932):
```csharp
public string AccessCode { get; set; }
public string MydonName { get; set; }
public uint RewardPtn { get; set; }
...
public uint Result { get; set; }
public uint? ComSvrResult { get; set; }
public uint? Baid { get; set; }
public string AccessCode { get; set; }
public bool? IsPublish { get; set; }
public string RegCountryId { get; set; }
public string MydonName { get; set; }
public uint? RewardPtn { get; set; }
public byte[] ContentInfo { get; set; }
```

**Apply to Momoiro:** same controller shape with `GameEra.Momoiro` and `Ac15EraProfiles.Momoiro`. Keep request `RewardPtn` echo behavior only; do not implement reward mutation in Phase 41.

### `Adapters.GameProtocol.Momoiro/Controllers/UserDataController.cs` and `Mappers/UserDataMappers.cs` (controller/mapper, request-response)

**Analogs:** `Adapters.GameProtocol.Kimidori/Controllers/UserDataController.cs`, `Adapters.GameProtocol.Kimidori/Mappers/UserDataMappers.cs`

**Controller assembly + release compaction** (Kimidori lines 9-37):
```csharp
public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
{
    Logger.LogInformation("Kimidori UserData request: {@Request}", request);
    var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.Kimidori), HttpContext.RequestAborted);

    var response = new UserDataResponse
    {
        Result = common.Result
    };
    UserDataMappers.Apply(common.SongFlags, response);
    UserDataMappers.Apply(common.SongLists, response);
    UserDataMappers.Apply(common.Counters, response);
    UserDataMappers.Apply(common.Display, response);
    UserDataMappers.Apply(common.Recommendations, response);
    var kimidori = gameDataService.Kimidori();
    response.HashReleaseSongFlg = Ac15SongHashCodec.CompactBitset(
        response.HashReleaseSongFlg,
        kimidori.SongHashTable);
    ...
    return Ok(response);
}
```

**Mapper pattern** (Kimidori lines 6-30):
```csharp
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class UserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataSongLists source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs), Use = nameof(MapRecommendBestSongs))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] UserDataResponse response);
```

**MOMOIRO generated userdata fields** (lines 1791-1835 and 1999-2027):
```csharp
public uint[] AryFavoriteSongNoes { get; set; }
public uint[] AryRecentSongNoes { get; set; }
public uint? SongHashVer { get; set; }
public byte[] HashReleaseSongFlg { get; set; }
public bool? IsDevil { get; set; }
public byte[] HashCrownFlg { get; set; }
...
public uint? SongPushedCnt { get; set; }
public uint? SongFavoriteCnt { get; set; }
public uint? SongRecentCnt { get; set; }
```

**Apply to Momoiro:** inject `IGameDataCatalog` like Kimidori, send `Ac15UserDataQuery(request.Baid, GameEra.Momoiro)`, map sections, compact `HashReleaseSongFlg` with `CompactBitset`, and compact/set `HashCrownFlg` with `CompactTenBitValues`. Keep `crownsdata.php` absent.

### `Adapters.GameProtocol.Momoiro/Controllers/SelfBestController.cs` and `Mappers/SelfBestMappers.cs` (controller/mapper, request-response)

**Analogs:** `Adapters.GameProtocol.Kimidori/Controllers/SelfBestController.cs`, `Adapters.GameProtocol.Kimidori/Mappers/SelfBestMappers.cs`

**Controller pattern** (Kimidori lines 6-15):
```csharp
[HttpPost(KimidoriRoutePrefixes.Game + "/selfbest.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
{
    Logger.LogInformation("Kimidori SelfBest request: {@Request}", request);
    var common = await Mediator.Send(
        new GetSelfBestQuery(request.Baid, GameEra.Kimidori, request.Level, request.ArySongNoes ?? []),
        HttpContext.RequestAborted);
    return Ok(SelfBestMappers.Map(common));
}
```

**Mapper pattern** (Kimidori lines 5-14):
```csharp
[Mapper]
public static partial class SelfBestMappers
{
    [MapProperty(nameof(CommonSelfBestResponse.ArySelfbestScores), nameof(SelfBestResponse.ArySelfbestScores))]
    [MapProperty(nameof(CommonSelfBestResponse.AryShinSelfbestScores), nameof(SelfBestResponse.AryShinSelfbestScores))]
    public static partial SelfBestResponse Map(CommonSelfBestResponse common);

    [MapperIgnoreSource(nameof(CommonSelfBestResponse.SelfBestData.SelfBestScoreRate))]
    [MapperIgnoreSource(nameof(CommonSelfBestResponse.SelfBestData.UraBestScoreRate))]
    private static partial SelfBestResponse.SelfBestData MapSelfBestData(CommonSelfBestResponse.SelfBestData row);
}
```

**MOMOIRO generated self-best fields** (lines 1505-1547):
```csharp
public partial class SelfBestResponse : global::ProtoBuf.IExtensible
{
    public uint Result { get; set; }
    public uint? Level { get; set; }
    public List<SelfBestData> ArySelfbestScores { get; } = new();
    public List<SelfBestData> AryShinSelfbestScores { get; } = new();

    public partial class SelfBestData : global::ProtoBuf.IExtensible
    {
        public uint SongNo { get; set; }
        public uint SelfBestScore { get; set; }
        public uint UraBestScore { get; set; }
    }
}
```

**Apply to Momoiro:** copy Kimidori exactly with MOMOIRO route prefix and `GameEra.Momoiro`. The `SelfBestScoreRate` and `UraBestScoreRate` common fields should be ignored because MOMOIRO generated wire does not expose them in `SelfBestData`.

### `Adapters.GameProtocol.Momoiro/Mappers/BaidResponseMapper.cs` (mapper, transform)

**Analog:** `Adapters.GameProtocol.Kimidori/Mappers/BaidResponseMapper.cs`

**Mapperly section apply pattern** (lines 7-40):
```csharp
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class BaidResponseMapper
{
    [MapProperty(nameof(Ac15BaidIdentity.MyDonName), nameof(BAIDResponse.MydonName))]
    [MapperIgnoreSource(nameof(Ac15BaidIdentity.MyDonNameLanguage))]
    public static partial void Apply(Ac15BaidIdentity source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidProfile.SelectedCostume), nameof(BAIDResponse.AryCostumedata), Use = nameof(MapCostumeData))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.TitlePlateId))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.IsAutoCostumeOn))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.DefaultToneSetting))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.LastPlayDatetime))]
    public static partial void Apply(Ac15BaidProfile source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg1), nameof(BAIDResponse.CostumeFlg1), Use = nameof(MapCostumeFlag))]
    public static partial void Apply(Ac15BaidCostumeFlags source, [MappingTarget] BAIDResponse response);

    private static byte[] MapCostumeFlag(byte[]? value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes);
}
```

**Apply to Momoiro:** create the same mapper in the Momoiro namespace, switch limit helpers to `Ac15EraProfiles.Momoiro`, and use generated nested `BAIDResponse.CustumeData` (MOMOIRO wire spelling) if Mapperly needs the explicit helper type. Verify generated `.g.cs` after build; do not hand-write mapper bodies except helper conversions.

## Shared Patterns

### Shared Identity, Era-Owned Save

**Source:** `Application/Ac15/Ac15MyDonEntryService.cs`
**Apply to:** `AddMyDonEntryCommand.Momoiro.cs`, `BaidQuery.Momoiro.cs`, runtime tests.

The shared identity boundary is `Cards`, `UserData`, and `Credentials`; the era-owned boundary is `UserSaveDataMomoiro` plus MOMOIRO best/favorite/recent rows. Copy the MyDon service pattern, and add tests like `MurasakiRuntimeHandlerTests.cs` lines 34-58:

```csharp
Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
Assert.NotNull(await fixture.Context.UserData.FindAsync(1u));
Assert.NotNull(await fixture.Context.Credentials.FindAsync(1u));
Assert.NotNull(await fixture.Context.UserSaveDataMurasaki.FindAsync(1u));
Assert.Empty(await fixture.Context.UserSaveDataBlue.Where(row => row.Baid == 1).ToListAsync());
Assert.Empty(await fixture.Context.UserSaveDataGreen.Where(row => row.Baid == 1).ToListAsync());
Assert.Empty(await fixture.Context.UserSaveDataYellow.Where(row => row.Baid == 1).ToListAsync());
Assert.Empty(await fixture.Context.UserSaveDataRed.Where(row => row.Baid == 1).ToListAsync());
Assert.Empty(await fixture.Context.UserSaveDataWhite.Where(row => row.Baid == 1).ToListAsync());
Assert.Empty(await fixture.Context.UserSaveDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
```

Extend the no-cross-era list to include `UserSaveDataMurasaki` and `UserSaveDataKimidori` when asserting MOMOIRO creation.

### Userdata Readback

**Sources:** `UserDataQuery.Kimidori.cs`, `KimidoriAc15UserDataAdapter.cs`, `Ac15UserDataService.cs`
**Apply to:** MOMOIRO userdata handler, adapter, mapper, controller.

Copy the handler flow:
1. validate shared `UserData` exists;
2. get or create MOMOIRO save data;
3. read MOMOIRO favorites and recents only;
4. build `Ac15CatalogSnapshotFactory.FromMomoiro`;
5. build shared response with `Ac15UserDataService`;
6. attach MOMOIRO-only optional sections and crown bytes.

Use the Murasaki runtime test style (lines 61-101) for readback assertions:
```csharp
Assert.Equal([101u], response.SongLists.AryFavoriteSongNoes);
Assert.Equal([102u], response.SongLists.AryRecentSongNoes);
Assert.True(response.ModeFlags!.IsDevil);
Assert.True(response.ModeFlags.IsExplain);
```

### Crown Readback

**Sources:** `Ac15CrownService.cs`, `Ac15SongHashCodec.cs`, `MomoiroProtocolLimitsTests.cs`, `MomoiroRouteSurfaceTests.cs`
**Apply to:** MOMOIRO userdata handler/controller tests.

Use:
```csharp
var inflated = Ac15CrownService.BuildInflatedBody(bestRows, momoiro.MusicInfoFileOrder.Select(song => song.SongNo), Ac15EraProfiles.Momoiro.Limits);
var compact = Ac15SongHashCodec.CompactTenBitValues(inflated, momoiro.SongHashTable);
```

Then map `compact` to `UserDataResponse.HashCrownFlg`. Phase 40 already pinned `CrownPlacement = UserData`, `CrownSongCount = 380`, and `CrownPackedBytes = 475` in `Ac15EraProfiles.cs` lines 119-185 and `MomoiroProtocolLimitsTests.cs` lines 6-53. Keep `MomoiroRouteSurfaceTests.cs` lines 87-98 unchanged as the dedicated crown-route absence guard.

### Self-Best

**Sources:** `GetSelfBestQuery.Kimidori.cs`, `Ac15SelfBestService.cs`, `SelfBestMappers.cs`
**Apply to:** MOMOIRO `selfbest.php`.

`Ac15SelfBestService` already preserves requested song order, emits zero rows for missing songs, separates normal and shin rows, and populates ura best for Oni. The regression examples are in `Tests/Ac15/Ac15SelfBestServiceTests.cs` lines 7-67.

### Mapperly

**Sources:** `Adapters.GameProtocol.Momoiro/MapperlyDefaults.cs`, Kimidori mappers.
**Apply to:** all new MOMOIRO mappers.

`Adapters.GameProtocol.Momoiro/MapperlyDefaults.cs` lines 3-6 already sets:
```csharp
[assembly: MapperDefaults(
    AutoUserMappings = false,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

Keep mappers source-generator driven. Verification should include:
```powershell
dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore
```

Then inspect emitted `BaidResponseMapper.g.cs`, `UserDataMappers.g.cs`, and `SelfBestMappers.g.cs`.

### Test Fixture

**Sources:** `Tests/Murasaki/MurasakiHandlerFixture.cs`, `Tests/Kimidori/KimidoriHandlerFixture.cs`, `Tests/Momoiro/MomoiroMetadataRouteTests.cs`
**Apply to:** `Tests/Momoiro/MomoiroHandlerFixture.cs`.

Use the in-memory SQLite pattern from Murasaki (lines 21-33):
```csharp
var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();

var options = new DbContextOptionsBuilder<TaikoDbContext>()
    .UseSqlite(connection)
    .Options;
var context = new TaikoDbContext(options);
await context.Database.EnsureCreatedAsync();

var catalog = new FileGameDataCatalog([murasakiCatalog ?? new TestMurasakiCatalog()]);
```

Use the lightweight catalog stub shape from Kimidori (lines 8-30):
```csharp
internal sealed class TestKimidoriCatalog : IKimidoriCatalog
{
    public GameEra Era => GameEra.Kimidori;
    public uint SongHashVersion => 505;
    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;
    public IReadOnlyList<ushort> SongHashTable
        => MusicInfoFileOrder.Select(song => checked((ushort)song.SongNo)).ToArray();
    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> KimidoriMusicInfos
        => MusicInfoFileOrder.ToDictionary(song => song.SongNo);
}
```

For controller invocation tests, reuse `MomoiroMetadataRouteTests.cs` lines 121-148 (`InvokeActionAsync`) and lines 150-171 (`BuildMomoiroProvider`) if the test should exercise the actual DI graph.

## Caution And Avoid Patterns

| Pattern | Source | Why to avoid or constrain |
|---------|--------|---------------------------|
| `UpdatePlayResultCommand.*`, `Ac15NormalPlayWriter`, `SongPlayDatum*` mutation writes | Murasaki/Kimidori playresult implementation | Phase 41 is readback only. Do not add score writes, crown writes, favorite/recent writes, Don Point/reward mutation, Dan mutation, or challenge-shaped mutation. |
| KIMIDORI/Murasaki Dan tables and handlers | `DanScoreDatumKimidori`, `GetTaikojukuQuery.*`, `TaikojukuController`, Dan sections in migrations | MOMOIRO Phase 41 excludes Dani/Taikojuku. Do not copy Dan tables just because Kimidori runtime migration has them. |
| Dedicated crown route/controller | `CrownsDataController` in KIMIDORI/Murasaki | MOMOIRO crown readback is `UserDataResponse.hash_crown_flg`; Phase 40 route tests explicitly exclude `crownsdata.php`. |
| Proto-only MOMOIRO routes | `BestScoreResponse`, `ShoppingResult`, `CommunicationLog`, `MainichiSong` generated wire families | Phase 39/40 route evidence excludes these active routes. Do not add controllers or persistence for them in Phase 41. |
| Newer feature families | Blue battle/Tokkun/Banacoin/gacha/tournament, Red/White Don Challenge/ChallengeCompe, item shop authority | Explicitly out of MOMOIRO Phase 41. They also risk cross-era writes and invented semantics. |
| Hand-written mapper replacement | Any manual mapping body replacing Mapperly projections | Repo rule requires Mapperly source-generator driven mappers. Handwritten code is limited to helper conversions Mapperly uses. |
| Handler/controller filesystem reads | Any `File.*`, `Directory.*`, or hardcoded `Host/wwwroot/data/momoiro` in Application/adapter readback code | Catalog access belongs behind `IGameDataCatalog.For(GameEra.Momoiro)` and `IMomoiroCatalog`. |
| Treating Phase 40 low-confidence constants as final native proof | `MomoiroProtocolLimitsTests.cs` assertion messages | Favorite max and crown byte count are implemented contracts, but still labeled low-confidence until stronger binary/client evidence appears. Keep tests honest. |

## No Analog Found

| File/Concern | Role | Data Flow | Reason |
|--------------|------|-----------|--------|
| Exact MOMOIRO-native favorite duplicate/truncation semantics | evidence/test | readback | Existing AC15 handlers can cap and order rows, but exact duplicate behavior needs client/binary proof. Keep implementation simple and tests scoped to current stored rows and cap unless new evidence appears. |
| MOMOIRO-specific crown packing beyond shared 10-bit/hash-order compaction | codec/evidence | transform | Phase 40 inferred 475-byte compact envelope from 380 songs * 10 bits. Use shared codec first; only add MOMOIRO-specific codec if tests or native evidence disprove it. |
| MOMOIRO `SongPlayDatum` readback role | model | CRUD/readback | No Phase 41 route consumes play history. Add only if the planner chooses to reserve future Phase 42 storage, and keep all writes out of Phase 41. |

## Metadata

**Analog search scope:** `Domain/Entities`, `Application/Common`, `Application/Abstractions`, `Application/Handlers`, `Application/Ac15`, `Infrastructure/Persistence`, `Infrastructure/Persistence/Migrations`, `Adapters.GameProtocol.Kimidori`, `Adapters.GameProtocol.Murasaki`, `Adapters.GameProtocol.Momoiro`, `Tests/Ac15`, `Tests/Kimidori`, `Tests/Murasaki`, `Tests/Momoiro`, and Phase 40 artifacts.

**Files scanned:** 90+ source/test/planning files through `rg` plus targeted line-numbered reads of 35 files/ranges.

**Strong analogs used:** KIMIDORI runtime state and controllers for simple older-era readback; Murasaki handler fixture/runtime tests for no-cross-era assertions; shared AC15 self-best, userdata, crown, and song-hash services; existing MOMOIRO Phase 40 profile/metadata tests and route absence guards.

**Pattern extraction date:** 2026-06-26
