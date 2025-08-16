using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    public static partial UserDataResponse MapToWW08(CommonUserDataResponse response);
    
    public static partial Models.CN00.UserDataResponse MapToCN00(CommonUserDataResponse response);
}