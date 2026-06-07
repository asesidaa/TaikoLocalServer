namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class FolderDataMappers
{
    public static GetfolderResponse Map(CommonGetFolderResponse common)
    {
        var response = new GetfolderResponse { Result = common.Result };
        response.AryEventfolderDatas.AddRange(common.AryEventfolderDatas.Select(folder => new GetfolderResponse.EventfolderData
        {
            FolderId = folder.FolderId,
            VerupNo = folder.VerupNo,
            SongNoes = folder.SongNoes ?? []
        }));

        return response;
    }
}
