using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Mappers;

[Mapper]
public static partial class GetTelopMappers
{
    [MapperIgnoreSource(nameof(CommonGetTelopResponse.VerupNo))]
    [MapProperty(nameof(CommonGetTelopResponse.StartDatetime), nameof(GetTelopResponse.StartDatetime), Use = nameof(@Ac15MapperNormalization.PresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.EndDatetime), nameof(GetTelopResponse.EndDatetime), Use = nameof(@Ac15MapperNormalization.PresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.Telop), nameof(GetTelopResponse.Telop), Use = nameof(@Ac15MapperNormalization.PresentString))]
    public static partial GetTelopResponse Map(CommonGetTelopResponse common);
}
