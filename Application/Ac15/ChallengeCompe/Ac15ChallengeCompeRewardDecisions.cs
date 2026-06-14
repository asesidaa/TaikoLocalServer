namespace TaikoLocalServer.Application.Ac15.ChallengeCompe;

public sealed record Ac15ChallengeCompeRewardGrant(
    IReadOnlyList<uint> RewardSongNoes,
    IReadOnlyList<uint> RewardTitleIds);

public static class Ac15ChallengeCompeRewardDecisions
{
    public static Ac15ChallengeCompeRewardGrant GetEarnedRewards(
        Ac15ChallengeCompeCatalog catalog,
        DateTimeOffset activeAt,
        IReadOnlyDictionary<string, uint> completedTaskCountsByBundle)
    {
        var songNoes = new SortedSet<uint>();
        var titleIds = new SortedSet<uint>();

        foreach (var bundle in catalog.GetActiveBundles(activeAt))
        {
            completedTaskCountsByBundle.TryGetValue(bundle.BundleId, out var completedTasks);
            foreach (var reward in bundle.Rewards.Where(reward =>
                         reward.RequiredCompletedTasks > 0 && completedTasks >= reward.RequiredCompletedTasks))
            {
                songNoes.UnionWith(reward.RewardSongNoes);
                titleIds.UnionWith(reward.RewardTitleIds);
            }
        }

        return new Ac15ChallengeCompeRewardGrant(songNoes.ToArray(), titleIds.ToArray());
    }
}
