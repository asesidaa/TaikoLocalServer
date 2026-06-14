using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15.ChallengeCompe;

public sealed record Ac15ChallengeCompeCatalog
{
    public Ac15ChallengeCompeCatalog()
    {
    }

    public Ac15ChallengeCompeCatalog(
        bool enabled,
        string? activeBundleId,
        IReadOnlyList<Ac15ChallengeCompeMonthlyBundle> monthlyBundles)
    {
        Enabled = enabled;
        ActiveBundleId = activeBundleId;
        MonthlyBundles = monthlyBundles;
    }

    public static Ac15ChallengeCompeCatalog Disabled { get; } = new(false, null, []);

    public bool Enabled { get; init; }

    public string? ActiveBundleId { get; init; }

    public IReadOnlyList<Ac15ChallengeCompeMonthlyBundle> MonthlyBundles { get; init; } = [];

    public Ac15ChallengeCompeMonthlyBundle? ActiveBundle
        => Enabled && ActiveBundleId is { } id
            ? MonthlyBundles.FirstOrDefault(bundle => string.Equals(bundle.BundleId, id, StringComparison.OrdinalIgnoreCase))
            : null;

    public IReadOnlyList<Ac15ChallengeCompeMonthlyBundle> GetActiveBundles()
        => ActiveBundle is { } activeBundle ? [activeBundle] : [];
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

    public bool HasConfiguredWindow => StartsAt is not null || EndsAt is not null;
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
    uint? MinimumScore = null,
    uint? RequiredSongCount = null,
    uint? RequiredCommunityCount = null,
    uint? MinimumLevel = null,
    IReadOnlyList<uint>? EligibleSongNoes = null)
{
    public static Ac15ChallengeCompeRule Unsupported { get; } = new(
        Ac15ChallengeCompeRuleKind.Unsupported);

    public IReadOnlyList<uint> EligibleSongNoes { get; init; } = EligibleSongNoes ?? [];

    public bool CanExecute => Kind switch
    {
        Ac15ChallengeCompeRuleKind.Clear => RequiredSongCount is null or > 0,
        Ac15ChallengeCompeRuleKind.FullCombo => RequiredSongCount is null or > 0,
        Ac15ChallengeCompeRuleKind.ScoreThreshold => MinimumScore > 0,
        Ac15ChallengeCompeRuleKind.CommunityCount => RequiredCommunityCount > 0,
        _ => false
    };

    public uint RequiredStageCount => RequiredSongCount.GetValueOrDefault(1);

    public bool RequiresDistinctSongProgress
        => (Kind is Ac15ChallengeCompeRuleKind.Clear
               or Ac15ChallengeCompeRuleKind.FullCombo)
           && RequiredStageCount > 1;

    public bool AllowsStage(Ac15StageResult stage)
        => (MinimumLevel is null || stage.Level >= MinimumLevel)
           && (EligibleSongNoes.Count == 0 || EligibleSongNoes.Contains(stage.SongNo));
}

public enum Ac15ChallengeCompeRuleKind
{
    Unsupported = 0,
    Clear,
    FullCombo,
    ScoreThreshold,
    CommunityCount
}

public sealed record Ac15ChallengeCompeReward(
    uint RequiredCompletedTasks,
    IReadOnlyList<uint> RewardSongNoes,
    IReadOnlyList<uint> RewardTitleIds);
