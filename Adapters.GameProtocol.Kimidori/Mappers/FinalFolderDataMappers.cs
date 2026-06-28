using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

public static class FinalFolderDataMappers
{
    public static FinalWire.GetfolderResponse MapSingle(CommonGetFolderResponse common, uint folderId)
    {
        var folder = common.AryEventfolderDatas.FirstOrDefault(row => row.FolderId == folderId);
        return new FinalWire.GetfolderResponse
        {
            Result = common.Result,
            FolderId = folderId,
            SongNoes = folder?.SongNoes ?? []
        };
    }
}
