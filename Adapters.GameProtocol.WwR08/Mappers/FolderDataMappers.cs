using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class FolderDataMappers
{
    public static partial GetfolderResponse MapToWW08(CommonGetFolderResponse response);
}
