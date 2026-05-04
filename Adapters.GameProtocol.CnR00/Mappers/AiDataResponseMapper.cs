using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class AiDataResponseMapper
{
    public static partial GetAiDataResponse MapToCN00(CommonAiDataResponse response);
}
