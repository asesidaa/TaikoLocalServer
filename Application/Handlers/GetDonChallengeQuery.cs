using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Ac15.DonChallenge;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetDonChallengeAvailabilityQuery(GameEra Era) : IRequest<DonChallengeAvailabilityResponse>;

public readonly record struct GetDonChallengeQuery(GameEra Era, uint Baid) : IRequest<DonChallengeResponse>;

public sealed class GetDonChallengeQueryHandler(
    ITaikoDbContext context,
    IGameDataCatalog catalog,
    ILogger<GetDonChallengeQueryHandler> logger)
    : IRequestHandler<GetDonChallengeAvailabilityQuery, DonChallengeAvailabilityResponse>,
      IRequestHandler<GetDonChallengeQuery, DonChallengeResponse>
{
    public ValueTask<DonChallengeAvailabilityResponse> Handle(
        GetDonChallengeAvailabilityQuery request,
        CancellationToken cancellationToken)
        => ValueTask.FromResult(BuildAvailability(request.Era));

    public async ValueTask<DonChallengeResponse> Handle(
        GetDonChallengeQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Era != GameEra.Red)
        {
            logger.LogInformation("Don Challenge AdminApi unavailable for era {Era}", request.Era);
            return UnavailableResponse(request.Era, $"Don Challenge is not available for {request.Era}.");
        }

        var bundle = catalog.Red().DonChallenge.ActiveBundle;
        if (bundle is null)
        {
            return UnavailableResponse(request.Era, "No active Don Challenge is configured for Red.");
        }

        var saveData = await context.UserSaveDataRed
            .AsNoTracking()
            .SingleOrDefaultAsync(row => row.Baid == request.Baid, cancellationToken);
        var progressRows = await context.RedDonChallengeProgress
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid && row.BundleId == bundle.BundleId)
            .ToArrayAsync(cancellationToken);

        var tasks = bundle.PersonalTasks
            .OrderBy(task => task.Slot)
            .ThenBy(task => task.TaskId)
            .Select(task => BuildTask(task, progressRows))
            .ToList();
        var completedTaskCount = (uint)tasks.Count(task => task.Completed);
        var rewards = bundle.Rewards
            .OrderBy(reward => reward.RequiredCompletedTasks)
            .Select(reward => BuildReward(reward, completedTaskCount, saveData))
            .ToList();

        return new DonChallengeResponse
        {
            Era = nameof(GameEra.Red),
            IsAvailable = true,
            BundleId = bundle.BundleId,
            StartsAt = bundle.StartsAt,
            EndsAt = bundle.EndsAt,
            CompletedTaskCount = completedTaskCount,
            PersonalTaskCount = (uint)bundle.PersonalTasks.Count,
            Tasks = tasks,
            Rewards = rewards
        };
    }

    private DonChallengeAvailabilityResponse BuildAvailability(GameEra era)
    {
        if (era != GameEra.Red)
        {
            return UnavailableAvailability(era, $"Don Challenge is not available for {era}.");
        }

        var bundle = catalog.Red().DonChallenge.ActiveBundle;
        return bundle is null
            ? UnavailableAvailability(era, "No active Don Challenge is configured for Red.")
            : new DonChallengeAvailabilityResponse
            {
                Era = nameof(GameEra.Red),
                IsAvailable = true,
                ActiveBundleId = bundle.BundleId,
                StartsAt = bundle.StartsAt,
                EndsAt = bundle.EndsAt
            };
    }

    private static DonChallengeAvailabilityResponse UnavailableAvailability(GameEra era, string message)
        => new()
        {
            Era = era.ToString(),
            IsAvailable = false,
            Message = message
        };

    private static DonChallengeResponse UnavailableResponse(GameEra era, string message)
        => new()
        {
            Era = era.ToString(),
            IsAvailable = false,
            Message = message
        };

    private static DonChallengeTask BuildTask(
        Ac15DonChallengeTask task,
        IReadOnlyList<RedDonChallengeProgress> progressRows)
    {
        var taskProgressRows = progressRows
            .Where(row => row.TaskId == task.TaskId && row.Slot == task.Slot)
            .ToArray();
        var progressValue = taskProgressRows.Length == 0
            ? 0
            : taskProgressRows.Max(row => row.ProgressValue);
        var completed = taskProgressRows.Any(row => row.Completed);

        return new DonChallengeTask
        {
            TaskId = task.TaskId,
            Slot = task.Slot,
            Name = task.Name,
            RuleLabel = BuildRuleLabel(task.Rule),
            ProgressValue = progressValue,
            TargetValue = GetTargetValue(task.Rule),
            Completed = completed,
            UpdatedAt = taskProgressRows
                .OrderByDescending(row => row.UpdatedAt)
                .Select(row => (DateTime?)row.UpdatedAt)
                .FirstOrDefault(),
            CompletedAt = taskProgressRows
                .Where(row => row.CompletedAt is not null)
                .OrderByDescending(row => row.CompletedAt)
                .Select(row => row.CompletedAt)
                .FirstOrDefault(),
            Tracks = Ac15DonChallengeTrackDefinitions.FromTask(task)
                .Select(track => new DonChallengeTrack
                {
                    TrackNumber = track.TrackNo,
                    SongNumber = track.SongNo,
                    Level = track.Level,
                    StageMode = track.StageMode
                })
                .ToList()
        };
    }

    private static DonChallengeReward BuildReward(
        Ac15DonChallengeReward reward,
        uint completedTaskCount,
        UserSaveDataRed? saveData)
    {
        var earned = reward.RequiredCompletedTasks > 0 && completedTaskCount >= reward.RequiredCompletedTasks
                     || HasAllConfiguredRewardFlags(saveData, reward);
        return new DonChallengeReward
        {
            RequiredCompletedTasks = reward.RequiredCompletedTasks,
            RewardSongNoes = reward.RewardSongNoes.Order().ToList(),
            RewardTitleIds = reward.RewardTitleIds.Order().ToList(),
            Status = earned ? DonChallengeRewardStatus.Earned : DonChallengeRewardStatus.Locked
        };
    }

    private static uint? GetTargetValue(Ac15DonChallengeRule rule)
        => rule.Kind switch
        {
            Ac15DonChallengeRuleKind.Clear or Ac15DonChallengeRuleKind.FullCombo => rule.RequiredStageCount,
            Ac15DonChallengeRuleKind.ScoreThreshold => rule.MinimumScore,
            Ac15DonChallengeRuleKind.CommunityCount => rule.RequiredCommunityCount,
            _ => null
        };

    private static string BuildRuleLabel(Ac15DonChallengeRule rule)
    {
        var label = rule.Kind switch
        {
            Ac15DonChallengeRuleKind.Clear => rule.RequiredStageCount == 1
                ? "Clear 1 song"
                : $"Clear {rule.RequiredStageCount} songs",
            Ac15DonChallengeRuleKind.FullCombo => rule.RequiredStageCount == 1
                ? "Full combo 1 song"
                : $"Full combo {rule.RequiredStageCount} songs",
            Ac15DonChallengeRuleKind.ScoreThreshold => rule.MinimumScore is { } score
                ? $"Score at least {score}"
                : "Score challenge",
            Ac15DonChallengeRuleKind.CommunityCount => rule.RequiredCommunityCount is { } count
                ? $"Community total {count}"
                : "Community challenge",
            _ => "Unsupported task"
        };

        return rule.MinimumLevel is { } minimumLevel
            ? $"{label} at level {minimumLevel}+"
            : label;
    }

    private static bool HasAllConfiguredRewardFlags(UserSaveDataRed? saveData, Ac15DonChallengeReward reward)
    {
        if (saveData is null || reward.RewardSongNoes.Count == 0 && reward.RewardTitleIds.Count == 0)
        {
            return false;
        }

        return reward.RewardSongNoes.All(songNo => BitIsSet(saveData.ReleaseSongFlg, songNo))
               && reward.RewardTitleIds.All(titleId => BitIsSet(saveData.TitleFlg, titleId));
    }

    private static bool BitIsSet(byte[]? source, uint id)
    {
        if (source is null)
        {
            return false;
        }

        var byteIndex = (int)(id >> 3);
        return byteIndex < source.Length && (source[byteIndex] & (1 << ((int)id & 7))) != 0;
    }
}
