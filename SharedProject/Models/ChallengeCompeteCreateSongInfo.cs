using SharedProject.Enums;
using System.Text.Json.Serialization;

namespace SharedProject.Models;
public class ChallengeCompeteCreateSongInfo
{
    [JsonPropertyName("songId")]
    public uint SongId { get; set; }

    [JsonPropertyName("difficulty")]
    public Difficulty Difficulty { get; set; }

    [JsonPropertyName("speed")]
    public int Speed { get; set; } = -1;

    [JsonPropertyName("isVanishOn")]
    public int IsVanishOn { get; set; } = -1;

    [JsonPropertyName("isInverseOn")]
    public int IsInverseOn { get; set; } = -1;

    [JsonPropertyName("randomType")]
    public int RandomType { get; set; } = -1;
}