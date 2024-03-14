using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class AiDataResponseMapper
{
    public static partial GetAiDataResponse MapTo3906(CommonAiDataResponse response);
    
    public static partial Models.v3209.GetAiDataResponse MapTo3209(CommonAiDataResponse response);
}