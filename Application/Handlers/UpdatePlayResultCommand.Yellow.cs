using System.Globalization;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint YellowDanCostumeId = 36;

    private partial async ValueTask<uint> HandleYellow(
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
            logger.LogWarning("Game uploading a non existing Yellow user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        if (IsYellowTokkunShaped(playResultData))
        {
            return 1;
        }

        var validStages = playResultData.AryStageInfoes
            .Where(stage => IsSupportedYellowNormalStage(request.Baid, stage))
            .ToList();
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Yellow playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        playResultData.AryStageInfoes = validStages;

        var saveData = await context.GetOrCreateYellowSaveDataAsync(request.Baid, cancellationToken);
        var yellow = gameDataService.Yellow();
        var shopSeasonState = await context.GetOrCreateActiveYellowShopSeasonStateAsync(
            saveData,
            yellow.ItemShopCatalog,
            cancellationToken);
        var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;

        if (!CanAddYellow(currentDonmedal, playResultData.GetDonmedal)
            || !CanAddYellow(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal))
        {
            logger.LogWarning("Rejecting invalid Yellow medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        var playTime = ParseYellowPlayDatetimeOrNow(playResultData.PlayDatetime);

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
            ApplyYellowCostume(saveData, playResultData.AryCurrentCostume);
        }

        ApplyYellowUnlockBits(saveData, playResultData);

        foreach (var stage in playResultData.AryStageInfoes)
        {
            ApplyYellowProfileStage(saveData, stage);
        }

        await SaveYellowDanAsync(saveData, playResultData, yellow, cancellationToken);
        LogYellowWaiWaiStageFacts(request.Baid, playResultData);

        return await Ac15NormalPlayService.SaveAsync(
            request.Baid,
            playResultData,
            Ac15EraProfiles.Yellow,
            new YellowAc15NormalPlayAdapter(context),
            DefaultAc15EraHooks.Instance,
            cancellationToken);
    }

    private static bool IsYellowTokkunShaped(CommonPlayResultData playResultData)
        => playResultData.IsTokkunPlayResult
           || playResultData.PlayMode == (uint)PlayMode.Tokkun
           || playResultData.TokkunStageData is not null;

    private bool IsSupportedYellowNormalStage(uint baid, CommonPlayResultData.StageData stage)
    {
        var limits = Ac15EraProfiles.Yellow.Limits;
        if (stage.SongNo >= limits.SongFlagBytes * 8
            || stage.Level < limits.MinCourseLevel
            || stage.Level > limits.MaxCourseLevel)
        {
            logger.LogWarning(
                "Skipping invalid Yellow stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.StageMode);
            return false;
        }

        var hookDecision = DefaultAc15EraHooks.Instance.IsSupportedStage(stage);
        if (!hookDecision.IsSupported)
        {
            logger.LogWarning(
                "Skipping unsupported Yellow stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.StageMode);
            return false;
        }

        return true;
    }

    private void LogYellowWaiWaiStageFacts(uint baid, CommonPlayResultData playResultData)
    {
        foreach (var stage in playResultData.AryStageInfoes.Where(stage => stage.WaiwaiResult.HasValue || stage.WaiwaiGauge.HasValue))
        {
            logger.LogInformation(
                "Yellow WaiWai stage fact for baid {Baid}: song={SongNo} level={Level} result={WaiwaiResult} gauge={WaiwaiGauge}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.WaiwaiResult,
                stage.WaiwaiGauge);
        }
    }

    private async Task SaveYellowDanAsync(
        UserSaveDataYellow saveData,
        CommonPlayResultData playResultData,
        IYellowCatalog yellow,
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
            logger.LogWarning("Skipping Yellow Dani save for baid {Baid}: expected one PlayDan value, got {Count}", saveData.Baid, danIds.Length);
            return;
        }

        if (playResultData.DanResult > (uint)YellowDanClearGrade.GoldClear)
        {
            logger.LogWarning("Skipping Yellow Dani save for baid {Baid}: invalid DanResult {DanResult}", saveData.Baid, playResultData.DanResult);
            return;
        }

        var danId = danIds[0];
        var pack = yellow.TaikojukuFileOrder.FirstOrDefault(row => row.ChallengeLevel == danId);
        if (pack is null || !YellowDanHelpers.IsKnownYellowDanId(danId))
        {
            logger.LogWarning("Skipping Yellow Dani save for baid {Baid}: unknown Dan id {DanId}", saveData.Baid, danId);
            return;
        }

        var isExtra = YellowDanHelpers.IsExtraDanId(danId);
        var danScore = await context.DanScoreDataYellow
            .Include(row => row.DanStageScoreData)
            .SingleOrDefaultAsync(row => row.Baid == saveData.Baid && row.DanId == danId && row.IsExtra == isExtra, cancellationToken);

        if (danScore is null)
        {
            danScore = new DanScoreDatumYellow
            {
                Baid = saveData.Baid,
                DanId = danId,
                IsExtra = isExtra,
                MedleyUniqueId = pack.UniqueId
            };
            context.DanScoreDataYellow.Add(danScore);
        }

        var incomingClearGrade = YellowDanHelpers.ClampGrade(playResultData.DanResult);
        UpdateYellowDanScore(danScore, playResultData);
        await UpdateYellowDanSummaryAsync(saveData, danScore, incomingClearGrade, cancellationToken);
    }

    private static void UpdateYellowDanScore(DanScoreDatumYellow danScore, CommonPlayResultData playResultData)
    {
        danScore.ClearGrade = YellowDanHelpers.ClampGrade(Math.Max((uint)danScore.ClearGrade, playResultData.DanResult));
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
                existing = new DanStageScoreDatumYellow
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

    private async ValueTask UpdateYellowDanSummaryAsync(
        UserSaveDataYellow saveData,
        DanScoreDatumYellow currentDanScore,
        YellowDanClearGrade incomingClearGrade,
        CancellationToken cancellationToken)
    {
        var rows = await context.DanScoreDataYellow
            .Where(row => row.Baid == saveData.Baid)
            .ToListAsync(cancellationToken);

        if (!rows.Any(row => row.DanId == currentDanScore.DanId && row.IsExtra == currentDanScore.IsExtra))
        {
            rows.Add(currentDanScore);
        }

        var normalGrades = rows
            .Where(row => !row.IsExtra && YellowDanHelpers.IsNormalDanId(row.DanId))
            .ToDictionary(row => row.DanId, row => row.ClearGrade);

        var limits = Ac15EraProfiles.Yellow.Limits;
        var normalFlags = new byte[limits.DanFlagBytes];
        foreach (var row in rows.Where(row => !row.IsExtra && YellowDanHelpers.IsNormalDanId(row.DanId)))
        {
            normalFlags = YellowDanHelpers.SetPackedGrade(
                normalFlags,
                YellowDanHelpers.GetPackedIndex(row.DanId),
                row.ClearGrade);
        }

        var extraFlags = new byte[limits.DanExtraFlagBytes];
        foreach (var row in rows.Where(row => row.IsExtra && YellowDanHelpers.IsExtraDanId(row.DanId)))
        {
            extraFlags = YellowDanHelpers.SetPackedGrade(
                extraFlags,
                YellowDanHelpers.GetPackedIndex(row.DanId),
                row.ClearGrade);
        }

        var isIncomingClear = YellowDanHelpers.IsClear(incomingClearGrade);

        saveData.GotDanFlg = normalFlags;
        saveData.GotDanExtraFlg = extraFlags;
        saveData.GotDanMax = YellowDanHelpers.GetGotDanMax(normalGrades);
        if (isIncomingClear && saveData.IsAutoCostumeOn)
        {
            saveData.Costume1 = YellowDanCostumeId;
            saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [YellowDanCostumeId], limits.CostumeFlagBytes);
        }

        saveData.DispTaikojukuDan = !currentDanScore.IsExtra
                                    && YellowDanHelpers.IsNormalDanId(currentDanScore.DanId)
                                    && isIncomingClear
            ? YellowDanHelpers.GetDisplayDanAfterNormalClear(currentDanScore.DanId)
            : YellowDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalGrades);
    }

    private static void ApplyYellowCostume(UserSaveDataYellow saveData, CommonPlayResultData.CostumeData costume)
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
        var limits = Ac15EraProfiles.Yellow.Limits;
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [costume.Costume1], limits.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [costume.Costume2], limits.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [costume.Costume3], limits.CostumeFlagBytes);
        saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [costume.Costume4], limits.CostumeFlagBytes);
        saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [costume.Costume5], limits.CostumeFlagBytes);
    }

    private static void ApplyYellowUnlockBits(UserSaveDataYellow saveData, CommonPlayResultData playResultData)
    {
        var limits = Ac15EraProfiles.Yellow.Limits;
        saveData.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(saveData.ReleaseSongFlg, playResultData.ReleaseSongNoes, limits.SongFlagBytes);
        saveData.ToneFlg = Ac15ProtocolBytes.SetBits(saveData.ToneFlg, playResultData.GetToneNoes, limits.ToneFlagBytes);
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, playResultData.GetCostumeNo1s, limits.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, playResultData.GetCostumeNo2s, limits.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, playResultData.GetCostumeNo3s, limits.CostumeFlagBytes);
        saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, playResultData.GetCostumeNo4s, limits.CostumeFlagBytes);
        saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, playResultData.GetCostumeNo5s, limits.CostumeFlagBytes);
        saveData.TitleFlg = Ac15ProtocolBytes.SetBits(saveData.TitleFlg, playResultData.GetTitleNoes, limits.TitleFlagBytes);
    }

    private static void ApplyYellowProfileStage(UserSaveDataYellow saveData, CommonPlayResultData.StageData stage)
    {
        IncrementYellowGenreCounter(saveData, stage.MusicCateg);
        if (stage.IsPushed) saveData.SongPushedCnt = SafeYellowIncrement(saveData.SongPushedCnt);
        if (stage.IsFavorite) saveData.SongFavoriteCnt = SafeYellowIncrement(saveData.SongFavoriteCnt);
        if (stage.IsRecent) saveData.SongRecentCnt = SafeYellowIncrement(saveData.SongRecentCnt);
    }

    private static void IncrementYellowGenreCounter(UserSaveDataYellow saveData, uint musicCateg)
    {
        switch (musicCateg)
        {
            case 1: saveData.CategJpopCnt = SafeYellowIncrement(saveData.CategJpopCnt); break;
            case 2: saveData.CategAnimeCnt = SafeYellowIncrement(saveData.CategAnimeCnt); break;
            case 3: saveData.CategVocaloidCnt = SafeYellowIncrement(saveData.CategVocaloidCnt); break;
            case 4: saveData.CategDoyoCnt = SafeYellowIncrement(saveData.CategDoyoCnt); break;
            case 5: saveData.CategVarietyCnt = SafeYellowIncrement(saveData.CategVarietyCnt); break;
            case 6: saveData.CategClassicCnt = SafeYellowIncrement(saveData.CategClassicCnt); break;
            case 7: saveData.CategGameCnt = SafeYellowIncrement(saveData.CategGameCnt); break;
            case 8: saveData.CategNamcoCnt = SafeYellowIncrement(saveData.CategNamcoCnt); break;
        }
    }

    private static uint SafeYellowIncrement(uint current)
        => current == uint.MaxValue ? current : current + 1;

    private static bool CanAddYellow(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static DateTime ParseYellowPlayDatetimeOrNow(string playDatetime)
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
}
