using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class QRCodeInfoEntry
{
    [JsonPropertyName("uniqueId")]
    public uint UniqueId { get; set; } = 0;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("modeId")]
    public uint ModeId { get; set; } = 0;

    [JsonPropertyName("daniGaidenOdaiId")]
    public uint DaniGaidenOdaiId { get; set; } = 0;

}