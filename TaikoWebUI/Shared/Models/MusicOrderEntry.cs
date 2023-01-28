using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class MusicOrderEntry
{
    [JsonPropertyName("uniqueId")] public uint SongId { get; set; }

    [JsonPropertyName("genreNo")] public uint GenreNo { get; set; }
}