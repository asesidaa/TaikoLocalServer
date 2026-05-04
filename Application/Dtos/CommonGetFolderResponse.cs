using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Application.Dtos;

public class CommonGetFolderResponse
{
    public uint Result { get; set; }

    public List<EventFolderData> AryEventfolderDatas { get; set; } = [];
}