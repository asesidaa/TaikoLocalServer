using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class TokenCountDataMappers
{
    public static partial GetTokenCountResponse MapTo3906(CommonGetTokenCountResponse response);
    
    public static partial Models.v3209.GetTokenCountResponse MapTo3209(CommonGetTokenCountResponse response);
}