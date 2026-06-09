using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    public static SelfBestResponse Map(CommonSelfBestResponse common)
    {
        var response = new SelfBestResponse
        {
            Result = common.Result,
            Level = common.Level
        };

        response.ArySelfbestScores.AddRange(common.ArySelfbestScores.Select(MapSelfBestData));

        response.AryShinSelfbestScores.AddRange(common.AryShinSelfbestScores.Select(MapSelfBestData));

        return response;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial SelfBestResponse.SelfBestData MapSelfBestData(CommonSelfBestResponse.SelfBestData row);
}
