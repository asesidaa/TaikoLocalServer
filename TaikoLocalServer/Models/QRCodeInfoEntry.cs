using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class QRCodeInfoEntry
{
    [JsonPropertyName("uniqueId")]
    public ulong UniqueId { get; set; } = 0;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("modeId")]
    public int ModeId { get; set; } = 0;

    [JsonPropertyName("daniGaidenOdaiId")]
    public int DaniGaidenOdaiId { get; set; } = 0;

}