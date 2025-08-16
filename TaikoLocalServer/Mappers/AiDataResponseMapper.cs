using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class AiDataResponseMapper
{
    public static partial GetAiDataResponse MapToWW08(CommonAiDataResponse response);
    
    public static partial Models.CN00.GetAiDataResponse MapToCN00(CommonAiDataResponse response);
}