using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class MyDonEntryMappers
{
    public static partial MydonEntryResponse MapToWW08(CommonMyDonEntryResponse response);
    
    public static partial Models.CN00.MydonEntryResponse MapToCN00(CommonMyDonEntryResponse response);
}