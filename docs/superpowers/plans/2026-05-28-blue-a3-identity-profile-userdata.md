# Blue A3 Identity Profile Userdata Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build Blue-owned normal profile persistence and real Blue behavior for `baidcheck.php`, `mydonentry.php`, `initialdatacheck.php`, and `userdata.php`.

**Architecture:** Blue shares global identity rows (`Card`, `UserDatum`, `Credential`) but owns normal save state in `UserSaveData_Blue`. Existing mediator requests gain `GameEra.Blue` branches that call Blue partial handlers, and the Blue adapter gains mapper classes for Blue generated protobuf types. Shared AC15 extraction stays small: bitset logic remains width-parameterized, while byte widths and wire mappers stay era-owned.

**Tech Stack:** C#/.NET, ASP.NET Core controllers, Mediator, Entity Framework Core with SQLite migrations, protobuf-net generated wire types, xUnit tests.

---

## Scope Check

This plan implements one roadmap stage: Blue A3 identity, profile, initial data, and userdata. It does not implement Blue play results, crowns, self-best, rewards, Dani completion, item purchase state, AdminApi/WebUI, Tokkun, Banacoin, or battle mode.

Implementation assumes A2 catalog work has landed with these names:

- `Application/Abstractions/IBlueCatalog.cs`
- `Application/Common/CatalogExtensions.cs` can expose `Blue()`
- `Application/Catalog/Blue/BlueMusicInfoEntry.cs`
- `Application/Catalog/Blue/BlueRecommendEntry.cs`
- `Application/Catalog/Blue/BlueItemShopCatalog.cs`
- `Application/Catalog/Blue/BlueTelopEntry.cs`
- `Application/Catalog/Blue/BlueTaikojukuEntry.cs`
- `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs`

If A2 lands with different type names, first make a tiny compatibility commit that aligns the A2 public surface with `docs/superpowers/specs/2026-05-28-blue-a2-catalog-data-layout-design.md`, then start Task 1.

## File Structure

Create:

- `Domain/Entities/UserSaveDataBlue.cs` - Blue normal profile save row.
- `Application/Abstractions/ITaikoDbContext.Blue.cs` - Blue DbSet on the application context abstraction.
- `Application/Common/BlueProtocolBytes.cs` - Blue-owned byte widths and fixed-width bitset wrappers.
- `Application/Common/UserSaveDataBlueExtensions.cs` - get-or-create and default Blue save data.
- `Application/Dtos/CommonInitialDataCheckResponse.Blue.cs` - Blue information-array and battle-presence DTO fields.
- `Application/Dtos/CommonUserDataResponse.Blue.cs` - Blue-specific userdata response fields.
- `Application/Handlers/BaidQuery.Blue.cs` - Blue BAID lookup/profile readback.
- `Application/Handlers/AddMyDonEntryCommand.Blue.cs` - Blue registration.
- `Application/Handlers/GetInitialDataQuery.Blue.cs` - Blue initial-data response.
- `Application/Handlers/UserDataQuery.Blue.cs` - Blue userdata response.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs` - EF mapping for `UserSaveData_Blue`.
- `Adapters.GameProtocol.Blue/Mappers/BaidResponseMapper.cs` - Blue BAID mapper.
- `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs` - Blue initial-data mapper.
- `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs` - Blue userdata mapper.
- `Tests/Blue/BlueHandlerFixture.cs` - in-memory DB plus test `IBlueCatalog`.
- `Tests/Blue/BlueSaveDataTests.cs` - default save and EF mapping tests.
- `Tests/Blue/BlueIdentityHandlerTests.cs` - BAID and mydon entry handler tests.
- `Tests/Blue/BlueInitialDataTests.cs` - initial-data handler tests.
- `Tests/Blue/BlueUserDataTests.cs` - userdata handler tests.
- `Tests/Blue/BlueMapperTests.cs` - Blue mapper optional-field tests.
- `Tests/Blue/BlueA3SourceGuardTests.cs` - source guard against Green dependencies.

Modify:

- `Infrastructure/Persistence/TaikoDbContext.cs` - call `OnModelCreatingBlue`.
- `Application/Common/CatalogExtensions.cs` - add `Blue()` extension.
- `Application/Dtos/CommonInitialDataCheckResponse.cs` - make the class `partial`.
- `Application/Handlers/BaidQuery.cs` - dispatch Blue.
- `Application/Handlers/AddMyDonEntryCommand.cs` - dispatch Blue.
- `Application/Handlers/GetInitialDataQuery.cs` - dispatch Blue.
- `Application/Handlers/UserDataQuery.cs` - dispatch Blue.
- `Adapters.GameProtocol.Blue/GlobalUsings.cs` - add application and mapper globals used by real controllers.
- `Adapters.GameProtocol.Blue/Controllers/BaidController.cs` - mediator-backed Blue BAID.
- `Adapters.GameProtocol.Blue/Controllers/MyDonEntryController.cs` - mediator-backed Blue registration.
- `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs` - mediator-backed Blue initial data.
- `Adapters.GameProtocol.Blue/Controllers/UserDataController.cs` - mediator-backed Blue userdata.
- `Tests/Blue/BlueRouteSkeletonTests.cs` - allow mediator use only in the four A3 controllers.
- `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` plus generated migration files - add `UserSaveData_Blue`.

## Task 1: Blue Save Persistence And Defaults

**Files:**
- Create: `Tests/Blue/BlueSaveDataTests.cs`
- Create: `Domain/Entities/UserSaveDataBlue.cs`
- Create: `Application/Abstractions/ITaikoDbContext.Blue.cs`
- Create: `Application/Common/BlueProtocolBytes.cs`
- Create: `Application/Common/UserSaveDataBlueExtensions.cs`
- Create: `Infrastructure/Persistence/TaikoDbContext.Blue.cs`
- Modify: `Infrastructure/Persistence/TaikoDbContext.cs`
- Generate: `Infrastructure/Persistence/Migrations/*AddBlueIdentityProfileSupport*`

- [ ] **Step 1: Write the failing save-data tests**

Create `Tests/Blue/BlueSaveDataTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueSaveDataTests
{
    [Fact]
    public void CreateDefaultBlueSaveData_InitializesFixedWidthBytesAndProfileDefaults()
    {
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(123);

        Assert.Equal(123u, save.Baid);
        Assert.Equal(string.Empty, save.Title);
        Assert.Equal(0u, save.TitleplateId);
        Assert.Equal(0u, save.ColorFace);
        Assert.Equal(1u, save.ColorBody);
        Assert.Equal(3u, save.ColorLimb);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg1.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg2.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg3.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg4.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg5.Length);
        Assert.True(BitIsSet(save.CostumeFlg1, 0));
        Assert.True(BitIsSet(save.CostumeFlg2, 0));
        Assert.True(BitIsSet(save.CostumeFlg3, 0));
        Assert.True(BitIsSet(save.CostumeFlg4, 0));
        Assert.True(BitIsSet(save.CostumeFlg5, 0));
        Assert.Equal(BlueProtocolBytes.ToneFlagBytes, save.ToneFlg.Length);
        Assert.True(BitIsSet(save.ToneFlg, 0));
        Assert.Equal(BlueProtocolBytes.TitleFlagBytes, save.TitleFlg.Length);
        Assert.Equal(2, save.DefaultOptionSetting.Length);
        Assert.Equal(1u, save.DispDanType);
        Assert.Equal(0u, save.GotDanMax);
        Assert.Equal(BlueProtocolBytes.DanFlagBytes, save.GotDanFlg.Length);
        Assert.Equal(BlueProtocolBytes.DanExtraFlagBytes, save.GotDanExtraFlg.Length);
        Assert.Equal(0u, save.DispTaikojukuDan);
        Assert.True(save.IsAutoCostumeOn);
        Assert.False(save.IsDevil);
        Assert.False(save.IsExplain);
        Assert.False(save.IsChallengeCompe);
        Assert.True(save.IsTojiru);
        Assert.Equal(DateTime.UnixEpoch, save.LastPlayDatetime);
    }

    [Fact]
    public async Task TaikoDbContext_CanPersistBlueSaveData()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 5, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(5));
        await fixture.Context.SaveChangesAsync();

        var loaded = await fixture.Context.UserSaveDataBlue.FindAsync(5u);

        Assert.NotNull(loaded);
        Assert.Equal(5u, loaded!.Baid);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
```

- [ ] **Step 2: Run the failing save-data tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueSaveDataTests
```

Expected: FAIL at compile time because `UserSaveDataBlueExtensions`, `BlueProtocolBytes`, `UserSaveDataBlue`, and `BlueHandlerFixture` do not exist.

- [ ] **Step 3: Add the Blue save entity**

Create `Domain/Entities/UserSaveDataBlue.cs`:

```csharp
namespace TaikoLocalServer.Domain.Entities;

public partial class UserSaveDataBlue
{
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
    public byte[] OptionFlg { get; set; } = [];
    public byte[] DefaultOptionSetting { get; set; } = [];
    public bool DefaultShinSetting { get; set; }
    public uint DefaultToneSetting { get; set; }
    public uint DispDanType { get; set; }
    public uint GotDanMax { get; set; }
    public byte[] GotDanFlg { get; set; } = [];
    public byte[] GotDanExtraFlg { get; set; } = [];
    public uint DispTaikojukuDan { get; set; }
    public uint TotalGetDonmedal { get; set; }
    public uint TotalUseDonmedal { get; set; }
    public uint TotalGetKatsumedal { get; set; }
    public uint TotalUseKatsumedal { get; set; }
    public uint ItemshopTutorialFlg { get; set; }
    public bool IsAutoCostumeOn { get; set; }
    public uint CategJpopCnt { get; set; }
    public uint CategAnimeCnt { get; set; }
    public uint CategDoyoCnt { get; set; }
    public uint CategVarietyCnt { get; set; }
    public uint CategClassicCnt { get; set; }
    public uint CategGameCnt { get; set; }
    public uint CategNamcoCnt { get; set; }
    public uint CategVocaloidCnt { get; set; }
    public uint SongPushedCnt { get; set; }
    public uint SongFavoriteCnt { get; set; }
    public uint SongRecentCnt { get; set; }
    public uint TotalCreditCnt { get; set; }
    public uint PrevAreaCode { get; set; }
    public uint ConsecAreaCnt { get; set; }
    public uint DispLevelTotal { get; set; }
    public uint DispLevelChassis { get; set; }
    public uint DispLevelSelf { get; set; }
    public bool IsDevil { get; set; }
    public uint DispScoreType { get; set; }
    public uint DifficultyPlayedCourse { get; set; }
    public uint DifficultyPlayedStar { get; set; }
    public uint WaiwaiTutorialFlg { get; set; }
    public bool IsChallengeCompe { get; set; }
    public bool IsTojiru { get; set; }
    public bool IsExplain { get; set; }
    public DateTime LastPlayDatetime { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

- [ ] **Step 4: Add Blue DbContext abstraction and EF mapping**

Create `Application/Abstractions/ITaikoDbContext.Blue.cs`:

```csharp
namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataBlue> UserSaveDataBlue { get; }
}
```

Create `Infrastructure/Persistence/TaikoDbContext.Blue.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataBlue> UserSaveDataBlue { get; set; } = null!;

    partial void OnModelCreatingBlue(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSaveDataBlue>(entity =>
        {
            entity.ToTable("UserSaveData_Blue");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

Modify `Infrastructure/Persistence/TaikoDbContext.cs` so `OnModelCreating` calls the Blue partial after Green:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    OnModelCreatingShared(modelBuilder);
    OnModelCreatingNijiiro(modelBuilder);
    OnModelCreatingGreen(modelBuilder);
    OnModelCreatingBlue(modelBuilder);
    OnModelCreatingPartial(modelBuilder);
}
```

Add the partial declaration near the existing partial declarations:

```csharp
partial void OnModelCreatingBlue(ModelBuilder modelBuilder);
```

- [ ] **Step 5: Add Blue protocol byte helpers and default-save helper**

Create `Application/Common/BlueProtocolBytes.cs`:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class BlueProtocolBytes
{
    public const int SongFlagBytes = 128;
    public const int ToneFlagBytes = 16;
    public const int TitleFlagBytes = 128;
    public const int CostumeFlagBytes = 32;
    public const int DanFlagBytes = 18;
    public const int DanExtraFlagBytes = 36;
    public const int ContentInfoBytes = 32;

    public static byte[] CreateFixedBitset(IEnumerable<uint> enabledIds, int byteCount)
    {
        return BitsetCodec.Encode(enabledIds, byteCount);
    }

    public static byte[] FixedOrZero(byte[]? source, int byteCount)
    {
        return BitsetCodec.Normalize(source, byteCount);
    }
}
```

Create `Application/Common/UserSaveDataBlueExtensions.cs`:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class UserSaveDataBlueExtensions
{
    public static async ValueTask<UserSaveDataBlue> GetOrCreateBlueSaveDataAsync(
        this ITaikoDbContext context,
        uint baid,
        CancellationToken cancellationToken = default)
    {
        var saveData = await context.UserSaveDataBlue.FindAsync([baid], cancellationToken);
        if (saveData is not null)
        {
            return saveData;
        }

        saveData = CreateDefaultBlueSaveData(baid);
        context.UserSaveDataBlue.Add(saveData);
        return saveData;
    }

    public static UserSaveDataBlue CreateDefaultBlueSaveData(uint baid) => new()
    {
        Baid = baid,
        Title = string.Empty,
        TitleplateId = 0,
        ColorFace = 0,
        ColorBody = 1,
        ColorLimb = 3,
        Costume1 = 0,
        Costume2 = 0,
        Costume3 = 0,
        Costume4 = 0,
        Costume5 = 0,
        CostumeFlg1 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        CostumeFlg2 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        CostumeFlg3 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        CostumeFlg4 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        CostumeFlg5 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        ToneFlg = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.ToneFlagBytes),
        TitleFlg = new byte[BlueProtocolBytes.TitleFlagBytes],
        OptionFlg = [],
        DefaultOptionSetting = new byte[2],
        DefaultShinSetting = false,
        DefaultToneSetting = 0,
        DispDanType = 1,
        GotDanMax = 0,
        GotDanFlg = new byte[BlueProtocolBytes.DanFlagBytes],
        GotDanExtraFlg = new byte[BlueProtocolBytes.DanExtraFlagBytes],
        DispTaikojukuDan = 0,
        IsAutoCostumeOn = true,
        IsDevil = false,
        IsExplain = false,
        IsChallengeCompe = false,
        IsTojiru = true,
        LastPlayDatetime = DateTime.UnixEpoch
    };
}
```

- [ ] **Step 6: Add the EF migration**

Run:

```powershell
dotnet ef migrations add AddBlueIdentityProfileSupport --project Infrastructure/Infrastructure.csproj --startup-project Host/Host.csproj --output-dir Persistence/Migrations
```

Expected: PASS and EF creates a migration class named `AddBlueIdentityProfileSupport` plus a designer file, and updates `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`. Inspect the generated migration and confirm it creates table `UserSaveData_Blue` with primary key `Baid` and foreign key to `UserData`.

- [ ] **Step 7: Add the temporary Blue handler fixture required by the tests**

Create `Tests/Blue/BlueHandlerFixture.cs` with a minimal fixture. This fixture compiles before handler tests and will be reused by later tasks:

```csharp
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Tests.Blue;

internal sealed class BlueHandlerFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    private BlueHandlerFixture(SqliteConnection connection, TaikoDbContext context, IGameDataCatalog catalog)
    {
        this.connection = connection;
        Context = context;
        Catalog = catalog;
    }

    public TaikoDbContext Context { get; }

    public IGameDataCatalog Catalog { get; }

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

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }

    internal sealed class TestBlueCatalog : IBlueCatalog
    {
        private readonly IReadOnlyList<BlueMusicInfoEntry> musicInfoFileOrder;
        private readonly IReadOnlyList<BlueTaikojukuEntry> taikojukuFileOrder;

        public TestBlueCatalog(
            IReadOnlyDictionary<uint, EventFolderData>? eventFolders = null,
            IReadOnlyDictionary<uint, BlueTelopEntry>? telops = null,
            IReadOnlyList<BlueMusicInfoEntry>? musicInfoFileOrder = null,
            IReadOnlyList<BlueTaikojukuEntry>? taikojukuFileOrder = null,
            BlueItemShopCatalog? itemShopCatalog = null,
            BlueRecommendEntry? recommend = null)
        {
            EventFolders = eventFolders ?? new Dictionary<uint, EventFolderData>();
            Telops = telops ?? new Dictionary<uint, BlueTelopEntry>();
            this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
            this.taikojukuFileOrder = taikojukuFileOrder ?? DefaultTaikojukuFileOrder;
            ItemShopCatalog = itemShopCatalog ?? BlueItemShopCatalog.Disabled;
            ItemShop = ItemShopCatalog.ActiveItemsByNo;
            Recommend = recommend ?? BlueRecommendEntry.Empty;
        }

        public GameEra Era => GameEra.Blue;

        public uint SongHashVersion => 456;

        public IReadOnlyList<BlueMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyDictionary<uint, BlueMusicInfoEntry> BlueMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyList<BlueTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

        public IReadOnlyDictionary<uint, BlueTaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(pack => pack.UniqueId);

        public BlueItemShopCatalog ItemShopCatalog { get; }

        public IReadOnlyDictionary<uint, BlueItemShopEntry> ItemShop { get; }

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

        public IReadOnlyDictionary<uint, BlueTelopEntry> Telops { get; }

        public IReadOnlyDictionary<uint, BlueGachaEntry> Gachas { get; } = new Dictionary<uint, BlueGachaEntry>();

        public IReadOnlyDictionary<uint, BlueTournamentEntry> Tournaments { get; } = new Dictionary<uint, BlueTournamentEntry>();

        public BlueRecommendEntry Recommend { get; }

        public IReadOnlyList<MovieData> Movies { get; init; } = [];

        public IReadOnlyList<Costume> CostumeList { get; init; } =
        [
            new() { CostumeId = 0, CostumeType = "kigurumi" },
            new() { CostumeId = 1, CostumeType = "head" },
            new() { CostumeId = 2, CostumeType = "body" },
            new() { CostumeId = 3, CostumeType = "face" },
            new() { CostumeId = 4, CostumeType = "puchi" }
        ];

        public IReadOnlyDictionary<uint, Title> TitleDictionary { get; init; } =
            new Dictionary<uint, Title>
            {
                [10] = new() { TitleId = 10, TitleName = "Blue Title", TitleRarity = 0 }
            };

        public IReadOnlyDictionary<uint, Neiro> NeiroDictionary { get; init; } =
            new Dictionary<uint, Neiro>
            {
                [0] = new() { NeiroId = 0, NeiroName = "Taiko" },
                [4] = new() { NeiroId = 4, NeiroName = "Tone 4" }
            };

        public IReadOnlyList<Costume> GetCostumeList() => CostumeList;

        public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => TitleDictionary;

        public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => NeiroDictionary;

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static IReadOnlyList<BlueMusicInfoEntry> DefaultMusicInfoFileOrder { get; } =
        [
            new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
            new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
            new() { SongNo = 103, MusicId = "c", FileOrder = 2 }
        ];

        private static IReadOnlyList<BlueTaikojukuEntry> DefaultTaikojukuFileOrder { get; } =
        [
            new()
            {
                UniqueId = 20001,
                ChallengeLevel = 1,
                VerupNo = 0,
                Songs =
                [
                    new() { SongNo = 101, Level = 0 },
                    new() { SongNo = 102, Level = 0 },
                    new() { SongNo = 103, Level = 0 }
                ]
            }
        ];
    }
}
```

- [ ] **Step 8: Run the save-data tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueSaveDataTests
```

Expected: PASS.

- [ ] **Step 9: Commit Task 1**

Run:

```powershell
git add -- Domain/Entities/UserSaveDataBlue.cs Application/Abstractions/ITaikoDbContext.Blue.cs Application/Common/BlueProtocolBytes.cs Application/Common/UserSaveDataBlueExtensions.cs Infrastructure/Persistence/TaikoDbContext.cs Infrastructure/Persistence/TaikoDbContext.Blue.cs Infrastructure/Persistence/Migrations Tests/Blue/BlueSaveDataTests.cs Tests/Blue/BlueHandlerFixture.cs
git commit -m "Add Blue profile save persistence"
```

## Task 2: Blue BAID And MyDon Entry Handlers

**Files:**
- Create: `Tests/Blue/BlueIdentityHandlerTests.cs`
- Create: `Application/Handlers/BaidQuery.Blue.cs`
- Create: `Application/Handlers/AddMyDonEntryCommand.Blue.cs`
- Modify: `Application/Common/CatalogExtensions.cs`
- Modify: `Application/Handlers/BaidQuery.cs`
- Modify: `Application/Handlers/AddMyDonEntryCommand.cs`

- [ ] **Step 1: Write failing Blue identity handler tests**

Create `Tests/Blue/BlueIdentityHandlerTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueIdentityHandlerTests
{
    [Fact]
    public async Task BaidQuery_Blue_UnknownCardReturnsNewUserWithoutWrites()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Blue, "12345678901234567890"), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.True(response.IsNewUser);
        Assert.Equal(1u, response.Baid);
        Assert.Empty(await fixture.Context.Cards.ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
    }

    [Fact]
    public async Task AddMyDonEntry_Blue_CreatesSharedIdentityAndBlueSaveOnly()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Blue, "12345678901234567890", "BLUE", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.Baid);
        Assert.Equal("BLUE", response.MydonName);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserData.FindAsync(1u));
        Assert.NotNull(await fixture.Context.Credentials.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(1u));
        Assert.Empty(await fixture.Context.SongBestDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenRecentSongs.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task BaidQuery_Blue_KnownCardWithBlueSaveReturnsProfile()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(7);
        save.Title = "Blue Title";
        save.TitleplateId = 10;
        save.ToneFlg = BlueProtocolBytes.CreateFixedBitset([0, 4], BlueProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Blue, "999"), CancellationToken.None);

        Assert.False(response.IsNewUser);
        Assert.Equal(7u, response.Baid);
        Assert.Equal("DON", response.MyDonName);
        Assert.Equal("Blue Title", response.Title);
        Assert.Equal(0u, response.TitlePlateId);
        Assert.Equal(BlueProtocolBytes.DanFlagBytes, response.GotDanFlg.Length);
        Assert.Equal(BlueProtocolBytes.DanExtraFlagBytes, response.GotDanExtraFlg!.Length);
        Assert.True(response.IsAutoCostumeOn.GetValueOrDefault());
    }

    [Fact]
    public async Task BaidQuery_Blue_SharedIdentityWithoutBlueSaveIsNewForBlueRegistration()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 8, MyDonName = "GREEN" });
        fixture.Context.Cards.Add(new Card { Baid = 8, AccessCode = "888" });
        fixture.Context.Credentials.Add(new Credential { Baid = 8, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(8));
        await fixture.Context.SaveChangesAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Blue, "888"), CancellationToken.None);

        Assert.True(response.IsNewUser);
        Assert.Equal(8u, response.Baid);
        Assert.Null(await fixture.Context.UserSaveDataBlue.FindAsync(8u));
    }

    [Fact]
    public async Task AddMyDonEntry_Blue_CompletesExistingSharedIdentityRegistration()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 8, MyDonName = "GREEN" });
        fixture.Context.Cards.Add(new Card { Baid = 8, AccessCode = "888" });
        fixture.Context.Credentials.Add(new Credential { Baid = 8, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(8));
        await fixture.Context.SaveChangesAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Blue, "888", "BLUE", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(8u, response.Baid);
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(8u));
        Assert.Single(await fixture.Context.Cards.Where(card => card.AccessCode == "888").ToListAsync());
        Assert.Single(await fixture.Context.UserData.Where(user => user.Baid == 8).ToListAsync());
        Assert.Single(await fixture.Context.Credentials.Where(credential => credential.Baid == 8).ToListAsync());
    }
}
```

- [ ] **Step 2: Run the failing identity handler tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueIdentityHandlerTests
```

Expected: FAIL with `Unsupported era: Blue`.

- [ ] **Step 3: Add `Blue()` catalog extension**

Modify `Application/Common/CatalogExtensions.cs`:

```csharp
public static IBlueCatalog Blue(this IGameDataCatalog catalog)
    => (IBlueCatalog)catalog.For(GameEra.Blue);
```

The full file should include existing `Nijiiro()` and `Green()` methods plus the new `Blue()` method.

- [ ] **Step 4: Add Blue dispatch cases**

Modify `Application/Handlers/BaidQuery.cs`:

```csharp
public ValueTask<CommonBaidResponse> Handle(BaidQuery request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};

private partial ValueTask<CommonBaidResponse> HandleBlue(BaidQuery request, CancellationToken cancellationToken);
```

Modify `Application/Handlers/AddMyDonEntryCommand.cs` the same way:

```csharp
public ValueTask<CommonMyDonEntryResponse> Handle(AddMyDonEntryCommand request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};

private partial ValueTask<CommonMyDonEntryResponse> HandleBlue(AddMyDonEntryCommand request, CancellationToken cancellationToken);
```

- [ ] **Step 5: Add the Blue BAID handler**

Create `Application/Handlers/BaidQuery.Blue.cs`:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<CommonBaidResponse> HandleBlue(
        BaidQuery request,
        CancellationToken cancellationToken)
    {
        var card = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
        if (card is null)
        {
            var nextBaid = await context.Cards.Select(existing => existing.Baid)
                .DefaultIfEmpty()
                .MaxAsync(cancellationToken) + 1;

            return new CommonBaidResponse
            {
                Result = 1,
                IsNewUser = true,
                Baid = nextBaid
            };
        }

        var saveData = await context.UserSaveDataBlue.FindAsync([card.Baid], cancellationToken);
        if (saveData is null)
        {
            return new CommonBaidResponse
            {
                Result = 1,
                IsNewUser = true,
                Baid = card.Baid
            };
        }

        var userData = await context.UserData.FindAsync([card.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Blue card baid {card.Baid}.");

        return new CommonBaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = card.Baid,
            MyDonName = userData.MyDonName,
            MyDonNameLanguage = userData.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = ResolveBlueTitlePlateId(saveData),
            ColorFace = saveData.ColorFace,
            ColorBody = saveData.ColorBody,
            ColorLimb = saveData.ColorLimb,
            CostumeData = [saveData.Costume1, saveData.Costume2, saveData.Costume3, saveData.Costume4, saveData.Costume5],
            CostumeFlg1 = BlueProtocolBytes.FixedOrZero(saveData.CostumeFlg1, BlueProtocolBytes.CostumeFlagBytes),
            CostumeFlg2 = BlueProtocolBytes.FixedOrZero(saveData.CostumeFlg2, BlueProtocolBytes.CostumeFlagBytes),
            CostumeFlg3 = BlueProtocolBytes.FixedOrZero(saveData.CostumeFlg3, BlueProtocolBytes.CostumeFlagBytes),
            CostumeFlg4 = BlueProtocolBytes.FixedOrZero(saveData.CostumeFlg4, BlueProtocolBytes.CostumeFlagBytes),
            CostumeFlg5 = BlueProtocolBytes.FixedOrZero(saveData.CostumeFlg5, BlueProtocolBytes.CostumeFlagBytes),
            TotalGetDonmedal = saveData.TotalGetDonmedal,
            TotalUseDonmedal = saveData.TotalUseDonmedal,
            TotalGetKatsumedal = saveData.TotalGetKatsumedal,
            TotalUseKatsumedal = saveData.TotalUseKatsumedal,
            ItemshopTutorialFlg = saveData.ItemshopTutorialFlg,
            IsAutoCostumeOn = saveData.IsAutoCostumeOn,
            DispDanType = saveData.DispDanType == 0 ? 0u : 1u,
            GotDanFlg = BlueProtocolBytes.FixedOrZero(saveData.GotDanFlg, BlueProtocolBytes.DanFlagBytes),
            GotDanMax = Math.Min(saveData.GotDanMax, 25u),
            GotDanExtraFlg = BlueProtocolBytes.FixedOrZero(saveData.GotDanExtraFlg, BlueProtocolBytes.DanExtraFlagBytes),
            DefaultToneSetting = saveData.DefaultToneSetting,
            WaiwaiTutorialFlg = saveData.WaiwaiTutorialFlg,
            LastPlayDatetime = saveData.LastPlayDatetime == DateTime.UnixEpoch
                ? DateTime.Now.ToString(Constants.DateTimeFormat)
                : saveData.LastPlayDatetime.ToString(Constants.DateTimeFormat)
        };
    }

    private uint ResolveBlueTitlePlateId(UserSaveDataBlue saveData)
    {
        return gameDataService.Blue().GetTitleDictionary().TryGetValue(saveData.TitleplateId, out var title)
               && BlueTitleTextMatches(title, saveData.Title)
            ? title.TitleRarity
            : saveData.TitleplateId;
    }

    private static bool BlueTitleTextMatches(Title title, string selectedTitle)
    {
        if (string.IsNullOrWhiteSpace(selectedTitle))
        {
            return false;
        }

        return string.Equals(title.TitleName, selectedTitle, StringComparison.Ordinal)
               || string.Equals(title.TitleNameEN, selectedTitle, StringComparison.Ordinal)
               || string.Equals(title.TitleNameCN, selectedTitle, StringComparison.Ordinal)
               || string.Equals(title.TitleNameKO, selectedTitle, StringComparison.Ordinal);
    }
}
```

- [ ] **Step 6: Add the Blue MyDon entry handler**

Create `Application/Handlers/AddMyDonEntryCommand.Blue.cs`:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class AddMyDonEntryCommandHandler
{
    private partial async ValueTask<CommonMyDonEntryResponse> HandleBlue(
        AddMyDonEntryCommand request,
        CancellationToken cancellationToken)
    {
        var existingCard = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
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
                MyDonName = request.Name,
                MyDonNameLanguage = request.Language
            });
        }
        else
        {
            userData.MyDonName = request.Name;
            userData.MyDonNameLanguage = request.Language;
        }

        if (await context.UserSaveDataBlue.FindAsync([baid], cancellationToken) is null)
        {
            context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(baid));
        }

        if (existingCard is null)
        {
            context.Cards.Add(new Card { AccessCode = request.AccessCode, Baid = baid });
        }

        if (await context.Credentials.FindAsync([baid], cancellationToken) is null)
        {
            context.Credentials.Add(new Credential { Baid = baid, Password = string.Empty, Salt = string.Empty });
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created Blue user {Baid} for access code {AccessCode}", baid, request.AccessCode);

        return new CommonMyDonEntryResponse
        {
            Result = 1,
            Baid = baid,
            MydonName = request.Name,
            MydonNameLanguage = request.Language,
            ComSvrResult = 1,
            AccessCode = request.AccessCode
        };
    }
}
```

- [ ] **Step 7: Run the Blue identity handler tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueIdentityHandlerTests
```

Expected: PASS.

- [ ] **Step 8: Commit Task 2**

Run:

```powershell
git add -- Tests/Blue/BlueIdentityHandlerTests.cs Application/Common/CatalogExtensions.cs Application/Handlers/BaidQuery.cs Application/Handlers/BaidQuery.Blue.cs Application/Handlers/AddMyDonEntryCommand.cs Application/Handlers/AddMyDonEntryCommand.Blue.cs
git commit -m "Add Blue identity handlers"
```

## Task 3: Blue Initial Data Handler And Mapper

**Files:**
- Create: `Tests/Blue/BlueInitialDataTests.cs`
- Create: `Application/Dtos/CommonInitialDataCheckResponse.Blue.cs`
- Create: `Application/Handlers/GetInitialDataQuery.Blue.cs`
- Create: `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs`
- Modify: `Application/Dtos/CommonInitialDataCheckResponse.cs`
- Modify: `Application/Handlers/GetInitialDataQuery.cs`

- [ ] **Step 1: Write failing initial-data tests**

Create `Tests/Blue/BlueInitialDataTests.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueInitialDataTests
{
    [Fact]
    public async Task InitialData_Blue_UnlocksAllCatalogSongsAndLeavesLegalTermsEmpty()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);
        var wire = InitialDataMappers.Map(response);

        Assert.Equal(1u, response.Result);
        Assert.Equal(BlueProtocolBytes.SongFlagBytes, response.DefaultSongFlg.Length);
        foreach (var song in fixture.Catalog.Blue().MusicInfoFileOrder)
        {
            Assert.True(BitIsSet(response.DefaultSongFlg, song.SongNo), $"Expected song {song.SongNo} to be unlocked.");
        }

        Assert.Empty(response.AryBlueLegaltermsDatas);
        Assert.Empty(wire.AryLegaltermsDatas);
        Assert.False(wire.ShouldSerializeIsBattleplay());
        Assert.False(wire.ShouldSerializeReleaseBattleStageFlg());
        Assert.False(wire.ShouldSerializeReleaseBattleSpecialFlg());
        Assert.False(wire.ShouldSerializeBattleBondsLvCap());
    }

    [Fact]
    public async Task InitialData_Blue_MapsCatalogInformationArrays()
    {
        var blueCatalog = new BlueHandlerFixture.TestBlueCatalog(
            eventFolders: new Dictionary<uint, EventFolderData>
            {
            [3] = new() { FolderId = 3, VerupNo = 9, SongNoes = [101, 102] }
            },
            telops: new Dictionary<uint, BlueTelopEntry>
            {
                [7] = new() { TelopId = 7, VerupNo = 4 }
            });
        await using var fixture = await BlueHandlerFixture.CreateAsync(blueCatalog);
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);

        Assert.Contains(response.AryBlueTelopDatas, row => row.InfoId == 7 && row.VerupNo == 4);
        Assert.Contains(response.AryBlueEventFolderDatas, row => row.InfoId == 3 && row.VerupNo == 9);
        Assert.Contains(response.AryBlueTaikojukuDatas, row => row.InfoId == 1 && row.VerupNo == 1);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
```

- [ ] **Step 2: Run the failing initial-data tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialDataTests
```

Expected: FAIL because `GetInitialDataQuery` does not support Blue, `CommonInitialDataCheckResponse` has no Blue information arrays, and the Blue mapper does not exist.

- [ ] **Step 3: Make initial-data DTO extensible and add Blue fields**

Modify `Application/Dtos/CommonInitialDataCheckResponse.cs`:

```csharp
public partial class CommonInitialDataCheckResponse
```

Create `Application/Dtos/CommonInitialDataCheckResponse.Blue.cs`:

```csharp
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonInitialDataCheckResponse
{
    public bool? IsBattleplay { get; set; }

    public byte[]? ReleaseBattleStageFlg { get; set; }

    public byte[]? ReleaseBattleSpecialFlg { get; set; }

    public uint? BattleBondsLvCap { get; set; }

    public List<InformationData> AryBlueTelopDatas { get; set; } = [];

    public List<InformationData> AryBlueEventFolderDatas { get; set; } = [];

    public List<InformationData> AryBlueTaikojukuDatas { get; set; } = [];

    public List<InformationData> AryBlueItemShopDatas { get; set; } = [];

    public List<InformationData> AryBlueLegaltermsDatas { get; set; } = [];
}
```

- [ ] **Step 4: Add Blue dispatch to the initial-data handler**

Modify `Application/Handlers/GetInitialDataQuery.cs`:

```csharp
public ValueTask<CommonInitialDataCheckResponse> Handle(GetInitialDataQuery request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};

private partial ValueTask<CommonInitialDataCheckResponse> HandleBlue(GetInitialDataQuery request, CancellationToken cancellationToken);
```

- [ ] **Step 5: Add the Blue initial-data handler**

Create `Application/Handlers/GetInitialDataQuery.Blue.cs`:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleBlue(GetInitialDataQuery request, CancellationToken cancellationToken)
    {
        var blue = gameDataService.Blue();
        var activeShop = blue.ItemShopCatalog.ActiveSeason;
        var shopSongIds = blue.ItemShopCatalog.IsEnabled && activeShop is not null
            ? activeShop.Items.Where(item => item.ItemType == 1).Select(item => item.ItemId).ToHashSet()
            : [];
        var allSongs = blue.MusicInfoFileOrder
            .Select(song => song.SongNo)
            .Where(songNo => !shopSongIds.Contains(songNo));

        return ValueTask.FromResult(new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = BlueProtocolBytes.CreateFixedBitset(allSongs, BlueProtocolBytes.SongFlagBytes),
            AchievementSongBit = new byte[BlueProtocolBytes.SongFlagBytes],
            UraReleaseBit = new byte[BlueProtocolBytes.SongFlagBytes],
            SongHashVer = blue.SongHashVersion,
            IsDanplay = true,
            IsClose = false,
            IsItemshop = blue.ItemShopCatalog.IsEnabled && activeShop is not null && activeShop.Items.Count > 0,
            AryBlueItemShopDatas = activeShop is null
                ? []
                :
                [
                    new CommonInitialDataCheckResponse.InformationData
                    {
                        InfoId = activeShop.SeasonId,
                        VerupNo = activeShop.VerupNo
                    }
                ],
            AryBlueTelopDatas = blue.Telops.Values
                .OrderBy(entry => entry.TelopId)
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.TelopId,
                    VerupNo = entry.VerupNo
                })
                .ToList(),
            AryBlueEventFolderDatas = blue.EventFolders.Values
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.FolderId,
                    VerupNo = entry.VerupNo
                })
                .ToList(),
            AryBlueTaikojukuDatas = blue.TaikojukuFileOrder
                .Where(entry => entry.ChallengeLevel is >= 1 and <= 25)
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.ChallengeLevel,
                    VerupNo = entry.VerupNo + 1
                })
                .ToList(),
            AryBlueLegaltermsDatas = [],
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        });
    }
}
```

- [ ] **Step 6: Add the Blue initial-data mapper**

Create `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs`:

```csharp
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    public static InitialdatacheckResponse Map(CommonInitialDataCheckResponse common)
    {
        var response = new InitialdatacheckResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashDefaultSongFlg = common.DefaultSongFlg,
            HashMainichidojoAll = common.AchievementSongBit,
            HashMainichidojoRare = common.UraReleaseBit,
            IsDanplay = common.IsDanplay,
            IsClose = common.IsClose,
            IsItemshop = common.IsItemshop
        };

        response.AryTelopDatas.AddRange(common.AryBlueTelopDatas.Select(MapInformation));
        response.AryEventfolderDatas.AddRange(common.AryBlueEventFolderDatas.Select(MapInformation));
        response.AryTaikojukuDatas.AddRange(common.AryBlueTaikojukuDatas.Select(MapInformation));
        response.AryItemshopDatas.AddRange(common.AryBlueItemShopDatas.Select(MapInformation));
        response.AryLegaltermsDatas.AddRange(common.AryBlueLegaltermsDatas.Select(MapInformation));

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

        return response;
    }

    private static InitialdatacheckResponse.InformationData MapInformation(
        CommonInitialDataCheckResponse.InformationData common)
        => new()
        {
            InfoId = common.InfoId,
            VerupNo = common.VerupNo
        };
}
```

- [ ] **Step 7: Run the initial-data tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialDataTests
```

Expected: PASS.

- [ ] **Step 8: Commit Task 3**

Run:

```powershell
git add -- Tests/Blue/BlueInitialDataTests.cs Application/Dtos/CommonInitialDataCheckResponse.cs Application/Dtos/CommonInitialDataCheckResponse.Blue.cs Application/Handlers/GetInitialDataQuery.cs Application/Handlers/GetInitialDataQuery.Blue.cs Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs
git commit -m "Add Blue initial data handler"
```

## Task 4: Blue UserData Handler And Mapper

**Files:**
- Create: `Tests/Blue/BlueUserDataTests.cs`
- Create: `Tests/Blue/BlueMapperTests.cs`
- Create: `Application/Dtos/CommonUserDataResponse.Blue.cs`
- Create: `Application/Handlers/UserDataQuery.Blue.cs`
- Create: `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs`
- Create: `Adapters.GameProtocol.Blue/Mappers/BaidResponseMapper.cs`
- Modify: `Application/Handlers/UserDataQuery.cs`

- [ ] **Step 1: Write failing userdata and mapper tests**

Create `Tests/Blue/BlueUserDataTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueUserDataTests
{
    [Fact]
    public async Task UserData_Blue_UnlocksAllCatalogSongsAndReturnsEmptyFavoriteRecentArrays()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 9, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(9));
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(9, GameEra.Blue), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(BlueProtocolBytes.SongFlagBytes, response.ReleaseSongFlg.Length);
        foreach (var song in fixture.Catalog.Blue().MusicInfoFileOrder)
        {
            Assert.True(BitIsSet(response.ReleaseSongFlg, song.SongNo), $"Expected song {song.SongNo} to be unlocked.");
        }

        Assert.Empty(response.AryFavoriteSongNoes);
        Assert.Empty(response.AryRecentSongNoes);
    }

    [Fact]
    public async Task UserData_Blue_ReturnsPersistedToneTitleAndDisplaySettings()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.ToneFlg = BlueProtocolBytes.CreateFixedBitset([0, 4], BlueProtocolBytes.ToneFlagBytes);
        save.TitleFlg = BlueProtocolBytes.CreateFixedBitset([10, 131], BlueProtocolBytes.TitleFlagBytes);
        save.IsTojiru = false;
        save.DispLevelTotal = 2;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 4;
        save.IsDevil = true;
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.Equal(BlueProtocolBytes.ToneFlagBytes, response.ToneFlg.Length);
        Assert.Equal(BlueProtocolBytes.TitleFlagBytes, response.TitleFlg.Length);
        Assert.True(BitIsSet(response.ToneFlg, 4));
        Assert.True(BitIsSet(response.TitleFlg, 10));
        Assert.False(response.IsTojiru);
        Assert.Equal(2u, response.DispLevelTotal);
        Assert.Equal(3u, response.DispLevelChassis);
        Assert.Equal(4u, response.DispLevelSelf);
        Assert.True(response.IsDevilBlue);
    }

    [Fact]
    public async Task UserData_Blue_RecommendComesFromCatalog()
    {
        var blueCatalog = new BlueHandlerFixture.TestBlueCatalog(
            recommend: new TaikoLocalServer.Application.Catalog.Blue.BlueRecommendEntry
            {
                RecommendSong = 102,
                RecommendBestSongs = [101, 102, 103]
            });
        await using var fixture = await BlueHandlerFixture.CreateAsync(blueCatalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.Equal(102u, response.RecommendSong);
        Assert.Equal(new List<uint> { 101, 102, 103 }, response.RecommendBestSong);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
```

Create `Tests/Blue/BlueMapperTests.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueMapperTests
{
    [Fact]
    public void UserDataMapper_Blue_OmitsTokkunTutorialFlag()
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            ReleaseSongFlg = new byte[BlueProtocolBytes.SongFlagBytes],
            ToneFlg = new byte[BlueProtocolBytes.ToneFlagBytes],
            TitleFlg = new byte[BlueProtocolBytes.TitleFlagBytes],
            DefaultOptionSetting = new byte[2],
            DispTaikojukuDan = 1
        });

        Assert.False(response.ShouldSerializeTokkunTutorialFlg());
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(26u)]
    [InlineData(20001u)]
    public void UserDataMapper_Blue_FallsBackToSentinelOneForInvalidDispTaikojukuDan(uint dispTaikojukuDan)
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            DispTaikojukuDan = dispTaikojukuDan
        });

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(1u, response.DispTaikojukuDan);
    }

    [Fact]
    public void BaidMapper_Blue_UsesBlueFixedWidthFallbacks()
    {
        var response = BaidResponseMapper.Map(new CommonBaidResponse
        {
            Result = 1,
            Baid = 3,
            MyDonName = "DON"
        });

        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg1.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg2.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg3.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg4.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg5.Length);
        Assert.Equal(BlueProtocolBytes.DanFlagBytes, response.GotDanFlg.Length);
        Assert.Equal(BlueProtocolBytes.DanExtraFlagBytes, response.GotDanextraFlg.Length);
        Assert.Equal(BlueProtocolBytes.ContentInfoBytes, response.ContentInfo.Length);
    }
}
```

- [ ] **Step 2: Run the failing userdata tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BlueMapperTests"
```

Expected: FAIL because the Blue userdata dispatch, DTO property `IsDevilBlue`, and Blue mappers do not exist.

- [ ] **Step 3: Add Blue userdata DTO field and dispatch**

Create `Application/Dtos/CommonUserDataResponse.Blue.cs`:

```csharp
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonUserDataResponse
{
    public bool? IsDevilBlue { get; set; }
}
```

Modify `Application/Handlers/UserDataQuery.cs`:

```csharp
public ValueTask<CommonUserDataResponse> Handle(UserDataQuery request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};

private partial ValueTask<CommonUserDataResponse> HandleBlue(UserDataQuery request, CancellationToken cancellationToken);
```

- [ ] **Step 4: Add the Blue userdata handler**

Create `Application/Handlers/UserDataQuery.Blue.cs`:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<CommonUserDataResponse> HandleBlue(
        UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Blue baid {request.Baid}.");
        var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
        var blue = gameDataService.Blue();

        return new CommonUserDataResponse
        {
            Result = 1,
            SongHashVer = blue.SongHashVersion,
            ReleaseSongFlg = BlueProtocolBytes.CreateFixedBitset(
                blue.MusicInfoFileOrder.Select(song => song.SongNo),
                BlueProtocolBytes.SongFlagBytes),
            ToneFlg = BlueProtocolBytes.FixedOrZero(saveData.ToneFlg, BlueProtocolBytes.ToneFlagBytes),
            TitleFlg = BlueProtocolBytes.FixedOrZero(saveData.TitleFlg, BlueProtocolBytes.TitleFlagBytes),
            DefaultOptionSetting = BlueProtocolBytes.FixedOrZero(saveData.DefaultOptionSetting, 2),
            OptionFlg = saveData.OptionFlg,
            AryFavoriteSongNoes = [],
            AryRecentSongNoes = [],
            CategJpopCnt = saveData.CategJpopCnt,
            CategAnimeCnt = saveData.CategAnimeCnt,
            CategDoyoCnt = saveData.CategDoyoCnt,
            CategVarietyCnt = saveData.CategVarietyCnt,
            CategClassicCnt = saveData.CategClassicCnt,
            CategGameCnt = saveData.CategGameCnt,
            CategNamcoCnt = saveData.CategNamcoCnt,
            CategVocaloidCnt = saveData.CategVocaloidCnt,
            SongPushedCnt = saveData.SongPushedCnt,
            RecommendSong = blue.Recommend.RecommendSong,
            RecommendBestSong = blue.Recommend.RecommendBestSongs.ToList(),
            SongFavoriteCnt = saveData.SongFavoriteCnt,
            SongRecentCnt = saveData.SongRecentCnt,
            TotalCreditCnt = saveData.TotalCreditCnt,
            PrevAreaCode = saveData.PrevAreaCode,
            ConsecAreaCnt = saveData.ConsecAreaCnt,
            DefaultShinSetting = saveData.DefaultShinSetting,
            DispLevelTotal = saveData.DispLevelTotal,
            DispLevelChassis = saveData.DispLevelChassis,
            DispLevelSelf = saveData.DispLevelSelf,
            DispTaikojukuDan = GetSafeBlueTaikojukuDanSlot(saveData.DispTaikojukuDan),
            DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
            DifficultyPlayedStar = saveData.DifficultyPlayedStar,
            IsChallengeCompe = saveData.IsChallengeCompe,
            IsTojiru = saveData.IsTojiru,
            IsDevilBlue = saveData.IsDevil
        };
    }

    private static uint GetSafeBlueTaikojukuDanSlot(uint value)
        => value is >= 1 and <= 25 ? value : 1u;
}
```

- [ ] **Step 5: Add the Blue BAID mapper**

Create `Adapters.GameProtocol.Blue/Mappers/BaidResponseMapper.cs`:

```csharp
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class BaidResponseMapper
{
    public static BAIDResponse Map(CommonBaidResponse common)
    {
        return new BAIDResponse
        {
            Result = common.Result,
            Baid = common.Baid,
            MydonName = common.MyDonName,
            Title = common.Title,
            TitleplateId = common.TitlePlateId,
            ColorFace = common.ColorFace,
            ColorBody = common.ColorBody,
            ColorLimb = common.ColorLimb,
            AryCostumedata = new BAIDResponse.CostumeData
            {
                Costume1 = common.CostumeData.ElementAtOrDefault(0),
                Costume2 = common.CostumeData.ElementAtOrDefault(1),
                Costume3 = common.CostumeData.ElementAtOrDefault(2),
                Costume4 = common.CostumeData.ElementAtOrDefault(3),
                Costume5 = common.CostumeData.ElementAtOrDefault(4)
            },
            CostumeFlg1 = common.CostumeFlg1 ?? new byte[BlueProtocolBytes.CostumeFlagBytes],
            CostumeFlg2 = common.CostumeFlg2 ?? new byte[BlueProtocolBytes.CostumeFlagBytes],
            CostumeFlg3 = common.CostumeFlg3 ?? new byte[BlueProtocolBytes.CostumeFlagBytes],
            CostumeFlg4 = common.CostumeFlg4 ?? new byte[BlueProtocolBytes.CostumeFlagBytes],
            CostumeFlg5 = common.CostumeFlg5 ?? new byte[BlueProtocolBytes.CostumeFlagBytes],
            TotalGetDonmedal = common.TotalGetDonmedal.GetValueOrDefault(),
            TotalUseDonmedal = common.TotalUseDonmedal.GetValueOrDefault(),
            TotalGetKatsumedal = common.TotalGetKatsumedal.GetValueOrDefault(),
            TotalUseKatsumedal = common.TotalUseKatsumedal.GetValueOrDefault(),
            ItemshopTutorialFlg = common.ItemshopTutorialFlg.GetValueOrDefault(),
            IsAutoCostumeOn = common.IsAutoCostumeOn.GetValueOrDefault(),
            LastPlayDatetime = common.LastPlayDatetime,
            DispDanType = common.DispDanType.GetValueOrDefault(),
            GotDanMax = common.GotDanMax,
            GotDanFlg = common.GotDanFlg ?? new byte[BlueProtocolBytes.DanFlagBytes],
            GotDanextraFlg = common.GotDanExtraFlg ?? new byte[BlueProtocolBytes.DanExtraFlagBytes],
            ContentInfo = new byte[BlueProtocolBytes.ContentInfoBytes],
            DefaultToneSetting = common.DefaultToneSetting.GetValueOrDefault(),
            Personid = common.PersonId ?? string.Empty,
            WaiwaiTutorialFlg = common.WaiwaiTutorialFlg.GetValueOrDefault()
        };
    }
}
```

- [ ] **Step 6: Add the Blue userdata mapper**

Create `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs`:

```csharp
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    public static UserDataResponse Map(CommonUserDataResponse common)
    {
        var response = new UserDataResponse
        {
            Result = common.Result,
            AryFavoriteSongNoes = common.AryFavoriteSongNoes,
            AryRecentSongNoes = common.AryRecentSongNoes,
            SongHashVer = common.SongHashVer,
            HashReleaseSongFlg = common.ReleaseSongFlg,
            OptionFlg = common.OptionFlg,
            ToneFlg = common.ToneFlg,
            TitleFlg = common.TitleFlg,
            CategJpopCnt = common.CategJpopCnt.GetValueOrDefault(),
            CategAnimeCnt = common.CategAnimeCnt.GetValueOrDefault(),
            CategDoyoCnt = common.CategDoyoCnt.GetValueOrDefault(),
            CategVarietyCnt = common.CategVarietyCnt.GetValueOrDefault(),
            CategClassicCnt = common.CategClassicCnt.GetValueOrDefault(),
            CategGameCnt = common.CategGameCnt.GetValueOrDefault(),
            CategNamcoCnt = common.CategNamcoCnt.GetValueOrDefault(),
            CategVocaloidCnt = common.CategVocaloidCnt.GetValueOrDefault(),
            SongPushedCnt = common.SongPushedCnt.GetValueOrDefault(),
            SongFavoriteCnt = common.SongFavoriteCnt.GetValueOrDefault(),
            PrevAreaCode = common.PrevAreaCode.GetValueOrDefault(),
            ConsecAreaCnt = common.ConsecAreaCnt.GetValueOrDefault(),
            RecommendSong = common.RecommendSong.GetValueOrDefault(),
            RecommendBestSongs = common.RecommendBestSong.ToArray(),
            TotalCreditCnt = common.TotalCreditCnt,
            SongRecentCnt = common.SongRecentCnt,
            DefaultOptionSetting = common.DefaultOptionSetting,
            DefaultShinSetting = common.DefaultShinSetting.GetValueOrDefault(),
            DispLevelTotal = common.DispLevelTotal,
            DispLevelChassis = common.DispLevelChassis,
            DispLevelSelf = common.DispLevelSelf,
            DifficultyPlayedCourse = common.DifficultyPlayedCourse,
            DifficultyPlayedStar = common.DifficultyPlayedStar,
            IsChallengecompe = common.IsChallengeCompe.GetValueOrDefault(),
            IsTojiru = common.IsTojiru.GetValueOrDefault(),
            IsDevil = common.IsDevilBlue.GetValueOrDefault()
        };

        response.DispTaikojukuDan = common.DispTaikojukuDan is { } dispTaikojukuDan
                                    && dispTaikojukuDan is >= 1 and <= 25
            ? dispTaikojukuDan
            : 1u;

        return response;
    }
}
```

- [ ] **Step 7: Run the userdata and mapper tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BlueMapperTests"
```

Expected: PASS.

- [ ] **Step 8: Commit Task 4**

Run:

```powershell
git add -- Tests/Blue/BlueUserDataTests.cs Tests/Blue/BlueMapperTests.cs Application/Dtos/CommonUserDataResponse.Blue.cs Application/Handlers/UserDataQuery.cs Application/Handlers/UserDataQuery.Blue.cs Adapters.GameProtocol.Blue/Mappers/BaidResponseMapper.cs Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs
git commit -m "Add Blue userdata readback"
```

## Task 5: Blue Controllers And A1 Route Guard Update

**Files:**
- Modify: `Adapters.GameProtocol.Blue/GlobalUsings.cs`
- Modify: `Adapters.GameProtocol.Blue/Controllers/BaidController.cs`
- Modify: `Adapters.GameProtocol.Blue/Controllers/MyDonEntryController.cs`
- Modify: `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs`
- Modify: `Adapters.GameProtocol.Blue/Controllers/UserDataController.cs`
- Modify: `Tests/Blue/BlueRouteSkeletonTests.cs`

- [ ] **Step 1: Update Blue adapter global usings**

Modify `Adapters.GameProtocol.Blue/GlobalUsings.cs` to include the application and mapper namespaces used by the real controllers:

```csharp
global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using ProtoBuf;
global using Swan.Formatters;
global using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
global using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Application.Common;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Handlers;
global using TaikoLocalServer.Domain.Enums;
```

- [ ] **Step 2: Update BAID controller**

Replace `Adapters.GameProtocol.Blue/Controllers/BaidController.cs` action with:

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> Baid([FromBody] BAIDRequest request)
{
    Logger.LogInformation("Blue BAID request: {Request}", request.Stringify());
    var common = await Mediator.Send(new BaidQuery(GameEra.Blue, request.AccessCode), HttpContext.RequestAborted);

    if (common.IsNewUser)
    {
        Logger.LogInformation("New Blue user with access code {AccessCode}", request.AccessCode);

        return Ok(new BAIDResponse
        {
            Result = 1,
            PlayerType = 1,
            Baid = common.Baid
        });
    }

    var response = BaidResponseMapper.Map(common);
    response.AccessCode = request.AccessCode;
    response.IsPublish = true;
    response.PlayerType = 0;

    return Ok(response);
}
```

- [ ] **Step 3: Update MyDon entry controller**

Replace `Adapters.GameProtocol.Blue/Controllers/MyDonEntryController.cs` action with:

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> MyDonEntry([FromBody] MydonEntryRequest request)
{
    Logger.LogInformation("Blue MyDonEntry request: {Request}", request.Stringify());

    var common = await Mediator.Send(
        new AddMyDonEntryCommand(GameEra.Blue, request.AccessCode, request.MydonName, 0),
        HttpContext.RequestAborted);

    return Ok(new MydonEntryResponse
    {
        Result = common.Result,
        ComSvrResult = common.ComSvrResult,
        Baid = common.Baid,
        AccessCode = common.AccessCode,
        MydonName = common.MydonName,
        IsPublish = true,
        ContentInfo = new byte[BlueProtocolBytes.ContentInfoBytes]
    });
}
```

- [ ] **Step 4: Update initial-data controller**

Replace `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs` action with:

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
{
    Logger.LogInformation("Blue InitialDataCheck request: {Request}", request.Stringify());
    var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Blue), HttpContext.RequestAborted);
    return Ok(InitialDataMappers.Map(common));
}
```

- [ ] **Step 5: Update userdata controller**

Replace `Adapters.GameProtocol.Blue/Controllers/UserDataController.cs` action with:

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
{
    Logger.LogInformation("Blue UserData request: {Request}", request.Stringify());
    var common = await Mediator.Send(new UserDataQuery(request.Baid, GameEra.Blue), HttpContext.RequestAborted);
    return Ok(UserDataMappers.Map(common));
}
```

- [ ] **Step 6: Update the Blue route skeleton mediator guard**

Modify `Tests/Blue/BlueRouteSkeletonTests.cs` so the mediator guard allows only the four A3 controllers:

```csharp
[Fact]
public void BlueStubControllers_DoNotCallMediatorOutsideA3ProfileEndpoints()
{
    var root = FindRepoRoot();
    var controllersRoot = Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers");
    var mediatorBackedControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "BaidController.cs",
        "MyDonEntryController.cs",
        "InitialDataCheckController.cs",
        "UserDataController.cs"
    };

    foreach (var file in Directory.EnumerateFiles(controllersRoot, "*.cs", SearchOption.AllDirectories))
    {
        if (mediatorBackedControllers.Contains(Path.GetFileName(file)))
        {
            continue;
        }

        var source = File.ReadAllText(file);
        Assert.DoesNotContain("Mediator.Send", source, StringComparison.Ordinal);
    }
}
```

- [ ] **Step 7: Run route and Blue tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueRouteSkeletonTests|FullyQualifiedName~BlueIdentityHandlerTests|FullyQualifiedName~BlueInitialDataTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BlueMapperTests"
```

Expected: PASS.

- [ ] **Step 8: Commit Task 5**

Run:

```powershell
git add -- Adapters.GameProtocol.Blue/GlobalUsings.cs Adapters.GameProtocol.Blue/Controllers/BaidController.cs Adapters.GameProtocol.Blue/Controllers/MyDonEntryController.cs Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs Adapters.GameProtocol.Blue/Controllers/UserDataController.cs Tests/Blue/BlueRouteSkeletonTests.cs
git commit -m "Wire Blue profile controllers"
```

## Task 6: Source Guards And Regression Verification

**Files:**
- Create: `Tests/Blue/BlueA3SourceGuardTests.cs`

- [ ] **Step 1: Add Blue A3 source guard tests**

Create `Tests/Blue/BlueA3SourceGuardTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueA3SourceGuardTests
{
    [Fact]
    public void BlueA3ApplicationCode_DoesNotDependOnGreenProtocolState()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Application", "Handlers", "BaidQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "AddMyDonEntryCommand.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "GetInitialDataQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "UserDataQuery.Blue.cs"),
            Path.Combine(root, "Application", "Common", "UserSaveDataBlueExtensions.cs"),
            Path.Combine(root, "Application", "Common", "BlueProtocolBytes.cs")
        };

        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
            Assert.DoesNotContain("IGreenCatalog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("UserSaveDataGreen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenShop", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenGhost", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenDan", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void BlueA3AdapterMappers_DoNotDependOnGreenAdapterTypes()
    {
        var root = FindRepoRoot();
        var mapperRoot = Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers");

        foreach (var file in Directory.EnumerateFiles(mapperRoot, "*.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("Adapters.GameProtocol.Green", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
        }
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
```

- [ ] **Step 2: Run source guard tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA3SourceGuardTests
```

Expected: PASS.

- [ ] **Step 3: Run all Blue tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
```

Expected: PASS. If unrelated untracked A2 tests are present, include them in this run only after A2 implementation has been completed and committed.

- [ ] **Step 4: Run focused Green regression tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenIdentityHandlerTests|FullyQualifiedName~GreenUserDataMapperTests"
```

Expected: PASS.

- [ ] **Step 5: Build Host to a temp output path**

Run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a3"
```

Expected: PASS. This temp output avoids known locked `Host/bin` output failures.

- [ ] **Step 6: Commit Task 6**

Run:

```powershell
git add -- Tests/Blue/BlueA3SourceGuardTests.cs
git commit -m "Add Blue profile source guards"
```

## Task 7: Final Review And Working Tree Hygiene

**Files:**
- Review only: all files changed by Tasks 1 through 6.

- [ ] **Step 1: Inspect committed history for A3**

Run:

```powershell
git log --oneline -6
```

Expected: the newest commits are the A3 task commits from this plan.

- [ ] **Step 2: Inspect working tree**

Run:

```powershell
git status --short
```

Expected: no modified tracked files from A3. Untracked A2 files may still appear if they were present before A3 execution; do not add them to A3 commits.

- [ ] **Step 3: Run final verification commands**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenIdentityHandlerTests|FullyQualifiedName~GreenUserDataMapperTests"
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a3"
```

Expected: all commands PASS.

- [ ] **Step 4: Record final evidence in the implementation response**

Include these facts in the final implementation summary:

```text
Implemented Blue A3 identity/profile/userdata.
Verified:
- dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
- dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenIdentityHandlerTests|FullyQualifiedName~GreenUserDataMapperTests"
- dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a3"
Known exclusions: play results, crowns, rewards, real Dani progression, item purchases, AdminApi/WebUI, Tokkun, Banacoin, and battle mode.
```

## Self-Review Checklist

- Spec coverage: Tasks 1-5 cover Blue save state, BAID, mydon entry, initial data, userdata, mapper optional fields, and controller wiring. Task 6 covers Green-dependency guardrails and verification. Task 7 covers final evidence and dirty-tree hygiene.
- Red-flag scan: The plan uses concrete file names, commands, expected results, and code blocks. EF migration filenames are generated by the `dotnet ef migrations add AddBlueIdentityProfileSupport` command, and the migration class name is fixed.
- Type consistency: The plan uses `UserSaveDataBlue`, `BlueProtocolBytes`, `IBlueCatalog`, `CatalogExtensions.Blue()`, `IsDevilBlue`, `AryBlue*Datas`, and Blue mapper class names consistently across tasks.
