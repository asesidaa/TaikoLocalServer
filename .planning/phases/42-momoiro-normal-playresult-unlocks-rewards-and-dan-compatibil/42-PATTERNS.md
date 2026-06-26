# Phase 42: MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility - Pattern Map

**Mapped:** 2026-06-26
**Files analyzed:** 22 candidate new/modified files
**Analogs found:** 22 / 22
**Research artifact:** no phase-local `42-RESEARCH.md` exists in this checkout

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/PlayResultController.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs` | mapper | transform | `Adapters.GameProtocol.Kimidori/Mappers/PlayResultMappers.cs` and `Adapters.GameProtocol.Murasaki/Mappers/PlayResultMappers.cs` | exact |
| `Application/Handlers/UpdatePlayResultCommand.cs` | handler dispatcher | request-response | existing AC15 dispatch arms in same file | exact |
| `Application/Handlers/UpdatePlayResultCommand.Momoiro.cs` | handler/service | CRUD mutation | `Application/Handlers/UpdatePlayResultCommand.Kimidori.cs` | exact |
| `Application/Ac15/Ac15NormalPlayMapper.cs` | mapper utility | transform | existing Kimidori/Murasaki mapper entries in same file | exact |
| `Application/Ac15/Ac15UnlockFlagAccess.cs` | mutation utility | transform/CRUD flags | existing Kimidori/Murasaki entries in same file | exact |
| `Application/Ac15/Ac15ProfileCounterUpdater.cs` | mutation utility | transform/CRUD counters | existing Kimidori/Murasaki entries in same file | exact |
| `Domain/Entities/SongPlayDatumMomoiro.cs` | model | CRUD | `Domain/Entities/SongPlayDatumKimidori.cs` | exact |
| `Application/Abstractions/ITaikoDbContext.Momoiro.cs` | persistence port | CRUD | `Application/Abstractions/ITaikoDbContext.Kimidori.cs` | exact |
| `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs` | persistence config | CRUD | `Infrastructure/Persistence/TaikoDbContext.Kimidori.cs` | exact |
| `Infrastructure/Persistence/Migrations/<timestamp>_AddMomoiroPlayResultState.cs` | migration | CRUD schema | `Infrastructure/Persistence/TaikoDbContext.Kimidori.cs` plus existing Momoiro migration | role-match |
| `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs` | test | request-response/CRUD | `Tests/Murasaki/MurasakiRuntimeHandlerTests.cs` | exact |
| `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` | test | request-response/transform | `Adapters.GameProtocol.Kimidori/Controllers/PlayResultController.cs` and current Momoiro controller tests | role-match |
| `Tests/Momoiro/MomoiroHandlerFixture.cs` | test fixture | CRUD/file-I/O surrogate | existing Momoiro fixture and Murasaki handler fixture | exact |
| `Domain/Entities/DanScoreDatumMomoiro.cs` | model | CRUD | `Domain/Entities/DanScoreDatumKimidori.cs` | conditional exact |
| `Domain/Entities/DanStageScoreDatumMomoiro.cs` | model | CRUD | `Domain/Entities/DanStageScoreDatumKimidori.cs` | conditional exact |
| `Application/Ac15/Ac15DaniMapper.cs` | mapper utility | transform | existing Kimidori/Murasaki entries in same file | conditional exact |
| `Application/Ac15/Ac15EraProfiles.cs` | config | transform/readback gating | existing Kimidori/Murasaki/Momoiro profiles in same file | conditional exact |
| `Application/Abstractions/IMomoiroCatalog.cs` | catalog port | read-only lookup | `Application/Abstractions/IKimidoriCatalog.cs` or `IMurasakiCatalog.cs` | conditional role-match |
| `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs` | catalog service | file-I/O/read-only lookup | `Infrastructure/GameDataCatalog/Kimidori/KimidoriEraGameDataCatalog.cs` | conditional role-match |
| `Application/Handlers/UserDataQuery.Momoiro.cs` | readback handler | request-response/CRUD read | existing file | conditional review |
| `Application/Handlers/BaidQuery.Momoiro.cs` | readback handler | request-response/CRUD read | existing file | conditional review |

## Pattern Assignments

### `Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs` (controller, request-response)

**Analog:** `Adapters.GameProtocol.Kimidori/Controllers/PlayResultController.cs`

**Current Momoiro scaffold to replace** (lines 8-20):

```csharp
[HttpPost(MomoiroRoutePrefixes.Game + "/playresult.php")]
[Produces("application/protobuf")]
public IActionResult PlayResult([FromBody] PlayResultRequest request)
{
    Logger.LogInformation(
        "Momoiro playresult.php scaffold request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}, StageCount={StageCount}",
        request.Baid,
        request.ChassisId,
        request.ShopId,
        request.AryStageInfoes.Count);

    return Ok(new PlayResultResponse { Result = 1 });
}
```

**Copy controller shape from Kimidori** (lines 6-15):

```csharp
[HttpPost(KimidoriRoutePrefixes.Game + "/playresult.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
{
    Logger.LogInformation("Kimidori PlayResult request: {@Request}", request);
    var playResult = PlayResultMappers.Map(request);
    var result = await Mediator.Send(
        new UpdateAc15PlayResultCommand(request.Baid, GameEra.Kimidori, playResult),
        HttpContext.RequestAborted);
    return Ok(PlayResultMappers.Map(result));
}
```

**Apply to Momoiro:**
Use `MomoiroRoutePrefixes.Game`, `GameEra.Momoiro`, and a new `Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs`. Keep controller work limited to log, map, Mediator send, and map response.

**Scope warning:**
Do not add business behavior to the controller. AGENTS.md says controllers deserialize, map, call Mediator, and map back.

### `Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs` (mapper, transform)

**Analog:** `Adapters.GameProtocol.Kimidori/Mappers/PlayResultMappers.cs`

**Mapper header and envelope mapping** (lines 5-18):

```csharp
[Mapper(
    AllowNullPropertyAssignment = false,
    ThrowOnMappingNullMismatch = false,
    ThrowOnPropertyMappingNullMismatch = false)]
public static partial class PlayResultMappers
{
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Metadata), Use = nameof(MapMetadata))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Profile), Use = nameof(MapProfile))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Normal), Use = nameof(MapNormal))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Dani), Use = nameof(MapDani))]
    [MapValue(nameof(Ac15PlayResultEnvelope.Tokkun), null)]
    [MapValue(nameof(Ac15PlayResultEnvelope.BlueBattle), null)]
    [MapValue(nameof(Ac15PlayResultEnvelope.GreenGhost), null)]
    public static partial Ac15PlayResultEnvelope Map(PlayResultRequest request);
```

**Profile field mapping pattern** (lines 31-54):

```csharp
[MapProperty(nameof(PlayResultRequest.GetDonpoint), nameof(Ac15ProfileMutationFacts.GetDonpoint), Use = nameof(MapUInt))]
[MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.RewardPtn), Use = nameof(MapRewardPtn))]
[MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.RewardProgress), Use = nameof(MapRewardProgress))]
[MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.IsDevil), Use = nameof(MapIsDevil))]
[MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.IsExplain), Use = nameof(MapIsExplain))]
[MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasAryCurrentCostume), Use = nameof(HasCurrentCostume))]
[MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.AryCurrentCostume), Use = nameof(MapCurrentCostume))]
[MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.GetDonmedal))]
[MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.GetKatsumedal))]
...
private static partial Ac15ProfileMutationFacts MapProfile(PlayResultRequest request);
```

**Stage and challenge-array mapping pattern** (lines 64-85):

```csharp
[MapProperty(nameof(PlayResultRequest.StageData.HitCnt), nameof(Ac15StageResult.HitCnt), Use = nameof(MapUInt))]
[MapProperty(nameof(PlayResultRequest.StageData.OptionFlg), nameof(Ac15StageResult.OptionFlg), Use = nameof(MapBytes))]
[MapProperty(nameof(PlayResultRequest.StageData.ToneFlg), nameof(Ac15StageResult.ToneFlg), Use = nameof(MapBytes))]
[MapProperty(nameof(PlayResultRequest.StageData.PlayDan), nameof(Ac15StageResult.PlayDan), Use = nameof(MapPlayDan))]
[MapProperty(nameof(PlayResultRequest.StageData.AryChallengeIds), nameof(Ac15StageResult.ChallengeIds), Use = nameof(MapCompeList))]
[MapProperty(nameof(PlayResultRequest.StageData.AryUserCompeIds), nameof(Ac15StageResult.UserCompeIds), Use = nameof(MapCompeList))]
[MapProperty(nameof(PlayResultRequest.StageData.AryBngCompeIds), nameof(Ac15StageResult.BngCompeIds), Use = nameof(MapCompeList))]
[MapPropertyFromSource(nameof(Ac15StageResult.SoulGauge), Use = nameof(MapSoulGauge))]
[MapPropertyFromSource(nameof(Ac15StageResult.HitCount), Use = nameof(MapHitCount))]
...
[UserMapping(Default = true)]
private static partial Ac15StageResult MapStage(PlayResultRequest.StageData stage);
```

**Generated Momoiro wire fields to preserve** (lines 977-1078, 1172-1180, 1235-1287):

```csharp
[global::ProtoBuf.ProtoMember(10, Name = @"release_song_no")]
public uint[] ReleaseSongNoes { get; set; }
...
[global::ProtoBuf.ProtoMember(18, Name = @"get_donpoint")]
public uint? GetDonpoint { get; set; }
...
[global::ProtoBuf.ProtoMember(20, Name = @"reward_progress")]
public uint? RewardProgress { get; set; }
...
[global::ProtoBuf.ProtoMember(35, Name = @"dan_result")]
public uint? DanResult { get; set; }
...
[global::ProtoBuf.ProtoMember(5, Name = @"ary_challenge_id")]
public List<ResultcompeData> AryChallengeIds { get; } = [];
...
[global::ProtoBuf.ProtoMember(22, Name = @"play_dan", IsRequired = true)]
public uint PlayDan { get; set; }
```

**Apply to Momoiro:**
Create a Momoiro mapper by copying the Kimidori/Murasaki mapper, then adjust only for actual Momoiro wire optionality. Preserve direct protobuf request/response mapping. Map challenge-shaped arrays into `Ac15StageResult` facts, but do not make them stateful by mapping alone.

**Scope warning:**
Generated wire presence is not persistence proof. Per Phase 42 context, stateful song unlock, Don Point/reward, Dani, and challenge behavior also needs Momoiro binary/client evidence. If evidence remains incomplete, map and log/ignore rather than persist.

### `Application/Handlers/UpdatePlayResultCommand.cs` (handler dispatcher, request-response)

**Analog:** existing AC15 dispatcher

**Current AC15 dispatch** (lines 25-35):

```csharp
public ValueTask<uint> Handle(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    GameEra.Yellow => HandleYellow(request, cancellationToken),
    GameEra.Red => HandleRed(request, cancellationToken),
    GameEra.White => HandleWhite(request, cancellationToken),
    GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
    GameEra.Kimidori => HandleKimidori(request, cancellationToken),
    _ => throw new InvalidOperationException($"Unsupported AC15 playresult command era: {request.Era}")
};
```

**Current partial declarations** (lines 38-44):

```csharp
private partial ValueTask<uint> HandleGreen(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
private partial ValueTask<uint> HandleBlue(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
private partial ValueTask<uint> HandleYellow(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
private partial ValueTask<uint> HandleRed(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
private partial ValueTask<uint> HandleWhite(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
private partial ValueTask<uint> HandleMurasaki(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
private partial ValueTask<uint> HandleKimidori(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
```

**Apply to Momoiro:**
Add `GameEra.Momoiro => HandleMomoiro(...)` and a `private partial ValueTask<uint> HandleMomoiro(...)` declaration. Keep Momoiro in the AC15 `UpdateAc15PlayResultCommand` path, not the Nijiiro `UpdatePlayResultCommand` path.

### `Application/Handlers/UpdatePlayResultCommand.Momoiro.cs` (handler/service, CRUD mutation)

**Primary analog:** `Application/Handlers/UpdatePlayResultCommand.Kimidori.cs`

**Normal-play handler skeleton** (lines 10-44):

```csharp
private partial async ValueTask<uint> HandleKimidori(
    UpdateAc15PlayResultCommand request,
    CancellationToken cancellationToken)
{
    if (request.Baid == 0)
    {
        return 1;
    }

    var user = await context.UserData.FindAsync([request.Baid], cancellationToken);
    if (user is null)
    {
        logger.LogWarning("Game uploading a non existing Kimidori user with baid {Baid}", request.Baid);
        return 1;
    }

    var playResultData = request.PlayResultData;
    var normal = playResultData.Normal;
    IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];

    var validStages = Ac15NormalStageFilter.Filter(
        request.Baid,
        stages,
        Ac15EraProfiles.Kimidori.Limits,
        Ac15NormalStagePolicies.Standard,
        logger);
```

**Profile/reward/unlock mutation pattern** (lines 42-56):

```csharp
var saveData = await context.GetOrCreateKimidoriSaveDataAsync(request.Baid, cancellationToken);
var kimidori = gameDataService.Kimidori();
var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
if (!Ac15CommonProfileMutation.TryApplyDonPoints(
        saveData,
        playResultData.Profile,
        validStages,
        Ac15ProfileCounterUpdater.Kimidori,
        Ac15UnlockFlagAccess.Kimidori,
        Ac15EraProfiles.Kimidori.Limits,
        playTime))
{
    logger.LogWarning("Rejecting invalid Kimidori Don point totals for baid {Baid}", request.Baid);
    return 1;
}
```

**Normal writer pattern** (lines 82-88):

```csharp
await Ac15NormalPlayWriter.SaveAsync(
    context,
    KimidoriNormalPlayTables(),
    new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Kimidori.Limits, playTime),
    Ac15NormalStagePolicies.Standard,
    cancellationToken);
return 1;
```

**Era-owned table bundle pattern** (lines 91-98):

```csharp
private Ac15NormalPlayTables<SongPlayDatumKimidori, SongBestDatumKimidori, KimidoriFavoriteSongs, KimidoriRecentSongs> KimidoriNormalPlayTables()
    => new(
        context.SongPlayDataKimidori,
        context.SongBestDataKimidori,
        context.KimidoriFavoriteSongs,
        context.KimidoriRecentSongs,
        Ac15NormalPlayMapper.ToKimidoriSongPlayDatum,
        Ac15NormalPlayMapper.ToKimidoriSongBestDatum);
```

**Apply to Momoiro:**
Create `HandleMomoiro` with the Kimidori/Murasaki sequence, replacing all era surfaces with Momoiro:

- `context.GetOrCreateMomoiroSaveDataAsync`
- `gameDataService.Momoiro()`
- `Ac15EraProfiles.Momoiro.Limits`
- `Ac15ProfileCounterUpdater.Momoiro`
- `Ac15UnlockFlagAccess.Momoiro`
- `MomoiroNormalPlayTables()`
- `SongPlayDatumMomoiro`, `SongBestDatumMomoiro`, `MomoiroFavoriteSongs`, `MomoiroRecentSongs`
- `Ac15NormalPlayMapper.ToMomoiroSongPlayDatum`
- `Ac15NormalPlayMapper.ToMomoiroSongBestDatum`

**Favorite-order warning:**
`Ac15NormalPlayWriter.SetFavoriteAsync` adds `new TFavorite { Baid = baid, SongNo = songNo }` (lines 96-118). `MomoiroFavoriteSongs` has `DisplayOrder` (lines 3-8), and `UserDataQuery.Momoiro` reads favorites by `DisplayOrder` then `SongNo` (lines 18-24). Do not blindly let all playresult-added Momoiro favorites default to display order `0` unless Phase 42 evidence explicitly accepts that behavior. Either derive/preserve `DisplayOrder` conservatively or gate favorite mutation until the contract is proven.

### `Application/Ac15/Ac15NormalPlayWriter.cs` (shared writer, CRUD mutation)

**Analog/source:** same file

**What it already owns** (lines 38-65):

```csharp
foreach (var stage in request.Stages)
{
    var difficulty = MapDifficulty(stage.Level);
    var crown = MapCrown(stage.PlayResult);
    var isShin = stage.StageMode == 1 || stage.StageMode == 4;
    var bestPolicy = policy.GetBestUpdatePolicy(stage, crown);
    var playRow = ToPlayRow(request.Baid, request.PlayMode, stage, difficulty, crown, isShin, request.PlayTime);
    var play = tables.CreatePlay(playRow);
    tables.PlayRows.Add(play);
    tables.AfterAddPlayRow?.Invoke(play, playRow);

    if (request.PlayMode != (uint)PlayMode.DanMode || isShin)
    {
        await UpsertBestAsync(...);
    }

    await SetFavoriteAsync(tables.FavoriteRows, request.Baid, stage.SongNo, stage.IsFavorite, request.Limits.MaxFavoriteSongs, cancellationToken);
    await UpsertRecentAsync(tables.RecentRows, request.Baid, stage.SongNo, request.PlayTime, cancellationToken);
}

await context.SaveChangesAsync(cancellationToken);
await TrimRecentAsync(tables.RecentRows, context.SaveChangesAsync, request.Baid, request.Limits.MaxRecentSongs, cancellationToken);
```

**Best update pattern** (lines 77-93):

```csharp
var existing = await bestRows.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
if (existing is null)
{
    bestRows.Add(create(baid, row, policy.AllowCrownUpdate));
    return;
}

if (policy.AllowScoreUpdate && row.BestScore > existing.BestScore)
{
    existing.BestScore = row.BestScore;
    existing.BestRate = row.BestRate;
}

if (policy.AllowCrownUpdate && CrownRank(row.BestCrown) > CrownRank(existing.BestCrown))
{
    existing.BestCrown = row.BestCrown;
}
```

**Recent update pattern** (lines 121-158):

```csharp
public static async ValueTask UpsertRecentAsync<TRecent>(
    DbSet<TRecent> recents,
    uint baid,
    uint songNo,
    DateTime playTime,
    CancellationToken cancellationToken)
...
recent.LastPlayed = playTime;
...
.OrderByDescending(song => song.LastPlayed)
.Skip(maxRecent)
```

**Apply to Momoiro:**
Reuse for score history, self-best rows, crown-source rows, and recent rows. Extend carefully if Momoiro favorite `DisplayOrder` needs an era-specific hook. Avoid changing behavior for other eras while solving Momoiro ordering.

### `Application/Ac15/Ac15CommonProfileMutation.cs` (shared profile/reward/unlock mutation)

**Analog/source:** same file

**Don Point profile mutation pattern** (lines 55-89):

```csharp
public static bool TryApplyDonPoints<TSave>(
    TSave saveData,
    Ac15ProfileMutationFacts profile,
    IReadOnlyList<Ac15StageResult> countedStages,
    Ac15ProfileCounterAccess<TSave> counterAccess,
    Ac15UnlockFlagAccess<TSave> unlockAccess,
    Ac15ProtocolLimits limits,
    DateTime playTime)
    where TSave :
        IAc15DonPointSaveData,
        IAc15PlayTutorialSaveData,
        IAc15PlayProfileSaveData,
        IAc15CustomizationSaveData
{
    if (!CanAdd(saveData.TotalGetDonpoint, profile.GetDonpoint))
    {
        return false;
    }

    saveData.TotalGetDonpoint += profile.GetDonpoint;
    saveData.RewardPtn = profile.RewardPtn ?? saveData.RewardPtn;
    saveData.RewardProgress = profile.RewardProgress ?? saveData.RewardProgress;
    saveData.DifficultyTutorialFlg = PreserveTutorialFlag(saveData.DifficultyTutorialFlg, profile.DifficultyTutorialFlg);
```

**Shared unlock/counter pattern** (lines 116-135):

```csharp
saveData.LastPlayDatetime = playTime;
saveData.PrevAreaCode = profile.AreaCode;
...
unlockAccess.ReleaseSongs?.Invoke(saveData, profile.ReleaseSongNoes.Where(id => id < (uint)limits.SongFlagBytes * 8));
unlockAccess.Tones(saveData, profile.GetToneNoes.Where(id => id < (uint)limits.ToneFlagBytes * 8));
unlockAccess.Titles(saveData, profile.GetTitleNoes.Where(id => id < (uint)limits.TitleFlagBytes * 8));
...
foreach (var stage in countedStages)
{
    Ac15ProfileCounterUpdater.ApplyStage(saveData, stage, counterAccess);
}
```

**Apply to Momoiro:**
Use `TryApplyDonPoints`, not `TryApply`, because Momoiro Phase 41 save state has Don Point/reward fields and no shop-season/Don medal authority. This fits `UserSaveDataMomoiro` lines 3-7 and 37-41.

**Scope warning:**
Only use this after Momoiro playresult research confirms `get_donpoint`, `reward_ptn`, `reward_progress`, `release_song_no`, and unlock flags are normal playresult write fields. If not confirmed, map them but do not call mutators for those fields.

### `Application/Ac15/Ac15UnlockFlagAccess.cs` (shared unlock accessor)

**Analog/source:** same file

**Kimidori entry to copy** (lines 75-83):

```csharp
public static Ac15UnlockFlagAccess<UserSaveDataKimidori> Kimidori { get; } = new(
    (save, ids) => save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, ids, Ac15EraProfiles.Kimidori.Limits.SongFlagBytes),
    (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, Ac15EraProfiles.Kimidori.Limits.ToneFlagBytes),
    (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, Ac15EraProfiles.Kimidori.Limits.TitleFlagBytes),
    (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes),
    (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes),
    (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes),
    (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes),
    (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes));
```

**Apply to Momoiro:**
Add `public static Ac15UnlockFlagAccess<UserSaveDataMomoiro> Momoiro` using `Ac15EraProfiles.Momoiro.Limits.*` and Momoiro save fields.

**Forbidden analog:**
Do not copy Green's `ReleaseSongs: null` (lines 25-33) if Phase 42 proves Momoiro `release_song_no`; do not add any item-shop policy entry in `Ac15ItemShopUnlockPolicies` because Momoiro item-shop authority is out of scope.

### `Application/Ac15/Ac15ProfileCounterUpdater.cs` (shared profile counters)

**Analog/source:** same file

**Kimidori entry to copy** (lines 85-96):

```csharp
public static Ac15ProfileCounterAccess<UserSaveDataKimidori> Kimidori { get; } = new(
    Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
    Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
    Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
    Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
    Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
    Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
    Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
    Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
    Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
    Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
    Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));
```

**Counter application behavior** (lines 119-143):

```csharp
public static void ApplyStage<TSave>(
    TSave saveData,
    Ac15StageResult stage,
    Ac15ProfileCounterAccess<TSave> counters)
{
    if (GenreCounter(counters, stage.MusicCateg) is { } genre)
    {
        Increment(saveData, genre);
    }

    if (stage.IsPushed)
    {
        Increment(saveData, counters.Pushed);
    }
    ...
}
```

**Apply to Momoiro:**
Add `Momoiro` accessors pointing to `UserSaveDataMomoiro` counter fields (lines 43-54 of `Domain/Entities/UserSaveDataMomoiro.cs`). Use only with valid normal stages.

### `Application/Ac15/Ac15NormalPlayMapper.cs` (mapper utility)

**Analog/source:** same file

**Song play mapper entries to copy** (lines 28-34):

```csharp
[MapperIgnoreTarget(nameof(SongPlayDatumMurasaki.Id))]
[MapperIgnoreTarget(nameof(SongPlayDatumMurasaki.Ba))]
public static partial SongPlayDatumMurasaki ToMurasakiSongPlayDatum(Ac15PlayRow row);

[MapperIgnoreTarget(nameof(SongPlayDatumKimidori.Id))]
[MapperIgnoreTarget(nameof(SongPlayDatumKimidori.Ba))]
public static partial SongPlayDatumKimidori ToKimidoriSongPlayDatum(Ac15PlayRow row);
```

**Best mapper entries to copy** (lines 96-118):

```csharp
public static SongBestDatumKimidori ToKimidoriSongBestDatum(uint baid, Ac15BestRow row, bool allowCrownUpdate)
{
    var best = ToKimidoriSongBestDatum(row);
    best.Baid = baid;
    if (!allowCrownUpdate)
    {
        best.BestCrown = CrownType.None;
    }

    return best;
}
```

**Apply to Momoiro:**
Add `ToMomoiroSongPlayDatum`, `ToMomoiroSongBestDatum(uint, Ac15BestRow, bool)`, and private partial `ToMomoiroSongBestDatum(Ac15BestRow row)`, mirroring Kimidori/Murasaki. Inspect generated Mapperly output after build if warnings or required mappings appear.

### `Domain/Entities/SongPlayDatumMomoiro.cs` (model, CRUD)

**Analog:** `Domain/Entities/SongPlayDatumKimidori.cs`

**Copy entity shape** (lines 5-38):

```csharp
public partial class SongPlayDatumKimidori : IAc15SongPlayDatum
{
    public long Id { get; set; }
    public uint Baid { get; set; }
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public CrownType Crown { get; set; }
    public uint Score { get; set; }
    public uint ScoreRate { get; set; }
    ...
    public uint PlayDan { get; set; }
    public uint WaiwaiResult { get; set; }
    public uint WaiwaiGauge { get; set; }
    public DateTime PlayTime { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

**Apply to Momoiro:**
Create `SongPlayDatumMomoiro` with the same shape unless Momoiro playresult evidence proves a narrower/wider persisted stage fact set. This table is required for play history facts even though Phase 41 only created best rows.

### `Application/Abstractions/ITaikoDbContext.Momoiro.cs` and `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs` (persistence port/config, CRUD)

**Existing Momoiro port** (lines 3-8):

```csharp
public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataMomoiro> UserSaveDataMomoiro { get; }
    DbSet<SongBestDatumMomoiro> SongBestDataMomoiro { get; }
    DbSet<MomoiroFavoriteSongs> MomoiroFavoriteSongs { get; }
    DbSet<MomoiroRecentSongs> MomoiroRecentSongs { get; }
}
```

**Kimidori analog with play and Dan tables** (lines 5-11):

```csharp
DbSet<UserSaveDataKimidori> UserSaveDataKimidori { get; }
DbSet<SongBestDatumKimidori> SongBestDataKimidori { get; }
DbSet<SongPlayDatumKimidori> SongPlayDataKimidori { get; }
DbSet<KimidoriFavoriteSongs> KimidoriFavoriteSongs { get; }
DbSet<KimidoriRecentSongs> KimidoriRecentSongs { get; }
DbSet<DanScoreDatumKimidori> DanScoreDataKimidori { get; }
DbSet<DanStageScoreDatumKimidori> DanStageScoreDataKimidori { get; }
```

**Existing Momoiro EF config** (lines 8-11, 27-64):

```csharp
public virtual DbSet<UserSaveDataMomoiro> UserSaveDataMomoiro { get; set; } = null!;
public virtual DbSet<SongBestDatumMomoiro> SongBestDataMomoiro { get; set; } = null!;
public virtual DbSet<MomoiroFavoriteSongs> MomoiroFavoriteSongs { get; set; } = null!;
public virtual DbSet<MomoiroRecentSongs> MomoiroRecentSongs { get; set; } = null!;
...
modelBuilder.Entity<SongBestDatumMomoiro>(entity =>
{
    entity.ToTable("SongBestDatum_Momoiro");
    entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty, e.IsShin });
    entity.HasIndex(e => new { e.SongId, e.Difficulty, e.BestScore });
    ...
});
...
modelBuilder.Entity<MomoiroFavoriteSongs>(entity =>
{
    entity.ToTable("MomoiroFavoriteSongs");
    entity.HasKey(e => new { e.Baid, e.SongNo });
    entity.HasIndex(e => new { e.Baid, e.DisplayOrder });
```

**Kimidori EF play/Dan analog** (lines 46-60, 86-108):

```csharp
modelBuilder.Entity<SongPlayDatumKimidori>(entity =>
{
    entity.ToTable("SongPlayDatum_Kimidori");
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => new { e.Baid, e.SongId, e.Difficulty, e.PlayTime });
    entity.Property(e => e.Id).ValueGeneratedOnAdd();
    entity.Property(e => e.PlayTime).HasColumnType("datetime");
    ...
    entity.Property(e => e.Difficulty).HasConversion<uint>();
    entity.Property(e => e.Crown).HasConversion<uint>();
});
...
modelBuilder.Entity<DanScoreDatumKimidori>(entity =>
{
    entity.ToTable("DanScoreDatum_Kimidori");
    entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra });
    entity.HasIndex(e => e.MedleyUniqueId);
```

**Apply to Momoiro:**
Add `SongPlayDataMomoiro` to the port and DbContext. Add Dan DbSets and EF config only if Momoiro Dan is proven. Generate a migration after source updates, then inspect that it adds only Momoiro-owned tables/columns for this phase.

**Scope warning:**
Do not remove or reorder `MomoiroFavoriteSongs.DisplayOrder`; Phase 41 intentionally made it the favorite readback order contract.

### Conditional Dani Files (only if Momoiro evidence proves MORUN-03)

**Candidate files:**

- `Domain/Entities/DanScoreDatumMomoiro.cs`
- `Domain/Entities/DanStageScoreDatumMomoiro.cs`
- `Application/Ac15/Ac15DaniMapper.cs`
- `Application/Handlers/UpdatePlayResultCommand.Momoiro.cs`
- `Application/Abstractions/IMomoiroCatalog.cs`
- `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs`
- `Application/Ac15/Ac15EraProfiles.cs`

**Dani writer analog** (lines 19-64 of `Application/Ac15/Ac15DaniWriter.cs`):

```csharp
if (playResultData is null)
{
    return;
}

var danIds = playResultData.Stages
    .Select(stage => stage.PlayDan.GetValueOrDefault())
    .Where(dan => dan != 0)
    .Distinct()
    .ToArray();

if (danIds.Length != 1)
{
    logger.LogWarning("Skipping AC15 Dani save for baid {Baid}: expected one PlayDan value, got {Count}", saveState.Baid, danIds.Length);
    return;
}
...
if (challenge is null
    || !Ac15DanHelpers.IsKnownDanId(danId, limits)
    || !knownChallengeLevels.Contains(danId))
{
    logger.LogWarning("Skipping AC15 Dani save for baid {Baid}: unknown Dan id {DanId}", saveState.Baid, danId);
    return;
}
```

**Handler analog from Kimidori** (lines 58-80):

```csharp
var dani = playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode && playResultData.Dani is { } inputDani
    ? inputDani with { Stages = validStages.ToList() }
    : null;

await Ac15DaniWriter.SaveAsync(
    KimidoriDaniTables(),
    dani,
    Ac15EraProfiles.Kimidori.Limits,
    kimidori.DaniFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
    new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, KimidoriDanCostumeId),
    update =>
    {
        saveData.GotDanFlg = update.GotDanFlg;
        saveData.GotDanExtraFlg = update.GotDanExtraFlg;
        saveData.GotDanMax = update.GotDanMax;
        saveData.DispTaikojukuDan = update.DisplayDan;
        if (update.ApplyDanCostume)
```

**Current Momoiro profile/capability state** (lines 35-40, 119-128 of `Application/Ac15/Ac15EraProfiles.cs`):

```csharp
private static readonly Ac15FeatureSet MomoiroFeatures = KimidoriFeatures with
{
    Folders = false,
    Dani = false,
    ItemShop = false
};
...
public static Ac15EraProfile Momoiro { get; } = new(
    GameEra.Momoiro,
    MomoiroFeatures,
    CreateMomoiroLimits(),
    new Ac15WirePlacement(
        CrownPlacement: Ac15CrownWirePlacement.UserData,
        HasInitialDataItemShopRows: false,
        HasInitialDataLegalTermsRows: false,
        HasTokkunTutorialFlagInUserData: false),
    Ac15ProfileCapabilities.CurrentWithoutTitlePlateOrTaikojuku);
```

**Current Momoiro catalog limitation** (lines 5-16 of `Application/Abstractions/IMomoiroCatalog.cs`):

```csharp
public interface IMomoiroCatalog : IEraGameDataCatalog
{
    IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder { get; }
    uint SongHashVersion { get; }
    IReadOnlyList<ushort> SongHashTable { get; }
    IReadOnlyDictionary<uint, Ac15MusicInfoEntry> MomoiroMusicInfos { get; }
    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; }
}
```

**Apply to Momoiro if proven:**
Add Momoiro Dan entities and DbSets by copying Kimidori names/shapes. Extend `Ac15DaniMapper` with `ToAc15DaniScore(DanScoreDatumMomoiro)`, summary, create/apply score, create/apply stage methods. Add a Momoiro catalog Dan order surface only if local Momoiro catalog/binary evidence identifies the correct source; do not name it Taikojuku unless evidence proves that surface. Then flip `Ac15EraProfiles.Momoiro.Features.Dani` intentionally.

**Scope warning:**
MORUN-03 says Dan/Dani is allowed only where Momoiro playresult, userdata, and binary/client evidence prove the normal Dan contract. Do not infer Taikojuku practice-folder behavior from `play_dan`/`dan_result`.

### `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs` (test, request-response/CRUD)

**Analog:** `Tests/Murasaki/MurasakiRuntimeHandlerTests.cs`

**Normal mutation and no-cross-era pattern** (lines 149-228):

```csharp
public async Task UpdatePlayResult_Murasaki_SavesNormalPlayRewardAndOnlyMurasakiRows()
{
    await using var fixture = await MurasakiHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataMurasaki.Add(UserSaveDataMurasakiExtensions.CreateDefaultMurasakiSaveData(1));
    fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
    ...
    var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
        1,
        GameEra.Murasaki,
        playDatetime: "20260608120000",
        profile: profile,
        stages: [CreateStage(101, 1, 0)]),
        CancellationToken.None);

    Assert.Equal(1u, result);
    var play = Assert.Single(await fixture.Context.SongPlayDataMurasaki.Where(row => row.Baid == 1).ToListAsync());
    ...
    Assert.Empty(await fixture.Context.SongPlayDataBlue.Where(row => row.Baid == 1).ToListAsync());
    ...
    Assert.Empty(await fixture.Context.WhiteDonChallengeProgress.Where(row => row.Baid == 1).ToListAsync());
}
```

**Dani test pattern if proven** (lines 232-281):

```csharp
public async Task UpdatePlayResult_MurasakiCompatibility_DaniCreatesMurasakiDanRows()
{
    await using var fixture = await MurasakiHandlerFixture.CreateAsync(CreateDanCatalog(1));
    ...
    var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
        1,
        GameEra.Murasaki,
        playMode: (uint)PlayMode.DanMode,
        stages: danStages,
        dani: new Ac15DaniPlayResult(...)),
        CancellationToken.None);
    ...
    Assert.Empty(await fixture.Context.DanScoreDataBlue.Where(row => row.Baid == 1).ToListAsync());
}
```

**Challenge no-state pattern** (lines 285-315):

```csharp
var stage = CreateStage(101, 1, 0) with
{
    ChallengeIds = [new Ac15CompeIdFact(42, 1)],
    UserCompeIds = [new Ac15CompeIdFact(43, 2)],
    BngCompeIds = [new Ac15CompeIdFact(44, 3)]
};
...
Assert.Single(await fixture.Context.SongPlayDataMurasaki.Where(row => row.Baid == 1).ToListAsync());
Assert.Empty(await fixture.Context.RedDonChallengeRawFacts.Where(row => row.Baid == 1).ToListAsync());
Assert.Empty(await fixture.Context.WhiteDonChallengeProgress.Where(row => row.Baid == 1).ToListAsync());
```

**Apply to Momoiro:**
Create focused tests that prove MORUN-01..05:

- normal play writes `SongPlayDataMomoiro`, `SongBestDataMomoiro`, `MomoiroRecentSongs`, profile counters, and evidence-backed save fields
- release song flags mutate only `UserSaveDataMomoiro.ReleaseSongFlg`
- favorite writes preserve/derive `DisplayOrder` or are explicitly gated
- no writes to KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, Tokkun, battle, Don Challenge, ChallengeCompe, shop-season, wallet, or unsupported feature tables
- challenge arrays do not create challenge state unless Momoiro evidence proves exact semantics

**Existing Momoiro fixture to extend** (lines 55-153, 181-299):

```csharp
public async Task EnsureMomoiroReadbackTablesAsync()
{
    await Context.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS UserSaveData_Momoiro (
            ...
        );
        """);
    ...
    CREATE TABLE IF NOT EXISTS MomoiroFavoriteSongs (
        Baid INTEGER NOT NULL,
        SongNo INTEGER NOT NULL,
        DisplayOrder INTEGER NOT NULL,
        PRIMARY KEY (Baid, SongNo)
    );
}
...
public Task SeedMomoiroFavoriteAsync(uint baid, uint songNo, int displayOrder)
```

Extend the fixture with `SongPlayDatum_Momoiro` and optional Dan tables only after production schema exists or tests use raw SQL future-table helpers in the same style.

## Shared Patterns

### Direct Protobuf Controller Pattern

**Source:** `Adapters.GameProtocol.Kimidori/Controllers/PlayResultController.cs` lines 6-15

**Apply to:** Momoiro playresult controller

```csharp
var playResult = PlayResultMappers.Map(request);
var result = await Mediator.Send(
    new UpdateAc15PlayResultCommand(request.Baid, GameEra.Kimidori, playResult),
    HttpContext.RequestAborted);
return Ok(PlayResultMappers.Map(result));
```

### AC15 Normal Mutation Ordering

**Source:** `Application/Handlers/UpdatePlayResultCommand.Kimidori.cs` lines 30-88

**Apply to:** `UpdatePlayResultCommand.Momoiro.cs`

Order should stay:

1. BAID zero returns success.
2. Missing shared user logs and returns success.
3. Extract normal stages.
4. Filter valid normal stages with `Ac15NormalStageFilter.Filter`.
5. Get/create Momoiro save row.
6. Parse play time.
7. Apply evidence-backed profile/reward/unlock mutation.
8. Conditionally save Dani only if proven.
9. Save normal play rows.
10. Return `1`.

### Era-Owned Persistence

**Sources:**

- `Application/Handlers/UpdatePlayResultCommand.Kimidori.cs` lines 91-110
- `Application/Abstractions/ITaikoDbContext.Kimidori.cs` lines 5-11
- `Infrastructure/Persistence/TaikoDbContext.Kimidori.cs` lines 46-108

**Apply to:** all Momoiro playresult state

Use Momoiro-owned entities/DbSets. Shared AC15 helpers may share algorithms, but not gameplay tables.

### Release/Crown Byte Helpers

**Sources:**

- `Application/Ac15/Ac15ProtocolBytes.cs` lines 23-38 for bit setting
- `Application/Ac15/Ac15ProtocolBytes.cs` lines 102-139 for crown packing helpers
- `Application/Ac15/Ac15SongHashCodec.cs` lines 21-46 for Momoiro compact readback

```csharp
public static byte[] SetBits(byte[]? source, IEnumerable<uint> ids, int byteCount)
{
    var result = FixedOrZero(source, byteCount);
    var maxBits = byteCount * 8;
    foreach (var id in ids)
    {
        if (id >= maxBits)
        {
            continue;
        }

        result[id >> 3] |= (byte)(1 << ((int)id & 7));
    }

    return result;
}
```

Use `Ac15UnlockFlagAccess.Momoiro` for release/title/tone/costume flag mutation and preserve `UserDataController` compact readback (lines 24-27).

### Momoiro Readback Contract

**Sources:**

- `Application/Handlers/UserDataQuery.Momoiro.cs` lines 18-29 for favorite/recent order
- `Application/Handlers/UserDataQuery.Momoiro.cs` lines 31-58 for best/crown/reward readback
- `Application/Handlers/BaidQuery.Momoiro.cs` lines 67-83 for BAID Dan/reward readback
- `Tests/Momoiro/MomoiroReadbackHandlerTests.cs` lines 130-183 for favorite/recent/reward assertions

```csharp
var favorites = await context.MomoiroFavoriteSongs
    .Where(song => song.Baid == request.Baid)
    .OrderBy(song => song.DisplayOrder)
    .ThenBy(song => song.SongNo)
    .Select(song => song.SongNo)
    .Take(limits.MaxFavoriteSongs)
    .ToArrayAsync(cancellationToken);
```

Phase 42 writes must make Phase 41 readback true. Do not write data that the existing Momoiro readback ignores unless it is a new Phase 42 requirement with readback evidence.

## Forbidden Patterns

### Do Not Copy Red/White Don Challenge Mutation

**Forbidden source:** `Application/Handlers/UpdatePlayResultCommand.Red.cs` lines 87-93 and `White.cs` lines 87-93

```csharp
await Ac15DonChallengeWriter.SaveAsync(
    RedDonChallengeTables(),
    new Ac15DonChallengeWriteRequest(request.Baid, red.DonChallenge, validStages, playTime),
    new Ac15DonChallengeRewardMutators(
        ids => Ac15UnlockFlagAccess.Red.ReleaseSongs?.Invoke(saveData, ids),
        ids => Ac15UnlockFlagAccess.Red.Titles(saveData, ids)),
    cancellationToken);
```

**Why forbidden:** MORUN-04 says challenge-shaped arrays do not create Don Challenge, ChallengeCompe, or reward-management behavior by assumption. Momoiro has no Don Challenge scope in Phase 42.

### Do Not Copy Yellow Shop-Season/Item-Shop Flow

**Forbidden source:** `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` lines 48-66

```csharp
var shopSeasonState = await context.GetOrCreateActiveYellowShopSeasonStateAsync(
    saveData,
    yellow.ItemShopCatalog,
    cancellationToken);
...
if (!Ac15CommonProfileMutation.TryApply(
        saveData,
        shopSeasonState,
        playResultData.Profile,
```

**Why forbidden:** MORUN-02 explicitly excludes `shoppingresult.php`, item-shop authority, wallet, payment, shop-season, and purchase semantics. Use `TryApplyDonPoints`, not `TryApply` with a shop season.

### Do Not Copy Tokkun/Battle Branches

**Forbidden sources:**

- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` lines 29-32
- `Application/Handlers/UpdatePlayResultCommand.Red.cs` lines 30-33 and 104-121
- Blue battle and Tokkun branches

**Why forbidden:** Tokkun, battle, Banacoin, and related side effects are out of Phase 42 scope. Momoiro playresult classification should be normal/Dani only if evidence proves Dani.

### Do Not Treat `Ac15ItemShopUnlockPolicies` as Momoiro Runtime Authority

**Forbidden source:** `Application/Ac15/Ac15ItemShopUnlockPolicies.cs` lines 5-12 and 21-56

The policy exists for Blue/Green/Yellow item-shop purchase flows. Phase 42 can use `Ac15UnlockFlagAccess.Momoiro` for evidence-backed playresult unlock flags, but must not add a Momoiro item-shop policy or purchase flow.

### Do Not Edit `proto/` or Generated `Wire/` Files

`Adapters.GameProtocol.Momoiro/Wire/Game.cs` is evidence for fields, not a manual edit target. The Phase 42 context explicitly excludes proto edits.

### Do Not Sort Momoiro Favorites by Raw Song Number

**Source of warning:** `MomoiroFavoriteSongs.DisplayOrder` lines 3-8 and `UserDataQuery.Momoiro` lines 18-24.

Phase 41 intentionally chose persisted `DisplayOrder` with `SongNo` only as a tie-breaker. If playresult favorite mutation is implemented, it must preserve or derive display order. The generic writer's current add path does not set `DisplayOrder`.

## Momoiro-Specific Add Locations

| Add/Modify | Location | Notes |
|------------|----------|-------|
| Add | `Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs` | Copy Kimidori/Murasaki playresult mapper; use Momoiro namespace and wire types. |
| Modify | `Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs` | Replace scaffold success with mapper + `UpdateAc15PlayResultCommand`. |
| Modify | `Application/Handlers/UpdatePlayResultCommand.cs` | Add Momoiro AC15 dispatch arm and partial declaration. |
| Add | `Application/Handlers/UpdatePlayResultCommand.Momoiro.cs` | Copy Kimidori flow; keep Red/White/Yellow special features out. |
| Add | `Domain/Entities/SongPlayDatumMomoiro.cs` | Copy `SongPlayDatumKimidori`; table `SongPlayDatum_Momoiro`. |
| Modify | `Application/Ac15/Ac15NormalPlayMapper.cs` | Add Momoiro play/best mapper methods. |
| Modify | `Application/Ac15/Ac15UnlockFlagAccess.cs` | Add Momoiro flag accessor if unlock mutation is evidence-backed. |
| Modify | `Application/Ac15/Ac15ProfileCounterUpdater.cs` | Add Momoiro counter accessor. |
| Modify | `Application/Abstractions/ITaikoDbContext.Momoiro.cs` | Add `SongPlayDataMomoiro`; add Dan DbSets only if proven. |
| Modify | `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs` | Add `SongPlayDatum_Momoiro` config; add Dan config only if proven. |
| Add | `Infrastructure/Persistence/Migrations/<timestamp>_AddMomoiroPlayResultState.cs` | Migration should add only Momoiro-owned playresult/Dan tables required by Phase 42. |
| Conditional add | `Domain/Entities/DanScoreDatumMomoiro.cs` | Only if MORUN-03 evidence passes. |
| Conditional add | `Domain/Entities/DanStageScoreDatumMomoiro.cs` | Only if MORUN-03 evidence passes. |
| Conditional modify | `Application/Ac15/Ac15DaniMapper.cs` | Add Momoiro mappings only with Dan entities. |
| Conditional modify | `Application/Ac15/Ac15EraProfiles.cs` | Flip `Dani` only after evidence and readback support exist. |
| Conditional modify | `Application/Abstractions/IMomoiroCatalog.cs` | Add a Momoiro Dan order surface only after local catalog/binary evidence. |
| Conditional modify | `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs` | Populate Dan order surface only if proven. |
| Add | `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs` | Use Murasaki runtime test structure with Momoiro-owned assertions. |
| Modify | `Tests/Momoiro/MomoiroHandlerFixture.cs` | Add play/Dan table setup and seed/count helpers as needed. |
| Add | `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` | Verify controller maps request and reaches handler path; avoid route inventory/source-text tests. |

## No Analog Found

No file is fully without an analog, but these areas are evidence-gated rather than analog-gated:

| File/Area | Role | Data Flow | Reason |
|-----------|------|-----------|--------|
| Momoiro favorite playresult ordering | utility/handler | CRUD mutation | Adjacent favorite tables lack `DisplayOrder`; Phase 41 Momoiro readback added a stricter order contract. |
| Momoiro Dan catalog source | catalog service | file-I/O/read-only lookup | `IMomoiroCatalog` currently has no Dani/Taikojuku order surface; adjacent `DaniFileOrder`/`TaikojukuFileOrder` names are not proof. |
| Momoiro challenge-shaped semantics | handler/service | event fact or no-op | Adjacent Red/White Don Challenge behavior is explicitly out of scope unless Momoiro evidence proves it. |

## Metadata

**Analog search scope:** `Application/`, `Domain/`, `Infrastructure/`, `Adapters.GameProtocol.Momoiro/`, adjacent AC15 adapters, `Tests/Momoiro/`, `Tests/Murasaki/`
**Files scanned:** adjacent playresult handlers, AC15 shared writers/mappers/helpers, Momoiro readback handlers/controllers/entities/fixture, generated Momoiro wire targeted ranges, Murasaki runtime tests
**Pattern extraction date:** 2026-06-26
