using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class MusicAttribute
{
    [JsonPropertyName("uniqueId")]
    public uint MusicId { get; set; }
    
    [JsonPropertyName("canPlayUra")]
    public bool HasUra { get; set; }
}