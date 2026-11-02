namespace TaikoLocalServer.Contracts.AdminApi.Responses;

public class DonChallengeAvailabilityResponse
{
    public string Era { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public string? ActiveBundleId { get; set; }

    public DateTimeOffset? StartsAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }

    public string? Message { get; set; }
}
