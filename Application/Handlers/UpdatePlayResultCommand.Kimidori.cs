using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint KimidoriDanCostumeId = 36;

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
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Kimidori playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

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
                {
                    Ac15CustomizationMutation.ApplyDanCostume(saveData, update.DanCostumeId, Ac15EraProfiles.Kimidori.Limits);
                }
            },
            logger,
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            KimidoriNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Kimidori.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumKimidori, SongBestDatumKimidori, KimidoriFavoriteSongs, KimidoriRecentSongs> KimidoriNormalPlayTables()
        => new(
            context.SongPlayDataKimidori,
            context.SongBestDataKimidori,
            context.KimidoriFavoriteSongs,
            context.KimidoriRecentSongs,
            Ac15NormalPlayMapper.ToKimidoriSongPlayDatum,
            Ac15NormalPlayMapper.ToKimidoriSongBestDatum);

    private Ac15DaniTables<DanScoreDatumKimidori, DanStageScoreDatumKimidori> KimidoriDaniTables()
        => new(
            context.DanScoreDataKimidori,
            context.DanScoreDataKimidori.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToKimidoriDanScoreDatum,
            Ac15DaniMapper.ApplyToKimidoriDanScoreDatum,
            Ac15DaniMapper.ToKimidoriDanStageScoreDatum,
            Ac15DaniMapper.ApplyToKimidoriDanStageScoreDatum);
}
