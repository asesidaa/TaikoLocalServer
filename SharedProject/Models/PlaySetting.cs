using SharedProject.Enums;
using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class PlaySetting
{
    [JsonPropertyName("speed")]
    public uint Speed { get; set; }

    [JsonPropertyName("isVanishOn")]
    public bool IsVanishOn { get; set; }

    [JsonPropertyName("isInverseOn")]
    public bool IsInverseOn { get; set; }

    [JsonPropertyName("randomType")]
    public RandomType RandomType { get; set; }
}