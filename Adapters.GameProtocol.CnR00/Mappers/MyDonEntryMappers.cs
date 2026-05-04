using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class MyDonEntryMappers
{
    public static partial MydonEntryResponse MapToCN00(CommonMyDonEntryResponse response);
}
