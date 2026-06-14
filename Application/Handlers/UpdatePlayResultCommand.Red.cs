using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Ac15.ChallengeCompe;
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
        if (IsRedTokkunShaped(playResultData))
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

        await SaveRedChallengeCompeAsync(
            request.Baid,
            red.ChallengeCompe,
            validStages,
            saveData,
            playTime,
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            RedNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Red.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private async ValueTask SaveRedChallengeCompeAsync(
        uint baid,
        Ac15ChallengeCompeCatalog catalog,
        IReadOnlyList<Ac15StageResult> stages,
        UserSaveDataRed saveData,
        DateTime playTime,
        CancellationToken cancellationToken)
    {
        if (!saveData.IsChallengeCompe)
        {
            return;
        }

        var activeAt = new DateTimeOffset(DateTime.SpecifyKind(playTime, DateTimeKind.Utc));
        var evaluations = Ac15ChallengeCompeProgressEvaluator.Evaluate(catalog, activeAt, stages);
        if (evaluations.Count == 0)
        {
            return;
        }

        foreach (var evaluation in evaluations)
        {
            context.RedChallengeCompeRawFacts.Add(new RedChallengeCompeRawFact
            {
                Baid = baid,
                BundleId = evaluation.BundleId,
                TaskId = evaluation.Task.TaskId,
                Slot = evaluation.Task.Slot,
                CompeId = evaluation.Fact.CompeId,
                TrackNo = evaluation.Fact.TrackNo,
                SongNo = evaluation.Stage.SongNo,
                Level = evaluation.Stage.Level,
                OptionFlg = evaluation.Stage.OptionFlg,
                StageMode = evaluation.Stage.StageMode,
                HighScore = evaluation.Stage.PlayScore,
                PlayResult = evaluation.Stage.PlayResult,
                ProgressValue = evaluation.ProgressValue,
                Completed = evaluation.Completed,
                PlayTime = playTime,
                CreatedAt = playTime
            });
        }

        foreach (var group in evaluations.GroupBy(evaluation => new
                 {
                     evaluation.BundleId,
                     evaluation.Task.TaskId,
                     evaluation.Task.Slot,
                     evaluation.Fact.TrackNo
                 }))
        {
            var representative = group.OrderByDescending(evaluation => evaluation.ProgressValue).First();
            var progressValue = await GetProgressValueAsync(baid, representative.Task, group.ToArray(), cancellationToken);
            var completed = IsCompleted(representative.Task.Rule, progressValue, group);

            var progress = await context.RedChallengeCompeProgress.FindAsync(
                [baid, group.Key.BundleId, group.Key.TaskId, group.Key.TrackNo],
                cancellationToken);
            if (progress is null)
            {
                context.RedChallengeCompeProgress.Add(new RedChallengeCompeProgress
                {
                    Baid = baid,
                    BundleId = group.Key.BundleId,
                    TaskId = representative.Task.TaskId,
                    Slot = representative.Task.Slot,
                    CompeId = representative.Fact.CompeId,
                    TrackNo = representative.Fact.TrackNo,
                    SongNo = representative.Stage.SongNo,
                    Level = representative.Stage.Level,
                    OptionFlg = representative.Stage.OptionFlg,
                    StageMode = representative.Stage.StageMode,
                    HighScore = representative.Stage.PlayScore,
                    ProgressValue = progressValue,
                    Completed = completed,
                    UpdatedAt = playTime,
                    CompletedAt = completed ? playTime : null
                });
                continue;
            }

            progress.Slot = representative.Task.Slot;
            progress.CompeId = representative.Fact.CompeId;
            progress.SongNo = representative.Stage.SongNo;
            progress.Level = representative.Stage.Level;
            progress.OptionFlg = representative.Stage.OptionFlg;
            progress.StageMode = representative.Stage.StageMode;
            progress.HighScore = Math.Max(progress.HighScore, representative.Stage.PlayScore);
            progress.ProgressValue = Math.Max(progress.ProgressValue, progressValue);
            progress.UpdatedAt = playTime;
            if (completed && !progress.Completed)
            {
                progress.Completed = true;
                progress.CompletedAt = playTime;
            }
        }

        await ApplyRedChallengeCompeRewardsAsync(
            baid,
            catalog,
            activeAt,
            saveData,
            cancellationToken);
    }

    private async ValueTask ApplyRedChallengeCompeRewardsAsync(
        uint baid,
        Ac15ChallengeCompeCatalog catalog,
        DateTimeOffset activeAt,
        UserSaveDataRed saveData,
        CancellationToken cancellationToken)
    {
        var activeBundleIds = catalog.GetActiveBundles(activeAt)
            .Select(bundle => bundle.BundleId)
            .ToArray();
        if (activeBundleIds.Length == 0)
        {
            return;
        }

        var completedTasks = new HashSet<(string BundleId, uint TaskId)>();
        var savedCompleted = await context.RedChallengeCompeProgress
            .Where(row => row.Baid == baid && row.Completed && activeBundleIds.Contains(row.BundleId))
            .Select(row => new { row.BundleId, row.TaskId })
            .ToArrayAsync(cancellationToken);
        foreach (var row in savedCompleted)
        {
            completedTasks.Add((row.BundleId, row.TaskId));
        }

        foreach (var row in context.RedChallengeCompeProgress.Local.Where(row =>
                     row.Baid == baid && row.Completed && activeBundleIds.Contains(row.BundleId)))
        {
            completedTasks.Add((row.BundleId, row.TaskId));
        }

        var completedCounts = completedTasks
            .GroupBy(key => key.BundleId)
            .ToDictionary(group => group.Key, group => (uint)group.Select(key => key.TaskId).Distinct().Count());
        var grant = Ac15ChallengeCompeRewardDecisions.GetEarnedRewards(catalog, activeAt, completedCounts);

        Ac15UnlockFlagAccess.Red.ReleaseSongs?.Invoke(saveData, grant.RewardSongNoes);
        Ac15UnlockFlagAccess.Red.Titles(saveData, grant.RewardTitleIds);
    }

    private async ValueTask<uint> GetProgressValueAsync(
        uint baid,
        Ac15ChallengeCompeTask task,
        IReadOnlyList<Ac15ChallengeCompeStageEvaluation> evaluations,
        CancellationToken cancellationToken)
    {
        if (task.Rule.Kind != Ac15ChallengeCompeRuleKind.SongSetCount)
        {
            return evaluations.Max(evaluation => evaluation.ProgressValue);
        }

        var bundleId = evaluations[0].BundleId;
        var trackNo = evaluations[0].Fact.TrackNo;
        var existingSongNoes = await context.RedChallengeCompeRawFacts
            .Where(row => row.Baid == baid
                          && row.BundleId == bundleId
                          && row.TaskId == task.TaskId
                          && row.TrackNo == trackNo
                          && row.ProgressValue > 0)
            .Select(row => row.SongNo)
            .ToArrayAsync(cancellationToken);

        return (uint)existingSongNoes
            .Concat(evaluations.Select(evaluation => evaluation.Stage.SongNo))
            .Distinct()
            .Count();
    }

    private static bool IsCompleted(
        Ac15ChallengeCompeRule rule,
        uint progressValue,
        IEnumerable<Ac15ChallengeCompeStageEvaluation> evaluations)
        => rule.Kind == Ac15ChallengeCompeRuleKind.SongSetCount
            ? rule.Threshold is { } threshold && progressValue >= threshold
            : evaluations.Any(evaluation => evaluation.Completed);

    private async ValueTask<uint> HandleRedTokkun(
        uint baid,
        Ac15PlayResultEnvelope playResultData,
        CancellationToken cancellationToken)
    {
        var saveData = await context.GetOrCreateRedSaveDataAsync(baid, cancellationToken);
        if (playResultData.Tokkun?.TutorialFlg is { } tokkunTutorialFlg)
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

    private static bool IsRedTokkunShaped(Ac15PlayResultEnvelope playResultData)
        => playResultData.Metadata.PlayMode == (uint)PlayMode.Tokkun
           || playResultData.Tokkun is not null;
}
