using System.Text.Json.Serialization;

namespace TaikoLocalServer.Models;

public class ChallengeCompeteInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("competeMode")]
    public CompeteModeType CompeteMode { get; set; }

    [JsonPropertyName("maxParticipant")]
    public uint MaxParticipant { get; set; }

    [JsonPropertyName("lastFor")]
    public uint LastFor { get; set; }

    [JsonPropertyName("requiredTitle")]
    public uint RequiredTitle { get; set; }

    [JsonPropertyName("shareType")]
    public ShareType ShareType { get; set; }

    [JsonPropertyName("competeTargetType")]
    public CompeteTargetType CompeteTargetType { get; set; }

    [JsonPropertyName("competeTargetType")]
    public List<ChallengeCompeteSongInfo> challengeCompeteSongs { get; set; } = new();
}
