using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class DanScoreMappers
{
    public static partial GetDanScoreResponse MapToCN00(CommonDanScoreDataResponse response);
}
