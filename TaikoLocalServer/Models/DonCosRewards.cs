using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class DonCosRewards
{
	[JsonPropertyName("items")]
	public List<DonCosRewardEntry> DonCosRewardEntries { get; set; } = new();
}