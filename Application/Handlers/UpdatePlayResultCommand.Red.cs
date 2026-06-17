using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Ac15.DonChallenge;
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

        var playResultData = request.PlayResultData;
        var normal = playResultData.Normal;
        IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];
        if (Ac15TokkunPlayResultPolicy.IsTokkun(playResultData))
        {
            return await HandleRedTokkun(request.Baid, playResultData, cancellationToken);
        }

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            stages,
            Ac15EraProfiles.Red.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Red playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        var saveData = await context.GetOrCreateRedSaveDataAsync(request.Baid, cancellationToken);
        var red = gameDataService.Red();
        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApplyDonPoints(
                saveData,
                playResultData.Profile,
                validStages,
                Ac15ProfileCounterUpdater.Red,
                Ac15UnlockFlagAccess.Red,
                Ac15EraProfiles.Red.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Red Don point totals for baid {Baid}", request.Baid);
            return 1;
        }

        var dani = playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode && playResultData.Dani is { } inputDani
            ? inputDani with { Stages = validStages.ToList() }
            : null;

        await Ac15DaniWriter.SaveAsync(
            RedDaniTables(),
            dani,
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

        await Ac15DonChallengeWriter.SaveAsync(
            RedDonChallengeTables(),
            new Ac15DonChallengeWriteRequest(request.Baid, red.DonChallenge, validStages, playTime),
            new Ac15DonChallengeRewardMutators(
                ids => Ac15UnlockFlagAccess.Red.ReleaseSongs?.Invoke(saveData, ids),
                ids => Ac15UnlockFlagAccess.Red.Titles(saveData, ids)),
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            RedNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Red.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private async ValueTask<uint> HandleRedTokkun(
        uint baid,
        Ac15PlayResultEnvelope playResultData,
        CancellationToken cancellationToken)
    {
        var saveData = await context.GetOrCreateRedSaveDataAsync(baid, cancellationToken);
        await Ac15TokkunWriter.SaveAsync(
            context,
            new Ac15TokkunTables<RedRecentSongs>(context.RedRecentSongs),
            new Ac15TokkunWriteRequest(
                baid,
                playResultData,
                saveData.TokkunTutorialFlg,
                value => saveData.TokkunTutorialFlg = value,
                Ac15EraProfiles.Red.Limits.MaxRecentSongs,
                ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime)),
            cancellationToken);
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

    private Ac15DonChallengeTables<RedDonChallengeProgress, RedDonChallengeRawFact> RedDonChallengeTables()
        => new(
            context.RedDonChallengeProgress,
            context.RedDonChallengeRawFacts);

}
