using GameDatabase.Entities;
using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class AiScoreMappers
{
    [MapProperty(nameof(AiScoreDatum.AiSectionScoreData), nameof(CommonAiScoreResponse.AryBestSectionDatas))]
    public static partial CommonAiScoreResponse MapToCommonAiScoreResponse(AiScoreDatum datum);

    public static CommonAiScoreResponse MapAsSuccess(AiScoreDatum datum)
    {
        var response= MapToCommonAiScoreResponse(datum);
        response.Result = 1;
        return response;
    }

    public static partial GetAiScoreResponse MapTo3906(CommonAiScoreResponse response);

    public static partial Models.v3209.GetAiScoreResponse MapTo3209(CommonAiScoreResponse response);
}