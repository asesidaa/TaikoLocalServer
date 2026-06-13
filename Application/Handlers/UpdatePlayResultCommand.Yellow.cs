using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint YellowDanCostumeId = 36;

    private partial async ValueTask<uint> HandleYellow(
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
            logger.LogWarning("Game uploading a non existing Yellow user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var normal = playResultData.Normal;
        IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];
        if (IsYellowTokkunShaped(playResultData))
        {
            return await HandleYellowTokkun(request.Baid, Ac15PlayResultCommonBridge.ToCommon(playResultData), cancellationToken);
        }

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            stages,
            Ac15EraProfiles.Yellow.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Yellow playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        var saveData = await context.GetOrCreateYellowSaveDataAsync(request.Baid, cancellationToken);
        var yellow = gameDataService.Yellow();
        var shopSeasonState = await context.GetOrCreateActiveYellowShopSeasonStateAsync(
            saveData,
            yellow.ItemShopCatalog,
            cancellationToken);

        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApply(
                saveData,
                shopSeasonState,
                playResultData.Profile,
                validStages,
                Ac15ProfileCounterUpdater.Yellow,
                Ac15UnlockFlagAccess.Yellow,
                Ac15EraProfiles.Yellow.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Yellow medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        var dani = playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode && playResultData.Dani is { } inputDani
            ? inputDani with { Stages = validStages.ToList() }
            : null;

        await Ac15DaniWriter.SaveAsync(
            YellowDaniTables(),
            dani,
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
        LogYellowWaiWaiStageFacts(request.Baid, validStages);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            YellowNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Yellow.Limits, playTime),
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

    private static bool IsYellowTokkunShaped(Ac15PlayResultEnvelope playResultData)
        => playResultData.Metadata.PlayMode == (uint)PlayMode.Tokkun
           || playResultData.Tokkun is not null;

    private void LogYellowWaiWaiStageFacts(uint baid, IEnumerable<Ac15StageResult> stages)
    {
        foreach (var stage in stages.Where(stage => stage.WaiwaiResult.HasValue || stage.WaiwaiGauge.HasValue))
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
