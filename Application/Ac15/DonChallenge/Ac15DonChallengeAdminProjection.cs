using TaikoLocalServer.Contracts.AdminApi.Responses;
using AdminReward = TaikoLocalServer.Contracts.AdminApi.ViewModels.DonChallengeReward;
using AdminTask = TaikoLocalServer.Contracts.AdminApi.ViewModels.DonChallengeTask;
using AdminTrack = TaikoLocalServer.Contracts.AdminApi.ViewModels.DonChallengeTrack;

namespace TaikoLocalServer.Application.Ac15.DonChallenge;

public sealed record Ac15DonChallengeRewardFlagState(
    byte[]? ReleaseSongFlags,
    byte[]? TitleFlags);

public static class Ac15DonChallengeAdminProjection
{
    public static DonChallengeAvailabilityResponse BuildAvailability(
        GameEra era,
        Ac15DonChallengeMonthlyBundle bundle)
        => new()
        {
            Era = era.ToString(),
            IsAvailable = true,
            ActiveBundleId = bundle.BundleId,
            StartsAt = bundle.StartsAt,
            EndsAt = bundle.EndsAt
        };

    public static DonChallengeResponse BuildResponse<TProgress>(
        GameEra era,
        Ac15DonChallengeMonthlyBundle bundle,
        IReadOnlyList<TProgress> progressRows,
        Ac15DonChallengeRewardFlagState? rewardFlags)
        where TProgress : IAc15DonChallengeProgress
    {
        var tasks = bundle.PersonalTasks
            .OrderBy(task => task.Slot)
            .ThenBy(task => task.TaskId)
            .Select(task => BuildTask(task, progressRows))
            .ToList();
        var completedTaskCount = (uint)tasks.Count(task => task.Completed);
        var rewards = bundle.Rewards
            .OrderBy(reward => reward.RequiredCompletedTasks)
            .Select(reward => BuildReward(reward, completedTaskCount, rewardFlags))
            .ToList();

        return new DonChallengeResponse
        {
            Era = era.ToString(),
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

    private static AdminTask BuildTask<TProgress>(
        Ac15DonChallengeTask task,
        IReadOnlyList<TProgress> progressRows)
        where TProgress : IAc15DonChallengeProgress
    {
        var taskProgressRows = progressRows
            .Where(row => row.TaskId == task.TaskId && row.Slot == task.Slot)
            .ToArray();
        var progressValue = taskProgressRows.Length == 0
            ? 0
            : taskProgressRows.Max(row => row.ProgressValue);
        var completed = taskProgressRows.Any(row => row.Completed);

        return new AdminTask
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
                .Select(track => new AdminTrack
                {
                    TrackNumber = track.TrackNo,
                    SongNumber = track.SongNo,
                    Level = Ac15Difficulty.ToProtocol(track.Level),
                    StageMode = track.StageMode
                })
                .ToList()
        };
    }

    private static AdminReward BuildReward(
        Ac15DonChallengeReward reward,
        uint completedTaskCount,
        Ac15DonChallengeRewardFlagState? rewardFlags)
    {
        var earned = reward.RequiredCompletedTasks > 0 && completedTaskCount >= reward.RequiredCompletedTasks
                     || HasAllConfiguredRewardFlags(rewardFlags, reward);
        return new AdminReward
        {
            RequiredCompletedTasks = reward.RequiredCompletedTasks,
            RewardSongNoes = reward.RewardSongNoes.Order().ToList(),
            RewardTitleIds = reward.RewardTitleIds.Order().ToList(),
            Status = earned
                ? TaikoLocalServer.Contracts.AdminApi.ViewModels.DonChallengeRewardStatus.Earned
                : TaikoLocalServer.Contracts.AdminApi.ViewModels.DonChallengeRewardStatus.Locked
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
            ? $"{label} at level {Ac15Difficulty.ToProtocol(minimumLevel)}+"
            : label;
    }

    private static bool HasAllConfiguredRewardFlags(
        Ac15DonChallengeRewardFlagState? rewardFlags,
        Ac15DonChallengeReward reward)
    {
        if (rewardFlags is null || reward.RewardSongNoes.Count == 0 && reward.RewardTitleIds.Count == 0)
        {
            return false;
        }

        return reward.RewardSongNoes.All(songNo => BitIsSet(rewardFlags.ReleaseSongFlags, songNo))
               && reward.RewardTitleIds.All(titleId => BitIsSet(rewardFlags.TitleFlags, titleId));
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
