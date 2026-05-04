using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    public static partial UserDataResponse MapToCN00(CommonUserDataResponse response);
}
