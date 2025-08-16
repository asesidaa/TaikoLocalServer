using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static partial CommonPlayResultData Map(PlayResultDataRequest request);
    
    public static partial CommonPlayResultData Map(Models.CN00.PlayResultDataRequest request);
}