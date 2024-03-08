using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class DonCosRewards
{
    [JsonPropertyName("items")]
    public List<DonCosRewardEntry> DonCosRewardEntries { get; set; } = new();
}