using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class DanData
{
    [JsonPropertyName("danId")] public uint DanId { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

    [JsonPropertyName("verupNo")] public uint VerupNo { get; set; }

    [JsonPropertyName("aryOdaiSong")] public List<OdaiSong> OdaiSongList { get; set; } = new();

    [JsonPropertyName("aryOdaiBorder")] public List<OdaiBorder> OdaiBorderList { get; set; } = new();

    public class OdaiSong
    {
        [JsonPropertyName("songNo")] public uint SongNo { get; set; }

        [JsonPropertyName("level")] public uint Level { get; set; }

        [JsonPropertyName("isHiddenSongName")] public bool IsHiddenSongName { get; set; }
    }

    public class OdaiBorder
    {
        [JsonPropertyName("odaiType")] public uint OdaiType { get; set; }

        [JsonPropertyName("borderType")] public uint BorderType { get; set; }

        [JsonPropertyName("redBorderTotal")] public uint RedBorderTotal { get; set; }

        [JsonPropertyName("goldBorderTotal")] public uint GoldBorderTotal { get; set; }

        [JsonPropertyName("redBorder_1")] public uint RedBorder1 { get; set; }

        [JsonPropertyName("redBorder_2")] public uint RedBorder2 { get; set; }

        [JsonPropertyName("redBorder_3")] public uint RedBorder3 { get; set; }

        [JsonPropertyName("goldBorder_1")] public uint GoldBorder1 { get; set; }

        [JsonPropertyName("goldBorder_2")] public uint GoldBorder2 { get; set; }

        [JsonPropertyName("goldBorder_3")] public uint GoldBorder3 { get; set; }
    }
}