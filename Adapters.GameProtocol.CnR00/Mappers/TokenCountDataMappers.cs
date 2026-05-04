using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class TokenCountDataMappers
{
    public static partial GetTokenCountResponse MapToCN00(CommonGetTokenCountResponse response);
}
