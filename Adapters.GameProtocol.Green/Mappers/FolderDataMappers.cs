using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class FolderDataMappers
{
    public static GetfolderResponse Map(CommonGetFolderResponse common)
    {
        return new GetfolderResponse { Result = common.Result };
    }
}
