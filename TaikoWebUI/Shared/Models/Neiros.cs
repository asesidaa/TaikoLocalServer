using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class Neiros
{
	[JsonPropertyName("items")]
	public List<NeiroEntry> NeiroEntries { get; set; } = new();
}