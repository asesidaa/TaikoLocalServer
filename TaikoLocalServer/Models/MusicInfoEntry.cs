using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class MusicInfoEntry
{
	[JsonPropertyName("uniqueId")]
	public uint MusicId { get; set; }

	[JsonPropertyName("starUra")]
	public uint starUra { get; set; }
}