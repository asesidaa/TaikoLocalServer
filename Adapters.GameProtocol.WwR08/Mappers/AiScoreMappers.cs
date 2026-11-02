using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class AiScoreMappers
{
    [MapProperty(nameof(AiScoreDatumNijiiro.AiSectionScoreData), nameof(CommonAiScoreResponse.AryBestSectionDatas))]
    public static partial CommonAiScoreResponse MapToCommonAiScoreResponse(AiScoreDatumNijiiro datum);

    public static CommonAiScoreResponse MapAsSuccess(AiScoreDatumNijiiro datum)
    {
        var response = MapToCommonAiScoreResponse(datum);
        response.Result = 1;
        return response;
    }

    public static partial GetAiScoreResponse MapToWW08(CommonAiScoreResponse response);
}
