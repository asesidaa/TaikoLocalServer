using System.Text.Json.Serialization;

namespace LocalSaveModScoreMigrator;

public class MusicInfo
{
    [JsonPropertyName("items")] 
    public List<MusicInfoEntry> Items { get; set; } = new();
}