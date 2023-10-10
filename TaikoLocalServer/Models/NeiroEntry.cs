using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class NeiroEntry
{
	[JsonPropertyName("uniqueId")]
	public uint uniqueId { get; set; }
}