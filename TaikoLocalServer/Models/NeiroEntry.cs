using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class NeiroEntry
{
	[JsonPropertyName("uniqueId")]
	public uint UniqueId { get; set; }
}