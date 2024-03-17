using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class AddTokenCountRequestMapper
{
    public static partial CommonAddTokenCountRequest Map(AddTokenCountRequest request);

    public static partial CommonAddTokenCountRequest Map(Models.v3209.AddTokenCountRequest request);
}