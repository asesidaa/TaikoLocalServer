# Phase 09: Tokkun Mapper and Safe Playresult Acceptance - Pattern Map

**Mapped:** 2026-06-05
**Files analyzed:** 5 target files
**Analogs found:** 5 / 5

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` | model / DTO | transform, request-response DTO | `Application/Dtos/CommonPlayResultData.BlueBattle.cs` | exact |
| `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` | mapper / utility | transform | `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` plus battle sections | exact |
| `Application/Handlers/UpdatePlayResultCommand.Blue.cs` | handler / service | request-response, branch dispatch, no-write acceptance | `Application/Handlers/UpdatePlayResultCommand.Blue.cs` and `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs` | exact |
| `Tests/Blue/BluePlayResultMapperTests.cs` | test | transform verification | `Tests/Blue/BluePlayResultMapperTests.cs` and `Tests/Blue/BlueBattlePlayResultMapperTests.cs` | exact |
| `Tests/Blue/BluePlayResultHandlerTests.cs` | test | request-response with CRUD no-write assertions | `Tests/Blue/BluePlayResultHandlerTests.cs` and `Tests/Blue/BlueBattlePlayResultHandlerTests.cs` | exact |

**Optional shape note:** If the planner wants a separate helper file, create `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs` by copying the partial-class shell from `UpdatePlayResultCommand.BlueBattle.cs`. The branch still must be inserted in `UpdatePlayResultCommand.Blue.cs` before the battle and normal paths.

**Read-only references, not targets:** `Adapters.GameProtocol.Blue/Wire/Game.cs`, `proto/blue/taiko.proto`, and `Domain/Enums/PlayMode.cs`.

## Pattern Assignments

### `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` (model / DTO, transform)

**Analog:** `Application/Dtos/CommonPlayResultData.BlueBattle.cs`

**Partial DTO file shell** (lines 1-8):

```csharp
// ReSharper disable InconsistentNaming
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public bool IsBattlePlayResult { get; set; }

    public BattleReleaseDataDto? BattleReleaseData { get; set; }
```

**Nested branch-specific DTO pattern** (lines 55-77):

```csharp
public class BattleReleaseDataDto
{
    public List<uint> ReleaseInfoIds { get; set; } = [];

    public List<uint> ReleaseBattleStageIds { get; set; } = [];

    public List<uint> ReleaseNpcIds { get; set; } = [];

    public List<uint> ReleaseNpcCostumeIds { get; set; } = [];

    public List<uint> ReleaseNpcSpecialIds { get; set; } = [];

    public List<BattleTokenData> BattleTokenData { get; set; } = [];

    public uint AssignNextStageId { get; set; }
}
```

**Copy/adapt for Tokkun:**

```csharp
// ReSharper disable InconsistentNaming
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public bool IsTokkunPlayResult { get; set; }
    public uint? TokkunTutorialFlg { get; set; }
    public TokkunStageDataDto? TokkunStageData { get; set; }

    public class TokkunStageDataDto
    {
        public string BanacoinDatetime { get; set; } = string.Empty;
        public uint TokkunSongCnt { get; set; }
        public List<uint> TookunSongnoes { get; set; } = [];
        public uint TokkunSpeedchangeCnt { get; set; }
        public uint TokkunAutoplayCnt { get; set; }
        public uint TokkunJumpCnt { get; set; }
    }
}
```

**Important constraints:**

- Keep all fields as raw protocol facts.
- Preserve the generated wire spelling `TookunSongnoes`.
- Do not add EF entities, DbSets, migrations, AdminApi DTOs, WebUI models, or userdata readback fields in Phase 9.

---

### `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` (mapper / utility, transform)

**Analog:** `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`

**Imports and mapper shell** (lines 1-8):

```csharp
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static CommonPlayResultData Map(PlayResultRequest request)
```

**Top-level mapping pattern** (lines 10-19):

```csharp
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
```

**Optional presence pattern** (lines 33-58):

```csharp
ItemshopTutorialFlg = request.ShouldSerializeItemshopTutorialFlg() ? request.ItemshopTutorialFlg : null,
IsDevil = request.ShouldSerializeIsDevil() ? request.IsDevil : null,
IsExplain = request.ShouldSerializeIsExplain() ? request.IsExplain : null,
AryPlayCostume = MapCostume(request.AryPlayCostume),
AryCurrentCostume = MapCostume(request.AryCurrentCostume),
HasAryCurrentCostume = request.AryCurrentCostume is not null,
GenderType = request.GenderType,
PlayerAge = request.PlayerAge,
PlayMode = request.PlayMode,
IsBattlePlayResult = request.AryReleaseBattledata is not null
    || request.AryStageInfoes.Any(stage => stage.AryBattlestagedata is not null),
BattleReleaseData = MapBattleReleaseData(request.AryReleaseBattledata),
AreaCode = request.AreaCode,
Reserved = request.Reserved ?? [],
LowerlimitAge = request.ShouldSerializeLowerlimitAge() ? request.LowerlimitAge : null,
UpperlimitAge = request.ShouldSerializeUpperlimitAge() ? request.UpperlimitAge : null,
AgeScore = request.ShouldSerializeAgeScore() ? request.AgeScore : null,
EstimationCount = request.ShouldSerializeEstimationCount() ? request.EstimationCount : null,
DanResult = request.ShouldSerializeDanResult() ? request.DanResult : 0,
Accesstoken = request.Accesstoken,
ContentInfo = request.ContentInfo ?? [],
DifficultyPlayedCourse = request.DifficultyPlayedCourse,
DifficultyPlayedStar = request.DifficultyPlayedStar,
HasDifficultyPlayedCourse = request.ShouldSerializeDifficultyPlayedCourse(),
HasDifficultyPlayedStar = request.ShouldSerializeDifficultyPlayedStar(),
WaiwaiTutorialFlg = request.ShouldSerializeWaiwaiTutorialFlg() ? request.WaiwaiTutorialFlg : null
```

**Nested helper pattern** (lines 101-115):

```csharp
private static CommonPlayResultData.BattleStageData? MapBattleStageData(
    PlayResultRequest.StageData.BattleStageData? data)
    => data is null
        ? null
        : new CommonPlayResultData.BattleStageData
        {
            SupportLv = data.SupportLv,
            BattleStageId = data.BattleStageId,
            NpcData = MapBattleNpcData(data.NpcData),
            KillCnt = data.KillCnt,
            BossLife = data.BossLife,
            TotalDamage = data.TotalDamage,
            CriticalCnt = data.CriticalCnt,
            SpecialMoveCnt = data.SpecialMoveCnt
        };
```

**Collection mapping pattern** (lines 134-155):

```csharp
private static CommonPlayResultData.BattleReleaseDataDto? MapBattleReleaseData(
    PlayResultRequest.ReleaseBattleData? data)
    => data is null
        ? null
        : new CommonPlayResultData.BattleReleaseDataDto
        {
            ReleaseInfoIds = (data.ReleaseInfoIds ?? []).ToList(),
            ReleaseBattleStageIds = (data.ReleaseBattleStageIds ?? []).ToList(),
            ReleaseNpcIds = (data.ReleaseNpcIds ?? []).ToList(),
            ReleaseNpcCostumeIds = (data.ReleaseNpcCostumeIds ?? []).ToList(),
            ReleaseNpcSpecialIds = (data.ReleaseNpcSpecialIds ?? []).ToList(),
            BattleTokenData = data.AryBattletokendatas.Select(MapBattleTokenData).ToList(),
            AssignNextStageId = data.AssignNextStageId
        };
```

**Tokkun wire reference** (read-only; `Wire/Game.cs` lines 2027-2038):

```csharp
[global::ProtoBuf.ProtoMember(44, Name = @"tokkun_tutorial_flg")]
public uint TokkunTutorialFlg
{
    get => __pbn__TokkunTutorialFlg.GetValueOrDefault();
    set => __pbn__TokkunTutorialFlg = value;
}
public bool ShouldSerializeTokkunTutorialFlg() => __pbn__TokkunTutorialFlg != null;
public void ResetTokkunTutorialFlg() => __pbn__TokkunTutorialFlg = null;
private uint? __pbn__TokkunTutorialFlg;

[global::ProtoBuf.ProtoMember(45, Name = @"ary_tokkunstage_info")]
public TokkunstageData AryTokkunstageInfo { get; set; }
```

**Tokkun stage wire reference** (read-only; `Wire/Game.cs` lines 2375-2397):

```csharp
public partial class TokkunstageData : global::ProtoBuf.IExtensible
{
    [global::ProtoBuf.ProtoMember(1, Name = @"banacoin_datetime", IsRequired = true)]
    public string BanacoinDatetime { get; set; }

    [global::ProtoBuf.ProtoMember(2, Name = @"tokkun_song_cnt", IsRequired = true)]
    public uint TokkunSongCnt { get; set; }

    [global::ProtoBuf.ProtoMember(3, Name = @"tookun_songno")]
    public uint[] TookunSongnoes { get; set; }

    [global::ProtoBuf.ProtoMember(4, Name = @"tokkun_speedchange_cnt", IsRequired = true)]
    public uint TokkunSpeedchangeCnt { get; set; }

    [global::ProtoBuf.ProtoMember(5, Name = @"tokkun_autoplay_cnt", IsRequired = true)]
    public uint TokkunAutoplayCnt { get; set; }

    [global::ProtoBuf.ProtoMember(6, Name = @"tokkun_jump_cnt", IsRequired = true)]
    public uint TokkunJumpCnt { get; set; }
```

**Apply this mapper shape:**

```csharp
IsTokkunPlayResult = request.AryTokkunstageInfo is not null,
TokkunTutorialFlg = request.ShouldSerializeTokkunTutorialFlg()
    ? request.TokkunTutorialFlg
    : null,
TokkunStageData = MapTokkunStageData(request.AryTokkunstageInfo),
```

```csharp
private static CommonPlayResultData.TokkunStageDataDto? MapTokkunStageData(
    PlayResultRequest.TokkunstageData? data)
    => data is null
        ? null
        : new CommonPlayResultData.TokkunStageDataDto
        {
            BanacoinDatetime = data.BanacoinDatetime ?? string.Empty,
            TokkunSongCnt = data.TokkunSongCnt,
            TookunSongnoes = (data.TookunSongnoes ?? []).ToList(),
            TokkunSpeedchangeCnt = data.TokkunSpeedchangeCnt,
            TokkunAutoplayCnt = data.TokkunAutoplayCnt,
            TokkunJumpCnt = data.TokkunJumpCnt
        };
```

**Do not copy:** any generated `Wire/` edits, any `PlayMode.Tokkun` enum addition, or any semantic conversion of `banacoin_datetime` / counts into payment, score, reward, unlock, or progression state.

---

### `Application/Handlers/UpdatePlayResultCommand.Blue.cs` (handler / service, request-response)

**Analogs:** `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`

**Shared dispatcher pattern** (`UpdatePlayResultCommand.cs` lines 17-27):

```csharp
public ValueTask<uint> Handle(UpdatePlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};

private partial ValueTask<uint> HandleNijiiro(UpdatePlayResultCommand request, CancellationToken cancellationToken);
private partial ValueTask<uint> HandleGreen(UpdatePlayResultCommand request, CancellationToken cancellationToken);
private partial ValueTask<uint> HandleBlue(UpdatePlayResultCommand request, CancellationToken cancellationToken);
```

**Current Blue branch ordering** (lines 18-36):

```csharp
if (request.Baid == 0)
{
    return 1;
}

var user = await context.UserData.FindAsync([request.Baid], cancellationToken);
if (user is null)
{
    logger.LogWarning("Game uploading a non existing Blue user with baid {Baid}", request.Baid);
    return 1;
}

var playResultData = request.PlayResultData;
if (playResultData.IsBattlePlayResult)
{
    return await HandleBlueBattle(request.Baid, playResultData, cancellationToken);
}

var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
```

**Normal write path Tokkun must bypass** (lines 36-103):

```csharp
var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
var blue = gameDataService.Blue();
var shopSeasonState = await context.GetOrCreateActiveBlueShopSeasonStateAsync(
    saveData,
    blue.ItemShopCatalog,
    cancellationToken);
var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;
...
ApplyUnlockBits(saveData, playResultData);
...
BlueProfileCounters.ApplyStage(saveData, stage);
await SaveBlueStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
...
await SaveBlueDanAsync(saveData, playResultData, blue, cancellationToken);

await context.SaveChangesAsync(cancellationToken);
await TrimBlueRecentSongsAsync(request.Baid, cancellationToken);
return 1;
```

**Battle write path Tokkun must bypass** (`UpdatePlayResultCommand.BlueBattle.cs` lines 5-29):

```csharp
private async ValueTask<uint> HandleBlueBattle(
    uint baid,
    CommonPlayResultData playResultData,
    CancellationToken cancellationToken)
{
    var now = DateTime.UtcNow;
    var playTime = ParseBluePlayDatetimeOrNow(playResultData.PlayDatetime);

    await context.AddBlueBattleStageResultsAsync(
        baid,
        playResultData,
        playTime,
        now,
        cancellationToken);
    await context.ApplyBlueBattleReleaseDataAsync(
        baid,
        playResultData.BattleReleaseData,
        now,
        cancellationToken);
    await AddBlueBattleShopDonmedalsAsync(baid, playResultData.GetDonmedal, now, cancellationToken);
    await UpsertBlueBattleRecentSongsAsync(baid, playResultData, playTime, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);
    await TrimBlueRecentSongsAsync(baid, cancellationToken);
    return 1;
}
```

**Apply this branch order in `HandleBlue`:**

```csharp
var playResultData = request.PlayResultData;
if (playResultData.IsTokkunPlayResult)
{
    logger.LogInformation(
        "Accepted Blue Tokkun playresult for baid {Baid}: play_mode={PlayMode}",
        request.Baid,
        playResultData.PlayMode);
    return 1;
}

if (playResultData.IsBattlePlayResult)
{
    return await HandleBlueBattle(request.Baid, playResultData, cancellationToken);
}
```

**Placement:** after guest and unknown-user exits, before `IsBattlePlayResult`, before `GetOrCreateBlueSaveDataAsync`, and before any normal/battle write helper. This preserves current unknown-user success behavior while ensuring Tokkun wins over normal-looking or battle-looking material.

**Error / validation pattern:** Blue playresult currently returns `1` for guest, unknown user, unsupported stage modes, invalid medal totals, and skipped unsupported data. Tokkun should follow the same success-shaped protocol boundary unless later client evidence proves a failure response.

---

### `Tests/Blue/BluePlayResultMapperTests.cs` (test, transform verification)

**Analogs:** `Tests/Blue/BluePlayResultMapperTests.cs`, `Tests/Blue/BlueBattlePlayResultMapperTests.cs`

**Imports and class shell** (`BluePlayResultMapperTests.cs` lines 1-7):

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BluePlayResultMapperTests
{
```

**Direct field preservation test pattern** (lines 8-58):

```csharp
[Fact]
public void Map_BluePlayResult_PreservesDirectRequestFields()
{
    var request = CreateRequest();
    request.ReleaseSongNoes = [104];
    request.GetToneNoes = [4];
    request.GetCostumeNo1s = [1];
    request.GetCostumeNo2s = [2];
    request.GetCostumeNo3s = [3];
    request.GetCostumeNo4s = [4];
    request.GetCostumeNo5s = [5];
    request.GetTitleNoes = [10];
    request.ItemshopTutorialFlg = 1;
    request.IsDevil = true;
    request.IsExplain = true;
    request.DifficultyPlayedCourse = 4;
    request.DifficultyPlayedStar = 8;
    request.AryCurrentCostume = new PlayResultRequest.CostumeData
    {
        Costume1 = 11,
        Costume2 = 12,
        Costume3 = 13,
        Costume4 = 14,
        Costume5 = 15
    };
    request.AryStageInfoes.Add(CreateStage(101, 1, 0));

    var common = PlayResultMappers.Map(request);

    Assert.Equal(1u, common.Baid);
    Assert.Equal("20260528120000", common.PlayDatetime);
    Assert.Equal([104u], common.ReleaseSongNoes);
```

**Current Tokkun-adjacent test to replace/expand** (lines 60-84):

```csharp
[Fact]
public void Map_BluePlayResult_DoesNotInferRuntimeSemanticsFromUnimplementedOptionalSections()
{
    var request = CreateRequest();
    request.AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
    {
        BanacoinDatetime = "20260528120000",
        TokkunSongCnt = 1,
        TookunSongnoes = [101],
        TokkunSpeedchangeCnt = 0,
        TokkunAutoplayCnt = 0,
        TokkunJumpCnt = 0
    };
    request.AryReleaseBattledata = new PlayResultRequest.ReleaseBattleData
    {
        AssignNextStageId = 2
    };
    request.AryStageInfoes.Add(CreateStage(101, 1, 0, includeBattle: true));

    var common = PlayResultMappers.Map(request);

    Assert.Equal(1u, common.Baid);
    Assert.Single(common.AryStageInfoes);
    Assert.Equal(101u, common.AryStageInfoes[0].SongNo);
}
```

**Raw client value assertion pattern** (`BlueBattlePlayResultMapperTests.cs` lines 37-85):

```csharp
[Fact]
public void Map_BattleSections_PreservesRawClientReportedValues()
{
    var request = CreateRequest();
    request.PlayMode = 6;
    request.AryReleaseBattledata = CreateReleaseBattleData();
    request.AryStageInfoes.Add(CreateStage(101, 4, 8, CreateBattleStageData()));

    var common = PlayResultMappers.Map(request);

    Assert.True(common.IsBattlePlayResult);
    Assert.Equal(6u, common.PlayMode);

    var stage = Assert.Single(common.AryStageInfoes);
    Assert.Equal(8u, stage.StageMode);
    var battleStage = stage.BattleStageData;
    Assert.NotNull(battleStage);
    Assert.Equal(3u, battleStage.SupportLv);
```

**Fixture helper pattern** (`BluePlayResultMapperTests.cs` lines 94-113 and 115-174):

```csharp
private static PlayResultRequest CreateRequest() => new()
{
    Baid = 1,
    ChassisId = "268410000000",
    ShopId = "JPN0JPN0123",
    PlayDatetime = "20260528120000",
    IsRight = false,
    CardType = 1,
    IsTwoPlayers = false,
    BonusDailyFlg = false,
    BonusWeeklyFlg = false,
    BonusMonthlyFlg = false,
    GetDonmedal = 0,
    GetKatsumedal = 0,
    GenderType = 0,
    PlayerAge = 0,
    PlayMode = 0,
    AreaCode = 1,
    Reserved = new byte[16]
};
```

**Mapper tests to add:**

- `Map_TokkunStageInfo_ClassifiesTokkunAndPreservesRawFacts`: set `AryTokkunstageInfo`, set `TokkunTutorialFlg`, assert `IsTokkunPlayResult`, `TokkunTutorialFlg`, `TokkunStageData`, `BanacoinDatetime`, `TokkunSongCnt`, `TookunSongnoes`, `TokkunSpeedchangeCnt`, `TokkunAutoplayCnt`, and `TokkunJumpCnt`.
- `Map_TutorialOnly_PreservesTutorialButDoesNotClassifyTokkun`: set only `TokkunTutorialFlg`, leave `AryTokkunstageInfo` null, assert `TokkunTutorialFlg` is present and `IsTokkunPlayResult` is false.
- Expand the mixed optional-section test so mapper preservation is explicit, but leave handler branch priority to handler tests.

**Do not add:** Tokkun source-word scans or allowlist-style mapper guards.

---

### `Tests/Blue/BluePlayResultHandlerTests.cs` (test, request-response with no-write assertions)

**Analogs:** `Tests/Blue/BluePlayResultHandlerTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `Tests/Blue/BlueHandlerFixture.cs`

**Imports and class shell** (`BluePlayResultHandlerTests.cs` lines 1-7):

```csharp
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BluePlayResultHandlerTests
{
```

**Unknown/guest success no-write pattern** (lines 8-48):

```csharp
[Fact]
public async Task UpdatePlayResult_Blue_GuestBaidDoesNotSave()
{
    await using var fixture = await BlueHandlerFixture.CreateAsync();
    var handler = CreateHandler(fixture);

    var result = await handler.Handle(new UpdatePlayResultCommand(
        0,
        GameEra.Blue,
        new CommonPlayResultData
        {
            Baid = 0,
            AryStageInfoes = [CreateStage(101, 1, 0)]
        }),
        CancellationToken.None);

    Assert.Equal(1u, result);
    Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
    Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
}
```

**Existing normal-write assertions to guard against Tokkun pollution** (lines 50-110):

```csharp
[Fact]
public async Task UpdatePlayResult_Blue_SavesPlayBestCountersAndUnlocks()
{
    await using var fixture = await BlueHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
    await fixture.Context.SaveChangesAsync();
    var handler = CreateHandler(fixture);
    ...
    Assert.Equal(10u, save!.TotalGetDonmedal);
    Assert.Equal(2u, save.TotalGetKatsumedal);
    Assert.True(BitIsSet(save.ReleaseSongFlg, 104));
    Assert.True(BitIsSet(save.ToneFlg, 4));
    Assert.True(BitIsSet(save.CostumeFlg1, 1));
    Assert.True(BitIsSet(save.CostumeFlg2, 2));
    Assert.True(BitIsSet(save.CostumeFlg3, 3));
    Assert.True(BitIsSet(save.CostumeFlg4, 4));
    Assert.True(BitIsSet(save.CostumeFlg5, 5));
    Assert.True(BitIsSet(save.TitleFlg, 10));
    Assert.Equal(1u, save.Costume1);
    Assert.Equal(1u, save.CategJpopCnt);
    Assert.Equal(1u, save.SongPushedCnt);
}
```

**Battle no-normal-state pattern to copy and make stricter** (`BlueBattlePlayResultHandlerTests.cs` lines 264-315):

```csharp
[Fact]
public async Task UpdatePlayResult_Blue_BattlePayloadLeavesExistingNormalStateUnchanged()
{
    await using var fixture = await BlueHandlerFixture.CreateAsync();
    var saveData = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
    saveData.TotalGetDonmedal = 5;
    saveData.TotalGetKatsumedal = 7;
    saveData.CategJpopCnt = 3;
    saveData.SongPushedCnt = 4;
    saveData.LastPlayDatetime = new DateTime(2026, 5, 1, 8, 0, 0);
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataBlue.Add(saveData);
    await fixture.Context.SaveChangesAsync();
    var handler = CreateHandler(fixture);
    ...
    Assert.Equal(1u, result);

    var reloaded = await fixture.Context.UserSaveDataBlue.SingleAsync(row => row.Baid == 1);
    Assert.Equal(5u, reloaded.TotalGetDonmedal);
    Assert.Equal(7u, reloaded.TotalGetKatsumedal);
    Assert.Equal(3u, reloaded.CategJpopCnt);
    Assert.Equal(4u, reloaded.SongPushedCnt);
    Assert.Equal(new DateTime(2026, 5, 1, 8, 0, 0), reloaded.LastPlayDatetime);
    Assert.False(BitIsSet(reloaded.ReleaseSongFlg, 104));
    Assert.False(BitIsSet(reloaded.ToneFlg, 4));
    Assert.False(BitIsSet(reloaded.CostumeFlg1, 1));
    Assert.False(BitIsSet(reloaded.TitleFlg, 10));

    await AssertBattleForbiddenNormalBlueStateEmptyAsync(fixture.Context);
}
```

**Forbidden normal state helper** (`BlueBattlePlayResultHandlerTests.cs` lines 477-485):

```csharp
private static async Task AssertBattleForbiddenNormalBlueStateEmptyAsync(TaikoDbContext context)
{
    Assert.Empty(await context.SongPlayDataBlue.ToListAsync());
    Assert.Empty(await context.SongBestDataBlue.ToListAsync());
    Assert.Empty(await context.BlueFavoriteSongs.ToListAsync());
    Assert.Empty(await context.DanScoreDataBlue.ToListAsync());
    Assert.Empty(await context.DanStageScoreDataBlue.ToListAsync());
    Assert.Empty(await context.BlueShopItemStates.ToListAsync());
}
```

**Handler factory pattern** (`BluePlayResultHandlerTests.cs` lines 447-454):

```csharp
private static UpdatePlayResultCommandHandler CreateHandler(
    BlueHandlerFixture fixture,
    ILogger<UpdatePlayResultCommandHandler>? logger = null)
{
    return new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        logger ?? NullLogger<UpdatePlayResultCommandHandler>.Instance);
}
```

**SQLite fixture pattern** (`BlueHandlerFixture.cs` lines 22-34):

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

**Handler tests to add:**

- Existing-user Tokkun returns `1` and leaves preexisting `UserSaveDataBlue` values unchanged.
- Unknown-user Tokkun returns `1` and creates no Blue normal, battle, shop, favorite, recent, Dani, or save rows.
- Mixed Tokkun plus normal-looking and battle-looking material returns `1`, routes through Tokkun first, and leaves `SongPlayDataBlue`, `SongBestDataBlue`, `BlueBattleStageResults`, `BlueBattleUserStates`, `BlueBattleNpcStates`, `BlueBattleTokenStates`, `BlueFavoriteSongs`, `BlueRecentSongs`, `DanScoreDataBlue`, `DanStageScoreDataBlue`, `BlueShopSeasonStates`, and `BlueShopItemStates` unchanged or empty as appropriate.

**Make Tokkun stricter than battle:** unlike battle, Tokkun must not add active shop Don medals or recent songs.

---

## Shared Patterns

### Direct Protobuf Controller Boundary

**Source:** `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` lines 7-18
**Apply to:** Do not add a new controller or route for Phase 9.

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
{
    Logger.LogInformation("Blue PlayResult request: {Request}", request.Stringify());
    var common = PlayResultMappers.Map(request);

    var result = await Mediator.Send(
        new UpdatePlayResultCommand(request.Baid, GameEra.Blue, common),
        HttpContext.RequestAborted);

    return Ok(PlayResultMappers.Map(result));
}
```

### Optional Field Presence

**Source:** `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` lines 33-58 and `Wire/Game.cs` lines 2027-2038
**Apply to:** `TokkunTutorialFlg` only.

Use `ShouldSerializeTokkunTutorialFlg()` for optional presence. Do not use tutorial presence as `IsTokkunPlayResult`.

### Branch Priority

**Source:** `Application/Handlers/UpdatePlayResultCommand.Blue.cs` lines 18-36
**Apply to:** Tokkun handler acceptance.

Tokkun branch placement must be:

1. After `request.Baid == 0` returns success.
2. After unknown Blue user returns success.
3. Before `IsBattlePlayResult`.
4. Before `GetOrCreateBlueSaveDataAsync`.
5. Before normal, battle, Dani, favorite/recent, shop, unlock, medal, customization, title, profile, score, and crown writes.

### No `PlayMode.Tokkun`

**Source:** `Domain/Enums/PlayMode.cs` lines 3-8
**Apply to:** All Phase 9 implementation and tests.

```csharp
public enum PlayMode
{
    Normal = 0,
    DanMode = 1,
    GaidenMode = 4,
    AiBattle = 6
}
```

Preserve/log raw `PlayMode` only as context. Do not add or depend on a guessed Tokkun numeric value.

### Wire And Proto References

**Source:** `proto/blue/taiko.proto` lines 441-452
**Apply to:** Mapper field list only.

```protobuf
optional uint32 payment_method = 43;
optional uint32 tokkun_tutorial_flg = 44;

optional TokkunstageData ary_tokkunstage_info = 45;
message TokkunstageData {
    required string banacoin_datetime = 1;
    required uint32 tokkun_song_cnt = 2;
    repeated uint32 tookun_songno = 3;
    required uint32 tokkun_speedchange_cnt = 4;
    required uint32 tokkun_autoplay_cnt = 5;
    required uint32 tokkun_jump_cnt = 6;
}
```

Generated `Wire/` files are reference-only. Do not modify them manually.

### No Source-Word Guards

**Source:** `Tests/Blue/BlueA4SourceGuardTests.cs` lines 5-31 show old Green leakage source guards; `09-CONTEXT.md` D-12 forbids Tokkun word bans and allowlist-style guards.
**Apply to:** Phase 9 tests.

Use behavior tests through the real mapper/handler and SQLite fixture. Do not add Tokkun source-scanning tests.

## No Analog Found

All Phase 9 target files have close analogs in the current Blue playresult and Blue battle code.

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| None | - | - | - |

## Metadata

**Analog search scope:** `Application`, `Adapters.GameProtocol.Blue`, `Tests/Blue`, `Domain`, `proto`
**Files scanned:** 367
**Focused candidate files scanned:** 27
**Analogs read:** `CommonPlayResultData.cs`, `CommonPlayResultData.Green.cs`, `CommonPlayResultData.BlueBattle.cs`, `PlayResultMappers.cs`, `PlayResultController.cs`, `UpdatePlayResultCommand.cs`, `UpdatePlayResultCommand.Blue.cs`, `UpdatePlayResultCommand.BlueBattle.cs`, `BluePlayResultMapperTests.cs`, `BlueBattlePlayResultMapperTests.cs`, `BluePlayResultHandlerTests.cs`, `BlueBattlePlayResultHandlerTests.cs`, `BlueHandlerFixture.cs`, `BlueItemShopStateTests.cs`, `BlueA4SourceGuardTests.cs`, `BlueBattleSourceGuardTests.cs`, `PlayMode.cs`, targeted `Wire/Game.cs`, targeted `proto/blue/taiko.proto`
**Pattern extraction date:** 2026-06-05
