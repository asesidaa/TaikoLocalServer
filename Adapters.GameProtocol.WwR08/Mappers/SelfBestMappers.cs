using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    public static partial SelfBestResponse MapToWW08(CommonSelfBestResponse response);
}
