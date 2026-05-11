using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class TaikojukuMappers
{
    public static TaikojukuResponse Map(CommonTaikojukuResponse common)
    {
        return new TaikojukuResponse { Result = common.Result };
    }
}
