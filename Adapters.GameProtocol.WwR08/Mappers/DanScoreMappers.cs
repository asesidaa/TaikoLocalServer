using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class DanScoreMappers
{
    public static partial GetDanScoreResponse MapToWW08(CommonDanScoreDataResponse response);
}
