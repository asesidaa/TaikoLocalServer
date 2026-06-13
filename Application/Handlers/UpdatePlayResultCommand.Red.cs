using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint RedDanCostumeId = 36;

    private partial async ValueTask<uint> HandleRed(
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
            logger.LogWarning("Game uploading a non existing Red user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = Ac15PlayResultCommonBridge.ToCommon(request.PlayResultData);
        if (IsRedTokkunShaped(playResultData))
        {
            return await HandleRedTokkun(request.Baid, playResultData, cancellationToken);
        }

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            playResultData.AryStageInfoes,
            Ac15EraProfiles.Red.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Red playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        playResultData.AryStageInfoes = validStages.ToList();

        var saveData = await context.GetOrCreateRedSaveDataAsync(request.Baid, cancellationToken);
        var red = gameDataService.Red();
        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApplyDonPoints(
                saveData,
                playResultData,
                validStages,
                Ac15ProfileCounterUpdater.Red,
                Ac15UnlockFlagAccess.Red,
                Ac15EraProfiles.Red.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Red Don point totals for baid {Baid}", request.Baid);
            return 1;
        }

        await Ac15DaniWriter.SaveAsync(
            RedDaniTables(),
            playResultData,
            Ac15EraProfiles.Red.Limits,
            red.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
            new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, RedDanCostumeId),
            update =>
            {
                saveData.GotDanFlg = update.GotDanFlg;
                saveData.GotDanExtraFlg = update.GotDanExtraFlg;
                saveData.GotDanMax = update.GotDanMax;
                saveData.DispTaikojukuDan = update.DisplayDan;
                if (update.ApplyDanCostume)
                {
                    Ac15CustomizationMutation.ApplyDanCostume(saveData, update.DanCostumeId, Ac15EraProfiles.Red.Limits);
                }
            },
            logger,
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            RedNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.PlayMode, validStages, Ac15EraProfiles.Red.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private async ValueTask<uint> HandleRedTokkun(
        uint baid,
        CommonPlayResultData playResultData,
        CancellationToken cancellationToken)
    {
        var saveData = await context.GetOrCreateRedSaveDataAsync(baid, cancellationToken);
        if (playResultData.TokkunTutorialFlg is { } tokkunTutorialFlg)
        {
            saveData.TokkunTutorialFlg = tokkunTutorialFlg;
        }

        await context.SaveChangesAsync(cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumRed, SongBestDatumRed, RedFavoriteSongs, RedRecentSongs> RedNormalPlayTables()
        => new(
            context.SongPlayDataRed,
            context.SongBestDataRed,
            context.RedFavoriteSongs,
            context.RedRecentSongs,
            Ac15NormalPlayMapper.ToRedSongPlayDatum,
            Ac15NormalPlayMapper.ToRedSongBestDatum);

    private Ac15DaniTables<DanScoreDatumRed, DanStageScoreDatumRed> RedDaniTables()
        => new(
            context.DanScoreDataRed,
            context.DanScoreDataRed.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToRedDanScoreDatum,
            Ac15DaniMapper.ApplyToRedDanScoreDatum,
            Ac15DaniMapper.ToRedDanStageScoreDatum,
            Ac15DaniMapper.ApplyToRedDanStageScoreDatum);

    private static bool IsRedTokkunShaped(CommonPlayResultData playResultData)
        => playResultData.IsTokkunPlayResult
           || playResultData.PlayMode == (uint)PlayMode.Tokkun
           || playResultData.TokkunTutorialFlg is not null
           || playResultData.TokkunStageData is not null;
}
