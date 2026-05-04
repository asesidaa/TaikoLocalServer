using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class AiScoreMappers
{
    [MapProperty(nameof(AiScoreDatum.AiSectionScoreData), nameof(CommonAiScoreResponse.AryBestSectionDatas))]
    public static partial CommonAiScoreResponse MapToCommonAiScoreResponse(AiScoreDatum datum);

    public static CommonAiScoreResponse MapAsSuccess(AiScoreDatum datum)
    {
        var response = MapToCommonAiScoreResponse(datum);
        response.Result = 1;
        return response;
    }

    public static partial GetAiScoreResponse MapToWW08(CommonAiScoreResponse response);
}
