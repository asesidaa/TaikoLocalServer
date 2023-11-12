using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class DonCosRewardEntry
{
	[JsonPropertyName("cosType")]
	public string CosType { get; set; } = null!;

	[JsonPropertyName("uniqueId")]
	public uint UniqueId { get; set; }
}