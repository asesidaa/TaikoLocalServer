using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    public static partial SelfBestResponse MapToWW08(CommonSelfBestResponse response);

    public static partial Models.CN00.SelfBestResponse MapToCN00(CommonSelfBestResponse response);
}