using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    [MapProperty(nameof(CommonSelfBestResponse.ArySelfbestScores), nameof(SelfBestResponse.ArySelfbestScores))]
    [MapProperty(nameof(CommonSelfBestResponse.AryShinSelfbestScores), nameof(SelfBestResponse.AryShinSelfbestScores))]
    public static partial SelfBestResponse Map(CommonSelfBestResponse common);

    [MapperIgnoreSource(nameof(CommonSelfBestResponse.SelfBestData.SelfBestScoreRate))]
    [MapperIgnoreSource(nameof(CommonSelfBestResponse.SelfBestData.UraBestScoreRate))]
    private static partial SelfBestResponse.SelfBestData MapSelfBestData(CommonSelfBestResponse.SelfBestData row);
}
