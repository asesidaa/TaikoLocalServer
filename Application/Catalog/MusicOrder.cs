using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class MusicOrder
{
    [JsonPropertyName("items")]
    public List<MusicOrderEntry> Order { get; set; } = new();
}