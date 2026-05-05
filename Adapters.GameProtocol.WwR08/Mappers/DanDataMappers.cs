using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class DanDataMappers
{
    public static partial GetDanOdaiResponse.OdaiData ToWW08OdaiData(DanData data);
}
