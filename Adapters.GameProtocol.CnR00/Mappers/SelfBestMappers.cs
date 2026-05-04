using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    public static partial SelfBestResponse MapToCN00(CommonSelfBestResponse response);
}
