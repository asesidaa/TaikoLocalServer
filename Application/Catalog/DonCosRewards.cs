using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class DonCosRewards
{
	[JsonPropertyName("items")]
	public List<DonCosRewardEntry> DonCosRewardEntries { get; set; } = new();
}