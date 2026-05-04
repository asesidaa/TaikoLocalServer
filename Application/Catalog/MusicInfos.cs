using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class MusicInfos
{
	[JsonPropertyName("items")]
	public List<MusicInfoEntry> MusicInfoEntries { get; set; } = new();
}