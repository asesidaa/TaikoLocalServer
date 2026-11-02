using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;
using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

[Mapper]
public static partial class FinalGetTelopMappers
{
    [MapperIgnoreSource(nameof(CommonGetTelopResponse.VerupNo))]
    [MapProperty(nameof(CommonGetTelopResponse.StartDatetime), nameof(FinalWire.GettelopResponse.StartDatetime), Use = nameof(@Ac15MapperNormalization.PresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.EndDatetime), nameof(FinalWire.GettelopResponse.EndDatetime), Use = nameof(@Ac15MapperNormalization.PresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.Telop), nameof(FinalWire.GettelopResponse.Telop), Use = nameof(@Ac15MapperNormalization.PresentString))]
    public static partial FinalWire.GettelopResponse Map(CommonGetTelopResponse common);
}
