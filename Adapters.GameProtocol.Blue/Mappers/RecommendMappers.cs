using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class RecommendMappers
{
    [MapProperty(nameof(CommonRecommendResponse.RecommendBestSong), nameof(RecommendResponse.RecommendBestSongs))]
    public static partial RecommendResponse Map(CommonRecommendResponse common);
}
