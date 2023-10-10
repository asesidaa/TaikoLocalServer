using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class ShougouEntry
{
	[JsonPropertyName("uniqueId")]
	public uint uniqueId { get; set; }
}