using SharedProject.Enums;
using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class ChallengeCompeteCreateInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("competeMode")]
    public CompeteModeType CompeteMode { get; set; }

    [JsonPropertyName("maxParticipant")]
    public uint MaxParticipant { get; set; }

    [JsonPropertyName("onlyPlayOnce")]
    public bool OnlyPlayOnce { get; set; }

    [JsonPropertyName("lastFor")]
    public uint LastFor { get; set; }

    [JsonPropertyName("requiredTitle")]
    public uint RequiredTitle { get; set; } = 0;

    [JsonPropertyName("shareType")]
    public ShareType ShareType { get; set; } = ShareType.EveryOne;

    [JsonPropertyName("competeTargetType")]
    public CompeteTargetType CompeteTargetType { get; set; } = CompeteTargetType.EveryOne;

    [JsonPropertyName("challengeCompeteSongs")]
    public List<ChallengeCompeteCreateSongInfo> challengeCompeteSongs { get; set; } = new();
}
