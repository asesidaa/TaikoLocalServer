namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class DonChallengeTask
{
    public uint TaskId { get; set; }

    public uint Slot { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RuleLabel { get; set; } = string.Empty;

    public uint ProgressValue { get; set; }

    public uint? TargetValue { get; set; }

    public bool Completed { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public List<DonChallengeTrack> Tracks { get; set; } = new();
}
