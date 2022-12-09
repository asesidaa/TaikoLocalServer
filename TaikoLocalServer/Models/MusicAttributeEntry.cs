using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class MusicAttributeEntry
{
    [JsonPropertyName("uniqueId")] public uint MusicId { get; set; }

    [JsonPropertyName("canPlayUra")] public bool HasUra { get; set; }
}