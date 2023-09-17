using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class QRCodeData
{
    [JsonPropertyName("serial")] public string Serial { get; set; } = null!;

    [JsonPropertyName("id")] public uint Id { get; set; }
}