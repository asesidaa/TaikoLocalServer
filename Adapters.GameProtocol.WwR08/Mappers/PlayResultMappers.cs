using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static partial CommonPlayResultData Map(PlayResultDataRequest request);
}
