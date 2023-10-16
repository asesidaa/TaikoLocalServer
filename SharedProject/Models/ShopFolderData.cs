using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class ShopFolderData
{
    [JsonPropertyName("songNo")] public uint SongNo { get; set; }

    [JsonPropertyName("price")] public uint Price { get; set; }
}