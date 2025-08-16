using Riok.Mapperly.Abstractions;
using SharedProject.Models;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class DanDataMappers
{
    public static partial GetDanOdaiResponse.OdaiData ToWW08OdaiData(DanData data);
    
    public static partial Models.CN00.GetDanOdaiResponse.OdaiData ToCN00OdaiData(DanData data);
}