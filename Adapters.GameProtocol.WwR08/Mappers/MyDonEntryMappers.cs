using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class MyDonEntryMappers
{
    public static partial MydonEntryResponse MapToWW08(CommonMyDonEntryResponse response);
}
