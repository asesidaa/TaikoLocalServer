using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Mappers;

[Mapper]
public static partial class GetTelopMappers
{
    [MapperIgnoreSource(nameof(CommonGetTelopResponse.VerupNo))]
    [MapProperty(nameof(CommonGetTelopResponse.StartDatetime), nameof(GetTelopResponse.StartDatetime), Use = nameof(MapPresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.EndDatetime), nameof(GetTelopResponse.EndDatetime), Use = nameof(MapPresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.Telop), nameof(GetTelopResponse.Telop), Use = nameof(MapPresentString))]
    public static partial GetTelopResponse Map(CommonGetTelopResponse common);

    private static string MapPresentString(string? value) => string.IsNullOrEmpty(value) ? null! : value;
}
