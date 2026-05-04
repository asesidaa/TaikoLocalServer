using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class AddTokenCountRequestMapper
{
    public static partial CommonAddTokenCountRequest Map(AddTokenCountRequest request);
}
