using System.Globalization;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint MinBlueCourseLevel = 1;
    private const uint MaxBlueCourseLevel = 5;
    private const uint BlueDanCostumeId = 36;

    private partial async ValueTask<uint> HandleBlue(
        UpdatePlayResultCommand request,
        CancellationToken cancellationToken)
    {
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
        if (playResultData.IsTokkunPlayResult)
        {
            return await HandleBlueTokkun(request.Baid, playResultData, cancellationToken);
        }

        if (playResultData.IsBattlePlayResult)
        {
            return await HandleBlueBattle(request.Baid, playResultData, cancellationToken);
        }

        var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
        var blue = gameDataService.Blue();
        var shopSeasonState = await context.GetOrCreateActiveBlueShopSeasonStateAsync(
            saveData,
            blue.ItemShopCatalog,
            cancellationToken);
        var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;

        if (!CanAddBlue(currentDonmedal, playResultData.GetDonmedal)
            || !CanAddBlue(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal))
        {
            logger.LogWarning("Rejecting invalid Blue medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        var playTime = ParseBluePlayDatetimeOrNow(playResultData.PlayDatetime);

        if (shopSeasonState is null)
        {
            saveData.TotalGetDonmedal += playResultData.GetDonmedal;
        }
        else
        {
            shopSeasonState.TotalGetDonmedal += playResultData.GetDonmedal;
            shopSeasonState.UpdatedAt = DateTime.UtcNow;
        }

        saveData.TotalGetKatsumedal += playResultData.GetKatsumedal;
        saveData.ItemshopTutorialFlg = playResultData.ItemshopTutorialFlg ?? saveData.ItemshopTutorialFlg;
        saveData.IsDevil = playResultData.IsDevil ?? saveData.IsDevil;
        saveData.IsExplain = playResultData.IsExplain ?? saveData.IsExplain;
        saveData.WaiwaiTutorialFlg = playResultData.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;
        if (playResultData.HasDifficultyPlayedCourse)
        {
            saveData.DifficultyPlayedCourse = playResultData.DifficultyPlayedCourse;
        }

        if (playResultData.HasDifficultyPlayedStar)
        {
            saveData.DifficultyPlayedStar = playResultData.DifficultyPlayedStar;
        }

        saveData.LastPlayDatetime = playTime;
        saveData.PrevAreaCode = playResultData.AreaCode;

        if (playResultData.HasAryCurrentCostume && saveData.IsAutoCostumeOn)
        {
            ApplyCostume(saveData, playResultData.AryCurrentCostume);
        }

        ApplyUnlockBits(saveData, playResultData);

        foreach (var stage in playResultData.AryStageInfoes)
        {
            if (!IsSupportedBlueStage(request.Baid, stage))
            {
                continue;
            }

            BlueProfileCounters.ApplyStage(saveData, stage);
        }

        await SaveBlueDanAsync(saveData, playResultData, blue, cancellationToken);

        return await Ac15NormalPlayService.SaveAsync(
            request.Baid,
            playResultData,
            Ac15EraProfiles.Blue,
            new BlueAc15NormalPlayAdapter(context),
            DefaultAc15EraHooks.Instance,
            cancellationToken);
    }

    private bool IsSupportedBlueStage(uint baid, CommonPlayResultData.StageData stage)
    {
        if (!BluePlayResultMapping.IsSupportedNormalStageMode(stage.StageMode))
        {
            logger.LogWarning(
                "Skipping unsupported Blue stage mode for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.StageMode);
            return false;
        }

        if (stage.SongNo >= BlueProtocolBytes.SongFlagBytes * 8 || stage.Level is < MinBlueCourseLevel or > MaxBlueCourseLevel)
        {
            logger.LogWarning(
                "Skipping invalid Blue stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.StageMode);
            return false;
        }

        return true;
    }

    private static void ApplyCostume(UserSaveDataBlue saveData, CommonPlayResultData.CostumeData costume)
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
        saveData.CostumeFlg1 = SetBlueBits(saveData.CostumeFlg1, [costume.Costume1], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = SetBlueBits(saveData.CostumeFlg2, [costume.Costume2], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = SetBlueBits(saveData.CostumeFlg3, [costume.Costume3], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = SetBlueBits(saveData.CostumeFlg4, [costume.Costume4], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = SetBlueBits(saveData.CostumeFlg5, [costume.Costume5], BlueProtocolBytes.CostumeFlagBytes);
    }

    private static void ApplyUnlockBits(UserSaveDataBlue saveData, CommonPlayResultData playResultData)
    {
        saveData.ReleaseSongFlg = SetBlueBits(saveData.ReleaseSongFlg, playResultData.ReleaseSongNoes, BlueProtocolBytes.SongFlagBytes);
        saveData.ToneFlg = SetBlueBits(saveData.ToneFlg, playResultData.GetToneNoes, BlueProtocolBytes.ToneFlagBytes);
        saveData.CostumeFlg1 = SetBlueBits(saveData.CostumeFlg1, playResultData.GetCostumeNo1s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = SetBlueBits(saveData.CostumeFlg2, playResultData.GetCostumeNo2s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = SetBlueBits(saveData.CostumeFlg3, playResultData.GetCostumeNo3s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = SetBlueBits(saveData.CostumeFlg4, playResultData.GetCostumeNo4s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = SetBlueBits(saveData.CostumeFlg5, playResultData.GetCostumeNo5s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.TitleFlg = SetBlueBits(saveData.TitleFlg, playResultData.GetTitleNoes, BlueProtocolBytes.TitleFlagBytes);
    }

    private async Task SaveBlueDanAsync(
        UserSaveDataBlue saveData,
        CommonPlayResultData playResultData,
        IBlueCatalog blue,
        CancellationToken cancellationToken)
    {
        if (playResultData.PlayMode != (uint)PlayMode.DanMode)
        {
            return;
        }

        var danIds = playResultData.AryStageInfoes
            .Select(stage => stage.PlayDan.GetValueOrDefault())
            .Where(dan => dan != 0)
            .Distinct()
            .ToArray();

        if (danIds.Length != 1)
        {
            logger.LogWarning("Skipping Blue Dani save for baid {Baid}: expected one PlayDan value, got {Count}", saveData.Baid, danIds.Length);
            return;
        }

        if (playResultData.DanResult > (uint)BlueDanClearGrade.GoldClear)
        {
            logger.LogWarning("Skipping Blue Dani save for baid {Baid}: invalid DanResult {DanResult}", saveData.Baid, playResultData.DanResult);
            return;
        }

        var danId = danIds[0];
        var pack = blue.TaikojukuFileOrder.FirstOrDefault(row => row.ChallengeLevel == danId);
        if (pack is null || !BlueDanHelpers.IsKnownBlueDanId(danId))
        {
            logger.LogWarning("Skipping Blue Dani save for baid {Baid}: unknown Dan id {DanId}", saveData.Baid, danId);
            return;
        }

        var isExtra = BlueDanHelpers.IsExtraDanId(danId);
        var danScore = await context.DanScoreDataBlue
            .Include(row => row.DanStageScoreData)
            .SingleOrDefaultAsync(row => row.Baid == saveData.Baid && row.DanId == danId && row.IsExtra == isExtra, cancellationToken);

        if (danScore is null)
        {
            danScore = new DanScoreDatumBlue
            {
                Baid = saveData.Baid,
                DanId = danId,
                IsExtra = isExtra,
                MedleyUniqueId = pack.UniqueId
            };
            context.DanScoreDataBlue.Add(danScore);
        }

        var incomingClearGrade = BlueDanHelpers.ClampGrade(playResultData.DanResult);
        UpdateBlueDanScore(danScore, playResultData);
        await UpdateBlueDanSummaryAsync(saveData, danScore, incomingClearGrade, cancellationToken);
    }

    private static void UpdateBlueDanScore(DanScoreDatumBlue danScore, CommonPlayResultData playResultData)
    {
        danScore.ClearGrade = BlueDanHelpers.ClampGrade(Math.Max((uint)danScore.ClearGrade, playResultData.DanResult));
        danScore.ArrivalSongCount = Math.Max(danScore.ArrivalSongCount, (uint)playResultData.AryStageInfoes.Count);
        danScore.ComboCountTotal = Math.Max(danScore.ComboCountTotal, playResultData.ComboCntTotal);
        danScore.SoulGaugeTotal = Math.Max(
            danScore.SoulGaugeTotal,
            playResultData.AryStageInfoes.LastOrDefault()?.SoulGauge.GetValueOrDefault() ?? 0);

        for (var i = 0; i < playResultData.AryStageInfoes.Count; i++)
        {
            var stage = playResultData.AryStageInfoes[i];
            var stageIndex = (uint)i;
            var existing = danScore.DanStageScoreData.FirstOrDefault(row => row.StageIndex == stageIndex);
            if (existing is null)
            {
                existing = new DanStageScoreDatumBlue
                {
                    Baid = danScore.Baid,
                    DanId = danScore.DanId,
                    IsExtra = danScore.IsExtra,
                    StageIndex = stageIndex,
                    SongNumber = stage.SongNo,
                    BadCount = stage.NgCnt
                };
                danScore.DanStageScoreData.Add(existing);
            }

            existing.SongNumber = stage.SongNo;
            existing.PlayScore = Math.Max(existing.PlayScore, stage.PlayScore);
            existing.HighScore = Math.Max(existing.HighScore, stage.PlayScore);
            existing.ComboCount = Math.Max(existing.ComboCount, stage.ComboCnt);
            existing.DrumrollCount = Math.Max(existing.DrumrollCount, stage.PoundCnt);
            existing.GoodCount = Math.Max(existing.GoodCount, stage.GoodCnt);
            existing.OkCount = Math.Max(existing.OkCount, stage.OkCnt);
            existing.TotalHitCount = Math.Max(existing.TotalHitCount, stage.HitCnt);
            existing.BadCount = Math.Min(existing.BadCount, stage.NgCnt);
        }
    }

    private async ValueTask UpdateBlueDanSummaryAsync(
        UserSaveDataBlue saveData,
        DanScoreDatumBlue currentDanScore,
        BlueDanClearGrade incomingClearGrade,
        CancellationToken cancellationToken)
    {
        var rows = await context.DanScoreDataBlue
            .Where(row => row.Baid == saveData.Baid)
            .ToListAsync(cancellationToken);

        if (!rows.Any(row => row.DanId == currentDanScore.DanId && row.IsExtra == currentDanScore.IsExtra))
        {
            rows.Add(currentDanScore);
        }

        var normalGrades = rows
            .Where(row => !row.IsExtra && BlueDanHelpers.IsNormalDanId(row.DanId))
            .ToDictionary(row => row.DanId, row => row.ClearGrade);

        var normalFlags = new byte[BlueProtocolBytes.DanFlagBytes];
        foreach (var row in rows.Where(row => !row.IsExtra && BlueDanHelpers.IsNormalDanId(row.DanId)))
        {
            normalFlags = BlueDanHelpers.SetPackedGrade(
                normalFlags,
                BlueDanHelpers.GetPackedIndex(row.DanId),
                row.ClearGrade);
        }

        var extraFlags = new byte[BlueProtocolBytes.DanExtraFlagBytes];
        foreach (var row in rows.Where(row => row.IsExtra && BlueDanHelpers.IsExtraDanId(row.DanId)))
        {
            extraFlags = BlueDanHelpers.SetPackedGrade(
                extraFlags,
                BlueDanHelpers.GetPackedIndex(row.DanId),
                row.ClearGrade);
        }

        var isIncomingClear = BlueDanHelpers.IsClear(incomingClearGrade);

        saveData.GotDanFlg = normalFlags;
        saveData.GotDanExtraFlg = extraFlags;
        saveData.GotDanMax = BlueDanHelpers.GetGotDanMax(normalGrades);
        if (isIncomingClear && saveData.IsAutoCostumeOn)
        {
            saveData.Costume1 = BlueDanCostumeId;
            saveData.CostumeFlg1 = SetBlueBits(saveData.CostumeFlg1, [BlueDanCostumeId], BlueProtocolBytes.CostumeFlagBytes);
        }

        saveData.DispTaikojukuDan = !currentDanScore.IsExtra
                                    && BlueDanHelpers.IsNormalDanId(currentDanScore.DanId)
                                    && isIncomingClear
            ? BlueDanHelpers.GetDisplayDanAfterNormalClear(currentDanScore.DanId)
            : BlueDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalGrades);
    }

    private static bool CanAddBlue(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static DateTime ParseBluePlayDatetimeOrNow(string playDatetime)
    {
        var formats = new[] { Constants.DateTimeFormat, "yyyy-MM-dd HH:mm:ss" };
        return DateTime.TryParseExact(
            playDatetime,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : DateTime.Now;
    }

    private static byte[] SetBlueBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = BlueProtocolBytes.FixedOrZero(source, byteCount);
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
}
