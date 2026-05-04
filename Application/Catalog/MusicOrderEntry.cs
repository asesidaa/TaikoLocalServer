using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class MusicOrderEntry
{
    [JsonPropertyName("uniqueId")]
    public uint SongId { get; set; }
}