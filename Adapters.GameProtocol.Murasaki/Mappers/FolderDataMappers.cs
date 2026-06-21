namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Mappers;

public static class FolderDataMappers
{
    public static GetfolderResponse MapSingle(CommonGetFolderResponse common, uint requestedFolderId)
    {
        var folder = common.AryEventfolderDatas.FirstOrDefault(data => data.FolderId == requestedFolderId);
        return new GetfolderResponse
        {
            Result = common.Result,
            FolderId = folder?.FolderId ?? requestedFolderId,
            SongNoes = folder?.SongNoes ?? []
        };
    }
}
