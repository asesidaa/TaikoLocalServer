namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

public static class FolderDataMappers
{
    public static GetfolderResponse MapSingle(CommonGetFolderResponse common, uint folderId)
    {
        var folder = common.AryEventfolderDatas.FirstOrDefault(row => row.FolderId == folderId);
        return new GetfolderResponse
        {
            Result = common.Result,
            FolderId = folderId,
            SongNoes = folder?.SongNoes ?? []
        };
    }
}
