using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class ShougouEntry
{
	[JsonPropertyName("uniqueId")]
	public uint uniqueId { get; set; }
}