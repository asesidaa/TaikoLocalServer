using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;

[Mapper]
public static partial class FolderDataMappers
{
    [MapProperty(nameof(CommonGetFolderResponse.AryEventfolderDatas), nameof(GetfolderResponse.AryEventfolderDatas))]
    public static partial GetfolderResponse Map(CommonGetFolderResponse common);

    private static partial GetfolderResponse.EventfolderData MapEventFolderData(
        EventFolderData folder);
}
