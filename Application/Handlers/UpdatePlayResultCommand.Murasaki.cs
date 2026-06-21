using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint MurasakiDanCostumeId = 36;

    private partial async ValueTask<uint> HandleMurasaki(
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
            logger.LogWarning("Game uploading a non existing Murasaki user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var normal = playResultData.Normal;
        IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            stages,
            Ac15EraProfiles.Murasaki.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Murasaki playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        var saveData = await context.GetOrCreateMurasakiSaveDataAsync(request.Baid, cancellationToken);
        var murasaki = gameDataService.Murasaki();
        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApplyDonPoints(
                saveData,
                playResultData.Profile,
                validStages,
                Ac15ProfileCounterUpdater.Murasaki,
                Ac15UnlockFlagAccess.Murasaki,
                Ac15EraProfiles.Murasaki.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Murasaki Don point totals for baid {Baid}", request.Baid);
            return 1;
        }

        var dani = playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode && playResultData.Dani is { } inputDani
            ? inputDani with { Stages = validStages.ToList() }
            : null;

        await Ac15DaniWriter.SaveAsync(
            MurasakiDaniTables(),
            dani,
            Ac15EraProfiles.Murasaki.Limits,
            murasaki.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
            new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, MurasakiDanCostumeId),
            update =>
            {
                saveData.GotDanFlg = update.GotDanFlg;
                saveData.GotDanExtraFlg = update.GotDanExtraFlg;
                saveData.GotDanMax = update.GotDanMax;
                saveData.DispTaikojukuDan = update.DisplayDan;
                if (update.ApplyDanCostume)
                {
                    Ac15CustomizationMutation.ApplyDanCostume(saveData, update.DanCostumeId, Ac15EraProfiles.Murasaki.Limits);
                }
            },
            logger,
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            MurasakiNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Murasaki.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumMurasaki, SongBestDatumMurasaki, MurasakiFavoriteSongs, MurasakiRecentSongs> MurasakiNormalPlayTables()
        => new(
            context.SongPlayDataMurasaki,
            context.SongBestDataMurasaki,
            context.MurasakiFavoriteSongs,
            context.MurasakiRecentSongs,
            Ac15NormalPlayMapper.ToMurasakiSongPlayDatum,
            Ac15NormalPlayMapper.ToMurasakiSongBestDatum);

    private Ac15DaniTables<DanScoreDatumMurasaki, DanStageScoreDatumMurasaki> MurasakiDaniTables()
        => new(
            context.DanScoreDataMurasaki,
            context.DanScoreDataMurasaki.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToMurasakiDanScoreDatum,
            Ac15DaniMapper.ApplyToMurasakiDanScoreDatum,
            Ac15DaniMapper.ToMurasakiDanStageScoreDatum,
            Ac15DaniMapper.ApplyToMurasakiDanStageScoreDatum);
}
