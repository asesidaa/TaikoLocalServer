using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class AddTokenCountRequestMapper
{
    public static partial CommonAddTokenCountRequest Map(AddTokenCountRequest request);
}
