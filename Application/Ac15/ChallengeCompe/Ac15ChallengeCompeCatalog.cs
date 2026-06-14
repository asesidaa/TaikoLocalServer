namespace TaikoLocalServer.Application.Ac15.ChallengeCompe;

public sealed record Ac15ChallengeCompeCatalog(
    bool Enabled,
    IReadOnlyList<Ac15ChallengeCompeMonthlyBundle> MonthlyBundles)
{
    public static Ac15ChallengeCompeCatalog Disabled { get; } = new(false, []);

    public IReadOnlyList<Ac15ChallengeCompeMonthlyBundle> GetActiveBundles(DateTimeOffset now)
        => !Enabled
            ? []
            : MonthlyBundles
                .Where(bundle => bundle.IsActiveAt(now))
                .ToArray();
}

public sealed record Ac15ChallengeCompeMonthlyBundle(
    string BundleId,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    IReadOnlyList<Ac15ChallengeCompeTask> PersonalTasks,
    Ac15ChallengeCompeCommunityTask? CommunityTask,
    IReadOnlyList<Ac15ChallengeCompeReward> Rewards)
{
    public const int ExpectedPersonalTaskCount = 10;

    public bool HasExpectedPersonalTaskCount => PersonalTasks.Count == ExpectedPersonalTaskCount;

    public bool IsActiveAt(DateTimeOffset now)
        => (StartsAt is null || StartsAt <= now)
            && (EndsAt is null || now < EndsAt);
}

public sealed record Ac15ChallengeCompeTask(
    uint TaskId,
    uint Slot,
    string Name,
    Ac15ChallengeCompeRule Rule)
{
    public uint CompeId => TaskId;

    public uint TrackNo => Slot;
}

public sealed record Ac15ChallengeCompeCommunityTask(
    uint TaskId,
    string Name,
    Ac15ChallengeCompeRule Rule,
    string? Description);

public sealed record Ac15ChallengeCompeRule(
    Ac15ChallengeCompeRuleKind Kind,
    uint? Threshold,
    IReadOnlyList<uint> SongNoes)
{
    public static Ac15ChallengeCompeRule Unsupported { get; } = new(
        Ac15ChallengeCompeRuleKind.Unsupported,
        null,
        []);

    public bool CanExecute => Kind switch
    {
        Ac15ChallengeCompeRuleKind.Clear => true,
        Ac15ChallengeCompeRuleKind.FullCombo => true,
        Ac15ChallengeCompeRuleKind.ScoreThreshold => Threshold > 0,
        Ac15ChallengeCompeRuleKind.SongSetCount => Threshold > 0 && SongNoes.Count > 0,
        Ac15ChallengeCompeRuleKind.CommunityCount => Threshold > 0,
        _ => false
    };
}

public enum Ac15ChallengeCompeRuleKind
{
    Unsupported = 0,
    Clear,
    FullCombo,
    ScoreThreshold,
    SongSetCount,
    CommunityCount
}

public sealed record Ac15ChallengeCompeReward(
    uint RequiredCompletedTasks,
    IReadOnlyList<uint> RewardSongNoes,
    IReadOnlyList<uint> RewardTitleIds);
