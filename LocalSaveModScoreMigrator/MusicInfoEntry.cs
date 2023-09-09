using System.Text.Json.Serialization;

namespace LocalSaveModScoreMigrator;

public class MusicInfoEntry
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("uniqueId")] public uint SongId { get; set; }
}