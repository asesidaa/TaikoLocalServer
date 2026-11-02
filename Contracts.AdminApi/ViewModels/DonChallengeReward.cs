namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public static class DonChallengeRewardStatus
{
    public const string Earned = "Earned";
    public const string Locked = "Locked";
    public const string Unavailable = "Unavailable";
}

public class DonChallengeReward
{
    public uint RequiredCompletedTasks { get; set; }

    public List<uint> RewardSongNoes { get; set; } = new();

    public List<uint> RewardTitleIds { get; set; } = new();

    public string Status { get; set; } = DonChallengeRewardStatus.Unavailable;
}
