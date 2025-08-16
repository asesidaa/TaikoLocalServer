using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class DanScoreMappers
{
    public static partial GetDanScoreResponse MapToWW08(CommonDanScoreDataResponse response);
    public static partial Models.CN00.GetDanScoreResponse MapToCN00(CommonDanScoreDataResponse response);
}