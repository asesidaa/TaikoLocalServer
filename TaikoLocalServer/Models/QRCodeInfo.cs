using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class QRCodeInfo
{
    [JsonPropertyName("items")]
    public List<QRCodeInfoEntry> QRCodeInfoEntries { get; set; } = new();
}