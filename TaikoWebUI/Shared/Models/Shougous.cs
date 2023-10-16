using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class Shougous
{
	[JsonPropertyName("items")]
	public List<ShougouEntry> ShougouEntries { get; set; } = new();
}