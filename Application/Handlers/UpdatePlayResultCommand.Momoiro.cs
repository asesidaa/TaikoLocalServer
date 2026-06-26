using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint MomoiroDanCostumeId = 36;

    private partial async ValueTask<uint> HandleMomoiro(
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
            logger.LogWarning("Game uploading a non existing Momoiro user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var normal = playResultData.Normal;
        IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            stages,
            Ac15EraProfiles.Momoiro.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Momoiro playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }
        LogMomoiroChallengeFacts(request.Baid, validStages);

        var saveData = await context.GetOrCreateMomoiroSaveDataAsync(request.Baid, cancellationToken);
        var momoiro = gameDataService.Momoiro();
        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApplyDonPoints(
                saveData,
                playResultData.Profile,
                validStages,
                Ac15ProfileCounterUpdater.Momoiro,
                Ac15UnlockFlagAccess.Momoiro,
                Ac15EraProfiles.Momoiro.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Momoiro Don point totals for baid {Baid}", request.Baid);
            return 1;
        }

        var dani = playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode && playResultData.Dani is { } inputDani
            ? inputDani with { Stages = validStages.ToList() }
            : null;

        await Ac15DaniWriter.SaveAsync(
            MomoiroDaniTables(),
            dani,
            Ac15EraProfiles.Momoiro.Limits,
            momoiro.DaniFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
            new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, MomoiroDanCostumeId),
            update =>
            {
                saveData.GotDanFlg = update.GotDanFlg;
                saveData.GotDanExtraFlg = update.GotDanExtraFlg;
                saveData.GotDanMax = update.GotDanMax;
                saveData.DispTaikojukuDan = update.DisplayDan;
                saveData.DispDanType = saveData.DispDanType == 0 ? 1u : saveData.DispDanType;
                if (update.ApplyDanCostume)
                {
                    Ac15CustomizationMutation.ApplyDanCostume(saveData, update.DanCostumeId, Ac15EraProfiles.Momoiro.Limits);
                }
            },
            logger,
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            MomoiroNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Momoiro.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumMomoiro, SongBestDatumMomoiro, MomoiroFavoriteSongs, MomoiroRecentSongs> MomoiroNormalPlayTables()
        => new(
            context.SongPlayDataMomoiro,
            context.SongBestDataMomoiro,
            context.MomoiroFavoriteSongs,
            context.MomoiroRecentSongs,
            Ac15NormalPlayMapper.ToMomoiroSongPlayDatum,
            Ac15NormalPlayMapper.ToMomoiroSongBestDatum,
            CreateFavorite: CreateMomoiroFavoriteAsync);

    private Ac15DaniTables<DanScoreDatumMomoiro, DanStageScoreDatumMomoiro> MomoiroDaniTables()
        => new(
            context.DanScoreDataMomoiro,
            context.DanScoreDataMomoiro.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToMomoiroDanScoreDatum,
            Ac15DaniMapper.ApplyToMomoiroDanScoreDatum,
            Ac15DaniMapper.ToMomoiroDanStageScoreDatum,
            Ac15DaniMapper.ApplyToMomoiroDanStageScoreDatum);

    private async ValueTask<MomoiroFavoriteSongs> CreateMomoiroFavoriteAsync(
        uint baid,
        uint songNo,
        CancellationToken cancellationToken)
    {
        var persistedOrders = await context.MomoiroFavoriteSongs
            .Where(song => song.Baid == baid)
            .Select(song => song.DisplayOrder)
            .ToArrayAsync(cancellationToken);
        var trackedOrders = context.MomoiroFavoriteSongs.Local
            .Where(song => song.Baid == baid)
            .Select(song => song.DisplayOrder);
        var displayOrder = persistedOrders.Concat(trackedOrders).DefaultIfEmpty(-1).Max() + 1;

        return new MomoiroFavoriteSongs
        {
            Baid = baid,
            SongNo = songNo,
            DisplayOrder = displayOrder
        };
    }

    private void LogMomoiroChallengeFacts(uint baid, IEnumerable<Ac15StageResult> stages)
    {
        var challengeIds = 0;
        var userCompeIds = 0;
        var bngCompeIds = 0;
        foreach (var stage in stages)
        {
            challengeIds += stage.ChallengeIds.Count;
            userCompeIds += stage.UserCompeIds.Count;
            bngCompeIds += stage.BngCompeIds.Count;
        }

        if (challengeIds == 0 && userCompeIds == 0 && bngCompeIds == 0)
        {
            return;
        }

        logger.LogInformation(
            "Momoiro playresult challenge-shaped fields received for baid {Baid}: ChallengeIds={ChallengeIds}, UserCompeIds={UserCompeIds}, BngCompeIds={BngCompeIds}; no challenge state persisted",
            baid,
            challengeIds,
            userCompeIds,
            bngCompeIds);
    }
}
