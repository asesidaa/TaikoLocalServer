# 04 - Green Identity, UserData, And InitialData

**Surface:** Replace hard-coded Green identity and user-data stubs with real application handlers and controller/mappers.

**Why after 03:** Identity flow needs default save data, fake score seeding, first-Dan grant, and parsed song unlock sets.

**Files:**
- Modify: `Application/Handlers/BaidQuery.Green.cs`
- Modify: `Application/Handlers/AddMyDonEntryCommand.Green.cs`
- Modify: `Application/Handlers/UserDataQuery.Green.cs`
- Modify: `Application/Handlers/GetInitialDataQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/BaidController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/MyDonEntryController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/UserDataController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/InitialDataCheckController.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/BaidResponseMapper.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/InitialDataMappers.cs`
- Create: `Tests/Green/GreenIdentityHandlerTests.cs`

---

## Task 04.1: Implement Green Registration Handler

**Acceptance Criteria:**
- [ ] `AddMyDonEntryCommand.Green` creates `Card`, `Credential`, `UserDatum`, `UserSaveDataGreen`, and fake best seeds.
- [ ] New response includes generated `baid`, access code, name, and success result.

**Steps:**

- [ ] **Step 1: Add handler test scaffold**

Create `Tests/Green/GreenIdentityHandlerTests.cs` with a SQLite-backed context helper:

```csharp
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Common;
using TaikoLocalServer.Application.Handlers;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;
using TaikoLocalServer.Infrastructure.Persistence;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenIdentityHandlerTests
{
    [Fact]
    public async Task AddMyDonEntry_Green_CreatesIdentitySaveAndSeeds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Green, "12345678901234567890", "DON", 0),
            CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)1, response.Baid);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserSaveDataGreen.FindAsync(1u));
        Assert.NotEmpty(await fixture.Context.SongBestDataGreen.Where(row => row.Baid == 1).ToListAsync());
    }

    private sealed class GreenHandlerFixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        private GreenHandlerFixture(SqliteConnection connection, TaikoDbContext context, IGameDataCatalog catalog)
        {
            this.connection = connection;
            Context = context;
            Catalog = catalog;
        }

        public TaikoDbContext Context { get; }
        public IGameDataCatalog Catalog { get; }

        public static async Task<GreenHandlerFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<TaikoDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new TaikoDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var greenCatalog = new TestGreenCatalog();
            var catalog = new FileGameDataCatalog([greenCatalog]);
            return new GreenHandlerFixture(connection, context, catalog);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class TestGreenCatalog : IGreenCatalog
    {
        public GameEra Era => GameEra.Green;
        public uint SongHashVersion => 123;
        public IReadOnlyList<GreenMusicInfoEntry> MusicInfoFileOrder { get; } =
        [
            new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
            new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
            new() { SongNo = 103, MusicId = "c", FileOrder = 2 },
            new() { SongNo = 104, MusicId = "d", FileOrder = 3 },
            new() { SongNo = 105, MusicId = "e", FileOrder = 4, HasExtreme = true },
            new() { SongNo = 106, MusicId = "f", FileOrder = 5 },
            new() { SongNo = 107, MusicId = "g", FileOrder = 6 },
            new() { SongNo = 108, MusicId = "h", FileOrder = 7 },
            new() { SongNo = 109, MusicId = "i", FileOrder = 8 },
            new() { SongNo = 110, MusicId = "j", FileOrder = 9 },
            new() { SongNo = 111, MusicId = "k", FileOrder = 10 },
            new() { SongNo = 112, MusicId = "l", FileOrder = 11 },
            new() { SongNo = 113, MusicId = "m", FileOrder = 12 },
            new() { SongNo = 114, MusicId = "n", FileOrder = 13 },
            new() { SongNo = 115, MusicId = "o", FileOrder = 14 },
            new() { SongNo = 116, MusicId = "p", FileOrder = 15 },
            new() { SongNo = 117, MusicId = "q", FileOrder = 16 },
            new() { SongNo = 118, MusicId = "r", FileOrder = 17 },
            new() { SongNo = 119, MusicId = "s", FileOrder = 18 },
            new() { SongNo = 120, MusicId = "t", FileOrder = 19 }
        ];

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);
        public IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);
        public IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder { get; } = [];
        public IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku { get; } = new Dictionary<uint, GreenTaikojukuEntry>();
        public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop { get; } = new Dictionary<uint, GreenItemShopEntry>();
        public IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders { get; } = new Dictionary<uint, GreenEventFolderEntry>();
        public IReadOnlyDictionary<uint, GreenTelopEntry> Telops { get; } = new Dictionary<uint, GreenTelopEntry>();
        public IReadOnlyDictionary<uint, GreenGachaEntry> Gachas { get; } = new Dictionary<uint, GreenGachaEntry>();
        public IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments { get; } = new Dictionary<uint, GreenTournamentEntry>();
        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter AddMyDonEntry_Green_CreatesIdentitySaveAndSeeds`

Expected: FAIL because Green handler is still a stub.

- [ ] **Step 3: Implement `AddMyDonEntryCommand.Green.cs`**

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class AddMyDonEntryCommandHandler
{
    private partial async ValueTask<CommonMyDonEntryResponse> HandleGreen(
        AddMyDonEntryCommand request,
        CancellationToken cancellationToken)
    {
        var nextBaid = await context.Cards.Select(card => card.Baid)
            .DefaultIfEmpty()
            .MaxAsync(cancellationToken) + 1;

        context.UserData.Add(new UserDatum
        {
            Baid = nextBaid,
            MyDonName = request.Name,
            MyDonNameLanguage = request.Language
        });

        context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(nextBaid));
        context.Cards.Add(new Card { AccessCode = request.AccessCode, Baid = nextBaid });
        context.Credentials.Add(new Credential { Baid = nextBaid, Password = "", Salt = "" });

        foreach (var seed in GreenSeedDataService.CreateFakeBestSeeds(
            nextBaid,
            gameDataService.Green().MusicInfoFileOrder))
        {
            context.SongBestDataGreen.Add(seed);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CommonMyDonEntryResponse
        {
            Result = 1,
            Baid = nextBaid,
            MydonName = request.Name,
            MydonNameLanguage = request.Language,
            AccessCode = request.AccessCode,
            ComSvrResult = 1
        };
    }
}
```

Update the primary constructor in `AddMyDonEntryCommand.cs` to inject `IGameDataCatalog`:

```csharp
public partial class AddMyDonEntryCommandHandler(
    ITaikoDbContext context,
    ILogger<AddMyDonEntryCommandHandler> logger,
    IGameDataCatalog gameDataService)
```

- [ ] **Step 4: Run focused test**

Run: `dotnet test --filter AddMyDonEntry_Green_CreatesIdentitySaveAndSeeds`

Expected: PASS.

---

## Task 04.2: Implement Green `baidcheck` Handler And Controller

**Acceptance Criteria:**
- [ ] Unknown card returns new-user success with next `baid`.
- [ ] Known card returns persisted save data.
- [ ] First known-card login grants first fake Dan if not already granted.
- [ ] Controller no longer returns hard-coded `Baid = 1`.

**Steps:**

- [ ] **Step 1: Add handler tests**

Append to `GreenIdentityHandlerTests`:

```csharp
[Fact]
public async Task BaidQuery_Green_KnownCardReturnsSaveDataAndGrantsFirstDan()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
    fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(7));
    await fixture.Context.SaveChangesAsync();

    var handler = new BaidQueryHandler(
        fixture.Context,
        NullLogger<BaidQueryHandler>.Instance,
        fixture.Catalog);

    var response = await handler.Handle(new BaidQuery(GameEra.Green, "999"), CancellationToken.None);

    Assert.False(response.IsNewUser);
    Assert.Equal((uint)7, response.Baid);
    Assert.Equal("DON", response.MyDonName);
    Assert.Equal(GreenProtocolBytes.DanFlagBytes, response.GotDanFlg!.Length);
    Assert.Equal(0b0000_0001, response.GotDanFlg[0]);
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter BaidQuery_Green_KnownCardReturnsSaveDataAndGrantsFirstDan`

Expected: FAIL because Green handler is still a stub.

- [ ] **Step 3: Implement `BaidQuery.Green.cs`**

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<CommonBaidResponse> HandleGreen(
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

        var userData = await context.UserData.FindAsync([card.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Green card baid {card.Baid}.");
        var saveData = await context.GetOrCreateGreenSaveDataAsync(card.Baid, cancellationToken);

        if (GreenSeedDataService.GrantFirstFakeDanIfNeeded(saveData))
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return new CommonBaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = card.Baid,
            MyDonName = userData.MyDonName,
            MyDonNameLanguage = userData.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = saveData.TitleplateId,
            ColorFace = saveData.ColorFace,
            ColorBody = saveData.ColorBody,
            ColorLimb = saveData.ColorLimb,
            CostumeData = [saveData.Costume1, saveData.Costume2, saveData.Costume3, saveData.Costume4, saveData.Costume5],
            CostumeFlg1 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg2 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg2, GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg3 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg3, GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg4 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg4, GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg5 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg5, GreenProtocolBytes.CostumeFlagBytes),
            TotalGetDonmedal = saveData.TotalGetDonmedal,
            TotalUseDonmedal = saveData.TotalUseDonmedal,
            TotalGetKatsumedal = saveData.TotalGetKatsumedal,
            TotalUseKatsumedal = saveData.TotalUseKatsumedal,
            ItemshopTutorialFlg = saveData.ItemshopTutorialFlg,
            IsAutoCostumeOn = saveData.IsAutoCostumeOn,
            DispDanType = saveData.DispDanType,
            GotDanFlg = GreenProtocolBytes.FixedOrZero(saveData.GotDanFlg, GreenProtocolBytes.DanFlagBytes),
            GotDanMax = saveData.GotDanMax,
            GotDanExtraFlg = GreenProtocolBytes.FixedOrZero(saveData.GotDanExtraFlg, GreenProtocolBytes.DanExtraFlagBytes),
            DefaultToneSetting = saveData.DefaultToneSetting,
            WaiwaiTutorialFlg = saveData.WaiwaiTutorialFlg
        };
    }
}
```

`CommonBaidResponse.Nijiiro.cs` already owns the shared profile fields used above (`Title`, `TitlePlateId`, `ColorFace`, `CostumeData`, `GotDanFlg`, `GotDanMax`). Reuse those fields for Green rather than adding duplicates.

- [ ] **Step 4: Update `BaidResponseMapper.cs`**

Ensure it maps fixed bytes and profile fields:

```csharp
CostumeFlg1 = common.CostumeFlg1 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
CostumeFlg2 = common.CostumeFlg2 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
CostumeFlg3 = common.CostumeFlg3 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
CostumeFlg4 = common.CostumeFlg4 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
CostumeFlg5 = common.CostumeFlg5 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
GotDanFlg = common.GotDanFlg ?? new byte[GreenProtocolBytes.DanFlagBytes],
GotDanextraFlg = common.GotDanExtraFlg ?? new byte[GreenProtocolBytes.DanExtraFlagBytes],
ContentInfo = new byte[GreenProtocolBytes.ContentInfoBytes],
```

Map current costume to the generated `AryCostumedata` property:

```csharp
AryCostumedata = new BAIDResponse.CostumeData
{
    Costume1 = common.CostumeData.ElementAtOrDefault(0),
    Costume2 = common.CostumeData.ElementAtOrDefault(1),
    Costume3 = common.CostumeData.ElementAtOrDefault(2),
    Costume4 = common.CostumeData.ElementAtOrDefault(3),
    Costume5 = common.CostumeData.ElementAtOrDefault(4)
}
```

- [ ] **Step 5: Update `BaidController.cs`**

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> Baid([FromBody] BAIDRequest request)
{
    Logger.LogInformation("Green Baid request: {Request}", request.Stringify());
    var common = await Mediator.Send(new BaidQuery(GameEra.Green, request.AccessCode), HttpContext.RequestAborted);

    var response = BaidResponseMapper.Map(common);
    response.AccessCode = request.AccessCode;
    response.IsPublish = true;
    response.PlayerType = common.IsNewUser ? 1u : 0u;

    return Ok(response);
}
```

- [ ] **Step 6: Run tests and build**

Run:

```bash
dotnet test --filter BaidQuery_Green_KnownCardReturnsSaveDataAndGrantsFirstDan
dotnet build
```

Expected: both PASS.

---

## Task 04.3: Wire Green `mydonentry` Controller

**Acceptance Criteria:**
- [ ] Controller sends `AddMyDonEntryCommand(GameEra.Green, request.AccessCode, request.MydonName, 0)`.
- [ ] Response includes persisted `baid`.

**Steps:**

- [ ] **Step 1: Update `MyDonEntryController.cs`**

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> MyDonEntry([FromBody] MydonEntryRequest request)
{
    Logger.LogInformation("Green MyDonEntry request: {Request}", request.Stringify());
    var common = await Mediator.Send(
        new AddMyDonEntryCommand(GameEra.Green, request.AccessCode, request.MydonName, 0),
        HttpContext.RequestAborted);

    return Ok(new MydonEntryResponse
    {
        Result = common.Result,
        ComSvrResult = common.ComSvrResult,
        Baid = common.Baid,
        AccessCode = common.AccessCode,
        MydonName = common.MydonName,
        IsPublish = true,
        ContentInfo = new byte[GreenProtocolBytes.ContentInfoBytes]
    });
}
```

- [ ] **Step 2: Build**

Run: `dotnet build`

Expected: PASS.

---

## Task 04.4: Implement Green InitialData And UserData Unlocks

**Acceptance Criteria:**
- [ ] InitialData returns first 10 songs in a 128-byte raw bitset.
- [ ] UserData returns first 20 songs in a 128-byte raw bitset.
- [ ] Both set `song_hash_ver`.
- [ ] UserData returns persisted option, tone, title, favorites, and recents.

**Steps:**

- [ ] **Step 1: Add handler tests**

Append to `GreenIdentityHandlerTests`:

```csharp
[Fact]
public async Task InitialData_Green_UnlocksFirstTenSongs()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var handler = new GetInitialDataQueryHandler(
        fixture.Catalog,
        NullLogger<GetInitialDataQueryHandler>.Instance,
        Microsoft.Extensions.Options.Options.Create(new TaikoLocalServer.Application.Settings.ServerSettings()));

    var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

    Assert.Equal((uint)1, response.Result);
    Assert.Equal(128, response.DefaultSongFlg.Length);
    Assert.True((response.DefaultSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
    Assert.True((response.DefaultSongFlg[110 >> 3] & (1 << (110 & 7))) != 0);
    Assert.False((response.DefaultSongFlg[111 >> 3] & (1 << (111 & 7))) != 0);
}

[Fact]
public async Task UserData_Green_UnlocksFirstTwentySongs()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 9, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(9));
    await fixture.Context.SaveChangesAsync();

    var handler = new UserDataQueryHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UserDataQueryHandler>.Instance,
        Microsoft.Extensions.Options.Options.Create(new TaikoLocalServer.Application.Settings.ServerSettings()));

    var response = await handler.Handle(new UserDataQuery(9, GameEra.Green), CancellationToken.None);

    Assert.Equal((uint)1, response.Result);
    Assert.Equal(128, response.ReleaseSongFlg.Length);
    Assert.True((response.ReleaseSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
    Assert.True((response.ReleaseSongFlg[120 >> 3] & (1 << (120 & 7))) != 0);
}
```

- [ ] **Step 2: Run focused tests and confirm failure**

Run: `dotnet test --filter "InitialData_Green|UserData_Green"`

Expected: FAIL because Green handlers are still stubs.

- [ ] **Step 3: Implement `GetInitialDataQuery.Green.cs`**

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleGreen(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var green = gameDataService.Green();
        var firstTen = green.MusicInfoFileOrder.Take(10).Select(song => song.SongNo);

        return ValueTask.FromResult(new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = GreenProtocolBytes.CreateFixedBitset(firstTen, GreenProtocolBytes.SongFlagBytes),
            AchievementSongBit = new byte[GreenProtocolBytes.SongFlagBytes],
            UraReleaseBit = new byte[GreenProtocolBytes.SongFlagBytes],
            SongHashVer = green.SongHashVersion,
            IsDanplay = true,
            IsClose = false,
            IsItemshop = true,
            IsGhostbattleplay = true,
            AryGreenTaikojukuDatas = green.TaikojukuFileOrder.Take(3)
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.UniqueId,
                    VerupNo = entry.VerupNo
                })
                .ToList()
        });
    }
}
```

Add missing Green fields to `CommonInitialDataCheckResponse`:

```csharp
public uint SongHashVer { get; set; }
public bool IsDanplay { get; set; }
public bool IsClose { get; set; }
public bool IsItemshop { get; set; }
public bool IsGhostbattleplay { get; set; }
public List<InformationData> AryGreenTelopDatas { get; set; } = [];
public List<InformationData> AryGreenEventFolderDatas { get; set; } = [];
public List<InformationData> AryGreenTaikojukuDatas { get; set; } = [];
public List<InformationData> AryGreenItemShopDatas { get; set; } = [];

public class InformationData
{
    public uint InfoId { get; set; }
    public uint VerupNo { get; set; }
}
```

- [ ] **Step 4: Implement `UserDataQuery.Green.cs`**

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<CommonUserDataResponse> HandleGreen(
        UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Green baid {request.Baid}.");
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var green = gameDataService.Green();

        var favorites = await context.GreenFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.GreenRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.SongNo)
            .Select(song => song.SongNo)
            .Take(10)
            .ToArrayAsync(cancellationToken);

        return new CommonUserDataResponse
        {
            Result = 1,
            ReleaseSongFlg = GreenProtocolBytes.CreateFixedBitset(
                green.MusicInfoFileOrder.Take(20).Select(song => song.SongNo),
                GreenProtocolBytes.SongFlagBytes),
            ToneFlg = GreenProtocolBytes.FixedOrZero(saveData.ToneFlg, GreenProtocolBytes.ToneFlagBytes),
            TitleFlg = GreenProtocolBytes.FixedOrZero(saveData.TitleFlg, GreenProtocolBytes.TitleFlagBytes),
            DefaultOptionSetting = saveData.DefaultOptionSetting,
            OptionFlg = saveData.OptionFlg,
            AryFavoriteSongNoes = favorites,
            AryRecentSongNoes = recent,
            SongFavoriteCnt = (uint)favorites.Length,
            SongRecentCnt = (uint)recent.Length,
            CategJpopCnt = saveData.CategJpopCnt,
            CategAnimeCnt = saveData.CategAnimeCnt,
            CategDoyoCnt = saveData.CategDoyoCnt,
            CategVarietyCnt = saveData.CategVarietyCnt,
            CategClassicCnt = saveData.CategClassicCnt,
            CategGameCnt = saveData.CategGameCnt,
            CategNamcoCnt = saveData.CategNamcoCnt,
            CategVocaloidCnt = saveData.CategVocaloidCnt,
            TotalCreditCnt = saveData.TotalCreditCnt,
            PrevAreaCode = saveData.PrevAreaCode,
            ConsecAreaCnt = saveData.ConsecAreaCnt,
            DefaultShinSetting = saveData.DefaultShinSetting,
            DispTaikojukuDan = saveData.DispTaikojukuDan,
            DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
            DifficultyPlayedStar = saveData.DifficultyPlayedStar,
            IsChallengeCompe = saveData.IsChallengeCompe,
            IsTojiru = saveData.IsTojiru,
            IsDevilGreen = saveData.IsDevil
        };
    }
}
```

Add `OptionFlg` to `CommonUserDataResponse.Green.cs`:

```csharp
public byte[] OptionFlg { get; set; } = [];
```

- [ ] **Step 5: Update Green mappers**

In `InitialDataMappers.Map`, map:

```csharp
SongHashVer = common.SongHashVer,
HashDefaultSongFlg = common.DefaultSongFlg,
HashMainichidojoAll = common.AchievementSongBit,
HashMainichidojoRare = common.UraReleaseBit,
IsDanplay = common.IsDanplay,
IsClose = common.IsClose,
IsItemshop = common.IsItemshop,
IsGhostbattleplay = common.IsGhostbattleplay
```

Map `AryTaikojukuDatas` from `common.AryGreenTaikojukuDatas`.

In `UserDataMappers.Map`, map:

```csharp
HashReleaseSongFlg = common.ReleaseSongFlg,
ToneFlg = common.ToneFlg,
TitleFlg = common.TitleFlg,
DefaultOptionSetting = common.DefaultOptionSetting,
OptionFlg = common.OptionFlg,
AryFavoriteSongNoes.AddRange(common.AryFavoriteSongNoes),
AryRecentSongNoes.AddRange(common.AryRecentSongNoes)
```

Use these generated collection property names from `Wire/Game.cs`: `AryFavoriteSongNoes` and `AryRecentSongNoes`.

- [ ] **Step 6: Update controllers**

`InitialDataCheckController`:

```csharp
var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Green), HttpContext.RequestAborted);
return Ok(InitialDataMappers.Map(common));
```

`UserDataController`:

```csharp
var common = await Mediator.Send(new UserDataQuery(request.Baid, GameEra.Green), HttpContext.RequestAborted);
return Ok(UserDataMappers.Map(common));
```

- [ ] **Step 7: Run tests and build**

Run:

```bash
dotnet test --filter "GreenIdentityHandlerTests|GreenSaveDataTests"
dotnet build
```

Expected: both PASS.

- [ ] **Step 8: Commit**

```bash
git add Application/Handlers Application/Dtos Application/Common Adapters.GameProtocol.Green Tests/Green/GreenIdentityHandlerTests.cs
git commit -m "feat(green): implement identity and song unlock responses"
```
