namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Mappers;

public static class FolderDataMappers
{
    public static GetfolderResponse Map(CommonGetFolderResponse common)
    {
        var response = new GetfolderResponse { Result = common.Result };
        response.AryEventfolderDatas.AddRange(common.AryEventfolderDatas.Select(MapEventFolderData));
        return response;
    }

    private static GetfolderResponse.EventfolderData MapEventFolderData(EventFolderData folder)
        => new()
        {
            FolderId = folder.FolderId,
            SongNoes = folder.SongNoes ?? []
        };
}
