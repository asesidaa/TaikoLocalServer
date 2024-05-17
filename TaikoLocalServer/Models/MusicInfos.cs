using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class MusicInfos
{
	[JsonPropertyName("items")]
	public List<MusicInfoEntry> MusicInfoEntries { get; set; } = new();
}