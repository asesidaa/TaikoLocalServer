using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    public static partial SelfBestResponse MapTo3906(CommonSelfBestResponse response);

    public static partial Models.v3209.SelfBestResponse MapTo3209(CommonSelfBestResponse response);
}