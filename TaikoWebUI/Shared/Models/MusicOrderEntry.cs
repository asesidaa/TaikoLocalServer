using System.Text.Json.Serialization;
using SharedProject.Enums;

namespace TaikoWebUI.Shared.Models;

public class MusicOrderEntry
{
    [JsonPropertyName("uniqueId")]
    public uint SongId { get; set; }
}