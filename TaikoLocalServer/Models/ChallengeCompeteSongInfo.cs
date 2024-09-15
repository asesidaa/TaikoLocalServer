using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class ChallengeCompeteSongInfo
{
    [JsonPropertyName("songId")]
    public uint SongId { get; set; }

    [JsonPropertyName("difficulty")]
    public Difficulty Difficulty { get; set; }

    [JsonPropertyName("speed")]
    public uint? Speed { get; set; } = null;

    [JsonPropertyName("isVanishOn")]
    public bool? IsVanishOn { get; set; } = null;

    [JsonPropertyName("isInverseOn")]
    public bool? IsInverseOn { get; set; } = null;

    [JsonPropertyName("randomType")]
    public RandomType? RandomType { get; set; } = null;
}
