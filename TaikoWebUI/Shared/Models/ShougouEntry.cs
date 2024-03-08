using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class ShougouEntry
{
    [JsonPropertyName("uniqueId")]
    public uint UniqueId { get; set; }

    [JsonPropertyName("rarity")]
    public uint Rarity { get; set; }
}