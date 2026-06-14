using TaikoLocalServer.Application.Ac15.ChallengeCompe;

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
            .SelectMany(bundle => bundle.PersonalTasks.Select(task => new ActiveChallengeTask(bundle.BundleId, task)))
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
            .Select(task => (task.BundleId, task.Task.TaskId))
            .ToHashSet();
        var progressRows = await context.RedChallengeCompeProgress
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid && activeBundleIds.Contains(row.BundleId))
            .ToArrayAsync(cancellationToken);
        progressRows = progressRows
            .Where(row => activeTaskKeys.Contains((row.BundleId, row.TaskId)))
            .OrderBy(row => row.TaskId)
            .ThenBy(row => row.TrackNo)
            .ToArray();

        var bestScores = await context.SongBestDataRed
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid)
            .ToDictionaryAsync(
                row => (row.SongId, Level: ToLevel(row.Difficulty), StageMode: row.IsShin ? 1u : 0u),
                row => row.BestScore,
                cancellationToken);

        return new CommonChallengeCompeResponse
        {
            AryChallengeStat = activeTasks
                .Select(task => MapTask(task, progressRows, bestScores))
                .Where(task => task.AryTrackStat.Count != 0)
                .ToList()
        };
    }

    private static CommonChallengeCompeResponse.CompeData MapTask(
        ActiveChallengeTask activeTask,
        IReadOnlyList<RedChallengeCompeProgress> progressRows,
        IReadOnlyDictionary<(uint SongId, uint Level, uint StageMode), uint> bestScores)
    {
        var configuredTracks = Ac15ChallengeCompeTrackDefinitions.FromTask(activeTask.Task);
        if (configuredTracks.Count != 0)
        {
            return new CommonChallengeCompeResponse.CompeData
            {
                CompeId = activeTask.Task.CompeId,
                AryTrackStat = configuredTracks
                    .Select(track => MapConfiguredTrack(activeTask, track, progressRows, bestScores))
                    .ToList()
            };
        }

        return new CommonChallengeCompeResponse.CompeData
        {
            CompeId = activeTask.Task.CompeId,
            AryTrackStat = progressRows
                .Where(row => row.BundleId == activeTask.BundleId && row.TaskId == activeTask.Task.TaskId)
                .Select(row => MapTrack(row, bestScores))
                .ToList()
        };
    }

    private static CommonChallengeCompeResponse.TracksData MapConfiguredTrack(
        ActiveChallengeTask activeTask,
        Ac15ChallengeCompeTrackDefinition track,
        IReadOnlyList<RedChallengeCompeProgress> progressRows,
        IReadOnlyDictionary<(uint SongId, uint Level, uint StageMode), uint> bestScores)
    {
        var progress = progressRows
            .Where(row => row.BundleId == activeTask.BundleId
                          && row.TaskId == activeTask.Task.TaskId
                          && (row.TrackNo == track.TrackNo || row.TrackNo == 0)
                          && row.SongNo == track.SongNo
                          && row.Level == track.Level)
            .OrderByDescending(row => row.HighScore)
            .FirstOrDefault();

        var highScore = 0u;
        if (progress is not null)
        {
            bestScores.TryGetValue((progress.SongNo, progress.Level, ToBestStageMode(progress.StageMode)), out var bestScore);
            highScore = Math.Max(progress.HighScore, bestScore);
        }

        return new CommonChallengeCompeResponse.TracksData
        {
            SongNo = track.SongNo,
            Level = track.Level,
            OptionFlg = track.OptionFlg,
            StageMode = track.StageMode,
            HighScore = highScore
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

    private sealed record ActiveChallengeTask(string BundleId, Ac15ChallengeCompeTask Task);
}
