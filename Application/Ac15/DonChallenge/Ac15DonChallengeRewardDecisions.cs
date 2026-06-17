namespace TaikoLocalServer.Application.Ac15.DonChallenge;

public sealed record Ac15DonChallengeRewardGrant(
    IReadOnlyList<uint> RewardSongNoes,
    IReadOnlyList<uint> RewardTitleIds);

public static class Ac15DonChallengeRewardDecisions
{
    public static Ac15DonChallengeRewardGrant GetEarnedRewards(
        Ac15DonChallengeCatalog catalog,
        IReadOnlyDictionary<string, uint> completedTaskCountsByBundle)
    {
        var songNoes = new SortedSet<uint>();
        var titleIds = new SortedSet<uint>();

        foreach (var bundle in catalog.GetActiveBundles())
        {
            completedTaskCountsByBundle.TryGetValue(bundle.BundleId, out var completedTasks);
            foreach (var reward in bundle.Rewards.Where(reward =>
                         reward.RequiredCompletedTasks > 0 && completedTasks >= reward.RequiredCompletedTasks))
            {
                songNoes.UnionWith(reward.RewardSongNoes);
                titleIds.UnionWith(reward.RewardTitleIds);
            }
        }

        return new Ac15DonChallengeRewardGrant(songNoes.ToArray(), titleIds.ToArray());
    }

    public static IReadOnlyList<uint> GetLockedRewardSongIds(
        Ac15DonChallengeCatalog catalog,
        byte[] releaseSongFlags,
        int songFlagBytes)
    {
        var releaseFlags = Ac15ProtocolBytes.FixedOrZero(releaseSongFlags, songFlagBytes);
        return catalog.GetActiveBundles()
            .SelectMany(bundle => bundle.Rewards)
            .Where(reward => reward.RequiredCompletedTasks > 0)
            .SelectMany(reward => reward.RewardSongNoes)
            .Distinct()
            .Where(songNo => !IsBitSet(releaseFlags, songNo))
            .Order()
            .ToArray();
    }

    private static bool IsBitSet(byte[] source, uint id)
    {
        var byteIndex = (int)(id >> 3);
        return byteIndex < source.Length && (source[byteIndex] & (1 << ((int)id & 7))) != 0;
    }
}
