namespace TaikoLocalServer.Application.Dtos;

public class CommonRecommendResponse
{
    public uint Result { get; set; } = 1;
    public uint RecommendSong { get; set; }
    public List<uint> RecommendBestSong { get; set; } = [];
}
