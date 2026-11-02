using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

[Mapper]
public static partial class FinalSelfBestMappers
{
    [MapProperty(nameof(CommonSelfBestResponse.ArySelfbestScores), nameof(FinalWire.SelfBestResponse.ArySelfbestScores))]
    [MapProperty(nameof(CommonSelfBestResponse.AryShinSelfbestScores), nameof(FinalWire.SelfBestResponse.AryShinSelfbestScores))]
    public static partial FinalWire.SelfBestResponse Map(CommonSelfBestResponse common);

    [MapperIgnoreSource(nameof(CommonSelfBestResponse.SelfBestData.SelfBestScoreRate))]
    [MapperIgnoreSource(nameof(CommonSelfBestResponse.SelfBestData.UraBestScoreRate))]
    private static partial FinalWire.SelfBestResponse.SelfBestData MapSelfBestData(CommonSelfBestResponse.SelfBestData row);
}
