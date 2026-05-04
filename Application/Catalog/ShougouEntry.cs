using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class ShougouEntry
{
	[JsonPropertyName("uniqueId")]
	public uint UniqueId { get; set; }
	
	[JsonPropertyName("rarity")]
	public uint Rarity { get; set; }
}