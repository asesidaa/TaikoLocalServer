using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class MusicInfoEntry
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;
	
	[JsonPropertyName("uniqueId")]
	public uint MusicId { get; set; }

	[JsonPropertyName("genreNo")]
	public SongGenre Genre { get; set; }

	[JsonPropertyName("starEasy")]
	public int StarEasy { get; set; }

	[JsonPropertyName("starNormal")]
	public int StarNormal { get; set; }

	[JsonPropertyName("starHard")]
	public int StarHard { get; set; }

	[JsonPropertyName("starMania")]
	public int StarOni { get; set; }

	[JsonPropertyName("starUra")]
	public int StarUra { get; set; }
}