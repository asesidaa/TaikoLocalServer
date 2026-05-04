using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    public static partial InitialdatacheckResponse MapToWW08(CommonInitialDataCheckResponse response);
}
