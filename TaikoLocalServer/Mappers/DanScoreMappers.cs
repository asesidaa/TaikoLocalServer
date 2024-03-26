using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class DanScoreMappers
{
    public static partial GetDanScoreResponse MapTo3906(CommonDanScoreDataResponse response);
    public static partial Models.v3209.GetDanScoreResponse MapTo3209(CommonDanScoreDataResponse response);
}