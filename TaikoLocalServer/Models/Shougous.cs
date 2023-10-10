using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class Shougous
{
	[JsonPropertyName("items")]
	public List<ShougouEntry> ShougouEntries { get; set; } = new();
}