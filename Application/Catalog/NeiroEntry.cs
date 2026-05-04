using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class NeiroEntry
{
	[JsonPropertyName("uniqueId")]
	public uint UniqueId { get; set; }
}