# Green Must-Fix Field Constraints Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add focused Green protocol guards for the audit report's "Must Fix Now" crash/corruption risks without implementing broader protocol features.

**Architecture:** Keep validation at the boundary that first knows enough context: mappers omit unsafe wire values, handlers validate catalog-backed state before persistence, and controllers reject mismatched request envelopes before sending commands. Use conservative behavior where catalog evidence is missing: reject or ignore unsafe unknown values rather than serializing or persisting defaults.

**Tech Stack:** C#/.NET, xUnit, EF Core SQLite test fixture, MediatR handlers, protobuf-net generated Green wire types.

---

## Source Map

- `docs/green-protocol-field-audit.md`: source audit and rationale.
- `Application/Handlers/GetTaikojukuQuery.Green.cs`: Taikojuku slot, song, and pack construction.
- `Adapters.GameProtocol.Green/Mappers/TaikojukuMappers.cs`: final Taikojuku wire serialization.
- `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`: outer playresult request envelope and decoded inner payload.
- `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`: Green generated request to common DTO mapping.
- `Application/Dtos/CommonPlayResultData.Green.cs`: Green-only optional-presence DTO additions.
- `Application/Handlers/UpdatePlayResultCommand.cs`: handler constructor dependencies.
- `Application/Handlers/UpdatePlayResultCommand.Green.cs`: Green playresult persistence guards.
- `Application/Handlers/GetInitialDataQuery.Green.cs`: itemshop advertisement flag.
- `Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs`: empty shop response serialization.
- `Application/Handlers/ItemPurchaseCommand.cs`: item purchase handler dependencies.
- `Application/Handlers/ItemPurchaseCommand.Green.cs`: shop item validation and medal arithmetic.
- `Application/Handlers/RewardExecutionCommand.Green.cs`: reward bitset validation.
- `Tests/Green/GreenTaikojukuTests.cs`: Taikojuku guard tests.
- `Tests/Green/GreenPlayResultHandlerTests.cs`: playresult guard tests.
- `Tests/Green/GreenGhostRewardTests.cs`: reward and item purchase tests.
- `Tests/Green/GreenIdentityHandlerTests.cs` or `Tests/Green/GreenSaveDataTests.cs`: initial-data/userdata-facing assertions if needed.
- `Tests/Green/GreenHandlerFixture.cs`: test catalog setup.

## Task 1: Taikojuku Pack Bounds

**Files:**
- Modify: `Tests/Green/GreenTaikojukuTests.cs`
- Modify: `Application/Handlers/GetTaikojukuQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/TaikojukuMappers.cs`

- [ ] **Step 1: Add failing Taikojuku tests**

Add these tests to `Tests/Green/GreenTaikojukuTests.cs`:

```csharp
[Fact]
public async Task GetTaikojuku_AllInvalidRequestSlotsFallbackIsCappedToEleven()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var handler = new GetTaikojukuQueryHandler(
        fixture.Catalog,
        NullLogger<GetTaikojukuQueryHandler>.Instance);

    var response = await handler.Handle(
        new GetTaikojukuQuery(Enumerable.Range(101, 25).Select(value => (uint)value).ToArray()),
        CancellationToken.None);

    Assert.Equal((uint)1, response.Result);
    Assert.Equal(11, response.Packs.Count);
    Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
}

[Fact]
public void TaikojukuMapper_DropsInvalidPackSlotsAndCapsSongsAtTen()
{
    var response = TaikojukuMappers.Map(new CommonTaikojukuResponse
    {
        Result = 1,
        Packs =
        [
            new CommonTaikojukuResponse.Pack
            {
                GetDan = 0,
                Songs = [new CommonTaikojukuResponse.Song { SongNo = 101, Level = 0 }]
            },
            new CommonTaikojukuResponse.Pack
            {
                GetDan = 1,
                Songs =
                [
                    new CommonTaikojukuResponse.Song { SongNo = 0, Level = 0 },
                    new CommonTaikojukuResponse.Song { SongNo = 1024, Level = 0 },
                    new CommonTaikojukuResponse.Song { SongNo = 101, Level = 5 },
                    new CommonTaikojukuResponse.Song { SongNo = 101, Level = 0 },
                    new CommonTaikojukuResponse.Song { SongNo = 102, Level = 1 },
                    new CommonTaikojukuResponse.Song { SongNo = 103, Level = 2 },
                    new CommonTaikojukuResponse.Song { SongNo = 104, Level = 3 },
                    new CommonTaikojukuResponse.Song { SongNo = 105, Level = 4 },
                    new CommonTaikojukuResponse.Song { SongNo = 106, Level = 0 },
                    new CommonTaikojukuResponse.Song { SongNo = 107, Level = 1 },
                    new CommonTaikojukuResponse.Song { SongNo = 108, Level = 2 },
                    new CommonTaikojukuResponse.Song { SongNo = 109, Level = 3 },
                    new CommonTaikojukuResponse.Song { SongNo = 110, Level = 4 },
                    new CommonTaikojukuResponse.Song { SongNo = 111, Level = 0 }
                ]
            }
        ]
    });

    var pack = Assert.Single(response.AryJukupackDatas);
    Assert.Equal((uint)1, pack.GetDan);
    Assert.Equal(10, pack.AryJukusongDatas.Count);
    Assert.DoesNotContain(pack.AryJukusongDatas, song => song.SongNo is 0 or 1024);
    Assert.All(pack.AryJukusongDatas, song => Assert.InRange(song.Level, 0u, 4u));
}
```

- [ ] **Step 2: Run the Taikojuku tests and confirm they fail**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenTaikojukuTests" --no-restore
```

Expected: the new tests fail because fallback can return 25 packs and the mapper currently serializes invalid packs/songs.

- [ ] **Step 3: Add Taikojuku constraints in the handler**

In `Application/Handlers/GetTaikojukuQuery.Green.cs`, add constants near the top of the partial class:

```csharp
private const int MaxDanSlots = 25;
private const int MaxRequestedSlotsPerRequest = 11;
private const int MaxSongsPerPack = 10;
private const uint MaxGreenCourseLevel = 4;
```

Change the response construction to pass catalog context and omit empty packs:

```csharp
return ValueTask.FromResult(new CommonTaikojukuResponse
{
    Result = 1,
    Packs = packs
        .Select(pack => ToCommonPack(pack, green.GreenMusicInfos))
        .Where(pack => pack.Songs.Count > 0)
        .ToList()
});
```

Replace hard-coded `25` values:

```csharp
private static bool IsValidDanSlot(uint getDan)
    => getDan is >= 1 and <= MaxDanSlots;
```

```csharp
return Enumerable.Range(1, Math.Min(requestedDans.Count, MaxRequestedSlotsPerRequest))
    .Select(slot => (uint)slot)
    .ToArray();
```

Replace `ToCommonPack` with:

```csharp
private static CommonTaikojukuResponse.Pack ToCommonPack(
    GreenTaikojukuEntry entry,
    IReadOnlyDictionary<uint, GreenMusicInfoEntry> validSongs)
{
    return new CommonTaikojukuResponse.Pack
    {
        GetDan = entry.ChallengeLevel,
        VerupNo = entry.VerupNo,
        Songs = entry.Songs
            .Where(song => validSongs.ContainsKey(song.SongNo))
            .Where(song => song.Level <= MaxGreenCourseLevel)
            .Take(MaxSongsPerPack)
            .Select(song => new CommonTaikojukuResponse.Song
            {
                SongNo = song.SongNo,
                Level = song.Level
            })
            .ToList()
    };
}
```

- [ ] **Step 4: Add final wire guards in the Taikojuku mapper**

In `Adapters.GameProtocol.Green/Mappers/TaikojukuMappers.cs`, add constants and helpers inside `TaikojukuMappers`:

```csharp
private const int MaxSongsPerPack = 10;
private const uint MaxGreenSongNo = GreenProtocolBytes.SongFlagBytes * 8 - 1;
private const uint MaxGreenCourseLevel = 4;

private static bool IsValidDanSlot(uint value)
    => value is >= 1 and <= 25;

private static bool IsValidJukusong(CommonTaikojukuResponse.Song song)
    => song.SongNo is > 0
        && song.SongNo <= MaxGreenSongNo
        && song.Level <= MaxGreenCourseLevel;
```

Add this using at the top:

```csharp
using TaikoLocalServer.Application.Common;
```

Then change the pack loop:

```csharp
foreach (var pack in common.Packs)
{
    if (!IsValidDanSlot(pack.GetDan))
    {
        continue;
    }

    var songs = pack.Songs
        .Where(IsValidJukusong)
        .Take(MaxSongsPerPack)
        .ToArray();

    if (songs.Length == 0)
    {
        continue;
    }

    var wirePack = new TaikojukuResponse.JukupackData
    {
        GetDan = pack.GetDan,
        VerupNo = pack.VerupNo
    };

    foreach (var song in songs)
    {
        wirePack.AryJukusongDatas.Add(new TaikojukuResponse.JukupackData.JukusongData
        {
            SongNo = song.SongNo,
            Level = song.Level
        });
    }

    response.AryJukupackDatas.Add(wirePack);
}
```

- [ ] **Step 5: Run the Taikojuku tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenTaikojukuTests" --no-restore
```

Expected: all `GreenTaikojukuTests` pass.

- [ ] **Step 6: Commit Taikojuku guards**

Run:

```powershell
git add -- Tests/Green/GreenTaikojukuTests.cs Application/Handlers/GetTaikojukuQuery.Green.cs Adapters.GameProtocol.Green/Mappers/TaikojukuMappers.cs
git commit -m "Guard Green Taikojuku pack constraints"
```

## Task 2: PlayResult Presence and Catalog Validation

**Files:**
- Modify: `Tests/Green/GreenPlayResultHandlerTests.cs`
- Modify: `Tests/Green/GreenHandlerFixture.cs`
- Modify: `Application/Dtos/CommonPlayResultData.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`

- [ ] **Step 1: Add failing playresult validation tests**

In `Tests/Green/GreenPlayResultHandlerTests.cs`, update existing `new UpdatePlayResultCommandHandler(...)` calls to include `fixture.Catalog` after Task 2 Step 4 changes. Then add:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_RejectsOutOfCatalogSongNo()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 1024,
                    Level = 0,
                    PlayResult = 1,
                    PlayScore = 123
                }
            ]
        }),
        CancellationToken.None);

    Assert.Equal((uint)0, result);
    Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
    Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
    Assert.Empty(await fixture.Context.GreenFavoriteSongs.ToListAsync());
    Assert.Empty(await fixture.Context.GreenRecentSongs.ToListAsync());
}

[Fact]
public async Task UpdatePlayResult_Green_RejectsStageLevelOutsideZeroThroughFour()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 5,
                    PlayResult = 1,
                    PlayScore = 123
                }
            ]
        }),
        CancellationToken.None);

    Assert.Equal((uint)0, result);
    Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
}

[Fact]
public async Task UpdatePlayResult_Green_MissingCurrentCostumeDoesNotClearSavedCostume()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.Costume1 = 7;
    save.Costume2 = 8;
    save.Costume3 = 9;
    save.Costume4 = 10;
    save.Costume5 = 11;
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            HasAryCurrentCostume = false,
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 0,
                    PlayResult = 1,
                    PlayScore = 123
                }
            ]
        }),
        CancellationToken.None);

    var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.Equal((uint)1, result);
    Assert.Equal((uint)7, reloaded!.Costume1);
    Assert.Equal((uint)8, reloaded.Costume2);
    Assert.Equal((uint)9, reloaded.Costume3);
    Assert.Equal((uint)10, reloaded.Costume4);
    Assert.Equal((uint)11, reloaded.Costume5);
}

[Fact]
public async Task UpdatePlayResult_Green_OmittedDifficultyPlayedFieldsPreserveExistingValues()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.DifficultyPlayedCourse = 3;
    save.DifficultyPlayedStar = 4;
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            HasDifficultyPlayedCourse = false,
            HasDifficultyPlayedStar = false,
            AryStageInfoes =
            [
                new CommonPlayResultData.StageData
                {
                    SongNo = 101,
                    Level = 0,
                    PlayResult = 1,
                    PlayScore = 123
                }
            ]
        }),
        CancellationToken.None);

    var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.Equal((uint)1, result);
    Assert.Equal((uint)3, reloaded!.DifficultyPlayedCourse);
    Assert.Equal((uint)4, reloaded.DifficultyPlayedStar);
}

[Fact]
public async Task UpdatePlayResult_Green_DoesNotOverflowMedalTotals()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.TotalGetDonmedal = uint.MaxValue;
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            GetDonmedal = 1
        }),
        CancellationToken.None);

    var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.Equal((uint)0, result);
    Assert.Equal(uint.MaxValue, reloaded!.TotalGetDonmedal);
}
```

- [ ] **Step 2: Run the playresult tests and confirm they fail**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests" --no-restore
```

Expected: new tests fail because invalid songs/levels are persisted, missing optional fields overwrite save data, and medal totals can wrap.

- [ ] **Step 3: Add Green optional-presence flags to the DTO**

In `Application/Dtos/CommonPlayResultData.Green.cs`, add these properties to `CommonPlayResultData`:

```csharp
public bool HasAryCurrentCostume { get; set; } = true;
public bool HasDifficultyPlayedCourse { get; set; } = true;
public bool HasDifficultyPlayedStar { get; set; } = true;
```

- [ ] **Step 4: Preserve optional presence in `PlayResultMappers`**

In `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, set the new flags in `Map(PlayResultDataRequest request)`:

```csharp
HasAryCurrentCostume = request.AryCurrentCostume is not null,
HasDifficultyPlayedCourse = request.ShouldSerializeDifficultyPlayedCourse(),
HasDifficultyPlayedStar = request.ShouldSerializeDifficultyPlayedStar(),
```

Keep `AryCurrentCostume = MapCostume(request.AryCurrentCostume)` so existing non-null consumers still compile.

- [ ] **Step 5: Add catalog dependency to the playresult handler**

In `Application/Handlers/UpdatePlayResultCommand.cs`, change the primary constructor to:

```csharp
public partial class UpdatePlayResultCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<UpdatePlayResultCommandHandler> logger)
    : IRequestHandler<UpdatePlayResultCommand, uint>
```

Update every direct test construction of `UpdatePlayResultCommandHandler` to pass `fixture.Catalog` between `fixture.Context` and the logger.

- [ ] **Step 6: Add Green playresult validation helpers**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, add constants and validation helpers inside `UpdatePlayResultCommandHandler`:

```csharp
private const uint MaxGreenCourseLevel = 4;
private const uint MaxGreenPlayResult = 3;
private const uint MaxGreenDanSlot = 25;

private static bool IsValidGreenStage(CommonPlayResultData.StageData stage, IGreenCatalog green)
{
    return stage.SongNo < GreenProtocolBytes.SongFlagBytes * 8
        && green.GreenMusicInfos.ContainsKey(stage.SongNo)
        && stage.Level <= MaxGreenCourseLevel
        && stage.PlayResult <= MaxGreenPlayResult
        && stage.PlayDan is null or (>= 1 and <= MaxGreenDanSlot);
}

private static bool CanAdd(uint current, uint delta)
    => delta <= uint.MaxValue - current;

private static bool HasBit(byte[] source, uint id, int byteCount)
{
    if (id >= byteCount * 8)
    {
        return false;
    }

    var fixedBytes = GreenProtocolBytes.FixedOrZero(source, byteCount);
    return (fixedBytes[id >> 3] & (1 << ((int)id & 7))) != 0;
}

private static bool AllAlreadyUnlocked(byte[] source, IEnumerable<uint> ids, int byteCount)
    => ids.All(id => HasBit(source, id, byteCount));

private static bool IsValidCurrentCostume(UserSaveDataGreen saveData, CommonPlayResultData.CostumeData costume)
{
    return HasBit(saveData.CostumeFlg1, costume.Costume1, GreenProtocolBytes.CostumeFlagBytes)
        && HasBit(saveData.CostumeFlg2, costume.Costume2, GreenProtocolBytes.CostumeFlagBytes)
        && HasBit(saveData.CostumeFlg3, costume.Costume3, GreenProtocolBytes.CostumeFlagBytes)
        && HasBit(saveData.CostumeFlg4, costume.Costume4, GreenProtocolBytes.CostumeFlagBytes)
        && HasBit(saveData.CostumeFlg5, costume.Costume5, GreenProtocolBytes.CostumeFlagBytes);
}

private static bool HasOnlyKnownUnlockRewards(UserSaveDataGreen saveData, CommonPlayResultData playResultData)
{
    return AllAlreadyUnlocked(saveData.ToneFlg, playResultData.GetToneNoes, GreenProtocolBytes.ToneFlagBytes)
        && AllAlreadyUnlocked(saveData.CostumeFlg1, playResultData.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes)
        && AllAlreadyUnlocked(saveData.CostumeFlg2, playResultData.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes)
        && AllAlreadyUnlocked(saveData.CostumeFlg3, playResultData.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes)
        && AllAlreadyUnlocked(saveData.CostumeFlg4, playResultData.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes)
        && AllAlreadyUnlocked(saveData.CostumeFlg5, playResultData.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes)
        && AllAlreadyUnlocked(saveData.TitleFlg, playResultData.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes);
}
```

Add this using if the file does not already resolve `IGreenCatalog`:

```csharp
using TaikoLocalServer.Application.Abstractions;
```

- [ ] **Step 7: Apply validation before persistence**

At the start of `HandleGreen`, after `saveData` is loaded, add:

```csharp
var green = gameDataService.Green();
if (!CanAdd(saveData.TotalGetDonmedal, playResultData.GetDonmedal)
    || !CanAdd(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal)
    || playResultData.AryStageInfoes.Any(stage => !IsValidGreenStage(stage, green))
    || !HasOnlyKnownUnlockRewards(saveData, playResultData)
    || (playResultData.HasAryCurrentCostume && !IsValidCurrentCostume(saveData, playResultData.AryCurrentCostume)))
{
    logger.LogWarning("Rejecting invalid Green playresult payload for baid {Baid}", request.Baid);
    return 0;
}
```

Then replace unconditional writes:

```csharp
saveData.TotalGetDonmedal += playResultData.GetDonmedal;
saveData.TotalGetKatsumedal += playResultData.GetKatsumedal;
```

Keep those only after the validation above.

Replace unconditional difficulty and costume writes:

```csharp
if (playResultData.HasDifficultyPlayedCourse)
{
    saveData.DifficultyPlayedCourse = playResultData.DifficultyPlayedCourse;
}

if (playResultData.HasDifficultyPlayedStar)
{
    saveData.DifficultyPlayedStar = playResultData.DifficultyPlayedStar;
}
```

```csharp
if (playResultData.HasAryCurrentCostume)
{
    ApplyCostume(saveData, playResultData.AryCurrentCostume);
}
```

- [ ] **Step 8: Reject outer/inner BAID mismatch in the controller**

In `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, after `commonRequest = PlayResultMappers.Map(decoded.Request);`, add:

```csharp
if (commonRequest.Baid != 0 && commonRequest.Baid != request.BaidConf)
{
    Logger.LogWarning(
        "Rejecting Green PlayResult baid mismatch: outer={OuterBaid}, inner={InnerBaid}",
        request.BaidConf,
        commonRequest.Baid);
    return Ok(new PlayResultResponse { Result = 0 });
}
```

- [ ] **Step 9: Run the playresult tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests" --no-restore
```

Expected: all `GreenPlayResultHandlerTests` pass.

- [ ] **Step 10: Commit playresult guards**

Run:

```powershell
git add -- Tests/Green/GreenPlayResultHandlerTests.cs Tests/Green/GreenHandlerFixture.cs Application/Dtos/CommonPlayResultData.Green.cs Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs Adapters.GameProtocol.Green/Controllers/PlayResultController.cs Application/Handlers/UpdatePlayResultCommand.cs Application/Handlers/UpdatePlayResultCommand.Green.cs
git commit -m "Guard Green playresult persistence constraints"
```

## Task 3: Item Shop Advertisement and Purchase Guards

**Files:**
- Modify: `Tests/Green/GreenGhostRewardTests.cs`
- Modify: `Tests/Green/GreenTaikojukuTests.cs` or add `Tests/Green/GreenInitialDataTests.cs`
- Modify: `Tests/Green/GreenHandlerFixture.cs`
- Modify: `Application/Handlers/GetInitialDataQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs`
- Modify: `Application/Handlers/ItemPurchaseCommand.cs`
- Modify: `Application/Handlers/ItemPurchaseCommand.Green.cs`

- [ ] **Step 1: Make test catalog configurable**

In `Tests/Green/GreenHandlerFixture.cs`, change `CreateAsync()` to accept an optional catalog:

```csharp
public static async Task<GreenHandlerFixture> CreateAsync(IGreenCatalog? greenCatalog = null)
{
    var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<TaikoDbContext>()
        .UseSqlite(connection)
        .Options;
    var context = new TaikoDbContext(options);
    await context.Database.EnsureCreatedAsync();

    var catalog = new FileGameDataCatalog([greenCatalog ?? new TestGreenCatalog()]);
    return new GreenHandlerFixture(connection, context, catalog);
}
```

Give `TestGreenCatalog` a constructor and configurable `ItemShop`:

```csharp
public TestGreenCatalog(IReadOnlyDictionary<uint, GreenItemShopEntry>? itemShop = null)
{
    ItemShop = itemShop ?? new Dictionary<uint, GreenItemShopEntry>();
}
```

Replace the current `ItemShop` property with:

```csharp
public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop { get; }
```

- [ ] **Step 2: Add failing itemshop tests**

Add these tests to `Tests/Green/GreenGhostRewardTests.cs`:

```csharp
[Fact]
public async Task ItemPurchase_RejectsUnknownGreenShopItem()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.TotalGetDonmedal = 100;
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var handler = new ItemPurchaseCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<ItemPurchaseCommandHandler>.Instance);

    var response = await handler.Handle(new ItemPurchaseCommand(1, 10, 1, 2, 40), CancellationToken.None);

    Assert.Equal((uint)0, response.Result);
    Assert.Equal((uint)0, response.TotalUseDonmedal);
}

[Fact]
public async Task ItemPurchase_RejectsOverflowingGreenMedalBalance()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync(new GreenHandlerFixture.TestGreenCatalog(
        new Dictionary<uint, GreenItemShopEntry>
        {
            [10] = new() { ItemType = 1, ItemId = 2, Price = 40 }
        }));
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.TotalGetDonmedal = uint.MaxValue;
    save.TotalUseDonmedal = uint.MaxValue - 10;
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var handler = new ItemPurchaseCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<ItemPurchaseCommandHandler>.Instance);

    var response = await handler.Handle(new ItemPurchaseCommand(1, 10, 1, 2, 40), CancellationToken.None);

    Assert.Equal((uint)0, response.Result);
    Assert.Equal(uint.MaxValue - 10, response.TotalUseDonmedal);
}
```

If `TestGreenCatalog` is private, make it `internal sealed` so the tests above can instantiate it:

```csharp
internal sealed class TestGreenCatalog : IGreenCatalog
```

Add this initial-data test to `Tests/Green/GreenTaikojukuTests.cs` or a new `Tests/Green/GreenInitialDataTests.cs`:

```csharp
[Fact]
public async Task InitialData_DoesNotAdvertiseEmptyGreenItemShop()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var handler = new GetInitialDataQueryHandler(
        fixture.Catalog,
        NullLogger<GetInitialDataQueryHandler>.Instance,
        Options.Create(new ServerSettings()));

    var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

    Assert.False(response.IsItemshop);
}
```

- [ ] **Step 3: Run the itemshop tests and confirm they fail**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenGhostRewardTests|FullyQualifiedName~GreenTaikojukuTests" --no-restore
```

Expected: item purchase currently succeeds without catalog validation, and initial data advertises an empty itemshop.

- [ ] **Step 4: Disable empty itemshop advertisement**

In `Application/Handlers/GetInitialDataQuery.Green.cs`, replace:

```csharp
IsItemshop = true,
```

with:

```csharp
IsItemshop = green.ItemShop.Count > 0,
```

In `Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs`, remove explicit empty optional strings:

```csharp
return Ok(new GetitemshopinfoResponse
{
    Result = 1
});
```

- [ ] **Step 5: Add catalog dependency to item purchase handler**

In `Application/Handlers/ItemPurchaseCommand.cs`, change the primary constructor to:

```csharp
public partial class ItemPurchaseCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<ItemPurchaseCommandHandler> logger)
    : IRequestHandler<ItemPurchaseCommand, CommonItemPurchaseResponse>
```

Update direct test constructions of `ItemPurchaseCommandHandler` to pass `fixture.Catalog`.

- [ ] **Step 6: Validate shop item and medal arithmetic**

Replace the body of `Handle` in `Application/Handlers/ItemPurchaseCommand.Green.cs` with:

```csharp
logger.LogDebug("Applying Green item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
var green = gameDataService.Green();
var available = saveData.TotalGetDonmedal >= saveData.TotalUseDonmedal
    ? saveData.TotalGetDonmedal - saveData.TotalUseDonmedal
    : 0;

if (!green.ItemShop.TryGetValue(request.ItemNo, out var item)
    || request.ItemType != item.ItemType
    || request.ItemId != item.ItemId
    || request.ItemPrice != item.Price
    || item.Price == 0
    || item.Price > available)
{
    return new CommonItemPurchaseResponse
    {
        Result = 0,
        TotalGetDonmedal = saveData.TotalGetDonmedal,
        TotalUseDonmedal = saveData.TotalUseDonmedal
    };
}

saveData.TotalUseDonmedal += item.Price;

await context.SaveChangesAsync(cancellationToken);
return new CommonItemPurchaseResponse
{
    Result = 1,
    TotalGetDonmedal = saveData.TotalGetDonmedal,
    TotalUseDonmedal = saveData.TotalUseDonmedal
};
```

- [ ] **Step 7: Run itemshop tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenGhostRewardTests|FullyQualifiedName~GreenTaikojukuTests" --no-restore
```

Expected: itemshop-related tests pass.

- [ ] **Step 8: Commit itemshop guards**

Run:

```powershell
git add -- Tests/Green/GreenGhostRewardTests.cs Tests/Green/GreenTaikojukuTests.cs Tests/Green/GreenHandlerFixture.cs Application/Handlers/GetInitialDataQuery.Green.cs Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs Application/Handlers/ItemPurchaseCommand.cs Application/Handlers/ItemPurchaseCommand.Green.cs
git commit -m "Guard Green itemshop defaults and purchases"
```

## Task 4: Reward Execution Conservative Guard

**Files:**
- Modify: `Tests/Green/GreenGhostRewardTests.cs`
- Modify: `Application/Handlers/RewardExecutionCommand.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs` if Task 2 did not already add the shared helper pattern.

- [ ] **Step 1: Replace reward unlock test with conservative rejection test**

In `Tests/Green/GreenGhostRewardTests.cs`, replace `RewardExecution_SetsGreenUnlockBits` with:

```csharp
[Fact]
public async Task RewardExecution_RejectsUnknownInRangeRewardIds()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new RewardExecutionCommandHandler(
        fixture.Context,
        NullLogger<RewardExecutionCommandHandler>.Instance);

    var response = await handler.Handle(new RewardExecutionCommand(
        1,
        [],
        [2],
        [3],
        [],
        [],
        [],
        [],
        [4]), CancellationToken.None);

    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.Equal((uint)0, response.Result);
    Assert.False((save!.ToneFlg[2 >> 3] & (1 << (2 & 7))) != 0);
    Assert.False((save.CostumeFlg1[3 >> 3] & (1 << (3 & 7))) != 0);
    Assert.False((save.TitleFlg[4 >> 3] & (1 << (4 & 7))) != 0);
}
```

- [ ] **Step 2: Run the reward test and confirm it fails**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenGhostRewardTests.RewardExecution_RejectsUnknownInRangeRewardIds" --no-restore
```

Expected: the test fails because current reward execution sets in-range bits directly.

- [ ] **Step 3: Add conservative validation in reward execution**

In `Application/Handlers/RewardExecutionCommand.Green.cs`, add helpers:

```csharp
private static bool HasBit(byte[] source, uint id, int byteCount)
{
    if (id >= byteCount * 8)
    {
        return false;
    }

    var fixedBytes = GreenProtocolBytes.FixedOrZero(source, byteCount);
    return (fixedBytes[id >> 3] & (1 << ((int)id & 7))) != 0;
}

private static bool AllAlreadyUnlocked(byte[] source, IEnumerable<uint> ids, int byteCount)
    => ids.All(id => HasBit(source, id, byteCount));
```

After loading `saveData`, add:

```csharp
if (!AllAlreadyUnlocked(saveData.ToneFlg, request.GetToneNoes, GreenProtocolBytes.ToneFlagBytes)
    || !AllAlreadyUnlocked(saveData.CostumeFlg1, request.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes)
    || !AllAlreadyUnlocked(saveData.CostumeFlg2, request.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes)
    || !AllAlreadyUnlocked(saveData.CostumeFlg3, request.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes)
    || !AllAlreadyUnlocked(saveData.CostumeFlg4, request.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes)
    || !AllAlreadyUnlocked(saveData.CostumeFlg5, request.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes)
    || !AllAlreadyUnlocked(saveData.TitleFlg, request.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes))
{
    logger.LogWarning("Rejecting unknown Green reward ids for baid {Baid}", request.Baid);
    return new CommonRewardExecutionResponse { Result = 0 };
}
```

This conservative guard prevents new unlocks until the client-evidence plan defines valid reward catalogs. The follow-up evidence plan should replace this with catalog-aware allowlists when exact Green item domains are proven.

- [ ] **Step 4: Run reward tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenGhostRewardTests" --no-restore
```

Expected: reward tests pass with the new conservative behavior.

- [ ] **Step 5: Commit reward guard**

Run:

```powershell
git add -- Tests/Green/GreenGhostRewardTests.cs Application/Handlers/RewardExecutionCommand.Green.cs
git commit -m "Conservatively guard Green reward unlock ids"
```

## Task 5: Full Verification

**Files:**
- No new files.

- [ ] **Step 1: Run focused Green test suite**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green" --no-restore
```

Expected: all Green tests pass.

- [ ] **Step 2: Run full test project**

Run:

```powershell
dotnet test Tests/Tests.csproj --no-restore
```

Expected: all tests pass.

- [ ] **Step 3: Build the host**

Run:

```powershell
dotnet build Host/Host.csproj --no-restore
```

Expected: build succeeds with exit code `0`.

- [ ] **Step 4: Check working tree**

Run:

```powershell
git status --short
```

Expected: either clean or only intentional uncommitted documentation/IDA metadata from the audit session.

- [ ] **Step 5: Commit verification note if needed**

If final verification required test-only or documentation adjustments, commit them:

```powershell
git add -- Tests Application Adapters.GameProtocol.Green docs
git commit -m "Document Green must-fix constraint verification"
```

Skip this commit if `git status --short` is clean after Task 4.
