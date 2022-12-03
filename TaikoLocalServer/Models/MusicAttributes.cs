using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class MusicAttributes
{
    [JsonPropertyName("items")] public List<MusicAttributeEntry> MusicAttributeEntries { get; set; } = new();
}