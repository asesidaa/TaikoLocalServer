using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class AiDataResponseMapper
{
    public static partial GetAiDataResponse MapToWW08(CommonAiDataResponse response);
}
