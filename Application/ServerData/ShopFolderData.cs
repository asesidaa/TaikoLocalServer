using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.ServerData;

public class ShopFolderData
{
    [JsonPropertyName("songNo")] public uint SongNo { get; set; }
    
    [JsonPropertyName("type")] public uint Type { get; set; }
    
    [JsonPropertyName("price")] public uint Price { get; set; }
}