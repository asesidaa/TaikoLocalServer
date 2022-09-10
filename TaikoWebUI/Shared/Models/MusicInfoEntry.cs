using System.Text.Json.Serialization;
using SharedProject.Enums;

namespace TaikoWebUI.Shared.Models;

public class MusicInfoEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("uniqueId")]
    public uint SongId { get; set; }
    
    [JsonPropertyName("genreNo")]
    public SongGenre Genre { get; set; }
}