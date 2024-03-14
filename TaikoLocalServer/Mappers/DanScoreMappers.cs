using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class DanScoreMappers
{
    public static partial GetDanScoreResponse MapTo3906(CommonDanScoreDataResponse response);
    public static partial Models.v3209.GetDanScoreResponse MapTo3209(CommonDanScoreDataResponse response);
}