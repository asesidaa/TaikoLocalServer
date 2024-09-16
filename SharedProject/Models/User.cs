using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class User
{
    [JsonPropertyName("baid")]
    public uint Baid { get; set; }

    [JsonPropertyName("accessCodes")]
    public List<string> AccessCodes { get; set; } = new();

    [JsonPropertyName("isAdmin")]
    public bool IsAdmin { get; set; }

    [JsonPropertyName("userSetting")]
    public UserSetting UserSetting { get; set; } = new();
}