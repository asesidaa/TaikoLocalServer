using Riok.Mapperly.Abstractions;
using SharedProject.Models;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class DanDataMappers
{
    public static partial GetDanOdaiResponse.OdaiData To3906OdaiData(DanData data);
    
    public static partial Models.v3209.GetDanOdaiResponse.OdaiData To3209OdaiData(DanData data);
}