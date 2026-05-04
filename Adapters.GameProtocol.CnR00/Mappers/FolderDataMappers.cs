using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class FolderDataMappers
{
    public static partial GetfolderResponse MapToCN00(CommonGetFolderResponse response);
}
