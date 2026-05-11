using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    public static SelfBestResponse Map(CommonSelfBestResponse common)
    {
        return new SelfBestResponse
        {
            Result = common.Result,
            Level = common.Level
        };
    }
}
