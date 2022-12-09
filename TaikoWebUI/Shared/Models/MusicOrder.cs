using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class MusicOrder
{
    [JsonPropertyName("items")] public List<MusicOrderEntry> Order { get; set; } = new();
}