using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    public static partial UserDataResponse MapTo3906(CommonUserDataResponse response);
    
    public static partial Models.v3209.UserDataResponse MapTo3209(CommonUserDataResponse response);
}