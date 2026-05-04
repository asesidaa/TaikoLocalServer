using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class Neiros
{
	[JsonPropertyName("items")]
	public List<NeiroEntry> NeiroEntries { get; set; } = new();
}