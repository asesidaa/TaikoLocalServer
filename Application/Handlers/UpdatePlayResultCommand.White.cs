using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint WhiteDanCostumeId = 36;

    private partial async ValueTask<uint> HandleWhite(
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
            logger.LogWarning("Game uploading a non existing White user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var normal = playResultData.Normal;
        IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];
        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            stages,
            Ac15EraProfiles.White.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping White playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        var saveData = await context.GetOrCreateWhiteSaveDataAsync(request.Baid, cancellationToken);
        var white = gameDataService.White();
        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApplyDonPoints(
                saveData,
                playResultData.Profile,
                validStages,
                Ac15ProfileCounterUpdater.White,
                Ac15UnlockFlagAccess.White,
                Ac15EraProfiles.White.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid White Don point totals for baid {Baid}", request.Baid);
            return 1;
        }

        var dani = playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode && playResultData.Dani is { } inputDani
            ? inputDani with { Stages = validStages.ToList() }
            : null;

        await Ac15DaniWriter.SaveAsync(
            WhiteDaniTables(),
            dani,
            Ac15EraProfiles.White.Limits,
            white.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
            new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, WhiteDanCostumeId),
            update =>
            {
                saveData.GotDanFlg = update.GotDanFlg;
                saveData.GotDanExtraFlg = update.GotDanExtraFlg;
                saveData.GotDanMax = update.GotDanMax;
                saveData.DispTaikojukuDan = update.DisplayDan;
                if (update.ApplyDanCostume)
                {
                    Ac15CustomizationMutation.ApplyDanCostume(saveData, update.DanCostumeId, Ac15EraProfiles.White.Limits);
                }
            },
            logger,
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            WhiteNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.White.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumWhite, SongBestDatumWhite, WhiteFavoriteSongs, WhiteRecentSongs> WhiteNormalPlayTables()
        => new(
            context.SongPlayDataWhite,
            context.SongBestDataWhite,
            context.WhiteFavoriteSongs,
            context.WhiteRecentSongs,
            Ac15NormalPlayMapper.ToWhiteSongPlayDatum,
            Ac15NormalPlayMapper.ToWhiteSongBestDatum);

    private Ac15DaniTables<DanScoreDatumWhite, DanStageScoreDatumWhite> WhiteDaniTables()
        => new(
            context.DanScoreDataWhite,
            context.DanScoreDataWhite.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToWhiteDanScoreDatum,
            Ac15DaniMapper.ApplyToWhiteDanScoreDatum,
            Ac15DaniMapper.ToWhiteDanStageScoreDatum,
            Ac15DaniMapper.ApplyToWhiteDanStageScoreDatum);
}
