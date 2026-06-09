using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class FolderDataMappers
{
    public static GetfolderResponse Map(CommonGetFolderResponse common)
    {
        var response = new GetfolderResponse { Result = common.Result };
        response.AryEventfolderDatas.AddRange(common.AryEventfolderDatas.Select(MapEventFolderData));

        return response;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial GetfolderResponse.EventfolderData MapEventFolderData(
        TaikoLocalServer.Application.ServerData.EventFolderData folder);
}
