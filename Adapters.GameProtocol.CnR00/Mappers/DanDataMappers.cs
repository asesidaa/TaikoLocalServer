using Riok.Mapperly.Abstractions;
using SharedProject.Models;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class DanDataMappers
{
    public static partial GetDanOdaiResponse.OdaiData ToCN00OdaiData(DanData data);
}
