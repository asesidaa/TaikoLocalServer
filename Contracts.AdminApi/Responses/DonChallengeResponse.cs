namespace TaikoLocalServer.Contracts.AdminApi.Responses;

public class DonChallengeResponse
{
    public string Era { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public string? BundleId { get; set; }

    public DateTimeOffset? StartsAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }

    public uint CompletedTaskCount { get; set; }

    public uint PersonalTaskCount { get; set; }

    public List<DonChallengeTask> Tasks { get; set; } = new();

    public List<DonChallengeReward> Rewards { get; set; } = new();

    public string? Message { get; set; }
}
