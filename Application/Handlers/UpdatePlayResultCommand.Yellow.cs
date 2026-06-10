using System.Globalization;
using TaikoLocalServer.Application.Ac15;

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
            return await HandleYellowTokkun(request.Baid, playResultData, cancellationToken);
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
        saveData.WaiwaiTutorialFlg = playResultData.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;
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
            Ac15ProfileCounterUpdater.ApplyYellowStage(saveData, stage);
        }

        await Ac15DaniService.SaveAsync(
            context,
            playResultData,
            Ac15EraProfiles.Yellow,
            yellow.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
            new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, YellowDanCostumeId),
            update =>
            {
                saveData.GotDanFlg = update.GotDanFlg;
                saveData.GotDanExtraFlg = update.GotDanExtraFlg;
                saveData.GotDanMax = update.GotDanMax;
                saveData.DispTaikojukuDan = update.DisplayDan;
                if (update.ApplyDanCostume)
                {
                    saveData.Costume1 = update.DanCostumeId;
                    saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(
                        saveData.CostumeFlg1,
                        [update.DanCostumeId],
                        Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes);
                }
            },
            logger,
            cancellationToken);
        LogYellowWaiWaiStageFacts(request.Baid, playResultData);

        return await Ac15NormalPlayService.SaveAsync(
            context,
            request.Baid,
            playResultData,
            Ac15EraProfiles.Yellow,
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
