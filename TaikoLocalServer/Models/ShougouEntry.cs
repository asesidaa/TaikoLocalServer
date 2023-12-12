using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class ShougouEntry
{
	[JsonPropertyName("uniqueId")]
	public uint UniqueId { get; set; }
	
	[JsonPropertyName("rarity")]
	public uint Rarity { get; set; }
}