using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class SongIntroductionData
{
    [JsonPropertyName("setId")] public uint SetId { get; set; }

    [JsonPropertyName("verupNo")] public uint VerupNo { get; set; }

    [JsonPropertyName("mainSongNo")] public uint MainSongNo { get; set; }

    [JsonPropertyName("subSongNo")] public uint[]? SubSongNo { get; set; }
}