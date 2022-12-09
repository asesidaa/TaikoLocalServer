using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class MusicInfo
{
    [JsonPropertyName("items")] public List<MusicInfoEntry> Items { get; set; } = new();
}