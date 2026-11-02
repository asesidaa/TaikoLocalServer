using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    [MapProperty(nameof(CommonSelfBestResponse.ArySelfbestScores), nameof(SelfBestResponse.ArySelfbestScores))]
    [MapProperty(nameof(CommonSelfBestResponse.AryShinSelfbestScores), nameof(SelfBestResponse.AryShinSelfbestScores))]
    public static partial SelfBestResponse Map(CommonSelfBestResponse common);

    private static partial SelfBestResponse.SelfBestData MapSelfBestData(CommonSelfBestResponse.SelfBestData row);
}
