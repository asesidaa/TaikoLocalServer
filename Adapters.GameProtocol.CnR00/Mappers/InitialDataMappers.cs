using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    public static partial InitialdatacheckResponse MapToCN00(CommonInitialDataCheckResponse response);
}
