namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class RecommendMappers
{
    public static RecommendResponse Map(CommonRecommendResponse common)
    {
        var response = new RecommendResponse
        {
            Result = common.Result,
            RecommendSong = common.RecommendSong
        };
        response.RecommendBestSongs = common.RecommendBestSong.ToArray();
        return response;
    }
}
