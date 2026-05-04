using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class DonCosRewardEntry
{
	[JsonPropertyName("cosType")]
	public string CosType { get; set; } = null!;

	[JsonPropertyName("uniqueId")]
	public uint UniqueId { get; set; }
}