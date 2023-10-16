using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class DonCosRewardEntry
{
	[JsonPropertyName("cosType")]
	public string cosType { get; set; } = null!;

	[JsonPropertyName("uniqueId")]
	public uint uniqueId { get; set; }
}