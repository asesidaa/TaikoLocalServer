using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class TokenCountDataMappers
{
    public static partial GetTokenCountResponse MapToWW08(CommonGetTokenCountResponse response);
}
