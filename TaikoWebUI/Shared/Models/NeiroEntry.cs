using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class NeiroEntry
{
	[JsonPropertyName("uniqueId")]
	public uint uniqueId { get; set; }
}