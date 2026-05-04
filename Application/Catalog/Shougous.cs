using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class Shougous
{
	[JsonPropertyName("items")]
	public List<ShougouEntry> ShougouEntries { get; set; } = new();
}