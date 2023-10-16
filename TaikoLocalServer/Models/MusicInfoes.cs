using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class MusicInfoes
{
	[JsonPropertyName("items")]
	public List<MusicInfoEntry> MusicInfoEntries { get; set; } = new();
}