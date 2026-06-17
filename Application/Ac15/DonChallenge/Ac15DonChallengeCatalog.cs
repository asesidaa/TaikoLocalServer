using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15.DonChallenge;

public sealed record Ac15DonChallengeCatalog
{
    public Ac15DonChallengeCatalog()
    {
    }

    public Ac15DonChallengeCatalog(
        bool enabled,
        string? activeBundleId,
        IReadOnlyList<Ac15DonChallengeMonthlyBundle> monthlyBundles)
    {
        Enabled = enabled;
        ActiveBundleId = activeBundleId;
        MonthlyBundles = monthlyBundles;
    }

    public static Ac15DonChallengeCatalog Disabled { get; } = new(false, null, []);

    public bool Enabled { get; init; }

    public string? ActiveBundleId { get; init; }

    public IReadOnlyList<Ac15DonChallengeMonthlyBundle> MonthlyBundles { get; init; } = [];

    public Ac15DonChallengeMonthlyBundle? ActiveBundle
        => Enabled && ActiveBundleId is { } id
            ? MonthlyBundles.FirstOrDefault(bundle => string.Equals(bundle.BundleId, id, StringComparison.OrdinalIgnoreCase))
            : null;

    public IReadOnlyList<Ac15DonChallengeMonthlyBundle> GetActiveBundles()
        => ActiveBundle is { } activeBundle ? [activeBundle] : [];
}

public sealed record Ac15DonChallengeMonthlyBundle(
    string BundleId,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    IReadOnlyList<Ac15DonChallengeTask> PersonalTasks,
    Ac15DonChallengeCommunityTask? CommunityTask,
    IReadOnlyList<Ac15DonChallengeReward> Rewards)
{
    public const int ExpectedPersonalTaskCount = 10;

    public bool HasExpectedPersonalTaskCount => PersonalTasks.Count == ExpectedPersonalTaskCount;

    public bool HasConfiguredWindow => StartsAt is not null || EndsAt is not null;
}

public sealed record Ac15DonChallengeTask(
    uint TaskId,
    uint Slot,
    string Name,
    Ac15DonChallengeRule Rule)
{
    public uint CompeId => TaskId;

    public uint TrackNo => Slot;
}

public sealed record Ac15DonChallengeCommunityTask(
    uint TaskId,
    string Name,
    Ac15DonChallengeRule Rule,
    string? Description);

public sealed record Ac15DonChallengeRule(
    Ac15DonChallengeRuleKind Kind,
    uint? MinimumScore = null,
    uint? RequiredSongCount = null,
    uint? RequiredCommunityCount = null,
    uint? MinimumLevel = null,
    IReadOnlyList<uint>? EligibleSongNoes = null)
{
    public static Ac15DonChallengeRule Unsupported { get; } = new(
        Ac15DonChallengeRuleKind.Unsupported);

    public IReadOnlyList<uint> EligibleSongNoes { get; init; } = EligibleSongNoes ?? [];

    public bool CanExecute => Kind switch
    {
        Ac15DonChallengeRuleKind.Clear => RequiredSongCount is null or > 0,
        Ac15DonChallengeRuleKind.FullCombo => RequiredSongCount is null or > 0,
        Ac15DonChallengeRuleKind.ScoreThreshold => MinimumScore > 0,
        Ac15DonChallengeRuleKind.CommunityCount => RequiredCommunityCount > 0,
        _ => false
    };

    public uint RequiredStageCount => RequiredSongCount.GetValueOrDefault(1);

    public bool RequiresDistinctSongProgress
        => (Kind is Ac15DonChallengeRuleKind.Clear
               or Ac15DonChallengeRuleKind.FullCombo)
           && RequiredStageCount > 1;

    public bool AllowsStage(Ac15StageResult stage)
        => (MinimumLevel is null || stage.Level >= MinimumLevel)
           && (EligibleSongNoes.Count == 0 || EligibleSongNoes.Contains(stage.SongNo));
}

public enum Ac15DonChallengeRuleKind
{
    Unsupported = 0,
    Clear,
    FullCombo,
    ScoreThreshold,
    CommunityCount
}

public sealed record Ac15DonChallengeReward(
    uint RequiredCompletedTasks,
    IReadOnlyList<uint> RewardSongNoes,
    IReadOnlyList<uint> RewardTitleIds);
