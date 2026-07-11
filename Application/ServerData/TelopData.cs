using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.ServerData;

public class TelopData : IVerupNo
{
    [JsonPropertyName("telopId")]
    public uint TelopId { get; set; }

    [JsonPropertyName("verupNo")]
    public uint VerupNo { get; set; }

    [JsonPropertyName("startDatetime")]
    public string StartDatetime { get; set; } = string.Empty;

    [JsonPropertyName("endDatetime")]
    public string EndDatetime { get; set; } = string.Empty;

    [JsonPropertyName("telop")]
    public string Telop { get; set; } = string.Empty;
}