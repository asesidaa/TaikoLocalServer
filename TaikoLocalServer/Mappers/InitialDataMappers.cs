using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    public static partial InitialdatacheckResponse MapTo3906(CommonInitialDataCheckResponse response);
    
    public static partial Models.v3209.InitialdatacheckResponse MapTo3209(CommonInitialDataCheckResponse response);
}