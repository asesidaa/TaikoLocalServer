using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15.DonChallenge;

public sealed record Ac15DonChallengeTables<TProgress, TRawFact>(
    DbSet<TProgress> ProgressRows,
    DbSet<TRawFact> RawFactRows)
    where TProgress : class, IAc15DonChallengeProgress, new()
    where TRawFact : class, IAc15DonChallengeRawFact, new();

public sealed record Ac15DonChallengeWriteRequest(
    uint Baid,
    Ac15DonChallengeCatalog Catalog,
    IReadOnlyList<Ac15StageResult> Stages,
    DateTime PlayTime);

public sealed record Ac15DonChallengeRewardMutators(
    Action<IReadOnlyList<uint>> ReleaseSongs,
    Action<IReadOnlyList<uint>> ReleaseTitles);

public static class Ac15DonChallengeWriter
{
    public static async ValueTask SaveAsync<TProgress, TRawFact>(
        Ac15DonChallengeTables<TProgress, TRawFact> tables,
        Ac15DonChallengeWriteRequest request,
        Ac15DonChallengeRewardMutators rewards,
        CancellationToken cancellationToken)
        where TProgress : class, IAc15DonChallengeProgress, new()
        where TRawFact : class, IAc15DonChallengeRawFact, new()
    {
        var evaluations = Ac15DonChallengeProgressEvaluator.Evaluate(request.Catalog, request.Stages);
        if (evaluations.Count == 0)
        {
            return;
        }

        foreach (var evaluation in evaluations)
        {
            tables.RawFactRows.Add(new TRawFact
            {
                Baid = request.Baid,
                BundleId = evaluation.BundleId,
                TaskId = evaluation.Task.TaskId,
                Slot = evaluation.Task.Slot,
                CompeId = evaluation.Task.CompeId,
                TrackNo = evaluation.Track.TrackNo,
                SongNo = evaluation.Stage.SongNo,
                Level = Ac15Difficulty.ToProtocol(evaluation.Stage.Level),
                OptionFlg = evaluation.Stage.OptionFlg,
                StageMode = evaluation.Stage.StageMode,
                HighScore = evaluation.Stage.PlayScore,
                PlayResult = evaluation.Stage.PlayResult,
                ProgressValue = evaluation.ProgressValue,
                Completed = evaluation.Completed,
                PlayTime = request.PlayTime,
                CreatedAt = request.PlayTime
            });
        }

        foreach (var group in evaluations.GroupBy(evaluation => new
                 {
                     evaluation.BundleId,
                     evaluation.Task.TaskId,
                     evaluation.Task.Slot
                 }))
        {
            var groupEvaluations = group.ToArray();
            var representative = groupEvaluations.OrderByDescending(evaluation => evaluation.ProgressValue).First();
            var progressValue = await GetProgressValueAsync(
                tables.RawFactRows,
                request.Baid,
                representative.Task,
                groupEvaluations,
                cancellationToken);
            var completed = IsCompleted(representative.Task.Rule, progressValue, groupEvaluations);

            var progress = await tables.ProgressRows.FindAsync(
                [request.Baid, group.Key.BundleId, group.Key.TaskId, 0u],
                cancellationToken);
            if (progress is null)
            {
                tables.ProgressRows.Add(new TProgress
                {
                    Baid = request.Baid,
                    BundleId = group.Key.BundleId,
                    TaskId = representative.Task.TaskId,
                    Slot = representative.Task.Slot,
                    CompeId = representative.Task.CompeId,
                    TrackNo = 0,
                    SongNo = representative.Stage.SongNo,
                    Level = Ac15Difficulty.ToProtocol(representative.Stage.Level),
                    OptionFlg = representative.Stage.OptionFlg,
                    StageMode = representative.Stage.StageMode,
                    HighScore = representative.Stage.PlayScore,
                    ProgressValue = progressValue,
                    Completed = completed,
                    UpdatedAt = request.PlayTime,
                    CompletedAt = completed ? request.PlayTime : null
                });
                continue;
            }

            progress.Slot = representative.Task.Slot;
            progress.CompeId = representative.Task.CompeId;
            progress.SongNo = representative.Stage.SongNo;
            progress.Level = Ac15Difficulty.ToProtocol(representative.Stage.Level);
            progress.OptionFlg = representative.Stage.OptionFlg;
            progress.StageMode = representative.Stage.StageMode;
            progress.HighScore = Math.Max(progress.HighScore, representative.Stage.PlayScore);
            progress.ProgressValue = Math.Max(progress.ProgressValue, progressValue);
            progress.UpdatedAt = request.PlayTime;
            if (completed && !progress.Completed)
            {
                progress.Completed = true;
                progress.CompletedAt = request.PlayTime;
            }
        }

        await ApplyRewardsAsync(
            tables.ProgressRows,
            request.Baid,
            request.Catalog,
            rewards,
            cancellationToken);
    }

    private static async ValueTask ApplyRewardsAsync<TProgress>(
        DbSet<TProgress> progressRows,
        uint baid,
        Ac15DonChallengeCatalog catalog,
        Ac15DonChallengeRewardMutators rewards,
        CancellationToken cancellationToken)
        where TProgress : class, IAc15DonChallengeProgress
    {
        var activeBundleIds = catalog.GetActiveBundles()
            .Select(bundle => bundle.BundleId)
            .ToArray();
        if (activeBundleIds.Length == 0)
        {
            return;
        }

        var completedTasks = new HashSet<(string BundleId, uint TaskId)>();
        var savedCompleted = await progressRows
            .Where(row => row.Baid == baid && row.Completed && activeBundleIds.Contains(row.BundleId))
            .Select(row => new { row.BundleId, row.TaskId })
            .ToArrayAsync(cancellationToken);
        foreach (var row in savedCompleted)
        {
            completedTasks.Add((row.BundleId, row.TaskId));
        }

        foreach (var row in progressRows.Local.Where(row =>
                     row.Baid == baid && row.Completed && activeBundleIds.Contains(row.BundleId)))
        {
            completedTasks.Add((row.BundleId, row.TaskId));
        }

        var completedCounts = completedTasks
            .GroupBy(key => key.BundleId)
            .ToDictionary(group => group.Key, group => (uint)group.Select(key => key.TaskId).Distinct().Count());
        var grant = Ac15DonChallengeRewardDecisions.GetEarnedRewards(catalog, completedCounts);

        rewards.ReleaseSongs(grant.RewardSongNoes);
        rewards.ReleaseTitles(grant.RewardTitleIds);
    }

    private static async ValueTask<uint> GetProgressValueAsync<TRawFact>(
        DbSet<TRawFact> rawFactRows,
        uint baid,
        Ac15DonChallengeTask task,
        IReadOnlyList<Ac15DonChallengeStageEvaluation> evaluations,
        CancellationToken cancellationToken)
        where TRawFact : class, IAc15DonChallengeRawFact
    {
        if (!task.Rule.RequiresDistinctSongProgress)
        {
            return evaluations.Max(evaluation => evaluation.ProgressValue);
        }

        var bundleId = evaluations[0].BundleId;
        var existingSongNoes = await rawFactRows
            .Where(row => row.Baid == baid
                          && row.BundleId == bundleId
                          && row.TaskId == task.TaskId
                          && row.ProgressValue > 0)
            .Select(row => row.SongNo)
            .ToArrayAsync(cancellationToken);

        return (uint)existingSongNoes
            .Concat(evaluations.Select(evaluation => evaluation.Stage.SongNo))
            .Distinct()
            .Count();
    }

    private static bool IsCompleted(
        Ac15DonChallengeRule rule,
        uint progressValue,
        IEnumerable<Ac15DonChallengeStageEvaluation> evaluations)
        => rule.RequiresDistinctSongProgress
            ? progressValue >= rule.RequiredStageCount
            : evaluations.Any(evaluation => evaluation.Completed);
}
