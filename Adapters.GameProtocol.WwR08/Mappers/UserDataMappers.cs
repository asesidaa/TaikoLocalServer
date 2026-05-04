using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    public static partial UserDataResponse MapToWW08(CommonUserDataResponse response);
}
