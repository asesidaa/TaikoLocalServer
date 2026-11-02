using TaikoLocalServer.Application.Dtos;

namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.LegacyWire;

public static class LegacyFolderDataMappers
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
