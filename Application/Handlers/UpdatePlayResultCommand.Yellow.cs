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

        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApply(
                saveData,
                shopSeasonState,
                playResultData,
                validStages,
                Ac15ProfileCounterUpdater.Yellow,
                Ac15UnlockFlagAccess.Yellow,
                Ac15EraProfiles.Yellow.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Yellow medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        await Ac15DaniWriter.SaveAsync(
            YellowDaniTables(),
            playResultData,
            Ac15EraProfiles.Yellow.Limits,
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
                    Ac15CustomizationMutation.ApplyDanCostume(saveData, update.DanCostumeId, Ac15EraProfiles.Yellow.Limits);
                }
            },
            logger,
            cancellationToken);
        LogYellowWaiWaiStageFacts(request.Baid, playResultData);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            YellowNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.PlayMode, validStages, Ac15EraProfiles.Yellow.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumYellow, SongBestDatumYellow, YellowFavoriteSongs, YellowRecentSongs> YellowNormalPlayTables()
        => new(
            context.SongPlayDataYellow,
            context.SongBestDataYellow,
            context.YellowFavoriteSongs,
            context.YellowRecentSongs,
            Ac15NormalPlayMapper.ToYellowSongPlayDatum,
            Ac15NormalPlayMapper.ToYellowSongBestDatum);

    private Ac15DaniTables<DanScoreDatumYellow, DanStageScoreDatumYellow> YellowDaniTables()
        => new(
            context.DanScoreDataYellow,
            context.DanScoreDataYellow.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToYellowDanScoreDatum,
            Ac15DaniMapper.ApplyToYellowDanScoreDatum,
            Ac15DaniMapper.ToYellowDanStageScoreDatum,
            Ac15DaniMapper.ApplyToYellowDanStageScoreDatum);

    private static bool IsYellowTokkunShaped(CommonPlayResultData playResultData)
        => playResultData.IsTokkunPlayResult
           || playResultData.PlayMode == (uint)PlayMode.Tokkun
           || playResultData.TokkunStageData is not null;

    private bool IsSupportedYellowNormalStage(uint baid, CommonPlayResultData.StageData stage)
    {
        var accepted = Ac15NormalStageFilter.Filter(
            baid,
            [stage],
            Ac15EraProfiles.Yellow.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        return accepted.Count == 1;
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

}
