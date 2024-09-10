using SharedProject.Models;
using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class ChallengeCompeteSongInfo
{
    [JsonPropertyName("songId")]
    public uint SongId { get; set; }

    [JsonPropertyName("difficulty")]
    public Difficulty Difficulty { get; set; }

    [JsonPropertyName("playSetting")]
    public PlaySetting PlaySetting { get; set; } = new();
}
