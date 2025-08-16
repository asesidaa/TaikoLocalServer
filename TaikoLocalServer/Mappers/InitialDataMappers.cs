using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    public static partial InitialdatacheckResponse MapToWW08(CommonInitialDataCheckResponse response);
    
    public static partial Models.CN00.InitialdatacheckResponse MapToCN00(CommonInitialDataCheckResponse response);
}