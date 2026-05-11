using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class RecommendMappers
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
