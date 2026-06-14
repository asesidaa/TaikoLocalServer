namespace TaikoLocalServer.Application.Handlers;

public partial class GetChallengeCompeQueryHandler
{
    private partial async ValueTask<CommonChallengeCompeResponse> HandleRed(
        GetChallengeCompeQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Baid == 0)
        {
            return new CommonChallengeCompeResponse();
        }

        var saveData = await context.UserSaveDataRed
            .AsNoTracking()
            .SingleOrDefaultAsync(row => row.Baid == request.Baid, cancellationToken);
        if (saveData is not { IsChallengeCompe: true })
        {
            logger.LogInformation("Red ChallengeCompe readback skipped for baid {Baid}: user is not enrolled", request.Baid);
            return new CommonChallengeCompeResponse();
        }

        var activeTasks = gameDataService.Red().ChallengeCompe
            .GetActiveBundles()
            .SelectMany(bundle => bundle.PersonalTasks.Select(task => new ActiveChallengeTask(bundle.BundleId, task.TaskId, task.TrackNo)))
            .ToArray();
        if (activeTasks.Length == 0)
        {
            return new CommonChallengeCompeResponse();
        }

        var activeBundleIds = activeTasks
            .Select(task => task.BundleId)
            .Distinct()
            .ToArray();
        var activeTaskKeys = activeTasks
            .Select(task => (task.BundleId, task.TaskId, task.TrackNo))
            .ToHashSet();
        var progressRows = await context.RedChallengeCompeProgress
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid && activeBundleIds.Contains(row.BundleId))
            .ToArrayAsync(cancellationToken);
        progressRows = progressRows
            .Where(row => activeTaskKeys.Contains((row.BundleId, row.TaskId, row.TrackNo)))
            .OrderBy(row => row.TaskId)
            .ThenBy(row => row.TrackNo)
            .ToArray();
        if (progressRows.Length == 0)
        {
            return new CommonChallengeCompeResponse();
        }

        var bestScores = await context.SongBestDataRed
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid)
            .ToDictionaryAsync(
                row => (row.SongId, Level: ToLevel(row.Difficulty), StageMode: row.IsShin ? 1u : 0u),
                row => row.BestScore,
                cancellationToken);

        return new CommonChallengeCompeResponse
        {
            AryChallengeStat = progressRows
                .GroupBy(row => row.CompeId)
                .Select(group => new CommonChallengeCompeResponse.CompeData
                {
                    CompeId = group.Key,
                    AryTrackStat = group
                        .Select(row => MapTrack(row, bestScores))
                        .ToList()
                })
                .ToList()
        };
    }

    private static CommonChallengeCompeResponse.TracksData MapTrack(
        RedChallengeCompeProgress progress,
        IReadOnlyDictionary<(uint SongId, uint Level, uint StageMode), uint> bestScores)
    {
        bestScores.TryGetValue((progress.SongNo, progress.Level, ToBestStageMode(progress.StageMode)), out var bestScore);
        return new CommonChallengeCompeResponse.TracksData
        {
            SongNo = progress.SongNo,
            Level = progress.Level,
            OptionFlg = progress.OptionFlg,
            StageMode = progress.StageMode,
            HighScore = Math.Max(progress.HighScore, bestScore)
        };
    }

    private static uint ToLevel(Difficulty difficulty)
        => difficulty switch
        {
            Difficulty.Easy => 1,
            Difficulty.Normal => 2,
            Difficulty.Hard => 3,
            Difficulty.Oni => 4,
            Difficulty.UraOni => 5,
            _ => 0
        };

    private static uint ToBestStageMode(uint stageMode) => stageMode is 1 or 4 ? 1u : 0u;

    private sealed record ActiveChallengeTask(string BundleId, uint TaskId, uint TrackNo);
}
