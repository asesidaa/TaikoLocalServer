using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class TokenCountDataMappers
{
    public static partial GetTokenCountResponse MapToWW08(CommonGetTokenCountResponse response);
    
    public static partial Models.CN00.GetTokenCountResponse MapToCN00(CommonGetTokenCountResponse response);
}