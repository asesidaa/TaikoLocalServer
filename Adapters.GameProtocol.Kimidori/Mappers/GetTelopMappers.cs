using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

[Mapper]
public static partial class GetTelopMappers
{
    [MapperIgnoreSource(nameof(CommonGetTelopResponse.VerupNo))]
    [MapProperty(nameof(CommonGetTelopResponse.StartDatetime), nameof(GettelopResponse.StartDatetime), Use = nameof(@Ac15MapperNormalization.PresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.EndDatetime), nameof(GettelopResponse.EndDatetime), Use = nameof(@Ac15MapperNormalization.PresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.Telop), nameof(GettelopResponse.Telop), Use = nameof(@Ac15MapperNormalization.PresentString))]
    public static partial GettelopResponse Map(CommonGetTelopResponse common);
}
